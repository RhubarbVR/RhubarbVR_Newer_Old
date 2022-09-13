﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	//
	// This class is just a wrapper around a static array that provides convenient 3-element set/get access
	// Useful for things like treating a float array as a list of vectors
	//
	public class VectorArray3<T> : IEnumerable<T>
	{
		public T[] array;

		public VectorArray3(in int nCount = 0) {
			array = new T[nCount * 3];
		}

		public VectorArray3(in T[] data) {
			array = data;
		}

		public int Count => array.Length / 3;

		public IEnumerator<T> GetEnumerator() {
			for (var i = 0; i < array.Length; ++i) {
				yield return array[i];
			}
		}

		public void Resize(in int Count) {
			array = new T[3 * Count];
		}

		public void Set(in int i, in T a, in T b, in T c) {
			array[3 * i] = a;
			array[(3 * i) + 1] = b;
			array[(3 * i) + 2] = c;
		}

		public void Set(in int iStart, in int iCount, in VectorArray3<T> source) {
			Array.Copy(source.array, 0, array, 3 * iStart, 3 * iCount);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return array.GetEnumerator();
		}
	}


	public sealed class VectorArray3d : VectorArray3<double>
	{
		public VectorArray3d(in int nCount) : base(nCount) {
		}
		public VectorArray3d(in double[] data) : base(data) { }
		public Vector3d this[in int i]
		{
			get => new(array[3 * i], array[(3 * i) + 1], array[(3 * i) + 2]);
			set => Set(i, value[0], value[1], value[2]);
		}

		public IEnumerable<Vector3d> AsVector3d() {
			for (var i = 0; i < Count; ++i) {
				yield return this[i];
			}
		}
	};


	public sealed class VectorArray3f : VectorArray3<float>
	{
		public VectorArray3f(in int nCount) : base(nCount) { }
		public VectorArray3f(in float[] data) : base(data) { }
		public Vector3f this[in int i]
		{
			get => new(array[3 * i], array[(3 * i) + 1], array[(3 * i) + 2]);
			set => Set(i, value[0], value[1], value[2]);
		}

		public IEnumerable<Vector3f> AsVector3f() {
			for (var i = 0; i < Count; ++i) {
				yield return this[i];
			}
		}
	};


	public sealed class VectorArray3i : VectorArray3<int>
	{
		public VectorArray3i(in int nCount) : base(nCount) { }
		public VectorArray3i(in int[] data) : base(data) { }
		public Vector3i this[in int i]
		{
			get => new(array[3 * i], array[(3 * i) + 1], array[(3 * i) + 2]);
			set => Set(i, value[0], value[1], value[2]);
		}
		// [RMS] for CW/CCW codes
		public void Set(in int i, in int a, in int b, in int c, in bool bCycle = false) {
			array[3 * i] = a;
			if (bCycle) {
				array[(3 * i) + 1] = c;
				array[(3 * i) + 2] = b;
			}
			else {
				array[(3 * i) + 1] = b;
				array[(3 * i) + 2] = c;
			}
		}

		public IEnumerable<Vector3i> AsVector3i() {
			for (var i = 0; i < Count; ++i) {
				yield return this[i];
			}
		}
	};



	public sealed class IndexArray3i : VectorArray3<int>
	{
		public IndexArray3i(in int nCount) : base(nCount) { }
		public IndexArray3i(in int[] data) : base(data) { }
		public Index3i this[in int i]
		{
			get => new(array[3 * i], array[(3 * i) + 1], array[(3 * i) + 2]);
			set => Set(i, value[0], value[1], value[2]);
		}
		// [RMS] for CW/CCW codes
		public void Set(in int i, in int a, in int b, in int c, in bool bCycle = false) {
			array[3 * i] = a;
			if (bCycle) {
				array[(3 * i) + 1] = c;
				array[(3 * i) + 2] = b;
			}
			else {
				array[(3 * i) + 1] = b;
				array[(3 * i) + 2] = c;
			}
		}

		public IEnumerable<Index3i> AsIndex3i() {
			for (var i = 0; i < Count; ++i) {
				yield return new Index3i(array[3 * i], array[(3 * i) + 1], array[(3 * i) + 2]);
			}
		}
	};





	//
	// Same as VectorArray3, but for 2D vectors/etc
	//
	public class VectorArray2<T> : IEnumerable<T>
	{
		public T[] array;

		public VectorArray2(in int nCount = 0) {
			array = new T[nCount * 2];
		}

		public VectorArray2(in T[] data) {
			array = data;
		}

		public int Count => array.Length / 2;

		public IEnumerator<T> GetEnumerator() {
			for (var i = 0; i < array.Length; ++i) {
				yield return array[i];
			}
		}

		public void Resize(in int Count) {
			array = new T[2 * Count];
		}

		public void Set(in int i, in T a, in T b) {
			array[2 * i] = a;
			array[(2 * i) + 1] = b;
		}

		public void Set(in int iStart, in int iCount, in VectorArray2<T> source) {
			Array.Copy(source.array, 0, array, 2 * iStart, 2 * iCount);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return array.GetEnumerator();
		}
	}
	public sealed class VectorArray2d : VectorArray2<double>
	{
		public VectorArray2d(in int nCount) : base(nCount) { }
		public VectorArray2d(in double[] data) : base(data) { }
		public Vector2d this[in int i]
		{
			get => new(array[2 * i], array[(2 * i) + 1]);
			set => Set(i, value[0], value[1]);
		}

		public IEnumerable<Vector2d> AsVector2d() {
			for (var i = 0; i < Count; ++i) {
				yield return this[i];
			}
		}
	};
	public sealed class VectorArray2f : VectorArray2<float>
	{
		public VectorArray2f(in int nCount) : base(nCount) { }
		public VectorArray2f(in float[] data) : base(data) { }
		public Vector2f this[in int i]
		{
			get => new(array[2 * i], array[(2 * i) + 1]);
			set => Set(i, value[0], value[1]);
		}

		public IEnumerable<Vector2d> AsVector2f() {
			for (var i = 0; i < Count; ++i) {
				yield return this[i];
			}
		}
	};



	public sealed class IndexArray2i : VectorArray2<int>
	{
		public IndexArray2i(in int nCount) : base(nCount) { }
		public IndexArray2i(in int[] data) : base(data) { }
		public Index2i this[in int i]
		{
			get => new(array[2 * i], array[(2 * i) + 1]);
			set => Set(i, value[0], value[1]);
		}

		public IEnumerable<Index2i> AsIndex2i() {
			for (var i = 0; i < Count; ++i) {
				yield return new Index2i(array[2 * i], array[(2 * i) + 1]);
			}
		}
	};











	public  class VectorArray4<T> : IEnumerable<T>
	{
		public T[] array;

		public VectorArray4(in int nCount = 0) {
			array = new T[nCount * 4];
		}

		public VectorArray4(in T[] data) {
			array = data;
		}

		public int Count => array.Length / 4;

		public IEnumerator<T> GetEnumerator() {
			for (var i = 0; i < array.Length; ++i) {
				yield return array[i];
			}
		}

		public void Resize(in int Count) {
			array = new T[4 * Count];
		}

		public void Set(in int i, in T a, in T b, in T c, in T d) {
			var j = 4 * i;
			array[j] = a;
			array[j + 1] = b;
			array[j + 2] = c;
			array[j + 3] = d;
		}

		public void Set(in int iStart, in int iCount, in VectorArray4<T> source) {
			Array.Copy(source.array, 0, array, 4 * iStart, 4 * iCount);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return array.GetEnumerator();
		}
	}



	public sealed class IndexArray4i : VectorArray4<int>
	{
		public IndexArray4i(in int nCount) : base(nCount) { }
		public IndexArray4i(in int[] data) : base(data) { }
		public Index4i this[in int i]
		{
			get { var j = 4 * i; return new Index4i(array[j], array[j + 1], array[j + 2], array[j + 3]); }
			set => Set(i, value[0], value[1], value[2], value[4]);
		}
		public IEnumerable<Index4i> AsIndex4i() {
			for (var i = 0; i < Count; ++i) {
				yield return this[i];
			}
		}
	};



}
