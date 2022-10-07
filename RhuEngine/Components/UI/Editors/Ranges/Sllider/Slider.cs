using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Editors/Ranges/Slider")]
	public abstract class Slider : Range
	{
		[Default(true)]
		public readonly Sync<bool> Editable;
		[Default(true)]
		public readonly Sync<bool> Scrollable;
		public readonly Sync<int> TickCount;
		public readonly Sync<bool> TickOnBorders;
	}
}
