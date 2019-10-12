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

		public static string[] GetFileLinesDebug(string pFile)
		{
			string[] result = new string[20];
			for(int i = 0; i <= 14; i++)
			{
				result[i] = "%";
			}

			result[15] = "% scale factor x y z         1 1 1";
			result[16] = "% offset x y z               0 0 0";
			result[17] = "% min x y z               0 0 0";
			result[18] = "% max x y z               0 0 0";
			result[19] = "-20.10000	-3.21350	631.06150	0";
			return result;
		}

		public static string[] GetFileLines(string pFile)
		{
			int sync_to_pps = 0;
			IntPtr h3ds = IntPtr.Zero;

			int opened = scanifc_point3dstream_open_with_logging(pFile, ref sync_to_pps, "log.txt", ref h3ds);
			Console.WriteLine($"opened = {opened}, h3ds = {h3ds}");

			return GetFileLinesDebug(pFile);
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
