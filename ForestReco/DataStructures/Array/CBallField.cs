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

		//public float ExpectedGroundZ = int.MaxValue;
		public List<Vector3> filteredOut = new List<Vector3>();

		public CBallField(Tuple<int, int> pIndexInField, Vector3 pCenter, float pStepSize, bool pDetail) : 
			base(pIndexInField, pCenter, pStepSize, pDetail)
		{
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

		public void FilterPointsAtHeight(float pMinHeight, float pMaxHeight)
		{
			for(int i = points.Count - 1; i >= 0; i--)
			{
				Vector3 p = points[i];

				//todo: right now we cant rely on ground being defined everywhere - fix and use
				CGroundField groundField = CProjectData.Points.groundArray.GetFieldContainingPoint(p);
				float? height = p.Z - groundField.GetHeight();
				//float height = p.Z - ExpectedGroundZ;
				if(height < pMinHeight || height > pMaxHeight)
				{
					points.RemoveAt(i);
					filteredOut.Add(p);
				}
			}
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
				//foreach(CBallField n in GetNeighbours(true))
				//{
				//	n.filteredOut.AddRange(n.points);
				//	n.points.Clear();
				//}
			}

			//if(AreAllNeighboursDefined())
			//{				
			//	foreach(CBallField n in GetNeighbours(true))
			//	{
			//		n.filteredOut.AddRange(n.points);
			//		n.points.Clear();
			//	}
			//	return true;
			//}
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

		/*public override void AddPoint(Vector3 pPoint)
		{
			base.AddPoint(pPoint);

			if(MinZ < ExpectedGroundZ)
				ExpectedGroundZ = (float)MinZ;
		}*/

		public CBall ball;

		public void Detect()
		{
			if(!HasAllNeighbours())
				return;

			if(IsBallInNeigbhourhood())
				return;

			if(!IsDefined())
				return;

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

			const int min_ball_points = 100;
			if(processPoints.Count < min_ball_points)
				return;


			ball = new CBall(processPoints);

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
