using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Assimp;

namespace RNumerics
{

	public interface IRDynamic { }

	public unsafe struct RDynamic<T>: IRDynamic
	{
		public static T DefaultValue = default;

		private readonly T _data;

		public RDynamic(T data) {
			_data = data;
		}

		private delegate T FixedData(ref T a, ref T b);

		private static Func<T, T, T> GetMethodAction(string target, Func<T, T, T> LastTry) {
			foreach (var item in typeof(T).GetMethods()) {
				if (!item.IsStatic) {
					continue;
				}
				if (item.Name == target) {
					var prams = item.GetParameters();
					if (prams.Length == 2) {
						if (prams[0].ParameterType == prams[1].ParameterType && prams[0].ParameterType == typeof(T).MakeByRefType()) {
							return (T a, T b) => ((FixedData)item.CreateDelegate(typeof(FixedData)))(ref a, ref b);
						}
						if (prams[0].ParameterType == prams[1].ParameterType && prams[0].ParameterType == typeof(T)) {
							return (Func<T, T, T>)item.CreateDelegate(typeof(Func<T, T, T>));
						}
					}
				}
			}
			return LastTry;
		}

		private delegate bool FixedDataBool(ref T a, ref T b);

		private static Func<T, T, bool> GetMethodActionBool(string target, Func<T, T, bool> LastTry) {
			foreach (var item in typeof(T).GetMethods()) {
				if (!item.IsStatic) {
					continue;
				}
				if (item.Name == target) {
					var prams = item.GetParameters();
					if (prams.Length == 2) {
						if (prams[0].ParameterType == prams[1].ParameterType && prams[0].ParameterType == typeof(T).MakeByRefType()) {
							return (T a, T b) => ((FixedDataBool)item.CreateDelegate(typeof(FixedDataBool)))(ref a, ref b);
						}
						if (prams[0].ParameterType == prams[1].ParameterType && prams[0].ParameterType == typeof(T)) {
							return (Func<T, T, bool>)item.CreateDelegate(typeof(Func<T, T, bool>));
						}
					}
				}
			}
			return LastTry;
		}


		private static Func<T, T, T> _op_Addition;
		public static RDynamic<T> operator +(in RDynamic<T> v0, in RDynamic<T> v1) {
			_op_Addition ??= GetMethodAction("op_Addition", (T a, T b) => (dynamic)a + (dynamic)b);
			return _op_Addition(v0, v1);
		}
		private static Func<T, T, T> _op_Subtraction;
		public static RDynamic<T> operator -(in RDynamic<T> v0, in RDynamic<T> v1) {
			_op_Subtraction ??= GetMethodAction("op_Subtraction", (T a, T b) => (dynamic)a - (dynamic)b);
			return _op_Subtraction(v0, v1);
		}
		private static Func<T, T, T> _op_Multiply;
		public static RDynamic<T> operator *(in RDynamic<T> v0, in RDynamic<T> v1) {
			_op_Multiply ??= GetMethodAction("op_Multiply", (T a, T b) => (dynamic)a * (dynamic)b);
			return _op_Multiply(v0, v1);
		}
		private static Func<T, T, T> _op_Division;
		public static RDynamic<T> operator /(in RDynamic<T> v0, in RDynamic<T> v1) {
			_op_Division ??= GetMethodAction("op_Division", (T a, T b) => (dynamic)a / (dynamic)b);
			return _op_Division(v0, v1);
		}
		private static Func<T, T, T> _op_Modulus;
		public static RDynamic<T> operator %(in RDynamic<T> v0, in RDynamic<T> v1) {
			_op_Modulus ??= GetMethodAction("op_Modulus", (T a, T b) => (dynamic)a % (dynamic)b);
			return _op_Modulus(v0, v1);
		}
		private static Func<T, T, T> _op_LeftShift;
		public static RDynamic<T> operator <<(in RDynamic<T> v0, in RDynamic<T> v1) {
			_op_LeftShift ??= GetMethodAction("op_LeftShift", (T a, T b) => (dynamic)a << (dynamic)b);
			return _op_LeftShift(v0, v1);
		}
		private static Func<T, T, T> _op_RightShift;
		public static RDynamic<T> operator >>(in RDynamic<T> v0, in RDynamic<T> v1) {
			_op_RightShift ??= GetMethodAction("op_RightShift", (T a, T b) => (dynamic)a >> (dynamic)b);
			return _op_RightShift(v0, v1);
		}
		private static Func<T, T, T> _op_BitwiseOr;

		public static RDynamic<T> operator |(in RDynamic<T> v0, in RDynamic<T> v1) {
			_op_BitwiseOr ??= GetMethodAction("op_BitwiseOr", (T a, T b) => (dynamic)a | (dynamic)b);
			return _op_BitwiseOr(v0, v1);
		}
		private static Func<T, T, T> _op_BitwiseAnd;

		public static RDynamic<T> operator &(in RDynamic<T> v0, in RDynamic<T> v1) {
			_op_BitwiseAnd ??= GetMethodAction("op_BitwiseAnd", (T a, T b) => (dynamic)a & (dynamic)b);
			return _op_BitwiseAnd(v0, v1);
		}
		private static Func<T, T, T> _op_ExclusiveOr;

		public static RDynamic<T> operator ^(in RDynamic<T> v0, in RDynamic<T> v1) {
			_op_ExclusiveOr ??= GetMethodAction("op_ExclusiveOr", (T a, T b) => (dynamic)a ^ (dynamic)b);
			return _op_ExclusiveOr(v0, v1);
		}

		private static Func<T, T, bool> _op_Equality;

		public static bool operator ==(in RDynamic<T> a, in RDynamic<T> b) {
			_op_Equality ??= GetMethodActionBool("op_Equality", (T a, T b) => (dynamic)a == (dynamic)b);
			return _op_Equality(a, b);
		}
		private static Func<T, T, bool> _op_Inequality;

		public static bool operator !=(in RDynamic<T> a, in RDynamic<T> b) {
			_op_Inequality ??= GetMethodActionBool("op_Inequality", (T a, T b) => (dynamic)a != (dynamic)b);
			return _op_Inequality(a, b);
		}

		private static Func<T, T, bool> _op_LessThan;

		public static bool operator <(in RDynamic<T> a, in RDynamic<T> b) {
			_op_LessThan ??= GetMethodActionBool("op_LessThan", (T a, T b) => (dynamic)a < (dynamic)b);
			return _op_LessThan(a, b);
		}

		private static Func<T, T, bool> _op_GreaterThan;

		public static bool operator >(in RDynamic<T> a, in RDynamic<T> b) {
			_op_GreaterThan ??= GetMethodActionBool("op_GreaterThan", (T a, T b) => (dynamic)a > (dynamic)b);
			return _op_GreaterThan(a, b);
		}



		private static Func<T, T, bool> _op_LessThanOrEqual;

		public static bool operator <=(in RDynamic<T> a, in RDynamic<T> b) {
			_op_LessThanOrEqual ??= GetMethodActionBool("op_LessThanOrEqual", (T a, T b) => (dynamic)a <= (dynamic)b);
			return _op_LessThanOrEqual(a, b);
		}

		private static Func<T, T, bool> _op_GreaterThanOrEqual;

		public static bool operator >=(in RDynamic<T> a, in RDynamic<T> b) {
			_op_GreaterThanOrEqual ??= GetMethodActionBool("op_GreaterThanOrEqual", (T a, T b) => (dynamic)a >= (dynamic)b);
			return _op_GreaterThanOrEqual(a, b);
		}

		public static implicit operator RDynamic<T>(in T b) => new(b);

		public static implicit operator T(in RDynamic<T> b) => b._data;

		public T2 CastTo<T2>() {
			return (T2)(dynamic)_data;
		}


		public static T2 CastTo<T2>(T value) {
			return ((RDynamic<T>)value).CastTo<T2>();
		}
	}
}
