using NCalc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
    public static class CBiomassController
    {
        private static List<FileInfo> exportedFiles;
        private static string newLine => Environment.NewLine;

        private const string PARAM_HEIGHT = "height";
        private const string PARAM_DBH = "dbh";
        private const string PARAM_E = "e";
		private const string DELIMETER = ", ";
		private static string DBH_EQUATION;
        private static string AGB_EQUATION;

		/// <summary>
		/// Save equations for later evaluation.
		/// Equations are expected to be checked and valid.
		/// </summary>
        public static void Init(string pDBH, string pAGB)
        {
            exportedFiles = new List<FileInfo>();
            DBH_EQUATION = pDBH;
            AGB_EQUATION = pAGB;
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

			using(StreamWriter writer = File.CreateText($"{CProjectData.outputFolder}\\biomass_all.txt"))
			{
				//writer.WriteLine(HEADER_LINE);

				foreach(string[] fileLines in filesLines)
				{
					int lineNum = 0;
					while(lineNum < fileLines.Length)
					{
						writer.WriteLine(fileLines[lineNum]);
						lineNum++;
					}
				}
			}
		}

		/// <summary>
		/// Export file using data from currently loaded trees
		/// </summary>
		public static void Export()
		{
			string output = "";
			foreach(CTree tree in CTreeManager.Trees)
			{
				string line = GetLine(tree);
				if(line != null)
					output += line + newLine;
			}
			//CDebug.WriteLine(output);
			WriteToFile(output);
		}

		/// <summary>
		/// Tries to evaluate the equation.
		/// If valid => return emty string.
		/// Else => return exception message.
		/// </summary>
		public static string IsValidEquation(string pEquation)
		{
			Expression expr = CreateExpression(pEquation, 10, 1);
			try
			{
				object eval = expr.Evaluate();
				//WARNING: throws invalid cast e.g. on "1+1"...has to be int cast?
				double result = (double)eval;
				return "";
			}
			catch(Exception e)
			{
				return e.Message;
			}
		}

		private static Expression CreateExpression(string pEquation, float pHeight, double pDBH)
		{
			Expression expr = new Expression(pEquation);
			expr.Parameters[PARAM_HEIGHT] = new Expression(pHeight.ToString("0.00"));
			expr.Parameters[PARAM_DBH] = new Expression(pDBH.ToString("0.00"));
			expr.Parameters[PARAM_E] = new Expression(Math.E.ToString());
			return expr;
		}

		private static double GetTreeStemDiameter(float pTreeHeight)
        {
			Expression dbhExp = CreateExpression(DBH_EQUATION, pTreeHeight, 0);
            dbhExp.Parameters[PARAM_HEIGHT] = new Expression(pTreeHeight.ToString("0.00"));
            double dresulth = (double)dbhExp.Evaluate();
            return dresulth;
        }

        private static double GetTreeBiomass(double pDBH, float pTreeHeight)
        {
			//pTreeHeight is probably not used in the biomass equation but it might be
			Expression biomassExp =CreateExpression(AGB_EQUATION, pTreeHeight, pDBH);
            biomassExp.Parameters[PARAM_DBH] = new Expression(pDBH.ToString("0.00"));
            double result = (double)biomassExp.Evaluate();
            return result;
        }
		
        private static string GetLine(CTree pTree)
        {
            string output = pTree.treeIndex + DELIMETER;

            output += pTree.peak.Center.X.ToString("0.00") + DELIMETER;
            output += pTree.peak.Center.Z.ToString("0.00") + DELIMETER;

			float treeHeight = pTree.GetTreeHeight();
			output += treeHeight.ToString("0.00") + DELIMETER;

            double stemDiameter = GetTreeStemDiameter(treeHeight);
            output += stemDiameter.ToString("0.00") + DELIMETER;

            double biomass = GetTreeBiomass(stemDiameter, treeHeight);
            output += biomass.ToString("0.00") + DELIMETER;

			//to know which tree in OBJ is this one - not needed in biomass file
			//bool debugObjName = true;
			//string objName = debugObjName ? ", " + pTree.GetObjName() : "";
			output += pTree.RefTreeTypeName;// + objName;

            return output;
        }

        private static void WriteToFile(string pText)
        {
            string fileName = "biomass.txt";
            string filePath = CProjectData.outputTileSubfolder + "/" + fileName;
            using (var outStream = File.OpenWrite(filePath))
            using (var writer = new StreamWriter(outStream))
            {
                writer.Write(pText);
            }

            exportedFiles.Add(new FileInfo(filePath));
        }
    }
}
