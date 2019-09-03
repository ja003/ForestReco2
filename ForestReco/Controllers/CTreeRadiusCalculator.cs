using NCalc;
using System;

namespace ForestReco
{
	public static class CTreeRadiusCalculator
	{
		private const string PARAM_HEIGHT = "height";
		private const string NUM_FORMAT = "0.00";
		private const string PARAM_E = "e";

		static Expression treeRadiusExpression;

		public static void Init()
		{
			string eq = CParameterSetter.GetStringSettings(ESettings.treeRadius);
			treeRadiusExpression = CreateExpression(eq);
		}

		public static float GetTreeRadius(float pHeight)
		{
			if(treeRadiusExpression == null)
				Init();

			treeRadiusExpression.Parameters[PARAM_HEIGHT] = new Expression(pHeight.ToString(NUM_FORMAT));
			treeRadiusExpression.Evaluate();
			double result = (double)treeRadiusExpression.Evaluate();
			return (float)result;
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

            Expression expr = CreateExpression(pEquation);
			try
			{
				expr.Parameters[PARAM_HEIGHT] = new Expression(10.ToString(NUM_FORMAT));
				object eval = expr.Evaluate();
				double result = (double)eval;
				return "";
			}
			catch(Exception e)
			{
				return e.Message;
			}
		}

		private static Expression CreateExpression(string pEquation)
		{
            if (pEquation.Length == 0)
                return null;

            Expression expr = new Expression(pEquation);
			//expr.Parameters[PARAM_HEIGHT] = new Expression(pHeight.ToString(NUM_FORMAT));
			expr.Parameters[PARAM_E] = new Expression(Math.E.ToString());
			return expr;
		}
	}
}
