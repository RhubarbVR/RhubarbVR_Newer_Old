using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RhuEngine.Linker
{
	public static class RWorld
	{

		public static Dictionary<object,Action> MainExecute = new();

		public static bool IsInVR { get; internal set; } = false;

		public static void ExecuteOnMain(object target,Action p) {
			lock (MainExecute) {
				if (!MainExecute.ContainsKey(target)) {
					MainExecute.Add(target, p);
				}
				else {
					MainExecute.Remove(target);
					MainExecute.Add(target,p);
				}
			}
		}

		public static void RunOnMain() {
			lock (MainExecute) {
				foreach (var item in MainExecute.ToArray()) {
					item.Value.Invoke();
				}
				MainExecute.Clear();
			}
		}
	}
}
