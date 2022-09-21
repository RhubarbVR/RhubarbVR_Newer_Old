using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Input
{
	public interface IDefualtDevice : IInputDevice
	{
		public bool IsDefualt { get; }
	}

	public interface INamedDevice : IInputDevice
	{
		public string DeviceName { get; }
	}

	public interface IInputDevice
	{
	}
}
