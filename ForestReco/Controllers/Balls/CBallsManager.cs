using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public static class CBallsManager
	{
		public static List<CBallSet> ballSets;

		internal static void Init()
		{
			ballSets = new List<CBallSet>();
		}

		static int currentIndex;
		internal static void InitTile(int pTileIndex)
		{
			currentIndex = pTileIndex;
			ballSets.Add(new CBallSet());
		}

		internal static CBall Process(CBallField pField)
		{
			pField.Detect();
			if(pField.ball != null && pField.ball.isValid)
			{
				AddBall(pField.ball);
				return pField.ball;
			}
			return null;
		}

		private static void AddBall(CBall pBall)
		{
			if(currentIndex < 0 || currentIndex >= ballSets.Count)
			{
				CDebug.Error("Ball OOR");
				return;
			}

			ballSets[currentIndex].balls.Add(pBall);
		}
	}

	public class CBallSet
	{
		public List<CBall> balls = new List<CBall>();
		public CRigidTransform transform;

		public CBallSet()
		{
		}
	}
}
