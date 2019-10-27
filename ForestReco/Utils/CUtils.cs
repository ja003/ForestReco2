using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;

namespace ForestReco
{
	public static class CUtils
	{
		//public static bool PointBelongsToTree(Vector3 pPoint, Vector3 pTreetop)
		//{
		//	Vector3 botPoint = new Vector3(pTreetop.X, pTreetop.Z, pPoint.Z);
		//	float angleBetweenPointAndTreetop = AngleBetweenThreePoints(botPoint, pTreetop, pPoint);
		//	return angleBetweenPointAndTreetop < 45;
		//}

		public static float GetAngle(Vector2 a, Vector2 b)
		{
			a = Vector2.Normalize(a);
			b = Vector2.Normalize(b);
			double atan2 = Math.Atan2(b.Y, b.X) - Math.Atan2(a.Y, a.X);
			return ToDegree((float)atan2);
		}

		public static float GetAngleToTree(CTree pTree, Vector3 pPoint)
		{
			return AngleBetweenThreePoints(pTree.peak.Center - Vector3.UnitZ * 100, pTree.peak.Center, pPoint);
		}

		//https://stackoverflow.com/questions/19729831/angle-between-3-points-in-3d-space
		public static float AngleBetweenThreePoints(Vector3 pA, Vector3 pB, Vector3 pC, bool pToDegree = true)
		{
			Vector3 v1 = new Vector3(pA.X - pB.X, pA.Y - pB.Y, pA.Z - pB.Z);
			//Similarly the vector BC(call it v2) is:

			Vector3 v2 = new Vector3(pC.X - pB.X, pC.Y - pB.Y, pC.Z - pB.Z);
			//The dot product of v1 and v2 is a function of the cosine of the angle between them(it's scaled by the product of their magnitudes). So first normalize v1 and v2:

			float v1mag = (float)Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z);
			Vector3 v1norm = new Vector3(v1.X / v1mag, v1.Y / v1mag, v1.Z / v1mag);

			float v2mag = (float)Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z);
			Vector3 v2norm = new Vector3(v2.X / v2mag, v2.Y / v2mag, v2.Z / v2mag);
			//Then calculate the dot product:

			float res = v1norm.X * v2norm.X + v1norm.Y * v2norm.Y + v1norm.Z * v2norm.Z;
			//And finally, recover the angle:

			float angle = (float)Math.Acos(res);
			if(pToDegree)
			{
				return ToDegree(angle);
			}
			return angle;
		}

		public static float ToRadians(float val)
		{
			return (float)Math.PI / 180 * val;
		}

		private static float ToDegree(float angle)
		{
			return (float)(angle * (180.0 / Math.PI));
		}

		internal static void TransformArrayIndexToBitmapIndex(ref Tuple<int, int> pArrayIndex,
			CHeaderInfo pArrayHeader, float pStepSize, Bitmap pMap)
		{
			float xPercent = pArrayIndex.Item1 * pStepSize / pArrayHeader.Width;
			float yPercent = pArrayIndex.Item2 * pStepSize / pArrayHeader.Height;
			yPercent = 1 - yPercent; //bitmap has different orientation than our array

			if(xPercent < 0 || xPercent > 1 || yPercent < 0 || yPercent > 1)
			{
				CDebug.Error($"wrong transformation! x = {xPercent}, y = {yPercent}");
				xPercent = 0;
				yPercent = 0;
			}

			int bitmapXindex = (int)((pMap.Width - 1) * xPercent);
			int bitmapYindex = (int)((pMap.Height - 1) * yPercent);

			//array is oriented from bot to top, bitmap top to bot
			bitmapYindex = pMap.Height - bitmapYindex - 1;

			pArrayIndex = new Tuple<int, int>(bitmapXindex, bitmapYindex);
		}

		

		internal static bool IsInBitmap(Tuple<int, int> pIndex, Bitmap pBitmap)
		{
			return pIndex.Item1 >= 0 && pIndex.Item1 < pBitmap.Width &&
				pIndex.Item2 >= 0 && pIndex.Item2 < pBitmap.Height;
		}

		public static float Get2DDistance(CTree a, CTree b)
		{
			return Get2DDistance(a.peak.Center, b.peak.Center);
		}

		public static float Get2DDistance(CTreePoint a, CTreePoint b)
		{
			return Vector2.Distance(new Vector2(a.X, a.Y), new Vector2(b.X, b.Y));
		}

		public static float Get2DDistance(Vector3 a, Vector3 b)
		{
			return Vector2.Distance(new Vector2(a.X, a.Y), new Vector2(b.X, b.Y));
		}

		public static float Get2DDistance(Vector3 a, CTreePoint b)
		{
			return Vector2.Distance(new Vector2(a.X, a.Y), new Vector2(b.X, b.Y));
		}

		public static float GetOverlapRatio(CBoundingBoxObject pOfObject, CBoundingBoxObject pWithObject)
		{
			float overlapVolume = GetOverlapVolume(pOfObject, pWithObject);
			float ofObjectVolume = pOfObject.Volume;

			if(ofObjectVolume == 0)
			{
				CDebug.Error("object " + pWithObject + " has no volume");
				return 0;
			}
			float ratio = overlapVolume / ofObjectVolume;
			return ratio;
		}

		public static float Get2DDistance(CField pField1, CField pField2)
		{
			if(pField1 == null || pField2 == null)
			{
				CDebug.Error("Field is null");
				return 0;
			}
			return Get2DDistance(pField1.Center, pField2.Center);
		}

		/// <summary>
		/// copied from: https://stackoverflow.com/questions/5556170/finding-shared-volume-of-two-overlapping-cuboids
		/// </summary>
		private static float GetOverlapVolume(CBoundingBoxObject pObject1, CBoundingBoxObject pObject2)
		{
			float o1minX = pObject1.b000.X;
			float o1minY = pObject1.b000.Y;
			float o1minZ = pObject1.b000.Z;

			float o1maxX = pObject1.b111.X;
			float o1maxY = pObject1.b111.Y;
			float o1maxZ = pObject1.b111.Z;

			float o2minX = pObject2.b000.X;
			float o2minY = pObject2.b000.Y;
			float o2minZ = pObject2.b000.Z;

			float o2maxX = pObject2.b111.X;
			float o2maxY = pObject2.b111.Y;
			float o2maxZ = pObject2.b111.Z;

			float xOverlap = Math.Min(o2maxX, o1maxX) - Math.Max(o2minX, o1minX);
			float yOverlap = Math.Min(o2maxY, o1maxY) - Math.Max(o2minY, o1minY);
			float zOverlap = Math.Min(o2maxZ, o1maxZ) - Math.Max(o2minZ, o1minZ);
			if(xOverlap < 0)
			{
				return 0;
			}
			if(yOverlap < 0)
			{
				return 0;
			}
			if(zOverlap < 0)
			{
				return 0;
			}
			float volume = xOverlap * yOverlap * zOverlap;
			return volume;
		}

		/// <summary>
		/// https://keisan.casio.com/exec/system/1223520411
		/// </summary>
		public static float GetArea(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			float _1 = p1.X * p2.Y;
			float _2 = p2.X * p3.Y;
			float _3 = p3.X * p1.Y;
			float _4 = p1.Y * p2.X;
			float _5 = p2.Y * p3.X;
			float _6 = p3.Y * p1.X;
			float sum = _1 + _2 + _3 - _4 - _5 - _6;
			return Math.Abs(sum) / 2;
		}

		public static double[,] CalculateGaussKernel(int lenght, double weight)
		{
			if(lenght % 2 == 0)
			{
				CDebug.Error("CalculateGaussKernel - lenght cant be even. " + lenght);
			}

			double[,] Kernel = new double[lenght, lenght];
			double sumTotal = 0;

			int kernelRadius = lenght / 2;
			double distance = 0;

			double calculatedEuler = 1.0 /
											 (2.0 * Math.PI * Math.Pow(weight, 2));

			for(int filterY = -kernelRadius;
				filterY <= kernelRadius;
				filterY++)
			{
				for(int filterX = -kernelRadius;
					filterX <= kernelRadius;
					filterX++)
				{
					distance = ((filterX * filterX) +
									(filterY * filterY)) /
								  (2 * (weight * weight));

					Kernel[filterY + kernelRadius,
							filterX + kernelRadius] =
						calculatedEuler * Math.Exp(-distance);

					sumTotal += Kernel[filterY + kernelRadius,
						filterX + kernelRadius];
				}
			}

			for(int y = 0; y < lenght; y++)
			{
				for(int x = 0; x < lenght; x++)
				{
					double finalVal = Kernel[y, x] * (1.0 / sumTotal);
					//CDebug.WriteLine(finalVal.ToString("0.00000"));
					Kernel[y, x] = finalVal;
				}
			}

			return Kernel;
		}

		public static string GetMethodSuffix(EDetectionMethod pMethod)
		{
			switch(pMethod)
			{
				case EDetectionMethod.AddFactor:
					return "_af";
				case EDetectionMethod.Detection2D:
					return "_2d";
				case EDetectionMethod.AddFactor2D:
					return "_af2d";
			}
			return "_noMethod";
		}

		private static Random rng = new Random();

		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while(n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static string SerializeVector3(Vector3 pVector)
		{
			return pVector.X + " " + pVector.Y + " " + pVector.Z;
		}

		public static bool IsPoint(Vector3 pPoint, Vector3 pEqual, float pTolerance = 0.1f)
		{
			return Vector3.Distance(pPoint, new Vector3(675.94f, 128.04f, 1140.9f)) < pTolerance;
		}

		public static float LimitToRange(this float value, float inclusiveMinimum, float inclusiveMaximum)
		{
			if(value < inclusiveMinimum)
			{ return inclusiveMinimum; }
			if(value > inclusiveMaximum)
			{ return inclusiveMaximum; }
			return value;
		}

		public static int LimitToRange(this int value, int inclusiveMinimum, int inclusiveMaximum)
		{
			if(value < inclusiveMinimum)
			{ return inclusiveMinimum; }
			if(value > inclusiveMaximum)
			{ return inclusiveMaximum; }
			return value;
		}

		public static string GetFileName(string pFilePath, bool pWithExtension = false)
		{
			return pWithExtension ? Path.GetFileName(pFilePath) : Path.GetFileNameWithoutExtension(pFilePath);
		}

		public static float GetDistanceFromBorder(Vector3 pPoint,
			Vector3 pBorderBotLeft, Vector3 pBorderTopRight)
		{
			float xDiff = GetDistanceFromBorderX(pPoint, pBorderBotLeft, pBorderTopRight);
			float zDiff = GetDistanceFromBorderY(pPoint, pBorderBotLeft, pBorderTopRight);

			return Math.Min(xDiff, zDiff);
		}

		public static float GetDistanceFromBorderX(Vector3 pPoint,
			Vector3 pBorderBotLeft, Vector3 pBorderTopRight)
		{
			float xDiff = Math.Abs(pPoint.X - pBorderBotLeft.X);
			xDiff = Math.Min(xDiff, Math.Abs(pPoint.X - pBorderTopRight.X));

			return xDiff;
		}

		public static float GetDistanceFromBorderY(Vector3 pPoint,
			Vector3 pBorderBotLeft, Vector3 pBorderTopRight)
		{
			float yDiff = Math.Abs(pPoint.Y - pBorderBotLeft.Y);
			yDiff = Math.Min(yDiff, Math.Abs(pPoint.Y - pBorderTopRight.Y));

			return yDiff;
		}

		/// <summary>
		/// Moves the local position (used in project) by main header offset
		/// back to original global coordinate system
		/// </summary>
		public static CVector3D GetGlobalPosition(Vector3 pLocalPos)
		{
			//without CVector3D cast there is a loss of precision due to float overflow
			return pLocalPos + (CVector3D)CProjectData.mainHeader.Offset; //.GetSwappedYZ();
		}

		public static Vector3 SwapYZ(ref Vector3 clonePoint)
		{
			float tmp = clonePoint.Y;
			clonePoint.Y = clonePoint.Z;
			clonePoint.Z = tmp;
			return clonePoint;
		}

		//public static Vector3 GetSwappedYZ(this Vector3 pVec)
		//{
		//	float tmp = pVec.Z;
		//	pVec.Z = pVec.Y;
		//	pVec.Y = tmp;
		//	return pVec;
		//}
	}
}
