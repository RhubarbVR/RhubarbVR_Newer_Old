using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Interaction\\Sliders" })]
	public class UIHeaderHover : UIComponent
	{
		public SyncDelegate<Action<Handed>> OnHover;
		public SyncDelegate<Action<Handed>> OnHoverLost;

		bool _isHoverd = false;

		public override void RenderUI() {
			var pos = UI.LayoutAt;
			pos -= new Vec3(UI.LayoutRemaining.x/2,0,0);
			if(Helper.IsHovering(FingerId.Index, JointId.Tip,pos,new Vec3((UI.LayoutRemaining.x/2) + (Engine.UISettings.padding * 3),UI.LineHeight*2f,0.2f),out var hand) == _isHoverd) {
				if (_isHoverd) {
					AddWorldCoroutine(()=>
					OnHover.Target?.Invoke(hand));
				}
				else {
					AddWorldCoroutine(()=>
					OnHoverLost.Target?.Invoke(hand));
				}
				_isHoverd = !_isHoverd;
			}
		}
	}
}
