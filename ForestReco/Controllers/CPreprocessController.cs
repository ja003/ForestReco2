using System;
using System.Collections.Generic;
using System.IO;

namespace ForestReco
{
	public static class CPreprocessController
	{
		private const string LAZ = ".laz";
		private const string GROUND_EXT = "_g";
		private const string HEIGHT_EXT = "_h";
		private const string CLASSIFY_EXT = "_c";
		private const string SPLIT_EXT = "_s";

		private static string tmpFolder => CParameterSetter.TmpFolder;

		private static string forestFilePath => CParameterSetter.GetStringSettings(ESettings.forestFilePath);

		private static string forestFileName => CUtils.GetFileName(forestFilePath);

		private static string currentTmpFolder
		{
			get
			{
				string path = tmpFolder + forestFileName + "_tmp\\";
				Directory.CreateDirectory(path);
				return path;
			}
		}

		private static string tiledFilesFolder
		{
			get
			{
				string path = currentTmpFolder + "_tiles\\";
				Directory.CreateDirectory(path);
				return path;
			}
		}

		public static string[] GetHeaderLines(string pForestFilePath)
		{
			if(!File.Exists(pForestFilePath))
			{
				CDebug.Error($"file: {pForestFilePath} not found");
				return null;
			}

			string infoFileName = Path.GetFileNameWithoutExtension(pForestFilePath) + "_i.txt";
			string infoFilePath = currentTmpFolder + infoFileName;

			string info =
					"lasinfo " +
					pForestFilePath +
					" -o " +
					infoFilePath;

			try
			{
				CCmdController.RunLasToolsCmd(info, infoFilePath);
			}
			catch(Exception e)
			{
				CDebug.Error($"Exception {e}");
			}

			return CProgramLoader.GetFileLines(infoFilePath, 20);
		}




		//private static string classifyFileName => forestFileName + "_c" + LAZ;

		private static string preprocessedFilePath => currentTmpFolder + forestFileName + "_preprocessed" + LAZ;

		internal static string ProcessForestFolder()
		{
			//string forestFolder = CParameterSetter.GetStringSettings(ESettings.forestFolderPath);
			DirectoryInfo forestFolder = new DirectoryInfo(CParameterSetter.GetStringSettings(ESettings.forestFolderPath));
			if(!forestFolder.Exists)
			{
				CDebug.Error("forestFolderPath not set");
				return "";
			}
			string outputFilePath = forestFolder.FullName + $"\\{forestFolder.Name}_merged.laz";
			//merge all LAS and LAX files in the folder into 1 file
			string merge =
					"lasmerge -i " +
					forestFolder.FullName + "\\*.las " +
					forestFolder.FullName + "\\*.laz " +
					" -o " + outputFilePath;
			CCmdController.RunLasToolsCmd(merge, outputFilePath);

			return outputFilePath;
		}

		/// <summary>
		/// Returns path to the classified forest file.
		/// If it is not created it runs:
		/// - lasground
		/// - lasheight
		/// - lasclassify
		/// commands
		/// </summary>
		internal static string GetClassifiedFilePath()
		{
			DateTime getClassifiedFilePathStart = DateTime.Now;
			DateTime start = DateTime.Now;

			if(File.Exists(preprocessedFilePath))
			{
				return preprocessedFilePath;
			}

			/////// lastile //////////
			FileInfo[] tiledFiles = GetTiledFiles(); //long tome for large files

			List<string> classifyFilePaths = new List<string>();
			for(int i = 0; i < tiledFiles.Length; i++)
			{
				CDebug.Progress(i, tiledFiles.Length, 1, ref start, getClassifiedFilePathStart, "GetClassifiedFilePath", true);

				FileInfo fi = tiledFiles[i];

				/////// lasground //////////

				string groundFilePath = LasGround(fi.FullName);

				/////// lasheight //////////

				string heightFilePath = LasHeight(groundFilePath);

				/////// lasclassify //////////

				string classifyFilePath = LasClassify(heightFilePath);

				classifyFilePaths.Add(classifyFilePath);
			}

			/////// lasmerge //////////

			LasMerge(classifyFilePaths);

			/////// delete unnecessary tmp files //////////

			DirectoryInfo di = new DirectoryInfo(currentTmpFolder);
			FileInfo[] fileInfos = di.GetFiles();
			for(int i = 0; i < fileInfos.Length; i++)
			{
				FileInfo fi = fileInfos[i];
				if(IsTmpFile(fi))
				{
					CDebug.WriteLine($"delete tmp file {fi.Name}");
					fi.Delete();
				}
			}
			//delete tiles folder
			new DirectoryInfo(tiledFilesFolder).Delete(true);

			return preprocessedFilePath;
		}

		private static bool IsTmpFile(FileInfo fi)
		{
			string name = fi.Name;
			return name.Contains(GROUND_EXT + LAZ) || name.Contains(HEIGHT_EXT + LAZ) ||
				name.Contains(GROUND_EXT + LAZ) || name.Contains(CLASSIFY_EXT + LAZ);
		}

		private static FileInfo[] GetTiledFiles()
		{
			string tiledFileName = forestFileName + "_t" + LAZ;
			DirectoryInfo tiledFilesFolderInfo = new DirectoryInfo(tiledFilesFolder);
			//string tiledFilesPath = tiledFilesFolder + tiledFileName;

			string tile =
					"lastile -i " +
					forestFilePath +
					" -tile_size 500 -odir " + //-buffer 10 ???, use -odir?
					tiledFilesFolder +
					" -olaz";

			try
			{
				//dont know name of the ouput file name -> choose any and catch exception
				CCmdController.RunLasToolsCmd(tile, tiledFilesFolder + "X");
			}
			catch(Exception e)
			{
				if(tiledFilesFolderInfo.GetFiles("*" + LAZ).Length == 0)
				{
					throw e;
				}
			}

			return tiledFilesFolderInfo.GetFiles("*" + LAZ);
		}

		private static string LasGround(string pForestFilePath)
		{
			string groundFileName = CUtils.GetFileName(pForestFilePath) + "_g" + LAZ;
			string groundFilePath = currentTmpFolder + groundFileName;

			string ground =
					"lasground_new -i " +
					pForestFilePath +
					" -o " +
					groundFilePath;
			CCmdController.RunLasToolsCmd(ground, groundFilePath);

			return groundFilePath;
		}

		private static string LasHeight(string pGroundFilePath)
		{
			string heightFileName = CUtils.GetFileName(pGroundFilePath) + "_h" + LAZ;
			string heightFilePath = currentTmpFolder + heightFileName;

			string height =
					"lasheight -i " +
					pGroundFilePath +
					" -o " +
					heightFilePath;
			CCmdController.RunLasToolsCmd(height, heightFilePath);

			return heightFilePath;
		}

		private static void LasMerge(List<string> pClassifyFilePaths)
		{
			string pathsString = "";
			foreach(string path in pClassifyFilePaths)
			{
				pathsString += path + " ";
			}

			string merge =
					"lasmerge -i " +
					currentTmpFolder + "*_c" + LAZ +
					" -o " + preprocessedFilePath;
			CCmdController.RunLasToolsCmd(merge, preprocessedFilePath);
		}

		private static string LasClassify(string pHeightFilePath)
		{
			//string classifyFileName = forestFileName + "_c" + LAZ;
			//string classifyFilePath = tmpFolder + classifyFileName;
			string classifyFileName = CUtils.GetFileName(pHeightFilePath) + "_c" + LAZ;
			string classifyFilePath = currentTmpFolder + classifyFileName;

			string classify =
				"lasclassify -i " +
				pHeightFilePath +
				" -o " +
				classifyFilePath;
			CCmdController.RunLasToolsCmd(classify, classifyFilePath);

			return classifyFilePath;
		}

		public static string LasClip(string classifyFilePath)
		{
			string shapefilePath = CParameterSetter.GetStringSettings(ESettings.shapeFilePath);
			string shapefileName = CUtils.GetFileName(shapefilePath);
			if(!File.Exists(shapefilePath))
			{
				throw new Exception($"shapefile does not exist. {shapefilePath}");
			}
			string clipFileName = $"{forestFileName}_clip[{shapefileName}]";

			string clipFilePath = currentTmpFolder + clipFileName + LAZ;
			if(File.Exists(clipFilePath))
				return clipFilePath; //already created

			string clip =
					"lasclip -i " +
					classifyFilePath +
					" -poly " + shapefilePath +
					" -o " +
					clipFilePath;
			CCmdController.RunLasToolsCmd(clip, clipFilePath);
			return clipFilePath;
		}

		public static string LasSplit(string classifyFilePath)
		{
			SSplitRange range = CParameterSetter.GetSplitRange();
			if(!range.IsValid())
			{
				throw new Exception($"range {range} is not valid");
			}

			string splitFileName = $"{forestFileName}_s[{range.MinX},{range.MinY}]-[{range.MaxX},{range.MaxY}]";

			string keepXY = $" -keep_xy {range.MinX} {range.MinY} {range.MaxX} {range.MaxY}";
			string splitFilePath = currentTmpFolder + splitFileName + LAZ;
			if(File.Exists(splitFilePath))
				return splitFilePath; //already created

			string split =
					"lassplit -i " +
					classifyFilePath +
					keepXY +
					" -o " +
					splitFilePath;

			//todo: when split file not created there is no error...(ie when invalid range is given)
			try
			{
				//todo: split fails on jen2_merged_preprocessed.laz
				CCmdController.RunLasToolsCmd(split, splitFilePath);
			}
			catch(Exception e)
			{
				//split command creates file with other name...
				//CDebug.WriteLine($"exception {e}");
			}

			//for some reason output split file gets appendix: "_0000000" => rename it
			#region rename
			//rename split file

			//todo: move to Utils
			string sourceFile = splitFileName + "_0000000" + LAZ;
			FileInfo fi = new FileInfo(currentTmpFolder + sourceFile);
			if(fi.Exists)
			{
				fi.MoveTo(currentTmpFolder + splitFileName + LAZ);
				Console.WriteLine("Split file Renamed.");
			}
			else
			{
				//todo: implement my own exception
				throw new Exception($"Split file not created. Perhaps there are no points at range {range}?");
			}
			#endregion

			return splitFilePath;
		}

		internal static string Las2Txt(string splitFilePath)
		{
			string splitFileName = CUtils.GetFileName(splitFilePath);

			//use split file name to get unique file name
			string txtFileName = splitFileName + ".txt";
			string txtFilePath = currentTmpFolder + txtFileName;

			string toTxt =
				"las2txt -i " +
				splitFilePath +
				" -o " +
				txtFilePath +
				" -parse xyzc -sep tab -header percent";
			CCmdController.RunLasToolsCmd(toTxt, txtFilePath);

			return txtFilePath;
		}
	}
}
