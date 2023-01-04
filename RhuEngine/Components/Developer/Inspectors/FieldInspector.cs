using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace RhuEngine.Components
{
	public interface IFiledInit : IComponent
	{
		public void InitField(MemberInfo memberInfo, IWorldObject targetValue);
	}

	[Category(new string[] { "Developer/Inspectors" })]
	public sealed class FieldInspector<T> : Component, IFiledInit where T : Component, IInspector, new()
	{
		public void InitField(MemberInfo memberInfo, IWorldObject targetValue) {
			var box = Entity.AttachComponent<BoxContainer>();
			box.Vertical.Value = false;
			box.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

			var text = Entity.AddChild("FiledName").AttachComponent<Button>();
			text.TextOverrunBehavior.Value = ROverrunBehavior.TrimEllipsis;
			text.ButtonMask.Value = RButtonMask.Secondary;
			text.FocusMode.Value = RFocusMode.None;
			text.MinSize.Value = new Vector2i(18, 18);
			text.Text.Value = memberInfo.Name;
			text.Alignment.Value = RButtonAlignment.Right;
			text.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			Entity.AddChild("Filed").AttachComponent<T>().TargetObjectWorld = targetValue;
		}
	}
}