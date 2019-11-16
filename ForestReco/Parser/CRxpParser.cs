using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public static class CRxpParser
	{
		public static bool IsRxp =>
			CParameterSetter.GetStringSettings(ESettings.forestFileExtension) == ".rxp" ||
			CTreeManager.GetDetectMethod() == EDetectionMethod.Balls;

		public const uint READ_BLOCK_SIZE = 100000;
		public const int EXPECTED_RXP_FILE_LENGTH = 1000 * (int)READ_BLOCK_SIZE;
		//private const int EXPECTED_PARSE_ITERATIONS = (int)(EXPECTED_RXP_FILE_LENGTH / READ_BLOCK_SIZE);

		//public static string[] GetDebugHeaderLines()
		//{
		//	string[] result = new string[19];
		//	for(int i = 0; i <= 14; i++)
		//	{
		//		result[i] = "%";
		//	}

		//	result[15] = "% scale factor x y z         1 1 1";
		//	result[16] = "% offset x y z               0 0 0";
		//	result[17] = "% min x y z               0 0 0";
		//	result[18] = "% max x y z               0 0 0";
		//	//result[19] = "-20.10000	-3.21350	631.06150	0";
		//	return result;
		//}

		static string currentFilePath;
		public static IntPtr OpenFile(string pFile)
		{
			IntPtr h3ds = IntPtr.Zero;
			currentFilePath = pFile;
			int sync_to_pps = 0;
			int opened = scanifc_point3dstream_open(pFile, ref sync_to_pps, ref h3ds);
			Console.WriteLine($"opened = {opened}, h3ds = {h3ds}");
			return h3ds;
		}

		//TODO: remove all choosable arguments, move filtering to separate logic
		public static CRxpInfo ParseFile(IntPtr pHandler, int pMaxLoadPoints = -1)
		{
			uint PointCount = 1;
			int EndOfFrame = 1;
			//10 000 => 14s
			//100 000 => 7s
			//1 000 000 => 7,5s
			scanifc_xyz32[] BufferXYZ = new scanifc_xyz32[READ_BLOCK_SIZE];
			scanifc_attributes[] BufferMISC = new scanifc_attributes[READ_BLOCK_SIZE];
			ulong[] BufferTIME = new ulong[READ_BLOCK_SIZE];

			List<Tuple<EClass, Vector3>> fileLines = new List<Tuple<EClass, Vector3>>();

			//fileLines.AddRange(GetDebugHeaderLines().ToList<string>());

			Vector3 min = new Vector3(int.MaxValue, int.MaxValue, int.MaxValue);
			Vector3 max = new Vector3(int.MinValue, int.MinValue, int.MinValue);

			int readIteration = 0;
			DateTime debugStart = DateTime.Now;

			int partIndex = 0;
			int iteration = 0;
			while(PointCount != 0 || EndOfFrame != 0)
			{
				if(CProjectData.backgroundWorker.CancellationPending)
					break;

				readIteration++;
				int read = scanifc_point3dstream_read(
							pHandler, READ_BLOCK_SIZE,
							BufferXYZ,
							//BufferMISC, BufferTIME,
							null, null, //no need for this info
							ref PointCount, ref EndOfFrame);
				for(int i = 0; i < PointCount; i++)
				{
					scanifc_xyz32 xyz = BufferXYZ[i];
					fileLines.Add(new Tuple<EClass, Vector3>(EClass.Undefined, xyz.ToVector()));
					//Console.WriteLine($"BufferXYZ = {xyz.x},{xyz.y},{xyz.z}");
				}

				iteration++;

				if(pMaxLoadPoints > 0 && fileLines.Count > pMaxLoadPoints)
				{
					break;
				}
			}

			CDebug.WriteLine($"ParseFile took {(DateTime.Now - debugStart).TotalSeconds}");

			CHeaderInfo header = new CHeaderInfo(new Vector3(1, 1, 1), new Vector3(0, 0, 0), min, max);

			bool readFinished = PointCount == 0 && EndOfFrame == 0;
			CRxpInfo rxpInfo = new CRxpInfo(fileLines, header, readFinished);

			return rxpInfo;
		}

		private static bool IsInAngle(Vector3 pPoint, int pMinAngle, int pMaxAngle)
		{
			float angle = CUtils.GetAngle(Vector2.UnitX, new Vector2(pPoint.X, pPoint.Y));
			return angle >= pMinAngle && angle < pMaxAngle;
		}

		private static void RefreshMinMax(scanifc_xyz32 pXYZ, ref Vector3 pMin, ref Vector3 pMax)
		{
			if(pXYZ.x < pMin.X)
				pMin.X = pXYZ.x;
			if(pXYZ.y < pMin.Y)
				pMin.Y = pXYZ.y;
			if(pXYZ.z < pMin.Z)
				pMin.Z = pXYZ.z;

			if(pXYZ.x > pMax.X)
				pMax.X = pXYZ.x;
			if(pXYZ.y > pMax.Y)
				pMax.Y = pXYZ.y;
			if(pXYZ.z > pMax.Z)
				pMax.Z = pXYZ.z;
		}

		[DllImport("scanifc-mt.dll")]
		static extern int scanifc_point3dstream_read(
		IntPtr h3ds,
		uint want,
		[Out] scanifc_xyz32[] pxyz32,
		[Out] scanifc_attributes[] pattributes,
		[Out] ulong[] ptime,
		ref uint got,
		ref int end_of_frame);

		[DllImport("scanifc-mt.dll")]
		static extern int scanifc_get_library_version(
		ref ushort major,
		ref ushort minor,
		ref ushort build);

		[DllImport("scanifc-mt.dll")]
		static extern int scanifc_point3dstream_open(
			[MarshalAs(UnmanagedType.LPStr)] string uri,
			ref int sync_to_pps,
			ref IntPtr h3ds);

		[DllImport("scanifc-mt.dll", CharSet = CharSet.Ansi)]
		static extern int scanifc_point3dstream_open_with_logging(
			[MarshalAs(UnmanagedType.LPStr)] string uri,
			ref int sync_to_pps,
			[MarshalAs(UnmanagedType.LPStr)] string log,
			ref IntPtr h3ds);
	}

	public class CRxpInfo
	{
		public bool ReadFinished = false;

		public List<Tuple<EClass, Vector3>> ParsedLines { get; private set; }
		public CHeaderInfo Header { get; private set; } //not used now - todo: remove?

		public CRxpInfo()
		{

		}

		public CRxpInfo(List<Tuple<EClass, Vector3>> pParsedLines, CHeaderInfo pHeader, bool pReadFinished)
		{
			ParsedLines = pParsedLines;
			Header = pHeader;
			ReadFinished = pReadFinished;
		}
	}


	[StructLayout(LayoutKind.Sequential)]
	struct scanifc_xyz32
	{
		public float x;
		public float y;
		public float z;

		public override string ToString()
		{
			return $"{x} {y} {z} 0";
		}

		public Vector3 ToVector()
		{
			return new Vector3(x, y, z);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	struct scanifc_attributes
	{
		public float amplitude;
		public float reflectance;
		public ushort deviation;
		public ushort flags;
		public float background_radiation;
	}
}
