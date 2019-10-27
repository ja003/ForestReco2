using ObjParser;
using ObjParser.Types;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	/// <summary>
	/// Z = height
	/// </summary>
	public class CTree : CBoundingBoxObject
	{
		public CPeak peak;
		protected List<CBranch> branches = new List<CBranch>();
		public List<CBranch> Branches => branches;
		public CBranch stem { get; protected set; }
		public float treePointExtent = CTreeManager.TREE_POINT_EXTENT; //default if not set

		public static int BRANCH_ANGLE_STEP = 45;
		private const float MAX_STEM_POINT_DISTANCE = 0.1f;

		public Vector3 possibleNewPoint;

		public int treeIndex { get; protected set; }

		public List<Vector3> Points = new List<Vector3>();

		public CRefTree assignedRefTree;
		public Obj assignedRefTreeObj;



		//public string RefTreeTypeName;

		public Material assignedMaterial;

		public bool isValid; //invalid by default - until Validate is called
		public bool notTree; //if true then this tree was created from unfiltered noise 
		public bool tooLow; //tree is lower than minimal required height

		private float? groundHeight;

		public CTreeField peakDetailField;
		public CTreeField peakNormalField;

		//all field on which this point has points
		public List<CTreeField> detailFields = new List<CTreeField>();
		public List<CTreeField> normalFields = new List<CTreeField>();
		/// <summary>
		/// pDetail = false => normal
		/// </summary>
		public void AddField(CTreeField pTreeField)
		{
			if(pTreeField.IsDetail)
			{
				if(!detailFields.Contains(pTreeField))
					detailFields.Add(pTreeField);
			}
			else
			{
				if(!normalFields.Contains(pTreeField))
					normalFields.Add(pTreeField);
			}
		}

		//INIT

		public CTree() { }

		public CTree(Vector3 pPoint, int pTreeIndex, float pTreePointExtent) : base(pPoint)
		{
			Init(pPoint, pTreeIndex, pTreePointExtent);
		}

		protected void Init(Vector3 pPoint, int pTreeIndex, float pTreePointExtent)
		{
			treePointExtent = pTreePointExtent;
			peak = new CPeak(pPoint, treePointExtent);

			if(CTreeManager.DEBUG)
			{ CDebug.WriteLine("new tree " + pTreeIndex); }

			treeIndex = pTreeIndex;
			for(int i = 0; i < 360; i += BRANCH_ANGLE_STEP)
			{
				branches.Add(new CBranch(this, i, i + BRANCH_ANGLE_STEP));
			}

			stem = new CBranch(this, 0, 0);

			AddPoint(pPoint);
		}

		public void AssignMaterial()
		{
			assignedMaterial = CMaterialManager.GetTreeMaterial(this);
			//CDebug.WriteLine($"{this} color = {assignedMaterial}");
		}

		//MOST IMPORTANT

		/// <summary>
		/// Used only in reftree
		/// </summary>
		public void Process()
		{
			OnProcess();
		}

		protected virtual void OnProcess()
		{
			//CDebug.WriteLine("OnProcess");
		}

		/// <summary>
		/// pMerging is used during merging process
		/// </summary>
		public float GetAddPointFactor(Vector3 pPoint, CTree pTreeToMerge = null)
		{
			if(IsNewPeak(pPoint))
				return 1;

			bool merging = pTreeToMerge != null;

			CBranch branchForPoint = GetBranchFor(pPoint);

			float distToPeak = CUtils.Get2DDistance(pPoint, peak.Center);
			if(!merging)
			{
				if(CParameterSetter.treeExtent < distToPeak)
				{
					return 0;
				}
			}

			else if(pTreeToMerge.isValid)
			{
				float peakPointDist = CUtils.Get2DDistance(pPoint, peak.Center);
				if(peakPointDist > GetTreeExtentFor(pPoint, CParameterSetter.treeExtentMultiply))
				{
					return 0;
				}
			}

			float bestFactor = 0;

			float branchFactor = branchForPoint.GetAddPointFactor(pPoint, true, pTreeToMerge);
			if(branchFactor > bestFactor)
			{ bestFactor = branchFactor; }
			if(bestFactor > 0.9f)
			{ return bestFactor; }

			branchFactor = branchForPoint.GetNeigbourBranch(1).GetAddPointFactor(pPoint, false, pTreeToMerge);
			if(branchFactor > bestFactor)
			{ bestFactor = branchFactor; }
			if(bestFactor > 0.9f)
			{ return bestFactor; }

			branchFactor = branchForPoint.GetNeigbourBranch(-1).GetAddPointFactor(pPoint, false, pTreeToMerge);
			if(branchFactor > bestFactor)
			{ bestFactor = branchFactor; }
			if(bestFactor > 0.9f)
			{ return bestFactor; }

			return bestFactor;
		}

		private float GetMaxDistFromNewPointToDefinedField()
		{
			const float minTreeHeight = 10;
			const float minDist = 1;
			const float maxTreeHeight = 40;
			const float maxDist = 3;
			float height = GetTreeHeight();
			float dist = minDist + ((height - minTreeHeight) / (maxTreeHeight - minTreeHeight)) * (maxDist - minDist);
			return dist;
		}

		public bool CanAdd(Vector3 pPoint, bool pRestrictive)
		{
			CTreeField treeFieldWithPoint =
				CProjectData.Points.treeNormalArray.GetFieldContainingPoint(pPoint);
			CTreeField detailTreeFieldWithPoint =
				CProjectData.Points.treeDetailArray.GetFieldContainingPoint(pPoint);

			if(peakDetailField.Equals(detailTreeFieldWithPoint))
			{
				return true;
			}

			//if(peakNormalField.Equals(treeFieldWithPoint))
			//{
			//	List<CTree> treesInHood = detailTreeFieldWithPoint.GetDetectedTreesFromNeighbourhood();
			//	if(treesInHood.Contains(this))
			//		return true;
			//}
			List<CTree> treesInHood = detailTreeFieldWithPoint.GetDetectedTreesFromNeighbourhood();
			if(treesInHood.Contains(this))
				return true;


			//float maxExtent = CTreeRadiusCalculator.GetTreeRadius(GetTreeHeight());
			//float heightDiffToPeak = peak.Center.Z - pPoint.Z;
			//if(!IsInTreeExtent(pPoint) && heightDiffToPeak < maxExtent)
			//{
			//	return false;
			//}



			//if(treeFieldWithPoint.DetectedTrees.Count > 0)
			//{
			//	bool containsTree = treeFieldWithPoint.DetectedTrees.Contains(this);
			//	bool pointIsTreePeak = treeFieldWithPoint.GetSingleDetectedTree().peak.CenterEquals(pPoint);
			//	if(!pointIsTreePeak)
			//		return containsTree;
			//}

			//prevent adding of point in the peak level to branches
			/*float angle = CUtils.GetAngleToTree(this, pPoint);
			bool belongsToPeak = BelongsToPeak(pPoint);
			if(belongsToPeak)
				return true;
			else if(angle > 60)
				return false;
			*/

			CBranch branch = GetBranchFor(pPoint);
			bool isPointInExtent = branch.IsPointInExtent(pPoint);
			if(isPointInExtent)
				return true;

			/*if(pRestrictive)
			{
				Vector3 closestBranchPoint = branch.GetClosestHigherTo(pPoint);
				float distToClosePoint = Vector3.Distance(pPoint, closestBranchPoint);
				bool isSomePointClose = distToClosePoint < 1;
				if(!isSomePointClose)
					return false;
			}*/


			//TODO: tree 9, 16, 17 se nemergne, ale jsou to špičky...buď tot ak prostě nechám, nebo je třeba přidat další kritérium. případně další merge.
			//Prvně otestovat na další sadě

			//todo: or fields?
			//get the closest field to pPoint containing this tree
			CTreeField closeField = GetClosestField(pPoint);
			//if there is valid path to it => pPoint can be added
			bool canReachCloseField = CanReach(pPoint, closeField.Center, pRestrictive);
			if(canReachCloseField)
				return true;

			//same with the tree peak
			bool canReachPeak = CanReach(pPoint, peak.Center, pRestrictive);
			if(canReachPeak)
				return true;

			return false;

			//todo: moved to CanReach method, test and remove
			const float max_path_length = 2f;
			const float allowed_descend = 0.3f;
			const int min_ascending_steps = 4;
			const int min_descending_steps = 2;
			float ignoredDescend = GetTreeHeight() / 2;
			bool isAscending = CProjectData.Points.preprocessDetailArray.
				IsPathAscending(
					pPoint, peak.Center,
					max_path_length, allowed_descend, min_ascending_steps,
					min_descending_steps, ignoredDescend, this);

			const int max_path_length_to_tree = 1;
			bool pathContainsTree = CProjectData.Points.treeDetailArray.PathContainsTree(pPoint, peak.Center, this, max_path_length_to_tree);

			return isAscending && pathContainsTree;


			EDirection dirToTreePeak = CField.GetDirection(treeFieldWithPoint, peakNormalField);
			//neighbourInTreeDir has to be defined!
			CTreeField neighbourInTreeDir = (CTreeField)treeFieldWithPoint.GetNeighbour(dirToTreePeak);
			float distToNeighbour = CUtils.Get2DDistance(treeFieldWithPoint, neighbourInTreeDir);

			if(neighbourInTreeDir == null)
			{
				CDebug.Error("neighbour is null");
				return false;
			}
			bool neigbourBelongsToTheTree = neighbourInTreeDir.DetectedTrees.Contains(this);


			//const float max_dist_to_defined_field = 1f; //todo make this dependent on tree height
			float max_dist_to_defined_field = GetMaxDistFromNewPointToDefinedField();
			while(!neigbourBelongsToTheTree && distToNeighbour < max_dist_to_defined_field)
			{
				dirToTreePeak = CField.GetDirection(neighbourInTreeDir, peakNormalField);
				CTreeField newNeighbour = (CTreeField)neighbourInTreeDir.GetNeighbour(dirToTreePeak);

				if(dirToTreePeak == EDirection.None)
				{
					CDebug.Error("Direction cant be none!");
				}
				if(treeFieldWithPoint == null || newNeighbour == null)
				{
					CDebug.Error($"Wrong direction calculated! {neighbourInTreeDir} to {peakNormalField} ?= {dirToTreePeak}");
				}

				float newDist = CUtils.Get2DDistance(treeFieldWithPoint, newNeighbour);
				if(newDist < distToNeighbour)
				{
					CDebug.Error("!");
				}
				distToNeighbour = newDist;
				neighbourInTreeDir = newNeighbour;
				neigbourBelongsToTheTree = neighbourInTreeDir.DetectedTrees.Contains(this);

				List<CField> perpendicularNeighbour = neighbourInTreeDir.GetPerpendicularNeighbours(dirToTreePeak);
				foreach(CTreeField treeField in perpendicularNeighbour)
				{
					if(treeField.DetectedTrees.Contains(this))
					{
						neigbourBelongsToTheTree = true;
						break;
					}
				}
			}

			/*
			////detect if the way from the point to the tree is ascending
			////...or maybe this should be the main criteria???
			//TODO: not very effective so far
			float distance = CUtils.Get2DDistance(pPoint, peak.Center);
			if(neigbourBelongsToTheTree && distance > 2) //todo: get radius value
			{
				//bool isAscending = CProjectData.preprocessArray.IsWayAscending(pPoint, peak.Center, 0.1f);
				bool isAscending = CProjectData.Points.preprocessNormalArray.IsPathAscending(pPoint, peak.Center, 1.6f, 0.3f, 1); //todo: make consts
				return isAscending;
			}

			return neigbourBelongsToTheTree;*/
		}

		/// <summary>
		/// The path from pPoint to pTarget in detail array is ascending
		/// and containg this tree
		/// </summary>
		private bool CanReach(Vector3 pPoint, Vector3 pTarget, bool pHasToBeAscending)
		{
			//const float max_path_length = 2f;
			//const float allowed_descend = 0.5f;
			//const int min_ascending_steps = 4;
			//const int min_descending_steps = 3;

			float max_path_length = 2f;
			max_path_length = CTreeRadiusCalculator.GetTreeRadius(this);

			//these params dont have much of effect, most is decided by isLocalMax
			float allowed_descend = CTreeManager.AllowedDescend;
			int min_ascending_steps = CTreeManager.MinAscendSteps;
			int min_descending_steps = CTreeManager.MinDescendSteps;
			//todo: maybe no need to pass as params?
			float ignoredDescend = GetTreeHeight() / 2;
			bool isAscending = CProjectData.Points.preprocessDetailArray.
				IsPathAscending(
					pPoint, pTarget,
					max_path_length, allowed_descend, min_ascending_steps,
					min_descending_steps, ignoredDescend, this);

			if(pHasToBeAscending && !isAscending)
				return false;

			float max_path_length_to_tree = max_path_length;
			bool pathContainsTree = CProjectData.Points.treeDetailArray.PathContainsTree(pPoint, pTarget, this, max_path_length_to_tree);

			return pathContainsTree;
		}

		/// <summary>
		/// Finds the closest field to pPoint containing this tree
		/// </summary>
		public CTreeField GetClosestField(Vector3 pPoint)
		{
			List<CTreeField> fields = detailFields;
			fields.Sort((a, b) => CUtils.Get2DDistance(pPoint, a.Center).CompareTo(CUtils.Get2DDistance(pPoint, b.Center)));
			return fields.Count > 0 ? fields[0] : null;
		}

		private bool IsInTreeExtent(Vector3 pPoint)
		{
			float distToPeak2D = CUtils.Get2DDistance(pPoint, peak.Center);
			float maxExtent = CTreeRadiusCalculator.GetTreeRadius(GetTreeHeight());
			return distToPeak2D < maxExtent;
		}


		/// <summary>
		/// search in normal array seems better
		/// </summary>
		/*public bool CanAdd_detailArray(Vector3 pPoint)
		{			
			//CVegeField elemenWithPointPreprocess = CProjectData.preprocessArray.GetFieldContainingPoint(pPoint);
			CTreeField treeFieldWithPoint = CProjectData.Points.treeDetailArray.GetFieldContainingPoint(pPoint);
			if(treeFieldWithPoint.GetSingleDetectedTree() != null)
			{
				return treeFieldWithPoint.GetSingleDetectedTree().Equals(this);
			}
			if(Vector3.Distance(pPoint, new Vector3(802.701f, 1299.346f, 180.733f)) < 0.1f)
			{
				CDebug.WriteLine("!");
			}

			EDirection dirToTreePeak = CField.GetDirection(treeFieldWithPoint, treeDetailField);
			//neighbourInTreeDir has to be defined!
			CTreeField neighbourInTreeDir = (CTreeField)treeFieldWithPoint.GetNeighbour(dirToTreePeak);
			float distToNeighbour = CUtils.Get2DDistance(treeFieldWithPoint, neighbourInTreeDir);

			bool neigbourBelongsToTheTree = neighbourInTreeDir.GetSingleDetectedTree() != null &&
				neighbourInTreeDir.GetSingleDetectedTree().Equals(this);

			//TODO: search for field in area (use treeNormalArray)

			const float max_dist_to_defined_field = 1f; //todo make this dependent on tree height
			while(!neigbourBelongsToTheTree && distToNeighbour < max_dist_to_defined_field)
			{
				dirToTreePeak = CField.GetDirection(neighbourInTreeDir, treeDetailField);
				CTreeField newNeighbour = (CTreeField)neighbourInTreeDir.GetNeighbour(dirToTreePeak);
				

				float newDist = CUtils.Get2DDistance(treeFieldWithPoint, newNeighbour);
				if(newDist < distToNeighbour)
				{
					CDebug.Error("!");
				}
				distToNeighbour = newDist;
				neighbourInTreeDir = newNeighbour;
				neigbourBelongsToTheTree = neighbourInTreeDir.GetSingleDetectedTree() != null &&
					neighbourInTreeDir.GetSingleDetectedTree().Equals(this);
			}

			//heightDiff is always >= 0 or null
			//float? heightDiff = neighbourInTreeDir.MaxZ - pPoint.Z;
			
			return neigbourBelongsToTheTree;
		}*/

		public void AddPoint(Vector3 pPoint, int pPointIndex = -1)
		{
			if(Equals(72))
				pPoint = pPoint;

			if(CDebug.IsDebugPoint3D(pPoint))
				CDebug.WriteLine();

			//the tree might have become 'not-tree' (eg during merging)
			//try add the point again, this tree won't be selected again
			if(notTree)
			{
				CTreeManager.AddPoint(pPoint, pPointIndex);
				return;
			}

			//if this tree is currently being merged into another tree add this 
			//point into the target tree. see 'mergingInto' comment
			if(mergingInto != null)
			{
				//CDebug.WriteLine($"{this} is being merged into {mergingInto}");
				mergingInto.AddPoint(pPoint, pPointIndex);
				return;
			}

			bool belongsToPeak = BelongsToPeak(pPoint);
			if(belongsToPeak)
			{
				//if(Equals(72))
				//	pPoint = pPoint;
				peak.AddPoint(pPoint);
			}
			else
			{
				GetBranchFor(pPoint).AddPoint(pPoint);
			}
			CTreeField treeDetailField = CProjectData.Points.treeDetailArray.GetFieldContainingPoint(pPoint);
			treeDetailField.AddDetectedTree(this, false);
			CTreeField treeNormalField = CProjectData.Points.treeNormalArray.GetFieldContainingPoint(pPoint);
			treeNormalField.AddDetectedTree(this, false);

			Points.Add(pPoint);
			OnAddPoint(pPoint);

			//CTreeManager.allPoints.Add(pPoint);
			if(!belongsToPeak && !firstNonPeakPoint)
				OnAddFirstNonPeakPoint(pPoint);
		}

		/// <summary>
		/// If this point would be added to the peak
		/// </summary>
		private bool BelongsToPeak(Vector3 pPoint)
		{
			return peak.Includes(pPoint) || pPoint.Z > peak.minBB.Z;
		}

		private bool firstNonPeakPoint;

		/// <summary>
		/// After first non peak point is added we need to check if newly created tree is valid.
		/// With fully formed peak we can try to add this peak to "possible" trees to which the peak points 
		/// would not be previously assigned.
		/// </summary>
		private void OnAddFirstNonPeakPoint(Vector3 pPoint)
		{
			//return;
			//firstNonPeakPoint = true;

			//if(!Equals(8))
			//{
			//	firstNonPeakPoint = true;
			//	return;
			//}
			//else
			//	CDebug.WriteLine();

			if(Equals(31))
			{
				CDebug.WriteLine();
			}

			float heightDiffToPeak = peak.Center.Z - pPoint.Z;
			//if the first point is too far from the peak then those processed points are
			//probably unfiltered noise
			//todo: even some normal trees have big heightDiffToPeak => the original peak is then 
			//considered not-tree [ERROR] 
			if(!CTreeManager.IsMerging && heightDiffToPeak > 3 * CPeak.MAX_PEAK_Y_DIFF)
			{
				if(Equals(335))
					CDebug.WriteLine();

				Points.Remove(pPoint); //shouldnt be expensive since it is the first nonpeak
				branches.Clear();
				CTreeManager.MarkAsNotTree(this);
				CTreeManager.AddPoint(pPoint, CTreeManager.currentPointIndex);
				return;
			}

			firstNonPeakPoint = true;

			if(CTreeManager.GetDetectMethod() == EDetectionMethod.AddFactor)
				return;


			bool isLocalMaximum = IsLocalMaximum();
			if(isLocalMaximum)
				return;

			//try add the newly formed tree peak to some close possible trees.
			//if it fits delete this tree and merge it into the best possible.
			//this prevents the situation when some point from newly formed peak is too far to be added to some possible tree
			//but later added points to the peak moves the peak center closer to the possible tree and now should be merged
			List<CTree> possibleTrees = CTreeManager.GetPossibleTreesFor(
				peak.Center, EPossibleTreesMethod.ClosestHigher, false, this);
			CTree selectedTree = CTreeManager.SelectBestPossibleTree(
				possibleTrees, peak.Center, false);
			if(selectedTree != null)
			{
				CTreeManager.MergeTrees(selectedTree, this);
				return;
			}

			return;


			//every tree peak should be local maximum
			//if it is not better merge it into the closest possible tree
			if(!isLocalMaximum && possibleTrees.Count > 0)
			{
				//todo: na ANE2k_cut.las se 194  mergne do 189...přitom tam vůbec nesedí.
				//1) je potřeba ještě nějaký dodatečný check...třeba CanAdd s volnějším parametrem
				//2) ClosestOrHigher vrací closest podle vzdálenosti k peaku...možná by 
				//bylo lepší podle vzdálenosti k nejbližšímu fieldu, obsahujícímu daný strom?
				if(treeIndex == 194)
				{
					CDebug.WriteLine();
					possibleTrees = CTreeManager.GetPossibleTreesFor(peak.Center, EPossibleTreesMethod.ClosestHigher, false, this);
				}
				CTreeManager.MergeTrees(possibleTrees[0], this);
				return;
			}
		}

		public float GetAverageExtent()
		{
			float extentSum = 0;
			int defined = 0;
			foreach(CBranch branch in branches)
			{
				if(branch.furthestPointDistance > 0.2f)
				{
					extentSum += branch.furthestPointDistance;
					defined++;
				}
			}
			if(defined == 0)
				return 0;
			return extentSum / defined;
		}

		/// <summary>
		/// If current peak is actually local maximum.
		/// Checks all closest neighbours which do not contain any of peak points
		/// - peak has to be higher than all of them 
		/// </summary>
		public bool IsLocalMaximum()
		{
			//CVegeField peakCenterField = CProjectData.Points.preprocessNormalArray.GetFieldContainingPoint(peak.Center);
			CVegeField peakCenterField =
				CProjectData.Points.preprocessDetailArray.GetFieldContainingPoint(peak.Center);

			var directions = Enum.GetValues(typeof(EDirection));

			if(Equals(17))
			{
				CDebug.WriteLine();
			}
			int higherCount = 0;
			foreach(EDirection dir in directions)
			{
				if(dir == EDirection.None)
					continue;

				CVegeField closestNeighbourField =
					(CVegeField)peakCenterField.GetClosestNeighbourWithout(peak.Points, dir);

				if(closestNeighbourField == null)
					continue;

				//CTreeField debugF = CProjectData.Points.treeNormalArray.GetElement(closestNeighbourField.indexInField);

				float maxDistanceToNeighbour = 2 * CPointsHolder.NORMAL_STEP;
				float distanceToNeighbour = closestNeighbourField.GetDistanceTo(peakCenterField);
				if(distanceToNeighbour > maxDistanceToNeighbour)
					continue;

				float? heightDiff = peak.maxHeight.Z - closestNeighbourField.MaxZ;
				//if(heightDiff == null || heightDiff > 2)
				//{
				//	float? neighbAvg = closestNeighbourField.GetAverageHeightFromNeighbourhood(1);
				//	heightDiff = peak.maxHeight.Z - neighbAvg;
				//}

				//if heights are very similar then they were probably affected by max filter
				if(heightDiff > 0.1f)
				{
					if(heightDiff < CTreeManager.LocalMaxHeight)
						return false;
					else
						higherCount++;
				}
			}

			return higherCount > 2;
		}

		//if not null then this tree is currently being merged into another tree.
		//this is for the situation, when tree A merges into B and B starts to merge into C
		//before all points from A are added to B. This way all points from A are added to C
		public CTree mergingInto;

		public void MergeWith(CTree pSubTree)
		{
			int debugIndex = -1;
			if(CTreeManager.DEBUG || Equals(debugIndex) || pSubTree.Equals(debugIndex))
			{
				CDebug.WriteLine(this.ToString(EDebug.Peak) + " MergeWith " + pSubTree.ToString(EDebug.Peak));
			}

			if(pSubTree.Equals(this))
			{
				CDebug.Error("cant merge with itself.");
				return;
			}
			pSubTree.mergingInto = this;

			foreach(Vector3 point in pSubTree.Points)
			{
				//when merging 'not-tree' some points of this tree can be on fields where other tree
				//has been detected
				if(pSubTree.notTree)
				{
					CTree t = CProjectData.Points.treeDetailArray.GetFieldContainingPoint(point).GetSingleDetectedTree();
					if(t != null)
					{
						t.AddPoint(point);
						continue;
					}
				}
				//the tree can become 'not-tree' during merging
				if(notTree)
				{
					CTreeManager.AddPoint(point, -1);
					continue;
				}

				AddPoint(point);
			}
		}

		//GETTERS

		/// <summary>
		/// Returns a branch with biggest number of tree points
		/// </summary>
		/// <returns></returns>
		public CBranch GetMostDefinedBranch()
		{
			CBranch mostDefinedBranch = branches[0];
			foreach(CBranch b in branches)
			{
				if(b.TreePoints.Count > mostDefinedBranch.TreePoints.Count)
				{
					mostDefinedBranch = b;
				}
			}
			return mostDefinedBranch;
		}

		/// <summary>
		/// Calculates the area of the tree projection on the 2D plane.
		/// - based on furthest points of each branch
		/// </summary>
		public float GetArea()
		{
			float area = 0;
			for(int i = 0; i < branches.Count; i++)
			{
				CBranch nextBranch = branches[(i + 1) % branches.Count];
				Vector3 p1 = peak.Center;
				Vector3 p2 = branches[i].furthestPoint;
				Vector3 p3 = nextBranch.furthestPoint;
				area += CUtils.GetArea(p1, p2, p3);
			}
			return area;
		}

		public float GetDistanceTo(Vector3 pPoint)
		{
			float distToPeak = CUtils.Get2DDistance(pPoint, peak.Center);

			float distanceToAnyPoint = int.MaxValue;
			foreach(Vector3 p in Points)
			{
				float dist = Vector3.Distance(p, pPoint);
				if(dist < distanceToAnyPoint)
				{
					distanceToAnyPoint = dist;
				}
			}

			return Math.Min(distToPeak, distanceToAnyPoint);
		}

		public virtual float GetTreeHeight()
		{
			float treeHeight = maxBB.Z - GetGroundHeight();
			return treeHeight;
		}

		public static float GetMaxBranchAngle(Vector3 pPeakPoint, Vector3 pNewPoint)
		{
			//float distance = Vector3.Distance(pPeakPoint, pNewPoint);
			float heightDiff = pPeakPoint.Z - pNewPoint.Z;
			float distance = CUtils.Get2DDistance(pPeakPoint, pNewPoint);
			const float HEIGHT_DIFF_STEP = 0.2f;
			const float DIST_STEP = 0.15f;
			const float ANGLE_STEP = 2.5f;
			const int MAX_ANGLE = 100;
			if(heightDiff < 0.5f)
			{
				return MAX_ANGLE;
			}

			float maxAngle = MAX_ANGLE - ANGLE_STEP * heightDiff / HEIGHT_DIFF_STEP;
			maxAngle -= ANGLE_STEP * distance / DIST_STEP;

			return Math.Max(CTreeManager.MAX_BRANCH_ANGLE, maxAngle);
		}

		private CBranch GetBranchFor(Vector3 pPoint)
		{
			if(peak.maxHeight.Z < pPoint.Z)
			{
				CDebug.Error(pPoint + " is higher than peak " + peak);
			}
			Vector2 peak2D = new Vector2(peak.X, peak.Y);
			Vector2 point2D = new Vector2(pPoint.X, pPoint.Y);
			Vector2 dir = point2D - peak2D;
			if(dir.Length() < MAX_STEM_POINT_DISTANCE)
			{
				if(CTreeManager.DEBUG)
					CDebug.WriteLine("- branch = stem");
				//return branches[branches.Count - 1]; //stem
				return stem; //stem
			}

			dir = Vector2.Normalize(dir);
			float angle = CUtils.GetAngle(Vector2.UnitX, dir);
			//if (CTreeManager.DEBUG) CDebug.WriteLine("angle " + peak2D + " - " + point2D + " = " + angle);
			if(angle < 0)
			{
				angle = 360 + angle;
			}
			int branchIndex = (int)(angle / BRANCH_ANGLE_STEP);
			if(branchIndex < 0 || branchIndex >= branches.Count)
			{
				CDebug.Error("branchIndex = " + branchIndex);
				branchIndex = 0;
			}
			//CDebug.WriteLine(pPoint + " goes to branch " + branchIndex + ". angle = " + angle);
			return branches[branchIndex];
		}


		/// <summary>
		/// Returns height of ground under peak of this tree
		/// </summary>
		public float GetGroundHeight()
		{
			if(groundHeight != null)
			{
				return (float)groundHeight;
			}
			groundHeight = CProjectData.Points.groundArray?.GetHeightAtPoint(peak.Center);
			return groundHeight ?? peak.Center.Z;
		}

		public List<CTreePoint> GetAllPoints()
		{
			List<CTreePoint> allPoints = new List<CTreePoint>();
			allPoints.Add(peak);
			foreach(CBranch b in branches)
			{
				foreach(CTreePoint p in b.TreePoints)
				{
					allPoints.Add(p);
				}
			}
			return allPoints;
		}

		private int GetBranchesCount()
		{
			int count = 0;
			foreach(CBranch b in branches)
			{
				if(b.TreePoints.Count > 0)
				{ count++; }
			}
			return count;
		}

		private int GetBranchesPointCount()
		{
			int count = 0;
			foreach(CBranch b in branches)
			{
				count += b.GetPointCount();
			}
			return count;
		}

		public Vector3 GetGroundPosition()
		{
			Vector3 gp = new Vector3(peak.X, GetGroundHeight(), peak.Z);
			return gp;
		}

		public string GetObjName()
		{
			string prefix = isValid ? "tree_" : "invalidTree_";
			prefix = notTree ? "notTree_" : prefix;

			string suffix = "";
			if(!isValid)
			{
				if(isAtBorder)
					suffix += "(border)";
				if(isAtBufferZone)
					suffix += "(buffer)";
				if(tooLow)
					suffix += "(too_low)";
			}
			return prefix + treeIndex + suffix;
		}

		public Obj GetObj(bool pExportBranches, bool pExportPoints, bool pExportSimple)
		{
			//if (CTreeManager.DEBUG) CDebug.WriteLine("GetObj " + pName);

			Obj obj = new Obj(GetObjName());

			obj.UseMtl = assignedMaterial?.Name; //assignedMaterial can be null

			List<CTreePoint> allTreePoints = GetAllPoints();

			//display all peak points
			foreach(Vector3 peakPoint in peak.Points)
			{
				allTreePoints.Add(new CTreePoint(peakPoint, treePointExtent));
			}

			//display highest peak point
			allTreePoints.Add(new CTreePoint(peak.maxHeight, treePointExtent));

			if(pExportPoints)
			{
				CObjExporter.AddTreePointsBBToObj(ref obj, allTreePoints);
			}

			if(pExportBranches)
			{
				//add centers of all tree points
				List<Vector3> vectorPoints = new List<Vector3>();
				foreach(CTreePoint p in allTreePoints)
				{
					vectorPoints.Add(p.Center);
				}
				CObjExporter.AddPointsToObj(ref obj, vectorPoints);

				foreach(CBranch b in branches)
				{
					CObjExporter.AddBranchToObj(ref obj, b);
				}
			}
			//export box representation of tree
			if(pExportSimple)
			{
				Vector3 point1 = b000;
				Vector3 point2 = b100;
				Vector3 point3 = b110;
				Vector3 point4 = b010;

				//float? goundHeight = peakDetailField.GetHeight();
				if(groundHeight != null)
				{
					point1.Z = (float)groundHeight;
					point2.Z = (float)groundHeight;
					point3.Z = (float)groundHeight;
					point4.Z = (float)groundHeight;
				}

				CObjExporter.AddFaceToObj(ref obj, point1, point2, peak.Center);
				CObjExporter.AddFaceToObj(ref obj, point1, point4, peak.Center);
				CObjExporter.AddFaceToObj(ref obj, point2, point3, peak.Center);
				CObjExporter.AddFaceToObj(ref obj, point4, point3, peak.Center);
			}

			return obj;
		}

		//BOOLS
		bool isAtBorder;
		bool isAtBufferZone;

		public bool Validate(bool pRestrictive, bool pFinal = false)
		{
			if(Equals(24))
			{
				CDebug.WriteLine("");
			}

			if(GetTreeHeight() < CParameterSetter.GetIntSettings(ESettings.minTreeHeight))
			{
				isValid = false;
				tooLow = true;
				return isValid;
			}

			isValid = ValidatePoints(true);
			if(!isValid)
			{
				return isValid;
			}

			//if(IsAtBorderOf(CProjectData.Points.groundArray))
			isAtBorder = IsAtBorder();
			isAtBufferZone = CTreeMath.IsAtBufferZone(this);
			if(isAtBorder || isAtBufferZone)
			{
				isValid = false;
				return isValid;
			}

			if(pFinal && CTreeMath.IsAtBufferZone(this))
			{
				isValid = false;
				return isValid;
			}

			if(CTreeManager.GetDetectMethod() == EDetectionMethod.Detection2D)
			{
				isValid = ValidateTopPoints();
				return isValid;
			}

			isValid = ValidateBranches(pRestrictive);

			if(pFinal && !isValid && !IsAtBorder())
			{
				isValid = ValidatePoints(false);
			}

			if(Equals(debugTree))
			{
				CDebug.WriteLine(isValid + " Validate " + this);
			}

			return isValid;
		}

		private const float TOP_POINTS_RANGE = 5;
		private const int MIN_POINTS_PER_METER = 3;

		private bool ValidateTopPoints()
		{
			List<Vector3> topPoints = GetTopPoints(TOP_POINTS_RANGE);
			int expectedTopPointsCount = (int)TOP_POINTS_RANGE * MIN_POINTS_PER_METER;
			float ratio = (float)topPoints.Count / expectedTopPointsCount;
			return ratio > 0.8f;
		}

		private List<Vector3> GetTopPoints(float pDistFromTop)
		{
			List<Vector3> topPoints = new List<Vector3>();
			for(int i = 0; i < Points.Count; i++)
			{
				if(Vector3.Distance(peak.Center, Points[i]) < pDistFromTop)
				{
					topPoints.Add(Points[i]);
				}
				else
				{
					break;
				}
			}
			return topPoints;
		}

		private bool ValidateFirstBranchPoints()
		{
			//too many branch points are too far from peak
			const float maxDistOfFirstBranchPoint = 1.5f;
			int tooFarPointsCount = 0;
			foreach(CBranch branch in branches)
			{
				if(branch.TreePoints.Count == 0)
				{ continue; }
				float dist = peak.Z - branch.TreePoints[0].Z;
				if(dist > maxDistOfFirstBranchPoint)
				{
					tooFarPointsCount++;
				}
			}
			return tooFarPointsCount < 6;
		}

		private bool ValidatePoints(bool pJustTotalCount)
		{
			int totalPointCount = GetBranchesPointCount();
			if(pJustTotalCount && totalPointCount < CParameterSetter.GetIntSettings(ESettings.minTreePoints))
			{
				return false;
			}

			float definedHeight = Extent.Z;

			//in case only few points are defined at bottom. in this case the mniddle part is almost not defined (its ok)
			//and validation is affected
			definedHeight = Math.Min(GetTreeHeight() / 2, definedHeight);

			if(definedHeight < 1)
			{ return false; }

			int requiredPointsPerMeter = 3;
			int requiredPointCount = (int)definedHeight * requiredPointsPerMeter;
			return totalPointCount > requiredPointCount;
		}

		/// <summary>
		/// Checks if all branches have good scale ration with its opposite branch
		/// </summary>
		private bool ValidateScale()
		{
			foreach(CBranch b in branches)
			{
				if(b.GetPointCount() == 0)
				{ return false; }
			}

			for(int i = 0; i < Branches.Count / 2; i++)
			{
				float br1Scale = Branches[i].furthestPointDistance;
				CBranch oppositeBranch = Branches[i + Branches.Count / 2];
				float br2Scale = oppositeBranch.furthestPointDistance;
				float branchIScaleRatio = br1Scale / br2Scale;

				if(br1Scale > 0.5f && br2Scale > 0.5f)
				{
					continue;
				}

				//ideal scale ratio = 1
				//if X > Y => scaleRatio > 1
				float idealScaleOffset = Math.Abs(1 - branchIScaleRatio);
				if(Math.Abs(idealScaleOffset) > 0.5f)
				{
					//isValidScale = false;
					return false;
				}
			}
			return true;
		}

		private int debugTree = -1;

		/// <summary>
		/// Determines whether the tree is defined enough.
		/// pAllBranchDefined = if one of branches is not defined => tree is not valid.
		/// All trees touching the boundaries should be eliminated by this
		/// </summary>
		private bool ValidateBranches(bool pAllBranchesDefined)
		{
			if(!ValidateFirstBranchPoints())
				return false;

			float branchDefinedFactor = 0;
			int undefinedBranchesCount = 0;
			int wellDefinedBranchesCount = 0;

			foreach(CBranch b in branches)
			{
				float branchFactor = b.GetDefinedFactor();
				//if (Equals(debugTree))
				//{
				//	CDebug.WriteLine(b + "-- branchFactor " + branchFactor);
				//}

				if(Math.Abs(branchFactor) < 0.1f)
				{
					undefinedBranchesCount++;
					continue;
				}
				if(branchFactor > 0.5f)
				{
					wellDefinedBranchesCount++;
				}
				branchDefinedFactor += branchFactor;
			}
			if(pAllBranchesDefined)
			{
				if(undefinedBranchesCount > 1)
					return false;
			}

			if(undefinedBranchesCount > 2)
				return false;
			if(wellDefinedBranchesCount > 2)
				return true;

			float validFactor = branchDefinedFactor / (branches.Count - undefinedBranchesCount);
			//CDebug.WriteLine("VALID " + treeIndex + " height = " + height + " validFactor = " + validFactor);
			return validFactor > 0.5f;
		}

		public override bool Contains(Vector3 pPoint)
		{
			return base.Contains(pPoint) && GetBranchFor(pPoint).IsPointInExtent(pPoint);
		}

		private bool IsNewPeak(Vector3 pPoint)
		{
			if(peak.Includes(pPoint))
				return true;

			if(pPoint.Z < peak.Z)
				return false;

			float angle = CUtils.AngleBetweenThreePoints(pPoint - Vector3.UnitZ, pPoint, peak.Center);
			return Math.Abs(angle) < CTreeManager.MAX_BRANCH_ANGLE;
		}

		// todo: not used anywhere, delete?
		public bool BelongsToTree(Vector3 pPoint, bool pDebug = true)
		{
			if(IsNewPeak(pPoint))
			{
				return true;
			}

			const float MAX_DIST_TO_TREE_BB = 0.1f;
			float dist2D = CUtils.Get2DDistance(peak.Center, pPoint);
			float distToBB = Get2DDistanceFromBBTo(pPoint);
			//it must be close to peak of some tree or to its BB

			float maxTreeRadius = GetTreeRadius();

			if(dist2D > maxTreeRadius && distToBB > MAX_DIST_TO_TREE_BB)
			{
				if(CTreeManager.DEBUG && pDebug)
					CDebug.WriteLine("- dist too high " + dist2D + "|" + distToBB);
				return false;
			}

			Vector3 suitablePoint = peak.GetClosestPointTo(pPoint);
			float angle = CUtils.AngleBetweenThreePoints(suitablePoint - Vector3.UnitZ, suitablePoint, pPoint);
			float maxBranchAngle = GetMaxBranchAngle(suitablePoint, pPoint);
			if(angle > maxBranchAngle)
			{
				if(CTreeManager.DEBUG && pDebug)
					CDebug.WriteLine("- angle too high " + angle + "°/" + maxBranchAngle + "°. dist = " +
Vector3.Distance(suitablePoint, pPoint));
				return false;
			}

			return true;
		}

		/// <summary>
		/// Calculates max acceptable 2D distance from peak for the new point to be added
		/// </summary>
		/// //TODO: obsolete method! use tree radius equation
		public float GetTreeExtentFor(Vector3 pNewPoint, float pMaxExtentMultiplier)
		{
			float treeHeight = GetTreeHeight();
			float ratio = treeHeight / CTreeManager.AVERAGE_TREE_HEIGHT;
			float maxExtent = Math.Max(CParameterSetter.treeExtent, ratio * CParameterSetter.treeExtent);
			maxExtent *= pMaxExtentMultiplier;
			float zDiff = peak.Center.Z - pNewPoint.Z;
			const float Z_DIFF_STEP = 0.1f;
			const float EXTENT_VALUE_STEP = 0.12f;

			float extent = CTreeManager.MIN_TREE_EXTENT + EXTENT_VALUE_STEP * zDiff / Z_DIFF_STEP;

			return Math.Min(extent, maxExtent);
		}

		public float GetTreeRadius()
		{
			float height = GetTreeHeight();
			float radius = CTreeRadiusCalculator.GetTreeRadius(height);

			return radius;
		}

		public List<Vector3> GetFurthestPoints()
		{
			List<Vector3> points = new List<Vector3>();
			foreach(CBranch b in branches)
			{
				points.Add(b.furthestPoint);
			}
			return points;
		}

		//DEBUG TRANSFORMATIONS - NOT CORRECT ON ALL TREES

		public void Rotate(int pYangle)
		{
			for(int i = 0; i < Points.Count; i++)
			{
				float angleRadians = CUtils.ToRadians(pYangle);
				Vector3 point = Points[i];
				Vector3 rotatedPoint = Vector3.Transform(point, Matrix4x4.CreateRotationY(angleRadians, peak.Center));
				Points[i] = rotatedPoint;
			}
			ResetBounds(Points);
			if(peak.Points.Count > 1)
			{
				CDebug.Error("Cant Rotate after process!");
			}
		}

		public void Scale(int pScale)
		{
			//scale with center point = ground point
			//if center point = peak, resulting points might be set under ground level
			Vector3 groundPosition = GetGroundPosition();
			for(int i = 0; i < Points.Count; i++)
			{
				Vector3 point = Points[i];
				Vector3 scaledPoint = Vector3.Transform(point, Matrix4x4.CreateScale(
					pScale, pScale, pScale, groundPosition));
				Points[i] = scaledPoint;
			}
			ResetBounds(Points);
			if(peak.Points.Count > 1)
			{
				CDebug.Error("Cant Scale after process!");
			}
			peak = new CPeak(Points[0], treePointExtent);
		}

		public void Move(Vector3 pOffset)
		{
			for(int i = 0; i < Points.Count; i++)
			{
				Vector3 point = Points[i];
				Vector3 movedPoint = point + pOffset;
				Points[i] = movedPoint;
			}
			ResetBounds(Points);
			if(peak.Points.Count > 1)
			{
				CDebug.Error("Cant move after process!");
			}

			peak.Points[0] += pOffset;
			peak.ResetBounds(peak.Points[0]);
		}

		public override string ToString()
		{
			return ToString(EDebug.Index);
		}

		public string ToString(EDebug pDebug)
		{
			switch(pDebug)
			{
				case EDebug.Height:
					return ToString(true, false, false, false, false, false, true);
				case EDebug.Peak:
					return ToString(true, false, true, false, false, false, false);
				case EDebug.Index:
					return ToString(true, false, false, false, false, false, false);

			}
			return ToString();
		}

		public enum EDebug
		{
			Height,
			Peak,
			Index
		}

		public string ToString(bool pIndex, bool pPoints, bool pPeak, bool pBranches, bool pReftree, bool pValid, bool pHeight)
		{
			string indexS = pIndex ? treeIndex.ToString("000") + "-" + peakDetailField.ToStringIndex() : "";
			string pointsS = pPoints ? (" [" + GetAllPoints().Count.ToString("000") + "]") : "";
			string validS = pValid ? (isValid ? "|<+>" : "<->") : "";
			string peakS = pPeak ? "||peak = " + peak : "";
			string branchesS = pBranches ? "||BR=" + GetBranchesCount() +
				"[" + GetBranchesPointCount().ToString("000") + "]" + "_|" : "";
			string refTreeS = pReftree && assignedRefTreeObj != null ? "||reftree = " + assignedRefTreeObj.Name : "";
			string heightS = pHeight ? "||height = " + GetTreeHeight() : "";

			if(pBranches)
			{
				foreach(CBranch b in branches)
				{
					if(branches.IndexOf(b) == branches.Count - 1)
					{ branchesS += ". Stem = "; }
					branchesS += b;
				}
			}
			return indexS + pointsS + validS + peakS + branchesS + refTreeS + heightS;
		}

		//OTHERS

		public void CheckTree()
		{
			//CDebug.WriteLine("Check tree " + ToString());
			foreach(CBranch b in branches)
			{
				b.CheckBranch();
			}
		}

		public bool Equals(int pIndex)
		{
			return treeIndex == pIndex;
		}

		public override bool Equals(object obj)
		{
			if(obj == null || GetType() != obj.GetType())
				return false;

			CTree t = (CTree)obj;
			return treeIndex == t.treeIndex;
		}

		public bool IsAtBorder()
		{
			return IsAtBorderOf(CProjectData.Points.groundArray);
		}
		public bool IsAtBorderOf(CGroundArray pArray)
		{
			Tuple<float, bool> ret = pArray.GetDistanceAndDirectionToBorderFrom(peak.Center);
			float distanceToBorder = ret.Item1;
			bool closestToHorizontal = ret.Item2;

			return distanceToBorder < CParameterSetter.GetFloatSettings(ESettings.treeExtent) * 2;
			//return distanceToBorder < CProjectData.bufferSize; //to many trees invalid

			float borderDistExtentDiff = distanceToBorder - (closestToHorizontal ? Extent.X : Extent.Y) / 2;
			return borderDistExtentDiff < 0;
		}

		public override int GetHashCode()
		{
			return -2093264365 + treeIndex.GetHashCode();
		}
	}
}