using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Editors/Ranges/ScrollBar")]
	public abstract partial class ScrollBar : Range
	{
		public readonly Sync<int> CustomStep;

		protected override void OnAttach() {
			base.OnAttach();
			FocusMode.Value = RFocusMode.All;
		}
	}
}
