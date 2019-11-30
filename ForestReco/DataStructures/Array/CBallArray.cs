using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public class CBallArray : CArray<CBallField>
	{
		public CBallArray(float pStepSize, bool pDetail) : base(pStepSize, pDetail)
		{
		}

		protected override void InitFields(float pStepSize)
		{
			array = new CBallField[arrayXRange, arrayYRange];
			fields = new List<CBallField>();
			for(int x = 0; x < arrayXRange; x++)
			{
				for(int y = 0; y < arrayYRange; y++)
				{
					CBallField newField = new CBallField(new Tuple<int, int>(x, y),
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
		
		public void FilterPointsAtHeight(float pMinHeight, float pMaxHeight)
		{
			foreach(CBallField f in fields)
			{
				f.FilterPointsAtHeight(pMinHeight, pMaxHeight);
			}
		}
	}
}
