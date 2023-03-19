using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;

namespace RhuEngine.Components
{
	[Category(new string[] { "Audio" })]
	public sealed partial class AudioListener3D : LinkedWorldComponent
	{
		public readonly Sync<bool> Current;
	}
}
