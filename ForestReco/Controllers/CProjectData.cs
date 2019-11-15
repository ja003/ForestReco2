using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;

namespace ForestReco
{
	/// <summary>
	/// Data accessible from anywhere in project.
	/// </summary>
	public static class CProjectData
	{
		public static string saveFileName;
		public static string outputFolder; //string ends with \\
		public static string outputTileSubfolder; //string ends with \\

		public static BackgroundWorker backgroundWorker;

		//main data structure holding processed points and arrays
		public static CPointsHolder Points;

		public static CHeaderInfo sourceFileHeader; //header of source file (not split)
		public static CHeaderInfo mainHeader; //header of file being processed (split applied)
		public static CHeaderInfo currentTileHeader; //header of currently processed tile file

		public static bool tryMergeTrees = true; //default true, user dont choose
		public static bool tryMergeTrees2 = true;//default true, user dont choose
		public static bool exportBeforeMerge = true;

		public static bool useMaterial;

		//buffer size has to be smaller than tile size
		public static int bufferSize
		{
			get
			{
				int tileSize = CParameterSetter.GetIntSettings(ESettings.tileSize);
				if(tileSize > 10)
					return 10;
				return tileSize - 1;
			}
		}

		public static void Init()
		{
			saveFileName = CUtils.GetFileName(
				CParameterSetter.GetStringSettings(ESettings.forestFileFullName));
			string outputFolderSettings = CParameterSetter.GetStringSettings(ESettings.outputFolderPath);

			//include the method alias into the main folder name
			EDetectionMethod method = CTreeManager.GetDetectMethod();
			string suffix = CUtils.GetMethodSuffix(method);
			saveFileName += suffix;

			outputFolder = CObjExporter.CreateFolderIn(saveFileName, outputFolderSettings);

			Points = new CPointsHolder();
		}

		public static void ReInit(int pTileIndex)
		{
			//dont create subfolder if we export only one tile
			bool isOnlyTile = CProgramStarter.tilesCount == 1;
			string tileIndexString = GetTileIndexString(pTileIndex);

			string tileExtent = currentTileHeader.GetExtentString();

			outputTileSubfolder = isOnlyTile ? outputFolder :
				CObjExporter.CreateFolderIn($"tile_{tileIndexString}_{tileExtent}", outputFolder);

			Points.ReInit();
		}

		/// <summary>
		/// Calculates correct tile index string for the folder based on 
		/// total count of tiles.
		/// For better ordering of output folders
		/// </summary>
		private static string GetTileIndexString(int pTileIndex)
		{
			string format = "0";
			if(CProgramStarter.tilesCount > 10)
				format = "00";
			if(CProgramStarter.tilesCount > 100)
				format = "000";
			return pTileIndex.ToString(format);
		}

		public static Vector3 GetOffset()
		{
			return mainHeader.Offset;
		}

		public static Vector3 GetArrayCenter()
		{
			return mainHeader.Center;
		}

		public static float GetMinHeight()
		{
			return mainHeader.MinHeight;
		}

		public static float GetMaxHeight()
		{
			return mainHeader.MaxHeight;
		}

	}
}
