using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using RhuEngine.Linker;
using System.Threading.Tasks;

namespace RhuEngine
{
	public static class TempFiles
	{
		public static readonly List<string> TempFile = new();

		public static void AddTempFile(string path) {
			lock (TempFile) {
				TempFile.Add(path);
			}
		}

		public static void CleanUpTempFiles() {
			lock (TempFile) {
				foreach (var item in TempFile) {
					try {
						File.Delete(item);
					}
					catch {
					}
				}
				TempFile.Clear();
			}
		}
	}
}
