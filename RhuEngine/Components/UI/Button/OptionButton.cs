using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Button")]
	public class OptionButton : Button
	{
		protected override bool AddToUpdateList => true;

		[Default(-1)]
		public readonly Sync<int> Selected;
		[Default(true)]
		public readonly Sync<bool> FitLongestItem;

		public readonly SyncObjList<MenuButtonItem> Items;

		public class MenuButtonItem : SyncObject
		{
			public readonly Sync<string> Text;
			public readonly Sync<string> ToolTip;
			public readonly AssetRef<RTexture2D> Icon;
			public readonly Sync<int> Id;
			public readonly Sync<bool> Disabled;
			public readonly Sync<bool> Separator;
		}
	}
}
