using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ObjParser.Types
{
	public class Face : IType
	{
		public const int MinimumDataLength = 4;
		public const string Prefix = "f";

		public string UseMtl { get; set; }
		public int[] VertexIndexList { get; set; }
		public int[] TextureVertexIndexList { get; set; }

		public Face() { }

		/// <summary>
		/// My own constructor.
		/// </summary>
		public Face(List<Vertex> pVertices)
		{
			VertexIndexList = new int[pVertices.Count];
			TextureVertexIndexList = new int[0];
			for (int i = 0; i < pVertices.Count; i++)
			{
				VertexIndexList[i] = pVertices[i].Index;
			}
		}

		public void LoadFromStringArray(string[] data)
		{
			if (data.Length < MinimumDataLength)
				throw new ArgumentException("Input array must be of minimum length " + MinimumDataLength, "data");

			if (!data[0].ToLower().Equals(Prefix))
				throw new ArgumentException("Data prefix must be '" + Prefix + "'", "data");

			int vcount = data.Count() - 1;
			VertexIndexList = new int[vcount];
			TextureVertexIndexList = new int[vcount];

			bool success;

			for (int i = 0; i < vcount; i++)
			{
				string[] parts = data[i + 1].Split('/');

				int vindex;
				success = int.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out vindex);
				if (!success) throw new ArgumentException("Could not parse parameter as int");
				VertexIndexList[i] = vindex;

				if (parts.Count() > 1)
				{
					success = int.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out vindex);
					if (success)
					{
						TextureVertexIndexList[i] = vindex;
					}
				}
			}
		}

		/// <summary>
		/// pVertexIndexOffset = my own implementation
		/// </summary>
		public override string ToString()
		{
			return ToString();
		}

		// HACKHACK this will write invalid files if there are no texture vertices in
		// the faces, need to identify that and write an alternate format
		public string ToString(int pVertexIndexOffset = 0)
		{
			StringBuilder b = new StringBuilder();
			b.Append("f");

			for (int i = 0; i < VertexIndexList.Count(); i++)
			{
				/*if (i < TextureVertexIndexList.Length)
				{
					b.AppendFormat(" {0}/{1}",
						VertexIndexList[i] + pVertexIndexOffset,
						TextureVertexIndexList[i] + pVertexIndexOffset);
				}
				else
				{
					b.AppendFormat(" {0}", VertexIndexList[i] + pVertexIndexOffset);
				}*/
				b.AppendFormat(" {0}", VertexIndexList[i] + pVertexIndexOffset);
			}

			return b.ToString();
		}
	}
}
