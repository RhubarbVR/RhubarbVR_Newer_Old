using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Rects" })]
	public class CuttingUIRect : UIRect
	{
		public override Vector2f CutZonesMax => Max + ScrollOffset.Xy;
		public override Vector2f CutZonesMin => Min + ScrollOffset.Xy;
	}
}
