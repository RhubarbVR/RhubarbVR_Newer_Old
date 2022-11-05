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

namespace RhubarbVR.Bindings.ComponentLinking
{
	public abstract class UIElementLinkBase<T, T2> : CanvasItemNodeLinked<T, T2>, IUIElementLinked where T : UIElement, new() where T2 : Control, new()
	{
		protected virtual bool FreeKeyboard => false;

		public event Action FocusEntered
		{
			add {
				if (LinkedComp is IUIElementLinked val) {
					val.FocusEntered += value;
				}
			}
			remove {
				if (LinkedComp is IUIElementLinked val) {
					val.FocusEntered -= value;
				}
			}
		}
		public event Action FocusExited
		{
			add {
				if (LinkedComp is IUIElementLinked val) {
					val.FocusExited += value;
				}
			}
			remove {
				if (LinkedComp is IUIElementLinked val) {
					val.FocusExited -= value;
				}
			}
		}
		public event Action Resized
		{
			add {
				if (LinkedComp is IUIElementLinked val) {
					val.Resized += value;
				}
			}
			remove {
				if (LinkedComp is IUIElementLinked val) {
					val.Resized -= value;
				}
			}
		}
		public event Action SizeFlagsChanged
		{
			add {
				if (LinkedComp is IUIElementLinked val) {
					val.SizeFlagsChanged += value;
				}
			}
			remove {
				if (LinkedComp is IUIElementLinked val) {
					val.SizeFlagsChanged -= value;
				}
			}
		}
		public event Action MinimumSizeChanged
		{
			add {
				if (LinkedComp is IUIElementLinked val) {
					val.MinimumSizeChanged += value;
				}
			}
			remove {
				if (LinkedComp is IUIElementLinked val) {
					val.MinimumSizeChanged -= value;
				}
			}
		}
		public event Action InputEntered
		{
			add {
				if (LinkedComp is IUIElementLinked val) {
					val.InputEntered += value;
				}
			}
			remove {
				if (LinkedComp is IUIElementLinked val) {
					val.InputEntered -= value;
				}
			}
		}
		public event Action InputExited
		{
			add {
				if (LinkedComp is IUIElementLinked val) {
					val.InputExited += value;
				}
			}
			remove {
				if (LinkedComp is IUIElementLinked val) {
					val.InputExited -= value;
				}
			}
		}

		public override void Init() {
			base.Init();
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
			node.FocusEntered += Node_FocusEntered;
			node.FocusExited += Node_FocusExited;
			foreach (var item in node.GetChildren(true)) {
				if (item is Control control) {
					control.FocusEntered += Node_FocusEntered;
					control.FocusExited += Node_FocusExited;
				}
			}
			if (LinkedComp.Engine.staticResources.MainFont.Inst is GodotFont font) {
				node.Theme ??= new Theme();
				node.Theme.DefaultFont = font.FontFile;
			}
		}
		protected void Node_FocusExited() {
			LinkedComp.Engine.KeyboardInteractionUnBind(LinkedComp);
		}

		protected void Node_FocusEntered() {
			if (FreeKeyboard) {
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
			node.FocusMode = LinkedComp.FocusMode.Value switch {
				RFocusMode.Click => FocusModeEnum.Click,
				RFocusMode.All => FocusModeEnum.All,
				_ => FocusModeEnum.None,
			};
		}

		private void CursorShape_Changed(IChangeable obj) {
			node.MouseDefaultCursorShape = (CursorShape)LinkedComp.CursorShape.Value;
		}

		private void ForceScrollEventPassing_Changed(IChangeable obj) {
			node.MouseForcePassScrollEvents = LinkedComp.ForceScrollEventPassing.Value;
		}

		private void InputFilter_Changed(IChangeable obj) {
			node.MouseFilter = LinkedComp.InputFilter.Value switch {
				RInputFilter.Pass => MouseFilterEnum.Pass,
				RInputFilter.Ignore => MouseFilterEnum.Ignore,
				_ => MouseFilterEnum.Stop,
			};
		}

		private void AutoTranslate_Changed(IChangeable obj) {
			node.AutoTranslate = LinkedComp.AutoTranslate.Value;
		}

		private void StretchRatio_Changed(IChangeable obj) {
			node.SizeFlagsStretchRatio = LinkedComp.StretchRatio.Value;
		}

		private void VerticalFilling_Changed(IChangeable obj) {
			node.SizeFlagsVertical = (int)LinkedComp.VerticalFilling.Value;
		}

		private void HorizontalFilling_Changed(IChangeable obj) {
			node.SizeFlagsHorizontal = (int)LinkedComp.HorizontalFilling.Value;
		}

		private void PivotOffset_Changed(IChangeable obj) {
			node.PivotOffset = new Vector2(LinkedComp.PivotOffset.Value.x, LinkedComp.PivotOffset.Value.y);
		}

		private void Scale_Changed(IChangeable obj) {
			node.Scale = new Vector2(LinkedComp.Scale.Value.x, LinkedComp.Scale.Value.y);
		}

		private void Rotation_Changed(IChangeable obj) {
			node.Rotation = LinkedComp.Rotation.Value * RNumerics.MathUtil.DEG_2_RADF;
		}

		private void GrowVertical_Changed(IChangeable obj) {
			node.GrowVertical = LinkedComp.GrowVertical.Value switch {
				RGrowVertical.Top => GrowDirection.Begin,
				RGrowVertical.Bottom => GrowDirection.End,
				_ => GrowDirection.Both,
			};
		}

		private void GrowHorizontal_Changed(IChangeable obj) {
			node.GrowHorizontal = LinkedComp.GrowHorizontal.Value switch {
				RGrowHorizontal.Left => GrowDirection.Begin,
				RGrowHorizontal.Right => GrowDirection.End,
				_ => GrowDirection.Both,
			};
		}

		private void MaxOffset_Changed(IChangeable obj) {
			node.OffsetBottom = LinkedComp.MaxOffset.Value.y;
			node.OffsetLeft = LinkedComp.MaxOffset.Value.x;
		}

		private void MinOffset_Changed(IChangeable obj) {
			node.OffsetTop = LinkedComp.MinOffset.Value.y;
			node.OffsetRight = LinkedComp.MinOffset.Value.x;
		}

		private void Max_Changed(IChangeable obj) {
			node.AnchorBottom = LinkedComp.Max.Value.y;
			node.AnchorRight = LinkedComp.Max.Value.x;
		}

		private void Min_Changed(IChangeable obj) {
			node.AnchorTop = LinkedComp.Min.Value.y;
			node.AnchorLeft = LinkedComp.Min.Value.x;
		}

		private void LayoutDir_Changed(IChangeable obj) {
			node.LayoutDirection = LinkedComp.LayoutDir.Value switch {
				RLayoutDir.Locale => LayoutDirectionEnum.Locale,
				RLayoutDir.Left_to_Right => LayoutDirectionEnum.Ltr,
				RLayoutDir.Right_to_Left => LayoutDirectionEnum.Rtl,
				_ => LayoutDirectionEnum.Inherited,
			};
		}

		private void MinSize_Changed(IChangeable obj) {
			node.CustomMinimumSize = new Vector2i(LinkedComp.MinSize.Value.x, LinkedComp.MinSize.Value.y);
		}

		private void ClipContents_Changed(IChangeable obj) {
			node.ClipContents = LinkedComp.ClipContents.Value;
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
