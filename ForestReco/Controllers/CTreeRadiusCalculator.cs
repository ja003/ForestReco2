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

		public static bool NeedsReinit;

		public static void Init()
		{
			string eq = CParameterSetter.GetStringSettings(ESettings.treeRadius);
			treeRadiusExpression = CreateExpression(eq);
			NeedsReinit = false;
		}

		public static float GetTreeRadius(CTree pTree)
		{
			float radius = 0;
			try
			{
				radius = GetTreeRadius(pTree.GetTreeHeight()); 
			}
			catch(Exception)
			{

			}
			return radius;
		}

		public static float GetTreeRadius(float pHeight)
		{
			if(NeedsReinit || treeRadiusExpression == null)
				Init();

			double result = 0;
			try
			{
				treeRadiusExpression.Parameters[PARAM_HEIGHT] = new Expression(pHeight.ToString(NUM_FORMAT));
				treeRadiusExpression.Evaluate();
				result = (double)treeRadiusExpression.Evaluate();
			}
			catch(Exception)
			{

			}
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
