using System;
using System.Numerics;

namespace ForestReco
{
	/// <summary>
	/// Used only for exporting in original coordinate system.
	/// Normal Vector3 works with float which is not enough.
	/// This class stores Vector using double.
	/// </summary>
	public class CVector3D : CVector2D
	{
		public double Z;
		internal static CVector3D One => new CVector3D(1, 0, 0);
		public static CVector3D UnitX => new CVector3D(1, 0, 0);
		public static CVector3D UnitY => new CVector3D(0, 1, 0);
		public static CVector3D UnitZ => new CVector3D(0, 0, 1);
		public static CVector3D Zero => new CVector3D(0, 0, 0);

		public CVector3D(double x, double y, double z) : base(x, y)
		{
			Z = z;
		}

		public CVector3D() : base(0, 0)
		{
			Z = 0;
		}

		public static double Distance(CVector3D v1, CVector3D v2)
		{
			return Math.Sqrt(Math.Pow(v1.X - v2.X, 2) + Math.Pow(v1.Y - v2.Y, 2) + +Math.Pow(v1.Z - v2.Z, 2));
		}


		public static CVector3D operator +(CVector3D left, CVector3D right)
		{
			return new CVector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static CVector3D operator -(CVector3D left, CVector3D right)
		{
			return new CVector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static CVector3D operator *(CVector3D vec, int val)
		{
			return new CVector3D(vec.X * val, vec.Y * val, vec.Z * val);
		}
		public static CVector3D operator *(int val, CVector3D vec)
		{
			return new CVector3D(vec.X * val, vec.Y * val, vec.Z * val);
		}

		public static CVector3D operator *(CVector3D vec, double val)
		{
			return new CVector3D(vec.X * val, vec.Y * val, vec.Z * val);
		}

		public static CVector3D operator *(double val, CVector3D vec)
		{
			return new CVector3D(vec.X * val, vec.Y * val, vec.Z * val);
		}

		public static CVector3D operator /(CVector3D vec, int val)
		{
			return new CVector3D(vec.X / val, vec.Y / val, vec.Z / val);
		}


		public static explicit operator Vector3(CVector3D vec)
		{
			return new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
		}

		static public implicit operator CVector3D(Vector3 vec)
		{
			return new CVector3D(vec.X, vec.Y, vec.Z);
		}

		public override string ToString()
		{
			return ToString("");
		}

		public string ToString(string pFormat)
		{
			return $"{X.ToString(pFormat)},{Y.ToString(pFormat)},{Z.ToString(pFormat)}";
		}

		public override bool Equals(object obj)
		{
			CVector3D item = obj as CVector3D;

			if(item == null)
			{
				return false;
			}
			return item.X == X && item.Y == Y && item.Z == Z;
		}

	}
}
