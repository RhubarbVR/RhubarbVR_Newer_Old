using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RNumerics
{
	public static class EnumratorHelper
	{
		public static IEnumerable<T> LimitSelect<T>(this IEnumerable<T> ts, int limmit) {
			var counter = 0;
			foreach (var item in ts) {
				yield return item;
				counter++;
				if (counter >= limmit) {
					yield break;
				}
			}
		}

	}

}
