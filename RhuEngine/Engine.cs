
using System;
using System.Linq;

using RhuEngine.Managers;

using StereoKit;

namespace RhuEngine
{
	public class Engine : IDisposable
	{
		private readonly bool _forceFlatscreen = false;

		private readonly bool _noVRSim = false;

		private readonly string _cachePathOverRide = null; 

		public readonly Version version = new(1, 0, 0);
#if DEBUG
		public const bool IS_MILK_SNAKE = true;
#else
		public const bool IS_MILK_SNAKE = false;
#endif
		public Engine() {
		}

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

		public Engine(string[] arg, OutputCapture outputCapture) : base() {
			_forceFlatscreen = arg.Any((v) => v.ToLower() == "--no-vr") | arg.Any((v) => v.ToLower() == "-no-vr");
			_noVRSim = arg.Any((v) => v.ToLower() == "--no-vr-sim") | arg.Any((v) => v.ToLower() == "-no-vr-sim");
			for (var i = 0; i < arg.Length; i++) {
				if(arg[i].ToLower() == "--cache-override" | arg[i].ToLower() == "-cache-override") {
					if (i + 1 <= arg.Length) {
						_cachePathOverRide = arg[i + 1];
					}
					else {
						Log.Err("Cache Path not specified");
					}
				}
			}
			if (arg.Length <= 0) {
				Log.Info($"Launched with no arguments");
			}
			else {
				Log.Info($"Launched with {(_forceFlatscreen ? "forceFlatscreen " : "")}{(_noVRSim ? "noVRSim " : "")}{((_cachePathOverRide != null) ? "Cache Override: " + _cachePathOverRide + " " : "")}");
			}
			this.outputCapture = outputCapture;
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
		public NetApiManager netApiManager = new();

		public WorldManager worldManager = new();

		public AssetManager assetManager;

		public OutputCapture outputCapture;

		public IManager[] _managers;

		public void Init() {
			Platform.ForceFallbackKeyboard = true;
			World.OcclusionEnabled = true;
			World.RaycastEnabled = true;
			assetManager = new AssetManager(_cachePathOverRide);
			_managers = new IManager[] { netApiManager, assetManager , worldManager };
			foreach (var item in _managers) {
				item.Init(this);
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