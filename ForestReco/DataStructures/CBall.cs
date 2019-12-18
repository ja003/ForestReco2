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

				//dont add points too far from top
				//there is a part of the ball-holder at the bottom of the ball
				//we dont want to add it to the processed points
				if(zDiff > GetMaxPointsDist(-1))
					break;

				points.Add(point);
				//we want to calculate center only from central set of points
				//since there might by some disturbance at the top or bottop of the ball
				if(zDiff > DIST_TOLLERANCE)
					centerCalculationPoints.Add(point);

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
			//todo: balls further from the center have much lower points density (eg. 300 points)
			//this condition cant be too restrictive
			if(isValid)
			{
				isValid = points.Count > 200;
			}

			if(isValid)
			{
				//calculate center
				Vector3? bestCenter = CalculateCenter();
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

		//points used for center calculation
		//these points shouldnt be too close to the very top or bottom of the ball
		private List<Vector3> centerCalculationPoints = new List<Vector3>();

		/// <summary>
		/// Getter method for precision comparison
		/// TODO: test. if ok => remove getter
		/// </summary>
		/// <returns></returns>
		public List<Vector3> GetCenterCalculationPoints()
		{
			const bool useAllPoints = false;
			return useAllPoints ? points : centerCalculationPoints;
		}

		internal string ToStringCenter()
		{
			return
				center.X.ToString() + "f, " +
				center.Y.ToString() + "f, " +
				center.Z.ToString() + "f";
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


		//keep statistics of ball center precision
		public float maxDiff = int.MinValue;
		public float minDiff = int.MaxValue;
		public float avgDiff;

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
			foreach(Vector3 point in GetCenterCalculationPoints())
			{
				float dist = Vector3.Distance(pCenter, point);
				float diff = Math.Abs(BALL_RADIUS - dist);

				if(diff > 4 * DIST_TOLLERANCE)
				{
					diffSum += diff;
					highDiffCount++;
				}

				//todo: center should be re-approximated better if diff is too big
				if(diff > 10 * DIST_TOLLERANCE || highDiffCount > 300)
				{
					return false;
				}

				diffSum += diff;

				if(diff > maxDiff)
					maxDiff = diff;
				if(diff < minDiff)
					minDiff = diff;
			}
			float highDiffPercentage = highDiffCount / GetCenterCalculationPoints().Count;
			if(highDiffPercentage > 0.2f)
				return false;

			avgDiff = diffSum / GetCenterCalculationPoints().Count;

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
		/// Calculates center from evenly distibuted 4 processed points
		/// Repeats process several times with different points and
		/// calculates their average.
		/// The precisioin of the approximated center is then iteratively increased.
		/// </summary>
		private Vector3? CalculateCenter()
		{
			List<Vector3> approximatedCenters = new List<Vector3>();

			//first try calculate center only from main points
			CircumcentreSolver solver;

			//center calculated from other points are more precise
			//and results from centers are usually evaluated as valid
			//todo: improve center validation
			bool useMainPoints = false;
			List<Vector3> mainPoints = GetMainPoints();
			if(mainPoints.Count < 4 || !useMainPoints)
				goto skipCenterApprox;

			//we ned only 4 points to calculate center
			//use all possible permutation (todo: lots of repeated calculations)
			IEnumerable<IEnumerable<Vector3>> mainPointsPermutations = mainPoints.Permute();
			foreach(var mainP in mainPointsPermutations)
			{
				List<Vector3> mainP4 = new List<Vector3>();
				mainP4.AddRange(mainP.Take(4));
				solver = CircumcentreSolver.Create(mainP4);
				Vector3 center = new Vector3(
					(float)solver.Centre[0],
					(float)solver.Centre[1],
					(float)solver.Centre[2]);

				approximatedCenters.Add(center);
			}
			Vector3 avgCenter = CUtils.GetAverage(approximatedCenters);
			if(CheckCenter(avgCenter))
				return avgCenter;

			skipCenterApprox: CDebug.WriteLine();

			approximatedCenters.Clear();


			for(int i = 0; i < 100; i++)
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
					CDebug.Warning("Center calculated out of ball extent");
					continue;
				}

				approximatedCenters.Add(center);
			}

			//use average of calculated centers
			avgCenter = CUtils.GetAverage(approximatedCenters);
			//increase center precision by removing approximated centers too far
			//from their average center
			for(int i = approximatedCenters.Count - 1; i >= 0; i--)
			{
				Vector3 c = approximatedCenters[i];
				float diff = Vector3.Distance(c, avgCenter);
				if(diff > 0.02f)
				{
					approximatedCenters.RemoveAt(i);
				}
				//CDebug.WriteLine("diff = " + diff);
			}

			avgCenter = CUtils.GetAverage(approximatedCenters);
			//repeat
			for(int i = approximatedCenters.Count - 1; i >= 0; i--)
			{
				Vector3 c = approximatedCenters[i];
				float diff = Vector3.Distance(c, avgCenter);
				if(diff > 0.01f)
				{
					approximatedCenters.RemoveAt(i);
				}
				//CDebug.WriteLine("diff = " + diff);
			}

			avgCenter = CUtils.GetAverage(approximatedCenters);

			return avgCenter;
		}

		private List<Vector3> GetEvenlyDistributedPoints(int pCount, int pOffset)
		{
			List<Vector3> evenPoints = new List<Vector3>();
			int totalCount = GetCenterCalculationPoints().Count;
			int step = totalCount / (pCount - 1);
			for(int i = 0; i < pCount; i++)
			{
				int index = i * step + pOffset;
				if(index >= totalCount)
					index -= 2 * pOffset;

				index = Math.Max(0, index);
				index = Math.Min(totalCount - 1, index);

				evenPoints.Add(GetCenterCalculationPoints()[index]);
			}
			return evenPoints;
		}

		private List<Vector3> GetRandomPoints(int pCount, float pMinDistance)
		{
			List<Vector3> randomPoints = new List<Vector3>();
			if(GetCenterCalculationPoints().Count < pCount)
				return randomPoints;

			for(int i = 0; i < pCount; i++)
			{
				int randomIndex = new Random().Next(0, GetCenterCalculationPoints().Count);
				Vector3 point = GetCenterCalculationPoints()[randomIndex];
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
			//return $"Ball[{isValid}], center = {center}, ballTop = {ballTop}, Tile = {tileIndex}";
			return $"Ball ({points.Count}), center = {center}, avgDiff = {avgDiff}, minDiff = {minDiff}, maxDiff = {maxDiff}";
		}
	}
}
