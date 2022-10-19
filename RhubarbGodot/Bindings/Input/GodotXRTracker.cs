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
		}

		public System.Collections.Generic.Dictionary<TrackerPos, IPos> dictionary = new();

		public IPos this[TrackerPos target]
		{
			get {
				if (dictionary.ContainsKey(target)) {
					return dictionary[target];
				}
				return null;
			}
		}

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
