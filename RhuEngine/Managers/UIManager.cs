using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.UIProcessing;

using RNumerics;

namespace RhuEngine.Managers
{
	public sealed class UIManager : IManager
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
		public bool SingleThread;

		public void Step() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			RectProcessor.Step();
			if (SingleThread) {
				foreach (var item in UpdatedRects) {
					item.RenderComponents.SafeOperation((list) => {
						foreach (var item in list) {
							item.ProcessMeshUpdate();
						}
					});
					item.MarkRenderMeshUpdateAsDone();
				}
			}
			else {
				Parallel.ForEach(UpdatedRects, (rect) => {
					rect.RenderComponents.SafeOperation((list) => {
						foreach (var item in list) {
							item.ProcessMeshUpdate();
						}
					});
					rect.MarkRenderMeshUpdateAsDone();
				});
			}
			RemoveUpdatedRectComponents();
		}

		public void Dispose() {
		}

		public void Init(Engine engine) {
			Engine = engine;
			RectProcessor = new RectProcessor(this);
			RLog.Info($"UI Manager sees {Environment.ProcessorCount} Threads");
			SingleThread = Environment.ProcessorCount <= 2;
			if (SingleThread) {
				RLog.Info($"UI Manager Running Single Threaded");
			}
		}

		public void RenderStep() {
		}


	}
}
