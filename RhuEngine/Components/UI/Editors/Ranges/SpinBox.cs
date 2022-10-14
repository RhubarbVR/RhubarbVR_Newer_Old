using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using static System.Net.Mime.MediaTypeNames;

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
		public override string EditString => Value.Value.ToString();

		protected override void OnAttach() {
			base.OnAttach();
			FocusMode.Value = RFocusMode.All;
		}
	}
}
