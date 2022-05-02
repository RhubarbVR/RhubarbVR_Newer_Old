using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	/// <summary>
	/// SparseList provides a linear-indexing interface, but internally may use an
	/// alternate data structure to store the [index,value] pairs, if the list
	/// is very sparse. 
	/// 
	/// Currently uses Dictionary<> as sparse data structure
	/// </summary>
	public class SparseList<T> where T : IEquatable<T>
	{
		readonly T[] _dense;
		readonly Dictionary<int, T> _sparse;
		readonly T _zeroValue;

		public SparseList(int MaxIndex, int SubsetCountEst, T ZeroValue) {
			_zeroValue = ZeroValue;

			var bSmall = MaxIndex is > 0 and < 1024;
			var fPercent = (MaxIndex == 0) ? 0 : SubsetCountEst / (float)MaxIndex;
			var fPercentThresh = 0.1f;

			if (bSmall || fPercent > fPercentThresh) {
				_dense = new T[MaxIndex];
				for (var k = 0; k < MaxIndex; ++k) {
					_dense[k] = ZeroValue;
				}
			}
			else {
				_sparse = new Dictionary<int, T>();
			}
		}


		public T this[int idx]
		{
			get => _dense != null ? _dense[idx] : _sparse.TryGetValue(idx, out var val) ? val : _zeroValue;
			set {
				if (_dense != null) {
					_dense[idx] = value;
				}
				else {
					_sparse[idx] = value;
				}
			}
		}


		public int Count(Func<T, bool> CountF) {
			var count = 0;
			if (_dense != null) {
				for (var i = 0; i < _dense.Length; ++i) {
					if (CountF(_dense[i])) {
						count++;
					}
				}
			}
			else {
				foreach (var v in _sparse) {
					if (CountF(v.Value)) {
						count++;
					}
				}
			}
			return count;
		}


		/// <summary>
		/// This enumeration will return pairs [index,0] for dense case
		/// </summary>
		public IEnumerable<KeyValuePair<int, T>> Values() {
			if (_dense != null) {
				for (var i = 0; i < _dense.Length; ++i) {
					yield return new KeyValuePair<int, T>(i, _dense[i]);
				}
			}
			else {
				foreach (var v in _sparse) {
					yield return v;
				}
			}
		}


		public IEnumerable<KeyValuePair<int, T>> NonZeroValues() {
			if (_dense != null) {
				for (var i = 0; i < _dense.Length; ++i) {
					if (_dense[i].Equals(_zeroValue) == false) {
						yield return new KeyValuePair<int, T>(i, _dense[i]);
					}
				}
			}
			else {
				foreach (var v in _sparse) {
					yield return v;
				}
			}
		}
	}










	/// <summary>
	/// variant of SparseList for class objects, then "zero" is null
	/// 
	/// TODO: can we combine these classes somehow?
	/// </summary>
	public class SparseObjectList<T> where T : class
	{
		readonly T[] _dense;
		readonly Dictionary<int, T> _sparse;

		public SparseObjectList(int MaxIndex, int SubsetCountEst) {
			var bSmall = MaxIndex < 1024;
			var fPercent = SubsetCountEst / (float)MaxIndex;
			var fPercentThresh = 0.1f;

			if (bSmall || fPercent > fPercentThresh) {
				_dense = new T[MaxIndex];
				for (var k = 0; k < MaxIndex; ++k) {
					_dense[k] = null;
				}
			}
			else {
				_sparse = new Dictionary<int, T>();
			}
		}


		public T this[int idx]
		{
			get => _dense != null ? _dense[idx] : _sparse.TryGetValue(idx, out var val) ? val : null;
			set {
				if (_dense != null) {
					_dense[idx] = value;
				}
				else {
					_sparse[idx] = value;
				}
			}
		}


		public int Count(Func<T, bool> CountF) {
			var count = 0;
			if (_dense != null) {
				for (var i = 0; i < _dense.Length; ++i) {
					if (CountF(_dense[i])) {
						count++;
					}
				}
			}
			else {
				foreach (var v in _sparse) {
					if (CountF(v.Value)) {
						count++;
					}
				}
			}
			return count;
		}


		/// <summary>
		/// This enumeration will return pairs [index,0] for dense case
		/// </summary>
		public IEnumerable<KeyValuePair<int, T>> Values() {
			if (_dense != null) {
				for (var i = 0; i < _dense.Length; ++i) {
					yield return new KeyValuePair<int, T>(i, _dense[i]);
				}
			}
			else {
				foreach (var v in _sparse) {
					yield return v;
				}
			}
		}


		public IEnumerable<KeyValuePair<int, T>> NonZeroValues() {
			if (_dense != null) {
				for (var i = 0; i < _dense.Length; ++i) {
					if (_dense[i] != null) {
						yield return new KeyValuePair<int, T>(i, _dense[i]);
					}
				}
			}
			else {
				foreach (var v in _sparse) {
					yield return v;
				}
			}
		}


		public void Clear() {
			if (_dense != null) {
				Array.Clear(_dense, 0, _dense.Length);
			}
			else {
				_sparse.Clear();
			}
		}

	}
}
