using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	/// <summary>
	/// Workflow:
	/// 1. OnPrepareSequence
	///		2. Init - called for each file
	///			3. Process - each tile -> ball detection
	/// 4. OnLastSequenceDone
	/// </summary>
	public static class CBallsManager
	{
		public static List<CBallSet> ballSets;
		static int currentBallSetIndex;

		internal static void OnPrepareSequence()
		{
			ballSets = new List<CBallSet>();
		}

		internal static void Init()
		{
			ballSets.Add(new CBallSet(
				CParameterSetter.GetStringSettings(ESettings.forestFileFullName)));
			currentBallSetIndex = ballSets.Count - 1;
		}

		/// <summary>
		/// Ball detection
		/// </summary>
		internal static CBall Process(CBallField pField, bool pForce)
		{
			pField.Detect(pForce);
			if(pField.ball != null && pField.ball.isValid)
			{
				bool result = AddBall(pField.ball);
				if(result)
					return pField.ball;
			}
			return null;
		}

		public static void DebugAddBall(CBall pBall)
		{
			AddBall(pBall);
		}

		/// <summary>
		/// Checks if ball hasnt been detected yet (tiles overlap by ~1 meter)
		/// Balls should be at least 1m away from each other.
		/// Returns true if the ball was successfully added.
		/// </summary>
		private static bool AddBall(CBall pBall)
		{
			if(currentBallSetIndex < 0 || currentBallSetIndex >= ballSets.Count)
			{
				CDebug.Error("Ball OOR");
				return false;
			}

			foreach(CBall ball in ballSets[currentBallSetIndex].balls)
			{
				float dist = Vector3.Distance(ball.center, pBall.center);
				if(dist < CBallsTransformator.MAX_OFFSET)
				{
					CDebug.Warning($"Ball { ball} already in ballset {currentBallSetIndex}");
					return false;
				}
				if(dist < 1)
				{
					CDebug.Error("Balls are too close to each other!");
					//todo: calculate average center?
					return false;
				}
			}

			ballSets[currentBallSetIndex].balls.Add(pBall);
			return true;
		}

		private const string transformationFileName = "transform.txt";
		private static string outputFilePath => CProjectData.outputFolder + transformationFileName;

		/// <summary>
		/// Calculate transformation of each set to the set[0]
		/// Export into file
		/// </summary>
		internal static void OnLastSequenceDone()
		{
			CDebug.WriteLine("TODO: calculate tranformations");
			StringBuilder output = new StringBuilder();

			//write detected balls
			foreach(CBallSet ballSet in ballSets)
			{
				output.AppendLine(ballSet.sourceFile);
				foreach(CBall ball in ballSet.balls)
				{
					output.AppendLine(ball.ToString());
				}
				output.AppendLine("==============");
			}

			//write transformations
			for(int i = 1; i < ballSets.Count; i++)
			{
				ballSets[i].transform = CBallsTransformator.GetRigidTransform(
					ballSets[i].balls, ballSets[0].balls);
				string resutI = $"Transform for set {i} is {ballSets[i].transform}";
				CDebug.WriteLine(resutI);
				output.AppendLine(resutI);
			}

			CUtils.WriteToFile(output, outputFilePath);
		}
	}

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
	}
}
