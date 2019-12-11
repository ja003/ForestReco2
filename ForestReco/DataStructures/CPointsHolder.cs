using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ForestReco
{
	/// <summary>
	/// Structure holding references to all added points.
	/// After all points are processed they are assigned to their corresponding array.
	/// </summary>
	public class CPointsHolder
	{
		//step sizes for arrays
		public const float NORMAL_STEP = 1f;
		public const float DETAIL_STEP = 0.2f;
		public const float DETAIL_STEP_BALLS = 0.15f; //ball diameter = 0.145

		public List<Vector3> unassigned = new List<Vector3>(); //1
		public List<Vector3> ground = new List<Vector3>(); //2
		public List<Vector3> vege = new List<Vector3>(); //5
		public List<Vector3> building = new List<Vector3>(); //6

		public List<Vector3> ballPoints = new List<Vector3>();
		public List<Vector3> ballsMainPoints = new List<Vector3>();
		public List<Vector3> ballsCenters = new List<Vector3>();
		public List<Vector3> ballsSurface = new List<Vector3>();
		public List<Vector3> arrayGrid = new List<Vector3>();


		//main arrays
		public CUnassignedArray unassignedArray;

		public CBallArray ballArray;
		public CBallArray ballDetailArray;
		public CGroundArray groundArray;
		public CVegeArray buildingArray;
		public CVegeArray vegeArray;
		public CVegeArray vegeDetailArray;

		public CVegeArray preprocessDetailArray;
		public CVegeArray preprocessNormalArray;

		public CTreeArray treeDetailArray;
		public CTreeArray treeNormalArray;

		public float lowestHeight { private set; get; }
		public float highestHeight { private set; get; }

		private static bool forceApplied = false;//debug

		public void ReInit()
		{
			lowestHeight = int.MaxValue;
			highestHeight = int.MinValue;

			groundArray = null;

			unassigned.Clear();
			ground.Clear();
			vege.Clear();
			building.Clear();

			InitArrays();

			forceApplied = false;
		}

		public List<Vector3> GetPoints(EClass pClass)
		{
			const int filter_frequency = 100; //0 = no filter

			switch(pClass)
			{
				case EClass.Unassigned:
					//points has been filtered out in fields only
					if(CTreeManager.GetDetectMethod() == EDetectionMethod.Balls)
					{
						return CUtils.GetPoints(ballDetailArray.GetPoints(), filter_frequency);
					}

					return unassigned;
				case EClass.FilteredOut:
					if(CTreeManager.GetDetectMethod() == EDetectionMethod.Balls)
						return CUtils.GetPoints(ballArray.GetFilteredOutPoints(), filter_frequency);
					return new List<Vector3>();

				case EClass.Balls:
					return ballPoints;
				case EClass.BallsMainPoints:
					return ballsMainPoints;
				case EClass.BallsCenters:
					return ballsCenters;
				case EClass.BallsSurface:
					return ballsSurface;
				case EClass.ArrayGrid:
					return arrayGrid;


				case EClass.Ground:
					if(CTreeManager.GetDetectMethod() == EDetectionMethod.Balls)
						return CUtils.GetPoints(ground, filter_frequency);
					return ground;
				case EClass.Vege:
					return vege;
				case EClass.Building:
					return building;
			}
			CDebug.Error($"Points of class {pClass} not defined");
			return null;
		}

		/// <summary>
		/// Classify each point into its according points list.
		/// Then adds it to its array.
		/// </summary>
		public bool AddPointsFromLines(List<Tuple<EClass, Vector3>> pParsedLines)
		{
			bool result = true;
			ClassifyPoints(pParsedLines);

			DateTime processStartTime = DateTime.Now;
			CDebug.Count("ProcessParsedLines", pParsedLines.Count);

			//TODO: unite processing all points

			//first we need to fill ground array
			CDebug.Step(EProgramStep.ProcessGroundPoints);
			if(!CRxpParser.IsRxp)
				ProcessGroundPoints();

			CDebug.Step(EProgramStep.ProcessUnassignedPoints);
			result = ProcessUnassignedPoints();
			CDebug.Step(EProgramStep.ProcessBuildingPoints);
			ProcessBuildingPoints();


			CDebug.Step(EProgramStep.PreprocessVegePoints);
			PreprocessVegePoints();
			CDebug.Step(EProgramStep.ProcessVegePoints);
			ProcessVegePoints();

			SetAnalytics();

			CDebug.Duration("All points added", processStartTime);
			return result;
		}

		private void ClassifyPoints(List<Tuple<EClass, Vector3>> pParsedLines)
		{
			int pointsToAddCount = pParsedLines.Count;
			//pointsToAddCount = 1000000;
			for(int i = 0; i < Math.Min(pParsedLines.Count, pointsToAddCount); i++)
			{
				Tuple<EClass, Vector3> parsedLine = pParsedLines[i];

				if(CRxpParser.IsRxp && parsedLine.Item1 != EClass.Ground)
					parsedLine = new Tuple<EClass, Vector3>(EClass.Unassigned, parsedLine.Item2);

				AddPoint(parsedLine);
			}
		}

		/// <summary>
		/// Returns field from array (of normal step size) of the given class on the given index
		/// </summary>
		public CField GetField(EClass pClass, int pX, int pY)
		{
			switch(pClass)
			{
				case EClass.Unassigned:
					return unassignedArray.GetField(pX, pY);
				case EClass.Ground:
					return groundArray.GetField(pX, pY);
				case EClass.Vege:
					return vegeArray.GetField(pX, pY);
				case EClass.Building:
					return buildingArray.GetField(pX, pY);
			}
			CDebug.Error($"array from class {pClass} not defined");
			return null;
		}

		//TODO: not practical since it can only return CField and the caller 
		// has to cast it to more specific type. Find out more general approach?
		//public CField GetFieldContaining(EArray pClass, bool pNormal, Vector3 pPoint)
		//{
		//	switch(pClass)
		//	{
		//		case EArray.Ground:
		//			if(!pNormal)
		//				break;
		//			return groundArray.GetFieldContainingPoint(pPoint);
		//		case EArray.Vege:
		//			if(pNormal)
		//				break;
		//			return vegeDetailArray.GetFieldContainingPoint(pPoint);
		//		case EArray.Tree:
		//			return pNormal ?
		//				treeNormalArray.GetFieldContainingPoint(pPoint) :
		//				treeDetailArray.GetFieldContainingPoint(pPoint);
		//	}
		//	CDebug.Error($"GetFieldContaining() for class {pClass} - {pNormal} not defined");
		//	return null;
		//}

		private void InitArrays()
		{
			unassignedArray = new CUnassignedArray(NORMAL_STEP, false);

			ballArray = new CBallArray(NORMAL_STEP, false);
			ballDetailArray = new CBallArray(DETAIL_STEP_BALLS, true);

			groundArray = new CGroundArray(NORMAL_STEP, false);
			buildingArray = new CVegeArray(NORMAL_STEP, false);
			vegeArray = new CVegeArray(NORMAL_STEP, false);

			vegeDetailArray = new CVegeArray(DETAIL_STEP, true);

			preprocessDetailArray = new CVegeArray(DETAIL_STEP, true);
			preprocessNormalArray = new CVegeArray(NORMAL_STEP, false);

			treeDetailArray = new CTreeArray(DETAIL_STEP, true);
			treeNormalArray = new CTreeArray(NORMAL_STEP, false);

			CObjPartition.Init();
		}

		private void AddPoint(Tuple<EClass, Vector3> pParsedLine)
		{
			Vector3 point = pParsedLine.Item2;

			switch(pParsedLine.Item1)
			{
				case EClass.Undefined:
				case EClass.Unassigned:
					unassigned.Add(point);
					break;
				case EClass.Ground:
					ground.Add(point);
					break;
				case EClass.Vege:
					vege.Add(point);
					break;
				case EClass.Building:
					building.Add(point);
					break;
				default:
					CDebug.Warning($"point class {pParsedLine.Item1} not recognized");
					return;
			}


			float height = point.Z;
			if(height < lowestHeight)
			{
				lowestHeight = height;
			}
			if(height > highestHeight)
			{
				highestHeight = height;
			}
		}

		private void SetAnalytics()
		{
			CAnalytics.unassignedPoints = unassigned.Count;
			CAnalytics.groundPoints = ground.Count;
			CAnalytics.vegePoints = vege.Count;
			CAnalytics.buildingPoints = building.Count;
		}

		/// <summary>
		/// Assigns all vege points in preprocess arrays.
		/// Then it calculates the expected average tree height.
		/// </summary>
		private void PreprocessVegePoints()
		{
			const int debugFrequency = 10000;

			DateTime PreprocessVegePointsStart = DateTime.Now;
			CDebug.WriteLine("PreprocessVegePoints", true);

			DateTime preprocessVegePointsStart = DateTime.Now;
			DateTime previousDebugStart = DateTime.Now;

			for(int i = 0; i < vege.Count; i++)
			{
				if(CProjectData.backgroundWorker.CancellationPending)
				{ return; }

				Vector3 point = vege[i];
				preprocessDetailArray.AddPointInField(point);
				preprocessNormalArray.AddPointInField(point);

				CDebug.Progress(i, vege.Count, debugFrequency, ref previousDebugStart, preprocessVegePointsStart, "preprocessed point");
			}

			//fill missing heigh - will be used in detection process
			//rank 2 - rank 1 (max) extends local maximas -> unwanted effect
			preprocessDetailArray.FillMissingHeights(2);
			preprocessDetailArray.FillMissingHeights(2);

			CDebug.Duration("PreprocessVegePoints", PreprocessVegePointsStart);

			//determine average tree height
			if(CParameterSetter.GetBoolSettings(ESettings.autoAverageTreeHeight))
			{
				//not valid anymore //why not valid??...seems to work fine
				CTreeManager.AVERAGE_TREE_HEIGHT = preprocessNormalArray.GetAverageZ();

				if(float.IsNaN(CTreeManager.AVERAGE_TREE_HEIGHT))
				{
					CDebug.Error("AVERAGE_TREE_HEIGHT = NaN. using input value");
					CTreeManager.AVERAGE_TREE_HEIGHT = CParameterSetter.GetIntSettings(ESettings.avgTreeHeigh);
				}
			}
			else
			{
				CTreeManager.AVERAGE_TREE_HEIGHT = CParameterSetter.GetIntSettings(ESettings.avgTreeHeigh);
			}
		}

		/// <summary>
		/// Add new tree to tree arrays
		/// </summary>
		public void OnTreeCreated(CTree pNewTree, Vector3 pFirstPoint)
		{
			CVegeField vegeField = vegeDetailArray.GetFieldContainingPoint(pFirstPoint);
			CTreeField treeDetailField = treeDetailArray.GetFieldContainingPoint(pFirstPoint);
			CTreeField treeNormalField = treeNormalArray.GetFieldContainingPoint(pFirstPoint);

			if(vegeField == null)
			{
				CDebug.Error($"Cant create tree. point {pFirstPoint} is OOB!");
				return;
			}

			treeDetailField.AddDetectedTree(pNewTree, true);
			treeNormalField.AddDetectedTree(pNewTree, true);

			pNewTree.peakDetailField = treeDetailField;
			pNewTree.peakNormalField = treeNormalField;
		}

		/// <summary>
		/// Assigns vege poins to trees. Handled in TreeManager
		/// </summary>
		private void ProcessVegePoints()
		{
			vege.Sort((b, a) => a.Z.CompareTo(b.Z)); //sort descending by height

			const int debugFrequency = 10000;

			DateTime processVegePointsStart = DateTime.Now;
			CDebug.WriteLine("ProcessVegePoints", true);

			DateTime previousDebugStart = DateTime.Now;


			int pointsToAddCount = vege.Count;
			//pointsToAddCount = 12000; 

			for(int i = 0; i < pointsToAddCount; i++)
			{
				if(CProjectData.backgroundWorker.CancellationPending)
					return;

				Vector3 point = vege[i];
				CTreeManager.AddPoint(point, i);

				CDebug.Progress(i, vege.Count, debugFrequency, ref previousDebugStart, processVegePointsStart, "added point");
			}
			CDebug.WriteLine("maxPossibleTreesAssignment = " + CTreeManager.maxPossibleTreesAssignment + " todo: investigate if too high");


			CAnalytics.processVegePointsDuration = CAnalytics.GetDuration(processVegePointsStart);
			CDebug.Duration("ProcessVegePoints", processVegePointsStart);
		}

		/// <summary>
		/// Assigns ground points into arrays (main and detailed for precess and later bitmap generation).
		/// Fills missing heights in the array and applies smoothing.
		/// </summary>
		private void ProcessGroundPoints()
		{
			//if(CRxpParser.IsRxp)
			//	return;

			for(int i = 0; i < ground.Count; i++)
			{
				if(CProjectData.backgroundWorker.CancellationPending)
				{ return; }

				Vector3 point = ground[i];
				groundArray.AddPointInField(point);
				//some points can be at border of detail array - not error -> dont log
				//detailArray?.AddPointInField(point, CGroundArray.EPointType.Ground, false);
			}

			if(groundArray == null)
			{
				CDebug.Error("No array defined");
				CDebug.WriteLine("setting height to " + lowestHeight);
				//CDebugData.DefineArray(true, lowestHeight);
			}

			groundArray.FillArray();

			groundArray?.SmoothenArray(1);
		}

		/// <summary>
		/// If detect method is BALLS it returns true if the ball was detected
		/// </summary>
		private bool ProcessUnassignedPoints()
		{
			bool result = true;
			DateTime debugStart = DateTime.Now;
			DateTime previousDebugStart = DateTime.Now;
			for(int i = 0; i < unassigned.Count; i++)
			{
				if(CProjectData.backgroundWorker.CancellationPending)
					return true;
				CDebug.Progress(i, unassigned.Count, 100000, ref previousDebugStart, debugStart, "ProcessUnassignedPoints");

				if(CProjectData.backgroundWorker.CancellationPending)
					return true;

				Vector3 point = unassigned[i];
				unassignedArray.AddPointInField(point);
				if(CTreeManager.GetDetectMethod() == EDetectionMethod.Balls)
				{
					ballArray.AddPointInField(point);
				}

			}

			if(CTreeManager.GetDetectMethod() == EDetectionMethod.Balls)
			{
				//unassignedArray.FillArray(); //doesnt make sense
				//ballArray.FillArray(); 

				//TODO: check that this approach works - seems fine now
				const bool filterBasedOnheight = true;
				// ===> ??? <===
				//we cant rely on ground to be filled everywhere
				//in many locations it is not defined at all
				//and processed tiles has to be pretty small in dense sets
				float minHeight = CParameterSetter.GetFloatSettings(ESettings.minBallHeight);
				float maxHeight = CParameterSetter.GetFloatSettings(ESettings.maxBallHeight);

				//balls are expected to be in this height above ground
				if(filterBasedOnheight)
				{
					ballArray.FilterPointsAtHeight(minHeight, maxHeight);
				}

				const bool filterBasedOnDistance = true;
				int minDistance = CParameterSetter.GetIntSettings(ESettings.minBallDistance);
				int maxDistance = CParameterSetter.GetIntSettings(ESettings.maxBallDistance);
				if(filterBasedOnDistance)
				{
					ballArray.FilterPointsAtDistance(minDistance, maxDistance);
				}


				//add filtered points to detail array
				List<Vector3> filteredPoints = ballArray.GetPoints();
				foreach(Vector3 point in filteredPoints)
				{
					if(CProjectData.backgroundWorker.CancellationPending)
						return true;

					ballDetailArray.AddPointInField(point);
				}

				//add filtered out points to detail array
				List<Vector3> filteredOutPoints = ballArray.GetFilteredOutPoints();
				foreach(Vector3 point in filteredOutPoints)
				{
					if(CProjectData.backgroundWorker.CancellationPending)
						return true;

					ballDetailArray.AddFilteredOutPointInField(point);
				}

				//this is actually not necessary
				//ballDetailArray.SortPoints();

				//array grid points - for result debug (increases file size)
				//arrayGrid = ballArray.GetArrayGridPoints();
				arrayGrid = ballDetailArray.GetArrayGridPoints();


				List<CBallField> ballFields = new List<CBallField>();

				//vege.Sort((b, a) => a.Z.CompareTo(b.Z)); //sort descending by height

				List<CBallField> sortedFields = ballDetailArray.fields;
				//sortedFields.Sort((a, b) => a.indexInField.Item1.CompareTo(b.indexInField.Item1));
				//sortedFields.Sort((a, b) => a.indexInField.Item2.CompareTo(b.indexInField.Item2));

				sortedFields.OrderBy(a => a.indexInField.Item1).ThenBy(a => a.indexInField.Item2);

				//List<Vector3> mainPoints = new List<Vector3>();

				//ballDetailArray.FilterPointsAtHeight


				debugStart = DateTime.Now;
				previousDebugStart = DateTime.Now;
				//process
				int count = ballDetailArray.GetPoints().Count;

				//result is false until a ball is detected
				result = false;
				for(int i = 0; i < sortedFields.Count; i++)
				{
					if(CProjectData.backgroundWorker.CancellationPending)
						return true;
					CDebug.Progress(i, sortedFields.Count, 100000, ref previousDebugStart, debugStart, "Detecting balls");

					CBallField field = sortedFields[i];
					//debug - apply force only once
					bool force = !forceApplied;
					force = false;
					//if(force || field.Equals(3,3))
					//{
					//	CDebug.WriteLine();
					//	force = true;
					//}


					CBall detectedBall = CBallsManager.Process(field, force);
					if(detectedBall != null && detectedBall.isValid)
					{
						forceApplied = true;
						ballFields.Add(field);
						ballsMainPoints.AddRange(detectedBall.GetMainPoints(true));

						ballsCenters.Add(detectedBall.center);
						ballsCenters.AddRange(CUtils.GetPointCross(detectedBall.center));
						result = true;
						//return;
						ballsSurface.AddRange(detectedBall.GetSurfacePoints());
					}

					//false positive:  -4,63   6,78    1,04(-5, -4; 6,8)
					bool debugField = CDebug.IsDebugField(field);
					if(debugField && false)
					{
						ballFields.Add(field);

						detectedBall = field.ball;
						if(detectedBall == null)
						{
							CDebug.Error("Nop ball detected in debug field");
						}
						else
						{
							ballFields.Add(field);
							ballsMainPoints.AddRange(detectedBall.GetMainPoints(true));

							ballsCenters.Add(detectedBall.center);
							ballsCenters.AddRange(CUtils.GetPointCross(detectedBall.center));

							ballsSurface.AddRange(detectedBall.GetSurfacePoints());
						}
					}

					/*if(count != ballDetailArray.GetPoints().Count)
					{
						CDebug.WriteLine();

					}*/
				}
				CDebug.WriteLine();

				foreach(CBallField field in ballFields)
				{
					if(field.ball != null)
						ballPoints.AddRange(field.ball.points);
					//ballPoints.AddRange(field.points);
				}

			}
			return result;
		}

		private void ProcessBuildingPoints()
		{
			for(int i = 0; i < building.Count; i++)
			{
				if(CProjectData.backgroundWorker.CancellationPending)
					return;

				Vector3 point = building[i];
				buildingArray.AddPointInField(point);
			}
		}
	}
}
