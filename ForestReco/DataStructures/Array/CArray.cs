using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Web.WebSockets;

// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace ForestReco
{
	/// <summary>
	/// Field orientation is from topLeft -> botRight, topLeft = [0,0]
	/// </summary>
	public abstract class CArray<TypeField> where TypeField : CField
	{
		protected TypeField[,] array;
		public List<TypeField> fields { get; protected set; }

		public float stepSize { get; }
		public int arrayXRange { get; }
		public int arrayYRange { get; }

		protected Vector3 botLeftCorner;
		protected Vector3 topRightCorner;
		protected Vector3 topLeftCorner;

		//to calculate coordinates of array field index
		private Vector3 CenterOffset;

		public float MinHeight = int.MaxValue; //height = Z
		public float MaxHeight = int.MinValue;

		private float sumZ = 0;
		private int pointsCount = 0;
		public bool Detail { get; private set; }

		//--------------------------------------------------------------

		public CArray(float pStepSize, bool pDetail)
		{
			stepSize = pStepSize;
			Detail = pDetail;

			botLeftCorner = CProjectData.currentTileHeader.BotLeftCorner;
			topRightCorner = CProjectData.currentTileHeader.TopRightCorner;
			topLeftCorner = new Vector3(botLeftCorner.X, topRightCorner.Y, 0);

			float width = topRightCorner.X - botLeftCorner.X;
			float height = topRightCorner.Y - botLeftCorner.Y;

			arrayXRange = (int)(width / stepSize) + 1;
			arrayYRange = (int)(height / stepSize) + 1;

			CenterOffset = new Vector3(arrayXRange / 2f * stepSize, arrayYRange / 2f * stepSize, 0);
			CenterOffset += new Vector3(-stepSize / 2, -stepSize / 2, 0); //better visualization
			CenterOffset += GetCurrentTileOffset();

			InitFields(pStepSize);
			SetNeighbours();

			CAnalytics.arrayWidth = width;
			CAnalytics.arrayHeight = height;

		}

		private void SetNeighbours()
		{
			for(int x = 0; x < arrayXRange; x++)
			{
				for(int y = 0; y < arrayYRange; y++)
				{
					if(x > 0)
					{
						array[x, y].Left = array[x - 1, y];
					}
					if(x < arrayXRange - 1)
					{
						array[x, y].Right = array[x + 1, y];
					}
					if(y > 0)
					{
						array[x, y].Top = array[x, y - 1]; //orig
					}
					if(y < arrayYRange - 1)
					{
						array[x, y].Bot = array[x, y + 1]; //orig
					}
				}
			}
		}

		//TODO: how to init this in this class using TypeField
		protected abstract void InitFields(float pStepSize);

		/// <summary>
		/// Offset of current tile center from main header center
		/// </summary>
		private static Vector3 GetCurrentTileOffset()
		{
			Vector3 diff = CProjectData.mainHeader.Center - CProjectData.currentTileHeader.Center;
			diff.Y *= -1; //to match coord style //TODO: check..weird
			return diff;
		}

		///GETTER

		public TypeField GetElement(Tuple<int, int> pIndex)
		{
			return GetField(pIndex.Item1, pIndex.Item2);
		}

		public TypeField GetField(int pXindex, int pYindex)
		{
			if (!IsWithinBounds(pXindex, pYindex)) { return null; }
			return array[pXindex, pYindex];
		}

		private bool IsWithinBounds(Tuple<int, int> pIndex)
		{
			return IsWithinBounds(pIndex.Item1, pIndex.Item2);
		}

		private bool IsWithinBounds(int pXindex, int pYindex)
		{
			return pXindex >= 0 && pXindex < arrayXRange && pYindex >= 0 && pYindex < arrayYRange;
		}

		public TypeField GetElementContainingPoint(Vector3 pPoint)
		{
			Tuple<int, int> index = GetIndexInArray(pPoint);
			if (!IsWithinBounds(index))
			{
				if (index.Item1 == -1) { index = new Tuple<int, int>(0, index.Item2);}
				if (index.Item2 == -1) { index = new Tuple<int, int>(index.Item1, 0); }
				if (index.Item1 == arrayXRange) { index = new Tuple<int, int>(arrayXRange - 1, index.Item2); }
				if (index.Item2 == arrayYRange) { index = new Tuple<int, int>(index.Item1, arrayYRange - 1); }

				if (!IsWithinBounds(index))
				{
					return null;
				}
				else
				{
					//todo: some points are 1 index away from the range
					CDebug.Error($"pPoint {pPoint} was OOB and was moved to {index}", false);
				}
			}
			return array[index.Item1, index.Item2];
		}

		public float? GetHeightDiffAtPoint(Vector3 pPoint)
		{
			return GetElementContainingPoint(pPoint).GetHeightDiff(pPoint);
		}
		
		public float? GetHeightAtPoint(Vector3 pPoint)
		{
			return GetElementContainingPoint(pPoint).GetHeight(pPoint);
		}
			  		
		public static Tuple<int, int> GetIndexInArray(Vector3 pPoint, 
			Vector3 pTopLeftCorner, float pStepSize)
		{
			//todo: doesnt work correctly, fix!

			int xPos = (int)Math.Floor((pPoint.X - pTopLeftCorner.X) / pStepSize);
			//due to array orientation //changed

			//todo: check if used method rounds up correctly

			//float yDist = (pTopLeftCorner.Y - pPoint.Y) / pStepSize;
			//int yPos = (int)Math.Floor(yDist);
			int yPos2 = (int)Math.Floor((pTopLeftCorner.Y - pPoint.Y) / pStepSize);
			//int yPos3 = (int)yDist;


			return new Tuple<int, int>(xPos, yPos2);
		}

		public Tuple<int, int> GetIndexInArray(Vector3 pPoint)
		{
			Tuple<int, int> pos = GetIndexInArray(pPoint, topLeftCorner, stepSize);

			CField el = GetField(pos.Item1, pos.Item2);

			if (el != null && el.IsPointOutOfField(pPoint))
			{
				CDebug.Error($"point {pPoint} is too far from center {el.center}");
			}

			return pos;
		}

		//PUBLIC

		public enum EPointType
		{
			Ground,
			Vege,
			Preprocess
		}

		public void AddPointInField(Vector3 pPoint, bool pLogErrorInAnalytics = true)
		{
			if(CUtils.IsDebugPoint(pPoint))
			{
				//CDebug.WriteLine("");
			}

			Tuple<int, int> index = GetIndexInArray(pPoint);
			if(!IsWithinBounds(index))
			{
				CDebug.Error($"point {pPoint} is OOB {index}", pLogErrorInAnalytics);
				index = GetIndexInArray(pPoint);
				return;
			}
			TypeField field = array[index.Item1, index.Item2];
			if(field.IsPointOutOfField(pPoint))
			{
				CDebug.Error($"point {pPoint} is too far from center {field.center}");
				index = GetIndexInArray(pPoint);
				field.IsPointOutOfField(pPoint);
			}

			if(pPoint.Z > MaxHeight) MaxHeight = pPoint.Z;
			if(pPoint.Z < MinHeight) MinHeight = pPoint.Z;

			pointsCount++;
			sumZ += pPoint.Z;

			array[index.Item1, index.Item2].AddPoint(pPoint);
		}

		public float GetAverageZ()
		{
			return pointsCount > 0 ? sumZ / pointsCount : 0;
		}

		/*public void AddPointInField(Vector3 pPoint, EPointType pType, bool pLogErrorInAnalytics)
		{
			Tuple<int, int> index = GetPositionInArray(pPoint);
			if (!IsWithinBounds(index))
			{
				CDebug.Error($"point {pPoint} is OOB {index}", pLogErrorInAnalytics);
				return;
			}
			switch (pType)
			{
				case EPointType.Ground:
					array[index.Item1, index.Item2].AddGroundPoint(pPoint);
					break;
				case EPointType.Vege:
					array[index.Item1, index.Item2].AddVegePoint(pPoint);
					break;
				case EPointType.Preprocess:
					array[index.Item1, index.Item2].AddPreProcessVegePoint(pPoint);
					break;
			}
		}*/

		//public void SortPoints()
		//{
		//	foreach (CField field in fields)
		//	{
		//		field.SortPoints();
		//	}
		//}

		public void FillMissingHeights(int pKernelMultiplier)
		{
			FillMissingHeights(CField.EFillMethod.FromNeighbourhood, pKernelMultiplier);
			//FillMissingHeights(CField.EFillMethod.ClosestDefined);
			//FillMissingHeights(CField.EFillMethod.ClosestDefined);
		}

		//todo: dont use during testing, otherwise result is nondeterministic
		bool useRandomForSmoothing = false;

		public void SmoothenArray(int pKernelMultiplier)
		{
			List<TypeField> fieldsRandom = fields;

			//todo: dont use during testing, otherwise result is nondeterministic
			if (useRandomForSmoothing)
			{
				fieldsRandom.Shuffle();
			}

			//prepare gauss kernel
			int kernelSize = GetKernelSize();
			kernelSize *= pKernelMultiplier;
			//cant work with even sized kernel
			if (kernelSize % 2 == 0) { kernelSize++; }
			double[,] gaussKernel = CUtils.CalculateGaussKernel(kernelSize, 1);

			foreach (CField el in fieldsRandom)
			{
				el.CalculateSmoothHeight(gaussKernel);
			}
		}

		public bool IsAllDefined()
		{
			foreach (CField f in fields)
			{
				if (!f.IsDefined()) { return false; }
			}
			return true;
		}

		private const float DEFAULT_KERNEL_SIZE = 5; //IN METERS
		
		public static int GetKernelSize()
		{
			int size = (int)(DEFAULT_KERNEL_SIZE / CParameterSetter.groundArrayStep);
			if (size % 2 == 0) { size++; }
			return size;
		}

		public bool IsPathAscending(Vector3 pFrom, Vector3 pTo, float pMaxPathLength, float pAllowedDescend, int pMinDefinedSteps)
		{
			TypeField fieldFrom = GetElementContainingPoint(pFrom);
			TypeField fieldTo = GetElementContainingPoint(pTo);
			List<TypeField> path = GetPathFrom(fieldFrom, fieldTo, pMaxPathLength);
			float? lastDefinedHeight = null;
			int definedSteps = 0;
			for(int i = 0; i < path.Count - 1; i++)
			{
				if(definedSteps >= pMinDefinedSteps)
					break;

				float? h1 = path[i].GetHeight();
				if(h1 != null)
					lastDefinedHeight = h1;

				float? h2 = path[i + 1].GetHeight();

				//tree 52 - diff je zápornej??QUEE?
				float? diff = h2 - lastDefinedHeight;
				if(diff < -pAllowedDescend)// && diff < 2) //todo: make const?
					return false;

				if(diff != null)
					definedSteps++;
			}
			return true;
		}

		private List<TypeField> GetPathFrom(TypeField pFieldFrom, TypeField pFieldTo, float pMaxPathLength = int.MaxValue)
		{
			TypeField current = pFieldFrom;
			List<TypeField> path = new List<TypeField>() { current };
			float pathLength = 0;
			while(!current.Equals(pFieldTo) && pathLength < pMaxPathLength)
			{
				EDirection dir = CField.GetDirection(current, pFieldTo);
				TypeField newCurrent = (TypeField)current.GetNeighbour(dir);
				pathLength += CUtils.Get2DDistance(current, newCurrent);
				current = newCurrent;
				path.Add(current);
			}
			return path;
		}

		///PRIVATE

		private void ApplyFillMissingHeights()
		{
			foreach (CField f in fields)
			{
				f.ApplyFillMissingHeight();
			}
		}

		private void FillMissingHeights(CField.EFillMethod pMethod, int pKernelMultiplier)
		{
			List<TypeField> fieldsRandom = fields;
			if (useRandomForSmoothing)
			{
				fieldsRandom.Shuffle();
			}

			foreach (CField el in fieldsRandom)
			{
				if (CProjectData.backgroundWorker.CancellationPending) { return; }

				if (!el.IsDefined())
				{
					el.FillMissingHeight(pMethod, pKernelMultiplier);
				}
			}
			ApplyFillMissingHeights();
		}

		//OTHER
		/// <summary>
		/// Returns string for x coordinate in array moved by offset
		/// </summary>
		public float GetFieldXCoord(int pXindex)
		{
			return pXindex * stepSize - CenterOffset.X;
		}

		/// <summary>
		/// Returns string for y coordinate in array moved by offset
		/// </summary>
		public float GetFieldYCoord(int pYindex)
		{
			return pYindex * stepSize - CenterOffset.Y;
		}

		public override string ToString()
		{
			return "Field " + arrayXRange + " x " + arrayYRange + ". Corner = " + topLeftCorner;
		}

		

		public string WriteBounds(bool pConsole = true)
		{
			string output = "[" + botLeftCorner + "," + topRightCorner + "]";
			if (pConsole) { CDebug.WriteLine(output); }
			return output;
		}

		public float? GetPointHeight(Vector3 pPoint)
		{
			float? groundHeight = GetHeightAtPoint(pPoint);
			if (groundHeight == null) { return null; }
			return pPoint.Z - groundHeight;
		}

		public bool IsAtBorder(Vector3 pPoint)
		{
			float distanceToBorder = GetDistanceToBorderFrom(pPoint);
			return distanceToBorder < CParameterSetter.GetFloatSettings(ESettings.treeExtent);
		}

		public float  GetDistanceToBorderFrom(Vector3 pPoint)
		{
			return GetDistanceAndDirectionToBorderFrom(pPoint).Item1;
		}

		/// <summary>
		/// Returns the shortest distance to one of the array border 
		/// and bool value: True = the shortest distance is to left or right border
		/// </summary>
		public Tuple<float, bool> GetDistanceAndDirectionToBorderFrom(Vector3 pPoint)
		{
			float xDistToRight = topRightCorner.X - pPoint.X;
			float xDistToLeft = pPoint.X - botLeftCorner.X;
			float xDist = Math.Min(xDistToLeft, xDistToRight);

			float yDistToTop = topRightCorner.Y - pPoint.Y;
			float yDistToBot = pPoint.Y - botLeftCorner.Y;
			float yDist = Math.Min(yDistToBot, yDistToTop);

			float dist = Math.Min(xDist, yDist);
			bool closestToHorizontal = xDist < yDist;

			return new Tuple<float, bool>(dist, closestToHorizontal);
		}

		public static float GetStepSizeForWidth(int pMaxArrayWidth, float pActualArrayWidth)
		{
			float width = pActualArrayWidth; //CProjectData.currentTileHeader.Width; //in meters
			const float minStepSize = .1f;
			float stepSize = minStepSize;
			int arrayWidth = (int)(width / stepSize);
			if (arrayWidth > pMaxArrayWidth)
			{
				float scale = arrayWidth / (float)pMaxArrayWidth;
				stepSize *= scale;
			}
			return stepSize;
		}
	}
}