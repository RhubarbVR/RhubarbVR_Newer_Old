// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

namespace RNumerics
{

	public enum EyesType
	{
		Normal,
		LegacyMove,
	}

	public enum Side
	{
		Left,
		Right,
		Max,
		None = Max,
	}

	public enum LimbIKType
	{
		Leg,
		Arm,
		Max,
		Unknown = Max,
	}

	public enum LimbIKLocation
	{
		LeftLeg,
		RightLeg,
		LeftArm,
		RightArm,
		Max,
		Unknown = Max,
	}


	public enum FingerIKType
	{
		LeftWrist,
		RightWrist,
		Max,
		None = Max,
	}

	public enum BoneType
	{
		Hips,
		Spine,
		Neck,
		Head,
		Eye,

		Leg,
		Knee,
		Foot,

		Shoulder,
		Arm,
		ArmRoll,
		Elbow,
		ElbowRoll,
		Wrist,

		HandFinger,

		Max,
		Unknown = Max,
	}

	public enum BoneLocation
	{
		Hips,
		Spine,
		Spine2,
		Spine3,
		Spine4,
		Neck,
		Head,
		LeftEye,
		RightEye,

		LeftLeg,
		RightLeg,
		LeftKnee,
		RightKnee,
		LeftFoot,
		RightFoot,

		LeftShoulder,
		RightShoulder,
		LeftArm,
		RightArm,
		LeftArmRoll0,
		LeftArmRoll1,
		LeftArmRoll2,
		LeftArmRoll3,
		RightArmRoll0,
		RightArmRoll1,
		RightArmRoll2,
		RightArmRoll3,
		LeftElbow,
		RightElbow,
		LeftElbowRoll0,
		LeftElbowRoll1,
		LeftElbowRoll2,
		LeftElbowRoll3,
		RightElbowRoll0,
		RightElbowRoll1,
		RightElbowRoll2,
		RightElbowRoll3,
		LeftWrist,
		RightWrist,

		LeftHandThumb0,
		LeftHandThumb1,
		LeftHandThumb2,
		LeftHandThumbTip,
		LeftHandIndex0,
		LeftHandIndex1,
		LeftHandIndex2,
		LeftHandIndexTip,
		LeftHandMiddle0,
		LeftHandMiddle1,
		LeftHandMiddle2,
		LeftHandMiddleTip,
		LeftHandRing0,
		LeftHandRing1,
		LeftHandRing2,
		LeftHandRingTip,
		LeftHandLittle0,
		LeftHandLittle1,
		LeftHandLittle2,
		LeftHandLittleTip,

		RightHandThumb0,
		RightHandThumb1,
		RightHandThumb2,
		RightHandThumbTip,
		RightHandIndex0,
		RightHandIndex1,
		RightHandIndex2,
		RightHandIndexTip,
		RightHandMiddle0,
		RightHandMiddle1,
		RightHandMiddle2,
		RightHandMiddleTip,
		RightHandRing0,
		RightHandRing1,
		RightHandRing2,
		RightHandRingTip,
		RightHandLittle0,
		RightHandLittle1,
		RightHandLittle2,
		RightHandLittleTip,

		Max,
		Unknown = Max,
		SpineU = Spine4,
	}


	public enum EffectorType
	{
		Root,
		Hips,
		Neck,
		Head,
		Eyes,

		Knee,
		Foot,

		Arm,
		Elbow,
		Wrist,

		HandFinger,

		Max,
		Unknown = Max,
	}

	public enum EffectorLocation
	{
		Root,
		Hips,
		Neck,
		Head,
		Eyes,

		LeftKnee,
		RightKnee,
		LeftFoot,
		RightFoot,

		LeftArm,
		RightArm,
		LeftElbow,
		RightElbow,
		LeftWrist,
		RightWrist,

		LeftHandThumb,
		LeftHandIndex,
		LeftHandMiddle,
		LeftHandRing,
		LeftHandLittle,
		RightHandThumb,
		RightHandIndex,
		RightHandMiddle,
		RightHandRing,
		RightHandLittle,

		Max,
		Unknown = Max,
	}

	public enum FingerType
	{
		Thumb,
		Index,
		Middle,
		Ring,
		Little,
		Max,
		Unknown = Max,
	}

	public enum DirectionAs
	{
		None,
		XPlus,
		XMinus,
		YPlus,
		YMinus,
		Max,
		Uknown = Max,
	}
}