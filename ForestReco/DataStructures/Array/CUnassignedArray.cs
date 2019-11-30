using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public class CUnassignedArray : CArray<CUnassignedField>
	{
		public CUnassignedArray(float pStepSize, bool pDetail) : base(pStepSize, pDetail)
		{
		}

		protected override void InitFields(float pStepSize)
		{
			array = new CUnassignedField[arrayXRange, arrayYRange];
			fields = new List<CUnassignedField>();
			for(int x = 0; x < arrayXRange; x++)
			{
				for(int y = 0; y < arrayYRange; y++)
				{
					CUnassignedField newField = new CUnassignedField(new Tuple<int, int>(x, y),
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
	}
}
