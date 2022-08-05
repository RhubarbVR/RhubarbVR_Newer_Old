using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI/Rects" })]
	public class CuttingUIRect : UIRect
	{
		[Default(true)]
		[OnChanged(nameof(CuttingUpdate))]
		public readonly Sync<bool> Inherent;

		protected override void OnMarkedForRenderMeshUpdate(RenderMeshUpdateType renderMeshUpdateType) {
			if (renderMeshUpdateType == RenderMeshUpdateType.BindAndCanvasScale) {
				return;
			}
			CuttingUpdate();
		}

		public override void OnLoaded() {
			base.OnLoaded();
			CuttingUpdate();
		}

		public void CuttingUpdate() {
			var newMax = CachedMax;
			var newMin = CachedMin;
			if (Inherent) {
				newMax = MathUtil.Max(CachedCutMin,newMax);
				newMin = MathUtil.Min(CachedCutMin,newMin);
			}
			UpdateCuttingZones(newMin, newMax);
		}
	}
}
