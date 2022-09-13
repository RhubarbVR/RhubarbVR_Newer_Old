using System;
using System.Collections;

namespace RNumerics
{

	/// <summary>
	/// RefCountedVector is used to keep track of which indices in a linear index list are in use/referenced.
	/// A free list is tracked so that unreferenced indices can be re-used.
	///
	/// The enumerator iterates over valid indices (ie where refcount > 0)
	/// 
	/// **refcounts are shorts** so the maximum count is 65536. 
	/// No overflow checking is done in release builds.
	/// 
	/// </summary>
	public sealed class RefCountVector : System.Collections.IEnumerable
	{
		public static readonly short invalid = -1;
		DVector<int> _free_indices;

		public RefCountVector()
		{
			RawRefCounts = new DVector<short>();
			_free_indices = new DVector<int>();
			Count = 0;
		}

		public RefCountVector(in RefCountVector copy)
		{
			RawRefCounts = new DVector<short>(copy.RawRefCounts);
			_free_indices = new DVector<int>(copy._free_indices);
			Count = copy.Count;
		}

		public RefCountVector(in short[] raw_ref_counts, in bool build_free_list = false)
		{
			RawRefCounts = new DVector<short>(raw_ref_counts);
			_free_indices = new DVector<int>();
			Count = 0;
			if (build_free_list) {
				Rebuild_free_list();
			}
		}

		public DVector<short> RawRefCounts { get; }

		public bool Empty => Count == 0;
		public int Count { get; private set; }
		public int Max_index => RawRefCounts.Size;
		public bool Is_dense => _free_indices.Length == 0;


		public bool IsValid(in int index)
		{
			return index >= 0 && index < RawRefCounts.Size && RawRefCounts[index] > 0;
		}
		public bool IsValidUnsafe(in int index)
		{
			return RawRefCounts[index] > 0;
		}


		public int RefCount(in int index)
		{
			int n = RawRefCounts[index];
			return (n == invalid) ? 0 : n;
		}
		public int RawRefCount(in int index)
		{
			return RawRefCounts[index];
		}


		public int Allocate()
		{
			Count++;
			if (_free_indices.Empty)
			{
				// [RMS] do we need this branch anymore? 
				RawRefCounts.Push_back(1);
				return RawRefCounts.Size - 1;
			}
			else
			{
				int iFree = invalid;
				while (iFree == invalid && _free_indices.Empty == false)
				{
					iFree = _free_indices.Back;
					_free_indices.Pop_back();
				}
				if (iFree != invalid)
				{
					RawRefCounts[iFree] = 1;
					return iFree;
				}
				else
				{
					RawRefCounts.Push_back(1);
					return RawRefCounts.Size - 1;
				}
			}
		}



		public int Increment(in int index, in short increment = 1)
		{
			RawRefCounts[index] += increment;
			return RawRefCounts[index];
		}

		public void Decrement(in int index, in short decrement = 1)
		{
			RawRefCounts[index] -= decrement;
			if (RawRefCounts[index] == 0)
			{
				_free_indices.Push_back(index);
				RawRefCounts[index] = invalid;
				Count--;
			}
		}



		/// <summary>
		/// allocate at specific index, which must either be larger than current max index,
		/// or on the free list. If larger, all elements up to this one will be pushed onto
		/// free list. otherwise we have to do a linear search through free list.
		/// If you are doing many of these, it is likely faster to use 
		/// allocate_at_unsafe(), and then rebuild_free_list() after you are done.
		/// </summary>
		public bool Allocate_at(in int index)
		{
			if (index >= RawRefCounts.Size)
			{
				var j = RawRefCounts.Size;
				while (j < index)
				{
					RawRefCounts.Push_back(invalid);
					_free_indices.Push_back(j);
					++j;
				}
				RawRefCounts.Push_back(1);
				Count++;
				return true;

			}
			else
			{
				if (RawRefCounts[index] > 0) {
					return false;
				}

				var N = _free_indices.Size;
				for (var i = 0; i < N; ++i)
				{
					if (_free_indices[i] == index)
					{
						_free_indices[i] = invalid;
						RawRefCounts[index] = 1;
						Count++;
						return true;
					}
				}
				return false;
			}
		}


		/// <summary>
		/// allocate at specific index, which must be free or larger than current max index.
		/// However, we do not update free list. So, you probably need to do 
		/// rebuild_free_list() after calling this.
		/// </summary>
		public bool Allocate_at_unsafe(in int index)
		{
			if (index >= RawRefCounts.Size)
			{
				var j = RawRefCounts.Size;
				while (j < index)
				{
					RawRefCounts.Push_back(invalid);
					++j;
				}
				RawRefCounts.Push_back(1);
				Count++;
				return true;

			}
			else
			{
				if (RawRefCounts[index] > 0) {
					return false;
				}

				RawRefCounts[index] = 1;
				Count++;
				return true;
			}
		}



		// [RMS] really should not use this!!
		public void Set_Unsafe(in int index, in short count)
		{
			RawRefCounts[index] = count;
		}

		// todo:
		//   remove
		//   clear


		public void Rebuild_free_list()
		{
			_free_indices = new DVector<int>();
			Count = 0;

			var N = RawRefCounts.Length;
			for (var i = 0; i < N; ++i)
			{
				if (RawRefCounts[i] > 0) {
					Count++;
				}
				else {
					_free_indices.Add(i);
				}
			}
		}


		public void Trim(in int maxIndex)
		{
			_free_indices = new DVector<int>();
			RawRefCounts.Resize(maxIndex);
			Count = maxIndex;
		}




		public IEnumerator GetEnumerator()
		{
			var nIndex = 0;
			var nLast = Max_index;

			// skip leading empties
			while (nIndex != nLast && RawRefCounts[nIndex] <= 0) {
				nIndex++;
			}

			while (nIndex != nLast)
			{
				yield return nIndex;

				if (nIndex != nLast) {
					nIndex++;
				}

				while (nIndex != nLast && RawRefCounts[nIndex] <= 0) {
					nIndex++;
				}
			}
		}


		public string UsageStats => string.Format("RefCountSize {0}  FreeSize {1} FreeMem {2}kb", RawRefCounts.Size, _free_indices.Size, _free_indices.MemoryUsageBytes / 1024);
	}
}

