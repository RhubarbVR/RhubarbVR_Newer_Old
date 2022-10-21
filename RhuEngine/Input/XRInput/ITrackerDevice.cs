using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using RhuEngine.Linker;

using RNumerics;

namespace RhuEngine.Input.XRInput
{
	public enum TrackerType : int
	{
		//
		// Summary:
		//     The tracker tracks the location of the players head. This is usually a location
		//     centered between the players eyes. Note that for handheld AR devices this can
		//     be the current location of the device.
		Head = 1,
		//
		// Summary:
		//     The tracker tracks the location of a controller.
		Controller = 2,
		//
		// Summary:
		//     The tracker tracks the location of a base station.
		Basestation = 4,
		//
		// Summary:
		//     The tracker tracks the location and size of an AR anchor.
		Anchor = 8,
		//
		// Summary:
		//     Used internally to filter trackers of any known type.
		AnyKnown = 0x7F,
		//
		// Summary:
		//     Used internally if we haven't set the tracker type yet.
		Unknown = 0x80,
		//
		// Summary:
		//     Used internally to select all trackers.
		Any = 0xFF
	}

	public enum TrackerPos
	{
		None,
		Default,
		Aim,
		Grip,
		Skeleton
	}

	public interface IPos
	{
		public bool HasPos { get; }

		public Vector3f Position { get; }
		public Quaternionf Rotation { get; }
	}

	public interface ITrackerDevice : INamedDevice
	{
		public IPos this[TrackerPos target] { get; }

		public bool HasBoolInput(string Input);

		public bool BoolInput(string Input);

		public IEnumerable<string> Inputs();

		public bool HasDoubleInput(string Input);

		public double DoubleInput(string Input);
		public bool HasVectorInput(string Input);

		public Vector2f VectorInput(string Input);

		public TrackerType TrackerType { get; }

		public string Profile { get; }

		public string Description { get; }

		public Handed Hand { get; }
	}
}
