using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RTabAlignment {
		Left,
		Center,
		Right,
	}

	[Category("UI/Container")]
	public class TabContainer : Container
	{
		public readonly Sync<RTabAlignment> TabAlignment;
		public readonly Sync<int> CurrentTab;
		[Default(true)]
		public readonly Sync<bool> ClipTabs;
		[Default(true)]
		public readonly Sync<bool> TabsVisible;
		[Default(-1)]
		public readonly Sync<int> RangeGroup;
		public readonly Sync<bool> UseHiddenTabsForMinSize;
	}
}
