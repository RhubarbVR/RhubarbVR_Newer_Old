using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RScrollBarVisibility
	{
		Disable,
		Auto,
		AlwaysShow,
		NeverShow
	}

	[Category("UI/Container")]
	public partial class ScrollContainer : Container
	{
		[Default(true)] 
		public readonly Sync<bool> FollowFocus;
		[Default(RScrollBarVisibility.Auto)] 
		public readonly Sync<RScrollBarVisibility> HorizontalScrollBar;
		[Default(RScrollBarVisibility.Auto)] 
		public readonly Sync<RScrollBarVisibility> VerticalScrollBar;
		public readonly Sync<int> ScrollDeadZone;
		public readonly Sync<int> HorizontalScroll;
		public readonly Sync<int> VerticalScroll;

		protected override void OnAttach() {
			base.OnAttach();
			ClipContents.Value = true;
		}

	}
}
