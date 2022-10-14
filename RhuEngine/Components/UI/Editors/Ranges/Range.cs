using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Editors/Ranges")]
	public class Range : UIElement
	{
		[Default(0.0)]public readonly Sync<double> MinValue;
		[Default(100.0)]public readonly Sync<double> MaxValue;
		[Default(0.0001)]public readonly Sync<double> StepValue;
		public readonly Sync<double> PageValue;
		public readonly Sync<double> Value;
		public readonly Sync<bool> ExpEdit;
		public readonly Sync<bool> Rounded;
		public readonly Sync<bool> AllowGreater;
		public readonly Sync<bool> AllowLesser;
		public readonly SyncDelegate ValueUpdated;

	}
}
