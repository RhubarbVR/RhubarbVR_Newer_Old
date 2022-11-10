using Godot;

using RhubarbVR.Bindings.ComponentLinking;

using RhuEngine;
using RhuEngine.Linker;

using System;

public partial class ConnectedViewport : TextureRect
{
	public Viewport targetViewport;

	private RhuEngine.Components.Viewport _viewport;
	public RhuEngine.Components.Viewport Viewport
	{
		get => _viewport;
		set {
			_viewport = value;
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_viewport.WorldLink is ViewportLink viewport) {
					targetViewport = viewport.node;
					Texture = viewport.node?.GetTexture();
				}
			});
		}
	}

	public override void _Ready() {
		base._Ready();
		IgnoreTextureSize = true;
		StretchMode = StretchModeEnum.Keep;
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);
		if (Viewport.IsRemoved || Viewport.IsDestroying) {
			return;
		}
		if (!IsVisibleInTree()) {
			return;
		}

		var xform = GetGlobalTransform();
		var ev = @event.XformedBy(xform.AffineInverse());
		targetViewport?.PushInput(ev);
	}

}
