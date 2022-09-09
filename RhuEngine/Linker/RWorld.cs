using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RhuEngine.Linker
{
	public static class RUpdateManager
	{
		public static Dictionary<object, Action> StartOfFrameExecute = new();
		public static List<Action> StartOfFrameList = new();

		public static void ExecuteOnStartOfFrame(Action p) {
			lock (StartOfFrameExecute) {
				StartOfFrameList.Add(p);
			}
		}

		public static void ExecuteOnStartOfFrame(object target, Action p) {
			lock (StartOfFrameExecute) {
				if (!StartOfFrameExecute.ContainsKey(target)) {
					StartOfFrameExecute.Add(target, p);
				}
				else {
					StartOfFrameExecute.Remove(target);
					StartOfFrameExecute.Add(target, p);
				}
			}
		}

		public static void RunOnStartOfFrame() {
			lock (StartOfFrameExecute) {
				var startcountlist = StartOfFrameList.Count;
				for (var i = 0; i < startcountlist; i++) {
					StartOfFrameList[0].Invoke();
					StartOfFrameList.RemoveAt(0);
				}
				var dectionaryvalues = StartOfFrameExecute.ToArray();
				for (var i = 0; i < dectionaryvalues.Length; i++) {
					var currentobj = dectionaryvalues[i];
					currentobj.Value.Invoke();
					StartOfFrameExecute.Remove(currentobj.Key);
				}
			}
		}


		public static Dictionary<object, Action> EndOfFrameExecute = new();
		public static List<Action> EndOfFrameList = new();

		public static void ExecuteOnEndOfFrame(Action p) {
			lock (EndOfFrameExecute) {
				EndOfFrameList.Add(p);
			}
		}

		public static void ExecuteOnEndOfFrame(object target, Action p) {
			lock (EndOfFrameExecute) {
				if (!EndOfFrameExecute.ContainsKey(target)) {
					EndOfFrameExecute.Add(target, p);
				}
				else {
					EndOfFrameExecute.Remove(target);
					EndOfFrameExecute.Add(target, p);
				}
			}
		}

		public static void RunOnEndOfFrame() {
			lock (EndOfFrameExecute) {
				var startcountlist = EndOfFrameList.Count;
				for (var i = 0; i < startcountlist; i++) {
					EndOfFrameList[0].Invoke();
					EndOfFrameList.RemoveAt(0);
				}
				var dectionaryvalues = EndOfFrameExecute.ToArray();
				for (var i = 0; i < dectionaryvalues.Length; i++) {
					var currentobj = dectionaryvalues[i];
					currentobj.Value.Invoke();
					EndOfFrameExecute.Remove(currentobj.Key);
				}
			}
		}

	}
}
