using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Components;
using RhuEngine.UIProcessing;

using RNumerics;

namespace RhuEngine.Managers
{
	public class UIManager : IManager
	{
		public readonly SafeList<UIRect> Rects = new();
		public RectProcessor RectProcessor;
		public readonly HashSet<UIRect> UpdatedRects = new();
		public void AddUpdatedRectComponent(UIRect comp) {
			UpdatedRects.Add(comp);
		}
		public void RemoveUpdatedRectComponents() {
			UpdatedRects.Clear();
		}
		public void AddRectComponent(UIRect comp) {
			Rects.SafeAdd(comp);
		}
		public void RemoveRectComponent(UIRect comp) {
			Rects.SafeRemove(comp);
		}

		public Engine Engine { get; set; }

		public void Step() {
			RectProcessor.Step();
			Parallel.ForEach(UpdatedRects, (rect) => {
				rect.RenderComponents.SafeOperation((list) => {
					foreach (var item in list) {
						item.ProcessMeshUpdate();
					}
				});
				rect.MarkRenderMeshUpdateAsDone();
			});
			UpdatedRects.Clear();
		}

		public void Dispose() {
		}

		public void Init(Engine engine) {
			Engine = engine;
			RectProcessor = new RectProcessor(this);
		}

		public void RenderStep() {
		}


	}
}
