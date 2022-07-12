using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials" })]
	public class ToonMaterial : MaterialBase<IToonMaterial>
	{
		[Default(BasicRenderMode.Opaque)]
		[OnChanged(nameof(RenderModeChanged))]
		public readonly Sync<BasicRenderMode> RenderMode;

		[OnChanged(nameof(CullModeChanged))]
		public readonly Sync<Cull> CullMode;

		[OnChanged(nameof(AlphaCutOutChanged))]
		public readonly Sync<float> AlphaCutOut;
		private void AlphaCutOutChanged() {
			if (_material is null) {
				return;
			}
			_material.AlphaCutOut = AlphaCutOut;
			_material.Material?.UpdatePrams();
		}

		[OnAssetLoaded(nameof(LitColorTextureUpdate))]
		public readonly AssetRef<RTexture2D> LitColorTexture;
		private void LitColorTextureUpdate() {
			if (_material is null) {
				return;
			}
			_material.LitColorTexture = LitColorTexture.Asset;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(LitColorTextureTillingChanged))]
		public readonly Sync<Vector2f> LitColorTextureTilling;
		private void LitColorTextureTillingChanged() {
			if (_material is null) {
				return;
			}
			_material.LitColorTextureTilling = LitColorTextureTilling;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(LitColorTextureOffsetChanged))]
		public readonly Sync<Vector2f> LitColorTextureOffset;
		private void LitColorTextureOffsetChanged() {
			if (_material is null) {
				return;
			}
			_material.LitColorTextureOffset = LitColorTextureOffset;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(LitColorTintChanged))]
		public readonly Sync<Colorf> LitColorTint;
		private void LitColorTintChanged() {
			if (_material is null) {
				return;
			}
			_material.LitColorTint = LitColorTint;
			_material.Material?.UpdatePrams();
		}

		[OnAssetLoaded(nameof(ShadeColorTextureUpdate))]
		public readonly AssetRef<RTexture2D> ShadeColorTexture;//fix
		private void ShadeColorTextureUpdate() {
			if (_material is null) {
				return;
			}
			_material.ShadeColorTexture = ShadeColorTexture.Asset;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(ShadeColorTextureTillingChanged))]
		public readonly Sync<Vector2f> ShadeColorTextureTilling;
		private void ShadeColorTextureTillingChanged() {
			if (_material is null) {
				return;
			}
			_material.ShadeColorTextureTilling = ShadeColorTextureTilling;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(ShadeColorTextureOffsetChanged))]
		public readonly Sync<Vector2f> ShadeColorTextureOffset;
		private void ShadeColorTextureOffsetChanged() {
			if (_material is null) {
				return;
			}
			_material.ShadeColorTextureOffset = ShadeColorTextureOffset;
			_material.Material?.UpdatePrams();
		}

		[OnChanged(nameof(ShadeColorTintChanged))]
		public readonly Sync<Colorf> ShadeColorTint;
		private void ShadeColorTintChanged() {
			if (_material is null) {
				return;
			}
			_material.ShadeColorTint = ShadeColorTint;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(ShadingToonyChanged))]
		public readonly Sync<float> ShadingToony;
		private void ShadingToonyChanged() {
			if (_material is null) {
				return;
			}
			_material.ShadingToony = ShadingToony;
			_material.Material?.UpdatePrams();
		}

		[OnAssetLoaded(nameof(NormalMapUpdate))]
		public readonly AssetRef<RTexture2D> NormalMap;//fix
		private void NormalMapUpdate() {
			if (_material is null) {
				return;
			}
			_material.NormalMap = NormalMap.Asset;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(NormalMapTillingChanged))]
		public readonly Sync<Vector2f> NormalMapTilling;
		private void NormalMapTillingChanged() {
			if (_material is null) {
				return;
			}
			_material.NormalMapTilling = NormalMapTilling;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(NormalMapOffsetChanged))]
		public readonly Sync<Vector2f> NormalMapOffset;
		private void NormalMapOffsetChanged() {
			if (_material is null) {
				return;
			}
			_material.NormalMapOffset = NormalMapOffset;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(NormalChanged))]
		public readonly Sync<float> Normal;
		private void NormalChanged() {
			if (_material is null) {
				return;
			}
			_material.Normal = Normal;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(ShadingShiftChanged))]
		public readonly Sync<float> ShadingShift;
		private void ShadingShiftChanged() {
			if (_material is null) {
				return;
			}
			_material.ShadingShift = ShadingShift;
			_material.Material?.UpdatePrams();
		}

		[OnAssetLoaded(nameof(ShadowReceiveMultiplierTextureUpdate))]
		public readonly AssetRef<RTexture2D> ShadowReceiveMultiplierTexture;//fix
		private void ShadowReceiveMultiplierTextureUpdate() {
			if (_material is null) {
				return;
			}
			_material.ShadowReceiveMultiplierTexture = ShadowReceiveMultiplierTexture.Asset;
			_material.Material?.UpdatePrams();
		}

		[OnChanged(nameof(ShadowReceiveMultiplierTextureTillingChanged))]
		public readonly Sync<Vector2f> ShadowReceiveMultiplierTextureTilling;
		private void ShadowReceiveMultiplierTextureTillingChanged() {
			if (_material is null) {
				return;
			}
			_material.ShadowReceiveMultiplierTextureTilling = ShadowReceiveMultiplierTextureTilling;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(ShadowReceiveMultiplierTextureOffsetChanged))]
		public readonly Sync<Vector2f> ShadowReceiveMultiplierTextureOffset;
		private void ShadowReceiveMultiplierTextureOffsetChanged() {
			if (_material is null) {
				return;
			}
			_material.ShadowReceiveMultiplierTextureOffset = ShadowReceiveMultiplierTextureOffset;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(ShadowReceiveMultiplierChanged))]
		public readonly Sync<float> ShadowReceiveMultiplier;
		private void ShadowReceiveMultiplierChanged() {
			if (_material is null) {
				return;
			}
			_material.ShadowReceiveMultiplier = ShadowReceiveMultiplier;
			_material.Material?.UpdatePrams();
		}
		[OnAssetLoaded(nameof(LitShadeMixingMultiplierTextureUpdate))]
		public readonly AssetRef<RTexture2D> LitShadeMixingMultiplierTexture;//fix
		private void LitShadeMixingMultiplierTextureUpdate() {
			if (_material is null) {
				return;
			}
			_material.LitShadeMixingMultiplierTexture = LitShadeMixingMultiplierTexture.Asset;
			_material.Material?.UpdatePrams();
		}

		[OnChanged(nameof(LitShadeMixingMultiplierTextureTillingChanged))]
		public readonly Sync<Vector2f> LitShadeMixingMultiplierTextureTilling;
		private void LitShadeMixingMultiplierTextureTillingChanged() {
			if (_material is null) {
				return;
			}
			_material.LitShadeMixingMultiplierTextureTilling = LitShadeMixingMultiplierTextureTilling;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(LitShadeMixingMultiplierTextureOffsetChanged))]
		public readonly Sync<Vector2f> LitShadeMixingMultiplierTextureOffset;
		private void LitShadeMixingMultiplierTextureOffsetChanged() {
			if (_material is null) {
				return;
			}
			_material.LitShadeMixingMultiplierTextureOffset = LitShadeMixingMultiplierTextureOffset;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(LitShadeMixingMultiplierChanged))]
		public readonly Sync<float> LitShadeMixingMultiplier;
		private void LitShadeMixingMultiplierChanged() {
			if (_material is null) {
				return;
			}
			_material.LitShadeMixingMultiplier = LitShadeMixingMultiplier;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(LightColorAttenuationChanged))]
		public readonly Sync<float> LightColorAttenuation;
		private void LightColorAttenuationChanged() {
			if (_material is null) {
				return;
			}
			_material.LightColorAttenuation = LightColorAttenuation;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(GLIntensityChanged))]
		public readonly Sync<float> GLIntensity;
		private void GLIntensityChanged() {
			if (_material is null) {
				return;
			}
			_material.GLIntensity = GLIntensity;
			_material.Material?.UpdatePrams();
		}
		[OnAssetLoaded(nameof(EmissionColorTextureUpdate))]
		public readonly AssetRef<RTexture2D> EmissionColorTexture;//fix
		private void EmissionColorTextureUpdate() {
			if (_material is null) {
				return;
			}
			_material.EmissionColorTexture = EmissionColorTexture.Asset;
			_material.Material?.UpdatePrams();
		}

		[OnChanged(nameof(EmissionColorTextureTillingChanged))]
		public readonly Sync<Vector2f> EmissionColorTextureTilling;
		private void EmissionColorTextureTillingChanged() {
			if (_material is null) {
				return;
			}
			_material.EmissionColorTextureTilling = EmissionColorTextureTilling;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(EmissionColorTextureOffsetChanged))]
		public readonly Sync<Vector2f> EmissionColorTextureOffset;
		private void EmissionColorTextureOffsetChanged() {
			if (_material is null) {
				return;
			}
			_material.EmissionColorTextureOffset = EmissionColorTextureOffset;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(EmissionColorTintChanged))]
		public readonly Sync<Colorf> EmissionColorTint;
		private void EmissionColorTintChanged() {
			if (_material is null) {
				return;
			}
			_material.EmissionColorTint = EmissionColorTint;
			_material.Material?.UpdatePrams();
		}
		[OnAssetLoaded(nameof(MatCapUpdate))]
		public readonly AssetRef<RTexture2D> MatCap;//fix
		private void MatCapUpdate() {
			if (_material is null) {
				return;
			}
			_material.MatCap = MatCap.Asset;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(MatCapTillingChanged))]
		public readonly Sync<Vector2f> MatCapTilling;
		private void MatCapTillingChanged() {
			if (_material is null) {
				return;
			}
			_material.MatCapTilling = MatCapTilling;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(MatCapOffsetChanged))]
		public readonly Sync<Vector2f> MatCapOffset;
		private void MatCapOffsetChanged() {
			if (_material is null) {
				return;
			}
			_material.MatCapOffset = MatCapOffset;
			_material.Material?.UpdatePrams();
		}
		[OnAssetLoaded(nameof(RimColorTextureUpdate))]
		public readonly AssetRef<RTexture2D> RimColorTexture;//fix
		private void RimColorTextureUpdate() {
			if (_material is null) {
				return;
			}
			_material.RimColorTexture = RimColorTexture.Asset;
			_material.Material?.UpdatePrams();
		}

		[OnChanged(nameof(RimColorTextureTillingChanged))]
		public readonly Sync<Vector2f> RimColorTextureTilling;
		private void RimColorTextureTillingChanged() {
			if (_material is null) {
				return;
			}
			_material.RimColorTextureTilling = RimColorTextureTilling;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(RimColorTextureOffsetChanged))]
		public readonly Sync<Vector2f> RimColorTextureOffset;
		private void RimColorTextureOffsetChanged() {
			if (_material is null) {
				return;
			}
			_material.RimColorTextureOffset = RimColorTextureOffset;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(RimColorTintChanged))]
		public readonly Sync<Colorf> RimColorTint;
		private void RimColorTintChanged() {
			if (_material is null) {
				return;
			}
			_material.RimColorTint = RimColorTint;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(LightingMixChanged))]
		public readonly Sync<float> LightingMix;
		private void LightingMixChanged() {
			if (_material is null) {
				return;
			}
			_material.LightingMix = LightingMix;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(FresnelPowerChanged))]
		public readonly Sync<float> FresnelPower;
		private void FresnelPowerChanged() {
			if (_material is null) {
				return;
			}
			_material.FresnelPower = FresnelPower;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(LiftChanged))]
		public readonly Sync<float> Lift;
		private void LiftChanged() {
			if (_material is null) {
				return;
			}
			_material.Lift = Lift;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(OutLineTypeChanged))]
		public readonly Sync<IToonMaterial.OutLineType> OutLineMode;
		private void OutLineTypeChanged() {
			if (_material is null) {
				return;
			}
			_material.OutLineMode = OutLineMode;
			_material.Material?.UpdatePrams();
		}
		[OnAssetLoaded(nameof(OutLineWidthTextureUpdate))]
		public readonly AssetRef<RTexture2D> OutLineWidthTexture;//fix
		private void OutLineWidthTextureUpdate() {
			if (_material is null) {
				return;
			}
			_material.OutLineWidthTexture = OutLineWidthTexture.Asset;
			_material.Material?.UpdatePrams();
		}

		[OnChanged(nameof(OutLineWidthTextureTillingChanged))]
		public readonly Sync<Vector2f> OutLineWidthTextureTilling;
		private void OutLineWidthTextureTillingChanged() {
			if (_material is null) {
				return;
			}
			_material.OutLineWidthTextureTilling = OutLineWidthTextureTilling;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(OutLineWidthTextureOffsetChanged))]
		public readonly Sync<Vector2f> OutLineWidthTextureOffset;
		private void OutLineWidthTextureOffsetChanged() {
			if (_material is null) {
				return;
			}
			_material.OutLineWidthTextureOffset = OutLineWidthTextureOffset;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(OutLineWidthChanged))]
		public readonly Sync<float> OutLineWidth;
		private void OutLineWidthChanged() {
			if (_material is null) {
				return;
			}
			_material.OutLineWidth = OutLineWidth;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(WidthScaledMaxDistanceChanged))]
		public readonly Sync<float> WidthScaledMaxDistance;
		private void WidthScaledMaxDistanceChanged() {
			if (_material is null) {
				return;
			}
			_material.WidthScaledMaxDistance = WidthScaledMaxDistance;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(FixedColorChanged))]
		public readonly Sync<bool> FixedColor;
		private void FixedColorChanged() {
			if (_material is null) {
				return;
			}
			_material.FixedColor = FixedColor;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(OutLineColorChanged))]
		public readonly Sync<Colorf> OutLineColor;
		private void OutLineColorChanged() {
			if (_material is null) {
				return;
			}
			_material.OutLineColor = OutLineColor;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(OutLineLightingMixChanged))]
		public readonly Sync<float> OutLineLightingMix;
		private void OutLineLightingMixChanged() {
			if (_material is null) {
				return;
			}
			_material.OutLineLightingMix = OutLineLightingMix;
			_material.Material?.UpdatePrams();
		}
		[OnAssetLoaded(nameof(AnimationMaskUpdate))]
		public readonly AssetRef<RTexture2D> AnimationMask;//fix////
		private void AnimationMaskUpdate() {
			if (_material is null) {
				return;
			}
			_material.AnimationMask = AnimationMask.Asset;
			_material.Material?.UpdatePrams();
		}

		[OnChanged(nameof(AnimationMaskTillingChanged))]
		public readonly Sync<Vector2f> AnimationMaskTilling;
		private void AnimationMaskTillingChanged() {
			if (_material is null) {
				return;
			}
			_material.AnimationMaskTilling = AnimationMaskTilling;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(AnimationMaskOffsetChanged))]
		public readonly Sync<Vector2f> AnimationMaskOffset;
		private void AnimationMaskOffsetChanged() {
			if (_material is null) {
				return;
			}
			_material.AnimationMaskOffset = AnimationMaskOffset;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(ScrollAnimationChanged))]
		public readonly Sync<Vector2f> ScrollAnimation;
		private void ScrollAnimationChanged() {
			if (_material is null) {
				return;
			}
			_material.ScrollAnimation = ScrollAnimation;
			_material.Material?.UpdatePrams();
		}
		[OnChanged(nameof(RotationAnimationChanged))]
		public readonly Sync<float> RotationAnimation;
		private void RotationAnimationChanged() {
			if (_material is null) {
				return;
			}
			_material.RotationAnimation = RotationAnimation;
			_material.Material?.UpdatePrams();
		}

		private void RenderModeChanged() {
			if (_material is null) {
				return;
			}
			_material.RenderMode = RenderMode;
			_material.Material?.UpdatePrams();
		}
		private void CullModeChanged() {
			if (_material is null) {
				return;
			}
			_material.CullMode = CullMode;
			_material.Material?.UpdatePrams();
		}

		public override void OnAttach() {
			base.OnAttach();
		}
		public override void UpdateAll() {

			RenderModeChanged();
			CullModeChanged();
			AlphaCutOutChanged();
			LitColorTextureUpdate();
			LitColorTextureTillingChanged();
			LitColorTextureOffsetChanged();
			LitColorTintChanged();
			ShadeColorTextureUpdate();
			ShadeColorTextureTillingChanged();
			ShadeColorTextureOffsetChanged();
			ShadeColorTintChanged();
			ShadingToonyChanged();
			NormalMapUpdate();
			NormalMapTillingChanged();
			NormalMapOffsetChanged();
			NormalChanged();
			ShadingShiftChanged();
			ShadowReceiveMultiplierTextureUpdate();
			ShadowReceiveMultiplierTextureTillingChanged();
			ShadowReceiveMultiplierTextureOffsetChanged();
			ShadowReceiveMultiplierChanged();
			LitShadeMixingMultiplierTextureUpdate();
			LitShadeMixingMultiplierTextureTillingChanged();
			LitShadeMixingMultiplierTextureOffsetChanged();
			LitShadeMixingMultiplierChanged();
			LightColorAttenuationChanged();
			GLIntensityChanged();
			EmissionColorTextureUpdate();
			EmissionColorTextureTillingChanged();
			EmissionColorTextureOffsetChanged();
			EmissionColorTintChanged();
			MatCapUpdate();
			MatCapTillingChanged();
			MatCapOffsetChanged();
			RimColorTextureUpdate();
			RimColorTextureTillingChanged();
			RimColorTextureOffsetChanged();
			RimColorTintChanged();
			LightingMixChanged();
			FresnelPowerChanged();
			LiftChanged();
			OutLineTypeChanged();
			OutLineWidthTextureUpdate();
			OutLineWidthTextureTillingChanged();
			OutLineWidthTextureOffsetChanged();
			OutLineWidthChanged();
			WidthScaledMaxDistanceChanged();
			FixedColorChanged();
			OutLineColorChanged();
			OutLineLightingMixChanged();
			AnimationMaskUpdate();
			AnimationMaskTillingChanged();
			AnimationMaskOffsetChanged();
			ScrollAnimationChanged();
			RotationAnimationChanged();

		}

	}
}
