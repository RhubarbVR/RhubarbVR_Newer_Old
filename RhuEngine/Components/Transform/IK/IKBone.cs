using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using RhuEngine.Physics;
using BEPUik;
using System;

namespace RhuEngine.Components
{
	[Category("Transform/IK")]
	[UpdateLevel(UpdateEnum.Movement)]
	public class IKBone : Component
	{
		[OnChanged(nameof(ReloadBone))]
		public readonly Sync<Vector3f> StartingPosistion;
		[OnChanged(nameof(ReloadBone))]
		public readonly Sync<Quaternionf> StartingRotation;

		public readonly Linker<Vector3f> EntityPosistion;
		public readonly Linker<Quaternionf> EntityRotation;

		public override void Step() {
			base.Step();
			if(LoadedBone is null) {
				return;
			}
			if (MoveMentSpace.Target is null) {
				return;
			}
			var bonePos = new Vector3f(LoadedBone.Position.X, LoadedBone.Position.Y, LoadedBone.Position.Z);
			var boneRot = new Quaternionf(LoadedBone.Orientation.X, LoadedBone.Orientation.Y, LoadedBone.Orientation.Z, LoadedBone.Orientation.W);
			var globalpos = MoveMentSpace.Target.LocalPosToGlobal(bonePos);
			var newLocalPos = Entity.parent.Target?.GlobalPointToLocal(globalpos)?? globalpos;
			var globalrot = MoveMentSpace.Target.LocalRotToGlobal(boneRot);
			var newLocalrot = Entity.parent.Target?.GlobalRotToLocal(globalrot) ?? globalrot;
			if (EntityPosistion.Linked) {
				EntityPosistion.LinkedValue = newLocalPos;
			}
			if (EntityRotation.Linked) {
				EntityRotation.LinkedValue = newLocalrot;
			}
		}

		[OnChanged(nameof(LoadStartingValues))]
		public readonly SyncRef<Entity> MoveMentSpace;
		[OnChanged(nameof(ReloadBone))]
		[Default(.25f)]
		public readonly Sync<float> Radius;
		[OnChanged(nameof(ReloadBone))]
		[Default(1f)]
		public readonly Sync<float> Height;

		[OnChanged(nameof(ReloadBone))]
		[Default(false)]
		public readonly Sync<bool> Pinned;
		private void LoadStartingValues() {
			if(MoveMentSpace.Target is null) {
				return;
			}
			StartingPosistion.Value = MoveMentSpace.Target.GlobalPointToLocal(Entity.GlobalTrans.Translation);
			StartingRotation.Value = MoveMentSpace.Target.GlobalRotToLocal(Entity.GlobalTrans.Rotation);
			EntityPosistion.Target = Entity.position;
			EntityRotation.Target = Entity.rotation;
		}

		public Bone LoadedBone;

		public event Action<Bone> BoneLoaded;

		private void ReloadBone() {
			if(MoveMentSpace.Target is null) {
				LoadedBone = null;
				return;
			}
			var entitySpace = MoveMentSpace.Target;
			if (LoadedBone is null) {
				LoadedBone = new Bone(new BEPUutilities.Vector3(StartingPosistion.Value.x, StartingPosistion.Value.y, StartingPosistion.Value.z), new BEPUutilities.Quaternion(StartingRotation.Value.x, StartingRotation.Value.y, StartingRotation.Value.z, StartingRotation.Value.w), Radius, Height) {
					Pinned = Pinned
				};
				BoneLoaded?.Invoke(LoadedBone);
			}
			else {
				LoadedBone.Position = new BEPUutilities.Vector3(StartingPosistion.Value.x, StartingPosistion.Value.y, StartingPosistion.Value.z);
				LoadedBone.Orientation = new BEPUutilities.Quaternion(StartingRotation.Value.x, StartingRotation.Value.y, StartingRotation.Value.z, StartingRotation.Value.w);
				LoadedBone.Radius = Radius;
				LoadedBone.Height = Height;
				LoadedBone.Pinned = Pinned;
			}
		}

		public override void OnLoaded() {
			base.OnLoaded();
			ReloadBone();
		}
	}
}
