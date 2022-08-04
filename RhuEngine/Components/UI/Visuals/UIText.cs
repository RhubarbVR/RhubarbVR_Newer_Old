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
	public class UIText : UIComponent,ITextComp
	{
		public event Action<Matrix, TextChar,int> OnCharRender;
		public DynamicTextRender TextRender => textRender;

		[Default("Text Here")]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<string> Text;
		[Default("")]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<string> EmptyString;
		[Default("<color=rgb(0.9,0.9,0.9)>null")]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<string> NullString;
		[OnAssetLoaded(nameof(UpdateText))]
		public readonly AssetRef<RFont> Font;
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<Colorf> StartingColor;
		[Default(0.1f)]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<float> Leading;
		[OnChanged(nameof(UpdateText))]
		[Default(FontStyle.Regular)]
		public readonly Sync<FontStyle> StartingStyle;

		[OnChanged(nameof(UpdateText))]
		[Default(10f)]
		public readonly Sync<float> StatingSize;

		[Default(false)]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<bool> Password;

		[OnChanged(nameof(UpdateText))]
		public readonly Sync<Vector2f> MaxClamp;

		[OnChanged(nameof(UpdateText))]
		public readonly Sync<Vector2f> MinClamp;

		public DynamicTextRender textRender = new(true);

		[Default(EVerticalAlien.Center)]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<EVerticalAlien> VerticalAlien;

		[Default(EHorizontalAlien.Middle)]
		[OnChanged(nameof(UpdateText))]
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
			if(newtext is null) {
				newtext = NullString.Value;
			}
			if (string.IsNullOrEmpty(newtext)) {
				newtext = EmptyString.Value;
			}
			textRender.LoadText(Pointer.ToString(), newtext, Font.Asset, Leading, StartingColor, StartingStyle, StatingSize, VerticalAlien, HorizontalAlien);
		}

		public override void OnLoaded() {
			base.OnLoaded();
			UpdateText();
		}

		public override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<MainFont>();
			StartingColor.Value = Colorf.White;
			MinClamp.Value = Vector2f.MinValue;
			MaxClamp.Value = Vector2f.MaxValue;
		}
	}
}
