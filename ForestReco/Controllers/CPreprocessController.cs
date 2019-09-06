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

		//this number can very based on current lastool version...better to pick bigger
		private const int HEADER_FILE_LINES = 30;

		//TODO: make settable
		//WARNING: too large can cause error with unlicensed version
		private const int PREPROCESS_TILE_SIZE = 250;

		private static string tmpFolder => CParameterSetter.TmpFolder;

		private static string forestFilePath => CParameterSetter.GetStringSettings(ESettings.forestFileFullName);

		private static string forestFileName => CUtils.GetFileName(forestFilePath);

		public static string currentTmpFolder
		{
			get
			{
				string path = tmpFolder + forestFileName + "_tmp\\";
				Directory.CreateDirectory(path);
				return path;
			}
		}

		private static string tmpTiledFilesFolder
		{
			get
			{
				string path = currentTmpFolder + "_tmpTiles\\";
				Directory.CreateDirectory(path);
				return path;
			}
		}

		/// <summary>
		/// Returns path to folder containing tiles of given size.
		/// If doesnt exist => create it
		/// </summary>
		private static string GetTiledFilesFolder(string pSourceFileName, int pTileSize)
		{
			string forestFileName = CUtils.GetFileName(CParameterSetter.GetStringSettings(ESettings.forestFileName));
			string path = currentTmpFolder + $"_tiles[{pTileSize}]_{pSourceFileName}\\";
			Directory.CreateDirectory(path);
			return path;
		}

		public static string[] GetHeaderLines(string pSourceFilePath, string pOutputFilePath)
		{
			if(!File.Exists(pSourceFilePath))
			{
				CDebug.Error($"file: {pSourceFilePath} not found");
				return null;
			}

			//string infoFileName = Path.GetFileNameWithoutExtension(pSourceFilePath) + "_i.txt";
			//string infoFilePath = currentTmpFolder + infoFileName;

			string info =
					"lasinfo " +
					pSourceFilePath +
					" -o " +
					pOutputFilePath;

			try
			{
				CCmdController.RunLasToolsCmd(info, pOutputFilePath);
			}
			catch(Exception e)
			{
				CDebug.Error($"Exception {e}");
			}

			return CProgramLoader.GetFileLines(pOutputFilePath, HEADER_FILE_LINES);
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

		public static int currentTileIndex = 0;
		public static int tilesCount = 0;

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

			//forest file is already processed
			if(!CParameterSetter.GetBoolSettings(ESettings.preprocess))
			{
				return forestFilePath;
			}

			/////// lastile //////////

			CDebug.Step(EProgramStep.Pre_Tile);

			//takes long time for large files but cant meassure process
			FileInfo[] tiledFiles = GetTiledFiles(forestFilePath, tmpTiledFilesFolder,
				PREPROCESS_TILE_SIZE, CProjectData.bufferSize);

			List<string> classifyFilePaths = new List<string>();
			tilesCount = tiledFiles.Length;
			for(int i = 0; i < tiledFiles.Length; i++)
			{
				currentTileIndex = i;
				CDebug.Progress(i, tiledFiles.Length, 1, ref start, getClassifiedFilePathStart, "GetClassifiedFilePath", true);

				FileInfo fi = tiledFiles[i];

				/////// lasnoise //////////

				CDebug.Step(EProgramStep.Pre_Noise);
				string noiseFilePath = LasNoise(fi.FullName);

				/////// lasground //////////

				CDebug.Step(EProgramStep.Pre_LasGround);
				string groundFilePath = LasGround(noiseFilePath);

				/////// lasheight //////////

				CDebug.Step(EProgramStep.Pre_LasHeight);
				string heightFilePath = LasHeight(groundFilePath);

				/////// lasclassify //////////

				CDebug.Step(EProgramStep.Pre_LasClassify);
				string classifyFilePath = LasClassify(heightFilePath);

				classifyFilePaths.Add(classifyFilePath);
			}

			/////// lasmerge //////////
			//LasMerge(classifyFilePaths);

			CDebug.Step(EProgramStep.Pre_LasReverseTile);

			/////// reverse lastile //////////
			LasReverseTiles(classifyFilePaths);

			/////// delete unnecessary tmp files //////////

			if(CParameterSetter.GetBoolSettings(ESettings.deleteTmp))
			{
				CDebug.Step(EProgramStep.Pre_DeleteTmp);

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
				new DirectoryInfo(tmpTiledFilesFolder).Delete(true);
			}
			return preprocessedFilePath;
		}

		/// <summary>
		/// Split preprocessed file into tiles and converto to txt
		/// </summary>
		internal static List<string> GetTxtFilesPaths(string pPreprocessedFilePath)
		{
			List<string> result = new List<string>();
			int tileSize = CParameterSetter.GetIntSettings(ESettings.tileSize);

			string outputFolderPath = GetTiledFilesFolder(
				CUtils.GetFileName(pPreprocessedFilePath), tileSize);

			FileInfo[] tiles;
			//lastile splits file into tiles even if tileSize is bigger
			//todo: WHY?
			//=> dont split to tiles
			if(CProjectData.mainHeader.Width <= tileSize &&
				CProjectData.mainHeader.Height <= tileSize)
			{
				tiles = new FileInfo[] { new FileInfo(pPreprocessedFilePath) };
			}
			else
			{
				tiles = GetTiledFiles(pPreprocessedFilePath,
				outputFolderPath, tileSize, CProjectData.bufferSize);
			}

			for(int i = 0; i < tiles.Length; i++)
			{
				string txtFile = Las2Txt(tiles[i].FullName, outputFolderPath);
				result.Add(txtFile);
			}
			return result;
		}

		private static bool IsTmpFile(FileInfo fi)
		{
			string name = fi.Name;
			return name.Contains(GROUND_EXT + LAZ) || name.Contains(HEIGHT_EXT + LAZ) ||
				name.Contains(GROUND_EXT + LAZ) || name.Contains(CLASSIFY_EXT + LAZ);
		}

		private static FileInfo[] GetTiledFiles(string pSourceFilePath, string pOutputFolderPath, int pTileSize, int pBufferSize)
		{
			DirectoryInfo tiledFilesFolderInfo = new DirectoryInfo(pOutputFolderPath);

			//we expect that only laz files in folder should be the expected output
			//if they are already there, skip the process
			FileInfo[] lazFilesInFolder = tiledFilesFolderInfo.GetFiles("*" + LAZ);
			if(lazFilesInFolder.Length > 0)
			{
				return lazFilesInFolder;
			}

			string tile =
					"lastile -i " +
					pSourceFilePath +
					$" -tile_size {pTileSize} -buffer {pBufferSize}" +
					" -reversible -odir " +
					pOutputFolderPath +
					" -olaz";

			try
			{
				//dont know name of the ouput file name -> choose any and catch exception
				CCmdController.RunLasToolsCmd(tile, tmpTiledFilesFolder + "X");
			}
			catch(Exception e)
			{
				if(tiledFilesFolderInfo.GetFiles("*" + LAZ).Length == 0)
				{
					throw e;
				}
			}

			lazFilesInFolder = tiledFilesFolderInfo.GetFiles("*" + LAZ);
			return lazFilesInFolder;
		}

		private static string LasNoise(string pForestFilePath)
		{
			string noiseFileName = CUtils.GetFileName(pForestFilePath) + "_n" + LAZ;
			string noiseFilePath = currentTmpFolder + noiseFileName;

			string ground =
					"lasnoise -i " +
					pForestFilePath +
					" -o " +
					noiseFilePath;
			CCmdController.RunLasToolsCmd(ground, noiseFilePath);

			return noiseFilePath;
		}

		private static string LasGround(string pNoiseFilePath)
		{
			string groundFileName = CUtils.GetFileName(pNoiseFilePath) + "_g" + LAZ;
			string groundFilePath = currentTmpFolder + groundFileName;

			string ground =
					"lasground_new -i " +
					pNoiseFilePath +
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

		private static void LasReverseTiles(List<string> pClassifyFilePaths)
		{
			//string pathsString = "";
			//foreach(string path in pClassifyFilePaths)
			//{
			//	pathsString += path + " ";
			//}

			string reverseTiles =
					"lastile -i " +
					currentTmpFolder + "*_c" + LAZ +
					" -reverse_tiling -o " + preprocessedFilePath;
			CCmdController.RunLasToolsCmd(reverseTiles, preprocessedFilePath);
		}

		/*private static void LasMerge(List<string> pClassifyFilePaths)
		{
			//string pathsString = "";
			//foreach(string path in pClassifyFilePaths)
			//{
			//	pathsString += path + " ";
			//}

			string merge =
					"lasmerge -i " +
					currentTmpFolder + "*_c" + LAZ +
					" -o " + preprocessedFilePath;
			CCmdController.RunLasToolsCmd(merge, preprocessedFilePath);
		}*/

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
				CDebug.WriteLine($"exception {e}");
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

		internal static string Las2Txt(string splitFilePath, string pOutputFolder)
		{
			string splitFileName = CUtils.GetFileName(splitFilePath);

			//use split file name to get unique file name
			string txtFileName = splitFileName + ".txt";
			string txtFilePath = pOutputFolder + txtFileName;

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
