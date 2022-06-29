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

		public override Vector2f CutZonesMax => Inherent ? MathUtil.Min(Max + ScrollOffset.Xy, ParentRect?.CutZonesMax ?? Max + ScrollOffset.Xy) : Max + ScrollOffset.Xy;
		public override Vector2f CutZonesMin => Inherent ? MathUtil.Max(Min + ScrollOffset.Xy, ParentRect?.CutZonesMin ?? Min + ScrollOffset.Xy) : Min + ScrollOffset.Xy;
	}
}
