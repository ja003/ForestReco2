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
		public Vector3 furthestPoint;

		public Vector3 furthestPointPlusX;
		public Vector3 furthestPointMinusX;
		public Vector3 furthestPointPlusY;
		public Vector3 furthestPointMinusY;

		const float BALL_DIAMETER = 0.145f;
		const float DEBUG_OFFSET = 0.0005f;
		private const float DIST_TOLLERANCE = 0.01f;

		public bool isValid = true;

		public CBall(List<Vector3> pPoints)
		{
			pPoints.Sort((a, b) => b.Z.CompareTo(a.Z));

			ballTop = pPoints[0];
			furthestPoint = ballTop;
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

				if(dist2D > GetMaxPointsDist(1) / 2)
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

			//if(ballBot == null)
			//	isBall = false;

			if(GetFurthestPointDist2D() < (GetMaxPointsDist(-3) / 2))
				isValid = false;
		}

		public List<Vector3> GetMainPoints(bool pAddDebugLine)
		{
			List<Vector3> points = new List<Vector3>();
			points.Add(ballTop);

			points.AddRange(GetPointLine(ballTop, Vector3.UnitZ));

			if(ballBot != null)
			{
				points.Add((Vector3)ballBot);
				if(pAddDebugLine)
					points.AddRange(GetPointLine((Vector3)ballBot, -Vector3.UnitZ));
			}

			if(IsValidMainPoint(furthestPointPlusX))
			{
				points.Add(furthestPointPlusX);
				if(pAddDebugLine)
					points.AddRange(GetPointLine(furthestPointPlusX, Vector3.UnitX));
			}
			if(IsValidMainPoint(furthestPointMinusX))
			{
				points.Add(furthestPointMinusX);
				if(pAddDebugLine)
					points.AddRange(GetPointLine(furthestPointMinusX, -Vector3.UnitX));
			}

			if(IsValidMainPoint(furthestPointPlusY))
			{
				points.Add(furthestPointPlusY);
				if(pAddDebugLine)
					points.AddRange(GetPointLine(furthestPointPlusY, Vector3.UnitY));
			}
			if(IsValidMainPoint(furthestPointMinusY))
			{
				points.Add(furthestPointMinusY);
				if(pAddDebugLine)
					points.AddRange(GetPointLine(furthestPointMinusY, -Vector3.UnitY));
			}
			return points;
		}

		private List<Vector3> GetPointLine(Vector3 pStart, Vector3 pDirection, float pLength = 100)
		{
			List<Vector3> points = new List<Vector3>();
			for(float i = 1; i < pLength; i++)
			{
				points.Add(pStart + pDirection * DEBUG_OFFSET * i);
			}
			return points;
		}

		private bool IsValidMainPoint(Vector3 pPoint)
		{
			return Vector3.Distance(ballTop, pPoint) > DIST_TOLLERANCE;
		}

		private float GetFurthestPointDist2D()
		{
			return CUtils.Get2DDistance(ballTop, furthestPoint);
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
				furthestPoint = pPoint;


			float diff = 0;

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

		public Vector3 GetCenter()
		{
			if(!isValid)
				return Vector3.Zero;

			Vector3 c1 = (furthestPointMinusY + furthestPointPlusY) / 2;

			Vector3 p1 = ballBot != null ? ballTop : furthestPointMinusY;
			Vector3 p2 = ballBot != null ? (Vector3)ballBot: furthestPointPlusY;
			Vector3 c2 = (p1 + p2) / 2;
			Vector3 center = (c1 + c2) / 2;
			//toto: move center in correct dir
			return center;
		}
	}
}
