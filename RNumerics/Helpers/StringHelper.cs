using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RNumerics
{
	public static class StringHelper
	{

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int Minimum(int a, int b) {
			return a < b ? a : b;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int Minimum(int a, int b, int c) {
			return (a = a < b ? a : b) < c ? a : c;
		}
		public static IEnumerable<string> OrderByStingDistancesLimited(this IEnumerable<string> strings, string searchSting, int amountOfResults = 25) {
			return strings.OrderBy(x => x.CommonStringDistances(searchSting)).LimitSelect(amountOfResults);
		}

		public static IEnumerable<string> OrderByStingDistances(this IEnumerable<string> strings, string searchSting) {
			return strings.OrderBy(x => x.CommonStringDistances(searchSting));
		}

		public static int CommonStringDistances(this string firstText, string secondText) {
			return DamerauLevenshteinDistance(firstText.ToLower(), secondText.ToLower()) - (firstText.ToLower().Contains(secondText.ToLower()) ? 10 : 0) - (secondText.ToLower().Contains(firstText.ToLower()) ? 2 : 0);
		}

		public static int DamerauLevenshteinDistance(this string firstText, string secondText) {
			var n = firstText.Length + 1;
			var m = secondText.Length + 1;
			var arrayD = (Span<int>)stackalloc int[n * m];

			for (var i = 0; i < n; i++) {
				arrayD[i] = i;
			}

			for (var j = 0; j < m; j++) {
				arrayD[j * n] = j;
			}

			for (var i = 1; i < n; i++) {
				for (var j = 1; j < m; j++) {
					var cost = firstText[i - 1] == secondText[j - 1] ? 0 : 1;

					arrayD[i + (j * n)] = Minimum(arrayD[i - 1 + (j * n)] + 1, // delete
										   arrayD[i + ((j - 1) * n)] + 1, // insert
										   arrayD[i - 1 + ((j - 1) * n)] + cost); // replacement

					if (i > 1 && j > 1
					   && firstText[i - 1] == secondText[j - 2]
					   && firstText[i - 2] == secondText[j - 1]) {
						arrayD[i + (j * n)] = Minimum(arrayD[i + (j * n)],
						arrayD[i - 2 + ((j - 2) * n)] + cost); // permutation
					}
				}
			}

			return arrayD[n - 1 + ((m - 1) * n)];
		}

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
