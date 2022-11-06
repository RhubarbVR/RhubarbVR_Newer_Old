using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Container/Visuals")]
	public class RichTextLabel : UIVisuals
	{
		[Default(true)]public readonly Sync<bool> BitcodeEnabled;
		public readonly Sync<string> Text;
		public readonly Sync<bool> FitContentHeight;
		[Default(true)]public readonly Sync<bool> ScrollActive;
		public readonly Sync<bool> ScrollFollowing;
		public readonly Sync<RAutowrapMode> AutoWrapMode;
		[Default(4)]public readonly Sync<int> TabSize;
		public readonly Sync<bool> ContextMenuEnabled;
		[Default(true)]public readonly Sync<bool> ShortcutKeysEnabled;
		[Default(true)]public readonly Sync<bool> MetaUnderline;
		[Default(true)]public readonly Sync<bool> HintUnderline;
		[Default(true)]public readonly Sync<bool> Threading;
		[Default(500)]public readonly Sync<int> ProgressBarDelay;
		public readonly Sync<bool> TextSelectionEnabled;
		[Default(true)]public readonly Sync<bool> DeselectingOnFocusLossEnabled;
		public readonly Sync<RVisibleCharactersBehavior> VisibleCharactersBehavior;
		[Default(1f)]public readonly Sync<float> VisibleRatio;
		public readonly Sync<RTextDirection> TextDir;
		public readonly Sync<string> Language;
	}
}
