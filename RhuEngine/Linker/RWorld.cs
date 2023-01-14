using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RhuEngine.Linker
{
	public static class RUpdateManager
	{
		public static ulong UpdateCount { get; private set; }

		public static readonly Dictionary<object, Action> StartOfFrameExecute = new();
		public static readonly List<Action> StartOfFrameList = new();
		[ThreadStatic]
		public static bool isStartOfUpdate;

		public static void ExecuteOnStartOfUpdate(Action p) {
			lock (StartOfFrameExecute) {
				if (isStartOfUpdate) {
					p();
				}
				else {
					StartOfFrameList.Add(p);
				}
			}
		}

		public static void ExecuteOnStartOfUpdate(object target, Action p) {
			lock (StartOfFrameExecute) {
				if (isStartOfUpdate) {
					p();
				}
				else {
					if (!StartOfFrameExecute.ContainsKey(target)) {
						StartOfFrameExecute.Add(target, p);
					}
					else {
						StartOfFrameExecute.Remove(target);
						StartOfFrameExecute.Add(target, p);
					}
				}
			}
		}

		public static void RunOnStartOfUpdate() {
			lock (StartOfFrameExecute) {
				isStartOfUpdate = true;
				var startcountlist = StartOfFrameList.Count;
				for (var i = 0; i < startcountlist; i++) {
					try {
						StartOfFrameList[0].Invoke();
					}
					catch { }
					StartOfFrameList.RemoveAt(0);
				}
				var dectionaryvalues = StartOfFrameExecute.ToArray();
				for (var i = 0; i < dectionaryvalues.Length; i++) {
					var currentobj = dectionaryvalues[i];
					try {
						currentobj.Value.Invoke();
					}
					catch { }
					StartOfFrameExecute.Remove(currentobj.Key);
				}
				isStartOfUpdate = false;
			}
		}


		public static readonly Dictionary<object, Action> EndOfFrameExecute = new();
		public static readonly List<Action> EndOfFrameList = new();
		[ThreadStatic]
		public static bool isEndOfFrame;

		public static void ExecuteOnEndOfUpdate(Action p) {
			lock (EndOfFrameExecute) {
				if (isEndOfFrame) {
					p();
				}
				else {
					EndOfFrameList.Add(p);
				}
			}
		}

		public static void ExecuteOnEndOfUpdate(object target, Action p) {
			lock (EndOfFrameExecute) {
				if (isEndOfFrame) {
					p();
				}
				else {
					if (!EndOfFrameExecute.ContainsKey(target)) {
						EndOfFrameExecute.Add(target, p);
					}
					else {
						EndOfFrameExecute.Remove(target);
						EndOfFrameExecute.Add(target, p);
					}
				}
			}
		}

		public static void RunOnEndOfUpdate() {
			lock (EndOfFrameExecute) {
				isEndOfFrame = true;
				UpdateCount++;
				var startcountlist = EndOfFrameList.Count;
				for (var i = 0; i < startcountlist; i++) {
					try {
						EndOfFrameList[0].Invoke();
					}
					catch { }
					EndOfFrameList.RemoveAt(0);
				}
				var dectionaryvalues = EndOfFrameExecute.ToArray();
				for (var i = 0; i < dectionaryvalues.Length; i++) {
					var currentobj = dectionaryvalues[i];
					try {
						currentobj.Value.Invoke();
					}
					catch { }
					EndOfFrameExecute.Remove(currentobj.Key);
				}
				isEndOfFrame = false;
			}
		}


	}
}
