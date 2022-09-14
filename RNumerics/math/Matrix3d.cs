﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MessagePack;
namespace RNumerics
{
	[MessagePackObject]
	public struct Matrix3d
	{
		[Key(0)]
		public Vector3d Row0;
		[Key(1)]
		public Vector3d Row1;
		[Key(2)]
		public Vector3d Row2;
		public Matrix3d() {
			Row0 = Vector3d.Zero;
			Row1 = Vector3d.Zero;
			Row2 = Vector3d.Zero;
		}
		public Matrix3d(in bool bIdentity) {
			if (bIdentity) {
				Row0 = Vector3d.AxisX;
				Row1 = Vector3d.AxisY;
				Row2 = Vector3d.AxisZ;
			}
			else {
				Row0 = Row1 = Row2 = Vector3d.Zero;
			}
		}

		// assumes input is row-major...
		public Matrix3d(in float[,] mat) {
			Row0 = new Vector3d(mat[0, 0], mat[0, 1], mat[0, 2]);
			Row1 = new Vector3d(mat[1, 0], mat[1, 1], mat[1, 2]);
			Row2 = new Vector3d(mat[2, 0], mat[2, 1], mat[2, 2]);
		}
		public Matrix3d(in float[] mat) {
			Row0 = new Vector3d(mat[0], mat[1], mat[2]);
			Row1 = new Vector3d(mat[3], mat[4], mat[5]);
			Row2 = new Vector3d(mat[6], mat[7], mat[8]);
		}
		public Matrix3d(in double[,] mat) {
			Row0 = new Vector3d(mat[0, 0], mat[0, 1], mat[0, 2]);
			Row1 = new Vector3d(mat[1, 0], mat[1, 1], mat[1, 2]);
			Row2 = new Vector3d(mat[2, 0], mat[2, 1], mat[2, 2]);
		}
		public Matrix3d(in double[] mat) {
			Row0 = new Vector3d(mat[0], mat[1], mat[2]);
			Row1 = new Vector3d(mat[3], mat[4], mat[5]);
			Row2 = new Vector3d(mat[6], mat[7], mat[8]);
		}
		public Matrix3d(in Func<int, double> matBufferF) {
			Row0 = new Vector3d(matBufferF(0), matBufferF(1), matBufferF(2));
			Row1 = new Vector3d(matBufferF(3), matBufferF(4), matBufferF(5));
			Row2 = new Vector3d(matBufferF(6), matBufferF(7), matBufferF(8));
		}
		public Matrix3d(in Func<int, int, double> matF) {
			Row0 = new Vector3d(matF(0, 0), matF(0, 1), matF(0, 2));
			Row1 = new Vector3d(matF(1, 0), matF(1, 1), matF(1, 2));
			Row2 = new Vector3d(matF(2, 0), matF(1, 2), matF(2, 2));
		}
		public Matrix3d(in double m00, in double m11, in double m22) {
			Row0 = new Vector3d(m00, 0, 0);
			Row1 = new Vector3d(0, m11, 0);
			Row2 = new Vector3d(0, 0, m22);
		}
		public Matrix3d(in Vector3d v1, in Vector3d v2, in Vector3d v3, in bool bRows) {
			if (bRows) {
				Row0 = v1;
				Row1 = v2;
				Row2 = v3;
			}
			else {
				Row0 = new Vector3d(v1.x, v2.x, v3.x);
				Row1 = new Vector3d(v1.y, v2.y, v3.y);
				Row2 = new Vector3d(v1.z, v2.z, v3.z);
			}
		}
		
		public Matrix3d(in double m00, in double m01, in double m02, in double m10, in double m11, in double m12, in double m20, in double m21, in double m22) {
			Row0 = new Vector3d(m00, m01, m02);
			Row1 = new Vector3d(m10, m11, m12);
			Row2 = new Vector3d(m20, m21, m22);
		}


		/// <summary>
		/// Construct outer-product of u*transpose(v) of u and v
		/// result is that Mij = u_i * v_j
		/// </summary>
		public Matrix3d(in Vector3d u, in Vector3d v) {
			Row0 = new Vector3d(u.x * v.x, u.x * v.y, u.x * v.z);
			Row1 = new Vector3d(u.y * v.x, u.y * v.y, u.y * v.z);
			Row2 = new Vector3d(u.z * v.x, u.z * v.y, u.z * v.z);
		}

		[IgnoreMember]
		public static readonly Matrix3d Identity = new(true);
		[IgnoreMember]
		public static readonly Matrix3d Zero = new(false);



		[IgnoreMember]
		public double this[in int r, in int c]
		{
			get => (r == 0) ? Row0[c] : ((r == 1) ? Row1[c] : Row2[c]);
			set {
				if (r == 0) {
					Row0[c] = value;
				}
				else if (r == 1) {
					Row1[c] = value;
				}
				else {
					Row2[c] = value;
				}
			}
		}


		[IgnoreMember]
		public double this[in int i]
		{
			get => (i > 5) ? Row2[i % 3] : ((i > 2) ? Row1[i % 3] : Row0[i % 3]);
			set {
				if (i > 5) {
					Row2[i % 3] = value;
				}
				else if (i > 2) {
					Row1[i % 3] = value;
				}
				else {
					Row0[i % 3] = value;
				}
			}
		}



		public Vector3d Row(in int i) {
			return (i == 0) ? Row0 : (i == 1) ? Row1 : Row2;
		}
		public Vector3d Column(in int i) {
			return i == 0
				? new Vector3d(Row0.x, Row1.x, Row2.x)
				: i == 1 ? new Vector3d(Row0.y, Row1.y, Row2.y) : new Vector3d(Row0.z, Row1.z, Row2.z);
		}


		public double[] ToBuffer() {
			return new double[9] {
				Row0.x, Row0.y, Row0.z,
				Row1.x, Row1.y, Row1.z,
				Row2.x, Row2.y, Row2.z };
		}
		public void ToBuffer(in double[] buf) {
			buf[0] = Row0.x;
			buf[1] = Row0.y;
			buf[2] = Row0.z;
			buf[3] = Row1.x;
			buf[4] = Row1.y;
			buf[5] = Row1.z;
			buf[6] = Row2.x;
			buf[7] = Row2.y;
			buf[8] = Row2.z;
		}




		public static Matrix3d operator *(in Matrix3d mat, in double f) {
			return new Matrix3d(
				mat.Row0.x * f, mat.Row0.y * f, mat.Row0.z * f,
				mat.Row1.x * f, mat.Row1.y * f, mat.Row1.z * f,
				mat.Row2.x * f, mat.Row2.y * f, mat.Row2.z * f);
		}
		public static Matrix3d operator *(in double f, in Matrix3d mat) {
			return new Matrix3d(
				mat.Row0.x * f, mat.Row0.y * f, mat.Row0.z * f,
				mat.Row1.x * f, mat.Row1.y * f, mat.Row1.z * f,
				mat.Row2.x * f, mat.Row2.y * f, mat.Row2.z * f);
		}


		public static Vector3d operator *(in Matrix3d mat, in Vector3d v) {
			return new Vector3d(
				(mat.Row0.x * v.x) + (mat.Row0.y * v.y) + (mat.Row0.z * v.z),
				(mat.Row1.x * v.x) + (mat.Row1.y * v.y) + (mat.Row1.z * v.z),
				(mat.Row2.x * v.x) + (mat.Row2.y * v.y) + (mat.Row2.z * v.z));
		}

		public Vector3d Multiply(in Vector3d v) {
			return new Vector3d(
				(Row0.x * v.x) + (Row0.y * v.y) + (Row0.z * v.z),
				(Row1.x * v.x) + (Row1.y * v.y) + (Row1.z * v.z),
				(Row2.x * v.x) + (Row2.y * v.y) + (Row2.z * v.z));
		}

		public void Multiply(in Vector3d v,ref Vector3d vOut) {
			vOut.x = (Row0.x * v.x) + (Row0.y * v.y) + (Row0.z * v.z);
			vOut.y = (Row1.x * v.x) + (Row1.y * v.y) + (Row1.z * v.z);
			vOut.z = (Row2.x * v.x) + (Row2.y * v.y) + (Row2.z * v.z);
		}

		public static Matrix3d operator *(in Matrix3d mat1, in Matrix3d mat2) {
			var m00 = (mat1.Row0.x * mat2.Row0.x) + (mat1.Row0.y * mat2.Row1.x) + (mat1.Row0.z * mat2.Row2.x);
			var m01 = (mat1.Row0.x * mat2.Row0.y) + (mat1.Row0.y * mat2.Row1.y) + (mat1.Row0.z * mat2.Row2.y);
			var m02 = (mat1.Row0.x * mat2.Row0.z) + (mat1.Row0.y * mat2.Row1.z) + (mat1.Row0.z * mat2.Row2.z);

			var m10 = (mat1.Row1.x * mat2.Row0.x) + (mat1.Row1.y * mat2.Row1.x) + (mat1.Row1.z * mat2.Row2.x);
			var m11 = (mat1.Row1.x * mat2.Row0.y) + (mat1.Row1.y * mat2.Row1.y) + (mat1.Row1.z * mat2.Row2.y);
			var m12 = (mat1.Row1.x * mat2.Row0.z) + (mat1.Row1.y * mat2.Row1.z) + (mat1.Row1.z * mat2.Row2.z);

			var m20 = (mat1.Row2.x * mat2.Row0.x) + (mat1.Row2.y * mat2.Row1.x) + (mat1.Row2.z * mat2.Row2.x);
			var m21 = (mat1.Row2.x * mat2.Row0.y) + (mat1.Row2.y * mat2.Row1.y) + (mat1.Row2.z * mat2.Row2.y);
			var m22 = (mat1.Row2.x * mat2.Row0.z) + (mat1.Row2.y * mat2.Row1.z) + (mat1.Row2.z * mat2.Row2.z);

			return new Matrix3d(m00, m01, m02, m10, m11, m12, m20, m21, m22);
		}



		public static Matrix3d operator +(in Matrix3d mat1, in Matrix3d mat2) => new(mat1.Row0 + mat2.Row0, mat1.Row1 + mat2.Row1, mat1.Row2 + mat2.Row2, true);
		public static Matrix3d operator -(in Matrix3d mat1, in Matrix3d mat2) => new(mat1.Row0 - mat2.Row0, mat1.Row1 - mat2.Row1, mat1.Row2 - mat2.Row2, true);



		public double InnerProduct(in Matrix3d m2) {
			return Row0.Dot(m2.Row0) + Row1.Dot(m2.Row1) + Row2.Dot(m2.Row2);
		}



		public double Determinant
		{
			get {
				double a11 = Row0.x, a12 = Row0.y, a13 = Row0.z, a21 = Row1.x, a22 = Row1.y, a23 = Row1.z, a31 = Row2.x, a32 = Row2.y, a33 = Row2.z;
				var i00 = (a33 * a22) - (a32 * a23);
				var i01 = -((a33 * a12) - (a32 * a13));
				var i02 = (a23 * a12) - (a22 * a13);
				return (a11 * i00) + (a21 * i01) + (a31 * i02);
			}
		}


		public Matrix3d Inverse() {
			double a11 = Row0.x, a12 = Row0.y, a13 = Row0.z, a21 = Row1.x, a22 = Row1.y, a23 = Row1.z, a31 = Row2.x, a32 = Row2.y, a33 = Row2.z;
			var i00 = (a33 * a22) - (a32 * a23);
			var i01 = -((a33 * a12) - (a32 * a13));
			var i02 = (a23 * a12) - (a22 * a13);

			var i10 = -((a33 * a21) - (a31 * a23));
			var i11 = (a33 * a11) - (a31 * a13);
			var i12 = -((a23 * a11) - (a21 * a13));

			var i20 = (a32 * a21) - (a31 * a22);
			var i21 = -((a32 * a11) - (a31 * a12));
			var i22 = (a22 * a11) - (a21 * a12);

			var det = (a11 * i00) + (a21 * i01) + (a31 * i02);
			if (Math.Abs(det) < double.Epsilon) {
				throw new Exception("Matrix3d.Inverse: matrix is not invertible");
			}

			det = 1.0 / det;
			return new Matrix3d(i00 * det, i01 * det, i02 * det, i10 * det, i11 * det, i12 * det, i20 * det, i21 * det, i22 * det);
		}

		public Matrix3d Transpose() {
			return new Matrix3d(
				Row0.x, Row1.x, Row2.x,
				Row0.y, Row1.y, Row2.y,
				Row0.z, Row1.z, Row2.z);
		}

		public Quaterniond ToQuaternion() {
			return new Quaterniond(this);
		}


		public bool EpsilonEqual(in Matrix3d m2, in double epsilon) {
			return Row0.EpsilonEqual(m2.Row0, epsilon) &&
				Row1.EpsilonEqual(m2.Row1, epsilon) &&
				Row2.EpsilonEqual(m2.Row2, epsilon);
		}




		public static Matrix3d AxisAngleD(in Vector3d axis, in double angleDeg) {
			var angle = angleDeg * MathUtil.DEG_2_RAD;
			var cs = Math.Cos(angle);
			var sn = Math.Sin(angle);
			var oneMinusCos = 1.0 - cs;
			var x2 = axis[0] * axis[0];
			var y2 = axis[1] * axis[1];
			var z2 = axis[2] * axis[2];
			var xym = axis[0] * axis[1] * oneMinusCos;
			var xzm = axis[0] * axis[2] * oneMinusCos;
			var yzm = axis[1] * axis[2] * oneMinusCos;
			var xSin = axis[0] * sn;
			var ySin = axis[1] * sn;
			var zSin = axis[2] * sn;
			return new Matrix3d(
				(x2 * oneMinusCos) + cs, xym - zSin, xzm + ySin,
				xym + zSin, (y2 * oneMinusCos) + cs, yzm - xSin,
				xzm - ySin, yzm + xSin, (z2 * oneMinusCos) + cs);
		}




		public override string ToString() {
			return string.Format("[{0}] [{1}] [{2}]", Row0, Row1, Row2);
		}
		public string ToString(in string fmt) {
			return string.Format("[{0}] [{1}] [{2}]", Row0.ToString(fmt), Row1.ToString(fmt), Row2.ToString(fmt));
		}
	}
}
