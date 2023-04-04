using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

using RhuEngine.Linker;

namespace RhuEngine
{
	public static class RenderThread
	{
		public static ulong UpdateCount { get; private set; }

		public static readonly ConcurrentDictionary<object, Action> StartOfFrameExecute = new();
		public static readonly ConcurrentQueue<Action> StartOfFrame = new();
		[ThreadStatic]
		public static bool isStartOfFrame;

		public static void ExecuteOnStartOfFrame(Action p) {
			if (isStartOfFrame) {
				p();
			}
			else {
				StartOfFrame.Enqueue(p);
			}
		}

		public static void ExecuteOnStartOfFrame(object target, Action p) {
			if (isStartOfFrame) {
				p();
			}
			else {
				StartOfFrameExecute.AddOrUpdate(target, (_) => p, (_, _) => p);
			}
		}

		public static void RunOnStartOfFrame() {
			isStartOfFrame = true;
			while (StartOfFrame.TryDequeue(out var frame)) {
				try {
					frame();
				}
				catch (Exception e) {
					RLog.Err($"RunOnStartOfFrame Error: {e}");
				}
			}
			var dectionaryvalues = StartOfFrameExecute.ToArray();
			for (var i = 0; i < dectionaryvalues.Length; i++) {
				var currentobj = dectionaryvalues[i];
				try {
					currentobj.Value.Invoke();
				}
				catch (Exception e) {
					RLog.Err($"RunOnStartOfFrame Dictionary Target {currentobj.Key} Error: {e}");
				}
				StartOfFrameExecute.TryRemove(currentobj);
			}
			isStartOfFrame = false;
		}


		public static readonly ConcurrentDictionary<object, Action> EndOfFrameExecute = new();
		public static readonly ConcurrentQueue<Action> EndOfFrameList = new();
		[ThreadStatic]
		public static bool isEndOfFrame;


		public static void ExecuteOnEndOfFrameNoPass(Action p) {
			EndOfFrameList.Enqueue(p);
		}

		public static void ExecuteOnEndOfFrame(Action p) {
			if (isEndOfFrame) {
				p();
			}
			else {
				EndOfFrameList.Enqueue(p);
			}
		}

		public static void ExecuteOnEndOfFrame(object target, Action p) {
			if (isEndOfFrame) {
				p();
			}
			else {
				EndOfFrameExecute.AddOrUpdate(target, (_) => p, (_, _) => p);
			}
		}

		public static void RunOnEndOfFrame() {
			isEndOfFrame = true;
			UpdateCount++;
			while (EndOfFrameList.TryDequeue(out var result)) {
				try {
					result();
				}
				catch (Exception e) {
					RLog.Err($"RunOnStartOfFrame Error: {e}");
				}
			}
			var dectionaryvalues = EndOfFrameExecute.ToArray();
			for (var i = 0; i < dectionaryvalues.Length; i++) {
				var currentobj = dectionaryvalues[i];
				try {
					currentobj.Value.Invoke();
				}
				catch (Exception e) {
					RLog.Err($"RunOnStartOfFrame Dictionary Target {currentobj.Key} Error: {e}");
				}
				EndOfFrameExecute.TryRemove(currentobj);
			}
			isEndOfFrame = false;
		}

	}
}
