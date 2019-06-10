using System;
using System.Globalization;
using System.Numerics;

namespace ForestReco
{
	public class CHeaderInfo
	{
		public Vector3 ScaleFactor;
		public Vector3 Offset { get; private set; }
		public Vector3 Min; //used in project (Y = elevation), moved by offset
		public Vector3 Max;

		public Vector3 Min_orig; //values from input forest file
		public Vector3 Max_orig;

		public Vector3 BotLeftCorner => new Vector3(Min.X, Min.Y, 0);
		public Vector3 TopRightCorner => new Vector3(Max.X, Max.Y, 0);
		public Vector3 TopLeftCorner => new Vector3(Min.X, Max.Y, 0);
		public Vector3 Center => (BotLeftCorner + TopRightCorner) / 2;
		public float MinHeight => Min.Z;
		public float MaxHeight => Max.Z;
		public float Width => TopRightCorner.X - BotLeftCorner.X; //of array
		public float Height => TopRightCorner.Y - BotLeftCorner.Y; //of array

		public CHeaderInfo(string[] lines)
		{
			string pScaleFactorLine = GetLineContaining(lines, EHeaderAttribute.Scale);
			string pOffsetLine = GetLineContaining(lines, EHeaderAttribute.Offset);
			string pMinLine = GetLineContaining(lines, EHeaderAttribute.Min);
			string pMaxLine = GetLineContaining(lines, EHeaderAttribute.Max);
            if(pScaleFactorLine.Length == 0)
                throw new Exception($"Invalid header line pScaleFactorLine");
            if(pOffsetLine.Length == 0)
                throw new Exception($"Invalid header line pOffsetLine");
            if(pMinLine.Length == 0)
                throw new Exception($"Invalid header line pMinLine");
            if(pMaxLine.Length == 0)
                throw new Exception($"Invalid header line pMaxLine");

            ScaleFactor = ParseLineVector3(pScaleFactorLine);
			Offset = ParseLineVector3(pOffsetLine);
			//Offset.Z = 0; //given Z offset will not be used
			Min_orig = ParseLineVector3(pMinLine);
			Min = Min_orig - Offset;
			Max_orig = ParseLineVector3(pMaxLine);
			Max = Max_orig - Offset;
			//transfer to format Y = height //DONT
			//float tmpY = Min.Y;
			//Min.Y = Min.Z;
			//Min.Z = tmpY;
			//tmpY = Max.Y;
			//Max.Y = Max.Z;
			//Max.Z = tmpY;

			if (Min == Vector3.Zero && Max == Vector3.Zero)
			{
				CDebug.WriteLine("Invalid header. Creating default header.");
				const int defaultArraySize = 15;
				Min = new Vector3(-defaultArraySize, -defaultArraySize, 0);
				Max = new Vector3(defaultArraySize, defaultArraySize, 0);
				Offset = Vector3.Zero;
			}
		}

		/*private string GetLine(string[] lines, EHeaderAttribute pAttribute)
		{
			switch (pAttribute)
			{
				case EHeaderAttribute.Scale: return lines[15];
				case EHeaderAttribute.Offset: return lines[16];
				case EHeaderAttribute.Min: return lines[17];
				case EHeaderAttribute.Max: return lines[18];
			}
			return "";
		}*/

		private string GetLineContaining(string[] pLines, EHeaderAttribute pKey)
		{
			for(int i = 0; i < pLines.Length; i++)
			{
				string line = pLines[i];
				if(line.Contains(GetHeaderAttributeKeyString(pKey)))
					return line;
			}
			return "";
		}

		private string GetHeaderAttributeKeyString(EHeaderAttribute pKey){
			switch(pKey)
			{
				case EHeaderAttribute.Scale:
					return "scale factor x y z";
				case EHeaderAttribute.Offset:
					return "offset x y z";
				case EHeaderAttribute.Min:
					return "min x y z";
				case EHeaderAttribute.Max:
					return "max x y z";
			}
			return "";
		}

		public enum EHeaderAttribute
		{
			Scale,
			Offset,
			Min,
			Max
		}


		private Vector3 ParseLineVector3(string pLine)
		{
			if (string.IsNullOrEmpty(pLine)) { return Vector3.Zero;}

			string[] split = pLine.Split(null);
			int length = split.Length;
			//											X					Y					Z
			return CLazTxtParser.ParseHeaderVector3(split[length - 3], split[length - 2], split[length - 1]);
		}

		public override string ToString()
		{
			return "ScaleFactor: " + ScaleFactor + "\nOffset: " + Offset + "\nMin: " + Min + "\nMax: " + Max;
		}

		/// <summary>
		/// First line of file with header should look like:
		/// % file signature:            'LASF'
		/// </summary>
		public static bool HasHeader(string pFirstLine)
		{
			string[] split = pFirstLine.Split(null);
			string firstSign = split[0];
			bool result = string.Equals(firstSign, "%") || string.Equals(firstSign, "#");
			return result;
		}
	}
}