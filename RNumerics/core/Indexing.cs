using System;
using System.Collections.Generic;
using System.Collections;


namespace RNumerics
{

	// An enumerator that enumerates over integers [start, start+count)
	// (useful when you need to do things like iterate over indices of an array rather than values)
	public sealed class IndexRangeEnumerator : IEnumerable<int>
	{
		readonly int _start = 0;
		readonly int _count = 0;
		public IndexRangeEnumerator(in int count) { _count = count; }
		public IndexRangeEnumerator(in int start, in int count) { _start = start; _count = count; }
		public IEnumerator<int> GetEnumerator() {
			for (var i = 0; i < _count; ++i) {
				yield return _start + i;
			}
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}




	// Add true/false operator[] to integer HashSet
	public sealed class IndexHashSet : HashSet<int>
	{
		public bool this[in int key]
		{
			get => Contains(key);
			set {
				if (value == true) {
					Add(key);
				}
				else if (value == false && Contains(key)) {
					Remove(key);
				}
			}
		}
	}




	/// <summary>
	/// This class provides a similar interface to BitArray, but can optionally
	/// use a HashSet (or perhaps some other DS) if the fraction of the index space 
	/// required is small
	/// </summary>
	public sealed class IndexFlagSet : IEnumerable<int>
	{
		readonly BitArray _bits;
		readonly HashSet<int> _hash;
		int _count;      // only tracked for bitset


		public IndexFlagSet(in bool bForceSparse, in int MaxIndex = -1) {
			if (bForceSparse) {
				_hash = new HashSet<int>();
			}
			else {
				_bits = new BitArray(MaxIndex);
			}
			_count = 0;
		}

		public IndexFlagSet(in int MaxIndex, in int SubsetCountEst) {
			var bSmall = MaxIndex < 128000;        // 16k in bits is a pretty small buffer?
			var fPercent = (float)SubsetCountEst / (float)MaxIndex;
			var fPercentThresh = 0.05f;

			if (bSmall || fPercent > fPercentThresh) {
				_bits = new BitArray(MaxIndex);
			}
			else {
				_hash = new HashSet<int>();
			}

			_count = 0;
		}

		/// <summary>
		/// checks if value i is true
		/// </summary>
		public bool Contains(in int i) {
			return this[i] == true;
		}

		/// <summary>
		/// sets value i to true
		/// </summary>
		public void Add(in int i) {
			this[i] = true;
		}

		/// <summary>
		/// Returns number of true values in set
		/// </summary>
		public int Count => _bits != null ? _count : _hash.Count;

		public bool this[in int key]
		{
			get => (_bits != null) ? _bits[key] : _hash.Contains(key);
			set {
				if (_bits != null) {
					if (_bits[key] != value) {
						_bits[key] = value;
						if (value == false) {
							_count--;
						}
						else {
							_count++;
						}
					}
				}
				else {
					if (value == true) {
						_hash.Add(key);
					}
					else if (value == false && _hash.Contains(key)) {
						_hash.Remove(key);
					}
				}
			}
		}

		/// <summary>
		/// enumerate over indices w/ value = true
		/// </summary>
		public IEnumerator<int> GetEnumerator() {
			if (_bits != null) {
				for (var i = 0; i < _bits.Length; ++i) {
					if (_bits[i]) {
						yield return i;
					}
				}
			}
			else {
				foreach (var i in _hash) {
					yield return i;
				}
			}
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}



	}



	// basic interface that allows mapping an index to another index
	public interface IIndexMap
	{
		int this[in int index] { get; }
	}


	// i = i index map
	public sealed class IdentityIndexMap : IIndexMap
	{
		public int this[in int index] => index;
	}


	// i = i + constant index map
	public sealed class ShiftIndexMap : IIndexMap
	{
		public int Shift;

		public ShiftIndexMap(in int n) {
			Shift = n;
		}

		public int this[in int index] => index + Shift;
	}


	// i = constant index map
	public sealed class ConstantIndexMap : IIndexMap
	{
		public int Constant;

		public ConstantIndexMap(in int c) {
			Constant = c;
		}

		public int this[in int index] => Constant;
	}



	// dense or sparse index map
	public sealed class IndexMap : IIndexMap
	{
		// this is returned if sparse map doesn't contain value
		public readonly int InvalidIndex = int.MinValue;
		readonly int[] _dense_map;
		readonly Dictionary<int, int> _sparse_map;
		readonly int _maxIndex;

		public IndexMap(in bool bForceSparse, in int MaxIndex = -1) {
			if (bForceSparse) {
				_sparse_map = new Dictionary<int, int>();
			}
			else {
				_dense_map = new int[MaxIndex];
			}
			_maxIndex = MaxIndex;
			SetToInvalid();
		}

		public IndexMap(in int[] use_dense_map, in int MaxIndex = -1) {
			_dense_map = use_dense_map;
			_maxIndex = MaxIndex;
		}


		public IndexMap(in int MaxIndex, in int SubsetCountEst) {
			var bSmall = MaxIndex < 32000;        // if buffer is less than 128k, just use dense map
			var fPercent = (float)SubsetCountEst / (float)MaxIndex;
			var fPercentThresh = 0.1f;

			if (bSmall || fPercent > fPercentThresh) {
				_dense_map = new int[MaxIndex];
			}
			else {
				_sparse_map = new Dictionary<int, int>();
			}
			_maxIndex = MaxIndex;
			SetToInvalid();
		}


		// no effect on sparse map
		public void SetToInvalid() {
			if (_dense_map != null) {
				for (var i = 0; i < _dense_map.Length; ++i) {
					_dense_map[i] = InvalidIndex;
				}
			}
		}


		// dense variant: returns true unless you have set index to InvalidIndex (eg via SetToInvalid)
		// sparse variant: returns true if index is in map
		// either: returns false if index is out-of-bounds
		public bool Contains(in int index) {
			return (_maxIndex <= 0 || index < _maxIndex) && (_dense_map != null ? _dense_map[index] != InvalidIndex : _sparse_map.ContainsKey(index));
		}



		public int this[in int index]
		{
			get => _dense_map != null ? _dense_map[index] : _sparse_map.TryGetValue(index, out var to) ? to : InvalidIndex;
			set {
				if (_dense_map != null) {
					_dense_map[index] = value;
				}
				else {
					_sparse_map[index] = value;
				}
			}
		}

	}



}
