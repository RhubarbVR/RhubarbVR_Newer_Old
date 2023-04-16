// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Diagnostics;

namespace RNumerics.IK
{

	public partial class FullBodyIK
	{
		public static void SafeNew<TYPE_>(ref TYPE_ obj)
			where TYPE_ : class, new() {
			if (obj == null) {
				obj = new TYPE_();
			}
		}

		public static void SafeResize<TYPE_>(ref TYPE_[] objArray, int length) {
			if (objArray == null) {
				objArray = new TYPE_[length];
			}
			else {
				System.Array.Resize(ref objArray, length);
			}
		}

		public static void PrepareArray<TypeA, TypeB>(ref TypeA[] dstArray, TypeB[] srcArray) {
			if (srcArray != null) {
				if (dstArray == null || dstArray.Length != srcArray.Length) {
					dstArray = new TypeA[srcArray.Length];
				}
			}
			else {
				dstArray = null;
			}
		}

		public static void CloneArray<Type>(ref Type[] dstArray, Type[] srcArray) {
			if (srcArray != null) {
				if (dstArray == null || dstArray.Length != srcArray.Length) {
					dstArray = new Type[srcArray.Length];
				}
				for (int i = 0; i < srcArray.Length; ++i) {
					dstArray[i] = srcArray[i];
				}
			}
			else {
				dstArray = null;
			}
		}

		public static bool IsParentOfRecusively(IIKBoneTransform parent, IIKBoneTransform child) {
			while (child != null) {
				if (child.parent == parent) {
					return true;
				}

				child = child.parent;
			}

			return false;
		}

		//----------------------------------------------------------------------------------------------------------------

		static Bone _PrepareBone(Bone bone) {
			return (bone != null && bone.transformIsAlive) ? bone : null;
		}

		static Bone[] _PrepareBones(Bone leftBone, Bone rightBone) {
			Assert(leftBone != null && rightBone != null);
			if (leftBone != null && rightBone != null) {
				if (leftBone.transformIsAlive && rightBone.transformIsAlive) {
					var bones = new Bone[2];
					bones[0] = leftBone;
					bones[1] = rightBone;
					return bones;
				}
			}

			return null;
		}

		//----------------------------------------------------------------------------------------------------------------

		static bool _ComputeEyesRange(ref Vector3f eyesDir, float rangeTheta) {
			if (rangeTheta >= -IKEpsilon) { // range
				if (eyesDir.z < 0.0f) {
					eyesDir.z = -eyesDir.z;
				}

				return true;
			}
			else if (rangeTheta >= -1.0f + IKEpsilon) {
				float shiftZ = -rangeTheta;
				eyesDir.z = (eyesDir.z + shiftZ);
				if (eyesDir.z < 0.0f) {
					eyesDir.z *= 1.0f / (1.0f - shiftZ);
				}
				else {
					eyesDir.z *= 1.0f / (1.0f + shiftZ);
				}

				float xyLen = SAFBIKSqrt(eyesDir.x * eyesDir.x + eyesDir.y * eyesDir.y);
				if (xyLen > FLOAT_EPSILON) {
					float xyLenTo = SAFBIKSqrt(1.0f - eyesDir.z * eyesDir.z);
					float xyLenScale = xyLenTo / xyLen;
					eyesDir.x *= xyLenScale;
					eyesDir.y *= xyLenScale;
					return true;
				}
				else {
					eyesDir.x = 0.0f;
					eyesDir.y = 0.0f;
					eyesDir.z = 1.0f;
					return false;
				}
			}
			else {
				return true;
			}
		}

		//----------------------------------------------------------------------------------------------------------------

		[System.Diagnostics.Conditional("SAFULLBODYIK_DEBUG")]
		public static void DebugLog(object msg) {
			Debugger.Log(1, "IK", msg?.ToString() ?? "null");
		}

		[System.Diagnostics.Conditional("SAFULLBODYIK_DEBUG")]
		public static void DebugLogWarning(object msg) {
			Debugger.Log(3, "IK", msg?.ToString() ?? "null");
		}

		[System.Diagnostics.Conditional("SAFULLBODYIK_DEBUG")]
		public static void DebugLogError(object msg) {
			Debugger.Log(4, "IK", msg?.ToString() ?? "null");
		}

		[System.Diagnostics.Conditional("SAFULLBODYIK_DEBUG")]
		public static void Assert(bool cmp) {
			if (!cmp) {
				Debugger.Log(4, "IK", "Assert");
				Debugger.Break();
			}
		}

		//----------------------------------------------------------------------------------------------------------------

		[System.Diagnostics.Conditional("SAFULLBODYIK_DEBUG_CHECKEVAL")]
		public static void CheckNormalized(Vector3f v) {
			float epsilon = 1e-4f;
			float n = v.x * v.x + v.y * v.y + v.z * v.z;
			if (n < 1.0f - epsilon || n > 1.0f + epsilon) {
				Debugger.Log(4, "IK", "CheckNormalized:" + n.ToString("F6"));
				Debugger.Break();
			}
		}

		[System.Diagnostics.Conditional("SAFULLBODYIK_DEBUG_CHECKEVAL")]
		public static void CheckNaN(float f) {
			if (float.IsNaN(f)) {
				Debugger.Log(4, "IK", "NaN");
			}
		}

		[System.Diagnostics.Conditional("SAFULLBODYIK_DEBUG_CHECKEVAL")]
		public static void CheckNaN(Vector3f v) {
			if (float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z)) {
				Debugger.Log(4, "IK", "NaN:" + v);
			}
		}

		[System.Diagnostics.Conditional("SAFULLBODYIK_DEBUG_CHECKEVAL")]
		public static void CheckNaN(Quaternionf q) {
			if (float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w)) {
				Debugger.Log(4, "IK", "NaN:" + q);
			}
		}
	}

}