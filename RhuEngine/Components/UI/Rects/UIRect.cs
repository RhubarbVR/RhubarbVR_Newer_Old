using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI/Rects" })]
	public class UIRect : Component
	{
		public enum RenderMeshUpdateType
		{
			None = 0,
			FullResized = 1,
			Movment = 2,
			CutMesh = 3,
			BindAndCanvasScale = 4,
		}

		[Flags]
		public enum UpdateType {
			None = 0,
			Local = 1,
			Parrent = 2,
			Child = 4,
		}
		[Default(0.05f)]
		[OnChanged(nameof(RegisterRectUpdateEvent))]
		public readonly Sync<float> Depth;
		[OnChanged(nameof(RegisterRectUpdateEvent))]
		public readonly Sync<Vector2f> OffsetLocalMin;
		[OnChanged(nameof(RegisterRectUpdateEvent))]
		public readonly Sync<Vector2f> OffsetLocalMax;
		[OnChanged(nameof(RegisterRectUpdateEvent))]
		public readonly Sync<Vector2f> OffsetMin;
		[OnChanged(nameof(RegisterRectUpdateEvent))]
		public readonly Sync<Vector2f> OffsetMax;
		[OnChanged(nameof(RegisterRectUpdateEvent))]
		public readonly Sync<Vector2f> AnchorMin;
		[OnChanged(nameof(RegisterRectUpdateEvent))]
		public readonly Sync<Vector2f> AnchorMax;

		public readonly SafeList<RenderUIComponent> RenderComponents = new();

		public float AddedDepth { get; private set; }
		public bool addedDepthIsDirty;
		public UICanvas CachedCanvas { get; private set; }
		public UICanvas Canvas => CachedCanvas;

		/// <summary>
		/// Should only be ran on the MainThread/GameThread
		/// </summary>
		public void AddAddedDepth(float addedDepth) {
			if(AddedDepth == addedDepth) {
				return;
			}
			AddedDepth = addedDepth;
			addedDepthIsDirty = true;
		}

		public void CanvasUpdate() {
			CachedCanvas = Entity.GetFirstComponent<UICanvas>();
			CachedCanvas ??= Entity.parent.Target?.UIRect?.CachedCanvas;
			foreach (Entity item in Entity.children) {
				item.UIRect?.CanvasUpdate();
			}
		}

		public override void OnLoaded() {
			base.OnLoaded();
			RenderComponents.SafeAddRange(Entity.GetAllComponents<RenderUIComponent>());
			CanvasUpdate();
			CachedCutMin = Entity.parent.Target?.UIRect?.CachedCutMin ?? Vector2f.NInf;
			CachedCutMax = Entity.parent.Target?.UIRect?.CachedCutMax ?? Vector2f.Inf;

		}

		public UpdateType Update { get; private set; }
		public RenderMeshUpdateType RenderMeshUpdate { get; private set; }
		public Vector2f CachedCutMin { get; private set; }
		public Vector2f CachedCutMax { get; private set; }
		public bool cutsAreDirty;

		public void UpdateCuttingZones(Vector2f NewMin, Vector2f NewMax) {
			var updatedValues = (CachedCutMin == NewMin)| (CachedCutMax == NewMax);
			CachedCutMin = NewMin;
			CachedCutMax = NewMax;
			foreach (Entity item in Entity.children) {
				item.UIRect?.UpdateCuttingZones(NewMin, NewMax);
			}
			if (updatedValues) {
				cutsAreDirty = true;
			}
		}
		public float CachedDepth { get; private set; }
		public Vector2f CachedMin { get; private set; }
		public Vector2f CachedMax { get; private set; }
		public Vector2f CachedElementSize { get; private set; }
		public Vector3f CachedMoveAmount { get; private set; }
		public UIRect ParentRect => Entity.parent.Target?.UIRect;

		public void MarkForRenderMeshUpdate(RenderMeshUpdateType renderMeshUpdateType) {
			if(RenderMeshUpdate >= renderMeshUpdateType) {
				if(renderMeshUpdateType <= RenderMeshUpdateType.Movment) {
					CachedMoveAmount = new Vector3f(CachedMin.x, CachedMin.y, AddedDepth);
				}
				return;
			}
			RenderMeshUpdate = renderMeshUpdateType;
			Engine.uiManager.AddUpdatedRectComponent(this);
			CachedMoveAmount = new Vector3f(CachedMin.x, CachedMin.y, AddedDepth);
		}

		public void ComputeDepth() {
			DepthUpdate((ParentRect?.CachedDepth??0f) + Depth.Value);
		}

		public void DepthUpdate(float newDepth) {
			if(CachedDepth != newDepth) {
				CachedDepth = newDepth;
				MarkForRenderMeshUpdate(RenderMeshUpdateType.FullResized);
			}
		}

		public void UpdateMinMax(Vector2f newMin,Vector2f newMax) {
			var newSize = newMax - newMin;
			var hasMoved = (CachedMin != newMin) | (CachedMax != newMax) | addedDepthIsDirty | cutsAreDirty;
			addedDepthIsDirty = false;
			cutsAreDirty = false;
			CachedMin = newMin;
			CachedMax = newMax;
			var sizeUpdate = CachedElementSize != newSize;
			CachedElementSize = newSize;
			if (sizeUpdate) {
				MarkForRenderMeshUpdate(RenderMeshUpdateType.FullResized);
				return;
			}
			if (hasMoved) {
				MarkForRenderMeshUpdate(RenderMeshUpdateType.Movment);
				return;
			}
		}

		public virtual void LocalRectUpdate() {
			ComputeDepth();
		}

		public virtual void ParrentRectUpdate() {
			ComputeDepth();

		}

		public virtual void ChildRectUpdate() {

		}

		public void RegisterRectUpdateEvent() {
			RegesterRectUpdate(UpdateType.Local);
		}

		public void RegesterRectUpdate(UpdateType updateType) {
			Update |= updateType;
		}

		internal void CompletedRectUpdate() {
			Update = UpdateType.None;
		}

		public override void AddListObject() {
			Engine.uiManager.AddRectComponent(this);
		}

		public override void RemoveListObject() {
			Engine.uiManager.AddRectComponent(this);
		}

		public override void OnAttach() {
			base.OnAttach();
			AnchorMin.Value = Vector2f.Zero;
			AnchorMax.Value = Vector2f.One;
			OffsetMin.Value = Vector2f.Zero;
			OffsetMax.Value = Vector2f.Zero;
		}

	}
}
