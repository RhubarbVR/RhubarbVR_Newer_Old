using System;
using System.Linq;

namespace RNumerics
{
	abstract public class MeshGenerator
	{
		public VectorArray3d vertices;
		public VectorArray2f uv;
		public VectorArray3f normals;
		public IndexArray3i triangles;
		public int[] groups;

		public bool WantUVs = true;
		public bool WantNormals = true;
		public bool WantGroups = true;

		public bool Clockwise = false;



		abstract public MeshGenerator Generate();


		public virtual void MakeMesh(SimpleMesh m) {
			m.AppendVertices(vertices, WantNormals ? normals : null, null, WantUVs ? uv : null);
			m.AppendTriangles(triangles);
		}
		public virtual SimpleMesh MakeSimpleMesh() {
			var m = new SimpleMesh();
			m.Initialize(vertices.array.Cast<double>(),triangles.array, WantNormals ? normals.array : null, null, WantUVs ? uv.array : null);
			return m;
		}

		public virtual void MakeMesh(NTMesh3 m) {
			var nV = vertices.Count;
			for (var i = 0; i < nV; ++i) {
				var vID = m.AppendVertex(vertices[i]);
			}
			var nT = triangles.Count;
			if (WantGroups && groups != null && groups.Length == nT) {
				m.EnableTriangleGroups();
				for (var i = 0; i < nT; ++i) {
					m.AppendTriangle(triangles[i], groups[i]);
				}
			}
			else {
				for (var i = 0; i < nT; ++i) {
					m.AppendTriangle(triangles[i]);
				}
			}
		}
		public virtual NTMesh3 MakeNTMesh() {
			var m = new NTMesh3();
			MakeMesh(m);
			return m;
		}









		public struct CircularSection
		{
			public float Radius;
			public float SectionY;
			public CircularSection(float r, float y) {
				Radius = r;
				SectionY = y;
			}
		}


		protected void Duplicate_vertex_span(int nStart, int nCount) {
			for (var i = 0; i < nCount; ++i) {
				vertices[nStart + nCount + i] = vertices[nStart + i];
				normals[nStart + nCount + i] = normals[nStart + i];
				uv[nStart + nCount + i] = uv[nStart + i];
			}
		}


		protected void Append_disc(int Slices, int nCenterV, int nRingStart, bool bClosed, bool bCycle, ref int tri_counter, int groupid = -1) {
			var nLast = nRingStart + Slices;
			for (var k = nRingStart; k < nLast - 1; ++k) {
				if (groupid >= 0) {
					groups[tri_counter] = groupid;
				}

				triangles.Set(tri_counter++, k, nCenterV, k + 1, bCycle);
			}
			if (bClosed) {     // close disc if we went all the way
				if (groupid >= 0) {
					groups[tri_counter] = groupid;
				}

				triangles.Set(tri_counter++, nLast - 1, nCenterV, nRingStart, bCycle);
			}
		}

		// assumes order would be [v0,v1,v2,v3], ccw
		protected void Append_rectangle(int v0, int v1, int v2, int v3, bool bCycle, ref int tri_counter, int groupid = -1) {
			if (groupid >= 0) {
				groups[tri_counter] = groupid;
			}

			triangles.Set(tri_counter++, v0, v1, v2, bCycle);
			if (groupid >= 0) {
				groups[tri_counter] = groupid;
			}

			triangles.Set(tri_counter++, v0, v2, v3, bCycle);
		}


		// append "disc" verts/tris between vEnd1 and vEnd2
		protected void Append_2d_disc_segment(int iCenter, int iEnd1, int iEnd2, int nSteps, bool bCycle, ref int vtx_counter, ref int tri_counter, int groupid = -1, double force_r = 0) {
			var c = vertices[iCenter];
			var e0 = vertices[iEnd1];
			var e1 = vertices[iEnd2];
			var v0 = e0 - c;
			var r0 = v0.Normalize();
			if (force_r > 0) {
				r0 = force_r;
			}

			var tStart = Math.Atan2(v0.z, v0.x);
			var v1 = e1 - c;
			var r1 = v1.Normalize();
			if (force_r > 0) {
				r1 = force_r;
			}

			var tEnd = Math.Atan2(v1.z, v1.x);

			// fix angles to handle sign. **THIS ONLY WORKS IF WE ARE GOING CCW!!**
			if (tStart < 0) {
				tStart += MathUtil.TWO_PI;
			}

			if (tEnd < 0) {
				tEnd += MathUtil.TWO_PI;
			}

			if (tEnd < tStart) {
				tEnd += MathUtil.TWO_PI;
			}

			var iPrev = iEnd1;
			for (var i = 0; i < nSteps; ++i) {
				var t = (double)(i + 1) / (nSteps + 1);
				var angle = ((1 - t) * tStart) + (t * tEnd);
				var pos = c + new Vector3d(r0 * Math.Cos(angle), 0, r1 * Math.Sin(angle));
				vertices.Set(vtx_counter, pos.x, pos.y, pos.z);
				if (groupid >= 0) {
					groups[tri_counter] = groupid;
				}

				triangles.Set(tri_counter++, iCenter, iPrev, vtx_counter, bCycle);
				iPrev = vtx_counter++;
			}
			if (groupid >= 0) {
				groups[tri_counter] = groupid;
			}

			triangles.Set(tri_counter++, iCenter, iPrev, iEnd2, bCycle);
		}

		protected Vector3f Estimate_normal(int v0, int v1, int v2) {
			var a = vertices[v0];
			var b = vertices[v1];
			var c = vertices[v2];
			var e1 = (b - a).Normalized;
			var e2 = (c - a).Normalized;
			return new Vector3f(e1.Cross(e2));
		}


		protected Vector3d Bilerp(ref Vector3d v00, ref Vector3d v10, ref Vector3d v11, ref Vector3d v01, double tx, double ty) {
			var a = Vector3d.Lerp(ref v00, ref v01, ty);
			var b = Vector3d.Lerp(ref v10, ref v11, ty);
			return Vector3d.Lerp(a, b, tx);
		}

		protected Vector2d Bilerp(ref Vector2d v00, ref Vector2d v10, ref Vector2d v11, ref Vector2d v01, double tx, double ty) {
			var a = Vector2d.Lerp(ref v00, ref v01, ty);
			var b = Vector2d.Lerp(ref v10, ref v11, ty);
			return Vector2d.Lerp(a, b, tx);
		}
		protected Vector2f Bilerp(ref Vector2f v00, ref Vector2f v10, ref Vector2f v11, ref Vector2f v01, float tx, float ty) {
			var a = Vector2f.Lerp(ref v00, ref v01, ty);
			var b = Vector2f.Lerp(ref v10, ref v11, ty);
			return Vector2f.Lerp(a, b, tx);
		}

		protected Vector3i Bilerp(ref Vector3i v00, ref Vector3i v10, ref Vector3i v11, ref Vector3i v01, double tx, double ty) {
			var a = Vector3d.Lerp((Vector3d)v00, (Vector3d)v01, ty);
			var b = Vector3d.Lerp((Vector3d)v10, (Vector3d)v11, ty);
			var c = Vector3d.Lerp(a, b, tx);
			return new Vector3i((int)Math.Round(c.x), (int)Math.Round(c.y), (int)Math.Round(c.z));
		}

		protected Vector3i Lerp(ref Vector3i a, ref Vector3i b, double t) {
			var c = Vector3d.Lerp((Vector3d)a, (Vector3d)b, t);
			return new Vector3i((int)Math.Round(c.x), (int)Math.Round(c.y), (int)Math.Round(c.z));
		}
	}
}
