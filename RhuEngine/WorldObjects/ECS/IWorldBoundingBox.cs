using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;

namespace RhuEngine.WorldObjects.ECS
{
	public interface IWorldBoundingBox
	{
		[Exposed]
		public AxisAlignedBox3f Bounds { get; }
	}
}
