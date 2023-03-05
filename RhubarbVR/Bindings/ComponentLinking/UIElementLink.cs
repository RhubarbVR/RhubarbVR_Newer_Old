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
using RhubarbVR.Bindings.FontBindings;
using RhuEngine;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public abstract class UIElementLinkBase<T, T2> : CanvasItemNodeLinked<T, T2>, IUIElementLinked where T : UIElement, new() where T2 : Control, new()
	{
		protected virtual bool FreeKeyboard => false;

		event Action _focusEntered;
		event Action _focusExited;
		event Action _resized;
		event Action _sizeFlagsChanged;
		event Action _minimumSizeChanged;
		event Action _inputEntered;
		event Action _inputExited;

		public event Action FocusEntered
		{
			add => _focusEntered += value;
			remove => _focusEntered -= value;
		}
		public event Action FocusExited
		{
			add => _focusExited += value;
			remove => _focusExited -= value;
		}
		public event Action Resized
		{
			add => _resized += value;
			remove => _resized -= value;
		}
		public event Action SizeFlagsChanged
		{
			add => _sizeFlagsChanged += value;
			remove => _sizeFlagsChanged -= value;
		}
		public event Action MinimumSizeChanged
		{
			add => _minimumSizeChanged += value;
			remove => _minimumSizeChanged -= value;
		}
		public event Action InputEntered
		{
			add => _inputEntered += value;
			remove => _inputEntered -= value;
		}
		public event Action InputExited
		{
			add => _inputExited += value;
			remove => _inputExited -= value;
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
			node.Resized += Node_Resized;
			node.SizeFlagsChanged += Node_SizeFlagsChanged;
			node.MinimumSizeChanged += Node_MinimumSizeChanged;
			node.MouseEntered += Node_MouseEntered;
			node.MouseExited += Node_MouseExited;
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

		private void Node_MouseExited() {
			_inputExited?.Invoke();
		}

		private void Node_MouseEntered() {
			_inputEntered?.Invoke();
		}

		private void Node_MinimumSizeChanged() {
			_minimumSizeChanged?.Invoke();
		}

		private void Node_SizeFlagsChanged() {
			_sizeFlagsChanged?.Invoke();
		}

		private void Node_Resized() {
			_resized?.Invoke();
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
			_focusExited?.Invoke();
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
			_focusEntered?.Invoke();
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
			RenderThread.ExecuteOnEndOfFrame(() => node.FocusModeValue = LinkedComp.FocusMode.Value switch {
				RFocusMode.Click => FocusMode.Click,
				RFocusMode.All => FocusMode.All,
				_ => FocusMode.None,
			});
		}

		private void CursorShape_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MouseDefaultCursorShape = (CursorShape)LinkedComp.CursorShape.Value);
		}

		private void ForceScrollEventPassing_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MouseForcePassScrollEvents = LinkedComp.ForceScrollEventPassing.Value);
		}

		private void InputFilter_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MouseFilterValue = LinkedComp.InputFilter.Value switch { RInputFilter.Pass => MouseFilter.Pass, RInputFilter.Ignore => MouseFilter.Ignore, _ => MouseFilter.Stop, });
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
				node.SetAnchor(Side.Bottom, LinkedComp.Max.Value.y);
				node.SetAnchor(Side.Right, LinkedComp.Max.Value.x);
			});
		}

		private void Min_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				node.SetAnchor(Side.Top, LinkedComp.Min.Value.y);
				node.SetAnchor(Side.Left, LinkedComp.Min.Value.x);
			});
		}

		private void LayoutDir_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LayoutDirectionValue = LinkedComp.LayoutDir.Value switch {
				RLayoutDir.Locale => LayoutDirection.Locale,
				RLayoutDir.Left_to_Right => LayoutDirection.Ltr,
				RLayoutDir.Right_to_Left => LayoutDirection.Rtl,
				_ => LayoutDirection.Inherited,
			});
		}

		private void MinSize_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.CustomMinimumSize = new Vector2(LinkedComp.MinSize.Value.x, LinkedComp.MinSize.Value.y));
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
