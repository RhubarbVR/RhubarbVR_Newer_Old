using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine.Components;

namespace RhubarbVR.Bindings.ComponentLinking
{

	public class SkinnedMeshRenderLink : WorldPositionLinked<SkinnedMeshRender, Node3D>
	{
		public override string ObjectName => "SkinnedMeshRender";

		public override void StartContinueInit() {
		}
	}

	public class MeshRenderLink : WorldPositionLinked<MeshRender, Node3D>
	{
		public override string ObjectName => "MeshRender";

		public override void StartContinueInit() {
		}
	}
}
