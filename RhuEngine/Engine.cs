
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
			_forceFlatscreen = arg.Any((v) => v.ToLower() == "--no-vr");
			_noVRSim = arg.Any((v) => v.ToLower() == "--no-vr-sim");
			this.outputCapture = outputCapture;
		}

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

		public OutputCapture outputCapture;

		public IManager[] _managers;

		public void Init() {
			_managers = new IManager[] { worldManager, netApiManager };
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