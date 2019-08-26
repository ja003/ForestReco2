using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public class CVegeField : CField
	{
		//public List<CTree> DetectedTrees = new List<CTree>();
		//public CTree DetectedTree;
		//public bool IsPeak;

		public CVegeField(Tuple<int, int> pIndexInField, Vector3 pCenter, float pStepSize, bool pDetail) : 
			base(pIndexInField, pCenter, pStepSize, pDetail)
		{
		}

		public override int? GetColorValue()
		{
			float? fieldHeight = MaxZ;
			float? groundHeight = CProjectData.groundArray.GetElementContainingPoint(center).GetHeight();

			float height = 0;
			if(fieldHeight != null && groundHeight != null)
			{
				height = (float)fieldHeight - (float)groundHeight;
			}
			//Color color = new Color();
			float max = CTreeManager.MaxTreeHeight;
			//float max = CTreeManager.AVERAGE_MAX_TREE_HEIGHT;

			float value = height / max;
			value *= 255;
			if(value < 0 || value > 255)
			{
				//not error - comparing just to AVERAGE_MAX_TREE_HEIGHT, not MAX
				value = Math.Max(0, value);
				value = Math.Min(255, value);
			}

			int intVal = (int)value;
			return intVal;
		}

		
	}
}
