using System;
using System.Collections;
using System.Collections.Generic;

namespace RNumerics
{
	public static class BoundsUtil
	{

		public static AxisAlignedBox3d Bounds(IPointSet source) {
			var bounds = AxisAlignedBox3d.Empty;
			foreach (var vid in source.VertexIndices()) {
				bounds.Contain(source.GetVertex(vid));
			}

			return bounds;
		}


		public static AxisAlignedBox3d Bounds(ref Triangle3d tri) {
			return Bounds(ref tri.V0, ref tri.V1, ref tri.V2);
		}

		public static AxisAlignedBox3d Bounds(ref Vector3d v0, ref Vector3d v1, ref Vector3d v2) {
			AxisAlignedBox3d box;
			MathUtil.MinMax(v0.x, v1.x, v2.x, out box.Min.x, out box.Max.x);
			MathUtil.MinMax(v0.y, v1.y, v2.y, out box.Min.y, out box.Max.y);
			MathUtil.MinMax(v0.z, v1.z, v2.z, out box.Min.z, out box.Max.z);
			return box;
		}

		public static AxisAlignedBox2d Bounds(ref Vector2d v0, ref Vector2d v1, ref Vector2d v2) {
			AxisAlignedBox2d box;
			MathUtil.MinMax(v0.x, v1.x, v2.x, out box.Min.x, out box.Max.x);
			MathUtil.MinMax(v0.y, v1.y, v2.y, out box.Min.y, out box.Max.y);
			return box;
		}

		// AABB of transformed AABB (corners)
		public static AxisAlignedBox3d Bounds(ref AxisAlignedBox3d boxIn, Func<Vector3d, Vector3d> TransformF) {
			if (TransformF == null) {
				return boxIn;
			}

			var box = new AxisAlignedBox3d(TransformF(boxIn.Corner(0)));
			for (var i = 1; i < 8; ++i) {
				box.Contain(TransformF(boxIn.Corner(i)));
			}

			return box;
		}


		public static AxisAlignedBox3d Bounds(IEnumerable<Vector3d> positions) {
			var box = AxisAlignedBox3d.Empty;
			foreach (var v in positions) {
				box.Contain(v);
			}

			return box;
		}
		public static AxisAlignedBox3f Bounds(IEnumerable<Vector3f> positions) {
			var box = AxisAlignedBox3f.Empty;
			foreach (var v in positions) {
				box.Contain(v);
			}

			return box;
		}


		public static AxisAlignedBox2d Bounds(IEnumerable<Vector2d> positions) {
			var box = AxisAlignedBox2d.Empty;
			foreach (var v in positions) {
				box.Contain(v);
			}

			return box;
		}
		public static AxisAlignedBox2f Bounds(IEnumerable<Vector2f> positions) {
			var box = AxisAlignedBox2f.Empty;
			foreach (var v in positions) {
				box.Contain(v);
			}

			return box;
		}


		public static AxisAlignedBox3d Bounds<T>(IEnumerable<T> values, Func<T, Vector3d> PositionF) {
			var box = AxisAlignedBox3d.Empty;
			foreach (var t in values) {
				box.Contain(PositionF(t));
			}

			return box;
		}
		public static AxisAlignedBox3f Bounds<T>(IEnumerable<T> values, Func<T, Vector3f> PositionF) {
			var box = AxisAlignedBox3f.Empty;
			foreach (var t in values) {
				box.Contain(PositionF(t));
			}

			return box;
		}


		/// <summary>
		/// compute axis-aligned bounds of set of points after transforming 
		/// </summary>
		public static AxisAlignedBox3d Bounds(IEnumerable<Vector3d> values, TransformSequence xform) {
			var box = AxisAlignedBox3d.Empty;
			foreach (var v in values) {
				box.Contain(xform.TransformP(v));
			}

			return box;
		}


		/// <summary>
		/// compute axis-aligned bounds of set of points after transforming into frame f
		/// </summary>
		public static AxisAlignedBox3d BoundsInFrame(IEnumerable<Vector3d> values, Frame3f f) {
			var box = AxisAlignedBox3d.Empty;
			foreach (var v in values) {
				box.Contain(f.ToFrameP(v));
			}

			return box;
		}
	}
}
