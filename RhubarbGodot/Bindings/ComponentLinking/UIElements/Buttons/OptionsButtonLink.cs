using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;
using Godot;
using RhuEngine.Components;
using static Godot.Control;
using RhubarbVR.Bindings.TextureBindings;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class OptionsButtonLink : Button<RhuEngine.Components.OptionButton, Godot.OptionButton>
	{
		public override string ObjectName => "OptionsButton";

		public override void StartContinueInit() {
			LinkedComp.Selected.Changed += Selected_Changed;
			LinkedComp.FitLongestItem.Changed += FitLongestItem_Changed;
			LinkedComp.Items.Changed += Items_Changed;
			Selected_Changed(null);
			FitLongestItem_Changed(null);
			Items_Changed(null);
			node.ItemSelected += Node_ItemSelected;
			_isDerty = true;
		}

		private void Node_ItemSelected(long index) {
			LinkedComp.Selected.Value = (int)index;
		}

		public sealed class LinkedButton
		{
			public RhuEngine.Components.OptionButton.MenuButtonItem target;
			public OptionsButtonLink parrent;

			public LinkedButton(RhuEngine.Components.OptionButton.MenuButtonItem item, OptionsButtonLink optionsButtonLink) {
				Link(item, optionsButtonLink);
			}

			public void Unlink() {
				target.Text.Changed -= parrent.MarkRebuild;
				target.Id.Changed -= parrent.MarkRebuild;
				target.Icon.LoadChange -= RebuildUI;
				target.Disabled.Changed -= parrent.MarkRebuild;
				target.Separator.Changed -= parrent.MarkRebuild;
				parrent.MarkRebuild(null);
			}
			public void Link(RhuEngine.Components.OptionButton.MenuButtonItem item, OptionsButtonLink optionsButtonLink) {
				parrent = optionsButtonLink;
				target = item;
				Link();
			}

			private void Link() {
				target.Text.Changed += parrent.MarkRebuild;
				target.Id.Changed += parrent.MarkRebuild;
				target.Icon.LoadChange += RebuildUI;
				target.Disabled.Changed += parrent.MarkRebuild;
				target.Separator.Changed += parrent.MarkRebuild;
				parrent.MarkRebuild(null);
			}

			private void RebuildUI(RTexture2D rTexture2D) {
				parrent.MarkRebuild(null);
			}

			internal void CheckLink(RhuEngine.Components.OptionButton.MenuButtonItem menuButtonItem) {
				if (target != menuButtonItem) {
					Unlink();
					target = menuButtonItem;
					Link();
				}
			}
		}

		public readonly List<LinkedButton> menuButtonItems = new();

		private void Items_Changed(IChangeable obj) {
			lock (menuButtonItems) {
				for (var i = menuButtonItems.Count - 1; i >= 0; i++) {
					if (i >= LinkedComp.Items.Count) {
						menuButtonItems[i].Unlink();
						menuButtonItems.RemoveAt(i);
					}
					else {
						menuButtonItems[i].CheckLink(LinkedComp.Items[i]);
					}
				}
				var start = menuButtonItems.Count;
				for (var i = 0; i < LinkedComp.Items.Count - start; i++) {
					var indes = start + i;
					menuButtonItems.Add(new LinkedButton(LinkedComp.Items[indes], this));
				}
			}
		}

		private bool _isDerty = false;

		public override void Render() {
			base.Render();
			if (_isDerty) {
				lock (menuButtonItems) {
					node.Clear();
					for (var i = 0; i < menuButtonItems.Count; i++) {
						var current = menuButtonItems[i].target;
						if (current.Separator.Value) {
							node.AddSeparator(current.Text.Value);
						}
						else {
							node.AddItem(current.Text.Value, current.Id.Value);
						}
						node.SetItemTooltip(i, current.ToolTip.Value);
						node.SetItemIcon(i, current.Icon.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null);
						node.SetItemDisabled(i, current.Disabled.Value);
					}
					Selected_Changed(null);
					_isDerty = false;
				}
			}
		}

		public void MarkRebuild(IChangeable _) {
			_isDerty = true;
		}

		private void FitLongestItem_Changed(IChangeable obj) {
			node.FitToLongestItem = LinkedComp.FitLongestItem.Value;
		}

		private void Selected_Changed(IChangeable obj) {
			node.Selected = LinkedComp.Selected.Value;
		}
	}
}
