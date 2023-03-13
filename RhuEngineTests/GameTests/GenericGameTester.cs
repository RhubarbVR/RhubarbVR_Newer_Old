using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

using NullContext;

using RhuEngine.Linker;

using RNumerics;

using SharedModels.GameSpecific;

namespace RhuEngine.GameTests.Tests
{

	public class GenericGameTester : IDisposable
	{
		public OutputCapture cap;
		public NullLinker rhu;
		public Engine app;
		public Stopwatch EngineStopWatch = new();
		public void RunForSteps(int amountofSteps = 10) {
			for (var i = 0; i < amountofSteps; i++) {
				Step();
			}
		}

		public void Step() {
			rhu.Elapsed = EngineStopWatch.ElapsedMilliseconds;
			if(rhu.Elapsed == 0) {
				rhu.Elapsed = 1;
			}
			EngineStopWatch.Restart();
			app.Step();
			var wait = (int)((1000 / 120) - rhu.Elapsed);
			if (wait > 0) {
				Thread.Sleep(wait);
			}
		}

		public void Start(string[] args) {
			Console.WriteLine("Starting Rhubarb Test!");
			EngineStopWatch.Start();
			cap = new OutputCapture();
			rhu = new NullLinker();
			var dir = AppDomain.CurrentDomain.BaseDirectory + $"\\Tests\\Test{DateTime.Now:yyyy-dd-M--HH-mm-ss}_{Guid.NewGuid()}\\";
			Directory.CreateDirectory(dir);
			app = new Engine(rhu, args, cap, dir, true);
			app.Init();
		}

		public void Dispose() {
			app.IsCloseing = true;
			app.Dispose();
			cap.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
