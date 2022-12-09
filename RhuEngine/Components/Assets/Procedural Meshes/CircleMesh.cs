﻿using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public sealed class CircleMesh : ProceduralMesh
	{
		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> Radius;

		[Default(0.0f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> StartAngleDeg;

		[Default(360.0f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> EndAngleDeg;

		[Default(32)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<int> Slices;

		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> DoubleSided;

		protected override void FirstCreation() {
			base.FirstCreation();
		}

		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			var mesh = new TrivialDiscGenerator {
				Radius = Radius,
				StartAngleDeg = StartAngleDeg,
				EndAngleDeg = EndAngleDeg,
				Slices = Slices
			};
			mesh.Generate();
			if (DoubleSided) {
				var fllipedMesh = mesh.MakeSimpleMesh();
				fllipedMesh.MakeDoubleSided();
				GenMesh(fllipedMesh);
			}
			else {
				GenMesh(mesh.MakeSimpleMesh());
			}
		}
	}
}
