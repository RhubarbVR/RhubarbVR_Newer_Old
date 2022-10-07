using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Container")]
	public class GridContainer : Container
	{
		[Default(1)]
		public readonly Sync<int> Columns;
	}
}
