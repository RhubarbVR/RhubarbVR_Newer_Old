using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

using MessagePack;
using MessagePack.Resolvers;

using NullContext;

using RhuEngine.Linker;

using RNumerics;

using SharedModels.GameSpecific;

namespace RhuEngine.GameTests.Tests
{

	public class GenericGameTester:IDisposable
	{
		public OutputCapture cap;
		public NullLinker rhu;
		public Engine app;
		public Stopwatch EngineStopWatch = new Stopwatch();
		public void RunForSteps(int amountofSteps = 10) {
			for (var i = 0; i < amountofSteps; i++) {
				Step();
			}
		}

		public void Step() {
			EngineStopWatch.Start();
			app.Step();
			rhu.Elapsedf = EngineStopWatch.ElapsedMilliseconds;
			EngineStopWatch.Restart();
			var wait = (int)((1000 / 120) - rhu.Elapsedf);
			if (wait > 0) {
				Thread.Sleep(wait);
			}
		}

		public void Start(string[] args) {
			Console.WriteLine("Starting Rhubarb Test!");
			cap = new OutputCapture();
			rhu = new NullLinker();
			var dir = AppDomain.CurrentDomain.BaseDirectory + $"\\Tests\\Test{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}_{Guid.NewGuid()}\\";
			Directory.CreateDirectory(dir);
			app = new Engine(rhu, args, cap, dir, true);
			app.Init();
		}

		public void Dispose() {
			app.IsCloseing = true;
			app.Dispose();
			cap.Dispose();
		}
	}
}
