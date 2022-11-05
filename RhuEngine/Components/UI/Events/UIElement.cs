using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace RhuEngine.Components
{
	[Category("UI/Events")]
	public abstract class ElementEvent<T> : Component where T : CanvasItem
	{
		protected override void OnLoaded() {
			base.OnLoaded();
			Entity.CanvasItemUpdateEvent += Entity_CanvasItemUpdateEvent;
			Entity_CanvasItemUpdateEvent();
		}

		protected T LoadedData;

		private void Entity_CanvasItemUpdateEvent() {
			if(Entity.CanvasItem is T data) {
				LoadedData = data;
				LoadCanvasItem(data);
			}
		}

		protected abstract void LoadCanvasItem(T data);
		protected abstract void UnLoadCanvasItem(T data);

		public override void Dispose() {
			base.Dispose();
			Entity.CanvasItemUpdateEvent -= Entity_CanvasItemUpdateEvent;
			UnLoadCanvasItem(LoadedData);
			LoadedData = null;
		}
	}
}
