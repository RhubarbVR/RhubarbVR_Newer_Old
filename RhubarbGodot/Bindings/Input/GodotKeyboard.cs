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
			return Godot.Input.IsPhysicalKeyPressed(ToGodotKey(key));
		}

		public static Godot.Key ToGodotKey(Key key) {
			return (Godot.Key)key;
		}
	}
}
