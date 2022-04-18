using System;
using System.Collections.Generic;
using System.Linq;

namespace RNumerics
{
	/// <summary>
	/// Create a mesh that contains a planar element for each point and normal
	/// (currently only triangles)
	/// </summary>
	public class PointSplatsGenerator : MeshGenerator
	{
		public IEnumerable<int> PointIndices;
		public int PointIndicesCount = -1;      // you can set this to avoid calling Count() on enumerable

		public Func<int, Vector3d> PointF;      // required
		public Func<int, Vector3d> NormalF;     // required
		public double Radius = 1.0f;

		public PointSplatsGenerator() {
			WantUVs = false;
		}

		public override MeshGenerator Generate() {
			var N = (PointIndicesCount == -1) ? PointIndices.Count() : PointIndicesCount;

			vertices = new VectorArray3d(N * 3);
			uv = null;
			normals = new VectorArray3f(vertices.Count);
			triangles = new IndexArray3i(N);

			var matRot = new Matrix2f(120 * MathUtil.DEG_2_RADF);
			var uva = new Vector2f(0, Radius);
			var uvb = matRot * uva;
			var uvc = matRot * uvb;

			var vi = 0;
			var ti = 0;
			foreach (var pid in PointIndices) {
				var v = PointF(pid);
				var n = NormalF(pid);
				var f = new Frame3f(v, n);
				triangles.Set(ti++, vi, vi + 1, vi + 2, Clockwise);
				vertices[vi++] = f.FromPlaneUV(uva, 2);
				vertices[vi++] = f.FromPlaneUV(uvb, 2);
				vertices[vi++] = f.FromPlaneUV(uvc, 2);
			}

			return this;
		}



		/// <summary>
		/// shortcut utility
		/// </summary>
		public static SimpleMesh Generate(IList<int> indices,
			Func<int, Vector3d> PointF, Func<int, Vector3d> NormalF,
			double radius) {
			var gen = new PointSplatsGenerator() {
				PointIndices = indices,
				PointIndicesCount = indices.Count,
				PointF = PointF,
				NormalF = NormalF,
				Radius = radius
			};
			return gen.Generate().MakeSimpleMesh();
		}


	}
}
