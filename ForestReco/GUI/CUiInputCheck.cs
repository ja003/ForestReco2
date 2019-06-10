using System.Collections.Generic;
using System.IO;

namespace ForestReco
{
	public static class CUiInputCheck
	{
		private static List<string> problems = new List<string>();

		private static void Reset()
		{
			problems = new List<string>();
		}

		private static bool CheckPath(string pTitle, string pPath, bool pFile) //false = folder
		{
			if(pFile)
			{
				bool fileExists = File.Exists(pPath);
				if(!fileExists)
				{
					problems.Add($"{pTitle} file not found: {pPath}");
					return false;
				}
			}
			else
			{
				bool folderExists = Directory.Exists(pPath);
				if(!folderExists)
				{
					problems.Add($"{pTitle} folder not found: {pPath}");
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// True = everything ok
		/// </summary>
		public static bool CheckProblems()
		{
			Reset();
			CheckPath("Forest", CParameterSetter.GetStringSettings(ESettings.forestFilePath), true);
			CheckPath("Reftree", CParameterSetter.GetStringSettings(ESettings.reftreeFolderPath), false);
			CheckPath("Output", CParameterSetter.GetStringSettings(ESettings.outputFolderPath), false);
			if(CParameterSetter.GetBoolSettings(ESettings.useCheckTreeFile))
			{
				CheckPath("Checktree", CParameterSetter.GetStringSettings(ESettings.checkTreeFilePath), true);
			}
			CheckRange();

			CheckExport();

			CheckDBH();
			CheckAGB();

			bool hasProblems = problems.Count > 0;
			if(hasProblems)
			{
				CDebug.WriteProblems(problems);
				problems.Clear();
			}
			return !hasProblems;
		}

		private static void CheckRange()
		{
			switch((ESplitMode)CParameterSetter.GetIntSettings(ESettings.currentSplitMode))
			{
				case ESplitMode.Manual:
					SSplitRange range = CParameterSetter.GetSplitRange();
					if(!range.IsValid())
					{
						problems.Add($"range {range} is not valid");
					}
					break;
				case ESplitMode.Shapefile:
					string shapefilePath = CParameterSetter.GetStringSettings(ESettings.shapeFilePath);
					if(!File.Exists(shapefilePath))
					{
						problems.Add($"shapefile not defined. {shapefilePath}");
					}
					break;
			}

		}

		private static void CheckExport()
		{
			bool willExportAny3D = CParameterSetter.GetBoolSettings(ESettings.export3d);

			if(willExportAny3D)
			{
				bool exportTreeStructures = CParameterSetter.GetBoolSettings(ESettings.exportTreeStructures);
				bool exportReftrees = CParameterSetter.GetBoolSettings(ESettings.exportRefTrees);
				bool exportTreeBoxes = CParameterSetter.GetBoolSettings(ESettings.exportTreeBoxes);
				willExportAny3D = exportTreeStructures || exportReftrees || exportTreeBoxes;
			}

			bool willExportSomeportBitmap = CParameterSetter.GetBoolSettings(ESettings.exportBitmap);

			if(willExportSomeportBitmap)
			{
				bool exportBMHeightmap = CParameterSetter.GetBoolSettings(ESettings.ExportBMHeightmap);
				bool exportBMTreeBorders = CParameterSetter.GetBoolSettings(ESettings.ExportBMTreeBorders);
				bool exportBMTreePositions = CParameterSetter.GetBoolSettings(ESettings.ExportBMTreePositions);
				willExportSomeportBitmap = exportBMHeightmap || exportBMTreeBorders || exportBMTreePositions;
			}

			bool willExportAnyShp = CParameterSetter.GetBoolSettings(ESettings.exportShape);
			if(willExportAnyShp)
			{
				bool exportShapeTreeAreas = CParameterSetter.GetBoolSettings(ESettings.exportShapeTreeAreas);
				bool exportShapeTreePositions = CParameterSetter.GetBoolSettings(ESettings.exportShapeTreePositions);
				willExportAnyShp = exportShapeTreeAreas || exportShapeTreePositions;
			}
			bool willExportAnyLas = CParameterSetter.GetBoolSettings(ESettings.exportLas);


			if(!willExportAny3D && !willExportSomeportBitmap && !willExportAnyShp && !willExportAnyLas)
			{
				problems.Add($"No reason to process when no 3D-obj, bitmap, SHP-file or LAS-file will be exported.");
				return;
			}
		}

		private static void CheckDBH()
		{
			string problem = CBiomassController.IsValidEquation(CParameterSetter.GetStringSettings(ESettings.dbh));
			if(problem.Length > 0)
				problems.Add($"DBH equation problem: {problem}");
		}

		private static void CheckAGB()
		{
			string problem = CBiomassController.IsValidEquation(CParameterSetter.GetStringSettings(ESettings.agb));
			if(problem.Length > 0)
				problems.Add($"DBH equation problem: {problem}");
		}
	}
}
