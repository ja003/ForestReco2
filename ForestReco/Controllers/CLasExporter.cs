using System;
using System.IO;
using System.Numerics;
using System.Text;

namespace ForestReco
{
	public static class CLasExporter
	{
		private static string newLine => Environment.NewLine;

		private static string tileFileName = "points";
		private static string tileTxtFilePath => CProjectData.outputTileSubfolder + tileFileName + ".txt";
		private static string tileLasFilePath => CProjectData.outputTileSubfolder + tileFileName + ".las";

		private static string mainFileName = tileFileName + "_main";
		private static string mainTxtFilePath => CProjectData.outputFolder + mainFileName + ".txt";
		private static string mainLasFilePath => CProjectData.outputFolder + mainFileName + ".las";

		private static bool export => CParameterSetter.GetBoolSettings(ESettings.exportLas);

		private static StringBuilder mainOutput;
		private const string txt2lasCmd = "txt2las -parse xyzcuRGB -i";
		private const string GROUND_COLOR = "255 0 255"; //pink

		private const string NUM_FORMAT = "0.00";
		private const int DEBUG_FREQUENCY = 100;

		public static void Init()
		{
			mainOutput = new StringBuilder();
		}

		public static void ExportTile()
		{
			if(!export)
				return;
			DateTime exportStart = DateTime.Now;

			StringBuilder output = new StringBuilder();
			string res;

			DateTime start = DateTime.Now;
			DateTime lastDebug = DateTime.Now;

			//ground points
			for(int i = 0; i < CProjectData.groundPoints.Count; i++)
			{
				if(CProjectData.backgroundWorker.CancellationPending) { return; }

				CDebug.Progress(i, CProjectData.groundPoints.Count, DEBUG_FREQUENCY, ref lastDebug, start, "Export las (ground points)");

				Vector3 p = CProjectData.groundPoints[i];
				CVector3D globalP = CUtils.GetGlobalPosition(p);
				res = GetPointLine(globalP, 2, (byte)0, GROUND_COLOR) + newLine;
				output.Append(res);
			}
			mainOutput.Append(output);

			//tree points
			for(int i = 0; i < CTreeManager.Trees.Count; i++)
			{
				if(CProjectData.backgroundWorker.CancellationPending) { return; }

				CDebug.Progress(i, CTreeManager.Trees.Count, DEBUG_FREQUENCY, ref lastDebug, start, "Export las (trees)");

				CTree t = CTreeManager.Trees[i];
				res = GetTreeLines(t); //already ends with "newLine"
				output.Append(res);
			}
			mainOutput.Append(output);

			//invalid tree points
			for(int i = 0; i < CTreeManager.InvalidTrees.Count; i++)
			{
				if(CProjectData.backgroundWorker.CancellationPending) { return; }

				CDebug.Progress(i, CTreeManager.InvalidTrees.Count, DEBUG_FREQUENCY, ref lastDebug, start, "Export las (invalid trees)");

				CTree t = CTreeManager.InvalidTrees[i];
				res = GetTreeLines(t);
				output.Append(res);
				//mainOutput += res; //dont add invalid trees to main file
			}

			if(output.Length == 0)
			{
				CDebug.Warning($"CLasExporter: no output created on tile {CProgramStarter.currentTileIndex}");
				return;
			}

			WriteToFile(output.ToString(), tileTxtFilePath);

			//format: x, y, z, class, user comment (id), R, G, B
			string cmd = $"{txt2lasCmd} {tileTxtFilePath}";
			CCmdController.RunLasToolsCmd(cmd, tileLasFilePath);

			CAnalytics.lasExportDuration = CAnalytics.GetDuration(exportStart);
		}

		public static void ExportMain()
		{
			if(!export)
				return;

			if(mainOutput.Length == 0)
			{
				CDebug.Warning($"CLasExporter: no mainoutput created");
				return;
			}

			WriteToFile(mainOutput.ToString(), mainTxtFilePath);

			//format: x, y, z, class, user comment (id), R, G, B
			string cmd = $"{txt2lasCmd} {mainTxtFilePath}";
			CCmdController.RunLasToolsCmd(cmd, mainLasFilePath);
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


		/// <summary>
		/// todo: create file manager
		/// </summary>
		/// <param name="pText"></param>
		private static void WriteToFile(string pText, string pFilePath)
		{
			using(var outStream = File.OpenWrite(pFilePath))
			using(var writer = new StreamWriter(outStream))
			{
				writer.Write(pText);
			}
		}
	}
}
