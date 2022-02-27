using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Visuals" })]
	public class UITintGroup : UIGroup
	{
		public Sync<Color> TintColor;

		public override void OnAttach() {
			TintColor.Value = Color.White;
		}

		public override void RenderUI() {
			UI.PushTint(TintColor);
			base.RenderUI();
			UI.PopTint();
		}
	}
}
