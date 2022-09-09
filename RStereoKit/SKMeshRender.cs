using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.Components;
using RhuEngine.WorldObjects.ECS;
using System.Linq;
using StereoKit;
using RNumerics;
using RhuEngine;
using RhuEngine.WorldObjects;

namespace RStereoKit
{
	//NotSupprted
	public class BasicLight : EngineWorldLinkBase<Light>
	{
		public override void Init() {
		}

		public override void Remove() {
		}

		public override void Render() {
		}

		public override void Started() {
		}

		public override void Stopped() {
		}
	}

	public class SKArmiturer : EngineWorldLinkBase<Armature>
	{
		public override void Init() {
		}

		public override void Remove() {
		}

		public override void Render() {
		}

		public override void Started() {
		}

		public override void Stopped() {
		}
	}

	public class SKTempMeshRender : EngineWorldLinkBase<SkinnedMeshRender>
	{
		public override void Init() {
			//Not needed for StereoKit
		}

		public override void Remove() {
			//Not needed for StereoKit
		}

		public override void Render() {
			if (LinkedComp.mesh.Asset != null) {
				SKRender(LinkedComp.mesh.Asset, LinkedComp.materials.ToArray(), LinkedComp.Entity.GlobalTrans, LinkedComp.colorLinear.Value, LinkedComp.renderLayer.Value);
			}
		}

		private void SKRender(RMesh rMesh, ISyncObject[] mits, RNumerics.Matrix globalTrans, Colorf color, RhuEngine.Linker.RenderLayer layer) {
			var mesh = (SKRMesh)rMesh.Inst;
			for (var i = 0; i < mesh.Meshs.Length; i++) {
				mesh.Draw(((AssetRef<RMaterial>)mits[i% mits.Length]).Asset, globalTrans, color, LinkedComp.OrderOffset.Value, layer, i);
			}
		}

		public override void Started() {
			//Not needed for StereoKit
		}

		public override void Stopped() {
			//Not needed for StereoKit
		}
	}

	public class SKMeshRender : EngineWorldLinkBase<MeshRender>
	{
		public override void Init() {
			//Not needed for StereoKit
		}

		public override void Remove() {
			//Not needed for StereoKit
		}

		public override void Render() {
			if (LinkedComp.mesh.Asset != null) {
				SKRender(LinkedComp.mesh.Asset, LinkedComp.materials.ToArray(), LinkedComp.Entity.GlobalTrans, LinkedComp.colorLinear.Value, LinkedComp.renderLayer.Value);
			}
		}

		private void SKRender(RMesh rMesh, ISyncObject[] mits, RNumerics.Matrix globalTrans, Colorf color, RhuEngine.Linker.RenderLayer layer) {
			var mesh = (SKRMesh)rMesh.Inst;
			for (var i = 0; i < mesh.Meshs.Length; i++) {
				mesh.Draw(((AssetRef<RMaterial>)mits[i % mits.Length]).Asset, globalTrans, color, LinkedComp.OrderOffset.Value, layer, i);
			}
		}
		public override void Started() {
			//Not needed for StereoKit
		}

		public override void Stopped() {
			//Not needed for StereoKit
		}
	}
}
