// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Numerics;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RNumerics.IK;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Transform/IK" })]
	public sealed partial class HumanoidIK : Component
	{
		public abstract partial class IKBone : SyncObject, IIKBoneTransform
		{
			[OnChanged(nameof(LocalBoneUpdate))]
			public readonly SyncRef<Entity> TargetBone;
			public readonly Linker<Vector3f> BonePos;
			public readonly Linker<Quaternionf> BoneRot;
			public readonly Linker<Vector3f> BoneScale;
			public void LocalBoneUpdate() {
				var targetEntity = TargetBone.Target;
				BoneScale.Target = null;
				BonePos.Target = null;
				BoneRot.Target = null;
				if (targetEntity is not null) {
					BonePos.Target = targetEntity.position;
					BoneRot.Target = targetEntity.rotation;
					BoneScale.Target = targetEntity.scale;
				}
				BoneUpdate();
			}

			public void BoneUpdate() {
				ParrentBone?.BoneUpdate();
				if (Parent is HumanoidIK bome) {
					bome.BoneUpdate();
				}
			}

			public bool IsVailed => TargetBone.Target is not null && BonePos.Target == TargetBone.Target.position && BoneRot.Target == TargetBone.Target.rotation && BoneScale.Target == TargetBone.Target.scale;

			public ITransform TargetEntity => TargetBone.Target;

			public IKBone ParrentBone => Parent is IKBone bome ? bome : null;

			IIKBoneTransform IIKBoneTransform.parent => !IsVailed ? null : (IIKBoneTransform)ParrentBone;

			public Vector3f position
			{
				get => !IsVailed ? Vector3f.Zero : TargetEntity.position;
				set {
					if (!IsVailed) {
						return;
					}
					TargetEntity.position = value;
				}
			}

			public Vector3f right => !IsVailed ? Vector3f.Zero : TargetEntity.right;

			public Vector3f up => !IsVailed ? Vector3f.Zero : TargetEntity.up;

			public Vector3f forward => !IsVailed ? Vector3f.Zero : TargetEntity.forward;

			public Vector3f localPosition => !IsVailed ? Vector3f.Zero : TargetEntity.localPosition;

			public Vector3f localScale
			{
				get => !IsVailed ? Vector3f.One : TargetEntity.localScale;
				set {
					if (!IsVailed) {
						return;
					}
					TargetEntity.localScale = value;
				}
			}
			public Quaternionf rotation
			{
				get => !IsVailed ? Quaternionf.Identity : TargetEntity.rotation;
				set {
					if (!IsVailed) {
						return;
					}
					TargetEntity.rotation = value;
				}
			}
			public Quaternionf localRotation
			{
				get => !IsVailed ? Quaternionf.Identity : TargetEntity.localRotation;
				set {
					if (!IsVailed) {
						return;
					}
					TargetEntity.localRotation = value;
				}
			}

			public ITransform AddChild(string v) {
				throw new System.NotImplementedException();
			}
		}

		public sealed partial class IKHips : IKBone
		{
			public sealed partial class IKSpine : IKBone
			{
				public sealed partial class IKSpine2 : IKBone
				{
					public sealed partial class IKSpine3 : IKBone
					{
						public sealed partial class IKSpine4 : IKBone
						{
							public sealed partial class IKNeck : IKBone
							{
								public sealed partial class IKHead : IKBone
								{
									public sealed partial class IKEye : IKBone
									{
									}

									public readonly IKEye LeftEye;
									public readonly IKEye RightEye;
								}

								public readonly IKHead Head;
							}

							public readonly IKNeck Neck;



							public sealed partial class IKSholder : IKBone
							{
								public sealed partial class IKArm : IKBone
								{
									public sealed partial class IKElbow : IKBone
									{
										public sealed partial class IKWrist : IKBone
										{
											public sealed partial class IKFingerWhole : IKBone
											{
												public sealed partial class IKFinger : IKBone
												{ }

												public readonly IKFinger Start;
												public readonly IKFinger Joint1;
												public readonly IKFinger Joint2;
												public readonly IKFinger Tip;
												public IIKBoneTransform[] GetFinngers() {
													return new IIKBoneTransform[] {
														Start,
														Joint1,
														Joint2,
														Tip
													};
												}
											}

											public readonly IKFingerWhole Thumb;
											public readonly IKFingerWhole Index;
											public readonly IKFingerWhole Middle;
											public readonly IKFingerWhole Ring;
											public readonly IKFingerWhole Pinky;



											public IIKBoneTransform[,] GetFinngers() {

												var jaggedArray = new IIKBoneTransform[][] {
													Thumb.GetFinngers(),
													Index.GetFinngers(),
													Middle.GetFinngers(),
													Ring.GetFinngers(),
													Pinky.GetFinngers(),
												};

												// Convert the jagged array to a multidimensional array
												var rowCount = jaggedArray.Length;
												var colCount = jaggedArray[0].Length;
												var multiDimensionalArray = new IIKBoneTransform[rowCount, colCount];

												for (var row = 0; row < rowCount; row++) {
													for (var col = 0; col < colCount; col++) {
														multiDimensionalArray[row, col] = jaggedArray[row][col];
													}
												}

												return multiDimensionalArray;
											}
										}

										public readonly IKWrist Wrist;
									}

									public readonly IKElbow Elbow;
								}

								public readonly IKArm Arm;

							}

							public readonly IKSholder LeftShoulder;
							public readonly IKSholder RightShoulder;

							public sealed partial class IKUpperLeg : IKBone
							{
								public sealed partial class IKKnee : IKBone
								{
									public sealed partial class IKFoot : IKBone
									{
									}
									public readonly IKFoot Foot;
								}
								public readonly IKKnee Knee;
							}

							public readonly IKUpperLeg LeftLeg;
							public readonly IKUpperLeg RightLeg;
						}

						public readonly IKSpine4 Spine4;
					}

					public readonly IKSpine3 Spine3;
				}

				public readonly IKSpine2 Spine2;
			}

			public readonly IKSpine Spine;
		}


		public readonly IKHips Hips;

		public FullBodyIK FullBodyIK { get; set; } = new();
		private void BoneUpdate() {
			RenderThread.ExecuteOnEndOfFrame(this, () => {
				FullBodyIK.ConfigureBoneIIKTransforms(
					Hips,
					Hips.Spine,
					Hips.Spine.Spine2,
					Hips.Spine.Spine2.Spine3,
					Hips.Spine.Spine2.Spine3.Spine4,
					Hips.Spine.Spine2.Spine3.Spine4.Neck,
					Hips.Spine.Spine2.Spine3.Spine4.Neck.Head,
					Hips.Spine.Spine2.Spine3.Spine4.Neck.Head.LeftEye,
					Hips.Spine.Spine2.Spine3.Spine4.Neck.Head.RightEye,
					Hips.Spine.Spine2.Spine3.Spine4.LeftLeg,
					Hips.Spine.Spine2.Spine3.Spine4.RightLeg,
					Hips.Spine.Spine2.Spine3.Spine4.LeftLeg.Knee,
					Hips.Spine.Spine2.Spine3.Spine4.RightLeg.Knee,
					Hips.Spine.Spine2.Spine3.Spine4.LeftLeg.Knee.Foot,
					Hips.Spine.Spine2.Spine3.Spine4.RightLeg.Knee.Foot,
					Hips.Spine.Spine2.Spine3.Spine4.LeftShoulder,
					Hips.Spine.Spine2.Spine3.Spine4.RightShoulder,
					Hips.Spine.Spine2.Spine3.Spine4.LeftShoulder.Arm,
					Hips.Spine.Spine2.Spine3.Spine4.RightShoulder.Arm,
					Hips.Spine.Spine2.Spine3.Spine4.LeftShoulder.Arm.Elbow,
					Hips.Spine.Spine2.Spine3.Spine4.RightShoulder.Arm.Elbow,
					Hips.Spine.Spine2.Spine3.Spine4.LeftShoulder.Arm.Elbow.Wrist,
					Hips.Spine.Spine2.Spine3.Spine4.RightShoulder.Arm.Elbow.Wrist,
					Hips.Spine.Spine2.Spine3.Spine4.LeftShoulder.Arm.Elbow.Wrist.GetFinngers(),
					Hips.Spine.Spine2.Spine3.Spine4.RightShoulder.Arm.Elbow.Wrist.GetFinngers()
					);
			});
		}
		protected override void OnLoaded() {
			base.OnLoaded();
			FullBodyIK.Awake(Entity);
			BoneUpdate();
		}


		protected override void OnAttach() {
			base.OnAttach();
			var avtarBones = Entity.GetFirstComponent<AvatarBones>();
			if(avtarBones is null) {
				return;
			}
			Hips.TargetBone.Target = avtarBones.Hips.Target;
			Hips.Spine.TargetBone.Target = avtarBones.Spine.Target;
			Hips.Spine.Spine2.TargetBone.Target = avtarBones.Spine2.Target;
			Hips.Spine.Spine2.Spine3.TargetBone.Target = avtarBones.Spine3.Target;
			Hips.Spine.Spine2.Spine3.Spine4.TargetBone.Target = avtarBones.Spine4.Target;
			Hips.Spine.Spine2.Spine3.Spine4.Neck.TargetBone.Target = avtarBones.Neck.Target;
			Hips.Spine.Spine2.Spine3.Spine4.Neck.Head.TargetBone.Target = avtarBones.Head.Target;
			Hips.Spine.Spine2.Spine3.Spine4.Neck.Head.LeftEye.TargetBone.Target = avtarBones.LeftEye.Target;
			Hips.Spine.Spine2.Spine3.Spine4.Neck.Head.RightEye.TargetBone.Target = avtarBones.RightEye.Target;
			Hips.Spine.Spine2.Spine3.Spine4.LeftLeg.TargetBone.Target = avtarBones.LeftLeg.Target;
			Hips.Spine.Spine2.Spine3.Spine4.RightLeg.TargetBone.Target = avtarBones.RightLeg.Target;
			Hips.Spine.Spine2.Spine3.Spine4.LeftLeg.Knee.TargetBone.Target = avtarBones.LeftKnee.Target;
			Hips.Spine.Spine2.Spine3.Spine4.RightLeg.Knee.TargetBone.Target = avtarBones.RightKnee.Target;
			Hips.Spine.Spine2.Spine3.Spine4.LeftLeg.Knee.Foot.TargetBone.Target = avtarBones.LeftFoot.Target;
			Hips.Spine.Spine2.Spine3.Spine4.RightLeg.Knee.Foot.TargetBone.Target = avtarBones.RightFoot.Target;
			Hips.Spine.Spine2.Spine3.Spine4.LeftShoulder.TargetBone.Target = avtarBones.LeftShoulder.Target;
			Hips.Spine.Spine2.Spine3.Spine4.RightShoulder.TargetBone.Target = avtarBones.RightShoulder.Target;
			Hips.Spine.Spine2.Spine3.Spine4.LeftShoulder.Arm.TargetBone.Target = avtarBones.LeftArm.Target;
			Hips.Spine.Spine2.Spine3.Spine4.RightShoulder.Arm.TargetBone.Target = avtarBones.RightArm.Target;
			Hips.Spine.Spine2.Spine3.Spine4.LeftShoulder.Arm.Elbow.TargetBone.Target = avtarBones.LeftElbow.Target;
			Hips.Spine.Spine2.Spine3.Spine4.RightShoulder.Arm.Elbow.TargetBone.Target = avtarBones.RightElbow.Target;
			Hips.Spine.Spine2.Spine3.Spine4.LeftShoulder.Arm.Elbow.Wrist.TargetBone.Target = avtarBones.LeftWrist.Target;
			Hips.Spine.Spine2.Spine3.Spine4.RightShoulder.Arm.Elbow.Wrist.TargetBone.Target = avtarBones.RightWrist.Target;
		}

		protected override void Step() {
			FullBodyIK.Update();
		}

	}
}