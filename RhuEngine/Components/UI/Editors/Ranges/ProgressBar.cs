using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RProgressBarFill {
		Begin_To_End,
		End_To_Begin,
		Top_To_Bottom,
		Bottom_To_Top,
	}

	[Category("UI/Editors/Ranges")]
	public partial class ProgressBar : Range
	{
		[Default(true)]
		public readonly Sync<bool> ShowPerrcentage;
		public readonly Sync<RProgressBarFill> FillMode;
	}
}
