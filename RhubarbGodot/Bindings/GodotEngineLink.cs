using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine;
using RhuEngine.Linker;
using RhuEngine.Managers;
using RhuEngine.Settings;

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

		public bool CanRender => false;

		public bool CanAudio => false;

		public bool CanInput => false;

		public string BackendID => "Godot";

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

			}
			else {

			}
		}

		public void LoadInput(InputManager manager) {

		}

		public void LoadStatics() {
			RTime.Instance = EngineRunner;
			new RBullet.BulletPhsyicsLink(true).RegisterPhysics();
		}

		public void Start() {

		}
	}
}
