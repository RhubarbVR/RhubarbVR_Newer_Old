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

public partial class EngineRunner : Node3D, IRTime
{
	[Export]
	public XRCamera3D Camera;
	[Export]
	public bool StartInVR = false;

	public Engine engine;

	[ThreadStatic]
	public bool IsMainThread = false;

	public static EngineRunner _;

	public OutputCapture outputCapture;

	public GodotEngineLink link;

	public override void _Ready()
	{
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
		var appPath = Environment.CurrentDirectory;
		RLog.Info("App Path: " + appPath);
		engine = new Engine(link, args.ToArray(), outputCapture, appPath);
		engine.OnCloseEngine += () => GetTree().Quit();
		engine.Init(false);

	}

	public override void _Process(double delta)
	{
		engine?.Step();
	}

	public override void _ExitTree() {
		base._ExitTree();
		RLog.Info("_ExitTree called");
		ProcessCleanup();
	}

	private bool IsDisposeing { set; get; }

	public float Elapsedf => (float)GetProcessDeltaTime();

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
}
