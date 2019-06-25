using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public enum EPossibleTreesMethod
	{
		Belongs, //finds trees, in which given point belongs
		ClosestHigher, //finds closest trees which are higher than given point
		Contains, //finds trees, which contains given point
				  //GoodAddFactor
	}
}
