using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// ported from WildMagic5 
	public sealed class IntrRay3Box3
	{
		Ray3d _ray;
		public Ray3d Ray
		{
			get => _ray;
			set { _ray = value; Result = IntersectionResult.NotComputed; }
		}

		Box3d _box;
		public Box3d Box
		{
			get => _box;
			set { _box = value; Result = IntersectionResult.NotComputed; }
		}

		public int Quantity = 0;
		public IntersectionResult Result = IntersectionResult.NotComputed;
		public IntersectionType Type = IntersectionType.Empty;

		public bool IsSimpleIntersection => Result == IntersectionResult.Intersects && Type == IntersectionType.Point;

		public double RayParam0, RayParam1;
		public Vector3d Point0 = Vector3d.Zero;
		public Vector3d Point1 = Vector3d.Zero;

		public IntrRay3Box3(in Ray3d r, in Box3d b) {
			_ray = r;
			_box = b;
		}

		public IntrRay3Box3 Compute() {
			Find();
			return this;
		}


		public bool Find() {
			if (Result != IntersectionResult.NotComputed) {
				return Result == IntersectionResult.Intersects;
			}

			// [RMS] if either line direction is not a normalized vector, 
			//   results are garbage, so fail query
			if (_ray.Direction.IsNormalized == false) {
				Type = IntersectionType.Empty;
				Result = IntersectionResult.InvalidQuery;
				return false;
			}

			RayParam0 = 0.0;
			RayParam1 = double.MaxValue;
			IntrLine3Box3.DoClipping(ref RayParam0, ref RayParam1, _ray.Origin, _ray.Direction, _box,
					  true, ref Quantity, ref Point0, ref Point1, ref Type);

			Result = (Type != IntersectionType.Empty) ?
				IntersectionResult.Intersects : IntersectionResult.NoIntersection;
			return Result == IntersectionResult.Intersects;
		}




		public bool Test() {
			return Intersects(ref _ray, ref _box);
		}



		/// <summary>
		/// test if ray intersects box.
		/// expandExtents allows you to scale box for hit-testing purposes.
		/// </summary>
		public static bool Intersects(ref Ray3d ray, ref Box3d box, in double expandExtents = 0) {
			var WdU = Vector3d.Zero;
			var AWdU = Vector3d.Zero;
			var DdU = Vector3d.Zero;
			var ADdU = Vector3d.Zero;
			var AWxDdU = Vector3d.Zero;
			double RHS;

			var diff = ray.Origin - box.Center;
			var extent = box.Extent + expandExtents;

			WdU[0] = ray.Direction.Dot(box.AxisX);
			AWdU[0] = Math.Abs(WdU[0]);
			DdU[0] = diff.Dot(box.AxisX);
			ADdU[0] = Math.Abs(DdU[0]);
			if (ADdU[0] > extent.x && DdU[0] * WdU[0] >= (double)0) {
				return false;
			}

			WdU[1] = ray.Direction.Dot(box.AxisY);
			AWdU[1] = Math.Abs(WdU[1]);
			DdU[1] = diff.Dot(box.AxisY);
			ADdU[1] = Math.Abs(DdU[1]);
			if (ADdU[1] > extent.y && DdU[1] * WdU[1] >= (double)0) {
				return false;
			}

			WdU[2] = ray.Direction.Dot(box.AxisZ);
			AWdU[2] = Math.Abs(WdU[2]);
			DdU[2] = diff.Dot(box.AxisZ);
			ADdU[2] = Math.Abs(DdU[2]);
			if (ADdU[2] > extent.z && DdU[2] * WdU[2] >= (double)0) {
				return false;
			}

			var WxD = ray.Direction.Cross(diff);

			AWxDdU[0] = Math.Abs(WxD.Dot(box.AxisX));
			RHS = (extent.y * AWdU[2]) + (extent.z * AWdU[1]);
			if (AWxDdU[0] > RHS) {
				return false;
			}

			AWxDdU[1] = Math.Abs(WxD.Dot(box.AxisY));
			RHS = (extent.x * AWdU[2]) + (extent.z * AWdU[0]);
			if (AWxDdU[1] > RHS) {
				return false;
			}

			AWxDdU[2] = Math.Abs(WxD.Dot(box.AxisZ));
			RHS = (extent.x * AWdU[1]) + (extent.y * AWdU[0]);
			return AWxDdU[2] <= RHS;
		}





	}
}
