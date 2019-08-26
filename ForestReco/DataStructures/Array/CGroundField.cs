using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public class CGroundField : CField
	{
		//public CTree DetectedTree;

		public CGroundField(Tuple<int, int> pIndexInField, Vector3 pCenter, float pStepSize, bool pDetail) : 
			base(pIndexInField, pCenter, pStepSize, pDetail)
		{
		}

		public override int? GetColorValue()
		{
			return 0;

			float? fieldHeight = MaxZ;
			float? groundHeight = CProjectData.GetMinHeight();
			//float? groundHeight = CProjectData.array.GetElementContainingPoint(center).GetHeight();
			float height = 0;
			if(fieldHeight != null && groundHeight != null)
			{
				height = (float)fieldHeight - (float)groundHeight;
			}
			//Color color = new Color();
			float max = CTreeManager.AVERAGE_MAX_TREE_HEIGHT;
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
