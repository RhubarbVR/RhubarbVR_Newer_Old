using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Visuals" })]
	public class UILabel : UIComponent
	{
		[Default("Null")]
		public Sync<string> NullValue;
		public Sync<string> Text;

		[Default(true)]
		public Sync<bool> UsePadding;

		public override void RenderUI() {
			UI.Label(Text.Value ?? NullValue.Value ?? "Null", UsePadding);
		}
	}
}
