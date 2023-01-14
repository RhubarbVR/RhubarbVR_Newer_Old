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
using RhuEngine;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class ScrollContainerLink : ContainerBase<RhuEngine.Components.ScrollContainer, Godot.ScrollContainer>
	{
		public override string ObjectName => "ScrollContainer";

		public override void StartContinueInit() {
			LinkedComp.FollowFocus.Changed += FollowFocus_Changed;
			LinkedComp.HorizontalScrollBar.Changed += HorizontalScrollBar_Changed;
			LinkedComp.VerticalScrollBar.Changed += VerticalScrollBar_Changed;
			LinkedComp.ScrollDeadZone.Changed += ScrollDeadZone_Changed;
			LinkedComp.HorizontalScroll.Changed += HorizontalScroll_Changed;
			LinkedComp.VerticalScroll.Changed += VerticalScroll_Changed;
			FollowFocus_Changed(null);
			HorizontalScrollBar_Changed(null);
			VerticalScrollBar_Changed(null);
			ScrollDeadZone_Changed(null);
			HorizontalScroll_Changed(null);
			VerticalScroll_Changed(null);

		}

		private void VerticalScroll_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ScrollVertical = LinkedComp.VerticalScroll.Value);
		}

		private void HorizontalScroll_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ScrollHorizontal = LinkedComp.HorizontalScroll.Value);
		}

		private void ScrollDeadZone_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ScrollDeadzone = LinkedComp.ScrollDeadZone.Value);
		}

		private void VerticalScrollBar_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.VerticalScrollMode = LinkedComp.VerticalScrollBar.Value switch { RScrollBarVisibility.Auto => Godot.ScrollContainer.ScrollMode.Auto, RScrollBarVisibility.AlwaysShow => Godot.ScrollContainer.ScrollMode.ShowAlways, RScrollBarVisibility.NeverShow => Godot.ScrollContainer.ScrollMode.ShowNever, _ => Godot.ScrollContainer.ScrollMode.Disabled, });
		}

		private void HorizontalScrollBar_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.HorizontalScrollMode = LinkedComp.HorizontalScrollBar.Value switch { RScrollBarVisibility.Auto => Godot.ScrollContainer.ScrollMode.Auto, RScrollBarVisibility.AlwaysShow => Godot.ScrollContainer.ScrollMode.ShowAlways, RScrollBarVisibility.NeverShow => Godot.ScrollContainer.ScrollMode.ShowNever, _ => Godot.ScrollContainer.ScrollMode.Disabled, });
		}

		private void FollowFocus_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FollowFocus = LinkedComp.FollowFocus.Value);
		}
	}
}
