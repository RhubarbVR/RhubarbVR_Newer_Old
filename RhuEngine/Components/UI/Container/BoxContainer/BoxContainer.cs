using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RBoxContainerAlignment {
		Begin,
		Center,
		End,
	}

	[Category("UI/Container/BoxContainer")]
	public partial class BoxContainer : Container
	{
		public readonly Sync<RBoxContainerAlignment> Alignment;
		public readonly Sync<bool> Vertical;
	}
}
