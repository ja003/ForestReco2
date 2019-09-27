using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public class CBall
	{
		public Vector3 ballTop;
		public Vector3? ballBot;
		public Vector3 furthestPoint2D;

		public Vector3 furthestPointPlusX;
		public Vector3 furthestPointMinusX;
		public Vector3 furthestPointPlusY;
		public Vector3 furthestPointMinusY;

		const float BALL_DIAMETER = 0.145f;
		private float BALL_RADIUS => BALL_DIAMETER / 2;

		const float DEBUG_OFFSET = 0.0005f;
		private const float DIST_TOLLERANCE = 0.01f;

		public bool isValid = true;
		public Vector3 center { get; private set; }

		public CBall(List<Vector3> pPoints)
		{
			pPoints.Sort((a, b) => b.Z.CompareTo(a.Z));

			//todo: make balltop a list of points and get avg
			ballTop = pPoints[0];
			furthestPoint2D = ballTop;
			furthestPointPlusX = ballTop;
			furthestPointMinusX = ballTop;
			furthestPointPlusY = ballTop;
			furthestPointMinusY = ballTop;

			foreach(Vector3 point in pPoints)
			{
				float zDiff = ballTop.Z - point.Z;

				if(zDiff > GetMaxPointsDist(1))
					break;

				bool isInBallExtent = Vector3.Distance(point, ballTop) > GetMaxPointsDist(1);
				float dist2D = CUtils.Get2DDistance(point, ballTop);
				UpdateFurthestPoints(point);

				if(ballBot == null && dist2D < 0.01 && zDiff > GetMaxPointsDist(0) / 2)
				{
					ballBot = point;
				}

				if(dist2D > GetMaxPointsDist(3) / 2)
				{
					isValid = false;
					return;
				}

				if(!isInBallExtent)
				{
					bool isUnderBallTop = dist2D < 0.1f;
					if(!isUnderBallTop)
					{
						isValid = false;
						return;
					}
				}
			}

			if(ballBot != null)
			{
				float topBotDiffZ = ballTop.Z - ((Vector3)ballBot).Z;
				//if bot is too close then it is probably not a ball
				if(topBotDiffZ < GetMaxPointsDist(2) / 2)
				{
					isValid = false;
					return;
				}
			}

			float furthestDist2D = GetFurthestPointDist2D();
			float maxDist = GetMaxPointsDist(-3) / 2;
			if(furthestDist2D < maxDist)
			{
				isValid = false;
				return;
			}


			isValid = HasValidMainPoints();

			if(isValid)
				center = CalculateCenter();
		}

		private bool HasValidMainPoints()
		{
			if(IsValidMainPoint(furthestPointPlusX) && IsValidMainPoint(furthestPointMinusX))
			{
				return IsValidMainPoint(furthestPointPlusY) || IsValidMainPoint(furthestPointMinusY);
			}
			else if(IsValidMainPoint(furthestPointPlusY) && IsValidMainPoint(furthestPointMinusY))
			{
				return IsValidMainPoint(furthestPointPlusX) || IsValidMainPoint(furthestPointMinusX);
			}
			else if(ballBot != null)
			{
				return IsValidMainPoint(furthestPointPlusX) || IsValidMainPoint(furthestPointMinusX) ||
					IsValidMainPoint(furthestPointPlusY) || IsValidMainPoint(furthestPointMinusY);
			}
			return false;
		}

		public List<Vector3> GetMainPoints(bool pAddDebugLine)
		{
			List<Vector3> points = new List<Vector3>();
			points.Add(ballTop);

			if(pAddDebugLine)
				points.AddRange(CUtils.GetPointLine(ballTop, Vector3.UnitZ));

			if(pAddDebugLine)
				points.AddRange(CUtils.GetPointLine(furthestPoint2D, -Vector3.UnitZ));


			if(ballBot != null)
			{
				points.Add((Vector3)ballBot);
				if(pAddDebugLine)
					points.AddRange(CUtils.GetPointLine((Vector3)ballBot, -Vector3.UnitZ));
			}

			if(IsValidMainPoint(furthestPointPlusX))
			{
				points.Add(furthestPointPlusX);
				if(pAddDebugLine)
					points.AddRange(CUtils.GetPointLine(furthestPointPlusX, Vector3.UnitX));
			}
			if(IsValidMainPoint(furthestPointMinusX))
			{
				points.Add(furthestPointMinusX);
				if(pAddDebugLine)
					points.AddRange(CUtils.GetPointLine(furthestPointMinusX, -Vector3.UnitX));
			}

			if(IsValidMainPoint(furthestPointPlusY))
			{
				points.Add(furthestPointPlusY);
				if(pAddDebugLine)
					points.AddRange(CUtils.GetPointLine(furthestPointPlusY, Vector3.UnitY));
			}
			if(IsValidMainPoint(furthestPointMinusY))
			{
				points.Add(furthestPointMinusY);
				if(pAddDebugLine)
					points.AddRange(CUtils.GetPointLine(furthestPointMinusY, -Vector3.UnitY));
			}
			return points;
		}



		private bool IsValidMainPoint(Vector3 pPoint)
		{
			return Vector3.Distance(ballTop, pPoint) > DIST_TOLLERANCE &&
				IsAtMainPointZDistance(pPoint);
		}

		private float GetFurthestPointDist2D()
		{
			return CUtils.Get2DDistance(ballTop, furthestPoint2D);
		}

		private void UpdateFurthestPoints(Vector3 pPoint)
		{
			if(!IsAtMainPointZDistance(pPoint))
				return;

			float minFurthestPointDist2D = GetMaxPointsDist(-6) / 2;
			Vector3 diff3D = ballTop - pPoint;
			float diffX = Math.Abs(diff3D.X);
			float diffY = Math.Abs(diff3D.Y);
			float diffZ = Math.Abs(diff3D.Z);
			if(diffZ < minFurthestPointDist2D)
				return;

			float dist2D = CUtils.Get2DDistance(pPoint, ballTop);

			if(dist2D > GetFurthestPointDist2D())
				furthestPoint2D = pPoint;

			float diff;

			if(diffY < DIST_TOLLERANCE)
			{
				if(pPoint.X > ballTop.X)
				{
					diff = pPoint.X - ballTop.X;
					if(/*diff > minFurthestPointDist2D &&*/ diff >= furthestPointPlusX.X - ballTop.X)
						furthestPointPlusX = pPoint;
				}
				else
				{
					diff = ballTop.X - pPoint.X;
					if(/*diff > minFurthestPointDist2D && */diff >= ballTop.X - furthestPointMinusX.X)
						furthestPointMinusX = pPoint;
				}
			}

			if(diffX < DIST_TOLLERANCE)
			{
				if(pPoint.Y > ballTop.Y)
				{
					diff = pPoint.Y - ballTop.Y;
					if(/*diff > minFurthestPointDist2D &&*/ diff >= furthestPointPlusY.Y - ballTop.Y)
						furthestPointPlusY = pPoint;
				}
				else
				{
					diff = ballTop.Y - pPoint.Y;
					if(/*diff > minFurthestPointDist2D && */diff >= ballTop.Y - furthestPointMinusY.Y)
						furthestPointMinusY = pPoint;
				}
			}
		}

		private bool IsAtMainPointZDistance(Vector3 pPoint)
		{
			float zDiff = ballTop.Z - pPoint.Z;
			float min = GetMaxPointsDist(-5) / 2;
			float max = GetMaxPointsDist(5) / 2;
			return zDiff > min && zDiff < max;
		}

		private float GetMaxPointsDist(int pTolleranceMultiply = 0)
		{
			return BALL_DIAMETER + pTolleranceMultiply * DIST_TOLLERANCE;
		}

		private float GetApproxPointCenterDist(int pTolleranceMultiply = 0)
		{
			return BALL_RADIUS + pTolleranceMultiply * DIST_TOLLERANCE;
		}

		/// <summary>
		/// Initial simple method for center approximation.
		/// Not very precise, probably delete.
		/// </summary>
		private Vector3 CalculateCenterB()
		{
			if(!isValid)
				return Vector3.Zero;

			List<Vector3> centers = new List<Vector3>();
			Vector3 centerY = (furthestPointMinusY + furthestPointPlusY) / 2;
			bool isMainPointYValid = IsValidMainPoint(centerY);
			if(isMainPointYValid)
				centers.Add(centerY);

			Vector3 centerX = (furthestPointMinusX + furthestPointPlusX) / 2;
			bool isMainPointXValid = IsValidMainPoint(centerX);
			if(isMainPointXValid)
				centers.Add(centerX);

			Vector3 ceterZ = GetAverage(centers);
			if(ballBot != null)
			{
				ceterZ = ((Vector3)ballBot + ballTop) / 2;
				centers.Add(ceterZ);
			}

			Vector3 approxCenter = GetAverage(centers);
			if(isMainPointXValid && isMainPointYValid)
				return approxCenter;

			//todo: make method
			if(isMainPointXValid)
			{
				Vector3 validMainPointY = (IsValidMainPoint(furthestPointMinusY) ?
						furthestPointMinusY : furthestPointPlusY);
				float yDistToMP = approxCenter.Y - validMainPointY.Y;
				approxCenter = validMainPointY + (yDistToMP > 0 ? 1 : -1) * (approxCenter - validMainPointY) * (BALL_DIAMETER / 2 - Math.Abs(yDistToMP));

			}
			else if(isMainPointYValid)
			{
				Vector3 validMainPointX = (IsValidMainPoint(furthestPointMinusX) ?
						furthestPointMinusX : furthestPointPlusX);
				float xDistToMP = approxCenter.X - validMainPointX.X;
				Vector3 dir = Vector3.Normalize(approxCenter - validMainPointX);
				float dist = BALL_DIAMETER / 2;// - Math.Abs(xDistToMP);
				approxCenter = validMainPointX + (xDistToMP > 0 ? 1 : -1) * dir * dist;
			}
			else
			{
				CDebug.Error("Incorrect calculation");
			}

			return approxCenter;
		}

		/// <summary>
		/// Iterates through possible centers and selects the on having smallest diff
		/// to distance-to-mainPoints function
		/// </summary>
		/// <returns></returns>
		private Vector3 CalculateCenter()
		{
			List<Vector3> possibleCenters = GetPossibleCenters();
			Dictionary<float, Vector3> diffCeters = new Dictionary<float, Vector3>();
			List<Vector3> mainPoints = GetMainPoints(false);

			foreach(Vector3 possibleCenter in possibleCenters)
			{
				float diff = 0;
				//calculate diff to all main points
				foreach(Vector3 mainPoint in mainPoints)
				{
					diff += GetDiffOfPossibleCenter(mainPoint, possibleCenter);
				}

				const float max_diff = 0.2f;
				if(diff < max_diff)
				{
					if(!diffCeters.ContainsKey(diff))
						diffCeters.Add(diff, possibleCenter);
				}
			}

			var tmp = diffCeters.OrderBy(key => key.Key);
			var orderedDiffCenters = tmp.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

			Vector3 center = orderedDiffCenters.First().Value;
			//todo: recalculate with higher precision
			return center;
		}

		/// <summary>
		/// Returns the difference between ball radius and distance of given points.
		/// For real center it should return 0.
		/// </summary>
		private float GetDiffOfPossibleCenter(Vector3 pPoint, Vector3 pPossibleCenter)
		{
			float dist = Vector3.Distance(pPoint, pPossibleCenter);
			return Math.Abs(dist - BALL_RADIUS);
		}

		/// <summary>
		/// Returns points bellow the ball top in 2D distance < ball radius and
		/// Z difference cca equal to ball radius
		/// </summary>
		private List<Vector3> GetPossibleCenters()
		{
			List<Vector3> centers = new List<Vector3>();

			const float step = 0.01f;
			for(float x = -BALL_RADIUS; x < BALL_RADIUS; x += step)
			{
				for(float y = -BALL_RADIUS; y < BALL_RADIUS; y += step)
				{
					for(float z = GetApproxPointCenterDist(-5); z < GetApproxPointCenterDist(5); z += step)
					{
						Vector3 possibleCenter = ballTop + new Vector3(x, y, -z);
						float dist = Vector3.Distance(ballTop, possibleCenter);
						if(dist < GetApproxPointCenterDist(2))
						{
							centers.Add(possibleCenter);
						}
					}
				}
			}

			return centers;
		}

		private static Vector3 GetAverage(List<Vector3> pPoints)
		{
			Vector3 avg = Vector3.Zero;
			foreach(Vector3 p in pPoints)
			{
				avg += p;
			}
			return avg / pPoints.Count;
		}

		public List<Vector3> GetSurfacePoints()
		{
			List<Vector3> points = new List<Vector3>();
			const float step = 0.001f;
			for(float x = -BALL_RADIUS; x < BALL_RADIUS; x += step)
			{
				for(float y = -BALL_RADIUS; y < BALL_RADIUS; y += step)
				{
					for(float z = -BALL_RADIUS; z < BALL_RADIUS; z += step)
					{
						Vector3 point = center + new Vector3(x, y, z);
						float dist = Vector3.Distance(center, point);
						if(Math.Abs(dist - GetApproxPointCenterDist()) < step)
						{
							points.Add(point);
						}
					}
				}
			}
			return points;
		}
	}
}
