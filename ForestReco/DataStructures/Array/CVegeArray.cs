using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public class CVegeArray : CArray<CVegeField>
	{
		public CVegeArray(float pStepSize, bool pDetail) : base(pStepSize, pDetail)
		{
		}

		protected override void InitFields(float pStepSize)
		{
			array = new CVegeField[arrayXRange, arrayYRange];
			fields = new List<CVegeField>();
			for(int x = 0; x < arrayXRange; x++)
			{
				for(int y = 0; y < arrayYRange; y++)
				{
					CVegeField newField = new CVegeField(new Tuple<int, int>(x, y),
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

		/// <summary>
		/// just check
		/// </summary>
		//public void DebugDetectedTrees()
		//{
		//	int detectedTreesCount = 0;
		//	int validTreesCount = 0;
		//	int invalidTreesCount = 0;
		//	foreach(CVegeField f in fields)
		//	{
		//		detectedTreesCount += f.DetectedTree == null ? 0 : 1;
		//		if(f.DetectedTree != null)
		//		{
		//			if(f.DetectedTree.isValid) { validTreesCount++; }
		//			else { invalidTreesCount++; }
		//		}
		//	}
		//	CDebug.Count("Detected trees", detectedTreesCount);
		//	CDebug.Count("valid trees", validTreesCount);
		//	CDebug.Count("invalid trees", invalidTreesCount);
		//}
	}
}
