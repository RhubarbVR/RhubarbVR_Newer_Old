﻿using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Container/FlowContainer")]
	public class FlowContainer : Container
	{
		public readonly Sync<bool> Vertical;
	}
}