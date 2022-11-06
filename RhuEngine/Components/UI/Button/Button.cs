using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RButtonAlignment {
		//
		// Summary:
		//     Horizontal left alignment, usually for text-derived classes.
		Left,
		//
		// Summary:
		//     Horizontal center alignment, usually for text-derived classes.
		Center,
		//
		// Summary:
		//     Horizontal right alignment, usually for text-derived classes.
		Right,
		//
		// Summary:
		//     Expand row to fit width, usually for text-derived classes.
		Fill
	}

	[Category("UI/Button")]
	public class Button : ButtonBase
	{
		public readonly Sync<string> Text;
		public readonly AssetRef<RTexture2D> Icon;
		public readonly Sync<bool> Flat;
		public readonly Sync<bool> ClipText;
		public readonly Sync<RButtonAlignment> Alignment;
		public readonly Sync<ROverrunBehavior> TextOverrunBehavior;
		public readonly Sync<RButtonAlignment> IconAlignment;
		public readonly Sync<bool> ExpandIcon;
		public readonly Sync<RTextDirection> TextDir;
		public readonly Sync<string> Language;

	}
}
