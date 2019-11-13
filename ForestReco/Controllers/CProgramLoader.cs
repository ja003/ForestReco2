using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace ForestReco
{
	public static class CProgramLoader
	{
		public static bool useDebugData = false;

		public static List<string> GetTiledPreprocessedFilePaths()
		{
			string preprocessedFilePath = GetPreprocessedFilePath();

			SetMainHeader(preprocessedFilePath);

			CDebug.Step(EProgramStep.Pre_LasToTxt);
			//split into tiles and convert to txt
			return CPreprocessController.GetTxtFilesPaths(preprocessedFilePath);

		}

		/// <summary>
		/// Get info from preprocessed file using lasinfo and set it to main header
		/// </summary>
		private static void SetMainHeader(string pPreprocessedFilePath)
		{
			//FileInfo fi = new FileInfo(pPreprocessedFilePath);
			string infoFilePath = CPreprocessController.currentTmpFolder + "\\" + CUtils.GetFileName(pPreprocessedFilePath) + "_i.txt";

			string[] headerLines = CPreprocessController.GetHeaderLines(pPreprocessedFilePath, infoFilePath);

			if(headerLines == null)
			{
				CDebug.Error("header lines are null");
				//todo: is it ok to leave it as null??
				//CProjectData.mainHeader = new CHeaderInfo();
			}
			else
			{
				CProjectData.mainHeader = new CHeaderInfo(headerLines);
			}

			//can be inited only after main header is set
			CBitmapExporter.Init();
		}

		public static string[] GetFileLines(string pPreprocessedFilePath)
		{
			CDebug.Step(EProgramStep.LoadLines);

			string[] lines = File.ReadAllLines(pPreprocessedFilePath);
			CDebug.Action("load", pPreprocessedFilePath);

			return lines;
		}

		private static string GetPreprocessedFilePath()
		{
			DateTime getPreprocessedFilePathStart = DateTime.Now;
			DateTime start = DateTime.Now;

			CDebug.Progress(1, 3, 1, ref start, getPreprocessedFilePathStart, "classifyFilePath", true);

			if(CRxpParser.IsRxp)
			{
				string convertedFilePath = CPreprocessController.ConvertRxpToLas();
				//set as processed file
				CParameterSetter.SetParameter(ESettings.forestFileFullName, convertedFilePath);
			}

			string classifyFilePath = CPreprocessController.GetClassifiedFilePath();

			/////// lassplit //////////

			CDebug.Step(EProgramStep.Pre_Split);

			CDebug.Progress(2, 3, 1, ref start, getPreprocessedFilePathStart, "splitFilePath", true);

			//split mode = NONE => split file is same as classified file
			string splitFilePath = classifyFilePath;
			switch((ESplitMode)CParameterSetter.GetIntSettings(ESettings.currentSplitMode))
			{
				case ESplitMode.Manual:
					splitFilePath = CPreprocessController.LasSplit(classifyFilePath);
					break;
				case ESplitMode.Shapefile:
					splitFilePath = CPreprocessController.LasClip(classifyFilePath);
					break;
			}

			return splitFilePath;
		}

		public static string[] GetFileLines(string pFile, int pLines)
		{
			//string fullFilePath = CParameterSetter.GetStringSettings(ESettings.forestFilePath);
			//if (!File.Exists(fullFilePath)) { return null; }
			if(!File.Exists(pFile))
			{ return null; }

			string[] lines = new string[pLines];

			int count = 0;
			using(StreamReader sr = File.OpenText(pFile))
			{
				string s = "";
				while((s = sr.ReadLine()) != null && count < pLines)
				{
					lines[count] = s;
					count++;
				}
			}

			return lines;
		}

		/// <summary>
		/// Reads parsed lines and loads class and point list.
		/// Result is sorted in descending order.
		/// </summary>
		public static List<Tuple<EClass, Vector3>> ParseLines(string[] lines, bool pUseHeader)
		{
			CDebug.Step(EProgramStep.ParseLines);

			//store coordinates to corresponding data strucures based on their class
			const int DEFAULT_START_LINE = 19;
			int startLine = pUseHeader && CProjectData.mainHeader != null ? DEFAULT_START_LINE : 0;

			CDebug.Warning("loading " + lines.Length + " lines!");

			int linesToRead = lines.Length;

			//todo: check that "classic" processed files work correctly without using default class
			//bool classesCorect = true;
			List<Tuple<EClass, Vector3>> parsedLines = new List<Tuple<EClass, Vector3>>();
			if(useDebugData)
			{
				parsedLines = CDebugData.GetStandartTree();
				//CDebugData.DefineArray(true, 0);
			}
			else
			{
				for(int i = startLine; i < linesToRead; i++)
				{
					// <class, coordinate>
					Tuple<EClass, Vector3> c = CLazTxtParser.ParseLine(lines[i], pUseHeader);
					if(c == null)
						continue;

					//some files have different class counting. we are interested only in classes in EClass
					//if(c.Item1 == EClass.Other)
					//{
					//	c = new Tuple<EClass, Vector3>(EClass.Vege, c.Item2);
					//	classesCorect = false;
					//}
					parsedLines.Add(c);
				}
			}

			//if(!classesCorect)
			//{
			//	CDebug.WriteLine("classes not correct. using default class");
			//}
			CDebug.Count("parsedLines", parsedLines.Count);

			return parsedLines;
		}

		public static void ProcessParsedLines(List<Tuple<EClass, Vector3>> parsedLines)
		{
			CAnalytics.loadedPoints = parsedLines.Count;
			CProjectData.Points.AddPointsFromLines(parsedLines);

			CObjPartition.AddGroundArrayObj();

			if(CParameterSetter.GetBoolSettings(ESettings.exportPoints))
			{
				CObjPartition.AddPoints(EClass.Unassigned);
				CObjPartition.AddPoints(EClass.Ground);
				CObjPartition.AddPoints(EClass.Vege);
				CObjPartition.AddPoints(EClass.Building);
			}

			CDebug.Count("Trees", CTreeManager.Trees.Count);

			CTreeManager.CheckAllTrees();

			CDebug.Step(EProgramStep.ValidateTrees1);
			//dont move invalid trees to invalid list yet, some invalid trees will be merged
			CTreeManager.ValidateTrees(false, false);

			//export before merge
			if(CProjectData.exportBeforeMerge)
			{
				CTreeManager.AssignMaterials(); //call before export

				CObjPartition.AddTrees(true);
				CObjPartition.AddTrees(false);
				CObjPartition.ExportPartition("_noMerge");
				CObjPartition.Init();
				//CObjPartition.AddArray();
			}

			CAnalytics.firstDetectedTrees = CTreeManager.Trees.Count;

			CDebug.Step(EProgramStep.MergeNotTrees1);
			CTreeManager.TryMergeNotTrees();


			CDebug.Step(EProgramStep.MergeTrees1);
			//try merge all (even valid)
			EDetectionMethod detectionMethod = CTreeManager.GetDetectMethod();
			if((detectionMethod == EDetectionMethod.AddFactor
				|| detectionMethod == EDetectionMethod.Detection2D
				)
				&& CProjectData.tryMergeTrees)
			{
				CTreeManager.TryMergeAllTrees(false);
			}
			CAnalytics.afterFirstMergedTrees = CTreeManager.Trees.Count;

			//validate restrictive
			bool cathegorize = false;
			if(detectionMethod == EDetectionMethod.Detection2D)
			{
				cathegorize = true;
			}

			CDebug.Step(EProgramStep.ValidateTrees2);
			CTreeManager.ValidateTrees(cathegorize, true);

			CDebug.Step(EProgramStep.MergeTrees2);
			if(detectionMethod == EDetectionMethod.AddFactor)
			{
				//merge only invalid
				if(CProjectData.tryMergeTrees2)
					CTreeManager.TryMergeAllTrees(true);
			}

			//try merging not-trees again after trees were merged
			CDebug.Step(EProgramStep.MergeNotTrees2);
			CTreeManager.TryMergeNotTrees();

			CDebug.Step(EProgramStep.ValidateTrees3);
			if(detectionMethod == EDetectionMethod.AddFactor)
			{
				//validate restrictive
				//cathegorize invalid trees
				CTreeManager.ValidateTrees(true, true, true);
			}

			//todo: just debug
			//CTreeManager.CheckAllTrees();

			CAnalytics.detectedTrees = CTreeManager.Trees.Count;
			CAnalytics.invalidTrees = CTreeManager.InvalidTrees.Count;
			CAnalytics.invalidTreesAtBorder = CTreeManager.GetInvalidTreesAtBorderCount();

			CAnalytics.inputAverageTreeHeight = CTreeManager.AVERAGE_TREE_HEIGHT;
			CAnalytics.averageTreeHeight = CTreeManager.GetAverageTreeHeight();
			CAnalytics.maxTreeHeight = CTreeManager.MaxTreeHeight;
			CAnalytics.minTreeHeight = CTreeManager.GetMinTreeHeight();

			CDebug.Count("Trees", CTreeManager.Trees.Count);
			CDebug.Count("InvalidTrees", CTreeManager.InvalidTrees.Count);
			//CProjectData.array.DebugDetectedTrees();

			CTreeManager.AssignMaterials();

			CDebug.Step(EProgramStep.AssignReftrees);
			CReftreeManager.AssignRefTrees();
			if(CParameterSetter.GetBoolSettings(ESettings.exportRefTrees)) //no reason to export when no refTrees were assigned
			{
				//CRefTreeManager.ExportTrees();
				CObjPartition.AddRefTrees();
			}

			CObjPartition.AddTrees(true);
			if(CParameterSetter.GetBoolSettings(ESettings.exportInvalidTrees))
			{
				CObjPartition.AddTrees(false);
			}
		}
	}
}
