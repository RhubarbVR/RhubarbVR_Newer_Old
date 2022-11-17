using System;
using System.Collections.Generic;

using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct AxisAlignedBox2i : IComparable<AxisAlignedBox2i>, IEquatable<AxisAlignedBox2i>
	{
		[Key(0)]
		public Vector2i min = new(int.MaxValue, int.MaxValue);
		[Key(1)]
		public Vector2i max = new(int.MinValue, int.MinValue);

		[Exposed, IgnoreMember]
		public Vector2i Min
		{
			get => min;
			set => min = value;
		}
		[Exposed, IgnoreMember]
		public Vector2i Max
		{
			get => max;
			set => max = value;
		}

		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox2i Empty = new();
		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox2i Zero = new(0);
		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox2i UnitPositive = new(1);
		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox2i Infinite =
			new(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue);

		public AxisAlignedBox2i() {

		}
		public AxisAlignedBox2i(in int xmin, in int ymin, in int xmax, in int ymax) {
			min = new Vector2i(xmin, ymin);
			max = new Vector2i(xmax, ymax);
		}

		public AxisAlignedBox2i(in int fCubeSize) {
			min = new Vector2i(0, 0);
			max = new Vector2i(fCubeSize, fCubeSize);
		}

		public AxisAlignedBox2i(in int fWidth, in int fHeight) {
			min = new Vector2i(0, 0);
			max = new Vector2i(fWidth, fHeight);
		}

		public AxisAlignedBox2i(in Vector2i vMin, in Vector2i vMax) {
			min = new Vector2i(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y));
			max = new Vector2i(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y));
		}

		public AxisAlignedBox2i(in Vector2i vCenter, in int fHalfWidth, in int fHalfHeight) {
			min = new Vector2i(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight);
			max = new Vector2i(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight);
		}
		public AxisAlignedBox2i(in Vector2i vCenter, in int fHalfSize) {
			min = new Vector2i(vCenter.x - fHalfSize, vCenter.y - fHalfSize);
			max = new Vector2i(vCenter.x + fHalfSize, vCenter.y + fHalfSize);
		}

		public AxisAlignedBox2i(in Vector2i vCenter) {
			min = max = vCenter;
		}
		[IgnoreMember]
		public int Width => Math.Max(max.x - min.x, 0);
		[IgnoreMember]
		public int Height => Math.Max(max.y - min.y, 0);


		[IgnoreMember]
		public int Area => Width * Height;
		[IgnoreMember]
		public int DiagonalLength => (int)Math.Sqrt(((max.x - min.x) * (max.x - min.x)) + ((max.y - min.y) * (max.y - min.y)));
		[IgnoreMember]
		public int MaxDim => Math.Max(Width, Height);

		[IgnoreMember]
		public Vector2i Diagonal => new(max.x - min.x, max.y - min.y);
		[IgnoreMember]
		public Vector2i Extents => new((max.x - min.x) / 2, (max.y - min.y) / 2);
		[IgnoreMember]
		public Vector2i Center => new((min.x + max.x) / 2, (min.y + max.y) / 2);


		public static bool operator ==(AxisAlignedBox2i a, AxisAlignedBox2i b) => a.min == b.min && a.max == b.max;
		public static bool operator !=(AxisAlignedBox2i a, AxisAlignedBox2i b) => a.min != b.min || a.max != b.max;
		public override bool Equals(object obj) {
			return this == (AxisAlignedBox2i)obj;
		}
		public bool Equals(AxisAlignedBox2i other) {
			return this == other;
		}
		public int CompareTo(AxisAlignedBox2i other) {
			var c = min.CompareTo(other.min);
			return c == 0 ? max.CompareTo(other.max) : c;
		}
		public override int GetHashCode() {
			unchecked { // Overflow is fine, just wrap
				var hash = (int)2166136261;
				hash = (hash * 16777619) ^ min.GetHashCode();
				hash = (hash * 16777619) ^ max.GetHashCode();
				return hash;
			}
		}


		//! 0 == bottom-left, 1 = bottom-right, 2 == top-right, 3 == top-left
		public Vector2i GetCorner(in int i) {
			return new Vector2i((i % 3 == 0) ? min.x : max.x, (i < 2) ? min.y : max.y);
		}

		//! value is subtracted from min and added to max
		public void Expand(in int nRadius) {
			min.x -= nRadius;
			min.y -= nRadius;
			max.x += nRadius;
			max.y += nRadius;
		}
		//! value is added to min and subtracted from max
		public void Contract(in int nRadius) {
			min.x += nRadius;
			min.y += nRadius;
			max.x -= nRadius;
			max.y -= nRadius;
		}

		public void Scale(in int sx, in int sy, in int sz = 1) {
			var c = Center;
			var e = Extents;
			e.x *= sx * sz;
			e.y *= sy * sz;
			min = new Vector2i(c.x - e.x, c.y - e.y);
			max = new Vector2i(c.x + e.x, c.y + e.y);
		}

		public void Contain(in Vector2i v) {
			min.x = Math.Min(min.x, v.x);
			min.y = Math.Min(min.y, v.y);
			max.x = Math.Max(max.x, v.x);
			max.y = Math.Max(max.y, v.y);
		}

		public void Contain(in AxisAlignedBox2i box) {
			min.x = Math.Min(min.x, box.min.x);
			min.y = Math.Min(min.y, box.min.y);
			max.x = Math.Max(max.x, box.max.x);
			max.y = Math.Max(max.y, box.max.y);
		}


		public void Contain(in Vector3d v) {
			min.x = Math.Min(min.x, (int)v.x);
			min.y = Math.Min(min.y, (int)v.y);
			max.x = Math.Max(max.x, (int)v.x);
			max.y = Math.Max(max.y, (int)v.y);
		}

		public void Contain(in AxisAlignedBox3d box) {
			min.x = Math.Min(min.x, (int)box.min.x);
			min.y = Math.Min(min.y, (int)box.min.y);
			max.x = Math.Max(max.x, (int)box.max.x);
			max.y = Math.Max(max.y, (int)box.max.y);
		}


		public AxisAlignedBox2i Intersect(in AxisAlignedBox2i box) {
			var intersect = new AxisAlignedBox2i(
				Math.Max(min.x, box.min.x), Math.Max(min.y, box.min.y),
				Math.Min(max.x, box.max.x), Math.Min(max.y, box.max.y));
			return intersect.Height <= 0 || intersect.Width <= 0 ? AxisAlignedBox2i.Empty : intersect;
		}



		public bool Contains(in Vector2i v) {
			return (min.x <= v.x) && (min.y <= v.y)
				&& (max.x >= v.x) && (max.y >= v.y);
		}


		public bool Contains(in AxisAlignedBox2i box) {
			return Contains(box.min) && Contains(box.max);
		}



		public bool Intersects(in AxisAlignedBox2i box) {
			return !((box.max.x <= min.x) || (box.min.x >= max.x)
				|| (box.max.y <= min.y) || (box.min.y >= max.y));
		}


		public double DistanceSquared(in Vector2i v) {
			var dx = (v.x < min.x) ? min.x - v.x : (v.x > max.x ? v.x - max.x : 0);
			var dy = (v.y < min.y) ? min.y - v.y : (v.y > max.y ? v.y - max.y : 0);
			return (dx * dx) + (dy * dy);
		}
		public int Distance(in Vector2i v) {
			return (int)Math.Sqrt(DistanceSquared(v));
		}


		public Vector2i NearestPoint(in Vector2i v) {
			var x = (v.x < min.x) ? min.x : (v.x > max.x ? max.x : v.x);
			var y = (v.y < min.y) ? min.y : (v.y > max.y ? max.y : v.y);
			return new Vector2i(x, y);
		}



		//! relative translation
		public void Translate(in Vector2i vTranslate) {
			min += vTranslate;
			max += vTranslate;
		}

		public void MoveMin(in Vector2i vNewMin) {
			max.x = vNewMin.x + (max.x - min.x);
			max.y = vNewMin.y + (max.y - min.y);
			min = vNewMin;
		}
		public void MoveMin(in int fNewX, in int fNewY) {
			max.x = fNewX + (max.x - min.x);
			max.y = fNewY + (max.y - min.y);
			min = new Vector2i(fNewX, fNewY);
		}




		public IEnumerable<Vector2i> IndicesInclusive() {
			for (var yi = min.y; yi <= max.y; ++yi) {
				for (var xi = min.x; xi <= max.x; ++xi) {
					yield return new Vector2i(xi, yi);
				}
			}
		}
		public IEnumerable<Vector2i> IndicesExclusive() {
			for (var yi = min.y; yi < max.y; ++yi) {
				for (var xi = min.x; xi < max.x; ++xi) {
					yield return new Vector2i(xi, yi);
				}
			}
		}


		public override string ToString() {
			return string.Format("x[{0},{1}] y[{2},{3}]", min.x, max.x, min.y, max.y);
		}




	}
}
