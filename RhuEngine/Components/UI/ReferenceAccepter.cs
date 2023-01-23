using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using RhuEngine.Input.XRInput;

namespace RhuEngine.Components.UI
{
	public interface IReferenceAccepter : IComponent
	{

	}

	[Category(new string[] { "UI" })]
	public sealed class ReferenceAccepter<T> : Component, IReferenceAccepter where T : class, IWorldObject
	{
		[OnChanged(nameof(PrivateSpaceManager_OnUpdateHolderReferen))]
		public readonly Linker<bool> DropVisual;

		[OnChanged(nameof(PrivateSpaceManager_OnUpdateHolderReferen))]
		[Default(true)]
		public readonly Sync<bool> AllowGrabbedObjects;

		[OnChanged(nameof(PrivateSpaceManager_OnUpdateHolderReferen))]
		[Default(true)]
		public readonly Sync<bool> WithComps;

		[OnChanged(nameof(PrivateSpaceManager_OnUpdateHolderReferen))]
		[Default(true)]
		public readonly Sync<bool> TryAndFind;

		[OnChanged(nameof(TargetButtonUpdate))]
		public readonly SyncRef<ButtonBase> targetButton;

		public readonly SyncDelegate<Action<T>> Dropped;

		protected override void OnAttach() {
			base.OnAttach();
			var main = targetButton.Target = Entity.AddChild("DropVisual").AttachComponent<Button>();
			main.ModulateSelf.Value = new Colorf(98, 98, 98);
			main.MinOffset.Value = new Vector2f(-10, 10);
			main.MaxOffset.Value = new Vector2f(10, -10);
			main.ButtonMask.Value = RButtonMask.Primary | RButtonMask.Secondary;
			main.ButtonUp.Target = OnButtonUp;
			var Label = main.Entity.AddChild("DropIcon").AttachComponent<TextureRect>();
			Label.ExpandedMode.Value = RExpandedMode.IgnoreSize;
			Label.StrechMode.Value = RStrechMode.KeepAspectCenter;
			var texture = main.Entity.AttachComponent<SingleIconTex>();
			texture.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.Plus;
			Label.Texture.Target = texture;
			Label.InputFilter.Value = RInputFilter.Ignore;
			DropVisual.Target = main.Entity.enabled;
		}

		private ButtonBase _lastButton;

		private void TargetButtonUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_lastButton is not null) {
					_lastButton.InputEntered -= LastButton_InputEntered;
					_lastButton.InputExited -= LastButton_InputExited;
				}
				_lastButton = targetButton.Target;
				_isOver = false;
				if (_lastButton is not null) {
					_lastButton.InputEntered += LastButton_InputEntered;
					_lastButton.InputExited += LastButton_InputExited;
				}
			});
		}

		bool _isOver;

		private void LastButton_InputExited() {
			_isOver = false;
		}

		private void LastButton_InputEntered() {
			_isOver = true;
		}

		private T _valueCache;

		[Exposed]
		public void OnButtonUp() {
			_isOver = false;
			if (_valueCache is null) {
				RLog.Info("NoRefToDrop");
				return;
			}
			RLog.Info("RefDrop:" + _valueCache.Pointer.ToString());
			Dropped.Target?.Invoke(_valueCache);
		}

		public T GetDropedObject(IWorldObject targetObject) {
			return targetObject is null
				? null
				: targetObject.World != World && !World.IsPersonalSpace
				? null
				: TryAndFind.Value
				? WithComps.Value ? targetObject.GetClosedGenericWithComps<T>() : targetObject.GetClosedGeneric<T>()
				: targetObject.Get<T>();
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			PrivateSpaceManager.OnUpdateHolderReferen += PrivateSpaceManager_OnUpdateHolderReferen;
			PrivateSpaceManager.OnDrop += PrivateSpaceManager_OnDrop;
		}

		private void PrivateSpaceManager_OnDrop() {
			if (!_isOver) {
				return;
			}
			OnButtonUp();
		}

		public override void Dispose() {
			PrivateSpaceManager.OnUpdateHolderReferen -= PrivateSpaceManager_OnUpdateHolderReferen;
			base.Dispose();
		}

		private void PrivateSpaceManager_OnUpdateHolderReferen() {
			if (DropVisual.Linked) {
				var targetObject = AllowGrabbedObjects ? PrivateSpaceManager.GetHolderRefGrabbable : PrivateSpaceManager.GetHolderRef;
				if (targetObject is null) {
					DropVisual.LinkedValue = false;
					return;
				}
				if (targetObject.World != World && !World.IsPersonalSpace) {
					return;
				}
				_valueCache = GetDropedObject(targetObject);
				DropVisual.LinkedValue = _valueCache is not null;
			}
		}
	}
}
