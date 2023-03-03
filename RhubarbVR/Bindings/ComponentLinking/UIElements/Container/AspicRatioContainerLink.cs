using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;
using GDExtension;
using RhuEngine.Components;
using static GDExtension.Control;
using RhuEngine;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class AspicRatioContainerLink : ContainerBase<RhuEngine.Components.AspectRatioContainer, GDExtension.AspectRatioContainer>
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
				RVerticalAlignment.Top => GDExtension.AspectRatioContainer.AlignmentMode.Begin,
				RVerticalAlignment.Center => GDExtension.AspectRatioContainer.AlignmentMode.Center,
				RVerticalAlignment.Bottom => GDExtension.AspectRatioContainer.AlignmentMode.End,
				_ => GDExtension.AspectRatioContainer.AlignmentMode.Center,
			});
		}

		private void HorizontalAlignment_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() =>
node.AlignmentHorizontal = LinkedComp.HorizontalAlignment.Value switch {
				RHorizontalAlignment.Left => GDExtension.AspectRatioContainer.AlignmentMode.Begin,
				RHorizontalAlignment.Right => GDExtension.AspectRatioContainer.AlignmentMode.End,
				_ => GDExtension.AspectRatioContainer.AlignmentMode.Center,
			});
		}

		private void StretchMode_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.StretchModeValue = LinkedComp.StretchMode.Value switch { RAspectRatioStretchMode.Cover => GDExtension.AspectRatioContainer.StretchMode.Cover, RAspectRatioStretchMode.Width_Controls_Height => GDExtension.AspectRatioContainer.StretchMode.WidthControlsHeight, RAspectRatioStretchMode.Height_Controls_Width => GDExtension.AspectRatioContainer.StretchMode.HeightControlsWidth, _ => GDExtension.AspectRatioContainer.StretchMode.Fit, });
		}

		private void Ratio_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Ratio = LinkedComp.Ratio.Value);
		}
	}
}
