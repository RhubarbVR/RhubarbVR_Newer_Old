using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components.PrivateSpace
{
	[PrivateSpaceOnly]
	public class DelegateCall:Component
	{
		public Action action;
		[Exposed]
		public void CallDelegate() {
			action();
		}
	}
}
