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
		public List<Vector3> ballTopPoints;
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

		public int tileIndex;

		CBallField processedField;

		public List<Vector3> points = new List<Vector3>();

		//Debug
		public CBall(Vector3 pCenter)
		{
			center = pCenter;
		}

		/// <summary>
		/// Ball top is defined as an average from set of points in very close
		/// distance to the very first processed points.
		/// The Z value is kept as the very first point's Z value
		/// </summary>
		private void RefreshBallTop(Vector3 pPoint)
		{
			float origZ = ballTop.Z;
			float zDiff = origZ - pPoint.Z;
			if(zDiff < 0.02)
			{
				ballTopPoints.Add(pPoint);
				ballTop =
					new Vector3(ballTopPoints.Average(a => a.X),
						ballTopPoints.Average(a => a.Y),
						origZ
						);
			}
		}

		public CBall(List<Vector3> pPoints, bool pForce, CBallField pField)
		{
			processedField = pField;
			tileIndex = CProgramStarter.currentTileIndex;
			//sort descending => last point is the groudn point 
			//todo: maybe calculate from more points?
			pPoints.Sort((a, b) => b.Z.CompareTo(a.Z));

			//force detection - just DEBUG
			if(pForce)
			{
				isValid = true;
				ballTop = pPoints.First();
				UpdateFurthestPoints(ballTop);
				return;
			}

			//filter points that are not in expected height			
			//FilterPoints(ref pPoints, pPoints.Last(), pMinHeight, pMaxHeight);


			//todo: make balltop a list of points and get avg
			ballTop = pPoints[0];
			ballTopPoints = new List<Vector3>();


			furthestPoint2D = ballTop;
			furthestPointPlusX = ballTop;
			furthestPointMinusX = ballTop;
			furthestPointPlusY = ballTop;
			furthestPointMinusY = ballTop;

			float maxDist3D = GetMaxPointsDist(1);

			//check if points in expected ball extent break any criteria
			foreach(Vector3 point in pPoints)
			{
				float zDiff = ballTop.Z - point.Z;

				if(zDiff > GetMaxPointsDist(1))
					break;

				points.Add(point);
				RefreshBallTop(point);

				float dist3D = Vector3.Distance(point, ballTop);
				bool isInBallExtent = dist3D < maxDist3D;
				float dist2D = CUtils.Get2DDistance(point, ballTop);
				UpdateFurthestPoints(point);

				if(ballBot == null && dist2D < 0.01 && zDiff > GetMaxPointsDist(0) / 2)
				{
					ballBot = point;
				}

				//point is too far (2D) from the top but is cca in ball Z extent
				float max2dDist = GetMaxPointsDist(-3);
				if(dist2D > max2dDist && zDiff < GetMaxPointsDist(3) / 2)
				{
					SetValid(false);
					return;
				}

				if(!isInBallExtent)
				{
					bool isUnderBallTop = dist2D < 0.1f;
					if(!isUnderBallTop && zDiff < GetMaxPointsDist(3) / 2)
					{
						SetValid(false);
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
					SetValid(false);
					return;
				}
			}

			float furthestDist2D = GetFurthestPointDist2D();
			float maxDist = GetMaxPointsDist(-3) / 2;
			if(furthestDist2D < maxDist)
			{
				SetValid(false);
				return;
			}

			if(CDebug.IsDebugField(processedField))
				CDebug.WriteLine();

			isValid = HasValidMainPoints();

			//todo: validate based on point count? at least 1000?
			if(isValid)
			{
				isValid = points.Count > 300;
			}

			if(isValid)
			{
				//calculate center
				Vector3? bestCenter = CalculateCenterJS();
				bool check = bestCenter != null;

				if(check)
				{
					center = (Vector3)bestCenter;
					//check if distance to all points is valid
					check = CheckCenter(center);
				}

				if(!check)
				{
					SetValid(false);
					return;
				}
			}
		}

		/// <summary>
		/// Every point belonging to the ball needs to be in the same
		/// distance from the center = BALL_RADIUS
		/// </summary>
		//private void CheckPoints()
		//{
		//	foreach(Vector3 point in points)
		//	{
		//		float dist = Vector3.Distance(center, point);
		//		float diff = Math.Abs(BALL_RADIUS - dist);

		//		//todo: center should be re-approximated better if diff is too big
		//		if(diff > 4 * DIST_TOLLERANCE)
		//		{
		//			CDebug.Error("Some point in detected ball is too fart from calculatyed center! INVALIDATE");
		//			SetValid(false);
		//			return;
		//		}
		//	}
		//}

		/// <summary>
		/// Every point belonging to the ball needs to be in the same
		/// distance from the center = BALL_RADIUS.
		/// There is some disturbance involved so some tollerance
		/// needs to be allowed.
		/// </summary>
		private bool CheckCenter(Vector3 pCenter)
		{
			int highDiffCount = 0;
			float diffSum = 0;
			//float diffMax = int.MinValue;
			//float diffMin = int.MaxValue;
			foreach(Vector3 point in points)
			{
				float dist = Vector3.Distance(pCenter, point);
				float diff = Math.Abs(BALL_RADIUS - dist);

				if(diff > 4 * DIST_TOLLERANCE)
				{
					diffSum += diff;
					highDiffCount++;
				}

				//todo: center should be re-approximated better if diff is too big
				if(diff > 10 * DIST_TOLLERANCE || highDiffCount > 100)
				{
					return false;
				}

				/*if(diff > DIST_TOLLERANCE)
				{
					diffSum += diff;
					highdiffCount++;
				}
				if(diff > diffMax)
					diffMax = diff;
				if(diff < diffMin)
					diffMin = diff;*/
			}

			float avgDiff = diffSum / highDiffCount;

			return true;
		}

		private void SetValid(bool pValue)
		{
			isValid = pValue;
		}

		//private void FilterPoints(ref List<Vector3> pPoints, Vector3 pGroundPoint, float pMinHeight, float pMaxHeight)
		//{
		//	for(int i = pPoints.Count - 1; i >= 0; i--)
		//	{
		//		float diff = pPoints[i].Z - pGroundPoint.Z;
		//		if(diff < pMinHeight || diff > pMaxHeight)
		//			pPoints.RemoveAt(i);
		//	}
		//}

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

		public List<Vector3> GetMainPoints(bool pAddDebugLine = false)
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
			float dist = Vector3.Distance(ballTop, pPoint);
			return dist > DIST_TOLLERANCE && IsAtMainPointZDistance(pPoint);
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

			//if(diffY < DIST_TOLLERANCE)
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

			//if(diffX < DIST_TOLLERANCE)
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

		private float GetBallRadius(int pTolleranceMultiply = 0)
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
		private Vector3? CalculateCenter()
		{
			CDebug.WriteLine($"Ball = {this}");

			List<Vector3> possibleCenters = GetPossibleCenters();
			Vector3? bestCenter = SelectBestCenter(possibleCenters);
			if(bestCenter != null)
			{
				center = (Vector3)bestCenter;
				CDebug.WriteLine($"center = {center}");
				//return center;
			}
			else
			{
				return null;
			}

			//increase precision
			//todo: doesnt SelectBestCenter => FIX!
			//possibleCenters = GetPointsInRadius(center, 0.01f, 0.001f);
			//center = (Vector3)SelectBestCenter(possibleCenters);
			//CDebug.WriteLine($"new center = {center}");

			return center;
		}

		private Vector3? CalculateCenterJS()
		{
			//first try calculate center only from main points
			List<Vector3> mainPoints = GetMainPoints();
			CircumcentreSolver solver = CircumcentreSolver.Create(mainPoints);
			Vector3 center = new Vector3(
					(float)solver.Centre[0],
					(float)solver.Centre[1],
					(float)solver.Centre[2]);
			if(CheckCenter(center))
			{
				return center;
			}


			List<Vector3> aproximatedCenters = new List<Vector3>();
			for(int i = 0; i < 5; i++)
			{
				//random points are hard to debug
				//List<Vector3> randomBallPoints = GetRandomPoints(4, 5 * DIST_TOLLERANCE);
				//solver = CircumcentreSolver.Create(randomBallPoints);

				//get 4 points at different height and calculate their sphere center
				List<Vector3> evenlyDistributedPoints = GetEvenlyDistributedPoints(4, i * 10);
				solver = CircumcentreSolver.Create(evenlyDistributedPoints);

				center = new Vector3(
					(float)solver.Centre[0],
					(float)solver.Centre[1],
					(float)solver.Centre[2]);

				//sometimes calculated center is out of ball extent
				float distToTop = Vector3.Distance(ballTop, center);
				if(distToTop > BALL_DIAMETER)
				{
					CDebug.Error("Center calculated out of ball extent");
					continue;
				}

				aproximatedCenters.Add(center);
			}

			//use average of calculated centers
			Vector3 sumCenters = Vector3.Zero;
			foreach(Vector3 c in aproximatedCenters)
			{
				sumCenters += c;
			}
			Vector3 avgCenter = sumCenters / aproximatedCenters.Count;

			return avgCenter;
		}

		private List<Vector3> GetEvenlyDistributedPoints(int pCount, int pOffset)
		{
			List<Vector3> evenPoints = new List<Vector3>();
			int totalCount = points.Count;
			int step = totalCount / (pCount - 1);
			for(int i = 0; i < pCount; i++)
			{
				int index = i * step + pOffset;
				if(index >= totalCount)
					index -= 2 * pOffset;

				index = Math.Max(0, index);
				index = Math.Min(totalCount - 1, index);

				evenPoints.Add(points[index]);
			}
			return evenPoints;
		}

		private List<Vector3> GetRandomPoints(int pCount, float pMinDistance)
		{
			List<Vector3> randomPoints = new List<Vector3>();
			if(points.Count < pCount)
				return points;

			for(int i = 0; i < pCount; i++)
			{
				int randomIndex = new Random().Next(0, points.Count);
				Vector3 point = points[randomIndex];
				bool isTooClose = false;
				foreach(Vector3 randPoint in randomPoints)
				{
					float dist = Vector3.Distance(randPoint, point);
					if(dist < DIST_TOLLERANCE * 5)
					{
						isTooClose = true;
						i--;
						break;
					}
				}
				if(!isTooClose)
					randomPoints.Add(point);
			}

			return randomPoints;
		}

		/// <summary>
		/// Returns the center which has the most equal distance to all of the main points.
		/// </summary>
		private Vector3? SelectBestCenter(List<Vector3> pPossibleCenters)
		{
			Dictionary<float, Vector3> diffCeters = new Dictionary<float, Vector3>();
			List<Vector3> mainPoints = GetMainPoints(false);

			foreach(Vector3 possibleCenter in pPossibleCenters)
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

			int maxCheckCount = Math.Min(10, orderedDiffCenters.Count);
			for(int i = 0; i < maxCheckCount; i++)
			{
				Vector3 c = orderedDiffCenters.ElementAt(i).Value;
				if(CheckCenter(c))
					return c;

			}

			return null;

			Vector3 center = orderedDiffCenters.First().Value;
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
					for(float z = GetBallRadius(-5); z < GetBallRadius(5); z += step)
					{
						Vector3 possibleCenter = ballTop + new Vector3(x, y, -z);
						float dist = Vector3.Distance(ballTop, possibleCenter);
						if(dist < GetBallRadius(2))
						{
							centers.Add(possibleCenter);
						}
					}
				}
			}

			return centers;
		}

		private static List<Vector3> GetPointsInRadius(Vector3 pPoint, float pRadius, float pStep)
		{
			List<Vector3> centers = new List<Vector3>();

			for(float x = -pRadius; x < pRadius; x += pStep)
			{
				for(float y = -pRadius; y < pRadius; y += pStep)
				{
					for(float z = -pRadius; z < pRadius; z += pStep)
					{
						Vector3 possibleCenter = pPoint + new Vector3(x, y, z);
						float dist = Vector3.Distance(pPoint, possibleCenter);
						if(dist < pRadius)
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

		/// <summary>
		/// Generates points which are in radius distance from the center
		/// </summary>
		/// <returns></returns>
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
						if(Math.Abs(dist - GetBallRadius()) < step)
						{
							points.Add(point);
						}
					}
				}
			}
			return points;
		}

		public override string ToString()
		{
			return $"Ball[{isValid}], center = {center}, ballTop = {ballTop}, Tile = {tileIndex}";
		}
	}
}
