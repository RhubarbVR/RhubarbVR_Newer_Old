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
using RhubarbVR.Bindings.FontBindings;
using RhuEngine;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public abstract class UIElementLinkBase<T, T2> : CanvasItemNodeLinked<T, T2>, IUIElementLinked where T : UIElement, new() where T2 : Control, new()
	{
		protected virtual bool FreeKeyboard => false;

		public event Action FocusEntered
		{
			add {
				if (node is null) {
					return;
				}
				node.FocusEntered += value;
			}
			remove {
				if (node is null) {
					return;
				}
				node.FocusEntered -= value;
			}
		}
		public event Action FocusExited
		{
			add {
				if (node is null) {
					return;
				}
				node.FocusExited += value;
			}
			remove {
				if (node is null) {
					return;
				}
				node.FocusExited -= value;
			}
		}
		public event Action Resized
		{
			add {
				if (node is null) {
					return;
				}
				node.Resized += value;
			}
			remove {
				if (node is null) {
					return;
				}
				node.Resized -= value;
			}
		}
		public event Action SizeFlagsChanged
		{
			add {
				if (node is null) {
					return;
				}
				node.SizeFlagsChanged += value;
			}
			remove {
				if (node is null) {
					return;
				}
				node.SizeFlagsChanged -= value;
			}
		}
		public event Action MinimumSizeChanged
		{
			add {
				if (node is null) {
					return;
				}
				node.MinimumSizeChanged += value;
			}
			remove {
				if (node is null) {
					return;
				}
				node.MinimumSizeChanged -= value;
			}
		}
		public event Action InputEntered
		{
			add {
				if (node is null) {
					return;
				}
				node.MouseEntered += value;
			}
			remove {
				if (node is null) {
					return;
				}
				node.MouseEntered -= value;
			}
		}
		public event Action InputExited
		{
			add {
				if (node is null) {
					return;
				}
				node.MouseExited += value;
			}
			remove {
				if (node is null) {
					return;
				}
				node.MouseExited -= value;
			}
		}

		public override void Init() {
			base.Init();
			node.FocusEntered += Node_FocusEntered;
			node.FocusExited += Node_FocusExited;
			foreach (var item in node.GetChildren(true)) {
				if (item is Control control) {
					control.FocusEntered += Node_FocusEntered;
					control.FocusExited += Node_FocusExited;
				}
			}
			LinkedComp.ClipContents.Changed += ClipContents_Changed;
			LinkedComp.MinSize.Changed += MinSize_Changed;
			LinkedComp.LayoutDir.Changed += LayoutDir_Changed;
			LinkedComp.Min.Changed += Min_Changed;
			LinkedComp.Max.Changed += Max_Changed;
			LinkedComp.MinOffset.Changed += MinOffset_Changed;
			LinkedComp.MaxOffset.Changed += MaxOffset_Changed;
			LinkedComp.GrowHorizontal.Changed += GrowHorizontal_Changed;
			LinkedComp.GrowVertical.Changed += GrowVertical_Changed;
			LinkedComp.Rotation.Changed += Rotation_Changed;
			LinkedComp.Scale.Changed += Scale_Changed;
			LinkedComp.PivotOffset.Changed += PivotOffset_Changed;
			LinkedComp.HorizontalFilling.Changed += HorizontalFilling_Changed;
			LinkedComp.VerticalFilling.Changed += VerticalFilling_Changed;
			LinkedComp.StretchRatio.Changed += StretchRatio_Changed;
			LinkedComp.AutoTranslate.Changed += AutoTranslate_Changed;
			LinkedComp.InputFilter.Changed += InputFilter_Changed;
			LinkedComp.ForceScrollEventPassing.Changed += ForceScrollEventPassing_Changed;
			LinkedComp.CursorShape.Changed += CursorShape_Changed;
			LinkedComp.FocusMode.Changed += FocusMode_Changed;
			LinkedComp.ToolTipText.Changed += ToolTipText_Changed;
			ToolTipText_Changed(null);
			ClipContents_Changed(null);
			MinSize_Changed(null);
			LayoutDir_Changed(null);
			Min_Changed(null);
			Max_Changed(null);
			MinOffset_Changed(null);
			MaxOffset_Changed(null);
			GrowHorizontal_Changed(null);
			GrowVertical_Changed(null);
			Rotation_Changed(null);
			Scale_Changed(null);
			PivotOffset_Changed(null);
			HorizontalFilling_Changed(null);
			VerticalFilling_Changed(null);
			StretchRatio_Changed(null);
			AutoTranslate_Changed(null);
			InputFilter_Changed(null);
			ForceScrollEventPassing_Changed(null);
			CursorShape_Changed(null);
			FocusMode_Changed(null);
			LinkedComp.KeyboardUnBindAction = KeyboardUnBindAction;
			LinkedComp.KeyboardBindAction = KeyboardBindAction;
			if (LinkedComp.Engine.staticResources.MainFont.Inst is GodotFont font) {
				node.Theme ??= new Theme();
				node.Theme.DefaultFont = font.FontFile;
			}
		}

		private void ToolTipText_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TooltipText = LinkedComp.ToolTipText.Value);
		}

		protected void Node_FocusExited() {
			if (LinkedComp is null) {
				return;
			}
			if (node is null) {
				return;
			}
			if (LinkedComp.IsRemoved | LinkedComp.IsDestroying) {
				return;
			}
			if (LinkedComp.Entity.Viewport?.TakeKeyboardFocus?.Value ?? true) {
				LinkedComp.Engine.KeyboardInteractionUnBind(LinkedComp);
			}
		}

		protected void Node_FocusEntered() {
			if(LinkedComp is null) {
				return;
			}
			if(node is null) {
				return;
			}
			if(LinkedComp.IsRemoved | LinkedComp.IsDestroying) {
				return;
			}
			if (FreeKeyboard & (LinkedComp.Entity.Viewport?.TakeKeyboardFocus?.Value ?? true)) {
				LinkedComp.Engine.KeyboardInteractionBind(LinkedComp);
			}
		}

		private void KeyboardBindAction() {
			node.GrabFocus();
		}

		private void KeyboardUnBindAction() {
			node.ReleaseFocus();
			foreach (var item in node.GetChildren(true)) {
				if (item is Control control) {
					control.ReleaseFocus();
				}
			}
		}

		private void FocusMode_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FocusMode = LinkedComp.FocusMode.Value switch {
				RFocusMode.Click => FocusModeEnum.Click,
				RFocusMode.All => FocusModeEnum.All,
				_ => FocusModeEnum.None,
			});
		}

		private void CursorShape_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MouseDefaultCursorShape = (CursorShape)LinkedComp.CursorShape.Value);
		}

		private void ForceScrollEventPassing_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MouseForcePassScrollEvents = LinkedComp.ForceScrollEventPassing.Value);
		}

		private void InputFilter_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MouseFilter = LinkedComp.InputFilter.Value switch { RInputFilter.Pass => MouseFilterEnum.Pass, RInputFilter.Ignore => MouseFilterEnum.Ignore, _ => MouseFilterEnum.Stop, });
		}

		private void AutoTranslate_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AutoTranslate = LinkedComp.AutoTranslate.Value);
		}

		private void StretchRatio_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.SizeFlagsStretchRatio = LinkedComp.StretchRatio.Value);
		}

		private void VerticalFilling_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.SizeFlagsVertical = (SizeFlags)LinkedComp.VerticalFilling.Value);
		}

		private void HorizontalFilling_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.SizeFlagsHorizontal = (SizeFlags)LinkedComp.HorizontalFilling.Value);
		}

		private void PivotOffset_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.PivotOffset = new Vector2(LinkedComp.PivotOffset.Value.x, LinkedComp.PivotOffset.Value.y));
		}

		private void Scale_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Scale = new Vector2(LinkedComp.Scale.Value.x, LinkedComp.Scale.Value.y));
		}

		private void Rotation_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Rotation = LinkedComp.Rotation.Value * RNumerics.MathUtil.DEG_2_RADF);
		}

		private void GrowVertical_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.GrowVertical = LinkedComp.GrowVertical.Value switch { RGrowVertical.Top => GrowDirection.Begin, RGrowVertical.Bottom => GrowDirection.End, _ => GrowDirection.Both, });
		}

		private void GrowHorizontal_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.GrowHorizontal = LinkedComp.GrowHorizontal.Value switch { RGrowHorizontal.Left => GrowDirection.Begin, RGrowHorizontal.Right => GrowDirection.End, _ => GrowDirection.Both, });
		}

		private void MaxOffset_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				node.OffsetBottom = LinkedComp.MaxOffset.Value.y;
				node.OffsetLeft = LinkedComp.MaxOffset.Value.x;
			});
		}

		private void MinOffset_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				node.OffsetTop = LinkedComp.MinOffset.Value.y;
				node.OffsetRight = LinkedComp.MinOffset.Value.x;
			});
		}

		private void Max_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				node.AnchorBottom = LinkedComp.Max.Value.y;
				node.AnchorRight = LinkedComp.Max.Value.x;
			});
		}

		private void Min_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				node.AnchorTop = LinkedComp.Min.Value.y;
				node.AnchorLeft = LinkedComp.Min.Value.x;
			});
		}

		private void LayoutDir_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LayoutDirection = LinkedComp.LayoutDir.Value switch {
				RLayoutDir.Locale => LayoutDirectionEnum.Locale,
				RLayoutDir.Left_to_Right => LayoutDirectionEnum.Ltr,
				RLayoutDir.Right_to_Left => LayoutDirectionEnum.Rtl,
				_ => LayoutDirectionEnum.Inherited,
			});
		}

		private void MinSize_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.CustomMinimumSize = new Vector2I(LinkedComp.MinSize.Value.x, LinkedComp.MinSize.Value.y));
		}

		private void ClipContents_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ClipContents = LinkedComp.ClipContents.Value);
		}

		public override void Render() {
		}

	}

	public sealed class UIElementLink : UIElementLinkBase<UIElement, Control>
	{
		public override string ObjectName => "UIElement";

		public override void StartContinueInit() {

		}
	}

}
