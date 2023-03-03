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
using System.Diagnostics;
using System.Drawing.Drawing2D;
using RhubarbVR.Bindings.TextureBindings;
using RhuEngine;

namespace RhubarbVR.Bindings.ComponentLinking
{


	public sealed class TexturedProgressBarLink : RangeBase<RhuEngine.Components.TexturedProgressBar, GDExtension.TextureProgressBar>
	{
		public override string ObjectName => "TexturedProgressBar";

		public override void StartContinueInit() {
			LinkedComp.FillMode.Changed += FillMode_Changed;
			LinkedComp.NinePatchStrech.Changed += NinePatchStrech_Changed;
			LinkedComp.MaxStrech.Changed += MaxStrech_Changed;
			LinkedComp.MinStrech.Changed += MinStrech_Changed;
			LinkedComp.Under.LoadChange += Under_LoadChange;
			LinkedComp.Under_Tint.Changed += Under_Tint_Changed;
			LinkedComp.Over.LoadChange += Over_LoadChange;
			LinkedComp.Over_Tint.Changed += Over_Tint_Changed;
			LinkedComp.Progress.LoadChange += Progress_LoadChange;
			LinkedComp.Progress_Tint.Changed += Progress_Tint_Changed;
			LinkedComp.ProgressOffset.Changed += ProgressOffset_Changed;
			LinkedComp.InitialAngle.Changed += InitialAngle_Changed;
			LinkedComp.FillDegrees.Changed += FillDegrees_Changed;
			LinkedComp.CenterOffset.Changed += CenterOffset_Changed;
			FillMode_Changed(null);
			NinePatchStrech_Changed(null);
			MaxStrech_Changed(null);
			MinStrech_Changed(null);
			Under_LoadChange(null);
			Under_Tint_Changed(null);
			Over_LoadChange(null);
			Over_Tint_Changed(null);
			Progress_LoadChange(null);
			Progress_Tint_Changed(null);
			ProgressOffset_Changed(null);
			InitialAngle_Changed(null);
			FillDegrees_Changed(null);
			CenterOffset_Changed(null);
		}

		private void Under_LoadChange(RTexture2D obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextureUnder = LinkedComp.Under.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null);
		}

		private void Over_LoadChange(RTexture2D obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextureOver = LinkedComp.Over.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null);
		}

		private void Progress_LoadChange(RTexture2D obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextureProgress = LinkedComp.Progress.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null);
		}

		private void CenterOffset_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.RadialCenterOffset = new Vector2(LinkedComp.CenterOffset.Value.x, LinkedComp.CenterOffset.Value.y));
		}

		private void FillDegrees_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.RadialFillDegrees = LinkedComp.FillDegrees.Value);
		}

		private void InitialAngle_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.RadialInitialAngle = LinkedComp.InitialAngle.Value);
		}

		private void ProgressOffset_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextureProgressOffset = new Vector2(LinkedComp.ProgressOffset.Value.x, LinkedComp.ProgressOffset.Value.y));
		}

		private void Progress_Tint_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TintProgress = new Color(LinkedComp.Progress_Tint.Value.r, LinkedComp.Progress_Tint.Value.g, LinkedComp.Progress_Tint.Value.b, LinkedComp.Progress_Tint.Value.a));
		}

		private void Over_Tint_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TintOver = new Color(LinkedComp.Over_Tint.Value.r, LinkedComp.Over_Tint.Value.g, LinkedComp.Over_Tint.Value.b, LinkedComp.Over_Tint.Value.a));
		}


		private void Under_Tint_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TintUnder = new Color(LinkedComp.Under_Tint.Value.r, LinkedComp.Under_Tint.Value.g, LinkedComp.Under_Tint.Value.b, LinkedComp.Under_Tint.Value.a));
		}

		private void MinStrech_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				node.StretchMarginLeft = LinkedComp.MinStrech.Value.x;
				node.StretchMarginBottom = LinkedComp.MinStrech.Value.y;
			});
		}

		private void MaxStrech_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				node.StretchMarginRight = LinkedComp.MaxStrech.Value.x;
				node.StretchMarginTop = LinkedComp.MaxStrech.Value.y;
			});
		}

		private void NinePatchStrech_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.NinePatchStretch = LinkedComp.NinePatchStrech.Value);
		}

		private void FillMode_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FillModeValue = LinkedComp.FillMode.Value switch { RTexturedProgressBarFillMode.Left_To_Right => (int)TextureProgressBar.FillMode.LeftToRight, RTexturedProgressBarFillMode.Right_To_Left => (int)TextureProgressBar.FillMode.RightToLeft, RTexturedProgressBarFillMode.Top_To_Bottom => (int)TextureProgressBar.FillMode.TopToBottom, RTexturedProgressBarFillMode.Bottom_To_Top => (int)TextureProgressBar.FillMode.BottomToTop, RTexturedProgressBarFillMode.ClockWise => (int)TextureProgressBar.FillMode.Clockwise, RTexturedProgressBarFillMode.Counter_ClockWise => (int)TextureProgressBar.FillMode.CounterClockwise, RTexturedProgressBarFillMode.Bilinear_Left_And_Right => (int)TextureProgressBar.FillMode.LeftToRight, RTexturedProgressBarFillMode.Bilinear_Top_And_Bottom => (int)TextureProgressBar.FillMode.BilinearTopAndBottom, _ => (int)TextureProgressBar.FillMode.ClockwiseAndCounterClockwise, });
		}
	}
}
