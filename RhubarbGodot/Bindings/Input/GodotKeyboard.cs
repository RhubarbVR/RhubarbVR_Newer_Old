using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine.Input;
using RhuEngine.Linker;

using Key = RhuEngine.Linker.Key;

namespace RhubarbVR.Bindings.Input
{
	public class GodotKeyboard : IKeyboardInputDriver
	{
		public string TypeDelta => EngineRunner._.TypeDelta;

		public bool GetIsDown(Key key) {
			return Godot.Input.IsKeyPressed(ToGodotKey(key));
		}

		public static Godot.Key ToGodotKey(Key key) {
			return key switch {
				Key.Backspace => Godot.Key.Backspace,
				Key.Tab => Godot.Key.Tab,
				Key.Return => Godot.Key.Enter,
				Key.Shift => Godot.Key.Shift,
				Key.Ctrl => Godot.Key.Ctrl,
				Key.Alt => Godot.Key.Alt,
				Key.CapsLock => Godot.Key.Capslock,
				Key.Esc => Godot.Key.Escape,
				Key.Space => Godot.Key.Space,
				Key.End => Godot.Key.End,
				Key.Home => Godot.Key.Home,
				Key.Left => Godot.Key.Left,
				Key.Right => Godot.Key.Right,
				Key.Up => Godot.Key.Up,
				Key.Down => Godot.Key.Down,
				Key.PageUp => Godot.Key.Pageup,
				Key.PageDown => Godot.Key.Pagedown,
				Key.Printscreen => Godot.Key.Print,
				Key.Insert => Godot.Key.Insert,
				Key.Del => Godot.Key.Delete,
				Key.N0 => Godot.Key.Key0,
				Key.N1 => Godot.Key.Key1,
				Key.N2 => Godot.Key.Key2,
				Key.N3 => Godot.Key.Key3,
				Key.N4 => Godot.Key.Key4,
				Key.N5 => Godot.Key.Key5,
				Key.N6 => Godot.Key.Key6,
				Key.N7 => Godot.Key.Key7,
				Key.N8 => Godot.Key.Key8,
				Key.N9 => Godot.Key.Key9,
				Key.A => Godot.Key.A,
				Key.B => Godot.Key.B,
				Key.C => Godot.Key.C,
				Key.D => Godot.Key.D,
				Key.E => Godot.Key.E,
				Key.F => Godot.Key.F,
				Key.G => Godot.Key.G,
				Key.H => Godot.Key.H,
				Key.I => Godot.Key.I,
				Key.J => Godot.Key.J,
				Key.K => Godot.Key.K,
				Key.L => Godot.Key.L,
				Key.M => Godot.Key.M,
				Key.N => Godot.Key.N,
				Key.O => Godot.Key.O,
				Key.P => Godot.Key.P,
				Key.Q => Godot.Key.Q,
				Key.R => Godot.Key.R,
				Key.S => Godot.Key.S,
				Key.T => Godot.Key.T,
				Key.U => Godot.Key.U,
				Key.V => Godot.Key.V,
				Key.W => Godot.Key.W,
				Key.X => Godot.Key.X,
				Key.Y => Godot.Key.Y,
				Key.Z => Godot.Key.Z,
				Key.Num0 => Godot.Key.Kp0,
				Key.Num1 => Godot.Key.Kp1,
				Key.Num2 => Godot.Key.Kp2,
				Key.Num3 => Godot.Key.Kp3,
				Key.Num4 => Godot.Key.Kp4,
				Key.Num5 => Godot.Key.Kp5,
				Key.Num6 => Godot.Key.Kp6,
				Key.Num7 => Godot.Key.Kp7,
				Key.Num8 => Godot.Key.Kp8,
				Key.Num9 => Godot.Key.Kp9,
				Key.F1 => Godot.Key.F1,
				Key.F2 => Godot.Key.F2,
				Key.F3 => Godot.Key.F3,
				Key.F4 => Godot.Key.F4,
				Key.F5 => Godot.Key.F5,
				Key.F6 => Godot.Key.F6,
				Key.F7 => Godot.Key.F7,
				Key.F8 => Godot.Key.F8,
				Key.F9 => Godot.Key.F9,
				Key.F10 => Godot.Key.F10,
				Key.F11 => Godot.Key.F11,
				Key.F12 => Godot.Key.F12,
				Key.Comma => Godot.Key.Comma,
				Key.Period => Godot.Key.Period,
				Key.SlashFwd => Godot.Key.Slash,
				Key.SlashBack => Godot.Key.Backslash,
				Key.Semicolon => Godot.Key.Semicolon,
				Key.Apostrophe => Godot.Key.Apostrophe,
				Key.BracketOpen => Godot.Key.Bracketleft,
				Key.BracketClose => Godot.Key.Bracketright,
				Key.Minus => Godot.Key.Minus,
				Key.Equals => Godot.Key.Equal,
				Key.Backtick => Godot.Key.Quoteleft,
				Key.Multiply => Godot.Key.Multiply,
				Key.Add => Godot.Key.KpAdd,
				Key.Subtract => Godot.Key.KpSubtract,
				Key.Decimal => Godot.Key.KpPeriod,
				Key.Divide => Godot.Key.KpDivide,
				_ => Godot.Key.None,
			};
		}
	}
}
