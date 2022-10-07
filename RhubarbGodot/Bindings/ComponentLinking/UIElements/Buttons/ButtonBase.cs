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
	public abstract class ButtonBase<T, T2> : UIElementLinkBase<T, T2> where T : ButtonBase, new() where T2 : BaseButton, new()
	{
		public override void Init() {
			base.Init();
			LinkedComp.Disabled.Changed += Disabled_Changed;
			LinkedComp.ToggleMode.Changed += ToggleMode_Changed;
			LinkedComp.ButtonPressed.Changed += ButtonPressed_Changed;
			LinkedComp.ActionMode.Changed += ActionMode_Changed;
			LinkedComp.ButtonMask.Changed += ButtonMask_Changed;
			LinkedComp.KeepPressedOutside.Changed += KeepPressedOutside_Changed;
			Disabled_Changed(null);
			ToggleMode_Changed(null);
			ButtonPressed_Changed(null);
			ActionMode_Changed(null);
			ButtonMask_Changed(null);
			KeepPressedOutside_Changed(null);
		}

		private void KeepPressedOutside_Changed(IChangeable obj) {
			node.KeepPressedOutside = LinkedComp.KeepPressedOutside.Value;
		}

		private void ButtonMask_Changed(IChangeable obj) {
			var mask = MouseButton.None;
			if((LinkedComp.ButtonMask.Value | RButtonMask.Primary) != RButtonMask.None) {
				mask |= MouseButton.MaskLeft;
			}
			if ((LinkedComp.ButtonMask.Value | RButtonMask.Secondary) != RButtonMask.None) {
				mask |= MouseButton.MaskRight;
			}
			if ((LinkedComp.ButtonMask.Value | RButtonMask.Tertiary) != RButtonMask.None) {
				mask |= MouseButton.MaskMiddle;
			}
			node.ButtonMask = mask;
		}

		private void ActionMode_Changed(IChangeable obj) {
			node.ActionMode = LinkedComp.ActionMode.Value switch {
				RButtonActionMode.Relases => BaseButton.ActionModeEnum.Release,
				_ => BaseButton.ActionModeEnum.Press,
			};
		}

		private void ButtonPressed_Changed(IChangeable obj) {
			node.ButtonPressed = LinkedComp.ButtonPressed.Value;
		}

		private void ToggleMode_Changed(IChangeable obj) {
			node.ToggleMode = LinkedComp.ToggleMode.Value;
		}

		private void Disabled_Changed(IChangeable obj) {
			node.Disabled = LinkedComp.Disabled.Value;
		}
	}


	public sealed class ButtonBaseLink : ButtonBase<ButtonBase, BaseButton>
	{
		public override string ObjectName => "ButtonBase";

		public override void StartContinueInit() {

		}
	}
}
