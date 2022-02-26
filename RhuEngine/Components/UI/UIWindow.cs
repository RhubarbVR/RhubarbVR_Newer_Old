using System.Collections.Generic;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	public class UIWindow : RenderingComponent
	{
		public Sync<string> Text;

		public Sync<Vec2> Size;
		[Default(UIWin.Normal)]
		public Sync<UIWin> WindowType;
		[Default(UIMove.FaceUser)]
		public Sync<UIMove> MoveType;

		public override void AddListObject() {
			World.RegisterRenderObject(this);
		}
		public override void RemoveListObject() {
			World.UnregisterRenderObject(this);
		}

		public override void Render() {
			var pose = Pose.Identity;
			UI.PushId(Pointer.GetHashCode());
			Hierarchy.Push(Entity.GlobalTrans);
			if(Size.Value.v == Vec2.Zero.v) {
				UI.WindowBegin(Text.Value ?? "", ref pose, WindowType, MoveType);
				foreach (Entity childEntity in Entity.children) {
					foreach (var item in childEntity.components) {
						if (item is UIComponent comp) {
							if (comp.Enabled) {
								comp.RenderUI();
							}
						}
					}
				}
				UI.WindowEnd();
			}
			else {
				UI.WindowBegin(Text.Value ?? "", ref pose,Size, WindowType, MoveType);
				foreach (Entity childEntity in Entity.children) {
					foreach (var item in childEntity.components) {
						if (item is UIComponent comp) {
							comp.RenderUI();
						}
					}
				}
				UI.WindowEnd();
			}
			Hierarchy.Pop();
			UI.PopId();
			if(!((pose.position.v == Pose.Identity.position.v) && (pose.orientation.q == Pose.Identity.orientation.q))) {
				Entity.LocalTrans = pose.ToMatrix() * Entity.LocalTrans;
			}
		}

	}
}
