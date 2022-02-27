using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Visuals" })]
	public class UIGroup : UIComponent
	{
		public override void RenderUI() {
			foreach (Entity childEntity in Entity.children) {
				foreach (var item in childEntity.components) {
					if (item is UIComponent comp) {
						if (comp.Enabled) {
							comp.RenderUI();
						}
					}
				}
			}
		}
	}
}
