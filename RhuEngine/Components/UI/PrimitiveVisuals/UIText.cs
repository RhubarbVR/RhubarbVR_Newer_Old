using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Linq;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{
	public enum EVerticalAlien
	{
		Bottom,
		Center,
		Top,
	}
	public enum EHorizontalAlien
	{
		Left,
		Middle,
		Right,
	}

	[Category(new string[] { "UI/PrimitiveVisuals" })]
	public class UIText : UIComponent
	{
		[Default("Hello World")]
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

		[Default(EVerticalAlien.Center)]
		[OnChanged(nameof(UpdateText))]
		public Sync<EVerticalAlien> VerticalAlien;

		[Default(EHorizontalAlien.Left)]
		[OnChanged(nameof(UpdateText))]
		public Sync<EHorizontalAlien> HorizontalAlien;

		[Default(false)]
		[OnChanged(nameof(UpdateText))]
		public Sync<bool> MiddleLines;

		public Matrix textOffset = Matrix.S(1);

		private void UpdateText() {
			textRender.LoadText(Pointer.ToString(), Text, Font.Asset, StartingColor, StartingStyle, StatingSize, VerticalAlien, HorizontalAlien,MiddleLines);
			UpdateTextOffset();
		}

		public override void OnLoaded() {
			base.OnLoaded();
			UpdateText();
		}

		public void UpdateTextOffset() {
			var startDepth = new Vector3f(0, 0, Entity.UIRect.StartPoint);
			var depth = new Vector3f(0, 0, Entity.UIRect.Depth.Value);
			var depthStart = startDepth + depth;
			var upleft = depthStart;
			var max = Rect.Max;
			var min = Rect.Min;
			var boxsize = max - min;
			boxsize /= Math.Max(boxsize.x, boxsize.y);
			var canvassize = Entity.UIRect.Canvas?.scale.Value.Xy ?? Vector2f.One;
			var texture = new Vector2f(textRender.axisAlignedBox3F.Width, textRender.axisAlignedBox3F.Height) * 10;
			Console.WriteLine("Text size is " + texture);
			texture /= canvassize;
			texture /= boxsize;
			texture /= Math.Max(texture.x, texture.y);
			var maxmin = (max - min) * texture;
			var maxoffset = maxmin + min;
			var minoffset = min;

			var offset = (max - min - maxmin) / 2;
			if (HorizontalAlien == EHorizontalAlien.Middle) {
				maxoffset = new Vector2f(maxoffset.x + offset.x, maxoffset.y);
				minoffset = new Vector2f(minoffset.x + offset.x, minoffset.y);
			}
			if (VerticalAlien == EVerticalAlien.Center) {
				maxoffset = new Vector2f(maxoffset.x, maxoffset.y + offset.y);
				minoffset = new Vector2f(minoffset.x, minoffset.y + offset.y);
			}
			if (HorizontalAlien == EHorizontalAlien.Right) {
				maxoffset = new Vector2f(max.x, maxoffset.y);
				minoffset = new Vector2f(max.x - maxmin.x, minoffset.y);
			}
			if (VerticalAlien == EVerticalAlien.Top) {
				maxoffset = new Vector2f(maxoffset.x, max.y);
				minoffset = new Vector2f(minoffset.x, max.y - maxmin.y);
			}
			upleft += new Vector3f(minoffset.x, maxoffset.y);
			var textscale = Vector2f.One;
			textscale /= canvassize;
			textscale /= boxsize;
			textscale /= Math.Max(textscale.x, textscale.y);
			textscale /= Math.Max(textRender.axisAlignedBox3F.Width, textRender.axisAlignedBox3F.Height * 2) * 5;
			textOffset = Matrix.TS(new Vector3f(upleft.x, upleft.y, Rect.StartPoint + 0.01f), textscale.XY_);
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
