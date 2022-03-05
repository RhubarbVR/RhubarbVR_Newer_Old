using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using StereoKit;
using RhuEngine.WorldObjects;

namespace RhuEngine
{
	public static class Helper
	{
		public static string CleanPath(this string path) {
			var regexSearch = new string(Path.GetInvalidPathChars());
			var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
			return r.Replace(path, "");
		}
		
		public static string TouchUpPath(this string path) {
			return path.Replace("\\", "/");
		}

		public static Matrix RotNormalized(this Matrix oldmatrix) {
			oldmatrix.Decompose(out var trans, out var rot, out var scale);
			return Matrix.TRS(trans, rot, scale);
		}

		public static Matrix GetLocal(this Matrix global, Matrix newglobal) {
			return newglobal * global.Inverse;
		}

		public static string GetFormattedName(this Type type) {
			if(type == null) {
				return "Null";
			}
			if (type.IsGenericType) {
				var genericArguments = type.GetGenericArguments()
									.Select(x => x.Name)
									.Aggregate((x1, x2) => $"{x1}, {x2}");
				return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}"
					 + $" <{genericArguments}>";
			}
			return type.Name;
		}
		public static unsafe int GetHashCodeSafe(this string s) {
			fixed (char* str = s.ToCharArray()) {
				var chPtr = str;
				var num = 0x15051505;
				var num2 = num;
				var numPtr = (int*)chPtr;
				for (var i = s.Length; i > 0; i -= 4) {
					num = ((num << 5) + num + (num >> 0x1b)) ^ numPtr[0];
					if (i <= 2) {
						break;
					}
					num2 = ((num2 << 5) + num2 + (num2 >> 0x1b)) ^ numPtr[1];
					numPtr += 2;
				}
				return num + (num2 * 0x5d588b65);
			}
		}

		public static Color GetTypeColor(this Type type) {
			if(type is null) {
				return Color.Black;
			}
			if (type == typeof(bool)) {
				return new Color(0.25f, 0.25f, 0.25f);
			}
			if (type == typeof(string)) {
				return new Color(0.5f,0f,0f);
			}
			if (type == typeof(Color)) {
				return new Color(0.75f, 0.4f, 0f);
			}
			if (type == typeof(Color32)) {
				return new Color(0.75f,0.5f,0f);
			}
			if (type == typeof(Action)) {
				return new Color(1,1,1);
			}
			if (type == typeof(object)) {
				return new Color(0.68f, 0.82f, 0.3137254901960784f);
			}
			if (type == typeof(byte)) {
				return new Color(0, 0.1f, 1) * new Color(0, 1f, 0.7f);
			}
			if (type == typeof(ushort)) {
				return new Color(0, 0.1f, 1) * new Color(0, 1f, 0.6f);
			}
			if (type == typeof(uint)) {
				return new Color(0, 0.1f, 1) * new Color(0, 1f, 0.5f);
			}
			if (type == typeof(ulong)) {
				return new Color(0, 0.1f, 1) * new Color(0, 1f, 0.4f);
			}
			if (type == typeof(sbyte)) {
				return new Color(1f, 0.7f, 0) * new Color(0.7f, 1f,0f);
			}
			if (type == typeof(short)) {
				return new Color(1f, 0.7f, 0) * new Color(0.6f, 1f, 0f);
			}
			if (type == typeof(int)) {
				return new Color(1f, 0.7f, 0) * new Color(0.5f, 1f, 0f);
			}
			if (type == typeof(long)) {
				return new Color(1f, 0.7f, 0) * new Color(0.4f, 1f, 0f);
			}
			if (type == typeof(float)) {
				return new Color(0f, 1f, 1f) * new Color(0f, 1f, 0.7f);
			}
			if (type == typeof(double)) {
				return new Color(0f, 1f, 1f) * new Color(0f, 1f, 0.6f);
			}
			if (type == typeof(decimal)) {
				return new Color(0f, 1f, 1f) * new Color(0f, 1f, 0.5f);
			}
			if (typeof(ILinkable).IsAssignableFrom(type)) {
				if (type.GenericTypeArguments.Length > 0) {
					return type.GenericTypeArguments[0].GetTypeColor() * new Color(0.5f, 0.5f, 0.5f);
				}
			}
			var hashCode = type.GetFormattedName().GetHashCodeSafe();
			var h = (float)(int)(ushort)hashCode / 65545f;
			var s = (float)(int)(byte)(hashCode >> 16) / 255f / 2f;
			var v = 0.5f + ((float)(int)(byte)(hashCode >> 24) / 255f * 0.5f);
			return Color.HSV(h, s, v);
		}

		public static bool IsHovering(FingerId finger, JointId joint, Vec3 pos, Vec3 bounds, Handed handed) {
			var compsize = Hierarchy.ToLocal(Input.Hand(handed).Get(finger, joint).Pose).position - pos;
			return !((Math.Abs(compsize.x) > bounds.x) || (Math.Abs(compsize.y) > bounds.y) || (Math.Abs(compsize.z) > bounds.z));
		}

		public static bool IsHovering(FingerId finger, JointId joint,Vec3 pos, Vec3 bounds,out Handed handed) {
			if (IsHovering(finger,joint,pos, bounds, Handed.Left)) {
				handed = Handed.Left;
				return true;
			}
			else if (IsHovering(finger, joint, pos, bounds, Handed.Right)) {
				handed = Handed.Right;
				return true;
			}
			handed = Handed.Max;
			return false;
		}


		public static Vec3 Bezier(Vec3 a, Vec3 b, Vec3 c, Vec3 d, float t) {
			var it = Vec3.Lerp(b, c, t);
			return Vec3.Lerp(Vec3.Lerp(Vec3.Lerp(a, b, t), it, t), Vec3.Lerp(it, Vec3.Lerp(c, d, t), t), t);
		}

		public static float Dist(this Vec3 start, Vec3 end) {
			return System.Numerics.Vector3.Distance(start.v, end.v);
		}
		public static float DistSquared(this Vec3 start, Vec3 end) {
			return System.Numerics.Vector3.DistanceSquared(start.v, end.v);
		}
	}
}
