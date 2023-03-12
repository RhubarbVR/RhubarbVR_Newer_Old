using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RMenuButtonCheckableState {
		No,
		AsCheckbox,
		AsRadio
	}

	[Category("UI/Button")]
	public partial class MenuButton : Button
	{
		public readonly Sync<bool> SwitchOnHover;
		public readonly SyncObjList<MenuButtonItem> Items;
		
		public sealed partial class MenuButtonItem : SyncObject {
			public readonly Sync<string> Text;
			public readonly AssetRef<RTexture2D> Icon;
			public readonly Sync<RMenuButtonCheckableState> Checkable;
			public readonly Sync<bool> Checked;
			public readonly Sync<int> Id;
			public readonly Sync<bool> Disabled;
			public readonly Sync<bool> Separator;
		}
	}
}
