using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace ForestReco
{
	/// <summary>
	/// https://redmine.czechglobe.cz/issues/233
	/// TXT file to be used to generate model in DART
	/// </summary>
	public static class CDartTxt
	{
		private static List<FileInfo> exportedFiles;

		private static string newLine => Environment.NewLine;

		private static string HEADER_LINE = "complete transformation	"; //TODO: is it neccessary?
		private const int DEBUG_FREQUENCY = 100;

		public static void Init()
		{
			exportedFiles = new List<FileInfo>();
		}

		/// <summary>
		/// Merges all exported files into one
		/// </summary>
		public static void ExportMain()
		{
			return;

			List<string[]> filesLines = new List<string[]>();

			foreach(FileInfo fi in exportedFiles)
			{
				filesLines.Add(File.ReadAllLines(fi.FullName));
			}

			if(filesLines.Count == 0)
			{
				CDebug.Warning($"CDartTxt: no main output created");
				return;
			}

			using(StreamWriter writer = File.CreateText($"{CProjectData.outputFolder}\\dart_main.txt"))
			{
				writer.WriteLine(HEADER_LINE);

				foreach(string[] fileLines in filesLines)
				{
					int lineNum = 1; //skip header
					while(lineNum < fileLines.Length)
					{
						writer.WriteLine(fileLines[lineNum]);
						lineNum++;
					}
				}
			}

		}

		public static void ExportTile()
		{
			return;

			if(CTreeManager.Trees.Count == 0)
			{
				CDebug.Warning($"CDartTxt: no output created on tile {CProgramStarter.currentTileIndex}");
				return;
			}
			DateTime start = DateTime.Now;
			DateTime lastDebug = DateTime.Now;

			StringBuilder output = new StringBuilder(HEADER_LINE + newLine);

			for(int i = 0; i < CTreeManager.Trees.Count; i++)
			{
				CDebug.Progress(i, CTreeManager.Trees.Count, DEBUG_FREQUENCY, ref lastDebug, start, "Export dart (trees)");
				CTree tree = CTreeManager.Trees[i];
				string line = GetLine(tree);
				if(line != null)
					output.Append(line + newLine);
			}

			//CDebug.WriteLine(output);
			WriteToFile(output.ToString());
		}

		/// <summary>
		/// format: type(0)	pX	pY	pZ(0)	sX	sY	sZ rX(0) r(Y) r(Z)	
		/// </summary>
		private static string GetLine(CTree pTree)
		{
			string output = "0 ";
			ObjParser.Obj treeObj = pTree.assignedRefTreeObj;
			if(treeObj == null)
				return null;

			//get coordinates relative to botleft corner of the area
			Vector3 treePos = GetMovedPoint(pTree.Center);
			//in final file Z = height, but here Y = height //changed!
			output += $"{treePos.X} {treePos.Y} 0 ";
			//scale will be same at all axix
			output += $"{treeObj.Scale.X} {treeObj.Scale.Y} {treeObj.Scale.Z} ";
			//rotation
			output += $"0 0 {treeObj.Rotation.Y} ";

			//to know which tree in OBJ is this one 
			bool debugObjName = true;
			string objName = debugObjName ? " " + pTree.GetObjName() : "";
			output += pTree.assignedRefTree.RefTreeTypeName + objName;

			return output;
		}

		/// <summary>
		/// Dart file starts at topleft corner -> transform
		/// </summary>
		private static Vector3 GetMovedPoint(Vector3 pPoint)
		{
			//todo fix
			pPoint.Z = CProjectData.mainHeader.TopRightCorner.Z - pPoint.Z;
			pPoint.X -= CProjectData.mainHeader.BotLeftCorner.X;

			//pPoint -= CProjectData.header.BotLeftCorner; //use this for botleft corner
			return pPoint;
		}

		/// <summary>
		/// todo: create file manager
		/// </summary>
		/// <param name="pText"></param>
		private static void WriteToFile(string pText)
		{
			string fileName = "dart.txt";
			string filePath = CProjectData.outputTileSubfolder + "/" + fileName;
			using(var outStream = File.OpenWrite(filePath))
			using(var writer = new StreamWriter(outStream))
			{
				writer.Write(pText);
			}

			exportedFiles.Add(new FileInfo(filePath));
		}
	}
}