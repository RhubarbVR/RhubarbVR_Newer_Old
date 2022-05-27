using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
namespace RhuEngine.Components
{

	[Category(new string[] { "Localisation" })]
	public class StandardLocale : Component
	{
		[OnChanged(nameof(LoadLocale))]
		public readonly Sync<string> Key;
		[OnChanged(nameof(LoadLocale))]
		public readonly Linker<string> TargetValue;

		public void LoadLocale() {
			if (TargetValue.Linked) {
				TargetValue.LinkedValue = Engine.localisationManager.GetLocalString(Key);
			}
		}

		public override void OnLoaded() {
			base.OnLoaded();
			Engine.localisationManager.LocalReload += LoadLocale;
		}
	}
}
