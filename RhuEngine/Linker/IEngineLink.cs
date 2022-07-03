using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Linker
{
	[Flags]
	public enum SupportedFancyFeatures : ulong
	{
		Basic = 0,
		Lighting = 1,
		GlobalIllumination = 2,
		MeshRenderShadowSettings = 4,
		LightCookie = 8,
		LightHalo = 16,
		NativeSkinnedMesheRender = 32,
		Camera = 64,
		ReflectionProbes = 128,
		BasicParticleSystem = 256,
		AdvancedParticleSystem = 512,
		PhysicalCamera = 1024,
		CalledCameraRender = 2048,
		LightProbeGroup = 4096,

	}

	public interface IEngineLink
	{
		SupportedFancyFeatures SupportedFeatures { get; }
		bool ForceLibLoad { get; }
		bool SpawnPlayer { get; }
		bool CanRender { get; }
		bool CanAudio { get; }
		bool CanInput { get; }
		string BackendID { get; }
		bool InVR { get; }

		event Action<bool> VRChange;

		bool LiveVRChange { get; }

		void ChangeVR(bool value);

		Type RenderSettingsType { get; }
		void BindEngine(Engine engine);
		void Start();
		void LoadStatics();

	}
}
