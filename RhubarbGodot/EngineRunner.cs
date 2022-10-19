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

	public Engine engine;

	[ThreadStatic]
	public bool IsMainThread = false;

	public static EngineRunner _;

	public OutputCapture outputCapture;

	public GodotEngineLink link;

	public override void _Ready() {
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
		if (!IsMainThread) {
			if (Thread.CurrentThread.Name != "Godot Main Thread") {
				Thread.CurrentThread.Name = "Godot Main Thread";
			}
			Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
			IsMainThread = true;
			_ = this;
		}
		outputCapture = new OutputCapture();
		link = new GodotEngineLink(this);
		var args = new List<string>(Environment.GetCommandLineArgs());
		if (!StartInVR) {
			args.Add("--no-vr");
		}
		var appPath = Environment.CurrentDirectory + "/";
		RLog.Info("App Path: " + appPath);
		engine = new Engine(link, args.ToArray(), outputCapture, appPath);
		engine.OnCloseEngine += () => GetTree().Quit();
		engine.Init();
	}

	public string Typer;

	public override void _Process(double delta) {
		//Dont Like this but it works
		if (!link.InVR) {
			Camera.Position = Vector3.Zero;
			Camera.Rotation = Vector3.Zero;
		}
		//Dont Like this but it works
		engine?.Step();
		TypeDelta = "";
		MouseDelta = Vector2f.Zero;
	}

	public override void _ExitTree() {
		base._ExitTree();
		RLog.Info("ExitTree called");
		ProcessCleanup();
	}

	private bool IsDisposeing { set; get; }

	public float Elapsedf => (float)GetProcessDeltaTime();

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
				Engine.MainEngine = null;
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
