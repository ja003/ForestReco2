using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public class CUnassignedField : CField
	{

		public float ExpectedGroundZ;

		public CUnassignedField(Tuple<int, int> pIndexInField, Vector3 pCenter, float pStepSize, bool pDetail) : 
			base(pIndexInField, pCenter, pStepSize, pDetail)
		{
		}
		
		public override void FillMissingHeight(EFillMethod pMethod, int pKernelMultiplier)
		{
			if(MinZ != null)
			{
				ExpectedGroundZ = (float)MinZ;
				return;
			}

			int maxSteps = 1;
			switch(pMethod)
			{
				case EFillMethod.ClosestDefined:
					float? avgMinZ = GetAverageHeightFromClosestDefined(10 * maxSteps, false, EHeight.MinZ);
					if(avgMinZ != null)
						ExpectedGroundZ = (float)avgMinZ;

					break;
				case EFillMethod.FromNeighbourhood:
					CDebug.Error("Unsupported method");
					break;
			}
		}

	}
}
