﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// adapted from IntrLine3Box3
	public sealed class IntrLine3AxisAlignedBox3
	{
		Line3d _line;
		public Line3d Line
		{
			get => _line;
			set { _line = value; Result = IntersectionResult.NotComputed; }
		}

		AxisAlignedBox3d _box;
		public AxisAlignedBox3d Box
		{
			get => _box;
			set { _box = value; Result = IntersectionResult.NotComputed; }
		}

		public int Quantity = 0;
		public IntersectionResult Result = IntersectionResult.NotComputed;
		public IntersectionType Type = IntersectionType.Empty;

		public bool IsSimpleIntersection => Result == IntersectionResult.Intersects && Type == IntersectionType.Point;

		public double LineParam0, LineParam1;
		public Vector3d Point0 = Vector3d.Zero;
		public Vector3d Point1 = Vector3d.Zero;

		public IntrLine3AxisAlignedBox3(in Line3d l, in AxisAlignedBox3d b) {
			_line = l;
			_box = b;
		}

		public IntrLine3AxisAlignedBox3 Compute() {
			Find();
			return this;
		}


		public bool Find() {
			if (Result != IntersectionResult.NotComputed) {
				return Result == IntersectionResult.Intersects;
			}

			// [RMS] if either line direction is not a normalized vector, 
			//   results are garbage, so fail query
			if (_line.Direction.IsNormalized == false) {
				Type = IntersectionType.Empty;
				Result = IntersectionResult.InvalidQuery;
				return false;
			}

			LineParam0 = -double.MaxValue;
			LineParam1 = double.MaxValue;
			DoClipping(ref LineParam0, ref LineParam1, ref _line.Origin, ref _line.Direction, ref _box,
					  true, ref Quantity, ref Point0, ref Point1, ref Type);

			Result = (Type != IntersectionType.Empty) ?
				IntersectionResult.Intersects : IntersectionResult.NoIntersection;
			return Result == IntersectionResult.Intersects;
		}



		// [RMS TODO: lots of useless dot products below!! left over from obox conversion]
		public bool Test() {
			var AWdU = Vector3d.Zero;
			var AWxDdU = Vector3d.Zero;
			double RHS;

			var diff = _line.Origin - _box.Center;
			var WxD = _line.Direction.Cross(diff);

			var extent = _box.Extents;

			AWdU[1] = Math.Abs(_line.Direction.Dot(Vector3d.AxisY));
			AWdU[2] = Math.Abs(_line.Direction.Dot(Vector3d.AxisZ));
			AWxDdU[0] = Math.Abs(WxD.Dot(Vector3d.AxisX));
			RHS = (extent.y * AWdU[2]) + (extent.z * AWdU[1]);
			if (AWxDdU[0] > RHS) {
				return false;
			}

			AWdU[0] = Math.Abs(_line.Direction.Dot(Vector3d.AxisX));
			AWxDdU[1] = Math.Abs(WxD.Dot(Vector3d.AxisY));
			RHS = (extent.x * AWdU[2]) + (extent.z * AWdU[0]);
			if (AWxDdU[1] > RHS) {
				return false;
			}

			AWxDdU[2] = Math.Abs(WxD.Dot(Vector3d.AxisZ));
			RHS = (extent.x * AWdU[1]) + (extent.y * AWdU[0]);
			return AWxDdU[2] <= RHS;
		}




		static public bool DoClipping(ref double t0, ref double t1,
						 ref Vector3d origin, ref Vector3d direction,
						 ref AxisAlignedBox3d box, in bool solid, ref int quantity,
						 ref Vector3d point0, ref Vector3d point1,
						 ref IntersectionType intrType) {
			var BOrigin = origin - box.Center;
			var extent = box.Extents;

			double saveT0 = t0, saveT1 = t1;
			var notAllClipped =
				Clip(+direction.x, -BOrigin.x - extent.x, ref t0, ref t1) &&
				Clip(-direction.x, +BOrigin.x - extent.x, ref t0, ref t1) &&
				Clip(+direction.y, -BOrigin.y - extent.y, ref t0, ref t1) &&
				Clip(-direction.y, +BOrigin.y - extent.y, ref t0, ref t1) &&
				Clip(+direction.z, -BOrigin.z - extent.z, ref t0, ref t1) &&
				Clip(-direction.z, +BOrigin.z - extent.z, ref t0, ref t1);

			if (notAllClipped && (solid || t0 != saveT0 || t1 != saveT1)) {
				if (t1 > t0) {
					intrType = IntersectionType.Segment;
					quantity = 2;
					point0 = origin + (t0 * direction);
					point1 = origin + (t1 * direction);
				}
				else {
					intrType = IntersectionType.Point;
					quantity = 1;
					point0 = origin + (t0 * direction);
				}
			}
			else {
				quantity = 0;
				intrType = IntersectionType.Empty;
			}

			return intrType != IntersectionType.Empty;
		}




		static public bool Clip(in double denom, in double numer, ref double t0, ref double t1) {
			// Return value is 'true' if line segment intersects the current test
			// plane.  Otherwise 'false' is returned in which case the line segment
			// is entirely clipped.

			if (denom > (double)0) {
				if (numer - (denom * t1) > MathUtil.ZERO_TOLERANCE) {
					return false;
				}
				if (numer > denom * t0) {
					t0 = numer / denom;
				}
				return true;
			}
			else if (denom < (double)0) {
				if (numer - (denom * t0) > MathUtil.ZERO_TOLERANCE) {
					return false;
				}
				if (numer > denom * t1) {
					t1 = numer / denom;
				}
				return true;
			}
			else {
				return numer <= (double)0;
			}
		}


	}
}
