using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine;
using RhuEngine.Linker;
using RhuEngine.Physics;
using NullContext;

namespace Rhubarb_VR_HeadLess
{

	public static class Program
    {
#pragma warning disable CA2211 // Non-constant fields should not be visible
		public static bool _isRunning = true;
		public static Engine _app;
		static OutputCapture _cap;
		static NullLinker _rhu;
#pragma warning restore CA2211 // Non-constant fields should not be visible
		static void Main(string[] args)
        {
			AppDomain.CurrentDomain.ProcessExit += (_, _) => _isRunning = false;
			Console.WriteLine("Starting Rhubarb HeadLess!");
			_cap = new OutputCapture();
			_rhu = new NullLinker();
			_app = new Engine(_rhu, args, _cap, AppDomain.CurrentDomain.BaseDirectory);
			var EngineThread = new Thread(() => {
				while (_isRunning) {
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write($"{_app?.netApiManager.User?.UserName ?? "Not Login"}> ");
					Console.ForegroundColor = ConsoleColor.White;
					var line = Console.ReadLine();
					_app.commandManager.RunComand(line);
					Thread.Sleep(8);
				}
			}) {
				Priority = ThreadPriority.AboveNormal
			};
			_app.OnEngineStarted = () => EngineThread.Start();
			_app.Init();
			var EngineStopWatch = new Stopwatch();
			try {
				while (_isRunning) {
					EngineStopWatch.Start();
					_app.Step();
					_rhu.Elapsedf = EngineStopWatch.ElapsedMilliseconds;
					EngineStopWatch.Restart();
					var wait = (int)((1000 / 120) - _rhu.Elapsedf);
					if (wait > 0) {
						Thread.Sleep(wait);
					}
				}
			}
			catch (Exception ex) {
				RLog.Err("Engine Crashed" + ex);
			}
			_app.IsCloseing = true;
			_cap.DisableSingleString = true;
			_app.Dispose();
			_cap.Dispose();
        }
	}
}
