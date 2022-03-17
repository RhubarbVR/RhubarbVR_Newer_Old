
using System;
using System.Linq;

using RhuEngine.Managers;

using StereoKit;
using RhuEngine.Settings;
using System.IO;
using RhuSettings;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Reflection;

namespace RhuEngine
{
	public class Engine : IDisposable
	{
		public readonly object RenderLock = new();

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
		public readonly static UISettings DefaultSettings = new() {
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

		public static Engine MainEngine;

		public static string BaseDir = AppDomain.CurrentDomain.BaseDirectory;

		public Engine(string[] arg, OutputCapture outputCapture, string baseDir = null) : base() {
			if (baseDir is null) {
				baseDir = AppDomain.CurrentDomain.BaseDirectory;
			}
			else {
				BaseDir = baseDir;
			}
			MainEngine = this;
			string error = null;
			_forceFlatscreen = arg.Any((v) => v.ToLower() == "--no-vr") | arg.Any((v) => v.ToLower() == "-no-vr");
			_noVRSim = arg.Any((v) => v.ToLower() == "--no-vr-sim") | arg.Any((v) => v.ToLower() == "-no-vr-sim");
			string settingsArg = null;
			for (var i = 0; i < arg.Length; i++) {
				if (arg[i].ToLower() == "--cache-override" | arg[i].ToLower() == "-cache-override") {
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
			outputCapture.LogsPath = _userDataPathOverRide is null ? baseDir + "\\Logs\\" : _userDataPathOverRide + "\\Logs\\";
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
			SettingsFile = ((_userDataPathOverRide is not null) ? _userDataPathOverRide : baseDir) + "\\settings.json";
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
			var data = SettingsManager.GetDataListFromSettingsObject(MainSettings, new DataList());
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

		public bool IsCloseing { get; set; }

		public NetApiManager netApiManager;

		public WorldManager worldManager = new();

		public AssetManager assetManager;

		public InputManager inputManager = new();

		//public WebBrowserManager webBrowserManager = new();

		public MainSettingsObject MainSettings;

		public OutputCapture outputCapture;

		public IManager[] _managers;

		public TextStyle MainTextStyle;

		public StaticResources staticResources = new();

		public string IntMsg = "Starting Engine";

		public bool EngineStarting = true;

		public Material LoadingLogo = null;
		public void ColorizeFingers(Handed hand,int size, Gradient horizontal, Gradient vertical) {
			var tex = new Tex(TexType.Image, TexFormat.Rgba32Linear) {
				AddressMode = TexAddress.Clamp
			};
			var pixels = new Color32[size * size];
			for (var y = 0; y < size; y++) {
				var v = vertical.Get(1 - (y / (size - 1.0f)));
				for (var x = 0; x < size; x++) {
					var h = horizontal.Get(x / (size - 1.0f));
					pixels[x + (y * size)] = v * h;
				}
			}
			tex.SetColors(size, size, pixels);
			var copyhand = Default.MaterialHand.Copy();
			copyhand[MatParamName.DiffuseTex] = tex;
			Input.Hand(hand).Material = copyhand;
		}

		public void Init() {
			Renderer.EnableSky = false;
			LoadingLogo = new Material(Shader.UnlitClip);
			LoadingLogo.SetTexture("diffuse", staticResources.RhubarbLogoV2);
			MainTextStyle = Text.MakeStyle(Font.Default, 0.02f, new Color(0.890f, 0.580f, 0.027f));
			Platform.ForceFallbackKeyboard = true;
			World.OcclusionEnabled = true;
			World.RaycastEnabled = true;
			ColorizeFingers(Handed.Left,16,
				new Gradient(
					new GradientKey(Color.HSV(0.4f, 1, 0.5f), 0.5f)),
				new Gradient(
					new GradientKey(new Color(1, 1, 1, 0), 0),
					new GradientKey(new Color(1, 1, 1, 0), 0.4f),
					new GradientKey(new Color(1, 1, 1, 1), 0.9f)));
			ColorizeFingers(Handed.Right, 16,
				new Gradient(
					new GradientKey(Color.HSV(1f, 1, 0.5f), 0.5f)),
				new Gradient(
					new GradientKey(new Color(1, 1, 1, 0), 0),
					new GradientKey(new Color(1, 1, 1, 0), 0.4f),
					new GradientKey(new Color(1, 1, 1, 1), 0.9f)));
			Task.Run(() => {
				IntMsg = "Building NetApiManager";
				netApiManager = new NetApiManager(_userDataPathOverRide);
				IntMsg = "Building AssetManager";
				assetManager = new AssetManager(_cachePathOverRide);
				_managers = new IManager[] { inputManager, netApiManager, assetManager, worldManager };
				foreach (var item in _managers) {
					IntMsg = $"Starting {item.GetType().Name}";
					try {
						item.Init(this);
					}
					catch (Exception ex) {
						Log.Err($"Failed to start {item.GetType().GetFormattedName()} Error:{ex}");
						IntMsg = $"Failed to start {item.GetType().GetFormattedName()} Error:{ex}";
						throw new Exception("LockLoad");
					}
				}
				EngineStarting = false;
				LoadingLogo = null;
				Renderer.EnableSky = true;
				Log.Info("Engine Started");
			});
		}

		private Vec3 _oldPlayerPos = Vec3.Zero;
		private Vec3 _loadingPos = Vec3.Zero;

		public void Step() {
			if (EngineStarting) {
				try {
					var textpos = Matrix.T(Vec3.Forward * 0.25f) * Input.Head.ToMatrix();
					var playerPos = Renderer.CameraRoot.Translation;
					_loadingPos += playerPos - _oldPlayerPos;
					_loadingPos += (textpos.Translation - _loadingPos) * Math.Min(Time.Elapsedf * 5f, 1);
					_oldPlayerPos = playerPos;
					var rootMatrix = new Pose(_loadingPos, Quat.LookAt(_loadingPos, Input.Head.position)).ToMatrix();
					Text.Add($"Loading Engine\n{IntMsg}...", Matrix.T(0, -0.07f, 0) * rootMatrix);
					Mesh.Quad.Draw(LoadingLogo, Matrix.TS(0, 0.06f, 0, 0.25f) * rootMatrix);
				}
				catch (Exception ex) {
					Log.Err("Failed to update msg text Error: " + ex.ToString());
				}
				return;
			}
			foreach (var item in _managers) {
				try {
					item.Step();
				}
				catch (Exception ex) {
					Log.Err($"Failed to step {item.GetType().GetFormattedName()} Error: {ex}");
				}
			}
		}

		public void Dispose() {
			Log.Info("Engine Disposed");
			foreach (var item in _managers) {
				try {
					item.Dispose();
				}
				catch (Exception ex) {
					Log.Err($"Failed to Disposed {item.GetType().GetFormattedName()} Error: {ex}");
				}
			}
		}
	}
}