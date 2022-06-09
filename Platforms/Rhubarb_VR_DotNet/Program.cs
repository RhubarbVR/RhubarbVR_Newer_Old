using System;
using StereoKit;
using RhuEngine;
using RStereoKit;

class SKLoader
{
	static void Main(string[] args) {
		// This will allow the App constructor to call a few SK methods
		// before Initialize is called.
		SK.PreLoadLibrary();
		var cap = new OutputCapture();
		var rhu = new RhuStereoKit();
		var app = new Engine(rhu, args, cap, AppDomain.CurrentDomain.BaseDirectory);
		if (app == null) {
			throw new Exception("StereoKit loader couldn't construct an instance of the App!");
		}

		// Initialize StereoKit, and the app
		if (!SK.Initialize(rhu.Settings)) {
			Environment.Exit(1);
		}
		app.OnCloseEngine += () => SK.Quit();
		app.Init();

		// Now loop until finished, and then shut down
		while (SK.Step(app.Step)) {
		}
		app.IsCloseing = true;
		cap.DisableSingleString = true;
		app.Dispose();
		SK.Shutdown();
	}
}