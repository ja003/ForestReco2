
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

		public List<CTree> GetTreesInDistanceFrom(Vector3 pPoint, float pDistance)
		{
			Tuple<int, int> index = GetIndexInArray(pPoint);
			int steps = (int)(pDistance / stepSize);
			steps = Math.Max(1, steps);

			List<CTree> trees = new List<CTree>();

			for(int x = index.Item1 - steps; x < index.Item1 + steps; x++)
			{
				for(int y = index.Item2 - steps; y < index.Item2 + steps; y++)
				{
					CTreeField field = GetField(x, y);
					if(field != null)
					{
						List<CTree> detectedTrees = field.DetectedTrees;
						foreach(CTree tree in detectedTrees)
						{
							if(!trees.Contains(tree))
								trees.Add(tree);
						}
						
					}
				}
			}

			return trees;
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
			CDebug.Count("Detected trees", detectedTreesCount);
			CDebug.Count("valid trees", validTreesCount);
			CDebug.Count("invalid trees", invalidTreesCount);
		}		
	}
}
