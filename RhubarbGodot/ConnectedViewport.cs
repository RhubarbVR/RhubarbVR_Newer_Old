using Godot;

using RhubarbVR.Bindings.ComponentLinking;

using RhuEngine;
using RhuEngine.Linker;

using System;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "<Pending>")]
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
					if(_viewport is null) {
						return;
					}
					if (_viewport.WorldLink is ViewportLink viewport) {
						targetViewport = viewport.node;
						Texture = viewport.node?.GetTexture();
					}
					else {
						RenderThread.ExecuteOnStartOfFrame(() => {
							if (_viewport.WorldLink is ViewportLink viewport) {
								targetViewport = viewport.node;
								Texture = viewport.node?.GetTexture();
							}
						});
					}
				}
				catch {
				}
			});
		}
	}

	public override void _Ready() {
		base._Ready();
		ExpandMode = ExpandModeEnum.IgnoreSize;
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
		if(Viewport is null) {
			return;
		}
		if (Viewport.IsRemoved || Viewport.IsDestroying) {
			return;
		}
		if(@event is InputEventKey) {
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
