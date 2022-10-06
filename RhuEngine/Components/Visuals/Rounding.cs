using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Visuals" })]
	public sealed class Rounding : Component
	{
		public readonly Linker<float> RectRounding;
		public readonly Linker<int> RectRoundingSteps;

		[Exposed]
		public void BindRect(UI3DRectangle uIRectangle) {
			RectRounding.SetLinkerTarget(uIRectangle.Rounding);
			RectRoundingSteps.SetLinkerTarget(uIRectangle.RoundingSteps);
			Engine_SettingsUpdate();
		}

		protected override void OnLoaded() {
			Engine.SettingsUpdate += Engine_SettingsUpdate;
		}

		public override void Dispose() {
			Engine.SettingsUpdate -= Engine_SettingsUpdate;
			base.Dispose();
		}
		private void Engine_SettingsUpdate() {
			if (RectRounding.Linked) {
				RectRounding.LinkedValue = Engine.MainSettings.UISettings.RectRounding;
			}
			if (RectRoundingSteps.Linked) {
				RectRoundingSteps.LinkedValue = Engine.MainSettings.UISettings.RectRoundingSteps;
			}
		}
	}
}
