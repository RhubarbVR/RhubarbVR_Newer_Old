using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using RhuEngine.Input.XRInput;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "User" })]
	public sealed partial class AvatarBones : Component
	{
		public readonly SyncRef<Entity> Jaw;
		public readonly SyncRef<Entity> Hips;
		public readonly SyncRef<Entity> Spine;
		public readonly SyncRef<Entity> Spine2;
		public readonly SyncRef<Entity> Spine3;
		public readonly SyncRef<Entity> Spine4;
		public readonly SyncRef<Entity> Neck;
		public readonly SyncRef<Entity> Head;
		public readonly SyncRef<Entity> LeftEye;
		public readonly SyncRef<Entity> RightEye;
		public readonly SyncRef<Entity> LeftLeg;
		public readonly SyncRef<Entity> LeftKnee;
		public readonly SyncRef<Entity> LeftFoot;
		public readonly SyncRef<Entity> LeftToe;
		public readonly SyncRef<Entity> LeftToe1;
		public readonly SyncRef<Entity> LeftToe2;
		public readonly SyncRef<Entity> LeftToe3;
		public readonly SyncRef<Entity> LeftToe4;
		public readonly SyncRef<Entity> RightLeg;
		public readonly SyncRef<Entity> RightKnee;
		public readonly SyncRef<Entity> RightFoot;
		public readonly SyncRef<Entity> RightToe;
		public readonly SyncRef<Entity> RightToe1;
		public readonly SyncRef<Entity> RightToe2;
		public readonly SyncRef<Entity> RightToe3;
		public readonly SyncRef<Entity> RightToe4;
		public readonly SyncRef<Entity> LeftShoulder;
		public readonly SyncRef<Entity> LeftArm;
		public readonly SyncRef<Entity> LeftArmRoll1;
		public readonly SyncRef<Entity> LeftArmRoll2;
		public readonly SyncRef<Entity> LeftArmRoll3;
		public readonly SyncRef<Entity> LeftArmRoll4;
		public readonly SyncRef<Entity> LeftElbow;
		public readonly SyncRef<Entity> LeftElbowRoll1;
		public readonly SyncRef<Entity> LeftElbowRoll2;
		public readonly SyncRef<Entity> LeftElbowRoll3;
		public readonly SyncRef<Entity> LeftElbowRoll4;
		public readonly SyncRef<Entity> LeftWrist;
		public readonly SyncRef<Entity> RightShoulder;
		public readonly SyncRef<Entity> RightArm;
		public readonly SyncRef<Entity> RightArmRoll1;
		public readonly SyncRef<Entity> RightArmRoll2;
		public readonly SyncRef<Entity> RightArmRoll3;
		public readonly SyncRef<Entity> RightArmRoll4;
		public readonly SyncRef<Entity> RightElbow;
		public readonly SyncRef<Entity> RightElbowRoll1;
		public readonly SyncRef<Entity> RightElbowRoll2;
		public readonly SyncRef<Entity> RightElbowRoll3;
		public readonly SyncRef<Entity> RightElbowRoll4;
		public readonly SyncRef<Entity> RightWrist;
		public readonly SyncRef<Entity> LeftThumb1;
		public readonly SyncRef<Entity> LeftThumb2;
		public readonly SyncRef<Entity> LeftThumb3;
		public readonly SyncRef<Entity> LeftThumbTip;
		public readonly SyncRef<Entity> LeftIndex1;
		public readonly SyncRef<Entity> LeftIndex2;
		public readonly SyncRef<Entity> LeftIndex3;
		public readonly SyncRef<Entity> LeftIndexTip;
		public readonly SyncRef<Entity> LeftMiddle1;
		public readonly SyncRef<Entity> LeftMiddle2;
		public readonly SyncRef<Entity> LeftMiddle3;
		public readonly SyncRef<Entity> LeftMiddleTip;
		public readonly SyncRef<Entity> LeftRing1;
		public readonly SyncRef<Entity> LeftRing2;
		public readonly SyncRef<Entity> LeftRing3;
		public readonly SyncRef<Entity> LeftRingTip;
		public readonly SyncRef<Entity> LeftLittle1;
		public readonly SyncRef<Entity> LeftLittle2;
		public readonly SyncRef<Entity> LeftLittle3;
		public readonly SyncRef<Entity> LeftLittleTip;
		public readonly SyncRef<Entity> RightThumb1;
		public readonly SyncRef<Entity> RightThumb2;
		public readonly SyncRef<Entity> RightThumb3;
		public readonly SyncRef<Entity> RightThumbTip;
		public readonly SyncRef<Entity> RightIndex1;
		public readonly SyncRef<Entity> RightIndex2;
		public readonly SyncRef<Entity> RightIndex3;
		public readonly SyncRef<Entity> RightIndexTip;
		public readonly SyncRef<Entity> RightMiddle1;
		public readonly SyncRef<Entity> RightMiddle2;
		public readonly SyncRef<Entity> RightMiddle3;
		public readonly SyncRef<Entity> RightMiddleTip;
		public readonly SyncRef<Entity> RightRing1;
		public readonly SyncRef<Entity> RightRing2;
		public readonly SyncRef<Entity> RightRing3;
		public readonly SyncRef<Entity> RightRingTip;
		public readonly SyncRef<Entity> RightLittle1;
		public readonly SyncRef<Entity> RightLittle2;
		public readonly SyncRef<Entity> RightLittle3;
		public readonly SyncRef<Entity> RightLittleTip;

		public void SetupIK() {

		}

		public string SetupAvatar(Entity entity = null) {
			FindIKBones(entity);
			var returnData = MissingBones();
			return string.IsNullOrEmpty(returnData) ? null : returnData;
		}

		public override string ToString() {
			var returnString = new StringBuilder();
			returnString.AppendLine($"{nameof(Jaw)}: {Jaw.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(Hips)}: {Hips.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(Spine)}: {Spine.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(Spine2)}: {Spine2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(Spine3)}: {Spine3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(Spine4)}: {Spine4.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(Neck)}: {Neck.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(Head)}: {Head.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftEye)}: {LeftEye.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightEye)}: {RightEye.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftLeg)}: {LeftLeg.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftKnee)}: {LeftKnee.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftFoot)}: {LeftFoot.Target?.name.Value ?? "null"}");

			returnString.AppendLine($"{nameof(LeftToe)}: {LeftToe.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftToe1)}: {LeftToe1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftToe2)}: {LeftToe2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftToe3)}: {LeftToe3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftToe4)}: {LeftToe4.Target?.name.Value ?? "null"}");

			returnString.AppendLine($"{nameof(RightLeg)}: {RightLeg.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightKnee)}: {RightKnee.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightFoot)}: {RightFoot.Target?.name.Value ?? "null"}");

			returnString.AppendLine($"{nameof(RightToe)}: {RightToe.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightToe1)}: {RightToe1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightToe2)}: {RightToe2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightToe3)}: {RightToe3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightToe4)}: {RightToe4.Target?.name.Value ?? "null"}");

			returnString.AppendLine($"{nameof(LeftShoulder)}: {LeftShoulder.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftArm)}: {LeftArm.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftArmRoll1)}: {LeftArmRoll1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftArmRoll2)}: {LeftArmRoll2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftArmRoll3)}: {LeftArmRoll3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftArmRoll4)}: {LeftArmRoll4.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftElbow)}: {LeftElbow.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftElbowRoll1)}: {LeftElbowRoll1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftElbowRoll2)}: {LeftElbowRoll2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftElbowRoll3)}: {LeftElbowRoll3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftElbowRoll4)}: {LeftElbowRoll4.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftWrist)}: {LeftWrist.Target?.name.Value ?? "null"}");

			returnString.AppendLine($"{nameof(RightShoulder)}: {RightShoulder.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightArm)}: {RightArm.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightArmRoll1)}: {RightArmRoll1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightArmRoll2)}: {RightArmRoll2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightArmRoll3)}: {RightArmRoll3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightArmRoll4)}: {RightArmRoll4.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightElbow)}: {RightElbow.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightElbowRoll1)}: {RightElbowRoll1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightElbowRoll2)}: {RightElbowRoll2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightElbowRoll3)}: {RightElbowRoll3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightElbowRoll4)}: {RightElbowRoll4.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightWrist)}: {RightWrist.Target?.name.Value ?? "null"}");

			returnString.AppendLine($"{nameof(LeftIndex1)}: {LeftIndex1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftIndex2)}: {LeftIndex2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftIndex3)}: {LeftIndex3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftIndexTip)}: {LeftIndexTip.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftThumb1)}: {LeftThumb1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftThumb2)}: {LeftThumb2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftThumb3)}: {LeftThumb3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftThumbTip)}: {LeftThumbTip.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftMiddle1)}: {LeftMiddle1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftMiddle2)}: {LeftMiddle2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftMiddle3)}: {LeftMiddle3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftMiddleTip)}: {LeftMiddleTip.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftRing1)}: {LeftRing1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftRing2)}: {LeftRing2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftRing3)}: {LeftRing3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftRingTip)}: {LeftRingTip.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftLittle1)}: {LeftLittle1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftLittle2)}: {LeftLittle2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftLittle3)}: {LeftLittle3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(LeftLittleTip)}: {LeftLittleTip.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightThumb1)}: {RightThumb1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightThumb2)}: {RightThumb2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightThumb3)}: {RightThumb3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightThumbTip)}: {RightThumbTip.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightIndex1)}: {RightIndex1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightIndex2)}: {RightIndex2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightIndex3)}: {RightIndex3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightIndexTip)}: {RightIndexTip.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightMiddle1)}: {RightMiddle1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightMiddle2)}: {RightMiddle2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightMiddle3)}: {RightMiddle3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightMiddleTip)}: {RightMiddleTip.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightRing1)}: {RightRing1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightRing2)}: {RightRing2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightRing3)}: {RightRing3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightRingTip)}: {RightRingTip.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightLittle1)}: {RightLittle1.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightLittle2)}: {RightLittle2.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightLittle3)}: {RightLittle3.Target?.name.Value ?? "null"}");
			returnString.AppendLine($"{nameof(RightLittleTip)}: {RightLittleTip.Target?.name.Value ?? "null"}");

			return returnString.ToString();
		}

		public string ExtraBones() {
			var returnString = "";
			if (Jaw.Target is not null) { returnString += " " + nameof(Jaw); }
			if (Spine2.Target is not null) { returnString += " " + nameof(Spine2); }
			if (Spine3.Target is not null) { returnString += " " + nameof(Spine3); }
			if (Spine4.Target is not null) { returnString += " " + nameof(Spine4); }
			if (Head.Target is not null) { returnString += " " + nameof(Head); }
			if (LeftEye.Target is not null) { returnString += " " + nameof(LeftEye); }
			if (RightEye.Target is not null) { returnString += " " + nameof(RightEye); }
			if (LeftToe.Target is not null) { returnString += " " + nameof(LeftToe); }
			if (RightToe.Target is not null) { returnString += " " + nameof(RightToe); }
			if (LeftToe1.Target is not null) { returnString += " " + nameof(LeftToe1); }
			if (RightToe1.Target is not null) { returnString += " " + nameof(RightToe1); }
			if (LeftToe2.Target is not null) { returnString += " " + nameof(LeftToe2); }
			if (RightToe2.Target is not null) { returnString += " " + nameof(RightToe2); }
			if (LeftToe3.Target is not null) { returnString += " " + nameof(LeftToe3); }
			if (RightToe3.Target is not null) { returnString += " " + nameof(RightToe3); }
			if (LeftToe4.Target is not null) { returnString += " " + nameof(LeftToe4); }
			if (RightToe4.Target is not null) { returnString += " " + nameof(RightToe4); }
			if (LeftShoulder.Target is not null) { returnString += " " + nameof(LeftShoulder); }
			if (LeftArmRoll1.Target is not null) { returnString += " " + nameof(LeftArmRoll1); }
			if (LeftArmRoll2.Target is not null) { returnString += " " + nameof(LeftArmRoll2); }
			if (LeftArmRoll3.Target is not null) { returnString += " " + nameof(LeftArmRoll3); }
			if (LeftArmRoll4.Target is not null) { returnString += " " + nameof(LeftArmRoll4); }
			if (LeftElbowRoll1.Target is not null) { returnString += " " + nameof(LeftElbowRoll1); }
			if (LeftElbowRoll2.Target is not null) { returnString += " " + nameof(LeftElbowRoll2); }
			if (LeftElbowRoll3.Target is not null) { returnString += " " + nameof(LeftElbowRoll3); }
			if (LeftElbowRoll4.Target is not null) { returnString += " " + nameof(LeftElbowRoll4); }
			if (RightShoulder.Target is not null) { returnString += " " + nameof(RightShoulder); }
			if (RightArmRoll1.Target is not null) { returnString += " " + nameof(RightArmRoll1); }
			if (RightArmRoll2.Target is not null) { returnString += " " + nameof(RightArmRoll2); }
			if (RightArmRoll3.Target is not null) { returnString += " " + nameof(RightArmRoll3); }
			if (RightArmRoll4.Target is not null) { returnString += " " + nameof(RightArmRoll4); }
			if (RightElbowRoll1.Target is not null) { returnString += " " + nameof(RightElbowRoll1); }
			if (RightElbowRoll2.Target is not null) { returnString += " " + nameof(RightElbowRoll2); }
			if (RightElbowRoll3.Target is not null) { returnString += " " + nameof(RightElbowRoll3); }
			if (RightElbowRoll4.Target is not null) { returnString += " " + nameof(RightElbowRoll4); }
			if (LeftThumb1.Target is not null) { returnString += " " + nameof(LeftThumb1); }
			if (LeftThumb2.Target is not null) { returnString += " " + nameof(LeftThumb2); }
			if (LeftThumb3.Target is not null) { returnString += " " + nameof(LeftThumb3); }
			if (LeftThumbTip.Target is not null) { returnString += " " + nameof(LeftThumbTip); }
			if (LeftIndex1.Target is not null) { returnString += " " + nameof(LeftIndex1); }
			if (LeftIndex2.Target is not null) { returnString += " " + nameof(LeftIndex2); }
			if (LeftIndex3.Target is not null) { returnString += " " + nameof(LeftIndex3); }
			if (LeftIndexTip.Target is not null) { returnString += " " + nameof(LeftIndexTip); }
			if (LeftMiddle1.Target is not null) { returnString += " " + nameof(LeftMiddle1); }
			if (LeftMiddle2.Target is not null) { returnString += " " + nameof(LeftMiddle2); }
			if (LeftMiddle3.Target is not null) { returnString += " " + nameof(LeftMiddle3); }
			if (LeftMiddleTip.Target is not null) { returnString += " " + nameof(LeftMiddleTip); }
			if (LeftLittle1.Target is not null) { returnString += " " + nameof(LeftLittle1); }
			if (LeftLittle2.Target is not null) { returnString += " " + nameof(LeftLittle2); }
			if (LeftLittle3.Target is not null) { returnString += " " + nameof(LeftLittle3); }
			if (LeftLittleTip.Target is not null) { returnString += " " + nameof(LeftLittleTip); }
			if (RightThumb1.Target is not null) { returnString += " " + nameof(RightThumb1); }
			if (RightThumb2.Target is not null) { returnString += " " + nameof(RightThumb2); }
			if (RightThumb3.Target is not null) { returnString += " " + nameof(RightThumb3); }
			if (RightThumbTip.Target is not null) { returnString += " " + nameof(RightThumbTip); }
			if (RightIndex1.Target is not null) { returnString += " " + nameof(RightIndex1); }
			if (RightIndex2.Target is not null) { returnString += " " + nameof(RightIndex2); }
			if (RightIndex3.Target is not null) { returnString += " " + nameof(RightIndex3); }
			if (RightIndexTip.Target is not null) { returnString += " " + nameof(RightIndexTip); }
			if (RightMiddle1.Target is not null) { returnString += " " + nameof(RightMiddle1); }
			if (RightMiddle2.Target is not null) { returnString += " " + nameof(RightMiddle2); }
			if (RightMiddle3.Target is not null) { returnString += " " + nameof(RightMiddle3); }
			if (RightMiddleTip.Target is not null) { returnString += " " + nameof(RightMiddleTip); }
			if (RightLittle1.Target is not null) { returnString += " " + nameof(RightLittle1); }
			if (RightLittle2.Target is not null) { returnString += " " + nameof(RightLittle2); }
			if (RightLittle3.Target is not null) { returnString += " " + nameof(RightLittle3); }
			if (RightLittleTip.Target is not null) { returnString += " " + nameof(RightLittleTip); }

			return returnString;
		}

		public string MissingBones() {
			var returnString = "";

			if (Hips.Target is null) { returnString += " " + nameof(Hips); }
			if (Spine.Target is null) { returnString += " " + nameof(Spine); }
			if (Neck.Target is null) { returnString += " " + nameof(Neck); }
			if (LeftLeg.Target is null) { returnString += " " + nameof(LeftLeg); }
			if (LeftKnee.Target is null) { returnString += " " + nameof(LeftKnee); }
			if (LeftFoot.Target is null) { returnString += " " + nameof(LeftFoot); }
			if (RightLeg.Target is null) { returnString += " " + nameof(RightLeg); }
			if (RightKnee.Target is null) { returnString += " " + nameof(RightKnee); }
			if (RightFoot.Target is null) { returnString += " " + nameof(RightFoot); }
			if (LeftArm.Target is null) { returnString += " " + nameof(LeftArm); }
			if (LeftElbow.Target is null) { returnString += " " + nameof(LeftElbow); }
			if (LeftWrist.Target is null) { returnString += " " + nameof(LeftWrist); }
			if (RightArm.Target is null) { returnString += " " + nameof(RightArm); }
			if (RightElbow.Target is null) { returnString += " " + nameof(RightElbow); }
			if (RightWrist.Target is null) { returnString += " " + nameof(RightWrist); }

			return returnString;
		}


		public static string CleanUpBoneString(string boneName) {
			var name = boneName.ToLower();

			name = name.Replace(" ", "_");
			name = name.Replace("-", "_");
			name = name.Replace(".", "_");
			name = name.Replace("_", "");

			name = name.Replace("left", "l");
			name = name.Replace("right", "r");
			name = name.Replace("right", "r");

			name = name.Replace("pelvis", "hip");
			name = name.Replace("root", "hip");
			name = name.Replace("base", "hip");
			name = name.Replace("forearm", "elbow");

			name = name.Replace("chest", "spine");
			name = name.Replace("body", "spine");
			name = name.Replace("controler", "");

			//IGNORE extra bones that might have same name
			name = name.Replace("collider", "IGNORE");
			name = name.Replace("twist", "IGNORE");

			name = name.Replace("armature", "");

			name = name.Replace("hand", "wrist");
			name = name.Replace("forearm", "elbow");
			name = name.Replace("upper", "");
			name = name.Replace("uper", "");
			name = name.Replace("lower", "");
			name = name.Replace("sholder", "shoulder");
			name = name.Replace("clavicle", "shoulder");
			name = name.Replace("thigh", "leg");
			name = name.Replace("shin", "knee");
			name = name.Replace("ankle", "foot");
			name = name.Replace("paw", "foot");
			name = name.Replace("calf", "knee");

			//This is thrown together so might not be that good
			name = name.Replace("足首", "foot");
			name = name.Replace("全ての親", "hip"); // all parents
			name = name.Replace("肩", "shoulder");
			name = name.Replace("指", "finger");
			name = name.Replace("腰", "hip");
			name = name.Replace("足", "leg");
			name = name.Replace("腕捩", "armtwist");
			name = name.Replace("腕", "arm");
			name = name.Replace("ひじ", "elbow");
			name = name.Replace("手首", "wrist");
			name = name.Replace("手", "wrist");
			name = name.Replace("首", "neck");
			name = name.Replace("頭", "head");
			name = name.Replace("ひざ", "knee");
			name = name.Replace("足首", "foot");
			name = name.Replace("下半身", "spine"); // lower body
			name = name.Replace("上半身", "spine1"); // upper body

			//Remove Other names that should not be grouped
			if (name.Contains("thumb") | name.Contains("index") | name.Contains("middle") | name.Contains("ring") | name.Contains("pinky")) {
				name = name.Replace("wrist", "");
			}

			name = name.Replace("pinky", "little");
			name = name.Replace("litle", "little");


			if (name.Contains('l') && name.Contains('r') && name.Length <= 4) {
				name = name.Replace("th", "thumb");
				name = name.Replace("ri", "little");
				name = name.Replace("in", "index");
			}

			return name;
		}

		private static void FindSpineBones(Entity hipsBone, List<Entity> bones) {
			var boneSave = CleanUpBoneString(hipsBone.name.Value);
			if (boneSave.Contains("spine")) {
				if (hipsBone.children.Count != 0) {
					bones.Add(hipsBone);
				}
			}
			foreach (var item in hipsBone.children.Cast<Entity>()) {
				FindSpineBones(item, bones);
			}
		}

		private static void FindAllBoneSided(Entity rootBone, char sideLedder, Queue<Entity> bones, List<Entity> bonese, params string[] names) {
			foreach (var item in names) {
				var current = FindBoneSided(rootBone, item, sideLedder, true);
				if (current is null) {
					continue;
				}
				if (bones.Contains(current)) {
					continue;
				}
				bones.Enqueue(current);
				bonese.Add(current);
			}
		}

		private static void GetBoneList(Entity rootBone, char sideLedder, List<Entity> bones, params string[] names) {
			var currentBones = new Queue<Entity>();
			currentBones.Enqueue(rootBone);
			while (currentBones.TryDequeue(out var current)) {
				FindAllBoneSided(current, sideLedder, currentBones, bones, names);
			}
		}

		private static Entity FindBoneSided(Entity rootBone, string name, char sideLedder, bool skipFirst = false) {
			if (!skipFirst) {
				var boneSave = CleanUpBoneString(rootBone.Name);
				if (boneSave.Contains(name)) {
					if (boneSave.Contains("IGNORE")) {
						return null;
					}
					if (name == "elbow") {
						if (rootBone.children.Count == 0) {
							return null;
						}
					}
					return !boneSave.Replace(name, "").Contains(sideLedder) ? null : rootBone;
				}
			}
			foreach (var item in rootBone.children.Cast<Entity>()) {
				var check = FindBoneSided(item, name, sideLedder);
				if (check != null) {
					return check;
				}
			}
			return null;
		}


		private static Entity FindBoneNoSide(Entity rootBone, string name) {
			var boneSave = CleanUpBoneString(rootBone.Name);
			if (boneSave.Contains(name)) {
				return rootBone.children.Count == 0 ? null : boneSave.Contains('l') ? null : boneSave.Contains('r') ? null : rootBone;
			}
			foreach (var item in rootBone.children.Cast<Entity>()) {
				var check = FindBoneNoSide(item, name);
				if (check != null) {
					return check;
				}
			}
			return null;
		}

		private static bool IsBoneChild(Entity rootBone, Entity targetBones) {
			if (rootBone == targetBones) {
				return true;
			}
			foreach (var item in rootBone.children.Cast<Entity>()) {
				if (IsBoneChild(item, targetBones)) {
					return true;
				}
			}
			return false;
		}

		public void FindIKBones(Entity targetBones = null) {
			targetBones ??= Entity;
			Hips.Target = FindBoneNoSide(targetBones, "hip") ?? targetBones;

			var spineBones = new List<Entity>();
			FindSpineBones(Hips.Target, spineBones);
			for (var i = 0; i < spineBones.Count; i++) {
				var bone = spineBones[i];
				if (i == 0) {
					Spine.Target = bone;
				}
				else if (i == 1) {
					Spine2.Target = bone;
				}
				else if (i == 2) {
					Spine3.Target = bone;
				}
				else if (i == 3) {
					Spine4.Target = bone;
				}
			}

			Neck.Target = FindBoneNoSide(Hips.Target, "neck");
			if (Neck.Target is null) {
				Neck.Target = FindBoneNoSide(Hips.Target, "head");
				Neck.Target ??= FindBoneNoSide(targetBones, "head");
			}
			Head.Target = FindBoneNoSide(Neck.Target, "head");

			LeftLeg.Target = FindBoneSided(Hips.Target, "leg", 'l');
			if (LeftLeg.Target is not null) {
				LeftKnee.Target = FindBoneSided(LeftLeg.Target, "knee", 'l');
				LeftKnee.Target ??= FindBoneSided(LeftLeg.Target, "leg", 'l', true);
				LeftKnee.Target ??= FindBoneSided(Hips.Target, "knee", 'l');
			}
			if (LeftKnee.Target is not null) {
				LeftFoot.Target = FindBoneSided(LeftKnee.Target, "foot", 'l');
				LeftFoot.Target ??= FindBoneSided(LeftKnee.Target, "leg", 'l', true);
				LeftFoot.Target ??= FindBoneSided(Hips.Target, "foot", 'l');
			}

			RightLeg.Target = FindBoneSided(Hips.Target, "leg", 'r');
			if (RightLeg.Target is not null) {
				RightKnee.Target = FindBoneSided(RightLeg.Target, "knee", 'r');
				RightKnee.Target ??= FindBoneSided(RightLeg.Target, "leg", 'r', true);
				RightKnee.Target ??= FindBoneSided(Hips.Target, "knee", 'r');
			}
			if (RightKnee.Target is not null) {
				RightFoot.Target = FindBoneSided(RightKnee.Target, "foot", 'r');
				RightFoot.Target ??= FindBoneSided(RightKnee.Target, "leg", 'r', true);
				RightFoot.Target ??= FindBoneSided(Hips.Target, "foot", 'r');
			}

			LeftShoulder.Target = FindBoneSided(Hips.Target, "shoulder", 'l');

			LeftArm.Target = FindBoneSided(Hips.Target, "arm", 'l');
			if (LeftArm.Target is not null) {
				LeftElbow.Target = FindBoneSided(LeftArm.Target, "elbow", 'l');
				LeftElbow.Target ??= FindBoneSided(LeftArm.Target, "arm", 'l', true);
				LeftElbow.Target ??= FindBoneSided(Hips.Target, "elbow", 'l');
			}
			if (LeftElbow.Target is not null) {
				LeftWrist.Target = FindBoneSided(LeftElbow.Target, "wrist", 'l');
				LeftWrist.Target ??= FindBoneSided(LeftElbow.Target, "arm", 'l', true);
				LeftWrist.Target ??= FindBoneSided(Hips.Target, "wrist", 'l');
			}

			if (LeftArm.Target is not null && LeftElbow.Target is not null && LeftWrist.Target is not null) {
				var leftArmBones = new List<Entity>();
				GetBoneList(LeftArm.Target, 'l', leftArmBones, "arm", "elbow", "wrist");

				foreach (var item in leftArmBones) {
					if (LeftWrist.Target == item || LeftElbow.Target == item || LeftArm.Target == item) {
						continue;
					}
					if (IsBoneChild(LeftWrist.Target, item)) {
						continue; //  have no idea what theses are
					}
					else if (IsBoneChild(LeftElbow.Target, item)) {
						if (LeftElbowRoll1.Target is null) {
							LeftElbowRoll1.Target = item;
						}
						else if (LeftElbowRoll2.Target is null) {
							LeftElbowRoll2.Target = item;
						}
						else if (LeftElbowRoll3.Target is null) {
							LeftElbowRoll3.Target = item;
						}
						else if (LeftElbowRoll4.Target is null) {
							LeftElbowRoll4.Target = item;
						}
					}
					else if (IsBoneChild(LeftArm.Target, item)) {
						if (LeftArmRoll1.Target is null) {
							LeftArmRoll1.Target = item;
						}
						else if (LeftArmRoll2.Target is null) {
							LeftArmRoll2.Target = item;
						}
						else if (LeftArmRoll3.Target is null) {
							LeftArmRoll3.Target = item;
						}
						else if (LeftArmRoll4.Target is null) {
							LeftArmRoll4.Target = item;
						}
					}
				}
			}

			RightShoulder.Target = FindBoneSided(Hips.Target, "shoulder", 'r');

			RightArm.Target = FindBoneSided(Hips.Target, "arm", 'r');
			if (RightArm.Target is not null) {
				RightElbow.Target = FindBoneSided(RightArm.Target, "elbow", 'r');
				RightElbow.Target ??= FindBoneSided(RightArm.Target, "arm", 'r', true);
				RightElbow.Target ??= FindBoneSided(Hips.Target, "elbow", 'r');
			}
			if (RightElbow.Target is not null) {
				RightWrist.Target = FindBoneSided(RightElbow.Target, "wrist", 'r');
				RightWrist.Target ??= FindBoneSided(RightElbow.Target, "arm", 'r', true);
				RightWrist.Target ??= FindBoneSided(Hips.Target, "wrist", 'r');
			}

			if (Neck.Target is not null) {
				Jaw.Target = FindBoneNoSide(Neck.Target, "jaw");
				LeftEye.Target = FindBoneSided(Neck.Target, "eye", 'l');
				RightEye.Target = FindBoneSided(Neck.Target, "eye", 'r');
			}

			if (LeftFoot.Target is not null) {
				foreach (var toe in LeftFoot.Target.children.Cast<Entity>()) {
					if (LeftToe.Target is null) {
						LeftToe.Target = toe;
					}
					else if (LeftToe1.Target is null) {
						LeftToe1.Target = toe;
					}
					else if (LeftToe2.Target is null) {
						LeftToe2.Target = toe;
					}
					else if (LeftToe3.Target is null) {
						LeftToe3.Target = toe;
					}
					else if (LeftToe4.Target is null) {
						LeftToe4.Target = toe;
					}
				}
			}

			if (RightFoot.Target is not null) {
				foreach (var toe in RightFoot.Target.children.Cast<Entity>()) {
					if (RightToe.Target is null) {
						RightToe.Target = toe;
					}
					else if (RightToe1.Target is null) {
						RightToe1.Target = toe;
					}
					else if (RightToe2.Target is null) {
						RightToe2.Target = toe;
					}
					else if (RightToe3.Target is null) {
						RightToe3.Target = toe;
					}
					else if (RightToe4.Target is null) {
						RightToe4.Target = toe;
					}
				}
			}

			if (LeftWrist.Target is not null) {
				LeftThumb1.Target = FindBoneSided(LeftWrist.Target, "thumb", 'l');
				LeftRing1.Target = FindBoneSided(LeftWrist.Target, "ring", 'l');
				LeftIndex1.Target = FindBoneSided(LeftWrist.Target, "index", 'l');
				LeftMiddle1.Target = FindBoneSided(LeftWrist.Target, "middle", 'l');
				LeftLittle1.Target = FindBoneSided(LeftWrist.Target, "little", 'l');

				if (LeftThumb1.Target is not null) {
					LeftThumb2.Target = FindBoneSided(LeftThumb1.Target, "thumb", 'l', true);
					if (LeftThumb2.Target is not null) {
						LeftThumb3.Target = FindBoneSided(LeftThumb2.Target, "thumb", 'l', true);
					}
					if (LeftThumb3.Target is not null) {
						LeftThumbTip.Target = FindBoneSided(LeftThumb3.Target, "thumb", 'l', true);
					}
				}

				if (LeftRing1.Target is not null) {
					LeftRing2.Target = FindBoneSided(LeftRing1.Target, "ring", 'l', true);
					if (LeftRing2.Target is not null) {
						LeftRing3.Target = FindBoneSided(LeftRing2.Target, "ring", 'l', true);
					}
					if (LeftRing3.Target is not null) {
						LeftRingTip.Target = FindBoneSided(LeftRing3.Target, "ring", 'l', true);
					}
				}

				if (LeftIndex1.Target is not null) {
					LeftIndex2.Target = FindBoneSided(LeftIndex1.Target, "index", 'l', true);
					if (LeftIndex2.Target is not null) {
						LeftIndex3.Target = FindBoneSided(LeftIndex2.Target, "index", 'l', true);
					}
					if (LeftIndex3.Target is not null) {
						LeftIndexTip.Target = FindBoneSided(LeftIndex3.Target, "index", 'l', true);
					}
				}

				if (LeftMiddle1.Target is not null) {
					LeftMiddle2.Target = FindBoneSided(LeftMiddle1.Target, "middle", 'l', true);
					if (LeftMiddle2.Target is not null) {
						LeftMiddle3.Target = FindBoneSided(LeftMiddle2.Target, "middle", 'l', true);
					}
					if (LeftMiddle3.Target is not null) {
						LeftMiddleTip.Target = FindBoneSided(LeftMiddle3.Target, "middle", 'l', true);
					}
				}

				if (LeftLittle1.Target is not null) {
					LeftLittle2.Target = FindBoneSided(LeftLittle1.Target, "little", 'l', true);
					if (LeftLittle2.Target is not null) {
						LeftLittle3.Target = FindBoneSided(LeftLittle2.Target, "little", 'l', true);
					}
					if (LeftLittle3.Target is not null) {
						LeftLittleTip.Target = FindBoneSided(LeftLittle3.Target, "little", 'l', true);
					}
				}
			}

			if (RightWrist.Target is not null) {
				RightThumb1.Target = FindBoneSided(RightWrist.Target, "thumb", 'r');
				RightRing1.Target = FindBoneSided(RightWrist.Target, "ring", 'r');
				RightIndex1.Target = FindBoneSided(RightWrist.Target, "index", 'r');
				RightMiddle1.Target = FindBoneSided(RightWrist.Target, "middle", 'r');
				RightLittle1.Target = FindBoneSided(RightWrist.Target, "little", 'r');

				if (RightThumb1.Target is not null) {
					RightThumb2.Target = FindBoneSided(RightThumb1.Target, "thumb", 'r', true);
					if (RightThumb2.Target is not null) {
						RightThumb3.Target = FindBoneSided(RightThumb2.Target, "thumb", 'r', true);
					}
					if (RightThumb3.Target is not null) {
						RightThumbTip.Target = FindBoneSided(RightThumb3.Target, "thumb", 'r', true);
					}
				}

				if (RightRing1.Target is not null) {
					RightRing2.Target = FindBoneSided(RightRing1.Target, "ring", 'r', true);
					if (RightRing2.Target is not null) {
						RightRing3.Target = FindBoneSided(RightRing2.Target, "ring", 'r', true);
					}
					if (RightRing3.Target is not null) {
						RightRingTip.Target = FindBoneSided(RightRing3.Target, "ring", 'r', true);
					}
				}

				if (RightIndex1.Target is not null) {
					RightIndex2.Target = FindBoneSided(RightIndex1.Target, "index", 'r', true);
					if (RightIndex2.Target is not null) {
						RightIndex3.Target = FindBoneSided(RightIndex2.Target, "index", 'r', true);
					}
					if (RightIndex3.Target is not null) {
						RightIndexTip.Target = FindBoneSided(RightIndex3.Target, "index", 'r', true);
					}
				}

				if (RightMiddle1.Target is not null) {
					RightMiddle2.Target = FindBoneSided(RightMiddle1.Target, "middle", 'r', true);
					if (RightMiddle2.Target is not null) {
						RightMiddle3.Target = FindBoneSided(RightMiddle2.Target, "middle", 'r', true);
					}
					if (RightMiddle3.Target is not null) {
						RightMiddleTip.Target = FindBoneSided(RightMiddle3.Target, "middle", 'r', true);
					}
				}

				if (RightLittle1.Target is not null) {
					RightLittle2.Target = FindBoneSided(RightLittle1.Target, "little", 'r', true);
					if (RightLittle2.Target is not null) {
						RightLittle3.Target = FindBoneSided(RightLittle2.Target, "little", 'r', true);
					}
					if (RightLittle3.Target is not null) {
						RightLittleTip.Target = FindBoneSided(RightLittle3.Target, "little", 'r', true);
					}
				}
			}
		}
	}
}