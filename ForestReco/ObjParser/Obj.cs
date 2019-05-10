using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ObjParser.Types;

namespace ObjParser
{
	public class Obj
	{
		private List<Vertex> vertexList;
		public List<Vertex> VertexList => vertexList;

		public List<Face> FaceList;
		public List<TextureVertex> TextureList;

		public Extent Size { get; set; }

		public string UseMtl { get; set; }
		public string Mtl { get; set; }

		//MY EXTENSION
		public Vector3 Position;
		public Vector3 Rotation;
		public Vector3 Scale = Vector3.One;
		public string Name;

		private bool isObjCentered;

		public SVertexTransform GetVertexTransform()
		{
			//return new SVertexTransform(Position, Scale, Rotation, Vector3.Zero);
			float xMid = (Size.XMin + Size.XMax) / 2;
			float zMid = (Size.ZMin + Size.ZMax) / 2;
			Vector3 referenceVertex = isObjCentered ? Vector3.Zero : new Vector3(xMid, Size.YMin, zMid);
			return new SVertexTransform(Position, Scale, Rotation, referenceVertex);
		}


		/// <summary>
		/// Constructor. Initializes VertexList, FaceList and TextureList.
		/// </summary>
		public Obj(string pName)
		{
			vertexList = new List<Vertex>();
			FaceList = new List<Face>();
			TextureList = new List<TextureVertex>();
			Name = pName;
		}

		/// <summary>
		/// Load .obj from a filepath.
		/// </summary>
		public void LoadObj(string path)
		{
			LoadObj(File.ReadAllLines(path));
			isObjCentered = true; //we count on that all loaded models are centered
		}

		/// <summary>
		/// Load .obj from a stream.
		/// </summary>
		/// <param name="file"></param>
		public void LoadObj(Stream data)
		{
			using (var reader = new StreamReader(data))
			{
				LoadObj(reader.ReadToEnd().Split(Environment.NewLine.ToCharArray()));
			}
		}

		/// <summary>
		/// Load .obj from a list of strings.
		/// </summary>
		/// <param name="data"></param>
		public void LoadObj(IEnumerable<string> data)
		{
			foreach (var line in data)
			{
				processLine(line);
			}

			updateSize();
		}

		public void WriteObjFile(string path, string[] headerStrings)
		{
			using (var outStream = File.OpenWrite(path))
			using (var writer = new StreamWriter(outStream))
			{
				// Write some header data
				WriteHeader(writer, headerStrings);

				if (!string.IsNullOrEmpty(Mtl))
				{
					writer.WriteLine("mtllib " + Mtl);
				}

				vertexList.ForEach(v => writer.WriteLine(v));
				TextureList.ForEach(tv => writer.WriteLine(tv));
				string lastUseMtl = "";
				foreach (Face face in FaceList)
				{
					if (face.UseMtl != null && !face.UseMtl.Equals(lastUseMtl))
					{
						writer.WriteLine("usemtl " + face.UseMtl);
						lastUseMtl = face.UseMtl;
					}
					writer.WriteLine(face);
				}
			}
		}

		private void WriteHeader(StreamWriter writer, string[] headerStrings)
		{
			if (headerStrings == null || headerStrings.Length == 0)
			{
				writer.WriteLine("# Generated by ObjParser");
				return;
			}

			foreach (var line in headerStrings)
			{
				writer.WriteLine("# " + line);
			}
		}

		public void AddVertex(Vertex pVertex)
		{
			vertexList.Add(pVertex);
			UpdateSize(pVertex);
		}

		public void UpdateSize(Vertex pVertex)
		{
			if (Size == null)
			{
				Size = new Extent
				{
					XMax = pVertex.X,
					XMin = pVertex.X,
					YMax = pVertex.Y,
					YMin = pVertex.Y,
					ZMax = pVertex.Z,
					ZMin = pVertex.Z
				};
				return;
			}

			if (pVertex.X > Size.XMax) { Size.XMax = pVertex.X; }
			if (pVertex.X < Size.XMin) { Size.XMin = pVertex.X; }
			if (pVertex.Y > Size.YMax) { Size.YMax = pVertex.Y; }
			if (pVertex.Y < Size.YMin) { Size.YMin = pVertex.Y; }
			if (pVertex.Z > Size.ZMax) { Size.ZMax = pVertex.Z; }
			if (pVertex.Z < Size.ZMin) { Size.ZMin = pVertex.Z; }
		}
		
		/// <summary>
		/// Sets our global object size with an extent object
		/// </summary>
		private void updateSize()
		{
			// If there are no vertices then size should be 0.
			if (vertexList.Count == 0)
			{
				Size = new Extent
				{
					XMax = 0,
					XMin = 0,
					YMax = 0,
					YMin = 0,
					ZMax = 0,
					ZMin = 0
				};

				// Avoid an exception below if VertexList was empty.
				return;
			}

			Size = new Extent
			{
				XMax = vertexList.Max(v => v.X),
				XMin = vertexList.Min(v => v.X),
				YMax = vertexList.Max(v => v.Y),
				YMin = vertexList.Min(v => v.Y),
				ZMax = vertexList.Max(v => v.Z),
				ZMin = vertexList.Min(v => v.Z)
			};
		}

		/// <summary>
		/// Parses and loads a line from an OBJ file.
		/// Currently only supports V, VT, F and MTLLIB prefixes
		/// </summary>		
		private void processLine(string line)
		{
			string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length > 0)
			{
				switch (parts[0])
				{
					case "usemtl":
						UseMtl = parts[1];
						break;
					case "mtllib":
						Mtl = parts[1];
						break;
					case "v":
						Vertex v = new Vertex();
						v.LoadFromStringArray(parts);
						vertexList.Add(v);
						v.Index = vertexList.Count();
						break;
					case "f":
						Face f = new Face();
						f.LoadFromStringArray(parts);
						f.UseMtl = UseMtl;
						FaceList.Add(f);
						break;
					case "vt":
						TextureVertex vt = new TextureVertex();
						vt.LoadFromStringArray(parts);
						TextureList.Add(vt);
						vt.Index = TextureList.Count();
						break;

				}
			}
		}

		public Obj Clone()
		{
			Obj clone = new Obj(Name)
			{
				vertexList = vertexList,
				FaceList = FaceList,
				TextureList = TextureList,
				Position = Position,
				Rotation = Rotation,
				Scale = Scale,
				Size = Size,
				Mtl = Mtl,
				UseMtl = UseMtl,
				isObjCentered = isObjCentered
			};
			return clone;
		}

		public override string ToString()
		{
			return Name + "[" + Position + "]" + vertexList.Count;
		}

		public int GetNextVertexIndex()
		{
			return vertexList.Count + 1;
		}
	}
}
