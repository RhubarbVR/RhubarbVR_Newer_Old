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
	public sealed class AspicRatioContainerLink : ContainerBase<RhuEngine.Components.AspectRatioContainer, Godot.AspectRatioContainer>
	{
		public override string ObjectName => "AspicRatioContainer";

		public override void StartContinueInit() {

			LinkedComp.Ratio.Changed += Ratio_Changed;
			LinkedComp.StretchMode.Changed += StretchMode_Changed;
			LinkedComp.HorizontalAlignment.Changed += HorizontalAlignment_Changed;
			LinkedComp.VerticalAlignment.Changed += VerticalAlignment_Changed;
			Ratio_Changed(null);
			StretchMode_Changed(null);
			HorizontalAlignment_Changed(null);
			VerticalAlignment_Changed(null);
		}

		private void VerticalAlignment_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AlignmentVertical = LinkedComp.VerticalAlignment.Value switch {
				RVerticalAlignment.Top => Godot.AspectRatioContainer.AlignmentMode.Begin,
				RVerticalAlignment.Center => Godot.AspectRatioContainer.AlignmentMode.Center,
				RVerticalAlignment.Bottom => Godot.AspectRatioContainer.AlignmentMode.End,
				_ => Godot.AspectRatioContainer.AlignmentMode.Center,
			});
		}

		private void HorizontalAlignment_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() =>
node.AlignmentHorizontal = LinkedComp.HorizontalAlignment.Value switch {
				RHorizontalAlignment.Left => Godot.AspectRatioContainer.AlignmentMode.Begin,
				RHorizontalAlignment.Right => Godot.AspectRatioContainer.AlignmentMode.End,
				_ => Godot.AspectRatioContainer.AlignmentMode.Center,
			});
		}

		private void StretchMode_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.StretchMode = LinkedComp.StretchMode.Value switch { RAspectRatioStretchMode.Cover => Godot.AspectRatioContainer.StretchModeEnum.Cover, RAspectRatioStretchMode.Width_Controls_Height => Godot.AspectRatioContainer.StretchModeEnum.WidthControlsHeight, RAspectRatioStretchMode.Height_Controls_Width => Godot.AspectRatioContainer.StretchModeEnum.HeightControlsWidth, _ => Godot.AspectRatioContainer.StretchModeEnum.Fit, });
		}

		private void Ratio_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Ratio = LinkedComp.Ratio.Value);
		}
	}
}
