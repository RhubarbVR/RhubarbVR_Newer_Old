using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components.PrivateSpace
{

	[PrivateSpaceOnly]
	public sealed class DelegateCall : Component
	{
		public Action action;

		[Exposed]
		public void CallDelegate() {
			action();
		}
	}
}
