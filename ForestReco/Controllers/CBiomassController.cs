using NCalc;
using System;

namespace ForestReco
{
	public static class CBiomassController
	{
		private const string PARAM_HEIGHT = "height";
		private const string PARAM_DBH = "dbh";
		private const string PARAM_E = "e";
		private const string DELIMETER = ", ";
		private const string NUM_FORMAT = "0.00";
		private static string DBH_EQUATION;
		private static string AGB_EQUATION;

		/// <summary>
		/// Save equations for later evaluation.
		/// Equations are expected to be checked and valid.
		/// </summary>
		public static void Init(string pDBH, string pAGB)
		{
			DBH_EQUATION = pDBH;
			AGB_EQUATION = pAGB;
		}

		/// <summary>
		/// Tries to evaluate the equation.
		/// If valid => return emty string.
		/// Else => return exception message.
		/// </summary>
		public static string IsValidEquation(string pEquation)
		{
            if (pEquation.Length == 0)
                return "Equation cant be empty";

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
            if (pEquation.Length == 0)
                return null;

			Expression expr = new Expression(pEquation);
			expr.Parameters[PARAM_HEIGHT] = new Expression(pHeight.ToString(NUM_FORMAT));
			expr.Parameters[PARAM_DBH] = new Expression(pDBH.ToString(NUM_FORMAT));
			expr.Parameters[PARAM_E] = new Expression(Math.E.ToString());
			return expr;
		}

		/// <summary>
		/// DBH
		/// </summary>
		public static double GetTreeStemDiameter(float pTreeHeight)
		{
			Expression dbhExp = CreateExpression(DBH_EQUATION, pTreeHeight, 0);
			dbhExp.Parameters[PARAM_HEIGHT] = new Expression(pTreeHeight.ToString(NUM_FORMAT));
			double dresulth = (double)dbhExp.Evaluate();
			return dresulth;
		}

		/// <summary>
		/// AGB
		/// </summary>
		public static double GetTreeBiomass(double pDBH, float pTreeHeight)
		{
			//pTreeHeight is probably not used in the biomass equation but it might be
			Expression biomassExp = CreateExpression(AGB_EQUATION, pTreeHeight, pDBH);
			biomassExp.Parameters[PARAM_DBH] = new Expression(pDBH.ToString(NUM_FORMAT));
			double result = (double)biomassExp.Evaluate();
			return result;
		}
	}
}
