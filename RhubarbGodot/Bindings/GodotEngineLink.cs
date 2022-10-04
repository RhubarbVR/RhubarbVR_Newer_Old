using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhubarbVR.Bindings.Input;
using RhubarbVR.Bindings.TextureBindings;

using RhuEngine;
using RhuEngine.Linker;
using RhuEngine.Managers;
using RhuEngine.Settings;

using RNumerics;

namespace RhubarbVR.Bindings
{
	public class GodotRenderSettings : RenderSettingsBase {

	}


	public class GodotEngineLink : IEngineLink
	{
		public GodotEngineLink(EngineRunner engineRunner) {
			EngineRunner = engineRunner;
		}

		public SupportedFancyFeatures SupportedFeatures => SupportedFancyFeatures.Basic;

		public bool ForceLibLoad => false;

		public bool SpawnPlayer => true;

		public bool CanRender => true;

		public bool CanAudio => true;

		public bool CanInput => true;

		public string BackendID => "Godot1.0.0";

		public bool InVR => false;

		public bool LiveVRChange => false;

		public Type RenderSettingsType => typeof(GodotRenderSettings);

		public EngineRunner EngineRunner { get; }

		public event Action<bool> VRChange;

		public Engine Engine;

		public void BindEngine(Engine engine) {
			Engine = engine;
		}

		public void ChangeVR(bool value) {

		}

		public void LoadArgs() {
			if (Engine._forceFlatscreen) {

			} else {

			}
		}

		public void LoadInput(InputManager manager) {
			manager.LoadInputDriver<GodotKeyboard>();
			manager.LoadInputDriver<GodotMouse>();
		}

		public void LoadStatics() {
			RTime.Instance = EngineRunner;
			RMesh.Instance = typeof(GodotMesh);
			RTexture.Instance = typeof(GodotTexture);
			RTexture2D.Instance = typeof(GodotTexture2D);
			RImageTexture2D.Instance = typeof(GodotImageTexture2D);
			RImage.Instance = typeof(GodotImage);
			RRenderer.Instance = new GodotRender(EngineRunner);
			RMaterial.Instance = new GoMat();
			StaticMaterialManager.Instanances = new GodotStaticMats();
			var image = new RImage(null);
			image.Create(2, 2, false, RFormat.Rgb8);
			RTexture2D.White = new RImageTexture2D(image);
			RMesh.Quad = new RMesh(new GodotMesh(GodotMesh.MakeQuad()), false);
			new RBullet.BulletPhsyicsLink().RegisterPhysics();
		}

		public void Start() {

		}
	}
}
