using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Container/FlowContainer")]
	public partial class FlowContainer : Container
	{
		public readonly Sync<bool> Vertical;
	}
}
