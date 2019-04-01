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

	[Serializable]
	public class apOptParamSet
	{
		// Members
		//--------------------------------------------
		[NonSerialized]
		public apOptParamSetGroup _parentParamSetGroup = null;

		//1. Controller Param에 동기화될 때
		public apControlParam SyncControlParam { get { return _parentParamSetGroup._keyControlParam; } }

		//public bool _conSyncValue_Bool = false;
		public int _conSyncValue_Int = 0;
		public float _conSyncValue_Float = 0.0f;
		public Vector2 _conSyncValue_Vector2 = Vector2.zero;
		//public Vector3 _conSyncValue_Vector3 = Vector3.zero;
		//public Color _conSyncValue_Color = Color.black;

		//<추가>
		//2. Keyframe에 동기화될 때
		//Bake때는 ID만 받고, 첫 시작시 Link를 한다.
		public int _keyframeUniqueID = -1;

		[NonSerialized]
		private apAnimKeyframe _syncKeyframe = null;
		public apAnimKeyframe SyncKeyframe { get { return _syncKeyframe; } }



		//추가
		//Control Param 타입에 한해서
		//ParamSet의 Weight를 100%이 아닌 일부로 둘 수 있다.
		//그럼 Overlap 되는 ParamSetGroup의 Weight를 바꿀 수 있다.
		//기존 : [ParamSetGroup Weight]로 보간 Weight 지정
		//변경 : [ParamSetGroup Weight x ParamSet Weight의 가중치합(0~1)]으로 보간 Weight 지정
		//이름은 OverlapWeight로 한다.
		//기본값은 1. Control Param 동기화 타입이 아니라면 이 값은 사용되지 않는다.
		[SerializeField]
		public float _overlapWeight = 1.0f;


		public int _nMeshData = 0;
		public int _nBoneData = 0;

		[SerializeField]
		public List<apOptModifiedMesh> _meshData = new List<apOptModifiedMesh>();

		[SerializeField]
		public List<apOptModifiedBone> _boneData = new List<apOptModifiedBone>();


		// Init
		//--------------------------------------------
		public apOptParamSet()
		{

		}

		public void LinkParamSetGroup(apOptParamSetGroup paramSetGroup, apPortrait portrait)
		{
			_parentParamSetGroup = paramSetGroup;

			_syncKeyframe = null;
			if (_keyframeUniqueID >= 0)
			{
				//TODO
				//_syncKeyframe = 
				if (paramSetGroup._keyAnimTimelineLayer != null)
				{
					_syncKeyframe = paramSetGroup._keyAnimTimelineLayer.GetKeyframeByID(_keyframeUniqueID);
				}
			}


			for (int i = 0; i < _meshData.Count; i++)
			{
				_meshData[i].Link(portrait);
			}

			//TODO : OptBone은 현재 Link할 객체가 없다.
			//필요하다면 Link를 여기에 추가해주자

		}

		public void BakeModifierParamSet(apModifierParamSet srcParamSet, apPortrait portrait)
		{
			//switch (srcParamSet._syncTarget)
			//{
			//	case apModifierParamSet.SYNC_TARGET.Static:
			//		_syncTarget = SYNC_TARGET.Static;
			//		break;

			//	case apModifierParamSet.SYNC_TARGET.Controller:
			//		_syncTarget = SYNC_TARGET.Controller;
			//		break;

			//	case apModifierParamSet.SYNC_TARGET.KeyFrame:
			//		_syncTarget = SYNC_TARGET.KeyFrame;
			//		break;

			//	default:
			//		Debug.LogError("연동 에러 : ParamSet에 정의되지 않은 타입 : " + srcParamSet._syncTarget);
			//		break;
			//}

			//_controlKeyName = srcParamSet._controlKeyName;

			//_conSyncValue_Bool = srcParamSet._conSyncValue_Bool;
			_conSyncValue_Int = srcParamSet._conSyncValue_Int;
			_conSyncValue_Float = srcParamSet._conSyncValue_Float;
			_conSyncValue_Vector2 = srcParamSet._conSyncValue_Vector2;
			//_conSyncValue_Vector3 = srcParamSet._conSyncValue_Vector3;
			//_conSyncValue_Color = srcParamSet._conSyncValue_Color;


			_keyframeUniqueID = srcParamSet._keyframeUniqueID;
			_syncKeyframe = null;

			_overlapWeight = srcParamSet._overlapWeight;//OverlapWeight를 집어넣자

			_meshData.Clear();
			_boneData.Clear();


			//SrcModifier ParamSet의 ModMesh, ModBone을 Bake해주자
			//Debug.LogError("TODO : Bone 데이터 연동");
			for (int i = 0; i < srcParamSet._meshData.Count; i++)
			{
				apModifiedMesh srcModMesh = srcParamSet._meshData[i];
				apOptModifiedMesh optModMesh = new apOptModifiedMesh();
				bool isResult = optModMesh.Bake(srcModMesh, portrait);
				if (isResult)
				{
					_meshData.Add(optModMesh);
				}
			}

			//추가 : ModBone
			for (int i = 0; i < srcParamSet._boneData.Count; i++)
			{
				apModifiedBone srcModBone = srcParamSet._boneData[i];
				apOptModifiedBone optModBone = new apOptModifiedBone();
				bool isResult = optModBone.Bake(srcModBone, portrait);
				if (isResult)
				{
					_boneData.Add(optModBone);
				}
			}
		}

		// Functions
		//--------------------------------------------


		// Get / Set
		//--------------------------------------------

		//public string ControlParamValue
		//{
		//	get
		//	{
		//		if (_controlParam == null)
		//		{
		//			return "<no-control type>";
		//		}

		//		switch (_controlParam._valueType)
		//		{
		//			case apControlParam.TYPE.Bool: return _conSyncValue_Bool.ToString();
		//			case apControlParam.TYPE.Int: return _conSyncValue_Int.ToString();
		//			case apControlParam.TYPE.Float: return _conSyncValue_Float.ToString();
		//			case apControlParam.TYPE.Vector2: return _conSyncValue_Vector2.ToString();
		//			case apControlParam.TYPE.Vector3: return _conSyncValue_Vector3.ToString();
		//			case apControlParam.TYPE.Color: return _conSyncValue_Color.ToString();
		//		}
		//		return "<unknown type>";
		//	}
		//}
	}

}