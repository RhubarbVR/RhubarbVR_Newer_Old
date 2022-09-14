using System;
using System.Collections.Generic;

namespace RNumerics
{
	/// <summary>
	/// Very basic object pool class. 
	/// </summary>
	public sealed class MemoryPool<T> where T : class, new()
	{
		DVector<T> _allocated;
		DVector<T> _free;

		public MemoryPool() {
			_allocated = new DVector<T>();
			_free = new DVector<T>();
		}

		public T Allocate() {
			if (_free.Size > 0) {
				var allocated = _free[_free.Size - 1];
				_free.Pop_back();
				return allocated;
			}
			else {
				var newval = new T();
				_allocated.Add(newval);
				return newval;
			}
		}

		public void Return(in T obj) {
			_free.Add(obj);
		}


		public void ReturnAll() {
			_free = new DVector<T>(_allocated);
		}


		public void FreeAll() {
			_allocated = new DVector<T>();
			_free = new DVector<T>();
		}

	}
}
