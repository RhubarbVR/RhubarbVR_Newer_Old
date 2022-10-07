using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RDraggerVisibillity {
		Visible,
		Hidden,
		Hidden_Callapsed,
	}

	[Category("UI/Container/SplitContainer")]
	public class SplitContainer : Container
	{
		public readonly Sync<int> SplitOffset;
		public readonly Sync<bool> Collapsed;
		public readonly Sync<RDraggerVisibillity> DraggerVisibillity;
		public readonly Sync<bool> Vertical;
	}
}
