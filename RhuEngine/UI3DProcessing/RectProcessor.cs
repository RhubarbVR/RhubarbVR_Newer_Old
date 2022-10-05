using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.UI3DProcessing
{
	public sealed class RectProcessor
	{
		public RectProcessor(UI3DManager uIManager) {
			UIManager = uIManager;
		}

		public UI3DManager UIManager { get; }
		public SafeList<UI3DRect> Rects => UIManager.Rects;

		private readonly RextComp _cop = new();

		public sealed class RextComp : IComparer<UI3DRect>
		{
			public int Compare(UI3DRect x, UI3DRect y) {
				return x.Entity.Depth < y.Entity.Depth ? -1 : x.Entity.Depth > y.Entity.Depth ? 1 : 0;
			}
		}

		public static bool Remove(UI3DRect uIRect) {
			return uIRect.IsRemoved;
		}

		public void Step() {
			Rects.SafeOperation((rectsList) => {
				//Todo: find where ther not being removed
				rectsList.RemoveAll(Remove);
				if (UIManager.ReOrder) {
					rectsList.Sort(_cop);
					UIManager.ReOrder = false;
				}

				//Register Parrent Update Enum
				foreach (var item in rectsList) {
					if (UIManager.Engine.inputManager.KeyboardSystem.IsKeyJustDown(Key.F10)) {
						//Reloads all ui
						item.MarkForRenderMeshUpdate(UI3DRect.RenderMeshUpdateType.FullResized);
					}
					var hasLocalUpdate = (item.Update & UI3DRect.UpdateType.Local) != UI3DRect.UpdateType.None;
					var parent = item.Entity.parent.Target;
					if (((parent?.UIRect?.Update ?? UI3DRect.UpdateType.None) & (UI3DRect.UpdateType.Local | UI3DRect.UpdateType.Parrent)) != UI3DRect.UpdateType.None) {
						item.RegesterRectUpdate(UI3DRect.UpdateType.Parrent);
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
						if (((child?.UIRect?.Update ?? UI3DRect.UpdateType.None) & (UI3DRect.UpdateType.Local | UI3DRect.UpdateType.Child)) != UI3DRect.UpdateType.None) {
							item.RegesterRectUpdate(UI3DRect.UpdateType.Child);
						}
					}
					if (item.Update != UI3DRect.UpdateType.None) {
						item.ChildRectUpdate();
					}
				}
				foreach (var item in rectsList) {
					if ((item.Update & UI3DRect.UpdateType.ForeParrentUpdate) != UI3DRect.UpdateType.None) {
						item.FowParrentRectUpdate();
					}
					item.CompletedRectUpdate();
				}
			});
		}
	}
}
