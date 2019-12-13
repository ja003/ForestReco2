using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace ForestReco
{
	public static class CLasExporter
	{
		private static string newLine => Environment.NewLine;

		private static string tileFileName = "points";
		private static string tileFilePath => CProjectData.outputTileSubfolder + tileFileName;

		private static List<string> exportedTiles;

		private static string mainFileName = tileFileName + "_main";

		private static string mainFilePath => CProjectData.outputFolder + mainFileName;

		private static bool export => CParameterSetter.GetBoolSettings(ESettings.exportLas);

		private const string txt2lasCmd = "txt2las -parse xyzcuRGB -i";
		private const string BALLS_COLOR = "255 0 0";
		private const string BALLS_MP_COLOR = "0 255 0";
		private const string BALLS_CENTER_COLOR = "255 0 255";
		private const string BALLS_SURFACE_COLOR = "255 255 0";
		private const string ARRAY_GRID_COLOR = "255 20 147"; //deep pink
		private const string FILTERED_OUT_COLOR = "0 0 180"; //dark blue

		//deep pink: 255 20 147

		private const string UNASIGNED_COLOR = "190 190 145"; //pale yellow
		private const string GROUND_COLOR = "150 90 0"; //brown
		private const string BUILDING_COLOR = "250 150 150"; //pale red
		private const string UNDEFINED_COLOR = "0 0 0"; //black

		private const string NUM_FORMAT = "0.000";
		private const int DEBUG_FREQUENCY = 100000;

		public static void Init()
		{
			exportedTiles = new List<string>();
		}

		public static void ExportTile(string pDebug = "")
		{
			if(!export)
				return;
			DateTime exportStart = DateTime.Now;

			StringBuilder output = new StringBuilder();
			string res;

			DateTime start = DateTime.Now;
			DateTime lastDebug = DateTime.Now;

			const bool export_surface = false;
			const bool export_filteredOut = false;
			const bool export_ground = false;

			AddPointsTo(ref output, EClass.Unassigned, ref start);
			if(export_ground)
				AddPointsTo(ref output, EClass.Ground, ref start);
			AddPointsTo(ref output, EClass.Building, ref start);

			
			if(CTreeManager.GetDetectMethod() == EDetectionMethod.Balls)
			{
				if(export_surface)
					AddPointsTo(ref output, EClass.BallsSurface, ref start);
				AddPointsTo(ref output, EClass.Balls, ref start);
				if(export_filteredOut)
					AddPointsTo(ref output, EClass.FilteredOut, ref start);
				AddPointsTo(ref output, EClass.BallsMainPoints, ref start);
				AddPointsTo(ref output, EClass.BallsCenters, ref start);
				AddPointsTo(ref output, EClass.ArrayGrid, ref start);
			}

			//tree points
			AddTreePointsTo(ref output, true, ref start);
			//invalid tree points
			AddTreePointsTo(ref output, false, ref start);

			if(output.Length == 0)
			{
				CDebug.Warning($"CLasExporter: no output created on tile {CProgramStarter.currentTileIndex}");
				return;
			}

			//1. Create the text file 
			string fileFullPath = tileFilePath + pDebug;
			string txtOutputPath = fileFullPath + ".txt";
			CUtils.WriteToFile(output, txtOutputPath);
			//2. save output path for the main file export
			exportedTiles.Add(txtOutputPath);

			//3. Convert the txt to las using LasTools
			//format: x, y, z, class, user comment (id), R, G, B
			string cmd = $"{txt2lasCmd} {txtOutputPath}";
			CCmdController.RunLasToolsCmd(cmd, fileFullPath + ".las");

			CAnalytics.lasExportDuration = CAnalytics.GetDuration(exportStart);
		}

		public static void ExportMain()
		{
			if(!export)
				return;

			if(exportedTiles.Count == 0)
			{
				CDebug.Warning($"CLasExporter: no mainoutput created");
				return;
			}
			string mainTxtFilePath = mainFilePath + ".txt";

			foreach(string filePath in exportedTiles)
			{
				//copy content of all exported tile files to one
				StringBuilder fileContent = new StringBuilder(File.ReadAllText(filePath));
				CUtils.WriteToFile(fileContent, mainTxtFilePath);
			}

			//convert to las
			string cmd = $"{txt2lasCmd} {mainTxtFilePath}";
			CCmdController.RunLasToolsCmd(cmd, mainFilePath + ".las");

			//delete all txt files
			File.Delete(mainTxtFilePath);
			foreach(string filePath in exportedTiles)
			{
				File.Delete(filePath);
			}
		}

		/// <summary>
		/// Add points from given class to the output and main output
		/// </summary>
		private static void AddPointsTo(ref StringBuilder pOutput, EClass pClass, ref DateTime start)
		{
			List<Vector3> points = CProjectData.Points.GetPoints(pClass);
			string res;
			DateTime lastDebug = DateTime.Now;

			for(int i = 0; i < points.Count; i++)
			{
				if(CProjectData.backgroundWorker.CancellationPending) { return; }

				CDebug.Progress(i, points.Count, DEBUG_FREQUENCY, ref lastDebug, start, "Export las (ground points)");

				Vector3 p = points[i];
				CVector3D globalP = CUtils.GetGlobalPosition(p);
				res = GetPointLine(globalP, 1, 0, GetClassColor(pClass)) + newLine;
				pOutput.Append(res);
			}
			//mainOutput.Append(pOutput);
		}

		private static string GetClassColor(EClass pClass)
		{
			switch(pClass)
			{
				case EClass.Ground:
					return GROUND_COLOR;
				case EClass.Building:
					return BUILDING_COLOR;
				case EClass.Unassigned:
					return UNASIGNED_COLOR;
				case EClass.Balls:
					return BALLS_COLOR;
				case EClass.BallsMainPoints:
					return BALLS_MP_COLOR;
				case EClass.BallsCenters:
					return BALLS_CENTER_COLOR;
				case EClass.BallsSurface:
					return BALLS_SURFACE_COLOR;

				case EClass.FilteredOut:
					return FILTERED_OUT_COLOR;

				case EClass.ArrayGrid:
					return ARRAY_GRID_COLOR;
			}
			return UNDEFINED_COLOR;
		}

		/// <summary>
		/// Add all (valid/invalid) tree points to the output and main output
		/// </summary>
		private static void AddTreePointsTo(ref StringBuilder pOutput, bool pValid, ref DateTime start)
		{
			string res;
			DateTime lastDebug = DateTime.Now;

			List<CTree> trees = pValid ? CTreeManager.Trees : CTreeManager.InvalidTrees;
			for(int i = 0; i < trees.Count; i++)
			{
				if(CProjectData.backgroundWorker.CancellationPending) { return; }

				CDebug.Progress(i, trees.Count, DEBUG_FREQUENCY, ref lastDebug, start, "Export las (trees)");

				CTree t = trees[i];
				res = GetTreeLines(t); //already ends with "newLine"
				pOutput.Append(res);
			}
			//mainOutput.Append(pOutput);
		}

		/// <summary>
		/// </summary>
		private static string GetTreeLines(CTree pTree)
		{
			string output = "";
			foreach(Vector3 p in pTree.Points)
			{
				string color = pTree.isValid ?
					pTree.assignedMaterial.ToString255() :
					CMaterialManager.GetInvalidMaterial().ToString255();

				//int treeIndex = pTree.isValid ? pTree.assignedRefTree.treeIndex : 0;
				int treeIndex = pTree.treeIndex;

				CVector3D globalP = CUtils.GetGlobalPosition(p);
				output += GetPointLine(globalP, 5, (byte)treeIndex, color) + newLine;
			}
			return output;
		}

		/// <summary>
		/// Txt line for 1 point.
		/// byte = unsigned char
		/// </summary>
		private static string GetPointLine(CVector3D pPoint, int pClass, byte pUserData, string pColor)
		{
			//todo: make string method
			string output = $"{pPoint.X.ToString(NUM_FORMAT)} {pPoint.Y.ToString(NUM_FORMAT)} {pPoint.Z.ToString(NUM_FORMAT)} ";//x y z (swap Y Z)
			output += $"{pClass} {pUserData} "; //class, id 
			output += pColor; //R G B
			return output;
		}

	}
}
