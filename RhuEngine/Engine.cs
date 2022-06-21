﻿
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
using System.Threading;
using RhuEngine.Components;
using System.Runtime;

namespace RhuEngine
{
	public class Engine : IDisposable
	{
		public bool PassErrors { get; private set; }
		public IEngineLink EngineLink { get; private set; }

		public readonly object RenderLock = new();

		public readonly bool _forceFlatscreen = false;

		public readonly bool _buildMissingLocal = false;

		public readonly bool _noVRSim = true;

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

		public CommandManager commandManager;

		public Engine(IEngineLink _EngineLink, string[] arg, OutputCapture outputCapture, string baseDir = null,bool PassErrors = false) : base() {
			Console.ForegroundColor = ConsoleColor.White;
			this.PassErrors = PassErrors;
			EngineLink = _EngineLink;
			commandManager = new CommandManager();
			if (_EngineLink.ForceLibLoad) {
				OpusDotNet.NativeLib.ForceLoad();
			}
			_EngineLink.BindEngine(this);
			RLog.Info($"Platform Information OSArc: {RuntimeInformation.OSArchitecture} Framework: {RuntimeInformation.FrameworkDescription} OS: {RuntimeInformation.OSDescription} ProcessArc: {RuntimeInformation.ProcessArchitecture}");
			EngineLink.LoadStatics();
			if (baseDir is null) {
				baseDir = AppDomain.CurrentDomain.BaseDirectory;
			}
			else {
				BaseDir = baseDir;
			}
			MainEngine = this;
			string error = null;
			_buildMissingLocal = arg.Any((v) => v.ToLower() == "--build-missing-local") | arg.Any((v) => v.ToLower() == "-build-missing-local") | arg.Any((v) => v.ToLower() == "-buildmissinglocal");
			_forceFlatscreen = arg.Any((v) => v.ToLower() == "--no-vr") | arg.Any((v) => v.ToLower() == "-no-vr") | arg.Any((v) => v.ToLower() == "-novr");
			_noVRSim = !(arg.Any((v) => v.ToLower() == "--vr-sim") | arg.Any((v) => v.ToLower() == "-vr-sim") | arg.Any((v) => v.ToLower() == "-vrsim"));
			DebugVisuals = arg.Any((v) => v.ToLower() == "--debug-visuals") | arg.Any((v) => v.ToLower() == "-debug-visuals") | arg.Any((v) => v.ToLower() == "-debugvisuals");
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

		public bool HasNoKeyboard => KeyboardInteraction is not null;

		public IKeyboardInteraction KeyboardInteraction { get;private set; }

		public void KeyboardInteractionBind(IKeyboardInteraction uITextEditorInteraction) {
			KeyboardInteraction?.KeyboardUnBind();
			KeyboardInteraction = uITextEditorInteraction;
		}

		public void KeyboardInteractionUnBind(IKeyboardInteraction uITextEditorInteraction) {
			if(KeyboardInteraction == uITextEditorInteraction) {
				KeyboardInteraction = null;
			}
		}

		public void SaveSettings() {
			var data = SettingsManager.GetDataListFromSettingsObject(MainSettings, new DataList());
			File.WriteAllText(SettingsFile, SettingsManager.GetJsonFromDataList(data).ToString());
		}

		public event Action SettingsUpdate;

		public void UpdateSettings() {
			SettingsUpdate?.Invoke();
		}

		public void ReloadSettings() {
			var lists = new List<DataList>();
			if (File.Exists(SettingsFile)) {
				var text = File.ReadAllText(SettingsFile);
				var liet = SettingsManager.GetDataFromJson(text);
				lists.Add(liet);
			}
			MainSettings = lists.Count == 0 ? new MainSettingsObject() : SettingsManager.LoadSettingsObject<MainSettingsObject>(lists.ToArray());
			UpdateSettings();
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
		public bool DebugVisuals { get; private set; }

		public NetApiManager netApiManager;

		public WorldManager worldManager = new();

		public AssetManager assetManager;

		public InputManager inputManager = new();

		public LocalisationManager localisationManager = new();

		public MainSettingsObject MainSettings;

		public OutputCapture outputCapture;

		public IManager[] _managers;

		public StaticResources staticResources = new();

		public string IntMsg = "Starting Engine";

		public bool EngineStarting = true;

		public RMaterial LoadingLogo = null;

		public Thread startingthread;

		public RText StartingText;
		public RMaterial StartingTextMit;

		public void Init(bool RunStartThread = true) {
			EngineLink.Start();
			commandManager.Init(this);
			IntMsg = $"Engine started Can Render {EngineLink.CanRender} Can Audio {EngineLink.CanAudio} Can input {EngineLink.CanInput}";
			RLog.Info(IntMsg);
			if (EngineLink.CanRender) {
				StartingText = new RText(staticResources.MainFont) {
					Text = "Starting"
				};
				StartingTextMit = new RMaterial(RShader.UnlitClip);
				StartingTextMit[RMaterial.Transparency] = Transparency.Blend;
				StartingTextMit[RMaterial.MainTexture] = StartingText.texture2D;
				StartingText.UpdatedTexture += () => StartingTextMit[RMaterial.MainTexture] = StartingText.texture2D;
				RRenderer.EnableSky = false;
				LoadingLogo = new RMaterial(RShader.UnlitClip);
				LoadingLogo[RMaterial.MainTexture] = staticResources.RhubarbLogoV2;
			}
			var startcode = () => {
				IntMsg = "Building NetApiManager";
				netApiManager = new NetApiManager(_userDataPathOverRide);
				IntMsg = "Building AssetManager";
				assetManager = new AssetManager(_cachePathOverRide);
				_managers = new IManager[] {localisationManager, inputManager, netApiManager, assetManager, worldManager };
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
				IntMsg = $"{localisationManager?.GetLocalString("Common.Loaded")}\nRunning First Step...";
				OnEngineStarted?.Invoke();
			};
			if (RunStartThread) {
				startingthread = new Thread(startcode.Invoke) {
					Priority = ThreadPriority.AboveNormal
				};
				startingthread.Start();
			}
			else {
				startcode.Invoke();
			}
		}

		private Vector3f _oldPlayerPos = Vector3f.Zero;
		private Vector3f _loadingPos = Vector3f.Zero;

		public void Step() {
			if (EngineStarting) {
				if (EngineLink.CanRender) {
					try {
						var headMat = RInput.Head.HeadMatrix;
						if (!RWorld.IsInVR) {
							RRenderer.CameraRoot = Matrix.Identity;
							headMat = Matrix.T(Vector3f.Forward / 10);
						}
						var textpos = Matrix.T(Vector3f.Forward * 0.5f) * (EngineLink.CanInput ? headMat : Matrix.S(1));
						var playerPos = RRenderer.CameraRoot.Translation;
						_loadingPos += playerPos - _oldPlayerPos;
						_loadingPos += (textpos.Translation - _loadingPos) * Math.Min(RTime.Elapsedf * 5f, 1);
						_oldPlayerPos = playerPos;
						var rootMatrix = Matrix.TR(_loadingPos,Quaternionf.LookAt(EngineLink.CanInput ? headMat.Translation : Vector3f.Zero, _loadingPos));
						StartingText.Text = $"{localisationManager?.GetLocalString("Common.Loading")}\n{IntMsg}";
						RMesh.Quad.Draw("UIText", StartingTextMit, Matrix.TS(0, -0.2f, 0,new Vector3f(StartingText.AspectRatio,1,1)/7) * rootMatrix);
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

		public event Action OnCloseEngine;

		public bool IsClosing;

		public void Close() {
			if (IsClosing) {
				return;
			}
			IsCloseing = true;
			OnCloseEngine?.Invoke();
		}

		public void Dispose() {
			RLog.Info("Engine Disposed");
			SaveSettings();
			foreach (var item in _managers) {
				try {
					item.Dispose();
				}
				catch (Exception ex) {
					RLog.Err($"Failed to Disposed {item.GetType().GetFormattedName()} Error: {ex}");
					if (PassErrors) {
						throw ex;
					}
				}
			}
		}
	}
}