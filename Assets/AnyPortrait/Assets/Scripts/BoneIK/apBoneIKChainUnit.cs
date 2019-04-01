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
	/// IK 계산시 입력을 받아서 계산을 수행할 때 사용되는 클래스
	/// Chaining 정보 입력 -> 각 관절의 각도 리턴을 해준다.
	/// 기본적으로 World좌표계로 계산이 되며,
	/// 얼마나 각도를 회전해야하는지 알려준다.
	/// 각 유닛은 Bone에 속하며, Tail->Head 방향으로 움직이며 계산한다.
	/// </summary>
	public class apBoneIKChainUnit
	{
		//Members
		//-------------------------------------------------
		public apBoneIKChainUnit _parentChainUnit = null;
		public apBoneIKChainUnit _childChainUnit = null;

		public apBoneIKChainUnit _tailChainUnit = null;


		public apBone _parentBone = null;//<Parent를 넣어야 IK
		public apBone _baseBone = null;
		public apBone _targetBone = null;

		//좌표계 값을 저장해야하는 것
		//(Parent) ------ [ (Cur Bone) ------------> ] (Target)
		//                World Pos <갱신됨>
		//                            Dir Length
		//                      () Local Angle <Prev, Next> - World Angle (갱신됨)

		//참조
		//Parent Pos / Dir / Angle (World Pos 갱신용 / Angle Constraint용)


		//Head인 경우)
		//[(Parent) ------------------- (Cur Bone) ------------> ] (Target)
		// Parent World Pos(연산 시작점)
		//     Parent Dir Vector (World)

		//Tail인 경우)
		//(Parent) ------ [ (Cur Bone) ------------> (Target) ]
		//                                       Target World Pos (계산하여 갱신한다)

		//연산 순서

		// Loop 연산 순서)
		// 1. (계산된) Target Pos W를 기준으로 
		//    [Cur Bone -> Target Angle]에서 [Cur Bone -> Request IK Angle]으로 변하는 Delta Angle을 구한다.
		//    Delta Angle을 현재의 Angle Next에 더한다.
		//    Angle Constraint가 적용되고 있는 중이라면) 
		//    -> Local Angle Next의 범위를 Lower / Upper로 제한한다.
		//    -> Loop 1회차에서는 "계산된 Angle"과 "Preferred Angle"의 중간값을 사용한다.

		// 2. Local Angle (Next)가 갱신되었으므로 "재귀적으로" Child Unit에게 World Matrix 갱신을 요청한다.
		//    (World Matrix 연산시에는 Local Angle + Parent World Angle => World Angle을 활용한다)
		//    마지막 Tail에서 연산을 마무리하면 Target Pos W가 완성된다.

		// 3. Parent로 이동하여 1.을 반복한다.





		//<현재 World Pos>
		//Bone들의 World 좌표
		//연산하면서 위치가 계속 바뀌지만, World Matrix로 반영하지 않고 별도로 계산하기 위해
		public Vector2 _bonePosW = Vector2.zero;

		//만약 Head라면 이 값을 사용해야한다.
		public Vector2 _parentPosW = Vector2.zero;
		public float _angleWorld_Parent = 0.0f;

		//만약 Tail이라면 완성된 World 값을 이용해서 Target Pos W를 구해야한다.
		public Vector2 _targetPosW = Vector2.zero;

		public float _lengthBoneToTarget = 0.0f;


		/// <summary>
		/// 계산 전의 벡터 각도(World). 
		/// 순수하게 Pos로만 계산된 각도이다.
		/// 계산 후에는 -90를 해서 Bone에 넣어줘야한다. (이 값은 -90 처리를 하지 않았다)
		/// </summary>
		public float _angleWorld_Prev = 0.0f;


		/// <summary>
		/// 계산 후의 벡터 각도(Parent 대비 상대값). 
		/// 순수하게 Pos로만 계산된 각도이다.
		/// 기본 연산에 의한 결과
		/// </summary>
		public float _angleLocal_Next = 0.0f;


		public float _angleWorld_Next = 0.0f;
		public float _angleLocal_Prev = 0.0f;//<<이건 계산용 값이다. 업데이트시에 매번 바뀜
		public float _angleLocal_Delta = 0.0f;//<<이건 계산용 값이다. 업데이트시에 매번 바뀜


		public bool _isAngleContraint = false;

		public float _angleParentToBase_Offset = 0.0f;
		public float _angleDir_Preferred = 0.0f;
		public float _angleDir_Lower = 0.0f;
		public float _angleDir_Upper = 0.0f;
		//public bool _isAngleDir_Plus = true;

		public bool _isPreferredAngleAdapted = false;


		// Functions
		//-------------------------------------------------
		public apBoneIKChainUnit(apBone baseBone, apBone targetBone, apBone parentBone)
		{
			_baseBone = baseBone;
			_targetBone = targetBone;
			_parentBone = parentBone;

			_parentChainUnit = null;
			_childChainUnit = null;

			_isPreferredAngleAdapted = false;
		}


		public void SetParent(apBoneIKChainUnit parentUnit)
		{
			_parentChainUnit = parentUnit;
		}

		public void SetChild(apBoneIKChainUnit childUnit)
		{
			_childChainUnit = childUnit;
		}

		public void SetTail(apBoneIKChainUnit tailChainUnit)
		{
			_tailChainUnit = tailChainUnit;
		}


		private float Lerp(float A, float B, float itp, float length)
		{
			itp = Mathf.Clamp(itp, 0.0f, length);
			return (A * (length - itp) + B * itp) / length;
		}

		// Update
		//-----------------------------------------------------------
		/// <summary>
		/// IK 시뮬레이션하기전에 호출하는 함수
		/// 위치값을 모두 넣어주고, baseBone의 설정을 복사한다.
		/// 이 함수를 호출한 후, Head에서 CalculateWorldRecursive를 호출하자
		/// </summary>
		public void ReadyToSimulate()
		{
			_isAngleContraint = _baseBone._isIKAngleRange;
			
			//if(string.Equals(_baseBone._name, "Bone Leg L 1"))
			//{
			//	Debug.Log("Bone Leg L 1 : ReadyToSimulate");
			//	Debug.Log("Default : " + _baseBone._defaultMatrix._angleDeg + " > +- 180 : " + defaultAngle180);
			//	Debug.Log("Lower : " + _baseBone._IKAngleRange_Lower + " > : " + _angleDir_Lower);
			//	Debug.Log("Upper : " + _baseBone._IKAngleRange_Upper + " > : " + _angleDir_Upper);
			//}
			
			
			//-180 ~ 180 이내로 바꿀 경우 -> Lower , Preferred , Upper 순서가 맞지 않을 수 있다.
			//while(_angleDir_Lower > _angleDir_Preferred)
			//{
			//	_angleDir_Lower -= 360.0f;
			//}

			//while(_angleDir_Upper < _angleDir_Preferred)
			//{
			//	_angleDir_Upper += 360.0f;
			//}


			//현재의 Bone의 Pos World를 이용해서 Local 정보를 만들자

			_bonePosW = _baseBone._worldMatrix._pos;
			_targetPosW = _targetBone._worldMatrix._pos;

			
			//if(string.Equals(_baseBone._name, "Bone Leg L 1"))
			//{
			//	Debug.Log("Bone Leg L 1 : ReadyToSimulate");
			//	Debug.Log("Default Angle : " + _baseBone._defaultMatrix._angleDeg);
			//	Debug.Log("Preferred : " + _angleDir_Preferred);
			//	Debug.Log("ConstAngle_Lower : " + _angleDir_Lower);
			//	Debug.Log("ConstAngle_Upper : " + _angleDir_Upper);
			//}

			if (_parentBone != null)
			{
				_parentPosW = _parentBone._worldMatrix._pos;

				//Angle : Parent -> Base
				_angleWorld_Parent = Vector2Angle(_bonePosW - _parentPosW);

				//Parent에서 Base로의 상대 각도가 Default와 같은지 체크
				//Offset만큼 위치가 맞지 않는다.
				_angleParentToBase_Offset = apUtil.AngleTo180((_angleWorld_Parent - _parentBone._worldMatrix._angleDeg) - 90);
				//if (string.Equals(_baseBone._name, "Bone Leg L 1"))
				//{
				//	Debug.Log("Parent Bone is Exist : " + _parentBone._name);
				//	Debug.Log("Parent World Angle : " + _angleWorld_Parent);
				//	Debug.Log("Offset : " + _angleParentToBase_Offset);
				//}
			}
			else
			{
				_parentPosW = Vector2.zero;
				//Parent가 없다면 AngleConstraint를 할 수 없다.
				_isAngleContraint = false;
				_angleWorld_Parent = 0.0f;
				_angleParentToBase_Offset = 0.0f;

				//if(string.Equals(_baseBone._name, "Bone Leg L 1"))
				//{
				//	Debug.Log("Parent Bone is Not Exist");
				//}
			}

			float defaultAngle180 = apUtil.AngleTo180(_baseBone._defaultMatrix._angleDeg - _angleParentToBase_Offset);
			_angleDir_Preferred = defaultAngle180 + _baseBone._IKAnglePreferred;
			_angleDir_Lower = defaultAngle180 + _baseBone._IKAngleRange_Lower;
			_angleDir_Upper = defaultAngle180 + _baseBone._IKAngleRange_Upper;



			_lengthBoneToTarget = Vector2.Distance(_targetPosW, _bonePosW);
			_angleWorld_Prev = Vector2Angle(_targetPosW - _bonePosW);
			_angleWorld_Next = _angleWorld_Prev;

			_angleLocal_Next = _angleWorld_Next - _angleWorld_Parent;
		}


		/// <summary>
		/// 현재 호출한 Bone Unit을 시작으로 Tail 방향으로 World를 갱신한다.
		/// Parent의 PosW, AngleWorld가 갱신되었어야 한다.
		/// IK의 핵심이 되는 _angleLocal_Next가 계산된 상태여야 한다.
		/// </summary>
		public void CalculateWorldRecursive()
		{
			if (_parentChainUnit != null)
			{
				//Parent 기준으로 Pos를 갱신한다.
				_parentPosW = _parentChainUnit._bonePosW;
				_angleWorld_Parent = _parentChainUnit._angleWorld_Next;

				_bonePosW.x = _parentPosW.x + _parentChainUnit._lengthBoneToTarget * Mathf.Cos(_angleWorld_Parent * Mathf.Deg2Rad);
				_bonePosW.y = _parentPosW.y + _parentChainUnit._lengthBoneToTarget * Mathf.Sin(_angleWorld_Parent * Mathf.Deg2Rad);
			}

			//Local Angle에 따라 World Angle을 갱신한다.
			_angleWorld_Next = _angleLocal_Next + _angleWorld_Parent;

			//Child Unit도 같이 갱신해주자
			if (_childChainUnit != null)
			{
				_childChainUnit.CalculateWorldRecursive();
			}
			else
			{
				//엥 여기가 Tail인가염
				_targetPosW.x = _bonePosW.x + _lengthBoneToTarget * Mathf.Cos(_angleWorld_Next * Mathf.Deg2Rad);
				_targetPosW.y = _bonePosW.y + _lengthBoneToTarget * Mathf.Sin(_angleWorld_Next * Mathf.Deg2Rad);
			}
		}



		/// <summary>
		/// IK를 요청한다.
		/// </summary>
		/// <param name="requestIKPosW"></param>
		/// <param name="isContinuous"></param>
		/// <returns></returns>
		public void RequestIK(Vector2 requestIKPosW, bool isContinuous
								//, int loopCount, int totalCount
								)
		{
			
			_angleLocal_Prev = _angleLocal_Next;

			//회전해야하는 World 각도
			float angleIK_Bone2IKtarget = Vector2Angle(requestIKPosW - _bonePosW);
			float angleIK_Bone2Tail = Vector2Angle(_tailChainUnit._targetPosW - _bonePosW);

			//현재 각도에서 빼자
			float angleIK_Delta = angleIK_Bone2IKtarget - angleIK_Bone2Tail;

			
			//Local에 더해주자
			_angleLocal_Next = apUtil.AngleTo360(_angleLocal_Next + angleIK_Delta);

			//Angle Constraint에 걸리나
			if (_isAngleContraint)
			{
				//if (string.Equals(_baseBone._name, "Bone Leg L 1"))
				//{
				//	Debug.Log("Next Angle : " + _angleLocal_Next);
				//	Debug.Log("Next Angle + Offset : " + (_angleLocal_Next + _angleParentToBase_Offset));
				//}
				

				if (_angleLocal_Next < _angleDir_Lower)
				{
					_angleLocal_Next = _angleDir_Lower;

					//if (string.Equals(_baseBone._name, "Bone Leg L 1"))
					//{
					//	Debug.LogError("Request IK [Bone Leg L 1] - Lower");
					//	Debug.Log("Next Angle : " + _angleLocal_Next + " (" + _angleDir_Lower + "  ~  " + _angleDir_Upper + " <p : " + _angleDir_Preferred + ">)");
					//}
				}
				else if (_angleLocal_Next > _angleDir_Upper)
				{
					_angleLocal_Next = _angleDir_Upper;

					//if (string.Equals(_baseBone._name, "Bone Leg L 1"))
					//{
					//	Debug.LogError("Request IK [Bone Leg L 1] - Upper");
					//	Debug.Log("Next Angle : " + _angleLocal_Next + " (" + _angleDir_Lower + "  ~  " + _angleDir_Upper + " <p : " + _angleDir_Preferred + ">)");
					//}
				}
			}

			//연속적인 처리에서 너무 차이가 크다면 IK가 점프해버리는 문제가 있다.
			//if(isContinuous)
			//{	
			//	if(Mathf.Abs(_angleLocal_Prev - _angleLocal_Next) > 90.0f)
			//	{
			//		_angleLocal_Next = _angleLocal_Prev;
			//	}
			//}

			//이후에 Calculate를 외부에서 호출해주자
			_angleLocal_Delta = _angleLocal_Next - _angleLocal_Prev;
		}


		// 계산 함수들
		//----------------------------------------------------------------------------------
		/// <summary>
		/// Dir Vector의 Angle (Degree)를 리턴한다.
		/// </summary>
		/// <param name="dirVec"></param>
		/// <returns></returns>
		private static float Vector2Angle(Vector2 dirVec)
		{
			return Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
		}

		/// <summary>
		/// 두개의 좌표계에서, Origin Pos를 기준으로 Target Pos를 회전하고, 그 위치를 리턴한다.
		/// 각도는 변환 뒤의 절대값이다. (Degree)
		/// </summary>
		/// <param name="originPos"></param>
		/// <param name="targetPos"></param>
		/// <param name="nextAngle"></param>
		/// <returns></returns>
		private static Vector2 RotateAngle(Vector2 originPos, Vector2 targetPos, float nextAngle)
		{
			float dist = Vector2.Distance(targetPos, originPos);

			return new Vector2(originPos.x + dist * Mathf.Cos(nextAngle * Mathf.Deg2Rad),
								originPos.y + dist * Mathf.Sin(nextAngle * Mathf.Deg2Rad)
								);
		}




	}

}