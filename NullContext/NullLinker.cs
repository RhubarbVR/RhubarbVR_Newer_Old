using System;
using System.Threading;

using RhuEngine;
using RhuEngine.Linker;
using RhuEngine.Managers;

namespace NullContext
{
	public sealed class NullLinker : IEngineLink, IRLog, IRTime
	{
		public void LoadArgs() {
		}
		public string BackendID => "HeadLess";

		public bool SpawnPlayer => false;

		private static string TimeStamp => DateTime.Now.ToLongTimeString();

		private readonly Semaphore _semaphore = new(1, 1);

		public bool CanRender => false;

		public bool CanAudio => false;

		public bool CanInput => false;

		public double Elapsed { get; set; }

		public bool ForceLibLoad => false;

		public bool InVR => false;

		public bool LiveVRChange => false;

		public Type RenderSettingsType => null;

		public void BindEngine(Engine engine) {
			RLog.Instance = this;
		}

		public void Err(string value) {
			LogOutput(LogLevel.Error, value);
		}

		private void LogOutput(LogLevel logLevel, string log) {
			if (string.IsNullOrEmpty(log)) {
				return;
			}
			_semaphore.WaitOne();
			var logPreamble = $"[{TimeStamp}]: ";
			switch (logLevel) {
				case LogLevel.Diagnostic:
					RhuConsole.ForegroundColor = ConsoleColor.Magenta;
					break;
				case LogLevel.Info:
					RhuConsole.ForegroundColor = ConsoleColor.Green;
					break;
				case LogLevel.Warning:
					RhuConsole.ForegroundColor = ConsoleColor.Yellow;
					break;
				case LogLevel.Error:
					RhuConsole.ForegroundColor = ConsoleColor.Red;
					break;
				default:
					break;
			}
			Console.Write(logPreamble);
			RhuConsole.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(log);
			_semaphore.Release();
			try {
				Log?.Invoke(logLevel, logPreamble + log);
			}
			catch { }
		}

		public void Info(string value) {
			LogOutput(LogLevel.Info, value);
		}

		public void LoadStatics() {
			RTime.Instance = this;
		}

		public void Start() {
		}

		public event Action<LogLevel, string> Log;

		public event Action<bool> VRChange;

		public void Subscribe(Action<LogLevel, string> logCall) {
			Log += logCall;
		}

		public void Unsubscribe(Action<LogLevel, string> logCall) {
			Log -= logCall;
		}

		public void Warn(string v) {
			LogOutput(LogLevel.Warning, v);
		}

		public void ChangeVR(bool value) {
			throw new NotImplementedException();
		}

		public void LoadInput(InputManager manager) {
		}
	}

}
