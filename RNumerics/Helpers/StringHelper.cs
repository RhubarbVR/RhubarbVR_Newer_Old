using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RNumerics
{
	public static class StringHelper
	{
		public static string[] ParseExact(this string data, string format) {
			return ParseExact(data, format, false);
		}

		public static string[] ParseExact(
			this string data,
			string format,
			bool ignoreCase) {

			return TryParseExact(data, format, out var values, ignoreCase)
				? values
				: throw new ArgumentException("Format not compatible with value.");
		}

		public static bool TryExtract(this string data, string format, out string[] values) {
			return TryParseExact(data, format, out values, false);
		}

		public static bool TryParseExact(
			this string data,
			string format,
			out string[] values,
			bool ignoreCase) {
			format = Regex.Escape(format).Replace("\\{", "{");

			int tokenCount;
			for (tokenCount = 0; ; tokenCount++) {
				var token = string.Format("{{{0}}}", tokenCount);
				if (!format.Contains(token)) {
					break;
				}
				format = format.Replace(token,
					string.Format("(?'group{0}'.*)", tokenCount));
			}

			var options =
				ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

			var match = new Regex(format, options).Match(data);

			if (tokenCount != (match.Groups.Count - 1)) {
				values = Array.Empty<string>();
				return false;
			}
			else {
				values = new string[tokenCount];
				for (var index = 0; index < tokenCount; index++) {
					values[index] =
						match.Groups[string.Format("group{0}", index)].Value;
				}

				return true;
			}
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
