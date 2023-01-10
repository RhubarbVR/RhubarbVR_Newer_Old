
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
	public class RhuException : Exception
	{
		public RhuException(string data) : base(data) { }
	}

	public sealed class Engine : IDisposable
	{
		public bool PassErrors { get; private set; }
		public IEngineLink EngineLink { get; private set; }

		public readonly object RenderLock = new();

		private readonly string _cachePathOverRide = null;

		private readonly string _userDataPathOverRide = null;

		public readonly Version version = new(1, 0, 0);
#if DEBUG
		public const bool IS_MILK_SNAKE = true;
#else
		public const bool IS_MILK_SNAKE = false;
#endif

		public readonly string SettingsFile;
		public Action OnEngineStarted;

		public CommandManager commandManager;

		public Engine(IEngineLink _EngineLink, string[] arg, OutputCapture outputCapture, string baseDir = null, bool PassErrors = false) : base() {
			baseDir ??= AppDomain.CurrentDomain.BaseDirectory;
			EngineHelpers.BaseDir = baseDir;
			RhuConsole.ForegroundColor = ConsoleColor.White;
			this.PassErrors = PassErrors;
			EngineLink = _EngineLink;
			commandManager = new CommandManager();
			if (_EngineLink.ForceLibLoad) {
				OpusDotNet.NativeLib.ForceLoad();
			}
			_EngineLink.BindEngine(this);
			RLog.Info($"Platform Information OSArc: {RuntimeInformation.OSArchitecture} Framework: {RuntimeInformation.FrameworkDescription} OS: {RuntimeInformation.OSDescription} ProcessArc: {RuntimeInformation.ProcessArchitecture}");
			EngineLink.LoadStatics();
			EngineHelpers.MainEngine = this;
			string error = null;
			DebugVisuals = arg.Any((v) => v.ToLower() == "--debug-visuals") | arg.Any((v) => v.ToLower() == "-debug-visuals") | arg.Any((v) => v.ToLower() == "-debugvisuals");
			_EngineLink.LoadArgs();
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
			outputCapture.LogsPath = _userDataPathOverRide is null ? baseDir + "/Logs/" : _userDataPathOverRide + "/Logs/";
			outputCapture.Start();
			if (error is not null) {
				RLog.Err(error);
			}
			if (arg.Length <= 0) {
				RLog.Info($"Launched with no arguments");
			}
			else {
				RLog.Info($"Launched {((_cachePathOverRide != null) ? "Cache Override: " + _cachePathOverRide + " " : "")}{((_userDataPathOverRide != null) ? "UserData Override: " + _userDataPathOverRide + " " : "")}");
			}
			this.outputCapture = outputCapture;

			var lists = new List<DataList>();
			SettingsFile = ((_userDataPathOverRide is not null) ? _userDataPathOverRide : baseDir) + "/settings.json";
			if (File.Exists(SettingsFile)) {
				var text = File.ReadAllText(SettingsFile);
				var liet = SettingsManager.GetDataFromJson(text);
				lists.Add(liet);
			}
			if (!string.IsNullOrWhiteSpace(settingsArg)) {
				foreach (var item in settingsArg.Split(';')) {
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
			var theType = typeof(MainSettingsObject<NullRenderSettingsBase>);
			if (_EngineLink.RenderSettingsType is not null) {
				theType = typeof(MainSettingsObject<>).MakeGenericType(_EngineLink.RenderSettingsType);
			}
			var thedata = (MainSettingsObject)Activator.CreateInstance(theType);
			MainSettings = lists.Count == 0 ? thedata : SettingsManager.LoadSettingsObject(thedata, lists.ToArray());
			PindingRestart |= MainSettings.RenderSettings.RenderSettingsUpdate();
			if (EngineLink.CanRender) {
				RRenderer.Fov = thedata.Fov;
			}
		}

		public bool PindingRestart;

		public bool HasKeyboard => KeyboardInteraction is not null;

		public IKeyboardInteraction KeyboardInteraction { get; private set; }

		private void KeyBoardUpdate() {
			worldManager.PrivateSpaceManager?.KeyBoardUpdate(KeyboardInteraction?.WorldPos ?? Matrix.Identity);
		}

		public void KeyboardInteractionBind(IKeyboardInteraction uITextEditorInteraction) {
			if (KeyboardInteraction == uITextEditorInteraction) {
				KeyBoardUpdate();
				return;
			}
			KeyboardInteraction?.KeyboardUnBind();
			KeyboardInteraction = uITextEditorInteraction;
			KeyBoardUpdate();
		}

		public void KeyboardInteractionUnBind(IKeyboardInteraction uITextEditorInteraction) {
			if (KeyboardInteraction == uITextEditorInteraction) {
				KeyboardInteraction = null;
			}
			KeyBoardUpdate();
		}

		public void SaveSettings() {
			var data = SettingsManager.GetDataListFromSettingsObject(MainSettings, new DataList());
			File.WriteAllText(SettingsFile, SettingsManager.GetJsonFromDataList(data).ToString());
		}

		public event Action SettingsUpdate;

		public void UpdateSettings() {
			if (EngineLink.CanRender) {
				RRenderer.Fov = MainSettings.Fov;
			}
			SettingsUpdate?.Invoke();
		}

		public void ReloadSettings() {
			var lists = new List<DataList>();
			if (File.Exists(SettingsFile)) {
				var text = File.ReadAllText(SettingsFile);
				var liet = SettingsManager.GetDataFromJson(text);
				lists.Add(liet);
			}
			var theType = typeof(MainSettingsObject<NullRenderSettingsBase>);
			if (EngineLink.RenderSettingsType is not null) {
				theType = typeof(MainSettingsObject<>).MakeGenericType(EngineLink.RenderSettingsType);
			}
			var thedata = (MainSettingsObject)Activator.CreateInstance(theType);
			MainSettings = lists.Count == 0 ? thedata : SettingsManager.LoadSettingsObject(thedata, lists.ToArray());
			PindingRestart |= MainSettings.RenderSettings.RenderSettingsUpdate();
			UpdateSettings();
		}

		public string MainMic
		{
			get => MainSettings.MainMic;
			set {
				MainSettings.MainMic = value;
				MicChanged?.Invoke(value);
			}
		}

		public Action<string> MicChanged;

		public bool IsCloseing { get; set; }
		public bool DebugVisuals { get; private set; }

		public NetApiManager netApiManager;

		public WindowManager windowManager = new();

		public WorldManager worldManager = new();

		public AssetManager assetManager;

		public InputManager inputManager = new();

		public FileManager fileManager = new();

		public DiscordManager discordManager = new();

		public LocalisationManager localisationManager = new();

		public MainSettingsObject MainSettings;

		public OutputCapture outputCapture;

		public IManager[] _managers;

		public StaticResources staticResources = new();

		public string IntMsg = "Starting Engine";

		public bool EngineStarting = true;

		public IUnlitMaterial LoadingLogo;

		public Thread startingthread;

		public RText StartingText;
		public RTempQuad StartingLogo;

		public bool IsInVR => EngineLink.InVR;

		public void Init(bool RunStartThread = true) {
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			EngineLink.Start();
			commandManager.Init(this);
			IntMsg = $"Engine started Can Render {EngineLink.CanRender} Can Audio {EngineLink.CanAudio} Can input {EngineLink.CanInput}";
			RLog.Info(IntMsg);
			if (EngineLink.CanRender) {
				if (RRenderer.PassthroughSupport) {
					RLog.Info("Passthrough Supported");
				}
				else {
					RLog.Info("Passthrough not Supported");
				}
			}
			if (EngineLink.CanRender) {
				StartingText = new RText(staticResources.MainFont) {
					Text = "Starting",
					HorizontalAlignment = RHorizontalAlignment.Center,
					VerticalAlignment = RVerticalAlignment.Center,
				};
				RRenderer.EnableSky = false;
				StartingLogo = new RTempQuad();
				LoadingLogo = StaticMaterialManager.GetMaterial<IUnlitMaterial>();
				LoadingLogo.Transparency = Transparency.Blend;
				LoadingLogo.DullSided = true;
				LoadingLogo.Texture = staticResources.RhubarbLogoV2;
				StartingLogo.Material = LoadingLogo.Material;
			}
			var startcode = () => {
				IntMsg = "Building NetApiManager";
				netApiManager = new NetApiManager((_userDataPathOverRide ?? EngineHelpers.BaseDir) + "/rhuCookie");
				IntMsg = "Building AssetManager";
				assetManager = new AssetManager(_cachePathOverRide);
				_managers = new IManager[] { discordManager, windowManager, fileManager, localisationManager, inputManager, netApiManager, assetManager, worldManager };
				foreach (var item in _managers) {
					IntMsg = $"Starting {item.GetType().Name}";
					try {
						item.Init(this);
					}
					catch (Exception ex) {
						RLog.Err($"Failed to start {item.GetType().GetFormattedName()} Error:{ex}");
						IntMsg = $"Failed to start {item.GetType().GetFormattedName()} Error:{ex}";
						return;
					}
				}
				IntMsg = $"{localisationManager?.GetLocalString("Common.Loaded")}\nEngine Started";
				EngineStarting = false;
				if (EngineLink.CanRender) {
					RenderThread.ExecuteOnStartOfFrame(() => {
						StartingLogo?.Dispose();
						StartingLogo = null;
						StartingText?.Dispose();
						StartingText = null;
						LoadingLogo?.Dispose();
						LoadingLogo = null;
						RRenderer.EnableSky = true;
					});
				}
				RLog.Info("Engine Started");
				IntMsg = $"{localisationManager?.GetLocalString("Common.Loaded")}\nRunning First Step...";
				OnEngineStarted?.Invoke();
			};
			if (RunStartThread) {
				startingthread = new Thread(startcode.Invoke) {
					Priority = ThreadPriority.BelowNormal
				};
				startingthread.Start();
			}
			else {
				startcode.Invoke();
			}
		}

		private bool _mouseFree = true;

		public bool MouseFree
		{
			get => _mouseFree;
			set {
				_mouseFree = value;
				MouseFreeStateUpdate();
			}
		}

		public void MouseFreeStateUpdate() {
			if (inputManager?.MouseSystem is not null) {
				if (IsInVR) {
					inputManager.MouseSystem.MouseHidden = false;
					inputManager.MouseSystem.MouseLocked = false;
				}
				else {
					inputManager.MouseSystem.MouseHidden = _mouseFree;
					inputManager.MouseSystem.MouseLocked = _mouseFree;
				}
			}
		}


		private Vector3f _oldPlayerPos = Vector3f.Zero;
		private Vector3f _loadingPos = Vector3f.Zero;

		public void Step() {
			if (EngineStarting) {
				RenderThread.RunOnStartOfFrame();
				if (EngineLink.CanRender) {
					try {
						var headMat = IsInVR
							? Matrix.TR(inputManager.XRInputSystem.HeadPos, inputManager.XRInputSystem.HeadRot)
							: inputManager.ScreenHeadMatrix;
						var textpos = Matrix.T(Vector3f.Forward * 0.5f) * (EngineLink.CanInput ? headMat : Matrix.S(1));
						var playerPos = RRenderer.CameraRoot.Translation;
						_loadingPos += playerPos - _oldPlayerPos;
						_loadingPos += (Vector3f)(((Vector3d)textpos.Translation - (Vector3d)_loadingPos) * Math.Min(RTime.Elapsed * 5, 1));
						_oldPlayerPos = playerPos;
						var rootMatrix = Matrix.TR(_loadingPos, Quaternionf.LookAt(EngineLink.CanInput ? headMat.Translation : Vector3f.Zero, _loadingPos));
						if (StartingText is not null) {
							StartingText.Text = $"{localisationManager?.GetLocalString("Common.Loading")}\n{IntMsg}";
							StartingText.Pos = Matrix.TS(0, -0.2f, 0, new Vector3f(-0.1f, 0.1f, 0.1f)) * rootMatrix;
							if (LoadingLogo is not null) {
								StartingLogo.Pos = Matrix.TS(0, 0.06f, 0, 0.25f) * rootMatrix;
							}
						}
					}
					catch (Exception ex) {
						RLog.Err("Failed to update msg text Error: " + ex.ToString());
						throw;
					}
				}
				RenderThread.RunOnEndOfFrame();
				return;
			}
			RenderThread.RunOnStartOfFrame();
			GameStep();
			RenderStep();
			RenderThread.RunOnEndOfFrame();
		}

		public void RenderStep() {
			try {
				foreach (var item in _managers) {
					try {
						item.RenderStep();
					}
					catch (Exception ex) {
						RLog.Err($"Failed to render step {item.GetType().GetFormattedName()} Error: {ex}");
						throw;
					}
				}
			}
			catch (Exception wa) {
				RLog.Err("Render Step Error" + wa.ToString());
			}
		}

		public void GameStep() {
			try {
				RUpdateManager.RunOnStartOfFrame();
				foreach (var item in _managers) {
					try {
						item.Step();
					}
					catch (Exception ex) {
						RLog.Err($"Failed to step {item.GetType().GetFormattedName()} Error: {ex}");
						throw;
					}
				}
				RUpdateManager.RunOnEndOfFrame();
			}
			catch (Exception wa) {
				RLog.Err("GameStep Error" + wa.ToString());
			}
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
			RLog.Info("CleanUpTemp");
			TempFiles.CleanUpTempFiles();
			foreach (var item in _managers) {
				try {
					item.Dispose();
				}
				catch (Exception ex) {
					RLog.Err($"Failed to Disposed {item.GetType().GetFormattedName()} Error: {ex}");
					if (PassErrors) {
						throw;
					}
				}
			}

		}
	}
}