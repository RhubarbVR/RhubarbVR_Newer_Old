// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

namespace RNumerics.IK
{
	public partial class FullBodyIK
	{
		public class HeadIK
		{
			Settings _settings;
			InternalValues _internalValues;

			Bone _neckBone;
			Bone _headBone;
			Bone _leftEyeBone;
			Bone _rightEyeBone;

			Effector _headEffector;
			Effector _eyesEffector;

			Quaternionf _headEffectorToWorldRotation = Quaternionf.Identity;
			Quaternionf _headToLeftEyeRotation = Quaternionf.Identity;
			Quaternionf _headToRightEyeRotation = Quaternionf.Identity;

			public HeadIK(FullBodyIK fullBodyIK) {
				_settings = fullBodyIK.settings;
				_internalValues = fullBodyIK.internalValues;

				_neckBone = _PrepareBone(fullBodyIK.headBones.neck);
				_headBone = _PrepareBone(fullBodyIK.headBones.head);
				_leftEyeBone = _PrepareBone(fullBodyIK.headBones.leftEye);
				_rightEyeBone = _PrepareBone(fullBodyIK.headBones.rightEye);
				_headEffector = fullBodyIK.headEffectors.head;
				_eyesEffector = fullBodyIK.headEffectors.eyes;
			}

			bool _isSyncDisplacementAtLeastOnce;
			bool _isEnabledCustomEyes;

			void _SyncDisplacement(FullBodyIK fullBodyIK) {
				// Measure bone length.(Using worldPosition)
				// Force execution on 1st time. (Ignore case _settings.syncDisplacement == SyncDisplacement.Disable)
				if (_settings.syncDisplacement == SyncDisplacement.Everyframe || !_isSyncDisplacementAtLeastOnce) {
					_isSyncDisplacementAtLeastOnce = true;

					if (_headBone != null && _headBone.transformIsAlive) {
						if (_headEffector != null) {
							SAFBIKQuatMultInv0(out _headEffectorToWorldRotation, ref _headEffector._defaultRotation, ref _headBone._defaultRotation);
						}
						if (_leftEyeBone != null && _leftEyeBone.transformIsAlive) {
							SAFBIKQuatMultInv0(out _headToLeftEyeRotation, ref _headBone._defaultRotation, ref _leftEyeBone._defaultRotation);
						}
						if (_rightEyeBone != null && _rightEyeBone.transformIsAlive) {
							SAFBIKQuatMultInv0(out _headToRightEyeRotation, ref _headBone._defaultRotation, ref _rightEyeBone._defaultRotation);
						}
					}

					_isEnabledCustomEyes = fullBodyIK._PrepareCustomEyes(ref _headToLeftEyeRotation, ref _headToRightEyeRotation);
				}
			}

			public bool Solve(FullBodyIK fullBodyIK) {
				if (_neckBone == null || !_neckBone.transformIsAlive ||
					_headBone == null || !_headBone.transformIsAlive ||
					_headBone.parentBone == null || !_headBone.parentBone.transformIsAlive) {
					return false;
				}

				_SyncDisplacement(fullBodyIK);

				float headPositionWeight = _headEffector.positionEnabled ? _headEffector.positionWeight : 0.0f;
				float eyesPositionWeight = _eyesEffector.positionEnabled ? _eyesEffector.positionWeight : 0.0f;

				if (headPositionWeight <= IKEpsilon && eyesPositionWeight <= IKEpsilon) {
					Quaternionf parentWorldRotation = _neckBone.parentBone.worldRotation;
					Quaternionf parentBaseRotation;
					SAFBIKQuatMult(out parentBaseRotation, ref parentWorldRotation, ref _neckBone.parentBone._worldToBaseRotation);

					float headRotationWeight = _headEffector.rotationEnabled ? _headEffector.rotationWeight : 0.0f;
					if (headRotationWeight > IKEpsilon) {
						Quaternionf headEffectorWorldRotation = _headEffector.worldRotation;
						Quaternionf toRotation;
						SAFBIKQuatMult(out toRotation, ref headEffectorWorldRotation, ref _headEffectorToWorldRotation);
						if (headRotationWeight < 1.0f - IKEpsilon) {
							Quaternionf fromRotation;
							fromRotation = _headBone.worldRotation; // This is able to use _headBone.worldRotation directly.
							_headBone.worldRotation = Quaternionf.Slerp(fromRotation, toRotation, headRotationWeight);
						}
						else {
							_headBone.worldRotation = toRotation;
						}

						_HeadRotationLimit();
					}


					return (headRotationWeight > IKEpsilon);
				}

				_Solve(fullBodyIK);
				return true;
			}

			void _HeadRotationLimit() {
				// Rotation Limit.
				Quaternionf tempRotation, headRotation, neckRotation, localRotation;
				tempRotation = _headBone.worldRotation;
				SAFBIKQuatMult(out headRotation, ref tempRotation, ref _headBone._worldToBaseRotation);
				tempRotation = _neckBone.worldRotation;
				SAFBIKQuatMult(out neckRotation, ref tempRotation, ref _neckBone._worldToBaseRotation);
				SAFBIKQuatMultInv0(out localRotation, ref neckRotation, ref headRotation);

				Matrix3x3 localBasis;
				SAFBIKMatSetRot(out localBasis, ref localRotation);

				Vector3f localDirY = localBasis.column1;
				Vector3f localDirZ = localBasis.column2;

				bool isLimited = false;
				isLimited |= _LimitXZ_Square(ref localDirY,
					_internalValues.headIK.headLimitRollTheta.sin,
					_internalValues.headIK.headLimitRollTheta.sin,
					_internalValues.headIK.headLimitPitchUpTheta.sin,
					_internalValues.headIK.headLimitPitchDownTheta.sin);
				isLimited |= _LimitXY_Square(ref localDirZ,
					_internalValues.headIK.headLimitYawTheta.sin,
					_internalValues.headIK.headLimitYawTheta.sin,
					_internalValues.headIK.headLimitPitchDownTheta.sin,
					_internalValues.headIK.headLimitPitchUpTheta.sin);

				if (isLimited) {
					if (SAFBIKComputeBasisFromYZLockZ(out localBasis, ref localDirY, ref localDirZ)) {
						SAFBIKMatGetRot(out localRotation, ref localBasis);
						SAFBIKQuatMultNorm3(out headRotation, ref neckRotation, ref localRotation, ref _headBone._baseToWorldRotation);
						_headBone.worldRotation = headRotation;
					}
				}
			}

			void _Solve(FullBodyIK fullBodyIK) {
				Quaternionf parentWorldRotation = _neckBone.parentBone.worldRotation;
				Matrix3x3 parentBasis;
				SAFBIKMatSetRotMultInv1(out parentBasis, ref parentWorldRotation, ref _neckBone.parentBone._defaultRotation);
				Matrix3x3 parentBaseBasis;
				SAFBIKMatMult(out parentBaseBasis, ref parentBasis, ref _internalValues.defaultRootBasis);
				Quaternionf parentBaseRotation;
				SAFBIKQuatMult(out parentBaseRotation, ref parentWorldRotation, ref _neckBone.parentBone._worldToBaseRotation);

				float headPositionWeight = _headEffector.positionEnabled ? _headEffector.positionWeight : 0.0f;
				float eyesPositionWeight = _eyesEffector.positionEnabled ? _eyesEffector.positionWeight : 0.0f;

				Quaternionf neckBonePrevRotation = Quaternionf.Identity;
				Quaternionf headBonePrevRotation = Quaternionf.Identity;
				Quaternionf leftEyeBonePrevRotation = Quaternionf.Identity;
				Quaternionf rightEyeBonePrevRotation = Quaternionf.Identity;
				neckBonePrevRotation = _neckBone.worldRotation;
				headBonePrevRotation = _headBone.worldRotation;
				if (_leftEyeBone != null && _leftEyeBone.transformIsAlive) {
					leftEyeBonePrevRotation = _leftEyeBone.worldRotation;
				}
				if (_rightEyeBone != null && _rightEyeBone.transformIsAlive) {
					rightEyeBonePrevRotation = _rightEyeBone.worldRotation;
				}


				// for Neck
				if (headPositionWeight > IKEpsilon) {
					Matrix3x3 neckBoneBasis;
					SAFBIKMatMult(out neckBoneBasis, ref parentBasis, ref _neckBone._localAxisBasis);

					Vector3f yDir = _headEffector.worldPosition - _neckBone.worldPosition; // Not use _hidden_worldPosition
					if (SAFBIKVecNormalize(ref yDir)) {
						Vector3f localDir;
						SAFBIKMatMultVecInv(out localDir, ref neckBoneBasis, ref yDir);

						if (_LimitXZ_Square(ref localDir,
							_internalValues.headIK.neckLimitRollTheta.sin,
							_internalValues.headIK.neckLimitRollTheta.sin,
							_internalValues.headIK.neckLimitPitchDownTheta.sin,
							_internalValues.headIK.neckLimitPitchUpTheta.sin)) {
							SAFBIKMatMultVec(out yDir, ref neckBoneBasis, ref localDir);
						}

						Vector3f xDir = parentBaseBasis.column0;
						Vector3f zDir = parentBaseBasis.column2;
						if (SAFBIKComputeBasisLockY(out neckBoneBasis, ref xDir, ref yDir, ref zDir)) {
							Quaternionf worldRotation;
							SAFBIKMatMultGetRot(out worldRotation, ref neckBoneBasis, ref _neckBone._boneToWorldBasis);
							if (headPositionWeight < 1.0f - IKEpsilon) {
								Quaternionf fromRotation;
								fromRotation = neckBonePrevRotation; // This is able to use _headBone.worldRotation directly.


								_neckBone.worldRotation = Quaternionf.Slerp(fromRotation, worldRotation, headPositionWeight);
							}
							else {
								_neckBone.worldRotation = worldRotation;
							}
						}
					}
				}

				// for Head / Eyes
				if (eyesPositionWeight <= IKEpsilon) {
					float headRotationWeight = _headEffector.rotationEnabled ? _headEffector.rotationWeight : 0.0f;
					if (headRotationWeight > IKEpsilon) {
						Quaternionf headEffectorWorldRotation = _headEffector.worldRotation;
						Quaternionf toRotation;
						SAFBIKQuatMult(out toRotation, ref headEffectorWorldRotation, ref _headEffectorToWorldRotation);
						if (headRotationWeight < 1.0f - IKEpsilon) {
							Quaternionf fromRotation;
							Quaternionf neckBoneWorldRotation = _neckBone.worldRotation;
							// Not use _headBone.worldRotation.
							SAFBIKQuatMultNorm3Inv1(out fromRotation, ref neckBoneWorldRotation, ref neckBonePrevRotation, ref headBonePrevRotation);
							_headBone.worldRotation = Quaternionf.Slerp(fromRotation, toRotation, headRotationWeight);
						}
						else {
							_headBone.worldRotation = toRotation;
						}
					}

					_HeadRotationLimit();

					return;
				}

				{
					Vector3f eyesPosition, parentBoneWorldPosition = _neckBone.parentBone.worldPosition;
					SAFBIKMatMultVecPreSubAdd(out eyesPosition, ref parentBasis, ref _eyesEffector._defaultPosition, ref _neckBone.parentBone._defaultPosition, ref parentBoneWorldPosition);

					// Note: Not use _eyesEffector._hidden_worldPosition
					Vector3f eyesDir = _eyesEffector.worldPosition - eyesPosition; // Memo: Not normalize yet.

					Matrix3x3 neckBaseBasis = parentBaseBasis;

					{
						Vector3f localDir;
						SAFBIKMatMultVecInv(out localDir, ref parentBaseBasis, ref eyesDir);

						localDir.y *= _settings.headIK.eyesToNeckPitchRate;
						SAFBIKVecNormalize(ref localDir);

						if (_ComputeEyesRange(ref localDir, _internalValues.headIK.eyesTraceTheta.cos)) {
							if (localDir.y < -_internalValues.headIK.neckLimitPitchDownTheta.sin) {
								localDir.y = -_internalValues.headIK.neckLimitPitchDownTheta.sin;
							}
							else if (localDir.y > _internalValues.headIK.neckLimitPitchUpTheta.sin) {
								localDir.y = _internalValues.headIK.neckLimitPitchUpTheta.sin;
							}
							localDir.x = 0.0f;
							localDir.z = SAFBIKSqrt(1.0f - localDir.y * localDir.y);
						}

						SAFBIKMatMultVec(out eyesDir, ref parentBaseBasis, ref localDir);

						{
							Vector3f xDir = parentBaseBasis.column0;
							Vector3f yDir = parentBaseBasis.column1;
							Vector3f zDir = eyesDir;

							if (!SAFBIKComputeBasisLockZ(out neckBaseBasis, ref xDir, ref yDir, ref zDir)) {
								neckBaseBasis = parentBaseBasis; // Failsafe.
							}
						}

						Quaternionf worldRotation;
						SAFBIKMatMultGetRot(out worldRotation, ref neckBaseBasis, ref _neckBone._baseToWorldBasis);
						if (_eyesEffector.positionWeight < 1.0f - IKEpsilon) {
							Quaternionf neckWorldRotation = Quaternionf.Slerp(_neckBone.worldRotation, worldRotation, _eyesEffector.positionWeight); // This is able to use _neckBone.worldRotation directly.
							_neckBone.worldRotation = neckWorldRotation;
							SAFBIKMatSetRotMult(out neckBaseBasis, ref neckWorldRotation, ref _neckBone._worldToBaseRotation);
						}
						else {
							_neckBone.worldRotation = worldRotation;
						}
					}

					Matrix3x3 neckBasis;
					SAFBIKMatMult(out neckBasis, ref neckBaseBasis, ref _internalValues.defaultRootBasisInv);

					Vector3f neckBoneWorldPosition = _neckBone.worldPosition;
					SAFBIKMatMultVecPreSubAdd(out eyesPosition, ref neckBasis, ref _eyesEffector._defaultPosition, ref _neckBone._defaultPosition, ref neckBoneWorldPosition);

					// Note: Not use _eyesEffector._hidden_worldPosition
					eyesDir = _eyesEffector.worldPosition - eyesPosition;

					Matrix3x3 headBaseBasis = neckBaseBasis;

					{
						Vector3f localDir;
						SAFBIKMatMultVecInv(out localDir, ref neckBaseBasis, ref eyesDir);

						localDir.x *= _settings.headIK.eyesToHeadYawRate;
						localDir.y *= _settings.headIK.eyesToHeadPitchRate;

						SAFBIKVecNormalize(ref localDir);

						if (_ComputeEyesRange(ref localDir, _internalValues.headIK.eyesTraceTheta.cos)) {
							// Note: Not use _LimitXY() for Stability
							_LimitXY_Square(ref localDir,
								_internalValues.headIK.headLimitYawTheta.sin,
								_internalValues.headIK.headLimitYawTheta.sin,
								_internalValues.headIK.headLimitPitchDownTheta.sin,
								_internalValues.headIK.headLimitPitchUpTheta.sin);
						}

						SAFBIKMatMultVec(out eyesDir, ref neckBaseBasis, ref localDir);

						{
							Vector3f xDir = neckBaseBasis.column0;
							Vector3f yDir = neckBaseBasis.column1;
							Vector3f zDir = eyesDir;

							if (!SAFBIKComputeBasisLockZ(out headBaseBasis, ref xDir, ref yDir, ref zDir)) {
								headBaseBasis = neckBaseBasis;
							}
						}

						Quaternionf worldRotation;
						SAFBIKMatMultGetRot(out worldRotation, ref headBaseBasis, ref _headBone._baseToWorldBasis);
						if (_eyesEffector.positionWeight < 1.0f - IKEpsilon) {
							Quaternionf neckBoneWorldRotation = _neckBone.worldRotation;
							Quaternionf headFromWorldRotation;
							SAFBIKQuatMultNorm3Inv1(out headFromWorldRotation, ref neckBoneWorldRotation, ref neckBonePrevRotation, ref headBonePrevRotation);
							Quaternionf headWorldRotation = Quaternionf.Slerp(headFromWorldRotation, worldRotation, _eyesEffector.positionWeight);
							_headBone.worldRotation = headWorldRotation;
							SAFBIKMatSetRotMult(out headBaseBasis, ref headWorldRotation, ref _headBone._worldToBaseRotation);
						}
						else {
							_headBone.worldRotation = worldRotation;
						}
					}

					Matrix3x3 headBasis;
					SAFBIKMatMult(out headBasis, ref headBaseBasis, ref _internalValues.defaultRootBasisInv);

					if (_isEnabledCustomEyes) {
						fullBodyIK._SolveCustomEyes(ref neckBasis, ref headBasis, ref headBaseBasis);
					}
					else {
						_SolveEyes(ref neckBasis, ref headBasis, ref headBaseBasis, ref headBonePrevRotation, ref leftEyeBonePrevRotation, ref rightEyeBonePrevRotation);
					}
				}
			}

			void _ResetEyes() {
				if (_headBone != null && _headBone.transformIsAlive) {
					Quaternionf headWorldRotation = _headBone.worldRotation;

					Quaternionf worldRotation;
					if (_leftEyeBone != null && _leftEyeBone.transformIsAlive) {
						SAFBIKQuatMultNorm(out worldRotation, ref headWorldRotation, ref _headToLeftEyeRotation);
						_leftEyeBone.worldRotation = worldRotation;
					}
					if (_rightEyeBone != null && _rightEyeBone.transformIsAlive) {
						SAFBIKQuatMultNorm(out worldRotation, ref headWorldRotation, ref _headToRightEyeRotation);
						_rightEyeBone.worldRotation = worldRotation;
					}
				}
			}

			void _SolveEyes(ref Matrix3x3 neckBasis, ref Matrix3x3 headBasis, ref Matrix3x3 headBaseBasis,
				ref Quaternionf headPrevRotation, ref Quaternionf leftEyePrevRotation, ref Quaternionf rightEyePrevRotation) {
				if (_headBone != null && _headBone.transformIsAlive) {
					if ((_leftEyeBone != null && _leftEyeBone.transformIsAlive) || (_rightEyeBone != null && _rightEyeBone.transformIsAlive)) {
						Vector3f headWorldPosition, neckBoneWorldPosition = _neckBone.worldPosition;
						SAFBIKMatMultVecPreSubAdd(out headWorldPosition, ref neckBasis, ref _headBone._defaultPosition, ref _neckBone._defaultPosition, ref neckBoneWorldPosition);

						Vector3f eyesPosition;
						SAFBIKMatMultVecPreSubAdd(out eyesPosition, ref headBasis, ref _eyesEffector._defaultPosition, ref _headBone._defaultPosition, ref headWorldPosition);

						Vector3f eyesDir = _eyesEffector.worldPosition - eyesPosition;

						SAFBIKMatMultVecInv(out eyesDir, ref headBaseBasis, ref eyesDir);

						SAFBIKVecNormalize(ref eyesDir);

						_LimitXY_Square(ref eyesDir,
							_internalValues.headIK.eyesLimitYawTheta.sin,
							_internalValues.headIK.eyesLimitYawTheta.sin,
							_internalValues.headIK.eyesLimitPitchTheta.sin,
							_internalValues.headIK.eyesLimitPitchTheta.sin);

						eyesDir.x *= _settings.headIK.eyesYawRate;
						eyesDir.y *= _settings.headIK.eyesPitchRate;
						Vector3f leftEyeDir = eyesDir;
						Vector3f rightEyeDir = eyesDir;

						if (eyesDir.x >= 0.0f) {
							leftEyeDir.x *= _settings.headIK.eyesYawInnerRate;
							rightEyeDir.x *= _settings.headIK.eyesYawOuterRate;
						}
						else {
							leftEyeDir.x *= _settings.headIK.eyesYawOuterRate;
							rightEyeDir.x *= _settings.headIK.eyesYawInnerRate;
						}

						SAFBIKVecNormalize2(ref leftEyeDir, ref rightEyeDir);

						SAFBIKMatMultVec(out leftEyeDir, ref headBaseBasis, ref leftEyeDir);
						SAFBIKMatMultVec(out rightEyeDir, ref headBaseBasis, ref rightEyeDir);

						Quaternionf worldRotation;

						Quaternionf headBoneWorldRotation = _headBone.worldRotation;

						if (_leftEyeBone != null && _leftEyeBone.transformIsAlive) {
							Matrix3x3 leftEyeBaseBasis;
							SAFBIKComputeBasisLockZ(out leftEyeBaseBasis, ref headBasis.column0, ref headBasis.column1, ref leftEyeDir);
							SAFBIKMatMultGetRot(out worldRotation, ref leftEyeBaseBasis, ref _leftEyeBone._baseToWorldBasis);
							if (_eyesEffector.positionWeight < 1.0f - IKEpsilon) {
								Quaternionf fromRotation;
								SAFBIKQuatMultNorm3Inv1(out fromRotation, ref headBoneWorldRotation, ref headPrevRotation, ref leftEyePrevRotation);
								_leftEyeBone.worldRotation = Quaternionf.Slerp(fromRotation, worldRotation, _eyesEffector.positionWeight);
							}
							else {
								_leftEyeBone.worldRotation = worldRotation;
							}
						}

						if (_rightEyeBone != null && _rightEyeBone.transformIsAlive) {
							Matrix3x3 rightEyeBaseBasis;
							SAFBIKComputeBasisLockZ(out rightEyeBaseBasis, ref headBasis.column0, ref headBasis.column1, ref rightEyeDir);
							SAFBIKMatMultGetRot(out worldRotation, ref rightEyeBaseBasis, ref _rightEyeBone._baseToWorldBasis);
							if (_eyesEffector.positionWeight < 1.0f - IKEpsilon) {
								Quaternionf fromRotation;
								SAFBIKQuatMultNorm3Inv1(out fromRotation, ref headBoneWorldRotation, ref headPrevRotation, ref rightEyePrevRotation);
								_rightEyeBone.worldRotation = Quaternionf.Slerp(fromRotation, worldRotation, _eyesEffector.positionWeight);
							}
							else {
								_rightEyeBone.worldRotation = worldRotation;
							}
						}
					}
				}

			}

		}
	}
}