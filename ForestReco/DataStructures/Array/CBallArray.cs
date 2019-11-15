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

		internal int FilterFieldsWithNeighbours()
		{
			int filteredCount = 0;
			foreach(CBallField f in fields)
			{
				filteredCount += f.FilterFieldsWithDefinedNeighbours();
			}
			return filteredCount;
		}

		internal void FilterPointsAtDistance(float pMinDistance, float pMaxDistance)
		{
			foreach(CBallField f in fields)
			{
				f.FilterPointsAtDistance(pMinDistance, pMaxDistance);
			}
		}

		internal List<Vector3> GetFilteredOutPoints()
		{
			List<Vector3> filteredOutPoints = new List<Vector3>();
			foreach(CBallField f in fields)
			{
				filteredOutPoints.AddRange(f.filteredOut);
			}
			return filteredOutPoints;
		}
	}
}
