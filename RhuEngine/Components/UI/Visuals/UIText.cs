using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Visuals" })]
	public class UIText : UIComponent
	{
		[Default("Null")]
		public Sync<string> NullValue;
		public Sync<string> Text;

		[Default(TextAlign.TopLeft)]
		public Sync<TextAlign> Align;

		public Sync<float> Width;
		public override void RenderUI() {
			//UI.Text((Text.Value ?? NullValue.Value) ?? "", Align);
		}
	}
}
