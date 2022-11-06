using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhubarbVR.Bindings.FontBindings;
using RhubarbVR.Bindings.Input;
using RhubarbVR.Bindings.TextureBindings;

using RhuEngine;
using RhuEngine.Linker;
using RhuEngine.Managers;
using RhuEngine.Settings;

using RNumerics;

using Engine = RhuEngine.Engine;

namespace RhubarbVR.Bindings
{
	public class GodotRenderSettings : RenderSettingsBase
	{
	}


	public class GodotEngineLink : IEngineLink
	{
		public GodotEngineLink(EngineRunner engineRunner) {
			EngineRunner = engineRunner;
		}

		public bool ForceLibLoad => false;

		public bool SpawnPlayer => true;

		public bool CanRender => true;

		public bool CanAudio => true;

		public bool CanInput => true;

		public string BackendID => "Godot1.0.0";

		public bool InVR { get; set; }

		public bool LiveVRChange { get; set; }

		public Type RenderSettingsType => typeof(GodotRenderSettings);

		public EngineRunner EngineRunner { get; }

		public event Action<bool> VRChange;

		public Engine Engine;

		private void VRStateUpdate() {
			if (XRServer.PrimaryInterface?.IsInitialized() ?? false) {
				EngineRunner.GetViewport().UseXr = true;
				RLog.Info("Is in VR");
				InVR = true;
				LiveVRChange |= InVR;
				VRChange?.Invoke(true);
				Engine.MouseFreeStateUpdate();
			}
			else {
				EngineRunner.GetViewport().UseXr = false;
				RLog.Info("Not in VR");
				InVR = false;
				LiveVRChange |= InVR;
				VRChange?.Invoke(false);
				Engine.MouseFreeStateUpdate();
			}
		}

		private void LoadAllTrackers() {
			var trackers = XRServer.GetTrackers((int)XRServer.TrackerType.Any);
			foreach (var item in trackers) {
				XRServer_TrackerAdded((string)item.Key, (long)((XRPositionalTracker)item.Value).Type);
			}
		}

		public void BindEngine(Engine engine) {
			Engine = engine;
			VRStateUpdate();
			engine.inputManager.OnLoaded(LoadVRInput);
		}

		private void LoadVRInput() {
			RLog.Info("Loading VRInput");
			XRServer.TrackerAdded += XRServer_TrackerAdded;
			XRServer.TrackerRemoved += XRServer_TrackerRemoved;
			if (XRServer.PrimaryInterface is OpenXRInterface xRInterface) {
				xRInterface.SessionStopping += XRInterface_SessionStopping;
				xRInterface.SessionFocussed += XRInterface_SessionFocussed;
			}
			LoadAllTrackers();
		}

		private void XRInterface_SessionFocussed() {
			RLog.Info("XR session Focused");
		}

		private void XRInterface_SessionStopping() {
			RLog.Info("XR session Closed closing");
			Engine.Close();
		}

		public readonly Dictionary<string, GodotXRTracker> Trackers = new();

		private void XRServer_TrackerRemoved(StringName trackerName, long type) {
			if (Trackers.ContainsKey(trackerName)) {
				Engine.inputManager.RemoveInputDriver(Trackers[trackerName]);
			}
			else {
				RLog.Warn("Tracker Not Found to Removed Name:" + trackerName + " Type:" + (XRServer.TrackerType)type);
			}
		}

		private void XRServer_TrackerAdded(StringName trackerName, long type) {
			if (Trackers.ContainsKey(trackerName)) {
				RLog.Warn("Tracker Allready added Name:" + trackerName + " Type:" + (XRServer.TrackerType)type);
			}
			else {
				var newTracker = new GodotXRTracker(XRServer.GetTracker(trackerName));
				Trackers.Add(trackerName, newTracker);
				Engine.inputManager.LoadInputDriver(newTracker);
			}
		}

		public void ChangeVR(bool value) {
			if (value != InVR) {
				if (InVR) {
					//WTF
					XRServer.PrimaryInterface?.Uninitialize();
					XRServer.PrimaryInterface?.Initialize();
					RLog.Info("Uninitialize VR");
				}
				else {
					XRServer.PrimaryInterface?.Initialize();
					RLog.Info("Initialize VR");
				}
				VRStateUpdate();
			}
		}

		public void LoadArgs() {
			if (Engine._forceFlatscreen && InVR) {
				ChangeVR(false);
			}
		}

		public void LoadInput(InputManager manager) {
			manager.LoadInputDriver<GodotKeyboard>();
			manager.LoadInputDriver<GodotMouse>();
		}

		public void LoadStatics() {
			RTime.Instance = EngineRunner;
			RFont.Instance = typeof(GodotFont);
			RText.Instance = typeof(GodotTextRender);
			RMesh.Instance = typeof(GodotMesh);
			RTexture.Instance = typeof(GodotTexture);
			RTexture2D.Instance = typeof(GodotTexture2D);
			RAtlasTexture.Instance = typeof(GodotAtlasTexture);
			RImageTexture2D.Instance = typeof(GodotImageTexture2D);
			RImage.Instance = typeof(GodotImage);
			RRenderer.Instance = new GodotRender(EngineRunner);
			RMaterial.Instance = new GoMat();
			StaticMaterialManager.Instanances = new GodotStaticMats();
			var image = new RImage(null);
			image.Create(2, 2, false, RFormat.Rgb8);
			RTexture2D.White = new RImageTexture2D(image);
			RTempQuad.Instance = typeof(GodotTempMeshRender);
			new RBullet.BulletPhsyicsLink().RegisterPhysics();
			Engine.windowManager.windowManagerLink = new GodotWindowManager();
			if (EngineRunner._.GetViewport() is Window window) {
				Engine.windowManager.LoadWindow(new GodotWindow(window));
			}
		}

		public void Start() {

		}
	}
}
