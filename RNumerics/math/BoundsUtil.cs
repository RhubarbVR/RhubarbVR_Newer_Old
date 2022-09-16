using System;
using System.Collections;
using System.Collections.Generic;

namespace RNumerics
{
	public static class BoundsUtil
	{

		public static AxisAlignedBox3d Bounds(in IPointSet source) {
			var bounds = AxisAlignedBox3d.Empty;
			foreach (var vid in source.VertexIndices()) {
				bounds.Contain(source.GetVertex(vid));
			}

			return bounds;
		}


		public static AxisAlignedBox3d Bounds(in Triangle3d tri) {
			return Bounds(tri.V0, tri.V1, tri.V2);
		}

		public static AxisAlignedBox3d Bounds(in Vector3d v0, in Vector3d v1, in Vector3d v2) {
			AxisAlignedBox3d box;
			MathUtil.MinMax(v0.x, v1.x, v2.x, out box.Min.x, out box.Max.x);
			MathUtil.MinMax(v0.y, v1.y, v2.y, out box.Min.y, out box.Max.y);
			MathUtil.MinMax(v0.z, v1.z, v2.z, out box.Min.z, out box.Max.z);
			return box;
		}

		public static AxisAlignedBox2d Bounds(in Vector2d v0, in Vector2d v1, in Vector2d v2) {
			AxisAlignedBox2d box;
			MathUtil.MinMax(v0.x, v1.x, v2.x, out box.Min.x, out box.Max.x);
			MathUtil.MinMax(v0.y, v1.y, v2.y, out box.Min.y, out box.Max.y);
			return box;
		}

		// AABB of transformed AABB (corners)
		public static AxisAlignedBox3d Bounds(in AxisAlignedBox3d boxIn, in Func<Vector3d, Vector3d> TransformF) {
			if (TransformF == null) {
				return boxIn;
			}

			var box = new AxisAlignedBox3d(TransformF(boxIn.Corner(0)));
			for (var i = 1; i < 8; ++i) {
				box.Contain(TransformF(boxIn.Corner(i)));
			}

			return box;
		}


		public static AxisAlignedBox3d Bounds(in IEnumerable<Vector3d> positions) {
			var box = AxisAlignedBox3d.Empty;
			foreach (var v in positions) {
				box.Contain(v);
			}

			return box;
		}
		public static AxisAlignedBox3f Bounds(in IEnumerable<Vector3f> positions) {
			var box = AxisAlignedBox3f.Empty;
			foreach (var v in positions) {
				box.Contain(v);
			}

			return box;
		}


		public static AxisAlignedBox2d Bounds(in IEnumerable<Vector2d> positions) {
			var box = AxisAlignedBox2d.Empty;
			foreach (var v in positions) {
				box.Contain(v);
			}

			return box;
		}
		public static AxisAlignedBox2f Bounds(in IEnumerable<Vector2f> positions) {
			var box = AxisAlignedBox2f.Empty;
			foreach (var v in positions) {
				box.Contain(v);
			}

			return box;
		}


		public static AxisAlignedBox3d Bounds<T>(in IEnumerable<T> values, in Func<T, Vector3d> PositionF) {
			var box = AxisAlignedBox3d.Empty;
			foreach (var t in values) {
				box.Contain(PositionF(t));
			}

			return box;
		}
		public static AxisAlignedBox3f Bounds<T>(in IEnumerable<T> values, in Func<T, Vector3f> PositionF) {
			var box = AxisAlignedBox3f.Empty;
			foreach (var t in values) {
				box.Contain(PositionF(t));
			}

			return box;
		}


		/// <summary>
		/// compute axis-aligned bounds of set of points after transforming 
		/// </summary>
		public static AxisAlignedBox3d Bounds(in IEnumerable<Vector3d> values, in TransformSequence xform) {
			var box = AxisAlignedBox3d.Empty;
			foreach (var v in values) {
				box.Contain(xform.TransformP(v));
			}

			return box;
		}

		public static AxisAlignedBox3f Combined(in AxisAlignedBox3f a, in AxisAlignedBox3f b) {
			return new AxisAlignedBox3f {
				Max = new Vector3f(Math.Max(a.Max.x, b.Max.x), Math.Max(a.Max.y, b.Max.y), Math.Max(a.Max.z, b.Max.z)),
				Min = new Vector3f(Math.Min(a.Min.x, b.Min.x), Math.Min(a.Min.y, b.Min.y), Math.Min(a.Min.z, b.Min.z)),
			};
		}


		/// <summary>
		/// compute axis-aligned bounds of set of points after transforming into frame f
		/// </summary>
		public static AxisAlignedBox3d BoundsInFrame(in IEnumerable<Vector3d> values, in Frame3f f) {
			var box = AxisAlignedBox3d.Empty;
			foreach (var v in values) {
				box.Contain(f.ToFrameP(v));
			}

			return box;
		}
	}
}
