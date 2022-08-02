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
	public class BasicLight : RenderLinkBase<Light>
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

	public class UIRender : RenderLinkBase<UICanvas>
	{
		public override void Init() {
		}

		public override void Remove() {
		}

		public override void Render() {
			RenderingComponent?.RenderUI();
		}

		public override void Started() {
		}

		public override void Stopped() {
		}
	}

	public class SKTextRender : RenderLinkBase<WorldText>
	{
		public override void Init() {
			//Not needed for StereoKit
		}

		public override void Remove() {
			//Not needed for StereoKit
		}

		public override void Render() {
			RenderingComponent.textRender.Render(RNumerics.Matrix.S(0.1f), RenderingComponent.Entity.GlobalTrans,RenderingComponent.TargetRenderLayer);
		}

		public override void Started() {
			//Not needed for StereoKit
		}

		public override void Stopped() {
			//Not needed for StereoKit
		}
	}

	public class SKArmiturer : RenderLinkBase<Armature>
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

	public class SKTempMeshRender : RenderLinkBase<SkinnedMeshRender>
	{
		public override void Init() {
			//Not needed for StereoKit
		}

		public override void Remove() {
			//Not needed for StereoKit
		}

		public override void Render() {
			if (RenderingComponent.mesh.Asset != null) {
				SKRender(RenderingComponent.mesh.Asset, RenderingComponent.materials.ToArray(), RenderingComponent.Entity.GlobalTrans, RenderingComponent.colorLinear.Value, RenderingComponent.renderLayer.Value);
			}
		}

		private void SKRender(RMesh rMesh, ISyncObject[] mits, RNumerics.Matrix globalTrans, Colorf color, RhuEngine.Linker.RenderLayer layer) {
			var mesh = (SKRMesh)rMesh.Inst;
			for (var i = 0; i < mesh.Meshs.Length; i++) {
				mesh.Draw(((AssetRef<RMaterial>)mits[i% mits.Length]).Asset, globalTrans, color, RenderingComponent.OrderOffset.Value, layer, i);
			}
		}

		public override void Started() {
			//Not needed for StereoKit
		}

		public override void Stopped() {
			//Not needed for StereoKit
		}
	}

	public class SKMeshRender : RenderLinkBase<MeshRender>
	{
		public override void Init() {
			//Not needed for StereoKit
		}

		public override void Remove() {
			//Not needed for StereoKit
		}

		public override void Render() {
			if (RenderingComponent.mesh.Asset != null) {
				SKRender(RenderingComponent.mesh.Asset, RenderingComponent.materials.ToArray(), RenderingComponent.Entity.GlobalTrans, RenderingComponent.colorLinear.Value, RenderingComponent.renderLayer.Value);
			}
		}

		private void SKRender(RMesh rMesh, ISyncObject[] mits, RNumerics.Matrix globalTrans, Colorf color, RhuEngine.Linker.RenderLayer layer) {
			var mesh = (SKRMesh)rMesh.Inst;
			for (var i = 0; i < mesh.Meshs.Length; i++) {
				mesh.Draw(((AssetRef<RMaterial>)mits[i % mits.Length]).Asset, globalTrans, color, RenderingComponent.OrderOffset.Value, layer, i);
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
