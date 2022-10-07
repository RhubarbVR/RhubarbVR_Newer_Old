using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Container/Visuals")]
	public class ColorRect : UIVisuals
	{
		public readonly Sync<Colorf> Color; 
	}
}
