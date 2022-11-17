using System;

using MessagePack;
namespace RNumerics
{
	[MessagePackObject]
	public struct AxisAlignedBox3d : IComparable<AxisAlignedBox3d>, IEquatable<AxisAlignedBox3d>
	{
		[Key(0)]
		public Vector3d min = new(double.MaxValue, double.MaxValue, double.MaxValue);
		[Key(1)]
		public Vector3d max = new(double.MinValue, double.MinValue, double.MinValue);

		[Exposed, IgnoreMember]
		public Vector3d Min
		{
			get => min;
			set => min = value;
		}
		[Exposed, IgnoreMember]
		public Vector3d Max
		{
			get => max;
			set => max = value;
		}

		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox3d Empty = new();
		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox3d Zero = new(0);
		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox3d UnitPositive = new(1);
		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox3d Infinite =
			new(double.MinValue, double.MinValue, double.MinValue, double.MaxValue, double.MaxValue, double.MaxValue);
		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox3d CenterZero =
			new(Vector3d.Zero, 0);

		public AxisAlignedBox3d() {

		}
		public AxisAlignedBox3d(in double xmin, in double ymin, in double zmin, in double xmax, in double ymax, in double zmax) {
			min = new Vector3d(xmin, ymin, zmin);
			max = new Vector3d(xmax, ymax, zmax);
		}

		/// <summary>
		/// init box [0,size] x [0,size] x [0,size]
		/// </summary>
		public AxisAlignedBox3d(in double fCubeSize) {
			min = new Vector3d(0, 0, 0);
			max = new Vector3d(fCubeSize, fCubeSize, fCubeSize);
		}

		/// <summary>
		/// Init box [0,width] x [0,height] x [0,depth]
		/// </summary>
		public AxisAlignedBox3d(in double fWidth, in double fHeight, in double fDepth) {
			min = new Vector3d(0, 0, 0);
			max = new Vector3d(fWidth, fHeight, fDepth);
		}

		public AxisAlignedBox3d(in Vector3d vMin, in Vector3d vMax) {
			min = new Vector3d(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y), Math.Min(vMin.z, vMax.z));
			max = new Vector3d(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y), Math.Max(vMin.z, vMax.z));
		}

		public AxisAlignedBox3d(in Vector3d vCenter, in double fHalfWidth, in double fHalfHeight, in double fHalfDepth) {
			min = new Vector3d(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight, vCenter.z - fHalfDepth);
			max = new Vector3d(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight, vCenter.z + fHalfDepth);
		}


		public AxisAlignedBox3d(in Vector3d vCenter, in double fHalfSize) {
			min = new Vector3d(vCenter.x - fHalfSize, vCenter.y - fHalfSize, vCenter.z - fHalfSize);
			max = new Vector3d(vCenter.x + fHalfSize, vCenter.y + fHalfSize, vCenter.z + fHalfSize);
		}

		public AxisAlignedBox3d(in Vector3d vCenter) {
			min = max = vCenter;
		}

		[IgnoreMember]
		public double Width => Math.Max(max.x - min.x, 0);
		[IgnoreMember]
		public double Height => Math.Max(max.y - min.y, 0);
		[IgnoreMember]
		public double Depth => Math.Max(max.z - min.z, 0);

		[IgnoreMember]
		public double Volume => Width * Height * Depth;
		[IgnoreMember]
		public double DiagonalLength
		=> (double)Math.Sqrt(((max.x - min.x) * (max.x - min.x)) + ((max.y - min.y) * (max.y - min.y)) + ((max.z - min.z) * (max.z - min.z)));
		[IgnoreMember]
		public double MaxDim => Math.Max(Width, Math.Max(Height, Depth));

		[IgnoreMember]
		public Vector3d Diagonal => new(max.x - min.x, max.y - min.y, max.z - min.z);
		[IgnoreMember]
		public Vector3d Extents => new((max.x - min.x) * 0.5, (max.y - min.y) * 0.5, (max.z - min.z) * 0.5);
		[IgnoreMember]
		public Vector3d Center => new(0.5 * (min.x + max.x), 0.5 * (min.y + max.y), 0.5 * (min.z + max.z));


		public static bool operator ==(in AxisAlignedBox3d a, in AxisAlignedBox3d b) => a.min == b.min && a.max == b.max;
		public static bool operator !=(in AxisAlignedBox3d a, in AxisAlignedBox3d b) => a.min != b.min || a.max != b.max;
		public override bool Equals(object obj) {
			return this == (AxisAlignedBox3d)obj;
		}
		public bool Equals(AxisAlignedBox3d other) {
			return this == other;
		}
		public int CompareTo(AxisAlignedBox3d other) {
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


		// See Box3.Corner for details on which corner is which
		public Vector3d Corner(in int i) {
			var x = (((i & 1) != 0) ^ ((i & 2) != 0)) ? max.x : min.x;
			var y = (i / 2 % 2 == 0) ? min.y : max.y;
			var z = (i < 4) ? min.z : max.z;
			return new Vector3d(x, y, z);
		}

		/// <summary>
		/// Returns point on face/edge/corner. For each coord value neg==min, 0==center, pos==max
		/// </summary>
		public Vector3d Point(in int xi, in int yi, in int zi) {
			var x = (xi < 0) ? min.x : ((xi == 0) ? (0.5 * (min.x + max.x)) : max.x);
			var y = (yi < 0) ? min.y : ((yi == 0) ? (0.5 * (min.y + max.y)) : max.y);
			var z = (zi < 0) ? min.z : ((zi == 0) ? (0.5 * (min.z + max.z)) : max.z);
			return new Vector3d(x, y, z);
		}


		// TODO
		////! 0 == bottom-left, 1 = bottom-right, 2 == top-right, 3 == top-left
		//public Vector3d GetCorner(int i) {
		//    return new Vector3d((i % 3 == 0) ? Min.x : Max.x, (i < 2) ? Min.y : Max.y);
		//}

		//! value is subtracted from min and added to max
		public void Expand(in double fRadius) {
			min.x -= fRadius;
			min.y -= fRadius;
			min.z -= fRadius;
			max.x += fRadius;
			max.y += fRadius;
			max.z += fRadius;
		}

		//! return this box expanded by radius
		public AxisAlignedBox3d Expanded(in double fRadius) {
			return new AxisAlignedBox3d(
				min.x - fRadius, min.y - fRadius, min.z - fRadius,
				max.x + fRadius, max.y + fRadius, max.z + fRadius);
		}

		//! value is added to min and subtracted from max
		public void Contract(in double fRadius) {
			var w = 2 * fRadius;
			if (w > max.x - min.x) { min.x = max.x = 0.5 * (min.x + max.x); }
			else { min.x += fRadius; max.x -= fRadius; }
			if (w > max.y - min.y) { min.y = max.y = 0.5 * (min.y + max.y); }
			else { min.y += fRadius; max.y -= fRadius; }
			if (w > max.z - min.z) { min.z = max.z = 0.5 * (min.z + max.z); }
			else { min.z += fRadius; max.z -= fRadius; }
		}

		//! return this box expanded by radius
		public AxisAlignedBox3d Contracted(in double fRadius) {
			var result = new AxisAlignedBox3d(
				min.x + fRadius, min.y + fRadius, min.z + fRadius,
				max.x - fRadius, max.y - fRadius, max.z - fRadius);
			if (result.min.x > result.max.x) { result.min.x = result.max.x = 0.5 * (min.x + max.x); }
			if (result.min.y > result.max.y) { result.min.y = result.max.y = 0.5 * (min.y + max.y); }
			if (result.min.z > result.max.z) { result.min.z = result.max.z = 0.5 * (min.z + max.z); }
			return result;
		}


		public void Scale(in double sx, in double sy, in double sz) {
			var c = Center;
			var e = Extents;
			e.x *= sx;
			e.y *= sy;
			e.z *= sz;
			min = new Vector3d(c.x - e.x, c.y - e.y, c.z - e.z);
			max = new Vector3d(c.x + e.x, c.y + e.y, c.z + e.z);
		}

		public void Contain(in Vector3d v) {
			min.x = Math.Min(min.x, v.x);
			min.y = Math.Min(min.y, v.y);
			min.z = Math.Min(min.z, v.z);
			max.x = Math.Max(max.x, v.x);
			max.y = Math.Max(max.y, v.y);
			max.z = Math.Max(max.z, v.z);
		}

		public void Contain(in AxisAlignedBox3d box) {
			min.x = Math.Min(min.x, box.min.x);
			min.y = Math.Min(min.y, box.min.y);
			min.z = Math.Min(min.z, box.min.z);
			max.x = Math.Max(max.x, box.max.x);
			max.y = Math.Max(max.y, box.max.y);
			max.z = Math.Max(max.z, box.max.z);
		}

		public AxisAlignedBox3d Intersect(in AxisAlignedBox3d box) {
			var intersect = new AxisAlignedBox3d(
				Math.Max(min.x, box.min.x), Math.Max(min.y, box.min.y), Math.Max(min.z, box.min.z),
				Math.Min(max.x, box.max.x), Math.Min(max.y, box.max.y), Math.Min(max.z, box.max.z));
			return intersect.Height <= 0 || intersect.Width <= 0 || intersect.Depth <= 0 ? AxisAlignedBox3d.Empty : intersect;
		}



		public bool Contains(in Vector3d v) {
			return (min.x <= v.x) && (min.y <= v.y) && (min.z <= v.z)
				&& (max.x >= v.x) && (max.y >= v.y) && (max.z >= v.z);
		}


		public bool Contains(in AxisAlignedBox3d box2) {
			return Contains(box2.min) && Contains(box2.max);
		}


		public bool Intersects(in AxisAlignedBox3d box) {
			return !((box.max.x <= min.x) || (box.min.x >= max.x)
				|| (box.max.y <= min.y) || (box.min.y >= max.y)
				|| (box.max.z <= min.z) || (box.min.z >= max.z));
		}



		public double DistanceSquared(in Vector3d v) {
			var dx = (v.x < min.x) ? min.x - v.x : (v.x > max.x ? v.x - max.x : 0);
			var dy = (v.y < min.y) ? min.y - v.y : (v.y > max.y ? v.y - max.y : 0);
			var dz = (v.z < min.z) ? min.z - v.z : (v.z > max.z ? v.z - max.z : 0);
			return (dx * dx) + (dy * dy) + (dz * dz);
		}
		public double Distance(in Vector3d v) {
			return Math.Sqrt(DistanceSquared(v));
		}

		public double SignedDistance(in Vector3d v) {
			if (Contains(v) == false) {
				return Distance(v);
			}
			else {
				var dx = Math.Min(Math.Abs(v.x - min.x), Math.Abs(v.x - max.x));
				var dy = Math.Min(Math.Abs(v.y - min.y), Math.Abs(v.y - max.y));
				var dz = Math.Min(Math.Abs(v.z - min.z), Math.Abs(v.z - max.z));
				return -MathUtil.Min(dx, dy, dz);
			}
		}


		public double DistanceSquared(in AxisAlignedBox3d box2) {
			// compute lensqr( max(0, abs(center1-center2) - (extent1+extent2)) )
			var delta_x = Math.Abs(box2.min.x + box2.max.x - (min.x + max.x))
					- (max.x - min.x + (box2.max.x - box2.min.x));
			if (delta_x < 0) {
				delta_x = 0;
			}

			var delta_y = Math.Abs(box2.min.y + box2.max.y - (min.y + max.y))
					- (max.y - min.y + (box2.max.y - box2.min.y));
			if (delta_y < 0) {
				delta_y = 0;
			}

			var delta_z = Math.Abs(box2.min.z + box2.max.z - (min.z + max.z))
					- (max.z - min.z + (box2.max.z - box2.min.z));
			if (delta_z < 0) {
				delta_z = 0;
			}

			return 0.25 * ((delta_x * delta_x) + (delta_y * delta_y) + (delta_z * delta_z));
		}


		// [TODO] we have handled corner cases, but not edge cases!
		//   those are 2D, so it would be like (dx > width && dy > height)
		//public double Distance(Vector3d v)
		//{
		//    double dx = (double)Math.Abs(v.x - Center.x);
		//    double dy = (double)Math.Abs(v.y - Center.y);
		//    double dz = (double)Math.Abs(v.z - Center.z);
		//    double fWidth = Width * 0.5;
		//    double fHeight = Height * 0.5;
		//    double fDepth = Depth * 0.5;
		//    if (dx < fWidth && dy < fHeight && dz < Depth)
		//        return 0.0f;
		//    else if (dx > fWidth && dy > fHeight && dz > fDepth)
		//        return (double)Math.Sqrt((dx - fWidth) * (dx - fWidth) + (dy - fHeight) * (dy - fHeight) + (dz - fDepth) * (dz - fDepth));
		//    else if (dx > fWidth)
		//        return dx - fWidth;
		//    else if (dy > fHeight)
		//        return dy - fHeight;
		//    else if (dz > fDepth)
		//        return dz - fDepth;
		//    return 0.0f;
		//}


		//! relative translation
		public void Translate(in Vector3d vTranslate) {
			min.Add(vTranslate);
			max.Add(vTranslate);
		}

		public void MoveMin(in Vector3d vNewMin) {
			max.x = vNewMin.x + (max.x - min.x);
			max.y = vNewMin.y + (max.y - min.y);
			max.z = vNewMin.z + (max.z - min.z);
			min.Set(vNewMin);
		}
		public void MoveMin(in double fNewX, in double fNewY, in double fNewZ) {
			max.x = fNewX + (max.x - min.x);
			max.y = fNewY + (max.y - min.y);
			max.z = fNewZ + (max.z - min.z);
			min.Set(fNewX, fNewY, fNewZ);
		}



		public override string ToString() {
			return string.Format("x[{0:F8},{1:F8}] y[{2:F8},{3:F8}] z[{4:F8},{5:F8}]", min.x, max.x, min.y, max.y, min.z, max.z);
		}


		public static implicit operator AxisAlignedBox3d(in AxisAlignedBox3f v) => new(v.min, v.max);
		public static explicit operator AxisAlignedBox3f(in AxisAlignedBox3d v) => new((Vector3f)v.min, (Vector3f)v.max);
	}
}
