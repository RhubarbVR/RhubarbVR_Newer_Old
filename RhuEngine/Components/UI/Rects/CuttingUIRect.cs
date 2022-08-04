using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI/Rects" })]
	public class CuttingUIRect : UIRect
	{
		[Default(true)]
		public readonly Sync<bool> Inherent;
	}
}
