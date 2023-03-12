using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public sealed partial class ConvexHullMesh : ProceduralMesh
	{
		[OnChanged(nameof(LoadMesh))]
		public readonly SyncValueList<Vector3f> points;

		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> splitVerts;

		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}

			var mesh = new ConvexHullGenerator {
				points = points,
				splitVerts = splitVerts,
			};
			mesh.Generate();
			GenMesh(mesh.MakeSimpleMesh());
		}
	}
}
