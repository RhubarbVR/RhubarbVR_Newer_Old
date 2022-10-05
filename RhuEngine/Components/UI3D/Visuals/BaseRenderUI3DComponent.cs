using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using System.Collections.Generic;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	public abstract class BaseRenderUI3DComponent : UI3DComponent
	{
		public Vector3f MoveAmount => UIRect.CachedMoveAmount;
		public Vector2f Min => Vector2f.Zero;

		public Vector2f Max => UIRect.CachedElementSize;
		public Vector2f CutMax => UIRect.CachedCutMax;
		public Vector2f CutMin => UIRect.CachedCutMin;
		public bool HasBeenCut => (CutMax != Vector2f.Inf) | (CutMin != Vector2f.NInf);
		protected override void OnLoaded() {
			base.OnLoaded();
			UIRect?.RenderComponents?.SafeAdd(this);
		}
		public void ForceUpdate() {
			UIRect?.MarkForRenderMeshUpdate(UI3DRect.RenderMeshUpdateType.FullResized);
		}
		public abstract void ProcessMeshUpdate();

		public abstract void Render(Matrix pos,int Depth);
	}
}
