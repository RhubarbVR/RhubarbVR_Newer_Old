using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class MobiusStripMesh : ProceduralMesh
	{
		[Default(100)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<int> planeResolution;
		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			var mesh = new MobiusStripGenerator {
				planeResolution = planeResolution,
			};
			mesh.Generate();
			GenMesh(mesh.MakeSimpleMesh());
		}
	}
}
