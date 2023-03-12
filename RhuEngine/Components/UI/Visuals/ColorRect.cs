using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Container/Visuals")]
	public partial class ColorRect : UIVisuals
	{
		public readonly Sync<Colorf> Color;
		protected override void OnAttach() {
			base.OnAttach();
			Color.Value = Colorf.White;
		}
	}
}
