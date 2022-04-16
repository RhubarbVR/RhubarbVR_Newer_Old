
using System;
using System.Linq;

using RhuEngine.Managers;

using RhuEngine.Settings;
using System.IO;
using RhuSettings;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Reflection;
using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine
{
	public class Engine : IDisposable
	{
		public IEngineLink EngineLink { get; private set; }

		public readonly object RenderLock = new();

		public readonly bool _forceFlatscreen = false;

		public readonly bool _noVRSim = false;

		private readonly string _cachePathOverRide = null;

		private readonly string _userDataPathOverRide = null;

		public readonly Version version = new(1, 0, 0);
#if DEBUG
		public const bool IS_MILK_SNAKE = true;
#else
		public const bool IS_MILK_SNAKE = false;
#endif

		public readonly string SettingsFile;

		public static Engine MainEngine;

		public static string BaseDir = AppDomain.CurrentDomain.BaseDirectory;

		public Action OnEngineStarted;

		public Engine(IEngineLink _EngineLink, string[] arg, OutputCapture outputCapture, string baseDir = null) : base() {
			EngineLink = _EngineLink;
			_EngineLink.BindEngine(this);
			EngineLink.LoadStatics();
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
				RLog.Err(error);
			}
			if (arg.Length <= 0) {
				RLog.Info($"Launched with no arguments");
			}
			else {
				RLog.Info($"Launched with {(_forceFlatscreen ? "forceFlatscreen " : "")}{(_noVRSim ? "noVRSim " : "")}{((_cachePathOverRide != null) ? "Cache Override: " + _cachePathOverRide + " " : "")}{((_userDataPathOverRide != null) ? "UserData Override: " + _userDataPathOverRide + " " : "")}");
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
						RLog.Err("Error loading settings ERROR:" + e.ToString());
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

		public bool IsCloseing { get; set; }

		public NetApiManager netApiManager;

		public WorldManager worldManager = new();

		public AssetManager assetManager;

		public InputManager inputManager = new();

		public MainSettingsObject MainSettings;

		public OutputCapture outputCapture;

		public IManager[] _managers;

		public StaticResources staticResources = new();

		public string IntMsg = "Starting Engine";

		public bool EngineStarting = true;

		public RMaterial LoadingLogo = null;


		public void Init() {
			EngineLink.Start();
			IntMsg = $"Engine started Can Render {EngineLink.CanRender} Can Audio {EngineLink.CanAudio} Can input {EngineLink.CanInput}";
			RLog.Info(IntMsg);
			if (EngineLink.CanRender) {
				RRenderer.EnableSky = false;
				LoadingLogo = new RMaterial(RShader.UnlitClip);
				LoadingLogo["diffuse"] = staticResources.RhubarbLogoV2;
			}
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
						RLog.Err($"Failed to start {item.GetType().GetFormattedName()} Error:{ex}");
						IntMsg = $"Failed to start {item.GetType().GetFormattedName()} Error:{ex}";
						throw ex;
					}
				}
				if (EngineLink.CanRender) {
					EngineStarting = false;
					LoadingLogo = null;
					RRenderer.EnableSky = true;
				}
				RLog.Info("Engine Started");
				OnEngineStarted?.Invoke();
		});
		}

		private Vector3f _oldPlayerPos = Vector3f.Zero;
		private Vector3f _loadingPos = Vector3f.Zero;

		public void Step() {
			if (EngineStarting) {
				if (EngineLink.CanRender) {
					try {
						var textpos = Matrix.T(Vector3f.Forward * 0.25f) * (EngineLink.CanInput ? RInput.Head.HeadMatrix : Matrix.S(1));
						var playerPos = RRenderer.CameraRoot.Translation;
						_loadingPos += playerPos - _oldPlayerPos;
						_loadingPos += (textpos.Translation - _loadingPos) * Math.Min(RTime.Elapsedf * 5f, 1);
						_oldPlayerPos = playerPos;
						var rootMatrix = Matrix.TR(_loadingPos,Quaternionf.LookAt((EngineLink.CanInput ? RInput.Head.Position : Vector3f.Zero), _loadingPos));
						RText.Add($"Loading Engine\n{IntMsg}...", Matrix.T(0, -0.07f, 0) * rootMatrix);
						RMesh.Quad.Draw("LoadingUi",LoadingLogo, Matrix.TS(0, 0.06f, 0, 0.25f) * rootMatrix);
					}
					catch (Exception ex) {
						RLog.Err("Failed to update msg text Error: " + ex.ToString());
						throw ex;
					}
				}
				return;
			}
			RWorld.RunOnStartOfFrame();
			foreach (var item in _managers) {
				try {
					item.Step();
				}
				catch (Exception ex) {
					RLog.Err($"Failed to step {item.GetType().GetFormattedName()} Error: {ex}");
					throw ex;
				}
			}
			RWorld.RunOnEndOfFrame();
		}

		public void Dispose() {
			RLog.Info("Engine Disposed");
			foreach (var item in _managers) {
				try {
					item.Dispose();
				}
				catch (Exception ex) {
					RLog.Err($"Failed to Disposed {item.GetType().GetFormattedName()} Error: {ex}");
				}
			}
		}
	}
}