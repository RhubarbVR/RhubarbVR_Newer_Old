using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Input.XRInput;
using RhuEngine.Linker;
using RhuEngine.Managers;

using RNumerics;

namespace RhuEngine.Input
{
	public sealed class XRInputSystem : IInputSystem
	{
		public InputManager InputManager { get; set; }
		public Vector3f HeadPos => Head?[TrackerPos.Default]?.Position ?? Vector3f.Zero;
		public Quaternionf HeadRot => Head?[TrackerPos.Default]?.Rotation ?? Quaternionf.Identity;

		public VirtualKeyboardInput virtualKeyboard;

		public readonly List<ITrackerDevice> Trackers = new();

		public ITrackerDevice Head;
		public ITrackerDevice LeftHand;
		public ITrackerDevice RightHand;

		public XRInputSystem(InputManager inputManager) {
			InputManager = inputManager;
			virtualKeyboard = new();
		}

		public void RemoveDevice(IInputDevice inputDevice) {
			if (inputDevice is ITrackerDevice target) {
				Trackers.Remove(target);
			}
		}

		public void LoadDevice(IInputDevice inputDevice) {
			if (inputDevice is ITrackerDevice target) {
				Trackers.Add(target);
				if (target.Hand == Handed.Left) {
					LeftHand = target;
				}
				if (target.Hand == Handed.Right) {
					RightHand = target;
				}
				if (target.TrackerType == TrackerType.Head) {
					Head = target;
				}
#if DEBUG
				RLog.Info($@" Tracker Loaded
	Name: {target.DeviceName}
	Description: {target.Description}
	Type: {target.TrackerType}
	Profile: {target.Profile}
	Hand: {target.Hand}
");
#endif
			}
		}

		public void Update() {

		}

		public ITrackerDevice GetHand(Handed value) {
			return value switch {
				Handed.Left => LeftHand,
				Handed.Right => RightHand,
				_ => null,
			};
		}
	}
}
