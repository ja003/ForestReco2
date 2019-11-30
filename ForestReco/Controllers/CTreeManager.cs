using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ForestReco
{
	public static class CTreeManager
	{
		public static List<CTree> Trees { get; private set; }
		public static List<CTree> InvalidTrees { get; private set; }
		//trees which were evaluated not to be actual trees but rather unfiltered noise
		public static List<CTree> NotTrees { get; private set; }

		public static float LocalMaxHeight { get; private set; }
		public static float AllowedDescend { get; private set; }
		public static int MinAscendSteps { get; private set; }
		public static int MinDescendSteps { get; private set; }

		//public static List<Vector3> invalidVegePoints; //?? todo: delete

		//public static List<Vector3> allPoints = new List<Vector3>(); //just debug

		public static float MaxTreeHeight = int.MinValue;
		//public static float minTreeHeight;

		public const float TREE_POINT_EXTENT = 0.1f;

		public const float MIN_TREE_EXTENT = 0.5f;

		public static float AVERAGE_TREE_HEIGHT;
		public static int MIN_BRANCH_POINT_COUNT = 5;
		public static int MIN_TREE_POINT_COUNT = 20;

		public static float MIN_FAKE_TREE_HEIGHT = 20;

		public static float AVERAGE_MAX_TREE_HEIGHT = 40;

		private static EDetectionMethod detectMethod;

		public static void Init()
		{
			treeIndex = 0;
			debugcalls = 0;
			maxPossibleTreesAssignment = 0;

			detectMethod = GetDetectMethod();
			AllowedDescend = CParameterSetter.GetFloatSettings(ESettings.allowedDescend);
			MinAscendSteps = CParameterSetter.GetIntSettings(ESettings.minAscendSteps);
			MinDescendSteps = CParameterSetter.GetIntSettings(ESettings.minDescendSteps);

			LocalMaxHeight = CParameterSetter.GetFloatSettings(ESettings.localMaxHeight);

			Reinit();
		}

		public static void Reinit()
		{
			Trees = new List<CTree>();
			InvalidTrees = new List<CTree>();
			NotTrees = new List<CTree>();
			pointCounter = 0;
		}

		public static float GetMinPeakDistance(float pMultiply)
		{
			return CParameterSetter.treeExtent * pMultiply;
		}

		/// <summary>
		/// Calculates minimal peak distance for given trees to be merged
		/// </summary>
		public static float GetMinPeakDistance(CTree pTree1, CTree pTree2)
		{
			float treeHeight = Math.Max(pTree1.GetTreeHeight(), pTree2.GetTreeHeight());
			float ratio = treeHeight / AVERAGE_TREE_HEIGHT;
			if(ratio < 1) { return CParameterSetter.treeExtent; }
			const float EXTENT_VALUE_STEP = 1.5f;

			return GetMinPeakDistance(1) + (ratio - 1) * EXTENT_VALUE_STEP;
		}

		public static void AssignMaterials()
		{
			foreach(CTree tree in Trees)
			{
				tree.AssignMaterial();
			}
		}

		public const float MAX_BRANCH_ANGLE = 45;
		private static int treeIndex;

		public static bool DEBUG = false;
		private static int pointCounter;

		private const int MAX_DEBUG_COUNT = 5;
		private const int MAX_DISTANCE_FOR_POSSIBLE_TREES = 5;

		public static int maxPossibleTreesAssignment; //just debug. number of assignment of point to the last of possible trees for that point

		public static int currentPointIndex;

		private static void ProcessDebug(Vector3 pPoint, int pPointIndex)
		{
			if(pPointIndex == 2899)
				return;

			if(CDebug.IsDebugPoint3D(pPoint))
			{
				return;
			}
			if(CDebug.IsDebugPoint(pPoint))
			{
				return;
			}
		}

		public static void AddPoint(Vector3 pPoint, int pPointIndex)
		{
			//index < 0 = the point was re-added during the tree merging process 
			//(where the index is not passed) - use the last index
			if(pPointIndex < 0)
				pPointIndex = currentPointIndex;

			currentPointIndex = pPointIndex;

			ProcessDebug(pPoint, pPointIndex);

			pointCounter++;

			CProjectData.Points.vegeArray.AddPointInField(pPoint);
			CProjectData.Points.vegeDetailArray.AddPointInField(pPoint);


			List<CTree> possibleTrees = GetPossibleTreesFor(pPoint, EPossibleTreesMethod.ClosestHigher, false);

			CTree selectedTree = SelectBestPossibleTree(possibleTrees, pPoint, true);

			/*if(selectedTree == null && detectMethod == EDetectionMethod.AddFactor2D)
			{
				CTreeField f = CProjectData.Points.treeDetailArray.GetFieldContainingPoint(pPoint);
				List<CTree> treesInHood = f.GetDetectedTreesFromNeighbourhood();
				foreach(CTree tree in treesInHood)
				{
					selectedTree = tree;
					break;
				}
			}*/

			if(selectedTree != null)
			{
				selectedTree.AddPoint(pPoint, pPointIndex);
			}
			else if(CProjectData.Points.treeDetailArray.GetFieldContainingPoint(pPoint).DetectedTrees.Count > 0)
			{
				//new tree cant be created on field where another tree was already detected - shouldnt happen
				CDebug.Error("trying to create tree on a field where a tree already is");
				return;
			}
			else
			{
				CreateNewTree(pPoint);
			}
		}



		public static CTree SelectBestPossibleTree(List<CTree> pPossibleTrees, Vector3 pPoint, bool pRestrictive)
		{
			CTree selectedTree = null;
			float bestAddPointFactor = 0;
			const int max_possible_trees = 3; //in most cases the point is not added to further trees
			int processCount = Math.Min(max_possible_trees, pPossibleTrees.Count);
			processCount = pPossibleTrees.Count;

			for(int i = 0; i < processCount; i++)
			{
				CTree tree = pPossibleTrees[i];
				if(DEBUG) { CDebug.WriteLine("- try add to : " + tree.ToString(CTree.EDebug.Peak)); }

				CTree treeAtTheField = CProjectData.Points.treeDetailArray.GetFieldContainingPoint(pPoint).GetSingleDetectedTree();
				if(treeAtTheField != null && treeAtTheField.Equals(tree))
				{
					selectedTree = tree;
					break;
				}

				if(detectMethod == EDetectionMethod.Detection2D)
				{
					bool canBeAdded = tree.CanAdd(pPoint, pRestrictive);
					if(canBeAdded)
					{
						//todo: what if point can be added to more trees?
						//maybe should be re-processed later
						selectedTree = tree;
						if(selectedTree.treeIndex == 12)
						{
							//CDebug.WriteLine("");
						}
						if(i == max_possible_trees - 1)
						{
							//this shouldnt happend too often
							maxPossibleTreesAssignment++;
							//CDebug.WriteLine("possibleTrees.IndexOf(t) = " + i);
						}

						break;
					}
				}
				else if(detectMethod == EDetectionMethod.AddFactor)
				{
					float addPointFactor = tree.GetAddPointFactor(pPoint);
					if(addPointFactor > 0.5f)
					{
						if(addPointFactor > bestAddPointFactor)
						{
							selectedTree = tree;
							bestAddPointFactor = addPointFactor;
						}
					}
				}
				else if(detectMethod == EDetectionMethod.AddFactor2D)
				{
					float addPointFactor = tree.GetAddPointFactor(pPoint);

					if(addPointFactor > 0.5f)
					{
						if(addPointFactor > bestAddPointFactor)
						{
							selectedTree = tree;
							bestAddPointFactor = addPointFactor;
						}
					}

					/*else {
						CTreeField f = CProjectData.Points.treeDetailArray.GetFieldContainingPoint(pPoint);
						if(f.GetSingleDetectedTree() != null)
						{
							selectedTree = f.GetSingleDetectedTree();
						}
					}*/

					//this joins too many trees together
					//else if(tree.CanAdd(pPoint))
					//{
					//	selectedTree = tree;
					//}
				}

			}
			return selectedTree;
		}

		static bool debugNewTree = true;
		static int debugcalls = 0; //to prevent stack overflow

		private static void CreateNewTree(Vector3 pPoint)
		{
			if(treeIndex == 24 && debugcalls < 1)
			{
				//CDebug.WriteLine("");
				if(debugNewTree)
				{
					debugcalls++;
					AddPoint(pPoint, -1);
					return;
				}
			}
			CTree newTree = new CTree(pPoint, treeIndex, TREE_POINT_EXTENT);

			Trees.Add(newTree);
			treeIndex++;

			CProjectData.Points.OnTreeCreated(newTree, pPoint);

			if(newTree.GetTreeHeight() > MaxTreeHeight)
				MaxTreeHeight = newTree.GetTreeHeight();
		}

		public static void MarkAsNotTree(CTree pTree)
		{
			pTree.notTree = true;
			//to remove references from arrays
			DeleteTree(pTree);
			NotTrees.Add(pTree);
		}

		public static void DeleteTree(CTree pTree)
		{
			if(pTree.treeIndex == 169)
			{
				bool i = true;
			}

			if(!Trees.Contains(pTree))
			{
				CDebug.Error("Trees dont contain " + pTree);
				return;
			}
			pTree.peakDetailField.RemoveTree(pTree);
			pTree.peakNormalField.RemoveTree(pTree);
			foreach(CTreeField field in pTree.detailFields)
			{
				field.RemoveTree(pTree);
			}
			foreach(CTreeField field in pTree.normalFields)
			{
				field.RemoveTree(pTree);
			}
			//shouldnt be neccessary
			//pTree.peakDetailField.RemoveTree(pTree);
			//pTree.peakNormalField.RemoveTree(pTree);
			Trees.Remove(pTree);
			//if(!pTree.treeDetailField.GetDetectedTrees().Contains(pTree))
			//{
			//	CDebug.Error("element " + pTree.treeDetailField + " doesnt contain " + pTree);
			//	return;
			//}			
		}

		private static void DebugPoint(Vector3 pPoint, int pPointIndex)
		{
			if(DEBUG) { CDebug.WriteLine("\n" + pointCounter + " AddPoint " + pPoint); }

			Vector3 debugPoint = CObjExporter.GetMovedPoint(pPoint);
			debugPoint.Z *= -1;
		}

		private static List<CTree> GetPossibleTreesToMergeWith(CTree pTree, EPossibleTreesMethod pMethod)
		{
			return GetPossibleTreesFor(pTree.peak.Center, pMethod, true, pTree);
		}

		public static EDetectionMethod GetDetectMethod()
		{
			//251 - Finalizace produktu
			return EDetectionMethod.AddFactor;

			//EDetectionMethod detectMethod = (EDetectionMethod)CParameterSetter.GetIntSettings(ESettings.detectMethod);

			//if(detectMethod == EDetectionMethod.None)
			//{
			//	CDebug.Error("No detection method set! setting default: CanBeAdded");
			//	CParameterSetter.SetParameter(ESettings.detectMethod, (int)EDetectionMethod.Detection2D);
			//	detectMethod = (EDetectionMethod)CParameterSetter.GetIntSettings(ESettings.detectMethod);
			//}
			//return detectMethod;
		}

		/// <summary>
		/// Returns trees to which is possible (probable) to add the pPoint
		/// </summary>
		/// <param name="pMerging">pPoint is a peak of a tree and we are not interested in this tree</param>
		/// <returns></returns>
		public static List<CTree> GetPossibleTreesFor(Vector3 pPoint, EPossibleTreesMethod pMethod, bool pMerging,
			CTree pExcludeTree = null)
		{
			List<CTree> possibleTrees = new List<CTree>();
			if(pMethod == EPossibleTreesMethod.ClosestHigher)
			{
				possibleTrees.AddRange(CProjectData.Points.treeNormalArray.GetTreesInMaxStepsFrom(pPoint, MAX_DISTANCE_FOR_POSSIBLE_TREES));

				//remove trees that are too far
				//for(int i = possibleTrees.Count - 1; i >= 0; i--)
				//{
				//	if(CUtils.Get2DDistance(pPoint, possibleTrees[i].peak.Center) > MAX_DISTANCE_FOR_POSSIBLE_TREES)
				//	{
				//		possibleTrees.RemoveAt(i);
				//	}
				//}

			}
			else if(pMethod == EPossibleTreesMethod.Contains)
			{
				foreach(CTree t in Trees)
				{
					if(pExcludeTree != null && pExcludeTree.Equals(t))
					{
						continue;
					}
					if(t.Contains(pPoint))
					{
						possibleTrees.Add(t);
					}
				}
			}
			else if(pMethod == EPossibleTreesMethod.Belongs)
			{
				//todo: either optimize or delete
				throw new Exception("shit code");
				//foreach(CTree t in Trees)
				//{
				//	if(pExcludeTree != null && pExcludeTree.Equals(t))
				//	{
				//		continue;
				//	}

				//	if(t.BelongsToTree(pPoint, false))
				//	{
				//		possibleTrees.Add(t);
				//		t.possibleNewPoint = pPoint;
				//	}
				//}
			}

			//Already sorted in GetTreesInMaxStepsFrom
			//sort trees by 2D distance of given point to them
			//possibleTrees.Sort((a, b) => CUtils.Get2DDistance(a.peak.Center, pPoint).CompareTo(
			//	CUtils.Get2DDistance(b.peak.Center, pPoint)));

			if(pMethod == EPossibleTreesMethod.ClosestHigher)
			{
				//CDebug.WriteLine("SELECT FROM " + possibleTrees.Count);

				List<CTree> closestTrees = new List<CTree>();
				//no reason to select more. small chance that point would fit better to further tree
				const int maxClosestTreesCount = 3;
				int counter = 0;
				foreach(CTree possibleTree in possibleTrees)
				{
					if(possibleTree.Equals(pExcludeTree)) { continue; }
					//we dont want trees that are lower than given point
					if(possibleTree.peak.maxHeight.Z < pPoint.Z) { continue; }

					closestTrees.Add(possibleTree);
					counter++;
					if(counter > maxClosestTreesCount)
					{
						break;
					}

				}
				//it can happen that the tree on the very same field as the point is skipped
				CTree treeOnFieldWithPoint = CProjectData.Points.treeDetailArray.GetFieldContainingPoint(pPoint).GetSingleDetectedTree();
				if(treeOnFieldWithPoint != null &&
					treeOnFieldWithPoint != pExcludeTree /*&& 
					!closestTrees.Contains(treeOnFieldWithPoint)*/)
				{
					closestTrees.Insert(0, treeOnFieldWithPoint);
				}


				return closestTrees;
			}

			return possibleTrees;
		}


		//MERGE
		public static bool IsMerging;

		/// <summary>
		/// If not-tree is very close to some tree then it is probably part of that tree.
		/// Not-tree will be merged into the closest one.
		/// </summary>
		public static void TryMergeNotTrees()
		{
			IsMerging = true;
			DateTime mergeStartTime = DateTime.Now;
			CDebug.WriteLine("TryMergeNotTrees");

			//sort in descending order - trees will be processed from the end
			NotTrees.Sort((b, a) => b.peak.Center.Z.CompareTo(a.peak.Center.Z));
			//TODO: CHECK!!! THIS IS NOT DESCENDING ORDER???


			int treeCountBeforeMerge = NotTrees.Count;

			for(int i = NotTrees.Count - 1; i >= 0; i--)
			{
				CTree notTree = NotTrees[i];
				//if(notTree.Equals(335))
				//	CDebug.WriteLine("");

				//just debug
				//List<CTree> closeTrees2 = CProjectData.Points.treeNormalArray.
				//		  GetTreesInMaxStepsFrom(notTree.peak.Center, 1);

				List<CTree> closeTrees = new List<CTree>();
				//fisrt try to get a tree on the same detail field
				CTree t = CProjectData.Points.treeDetailArray.
					GetFieldContainingPoint(notTree.peak.Center).GetSingleDetectedTree();
				if(t != null)
					closeTrees.Add(t);

				//if no tree is on the same field get some in close distance
				if(closeTrees.Count == 0)
				{
					closeTrees = CProjectData.Points.treeDetailArray.
						  GetTreesInMaxStepsFrom(notTree.peak.Center, 2);
				}

				//merge not-tree with the closest tree but only if it is higher.
				//if not then it is probably some noise above the regular tree
				if(closeTrees.Count > 0 &&
					closeTrees[0].GetTreeHeight() > notTree.GetTreeHeight() &&
					!notTree.IsLocalMaximum())
				{
					closeTrees[0].MergeWith(notTree);
					NotTrees.RemoveAt(i);
				}
			}
			IsMerging = false;

			CDebug.Duration("Not-trees merge", mergeStartTime);
			CDebug.Count("Number of not-trees merged", treeCountBeforeMerge - Trees.Count);
		}

		public static void TryMergeAllTrees(bool pOnlyInvalid)
		{
			IsMerging = true;
			DateTime mergeStartTime = DateTime.Now;
			CDebug.WriteLine("TryMergeAllTrees");

			Trees.Sort((a, b) => b.peak.Center.Z.CompareTo(a.peak.Center.Z));

			int treeCountBeforeMerge = Trees.Count;

			if(detectMethod == EDetectionMethod.AddFactor)
				MergeGoodAddFactorTrees(pOnlyInvalid);
			if(detectMethod == EDetectionMethod.Detection2D)
				Merge2DTrees();

			if(pOnlyInvalid)
			{
				CAnalytics.secondMergeDuration = CAnalytics.GetDuration(mergeStartTime);
			}
			else
			{
				CAnalytics.firstMergeDuration = CAnalytics.GetDuration(mergeStartTime);
			}
			IsMerging = false;

			CDebug.Duration("Trees merge", mergeStartTime);
			CDebug.Count("Number of trees merged", treeCountBeforeMerge - Trees.Count);
		}

		/// <summary>
		/// Mergind method for Detection2D
		/// </summary>
		private static void Merge2DTrees()
		{
			return;
			for(int i = Trees.Count - 1; i >= 0; i--)
			{
				CTree treeToMerge = Trees[i];
				if(treeToMerge.Equals(173))
				{
					CDebug.WriteLine();
				}

				//if(treeToMerge.IsLocalMaximum())
				//{
				//	continue;
				//}

				List<CTree> possibleTrees =
					GetPossibleTreesToMergeWith(treeToMerge, EPossibleTreesMethod.ClosestHigher);

				for(int t = 0; t < possibleTrees.Count; t++)
				{
					//merge trees if the target tree can be reached from the treeToMerge
					//and they are of different heights
					CTree tree = possibleTrees[t];

					float maxRadius = CTreeRadiusCalculator.GetTreeRadius(tree);
					float treeAvgExtent = tree.GetAverageExtent();
					float maxDist = Math.Max(maxRadius, treeAvgExtent);

					float treeDist = CUtils.Get2DDistance(treeToMerge, tree);
					if(treeDist > maxDist)
					{
						continue;
					}

					/*float heightDiff = tree.GetTreeHeight() - treeToMerge.GetTreeHeight();
					if(heightDiff < 2)
					{
						continue;
					}*/
					bool pathContainsTree = false;
					/*
					CTreeField closeField = tree.GetClosestField(treeToMerge.peak.Center);
					bool pathContainsTree =
						CProjectData.Points.treeDetailArray.PathContainsTree(
							treeToMerge.peak.Center, closeField.Center, tree, 1, 1);

					if(!pathContainsTree)
					{
						pathContainsTree =
						  CProjectData.Points.treeDetailArray.PathContainsTree(
							  treeToMerge.peak.Center, tree.peak.Center, tree, 1, 1);
					}*/

					bool canAdd = tree.CanAdd(treeToMerge.peak.Center, false);

					if(pathContainsTree || canAdd)
					{
						treeToMerge = MergeTrees(ref treeToMerge, ref tree, true);
						break;
					}
				}

				/*List<CField> fields = treeToMerge.peakDetailField.GetFieldsInDistance(5);
				foreach(CField field in fields)
				{
					CTreeField treeField = (CTreeField)field;
					CTree detectedTree = treeField.GetSingleDetectedTree();
					if(detectedTree != null && !detectedTree.Equals(treeToMerge))
					{
						float heightDiff = detectedTree.GetTreeHeight() - treeToMerge.GetTreeHeight();
						if(heightDiff > 2)
						{
							treeToMerge = MergeTrees(ref treeToMerge, ref detectedTree, true);
							break;
						}
					}
				}*/
			}
		}

		/// <summary>
		/// Merges all trees, whose peak has good AddFactor to another tree
		/// </summary>
		private static void MergeGoodAddFactorTrees(bool pOnlyInvalid)
		{
			DateTime mergeStart = DateTime.Now;
			DateTime previousMergeStart = DateTime.Now;
			int iteration = 0;
			int maxIterations = Trees.Count;
			for(int i = Trees.Count - 1; i >= 0; i--)
			{
				if(CProjectData.backgroundWorker.CancellationPending) { return; }

				if(i >= Trees.Count)
				{
					//CDebug.WriteLine("Tree was deleted");
					continue;
				}
				CTree treeToMerge = Trees[i];
				if(treeToMerge.treeIndex == 48 || treeToMerge.treeIndex == 1001)
				{
					CDebug.WriteLine("");
				}

				if(pOnlyInvalid && !treeToMerge.isValid && treeToMerge.IsAtBorder())
				{
					//CDebug.Warning(treeToMerge + " is at border");
					continue;
				}

				//dont merge tree if it is local maximum
				//if some tree is very close (in neighbouring detail field)
				//merge it with the highest one
				if(detectMethod == EDetectionMethod.AddFactor2D)
				{
					if(treeToMerge.Equals(22))
						CDebug.WriteLine();

					if(treeToMerge.IsLocalMaximum())
						continue;

					CTreeField f = CProjectData.Points.treeDetailArray.GetFieldContainingPoint(treeToMerge.peak.Center);
					List<CTree> treesInHood = f.GetDetectedTreesFromNeighbourhood();
					foreach(CTree tree in treesInHood)
					{
						if(tree.Equals(treeToMerge))
							continue;

						MergeTrees(tree, treeToMerge);
						break;
					}
					continue;
				}


				List<CTree> possibleTrees = GetPossibleTreesToMergeWith(treeToMerge, EPossibleTreesMethod.ClosestHigher);
				Vector3 pPoint = treeToMerge.peak.Center;
				float bestAddPointFactor = 0;
				CTree selectedTree = null;

				foreach(CTree possibleTree in possibleTrees)
				{
					bool isFar = false;
					bool isSimilarHeight = false;

					if(possibleTree.isValid && treeToMerge.isValid)
					{
						float mergeFactor = GetMergeValidFacor(treeToMerge, possibleTree);
						if(mergeFactor > 0.9f)
						{
							selectedTree = possibleTree;
							break;
						}
					}
					if(pOnlyInvalid && possibleTree.isValid && treeToMerge.isValid)
					{
						continue;
					}

					if(treeToMerge.isValid)
					{
						//treeToMerge is always lower
						float possibleTreeHeight = possibleTree.GetTreeHeight();
						float treeToMergeHeight = treeToMerge.GetTreeHeight();

						const float maxPeaksDistance = 1;
						float peaksDist = CUtils.Get2DDistance(treeToMerge.peak, possibleTree.peak);
						if(peaksDist > maxPeaksDistance)
						{
							isFar = true;
						}

						const float maxPeakHeightDiff = 1;
						if(possibleTreeHeight - treeToMergeHeight < maxPeakHeightDiff)
						{
							isSimilarHeight = true;
						}
					}

					float addPointFactor = possibleTree.GetAddPointFactor(pPoint, treeToMerge);
					float requiredFactor = 0.5f;
					if(isFar) { requiredFactor += 0.1f; }
					if(isSimilarHeight) { requiredFactor += 0.1f; }
					if(pOnlyInvalid) { requiredFactor -= 0.2f; }

					if(addPointFactor > requiredFactor && addPointFactor > bestAddPointFactor)
					{
						selectedTree = possibleTree;
						bestAddPointFactor = addPointFactor;
						if(bestAddPointFactor > 0.9f) { break; }
					}
				}
				if(selectedTree != null)
				{
					float dist = CUtils.Get2DDistance(treeToMerge.peak.Center, selectedTree.peak.Center);
					treeToMerge = MergeTrees(ref treeToMerge, ref selectedTree, pOnlyInvalid);
				}

				CDebug.Progress(iteration, maxIterations, 50, ref previousMergeStart, mergeStart, "merge");
				iteration++;
			}
		}

		/// <summary>
		/// Merge trees with similar height which have peaks too close
		/// </summary>
		private static float GetMergeValidFacor(CTree pTreeToMerge, CTree pPossibleTree)
		{
			float factor = 0;
			float peakHeightDiff = pPossibleTree.peak.Z - pTreeToMerge.peak.Z;
			float treeExtent = CParameterSetter.treeExtent * CParameterSetter.treeExtentMultiply;
			float similarHeightFactor = (treeExtent - peakHeightDiff) / treeExtent;
			bool isSimilarHeight = similarHeightFactor > 0.5f;
			if(!isSimilarHeight) { return 0; }

			float peakDist2D = CUtils.Get2DDistance(pTreeToMerge.peak, pPossibleTree.peak);
			if(peakDist2D < treeExtent)
			{
				return 1;
			}

			return factor;
		}

		/// <summary>
		/// Merges the lower tree into the higher one.
		/// First calculates which one is lower.
		/// Then does the treevalidation.
		/// </summary>
		private static CTree MergeTrees(ref CTree pTree1, ref CTree pTree2, bool pValidateRestrictive)
		{
			CTree higherTree = pTree1.peak.maxHeight.Z >= pTree2.peak.maxHeight.Z ? pTree1 : pTree2;
			CTree lowerTree = pTree1.peak.maxHeight.Z < pTree2.peak.maxHeight.Z ? pTree1 : pTree2;
			MergeTrees(higherTree, lowerTree);

			higherTree.Validate(pValidateRestrictive);

			return higherTree;
		}

		/// <summary>
		/// Deletes the lower tree and merges it into the higher one.
		/// important! delete has to be done before the merge to prevent deadlock by adding points to new tree during merging
		/// </summary>
		public static void MergeTrees(CTree pHigherTree, CTree pLowerTree)
		{
			float higherTreeZ = pHigherTree.peak.maxHeight.Z;
			float lowerTreeZ = pLowerTree.peak.minHeight.Z;
			if(lowerTreeZ > higherTreeZ)
			{
				CDebug.Error("given pHigherTree is lower!");
				return;
			}

			DeleteTree(pLowerTree);
			pHigherTree.MergeWith(pLowerTree);
		}

		public enum EValidation
		{
			Scale,
			BranchDefine
		}

		/// <summary>
		/// Evaluates if tree is valid. 
		/// More restrictive evaluation is optional.
		/// pCathegorize => assign invalid tree to InvalidTrees and remove from Trees
		/// pFinal => trees at array buffer positions will be invalid
		/// </summary>
		public static void ValidateTrees(bool pCathegorize, bool pRestrictive, bool pFinal = false)
		{
			CDebug.WriteLine("Detect invalid trees", true);

			for(int i = Trees.Count - 1; i >= 0; i--)
			{
				CTree tree = Trees[i];

				bool isValid = tree.Validate(pRestrictive, pFinal);

				if(!isValid)
				{
					if(pCathegorize)
					{
						InvalidTrees.Add(tree);
						Trees.RemoveAt(i);
					}
				}
			}
		}

		public static void DebugTrees()
		{
			CDebug.WriteLine("===============", true);
			CDebug.WriteLine("Detected trees");
			foreach(CTree t in Trees)
			{
				CDebug.WriteLine(Trees.IndexOf(t).ToString("00") + ": " + t);
				if(Trees.IndexOf(t) > MAX_DEBUG_COUNT)
				{
					CDebug.WriteLine("too much to debug...total = " + Trees.Count);
					return;
				}
			}
			CDebug.WriteLine("\n===============\n");
		}

		public static void CheckAllTrees()
		{
			foreach(CTree t in Trees)
			{
				t.CheckTree();
			}
		}

		public static void DebugTree(int pIndex)
		{
			foreach(CTree tree in Trees)
			{
				if(tree.treeIndex == pIndex)
				{
					CDebug.WriteLine("DebugTree " + tree);
					return;
				}
			}
		}

		public static int GetInvalidTreesAtBorderCount()
		{
			return InvalidTrees.Count(tree => tree.IsAtBorder());
		}

		public static float GetAverageTreeHeight()
		{
			float sum = 0;
			foreach(CTree tree in Trees)
			{
				sum += tree.GetTreeHeight();
			}
			return sum / Trees.Count;
		}

		//todo: calculate once and cache
		public static float GetMinTreeHeight()
		{
			float min = 666;
			foreach(CTree tree in Trees)
			{
				if(tree.GetTreeHeight() < min)
				{
					min = tree.GetTreeHeight();
				}
			}
			return min;
		}

		/*public static float GetMaxTreeHeight()
		{
			float max = 0;
			foreach(CTree tree in Trees)
			{
				if(tree.GetTreeHeight() > max)
				{
					max = tree.GetTreeHeight();
				}
			}
			return max;
		}*/
	}
}