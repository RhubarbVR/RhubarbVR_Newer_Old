using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public sealed class IcosphereMesh : ProceduralMesh
	{
		[Default(8)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<int> iterations;

		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> Radius;

		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			var mesh = new IcosphereGenerator {
				iterations = iterations,
				radius = Radius,
			};
			mesh.Generate();
			GenMesh(mesh.MakeSimpleMesh());
		}
	}
}
