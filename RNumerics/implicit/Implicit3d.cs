using System;
using System.Collections.Generic;

namespace RNumerics
{
	/// <summary>
	/// Minimalist implicit function interface
	/// </summary>
	public interface IMplicitFunction3d
	{
		double Value(in Vector3d pt);
	}


	/// <summary>
	/// Bounded implicit function has a bounding box within which
	/// the "interesting" part of the function is contained 
	/// (eg the surface)
	/// </summary>
	public interface IBoundedImplicitFunction3d : IMplicitFunction3d
	{
		AxisAlignedBox3d Bounds();
	}


	/// <summary>
	/// Implicit sphere, where zero isocontour is at Radius
	/// </summary>
	public sealed class ImplicitSphere3d : IBoundedImplicitFunction3d
	{
		public Vector3d Origin;
		public double Radius;

		public double Value(in Vector3d pt) {
			return pt.Distance(Origin) - Radius;
		}

		public AxisAlignedBox3d Bounds() {
			return new AxisAlignedBox3d(Origin, Radius);
		}
	}


	/// <summary>
	/// Implicit half-space. "Inside" is opposite of Normal direction.
	/// </summary>
	public sealed class ImplicitHalfSpace3d : IBoundedImplicitFunction3d
	{
		public Vector3d Origin;
		public Vector3d Normal;

		public double Value(in Vector3d pt) {
			return (pt - Origin).Dot(Normal);
		}

		public AxisAlignedBox3d Bounds() {
			return new AxisAlignedBox3d(Origin, MathUtil.EPSILON);
		}
	}



	/// <summary>
	/// Implicit axis-aligned box
	/// </summary>
	public sealed class ImplicitAxisAlignedBox3d : IBoundedImplicitFunction3d
	{
		public AxisAlignedBox3d AABox;

		public double Value(in Vector3d pt) {
			return AABox.SignedDistance(pt);
		}

		public AxisAlignedBox3d Bounds() {
			return AABox;
		}
	}



	/// <summary>
	/// Implicit oriented box
	/// </summary>
	public sealed class ImplicitBox3d : IBoundedImplicitFunction3d
	{
		Box3d _box;
		AxisAlignedBox3d _local_aabb;
		AxisAlignedBox3d _bounds_aabb;
		public Box3d Box
		{
			get => _box;
			set {
				_box = value;
				_local_aabb = new AxisAlignedBox3d(
					-Box.Extent.x, -Box.Extent.y, -Box.Extent.z,
					Box.Extent.x, Box.Extent.y, Box.Extent.z);
				_bounds_aabb = _box.ToAABB();
			}
		}


		public double Value(in Vector3d pt) {
			var dx = (pt - Box.Center).Dot(Box.AxisX);
			var dy = (pt - Box.Center).Dot(Box.AxisY);
			var dz = (pt - Box.Center).Dot(Box.AxisZ);
			return _local_aabb.SignedDistance(new Vector3d(dx, dy, dz));
		}

		public AxisAlignedBox3d Bounds() {
			return _bounds_aabb;
		}
	}



	/// <summary>
	/// Implicit tube around line segment
	/// </summary>
	public sealed class ImplicitLine3d : IBoundedImplicitFunction3d
	{
		public Segment3d Segment;
		public double Radius;

		public double Value(in Vector3d pt) {
			var d = Math.Sqrt(Segment.DistanceSquared(pt));
			return d - Radius;
		}

		public AxisAlignedBox3d Bounds() {
			Vector3d o = Radius * Vector3d.One, p0 = Segment.P0, p1 = Segment.P1;
			var box = new AxisAlignedBox3d(p0 - o, p0 + o);
			box.Contain(p1 - o);
			box.Contain(p1 + o);
			return box;
		}
	}




	/// <summary>
	/// Offset the zero-isocontour of an implicit function.
	/// Assumes that negative is inside, if not, reverse offset.
	/// </summary>
	public sealed class ImplicitOffset3d : IBoundedImplicitFunction3d
	{
		public IBoundedImplicitFunction3d A;
		public double Offset;

		public double Value(in Vector3d pt) {
			return A.Value(pt) - Offset;
		}

		public AxisAlignedBox3d Bounds() {
			var box = A.Bounds();
			box.Expand(Offset);
			return box;
		}
	}



	/// <summary>
	/// remaps values so that values within given interval are negative,
	/// and values outside this interval are positive. So, for a distance
	/// field, this converts single isocontour into two nested isocontours
	/// with zeros at interval a and b, with 'inside' in interval
	/// </summary>
	public sealed class ImplicitShell3d : IBoundedImplicitFunction3d
	{
		public IBoundedImplicitFunction3d A;
		public Interval1d Inside;

		public double Value(in Vector3d pt) {
			var f = A.Value(pt);
			f = f < Inside.a ? Inside.a - f : f > Inside.b ? f - Inside.b : -Math.Min(Math.Abs(f - Inside.a), Math.Abs(f - Inside.b));
			return f;
		}

		public AxisAlignedBox3d Bounds() {
			var box = A.Bounds();
			box.Expand(Math.Max(0, Inside.b));
			return box;
		}
	}




	/// <summary>
	/// Boolean Union of two implicit functions, A OR B.
	/// Assumption is that both have surface at zero isocontour and 
	/// negative is inside.
	/// </summary>
	public sealed class ImplicitUnion3d : IBoundedImplicitFunction3d
	{
		public IBoundedImplicitFunction3d A;
		public IBoundedImplicitFunction3d B;

		public double Value(in Vector3d pt) {
			return Math.Min(A.Value( pt), B.Value( pt));
		}

		public AxisAlignedBox3d Bounds() {
			var box = A.Bounds();
			box.Contain(B.Bounds());
			return box;
		}
	}



	/// <summary>
	/// Boolean Intersection of two implicit functions, A AND B
	/// Assumption is that both have surface at zero isocontour and 
	/// negative is inside.
	/// </summary>
	public sealed class ImplicitIntersection3d : IBoundedImplicitFunction3d
	{
		public IBoundedImplicitFunction3d A;
		public IBoundedImplicitFunction3d B;

		public double Value(in Vector3d pt) {
			return Math.Max(A.Value(pt), B.Value(pt));
		}

		public AxisAlignedBox3d Bounds() {
			// [TODO] intersect boxes
			var box = A.Bounds();
			box.Contain(B.Bounds());
			return box;
		}
	}



	/// <summary>
	/// Boolean Difference/Subtraction of two implicit functions A-B = A AND (NOT B)
	/// Assumption is that both have surface at zero isocontour and 
	/// negative is inside.
	/// </summary>
	public sealed class ImplicitDifference3d : IBoundedImplicitFunction3d
	{
		public IBoundedImplicitFunction3d A;
		public IBoundedImplicitFunction3d B;

		public double Value(in Vector3d pt) {
			return Math.Max(A.Value( pt), -B.Value( pt));
		}

		public AxisAlignedBox3d Bounds() {
			// [TODO] can actually subtract B.Bounds() here...
			return A.Bounds();
		}
	}




	/// <summary>
	/// Boolean Union of N implicit functions, A OR B.
	/// Assumption is that both have surface at zero isocontour and 
	/// negative is inside.
	/// </summary>
	public sealed class ImplicitNaryUnion3d : IBoundedImplicitFunction3d
	{
		public List<IBoundedImplicitFunction3d> Children;

		public double Value(in Vector3d pt) {
			var f = Children[0].Value( pt);
			var N = Children.Count;
			for (var k = 1; k < N; ++k) {
				f = Math.Min(f, Children[k].Value( pt));
			}

			return f;
		}

		public AxisAlignedBox3d Bounds() {
			var box = Children[0].Bounds();
			var N = Children.Count;
			for (var k = 1; k < N; ++k) {
				box.Contain(Children[k].Bounds());
			}

			return box;
		}
	}




	/// <summary>
	/// Boolean Intersection of N implicit functions, A AND B.
	/// Assumption is that both have surface at zero isocontour and 
	/// negative is inside.
	/// </summary>
	public sealed class ImplicitNaryIntersection3d : IBoundedImplicitFunction3d
	{
		public List<IBoundedImplicitFunction3d> Children;

		public double Value(in Vector3d pt) {
			var f = Children[0].Value(in pt);
			var N = Children.Count;
			for (var k = 1; k < N; ++k) {
				f = Math.Max(f, Children[k].Value(in pt));
			}

			return f;
		}

		public AxisAlignedBox3d Bounds() {
			var box = Children[0].Bounds();
			var N = Children.Count;
			for (var k = 1; k < N; ++k) {
				box = box.Intersect(Children[k].Bounds());
			}
			return box;
		}
	}





	/// <summary>
	/// Boolean Difference of N implicit functions, A - Union(B1..BN)
	/// Assumption is that both have surface at zero isocontour and 
	/// negative is inside.
	/// </summary>
	public sealed class ImplicitNaryDifference3d : IBoundedImplicitFunction3d
	{
		public IBoundedImplicitFunction3d A;
		public List<IBoundedImplicitFunction3d> BSet;

		public double Value(in Vector3d pt) {
			var fA = A.Value(in pt);
			var N = BSet.Count;
			if (N == 0) {
				return fA;
			}

			var fB = BSet[0].Value(in pt);
			for (var k = 1; k < N; ++k) {
				fB = Math.Min(fB, BSet[k].Value(in pt));
			}

			return Math.Max(fA, -fB);
		}

		public AxisAlignedBox3d Bounds() {
			// [TODO] could actually subtract other bounds here...
			return A.Bounds();
		}
	}




	/// <summary>
	/// Continuous R-Function Boolean Union of two implicit functions, A OR B.
	/// Assumption is that both have surface at zero isocontour and 
	/// negative is inside.
	/// </summary>
	public sealed class ImplicitSmoothUnion3d : IBoundedImplicitFunction3d
	{
		public IBoundedImplicitFunction3d A;
		public IBoundedImplicitFunction3d B;

		const double MUL = 1.0 / 1.5;

		public double Value(in Vector3d pt) {
			var fA = A.Value(in pt);
			var fB = B.Value(in pt);
			return MUL * (fA + fB - Math.Sqrt((fA * fA) + (fB * fB) - (fA * fB)));
		}

		public AxisAlignedBox3d Bounds() {
			var box = A.Bounds();
			box.Contain(B.Bounds());
			return box;
		}
	}



	/// <summary>
	/// Continuous R-Function Boolean Intersection of two implicit functions, A-B = A AND (NOT B)
	/// Assumption is that both have surface at zero isocontour and 
	/// negative is inside.
	/// </summary>
	public sealed class ImplicitSmoothIntersection3d : IBoundedImplicitFunction3d
	{
		public IBoundedImplicitFunction3d A;
		public IBoundedImplicitFunction3d B;

		const double MUL = 1.0 / 1.5;

		public double Value(in Vector3d pt) {
			var fA = A.Value(in pt);
			var fB = B.Value(in pt);
			return MUL * (fA + fB + Math.Sqrt((fA * fA) + (fB * fB) - (fA * fB)));
		}

		public AxisAlignedBox3d Bounds() {
			var box = A.Bounds();
			box.Contain(B.Bounds());
			return box;
		}
	}




	/// <summary>
	/// Continuous R-Function Boolean Difference of two implicit functions, A AND B
	/// Assumption is that both have surface at zero isocontour and 
	/// negative is inside.
	/// </summary>
	public sealed class ImplicitSmoothDifference3d : IBoundedImplicitFunction3d
	{
		public IBoundedImplicitFunction3d A;
		public IBoundedImplicitFunction3d B;

		const double MUL = 1.0 / 1.5;

		public double Value(in Vector3d pt) {
			var fA = A.Value( pt);
			var fB = -B.Value( pt);
			return MUL * (fA + fB + Math.Sqrt((fA * fA) + (fB * fB) - (fA * fB)));
		}

		public AxisAlignedBox3d Bounds() {
			var box = A.Bounds();
			box.Contain(B.Bounds());
			return box;
		}
	}




	/// <summary>
	/// Blend of two implicit surfaces. Assumes surface is at zero iscontour.
	/// Uses Pasko blend from http://www.hyperfun.org/F-rep.pdf
	/// </summary>
	public sealed class ImplicitBlend3d : IBoundedImplicitFunction3d
	{
		public IBoundedImplicitFunction3d A;
		public IBoundedImplicitFunction3d B;


		/// <summary>Weight on implicit A</summary>
		public double WeightA
		{
			get => _weightA;
			set => _weightA = MathUtil.Clamp(value, 0.00001, 100000);
		}
		double _weightA = 0.01;

		/// <summary>Weight on implicit B</summary>
		public double WeightB
		{
			get => _weightB;
			set => _weightB = MathUtil.Clamp(value, 0.00001, 100000);
		}
		double _weightB = 0.01;

		/// <summary>Blending power</summary>
		public double Blend
		{
			get => _blend;
			set => _blend = MathUtil.Clamp(value, 0.0, 100000);
		}
		double _blend = 2.0;


		public double ExpandBounds = 0.25;


		public double Value(in Vector3d pt) {
			var fA = A.Value( pt);
			var fB = B.Value( pt);
			var sqr_sum = (fA * fA) + (fB * fB);
			if (sqr_sum > 1e12) {
				return Math.Min(fA, fB);
			}

			double wa = fA / _weightA, wb = fB / _weightB;
			var b = _blend / (1.0 + (wa * wa) + (wb * wb));
			//double a = 0.5;
			//return (1.0/(1.0+a)) * (fA + fB - Math.Sqrt(fA*fA + fB*fB - 2*a*fA*fB)) - b;
			return (0.666666 * (fA + fB - Math.Sqrt(sqr_sum - (fA * fB)))) - b;
		}

		public AxisAlignedBox3d Bounds() {
			var box = A.Bounds();
			box.Contain(B.Bounds());
			box.Expand(ExpandBounds * box.MaxDim);
			return box;
		}
	}








	/*
     *  Skeletal implicit ops
     */



	/// <summary>
	/// This class converts the interval [-falloff,falloff] to [0,1],
	/// Then applies Wyvill falloff function (1-t^2)^3.
	/// The result is a skeletal-primitive-like shape with 
	/// the distance=0 isocontour lying just before midway in
	/// the range (at the .ZeroIsocontour constant)
	/// </summary>
	public sealed class DistanceFieldToSkeletalField : IBoundedImplicitFunction3d
	{
		public IBoundedImplicitFunction3d DistanceField;
		public double FalloffDistance;
		public const double ZERO_ISOCONTOUR = 0.421875;

		public AxisAlignedBox3d Bounds() {
			var bounds = DistanceField.Bounds();
			bounds.Expand(FalloffDistance);
			return bounds;
		}

		public double Value(in Vector3d pt) {
			var d = DistanceField.Value( pt);
			if (d > FalloffDistance) {
				return 0;
			}
			else if (d < -FalloffDistance) {
				return 1.0;
			}

			var a = (d + FalloffDistance) / (2 * FalloffDistance);
			var t = 1 - (a * a);
			return t * t * t;
		}
	}







	/// <summary>
	/// sum-blend
	/// </summary>
	public sealed class SkeletalBlend3d : IBoundedImplicitFunction3d
	{
		public IBoundedImplicitFunction3d A;
		public IBoundedImplicitFunction3d B;

		public double Value(in Vector3d pt) {
			return A.Value( pt) + B.Value( pt);
		}

		public AxisAlignedBox3d Bounds() {
			var box = A.Bounds();
			box.Contain(B.Bounds());
			box.Expand(0.25 * box.MaxDim);
			return box;
		}
	}



	/// <summary>
	/// Ricci blend
	/// </summary>
	public sealed class SkeletalRicciBlend3d : IBoundedImplicitFunction3d
	{
		public IBoundedImplicitFunction3d A;
		public IBoundedImplicitFunction3d B;
		public double BlendPower = 2.0;

		public double Value(in Vector3d pt) {
			var a = A.Value( pt);
			var b = B.Value( pt);
			return BlendPower == 1.0
				? a + b
				: BlendPower == 2.0
					? Math.Sqrt((a * a) + (b * b))
					: Math.Pow(Math.Pow(a, BlendPower) + Math.Pow(b, BlendPower), 1.0 / BlendPower);
		}

		public AxisAlignedBox3d Bounds() {
			var box = A.Bounds();
			box.Contain(B.Bounds());
			box.Expand(0.25 * box.MaxDim);
			return box;
		}
	}




	/// <summary>
	/// Boolean Union of N implicit functions, A OR B.
	/// Assumption is that both have surface at zero isocontour and 
	/// negative is inside.
	/// </summary>
	public sealed class SkeletalRicciNaryBlend3d : IBoundedImplicitFunction3d
	{
		public List<IBoundedImplicitFunction3d> Children;
		public double BlendPower = 2.0;
		public double FieldShift = 0;

		public double Value(in Vector3d pt) {
			var N = Children.Count;
			double f = 0;
			if (BlendPower == 1.0) {
				for (var k = 0; k < N; ++k) {
					f += Children[k].Value( pt);
				}
			}
			else if (BlendPower == 2.0) {
				for (var k = 0; k < N; ++k) {
					var v = Children[k].Value( pt);
					f += v * v;
				}
				f = Math.Sqrt(f);
			}
			else {
				for (var k = 0; k < N; ++k) {
					var v = Children[k].Value( pt);
					f += Math.Pow(v, BlendPower);
				}
				f = Math.Pow(f, 1.0 / BlendPower);
			}
			return f + FieldShift;
		}

		public AxisAlignedBox3d Bounds() {
			var box = Children[0].Bounds();
			var N = Children.Count;
			for (var k = 1; k < N; ++k) {
				box.Contain(Children[k].Bounds());
			}

			box.Expand(0.25 * box.MaxDim);
			return box;
		}
	}





}
