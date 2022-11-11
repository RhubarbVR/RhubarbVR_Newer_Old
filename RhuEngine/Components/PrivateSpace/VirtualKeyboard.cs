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
			foreach (KeyboardButton item in KeyboardButtons) {
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
					keyboard.Layer.Value = LayerChange.Value;
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
				}
				if ((DateTimeOffset.UtcNow - _lastPressClick).Milliseconds <= 50) {
					return;
				}
				var layerValue = LayerValues[layer];
				Engine.inputManager.KeyboardSystem.virtualKeyboard.PressingKeys.Add(layerValue.TypedKey.Value);
				if (PressedEpoch || _justClick) {
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

		protected override void OnAttach() {
			base.OnAttach();
			var material = Entity.AttachComponent<UnlitMaterial>();
			var inputTexture = Entity.AttachComponent<Viewport>();
			inputTexture.TakeKeyboardFocus.Value = false;
			inputTexture.Size.Value = new Vector2i(760, 440);
			material.MainTexture.Target = inputTexture;
			var canvas = Entity.AttachMesh<CanvasMesh>(material);
			canvas.FrontBind.Value = false;
			canvas.TopOffset.Value = false;
			canvas.Scale.Value /= 2;
			canvas.InputInterface.Target = inputTexture;
			var e = Entity.AttachComponent<ValueCopy<Vector2i>>();
			e.Target.Target = canvas.Resolution;
			e.Source.Target = inputTexture.Size;


			var grabButton = Entity.AddChild("GrabBUtton").AttachComponent<ButtonBase>();
			grabButton.FocusMode.Value = RFocusMode.None;
			grabButton.InputFilter.Value = RInputFilter.Pass;
			grabButton.ButtonMask.Value = RButtonMask.Secondary;
			grabButton.Pressed.Target = Grab;
			GrabButton.Target = grabButton;


			Button AddKeyboardButton(string label, string label2, Key key) {
				var button = grabButton.Entity.AddChild("KeyButton").AttachComponent<Button>();
				var keyButton = KeyboardButtons.Add();
				var keyboardLable = keyButton.LayerValues.Add();
				keyboardLable.Label.Value = label;
				keyboardLable.TypedKey.Value = key;
				keyboardLable.Type.Value = label;
				var keyboardLable2 = keyButton.LayerValues.Add();
				keyboardLable2.Label.Value = label2;
				keyboardLable2.TypedKey.Value = key;
				keyboardLable2.Type.Value = label2;
				keyButton.Button.Target = button;
				button.FocusMode.Value = RFocusMode.None;
				button.InputFilter.Value = RInputFilter.Pass;
				button.ButtonDown.Target = keyButton.KeyDown;
				button.ButtonUp.Target = keyButton.KeyUp;
				return button;
			}

			void AddButtons((string, string, Key)[] values, Vector2f offset) {
				for (var i = 0; i < values.Length; i++) {
					var item = values[i];
					var button = AddKeyboardButton(item.Item1, item.Item2, item.Item3);
					button.Min.Value = button.Max.Value = Vector2f.Zero;
					button.MinSize.Value = new Vector2i(25);
					button.Alignment.Value = RButtonAlignment.Center;
					button.MinOffset.Value = button.MaxOffset.Value = offset + new Vector2f(30 * i, 0) + (Vector2f)(button.MinSize.Value);
				}
			}

			AddButtons(new (string, string, Key)[] {
				("`","~",Key.Backtick),
				("1","!",Key.N1),
				("2","@",Key.N2),
				("3","#",Key.N3),
				("4","$",Key.N4),
				("5","%",Key.N5),
				("6","^",Key.N6),
				("7","&",Key.N7),
				("8","*",Key.N8),
				("9","(",Key.N9),
				("0",")",Key.N0),
				("-","_",Key.Subtract),
				("=","+",Key.Equals),
			}, Vector2f.Zero);
			AddButtons(new (string, string, Key)[] {
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
				("[","{",Key.BracketOpen),
				("]","}",Key.BracketClose),
			}, new Vector2f(35,45));
			AddButtons(new (string, string, Key)[] {
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
			}, new Vector2f(40, 90));
			AddButtons(new (string, string, Key)[] {
				("z","Z",Key.Z),
				("x","X",Key.X),
				("c","C",Key.C),
				("v","V",Key.V),
				("b","B",Key.B),
				("n","N",Key.N),
				("m","M",Key.M),
				(",","<",Key.Comma),
				(".",">",Key.Period),
				("/","?",Key.SlashFwd),
			}, new Vector2f(45, 135));
		}
		[Exposed]
		public void Grab() {
			Entity.GetFirstComponent<Grabbable>()?.RemoteGrab(GrabButton.Target.LastHanded);
		}


		protected override void Step() {
			base.Step();
			foreach (KeyboardButton item in KeyboardButtons) {
				if (item.Pressed) {
					item.SendClick(Layer);
				}
			}
		}
	}
}
