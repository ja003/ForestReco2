using System;
using System.Numerics;

namespace ForestReco
{
	public class CHeaderInfo
	{
		public Vector3 ScaleFactor;
		public Vector3 Offset { get; private set; }
		public Vector3 Min; //used in project (Z = elevation), moved by offset
		public Vector3 Max;

		public CVector3D Min_orig; //values from input forest file
		public CVector3D Max_orig;

		public Vector3 BotLeftCorner => new Vector3(Min.X, Min.Y, 0);
		public Vector3 BotRightCorner => new Vector3(Max.X, Min.Y, 0);
		public Vector3 TopRightCorner => new Vector3(Max.X, Max.Y, 0);
		public Vector3 TopLeftCorner => new Vector3(Min.X, Max.Y, 0);
		public Vector3 Center => (BotLeftCorner + TopRightCorner) / 2;
		public float MinHeight => Min.Z;
		public float MaxHeight => Max.Z;
		public float Width => TopRightCorner.X - BotLeftCorner.X; //of array
		public float Height => TopRightCorner.Y - BotLeftCorner.Y; //of array

		public CHeaderInfo()
		{

		}

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

			ScaleFactor = (Vector3)ParseLineVector3(pScaleFactorLine);
			Offset = (Vector3)ParseLineVector3(pOffsetLine);
			//Offset.Z = 0; //given Z offset will not be used
			Min_orig = ParseLineVector3(pMinLine);
			CVector3D minDouble = Min_orig - Offset;
			Min = (Vector3)minDouble;
			Max_orig = ParseLineVector3(pMaxLine);
			CVector3D maxDouble = Max_orig - Offset;
			Max = (Vector3)maxDouble;

			if(Min == Vector3.Zero && Max == Vector3.Zero)
			{
				CDebug.Error("Invalid header. Creating default header.");
				const int defaultArraySize = 15;
				Min = new Vector3(-defaultArraySize, -defaultArraySize, 0);
				Max = new Vector3(defaultArraySize, defaultArraySize, 0);
				Offset = Vector3.Zero;
			}
		}

		/// <summary>
		/// Returns a string of the tile extent (original values - offset is applied).
		/// pShortFormat: [min.x-max.x.last3digits, min.y-max.y.last3digits]
		/// </summary>
		public string GetExtentString(bool pShortFormat = false)
		{
			float minXOrig = (Min + Offset).X;
			float maxXOrig = (Max + Offset).X;
			float minYOrig = (Min + Offset).Y;
			float maxYOrig = (Max + Offset).Y;
			if(!pShortFormat)
				return $"[{minXOrig}-{maxXOrig},{minYOrig}-{maxYOrig}]";
			return $"[{minXOrig}-{maxXOrig % 1000},{minYOrig}-{maxYOrig % 1000}]";
		}

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

		private string GetHeaderAttributeKeyString(EHeaderAttribute pKey)
		{
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


		private CVector3D ParseLineVector3(string pLine)
		{
			if(string.IsNullOrEmpty(pLine)) { return CVector3D.Zero; }

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