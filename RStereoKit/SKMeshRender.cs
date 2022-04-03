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
				SKRender(RenderingComponent.mesh.Asset, RenderingComponent.materials,RenderingComponent.Entity.GlobalTrans, RenderingComponent.colorLinear.Value, RenderingComponent.renderLayer.Value);
			}
		}

		private void SKRender(RMesh rMesh,IEnumerable<ISyncObject> mits, RNumerics.Matrix globalTrans,Colorf color,RhuEngine.Linker.RenderLayer layer) {
			foreach (AssetRef<RMaterial> item in mits) {
				if (item.Asset != null) {
					((Mesh)rMesh.mesh)?.Draw((Material)item.Asset.Target, new StereoKit.Matrix(globalTrans.m), new Color(color.r, color.g, color.b, color.a), (StereoKit.RenderLayer)layer);
				}
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
