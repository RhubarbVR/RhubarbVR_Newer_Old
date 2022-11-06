using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	public abstract class VisualInstance3D: LinkedWorldComponent
	{
		[Default(RenderLayer.MainLayer)]
		public readonly Sync<RenderLayer> renderLayer;
	}
}
