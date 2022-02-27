using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Visuals" })]
	public class UIHSeparator : UIComponent
	{
		public override void RenderUI() {
			UI.HSeparator();
		}
	}
}
