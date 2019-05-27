using System;
using System.Diagnostics;
using System.IO;

namespace ForestReco
{
	public static class CCmdController
	{
		private static string lasToolsFolder => CParameterSetter.GetStringSettings(ESettings.lasToolsFolderPath) + "\\";

		private static string ParseCommand(string pLasToolCommand){
			string[] cmdSplit = pLasToolCommand.Split(' ');
			if(cmdSplit.Length == 0)
				return "";
			string lasCommand = pLasToolCommand.Split(' ')[0];
			return lasCommand;
		}

		private static bool CanRunCommand(string pLasToolCommand)
		{
			string lasCommand = ParseCommand(pLasToolCommand);

			if(!Directory.Exists(lasToolsFolder))
			{
				//todo: make own exception
				return false;
				//throw new Exception("LasToolsFolder not found");
			}

			switch(lasCommand)
			{
				case "lasinfo":
				case "lasmerge":
				case "lastile":
				case "lasnoise":
				case "lasground":
                case "lasground_new":
				case "lasheight":
				case "lasclassify":
				case "lassplit":
				case "las2txt":
				case "lasclip":
					return File.Exists(lasToolsFolder + lasCommand + ".exe");
			}
			return false;
		}

		public static void RunLasToolsCmd(string pLasToolCommand, string pOutputFilePath)
		{
			if(!CanRunCommand(pLasToolCommand))
			{
				throw new Exception($"Cannot run command: {ParseCommand(pLasToolCommand)} {Environment.NewLine} {pLasToolCommand}");				
			}

			//string outputFilePath = tmpFolder + pOutputFilePath;
			bool outputFileExists = File.Exists(pOutputFilePath);
			CDebug.WriteLine($"file: {pOutputFilePath} exists = {outputFileExists}");

			if(!outputFileExists)
			{
				string command = "/C " + pLasToolCommand;

				//if(!Directory.Exists(lasToolsFolder))
				//{
				//	//todo: make own exception
				//	throw new Exception("LasToolsFolder not found");
				//}

				ProcessStartInfo processStartInfo = new ProcessStartInfo
				{
					WorkingDirectory = lasToolsFolder,
					FileName = "CMD.exe",
					Arguments = command					
				};

				Process currentProcess = Process.Start(processStartInfo);
				currentProcess.WaitForExit();

				int result = currentProcess.ExitCode;

				//todo: throw and handle exception?
				if(result == 1) //0 = OK, 1 = error...i.e. the .exe file is missing
				{
					throw new Exception($"Command {command} resulted in error");
				}
				// Check if command generated desired result
				outputFileExists = File.Exists(pOutputFilePath);
				if(!outputFileExists)
				{
					throw new Exception($"File {pOutputFilePath} not created");
				}
			}
		}
	}
}
