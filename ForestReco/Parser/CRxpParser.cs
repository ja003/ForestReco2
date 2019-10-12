using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public static class CRxpParser
	{
		public static bool IsRxp => CParameterSetter.GetStringSettings(ESettings.forestFileExtension) == ".rxp";

		public static string[] GetDebugHeaderLines()
		{
			string[] result = new string[19];
			for(int i = 0; i <= 14; i++)
			{
				result[i] = "%";
			}

			result[15] = "% scale factor x y z         1 1 1";
			result[16] = "% offset x y z               0 0 0";
			result[17] = "% min x y z               0 0 0";
			result[18] = "% max x y z               0 0 0";
			//result[19] = "-20.10000	-3.21350	631.06150	0";
			return result;
		}

		public static string[] GetFileLines(string pFile)
		{
			int sync_to_pps = 0;
			IntPtr h3ds = IntPtr.Zero;

			int opened = scanifc_point3dstream_open_with_logging(pFile, ref sync_to_pps, "log.txt", ref h3ds);
			Console.WriteLine($"opened = {opened}, h3ds = {h3ds}");

			uint PointCount = 1;
			int EndOfFrame = 1;
			const uint BLOCK_SIZE = 1000;
			scanifc_xyz32[] BufferXYZ = new scanifc_xyz32[BLOCK_SIZE];
			scanifc_attributes[] BufferMISC = new scanifc_attributes[BLOCK_SIZE];
			ulong[] BufferTIME = new ulong[BLOCK_SIZE];

			List<string> fileLines = new List<string>();

			fileLines.AddRange(GetDebugHeaderLines().ToList<string>());

			while(PointCount != 0 || EndOfFrame != 0)
			{
				int read = scanifc_point3dstream_read(
							h3ds, BLOCK_SIZE,
							BufferXYZ, BufferMISC, BufferTIME,
							ref PointCount, ref EndOfFrame);
				for(int i = 0; i < PointCount; i++)
				{
					scanifc_xyz32 xyz = BufferXYZ[i];
					Console.WriteLine($"BufferXYZ = {xyz.x},{xyz.y},{xyz.z}");

					fileLines.Add(xyz.ToString());
				}
			}
					   
			return fileLines.ToArray();
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
