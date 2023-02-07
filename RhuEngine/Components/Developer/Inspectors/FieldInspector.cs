using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace RhuEngine.Components
{
	public interface IFiledInit : IComponent
	{
		public void InitField(MemberInfo memberInfo, IWorldObject targetValue);
	}

	[Category(new string[] { "Developer/Inspectors" })]
	public sealed class FieldInspector<T> : Component, IFiledInit where T : Component, IInspector, new()
	{
		[OnChanged(nameof(TargetLoad))]
		public readonly SyncRef<IWorldObject> TargetObject;
		public readonly SyncRef<Button> TargetButton;
		[OnChanged(nameof(TargetLoad))]
		public readonly Linker<Colorf> DetailColor;

		private ILinkable _lastObject;

		public void TargetLoad() {
			if (_lastObject == TargetObject.Target) {
				OnLinked(null);
				return;
			}
			if (_lastObject is not null) {
				_lastObject.OnLinked -= OnLinked;
				_lastObject = null;
			}
			if (TargetObject.Target is ILinkable linkable) {
				_lastObject = linkable;
				_lastObject.OnLinked += OnLinked;
				OnLinked(null);
			}
		}

		private void OnLinked(ILinker obj) {
			if (DetailColor.Linked) {
				DetailColor.LinkedValue = _lastObject.IsLinkedTo ? Colorf.Magenta : Colorf.White;
			}
		}

		[Exposed]
		public void GetRef() {
			if (TargetButton.Target is not null) {
				try {
					PrivateSpaceManager.GetGrabbableHolder(TargetButton.Target.LastHanded).GetGrabbableHolderFromWorld(World).Referencer.Target = TargetObject.Target;
				}
				catch {
				}
			}
		}

		public void InitField(MemberInfo memberInfo, IWorldObject targetValue) {
			TargetObject.Target = targetValue;
			var box = Entity.AttachComponent<BoxContainer>();
			box.Vertical.Value = false;
			box.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

			var text = Entity.AddChild("FiledName").AttachComponent<Button>();
			TargetButton.Target = text;
			text.ButtonDown.Target = GetRef;
			text.TextOverrunBehavior.Value = ROverrunBehavior.TrimEllipsis;
			text.ButtonMask.Value = RButtonMask.Secondary;
			text.FocusMode.Value = RFocusMode.None;
			text.MinSize.Value = new Vector2i(18, 18);
			text.Text.Value = memberInfo.Name;
			text.Alignment.Value = RButtonAlignment.Right;
			text.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			Entity.AddChild("Filed").AttachComponent<T>().TargetObjectWorld = targetValue;
			DetailColor.Target = box.Modulate;
		}
	}
}