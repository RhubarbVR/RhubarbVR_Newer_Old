using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RMsaa
	{
		Disabled,
		TwoX,
		FourX,
		EightX,
	}

	public enum RClearMode
	{
		Never,
		Always,
	}

	public enum RScreenSpaceAA
	{
		Disabled,
		Fxaa
	}

	public enum RUpdateMode
	{
		Disable,
		InputUpdate,
		Always,
	}

	public enum RDebugDraw
	{
		//
		// Summary:
		//     Objects are displayed normally.
		Disabled,
		//
		// Summary:
		//     Objects are displayed without light information.
		Unshaded,
		Lighting,
		//
		// Summary:
		//     Objects are displayed semi-transparent with additive blending so you can see
		//     where they are drawing over top of one another. A higher overdraw means you are
		//     wasting performance on drawing pixels that are being hidden behind others.
		Overdraw,
		//
		// Summary:
		//     Objects are displayed in wireframe style.
		Wireframe,
		NormalBuffer,
		//
		// Summary:
		//     Objects are displayed with only the albedo value from Godot.VoxelGIs.
		VoxelGiAlbedo,
		//
		// Summary:
		//     Objects are displayed with only the lighting value from Godot.VoxelGIs.
		VoxelGiLighting,
		//
		// Summary:
		//     Objects are displayed with only the emission color from Godot.VoxelGIs.
		VoxelGiEmission,
		//
		// Summary:
		//     Draws the shadow atlas that stores shadows from Godot.OmniLight3Ds and Godot.SpotLight3Ds
		//     in the upper left quadrant of the Godot.Viewport.
		ShadowAtlas,
		//
		// Summary:
		//     Draws the shadow atlas that stores shadows from Godot.DirectionalLight3Ds in
		//     the upper left quadrant of the Godot.Viewport.
		DirectionalShadowAtlas,
		SceneLuminance,
		//
		// Summary:
		//     Draws the screen-space ambient occlusion texture instead of the scene so that
		//     you can clearly see how it is affecting objects. In order for this display mode
		//     to work, you must have Godot.Environment.SsaoEnabled set in your Godot.WorldEnvironment.
		Ssao,
		//
		// Summary:
		//     Draws the screen-space indirect lighting texture instead of the scene so that
		//     you can clearly see how it is affecting objects. In order for this display mode
		//     to work, you must have Godot.Environment.SsilEnabled set in your Godot.WorldEnvironment.
		Ssil,
		//
		// Summary:
		//     Colors each PSSM split for the Godot.DirectionalLight3Ds in the scene a different
		//     color so you can see where the splits are. In order, they will be colored red,
		//     green, blue, and yellow.
		PssmSplits,
		//
		// Summary:
		//     Draws the decal atlas used by Godot.Decals and light projector textures in the
		//     upper left quadrant of the Godot.Viewport.
		DecalAtlas,
		Sdfgi,
		SdfgiProbes,
		GiBuffer,
		DisableLod,
		ClusterOmniLights,
		ClusterSpotLights,
		ClusterDecals,
		ClusterReflectionProbes,
		Occluders,
		MotionVectors
	}

	public enum RScaling3D
	{
		Billinear,
		FSR,
	}

	public enum RTextureFilter
	{
		Nearest,
		Linear,
		LinearMipmap,
		NearestMipmap
	}

	public enum RTextureRepeat
	{
		Disable,
		Enabled,
		Mirror,
	}

	public enum RSDFOversize
	{
		_100,
		_120,
		_150,
		_200,
	}
	public enum RSDFSize
	{
		_100,
		_50,
		_25,
	}
	public enum RShadowSelect
	{
		Disable,
		Shadow_1,
		Shadows_4,
		Shadows_16,
		Shadows_64,
		Shadows_256,
		Shadows_1024,
	}

	public interface IInputInterface : IWorldObject
	{
		RCursorShape RCursorShape { get; }
		/// <summary>
		/// Sends input as a 0,1 value
		/// </summary>
		/// <param name="pos"></param>
		void SendInput(Vector2f pos, Vector2f Tilt, float PressForce, Handed side, int current, bool isLazer, bool IsClickedPrime, bool IsClickedSecod, bool IsClickedTur);
	}

	[SingleComponentLock]
	[Category(new string[] { "Visuals" })]
	public sealed class Viewport : LinkedWorldComponent, IAssetProvider<RTexture2D>, IInputInterface
	{
		public readonly Sync<Vector2i> Size;
		public readonly Sync<Vector2i> Size2DOverride;
		public readonly Sync<bool> Size2DOverrideStretch;

		public readonly Sync<bool> UseTAA;
		public readonly Sync<bool> UseDebanding;

		public readonly Sync<bool> Disable3D;
		public readonly Sync<bool> OwnWorld3D;

		public readonly Sync<bool> TransparentBG;

		public readonly Sync<bool> GUIDisableInput;

		public readonly Sync<bool> Snap2DTransformsToPixels;
		public readonly Sync<bool> Snap2DVerticesToPixels;
		[Default(RMsaa.Disabled)]
		public readonly Sync<RMsaa> Msaa2D;
		[Default(RMsaa.Disabled)]
		public readonly Sync<RMsaa> Msaa3D;
		[Default(RScreenSpaceAA.Disabled)]
		public readonly Sync<RScreenSpaceAA> ScreenSpaceAA;
		[Default(RClearMode.Always)]
		public readonly Sync<RClearMode> ClearMode;
		[Default(RUpdateMode.Always)]
		public readonly Sync<RUpdateMode> UpdateMode;

		public readonly Sync<bool> UseOcclusionCulling;
		[Default(RDebugDraw.Disabled)]
		public readonly Sync<RDebugDraw> DebugDraw;
		[Default(RScaling3D.Billinear)]
		public readonly Sync<RScaling3D> Scaling3DMode;
		[Default(1f)]
		public readonly Sync<float> Scaling3DScale;
		public readonly Sync<float> TextureMipmapBias;
		[Default(0.2f)]
		public readonly Sync<float> FSRSharpness;
		[Default(RTextureFilter.Linear)]
		public readonly Sync<RTextureFilter> CanvasDefaultTextureFilter;
		[Default(RTextureRepeat.Disable)]
		public readonly Sync<RTextureRepeat> CanvasDefaultTextureRepate;
		[Default(true)]
		public readonly Sync<bool> SnapUIToPixels;
		[Default(RSDFOversize._120)]
		public readonly Sync<RSDFOversize> SDFOversize;
		[Default(RSDFSize._50)]
		public readonly Sync<RSDFSize> SDFScale;
		[Default(2048)]
		public readonly Sync<int> PositionalShadowAtlas;
		[Default(true)]
		public readonly Sync<bool> PositionalShadow16Bit;
		[Default(RShadowSelect.Shadows_4)]
		public readonly Sync<RShadowSelect> QuadZero;
		[Default(RShadowSelect.Shadows_4)]
		public readonly Sync<RShadowSelect> QuadOne;
		[Default(RShadowSelect.Shadows_16)]
		public readonly Sync<RShadowSelect> QuadTwo;
		[Default(RShadowSelect.Shadows_64)]
		public readonly Sync<RShadowSelect> QuadThree;

		protected override void OnAttach() {
			base.OnAttach();
			Size.Value = new Vector2i(255);
		}

		public Action ClearBackGroundCalled;
		public Action RenderFrameCalled;

		[Exposed]
		public void ClearBackGround() {
			ClearBackGroundCalled?.Invoke();
		}
		[Exposed]
		public void RenderFrame() {
			RenderFrameCalled?.Invoke();
		}

		protected override void OnLoaded() {
			base.OnLoaded();

		}


		public event Action<RTexture2D> OnAssetLoaded;

		public RTexture2D Value { get; private set; }

		public void Load(RTexture2D data) {
			Value = data;
			Loaded = data != null;
			OnAssetLoaded?.Invoke(data);
		}

		public bool Loaded { get; private set; } = false;

		public RCursorShape RCursorShape { get; set; }

		public override void Dispose() {
			Load(null);
			base.Dispose();
		}

		public Action<Vector2f, Vector2f, float, Handed, int, bool, bool, bool, bool> SendInputEvent;

		public void SendInput(Vector2f pos, Vector2f Tilt, float PressForce, Handed side, int current, bool isLazer, bool IsClickedPrime, bool IsClickedSecod, bool IsClickedTur) {
			SendInputEvent?.Invoke(pos, Tilt, PressForce, side, current, isLazer, IsClickedPrime, IsClickedSecod, IsClickedTur);
		}
	}
}
