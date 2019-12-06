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

		public const bool useDebugData = true;

		internal static void Init()
		{
			ballSets.Add(new CBallSet(
				CParameterSetter.GetStringSettings(ESettings.forestFileFullName)));
			currentBallSetIndex = ballSets.Count - 1;

			if(useDebugData)
				UseDebugData();
		}

		private static void UseDebugData()
		{
			switch(currentBallSetIndex)
			{
				case 0:
					AddBall(new CBall(new Vector3(-5.453406f, 2.855769f, 2.109725f))); //1
					AddBall(new CBall(new Vector3(-4.448736f, 6.231966f, 1.994494f))); //2
					AddBall(new CBall(new Vector3(-0.8353631f, -6.669966f, 1.979764f))); //3
					AddBall(new CBall(new Vector3(-3.608419f, -2.88409f, 2.065792f))); //4
					AddBall(new CBall(new Vector3(0.4928541f, 5.454957f, 2.02323f))); //5
					AddBall(new CBall(new Vector3(3.025239f, -4.26632f, 2.103709f))); //6
					AddBall(new CBall(new Vector3(5.462094f, -1.214247f, 2.037038f))); //7
					AddBall(new CBall(new Vector3(5.085885f, 3.084216f, 2.026366f))); //8
					break;
				case 1:
					AddBall(new CBall(new Vector3(-4.837209f, -12.86546f, 2.007648f))); //1
					AddBall(new CBall(new Vector3(-3.657253f, -8.836858f, 2.06641f))); //2
					AddBall(new CBall(new Vector3(-0.2954954f, -6.911431f, 2.080708f))); //3
					AddBall(new CBall(new Vector3(-1.448363f, -16.82076f, 2.007916f))); //4
					AddBall(new CBall(new Vector3(4.178645f, -6.046964f, 2.036028f))); //5
					AddBall(new CBall(new Vector3(5.36111f, -10.56972f, 2.041391f))); //6
					AddBall(new CBall(new Vector3(2.850861f, -19.39396f, 1.815004f))); //7
					AddBall(new CBall(new Vector3(4.989944f, -16.49725f, 1.966762f))); //8
					break;
			}
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
				output.AppendLine("--- centers ---");
				foreach(CBall ball in ballSet.balls)
				{
					output.AppendLine(ball.ToStringCenter());
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
