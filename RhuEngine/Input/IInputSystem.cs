using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Input
{
	public interface IInputSystem
	{
		public void RemoveDevice(IInputDevice inputDevice);

		public void LoadDevice(IInputDevice inputDevice);
		public void Update();
	}
}
