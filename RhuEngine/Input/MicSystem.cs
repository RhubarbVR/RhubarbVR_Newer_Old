using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.Managers;

using RNumerics;

namespace RhuEngine.Input
{
	public sealed class MicSystem : IInputSystem
	{
		public InputManager InputManager { get; set; }

		public MicSystem(InputManager inputManager) {
			InputManager = inputManager;
		}

		public IMicDevice GetDefaultMic
		{
			get {
				foreach (var item in micDevices) {
					if (item.IsDefualt) {
						return item;
					}
				}
				return null;
			}
		}

		public IMicDevice this[string device]
		{
			get {
				if (device == null) {
					return GetDefaultMic;
				}
				foreach (var item in micDevices) {
					if (item.DeviceName == device) {
						return item;
					}
				}
				return null;
			}
		}

		public readonly List<IMicDevice> micDevices = new();

		public void RemoveDevice(IInputDevice inputDevice) {
			if (inputDevice is IMicDevice mic) {
				micDevices.Remove(mic);
			}
		}

		public void LoadDevice(IInputDevice inputDevice) {
			if (inputDevice is IMicDevice mic) {
				micDevices.Add(mic);
			}
		}

		public void Update() {

		}
	}
}
