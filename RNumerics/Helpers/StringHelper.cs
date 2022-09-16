using System;
using System.Collections.Generic;
using System.Text;

namespace RNumerics
{
	public static class StringHelper
	{
		public static IEnumerable<string> GetArgStrings(this string str) {
			var section = "";
			foreach (var item in str) {
				if (item == ',') {
					yield return section;
					section = "";
				}
				else {
					if (item != '(') {
						section += item;
					}
				}
			}
		}
	}

}
