using System;
using System.Collections;
using System.Collections.Generic;

namespace RNumerics
{

	//
	// [RMS] AAAAHHHH usage of Blocks vs iCurBlock is not consistent!!
	//   - Should be supporting Capacity vs Size...
	//   - this[] operator does not check bounds, so it can write to any valid Block
	//   - some fns discard Blocks beyond iCurBlock
	//   - wtf...
	public class DVector<T> : IEnumerable<T>
	{
		List<T[]> _blocks;
		int _iCurBlock;
		int _iCurBlockUsed;
		const int N_SHIFT_BITS = 11;
		const int N_BLOCK_INDEX_BITMASK = 2047;   // low 11 bits

		public DVector() {
			_iCurBlock = 0;
			_iCurBlockUsed = 0;
			_blocks = new List<T[]> {
				new T[BlockCount]
			};
		}

		public DVector(DVector<T> copy) {
			BlockCount = copy.BlockCount;
			_iCurBlock = copy._iCurBlock;
			_iCurBlockUsed = copy._iCurBlockUsed;
			_blocks = new List<T[]>();
			for (var i = 0; i < copy._blocks.Count; ++i) {
				_blocks.Add(new T[BlockCount]);
				Array.Copy(copy._blocks[i], _blocks[i], copy._blocks[i].Length);
			}
		}

		public DVector(T[] data) {
			Initialize(data);
		}

		public DVector(IEnumerable<T> init) {
			_iCurBlock = 0;
			_iCurBlockUsed = 0;
			_blocks = new List<T[]> {
				new T[BlockCount]
			};
			foreach (var v in init) {
				Add(v);
			}
		}



		//public int Capacity {
		//    get { return Blocks.Count * nBlockSize;  }
		//}

		public int Length => (_iCurBlock * BlockCount) + _iCurBlockUsed;

		public int BlockCount { get; } = 2048;

		public int Size => Length;

		public bool Empty => _iCurBlock == 0 && _iCurBlockUsed == 0;

		public int MemoryUsageBytes => (_blocks.Count == 0) ? 0 : _blocks.Count * BlockCount * System.Runtime.InteropServices.Marshal.SizeOf(_blocks[0][0]);

		public void Add(T value) {
			if (_iCurBlockUsed == BlockCount) {
				if (_iCurBlock == _blocks.Count - 1) {
					_blocks.Add(new T[BlockCount]);
				}

				_iCurBlock++;
				_iCurBlockUsed = 0;
			}
			_blocks[_iCurBlock][_iCurBlockUsed] = value;
			_iCurBlockUsed++;
		}

		public void Add(T value, int nRepeat) {
			for (var i = 0; i < nRepeat; i++) {
				Add(value);
			}
		}

		public void Add(T[] values) {
			// TODO make this more efficient?
			for (var i = 0; i < values.Length; ++i) {
				Add(values[i]);
			}
		}

		public void Add(T[] values, int nRepeat) {
			for (var i = 0; i < nRepeat; i++) {
				for (var j = 0; j < values.Length; ++j) {
					Add(values[j]);
				}
			}
		}


		public void Push_back(T value) {
			Add(value);
		}
		public void Pop_back() {
			if (_iCurBlockUsed > 0) {
				_iCurBlockUsed--;
			}

			if (_iCurBlockUsed == 0 && _iCurBlock > 0) {
				_iCurBlock--;
				_iCurBlockUsed = BlockCount;

				// remove block ??
			}
		}

		public void Insert(T value, int index) {
			InsertAt(value, index);
		}
		public void InsertAt(T value, int index) {
			var s = Size;
			if (index == s) {
				Push_back(value);
			}
			else if (index > s) {
				Resize(index);
				Push_back(value);
			}
			else {
				this[index] = value;
			}
		}


		public void Resize(int count) {
			if (Length == count) {
				return;
			}

			// figure out how many segments we need
			var nNumSegs = 1 + ((int)count / BlockCount);

			// figure out how many are currently allocated...
			var nCurCount = _blocks.Count;

			// erase extra segments memory
			for (var i = nNumSegs; i < nCurCount; ++i) {
				_blocks[i] = null;
			}

			// resize to right number of segments
			if (nNumSegs >= _blocks.Count) {
				_blocks.Capacity = nNumSegs;
			}
			else {
				_blocks.RemoveRange(nNumSegs, _blocks.Count - nNumSegs);
			}

			// allocate new segments
			for (var i = (int)nCurCount; i < nNumSegs; ++i) {
				_blocks.Add(new T[BlockCount]);
			}

			// mark last segment
			_iCurBlockUsed = count - ((nNumSegs - 1) * BlockCount);

			_iCurBlock = nNumSegs - 1;
		}


		public void Copy(DVector<T> copyIn) {
			if (_blocks != null && copyIn._blocks.Count == _blocks.Count) {
				var N = copyIn._blocks.Count;
				for (var k = 0; k < N; ++k) {
					Array.Copy(copyIn._blocks[k], _blocks[k], copyIn._blocks[k].Length);
				}

				_iCurBlock = copyIn._iCurBlock;
				_iCurBlockUsed = copyIn._iCurBlockUsed;
			}
			else {
				Resize(copyIn.Size);
				var N = copyIn._blocks.Count;
				for (var k = 0; k < N; ++k) {
					Array.Copy(copyIn._blocks[k], _blocks[k], copyIn._blocks[k].Length);
				}

				_iCurBlock = copyIn._iCurBlock;
				_iCurBlockUsed = copyIn._iCurBlockUsed;
			}
		}



		public T this[int i]
		{
			// [RMS] bit-shifts here are significantly faster
			get =>
				//int bi = i / nBlockSize;
				//return Blocks[bi][i - (bi * nBlockSize)];
				//int bi = i >> nShiftBits;
				//return Blocks[bi][i - (bi << nShiftBits)];
				_blocks[i >> N_SHIFT_BITS][i & N_BLOCK_INDEX_BITMASK];
			set =>
				//int bi = i / nBlockSize;
				//Blocks[bi][i - (bi * nBlockSize)] = value;
				//int bi = i >> nShiftBits;
				//Blocks[bi][i - (bi << nShiftBits)] = value;
				_blocks[i >> N_SHIFT_BITS][i & N_BLOCK_INDEX_BITMASK] = value;
		}


		public T Back
		{
			get => _blocks[_iCurBlock][_iCurBlockUsed - 1];
			set => _blocks[_iCurBlock][_iCurBlockUsed - 1] = value;
		}
		public T Front
		{
			get => _blocks[0][0];
			set => _blocks[0][0] = value;
		}



		// TODO: 
		//   - iterate through blocks in above to avoid div/mod for each element
		//   - provide function that takes lambda?


		// [RMS] slowest option, but only one that is completely generic
		public void GetBuffer(T[] data) {
			var nLen = Length;
			for (var k = 0; k < nLen; ++k) {
				data[k] = this[k];
			}
		}
		public T[] GetBuffer()      // todo: deprecate this...
		{
			var data = new T[Length];
			for (var k = 0; k < Length; ++k) {
				data[k] = this[k];
			}

			return data;
		}
		public T[] ToArray() {
			return GetBuffer();
		}

		// warning: this may be quite slow!
		public T2[] GetBufferCast<T2>() {
			var data = new T2[Length];
			for (var k = 0; k < Length; ++k) {
				data[k] = (T2)Convert.ChangeType(this[k], typeof(T2));
			}

			return data;
		}


		public byte[] GetBytes() {
			var type = typeof(T);
			var n = System.Runtime.InteropServices.Marshal.SizeOf(type);
			var buffer = new byte[Length * n];
			var i = 0;
			var N = _blocks.Count;
			for (var k = 0; k < N - 1; ++k) {
				Buffer.BlockCopy(_blocks[k], 0, buffer, i, BlockCount * n);
				i += BlockCount * n;
			}
			Buffer.BlockCopy(_blocks[N - 1], 0, buffer, i, _iCurBlockUsed * n);
			return buffer;
		}



		public void Initialize(T[] data) {
			var blocks = data.Length / BlockCount;
			_blocks = new List<T[]>();
			var ai = 0;
			for (var i = 0; i < blocks; ++i) {
				var block = new T[BlockCount];
				Array.Copy(data, ai, block, 0, BlockCount);
				_blocks.Add(block);
				ai += BlockCount;
			}
			_iCurBlockUsed = data.Length - ai;
			if (_iCurBlockUsed != 0) {
				var last = new T[BlockCount];
				Array.Copy(data, ai, last, 0, _iCurBlockUsed);
				_blocks.Add(last);
			}
			else {
				_iCurBlockUsed = BlockCount;
			}
			_iCurBlock = _blocks.Count - 1;
		}




		/// <summary>
		/// Calls Array.Clear() on each block, which sets value to 'default' for type
		/// </summary>
		public void Clear() {
			foreach (var block in _blocks) {
				Array.Clear(block, 0, block.Length);
			}
		}


		/// <summary>
		/// Apply action to each element of vector. Iterates by block so this is more efficient.
		/// </summary>
		public void Apply(Action<T, int> applyF) {
			for (var bi = 0; bi < _iCurBlock; ++bi) {
				var block = _blocks[bi];
				for (var k = 0; k < BlockCount; ++k) {
					applyF(block[k], k);
				}
			}
			var lastblock = _blocks[_iCurBlock];
			for (var k = 0; k < _iCurBlockUsed; ++k) {
				applyF(lastblock[k], k);
			}
		}


		/// <summary>
		/// set vec[i] = applyF(vec[i], i) for each element of vector
		/// </summary>
		public void ApplyReplace(Func<T, int, T> applyF) {
			for (var bi = 0; bi < _iCurBlock; ++bi) {
				var block = _blocks[bi];
				for (var k = 0; k < BlockCount; ++k) {
					block[k] = applyF(block[k], k);
				}
			}
			var lastblock = _blocks[_iCurBlock];
			for (var k = 0; k < _iCurBlockUsed; ++k) {
				lastblock[k] = applyF(lastblock[k], k);
			}
		}




		/*
         * [RMS] C# resolves generics at compile-type, so we cannot call an overloaded
         *   function based on the generic type. Hence, we have these static helpers for
         *   common cases...
         */

		public static unsafe void FastGetBuffer(DVector<double> v, double* pBuffer) {
			var pCur = new IntPtr(pBuffer);
			var N = v._blocks.Count;
			for (var k = 0; k < N - 1; k++) {
				System.Runtime.InteropServices.Marshal.Copy(v._blocks[k], 0, pCur, v.BlockCount);
				pCur = new IntPtr(
					pCur.ToInt64() + (v.BlockCount * sizeof(double)));
			}
			System.Runtime.InteropServices.Marshal.Copy(v._blocks[N - 1], 0, pCur, v._iCurBlockUsed);
		}
		public static unsafe void FastGetBuffer(DVector<float> v, float* pBuffer) {
			var pCur = new IntPtr(pBuffer);
			var N = v._blocks.Count;
			for (var k = 0; k < N - 1; k++) {
				System.Runtime.InteropServices.Marshal.Copy(v._blocks[k], 0, pCur, v.BlockCount);
				pCur = new IntPtr(
					pCur.ToInt64() + (v.BlockCount * sizeof(float)));
			}
			System.Runtime.InteropServices.Marshal.Copy(v._blocks[N - 1], 0, pCur, v._iCurBlockUsed);
		}
		public static unsafe void FastGetBuffer(DVector<int> v, int* pBuffer) {
			var pCur = new IntPtr(pBuffer);
			var N = v._blocks.Count;
			for (var k = 0; k < N - 1; k++) {
				System.Runtime.InteropServices.Marshal.Copy(v._blocks[k], 0, pCur, v.BlockCount);
				pCur = new IntPtr(
					pCur.ToInt64() + (v.BlockCount * sizeof(int)));
			}
			System.Runtime.InteropServices.Marshal.Copy(v._blocks[N - 1], 0, pCur, v._iCurBlockUsed);
		}



		public IEnumerator<T> GetEnumerator() {
			for (var bi = 0; bi < _iCurBlock; ++bi) {
				var block = _blocks[bi];
				for (var k = 0; k < BlockCount; ++k) {
					yield return block[k];
				}
			}
			var lastblock = _blocks[_iCurBlock];
			for (var k = 0; k < _iCurBlockUsed; ++k) {
				yield return lastblock[k];
			}
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}



		// block iterator
		public struct DBlock
		{
			public T[] data;
			public int usedCount;
		}
		public IEnumerable<DBlock> BlockIterator() {
			for (var i = 0; i < _iCurBlock; ++i) {
				yield return new DBlock() { data = _blocks[i], usedCount = BlockCount };
			}

			yield return new DBlock() { data = _blocks[_iCurBlock], usedCount = _iCurBlockUsed };
		}

	}
}
