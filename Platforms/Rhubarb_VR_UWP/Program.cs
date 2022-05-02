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

		// If the app has a constructor that takes a string array, then
		// we'll use that, and pass the command line arguments into it on
		// creation
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

		app.Init();

		// Now loop until finished, and then shut down
		while (SK.Step(app.Step)) { }
		cap.DisableSingleString = true;
		app.IsCloseing = true;
		app.Dispose();
		SK.Shutdown();
	}
}