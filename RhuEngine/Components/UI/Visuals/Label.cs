using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RVisibleCharactersBehavior {
		CharactersBeforeShaping,
		CharactersAfterShaping,
		GlyphsLayoutDir,
		GlyphsLayoutLeftToRight,
		GlyphsLayoutRightToLeft,
	}

	[Category("UI/Container/Visuals")]
	public class TextLabel : UIVisuals
	{
		public readonly Sync<int> LineSpacing;
		public readonly AssetRef<RFont> Font;
		public readonly Sync<int> TextSize;
		public readonly Sync<Colorf> TextColor;
		public readonly Sync<int> OutlineSize;
		public readonly Sync<Colorf> OutlineColor;
		[Default(1)]public readonly Sync<int> ShadowSize;
		public readonly Sync<Colorf> ShadowColor;
		public readonly Sync<Vector2i> ShadowOffset;
		[Default(RHorizontalAlignment.Center)]public readonly Sync<RHorizontalAlignment> HorizontalAlignment;
		[Default(RVerticalAlignment.Center)]public readonly Sync<RVerticalAlignment> VerticalAlignment;
		[Default(RAutowrapMode.Off)]public readonly Sync<RAutowrapMode> AutowrapMode;
		public readonly Sync<bool> ClipText;
		public readonly Sync<ROverrunBehavior> OverrunBehavior;
		public readonly Sync<bool> Uppercase;
		public readonly Sync<int> LinesSkipped;
		[Default(-1)]public readonly Sync<int> MaxLinesVisible;
		public readonly Sync<RVisibleCharactersBehavior> VisibleCharactersBehavior;
		[Default(1f)]public readonly Sync<float> VisibleRatio;
		public readonly Sync<RTextDirection> TextDir;
		public readonly Sync<string> Language;

	}
}
