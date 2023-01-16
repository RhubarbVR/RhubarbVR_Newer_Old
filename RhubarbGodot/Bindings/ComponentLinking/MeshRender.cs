using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine;
using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;

using RNumerics;

using static Godot.GeometryInstance3D;

namespace RhubarbVR.Bindings.ComponentLinking
{

	public sealed class SkinnedMeshRenderLink : MeshRenderLinkBase<SkinnedMeshRender>
	{
		public override string ObjectName => "SkinnedMeshRender";

		public override void StartContinueInit() {
			base.StartContinueInit();
			LinkedComp.Armature.Changed += Armature_Changed;
			Armature_Changed(null);
		}

		public override void Render() {
			base.Render();
			if (node.Mesh is null) {
				return;
			}
			var currentIndex = 0;
			for (var i = 0; i < LinkedComp.BlendShapes.Count; i++) {
				var item = LinkedComp.BlendShapes[i];
				if (node.Mesh._GetBlendShapeCount() <= currentIndex) {
					currentIndex++;
					continue;
				}
				node.SetBlendShapeValue(currentIndex, item.Weight.Value);
				currentIndex++;
			}
		}


		private void Armature_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				var target = LinkedComp.Armature.Target?.WorldLink;
				if (target is null) {
					node.Skeleton = null;
					return;
				}
				if (target is ArmatureLink armature) {
					node.Skeleton = armature.node.GetPath();
				}
			});
		}
	}

	public abstract class GeometryInstance3DBase<T, T2> : VisualInstance3DBase<T, T2> where T : RhuEngine.Components.GeometryInstance3D, new() where T2 : Godot.GeometryInstance3D, new()
	{
		public override void Init() {
			base.Init();
			LinkedComp.CastShadows.Changed += CastShadows_Changed;
			LinkedComp.Transparency.Changed += Transparency_Changed;
			LinkedComp.ExtraCullMargin.Changed += ExtraCullMargin_Changed;
			LinkedComp.IgnoreOcclusionCulling.Changed += IgnoreOcclusionCulling_Changed;
			LinkedComp.GlobalIlluminationMode.Changed += GlobalIlluminationMode_Changed;
			LinkedComp.LightMapScale.Changed += LightMapScale_Changed;
			LinkedComp.VisibilityBegin.Changed += VisibilityBegin_Changed;
			LinkedComp.VisibilityBeginMargin.Changed += VisibilityBeginMargin_Changed;
			LinkedComp.VisibilityEnd.Changed += VisibilityEnd_Changed;
			LinkedComp.VisibilityEndMargin.Changed += VisibilityEndMargin_Changed;
			LinkedComp.FadeMode.Changed += FadeMode_Changed;
			Transparency_Changed(null);
			ExtraCullMargin_Changed(null);
			IgnoreOcclusionCulling_Changed(null);
			GlobalIlluminationMode_Changed(null);
			LightMapScale_Changed(null);
			VisibilityBegin_Changed(null);
			VisibilityBeginMargin_Changed(null);
			VisibilityEnd_Changed(null);
			VisibilityEndMargin_Changed(null);
			FadeMode_Changed(null);
			CastShadows_Changed(null);
		}

		private void FadeMode_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.VisibilityRangeFadeMode = LinkedComp.FadeMode.Value switch { RFadeMode.Self => VisibilityRangeFadeModeEnum.Self, RFadeMode.Dependencies => VisibilityRangeFadeModeEnum.Dependencies, _ => VisibilityRangeFadeModeEnum.Disabled, });
		}

		private void VisibilityEndMargin_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.VisibilityRangeEndMargin = LinkedComp.VisibilityEndMargin.Value);
		}

		private void VisibilityEnd_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.VisibilityRangeEnd = LinkedComp.VisibilityEnd.Value);
		}

		private void VisibilityBeginMargin_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.VisibilityRangeBeginMargin = LinkedComp.VisibilityBeginMargin.Value);
		}

		private void VisibilityBegin_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.VisibilityRangeBegin = LinkedComp.VisibilityBegin.Value);
		}

		private void LightMapScale_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.GiLightmapScale = LinkedComp.LightMapScale.Value switch { RLightMapScale.TwoX => LightmapScale.Scale2x, RLightMapScale.FourX => LightmapScale.Scale4x, RLightMapScale.EightX => LightmapScale.Scale8x, _ => LightmapScale.Scale1x, });
		}

		private void GlobalIlluminationMode_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.GiMode = LinkedComp.GlobalIlluminationMode.Value switch { RGlobalIlluminationMode.Static => GIMode.Static, RGlobalIlluminationMode.Dynamic => GIMode.Dynamic, _ => GIMode.Disabled, });
		}

		private void IgnoreOcclusionCulling_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.IgnoreOcclusionCulling = LinkedComp.IgnoreOcclusionCulling.Value);
		}

		private void ExtraCullMargin_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ExtraCullMargin = LinkedComp.ExtraCullMargin.Value);
		}

		private void Transparency_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Transparency = LinkedComp.Transparency.Value);
		}

		private void CastShadows_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.CastShadow = LinkedComp.CastShadows.Value switch {
				ShadowCast.On => Godot.GeometryInstance3D.ShadowCastingSetting.On,
				ShadowCast.TwoSided => Godot.GeometryInstance3D.ShadowCastingSetting.DoubleSided,
				ShadowCast.ShadowsOnly => Godot.GeometryInstance3D.ShadowCastingSetting.ShadowsOnly,
				_ => Godot.GeometryInstance3D.ShadowCastingSetting.Off,
			});
		}


	}

	public abstract class VisualInstance3DBase<T, T2> : WorldPositionLinked<T, T2> where T : RhuEngine.Components.VisualInstance3D, new() where T2 : Godot.VisualInstance3D, new()
	{
		public override void Init() {
			base.Init();
			LinkedComp.renderLayer.Changed += RenderLayer_Changed;
			RenderLayer_Changed(null);
		}

		private void RenderLayer_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Layers = (uint)(int)LinkedComp.renderLayer.Value);
		}

	}

	public abstract class MeshRenderLinkBase<T> : GeometryInstance3DBase<T, MeshInstance3D> where T : MeshRender, new()
	{
		public override string ObjectName => "MeshRender";

		public override void StartContinueInit() {
			LinkedComp.mesh.LoadChange += Mesh_LoadChange;
			LinkedComp.materials.Changed += Materials_Changed;
			LinkedComp.colorLinear.Changed += MatUpdate;
			Materials_Changed(null);
			MatUpdate(null);
			Mesh_LoadChange(LinkedComp.mesh.Asset);
		}

		public List<AssetRef<RMaterial>> BoundTo = new();

		private void AssetUpdate(RMaterial rMaterial) {
			RenderThread.ExecuteOnEndOfFrameNoPass(MitReloadAction);
		}

		private void Materials_Changed(IChangeable obj) {
			lock (BoundTo) {
				foreach (var item in BoundTo) {
					item.LoadChange -= AssetUpdate;
				}
				BoundTo.Clear();
				lock (LinkedComp.materials.Lock) {
					foreach (var item in LinkedComp.materials) {
						var target = (AssetRef<RMaterial>)item;
						target.LoadChange += AssetUpdate;
						BoundTo.Add(target);
					}
				}
			}
			AssetUpdate(null);
		}

		private void MitReloadAction() {
			node.MaterialOverride = null;
			var amount = node.GetSurfaceOverrideMaterialCount();
			for (var i = 0; i < amount; i++) {
				node.SetSurfaceOverrideMaterial(i, null);
			}
			if (LinkedComp.materials.Count == 0) {
				return;
			}
			else if (amount == 1) {
				node.MaterialOverride = ((GodotMaterial)LinkedComp.materials[0].Asset?.Target)?.GetMatarial(LinkedComp.colorLinear.Value);
			}
			else {
				for (var i = 0; i < LinkedComp.materials.Count; i++) {
					var mat = ((GodotMaterial)LinkedComp.materials[i].Asset?.Target)?.GetMatarial(LinkedComp.colorLinear.Value);
					if(i < amount) {
						node.SetSurfaceOverrideMaterial(i, mat);
					}
				}
			}
		}

		private void MatUpdate(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(MitReloadAction);
		}

		private void Mesh_LoadChange(RhuEngine.Linker.RMesh obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (node is null) {
					return;
				}
				if (obj is null) {
					node.Mesh = null;
					node.Skin = null;
					return;
				}
				node.Mesh = ((GodotMesh)obj.Inst).LoadedMesh;
				node.Skin = ((GodotMesh)obj.Inst).LoadedSkin;
				Materials_Changed(null);
			});
		}

	}

	public sealed class MeshRenderLink : MeshRenderLinkBase<MeshRender>
	{

	}
}
