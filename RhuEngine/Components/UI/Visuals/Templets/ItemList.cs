using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RItemListSelectMode {
		Single,
		Multi
	}
	public enum RItemListIconMode
	{
		Left,
		Top,
	}
	[Category("UI/Container/Visuals/Templets")]
	public class ItemList : UITemplet
	{
		public readonly Sync<RItemListSelectMode> SelectMode;
		public readonly Sync<bool> AllowReselect;
		public readonly Sync<bool> AllowRMBSelect;
		[Default(1)]public readonly Sync<int> MaxTextLines;
		public readonly Sync<bool> AutoHeight;
		public readonly Sync<ROverrunBehavior> TextOverrun;
		public readonly SyncObjList<ItemListItem> Items;
		[Default(1)]public readonly Sync<int> MaxColumns;
		public readonly Sync<bool> SameWidthColumns;
		public readonly Sync<int> FixedWidthColumns;
		public readonly Sync<RItemListIconMode> IconMode;
		[Default(1f)]public readonly Sync<float> IconScale;
		public readonly Sync<Vector2i> FixedIconSize;

		public class ItemListItem : SyncObject
		{
			public readonly Sync<string> Text;
			public readonly AssetRef<RTexture2D> Icon;
			[Default(true)]
			public readonly Sync<bool> Selectable;
			public readonly Sync<bool> Disabled;
		}
	}
}
