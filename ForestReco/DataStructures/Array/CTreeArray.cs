
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public class CTreeArray : CArray<CTreeField>
	{
		public CTreeArray(float pStepSize, bool pDetail) : base(pStepSize, pDetail)
		{
		}

		protected override void InitFields(float pStepSize)
		{
			array = new CTreeField[arrayXRange, arrayYRange];
			fields = new List<CTreeField>();
			for(int x = 0; x < arrayXRange; x++)
			{
				for(int y = 0; y < arrayYRange; y++)
				{
					CTreeField newField = new CTreeField(new Tuple<int, int>(x, y),
						new Vector3(
							topLeftCorner.X + x * stepSize + stepSize / 2,
							topLeftCorner.Y - y * stepSize - stepSize / 2,
							0),
						pStepSize, Detail);
					array[x, y] = newField;
					fields.Add(newField);
				}
			}
		}

		public bool PathContainsTree(Vector3 pFrom, Vector3 pTo, CTree pTree, float pMaxPathLength)//, float pMaxDescend)
		{
			CTreeField fieldFrom = GetFieldContainingPoint(pFrom);
			CTreeField fieldTo = GetFieldContainingPoint(pTo);
			List<CTreeField> path = GetPathFrom(fieldFrom, fieldTo, pMaxPathLength);

			if(fieldFrom.GetDetectedTreesFromNeighbourhood().Contains(pTree))
				return true;

			for(int i = 1; i < path.Count; i++)
			{
				CTreeField treeField = GetField(path[i].indexInField);
				List<CTree> treesInHood = treeField.GetDetectedTreesFromNeighbourhood();
				/*float? fieldheight = (Detail ?
					CProjectData.Points.preprocessDetailArray :
					CProjectData.Points.preprocessNormalArray).
						GetField(path[i].indexInField).GetHeight();

				if(fieldheight != null)
				{
					float heightDiff = pFrom.Z - (float)fieldheight;
					if(heightDiff > pMaxDescend)
						return false;
				}*/

				foreach(CTree tree in treesInHood)
				{
					if(tree.Equals(pTree))
						return true;
				}
			}
			return false;
		}

		public List<CTree> GetTreesInDistanceFrom(Vector3 pPoint, float pDistance)
		{
			int steps = (int)(pDistance / stepSize);
			steps = Math.Max(1, steps);
			return GetTreesInMaxStepsFrom(pPoint, steps);
		}

		/// <summary>
		/// Returns closest trees to the point being in maximal distance of X steps.
		/// Trees are sorted based on distance to the point
		/// </summary>
		public List<CTree> GetTreesInMaxStepsFrom(Vector3 pPoint, int pSteps)
		{
			Tuple<int, int> index = GetIndexInArray(pPoint);

			//TODO: test newer approach using treeFields and sorting based on distance from point 
			//to the field rather than to tree peak.
			//If ok => delete
			//List<CTree> trees = new List<CTree>();
			Dictionary<CTree, CTreeField> treeFields = new Dictionary<CTree, CTreeField>();

			for(int x = index.Item1 - pSteps; x <= index.Item1 + pSteps; x++)
			{
				for(int y = index.Item2 - pSteps; y <= index.Item2 + pSteps; y++)
				{
					CTreeField field = GetField(x, y);
					if(field != null)
					{
						List<CTree> detectedTrees = field.DetectedTrees;
						foreach(CTree tree in detectedTrees)
						{
							//if(!trees.Contains(tree))
							//	trees.Add(tree);

							CTreeField detectedField;
							if(treeFields.TryGetValue(tree, out detectedField))

							{
								if(detectedField.GetDistanceTo(pPoint) < field.GetDistanceTo(pPoint))
									continue;
								treeFields[tree] = field;
								continue;
							}

							treeFields.Add(tree, field);
						}
					}
				}
			}

			////todo: sort based on a distance from point to the closest field where tree has been detected.
			////in this implementation some tree in the result can be actually further from point than other
			//trees.Sort((a, b) => CUtils.Get2DDistance(pPoint, a.Center).CompareTo(CUtils.Get2DDistance(pPoint, b.Center)));

			List<KeyValuePair<CTree, CTreeField>> treeFieldList = treeFields.ToList();
			//sort based on a distance from point to the closest field where tree has been detected
			treeFieldList.Sort((a, b) => CUtils.Get2DDistance(pPoint, a.Value.Center).CompareTo(CUtils.Get2DDistance(pPoint, b.Value.Center)));

			List<CTree> result = treeFieldList.ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
			return result;
			//return trees;
		}

		/// <summary>
		/// just check
		/// </summary>
		public void DebugDetectedTrees()
		{
			int detectedTreesCount = 0;
			int validTreesCount = 0;
			int invalidTreesCount = 0;
			foreach(CTreeField f in fields)
			{
				detectedTreesCount += f.DetectedTrees.Count;
				if(f.DetectedTrees.Count != 0)
				{
					foreach(CTree tree in f.DetectedTrees)
					{
						if(tree.isValid) { validTreesCount++; }
						else { invalidTreesCount++; }
					}
				}
			}
			if(detectedTreesCount == 0)
				return;

			CDebug.Count("Detected trees", detectedTreesCount);
			CDebug.Count("valid trees", validTreesCount);
			CDebug.Count("invalid trees", invalidTreesCount);
		}
	}
}
