using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
namespace RhuEngine.Components
{

	[Category(new string[] { "Localisation" })]
	public sealed class StandardLocale : Component
	{
		[OnChanged(nameof(LoadLocale))]
		public readonly Sync<string> Key;
		[OnChanged(nameof(LoadLocale))]
		public readonly Linker<string> TargetValue;
		[OnChanged(nameof(LoadLocale))]
		public readonly Sync<string> Append;
		[OnChanged(nameof(LoadLocale))]
		public readonly Sync<string> AppendFront;
		private void LoadLocale() {
			if (TargetValue.Linked) {
				TargetValue.LinkedValue = AppendFront.Value + Engine.localisationManager.GetLocalString(Key) + Append.Value;
			}
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			Engine.localisationManager.LocalReload += LoadLocale;
		}
	}
}
