﻿using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI3D/Rects" })]
	public sealed class BasicScrollUI3DRect : RawScrollUI3DRect
	{
	}

	[Category(new string[] { "UI3D/Rects" })]
	public sealed class CustomScrollUI3DRect : RawScrollUI3DRect
	{
		public readonly Sync<Vector2f> Max_Scroll;
		public readonly Sync<Vector2f> Min_Scroll;
		public override Vector2f MaxScroll => Max_Scroll;
		public override Vector2f MinScroll => Min_Scroll;

		protected override void OnAttach() {
			base.OnAttach();
			Max_Scroll.Value = Vector2f.Inf;
			Min_Scroll.Value = Vector2f.NInf;
		}
	}
}