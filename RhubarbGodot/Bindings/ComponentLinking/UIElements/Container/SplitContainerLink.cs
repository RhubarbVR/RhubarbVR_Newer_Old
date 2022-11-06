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
	public sealed class SplitContainerLink : ContainerBase<RhuEngine.Components.SplitContainer, Godot.SplitContainer>
	{
		public override string ObjectName => "SplitContainer";

		public override void StartContinueInit() {

			LinkedComp.SplitOffset.Changed += SplitOffset_Changed;
			LinkedComp.Collapsed.Changed += Collapsed_Changed;
			LinkedComp.DraggerVisibillity.Changed += DraggerVisibillity_Changed;
			LinkedComp.Vertical.Changed += Vertical_Changed;
			SplitOffset_Changed(null);
			Collapsed_Changed(null);
			DraggerVisibillity_Changed(null);
			Vertical_Changed(null);
		}

		private void Vertical_Changed(IChangeable obj) {
			node.Vertical = LinkedComp.Vertical.Value;
		}

		private void DraggerVisibillity_Changed(IChangeable obj) {
			node.DraggerVisibility = LinkedComp.DraggerVisibillity.Value switch {
				RDraggerVisibillity.Visible => Godot.SplitContainer.DraggerVisibilityEnum.Visible,
				RDraggerVisibillity.Hidden_Callapsed => Godot.SplitContainer.DraggerVisibilityEnum.HiddenCollapsed,
				_ => Godot.SplitContainer.DraggerVisibilityEnum.Hidden,
			};
		}

		private void Collapsed_Changed(IChangeable obj) {
			node.Collapsed = LinkedComp.Collapsed.Value;
		}

		private void SplitOffset_Changed(IChangeable obj) {
			node.SplitOffset = LinkedComp.SplitOffset.Value;
		}
	}
}
