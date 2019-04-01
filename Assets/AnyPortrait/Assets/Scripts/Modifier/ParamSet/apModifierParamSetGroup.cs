﻿/*
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
	/// Modifier에서 ParamSet 중 비슷한 것끼리 묶어서 처리하는 클래스
	/// Editor에서 - 작업용 기능을 제공하고, UI의 편의성을 높이는 기능을 제공한다.
	/// Realtime에서 - matrix 계산시 도와주도록 한다.
	/// Serailize는 안되는 참조용 클래스 > 수정 : Serialize가 되며 레이어 값을 가진다.
	/// </summary>
	[Serializable]
	public class apModifierParamSetGroup
	{
		// Members
		//-------------------------------------------------
		[NonSerialized]
		public apPortrait _portrait = null;

		[NonSerialized]
		public apModifierBase _parentModifier = null;

		//어떤 값에 의해서 영향을 받는가
		public enum SYNC_TARGET
		{
			/// <summary>고정값. 세팅한 값이 그대로 적용된다. Input이 없어서 1개의 ParamSetGroup만 사용한다. (Rigging 등)</summary>
			Static = 0,
			/// <summary>컨트롤러에 의해 적용 수준이 결정된다. 복수개의 ParamSet이 있어야 보간이 된다.</summary>
			Controller = 1,
			/// <summary>키프레임에 의해 적용 수준이 결정된다. 이 값은 모션 데이터의 KeyFrame에 저장되며, Modifier와 연결된다.</summary>
			KeyFrame = 2,
			/// <summary>컨트롤러에 의해 적용 수준이 결정되지만 값은 Static하게 고정되며 자동으로 보간이 이루어진다. [키 추가 불가]</summary>
			ControllerWithoutKey = 3,
			/// <summary>본에 연동된다. TF류 모디파이어와 유사하게 작동됨</summary>
			Bones = 4,
		}

		//Sync 값은 하위의 모든 ParamSet과 동일하다.
		public SYNC_TARGET _syncTarget = SYNC_TARGET.Static;

		//타겟의 어떤 값에 연동할 것인가
		//1. None.. 없다. 그냥 고정값

		//2. Controller -> Controller Param
		//Controller 방식일때
		//public string _keyControlParamName = "";
		public int _keyControlParamID = -1;

		[NonSerialized]
		public apControlParam _keyControlParam = null;

		//3. KeyFrame으로 정의될 때
		//ParamSetGroup은 AnimClip / AnimTimeline / AnimTimelineLayer까지 저장한다. (여기까지가 전부 Key다)
		//ParamSet에서 AnimKeyframe을 저장
		public int _keyAnimClipID = -1;
		public int _keyAnimTimelineID = -1;
		public int _keyAnimTimelineLayerID = -1;

		[NonSerialized]
		public apAnimClip _keyAnimClip = null;

		[NonSerialized]
		public apAnimTimeline _keyAnimTimeline = null;

		[NonSerialized]
		public apAnimTimelineLayer _keyAnimTimelineLayer = null;



		[SerializeField]
		public List<apModifierParamSet> _paramSetList = new List<apModifierParamSet>();


		//해당 파라미터가 적용 중인지 체크한다.
		[SerializeField]
		public bool _isEnabled = true;

		//추가 3.22 : ExclusiveEnabled 처리 타입이 조금 더 상세해진다.
		//3가지 타입으로 바뀌며,
		//Transform / Color로 나뉜다.
		public enum MOD_EX_CALCULATE
		{
			/// <summary>Mod 계산 비활성</summary>
			Disabled,
			/// <summary>Mod 계산이 허용</summary>
			Enabled,
			/// <summary>선택된 "다른 Mod"에 속한 "객체"가 "아닌!" 경우에만 계산이 허용</summary>
			SubExEnabled,
		}

		//[NonSerialized]
		//public bool _isEnabledExclusive = true;//<<이건 전버전

		[NonSerialized]
		public MOD_EX_CALCULATE _modExType_Transform = MOD_EX_CALCULATE.Enabled;

		[NonSerialized]
		public MOD_EX_CALCULATE _modExType_Color = MOD_EX_CALCULATE.Enabled;

		//일단 이거 생략
		public bool IsCalculateEnabled
		{
			get
			{
				//return _isEnabled && _isEnabledExclusive;
				return _isEnabled && 
					(_modExType_Transform != MOD_EX_CALCULATE.Disabled || _modExType_Color != MOD_EX_CALCULATE.Disabled);
			}
		}

		public bool IsExCalculatable_Transform { get { return _modExType_Transform != MOD_EX_CALCULATE.Disabled; } }
		public bool IsExCalculatable_Color {  get { return _modExType_Color != MOD_EX_CALCULATE.Disabled; } }

		





		// 추가 - 직렬화 + 레이어
		// [이 레이어값은 "Animated가 아닌" 모디파이어에서만 적용된다]
		public int _layerIndex = 0;//레이어 값이 낮을 수록 먼저 계산된다.
		public float _layerWeight = 0.0f;

		public enum BLEND_METHOD
		{
			/// <summary>기존 값을 유지하면서 변화값을 덮어 씌운다.</summary>
			Additive = 0,
			/// <summary>기존 값과 선형 보간을 하며 덮어씌운다.</summary>
			Interpolation = 1
		}

		public BLEND_METHOD _blendMethod = BLEND_METHOD.Interpolation;

		//추가 : 
		/// <summary>
		/// Color/Visible을 제외하는 Modifier라 할지라도 ParamSetGroup에서 색상 옵션이 꺼져있으면 색상이 계산되지 않는다.
		/// </summary>
		[SerializeField]
		public bool _isColorPropertyEnabled = true;

		//중요!
		//TODO : Animated Modifier에서는 리얼타임으로 "AnimClip을 호출될 때의 Blend + Layer 정보"를 그대로 받아서 쓴다.


		//추가 : Vertex Work Weight
		//Transform-Mesh에서 작업한 VertMorph 내용이 100% 적용되는건 아니다
		//전체 중에서 "일부 Vertex만 적용"할 수 있도록 별도의 VertexWeight를 저장한다. (Layer가 0이 아닐 경우 적용)
		//값은 저장되며, 리스트 순서는 Vertex ID와 Index를 기준으로 배열 순서를 잘 맞춘다.
		//정렬은 자동이다.


		//현재 파라미터들에 공통적으로 적용된 Transform들을 저장한다.
		//뭔가 변동사항이 생기면 Refresh하자

		[NonSerialized]
		public List<apTransform_Mesh> _syncTransform_Mesh = new List<apTransform_Mesh>();

		[NonSerialized]
		public List<apTransform_MeshGroup> _syncTransform_MeshGroup = new List<apTransform_MeshGroup>();

		[NonSerialized]
		public List<apBone> _syncBone = new List<apBone>();



		//추가
		//ParamSet List + 타겟 Mesh Transform에 대해서
		//일부 Vertex만 가중치를 줘서 Modifier를 적용할 수 있도록 리스트를 만든다.
		//MeshTransform에 맞게 구분할 수 있다.
		//CaculateParamKeyValue에도 연동을 해준다.
		[SerializeField]
		public List<apModifierParamSetGroupVertWeight> _calculatedWeightedVertexList = new List<apModifierParamSetGroupVertWeight>();

		//Editor 제어를 위한 apLinkedMatrix
		[NonSerialized]
		public apMatrix _tmpMatrix = new apMatrix();

		[NonSerialized]
		public Vector2[] _tmpPositions = null;

		[NonSerialized]
		public apMatrix3x3[] _tmpVertMatrices = null;//<<Rigging의 경우 Vert마다 Matrix가 필요하다


		//[NonSerialized]
		//private apCalculatedLog _calLog = null;

		//public apCalculatedLog CalculatedLog
		//{
		//	get
		//	{
		//		if (_calLog == null)
		//		{ _calLog = new apCalculatedLog(this); }
		//		return _calLog;
		//	}
		//}


		// Init
		//-------------------------------------------------
		/// <summary>
		/// 백업용 생성자. 코드에서는 사용하지 말것
		/// </summary>
		public apModifierParamSetGroup()
		{

		}

		public apModifierParamSetGroup(apPortrait portrait, apModifierBase parentModifier, int layerIndex)
		{
			LinkPortrait(portrait, parentModifier);

			_layerIndex = layerIndex;
			_layerWeight = 1.0f;
			//_blendMethod = BLEND_METHOD.Interpolation;
			_blendMethod = BLEND_METHOD.Additive;//<<기본값을 Additive로 변경

		}

		public void LinkPortrait(apPortrait portrait, apModifierBase parentModifier)
		{
			_portrait = portrait;
			_parentModifier = parentModifier;

			if (_tmpMatrix == null)
			{
				_tmpMatrix = new apMatrix();
			}

			//if (_tmpPositions == null)
			//{
			//	_tmpPositions = new List<Vector2>();
			//}

			//if(_tmpVertMatrices == null)
			//{
			//	_tmpVertMatrices = new List<apMatrix3x3>();
			//}
		}



		public void SetStatic()
		{
			_syncTarget = SYNC_TARGET.Static;
		}

		public void SetController(apControlParam controlParam)
		{
			_syncTarget = SYNC_TARGET.Controller;

			_keyControlParam = controlParam;
			//_keyControlParamName = controlParam._keyName;//Name은 사용하지 않는다.
			_keyControlParamID = controlParam._uniqueID;//ID 저장으로 변경

		}

		//TODO : 이걸 호출해야한다.
		public void SetTimeline(apAnimClip animClip, apAnimTimeline timeline, apAnimTimelineLayer timelineLayer)
		{
			_syncTarget = SYNC_TARGET.KeyFrame;
			_keyAnimClipID = animClip._uniqueID;
			_keyAnimClip = animClip;

			_keyAnimTimelineID = timeline._uniqueID;
			_keyAnimTimeline = timeline;

			_keyAnimTimelineLayerID = timelineLayer._uniqueID;
			_keyAnimTimelineLayer = timelineLayer;
		}



		// Functions
		//-------------------------------------------------
		public void RemoveInvalidParamSet()
		{
			for (int i = 0; i < _paramSetList.Count; i++)
			{
				apModifierParamSet paramSet = _paramSetList[i];

				int nRemoveModMesh = paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
				{
					if (a._meshGroupOfModifier == null) { return true; }
					if (a._meshGroupOfTransform == null) { return true; }
					if (a._transform_Mesh == null && a._transform_MeshGroup == null) { return true; }
					return false;
				});

				if (nRemoveModMesh > 0)
				{
					//Debug.LogError("RemoveInvalidParamSet ModMesh 삭제됨 : " + nRemoveModMesh);
				}

				//Debug.LogError("TODO : RemoveInvalidParamSet : 유효하지 않는 Bone을 결정해야한다.");
				int nRemoveModBone = paramSet._boneData.RemoveAll(delegate (apModifiedBone a)
				{
					return a._bone == null;//4.4 변경
				});

				if(nRemoveModBone > 0)
				{
					//Debug.LogError("RemoveInvalidParamSet ModBone 삭제됨 : " + nRemoveModMesh);
				}

			}
			//_paramSetList.RemoveAll(delegate (apModifierParamSet a)
			//{
			//	bool isAnyData = false;
			//	if(a._meshData.Count != 0)	{ isAnyData = true; }
			//	if(a._boneData.Count != 0)	{ isAnyData = true; }

			//	return !isAnyData;//데이터가 없다면 삭제
			//});
		}

		//public void ClearParamSet()
		//{
		//	_paramSetList.Clear();
		//}

		//public void AddParamSet(apModifierParamSet paramSet)
		//{
		//	_paramSetList.Add(paramSet);

		//	RefreshSync();
		//}

		public void SortParamSet()
		{
			if (_keyControlParam != null)
			{
				_paramSetList.Sort(delegate (apModifierParamSet a, apModifierParamSet b)
				{
					switch (_keyControlParam._valueType)
					{
					//case apControlParam.TYPE.Bool:
					//	return 0;

					case apControlParam.TYPE.Int:
							return a._conSyncValue_Int - b._conSyncValue_Int;

						case apControlParam.TYPE.Float:
							return (int)((a._conSyncValue_Float - b._conSyncValue_Float) * 1000.0f);

						case apControlParam.TYPE.Vector2:
							if (Mathf.Abs(a._conSyncValue_Vector2.y - b._conSyncValue_Vector2.y) < 0.001f)
							{
								return (int)((a._conSyncValue_Vector2.x - b._conSyncValue_Vector2.x) * 1000.0f);
							}
							else
							{
								return (int)((a._conSyncValue_Vector2.y - b._conSyncValue_Vector2.y) * 1000.0f);
							}

						//case apControlParam.TYPE.Vector3:
						//	if (Mathf.Abs(a._conSyncValue_Vector3.z - b._conSyncValue_Vector3.z) < 0.001f)
						//	{
						//		if (Mathf.Abs(a._conSyncValue_Vector3.y - b._conSyncValue_Vector3.y) < 0.001f)
						//		{
						//			return (int)((a._conSyncValue_Vector3.x - b._conSyncValue_Vector3.x) * 1000.0f);
						//		}
						//		else
						//		{
						//			return (int)((a._conSyncValue_Vector3.y - b._conSyncValue_Vector3.y) * 1000.0f);
						//		}
						//	}
						//	else
						//	{
						//		return (int)((a._conSyncValue_Vector3.z - b._conSyncValue_Vector3.z) * 1000.0f);
						//	}

						//case apControlParam.TYPE.Color:
						//	return 0;
				}
					return 0;

				});

				//추가 : ParamSet의 Dist 보간을 위해서 "보간 영역"을 설정해줘야 한다.
				float rangeBias = 1000.0f;
				for (int iParam = 0; iParam < _paramSetList.Count; iParam++)
				{
					apModifierParamSet paramSet = _paramSetList[iParam];

					//일단 값 초기화 (영역 전체로 동기화를 하고 다른 값이 있으면 줄인다.)
					//if (_keyControlParam._isRange)
					//{
					//	switch (_keyControlParam._valueType)
					//	{
					//		case apControlParam.TYPE.Int:
					//			paramSet._conSyncValueRange_Under = new Vector3(_keyControlParam._int_Min, 0.0f, 0.0f);
					//			paramSet._conSyncValueRange_Over = new Vector3(_keyControlParam._int_Max, 0.0f, 0.0f);
					//			break;

					//		case apControlParam.TYPE.Float:
					//			paramSet._conSyncValueRange_Under = new Vector3(_keyControlParam._float_Min, 0.0f, 0.0f);
					//			paramSet._conSyncValueRange_Over = new Vector3(_keyControlParam._float_Max, 0.0f, 0.0f);
					//			break;

					//		case apControlParam.TYPE.Vector2:
					//			paramSet._conSyncValueRange_Under = new Vector3(_keyControlParam._vec2_Min.x, _keyControlParam._vec2_Min.y, 0.0f);
					//			paramSet._conSyncValueRange_Over = new Vector3(_keyControlParam._vec2_Max.x, _keyControlParam._vec2_Max.y, 0.0f);
					//			break;

					//		case apControlParam.TYPE.Vector3:
					//			paramSet._conSyncValueRange_Under = _keyControlParam._vec3_Min;
					//			paramSet._conSyncValueRange_Over = _keyControlParam._vec3_Max;
					//			break;
					//	}

					//	//Bias를 가감해줘야 이후 Weight 계산시 Range 경계에서 적절한 값을 가진다.
					//	paramSet._conSyncValueRange_Under -= Vector3.one * rangeBias;
					//	paramSet._conSyncValueRange_Over += Vector3.one * rangeBias;
					//}
					//else
					//{
					//	paramSet._conSyncValueRange_Under = -(Vector3.one * 1000000.0f);
					//	paramSet._conSyncValueRange_Over = (Vector3.one * 1000000.0f);
					//}

					switch (_keyControlParam._valueType)
					{
						case apControlParam.TYPE.Int:
							paramSet._conSyncValueRange_Under = new Vector3(_keyControlParam._int_Min, 0.0f, 0.0f);
							paramSet._conSyncValueRange_Over = new Vector3(_keyControlParam._int_Max, 0.0f, 0.0f);
							break;

						case apControlParam.TYPE.Float:
							paramSet._conSyncValueRange_Under = new Vector3(_keyControlParam._float_Min, 0.0f, 0.0f);
							paramSet._conSyncValueRange_Over = new Vector3(_keyControlParam._float_Max, 0.0f, 0.0f);
							break;

						case apControlParam.TYPE.Vector2:
							paramSet._conSyncValueRange_Under = new Vector3(_keyControlParam._vec2_Min.x, _keyControlParam._vec2_Min.y, 0.0f);
							paramSet._conSyncValueRange_Over = new Vector3(_keyControlParam._vec2_Max.x, _keyControlParam._vec2_Max.y, 0.0f);
							break;
					}

					//Bias를 가감해줘야 이후 Weight 계산시 Range 경계에서 적절한 값을 가진다.
					paramSet._conSyncValueRange_Under -= Vector2.one * rangeBias;
					paramSet._conSyncValueRange_Over += Vector2.one * rangeBias;




					//다른 값과 비교하여 Range를 정한다.
					//영역을 축소하는 방향으로
					for (int iNext = 0; iNext < _paramSetList.Count; iNext++)
					{
						apModifierParamSet nextParamSet = _paramSetList[iNext];
						if (nextParamSet == paramSet)
						{
							continue;
						}

						switch (_keyControlParam._valueType)
						{
							case apControlParam.TYPE.Int:
								{
									int nextValue_Int = nextParamSet._conSyncValue_Int;
									if (nextValue_Int <= paramSet._conSyncValue_Int)
									{
										//값이 작은 경우 + 영역보다 값이 큰 경우
										if (nextValue_Int > paramSet._conSyncValueRange_Under.x)
										{
											paramSet._conSyncValueRange_Under.x = nextValue_Int;
										}
									}
									if (nextValue_Int >= paramSet._conSyncValue_Int)
									{
										//값이 큰 경우 + 영역보다 값이 작은 경우
										if (nextValue_Int < paramSet._conSyncValueRange_Over.x)
										{
											paramSet._conSyncValueRange_Over.x = nextValue_Int;
										}
									}
								}
								break;

							case apControlParam.TYPE.Float:
								{
									float nextValue_float = nextParamSet._conSyncValue_Float;
									if (nextValue_float <= paramSet._conSyncValue_Float)
									{
										//값이 작은 경우 + 영역보다 값이 큰 경우
										if (nextValue_float > paramSet._conSyncValueRange_Under.x)
										{
											paramSet._conSyncValueRange_Under.x = nextValue_float;
										}
									}
									if (nextValue_float >= paramSet._conSyncValue_Float)
									{
										//값이 큰 경우 + 영역보다 값이 작은 경우
										if (nextValue_float < paramSet._conSyncValueRange_Over.x)
										{
											paramSet._conSyncValueRange_Over.x = nextValue_float;
										}
									}
								}
								break;

							case apControlParam.TYPE.Vector2:
								{
									Vector2 nextValue_Vec2 = nextParamSet._conSyncValue_Vector2;
									//X, Y에 대해서 값을 각각 처리한다.

									//X에 대해서 영역 처리할 땐 -> Y가 같은 경우에만
									//Y에 대해서 영역 처리할 땐 -> X가 같은 경우에만

									bool isXSame = Mathf.Abs(nextValue_Vec2.x - paramSet._conSyncValue_Vector2.x) < 0.01f;
									bool isYSame = Mathf.Abs(nextValue_Vec2.y - paramSet._conSyncValue_Vector2.y) < 0.01f;

									//Under - X
									if (nextValue_Vec2.x <= paramSet._conSyncValue_Vector2.x && isYSame)
									{
										//값이 작은 경우 + 영역보다 값이 큰 경우
										if (nextValue_Vec2.x > paramSet._conSyncValueRange_Under.x)
										{
											paramSet._conSyncValueRange_Under.x = nextValue_Vec2.x;
										}
									}

									//Under - Y
									if (nextValue_Vec2.y <= paramSet._conSyncValue_Vector2.y && isXSame)
									{
										//값이 작은 경우 + 영역보다 값이 큰 경우
										if (nextValue_Vec2.y > paramSet._conSyncValueRange_Under.y)
										{
											paramSet._conSyncValueRange_Under.y = nextValue_Vec2.y;
										}
									}

									//Over - X
									if (nextValue_Vec2.x >= paramSet._conSyncValue_Vector2.x && isYSame)
									{
										//값이 큰 경우 + 영역보다 값이 작은 경우
										if (nextValue_Vec2.x < paramSet._conSyncValueRange_Over.x)
										{
											paramSet._conSyncValueRange_Over.x = nextValue_Vec2.x;
										}
									}

									//Over - Y
									if (nextValue_Vec2.y >= paramSet._conSyncValue_Vector2.y && isXSame)
									{
										//값이 큰 경우 + 영역보다 값이 작은 경우
										if (nextValue_Vec2.y < paramSet._conSyncValueRange_Over.y)
										{
											paramSet._conSyncValueRange_Over.y = nextValue_Vec2.y;
										}
									}
								}
								break;

								//case apControlParam.TYPE.Vector3:
								//	{
								//		Vector3 nextValue_Vec3 = nextParamSet._conSyncValue_Vector3;
								//		//X, Y에 대해서 값을 각각 처리한다.

								//		//X에 대해서 영역 처리할 땐 -> Y가 같은 경우에만
								//		//Y에 대해서 영역 처리할 땐 -> X가 같은 경우에만
								//		//Z는 자유롭게

								//		bool isXSame = Mathf.Abs(nextValue_Vec3.x - paramSet._conSyncValue_Vector3.x) < 0.01f;
								//		bool isYSame = Mathf.Abs(nextValue_Vec3.y - paramSet._conSyncValue_Vector3.y) < 0.01f;

								//		//Under - X
								//		if(nextValue_Vec3.x <= paramSet._conSyncValue_Vector3.x && isYSame)
								//		{
								//			//값이 작은 경우 + 영역보다 값이 큰 경우
								//			if(nextValue_Vec3.x > paramSet._conSyncValueRange_Under.x)
								//			{
								//				paramSet._conSyncValueRange_Under.x = nextValue_Vec3.x;
								//			}
								//		}

								//		//Under - Y
								//		if(nextValue_Vec3.y <= paramSet._conSyncValue_Vector3.y && isXSame)
								//		{
								//			//값이 작은 경우 + 영역보다 값이 큰 경우
								//			if(nextValue_Vec3.y > paramSet._conSyncValueRange_Under.y)
								//			{
								//				paramSet._conSyncValueRange_Under.y = nextValue_Vec3.y;
								//			}
								//		}

								//		//Under - Z
								//		if(nextValue_Vec3.z <= paramSet._conSyncValue_Vector3.z)
								//		{
								//			//값이 작은 경우 + 영역보다 값이 큰 경우
								//			if(nextValue_Vec3.z > paramSet._conSyncValueRange_Under.z)
								//			{
								//				paramSet._conSyncValueRange_Under.z = nextValue_Vec3.z;
								//			}
								//		}

								//		//Over - X
								//		if(nextValue_Vec3.x >= paramSet._conSyncValue_Vector3.x && isYSame)
								//		{
								//			//값이 큰 경우 + 영역보다 값이 작은 경우
								//			if(nextValue_Vec3.x < paramSet._conSyncValueRange_Over.x)
								//			{
								//				paramSet._conSyncValueRange_Over.x = nextValue_Vec3.x;
								//			}
								//		}

								//		//Over - Y
								//		if(nextValue_Vec3.y >= paramSet._conSyncValue_Vector3.y && isXSame)
								//		{
								//			//값이 큰 경우 + 영역보다 값이 작은 경우
								//			if(nextValue_Vec3.y < paramSet._conSyncValueRange_Over.y)
								//			{
								//				paramSet._conSyncValueRange_Over.y = nextValue_Vec3.y;
								//			}
								//		}

								//		//Over - Z
								//		if(nextValue_Vec3.z >= paramSet._conSyncValue_Vector3.z)
								//		{
								//			//값이 큰 경우 + 영역보다 값이 작은 경우
								//			if(nextValue_Vec3.z < paramSet._conSyncValueRange_Over.z)
								//			{
								//				paramSet._conSyncValueRange_Over.z = nextValue_Vec3.z;
								//			}
								//		}
								//	}
								//	break;
						}
					}
				}
			}
		}

		public bool RefreshSync()
		{
			if (_syncTransform_Mesh == null)
			{
				_syncTransform_Mesh = new List<apTransform_Mesh>();
			}
			_syncTransform_Mesh.Clear();

			if (_syncTransform_MeshGroup == null)
			{
				_syncTransform_MeshGroup = new List<apTransform_MeshGroup>();
			}
			_syncTransform_MeshGroup.Clear();

			if (_syncBone == null)
			{
				_syncBone = new List<apBone>();
			}
			_syncBone.Clear();

			//한번이라도 등장한 MeshTransform / MeshGroup Transform을 찾자
			for (int i = 0; i < _paramSetList.Count; i++)
			{
				apModifierParamSet paramSet = _paramSetList[i];

				for (int iModMesh = 0; iModMesh < paramSet._meshData.Count; iModMesh++)
				{
					apModifiedMesh modMesh = paramSet._meshData[iModMesh];
					if (modMesh._transform_Mesh != null)
					{
						//수정 4.1 : Mesh가 없는 MeshTransform 
						if (modMesh._transform_Mesh._mesh == null)
						{
							//Debug.Log("AnyPortrait : Mesh of [" + modMesh._transform_Mesh._nickName + "] is removed. Please check it.");
						}
						else
						{
							if (!_syncTransform_Mesh.Contains(modMesh._transform_Mesh))
							{
								_syncTransform_Mesh.Add(modMesh._transform_Mesh);
							}
						}

						//이전 코드
						//if (!_syncTransform_Mesh.Contains(modMesh._transform_Mesh))
						//{
						//	_syncTransform_Mesh.Add(modMesh._transform_Mesh);
						//}
					}
					if (modMesh._transform_MeshGroup != null)
					{
						if (!_syncTransform_MeshGroup.Contains(modMesh._transform_MeshGroup))
						{
							_syncTransform_MeshGroup.Add(modMesh._transform_MeshGroup);
						}
					}
				}
				for (int iModBone = 0; iModBone < paramSet._boneData.Count; iModBone++)
				{
					apModifiedBone modBone = paramSet._boneData[iModBone];

					if (modBone._bone != null)
					{
						if (modBone._meshGroup_Bone != null && modBone._meshGroup_Bone._boneList_All.Contains(modBone._bone))
						{
							if (!_syncBone.Contains(modBone._bone))
							{
								_syncBone.Add(modBone._bone);
							}
						}
						else
						{
							//Modifier로 등록되었지만 Bone이 삭제되었다면 여기로 들어온다.
							//Sync가 안됨
						}
					}
				}
			}

			//동기화 전용 Sync Transform을 모든 ParamSet에 넣자
			bool isAnyChanged = false;

			for (int iSync = 0; iSync < _syncTransform_Mesh.Count; iSync++)
			{
				apTransform_Mesh meshTransform = _syncTransform_Mesh[iSync];
				//bool isAdd = _parentModifier.AddMeshTransformToAllParamSet(_parentModifier._meshGroup, meshTransform, false);
				for (int iParamSet = 0; iParamSet < _paramSetList.Count; iParamSet++)
				{
					bool isAdd = AddMeshTransformToParamSet(_paramSetList[iParamSet], meshTransform);
					if (isAdd)
					{
						isAnyChanged = true;
					}
				}

			}

			for (int iSync = 0; iSync < _syncTransform_MeshGroup.Count; iSync++)
			{
				apTransform_MeshGroup meshGroupTransform = _syncTransform_MeshGroup[iSync];
				for (int iParamSet = 0; iParamSet < _paramSetList.Count; iParamSet++)
				{
					bool isAdd = AddMeshGroupTransformToParamSet(_paramSetList[iParamSet], meshGroupTransform);
					//bool isAdd = _parentModifier.AddMeshGroupTransformToAllParamSet(_parentModifier._meshGroup, meshGroupTransform, false);
					if (isAdd)
					{
						isAnyChanged = true;
					}
				}
			}

			for (int iSync = 0; iSync < _syncBone.Count; iSync++)
			{
				apBone bone = _syncBone[iSync];

				for (int iParamSet = 0; iParamSet < _paramSetList.Count; iParamSet++)
				{
					bool isAdd = AddBoneToParamSet(_paramSetList[iParamSet], bone._meshGroup._rootMeshGroupTransform, bone);
					if (isAdd)
					{
						isAnyChanged = true;
					}
				}
			}

			//추가 : Sync시 WeightedVertex도 같이 처리해주자
			// 동기화 안된건 자동 삭제
			for (int i = 0; i < _calculatedWeightedVertexList.Count; i++)
			{
				_calculatedWeightedVertexList[i]._isSync = false;
			}

			for (int iSync = 0; iSync < _syncTransform_Mesh.Count; iSync++)
			{
				apTransform_Mesh meshTransform = _syncTransform_Mesh[iSync];
				apModifierParamSetGroupVertWeight existWV = _calculatedWeightedVertexList.Find(delegate (apModifierParamSetGroupVertWeight a)
				{
					return a._meshTransform_ID == meshTransform._transformUniqueID;
				});
				if (existWV != null)
				{
					existWV._isSync = true;
					existWV.LinkMeshTransform(meshTransform);
				}
				else
				{
					//없다. 새로 만들자
					apModifierParamSetGroupVertWeight newVW = new apModifierParamSetGroupVertWeight(meshTransform);
					newVW._isSync = true;
					_calculatedWeightedVertexList.Add(newVW);
				}
			}

			//동기화 되지 않은건 지운다.
			_calculatedWeightedVertexList.RemoveAll(delegate (apModifierParamSetGroupVertWeight a)
			{
				return !a._isSync;
			});

			return isAnyChanged;
		}


		// 중요!
		//Mesh Transform / MeshGroup Transform을 각각의 ParamSet에 넣어준다.
		//Modifier 조건에 맞게 처리한다.
		private bool AddMeshTransformToParamSet(apModifierParamSet paramSet, apTransform_Mesh meshTransform)
		{
			//if(_parentModifier.ModifiedTargetType != apModifiedMesh.TARGET_TYPE.MeshTransformOnly &&
			//	_parentModifier.ModifiedTargetType != apModifiedMesh.TARGET_TYPE.VertexWithMeshTransform)
			//{
			//	//추가할 수 없다.
			//	return false;
			//}
			if (!_parentModifier.IsTarget_MeshTransform)
			{
				return false;
			}
			
			bool isExist = paramSet._meshData.Exists(delegate (apModifiedMesh a)
			{
				return a.IsContains_MeshTransform(_parentModifier._meshGroup, meshTransform, meshTransform._mesh);
			});

			if (!isExist)
			{
				apRenderUnit targetRenderUnit = _parentModifier._meshGroup.GetRenderUnit(meshTransform);
				if (targetRenderUnit != null)
				{
					apMeshGroup parentMeshGroupOfTransform = GetParentMeshGroupOfMeshTransform(meshTransform);
					if (parentMeshGroupOfTransform == null)
					{
						//Parent MeshGroup이 없네염
						return false;
					}

					apModifiedMesh modMesh = new apModifiedMesh();




					modMesh.Init(_parentModifier._meshGroup._uniqueID, parentMeshGroupOfTransform._uniqueID, _parentModifier.ModifiedValueType);

					modMesh.SetTarget_MeshTransform(meshTransform._transformUniqueID, meshTransform._mesh._uniqueID, meshTransform._meshColor2X_Default, meshTransform._isVisible_Default);
					modMesh.Link_MeshTransform(_parentModifier._meshGroup, parentMeshGroupOfTransform, meshTransform, targetRenderUnit, _portrait);

					#region [미사용 코드] 확장성 있는 코드로 변경했다.
					//if (_parentModifier.ModifiedTargetType == apModifiedMesh.TARGET_TYPE.VertexWithMeshTransform)
					//{
					//	modMesh.Init_VertexMorph(_parentModifier._meshGroup._uniqueID,
					//		meshTransform._transformUniqueID,
					//		meshTransform._mesh._uniqueID);

					//	modMesh.Link_VertexMorph(_parentModifier._meshGroup, meshTransform, targetRenderUnit);
					//}
					//else if (_parentModifier.ModifiedTargetType == apModifiedMesh.TARGET_TYPE.MeshTransformOnly)
					//{
					//	modMesh.Init_MeshTransform(_parentModifier._meshGroup._uniqueID,
					//		meshTransform._transformUniqueID,
					//		meshTransform._mesh._uniqueID);

					//	modMesh.Link_MeshTransform(_parentModifier._meshGroup, meshTransform, targetRenderUnit);
					//} 
					#endregion

					paramSet._meshData.Add(modMesh);
				}
			}

			return !isExist;
		}

		private bool AddMeshGroupTransformToParamSet(apModifierParamSet paramSet, apTransform_MeshGroup meshGroupTransform)
		{
			//if(_parentModifier.ModifiedTargetType != apModifiedMesh.TARGET_TYPE.MeshGroupTransformOnly)
			//{
			//	return false;
			//}
			if (!_parentModifier.IsTarget_MeshGroupTransform)
			{
				return false;
			}

			bool isExist = paramSet._meshData.Exists(delegate (apModifiedMesh a)
			{
				return a.IsContains_MeshGroupTransform(_parentModifier._meshGroup, meshGroupTransform);
			});

			if (!isExist)
			{
				apRenderUnit targetRenderUnit = _parentModifier._meshGroup.GetRenderUnit(meshGroupTransform);
				if (targetRenderUnit != null)
				{
					apMeshGroup parentMeshGroupOfTransform = GetParentMeshGroupOfMeshGroupTransform(meshGroupTransform);
					if (parentMeshGroupOfTransform == null)
					{
						//Parent MeshGroup이 없네염
						return false;
					}

					apModifiedMesh modMesh = new apModifiedMesh();
					modMesh.Init(_parentModifier._meshGroup._uniqueID, parentMeshGroupOfTransform._uniqueID, _parentModifier.ModifiedValueType);
					modMesh.SetTarget_MeshGroupTransform(meshGroupTransform._transformUniqueID, meshGroupTransform._meshColor2X_Default, meshGroupTransform._isVisible_Default);

					//modMesh.Init_MeshGroupTransform(_parentModifier._meshGroup._uniqueID,
					//		meshGroupTransform._transformUniqueID);

					modMesh.Link_MeshGroupTransform(_parentModifier._meshGroup, parentMeshGroupOfTransform, meshGroupTransform, targetRenderUnit);
					paramSet._meshData.Add(modMesh);
				}
			}

			return !isExist;
		}



		private bool AddBoneToParamSet(apModifierParamSet paramSet, apTransform_MeshGroup meshGroupTransform, apBone bone)
		{
			if (!_parentModifier.IsTarget_Bone)
			{
				return false;
			}

			bool isExist = paramSet._boneData.Exists(delegate (apModifiedBone a)
			{
				return a._bone == bone;
			});

			if (!isExist)
			{
				apMeshGroup parentMeshGroupOfBone = GetParentMeshGroupOfBone(bone);
				if (parentMeshGroupOfBone == null)
				{
					return false;
				}

				apRenderUnit targetRenderUnit = _parentModifier._meshGroup.GetRenderUnit(meshGroupTransform);

				apModifiedBone modBone = new apModifiedBone();
				modBone.Init(_parentModifier._meshGroup._uniqueID, parentMeshGroupOfBone._uniqueID, meshGroupTransform._transformUniqueID, bone);
				modBone.Link(_parentModifier._meshGroup, parentMeshGroupOfBone, bone, targetRenderUnit, meshGroupTransform);


				paramSet._boneData.Add(modBone);
			}

			return !isExist;
		}

		/// <summary>
		/// 처리를 도와주는 함수. MeshTransform의 Parent MeshGroup을 검색한다.
		/// </summary>
		/// <param name="meshTransform"></param>
		/// <returns></returns>
		private apMeshGroup GetParentMeshGroupOfMeshTransform(apTransform_Mesh meshTransform)
		{
			for (int i = 0; i < _portrait._meshGroups.Count; i++)
			{
				if (_portrait._meshGroups[i]._childMeshTransforms.Contains(meshTransform))
				{
					//찾았다!
					return _portrait._meshGroups[i];
				}
			}
			return null;
		}


		/// <summary>
		/// 처리를 도와주는 함수. MeshTransform의 Parent MeshGroup을 검색한다.
		/// </summary>
		/// <param name="meshTransform"></param>
		/// <returns></returns>
		private apMeshGroup GetParentMeshGroupOfMeshGroupTransform(apTransform_MeshGroup meshGroupTransform)
		{
			for (int i = 0; i < _portrait._meshGroups.Count; i++)
			{
				if (_portrait._meshGroups[i]._childMeshGroupTransforms.Contains(meshGroupTransform))
				{
					//찾았다!
					return _portrait._meshGroups[i];
				}
			}
			return null;
		}

		private apMeshGroup GetParentMeshGroupOfBone(apBone bone)
		{
			for (int i = 0; i < _portrait._meshGroups.Count; i++)
			{
				if (_portrait._meshGroups[i]._boneList_All.Contains(bone))
				{
					//찾았다!
					return _portrait._meshGroups[i];
				}
			}
			return null;
		}

		public bool IsSubMeshInGroup(apTransform_Mesh subMeshTransform)
		{
			if (subMeshTransform == null)
			{ return false; }
			return _syncTransform_Mesh.Contains(subMeshTransform);
		}

		public bool IsSubMeshGroupInGroup(apTransform_MeshGroup subMeshGroupTransform)
		{
			if (subMeshGroupTransform == null)
			{ return false; }
			return _syncTransform_MeshGroup.Contains(subMeshGroupTransform);
		}


		// 추가
		//--------------------------------------------------------------------------------
		/// <summary>
		/// ParamSetGroup 내의 모든 ParamSet에서 MeshTransform을 포함한 ModMesh를 모두 삭제한다.
		/// 주의 메세지를 꼭 출력할 것
		/// </summary>
		/// <param name="meshTransform"></param>
		public void RemoveModifierMeshes(apTransform_Mesh meshTransform)
		{
			apModifierParamSet paramSet = null;
			for (int i = 0; i < _paramSetList.Count; i++)
			{
				paramSet = _paramSetList[i];
				int nRemoved = paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
				{
					return a.IsContains_MeshTransform(_parentModifier._meshGroup, meshTransform, meshTransform._mesh);
				});

				if (nRemoved > 0)
				{	
					//Debug.LogError("Remove ModMesh [RemoveModifierMeshes / MeshTransform] : " + meshTransform._nickName + " / " + nRemoved);
				}
			}


		}

		/// <summary>
		/// ParamSetGroup 내의 모든 ParamSet에서 MeshGroupTransform을 포함한 ModMesh를 모두 삭제한다.
		/// 주의 메세지를 꼭 출력할 것
		/// </summary>
		/// <param name="meshGroupTransform"></param>
		public void RemoveModifierMeshes(apTransform_MeshGroup meshGroupTransform)
		{
			apModifierParamSet paramSet = null;
			for (int i = 0; i < _paramSetList.Count; i++)
			{
				paramSet = _paramSetList[i];
				int nRemoved = paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
				{
					return a.IsContains_MeshGroupTransform(_parentModifier._meshGroup, meshGroupTransform);
				});

				if (nRemoved > 0)
				{
					//Debug.LogError("Remove ModMesh [RemoveModifierMeshes / MeshGroupTransform] : " + meshGroupTransform._nickName + " / " + nRemoved);
				}
			}
		}

		public void RemoveModifierBones(apBone bone)
		{
			apModifierParamSet paramSet = null;
			for (int i = 0; i < _paramSetList.Count; i++)
			{
				paramSet = _paramSetList[i];
				int nRemoved = paramSet._boneData.RemoveAll(delegate (apModifiedBone a)
				{
					return a._bone == bone;
				});

				if (nRemoved > 0)
				{
					//Debug.LogError("Remove ModMesh [RemoveModifierBones] : " + bone._name + " / " + nRemoved);
				}
			}
		}

		// Get / Set
		//-------------------------------------------------
		public bool IsMeshTransformContain(apTransform_Mesh meshTransform)
		{
			return _syncTransform_Mesh.Contains(meshTransform);
		}

		public bool IsMeshGroupTransformContain(apTransform_MeshGroup meshGroupTransform)
		{
			return _syncTransform_MeshGroup.Contains(meshGroupTransform);
		}

		public apModifierParamSetGroupVertWeight GetWeightVertexData(apTransform_Mesh meshTransform)
		{
			return _calculatedWeightedVertexList.Find(delegate (apModifierParamSetGroupVertWeight a)
				{
					return a._meshTransform_ID == meshTransform._transformUniqueID;
				});
		}
		public bool IsBoneContain(apBone bone)
		{
			return _syncBone.Contains(bone);
		}
	}

}