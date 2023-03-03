using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GDExtension;
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
			var trains = Console.ForegroundColor;
			switch (level) {
				case LogLevel.Diagnostic:
					Console.ForegroundColor = ConsoleColor.Magenta;
					Console.WriteLine("Diagnostic: " + v);
					Console.ForegroundColor = trains;
					break;
				case LogLevel.Warning:
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Warning: " + v);
					Console.ForegroundColor = trains;
					break;
				case LogLevel.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(v);
					Console.ForegroundColor = trains;
					break;
				default:
					Console.WriteLine(v);
					break;
			}
			try {
				Callback?.Invoke(level, $"[{level.ToString().ToLower()}] " + v + "\n");
			}
			catch { }
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
