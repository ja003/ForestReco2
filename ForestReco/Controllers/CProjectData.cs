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

		//during one session is always processed one array file
		public static List<Vector3> groundPoints = new List<Vector3>();
		public static List<Vector3> vegePoints = new List<Vector3>();
		public static List<Vector3> filteredPoints = new List<Vector3>();

		public static CGroundArray groundArray;
		public static CVegeArray vegeArray;
		public static CVegeArray preprocessDetailArray;
		public static CVegeArray preprocessNormalArray;
		
		public static CTreeArray treeDetailArray;
		public static CTreeArray treeNormalArray;


		//public static CGroundArray detailArray;

		public static CHeaderInfo sourceFileHeader; //header of source file (not split)
		public static CHeaderInfo mainHeader; //header of file being processed (split applied)
		public static CHeaderInfo currentTileHeader; //header of currently processed tile file

		public static bool tryMergeTrees = true; //default true, user dont choose
		public static bool tryMergeTrees2 = true;//default true, user dont choose
		//public static bool exportBeforeMerge = false;

		public static bool useMaterial;

		public static float lowestHeight = int.MaxValue;
		public static float highestHeight = int.MinValue;
		public const int bufferSize = 10;

		private const float DETAIL_STEP = 0.1f;

		public static void Init()
		{
			saveFileName = CUtils.GetFileName(
				CParameterSetter.GetStringSettings(ESettings.forestFileFullName));
			string outputFolderSettings = CParameterSetter.GetStringSettings(ESettings.outputFolderPath);

			outputFolder = CObjExporter.CreateFolderIn(saveFileName, outputFolderSettings);


			//string[] lines = CPreprocessController.GetHeaderLines(fullFilePath);
			//sourceFileHeader = new CHeaderInfo(lines);

		}

		public static void ReInit(int pTileIndex)
		{
			//dont create subfolder if we export only one tile
			bool isOnlyTile = CProgramStarter.tilesCount == 1;
			outputTileSubfolder = isOnlyTile ? outputFolder :
				CObjExporter.CreateFolderIn($"tile_{pTileIndex}", outputFolder);

			groundArray = null;
			//header = null;
			currentTileHeader = null;

			vegePoints.Clear();
			groundPoints.Clear();
			filteredPoints.Clear();

			lowestHeight = int.MaxValue;
			highestHeight = int.MinValue;
		}

		public static void InitArrays()
		{
			groundArray = new CGroundArray(CParameterSetter.groundArrayStep, false);
			vegeArray = new CVegeArray(DETAIL_STEP, true);
			preprocessDetailArray = new CVegeArray(DETAIL_STEP, true);
			preprocessNormalArray = new CVegeArray(CParameterSetter.groundArrayStep, false);

			treeDetailArray = new CTreeArray(DETAIL_STEP, true);
			treeNormalArray = new CTreeArray(CParameterSetter.groundArrayStep, false);


			CObjPartition.Init();
		}

		public static Vector3 GetOffset()
		{
			//return mainHeader?.Offset ?? Vector3.Zero;
			return mainHeader.Offset;
			//return currentTileHeader.Offset;
		}

		public static Vector3 GetArrayCenter()
		{
			//return mainHeader?.Center ?? Vector3.Zero;
			return mainHeader.Center;
			//return currentTileHeader.Center;
		}

		public static float GetMinHeight()
		{
			//return mainHeader?.MinHeight ?? 0;
			return mainHeader.MinHeight;
		}

		public static float GetMaxHeight()
		{
			//return mainHeader?.MaxHeight ?? 1;
			return mainHeader.MaxHeight;
		}

		public static void AddPoint(Tuple<EClass, Vector3> pParsedLine)
		{
			//1 = unclassified
			//2 = ground
			//5 = high vegetation
			Vector3 point = pParsedLine.Item2;

			if(pParsedLine.Item1 == EClass.Ground)
			{
				groundPoints.Add(point);
			}
			else if(pParsedLine.Item1 == EClass.Vege)
			{
				vegePoints.Add(point);
			}

			float height = point.Z;
			if (height < lowestHeight)
			{
				lowestHeight = height;
			}
			if(height > highestHeight)
			{
				highestHeight = height;
			}
		}
	}
}
