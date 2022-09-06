using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient
{
	public static class FileSizeFormatter
	{
		static readonly string[] _suffixes =
		{ "Bytes", "KB", "MB", "GB", "TB", "PB" };
		public static string FormatSize(long bytes) {
			var counter = 0;
			var number = (decimal)bytes;
			while (Math.Round(number / 1024) >= 1) {
				number /= 1024;
				counter++;
			}
			return string.Format("{0:n2}{1}", number, _suffixes[counter]);
		}
	}
}
