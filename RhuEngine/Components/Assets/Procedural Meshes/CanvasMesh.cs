using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public sealed partial class CanvasMesh : ProceduralMesh
	{
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2i> Resolution;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2i> MinOffset;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2i> MaxOffset;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector3f> Scale;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2f> Min;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2f> Max;

		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> TopOffset;

		[Default(3f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> TopOffsetValue;

		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> FrontBind;

		[Default(20)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<int> FrontBindSegments;

		[Default(135f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> FrontBindAngle;

		[Default(7.5f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> FrontBindRadus;

		public override void ComputeMesh() {
			var min = Min.Value + ((Vector2f)MinOffset.Value / (Vector2f)Resolution.Value);
			var max = Max.Value + ((Vector2f)MaxOffset.Value / (Vector2f)Resolution.Value);
			var rectSize = max - min;
			var rectMin = min;
			var newMeshGen = new TrivialRectGenerator {
				IndicesMap = new Index2i(1, 2),
			};
			var NewMesh = newMeshGen.Generate().MakeSimpleMesh();
			NewMesh.Scale(1, -1, 1);
			NewMesh.Translate(0.5f, 0.5f, 0);
			NewMesh = NewMesh.Cut(rectSize + rectMin, rectMin);
			if (TopOffset.Value) {
				NewMesh.OffsetTop(TopOffsetValue.Value);
			}
			if (FrontBind.Value) {
				NewMesh = NewMesh.UIBind(FrontBindAngle.Value, FrontBindRadus.Value, FrontBindSegments.Value, Scale);
			}
			NewMesh.Scale(Scale.Value.x / 10, Scale.Value.y / 10, Scale.Value.z / 10);
			NewMesh.Translate(-(Scale.Value.x / 20), -(Scale.Value.y / 20), 0);
			GenMesh(NewMesh);
		}

		protected override void OnAttach() {
			base.OnAttach();
			Scale.Value = new Vector3f(16, 9, 1);
			Max.Value = Vector2f.One;
		}
	}
}
