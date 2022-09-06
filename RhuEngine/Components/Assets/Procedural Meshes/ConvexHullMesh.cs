﻿using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;

using RNumerics;

namespace RhuEngine.Components
{
	public class ConvexHullMesh : ProceduralMesh
	{
		[OnChanged(nameof(LoadMesh))]
		public readonly SyncValueList<Vector3f> points = new();

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
