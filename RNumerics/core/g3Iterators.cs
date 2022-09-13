﻿using System;
using System.Collections;
using System.Collections.Generic;


namespace RNumerics
{
	/*
     * Utility generic iterators
     */

	/// <summary>
	/// Iterator that just returns a constant value N times
	/// </summary>
	public sealed class ConstantItr<T> : IEnumerable<T>
	{
		public T ConstantValue = default;
		public int N;

		public ConstantItr(in int count, in T constant) {
			N = count;
			ConstantValue = constant;
		}
		public IEnumerator<T> GetEnumerator() {
			for (var i = 0; i < N; ++i) {
				yield return ConstantValue;
			}
		}
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}


	/// <summary>
	/// Iterator that re-maps iterated values via a Func
	/// </summary>
	public sealed class RemapItr<T, T2> : IEnumerable<T>
	{
		public IEnumerable<T2> OtherItr;
		public Func<T2, T> ValueF;

		public RemapItr(in IEnumerable<T2> otherIterator, in Func<T2, T> valueFunction) {
			OtherItr = otherIterator;
			ValueF = valueFunction;
		}
		public IEnumerator<T> GetEnumerator() {
			foreach (var idx in OtherItr) {
				yield return ValueF(idx);
			}
		}
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}



	/// <summary>
	/// IList wrapper that remaps values via a Func (eg for index maps)
	/// </summary>
	public sealed class MappedList : IList<int>
	{
		public IList<int> BaseList;
		public Func<int, int> MapF = (i) => i;

		public MappedList(in IList<int> list, int[] map) {
			BaseList = list;
			MapF = (v) => map[v];
		}

		public int this[int index]
		{
			get => MapF(BaseList[index]);
			set => throw new NotImplementedException();
		}
		public int Count => BaseList.Count;
		public bool IsReadOnly => true;

		public void Add(int item) { throw new NotImplementedException(); }
		public void Clear() { throw new NotImplementedException(); }
		public void Insert(int index, int item) { throw new NotImplementedException(); }
		public bool Remove(int item) { throw new NotImplementedException(); }
		public void RemoveAt(int index) { throw new NotImplementedException(); }

		// could be implemented...
		public bool Contains(int item) { throw new NotImplementedException(); }
		public int IndexOf(int item) { throw new NotImplementedException(); }
		public void CopyTo(int[] array, int arrayIndex) { throw new NotImplementedException(); }

		public IEnumerator<int> GetEnumerator() {
			var N = BaseList.Count;
			for (var i = 0; i < N; ++i) {
				yield return MapF(BaseList[i]);
			}
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}




	/// <summary>
	/// IList wrapper for an Interval1i, ie sequential list of integers
	/// </summary>
	public struct IntSequence : IList<int>
	{
		Interval1i _range;

		public IntSequence(in Interval1i ival) {
			_range = ival;
		}
		public IntSequence(in int iStart, in int iEnd) {
			_range = new Interval1i(iStart, iEnd);
		}

		/// <summary> construct interval [0, N-1] </summary>
		static public IntSequence Range(in int N) { return new IntSequence(0, N - 1); }

		/// <summary> construct interval [0, N-1] </summary>
		static public IntSequence RangeInclusive(in int N) { return new IntSequence(0, N); }

		/// <summary> construct interval [start, start+N-1] </summary>
		static public IntSequence Range(in int start, in int N) { return new IntSequence(start, start + N - 1); }


		/// <summary> construct interval [a, b] </summary>
		static public IntSequence FromToInclusive(in int a, in int b) { return new IntSequence(a, b); }

		public int this[int index]
		{
			get => _range.a + index;
			set => throw new NotImplementedException();
		}
		public int Count => _range.Length + 1;
		public bool IsReadOnly => true;

		public void Add(int item) { throw new NotImplementedException(); }
		public void Clear() { throw new NotImplementedException(); }
		public void Insert(int index, int item) { throw new NotImplementedException(); }
		public bool Remove(int item) { throw new NotImplementedException(); }
		public void RemoveAt(int index) { throw new NotImplementedException(); }

		// could be implemented...
		public bool Contains(int item) { return _range.Contains(item); }
		public int IndexOf(int item) { throw new NotImplementedException(); }
		public void CopyTo(int[] array, int arrayIndex) { throw new NotImplementedException(); }

		public IEnumerator<int> GetEnumerator() {
			return _range.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}





}
