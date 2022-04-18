using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RhuEngine.Linker
{
	public static class RWorld
	{
		public static bool IsInVR { get; internal set; } = false;

		public static Dictionary<object,Action> StartOfFrameExecute = new();
		public static List<Action> StartOfFrameList = new();

		public static void ExecuteOnStartOfFrame(Action p) {
			lock (StartOfFrameExecute) {
				StartOfFrameList.Add(p);
			}
		}

		public static void ExecuteOnStartOfFrame(object target,Action p) {
			lock (StartOfFrameExecute) {
				if (!StartOfFrameExecute.ContainsKey(target)) {
					StartOfFrameExecute.Add(target, p);
				}
				else {
					StartOfFrameExecute.Remove(target);
					StartOfFrameExecute.Add(target,p);
				}
			}
		}

		public static void RunOnStartOfFrame() {
			lock (StartOfFrameExecute) {
				foreach (var item in StartOfFrameExecute.ToArray()) {
					item.Value.Invoke();
				}
				foreach (var item in StartOfFrameList.ToArray()) {
					item.Invoke();
				}
				StartOfFrameList.Clear();
				StartOfFrameExecute.Clear();
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
				foreach (var item in EndOfFrameExecute.ToArray()) {
					item.Value.Invoke();
				}
				foreach (var item in EndOfFrameList.ToArray()) {
					item.Invoke();
				}
				EndOfFrameList.Clear();
				EndOfFrameExecute.Clear();
			}
		}

	}
}
