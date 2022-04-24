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

	[Category(new string[] { "UI/Visuals" })]
	public class UIText : UIComponent
	{
		[Default("<color=hsv(240,100,100)>Hello<color=blue><size14>World \n <size5>Trains \n are cool man<size10>\nHello ")]
		[OnChanged(nameof(UpdateText))]
		public Sync<string> Text;
		[OnAssetLoaded(nameof(UpdateText))]
		public AssetRef<RFont> Font;
		[OnChanged(nameof(UpdateText))]
		public Sync<Colorf> StartingColor;
		[Default(0f)]
		[OnChanged(nameof(UpdateText))]
		public Sync<float> Leading;
		[OnChanged(nameof(UpdateText))]
		[Default(FontStyle.Regular)]
		public Sync<FontStyle> StartingStyle;

		[OnChanged(nameof(UpdateText))]
		[Default(10f)]
		public Sync<float> StatingSize;

		public DynamicTextRender textRender = new();

		[Default(EVerticalAlien.Center)]
		[OnChanged(nameof(UpdateText))]
		public Sync<EVerticalAlien> VerticalAlien;

		[Default(EHorizontalAlien.Middle)]
		[OnChanged(nameof(UpdateText))]
		public Sync<EHorizontalAlien> HorizontalAlien;

		[Default(true)]
		[OnChanged(nameof(UpdateText))]
		public Sync<bool> MiddleLines;

		public Matrix textOffset = Matrix.S(1);

		private void UpdateText() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			textRender.LoadText(Pointer.ToString(), Text, Font.Asset, Leading, StartingColor, StartingStyle, StatingSize, VerticalAlien, HorizontalAlien,MiddleLines);
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
			maxmin /= Math.Min(textRender.axisAlignedBox3F.Width, textRender.axisAlignedBox3F.Height);
			textOffset = Matrix.TS(new Vector3f(upleft.x, upleft.y, Rect.StartPoint + 0.01f), new Vector3f(Math.Min(maxmin.x, maxmin.y)/2) + new Vector3f(0,0,Rect.Canvas.scale.Value.z));
			CutElement(true,false);
		}

		public override void RenderTargetChange() {
			UpdateTextOffset();
		}

		public override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<DefaultFont>();
		}

		public override void Render(Matrix matrix) {
			textRender.Render(textOffset, Matrix.T(Rect.ScrollOffset) * matrix);
		}

		public override void CutElement(bool cut,bool update) {
			var min = Rect.CutZonesMin;
			var max = Rect.CutZonesMax;
			if (cut) {
				textRender.Chars.SafeOperation((list) => {
					foreach (var chare in list) {
						var charbottomleft = (chare.p * textOffset * Matrix.T(Rect.ScrollOffset)).Translation.Xy;
						var charbottomright = (Matrix.T(new Vector3f(chare.textsize.x,0,0) + chare.p.Translation) * textOffset * Matrix.T(Rect.ScrollOffset)).Translation.Xy;
						var chartopleft = (Matrix.T(new Vector3f(0,chare.textsize.y,0) + chare.p.Translation) * textOffset * Matrix.T(Rect.ScrollOffset)).Translation.Xy;
						var chartopright = (Matrix.T(chare.textsize.XY_ + chare.p.Translation) * textOffset * Matrix.T(Rect.ScrollOffset)).Translation.Xy;
						var bottomleft = max.IsInBox(min, charbottomleft);
						var bottomright = max.IsInBox(min, charbottomright);
						var topleft = max.IsInBox(min, chartopleft);
						var topright = max.IsInBox(min, chartopright);
						if (!(bottomleft || bottomright || topleft || topright)) {
							chare.Cull = true;
							continue;
						}
						chare.Cull = false;
						if (bottomleft && bottomright && topleft && topright) {
							chare.textCut = Vector2f.Zero;
							continue;
						}
						var ycut = 0f;
						var xcut = 0f;
						if (topleft && topright) {
							var newpos = Vector2f.MinMaxIntersect(charbottomleft, min, max).y;
							ycut = -((charbottomleft.y - newpos) / (chartopleft.y - charbottomleft.y));
						}
						if (bottomleft && bottomright) {
							var newpos = Vector2f.MinMaxIntersect(chartopright, min, max).y;
							ycut = -(chartopright.y - newpos) / (chartopleft.y - charbottomleft.y);
						}
						if (bottomleft && topleft) {
							var newpos = Vector2f.MinMaxIntersect(chartopright, min, max).x;
							xcut = -((chartopright.x - newpos) / (chartopright.x - chartopleft.x));
						}
						if (bottomright && topright) {
							var newpos = Vector2f.MinMaxIntersect(charbottomleft, min, max).x;
							xcut = -((charbottomleft.x - newpos) / (chartopright.x - chartopleft.x));
						}
						chare.textCut = new Vector2f(xcut, ycut);
					}
				});
			}
			else {
				textRender.Chars.SafeOperation((list) => {
					foreach (var chare in list) {
						chare.textCut = Vector2f.Zero;
						chare.Cull = false;
					}
				});
			}
			textRender.Chars.SafeOperation((list) => {
				foreach (var chare in list) {
					var newMat = Rect.MatrixMove(chare.p * textOffset * Matrix.T(Rect.ScrollOffset));
					chare.Offset = Matrix.T(newMat.Translation);
					chare.Offset2 = Matrix.R(newMat.Rotation);
				}
			});
		}
	}
}
