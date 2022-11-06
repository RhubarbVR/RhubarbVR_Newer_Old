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

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class TabContainerLink : ContainerBase<RhuEngine.Components.TabContainer, Godot.TabContainer>
	{
		public override string ObjectName => "TabContainer";

		public override void StartContinueInit() {

			LinkedComp.TabAlignment.Changed += TabAlignment_Changed;
			LinkedComp.CurrentTab.Changed += CurrentTab_Changed;
			LinkedComp.ClipTabs.Changed += ClipTabs_Changed;
			LinkedComp.TabsVisible.Changed += TabsVisible_Changed;
			LinkedComp.RangeGroup.Changed += RangeGroup_Changed;
			LinkedComp.UseHiddenTabsForMinSize.Changed += UseHiddenTabsForMinSize_Changed;
			TabAlignment_Changed(null);
			CurrentTab_Changed(null);
			ClipTabs_Changed(null);
			TabsVisible_Changed(null);
			RangeGroup_Changed(null);
			UseHiddenTabsForMinSize_Changed(null);

		}

		private void UseHiddenTabsForMinSize_Changed(IChangeable obj) {
			node.UseHiddenTabsForMinSize = LinkedComp.UseHiddenTabsForMinSize.Value;
		}

		private void RangeGroup_Changed(IChangeable obj) {
			node.TabsRearrangeGroup = LinkedComp.RangeGroup.Value;
		}

		private void TabsVisible_Changed(IChangeable obj) {
			node.TabsVisible = LinkedComp.TabsVisible.Value;
		}

		private void ClipTabs_Changed(IChangeable obj) {
			node.ClipTabs = LinkedComp.TabsVisible.Value;
		}

		private void CurrentTab_Changed(IChangeable obj) {
			node.CurrentTab = LinkedComp.CurrentTab.Value;
		}

		private void TabAlignment_Changed(IChangeable obj) {
			node.TabAlignment = LinkedComp.TabAlignment.Value switch {
				RTabAlignment.Left => TabBar.AlignmentMode.Left,
				RTabAlignment.Right => TabBar.AlignmentMode.Right,
				_ => TabBar.AlignmentMode.Center,
			};
		}
	}
}
