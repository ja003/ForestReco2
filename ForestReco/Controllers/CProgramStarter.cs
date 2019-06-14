﻿using System;
using System.Collections.Generic;
using System.Globalization;
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
			CShpController.Init();
			CReftreeManager.Init();

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
				List<string> tiledFiles = CProgramLoader.GetTiledPreprocessedFilePaths();

				tilesCount = tiledFiles.Count;
				foreach(string tiledFilePath in tiledFiles)
				{
					EProcessResult tileProcess = ProcessTile(tiledFilePath, tiledFiles.IndexOf(tiledFilePath));
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

			if(CProjectData.backgroundWorker.CancellationPending)
			{
				CDebug.Step(EProgramStep.Cancelled);
				return EProcessResult.Cancelled;
			}

			CDartTxt.ExportMain();
			CLasExporter.ExportMain();
			CShpController.ExportMain();
			CBitmapExporter.ExportMain();


			CDebug.Step(EProgramStep.Done);

			if(CSequenceController.IsLastSequence())
			{
				CSequenceController.OnLastSequenceEnd();
				return EProcessResult.Done;
			}

			CSequenceController.currentConfigIndex++;
			return Start();
		}

		private static EProcessResult ProcessTile(string pTilePath, int pTileIndex)
		{
			DateTime startTime = DateTime.Now;
			currentTileIndex = pTileIndex;
			CProjectData.ReInit(pTileIndex); //has to reinit after each tile is processed
			CTreeManager.Reinit();
			CDebug.ReInit();

			string[] lines = CProgramLoader.GetFileLines(pTilePath);

			if(CProjectData.backgroundWorker.CancellationPending)
			{ return EProcessResult.Cancelled; }

			bool linesOk = lines != null && lines.Length > 0 && !string.IsNullOrEmpty(lines[0]);
			if(linesOk && CHeaderInfo.HasHeader(lines[0]))
			{
				//todo: where to init header?
				CProjectData.currentTileHeader = new CHeaderInfo(lines);
			}
			else
			{
				CDebug.Error("No header is defined");
				throw new Exception("No header is defined");
			}

			if(CProjectData.backgroundWorker.CancellationPending)
			{ return EProcessResult.Cancelled; }

			List<Tuple<EClass, Vector3>> parsedLines =
				CProgramLoader.ParseLines(lines, CProjectData.mainHeader != null, true);

			CProgramLoader.ProcessParsedLines(parsedLines);

			if(CProjectData.backgroundWorker.CancellationPending)
			{ return EProcessResult.Cancelled; }


			//has to be called after array initialization
			CCheckTreeManager.Init(); //todo: has to be inited for every tile...not good: fix
			if(CProjectData.backgroundWorker.CancellationPending)
				return EProcessResult.Cancelled;

			CTreeManager.DebugTrees();

			CDebug.Step(EProgramStep.Export);
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

			CAnalytics.Write(true);

			CDartTxt.ExportTile();
			CShpController.ExportCurrent();
			CLasExporter.ExportTile();

			return EProcessResult.Done;
		}

		public static void OnException()
		{
			CDebug.Step(EProgramStep.Exception);
		}
	}
}