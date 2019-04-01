/*
*	Copyright (c) 2017-2018. RainyRizzle. All rights reserved
*	contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of AnyPortrait.
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of Seungjik Lee.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// apBoneIKChainSet의 Opt버전
	/// Serialize가 안되므로 초기화시 Link를 해야한다.
	/// apBoneIKChainSet을 복붙하여 Opt만 추가하자
	/// </summary>
	[Serializable]
	public class apOptBoneIKChainSet
	{
		// Members
		//------------------------------------------------------------
		[SerializeField]
		public apOptBone _bone = null;

		/// <summary>
		/// Tail -> Head로 이어지는 ChainUnit
		/// IK 특성상 이 ChainSet을 소유하고 있는 Bone이 베이스가 되는 ChainUnit은 없다.
		/// </summary>
		[SerializeField]
		public List<apOptBoneIKChainUnit> _chainUnits = new List<apOptBoneIKChainUnit>();


		//Head/Tail의 체인들.
		//기존과 달리 NonSerialized이다.
		[NonSerialized]
		private apOptBoneIKChainUnit _headChainUnit = null;

		[NonSerialized]
		private apOptBoneIKChainUnit _tailChainUnit = null;

		public Vector2 _requestedTargetPosW = Vector2.zero;
		public Vector2 _requestedBonePosW = Vector2.zero;


		/// <summary>
		/// 계산을 반복하는 Loop의 최대 횟수
		/// </summary>
		public const int MAX_CALCULATE_LOOP_EDITOR = 30;
		public const int MAX_CALCULATE_LOOP_RUNTIME = 20;
		public const int MAX_TOTAL_UNIT_CALCULATE = 100;

		public const float CONTINUOUS_TARGET_POS_BIAS = 5.0f;
		public const float CONTINUOUS_ANGLE_JUMP_LIMIT = 30.0f;

		/// <summary>
		/// 연산이 종료되는 거리값 오차 허용값
		/// </summary>
		public const float BIAS_TARGET_POS_MATCH = 0.1f * 0.1f;//sqr Distance 값이다.


		private int _nLoop = 0;

		private Vector2 _prevTargetPosW = Vector2.zero;
		private bool _isContinuousPrevPos = false;

		
		// Init
		//------------------------------------------------------------
		public apOptBoneIKChainSet(apOptBone bone)
		{
			_bone = bone;
			_nLoop = 0;
		}

		// Functions
		//------------------------------------------------------------
		/// <summary>
		/// Bone Hierarchy에 맞추어서 다시 Chain을 만든다.
		/// </summary>
		public void RefreshChain()
		{
			_chainUnits.Clear();
			_headChainUnit = null;
			_tailChainUnit = null;

			if (!_bone._isIKTail)
			{
				return;
			}


			//Bone으로부터 Head가 나올때까지 Chain을 추가하자
			//[Parent] .... [Cur Bone] ----> [Target Bone < 이것 부터 시작]
			apOptBone parentBone = null;
			apOptBone curBone = null;
			apOptBone targetBone = _bone;

			string strBoneNames = "";

			//int curLevel = 0;
			while (true)
			{
				curBone = targetBone._parentBone;
				if (curBone == null)
				{
					//? 왜 여기서 끊기지..
					break;
				}

				parentBone = curBone._parentBone;//<<이건 Null 일 수 있다.

				//apBoneIKChainUnit newUnit = new apBoneIKChainUnit(curBone, targetBone, parentBone, curLevel);
				apOptBoneIKChainUnit newUnit = new apOptBoneIKChainUnit(curBone, targetBone, parentBone);

				_chainUnits.Add(newUnit);

				strBoneNames += curBone.transform.name + ", ";

				//끝났당
				if (curBone == _bone._IKHeaderBone)
				{
					break;
				}

				//하나씩 위로 탐색하자
				targetBone = curBone;
				//curLevel++;
			}
			if (_chainUnits.Count == 0)
			{
				return;
			}
			////전체 Unit 개수를 넣어주자
			//for (int i = 0; i < _chainUnits.Count; i++)
			//{
			//	_chainUnits[i].SetTotalChainLevel(_chainUnits.Count);
			//}

			//Debug.Log("Refresh Chain : [" + _bone._name + "] " + strBoneNames);

			//앞쪽이 Tail이다.
			_tailChainUnit = _chainUnits[0];
			_headChainUnit = _chainUnits[_chainUnits.Count - 1];

			//Chain Unit간의 연결을 한다.
			apOptBoneIKChainUnit curUnit = null;
			for (int i = 0; i < _chainUnits.Count; i++)
			{
				curUnit = _chainUnits[i];

				if (i > 0)
				{
					curUnit.SetChild(_chainUnits[i - 1]);
				}

				if (i < _chainUnits.Count - 1)
				{
					curUnit.SetParent(_chainUnits[i + 1]);
				}

				curUnit.SetTail(_tailChainUnit);
			}

			if (_chainUnits.Count == 0)
			{
				_nLoop = 0;
			}
			else
			{
				// 얼마나 연산을 반복할 것인지 결정 (연산 횟수는 루프 단위로 결정한다)
				_nLoop = MAX_CALCULATE_LOOP_EDITOR;
				if (_chainUnits.Count * _nLoop > MAX_TOTAL_UNIT_CALCULATE)
				{
					//전체 계산 횟수 (Unit * Loop)가 제한을 넘겼을 때
					_nLoop = MAX_TOTAL_UNIT_CALCULATE / _chainUnits.Count;
					if (_nLoop < 2)
					{
						_nLoop = 2;
					}
				}
			}

			_isContinuousPrevPos = false;
		}



		/// <summary>
		/// IK를 시뮬레이션한다.
		/// 요청한 Bone을 Tail로 하여 Head까지 처리한다.
		/// 결과값은 Delta Angle로 나오며, 이 값을 참조하여 결정한다. (Matrix 중 어디에 쓸지는 외부에서 결정)
		/// </summary>
		/// <param name="targetPosW"></param>
		public bool SimulateIK(Vector2 targetPosW, bool isContinuous)
		{
			
			if (!_bone._isIKTail)
			{
				//Debug.LogError("Failed 1 - _bone._isIKTail : " + _bone._isIKTail);
				return false;
			}

			if (_chainUnits.Count == 0)
			{
				//Debug.LogError("Failed 2 - _chainUnits.Count : " + _chainUnits.Count);
				return false;
			}

			apOptBoneIKChainUnit chainUnit = null;

			//[Tail] .....[] .... [Head]
			//Tail에 가까운(인덱스가 가장 작은) Constraint가 적용된 Bone을 구한다.
			//Head에 가까운(인덱스가 가장 큰) Constraint가 적용된 Bone을 구한다.

			float lengthTotal = 0.0f;

			//1. Simulate 준비
			//Debug.Log("ReadyToSimulate : " + _chainUnits.Count);
			for (int i = 0; i < _chainUnits.Count; i++)
			{
				chainUnit = _chainUnits[i];
				chainUnit.ReadyToSimulate();

				lengthTotal += chainUnit._lengthBoneToTarget;
			}

			if(_tailChainUnit == null)
			{
				_tailChainUnit = _chainUnits[0];
			}
			if(_headChainUnit == null)
			{
				_headChainUnit = _chainUnits[_chainUnits.Count - 1];
			}

			//1. 길이 확인 후 압축을 해야하는지 적용
			float length2Target = (targetPosW - _headChainUnit._bonePosW).magnitude;

			//Debug.Log("1) Length to Target : " + length2Target);
			//Debug.Log("Target PosW : " + targetPosW + " <- Head : " + _headChainUnit._bonePosW + " [" + _headChainUnit._baseBone._name + "]");


			float length2Tail = (_tailChainUnit._targetPosW - _headChainUnit._bonePosW).magnitude;

			//Debug.Log("2) Length to Tail : " + length2Tail);
			//Debug.Log("Tail : " + _tailChainUnit._targetPosW + " [" + _tailChainUnit._baseBone._name + "] <- Head : " + _headChainUnit._bonePosW + " [" + _headChainUnit._baseBone._name + "]");

			if (length2Tail == 0.0f)
			{
				//Debug.LogError("Failed 3 - length2Tail : " + length2Tail);
				//Debug.LogError("_tailChainUnit._targetPosW : " + _tailChainUnit._targetPosW);
				//Debug.LogError("_headChainUnit._bonePosW : " + _headChainUnit._bonePosW);

				return false;
			}

			float beforSqrDist = (targetPosW - _tailChainUnit._bonePosW).sqrMagnitude;

			apOptBoneIKChainUnit curBoneUnit = null;

			if (length2Target < lengthTotal)
			{
				//압축을 해야한다.
				float compressRatio = Mathf.Clamp01(length2Target / lengthTotal);
				//float compressAngle = Mathf.Acos(compressRatio) * Mathf.Rad2Deg;


				//Vector2 dirHeadToTarget = targetPosW - _headChainUnit._bonePosW;
				for (int i = 0; i < _chainUnits.Count; i++)
				{
					curBoneUnit = _chainUnits[i];
					if (curBoneUnit._isAngleContraint)
					{
						curBoneUnit._angleLocal_Next = curBoneUnit._angleDir_Preferred * (1.0f - compressRatio) + curBoneUnit._angleLocal_Next + compressRatio;

						//Preferred를 적용했다는 것을 알려주자
						curBoneUnit._isPreferredAngleAdapted = true;
					}
				}

				_headChainUnit.CalculateWorldRecursive();
			}
			else if (length2Target > lengthTotal + 1.0f)//Bias 추가해서 플래그 리셋
			{
				for (int i = 0; i < _chainUnits.Count; i++)
				{
					_chainUnits[i]._isPreferredAngleAdapted = false;
				}
			}

			curBoneUnit = null;
			int nCalculate = 1;
			for (int i = 0; i < _nLoop; i++)
			{
				curBoneUnit = _tailChainUnit;

				while (true)
				{
					//루프를 돕시다.
					curBoneUnit.RequestIK(targetPosW, isContinuous);

					curBoneUnit.CalculateWorldRecursive();

					//Debug.Log(" curBoneUnit._angleWorld_Next

					if (curBoneUnit._parentChainUnit != null)
					{
						curBoneUnit = curBoneUnit._parentChainUnit;
					}
					else
					{
						break;
					}
				}

				//마지막으로 Tail에서 처리 한번더
				curBoneUnit = _tailChainUnit;
				//curBoneUnit.RequestIK(targetPosW, i, _nLoop);
				curBoneUnit.RequestIK(targetPosW, isContinuous);
				curBoneUnit.CalculateWorldRecursive();

				nCalculate++;
			}


			//만약 Continuous 모드에서 각도가 너무 많이 차이가 나면 실패한 처리다.
			//이전 요청 좌표와 거리가 적은 경우 유효

			if (isContinuous)
			{
				if (_isContinuousPrevPos)
				{
					float distTargetDelta = Vector2.Distance(_prevTargetPosW, targetPosW);
					if (distTargetDelta < CONTINUOUS_TARGET_POS_BIAS)
					{

						//연속된 위치 입력인 경우
						//전체의 각도 크기를 구하자
						float totalDeltaAngle = 0.0f;
						for (int i = 0; i < _chainUnits.Count; i++)
						{
							totalDeltaAngle += Mathf.Abs(_chainUnits[i]._angleLocal_Delta);
						}
						//Debug.Log("Cont Move : " + distTargetDelta + " / Delta Angle : " + totalDeltaAngle);
						if (totalDeltaAngle > CONTINUOUS_ANGLE_JUMP_LIMIT)
						{
							//너무 많이 움직였다.
							_isContinuousPrevPos = true;
							_prevTargetPosW = targetPosW;
							//Debug.LogError("Angle Jump Error : Total Angle : " + totalDeltaAngle + " / Delta Target : " + distTargetDelta);
							return false;
						}
					}
				}
				_isContinuousPrevPos = true;
				_prevTargetPosW = targetPosW;
			}
			else
			{
				_isContinuousPrevPos = false;
			}

			if (isContinuous && length2Target < lengthTotal)
			{
				float afterSqrdist = (_tailChainUnit._targetPosW - targetPosW).sqrMagnitude;
				if (beforSqrDist * 1.2f < afterSqrdist)
				{
					//오히려 더 멀어졌다.
					//Debug.LogError("다시 멀어졌다");
					//Debug.LogError("Failed 4 - length2Target < lengthTotal : " + length2Target + " < " + lengthTotal);
					return false;
				}
			}

			_requestedTargetPosW = _tailChainUnit._targetPosW;
			_requestedBonePosW = _tailChainUnit._bonePosW;

			return true;
		}

		//Limited Simulate는 없애자.

		/// <summary>
		/// IK 결과값을 일단 각 Bone에게 넣어준다.
		/// 적용된 값이 아니라 변수로 저장하는 것이므로, 
		/// 각 Bone에 있는 _IKRequestAngleResult를 참조하자
		/// </summary>
		/// <param name="weight"></param>
		public void AdaptIKResultToBones(float weight)
		{
			//Debug.Log("AdaptIKResultToBones - " + weight);

			apOptBoneIKChainUnit chainUnit = null;
			for (int i = 0; i < _chainUnits.Count; i++)
			{
				chainUnit = _chainUnits[i];

				//Debug.Log("[" + i + "] : " + (chainUnit._angleWorld_Next - chainUnit._angleWorld_Prev));
				//float deltaAngle = (chainUnit._angleWorld_Next - chainUnit._angleWorld_Prev);
				//Debug.Log("[" + i + " - " + chainUnit._baseBone._name +"] : Delta Angle : " + deltaAngle + " ( Angle Next : " + chainUnit._angleWorld_Next + " - Angle Prev : " + chainUnit._angleWorld_Prev + " )");
				//chainUnit._baseBone.AddIKAngle(deltaAngle, weight);
				chainUnit._baseBone.AddIKAngle(chainUnit._angleWorld_Next, weight);
			}
		}
	}
}