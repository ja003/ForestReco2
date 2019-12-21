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
		public string applyOnFile; //todo: apply on all source files and connect them?
			//"C:\\Users\\ja004\\Documents\\ForestReco\\tmp\\190910_103910_tmp\\_tiles[5]_190910_103910_s[-1.5,-7]-[1,-5]\\190910_103910_s[-1.5,-7]-[1,-5].txt";

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
