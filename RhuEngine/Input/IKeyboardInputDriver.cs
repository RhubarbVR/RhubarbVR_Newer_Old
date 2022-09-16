using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;

namespace RhuEngine.Input
{
	public interface IKeyboardInputDriver: IInputDevice
	{
		public bool GetIsDown(Key key);

		public string TypeDelta { get; }
	}
}
