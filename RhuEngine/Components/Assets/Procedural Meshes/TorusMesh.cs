using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;
using RNumerics;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class TorusMesh : ProceduralMesh
	{
		[Default(2.0f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> MajorRadius;

		[Default(0.5f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> MinorRadius;

		[Default(48)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<int> MajorSegments;

		[Default(12)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<int> MinorSegments;

		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			var mesh = new TorusGenerator {
				MajorRadius = MajorRadius,
				MinorRadius = MinorRadius,
				MajorSegments = MajorSegments,
				MinorSegments = MinorSegments,
			};
			mesh.Generate();
			GenMesh(mesh.MakeSimpleMesh());
		}
	}
}
