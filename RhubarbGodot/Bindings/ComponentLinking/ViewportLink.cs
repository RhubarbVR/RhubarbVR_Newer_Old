﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhubarbVR.Bindings.TextureBindings;

using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

using Viewport = RhuEngine.Components.Viewport;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class ViewportLink : WorldNodeLinked<Viewport, SubViewport>
	{
		public override string ObjectName => "Viewport";

		public override void Render() {

		}

		public override void StartContinueInit() {
			LinkedComp.Size.Changed += Size_Changed;
			LinkedComp.Size2DOverride.Changed += Size2DOverride_Changed;
			LinkedComp.Size2DOverrideStretch.Changed += Size2DOverrideStretch_Changed;
			LinkedComp.UseTAA.Changed += UseTAA_Changed;
			LinkedComp.UseDebanding.Changed += UseDebanding_Changed;
			LinkedComp.Disable3D.Changed += Disable3D_Changed;
			LinkedComp.OwnWorld3D.Changed += OwnWorld3D_Changed;
			LinkedComp.TransparentBG.Changed += TransparentBG_Changed;
			LinkedComp.Snap2DTransformsToPixels.Changed += Snap2DTransformsToPixels_Changed;
			LinkedComp.Snap2DVerticesToPixels.Changed += Snap2DVerticesToPixels_Changed;
			LinkedComp.Msaa2D.Changed += Msaa2D_Changed;
			LinkedComp.Msaa3D.Changed += Msaa3D_Changed;
			LinkedComp.ScreenSpaceAA.Changed += ScreenSpaceAA_Changed;
			LinkedComp.ClearMode.Changed += ClearMode_Changed;
			LinkedComp.UpdateMode.Changed += UpdateMode_Changed;
			LinkedComp.UseOcclusionCulling.Changed += UseOcclusionCulling_Changed;
			LinkedComp.DebugDraw.Changed += DebugDraw_Changed;
			LinkedComp.Scaling3DMode.Changed += Scaling3DMode_Changed;
			LinkedComp.Scaling3DScale.Changed += Scaling3DScale_Changed;
			LinkedComp.TextureMipmapBias.Changed += TextureMipmapBias_Changed;
			LinkedComp.FSRSharpness.Changed += FSRSharpness_Changed;
			LinkedComp.CanvasDefaultTextureFilter.Changed += CanvasDefaultTextureFilter_Changed;
			LinkedComp.CanvasDefaultTextureRepate.Changed += CanvasDefaultTextureRepate_Changed;
			LinkedComp.SnapUIToPixels.Changed += SnapUIToPixels_Changed;
			LinkedComp.SDFOversize.Changed += SDFOversize_Changed;
			LinkedComp.SDFScale.Changed += SDFScale_Changed;
			LinkedComp.PositionalShadowAtlas.Changed += PositionalShadowAtlas_Changed;
			LinkedComp.PositionalShadow16Bit.Changed += PositionalShadow16Bit_Changed;
			LinkedComp.QuadZero.Changed += QuadZero_Changed;
			LinkedComp.QuadOne.Changed += QuadOne_Changed;
			LinkedComp.QuadTwo.Changed += QuadTwo_Changed;
			LinkedComp.QuadThree.Changed += QuadThree_Changed;
			Size_Changed(null);
			Size2DOverride_Changed(null);
			Size2DOverrideStretch_Changed(null);
			UseTAA_Changed(null);
			UseDebanding_Changed(null);
			Disable3D_Changed(null);
			OwnWorld3D_Changed(null);
			TransparentBG_Changed(null);
			Snap2DTransformsToPixels_Changed(null);
			Snap2DVerticesToPixels_Changed(null);
			Msaa2D_Changed(null);
			Msaa3D_Changed(null);
			ScreenSpaceAA_Changed(null);
			ClearMode_Changed(null);
			UpdateMode_Changed(null);
			UseOcclusionCulling_Changed(null);
			DebugDraw_Changed(null);
			Scaling3DMode_Changed(null);
			Scaling3DScale_Changed(null);
			TextureMipmapBias_Changed(null);
			FSRSharpness_Changed(null);
			CanvasDefaultTextureFilter_Changed(null);
			CanvasDefaultTextureRepate_Changed(null);
			SnapUIToPixels_Changed(null);
			SDFOversize_Changed(null);
			SDFScale_Changed(null);
			PositionalShadowAtlas_Changed(null);
			PositionalShadow16Bit_Changed(null);
			QuadZero_Changed(null);
			QuadOne_Changed(null);
			QuadTwo_Changed(null);
			QuadThree_Changed(null);
			LinkedComp.Load(new RTexture2D(new GodotTexture2D(node.GetTexture())));
			LinkedComp.SendInputEvent = UpdateInput;
			LinkedComp.ClearBackGroundCalled = ClearCalled;
			LinkedComp.RenderFrameCalled = RenderFrameCalled;
			LinkedComp.Entity.children.OnReorderList += Children_OnReorderList;
		}


		private void Children_OnReorderList() {
			foreach (Entity item in LinkedComp.Entity.children) {
				if (item?.CanvasItem?.WorldLink is ICanvasItemNodeLinked canvasItem) {
					node.MoveChild(canvasItem.CanvasItem, -1);
				}
			}
		}

		private void RenderFrameCalled() {
			switch (node.RenderTargetUpdateMode) {
				case SubViewport.UpdateMode.Disabled:
					node.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
					break;
				case SubViewport.UpdateMode.Once:
					node.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
					break;
				default:
					break;
			}
		}

		private void ClearCalled() {
			switch (node.RenderTargetClearMode) {
				case SubViewport.ClearMode.Never:
					node.RenderTargetClearMode = SubViewport.ClearMode.Once;
					break;
				case SubViewport.ClearMode.Once:
					node.RenderTargetClearMode = SubViewport.ClearMode.Once;
					break;
				default:
					break;
			}
		}

		private void UpdateInput(RNumerics.Vector2f pos) {
			if (_isInputUpdate) {
				node.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
			}

		}

		private void QuadThree_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.PositionalShadowAtlasQuad3 = LinkedComp.QuadThree.Value switch {
				RShadowSelect.Shadow_1 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv1,
				RShadowSelect.Shadows_4 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv4,
				RShadowSelect.Shadows_16 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv16,
				RShadowSelect.Shadows_64 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv64,
				RShadowSelect.Shadows_256 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv256,
				RShadowSelect.Shadows_1024 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv1024,
				_ => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Disabled,
			};
		}

		private void QuadTwo_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.PositionalShadowAtlasQuad2 = LinkedComp.QuadTwo.Value switch {
				RShadowSelect.Shadow_1 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv1,
				RShadowSelect.Shadows_4 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv4,
				RShadowSelect.Shadows_16 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv16,
				RShadowSelect.Shadows_64 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv64,
				RShadowSelect.Shadows_256 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv256,
				RShadowSelect.Shadows_1024 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv1024,
				_ => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Disabled,
			};
		}

		private void QuadOne_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.PositionalShadowAtlasQuad1 = LinkedComp.QuadOne.Value switch {
				RShadowSelect.Shadow_1 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv1,
				RShadowSelect.Shadows_4 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv4,
				RShadowSelect.Shadows_16 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv16,
				RShadowSelect.Shadows_64 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv64,
				RShadowSelect.Shadows_256 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv256,
				RShadowSelect.Shadows_1024 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv1024,
				_ => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Disabled,
			};
		}

		private void QuadZero_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.PositionalShadowAtlasQuad0 = LinkedComp.QuadZero.Value switch {
				RShadowSelect.Shadow_1 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv1,
				RShadowSelect.Shadows_4 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv4,
				RShadowSelect.Shadows_16 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv16,
				RShadowSelect.Shadows_64 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv64,
				RShadowSelect.Shadows_256 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv256,
				RShadowSelect.Shadows_1024 => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Subdiv1024,
				_ => Godot.Viewport.PositionalShadowAtlasQuadrantSubdiv.Disabled,
			};
		}

		private void PositionalShadow16Bit_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.PositionalShadowAtlas16Bits = LinkedComp.PositionalShadow16Bit.Value;
		}

		private void PositionalShadowAtlas_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.PositionalShadowAtlasSize = LinkedComp.PositionalShadowAtlas.Value;
		}

		private void SDFScale_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.SdfScale = LinkedComp.SDFScale.Value switch {
				RSDFSize._100 => Godot.Viewport.SDFScale.Scale100Percent,
				RSDFSize._50 => Godot.Viewport.SDFScale.Scale50Percent,
				_ => Godot.Viewport.SDFScale.Scale25Percent,
			};
		}

		private void SDFOversize_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.SdfOversize = LinkedComp.SDFOversize.Value switch {
				RSDFOversize._120 => Godot.Viewport.SDFOversize.Oversize120Percent,
				RSDFOversize._150 => Godot.Viewport.SDFOversize.Oversize150Percent,
				RSDFOversize._200 => Godot.Viewport.SDFOversize.Oversize200Percent,
				_ => Godot.Viewport.SDFOversize.Oversize100Percent,
			};
		}

		private void SnapUIToPixels_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.GuiSnapControlsToPixels = LinkedComp.SnapUIToPixels.Value;
		}

		private void CanvasDefaultTextureRepate_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.CanvasItemDefaultTextureRepeat = LinkedComp.CanvasDefaultTextureRepate.Value switch {
				RTextureRepeat.Enabled => Godot.Viewport.DefaultCanvasItemTextureRepeat.Enabled,
				RTextureRepeat.Mirror => Godot.Viewport.DefaultCanvasItemTextureRepeat.Mirror,
				_ => Godot.Viewport.DefaultCanvasItemTextureRepeat.Disabled,
			};
		}

		private void CanvasDefaultTextureFilter_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.CanvasItemDefaultTextureFilter = LinkedComp.CanvasDefaultTextureFilter.Value switch {
				RTextureFilter.Linear => Godot.Viewport.DefaultCanvasItemTextureFilter.Linear,
				RTextureFilter.LinearMipmap => Godot.Viewport.DefaultCanvasItemTextureFilter.LinearWithMipmaps,
				RTextureFilter.NearestMipmap => Godot.Viewport.DefaultCanvasItemTextureFilter.NearestWithMipmaps,
				_ => Godot.Viewport.DefaultCanvasItemTextureFilter.Nearest,
			};
		}

		private void FSRSharpness_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.FsrSharpness = LinkedComp.FSRSharpness.Value;
		}

		private void TextureMipmapBias_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.TextureMipmapBias = LinkedComp.TextureMipmapBias.Value;
		}

		private void Scaling3DScale_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.Scaling3dScale = LinkedComp.Scaling3DScale.Value;
		}

		private void Scaling3DMode_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.Scaling3dMode = LinkedComp.Scaling3DMode.Value switch {
				RScaling3D.FSR => Godot.Viewport.Scaling3DMode.Fsr,
				_ => Godot.Viewport.Scaling3DMode.Bilinear,
			};
		}

		private void DebugDraw_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.DebugDraw = LinkedComp.DebugDraw.Value switch {
				RDebugDraw.Unshaded => Godot.Viewport.DebugDrawEnum.Unshaded,
				RDebugDraw.Lighting => Godot.Viewport.DebugDrawEnum.Lighting,
				RDebugDraw.Overdraw => Godot.Viewport.DebugDrawEnum.Overdraw,
				RDebugDraw.Wireframe => Godot.Viewport.DebugDrawEnum.Wireframe,
				RDebugDraw.NormalBuffer => Godot.Viewport.DebugDrawEnum.NormalBuffer,
				RDebugDraw.VoxelGiAlbedo => Godot.Viewport.DebugDrawEnum.VoxelGiAlbedo,
				RDebugDraw.VoxelGiLighting => Godot.Viewport.DebugDrawEnum.VoxelGiLighting,
				RDebugDraw.VoxelGiEmission => Godot.Viewport.DebugDrawEnum.VoxelGiEmission,
				RDebugDraw.ShadowAtlas => Godot.Viewport.DebugDrawEnum.ShadowAtlas,
				RDebugDraw.DirectionalShadowAtlas => Godot.Viewport.DebugDrawEnum.DirectionalShadowAtlas,
				RDebugDraw.SceneLuminance => Godot.Viewport.DebugDrawEnum.SceneLuminance,
				RDebugDraw.Ssao => Godot.Viewport.DebugDrawEnum.Ssao,
				RDebugDraw.Ssil => Godot.Viewport.DebugDrawEnum.Ssil,
				RDebugDraw.PssmSplits => Godot.Viewport.DebugDrawEnum.PssmSplits,
				RDebugDraw.DecalAtlas => Godot.Viewport.DebugDrawEnum.DecalAtlas,
				RDebugDraw.Sdfgi => Godot.Viewport.DebugDrawEnum.Sdfgi,
				RDebugDraw.SdfgiProbes => Godot.Viewport.DebugDrawEnum.SdfgiProbes,
				RDebugDraw.GiBuffer => Godot.Viewport.DebugDrawEnum.GiBuffer,
				RDebugDraw.DisableLod => Godot.Viewport.DebugDrawEnum.DisableLod,
				RDebugDraw.ClusterOmniLights => Godot.Viewport.DebugDrawEnum.ClusterOmniLights,
				RDebugDraw.ClusterSpotLights => Godot.Viewport.DebugDrawEnum.ClusterSpotLights,
				RDebugDraw.ClusterDecals => Godot.Viewport.DebugDrawEnum.ClusterDecals,
				RDebugDraw.ClusterReflectionProbes => Godot.Viewport.DebugDrawEnum.ClusterReflectionProbes,
				RDebugDraw.Occluders => Godot.Viewport.DebugDrawEnum.Occluders,
				RDebugDraw.MotionVectors => Godot.Viewport.DebugDrawEnum.MotionVectors,
				_ => Godot.Viewport.DebugDrawEnum.Disabled,
			};
		}

		private void UseOcclusionCulling_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.UseOcclusionCulling = LinkedComp.UseOcclusionCulling.Value;
		}

		private bool _isInputUpdate = false;

		private void UpdateMode_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			switch (LinkedComp.UpdateMode.Value) {
				case RUpdateMode.InputUpdate:
					_isInputUpdate = true;
					node.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
					break;
				case RUpdateMode.Always:
					node.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
					break;
				default:
					node.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
					break;
			}
		}

		private void ClearMode_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.RenderTargetClearMode = LinkedComp.ClearMode.Value switch {
				RClearMode.Always => SubViewport.ClearMode.Always,
				_ => SubViewport.ClearMode.Never,
			};
		}

		private void ScreenSpaceAA_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.ScreenSpaceAa = LinkedComp.ScreenSpaceAA.Value switch {
				RScreenSpaceAA.Fxaa => Godot.Viewport.ScreenSpaceAA.Fxaa,
				_ => Godot.Viewport.ScreenSpaceAA.Disabled,
			};
		}

		private void Msaa3D_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.Msaa3d = LinkedComp.Msaa3D.Value switch {
				RMsaa.TwoX => Godot.Viewport.MSAA.Msaa2x,
				RMsaa.FourX => Godot.Viewport.MSAA.Msaa4x,
				RMsaa.EightX => Godot.Viewport.MSAA.Msaa8x,
				_ => Godot.Viewport.MSAA.Disabled,
			};
		}

		private void Msaa2D_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.Msaa2d = LinkedComp.Msaa2D.Value switch {
				RMsaa.TwoX => Godot.Viewport.MSAA.Msaa2x,
				RMsaa.FourX => Godot.Viewport.MSAA.Msaa4x,
				RMsaa.EightX => Godot.Viewport.MSAA.Msaa8x,
				_ => Godot.Viewport.MSAA.Disabled,
			};
		}

		private void Snap2DVerticesToPixels_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.Snap2dVerticesToPixel = LinkedComp.Snap2DVerticesToPixels.Value;
		}

		private void Snap2DTransformsToPixels_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.Snap2dTransformsToPixel = LinkedComp.Snap2DTransformsToPixels.Value;
		}

		private void TransparentBG_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.TransparentBg = LinkedComp.TransparentBG.Value;
		}

		private void OwnWorld3D_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.OwnWorld3d = LinkedComp.OwnWorld3D.Value;
		}

		private void Disable3D_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.Disable3d = LinkedComp.Disable3D.Value;
		}

		private void UseDebanding_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.UseDebanding = LinkedComp.UseDebanding.Value;
		}

		private void UseTAA_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.UseTaa = LinkedComp.UseTAA.Value;
		}

		private void Size2DOverrideStretch_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.Size2dOverrideStretch = LinkedComp.Size2DOverrideStretch.Value;
		}

		private void Size2DOverride_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.Size2dOverride = new Vector2i(LinkedComp.Size2DOverride.Value.x, LinkedComp.Size2DOverride.Value.y);
		}

		private void Size_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			node.Size = new Vector2i(Math.Max(2, LinkedComp.Size.Value.x), Math.Max(2, LinkedComp.Size.Value.y));
		}

		public override void Started() {

		}

		public override void Stopped() {

		}
	}
}
