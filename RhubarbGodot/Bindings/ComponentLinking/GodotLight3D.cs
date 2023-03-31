using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhubarbVR.Bindings.TextureBindings;

using RhuEngine;
using RhuEngine.Components;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class GodotDirectionalLight3D : GodotLight3D<RhuEngine.Components.DirectionalLight3D, Godot.DirectionalLight3D>
	{
		public override string ObjectName => "DirectionalLight3D";

		public override void StartContinueInit() {
			LinkedComp.SkyMode.Changed += SkyMode_Changed;
			LinkedComp.ShadowMode.Changed += ShadowMode_Changed;
			LinkedComp.SplitOne.Changed += SplitOne_Changed;
			LinkedComp.SplitTwo.Changed += SplitTwo_Changed;
			LinkedComp.SplitThree.Changed += SplitThree_Changed;
			LinkedComp.BlendSplits.Changed += BlendSplits_Changed;
			LinkedComp.FadeStart.Changed += FadeStart_Changed;
			LinkedComp.MaxDistance.Changed += MaxDistance_Changed;
			LinkedComp.PancakeSize.Changed += PancakeSize_Changed;
			SkyMode_Changed(null);
			ShadowMode_Changed(null);
			SplitOne_Changed(null);
			SplitTwo_Changed(null);
			SplitThree_Changed(null);
			BlendSplits_Changed(null);
			FadeStart_Changed(null);
			MaxDistance_Changed(null);
			PancakeSize_Changed(null);
		}

		private void PancakeSize_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DirectionalShadowPancakeSize = LinkedComp.PancakeSize.Value);
		}

		private void MaxDistance_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DirectionalShadowMaxDistance = LinkedComp.MaxDistance.Value);
		}

		private void FadeStart_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DirectionalShadowFadeStart = LinkedComp.FadeStart.Value);
		}

		private void BlendSplits_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DirectionalShadowBlendSplits = LinkedComp.BlendSplits.Value);
		}

		private void SplitThree_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DirectionalShadowSplit3 = LinkedComp.SplitThree.Value);
		}

		private void SplitTwo_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DirectionalShadowSplit2 = LinkedComp.SplitTwo.Value);
		}

		private void SplitOne_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DirectionalShadowSplit1 = LinkedComp.SplitOne.Value);
		}

		private void ShadowMode_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DirectionalShadowMode = LinkedComp.ShadowMode.Value switch {
				RhuEngine.Components.DirectionalLight3D.DirectionalLightShadowMode.PSSM2Splits => Godot.DirectionalLight3D.ShadowMode.Parallel2Splits,
				RhuEngine.Components.DirectionalLight3D.DirectionalLightShadowMode.PSSM4Splits => Godot.DirectionalLight3D.ShadowMode.Parallel4Splits,
				_ => Godot.DirectionalLight3D.ShadowMode.Orthogonal,
			});
		}

		private void SkyMode_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.SkyMode = LinkedComp.SkyMode.Value switch {
				RhuEngine.Components.DirectionalLight3D.DirectionalLightMode.Sky => Godot.DirectionalLight3D.SkyModeEnum.SkyOnly,
				RhuEngine.Components.DirectionalLight3D.DirectionalLightMode.LightAndSky => Godot.DirectionalLight3D.SkyModeEnum.LightAndSky,
				_ => Godot.DirectionalLight3D.SkyModeEnum.LightOnly,
			});
		}
	}


	public sealed class GodotPointLight3D : GodotLight3D<RhuEngine.Components.PointLight3D, Godot.OmniLight3D>
	{
		public override string ObjectName => "PointLight3D";

		public override void StartContinueInit() {
			LinkedComp.Range.Changed += Range_Changed;
			LinkedComp.Attenuation.Changed += Attenuation_Changed;
			LinkedComp.ShadowMode.Changed += ShadowMode_Changed;
			Range_Changed(null);
			Attenuation_Changed(null);
			ShadowMode_Changed(null);
		}

		private void ShadowMode_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() =>
				node.OmniShadowMode = LinkedComp.ShadowMode.Value switch {
					PointLight3D.PointLightShadowMode.DualParanoloid => OmniLight3D.ShadowMode.DualParaboloid,
					_ => OmniLight3D.ShadowMode.Cube,
				});
		}

		private void Attenuation_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.OmniAttenuation = LinkedComp.Attenuation);
		}

		private void Range_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.OmniRange = LinkedComp.Range);
		}
	}


	public sealed class GodotSpotLight3D : GodotLight3D<RhuEngine.Components.SpotLight3D, Godot.SpotLight3D>
	{
		public override string ObjectName => "SpotLight3D";

		public override void StartContinueInit() {
			LinkedComp.Range.Changed += Range_Changed;
			LinkedComp.Attenuation.Changed += Attenuation_Changed;
			LinkedComp.Angle.Changed += Angle_Changed;
			LinkedComp.AngleAttenuation.Changed += AngleAttenuation_Changed;
			Range_Changed(null);
			Attenuation_Changed(null);
			Angle_Changed(null);
			AngleAttenuation_Changed(null);
		}

		private void AngleAttenuation_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.SpotAngleAttenuation = LinkedComp.AngleAttenuation);
		}

		private void Angle_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.SpotAngle = LinkedComp.Angle);
		}

		private void Attenuation_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.SpotAttenuation = LinkedComp.Attenuation);
		}

		private void Range_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.SpotRange = LinkedComp.Range);
		}
	}

	public abstract class GodotLight3D<T, T2> : VisualInstance3DBase<T, T2> where T : RhuEngine.Components.Light3D, new() where T2 : Godot.Light3D, new()
	{
		public override void Init() {
			base.Init();
			LinkedComp.Color.Changed += Color_Changed;
			LinkedComp.Energy.Changed += Energy_Changed;
			LinkedComp.IndirectEnergy.Changed += IndirectEnergy_Changed;
			LinkedComp.VolumetricFogEnergy.Changed += VolumetricFogEnergy_Changed;
			LinkedComp.Projector.LoadChange += Projector_LoadChange;
			LinkedComp.Size.Changed += Size_Changed;
			LinkedComp.Negative.Changed += Negative_Changed;
			LinkedComp.Specular.Changed += Specular_Changed;
			LinkedComp.BakeMode.Changed += BakeMode_Changed;
			LinkedComp.CullMask.Changed += CullMask_Changed;
			LinkedComp.ShadowEnabled.Changed += ShadowEnabled_Changed;
			LinkedComp.ShadowBias.Changed += ShadowBias_Changed;
			LinkedComp.ShadowNormalBias.Changed += ShadowNormalBias_Changed;
			LinkedComp.ShadowReverseCullFace.Changed += ShadowReverseCullFace_Changed;
			LinkedComp.ShadowTransmittanceBias.Changed += ShadowTransmittanceBias_Changed;
			LinkedComp.ShadowOpacity.Changed += ShadowOpacity_Changed;
			LinkedComp.ShadowBlur.Changed += ShadowBlur_Changed;
			LinkedComp.DistanceFadeEnabled.Changed += DistanceFadeEnabled_Changed;
			LinkedComp.DistanceFadeShadow.Changed += DistanceFadeShadow_Changed;
			LinkedComp.DistanceFadeLength.Changed += DistanceFadeLength_Changed;
			Color_Changed(null);
			Energy_Changed(null);
			IndirectEnergy_Changed(null);
			VolumetricFogEnergy_Changed(null);
			Projector_LoadChange(null);
			Size_Changed(null);
			Negative_Changed(null);
			Specular_Changed(null);
			BakeMode_Changed(null);
			CullMask_Changed(null);
			ShadowEnabled_Changed(null);
			ShadowBias_Changed(null);
			ShadowNormalBias_Changed(null);
			ShadowReverseCullFace_Changed(null);
			ShadowTransmittanceBias_Changed(null);
			ShadowOpacity_Changed(null);
			ShadowBlur_Changed(null);
			DistanceFadeEnabled_Changed(null);
			DistanceFadeShadow_Changed(null);
			DistanceFadeLength_Changed(null);
		}

		private void DistanceFadeLength_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DistanceFadeLength = LinkedComp.DistanceFadeLength);
		}

		private void DistanceFadeShadow_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DistanceFadeShadow = LinkedComp.DistanceFadeShadow);
		}

		private void DistanceFadeEnabled_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DistanceFadeEnabled = LinkedComp.DistanceFadeEnabled);
		}

		private void ShadowBlur_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ShadowBlur = LinkedComp.ShadowBlur);
		}

		private void ShadowOpacity_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ShadowOpacity = LinkedComp.ShadowOpacity);
		}

		private void ShadowTransmittanceBias_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ShadowTransmittanceBias = LinkedComp.ShadowTransmittanceBias);
		}

		private void ShadowReverseCullFace_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ShadowReverseCullFace = LinkedComp.ShadowReverseCullFace);
		}

		private void ShadowNormalBias_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ShadowNormalBias = LinkedComp.ShadowNormalBias);
		}

		private void ShadowBias_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ShadowBias = LinkedComp.ShadowBias);
		}

		private void ShadowEnabled_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ShadowEnabled = LinkedComp.ShadowEnabled);
		}

		private void CullMask_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LightCullMask = (uint)LinkedComp.CullMask.Value);
		}

		private void BakeMode_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() =>
				node.LightBakeMode = LinkedComp.BakeMode.Value switch {
					BakeMode.Static => Godot.Light3D.BakeMode.Static,
					BakeMode.Dynamic => Godot.Light3D.BakeMode.Dynamic,
					_ => Godot.Light3D.BakeMode.Disabled,
				});
		}

		private void Specular_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LightSpecular = LinkedComp.Specular.Value);
		}

		private void Negative_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LightNegative = LinkedComp.Negative.Value);
		}

		private void Size_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LightSize = LinkedComp.Size.Value);
		}

		private void Projector_LoadChange(RhuEngine.Linker.RTexture obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LightProjector = LinkedComp.Projector?.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null);
		}

		private void VolumetricFogEnergy_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LightVolumetricFogEnergy = LinkedComp.VolumetricFogEnergy.Value);
		}

		private void IndirectEnergy_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LightIndirectEnergy = LinkedComp.IndirectEnergy.Value);
		}

		private void Energy_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LightEnergy = LinkedComp.Energy.Value);
		}

		private void Color_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LightColor = new Color(LinkedComp.Color.Value.r, LinkedComp.Color.Value.g, LinkedComp.Color.Value.b, LinkedComp.Color.Value.a));
		}
	}
}
