using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

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

		public static void Init()
		{
			exportedFiles = new List<FileInfo>();
		}

		/// <summary>
		/// Merges all exported files into one
		/// </summary>
		public static void MergeAll()
		{

			List<string[]> filesLines = new List<string[]>();

			foreach(FileInfo fi in exportedFiles)
			{
				filesLines.Add(File.ReadAllLines(fi.FullName));
			}

			using(StreamWriter writer = File.CreateText($"{CProjectData.outputFolder}\\dart_all.txt"))
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

		public static void Export()
		{
			string output = HEADER_LINE + newLine;

			foreach(CTree tree in CTreeManager.Trees)
			{
				string line = GetLine(tree);
				if(line != null)
					output += line + newLine;
			}

			CDebug.WriteLine(output);
			WriteToFile(output);
		}

		/// <summary>
		/// format: type(0)	pX	pY	pZ(0)	sX	sY	sZ rX(0) r(Y) r(Z)	
		/// </summary>
		private static string GetLine(CTree pTree)
		{
			string output = "0 ";
			ObjParser.Obj treeObj = pTree.mostSuitableRefTreeObj;
			if(treeObj == null)
				return null;

			//get coordinates relative to botleft corner of the area
			Vector3 treePos = GetMovedPoint(pTree.Center);
			//in final file Z = height, but here Y = height
			output += $"{treePos.X} {treePos.Z} 0 ";
			//scale will be same at all axix
			output += $"{treeObj.Scale.X} {treeObj.Scale.Z} {treeObj.Scale.Y} ";
			//rotation
			output += $"0 0 {treeObj.Rotation.Y} ";

			//to know which tree in OBJ is this one 
			bool debugObjName = true;
			string objName = debugObjName ? " " + pTree.GetObjName() : "";
			output += pTree.RefTreeTypeName + objName;

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