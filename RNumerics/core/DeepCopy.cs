using System;
using System.Collections.Generic;
using System.Linq;

namespace RNumerics
{

	/// <summary>
	/// Collection of utility functions for one-line deep copies of lists
	/// </summary>
	public static class DeepCopy
	{

		public static List<T> List<T>(in IEnumerable<T> Input) where T : IDuplicatable<T> {
			var result = new List<T>();
			foreach (var val in Input) {
				result.Add(val.Duplicate());
			}
			return result;
		}


		public static T[] Array<T>(in IEnumerable<T> Input) where T : IDuplicatable<T> {
			var count = Input.Count();
			var a = new T[count];
			var i = 0;
			foreach (var val in Input) {
				a[i++] = val.Duplicate();
			}

			return a;
		}

	}
}
