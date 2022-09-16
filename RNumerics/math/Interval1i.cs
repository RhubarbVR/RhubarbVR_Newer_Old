using System;
using System.Collections;
using System.Collections.Generic;

using MessagePack;

namespace RNumerics
{
	// Interval [a,b] over Integers
	// Note that Interval1i has an enumerator, so you can directly
	//  enumerate over the range of values (inclusive!!)
	//
	//   TODO: should check that a <= b !!
	[MessagePackObject]
	public struct Interval1i : IEnumerable<int>
	{
		[Key(0)]
		public int a;
		[Key(1)]
		public int b;

		public Interval1i(in int f) { a = b = f; }
		public Interval1i(in int x, in int y) { a = x; b = y; }
		public Interval1i(in int[] v2) { a = v2[0]; b = v2[1]; }
		public Interval1i(in Interval1i copy) { a = copy.a; b = copy.b; }


		[IgnoreMember]
		static public readonly Interval1i Zero = new (0, 0);
		[IgnoreMember]
		static public readonly Interval1i Empty = new (int.MaxValue, -int.MaxValue);
		[IgnoreMember]
		static public readonly Interval1i Infinite = new (-int.MaxValue, int.MaxValue);

		/// <summary> construct interval [0, N-1] </summary>
		static public Interval1i Range(in int N) { return new (0, N - 1); }

		/// <summary> construct interval [0, N-1] </summary>
		static public Interval1i RangeInclusive(in int N) { return new (0, N); }

		/// <summary> construct interval [start, start+N-1] </summary>
		static public Interval1i Range(in int start, in int N) { return new (start, start + N - 1); }


		/// <summary> construct interval [a, b] </summary>
		static public Interval1i FromToInclusive(in int a, in int b) { return new (a, b); }



		[IgnoreMember]
		public int this[in int key]
		{
			get => (key == 0) ? a : b;
			set {
				if (key == 0) { a = value; }
				else {
					b = value;
				}
			}
		}


		[IgnoreMember]
		public int LengthSquared => (a - b) * (a - b);
		[IgnoreMember]
		public int Length => b - a;

		[IgnoreMember]
		public int Center => (b + a) / 2;

		public void Contain(in int d) {
			if (d < a) {
				a = d;
			}

			if (d > b) {
				b = d;
			}
		}

		public bool Contains(in int d) {
			return d >= a && d <= b;
		}


		public bool Overlaps(in Interval1i o) {
			return !(o.a > b || o.b < a);
		}

		public int SquaredDist(in Interval1i o) {
			return b < o.a ? (o.a - b) * (o.a - b) : a > o.b ? (a - o.b) * (a - o.b) : 0;
		}
		public int Dist(in Interval1i o) {
			return b < o.a ? o.a - b : a > o.b ? a - o.b : 0;
		}


		public void Set(in Interval1i o) {
			a = o.a;
			b = o.b;
		}
		public void Set(in int fA, in int fB) {
			a = fA;
			b = fB;
		}



		public static Interval1i operator -(in Interval1i v) => new(-v.a, -v.b);


		public static Interval1i operator +(in Interval1i a, in int f) => new(a.a + f, a.b + f);
		public static Interval1i operator -(in Interval1i a, in int f) => new(a.a - f, a.b - f);

		public static Interval1i operator *(in Interval1i a, in int f) => new(a.a * f, a.b * f);


		public IEnumerator<int> GetEnumerator() {
			for (var i = a; i <= b; ++i) {
				yield return i;
			}
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public override string ToString() {
			return string.Format("[{0},{1}]", a, b);
		}


	}
}
