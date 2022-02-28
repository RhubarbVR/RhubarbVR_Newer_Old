using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Visuals" })]
	public class UIGroupPages : UIComponent
	{
		public Sync<int> CurrentPage;
		public Sync<int> MaxPages;

		[Exsposed]
		public void NextPage() {
			CurrentPage.Value++;
			if(CurrentPage >= MaxPages) {
				CurrentPage.Value = 0;
			}
		}

		[Exsposed]
		public void PreviousPage() {
			CurrentPage.Value--;
			if (CurrentPage < 0) {
				CurrentPage.Value = MaxPages - 1;
			}
		}

		public override void RenderUI() {
			foreach (Entity childEntity in Entity.children) {
				foreach (var item in childEntity.components) {
					if (item is UIGroupPageItem comp) {
						if (comp.Enabled) {
							if(comp.PageIndex.Value == CurrentPage.Value) {
								comp.RenderUI();
							}
						}
					}
				}
			}
		}
	}
}
