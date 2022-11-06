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
	public enum RWindowModeEnum
	{
		//
		// Summary:
		//     Windowed mode, i.e. Godot.Window doesn't occupy the whole screen (unless set
		//     to the size of the screen).
		Windowed,
		//
		// Summary:
		//     Minimized window mode, i.e. Godot.Window is not visible and available on window
		//     manager's window list. Normally happens when the minimize button is pressed.
		Minimized,
		//
		// Summary:
		//     Maximized window mode, i.e. Godot.Window will occupy whole screen area except
		//     task bar and still display its borders. Normally happens when the minimize button
		//     is pressed.
		Maximized,
		//
		// Summary:
		//     Full screen window mode. Note that this is not exclusive full screen. On Windows
		//     and Linux, a borderless window is used to emulate full screen. On macOS, a new
		//     desktop is used to display the running project.
		//     Regardless of the platform, enabling full screen will change the window size
		//     to match the monitor's size. Therefore, make sure your project supports multiple
		//     resolutions when enabling full screen mode.
		Fullscreen,
		//
		// Summary:
		//     Exclusive full screen window mode. This mode is implemented on Windows only.
		//     On other platforms, it is equivalent to Godot.Window.ModeEnum.Fullscreen.
		//     Only one window in exclusive full screen mode can be visible on a given screen
		//     at a time. If multiple windows are in exclusive full screen mode for the same
		//     screen, the last one being set to this mode takes precedence.
		//     Regardless of the platform, enabling full screen will change the window size
		//     to match the monitor's size. Therefore, make sure your project supports multiple
		//     resolutions when enabling full screen mode.
		ExclusiveFullscreen
	}



	public interface IWindow : IDisposable
	{
		public bool LoadedIn { get; }

		public event Action<IWindow> OnLoadedIn;

		public void WaitOnLoadedIn(Action<IWindow> action);

		public int Width { get; set; }
		public int Height { get; set; }

		public string Title { get; set; }
		public Vector2i Size { get; set; }
		public Vector2i Position { get; set; }
		public RWindowModeEnum Mode { get; set; }

		public int CurrentScreen { get; set; }

		public bool Visible { get; set; }
		public bool WrapControls { get; set; }
		public bool Transient { get; set; }
		public bool Exclusive { get; set; }
		public bool Unresizable { get; set; }
		public bool Borderless { get; set; }
		public bool AlwaysOnTop { get; set; }
		public bool Transparent { get; set; }
		public bool Unfocusable { get; set; }
		public bool PopupWindow { get; set; }
		public bool ExtendToTitle { get; set; }
		public Vector2i MinSize { get; set; }
		public Vector2i MaxSize { get; set; }

		public Viewport Viewport { get; set; }
		public bool CanDraw();
		public bool HasFocus();
		public void GrabFocus();
		public bool IsEmbedded();

		public event Action<string[]> FilesDropped;
		public event Action MouseEntered;
		public event Action MouseExited;
		public event Action FocusEntered;
		public event Action FocusExited;
		public event Action CloseRequested;
		public event Action GoBackRequested;
		public event Action VisibilityChanged;
		public event Action AboutToPopup;
		public event Action TitlebarChanged;
		public event Action SizeChanged;

	}

	public interface IWindowManagerLink
	{
		public IWindow CreateNewWindow();
	}

	public sealed class WindowManager : IManager
	{

		private Engine _engine;

		private readonly List<IWindow> _windows = new();

		public IWindow MainWindow { get; private set; }

		public IWindowManagerLink windowManagerLink;

		public void Init(Engine engine) {
			_engine = engine;
			if (windowManagerLink is null) {
				return;//Stop no window manager
			}
			MainWindow.FilesDropped += MainWindow_FilesDropped;
		}

		private void MainWindow_FilesDropped(string[] obj) {

		}

		public void Dispose() {

		}

		public IWindow CreateNewWindow(int width = 800,int height = 600) {
			var data = windowManagerLink?.CreateNewWindow();
			data.WaitOnLoadedIn(LoadWindow);
			data.WaitOnLoadedIn(dataw => {
				dataw.Position = _windows[_windows.Count - 2].Position + new Vector2i(10, 10);
				dataw.Size = new Vector2i(width, height);
			});
			return data;
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
