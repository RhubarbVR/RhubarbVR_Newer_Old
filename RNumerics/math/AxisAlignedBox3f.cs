using System;

using MessagePack;
namespace RNumerics
{
	[MessagePackObject]
	public struct AxisAlignedBox3f : IComparable<AxisAlignedBox3f>, IEquatable<AxisAlignedBox3f>
	{
		[Key(0)]
		public Vector3f min = new(float.MaxValue, float.MaxValue, float.MaxValue);
		[Key(1)]
		public Vector3f max = new(float.MinValue, float.MinValue, float.MinValue);

		[Exposed, IgnoreMember]
		public Vector3f Min
		{
			get => min;
			set => min = value;
		}
		[Exposed, IgnoreMember]
		public Vector3f Max
		{
			get => max;
			set => max = value;
		}

		[Exposed, IgnoreMember]
		public static readonly AxisAlignedBox3f Empty = new();
		[Exposed, IgnoreMember]
		public static readonly AxisAlignedBox3f Zero = new(0);
		[Exposed, IgnoreMember]
		public static readonly AxisAlignedBox3f UnitPositive = new(1);
		[Exposed, IgnoreMember]
		public static readonly AxisAlignedBox3f Infinite =
			new(float.MinValue, float.MinValue, float.MinValue, float.MaxValue, float.MaxValue, float.MaxValue);
		[Exposed, IgnoreMember]
		public static readonly AxisAlignedBox3f CenterZero =
			new(Vector3f.Zero, 0);
		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox3f CenterOne =
	new(Vector3f.One, 0);

		public AxisAlignedBox3f() {

		}
		public AxisAlignedBox3f(in float xmin, in float ymin, in float zmin, in float xmax, in float ymax, in float zmax) {
			min = new Vector3f(xmin, ymin, zmin);
			max = new Vector3f(xmax, ymax, zmax);
		}

		public AxisAlignedBox3f(in float fCubeSize) {
			min = new Vector3f(0, 0, 0);
			max = new Vector3f(fCubeSize, fCubeSize, fCubeSize);
		}

		public AxisAlignedBox3f(in float fWidth, in float fHeight, in float fDepth) {
			min = new Vector3f(0, 0, 0);
			max = new Vector3f(fWidth, fHeight, fDepth);
		}

		public AxisAlignedBox3f(in Vector3f vMin, in Vector3f vMax) {
			min = new Vector3f(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y), Math.Min(vMin.z, vMax.z));
			max = new Vector3f(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y), Math.Max(vMin.z, vMax.z));
		}

		public void Rotate(in Quaternionf rotation) {
			var center = Center;
			var e =  rotation * Extents;
			min = center - e;
			max = center + e;
		}

		public AxisAlignedBox3f(in Vector3f vCenter, in float fHalfWidth, in float fHalfHeight, in float fHalfDepth) {
			min = new Vector3f(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight, vCenter.z - fHalfDepth);
			max = new Vector3f(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight, vCenter.z + fHalfDepth);
		}


		public AxisAlignedBox3f(in Vector3f vCenter, in float fHalfSize) {
			min = new Vector3f(vCenter.x - fHalfSize, vCenter.y - fHalfSize, vCenter.z - fHalfSize);
			max = new Vector3f(vCenter.x + fHalfSize, vCenter.y + fHalfSize, vCenter.z + fHalfSize);
		}

		public AxisAlignedBox3f(in Vector3f vCenter) {
			min = max = vCenter;
		}

		[IgnoreMember]
		public float Width => Math.Max(max.x - min.x, 0);
		[IgnoreMember]
		public float Height => Math.Max(max.y - min.y, 0);
		[IgnoreMember]
		public float Depth => Math.Max(max.z - min.z, 0);

		[IgnoreMember]
		public float Volume => Width * Height * Depth;
		[IgnoreMember]
		public float DiagonalLength
		{
			get {
				return (float)Math.Sqrt(((max.x - min.x) * (max.x - min.x))
			  + ((max.y - min.y) * (max.y - min.y)) + ((max.z - min.z) * (max.z - min.z)));
			}
		}

		

		[IgnoreMember]
		public float MaxDim => Math.Max(Width, Math.Max(Height, Depth));

		[IgnoreMember]
		public Vector3f Diagonal => new(max.x - min.x, max.y - min.y, max.z - min.z);
		[IgnoreMember]
		public Vector3f Extents => new((max.x - min.x) * 0.5, (max.y - min.y) * 0.5, (max.z - min.z) * 0.5);
		[IgnoreMember]
		public Vector3f Center => new(0.5 * (min.x + max.x), 0.5 * (min.y + max.y), 0.5 * (min.z + max.z));


		public static bool operator ==(in AxisAlignedBox3f a, in AxisAlignedBox3f b) => a.min == b.min && a.max == b.max;
		public static bool operator !=(in AxisAlignedBox3f a, in AxisAlignedBox3f b) => a.min != b.min || a.max != b.max;
		public override bool Equals(object obj) {
			return this == (AxisAlignedBox3f)obj;
		}
		public bool Equals(AxisAlignedBox3f other) {
			return this == other;
		}
		public int CompareTo(AxisAlignedBox3f other) {
			var c = min.CompareTo(other.min);
			return c == 0 ? max.CompareTo(other.max) : c;
		}
		public override int GetHashCode() {
			return HashCode.Combine(min, max);
		}


		// See Box3.Corner for details on which corner is which
		public Vector3f Corner(in int i) {
			var x = (((i & 1) != 0) ^ ((i & 2) != 0)) ? max.x : min.x;
			var y = (i / 2 % 2 == 0) ? min.y : max.y;
			var z = (i < 4) ? min.z : max.z;
			return new Vector3f(x, y, z);
		}


		/// <summary>
		/// Returns point on face/edge/corner. For each coord value neg==min, 0==center, pos==max
		/// </summary>
		public Vector3f Point(in int xi, in int yi, in int zi) {
			var x = (xi < 0) ? min.x : ((xi == 0) ? (0.5f * (min.x + max.x)) : max.x);
			var y = (yi < 0) ? min.y : ((yi == 0) ? (0.5f * (min.y + max.y)) : max.y);
			var z = (zi < 0) ? min.z : ((zi == 0) ? (0.5f * (min.z + max.z)) : max.z);
			return new Vector3f(x, y, z);
		}


		//! value is subtracted from min and added to max
		public void Expand(in float fRadius) {
			min.x -= fRadius;
			min.y -= fRadius;
			min.z -= fRadius;
			max.x += fRadius;
			max.y += fRadius;
			max.z += fRadius;
		}
		//! value is added to min and subtracted from max
		public void Contract(in float fRadius) {
			min.x += fRadius;
			min.y += fRadius;
			min.z += fRadius;
			max.x -= fRadius;
			max.y -= fRadius;
			max.z -= fRadius;
		}

		public void Scale(in Vector3f scale) {
			Scale(scale.x, scale.y, scale.z);
		}

		public void Scale(in float sx, in float sy, in float sz) {
			var c = Center;
			var e = Extents;
			e.x *= sx;
			e.y *= sy;
			e.z *= sz;
			min = new Vector3f(c.x - e.x, c.y - e.y, c.z - e.z);
			max = new Vector3f(c.x + e.x, c.y + e.y, c.z + e.z);
		}

		public void Contain(in Vector3f v) {
			min.x = Math.Min(min.x, v.x);
			min.y = Math.Min(min.y, v.y);
			min.z = Math.Min(min.z, v.z);
			max.x = Math.Max(max.x, v.x);
			max.y = Math.Max(max.y, v.y);
			max.z = Math.Max(max.z, v.z);
		}

		public void Contain(in AxisAlignedBox3f box) {
			min.x = Math.Min(min.x, box.min.x);
			min.y = Math.Min(min.y, box.min.y);
			min.z = Math.Min(min.z, box.min.z);
			max.x = Math.Max(max.x, box.max.x);
			max.y = Math.Max(max.y, box.max.y);
			max.z = Math.Max(max.z, box.max.z);
		}


		public void Contain(in Vector3d v) {
			min.x = Math.Min(min.x, (float)v.x);
			min.y = Math.Min(min.y, (float)v.y);
			min.z = Math.Min(min.z, (float)v.z);
			max.x = Math.Max(max.x, (float)v.x);
			max.y = Math.Max(max.y, (float)v.y);
			max.z = Math.Max(max.z, (float)v.z);
		}

		public void Contain(in AxisAlignedBox3d box) {
			min.x = Math.Min(min.x, (float)box.min.x);
			min.y = Math.Min(min.y, (float)box.min.y);
			min.z = Math.Min(min.z, (float)box.min.z);
			max.x = Math.Max(max.x, (float)box.max.x);
			max.y = Math.Max(max.y, (float)box.max.y);
			max.z = Math.Max(max.z, (float)box.max.z);
		}


		public AxisAlignedBox3f Intersect(in AxisAlignedBox3f box) {
			var intersect = new AxisAlignedBox3f(
				Math.Max(min.x, box.min.x), Math.Max(min.y, box.min.y), Math.Max(min.z, box.min.z),
				Math.Min(max.x, box.max.x), Math.Min(max.y, box.max.y), Math.Min(max.z, box.max.z));
			return intersect.Height <= 0 || intersect.Width <= 0 || intersect.Depth <= 0 ? AxisAlignedBox3f.Empty : intersect;
		}



		public bool Contains(in Vector3f v) {
			return (min.x <= v.x) && (min.y <= v.y) && (min.z <= v.z)
				&& (max.x >= v.x) && (max.y >= v.y) && (max.z >= v.z);
		}
		public bool Intersects(in AxisAlignedBox3f box) {
			return !((box.max.x <= min.x) || (box.min.x >= max.x)
				|| (box.max.y <= min.y) || (box.min.y >= max.y)
				|| (box.max.z <= min.z) || (box.min.z >= max.z));
		}


		public double DistanceSquared(in Vector3f v) {
			var dx = (v.x < min.x) ? min.x - v.x : (v.x > max.x ? v.x - max.x : 0);
			var dy = (v.y < min.y) ? min.y - v.y : (v.y > max.y ? v.y - max.y : 0);
			var dz = (v.z < min.z) ? min.z - v.z : (v.z > max.z ? v.z - max.z : 0);
			return (dx * dx) + (dy * dy) + (dz * dz);
		}
		public float Distance(in Vector3f v) {
			return (float)Math.Sqrt(DistanceSquared(v));
		}


		public Vector3f NearestPoint(in Vector3f v) {
			var x = (v.x < min.x) ? min.x : (v.x > max.x ? max.x : v.x);
			var y = (v.y < min.y) ? min.y : (v.y > max.y ? max.y : v.y);
			var z = (v.z < min.z) ? min.z : (v.z > max.z ? max.z : v.z);
			return new Vector3f(x, y, z);
		}



		//! relative translation
		public void Translate(in Vector3f vTranslate) {
			min.Add(vTranslate);
			max.Add(vTranslate);
		}

		public void MoveMin(in Vector3f vNewMin) {
			max.x = vNewMin.x + (max.x - min.x);
			max.y = vNewMin.y + (max.y - min.y);
			max.z = vNewMin.z + (max.z - min.z);
			min.Set(vNewMin);
		}
		public void MoveMin(in float fNewX, in float fNewY, in float fNewZ) {
			max.x = fNewX + (max.x - min.x);
			max.y = fNewY + (max.y - min.y);
			max.z = fNewZ + (max.z - min.z);
			min.Set(fNewX, fNewY, fNewZ);
		}



		public override string ToString() {
			return string.Format("x[{0:F8},{1:F8}] y[{2:F8},{3:F8}] z[{4:F8},{5:F8}]", min.x, max.x, min.y, max.y, min.z, max.z);
		}
	}
}
