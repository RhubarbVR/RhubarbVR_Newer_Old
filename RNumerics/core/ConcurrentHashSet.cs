using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RNumerics
{
	public class ConcurrentHashSet<T> : IDisposable, IEnumerable<T>
	{
		private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
		private readonly HashSet<T> _hashSet = new();

		public IEnumerator<T> GetEnumerator() {
			_lock.EnterReadLock();
			try {
				return _hashSet.GetEnumerator();
			}
			finally {
				if (_lock.IsReadLockHeld) {
					_lock.ExitReadLock();
				}
			}
		}

		public bool Add(T item) {
			_lock.EnterWriteLock();
			try {
				return _hashSet.Add(item);
			}
			finally {
				if (_lock.IsWriteLockHeld) {
					_lock.ExitWriteLock();
				}
			}
		}

		public void Clear() {
			_lock.EnterWriteLock();
			try {
				_hashSet.Clear();
			}
			finally {
				if (_lock.IsWriteLockHeld) {
					_lock.ExitWriteLock();
				}
			}
		}

		public bool Contains(T item) {
			_lock.EnterReadLock();
			try {
				return _hashSet.Contains(item);
			}
			finally {
				if (_lock.IsReadLockHeld) {
					_lock.ExitReadLock();
				}
			}
		}

		public bool Remove(T item) {
			_lock.EnterWriteLock();
			try {
				return _hashSet.Remove(item);
			}
			finally {
				if (_lock.IsWriteLockHeld) {
					_lock.ExitWriteLock();
				}
			}
		}

		public int Count
		{
			get {
				_lock.EnterReadLock();
				try {
					return _hashSet.Count;
				}
				finally {
					if (_lock.IsReadLockHeld) {
						_lock.ExitReadLock();
					}
				}
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				_lock?.Dispose();
			}
		}


		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		~ConcurrentHashSet() {
			Dispose(false);
		}
	}
}
