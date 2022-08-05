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
		public enum UpdateType
		{
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
			if (AddedDepth == addedDepth) {
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
		
		internal void MarkRenderMeshUpdateAsDone() {
			RenderMeshUpdate = RenderMeshUpdateType.None;
		}


		public Vector2f CachedCutMin { get; private set; }
		public Vector2f CachedCutMax { get; private set; }
		public bool cutsAreDirty;

		public void UpdateCuttingZones(Vector2f NewMin, Vector2f NewMax) {
			var updatedValues = (CachedCutMin == NewMin) | (CachedCutMax == NewMax);
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
		public Vector2f CachedOverlapSize { get; private set; }

		public Vector2f MoveAmount { get; private set; }

		public void ApplyMovement(Vector2f moveAmount) {
			if (MoveAmount != moveAmount) {
				MoveAmount = moveAmount;
				RegisterRectUpdateEvent();
			}
		}

		public UIRect ParentRect => Entity.parent.Target?.UIRect;

		protected virtual void OnMarkedForRenderMeshUpdate(RenderMeshUpdateType renderMeshUpdateType) {

		}

		public void MarkForRenderMeshUpdate(RenderMeshUpdateType renderMeshUpdateType) {
			if (RenderMeshUpdate >= renderMeshUpdateType) {
				if (renderMeshUpdateType <= RenderMeshUpdateType.Movment) {
					CachedMoveAmount = new Vector3f(CachedMin.x, CachedMin.y, AddedDepth);
				}
				return;
			}
			RenderMeshUpdate = renderMeshUpdateType;
			Engine.uiManager.AddUpdatedRectComponent(this);
			CachedMoveAmount = new Vector3f(CachedMin.x, CachedMin.y, AddedDepth);
			OnMarkedForRenderMeshUpdate(renderMeshUpdateType);
		}

		public void ComputeDepth() {
			DepthUpdate((ParentRect?.CachedDepth ?? 0f) + Depth.Value);
		}

		public void DepthUpdate(float newDepth) {
			if (CachedDepth != newDepth) {
				CachedDepth = newDepth;
				MarkForRenderMeshUpdate(RenderMeshUpdateType.FullResized);
			}
		}

		public void UpdateMinMax(Vector2f newMin, Vector2f newMax) {
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

		public Vector2f TrueMax;
		public Vector2f TrueMin;
		public Vector2f BadMin;

		public void StandardMinMaxCalculation() {
			TrueMax = (((ParentRect?.TrueMax ?? Vector2f.One) - (ParentRect?.TrueMin ?? Vector2f.Zero)) * AnchorMax.Value) + (ParentRect?.TrueMin ?? Vector2f.Zero) + (OffsetMax.Value / (Canvas?.scale.Value.Xy ?? Vector2f.One));
			BadMin = (((ParentRect?.BadMin ?? Vector2f.One) - (Vector2f.One - (ParentRect?.TrueMax ?? Vector2f.One))) * (Vector2f.One - AnchorMin.Value)) + (Vector2f.One - (ParentRect?.TrueMax ?? Vector2f.One)) - (OffsetMin.Value / (Canvas?.scale.Value.Xy ?? Vector2f.One));
			TrueMin = Vector2f.One - BadMin;
			var compMin = TrueMin + (OffsetLocalMin.Value / (Canvas?.scale.Value.Xy ?? Vector2f.One));
			var compMax = TrueMax + (OffsetLocalMax.Value / (Canvas?.scale.Value.Xy ?? Vector2f.One));
			UpdateMinMax(compMin + MoveAmount, compMax + MoveAmount);
		}

		public virtual void LocalRectUpdate() {
			ComputeDepth();
			StandardMinMaxCalculation();
		}

		public virtual void ParrentRectUpdate() {
			ComputeDepth();
			StandardMinMaxCalculation();
		}

		public virtual void ChildRectUpdate() {
			CachedOverlapSize = CachedElementSize;
			foreach (Entity item in Entity.children) {
				CachedOverlapSize = MathUtil.Max(CachedOverlapSize, item.UIRect?.CachedElementSize ?? Vector2f.Zero);
			}
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

		public void RenderRect(Matrix matrix) {
			//Todo: Add culling check
			RenderComponents.SafeOperation((renderComs) => {
				foreach (var item in renderComs) {
					if(item.RenderMaterial is null) {
						continue;
					}
					item.mesh?.Draw(item.RenderMaterial, matrix,item.RenderTint,(int)Entity.Depth);
				}
			});
			foreach (Entity item in Entity.children) {
				item?.UIRect?.RenderRect(matrix);
			}
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
