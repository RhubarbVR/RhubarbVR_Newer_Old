using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	public class UIComponent: RenderingComponent
	{
		public override void AddListObject() {
			World.RegisterRenderObject(this);
		}
		public override void RemoveListObject() {
			World.UnregisterRenderObject(this);
		}

		public override void Render() {

		}
	}
}
