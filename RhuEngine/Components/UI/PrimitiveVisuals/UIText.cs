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
		[Default("<style=regular><color=red>H\nell>o<size5><color=blue> W<>or\nld<size10><color=red>!!!Sthe\n<size7>size willbreak\n this<size25><colorgreen>No <size10><colorblack>it does not\nI\nlike\nnew\n\nlines</color></color></color></color></color>")]
		//[Default("Hello THIS IS A STRING THAT IS BIG THE MORE I TYPE THE LESS ROOM")]
		//[Default("Hello")]
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

		[Default(true)]
		public Sync<bool> CenterX;
		
		[Default(true)]
		public Sync<bool> CenterY;

		[Default(true)]
		public Sync<bool> KeepAspectRatio;

		public Matrix textOffset = Matrix.S(1);

		private void UpdateText() {
			textRender.LoadText(Pointer.ToString(), Text, Font.Asset, StartingColor, StartingStyle, StatingSize);
			UpdateTextOffset();
		}

		public override void OnLoaded() {
			base.OnLoaded();
			UpdateText();
		}

		public void UpdateTextOffset() {
			var max = Rect.Max;
			var min = Rect.Min;
			var boxsize = max - min;
			boxsize /= Math.Max(boxsize.x, boxsize.y);
			var canvassize = Entity.UIRect.Canvas?.scale.Value.Xy ?? Vector2f.One;
			var texture = Vector2f.One;
			if (KeepAspectRatio.Value) {
				texture = new Vector2f(textRender.axisAlignedBox3F.Width, textRender.axisAlignedBox3F.Height);
				texture /= canvassize;
				texture /= boxsize;
				texture /= Math.Max(texture.x, texture.y);
			}
			var maxmin = (max - min) * texture;
			var maxoffset = maxmin + min;
			var minoffset = min;
			var offset = (max - min - maxmin) / 2;
			if (CenterX) {
				maxoffset = new Vector2f(maxoffset.x + offset.x, maxoffset.y);
				minoffset = new Vector2f(minoffset.x + offset.x, minoffset.y);
			}
			if (CenterY) {
				maxoffset = new Vector2f(maxoffset.x, maxoffset.y + offset.y);
				minoffset = new Vector2f(minoffset.x, minoffset.y + offset.y);
			}
			var textscale = (max - min).XY_ / Math.Max(textRender.axisAlignedBox3F.Width, textRender.axisAlignedBox3F.Height);
			textOffset = Matrix.TS(new Vector3f(minoffset.x, maxoffset.y, Rect.StartPoint + 0.01f), textscale);
		}

		public override void RenderTargetChange() {
			UpdateTextOffset();
		}

		public override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<DefaultFont>();
		}

		public override void Render(Matrix matrix) {
			textRender.Render(textOffset * matrix);
		}
	}
}
