using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI3D/Rects" })]
	public class UI3DRect : Component
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
			ForeParrentUpdate = 8,
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


		public IEnumerable<UI3DCanvas.HitData> GetRectHitData() {
			return CachedCanvas?.HitDataInVolume(CachedMin, CachedMax);
		}

		public readonly SafeList<BaseRenderUI3DComponent> RenderComponents = new();

		public float AddedDepth { get; private set; }
		public UI3DCanvas CachedCanvas { get; private set; }
		public UI3DCanvas Canvas => CachedCanvas;

		/// <summary>
		/// Should only be ran on the GameThread
		/// </summary>
		public void AddAddedDepth(float addedDepth) {
			if (AddedDepth == addedDepth) {
				return;
			}
			AddedDepth = addedDepth;
			foreach (Entity item in Entity.children) {
				item.UIRect?.AddAddedDepth(addedDepth);
			}
			MarkForRenderMeshUpdate(RenderMeshUpdateType.Movment);
		}

		public void CanvasUpdate() {
			if (CachedCanvas is not null) {
				OnRectUpdate -= CachedCanvas.RectUpdate;
			}
			CachedCanvas = Entity.GetFirstComponent<UI3DCanvas>();
			if(CachedCanvas is not null) {
				OnRectUpdate += CachedCanvas.RectUpdate;
			}
			CachedCanvas ??= Entity.parent.Target?.UIRect?.CachedCanvas;
			if (Entity.parent.Target?.GetFirstComponent<UI3DCanvas>() != null) {
				if (CachedCanvas is not null) {
					OnRectUpdate += CachedCanvas.RectUpdate;
				}
			}
			foreach (Entity item in Entity.children) {
				item.UIRect?.CanvasUpdate();
			}
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			RenderComponents.SafeAddRange(Entity.GetAllComponents<BaseRenderUI3DComponent>());
			CanvasUpdate();
			CachedCutMin = ParentRect?.CachedCutMin ?? Vector2f.NInf;
			CachedCutMax = ParentRect?.CachedCutMax ?? Vector2f.Inf;

		}

		public void RegisterNestedParentUpdate(bool doSelf = true) {
			if((Update & UpdateType.ForeParrentUpdate) != UpdateType.None) {
				return;
			}
			foreach (Entity item in Entity.children) {
				item.UIRect?.RegisterNestedParentUpdate();
			}
			if (doSelf) {
				RegesterRectUpdate(UpdateType.ForeParrentUpdate);
			}
		}

		public UpdateType Update { get; private set; }
		public RenderMeshUpdateType RenderMeshUpdate { get; private set; }

		internal void MarkRenderMeshUpdateAsDone() {
			RenderMeshUpdate = RenderMeshUpdateType.None;
		}


		public Vector2f CachedCutMin { get; private set; }
		public Vector2f CachedCutMax { get; private set; }
		public bool cutsAreDirty;

		public void UpdateCuttingZones(Vector2f NewMin, Vector2f NewMax,bool firstValue = false) {
			var updatedValues = (CachedCutMin != NewMin) | (CachedCutMax != NewMax);
			if (!firstValue) {
				CachedCutMin = NewMin;
				CachedCutMax = NewMax;
			}
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
		public Vector2f CachedOverlapSize { get; protected set; }
		public Vector2f TotalMove { get; private set; }
		public Vector2f MoveAmount { get; private set; }

		public void ApplyMovement(Vector2f moveAmount) {
			MoveAmount = moveAmount;
			RegisterRectUpdateEvent();
		}

		public UI3DRect ParentRect => Entity.parent.Target?.UIRect;

		protected virtual void CutZoneNotify() {

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
			var hasMoved = (CachedMin != newMin) | (CachedMax != newMax) | cutsAreDirty;
			cutsAreDirty = false;
			CachedMin = newMin;
			CachedMax = newMax;
			var sizeUpdate = CachedElementSize != newSize;
			CachedElementSize = newSize;
			if (sizeUpdate) {
				MarkForRenderMeshUpdate(RenderMeshUpdateType.FullResized);
				CutZoneNotify();
				return;
			}
			if (hasMoved) {
				MarkForRenderMeshUpdate(RenderMeshUpdateType.Movment);
				CutZoneNotify();
				return;
			}
		}

		public Vector2f TrueMax;
		public Vector2f TrueMin;
		public Vector2f BadMin;

		public void StandardMinMaxCalculation(Vector2f ParentRectTrueMax, Vector2f ParentRectTrueMin, Vector2f ParentRectBadMin) {
			TrueMax = ((ParentRectTrueMax - ParentRectTrueMin) * AnchorMax.Value) + ParentRectTrueMin + (OffsetMax.Value / (Canvas?.scale.Value.Xy ?? Vector2f.One));
			BadMin = ((ParentRectBadMin - (Vector2f.One - ParentRectTrueMax)) * (Vector2f.One - AnchorMin.Value)) + (Vector2f.One - ParentRectTrueMax) - (OffsetMin.Value / (Canvas?.scale.Value.Xy ?? Vector2f.One));
			TrueMin = Vector2f.One - BadMin;
			var compMin = TrueMin + (OffsetLocalMin.Value / (Canvas?.scale.Value.Xy ?? Vector2f.One));
			var compMax = TrueMax + (OffsetLocalMax.Value / (Canvas?.scale.Value.Xy ?? Vector2f.One));
			UpdateMinMax(compMin + TotalMove, compMax + TotalMove);
		}

		public void StandardMinMaxCalculation() {
			StandardMinMaxCalculation(ParentRect?.TrueMax ?? Vector2f.One, ParentRect?.TrueMin ?? Vector2f.Zero, ParentRect?.BadMin ?? Vector2f.One);
		}

		public event Action OnRectUpdate;

		public virtual void LocalRectUpdate() {
			TotalMove = (ParentRect?.TotalMove??Vector2f.Zero) + MoveAmount;
			ComputeDepth();
			StandardMinMaxCalculation();
			OnRectUpdate?.Invoke();
		}

		public virtual void ParrentRectUpdate() {
			TotalMove = (ParentRect?.TotalMove ?? Vector2f.Zero) + MoveAmount;
			ComputeDepth();
			StandardMinMaxCalculation();
		}

		public virtual void FowParrentRectUpdate() {
			ParrentRectUpdate();
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

		protected override void AddListObject() {
			Engine.uiManager.AddRectComponent(this);
		}

		protected override void RemoveListObject() {
			Engine.uiManager.RemoveRectComponent(this);
		}

		public void RenderRect(Matrix matrix) {
			//Todo: Add culling check
			if (!Entity.IsEnabled) {
				return;
			}
			RenderComponents.SafeOperation((renderComs) => {
				foreach (var item in renderComs) {
					item.Render(matrix, (int)Entity.Depth - (int)(CachedCanvas.Entity).Depth);
				}
			});
			foreach (Entity item in Entity.children) {
				item?.UIRect?.RenderRect(matrix);
			}
		}

		protected override void OnAttach() {
			base.OnAttach();
			AnchorMin.Value = Vector2f.Zero;
			AnchorMax.Value = Vector2f.One;
			OffsetMin.Value = Vector2f.Zero;
			OffsetMax.Value = Vector2f.Zero;
		}

	}
}
