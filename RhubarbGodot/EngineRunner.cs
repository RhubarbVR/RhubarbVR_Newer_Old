using Godot;
using System;
using RhuEngine;
using GEngine = Godot.Engine;
using Engine = RhuEngine.Engine;
using RhuEngine.Linker;
using RhubarbVR.Bindings;
using Thread = System.Threading.Thread;
using System.Collections.Generic;
using Environment = System.Environment;
using System.Collections;
using RNumerics;
using RhubarbVR.Bindings.Input;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "<Pending>")]
public partial class EngineRunner : Node3D, IRTime
{

	[Export]
	public SubViewport ThowAway;

	[Export]
	public XRCamera3D Camera;

	[Export]
	public XROrigin3D Rigin;

	[Export]
	public AudioListener3D AudioListener;

	[Export]
	public bool StartInVR = false;

	[Export]
	public bool StartDebugVisuals = false;

	public Engine engine;

	[ThreadStatic]
	private static bool _isMainThread = false;
	public OutputCapture outputCapture;

	public GodotEngineLink link;

	private readonly List<Action> _runOnMainThread = new();

	public void RunOnMainThread(Action action) {
		if (_isMainThread) {
			action?.Invoke();
		}
		else {
			lock (_runOnMainThread) {
				_runOnMainThread.Add(action);
			}
		}
	}

	public override void _Ready() {
		SetProcessInternal(true);
		if (RLog.Instance == null) {
			RLog.Instance = new GodotLogs();
		}
		else {
			((GodotLogs)RLog.Instance).Clear();
		}
		if (engine != null) {
			RLog.Err("Engine already running");
			return;
		}
		if (!_isMainThread) {
			if (Thread.CurrentThread.Name != "Godot Main Thread") {
				Thread.CurrentThread.Name = "Godot Main Thread";
			}
			Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
			_isMainThread = true;
			EngineRunnerHelpers._ = this;
		}
		outputCapture = new OutputCapture();
		link = new GodotEngineLink(this);
		var args = new List<string>(Environment.GetCommandLineArgs());
		if (!StartInVR) {
			args.Add("--no-vr");
		}
		if (StartDebugVisuals) {
			args.Add("--debug-visuals");
		}
		var appPath = Environment.CurrentDirectory + "/";
		RLog.Info("App Path: " + appPath);
		engine = new Engine(link, args.ToArray(), outputCapture, appPath);
		engine.OnCloseEngine += () => GetTree().Quit();
		engine.Init();
	}

	public string Typer;

	public Vector2f MouseScrollDelta;
	public Vector2f MouseScrollPos;
	public Vector2f LastMouseScrollPos;

	public override void _Notification(long what) {
		if (what == Node.NotificationInternalProcess) {
			MouseScrollDelta = MouseScrollPos - LastMouseScrollPos;
			LastMouseScrollPos = MouseScrollPos;
			engine?.Step();
			TypeDelta = "";
			MouseDelta = Vector2f.Zero;
		}
	}

	public override void _Process(double delta) {
		lock (_runOnMainThread) {
			foreach (var item in _runOnMainThread) {
				item?.Invoke();
			}
			_runOnMainThread.Clear();
		}
		//Dont Like this but it works
		if (!link.InVR) {
			Camera.SetPos(RRenderer.LocalCam);
		}
		//Dont Like this but it works
	}

	public override void _ExitTree() {
		base._ExitTree();
		RLog.Info("ExitTree called");
		ProcessCleanup();
	}

	private bool IsDisposeing { set; get; }

	public double Elapsed => GetProcessDeltaTime();

	public Vector2f MouseDelta { get; internal set; }
	public Vector2f MousePos { get; internal set; }

	private void ProcessCleanup() {
		if (engine == null) {
			RLog.Err("Engine not started for cleanup");
			return;
		}
		try {
			if (!IsDisposeing) {
				IsDisposeing = true;
				RLog.Info("Rhubarb CleanUp Started");
				engine.IsCloseing = true;
				engine.Dispose();
				engine = null;
				EngineHelpers.MainEngine = null;
			}
		}
		catch (Exception ex) {
			RLog.Err("Failed to start Rhubarb CleanUp " + ex.ToString());
		}
	}

	public string TypeDelta = "";

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseMotion mouseMotion) {
			MousePos = new Vector2f(mouseMotion.Position.x, mouseMotion.Position.y);
			MouseDelta = new Vector2f(mouseMotion.Relative.x, mouseMotion.Relative.y);
		}
		if(@event is InputEventMouseButton button) {
			if(button.ButtonIndex == MouseButton.WheelUp) {
				MouseScrollPos += new Vector2f(0,1);
			}
			if (button.ButtonIndex == MouseButton.WheelDown) {
				MouseScrollPos += new Vector2f(0, -1);
			}
			if (button.ButtonIndex == MouseButton.WheelRight) {
				MouseScrollPos += new Vector2f(1, 0);
			}
			if (button.ButtonIndex == MouseButton.WheelLeft) {
				MouseScrollPos += new Vector2f(-1, 0);
			}
		}
		if (@event is InputEventKey key) {
			var newString = System.Text.Encoding.UTF8.GetString(BitConverter.GetBytes(key.Unicode));
			var clearFrom = newString.IndexOf('\0', 0);
			if(key.Keycode == Godot.Key.Backspace) {
				if (key.Pressed) {
					TypeDelta += "\b";
				}
			}
			else {
				TypeDelta += newString.Remove(clearFrom);
			}
		}

	}
}
