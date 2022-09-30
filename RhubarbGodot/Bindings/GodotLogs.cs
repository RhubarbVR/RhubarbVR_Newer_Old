using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;
using RhuEngine.Linker;

namespace RhubarbVR.Bindings
{
	public class GodotLogs : IRLog
	{
		public void Err(string value) {
			Log(LogLevel.Error, value);
		}

		public void Info(string value) {
			Log(LogLevel.Info, value);
		}
		public void Warn(string v) {
			Log(LogLevel.Warning, v);
		}

		public void Log(LogLevel level, string v) {
			switch (level) {
				case LogLevel.Diagnostic:
					GD.Print("Diagnostic: " + v);
					break;
				case LogLevel.Warning:
					GD.Print("Warning: " + v);
					break;
				case LogLevel.Error:
					GD.PrintErr(v);
					break;
				default:
					GD.Print(v);
					break;
			}
			Callback?.Invoke(level, $"[{level.ToString().ToLower()}] " + v + "\n");
		}

		event Action<LogLevel, string> Callback;

		public void Subscribe(Action<LogLevel, string> logCall) {
			Callback += logCall;
		}

		public void Unsubscribe(Action<LogLevel, string> logCall) {
			Callback -= logCall;
		}

		public void Clear() {
			var delegates = Callback.GetInvocationList();
			foreach (var item in delegates) {
				Callback -= (Action<LogLevel, string>)item;
			}
		}
	}
}
