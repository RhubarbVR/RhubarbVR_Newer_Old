using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Editors/Ranges")]
	public class SpinBox : Range
	{
		[Default(true)]
		public readonly Sync<bool> Editable;
		public readonly Sync<bool> UpdateOnTextChanged;
		public readonly Sync<string> Prefix;
		public readonly Sync<string> Suffix;
		public readonly Sync<double> ArrowStep;

		protected override void OnAttach() {
			base.OnAttach();
			FocusMode.Value = RFocusMode.All;
		}
	}
}
