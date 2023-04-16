// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;

using static RNumerics.IK.FullBodyIK;

namespace RNumerics.IK
{
	[System.Serializable]
	public partial class FullBodyIK
	{
		[System.Serializable]
		public class BodyBones
		{
			public Bone hips;
			public Bone spine;
			public Bone spine2;
			public Bone spine3;
			public Bone spine4;

			public Bone SpineU => spine4;
		}

		[System.Serializable]
		public class HeadBones
		{
			public Bone neck;
			public Bone head;
			public Bone leftEye;
			public Bone rightEye;
		}

		[System.Serializable]
		public class LegBones
		{
			public Bone leg;
			public Bone knee;
			public Bone foot;
		}

		[System.Serializable]
		public class ArmBones
		{
			public Bone shoulder;
			public Bone arm;
			public Bone[] armRoll;
			public Bone elbow;
			public Bone[] elbowRoll;
			public Bone wrist;

			public void Repair() {
				SafeResize(ref armRoll, MaxArmRollLength);
				SafeResize(ref elbowRoll, MaxElbowRollLength);
			}
		}

		[System.Serializable]
		public class FingersBones
		{
			public Bone[] thumb;
			public Bone[] index;
			public Bone[] middle;
			public Bone[] ring;
			public Bone[] little;

			public void Repair() {
				SafeResize(ref thumb, MaxHandFingerLength);
				SafeResize(ref index, MaxHandFingerLength);
				SafeResize(ref middle, MaxHandFingerLength);
				SafeResize(ref ring, MaxHandFingerLength);
				SafeResize(ref little, MaxHandFingerLength);
				// Memo: Don't alloc each bone instances.( Alloc in _Prefix() ).
			}
		}

		[System.Serializable]
		public class BodyEffectors
		{
			public Effector hips;
		}

		[System.Serializable]
		public class HeadEffectors
		{
			public Effector neck;
			public Effector head;
			public Effector eyes;
		}

		[System.Serializable]
		public class LegEffectors
		{
			public Effector knee;
			public Effector foot;
		}

		[System.Serializable]
		public class ArmEffectors
		{
			public Effector arm;
			public Effector elbow;
			public Effector wrist;
		}

		[System.Serializable]
		public class FingersEffectors
		{
			public Effector thumb;
			public Effector index;
			public Effector middle;
			public Effector ring;
			public Effector little;
		}

		public enum AutomaticBool
		{
			Auto = -1,
			Disable = 0,
			Enable = 1,
		}

		public enum SyncDisplacement
		{
			Disable,
			Firstframe,
			Everyframe,
		}

		[System.Serializable]
		public class Settings
		{
			public SyncDisplacement syncDisplacement = SyncDisplacement.Disable;

			public AutomaticBool shoulderDirYAsNeck = AutomaticBool.Auto;


			public bool rollBonesEnabled = false;
			public bool createEffectorIIKTransform = true;

			[System.Serializable]
			public class BodyIK
			{
				public bool forceSolveEnabled = true;

				public bool lowerSolveEnabled = true;
				public bool upperSolveEnabled = true;
				public bool computeWorldIIKTransform = true;

				public bool shoulderSolveEnabled = true;
				public float shoulderSolveBendingRate = 0.25f;
				public bool shoulderLimitEnabled = true;
				public float shoulderLimitAngleYPlus = 30.0f;
				public float shoulderLimitAngleYMinus = 1.0f;
				public float shoulderLimitAngleZ = 30.0f;

				public float spineDirXLegToArmRate = 0.5f;
				public float spineDirXLegToArmToRate = 1.0f;
				public float spineDirYLerpRate = 0.5f;

				public float upperBodyMovingfixRate = 1.0f;
				public float upperHeadMovingfixRate = 0.8f;
				public float upperCenterLegTranslateRate = 0.5f;
				public float upperSpineTranslateRate = 0.65f;
				public float upperCenterLegRotateRate = 0.6f;
				public float upperSpineRotateRate = 0.9f;
				public float upperPostTranslateRate = 1.0f;

				public bool upperSolveHipsEnabled = true;
				public bool upperSolveSpineEnabled = true;
				public bool upperSolveSpine2Enabled = true;
				public bool upperSolveSpine3Enabled = true;
				public bool upperSolveSpine4Enabled = true;

				public float upperCenterLegLerpRate = 1.0f;
				public float upperSpineLerpRate = 1.0f;

				public bool upperDirXLimitEnabled = true; // Effective for spineLimitEnabled && spineLimitAngleX
				public float upperDirXLimitAngleY = 20.0f;

				public bool spineLimitEnabled = true;
				public bool spineAccurateLimitEnabled = false;
				public float spineLimitAngleX = 40.0f;
				public float spineLimitAngleY = 25.0f;

				public float upperContinuousPreTranslateRate = 0.2f;
				public float upperContinuousPreTranslateStableRate = 0.65f;
				public float upperContinuousCenterLegRotationStableRate = 0.0f;
				public float upperContinuousPostTranslateStableRate = 0.01f;
				public float upperContinuousSpineDirYLerpRate = 0.5f;

				public float upperNeckToCenterLegRate = 0.6f;
				public float upperNeckToSpineRate = 0.9f;
				public float upperEyesToCenterLegRate = 0.2f;
				public float upperEyesToSpineRate = 0.5f;
				public float upperEyesYawRate = 0.8f;
				public float upperEyesPitchUpRate = 0.25f;
				public float upperEyesPitchDownRate = 0.5f;
				public float upperEyesLimitYaw = 80.0f;
				public float upperEyesLimitPitchUp = 10.0f;
				public float upperEyesLimitPitchDown = 45.0f;
				public float upperEyesTraceAngle = 160.0f;
			}

			[System.Serializable]
			public class LimbIK
			{
				public bool legAlwaysSolveEnabled = true;
				public bool armAlwaysSolveEnabled = false;

				public float automaticKneeBaseAngle = 0.0f;

				public bool presolveKneeEnabled = false;
				public bool presolveElbowEnabled = false;
				public float presolveKneeRate = 1.0f;
				public float presolveKneeLerpAngle = 10.0f;
				public float presolveKneeLerpLengthRate = 0.1f;
				public float presolveElbowRate = 1.0f;
				public float presolveElbowLerpAngle = 10.0f;
				public float presolveElbowLerpLengthRate = 0.1f;

				public bool prefixLegEffectorEnabled = true;

				public float prefixLegUpperLimitAngle = 60.0f;
				public float prefixKneeUpperLimitAngle = 45.0f;

				public float legEffectorMinLengthRate = 0.1f;
				public float legEffectorMaxLengthRate = 0.9999f;
				public float armEffectorMaxLengthRate = 0.9999f;

				public bool armBasisForcefixEnabled = true;
				public float armBasisForcefixEffectorLengthRate = 0.99f;
				public float armBasisForcefixEffectorLengthLerpRate = 0.03f;

				public bool armEffectorBackfixEnabled = true;
				public bool armEffectorInnerfixEnabled = true;

				// Arm back area.(Automatic only, Based on localXZ)
				public float armEffectorBackBeginAngle = 5.0f;
				public float armEffectorBackCoreBeginAngle = -10.0f;
				public float armEffectorBackCoreEndAngle = -30.0f;
				public float armEffectorBackEndAngle = -160.0f;

				// Arm back area.(Automatic only, Based on localYZ)
				public float armEffectorBackCoreUpperAngle = 8.0f;
				public float armEffectorBackCoreLowerAngle = -15.0f;

				// Arm elbow angles.(Automatic only)
				public float automaticElbowBaseAngle = 30.0f;
				public float automaticElbowLowerAngle = 90.0f;
				public float automaticElbowUpperAngle = 90.0f;
				public float automaticElbowBackUpperAngle = 180.0f;
				public float automaticElbowBackLowerAngle = 330.0f;

				// Arm elbow limit angles.(Automatic / Manual)
				public float elbowFrontInnerLimitAngle = 5.0f;
				public float elbowBackInnerLimitAngle = 0.0f;

				// Wrist limit
				public bool wristLimitEnabled = true;
				public float wristLimitAngle = 90.0f;

				// Foot limit
				public bool footLimitEnabled = true;
				public float footLimitYaw = 45.0f;
				public float footLimitPitchUp = 45.0f;
				public float footLimitPitchDown = 60.0f;
				public float footLimitRoll = 45.0f;
			}

			[System.Serializable]
			public class HeadIK
			{
				public float neckLimitPitchUp = 15.0f;
				public float neckLimitPitchDown = 30.0f;
				public float neckLimitRoll = 5.0f;

				public float eyesToNeckPitchRate = 0.4f;

				public float headLimitYaw = 60.0f;
				public float headLimitPitchUp = 15.0f;
				public float headLimitPitchDown = 15.0f;
				public float headLimitRoll = 5.0f;

				public float eyesToHeadYawRate = 0.8f;
				public float eyesToHeadPitchRate = 0.5f;

				public float eyesTraceAngle = 110.0f;

				public float eyesLimitYaw = 40.0f;
				public float eyesLimitPitch = 12.0f;
				public float eyesYawRate = 0.796f;
				public float eyesPitchRate = 0.729f;
				public float eyesYawOuterRate = 0.356f;
				public float eyesYawInnerRate = 0.212f;
			}

			[System.Serializable]
			public class FingerIK
			{
			}

			public BodyIK bodyIK;
			public LimbIK limbIK;
			public HeadIK headIK;
			public FingerIK fingerIK;

			public void Prefix() {
				SafeNew(ref bodyIK);
				SafeNew(ref limbIK);
				SafeNew(ref headIK);
				SafeNew(ref fingerIK);
			}
		}

		[System.Serializable]
		public class EditorSettings
		{
			public bool isAdvanced;
			public int toolbarSelected;
			public bool isShowEffectorIIKTransform;
		}

		// Memo: Not Serializable
		public class InternalValues
		{
			public bool animatorEnabled;
			public bool resetIIKTransforms;
			public bool continuousSolverEnabled;
			public int shoulderDirYAsNeck = -1;

			public Vector3f defaultRootPosition = Vector3f.Zero;
			public Matrix3x3 defaultRootBasis = Matrix3x3.identity;
			public Matrix3x3 defaultRootBasisInv = Matrix3x3.identity;
			public Quaternionf defaultRootRotation = Quaternionf.Identity;

			// Using by resetIIKTransforms & continuousSolverEnabled.
			public Vector3f baseHipsPos = Vector3f.Zero;
			public Matrix3x3 baseHipsBasis = Matrix3x3.identity;

			public class BodyIK
			{
				public CachedDegreesToSin shoulderLimitThetaYPlus = CachedDegreesToSin.zero;
				public CachedDegreesToSin shoulderLimitThetaYMinus = CachedDegreesToSin.zero;
				public CachedDegreesToSin shoulderLimitThetaZ = CachedDegreesToSin.zero;

				public CachedRate01 upperCenterLegTranslateRate = CachedRate01.zero;
				public CachedRate01 upperSpineTranslateRate = CachedRate01.zero;

				public CachedRate01 upperPreTranslateRate = CachedRate01.zero;
				public CachedRate01 upperPostTranslateRate = CachedRate01.zero;

				public CachedRate01 upperCenterLegRotateRate = CachedRate01.zero;
				public CachedRate01 upperSpineRotateRate = CachedRate01.zero;
				public bool isFuzzyUpperCenterLegAndSpineRotationRate = true;

				public CachedDegreesToSin upperEyesLimitYaw = CachedDegreesToSin.zero;
				public CachedDegreesToSin upperEyesLimitPitchUp = CachedDegreesToSin.zero;
				public CachedDegreesToSin upperEyesLimitPitchDown = CachedDegreesToSin.zero;
				public CachedDegreesToCos upperEyesTraceTheta = CachedDegreesToCos.zero;

				public CachedDegreesToSin upperDirXLimitThetaY = CachedDegreesToSin.zero;

				public CachedScaledValue spineLimitAngleX = CachedScaledValue.zero; // MathUtil.DEG_2_RADF(Not sin)
				public CachedScaledValue spineLimitAngleY = CachedScaledValue.zero; // MathUtil.DEG_2_RADF(Not sin)

				public CachedRate01 upperContinuousPreTranslateRate = CachedRate01.zero;
				public CachedRate01 upperContinuousPreTranslateStableRate = CachedRate01.zero;
				public CachedRate01 upperContinuousCenterLegRotationStableRate = CachedRate01.zero;
				public CachedRate01 upperContinuousPostTranslateStableRate = CachedRate01.zero;

				public void Update(Settings.BodyIK settingsBodyIK) {
					// Optimize: Reduce C# fuction call.
					Assert(settingsBodyIK != null);

					if (shoulderLimitThetaYPlus._degrees != settingsBodyIK.shoulderLimitAngleYPlus) {
						shoulderLimitThetaYPlus._Reset(settingsBodyIK.shoulderLimitAngleYPlus);
					}
					if (shoulderLimitThetaYMinus._degrees != settingsBodyIK.shoulderLimitAngleYMinus) {
						shoulderLimitThetaYMinus._Reset(settingsBodyIK.shoulderLimitAngleYMinus);
					}
					if (shoulderLimitThetaZ._degrees != settingsBodyIK.shoulderLimitAngleZ) {
						shoulderLimitThetaZ._Reset(settingsBodyIK.shoulderLimitAngleZ);
					}

					if (upperCenterLegTranslateRate._value != settingsBodyIK.upperCenterLegTranslateRate ||
						upperSpineTranslateRate._value != settingsBodyIK.upperSpineTranslateRate) {
						upperCenterLegTranslateRate._Reset(settingsBodyIK.upperCenterLegTranslateRate);
						upperSpineTranslateRate._Reset(MathF.Max(settingsBodyIK.upperCenterLegTranslateRate, settingsBodyIK.upperSpineTranslateRate));
					}

					if (upperPostTranslateRate._value != settingsBodyIK.upperPostTranslateRate) {
						upperPostTranslateRate._Reset(settingsBodyIK.upperPostTranslateRate);
					}

					if (upperCenterLegRotateRate._value != settingsBodyIK.upperCenterLegRotateRate ||
						upperSpineRotateRate._value != settingsBodyIK.upperSpineRotateRate) {
						upperCenterLegRotateRate._Reset(settingsBodyIK.upperCenterLegRotateRate);
						upperSpineRotateRate._Reset(MathF.Max(settingsBodyIK.upperCenterLegRotateRate, settingsBodyIK.upperSpineRotateRate));
						isFuzzyUpperCenterLegAndSpineRotationRate = IsFuzzy(upperCenterLegRotateRate.value, upperSpineRotateRate.value);
					}

					if (upperEyesLimitYaw._degrees != settingsBodyIK.upperEyesLimitYaw) {
						upperEyesLimitYaw._Reset(settingsBodyIK.upperEyesLimitYaw);
					}
					if (upperEyesLimitPitchUp._degrees != settingsBodyIK.upperEyesLimitPitchUp) {
						upperEyesLimitPitchUp._Reset(settingsBodyIK.upperEyesLimitPitchUp);
					}
					if (upperEyesLimitPitchDown._degrees != settingsBodyIK.upperEyesLimitPitchDown) {
						upperEyesLimitPitchDown._Reset(settingsBodyIK.upperEyesLimitPitchDown);
					}
					if (upperEyesTraceTheta._degrees != settingsBodyIK.upperEyesTraceAngle) {
						upperEyesTraceTheta._Reset(settingsBodyIK.upperEyesTraceAngle);
					}

					if (spineLimitAngleX._a != settingsBodyIK.spineLimitAngleX) {
						spineLimitAngleX._Reset(settingsBodyIK.spineLimitAngleX, MathUtil.DEG_2_RADF);
					}
					if (spineLimitAngleY._a != settingsBodyIK.spineLimitAngleY) {
						spineLimitAngleY._Reset(settingsBodyIK.spineLimitAngleY, MathUtil.DEG_2_RADF);
					}
					if (upperDirXLimitThetaY._degrees != settingsBodyIK.upperDirXLimitAngleY) {
						upperDirXLimitThetaY._Reset(settingsBodyIK.upperDirXLimitAngleY);
					}

					if (upperContinuousPreTranslateRate._value != settingsBodyIK.upperContinuousPreTranslateRate) {
						upperContinuousPreTranslateRate._Reset(settingsBodyIK.upperContinuousPreTranslateRate);
					}
					if (upperContinuousPreTranslateStableRate._value != settingsBodyIK.upperContinuousPreTranslateStableRate) {
						upperContinuousPreTranslateStableRate._Reset(settingsBodyIK.upperContinuousPreTranslateStableRate);
					}
					if (upperContinuousCenterLegRotationStableRate._value != settingsBodyIK.upperContinuousCenterLegRotationStableRate) {
						upperContinuousCenterLegRotationStableRate._Reset(settingsBodyIK.upperContinuousCenterLegRotationStableRate);
					}
					if (upperContinuousPostTranslateStableRate._value != settingsBodyIK.upperContinuousPostTranslateStableRate) {
						upperContinuousPostTranslateStableRate._Reset(settingsBodyIK.upperContinuousPostTranslateStableRate);
					}
				}
			}

			public class LimbIK
			{
				public CachedDegreesToSin armEffectorBackBeginTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin armEffectorBackCoreBeginTheta = CachedDegreesToSin.zero;
				public CachedDegreesToCos armEffectorBackCoreEndTheta = CachedDegreesToCos.zero;
				public CachedDegreesToCos armEffectorBackEndTheta = CachedDegreesToCos.zero;

				public CachedDegreesToSin armEffectorBackCoreUpperTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin armEffectorBackCoreLowerTheta = CachedDegreesToSin.zero;

				public CachedDegreesToSin elbowFrontInnerLimitTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin elbowBackInnerLimitTheta = CachedDegreesToSin.zero;

				public CachedDegreesToSin footLimitYawTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin footLimitPitchUpTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin footLimitPitchDownTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin footLimitRollTheta = CachedDegreesToSin.zero;

				public void Update(Settings.LimbIK settingsLimbIK) {
					// Optimize: Reduce C# fuction call.
					Assert(settingsLimbIK != null);

					if (armEffectorBackBeginTheta._degrees != settingsLimbIK.armEffectorBackBeginAngle) {
						armEffectorBackBeginTheta._Reset(settingsLimbIK.armEffectorBackBeginAngle);
					}
					if (armEffectorBackCoreBeginTheta._degrees != settingsLimbIK.armEffectorBackCoreBeginAngle) {
						armEffectorBackCoreBeginTheta._Reset(settingsLimbIK.armEffectorBackCoreBeginAngle);
					}
					if (armEffectorBackCoreEndTheta._degrees != settingsLimbIK.armEffectorBackCoreEndAngle) {
						armEffectorBackCoreEndTheta._Reset(settingsLimbIK.armEffectorBackCoreEndAngle);
					}
					if (armEffectorBackEndTheta._degrees != settingsLimbIK.armEffectorBackEndAngle) {
						armEffectorBackEndTheta._Reset(settingsLimbIK.armEffectorBackEndAngle);
					}

					if (armEffectorBackCoreUpperTheta._degrees != settingsLimbIK.armEffectorBackCoreUpperAngle) {
						armEffectorBackCoreUpperTheta._Reset(settingsLimbIK.armEffectorBackCoreUpperAngle);
					}
					if (armEffectorBackCoreLowerTheta._degrees != settingsLimbIK.armEffectorBackCoreLowerAngle) {
						armEffectorBackCoreLowerTheta._Reset(settingsLimbIK.armEffectorBackCoreLowerAngle);
					}

					if (elbowFrontInnerLimitTheta._degrees != settingsLimbIK.elbowFrontInnerLimitAngle) {
						elbowFrontInnerLimitTheta._Reset(settingsLimbIK.elbowFrontInnerLimitAngle);
					}
					if (elbowBackInnerLimitTheta._degrees != settingsLimbIK.elbowBackInnerLimitAngle) {
						elbowBackInnerLimitTheta._Reset(settingsLimbIK.elbowBackInnerLimitAngle);
					}

					if (footLimitYawTheta._degrees != settingsLimbIK.footLimitYaw) {
						footLimitYawTheta._Reset(settingsLimbIK.footLimitYaw);
					}
					if (footLimitPitchUpTheta._degrees != settingsLimbIK.footLimitPitchUp) {
						footLimitPitchUpTheta._Reset(settingsLimbIK.footLimitPitchUp);
					}
					if (footLimitPitchDownTheta._degrees != settingsLimbIK.footLimitPitchDown) {
						footLimitPitchDownTheta._Reset(settingsLimbIK.footLimitPitchDown);
					}
					if (footLimitRollTheta._degrees != settingsLimbIK.footLimitRoll) {
						footLimitRollTheta._Reset(settingsLimbIK.footLimitRoll);
					}
				}
			}

			public class HeadIK
			{
				public CachedDegreesToSin neckLimitPitchUpTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin neckLimitPitchDownTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin neckLimitRollTheta = CachedDegreesToSin.zero;

				public CachedDegreesToSin headLimitYawTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin headLimitPitchUpTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin headLimitPitchDownTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin headLimitRollTheta = CachedDegreesToSin.zero;

				public CachedDegreesToCos eyesTraceTheta = CachedDegreesToCos.zero;

				public CachedDegreesToSin eyesLimitYawTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin eyesLimitPitchTheta = CachedDegreesToSin.zero;

				public void Update(Settings.HeadIK settingsHeadIK) {
					Assert(settingsHeadIK != null);

					if (neckLimitPitchUpTheta._degrees != settingsHeadIK.neckLimitPitchUp) {
						neckLimitPitchUpTheta._Reset(settingsHeadIK.neckLimitPitchUp);
					}
					if (neckLimitPitchDownTheta._degrees != settingsHeadIK.neckLimitPitchDown) {
						neckLimitPitchDownTheta._Reset(settingsHeadIK.neckLimitPitchDown);
					}
					if (neckLimitRollTheta._degrees != settingsHeadIK.neckLimitRoll) {
						neckLimitRollTheta._Reset(settingsHeadIK.neckLimitRoll);
					}

					if (headLimitYawTheta._degrees != settingsHeadIK.headLimitYaw) {
						headLimitYawTheta._Reset(settingsHeadIK.headLimitYaw);
					}
					if (headLimitPitchUpTheta._degrees != settingsHeadIK.headLimitPitchUp) {
						headLimitPitchUpTheta._Reset(settingsHeadIK.headLimitPitchUp);
					}
					if (headLimitPitchDownTheta._degrees != settingsHeadIK.headLimitPitchDown) {
						headLimitPitchDownTheta._Reset(settingsHeadIK.headLimitPitchDown);
					}
					if (headLimitRollTheta._degrees != settingsHeadIK.headLimitRoll) {
						headLimitRollTheta._Reset(settingsHeadIK.headLimitRoll);
					}

					if (eyesTraceTheta._degrees != settingsHeadIK.eyesTraceAngle) {
						eyesTraceTheta._Reset(settingsHeadIK.eyesTraceAngle);
					}

					if (eyesLimitYawTheta._degrees != settingsHeadIK.eyesLimitYaw) {
						eyesLimitYawTheta._Reset(settingsHeadIK.eyesLimitYaw);
					}
					if (eyesLimitPitchTheta._degrees != settingsHeadIK.eyesLimitPitch) {
						eyesLimitPitchTheta._Reset(settingsHeadIK.eyesLimitPitch);
					}
				}
			}

			public BodyIK bodyIK = new();
			public LimbIK limbIK = new();
			public HeadIK headIK = new();
		}

		// Memo: Not Serializable
		public class BoneCaches
		{
			public struct HipsToFootLength
			{
				public Vector3f hipsToLeg;
				public Vector3f legToKnee;
				public Vector3f kneeToFoot;

				public Vector3f defaultOffset;
			}

			public HipsToFootLength[] hipsToFootLength = new HipsToFootLength[2];

			void PrepareHipsToFootLength(int index, Bone legBone, Bone kneeBone, Bone footBone, InternalValues internalValues) {
				Assert(internalValues != null);
				if (legBone != null && kneeBone != null && footBone != null) {
					var hipsToLegLen = legBone._defaultLocalLength.length;
					var legToKneeLen = kneeBone._defaultLocalLength.length;
					var kneeToFootLen = footBone._defaultLocalLength.length;

					var hipsToLegDir = legBone._defaultLocalDirection;
					var legToKneeDir = kneeBone._defaultLocalDirection;
					var kneeToFootDir = footBone._defaultLocalDirection;

					SAFBIKMatMultVec(out hipsToFootLength[index].hipsToLeg, ref internalValues.defaultRootBasisInv, ref hipsToLegDir);
					SAFBIKMatMultVec(out hipsToFootLength[index].legToKnee, ref internalValues.defaultRootBasisInv, ref legToKneeDir);
					SAFBIKMatMultVec(out hipsToFootLength[index].kneeToFoot, ref internalValues.defaultRootBasisInv, ref kneeToFootDir);

					hipsToFootLength[index].defaultOffset =
						(hipsToFootLength[index].hipsToLeg * hipsToLegLen) +
						(hipsToFootLength[index].legToKnee * legToKneeLen) +
						(hipsToFootLength[index].kneeToFoot * kneeToFootLen);
				}
			}

			Vector3f _GetHipsOffset(int index, Bone legBone, Bone kneeBone, Bone footBone) {
				if (legBone != null && kneeBone != null && footBone != null) {
					float hipsToLegLen = legBone._defaultLocalLength.length;
					float legToKneeLen = kneeBone._defaultLocalLength.length;
					float kneeToFootLen = footBone._defaultLocalLength.length;

					Vector3f currentOffset =
						hipsToFootLength[index].hipsToLeg * hipsToLegLen +
						hipsToFootLength[index].legToKnee * legToKneeLen +
						hipsToFootLength[index].kneeToFoot * kneeToFootLen;

					return currentOffset - hipsToFootLength[index].defaultOffset;
				}

				return Vector3f.Zero;
			}

			public Vector3f defaultHipsPosition = Vector3f.Zero;
			public Vector3f hipsOffset = Vector3f.Zero;

			public void Prepare(FullBodyIK fullBodyIK) {
				PrepareHipsToFootLength(0, fullBodyIK.leftLegBones.leg, fullBodyIK.leftLegBones.knee, fullBodyIK.leftLegBones.foot, fullBodyIK.internalValues);
				PrepareHipsToFootLength(1, fullBodyIK.rightLegBones.leg, fullBodyIK.rightLegBones.knee, fullBodyIK.rightLegBones.foot, fullBodyIK.internalValues);
				if (fullBodyIK.bodyBones.hips != null) {
					defaultHipsPosition = fullBodyIK.bodyBones.hips._defaultPosition;
				}
			}

			public void _SyncDisplacement(FullBodyIK fullBodyIK) {
				Assert(fullBodyIK != null);

				Vector3f hipsOffset0 = _GetHipsOffset(0, fullBodyIK.leftLegBones.leg, fullBodyIK.leftLegBones.knee, fullBodyIK.leftLegBones.foot);
				Vector3f hipsOffset1 = _GetHipsOffset(1, fullBodyIK.rightLegBones.leg, fullBodyIK.rightLegBones.knee, fullBodyIK.rightLegBones.foot);
				hipsOffset = (hipsOffset0 + hipsOffset1) * 0.5f;
			}
		}

		public ITransform rootIIKTransform;

		[System.NonSerialized]
		public InternalValues internalValues = new InternalValues();
		[System.NonSerialized]
		public BoneCaches boneCaches = new BoneCaches();

		public Settings settings;
		public EditorSettings editorSettings;

		public BodyBones bodyBones;
		public HeadBones headBones;
		public LegBones leftLegBones;
		public LegBones rightLegBones;
		public ArmBones leftArmBones;
		public ArmBones rightArmBones;
		public FingersBones leftHandFingersBones;
		public FingersBones rightHandFingersBones;

		public Effector rootEffector;
		public BodyEffectors bodyEffectors;
		public HeadEffectors headEffectors;
		public LegEffectors leftLegEffectors;
		public LegEffectors rightLegEffectors;
		public ArmEffectors leftArmEffectors;
		public ArmEffectors rightArmEffectors;
		public FingersEffectors leftHandFingersEffectors;
		public FingersEffectors rightHandFingersEffectors;

		public Bone[] bones { get { return _bones; } }
		public Effector[] effectors { get { return _effectors; } }

		Bone[] _bones = new Bone[(int)BoneType.Max];
		Effector[] _effectors = new Effector[(int)EffectorLocation.Max];

		BodyIK _bodyIK;
		LimbIK[] _limbIK = new LimbIK[(int)LimbIKLocation.Max];
		HeadIK _headIK;
		FingerIK[] _fingerIK = new FingerIK[(int)FingerIKType.Max];

		bool _isNeedFixShoulderWorldIIKTransform;

		bool _isPrefixed;
		bool _isPrepared;

		bool _isPrefixedAtLeastOnce;

		public void Awake(ITransform rootTransorm_) {
			if (rootIIKTransform != rootTransorm_) {
				rootIIKTransform = rootTransorm_;
			}

			_Prefix();
			Prepare();
		}

		static void _SetBoneIIKTransform(ref Bone bone, IIKBoneTransform transform) {
			if (bone == null) {
				bone = new Bone();
			}

			bone.transform = transform;
		}

		static void _SetFingerBoneIIKTransform(ref Bone[] bones, IIKBoneTransform[,] transforms, int index) {
			if (bones == null || bones.Length != MaxHandFingerLength) {
				bones = new Bone[MaxHandFingerLength];
			}

			for (int i = 0; i != MaxHandFingerLength; ++i) {
				if (bones[i] == null) {
					bones[i] = new Bone();
				}
				bones[i].transform = transforms[index, i];
			}
		}

		// - Call from Editor script.
		public void Prefix(ITransform rootIIKTransform_) {
			if (rootIIKTransform != rootIIKTransform_) {
				rootIIKTransform = rootIIKTransform_;
			}

			_Prefix();
		}

		// - Call from FullBodyIKBehaviour.Awake() / FullBodyIK.Initialize().
		// - Bone transforms are null yet.
		void _Prefix() {
			if (_isPrefixed) {
				return;
			}

			_isPrefixed = true;

			SafeNew(ref bodyBones);
			SafeNew(ref headBones);
			SafeNew(ref leftLegBones);
			SafeNew(ref rightLegBones);

			SafeNew(ref leftArmBones);
			leftArmBones.Repair();
			SafeNew(ref rightArmBones);
			rightArmBones.Repair();

			SafeNew(ref leftHandFingersBones);
			leftHandFingersBones.Repair();
			SafeNew(ref rightHandFingersBones);
			rightHandFingersBones.Repair();

			SafeNew(ref bodyEffectors);
			SafeNew(ref headEffectors);
			SafeNew(ref leftArmEffectors);
			SafeNew(ref rightArmEffectors);
			SafeNew(ref leftLegEffectors);
			SafeNew(ref rightLegEffectors);
			SafeNew(ref leftHandFingersEffectors);
			SafeNew(ref rightHandFingersEffectors);

			SafeNew(ref settings);
			SafeNew(ref editorSettings);
			SafeNew(ref internalValues);

			settings.Prefix();

			if (_bones == null || _bones.Length != (int)BoneLocation.Max) {
				_bones = new Bone[(int)BoneLocation.Max];
			}
			if (_effectors == null || _effectors.Length != (int)EffectorLocation.Max) {
				_effectors = new Effector[(int)EffectorLocation.Max];
			}

			_Prefix(ref bodyBones.hips, BoneLocation.Hips, null);
			_Prefix(ref bodyBones.spine, BoneLocation.Spine, bodyBones.hips);
			_Prefix(ref bodyBones.spine2, BoneLocation.Spine2, bodyBones.spine);
			_Prefix(ref bodyBones.spine3, BoneLocation.Spine3, bodyBones.spine2);
			_Prefix(ref bodyBones.spine4, BoneLocation.Spine4, bodyBones.spine3);
			_Prefix(ref headBones.neck, BoneLocation.Neck, bodyBones.SpineU);
			_Prefix(ref headBones.head, BoneLocation.Head, headBones.neck);
			_Prefix(ref headBones.leftEye, BoneLocation.LeftEye, headBones.head);
			_Prefix(ref headBones.rightEye, BoneLocation.RightEye, headBones.head);
			for (int i = 0; i != 2; ++i) {
				var legBones = (i == 0) ? leftLegBones : rightLegBones;
				_Prefix(ref legBones.leg, (i == 0) ? BoneLocation.LeftLeg : BoneLocation.RightLeg, bodyBones.hips);
				_Prefix(ref legBones.knee, (i == 0) ? BoneLocation.LeftKnee : BoneLocation.RightKnee, legBones.leg);
				_Prefix(ref legBones.foot, (i == 0) ? BoneLocation.LeftFoot : BoneLocation.RightFoot, legBones.knee);

				var armBones = (i == 0) ? leftArmBones : rightArmBones;
				_Prefix(ref armBones.shoulder, (i == 0) ? BoneLocation.LeftShoulder : BoneLocation.RightShoulder, bodyBones.SpineU);
				_Prefix(ref armBones.arm, (i == 0) ? BoneLocation.LeftArm : BoneLocation.RightArm, armBones.shoulder);
				_Prefix(ref armBones.elbow, (i == 0) ? BoneLocation.LeftElbow : BoneLocation.RightElbow, armBones.arm);
				_Prefix(ref armBones.wrist, (i == 0) ? BoneLocation.LeftWrist : BoneLocation.RightWrist, armBones.elbow);

				for (int n = 0; n != MaxArmRollLength; ++n) {
					var armRollLocation = (i == 0) ? BoneLocation.LeftArmRoll0 : BoneLocation.RightArmRoll0;
					_Prefix(ref armBones.armRoll[n], (BoneLocation)((int)armRollLocation + n), armBones.arm);
				}

				for (int n = 0; n != MaxElbowRollLength; ++n) {
					var elbowRollLocation = (i == 0) ? BoneLocation.LeftElbowRoll0 : BoneLocation.RightElbowRoll0;
					_Prefix(ref armBones.elbowRoll[n], (BoneLocation)((int)elbowRollLocation + n), armBones.elbow);
				}

				var fingerBones = (i == 0) ? leftHandFingersBones : rightHandFingersBones;
				for (int n = 0; n != MaxHandFingerLength; ++n) {
					var thumbLocation = (i == 0) ? BoneLocation.LeftHandThumb0 : BoneLocation.RightHandThumb0;
					var indexLocation = (i == 0) ? BoneLocation.LeftHandIndex0 : BoneLocation.RightHandIndex0;
					var middleLocation = (i == 0) ? BoneLocation.LeftHandMiddle0 : BoneLocation.RightHandMiddle0;
					var ringLocation = (i == 0) ? BoneLocation.LeftHandRing0 : BoneLocation.RightHandRing0;
					var littleLocation = (i == 0) ? BoneLocation.LeftHandLittle0 : BoneLocation.RightHandLittle0;
					_Prefix(ref fingerBones.thumb[n], (BoneLocation)((int)thumbLocation + n), (n == 0) ? armBones.wrist : fingerBones.thumb[n - 1]);
					_Prefix(ref fingerBones.index[n], (BoneLocation)((int)indexLocation + n), (n == 0) ? armBones.wrist : fingerBones.index[n - 1]);
					_Prefix(ref fingerBones.middle[n], (BoneLocation)((int)middleLocation + n), (n == 0) ? armBones.wrist : fingerBones.middle[n - 1]);
					_Prefix(ref fingerBones.ring[n], (BoneLocation)((int)ringLocation + n), (n == 0) ? armBones.wrist : fingerBones.ring[n - 1]);
					_Prefix(ref fingerBones.little[n], (BoneLocation)((int)littleLocation + n), (n == 0) ? armBones.wrist : fingerBones.little[n - 1]);
				}
			}

			_Prefix(ref rootEffector, EffectorLocation.Root);
			_Prefix(ref bodyEffectors.hips, EffectorLocation.Hips, rootEffector, bodyBones.hips, leftLegBones.leg, rightLegBones.leg);
			_Prefix(ref headEffectors.neck, EffectorLocation.Neck, bodyEffectors.hips, headBones.neck);
			_Prefix(ref headEffectors.head, EffectorLocation.Head, headEffectors.neck, headBones.head);
			_Prefix(ref headEffectors.eyes, EffectorLocation.Eyes, rootEffector, headBones.head, headBones.leftEye, headBones.rightEye);

			_Prefix(ref leftLegEffectors.knee, EffectorLocation.LeftKnee, rootEffector, leftLegBones.knee);
			_Prefix(ref leftLegEffectors.foot, EffectorLocation.LeftFoot, rootEffector, leftLegBones.foot);
			_Prefix(ref rightLegEffectors.knee, EffectorLocation.RightKnee, rootEffector, rightLegBones.knee);
			_Prefix(ref rightLegEffectors.foot, EffectorLocation.RightFoot, rootEffector, rightLegBones.foot);

			_Prefix(ref leftArmEffectors.arm, EffectorLocation.LeftArm, bodyEffectors.hips, leftArmBones.arm);
			_Prefix(ref leftArmEffectors.elbow, EffectorLocation.LeftElbow, bodyEffectors.hips, leftArmBones.elbow);
			_Prefix(ref leftArmEffectors.wrist, EffectorLocation.LeftWrist, bodyEffectors.hips, leftArmBones.wrist);
			_Prefix(ref rightArmEffectors.arm, EffectorLocation.RightArm, bodyEffectors.hips, rightArmBones.arm);
			_Prefix(ref rightArmEffectors.elbow, EffectorLocation.RightElbow, bodyEffectors.hips, rightArmBones.elbow);
			_Prefix(ref rightArmEffectors.wrist, EffectorLocation.RightWrist, bodyEffectors.hips, rightArmBones.wrist);

			_Prefix(ref leftHandFingersEffectors.thumb, EffectorLocation.LeftHandThumb, leftArmEffectors.wrist, leftHandFingersBones.thumb);
			_Prefix(ref leftHandFingersEffectors.index, EffectorLocation.LeftHandIndex, leftArmEffectors.wrist, leftHandFingersBones.index);
			_Prefix(ref leftHandFingersEffectors.middle, EffectorLocation.LeftHandMiddle, leftArmEffectors.wrist, leftHandFingersBones.middle);
			_Prefix(ref leftHandFingersEffectors.ring, EffectorLocation.LeftHandRing, leftArmEffectors.wrist, leftHandFingersBones.ring);
			_Prefix(ref leftHandFingersEffectors.little, EffectorLocation.LeftHandLittle, leftArmEffectors.wrist, leftHandFingersBones.little);

			_Prefix(ref rightHandFingersEffectors.thumb, EffectorLocation.RightHandThumb, rightArmEffectors.wrist, rightHandFingersBones.thumb);
			_Prefix(ref rightHandFingersEffectors.index, EffectorLocation.RightHandIndex, rightArmEffectors.wrist, rightHandFingersBones.index);
			_Prefix(ref rightHandFingersEffectors.middle, EffectorLocation.RightHandMiddle, rightArmEffectors.wrist, rightHandFingersBones.middle);
			_Prefix(ref rightHandFingersEffectors.ring, EffectorLocation.RightHandRing, rightArmEffectors.wrist, rightHandFingersBones.ring);
			_Prefix(ref rightHandFingersEffectors.little, EffectorLocation.RightHandLittle, rightArmEffectors.wrist, rightHandFingersBones.little);

			if (!_isPrefixedAtLeastOnce) {
				_isPrefixedAtLeastOnce = true;
				for (int i = 0; i != _effectors.Length; ++i) {
					_effectors[i].Prefix();
				}
			}
		}

		public void ConfigureBoneIIKTransforms(
			IIKBoneTransform hips,
			IIKBoneTransform spine,
			IIKBoneTransform spine2,
			IIKBoneTransform spine3,
			IIKBoneTransform spine4,
			IIKBoneTransform neck,
			IIKBoneTransform head,
			IIKBoneTransform leftEye,
			IIKBoneTransform rightEye,
			IIKBoneTransform leftLeg,
			IIKBoneTransform rightLeg,
			IIKBoneTransform leftKnee,
			IIKBoneTransform rightKnee,
			IIKBoneTransform leftFoot,
			IIKBoneTransform rightFoot,
			IIKBoneTransform leftShoulder,
			IIKBoneTransform rightShoulder,
			IIKBoneTransform leftArm,
			IIKBoneTransform rightArm,
			IIKBoneTransform leftElbow,
			IIKBoneTransform rightElbow,
			IIKBoneTransform leftWrist,
			IIKBoneTransform rightWrist,
			IIKBoneTransform[,] leftFingers,
			IIKBoneTransform[,] rightFingers
			) {
			_Prefix();

			Assert(settings != null && rootIIKTransform != null);
			if (rootIIKTransform != null) {
				leftFingers ??= new IIKBoneTransform[5, 4];
				rightFingers ??= new IIKBoneTransform[5, 4];
				if(!(leftFingers.GetLength(0) == 5 && leftFingers.GetLength(1) == 4)) {
					throw new Exception(nameof(leftFingers) + " NEEDS to be [5, 4]");
				}
				if (!(rightFingers.GetLength(0) == 5 && rightFingers.GetLength(1) == 4)) {
					throw new Exception(nameof(rightFingers) + " NEEDS to be [5, 4]");
				}
				_SetBoneIIKTransform(ref bodyBones.hips, hips);
				_SetBoneIIKTransform(ref bodyBones.spine, spine);
				_SetBoneIIKTransform(ref bodyBones.spine2, spine2);
				_SetBoneIIKTransform(ref bodyBones.spine3, spine3);
				_SetBoneIIKTransform(ref bodyBones.spine4, spine4);

				_SetBoneIIKTransform(ref headBones.neck, neck);
				_SetBoneIIKTransform(ref headBones.head, head);
				_SetBoneIIKTransform(ref headBones.leftEye, leftEye);
				_SetBoneIIKTransform(ref headBones.rightEye, rightEye);

				_SetBoneIIKTransform(ref leftLegBones.leg, leftLeg);
				_SetBoneIIKTransform(ref leftLegBones.knee, leftKnee);
				_SetBoneIIKTransform(ref leftLegBones.foot, leftFoot);
				_SetBoneIIKTransform(ref rightLegBones.leg, rightLeg);
				_SetBoneIIKTransform(ref rightLegBones.knee, rightKnee);
				_SetBoneIIKTransform(ref rightLegBones.foot, rightFoot);

				_SetBoneIIKTransform(ref leftArmBones.shoulder, leftShoulder);
				_SetBoneIIKTransform(ref leftArmBones.arm, leftArm);
				_SetBoneIIKTransform(ref leftArmBones.elbow, leftElbow);
				_SetBoneIIKTransform(ref leftArmBones.wrist, leftWrist);
				_SetBoneIIKTransform(ref rightArmBones.shoulder, rightShoulder);
				_SetBoneIIKTransform(ref rightArmBones.arm, rightArm);
				_SetBoneIIKTransform(ref rightArmBones.elbow, rightElbow);
				_SetBoneIIKTransform(ref rightArmBones.wrist, rightWrist);

				_SetFingerBoneIIKTransform(ref leftHandFingersBones.thumb, leftFingers, 0);
				_SetFingerBoneIIKTransform(ref leftHandFingersBones.index, leftFingers, 1);
				_SetFingerBoneIIKTransform(ref leftHandFingersBones.middle, leftFingers, 2);
				_SetFingerBoneIIKTransform(ref leftHandFingersBones.ring, leftFingers, 3);
				_SetFingerBoneIIKTransform(ref leftHandFingersBones.little, leftFingers, 4);

				_SetFingerBoneIIKTransform(ref rightHandFingersBones.thumb, rightFingers, 0);
				_SetFingerBoneIIKTransform(ref rightHandFingersBones.index, rightFingers, 1);
				_SetFingerBoneIIKTransform(ref rightHandFingersBones.middle, rightFingers, 2);
				_SetFingerBoneIIKTransform(ref rightHandFingersBones.ring, rightFingers, 3);
				_SetFingerBoneIIKTransform(ref rightHandFingersBones.little, rightFingers, 4);

			}
		}

		// - Wakeup for solvers.
		// - Require to setup each transforms.
		public bool Prepare() {
			_Prefix();

			if (_isPrepared) {
				return false;
			}

			_isPrepared = true;

			Assert(rootIIKTransform != null);
			if (rootIIKTransform != null) { // Failsafe.
				internalValues.defaultRootPosition = rootIIKTransform.position;
				internalValues.defaultRootBasis = Matrix3x3.FromColumn(rootIIKTransform.right, rootIIKTransform.up, rootIIKTransform.forward);
				internalValues.defaultRootBasisInv = internalValues.defaultRootBasis.transpose;
				internalValues.defaultRootRotation = rootIIKTransform.rotation;
			}

			if (_bones != null) {
				int boneLength = _bones.Length;
				for (int i = 0; i != boneLength; ++i) {
					Assert(_bones[i] != null);
					if (_bones[i] != null) {
						_bones[i].Prepare(this);
					}
				}
				for (int i = 0; i != boneLength; ++i) {
					if (_bones[i] != null) {
						_bones[i].PostPrepare();
					}
				}
			}

			boneCaches.Prepare(this);

			if (_effectors != null) {
				int effectorLength = _effectors.Length;
				for (int i = 0; i != effectorLength; ++i) {
					Assert(_effectors[i] != null);
					if (_effectors[i] != null) {
						_effectors[i].Prepare(this);
					}
				}
			}

			if (_limbIK == null || _limbIK.Length != (int)LimbIKLocation.Max) {
				_limbIK = new LimbIK[(int)LimbIKLocation.Max];
			}

			for (int i = 0; i != (int)LimbIKLocation.Max; ++i) {
				_limbIK[i] = new LimbIK(this, (LimbIKLocation)i);
			}

			_bodyIK = new BodyIK(this, _limbIK);
			_headIK = new HeadIK(this);

			if (_fingerIK == null || _fingerIK.Length != (int)FingerIKType.Max) {
				_fingerIK = new FingerIK[(int)FingerIKType.Max];
			}

			for (int i = 0; i != (int)FingerIKType.Max; ++i) {
				_fingerIK[i] = new FingerIK(this, (FingerIKType)i);
			}

			{
				Bone neckBone = headBones.neck;
				Bone leftShoulder = leftArmBones.shoulder;
				Bone rightShoulder = rightArmBones.shoulder;
				if (leftShoulder != null && leftShoulder.transformIsAlive &&
					rightShoulder != null && rightShoulder.transformIsAlive &&
					neckBone != null && neckBone.transformIsAlive) {
					if (leftShoulder.transform.parent == neckBone.transform &&
						rightShoulder.transform.parent == neckBone.transform) {
						_isNeedFixShoulderWorldIIKTransform = true;
					}
				}
			}

			return true;
		}

		void _UpdateInternalValues() {
			// _animatorEnabledImmediately

			internalValues.resetIIKTransforms = true;

			internalValues.continuousSolverEnabled = !internalValues.animatorEnabled && !internalValues.resetIIKTransforms;

			internalValues.bodyIK.Update(settings.bodyIK);
			internalValues.limbIK.Update(settings.limbIK);
			internalValues.headIK.Update(settings.headIK);
		}

		bool _isSyncDisplacementAtLeastOnce = false;

		void _Bones_SyncDisplacement() {
			// Sync Displacement.
			if (settings.syncDisplacement != SyncDisplacement.Disable) {
				if (settings.syncDisplacement == SyncDisplacement.Everyframe || !_isSyncDisplacementAtLeastOnce) {
					_isSyncDisplacementAtLeastOnce = true;

					if (_bones != null) {
						int boneLength = _bones.Length;
						for (int i = 0; i != boneLength; ++i) {
							if (_bones[i] != null) {
								_bones[i].SyncDisplacement();
							}
						}

						// for Hips
						boneCaches._SyncDisplacement(this);

						for (int i = 0; i != boneLength; ++i) {
							if (_bones[i] != null) {
								_bones[i].PostSyncDisplacement(this);
							}
						}

						for (int i = 0; i != boneLength; ++i) {
							if (_bones[i] != null) {
								_bones[i].PostPrepare();
							}
						}
					}

					// Forceupdate _defaultPosition / _defaultRotation
					if (_effectors != null) {
						int effectorLength = _effectors.Length;
						for (int i = 0; i != effectorLength; ++i) {
							if (_effectors[i] != null) {
								_effectors[i]._ComputeDefaultIIKTransform(this);
							}
						}
					}
				}
			}
		}

		// for effector._hidden_worldPosition / BodyIK
		void _ComputeBaseHipsIIKTransform() {
			Assert(internalValues != null);

			if (bodyEffectors == null) { // Note: bodyEffectors is public.
				return;
			}

			Effector hipsEffector = bodyEffectors.hips;
			if (hipsEffector == null || rootEffector == null) {
				return;
			}

			if (hipsEffector.rotationEnabled && hipsEffector.rotationWeight > IKEpsilon) {
				Quaternionf hipsRotation = hipsEffector.worldRotation * Inverse(hipsEffector._defaultRotation);
				if (hipsEffector.rotationWeight < 1.0f - IKEpsilon) {
					Quaternionf rootRotation = rootEffector.worldRotation * Inverse(rootEffector._defaultRotation);
					Quaternionf tempRotation = Quaternionf.Slerp(rootRotation, hipsRotation, hipsEffector.rotationWeight);
					SAFBIKMatSetRot(out internalValues.baseHipsBasis, ref tempRotation);
				}
				else {
					SAFBIKMatSetRot(out internalValues.baseHipsBasis, ref hipsRotation);
				}
			}
			else {
				Quaternionf rootEffectorWorldRotation = rootEffector.worldRotation;
				SAFBIKMatSetRotMultInv1(out internalValues.baseHipsBasis, ref rootEffectorWorldRotation, ref rootEffector._defaultRotation);
			}

			if (hipsEffector.positionEnabled && hipsEffector.positionWeight > IKEpsilon) {
				Vector3f hipsEffectorWorldPosition = hipsEffector.worldPosition;
				internalValues.baseHipsPos = hipsEffectorWorldPosition;
				if (hipsEffector.positionWeight < 1.0f - IKEpsilon) {
					Vector3f rootEffectorWorldPosition = rootEffector.worldPosition;
					Vector3f hipsPosition;
					SAFBIKMatMultVecPreSubAdd(
						out hipsPosition,
						ref internalValues.baseHipsBasis,
						ref hipsEffector._defaultPosition,
						ref rootEffector._defaultPosition,
						ref rootEffectorWorldPosition);
					internalValues.baseHipsPos = Vector3f.Lerp(hipsPosition, internalValues.baseHipsPos, hipsEffector.positionWeight);
				}
			}
			else {
				Vector3f rootEffectorWorldPosition = rootEffector.worldPosition;
				SAFBIKMatMultVecPreSubAdd(
					out internalValues.baseHipsPos,
					ref internalValues.baseHipsBasis,
					ref hipsEffector._defaultPosition,
					ref rootEffector._defaultPosition,
					ref rootEffectorWorldPosition);
			}
		}

		public void Update() {
			_UpdateInternalValues();

			if (_effectors != null) {
				int effectorLength = _effectors.Length;
				for (int i = 0; i != effectorLength; ++i) {
					if (_effectors[i] != null) {
						_effectors[i].PrepareUpdate();
					}
				}
			}

			_Bones_PrepareUpdate();

			_Bones_SyncDisplacement();

			if (internalValues.resetIIKTransforms || internalValues.continuousSolverEnabled) {
				_ComputeBaseHipsIIKTransform();
			}

			// Feedback bonePositions to effectorPositions.
			// (for AnimatorEnabled only.)
			if (_effectors != null) {
				int effectorLength = _effectors.Length;
				for (int i = 0; i != effectorLength; ++i) {
					Effector effector = _effectors[i];
					if (effector != null) {
						// todo: Optimize. (for BodyIK)

						// LimbIK : bending / end
						// BodyIK :  wrist / foot / neck
						// FingerIK : nothing
						if (effector.effectorType == EffectorType.Eyes ||
							effector.effectorType == EffectorType.HandFinger) { // Optimize.
						}
						else {
							float weight = effector.positionEnabled ? effector.positionWeight : 0.0f;
							Vector3f destPosition = (weight > IKEpsilon) ? effector.worldPosition : new Vector3f();
							if (weight < 1.0f - IKEpsilon) {
								Vector3f sourcePosition = destPosition; // Failsafe.
								if (!internalValues.animatorEnabled && (internalValues.resetIIKTransforms || internalValues.continuousSolverEnabled)) {
									if (effector.effectorLocation == EffectorLocation.Hips) {
										sourcePosition = internalValues.baseHipsPos; // _ComputeBaseHipsIIKTransform()
									}
									else {
										Effector hipsEffector = (bodyEffectors != null) ? bodyEffectors.hips : null;
										if (hipsEffector != null) {
											SAFBIKMatMultVecPreSubAdd(
												out sourcePosition,
												ref internalValues.baseHipsBasis,
												ref effector._defaultPosition,
												ref hipsEffector._defaultPosition,
												ref internalValues.baseHipsPos);
										}
									}
								}
								else { // for Animation.
									if (effector.bone != null && effector.bone.transformIsAlive) {
										sourcePosition = effector.bone.worldPosition;
									}
								}

								if (weight > IKEpsilon) {
									effector._hidden_worldPosition = Vector3f.Lerp(sourcePosition, destPosition, weight);
								}
								else {
									effector._hidden_worldPosition = sourcePosition;
								}
							}
							else {
								effector._hidden_worldPosition = destPosition;
							}
						}
					}
				}
			}

			// Presolve locations.
			if (_limbIK != null) {
				int limbIKLength = _limbIK.Length;
				for (int i = 0; i != limbIKLength; ++i) {
					if (_limbIK[i] != null) {
						_limbIK[i].PresolveBeinding();
					}
				}
			}

			if (_bodyIK != null) {
				if (_bodyIK.Solve()) {
					_Bones_WriteToIIKTransform();
				}
			}

			// todo: Force overwrite _hidden_worldPosition (LimbIK, arms)

			// settings.
			//		public bool legAlwaysSolveEnabled = true;
			//		public bool armAlwaysSolveEnabled = false;

			if (_limbIK != null || _headIK != null) {
				_Bones_PrepareUpdate();

				bool isSolved = false;
				bool isHeadSolved = false;
				if (_limbIK != null) {
					int limbIKLength = _limbIK.Length;
					for (int i = 0; i != limbIKLength; ++i) {
						if (_limbIK[i] != null) {
							isSolved |= _limbIK[i].Solve();
						}
					}
				}
				if (_headIK != null) {
					isHeadSolved = _headIK.Solve(this);
					isSolved |= isHeadSolved;
				}

				if (isHeadSolved && _isNeedFixShoulderWorldIIKTransform) {
					if (leftArmBones.shoulder != null) {
						leftArmBones.shoulder.forcefix_worldRotation();
					}
					if (rightArmBones.shoulder != null) {
						rightArmBones.shoulder.forcefix_worldRotation();
					}
				}

				if (isSolved) {
					_Bones_WriteToIIKTransform();
				}
			}

			if (_fingerIK != null) {
				_Bones_PrepareUpdate();

				bool isSolved = false;
				int fingerIKLength = _fingerIK.Length;
				for (int i = 0; i != fingerIKLength; ++i) {
					if (_fingerIK[i] != null) {
						isSolved |= _fingerIK[i].Solve();
					}
				}

				if (isSolved) {
					_Bones_WriteToIIKTransform();
				}
			}
		}

		void _Bones_PrepareUpdate() {
			if (_bones != null) {
				int boneLength = _bones.Length;
				for (int i = 0; i != boneLength; ++i) {
					if (_bones[i] != null) {
						_bones[i].PrepareUpdate();
					}
				}
			}
		}

		void _Bones_WriteToIIKTransform() {
			if (_bones != null) {
				int boneLength = _bones.Length;
				for (int i = 0; i != boneLength; ++i) {
					if (_bones[i] != null) {
						_bones[i].WriteToIIKTransform();
					}
				}
			}
		}

		void _Prefix(ref Bone bone, BoneLocation boneLocation, Bone parentBoneLocationBased) {
			Assert(_bones != null);
			Bone.Prefix(_bones, ref bone, boneLocation, parentBoneLocationBased);
		}

		void _Prefix(
			ref Effector effector,
			EffectorLocation effectorLocation) {
			Assert(_effectors != null);
			bool createEffectorIIKTransform = this.settings.createEffectorIIKTransform;
			Assert(rootIIKTransform != null);
			Effector.Prefix(_effectors, ref effector, effectorLocation, createEffectorIIKTransform, rootIIKTransform);
		}

		void _Prefix(
			ref Effector effector,
			EffectorLocation effectorLocation,
			Effector parentEffector,
			Bone[] bones) {
			_Prefix(ref effector, effectorLocation, parentEffector, (bones != null && bones.Length > 0) ? bones[bones.Length - 1] : null);
		}

		void _Prefix(
			ref Effector effector,
			EffectorLocation effectorLocation,
			Effector parentEffector,
			Bone bone,
			Bone leftBone = null,
			Bone rightBone = null) {
			Assert(_effectors != null);
			bool createEffectorIIKTransform = this.settings.createEffectorIIKTransform;
			Effector.Prefix(_effectors, ref effector, effectorLocation, createEffectorIIKTransform, null, parentEffector, bone, leftBone, rightBone);
		}

		//----------------------------------------------------------------------------------------------------------------------------

		// Custom Solver.
		public virtual bool _IsHiddenCustomEyes() {
			return false;
		}

		public virtual bool _PrepareCustomEyes(ref Quaternionf headToLeftEyeRotation, ref Quaternionf headToRightEyeRotation) {
			return false;
		}

		public virtual void _ResetCustomEyes() {
		}

		public virtual void _SolveCustomEyes(ref Matrix3x3 neckBasis, ref Matrix3x3 headBasis, ref Matrix3x3 headBaseBasis) {
		}
	}
}
