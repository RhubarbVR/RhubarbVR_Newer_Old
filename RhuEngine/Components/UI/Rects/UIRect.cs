using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI/Rects" })]
	public class UIRect : Component
	{

		public readonly Sync<Vector2f> OffsetLocalMin;
		public readonly Sync<Vector2f> OffsetLocalMax;
		public readonly Sync<Vector2f> OffsetMin;
		public readonly Sync<Vector2f> OffsetMax;
		public readonly Sync<Vector2f> AnchorMin;
		public readonly Sync<Vector2f> AnchorMax;

		[Default(0.05f)]
		public readonly Sync<float> Depth;

		public override void OnAttach() {
			base.OnAttach();
			AnchorMin.Value = Vector2f.Zero;
			AnchorMax.Value = Vector2f.One;
			OffsetMin.Value = Vector2f.Zero;
			OffsetMax.Value = Vector2f.Zero;
		}

	}
}
