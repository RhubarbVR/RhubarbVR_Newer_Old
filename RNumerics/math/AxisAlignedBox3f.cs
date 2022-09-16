﻿using System;

using MessagePack;
namespace RNumerics
{
	[MessagePackObject]
	public struct AxisAlignedBox3f : IComparable<AxisAlignedBox3f>, IEquatable<AxisAlignedBox3f>
	{
		[Key(0)]
		public Vector3f Min = new(float.MaxValue, float.MaxValue, float.MaxValue);
		[Key(1)]
		public Vector3f Max = new(float.MinValue, float.MinValue, float.MinValue);

		[IgnoreMember]
		public static readonly AxisAlignedBox3f Empty = new();
		[IgnoreMember]
		public static readonly AxisAlignedBox3f Zero = new(0);
		[IgnoreMember]
		public static readonly AxisAlignedBox3f UnitPositive = new(1);
		[IgnoreMember]
		public static readonly AxisAlignedBox3f Infinite =
			new(float.MinValue, float.MinValue, float.MinValue, float.MaxValue, float.MaxValue, float.MaxValue);
		[IgnoreMember]
		public static readonly AxisAlignedBox3f CenterZero =
			new(Vector3f.Zero, 0);
		public AxisAlignedBox3f() {

		}
		public AxisAlignedBox3f(in float xmin, in float ymin, in float zmin, in float xmax, in float ymax, in float zmax) {
			Min = new Vector3f(xmin, ymin, zmin);
			Max = new Vector3f(xmax, ymax, zmax);
		}

		public AxisAlignedBox3f(in float fCubeSize) {
			Min = new Vector3f(0, 0, 0);
			Max = new Vector3f(fCubeSize, fCubeSize, fCubeSize);
		}

		public AxisAlignedBox3f(in float fWidth, in float fHeight, in float fDepth) {
			Min = new Vector3f(0, 0, 0);
			Max = new Vector3f(fWidth, fHeight, fDepth);
		}

		public AxisAlignedBox3f(in Vector3f vMin, in Vector3f vMax) {
			Min = new Vector3f(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y), Math.Min(vMin.z, vMax.z));
			Max = new Vector3f(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y), Math.Max(vMin.z, vMax.z));
		}

		public void Rotate(in Quaternionf rotation) {
			var center = Center;
			var e =  rotation * Extents;
			Min = center - e;
			Max = center + e;
		}

		public AxisAlignedBox3f(in Vector3f vCenter, in float fHalfWidth, in float fHalfHeight, in float fHalfDepth) {
			Min = new Vector3f(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight, vCenter.z - fHalfDepth);
			Max = new Vector3f(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight, vCenter.z + fHalfDepth);
		}


		public AxisAlignedBox3f(in Vector3f vCenter, in float fHalfSize) {
			Min = new Vector3f(vCenter.x - fHalfSize, vCenter.y - fHalfSize, vCenter.z - fHalfSize);
			Max = new Vector3f(vCenter.x + fHalfSize, vCenter.y + fHalfSize, vCenter.z + fHalfSize);
		}

		public AxisAlignedBox3f(in Vector3f vCenter) {
			Min = Max = vCenter;
		}

		[IgnoreMember]
		public float Width => Math.Max(Max.x - Min.x, 0);
		[IgnoreMember]
		public float Height => Math.Max(Max.y - Min.y, 0);
		[IgnoreMember]
		public float Depth => Math.Max(Max.z - Min.z, 0);

		[IgnoreMember]
		public float Volume => Width * Height * Depth;
		[IgnoreMember]
		public float DiagonalLength
		{
			get {
				return (float)Math.Sqrt(((Max.x - Min.x) * (Max.x - Min.x))
			  + ((Max.y - Min.y) * (Max.y - Min.y)) + ((Max.z - Min.z) * (Max.z - Min.z)));
			}
		}

		

		[IgnoreMember]
		public float MaxDim => Math.Max(Width, Math.Max(Height, Depth));

		[IgnoreMember]
		public Vector3f Diagonal => new(Max.x - Min.x, Max.y - Min.y, Max.z - Min.z);
		[IgnoreMember]
		public Vector3f Extents => new((Max.x - Min.x) * 0.5, (Max.y - Min.y) * 0.5, (Max.z - Min.z) * 0.5);
		[IgnoreMember]
		public Vector3f Center => new(0.5 * (Min.x + Max.x), 0.5 * (Min.y + Max.y), 0.5 * (Min.z + Max.z));


		public static bool operator ==(in AxisAlignedBox3f a, in AxisAlignedBox3f b) => a.Min == b.Min && a.Max == b.Max;
		public static bool operator !=(in AxisAlignedBox3f a, in AxisAlignedBox3f b) => a.Min != b.Min || a.Max != b.Max;
		public override bool Equals(object obj) {
			return this == (AxisAlignedBox3f)obj;
		}
		public bool Equals(AxisAlignedBox3f other) {
			return this == other;
		}
		public int CompareTo(AxisAlignedBox3f other) {
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


		// See Box3.Corner for details on which corner is which
		public Vector3f Corner(in int i) {
			var x = (((i & 1) != 0) ^ ((i & 2) != 0)) ? Max.x : Min.x;
			var y = (i / 2 % 2 == 0) ? Min.y : Max.y;
			var z = (i < 4) ? Min.z : Max.z;
			return new Vector3f(x, y, z);
		}


		/// <summary>
		/// Returns point on face/edge/corner. For each coord value neg==min, 0==center, pos==max
		/// </summary>
		public Vector3f Point(in int xi, in int yi, in int zi) {
			var x = (xi < 0) ? Min.x : ((xi == 0) ? (0.5f * (Min.x + Max.x)) : Max.x);
			var y = (yi < 0) ? Min.y : ((yi == 0) ? (0.5f * (Min.y + Max.y)) : Max.y);
			var z = (zi < 0) ? Min.z : ((zi == 0) ? (0.5f * (Min.z + Max.z)) : Max.z);
			return new Vector3f(x, y, z);
		}


		//! value is subtracted from min and added to max
		public void Expand(in float fRadius) {
			Min.x -= fRadius;
			Min.y -= fRadius;
			Min.z -= fRadius;
			Max.x += fRadius;
			Max.y += fRadius;
			Max.z += fRadius;
		}
		//! value is added to min and subtracted from max
		public void Contract(in float fRadius) {
			Min.x += fRadius;
			Min.y += fRadius;
			Min.z += fRadius;
			Max.x -= fRadius;
			Max.y -= fRadius;
			Max.z -= fRadius;
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
			Min = new Vector3f(c.x - e.x, c.y - e.y, c.z - e.z);
			Max = new Vector3f(c.x + e.x, c.y + e.y, c.z + e.z);
		}

		public void Contain(in Vector3f v) {
			Min.x = Math.Min(Min.x, v.x);
			Min.y = Math.Min(Min.y, v.y);
			Min.z = Math.Min(Min.z, v.z);
			Max.x = Math.Max(Max.x, v.x);
			Max.y = Math.Max(Max.y, v.y);
			Max.z = Math.Max(Max.z, v.z);
		}

		public void Contain(in AxisAlignedBox3f box) {
			Min.x = Math.Min(Min.x, box.Min.x);
			Min.y = Math.Min(Min.y, box.Min.y);
			Min.z = Math.Min(Min.z, box.Min.z);
			Max.x = Math.Max(Max.x, box.Max.x);
			Max.y = Math.Max(Max.y, box.Max.y);
			Max.z = Math.Max(Max.z, box.Max.z);
		}


		public void Contain(in Vector3d v) {
			Min.x = Math.Min(Min.x, (float)v.x);
			Min.y = Math.Min(Min.y, (float)v.y);
			Min.z = Math.Min(Min.z, (float)v.z);
			Max.x = Math.Max(Max.x, (float)v.x);
			Max.y = Math.Max(Max.y, (float)v.y);
			Max.z = Math.Max(Max.z, (float)v.z);
		}

		public void Contain(in AxisAlignedBox3d box) {
			Min.x = Math.Min(Min.x, (float)box.Min.x);
			Min.y = Math.Min(Min.y, (float)box.Min.y);
			Min.z = Math.Min(Min.z, (float)box.Min.z);
			Max.x = Math.Max(Max.x, (float)box.Max.x);
			Max.y = Math.Max(Max.y, (float)box.Max.y);
			Max.z = Math.Max(Max.z, (float)box.Max.z);
		}


		public AxisAlignedBox3f Intersect(in AxisAlignedBox3f box) {
			var intersect = new AxisAlignedBox3f(
				Math.Max(Min.x, box.Min.x), Math.Max(Min.y, box.Min.y), Math.Max(Min.z, box.Min.z),
				Math.Min(Max.x, box.Max.x), Math.Min(Max.y, box.Max.y), Math.Min(Max.z, box.Max.z));
			return intersect.Height <= 0 || intersect.Width <= 0 || intersect.Depth <= 0 ? AxisAlignedBox3f.Empty : intersect;
		}



		public bool Contains(in Vector3f v) {
			return (Min.x <= v.x) && (Min.y <= v.y) && (Min.z <= v.z)
				&& (Max.x >= v.x) && (Max.y >= v.y) && (Max.z >= v.z);
		}
		public bool Intersects(in AxisAlignedBox3f box) {
			return !((box.Max.x <= Min.x) || (box.Min.x >= Max.x)
				|| (box.Max.y <= Min.y) || (box.Min.y >= Max.y)
				|| (box.Max.z <= Min.z) || (box.Min.z >= Max.z));
		}


		public double DistanceSquared(in Vector3f v) {
			var dx = (v.x < Min.x) ? Min.x - v.x : (v.x > Max.x ? v.x - Max.x : 0);
			var dy = (v.y < Min.y) ? Min.y - v.y : (v.y > Max.y ? v.y - Max.y : 0);
			var dz = (v.z < Min.z) ? Min.z - v.z : (v.z > Max.z ? v.z - Max.z : 0);
			return (dx * dx) + (dy * dy) + (dz * dz);
		}
		public float Distance(in Vector3f v) {
			return (float)Math.Sqrt(DistanceSquared(v));
		}


		public Vector3f NearestPoint(in Vector3f v) {
			var x = (v.x < Min.x) ? Min.x : (v.x > Max.x ? Max.x : v.x);
			var y = (v.y < Min.y) ? Min.y : (v.y > Max.y ? Max.y : v.y);
			var z = (v.z < Min.z) ? Min.z : (v.z > Max.z ? Max.z : v.z);
			return new Vector3f(x, y, z);
		}



		//! relative translation
		public void Translate(in Vector3f vTranslate) {
			Min.Add(vTranslate);
			Max.Add(vTranslate);
		}

		public void MoveMin(in Vector3f vNewMin) {
			Max.x = vNewMin.x + (Max.x - Min.x);
			Max.y = vNewMin.y + (Max.y - Min.y);
			Max.z = vNewMin.z + (Max.z - Min.z);
			Min.Set(vNewMin);
		}
		public void MoveMin(in float fNewX, in float fNewY, in float fNewZ) {
			Max.x = fNewX + (Max.x - Min.x);
			Max.y = fNewY + (Max.y - Min.y);
			Max.z = fNewZ + (Max.z - Min.z);
			Min.Set(fNewX, fNewY, fNewZ);
		}



		public override string ToString() {
			return string.Format("x[{0:F8},{1:F8}] y[{2:F8},{3:F8}] z[{4:F8},{5:F8}]", Min.x, Max.x, Min.y, Max.y, Min.z, Max.z);
		}
	}
}
