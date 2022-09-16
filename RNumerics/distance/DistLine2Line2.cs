using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// ported from WildMagic 5 
	// https://www.geometrictools.com/Downloads/Downloads.html

	public sealed class DistLine2Line2
	{
		Line2d _line1;
		public Line2d Line
		{
			get => _line1;
			set { _line1 = value; DistanceSquared = -1.0; }
		}

		Line2d _line2;
		public Line2d Line2
		{
			get => _line2;
			set { _line2 = value; DistanceSquared = -1.0; }
		}

		public double DistanceSquared = -1.0;

		public Vector2d Line1Closest;
		public Vector2d Line2Closest;
		public double Line1Parameter;
		public double Line2Parameter;


		public DistLine2Line2(in Line2d Line1, in Line2d Line2) {
			_line2 = Line2;
			_line1 = Line1;
		}

		static public double MinDistance(in Line2d line1, in Line2d line2) {
			return new DistLine2Line2(line1, line2).Get();
		}


		public DistLine2Line2 Compute() {
			GetSquared();
			return this;
		}

		public double Get() {
			return Math.Sqrt(GetSquared());
		}


		public double GetSquared() {
			if (DistanceSquared >= 0) {
				return DistanceSquared;
			}

			var diff = _line1.Origin - _line2.Origin;
			var a01 = -_line1.Direction.Dot(_line2.Direction);
			var b0 = diff.Dot(_line1.Direction);
			var c = diff.LengthSquared;
			var det = Math.Abs(1.0 - (a01 * a01));
			double b1, s0, s1, sqrDist;

			if (det >= MathUtil.ZERO_TOLERANCE) {
				// Lines are not parallel.
				b1 = -diff.Dot(_line2.Direction);
				var invDet = ((double)1) / det;
				s0 = ((a01 * b1) - b0) * invDet;
				s1 = ((a01 * b0) - b1) * invDet;
				sqrDist = (double)0;
			}
			else {
				// Lines are parallel, select any closest pair of points.
				s0 = -b0;
				s1 = (double)0;
				sqrDist = (b0 * s0) + c;

				// Account for numerical round-off errors.
				if (sqrDist < (double)0) {
					sqrDist = (double)0;
				}
			}

			Line1Parameter = s0;
			Line1Closest = _line1.Origin + (s0 * _line1.Direction);
			Line2Parameter = s1;
			Line2Closest = _line2.Origin + (s1 * _line2.Direction);

			DistanceSquared = sqrDist;
			return sqrDist;
		}
	}

}
