using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using StereoKit;

namespace RhuEngine
{
	public static class Helper
	{
		public static string CleanPath(this string path) {
			var regexSearch = new string(Path.GetInvalidPathChars());
			var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
			return r.Replace(path, "");
		}
		
		public static string TouchUpPath(this string path) {
			return path.Replace("\\", "/");
		}

		public static Matrix RotNormalized(this Matrix oldmatrix) {
			oldmatrix.Decompose(out var trans, out var rot, out var scale);
			return Matrix.TRS(trans, rot, scale);
		}

		public static Matrix GetLocal(this Matrix global, Matrix newglobal) {
			return newglobal * global.Inverse;
		}

		public static string GetFormattedName(this Type type) {
			if (type.IsGenericType) {
				var genericArguments = type.GetGenericArguments()
									.Select(x => x.Name)
									.Aggregate((x1, x2) => $"{x1}, {x2}");
				return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}"
					 + $" <{genericArguments}>";
			}
			return type.Name;
		}

	}
}
