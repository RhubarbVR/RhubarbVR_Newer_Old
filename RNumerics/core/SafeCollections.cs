using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RNumerics
{

	/// <summary>
	/// A simple wrapper around a List<T> that supports multi-threaded construction.
	/// Basically intended for use within things like a Parallel.ForEach
	/// </summary>
	public class SafeListBuilder<T>
	{
		public List<T> List;
		public SpinLock spinlock;

		public SafeListBuilder() {
			List = new List<T>();
			spinlock = new SpinLock();
		}

		public void SafeAdd(T value) {
			var lockTaken = false;
			while (lockTaken == false) {
				spinlock.Enter(ref lockTaken);
			}

			List.Add(value);

			spinlock.Exit();
		}


		public void SafeOperation(Action<List<T>> opF) {
			var lockTaken = false;
			while (lockTaken == false) {
				spinlock.Enter(ref lockTaken);
			}

			opF(List);

			spinlock.Exit();
		}


		public List<T> Result => List;
	}


}
