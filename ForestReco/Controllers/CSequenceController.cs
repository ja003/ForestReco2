using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace ForestReco
{
	/// <summary>
	/// NormalF format:
	/// path
	/// avg tree height
	/// tree extent
	/// extent multiply
	/// 
	/// RXP format:
	/// path
	/// </summary>
	public static class CSequenceController
	{
		public const string SEQ_EXTENSION = ".seq";

		private static int oneSequenceLength = 5;

		public static List<SSequenceConfig> configs;

		public static int currentConfigIndex;

		public static void SetValues()
		{
			if(configs.Count == 0) { return; }

			//CDebug.WriteLine("SetValues from config");
			CDebug.WriteLine($"================================\n" +
				$"Start processing sequence {currentConfigIndex + 1}/{configs.Count}" +
				$"\n================================", true, true);

			SSequenceConfig currentConfig = configs[currentConfigIndex];
			CParameterSetter.SetParameter(ESettings.forestFileFullName, currentConfig.path);
			int treeHeight = currentConfig.treeHeight;

			CParameterSetter.SetParameter(ESettings.autoAverageTreeHeight, treeHeight <= 0);
			if(treeHeight > 0)
			{
				CParameterSetter.SetParameter(ESettings.avgTreeHeigh, treeHeight);
			}
			CParameterSetter.SetParameter(ESettings.treeExtent, currentConfig.treeExtent);
			CParameterSetter.SetParameter(ESettings.treeExtentMultiply, currentConfig.treeExtentMultiply);

			CBallsManager.useConfigDebugData = false;
			CBallsManager.configBallCenters.Clear();
			foreach(Vector3 ballCenter in configs[currentConfigIndex].ballCenters)
			{
				CBallsManager.configBallCenters.Add(ballCenter);
				CBallsManager.useConfigDebugData = true;
			}
		}

		public static void OnLastSequenceEnd()
		{
			CDebug.WriteLine("LAST SEQUENCE DONE", true, true);

			if(string.IsNullOrEmpty(lastSequenceFile)) { return; }

			//set the processed sequence file back as a forest file
			CParameterSetter.SetParameter(ESettings.forestFileFullName, lastSequenceFile);
		}

		private static string lastSequenceFile;

		public static void Init()
		{
			configs = new List<SSequenceConfig>();
			currentConfigIndex = 0;
			if(!IsSequence()) { return; }

			oneSequenceLength = CTreeManager.GetDetectMethod() ==
				EDetectionMethod.Balls ? 2 : 5;
			//expected format for rxp:
			//1) full file path 1
			//2) center1xf, center1yf, center1zf;center2xf, center2yf, center2zf;... 
			//		(if none, has to be empty)
			//3) full file path 2
			//...

			lastSequenceFile = CParameterSetter.GetStringSettings(ESettings.forestFileFullName);
			CDebug.WriteLine($"Setting file from config: {lastSequenceFile}");

			string[] lines = File.ReadAllLines(lastSequenceFile);

			for(int i = 0; i < lines.Length; i += oneSequenceLength)
			{
				string[] configLines = new string[oneSequenceLength];
				for(int j = 0; j < oneSequenceLength; j++)
				{
					if(i + j >= lines.Length)
						continue;
					configLines[j] = lines[i + j];
				}
				SSequenceConfig config = GetConfig(configLines);
				configs.Add(config);
			}
		}

		private static SSequenceConfig GetConfig(string[] pLines)
		{
			SSequenceConfig config = new SSequenceConfig();
			config.path = GetValue(pLines[0]);
			CDebug.WriteLine($"File: {config.path}", true);

			if(CRxpParser.IsRxp)
			{
				config.ballCenters = ParseBallCenters(pLines[1]);
				return config;
			}

			if(pLines.Length == 1)
			{
				config.treeHeight =
					CParameterSetter.GetIntSettings(ESettings.avgTreeHeigh);
				config.treeExtent =
					CParameterSetter.GetFloatSettings(ESettings.treeExtent);
				config.treeExtentMultiply =
					CParameterSetter.GetFloatSettings(ESettings.treeExtentMultiply);
				return config;
			}
			config.treeHeight = int.Parse(GetValue(pLines[1]));
			config.treeExtent = float.Parse(GetValue(pLines[2]));
			config.treeExtentMultiply = float.Parse(GetValue(pLines[3]));
			return config;
		}

		/// <summary>
		/// Expected format: center1xf, center1yf, center1zf
		/// </summary>
		private static List<Vector3> ParseBallCenters(string pCentersLine)
		{
			CDebug.WriteLine($"Parsing ball centers");

			List<Vector3> ballCenters = new List<Vector3>();
			if(pCentersLine == null || pCentersLine.Length == 0)
				return ballCenters;

			string[] centers = pCentersLine.Split(';');
			for(int i = 0; i < centers.Length; i++)
			{
				string[] coords = centers[i].Split(',');
				if(coords.Length < 2)
				{
					string parsedStr = centers[i].Length == 0 ? "empty" : centers[i];
					CDebug.Warning($"Invalid format of parsed coordinates: {parsedStr}. Skipping");
					continue;
				}

				//cant parse float with 'f'
				float x = float.Parse(coords[0].Replace('f', '0'));
				float y = float.Parse(coords[1].Replace('f', '0'));
				float z = float.Parse(coords[2].Replace('f', '0'));
				Vector3 center = new Vector3(x, y, z);
				ballCenters.Add(center);
				CDebug.WriteLine($"- add center {center}");
			}

			return ballCenters;
		}

		private static string GetValue(string pLine)
		{
			string[] split = pLine.Split('=');
			return split.Last();
		}

		public static bool IsSequence()
		{
			string mainFile = CParameterSetter.GetStringSettings(ESettings.forestFileFullName);
			if(!File.Exists(mainFile)) { return false; }
			return Path.GetExtension(mainFile) == SEQ_EXTENSION;
		}

		public static bool IsLastSequence()
		{
			return configs.Count == 0 || currentConfigIndex == configs.Count - 1;
		}

	}

	public struct SSequenceConfig
	{
		public string path;
		public int treeHeight;
		public float treeExtent;
		public float treeExtentMultiply;
		public List<Vector3> ballCenters;
	}
}