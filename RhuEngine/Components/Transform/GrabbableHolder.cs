using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;

namespace RhuEngine.Components
{

	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "Interaction" })]
	public class GrabbableHolder : Component
	{
		public readonly SyncRef<Entity> holder;

		public readonly SyncRef<IWorldObject> Referencer;

		public readonly SyncRef<Entity> RefrencerEntity;

		public readonly SyncRef<User> user;

		public readonly Sync<Handed> source;

		public readonly List<Grabbable> GrabbedObjects = new();
		
		public override void OnAttach() {
			base.OnAttach();
			holder.Target = Entity.AddChild("Holder");
		}

		public void DeleteGrabObjects() {
			foreach (var item in GrabbedObjects) {
				item.DestroyGrabbedObject();
			}
		}

		public bool CanDestroyAnyGabbed
		{
			get {
				if (GrabbedObjects.Count <= 0) {
					return false;
				}
				foreach (var item in GrabbedObjects) {
					if (!item.CanNotDestroy.Value) {
						return true;
					}
				}
				return false;
			}
		}
		public IWorldObject HolderReferen
		{
			get {
				if ((World.HeadGrabbableHolder.Referencer.Target != this) && World.HeadGrabbableHolder.Referencer.Target is not null) {
					if (World.HeadGrabbableHolder.Referencer.Target is not null) {
						return World.HeadGrabbableHolder.Referencer.Target;
					}
				}
				return Referencer.Target;
			}
		}

		public void InitializeGrabHolder(Handed _source) {
			user.Target = LocalUser;
			source.Value = _source;
			switch (_source) {
				case Handed.Left:
					World.LeftGrabbableHolder = this;
					break;
				case Handed.Right:
					World.RightGrabbableHolder = this;
					break;
				case Handed.Max:
					World.HeadGrabbableHolder = this;
					break;
				default:
					break;
			}
		}

		bool _gripping = false;

		public override void Step() {
			base.Step();
			if (!Engine.EngineLink.CanInput) {
				return;
			}
			if (holder.Target == null) {
				return;
			}
			if (user.Target != LocalUser) {
				return;
			}

			var isGrab = source.Value switch {
				Handed.Left => Engine.inputManager.GetInputFloatFromController(Managers.InputManager.InputTypes.Grab, RInput.Controller(Handed.Left), Engine.MainSettings.InputSettings.SecondaryControllerInputSettings) > 0.5,
				Handed.Right => Engine.inputManager.GetInputFloatFromController(Managers.InputManager.InputTypes.Grab, RInput.Controller(Handed.Right), Engine.MainSettings.InputSettings.MainControllerInputSettings) > 0.5,
				Handed.Max => Engine.inputManager.GetInputFloatFromKeyboard(Managers.InputManager.InputTypes.Grab) > 0.5f,
				_ => false,
			};
			if (isGrab && !_gripping) {
				//StartGabbing
				RWorld.ExecuteOnStartOfFrame(() => {
					switch (source.Value) {
						case Handed.Left:
							break;
						case Handed.Right:
							break;
						case Handed.Max:
							WorldManager.PrivateSpaceManager.DisableHeadLaser = true;
							break;
						default:
							break;
					}
				});
				_gripping = true;
			}
			if (_gripping && !isGrab) {
				//DoneGrabbing
				RWorld.ExecuteOnStartOfFrame(() => {
					switch (source.Value) {
						case Handed.Left:
							break;
						case Handed.Right:
							break;
						case Handed.Max:
							WorldManager.PrivateSpaceManager.DisableHeadLaser = false;
							break;
						default:
							break;
					}
				});
				for (var i = 0; i < GrabbedObjects.Count; i++) {
					GrabbedObjects[0].Drop();
				}
				_gripping = false;
			}
		}
	}
}
