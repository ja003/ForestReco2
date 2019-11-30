using System;
using System.Numerics;

namespace ForestReco
{
	public class CVector2D
	{
		public double X;
		public double Y;

		public static CVector2D zero => new CVector2D(0, 0);

		public CVector2D(double x, double y)
		{
			X = x;
			Y = y;
		}


		public static CVector2D operator -(CVector2D left, CVector2D right)
		{
			return new CVector2D(left.X - right.X, left.Y - right.Y);
		}

		public static double Distance(CVector2D v1, CVector2D v2)
		{
			return Math.Sqrt(Math.Pow(v1.X - v2.X, 2) + Math.Pow(v1.Y - v2.Y, 2));
		}

		public double Length()
		{
			return Distance(this, zero);
		}

		public static explicit operator Vector2(CVector2D vec)
		{
			return new Vector2((float)vec.X, (float)vec.Y);
		}

		static public implicit operator CVector2D(Vector2 vec)
		{
			return new CVector2D(vec.X, vec.Y);
		}

		internal static CVector2D Normalize(CVector2D dir)
		{
			return (CVector2D)Vector2.Normalize((Vector2)dir);
		}
	}
}
