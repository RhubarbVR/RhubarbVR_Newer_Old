﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using RhuEngine.WorldObjects;
using RNumerics;
using RhuEngine.Linker;
using System.Reflection;

namespace RhuEngine
{
	public static class Helper
	{
		public static Matrix CastToNormal(this Assimp.Matrix4x4 matrix) {
			return new Matrix(matrix.A1, matrix.A2, matrix.A3, matrix.A4, matrix.B1, matrix.B2, matrix.B3, matrix.B4, matrix.C1, matrix.C2, matrix.C3, matrix.C4, matrix.D1, matrix.D2, matrix.D3, matrix.D4);
		}

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
			if (s == null) {
				return int.MinValue;
			}
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

		public static Colorf GetHashHue(this string str) {
			var hashCode = str.GetHashCodeSafe();
			var h = (float)(int)(ushort)hashCode%360f;
			var s = (float)(int)(byte)(hashCode >> 16) / 255f / 2f;
			var v = 0.5f + ((float)(int)(byte)(hashCode >> 24) / 255f * 0.5f);
			return new ColorHSV(h, s, v).RGBA;
		}
		public static Type MemberInnerType(this MemberInfo data) {
			switch (data.MemberType) {
				case MemberTypes.Field:
					return ((FieldInfo)data).FieldType;
				case MemberTypes.Property:
					return ((PropertyInfo)data).PropertyType;
				default:
					break;
			}
			return null;
		}
			public static Colorf GetTypeColor(this Type type) {
			if(type is null) {
				return Colorf.Black;
			}
			if (type == typeof(bool)) {
				return new Colorf(0.25f, 0.25f, 0.25f);
			}
			if (type == typeof(string)) {
				return new Colorf(0.5f,0f,0f);
			}
			if (type == typeof(Colorf)) {
				return new Colorf(0.75f, 0.4f, 0f);
			}
			if (type == typeof(Colorb)) {
				return new Colorf(0.75f,0.5f,0f);
			}
			if (type == typeof(Action)) {
				return new Colorf(1,1,1);
			}
			if (type == typeof(object)) {
				return new Colorf(0.68f, 0.82f, 0.3137254901960784f);
			}
			if (type == typeof(byte)) {
				return new Colorf(0, 0.1f, 1) + new Colorf(0, 1f, 0.7f);
			}
			if (type == typeof(ushort)) {
				return new Colorf(0, 0.1f, 1) + new Colorf(0, 1f, 0.6f);
			}
			if (type == typeof(uint)) {
				return new Colorf(0, 0.1f, 1) + new Colorf(0, 1f, 0.5f);
			}
			if (type == typeof(ulong)) {
				return new Colorf(0, 0.1f, 1) + new Colorf(0, 1f, 0.4f);
			}
			if (type == typeof(sbyte)) {
				return new Colorf(1f, 0.7f, 0) + new Colorf(0.7f, 1f,0f);
			}
			if (type == typeof(short)) {
				return new Colorf(1f, 0.7f, 0) + new Colorf(0.6f, 1f, 0f);
			}
			if (type == typeof(int)) {
				return new Colorf(1f, 0.7f, 0) + new Colorf(0.5f, 1f, 0f);
			}
			if (type == typeof(long)) {
				return new Colorf(1f, 0.7f, 0) + new Colorf(0.4f, 1f, 0f);
			}
			if (type == typeof(float)) {
				return new Colorf(0f, 1f, 1f) + new Colorf(0f, 1f, 0.7f);
			}
			if (type == typeof(double)) {
				return new Colorf(0f, 1f, 1f) + new Colorf(0f, 1f, 0.6f);
			}
			if (type == typeof(decimal)) {
				return new Colorf(0f, 1f, 1f) + new Colorf(0f, 1f, 0.5f);
			}
			if (typeof(ILinkable).IsAssignableFrom(type)) {
				if (type.GenericTypeArguments.Length > 0) {
					return type.GenericTypeArguments[0].GetTypeColor() + new Colorf(0.5f, 0.5f, 0.5f);
				}
			}
			var hashCode = type.GetFormattedName().GetHashCodeSafe();
			var h = (float)(int)(ushort)hashCode%360f;
			var s = (float)(int)(byte)(hashCode >> 16) / 255f / 2f;
			var v = 0.5f + ((float)(int)(byte)(hashCode >> 24) / 255f * 0.5f);
			return new ColorHSV(h, s, v).RGBA;
		}

		public static Vector3f Bezier(Vector3f a, Vector3f b, Vector3f c, Vector3f d, float t) {
			var it = Vector3f.Lerp(b, c, t);
			return Vector3f.Lerp(Vector3f.Lerp(Vector3f.Lerp(a, b, t), it, t), Vector3f.Lerp(it, Vector3f.Lerp(c, d, t), t), t);
		}

		public static float Dist(this Vector3f start, Vector3f end) {
			return System.Numerics.Vector3.Distance(start, end);
		}
		public static float DistSquared(this Vector3f start, Vector3f end) {
			return System.Numerics.Vector3.DistanceSquared(start, end);
		}
	}
}
