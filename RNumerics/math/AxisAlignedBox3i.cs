using System;
using System.Collections.Generic;

using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct AxisAlignedBox3i : IComparable<AxisAlignedBox3i>, IEquatable<AxisAlignedBox3i>
	{
		[Key(0)]
		public Vector3i min = new(int.MaxValue, int.MaxValue, int.MaxValue);
		[Key(1)]
		public Vector3i max = new(int.MinValue, int.MinValue, int.MinValue);


		[Exposed, IgnoreMember]
		public Vector3i Min
		{
			get => min;
			set => min = value;
		}
		[Exposed, IgnoreMember]
		public Vector3i Max
		{
			get => max;
			set => max = value;
		}



		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox3i Empty = new();
		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox3i Zero = new(0);
		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox3i UnitPositive = new(1);
		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox3i Infinite =
			new(int.MinValue, int.MinValue, int.MinValue, int.MaxValue, int.MaxValue, int.MaxValue);

		public AxisAlignedBox3i() {

		}
		public AxisAlignedBox3i(in int xmin, in int ymin, in int zmin, in int xmax, in int ymax, in int zmax) {
			min = new Vector3i(xmin, ymin, zmin);
			max = new Vector3i(xmax, ymax, zmax);
		}

		public AxisAlignedBox3i(in int fCubeSize) {
			min = new Vector3i(0, 0, 0);
			max = new Vector3i(fCubeSize, fCubeSize, fCubeSize);
		}

		public AxisAlignedBox3i(in int fWidth, in int fHeight, in int fDepth) {
			min = new Vector3i(0, 0, 0);
			max = new Vector3i(fWidth, fHeight, fDepth);
		}

		public AxisAlignedBox3i(in Vector3i vMin, in Vector3i vMax) {
			min = new Vector3i(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y), Math.Min(vMin.z, vMax.z));
			max = new Vector3i(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y), Math.Max(vMin.z, vMax.z));
		}

		public AxisAlignedBox3i(in Vector3i vCenter, in int fHalfWidth, in int fHalfHeight, in int fHalfDepth) {
			min = new Vector3i(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight, vCenter.z - fHalfDepth);
			max = new Vector3i(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight, vCenter.z + fHalfDepth);
		}
		public AxisAlignedBox3i(in Vector3i vCenter, in int fHalfSize) {
			min = new Vector3i(vCenter.x - fHalfSize, vCenter.y - fHalfSize, vCenter.z - fHalfSize);
			max = new Vector3i(vCenter.x + fHalfSize, vCenter.y + fHalfSize, vCenter.z + fHalfSize);
		}

		public AxisAlignedBox3i(in Vector3i vCenter) {
			min = max = vCenter;
		}

		[IgnoreMember]
		public int Width => Math.Max(max.x - min.x, 0);
		[IgnoreMember]
		public int Height => Math.Max(max.y - min.y, 0);
		[IgnoreMember]
		public int Depth => Math.Max(max.z - min.z, 0);

		[IgnoreMember]
		public int Volume => Width * Height * Depth;
		[IgnoreMember]
		public int DiagonalLength
		{
			get {
				return (int)Math.Sqrt(((max.x - min.x) * (max.x - min.x))
					+ ((max.y - min.y) * (max.y - min.y)) + ((max.z - min.z) * (max.z - min.z)));
			}
		}
		[IgnoreMember]
		public int MaxDim => Math.Max(Width, Math.Max(Height, Depth));

		[IgnoreMember]
		public Vector3i Diagonal => new(max.x - min.x, max.y - min.y, max.z - min.z);
		[IgnoreMember]
		public Vector3i Extents => new((max.x - min.x) / 2, (max.y - min.y) / 2, (max.z - min.z) / 2);
		[IgnoreMember]
		public Vector3i Center => new((min.x + max.x) / 2, (min.y + max.y) / 2, (min.z + max.z) / 2);


		public static bool operator ==(in AxisAlignedBox3i a, in AxisAlignedBox3i b) => a.min == b.min && a.max == b.max;
		public static bool operator !=(in AxisAlignedBox3i a, in AxisAlignedBox3i b) => a.min != b.min || a.max != b.max;
		public override bool Equals(object obj) {
			return this == (AxisAlignedBox3i)obj;
		}
		public bool Equals(AxisAlignedBox3i other) {
			return this == other;
		}
		public int CompareTo(AxisAlignedBox3i other) {
			var c = min.CompareTo(other.min);
			return c == 0 ? max.CompareTo(other.max) : c;
		}
		public override int GetHashCode() {
			return HashCode.Combine(min, max);
		}


		// TODO
		////! 0 == bottom-left, 1 = bottom-right, 2 == top-right, 3 == top-left
		//public Vector3i GetCorner(int i) {
		//    return new Vector3i((i % 3 == 0) ? Min.x : Max.x, (i < 2) ? Min.y : Max.y);
		//}

		//! value is subtracted from min and added to max
		public void Expand(in int nRadius) {
			min.x -= nRadius;
			min.y -= nRadius;
			min.z -= nRadius;
			max.x += nRadius;
			max.y += nRadius;
			max.z += nRadius;
		}
		//! value is added to min and subtracted from max
		public void Contract(in int nRadius) {
			min.x += nRadius;
			min.y += nRadius;
			min.z += nRadius;
			max.x -= nRadius;
			max.y -= nRadius;
			max.z -= nRadius;
		}

		public void Scale(in int sx, in int sy, in int sz) {
			var c = Center;
			var e = Extents;
			e.x *= sx;
			e.y *= sy;
			e.z *= sz;
			min = new Vector3i(c.x - e.x, c.y - e.y, c.z - e.z);
			max = new Vector3i(c.x + e.x, c.y + e.y, c.z + e.z);
		}

		public void Contain(in Vector3i v) {
			min.x = Math.Min(min.x, v.x);
			min.y = Math.Min(min.y, v.y);
			min.z = Math.Min(min.z, v.z);
			max.x = Math.Max(max.x, v.x);
			max.y = Math.Max(max.y, v.y);
			max.z = Math.Max(max.z, v.z);
		}

		public void Contain(in AxisAlignedBox3i box) {
			min.x = Math.Min(min.x, box.min.x);
			min.y = Math.Min(min.y, box.min.y);
			min.z = Math.Min(min.z, box.min.z);
			max.x = Math.Max(max.x, box.max.x);
			max.y = Math.Max(max.y, box.max.y);
			max.z = Math.Max(max.z, box.max.z);
		}


		public void Contain(in Vector3d v) {
			min.x = Math.Min(min.x, (int)v.x);
			min.y = Math.Min(min.y, (int)v.y);
			min.z = Math.Min(min.z, (int)v.z);
			max.x = Math.Max(max.x, (int)v.x);
			max.y = Math.Max(max.y, (int)v.y);
			max.z = Math.Max(max.z, (int)v.z);
		}

		public void Contain(in AxisAlignedBox3d box) {
			min.x = Math.Min(min.x, (int)box.min.x);
			min.y = Math.Min(min.y, (int)box.min.y);
			min.z = Math.Min(min.z, (int)box.min.z);
			max.x = Math.Max(max.x, (int)box.max.x);
			max.y = Math.Max(max.y, (int)box.max.y);
			max.z = Math.Max(max.z, (int)box.max.z);

		}


		public AxisAlignedBox3i Intersect(in AxisAlignedBox3i box) {
			var intersect = new AxisAlignedBox3i(
				Math.Max(min.x, box.min.x), Math.Max(min.y, box.min.y), Math.Max(min.z, box.min.z),
				Math.Min(max.x, box.max.x), Math.Min(max.y, box.max.y), Math.Min(max.z, box.max.z));
			return intersect.Height <= 0 || intersect.Width <= 0 || intersect.Depth <= 0 ? AxisAlignedBox3i.Empty : intersect;
		}



		public bool Contains(in Vector3i v) {
			return (min.x <= v.x) && (min.y <= v.y) && (min.z <= v.z)
				&& (max.x >= v.x) && (max.y >= v.y) && (max.z >= v.z);
		}
		public bool Intersects(in AxisAlignedBox3i box) {
			return !((box.max.x <= min.x) || (box.min.x >= max.x)
				|| (box.max.y <= min.y) || (box.min.y >= max.y)
				|| (box.max.z <= min.z) || (box.min.z >= max.z));
		}


		public double DistanceSquared(in Vector3i v) {
			var dx = (v.x < min.x) ? min.x - v.x : (v.x > max.x ? v.x - max.x : 0);
			var dy = (v.y < min.y) ? min.y - v.y : (v.y > max.y ? v.y - max.y : 0);
			var dz = (v.z < min.z) ? min.z - v.z : (v.z > max.z ? v.z - max.z : 0);
			return (dx * dx) + (dy * dy) + (dz * dz);
		}
		public int Distance(in Vector3i v) {
			return (int)Math.Sqrt(DistanceSquared(v));
		}


		public Vector3i NearestPoint(in Vector3i v) {
			var x = (v.x < min.x) ? min.x : (v.x > max.x ? max.x : v.x);
			var y = (v.y < min.y) ? min.y : (v.y > max.y ? max.y : v.y);
			var z = (v.z < min.z) ? min.z : (v.z > max.z ? max.z : v.z);
			return new Vector3i(x, y, z);
		}


		/// <summary>
		/// Clamp v to grid bounds [min, max]
		/// </summary>
		public Vector3i ClampInclusive(in Vector3i v) {
			return new Vector3i(
				MathUtil.Clamp(v.x, min.x, max.x),
				MathUtil.Clamp(v.y, min.y, max.y),
				MathUtil.Clamp(v.z, min.z, max.z));
		}

		/// <summary>
		/// clamp v to grid bounds [min,max)
		/// </summary>
		public Vector3i ClampExclusive(in Vector3i v) {
			return new Vector3i(
				MathUtil.Clamp(v.x, min.x, max.x - 1),
				MathUtil.Clamp(v.y, min.y, max.y - 1),
				MathUtil.Clamp(v.z, min.z, max.z - 1));
		}



		//! relative translation
		public void Translate(in Vector3i vTranslate) {
			min.Add(vTranslate);
			max.Add(vTranslate);
		}

		public void MoveMin(in Vector3i vNewMin) {
			max.x = vNewMin.x + (max.x - min.x);
			max.y = vNewMin.y + (max.y - min.y);
			max.z = vNewMin.z + (max.z - min.z);
			min.Set(vNewMin);
		}
		public void MoveMin(in int fNewX, in int fNewY, in int fNewZ) {
			max.x = fNewX + (max.x - min.x);
			max.y = fNewY + (max.y - min.y);
			max.z = fNewZ + (max.z - min.z);
			min.Set(fNewX, fNewY, fNewZ);
		}




		public IEnumerable<Vector3i> IndicesInclusive() {
			for (var zi = min.z; zi <= max.z; ++zi) {
				for (var yi = min.y; yi <= max.y; ++yi) {
					for (var xi = min.x; xi <= max.x; ++xi) {
						yield return new Vector3i(xi, yi, zi);
					}
				}
			}
		}
		public IEnumerable<Vector3i> IndicesExclusive() {
			for (var zi = min.z; zi < max.z; ++zi) {
				for (var yi = min.y; yi < max.y; ++yi) {
					for (var xi = min.x; xi < max.x; ++xi) {
						yield return new Vector3i(xi, yi, zi);
					}
				}
			}
		}



		public override string ToString() {
			return string.Format("x[{0},{1}] y[{2},{3}] z[{4},{5}]", min.x, max.x, min.y, max.y, min.z, max.z);
		}




	}
}
