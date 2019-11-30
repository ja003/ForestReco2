using System;

namespace ForestReco
{
	public struct SSplitRange
	{
		public float MinX;
		public float MinY;
		public float MaxX;
		public float MaxY;

		public float RangeX => MaxX - MinX;
		public float RangeY => MaxY - MinY;

		public SSplitRange(float minX, float minY, float maxX, float maxY)
		{
			MinX = minX;
			MinY = minY;
			MaxX = maxX;
			MaxY = maxY;
		}

		public bool IsValid()
		{
			bool xOk = MinX < MaxX;
			bool yOk = MinY < MaxY;
			return xOk && yOk;
		}

		public string ToStringXmin()
		{
			return $"{MinX.ToString("0.0")}";
		}

		public string ToStringXmax()
		{
			return $"{MaxX.ToString("0.0")}";
		}

		public string ToStringX()
		{
			return $"[{ToStringXmin()}] - [{ToStringXmax()}]";
		}

		public string ToStringYmin()
		{
			return $"{MinY.ToString("0.0")}";
		}

		public string ToStringYmax()
		{
			return $"{MaxY.ToString("0.0")}";
		}

		public string ToStringY()
		{
			return $"[{ToStringYmin()}] - [{ToStringYmax()}]";
		}

		public override string ToString()
		{
			return ToStringX() + "," + ToStringY();
		}

	}
}
