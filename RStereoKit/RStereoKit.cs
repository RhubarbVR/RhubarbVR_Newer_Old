using System;

using StereoKit;
using RhuEngine;
using RhuEngine.Linker;
using RhuEngine.Physics;
using RhuEngine.Managers;

namespace RStereoKit
{
	public sealed class RhuStereoKit : IEngineLink
	{
		public string BackendID => "StereoKit";

		public Engine engine;


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
					displayPreference = engine._forceFlatscreen ? StereoKit.DisplayMode.Flatscreen : StereoKit.DisplayMode.MixedReality,
					disableFlatscreenMRSim = engine._noVRSim,
					flatscreenHeight = 720,
					flatscreenWidth = 1280,
				};
			}
		}

		public bool SpawnPlayer => true;

		public bool CanRender => true;

		public bool CanAudio => true;
		public bool CanInput => true;

		public bool ForceLibLoad => false;

		public bool InVR => (SK.ActiveDisplayMode == DisplayMode.MixedReality) || !engine._noVRSim;
		public event Action<bool> VRChange;

		public bool LiveVRChange => false;

		public Type RenderSettingsType => typeof(SKRenderSettings);

		public SupportedFancyFeatures SupportedFeatures => SupportedFancyFeatures.Basic;

		public void ChangeVR(bool value) {
			throw new NotImplementedException();
		}
		public void BindEngine(Engine engine) {
			this.engine = engine;
			Renderer.OverrideCaptureFilter(true,(StereoKit.RenderLayer)(int)RhuEngine.Linker.RenderLayer.MainCam);
			RLog.Instance = new Logger();
		}

		public void ColorizeFingers(StereoKit.Handed hand, int size, Gradient horizontal, Gradient vertical) {
			var tex = new Tex(StereoKit.TexType.Image, StereoKit.TexFormat.Rgba32Linear) {
				AddressMode = StereoKit.TexAddress.Clamp
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

		public void LoadStatics() {
			World.RaycastEnabled = false;
			RTexture2D.Instance = new SKTexture2d();
			RMaterial.Instance = new SKRMaterial();
			RMaterial.ConstInstance = new SKMitStactic();
			RShader.Instance = new SKShader();
			RMesh.Instance = typeof(SKRMesh);
			RenderThread.ExecuteOnStartOfFrame(() => RMesh.Quad = new RMesh(new SKRMesh(Mesh.Quad), false));
			RTime.Instance = new SKTime();
			RRenderer.Instance = new SKRRenderer();
			StaticMaterialManager.Instanances = new StaticMitsManager();
			PhysicsHelper.RegisterPhysics<RBullet.BulletPhsyicsLink>();
		}

		public void Start() {
			ColorizeFingers(StereoKit.Handed.Right, 16,
	new Gradient(
		new GradientKey(Color.HSV(0.4f, 1, 0.5f), 0.5f)),
	new Gradient(
		new GradientKey(new Color(1, 1, 1, 0), 0),
		new GradientKey(new Color(1, 1, 1, 0), 0.4f),
		new GradientKey(new Color(1, 1, 1, 1), 0.9f)));
			ColorizeFingers(StereoKit.Handed.Left, 16,
				new Gradient(
					new GradientKey(Color.HSV(1f, 1, 0.5f), 0.5f)),
				new Gradient(
					new GradientKey(new Color(1, 1, 1, 0), 0),
					new GradientKey(new Color(1, 1, 1, 0), 0.4f),
					new GradientKey(new Color(1, 1, 1, 1), 0.9f)));
		}

		public void LoadArgs() {
		}

		public void LoadInput(InputManager manager) {
			manager.LoadInputDriver<SKKeyboardDriver>();
			manager.LoadInputDriver<SKMosueDriver>();
		}
	}
}
