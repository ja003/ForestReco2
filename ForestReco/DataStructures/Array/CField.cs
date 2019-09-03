using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace ForestReco
{
	public class CField
	{
		public CField Left;
		public CField Right;
		public CField Top;
		public CField Bot;
		private List<CField> neighbours;
		public bool Detail { get; private set; }

		private float stepSize;

		public List<Vector3> points = new List<Vector3>();

		public float? SmoothHeight;
		public float? MaxFilledHeight;

		public float? MinZ; //height = Z
		public float? MaxZ;
		public float? SumZ;

		public int VertexIndex = -1;

		public readonly Tuple<int, int> indexInField;

		public Vector3 center;

		//--------------------------------------------------------------

		public CField(Tuple<int, int> pIndexInField, Vector3 pCenter, float pStepSize, bool pDetail)
		{
			indexInField = pIndexInField;
			center = pCenter;
			stepSize = pStepSize;
			Detail = pDetail;
		}


		//NEIGHBOUR

		public bool IsAnyNeighbourDefined()
		{
			List<CField> _neighbours = GetNeighbours();
			foreach (CField n in _neighbours)
			{
				if (n.IsDefined()) { return true; }
			}
			return false;
		}

		public bool AreAllNeighboursDefined()
		{
			List<CField> _neighbours = GetNeighbours();
			foreach (CField n in _neighbours)
			{
				if (!n.IsDefined()) { return false; }
			}
			return true;
		}

		public CField GetNeighbour(EDirection pNeighbour)
		{
			switch (pNeighbour)
			{
				case EDirection.Bot: return Bot;
				case EDirection.Left: return Left;
				case EDirection.Right: return Right;
				case EDirection.Top: return Top;

				case EDirection.LeftTop: return Left?.Top;
				case EDirection.RightTop: return Right?.Top;
				case EDirection.RightBot: return Right?.Bot;
				case EDirection.LeftBot: return Left?.Bot;
			}
			return null;
		}

		public List<CField> GetPerpendicularNeighbours(EDirection pToDirection)
		{
			List<CField> neigh = new List<CField>();
			CField n1 = null;
			CField n2 = null;
			switch(pToDirection)
			{
				case EDirection.Top:
				case EDirection.Bot:
					n1 = GetNeighbour(EDirection.Left);
					n2 = GetNeighbour(EDirection.Right);
					break;
				case EDirection.RightTop:
				case EDirection.LeftBot:
					n1 = GetNeighbour(EDirection.LeftTop);
					n2 = GetNeighbour(EDirection.RightBot);
					break;
				case EDirection.Right:
				case EDirection.Left:
					n1 = GetNeighbour(EDirection.Top);
					n2 = GetNeighbour(EDirection.Bot);
					break;
				case EDirection.RightBot:
				case EDirection.LeftTop:
					n1 = GetNeighbour(EDirection.RightTop);
					n2 = GetNeighbour(EDirection.LeftBot);
					break;
			}
			if(n1 != null) neigh.Add(n1);
			if(n2 != null) neigh.Add(n2);
			return neigh;
		}

		/// <summary>
		/// All points but those at edge should have assigned neigbours
		/// </summary>
		public bool HasAllNeighbours()
		{
			return Left != null && Right != null && Top != null && Bot != null;
		}

		private bool HasAllNeighboursDefined(bool p8neighbourhood)
		{
			bool neighbourhood4Defined = Left.IsDefined() && Right.IsDefined() && Top.IsDefined() && Bot.IsDefined();
			if (!p8neighbourhood)
			{
				return neighbourhood4Defined;
			}
			else
			{
				return neighbourhood4Defined && Left.Top.IsDefined() && Right.Top.IsDefined() && Left.Bot.IsDefined() &&
					   Right.Bot.IsDefined();
			}
		}
		
		/// <summary>
		/// Returns neighbour using 8-neighbourhood
		/// </summary>
		/// <param name="pIncludeThis"></param>
		/// <returns></returns>
		public List<CField> GetNeighbours(bool pIncludeThis = false)
		{
			if (neighbours != null)
			{
				if (pIncludeThis)
				{
					List<CField> neighbourCopy = new List<CField>();
					neighbourCopy.Add(this);
					foreach (CField n in neighbours)
					{
						neighbourCopy.Add(n);
					}
					return neighbourCopy;
				}
				return neighbours;
			}

			neighbours = new List<CField>();
			if (pIncludeThis)
			{
				neighbours.Add(this);
			}

			var directions = Enum.GetValues(typeof(EDirection));
			foreach (EDirection d in directions)
			{
				CField neighour = GetNeighbour(d);
				if (neighour != null) { neighbours.Add(neighour); }
			}

			return neighbours;
		}

		/// <summary>
		/// Finds the closest neighbour in given direction which doesnt contain any of the given points
		/// </summary>
		public CField GetClosestNeighbourWithout(List<Vector3> pPoints, EDirection pDirection)
		{
			CField current = GetNeighbour(pDirection);
			while(current != null && current.Contains(pPoints, false))
			{
				current = current.GetNeighbour(pDirection);
			}
			return current;
		}

		/// <summary>
		/// If given points are in this field.
		/// </summary>
		/// <param name="pAll">True = all of them, false = at least one</param>
		/// <returns></returns>
		private bool Contains(List<Vector3> pPoints, bool pAll)
		{
			foreach(Vector3 point in pPoints)
			{
				if(pAll && IsPointOutOfField(point))
					return false;
				else if(!pAll && !IsPointOutOfField(point))
					return true;
			}
			return pAll;
		}

		//PUBLIC

		public virtual int? GetColorValue()
		{
			return 0;
			
		}

		public void AddPoint(Vector3 pPoint)
		{
			if (IsPointOutOfField(pPoint))
			{
				CDebug.Error($"point {pPoint} is too far from center {center}");
			}

			float height = pPoint.Z;

			//todo test
			points.Add(pPoint);

			//null checks are neccessary!
			if(SumZ != null) { SumZ += height; }
			else { SumZ = height; }
			if(height > MaxZ || MaxZ == null) { MaxZ = height; }
			if(height < MinZ || MinZ == null) { MinZ = height; }
		}

		public static EDirection GetDirection(CField pFrom, CField pTo)
		{
			Vector3 dir = pTo.center - pFrom.center;
			dir = Vector3.Normalize(dir);
			if(dir.X > 0.5f)
			{
				if(dir.Y > 0.5f)
					return EDirection.RightTop;
				else if(dir.Y < -0.5f)
					return EDirection.RightBot;
				else 
					return EDirection.Right;
			}
			else if(dir.X < -0.5f)
			{
				if(dir.Y > 0.5f)
					return EDirection.LeftTop;
				else if(dir.Y < -0.5f)
					return EDirection.LeftBot;
				else
					return EDirection.Left;
			}
			else
			{
				if(dir.Y > 0.5f)
					return EDirection.Top;
				else if(dir.Y < -0.5f)
					return EDirection.Bot;
				else
					return EDirection.None;
			}
		}

		public bool IsPointOutOfField(Vector3 pPoint)
		{
			float distX = Math.Abs(pPoint.X - center.X);
			float distY = Math.Abs(pPoint.Y - center.Y);
			return distX > stepSize / 2 + FLOAT_E || distY > stepSize / 2 + FLOAT_E;
		}

		const float FLOAT_E = 0.0001f;
				
		public bool IsDefined()
		{
			return points.Count > 0;
		}

		float? maxFromNeighbourhood;
		bool maxFromNeighbourhoodCalculated;

		/// <summary>
		/// Not used now
		/// </summary>
		public float? GetMaxHeightFromNeigbourhood()
		{
			if(maxFromNeighbourhoodCalculated)
				return maxFromNeighbourhood;

			maxFromNeighbourhoodCalculated = true;

			foreach(CField field in GetNeighbours(true))
			{
				if(field.GetHeight() > maxFromNeighbourhood)
					maxFromNeighbourhood = field.GetHeight();
			}
			return maxFromNeighbourhood;
		}

		public float? GetAverageHeightFromNeighbourhood(int pKernelSizeMultiplier)
		{
			int pKernelSize = CGroundArray.GetKernelSize();
			pKernelSize *= pKernelSizeMultiplier;

			int defined = 0;
			float heightSum = 0;
			for (int x = -pKernelSize; x < pKernelSize; x++)
			{
				for (int y = -pKernelSize; y < pKernelSize; y++)
				{
					int xIndex = indexInField.Item1 + x;
					int yIndex = indexInField.Item2 + y;
					CField el = CProjectData.groundArray.GetField(xIndex, yIndex);
					if (el != null && el.IsDefined())
					{
						defined++;
						heightSum += (float)el.GetHeight();
					}
				}
			}
			if (defined == 0) { return null; }
			return heightSum / defined;
		}


		/// <summary>
		/// Finds closest defined fields in direction based on pDiagonal parameter.
		/// Returns average of 2 found heights considering their distance from this field.
		/// </summary>
		public float? GetAverageHeightFromClosestDefined(int pMaxSteps, bool pDiagonal)
		{
			if (IsDefined()) { return GetHeight(); }

			CField closestFirst = null;
			CField closestSecond = null;
			CField closestLeft = GetClosestDefined(pDiagonal ? EDirection.LeftTop : EDirection.Left, pMaxSteps);
			CField closestRight = GetClosestDefined(pDiagonal ? EDirection.RightBot : EDirection.Right, pMaxSteps);
			CField closestTop = GetClosestDefined(pDiagonal ? EDirection.RightTop : EDirection.Top, pMaxSteps);
			CField closestBot = GetClosestDefined(pDiagonal ? EDirection.LeftBot : EDirection.Bot, pMaxSteps);

			closestFirst = closestLeft;
			closestSecond = closestRight;
			if ((closestFirst == null || closestSecond == null) && closestTop != null && closestBot != null)
			{
				closestFirst = closestTop;
				closestSecond = closestBot;
			}
			
			if (closestFirst != null && closestSecond != null)
			{
				CField smaller = closestFirst;
				CField higher = closestSecond;
				if (closestSecond.GetHeight() < closestFirst.GetHeight())
				{
					higher = closestFirst;
					smaller = closestSecond;
				}
				int totalDistance = smaller.GetDistanceTo(higher);
				float? heightDiff = higher.GetHeight() - smaller.GetHeight();
				if (heightDiff != null)
				{
					float? smallerHeight = smaller.GetHeight();
					float distanceToSmaller = GetDistanceTo(smaller);
					
					return smallerHeight + distanceToSmaller / totalDistance * heightDiff;
				}
			}
			else if (!HasAllNeighbours())
			{
				if (closestLeft != null) { return closestLeft.GetHeight(); }
				if (closestTop != null) { return closestTop.GetHeight(); }
				if (closestRight != null) { return closestRight.GetHeight(); }
				if (closestBot != null) { return closestBot.GetHeight(); }
			}
			if (!pDiagonal)
			{
				return GetAverageHeightFromClosestDefined(pMaxSteps, true);
			}

			return null;
		}

		public float? GetHeightDiff(Vector3 pPoint)
		{
			float? height = GetHeight();
			return height == null ? null : pPoint.Z - height;
		}


		public float? GetHeight(bool pUseSmoothHeight = true)
		{
			if (pUseSmoothHeight && SmoothHeight != null)
			{
				return SmoothHeight;
			}
			return MaxZ;
		}

		/// <summary>
		/// Returns the interpolated height.
		/// Interpolation = bilinear.
		/// </summary>
		public float? GetHeight(Vector3 pPoint)
		{
			if (!HasAllNeighbours() || !HasAllNeighboursDefined(true))
			{
				if (!IsDefined())
				{
					return null;
				}
				return GetHeight();
			}
			//return GetHeight(); //uncomment to cancel interpolation

			//http://www.geocomputation.org/1999/082/gc_082.htm
			//3.4 Bilinear interpolation

			List<CField> bilinearFields = GetBilinearFieldsFor(pPoint);
			CField h1 = bilinearFields[0];
			CField h2 = bilinearFields[1];
			CField h3 = bilinearFields[2];
			CField h4 = bilinearFields[3];

			float a00 = (float)h1.GetHeight();
			float a10 = (float)h2.GetHeight() - (float)h1.GetHeight();
			float a01 = (float)h3.GetHeight() - (float)h1.GetHeight();
			float a11 = (float)h1.GetHeight() - (float)h2.GetHeight() - (float)h3.GetHeight() + (float)h4.GetHeight();

			float step = CParameterSetter.groundArrayStep;

			float x = pPoint.X - center.X;
			x += step / 2;
			x = x / step;
			float y = center.Y - pPoint.Y;
			y += step / 2;
			y = y / step;

			if (x < 0 || x > 1 || y < 0 || y > 1)
			{
				CDebug.Error("field " + this + " interpolation is incorrect! x = " + x + " z = " + y);
			}

			//pPoint space coords are X and Z, Y = height
			float hi = a00 + a10 * x + a01 * y + a11 * x * y;
			return hi;
		}

		private List<CField> GetBilinearFieldsFor(Vector3 pPoint)
		{
			List<CField> fields = new List<CField>();
			if (pPoint.X > center.X)
			{
				if (pPoint.Z > center.Z)
				{
					fields.Add(this);
					fields.Add(GetNeighbour(EDirection.Right));
					fields.Add(GetNeighbour(EDirection.Top));
					fields.Add(GetNeighbour(EDirection.RightTop));
				}
				else
				{
					fields.Add(GetNeighbour(EDirection.Bot));
					fields.Add(GetNeighbour(EDirection.RightBot));
					fields.Add(this);
					fields.Add(GetNeighbour(EDirection.Right));
				}
			}
			else
			{
				if (pPoint.Z > center.Z)
				{
					fields.Add(GetNeighbour(EDirection.Left));
					fields.Add(this);
					fields.Add(GetNeighbour(EDirection.LeftTop));
					fields.Add(GetNeighbour(EDirection.Top));
				}
				else
				{
					fields.Add(GetNeighbour(EDirection.LeftBot));
					fields.Add(GetNeighbour(EDirection.Bot));
					fields.Add(GetNeighbour(EDirection.Left));
					fields.Add(this);
				}
			}
			return fields;
		}

		public int GetDistanceTo(CField pGroundField)
		{
			return Math.Abs(indexInField.Item1 - pGroundField.indexInField.Item1) +
				   Math.Abs(indexInField.Item2 - pGroundField.indexInField.Item2);
		}


		/// <summary>
		/// Sets SmoothHeight based on average from neighbourhood
		/// </summary>
		public void CalculateSmoothHeight(double[,] pGaussKernel)
		{
			if (!IsDefined()) { return; }

			//int defined = 0;
			float heightSum = 0;

			float midHeight = (float)GetHeight();

			int kernelSize = CGroundArray.GetKernelSize();

			float gaussWeightSum = 0;
			for (int x = 0; x < kernelSize; x++)
			{
				for (int y = 0; y < kernelSize; y++)
				{
					int xIndex = indexInField.Item1 + x - kernelSize / 2;
					int yIndex = indexInField.Item2 + y - kernelSize / 2;
					CField el = CProjectData.groundArray.GetField(xIndex, yIndex);
					float? elHeight = el?.GetHeight();

					//if element is not defined, use height from the middle element
					float definedHeight = midHeight;
					if (elHeight != null)
					{
						definedHeight = (float)elHeight;
					}
					float gaussWeight = (float)pGaussKernel[x, y];
					gaussWeightSum += gaussWeight;
					heightSum += definedHeight * gaussWeight;
				}
			}

			SmoothHeight = heightSum;
		}


		public void ApplyFillMissingHeight()
		{
			if (IsDefined()) { return; }
			if (MaxFilledHeight == null) { return; }

			Vector3 filledPoint = center;
			filledPoint.Z = (float)MaxFilledHeight;
			AddPoint(filledPoint);
		}


		public void FillMissingHeight(EFillMethod pMethod, int pKernelMultiplier)
		{
			if (IsDefined()) { return; }

			int maxSteps = 1;
			switch (pMethod)
			{
				case EFillMethod.ClosestDefined:
					MaxFilledHeight = GetAverageHeightFromClosestDefined(10 * maxSteps, false);
					break;
				case EFillMethod.FromNeighbourhood:
					MaxFilledHeight = GetAverageHeightFromNeighbourhood(pKernelMultiplier);
					break;
			}
		}

		public enum EFillMethod
		{
			ClosestDefined,
			FromNeighbourhood
		}

		///PRIVATE

		private CField GetClosestDefined(EDirection pDirection, int pMaxSteps)
		{
			if (IsDefined()) { return this; }
			if (pMaxSteps == 0) { return null; }
			return GetNeighbour(pDirection)?.GetClosestDefined(pDirection, pMaxSteps - 1);
		}

		/// <summary>
		/// Returns maximal/minimal point in this field.
		/// pMax: True = maximum, False = minimum
		/// </summary>
		private float? GetHeightExtrem(bool pMax)
		{
			return pMax ? MaxZ : MinZ;
		}

		private float? GetHeightAverage()
		{
			if (!IsDefined()) { return null; }
			return SumZ / points.Count;
		}

		/// <summary>
		/// Returnd point with given local position to this point
		/// </summary>
		private CField GetFieldWithOffset(int pIndexOffsetX, int pIndexOffsetY)
		{
			CField el = this;
			for (int x = 0; x < Math.Abs(pIndexOffsetX); x++)
			{
				el = pIndexOffsetX > 0 ? el.Right : el.Left;
				if (el == null) { return null; }
			}
			for (int y = 0; y < Math.Abs(pIndexOffsetY); y++)
			{
				el = pIndexOffsetY > 0 ? el.Top : el.Bot;
				if (el == null) { return null; }
			}
			return el;
		}

		//UNUSED

		private EDirection GetOpositeNeighbour(EDirection pNeighbour)
		{
			switch (pNeighbour)
			{
				case EDirection.Bot: return EDirection.Top;
				case EDirection.Top: return EDirection.Bot;
				case EDirection.Left: return EDirection.Right;
				case EDirection.Right: return EDirection.Left;
			}
			return EDirection.None;
		}

		//OTHER

		public string ToStringIndex()
		{
			return "[" + indexInField + "].";
		}

		public override string ToString()
		{
			return ToStringIndex() + " Ground = " + GetHeight() + ". Center = " + center;
		}

		public override bool Equals(object obj)
		{
			// Check for null values and compare run-time types.
			if (obj == null || GetType() != obj.GetType())
				return false;

			CField e = (CField)obj;
			return (indexInField.Item1 == e.indexInField.Item1) && (indexInField.Item2 == e.indexInField.Item2);
		}

		public override int GetHashCode()
		{
			return -584859404 + EqualityComparer<Tuple<int, int>>.Default.GetHashCode(indexInField);
		}
	}
}