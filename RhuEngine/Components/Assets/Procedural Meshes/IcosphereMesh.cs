using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;

using RNumerics;

namespace RhuEngine.Components
{
	public class IcosphereMesh : ProceduralMesh
	{
		[Default(4)]
		[OnChanged(nameof(LoadMesh))]
		public int iterations = 4;
		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public float radius = 1f;
		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			var mesh = new IcosphereGenerator {
				iterations = iterations,
				radius = radius,
			};
			mesh.Generate();
			GenMesh(mesh.MakeSimpleMesh());
		}
	}
}
