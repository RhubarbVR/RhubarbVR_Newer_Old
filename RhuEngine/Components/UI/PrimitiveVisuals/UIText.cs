using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Linq;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI/PrimitiveVisuals" })]
	public class UIText : UIComponent
	{
		[Default("<style=regular><color=red>H\nell>o<size5><color=blue> W<>or\nld<size10><color=red>!!!Sthe\n<size7>size willbreak\n this</color></color></color></color></color>")]
		[OnChanged(nameof(UpdateText))]
		public Sync<string> Text;
		[OnChanged(nameof(UpdateText))]
		public AssetRef<RFont> Font;
		[OnChanged(nameof(UpdateText))]
		public Sync<Colorf> StartingColor;

		[OnChanged(nameof(UpdateText))]
		[Default(FontStyle.Regular)]
		public Sync<FontStyle> StartingStyle;

		[OnChanged(nameof(UpdateText))]
		[Default(10f)]
		public Sync<float> StatingSize;

		public UITextRender textRender = new();

		private void UpdateText() {
			textRender.LoadText(Pointer.ToString(), Text, Font.Asset, StartingColor, StartingStyle, StatingSize);
		}

		public override void OnLoaded() {
			base.OnLoaded();
			UpdateText();
		}

		public override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<DefaultFont>();
		}

		public override void Render(Matrix matrix) {
			var canvassize = Entity.UIRect.Canvas?.scale.Value.Xy ?? Vector2f.One;
			var sizetext = new Vector2f(textRender.axisAlignedBox3F.Width, textRender.axisAlignedBox3F.Height);
			sizetext /= canvassize / Math.Max(canvassize.x, canvassize.y);
			var rootmat = Matrix.TS(new Vector3f(0, 0, Rect.StartPoint + 0.01f), sizetext.XY_) * matrix;
			textRender.Render(rootmat);
		}
	}
}
