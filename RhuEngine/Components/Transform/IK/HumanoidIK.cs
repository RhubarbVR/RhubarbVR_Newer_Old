// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;

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
		public void SetUp(Entity transform) {
			Prefix(transform);
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			Awake(Entity);
		}

		protected override void Step() {
			Update();
		}

		public sealed class LimbIK
		{
			struct RollBone
			{
				public Bone bone;
				public float rate;
			}

			private readonly Settings _settings;
			private readonly InternalValues _internalValues;

			public LimbIKLocation _limbIKLocation;
			private readonly LimbIKType _limbIKType;
			private readonly Side _limbIKSide;
			private readonly Bone _beginBone;
			private readonly Bone _bendingBone;
			private readonly Bone _endBone;
			private readonly Effector _bendingEffector;
			private readonly Effector _endEffector;
			private readonly RollBone[] _armRollBones;
			private readonly RollBone[] _elbowRollBones;

			public float _beginToBendingLength;
			public float _beginToBendingLengthSq;
			public float _bendingToEndLength;
			public float _bendingToEndLengthSq;

			private IKMatrix3x3 _beginToBendingBoneBasis = IKMatrix3x3.identity;
			private Quaternionf _endEffectorToWorldRotation = Quaternionf.Identity;

			private IKMatrix3x3 _effectorToBeginBoneBasis = IKMatrix3x3.identity;
			private float _defaultSinTheta = 0.0f;
			private float _defaultCosTheta = 1.0f;

			private float _beginToEndMaxLength = 0.0f;
			private CachedScaledValue _effectorMaxLength = CachedScaledValue.zero;
			private CachedScaledValue _effectorMinLength = CachedScaledValue.zero;

			private float _leg_upperLimitNearCircleZ = 0.0f;
			private float _leg_upperLimitNearCircleY = 0.0f;

			private CachedScaledValue _arm_elbowBasisForcefixEffectorLengthBegin = CachedScaledValue.zero;
			private CachedScaledValue _arm_elbowBasisForcefixEffectorLengthEnd = CachedScaledValue.zero;

			// for Arm roll.
			private IKMatrix3x3 _arm_bendingToBeginBoneBasis = IKMatrix3x3.identity;
			private Quaternionf _arm_bendingWorldToBeginBoneRotation = Quaternionf.Identity;
			// for Hand roll.
			private Quaternionf _arm_endWorldToBendingBoneRotation = Quaternionf.Identity;
			// for Arm/Hand roll.(Temporary)
			private bool _arm_isSolvedLimbIK;
			private IKMatrix3x3 _arm_solvedBeginBoneBasis = IKMatrix3x3.identity;
			private IKMatrix3x3 _arm_solvedBendingBoneBasis = IKMatrix3x3.identity;

			public LimbIK(HumanoidIK fullBodyIK, LimbIKLocation limbIKLocation) {
				Assert(fullBodyIK != null);
				if (fullBodyIK == null) {
					return;
				}

				_settings = fullBodyIK.settings;
				_internalValues = fullBodyIK.internalValues;

				_limbIKLocation = limbIKLocation;
				_limbIKType = ToLimbIKType(limbIKLocation);
				_limbIKSide = ToLimbIKSide(limbIKLocation);

				if (_limbIKType == LimbIKType.Leg) {
					var legBones = _limbIKSide == Side.Left ? fullBodyIK.leftLegBones : fullBodyIK.rightLegBones;
					var legEffectors = _limbIKSide == Side.Left ? fullBodyIK.leftLegEffectors : fullBodyIK.rightLegEffectors;
					_beginBone = legBones.leg;
					_bendingBone = legBones.knee;
					_endBone = legBones.foot;
					_bendingEffector = legEffectors.knee;
					_endEffector = legEffectors.foot;
				}
				else if (_limbIKType == LimbIKType.Arm) {
					var armBones = _limbIKSide == Side.Left ? fullBodyIK.leftArmBones : fullBodyIK.rightArmBones;
					var armEffectors = _limbIKSide == Side.Left ? fullBodyIK.leftArmEffectors : fullBodyIK.rightArmEffectors;
					_beginBone = armBones.arm;
					_bendingBone = armBones.elbow;
					_endBone = armBones.wrist;
					_bendingEffector = armEffectors.elbow;
					_endEffector = armEffectors.wrist;
					PrepareRollBones(ref _armRollBones, armBones.armRoll);
					PrepareRollBones(ref _elbowRollBones, armBones.elbowRoll);
				}

				Prepare();
			}

			void Prepare() {
				IKMath.QuatMultInv0(out _endEffectorToWorldRotation, _endEffector._defaultRotation, _endBone._defaultRotation);

				// for _defaultCosTheta, _defaultSinTheta
				_beginToBendingLength = _bendingBone._defaultLocalLength.length;
				_beginToBendingLengthSq = _bendingBone._defaultLocalLength.lengthSq;
				_bendingToEndLength = _endBone._defaultLocalLength.length;
				_bendingToEndLengthSq = _endBone._defaultLocalLength.lengthSq;

				float beginToEndLength;
				beginToEndLength = IKMath.VecLengthAndLengthSq2(out var beginToEndLengthSq,
					 _endBone._defaultPosition, _beginBone._defaultPosition);

				_defaultCosTheta = IKMath.ComputeCosTheta(
					_bendingToEndLengthSq,          // lenASq
					beginToEndLengthSq,             // lenBSq
					_beginToBendingLengthSq,        // lenCSq
					beginToEndLength,               // lenB
					_beginToBendingLength);        // lenC

				_defaultSinTheta = IKMath.SqrtClamp01(1.0f - _defaultCosTheta * _defaultCosTheta);
				CheckNaN(_defaultSinTheta);
			}

			bool _isSyncDisplacementAtLeastOnce;

			void SyncDisplacement() {
				// Require to call before _UpdateArgs()

				// Measure bone length.(Using worldPosition)
				// Force execution on 1st time. (Ignore case _settings.syncDisplacement == SyncDisplacement.Disable)
				if (_settings.syncDisplacement == HumanoidIK.SyncDisplacement.Everyframe || !_isSyncDisplacementAtLeastOnce) {
					_isSyncDisplacementAtLeastOnce = true;

					IKMath.MatMult(out _beginToBendingBoneBasis, _beginBone._localAxisBasisInv, _bendingBone._localAxisBasis);

					if (_armRollBones != null) {
						if (_beginBone != null && _bendingBone != null) {
							IKMath.MatMult(out _arm_bendingToBeginBoneBasis, _bendingBone._boneToBaseBasis, _beginBone._baseToBoneBasis);
							IKMath.MatMultGetRot(out _arm_bendingWorldToBeginBoneRotation, _bendingBone._worldToBaseBasis, _beginBone._baseToBoneBasis);
						}
					}

					if (_elbowRollBones != null) {
						if (_endBone != null && _bendingBone != null) {
							IKMath.MatMultGetRot(out _arm_endWorldToBendingBoneRotation, _endBone._worldToBaseBasis, _bendingBone._baseToBoneBasis);
						}
					}

					_beginToBendingLength = _bendingBone._defaultLocalLength.length;
					_beginToBendingLengthSq = _bendingBone._defaultLocalLength.lengthSq;
					_bendingToEndLength = _endBone._defaultLocalLength.length;
					_bendingToEndLengthSq = _endBone._defaultLocalLength.lengthSq;
					_beginToEndMaxLength = _beginToBendingLength + _bendingToEndLength;

					var beginToEndDir = _endBone._defaultPosition - _beginBone._defaultPosition;
					if (IKMath.VecNormalize(ref beginToEndDir)) {
						if (_limbIKType == LimbIKType.Arm) {
							if (_limbIKSide == Side.Left) {
								beginToEndDir = -beginToEndDir;
							}
							var dirY = _internalValues.defaultRootBasis.column1;
							var dirZ = _internalValues.defaultRootBasis.column2;
							if (IKMath.ComputeBasisLockX(out _effectorToBeginBoneBasis, ref beginToEndDir, dirY, dirZ)) {
								_effectorToBeginBoneBasis = _effectorToBeginBoneBasis.Transpose;
							}
						}
						else {
							beginToEndDir = -beginToEndDir;
							var dirX = _internalValues.defaultRootBasis.column0;
							var dirZ = _internalValues.defaultRootBasis.column2;
							// beginToEffectorBasis( identity to effectorDir(y) )
							if (IKMath.ComputeBasisLockY(out _effectorToBeginBoneBasis, dirX, ref beginToEndDir, dirZ)) {
								// effectorToBeginBasis( effectorDir(y) to identity )
								_effectorToBeginBoneBasis = _effectorToBeginBoneBasis.Transpose;
							}
						}

						// effectorToBeginBasis( effectorDir(y) to _beginBone._localAxisBasis )
						IKMath.MatMultRet0(ref _effectorToBeginBoneBasis, _beginBone._localAxisBasis);
					}

					if (_limbIKType == LimbIKType.Leg) {
						_leg_upperLimitNearCircleZ = 0.0f;
						_leg_upperLimitNearCircleY = _beginToEndMaxLength;
					}

					// Forcereset args.
					SyncDisplacement_UpdateArgs();
				}
			}

			float _cache_legUpperLimitAngle = 0.0f;
			float _cache_kneeUpperLimitAngle = 0.0f;

			void UpdateArgs() {
				if (_limbIKType == LimbIKType.Leg) {
					var effectorMinLengthRate = _settings.limbIK.legEffectorMinLengthRate;
					if (_effectorMinLength._b != effectorMinLengthRate) {
						_effectorMinLength.Reset(_beginToEndMaxLength, effectorMinLengthRate);
					}

					if (_cache_kneeUpperLimitAngle != _settings.limbIK.prefixKneeUpperLimitAngle ||
						_cache_legUpperLimitAngle != _settings.limbIK.prefixLegUpperLimitAngle) {
						_cache_kneeUpperLimitAngle = _settings.limbIK.prefixKneeUpperLimitAngle;
						_cache_legUpperLimitAngle = _settings.limbIK.prefixLegUpperLimitAngle;

						// Memo: Their CachedDegreesToCosSin aren't required caching. (Use instantly.)
						var kneeUpperLimitTheta = new CachedDegreesToCosSin(_settings.limbIK.prefixKneeUpperLimitAngle);
						var legUpperLimitTheta = new CachedDegreesToCosSin(_settings.limbIK.prefixLegUpperLimitAngle);

						_leg_upperLimitNearCircleZ = _beginToBendingLength * legUpperLimitTheta.cos
													+ _bendingToEndLength * kneeUpperLimitTheta.cos;

						_leg_upperLimitNearCircleY = _beginToBendingLength * legUpperLimitTheta.sin
													+ _bendingToEndLength * kneeUpperLimitTheta.sin;
					}
				}

				if (_limbIKType == LimbIKType.Arm) {
					var beginRate = _settings.limbIK.armBasisForcefixEffectorLengthRate - _settings.limbIK.armBasisForcefixEffectorLengthLerpRate;
					var endRate = _settings.limbIK.armBasisForcefixEffectorLengthRate;
					if (_arm_elbowBasisForcefixEffectorLengthBegin._b != beginRate) {
						_arm_elbowBasisForcefixEffectorLengthBegin.Reset(_beginToEndMaxLength, beginRate);
					}
					if (_arm_elbowBasisForcefixEffectorLengthEnd._b != endRate) {
						_arm_elbowBasisForcefixEffectorLengthEnd.Reset(_beginToEndMaxLength, endRate);
					}
				}

				var effectorMaxLengthRate = _limbIKType == LimbIKType.Leg ? _settings.limbIK.legEffectorMaxLengthRate : _settings.limbIK.armEffectorMaxLengthRate;
				if (_effectorMaxLength._b != effectorMaxLengthRate) {
					_effectorMaxLength.Reset(_beginToEndMaxLength, effectorMaxLengthRate);
				}
			}

			void SyncDisplacement_UpdateArgs() {
				if (_limbIKType == LimbIKType.Leg) {
					var effectorMinLengthRate = _settings.limbIK.legEffectorMinLengthRate;
					_effectorMinLength.Reset(_beginToEndMaxLength, effectorMinLengthRate);

					// Memo: Their CachedDegreesToCosSin aren't required caching. (Use instantly.)
					var kneeUpperLimitTheta = new CachedDegreesToCosSin(_settings.limbIK.prefixKneeUpperLimitAngle);
					var legUpperLimitTheta = new CachedDegreesToCosSin(_settings.limbIK.prefixLegUpperLimitAngle);

					_leg_upperLimitNearCircleZ = _beginToBendingLength * legUpperLimitTheta.cos
												+ _bendingToEndLength * kneeUpperLimitTheta.cos;

					_leg_upperLimitNearCircleY = _beginToBendingLength * legUpperLimitTheta.sin
												+ _bendingToEndLength * kneeUpperLimitTheta.sin;
				}

				var effectorMaxLengthRate = _limbIKType == LimbIKType.Leg ? _settings.limbIK.legEffectorMaxLengthRate : _settings.limbIK.armEffectorMaxLengthRate;
				_effectorMaxLength.Reset(_beginToEndMaxLength, effectorMaxLengthRate);
			}

			// for animatorEnabled
			bool _isPresolvedBending = false;
			IKMatrix3x3 _presolvedBendingBasis = IKMatrix3x3.identity;
			Vector3f _presolvedEffectorDir = Vector3f.Zero;
			float _presolvedEffectorLength = 0.0f;

			// effectorDir to beginBoneBasis
			void SolveBaseBasis(out IKMatrix3x3 baseBasis, ref IKMatrix3x3 parentBaseBasis, ref Vector3f effectorDir) {
				if (_limbIKType == LimbIKType.Arm) {
					var dirX = _limbIKSide == Side.Left ? -effectorDir : effectorDir;
					var basisY = parentBaseBasis.column1;
					var basisZ = parentBaseBasis.column2;
					if (IKMath.ComputeBasisLockX(out baseBasis, ref dirX, basisY, basisZ)) {
						IKMath.MatMultRet0(ref baseBasis, _effectorToBeginBoneBasis);
					}
					else { // Failsafe.(Counts as default effectorDir.)
						IKMath.MatMult(out baseBasis, parentBaseBasis, _beginBone._localAxisBasis);
					}
				}
				else {
					var dirY = -effectorDir;
					var basisX = parentBaseBasis.column0;
					var basisZ = parentBaseBasis.column2;
					if (IKMath.ComputeBasisLockY(out baseBasis, basisX, ref dirY, basisZ)) {
						IKMath.MatMultRet0(ref baseBasis, _effectorToBeginBoneBasis);
					}
					else { // Failsafe.(Counts as default effectorDir.)
						IKMath.MatMult(out baseBasis, parentBaseBasis, _beginBone._localAxisBasis);
					}
				}
			}

			static void PrepareRollBones(ref RollBone[] rollBones, ConstArrayFour<Bone> bones) {
				if (bones != null && bones.Length > 0) {
					var length = bones.Length;
					var t = 1.0f / (length + 1);
					var r = t;
					rollBones = new RollBone[length];
					for (var i = 0; i < length; ++i, r += t) {
						rollBones[i].bone = bones[i];
						rollBones[i].rate = r;
					}
				}
				else {
					rollBones = null;
				}
			}

			public void PresolveBeinding() {
				SyncDisplacement();

				var presolvedEnabled = _limbIKType == LimbIKType.Leg ? _settings.limbIK.presolveKneeEnabled : _settings.limbIK.presolveElbowEnabled;
				if (!presolvedEnabled) {
					return;
				}

				_isPresolvedBending = false;

				if (_beginBone == null ||
					!_beginBone.TransformIsAlive ||
					_beginBone.ParentBone == null ||
					!_beginBone.ParentBone.TransformIsAlive ||
					_bendingEffector == null ||
					_bendingEffector.Bone == null ||
					!_bendingEffector.Bone.TransformIsAlive ||
					_endEffector == null ||
					_endEffector.Bone == null ||
					!_endEffector.Bone.TransformIsAlive) {
					return; // Failsafe.
				}

				if (!_internalValues.animatorEnabled) {
					return; // No require.
				}

				if (_bendingEffector.positionEnabled) {
					return; // No require.
				}

				if (_limbIKType == LimbIKType.Leg) {
					if (_settings.limbIK.presolveKneeRate < IKMath.IK_EPSILON) {
						return; // No effect.
					}
				}
				else {
					if (_settings.limbIK.presolveElbowRate < IKMath.IK_EPSILON) {
						return; // No effect.
					}
				}

				var beginPos = _beginBone.WorldPosition;
				var bendingPos = _bendingEffector.Bone.WorldPosition;
				var effectorPos = _endEffector.Bone.WorldPosition;
				var effectorTrans = effectorPos - beginPos;
				var bendingTrans = bendingPos - beginPos;

				var effectorLen = effectorTrans.Magnitude;
				var bendingLen = bendingTrans.Magnitude;
				if (effectorLen <= IKMath.IK_EPSILON || bendingLen <= IKMath.IK_EPSILON) {
					return;
				}

				var effectorDir = effectorTrans * (1.0f / effectorLen);
				var bendingDir = bendingTrans * (1.0f / bendingLen);

				var parentBoneWorldRotation = _beginBone.ParentBone.WorldRotation;

				IKMath.MatSetRotMult(out var parentBaseBasis, parentBoneWorldRotation, _beginBone.ParentBone._worldToBaseRotation);

				// Solve EffectorDir Based Basis.
				SolveBaseBasis(out var baseBasis, ref parentBaseBasis, ref effectorDir);

				_presolvedEffectorDir = effectorDir;
				_presolvedEffectorLength = effectorLen;

				IKMatrix3x3 toBasis;
				if (_limbIKType == LimbIKType.Arm) {
					var dirX = _limbIKSide == Side.Left ? -bendingDir : bendingDir;
					var basisY = parentBaseBasis.column1;
					var basisZ = parentBaseBasis.column2;
					if (IKMath.ComputeBasisLockX(out toBasis, ref dirX, basisY, basisZ)) {
						IKMath.MatMultInv1(out _presolvedBendingBasis, ref toBasis, ref baseBasis);
						_isPresolvedBending = true;
					}
				}
				else {
					var dirY = -bendingDir;
					var basisX = parentBaseBasis.column0;
					var basisZ = parentBaseBasis.column2;
					if (IKMath.ComputeBasisLockY(out toBasis, basisX, ref dirY, basisZ)) {
						IKMath.MatMultInv1(out _presolvedBendingBasis, ref toBasis, ref baseBasis);
						_isPresolvedBending = true;
					}
				}
			}

			//------------------------------------------------------------------------------------------------------------

			bool PrefixLegEffectorPos_UpperNear(ref Vector3f localEffectorTrans) {
				var y = localEffectorTrans.y - _leg_upperLimitNearCircleY;
				var z = localEffectorTrans.z;

				var rZ = _leg_upperLimitNearCircleZ;
				var rY = _leg_upperLimitNearCircleY + _effectorMinLength.value;

				if (rZ > IKMath.IK_EPSILON && rY > IKMath.IK_EPSILON) {
					var isLimited = false;

					z /= rZ;
					if (y > _leg_upperLimitNearCircleY) {
						isLimited = true;
					}
					else {
						y /= rY;
						var len = IKMath.Sqrt(y * y + z * z);
						if (len < 1.0f) {
							isLimited = true;
						}
					}

					if (isLimited) {
						var n = IKMath.Sqrt(1.0f - z * z);
						if (n > IKMath.IK_EPSILON) { // Memo: Upper only.
							localEffectorTrans.y = -n * rY + _leg_upperLimitNearCircleY;
						}
						else { // Failsafe.
							localEffectorTrans.z = 0.0f;
							localEffectorTrans.y = -_effectorMinLength.value;
						}
						return true;
					}
				}

				return false;
			}

			static bool PrefixLegEffectorPos_Circular_Far(ref Vector3f localEffectorTrans, float effectorLength) {
				return PrefixLegEffectorPos_Circular(ref localEffectorTrans, effectorLength, true);
			}

			static bool PrefixLegEffectorPos_Circular(ref Vector3f localEffectorTrans, float effectorLength, bool isFar) {
				var y = localEffectorTrans.y;
				var z = localEffectorTrans.z;
				var len = IKMath.Sqrt(y * y + z * z);
				if (isFar && len > effectorLength || !isFar && len < effectorLength) {
					var n = IKMath.Sqrt(effectorLength * effectorLength - localEffectorTrans.z * localEffectorTrans.z);
					if (n > IKMath.IK_EPSILON) { // Memo: Lower only.
						localEffectorTrans.y = -n;
					}
					else { // Failsafe.
						localEffectorTrans.z = 0.0f;
						localEffectorTrans.y = -effectorLength;
					}

					return true;
				}

				return false;
			}

			static bool PrefixLegEffectorPos_Upper_Circular_Far(ref Vector3f localEffectorTrans,
				float centerPositionZ,
				float effectorLengthZ, float effectorLengthY) {
				if (effectorLengthY > IKMath.IK_EPSILON && effectorLengthZ > IKMath.IK_EPSILON) {
					var y = localEffectorTrans.y;
					var z = localEffectorTrans.z - centerPositionZ;

					y /= effectorLengthY;
					z /= effectorLengthZ;

					var len = IKMath.Sqrt(y * y + z * z);
					if (len > 1.0f) {
						var n = IKMath.Sqrt(1.0f - z * z);
						if (n > IKMath.IK_EPSILON) { // Memo: Upper only.
							localEffectorTrans.y = n * effectorLengthY;
						}
						else { // Failsafe.
							localEffectorTrans.z = centerPositionZ;
							localEffectorTrans.y = effectorLengthY;
						}

						return true;
					}
				}

				return false;
			}

			//------------------------------------------------------------------------------------------------------------

			// for Arms.

			const float LOCAL_DIR_MAX_THETA = 0.99f;
			const float LOCAL_DIR_LERP_THETA = 0.01f;

			// Lefthand based.
			static void ComputeLocalDirXZ(ref Vector3f localDir, out Vector3f localDirXZ) {
				if (localDir.y >= LOCAL_DIR_MAX_THETA - IKMath.IK_EPSILON) {
					localDirXZ = new Vector3f(1.0f, 0.0f, 0.0f);
				}
				else if (localDir.y > LOCAL_DIR_MAX_THETA - LOCAL_DIR_LERP_THETA - IKMath.IK_EPSILON) {
					var r = (localDir.y - (LOCAL_DIR_MAX_THETA - LOCAL_DIR_LERP_THETA)) * (1.0f / LOCAL_DIR_LERP_THETA);
					localDirXZ = new Vector3f(localDir.x + (1.0f - localDir.x) * r, 0.0f, localDir.z - localDir.z * r);
					if (!IKMath.VecNormalizeXZ(ref localDirXZ)) {
						localDirXZ = new Vector3f(1.0f, 0.0f, 0.0f);
					}
				}
				else if (localDir.y <= -LOCAL_DIR_MAX_THETA + IKMath.IK_EPSILON) {
					localDirXZ = new Vector3f(-1.0f, 0.0f, 0.0f);
				}
				else if (localDir.y < -(LOCAL_DIR_MAX_THETA - LOCAL_DIR_LERP_THETA - IKMath.IK_EPSILON)) {
					var r = (-(LOCAL_DIR_MAX_THETA - LOCAL_DIR_LERP_THETA) - localDir.y) * (1.0f / LOCAL_DIR_LERP_THETA);
					localDirXZ = new Vector3f(localDir.x + (-1.0f - localDir.x) * r, 0.0f, localDir.z - localDir.z * r);
					if (!IKMath.VecNormalizeXZ(ref localDirXZ)) {
						localDirXZ = new Vector3f(-1.0f, 0.0f, 0.0f);
					}
				}
				else {
					localDirXZ = new Vector3f(localDir.x, 0.0f, localDir.z);
					if (!IKMath.VecNormalizeXZ(ref localDirXZ)) {
						localDirXZ = new Vector3f(1.0f, 0.0f, 0.0f);
					}
				}
			}

			// Lefthand based.
			static void ComputeLocalDirYZ(ref Vector3f localDir, out Vector3f localDirYZ) {
				if (localDir.x >= LOCAL_DIR_MAX_THETA - IKMath.IK_EPSILON) {
					localDirYZ = new Vector3f(0.0f, 0.0f, -1.0f);
				}
				else if (localDir.x > LOCAL_DIR_MAX_THETA - LOCAL_DIR_LERP_THETA - IKMath.IK_EPSILON) {
					var r = (localDir.x - (LOCAL_DIR_MAX_THETA - LOCAL_DIR_LERP_THETA)) * (1.0f / LOCAL_DIR_LERP_THETA);
					localDirYZ = new Vector3f(0.0f, localDir.y - localDir.y * r, localDir.z + (-1.0f - localDir.z) * r);
					if (!IKMath.VecNormalizeYZ(ref localDirYZ)) {
						localDirYZ = new Vector3f(0.0f, 0.0f, -1.0f);
					}
				}
				else if (localDir.x <= -LOCAL_DIR_MAX_THETA + IKMath.IK_EPSILON) {
					localDirYZ = new Vector3f(0.0f, 0.0f, 1.0f);
				}
				else if (localDir.x < -(LOCAL_DIR_MAX_THETA - LOCAL_DIR_LERP_THETA - IKMath.IK_EPSILON)) {
					var r = (-(LOCAL_DIR_MAX_THETA - LOCAL_DIR_LERP_THETA) - localDir.x) * (1.0f / LOCAL_DIR_LERP_THETA);
					localDirYZ = new Vector3f(0.0f, localDir.y - localDir.y * r, localDir.z + (1.0f - localDir.z) * r);
					if (!IKMath.VecNormalizeYZ(ref localDirYZ)) {
						localDirYZ = new Vector3f(0.0f, 0.0f, 1.0f);
					}
				}
				else {
					localDirYZ = new Vector3f(0.0f, localDir.y, localDir.z);
					if (!IKMath.VecNormalizeYZ(ref localDirYZ)) {
						localDirYZ = new Vector3f(0.0f, 0.0f, localDir.x >= 0.0f ? -1.0f : 1.0f);
					}
				}
			}

			//------------------------------------------------------------------------------------------------------------

			CachedDegreesToCos _presolvedLerpTheta = CachedDegreesToCos.zero;
			CachedDegreesToCos _automaticKneeBaseTheta = CachedDegreesToCos.zero;
			CachedDegreesToCosSin _automaticArmElbowTheta = CachedDegreesToCosSin.zero;

			//------------------------------------------------------------------------------------------------------------

			public bool IsSolverEnabled() {
				if (!_endEffector.positionEnabled && !(_bendingEffector.positionEnabled && _bendingEffector.pull > IKMath.IK_EPSILON)) {
					if (_limbIKType == LimbIKType.Arm) {
						if (!_settings.limbIK.armAlwaysSolveEnabled) {
							return false;
						}
					}
					else if (_limbIKType == LimbIKType.Leg) {
						if (!_settings.limbIK.legAlwaysSolveEnabled) {
							return false;
						}
					}
				}

				return true;
			}

			public bool Presolve(
				ref IKMatrix3x3 parentBaseBasis,
				ref Vector3f beginPos,
				out Vector3f solvedBeginToBendingDir,
				out Vector3f solvedBendingToEndDir) {
				return PresolveInternal(ref parentBaseBasis, ref beginPos, out var effectorLen, out var baseBasis, out solvedBeginToBendingDir, out solvedBendingToEndDir);
			}

			public bool PresolveInternal(
				ref IKMatrix3x3 parentBaseBasis,
				ref Vector3f beginPos,
				out float effectorLen,
				out IKMatrix3x3 baseBasis,
				out Vector3f solvedBeginToBendingDir,
				out Vector3f solvedBendingToEndDir) {
				solvedBeginToBendingDir = Vector3f.Zero;
				solvedBendingToEndDir = Vector3f.Zero;

				var bendingPos = _bendingEffector._hidden_worldPosition;
				var effectorPos = _endEffector._hidden_worldPosition;

				if (_bendingEffector.positionEnabled && _bendingEffector.pull > IKMath.IK_EPSILON) {
					var beginToBending = bendingPos - beginPos;
					var beginToBendingLenSq = beginToBending.SqrMagnitude;
					if (beginToBendingLenSq > _bendingBone._defaultLocalLength.length) {
						var beginToBendingLen = IKMath.Sqrt(beginToBendingLenSq);
						var tempLen = beginToBendingLen - _bendingBone._defaultLocalLength.length;
						if (tempLen < -IKMath.IK_EPSILON && beginToBendingLen > IKMath.IK_EPSILON) {
							bendingPos += beginToBending * (tempLen / beginToBendingLen);
						}
					}
				}

				if (_bendingEffector.positionEnabled && _bendingEffector.pull > IKMath.IK_EPSILON) {
					var bendingToEffector = effectorPos - bendingPos;
					var bendingToEffectorLen = bendingToEffector.Magnitude;
					if (bendingToEffectorLen > IKMath.IK_EPSILON) {
						var tempLen = _endBone._defaultLocalLength.length - bendingToEffectorLen;
						if (tempLen is > IKMath.IK_EPSILON or < (-IKMath.IK_EPSILON)) {
							var pull = _endEffector.positionEnabled && _endEffector.pull > IKMath.IK_EPSILON
								? _bendingEffector.pull / (_bendingEffector.pull + _endEffector.pull)
								: _bendingEffector.pull;
							effectorPos += bendingToEffector * (tempLen * pull / bendingToEffectorLen);
						}
					}
				}

				var parentBaseBasisInv = parentBaseBasis.Transpose;

				var effectorTrans = effectorPos - beginPos;

				effectorLen = effectorTrans.Magnitude;
				if (effectorLen <= IKMath.IK_EPSILON) {
					baseBasis = IKMatrix3x3.identity;
					return false;
				}
				if (_effectorMaxLength.value <= IKMath.IK_EPSILON) {
					baseBasis = IKMatrix3x3.identity;
					return false;
				}

				var effectorDir = effectorTrans * (1.0f / effectorLen);

				if (effectorLen > _effectorMaxLength.value) {
					effectorTrans = effectorDir * _effectorMaxLength.value;
					effectorPos = beginPos + effectorTrans;
					effectorLen = _effectorMaxLength.value;
				}

				var localEffectorDir = new Vector3f(0.0f, 0.0f, 1.0f);
				if (_limbIKType == LimbIKType.Arm) {
					IKMath.MatMultVec(out localEffectorDir, parentBaseBasisInv, effectorDir);
				}

				// pending: Detail processing for Arm too.
				if (_limbIKType == LimbIKType.Leg && _settings.limbIK.prefixLegEffectorEnabled) { // Override Effector Pos.
					IKMath.MatMultVec(out var localEffectorTrans, parentBaseBasisInv, effectorTrans);

					var isProcessed = false;
					var isLimited = false;
					if (localEffectorTrans.z >= 0.0f) { // Front
						if (localEffectorTrans.z >= _beginToBendingLength + _bendingToEndLength) { // So far.
							isProcessed = true;
							localEffectorTrans.z = _beginToBendingLength + _bendingToEndLength;
							localEffectorTrans.y = 0.0f;
						}

						if (!isProcessed &&
							localEffectorTrans.y >= -_effectorMinLength.value &&
							localEffectorTrans.z <= _leg_upperLimitNearCircleZ) { // Upper(Near)
							isProcessed = true;
							isLimited = PrefixLegEffectorPos_UpperNear(ref localEffectorTrans);
						}

						if (!isProcessed &&
							localEffectorTrans.y >= 0.0f &&
							localEffectorTrans.z > _leg_upperLimitNearCircleZ) { // Upper(Far)
							isProcessed = true;
							PrefixLegEffectorPos_Upper_Circular_Far(ref localEffectorTrans,
								_leg_upperLimitNearCircleZ,
								_beginToBendingLength + _bendingToEndLength - _leg_upperLimitNearCircleZ,
								_leg_upperLimitNearCircleY);
						}

						if (!isProcessed) { // Lower
							isLimited = PrefixLegEffectorPos_Circular_Far(ref localEffectorTrans, _beginToBendingLength + _bendingToEndLength);
						}

					}
					else { // Back
						   // Pending: Detail Processing.
						if (localEffectorTrans.y >= -_effectorMinLength.value) {
							isLimited = true;
							localEffectorTrans.y = -_effectorMinLength.value;
						}
						else {
							isLimited = PrefixLegEffectorPos_Circular_Far(ref localEffectorTrans, _beginToBendingLength + _bendingToEndLength);
						}
					}

					if (isLimited) {

						IKMath.MatMultVec(out effectorTrans, parentBaseBasis, localEffectorTrans);
						effectorLen = effectorTrans.Magnitude;
						effectorPos = beginPos + effectorTrans;
						if (effectorLen > IKMath.IK_EPSILON) {
							effectorDir = effectorTrans * (1.0f / effectorLen);
						}

					}
				}

				//Matrix3x3 baseBasis;
				SolveBaseBasis(out baseBasis, ref parentBaseBasis, ref effectorDir);

				// Automatical bendingPos
				if (!_bendingEffector.positionEnabled) {
					var presolvedEnabled = _limbIKType == LimbIKType.Leg ? _settings.limbIK.presolveKneeEnabled.Value : _settings.limbIK.presolveElbowEnabled.Value;
					var presolvedBendingRate = _limbIKType == LimbIKType.Leg ? _settings.limbIK.presolveKneeRate.Value : _settings.limbIK.presolveElbowRate.Value;
					var presolvedLerpAngle = _limbIKType == LimbIKType.Leg ? _settings.limbIK.presolveKneeLerpAngle.Value : _settings.limbIK.presolveElbowLerpAngle.Value;
					var presolvedLerpLengthRate = _limbIKType == LimbIKType.Leg ? _settings.limbIK.presolveKneeLerpLengthRate.Value : _settings.limbIK.presolveElbowLerpLengthRate.Value;

					var presolvedBendingPos = Vector3f.Zero;

					if (presolvedEnabled && _isPresolvedBending) {
						if (_presolvedEffectorLength > IKMath.IK_EPSILON) {
							var lerpLength = _presolvedEffectorLength * presolvedLerpLengthRate;
							if (lerpLength > IKMath.IK_EPSILON) {
								var tempLength = MathF.Abs(_presolvedEffectorLength - effectorLen);
								if (tempLength < lerpLength) {
									presolvedBendingRate *= 1.0f - tempLength / lerpLength;
								}
								else {
									presolvedBendingRate = 0.0f;
								}
							}
							else { // Failsafe.
								presolvedBendingRate = 0.0f;
							}
						}
						else { // Failsafe.
							presolvedBendingRate = 0.0f;
						}

						if (presolvedBendingRate > IKMath.IK_EPSILON) {
							if (_presolvedLerpTheta._degrees != presolvedLerpAngle) {
								_presolvedLerpTheta.Reset(presolvedLerpAngle);
							}
							if (_presolvedLerpTheta.cos < 1.0f - IKMath.IK_EPSILON) { // Lerp
								var presolvedFeedbackTheta = Vector3f.Dot(effectorDir, _presolvedEffectorDir);
								if (presolvedFeedbackTheta > _presolvedLerpTheta.cos + IKMath.IK_EPSILON) {
									var presolvedFeedbackRate = (presolvedFeedbackTheta - _presolvedLerpTheta.cos) / (1.0f - _presolvedLerpTheta.cos);
									presolvedBendingRate *= presolvedFeedbackRate;
								}
								else {
									presolvedBendingRate = 0.0f;
								}
							}
							else {
								presolvedBendingRate = 0.0f;
							}
						}

						if (presolvedBendingRate > IKMath.IK_EPSILON) {
							Vector3f bendingDir;
							IKMath.MatMult(out var presolvedBendingBasis, baseBasis, _presolvedBendingBasis);

							bendingDir = _limbIKType == LimbIKType.Arm
								? _limbIKSide == Side.Left ? -presolvedBendingBasis.column0 : presolvedBendingBasis.column0
								: -presolvedBendingBasis.column1;

							presolvedBendingPos = beginPos + bendingDir * _beginToBendingLength;
							bendingPos = presolvedBendingPos; // Failsafe.
						}
					}
					else {
						presolvedBendingRate = 0.0f;
					}

					if (presolvedBendingRate < 1.0f - IKMath.IK_EPSILON) {
						var cosTheta = IKMath.ComputeCosTheta(
							_bendingToEndLengthSq,          // lenASq
							effectorLen * effectorLen,      // lenBSq
							_beginToBendingLengthSq,        // lenCSq
							effectorLen,                    // lenB
							_beginToBendingLength);        // lenC

						var sinTheta = IKMath.SqrtClamp01(1.0f - cosTheta * cosTheta);

						var moveC = _beginToBendingLength * (1.0f - MathF.Max(_defaultCosTheta - cosTheta, 0.0f));
						var moveS = _beginToBendingLength * MathF.Max(sinTheta - _defaultSinTheta, 0.0f);

						if (_limbIKType == LimbIKType.Arm) {
							var dirX = _limbIKSide == Side.Left ? -baseBasis.column0 : baseBasis.column0;
							{
								var elbowBaseAngle = _settings.limbIK.automaticElbowBaseAngle.Value;
								var elbowLowerAngle = _settings.limbIK.automaticElbowLowerAngle.Value;
								var elbowUpperAngle = _settings.limbIK.automaticElbowUpperAngle.Value;

								var elbowAngle = elbowBaseAngle;

								var localDir = _limbIKSide == Side.Left ? localEffectorDir : new Vector3f(-localEffectorDir.x, localEffectorDir.y, localEffectorDir.z);

								elbowAngle = localDir.y < 0.0f ? MathUtil.Lerp(elbowAngle, elbowLowerAngle, -localDir.y) : MathUtil.Lerp(elbowAngle, elbowUpperAngle, localDir.y);

								if (_settings.limbIK.armEffectorBackfixEnabled) {
									var elbowBackUpperAngle = _settings.limbIK.automaticElbowBackUpperAngle;
									var elbowBackLowerAngle = _settings.limbIK.automaticElbowBackLowerAngle;

									// Based on localXZ
									var armEffectorBackBeginSinTheta = _internalValues.limbIK.armEffectorBackBeginTheta.sin;
									var armEffectorBackCoreBeginSinTheta = _internalValues.limbIK.armEffectorBackCoreBeginTheta.sin;
									var armEffectorBackCoreEndCosTheta = _internalValues.limbIK.armEffectorBackCoreEndTheta.cos;
									var armEffectorBackEndCosTheta = _internalValues.limbIK.armEffectorBackEndTheta.cos;

									// Based on localYZ
									var armEffectorBackCoreUpperSinTheta = _internalValues.limbIK.armEffectorBackCoreUpperTheta.sin;
									var armEffectorBackCoreLowerSinTheta = _internalValues.limbIK.armEffectorBackCoreLowerTheta.sin;

									// X is reversed in RightSide.
									ComputeLocalDirXZ(ref localDir, out var localXZ); // Lefthand Based.
									ComputeLocalDirYZ(ref localDir, out var localYZ); // Lefthand Based.

									if (localXZ.z < armEffectorBackBeginSinTheta &&
										localXZ.x > armEffectorBackEndCosTheta) {

										float targetAngle;
										if (localYZ.y >= armEffectorBackCoreUpperSinTheta) {
											targetAngle = elbowBackUpperAngle;
										}
										else if (localYZ.y <= armEffectorBackCoreLowerSinTheta) {
											targetAngle = elbowBackLowerAngle;
										}
										else {
											var t = armEffectorBackCoreUpperSinTheta - armEffectorBackCoreLowerSinTheta;
											if (t > IKMath.IK_EPSILON) {
												var r = (localYZ.y - armEffectorBackCoreLowerSinTheta) / t;
												targetAngle = MathUtil.Lerp(elbowBackLowerAngle, elbowBackUpperAngle, r);
											}
											else {
												targetAngle = elbowBackLowerAngle;
											}
										}

										if (localXZ.x < armEffectorBackCoreEndCosTheta) {
											var t = armEffectorBackCoreEndCosTheta - armEffectorBackEndCosTheta;
											if (t > IKMath.IK_EPSILON) {
												var r = (localXZ.x - armEffectorBackEndCosTheta) / t;

												if (localYZ.y <= armEffectorBackCoreLowerSinTheta) {
													elbowAngle = MathUtil.Lerp(elbowAngle, targetAngle, r);
												}
												else if (localYZ.y >= armEffectorBackCoreUpperSinTheta) {
													elbowAngle = MathUtil.Lerp(elbowAngle, targetAngle - 360.0f, r);
												}
												else {
													var angle0 = MathUtil.Lerp(elbowAngle, targetAngle, r); // Lower
													var angle1 = MathUtil.Lerp(elbowAngle, targetAngle - 360.0f, r); // Upper
													var t2 = armEffectorBackCoreUpperSinTheta - armEffectorBackCoreLowerSinTheta;
													if (t2 > IKMath.IK_EPSILON) {
														var r2 = (localYZ.y - armEffectorBackCoreLowerSinTheta) / t2;
														if (angle0 - angle1 > 180.0f) {
															angle1 += 360.0f;
														}

														elbowAngle = MathUtil.Lerp(angle0, angle1, r2);
													}
													else { // Failsafe.
														elbowAngle = angle0;
													}
												}
											}
										}
										else if (localXZ.z > armEffectorBackCoreBeginSinTheta) {
											var t = armEffectorBackBeginSinTheta - armEffectorBackCoreBeginSinTheta;
											if (t > IKMath.IK_EPSILON) {
												var r = (armEffectorBackBeginSinTheta - localXZ.z) / t;
												elbowAngle = localDir.y >= 0.0f ? MathUtil.Lerp(elbowAngle, targetAngle, r) : MathUtil.Lerp(elbowAngle, targetAngle - 360.0f, r);
											}
											else { // Failsafe.
												elbowAngle = targetAngle;
											}
										}
										else {
											elbowAngle = targetAngle;
										}
									}
								}

								var dirY = parentBaseBasis.column1;
								var dirZ = Vector3f.Cross(baseBasis.column0, dirY);
								dirY = Vector3f.Cross(dirZ, baseBasis.column0);
								if (!IKMath.VecNormalize2(ref dirY, ref dirZ)) { // Failsafe.
									dirY = parentBaseBasis.column1;
									dirZ = parentBaseBasis.column2;
								}

								if (_automaticArmElbowTheta._degrees != elbowAngle) {
									_automaticArmElbowTheta.Reset(elbowAngle);
								}

								bendingPos = beginPos + dirX * moveC
									+ _automaticArmElbowTheta.cos * moveS * -dirY
									+ _automaticArmElbowTheta.sin * moveS * -dirZ;
							}
						}
						else { // Leg
							var automaticKneeBaseAngle = _settings.limbIK.automaticKneeBaseAngle.Value;
							if (automaticKneeBaseAngle is >= (-IKMath.IK_EPSILON) and <= IKMath.IK_EPSILON) { // Fuzzy 0
								bendingPos = beginPos + -baseBasis.column1 * moveC + baseBasis.column2 * moveS;
							}
							else {
								if (_automaticKneeBaseTheta._degrees != automaticKneeBaseAngle) {
									_automaticKneeBaseTheta.Reset(automaticKneeBaseAngle);
								}

								var kneeSin = _automaticKneeBaseTheta.cos;
								var kneeCos = IKMath.Sqrt(1.0f - kneeSin * kneeSin);
								if (_limbIKSide == Side.Right) {
									if (automaticKneeBaseAngle >= 0.0f) {
										kneeCos = -kneeCos;
									}
								}
								else {
									if (automaticKneeBaseAngle < 0.0f) {
										kneeCos = -kneeCos;
									}
								}

								bendingPos = beginPos + -baseBasis.column1 * moveC
									+ kneeCos * moveS * baseBasis.column0
									+ kneeSin * moveS * baseBasis.column2;
							}
						}
					}

					if (presolvedBendingRate > IKMath.IK_EPSILON) {
						bendingPos = Vector3f.Lerp(bendingPos, presolvedBendingPos, presolvedBendingRate);
					}
				}

				var isSolved = false;

				{
					var beginToBendingTrans = bendingPos - beginPos;
					var intersectBendingTrans = beginToBendingTrans - effectorDir * Vector3f.Dot(effectorDir, beginToBendingTrans);
					var intersectBendingLen = intersectBendingTrans.Magnitude;

					if (intersectBendingLen > IKMath.IK_EPSILON) {
						var intersectBendingDir = intersectBendingTrans * (1.0f / intersectBendingLen);

						var bc2 = 2.0f * _beginToBendingLength * effectorLen;
						if (bc2 > IKMath.IK_EPSILON) {
							var effectorCosTheta = (_beginToBendingLengthSq + effectorLen * effectorLen - _bendingToEndLengthSq) / bc2;
							var effectorSinTheta = IKMath.SqrtClamp01(1.0f - effectorCosTheta * effectorCosTheta);

							var beginToInterTranslate = _beginToBendingLength * effectorCosTheta * effectorDir
															+ _beginToBendingLength * effectorSinTheta * intersectBendingDir;
							var interToEndTranslate = effectorPos - (beginPos + beginToInterTranslate);

							if (IKMath.VecNormalize2(ref beginToInterTranslate, ref interToEndTranslate)) {
								isSolved = true;
								solvedBeginToBendingDir = beginToInterTranslate;
								solvedBendingToEndDir = interToEndTranslate;
							}
						}
					}
				}

				if (isSolved && _limbIKType == LimbIKType.Arm && _settings.limbIK.armEffectorInnerfixEnabled) {
					var elbowFrontInnerLimitSinTheta = _internalValues.limbIK.elbowFrontInnerLimitTheta.sin;
					var elbowBackInnerLimitSinTheta = _internalValues.limbIK.elbowBackInnerLimitTheta.sin;

					IKMath.MatMultVec(out var localBendingDir, parentBaseBasisInv, solvedBeginToBendingDir);

					var isBack = localBendingDir.z < 0.0f;
					var limitTheta = isBack ? elbowBackInnerLimitSinTheta : elbowFrontInnerLimitSinTheta;

					var localX = _limbIKSide == Side.Left ? localBendingDir.x : -localBendingDir.x;
					if (localX > limitTheta) {
						localBendingDir.x = _limbIKSide == Side.Left ? limitTheta : -limitTheta;
						localBendingDir.z = IKMath.Sqrt(1.0f - (localBendingDir.x * localBendingDir.x + localBendingDir.y * localBendingDir.y));
						if (isBack) {
							localBendingDir.z = -localBendingDir.z;
						}
						IKMath.MatMultVec(out var bendingDir, parentBaseBasis, localBendingDir);
						var interPos = beginPos + bendingDir * _beginToBendingLength;
						var endDir = effectorPos - interPos;
						if (IKMath.VecNormalize(ref endDir)) {
							solvedBeginToBendingDir = bendingDir;
							solvedBendingToEndDir = endDir;

							if (_settings.limbIK.armBasisForcefixEnabled) { // Invalidate effectorLen.
								effectorLen = (effectorPos - beginPos).Magnitude;
							}
						}
					}
				}

				if (!isSolved) { // Failsafe.
					var bendingDir = bendingPos - beginPos;
					if (IKMath.VecNormalize(ref bendingDir)) {
						var interPos = beginPos + bendingDir * _beginToBendingLength;
						var endDir = effectorPos - interPos;
						if (IKMath.VecNormalize(ref endDir)) {
							isSolved = true;
							solvedBeginToBendingDir = bendingDir;
							solvedBendingToEndDir = endDir;
						}
					}
				}

				return isSolved;
			}

			//------------------------------------------------------------------------------------------------------------

			public bool Solve() {
				UpdateArgs();

				_arm_isSolvedLimbIK = false;

				var bendingBonePrevRotation = Quaternionf.Identity;
				var endBonePrevRotation = Quaternionf.Identity;
				if (!_internalValues.resetTransforms) {
					var endRotationWeight = _endEffector.rotationEnabled ? _endEffector.rotationWeight : 0.0f;
					if (endRotationWeight > IKMath.IK_EPSILON) {
						if (endRotationWeight < 1.0f - IKMath.IK_EPSILON) {
							bendingBonePrevRotation = _bendingBone.WorldRotation;
							endBonePrevRotation = _endBone.WorldRotation;
						}
					}
				}

				var r = SolveInternal();
				r |= SolveEndRotation(r, ref bendingBonePrevRotation, ref endBonePrevRotation);
				r |= RollInternal();

				return r;
			}

			public bool SolveInternal() {
				if (!IsSolverEnabled()) {
					return false;
				}

				if (_beginBone.ParentBone == null || !_beginBone.ParentBone.TransformIsAlive) {
					return false; // Failsafe.
				}

				var parentBoneWorldRotation = _beginBone.ParentBone.WorldRotation;
				IKMath.MatSetRotMult(out var parentBaseBasis, parentBoneWorldRotation, _beginBone.ParentBone._worldToBaseRotation);

				var beginPos = _beginBone.WorldPosition;

				if (!PresolveInternal(ref parentBaseBasis, ref beginPos, out var effectorLen, out var baseBasis, out var solvedBeginToBendingDir, out var solvedBendingToEndDir)) {
					return false;
				}

				IKMatrix3x3 bendingBasis;
				IKMatrix3x3 beginBasis;
				if (_limbIKType == LimbIKType.Arm) {
					// Memo: Arm Bone Based Y Axis.
					if (_limbIKSide == Side.Left) {
						solvedBeginToBendingDir = -solvedBeginToBendingDir;
						solvedBendingToEndDir = -solvedBendingToEndDir;
					}

					var basisY = parentBaseBasis.column1;
					var basisZ = parentBaseBasis.column2;
					if (!IKMath.ComputeBasisLockX(out beginBasis, ref solvedBeginToBendingDir, basisY, basisZ)) {
						return false;
					}

					{
						var forcefixEnabled = _settings.limbIK.armBasisForcefixEnabled;

						if (forcefixEnabled && effectorLen > _arm_elbowBasisForcefixEffectorLengthEnd.value) {
							IKMath.MatMultCol1(out basisY, beginBasis, _beginToBendingBoneBasis);
						}
						else {
							basisY = Vector3f.Cross(-solvedBeginToBendingDir, solvedBendingToEndDir); // Memo: Require to MaxEffectorLengthRate is less than 1.0
							if (_limbIKSide == Side.Left) {
								basisY = -basisY;
							}

							if (forcefixEnabled && effectorLen > _arm_elbowBasisForcefixEffectorLengthBegin.value) {
								var t = _arm_elbowBasisForcefixEffectorLengthEnd.value - _arm_elbowBasisForcefixEffectorLengthBegin.value;
								if (t > IKMath.IK_EPSILON) {
									var r = (effectorLen - _arm_elbowBasisForcefixEffectorLengthBegin.value) / t;
									IKMath.MatMultCol1(out var tempY, beginBasis, _beginToBendingBoneBasis);
									basisY = Vector3f.Lerp(basisY, tempY, r);
								}
							}
						}

						if (!IKMath.ComputeBasisFromXYLockX(out bendingBasis, ref solvedBendingToEndDir, basisY)) {
							return false;
						}
					}
				}
				else {
					// Memo: Leg Bone Based X Axis.
					solvedBeginToBendingDir = -solvedBeginToBendingDir;
					solvedBendingToEndDir = -solvedBendingToEndDir;

					var basisX = baseBasis.column0;
					var basisZ = baseBasis.column2;
					if (!IKMath.ComputeBasisLockY(out beginBasis, basisX, ref solvedBeginToBendingDir, basisZ)) {
						return false;
					}

					IKMath.MatMultCol0(out basisX, beginBasis, _beginToBendingBoneBasis);

					if (!IKMath.ComputeBasisFromXYLockY(out bendingBasis, basisX, ref solvedBendingToEndDir)) {
						return false;
					}
				}

				if (_limbIKType == LimbIKType.Arm) {
					_arm_isSolvedLimbIK = true;
					_arm_solvedBeginBoneBasis = beginBasis;
					_arm_solvedBendingBoneBasis = bendingBasis;
				}

				IKMath.MatMultGetRot(out var worldRotation, beginBasis, _beginBone._boneToWorldBasis);
				_beginBone.WorldRotation = worldRotation;
				IKMath.MatMultGetRot(out worldRotation, bendingBasis, _bendingBone._boneToWorldBasis);
				_bendingBone.WorldRotation = worldRotation;
				return true;
			}

			bool SolveEndRotation(bool isSolved, ref Quaternionf bendingBonePrevRotation, ref Quaternionf endBonePrevRotation) {
				var endRotationWeight = _endEffector.rotationEnabled ? _endEffector.rotationWeight : 0.0f;
				if (endRotationWeight > IKMath.IK_EPSILON) {
					var endEffectorWorldRotation = _endEffector.WorldRotation;
					IKMath.QuatMult(out var toRotation, endEffectorWorldRotation, _endEffectorToWorldRotation);

					if (endRotationWeight < 1.0f - IKMath.IK_EPSILON) {
						Quaternionf fromRotation;
						if (_internalValues.resetTransforms) {
							var bendingBoneWorldRotation = _bendingBone.WorldRotation;
							IKMath.QuatMult3(out fromRotation, bendingBoneWorldRotation, _bendingBone._worldToBaseRotation, _endBone._baseToWorldRotation);
						}
						else {
							if (isSolved) {
								var bendingBoneWorldRotation = _bendingBone.WorldRotation;
								IKMath.QuatMultNorm3Inv1(out fromRotation, bendingBoneWorldRotation, bendingBonePrevRotation, endBonePrevRotation);
							}
							else {
								fromRotation = endBonePrevRotation; // This is able to use endBonePrevRotation directly.
							}
						}
						_endBone.WorldRotation = Quaternionf.Slerp(fromRotation, toRotation, endRotationWeight);
					}
					else {
						_endBone.WorldRotation = toRotation;
					}

					EndRotationLimit();
					return true;
				}
				else {
					if (_internalValues.resetTransforms) {
						var bendingBoneWorldRotation = _bendingBone.WorldRotation;
						IKMath.QuatMult3(out var fromRotation, bendingBoneWorldRotation, _bendingBone._worldToBaseRotation, _endBone._baseToWorldRotation);
						_endBone.WorldRotation = fromRotation;
						return true;
					}
				}

				return false;
			}

			void EndRotationLimit() {
				if (_limbIKType == LimbIKType.Arm) {
					if (!_settings.limbIK.wristLimitEnabled) {
						return;
					}
				}
				else if (_limbIKType == LimbIKType.Leg) {
					if (!_settings.limbIK.footLimitEnabled) {
						return;
					}
				}
				// Rotation Limit.
				var tempRotation = _endBone.WorldRotation;
				IKMath.QuatMult(out var endRotation, tempRotation, _endBone._worldToBaseRotation);
				tempRotation = _bendingBone.WorldRotation;
				IKMath.QuatMult(out var bendingRotation, tempRotation, _bendingBone._worldToBaseRotation);
				IKMath.QuatMultInv0(out var localRotation, bendingRotation, endRotation);

				if (_limbIKType == LimbIKType.Arm) {
					var isLimited = false;
					var limitAngle = _settings.limbIK.wristLimitAngle;


					localRotation.ToAngleAxis(out var angle, out var axis);
					if (angle < -limitAngle) {
						angle = -limitAngle;
						isLimited = true;
					}
					else if (angle > limitAngle) {
						angle = limitAngle;
						isLimited = true;
					}

					if (isLimited) {
						localRotation = new Quaternionf(axis, angle);
						IKMath.QuatMultNorm3(out endRotation, bendingRotation, localRotation, _endBone._baseToWorldRotation);
						_endBone.WorldRotation = endRotation;
					}
				}
				else if (_limbIKType == LimbIKType.Leg) {
					IKMath.MatSetRot(out var localBasis, ref localRotation);

					var localDirY = localBasis.column1;
					var localDirZ = localBasis.column2;

					var isLimited = false;
					isLimited |= IKMath.LimitXZ_Square(ref localDirY,
						_internalValues.limbIK.footLimitRollTheta.sin,
						_internalValues.limbIK.footLimitRollTheta.sin,
						_internalValues.limbIK.footLimitPitchUpTheta.sin,
						_internalValues.limbIK.footLimitPitchDownTheta.sin);
					isLimited |= IKMath.LimitXY_Square(ref localDirZ,
						_internalValues.limbIK.footLimitYawTheta.sin,
						_internalValues.limbIK.footLimitYawTheta.sin,
						_internalValues.limbIK.footLimitPitchDownTheta.sin,
						_internalValues.limbIK.footLimitPitchUpTheta.sin);

					if (isLimited) {
						if (IKMath.ComputeBasisFromYZLockZ(out localBasis, localDirY, ref localDirZ)) {
							IKMath.MatGetRot(out localRotation, localBasis);
							IKMath.QuatMultNorm3(out endRotation, bendingRotation, localRotation, _endBone._baseToWorldRotation);
							_endBone.WorldRotation = endRotation;
						}
					}
				}
			}

			bool RollInternal() {
				if (_limbIKType != LimbIKType.Arm || !_settings.rollBonesEnabled) {
					return false;
				}

				var isSolved = false;

				if (_armRollBones != null && _armRollBones.Length > 0) {
					var boneLength = _armRollBones.Length;

					IKMatrix3x3 beginBoneBasis;
					IKMatrix3x3 bendingBoneBasis; // Attension: bendingBoneBasis is based on beginBoneBasis
					if (_arm_isSolvedLimbIK) {
						beginBoneBasis = _arm_solvedBeginBoneBasis;
						IKMath.MatMult(out bendingBoneBasis, _arm_solvedBendingBoneBasis, _arm_bendingToBeginBoneBasis);
					}
					else {
						beginBoneBasis = new IKMatrix3x3(_beginBone.WorldRotation * _beginBone._worldToBoneRotation);
						bendingBoneBasis = new IKMatrix3x3(_bendingBone.WorldRotation * _arm_bendingWorldToBeginBoneRotation);
					}

					var dirX = beginBoneBasis.column0;
					var dirY = bendingBoneBasis.column1;
					var dirZ = bendingBoneBasis.column2;


					if (IKMath.ComputeBasisLockX(out var bendingBasisTo, ref dirX, dirY, dirZ)) {
						IKMath.MatMult(out var baseBasis, beginBoneBasis, _beginBone._boneToBaseBasis);
						IKMath.MatMult(out var baseBasisTo, bendingBasisTo, _beginBone._boneToBaseBasis);

						for (var i = 0; i < boneLength; ++i) {
							if (_armRollBones[i].bone != null && _armRollBones[i].bone.TransformIsAlive) {
								var rate = _armRollBones[i].rate;
								IKMath.MatFastLerp(out var tempBasis, ref baseBasis, ref baseBasisTo, rate);
								IKMath.MatMultGetRot(out var worldRotation, tempBasis, _elbowRollBones[i].bone._baseToWorldBasis);
								_armRollBones[i].bone.WorldRotation = worldRotation;
								isSolved = true;
							}
						}
					}
				}

				if (_elbowRollBones != null && _elbowRollBones.Length > 0) {
					var boneLength = _elbowRollBones.Length;

					var bendingBoneBasis = _arm_isSolvedLimbIK
						? _arm_solvedBendingBoneBasis
						: new IKMatrix3x3(_bendingBone.WorldRotation * _bendingBone._worldToBoneRotation);

					// Attension: endBoneBasis is based on bendingBoneBasis
					var endBoneBasis = new IKMatrix3x3(_endBone.WorldRotation * _arm_endWorldToBendingBoneRotation);

					var dirZ = endBoneBasis.column2;
					var dirX = bendingBoneBasis.column0;
					var dirY = Vector3f.Cross(dirZ, dirX);
					dirZ = Vector3f.Cross(dirX, dirY);
					if (IKMath.VecNormalize2(ref dirY, ref dirZ)) { // Lock dirX(bendingBoneBasis.column0)
						var baseBasisTo = IKMatrix3x3.FromColumn(ref dirX, ref dirY, ref dirZ);
						IKMath.MatMult(out var baseBasis, bendingBoneBasis, _bendingBone._boneToBaseBasis);
						IKMath.MatMultRet0(ref baseBasisTo, _bendingBone._boneToBaseBasis);

						for (var i = 0; i < boneLength; ++i) {
							if (_elbowRollBones[i].bone != null && _elbowRollBones[i].bone.TransformIsAlive) {
								var rate = _elbowRollBones[i].rate;
								IKMath.MatFastLerp(out var tempBasis, ref baseBasis, ref baseBasisTo, rate);
								IKMath.MatMultGetRot(out var worldRotation, tempBasis, _elbowRollBones[i].bone._baseToWorldBasis);
								_elbowRollBones[i].bone.WorldRotation = worldRotation;
								isSolved = true;
							}
						}
					}
				}

				return isSolved;
			}
		}

		public class HeadIK
		{
			private readonly Settings _settings;
			private readonly InternalValues _internalValues;

			private readonly Bone _neckBone;
			private readonly Bone _headBone;
			private readonly Bone _leftEyeBone;
			private readonly Bone _rightEyeBone;

			private readonly Effector _headEffector;
			private readonly Effector _eyesEffector;

			private Quaternionf _headEffectorToWorldRotation = Quaternionf.Identity;
			private Quaternionf _headToLeftEyeRotation = Quaternionf.Identity;
			private Quaternionf _headToRightEyeRotation = Quaternionf.Identity;

			public HeadIK(HumanoidIK fullBodyIK) {
				_settings = fullBodyIK.settings;
				_internalValues = fullBodyIK.internalValues;

				_neckBone = PrepareBone(fullBodyIK.headBones.neck);
				_headBone = PrepareBone(fullBodyIK.headBones.head);
				_leftEyeBone = PrepareBone(fullBodyIK.headBones.leftEye);
				_rightEyeBone = PrepareBone(fullBodyIK.headBones.rightEye);
				_headEffector = fullBodyIK.headEffectors.head;
				_eyesEffector = fullBodyIK.headEffectors.eyes;
			}

			bool _isSyncDisplacementAtLeastOnce;
			bool _isEnabledCustomEyes;

			void SyncDisplacement(HumanoidIK fullBodyIK) {
				// Measure bone length.(Using worldPosition)
				// Force execution on 1st time. (Ignore case _settings.syncDisplacement == SyncDisplacement.Disable)
				if (_settings.syncDisplacement == HumanoidIK.SyncDisplacement.Everyframe || !_isSyncDisplacementAtLeastOnce) {
					_isSyncDisplacementAtLeastOnce = true;

					if (_headBone != null && _headBone.TransformIsAlive) {
						if (_headEffector != null) {
							IKMath.QuatMultInv0(out _headEffectorToWorldRotation, _headEffector._defaultRotation, _headBone._defaultRotation);
						}
						if (_leftEyeBone != null && _leftEyeBone.TransformIsAlive) {
							IKMath.QuatMultInv0(out _headToLeftEyeRotation, _headBone._defaultRotation, _leftEyeBone._defaultRotation);
						}
						if (_rightEyeBone != null && _rightEyeBone.TransformIsAlive) {
							IKMath.QuatMultInv0(out _headToRightEyeRotation, _headBone._defaultRotation, _rightEyeBone._defaultRotation);
						}
					}

					_isEnabledCustomEyes = fullBodyIK.PrepareCustomEyes(ref _headToLeftEyeRotation, ref _headToRightEyeRotation);
				}
			}

			public bool Solve(HumanoidIK fullBodyIK) {
				if (_neckBone == null || !_neckBone.TransformIsAlive ||
					_headBone == null || !_headBone.TransformIsAlive ||
					_headBone.ParentBone == null || !_headBone.ParentBone.TransformIsAlive) {
					return false;
				}

				SyncDisplacement(fullBodyIK);

				var headPositionWeight = _headEffector.positionEnabled ? _headEffector.positionWeight : 0.0f;
				var eyesPositionWeight = _eyesEffector.positionEnabled ? _eyesEffector.positionWeight : 0.0f;

				if (headPositionWeight <= IKMath.IK_EPSILON && eyesPositionWeight <= IKMath.IK_EPSILON) {
					var parentWorldRotation = _neckBone.ParentBone.WorldRotation;
					IKMath.QuatMult(out var parentBaseRotation, parentWorldRotation, _neckBone.ParentBone._worldToBaseRotation);

					if (_internalValues.resetTransforms) {
						IKMath.QuatMult(out var tempRotation, parentBaseRotation, _neckBone._baseToWorldRotation);
						_neckBone.WorldRotation = tempRotation;
					}

					var headRotationWeight = _headEffector.rotationEnabled ? _headEffector.rotationWeight : 0.0f;
					if (headRotationWeight > IKMath.IK_EPSILON) {
						var headEffectorWorldRotation = _headEffector.WorldRotation;
						IKMath.QuatMult(out var toRotation, headEffectorWorldRotation, _headEffectorToWorldRotation);
						if (headRotationWeight < 1.0f - IKMath.IK_EPSILON) {
							Quaternionf fromRotation;
							if (_internalValues.resetTransforms) {
								IKMath.QuatMult(out fromRotation, parentBaseRotation, _headBone._baseToWorldRotation);
							}
							else {
								fromRotation = _headBone.WorldRotation; // This is able to use _headBone.worldRotation directly.
							}
							_headBone.WorldRotation = Quaternionf.Slerp(fromRotation, toRotation, headRotationWeight);
						}
						else {
							_headBone.WorldRotation = toRotation;
						}

						HeadRotationLimit();
					}
					else {
						if (_internalValues.resetTransforms) {
							IKMath.QuatMult(out var tempRotation, parentBaseRotation, _headBone._baseToWorldRotation);
							_headBone.WorldRotation = tempRotation;
						}
					}

					if (_internalValues.resetTransforms) {
						if (_isEnabledCustomEyes) {
							fullBodyIK.ResetCustomEyes();
						}
						else {
							ResetEyes();
						}
					}

					return _internalValues.resetTransforms || headRotationWeight > IKMath.IK_EPSILON;
				}

				IK_Solve(fullBodyIK);
				return true;
			}

			void HeadRotationLimit() {
				// Rotation Limit.
				Quaternionf tempRotation;
				tempRotation = _headBone.WorldRotation;
				IKMath.QuatMult(out var headRotation, tempRotation, _headBone._worldToBaseRotation);
				tempRotation = _neckBone.WorldRotation;
				IKMath.QuatMult(out var neckRotation, tempRotation, _neckBone._worldToBaseRotation);
				IKMath.QuatMultInv0(out var localRotation, neckRotation, headRotation);

				IKMath.MatSetRot(out var localBasis, ref localRotation);

				var localDirY = localBasis.column1;
				var localDirZ = localBasis.column2;

				var isLimited = false;
				isLimited |= IKMath.LimitXZ_Square(ref localDirY,
					_internalValues.headIK.headLimitRollTheta.sin,
					_internalValues.headIK.headLimitRollTheta.sin,
					_internalValues.headIK.headLimitPitchUpTheta.sin,
					_internalValues.headIK.headLimitPitchDownTheta.sin);
				isLimited |= IKMath.LimitXY_Square(ref localDirZ,
					_internalValues.headIK.headLimitYawTheta.sin,
					_internalValues.headIK.headLimitYawTheta.sin,
					_internalValues.headIK.headLimitPitchDownTheta.sin,
					_internalValues.headIK.headLimitPitchUpTheta.sin);

				if (isLimited) {
					if (IKMath.ComputeBasisFromYZLockZ(out localBasis, localDirY, ref localDirZ)) {
						IKMath.MatGetRot(out localRotation, localBasis);
						IKMath.QuatMultNorm3(out headRotation, neckRotation, localRotation, _headBone._baseToWorldRotation);
						_headBone.WorldRotation = headRotation;
					}
				}
			}

			void IK_Solve(HumanoidIK fullBodyIK) {
				var parentWorldRotation = _neckBone.ParentBone.WorldRotation;
				IKMath.MatSetRotMultInv1(out var parentBasis, parentWorldRotation, _neckBone.ParentBone._defaultRotation);
				IKMath.MatMult(out var parentBaseBasis, parentBasis, _internalValues.defaultRootBasis);
				IKMath.QuatMult(out var parentBaseRotation, parentWorldRotation, _neckBone.ParentBone._worldToBaseRotation);

				var headPositionWeight = _headEffector.positionEnabled ? _headEffector.positionWeight : 0.0f;
				var eyesPositionWeight = _eyesEffector.positionEnabled ? _eyesEffector.positionWeight : 0.0f;

				var neckBonePrevRotation = Quaternionf.Identity;
				var headBonePrevRotation = Quaternionf.Identity;
				var leftEyeBonePrevRotation = Quaternionf.Identity;
				var rightEyeBonePrevRotation = Quaternionf.Identity;
				if (!_internalValues.resetTransforms) {
					neckBonePrevRotation = _neckBone.WorldRotation;
					headBonePrevRotation = _headBone.WorldRotation;
					if (_leftEyeBone != null && _leftEyeBone.TransformIsAlive) {
						leftEyeBonePrevRotation = _leftEyeBone.WorldRotation;
					}
					if (_rightEyeBone != null && _rightEyeBone.TransformIsAlive) {
						rightEyeBonePrevRotation = _rightEyeBone.WorldRotation;
					}
				}

				// for Neck
				if (headPositionWeight > IKMath.IK_EPSILON) {
					IKMath.MatMult(out var neckBoneBasis, parentBasis, _neckBone._localAxisBasis);

					var yDir = _headEffector.WorldPosition - _neckBone.WorldPosition; // Not use _hidden_worldPosition
					if (IKMath.VecNormalize(ref yDir)) {
						IKMath.MatMultVecInv(out var localDir, neckBoneBasis, yDir);

						if (IKMath.LimitXZ_Square(ref localDir,
							_internalValues.headIK.neckLimitRollTheta.sin,
							_internalValues.headIK.neckLimitRollTheta.sin,
							_internalValues.headIK.neckLimitPitchDownTheta.sin,
							_internalValues.headIK.neckLimitPitchUpTheta.sin)) {
							IKMath.MatMultVec(out yDir, neckBoneBasis, localDir);
						}

						var xDir = parentBaseBasis.column0;
						var zDir = parentBaseBasis.column2;
						if (IKMath.ComputeBasisLockY(out neckBoneBasis, xDir, ref yDir, zDir)) {
							IKMath.MatMultGetRot(out var worldRotation, neckBoneBasis, _neckBone._boneToWorldBasis);
							if (headPositionWeight < 1.0f - IKMath.IK_EPSILON) {
								Quaternionf fromRotation;
								if (_internalValues.resetTransforms) {
									IKMath.QuatMult(out fromRotation, parentBaseRotation, _neckBone._baseToWorldRotation);
								}
								else {
									fromRotation = neckBonePrevRotation; // This is able to use _headBone.worldRotation directly.
								}

								_neckBone.WorldRotation = Quaternionf.Slerp(fromRotation, worldRotation, headPositionWeight);
							}
							else {
								_neckBone.WorldRotation = worldRotation;
							}
						}
					}
				}
				else if (_internalValues.resetTransforms) {
					IKMath.QuatMult(out var tempRotation, parentBaseRotation, _neckBone._baseToWorldRotation);
					_neckBone.WorldRotation = tempRotation;
				}

				// for Head / Eyes
				if (eyesPositionWeight <= IKMath.IK_EPSILON) {
					var headRotationWeight = _headEffector.rotationEnabled ? _headEffector.rotationWeight : 0.0f;
					if (headRotationWeight > IKMath.IK_EPSILON) {
						var headEffectorWorldRotation = _headEffector.WorldRotation;
						IKMath.QuatMult(out var toRotation, headEffectorWorldRotation, _headEffectorToWorldRotation);
						if (headRotationWeight < 1.0f - IKMath.IK_EPSILON) {
							Quaternionf fromRotation;
							var neckBoneWorldRotation = _neckBone.WorldRotation;
							if (_internalValues.resetTransforms) {
								IKMath.QuatMult3(out fromRotation, neckBoneWorldRotation, _neckBone._worldToBaseRotation, _headBone._baseToWorldRotation);
							}
							else {
								// Not use _headBone.worldRotation.
								IKMath.QuatMultNorm3Inv1(out fromRotation, neckBoneWorldRotation, neckBonePrevRotation, headBonePrevRotation);
							}
							_headBone.WorldRotation = Quaternionf.Slerp(fromRotation, toRotation, headRotationWeight);
						}
						else {
							_headBone.WorldRotation = toRotation;
						}
					}
					else {
						if (_internalValues.resetTransforms) {
							var neckBoneWorldRotation = _neckBone.WorldRotation;
							IKMath.QuatMult3(out var headBoneWorldRotation, neckBoneWorldRotation, _neckBone._worldToBaseRotation, _headBone._baseToWorldRotation);
							_headBone.WorldRotation = headBoneWorldRotation;
						}
					}

					HeadRotationLimit();

					if (_internalValues.resetTransforms) {
						if (_isEnabledCustomEyes) {
							fullBodyIK.ResetCustomEyes();
						}
						else {
							ResetEyes();
						}
					}

					return;
				}

				{
					var parentBoneWorldPosition = _neckBone.ParentBone.WorldPosition;
					IKMath.MatMultVecPreSubAdd(out var eyesPosition, parentBasis, _eyesEffector._defaultPosition, _neckBone.ParentBone._defaultPosition, parentBoneWorldPosition);

					// Note: Not use _eyesEffector._hidden_worldPosition
					var eyesDir = _eyesEffector.WorldPosition - eyesPosition; // Memo: Not normalize yet.

					IKMatrix3x3 neckBaseBasis;
					{
						IKMath.MatMultVecInv(out var localDir, parentBaseBasis, eyesDir);

						localDir.y *= _settings.headIK.eyesToNeckPitchRate;
						IKMath.VecNormalize(ref localDir);

						if (HumanoidIK.ComputeEyesRange(ref localDir, _internalValues.headIK.eyesTraceTheta.cos)) {
							if (localDir.y < -_internalValues.headIK.neckLimitPitchDownTheta.sin) {
								localDir.y = -_internalValues.headIK.neckLimitPitchDownTheta.sin;
							}
							else if (localDir.y > _internalValues.headIK.neckLimitPitchUpTheta.sin) {
								localDir.y = _internalValues.headIK.neckLimitPitchUpTheta.sin;
							}
							localDir.x = 0.0f;
							localDir.z = IKMath.Sqrt(1.0f - localDir.y * localDir.y);
						}

						IKMath.MatMultVec(out eyesDir, parentBaseBasis, localDir);

						{
							var xDir = parentBaseBasis.column0;
							var yDir = parentBaseBasis.column1;
							var zDir = eyesDir;

							if (!IKMath.ComputeBasisLockZ(out neckBaseBasis, xDir, yDir, ref zDir)) {
								neckBaseBasis = parentBaseBasis; // Failsafe.
							}
						}

						IKMath.MatMultGetRot(out var worldRotation, neckBaseBasis, _neckBone._baseToWorldBasis);
						if (_eyesEffector.positionWeight < 1.0f - IKMath.IK_EPSILON) {
							var neckWorldRotation = Quaternionf.Slerp(_neckBone.WorldRotation, worldRotation, _eyesEffector.positionWeight); // This is able to use _neckBone.worldRotation directly.
							_neckBone.WorldRotation = neckWorldRotation;
							IKMath.MatSetRotMult(out neckBaseBasis, neckWorldRotation, _neckBone._worldToBaseRotation);
						}
						else {
							_neckBone.WorldRotation = worldRotation;
						}
					}

					IKMath.MatMult(out var neckBasis, neckBaseBasis, _internalValues.defaultRootBasisInv);

					var neckBoneWorldPosition = _neckBone.WorldPosition;
					IKMath.MatMultVecPreSubAdd(out eyesPosition, neckBasis, _eyesEffector._defaultPosition, _neckBone._defaultPosition, neckBoneWorldPosition);

					// Note: Not use _eyesEffector._hidden_worldPosition
					eyesDir = _eyesEffector.WorldPosition - eyesPosition;

					IKMatrix3x3 headBaseBasis;
					{
						IKMath.MatMultVecInv(out var localDir, neckBaseBasis, eyesDir);

						localDir.x *= _settings.headIK.eyesToHeadYawRate;
						localDir.y *= _settings.headIK.eyesToHeadPitchRate;

						IKMath.VecNormalize(ref localDir);

						if (HumanoidIK.ComputeEyesRange(ref localDir, _internalValues.headIK.eyesTraceTheta.cos)) {
							// Note: Not use _LimitXY() for Stability
							IKMath.LimitXY_Square(ref localDir,
								_internalValues.headIK.headLimitYawTheta.sin,
								_internalValues.headIK.headLimitYawTheta.sin,
								_internalValues.headIK.headLimitPitchDownTheta.sin,
								_internalValues.headIK.headLimitPitchUpTheta.sin);
						}

						IKMath.MatMultVec(out eyesDir, neckBaseBasis, localDir);

						{
							var xDir = neckBaseBasis.column0;
							var yDir = neckBaseBasis.column1;
							var zDir = eyesDir;

							if (!IKMath.ComputeBasisLockZ(out headBaseBasis, xDir, yDir, ref zDir)) {
								headBaseBasis = neckBaseBasis;
							}
						}

						IKMath.MatMultGetRot(out var worldRotation, headBaseBasis, _headBone._baseToWorldBasis);
						if (_eyesEffector.positionWeight < 1.0f - IKMath.IK_EPSILON) {
							var neckBoneWorldRotation = _neckBone.WorldRotation;
							IKMath.QuatMultNorm3Inv1(out var headFromWorldRotation, neckBoneWorldRotation, neckBonePrevRotation, headBonePrevRotation);
							var headWorldRotation = Quaternionf.Slerp(headFromWorldRotation, worldRotation, _eyesEffector.positionWeight);
							_headBone.WorldRotation = headWorldRotation;
							IKMath.MatSetRotMult(out headBaseBasis, headWorldRotation, _headBone._worldToBaseRotation);
						}
						else {
							_headBone.WorldRotation = worldRotation;
						}
					}

					IKMath.MatMult(out var headBasis, headBaseBasis, _internalValues.defaultRootBasisInv);

					if (_isEnabledCustomEyes) {
						fullBodyIK.SolveCustomEyes(ref neckBasis, ref headBasis, ref headBaseBasis);
					}
					else {
						SolveEyes(ref neckBasis, ref headBasis, ref headBaseBasis, ref headBonePrevRotation, ref leftEyeBonePrevRotation, ref rightEyeBonePrevRotation);
					}
				}
			}

			void ResetEyes() {
				if (_headBone != null && _headBone.TransformIsAlive) {
					var headWorldRotation = _headBone.WorldRotation;

					Quaternionf worldRotation;
					if (_leftEyeBone != null && _leftEyeBone.TransformIsAlive) {
						IKMath.QuatMultNorm(out worldRotation, headWorldRotation, _headToLeftEyeRotation);
						_leftEyeBone.WorldRotation = worldRotation;
					}
					if (_rightEyeBone != null && _rightEyeBone.TransformIsAlive) {
						IKMath.QuatMultNorm(out worldRotation, headWorldRotation, _headToRightEyeRotation);
						_rightEyeBone.WorldRotation = worldRotation;
					}
				}
			}

			void SolveEyes(ref IKMatrix3x3 neckBasis, ref IKMatrix3x3 headBasis, ref IKMatrix3x3 headBaseBasis,
				ref Quaternionf headPrevRotation, ref Quaternionf leftEyePrevRotation, ref Quaternionf rightEyePrevRotation) {
				if (_headBone != null && _headBone.TransformIsAlive) {
					if (_leftEyeBone != null && _leftEyeBone.TransformIsAlive || _rightEyeBone != null && _rightEyeBone.TransformIsAlive) {
						var neckBoneWorldPosition = _neckBone.WorldPosition;
						IKMath.MatMultVecPreSubAdd(out var headWorldPosition, neckBasis, _headBone._defaultPosition, _neckBone._defaultPosition, neckBoneWorldPosition);

						IKMath.MatMultVecPreSubAdd(out var eyesPosition, headBasis, _eyesEffector._defaultPosition, _headBone._defaultPosition, headWorldPosition);

						IKMath.MatMultVecInv(out var eyesDir, headBaseBasis, _eyesEffector.WorldPosition - eyesPosition);

						IKMath.VecNormalize(ref eyesDir);

						if (_internalValues.resetTransforms && _eyesEffector.positionWeight < 1.0f - IKMath.IK_EPSILON) {
							var tempDir = Vector3f.Lerp(new Vector3f(0.0f, 0.0f, 1.0f), eyesDir, _eyesEffector.positionWeight);
							if (IKMath.VecNormalize(ref tempDir)) {
								eyesDir = tempDir;
							}
						}

						IKMath.LimitXY_Square(ref eyesDir,
							_internalValues.headIK.eyesLimitYawTheta.sin,
							_internalValues.headIK.eyesLimitYawTheta.sin,
							_internalValues.headIK.eyesLimitPitchTheta.sin,
							_internalValues.headIK.eyesLimitPitchTheta.sin);

						eyesDir.x *= _settings.headIK.eyesYawRate;
						eyesDir.y *= _settings.headIK.eyesPitchRate;
						var leftEyeDir = eyesDir;
						var rightEyeDir = eyesDir;

						if (eyesDir.x >= 0.0f) {
							leftEyeDir.x *= _settings.headIK.eyesYawInnerRate;
							rightEyeDir.x *= _settings.headIK.eyesYawOuterRate;
						}
						else {
							leftEyeDir.x *= _settings.headIK.eyesYawOuterRate;
							rightEyeDir.x *= _settings.headIK.eyesYawInnerRate;
						}

						IKMath.VecNormalize2(ref leftEyeDir, ref rightEyeDir);

						IKMath.MatMultVec(out leftEyeDir, headBaseBasis, leftEyeDir);
						IKMath.MatMultVec(out rightEyeDir, headBaseBasis, rightEyeDir);

						Quaternionf worldRotation;

						var headBoneWorldRotation = _headBone.WorldRotation;

						if (_leftEyeBone != null && _leftEyeBone.TransformIsAlive) {
							IKMath.ComputeBasisLockZ(out var leftEyeBaseBasis, headBasis.column0, headBasis.column1, ref leftEyeDir);
							IKMath.MatMultGetRot(out worldRotation, leftEyeBaseBasis, _leftEyeBone._baseToWorldBasis);
							if (!_internalValues.resetTransforms && _eyesEffector.positionWeight < 1.0f - IKMath.IK_EPSILON) {
								IKMath.QuatMultNorm3Inv1(out var fromRotation, headBoneWorldRotation, headPrevRotation, leftEyePrevRotation);
								_leftEyeBone.WorldRotation = Quaternionf.Slerp(fromRotation, worldRotation, _eyesEffector.positionWeight);
							}
							else {
								_leftEyeBone.WorldRotation = worldRotation;
							}
						}

						if (_rightEyeBone != null && _rightEyeBone.TransformIsAlive) {
							IKMath.ComputeBasisLockZ(out var rightEyeBaseBasis, headBasis.column0, headBasis.column1, ref rightEyeDir);
							IKMath.MatMultGetRot(out worldRotation, rightEyeBaseBasis, _rightEyeBone._baseToWorldBasis);
							if (!_internalValues.resetTransforms && _eyesEffector.positionWeight < 1.0f - IKMath.IK_EPSILON) {
								IKMath.QuatMultNorm3Inv1(out var fromRotation, headBoneWorldRotation, headPrevRotation, rightEyePrevRotation);
								_rightEyeBone.WorldRotation = Quaternionf.Slerp(fromRotation, worldRotation, _eyesEffector.positionWeight);
							}
							else {
								_rightEyeBone.WorldRotation = worldRotation;
							}
						}
					}
				}

			}

		}

		public sealed class FingerIK
		{
			const float POSITION_LERP_RATE = 1.15f;

			public sealed class FingerLink
			{
				public Bone bone = null;
				public IKMatrix3x3 boneToSolvedBasis = IKMatrix3x3.identity;
				public IKMatrix3x3 solvedToBoneBasis = IKMatrix3x3.identity;
				public IKMatrix3x4 boneTransform = IKMatrix3x4.identity;
				public float childToLength = 0.0f;
				public float childToLengthSq = 0.0f;
			}

			public struct FingerIKParams
			{
				public float lengthD0;
				public float lengthABCDInv;
				public float beginLink_endCosTheta;
			}

			public sealed class FingerBranch
			{
				public Effector effector = null;
				public FingerLink[] fingerLinks = null;
				public IKMatrix3x3 boneToSolvedBasis = IKMatrix3x3.identity;
				public IKMatrix3x3 solvedToBoneBasis = IKMatrix3x3.identity;
				public FastAngle notThumb1BaseAngle = new();
				public FastAngle notThumb2BaseAngle = new();

				public float link0ToEffectorLength = 0.0f;
				public float link0ToEffectorLengthSq = 0.0f;

				public FingerIKParams fingerIKParams = new();
			}

			sealed class ThumbLink
			{
				public IKMatrix3x3 thumb_boneToSolvedBasis = IKMatrix3x3.identity; // link to effector.
				public IKMatrix3x3 thumb_solvedToBoneBasis = IKMatrix3x3.identity; // link to effector.
			}

			sealed class ThumbBranch
			{
				public ThumbLink[] thumbLinks = null;
				public Vector3f thumbSolveY = Vector3f.Zero;
				public Vector3f thumbSolveZ = Vector3f.Zero;

				public bool thumb0_isLimited = false;
				public float thumb0_lowerLimit = 0.0f;
				public float thumb0_upperLimit = 0.0f;
				public float thumb0_innerLimit = 0.0f;
				public float thumb0_outerLimit = 0.0f;

				public float linkLength0to1Sq = 0.0f;
				public float linkLength0to1 = 0.0f;
				public float linkLength1to3Sq = 0.0f;
				public float linkLength1to3 = 0.0f;

				public float linkLength1to2Sq = 0.0f;
				public float linkLength1to2 = 0.0f;
				public float linkLength2to3Sq = 0.0f;
				public float linkLength2to3 = 0.0f;

				public float thumb1_baseThetaAtoB = 1.0f;
				public float thumb1_Acos_baseThetaAtoB = 0.0f;
			}

			private readonly FingerIKType _fingerIKType;
			private readonly Settings _settings;
			private readonly InternalValues _internalValues;

			private readonly Bone _parentBone; // wrist/leg
			private readonly FingerBranch[] _fingerBranches = new FingerBranch[(int)FingerType.Max];
			private ThumbBranch _thumbBranch = null;

			private FastAngle _notThumbYawThetaLimit = new(10.0f * MathUtil.DEG_2_RADF);
			private FastAngle _notThumbPitchUThetaLimit = new(60.0f * MathUtil.DEG_2_RADF);
			private FastAngle _notThumbPitchLThetaLimit = new(160.0f * MathUtil.DEG_2_RADF);

			private FastAngle _notThumb0FingerIKLimit = new(60.0f * MathUtil.DEG_2_RADF);

			private FastAngle _notThumb1PitchUTrace = new(5.0f * MathUtil.DEG_2_RADF);
			private FastAngle _notThumb1PitchUSmooth = new(5.0f * MathUtil.DEG_2_RADF);
			private FastAngle _notThumb1PitchUTraceSmooth = new(10.0f * MathUtil.DEG_2_RADF); // _notThumb1PitchUTrace + _notThumb1PitchUSmooth
			private FastAngle _notThumb1PitchLTrace = new(10.0f * MathUtil.DEG_2_RADF);
			private FastAngle _notThumb1PitchLLimit = new(80.0f * MathUtil.DEG_2_RADF);

			public FingerIK(HumanoidIK fullBodyIK, FingerIKType fingerIKType) {
				_fingerIKType = fingerIKType;
				_settings = fullBodyIK.settings;
				_internalValues = fullBodyIK.internalValues;

				FingersBones fingerBones = null;
				FingersEffectors fingerEffectors = null;
				switch (fingerIKType) {
					case FingerIKType.LeftWrist:
						_parentBone = fullBodyIK.leftArmBones.wrist;
						fingerBones = fullBodyIK.leftHandFingersBones;
						fingerEffectors = fullBodyIK.leftHandFingersEffectors;
						break;
					case FingerIKType.RightWrist:
						_parentBone = fullBodyIK.rightArmBones.wrist;
						fingerBones = fullBodyIK.rightHandFingersBones;
						fingerEffectors = fullBodyIK.rightHandFingersEffectors;
						break;
				}

				_notThumb1PitchUTraceSmooth = new FastAngle(_notThumb1PitchUTrace.angle + _notThumb1PitchUSmooth.angle);

				if (fingerBones != null && fingerEffectors != null) {
					for (var fingerType = 0; fingerType < (int)FingerType.Max; ++fingerType) {
						IArray<Bone> bones = null;
						Effector effector = null;
						switch (fingerType) {
							case (int)FingerType.Thumb:
								bones = fingerBones.thumb;
								effector = fingerEffectors.thumb;
								break;
							case (int)FingerType.Index:
								bones = fingerBones.index;
								effector = fingerEffectors.index;
								break;
							case (int)FingerType.Middle:
								bones = fingerBones.middle;
								effector = fingerEffectors.middle;
								break;
							case (int)FingerType.Ring:
								bones = fingerBones.ring;
								effector = fingerEffectors.ring;
								break;
							case (int)FingerType.Little:
								bones = fingerBones.little;
								effector = fingerEffectors.little;
								break;
						}

						if (bones != null && effector != null) {
							PrepareBranch(fingerType, bones, effector);
						}
					}
				}
			}

			// Allocation only.
			void PrepareBranch(int fingerType, IArray<Bone> bones, Effector effector) {
				if (_parentBone == null || bones == null || effector == null) {
					return;
				}

				var boneLength = bones.Length;
				if (boneLength == 0) {
					return;
				}

				if (effector.Bone != null && bones[boneLength - 1] == effector.Bone) {
					boneLength -= 1;
					if (boneLength == 0) {
						return;
					}
				}

				if (boneLength != 0) {
					if (bones[boneLength - 1] == null || bones[boneLength - 1].transform == null) {
						boneLength -= 1;
						if (boneLength == 0) {
							return;
						}
					}
				}

				var fingerBranch = new FingerBranch {
					effector = effector,
					fingerLinks = new FingerLink[boneLength]
				};
				for (var linkID = 0; linkID < boneLength; ++linkID) {
					if (bones[linkID] == null || bones[linkID].transform == null) {
						return;
					}
					fingerBranch.fingerLinks[linkID] = new FingerLink {
						bone = bones[linkID]
					};
				}

				_fingerBranches[fingerType] = fingerBranch;

				if (fingerType == (int)FingerType.Thumb) {
					_thumbBranch = new ThumbBranch {
						thumbLinks = new ThumbLink[boneLength]
					};
					for (var i = 0; i != boneLength; ++i) {
						_thumbBranch.thumbLinks[i] = new ThumbLink();
					}
				}
			}

			static bool SolveThumbYZ(
				ref IKMatrix3x3 middleBoneToSolvedBasis,
				ref Vector3f thumbSolveY,
				ref Vector3f thumbSolveZ) {
				if (IKMath.VecNormalize2(ref thumbSolveY, ref thumbSolveZ)) {
					if (MathF.Abs(thumbSolveY.z) > MathF.Abs(thumbSolveZ.z)) {
						(thumbSolveZ, thumbSolveY) = (thumbSolveY, thumbSolveZ);
					}

					if (thumbSolveY.y < 0.0f) {
						thumbSolveY = -thumbSolveY;
					}
					if (thumbSolveZ.z < 0.0f) {
						thumbSolveZ = -thumbSolveZ;
					}

					IKMath.MatMultVec(out thumbSolveY, middleBoneToSolvedBasis, thumbSolveY);
					IKMath.MatMultVec(out thumbSolveZ, middleBoneToSolvedBasis, thumbSolveZ);
					return true;
				}

				thumbSolveY = Vector3f.Zero;
				thumbSolveZ = Vector3f.Zero;
				return false;
			}

			// for Prepare, SyncDisplacement.
			void PrepareBranch2(int fingerType) {
				var fingerBranch = _fingerBranches[fingerType];
				if (_parentBone == null || fingerBranch == null) {
					return;
				}

				var fingerEffector = fingerBranch.effector;
				var fingerLinkLength = fingerBranch.fingerLinks.Length;

				var isRight = _fingerIKType == FingerIKType.RightWrist;

				if (fingerBranch.fingerLinks != null && fingerBranch.fingerLinks.Length > 0 && fingerBranch.fingerLinks[0].bone != null) {
					var dirX = fingerEffector.DefaultPosition - fingerBranch.fingerLinks[0].bone._defaultPosition;
					dirX = isRight ? dirX : -dirX;
					if (IKMath.VecNormalize(ref dirX) && IKMath.ComputeBasisFromXZLockX(out fingerBranch.boneToSolvedBasis, dirX, _internalValues.defaultRootBasis.column2)) {
						fingerBranch.solvedToBoneBasis = fingerBranch.boneToSolvedBasis.Transpose;
					}

					fingerBranch.link0ToEffectorLength = IKMath.VecLengthAndLengthSq2(
						out fingerBranch.link0ToEffectorLengthSq,
						 fingerEffector._defaultPosition, fingerBranch.fingerLinks[0].bone._defaultPosition);
				}

				if (fingerType == (int)FingerType.Thumb) {
					var middleFingerBranch = _fingerBranches[(int)FingerType.Middle];
					if (middleFingerBranch == null) {
						return;
					}

					if (middleFingerBranch.fingerLinks.Length >= 1) {
						var middleFingerLink0 = middleFingerBranch.fingerLinks[0];
						var middleBoneToSolvedBasis = IKMatrix3x3.identity;
						var middleSolvedToBoneBasis = IKMatrix3x3.identity;
						var middleDirX = middleFingerLink0.bone._defaultPosition - _parentBone._defaultPosition;
						if (IKMath.VecNormalize(ref middleDirX)) {
							middleDirX = isRight ? middleDirX : -middleDirX;
							if (IKMath.ComputeBasisFromXZLockX(out middleBoneToSolvedBasis, middleDirX, _internalValues.defaultRootBasis.column2)) {
								middleSolvedToBoneBasis = middleBoneToSolvedBasis.Transpose;
							}
						}

						// Solve thumb's basis Y / Z vectors.

						var isSolved = false;
						if (fingerLinkLength >= 2 && fingerEffector._isSimulateFingerTips == false) {
							// Memo: Skip if fingerEffector._isSimulateFingerTips = true.(Because always thumbSolveZ = 0, 0, 1)
							var thumbFingerLink0 = fingerBranch.fingerLinks[fingerLinkLength - 2];
							var thumbFingerLink1 = fingerBranch.fingerLinks[fingerLinkLength - 1];
							var thumbPosition0 = thumbFingerLink0.bone._defaultPosition;
							var thumbPosition1 = thumbFingerLink1.bone._defaultPosition;
							var thumbPosition2 = fingerEffector._defaultPosition;

							// World to Local Basis.(Reference Middle Finger.)
							IKMath.MatMultVec(out var thumb0to1, middleSolvedToBoneBasis, thumbPosition1 - thumbPosition0);
							IKMath.MatMultVec(out var thumb1to2, middleSolvedToBoneBasis, thumbPosition2 - thumbPosition1);

							var tempY = Vector3f.Cross(thumb0to1, thumbPosition2 - thumbPosition1);

							_thumbBranch.thumbSolveY = tempY;
							_thumbBranch.thumbSolveZ = Vector3f.Cross(thumbPosition2 - thumbPosition1, tempY);

							isSolved = SolveThumbYZ(ref middleBoneToSolvedBasis,
								ref _thumbBranch.thumbSolveY,
								ref _thumbBranch.thumbSolveZ);
						}

						if (!isSolved && fingerLinkLength >= 3) {
							var thumbFingerLink0 = fingerBranch.fingerLinks[fingerLinkLength - 3];
							var thumbFingerLink1 = fingerBranch.fingerLinks[fingerLinkLength - 2];
							var thumbFingerLink2 = fingerBranch.fingerLinks[fingerLinkLength - 1];
							var thumbPosition0 = thumbFingerLink0.bone._defaultPosition;
							var thumbPosition1 = thumbFingerLink1.bone._defaultPosition;
							var thumbPosition2 = thumbFingerLink2.bone._defaultPosition;
#pragma warning disable IDE0059 // Unnecessary assignment of a value
							var thumb0to1 = thumbPosition1 - thumbPosition0;
							var thumb1to2 = thumbPosition2 - thumbPosition1;
#pragma warning restore IDE0059 // Unnecessary assignment of a value

							// World to Local Basis.(Reference Middle Finger.)
							IKMath.MatMultVec(out thumb0to1, middleSolvedToBoneBasis, thumb0to1);
							IKMath.MatMultVec(out thumb1to2, middleSolvedToBoneBasis, thumb1to2);

							var tempY = Vector3f.Cross(thumb0to1, thumb1to2);

							_thumbBranch.thumbSolveY = tempY;
							_thumbBranch.thumbSolveZ = Vector3f.Cross(thumb1to2, tempY);

							isSolved = SolveThumbYZ(ref middleBoneToSolvedBasis,
								ref _thumbBranch.thumbSolveY,
								ref _thumbBranch.thumbSolveZ);
						}

						if (!isSolved) {
							_thumbBranch.thumbSolveZ = new Vector3f(0.0f, 1.0f, 2.0f);
							_thumbBranch.thumbSolveY = new Vector3f(0.0f, 2.0f, -1.0f);
							IKMath.VecNormalize2(ref _thumbBranch.thumbSolveZ, ref _thumbBranch.thumbSolveY);
						}
					}
				}

				for (var n = 0; n != fingerBranch.fingerLinks.Length; ++n) {
					var fingerLink = fingerBranch.fingerLinks[n];

					var sourcePosition = fingerLink.bone._defaultPosition;
					Vector3f destPosition;
					FastLength sourceToDestLength;
					Vector3f sourceToDestDirection;
					if (n + 1 != fingerBranch.fingerLinks.Length) {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
						destPosition = fingerBranch.fingerLinks[n + 1].bone._defaultPosition;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
						sourceToDestLength = fingerBranch.fingerLinks[n + 1].bone._defaultLocalLength;
						sourceToDestDirection = fingerBranch.fingerLinks[n + 1].bone._defaultLocalDirection;
					}
					else {
						destPosition = fingerBranch.effector._defaultPosition;
						if (!fingerBranch.effector._isSimulateFingerTips) {
							sourceToDestLength = fingerBranch.effector.Bone._defaultLocalLength;
							sourceToDestDirection = fingerBranch.effector.Bone._defaultLocalDirection;
						}
						else {
							var tempTranslate = destPosition - sourcePosition;
							sourceToDestLength = FastLength.FromVector3(ref tempTranslate);
							sourceToDestDirection = sourceToDestLength.length > IKMath.FLOAT_EPSILON ? tempTranslate * (1.0f / sourceToDestLength.length) : Vector3f.Zero;
						}
					}

					if (fingerType != (int)FingerType.Thumb) {
						fingerLink.childToLength = sourceToDestLength.length;
						fingerLink.childToLengthSq = sourceToDestLength.lengthSq;
					}

					{
						var dirX = sourceToDestDirection;
						if (dirX.x != 0.0f || dirX.y != 0.0f || dirX.z != 0.0f) {
							dirX = isRight ? dirX : -dirX;
							if (IKMath.ComputeBasisFromXZLockX(out fingerLink.boneToSolvedBasis, dirX, _internalValues.defaultRootBasis.column2)) {
								fingerLink.solvedToBoneBasis = fingerLink.boneToSolvedBasis.Transpose;
							}
						}
					}

					if (fingerType == (int)FingerType.Thumb) {
						var thumbLink = _thumbBranch.thumbLinks[n];

						var dirX = fingerBranch.effector._defaultPosition - sourcePosition;
						if (IKMath.VecNormalize(ref dirX)) {
							dirX = isRight ? dirX : -dirX;
							if (IKMath.ComputeBasisFromXYLockX(out thumbLink.thumb_boneToSolvedBasis, ref dirX, _thumbBranch.thumbSolveY)) {
								thumbLink.thumb_solvedToBoneBasis = thumbLink.thumb_boneToSolvedBasis.Transpose;
							}
						}
					}
				}

				if (fingerType != (int)FingerType.Thumb) {
					if (fingerBranch.fingerLinks.Length == 3) {
						// Compute rotate angle. Based X/Y coordinate.
						// !isRight ... Plus value as warp, minus value as bending.
						// isRight ... Minus value as warp, plus value as bending.
						fingerBranch.notThumb1BaseAngle = new FastAngle(ComputeJointBaseAngle(
							ref _internalValues.defaultRootBasis,
							ref fingerBranch.fingerLinks[0].bone._defaultPosition,
							ref fingerBranch.fingerLinks[1].bone._defaultPosition,
							fingerBranch.effector._defaultPosition, isRight));

						fingerBranch.notThumb2BaseAngle = new FastAngle(ComputeJointBaseAngle(
							ref _internalValues.defaultRootBasis,
							ref fingerBranch.fingerLinks[1].bone._defaultPosition,
							ref fingerBranch.fingerLinks[2].bone._defaultPosition,
							fingerBranch.effector._defaultPosition, isRight));

						var linkLength0 = fingerBranch.fingerLinks[0].childToLength;
						var linkLength1 = fingerBranch.fingerLinks[1].childToLength;
						var linkLength2 = fingerBranch.fingerLinks[2].childToLength;

						var lengthH0 = MathF.Abs(linkLength0 - linkLength2);
						var lengthD0 = IKMath.Sqrt(linkLength1 * linkLength1 - lengthH0 * lengthH0);

						var beginLink_endCosTheta = 0.0f; // 90'

						if (linkLength0 > linkLength2) {
							var min_cosTheta = _notThumb0FingerIKLimit.cos; // 60
							var min_sinTheta = _notThumb0FingerIKLimit.sin; // 60
							var norm_lengthD0 = lengthD0 * (1.0f / linkLength1); // = cosTheta
							if (norm_lengthD0 < min_cosTheta) {
								lengthD0 = min_cosTheta * linkLength1;
								lengthH0 = min_sinTheta * linkLength1;

								var beginLink_endSinTheta = MathUtil.Clamp((linkLength2 + lengthH0) * (1.0f / linkLength0), 0, 1);
								beginLink_endCosTheta = IKMath.SqrtClamp01(1.0f - beginLink_endSinTheta * beginLink_endSinTheta);
							}
						}

						var lengthCtoD = linkLength1 - lengthD0;
						var lengthAplusB = linkLength0 + linkLength2;
						var lengthABCDInv = lengthAplusB + lengthCtoD;
						lengthABCDInv = lengthABCDInv > IKMath.IK_EPSILON ? 1.0f / lengthABCDInv : 0.0f;

						fingerBranch.fingerIKParams.lengthD0 = lengthD0;
						fingerBranch.fingerIKParams.lengthABCDInv = lengthABCDInv;
						fingerBranch.fingerIKParams.beginLink_endCosTheta = beginLink_endCosTheta;
					}
				}
			}

			// for Prepare, SyncDisplacement.
			void PrepareThumb() {
				var fingerBranch = _fingerBranches[(int)FingerType.Thumb];
				var indexFingerBranch = _fingerBranches[(int)FingerType.Index];
				if (fingerBranch == null || fingerBranch.fingerLinks.Length != 3 ||
					indexFingerBranch == null || indexFingerBranch.fingerLinks.Length == 0) {
					return;
				}

				var fingerLink0 = fingerBranch.fingerLinks[0];
				var fingerLink1 = fingerBranch.fingerLinks[1];
				var fingerLink2 = fingerBranch.fingerLinks[2];

				{
					var indexBeginLink = indexFingerBranch.fingerLinks[0];
					// Direction thumb0 to index0.
					var thumbToIndex = indexBeginLink.bone._defaultPosition - fingerLink0.bone._defaultPosition;
					IKMath.MatMultVec(out var localThumbToIndex, fingerBranch.solvedToBoneBasis, thumbToIndex);
					if (IKMath.VecNormalize(ref localThumbToIndex)) {
						_thumbBranch.thumb0_isLimited = true;
						_thumbBranch.thumb0_innerLimit = MathF.Max(-localThumbToIndex.z, 0.0f); // innerLimit = under index 0
						_thumbBranch.thumb0_outerLimit = (float)Math.Sin(MathF.Max(-(IKMath.Asin(_thumbBranch.thumb0_innerLimit) - 40.0f * MathUtil.DEG_2_RADF), 0.0f));
						_thumbBranch.thumb0_upperLimit = MathF.Max(localThumbToIndex.y, 0.0f); // upperLimit = height index 0
						_thumbBranch.thumb0_lowerLimit = (float)Math.Sin(MathF.Max(-(IKMath.Asin(_thumbBranch.thumb0_upperLimit) - 45.0f * MathUtil.DEG_2_RADF), 0.0f));
					}
				}

				_thumbBranch.linkLength0to1 = IKMath.VecLengthAndLengthSq2(out _thumbBranch.linkLength0to1Sq,
					 fingerLink1.bone._defaultPosition, fingerLink0.bone._defaultPosition);
				_thumbBranch.linkLength1to2 = IKMath.VecLengthAndLengthSq2(out _thumbBranch.linkLength1to2Sq,
					 fingerLink2.bone._defaultPosition, fingerLink1.bone._defaultPosition);
				_thumbBranch.linkLength2to3 = IKMath.VecLengthAndLengthSq2(out _thumbBranch.linkLength2to3Sq,
					 fingerBranch.effector._defaultPosition, fingerLink2.bone._defaultPosition);

				// Memo: Straight length.
				_thumbBranch.linkLength1to3 = IKMath.VecLengthAndLengthSq2(out _thumbBranch.linkLength1to3Sq,
					 fingerBranch.effector._defaultPosition, fingerLink1.bone._defaultPosition);

				_thumbBranch.thumb1_baseThetaAtoB = ComputeTriangleTheta(
					_thumbBranch.linkLength1to2,
					_thumbBranch.linkLength1to3,
					_thumbBranch.linkLength2to3,
					_thumbBranch.linkLength1to2Sq,
					_thumbBranch.linkLength1to3Sq,
					_thumbBranch.linkLength2to3Sq);

				_thumbBranch.thumb1_Acos_baseThetaAtoB = IKMath.Acos(_thumbBranch.thumb1_baseThetaAtoB);
			}

			bool _isSyncDisplacementAtLeastOnce;

			void SyncDisplacement() {
				// Measure bone length.(Using worldPosition)
				// Force execution on 1st time. (Ignore case _settings.syncDisplacement == SyncDisplacement.Disable)
				if (_settings.syncDisplacement == HumanoidIK.SyncDisplacement.Everyframe || !_isSyncDisplacementAtLeastOnce) {
					_isSyncDisplacementAtLeastOnce = true;

					for (var fingerType = 0; fingerType != (int)FingerType.Max; ++fingerType) {
						PrepareBranch2(fingerType);
					}

					PrepareThumb();
				}
			}

			// Helpers.

			static float ComputeJointBaseAngle(
				ref IKMatrix3x3 rootBaseBasis,
				ref Vector3f beginPosition,
				ref Vector3f nextPosition,
				 Vector3f endPosition,
				bool isRight) {
				var linkToEnd = endPosition - beginPosition;
				var linkToNext = nextPosition - beginPosition;
				if (IKMath.VecNormalize2(ref linkToEnd, ref linkToNext)) {
					var dirX = isRight ? linkToEnd : -linkToEnd;
					IKMath.ComputeBasisFromXZLockX(out var linkToEndBasis, dirX, rootBaseBasis.column2);
					dirX = isRight ? linkToNext : -linkToNext;
					var dirY = linkToEndBasis.column2;
					IKMath.ComputeBasisFromXZLockZ(out var linkToNextBasis, dirX, ref dirY);

					var dotX = Vector3f.Dot(linkToEndBasis.column0, linkToNextBasis.column0);
					var dotY = Vector3f.Dot(linkToEndBasis.column1, linkToNextBasis.column0);

					var r = IKMath.Acos(dotX);
					if (dotY < 0.0f) {
						r = -r;
					}

					return r;
				}

				return 0.0f;
			}

			static bool SolveInDirect(
				bool isRight,
				ref Vector3f solvedDirY,
				ref Vector3f solvedDirZ,
				ref IKMatrix3x3 rootBasis,
				ref IKMatrix3x3 linkBoneToSolvedBasis,
				ref Vector3f effectorDirection) {
				var dirX = isRight ? effectorDirection : -effectorDirection;
				IKMath.MatMultVec(out var dirZ, rootBasis, linkBoneToSolvedBasis.column2);
				if (!IKMath.ComputeBasisFromXZLockX(out var linkSolvedBasis, ref dirX, dirZ)) {
					return false;
				}

				solvedDirY = linkSolvedBasis.column1;
				solvedDirZ = linkSolvedBasis.column2;
				return true;
			}

			private static float ComputeTriangleTheta(float lenA, float lenB, float _, float lenASq, float lenBSq, float lenCSq) {
				var tempAB = lenA * lenB;
				return tempAB >= IKMath.IK_EPSILON ? (lenASq + lenBSq - lenCSq) / (2.0f * tempAB) : 1.0f;
			}

			static void LerpEffectorLength(
				ref float effectorLength, // out
				ref Vector3f effectorDirection,
				ref Vector3f effectorTranslate,
				ref Vector3f effectorPosition,
				ref Vector3f effectorOrigin,
				float minLength,
				float maxLength,
				float lerpLength) {
				if (lerpLength > IKMath.IK_EPSILON) {
					var subLength = effectorLength - minLength;
					var r = subLength / lerpLength;
					effectorLength = minLength + r * (maxLength - minLength);
				}
				else {
					effectorLength = minLength;
				}

				effectorTranslate = effectorLength * effectorDirection;
				effectorPosition = effectorOrigin + effectorTranslate;
			}

			static Vector3f SolveFingerIK(
				ref Vector3f beginPosition,
				ref Vector3f endPosition,
				ref Vector3f bendingDirection,
				float linkLength0,
				float linkLength1,
				float linkLength2,
				ref FingerIKParams fingerIKParams) {
				var beginToEndBaseLength = linkLength0 + linkLength1 + linkLength2;
				var beginToEndLength = (endPosition - beginPosition).Magnitude;
				if (beginToEndLength <= IKMath.IK_EPSILON) {
					return Vector3f.Zero;
				}

				var beginToEndDirection = endPosition - beginPosition;
				beginToEndDirection *= 1.0f / beginToEndLength;

				if (beginToEndLength >= beginToEndBaseLength - IKMath.IK_EPSILON) {
					return beginToEndDirection;
				}

				if (linkLength0 <= IKMath.IK_EPSILON || linkLength1 <= IKMath.IK_EPSILON || linkLength2 <= IKMath.IK_EPSILON) {
					return Vector3f.Zero;
				}

				var centerToBendingDirection = Vector3f.Cross(beginToEndDirection, bendingDirection);
				centerToBendingDirection = Vector3f.Cross(centerToBendingDirection, beginToEndDirection);

				var centerToBendingDirectionLengthTemp = centerToBendingDirection.Magnitude;
				if (centerToBendingDirectionLengthTemp <= IKMath.IK_EPSILON) {
					return Vector3f.Zero;
				}

				centerToBendingDirection *= 1.0f / centerToBendingDirectionLengthTemp;

				var solveCosTheta = MathUtil.Lerp(fingerIKParams.beginLink_endCosTheta, 1.0f, MathUtil.Clamp((beginToEndLength - fingerIKParams.lengthD0) * fingerIKParams.lengthABCDInv, 0, 1));
				var solveSinTheta = IKMath.SqrtClamp01(1.0f - solveCosTheta * solveCosTheta);

				var solvedDirection = beginToEndDirection * solveCosTheta + centerToBendingDirection * solveSinTheta;
				return !IKMath.VecNormalize(ref solvedDirection) ? Vector3f.Zero : solvedDirection;
			}

			static Vector3f SolveLimbIK(
				ref Vector3f beginPosition,
				ref Vector3f endPosition,
				float beginToInterBaseLength,
				float beginToInterBaseLengthSq,
				float interToEndBaseLength,
				float interToEndBaseLengthSq,
				ref Vector3f bendingDirection) {
				var beginToEndBaseLength = beginToInterBaseLength + interToEndBaseLength;
				if (beginToEndBaseLength <= IKMath.IK_EPSILON) {
					return Vector3f.Zero;
				}

				var beginToEndLengthSq = (endPosition - beginPosition).SqrMagnitude;
				var beginToEndLength = IKMath.Sqrt(beginToEndLengthSq);
				if (beginToEndLength <= IKMath.IK_EPSILON) {
					return Vector3f.Zero;
				}

				var beginToEndDirection = (endPosition - beginPosition) * (1.0f / beginToEndLength);
				if (beginToEndLength >= beginToEndBaseLength - IKMath.IK_EPSILON) {
					return beginToEndDirection;
				}

				var centerToBendingDirection = Vector3f.Cross(beginToEndDirection, bendingDirection);
				centerToBendingDirection = Vector3f.Cross(centerToBendingDirection, beginToEndDirection);

				var centerToBendingDirectionLengthTemp = centerToBendingDirection.Magnitude;
				if (centerToBendingDirectionLengthTemp <= IKMath.IK_EPSILON) {
					return Vector3f.Zero;
				}

				centerToBendingDirection *= 1.0f / centerToBendingDirectionLengthTemp;

				var beginToInterTheta = 1.0f;
				var triASq = interToEndBaseLengthSq;
				var triB = beginToInterBaseLength;
				var triBSq = beginToInterBaseLengthSq;
				var triC = beginToEndLength;
				var triCSq = beginToEndLengthSq;
				if (beginToEndLength < beginToEndBaseLength) {
					var bc2 = 2.0f * triB * triC;
					if (bc2 > IKMath.IK_EPSILON) {
						beginToInterTheta = (triASq - triBSq - triCSq) / -bc2;
					}
				}

				var sinTheta = IKMath.SqrtClamp01(1.0f - beginToInterTheta * beginToInterTheta);

				var beginToInterDirection = beginToInterBaseLength * beginToInterTheta * beginToEndDirection
												+ beginToInterBaseLength * sinTheta * centerToBendingDirection;
				var beginToInterDirectionLengthTemp = beginToInterDirection.Magnitude;
				if (beginToInterDirectionLengthTemp <= IKMath.IK_EPSILON) {
					return Vector3f.Zero;
				}

				beginToInterDirection *= 1.0f / beginToInterDirectionLengthTemp;
				return beginToInterDirection;
			}

			//------------------------------------------------------------------------------------------------------------------------------------------------

			public bool Solve() {
				if (_parentBone == null) {
					return false;
				}

				SyncDisplacement();

				var isSolved = false;

				var parentTransform = IKMatrix3x4.identity;
				parentTransform.origin = _parentBone.WorldPosition;
				var parentBoneWorldRotation = _parentBone.WorldRotation;
				IKMath.MatSetRotMultInv1(out parentTransform.basis, parentBoneWorldRotation, _parentBone._defaultRotation);

				for (var i = 0; i != (int)FingerType.Max; ++i) {
					var fingerBranch = _fingerBranches[i];
					if (fingerBranch == null || fingerBranch.effector == null || !fingerBranch.effector.positionEnabled) {
						continue;
					}

					if (i == (int)FingerType.Thumb) {
						isSolved |= SolveThumb(ref parentTransform);
					}
					else {
						isSolved |= SolveNotThumb(i, ref parentTransform);
					}
				}

				return isSolved;
			}

			static Vector3f GetEffectorPosition(
				InternalValues internalValues,
				Bone rootBone,
				Bone beginLinkBone,
				Effector effector,
				float link0ToEffectorLength,
				ref IKMatrix3x4 parentTransform) {
				if (rootBone != null && beginLinkBone != null && effector != null) {
					var effectorPosition = effector.WorldPosition;
					if (effector.positionWeight < 1.0f - IKMath.IK_EPSILON) {
						var endLinkPosition = internalValues.continuousSolverEnabled || internalValues.resetTransforms
							? parentTransform * (effector._defaultPosition - rootBone._defaultPosition)
							: effector.BoneWorldPosition;

						var beginLinkPosition = parentTransform * (beginLinkBone._defaultPosition - rootBone._defaultPosition);

						var moveFrom = endLinkPosition - beginLinkPosition;
						var moveTo = effectorPosition - beginLinkPosition;

						var lengthFrom = link0ToEffectorLength; // Optimized.
						var lengthTo = moveTo.Magnitude;

						if (lengthFrom > IKMath.IK_EPSILON && lengthTo > IKMath.IK_EPSILON) {
							var dirFrom = moveFrom * (1.0f / lengthFrom);
							var dirTo = moveTo * (1.0f / lengthTo);
							var dir = IKMath.LerpDir(ref dirFrom, ref dirTo, effector.positionWeight);
							var len = MathUtil.Lerp(lengthFrom, lengthTo, MathUtil.Clamp(1.0f - (1.0f - effector.positionWeight) * POSITION_LERP_RATE, 0, 1));
							return dir * len + beginLinkPosition;
						}
					}

					return effectorPosition;
				}

				return Vector3f.Zero;
			}

			bool SolveNotThumb(int fingerType, ref IKMatrix3x4 parentTransform) {
				var fingerBranch = _fingerBranches[fingerType];
				if (fingerBranch == null || fingerBranch.fingerLinks.Length != 3) {
					return false;
				}

				var isRight = _fingerIKType == FingerIKType.RightWrist;

				var beginLink = fingerBranch.fingerLinks[0];
				var bendingLink0 = fingerBranch.fingerLinks[1];
				var bendingLink1 = fingerBranch.fingerLinks[2];
				var endEffector = fingerBranch.effector;

				var linkLength0 = beginLink.childToLength;
				var linkLength1 = bendingLink0.childToLength;
				var linkLength1Sq = bendingLink0.childToLengthSq;
				var linkLength2 = bendingLink1.childToLength;
				var linkLength2Sq = bendingLink1.childToLengthSq;
				var baseLength = fingerBranch.link0ToEffectorLength;

				var beginLinkPosition = parentTransform * (beginLink.bone._defaultPosition - _parentBone._defaultPosition);
				var effectorPosition = GetEffectorPosition(_internalValues, _parentBone, beginLink.bone, endEffector, fingerBranch.link0ToEffectorLength, ref parentTransform);
				var effectorTranslate = effectorPosition - beginLinkPosition;

				var effectorLength = effectorTranslate.Magnitude;
				if (effectorLength <= IKMath.IK_EPSILON || baseLength <= IKMath.IK_EPSILON) {
					return false;
				}

				var effectorDirection = effectorTranslate * (1.0f / effectorLength);

				var isWarp = isRight ? fingerBranch.notThumb1BaseAngle.angle <= IKMath.IK_EPSILON : fingerBranch.notThumb1BaseAngle.angle >= -IKMath.IK_EPSILON;

				{
					var maxLength = isWarp ? baseLength : linkLength0 + linkLength1 + linkLength2;
					if (effectorLength > maxLength) {
						effectorLength = maxLength;
						effectorTranslate = effectorDirection * effectorLength;
						effectorPosition = beginLinkPosition + effectorTranslate;
					}
					else if (effectorLength < linkLength1) {
						effectorLength = linkLength1;
						effectorTranslate = effectorDirection * effectorLength;
						effectorPosition = beginLinkPosition + effectorTranslate;
					}
				}

				bool isUpper;
				{
					IKMath.MatMult(out var beginToEndBasis, parentTransform.basis, fingerBranch.boneToSolvedBasis);
					IKMath.MatMultVecInv(out var localEffectorDirection, beginToEndBasis, effectorDirection);

					isUpper = localEffectorDirection.y >= 0.0f;

					if (IKMath.LimitFingerNotThumb(
						isRight,
						ref localEffectorDirection,
						ref _notThumbPitchUThetaLimit,
						ref _notThumbPitchLThetaLimit,
						ref _notThumbYawThetaLimit)) {
						IKMath.MatMultVec(out effectorDirection, beginToEndBasis, localEffectorDirection);
						effectorTranslate = effectorDirection * effectorLength;
						effectorPosition = beginLinkPosition + effectorTranslate;
					}
				}

				var solveDirY = Vector3f.Zero;
				var solveDirZ = Vector3f.Zero;
				if (!SolveInDirect(
					isRight,
					ref solveDirY,
					ref solveDirZ,
					ref parentTransform.basis,
					ref beginLink.boneToSolvedBasis,
					ref effectorDirection)) {
					return false;
				}

				var solveFingerIK = !isWarp;

				if (isWarp) {
					var imm_isUpper = false;
					var imm_traceRate = 0.0f;

					var bendingLink0Position = parentTransform * (bendingLink0.bone._defaultPosition - _parentBone._defaultPosition);
					//var bendingLink1Position = parentTransform * (bendingLink1.bone._defaultPosition - _parentBone._defaultPosition);
					var endPosition = parentTransform * (endEffector._defaultPosition - _parentBone._defaultPosition);

					var beginLinkDirX = Vector3f.Zero;

					{
						var beginLinkToBendingLink0Direction = bendingLink0Position - beginLinkPosition;
						var beginLinkToEndDirection = endPosition - beginLinkPosition;
						if (IKMath.VecNormalize2(ref beginLinkToBendingLink0Direction, ref beginLinkToEndDirection)) {

							if (IKMath.ComputeBasisFromXZLockX(out var effBasis, isRight ? effectorDirection : -effectorDirection, solveDirZ)) {
								if (IKMath.ComputeBasisFromXZLockZ(out var bendBasis, isRight ? beginLinkToBendingLink0Direction : -beginLinkToBendingLink0Direction, effBasis.column2) &&
									IKMath.ComputeBasisFromXZLockZ(out var endBasis, isRight ? beginLinkToEndDirection : -beginLinkToEndDirection, effBasis.column2)) {
									// effBasis  ... beginLink to current effector basis.
									// bendBasis ... beginLink to default bendLink0 basis.
									// endBasis  ... beginLink to default effector basis.

									var effX = isRight ? effBasis.column0 : -effBasis.column0;
									var effY = effBasis.column1;
									var effZ = effBasis.column2;
									var bendX = isRight ? bendBasis.column0 : -bendBasis.column0;
									var bendY = bendBasis.column1;
									var endX = isRight ? endBasis.column0 : -endBasis.column0;
									var endY = endBasis.column1;

									// rotBendX ... begin to current bendLink0 basis.			
									var endBendDotX = Vector3f.Dot(bendX, endX); // Cosine
									var endBendDotY = Vector3f.Dot(bendX, endY); // Sine
									var rotBendX = IKMath.Rotate(ref effX, ref effY, endBendDotX, endBendDotY);

									imm_isUpper = Vector3f.Dot(endY, effX) >= 0.0f;

									var imm_isLimitL = false;

									var endEffDotX = Vector3f.Dot(endX, effX);
									if (imm_isUpper) {
										if (isWarp) {
											var traceLimitUAngle = _notThumb1PitchUTraceSmooth.angle;
											var cosTraceLimitUAngle = _notThumb1PitchUTraceSmooth.cos;
											if (traceLimitUAngle <= IKMath.IK_EPSILON || endEffDotX < cosTraceLimitUAngle) {
												var rotBendY = Vector3f.Cross(effZ, rotBendX);
												if (IKMath.VecNormalize(ref rotBendY)) {
													var cosTraceAngle = _notThumb1PitchUTrace.cos;
													var sinTraceAngle = _notThumb1PitchUTrace.sin;
													beginLinkDirX = IKMath.Rotate(ref rotBendX, ref rotBendY, cosTraceAngle, isRight ? -sinTraceAngle : sinTraceAngle);
												}
											}
											else {
												var r = IKMath.Acos(endEffDotX);
												r /= traceLimitUAngle;
												r = _notThumb1PitchUTrace.angle * r;
												beginLinkDirX = IKMath.Rotate(ref bendX, ref bendY, r);
											}
										}
										else {
											solveFingerIK = true;
										}
									}
									else {
										if (isWarp) {
											var baseAngle = MathF.Abs(fingerBranch.notThumb1BaseAngle.angle);
											var traceAngle = MathF.Max(baseAngle, _notThumb1PitchLTrace.angle);
											var cosTraceAngle = MathF.Min(fingerBranch.notThumb1BaseAngle.cos, _notThumb1PitchLTrace.cos);

											if (endEffDotX < cosTraceAngle) {
												solveFingerIK = true;
												var smoothLen = linkLength2 * 0.25f;
												if (effectorLength >= baseLength - smoothLen) {
													LerpEffectorLength(
														ref effectorLength, ref effectorDirection, ref effectorTranslate, ref effectorPosition, ref beginLinkPosition,
														baseLength - smoothLen, linkLength0 + linkLength1 + linkLength2, smoothLen);
												}
												else {
													// Nothing.
												}
											}
											else {
												if (traceAngle <= IKMath.IK_EPSILON || traceAngle == baseAngle) {
													beginLinkDirX = bendX;
													if (traceAngle <= IKMath.IK_EPSILON) {
														imm_traceRate = 1.0f;
													}
													else {
														var r = IKMath.Acos(endEffDotX);
														imm_traceRate = r / traceAngle;
													}
												}
												else {
													var r = IKMath.Acos(endEffDotX);
													r /= traceAngle;
													imm_traceRate = r;
													r = (_notThumb1PitchLTrace.angle - baseAngle) * r;
													beginLinkDirX = IKMath.Rotate(ref bendX, ref bendY, -r);
												}
											}
										}
										else {
											solveFingerIK = true;
										}
									}

									if (isWarp) {
										if (!solveFingerIK) {
											if (effectorLength < baseLength - IKMath.IK_EPSILON) {
												var extendLen = 0.0f;
												if (!imm_isLimitL) {
													extendLen = Vector3f.Dot(beginLinkDirX, effX);
													extendLen = IKMath.Sqrt(1.0f - extendLen * extendLen); // Cosine to Sine
													extendLen *= linkLength0; // Sine Length
												}
												var smoothLen = linkLength2 * 0.25f;
												if (extendLen > IKMath.IK_EPSILON && effectorLength >= baseLength - extendLen) {
													var r = 1.0f - (effectorLength - (baseLength - extendLen)) / extendLen;
													beginLinkDirX = IKMath.FastLerpDir(ref beginLinkDirX, ref effX, r);
													imm_traceRate += (1.0f - imm_traceRate) * r;
												}
												else {
													solveFingerIK = true;
													if (effectorLength >= baseLength - (extendLen + smoothLen)) {
														LerpEffectorLength(
															ref effectorLength, ref effectorDirection, ref effectorTranslate, ref effectorPosition, ref beginLinkPosition,
															baseLength - (extendLen + smoothLen), linkLength0 + linkLength1 + linkLength2, smoothLen);
													}
													else {
														// Nothing.
													}
												}
											}
										}
									}
								}
							}
						}
					}

					if (!solveFingerIK) {
						if (beginLinkDirX == Vector3f.Zero) {
							return false;
						}

						if (!IKMath.ComputeBasisFromXZLockX(out beginLink.boneTransform.basis, isRight ? beginLinkDirX : -beginLinkDirX, solveDirZ)) {
							return false;
						}

						beginLink.boneTransform.origin = beginLinkPosition;
						IKMath.MatMultRet0(ref beginLink.boneTransform.basis, beginLink.solvedToBoneBasis);

						bendingLink0Position = beginLink.boneTransform * (bendingLink0.bone._defaultPosition - beginLink.bone._defaultPosition);
						var bendingLink1Position = beginLink.boneTransform * (bendingLink1.bone._defaultPosition - beginLink.bone._defaultPosition);
						endPosition = beginLink.boneTransform * (endEffector._defaultPosition - beginLink.bone._defaultPosition);

						var basedEffectorPosition = beginLinkPosition + effectorDirection * baseLength;

						var bendingLink0ToEffectorDirection = basedEffectorPosition - bendingLink0Position;
						var bendingLink0ToBendingLink0Direction = bendingLink1Position - bendingLink0Position;
						var bendingLink0ToEndDirection = endPosition - bendingLink0Position;
						if (!IKMath.VecNormalize3(ref bendingLink0ToEffectorDirection, ref bendingLink0ToBendingLink0Direction, ref bendingLink0ToEndDirection)) {
							return false;
						}

						Vector3f bendingLink0DirX;
						{
							if (!IKMath.ComputeBasisFromXZLockX(out var effBasis, isRight ? bendingLink0ToEffectorDirection : -bendingLink0ToEffectorDirection, solveDirZ)) {
								return false;
							}

							// Effector direction stamp X/Y Plane.(Feedback Y Axis.)
							if (!IKMath.ComputeBasisFromXZLockZ(out var bendBasis, isRight ? bendingLink0ToBendingLink0Direction : -bendingLink0ToBendingLink0Direction, effBasis.column2) ||
								!IKMath.ComputeBasisFromXZLockZ(out var endBasis, isRight ? bendingLink0ToEndDirection : -bendingLink0ToEndDirection, effBasis.column2)) {
								return false;
							}

							var effX = isRight ? effBasis.column0 : -effBasis.column0;
							var effY = effBasis.column1;
							var bendX = isRight ? bendBasis.column0 : -bendBasis.column0;
							var endX = isRight ? endBasis.column0 : -endBasis.column0;
							var endY = endBasis.column1;

							var endBendDotX = Vector3f.Dot(bendX, endX); // Cosine
							var endBendDotY = Vector3f.Dot(bendX, endY); // Sine
							var rotBendX = IKMath.Rotate(ref effX, ref effY, endBendDotX, endBendDotY);

							bendingLink0DirX = imm_isUpper ? IKMath.FastLerpDir(ref rotBendX, ref effX, imm_traceRate) : IKMath.FastLerpDir(ref bendX, ref effX, imm_traceRate);
						}

						if (!IKMath.ComputeBasisFromXZLockX(out bendingLink0.boneTransform.basis, isRight ? bendingLink0DirX : -bendingLink0DirX, solveDirZ)) {
							return false;
						}

						bendingLink0.boneTransform.origin = bendingLink0Position;
						IKMath.MatMultRet0(ref bendingLink0.boneTransform.basis, bendingLink0.solvedToBoneBasis);

						bendingLink1Position = bendingLink0.boneTransform * (bendingLink1.bone._defaultPosition - bendingLink0.bone._defaultPosition);

						{
							var dirX = basedEffectorPosition - bendingLink1Position;
							if (!IKMath.VecNormalize(ref dirX)) {
								return false;
							}

							IKMath.MatMultVec(out var dirZ, bendingLink0.boneTransform.basis, bendingLink1.boneToSolvedBasis.column2);

							if (!IKMath.ComputeBasisFromXZLockX(out bendingLink1.boneTransform.basis, isRight ? dirX : -dirX, dirZ)) {
								return false;
							}

							bendingLink1.boneTransform.origin = bendingLink1Position;
							IKMath.MatMultRet0(ref bendingLink1.boneTransform.basis, bendingLink1.solvedToBoneBasis);
						}
					}
				}

				if (solveFingerIK) {
					{
						var linkSolved = SolveFingerIK(
							ref beginLinkPosition,
							ref effectorPosition,
							ref solveDirY,
							linkLength0,
							linkLength1,
							linkLength2,
							ref fingerBranch.fingerIKParams);

						if (linkSolved == Vector3f.Zero) {
							return false;
						}

						// Limit angle for finger0.
						if (!isUpper) {
							IKMath.MatMultVec(out var dirX, parentTransform.basis, beginLink.boneToSolvedBasis.column0);

							if (IKMath.ComputeBasisFromXZLockZ(out var baseBasis, dirX, solveDirZ)) {
								IKMath.MatMultVecInv(out var localFingerSolve, baseBasis, linkSolved);

								var finX = localFingerSolve.x;
								var finY = localFingerSolve.y;
								var finZ = localFingerSolve.z;

								var cosNotThumb1PitchLLimit = _notThumb1PitchLLimit.cos;
								if (isRight && finX < cosNotThumb1PitchLLimit || !isRight && finX > -cosNotThumb1PitchLLimit) {
									var lenY = IKMath.Sqrt(1.0f - (cosNotThumb1PitchLLimit * cosNotThumb1PitchLLimit + finZ * finZ));
									localFingerSolve.x = isRight ? cosNotThumb1PitchLLimit : -cosNotThumb1PitchLLimit;
									localFingerSolve.y = finY >= 0.0f ? lenY : -lenY;
									IKMath.MatMultVec(out linkSolved, baseBasis, localFingerSolve);
								}
							}
						}

						if (!IKMath.ComputeBasisFromXZLockX(out beginLink.boneTransform.basis, isRight ? linkSolved : -linkSolved, solveDirZ)) {
							return false;
						}

						beginLink.boneTransform.origin = beginLinkPosition;
						IKMath.MatMultRet0(ref beginLink.boneTransform.basis, beginLink.solvedToBoneBasis);
					}

					{
						var bendingLink0Position = beginLink.boneTransform * (bendingLink0.bone._defaultPosition - beginLink.bone._defaultPosition);

						// Forcefix:
						var bendingLink0ToEffector = effectorPosition - bendingLink0Position;

						IKMath.MatMultVec(out var dirZ, beginLink.boneTransform.basis, _internalValues.defaultRootBasis.column2);

						solveDirY = Vector3f.Cross(dirZ, bendingLink0ToEffector);
						if (!IKMath.VecNormalize(ref solveDirY)) {
							return false;
						}

						solveDirY = isRight ? solveDirY : -solveDirY;

						var linkSolved = SolveLimbIK(
							ref bendingLink0Position,
							ref effectorPosition,
							linkLength1,
							linkLength1Sq,
							linkLength2,
							linkLength2Sq,
							ref solveDirY);

						if (linkSolved == Vector3f.Zero) {
							return false;
						}

						IKMath.MatMultVec(out dirZ, beginLink.boneTransform.basis, bendingLink0.boneToSolvedBasis.column2);

						if (!IKMath.ComputeBasisFromXZLockX(out bendingLink0.boneTransform.basis, isRight ? linkSolved : -linkSolved, dirZ)) {
							return false;
						}

						bendingLink0.boneTransform.origin = bendingLink0Position;
						IKMath.MatMultRet0(ref bendingLink0.boneTransform.basis, bendingLink0.solvedToBoneBasis);
					}

					{
						var bendingLink1Position = bendingLink0.boneTransform * (bendingLink1.bone._defaultPosition - bendingLink0.bone._defaultPosition);

						var dirX = effectorPosition - bendingLink1Position;
						if (!IKMath.VecNormalize(ref dirX)) {
							return false;
						}

						IKMath.MatMultVec(out var dirZ, bendingLink0.boneTransform.basis, bendingLink1.boneToSolvedBasis.column2);

						if (!IKMath.ComputeBasisFromXZLockX(out bendingLink1.boneTransform.basis, isRight ? dirX : -dirX, dirZ)) {
							return false;
						}

						bendingLink1.boneTransform.origin = bendingLink1Position;
						IKMath.MatMultRet0(ref bendingLink1.boneTransform.basis, bendingLink1.solvedToBoneBasis);
					}
				}

				IKMath.MatMultGetRot(out var worldRotation, beginLink.boneTransform.basis, beginLink.bone._defaultBasis);
				beginLink.bone.WorldRotation = worldRotation;
				IKMath.MatMultGetRot(out worldRotation, bendingLink0.boneTransform.basis, bendingLink0.bone._defaultBasis);
				bendingLink0.bone.WorldRotation = worldRotation;
				IKMath.MatMultGetRot(out worldRotation, bendingLink1.boneTransform.basis, bendingLink1.bone._defaultBasis);
				bendingLink1.bone.WorldRotation = worldRotation;
				return true;
			}

			bool SolveThumb(ref IKMatrix3x4 parentTransform) {
				var fingerBranch = _fingerBranches[(int)FingerType.Thumb];
				if (fingerBranch == null || fingerBranch.fingerLinks.Length != 3) {
					return false;
				}

				var fingerLink0 = fingerBranch.fingerLinks[0];
				var fingerLink1 = fingerBranch.fingerLinks[1];
				var fingerLink2 = fingerBranch.fingerLinks[2];

				var thumbLink0 = _thumbBranch.thumbLinks[0];
				var thumbLink1 = _thumbBranch.thumbLinks[1];
				var thumbLink2 = _thumbBranch.thumbLinks[2];

				var isRight = _fingerIKType == FingerIKType.RightWrist;

				{
					var fingerLinkPosition0 = parentTransform * (fingerLink0.bone._defaultPosition - _parentBone._defaultPosition);
					var endEffector = fingerBranch.effector;
					var effectorPosition = GetEffectorPosition(_internalValues, _parentBone, fingerLink0.bone, endEffector, fingerBranch.link0ToEffectorLength, ref parentTransform);
					var effectorTranslate = effectorPosition - fingerLinkPosition0;
					var effectorLength = effectorTranslate.Magnitude;
					if (effectorLength < IKMath.IK_EPSILON || fingerBranch.link0ToEffectorLength < IKMath.IK_EPSILON) {
						return false;
					}

					var effectorDirection = effectorTranslate * (1.0f / effectorLength);
					if (effectorLength > fingerBranch.link0ToEffectorLength) {
						//effectorLength = fingerBranch.link0ToEffectorLength;
						effectorTranslate = effectorDirection * fingerBranch.link0ToEffectorLength;
						effectorPosition = fingerLinkPosition0 + effectorTranslate;
					}

					{
						// thumb0 (1st pass.)
						// Simply, compute direction thumb0 to effector.
						var dirX = effectorDirection;

						// Limit yaw pitch for thumb0 to effector.VecLengthAndLengthSq
						if (_thumbBranch.thumb0_isLimited) {
							IKMath.MatMult(out var beginToEndBasis, parentTransform.basis, fingerBranch.boneToSolvedBasis);
							IKMath.MatMultVecInv(out var localEffectorDirection, beginToEndBasis, dirX);
							if (IKMath.LimitYZ(
								isRight,
								ref localEffectorDirection,
								_thumbBranch.thumb0_lowerLimit,
								_thumbBranch.thumb0_upperLimit,
								_thumbBranch.thumb0_innerLimit,
								_thumbBranch.thumb0_outerLimit)) {
								IKMath.MatMultVec(out dirX, beginToEndBasis, localEffectorDirection); // Local to world.
							}
						}

						IKMath.MatMultVec(out var dirY, parentTransform.basis, thumbLink0.thumb_boneToSolvedBasis.column1);

						if (!IKMath.ComputeBasisFromXYLockX(out fingerLink0.boneTransform.basis, isRight ? dirX : -dirX, dirY)) {
							return false;
						}

						fingerLink0.boneTransform.origin = fingerLinkPosition0;
						IKMath.MatMultRet0(ref fingerLink0.boneTransform.basis, thumbLink0.thumb_solvedToBoneBasis);
					}

					// thumb0 / Limit length based thumb1/2 (Type3)
					{
						var fingerLinkPosition1 = fingerLink0.boneTransform * (fingerLink1.bone._defaultPosition - fingerLink0.bone._defaultPosition);
						var effectorTranslate1to3 = effectorPosition - fingerLinkPosition1;
						var effectorLength1to3 = effectorTranslate1to3.Magnitude;

						if (effectorLength1to3 < _thumbBranch.linkLength1to3 - IKMath.IK_EPSILON) {
							var effectorTranslate0to3 = effectorPosition - fingerLink0.boneTransform.origin;
							var effectorLength0to3 = IKMath.VecLengthAndLengthSq(out var effectorLength0to3Sq, effectorTranslate0to3);

							var baseTheta = 1.0f;
							if (effectorLength0to3 > IKMath.IK_EPSILON) {
								var baseDirection0to1 = fingerLinkPosition1 - fingerLink0.boneTransform.origin;
								if (IKMath.VecNormalize(ref baseDirection0to1)) {
									var effectorDirection0to3 = effectorTranslate0to3 * (1.0f / effectorLength0to3);
									baseTheta = Vector3f.Dot(effectorDirection0to3, baseDirection0to1);
								}
							}

							var moveLenA = _thumbBranch.linkLength0to1;
							var moveLenASq = _thumbBranch.linkLength0to1Sq;
							var moveLenB = effectorLength0to3;
							var moveLenBSq = effectorLength0to3Sq;
							var moveLenC = effectorLength1to3 + (_thumbBranch.linkLength1to3 - effectorLength1to3) * 0.5f; // 0.5f = Magic number.(Balancer)
							var moveLenCSq = moveLenC * moveLenC;

							var moveTheta = ComputeTriangleTheta(moveLenA, moveLenB, moveLenC, moveLenASq, moveLenBSq, moveLenCSq);
							if (moveTheta < baseTheta) {
								var newAngle = IKMath.Acos(moveTheta) - IKMath.Acos(baseTheta);
								if (newAngle > 0.01f * MathUtil.DEG_2_RADF) {
									// moveLenAtoAD = Move length thumb1 origin with bending thumb0.
									var moveLenASq2 = moveLenASq * 2.0f;
									var moveLenAtoAD = IKMath.Sqrt(moveLenASq2 * (1.0f - IKMath.Cos(newAngle)));
									if (moveLenAtoAD > IKMath.IK_EPSILON) {
										IKMath.MatMultVec(out var solveDirection, fingerLink0.boneTransform.basis, _thumbBranch.thumbSolveZ);

										fingerLinkPosition1 += solveDirection * moveLenAtoAD;

										var newX = fingerLinkPosition1 - fingerLink0.boneTransform.origin;
										if (IKMath.VecNormalize(ref newX)) {
											IKMath.MatMultVec(out var dirY, fingerLink0.boneTransform.basis, fingerLink0.boneToSolvedBasis.column1);

											if (IKMath.ComputeBasisFromXYLockX(out var solveBasis0, isRight ? newX : -newX, dirY)) {
												IKMath.MatMult(out fingerLink0.boneTransform.basis, solveBasis0, fingerLink0.solvedToBoneBasis);
											}
										}
									}
								}
							}
						}
					}

					{
						// thumb1
						{
							var fingerLinkPosition1 = fingerLink0.boneTransform * (fingerLink1.bone._defaultPosition - fingerLink0.bone._defaultPosition);
							// Simply, compute direction thumb1 to effector.
							// (Compute push direction for thumb1.)
							var dirX = effectorPosition - fingerLinkPosition1;
							if (!IKMath.VecNormalize(ref dirX)) {
								return false;
							}

							IKMath.MatMultVec(out var dirY, fingerLink0.boneTransform.basis, thumbLink1.thumb_boneToSolvedBasis.column1);

							if (!IKMath.ComputeBasisFromXYLockX(out fingerLink1.boneTransform.basis, isRight ? dirX : -dirX, dirY)) {
								return false;
							}

							fingerLink1.boneTransform.origin = fingerLinkPosition1;
							IKMath.MatMultRet0(ref fingerLink1.boneTransform.basis, thumbLink1.thumb_solvedToBoneBasis);
						}

						var effectorTranslate1to3 = effectorPosition - fingerLink1.boneTransform.origin;
						var effectorLength1to3Sq = effectorTranslate1to3.SqrMagnitude;
						var effectorLength1to3 = IKMath.Sqrt(effectorLength1to3Sq);

						var moveLenA = _thumbBranch.linkLength1to2;
						var moveLenASq = _thumbBranch.linkLength1to2Sq;
						var moveLenB = effectorLength1to3;
						var moveLenBSq = effectorLength1to3Sq;
						var moveLenC = _thumbBranch.linkLength2to3;
						var moveLenCSq = _thumbBranch.linkLength2to3Sq;

						// Compute angle moved A/B origin.
						var moveThetaAtoB = ComputeTriangleTheta(moveLenA, moveLenB, moveLenC, moveLenASq, moveLenBSq, moveLenCSq);
						if (moveThetaAtoB < _thumbBranch.thumb1_baseThetaAtoB) {
							var newAngle = IKMath.Acos(moveThetaAtoB) - _thumbBranch.thumb1_Acos_baseThetaAtoB;
							if (newAngle > 0.01f * MathUtil.DEG_2_RADF) {
								var moveLenASq2 = moveLenASq * 2.0f;
								var moveLenAtoAD = IKMath.Sqrt(moveLenASq2 - moveLenASq2 * IKMath.Cos(newAngle));
								{
									IKMath.MatMultVec(out var solveDirection, fingerLink1.boneTransform.basis, _thumbBranch.thumbSolveZ);
									var fingerLinkPosition2 = fingerLink1.boneTransform * (fingerLink2.bone._defaultPosition - fingerLink1.bone._defaultPosition);
									fingerLinkPosition2 += solveDirection * moveLenAtoAD;

									var newX = fingerLinkPosition2 - fingerLink1.boneTransform.origin;
									if (IKMath.VecNormalize(ref newX)) {
										IKMath.MatMultVec(out var dirY, fingerLink1.boneTransform.basis, fingerLink1.boneToSolvedBasis.column1);
										if (IKMath.ComputeBasisFromXYLockX(out var solveBasis1, isRight ? newX : -newX, dirY)) {
											IKMath.MatMult(out fingerLink1.boneTransform.basis, solveBasis1, fingerLink1.solvedToBoneBasis);
										}
									}
								}
							}
						}
					}

					{
						// thumb2
						// Simply, compute direction thumb2 to effector.
						var fingerLinkPosition2 = fingerLink1.boneTransform * (fingerLink2.bone._defaultPosition - fingerLink1.bone._defaultPosition);
						var dirX = effectorPosition - fingerLinkPosition2;
						if (!IKMath.VecNormalize(ref dirX)) {
							return false;
						}

						IKMath.MatMultVec(out var dirY, fingerLink1.boneTransform.basis, thumbLink2.thumb_boneToSolvedBasis.column1);
						if (!IKMath.ComputeBasisFromXYLockX(out fingerLink2.boneTransform.basis, isRight ? dirX : -dirX, dirY)) {
							return false;
						}

						fingerLink2.boneTransform.origin = fingerLinkPosition2;
						IKMath.MatMultRet0(ref fingerLink2.boneTransform.basis, thumbLink2.thumb_solvedToBoneBasis);
					}
				}

				IKMath.MatMultGetRot(out var worldRotation, fingerLink0.boneTransform.basis, fingerLink0.bone._defaultBasis);
				fingerLink0.bone.WorldRotation = worldRotation;
				IKMath.MatMultGetRot(out worldRotation, fingerLink1.boneTransform.basis, fingerLink1.bone._defaultBasis);
				fingerLink1.bone.WorldRotation = worldRotation;
				IKMath.MatMultGetRot(out worldRotation, fingerLink2.boneTransform.basis, fingerLink2.bone._defaultBasis);
				fingerLink2.bone.WorldRotation = worldRotation;
				return true;
			}


		}


		public sealed class BodyIK
		{
			private readonly LimbIK[] _limbIK; // for UpperSolve. (Presolve Shoulder / Elbow)

			private readonly Bone _hipsBone; // Null accepted.
			private readonly Bone[] _spineBones; // Null accepted.
			private readonly bool[] _spineEnabled; // Null accepted.
			IKMatrix3x3[] _spinePrevCenterArmToChildBasis; // Null accepted.
			IKMatrix3x3[] _spineCenterArmToChildBasis; // Null accepted.

			private readonly Bone _spineBone; // Null accepted.
			private readonly Bone _spineUBone; // Null accepted.
			private readonly Bone _neckBone; // Null accepted.
			private readonly Bone _headBone; // Null accepted.
			private readonly Bone[] _kneeBones;
			private readonly Bone[] _elbowBones;
			private readonly Bone[] _legBones; // Null accepted.
			private readonly Bone[] _shoulderBones; // Null accepted.
			private readonly Bone[] _armBones; // Null accepted.
			private readonly Bone[] _nearArmBones; // _shouderBones or _armBones
			float[] _spineDirXRate;

			private readonly Effector _hipsEffector;
			private readonly Effector _neckEffector;
			private readonly Effector _headEffector;
			private readonly Effector _eyesEffector;

			private readonly Effector[] _armEffectors = new Effector[2];
			private readonly Effector[] _elbowEffectors = new Effector[2];
			private readonly Effector[] _wristEffectors = new Effector[2];
			private readonly Effector[] _kneeEffectors = new Effector[2];
			private readonly Effector[] _footEffectors = new Effector[2];

			Vector3f _defaultCenterLegPos = Vector3f.Zero;

			IKMatrix3x3 _centerLegBoneBasis = IKMatrix3x3.identity;
			IKMatrix3x3 _centerLegBoneBasisInv = IKMatrix3x3.identity;

			IKMatrix3x3 _centerLegToArmBasis = IKMatrix3x3.identity;        // dirX = armPos[1] - armPos[0] or shoulderPos[1] - shoulderPos[0], dirY = centerArmPos - centerLegPos
			IKMatrix3x3 _centerLegToArmBasisInv = IKMatrix3x3.identity;     // _centerLegToArmBasis.transpose
			IKMatrix3x3 _centerLegToArmBoneToBaseBasis = IKMatrix3x3.identity;
			IKMatrix3x3 _centerLegToArmBaseToBoneBasis = IKMatrix3x3.identity;

			private readonly float[] _shoulderToArmLength = new float[2];
			private readonly bool[] _shouderLocalAxisYInv = new bool[2];
			private readonly FastLength[] _elbowEffectorMaxLength = new FastLength[2];
			private readonly FastLength[] _wristEffectorMaxLength = new FastLength[2];
			private readonly FastLength[] _kneeEffectorMaxLength = new FastLength[2];
			private readonly FastLength[] _footEffectorMaxLength = new FastLength[2];

			public class SolverCaches
			{
				public Bone[] armBones;
				public Bone[] shoulderBones;
				public Bone[] nearArmBones;

				public float armToArmLen;
				public float nearArmToNearArmLen;
				public float[] shoulderToArmLength;
				public float[] nearArmToNeckLength = new float[2];
				public float neckToHeadLength;

				public float neckPull = 0.0f;
				public float headPull = 0.0f;
				public float eyesRate = 0.0f;
				public float neckHeadPull = 0.0f;
				public float[] armPull = new float[2];
				public float[] elbowPull = new float[2];
				public float[] wristPull = new float[2];
				public float[] kneePull = new float[2];
				public float[] footPull = new float[2];

				public float[] fullArmPull = new float[2]; // arm + elbow + wrist, max 1.0
				public float[] limbLegPull = new float[2]; // knee + foot, max 1.0

				public float[] armToElbowPull = new float[2]; // pull : arm / elbow
				public float[] armToWristPull = new float[2]; // pull : arm / wrist
				public float[] neckHeadToFullArmPull = new float[2]; // pull : (neck + head) / (arm + elbow + wrist)

				public float limbArmRate = 0.0f;
				public float limbLegRate = 0.0f;

				public float armToLegRate = 0.0f;

				public IKMatrix3x3 centerLegToNearArmBasis = IKMatrix3x3.identity; // dirX = armPos[1] - armPos[0] or shoulderPos[1] - shoulderPos[0], dirY = centerArmPos - centerLegPos
				public IKMatrix3x3 centerLegToNearArmBasisInv = IKMatrix3x3.identity; // centerLegToNearArmBasis.transpose
				public IKMatrix3x3 centerLegToNaerArmBoneToBaseBasis = IKMatrix3x3.identity;
				public IKMatrix3x3 centerLegToNaerArmBaseToBoneBasis = IKMatrix3x3.identity;

				public Vector3f defaultCenterLegPos = Vector3f.Zero;
			}

			private readonly SolverCaches _solverCaches = new();

			Vector3f _defaultCenterArmPos = Vector3f.Zero;
			float _defaultCenterLegLen; // LeftLeg to RightLeg Length.
			float _defaultCenterLegHalfLen; // LeftLeg to RightLeg Length / 2.
			float _defaultNearArmToNearArmLen = 0.0f;
			float _defaultCenterLegToCeterArmLen = 0.0f;

			Vector3f _defaultCenterEyePos = Vector3f.Zero;

			SolverInternal _solverInternal;

			private readonly Settings _settings;
			private readonly InternalValues _internalValues;

			public BodyIK(HumanoidIK fullBodyIK, LimbIK[] limbIK) {
				Assert(fullBodyIK != null);

				_limbIK = limbIK;

				_settings = fullBodyIK.settings;
				_internalValues = fullBodyIK.internalValues;

				_hipsBone = PrepareBone(fullBodyIK.bodyBones.hips);
				_neckBone = PrepareBone(fullBodyIK.headBones.neck);
				_headBone = PrepareBone(fullBodyIK.headBones.head);

				_hipsEffector = fullBodyIK.bodyEffectors.hips;
				_neckEffector = fullBodyIK.headEffectors.neck;
				_headEffector = fullBodyIK.headEffectors.head;
				_eyesEffector = fullBodyIK.headEffectors.eyes;

				_armEffectors[0] = fullBodyIK.leftArmEffectors.arm;
				_armEffectors[1] = fullBodyIK.rightArmEffectors.arm;
				_elbowEffectors[0] = fullBodyIK.leftArmEffectors.elbow;
				_elbowEffectors[1] = fullBodyIK.rightArmEffectors.elbow;
				_wristEffectors[0] = fullBodyIK.leftArmEffectors.wrist;
				_wristEffectors[1] = fullBodyIK.rightArmEffectors.wrist;
				_kneeEffectors[0] = fullBodyIK.leftLegEffectors.knee;
				_kneeEffectors[1] = fullBodyIK.rightLegEffectors.knee;
				_footEffectors[0] = fullBodyIK.leftLegEffectors.foot;
				_footEffectors[1] = fullBodyIK.rightLegEffectors.foot;

				_spineBones = PrepareSpineBones(fullBodyIK.Bones);
				if (_spineBones != null && _spineBones.Length > 0) {
					var spineLength = _spineBones.Length;
					_spineBone = _spineBones[0];
					_spineUBone = _spineBones[spineLength - 1];
					_spineEnabled = new bool[spineLength];
				}

				// Memo: These should be pair bones.(Necessary each side bones.)

				_kneeBones = PrepareBones(fullBodyIK.leftLegBones.knee, fullBodyIK.rightLegBones.knee);
				_elbowBones = PrepareBones(fullBodyIK.leftArmBones.elbow, fullBodyIK.rightArmBones.elbow);
				_legBones = PrepareBones(fullBodyIK.leftLegBones.leg, fullBodyIK.rightLegBones.leg);
				_armBones = PrepareBones(fullBodyIK.leftArmBones.arm, fullBodyIK.rightArmBones.arm);
				_shoulderBones = PrepareBones(fullBodyIK.leftArmBones.shoulder, fullBodyIK.rightArmBones.shoulder);
				_nearArmBones = _shoulderBones ?? _nearArmBones;

				Prepare();
			}

			static Bone[] PrepareSpineBones(Bone[] bones) {
				if (bones == null || bones.Length != (int)BoneLocation.Max) {
					Assert(false);
					return null;
				}

				var spineLength = 0;
				for (var i = (int)BoneLocation.Spine; i <= (int)BoneLocation.SpineU; ++i) {
					if (bones[i] != null && bones[i].TransformIsAlive) {
						++spineLength;
					}
				}

				if (spineLength == 0) {
					return null;
				}

				var spineBones = new Bone[spineLength];
				var index = 0;
				for (var i = (int)BoneLocation.Spine; i <= (int)BoneLocation.SpineU; ++i) {
					if (bones[i] != null && bones[i].TransformIsAlive) {
						spineBones[index] = bones[i];
						++index;
					}
				}

				return spineBones;
			}

			void Prepare() {
				if (_spineBones != null) {
					var spineLength = _spineBones.Length;
					_spineDirXRate = new float[spineLength];

					if (spineLength > 1) {
						_spinePrevCenterArmToChildBasis = new IKMatrix3x3[spineLength - 1];
						_spineCenterArmToChildBasis = new IKMatrix3x3[spineLength - 1];
						for (var i = 0; i != spineLength - 1; ++i) {
							_spinePrevCenterArmToChildBasis[i] = IKMatrix3x3.identity;
							_spineCenterArmToChildBasis[i] = IKMatrix3x3.identity;
						}
					}
				}
			}

			bool _isSyncDisplacementAtLeastOnce;

			void SyncDisplacement() {
				// Measure bone length.(Using worldPosition)
				// Force execution on 1st time. (Ignore case _settings.syncDisplacement == SyncDisplacement.Disable)
				if (_settings.syncDisplacement == HumanoidIK.SyncDisplacement.Everyframe || !_isSyncDisplacementAtLeastOnce) {
					_isSyncDisplacementAtLeastOnce = true;

					// Limit for Shoulder.
					if (_shoulderBones != null) {
						for (var i = 0; i != 2; ++i) {
							Assert(_shoulderBones[i] != null);
							var dirY = _shoulderBones[i]._localAxisBasis.column1;
							_shouderLocalAxisYInv[i] = Vector3f.Dot(dirY, _internalValues.defaultRootBasis.column1) < 0.0f;
						}
					}

					// _defaultCenterEyePos
					if (_eyesEffector != null) {
						_defaultCenterEyePos = _eyesEffector.DefaultPosition;
					}

					// _defaultCenterLegPos
					if (_legBones != null) {
						_defaultCenterLegPos = (_legBones[0]._defaultPosition + _legBones[1]._defaultPosition) * 0.5f;
					}

					// _defaultCenterArmPos, _centerLegToArmBasis, _centerLegToArmBasisInv, _centerLegToArmBoneToBaseBasis, _centerLegToArmBaseToBoneBasis

					if (_nearArmBones != null) {
						_defaultCenterArmPos = (_nearArmBones[1]._defaultPosition + _nearArmBones[0]._defaultPosition) * 0.5f;

						var dirX = _nearArmBones[1]._defaultPosition - _nearArmBones[0]._defaultPosition;
						var dirY = _defaultCenterArmPos - _defaultCenterLegPos;
						if (IKMath.VecNormalize(ref dirY) && IKMath.ComputeBasisFromXYLockY(out _centerLegToArmBasis, dirX, ref dirY)) {
							_centerLegToArmBasisInv = _centerLegToArmBasis.Transpose;
							IKMath.MatMult(out _centerLegToArmBoneToBaseBasis, _centerLegToArmBasisInv, _internalValues.defaultRootBasis);
							_centerLegToArmBaseToBoneBasis = _centerLegToArmBoneToBaseBasis.Transpose;
						}
					}

					_solverCaches.armBones = _armBones;
					_solverCaches.shoulderBones = _shoulderBones;
					_solverCaches.nearArmBones = _nearArmBones;
					_solverCaches.centerLegToNearArmBasis = _centerLegToArmBasis;
					_solverCaches.centerLegToNearArmBasisInv = _centerLegToArmBasisInv;
					_solverCaches.centerLegToNaerArmBoneToBaseBasis = _centerLegToArmBoneToBaseBasis;
					_solverCaches.centerLegToNaerArmBaseToBoneBasis = _centerLegToArmBaseToBoneBasis;
					_solverCaches.defaultCenterLegPos = _defaultCenterLegPos;

					_defaultCenterLegToCeterArmLen = IKMath.VecLength2(_defaultCenterLegPos, _defaultCenterArmPos);

					if (_footEffectors != null) {
						if (_footEffectors[0].Bone != null && _footEffectors[1].Bone != null) {
							_defaultCenterLegLen = IKMath.VecLength2(_footEffectors[0].Bone._defaultPosition, _footEffectors[1].Bone._defaultPosition);
							_defaultCenterLegHalfLen = _defaultCenterLegLen * 0.5f;
						}
					}

					if (_spineBone != null && _legBones != null) {
						if (ComputeCenterLegBasis(out _centerLegBoneBasis,
							ref _spineBone._defaultPosition,
							ref _legBones[0]._defaultPosition,
							ref _legBones[1]._defaultPosition)) {
							_centerLegBoneBasisInv = _centerLegBoneBasis.Transpose;
						}
					}

					// for UpperSolve.
					if (_armBones != null) {
						if (_shoulderBones != null) {
							for (var i = 0; i != 2; ++i) {
								_shoulderToArmLength[i] = _armBones[i]._defaultLocalLength.length;
							}
						}

						_solverCaches.armToArmLen = IKMath.VecLength2(_armBones[0]._defaultPosition, _armBones[1]._defaultPosition);
					}

					if (_nearArmBones != null) {
						_defaultNearArmToNearArmLen = IKMath.VecLength2(_nearArmBones[0]._defaultPosition, _nearArmBones[1]._defaultPosition);
						if (_neckBone != null && _neckBone.TransformIsAlive) {
							_solverCaches.nearArmToNeckLength[0] = IKMath.VecLength2(_neckBone._defaultPosition, _nearArmBones[0]._defaultPosition);
							_solverCaches.nearArmToNeckLength[1] = IKMath.VecLength2(_neckBone._defaultPosition, _nearArmBones[1]._defaultPosition);
						}
					}
					if (_neckBone != null && _headBone != null) {
						_solverCaches.neckToHeadLength = IKMath.VecLength2(_neckBone._defaultPosition, _headBone._defaultPosition);
					}

					_solverCaches.shoulderToArmLength = _shoulderToArmLength;
					_solverCaches.nearArmToNearArmLen = _defaultNearArmToNearArmLen;

					if (_kneeBones != null && _footEffectors != null) {
						for (var i = 0; i != 2; ++i) {
							var bendingBone = _kneeBones[i];
							var endBone = _footEffectors[i].Bone;
							_kneeEffectorMaxLength[i] = bendingBone._defaultLocalLength;
							_footEffectorMaxLength[i] = FastLength.FromLength(bendingBone._defaultLocalLength.length + endBone._defaultLocalLength.length);
						}
					}

					if (_elbowBones != null && _wristEffectors != null) {
						for (var i = 0; i != 2; ++i) {
							var bendingBone = _elbowBones[i];
							var endBone = _wristEffectors[i].Bone;
							_elbowEffectorMaxLength[i] = bendingBone._defaultLocalLength;
							_wristEffectorMaxLength[i] = FastLength.FromLength(bendingBone._defaultLocalLength.length + endBone._defaultLocalLength.length);
						}
					}

					if (_spineBones != null) {
						if ((_nearArmBones != null || _legBones != null) && _neckBone != null && _neckBone.TransformIsAlive) {
							//Vector3 armDirX = (_nearArmBones != null)
							//	? (_nearArmBones[1]._defaultPosition - _nearArmBones[0]._defaultPosition)
							//	: (_legBones[1]._defaultPosition - _legBones[0]._defaultPosition);

							var armDirX = _internalValues.defaultRootBasis.column0;

							var spineLength = _spineBones.Length;

							for (var i = 0; i < spineLength - 1; ++i) {
								var prevPos = i != 0 ? _spineBones[i - 1]._defaultPosition : _defaultCenterLegPos;
								var dirY0 = _defaultCenterArmPos - prevPos;
								var dirY1 = _defaultCenterArmPos - _spineBones[i]._defaultPosition;

								if (!IKMath.VecNormalize2(ref dirY0, ref dirY1)) {
									continue;
								}

								if (!IKMath.ComputeBasisFromXYLockY(out var prevToCenterArmBasis, armDirX, ref dirY0) ||
									!IKMath.ComputeBasisFromXYLockY(out var currToNeckBasis, armDirX, ref dirY1)) {
									continue;
								}

								IKMath.MatMultInv0(out _spinePrevCenterArmToChildBasis[i], prevToCenterArmBasis, _spineBones[i]._localAxisBasis);
								IKMath.MatMultInv0(out _spineCenterArmToChildBasis[i], currToNeckBasis, _spineBones[i]._localAxisBasis);
							}
						}
					}
				}
			}

			public bool Solve() {
				var isEffectorEnabled = IsEffectorEnabled();
				if (!isEffectorEnabled && !_settings.bodyIK.forceSolveEnabled) {
					return false;
				}

				SyncDisplacement();

				if (!PrepareSolverInternal()) {
					return false;
				}

				var temp = _solverInternal;

				if (!_internalValues.resetTransforms) {
					if (temp.spinePos != null) {
						for (var i = 0; i != _spineBones.Length; ++i) {
							if (_spineBones[i] != null) {
								temp.spinePos[i] = _spineBones[i].WorldPosition;
							}
						}
					}
					if (_neckBone != null) {
						temp.neckPos = _neckBone.WorldPosition;
					}
					if (_headBone != null) {
						temp.headPos = _headBone.WorldPosition;
					}
					if (temp.shoulderPos != null) {
						for (var i = 0; i < 2; ++i) {
							temp.shoulderPos[i] = _shoulderBones[i].WorldPosition;
						}
					}
					if (temp.armPos != null) {
						for (var i = 0; i != 2; ++i) {
							temp.armPos[i] = _armBones[i].WorldPosition;
						}
					}
					if (temp.legPos != null) {
						for (var i = 0; i != 2; ++i) {
							temp.legPos[i] = _legBones[i].WorldPosition;
						}
					}

					temp.SetDirtyVariables();
				}

				if (_internalValues.resetTransforms) {
					ResetTransforms();
				}
				else if (_internalValues.animatorEnabled) {
					PresolveHips();
				}

				if (!_internalValues.resetTransforms) {
					if (_settings.bodyIK.shoulderSolveEnabled) {
						ResetShoulderTransform();
					}
				}

				// Arms, Legs (Need calls after _ResetTransform() / _PresolveHips() / _ResetShoulderTransform().)
				// (Using temp.armPos[] / temp.legPos[])
				_solverInternal.arms.Prepare(_elbowEffectors, _wristEffectors);
				_solverInternal.legs.Prepare(_kneeEffectors, _footEffectors);

				if (_settings.bodyIK.lowerSolveEnabled) {
					LowerSolve(true);
				}

				if (_settings.bodyIK.upperSolveEnabled) {
					UpperSolve();
				}

				if (_settings.bodyIK.lowerSolveEnabled) {
					LowerSolve(false);
				}

				if (_settings.bodyIK.shoulderSolveEnabled) {
					ShoulderResolve();
				}

				if (_settings.bodyIK.computeWorldTransform) {
					ComputeWorldTransform();
				}

				return true;
			}

			bool UpperSolve() {
				var temp = _solverInternal;
				Assert(temp != null);

				var hipsPull = _hipsEffector.positionEnabled ? _hipsEffector.pull : 0.0f;
				var neckPull = _solverCaches.neckPull;
				var headPull = _solverCaches.headPull;
				var eyesRate = _solverCaches.eyesRate; // pull * positionWeight
				var armPull = _solverCaches.armPull;
				var elbowPull = _solverCaches.elbowPull;
				var wristPull = _solverCaches.wristPull;

				if (_settings.bodyIK.forceSolveEnabled) {
					// Nothing.
				}
				else {
					if (hipsPull <= IKMath.IK_EPSILON && neckPull <= IKMath.IK_EPSILON && headPull <= IKMath.IK_EPSILON && eyesRate <= IKMath.IK_EPSILON &&
						armPull[0] <= IKMath.IK_EPSILON && armPull[1] <= IKMath.IK_EPSILON &&
						elbowPull[0] <= IKMath.IK_EPSILON && elbowPull[1] <= IKMath.IK_EPSILON &&
						wristPull[0] <= IKMath.IK_EPSILON && wristPull[1] <= IKMath.IK_EPSILON) {
						return false; // No moved.
					}
				}

				var baseCenterLegPos = Vector3f.Zero; // for continuousSolver

				var continuousSolverEnabled = _internalValues.continuousSolverEnabled;

				// Preprocess for armPos / armPos2
				if (continuousSolverEnabled) {
					UpperSolve_PresolveBaseCenterLegTransform(out baseCenterLegPos, out var centerLegBasis);

					temp.Backup(); // for Testsolver.

					if (_spineBones != null) {
						for (var i = 0; i < _spineBones.Length; ++i) {
							IKMath.MatMultVecPreSubAdd(out temp.spinePos[i], centerLegBasis, _spineBones[i]._defaultPosition, _defaultCenterLegPos, baseCenterLegPos);
						}
					}
					if (_neckBone != null) {
						IKMath.MatMultVecPreSubAdd(out temp.neckPos, centerLegBasis, _neckBone._defaultPosition, _defaultCenterLegPos, baseCenterLegPos);
					}
					if (_headBone != null && temp.headEnabled) {
						IKMath.MatMultVecPreSubAdd(out temp.headPos, centerLegBasis, _headBone._defaultPosition, _defaultCenterLegPos, baseCenterLegPos);
					}
					for (var n = 0; n < 2; ++n) {
						if (_shoulderBones != null) {
							IKMath.MatMultVecPreSubAdd(out temp.shoulderPos[n], centerLegBasis, _shoulderBones[n]._defaultPosition, _defaultCenterLegPos, baseCenterLegPos);
						}
						if (_armBones != null) {
							IKMath.MatMultVecPreSubAdd(out temp.armPos[n], centerLegBasis, _armBones[n]._defaultPosition, _defaultCenterLegPos, baseCenterLegPos);
						}
						if (_legBones != null) {
							IKMath.MatMultVecPreSubAdd(out temp.legPos[n], centerLegBasis, _legBones[n]._defaultPosition, _defaultCenterLegPos, baseCenterLegPos);
						}
					}
					temp.SetDirtyVariables();
				}

				temp.UpperSolve();

				var upperIKMathLerpDir1Rate = _internalValues.bodyIK.upperCenterLegRotateRate.value;
				var upperIKMathLerpDir2Rate = _internalValues.bodyIK.upperSpineRotateRate.value;
				var upperLerpPos1Rate = _internalValues.bodyIK.upperCenterLegTranslateRate.value;
				var upperLerpPos2Rate = _internalValues.bodyIK.upperSpineTranslateRate.value;

				var targetCenterArmPos = temp.targetCenterArmPos;
				var targetCenterArmDir = temp.targetCenterArmDir;
				var currentCenterArmPos = temp.CurrentCenterArmPos;
				var currentCenterArmDir = temp.CurrentCenterArmDir;

				var centerArmDirX = IKMath.LerpDir(ref currentCenterArmDir, ref targetCenterArmDir, upperIKMathLerpDir1Rate);
				var centerArmDirX2 = IKMath.LerpDir(ref currentCenterArmDir, ref targetCenterArmDir, upperIKMathLerpDir2Rate);

				Vector3f centerArmPos, centerArmPos2;
				Vector3f centerArmDirY, centerArmDirY2;

				centerArmPos = Vector3f.Lerp(currentCenterArmPos, targetCenterArmPos, upperLerpPos1Rate);
				centerArmPos2 = Vector3f.Lerp(currentCenterArmPos, targetCenterArmPos, upperLerpPos2Rate);

				centerArmDirY = centerArmPos - temp.CenterLegPos;
				centerArmDirY2 = centerArmPos2 - temp.CenterLegPos;
				if (!IKMath.VecNormalize2(ref centerArmDirY, ref centerArmDirY2)) {
					return false;
				}

				if (_settings.bodyIK.upperDirXLimitEnabled) {
					if (_internalValues.bodyIK.upperDirXLimitThetaY.sin <= IKMath.IK_EPSILON) {
						if (!IKMath.FitToPlaneDir(ref centerArmDirX, centerArmDirY) ||
							!IKMath.FitToPlaneDir(ref centerArmDirX2, centerArmDirY2)) {
							return false;
						}
					}
					else {
						if (!IKMath.LimitToPlaneDirY(ref centerArmDirX, centerArmDirY, _internalValues.bodyIK.upperDirXLimitThetaY.sin) ||
							!IKMath.LimitToPlaneDirY(ref centerArmDirX2, centerArmDirY2, _internalValues.bodyIK.upperDirXLimitThetaY.sin)) {
							return false;
						}
					}
				}

				// Limit for spine.
				if (_settings.bodyIK.spineLimitEnabled) {
					var spineLimitAngleX = _internalValues.bodyIK.spineLimitAngleX.value;
					var spineLimitAngleY = _internalValues.bodyIK.spineLimitAngleY.value;

					if (_settings.bodyIK.spineAccurateLimitEnabled) { // Quaternion lerp.
						var fromToX = Vector3f.Dot(centerArmDirX, centerArmDirX2);
						var fromToXAng = IKMath.Acos(fromToX);
						if (fromToXAng > spineLimitAngleX) {
							var axisDir = Vector3f.Cross(centerArmDirX, centerArmDirX2);
							if (IKMath.VecNormalize(ref axisDir)) {
								var q = new Quaternionf(axisDir, _settings.bodyIK.spineLimitAngleX);
								IKMath.MatSetRot(out var rotateBasis, ref q);
								IKMath.MatMultVec(out centerArmDirX2, rotateBasis, centerArmDirX);
							}
						}

						var fromToY = Vector3f.Dot(centerArmDirY, centerArmDirY2);
						var fromToYAng = IKMath.Acos(fromToY);
						if (fromToYAng > spineLimitAngleY) {
							var axisDir = Vector3f.Cross(centerArmDirY, centerArmDirY2);
							if (IKMath.VecNormalize(ref axisDir)) {
								var q = new Quaternionf(axisDir, _settings.bodyIK.spineLimitAngleY);
								IKMath.MatSetRot(out var rotateBasis, ref q);
								IKMath.MatMultVec(out centerArmDirY2, rotateBasis, centerArmDirY);
							}
						}
					}
					else { // Lienar lerp.
						   // Recompute centerLegToArmBoneBasisTo2( for Spine )
						var fromToX = Vector3f.Dot(centerArmDirX, centerArmDirX2);
						var fromToXAng = IKMath.Acos(fromToX);
						if (fromToXAng > spineLimitAngleX) {
							if (fromToXAng > IKMath.IK_EPSILON) {
								var balancedRate = spineLimitAngleX / fromToXAng;
								var dirX2Balanced = Vector3f.Lerp(centerArmDirX, centerArmDirX2, balancedRate);
								if (IKMath.VecNormalize(ref dirX2Balanced)) {
									centerArmDirX2 = dirX2Balanced;
								}
							}
						}

						// Pending: spine stiffness.(Sin scale to balanced rate.)
						var fromToY = Vector3f.Dot(centerArmDirY, centerArmDirY2);
						var fromToYAng = IKMath.Acos(fromToY);
						if (fromToYAng > spineLimitAngleY) {
							if (fromToYAng > IKMath.IK_EPSILON) {
								var balancedRate = spineLimitAngleY / fromToYAng;
								var dirY2Balanced = Vector3f.Lerp(centerArmDirY, centerArmDirY2, balancedRate);
								if (IKMath.VecNormalize(ref dirY2Balanced)) {
									centerArmDirY2 = dirY2Balanced;
								}
							}
						}
					}
				}

				// This is missing. todo: Fix
				var presolveCenterLegPos = temp.CenterLegPos; // for continuousSolverEnabled

				// for eyes.
				// presolvedCenterLegPos2 = presolveCenterLegPos + presolved postTranslate.
				var presolvedCenterLegPos2 = temp.CenterLegPos;
				if (eyesRate > IKMath.IK_EPSILON) {
					var source = temp.CenterLegPos + centerArmDirY2 * _defaultCenterLegToCeterArmLen;
					presolvedCenterLegPos2 += temp.targetCenterArmPos - source;
				}

				// Eyes
				if (eyesRate > IKMath.IK_EPSILON) {
					// Based on centerArmDirX2 / centerArmDirY2
					if (IKMath.ComputeBasisFromXYLockY(out var toBasis, centerArmDirX2, ref centerArmDirY2)) {
						IKMath.MatMult(out var toBasisGlobal, toBasis, _centerLegToArmBasisInv);

						var fromBasis = toBasis;
						IKMath.MatMultRet0(ref toBasis, _centerLegToArmBoneToBaseBasis);

						IKMath.MatMultVecPreSubAdd(out var eyePos, toBasisGlobal, _defaultCenterEyePos, _defaultCenterLegPos, presolvedCenterLegPos2);

						{
							var upperEyesXLimit = _internalValues.bodyIK.upperEyesLimitYaw.sin;
							var upperEyesYUpLimit = _internalValues.bodyIK.upperEyesLimitPitchUp.sin;
							var upperEyesYDownLimit = _internalValues.bodyIK.upperEyesLimitPitchDown.sin;

							// Memo: Not use _eyesEffector._hidden_worldPosition
							IKMath.MatMultVecInv(out var eyeDir, toBasis, _eyesEffector.WorldPosition - eyePos); // to Local

							eyeDir.x *= _settings.bodyIK.upperEyesYawRate;
							if (eyeDir.y >= 0.0f) {
								eyeDir.y *= _settings.bodyIK.upperEyesPitchUpRate;
							}
							else {
								eyeDir.y *= _settings.bodyIK.upperEyesPitchDownRate;
							}

							IKMath.VecNormalize(ref eyeDir);

							if (ComputeEyesRange(ref eyeDir, _internalValues.bodyIK.upperEyesTraceTheta.cos)) {
								IKMath.LimitXY(ref eyeDir, upperEyesXLimit, upperEyesXLimit, upperEyesYDownLimit, upperEyesYUpLimit);
							}

							IKMath.MatMultVec(out eyeDir, toBasis, eyeDir); // to Global

							{
								var xDir = toBasis.column0;
								var yDir = toBasis.column1;
								var zDir = eyeDir;

								if (IKMath.ComputeBasisLockZ(out toBasis, xDir, yDir, ref zDir)) {
									// Nothing.
								}
							}
						}

						IKMath.MatMultRet0(ref toBasis, _centerLegToArmBaseToBoneBasis);

						var upperEyesRate1 = _settings.bodyIK.upperEyesToCenterLegRate * eyesRate;
						var upperEyesRate2 = _settings.bodyIK.upperEyesToSpineRate * eyesRate;

						IKMatrix3x3 solveBasis;
						if (upperEyesRate2 > IKMath.IK_EPSILON) {
							IKMath.MatFastLerp(out solveBasis, ref fromBasis, ref toBasis, upperEyesRate2);
							centerArmDirX2 = solveBasis.column0;
							centerArmDirY2 = solveBasis.column1;
						}

						if (upperEyesRate1 > IKMath.IK_EPSILON) {
							if (IKMath.ComputeBasisFromXYLockY(out fromBasis, centerArmDirX, ref centerArmDirY)) {
								IKMath.MatFastLerp(out solveBasis, ref fromBasis, ref toBasis, upperEyesRate1);
								centerArmDirX = solveBasis.column0;
								centerArmDirY = solveBasis.column1;
							}
						}
					}
				}

				if (continuousSolverEnabled) {
					// Salvage bone positions at end of testsolver.
					temp.Restore();

					temp.UpperSolve();
				}

				var spineLength = _spineBones != null ? _spineBones.Length : 0;

				var stableCenterLegRate = _settings.bodyIK.upperContinuousCenterLegRotationStableRate;

				// centerLeg(Hips)
				if (_settings.bodyIK.upperSolveHipsEnabled) {
					if (IKMath.ComputeBasisFromXYLockY(out var toBasis, centerArmDirX, ref centerArmDirY)) {
						var rotateBasis = IKMatrix3x3.identity;

						if (_internalValues.animatorEnabled || _internalValues.resetTransforms) {
							// for animatorEnabled or resetTransform(Base on armPos)
							if (continuousSolverEnabled && stableCenterLegRate > IKMath.IK_EPSILON) {
								var solveDirY = centerArmPos - presolveCenterLegPos;
								var solveDirX = centerArmDirX;
								if (IKMath.VecNormalize(ref solveDirY) && IKMath.ComputeBasisFromXYLockY(out var presolveCenterLegBasis, solveDirX, ref solveDirY)) {
									IKMath.MatFastLerp(out var tempBasis, ref toBasis, ref presolveCenterLegBasis, stableCenterLegRate);
									toBasis = tempBasis;
								}
							}

							var currentDirX = temp.nearArmPos[1] - temp.nearArmPos[0];
							var currentDirY = (temp.nearArmPos[1] + temp.nearArmPos[0]) * 0.5f - temp.CenterLegPos;

							if (IKMath.VecNormalize(ref currentDirY) && IKMath.ComputeBasisFromXYLockY(out var fromBasis, currentDirX, ref currentDirY)) {
								IKMath.MatMultInv1(out rotateBasis, ref toBasis, ref fromBasis);
							}
						}
						else { // for continuousSolverEnabled.(Base on centerLegBasis)
							IKMath.MatMultRet0(ref toBasis, _centerLegToArmBasisInv);

							if (continuousSolverEnabled && stableCenterLegRate > IKMath.IK_EPSILON) {
								var solveDirY = centerArmPos - presolveCenterLegPos;
								var solveDirX = centerArmDirX;
								if (IKMath.VecNormalize(ref solveDirY) && IKMath.ComputeBasisFromXYLockY(out var presolveCenterLegBasis, solveDirX, ref solveDirY)) {
									IKMath.MatMultRet0(ref presolveCenterLegBasis, _centerLegToArmBasisInv);
									IKMath.MatFastLerp(out var tempBasis, ref toBasis, ref presolveCenterLegBasis, stableCenterLegRate);
									toBasis = tempBasis;
								}
							}

							var centerLegBasis = temp.CenterLegBasis;
							IKMath.MatMultInv1(out rotateBasis, ref toBasis, ref centerLegBasis);
						}

						if (_settings.bodyIK.upperCenterLegLerpRate < 1.0f - IKMath.IK_EPSILON) {
							IKMath.MatFastLerpToIdentity(ref rotateBasis, 1.0f - _settings.bodyIK.upperCenterLegLerpRate);
						}

						temp.UpperRotation(-1, ref rotateBasis);
					}
				}

				{
					var centerLegToArmLength = _defaultCenterLegToCeterArmLen;

					var centerLegBasisX = temp.CenterLegBasis.column0;
					var centerArmPosY2 = centerArmDirY2 * centerLegToArmLength + temp.CenterLegPos;
					var upperSpineLerpRate = _settings.bodyIK.upperSpineLerpRate;

					for (var i = 0; i != spineLength; ++i) {
						if (!_spineEnabled[i]) {
							continue;
						}

						var origPos = temp.spinePos[i];

						if (i + 1 == spineLength) {
							var currentDirX = temp.nearArmPos[1] - temp.nearArmPos[0];
							var currentDirY = temp.CenterArmPos - origPos;

							var targetDirX = centerArmDirX2;
							var targetDirY = centerArmPosY2 - origPos;

							if (!IKMath.VecNormalize2(ref currentDirY, ref targetDirY)) {
								continue; // Skip.
							}

							IKMath.ComputeBasisFromXYLockY(out var toBasis, targetDirX, ref targetDirY);
							IKMath.ComputeBasisFromXYLockY(out var fromBasis, currentDirX, ref currentDirY);

							IKMath.MatMultInv1(out var rotateBasis, ref toBasis, ref fromBasis);

							if (upperSpineLerpRate < 1.0f - IKMath.IK_EPSILON) {
								IKMath.MatFastLerpToIdentity(ref rotateBasis, 1.0f - upperSpineLerpRate);
							}

							temp.UpperRotation(i, ref rotateBasis);
						}
						else {
							var childPos = i + 1 == spineLength ? temp.neckPos : temp.spinePos[i + 1];
							var prevPos = i != 0 ? temp.spinePos[i - 1] : temp.CenterLegPos;

							var currentDirX = temp.nearArmPos[1] - temp.nearArmPos[0];
							var currentDirY = childPos - origPos;

							var targetDirX = centerArmDirX2;
							var targetDirY = centerArmPosY2 - prevPos;
							var targetDirY2 = centerArmPosY2 - origPos;

							if (!IKMath.VecNormalize4(ref currentDirX, ref currentDirY, ref targetDirY, ref targetDirY2)) {
								continue; // Skip.
							}


							if (!IKMath.ComputeBasisFromXYLockY(out var prevToNearArmBasis, currentDirX, ref targetDirY) ||
								!IKMath.ComputeBasisFromXYLockY(out var origToNearArmBasis, currentDirX, ref targetDirY2)) {
								continue;
							}

							// Get prevPos to child dir.
							IKMath.MatMultCol1(out targetDirY, prevToNearArmBasis, _spinePrevCenterArmToChildBasis[i]);
							IKMath.MatMultCol1(out targetDirY2, origToNearArmBasis, _spineCenterArmToChildBasis[i]);

							var spineDirXLegToArmRate = _spineDirXRate[i];

							// Simply Lerp.(dirX)
							currentDirX = Vector3f.Lerp(centerLegBasisX, currentDirX, spineDirXLegToArmRate);
							targetDirX = Vector3f.Lerp(centerLegBasisX, targetDirX, spineDirXLegToArmRate);

							// Simply Lerp.(dirY)
							if (i + 1 != spineLength) { // Exclude spineU
								targetDirY = Vector3f.Lerp(targetDirY, targetDirY2, _settings.bodyIK.spineDirYLerpRate);
								if (!IKMath.VecNormalize(ref targetDirY)) { // Failsafe.
									targetDirY = currentDirY;
								}
							}

							IKMath.ComputeBasisFromXYLockY(out var toBasis, targetDirX, ref targetDirY);
							IKMath.ComputeBasisFromXYLockY(out var fromBasis, currentDirX, ref currentDirY);
							IKMath.MatMultInv1(out var rotateBasis, ref toBasis, ref fromBasis);

							if (upperSpineLerpRate < 1.0f - IKMath.IK_EPSILON) {
								IKMath.MatFastLerpToIdentity(ref rotateBasis, 1.0f - upperSpineLerpRate);
							}

							temp.UpperRotation(i, ref rotateBasis);
						}
					}
				}

				UpperSolve_Translate2(
					ref _internalValues.bodyIK.upperPostTranslateRate,
					ref _internalValues.bodyIK.upperContinuousPostTranslateStableRate,
					ref baseCenterLegPos);

				return true;
			}

			void ShoulderResolve() {
				var temp = _solverInternal;
				Assert(temp != null);
				temp.ShoulderResolve();
			}

			void UpperSolve_PresolveBaseCenterLegTransform(out Vector3f centerLegPos, out IKMatrix3x3 centerLegBasis) {
				Assert(_internalValues != null && _internalValues.continuousSolverEnabled);
				var temp = _solverInternal;
				var cache = _solverCaches;
				Assert(temp != null);
				Assert(cache != null);

				GetBaseCenterLegTransform(out centerLegPos, out centerLegBasis);

				if (_legBones == null || !_legBones[0].TransformIsAlive || !_legBones[1].TransformIsAlive) {
					return;
				}

				if (cache.limbLegPull[0] <= IKMath.IK_EPSILON && cache.limbLegPull[1] <= IKMath.IK_EPSILON) {
					return; // Pull nothing.
				}

				IKMath.MatMultVecPreSubAdd(out var legPos0, centerLegBasis, _legBones[0]._defaultPosition, _defaultCenterLegPos, centerLegPos);
				IKMath.MatMultVecPreSubAdd(out var legPos1, centerLegBasis, _legBones[1]._defaultPosition, _defaultCenterLegPos, centerLegPos);

				var isLimited = false;
				isLimited |= temp.legs.SolveTargetBeginPos(0, ref legPos0);
				isLimited |= temp.legs.SolveTargetBeginPos(1, ref legPos1);

				if (isLimited) {
					var vecX = centerLegBasis.column0 * _defaultCenterLegHalfLen;
					centerLegPos = Vector3f.Lerp(legPos0 + vecX, legPos1 - vecX, cache.limbLegRate);
				}
			}

			bool UpperSolve_PreTranslate2(out Vector3f translate, ref CachedRate01 translateRate, ref CachedRate01 stableRate, ref Vector3f stableCenterLegPos) {
				// If resetTransform = false, contain targetBeginPos to default transform or modify _UpperSolve_Translate()

				// Memo: Prepare SolveTargetBeginPosRated().

				translate = Vector3f.Zero;

				Assert(_hipsEffector != null);
				if (_hipsEffector.positionEnabled &&
					_hipsEffector.positionWeight <= IKMath.IK_EPSILON &&
					_hipsEffector.pull >= 1.0f - IKMath.IK_EPSILON) {
					return false; // Always locked.
				}

				var temp = _solverInternal;
				Assert(temp != null);

				var continuousSolverEnabled = _internalValues.continuousSolverEnabled;

				var translateEnabled = continuousSolverEnabled && stableRate.isGreater0;
				if (temp.targetCenterArmEnabled) {
					translate = temp.targetCenterArmPos - temp.CurrentCenterArmPos;
					translateEnabled = true;
				}

				if (translateEnabled) {
					if (translateRate.isLess1) {
						translate *= translateRate.value;
					}

					if (continuousSolverEnabled && stableRate.isGreater0) {
						var extraTranslate = stableCenterLegPos - temp.CenterLegPos;
						translate = Vector3f.Lerp(translate, extraTranslate, stableRate.value);
					}

					if (_hipsEffector.positionEnabled && _hipsEffector.pull > IKMath.IK_EPSILON) {
						var extraTranslate = _hipsEffector._hidden_worldPosition - temp.CenterLegPos;
						translate = Vector3f.Lerp(translate, extraTranslate, _hipsEffector.pull);
					}

					return true;
				}

				return false;
			}

			void UpperSolve_Translate2(ref CachedRate01 translateRate, ref CachedRate01 stableRate, ref Vector3f stableCenterLegPos) {
				if (UpperSolve_PreTranslate2(out var translate, ref translateRate, ref stableRate, ref stableCenterLegPos)) {
					var temp = _solverInternal;
					Assert(temp != null);
					temp.Translate(ref translate);
				}
			}

			void LowerSolve(bool firstPass) {
				var temp = _solverInternal;
				var cache = _solverCaches;
				Assert(temp != null);
				Assert(cache != null);
				if (temp.spinePos == null || temp.spinePos.Length == 0) {
					return;
				}

				var limbRate = firstPass ? 1.0f : cache.armToLegRate;

				if (temp.PrepareLowerRotation(0)) {
					var centerLegBoneY = temp.CenterLegBasis.column1;
					for (var i = 0; i < 2; ++i) {
						if (temp.legs.endPosEnabled[i] && temp.legs.targetBeginPosEnabled[i]) {
							var legDir = temp.legs.targetBeginPos[i] - temp.legs.beginPos[i];
							if (IKMath.VecNormalize(ref legDir)) {
								var legDirFeedbackRate = Vector3f.Dot(centerLegBoneY, -legDir);
								legDirFeedbackRate = MathUtil.Clamp(legDirFeedbackRate, 0, 1);
								legDirFeedbackRate = 1.0f - legDirFeedbackRate;
								temp.SetSolveFeedbackRate(i, legDirFeedbackRate * limbRate);
							}
						}
					}

					if (temp.SolveLowerRotation(0, out var origLowerRotation)) {
						temp.LowerRotation(0, ref origLowerRotation, false);
					}
				}

				if (_hipsEffector.positionEnabled &&
					_hipsEffector.positionWeight <= IKMath.IK_EPSILON &&
					_hipsEffector.pull >= 1.0f - IKMath.IK_EPSILON) {
					// Nothing.(Always locked.)
				}
				else {
					if (temp.PrepareLowerTranslate()) {
						if (temp.SolveLowerTranslate(out var origLowerTranslate)) {
							if (limbRate < 1.0f - IKMath.IK_EPSILON) {
								origLowerTranslate *= limbRate;
							}

							if (_hipsEffector.positionEnabled && _hipsEffector.pull > IKMath.IK_EPSILON) {
								var extraTranslate = _hipsEffector._hidden_worldPosition - temp.CenterLegPos;
								origLowerTranslate = Vector3f.Lerp(origLowerTranslate, extraTranslate, _hipsEffector.pull);
							}

							temp.Translate(ref origLowerTranslate);
						}
					}
				}
			}

			void ComputeWorldTransform() {
				var temp = _solverInternal;
				if (temp == null || temp.spinePos == null || temp.spinePos.Length == 0) {
					return;
				}

				// Compute worldPosition / worldRotation.
				if (_hipsBone != null && _hipsBone.TransformIsAlive && temp.spinePos != null && temp.spinePos.Length > 0 && _neckBone != null && _neckBone.TransformIsAlive) {
					var hipsToSpineDirX = new Vector3f(1.0f, 0.0f, 0.0f);

					var dirX = temp.legs.beginPos[1] - temp.legs.beginPos[0];
					var dirY = temp.spinePos[0] - (temp.legs.beginPos[1] + temp.legs.beginPos[0]) * 0.5f;

					var boneBasis = new IKMatrix3x3();

					if (IKMath.VecNormalize(ref dirY) && IKMath.ComputeBasisFromXYLockY(out boneBasis, dirX, ref dirY)) {
						IKMath.MatMult(out var tempBasis, boneBasis, _centerLegBoneBasisInv);

						hipsToSpineDirX = boneBasis.column0; // Counts as baseBasis.

						IKMath.MatMultGetRot(out var worldRotation, tempBasis, _hipsBone._defaultBasis);
						_hipsBone.WorldRotation = worldRotation;

						if (_hipsBone.IsWritebackWorldPosition) {
							var inv_defaultLocalTranslate = -_spineBone._defaultLocalTranslate;
							IKMath.MatMultVecAdd(out var worldPosition, tempBasis, inv_defaultLocalTranslate, temp.spinePos[0]);
							_hipsBone.WorldPosition = worldPosition;
						}
					}
					else { // Failsafe.
						if (IKMath.VecNormalize(ref dirX)) {
							hipsToSpineDirX = dirX;
						}
					}

					var spineLength = temp.spinePos.Length;
					for (var i = 0; i != spineLength; ++i) {
						if (!_spineEnabled[i]) {
							continue;
						}

						if (i + 1 == spineLength) {
							dirY = temp.neckPos - temp.spinePos[i];
							dirX = temp.nearArmPos != null ? temp.nearArmPos[1] - temp.nearArmPos[0] : hipsToSpineDirX;
						}
						else {
							dirY = temp.spinePos[i + 1] - temp.spinePos[i];
							dirX = hipsToSpineDirX;
							if (temp.nearArmPos != null) {
								var dirX0 = temp.nearArmPos[1] - temp.nearArmPos[0];
								if (IKMath.VecNormalize(ref dirX0)) {
									dirX = Vector3f.Lerp(dirX, dirX0, _settings.bodyIK.spineDirXLegToArmRate);
								}
							}
						}

						if (IKMath.VecNormalize(ref dirY) && IKMath.ComputeBasisFromXYLockY(out boneBasis, dirX, ref dirY)) {
							hipsToSpineDirX = boneBasis.column0;
							IKMath.MatMultGetRot(out var worldRotation, boneBasis, _spineBones[i]._boneToWorldBasis);
							_spineBones[i].WorldRotation = worldRotation;
							if (_spineBones[i].IsWritebackWorldPosition) {
								_spineBones[i].WorldPosition = temp.spinePos[i];
							}
						}
					}

					if (_shoulderBones != null) {
						for (var i = 0; i != 2; ++i) {
							Vector3f xDir, yDir, zDir;
							xDir = temp.armPos[i] - temp.shoulderPos[i];
							yDir = _internalValues.shoulderDirYAsNeck != 0 ? temp.neckPos - temp.shoulderPos[i] : temp.shoulderPos[i] - temp.SpineUPos;
							xDir = i == 0 ? -xDir : xDir;
							zDir = Vector3f.Cross(xDir, yDir);
							yDir = Vector3f.Cross(zDir, xDir);

							if (IKMath.VecNormalize3(ref xDir, ref yDir, ref zDir)) {
								boneBasis.SetColumn(ref xDir, ref yDir, ref zDir);
								IKMath.MatMultGetRot(out var worldRotation, boneBasis, _shoulderBones[i]._boneToWorldBasis);
								_shoulderBones[i].WorldRotation = worldRotation;
							}
						}
					}
				}
			}

			bool IsEffectorEnabled() {
				return _hipsEffector.positionEnabled && _hipsEffector.pull > IKMath.IK_EPSILON ||
					_hipsEffector.rotationEnabled && _hipsEffector.rotationWeight > IKMath.IK_EPSILON ||
					_neckEffector.positionEnabled && _neckEffector.pull > IKMath.IK_EPSILON ||
					_eyesEffector.positionEnabled && _eyesEffector.positionWeight > IKMath.IK_EPSILON && _eyesEffector.pull > IKMath.IK_EPSILON ||
					_armEffectors[0].positionEnabled && _armEffectors[0].pull > IKMath.IK_EPSILON ||
					_armEffectors[1].positionEnabled && _armEffectors[1].pull > IKMath.IK_EPSILON
					|| _elbowEffectors[0].positionEnabled && _elbowEffectors[0].pull > IKMath.IK_EPSILON ||
					_elbowEffectors[1].positionEnabled && _elbowEffectors[1].pull > IKMath.IK_EPSILON ||
					_kneeEffectors[0].positionEnabled && _kneeEffectors[0].pull > IKMath.IK_EPSILON ||
					_kneeEffectors[1].positionEnabled && _kneeEffectors[1].pull > IKMath.IK_EPSILON ||
					_wristEffectors[0].positionEnabled && _wristEffectors[0].pull > IKMath.IK_EPSILON ||
					_wristEffectors[1].positionEnabled && _wristEffectors[1].pull > IKMath.IK_EPSILON ||
					_footEffectors[0].positionEnabled && _footEffectors[0].pull > IKMath.IK_EPSILON ||
					_footEffectors[1].positionEnabled && _footEffectors[1].pull > IKMath.IK_EPSILON;
			}

			bool PrepareSolverInternal() {
				if (_armBones == null || _legBones == null) {
					_solverInternal = null;
					return false;
				}

				// Get pull values at SolverCaches.
				if (_neckEffector != null) {
					_solverCaches.neckPull = _neckEffector.positionEnabled ? _neckEffector.pull : 0.0f;
				}
				if (_headEffector != null) {
					_solverCaches.headPull = _headEffector.positionEnabled ? _headEffector.pull : 0.0f;
				}
				if (_eyesEffector != null) {
					_solverCaches.eyesRate = _eyesEffector.positionEnabled ? _eyesEffector.pull * _eyesEffector.positionWeight : 0.0f;
				}
				for (var i = 0; i != 2; ++i) {
					if (_armEffectors[i] != null) {
						_solverCaches.armPull[i] = _armEffectors[i].positionEnabled ? _armEffectors[i].pull : 0.0f;
					}
					if (_elbowEffectors[i] != null) {
						_solverCaches.elbowPull[i] = _elbowEffectors[i].positionEnabled ? _elbowEffectors[i].pull : 0.0f;
					}
					if (_wristEffectors[i] != null) {
						_solverCaches.wristPull[i] = _wristEffectors[i].positionEnabled ? _wristEffectors[i].pull : 0.0f;
					}
					if (_kneeEffectors[i] != null) {
						_solverCaches.kneePull[i] = _kneeEffectors[i].positionEnabled ? _kneeEffectors[i].pull : 0.0f;
					}
					if (_footEffectors[i] != null) {
						_solverCaches.footPull[i] = _footEffectors[i].positionEnabled ? _footEffectors[i].pull : 0.0f;
					}
				}

				_solverCaches.neckHeadPull = ConcatPull(_solverCaches.neckPull, _solverCaches.headPull);

				// Update pull values at SolverInternal.
				var upperPull = _solverCaches.neckHeadPull;
				var lowerPull = 0.0f;
				for (var i = 0; i != 2; ++i) {
					var fullArmPull = _solverCaches.armPull[i];
					fullArmPull = fullArmPull == 0.0f ? _solverCaches.elbowPull[i] : ConcatPull(fullArmPull, _solverCaches.elbowPull[i]);
					fullArmPull = fullArmPull == 0.0f ? _solverCaches.wristPull[i] : ConcatPull(fullArmPull, _solverCaches.wristPull[i]);

					//var limbArmPull = _solverCaches.kneePull[i];
					//limbArmPull = limbArmPull == 0.0f ? _solverCaches.wristPull[i] : ConcatPull(limbArmPull, _solverCaches.wristPull[i]);
					var legPull = _solverCaches.kneePull[i];
					legPull = legPull == 0.0f ? _solverCaches.footPull[i] : ConcatPull(legPull, _solverCaches.footPull[i]);

					_solverCaches.fullArmPull[i] = fullArmPull;
					_solverCaches.limbLegPull[i] = legPull;

					_solverCaches.armToElbowPull[i] = GetBalancedPullLockFrom(_solverCaches.armPull[i], _solverCaches.elbowPull[i]);
					_solverCaches.armToWristPull[i] = GetBalancedPullLockFrom(_solverCaches.armPull[i], _solverCaches.wristPull[i]);
					_solverCaches.neckHeadToFullArmPull[i] = GetBalancedPullLockTo(_solverCaches.neckHeadPull, fullArmPull);

					upperPull += fullArmPull;
					lowerPull += legPull;
				}

				_solverCaches.limbArmRate = GetLerpRateFromPull2(_solverCaches.fullArmPull[0], _solverCaches.fullArmPull[1]);
				_solverCaches.limbLegRate = GetLerpRateFromPull2(_solverCaches.limbLegPull[0], _solverCaches.limbLegPull[1]);
				_solverCaches.armToLegRate = GetLerpRateFromPull2(upperPull, lowerPull);

				// _spineDirXRate, _spineEnabled
				if (_spineBones != null) {
					var spineLength = _spineBones.Length;
					Assert(_spineDirXRate != null && _spineDirXRate.Length == spineLength);

					var spineDirXRate = MathUtil.Clamp(_settings.bodyIK.spineDirXLegToArmRate, 0, 1);
					var spineDirXToRate = MathF.Max(_settings.bodyIK.spineDirXLegToArmToRate, spineDirXRate);

					for (var i = 0; i != spineLength; ++i) {
						_spineDirXRate[i] = i == 0
							? spineDirXRate
							: i + 1 == spineLength
								? spineDirXToRate
								: spineDirXRate + (spineDirXToRate - spineDirXRate) * (i / (float)(spineLength - 1));
					}

					if (spineLength > 0) {
						_spineEnabled[0] = _settings.bodyIK.upperSolveSpineEnabled;
					}
					if (spineLength > 1) {
						_spineEnabled[1] = _settings.bodyIK.upperSolveSpine2Enabled;
					}
					if (spineLength > 2) {
						_spineEnabled[2] = _settings.bodyIK.upperSolveSpine3Enabled;
					}
					if (spineLength > 3) {
						_spineEnabled[3] = _settings.bodyIK.upperSolveSpine4Enabled;
					}
				}

				//------------------------------------------------------------------------------------------------------------------------

				// Allocate solverInternal.
				if (_solverInternal == null) {
					_solverInternal = new() {
						settings = _settings,
						internalValues = _internalValues,
						_solverCaches = _solverCaches
					};
					_solverInternal.arms._bendingPull = _solverCaches.armToElbowPull;
					_solverInternal.arms._endPull = _solverCaches.armToWristPull;
					_solverInternal.arms._beginToBendingLength = _elbowEffectorMaxLength;
					_solverInternal.arms._beginToEndLength = _wristEffectorMaxLength;
					_solverInternal.legs._bendingPull = _solverCaches.kneePull;
					_solverInternal.legs._endPull = _solverCaches.footPull;
					_solverInternal.legs._beginToBendingLength = _kneeEffectorMaxLength;
					_solverInternal.legs._beginToEndLength = _footEffectorMaxLength;

					_solverInternal._shouderLocalAxisYInv = _shouderLocalAxisYInv;
					_solverInternal._armEffectors = _armEffectors;
					_solverInternal._wristEffectors = _wristEffectors;
					_solverInternal._neckEffector = _neckEffector;
					_solverInternal._headEffector = _headEffector;
					_solverInternal._spineBones = _spineBones;
					_solverInternal._shoulderBones = _shoulderBones;
					_solverInternal._armBones = _armBones;
					_solverInternal._limbIK = _limbIK;
					_solverInternal._centerLegBoneBasisInv = _centerLegBoneBasisInv;
					PrepareArray(ref _solverInternal.shoulderPos, _shoulderBones);
					PrepareArray(ref _solverInternal.spinePos, _spineBones);
					_solverInternal.nearArmPos = _shoulderBones != null ? _solverInternal.shoulderPos : _solverInternal.armPos;
					if (_spineUBone != null) {
						if (_shoulderBones != null || _armBones != null) {
							var nearArmBones = _shoulderBones ?? _armBones;
							var dirY = nearArmBones[1]._defaultPosition + nearArmBones[0]._defaultPosition;
							var dirX = nearArmBones[1]._defaultPosition - nearArmBones[0]._defaultPosition;
							dirY = dirY * 0.5f - _spineUBone._defaultPosition;
							var dirZ = Vector3f.Cross(dirX, dirY);
							dirX = Vector3f.Cross(dirY, dirZ);
							if (IKMath.VecNormalize3(ref dirX, ref dirY, ref dirZ)) {
								var localBasis = IKMatrix3x3.FromColumn(ref dirX, ref dirY, ref dirZ);
								_solverInternal._spineUBoneLocalAxisBasisInv = localBasis.Transpose;
							}
						}
					}
				}

				_solverInternal.headEnabled = _headBone != null && _solverCaches.headPull > IKMath.IK_EPSILON;
				return true;
			}

			void PresolveHips() {
				Assert(_internalValues != null && _internalValues.animatorEnabled);

				var temp = _solverInternal;
				Assert(temp != null);

				if (_hipsEffector == null) {
					return;
				}

				var rotationEnabled = _hipsEffector.rotationEnabled && _hipsEffector.rotationWeight > IKMath.IK_EPSILON;
				var positionEnabled = _hipsEffector.positionEnabled && _hipsEffector.pull > IKMath.IK_EPSILON; // Note: Not positionWeight.

				if (!rotationEnabled && !positionEnabled) {
					return;
				}

				var centerLegBasis = temp.CenterLegBasis;

				if (rotationEnabled) {
					var centerLegRotationTo = _hipsEffector.WorldRotation * IKMath.Inverse(_hipsEffector._defaultRotation);
					IKMath.MatGetRot(out var centerLegRotationFrom, centerLegBasis);

					var centerLegRotation = centerLegRotationTo * IKMath.Inverse(centerLegRotationFrom);

					if (_hipsEffector.rotationWeight < 1.0f - IKMath.IK_EPSILON) {
						centerLegRotation = Quaternionf.Slerp(Quaternionf.Identity, centerLegRotation, _hipsEffector.rotationWeight);
					}

					temp.LowerRotation(-1, ref centerLegRotation, true);
					centerLegBasis = temp.CenterLegBasis;
				}

				if (positionEnabled) {
					// Note: _hidden_worldPosition is contained positionWeight.
					var hipsEffectorWorldPosition = _hipsEffector._hidden_worldPosition;
					IKMath.MatMultVecPreSubAdd(out var centerLegPos, centerLegBasis, _defaultCenterLegPos, _hipsEffector._defaultPosition, hipsEffectorWorldPosition);

					var translate = centerLegPos - temp.CenterLegPos;
					if (_hipsEffector.pull < 1.0f - IKMath.IK_EPSILON) {
						translate *= _hipsEffector.pull;
					}

					temp.Translate(ref translate);
				}
			}

			void ResetTransforms() {
				Assert(_internalValues != null && _internalValues.resetTransforms);
				GetBaseCenterLegTransform(out var centerLegPos, out var centerLegBasis);
				ResetCenterLegTransform(ref centerLegPos, ref centerLegBasis);
			}

			void GetBaseCenterLegTransform(out Vector3f centerLegPos, out IKMatrix3x3 centerLegBasis) {
				// Use from resetTransforms & continuousSolverEnabled.
				Assert(_internalValues != null);

				centerLegBasis = _internalValues.baseHipsBasis;

				if (_hipsEffector != null) {
					IKMath.MatMultVecPreSubAdd(
						out centerLegPos,
						_internalValues.baseHipsBasis,
						_defaultCenterLegPos,
						_hipsEffector._defaultPosition,
						_internalValues.baseHipsPos);
				}
				else { // Failsafe.
					centerLegPos = new Vector3f();
				}
			}

			void ResetCenterLegTransform(ref Vector3f centerLegPos, ref IKMatrix3x3 centerLegBasis) {
				var temp = _solverInternal;
				Assert(temp != null);

				var defaultCenterLegPos = _defaultCenterLegPos;

				if (_legBones != null) {
					for (var i = 0; i != 2; ++i) {
						IKMath.MatMultVecPreSubAdd(out temp.legPos[i], centerLegBasis, _legBones[i]._defaultPosition, defaultCenterLegPos, centerLegPos);
					}
				}
				if (_spineBones != null) {
					for (var i = 0; i != _spineBones.Length; ++i) {
						IKMath.MatMultVecPreSubAdd(out temp.spinePos[i], centerLegBasis, _spineBones[i]._defaultPosition, defaultCenterLegPos, centerLegPos);
					}
				}
				if (_shoulderBones != null) {
					for (var i = 0; i != 2; ++i) {
						IKMath.MatMultVecPreSubAdd(out temp.shoulderPos[i], centerLegBasis, _shoulderBones[i]._defaultPosition, defaultCenterLegPos, centerLegPos);
					}
				}
				if (_armBones != null) {
					for (var i = 0; i != 2; ++i) {
						IKMath.MatMultVecPreSubAdd(out temp.armPos[i], centerLegBasis, _armBones[i]._defaultPosition, defaultCenterLegPos, centerLegPos);
					}
				}
				if (_neckBone != null) {
					IKMath.MatMultVecPreSubAdd(out temp.neckPos, centerLegBasis, _neckBone._defaultPosition, defaultCenterLegPos, centerLegPos);
				}
				if (_headBone != null && temp.headEnabled) {
					IKMath.MatMultVecPreSubAdd(out temp.headPos, centerLegBasis, _headBone._defaultPosition, defaultCenterLegPos, centerLegPos);
				}

				temp.SetDirtyVariables();
				temp.SetCenterLegPos(ref centerLegPos); // Optimized.
			}

			// for ShoulderResolve
			void ResetShoulderTransform() {
				var temp = _solverInternal;
				Assert(temp != null);
				Assert(_limbIK != null);

				if (_armBones == null || _shoulderBones == null) {
					return;
				}

				if (_spineUBone == null || !_spineUBone.TransformIsAlive ||
					_neckBone == null || !_neckBone.TransformIsAlive) {
				}

				if (!_limbIK[(int)LimbIKLocation.LeftArm].IsSolverEnabled() &&
					!_limbIK[(int)LimbIKLocation.RightArm].IsSolverEnabled()) {
					return;
				}

				var dirY = temp.neckPos - temp.SpineUPos;
				var dirX = temp.nearArmPos[1] - temp.nearArmPos[0];

				if (IKMath.VecNormalize(ref dirY) && IKMath.ComputeBasisFromXYLockY(out var boneBasis, dirX, ref dirY)) {
					IKMath.MatMult(out var tempBasis, boneBasis, _spineUBone._localAxisBasisInv);

					var tempPos = temp.SpineUPos;
					for (var i = 0; i != 2; ++i) {
						var limbIKIndex = i == 0 ? (int)LimbIKLocation.LeftArm : (int)LimbIKLocation.RightArm;
						if (_limbIK[limbIKIndex].IsSolverEnabled()) {
							IKMath.MatMultVecPreSubAdd(out temp.armPos[i], tempBasis, _armBones[i]._defaultPosition, _spineUBone._defaultPosition, tempPos);
						}
					}
				}
			}

			//----------------------------------------------------------------------------------------------------------------------------------------

			class SolverInternal
			{
				public class Limb
				{
					public Vector3f[] beginPos;

					public float[] _bendingPull;
					public float[] _endPull;
					public FastLength[] _beginToBendingLength;
					public FastLength[] _beginToEndLength;

					public bool[] targetBeginPosEnabled = new bool[2];
					public Vector3f[] targetBeginPos = new Vector3f[2];

					public bool[] bendingPosEnabled = new bool[2];
					public bool[] endPosEnabled = new bool[2];
					public Vector3f[] bendingPos = new Vector3f[2];
					public Vector3f[] endPos = new Vector3f[2];

					public void Prepare(Effector[] bendingEffectors, Effector[] endEffectors) {
						Assert(bendingEffectors != null);
						Assert(endEffectors != null);

						for (var i = 0; i < 2; ++i) {
							targetBeginPos[i] = beginPos[i];
							targetBeginPosEnabled[i] = false;

							if (bendingEffectors[i] != null && bendingEffectors[i].Bone != null && bendingEffectors[i].Bone.TransformIsAlive) {
								bendingPosEnabled[i] = true;
								bendingPos[i] = bendingEffectors[i]._hidden_worldPosition;
							}
							else {
								bendingPosEnabled[i] = false;
								bendingPos[i] = new Vector3f();
							}

							if (endEffectors[i] != null && endEffectors[i].Bone != null && endEffectors[i].Bone.TransformIsAlive) {
								endPosEnabled[i] = true;
								endPos[i] = endEffectors[i]._hidden_worldPosition;
							}
							else {
								endPosEnabled[i] = false;
								endPos[i] = new Vector3f();
							}
						}
					}

					public bool SolveTargetBeginPos(int i) {
						return SolveTargetBeginPos(i, ref beginPos[i]);
					}

					public bool SolveTargetBeginPos(int i, ref Vector3f beginPos) {
						targetBeginPos[i] = beginPos;
						targetBeginPosEnabled[i] = false;

						if (endPosEnabled[i] && _endPull[i] > IKMath.IK_EPSILON) {
							targetBeginPosEnabled[i] |= SolveTargetBeginPos(ref targetBeginPos[i], ref endPos[i], ref _beginToEndLength[i], _endPull[i]);
						}
						if (bendingPosEnabled[i] && _bendingPull[i] > IKMath.IK_EPSILON) {
							targetBeginPosEnabled[i] |= SolveTargetBeginPos(ref targetBeginPos[i], ref bendingPos[i], ref _beginToBendingLength[i], _bendingPull[i]);
						}

						return targetBeginPosEnabled[i];
					}

					static bool SolveTargetBeginPos(ref Vector3f targetBeginPos, ref Vector3f targetEndPos, ref FastLength targetBeginToEndLength, float endPull) {
						var beginToEnd = targetEndPos - targetBeginPos;
						var beginToEndLengthSq = beginToEnd.SqrMagnitude;
						if (beginToEndLengthSq > targetBeginToEndLength.lengthSq + IKMath.FLOAT_EPSILON) {
							var beginToEndLength = IKMath.Sqrt(beginToEndLengthSq);
							if (beginToEndLength > IKMath.IK_EPSILON) {
								var tempLength = beginToEndLength - targetBeginToEndLength.length;
								tempLength /= beginToEndLength;
								if (tempLength > IKMath.IK_EPSILON) {
									if (endPull < 1.0f - IKMath.IK_EPSILON) {
										tempLength *= endPull;
									}
									targetBeginPos += beginToEnd * tempLength;
									return true;
								}
							}
						}

						return false;
					}
				}

				public Settings settings;
				public InternalValues internalValues;
				public bool[] _shouderLocalAxisYInv;
				public Effector[] _armEffectors;
				public Effector _neckEffector;
				public Effector _headEffector;

				public Bone[] _spineBones;
				public Bone[] _shoulderBones;
				public Bone[] _armBones;

				public Limb arms = new();
				public Limb legs = new();

				public Vector3f[] origToBeginDir = new Vector3f[2];
				public Vector3f[] origToTargetBeginDir = new Vector3f[2];
				public float[] origTheta = new float[2];
				public Vector3f[] origAxis = new Vector3f[2];
				public Vector3f[] origTranslate = new Vector3f[2];
				public float[] origFeedbackRate = new float[2];

				public Vector3f[] spinePos;

				public Vector3f neckPos;
				public Vector3f headPos;
				public bool headEnabled; // _headEffector != null && _headEffector.positionEnabled && _headEffector.pull > IKEpsilon

				public Vector3f[] nearArmPos;
				public Vector3f[] shoulderPos;
				public Vector3f[] armPos = new Vector3f[2]; // = arms.beginPos
				public Vector3f[] legPos = new Vector3f[2]; // = legs.beginPos

				public IKMatrix3x3 _centerLegBoneBasisInv = IKMatrix3x3.identity; // Require setting on initialize.
				public IKMatrix3x3 _spineUBoneLocalAxisBasisInv = IKMatrix3x3.identity; // Require setting on initialize.

				public Vector3f _centerArmPos = Vector3f.Zero;
				public Vector3f _centerLegPos = Vector3f.Zero;
				public IKMatrix3x3 _centerLegBasis = IKMatrix3x3.identity;
				public IKMatrix3x3 _spineUBasis = IKMatrix3x3.identity;

				bool _isDirtyCenterArmPos = true;
				bool _isDirtyCenterLegPos = true;
				bool _isDirtyCenterLegBasis = true;
				bool _isDirtySpineUBasis = true;

				public SolverInternal() {
					arms.beginPos = armPos;
					legs.beginPos = legPos;
				}

				public Vector3f CenterArmPos
				{
					get {
						if (_isDirtyCenterArmPos) {
							UpdateCenterArmPos();
						}

						return _centerArmPos;
					}
				}

				public Vector3f CenterLegPos
				{
					get {
						if (_isDirtyCenterLegPos) {
							UpdateCenterLegPos();
						}

						return _centerLegPos;
					}
				}

				public void UpdateCenterArmPos() {
					if (_isDirtyCenterArmPos) {
						_isDirtyCenterArmPos = false;
						var nearArmPos = shoulderPos;
						nearArmPos ??= armPos;
						if (nearArmPos != null) {
							_centerArmPos = (nearArmPos[0] + nearArmPos[1]) * 0.5f;
						}
					}
				}

				public void UpdateCenterLegPos() {
					if (_isDirtyCenterLegPos) {
						_isDirtyCenterLegPos = false;
						var legPos = this.legPos;
						if (legPos != null) {
							_centerLegPos = (legPos[0] + legPos[1]) * 0.5f;
						}
					}
				}

				public void SetCenterLegPos(ref Vector3f centerLegPos) {
					_isDirtyCenterLegPos = false;
					_centerLegPos = centerLegPos;
				}

				public IKMatrix3x3 CenterLegBasis
				{
					get {
						if (_isDirtyCenterLegBasis) {
							UpdateCenterLegBasis();
						}

						return _centerLegBasis;
					}
				}

				public IKMatrix3x3 SpineUBasis
				{
					get {
						if (_isDirtySpineUBasis) {
							UpdateSpineUBasis();
						}

						return _spineUBasis;
					}
				}

				public void UpdateCenterLegBasis() {
					if (_isDirtyCenterLegBasis) {
						_isDirtyCenterLegBasis = false;
						var legPos = this.legPos;
						_centerLegBasis = IKMatrix3x3.identity;
						if (spinePos != null && spinePos.Length > 0 && legPos != null) {
							var dirX = legPos[1] - legPos[0];
							var dirY = spinePos[0] - CenterLegPos;
							var dirZ = Vector3f.Cross(dirX, dirY);
							dirX = Vector3f.Cross(dirY, dirZ);
							if (IKMath.VecNormalize3(ref dirX, ref dirY, ref dirZ)) {
								_centerLegBasis.SetColumn(ref dirX, ref dirY, ref dirZ);
								IKMath.MatMultRet0(ref _centerLegBasis, _centerLegBoneBasisInv);
							}
						}
					}
				}

				public void UpdateSpineUBasis() {
					if (_isDirtySpineUBasis) {
						_isDirtySpineUBasis = false;
						_spineUBasis = IKMatrix3x3.identity;
						var dirY = shoulderPos != null ? shoulderPos[1] + shoulderPos[0] : armPos[1] + armPos[0];
						dirY = dirY * 0.5f - SpineUPos;
						var dirX = shoulderPos != null ? shoulderPos[1] - shoulderPos[0] : armPos[1] - armPos[0];
						var dirZ = Vector3f.Cross(dirX, dirY);
						dirX = Vector3f.Cross(dirY, dirZ);
						if (IKMath.VecNormalize3(ref dirX, ref dirY, ref dirZ)) {
							_spineUBasis.SetColumn(ref dirX, ref dirY, ref dirZ);
							IKMath.MatMultRet0(ref _spineUBasis, _spineUBoneLocalAxisBasisInv);
						}
					}
				}

				public void SetDirtyVariables() {
					_isDirtyCenterArmPos = true;
					_isDirtyCenterLegPos = true;
					_isDirtyCenterLegBasis = true;
					_isDirtySpineUBasis = true;
				}

				public Vector3f SpineUPos => spinePos != null && spinePos.Length != 0 ? spinePos[spinePos.Length - 1] : Vector3f.Zero;

				public class BackupData
				{
					public Vector3f centerArmPos;
					public Vector3f centerLegPos;
					public IKMatrix3x3 centerLegBasis;
					public IKMatrix3x3 spineUBasis;

					public Vector3f[] spinePos;
					public Vector3f neckPos;
					public Vector3f headPos;
					public Vector3f[] shoulderPos;

					public Vector3f[] armPos = new Vector3f[2];
					public Vector3f[] legPos = new Vector3f[2];
				}

				readonly BackupData _backupData = new();

				public void Backup() {
					_backupData.centerArmPos = CenterArmPos;
					_backupData.centerLegPos = CenterLegPos;
					_backupData.centerLegBasis = CenterLegBasis;
					_backupData.spineUBasis = SpineUBasis;
					CloneArray(ref _backupData.spinePos, spinePos);
					_backupData.neckPos = neckPos;
					_backupData.headPos = headPos;
					CloneArray(ref _backupData.shoulderPos, shoulderPos);
					CloneArray(ref _backupData.armPos, arms.beginPos);
					CloneArray(ref _backupData.legPos, legs.beginPos);
				}

				public void Restore() {
					_isDirtyCenterArmPos = false;
					_isDirtyCenterLegPos = false;
					_isDirtyCenterLegBasis = false;
					_isDirtySpineUBasis = false;
					_centerArmPos = _backupData.centerArmPos;
					_centerLegPos = _backupData.centerLegPos;
					_centerLegBasis = _backupData.centerLegBasis;
					_spineUBasis = _backupData.spineUBasis;
					CloneArray(ref spinePos, _backupData.spinePos);
					neckPos = _backupData.neckPos;
					headPos = _backupData.headPos;
					CloneArray(ref shoulderPos, _backupData.shoulderPos);
					CloneArray(ref arms.beginPos, _backupData.armPos);
					CloneArray(ref legs.beginPos, _backupData.legPos);
				}

				struct UpperSolverPreArmsTemp
				{
					public Vector3f[] shoulderPos;
					public Vector3f[] armPos;
					public Vector3f[] nearArmPos; // shoulderPos / armPos
					public Vector3f neckPos;
					public bool shoulderEnabled;

					public static UpperSolverPreArmsTemp Alloc() {
						return new UpperSolverPreArmsTemp {
							shoulderPos = new Vector3f[2],
							armPos = new Vector3f[2],
							nearArmPos = null, // shoulderPos / armPos
							shoulderEnabled = false
						};
					}
				}

				struct UpperSolverArmsTemp
				{
					public Vector3f[] shoulderPos;
					public Vector3f[] armPos;
					public Vector3f[] nearArmPos; // shoulderPos / armPos
					public bool shoulderEnabled;

					public Vector3f centerArmPos;
					public Vector3f centerArmDir;

					public static UpperSolverArmsTemp Alloc() {
						return new UpperSolverArmsTemp {
							shoulderPos = new Vector3f[2],
							armPos = new Vector3f[2],
							nearArmPos = null, // shoulderPos / armPos
							shoulderEnabled = false,
							centerArmPos = Vector3f.Zero,
							centerArmDir = Vector3f.Zero
						};
					}
				}

				struct UpperSolverTemp
				{
					public Vector3f[] targetArmPos;
					public Vector3f targetNeckPos;
					public Vector3f targetHeadPos;

					public float[] wristToArmRate; // wristPull or balanced to armEffector.pull / wristEffector.pull
					public float[] neckToWristRate; // neckPull or balanced to neckPull / neckEffector.pull

					public static UpperSolverTemp Alloc() {
						return new UpperSolverTemp {
							targetArmPos = new Vector3f[2],
							targetNeckPos = new Vector3f(),
							targetHeadPos = new Vector3f(),
							wristToArmRate = new float[2],
							neckToWristRate = new float[2]
						};
					}
				}

				public Effector[] _wristEffectors;
				public SolverCaches _solverCaches;
				UpperSolverPreArmsTemp _upperSolverPreArmsTemp = UpperSolverPreArmsTemp.Alloc();
				readonly UpperSolverArmsTemp[] _upperSolverArmsTemps = new UpperSolverArmsTemp[2] { UpperSolverArmsTemp.Alloc(), UpperSolverArmsTemp.Alloc() };
				UpperSolverTemp _upperSolverTemp = UpperSolverTemp.Alloc();

				void SolveArmsToArms(ref UpperSolverArmsTemp armsTemp, float armPull, int idx0) {
					var targetArmPos = _upperSolverTemp.targetArmPos[idx0];
					armsTemp.armPos[idx0] = Vector3f.Lerp(armsTemp.armPos[idx0], targetArmPos, armPull);
				}

				void SolveArmsToNeck(ref UpperSolverArmsTemp armsTemp, float neckToFullArmPull, int idx0) {
					var nearArmPos0 = armsTemp.nearArmPos[idx0];
					KeepLength(ref nearArmPos0, ref _upperSolverTemp.targetNeckPos, _solverCaches.nearArmToNeckLength[idx0]);
					armsTemp.nearArmPos[idx0] = Vector3f.Lerp(nearArmPos0, armsTemp.nearArmPos[idx0], neckToFullArmPull);
				}

				void SolveArms(ref UpperSolverArmsTemp armsTemp, int idx0) {
					var idx1 = 1 - idx0;

					var neckHeadPull = _solverCaches.neckHeadPull;
					var armPull = _solverCaches.armPull;
					var elbowPull = _solverCaches.elbowPull;
					var wristPull = _solverCaches.wristPull;
					var neckHeadToFullArmPull = _solverCaches.neckHeadToFullArmPull;

					if (wristPull[idx0] > IKMath.IK_EPSILON || elbowPull[idx0] > IKMath.IK_EPSILON || armPull[idx0] > IKMath.IK_EPSILON || neckHeadPull > IKMath.IK_EPSILON) {
						if (armPull[idx0] > IKMath.IK_EPSILON) {
							SolveArmsToArms(ref armsTemp, armPull[idx0], idx0);
						}
						if ((wristPull[idx0] > IKMath.IK_EPSILON || elbowPull[idx0] > IKMath.IK_EPSILON) &&
							arms.SolveTargetBeginPos(idx0, ref armsTemp.armPos[idx0])) {
							armsTemp.armPos[idx0] = arms.targetBeginPos[idx0]; // Update armPos
							if (armsTemp.shoulderEnabled) {
								KeepLength(ref armsTemp.shoulderPos[idx0], ref armsTemp.armPos[idx0], _solverCaches.shoulderToArmLength[idx0]);
								if (neckHeadPull > IKMath.IK_EPSILON) {
									SolveArmsToNeck(ref armsTemp, neckHeadToFullArmPull[idx0], idx0); // Contain wristPull/neckPull.
									KeepLength(ref armsTemp.armPos[idx0], ref armsTemp.shoulderPos[idx0], _solverCaches.shoulderToArmLength[idx0]);
								}
								KeepLength(ref armsTemp.shoulderPos[idx1], ref armsTemp.shoulderPos[idx0], _solverCaches.nearArmToNearArmLen);
								KeepLength(ref armsTemp.armPos[idx1], ref armsTemp.shoulderPos[idx1], _solverCaches.shoulderToArmLength[idx1]);
							}
							else {
								if (neckHeadPull > IKMath.IK_EPSILON) {
									SolveArmsToNeck(ref armsTemp, neckHeadToFullArmPull[idx0], idx0);
								}
								KeepLength(ref armsTemp.armPos[idx1], ref armsTemp.armPos[idx0], _solverCaches.armToArmLen);
							}
						}
						else if (armPull[idx0] > IKMath.IK_EPSILON || neckHeadPull > IKMath.IK_EPSILON) {
							if (armPull[idx0] > IKMath.IK_EPSILON) {
								if (armsTemp.shoulderEnabled) {
									KeepLength(ref armsTemp.shoulderPos[idx0], ref armsTemp.armPos[idx0], _solverCaches.shoulderToArmLength[idx0]);
								}
							}
							if (neckHeadPull > IKMath.IK_EPSILON) {
								SolveArmsToNeck(ref armsTemp, neckHeadToFullArmPull[idx0], idx0); // Contain wristPull/neckPull.
								if (armsTemp.shoulderEnabled) {
									KeepLength(ref armsTemp.armPos[idx0], ref armsTemp.shoulderPos[idx0], _solverCaches.shoulderToArmLength[idx0]);
								}
							}
							if (armsTemp.shoulderEnabled) {
								KeepLength(ref armsTemp.shoulderPos[idx1], ref armsTemp.shoulderPos[idx0], _solverCaches.nearArmToNearArmLen);
								KeepLength(ref armsTemp.armPos[idx1], ref armsTemp.shoulderPos[idx1], _solverCaches.shoulderToArmLength[idx1]);
							}
							else {
								KeepLength(ref armsTemp.armPos[idx1], ref armsTemp.armPos[idx0], _solverCaches.armToArmLen);
							}
						}
					}
				}

				public bool targetCenterArmEnabled = false;
				public Vector3f targetCenterArmPos = Vector3f.Zero;
				public Vector3f targetCenterArmDir = Vector3f.Zero;

				public Vector3f CurrentCenterArmPos
				{
					get {
						if (shoulderPos != null) {
							return (shoulderPos[0] + shoulderPos[1]) * 0.5f;
						}
						else if (armPos != null) {
							return (armPos[0] + armPos[1]) * 0.5f;
						}
						return Vector3f.Zero;
					}
				}

				public Vector3f CurrentCenterArmDir
				{
					get {
						if (shoulderPos != null) {
							var dir = shoulderPos[1] - shoulderPos[0];
							if (IKMath.VecNormalize(ref dir)) {
								return dir;
							}
						}
						else if (armPos != null) {
							var dir = armPos[1] - armPos[0];
							if (IKMath.VecNormalize(ref dir)) {
								return dir;
							}
						}
						return Vector3f.Zero;
					}
				}

				public bool UpperSolve() {
					targetCenterArmEnabled = false;

					var neckPull = _solverCaches.neckPull;
					var headPull = _solverCaches.headPull;
					var armPull = _solverCaches.armPull;
					var elbowPull = _solverCaches.elbowPull;
					var wristPull = _solverCaches.wristPull;

					if (wristPull[0] <= IKMath.IK_EPSILON && wristPull[1] <= IKMath.IK_EPSILON &&
						elbowPull[0] <= IKMath.IK_EPSILON && elbowPull[1] <= IKMath.IK_EPSILON &&
						armPull[0] <= IKMath.IK_EPSILON && armPull[1] <= IKMath.IK_EPSILON &&
						neckPull <= IKMath.IK_EPSILON && headPull <= IKMath.IK_EPSILON) {
						targetCenterArmPos = CurrentCenterArmPos;
						targetCenterArmDir = CurrentCenterArmDir;
						return false;
					}

					// Prepare _upperSolverTemp
					_upperSolverTemp.targetNeckPos = _neckEffector != null ? _neckEffector._hidden_worldPosition : neckPos;
					_upperSolverTemp.targetHeadPos = _headEffector != null ? _headEffector._hidden_worldPosition : headPos;
					_upperSolverTemp.targetArmPos[0] = _armEffectors != null ? _armEffectors[0]._hidden_worldPosition : armPos[0];
					_upperSolverTemp.targetArmPos[1] = _armEffectors != null ? _armEffectors[1]._hidden_worldPosition : armPos[1];

					// Prepare _upperSolverPreArmsTemp
					_upperSolverPreArmsTemp.neckPos = neckPos;
					_upperSolverPreArmsTemp.armPos[0] = armPos[0];
					_upperSolverPreArmsTemp.armPos[1] = armPos[1];
					_upperSolverPreArmsTemp.shoulderEnabled = shoulderPos != null;
					if (_upperSolverPreArmsTemp.shoulderEnabled) {
						_upperSolverPreArmsTemp.shoulderPos[0] = shoulderPos[0];
						_upperSolverPreArmsTemp.shoulderPos[1] = shoulderPos[1];
						_upperSolverPreArmsTemp.nearArmPos = _upperSolverPreArmsTemp.shoulderPos;
					}
					else {
						_upperSolverPreArmsTemp.nearArmPos = _upperSolverPreArmsTemp.armPos;
					}

					// Moving fix.
					var bodyMovingfixRate = settings.bodyIK.upperBodyMovingfixRate;
					var headMovingfixRate = settings.bodyIK.upperHeadMovingfixRate;
					if (bodyMovingfixRate > IKMath.IK_EPSILON || headMovingfixRate > IKMath.IK_EPSILON) {
						var headMove = Vector3f.Zero;
						var bodyMove = Vector3f.Zero;
						if (headPull > IKMath.IK_EPSILON) {
							headMove = _upperSolverTemp.targetHeadPos - headPos;
							if (headMovingfixRate < 1.0f - IKMath.IK_EPSILON) {
								headMove *= headPull * headMovingfixRate;
							}
							else {
								headMove *= headPull;
							}
						}

						var bodyPull = 0.0f;
						float bodyPullInv;
						if (neckPull > IKMath.IK_EPSILON || armPull[0] > IKMath.IK_EPSILON || armPull[1] > IKMath.IK_EPSILON) {
							bodyPull = neckPull + armPull[0] + armPull[1];
							bodyPullInv = 1.0f / bodyPull;
							if (neckPull > IKMath.IK_EPSILON) {
								bodyMove = (_upperSolverTemp.targetNeckPos - neckPos) * (neckPull * neckPull);
							}
							if (armPull[0] > IKMath.IK_EPSILON) {
								bodyMove += (_upperSolverTemp.targetArmPos[0] - armPos[0]) * (armPull[0] * armPull[0]);
							}
							if (armPull[1] > IKMath.IK_EPSILON) {
								bodyMove += (_upperSolverTemp.targetArmPos[1] - armPos[1]) * (armPull[1] * armPull[1]);
							}
							if (bodyMovingfixRate < 1.0f - IKMath.IK_EPSILON) {
								bodyMove *= bodyPullInv * bodyMovingfixRate;
							}
							else {
								bodyMove *= bodyPullInv;
							}
						}

						Vector3f totalMove;
						if (headPull > IKMath.IK_EPSILON && bodyPull > IKMath.IK_EPSILON) {
							totalMove = headMove * headPull + bodyMove * bodyPull;
							totalMove *= 1.0f / (headPull + bodyPull);
						}
						else {
							totalMove = headPull > IKMath.IK_EPSILON ? headMove : bodyMove;
						}

						_upperSolverPreArmsTemp.neckPos += totalMove;
						_upperSolverPreArmsTemp.armPos[0] += totalMove;
						_upperSolverPreArmsTemp.armPos[1] += totalMove;
						if (_upperSolverPreArmsTemp.shoulderEnabled) {
							_upperSolverPreArmsTemp.shoulderPos[0] += totalMove;
							_upperSolverPreArmsTemp.shoulderPos[1] += totalMove;
						}
					}

					// Preprocess neckSolver.
					if (headMovingfixRate < 1.0f - IKMath.IK_EPSILON || bodyMovingfixRate < 1.0f - IKMath.IK_EPSILON) {
						if (headPull > IKMath.IK_EPSILON || neckPull > IKMath.IK_EPSILON) {
							if (headMovingfixRate < 1.0f - IKMath.IK_EPSILON && headPull > IKMath.IK_EPSILON) {
								var tempNeckPos = _upperSolverPreArmsTemp.neckPos;
								if (KeepMaxLength(ref tempNeckPos, ref _upperSolverTemp.targetHeadPos, _solverCaches.neckToHeadLength)) { // Not KeepLength
									_upperSolverPreArmsTemp.neckPos = Vector3f.Lerp(_upperSolverPreArmsTemp.neckPos, tempNeckPos, headPull);
								}
							}
							for (var i = 0; i != 2; ++i) {
								if (bodyMovingfixRate < 1.0f - IKMath.IK_EPSILON && neckPull > IKMath.IK_EPSILON) {
									var tempNearArmPos = _upperSolverPreArmsTemp.nearArmPos[i];
									KeepLength(ref tempNearArmPos, ref _upperSolverTemp.targetNeckPos, _solverCaches.nearArmToNeckLength[i]);
									_upperSolverPreArmsTemp.nearArmPos[i] = Vector3f.Lerp(_upperSolverPreArmsTemp.nearArmPos[i], tempNearArmPos, neckPull); // Not use neckToFullArmPull in Presolve.
								}
								else {
									KeepLength(
										ref _upperSolverPreArmsTemp.nearArmPos[i],
										ref _upperSolverPreArmsTemp.neckPos,
										_solverCaches.nearArmToNeckLength[i]);
								}
								if (_upperSolverPreArmsTemp.shoulderEnabled) {
									KeepLength(
										ref _upperSolverPreArmsTemp.armPos[i],
										ref _upperSolverPreArmsTemp.shoulderPos[i],
										_solverCaches.shoulderToArmLength[i]);
								}
								//internalValues.AddDebugPoint( nearArmPos, Color.black, 0.1f );
							}
						}
					}

					// Update targetNeckPos using presolved. (Contain neckPull / headPull)
					_upperSolverTemp.targetNeckPos = _upperSolverPreArmsTemp.neckPos;

					// Prepare _upperSolverArmsTemps
					for (var i = 0; i != 2; ++i) {
						_upperSolverArmsTemps[i].armPos[0] = _upperSolverPreArmsTemp.armPos[0];
						_upperSolverArmsTemps[i].armPos[1] = _upperSolverPreArmsTemp.armPos[1];
						_upperSolverArmsTemps[i].shoulderEnabled = _upperSolverPreArmsTemp.shoulderEnabled;
						if (_upperSolverArmsTemps[i].shoulderEnabled) {
							_upperSolverArmsTemps[i].shoulderPos[0] = _upperSolverPreArmsTemp.shoulderPos[0];
							_upperSolverArmsTemps[i].shoulderPos[1] = _upperSolverPreArmsTemp.shoulderPos[1];
							_upperSolverArmsTemps[i].nearArmPos = _upperSolverArmsTemps[i].shoulderPos;
						}
						else {
							_upperSolverArmsTemps[i].nearArmPos = _upperSolverArmsTemps[i].armPos;
						}
					}

					// Check enabled by side.
					var enabled0 = wristPull[0] > IKMath.IK_EPSILON || elbowPull[0] > IKMath.IK_EPSILON || armPull[0] > IKMath.IK_EPSILON;
					var enabled1 = wristPull[1] > IKMath.IK_EPSILON || elbowPull[1] > IKMath.IK_EPSILON || armPull[1] > IKMath.IK_EPSILON;

					var neckHeadPull = _solverCaches.neckHeadPull;
					if (enabled0 && enabled1 || neckHeadPull > IKMath.IK_EPSILON) {
						for (var i = 0; i != 2; ++i) {
							var idx0 = i;
							var idx1 = 1 - i;

							SolveArms(ref _upperSolverArmsTemps[idx0], idx0);
							SolveArms(ref _upperSolverArmsTemps[idx0], idx1);
							SolveArms(ref _upperSolverArmsTemps[idx0], idx0);

							if (_upperSolverArmsTemps[idx0].shoulderEnabled) {
								_upperSolverArmsTemps[idx0].centerArmPos = (_upperSolverArmsTemps[idx0].shoulderPos[0] + _upperSolverArmsTemps[idx0].shoulderPos[1]) * 0.5f;
								_upperSolverArmsTemps[idx0].centerArmDir = _upperSolverArmsTemps[idx0].shoulderPos[1] - _upperSolverArmsTemps[idx0].shoulderPos[0];
							}
							else {
								_upperSolverArmsTemps[idx0].centerArmPos = (_upperSolverArmsTemps[idx0].armPos[0] + _upperSolverArmsTemps[idx0].armPos[1]) * 0.5f;
								_upperSolverArmsTemps[idx0].centerArmDir = _upperSolverArmsTemps[idx0].armPos[1] - _upperSolverArmsTemps[idx0].armPos[0];
							}
						}

						if (!IKMath.VecNormalize2(ref _upperSolverArmsTemps[0].centerArmDir, ref _upperSolverArmsTemps[1].centerArmDir)) {
							return false;
						}

						var limbArmRate = _solverCaches.limbArmRate;

						targetCenterArmEnabled = true;
						targetCenterArmPos = Vector3f.Lerp(_upperSolverArmsTemps[0].centerArmPos, _upperSolverArmsTemps[1].centerArmPos, limbArmRate);
						targetCenterArmDir = IKMath.LerpDir(ref _upperSolverArmsTemps[0].centerArmDir, ref _upperSolverArmsTemps[1].centerArmDir, limbArmRate);
					}
					else {
						var idx0 = enabled0 ? 0 : 1;
						SolveArms(ref _upperSolverArmsTemps[idx0], idx0);

						if (_upperSolverArmsTemps[idx0].shoulderEnabled) {
							_upperSolverArmsTemps[idx0].centerArmPos = (_upperSolverArmsTemps[idx0].shoulderPos[0] + _upperSolverArmsTemps[idx0].shoulderPos[1]) * 0.5f;
							_upperSolverArmsTemps[idx0].centerArmDir = _upperSolverArmsTemps[idx0].shoulderPos[1] - _upperSolverArmsTemps[idx0].shoulderPos[0];
							//internalValues.AddDebugPoint( _upperSolverArmsTemps[idx0].shoulderPos[0], Color.black, 0.1f );
							//internalValues.AddDebugPoint( _upperSolverArmsTemps[idx0].shoulderPos[1], Color.black, 0.1f );
						}
						else {
							_upperSolverArmsTemps[idx0].centerArmPos = (_upperSolverArmsTemps[idx0].armPos[0] + _upperSolverArmsTemps[idx0].armPos[1]) * 0.5f;
							_upperSolverArmsTemps[idx0].centerArmDir = _upperSolverArmsTemps[idx0].armPos[1] - _upperSolverArmsTemps[idx0].armPos[0];
							//internalValues.AddDebugPoint( _upperSolverArmsTemps[idx0].armPos[0], Color.black, 0.1f );
							//internalValues.AddDebugPoint( _upperSolverArmsTemps[idx0].armPos[1], Color.black, 0.1f );
						}

						if (!IKMath.VecNormalize(ref _upperSolverArmsTemps[idx0].centerArmDir)) {
							return false;
						}

						targetCenterArmEnabled = true;
						targetCenterArmPos = _upperSolverArmsTemps[idx0].centerArmPos;
						targetCenterArmDir = _upperSolverArmsTemps[idx0].centerArmDir;
					}

					return true;
				}

				readonly Vector3f[] _tempArmPos = new Vector3f[2];
				readonly Vector3f[] _tempArmToElbowDir = new Vector3f[2];
				readonly Vector3f[] _tempElbowToWristDir = new Vector3f[2];
				readonly bool[] _tempElbowPosEnabled = new bool[2];
				public LimbIK[] _limbIK;

				readonly IKMatrix3x3[] _tempParentBasis = new IKMatrix3x3[2] { IKMatrix3x3.identity, IKMatrix3x3.identity };
				readonly Vector3f[] _tempArmToElbowDefaultDir = new Vector3f[2];

				public bool ShoulderResolve() {
					var armBones = _solverCaches.armBones;
					var shoulderBones = _solverCaches.shoulderBones;
					var shoulderToArmLength = _solverCaches.shoulderToArmLength;
					if (armBones == null || shoulderBones == null) {
						return false;
					}

					Assert(shoulderToArmLength != null);
					Assert(_limbIK != null);

					if (!_limbIK[(int)LimbIKLocation.LeftArm].IsSolverEnabled() &&
						!_limbIK[(int)LimbIKLocation.RightArm].IsSolverEnabled()) {
						return false; // Not required.
					}

					for (var i = 0; i != 2; ++i) {
						var limbIKIndex = i == 0 ? (int)LimbIKLocation.LeftArm : (int)LimbIKLocation.RightArm;

						if (_limbIK[limbIKIndex].IsSolverEnabled()) {
							Vector3f xDir, yDir, zDir;
							xDir = armPos[i] - shoulderPos[i];
							yDir = internalValues.shoulderDirYAsNeck != 0 ? neckPos - shoulderPos[i] : shoulderPos[i] - SpineUPos;
							xDir = i == 0 ? -xDir : xDir;
							zDir = Vector3f.Cross(xDir, yDir);
							yDir = Vector3f.Cross(zDir, xDir);
							if (IKMath.VecNormalize3(ref xDir, ref yDir, ref zDir)) {
								var boneBasis = IKMatrix3x3.FromColumn(ref xDir, ref yDir, ref zDir);
								IKMath.MatMult(out _tempParentBasis[i], boneBasis, _shoulderBones[i]._boneToBaseBasis);
							}

							_tempArmPos[i] = armPos[i];
							_tempElbowPosEnabled[i] = _limbIK[limbIKIndex].Presolve(
								ref _tempParentBasis[i],
								ref _tempArmPos[i],
								out _tempArmToElbowDir[i],
								out _tempElbowToWristDir[i]);

							if (_tempElbowPosEnabled[i]) {
								IKMath.MatMultCol0(out _tempArmToElbowDefaultDir[i], _tempParentBasis[i], _armBones[i]._baseToBoneBasis);
								if (i == 0) {
									_tempArmToElbowDefaultDir[i] = -_tempArmToElbowDefaultDir[i];
								}
							}
						}
					}

					if (!_tempElbowPosEnabled[0] && !_tempElbowPosEnabled[1]) {
						return false; // Not required.
					}

					var feedbackRate = settings.bodyIK.shoulderSolveBendingRate;

					var updateAnything = false;
					for (var i = 0; i != 2; ++i) {
						if (_tempElbowPosEnabled[i]) {
							IKMath.ComputeThetaAxis(ref _tempArmToElbowDefaultDir[i], ref _tempArmToElbowDir[i], out var theta, out var axis);
							if (theta is >= (-IKMath.FLOAT_EPSILON) and <= IKMath.FLOAT_EPSILON) {
								// Nothing.
							}
							else {
								updateAnything = true;
								theta = IKMath.Cos(IKMath.Acos(theta) * feedbackRate);
								IKMath.MatSetAxisAngle(out var m, ref axis, theta);

								var tempShoulderPos = shoulderPos[i];
								var tempDir = _tempArmPos[i] - tempShoulderPos;
								IKMath.VecNormalize(ref tempDir);
								IKMath.MatMultVec(out var resultDir, m, tempDir);
								var destArmPos = tempShoulderPos + resultDir * shoulderToArmLength[i];

								SolveShoulderToArmInternal(i, ref destArmPos);
							}
						}
					}

					return updateAnything;
				}

				public bool PrepareLowerRotation(int origIndex) {
					var r = false;
					for (var i = 0; i < 2; ++i) {
						legs.SolveTargetBeginPos(i);
						r |= PrepareLimbRotation(legs, i, origIndex, ref legs.beginPos[i]);
					}
					return r;
				}

				public bool PrepareLimbRotation(Limb limb, int i, int origIndex, ref Vector3f beginPos) {
					Assert(i < 2);
					origTheta[i] = 0.0f;
					origAxis[i] = new Vector3f(0.0f, 0.0f, 1.0f);

					if (!limb.targetBeginPosEnabled[i]) {
						return false;
					}

					// Memo: limb index = orig index.

					var targetBeginPos = limb.targetBeginPos;

					var origPos = origIndex == -1 ? CenterLegPos : spinePos[origIndex];

					return IKMath.ComputeThetaAxis(
						ref origPos,
						ref beginPos,
						ref targetBeginPos[i],
						out origTheta[i],
						out origAxis[i]);
				}

				public void SetSolveFeedbackRate(int i, float feedbackRate) {
					origFeedbackRate[i] = feedbackRate;
				}

				public bool SolveLowerRotation(int origIndex, out Quaternionf origRotation) {
					return SolveLimbRotation(legs, origIndex, out origRotation);
				}

				bool SolveLimbRotation(Limb limb, int _, out Quaternionf origRotation) {
					origRotation = Quaternionf.Identity;

					var pullIndex = -1;
					var pullLength = 0;
					for (var i = 0; i < 2; ++i) {
						if (limb.targetBeginPosEnabled[i]) {
							pullIndex = i;
							++pullLength;
						}
					}

					if (pullLength == 0) {
						return false; // Failsafe.
					}

					var lerpRate = limb == arms ? _solverCaches.limbArmRate : _solverCaches.limbLegRate;

					if (pullLength == 1) {
						var i0 = pullIndex;
						if (origTheta[i0] == 0.0f) {
							return false;
						}

						if (i0 == 0) {
							lerpRate = 1.0f - lerpRate;
						}

						origRotation = GetRotation(ref origAxis[i0], origTheta[i0], origFeedbackRate[i0] * lerpRate);
						return true;
					}

					// Fix for rotate 180 degrees or more.( half rotation in GetRotation & double rotation in origRotation * origRotation. )
					var origRotation0 = GetRotation(ref origAxis[0], origTheta[0], origFeedbackRate[0] * 0.5f);
					var origRotation1 = GetRotation(ref origAxis[1], origTheta[1], origFeedbackRate[1] * 0.5f);
					origRotation = Quaternionf.Slerp(origRotation0, origRotation1, lerpRate);
					origRotation *= origRotation; // Optimized: Not normalize.
					return true;
				}

				public void UpperRotation(int origIndex, ref IKMatrix3x3 origBasis) {
					var origPos = origIndex == -1 ? CenterLegPos : spinePos[origIndex];

					{
						var armPos = this.armPos;
						if (armPos != null) {
							for (var i = 0; i < armPos.Length; ++i) {
								IKMath.MatMultVecPreSubAdd(out armPos[i], origBasis, armPos[i], origPos, origPos);
							}
						}
					}

					if (shoulderPos != null) {
						for (var i = 0; i < shoulderPos.Length; ++i) {
							IKMath.MatMultVecPreSubAdd(out shoulderPos[i], origBasis, shoulderPos[i], origPos, origPos);
						}
					}

					IKMath.MatMultVecPreSubAdd(out neckPos, origBasis, neckPos, origPos, origPos);
					if (headEnabled) {
						IKMath.MatMultVecPreSubAdd(out headPos, origBasis, headPos, origPos, origPos);
					}

					// Legs					
					if (origIndex == -1) { // Rotation origin is centerLeg
						var legPos = this.legPos;
						if (legPos != null) {
							for (var i = 0; i < legPos.Length; ++i) {
								IKMath.MatMultVecPreSubAdd(out legPos[i], origBasis, legPos[i], origPos, origPos);
							}
						}

						_isDirtyCenterLegBasis = true;
					}

					// Spine
					for (var t = origIndex == -1 ? 0 : origIndex; t < spinePos.Length; ++t) {
						IKMath.MatMultVecPreSubAdd(out spinePos[t], origBasis, spinePos[t], origPos, origPos);
					}

					_isDirtyCenterArmPos = true;
					_isDirtySpineUBasis = true;
				}

				public void LowerRotation(int origIndex, ref Quaternionf origRotation, bool bodyRotation) {
					var origBasis = new IKMatrix3x3(origRotation);
					LowerRotation(origIndex, ref origBasis, bodyRotation);
				}

				public void LowerRotation(int origIndex, ref IKMatrix3x3 origBasis, bool bodyRotation) {
					var origPos = origIndex == -1 ? CenterLegPos : spinePos[origIndex];

					var legPos = this.legPos;
					if (legPos != null) {
						for (var i = 0; i < 2; ++i) {
							IKMath.MatMultVecPreSubAdd(out legPos[i], origBasis, legPos[i], origPos, origPos);
						}
					}

					if (spinePos != null) {
						var length = bodyRotation ? spinePos.Length : origIndex;
						for (var n = 0; n < length; ++n) {
							IKMath.MatMultVecPreSubAdd(out spinePos[n], origBasis, spinePos[n], origPos, origPos);
						}
					}

					_isDirtyCenterArmPos = true;
					_isDirtyCenterLegPos = true;
					_isDirtyCenterLegBasis = true;

					if (bodyRotation || spinePos == null || origIndex + 1 == spinePos.Length) {
						IKMath.MatMultVecPreSubAdd(out neckPos, origBasis, neckPos, origPos, origPos);
						if (headEnabled) {
							IKMath.MatMultVecPreSubAdd(out headPos, origBasis, headPos, origPos, origPos);
						}

						var armPos = this.armPos;
						if (armPos != null) {
							for (var i = 0; i < 2; ++i) {
								IKMath.MatMultVecPreSubAdd(out armPos[i], origBasis, armPos[i], origPos, origPos);
							}
						}

						if (shoulderPos != null) {
							for (var i = 0; i < 2; ++i) {
								IKMath.MatMultVecPreSubAdd(out shoulderPos[i], origBasis, shoulderPos[i], origPos, origPos);
							}
						}

						_isDirtySpineUBasis = true;
					}
				}

				public bool PrepareLowerTranslate() {
					var r = false;
					for (var i = 0; i < 2; ++i) {
						legs.SolveTargetBeginPos(i);
						r |= PrepareLimbTranslate(legs, i, ref legs.beginPos[i]);
					}
					return r;
				}

				bool PrepareLimbTranslate(Limb limb, int i, ref Vector3f beginPos) {
					origTranslate[i] = Vector3f.Zero;
					if (limb.targetBeginPosEnabled[i]) {
						origTranslate[i] = limb.targetBeginPos[i] - beginPos;
						return true;
					}

					return false;
				}

				public bool SolveLowerTranslate(out Vector3f translate) {
					return SolveLimbTranslate(legs, out translate);
				}

				private bool SolveLimbTranslate(Limb limb, out Vector3f origTranslate) {
					origTranslate = Vector3f.Zero;

					var lerpRate = limb == arms ? _solverCaches.limbArmRate : _solverCaches.limbLegRate;

					if (limb.targetBeginPosEnabled[0] && limb.targetBeginPosEnabled[1]) {
						origTranslate = Vector3f.Lerp(this.origTranslate[0], this.origTranslate[1], lerpRate);
					}
					else if (limb.targetBeginPosEnabled[0] || limb.targetBeginPosEnabled[1]) {
						var i0 = limb.targetBeginPosEnabled[0] ? 0 : 1;
						var lerpRate1to0 = limb.targetBeginPosEnabled[0] ? 1.0f - lerpRate : lerpRate;
						origTranslate = this.origTranslate[i0] * lerpRate1to0;
					}

					return origTranslate != Vector3f.Zero;
				}

				public void Translate(ref Vector3f origTranslate) {
					_centerArmPos += origTranslate;
					_centerLegPos += origTranslate;

					if (spinePos != null) {
						for (var i = 0; i != spinePos.Length; ++i) {
							spinePos[i] += origTranslate;
						}
					}

					neckPos += origTranslate;
					if (headEnabled) {
						headPos += origTranslate;
					}

					for (var i = 0; i != 2; ++i) {
						if (legPos != null) {
							legPos[i] += origTranslate;
						}

						if (shoulderPos != null) {
							shoulderPos[i] += origTranslate;
						}

						if (armPos != null) {
							armPos[i] += origTranslate;
						}
					}
				}

				public void SolveShoulderToArmInternal(int i, ref Vector3f destArmPos) {
					if (!settings.bodyIK.shoulderSolveEnabled) {
						return;
					}

					var shoulderBones = _solverCaches.shoulderBones;
					var shoulderToArmLength = _solverCaches.shoulderToArmLength;
					var limitYPlus = internalValues.bodyIK.shoulderLimitThetaYPlus.sin;
					var limitYMinus = internalValues.bodyIK.shoulderLimitThetaYMinus.sin;
					var limitZ = internalValues.bodyIK.shoulderLimitThetaZ.sin;

					if (shoulderBones == null) {
						return;
					}

					if (_shouderLocalAxisYInv[i]) {
						(limitYMinus, limitYPlus) = (limitYPlus, limitYMinus);
					}

					if (!IKMath.IsFuzzy(ref armPos[i], ref destArmPos)) {
						var dirX = destArmPos - shoulderPos[i];
						if (IKMath.VecNormalize(ref dirX)) {
							if (settings.bodyIK.shoulderLimitEnabled) {
								var worldBasis = SpineUBasis;
								IKMath.MatMultRet0(ref worldBasis, shoulderBones[i]._localAxisBasis);
								IKMath.MatMultVecInv(out dirX, worldBasis, dirX);
								IKMath.LimitYZ_Square(i != 0, ref dirX, limitYMinus, limitYPlus, limitZ, limitZ);
								IKMath.MatMultVec(out dirX, worldBasis, dirX);
							}

							armPos[i] = shoulderPos[i] + dirX * shoulderToArmLength[i];
						}
					}
				}
			}

			//----------------------------------------------------------------------------------------------------------------------------------------

			static bool ComputeCenterLegBasis(
				out IKMatrix3x3 centerLegBasis,
				ref Vector3f spinePos,
				ref Vector3f leftLegPos,
				ref Vector3f rightLegPos) {
				var dirX = rightLegPos - leftLegPos;
				var dirY = spinePos - (rightLegPos + leftLegPos) * 0.5f;
				if (IKMath.VecNormalize(ref dirY)) {
					return IKMath.ComputeBasisFromXYLockY(out centerLegBasis, dirX, ref dirY);
				}
				else {
					centerLegBasis = IKMatrix3x3.identity;
					return false;
				}
			}

			static bool KeepMaxLength(ref Vector3f posTo, ref Vector3f posFrom, float keepLength) {
				var v = posTo - posFrom;
				var len = IKMath.VecLength(v);
				if (len > IKMath.IK_EPSILON && len > keepLength) {
					v *= keepLength / len;
					posTo = posFrom + v;
					return true;
				}

				return false;
			}

			static bool KeepLength(ref Vector3f posTo, ref Vector3f posFrom, float keepLength) {
				var v = posTo - posFrom;
				var len = IKMath.VecLength(v);
				if (len > IKMath.IK_EPSILON) {
					v *= keepLength / len;
					posTo = posFrom + v;
					return true;
				}

				return false;
			}

			static Quaternionf GetRotation(ref Vector3f axisDir, float theta, float rate) {
				return theta >= -IKMath.IK_EPSILON && theta <= IKMath.IK_EPSILON || rate >= -IKMath.IK_EPSILON && rate <= IKMath.IK_EPSILON
					? Quaternionf.Identity
					: new Quaternionf(axisDir, IKMath.Acos(theta) * rate * MathUtil.RAD_2_DEGF);
			}

			//--------------------------------------------------------------------------------------------------------------------------------

			static float ConcatPull(float pull, float effectorPull) {
				return pull >= 1.0f - IKMath.IK_EPSILON
					? 1.0f
					: pull <= IKMath.IK_EPSILON
					? effectorPull
					: effectorPull > IKMath.IK_EPSILON ? effectorPull >= 1.0f - IKMath.IK_EPSILON ? 1.0f : pull + (1.0f - pull) * effectorPull : pull;
			}

			static float GetBalancedPullLockTo(float pullFrom, float pullTo) {
				if (pullTo <= IKMath.IK_EPSILON) {
					return 1.0f - pullFrom;
				}
				if (pullFrom <= IKMath.IK_EPSILON) {
					return 1.0f; // Lock to.
				}

				return pullTo / (pullFrom + pullTo);
			}

			static float GetBalancedPullLockFrom(float pullFrom, float pullTo) {
				if (pullFrom <= IKMath.IK_EPSILON) {
					return pullTo;
				}
				if (pullTo <= IKMath.IK_EPSILON) {
					return 0.0f; // Lock from.
				}

				return pullTo / (pullFrom + pullTo);
			}

			static float GetLerpRateFromPull2(float pull0, float pull1) {
				return pull0 > IKMath.FLOAT_EPSILON && pull1 > IKMath.FLOAT_EPSILON
					? MathUtil.Clamp(pull1 / (pull0 + pull1), 0, 1)
					: pull0 > IKMath.FLOAT_EPSILON ? 0.0f : pull1 > IKMath.FLOAT_EPSILON ? 1.0f : 0.5f;
			}
		}


		public sealed partial class Effector : SyncObject
		{
			[Flags]
			public enum EffectorFlags : byte
			{
				None = 0x00,
				RotationContained = 0x01, // Hips/Wrist/Foot
				PullContained = 0x02, // Foot/Wrist
			}

			// Memo: If transform is created & cloned this instance, will be cloned effector transform, too.
			public readonly SyncRef<Entity> transform;
			[Default(false)]
			public readonly Sync<bool> positionEnabled;
			[Default(false)]
			public readonly Sync<bool> rotationEnabled;
			[Default(1.0f)]
			public readonly Sync<float> positionWeight;
			[Default(1.0f)]
			public readonly Sync<float> rotationWeight;
			[Default(0.0f)]
			public readonly Sync<float> pull;

			[NonSerialized]
			public Vector3f _hidden_worldPosition = Vector3f.Zero;

			public bool EffectorEnabled => positionEnabled || RotationContained && RotationContained;

			[Default(false)]
			public readonly Sync<bool> _isPresetted;
			[Default(EffectorLocation.Unknown)]
			public readonly Sync<EffectorLocation> _effectorLocation;
			[Default(EffectorType.Unknown)]
			public readonly Sync<EffectorType> _effectorType;
			[Default(EffectorFlags.None)]
			public readonly Sync<EffectorFlags> _effectorFlags;

			// These aren't serialize field.
			// Memo: If this instance is cloned, will be copyed these properties, too.
			Effector _parentEffector = null;
			Bone _bone = null; // Hips : Hips Eyes : Head
			Bone _leftBone = null; // Hips : LeftLeg Eyes : LeftEye Others : null
			Bone _rightBone = null; // Hips : RightLeg Eyes : RightEye Others : null

			// Memo: If transform is created & cloned this instance, will be cloned effector transform, too.
			public readonly SyncRef<Entity> _createdTransform; // Hidden, for destroy check.

			// Memo: defaultPosition / defaultRotation is copied from bone.
			public readonly Sync<Vector3f> _defaultPosition;
			public readonly Sync<Quaternionf> _defaultRotation;

			protected override void FirstCreation() {
				base.FirstCreation();
				_defaultPosition.Value = Vector3f.Zero;
				_defaultRotation.Value = Quaternionf.Identity;
			}

			public bool _isSimulateFingerTips = false; // Bind effector fingerTips2

			// Basiclly flags.
			public bool RotationContained => (_effectorFlags & EffectorFlags.RotationContained) != EffectorFlags.None;
			public bool PullContained => (_effectorFlags & EffectorFlags.PullContained) != EffectorFlags.None;

			// These are read only properties.
			public EffectorLocation EffectorLocation => _effectorLocation;
			public EffectorType EffectorType => _effectorType;
			public Effector ParentEffector => _parentEffector;
			public Bone Bone => _bone;
			public Bone LeftBone => _leftBone;
			public Bone RightBone => _rightBone;
			public Vector3f DefaultPosition => _defaultPosition;
			public Quaternionf DefaultRotation => _defaultRotation;

			// Internal values. Acepted public accessing. Because these values are required for OnDrawGizmos.
			// (For debug only. You must use worldPosition / worldRotation in useful case.)
			public Vector3f _worldPosition = Vector3f.Zero;
			public Quaternionf _worldRotation = Quaternionf.Identity;

			// Internal flags.
			bool _isReadWorldPosition = false;
			bool _isReadWorldRotation = false;
			bool _isWrittenWorldPosition = false;
			bool _isWrittenWorldRotation = false;

			bool _isHiddenEyes = false;

			int _transformIsAlive = -1;

			public string EffectName => GetEffectorName(_effectorLocation);

			public bool TransformIsAlive
			{
				get {
					if (_transformIsAlive == -1) {
						_transformIsAlive = transform.Target == null ? 0 : 1;
					}

					return _transformIsAlive != 0;
				}
			}

			bool DefaultLocalBasisIsIdentity
			{
				get {
					if ((_effectorFlags & EffectorFlags.RotationContained) != EffectorFlags.None) { // Hips, Wrist, Foot
						Assert(_bone != null);
						if (_bone != null && _bone.LocalAxisFrom != LocalAxisFrom.None && _bone.BoneType != BoneType.Hips) { // Exclude Hips.
																															 // Hips is identity transform.
							return false;
						}
					}

					return true;
				}
			}

			public void Prefix() {
				positionEnabled.Value = GetPresetPositionEnabled(_effectorType);
				positionWeight.Value = GetPresetPositionWeight(_effectorType);
				pull.Value = GetPresetPull(_effectorType);
			}

			void PresetEffectorLocation(EffectorLocation effectorLocation) {
				_isPresetted.Value = true;
				_effectorLocation.Value = effectorLocation;
				_effectorType.Value = ToEffectorType(effectorLocation);
				_effectorFlags.Value = GetEffectorFlags(_effectorType);
			}

			// Call from Awake() or Editor Scripts.
			// Memo: bone.transform is null yet.
			public static void Prefix(
				Effector[] effectors,
				ref Effector effector,
				EffectorLocation effectorLocation,
				bool createEffectorTransform,
				Entity parentTransform,
				Effector parentEffector = null,
				Bone bone = null,
				Bone leftBone = null,
				Bone rightBone = null) {
				effector ??= new Effector();

				if (!effector._isPresetted ||
					effector._effectorLocation != effectorLocation ||
					(int)effector._effectorType.Value < 0 ||
					(int)effector._effectorType.Value >= (int)EffectorType.Max) {
					effector.PresetEffectorLocation(effectorLocation);
				}

				effector._parentEffector = parentEffector;
				effector._bone = bone;
				effector._leftBone = leftBone;
				effector._rightBone = rightBone;

				// Create or destroy effectorTransform.
				effector.PrefixTransform(createEffectorTransform, parentTransform);

				if (effectors != null) {
					effectors[(int)effectorLocation] = effector;
				}
			}

			static bool GetPresetPositionEnabled(EffectorType effectorType) {
				return effectorType switch {
					EffectorType.Wrist => true,
					EffectorType.Foot => true,
					_ => false,
				};
			}

			static float GetPresetPositionWeight(EffectorType effectorType) {
				return effectorType switch {
					EffectorType.Arm => 0.0f,
					_ => 1.0f,
				};
			}

			static float GetPresetPull(EffectorType effectorType) {
				return effectorType switch {
					EffectorType.Hips => 1.0f,
					EffectorType.Eyes => 1.0f,
					EffectorType.Arm => 1.0f,
					EffectorType.Wrist => 1.0f,
					EffectorType.Foot => 1.0f,
					_ => 0.0f,
				};
			}

			static EffectorFlags GetEffectorFlags(EffectorType effectorType) {
				return effectorType switch {
					EffectorType.Hips => EffectorFlags.RotationContained | EffectorFlags.PullContained,
					EffectorType.Neck => EffectorFlags.PullContained,
					EffectorType.Head => EffectorFlags.RotationContained | EffectorFlags.PullContained,
					EffectorType.Eyes => EffectorFlags.PullContained,
					EffectorType.Arm => EffectorFlags.PullContained,
					EffectorType.Wrist => EffectorFlags.RotationContained | EffectorFlags.PullContained,
					EffectorType.Foot => EffectorFlags.RotationContained | EffectorFlags.PullContained,
					EffectorType.Elbow => EffectorFlags.PullContained,
					EffectorType.Knee => EffectorFlags.PullContained,
					_ => EffectorFlags.None,
				};
			}

			void PrefixTransform(bool createEffectorTransform, Entity parentTransform) {
				if (createEffectorTransform) {
					if (transform.Target == null || transform.Target != _createdTransform.Target) {
						if (transform.Target == null) {
							var go = parentTransform.AddChild(GetEffectorName(_effectorLocation));
							transform.Target = go;
							_createdTransform.Target = go;
						}
					}
				}
			}

			public void Prepare(HumanoidIK fullBodyIK) {
				Assert(fullBodyIK != null);

				ClearInternal();

				ComputeDefaultTransform(fullBodyIK);

				// Reset transform.
				if (TransformIsAlive) {
					transform.Target.position.Value = _effectorType == EffectorType.Eyes
						? _defaultPosition + fullBodyIK.internalValues.defaultRootBasis.column2 * EYES_DEFAULTDISTANCE
						: _defaultPosition;

					transform.Target.rotation.Value = _defaultRotation;


					transform.Target.scale.Value = Vector3f.One;
				}

				_worldPosition = _defaultPosition;
				_worldRotation = _defaultRotation;
				if (_effectorType == EffectorType.Eyes) {
					_worldPosition += fullBodyIK.internalValues.defaultRootBasis.column2 * EYES_DEFAULTDISTANCE;
				}
			}

			public void ComputeDefaultTransform(HumanoidIK fullBodyIK) {
				if (_parentEffector != null) {
					_defaultRotation.Value = _parentEffector._defaultRotation;
				}

				if (_effectorType == EffectorType.Root) {
					_defaultPosition.Value = fullBodyIK.internalValues.defaultRootPosition;
					_defaultRotation.Value = fullBodyIK.internalValues.defaultRootRotation;
				}
				else if (_effectorType == EffectorType.HandFinger) {
					Assert(_bone != null);
					if (_bone != null) {
						if (_bone.TransformIsAlive) {
							_defaultPosition.Value = Bone._defaultPosition;
						}
						else { // Failsafe. Simulate finger tips.
							   // Memo: If transformIsAlive == false, _parentBone is null.
							Assert(_bone.ParentBoneLocationBased != null && _bone.ParentBoneLocationBased.ParentBoneLocationBased != null);
							if (_bone.ParentBoneLocationBased != null && _bone.ParentBoneLocationBased.ParentBoneLocationBased != null) {
								var tipTranslate = Bone.ParentBoneLocationBased._defaultPosition - Bone.ParentBoneLocationBased.ParentBoneLocationBased._defaultPosition;
								_defaultPosition.Value = Bone.ParentBoneLocationBased._defaultPosition + tipTranslate;
								_isSimulateFingerTips = true;
							}
						}
					}
				}
				else if (_effectorType == EffectorType.Eyes) {
					Assert(_bone != null);
					_isHiddenEyes = fullBodyIK.IsHiddenCustomEyes();
					if (!_isHiddenEyes && _bone != null && _bone.TransformIsAlive &&
						_leftBone != null && _leftBone.TransformIsAlive &&
						_rightBone != null && _rightBone.TransformIsAlive) {
						// _bone ... Head / _leftBone ... LeftEye / _rightBone ... RightEye
						_defaultPosition.Value = (_leftBone._defaultPosition + _rightBone._defaultPosition) * 0.5f;
					}
					else if (_bone != null && _bone.TransformIsAlive) {
						_defaultPosition.Value = _bone._defaultPosition;
						// _bone ... Head / _bone.parentBone ... Neck
						if (_bone.ParentBone != null && _bone.ParentBone.TransformIsAlive && _bone.ParentBone.BoneType == BoneType.Neck) {
							var neckToHead = _bone._defaultPosition - _bone.ParentBone._defaultPosition;
							var neckToHeadY = neckToHead.y > 0.0f ? neckToHead.y : 0.0f;
							_defaultPosition.Value += fullBodyIK.internalValues.defaultRootBasis.column1 * neckToHeadY;
							_defaultPosition.Value += fullBodyIK.internalValues.defaultRootBasis.column2 * neckToHeadY;
						}
					}
				}
				else if (_effectorType == EffectorType.Hips) {
					Assert(_bone != null && _leftBone != null && _rightBone != null);
					if (_bone != null && _leftBone != null && _rightBone != null) {
						// _bone ... Hips / _leftBone ... LeftLeg / _rightBone ... RightLeg
						_defaultPosition.Value = (_leftBone._defaultPosition + _rightBone._defaultPosition) * 0.5f;
					}
				}
				else { // Normally case.
					Assert(_bone != null);
					if (_bone != null) {
						_defaultPosition.Value = Bone._defaultPosition;
						if (!DefaultLocalBasisIsIdentity) { // For wrist & foot.
							_defaultRotation.Value = Bone._localAxisRotation;
						}
					}
				}
			}

			void ClearInternal() {
				_transformIsAlive = -1;
				_defaultPosition.Value = Vector3f.Zero;
				_defaultRotation.Value = Quaternionf.Identity;
			}

			public void PrepareUpdate() {
				_transformIsAlive = -1;
				_isReadWorldPosition = false;
				_isReadWorldRotation = false;
				_isWrittenWorldPosition = false;
				_isWrittenWorldRotation = false;
			}

			public Vector3f WorldPosition
			{
				get {
					if (!_isReadWorldPosition && !_isWrittenWorldPosition) {
						_isReadWorldPosition = true;
						if (TransformIsAlive) {
							_worldPosition = transform.Target.GlobalTrans.Translation;
						}
					}
					return _worldPosition;
				}
				set {
					_isWrittenWorldPosition = true;
					_worldPosition = value;
				}
			}

			public Vector3f BoneWorldPosition
			{
				get {
					if (_effectorType == EffectorType.Eyes) {
						if (!_isHiddenEyes && _bone != null && _bone.TransformIsAlive &&
							_leftBone != null && _leftBone.TransformIsAlive &&
							_rightBone != null && _rightBone.TransformIsAlive) {
							// _bone ... Head / _leftBone ... LeftEye / _rightBone ... RightEye
							return (_leftBone.WorldPosition + _rightBone.WorldPosition) * 0.5f;
						}
						else if (_bone != null && _bone.TransformIsAlive) {
							var currentPosition = _bone.WorldPosition;
							// _bone ... Head / _bone.parentBone ... Neck
							if (_bone.ParentBone != null && _bone.ParentBone.TransformIsAlive && _bone.ParentBone.BoneType == BoneType.Neck) {
								var neckToHead = _bone._defaultPosition - _bone.ParentBone._defaultPosition;
								var neckToHeadY = neckToHead.y > 0.0f ? neckToHead.y : 0.0f;
								var parentBaseRotation = _bone.ParentBone.WorldRotation * _bone.ParentBone._worldToBaseRotation;
								IKMath.MatSetRot(out var parentBaseBasis, ref parentBaseRotation);
								currentPosition += parentBaseBasis.column1 * neckToHeadY;
								currentPosition += parentBaseBasis.column2 * neckToHeadY;
							}
							return currentPosition;
						}
					}
					else if (_isSimulateFingerTips) {
						if (_bone != null &&
							_bone.ParentBoneLocationBased != null &&
							_bone.ParentBoneLocationBased.TransformIsAlive &&
							_bone.ParentBoneLocationBased.ParentBoneLocationBased != null &&
							_bone.ParentBoneLocationBased.ParentBoneLocationBased.TransformIsAlive) {
							var parentPosition = _bone.ParentBoneLocationBased.WorldPosition;
							var parentParentPosition = _bone.ParentBoneLocationBased.ParentBoneLocationBased.WorldPosition;
							return parentPosition + (parentPosition - parentParentPosition);
						}
					}
					else {
						if (_bone != null && _bone.TransformIsAlive) {
							return _bone.WorldPosition;
						}
					}

					return WorldPosition; // Failsafe.
				}
			}

			public Quaternionf WorldRotation
			{
				get {
					if (!_isReadWorldRotation && !_isWrittenWorldRotation) {
						_isReadWorldRotation = true;
						if (TransformIsAlive) {
							_worldRotation = transform.Target.GlobalTrans.Rotation;
						}
					}
					return _worldRotation;
				}
				set {
					_isWrittenWorldRotation = true;
					_worldRotation = value;
				}
			}
		}




		public enum LocalAxisFrom : byte
		{
			None,
			Parent,
			Child,
			Max,
			Unknown = Max,
		}

		public sealed partial class Bone : SyncObject
		{
			public readonly SyncRef<Entity> transform;

			[Default(false)]
			public readonly Sync<bool> _isPresetted;
			[Default(BoneLocation.Unknown)]
			public readonly Sync<BoneLocation> _boneLocation;
			[Default(BoneType.Unknown)]
			public readonly Sync<BoneType> _boneType;
			[Default(Side.None)]
			public readonly Sync<Side> _boneSide;
			[Default(FingerType.Unknown)]
			public readonly Sync<FingerType> _fingerType;
			[Default(-1)]
			public readonly Sync<int> _fingerIndex;
			[Default(LocalAxisFrom.Unknown)]
			public readonly Sync<LocalAxisFrom> _localAxisFrom;
			[Default(DirectionAs.Uknown)]
			public readonly Sync<DirectionAs> _localDirectionAs;

			public BoneLocation BoneLocation => _boneLocation;
			public BoneType BoneType => _boneType;
			public Side BoneSide => _boneSide;
			public FingerType FingerType => _fingerType;
			public int FingerIndex => _fingerIndex;
			public LocalAxisFrom LocalAxisFrom => _localAxisFrom;
			public DirectionAs LocalDirectionAs => _localDirectionAs;

			public Bone ParentBone { get; private set; }
			public Bone ParentBoneLocationBased { get; private set; }

			// Internal values. Acepted public accessing. Because faster than property methods.
			// Memo: defaultPosition / defaultRotation is copied from transform.
			public Vector3f _defaultPosition = Vector3f.Zero;             // transform.position
			public Quaternionf _defaultRotation = Quaternionf.Identity;   // transform.rotation
			public IKMatrix3x3 _defaultBasis = IKMatrix3x3.identity;
			public Vector3f _defaultLocalTranslate = Vector3f.Zero;       // transform.position - transform.parent.position
			public Vector3f _defaultLocalDirection = Vector3f.Zero;       // _defaultLocalTranslate.Normalize()
			public FastLength _defaultLocalLength = new();   // _defaultLocalTranslate.magnitude

			// Internal values. Acepted public accessing. Because faster than property methods.
			// Memo: These values are modified in Prepare().
			public IKMatrix3x3 _localAxisBasis = IKMatrix3x3.identity;
			public IKMatrix3x3 _localAxisBasisInv = IKMatrix3x3.identity;
			public Quaternionf _localAxisRotation = Quaternionf.Identity;
			public Quaternionf _localAxisRotationInv = Quaternionf.Identity;
			public IKMatrix3x3 _worldToBoneBasis = IKMatrix3x3.identity;
			public IKMatrix3x3 _boneToWorldBasis = IKMatrix3x3.identity;
			public IKMatrix3x3 _worldToBaseBasis = IKMatrix3x3.identity;
			public IKMatrix3x3 _baseToWorldBasis = IKMatrix3x3.identity;
			public Quaternionf _worldToBoneRotation = Quaternionf.Identity; // Inverse( _defaultRotation ) * _localAxisRotation
			public Quaternionf _boneToWorldRotation = Quaternionf.Identity; // Inverse( _worldToBoneRotation )
			public Quaternionf _worldToBaseRotation = Quaternionf.Identity; // Inverse( _defaultRotation ) * baseRotation
			public Quaternionf _baseToWorldRotation = Quaternionf.Identity; // Inverse( _worldToBaseRotation )
			public IKMatrix3x3 _baseToBoneBasis = IKMatrix3x3.identity;
			public IKMatrix3x3 _boneToBaseBasis = IKMatrix3x3.identity;

			// Internal Flags. These values are modified in Prepare().
			[Default(false)]
			public readonly Sync<bool> _isWritebackWorldPosition; // for Hips / Spine only.

			public bool IsWritebackWorldPosition => _isWritebackWorldPosition;

			// Internal values. Acepted public accessing. Because these values are required for OnDrawGizmos.
			// (For debug only. You must use worldPosition / worldRotation in useful case.)
			public Vector3f _worldPosition = Vector3f.Zero;
			public Quaternionf _worldRotation = Quaternionf.Identity;

			// Internal Flags.
			bool _isReadWorldPosition = false;
			bool _isReadWorldRotation = false;
			bool _isWrittenWorldPosition = false;
			bool _isWrittenWorldRotation = false;

			int _transformIsAlive = -1;

			public string BoneName => _boneType.ToString();

			public bool TransformIsAlive
			{
				get {
					if (_transformIsAlive == -1) {
						_transformIsAlive = transform.Target == null ? 0 : 1;
					}

					return _transformIsAlive != 0;
				}
			}

			public Entity ParentTransform => ParentBone?.transform.Target;

			void PresetBoneLocation(BoneLocation boneLocation) {
				_isPresetted.Value = true;
				_boneLocation.Value = boneLocation;
				_boneType.Value = ToBoneType(boneLocation);
				_boneSide.Value = ToBoneSide(boneLocation);
				if (_boneType == BoneType.HandFinger) {
					_fingerType.Value = ToFingerType(boneLocation);
					_fingerIndex.Value = ToFingerIndex(boneLocation);
				}
				else {
					_fingerType.Value = FingerType.Unknown;
					_fingerIndex.Value = -1;
				}
				PresetLocalAxis();
			}

			void PresetLocalAxis() {
				switch (_boneType.Value) {
					case BoneType.Hips:
						PresetLocalAxis(LocalAxisFrom.Child, DirectionAs.YPlus);
						return;
					case BoneType.Spine:
						PresetLocalAxis(LocalAxisFrom.Child, DirectionAs.YPlus);
						return;
					case BoneType.Neck:
						PresetLocalAxis(LocalAxisFrom.Child, DirectionAs.YPlus);
						return;
					case BoneType.Head:
						PresetLocalAxis(LocalAxisFrom.None, DirectionAs.None);
						return;
					case BoneType.Eye:
						PresetLocalAxis(LocalAxisFrom.None, DirectionAs.None);
						return;

					case BoneType.Leg:
						PresetLocalAxis(LocalAxisFrom.Child, DirectionAs.YMinus);
						return;
					case BoneType.Knee:
						PresetLocalAxis(LocalAxisFrom.Child, DirectionAs.YMinus);
						return;
					case BoneType.Foot:
						PresetLocalAxis(LocalAxisFrom.Parent, DirectionAs.YMinus);
						return;

					case BoneType.Shoulder:
						PresetLocalAxis(LocalAxisFrom.Child, _boneSide == Side.Left ? DirectionAs.XMinus : DirectionAs.XPlus);
						return;
					case BoneType.Arm:
						PresetLocalAxis(LocalAxisFrom.Child, _boneSide == Side.Left ? DirectionAs.XMinus : DirectionAs.XPlus);
						return;
					case BoneType.ArmRoll:
						PresetLocalAxis(LocalAxisFrom.Parent, _boneSide == Side.Left ? DirectionAs.XMinus : DirectionAs.XPlus);
						return;
					case BoneType.Elbow:
						PresetLocalAxis(LocalAxisFrom.Child, _boneSide == Side.Left ? DirectionAs.XMinus : DirectionAs.XPlus);
						return;
					case BoneType.ElbowRoll:
						PresetLocalAxis(LocalAxisFrom.Parent, _boneSide == Side.Left ? DirectionAs.XMinus : DirectionAs.XPlus);
						return;
					case BoneType.Wrist:
						PresetLocalAxis(LocalAxisFrom.Parent, _boneSide == Side.Left ? DirectionAs.XMinus : DirectionAs.XPlus);
						return;
				}

				if (_boneType == BoneType.HandFinger) {
					var localAxisFrom = _fingerIndex + 1 == MAX_HAND_FINGER_LENGTH ? LocalAxisFrom.Parent : LocalAxisFrom.Child;
					PresetLocalAxis(localAxisFrom, _boneSide == Side.Left ? DirectionAs.XMinus : DirectionAs.XPlus);
					return;
				}
			}

			void PresetLocalAxis(LocalAxisFrom localAxisFrom, DirectionAs localDirectionAs) {
				_localAxisFrom.Value = localAxisFrom;
				_localDirectionAs.Value = localDirectionAs;
			}

			// Call from Awake() or Editor Scripts.
			// Memo: transform is null yet.
			public static void Prefix(Bone[] bones, ref Bone bone, BoneLocation boneLocation, Bone parentBoneLocationBased = null) {
				Assert(bones != null);
				bone ??= new Bone();

				if (!bone._isPresetted ||
					bone._boneLocation != boneLocation ||
					(int)bone._boneType.Value < 0 ||
					(int)bone._boneType.Value >= (int)BoneType.Max ||
					bone._localAxisFrom == LocalAxisFrom.Unknown ||
					bone._localDirectionAs == DirectionAs.Uknown) {
					bone.PresetBoneLocation(boneLocation);
				}

				bone.ParentBoneLocationBased = parentBoneLocationBased;

				if (bones != null) {
					bones[(int)boneLocation] = bone;
				}
			}

			public void Prepare(HumanoidIK fullBodyIK) {
				Assert(fullBodyIK != null);

				_transformIsAlive = -1;
				_localAxisBasis = IKMatrix3x3.identity;
				_isWritebackWorldPosition.Value = false;

				ParentBone = null;

				// Find transform alive parent bone.
				if (TransformIsAlive) {
					for (var temp = ParentBoneLocationBased; temp != null; temp = temp.ParentBoneLocationBased) {
						if (temp.TransformIsAlive) {
							ParentBone = temp;
							break;
						}
					}
				}

				if (_boneLocation == BoneLocation.Hips) {
					if (TransformIsAlive) {
						_isWritebackWorldPosition.Value = true;
					}
				}
				else if (_boneLocation == BoneLocation.Spine) {
					if (TransformIsAlive) {
						if (ParentBone != null && ParentBone.TransformIsAlive) {
							if (HumanoidIK.IsParentOfRecusively(ParentBone.transform.Target, transform.Target)) {
								_isWritebackWorldPosition.Value = true;
							}
						}
					}
				}

				if (_boneType == BoneType.Eye) {
					if (fullBodyIK.IsHiddenCustomEyes()) {
						_isWritebackWorldPosition.Value = true;
					}
				}

				// Get defaultPosition / defaultRotation
				if (TransformIsAlive) {
					_defaultPosition = transform.Target.GlobalTrans.Translation;
					_defaultRotation = transform.Target.GlobalTrans.Rotation;
					IKMath.MatSetRot(out _defaultBasis, ref _defaultRotation);
					if (ParentBone != null) { // Always _parentBone.transformIsAlive == true
						_defaultLocalTranslate = _defaultPosition - ParentBone._defaultPosition;
						_defaultLocalLength = FastLength.FromVector3(ref _defaultLocalTranslate);
						if (_defaultLocalLength.length > IKMath.FLOAT_EPSILON) {
							var lengthInv = 1.0f / _defaultLocalLength.length;
							_defaultLocalDirection.x = _defaultLocalTranslate.x * lengthInv;
							_defaultLocalDirection.y = _defaultLocalTranslate.y * lengthInv;
							_defaultLocalDirection.z = _defaultLocalTranslate.z * lengthInv;
						}
					}

					IKMath.MatMultInv0(out _worldToBaseBasis, _defaultBasis, fullBodyIK.internalValues.defaultRootBasis);
					_baseToWorldBasis = _worldToBaseBasis.Transpose;
					IKMath.MatGetRot(out _worldToBaseRotation, _worldToBaseBasis);
					_baseToWorldRotation = IKMath.Inverse(_worldToBaseRotation);
				}
				else {
					_defaultPosition = Vector3f.Zero;
					_defaultRotation = Quaternionf.Identity;
					_defaultBasis = IKMatrix3x3.identity;
					_defaultLocalTranslate = Vector3f.Zero;
					_defaultLocalLength = new FastLength();
					_defaultLocalDirection = Vector3f.Zero;

					_worldToBaseBasis = IKMatrix3x3.identity;
					_baseToWorldBasis = IKMatrix3x3.identity;
					_worldToBaseRotation = Quaternionf.Identity;
					_baseToWorldRotation = Quaternionf.Identity;
				}

				ComputeLocalAxis(fullBodyIK); // Require PostPrepare()
			}

			void ComputeLocalAxis(HumanoidIK fullBodyIK) {
				// Compute _localAxisBasis for each bones.
				if (TransformIsAlive && ParentBone != null && ParentBone.TransformIsAlive) {
					if (_localAxisFrom == LocalAxisFrom.Parent ||
						ParentBone._localAxisFrom == LocalAxisFrom.Child) {
						var dir = _defaultLocalDirection;
						if (dir.x != 0.0f || dir.y != 0.0f || dir.z != 0.0f) {
							if (_localAxisFrom == LocalAxisFrom.Parent) {
								IKMath.ComputeBasisFrom(out _localAxisBasis, fullBodyIK.internalValues.defaultRootBasis, dir, _localDirectionAs);
							}

							if (ParentBone._localAxisFrom == LocalAxisFrom.Child) {
								if (ParentBone._boneType == BoneType.Shoulder) {
									var shoulderBone = ParentBone;
									var spineUBone = ParentBone.ParentBone;
									var neckBone = fullBodyIK.headBones?.neck;
									if (neckBone != null && !neckBone.TransformIsAlive) {
										neckBone = null;
									}

									if (fullBodyIK.internalValues.shoulderDirYAsNeck == -1) {
										if (fullBodyIK.settings.shoulderDirYAsNeck == AutomaticBool.Auto) {
											if (spineUBone != null && neckBone != null) {
												var shoulderToSpineU = shoulderBone._defaultLocalDirection;
												var shoulderToNeck = neckBone._defaultPosition - shoulderBone._defaultPosition;
												if (IKMath.VecNormalize(ref shoulderToNeck)) {
													var shoulderToSpineUTheta = MathF.Abs(Vector3f.Dot(dir, shoulderToSpineU));
													var shoulderToNeckTheta = MathF.Abs(Vector3f.Dot(dir, shoulderToNeck));
													fullBodyIK.internalValues.shoulderDirYAsNeck = shoulderToSpineUTheta < shoulderToNeckTheta ? 0 : 1;
												}
												else {
													fullBodyIK.internalValues.shoulderDirYAsNeck = 0;
												}
											}
											else {
												fullBodyIK.internalValues.shoulderDirYAsNeck = 0;
											}
										}
										else {
											fullBodyIK.internalValues.shoulderDirYAsNeck = fullBodyIK.settings.shoulderDirYAsNeck != AutomaticBool.Disable ? 1 : 0;
										}
									}

									Vector3f xDir, yDir, zDir;
									xDir = ParentBone._localDirectionAs == DirectionAs.XMinus ? -dir : dir;
									yDir = fullBodyIK.internalValues.shoulderDirYAsNeck != 0 && neckBone != null
										? neckBone._defaultPosition - shoulderBone._defaultPosition
										: shoulderBone._defaultLocalDirection;
									zDir = Vector3f.Cross(xDir, yDir);
									yDir = Vector3f.Cross(zDir, xDir);
									if (IKMath.VecNormalize2(ref yDir, ref zDir)) {
										ParentBone._localAxisBasis.SetColumn(ref xDir, ref yDir, ref zDir);
									}
								}
								else if (ParentBone._boneType == BoneType.Spine && _boneType != BoneType.Spine && _boneType != BoneType.Neck) {
									// Compute spine/neck only( Exclude shouder / arm ).
								}
								else if (ParentBone._boneType == BoneType.Hips && _boneType != BoneType.Spine) {
									// Compute spine only( Exclude leg ).
								}
								else {
									if (ParentBone._boneType == BoneType.Hips) {
										var baseX = fullBodyIK.internalValues.defaultRootBasis.column0;
										IKMath.ComputeBasisFromXYLockY(out ParentBone._localAxisBasis, baseX, ref dir);
									}
									else if (ParentBone._boneType.Value is BoneType.Spine or BoneType.Neck) {
										// Using parent axis for spine or neck. Preprocess for BodyIK.
										if (ParentBone.ParentBone != null) {
											var dirX = ParentBone.ParentBone._localAxisBasis.column0;
											IKMath.ComputeBasisFromXYLockY(out ParentBone._localAxisBasis, dirX, ref dir);
										}
									}
									else {
										if (_localAxisFrom == LocalAxisFrom.Parent && _localDirectionAs == ParentBone._localDirectionAs) {
											ParentBone._localAxisBasis = _localAxisBasis;
										}
										else {
											IKMath.ComputeBasisFrom(out ParentBone._localAxisBasis,
												 fullBodyIK.internalValues.defaultRootBasis, dir, ParentBone._localDirectionAs);
										}
									}
								}
							}

						}
					}
				}
			}

			public void PostPrepare() {
				if (_localAxisFrom != LocalAxisFrom.None) {
					_localAxisBasisInv = _localAxisBasis.Transpose;
					IKMath.MatGetRot(out _localAxisRotation, _localAxisBasis);
					_localAxisRotationInv = IKMath.Inverse(_localAxisRotation);
					IKMath.MatMultInv0(out _worldToBoneBasis, _defaultBasis, _localAxisBasis);
					_boneToWorldBasis = _worldToBoneBasis.Transpose;
					IKMath.MatGetRot(out _worldToBoneRotation, _worldToBoneBasis);
					_boneToWorldRotation = IKMath.Inverse(_worldToBoneRotation);
				}
				else {
					_localAxisBasis = IKMatrix3x3.identity;
					_localAxisBasisInv = IKMatrix3x3.identity;
					_localAxisRotation = Quaternionf.Identity;
					_localAxisRotationInv = Quaternionf.Identity;

					_worldToBoneBasis = _defaultBasis.Transpose;
					_boneToWorldBasis = _defaultBasis;
					_worldToBoneRotation = IKMath.Inverse(_defaultRotation);
					_boneToWorldRotation = _defaultRotation;
				}

				IKMath.MatMultInv0(out _baseToBoneBasis, _worldToBaseBasis, _worldToBoneBasis);
				_boneToBaseBasis = _baseToBoneBasis.Transpose;
			}

			public void PrepareUpdate() {
				_transformIsAlive = -1;
				_isReadWorldPosition = false;
				_isReadWorldRotation = false;
				_isWrittenWorldPosition = false;
				_isWrittenWorldRotation = false;
			}

			public void SyncDisplacement() {
				if (ParentBone != null && ParentBone.TransformIsAlive && TransformIsAlive) {
					var translate = WorldPosition - ParentBone.WorldPosition;
					_defaultLocalLength = FastLength.FromVector3(ref translate);
					if (ParentBone.transform.Target == transform.Target.parent.Target) {
						var localPosition = transform.Target.position.Value;
						if (IKMath.VecNormalize(ref localPosition)) {
							IKMath.MatMultVec(out var tempDirection, ParentBone._defaultBasis, localPosition);
							_defaultLocalDirection = tempDirection;
							_defaultLocalTranslate = tempDirection * _defaultLocalLength.length;
						}
						else {
							_defaultLocalDirection = Vector3f.Zero;
							_defaultLocalTranslate = Vector3f.Zero;
						}
					}
					else {
						_defaultLocalTranslate = _defaultLocalDirection * _defaultLocalLength.length;
					}
				}
			}

			public void PostSyncDisplacement(HumanoidIK fullBodyIK) {
				if (_boneLocation == BoneLocation.Hips) {
					_defaultPosition = fullBodyIK.boneCaches.defaultHipsPosition + fullBodyIK.boneCaches.hipsOffset;
				}
				else if (ParentBone != null) {
					_defaultPosition = ParentBone._defaultPosition + _defaultLocalTranslate;
				}

				ComputeLocalAxis(fullBodyIK); // Require PostPrepare()
			}

			public Vector3f WorldPosition
			{
				get {
					if (!_isReadWorldPosition && !_isWrittenWorldPosition) {
						_isReadWorldPosition = true;
						if (TransformIsAlive) {
							_worldPosition = transform.Target.GlobalTrans.Translation;
						}
					}
					return _worldPosition;
				}
				set {
					_isWrittenWorldPosition = true;
					_worldPosition = value;
				}
			}

			public Quaternionf WorldRotation
			{
				get {
					if (!_isReadWorldRotation && !_isWrittenWorldRotation) {
						_isReadWorldRotation = true;
						if (TransformIsAlive) {
							_worldRotation = transform.Target.GlobalTrans.Rotation;
						}
					}
					return _worldRotation;
				}
				set {
					_isWrittenWorldRotation = true;
					_worldRotation = value;
				}
			}

			public void Forcefix_worldRotation() {
				if (TransformIsAlive) {
					if (!_isReadWorldRotation) {
						_isReadWorldRotation = true;
						_worldRotation = transform.Target.GlobalTrans.Rotation;
					}
					_isWrittenWorldRotation = true;

					// Fix worldPosition
					if (ParentBone != null && ParentBone.TransformIsAlive) {
						var parentWorldRotation = ParentBone.WorldRotation;

						IKMath.MatSetRotMultInv1(out var parentRotationBasis, parentWorldRotation, ParentBone._defaultRotation);

						var parentWorldPosition = ParentBone.WorldPosition;

						IKMath.MatMultVecPreSubAdd(out var tempPos, parentRotationBasis, _defaultPosition, ParentBone._defaultPosition, parentWorldPosition);

						_isWrittenWorldPosition = true;
						_isWritebackWorldPosition.Value = true;
						_worldPosition = tempPos;
					}
				}
			}

			public void WriteToTransform() {
				if (_isWrittenWorldPosition) {
					_isWrittenWorldPosition = false; // Turn off _isWrittenWorldPosition
					if (_isWritebackWorldPosition && TransformIsAlive) {
						transform.Target.position.Value = _worldPosition;
					}
				}
				if (_isWrittenWorldRotation) {
					_isWrittenWorldRotation = false; // Turn off _isWrittenWorldRotation
					if (TransformIsAlive) {
						transform.Target.rotation.Value = _worldRotation;
					}
				}
			}
		}

		public sealed partial class BodyBones : SyncObject
		{
			public readonly Bone hips;
			public readonly Bone spine;
			public readonly Bone spine2;
			public readonly Bone spine3;
			public readonly Bone spine4;

			public Bone SpineU => spine4;
		}

		public sealed partial class HeadBones : SyncObject
		{
			public readonly Bone neck;
			public readonly Bone head;
			public readonly Bone leftEye;
			public readonly Bone rightEye;
		}

		public sealed partial class LegBones : SyncObject
		{
			public readonly Bone leg;
			public readonly Bone knee;
			public readonly Bone foot;
		}

		public sealed partial class ArmBones : SyncObject
		{
			public readonly Bone shoulder;
			public readonly Bone arm;
			public readonly ConstArrayFour<Bone> armRoll;
			public readonly Bone elbow;
			public readonly ConstArrayFour<Bone> elbowRoll;
			public readonly Bone wrist;
		}

		public sealed partial class FingersBones : SyncObject
		{
			public readonly ConstArrayFour<Bone> thumb;
			public readonly ConstArrayFour<Bone> index;
			public readonly ConstArrayFour<Bone> middle;
			public readonly ConstArrayFour<Bone> ring;
			public readonly ConstArrayFour<Bone> little;
		}

		public sealed partial class BodyEffectors : SyncObject
		{
			public readonly Effector hips;
		}

		public sealed partial class HeadEffectors : SyncObject
		{
			public readonly Effector neck;
			public readonly Effector head;
			public readonly Effector eyes;
		}

		public sealed partial class LegEffectors : SyncObject
		{
			public readonly Effector knee;
			public readonly Effector foot;
		}

		public sealed partial class ArmEffectors : SyncObject
		{
			public readonly Effector arm;
			public readonly Effector elbow;
			public readonly Effector wrist;
		}

		public sealed partial class FingersEffectors : SyncObject
		{
			public readonly Effector thumb;
			public readonly Effector index;
			public readonly Effector middle;
			public readonly Effector ring;
			public readonly Effector little;
		}

		public enum AutomaticBool : sbyte
		{
			Auto = -1,
			Disable = 0,
			Enable = 1,
		}

		public enum SyncDisplacement : byte
		{
			Disable,
			Firstframe,
			Everyframe,
		}

		public sealed partial class Settings : SyncObject
		{
			[Default(AutomaticBool.Auto)]
			public readonly Sync<AutomaticBool> animatorEnabled;
			[Default(AutomaticBool.Auto)]
			public readonly Sync<AutomaticBool> resetTransforms;
			[Default(SyncDisplacement.Disable)]
			public readonly Sync<SyncDisplacement> syncDisplacement;
			[Default(AutomaticBool.Auto)]
			public readonly Sync<AutomaticBool> shoulderDirYAsNeck;
			[Default(true)]
			public readonly Sync<bool> automaticPrepareHumanoid;
			[Default(false)]
			public readonly Sync<bool> automaticConfigureSpineEnabled;
			[Default(false)]
			public readonly Sync<bool> automaticConfigureRollBonesEnabled;
			[Default(false)]
			public readonly Sync<bool> rollBonesEnabled;
			[Default(true)]
			public readonly Sync<bool> createEffectorTransform;

			public sealed partial class BodyIK : SyncObject
			{
				[Default(true)]
				public readonly Sync<bool> forceSolveEnabled;
				[Default(true)]
				public readonly Sync<bool> lowerSolveEnabled;
				[Default(true)]
				public readonly Sync<bool> upperSolveEnabled;
				[Default(true)]
				public readonly Sync<bool> computeWorldTransform;
				[Default(true)]
				public readonly Sync<bool> shoulderSolveEnabled;
				[Default(0.25f)]
				public readonly Sync<float> shoulderSolveBendingRate;
				[Default(true)]
				public readonly Sync<bool> shoulderLimitEnabled;
				[Default(30.0f)]
				public readonly Sync<float> shoulderLimitAngleYPlus;
				[Default(1.0f)]
				public readonly Sync<float> shoulderLimitAngleYMinus;
				[Default(30.0f)]
				public readonly Sync<float> shoulderLimitAngleZ;
				[Default(0.5f)]
				public readonly Sync<float> spineDirXLegToArmRate;
				[Default(1.0f)]
				public readonly Sync<float> spineDirXLegToArmToRate;
				[Default(0.5f)]
				public readonly Sync<float> spineDirYLerpRate;
				[Default(1.0f)]
				public readonly Sync<float> upperBodyMovingfixRate;
				[Default(0.8f)]
				public readonly Sync<float> upperHeadMovingfixRate;
				[Default(0.5f)]
				public readonly Sync<float> upperCenterLegTranslateRate;
				[Default(0.65f)]
				public readonly Sync<float> upperSpineTranslateRate;
				[Default(0.6f)]
				public readonly Sync<float> upperCenterLegRotateRate;
				[Default(0.9f)]
				public readonly Sync<float> upperSpineRotateRate;
				[Default(1.0f)]
				public readonly Sync<float> upperPostTranslateRate;
				[Default(true)]
				public readonly Sync<bool> upperSolveHipsEnabled;
				[Default(true)]
				public readonly Sync<bool> upperSolveSpineEnabled;
				[Default(true)]
				public readonly Sync<bool> upperSolveSpine2Enabled;
				[Default(true)]
				public readonly Sync<bool> upperSolveSpine3Enabled;
				[Default(true)]
				public readonly Sync<bool> upperSolveSpine4Enabled;
				[Default(1.0f)]
				public readonly Sync<float> upperCenterLegLerpRate;
				[Default(1.0f)]
				public readonly Sync<float> upperSpineLerpRate;
				[Default(true)]
				public readonly Sync<bool> upperDirXLimitEnabled; // Effective for spineLimitEnabled && spineLimitAngleX
				[Default(20.0f)]
				public readonly Sync<float> upperDirXLimitAngleY;
				[Default(true)]
				public readonly Sync<bool> spineLimitEnabled;
				[Default(false)]
				public readonly Sync<bool> spineAccurateLimitEnabled;
				[Default(40.0f)]
				public readonly Sync<float> spineLimitAngleX;
				[Default(25.0f)]
				public readonly Sync<float> spineLimitAngleY;
				[Default(0.2f)]
				public readonly Sync<float> upperContinuousPreTranslateRate;
				[Default(0.65f)]
				public readonly Sync<float> upperContinuousPreTranslateStableRate;
				[Default(0.0f)]
				public readonly Sync<float> upperContinuousCenterLegRotationStableRate;
				[Default(0.01f)]
				public readonly Sync<float> upperContinuousPostTranslateStableRate;
				[Default(0.5f)]
				public readonly Sync<float> upperContinuousSpineDirYLerpRate;
				[Default(0.6f)]
				public readonly Sync<float> upperNeckToCenterLegRate;
				[Default(0.9f)]
				public readonly Sync<float> upperNeckToSpineRate;
				[Default(0.2f)]
				public readonly Sync<float> upperEyesToCenterLegRate;
				[Default(0.5f)]
				public readonly Sync<float> upperEyesToSpineRate;
				[Default(0.8f)]
				public readonly Sync<float> upperEyesYawRate;
				[Default(0.25f)]
				public readonly Sync<float> upperEyesPitchUpRate;
				[Default(0.5f)]
				public readonly Sync<float> upperEyesPitchDownRate;
				[Default(80.0f)]
				public readonly Sync<float> upperEyesLimitYaw;
				[Default(10.0f)]
				public readonly Sync<float> upperEyesLimitPitchUp;
				[Default(45.0f)]
				public readonly Sync<float> upperEyesLimitPitchDown;
				[Default(160.0f)]
				public readonly Sync<float> upperEyesTraceAngle;
			}

			public sealed partial class LimbIK : SyncObject
			{
				[Default(true)]
				public readonly Sync<bool> legAlwaysSolveEnabled;
				[Default(false)]
				public readonly Sync<bool> armAlwaysSolveEnabled;
				[Default(0.0f)]
				public readonly Sync<float> automaticKneeBaseAngle;
				[Default(false)]
				public readonly Sync<bool> presolveKneeEnabled;
				[Default(false)]
				public readonly Sync<bool> presolveElbowEnabled;
				[Default(1.0f)]
				public readonly Sync<float> presolveKneeRate;
				[Default(10.0f)]
				public readonly Sync<float> presolveKneeLerpAngle;
				[Default(0.1f)]
				public readonly Sync<float> presolveKneeLerpLengthRate;
				[Default(1.0f)]
				public readonly Sync<float> presolveElbowRate;
				[Default(10.0f)]
				public readonly Sync<float> presolveElbowLerpAngle;
				[Default(0.1f)]
				public readonly Sync<float> presolveElbowLerpLengthRate;
				[Default(true)]
				public readonly Sync<bool> prefixLegEffectorEnabled;
				[Default(60.0f)]
				public readonly Sync<float> prefixLegUpperLimitAngle;
				[Default(45.0f)]
				public readonly Sync<float> prefixKneeUpperLimitAngle;
				[Default(0.1f)]
				public readonly Sync<float> legEffectorMinLengthRate;
				[Default(0.9999f)]
				public readonly Sync<float> legEffectorMaxLengthRate;
				[Default(0.9999f)]
				public readonly Sync<float> armEffectorMaxLengthRate;
				[Default(true)]
				public readonly Sync<bool> armBasisForcefixEnabled;
				[Default(0.99f)]
				public readonly Sync<float> armBasisForcefixEffectorLengthRate;
				[Default(0.03f)]
				public readonly Sync<float> armBasisForcefixEffectorLengthLerpRate;
				[Default(true)]
				public readonly Sync<bool> armEffectorBackfixEnabled;
				[Default(true)]
				public readonly Sync<bool> armEffectorInnerfixEnabled;

				// Arm back area.(Automatic only, Based on localXZ)
				[Default(5.0f)]
				public readonly Sync<float> armEffectorBackBeginAngle;
				[Default(-10.0f)]
				public readonly Sync<float> armEffectorBackCoreBeginAngle;
				[Default(-30.0f)]
				public readonly Sync<float> armEffectorBackCoreEndAngle;
				[Default(-160.0f)]
				public readonly Sync<float> armEffectorBackEndAngle;

				// Arm back area.(Automatic only, Based on localYZ)
				[Default(8.0f)]
				public readonly Sync<float> armEffectorBackCoreUpperAngle;
				[Default(-15.0f)]
				public readonly Sync<float> armEffectorBackCoreLowerAngle;

				// Arm elbow angles.(Automatic only)
				[Default(30.0f)]
				public readonly Sync<float> automaticElbowBaseAngle;
				[Default(90.0f)]
				public readonly Sync<float> automaticElbowLowerAngle;
				[Default(90.0f)]
				public readonly Sync<float> automaticElbowUpperAngle;
				[Default(180.0f)]
				public readonly Sync<float> automaticElbowBackUpperAngle;
				[Default(330.0f)]
				public readonly Sync<float> automaticElbowBackLowerAngle;

				// Arm elbow limit angles.(Automatic / Manual)
				[Default(5.0f)]
				public readonly Sync<float> elbowFrontInnerLimitAngle;
				[Default(0.0f)]
				public readonly Sync<float> elbowBackInnerLimitAngle;

				// Wrist limit
				[Default(true)]
				public readonly Sync<bool> wristLimitEnabled;
				[Default(90.0f)]
				public readonly Sync<float> wristLimitAngle;

				// Foot limit
				[Default(true)]
				public readonly Sync<bool> footLimitEnabled;
				[Default(45.0f)]
				public readonly Sync<float> footLimitYaw;
				[Default(45.0f)]
				public readonly Sync<float> footLimitPitchUp;
				[Default(60.0f)]
				public readonly Sync<float> footLimitPitchDown;
				[Default(45.0f)]
				public readonly Sync<float> footLimitRoll;
			}

			public sealed partial class HeadIK : SyncObject
			{
				[Default(15.0f)]
				public readonly Sync<float> neckLimitPitchUp;
				[Default(30.0f)]
				public readonly Sync<float> neckLimitPitchDown;
				[Default(5.0f)]
				public readonly Sync<float> neckLimitRoll;
				[Default(0.4f)]
				public readonly Sync<float> eyesToNeckPitchRate;
				[Default(60.0f)]
				public readonly Sync<float> headLimitYaw;
				[Default(15.0f)]
				public readonly Sync<float> headLimitPitchUp;
				[Default(15.0f)]
				public readonly Sync<float> headLimitPitchDown;
				[Default(5.0f)]
				public readonly Sync<float> headLimitRoll;
				[Default(0.8f)]
				public readonly Sync<float> eyesToHeadYawRate;
				[Default(0.5f)]
				public readonly Sync<float> eyesToHeadPitchRate;
				[Default(110.0f)]
				public readonly Sync<float> eyesTraceAngle;
				[Default(40.0f)]
				public readonly Sync<float> eyesLimitYaw;
				[Default(12.0f)]
				public readonly Sync<float> eyesLimitPitch;
				[Default(0.796f)]
				public readonly Sync<float> eyesYawRate;
				[Default(0.729f)]
				public readonly Sync<float> eyesPitchRate;
				[Default(0.356f)]
				public readonly Sync<float> eyesYawOuterRate;
				[Default(0.212f)]
				public readonly Sync<float> eyesYawInnerRate;
			}

			public sealed partial class FingerIK : SyncObject
			{
			}

			public readonly BodyIK bodyIK;
			public readonly LimbIK limbIK;
			public readonly HeadIK headIK;
			public readonly FingerIK fingerIK;
		}

		public sealed partial class EditorSettings : SyncObject
		{
			public readonly Sync<bool> isAdvanced;
			public readonly Sync<int> toolbarSelected;
			public readonly Sync<bool> isShowEffectorTransform;
		}

		// Memo: Not Serializable
		public class InternalValues
		{
			public bool animatorEnabled;
			public bool resetTransforms;
			public bool continuousSolverEnabled;
			public int shoulderDirYAsNeck = -1;

			public Vector3f defaultRootPosition = Vector3f.Zero;
			public IKMatrix3x3 defaultRootBasis = IKMatrix3x3.identity;
			public IKMatrix3x3 defaultRootBasisInv = IKMatrix3x3.identity;
			public Quaternionf defaultRootRotation = Quaternionf.Identity;

			// Using by resetTransforms & continuousSolverEnabled.
			public Vector3f baseHipsPos = Vector3f.Zero;
			public IKMatrix3x3 baseHipsBasis = IKMatrix3x3.identity;

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
						shoulderLimitThetaYPlus.Reset(settingsBodyIK.shoulderLimitAngleYPlus);
					}
					if (shoulderLimitThetaYMinus._degrees != settingsBodyIK.shoulderLimitAngleYMinus) {
						shoulderLimitThetaYMinus.Reset(settingsBodyIK.shoulderLimitAngleYMinus);
					}
					if (shoulderLimitThetaZ._degrees != settingsBodyIK.shoulderLimitAngleZ) {
						shoulderLimitThetaZ.Reset(settingsBodyIK.shoulderLimitAngleZ);
					}

					if (upperCenterLegTranslateRate._value != settingsBodyIK.upperCenterLegTranslateRate ||
						upperSpineTranslateRate._value != settingsBodyIK.upperSpineTranslateRate) {
						upperCenterLegTranslateRate.Reset(settingsBodyIK.upperCenterLegTranslateRate);
						upperSpineTranslateRate.Reset(MathF.Max(settingsBodyIK.upperCenterLegTranslateRate, settingsBodyIK.upperSpineTranslateRate));
					}

					if (upperPostTranslateRate._value != settingsBodyIK.upperPostTranslateRate) {
						upperPostTranslateRate.Reset(settingsBodyIK.upperPostTranslateRate);
					}

					if (upperCenterLegRotateRate._value != settingsBodyIK.upperCenterLegRotateRate ||
						upperSpineRotateRate._value != settingsBodyIK.upperSpineRotateRate) {
						upperCenterLegRotateRate.Reset(settingsBodyIK.upperCenterLegRotateRate);
						upperSpineRotateRate.Reset(MathF.Max(settingsBodyIK.upperCenterLegRotateRate, settingsBodyIK.upperSpineRotateRate));
						isFuzzyUpperCenterLegAndSpineRotationRate = IKMath.IsFuzzy(upperCenterLegRotateRate.value, upperSpineRotateRate.value);
					}

					if (upperEyesLimitYaw._degrees != settingsBodyIK.upperEyesLimitYaw) {
						upperEyesLimitYaw.Reset(settingsBodyIK.upperEyesLimitYaw);
					}
					if (upperEyesLimitPitchUp._degrees != settingsBodyIK.upperEyesLimitPitchUp) {
						upperEyesLimitPitchUp.Reset(settingsBodyIK.upperEyesLimitPitchUp);
					}
					if (upperEyesLimitPitchDown._degrees != settingsBodyIK.upperEyesLimitPitchDown) {
						upperEyesLimitPitchDown.Reset(settingsBodyIK.upperEyesLimitPitchDown);
					}
					if (upperEyesTraceTheta._degrees != settingsBodyIK.upperEyesTraceAngle) {
						upperEyesTraceTheta.Reset(settingsBodyIK.upperEyesTraceAngle);
					}

					if (spineLimitAngleX._a != settingsBodyIK.spineLimitAngleX) {
						spineLimitAngleX.Reset(settingsBodyIK.spineLimitAngleX, MathUtil.DEG_2_RADF);
					}
					if (spineLimitAngleY._a != settingsBodyIK.spineLimitAngleY) {
						spineLimitAngleY.Reset(settingsBodyIK.spineLimitAngleY, MathUtil.DEG_2_RADF);
					}
					if (upperDirXLimitThetaY._degrees != settingsBodyIK.upperDirXLimitAngleY) {
						upperDirXLimitThetaY.Reset(settingsBodyIK.upperDirXLimitAngleY);
					}

					if (upperContinuousPreTranslateRate._value != settingsBodyIK.upperContinuousPreTranslateRate) {
						upperContinuousPreTranslateRate.Reset(settingsBodyIK.upperContinuousPreTranslateRate);
					}
					if (upperContinuousPreTranslateStableRate._value != settingsBodyIK.upperContinuousPreTranslateStableRate) {
						upperContinuousPreTranslateStableRate.Reset(settingsBodyIK.upperContinuousPreTranslateStableRate);
					}
					if (upperContinuousCenterLegRotationStableRate._value != settingsBodyIK.upperContinuousCenterLegRotationStableRate) {
						upperContinuousCenterLegRotationStableRate.Reset(settingsBodyIK.upperContinuousCenterLegRotationStableRate);
					}
					if (upperContinuousPostTranslateStableRate._value != settingsBodyIK.upperContinuousPostTranslateStableRate) {
						upperContinuousPostTranslateStableRate.Reset(settingsBodyIK.upperContinuousPostTranslateStableRate);
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
						armEffectorBackBeginTheta.Reset(settingsLimbIK.armEffectorBackBeginAngle);
					}
					if (armEffectorBackCoreBeginTheta._degrees != settingsLimbIK.armEffectorBackCoreBeginAngle) {
						armEffectorBackCoreBeginTheta.Reset(settingsLimbIK.armEffectorBackCoreBeginAngle);
					}
					if (armEffectorBackCoreEndTheta._degrees != settingsLimbIK.armEffectorBackCoreEndAngle) {
						armEffectorBackCoreEndTheta.Reset(settingsLimbIK.armEffectorBackCoreEndAngle);
					}
					if (armEffectorBackEndTheta._degrees != settingsLimbIK.armEffectorBackEndAngle) {
						armEffectorBackEndTheta.Reset(settingsLimbIK.armEffectorBackEndAngle);
					}

					if (armEffectorBackCoreUpperTheta._degrees != settingsLimbIK.armEffectorBackCoreUpperAngle) {
						armEffectorBackCoreUpperTheta.Reset(settingsLimbIK.armEffectorBackCoreUpperAngle);
					}
					if (armEffectorBackCoreLowerTheta._degrees != settingsLimbIK.armEffectorBackCoreLowerAngle) {
						armEffectorBackCoreLowerTheta.Reset(settingsLimbIK.armEffectorBackCoreLowerAngle);
					}

					if (elbowFrontInnerLimitTheta._degrees != settingsLimbIK.elbowFrontInnerLimitAngle) {
						elbowFrontInnerLimitTheta.Reset(settingsLimbIK.elbowFrontInnerLimitAngle);
					}
					if (elbowBackInnerLimitTheta._degrees != settingsLimbIK.elbowBackInnerLimitAngle) {
						elbowBackInnerLimitTheta.Reset(settingsLimbIK.elbowBackInnerLimitAngle);
					}

					if (footLimitYawTheta._degrees != settingsLimbIK.footLimitYaw) {
						footLimitYawTheta.Reset(settingsLimbIK.footLimitYaw);
					}
					if (footLimitPitchUpTheta._degrees != settingsLimbIK.footLimitPitchUp) {
						footLimitPitchUpTheta.Reset(settingsLimbIK.footLimitPitchUp);
					}
					if (footLimitPitchDownTheta._degrees != settingsLimbIK.footLimitPitchDown) {
						footLimitPitchDownTheta.Reset(settingsLimbIK.footLimitPitchDown);
					}
					if (footLimitRollTheta._degrees != settingsLimbIK.footLimitRoll) {
						footLimitRollTheta.Reset(settingsLimbIK.footLimitRoll);
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
						neckLimitPitchUpTheta.Reset(settingsHeadIK.neckLimitPitchUp);
					}
					if (neckLimitPitchDownTheta._degrees != settingsHeadIK.neckLimitPitchDown) {
						neckLimitPitchDownTheta.Reset(settingsHeadIK.neckLimitPitchDown);
					}
					if (neckLimitRollTheta._degrees != settingsHeadIK.neckLimitRoll) {
						neckLimitRollTheta.Reset(settingsHeadIK.neckLimitRoll);
					}

					if (headLimitYawTheta._degrees != settingsHeadIK.headLimitYaw) {
						headLimitYawTheta.Reset(settingsHeadIK.headLimitYaw);
					}
					if (headLimitPitchUpTheta._degrees != settingsHeadIK.headLimitPitchUp) {
						headLimitPitchUpTheta.Reset(settingsHeadIK.headLimitPitchUp);
					}
					if (headLimitPitchDownTheta._degrees != settingsHeadIK.headLimitPitchDown) {
						headLimitPitchDownTheta.Reset(settingsHeadIK.headLimitPitchDown);
					}
					if (headLimitRollTheta._degrees != settingsHeadIK.headLimitRoll) {
						headLimitRollTheta.Reset(settingsHeadIK.headLimitRoll);
					}

					if (eyesTraceTheta._degrees != settingsHeadIK.eyesTraceAngle) {
						eyesTraceTheta.Reset(settingsHeadIK.eyesTraceAngle);
					}

					if (eyesLimitYawTheta._degrees != settingsHeadIK.eyesLimitYaw) {
						eyesLimitYawTheta.Reset(settingsHeadIK.eyesLimitYaw);
					}
					if (eyesLimitPitchTheta._degrees != settingsHeadIK.eyesLimitPitch) {
						eyesLimitPitchTheta.Reset(settingsHeadIK.eyesLimitPitch);
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

					IKMath.MatMultVec(out hipsToFootLength[index].hipsToLeg, internalValues.defaultRootBasisInv, hipsToLegDir);
					IKMath.MatMultVec(out hipsToFootLength[index].legToKnee, internalValues.defaultRootBasisInv, legToKneeDir);
					IKMath.MatMultVec(out hipsToFootLength[index].kneeToFoot, internalValues.defaultRootBasisInv, kneeToFootDir);

					hipsToFootLength[index].defaultOffset =
						hipsToFootLength[index].hipsToLeg * hipsToLegLen +
						hipsToFootLength[index].legToKnee * legToKneeLen +
						hipsToFootLength[index].kneeToFoot * kneeToFootLen;
				}
			}

			Vector3f GetHipsOffset(int index, Bone legBone, Bone kneeBone, Bone footBone) {
				if (legBone != null && kneeBone != null && footBone != null) {
					var hipsToLegLen = legBone._defaultLocalLength.length;
					var legToKneeLen = kneeBone._defaultLocalLength.length;
					var kneeToFootLen = footBone._defaultLocalLength.length;

					var currentOffset =
						hipsToFootLength[index].hipsToLeg * hipsToLegLen +
						hipsToFootLength[index].legToKnee * legToKneeLen +
						hipsToFootLength[index].kneeToFoot * kneeToFootLen;

					return currentOffset - hipsToFootLength[index].defaultOffset;
				}

				return Vector3f.Zero;
			}

			public Vector3f defaultHipsPosition = Vector3f.Zero;
			public Vector3f hipsOffset = Vector3f.Zero;

			public void Prepare(HumanoidIK fullBodyIK) {
				PrepareHipsToFootLength(0, fullBodyIK.leftLegBones.leg, fullBodyIK.leftLegBones.knee, fullBodyIK.leftLegBones.foot, fullBodyIK.internalValues);
				PrepareHipsToFootLength(1, fullBodyIK.rightLegBones.leg, fullBodyIK.rightLegBones.knee, fullBodyIK.rightLegBones.foot, fullBodyIK.internalValues);
				if (fullBodyIK.bodyBones.hips != null) {
					defaultHipsPosition = fullBodyIK.bodyBones.hips._defaultPosition;
				}
			}

			public void SyncDisplacement(HumanoidIK fullBodyIK) {
				Assert(fullBodyIK != null);

				var hipsOffset0 = GetHipsOffset(0, fullBodyIK.leftLegBones.leg, fullBodyIK.leftLegBones.knee, fullBodyIK.leftLegBones.foot);
				var hipsOffset1 = GetHipsOffset(1, fullBodyIK.rightLegBones.leg, fullBodyIK.rightLegBones.knee, fullBodyIK.rightLegBones.foot);
				hipsOffset = (hipsOffset0 + hipsOffset1) * 0.5f;
			}
		}



		public readonly SyncRef<Entity> rootTransform;

		public InternalValues internalValues = new();
		public BoneCaches boneCaches = new();

		public readonly Settings settings;
		public readonly EditorSettings editorSettings;

		public readonly BodyBones bodyBones;
		public readonly HeadBones headBones;
		public readonly LegBones leftLegBones;
		public readonly LegBones rightLegBones;
		public readonly ArmBones leftArmBones;
		public readonly ArmBones rightArmBones;
		public readonly FingersBones leftHandFingersBones;
		public readonly FingersBones rightHandFingersBones;

		public readonly Effector rootEffector;
		public readonly BodyEffectors bodyEffectors;
		public readonly HeadEffectors headEffectors;
		public readonly LegEffectors leftLegEffectors;
		public readonly LegEffectors rightLegEffectors;
		public readonly ArmEffectors leftArmEffectors;
		public readonly ArmEffectors rightArmEffectors;
		public readonly FingersEffectors leftHandFingersEffectors;
		public readonly FingersEffectors rightHandFingersEffectors;

		public Bone[] Bones => _bones;
		public Effector[] Effectors => _effectors;

		Bone[] _bones = new Bone[(int)BoneType.Max];
		Effector[] _effectors = new Effector[(int)EffectorLocation.Max];

		BodyIK _bodyIK;
		LimbIK[] _limbIK = new LimbIK[(int)LimbIKLocation.Max];
		HeadIK _headIK;
		FingerIK[] _fingerIK = new FingerIK[(int)FingerIKType.Max];

		bool _isNeedFixShoulderWorldTransform;

		bool _isPrefixed;
		bool _isPrepared;

		public readonly Sync<bool> _isPrefixedAtLeastOnce;

		public void Awake(Entity rootTransorm_) {
			if (rootTransform.Target != rootTransorm_) {
				rootTransform.Target = rootTransorm_;
			}

#if IK_DEBUG
			var constructBeginTime = Time.realtimeSinceStartup;
#endif
			Prefix();
#if IK_DEBUG
			var prefixEndTime = Time.realtimeSinceStartup;
#endif
			ConfigureBoneTransforms();
#if IK_DEBUG
			var configureBoneEndTime = Time.realtimeSinceStartup;
#endif
			Prepare();
#if IK_DEBUG
			var prefetchEndTime = Time.realtimeSinceStartup;
			Debug.Log("Total time: " + (prefetchEndTime - constructBeginTime) + " _Prefix():" + (prefixEndTime - constructBeginTime) + " ConfigureBoneTransforms():" + (configureBoneEndTime - prefixEndTime) + " Prefetch():" + (prefetchEndTime - configureBoneEndTime));
#endif
		}

		static void SetBoneTransform(Bone bone, Entity transform) {
			bone.transform.Target = transform;
		}

		static void SetFingerBoneTransform(ref Bone[] bones, Entity[,] transforms, int index) {
			if (bones == null || bones.Length != MAX_HAND_FINGER_LENGTH) {
				bones = new Bone[MAX_HAND_FINGER_LENGTH];
			}

			for (var i = 0; i != MAX_HAND_FINGER_LENGTH; ++i) {
				if (bones[i] == null) {
					bones[i] = new Bone();
				}
				bones[i].transform.Target = transforms[index, i];
			}
		}

		static bool IsSpine(Entity trn) {
			if (trn != null) {
				var name = trn.name.Value;
				if (name.Contains("Spine") || name.Contains("spine") || name.Contains("SPINE")) {
					return true;
				}
				if (name.Contains("Torso") || name.Contains("torso") || name.Contains("TORSO")) {
					return true;
				}
			}

			return false;
		}

		static bool IsNeck(Entity trn) {
			if (trn != null) {
				var name = trn.name.Value;
				if (name != null) {
					if (name.Contains("Neck") || name.Contains("neck") || name.Contains("NECK")) {
						return true;
					}
					if (name.Contains("Kubi") || name.Contains("kubi") || name.Contains("KUBI")) {
						return true;
					}
					if (name.Contains("\u304F\u3073")) { // Kubi(Hira-gana)
						return true;
					}
					if (name.Contains("\u30AF\u30D3")) { // Kubi(Kana-kana)
						return true;
					}
					if (name.Contains("\u9996")) { // Kubi(Kanji)
						return true;
					}
				}
			}

			return false;
		}

		// - Call from Editor script.
		public void Prefix(Entity rootTransform_) {
			if (rootTransform.Target != rootTransform_) {
				rootTransform.Target = rootTransform_;
			}

			Prefix();
		}

		// - Call from FullBodyIKBehaviour.Awake() / FullBodyIK.Initialize().
		// - Bone transforms are null yet.
		void Prefix() {
			if (_isPrefixed) {
				return;
			}

			_isPrefixed = true;

			if (_bones == null || _bones.Length != (int)BoneLocation.Max) {
				_bones = new Bone[(int)BoneLocation.Max];
			}
			if (_effectors == null || _effectors.Length != (int)EffectorLocation.Max) {
				_effectors = new Effector[(int)EffectorLocation.Max];
			}

			Prefix(bodyBones.hips, BoneLocation.Hips, null);
			Prefix(bodyBones.spine, BoneLocation.Spine, bodyBones.hips);
			Prefix(bodyBones.spine2, BoneLocation.Spine2, bodyBones.spine);
			Prefix(bodyBones.spine3, BoneLocation.Spine3, bodyBones.spine2);
			Prefix(bodyBones.spine4, BoneLocation.Spine4, bodyBones.spine3);
			Prefix(headBones.neck, BoneLocation.Neck, bodyBones.SpineU);
			Prefix(headBones.head, BoneLocation.Head, headBones.neck);
			Prefix(headBones.leftEye, BoneLocation.LeftEye, headBones.head);
			Prefix(headBones.rightEye, BoneLocation.RightEye, headBones.head);
			for (var i = 0; i != 2; ++i) {
				var legBones = i == 0 ? leftLegBones : rightLegBones;
				Prefix(legBones.leg, i == 0 ? BoneLocation.LeftLeg : BoneLocation.RightLeg, bodyBones.hips);
				Prefix(legBones.knee, i == 0 ? BoneLocation.LeftKnee : BoneLocation.RightKnee, legBones.leg);
				Prefix(legBones.foot, i == 0 ? BoneLocation.LeftFoot : BoneLocation.RightFoot, legBones.knee);

				var armBones = i == 0 ? leftArmBones : rightArmBones;
				Prefix(armBones.shoulder, i == 0 ? BoneLocation.LeftShoulder : BoneLocation.RightShoulder, bodyBones.SpineU);
				Prefix(armBones.arm, i == 0 ? BoneLocation.LeftArm : BoneLocation.RightArm, armBones.shoulder);
				Prefix(armBones.elbow, i == 0 ? BoneLocation.LeftElbow : BoneLocation.RightElbow, armBones.arm);
				Prefix(armBones.wrist, i == 0 ? BoneLocation.LeftWrist : BoneLocation.RightWrist, armBones.elbow);

				for (var n = 0; n != MAX_ARM_ROLL_LENGTH; ++n) {
					var armRollLocation = i == 0 ? BoneLocation.LeftArmRoll0 : BoneLocation.RightArmRoll0;
					Prefix(armBones.armRoll[n], (BoneLocation)((int)armRollLocation + n), armBones.arm);
				}

				for (var n = 0; n != MAX_ELBOW_ROLL_LENGTH; ++n) {
					var elbowRollLocation = i == 0 ? BoneLocation.LeftElbowRoll0 : BoneLocation.RightElbowRoll0;
					Prefix(armBones.elbowRoll[n], (BoneLocation)((int)elbowRollLocation + n), armBones.elbow);
				}

				var fingerBones = i == 0 ? leftHandFingersBones : rightHandFingersBones;
				for (var n = 0; n != MAX_HAND_FINGER_LENGTH; ++n) {
					var thumbLocation = i == 0 ? BoneLocation.LeftHandThumb0 : BoneLocation.RightHandThumb0;
					var indexLocation = i == 0 ? BoneLocation.LeftHandIndex0 : BoneLocation.RightHandIndex0;
					var middleLocation = i == 0 ? BoneLocation.LeftHandMiddle0 : BoneLocation.RightHandMiddle0;
					var ringLocation = i == 0 ? BoneLocation.LeftHandRing0 : BoneLocation.RightHandRing0;
					var littleLocation = i == 0 ? BoneLocation.LeftHandLittle0 : BoneLocation.RightHandLittle0;
					Prefix(fingerBones.thumb[n], (BoneLocation)((int)thumbLocation + n), n == 0 ? armBones.wrist : fingerBones.thumb[n - 1]);
					Prefix(fingerBones.index[n], (BoneLocation)((int)indexLocation + n), n == 0 ? armBones.wrist : fingerBones.index[n - 1]);
					Prefix(fingerBones.middle[n], (BoneLocation)((int)middleLocation + n), n == 0 ? armBones.wrist : fingerBones.middle[n - 1]);
					Prefix(fingerBones.ring[n], (BoneLocation)((int)ringLocation + n), n == 0 ? armBones.wrist : fingerBones.ring[n - 1]);
					Prefix(fingerBones.little[n], (BoneLocation)((int)littleLocation + n), n == 0 ? armBones.wrist : fingerBones.little[n - 1]);
				}
			}

			Prefix(rootEffector, EffectorLocation.Root);
			Prefix(bodyEffectors.hips, EffectorLocation.Hips, rootEffector, bodyBones.hips, leftLegBones.leg, rightLegBones.leg);
			Prefix(headEffectors.neck, EffectorLocation.Neck, bodyEffectors.hips, headBones.neck);
			Prefix(headEffectors.head, EffectorLocation.Head, headEffectors.neck, headBones.head);
			Prefix(headEffectors.eyes, EffectorLocation.Eyes, rootEffector, headBones.head, headBones.leftEye, headBones.rightEye);

			Prefix(leftLegEffectors.knee, EffectorLocation.LeftKnee, rootEffector, leftLegBones.knee);
			Prefix(leftLegEffectors.foot, EffectorLocation.LeftFoot, rootEffector, leftLegBones.foot);
			Prefix(rightLegEffectors.knee, EffectorLocation.RightKnee, rootEffector, rightLegBones.knee);
			Prefix(rightLegEffectors.foot, EffectorLocation.RightFoot, rootEffector, rightLegBones.foot);

			Prefix(leftArmEffectors.arm, EffectorLocation.LeftArm, bodyEffectors.hips, leftArmBones.arm);
			Prefix(leftArmEffectors.elbow, EffectorLocation.LeftElbow, bodyEffectors.hips, leftArmBones.elbow);
			Prefix(leftArmEffectors.wrist, EffectorLocation.LeftWrist, bodyEffectors.hips, leftArmBones.wrist);
			Prefix(rightArmEffectors.arm, EffectorLocation.RightArm, bodyEffectors.hips, rightArmBones.arm);
			Prefix(rightArmEffectors.elbow, EffectorLocation.RightElbow, bodyEffectors.hips, rightArmBones.elbow);
			Prefix(rightArmEffectors.wrist, EffectorLocation.RightWrist, bodyEffectors.hips, rightArmBones.wrist);

			Prefix(leftHandFingersEffectors.thumb, EffectorLocation.LeftHandThumb, leftArmEffectors.wrist, leftHandFingersBones.thumb);
			Prefix(leftHandFingersEffectors.index, EffectorLocation.LeftHandIndex, leftArmEffectors.wrist, leftHandFingersBones.index);
			Prefix(leftHandFingersEffectors.middle, EffectorLocation.LeftHandMiddle, leftArmEffectors.wrist, leftHandFingersBones.middle);
			Prefix(leftHandFingersEffectors.ring, EffectorLocation.LeftHandRing, leftArmEffectors.wrist, leftHandFingersBones.ring);
			Prefix(leftHandFingersEffectors.little, EffectorLocation.LeftHandLittle, leftArmEffectors.wrist, leftHandFingersBones.little);

			Prefix(rightHandFingersEffectors.thumb, EffectorLocation.RightHandThumb, rightArmEffectors.wrist, rightHandFingersBones.thumb);
			Prefix(rightHandFingersEffectors.index, EffectorLocation.RightHandIndex, rightArmEffectors.wrist, rightHandFingersBones.index);
			Prefix(rightHandFingersEffectors.middle, EffectorLocation.RightHandMiddle, rightArmEffectors.wrist, rightHandFingersBones.middle);
			Prefix(rightHandFingersEffectors.ring, EffectorLocation.RightHandRing, rightArmEffectors.wrist, rightHandFingersBones.ring);
			Prefix(rightHandFingersEffectors.little, EffectorLocation.RightHandLittle, rightArmEffectors.wrist, rightHandFingersBones.little);

			if (!_isPrefixedAtLeastOnce.Value) {
				_isPrefixedAtLeastOnce.Value = true;
				for (var i = 0; i != _effectors.Length; ++i) {
					_effectors[i].Prefix();
				}
			}
		}

		static readonly string[] _leftKeywords = new string[]
		{
			"left",
			"_l",
		};

		static readonly string[] _rightKeywords = new string[]
		{
			"right",
			"_r",
		};

		static Entity FindEye(Entity head, bool isRight) {
			if (head != null) {
				var keywords = isRight ? _rightKeywords : _leftKeywords;

				var childCount = head.children.Count;
				for (var i = 0; i < childCount; ++i) {
					var child = head.children[i];
					if (child != null) {
						var name = child.name.Value;
						if (name != null) {
							name = name.ToLower();
							if (name != null && name.Contains("eye")) {
								for (var n = 0; n < keywords.Length; ++n) {
									if (name.Contains(keywords[n])) {
										return child;
									}
								}
							}
						}
					}
				}
			}

			return null;
		}

		public void ConfigureBoneTransforms() {
			Prefix();

			Assert(settings != null && rootTransform != null);
			if (settings.automaticPrepareHumanoid && rootTransform != null) {
				//Todo find bones
			}

			if (settings.automaticConfigureRollBonesEnabled) {
				var tempBones = new List<Entity>();

				for (var side = 0; side != 2; ++side) {
					var armBones = side == 0 ? leftArmBones : rightArmBones;
					if (armBones != null &&
						armBones.arm != null && armBones.arm.transform != null &&
						armBones.elbow != null && armBones.elbow.transform != null &&
						armBones.wrist != null && armBones.wrist.transform != null) {

						ConfigureRollBones(armBones.armRoll, tempBones, armBones.arm.transform.Target, armBones.elbow.transform.Target, (Side)side, true);
						ConfigureRollBones(armBones.elbowRoll, tempBones, armBones.elbow.transform.Target, armBones.wrist.transform.Target, (Side)side, false);
					}
				}
			}
		}

		void ConfigureRollBones(IArray<Bone> bones, List<Entity> tempBones, Entity transform, Entity excludeTransform, Side side, bool isArm) {
			var isRollSpecial = false;
			var rollSpecialName = isArm ? side == Side.Left ? "LeftArmRoll" : "RightArmRoll" : side == Side.Left ? "LeftElbowRoll" : "RightElbowRoll";

			var childCount = transform.children.Count;

			for (var i = 0; i != childCount; ++i) {
				var childTransform = transform.children[i];
				var name = childTransform.name.Value;
				if (name != null && name.Contains(rollSpecialName)) {
					isRollSpecial = true;
					break;
				}
			}

			tempBones.Clear();

			for (var i = 0; i != childCount; ++i) {
				var childTransform = transform.children[i];
				var name = childTransform.name.Value;
				if (name != null) {
					if (excludeTransform != childTransform &&
						!excludeTransform.IsChildOf(childTransform)) {
						if (isRollSpecial) {
							if (name.Contains(rollSpecialName)) {
								var nameEnd = name[name.Length - 1];
								if (nameEnd is >= '0' and <= '9') {
									tempBones.Add(childTransform);
								}
							}
						}
						else {
							tempBones.Add(childTransform);
						}
					}
				}
			}

			childCount = Math.Min(tempBones.Count, bones.Length);
			for (var i = 0; i != childCount; ++i) {
				SetBoneTransform(bones[i], tempBones[i]);
			}
		}

		// - Wakeup for solvers.
		// - Require to setup each transforms.
		public bool Prepare() {
			Prefix();

			if (_isPrepared) {
				return false;
			}

			_isPrepared = true;

			Assert(rootTransform != null);
			if (rootTransform != null) { // Failsafe.
				internalValues.defaultRootPosition = rootTransform.Target.GlobalTrans.Translation;
				//internalValues.defaultRootBasis = IKMatrix3x3.FromColumn(rootTransform.right, rootTransform.up, rootTransform.forward); Todo Add Face values
				internalValues.defaultRootBasisInv = internalValues.defaultRootBasis.Transpose;
				internalValues.defaultRootRotation = rootTransform.Target.GlobalTrans.Rotation;
			}

			if (_bones != null) {
				var boneLength = _bones.Length;
				for (var i = 0; i != boneLength; ++i) {
					Assert(_bones[i] != null);
					_bones[i]?.Prepare(this);
				}
				for (var i = 0; i != boneLength; ++i) {
					_bones[i]?.PostPrepare();
				}
			}

			boneCaches.Prepare(this);

			if (_effectors != null) {
				var effectorLength = _effectors.Length;
				for (var i = 0; i != effectorLength; ++i) {
					Assert(_effectors[i] != null);
					_effectors[i]?.Prepare(this);
				}
			}

			if (_limbIK == null || _limbIK.Length != (int)LimbIKLocation.Max) {
				_limbIK = new LimbIK[(int)LimbIKLocation.Max];
			}

			for (var i = 0; i != (int)LimbIKLocation.Max; ++i) {
				_limbIK[i] = new LimbIK(this, (LimbIKLocation)i);
			}

			_bodyIK = new BodyIK(this, _limbIK);
			_headIK = new HeadIK(this);

			if (_fingerIK == null || _fingerIK.Length != (int)FingerIKType.Max) {
				_fingerIK = new FingerIK[(int)FingerIKType.Max];
			}

			for (var i = 0; i != (int)FingerIKType.Max; ++i) {
				_fingerIK[i] = new FingerIK(this, (FingerIKType)i);
			}

			{
				var neckBone = headBones.neck;
				var leftShoulder = leftArmBones.shoulder;
				var rightShoulder = rightArmBones.shoulder;
				if (leftShoulder != null && leftShoulder.TransformIsAlive &&
					rightShoulder != null && rightShoulder.TransformIsAlive &&
					neckBone != null && neckBone.TransformIsAlive) {
					if (leftShoulder.transform.Target.parent.Target == neckBone.transform.Target &&
						rightShoulder.transform.Target.parent.Target == neckBone.transform.Target) {
						_isNeedFixShoulderWorldTransform = true;
					}
				}
			}

			return true;
		}

		bool _isAnimatorCheckedAtLeastOnce = false;

		void UpdateInternalValues() {
			// _animatorEnabledImmediately
			if (settings.animatorEnabled == AutomaticBool.Auto) {
				if (!_isAnimatorCheckedAtLeastOnce) {
					_isAnimatorCheckedAtLeastOnce = true;
					internalValues.animatorEnabled = false;
					if (rootTransform != null) {
						//Todo ANIM
					}
				}
			}
			else {
				internalValues.animatorEnabled = settings.animatorEnabled != AutomaticBool.Disable;
				_isAnimatorCheckedAtLeastOnce = false;
			}

			internalValues.resetTransforms = settings.resetTransforms == AutomaticBool.Auto
				? !internalValues.animatorEnabled
				: settings.resetTransforms != AutomaticBool.Disable;

			internalValues.continuousSolverEnabled = !internalValues.animatorEnabled && !internalValues.resetTransforms;

			internalValues.bodyIK.Update(settings.bodyIK);
			internalValues.limbIK.Update(settings.limbIK);
			internalValues.headIK.Update(settings.headIK);
		}

		bool _isSyncDisplacementAtLeastOnce = false;

		void Bones_SyncDisplacement() {
			// Sync Displacement.
			if (settings.syncDisplacement != SyncDisplacement.Disable) {
				if (settings.syncDisplacement == SyncDisplacement.Everyframe || !_isSyncDisplacementAtLeastOnce) {
					_isSyncDisplacementAtLeastOnce = true;

					if (_bones != null) {
						var boneLength = _bones.Length;
						for (var i = 0; i != boneLength; ++i) {
							_bones[i]?.SyncDisplacement();
						}

						// for Hips
						boneCaches.SyncDisplacement(this);

						for (var i = 0; i != boneLength; ++i) {
							_bones[i]?.PostSyncDisplacement(this);
						}

						for (var i = 0; i != boneLength; ++i) {
							_bones[i]?.PostPrepare();
						}
					}

					// Forceupdate _defaultPosition / _defaultRotation
					if (_effectors != null) {
						var effectorLength = _effectors.Length;
						for (var i = 0; i != effectorLength; ++i) {
							_effectors[i]?.ComputeDefaultTransform(this);
						}
					}
				}
			}
		}

		// for effector._hidden_worldPosition / BodyIK
		void ComputeBaseHipsTransform() {
			Assert(internalValues != null);

			if (bodyEffectors == null) { // Note: bodyEffectors is public.
				return;
			}

			var hipsEffector = bodyEffectors.hips;
			if (hipsEffector == null || rootEffector == null) {
				return;
			}

			if (hipsEffector.rotationEnabled && hipsEffector.rotationWeight > IKMath.IK_EPSILON) {
				var hipsRotation = hipsEffector.WorldRotation * IKMath.Inverse(hipsEffector._defaultRotation);
				if (hipsEffector.rotationWeight < 1.0f - IKMath.IK_EPSILON) {
					var rootRotation = rootEffector.WorldRotation * IKMath.Inverse(rootEffector._defaultRotation);
					var tempRotation = Quaternionf.Slerp(rootRotation, hipsRotation, hipsEffector.rotationWeight);
					IKMath.MatSetRot(out internalValues.baseHipsBasis, ref tempRotation);
				}
				else {
					IKMath.MatSetRot(out internalValues.baseHipsBasis, ref hipsRotation);
				}
			}
			else {
				var rootEffectorWorldRotation = rootEffector.WorldRotation;
				IKMath.MatSetRotMultInv1(out internalValues.baseHipsBasis, rootEffectorWorldRotation, rootEffector._defaultRotation);
			}

			if (hipsEffector.positionEnabled && hipsEffector.positionWeight > IKMath.IK_EPSILON) {
				var hipsEffectorWorldPosition = hipsEffector.WorldPosition;
				internalValues.baseHipsPos = hipsEffectorWorldPosition;
				if (hipsEffector.positionWeight < 1.0f - IKMath.IK_EPSILON) {
					var rootEffectorWorldPosition = rootEffector.WorldPosition;
					IKMath.MatMultVecPreSubAdd(
						out var hipsPosition,
						 internalValues.baseHipsBasis,
						 hipsEffector._defaultPosition,
						 rootEffector._defaultPosition,
						 rootEffectorWorldPosition);
					internalValues.baseHipsPos = Vector3f.Lerp(hipsPosition, internalValues.baseHipsPos, hipsEffector.positionWeight);
				}
			}
			else {
				var rootEffectorWorldPosition = rootEffector.WorldPosition;
				IKMath.MatMultVecPreSubAdd(
					out internalValues.baseHipsPos,
					internalValues.baseHipsBasis,
					hipsEffector._defaultPosition,
					rootEffector._defaultPosition,
					rootEffectorWorldPosition);
			}
		}

		public void Update() {
			UpdateInternalValues();

			if (_effectors != null) {
				var effectorLength = _effectors.Length;
				for (var i = 0; i != effectorLength; ++i) {
					_effectors[i]?.PrepareUpdate();
				}
			}

			Bones_PrepareUpdate();

			Bones_SyncDisplacement();

			if (internalValues.resetTransforms || internalValues.continuousSolverEnabled) {
				ComputeBaseHipsTransform();
			}

			// Feedback bonePositions to effectorPositions.
			// (for AnimatorEnabled only.)
			if (_effectors != null) {
				var effectorLength = _effectors.Length;
				for (var i = 0; i != effectorLength; ++i) {
					var effector = _effectors[i];
					if (effector != null) {
						// todo: Optimize. (for BodyIK)

						// LimbIK : bending / end
						// BodyIK :  wrist / foot / neck
						// FingerIK : nothing
						if (effector.EffectorType is EffectorType.Eyes or EffectorType.HandFinger) { // Optimize.
#if IK_DEBUG
							effector._hidden_worldPosition = new Vector3();
#endif
						}
						else {
							var weight = effector.positionEnabled ? effector.positionWeight : 0.0f;
							var destPosition = weight > IKMath.IK_EPSILON ? effector.WorldPosition : new Vector3f();
							if (weight < 1.0f - IKMath.IK_EPSILON) {
								var sourcePosition = destPosition; // Failsafe.
								if (!internalValues.animatorEnabled && (internalValues.resetTransforms || internalValues.continuousSolverEnabled)) {
									if (effector.EffectorLocation == EffectorLocation.Hips) {
										sourcePosition = internalValues.baseHipsPos; // _ComputeBaseHipsTransform()
									}
									else {
										var hipsEffector = bodyEffectors?.hips;
										if (hipsEffector != null) {
											IKMath.MatMultVecPreSubAdd(
												out sourcePosition,
												 internalValues.baseHipsBasis,
												 effector._defaultPosition,
												 hipsEffector._defaultPosition,
												 internalValues.baseHipsPos);
										}
									}
								}
								else { // for Animation.
									if (effector.Bone != null && effector.Bone.TransformIsAlive) {
										sourcePosition = effector.Bone.WorldPosition;
									}
								}

								effector._hidden_worldPosition = weight > IKMath.IK_EPSILON ? Vector3f.Lerp(sourcePosition, destPosition, weight) : sourcePosition;
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
				var limbIKLength = _limbIK.Length;
				for (var i = 0; i != limbIKLength; ++i) {
					_limbIK[i]?.PresolveBeinding();
				}
			}

			if (_bodyIK != null) {
				if (_bodyIK.Solve()) {
					Bones_WriteToTransform();
				}
			}

			// todo: Force overwrite _hidden_worldPosition (LimbIK, arms)

			// settings.
			//		public bool legAlwaysSolveEnabled = true;
			//		public bool armAlwaysSolveEnabled = false;

			if (_limbIK != null || _headIK != null) {
				Bones_PrepareUpdate();

				var isSolved = false;
				var isHeadSolved = false;
				if (_limbIK != null) {
					var limbIKLength = _limbIK.Length;
					for (var i = 0; i != limbIKLength; ++i) {
						if (_limbIK[i] != null) {
							isSolved |= _limbIK[i].Solve();
						}
					}
				}
				if (_headIK != null) {
					isHeadSolved = _headIK.Solve(this);
					isSolved |= isHeadSolved;
				}

				if (isHeadSolved && _isNeedFixShoulderWorldTransform) {
					leftArmBones.shoulder?.Forcefix_worldRotation();
					rightArmBones.shoulder?.Forcefix_worldRotation();
				}

				if (isSolved) {
					Bones_WriteToTransform();
				}
			}

			if (_fingerIK != null) {
				Bones_PrepareUpdate();

				var isSolved = false;
				var fingerIKLength = _fingerIK.Length;
				for (var i = 0; i != fingerIKLength; ++i) {
					if (_fingerIK[i] != null) {
						isSolved |= _fingerIK[i].Solve();
					}
				}

				if (isSolved) {
					Bones_WriteToTransform();
				}
			}
		}

		void Bones_PrepareUpdate() {
			if (_bones != null) {
				var boneLength = _bones.Length;
				for (var i = 0; i != boneLength; ++i) {
					_bones[i]?.PrepareUpdate();
				}
			}
		}

		void Bones_WriteToTransform() {
			if (_bones != null) {
				var boneLength = _bones.Length;
				for (var i = 0; i != boneLength; ++i) {
					_bones[i]?.WriteToTransform();
				}
			}
		}

		void Prefix(Bone bone, BoneLocation boneLocation, Bone parentBoneLocationBased) {
			Assert(_bones != null);
			Bone.Prefix(_bones, ref bone, boneLocation, parentBoneLocationBased);
		}

		void Prefix(
			Effector effector,
			EffectorLocation effectorLocation) {
			Assert(_effectors != null);
			var createEffectorTransform = settings.createEffectorTransform;
			Assert(rootTransform != null);
			Effector.Prefix(_effectors, ref effector, effectorLocation, createEffectorTransform, rootTransform.Target);
		}

		void Prefix(
			Effector effector,
			EffectorLocation effectorLocation,
			Effector parentEffector,
			IArray<Bone> bones) {
			Prefix(effector, effectorLocation, parentEffector, bones != null && bones.Length > 0 ? bones[bones.Length - 1] : null);
		}

		void Prefix(
			Effector effector,
			EffectorLocation effectorLocation,
			Effector parentEffector,
			Bone bone,
			Bone leftBone = null,
			Bone rightBone = null) {
			Assert(_effectors != null);
			var createEffectorTransform = settings.createEffectorTransform;
			Effector.Prefix(_effectors, ref effector, effectorLocation, createEffectorTransform, null, parentEffector, bone, leftBone, rightBone);
		}

		//----------------------------------------------------------------------------------------------------------------------------

		// Custom Solver.
		public bool IsHiddenCustomEyes() {
			return false;
		}

		public bool PrepareCustomEyes(ref Quaternionf headToLeftEyeRotation, ref Quaternionf headToRightEyeRotation) {
			return false;
		}

		public void ResetCustomEyes() {
		}

		public void SolveCustomEyes(ref IKMatrix3x3 neckBasis, ref IKMatrix3x3 headBasis, ref IKMatrix3x3 headBaseBasis) {
		}


		public static void SafeResize<TYPE_>(ref TYPE_[] objArray, int length) {
			if (objArray == null) {
				objArray = new TYPE_[length];
			}
			else {
				Array.Resize(ref objArray, length);
			}
		}

		public static void PrepareArray<TypeA, TypeB>(ref TypeA[] dstArray, TypeB[] srcArray) {
			if (srcArray != null) {
				if (dstArray == null || dstArray.Length != srcArray.Length) {
					dstArray = new TypeA[srcArray.Length];
				}
			}
			else {
				dstArray = null;
			}
		}

		public static void CloneArray<Type>(ref Type[] dstArray, Type[] srcArray) {
			if (srcArray != null) {
				if (dstArray == null || dstArray.Length != srcArray.Length) {
					dstArray = new Type[srcArray.Length];
				}
				for (var i = 0; i < srcArray.Length; ++i) {
					dstArray[i] = srcArray[i];
				}
			}
			else {
				dstArray = null;
			}
		}


		public static bool IsParentOfRecusively(Entity parent, Entity child) {
			while (child != null) {
				if (child.parent.Target == parent) {
					return true;
				}

				child = child.parent.Target;
			}

			return false;
		}

		//----------------------------------------------------------------------------------------------------------------

		public static Bone PrepareBone(Bone bone) {
			return bone != null && bone.TransformIsAlive ? bone : null;
		}

		public static Bone[] PrepareBones(Bone leftBone, Bone rightBone) {
			Assert(leftBone != null && rightBone != null);
			if (leftBone != null && rightBone != null) {
				if (leftBone.TransformIsAlive && rightBone.TransformIsAlive) {
					var bones = new Bone[2];
					bones[0] = leftBone;
					bones[1] = rightBone;
					return bones;
				}
			}

			return null;
		}

		//----------------------------------------------------------------------------------------------------------------

		public static bool ComputeEyesRange(ref Vector3f eyesDir, float rangeTheta) {
			if (rangeTheta >= -IKMath.IK_EPSILON) { // range
				if (eyesDir.z < 0.0f) {
					eyesDir.z = -eyesDir.z;
				}

				return true;
			}
			else if (rangeTheta >= -1.0f + IKMath.IK_EPSILON) {
				var shiftZ = -rangeTheta;
				eyesDir.z += shiftZ;
				if (eyesDir.z < 0.0f) {
					eyesDir.z *= 1.0f / (1.0f - shiftZ);
				}
				else {
					eyesDir.z *= 1.0f / (1.0f + shiftZ);
				}

				var xyLen = IKMath.Sqrt(eyesDir.x * eyesDir.x + eyesDir.y * eyesDir.y);
				if (xyLen > IKMath.FLOAT_EPSILON) {
					var xyLenTo = IKMath.Sqrt(1.0f - eyesDir.z * eyesDir.z);
					var xyLenScale = xyLenTo / xyLen;
					eyesDir.x *= xyLenScale;
					eyesDir.y *= xyLenScale;
					return true;
				}
				else {
					eyesDir.x = 0.0f;
					eyesDir.y = 0.0f;
					eyesDir.z = 1.0f;
					return false;
				}
			}
			else {
				return true;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DebugLogError(object msg) {
#if IK_DEBUG
			Debug.LogError(msg);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Assert(bool cmp) {
#if IK_DEBUG
			if (!cmp) {
				Debug.LogError("Assert");
				Debug.Break();
			}
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CheckNormalized(Vector3f v) {
#if IK_DEBUG
			var epsilon = 1e-4f;
			var n = (v.x * v.x) + (v.y * v.y) + (v.z * v.z);
			if (n < 1.0f - epsilon || n > 1.0f + epsilon) {
				Debug.LogError("CheckNormalized:" + n.ToString("F6"));
				Debug.Break();
			}
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CheckNaN(float f) {
#if IK_DEBUG
			if (float.IsNaN(f)) {
				Debug.LogError("NaN");
			}
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CheckNaN(Vector3f v) {
#if IK_DEBUG
			if (float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z)) {
				Debug.LogError("NaN:" + v);
			}
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CheckNaN(Quaternionf q) {
#if IK_DEBUG
			if (float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w)) {
				Debug.LogError("NaN:" + q);
			}
#endif
		}

		public const float EYES_DEFAULTDISTANCE = 1.0f;
		public const float EYES_MINDISTANCE = 0.5f;

		public const float SIMUALTEEYS_NECKHEADDISTANCESCALE = 1.0f;
		public const int MAX_ARM_ROLL_LENGTH = 4;
		public const int MAX_ELBOW_ROLL_LENGTH = 4;
		public const int MAX_HAND_FINGER_LENGTH = 4;

		public static LimbIKType ToLimbIKType(LimbIKLocation limbIKLocation) {
			return limbIKLocation switch {
				LimbIKLocation.LeftLeg => LimbIKType.Leg,
				LimbIKLocation.RightLeg => LimbIKType.Leg,
				LimbIKLocation.LeftArm => LimbIKType.Arm,
				LimbIKLocation.RightArm => LimbIKType.Arm,
				_ => LimbIKType.Unknown,
			};
		}

		public static Side ToLimbIKSide(LimbIKLocation limbIKLocation) {
			return limbIKLocation switch {
				LimbIKLocation.LeftLeg => Side.Left,
				LimbIKLocation.RightLeg => Side.Right,
				LimbIKLocation.LeftArm => Side.Left,
				LimbIKLocation.RightArm => Side.Right,
				_ => Side.None,
			};
		}

		public static BoneType ToBoneType(BoneLocation boneLocation) {
			return boneLocation switch {
				BoneLocation.Hips => BoneType.Hips,
				BoneLocation.Neck => BoneType.Neck,
				BoneLocation.Head => BoneType.Head,
				BoneLocation.LeftEye => BoneType.Eye,
				BoneLocation.RightEye => BoneType.Eye,
				BoneLocation.LeftLeg => BoneType.Leg,
				BoneLocation.RightLeg => BoneType.Leg,
				BoneLocation.LeftKnee => BoneType.Knee,
				BoneLocation.RightKnee => BoneType.Knee,
				BoneLocation.LeftFoot => BoneType.Foot,
				BoneLocation.RightFoot => BoneType.Foot,
				BoneLocation.LeftShoulder => BoneType.Shoulder,
				BoneLocation.RightShoulder => BoneType.Shoulder,
				BoneLocation.LeftArm => BoneType.Arm,
				BoneLocation.RightArm => BoneType.Arm,
				BoneLocation.LeftElbow => BoneType.Elbow,
				BoneLocation.RightElbow => BoneType.Elbow,
				BoneLocation.LeftWrist => BoneType.Wrist,
				BoneLocation.RightWrist => BoneType.Wrist,
				_ => (int)boneLocation is >= ((int)BoneLocation.Spine) and
								<= ((int)BoneLocation.SpineU)
								? BoneType.Spine
								: (int)boneLocation is >= ((int)BoneLocation.LeftArmRoll0) and
								<= ((int)BoneLocation.RightArmRoll0 + MAX_ARM_ROLL_LENGTH - 1)
								? BoneType.ArmRoll
								: (int)boneLocation is >= ((int)BoneLocation.LeftElbowRoll0) and
								<= ((int)BoneLocation.RightElbowRoll0 + MAX_ELBOW_ROLL_LENGTH - 1)
								? BoneType.ElbowRoll
								: (int)boneLocation is >= ((int)BoneLocation.LeftHandThumb0) and
								<= ((int)BoneLocation.RightHandLittleTip)
								? BoneType.HandFinger
								: BoneType.Unknown,
			};
		}

		public static Side ToBoneSide(BoneLocation boneLocation) {
			return boneLocation switch {
				BoneLocation.LeftEye => Side.Left,
				BoneLocation.RightEye => Side.Right,
				BoneLocation.LeftLeg => Side.Left,
				BoneLocation.RightLeg => Side.Right,
				BoneLocation.LeftKnee => Side.Left,
				BoneLocation.RightKnee => Side.Right,
				BoneLocation.LeftFoot => Side.Left,
				BoneLocation.RightFoot => Side.Right,
				BoneLocation.LeftShoulder => Side.Left,
				BoneLocation.RightShoulder => Side.Right,
				BoneLocation.LeftArm => Side.Left,
				BoneLocation.RightArm => Side.Right,
				BoneLocation.LeftElbow => Side.Left,
				BoneLocation.RightElbow => Side.Right,
				BoneLocation.LeftWrist => Side.Left,
				BoneLocation.RightWrist => Side.Right,
				_ => (int)boneLocation is >= ((int)BoneLocation.LeftHandThumb0) and
								<= ((int)BoneLocation.LeftHandLittleTip)
								? Side.Left
								: (int)boneLocation is >= ((int)BoneLocation.LeftArmRoll0) and
								<= ((int)BoneLocation.LeftArmRoll0 + MAX_ARM_ROLL_LENGTH - 1)
								? Side.Left
								: (int)boneLocation is >= ((int)BoneLocation.RightArmRoll0) and
								<= ((int)BoneLocation.RightArmRoll0 + MAX_ARM_ROLL_LENGTH - 1)
								? Side.Right
								: (int)boneLocation is >= ((int)BoneLocation.LeftElbowRoll0) and
								<= ((int)BoneLocation.LeftElbowRoll0 + MAX_ELBOW_ROLL_LENGTH - 1)
								? Side.Left
								: (int)boneLocation is >= ((int)BoneLocation.RightElbowRoll0) and
								<= ((int)BoneLocation.RightElbowRoll0 + MAX_ELBOW_ROLL_LENGTH - 1)
								? Side.Right
								: (int)boneLocation is >= ((int)BoneLocation.RightHandThumb0) and
								<= ((int)BoneLocation.RightHandLittleTip)
								? Side.Right
								: Side.None,
			};
		}

		public static FingerType ToFingerType(BoneLocation boneLocation) {
			return (int)boneLocation is >= ((int)BoneLocation.LeftHandThumb0) and
				<= ((int)BoneLocation.LeftHandLittleTip)
				? (FingerType)(((int)boneLocation - (int)BoneLocation.LeftHandThumb0) / MAX_HAND_FINGER_LENGTH)
				: (int)boneLocation is >= ((int)BoneLocation.RightHandThumb0) and
				<= ((int)BoneLocation.RightHandLittleTip)
				? (FingerType)(((int)boneLocation - (int)BoneLocation.RightHandThumb0) / MAX_HAND_FINGER_LENGTH)
				: FingerType.Unknown;
		}

		public static int ToFingerIndex(BoneLocation boneLocation) {
			return (int)boneLocation is >= ((int)BoneLocation.LeftHandThumb0) and
				<= ((int)BoneLocation.LeftHandLittleTip)
				? ((int)boneLocation - (int)BoneLocation.LeftHandThumb0) % MAX_HAND_FINGER_LENGTH
				: (int)boneLocation is >= ((int)BoneLocation.RightHandThumb0) and
				<= ((int)BoneLocation.RightHandLittleTip)
				? ((int)boneLocation - (int)BoneLocation.RightHandThumb0) % MAX_HAND_FINGER_LENGTH
				: -1;
		}

		public static EffectorType ToEffectorType(EffectorLocation effectorLocation) {
			return effectorLocation switch {
				EffectorLocation.Root => EffectorType.Root,
				EffectorLocation.Hips => EffectorType.Hips,
				EffectorLocation.Neck => EffectorType.Neck,
				EffectorLocation.Head => EffectorType.Head,
				EffectorLocation.Eyes => EffectorType.Eyes,
				EffectorLocation.LeftKnee => EffectorType.Knee,
				EffectorLocation.RightKnee => EffectorType.Knee,
				EffectorLocation.LeftFoot => EffectorType.Foot,
				EffectorLocation.RightFoot => EffectorType.Foot,
				EffectorLocation.LeftArm => EffectorType.Arm,
				EffectorLocation.RightArm => EffectorType.Arm,
				EffectorLocation.LeftElbow => EffectorType.Elbow,
				EffectorLocation.RightElbow => EffectorType.Elbow,
				EffectorLocation.LeftWrist => EffectorType.Wrist,
				EffectorLocation.RightWrist => EffectorType.Wrist,
				_ => (int)effectorLocation is >= ((int)EffectorLocation.LeftHandThumb) and
								<= ((int)EffectorLocation.RightHandLittle)
								? EffectorType.HandFinger
								: EffectorType.Unknown,
			};
		}

		public static Side ToEffectorSide(EffectorLocation effectorLocation) {
			return effectorLocation switch {
				EffectorLocation.LeftKnee => Side.Left,
				EffectorLocation.RightKnee => Side.Right,
				EffectorLocation.LeftFoot => Side.Left,
				EffectorLocation.RightFoot => Side.Right,
				EffectorLocation.LeftArm => Side.Left,
				EffectorLocation.RightArm => Side.Right,
				EffectorLocation.LeftElbow => Side.Left,
				EffectorLocation.RightElbow => Side.Right,
				EffectorLocation.LeftWrist => Side.Left,
				EffectorLocation.RightWrist => Side.Right,
				_ => (int)effectorLocation is >= ((int)EffectorLocation.LeftHandThumb) and
								<= ((int)EffectorLocation.LeftHandLittle)
								? Side.Left
								: (int)effectorLocation is >= ((int)EffectorLocation.RightHandThumb) and
								<= ((int)EffectorLocation.RightHandLittle)
								? Side.Right
								: Side.None,
			};
		}

		public static string GetEffectorName(EffectorLocation effectorLocation) {
			return effectorLocation == EffectorLocation.Root
				? "FullBodyIK"
				: IsHandFingerEffectors(effectorLocation) ? ToFingerType(effectorLocation).ToString() : effectorLocation.ToString();
		}

		public static bool IsHandFingerEffectors(EffectorLocation effectorLocation) {
			var v = (int)effectorLocation;
			return v is >= ((int)EffectorLocation.LeftHandThumb) and <= ((int)EffectorLocation.RightHandLittle);
		}

		public static FingerType ToFingerType(EffectorLocation effectorLocation) {
			if (IsHandFingerEffectors(effectorLocation)) {
				var value = (int)effectorLocation - (int)EffectorLocation.LeftHandThumb;
				return (FingerType)(value % 5);
			}

			return FingerType.Unknown;
		}


	}

}