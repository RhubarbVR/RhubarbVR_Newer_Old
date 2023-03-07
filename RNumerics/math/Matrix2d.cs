using System;
using System.IO;

namespace RNumerics
{
	// some functions ported from WildMagic5 Matrix2
	public struct Matrix2d : ISerlize<Matrix2d>
	{
		public double m00;
		public double m01;
		public double m10;
		public double m11;

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(m00);
			binaryWriter.Write(m01);
			binaryWriter.Write(m10);
			binaryWriter.Write(m11);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			m00 = binaryReader.ReadDouble();
			m01 = binaryReader.ReadDouble();
			m10 = binaryReader.ReadDouble();
			m11 = binaryReader.ReadDouble();
		}

		[Exposed]
		public double M00
		{
			get => m00;
			set => m00 = value;
		}
		[Exposed]
		public double M01
		{
			get => m01;
			set => m01 = value;
		}
		[Exposed]
		public double M10
		{
			get => m10;
			set => m10 = value;
		}
		[Exposed]
		public double M11
		{
			get => m11;
			set => m11 = value;
		}

		[Exposed]
		public static readonly Matrix2d Identity = new(true);
		[Exposed]
		public static readonly Matrix2d Zero = new(false);
		[Exposed]
		public static readonly Matrix2d One = new(1, 1, 1, 1);
		public Matrix2d() {
		}

		public Matrix2d(in bool bIdentity) {
			if (bIdentity) {
				m00 = m11 = 1;
				m01 = m10 = 0;
			}
			else {
				m00 = m01 = m10 = m11 = 0;
			}
		}
		public Matrix2d(in double m00, in double m01, in double m10, in double m11) {
			this.m00 = m00;
			this.m01 = m01;
			this.m10 = m10;
			this.m11 = m11;
		}
		public Matrix2d(in double m00, in double m11) {
			this.m00 = m00;
			this.m11 = m11;
			m01 = m10 = 0;
		}

		// Create a rotation matrix (positive angle -> counterclockwise).
		public Matrix2d(in double angle, in bool bDegrees = false) {
			if (bDegrees) {
				SetToRotationDeg(angle);
			}
			else {
				SetToRotationRad(angle);
			}
		}

		// Create matrices based on vector input.  The bool is interpreted as
		//   true: vectors are columns of the matrix
		//   false: vectors are rows of the matrix
		public Matrix2d(in Vector2d u, in Vector2d v, in bool columns) {
			if (columns) {
				m00 = u.x;
				m01 = v.x;
				m10 = u.y;
				m11 = v.y;
			}
			else {
				m00 = u.x;
				m01 = u.y;
				m10 = v.x;
				m11 = v.y;
			}
		}

		// Create a tensor product U*V^T.
		public Matrix2d(in Vector2d u, in Vector2d v) {
			m00 = u.x * v.x;
			m01 = u.x * v.y;
			m10 = u.y * v.x;
			m11 = u.y * v.y;
		}



		
		public double this[in int r, in int c] => (r == 0) ? ((c == 0) ? m00 : m01) : ((c == 0) ? m10 : m11);


		public void SetToDiagonal(in double m00, in double m11) {
			this.m00 = m00;
			this.m11 = m11;
			m01 = m10 = 0;
		}

		public void SetToRotationRad(in double angleRad) {
			m11 = m00 = Math.Cos(angleRad);
			m10 = Math.Sin(angleRad);
			m01 = -m10;
		}
		public void SetToRotationDeg(in double angleDeg) {
			SetToRotationRad(MathUtil.DEG_2_RAD * angleDeg);
		}


		// u^T*M*v
		public double QForm(in Vector2d u, in Vector2d v) {
			return u.Dot(this * v);
		}


		public Matrix2d Transpose() {
			return new(m00, m10, m01, m11);
		}

		// Other operations.
		public Matrix2d Inverse(in double epsilon = 0) {
			var det = (m00 * m11) - (m10 * m01);
			if (Math.Abs(det) > epsilon) {
				var invDet = 1.0 / det;
				return new(m11 * invDet, -m01 * invDet,
									-m10 * invDet, m00 * invDet);
			}
			else {
				return Zero;
			}
		}
		public Matrix2d Adjoint() {
			return new(m11, -m01, -m10, m00);
		}
		
		public double Determinant => (m00 * m11) - (m01 * m10);


		public double ExtractAngle() {
			// assert:  'this' matrix represents a rotation
			return Math.Atan2(m10, m00);
		}


		public Vector2d Row(in int i) {
			return (i == 0) ? new Vector2d(m00, m01) : new Vector2d(m10, m11);
		}
		public Vector2d Column(in int i) {
			return (i == 0) ? new Vector2d(m00, m10) : new Vector2d(m01, m11);
		}


		public void Orthonormalize() {
			// Algorithm uses Gram-Schmidt orthogonalization.  If 'this' matrix is
			// M = [m0|m1], then orthonormal output matrix is Q = [q0|q1],
			//
			//   q0 = m0/|m0|
			//   q1 = (m1-(q0*m1)q0)/|m1-(q0*m1)q0|
			//
			// where |V| indicates length of vector V and A*B indicates dot
			// product of vectors A and B.

			// Compute q0.
			var invLength = 1.0 / Math.Sqrt((m00 * m00) + (m10 * m10));

			m00 *= invLength;
			m10 *= invLength;

			// Compute q1.
			var dot0 = (m00 * m01) + (m10 * m11);
			m01 -= dot0 * m00;
			m11 -= dot0 * m10;

			invLength = 1.0 / Math.Sqrt((m01 * m01) + (m11 * m11));

			m01 *= invLength;
			m11 *= invLength;
		}


		public void EigenDecomposition(ref Matrix2d rot, ref Matrix2d diag) {
			var sum = Math.Abs(m00) + Math.Abs(m11);
			if (Math.Abs(m01) + sum == sum) {
				// The matrix M is diagonal (within numerical round-off).
				rot.m00 = (double)1;
				rot.m01 = (double)0;
				rot.m10 = (double)0;
				rot.m11 = (double)1;
				diag.m00 = m00;
				diag.m01 = (double)0;
				diag.m10 = (double)0;
				diag.m11 = m11;
				return;
			}

			var trace = m00 + m11;
			var diff = m00 - m11;
			var discr = Math.Sqrt((diff * diff) + (4 * m01 * m01));
			var eigVal0 = 0.5 * (trace - discr);
			var eigVal1 = 0.5 * (trace + discr);
			diag.SetToDiagonal(eigVal0, eigVal1);

			double cs, sn;
			if (diff >= 0.0) {
				cs = m01;
				sn = eigVal0 - m00;
			}
			else {
				cs = eigVal0 - m11;
				sn = m01;
			}
			var invLength = 1.0 / Math.Sqrt((cs * cs) + (sn * sn));
			cs *= invLength;
			sn *= invLength;

			rot.m00 = cs;
			rot.m01 = -sn;
			rot.m10 = sn;
			rot.m11 = cs;
		}




		public static Matrix2d operator -(in Matrix2d v) => new(-v.m00, -v.m01, -v.m10, -v.m11);

		public static Matrix2d operator +(in Matrix2d a, in Matrix2d o) => new(a.m00 + o.m00, a.m01 + o.m01, a.m10 + o.m10, a.m11 + o.m11);
		public static Matrix2d operator +(in Matrix2d a, in double f) => new(a.m00 + f, a.m01 + f, a.m10 + f, a.m11 + f);

		public static Matrix2d operator -(in Matrix2d a, in Matrix2d o) => new(a.m00 - o.m00, a.m01 - o.m01, a.m10 - o.m10, a.m11 - o.m11);
		public static Matrix2d operator -(in Matrix2d a, in double f) => new(a.m00 - f, a.m01 - f, a.m10 - f, a.m11 - f);

		public static Matrix2d operator *(in Matrix2d a, in double f) => new(a.m00 * f, a.m01 * f, a.m10 * f, a.m11 * f);
		public static Matrix2d operator *(in double f, in Matrix2d a) => new(a.m00 * f, a.m01 * f, a.m10 * f, a.m11 * f);
		public static Matrix2d operator /(in Matrix2d a, in double f) => new(a.m00 / f, a.m01 / f, a.m10 / f, a.m11 / f);


		// row*vector multiply
		public static Vector2d operator *(in Matrix2d m, in Vector2d v) {
			return new Vector2d((m.m00 * v.x) + (m.m01 * v.y),
								 (m.m10 * v.x) + (m.m11 * v.y));
		}

		// vector*column multiply
		public static Vector2d operator *(in Vector2d v, in Matrix2d m) {
			return new Vector2d((v.x * m.m00) + (v.y * m.m10),
								 (v.x * m.m01) + (v.y * m.m11));
		}

	}
}
