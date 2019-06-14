using System;
using System.Collections.Generic;

namespace ForestReco
{
	public static class CDebug
	{
		private static int stepCallCount;
		private static CMainForm form;

		private static int countedStepsCount; //cache value

		public static void Init(CMainForm pForm)
		{
			form = pForm;
		}

		public static void ReInit()
		{
			stepCallCount = 0;
			countedStepsCount = GetCountedStepsCount();
		}

		public static void Count(string pText, int pCount, int pOutOf = -1)
		{
			WriteLine(pText + ": " + pCount + (pOutOf > 0 ? " out of " + pOutOf : ""));
		}

		public static void WriteLine(string pText, bool pBreakLineBefore = false, bool pBreakLineAfter = false)
		{
			Console.WriteLine((pBreakLineBefore ? "\n" : "") + pText + (pBreakLineAfter ? "\n" : ""));
		}

		public static void Action(string pAction, string pText)
		{
			WriteLine(pAction + ": " + pText, true);
		}

		internal static void Warning(string pText)
		{
			WriteLine("WARNING: " + pText, true);
		}

		internal static void Error(string pText, bool pWriteInAnalytics = true)
		{
			WriteLine("ERROR: " + pText, true);
			if(pWriteInAnalytics)
			{
				CAnalytics.AddError(pText);
			}
		}

		internal static void Duration(string pText, DateTime pStartTime)
		{
			double totalSeconds = CAnalytics.GetDuration(pStartTime);
			WriteLine(pText + " | duration = " + totalSeconds);
		}

		public static void Progress(int pIteration, int pMaxIteration, int pDebugFrequency, ref DateTime pPreviousDebugStart, DateTime pStart, string pText, bool pShowInConsole = false)
		{
			if(pIteration % pDebugFrequency == 0 && pIteration > 0)
			{

				double lastIterationBatchTime = (DateTime.Now - pPreviousDebugStart).TotalSeconds;

				double timeFromStart = (DateTime.Now - pStart).TotalSeconds;

				float remainsRatio = ((float)pMaxIteration / pIteration);
				double estimatedTotalSeconds = remainsRatio * timeFromStart;

				int percentage = pIteration * 100 / pMaxIteration;

				string comment = "\n" + pText + " " + pIteration + " out of " + pMaxIteration;
				if(pShowInConsole)
				{
					WriteLine(comment);
					WriteLine("- time of last " + pDebugFrequency + " = " + lastIterationBatchTime);
					WriteLine($"- total time = {timeFromStart}");
				}
				WriteExtimatedTimeLeft(percentage, estimatedTotalSeconds - timeFromStart, comment, pShowInConsole);
				pPreviousDebugStart = DateTime.Now;
			}

			//after last iteration set progressbar to 0.
			//next step doesnt have to use progressbar and it wouldnt get refreshed
			if(pIteration == pMaxIteration - 1)
			{
				WriteExtimatedTimeLeft(100, 0, "done", pShowInConsole);
			}
		}

		private static string lastTextProgress;
		private static void WriteExtimatedTimeLeft(int pPercentage, double pSecondsLeft, string pComment, bool pShowInConsole)
		{
			TimeSpan ts = new TimeSpan(0, 0, 0, (int)pSecondsLeft);
			string timeString = ts.Hours + " hours " + ts.Minutes + " minutes " + ts.Seconds + " seconds.";

			string timeLeftString =
				$"- estimated time left = {timeString}\n";
			if(pShowInConsole)
				WriteLine(timeLeftString);

			CProjectData.backgroundWorker.ReportProgress(pPercentage, new[]
			{
				lastTextProgress , pComment , timeLeftString
			});
		}

		public static void Step(EProgramStep pStep)
		{
			string tileProgress = GetTileProgress(pStep);

			lastTextProgress = tileProgress + GetStepText(pStep);

			string[] message = new[] { lastTextProgress };
			if(pStep == EProgramStep.Exception)
			{
				CAnalytics.WriteErrors();
				return;
			}

			CProjectData.backgroundWorker.ReportProgress(0, message);
		}

		private static string GetTileProgress(EProgramStep pStep)
		{
			if(!IsCountableStep(pStep))
				return "";
			string progress = $"{CProgramStarter.currentTileIndex + 1} / {CProgramStarter.tilesCount}";
			string nl = Environment.NewLine;
			string sep = "==========";
			return $"{sep}{nl}TILE: {progress} {nl}{sep}{nl}";
		}

		public static void WriteProblems(List<string> problems)
		{
			string message = "Problems:" + Environment.NewLine;

			foreach(string p in problems)
			{
				message += p + Environment.NewLine;
			}
			WriteLine(message);

			//todo: maybe its not neccessary to handle progress messages 
			//through ReportProgress but rather like this...but works for now
			form.textProgress.Text = message;
		}

		private static string GetStepText(EProgramStep pStep)
		{
			if(pStep == EProgramStep.Exception)
			{
				return "EXCEPTION";
			}

			stepCallCount++;
			//-2 for abort states
			int maxSteps = countedStepsCount;
			stepCallCount = Math.Min(stepCallCount, maxSteps); //bug: sometimes writes higher value
			string progress = IsCountableStep(pStep) ?
				stepCallCount + "/" + maxSteps + ": " : "";
			string text = GetStepString(pStep);

			return progress + text;
		}


		/// <summary>
		/// Preprocess steps are not counted.
		/// </summary>
		private static int GetCountedStepsCount()
		{
			int count = 0;
			foreach(EProgramStep type in Enum.GetValues(typeof(EProgramStep)))
			{
				if(IsCountableStep(type))
					count++;
			}
			return count;
			//return Enum.GetNames(typeof(EProgramStep)).Length - 3;
		}

		/// <summary>
		/// Steps from preprocessing are not counted
		/// </summary>
		private static bool IsCountableStep(EProgramStep pStep)
		{
			//todo: tmp hack for preprocessing steps
			if(pStep.ToString().Contains("Pre_"))
				return false;

			switch(pStep)
			{
				case EProgramStep.Exception:
				case EProgramStep.Cancelled:
				case EProgramStep.LoadReftrees:
					return false;
			}
			return true;
		}

		private static string GetStepString(EProgramStep pStep)
		{
			string text;
			switch(pStep)
			{
				case EProgramStep.LoadLines:
					text = "load forest file lines";
					break;
				case EProgramStep.ParseLines:
					text = "parse forest file lines";
					break;
				case EProgramStep.ProcessGroundPoints:
					text = "process ground points";
					break;
				case EProgramStep.ProcessVegePoints:
					text = "process vege points";
					break;
				case EProgramStep.PreprocessVegePoints:
					text = "preprocess vege points";
					break;
				case EProgramStep.ValidateTrees1:
					text = "first tree validation";
					break;
				case EProgramStep.MergeTrees1:
					text = "first tree merge";
					break;
				case EProgramStep.ValidateTrees2:
					text = "second tree validation";
					break;
				case EProgramStep.MergeTrees2:
					text = "second tree merging";
					break;
				case EProgramStep.ValidateTrees3:
					text = "final tree validation";
					break;
				case EProgramStep.LoadReftrees:
					text = "loading reftrees";
					break;
				case EProgramStep.AssignReftrees:
					text = "assigning reftrees";
					break;
				case EProgramStep.LoadCheckTrees:
					text = "loading checktrees";
					break;
				case EProgramStep.AssignCheckTrees:
					text = "assigning checktrees";
					break;
				case EProgramStep.Export:
					text = "exporting";
					break;
				case EProgramStep.Bitmap:
					text = "generating bitmaps";
					break;
				case EProgramStep.Done:
					text = "DONE";
					break;

				default:
					text = $"{pStep} - comment not specified";
					break;
			}

			return text;
		}
	}

	public enum EProgramStep
	{
		LoadLines = 1,
		LoadReftrees = 2, //not part of tile processing
		ParseLines = 3,
		ProcessGroundPoints = 4,
		PreprocessVegePoints = 5,
		ProcessVegePoints = 6,
		ValidateTrees1 = 7,
		MergeTrees1 = 8,
		ValidateTrees2 = 9,
		MergeTrees2 = 10,
		ValidateTrees3 = 11,
		AssignReftrees = 12,
		LoadCheckTrees = 13,
		AssignCheckTrees = 14,
		Export = 15,
		Bitmap = 16,
		Done = 17,

		Cancelled,
		Exception,
		Pre_Split,
		Pre_LasClassify,
		Pre_Tile,
		Pre_DeleteTmp,
		Pre_LasReverseTile,
		Pre_LasHeight,
		Pre_LasGround,
		Pre_LasToTxt,
		Pre_Noise
	}
}
