using System;
using System.Threading;

using RhuEngine;
using RhuEngine.Linker;
using RhuEngine.Physics;

namespace NullContext
{
	public class NullLinker : IEngineLink, IRLog, IRTime
	{
		public string BackendID => "HeadLess";

		public bool SpawnPlayer => false;

		private string TimeStamp => DateTime.Now.ToLongTimeString();

		private readonly Semaphore _semaphore = new Semaphore(1, 1);

		public bool CanRender => false;

		public bool CanAudio => false;

		public bool CanInput => false;

		public float Elapsedf { get; set; }

		public bool ForceLibLoad => false;

		public bool InVR => false;

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
					Console.ForegroundColor = ConsoleColor.Magenta;
					break;
				case LogLevel.Info:
					Console.ForegroundColor = ConsoleColor.Green;
					break;
				case LogLevel.Warning:
					Console.ForegroundColor = ConsoleColor.Yellow;
					break;
				case LogLevel.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
				default:
					break;
			}
			Console.Write(logPreamble);
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(log);
			_semaphore.Release();
			Log?.Invoke(logLevel, logPreamble + log);
		}

		public void Info(string value) {
			LogOutput(LogLevel.Info, value);
		}

		public void LoadStatics() {
			RTime.Instance = this;
			PhysicsHelper.RegisterPhysics<RBullet.BulletPhsyicsLink>();
		}

		public void Start() {
		}

		public event Action<LogLevel, string> Log;

		public void Subscribe(Action<LogLevel, string> logCall) {
			Log += logCall;
		}

		public void Unsubscribe(Action<LogLevel, string> logCall) {
			Log -= logCall;
		}

		public void Warn(string v) {
			LogOutput(LogLevel.Warning, v);
		}
	}

}
