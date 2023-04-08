using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{

	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "Interaction" })]
	public sealed partial class GrabbableHolder : Component
	{
		public readonly Linker<Vector3f> LazerScaleLinker;
		public readonly Linker<Vector3f> LazerPosLinker;
		public readonly Linker<Quaternionf> LazerRotLinker;
		public readonly SyncRef<ILinkerMember<Vector3f>> LazerScale;
		public readonly SyncRef<ILinkerMember<Vector3f>> LazerPos;
		public readonly SyncRef<ILinkerMember<Quaternionf>> LazerRot;
		public readonly SyncRef<Entity> LazerEntity;

		public void AddLazerEntity(Entity entity) {
			if (entity is null) {
				LazerScale.Target = null;
				LazerPos.Target = null;
				LazerRot.Target = null;
				LazerScaleLinker.Target = null;
				LazerPosLinker.Target = null;
				LazerRotLinker.Target = null;
				return;
			}
			LazerEntity.Target = entity;
			LazerScaleLinker.Target = entity.scale;
			LazerScale.Target = entity.scale;
			LazerPosLinker.Target = entity.position;
			LazerPos.Target = entity.position;
			LazerRotLinker.Target = entity.rotation;
			LazerRot.Target = entity.rotation;
		}

		public readonly SyncRef<Entity> holder;

		[OnChanged(nameof(UpdateReferencer))]
		public readonly SyncRef<IWorldObject> Referencer;

		public readonly SyncRef<Entity> RefrencerEntity;

		public readonly SyncRef<User> user;

		public readonly Sync<Handed> source;

		public readonly List<Grabbable> GrabbedObjects = new();

		internal void UpdateReferencer() {
			try {
				PrivateSpaceManager.GetGrabbableHolder(source.Value).UpdateHolderReferen();
			}
			catch { }
		}



		protected override void OnAttach() {
			base.OnAttach();
			holder.Target = Entity.AddChild("Holder");
		}

		public void DeleteGrabObjects() {
			foreach (var item in GrabbedObjects) {
				item.DestroyGrabbedObject();
			}
		}
		public bool IsAnyLaserGrabbed
		{
			get {
				if (GrabbedObjects.Count <= 0) {
					return false;
				}
				foreach (var item in GrabbedObjects) {
					if (item.LaserGrabbed) {
						return true;
					}
				}
				return false;
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
				return Referencer.Target is not null
					? Referencer.Target
					: World.HeadGrabbableHolder?.Referencer.Target;
			}
		}

		public void InitializeGrabHolder(Handed _source) {
			user.Target = LocalUser;
			source.Value = _source;
			if (_source != Handed.Max) {
				var shape = Entity.AttachComponent<SphereShape>();
				shape.RayCastEnabled.Value = false;
				shape.Radius.Value = 0.05f;
			}
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

		public bool gripping = false;
		public bool grippingLastFrame = false;

		protected override void RenderStep() {
			if (user.Target is null) {
				return;
			}
			if (user.Target != LocalUser) {
				var nonescaleStream = user.Target.FindSyncStream<SyncValueStream<Vector3f>>($"{Pointer}.scale");
				var noneposStream = user.Target.FindSyncStream<SyncValueStream<Vector3f>>($"{Pointer}.pos");
				var nonerotStream = user.Target.FindSyncStream<SyncValueStream<Quaternionf>>($"{Pointer}.rot");
				if ((LazerScale.Target?.IsLinkedTo ?? false) & nonescaleStream is not null) {
					LazerScale.Target.Value = nonescaleStream.Value;
				}
				if ((LazerPos.Target?.IsLinkedTo ?? false) & noneposStream is not null) {
					LazerPos.Target.Value = noneposStream.Value;
				}
				if ((LazerRot.Target?.IsLinkedTo ?? false) & nonerotStream is not null) {
					LazerRot.Target.Value = nonerotStream.Value;
				}
				return;
			}
			if (!Engine.EngineLink.CanInput) {
				return;
			}
			if (holder.Target == null) {
				return;
			}
			if (source.Value == Handed.Max) {
				if (Engine.IsInVR) {
					return;
				}
			}
			if (source.Value == Handed.Left && World.LeftGrabbableHolder != this) {
				return;
			}
			else if (source.Value == Handed.Right && World.RightGrabbableHolder != this) {
				return;
			}
			else if (source.Value == Handed.Max && World.HeadGrabbableHolder != this) {
				return;
			}
			else if (source.Value > Handed.Max) {
				return;
			}

			var grabForce = InputManager.Grab.HandedValue(source.Value);
			var isGrab = grabForce > 0.6;
			grippingLastFrame = gripping;
			if (isGrab && !gripping) {
				//foreach (var item in _overLappingObjects) {
				//	if (item.CustomObject is PhysicsObject physicsObject) {
				//		physicsObject.Entity.CallOnGrip(this, false, grabForce);
				//		if (physicsObject.Entity == Entity) {
				//			RLog.Info("Grabing self");
				//		}
				//		RLog.Info("Grip " + physicsObject.Entity.name.Value);
				//	}
				//}
				gripping = true;
				UpdateReferencer();
				if (World.IsPersonalSpace) {
					PrivateSpaceManager.HolderGrip();
				}
			}
			if (isGrab && gripping) {
				foreach (var item in GrabbedObjects) {
					item.UpdateGrabbedObject();
				}
			}
			if (gripping && !isGrab) {
				if (World.IsPersonalSpace) {
					PrivateSpaceManager.HolderDrop();
				}
				//DoneGrabbing
				for (var i = GrabbedObjects.Count - 1; i >= 0; i--) {
					try {
						GrabbedObjects[i]?.Drop();
					}
					catch { }
				}
				gripping = false;
				PrivateSpaceManager.GetLazer(source.Value).Locked.Value = false;
				Referencer.Target = null;
				if(World.HeadGrabbableHolder == this) {
					World.RightGrabbableHolder.Referencer.Target = null;
					World.LeftGrabbableHolder.Referencer.Target = null;
				}
				UpdateReferencer();
			}
			//_overLappingObjects.Clear();
			var scaleStream = LocalUser.FindOrCreateSyncStream<SyncValueStream<Vector3f>>($"{Pointer}.scale");
			var posStream = LocalUser.FindOrCreateSyncStream<SyncValueStream<Vector3f>>($"{Pointer}.pos");
			var rotStream = LocalUser.FindOrCreateSyncStream<SyncValueStream<Quaternionf>>($"{Pointer}.rot");
			if ((LazerScale.Target?.IsLinkedTo ?? false) & scaleStream is not null) {
				scaleStream.Value = LazerScale.Target.Value;
			}
			if ((LazerPos.Target?.IsLinkedTo ?? false) & posStream is not null) {
				posStream.Value = LazerPos.Target.Value;
			}
			if ((LazerRot.Target?.IsLinkedTo ?? false) & rotStream is not null) {
				rotStream.Value = LazerRot.Target.Value;
			}
			return;
		}

		public void DropObject(Grabbable grabbable) {
			if (grabbable.LaserGrabbed) {
				AddLazerEntity(null);
				grabbable.Entity.GlobalTrans = grabbable.Entity.GlobalTrans;
			}
			GrabbedObjects.Remove(grabbable);
		}
	}
}
