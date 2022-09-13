using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Linq;
using System.Collections.Generic;
using System;
using static RhuEngine.Components.DynamicTextRender;
using SixLabors.Fonts;

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
	public sealed class UIText : MultiRenderUIComponent, ITextComp
	{
		public DynamicTextRender TextRender => textRender;

		public override Colorf[] RenderTint => throw new NotImplementedException();

		public RMaterial[] materials = Array.Empty<RMaterial>();

		public override RMaterial[] RenderMaterial => materials;

		public override bool UseSingle => true;

		public override Colorf RenderTintSingle => Colorf.White;

		[Default(true)]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<bool> FitText;

		[Default("Text Here")]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<string> Text;
		[Default("")]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<string> EmptyString;
		[Default("<color=rgb(0.9,0.9,0.9)>null")]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<string> NullString;
		[OnAssetLoaded(nameof(ForceUpdate))]
		public readonly AssetRef<RFont> Font;
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<Colorf> StartingColor;
		[Default(0.1f)]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<float> Leading;
		[OnChanged(nameof(ForceUpdate))]
		[Default(FontStyle.Regular)]
		public readonly Sync<FontStyle> StartingStyle;

		[OnChanged(nameof(ForceUpdate))]
		[Default(10f)]
		public readonly Sync<float> StatingSize;

		[Default(false)]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<bool> Password;

		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<Vector2f> MaxClamp;

		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<Vector2f> MinClamp;

		public DynamicTextRender textRender = new(true);

		[Default(EVerticalAlien.Center)]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<EVerticalAlien> VerticalAlien;

		[Default(EHorizontalAlien.Middle)]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<EHorizontalAlien> HorizontalAlien;

		public Matrix textOffset = Matrix.S(1);

		private void UpdateText() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			textRender.MaxClamp = MaxClamp;
			textRender.MinClamp = MinClamp;
			var newtext = Text.Value;
			if (Password.Value) {
				newtext = new string('●', newtext.Length);
			}
			if (newtext is null) {
				newtext = NullString.Value;
			}
			if (string.IsNullOrEmpty(newtext)) {
				newtext = EmptyString.Value;
			}
			textRender.LoadText(newtext, Font.Asset, Leading, StartingColor, StartingStyle, StatingSize, HorizontalAlien);
		}

		protected override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<MainFont>();
			StartingColor.Value = Colorf.White;
			MinClamp.Value = Vector2f.MinValue;
			MaxClamp.Value = Vector2f.MaxValue;
		}

		protected override void UpdateMesh() {
			UpdateText();
			materials = textRender.renderMits.ToArray();
			StandaredBaseMesh = new SimpleMesh[textRender.simprendermeshes.Count];
			for (var i = 0; i < textRender.simprendermeshes.Count; i++) {
				StandaredBaseMesh[i] = textRender.simprendermeshes[i];
				StandaredBaseMesh[i].Translate(-textRender.axisAlignedBox3F.Min.x, -textRender.axisAlignedBox3F.Min.y - 0.5f, 0.05f);
				StandaredBaseMesh[i].Scale(1 / UIRect.Canvas.scale.Value.x, 1 / UIRect.Canvas.scale.Value.y, 1 / UIRect.Canvas.scale.Value.z);
				var SizeOnCavas = new Vector2f(textRender.Width / UIRect.Canvas.scale.Value.x, textRender.Height / UIRect.Canvas.scale.Value.y);
				if (FitText) {
					var scaler = Math.Min(UIRect.CachedElementSize.x / SizeOnCavas.x, UIRect.CachedElementSize.y / SizeOnCavas.y);
					StandaredBaseMesh[i].Scale(scaler);
					SizeOnCavas *= scaler;
				}
				var y = VerticalAlien.Value switch {
					EVerticalAlien.Bottom => 0,
					EVerticalAlien.Center => (UIRect.CachedElementSize.y / 2) - (SizeOnCavas.y / 2),
					EVerticalAlien.Top => UIRect.CachedElementSize.y - SizeOnCavas.y,
					_ => 0,
				};
				var x = HorizontalAlien.Value switch {
					EHorizontalAlien.Left => 0,
					EHorizontalAlien.Middle => (UIRect.CachedElementSize.x / 2) - (SizeOnCavas.x / 2),
					EHorizontalAlien.Right => UIRect.CachedElementSize.x - SizeOnCavas.x,
					_ => 0,
				};
				StandaredBaseMesh[i].Translate(new Vector3d(x, y, UIRect.CachedDepth + 0.01));
			}
			if (!FitText) {
				//Todo: Tell uirect to have defrentCHildSize
			}
		}
	}
}
