﻿using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI3D/Rects" })]
	public sealed class CuttingUI3DRect : UI3DRect
	{
		[Default(true)]
		[OnChanged(nameof(CuttingUpdate))]
		public readonly Sync<bool> Inherent;

		protected override void CutZoneNotify() {
			CuttingUpdate();
		}

		public void CuttingUpdate() {
			var newMax = CachedMax;
			var newMin = CachedMin;
			if (Inherent) {
				if (CachedCutMax.IsFinite) {
					newMax = MathUtil.Min(CachedCutMax, newMax);
				}
				if(CachedCutMin.IsFinite) {
					newMin = MathUtil.Max(CachedCutMin, newMin);
				 }
			}
			UpdateCuttingZones(newMin, newMax,true);
		}
	}
}