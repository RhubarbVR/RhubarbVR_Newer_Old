
using System;
using System.Linq;

using RhuEngine.Managers;

using StereoKit;
using RhuEngine.Settings;
using System.IO;
using RhuSettings;
using System.Collections.Generic;

namespace RhuEngine
{
	public class Engine : IDisposable
	{
		private readonly bool _forceFlatscreen = false;

		private readonly bool _noVRSim = false;

		private readonly string _cachePathOverRide = null;

		private readonly string _userDataPathOverRide = null;

		public readonly Version version = new(1, 0, 0);
#if DEBUG
		public const bool IS_MILK_SNAKE = true;
#else
		public const bool IS_MILK_SNAKE = false;
#endif
		public readonly static UISettings DefaultSettings = new(){
			backplateBorder = 1f * Units.mm2m,
			backplateDepth = 0.4f,
			depth = 10 * Units.mm2m,
			gutter = 10 * Units.mm2m,
			padding = 10 * Units.mm2m
		};

		static UISettings _globalSettings = DefaultSettings;

		public UISettings UISettings
		{
			get => _globalSettings;
			set { _globalSettings = value; UI.Settings = value; }
		}

		public readonly string SettingsFile;

		public Engine(string[] arg, OutputCapture outputCapture) : base() {
			outputCapture.LogsPath = _userDataPathOverRide is null ? AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" : _userDataPathOverRide + "\\Logs\\";
			outputCapture.Start();
			string error = null;
			_forceFlatscreen = arg.Any((v) => v.ToLower() == "--no-vr") | arg.Any((v) => v.ToLower() == "-no-vr");
			_noVRSim = arg.Any((v) => v.ToLower() == "--no-vr-sim") | arg.Any((v) => v.ToLower() == "-no-vr-sim");
			string settingsArg = null;
			for (var i = 0; i < arg.Length; i++) {
				if(arg[i].ToLower() == "--cache-override" | arg[i].ToLower() == "-cache-override") {
					if (i + 1 <= arg.Length) {
						_cachePathOverRide = arg[i + 1];
					}
					else {
						error = "Cache Path not specified";
					}
				}
				if (arg[i].ToLower() == "--userdata-override" | arg[i].ToLower() == "-userdata-override") {
					if (i + 1 <= arg.Length) {
						_userDataPathOverRide = arg[i + 1];
					}
					else {
						error = "User Data Path not specified";
					}
				}
				if (arg[i].ToLower() == "--settings" | arg[i].ToLower() == "-settings") {
					if (i + 1 <= arg.Length) {
						settingsArg = arg[i + 1];
					}
					else {
						error = "Settings not specified";
					}
				}
			}
			outputCapture.LogsPath = _userDataPathOverRide is null ? AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" : _userDataPathOverRide + "\\Logs\\";
			outputCapture.Start();
			if (error is not null) {
				Log.Err(error);
			}
			if (arg.Length <= 0) {
				Log.Info($"Launched with no arguments");
			}
			else {
				Log.Info($"Launched with {(_forceFlatscreen ? "forceFlatscreen " : "")}{(_noVRSim ? "noVRSim " : "")}{((_cachePathOverRide != null) ? "Cache Override: " + _cachePathOverRide + " " : "")}{((_userDataPathOverRide != null) ? "UserData Override: " + _userDataPathOverRide + " " : "")}");
			}
			this.outputCapture = outputCapture;

			var lists = new List<DataList>();
			SettingsFile = _userDataPathOverRide + "\\settings.json";
			if (File.Exists(SettingsFile)) {
				var text = File.ReadAllText(SettingsFile);
				var liet = SettingsManager.GetDataFromJson(text);
				lists.Add(liet);
			}
			if (!string.IsNullOrWhiteSpace(settingsArg)) {
				foreach (var item in settingsArg.Split('|')) {
					var text = File.Exists(item) ? File.ReadAllText(item) : item;
					try {
						var liet = SettingsManager.GetDataFromJson(text);
						lists.Add(liet);
					}
					catch (Exception e) {
						Log.Err("Error loading settings ERROR:" + e.ToString(), true);
					}
				}
			}
			MainSettings = lists.Count == 0 ? new MainSettingsObject() : SettingsManager.LoadSettingsObject<MainSettingsObject>(lists.ToArray());
		}

		public void SaveSettings() {
			var data = SettingsManager.GetDataListFromSettingsObject(MainSettings,new DataList());
			File.WriteAllText(SettingsFile, SettingsManager.GetJsonFromDataList(data).ToString());
		}
		public void ReloadSettings() {
			var lists = new List<DataList>();
			if (File.Exists(SettingsFile)) {
				var text = File.ReadAllText(SettingsFile);
				var liet = SettingsManager.GetDataFromJson(text);
				lists.Add(liet);
			}
			MainSettings = lists.Count == 0 ? new MainSettingsObject() : SettingsManager.LoadSettingsObject<MainSettingsObject>(lists.ToArray());
		}

		private string _mainMic;

		public string MainMic
		{
			get => _mainMic;
			set {
				_mainMic = value;
				MicChanged?.Invoke(value);
			}
		}

		public Action<string> MicChanged;

		public SKSettings Settings
		{
			get {
				return new SKSettings {
#if DEBUG
					appName = "Milk Snake",
#else
					appName = "RhubarbVR",
#endif
					assetsFolder = "Assets",
					displayPreference = _forceFlatscreen ? DisplayMode.Flatscreen : DisplayMode.MixedReality,
					disableFlatscreenMRSim = _noVRSim,
					flatscreenHeight = 720,
					flatscreenWidth = 1280,
				};
			}
		}
		public NetApiManager netApiManager;

		public WorldManager worldManager = new();

		public AssetManager assetManager;

		public InputManager inputManager = new();

		//public WebBrowserManager webBrowserManager = new();

		public MainSettingsObject MainSettings;

		public OutputCapture outputCapture;

		public IManager[] _managers;

		public void Init() {
			Platform.ForceFallbackKeyboard = true;
			World.OcclusionEnabled = true;
			World.RaycastEnabled = true;
			netApiManager = new NetApiManager(_userDataPathOverRide);
			assetManager = new AssetManager(_cachePathOverRide);
			_managers = new IManager[] { inputManager, netApiManager, assetManager , worldManager };
			foreach (var item in _managers) {
				try {
					item.Init(this);
				}
				catch (Exception ex) {
					Log.Err($"Failed to start {item.GetType().GetFormattedName()} Error:{ex}");
				}
			}
		}

		public void Step() {
			foreach (var item in _managers) {
				item.Step();
			}
		}

		public void Dispose() {
			foreach (var item in _managers) {
				item.Dispose();
			}
		}
	}
}