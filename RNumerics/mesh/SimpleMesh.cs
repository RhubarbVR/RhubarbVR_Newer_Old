using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace RNumerics
{
	public class SimpleMesh : IDeformableMesh
	{
		public IEnumerable<Vector3f> VertexPos() {
			for (var i = 0; i < VertexCount; i++) {
				yield return (Vector3f)GetVertex(i);
			}
		}
		public DVector<double> Vertices;
		public DVector<float> Normals;
		public DVector<float> Colors;
		public DVector<float> UVs;

		public DVector<int> Triangles;
		public DVector<int> FaceGroups;

		public SimpleMesh() {
			Initialize();
		}

		//public void CopyTo(SimpleMesh mTo)
		//{
		//    mTo.Vertices = Util.BufferCopy(this.Vertices, mTo.Vertices);
		//    mTo.Normals = Util.BufferCopy(this.Normals, mTo.Normals);
		//    mTo.Colors = Util.BufferCopy(this.Colors, mTo.Colors);
		//    mTo.Triangles = Util.BufferCopy(this.Triangles, mTo.Triangles);
		//    mTo.FaceGroups = Util.BufferCopy(this.FaceGroups, mTo.FaceGroups);
		//}

		public SimpleMesh Copy()
		{
		    return new SimpleMesh(this);
		}

		public SimpleMesh CutOnPlane(Plane3d plane,bool switchSide,bool removeOtherSide = false) {
			var cutMesh = new SimpleMesh();
			foreach (var item in Triangles) {
				var tryangle = GetTriangle(item);
				var v1 = GetVertexAll(tryangle.a);
				var v2 = GetVertexAll(tryangle.b);
				var v3 = GetVertexAll(tryangle.c);
				var v1PastPlane = (plane.DistanceTo(v1.v) > 0) == switchSide;
				var v1isOnPlane = plane.DistanceTo(v1.v) == 0;
				var v2PastPlane = (plane.DistanceTo(v2.v) > 0) == switchSide;
				var v2isOnPlane = plane.DistanceTo(v1.v) == 0;
				var v3PastPlane = (plane.DistanceTo(v3.v) > 0) == switchSide;
				var v3isOnPlane = plane.DistanceTo(v3.v) == 0;
				if (!(v1PastPlane || v2PastPlane || v3PastPlane)) {
					cutMesh.AppendTriangle(v1,v2,v3);
					continue;
				}
				if(v1PastPlane && v2PastPlane && v3PastPlane) {
					if (removeOtherSide) {
						continue;
					}
					else {
						cutMesh.AppendTriangle(v1, v2, v3);
						continue;
					}
				}
				if(v1PastPlane && !v2PastPlane && !v3PastPlane) {
					var newtry1 = plane.IntersectLine(v1.v, v2.v);
					var newtry2 = plane.IntersectLine(v1.v, v3.v);
					var newvert1 = new NewVertexInfo { v = newtry1 };
					var newvert2 = new NewVertexInfo { v = newtry2 };
					cutMesh.AppendTriangle(v3, newvert2, newvert1);
					cutMesh.AppendTriangle(newvert1, v2, v3);
					if (!removeOtherSide) {
						cutMesh.AppendTriangle(newvert1, newvert2, v1);
					}
					continue;
				}
				if (!v1PastPlane && v2PastPlane && v3PastPlane) {
					var newtry1 = plane.IntersectLine(v1.v, v2.v);
					var newtry2 = plane.IntersectLine(v1.v, v3.v);
					var newvert1 = new NewVertexInfo { v = newtry1 };
					var newvert2 = new NewVertexInfo { v = newtry2 };
					cutMesh.AppendTriangle(newvert1, newvert2, v1);
					if (!removeOtherSide) {
						cutMesh.AppendTriangle(v3, newvert2, newvert1);
						cutMesh.AppendTriangle(newvert1, v2, v3);
					}
					continue;
				}

				if (v2PastPlane && !v3PastPlane && !v1PastPlane) {
					var newtry1 = plane.IntersectLine(v2.v, v3.v);
					var newtry2 = plane.IntersectLine(v2.v, v1.v);
					var newvert1 = new NewVertexInfo { v = newtry1 };
					var newvert2 = new NewVertexInfo { v = newtry2 };
					cutMesh.AppendTriangle(newvert2, newvert1, v3);
					cutMesh.AppendTriangle(v1, newvert2, v3);
					if (!removeOtherSide) {
						cutMesh.AppendTriangle(v2, newvert1, newvert2);
					}
					continue;
				}
				if (!v2PastPlane && v3PastPlane && v1PastPlane) {
					var newtry1 = plane.IntersectLine(v2.v, v3.v);
					var newtry2 = plane.IntersectLine(v2.v, v1.v);
					var newvert1 = new NewVertexInfo { v = newtry1 };
					var newvert2 = new NewVertexInfo { v = newtry2 };
					cutMesh.AppendTriangle(v2, newvert1, newvert2 );
					if (!removeOtherSide) {
						cutMesh.AppendTriangle(newvert2, newvert1, v3 );
						cutMesh.AppendTriangle(v1, newvert2, v3 );
					}
					continue;
				}
				if (v3PastPlane && !v1PastPlane && !v2PastPlane) {
					var newtry1 = plane.IntersectLine(v3.v, v1.v);
					var newtry2 = plane.IntersectLine(v3.v, v2.v);
					var newvert1 = new NewVertexInfo { v = newtry1 };
					var newvert2 = new NewVertexInfo { v = newtry2 };
					cutMesh.AppendTriangle(v2, newvert2, newvert1);
					cutMesh.AppendTriangle(v2, newvert1, v1);
					if (!removeOtherSide) {
						cutMesh.AppendTriangle(v3, newvert1, newvert2);
					}
					continue;
				}
				if (!v3PastPlane && v1PastPlane && v2PastPlane) {
					var newtry1 = plane.IntersectLine(v3.v, v1.v);
					var newtry2 = plane.IntersectLine(v3.v, v2.v);
					var newvert1 = new NewVertexInfo { v = newtry1 };
					var newvert2 = new NewVertexInfo { v = newtry2 };
					cutMesh.AppendTriangle(newvert2, newvert1, v3 );
					if (!removeOtherSide) {
						cutMesh.AppendTriangle(newvert1, newvert2, v2);
						cutMesh.AppendTriangle(v1, newvert1, v2);
					}
					continue;
				}

				Console.WriteLine("Not supposed to be here");
				throw new Exception();
			}
			return cutMesh;
		}

		public SimpleMesh Cut(Vector2f cutmax,Vector2f cutmin) {
			var cutMesh = new SimpleMesh();
			var vertsThatNeedCap = new List<NewVertexInfo>();
			foreach (var item in Triangles) {
				var tryangle = GetTriangle(item);
				var v1 = GetVertexAll(tryangle.a);
				var v2 = GetVertexAll(tryangle.b);
				var v3 = GetVertexAll(tryangle.c);
				var v1inbox = cutmax.IsInBox(cutmin, v1.v.Xy);
				var v2inbox = cutmax.IsInBox(cutmin, v2.v.Xy);
				var v3inbox = cutmax.IsInBox(cutmin, v3.v.Xy);
				void TryAdd(NewVertexInfo vert, NewVertexInfo vert2, NewVertexInfo vert3) {
					var intesect1 = Vector2f.MinMaxIntersect(vert2.v.Xy, cutmin, cutmax);
					var intesect2 = Vector2f.MinMaxIntersect(vert3.v.Xy, cutmin, cutmax);
					var present1 = (MathUtil.Abs(vert.v.Xy - intesect1) / MathUtil.Abs(vert.v.Xy - vert2.v.Xy)).Clean;
					var present2 = (MathUtil.Abs(vert.v.Xy - intesect2) / MathUtil.Abs(vert.v.Xy - vert3.v.Xy)).Clean;
					var uv1 = ((vert2.uv[0] - vert.uv[0]) * present1) + vert.uv[0];
					var uv2 = ((vert3.uv[0] - vert.uv[0]) * present2) + vert.uv[0];
					var new2 = new NewVertexInfo(new Vector3d(intesect1.x, intesect1.y, vert2.v.z), vert2.n, vert2.c, (Vector2f)uv1);
					var new3 = new NewVertexInfo(new Vector3d(intesect2.x, intesect2.y, vert3.v.z), vert3.n, vert3.c, (Vector2f)uv2);
					cutMesh.AppendTriangle(vert, new2, new3);
					vertsThatNeedCap.Add(new2);
					vertsThatNeedCap.Add(new3);
				}
				void QuadAdd(NewVertexInfo vert, NewVertexInfo vert2, NewVertexInfo outvert) {
					//To only works with rect any complex shape will brake
					var intesect1 = Vector2f.MinMaxIntersect(vert2.v.Xy, cutmin, cutmax);
					var intesect2 = Vector2f.MinMaxIntersect(outvert.v.Xy, cutmin, cutmax);
					var present1 = (MathUtil.Abs(vert.v.Xy - intesect1) / MathUtil.Abs(vert.v.Xy - vert2.v.Xy)).Clean;
					var present2 = (MathUtil.Abs(vert.v.Xy - intesect2) / MathUtil.Abs(vert.v.Xy - outvert.v.Xy)).Clean;
					var uv1 = ((vert2.uv[0] - vert.uv[0]) * present1) + vert.uv[0];
					var uv2 = ((outvert.uv[0] - vert.uv[0]) * present2) + vert.uv[0];
					var new2 = new NewVertexInfo(new Vector3d(intesect1.x, intesect1.y, vert2.v.z), vert2.n, vert2.c, (Vector2f)uv1);
					var new3 = new NewVertexInfo(new Vector3d(intesect2.x, intesect2.y, outvert.v.z), outvert.n, outvert.c, (Vector2f)uv2);
					cutMesh.AppendTriangle(vert, new2, new3);
					vertsThatNeedCap.Add(new2);
					vertsThatNeedCap.Add(new3);
				}
				if (v1inbox && v2inbox && v3inbox) {
					cutMesh.AppendTriangle(v1, v2, v3);
				}
				else if (!(v1inbox || v2inbox || v3inbox)) {
					continue;
				}
				else if (v1inbox) {
					if (v2inbox) {
						QuadAdd(v1, v2, v3);
					}
					else if (v3inbox) {
						QuadAdd(v1, v3, v2);
					}
					else {
						TryAdd(v1, v2, v3);
					}
				}
				else if (v2inbox) {
					if (v1inbox) {
						QuadAdd(v2, v1, v3);
					}
					else if (v3inbox) {
						QuadAdd(v2, v3, v1);
					}
					else {
						TryAdd(v2, v1, v2);
					}
				}
				else {
					if (v2inbox) {
						QuadAdd(v3, v2, v1);
					}
					else if (v1inbox) {
						QuadAdd(v3, v1, v2);
					}
					else {
						TryAdd(v3, v1, v2);
					}
				}
			}
			if (vertsThatNeedCap.Count != 0) {
				var firstvert = vertsThatNeedCap[0];
				var min = firstvert.v;
				var max = firstvert.v;
				foreach (var item in vertsThatNeedCap) {
					max = MathUtil.Max(item.v, max);
					min = MathUtil.Min(item.v, min);
				}
				var new11 = new NewVertexInfo { bHaveN = true, n = Vector3f.Up, bHaveC = true, c = firstvert.c, v = new Vector3d(min.x, max.y, min.z) };
				var new21 = new NewVertexInfo { bHaveN = true, bHaveC = true, n = Vector3f.Up, c = firstvert.c, v = new Vector3d(max.x, max.y, min.z) };
				var new31 = new NewVertexInfo { bHaveN = true, bHaveC = true, n = Vector3f.Up, c = firstvert.c, v = new Vector3d(min.x, max.y, max.z) };
				var new41 = new NewVertexInfo { bHaveN = true, bHaveC = true, n = Vector3f.Up, c = firstvert.c, v = new Vector3d(max.x, max.y, max.z) };
				cutMesh.AppendTriangle(new31, new21, new11);
				cutMesh.AppendTriangle(new41, new21, new31);
				var new1 = new NewVertexInfo { bHaveN = true, n = Vector3f.Up, bHaveC = true, c = firstvert.c, v = new Vector3d(min.x, min.y, min.z) };
				var new2 = new NewVertexInfo { bHaveN = true, n = Vector3f.Up, bHaveC = true, c = firstvert.c, v = new Vector3d(max.x, min.y, min.z) };
				var new3 = new NewVertexInfo { bHaveN = true, n = Vector3f.Up, bHaveC = true, c = firstvert.c, v = new Vector3d(min.x, min.y, max.z) };
				var new4 = new NewVertexInfo { bHaveN = true, n = Vector3f.Up, bHaveC = true, c = firstvert.c, v = new Vector3d(max.x, min.y, max.z) };
				cutMesh.AppendTriangle(new1, new2, new3);
				cutMesh.AppendTriangle(new3, new2, new4);
			}
			return cutMesh;
		}

		public SimpleMesh(IMesh copy) {
			Initialize(copy.HasVertexNormals, copy.HasVertexColors, copy.HasVertexUVs, copy.HasTriangleGroups);
			var mapV = new int[copy.MaxVertexID];
			foreach (var vid in copy.VertexIndices()) {
				var vi = copy.GetVertexAll(vid);
				var new_vid = AppendVertex(vi);
				mapV[vid] = new_vid;
			}
			foreach (var tid in copy.TriangleIndices()) {
				var t = copy.GetTriangle(tid);
				t[0] = mapV[t[0]];
				t[1] = mapV[t[1]];
				t[2] = mapV[t[2]];
				if (copy.HasTriangleGroups) {
					AppendTriangle(t[0], t[1], t[2], copy.GetTriangleGroup(tid));
				}
				else {
					AppendTriangle(t[0], t[1], t[2]);
				}
			}
		}


		public void Initialize(bool bWantNormals = true, bool bWantColors = true, bool bWantUVs = true, bool bWantFaceGroups = true) {
			Vertices = new DVector<double>();
			Normals = bWantNormals ? new DVector<float>() : null;
			Colors = bWantColors ? new DVector<float>() : null;
			UVs = bWantUVs ? new DVector<float>() : null;
			Triangles = new DVector<int>();
			FaceGroups = bWantFaceGroups ? new DVector<int>() : null;
		}

		public void Initialize(VectorArray3d v, VectorArray3i t,
			VectorArray3f n = null, VectorArray3f c = null, VectorArray2f uv = null, int[] g = null) {
			Vertices = new DVector<double>(v);
			Triangles = new DVector<int>(t);
			Normals = Colors = UVs = null;
			FaceGroups = null;
			if (n != null) {
				Normals = new DVector<float>(n);
			}

			if (c != null) {
				Colors = new DVector<float>(c);
			}

			if (uv != null) {
				UVs = new DVector<float>(uv);
			}

			if (g != null) {
				FaceGroups = new DVector<int>(g);
			}
		}

		public void Initialize(IEnumerable<double> v, int[] t,
			float[] n = null, float[] c = null, float[] uv = null, int[] g = null) {
			Vertices = new DVector<double>(v);
			Triangles = new DVector<int>(t);
			Normals = Colors = UVs = null;
			FaceGroups = null;
			if (n != null) {
				Normals = new DVector<float>(n);
			}

			if (c != null) {
				Colors = new DVector<float>(c);
			}

			if (uv != null) {
				UVs = new DVector<float>(uv);
			}

			if (g != null) {
				FaceGroups = new DVector<int>(g);
			}
		}

		/// <summary>
		/// Timestamp is incremented any time any change is made to the mesh
		/// </summary>
		public int Timestamp { get; private set; } = 0;

		void UpdateTimeStamp() {
			Timestamp++;
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
			UpdateTimeStamp();
			return i;
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
			UpdateTimeStamp();
			return i;
		}

		public int AppendTriangle(NewVertexInfo a, NewVertexInfo b, NewVertexInfo c) {
			var va = AppendVertex(a);
			var vb = AppendVertex(b);
			var vc = AppendVertex(c);
			return AppendTriangle(va, vb, vc);
		}

		public void AppendVertices(VectorArray3d v, VectorArray3f n = null, VectorArray3f c = null, VectorArray2f uv = null) {
			Vertices.Add(v.array);
			if (n != null && HasVertexNormals) {
				Normals.Add(n.array);
			}
			else if (HasVertexNormals) {
				Normals.Add(new float[] { 0, 1, 0 }, v.Count);
			}

			if (c != null && HasVertexColors) {
				Colors.Add(c.array);
			}
			else if (HasVertexColors) {
				Normals.Add(new float[] { 1, 1, 1 }, v.Count);
			}

			if (uv != null && HasVertexUVs) {
				UVs.Add(uv.array);
			}
			else if (HasVertexUVs) {
				UVs.Add(new float[] { 0, 0 }, v.Count);
			}

			UpdateTimeStamp();
		}



		public int AppendTriangle(int i, int j, int k, int g = -1) {
			var ti = Triangles.Length / 3;
			if (HasTriangleGroups) {
				FaceGroups.Add((g == -1) ? 0 : g);
			}

			Triangles.Add(i);
			Triangles.Add(j);
			Triangles.Add(k);
			UpdateTimeStamp();
			return ti;
		}


		public void AppendTriangles(int[] vTriangles, int[] vertexMap, int g = -1) {
			for (var ti = 0; ti < vTriangles.Length; ++ti) {
				Triangles.Add(vertexMap[vTriangles[ti]]);
			}
			if (HasTriangleGroups) {
				for (var ti = 0; ti < vTriangles.Length / 3; ++ti) {
					FaceGroups.Add((g == -1) ? 0 : g);
				}
			}
			UpdateTimeStamp();
		}

		public void AppendTriangles(IndexArray3i t, int[] groups = null) {
			Triangles.Add(t.array);
			if (HasTriangleGroups) {
				if (groups != null) {
					FaceGroups.Add(groups);
				}
				else {
					FaceGroups.Add(0, t.Count);
				}
			}
			UpdateTimeStamp();
		}


		/*
         * Utility / Convenience
         */

		// [RMS] this is convenience stuff...
		public void Translate(double tx, double ty, double tz) {
			var c = VertexCount;
			for (var i = 0; i < c; ++i) {
				Vertices[3 * i] += tx;
				Vertices[(3 * i) + 1] += ty;
				Vertices[(3 * i) + 2] += tz;
			}
			UpdateTimeStamp();
		}

		public void Translate(Vector3d vector3D) {
			Translate(vector3D.x, vector3D.y, vector3D.z);
		}

		public void Scale(double sx, double sy, double sz) {
			var c = VertexCount;
			for (var i = 0; i < c; ++i) {
				Vertices[3 * i] *= sx;
				Vertices[(3 * i) + 1] *= sy;
				Vertices[(3 * i) + 2] *= sz;
			}
			UpdateTimeStamp();
		}
		public void Scale(double s) {
			Scale(s, s, s);
			UpdateTimeStamp();
		}


		/*
         * IMesh interface
         */


		public int VertexCount => Vertices.Length / 3;
		public int TriangleCount => Triangles.Length / 3;
		public int MaxVertexID => VertexCount;
		public int MaxTriangleID => TriangleCount;


		public bool IsVertex(int vID) {
			return vID * 3 < Vertices.Length;
		}
		public bool IsTriangle(int tID) {
			return tID * 3 < Triangles.Length;
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

		public Vector2f GetVertexUV(int i, int channel = 1) {
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


		public bool HasTriangleGroups => FaceGroups != null && FaceGroups.Length == Triangles.Length / 3;

		public Index3i GetTriangle(int i) {
			return new Index3i(Triangles[3 * i], Triangles[(3 * i) + 1], Triangles[(3 * i) + 2]);
		}

		public int GetTriangleGroup(int i) {
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

		public IEnumerable<Index3i> TrianglesItr() {
			var N = TriangleCount;
			for (var i = 0; i < N; ++i) {
				yield return new Index3i(Triangles[3 * i], Triangles[(3 * i) + 1], Triangles[(3 * i) + 2]);
			}
		}

		public IEnumerable<int> TriangleGroupsItr() {
			var N = TriangleCount;
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
		public IEnumerable<int> TriangleIndices() {
			var N = TriangleCount;
			for (var i = 0; i < N; ++i) {
				yield return i;
			}
		}

		public IEnumerable<int> RenderIndices() {
			var N = TriangleCount;
			for (var i = 0; i < N; ++i) {
				yield return GetTriangle(i).a;
				yield return GetTriangle(i).b;
				yield return GetTriangle(i).c;
			}
		}

		// setters

		public void SetVertex(int i, Vector3d v) {
			Vertices[3 * i] = v.x;
			Vertices[(3 * i) + 1] = v.y;
			Vertices[(3 * i) + 2] = v.z;
			UpdateTimeStamp();
		}

		public void SetVertexNormal(int i, Vector3f n) {
			Normals[3 * i] = n.x;
			Normals[(3 * i) + 1] = n.y;
			Normals[(3 * i) + 2] = n.z;
			UpdateTimeStamp();
		}

		public void SetVertexColor(int i, Vector3f c) {
			Colors[3 * i] = c.x;
			Colors[(3 * i) + 1] = c.y;
			Colors[(3 * i) + 2] = c.z;
			UpdateTimeStamp();
		}

		public void SetVertexUV(int i, Vector2f uv) {
			UVs[2 * i] = uv.x;
			UVs[(2 * i) + 1] = uv.y;
			UpdateTimeStamp();
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

		public int[] GetTriangleArray() {
			return Triangles.GetBuffer();
		}

		public int[] GetFaceGroupsArray() {
			return HasTriangleGroups ? FaceGroups.GetBuffer() : null;
		}




		/*
         * copy internal data into buffers. Assumes that buffers are big enough!!
         */

		public unsafe void GetVertexBuffer(double* pBuffer) {
			DVector<double>.FastGetBuffer(Vertices, pBuffer);
		}

		public unsafe void GetVertexNormalBuffer(float* pBuffer) {
			if (HasVertexNormals) {
				DVector<float>.FastGetBuffer(Normals, pBuffer);
			}
		}

		public unsafe void GetVertexColorBuffer(float* pBuffer) {
			if (HasVertexColors) {
				DVector<float>.FastGetBuffer(Colors, pBuffer);
			}
		}

		public unsafe void GetVertexUVBuffer(float* pBuffer) {
			if (HasVertexUVs) {
				DVector<float>.FastGetBuffer(UVs, pBuffer);
			}
		}

		public unsafe void GetTriangleBuffer(int* pBuffer) {
			DVector<int>.FastGetBuffer(Triangles, pBuffer);
		}

		public unsafe void GetFaceGroupsBuffer(int* pBuffer) {
			if (HasTriangleGroups) {
				DVector<int>.FastGetBuffer(FaceGroups, pBuffer);
			}
		}

	}






	public class SimpleMeshBuilder : IMeshBuilder
	{
		public List<SimpleMesh> Meshes;
		public List<int> MaterialAssignment;

		int _nActiveMesh;

		public SimpleMeshBuilder() {
			Meshes = new List<SimpleMesh>();
			MaterialAssignment = new List<int>();
			_nActiveMesh = -1;
		}

		public int AppendNewMesh(bool bHaveVtxNormals, bool bHaveVtxColors, bool bHaveVtxUVs, bool bHaveFaceGroups) {
			var index = Meshes.Count;
			var m = new SimpleMesh();
			m.Initialize(bHaveVtxNormals, bHaveVtxColors, bHaveVtxUVs, bHaveFaceGroups);
			Meshes.Add(m);
			MaterialAssignment.Add(-1);     // no material is known
			_nActiveMesh = index;
			return index;
		}

		public void SetActiveMesh(int id) {
			_nActiveMesh = id >= 0 && id < Meshes.Count ? id : throw new ArgumentOutOfRangeException("active mesh id is out of range");
		}

		public int AppendTriangle(int i, int j, int k) {
			return Meshes[_nActiveMesh].AppendTriangle(i, j, k);
		}

		public int AppendTriangle(int i, int j, int k, int g) {
			return Meshes[_nActiveMesh].AppendTriangle(i, j, k, g);
		}

		public int AppendVertex(double x, double y, double z) {
			return Meshes[_nActiveMesh].AppendVertex(x, y, z);
		}
		public int AppendVertex(NewVertexInfo info) {
			return Meshes[_nActiveMesh].AppendVertex(info);
		}

		public bool SupportsMetaData => false;
		public void AppendMetaData(string identifier, object data) {
			throw new NotImplementedException("SimpleMeshBuilder: metadata not supported");
		}

	}

}
