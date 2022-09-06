using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RhuEngine.Components;
using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.UIProcessing
{
	public class RectProcessor
	{
		public RectProcessor(UIManager uIManager) {
			UIManager = uIManager;
		}

		public UIManager UIManager { get; }
		public SafeList<UIRect> Rects => UIManager.Rects;
		public void Step() {
			Rects.SafeOperation((rectsList) => {
				//Todo: find where ther not being removed
				rectsList.RemoveAll(x => x.IsRemoved);
				var orderList = rectsList.OrderBy((x) => x.Entity.Depth).ToArray();
				//Register Parrent Update Enum
				for (var i = 0; i < orderList.Length; i++) {
					var item = orderList[i];
					var hasLocalUpdate = (item.Update & UIRect.UpdateType.Local) != UIRect.UpdateType.None;
					var parent = item.Entity.parent.Target;
					if (((parent?.UIRect?.Update ?? UIRect.UpdateType.None) & (UIRect.UpdateType.Local | UIRect.UpdateType.Parrent)) != UIRect.UpdateType.None) {
						item.RegesterRectUpdate(UIRect.UpdateType.Parrent);
						item.ParrentRectUpdate();
					}
					else if (hasLocalUpdate) {
						item.LocalRectUpdate();
					}
				}
				//Register Child Update Enum
				for (var i = orderList.Length - 1; i >= 0; i--) {
					var item = orderList[i];
					//Todo: make forLoop
					foreach (var child in item.Entity.children.Cast<Entity>()) {
						if (((child?.UIRect?.Update ?? UIRect.UpdateType.None) & (UIRect.UpdateType.Local | UIRect.UpdateType.Child)) != UIRect.UpdateType.None) {
							item.RegesterRectUpdate(UIRect.UpdateType.Child);
						}
					}
					if (item.Update != UIRect.UpdateType.None) {
						item.ChildRectUpdate();
					}
				}
				for (var i = 0; i < orderList.Length; i++) {
					var item = orderList[i];
					if ((item.Update & UIRect.UpdateType.ForeParrentUpdate) != UIRect.UpdateType.None) {
						item.ParrentRectUpdate();
					}
					item.CompletedRectUpdate();
				}
			});
		}
	}
}
