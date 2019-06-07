﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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

		private static string mainOutput;
		private const string txt2lasCmd = "txt2las -parse xyzcuRGB -i";
		private const string GROUND_COLOR = "255 0 255"; //pink

		public static void Init()
		{
			mainOutput = "";
		}

		public static void ExportTile()
		{
			if(!export)
				return;

			string output = "";
			string res;

			//ground points
			foreach(Vector3 p in CProjectData.groundPoints)
			{
				Vector3 globalP = CUtils.GetGlobalPosition(p);
				res = GetPointLine(globalP, 2, (byte)0, GROUND_COLOR) + newLine;
				output += res;
				mainOutput += res;
			}

			//tree points
			foreach(CTree t in CTreeManager.Trees)
			{
				res = GetTreeLines(t); //already ends with "newLine"
				output += res;
				mainOutput += res;
			}

			//invalid tree points
			foreach(CTree t in CTreeManager.InvalidTrees)
			{
				res = GetTreeLines(t);
				output += res;
				//mainOutput += res; //dont add invalid trees to main file
			}

			WriteToFile(output, tileTxtFilePath);

			//format: x, y, z, class, user comment (id), R, G, B
			string cmd = $"{txt2lasCmd} {tileTxtFilePath}";
			CCmdController.RunLasToolsCmd(cmd, tileLasFilePath);
		}
		
		public static void ExportMain()
		{
			if(!export)
				return;

			WriteToFile(mainOutput, mainTxtFilePath);

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

				Vector3 globalP = CUtils.GetGlobalPosition(p);
				output += GetPointLine(globalP, 5, (byte)treeIndex, color) + newLine;
			}
			return output;
		}

		/// <summary>
		/// Txt line for 1 point.
		/// byte = unsigned char
		/// </summary>
		private static string GetPointLine(Vector3 pPoint, int pClass, byte pUserData, string pColor)
		{
			string output = $"{pPoint.X} {pPoint.Z} {pPoint.Y} ";//x y z (swap Y Z)
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
