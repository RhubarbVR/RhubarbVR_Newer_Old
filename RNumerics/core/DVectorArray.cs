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

		public DVectorArray3(int nCount = 0) {
			vector = new DVector<T>();
			if (nCount > 0) {
				vector.Resize(nCount * 3);
			}
		}

		public DVectorArray3(T[] data) {
			vector = new DVector<T>(data);
		}

		public int Count => vector.Length / 3;

		public IEnumerator<T> GetEnumerator() {
			return vector.GetEnumerator();
		}

		public void Resize(int count) {
			vector.Resize(3 * count);
		}

		public void Set(int i, T a, T b, T c) {
			vector.Insert(a, 3 * i);
			vector.Insert(b, (3 * i) + 1);
			vector.Insert(c, (3 * i) + 2);
		}

		public void Append(T a, T b, T c) {
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


	public class DVectorArray3d : DVectorArray3<double>
	{
		public DVectorArray3d(int nCount = 0) : base(nCount) {
		}
		public DVectorArray3d(double[] data) : base(data) { }
		public Vector3d this[int i]
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


	public class DVectorArray3f : DVectorArray3<float>
	{
		public DVectorArray3f(int nCount = 0) : base(nCount) { }
		public DVectorArray3f(float[] data) : base(data) { }
		public Vector3f this[int i]
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


	public class DVectorArray3i : DVectorArray3<int>
	{
		public DVectorArray3i(int nCount = 0) : base(nCount) { }
		public DVectorArray3i(int[] data) : base(data) { }
		public Vector3i this[int i]
		{
			get => new(vector[3 * i], vector[(3 * i) + 1], vector[(3 * i) + 2]);
			set => Set(i, value[0], value[1], value[2]);
		}
		// [RMS] for CW/CCW codes
		public void Set(int i, int a, int b, int c, bool bCycle = false) {
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



	public class DIndexArray3i : DVectorArray3<int>
	{
		public DIndexArray3i(int nCount = 0) : base(nCount) { }
		public DIndexArray3i(int[] data) : base(data) { }
		public Index3i this[int i]
		{
			get => new(vector[3 * i], vector[(3 * i) + 1], vector[(3 * i) + 2]);
			set => Set(i, value[0], value[1], value[2]);
		}
		// [RMS] for CW/CCW codes
		public void Set(int i, int a, int b, int c, bool bCycle = false) {
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

		public DVectorArray2(int nCount = 0) {
			vector = new DVector<T>();
			vector.Resize(nCount * 2);
		}

		public DVectorArray2(T[] data) {
			vector = new DVector<T>(data);
		}

		public int Count => vector.Length / 2;

		public IEnumerator<T> GetEnumerator() {
			for (var i = 0; i < vector.Length; ++i) {
				yield return vector[i];
			}
		}

		public void Resize(int count) {
			vector.Resize(2 * count);
		}

		public void Set(int i, T a, T b) {
			vector.Insert(a, 2 * i);
			vector.Insert(b, (2 * i) + 1);
		}

		public void Append(T a, T b) {
			vector.Push_back(a);
			vector.Push_back(b);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
	public class DVectorArray2d : DVectorArray2<double>
	{
		public DVectorArray2d(int nCount = 0) : base(nCount) { }
		public DVectorArray2d(double[] data) : base(data) { }
		public Vector2d this[int i]
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
	public class DVectorArray2f : DVectorArray2<float>
	{
		public DVectorArray2f(int nCount = 0) : base(nCount) { }
		public DVectorArray2f(float[] data) : base(data) { }
		public Vector2f this[int i]
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



	public class DIndexArray2i : DVectorArray2<int>
	{
		public DIndexArray2i(int nCount = 0) : base(nCount) { }
		public DIndexArray2i(int[] data) : base(data) { }
		public Index2i this[int i]
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
