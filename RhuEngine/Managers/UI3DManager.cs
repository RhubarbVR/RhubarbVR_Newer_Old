using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.UI3DProcessing;

using RNumerics;

namespace RhuEngine.Managers
{
	public sealed class UI3DManager : IManager
	{
		public readonly SafeList<UI3DRect> Rects = new();
		public bool ReOrder;
		public RectProcessor RectProcessor;
		public readonly HashSet<UI3DRect> UpdatedRects = new();
		public void AddUpdatedRectComponent(UI3DRect comp) {
			UpdatedRects.Add(comp);
		}
		public void RemoveUpdatedRectComponents() {
			UpdatedRects.Clear();
		}
		public void AddRectComponent(UI3DRect comp) {
			Rects.SafeAdd(comp);
			ReOrder = true;
		}
		public void RemoveRectComponent(UI3DRect comp) {
			Rects.SafeRemove(comp);
		}

		public Engine Engine { get; set; }
		public bool SingleThread;
		private static void RectRenderUpdate(List<BaseRenderUI3DComponent> list) {
			foreach (var item in list) {
				item.ProcessMeshUpdate();
			}
		}

		private static void RectUpdate(UI3DRect uIRect) {
			uIRect.RenderComponents.SafeOperation(RectRenderUpdate);
			uIRect.MarkRenderMeshUpdateAsDone();
		}

		public void Step() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			RectProcessor.Step();
			if (SingleThread) {
				foreach (var item in UpdatedRects) {
					RectUpdate(item);
				}
			}
			else {
				Parallel.ForEach(UpdatedRects, RectUpdate);
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
