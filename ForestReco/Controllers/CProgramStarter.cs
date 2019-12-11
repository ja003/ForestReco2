using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Threading;

namespace ForestReco
{
	public enum EProcessResult
	{
		Cancelled,
		Exception,
		Done
	}


	public static class CProgramStarter
	{
		public static int currentTileIndex;
		public static int tilesCount;

		public static void PrepareSequence()
		{
			CSequenceController.Init();
			CBallsManager.OnPrepareSequence();
			CDebugData.OnPrepareSequence();
		}

		public static EProcessResult Start()
		{
			CSequenceController.SetValues();

			DateTime startTime = DateTime.Now;
			CProjectData.Init();
			CTreeManager.Init();
			CAnalytics.Init();

			CDartTxt.Init();
			CLasExporter.Init();
			CBiomassController.Init(
				CParameterSetter.GetStringSettings(ESettings.dbh),
				CParameterSetter.GetStringSettings(ESettings.agb));
			CTreeRadiusCalculator.Init();
			CShpController.Init();
			CReftreeManager.Init();
			CBallsManager.Init();
			CDebugData.Init();

			Thread.CurrentThread.CurrentCulture = new CultureInfo("en");

			//GENERAL
			CProjectData.useMaterial = true;
			CObjExporter.simplePointsObj = false;

			CMaterialManager.Init();

			string[] workerResult = new string[2];
			workerResult[0] = "this string";
			workerResult[1] = "some other string";
			CProjectData.backgroundWorker.ReportProgress(10, workerResult);

			try
			{
				if(CBallsManager.useDebugData)
					goto afterTileProcess;

				List<string> tiledFiles = CProgramLoader.GetTiledPreprocessedFilePaths();

				tilesCount = tiledFiles.Count;
				int startTile = CParameterSetter.GetIntSettings(ESettings.startIndex);
				if(startTile < 0 || startTile >= tiledFiles.Count)
				{
					throw new Exception($"Parameter startTile = {startTile}, tiledFiles.Count = {tiledFiles.Count}");
				}

				for(int i = startTile; i < tiledFiles.Count; i++)
				{
					string tiledFilePath = tiledFiles[i];
					EProcessResult tileProcess = ProcessTile(tiledFilePath, i);
					if(CProjectData.backgroundWorker.CancellationPending)
						break;
				}
			}
			catch(Exception e)
			{
				CDebug.Error(
					$"{Environment.NewLine}exception: {e.Message} {Environment.NewLine}{Environment.NewLine}" +
					$"StackTrace: {e.StackTrace}{Environment.NewLine}");
				OnException();
				return EProcessResult.Exception;
			}

			afterTileProcess: CDebug.WriteLine();

			if(CProjectData.backgroundWorker.CancellationPending)
			{
				CDebug.Step(EProgramStep.Cancelled);
				return EProcessResult.Cancelled;
			}

			CDebug.Step(EProgramStep.ExportMainFiles);
			//TODO: implement this in super class for all controllers
			//dont create the main file if not needed
			if(tilesCount > 1)
			{
				CDartTxt.ExportMain();
				CLasExporter.ExportMain();
				CShpController.ExportMain();
			}
			CBitmapExporter.ExportMain();

			CDebug.Step(EProgramStep.Done);

			if(CSequenceController.IsLastSequence())
			{
				CSequenceController.OnLastSequenceEnd();
				CBallsManager.OnLastSequenceDone();
				return EProcessResult.Done;
			}

			CSequenceController.currentConfigIndex++;
			return Start();
		}

		private static EProcessResult ProcessTile(string pTilePath, int pTileIndex)
		{
			//has to reinit first for the correct progress output
			CDebug.ReInit();

			DateTime startTime = DateTime.Now;
			currentTileIndex = pTileIndex;

			if(CDebugData.BallsDebug())
				return EProcessResult.Done;

			List<Tuple<EClass, Vector3>> parsedLines = new List<Tuple<EClass, Vector3>>();

			string[] lines = CProgramLoader.GetFileLines(pTilePath);
			bool linesOk = lines != null && lines.Length > 0 && !string.IsNullOrEmpty(lines[0]);
			if(linesOk && CHeaderInfo.HasHeader(lines[0]))
			{
				//todo: where to init header?
				CProjectData.currentTileHeader = new CHeaderInfo(lines);
			}
			else
			{
				const string noHeaderMsg = "Processed tile has no header";
				CDebug.Error(noHeaderMsg);
				throw new Exception(noHeaderMsg);
			}
			parsedLines = CProgramLoader.ParseLines(lines, true);

			//has to be called after currentTileHeader is assigned
			CProjectData.ReInit(pTileIndex); //has to reinit after each tile is processed
			CTreeManager.Reinit();

			if(CProjectData.backgroundWorker.CancellationPending)
				return EProcessResult.Cancelled;

			bool result = CProgramLoader.ProcessParsedLines(parsedLines);
			bool debug_allowExport = false;
			//allow export of tiles where no ball has been detected
			bool exportUndetected = CParameterSetter.GetBoolSettings(ESettings.exportUndetected);
			if(CRxpParser.IsRxp)
			{
				//cant rename folder while it is being used
				/*if(result)
				{
					const string ball_ext = "_ball";
					Directory.Move(CProjectData.outputTileSubfolder, CProjectData.outputTileSubfolder + ball_ext);
					CProjectData.outputTileSubfolder += ball_ext;
				}
				else */
				//dont export tile where no ball was detected
				if(!result && !debug_allowExport && !exportUndetected)
				{
					//no ball was detected => delete folder and continue
					Directory.Delete(CProjectData.outputTileSubfolder);
					CDebug.WriteLine("No ball detected in tile " + pTileIndex);
					return EProcessResult.Done;
				}
			}

			if(CProjectData.backgroundWorker.CancellationPending)
				return EProcessResult.Cancelled;

			CTreeManager.DebugTrees();

			CDebug.Step(EProgramStep.Export3D);
			CObjPartition.ExportPartition("", "tile" + pTileIndex);

			if(CProjectData.backgroundWorker.CancellationPending)
				return EProcessResult.Cancelled;

			//has to be called after ExportPartition where final folder location is determined
			try
			{
				CDebug.Step(EProgramStep.Bitmap);
				CBitmapExporter.Export(pTileIndex);
			}
			catch(Exception e)
			{
				CDebug.Error("exception: " + e.Message);
			}

			CAnalytics.totalDuration = CAnalytics.GetDuration(startTime);
			CDebug.Duration("total time", startTime);

			CDebug.Step(EProgramStep.Dart);
			CDartTxt.ExportTile();

			CDebug.Step(EProgramStep.Shp);
			CShpController.ExportCurrent();

			CDebug.Step(EProgramStep.Las);
			CLasExporter.ExportTile();

			CDebug.Step(EProgramStep.Analytics);
			CAnalytics.Write(true);
			
			return EProcessResult.Done;
		}

		public static void OnException()
		{
			CDebug.Step(EProgramStep.Exception);
		}
	}
}