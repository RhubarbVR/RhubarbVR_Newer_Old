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
