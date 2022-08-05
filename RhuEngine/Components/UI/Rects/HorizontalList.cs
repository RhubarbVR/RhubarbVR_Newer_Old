using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{

	[Category(new string[] { "UI/Rects" })]
	public class HorizontalList : UIRect
	{
		[Default(false)]
		public readonly Sync<bool> Fit;
	}
}
