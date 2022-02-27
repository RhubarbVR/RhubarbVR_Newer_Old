using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Visuals" })]
	public class UIEnableGroup : UIGroup
	{
		[Default(true)]
		public Sync<bool> UIEnabled;

		public override void RenderUI() {
			UI.PushEnabled(UIEnabled);
			base.RenderUI();
			UI.PopEnabled();
		}
	}
}
