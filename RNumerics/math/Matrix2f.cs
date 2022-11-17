using System;

using MessagePack;
namespace RNumerics
{
	// some functions ported from WildMagic5 Matrix2
	[MessagePackObject]
	public sealed class Matrix2f
	{
		[Key(0)]
		public float m00;
		[Key(1)]
		public float m01;
		[Key(2)]
		public float m10;
		[Key(3)]
		public float m11;

		[Exposed, IgnoreMember]
		public float M00
		{
			get => m00;
			set => m00 = value;
		}
		[Exposed, IgnoreMember]
		public float M01
		{
			get => m01;
			set => m01 = value;
		}
		[Exposed, IgnoreMember]
		public float M10
		{
			get => m10;
			set => m10 = value;
		}
		[Exposed, IgnoreMember]
		public float M11
		{
			get => m11;
			set => m11 = value;
		}

		public Matrix2f() {
		}
		[Exposed,IgnoreMember]
		public static readonly Matrix2f Identity = new (true);
		[Exposed,IgnoreMember]
		public static readonly Matrix2f Zero = new (false);
		[Exposed,IgnoreMember]
		public static readonly Matrix2f One = new (1, 1, 1, 1);


		public Matrix2f(in bool bIdentity)
		{
			if (bIdentity)
			{
				m00 = m11 = 1;
				m01 = m10 = 0;
			}
			else {
				m00 = m01 = m10 = m11 = 0;
			}
		}
		public Matrix2f(in float m00, in float m01, in float m10, in float m11)
		{
			this.m00 = m00;
			this.m01 = m01;
			this.m10 = m10;
			this.m11 = m11;
		}
		public Matrix2f(in float m00, in float m11)
		{
			this.m00 = m00;
			this.m11 = m11;
			m01 = m10 = 0;
		}

		// Create a rotation matrix (positive angle -> counterclockwise).
		public Matrix2f(in float radians)
		{
			SetToRotationRad(radians);
		}

		// Create matrices based on vector input.  The bool is interpreted as
		//   true: vectors are columns of the matrix
		//   false: vectors are rows of the matrix
		public Matrix2f(in Vector2f u, in Vector2f v, in bool columns)
		{
			if (columns)
			{
				m00 = u.x;
				m01 = v.x;
				m10 = u.y;
				m11 = v.y;
			}
			else
			{
				m00 = u.x;
				m01 = u.y;
				m10 = v.x;
				m11 = v.y;
			}
		}

		// Create a tensor product U*V^T.
		public Matrix2f(in Vector2f u, in Vector2f v)
		{
			m00 = u.x * v.x;
			m01 = u.x * v.y;
			m10 = u.y * v.x;
			m11 = u.y * v.y;
		}


		public void SetToDiagonal(in float m00, in float m11)
		{
			this.m00 = m00;
			this.m11 = m11;
			m01 = m10 = 0;
		}

		public void SetToRotationRad(in float angleRad)
		{
			m11 = m00 = (float)Math.Cos(angleRad);
			m10 = (float)Math.Sin(angleRad);
			m01 = -m10;
		}
		public void SetToRotationDeg(in float angleDeg)
		{
			SetToRotationRad(MathUtil.DEG_2_RADF * angleDeg);
		}


		// u^T*M*v
		public float QForm(in Vector2f u, in Vector2f v)
		{
			return u.Dot(this * v);
		}


		public Matrix2f Transpose()
		{
			return new (m00, m10, m01, m11);
		}

		// Other operations.
		public Matrix2f Inverse(in float epsilon = 0)
		{
			var det = (m00 * m11) - (m10 * m01);
			if (Math.Abs(det) > epsilon)
			{
				var invDet = 1.0f / det;
				return new (m11 * invDet, -m01 * invDet,
									-m10 * invDet, m00 * invDet);
			}
			else {
				return Zero;
			}
		}
		public Matrix2f Adjoint()
		{
			return new (m11, -m01, -m10, m00);
		}
		[IgnoreMember]
		public float Determinant => (m00 * m11) - (m01 * m10);


		public float ExtractAngle()
		{
			// assert:  'this' matrix represents a rotation
			return (float)Math.Atan2(m10, m00);
		}


		public void Orthonormalize()
		{
			// Algorithm uses Gram-Schmidt orthogonalization.  If 'this' matrix is
			// M = [m0|m1], then orthonormal output matrix is Q = [q0|q1],
			//
			//   q0 = m0/|m0|
			//   q1 = (m1-(q0*m1)q0)/|m1-(q0*m1)q0|
			//
			// where |V| indicates length of vector V and A*B indicates dot
			// product of vectors A and B.

			// Compute q0.
			var invLength = 1.0f / (float)Math.Sqrt((m00 * m00) + (m10 * m10));

			m00 *= invLength;
			m10 *= invLength;

			// Compute q1.
			var dot0 = (m00 * m01) + (m10 * m11);
			m01 -= dot0 * m00;
			m11 -= dot0 * m10;

			invLength = 1.0f / (float)Math.Sqrt((m01 * m01) + (m11 * m11));

			m01 *= invLength;
			m11 *= invLength;
		}


		public void EigenDecomposition(in Matrix2f rot, in Matrix2f diag)
		{
			var sum = Math.Abs(m00) + Math.Abs(m11);
			if (Math.Abs(m01) + sum == sum)
			{
				// The matrix M is diagonal (within numerical round-off).
				rot.m00 = (float)1;
				rot.m01 = (float)0;
				rot.m10 = (float)0;
				rot.m11 = (float)1;
				diag.m00 = m00;
				diag.m01 = (float)0;
				diag.m10 = (float)0;
				diag.m11 = m11;
				return;
			}

			var trace = m00 + m11;
			var diff = m00 - m11;
			var discr = (float)Math.Sqrt((diff * diff) + (4 * m01 * m01));
			var eigVal0 = 0.5f * (trace - discr);
			var eigVal1 = 0.5f * (trace + discr);
			diag.SetToDiagonal(eigVal0, eigVal1);

			float cs, sn;
			if (diff >= 0.0)
			{
				cs = m01;
				sn = eigVal0 - m00;
			}
			else
			{
				cs = eigVal0 - m11;
				sn = m01;
			}
			var invLength = 1.0f / (float)Math.Sqrt((cs * cs) + (sn * sn));
			cs *= invLength;
			sn *= invLength;

			rot.m00 = cs;
			rot.m01 = -sn;
			rot.m10 = sn;
			rot.m11 = cs;
		}




		public static Matrix2f operator -(in Matrix2f v) => new (-v.m00, -v.m01, -v.m10, -v.m11);

		public static Matrix2f operator +(in Matrix2f a, in Matrix2f o) => new (a.m00 + o.m00, a.m01 + o.m01, a.m10 + o.m10, a.m11 + o.m11);
		public static Matrix2f operator +(in Matrix2f a, in float f) => new (a.m00 + f, a.m01 + f, a.m10 + f, a.m11 + f);

		public static Matrix2f operator -(in Matrix2f a, in Matrix2f o) => new (a.m00 - o.m00, a.m01 - o.m01, a.m10 - o.m10, a.m11 - o.m11);
		public static Matrix2f operator -(in Matrix2f a, in float f) => new (a.m00 - f, a.m01 - f, a.m10 - f, a.m11 - f);

		public static Matrix2f operator *(in Matrix2f a, in float f) => new (a.m00 * f, a.m01 * f, a.m10 * f, a.m11 * f);
		public static Matrix2f operator *(in float f, in Matrix2f a) => new (a.m00 * f, a.m01 * f, a.m10 * f, a.m11 * f);
		public static Matrix2f operator /(in Matrix2f a, in float f) => new (a.m00 / f, a.m01 / f, a.m10 / f, a.m11 / f);


		// row*vector multiply
		public static Vector2f operator *(in Matrix2f m, in Vector2f v)
		{
			return new Vector2f((m.m00 * v.x) + (m.m01 * v.y),
								 (m.m10 * v.x) + (m.m11 * v.y));
		}

		// vector*column multiply
		public static Vector2f operator *(in Vector2f v, in Matrix2f m)
		{
			return new Vector2f((v.x * m.m00) + (v.y * m.m10),
								 (v.x * m.m01) + (v.y * m.m11));
		}

	}
}
