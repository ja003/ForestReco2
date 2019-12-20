using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public class CBallSet
	{
		public List<CBall> balls;
		public CRigidTransform transform;
		public string sourceFile;

		public CBallSet(string pSourceFile)
		{
			sourceFile = pSourceFile;
			balls = new List<CBall>();
		}

		public override string ToString()
		{
			return sourceFile;
		}

		public string ToStringFile()
		{
			return sourceFile;
		}

		public string ToStringFileBallsCount()
		{
			return sourceFile + " balls: " + balls.Count;
		}
	}
}
