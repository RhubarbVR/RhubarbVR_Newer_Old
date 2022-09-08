﻿using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System;
using System.Runtime.CompilerServices;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Observer/Observers/SyncElements" })]
	public class BoolSyncObserver : ValueObserverBase<bool>
	{
		public readonly SyncRef<CheckBox> CheckBox;

		protected override void BuildUI(UIBuilder ui) {
			ui.PushRectNoDepth(null, new Vector2f(0, 1f));
			ui.SetOffsetMinMax(null, new Vector2f(ELMENTHIGHTSIZE, 0));
			ui.PushRectNoDepth(new Vector2f(0.1f, 0f), new Vector2f(0.9f, 1f));
			var iconMit = Entity.AttachComponent<UnlitMaterial>();
			iconMit.DullSided.Value = true;
			iconMit.Transparency.Value = Transparency.Blend;
			iconMit.MainTexture.Target = World.RootEntity.GetFirstComponentOrAttach<IconsTex>();
			var sprite = World.RootEntity.GetFirstComponentOrAttach<SpriteProvder>();
			sprite.Texture.Target = iconMit.MainTexture.Target;
			sprite.GridSize.Value = new Vector2i(26, 7);
			CheckBox.Target = ui.AddGenaricCheckBox(iconMit, sprite);
			ValueChanged();
			CheckBox.Target.StateChange.Target = ValueUpdate;
			ui.PopRect();
			ui.PopRect();
		}
		[Exposed]
		protected void ValueUpdate(bool data) {
			if(TargetElement is null) {
				return;
			}
			TargetElement.Value = data;
		}

		protected override void ValueChanged() {
			if (CheckBox.Target is null) {
				return;
			}
			CheckBox.Target.Open.Value = TargetElement.Value;
		}
	}
}