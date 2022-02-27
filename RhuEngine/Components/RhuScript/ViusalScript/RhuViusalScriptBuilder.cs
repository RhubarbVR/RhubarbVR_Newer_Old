using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "RhuScript\\ViusalScript" })]
	public class RhuViusalScriptBuilder : Component
	{
		public SyncRef<RhuScript> Script;
	}
}
