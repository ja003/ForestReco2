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
		public const float MAX_OFFSET = 0.001F;

		internal static CRigidTransform GetRigidTransform(List<CBall> pBalls1, List<CBall> pBallsOrig)
		{
			return GetRigidTransform(
				pBalls1.Select(a => a.center).ToList(),
				pBallsOrig.Select(a => a.center).ToList());
		}

		internal static CRigidTransform GetRigidTransform(List<Vector3> pCenters, List<Vector3> pCentersOrig)
		{
			//AllPermutations seems to return better result
			return GetRigidTransformAllPermutations(pCenters, pCentersOrig);
			//return GetRigidTransform(centers1, centersOrig);
		}

		/// <summary>
		/// Calculates rigid transformations between all permutations of larger set
		/// from pCenters and pCentersOrig and the smaller one.
		/// Sets have to have the same points count so we need o try all permutations of the 
		/// larger set.
		/// 
		/// Returns the best transformation (having the smallest offset).
		/// Permutating needs to be done also due to the expected 
		/// indexing in function CalculateRigidTransform 
		/// e.g. input (a,b,c), (a',b',c') = OK, but (a,b,c), (b',a',c') = NOK
		/// </summary>
		private static CRigidTransform GetRigidTransformAllPermutations(List<Vector3> pCenters, List<Vector3> pCentersOrig)
		{
			if(pCenters.Count == 0 || pCentersOrig.Count == 0)
				return null;

			bool isOrigLarger = pCentersOrig.Count > pCenters.Count;
			IEnumerable<IEnumerable<Vector3>> largerSetPermutations = isOrigLarger ? pCentersOrig.Permute() : pCenters.Permute();
			List<Vector3> smallerSet = isOrigLarger ? pCenters : pCentersOrig;

			List<CRigidTransform> rigTransforms = new List<CRigidTransform>();
			foreach(var permutation in largerSetPermutations)
			{
				CRigidTransform rigTransform = CalculateRigidTransform(
					isOrigLarger ? smallerSet : permutation.ToList(),
					isOrigLarger ? permutation.ToList() : smallerSet);

				if(rigTransform.offset < 1)
				{
					rigTransform.CalculateOffsets(pCenters, pCentersOrig);
					rigTransforms.Add(rigTransform);
				}
				//CDebug.WriteLine($"{rigTransform}");
				if(rigTransform.offset < MAX_OFFSET)
					break;
			}

			CRigidTransform minOffsetRigTransform = rigTransforms.Aggregate(
				(curMin, x) => x.offset < curMin.offset ? x : curMin);

			CDebug.WriteLine($"Selected {minOffsetRigTransform}", true, true);
			return minOffsetRigTransform;

		}

		/// <summary>
		/// Same as GetRigidTransformAllPermutations but processes all combinations
		/// of 4 selected centers from both sets
		/// </summary>
		private static CRigidTransform GetRigidTransform4Comb(List<Vector3> pCenters, List<Vector3> pCentersOrig)
		{
			if(pCenters.Count < 4 || pCentersOrig.Count < 4)
			{
				CDebug.Error("Cant GetRigidTransform from less than 4 centers");
				return null;
			}

			List<CRigidTransform> rigTransforms = new List<CRigidTransform>();
			IEnumerable<Vector3[]> origAll4Combinations =
				CUtils.CombinationsRosettaWoRecursion(pCentersOrig.ToArray(), 4);
			IEnumerable<Vector3[]> otherAll4Combinations =
				CUtils.CombinationsRosettaWoRecursion(pCenters.ToArray(), 4);

			foreach(Vector3[] origComb in origAll4Combinations)
			{
				foreach(Vector3[] otherComb in otherAll4Combinations)
				{
					//need to test all permutations because in combinations
					//the order of elements is ignored
					var otherPermutation = otherComb.Permute();
					foreach(IEnumerable<Vector3> otherPerm in otherPermutation)
					{
						CRigidTransform rigTransform =
							CalculateRigidTransform(
								otherPerm.ToList(), origComb.ToList());

						//add only transforms with good offset
						if(rigTransform.offset < 1)
						{
							rigTransform.CalculateOffsets(pCenters, pCentersOrig);
							rigTransforms.Add(rigTransform);
						}

						if(rigTransform.offset < MAX_OFFSET)
							break;
					}

				}
			}

			CRigidTransform minOffsetRigTransform = rigTransforms.Aggregate(
				(curMin, x) => x.offset < curMin.offset ? x : curMin);

			CDebug.WriteLine($"Selected {minOffsetRigTransform}", true, true);
			return minOffsetRigTransform;

		}




		/// <summary>
		/// Calculates a rotation and translation that needs to be applied
		/// on pSetA to match pSetOrig.
		/// The process expects the matching points from sets having the same index.
		/// </summary>
		private static CRigidTransform CalculateRigidTransform(List<Vector3> pSetA, List<Vector3> pSetOrig)
		{
			//prevent modification of the input parameters
			List<Vector3> setA = CUtils.GetCopy(pSetA);
			List<Vector3> setB = CUtils.GetCopy(pSetOrig);

			//setA and setB will be modified
			List<Vector3> setAorig = CUtils.GetCopy(pSetA);
			List<Vector3> setBorig = CUtils.GetCopy(pSetOrig);

			Vector3 centroid_A = CUtils.GetAverage(setA);
			Vector3 centroid_B = CUtils.GetAverage(setB);

			CUtils.MovePointsBy(ref setA, -centroid_A);
			CUtils.MovePointsBy(ref setB, -centroid_B);

			Matrix mA = CreateMatrix(setA);
			Matrix mB = CreateMatrix(setB);
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
			Matrix rotation = Vt.MultiplyTransposeBy(U.Transpose(true));
			//R = Vt.T * U.T

			if(GetDeterminant(ConvertToDouble(rotation), 3) < 0)
			{
				//CDebug.Warning("Reflection detected");

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
				rotation = Vt.MultiplyTransposeBy(U.Transpose(true));
			}

			Matrix centerA =
				new Matrix(new double[,] { { centroid_A.X, centroid_A.Y, centroid_A.Z } });
			Matrix centerB =
				new Matrix(new double[,] { { centroid_B.X, centroid_B.Y, centroid_B.Z } });

			//create a copy - operations affects the original object
			Matrix rotationCopy = new Matrix(rotation);
			Matrix _R_mult_centerA = rotationCopy.Negate().MultiplyByTranspose(centerA);
			Matrix translation = _R_mult_centerA + centerB.Transpose(true);

			//CDebug.WriteLine("Rotation = ");
			//CDebug.WriteLine("" + R);
			//CDebug.WriteLine("Translation = ");
			//CDebug.WriteLine("" + T);


			//List<Vector3> offsetCheckSetA = pFullSetA != null ? pFullSetA : setAorig;
			//List<Vector3> offsetCheckSetB = pFullSetOrig != null ? pFullSetOrig : setBorig;
			//float offset = GetOffset(offsetCheckSetA, offsetCheckSetB, rotation, translation);

			float offset = GetOffset(setAorig, setBorig, rotation, translation);

			return new CRigidTransform(rotation, translation, offset);
		}




		/// <summary>
		/// Returns sum of difference between transformed points from pSetA
		/// and the closest point from pSetB
		/// </summary>
		public static float GetOffset(List<Vector3> pSetA, List<Vector3> pSetB, Matrix pRotation, Matrix pTranslation)
		{
			List<float> offsets = GetOffsets(pSetA, pSetB, pRotation, pTranslation);
			return offsets.Sum();
		}

		/// <summary>
		/// Returns differences between transformed points from pSetA
		/// and the closest point from pSetB
		/// </summary>
		public static List<float> GetOffsets(List<Vector3> pSetA, List<Vector3> pSetB, Matrix pRotation, Matrix pTranslation)
		{
			List<float> offsets = new List<float>();
			//CDebug.WriteLine("GetOffset");

			//todo: this is not valid when sets count are not equal
			int maxCount = Math.Min(pSetA.Count, pSetB.Count);
			for(int i = 0; i < maxCount; i++)
			{
				Vector3 p = pSetA[i];
				Vector3 transformedP = GetTransformed(p, pRotation, pTranslation);
				float distance = GetDistanceToClosest(transformedP, pSetB);
				offsets.Add(distance);
				//CDebug.WriteLine($"Point {p} transformed to {transformedP}. Result = {SetContains(pSetB, transformedP, 0.01f)}");
			}
			//CDebug.WriteLine("=====");
			return offsets;
		}



		/// <summary>
		/// Returns a distance from pPoint to the closest point from pSet
		/// </summary>
		private static float GetDistanceToClosest(Vector3 pPoint, List<Vector3> pSet)
		{
			float distance = int.MaxValue;
			foreach(Vector3 p in pSet)
			{
				float dist = Vector3.Distance(p, pPoint);
				if(dist < distance)
					distance = dist;
			}
			return distance;
		}

		public static Vector3 GetTransformed(Vector3 pPoint, CRigidTransform pTransformation)
		{
			return GetTransformed(pPoint, pTransformation.rotation, pTransformation.translation);
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

	public class CRigidTransform
	{
		public Matrix rotation { get; }
		public Matrix translation { get; }
		public float offset { get; private set; }
		public List<float> offsets { get; private set; }

		public CRigidTransform(Matrix pRotation, Matrix pTransform, float pOffset)
		{
			rotation = pRotation;
			translation = pTransform;
			this.offset = pOffset;
		}

		public override string ToString()
		{
			string offsetsStr = "";
			foreach(float o in offsets)
			{
				offsetsStr += o.ToString("0.000") + " ";
			}
			offsetsStr += "| avg = " + offsets.Average().ToString("0.000") + ", ";
			offsetsStr += "sum = " + offsets.Sum().ToString("0.000");

			return $"RT:\n" +
				$"-rotation =\n{rotation}" +
				$"-translation =\n{translation}" +
				$"-offset (processed points): {offset}\n" +
				$"-offsets: {offsetsStr}";
		}

		internal void CalculateOffsets(List<Vector3> pCenters, List<Vector3> pCentersOrig)
		{
			offsets = CBallsTransformator.GetOffsets(pCenters, pCentersOrig, rotation, translation);
		}
	}
}
