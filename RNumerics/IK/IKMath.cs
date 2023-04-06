// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System;

namespace RNumerics.IK
{
	public static class IKMath
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sqrt(in float a) {
			return a <= FLOAT_EPSILON ? 0.0f : System.MathF.Sqrt(a);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SqrtClamp01(in float a) {
			if (a <= FLOAT_EPSILON) { // Counts as 0
				return 0.0f;
			}
			else if (a >= 1.0f - FLOAT_EPSILON) {
				return 1.0f;
			}
			return System.MathF.Sqrt(a);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Cos(in float a) {
			return System.MathF.Cos(a);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Acos(in float cos) {
			return cos >= 1.0f - FLOAT_EPSILON
				? 0.0f
				: cos <= -1.0f + FLOAT_EPSILON ? 180.0f * MathUtil.DEG_2_RADF : System.MathF.Acos(cos);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Asin(in float sin) {
			return sin >= 1.0f - FLOAT_EPSILON
				? 90.0f * MathUtil.DEG_2_RADF
				: sin <= -1.0f + FLOAT_EPSILON ? -90.0f * MathUtil.DEG_2_RADF : System.MathF.Asin(sin);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float VecLength(in Vector3f v) {
			var sq = (v.x * v.x) + (v.y * v.y) + (v.z * v.z);
			return sq > FLOAT_EPSILON ? System.MathF.Sqrt(sq) : 0.0f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float VecLengthAndLengthSq(out float lengthSq, in Vector3f v) {
			lengthSq = (v.x * v.x) + (v.y * v.y) + (v.z * v.z);
			return lengthSq > FLOAT_EPSILON ? System.MathF.Sqrt(lengthSq) : 0.0f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float VecLength2(in Vector3f lhs, in Vector3f rhs) {
			var rx = lhs.x - rhs.x;
			var ry = lhs.y - rhs.y;
			var rz = lhs.z - rhs.z;
			var sq = (rx * rx) + (ry * ry) + (rz * rz);
			return sq > FLOAT_EPSILON ? System.MathF.Sqrt(sq) : 0.0f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float VecLengthAndLengthSq2(out float lengthSq, in Vector3f lhs, in Vector3f rhs) {
			var rx = lhs.x - rhs.x;
			var ry = lhs.y - rhs.y;
			var rz = lhs.z - rhs.z;
			lengthSq = (rx * rx) + (ry * ry) + (rz * rz);
			return lengthSq > FLOAT_EPSILON ? System.MathF.Sqrt(lengthSq) : 0.0f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool VecNormalize(ref Vector3f v0) {
			var sq0 = (v0.x * v0.x) + (v0.y * v0.y) + (v0.z * v0.z);
			if (sq0 > FLOAT_EPSILON) {
				var len0 = (float)System.Math.Sqrt((double)sq0);
				if (len0 > IK_EPSILON) {
					len0 = 1.0f / len0;
					v0.x *= len0;
					v0.y *= len0;
					v0.z *= len0;
					return true;
				}
			}

			v0.x = v0.y = v0.z = 0.0f;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool VecNormalizeXZ(ref Vector3f v0) {
			var sq0 = (v0.x * v0.x) + (v0.z * v0.z);
			if (sq0 > FLOAT_EPSILON) {
				var len0 = System.MathF.Sqrt(sq0);
				if (len0 > IK_EPSILON) {
					len0 = 1.0f / len0;
					v0.x *= len0;
					v0.z *= len0;
					return true;
				}
			}

			v0.x = v0.z = 0.0f;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool VecNormalizeYZ(ref Vector3f v0) {
			var sq0 = (v0.y * v0.y) + (v0.z * v0.z);
			if (sq0 > FLOAT_EPSILON) {
				var len0 = System.MathF.Sqrt(sq0);
				if (len0 > IK_EPSILON) {
					len0 = 1.0f / len0;
					v0.y *= len0;
					v0.z *= len0;
					return true;
				}
			}

			v0.y = v0.z = 0.0f;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool VecNormalize2(ref Vector3f v0, ref Vector3f v1) {
			var r0 = VecNormalize(ref v0);
			var r1 = VecNormalize(ref v1);
			return r0 && r1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool VecNormalize3(ref Vector3f v0, ref Vector3f v1, ref Vector3f v2) {
			var r0 = VecNormalize(ref v0);
			var r1 = VecNormalize(ref v1);
			var r2 = VecNormalize(ref v2);
			return r0 && r1 && r2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool VecNormalize4(ref Vector3f v0, ref Vector3f v1, ref Vector3f v2, ref Vector3f v3) {
			var r0 = VecNormalize(ref v0);
			var r1 = VecNormalize(ref v1);
			var r2 = VecNormalize(ref v2);
			var r3 = VecNormalize(ref v3);
			return r0 && r1 && r2 && r3;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatMult(out IKMatrix3x3 ret, in IKMatrix3x3 lhs, in IKMatrix3x3 rhs) {
			ret = new IKMatrix3x3(
				(lhs.column0.x * rhs.column0.x) + (lhs.column1.x * rhs.column0.y) + (lhs.column2.x * rhs.column0.z),
				(lhs.column0.x * rhs.column1.x) + (lhs.column1.x * rhs.column1.y) + (lhs.column2.x * rhs.column1.z),
				(lhs.column0.x * rhs.column2.x) + (lhs.column1.x * rhs.column2.y) + (lhs.column2.x * rhs.column2.z),

				(lhs.column0.y * rhs.column0.x) + (lhs.column1.y * rhs.column0.y) + (lhs.column2.y * rhs.column0.z),
				(lhs.column0.y * rhs.column1.x) + (lhs.column1.y * rhs.column1.y) + (lhs.column2.y * rhs.column1.z),
				(lhs.column0.y * rhs.column2.x) + (lhs.column1.y * rhs.column2.y) + (lhs.column2.y * rhs.column2.z),

				(lhs.column0.z * rhs.column0.x) + (lhs.column1.z * rhs.column0.y) + (lhs.column2.z * rhs.column0.z),
				(lhs.column0.z * rhs.column1.x) + (lhs.column1.z * rhs.column1.y) + (lhs.column2.z * rhs.column1.z),
				(lhs.column0.z * rhs.column2.x) + (lhs.column1.z * rhs.column2.y) + (lhs.column2.z * rhs.column2.z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatMultRet0(ref  IKMatrix3x3 lhs, in IKMatrix3x3 rhs) {
			lhs = new IKMatrix3x3(
				(lhs.column0.x * rhs.column0.x) + (lhs.column1.x * rhs.column0.y) + (lhs.column2.x * rhs.column0.z),
				(lhs.column0.x * rhs.column1.x) + (lhs.column1.x * rhs.column1.y) + (lhs.column2.x * rhs.column1.z),
				(lhs.column0.x * rhs.column2.x) + (lhs.column1.x * rhs.column2.y) + (lhs.column2.x * rhs.column2.z),

				(lhs.column0.y * rhs.column0.x) + (lhs.column1.y * rhs.column0.y) + (lhs.column2.y * rhs.column0.z),
				(lhs.column0.y * rhs.column1.x) + (lhs.column1.y * rhs.column1.y) + (lhs.column2.y * rhs.column1.z),
				(lhs.column0.y * rhs.column2.x) + (lhs.column1.y * rhs.column2.y) + (lhs.column2.y * rhs.column2.z),

				(lhs.column0.z * rhs.column0.x) + (lhs.column1.z * rhs.column0.y) + (lhs.column2.z * rhs.column0.z),
				(lhs.column0.z * rhs.column1.x) + (lhs.column1.z * rhs.column1.y) + (lhs.column2.z * rhs.column1.z),
				(lhs.column0.z * rhs.column2.x) + (lhs.column1.z * rhs.column2.y) + (lhs.column2.z * rhs.column2.z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatMultCol0(out Vector3f ret, in IKMatrix3x3 lhs, in IKMatrix3x3 rhs) {
			ret = new Vector3f(
				(lhs.column0.x * rhs.column0.x) + (lhs.column1.x * rhs.column0.y) + (lhs.column2.x * rhs.column0.z),
				(lhs.column0.y * rhs.column0.x) + (lhs.column1.y * rhs.column0.y) + (lhs.column2.y * rhs.column0.z),
				(lhs.column0.z * rhs.column0.x) + (lhs.column1.z * rhs.column0.y) + (lhs.column2.z * rhs.column0.z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatMultCol1(out Vector3f ret, in IKMatrix3x3 lhs, in IKMatrix3x3 rhs) {
			ret = new Vector3f(
				(lhs.column0.x * rhs.column1.x) + (lhs.column1.x * rhs.column1.y) + (lhs.column2.x * rhs.column1.z),
				(lhs.column0.y * rhs.column1.x) + (lhs.column1.y * rhs.column1.y) + (lhs.column2.y * rhs.column1.z),
				(lhs.column0.z * rhs.column1.x) + (lhs.column1.z * rhs.column1.y) + (lhs.column2.z * rhs.column1.z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatMultVec(out Vector3f ret, in IKMatrix3x3 m, in Vector3f v) {
			ret = new Vector3f(
				(m.column0.x * v.x) + (m.column1.x * v.y) + (m.column2.x * v.z),
				(m.column0.y * v.x) + (m.column1.y * v.y) + (m.column2.y * v.z),
				(m.column0.z * v.x) + (m.column1.z * v.y) + (m.column2.z * v.z));
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatGetRot(out Quaternionf q, in IKMatrix3x3 m) {
			q = new Quaternionf();
			var t = m.column0.x + m.column1.y + m.column2.z;
			if (t > 0.0f) {
				var s = (float)System.Math.Sqrt(t + 1.0f);
				q.w = s * 0.5f;
				s = 0.5f / s;
				q.x = (m.column1.z - m.column2.y) * s;
				q.y = (m.column2.x - m.column0.z) * s;
				q.z = (m.column0.y - m.column1.x) * s;
			}
			else {
				if (m.column0.x > m.column1.y && m.column0.x > m.column2.z) {
					var s = m.column0.x - m.column1.y - m.column2.z + 1.0f;
					if (s <= FLOAT_EPSILON) {
						q = Quaternionf.Identity;
						return;
					}
					s = (float)System.Math.Sqrt(s);
					q.x = s * 0.5f;
					s = 0.5f / s;
					q.w = (m.column1.z - m.column2.y) * s;
					q.y = (m.column0.y + m.column1.x) * s;
					q.z = (m.column0.z + m.column2.x) * s;
				}
				else if (m.column1.y > m.column2.z) {
					var s = m.column1.y - m.column0.x - m.column2.z + 1.0f;
					if (s <= FLOAT_EPSILON) {
						q = Quaternionf.Identity;
						return;
					}
					s = (float)System.Math.Sqrt(s);
					q.y = s * 0.5f;
					s = 0.5f / s;
					q.w = (m.column2.x - m.column0.z) * s;
					q.z = (m.column1.z + m.column2.y) * s;
					q.x = (m.column1.x + m.column0.y) * s;
				}
				else {
					var s = m.column2.z - m.column0.x - m.column1.y + 1.0f;
					if (s <= FLOAT_EPSILON) {
						q = Quaternionf.Identity;
						return;
					}
					s = (float)System.Math.Sqrt(s);
					q.z = s * 0.5f;
					s = 0.5f / s;
					q.w = (m.column0.y - m.column1.x) * s;
					q.x = (m.column2.x + m.column0.z) * s;
					q.y = (m.column2.y + m.column1.z) * s;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatSetRot(out IKMatrix3x3 m, ref Quaternionf q) {
			var d = (q.x * q.x) + (q.y * q.y) + (q.z * q.z) + (q.w * q.w);
			var s = (d > FLOAT_EPSILON) ? (2.0f / d) : 0.0f;
			float xs = q.x * s, ys = q.y * s, zs = q.z * s;
			float wx = q.w * xs, wy = q.w * ys, wz = q.w * zs;
			float xx = q.x * xs, xy = q.x * ys, xz = q.x * zs;
			float yy = q.y * ys, yz = q.y * zs, zz = q.z * zs;
			m.column0.x = 1.0f - (yy + zz);
			m.column1.x = xy - wz;
			m.column2.x = xz + wy;
			m.column0.y = xy + wz;
			m.column1.y = 1.0f - (xx + zz);
			m.column2.y = yz - wx;
			m.column0.z = xz - wy;
			m.column1.z = yz + wx;
			m.column2.z = 1.0f - (xx + yy);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatSetAxisAngle(out IKMatrix3x3 m, ref Vector3f axis, float cos) {
			if (cos is >= (-FLOAT_EPSILON) and <= FLOAT_EPSILON) {
				m = IKMatrix3x3.identity;
				return;
			}

			m = new IKMatrix3x3();

			var sin = 1.0f - (cos * cos);
			sin = (sin <= FLOAT_EPSILON) ? 0.0f : ((sin >= 1.0f - FLOAT_EPSILON) ? 1.0f : (float)System.Math.Sqrt((float)sin));

			var axis_x_sin = axis.x * sin;
			var axis_y_sin = axis.y * sin;
			var axis_z_sin = axis.z * sin;

			m.column0.x = cos;
			m.column0.y = axis_z_sin;
			m.column0.z = -axis_y_sin;

			m.column1.x = -axis_z_sin;
			m.column1.y = cos;
			m.column1.z = axis_x_sin;

			m.column2.x = axis_y_sin;
			m.column2.y = -axis_x_sin;
			m.column2.z = cos;

			var cosI = 1.0f - cos;
			var axis_x_cosI = axis.x * cosI;
			var axis_y_cosI = axis.y * cosI;
			var axis_z_cosI = axis.z * cosI;

			m.column0.x += axis.x * axis_x_cosI;
			m.column0.y += axis.y * axis_x_cosI;
			m.column0.z += axis.z * axis_x_cosI;

			m.column1.x += axis.x * axis_y_cosI;
			m.column1.y += axis.y * axis_y_cosI;
			m.column1.z += axis.z * axis_y_cosI;

			m.column2.x += axis.x * axis_z_cosI;
			m.column2.y += axis.y * axis_z_cosI;
			m.column2.z += axis.z * axis_z_cosI;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatFastLerp(out IKMatrix3x3 ret, ref IKMatrix3x3 lhs, ref IKMatrix3x3 rhs, float rate) {
			if (rate <= IK_EPSILON) {
				ret = lhs;
				return;
			}
			else if (rate >= 1.0f - IK_EPSILON) {
				ret = rhs;
				return;
			}
			else {
				var x = lhs.column0;
				var y = lhs.column1;
				x += (rhs.column0 - x) * rate;
				y += (rhs.column1 - y) * rate;

				var z = Vector3f.Cross(x, y);
				x = Vector3f.Cross(y, z);

				ret = VecNormalize3(ref x, ref y, ref z) ? IKMatrix3x3.FromColumn(ref x, ref y, ref z) : lhs;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatFastLerpToIdentity(ref IKMatrix3x3 m, float rate) {
			if (rate <= IK_EPSILON) {
				// Nothing
			}
			else if (rate >= 1.0f - IK_EPSILON) {
				m = IKMatrix3x3.identity;
			}
			else {
				var x = m.column0;
				var y = m.column1;
				x += (new Vector3f(1.0f, 0.0f, 0.0f) - x) * rate;
				y += (new Vector3f(0.0f, 1.0f, 0.0f) - y) * rate;

				var z = Vector3f.Cross(x, y);
				x = Vector3f.Cross(y, z);

				if (VecNormalize3(ref x, ref y, ref z)) {
					m = IKMatrix3x3.FromColumn(ref x, ref y, ref z);
				}
			}
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatMultVecInv(out Vector3f ret, in IKMatrix3x3 mat, in Vector3f vec) {
			var tmpMat = mat.Transpose;
			MatMultVec(out ret, tmpMat, vec);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatMultVecAdd(out Vector3f ret, in IKMatrix3x3 mat, in Vector3f vec, in Vector3f addVec) {
			MatMultVec(out ret, mat, vec);
			ret += addVec;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatMultVecPreSubAdd(out Vector3f ret, in IKMatrix3x3 mat, in Vector3f vec, in Vector3f subVec, in Vector3f addVec) {
			var tmpVec = vec - subVec;
			MatMultVec(out ret, mat, tmpVec);
			ret += addVec;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatMultInv0(out IKMatrix3x3 ret, in IKMatrix3x3 lhs, in IKMatrix3x3 rhs) {
			ret = new IKMatrix3x3(
				(lhs.column0.x * rhs.column0.x) + (lhs.column0.y * rhs.column0.y) + (lhs.column0.z * rhs.column0.z),
				(lhs.column0.x * rhs.column1.x) + (lhs.column0.y * rhs.column1.y) + (lhs.column0.z * rhs.column1.z),
				(lhs.column0.x * rhs.column2.x) + (lhs.column0.y * rhs.column2.y) + (lhs.column0.z * rhs.column2.z),

				(lhs.column1.x * rhs.column0.x) + (lhs.column1.y * rhs.column0.y) + (lhs.column1.z * rhs.column0.z),
				(lhs.column1.x * rhs.column1.x) + (lhs.column1.y * rhs.column1.y) + (lhs.column1.z * rhs.column1.z),
				(lhs.column1.x * rhs.column2.x) + (lhs.column1.y * rhs.column2.y) + (lhs.column1.z * rhs.column2.z),

				(lhs.column2.x * rhs.column0.x) + (lhs.column2.y * rhs.column0.y) + (lhs.column2.z * rhs.column0.z),
				(lhs.column2.x * rhs.column1.x) + (lhs.column2.y * rhs.column1.y) + (lhs.column2.z * rhs.column1.z),
				(lhs.column2.x * rhs.column2.x) + (lhs.column2.y * rhs.column2.y) + (lhs.column2.z * rhs.column2.z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatMultInv1(out IKMatrix3x3 ret, ref IKMatrix3x3 lhs, ref IKMatrix3x3 rhs) {
			ret = new IKMatrix3x3(
				(lhs.column0.x * rhs.column0.x) + (lhs.column1.x * rhs.column1.x) + (lhs.column2.x * rhs.column2.x),
				(lhs.column0.x * rhs.column0.y) + (lhs.column1.x * rhs.column1.y) + (lhs.column2.x * rhs.column2.y),
				(lhs.column0.x * rhs.column0.z) + (lhs.column1.x * rhs.column1.z) + (lhs.column2.x * rhs.column2.z),

				(lhs.column0.y * rhs.column0.x) + (lhs.column1.y * rhs.column1.x) + (lhs.column2.y * rhs.column2.x),
				(lhs.column0.y * rhs.column0.y) + (lhs.column1.y * rhs.column1.y) + (lhs.column2.y * rhs.column2.y),
				(lhs.column0.y * rhs.column0.z) + (lhs.column1.y * rhs.column1.z) + (lhs.column2.y * rhs.column2.z),

				(lhs.column0.z * rhs.column0.x) + (lhs.column1.z * rhs.column1.x) + (lhs.column2.z * rhs.column2.x),
				(lhs.column0.z * rhs.column0.y) + (lhs.column1.z * rhs.column1.y) + (lhs.column2.z * rhs.column2.y),
				(lhs.column0.z * rhs.column0.z) + (lhs.column1.z * rhs.column1.z) + (lhs.column2.z * rhs.column2.z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatMultGetRot(out Quaternionf ret, in IKMatrix3x3 lhs, in IKMatrix3x3 rhs) {
			MatMult(out var tmpMat, lhs, rhs);
			MatGetRot(out ret, tmpMat);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatSetRotMult(out IKMatrix3x3 ret, in Quaternionf lhs, in Quaternionf rhs) {
			var q = lhs * rhs;
			MatSetRot(out ret, ref q);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MatSetRotMultInv1(out IKMatrix3x3 ret, in Quaternionf lhs, in Quaternionf rhs) {
			var q = lhs * Inverse(rhs);
			MatSetRot(out ret, ref q);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void QuatMult(out Quaternionf ret, in Quaternionf q0, in Quaternionf q1) {
			ret = q0 * q1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void QuatMultInv0(out Quaternionf ret, in Quaternionf q0, in Quaternionf q1) {
			ret = Inverse(q0) * q1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void QuatMultNorm(out Quaternionf ret, in Quaternionf q0, in Quaternionf q1) {
			ret = Normalize(q0 * q1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void QuatMult3(out Quaternionf ret, in Quaternionf q0, in Quaternionf q1, in Quaternionf q2) {
			ret = q0 * q1 * q2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void QuatMultNorm3(out Quaternionf ret, in Quaternionf q0, in Quaternionf q1, in Quaternionf q2) {
			ret = Normalize(q0 * q1 * q2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void QuatMultNorm3Inv1(out Quaternionf ret, in Quaternionf q0, in Quaternionf q1, in Quaternionf q2) {
			ret = Normalize(q0 * Inverse(q1) * q2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeBasisFromXZLockX(out IKMatrix3x3 basis, ref Vector3f dirX, in Vector3f dirZ) {
			var baseY = Vector3f.Cross(dirZ, dirX);
			var baseZ = Vector3f.Cross(dirX, baseY);
			if (VecNormalize2(ref baseY, ref baseZ)) {
				basis = IKMatrix3x3.FromColumn(ref dirX, ref baseY, ref baseZ);
				return true;
			}
			else {
				basis = IKMatrix3x3.identity;
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeBasisFromXYLockX(out IKMatrix3x3 basis, ref Vector3f dirX, in Vector3f dirY) {
			var baseZ = Vector3f.Cross(dirX, dirY);
			var baseY = Vector3f.Cross(baseZ, dirX);
			if (VecNormalize2(ref baseY, ref baseZ)) {
				basis = IKMatrix3x3.FromColumn(ref dirX, ref baseY, ref baseZ);
				return true;
			}
			else {
				basis = IKMatrix3x3.identity;
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeBasisFromXYLockY(out IKMatrix3x3 basis, in Vector3f dirX, ref Vector3f dirY) {
			var baseZ = Vector3f.Cross(dirX, dirY);
			var baseX = Vector3f.Cross(dirY, baseZ);
			if (VecNormalize2(ref baseX, ref baseZ)) {
				basis = IKMatrix3x3.FromColumn(ref baseX, ref dirY, ref baseZ);
				return true;
			}
			else {
				basis = IKMatrix3x3.identity;
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeBasisFromXZLockZ(out IKMatrix3x3 basis, in Vector3f dirX, ref Vector3f dirZ) {
			var baseY = Vector3f.Cross(dirZ, dirX);
			var baseX = Vector3f.Cross(baseY, dirZ);
			if (VecNormalize2(ref baseX, ref baseY)) {
				basis = IKMatrix3x3.FromColumn(ref baseX, ref baseY, ref dirZ);
				return true;
			}
			else {
				basis = IKMatrix3x3.identity;
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeBasisFromYZLockY(out IKMatrix3x3 basis, ref Vector3f dirY, in Vector3f dirZ) {
			var baseX = Vector3f.Cross(dirY, dirZ);
			var baseZ = Vector3f.Cross(baseX, dirY);
			if (VecNormalize2(ref baseX, ref baseZ)) {
				basis = IKMatrix3x3.FromColumn(ref baseX, ref dirY, ref baseZ);
				return true;
			}
			else {
				basis = IKMatrix3x3.identity;
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeBasisFromYZLockZ(out IKMatrix3x3 basis, in Vector3f dirY, ref Vector3f dirZ) {
			var baseX = Vector3f.Cross(dirY, dirZ);
			var baseY = Vector3f.Cross(dirZ, baseX);
			if (VecNormalize2(ref baseX, ref baseY)) {
				basis = IKMatrix3x3.FromColumn(ref baseX, ref baseY, ref dirZ);
				return true;
			}
			else {
				basis = IKMatrix3x3.identity;
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeBasisLockX(out IKMatrix3x3 basis, ref Vector3f dirX, in Vector3f dirY, in Vector3f dirZ) {
			var solveY = ComputeBasisFromXYLockX(out var basisY, ref dirX, dirY);
			var solveZ = ComputeBasisFromXZLockX(out var basisZ, ref dirX, dirZ);
			if (solveY && solveZ) {
				var nearY = MathF.Abs(Vector3f.Dot(dirX, dirY));
				var nearZ = MathF.Abs(Vector3f.Dot(dirX, dirZ));
				if (nearZ <= IK_EPSILON) {
					basis = basisZ;
					return true;
				}
				else if (nearY <= IK_EPSILON) {
					basis = basisY;
					return true;
				}
				else {
					MatFastLerp(out basis, ref basisY, ref basisZ, nearY / (nearY + nearZ));
					return true;
				}
			}
			else if (solveY) {
				basis = basisY;
				return true;
			}
			else if (solveZ) {
				basis = basisZ;
				return true;
			}

			basis = IKMatrix3x3.identity;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeBasisLockY(out IKMatrix3x3 basis, in Vector3f dirX, ref Vector3f dirY, in Vector3f dirZ) {
			var solveX = ComputeBasisFromXYLockY(out var basisX, dirX, ref dirY);
			var solveZ = ComputeBasisFromYZLockY(out var basisZ, ref dirY, dirZ);
			if (solveX && solveZ) {
				var nearX = MathF.Abs(Vector3f.Dot(dirY, dirX));
				var nearZ = MathF.Abs(Vector3f.Dot(dirY, dirZ));
				if (nearZ <= IK_EPSILON) {
					basis = basisZ;
					return true;
				}
				else if (nearX <= IK_EPSILON) {
					basis = basisX;
					return true;
				}
				else {
					MatFastLerp(out basis, ref basisX, ref basisZ, nearX / (nearX + nearZ));
					return true;
				}
			}
			else if (solveX) {
				basis = basisX;
				return true;
			}
			else if (solveZ) {
				basis = basisZ;
				return true;
			}

			basis = IKMatrix3x3.identity;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeBasisLockZ(out IKMatrix3x3 basis, in Vector3f dirX, in  Vector3f dirY, ref Vector3f dirZ) {
			var solveX = ComputeBasisFromXZLockZ(out var basisX, dirX, ref dirZ);
			var solveY = ComputeBasisFromYZLockZ(out var basisY, dirY, ref dirZ);
			if (solveX && solveY) {
				var nearX = MathF.Abs(Vector3f.Dot(dirZ, dirX));
				var nearY = MathF.Abs(Vector3f.Dot(dirZ, dirY));
				if (nearY <= IK_EPSILON) {
					basis = basisY;
					return true;
				}
				else if (nearX <= IK_EPSILON) {
					basis = basisX;
					return true;
				}
				else {
					MatFastLerp(out basis, ref basisX, ref basisY, nearX / (nearX + nearY));
					return true;
				}
			}
			else if (solveX) {
				basis = basisX;
				return true;
			}
			else if (solveY) {
				basis = basisY;
				return true;
			}

			basis = IKMatrix3x3.identity;
			return false;
		}

		public const float FLOAT_EPSILON = 1.401298e-45f;
		public const float IK_EPSILON = 1e-7f;
		public const float IK_MOVE_EPSILON = 1e-05f;
		public const float IK_WRITEBACK_EPSILON = 0.01f;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsFuzzy(float lhs, float rhs, float epsilon = IK_EPSILON) {
			var t = lhs - rhs;
			return t >= -epsilon && t <= epsilon;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsFuzzy(ref Vector3f lhs, ref Vector3f rhs, float epsilon = IK_EPSILON) {
			var x = lhs.x - rhs.x;
			if (x >= -epsilon && x <= epsilon) {
				x = lhs.y - rhs.y;
				if (x >= -epsilon && x <= epsilon) {
					x = lhs.z - rhs.z;
					if (x >= -epsilon && x <= epsilon) {
						return true;
					}
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3f Rotate(ref Vector3f dirX, ref Vector3f dirY, float cosR, float sinR) {
			return (dirX * cosR) + (dirY * sinR);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3f Rotate(ref Vector3f dirX, ref Vector3f dirY, float r) {
			var cosR = MathF.Cos(r);
			var sinR = MathF.Sin(r);
			return (dirX * cosR) + (dirY * sinR);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternionf Inverse(Quaternionf q) {
			return new Quaternionf(-q.x, -q.y, -q.z, q.w);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternionf Normalize(Quaternionf q) {
			var lenSq = (q.x * q.x) + (q.y * q.y) + (q.z * q.z) + (q.w * q.w);
			if (lenSq > IK_EPSILON) {
				if (lenSq is >= (1.0f - IK_EPSILON) and <= (float)(1.0 + IK_EPSILON)) {
					return q;
				}
				else {
					var s = 1.0f / MathF.Sqrt(lenSq);
					return new Quaternionf(q.x * s, q.y * s, q.z * s, q.w * s);
				}
			}

			return q; // Failsafe.
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeBasisFromXZLockX(out IKMatrix3x3 basis, Vector3f dirX, Vector3f dirZ) {
			return ComputeBasisFromXZLockX(out basis,ref dirX, dirZ);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeBasisFromXYLockX(out IKMatrix3x3 basis, Vector3f dirX, Vector3f dirY) {
			return ComputeBasisFromXYLockX(out basis, ref dirX, dirY);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeBasisFromXYLockY(out IKMatrix3x3 basis, Vector3f dirX, Vector3f dirY) {
			return ComputeBasisFromXYLockY(out basis, dirX, ref dirY);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeBasisFromXZLockZ(out IKMatrix3x3 basis, Vector3f dirX, Vector3f dirZ) {
			return ComputeBasisFromXZLockZ(out basis, dirX, ref dirZ);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeBasisFrom(out IKMatrix3x3 basis, in IKMatrix3x3 rootBasis, in Vector3f dir, in DirectionAs directionAs) {
			switch (directionAs) {
				case DirectionAs.XPlus:
					return ComputeBasisFromXYLockX(out basis, dir, rootBasis.column1);
				case DirectionAs.XMinus:
					return ComputeBasisFromXYLockX(out basis, -dir, rootBasis.column1);
				case DirectionAs.YPlus:
					return ComputeBasisFromXYLockY(out basis, rootBasis.column0, dir);
				case DirectionAs.YMinus:
					return ComputeBasisFromXYLockY(out basis, rootBasis.column0, -dir);
			}

			basis = IKMatrix3x3.identity;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ComputeCosTheta(
			in float lenASq,
			in float lenBSq,
			in float lenCSq,
			in float lenB,
			in float lenC) {
			var bc2 = lenB * lenC * 2.0f;
			return bc2 > IK_EPSILON ? (lenBSq + lenCSq - lenASq) / bc2 : 1.0f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeThetaAxis(
			ref Vector3f origPos,
			ref Vector3f fromPos,
			ref Vector3f toPos,
			out float theta,
			out Vector3f axis) {
			var dirFrom = fromPos - origPos;
			var dirTo = toPos - origPos;
			if (!VecNormalize2(ref dirFrom, ref dirTo)) {
				theta = 0.0f;
				axis = new Vector3f(0.0f, 0.0f, 1.0f);
				return false;
			}

			return ComputeThetaAxis(ref dirFrom, ref dirTo, out theta, out axis);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputeThetaAxis(
			ref Vector3f dirFrom,
			ref Vector3f dirTo,
			out float theta,
			out Vector3f axis) {
			axis = Vector3f.Cross(dirFrom, dirTo);
			if (!VecNormalize(ref axis)) {
				theta = 0.0f;
				axis = new Vector3f(0.0f, 0.0f, 1.0f);
				return false;
			}

			theta = Vector3f.Dot(dirFrom, dirTo);
			return true;
		}

		// Limited Square.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool LimitXY_Square(
			ref Vector3f dir,                // dirZ
			float limitXMinus,               // X-
			float limitXPlus,                // X+
			float limitYMinus,               // Z-
			float limitYPlus)               // Z+
		{
			var isXLimited = false;
			var isYLimited = false;

			if (dir.x < -limitXMinus) {
				dir.x = -limitXMinus;
				isXLimited = true;
			}
			else if (dir.x > limitXPlus) {
				dir.x = limitXPlus;
				isXLimited = true;
			}

			if (dir.y < -limitYMinus) {
				dir.y = -limitYMinus;
				isYLimited = true;
			}
			else if (dir.y > limitYPlus) {
				dir.y = limitYPlus;
				isYLimited = true;
			}

			if (isXLimited || isYLimited) {
				dir.z = Sqrt(1.0f - ((dir.x * dir.x) + (dir.y * dir.y)));
				return true;
			}
			else {
				if (dir.z < 0.0f) {
					dir.z = -dir.z;
					return true;
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool LimitYZ_Square(
			in bool isRight,
			ref Vector3f dir,                    // dirX
			in float limitYMinus,                  // Y-
			in float limitYPlus,                   // Y+
			in float limitZMinus,                  // Z-
			in float limitZPlus)                  // Z+
		{
			var isYLimited = false;
			var isZLimited = false;

			if (dir.y < -limitYMinus) {
				dir.y = -limitYMinus;
				isYLimited = true;
			}
			else if (dir.y > limitYPlus) {
				dir.y = limitYPlus;
				isYLimited = true;
			}

			if (dir.z < -limitZMinus) {
				dir.z = -limitZMinus;
				isZLimited = true;
			}
			else if (dir.z > limitZPlus) {
				dir.z = limitZPlus;
				isZLimited = true;
			}

			if (isYLimited || isZLimited) {
				dir.x = Sqrt(1.0f - ((dir.y * dir.y) + (dir.z * dir.z)));
				if (!isRight) {
					dir.x = -dir.x;
				}
				return true;
			}
			else {
				if (isRight) {
					if (dir.x < 0.0f) {
						dir.x = -dir.x;
						return true;
					}
				}
				else {
					if (dir.x >= 0.0f) {
						dir.x = -dir.x;
						return true;
					}
				}
			}

			return false;
		}

		// Limited Square.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool LimitXZ_Square(
			ref Vector3f dir,                // dirZ
			in float limitXMinus,               // X-
			in float limitXPlus,                // X+
			in float limitZMinus,               // Z-
			in float limitZPlus)               // Z+
		{
			var isXLimited = false;
			var isZLimited = false;

			if (dir.x < -limitXMinus) {
				dir.x = -limitXMinus;
				isXLimited = true;
			}
			else if (dir.x > limitXPlus) {
				dir.x = limitXPlus;
				isXLimited = true;
			}

			if (dir.z < -limitZMinus) {
				dir.z = -limitZMinus;
				isZLimited = true;
			}
			else if (dir.z > limitZPlus) {
				dir.z = limitZPlus;
				isZLimited = true;
			}

			if (isXLimited || isZLimited) {
				dir.y = Sqrt(1.0f - ((dir.x * dir.x) + (dir.z * dir.z)));
				return true;
			}
			else {
				if (dir.y < 0.0f) {
					dir.y = -dir.y;
					return true;
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool LimitXY(
			ref Vector3f dir,                // dirZ
			in float limitXMinus,              // X-
			in float limitXPlus,               // X+
			in float limitYMinus,              // Z-
			in float limitYPlus)               // Z+
		{
			var isXPlus = dir.x >= 0.0f;
			var isYPlus = dir.y >= 0.0f;
			var xLimit = isXPlus ? limitXPlus : limitXMinus;
			var yLimit = isYPlus ? limitYPlus : limitYMinus;

			var isLimited = false;
			if (xLimit <= IK_EPSILON && yLimit <= IK_EPSILON) {
				var limitedDir = new Vector3f(0.0f, 0.0f, 1.0f);
				var temp = limitedDir - dir;
				if (MathF.Abs(temp.x) > IK_EPSILON || MathF.Abs(temp.y) > IK_EPSILON || MathF.Abs(temp.z) > IK_EPSILON) {
					dir = limitedDir;
					isLimited = true;
				}
			}
			else {
				var inv_xLimit = (xLimit >= IK_EPSILON) ? (1.0f / xLimit) : 0.0f;
				var inv_yLimit = (yLimit >= IK_EPSILON) ? (1.0f / yLimit) : 0.0f;
				var localX = dir.x * inv_xLimit;
				var localY = dir.y * inv_yLimit;
				var localLen = Sqrt((localX * localX) + (localY * localY) + (dir.z * dir.z));

				var inv_localLen = (localLen > IK_EPSILON) ? (1.0f / localLen) : 0.0f;
				var nrm_localX = localX * inv_localLen; // Counts as sinTheta
				var nrm_localY = localY * inv_localLen; // Counts as cosTheta

				if (localLen > 1.0f) { // Outer circle.
					if (!isLimited) {
						isLimited = true;
						localX = nrm_localX;
						localY = nrm_localY;
					}
				}

				var worldX = isLimited ? (localX * xLimit) : dir.x;
				var worldY = isLimited ? (localY * yLimit) : dir.y;

				var isInverse = dir.z < 0.0f;

				if (isLimited) {
					var limitSinSq = (worldX * worldX) + (worldY * worldY);
					var limitSin = Sqrt(limitSinSq);
					var limitCos = Sqrt(1.0f - (limitSin * limitSin));
					dir.x = worldX;
					dir.y = worldY;
					dir.z = limitCos;
				}
				else if (isInverse) {
					isLimited = true;
					dir.z = -dir.z;
				}
			}

			return isLimited;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool LimitYZ(
			in bool isRight,
			ref Vector3f dir,                    // dirX
			in float limitYMinus,                  // Y-
			in float limitYPlus,                   // Y+
			in float limitZMinus,                  // Z-
			in float limitZPlus)                   // Z+
		{
			var isYPlus = dir.y >= 0.0f;
			var isZPlus = dir.z >= 0.0f;
			var yLimit = isYPlus ? limitYPlus : limitYMinus;
			var zLimit = isZPlus ? limitZPlus : limitZMinus;

			var isLimited = false;
			if (yLimit <= IK_EPSILON && zLimit <= IK_EPSILON) {
				var limitedDir = isRight ? new Vector3f(1.0f, 0.0f, 0.0f) : new Vector3f(-1.0f, 0.0f, 0.0f);
				var temp = limitedDir - dir;
				if (MathF.Abs(temp.x) > IK_EPSILON || MathF.Abs(temp.y) > IK_EPSILON || MathF.Abs(temp.z) > IK_EPSILON) {
					dir = limitedDir;
					isLimited = true;
				}
			}
			else {
				var inv_yLimit = (yLimit >= IK_EPSILON) ? (1.0f / yLimit) : 0.0f;
				var inv_zLimit = (zLimit >= IK_EPSILON) ? (1.0f / zLimit) : 0.0f;
				var localY = dir.y * inv_yLimit;
				var localZ = dir.z * inv_zLimit;
				var localLen = Sqrt((dir.x * dir.x) + (localY * localY) + (localZ * localZ));

				var inv_localLen = (localLen > IK_EPSILON) ? (1.0f / localLen) : 0.0f;
				var nrm_localY = localY * inv_localLen; // Counts as sinTheta
				var nrm_localZ = localZ * inv_localLen; // Counts as cosTheta

				if (localLen > 1.0f) { // Outer circle.
					if (!isLimited) {
						isLimited = true;
						localY = nrm_localY;
						localZ = nrm_localZ;
					}
				}

				var worldY = isLimited ? (localY * yLimit) : dir.y;
				var worldZ = isLimited ? (localZ * zLimit) : dir.z;

				var isInverse = (dir.x >= 0.0f) != isRight;

				if (isLimited) {
					var limitSinSq = (worldY * worldY) + (worldZ * worldZ);
					var limitSin = Sqrt(limitSinSq);
					var limitCos = Sqrt(1.0f - (limitSin * limitSin));
					dir.x = isRight ? limitCos : -limitCos;
					dir.y = worldY;
					dir.z = worldZ;
				}
				else if (isInverse) {
					isLimited = true;
					dir.x = -dir.x;
				}
			}

			return isLimited;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool FitToPlaneDir(ref Vector3f dir, in Vector3f planeDir) {
			var d = Vector3f.Dot(dir, planeDir);
			if (d is <= IK_EPSILON and >= (-IK_EPSILON)) {
				return false;
			}

			var tmp = dir - (planeDir * d);
			if (!VecNormalize(ref tmp)) {
				return false;
			}

			dir = tmp;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool LimitToPlaneDirY(ref Vector3f dir, in Vector3f planeDir, in float thetaY) {
			var d = Vector3f.Dot(dir, planeDir);
			if (d is <= IK_EPSILON and >= (-IK_EPSILON)) {
				return false;
			}

			if (d <= thetaY && d >= -thetaY) {
				return true;
			}

			var tmp = dir - (planeDir * d);
			var tmpLen = VecLength(tmp);
			if (tmpLen <= FLOAT_EPSILON) {
				return false;
			}

			var targetLen = Sqrt(1.0f - (thetaY * thetaY));

			tmp *= targetLen / tmpLen;

			dir = tmp;
			if (d >= 0.0f) {
				dir += planeDir * thetaY;
			}
			else {
				dir -= planeDir * thetaY;
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void LerpRotateBasis(out IKMatrix3x3 basis, ref Vector3f axis, float cos, float rate) {
			if (rate <= IK_EPSILON) {
				basis = IKMatrix3x3.identity;
				return;
			}

			if (rate <= 1.0f - IK_EPSILON) {
				var acos = (cos >= 1.0f - IK_EPSILON) ? 0.0f : ((cos <= -1.0f + IK_EPSILON) ? (180.0f * MathUtil.DEG_2_RADF) : (float)System.Math.Acos((float)cos));
				cos = (float)System.Math.Cos((float)(acos * rate));
			}

			MatSetAxisAngle(out basis, ref axis, cos);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3f LerpDir(ref Vector3f src, ref Vector3f dst, in float r) {
			if (ComputeThetaAxis(ref src, ref dst, out var theta, out var axis)) {
				LerpRotateBasis(out var basis, ref axis, theta, r);
				MatMultVec(out var tmp, basis, src);
				return tmp;
			}

			return dst;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3f FastLerpDir(ref Vector3f src, ref Vector3f dst, float r) {
			if (r <= IK_EPSILON) {
				return src;
			}
			else if (r >= 1.0f - IK_EPSILON) {
				return dst;
			}

			var tmp = src + ((dst - src) * r);
			var len = tmp.Magnitude;
			return len > IK_EPSILON ? tmp * (1.0f / len) : dst;
		}

		// for Finger.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool LimitFingerNotThumb(
			bool isRight,
			ref Vector3f dir, // dirX
			ref FastAngle limitYPlus,
			ref FastAngle limitYMinus,
			ref FastAngle limitZ) {
			var isLimited = false;

			// Yaw
			if (limitZ.cos > IK_EPSILON) {
				// Memo: Unstable when dir.z near 1.
				if (dir.z < -limitZ.sin || dir.z > limitZ.sin) {
					isLimited = true;
					var isPlus = dir.z >= 0.0f;
					var lenXY = Sqrt((dir.x * dir.x) + (dir.y * dir.y));
					if (limitZ.sin <= IK_EPSILON) { // Optimized.
						if (lenXY > IK_EPSILON) {
							dir.z = 0.0f;
							dir *= 1.0f / lenXY;
						}
						else { // Failsafe.
							dir.Set(isRight ? limitZ.cos : -limitZ.cos, 0.0f, isPlus ? limitZ.sin : -limitZ.sin);
						}
					}
					else {
						var lenZ = limitZ.sin * lenXY / limitZ.cos;
						dir.z = isPlus ? lenZ : -lenZ;

						var len = dir.Magnitude;
						if (len > IK_EPSILON) {
							dir *= 1.0f / len;
						}
						else { // Failsafe.
							dir.Set(isRight ? limitZ.cos : -limitZ.cos, 0.0f, isPlus ? limitZ.sin : -limitZ.sin);
						}
					}
				}
			}

			// Pitch
			{
				// Memo: Not use z.( For yaw limit. )
				var isPlus = dir.y >= 0.0f;
				var cosPitchLimit = isPlus ? limitYPlus.cos : limitYMinus.cos;
				if ((isRight && dir.x < cosPitchLimit) || (!isRight && dir.x > -cosPitchLimit)) {
					var lenY = Sqrt(1.0f - ((cosPitchLimit * cosPitchLimit) + (dir.z * dir.z)));
					dir.x = isRight ? cosPitchLimit : -cosPitchLimit;
					dir.y = isPlus ? lenY : -lenY;
				}
			}

			return isLimited;
		}
	}

}