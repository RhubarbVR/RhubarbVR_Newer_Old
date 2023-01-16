using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RhuEngine.Linker;

namespace RhuEngine
{
	public static class RenderThread
	{
		public static ulong UpdateCount { get; private set; }

		public static readonly Dictionary<object, Action> StartOfFrameExecute = new();
		public static readonly List<Action> StartOfFrameList = new();
		[ThreadStatic]
		public static bool isStartOfFrame;

		public static void ExecuteOnStartOfFrame(Action p) {
			lock (StartOfFrameExecute) {
				if (isStartOfFrame) {
					p();
				}
				else {
					StartOfFrameList.Add(p);
				}
			}
		}

		public static void ExecuteOnStartOfFrame(object target, Action p) {
			lock (StartOfFrameExecute) {
				if (isStartOfFrame) {
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

		public static void RunOnStartOfFrame() {
			lock (StartOfFrameExecute) {
				isStartOfFrame = true;
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
				isStartOfFrame = false;
			}
		}


		public static readonly Dictionary<object, Action> EndOfFrameExecute = new();
		public static readonly List<Action> EndOfFrameList = new();
		[ThreadStatic]
		public static bool isEndOfFrame;


		public static void ExecuteOnEndOfFrameNoPass(Action p) {
			lock (EndOfFrameExecute) {
				EndOfFrameList.Add(p);
			}
		}

		public static void ExecuteOnEndOfFrame(Action p) {
			lock (EndOfFrameExecute) {
				if (isEndOfFrame) {
					p();
				}
				else {
					EndOfFrameList.Add(p);
				}
			}
		}

		public static void ExecuteOnEndOfFrame(object target, Action p) {
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

		public static void RunOnEndOfFrame() {
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
