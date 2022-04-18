using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine;
using RhuEngine.Linker;
using RhuEngine.Physics;

namespace Rhubarb_VR_HeadLess
{
	public class NullLinker : IEngineLink,IRLog,IRTime
	{
		public bool SpawnPlayer => false;

		private string TimeStamp => DateTime.Now.ToLongTimeString();

		private readonly Semaphore _semaphore = new(1,1);

		public bool CanRender => false;

		public bool CanAudio => false;

		public bool CanInput => false;

		public float Elapsedf { get; set; }

		public void BindEngine(Engine engine) {
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
			RLog.Instance = this;
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


	public static class Program
    {
		public static bool isRunning = true;
		public static Engine app;
		public static OutputCapture cap;
		public static NullLinker rhu;
		public static Type[] commands;
		static void Main(string[] args)
        {
			AppDomain.CurrentDomain.ProcessExit += (_, _) => isRunning = false;
			Console.WriteLine("Starting Rhubarb HeadLess!");
			cap = new OutputCapture();
			rhu = new NullLinker();
			app = new Engine(rhu, args, cap, AppDomain.CurrentDomain.BaseDirectory);
			var EngineThread = new Thread(() => {
				while (isRunning) {
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write($"{app?.netApiManager.User?.UserName ?? "Not Login"}> ");
					Console.ForegroundColor = ConsoleColor.White;
					var line = Console.ReadLine();
					var foundcomand = false;
					foreach (var item in commands) {
						if (item.Name.ToLower() == line.ToLower()) {
							foundcomand = true;
							((Command)Activator.CreateInstance(item)).RunCommand();
						}
					}
					if (!foundcomand) {
						Console.WriteLine($"{line} Is not a valid command run Help for available commands");
					}
					Thread.Sleep(8);
				}
			}) {
				Priority = ThreadPriority.AboveNormal
			};
			app.OnEngineStarted = () => EngineThread.Start();
			app.Init();
			var EngineStopWatch = new Stopwatch();
			commands = (from a in AppDomain.CurrentDomain.GetAssemblies()
						   from t in a.GetTypes()
						   where typeof(Command).IsAssignableFrom(t)
						   where t.IsClass && !t.IsAbstract
						   select t).ToArray();

			try {
				while (isRunning) {
					EngineStopWatch.Start();
					app.Step();
					rhu.Elapsedf = EngineStopWatch.ElapsedMilliseconds;
					EngineStopWatch.Restart();
					var wait = (int)((1000 / 120) - rhu.Elapsedf);
					if (wait > 0) {
						Thread.Sleep(wait);
					}
				}
			}
			catch (Exception ex) {
				RLog.Err("Engine Crashed" + ex);
			}
			app.IsCloseing = true;
			cap.DisableSingleString = true;
			app.Dispose();
			cap.Dispose();
        }
	}
}
