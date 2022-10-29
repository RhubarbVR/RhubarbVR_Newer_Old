using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.Components;
using RhuEngine.DataStructure;
using RhuEngine.WorldObjects;

using SharedModels;
using SharedModels.GameSpecific;

using RNumerics;
using RhuEngine.Linker;
using DataModel.Enums;
using Esprima;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Managers
{
	public interface IWindow
	{
		public int Width { get; set; }
		public int Height { get; set; }

		public string Title { get; set; }
		public Vector2i Position { get; set; }
	}

	public sealed class WindowManager : IManager
	{

		private Engine _engine;

		private readonly List<IWindow> _windows = new();

		public IWindow MainWindow { get; private set; }

		public void Init(Engine engine) {
			_engine = engine;
		}

		public void Dispose() {

		}

		public void LoadWindow(IWindow window) {
			MainWindow ??= window;
			_windows.Add(window);
#if DEBUG
			RLog.Info($"Loaded Window\nTitle:{window.Title}\nPosition:{window.Position}\nWidth:{window.Width}\nHeight:{window.Height}");
#endif
		}
		public void UnLoadWindow(IWindow window) {
			_windows.Remove(window);
		}
		public void RenderStep() {

		}

		public void Step() {

		}
	}
}
