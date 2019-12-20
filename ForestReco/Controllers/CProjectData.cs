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

		/// <summary>
		/// Returns buffer size for tiling.
		/// Buffer size has to be smaller than tile size.
		/// In Rxp process the data is expected to be very dense and tiles will be small
		/// so we need to always return 1.
		/// pBig parameter is used when spliting large file during preprocess.
		/// The size has to be big enough otherwise splitted tiles wont be preprocessed
		/// correctly (lasheight doesnt compute values well etc) - todo: check
		/// - result: larger buffer size did not help, tiling had to be canceled
		/// completely in RXP preprocess. However these values can be used
		/// </summary>
		public static int GetBufferSize(bool pBig = false)
		{
			if(pBig)
				return 25;

			//rxp file is expected to be very dense
			if(CRxpParser.IsRxp)
				return 1;

			int tileSize = CParameterSetter.GetIntSettings(ESettings.tileSize);
			if(tileSize > 10)
				return 10;
			return tileSize - 1;
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

		public static bool IsOnlyTile => CProgramStarter.tilesCount == 1;

		public static void ReInit(int pTileIndex)
		{
			//dont create subfolder if we export only one tile
			
			string tileIndexString = GetTileIndexString(pTileIndex);

			string tileExtent = currentTileHeader.GetExtentString();

			outputTileSubfolder = IsOnlyTile ? outputFolder :
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
