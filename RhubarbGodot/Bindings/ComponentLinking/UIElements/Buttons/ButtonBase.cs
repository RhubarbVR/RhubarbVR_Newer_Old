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
using RNumerics;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public abstract class ButtonBase<T, T2> : UIElementLinkBase<T, T2> where T : ButtonBase, new() where T2 : BaseButton, new()
	{
		public override void Remove() {
			node.ButtonDown -= Node_ButtonDown;
			node.ButtonUp -= Node_ButtonUp;
			node.Pressed -= Node_Pressed;
			node.Toggled -= Node_Toggled;
			node.GuiInput -= Node_GuiInput;
			base.Remove();
		}

		public override void Init() {
			base.Init();
			LinkedComp.Disabled.Changed += Disabled_Changed;
			LinkedComp.ToggleMode.Changed += ToggleMode_Changed;
			LinkedComp.ButtonPressed.Changed += ButtonPressed_Changed;
			LinkedComp.ActionMode.Changed += ActionMode_Changed;
			LinkedComp.ButtonMask.Changed += ButtonMask_Changed;
			LinkedComp.KeepPressedOutside.Changed += KeepPressedOutside_Changed;
			node.ButtonDown += Node_ButtonDown;
			node.ButtonUp += Node_ButtonUp;
			node.Pressed += Node_Pressed;
			node.Toggled += Node_Toggled;
			node.GuiInput += Node_GuiInput;
			LinkedComp.GetPosFunc = GetPos;

			Disabled_Changed(null);
			ToggleMode_Changed(null);
			ButtonPressed_Changed(null);
			ActionMode_Changed(null);
			ButtonMask_Changed(null);
			KeepPressedOutside_Changed(null);
		}
		public Handed GetSideFromMouseID(int id) {
			return (Handed)(id >> 16);
		}

		public Vector2f LeftPos;
		public Vector2f RightPos;
		public Vector2f MaxPos;

		private Vector2f GetPos(Handed handed) {
			return handed switch {
				Handed.Left => LeftPos,
				Handed.Right => RightPos,
				Handed.Max => MaxPos,
				_ => Vector2f.Zero,
			};
		}

		private void Node_GuiInput(InputEvent @event) {
			if (@event is InputEventMouse mouse) {
				var newPos = new Vector2f(mouse.GlobalPosition.x, mouse.GlobalPosition.y);
				switch (GetSideFromMouseID(mouse.Device)) {
					case Handed.Left:
						LeftPos = newPos;
						break;
					case Handed.Right:
						RightPos = newPos;
						break;
					case Handed.Max:
						MaxPos = newPos;
						break;
					default:
						break;
				}
				if ((mouse.ButtonMask & node.ButtonMask) == 0) {
					return;
				}
				LinkedComp.LastHanded = GetSideFromMouseID(mouse.Device);
			}

		}


		private void SendState() {
			LinkedComp.ButtonPressed.Value = node.ButtonPressed;
		}

		private void Node_ButtonDown() {
			SendState();
			RUpdateManager.ExecuteOnEndOfFrame(LinkedComp.ButtonDown.Invoke);
		}

		private void Node_ButtonUp() {
			SendState();
			RUpdateManager.ExecuteOnEndOfFrame(LinkedComp.ButtonUp.Invoke);
		}

		private void Node_Pressed() {
			SendState();
			RUpdateManager.ExecuteOnEndOfFrame(() => {
				LinkedComp?.Pressed?.Invoke();
				LinkedComp?.SendPressedAction();
			});
		}

		private void Node_Toggled(bool buttonPressed) {
			SendState();
			RUpdateManager.ExecuteOnEndOfFrame(() => LinkedComp?.Toggled?.Target?.Invoke(buttonPressed));
		}

		private void KeepPressedOutside_Changed(IChangeable obj) {
			node.KeepPressedOutside = LinkedComp.KeepPressedOutside.Value;
		}

		private void ButtonMask_Changed(IChangeable obj) {
			var mask = (MouseButtonMask)0;
			if ((LinkedComp.ButtonMask.Value & RButtonMask.Primary) != RButtonMask.None) {
				mask |= MouseButtonMask.Left;
			}
			if ((LinkedComp.ButtonMask.Value & RButtonMask.Secondary) != RButtonMask.None) {
				mask |= MouseButtonMask.Right;
			}
			if ((LinkedComp.ButtonMask.Value & RButtonMask.Tertiary) != RButtonMask.None) {
				mask |= MouseButtonMask.Middle;
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
			if (node.ButtonPressed != LinkedComp.ButtonPressed.Value) {
				node.ButtonPressed = LinkedComp.ButtonPressed.Value;
			}
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
