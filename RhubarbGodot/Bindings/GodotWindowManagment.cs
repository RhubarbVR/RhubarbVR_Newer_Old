using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhubarbVR.Bindings.FontBindings;
using RhubarbVR.Bindings.Input;
using RhubarbVR.Bindings.TextureBindings;

using RhuEngine;
using RhuEngine.Linker;
using RhuEngine.Managers;
using RhuEngine.Settings;

using RNumerics;

using static Godot.Window;

using Engine = RhuEngine.Engine;

namespace RhubarbVR.Bindings
{
	public sealed class GodotWindowManager : IWindowManagerLink
	{
		public IWindow CreateNewWindow() {
			var newWindow = new GodotWindow();
			EngineRunnerHelpers._.RunOnMainThread(() => {
				var newWin = new Window {
					TransparentBg = true
				};
				var con = new ConnectedViewport();
				newWin.AddChild(con);
				con.SetAnchorsPreset(Control.LayoutPreset.FullRect);
				EngineRunnerHelpers._.AddChild(newWin);
				newWindow.LoadInValue(newWin);
			});
			return newWindow;
		}
	}

	public sealed class GodotWindow : IWindow
	{
		public Window window;

		public GodotWindow() {

		}

		public GodotWindow(Window window) {
			LoadInValue(window);
		}

		public int Width { get => window.Size.x; set => window.Size = new Godot.Vector2i(value, window.Size.y); }
		public int Height { get => window.Size.y; set => window.Size = new Godot.Vector2i(window.Size.x, value); }
		public string Title { get => window.Title; set => window.Title = value; }
		public RNumerics.Vector2i Position { get => new(window.Position.x, window.Position.y); set => window.Position = new Godot.Vector2i(value.x, value.y); }

		public bool LoadedIn => window is not null;

		public RNumerics.Vector2i Size { get => new(window.Size.x, window.Size.y); set => window.Size = new(value.x, value.y); }
		public RNumerics.Vector2i MinSize { get => new(window.MinSize.x, window.MinSize.y); set => window.MinSize = new(value.x, value.y); }
		public RNumerics.Vector2i MaxSize { get => new(window.MaxSize.x, window.MaxSize.y); set => window.MaxSize = new(value.x, value.y); }
		public RWindowModeEnum Mode { get => (RWindowModeEnum)window.Mode; set => window.Mode = (ModeEnum)value; }
		public int CurrentScreen { get => window.CurrentScreen; set => window.CurrentScreen = value; }
		public bool Visible { get => window.Visible; set => window.Visible = value; }
		public bool WrapControls { get => window.WrapControls; set => window.WrapControls = value; }
		public bool Transient { get => window.Transient; set => window.Transient = value; }
		public bool Exclusive { get => window.Exclusive; set => window.Exclusive = value; }
		public bool Unresizable { get => window.Unresizable; set => window.Unresizable = value; }
		public bool Borderless { get => window.Borderless; set => window.Borderless = value; }
		public bool AlwaysOnTop { get => window.AlwaysOnTop; set => window.AlwaysOnTop = value; }
		public bool Transparent { get => window.Transparent; set => window.Transparent = value; }
		public bool Unfocusable { get => window.Unfocusable; set => window.Unfocusable = value; }
		public bool PopupWindow { get => window.PopupWindow; set => window.PopupWindow = value; }
		public bool ExtendToTitle { get => window.ExtendToTitle; set => window.ExtendToTitle = value; }
		public RhuEngine.Components.Viewport Viewport { get => window.GetChild<ConnectedViewport>(0).Viewport; set => window.GetChild<ConnectedViewport>(0).Viewport = value; }

		public event Action<IWindow> OnLoadedIn;

		public event Action SizeChanged { add => window.SizeChanged += value; remove => window.SizeChanged -= value; }
		public event Action<string[]> FilesDropped { add => window.FilesDropped += new(value); remove => window.FilesDropped -= new(value); }
		public event Action MouseEntered { add => window.MouseEntered += new(value); remove => window.MouseEntered -= new(value); }
		public event Action MouseExited { add => window.MouseExited += new(value); remove => window.MouseExited -= new(value); }
		public event Action FocusEntered { add => window.FocusEntered += new(value); remove => window.FocusEntered -= new(value); }
		public event Action FocusExited { add => window.FocusExited += new(value); remove => window.FocusExited -= new(value); }
		public event Action CloseRequested { add => window.CloseRequested += new(value); remove => window.CloseRequested -= new(value); }
		public event Action GoBackRequested { add => window.GoBackRequested += new(value); remove => window.GoBackRequested -= new(value); }
		public event Action VisibilityChanged { add => window.VisibilityChanged += new(value); remove => window.VisibilityChanged -= new(value); }
		public event Action AboutToPopup { add => window.AboutToPopup += new(value); remove => window.AboutToPopup -= new(value); }
		public event Action TitlebarChanged { add => window.TitlebarChanged += new(value); remove => window.TitlebarChanged -= new(value); }

		public void LoadInValue(Window newwindow) {
			window = newwindow ?? throw new ArgumentNullException(nameof(newwindow));
			OnLoadedIn?.Invoke(this);
			OnLoadedIn = null;
		}

		public void Dispose() {
			window?.QueueFree();
			window = null;
			EngineRunnerHelpers._.engine.windowManager.UnLoadWindow(this);
		}

		public void WaitOnLoadedIn(Action<IWindow> action) {
			if (LoadedIn) {
				action(this);
			}
			else {
				OnLoadedIn += action;
			}
		}

		public bool CanDraw() {
			return window.CanDraw();
		}

		public bool HasFocus() {
			return window.HasFocus();
		}

		public void GrabFocus() {
			window.GrabFocus();
		}

		public bool IsEmbedded() {
			return window.IsEmbedded();
		}
	}
}
