using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public static class CDebugData
	{
		const float POINT_STEP = 0.05f;

		public static List<Tuple<int, Vector3>> GetTreeStraight()
		{
			List<Tuple<int, Vector3>> points = new List<Tuple<int, Vector3>>();
			for (int i = 0; i < 10; i++)
			{
				points.Add(new Tuple<int, Vector3>(5, new Vector3(0, 0, i * POINT_STEP)));
			}
			return points;
		}

		public static List<Tuple<int, Vector3>> GetTreeStraight2()
		{
			List<Tuple<int, Vector3>> points = new List<Tuple<int, Vector3>>();
			points.Add(new Tuple<int, Vector3>(5, new Vector3(0, 0, 1)));
			points.Add(new Tuple<int, Vector3>(5, new Vector3(0, 0, 1 - POINT_STEP)));
			points.Add(new Tuple<int, Vector3>(5, new Vector3(5 * POINT_STEP, 0, 1)));
			points.Add(new Tuple<int, Vector3>(5, new Vector3(0, 5 * POINT_STEP, 1)));

			points.Add(new Tuple<int, Vector3>(5, new Vector3(0, 0, 0)));

			return points;
		}

		public static List<Tuple<EClass, Vector3>> GetStandartTree()
		{
			List<Vector3> points = new List<Vector3>();
			points.Add(new Vector3(0, 0, 1));
			points.Add(new Vector3(0, 0, .5f));

			points.Add(new Vector3(.2f, 0, .5f));
			points.Add(new Vector3(-.2f, 0, .5f));
			points.Add(new Vector3(0, .2f, .5f));
			points.Add(new Vector3(0, -.2f, .5f));


			List<Tuple<EClass, Vector3>> pointTuples = new List<Tuple<EClass, Vector3>>();
			foreach (Vector3 p in points)
			{
				pointTuples.Add(new Tuple<EClass, Vector3>(EClass.Ground, p));
			}

			return pointTuples;
		}

		internal static void OnPrepareSequence()
		{
			index = -1;
		}

		internal static void Init()
		{
			index++;
			debugDone = false;
		}

		//implemented for balls detection debug
		private const bool use_debug_data = false;
		public static bool debugDone = false;
		public static int index = -1;

		/// <summary>
		/// Return false if debug is not processed.
		/// </summary>
		public static bool BallsDebug()
		{
			if(!CRxpParser.IsRxp || !use_debug_data)
				return false;
			if(debugDone)
				return true;

			switch(index)
			{
				case 0:
					CBallsManager.DebugAddBall(new CBall(new Vector3(0, 0, 0)));
					CBallsManager.DebugAddBall(new CBall(new Vector3(1, 0, 0)));
					break;
				case 1:
					CBallsManager.DebugAddBall(new CBall(new Vector3(0, 0, 0)));
					CBallsManager.DebugAddBall(new CBall(new Vector3(0, 1, 0)));
					break;
			}
			debugDone = true;
			return true;
		}

		//public static void DefineArray(bool pConstantHeight, float pHeight)
		//{
		//	CDebug.WriteLine("Define debug array");

		//	CProjectData.mainHeader = new CHeaderInfo(new[]
		//	{
		//		"","","","","","","","","","","","","","","",
		//		"0 0 0", "0 0 0" , "0 0 0" , "0 0 0"
		//	});
		//	CProjectData.Points.groundArray = new CGroundArray(CParameterSetter.groundArrayStep);

		//	if (pConstantHeight)
		//	{
		//		for (int x = 0; x < CProjectData.Points.groundArray.arrayXRange; x++)
		//		{
		//			for (int y = 0; y < CProjectData.Points.groundArray.arrayXRange; y++)
		//			{
		//				//CProjectData.array.GetElement(x, y).AddPoint(new Vector3(0, pHeight, 0));
		//				CProjectData.Points.groundArray.SetHeight(pHeight, x, y);
		//			}
		//		}
		//	}
		//	else
		//	{
		//		CProjectData.Points.groundArray.SetHeight(0, 0, 0);
		//		CProjectData.Points.groundArray.SetHeight(0, CProjectData.Points.groundArray.arrayXRange - 1, 0);

		//		CProjectData.Points.groundArray.SetHeight(2, CProjectData.Points.groundArray.arrayXRange / 2, CProjectData.Points.groundArray.arrayYRange / 2);

		//		CProjectData.Points.groundArray.SetHeight(5, CProjectData.Points.groundArray.arrayXRange - 1, CProjectData.Points.groundArray.arrayYRange - 1);
		//		CProjectData.Points.groundArray.SetHeight(5, 0, CProjectData.Points.groundArray.arrayYRange - 1);
		//	}
		//	CObjPartition.Init();
		//}
	}
}