using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	//
	// This class is just a wrapper around a dvector that provides convenient 3-element set/get access
	// Useful for things like treating a float array as a list of vectors
	//
	public class DVectorArray3<T> : IEnumerable<T>
	{
		public DVector<T> vector;

		public DVectorArray3(in int nCount = 0) {
			vector = new DVector<T>();
			if (nCount > 0) {
				vector.Resize(nCount * 3);
			}
		}

		public DVectorArray3(in T[] data) {
			vector = new DVector<T>(data);
		}

		public int Count => vector.Length / 3;

		public IEnumerator<T> GetEnumerator() {
			return vector.GetEnumerator();
		}

		public void Resize(in int count) {
			vector.Resize(3 * count);
		}

		public void Set(in int i, in T a, in T b, in T c) {
			vector.Insert(a, 3 * i);
			vector.Insert(b, (3 * i) + 1);
			vector.Insert(c, (3 * i) + 2);
		}

		public void Append(in T a, in T b, in T c) {
			vector.Push_back(a);
			vector.Push_back(b);
			vector.Push_back(c);
		}

		public void Clear() {
			vector.Clear();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}


	public sealed class DVectorArray3d : DVectorArray3<double>
	{
		public DVectorArray3d(in int nCount = 0) : base(nCount) {
		}
		public DVectorArray3d(in double[] data) : base(data) { }
		public Vector3d this[in int i]
		{
			get => new(vector[3 * i], vector[(3 * i) + 1], vector[(3 * i) + 2]);
			set => Set(i, value[0], value[1], value[2]);
		}

		public IEnumerable<Vector3d> AsVector3d() {
			for (var i = 0; i < Count; ++i) {
				yield return this[i];
			}
		}
	};


	public sealed class DVectorArray3f : DVectorArray3<float>
	{
		public DVectorArray3f(in int nCount = 0) : base(nCount) { }
		public DVectorArray3f(in float[] data) : base(data) { }
		public Vector3f this[in int i]
		{
			get => new(vector[3 * i], vector[(3 * i) + 1], vector[(3 * i) + 2]);
			set => Set(i, value[0], value[1], value[2]);
		}

		public IEnumerable<Vector3f> AsVector3f() {
			for (var i = 0; i < Count; ++i) {
				yield return this[i];
			}
		}
	};


	public sealed class DVectorArray3i : DVectorArray3<int>
	{
		public DVectorArray3i(in int nCount = 0) : base(nCount) { }
		public DVectorArray3i(in int[] data) : base(data) { }
		public Vector3i this[in int i]
		{
			get => new(vector[3 * i], vector[(3 * i) + 1], vector[(3 * i) + 2]);
			set => Set(i, value[0], value[1], value[2]);
		}
		// [RMS] for CW/CCW codes
		public void Set(in int i, in int a, in int b, in int c, in bool bCycle = false) {
			vector[3 * i] = a;
			if (bCycle) {
				vector[(3 * i) + 1] = c;
				vector[(3 * i) + 2] = b;
			}
			else {
				vector[(3 * i) + 1] = b;
				vector[(3 * i) + 2] = c;
			}
		}

		public IEnumerable<Vector3i> AsVector3i() {
			for (var i = 0; i < Count; ++i) {
				yield return this[i];
			}
		}
	};



	public sealed class DIndexArray3i : DVectorArray3<int>
	{
		public DIndexArray3i(in int nCount = 0) : base(nCount) { }
		public DIndexArray3i(in int[] data) : base(data) { }
		public Index3i this[in int i]
		{
			get => new(vector[3 * i], vector[(3 * i) + 1], vector[(3 * i) + 2]);
			set => Set(i, value[0], value[1], value[2]);
		}
		// [RMS] for CW/CCW codes
		public void Set(in int i, in int a, in int b, in int c, in bool bCycle = false) {
			vector[3 * i] = a;
			if (bCycle) {
				vector[(3 * i) + 1] = c;
				vector[(3 * i) + 2] = b;
			}
			else {
				vector[(3 * i) + 1] = b;
				vector[(3 * i) + 2] = c;
			}
		}

		public IEnumerable<Index3i> AsIndex3i() {
			for (var i = 0; i < Count; ++i) {
				yield return new Index3i(vector[3 * i], vector[(3 * i) + 1], vector[(3 * i) + 2]);
			}
		}
	};





	//
	// Same as DVectorArray3, but for 2D vectors/etc
	//
	public class DVectorArray2<T> : IEnumerable<T>
	{
		public DVector<T> vector;

		public DVectorArray2(in int nCount = 0) {
			vector = new DVector<T>();
			vector.Resize(nCount * 2);
		}

		public DVectorArray2(in T[] data) {
			vector = new DVector<T>(data);
		}

		public int Count => vector.Length / 2;

		public IEnumerator<T> GetEnumerator() {
			for (var i = 0; i < vector.Length; ++i) {
				yield return vector[i];
			}
		}

		public void Resize(in int count) {
			vector.Resize(2 * count);
		}

		public void Set(in int i, in T a, in T b) {
			vector.Insert(a, 2 * i);
			vector.Insert(b, (2 * i) + 1);
		}

		public void Append(in T a, in T b) {
			vector.Push_back(a);
			vector.Push_back(b);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
	public sealed class DVectorArray2d : DVectorArray2<double>
	{
		public DVectorArray2d(in int nCount = 0) : base(nCount) { }
		public DVectorArray2d(in double[] data) : base(data) { }
		public Vector2d this[in int i]
		{
			get => new(vector[2 * i], vector[(2 * i) + 1]);
			set => Set(i, value[0], value[1]);
		}

		public IEnumerable<Vector2d> AsVector2d() {
			for (var i = 0; i < Count; ++i) {
				yield return this[i];
			}
		}
	};
	public sealed class DVectorArray2f : DVectorArray2<float>
	{
		public DVectorArray2f(in int nCount = 0) : base(nCount) { }
		public DVectorArray2f(in float[] data) : base(data) { }
		public Vector2f this[in int i]
		{
			get => new(vector[2 * i], vector[(2 * i) + 1]);
			set => Set(i, value[0], value[1]);
		}

		public IEnumerable<Vector2d> AsVector2f() {
			for (var i = 0; i < Count; ++i) {
				yield return this[i];
			}
		}
	};



	public sealed class DIndexArray2i : DVectorArray2<int>
	{
		public DIndexArray2i(in int nCount = 0) : base(nCount) { }
		public DIndexArray2i(in int[] data) : base(data) { }
		public Index2i this[in int i]
		{
			get => new(vector[2 * i], vector[(2 * i) + 1]);
			set => Set(i, value[0], value[1]);
		}

		public IEnumerable<Index2i> AsIndex2i() {
			for (var i = 0; i < Count; ++i) {
				yield return new Index2i(vector[2 * i], vector[(2 * i) + 1]);
			}
		}
	};


}
