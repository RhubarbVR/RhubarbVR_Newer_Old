using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;
using Godot.Collections;

using RhuEngine.Input;
using RhuEngine.Input.XRInput;
using RhuEngine.Linker;

using RNumerics;

namespace RhubarbVR.Bindings.Input
{
	public class GodotXRPos : IPos
	{
		private readonly XRPositionalTracker _tracker;
		private readonly TrackerPos _trackerPos;

		public GodotXRPos(XRPositionalTracker tracker, TrackerPos trackerPos) {
			_tracker = tracker;
			_trackerPos = trackerPos;
			_tracker.GetPose(GetTrackerPosName(trackerPos));
		}

		public static string GetTrackerPosName(TrackerPos trackerPos) {
			return trackerPos switch {
				TrackerPos.Default => "default",
				TrackerPos.Aim => "aim",
				TrackerPos.Grip => "grip",
				TrackerPos.Skeleton => "skeleton",
				_ => "",
			};
		}

		public bool HasPos => _tracker.HasPose(GetTrackerPosName(_trackerPos));

		public Vector3f Position
		{
			get {
				if (HasPos) {
					var location = _tracker.GetPose(GetTrackerPosName(_trackerPos)).Transform.origin;
					return new Vector3f(location.x, location.y, location.z);
				}
				return Vector3f.Zero;
			}
		}

		public Quaternionf Rotation
		{
			get {
				if (HasPos) {
					var location = _tracker.GetPose(GetTrackerPosName(_trackerPos)).Transform.basis.GetRotationQuaternion();
					return new Quaternionf(location.x, location.y, location.z, location.w);
				}
				return Quaternionf.Identity;
			}
		}

	}

	public class GodotXRTracker : ITrackerDevice
	{
		public XRPositionalTracker Tracker;

		public GodotXRTracker(XRPositionalTracker tracker) {
			Tracker = tracker;
			foreach (TrackerPos item in Enum.GetValues(typeof(TrackerPos))) {
				dictionary.Add(item, new GodotXRPos(tracker, item));
			}
			Tracker.ButtonPressed += Tracker_ButtonPressed;
			tracker.ButtonReleased += Tracker_ButtonReleased;
			tracker.InputAxisChanged += Tracker_InputAxisChanged;
			tracker.InputValueChanged += Tracker_InputValueChanged;
		}

		private void Tracker_InputValueChanged(string name, double value) {
			if (_doubleinputs.ContainsKey(name)) {
				_doubleinputs[name] = value;
			}
			else {
				_doubleinputs.Add(name, value);
			}
		}

		private void Tracker_InputAxisChanged(string name, Vector2 vector) {
			if (_vectorinputs.ContainsKey(name)) {
				_vectorinputs[name] = new Vector2f(vector.x,vector.y);
			}
			else {
				_vectorinputs.Add(name, new Vector2f(vector.x, vector.y));
			}
		}

		private readonly System.Collections.Generic.Dictionary<string, Vector2f> _vectorinputs = new();

		private readonly System.Collections.Generic.Dictionary<string, double> _doubleinputs = new();

		private readonly System.Collections.Generic.Dictionary<string, bool> _boolinputs = new();

		private void Tracker_ButtonReleased(string name) {
			if (_boolinputs.ContainsKey(name)) {
				_boolinputs[name] = false;
			}
			else {
				_boolinputs.Add(name, false);
			}
		}

		private void Tracker_ButtonPressed(string name) {
			if (_boolinputs.ContainsKey(name)) {
				_boolinputs[name] = true;
			}
			else {
				_boolinputs.Add(name, true);
			}
		}
		public bool BoolInput(string Input) {
			return _boolinputs[Input];
		}
		public double DoubleInput(string Input) {
			return _doubleinputs[Input];
		}

		public Vector2f VectorInput(string Input) {
			return _vectorinputs[Input];
		}
		public IEnumerable<string> Inputs() {
			foreach (var item in _boolinputs.Keys) {
				yield return item;
			}
			foreach (var item in _doubleinputs.Keys) {
				yield return item;
			}
			foreach (var item in _vectorinputs.Keys) {
				yield return item;
			}
		}

		public bool HasBoolInput(string Input) {
			return _boolinputs.ContainsKey(Input);
		}

		public bool HasDoubleInput(string Input) {
			return _doubleinputs.ContainsKey(Input);
		}

		public bool HasVectorInput(string Input) {
			return _vectorinputs.ContainsKey(Input);
		}

		public void TriggerHapticPulse(float frequency, float amplitude, float duration_sec, float delay_sec) {
			XRServer.PrimaryInterface?.TriggerHapticPulse("haptic", Tracker.Name, frequency, amplitude, duration_sec, delay_sec);
		}

		public System.Collections.Generic.Dictionary<TrackerPos, IPos> dictionary = new();

		public IPos this[TrackerPos target] => dictionary.ContainsKey(target) ? dictionary[target] : null;

		public TrackerType TrackerType => (TrackerType)Tracker.Type;

		public string DeviceName => Tracker.Name;

		public string Profile => Tracker.Profile;

		public string Description => Tracker.Description;

		public Handed Hand
		{
			get {
				return Tracker.Hand switch {
					XRPositionalTracker.TrackerHand.Left => Handed.Left,
					XRPositionalTracker.TrackerHand.Right => Handed.Right,
					_ => Handed.Max,
				};
			}
		}

	}
}
