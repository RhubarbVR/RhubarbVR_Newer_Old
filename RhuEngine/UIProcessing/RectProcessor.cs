using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.UIProcessing
{
	public sealed class RectProcessor
	{
		public RectProcessor(UIManager uIManager) {
			UIManager = uIManager;
		}

		public UIManager UIManager { get; }
		public SafeList<UIRect> Rects => UIManager.Rects;

		private readonly RextComp _cop = new();

		public sealed class RextComp : IComparer<UIRect>
		{
			public int Compare(UIRect x, UIRect y) {
				return x.Entity.Depth < y.Entity.Depth ? -1 : x.Entity.Depth > y.Entity.Depth ? 1 : 0;
			}
		}

		public static bool Remove(UIRect uIRect) {
			return uIRect.IsRemoved;
		}

		public void Step() {
			Rects.SafeOperation((rectsList) => {
				//Todo: find where ther not being removed
				if (UIManager.ReOrder) {
					rectsList.RemoveAll(Remove);
					rectsList.Sort(_cop);
					UIManager.ReOrder = false;
				}

				//Register Parrent Update Enum
				foreach (var item in rectsList) {
					if (RInput.Key(Key.F10).IsActive()) {
						//Reloads all ui
						item.MarkForRenderMeshUpdate(UIRect.RenderMeshUpdateType.FullResized);
					}
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
				for (var i = rectsList.Count - 1; i >= 0; i--) {
					var item = rectsList[i];
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
				foreach (var item in rectsList) {
					if ((item.Update & UIRect.UpdateType.ForeParrentUpdate) != UIRect.UpdateType.None) {
						item.FowParrentRectUpdate();
					}
					item.CompletedRectUpdate();
				}
			});
		}
	}
}
