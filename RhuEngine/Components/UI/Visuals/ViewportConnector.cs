using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Container/Visuals")]
	public class ViewportConnector : UIVisuals
	{
		[OnChanged(nameof(LinkToViewport))]
		public readonly SyncRef<Viewport> Target;

		private Viewport _lastViewport;

		private void LinkToViewport() {
			if (_lastViewport is not null) {
				if (_lastViewport.ViewportConnector == this) {
					_lastViewport.ViewportConnector = null;
				}
			}
			_lastViewport = Target.Target;
			if (_lastViewport is not null) {
				_lastViewport.ViewportConnector = this;
			}
		}
	}
}
