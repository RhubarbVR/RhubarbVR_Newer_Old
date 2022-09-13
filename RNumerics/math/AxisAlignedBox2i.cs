using System;
using System.Collections.Generic;

using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct AxisAlignedBox2i : IComparable<AxisAlignedBox2i>, IEquatable<AxisAlignedBox2i>
	{
		[Key(0)]
		public Vector2i Min = new(int.MaxValue, int.MaxValue);
		[Key(1)]
		public Vector2i Max = new(int.MinValue, int.MinValue);
		[IgnoreMember]
		public static readonly AxisAlignedBox2i Empty = new();
		[IgnoreMember]
		public static readonly AxisAlignedBox2i Zero = new(0);
		[IgnoreMember]
		public static readonly AxisAlignedBox2i UnitPositive = new(1);
		[IgnoreMember]
		public static readonly AxisAlignedBox2i Infinite =
			new(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue);

		public AxisAlignedBox2i() {

		}
		public AxisAlignedBox2i(in int xmin, in int ymin, in int xmax, in int ymax) {
			Min = new Vector2i(xmin, ymin);
			Max = new Vector2i(xmax, ymax);
		}

		public AxisAlignedBox2i(in int fCubeSize) {
			Min = new Vector2i(0, 0);
			Max = new Vector2i(fCubeSize, fCubeSize);
		}

		public AxisAlignedBox2i(in int fWidth, in int fHeight) {
			Min = new Vector2i(0, 0);
			Max = new Vector2i(fWidth, fHeight);
		}

		public AxisAlignedBox2i(in Vector2i vMin, in Vector2i vMax) {
			Min = new Vector2i(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y));
			Max = new Vector2i(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y));
		}

		public AxisAlignedBox2i(in Vector2i vCenter, in int fHalfWidth, in int fHalfHeight) {
			Min = new Vector2i(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight);
			Max = new Vector2i(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight);
		}
		public AxisAlignedBox2i(in Vector2i vCenter, in int fHalfSize) {
			Min = new Vector2i(vCenter.x - fHalfSize, vCenter.y - fHalfSize);
			Max = new Vector2i(vCenter.x + fHalfSize, vCenter.y + fHalfSize);
		}

		public AxisAlignedBox2i(in Vector2i vCenter) {
			Min = Max = vCenter;
		}
		[IgnoreMember]
		public int Width => Math.Max(Max.x - Min.x, 0);
		[IgnoreMember]
		public int Height => Math.Max(Max.y - Min.y, 0);


		[IgnoreMember]
		public int Area => Width * Height;
		[IgnoreMember]
		public int DiagonalLength => (int)Math.Sqrt(((Max.x - Min.x) * (Max.x - Min.x)) + ((Max.y - Min.y) * (Max.y - Min.y)));
		[IgnoreMember]
		public int MaxDim => Math.Max(Width, Height);

		[IgnoreMember]
		public Vector2i Diagonal => new(Max.x - Min.x, Max.y - Min.y);
		[IgnoreMember]
		public Vector2i Extents => new((Max.x - Min.x) / 2, (Max.y - Min.y) / 2);
		[IgnoreMember]
		public Vector2i Center => new((Min.x + Max.x) / 2, (Min.y + Max.y) / 2);


		public static bool operator ==(AxisAlignedBox2i a, AxisAlignedBox2i b) => a.Min == b.Min && a.Max == b.Max;
		public static bool operator !=(AxisAlignedBox2i a, AxisAlignedBox2i b) => a.Min != b.Min || a.Max != b.Max;
		public override bool Equals(object obj) {
			return this == (AxisAlignedBox2i)obj;
		}
		public bool Equals(AxisAlignedBox2i other) {
			return this == other;
		}
		public int CompareTo(AxisAlignedBox2i other) {
			var c = Min.CompareTo(other.Min);
			return c == 0 ? Max.CompareTo(other.Max) : c;
		}
		public override int GetHashCode() {
			unchecked { // Overflow is fine, just wrap
				var hash = (int)2166136261;
				hash = (hash * 16777619) ^ Min.GetHashCode();
				hash = (hash * 16777619) ^ Max.GetHashCode();
				return hash;
			}
		}


		//! 0 == bottom-left, 1 = bottom-right, 2 == top-right, 3 == top-left
		public Vector2i GetCorner(in int i) {
			return new Vector2i((i % 3 == 0) ? Min.x : Max.x, (i < 2) ? Min.y : Max.y);
		}

		//! value is subtracted from min and added to max
		public void Expand(in int nRadius) {
			Min.x -= nRadius;
			Min.y -= nRadius;
			Max.x += nRadius;
			Max.y += nRadius;
		}
		//! value is added to min and subtracted from max
		public void Contract(in int nRadius) {
			Min.x += nRadius;
			Min.y += nRadius;
			Max.x -= nRadius;
			Max.y -= nRadius;
		}

		public void Scale(in int sx, in int sy, in int sz = 1) {
			var c = Center;
			var e = Extents;
			e.x *= sx * sz;
			e.y *= sy * sz;
			Min = new Vector2i(c.x - e.x, c.y - e.y);
			Max = new Vector2i(c.x + e.x, c.y + e.y);
		}

		public void Contain(in Vector2i v) {
			Min.x = Math.Min(Min.x, v.x);
			Min.y = Math.Min(Min.y, v.y);
			Max.x = Math.Max(Max.x, v.x);
			Max.y = Math.Max(Max.y, v.y);
		}

		public void Contain(in AxisAlignedBox2i box) {
			Min.x = Math.Min(Min.x, box.Min.x);
			Min.y = Math.Min(Min.y, box.Min.y);
			Max.x = Math.Max(Max.x, box.Max.x);
			Max.y = Math.Max(Max.y, box.Max.y);
		}


		public void Contain(in Vector3d v) {
			Min.x = Math.Min(Min.x, (int)v.x);
			Min.y = Math.Min(Min.y, (int)v.y);
			Max.x = Math.Max(Max.x, (int)v.x);
			Max.y = Math.Max(Max.y, (int)v.y);
		}

		public void Contain(in AxisAlignedBox3d box) {
			Min.x = Math.Min(Min.x, (int)box.Min.x);
			Min.y = Math.Min(Min.y, (int)box.Min.y);
			Max.x = Math.Max(Max.x, (int)box.Max.x);
			Max.y = Math.Max(Max.y, (int)box.Max.y);
		}


		public AxisAlignedBox2i Intersect(in AxisAlignedBox2i box) {
			var intersect = new AxisAlignedBox2i(
				Math.Max(Min.x, box.Min.x), Math.Max(Min.y, box.Min.y),
				Math.Min(Max.x, box.Max.x), Math.Min(Max.y, box.Max.y));
			return intersect.Height <= 0 || intersect.Width <= 0 ? AxisAlignedBox2i.Empty : intersect;
		}



		public bool Contains(in Vector2i v) {
			return (Min.x <= v.x) && (Min.y <= v.y)
				&& (Max.x >= v.x) && (Max.y >= v.y);
		}


		public bool Contains(in AxisAlignedBox2i box) {
			return Contains(box.Min) && Contains(box.Max);
		}



		public bool Intersects(in AxisAlignedBox2i box) {
			return !((box.Max.x <= Min.x) || (box.Min.x >= Max.x)
				|| (box.Max.y <= Min.y) || (box.Min.y >= Max.y));
		}


		public double DistanceSquared(in Vector2i v) {
			var dx = (v.x < Min.x) ? Min.x - v.x : (v.x > Max.x ? v.x - Max.x : 0);
			var dy = (v.y < Min.y) ? Min.y - v.y : (v.y > Max.y ? v.y - Max.y : 0);
			return (dx * dx) + (dy * dy);
		}
		public int Distance(in Vector2i v) {
			return (int)Math.Sqrt(DistanceSquared(v));
		}


		public Vector2i NearestPoint(in Vector2i v) {
			var x = (v.x < Min.x) ? Min.x : (v.x > Max.x ? Max.x : v.x);
			var y = (v.y < Min.y) ? Min.y : (v.y > Max.y ? Max.y : v.y);
			return new Vector2i(x, y);
		}



		//! relative translation
		public void Translate(in Vector2i vTranslate) {
			Min += vTranslate;
			Max += vTranslate;
		}

		public void MoveMin(in Vector2i vNewMin) {
			Max.x = vNewMin.x + (Max.x - Min.x);
			Max.y = vNewMin.y + (Max.y - Min.y);
			Min = vNewMin;
		}
		public void MoveMin(in int fNewX, in int fNewY) {
			Max.x = fNewX + (Max.x - Min.x);
			Max.y = fNewY + (Max.y - Min.y);
			Min = new Vector2i(fNewX, fNewY);
		}




		public IEnumerable<Vector2i> IndicesInclusive() {
			for (var yi = Min.y; yi <= Max.y; ++yi) {
				for (var xi = Min.x; xi <= Max.x; ++xi) {
					yield return new Vector2i(xi, yi);
				}
			}
		}
		public IEnumerable<Vector2i> IndicesExclusive() {
			for (var yi = Min.y; yi < Max.y; ++yi) {
				for (var xi = Min.x; xi < Max.x; ++xi) {
					yield return new Vector2i(xi, yi);
				}
			}
		}


		public override string ToString() {
			return string.Format("x[{0},{1}] y[{2},{3}]", Min.x, Max.x, Min.y, Max.y);
		}




	}
}
