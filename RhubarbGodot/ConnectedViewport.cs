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
				try {
					if (_viewport.WorldLink is ViewportLink viewport) {
						targetViewport = viewport.node;
						Texture = viewport.node?.GetTexture();
					}
				}
				catch {
				}
			});
		}
	}

	public override void _Ready() {
		base._Ready();
		IgnoreTextureSize = true;
		StretchMode = StretchModeEnum.Keep;
		MouseEntered += ConnectedViewport_MouseEntered;
		MouseExited += ConnectedViewport_MouseExited;
	}
	bool _hover = false;

	private void ConnectedViewport_MouseExited() {
		_hover = false;
	}

	private void ConnectedViewport_MouseEntered() {
		_hover = true;
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);
		if (Viewport.IsRemoved || Viewport.IsDestroying) {
			return;
		}
		if (!IsVisibleInTree()) {
			return;
		}
		if (!_hover) {
			return;
		}
		var xform = GetGlobalTransform();
		var ev = @event.XformedBy(xform.AffineInverse());
		targetViewport?.PushInput(ev);
	}

}
