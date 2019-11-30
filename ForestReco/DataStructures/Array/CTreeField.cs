using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public class CTreeField : CField
	{
		public bool IsPeak { get; private set; }

		//todo: create AllDetectedTrees and CurrentDetectedTrees (just for current tile)
		public List<CTree> DetectedTrees { get; }

		public CTreeField(Tuple<int, int> pIndexInField, Vector3 pCenter, float pStepSize, bool pDetail) :
			base(pIndexInField, pCenter, pStepSize, pDetail)
		{
			DetectedTrees = new List<CTree>();
		}

		/// <summary>
		/// Returns true if tree was added
		/// </summary>
		public bool AddDetectedTree(CTree pTree, bool pIsPeak)
		{
			//if tree is not peak, dont set IsPeak to false, could be a peak from previous tree
			if(pIsPeak)
				IsPeak = true;

			if(this.indexInField.Item1 == 171 && this.indexInField.Item1 == 144)
				CDebug.WriteLine();

			if(!DetectedTrees.Contains(pTree))
			{
				if(IsDetail && DetectedTrees.Count > 0)
				{
					CDebug.Error($"Adding tree to detail field {this} where tree {DetectedTrees[0]} altready is");
				}

				DetectedTrees.Add(pTree);
				pTree.AddField(this);
				return true;
			}
			return false;
		}

		public bool RemoveTree(CTree pTree)
		{
			return DetectedTrees.Remove(pTree);
		}

		public CTree GetSingleDetectedTree()
		{
			if(DetectedTrees.Count == 0)
				return null;
			if(DetectedTrees.Count > 1)
				CDebug.Error($"User requested single tree but field {this} contains more");
			return DetectedTrees[0];
		}

		/// <summary>
		/// Returns all detected trees in this field and its neighbours
		/// sorted descending by height
		/// </summary>
		/// <returns></returns>
		public List<CTree> GetDetectedTreesFromNeighbourhood()
		{
			List<CTree> trees = new List<CTree>();
			foreach(CTreeField field in GetNeighbours(true))
			{
				foreach(CTree tree in field.DetectedTrees)
				{
					if(!trees.Contains(tree))
						trees.Add(tree);
				}
			}
			trees.Sort((a, b) => b.GetTreeHeight().CompareTo(a.GetTreeHeight()));
			return trees;
		}

		//public CTreeField GetFirstOtherTreeField(CTree pOtherThanTree, EDirection pDirection)
		//{
		//	CTreeField currentField = (CTreeField)GetNeighbour(pDirection);
		//	if(currentField == null)
		//		return null;

		//..not sure about this
		//	while(currentField.DetectedTrees.Count > 0 && currentField.DetectedTrees.Contains(pOtherThanTree))
		//	{
		//		currentField = (CTreeField)currentField.GetNeighbour(pDirection);
		//		if(currentField == null)
		//			return null;
		//	}
		//	return currentField;
		//}

		public override string ToString()
		{
			string trees = DetectedTrees.Count > 0 ? DetectedTrees[0].ToString() : " - ";
			return $"{ToStringIndex()}, [{DetectedTrees.Count}]:{trees}";
		}
	}
}
