using System;
using System.Collections.Generic;
using System.IO;

namespace RNumerics
{
	/// <summary>
	/// SimpleTriangleMesh but for quads. Data packed into buffers, no dynamics.
	/// Supports Per-Vertex Normals, Colors, UV, and Per-Quad Facegroup.
	/// 
	/// use static WriteOBJ() to save. No loading, for now. 
	/// 
	/// </summary>
	public class SimpleQuadMesh
	{
		public DVector<double> Vertices;
		public DVector<float> Normals;
		public DVector<float> Colors;
		public DVector<float> UVs;

		public DVector<int> Quads;
		public DVector<int> FaceGroups;

		public SimpleQuadMesh() {
			Initialize();
		}

		public void Initialize(bool bWantNormals = true, bool bWantColors = true, bool bWantUVs = true, bool bWantFaceGroups = true) {
			Vertices = new DVector<double>();
			Normals = bWantNormals ? new DVector<float>() : null;
			Colors = bWantColors ? new DVector<float>() : null;
			UVs = bWantUVs ? new DVector<float>() : null;
			Quads = new DVector<int>();
			FaceGroups = bWantFaceGroups ? new DVector<int>() : null;
		}

		/*
         * Construction
         */
		public int AppendVertex(double x, double y, double z) {
			var i = Vertices.Length / 3;
			if (HasVertexNormals) {
				Normals.Add(0);
				Normals.Add(1);
				Normals.Add(0);
			}
			if (HasVertexColors) {
				Colors.Add(1);
				Colors.Add(1);
				Colors.Add(1);
			}
			if (HasVertexUVs) {
				UVs.Add(0);
				UVs.Add(0);
			}
			Vertices.Add(x);
			Vertices.Add(y);
			Vertices.Add(z);
			return i;
		}
		public int AppendVertex(Vector3d v) {
			return AppendVertex(v.x, v.y, v.z);
		}

		public int AppendVertex(NewVertexInfo info) {
			var i = Vertices.Length / 3;

			if (info.bHaveN && HasVertexNormals) {
				Normals.Add(info.n[0]);
				Normals.Add(info.n[1]);
				Normals.Add(info.n[2]);
			}
			else if (HasVertexNormals) {
				Normals.Add(0);
				Normals.Add(1);
				Normals.Add(0);
			}
			if (info.bHaveC && HasVertexColors) {
				Colors.Add(info.c[0]);
				Colors.Add(info.c[1]);
				Colors.Add(info.c[2]);
			}
			else if (HasVertexColors) {
				Colors.Add(1);
				Colors.Add(1);
				Colors.Add(1);
			}
			if (info.bHaveUV && HasVertexUVs) {
				UVs.Add(info.uv[0][0]);
				UVs.Add(info.uv[0][1]);
			}
			else if (HasVertexUVs) {
				UVs.Add(0);
				UVs.Add(0);
			}

			Vertices.Add(info.v[0]);
			Vertices.Add(info.v[1]);
			Vertices.Add(info.v[2]);
			return i;
		}


		public int AppendQuad(int i, int j, int k, int l, int g = -1) {
			var qi = Quads.Length / 4;
			if (HasFaceGroups) {
				FaceGroups.Add((g == -1) ? 0 : g);
			}

			Quads.Add(i);
			Quads.Add(j);
			Quads.Add(k);
			Quads.Add(l);
			return qi;
		}


		public int VertexCount => Vertices.Length / 3;
		public int QuadCount => Quads.Length / 4;
		public int MaxVertexID => VertexCount;
		public int MaxQuadID => QuadCount;


		public bool IsVertex(int vID) {
			return vID * 3 < Vertices.Length;
		}
		public bool IsQuad(int qID) {
			return qID * 4 < Quads.Length;
		}

		public bool HasVertexColors => Colors != null && Colors.Length == Vertices.Length;

		public bool HasVertexNormals => Normals != null && Normals.Length == Vertices.Length;

		public bool HasVertexUVs => UVs != null && UVs.Length / 2 == Vertices.Length / 3;

		public Vector3d GetVertex(int i) {
			return new Vector3d(Vertices[3 * i], Vertices[(3 * i) + 1], Vertices[(3 * i) + 2]);
		}

		public Vector3f GetVertexNormal(int i) {
			return new Vector3f(Normals[3 * i], Normals[(3 * i) + 1], Normals[(3 * i) + 2]);
		}

		public Vector3f GetVertexColor(int i) {
			return new Vector3f(Colors[3 * i], Colors[(3 * i) + 1], Colors[(3 * i) + 2]);
		}

		public Vector2f GetVertexUV(int i) {
			return new Vector2f(UVs[2 * i], UVs[(2 * i) + 1]);
		}

		public NewVertexInfo GetVertexAll(int i) {
			var vi = new NewVertexInfo {
				v = GetVertex(i)
			};
			if (HasVertexNormals) {
				vi.bHaveN = true;
				vi.n = GetVertexNormal(i);
			}
			else {
				vi.bHaveN = false;
			}

			if (HasVertexColors) {
				vi.bHaveC = true;
				vi.c = GetVertexColor(i);
			}
			else {
				vi.bHaveC = false;
			}

			if (HasVertexUVs) {
				vi.bHaveUV = true;
				vi.uv = new Vector2f[] { GetVertexUV(i) };
			}
			else {
				vi.bHaveUV = false;
			}

			return vi;
		}


		public bool HasFaceGroups => FaceGroups != null && FaceGroups.Length == Quads.Length / 4;

		public Index4i GetQuad(int i) {
			return new Index4i(Quads[4 * i], Quads[(4 * i) + 1], Quads[(4 * i) + 2], Quads[(4 * i) + 3]);
		}

		public int GetFaceGroup(int i) {
			return FaceGroups[i];
		}


		public IEnumerable<Vector3d> VerticesItr() {
			var N = VertexCount;
			for (var i = 0; i < N; ++i) {
				yield return new Vector3d(Vertices[3 * i], Vertices[(3 * i) + 1], Vertices[(3 * i) + 2]);
			}
		}

		public IEnumerable<Vector3f> NormalsItr() {
			var N = VertexCount;
			for (var i = 0; i < N; ++i) {
				yield return new Vector3f(Normals[3 * i], Normals[(3 * i) + 1], Normals[(3 * i) + 2]);
			}
		}

		public IEnumerable<Vector3f> ColorsItr() {
			var N = VertexCount;
			for (var i = 0; i < N; ++i) {
				yield return new Vector3f(Colors[3 * i], Colors[(3 * i) + 1], Colors[(3 * i) + 2]);
			}
		}

		public IEnumerable<Vector2f> UVsItr() {
			var N = VertexCount;
			for (var i = 0; i < N; ++i) {
				yield return new Vector2f(UVs[2 * i], UVs[(2 * i) + 1]);
			}
		}

		public IEnumerable<Index4i> QuadsItr() {
			var N = QuadCount;
			for (var i = 0; i < N; ++i) {
				yield return new Index4i(Quads[4 * i], Quads[(4 * i) + 1], Quads[(4 * i) + 2], Quads[(4 * i) + 3]);
			}
		}

		public IEnumerable<int> FaceGroupsItr() {
			var N = QuadCount;
			for (var i = 0; i < N; ++i) {
				yield return FaceGroups[i];
			}
		}

		public IEnumerable<int> VertexIndices() {
			var N = VertexCount;
			for (var i = 0; i < N; ++i) {
				yield return i;
			}
		}
		public IEnumerable<int> QuadIndices() {
			var N = QuadCount;

			for (var i = 0; i < N; ++i) {
				yield return i;
			}
		}


		// setters

		public void SetVertex(int i, Vector3d v) {
			Vertices[3 * i] = v.x;
			Vertices[(3 * i) + 1] = v.y;
			Vertices[(3 * i) + 2] = v.z;
		}

		public void SetVertexNormal(int i, Vector3f n) {
			Normals[3 * i] = n.x;
			Normals[(3 * i) + 1] = n.y;
			Normals[(3 * i) + 2] = n.z;
		}

		public void SetVertexColor(int i, Vector3f c) {
			Colors[3 * i] = c.x;
			Colors[(3 * i) + 1] = c.y;
			Colors[(3 * i) + 2] = c.z;
		}

		public void SetVertexUV(int i, Vector2f uv) {
			UVs[2 * i] = uv.x;
			UVs[(2 * i) + 1] = uv.y;
		}


		/*
         * Array-based access (allocates arrays automatically)
         */
		public double[] GetVertexArray() {
			return Vertices.GetBuffer();
		}
		public float[] GetVertexArrayFloat() {
			var buf = new float[Vertices.Length];
			for (var i = 0; i < Vertices.Length; ++i) {
				buf[i] = (float)Vertices[i];
			}

			return buf;
		}

		public float[] GetVertexNormalArray() {
			return HasVertexNormals ? Normals.GetBuffer() : null;
		}

		public float[] GetVertexColorArray() {
			return HasVertexColors ? Colors.GetBuffer() : null;
		}

		public float[] GetVertexUVArray() {
			return HasVertexUVs ? UVs.GetBuffer() : null;
		}

		public int[] GetQuadArray() {
			return Quads.GetBuffer();
		}

		public int[] GetFaceGroupsArray() {
			return HasFaceGroups ? FaceGroups.GetBuffer() : null;
		}

	}
}





