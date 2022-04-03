using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Linker
{
	public interface IRLog
	{

		public void Info(string value);

		public void Err(string value);

		public void Warn(string v);

		public void Subscribe(Action<LogLevel, string> logCall);

		public void Unsubscribe(Action<LogLevel, string> logCall);
	}

	public static class RLog
	{
		public static IRLog Instance { get; set; }

		public static void Info(string value) {
			Instance?.Info(value);
		}

		public static void Err(string value) {
			Instance?.Err(value);
		}

		public static void Warn(string value) {
			Instance?.Warn(value);
		}

		public static void Subscribe(Action<LogLevel, string> logCall) {
			Instance?.Subscribe(logCall);
		}

		public static void Unsubscribe(Action<LogLevel, string> logCall) {
			Instance?.Unsubscribe(logCall);
		}
	}
}
