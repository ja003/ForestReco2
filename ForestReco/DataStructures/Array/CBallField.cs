using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public class CBallField : CField
	{
		public List<Vector3> filteredOut = new List<Vector3>();

		//keeps record if there is any point at height defined by GetHeightKey
		HashSet<int> definedAtHeight = new HashSet<int>();
		HashSet<int> filteredOutAtHeight = new HashSet<int>();

		public CBallField(Tuple<int, int> pIndexInField, Vector3 pCenter, float pStepSize, bool pDetail) :
			base(pIndexInField, pCenter, pStepSize, pDetail)
		{
		}

		public override void AddPoint(Vector3 pPoint)
		{
			AddPoint(pPoint, false);
		}

		/// <summary>
		/// Adds either normal or filtered out point.
		/// Usage: Filtering is done in normal array and points are then coppied 
		/// into detail array. We need to keep filtered out points as well.
		/// </summary>
		public void AddPoint(Vector3 pPoint, bool pFilteredOut)
		{
			int heightKey = GetHeightKey(pPoint);
			if(!pFilteredOut)
			{
				base.AddPoint(pPoint);
				definedAtHeight.Add(heightKey);
			}
			else
			{
				filteredOut.Add(pPoint);
				filteredOutAtHeight.Add(heightKey);
			}
		}

		private int GetHeightKey(Vector3 pPoint)
		{
			return GetHeightKey(pPoint.Z);
		}

		/// <summary>
		/// Calculates height key.
		/// Percision is always 0.1
		/// </summary>
		private int GetHeightKey(float pHeight)
		{
			return (int)(pHeight * 10);
		}

		public override void FillMissingHeight(EFillMethod pMethod, int pParam)
		{
			if(heightFilled)
				return;

			float? minNeighbourAvg = GetAverageHeightFromNeighbourhood(1, EHeight.MinZ);
			float? minNeighbour = GetKRankHeightFromNeigbourhood(9, 3);

			if(minNeighbour != null && minNeighbourAvg != null)
			{
				if(Math.Abs((float)minNeighbour - (float)minNeighbourAvg) > 0.5f)
				{
					CDebug.WriteLine();
				}

				MinFilledHeight = Math.Min((float)minNeighbour, (float)minNeighbourAvg);
			}
		}

		public override void ApplyFillMissingHeight()
		{
			if(heightFilled) return;
			if(MinFilledHeight == null) { return; }

			Vector3 filledPoint = Center;
			filledPoint.Z = (float)MinFilledHeight;
			AddPoint(filledPoint);
			heightFilled = true;

			float? minNeighbourAvg = GetAverageHeightFromNeighbourhood(1, EHeight.MinZ);
			if(Math.Abs((float)MinFilledHeight - (float)minNeighbourAvg) > 0.5f)
			{
				heightFilled = false;
			}
		}

		internal void FilterPointsAtDistance(float pMinDistance, float pMaxDistance)
		{
			for(int i = points.Count - 1; i >= 0; i--)
			{
				Vector3 p = points[i];
				float distance = CUtils.Get2DDistance(Vector3.Zero, p);
				if(distance < pMinDistance || distance > pMaxDistance)
				{
					points.RemoveAt(i);
					filteredOut.Add(p);
				}
			}
		}

		/// <summary>
		/// Sort descending
		/// </summary>
		internal void SortPoints()
		{
			points.Sort((a, b) => b.Z.CompareTo(a.Z));
			filteredOut.Sort((a, b) => b.Z.CompareTo(a.Z));
		}

		public void FilterPointsAtHeight(float pMinHeight, float pMaxHeight)
		{
			if(Equals(3, 6))
			{
				CDebug.WriteLine();
			}
			for(int i = points.Count - 1; i >= 0; i--)
			{
				Vector3 p = points[i];

				float height = p.Z;
				if(height < pMinHeight || height > pMaxHeight)
				{
					points.RemoveAt(i);
					filteredOut.Add(p);
				}
			}
			//SORT DESCENDING
			points.Sort((a, b) => b.Z.CompareTo(a.Z));
		}

		public float GetDefinedHeight()
		{
			if(points.Count == 0)
				return 0;
			return points[0].Z;
		}

		internal int FilterFieldsWithDefinedNeighbours()
		{
			if(Equals(26, 9))
				CDebug.WriteLine();

			if(!IsDefined())
				return 0;

			List<CField> definedNeighbours = GetDefinedNeighbours(3);

			int filteredSum = 0;

			if(definedNeighbours.Count > 4)
			{
				filteredSum += FilterOutAllDefinedNeighbours();
			}
			return filteredSum;
		}

		private int FilterOutAllDefinedNeighbours()
		{
			if(!IsDefined())
				return 0;
			FilterOut();

			int filteredSum = 1;
			foreach(CBallField n in GetNeighbours())
			{
				filteredSum += n.FilterOutAllDefinedNeighbours();
			}
			return filteredSum;
		}

		private void FilterOut()
		{
			filteredOut.AddRange(points);
			points.Clear();
		}

		public CBall ball;

		private bool IsProcessedNeighbourhoodDefined()
		{
			if(IsDefined())
				return true;
			if(Left.IsDefined())
				return true;
			if(Bot.IsDefined())
				return true;
			if(Bot.Right.IsDefined())
				return true;

			return false;
		}

		/// <summary>
		/// Try to detect ball in fields:
		/// - this
		/// - right
		/// - bot
		/// - bot.right
		/// First the filtering is done:
		/// - there cant be any filtered out points in similar height as the 
		/// highest point in the processed fields
		/// - also no similar heights in neighbouring fields
		/// </summary>
		public void Detect(bool pForce)
		{
			if(CDebug.IsDebugField(this))
				CDebug.WriteLine();

			if(!HasAllNeighbours())
				return;

			if(IsBallInNeigbhourhood())
				return;

			if(!IsProcessedNeighbourhoodDefined())
				return;

			//not processed neighbours shouldnt have similar height as the processed field
			//if some of them does => there is some object in the potential ball height =>
			//the ball cant be here
			float processHeight = GetProcessedHeight();

			List<CBallField> neighboursAroundProcessed = GetNeighboursAroundProcessed();

			foreach(CBallField n in neighboursAroundProcessed)
			{
				if(n.IsDefinedAtHeight(processHeight, 0.1f, false))
					return;
			}

			//check if there werent any filtered out points in similar height
			//as in the processed fields
			List<CBallField> processedFields = new List<CBallField>();
			processedFields.Add(this);
			processedFields.Add((CBallField)Right);
			processedFields.Add((CBallField)Bot);
			processedFields.Add((CBallField)Bot.Right);
			foreach(CBallField f in processedFields)
			{
				if(f.IsDefinedAtHeight(processHeight, 0.3f, true))
					return;
			}

			//we need to make a copy in order not to modify the points
			Vector3[] _processPoints = new Vector3[points.Count];
			points.CopyTo(_processPoints);
			List<Vector3> processPoints = _processPoints.ToList();

			//part of ball can be in neighbours
			//we take into consideration only 4 neighbours:
			//this, right, bot and bot-right
			//ball shouldnt span over more fields than 4
			processPoints.AddRange(Right.points);
			processPoints.AddRange(Bot.points);
			processPoints.AddRange(Bot.Right.points);

			const int min_ball_points = 300;
			if(processPoints.Count < min_ball_points)
				return;

			//try to create the ball from processed points
			ball = new CBall(processPoints, pForce, this);
		}

		/// <summary>
		/// Returns max height of processed fields
		/// </summary>
		private float GetProcessedHeight()
		{
			float h0 = GetDefinedHeight();
			float maxHeight = h0;
			float h1 = ((CBallField)Right).GetDefinedHeight();
			float h2 = ((CBallField)Right.Bot).GetDefinedHeight();
			float h3 = ((CBallField)Bot).GetDefinedHeight();
			if(h1 > maxHeight)
				maxHeight = h1;
			if(h2 > maxHeight)
				maxHeight = h2;
			if(h3 > maxHeight)
				maxHeight = h3;
			return maxHeight;
		}

		/// <summary>
		/// Check if there is any (filtered out) point in the given height
		/// </summary>
		private bool IsDefinedAtHeight(float pHeight, float pRangeTollerance, bool pFilteredOut)
		{
			const float height_diff_step = 0.1f;
			for(float i = -pRangeTollerance; i < pRangeTollerance; i += height_diff_step)
			{
				float h = pHeight + i;
				int heightKey = GetHeightKey(h);
				if(pFilteredOut)
				{
					if(filteredOutAtHeight.Contains(heightKey))
						return true;
				}
				else
				{
					if(definedAtHeight.Contains(heightKey))
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns neighbours around:
		/// - this
		/// - right
		/// - right.bot
		/// - bot
		/// TODO: use effectively GetNeighbours()?
		/// </summary>
		/// <returns></returns>
		private List<CBallField> GetNeighboursAroundProcessed()
		{
			List<CBallField> neighs = new List<CBallField>();
			if(!HasAllNeighbours())
				return neighs;

			neighs.Add((CBallField)Left);
			neighs.Add((CBallField)Left.Top);
			neighs.Add((CBallField)Top);
			neighs.Add((CBallField)Top.Right);

			CBallField n = (CBallField)Top.Right?.Right;
			if(n != null)
				neighs.Add(n);
			n = (CBallField)n?.Bot;
			if(n != null)
				neighs.Add(n);
			n = (CBallField)n?.Bot;
			if(n != null)
				neighs.Add(n);
			n = (CBallField)n?.Bot;
			if(n != null)
				neighs.Add(n);
			n = (CBallField)n?.Left;
			if(n != null)
				neighs.Add(n);

			neighs.Add((CBallField)Left.Bot);
			n = (CBallField)Left.Bot.Bot;
			if(n != null)
				neighs.Add(n);
			n = (CBallField)n?.Right;
			if(n != null)
				neighs.Add(n);

			return neighs;
		}

		/// <summary>
		/// Check if diffence between processed height and neighbourhood is small enough.
		/// If is high => there shouldnt be ball opn this field
		/// </summary>
		/// <param name="pDiff"></param>
		/// <returns></returns>
		private bool IsHeightDiffInvalid(float pDiff)
		{
			if(!IsDefined() && pDiff == 0)
				return false;

			const float min_neighbour_height_diff = 0.3f;
			return /*pDiff > 0 || */Math.Abs(pDiff) < min_neighbour_height_diff;
		}

		private bool IsBallInNeigbhourhood()
		{
			foreach(CBallField neigbour in GetNeighbours())
			{
				if(neigbour.ball != null && neigbour.ball.isValid)
					return true;
			}
			return false;
		}
	}
}
