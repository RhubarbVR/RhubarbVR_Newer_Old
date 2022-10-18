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

		public bool LiveVRChange => true;

		public Type RenderSettingsType => typeof(GodotRenderSettings);

		public EngineRunner EngineRunner { get; }

		public event Action<bool> VRChange;

		public Engine Engine;

		private void VRStateUpdate() {
			if (XRServer.PrimaryInterface?.IsInitialized() ?? false) {
				EngineRunner.GetViewport().UseXr = true;
				RLog.Info("Is in VR");
				InVR = true;
				VRChange?.Invoke(true);
				Engine.MouseFreeStateUpdate();
			}
			else {
				EngineRunner.GetViewport().UseXr = false;
				RLog.Info("Not in VR");
				InVR = false;
				VRChange?.Invoke(false);
				Engine.MouseFreeStateUpdate();
			}
		}

		public void BindEngine(Engine engine) {
			Engine = engine;
			VRStateUpdate();
		}

		public void ChangeVR(bool value) {
			if (value != InVR) {
				if (InVR) {
					XRServer.PrimaryInterface.Uninitialize();
					RLog.Info("Uninitialize VR");
				}
				else {
					//Dont know why it works but it does
					XRServer.PrimaryInterface.Initialize();
					XRServer.PrimaryInterface.Initialize();
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
		}

		public void Start() {

		}
	}
}
