using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{

	[Category(new string[] { "UI/Rects" })]
	public class Grid : RawScrollUIRect
	{
		[Default(2)]
		public readonly Sync<int> GridWidth;
	}
}
