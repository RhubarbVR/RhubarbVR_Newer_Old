using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RUnderLine {
		Always,
		OnHover,
		Never
	}

	[Category("UI/Button")]
	public partial class LinkButton : ButtonBase
	{
		public readonly Sync<string> Text;
		public readonly Sync<RUnderLine> UnderLine;
		public readonly Sync<RTextDirection> TextDir;
		public readonly Sync<string> Language;
	}
}
