using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using csMatrix;

namespace ForestReco
{
	/// <summary>
	/// Manages calculation of a transformation between 2 sets of points
	/// to match.
	/// </summary>
	public static class CBallsTransformator
	{
		/// <summary>
		/// Calculates a rotation and translation that needs to be applied
		/// on pSetA to match pSetB
		/// </summary>
		public static void GetRigidTransform(List<Vector3> pSetA, List<Vector3> pSetB)
		{
			if(pSetA.Count != pSetB.Count)
			{
				return;
			}
			CDebug.WriteLine($"GetRigidTransform from set : {CDebug.GetString(pSetA)} to {CDebug.GetString(pSetB)}");

			//List<Vector3> setAorig;
			Vector3[] setAorig = new Vector3[pSetA.Count];
			pSetA.CopyTo(setAorig);
			Vector3[] setBorig = new Vector3[pSetB.Count];
			pSetB.CopyTo(setBorig);

			Vector3 centroid_A = CUtils.GetAverage(pSetA);
			Vector3 centroid_B = CUtils.GetAverage(pSetB);

			CUtils.MovePointsBy(ref pSetA, -centroid_A);
			CUtils.MovePointsBy(ref pSetB, -centroid_B);

			Matrix mA = CreateMatrix(pSetA);
			Matrix mB = CreateMatrix(pSetB);
			Matrix A_multiply_B = mA.MultiplyTransposeBy(mB);

			double[,] H = ConvertToDouble(A_multiply_B);

			double[] w = new double[3];
			double[,] u = new double[3, 3];
			double[,] vt = new double[3, 3];
			//U, S, Vt = linalg.svd(H)

			//viz https://radfiz.org.ua/files/temp/Lab1_16/alglib-3.4.0.csharp/csharp/manual.csharp.html
			const int u_needed = 2;
			const int vt_needed = 2;
			const int additional_memory = 2;
			alglib.svd.rmatrixsvd(H, 3, 3, u_needed, vt_needed, additional_memory,
				ref w, ref u, ref vt, null);

			Matrix Vt = new Matrix(vt);
			Matrix U = new Matrix(u);
			Matrix R = Vt.MultiplyTransposeBy(U.Transpose(true));
			//R = Vt.T * U.T

			if(GetDeterminant(ConvertToDouble(R), 3) < 0)
			{
				CDebug.Warning("Reflection detected");
				//for(int i = 0; i < R.Rows; i++)
				//{
				//	R[i, 2] *= -1; // NOT THE SAME!
				//}
				vt[2, 0] *= -1;
				vt[2, 1] *= -1;
				vt[2, 2] *= -1;
				//Matrices need reinitialization - they are modified
				Vt = new Matrix(vt);
				U = new Matrix(u);
				R = Vt.MultiplyTransposeBy(U.Transpose(true));
			}

			Matrix centerA =
				new Matrix(new double[,] { { centroid_A.X, centroid_A.Y, centroid_A.Z } });
			Matrix centerB =
				new Matrix(new double[,] { { centroid_B.X, centroid_B.Y, centroid_B.Z } });

			//create a copy - operations affects the original object
			Matrix Rcopy = new Matrix(R);
			Matrix _R_mult_centerA = Rcopy.Negate().MultiplyByTranspose(centerA);
			Matrix T = _R_mult_centerA + centerB.Transpose(true);

			CDebug.WriteLine("Rotation = ");
			CDebug.WriteLine("" + R);
			CDebug.WriteLine("Translation = ");
			CDebug.WriteLine("" + T);

			Check(setAorig.ToList(), setBorig.ToList(), R, T);
		}



		/// <summary>
		/// Checks if all points from pSetA transforms correctly 
		/// to one of points from the pSetB after applying
		/// pRotation and pTranslation
		/// </summary>
		private static void Check(List<Vector3> pSetA, List<Vector3> pSetB, Matrix pRotation, Matrix pTranslation)
		{
			CDebug.WriteLine("CHECK");
			foreach(Vector3 p in pSetA)
			{
				Vector3 transformedP = GetTransformed(p, pRotation, pTranslation);
				CDebug.WriteLine($"Point {p} transformed to {transformedP}. Result = {SetContains(pSetB, transformedP, 0.01f)}");
			}
			CDebug.WriteLine("=====");
		}

		/// <summary>
		/// True = one of points from pSet has distance to the pPoint 
		/// lower than pMaxOffset
		/// </summary>
		private static object SetContains(List<Vector3> pSet, Vector3 pPoint, float pMaxOffset)
		{
			foreach(Vector3 p in pSet)
			{
				if(Vector3.Distance(p, pPoint) < pMaxOffset)
					return true;
			}
			return false;
		}

		private static Vector3 GetTransformed(Vector3 pPoint, Matrix pRotation, Matrix pTranslation)
		{
			Matrix p = CreateMatrix(pPoint);
			Matrix rotation = new Matrix(pRotation);
			Matrix res = rotation.MultiplyByTranspose(p) + pTranslation;
			return ConvertToVector(res);
		}

		private static Vector3 ConvertToVector(Matrix pMatrix)
		{
			float x = (float)pMatrix[0, 0];
			x = Math.Abs(x) < 0.001f ? 0 : x;
			float y = (float)pMatrix[1, 0];
			y = Math.Abs(y) < 0.001f ? 0 : y;
			float z = (float)pMatrix[2, 0];
			z = Math.Abs(z) < 0.001f ? 0 : z;
			return new Vector3(x, y, z);
		}

		private static double[,] ConvertToDouble(Matrix pMatrix)
		{
			double[,] m = new double[pMatrix.Rows, pMatrix.Columns];
			for(int i = 0; i < pMatrix.Rows; i++)
			{
				for(int j = 0; j < pMatrix.Columns; j++)
				{
					m[i, j] = pMatrix[i, j];
				}
			}
			return m;
		}

		private static double GetDeterminant(double[,] a, int n)
		{
			int i, j, k;
			double det = 0;
			for(i = 0; i < n - 1; i++)
			{
				for(j = i + 1; j < n; j++)
				{
					det = a[j, i] / a[i, i];
					for(k = i; k < n; k++)
						a[j, k] = a[j, k] - det * a[i, k]; // Here's exception
				}
			}
			det = 1;
			for(i = 0; i < n; i++)
				det = det * a[i, i];
			return det;
		}

		private static Matrix CreateMatrix(Vector3 pPoint)
		{
			return CreateMatrix(new List<Vector3>() { pPoint });
		}

		private static Matrix CreateMatrix(List<Vector3> pMatrixA)
		{
			Matrix m = new Matrix(pMatrixA.Count, 3, 0);
			for(int i = 0; i < pMatrixA.Count; i++)
			{
				Vector3 p = pMatrixA[i];
				m[i, 0] = p.X;
				m[i, 1] = p.Y;
				m[i, 2] = p.Z;
			}
			return m;
		}
	}
}
