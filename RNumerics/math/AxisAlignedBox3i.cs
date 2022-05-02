using System;
using System.Collections.Generic;

using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct AxisAlignedBox3i : IComparable<AxisAlignedBox3i>, IEquatable<AxisAlignedBox3i>
	{
		[Key(0)]
		public Vector3i Min = new Vector3i(int.MaxValue, int.MaxValue, int.MaxValue);
		[Key(1)]
		public Vector3i Max = new Vector3i(int.MinValue, int.MinValue, int.MinValue);

		[IgnoreMember]
		public static readonly AxisAlignedBox3i Empty = new();
		[IgnoreMember]
		public static readonly AxisAlignedBox3i Zero = new(0);
		[IgnoreMember]
		public static readonly AxisAlignedBox3i UnitPositive = new(1);
		[IgnoreMember]
		public static readonly AxisAlignedBox3i Infinite =
			new(int.MinValue, int.MinValue, int.MinValue, int.MaxValue, int.MaxValue, int.MaxValue);


		public AxisAlignedBox3i(int xmin, int ymin, int zmin, int xmax, int ymax, int zmax) {
			Min = new Vector3i(xmin, ymin, zmin);
			Max = new Vector3i(xmax, ymax, zmax);
		}

		public AxisAlignedBox3i(int fCubeSize) {
			Min = new Vector3i(0, 0, 0);
			Max = new Vector3i(fCubeSize, fCubeSize, fCubeSize);
		}

		public AxisAlignedBox3i(int fWidth, int fHeight, int fDepth) {
			Min = new Vector3i(0, 0, 0);
			Max = new Vector3i(fWidth, fHeight, fDepth);
		}

		public AxisAlignedBox3i(Vector3i vMin, Vector3i vMax) {
			Min = new Vector3i(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y), Math.Min(vMin.z, vMax.z));
			Max = new Vector3i(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y), Math.Max(vMin.z, vMax.z));
		}

		public AxisAlignedBox3i(Vector3i vCenter, int fHalfWidth, int fHalfHeight, int fHalfDepth) {
			Min = new Vector3i(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight, vCenter.z - fHalfDepth);
			Max = new Vector3i(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight, vCenter.z + fHalfDepth);
		}
		public AxisAlignedBox3i(Vector3i vCenter, int fHalfSize) {
			Min = new Vector3i(vCenter.x - fHalfSize, vCenter.y - fHalfSize, vCenter.z - fHalfSize);
			Max = new Vector3i(vCenter.x + fHalfSize, vCenter.y + fHalfSize, vCenter.z + fHalfSize);
		}

		public AxisAlignedBox3i(Vector3i vCenter) {
			Min = Max = vCenter;
		}

		[IgnoreMember]
		public int Width => Math.Max(Max.x - Min.x, 0);
		[IgnoreMember]
		public int Height => Math.Max(Max.y - Min.y, 0);
		[IgnoreMember]
		public int Depth => Math.Max(Max.z - Min.z, 0);

		[IgnoreMember]
		public int Volume => Width * Height * Depth;
		[IgnoreMember]
		public int DiagonalLength
		{
			get {
				return (int)Math.Sqrt(((Max.x - Min.x) * (Max.x - Min.x))
					+ ((Max.y - Min.y) * (Max.y - Min.y)) + ((Max.z - Min.z) * (Max.z - Min.z)));
			}
		}
		[IgnoreMember]
		public int MaxDim => Math.Max(Width, Math.Max(Height, Depth));

		[IgnoreMember]
		public Vector3i Diagonal => new(Max.x - Min.x, Max.y - Min.y, Max.z - Min.z);
		[IgnoreMember]
		public Vector3i Extents => new((Max.x - Min.x) / 2, (Max.y - Min.y) / 2, (Max.z - Min.z) / 2);
		[IgnoreMember]
		public Vector3i Center => new((Min.x + Max.x) / 2, (Min.y + Max.y) / 2, (Min.z + Max.z) / 2);


		public static bool operator ==(AxisAlignedBox3i a, AxisAlignedBox3i b) => a.Min == b.Min && a.Max == b.Max;
		public static bool operator !=(AxisAlignedBox3i a, AxisAlignedBox3i b) => a.Min != b.Min || a.Max != b.Max;
		public override bool Equals(object obj) {
			return this == (AxisAlignedBox3i)obj;
		}
		public bool Equals(AxisAlignedBox3i other) {
			return this == other;
		}
		public int CompareTo(AxisAlignedBox3i other) {
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


		// TODO
		////! 0 == bottom-left, 1 = bottom-right, 2 == top-right, 3 == top-left
		//public Vector3i GetCorner(int i) {
		//    return new Vector3i((i % 3 == 0) ? Min.x : Max.x, (i < 2) ? Min.y : Max.y);
		//}

		//! value is subtracted from min and added to max
		public void Expand(int nRadius) {
			Min.x -= nRadius;
			Min.y -= nRadius;
			Min.z -= nRadius;
			Max.x += nRadius;
			Max.y += nRadius;
			Max.z += nRadius;
		}
		//! value is added to min and subtracted from max
		public void Contract(int nRadius) {
			Min.x += nRadius;
			Min.y += nRadius;
			Min.z += nRadius;
			Max.x -= nRadius;
			Max.y -= nRadius;
			Max.z -= nRadius;
		}

		public void Scale(int sx, int sy, int sz) {
			var c = Center;
			var e = Extents;
			e.x *= sx;
			e.y *= sy;
			e.z *= sz;
			Min = new Vector3i(c.x - e.x, c.y - e.y, c.z - e.z);
			Max = new Vector3i(c.x + e.x, c.y + e.y, c.z + e.z);
		}

		public void Contain(Vector3i v) {
			Min.x = Math.Min(Min.x, v.x);
			Min.y = Math.Min(Min.y, v.y);
			Min.z = Math.Min(Min.z, v.z);
			Max.x = Math.Max(Max.x, v.x);
			Max.y = Math.Max(Max.y, v.y);
			Max.z = Math.Max(Max.z, v.z);
		}

		public void Contain(AxisAlignedBox3i box) {
			Min.x = Math.Min(Min.x, box.Min.x);
			Min.y = Math.Min(Min.y, box.Min.y);
			Min.z = Math.Min(Min.z, box.Min.z);
			Max.x = Math.Max(Max.x, box.Max.x);
			Max.y = Math.Max(Max.y, box.Max.y);
			Max.z = Math.Max(Max.z, box.Max.z);
		}


		public void Contain(Vector3d v) {
			Min.x = Math.Min(Min.x, (int)v.x);
			Min.y = Math.Min(Min.y, (int)v.y);
			Min.z = Math.Min(Min.z, (int)v.z);
			Max.x = Math.Max(Max.x, (int)v.x);
			Max.y = Math.Max(Max.y, (int)v.y);
			Max.z = Math.Max(Max.z, (int)v.z);
		}

		public void Contain(AxisAlignedBox3d box) {
			Min.x = Math.Min(Min.x, (int)box.Min.x);
			Min.y = Math.Min(Min.y, (int)box.Min.y);
			Min.z = Math.Min(Min.z, (int)box.Min.z);
			Max.x = Math.Max(Max.x, (int)box.Max.x);
			Max.y = Math.Max(Max.y, (int)box.Max.y);
			Max.z = Math.Max(Max.z, (int)box.Max.z);

		}


		public AxisAlignedBox3i Intersect(AxisAlignedBox3i box) {
			var intersect = new AxisAlignedBox3i(
				Math.Max(Min.x, box.Min.x), Math.Max(Min.y, box.Min.y), Math.Max(Min.z, box.Min.z),
				Math.Min(Max.x, box.Max.x), Math.Min(Max.y, box.Max.y), Math.Min(Max.z, box.Max.z));
			return intersect.Height <= 0 || intersect.Width <= 0 || intersect.Depth <= 0 ? AxisAlignedBox3i.Empty : intersect;
		}



		public bool Contains(Vector3i v) {
			return (Min.x <= v.x) && (Min.y <= v.y) && (Min.z <= v.z)
				&& (Max.x >= v.x) && (Max.y >= v.y) && (Max.z >= v.z);
		}
		public bool Intersects(AxisAlignedBox3i box) {
			return !((box.Max.x <= Min.x) || (box.Min.x >= Max.x)
				|| (box.Max.y <= Min.y) || (box.Min.y >= Max.y)
				|| (box.Max.z <= Min.z) || (box.Min.z >= Max.z));
		}


		public double DistanceSquared(Vector3i v) {
			var dx = (v.x < Min.x) ? Min.x - v.x : (v.x > Max.x ? v.x - Max.x : 0);
			var dy = (v.y < Min.y) ? Min.y - v.y : (v.y > Max.y ? v.y - Max.y : 0);
			var dz = (v.z < Min.z) ? Min.z - v.z : (v.z > Max.z ? v.z - Max.z : 0);
			return (dx * dx) + (dy * dy) + (dz * dz);
		}
		public int Distance(Vector3i v) {
			return (int)Math.Sqrt(DistanceSquared(v));
		}


		public Vector3i NearestPoint(Vector3i v) {
			var x = (v.x < Min.x) ? Min.x : (v.x > Max.x ? Max.x : v.x);
			var y = (v.y < Min.y) ? Min.y : (v.y > Max.y ? Max.y : v.y);
			var z = (v.z < Min.z) ? Min.z : (v.z > Max.z ? Max.z : v.z);
			return new Vector3i(x, y, z);
		}


		/// <summary>
		/// Clamp v to grid bounds [min, max]
		/// </summary>
		public Vector3i ClampInclusive(Vector3i v) {
			return new Vector3i(
				MathUtil.Clamp(v.x, Min.x, Max.x),
				MathUtil.Clamp(v.y, Min.y, Max.y),
				MathUtil.Clamp(v.z, Min.z, Max.z));
		}

		/// <summary>
		/// clamp v to grid bounds [min,max)
		/// </summary>
		public Vector3i ClampExclusive(Vector3i v) {
			return new Vector3i(
				MathUtil.Clamp(v.x, Min.x, Max.x - 1),
				MathUtil.Clamp(v.y, Min.y, Max.y - 1),
				MathUtil.Clamp(v.z, Min.z, Max.z - 1));
		}



		//! relative translation
		public void Translate(Vector3i vTranslate) {
			Min.Add(vTranslate);
			Max.Add(vTranslate);
		}

		public void MoveMin(Vector3i vNewMin) {
			Max.x = vNewMin.x + (Max.x - Min.x);
			Max.y = vNewMin.y + (Max.y - Min.y);
			Max.z = vNewMin.z + (Max.z - Min.z);
			Min.Set(vNewMin);
		}
		public void MoveMin(int fNewX, int fNewY, int fNewZ) {
			Max.x = fNewX + (Max.x - Min.x);
			Max.y = fNewY + (Max.y - Min.y);
			Max.z = fNewZ + (Max.z - Min.z);
			Min.Set(fNewX, fNewY, fNewZ);
		}




		public IEnumerable<Vector3i> IndicesInclusive() {
			for (var zi = Min.z; zi <= Max.z; ++zi) {
				for (var yi = Min.y; yi <= Max.y; ++yi) {
					for (var xi = Min.x; xi <= Max.x; ++xi) {
						yield return new Vector3i(xi, yi, zi);
					}
				}
			}
		}
		public IEnumerable<Vector3i> IndicesExclusive() {
			for (var zi = Min.z; zi < Max.z; ++zi) {
				for (var yi = Min.y; yi < Max.y; ++yi) {
					for (var xi = Min.x; xi < Max.x; ++xi) {
						yield return new Vector3i(xi, yi, zi);
					}
				}
			}
		}



		public override string ToString() {
			return string.Format("x[{0},{1}] y[{2},{3}] z[{4},{5}]", Min.x, Max.x, Min.y, Max.y, Min.z, Max.z);
		}




	}
}
