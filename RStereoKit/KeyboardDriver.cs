using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Input;

using StereoKit;

namespace RStereoKit
{
	public sealed class SKKeyboardDriver: IKeyboardInputDriver
	{
		public string TypeDelta
		{
			get {
				var chare = Input.TextConsume();
				Input.TextReset();
				return chare == '\0' ? "" : chare.ToString();
			}
		}

		public bool GetIsDown(RhuEngine.Linker.Key key) {
			return Input.Key((Key)key).IsActive();
		}
	}
}
