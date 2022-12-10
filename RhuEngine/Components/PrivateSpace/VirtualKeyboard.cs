using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Components.PrivateSpace;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Local" })]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class VirtualKeyboard : Component
	{
		[OnChanged(nameof(UpdateLayer))]
		public readonly Sync<int> Layer;

		public readonly SyncRef<ButtonBase> GrabButton;

		public void UpdateLayer() {
			for (var i = 0; i < KeyboardButtons.Count; i++) {
				var item = KeyboardButtons[i];
				item?.UpdateLabel();
			}
		}

		public sealed class KeyboardButton : SyncObject
		{
			public sealed class LayerValue : SyncObject
			{
				[OnChanged(nameof(UpdateLabel))]
				public readonly Sync<string> Label;
				public readonly Sync<string> Type;
				public readonly Sync<Key> TypedKey;

				public void UpdateLabel() {
					if (Parent.Parent is KeyboardButton keyboard) {
						keyboard.UpdateLabel();
					}
				}
			}
			private bool _press;

			private bool _justClick;
			private DateTimeOffset _lastClick;
			private DateTimeOffset _lastPressClick;

			public bool PressedEpoch => (DateTimeOffset.UtcNow - _lastClick).TotalSeconds >= 1;

			public bool Pressed => Button.Target?.ButtonPressed.Value ?? false;

			[Default(-1)]
			public readonly Sync<int> LayerChange;

			[Exposed]
			public void KeyDown() {
				_justClick = true;
				_press = true;
				_lastClick = DateTimeOffset.UtcNow;
				if (LayerChange.Value >= 0 && Parent.Parent is VirtualKeyboard keyboard) {
					keyboard.Layer.Value = keyboard.Layer.Value == LayerChange.Value ? 0 : LayerChange.Value;
				}
			}

			[Exposed]
			public void KeyUp() {
				_press = false;
			}

			public void SendClick(int layer) {
				if (layer >= LayerValues.Count) {
					return;
				}
				if (!(Button.Target?.ToggleMode.Value ?? false)) {
					if (!_press) {
						return;
					}
					if ((DateTimeOffset.UtcNow - _lastPressClick).Milliseconds <= 50) {
						return;
					}
				}
				var layerValue = LayerValues[layer];
				if (PressedEpoch || _justClick || (Button.Target?.ToggleMode.Value??false)) {
					Engine.inputManager.KeyboardSystem.virtualKeyboard.PressingKeys.Add(layerValue.TypedKey.Value);
					_lastPressClick = DateTimeOffset.UtcNow;
					_justClick = false;
					Engine.inputManager.KeyboardSystem.virtualKeyboard.TypeDelta += layerValue.Type.Value;
				}
			}

			public void UpdateLabel() {
				if (Button.Target is null) {
					return;
				}
				if (Parent.Parent is VirtualKeyboard keyboard) {
					var currentLayer = keyboard.Layer.Value;
					if (currentLayer < LayerValues.Count) {
						Button.Target.Text.Value = LayerValues[currentLayer].Label.Value;
					}
				}
			}
			[OnChanged(nameof(UpdateLabel))]
			public readonly SyncRef<Button> Button;

			[OnChanged(nameof(UpdateLabel))]
			public readonly SyncObjList<LayerValue> LayerValues;
		}

		public readonly SyncObjList<KeyboardButton> KeyboardButtons;

		Button AddCommlexButton(Vector2f offset, float widthadd, string label, string type, Key key, bool toggle = false, int layer = -1) {
			var button = _grabButton.Entity.AddChild("KeyButton").AttachComponent<Button>();
			var keyButton = KeyboardButtons.Add();
			var keyboardLable = keyButton.LayerValues.Add();
			keyboardLable.Label.Value = label;
			keyboardLable.TypedKey.Value = key;
			keyboardLable.Type.Value = type;
			var keyboardLable2 = keyButton.LayerValues.Add();
			keyboardLable2.Label.Value = label;
			keyboardLable2.TypedKey.Value = key;
			keyboardLable2.Type.Value = type;
			keyButton.Button.Target = button;
			keyButton.LayerChange.Value = layer;
			button.FocusMode.Value = RFocusMode.None;
			button.InputFilter.Value = RInputFilter.Pass;
			button.ToggleMode.Value = toggle;
			button.ButtonDown.Target = keyButton.KeyDown;
			button.ButtonUp.Target = keyButton.KeyUp;
			button.Min.Value = button.Max.Value = Vector2f.Zero;
			button.MinSize.Value = new Vector2i(40);
			button.Alignment.Value = RButtonAlignment.Center;
			button.MinOffset.Value = button.MaxOffset.Value = offset + (Vector2f)button.MinSize.Value + new Vector2f(widthadd / 2, 0);
			button.MinSize.Value += new Vector2i(widthadd, 0);
			return button;
		}


		Button AddKeyboardButton(string label, string label2, Key key) {
			var button = _grabButton.Entity.AddChild("KeyButton").AttachComponent<Button>();
			var keyButton = KeyboardButtons.Add();
			if (label2 is not null) {
				var keyboardLable = keyButton.LayerValues.Add();
				keyboardLable.Label.Value = label;
				keyboardLable.TypedKey.Value = key;
				keyboardLable.Type.Value = label;
				var keyboardLable2 = keyButton.LayerValues.Add();
				keyboardLable2.Label.Value = label2;
				keyboardLable2.TypedKey.Value = key;
				keyboardLable2.Type.Value = label2;
			}
			else {
				var keyboardLable = keyButton.LayerValues.Add();
				keyboardLable.Label.Value = label;
				keyboardLable.TypedKey.Value = key;
				keyboardLable.Type.Value = null;
				var keyboardLable2 = keyButton.LayerValues.Add();
				keyboardLable2.Label.Value = label;
				keyboardLable2.TypedKey.Value = key;
				keyboardLable2.Type.Value = null;
			}
			keyButton.Button.Target = button;
			button.FocusMode.Value = RFocusMode.None;
			button.InputFilter.Value = RInputFilter.Pass;
			button.ButtonDown.Target = keyButton.KeyDown;
			button.ButtonUp.Target = keyButton.KeyUp;
			return button;
		}

		Button AddKeyboardButtonWith(Vector2f offset, float widthadd, string label, string label2, Key key) {
			var button = AddKeyboardButton(label, label2, key);
			button.Min.Value = button.Max.Value = Vector2f.Zero;
			button.MinSize.Value = new Vector2i(40);
			button.Alignment.Value = RButtonAlignment.Center;
			button.MinOffset.Value = button.MaxOffset.Value = offset + (Vector2f)button.MinSize.Value + new Vector2f(widthadd / 2, 0);
			button.MinSize.Value += new Vector2i(widthadd, 0);
			return button;
		}

		void AddButtons((string, string, Key)?[] values, Vector2f offset) {
			for (var i = 0; i < values.Length; i++) {
				if (values[i] is null) {
					continue;
				}
				var item = values[i] ?? ("", "", Key.None);
				var button = AddKeyboardButton(item.Item1, item.Item2, item.Item3);
				button.Min.Value = button.Max.Value = Vector2f.Zero;
				button.MinSize.Value = new Vector2i(40);
				button.Alignment.Value = RButtonAlignment.Center;
				button.MinOffset.Value = button.MaxOffset.Value = offset + new Vector2f(43 * i, 0) + (Vector2f)button.MinSize.Value;
			}
		}


		private ButtonBase _grabButton;

		protected override void OnAttach() {
			base.OnAttach();
			var material = Entity.AttachComponent<UnlitMaterial>();
			var inputTexture = Entity.AttachComponent<Viewport>();
			inputTexture.TakeKeyboardFocus.Value = false;
			inputTexture.Size.Value = new Vector2i(1000, 300);
			material.MainTexture.Target = inputTexture;
			var canvas = Entity.AttachMesh<CanvasMesh>(material);
			canvas.FrontBind.Value = false;
			canvas.TopOffset.Value = false;
			canvas.Scale.Value = new Vector3f(inputTexture.Size.Value.x / inputTexture.Size.Value.y, 1, 1) * 2;
			canvas.InputInterface.Target = inputTexture;
			var e = Entity.AttachComponent<ValueCopy<Vector2i>>();
			e.Target.Target = canvas.Resolution;
			e.Source.Target = inputTexture.Size;


			_grabButton = Entity.AddChild("GrabBUtton").AttachComponent<ButtonBase>();
			_grabButton.FocusMode.Value = RFocusMode.None;
			_grabButton.InputFilter.Value = RInputFilter.Pass;
			_grabButton.ButtonMask.Value = RButtonMask.Secondary;
			_grabButton.Pressed.Target = Grab;
			GrabButton.Target = _grabButton;
			//TODO Add detection for other layouts
			BuildQWERTY_US();
		}
		[Exposed]
		public void Grab() {
			Entity.GetFirstComponent<Grabbable>()?.RemoteGrab(GrabButton.Target.LastHanded);
		}


		protected override void Step() {
			base.Step();
			if (!(World.IsOverlayWorld || World.IsPersonalSpace) || World.IsNetworked) {
				return;
			}
			for (var i = 0; i < KeyboardButtons.Count; i++) {
				var item = KeyboardButtons[i];
				if (item.Pressed) {
					item.SendClick(Layer);
				}
			}
		}


		private void BuildQWERTY_US() {
			AddCommlexButton(new Vector2f(0, 90), 22.5f, "TAB", "\t", Key.Tab);
			AddCommlexButton(new Vector2f(0, 135), 45f, "CAPS", null, Key.Capslock, true, 1);
			AddCommlexButton(new Vector2f(0, 180), 67.5f, "SHIFT", null, Key.Shift, true, 1);
			AddCommlexButton(new Vector2f(0, 225), 21f, "CTRL", null, Key.Ctrl, true);
			AddCommlexButton(new Vector2f(110, 225), 21f, "ALT", null, Key.Alt, true);

			AddCommlexButton(new Vector2f(175, 225), 215, " ", " ", Key.Space, false);
			AddCommlexButton(new Vector2f(433, 225), 21f, "ALT", null, Key.Alt, true);
			AddCommlexButton(new Vector2f(575, 225), 27f, "CTRL", null, Key.Ctrl, true);

			AddCommlexButton(new Vector2f(540, 180), 63f, "SHIFT", null, Key.Shift, true, 1);
			AddCommlexButton(new Vector2f(560, 135), 45f, "ENTER", "\n", Key.Enter, false);
			AddKeyboardButtonWith(new Vector2f(580, 90), 22.5f, "\\", "|", Key.Backslash);
			AddCommlexButton(new Vector2f(560, 45), 45f, "🠔", "\b", Key.Backspace);

			AddCommlexButton(new Vector2f(650, 225), 0, "🠔", null, Key.Left);
			AddCommlexButton(new Vector2f(695, 225), 0, "🠋", null, Key.Down);
			AddCommlexButton(new Vector2f(740, 225), 0, "🠚", null, Key.Right);
			AddCommlexButton(new Vector2f(695, 180), 0, "🠉", null, Key.Up);

			//TODO add numPad
			AddButtons(new (string, string, Key)?[] {
				("Prt",null,Key.Print),
			    ("Scr",null,Key.Scrolllock),
				("Pau",null,Key.Pause),
			}, new Vector2f(650, 0));
			AddButtons(new (string, string, Key)?[] {
				("Ins",null,Key.Insert),
				("Hom",null,Key.Home),
				("Pg🠉",null,Key.Pageup),
			}, new Vector2f(650, 45));
			AddButtons(new (string, string, Key)?[] {
				("Del",null,Key.Delete),
				("End",null,Key.End),
				("Pg🠋",null,Key.Pagedown),
			}, new Vector2f(650, 90));


			AddButtons(new (string, string, Key)?[] {
				("Num",null,Key.Numlock),
				("/","/",Key.KpDivide),
				("*","*",Key.KpMultiply),
				("-","-",Key.KpSubtract),
			}, new Vector2f(785, 45));
			AddButtons(new (string, string, Key)?[] {
				("7","7",Key.Kp7),
				("8","8",Key.Kp8),
				("9","9",Key.Kp9),
			}, new Vector2f(785, 90));
			AddButtons(new (string, string, Key)?[] {
				("4","4",Key.Kp4),
				("5","5",Key.Kp5),
				("6","6",Key.Kp6),
			}, new Vector2f(785, 135));
			AddButtons(new (string, string, Key)?[] {
				("1","1",Key.Kp1),
				("2","2",Key.Kp2),
				("3","3",Key.Kp3),
			}, new Vector2f(785, 180));


			AddCommlexButton(new Vector2f(785, 225), 45, "0", "0", Key.Kp0);
			AddCommlexButton(new Vector2f(873, 225), 0, ".", ".", Key.KpPeriod);

			var kpEnter = AddCommlexButton(new Vector2f(918, 202.5), 0, "Ent", "\n", Key.KpEnter);
			kpEnter.MinSize.Value += new Vector2i(0, 45);

			var kpAdd = AddCommlexButton(new Vector2f(918, 112.5), 0, "+", "+", Key.KpAdd);
			kpAdd.MinSize.Value += new Vector2i(0, 45);

			AddButtons(new (string, string, Key)?[] {
				("ESC",null,Key.Escape),
				null,
				("F1",null,Key.F1),
				("F2",null,Key.F2),
				("F3",null,Key.F3),
				("F4",null,Key.F4),
			}, new Vector2f(0, 0));


			AddButtons(new (string, string, Key)?[] {
				("F5",null,Key.F5),
				("F6",null,Key.F6),
				("F7",null,Key.F7),
				("F8",null,Key.F8),
			}, new Vector2f(280, 0));

			AddButtons(new (string, string, Key)?[] {
				("F9",null,Key.F9),
				("F10",null,Key.F10),
				("F11",null,Key.F11),
				("F12",null,Key.F12),
			}, new Vector2f(474, 0));

			AddButtons(new (string, string, Key)?[] {
				("`","~",Key.Quoteleft),
				("1","!",Key.Key1),
				("2","@",Key.Key2),
				("3","#",Key.Key3),
				("4","$",Key.Key4),
				("5","%",Key.Key5),
				("6","^",Key.Key6),
				("7","&",Key.Key7),
				("8","*",Key.Key8),
				("9","(",Key.Key9),
				("0",")",Key.Key0),
				("-","_",Key.Minus),
				("=","+",Key.Equal),
			}, new Vector2f(0, 45));
			AddButtons(new (string, string, Key)?[] {
				("q","Q",Key.Q),
				("w","W",Key.W),
				("e","E",Key.E),
				("r","R",Key.R),
				("t","T",Key.T),
				("y","Y",Key.Y),
				("u","U",Key.U),
				("i","I",Key.I),
				("o","O",Key.O),
				("p","P",Key.P),
				("[","{",Key.Bracketleft),
				("]","}",Key.Braceright),
			}, new Vector2f(65, 90));
			AddButtons(new (string, string, Key)?[] {
				("a","A",Key.A),
				("s","S",Key.S),
				("d","D",Key.D),
				("f","F",Key.F),
				("g","G",Key.G),
				("h","H",Key.H),
				("j","J",Key.J),
				("k","K",Key.K),
				("l","L",Key.L),
				(";",":",Key.Semicolon),
				("'",":",Key.Apostrophe),
			}, new Vector2f(87.5f, 135));
			AddButtons(new (string, string, Key)?[] {
				("z","Z",Key.Z),
				("x","X",Key.X),
				("c","C",Key.C),
				("v","V",Key.V),
				("b","B",Key.B),
				("n","N",Key.N),
				("m","M",Key.M),
				(",","<",Key.Comma),
				(".",">",Key.Period),
				("/","?",Key.Slash),
			}, new Vector2f(110, 180));
		}
	}
}
