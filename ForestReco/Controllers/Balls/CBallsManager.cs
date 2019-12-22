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

		//skips detection process and uses data defined in UseDebugData()
		public const bool useDebugData = false;

		//skips detection process and uses data defined in config file 
		//(see CSequenceController::GetConfig)
		public static bool useConfigDebugData = false;		

		internal static void Init()
		{
			ballSets.Add(new CBallSet(
				CParameterSetter.GetStringSettings(ESettings.forestFileFullName)));
			currentBallSetIndex = ballSets.Count - 1;

			if(useDebugData)
				UseDebugData();
			if(useConfigDebugData)
				UseConfigDebugData();
		}

		public static List<Vector3> configBallCenters = new List<Vector3>();
		private static void UseConfigDebugData()
		{
			foreach(Vector3 c in configBallCenters)
			{
				AddBall(new CBall(c));
			}
		}

		private static void UseDebugData()
		{
			switch(currentBallSetIndex)
			{
				//case 0:
				//	AddBall(new CBall(new Vector3(-5.453406f, 2.855769f, 2.109725f))); //1
				//	AddBall(new CBall(new Vector3(-4.448736f, 6.231966f, 1.994494f))); //2
				//	AddBall(new CBall(new Vector3(-0.8353631f, -6.669966f, 1.979764f))); //3
				//	AddBall(new CBall(new Vector3(-3.608419f, -2.88409f, 2.065792f))); //4
				//	AddBall(new CBall(new Vector3(0.4928541f, 5.454957f, 2.02323f))); //5
				//	AddBall(new CBall(new Vector3(3.025239f, -4.26632f, 2.103709f))); //6
				//	AddBall(new CBall(new Vector3(5.462094f, -1.214247f, 2.037038f))); //7
				//	AddBall(new CBall(new Vector3(5.085885f, 3.084216f, 2.026366f))); //8
				//	break;
				//case 1:
				//	AddBall(new CBall(new Vector3(-4.837209f, -12.86546f, 2.007648f))); //1
				//	AddBall(new CBall(new Vector3(-3.657253f, -8.836858f, 2.06641f))); //2
				//	AddBall(new CBall(new Vector3(-0.2954954f, -6.911431f, 2.080708f))); //3
				//	AddBall(new CBall(new Vector3(-1.448363f, -16.82076f, 2.007916f))); //4
				//	AddBall(new CBall(new Vector3(4.178645f, -6.046964f, 2.036028f))); //5
				//	AddBall(new CBall(new Vector3(5.36111f, -10.56972f, 2.041391f))); //6
				//	AddBall(new CBall(new Vector3(2.850861f, -19.39396f, 1.815004f))); //7
				//	AddBall(new CBall(new Vector3(4.989944f, -16.49725f, 1.966762f))); //8
				//	break;

				//update1
				//case 0:
				//	AddBall(new CBall(new Vector3(-5.453406f, 2.855769f, 2.109725f))); //1
				//	AddBall(new CBall(new Vector3(-4.474649f, 6.281722f, 1.984683f))); //2
				//	AddBall(new CBall(new Vector3(-0.8475582f, -6.652481f, 1.986673f))); //3
				//	AddBall(new CBall(new Vector3(-3.588191f, -2.87209f, 2.060397f))); //4
				//	AddBall(new CBall(new Vector3(0.4957421f, 5.435482f, 2.010766f))); //5
				//	AddBall(new CBall(new Vector3(3.025081f, -4.238124f, 2.090678f))); //6
				//	AddBall(new CBall(new Vector3(5.482442f, -1.234958f, 2.023156f))); //7
				//	AddBall(new CBall(new Vector3(5.093435f, 3.073572f, 2.002615f))); //8
				//	break;
				//case 1:
				//	AddBall(new CBall(new Vector3(-4.845504f, -12.9602f, 1.981996f))); //1
				//	AddBall(new CBall(new Vector3(-3.659401f, -8.830884f, 2.058574f))); //2
				//	AddBall(new CBall(new Vector3(-0.2793041f, -6.903613f, 2.080702f))); //3
				//	AddBall(new CBall(new Vector3(-1.439231f, -16.82587f, 2.00737f))); //4
				//	AddBall(new CBall(new Vector3(4.200833f, -6.0719f, 2.031518f))); //5
				//	AddBall(new CBall(new Vector3(5.347064f, -10.55185f, 2.028504f))); //6
				//	AddBall(new CBall(new Vector3(2.855435f, -19.38328f, 1.809576f))); //7
				//	AddBall(new CBall(new Vector3(4.992101f, -16.5478f, 1.970753f))); //8
				//	break;

				//just some balls
				case 0: //31
						//AddBall(new CBall(new Vector3(-5.453406f, 2.855769f, 2.109725f))); //1
						//AddBall(new CBall(new Vector3(-4.474649f, 6.281722f, 1.984683f))); //2
						//AddBall(new CBall(new Vector3(-0.8475582f, -6.652481f, 1.986673f))); //3
						//AddBall(new CBall(new Vector3(-3.588191f, -2.87209f, 2.060397f))); //4
						//AddBall(new CBall(new Vector3(0.4957421f, 5.435482f, 2.010766f))); //5

					//AddBall(new CBall(new Vector3(3.025081f, -4.238124f, 2.090678f))); //6
					AddBall(new CBall(new Vector3(5.482442f, -1.234958f, 2.023156f))); //7
					AddBall(new CBall(new Vector3(5.093435f, 3.073572f, 2.002615f))); //8
					break;
				case 1: //10
					AddBall(new CBall(new Vector3(-4.845504f, -12.9602f, 1.981996f))); //1=>8
					AddBall(new CBall(new Vector3(-3.659401f, -8.830884f, 2.058574f))); //2=>7
					AddBall(new CBall(new Vector3(-0.2793041f, -6.903613f, 2.080702f))); //3=>6

					//AddBall(new CBall(new Vector3(-1.439231f, -16.82587f, 2.00737f))); //4
					//AddBall(new CBall(new Vector3(4.200833f, -6.0719f, 2.031518f))); //5
					//AddBall(new CBall(new Vector3(5.347064f, -10.55185f, 2.028504f))); //6
					//AddBall(new CBall(new Vector3(2.855435f, -19.38328f, 1.809576f))); //7
					//AddBall(new CBall(new Vector3(4.992101f, -16.5478f, 1.970753f))); //8
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
		public static bool AddBall(CBall pBall)
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
				const int min_ball_distance = 1;
				if(dist < min_ball_distance)
				{
					//if balls are very close to each other they are most likely
					//the same ball detected twice => not error
					if(dist > CBall.BALL_DIAMETER)
						CDebug.Error("Balls are too close to each other!");

					//todo: calculate average center?
					return false;
				}
			}

			ballSets[currentBallSetIndex].balls.Add(pBall);
			return true;
		}


		private static string GetOutputFilePath(bool pIsFinal)
		{
			const string fileName = "transform";
			const string ext = ".txt";

			string appendix = pIsFinal ? "_final" : "";

			return CProjectData.outputFolder + fileName + appendix + ext;
		}


		/// <summary>
		/// Process the last calculated ballSet and the first (origin)
		/// Export into file
		/// </summary>
		internal static void OnSequenceDone()
		{
			if(ballSets.Count <= 1) //is 1 in after the first set is processed
			{
				if(ballSets.Count == 0)
					CDebug.Error("No ball sets detected");
				return;
			}

			List<CBallSet> firstAndLastSet = new List<CBallSet>();
			firstAndLastSet.Add(ballSets.First());
			firstAndLastSet.Add(ballSets.Last());

			StringBuilder output = ProcessSets(firstAndLastSet);
			CUtils.WriteToFile(output, GetOutputFilePath(false));
		}

		/// <summary>
		/// Process all calculated ballSets
		/// Export into file
		/// </summary>
		internal static void OnLastSequenceDone()
		{
			StringBuilder output = ProcessSets(ballSets);
			CUtils.WriteToFile(output, GetOutputFilePath(true));
		}


		/// <summary>
		/// Calculate transformation of each set to the set[0]
		/// Return the output string
		/// </summary>
		internal static StringBuilder ProcessSets(List<CBallSet> pSets)
		{
			StringBuilder output = new StringBuilder();

			//write detected balls
			foreach(CBallSet ballSet in pSets)
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
				output.AppendLine("================================");
			}

			//calculate and write transformations
			List<CBall> origBalls = pSets[0].balls;
			for(int i = 1; i < pSets.Count; i++)
			{
				List<CBall> processedBalls = pSets[i].balls;
				pSets[i].transform =
					CBallsTransformator.GetRigidTransform(processedBalls, origBalls);
				CRigidTransform resultTransformation = pSets[i].transform;
				if(resultTransformation == null)
				{
					CDebug.Error($"No transformation for the set {pSets[i]} has been calculated.\n" +
						$"Origin set = {pSets[0].ToStringFileBallsCount()}\n" +
						$"This set = {pSets[i].ToStringFileBallsCount()}\n");
					continue;
				}

				string resutI = $"Transform for set {i} is {resultTransformation}";
				//CDebug.WriteLine(resutI);
				output.AppendLine(resutI);

				output.AppendLine("---------");
				//write the result projections of processed balls to orig set
				foreach(CBall ball in processedBalls)
				{
					Vector3 transformedPoint =
						CBallsTransformator.GetTransformed(ball.center, resultTransformation);
					string transformedPointResult = $"{ball.center} => {transformedPoint}";
					output.AppendLine(transformedPointResult);
				}
				output.AppendLine("-----------------------------------------------------");

				if(pSets[i].applyOnFile != null)
					ApplyTransform(resultTransformation, pSets[i].applyOnFile);
			}
			CDebug.WriteLine(output.ToString(), true);

			return output;
		}

		/// <summary>
		/// Reads the file, applies transformation on every point and exports it to las
		/// </summary>
		private static void ApplyTransform(CRigidTransform pTransformation, string pApplyOnFile)
		{
			string[] lines = CProgramLoader.GetFileLines(pApplyOnFile);
			List<Tuple<EClass, Vector3>> parsedLines = CProgramLoader.ParseLines(lines, false, true);

			List<Vector3> projectedPoints = new List<Vector3>();
			foreach(Tuple<EClass, Vector3> line in parsedLines)
			{
				Vector3 projected = CBallsTransformator.GetTransformed(line.Item2, pTransformation);
				projectedPoints.Add(projected);
			}

			CLasExporter.ExportPoints(projectedPoints, "projected");
		}
	}
}
