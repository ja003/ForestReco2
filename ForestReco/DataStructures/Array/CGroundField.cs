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

		public override void FillMissingHeight(EFillMethod pMethod, int pKernelMultiplier)
		{
			if(IsDefined()) { return; }
			
			int maxSteps = 1;
			switch(pMethod)
			{
				case EFillMethod.ClosestDefined:
					MaxFilledHeight = GetAverageHeightFromClosestDefined(10 * maxSteps, false);
					break;
				case EFillMethod.FromNeighbourhood:
					MaxFilledHeight = GetAverageHeightFromNeighbourhood(pKernelMultiplier);
					break;
			}
		}
	}
}
