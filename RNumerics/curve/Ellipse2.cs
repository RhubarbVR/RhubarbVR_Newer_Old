using System;

namespace RNumerics
{
	// ported from WildMagic5 Ellipse2
	public class Ellipse2d : IParametricCurve2d
	{
		// An ellipse has center K, axis directions U[0] and U[1] (both
		// unit-length vectors), and extents e[0] and e[1] (both positive
		// numbers).  A point X = K+y[0]*U[0]+y[1]*U[1] is on the ellipse whenever
		// (y[0]/e[0])^2+(y[1]/e[1])^2 = 1.  The test for a point inside the
		// ellipse uses "<=" instead of "=" in the previous expression.  An
		// algebraic representation for the ellipse is
		//   1 = (X-K)^T * (U[0]*U[0]^T/e[0]^2 + U[1]*U[1]^T/e[1]^2) * (X-K)
		//     = (X-K)^T * M * (X-K)
		// where the superscript T denotes transpose.  Observe that U[i]*U[i]^T
		// is a matrix, not a scalar dot product.  The matrix M is symmetric.
		// The ellipse is also represented by a quadratic equation
		//   0 = a0 + a1*x[0] + a2*x[1] + a3*x[0]^2 + a4*x[0]*x[1] + a5*x[1]^2
		//     = a0 + [a1 a2]*X + X^T*[a3   a4/2]*X
		//                            [a4/2 a5  ]
		//     = C + B^T*X + X^T*A*X
		// where X = (x[0],x[1]).  This equation can be factored to the form
		// (X-K)^T*M*(X-K) = 1, where K = -A^{-1}*B/2, M = A/(B^T*A^{-1}*B/4-C).
		// To be an ellipse, M must have all positive eigenvalues.


		public Vector2d Center;
		public Vector2d Axis0, Axis1;
		public Vector2d Extent;
		public bool IsReversed;     // use ccw orientation instead of cw


		public Ellipse2d(in Vector2d center, in Vector2d axis0, in Vector2d axis1, in Vector2d extent) {
			Center = center;
			Axis0 = axis0;
			Axis1 = axis1;
			Extent.x = extent.x;
			Extent.y = extent.y;
			IsReversed = false;
		}

		public Ellipse2d(in Vector2d center, in Vector2d axis0, in Vector2d axis1, in double extent0, in double extent1) {
			Center = center;
			Axis0 = axis0;
			Axis1 = axis1;
			Extent.x = extent0;
			Extent.y = extent1;
			IsReversed = false;
		}

		public Ellipse2d(in Vector2d center, in double rotationAngleDeg, in double extent0, in double extent1) {
			Center = center;
			var m = new Matrix2d(rotationAngleDeg * MathUtil.DEG_2_RAD);
			Axis0 = m * Vector2d.AxisX;
			Axis1 = m * Vector2d.AxisY;
			Extent = new Vector2d(extent0, extent1);
			IsReversed = false;
		}

		// Compute M = sum_{i=0}^1 U[i]*U[i]^T/e[i]^2.
		public Matrix2d GetM() {
			var ratio0 = Axis0 / Extent[0];
			var ratio1 = Axis1 / Extent[1];
			return new Matrix2d(ratio0, ratio0) + new Matrix2d(ratio1, ratio1);
		}

		// Compute M^{-1} = sum_{i=0}^1 U[i]*U[i]^T*e[i]^2.
		public Matrix2d GetMInverse() {
			var ratio0 = Axis0 * Extent[0];
			var ratio1 = Axis1 * Extent[1];
			return new Matrix2d(ratio0, ratio0) + new Matrix2d(ratio1, ratio1);
		}

		// construct the coefficients in the quadratic equation that represents
		// the ellipse.  'coeff' stores a0 through a5.  'A', 'B', and 'C' are as
		// described in the comments before the constructors.
		public double[] ToCoefficients() {
			var A = Matrix2d.Zero;
			var B = Vector2d.Zero;
			double C = 0;
			ToCoefficients(ref A, ref B, ref C);
			var coeff = Convert(A, B, C);

			// Arrange for one of the x0^2 or x1^2 coefficients to be 1.
			var maxValue = Math.Abs(coeff[3]);
			var maxIndex = 3;
			var absValue = Math.Abs(coeff[5]);
			if (absValue > maxValue) {
				maxValue = absValue;
				maxIndex = 5;
			}

			var invMaxValue = ((double)1) / maxValue;
			for (var i = 0; i < 6; ++i) {
				if (i != maxIndex) {
					coeff[i] *= invMaxValue;
				}
				else {
					coeff[i] = (double)1;
				}
			}

			return coeff;
		}

		public void ToCoefficients(ref Matrix2d A, ref Vector2d B, ref double C) {
			var ratio0 = Axis0 / Extent[0];
			var ratio1 = Axis1 / Extent[1];
			A = new Matrix2d(ratio0, ratio0) + new Matrix2d(ratio1, ratio1);
			B = ((double)-2) * (A * Center);
			C = A.QForm(Center, Center) - (double)1;
		}

		// Evaluate the quadratic function Q(X) = (X-K)^T * M * (X-K) - 1.
		public double Evaluate(in Vector2d point) {
			var diff = point - Center;
			var ratio0 = Axis0.Dot(diff) / Extent[0];
			var ratio1 = Axis1.Dot(diff) / Extent[1];
			var value = (ratio0 * ratio0) + (ratio1 * ratio1) - (double)1;
			return value;
		}


		// Test whether the input point is inside or on the ellipse.  The point
		// is contained when Q(X) <= 0, where Q(X) is the function in the comment
		// before the function Evaluate().
		public bool Contains(in Vector2d point) {
			return Evaluate(point) <= (double)0;
		}

		static double[] Convert(in Matrix2d A, in Vector2d B, in double C) {
			var coeff = new double[6];
			coeff[0] = C;
			coeff[1] = B.x;
			coeff[2] = B.y;
			coeff[3] = A.m00;
			coeff[4] = 2.0 * A.m01;
			coeff[5] = A.m11;
			return coeff;
		}





		public bool IsClosed => true;

		public void Reverse() {
			IsReversed = !IsReversed;
		}

		public IParametricCurve2d Clone() {
			return new Ellipse2d(Center, Axis0, Axis1, Extent) { IsReversed = IsReversed };
		}


		public bool IsTransformable => true;
		public void Transform(in ITransform2 xform) {
			Center = xform.TransformP(Center);
			Axis0 = xform.TransformN(Axis0);
			Axis1 = xform.TransformN(Axis1);
			Extent.x = xform.TransformScalar(Extent.x);
			Extent.y = xform.TransformScalar(Extent.y);
		}



		// angle in range [-2pi,2pi]
		public Vector2d SampleDeg(in double degrees) {
			var theta = degrees * MathUtil.DEG_2_RAD;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			return Center + (Extent.x * c * Axis0) + (Extent.y * s * Axis1);
		}

		// angle in range [-2pi,2pi]
		public Vector2d SampleRad(in double radians) {
			double c = Math.Cos(radians), s = Math.Sin(radians);
			return Center + (Extent.x * c * Axis0) + (Extent.y * s * Axis1);
		}


		public double ParamLength => 1.0f;

		// t in range[0,1] spans ellipse
		public Vector2d SampleT(in double t) {
			var theta = IsReversed ? -t * MathUtil.TWO_PI : t * MathUtil.TWO_PI;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			return Center + (Extent.x * c * Axis0) + (Extent.y * s * Axis1);
		}

		// t in range[0,1] spans ellipse
		public Vector2d TangentT(in double t) {
			var theta = IsReversed ? -t * MathUtil.TWO_PI : t * MathUtil.TWO_PI;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			var tangent = (-Extent.x * s * Axis0) + (Extent.y * c * Axis1);
			if (IsReversed) {
				tangent = -tangent;
			}

			tangent.Normalize();
			return tangent;
		}




		// [TODO] could use RombergIntegral like BaseCurve2, but need
		// first-derivative function

		public bool HasArcLength => false;

		public double ArcLength => throw new NotImplementedException("Ellipse2.ArcLength");

		public Vector2d SampleArcLength(in double a) {
			throw new NotImplementedException("Ellipse2.SampleArcLength");
		}



		public double Area => Math.PI * Extent.x * Extent.y;
		public double ApproxArcLen
		{
			get {
				// [RMS] from http://mathforum.org/dr.math/faq/formulas/faq.ellipse.html, 
				//   apparently due to Ramanujan
				var a = Math.Max(Extent.x, Extent.y);
				var b = Math.Min(Extent.x, Extent.y);
				var x = (a - b) / (a + b);
				var tx2 = 3 * x * x;
				var denom = 10.0 + Math.Sqrt(4 - tx2);
				return Math.PI * (a + b) * (1 + (tx2 / denom));
			}
		}


	}
}
