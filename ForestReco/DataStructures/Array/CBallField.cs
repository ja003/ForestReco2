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

		public float ExpectedGroundZ = int.MaxValue;
		public List<Vector3> filteredOut = new List<Vector3>();

		public CBallField(Tuple<int, int> pIndexInField, Vector3 pCenter, float pStepSize, bool pDetail) : 
			base(pIndexInField, pCenter, pStepSize, pDetail)
		{
		}
		
		public override void FillMissingHeight(EFillMethod pMethod, int pKernelMultiplier)
		{
			if(MinZ != null)
			{
				ExpectedGroundZ = (float)MinZ;
				return;
			}

			int maxSteps = 1;
			switch(pMethod)
			{
				case EFillMethod.ClosestDefined:
					float? avgMinZ = GetAverageHeightFromClosestDefined(10 * maxSteps, false, EHeight.MinZ);
					if(avgMinZ != null)
						ExpectedGroundZ = (float)avgMinZ;

					break;
				case EFillMethod.FromNeighbourhood:
					CDebug.Error("Unsupported method");
					break;
			}
		}

		public void FilterPointsAtHeight(float pMinHeight, float pMaxHeight)
		{
			for(int i = points.Count - 1; i >= 0; i--)
			{
				Vector3 p = points[i];
				float height = p.Z - ExpectedGroundZ;
				if(height < pMinHeight || height > pMaxHeight)
				{
					points.RemoveAt(i);
					filteredOut.Add(p);
				}
			}
		}

		public override void AddPoint(Vector3 pPoint)
		{
			base.AddPoint(pPoint);

			if(MinZ < ExpectedGroundZ)
				ExpectedGroundZ = (float)MinZ;
		}

		public CBall ball;

		public void Detect()
		{
			if(!HasAllNeighbours())
				return;

			if(IsBallInNeigbhourhood())
				return;

			List<Vector3> processPoints = points;
			if(processPoints.Count == 0)
				return;

			//part of ball can be in neighbours
			//we take into consideration only 4 neighbours:
			//this, right, bot and bot-right
			//ball shouldnt span over more fields than 4
			processPoints.AddRange(Right.points);
			processPoints.AddRange(Bot.points);
			processPoints.AddRange(Bot.Right.points);

			ball = new CBall(processPoints, 
				CParameterSetter.GetFloatSettings(ESettings.minBallHeight),
				CParameterSetter.GetFloatSettings(ESettings.maxBallHeight));

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
