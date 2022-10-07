﻿using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Container")]
	public class CenterContainer : Container
	{
		public readonly Sync<bool> UseTopLeft;
	}
}
