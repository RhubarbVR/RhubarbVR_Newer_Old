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
				var orderList = rectsList.AsParallel().OrderBy((x) => x.Entity.Depth);
				//Register Parrent Update Enum
				foreach (var item in orderList) {
					if ((item.Update & UIRect.UpdateType.Local) != UIRect.UpdateType.None) {
						item.LocalRectUpdate();
					}
					var parent = item.Entity.parent.Target;
					if (((parent?.UIRect?.Update ?? UIRect.UpdateType.None) & (UIRect.UpdateType.Local | UIRect.UpdateType.Parrent)) != UIRect.UpdateType.None) {
						item.RegesterRectUpdate(UIRect.UpdateType.Parrent);
						item.ParrentRectUpdate();
					}
				}
				//Register Child Update Enum
				foreach (var item in orderList.Reverse()) {
					foreach (Entity child in item.Entity.children) {
						if (((child?.UIRect?.Update ?? UIRect.UpdateType.None) & (UIRect.UpdateType.Local | UIRect.UpdateType.Child)) != UIRect.UpdateType.None) {
							item.RegesterRectUpdate(UIRect.UpdateType.Child);
							item.ChildRectUpdate();
						}
					}
				}
				//Apply rect Updates
				foreach (var item in rectsList) {
					item.CompletedRectUpdate();
				}
			});
		}
	}
}
