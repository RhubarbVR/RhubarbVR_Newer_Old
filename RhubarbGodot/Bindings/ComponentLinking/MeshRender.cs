using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;

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

		private void Armature_Changed(IChangeable obj) {
			return; //Todo fix armature problems
			var target = LinkedComp.Armature.Target?.WorldLink;
			if (target is null) {
				node.Skeleton = null;
				return;
			}

			if (target is ArmatureLink armature) {
				node.Skeleton = armature.node.GetPath();
			}
		}
	}

	public abstract class MeshRenderLinkBase<T> : WorldPositionLinked<T, MeshInstance3D> where T : MeshRender, new()
	{
		public override string ObjectName => "MeshRender";

		public override void StartContinueInit() {
			LinkedComp.mesh.LoadChange += Mesh_LoadChange;
			LinkedComp.materials.Changed += Materials_Changed;
			LinkedComp.colorLinear.Changed += MatUpdate;
			LinkedComp.zOrderOffset.Changed += MatUpdate;
			LinkedComp.renderLayer.Changed += RenderLayer_Changed;
			LinkedComp.CastShadows.Changed += CastShadows_Changed;
			Materials_Changed(null);
			RenderLayer_Changed(null);
			MatUpdate(null);
			CastShadows_Changed(null);
			Mesh_LoadChange(LinkedComp.mesh.Asset);
		}

		private void CastShadows_Changed(IChangeable obj) {
			node.CastShadow = LinkedComp.CastShadows.Value switch {
				ShadowCast.On => GeometryInstance3D.ShadowCastingSetting.On,
				ShadowCast.TwoSided => GeometryInstance3D.ShadowCastingSetting.DoubleSided,
				ShadowCast.ShadowsOnly => GeometryInstance3D.ShadowCastingSetting.ShadowsOnly,
				_ => GeometryInstance3D.ShadowCastingSetting.Off,
			};
		}

		private void RenderLayer_Changed(IChangeable obj) {
			node.Layers = (uint)(int)LinkedComp.renderLayer.Value;
		}

		public List<AssetRef<RMaterial>> BoundTo = new();

		private void AssetUpdate(RMaterial rMaterial) {
			RUpdateManager.ExecuteOnStartOfFrame(this, MitReloadAction);
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
			else if (LinkedComp.materials.Count == 1) {
				node.MaterialOverride = ((GodotMaterial)LinkedComp.materials[0].Asset?.Target)?.GetMatarial(LinkedComp.colorLinear.Value, LinkedComp.zOrderOffset.Value);
			}
			else {
				for (var i = 0; i < LinkedComp.materials.Count; i++) {
					var mat = ((GodotMaterial)LinkedComp.materials[i].Asset?.Target)?.GetMatarial(LinkedComp.colorLinear.Value, LinkedComp.zOrderOffset.Value);
					node.SetSurfaceOverrideMaterial(i, mat);
				}
			}
		}

		private void MatUpdate(RhuEngine.WorldObjects.IChangeable obj) {
			RUpdateManager.ExecuteOnStartOfFrame(this, MitReloadAction);
		}

		private void Mesh_LoadChange(RhuEngine.Linker.RMesh obj) {
			if (obj is null) {
				node.Mesh = null;
				return;
			}
			node.Mesh = ((GodotMesh)obj.Inst).LoadedMesh;
		}
	}

	public sealed class MeshRenderLink : MeshRenderLinkBase<MeshRender>
	{

	}
}
