using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace RhuEngine.Components
{
	[Category("UI/Editors/Ranges")]
	[UpdateLevel(UpdateEnum.Normal)]
	public class SpinBox : Range
	{
		[Default(true)]
		public readonly Sync<bool> Editable;
		public readonly Sync<bool> UpdateOnTextChanged;
		public readonly Sync<string> Prefix;
		public readonly Sync<string> Suffix;
		public readonly Sync<double> ArrowStep;
		public override string EditString => Value.Value.ToString();

		[Default(true)]
		public readonly Sync<bool> FocusLossOnEnter;

		protected override void Step() {
			base.Step();
			if (Engine.KeyboardInteraction == this) {
				if (Engine.inputManager.KeyboardSystem.IsKeyJustDown(Key.Return) && !Engine.inputManager.KeyboardSystem.IsKeyDown(Key.Shift)) {
					KeyboardUnBind();
				}
			}
		}
		protected override void OnAttach() {
			base.OnAttach();
			FocusMode.Value = RFocusMode.All;
		}
	}
}
