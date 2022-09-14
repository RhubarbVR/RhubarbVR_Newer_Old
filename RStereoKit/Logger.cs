using System;
using System.Collections.Generic;
using System.Text;
using StereoKit;
using RhuEngine.Linker;

namespace RStereoKit
{
	public sealed class Logger : IRLog
	{
		public void Err(string value) {
			Log.Err(value);
		}

		public void Info(string value) {
			Log.Info(value);
		}

		private Action<RhuEngine.Linker.LogLevel, string> _logCall;

		public void Subscribe(Action<RhuEngine.Linker.LogLevel, string> logCall) {
			_logCall = logCall ?? throw new ArgumentNullException(nameof(logCall));
			Log.Subscribe(Logs);
		}

		private void Logs(StereoKit.LogLevel level, string str) {
			_logCall?.Invoke((RhuEngine.Linker.LogLevel)(int)level, str);
		}

		public void Unsubscribe(Action<RhuEngine.Linker.LogLevel, string> logCall) {
			_logCall = null;
			Log.Unsubscribe(Logs);
		}

		public void Warn(string v) {
			Log.Warn(v);
		}
	}
}
