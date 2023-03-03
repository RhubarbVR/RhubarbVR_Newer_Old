using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GDExtension;

using RhuEngine.Input;
using RhuEngine.Linker;

using Key = RhuEngine.Linker.Key;

namespace RhubarbVR.Bindings.Input
{
	public class GodotKeyboard : IKeyboardInputDriver
	{
		public string TypeDelta => EngineRunnerHelpers._.TypeDelta;

		public bool GetIsDown(Key key) {
			return GDExtension.Input.IsPhysicalKeyPressed(ToGodotKey(key));
		}

		public static GDExtension.Key ToGodotKey(Key key) {
			return (GDExtension.Key)key;
		}
	}
}
