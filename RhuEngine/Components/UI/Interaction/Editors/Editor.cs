using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Interaction\\Editors" })]
	public abstract class Editor : Component
	{
		public SyncDelegate<Action<object>> SetValue;

		public SyncDelegate<Func<object>> GetValue;
	}
}
