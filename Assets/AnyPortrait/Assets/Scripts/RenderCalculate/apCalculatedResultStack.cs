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
	/// Render Unit에 포함되어서
	/// 1. Result Type별로 분류하고
	/// 2. Blend를 하여
	/// 3. 최종적인 Calculate 결과를 리턴해준다.
	/// </summary>
	public class apCalculatedResultStack
	{
		// Members
		//---------------------------------------------------
		private apRenderUnit _parentRenderUnit = null;

		//계산된 결과 값들
		//(계산은 Modifier에서 직접 하기 때문에 여기엔 이미 계산된 값만 들어오게 된다)
		private List<apCalculatedResultParam> _resultParams_Rigging = new List<apCalculatedResultParam>();
		private List<apCalculatedResultParam> _resultParams_VertLocal = new List<apCalculatedResultParam>();
		private List<apCalculatedResultParam> _resultParams_Transform = new List<apCalculatedResultParam>();
		private List<apCalculatedResultParam> _resultParams_MeshColor = new List<apCalculatedResultParam>();
		private List<apCalculatedResultParam> _resultParams_VertWorld = new List<apCalculatedResultParam>();

		//BoneTransform은 바로 apCalculatedResultParam 리스트를 만드는게 아니라 2중으로 묶어야 한다.
		//키값은 Bone
		private List<BoneAndModParamPair> _resultParams_BoneTransform = new List<BoneAndModParamPair>();

		/// <summary>
		/// Bone 처리에 대한 Pair
		/// Bone을 키값으로 하여 Modifier -> CalculateResultParam List를 저장한다.
		/// </summary>
		public class BoneAndModParamPair
		{
			public apBone _keyBone = null;
			public Dictionary<apModifierBase, ModifierAndResultParamListPair> _modParamPairs_ModKey = new Dictionary<apModifierBase, ModifierAndResultParamListPair>();
			public List<ModifierAndResultParamListPair> _modParamPairs = new List<ModifierAndResultParamListPair>();

			public BoneAndModParamPair(apBone bone)
			{
				_keyBone = bone;
			}

			public void AddCalculatedResultParam(apCalculatedResultParam calculatedResultParam)
			{
				apModifierBase modifier = calculatedResultParam._linkedModifier;
				if (modifier == null)
				{ return; }

				ModifierAndResultParamListPair modParamPair = null;
				if (!_modParamPairs_ModKey.ContainsKey(modifier))
				{
					modParamPair = new ModifierAndResultParamListPair(modifier);
					_modParamPairs_ModKey.Add(modifier, modParamPair);
					_modParamPairs.Add(modParamPair);
				}
				else
				{
					modParamPair = _modParamPairs_ModKey[modifier];
				}
				modParamPair.AddCalculatedResultParam(calculatedResultParam);
			}

			public bool Remove(apCalculatedResultParam calculatedResultParam)
			{
				bool isAnyClearedParam = false;
				for (int i = 0; i < _modParamPairs.Count; i++)
				{
					_modParamPairs[i].Remove(calculatedResultParam);
					if (_modParamPairs[i]._resultParams.Count == 0)
					{
						isAnyClearedParam = true;
					}
				}
				if (isAnyClearedParam)
				{
					//Param이 없는 Pair는 삭제하고, Dictionary를 다시 만들어주자
					_modParamPairs_ModKey.Clear();
					_modParamPairs.RemoveAll(delegate (ModifierAndResultParamListPair a)
					{
						return a._resultParams.Count == 0;
					});

					for (int i = 0; i < _modParamPairs.Count; i++)
					{
						ModifierAndResultParamListPair modPair = _modParamPairs[i];

						//빠른 참조를 위해 Dictionary도 세팅해주자
						if (!_modParamPairs_ModKey.ContainsKey(modPair._keyModifier))
						{
							_modParamPairs_ModKey.Add(modPair._keyModifier, modPair);
						}
					}
				}

				return isAnyClearedParam;
			}

			public void Sort()
			{
				_modParamPairs.Sort(delegate (ModifierAndResultParamListPair a, ModifierAndResultParamListPair b)
				{
					return a._keyModifier._layer - b._keyModifier._layer;
				});
			}

		}
		/// <summary>
		/// Bone 처리에 대한 Result Param은 같은 RenderUnit에 대해서
		/// Bone에 따라 리스트가 계속 추가되는 문제가 있다. (레이어를 구분할 수 없다)
		/// 따라서 Modifier를 키값으로 하여 연산 레벨을 구분해야한다.
		/// </summary>
		public class ModifierAndResultParamListPair
		{
			public apModifierBase _keyModifier;
			public List<apCalculatedResultParam> _resultParams = new List<apCalculatedResultParam>();

			public ModifierAndResultParamListPair(apModifierBase modifier)
			{
				_keyModifier = modifier;
			}

			public void AddCalculatedResultParam(apCalculatedResultParam calculatedResultParam)
			{
				if (!_resultParams.Contains(calculatedResultParam))
				{
					_resultParams.Add(calculatedResultParam);
				}
			}

			public void Remove(apCalculatedResultParam calculatedResultParam)
			{
				_resultParams.Remove(calculatedResultParam);
			}
		}

		//public List<Vector2> _result_VertLocal = null;
		public Vector2[] _result_VertLocal = null;//<<최적화를 위해 변경
		public apMatrix _result_MeshTransform = new apMatrix();
		public Color _result_Color = new Color(0.5f, 0.5f, 0.5f, 1f);
		public bool _result_IsVisible = true;

		//public List<Vector2> _result_VertWorld = null;
		public Vector2[] _result_VertWorld = null;

		//추가
		//Rigging Result
		public Vector2[] _result_Rigging = null;
		public float _result_RiggingWeight = 0.0f;
		public apMatrix3x3[] _result_RiggingMatrices = null;

		//Bone Transform
		//값을 계속 초기화해서 사용하는 지역변수의 역할
		private apMatrix _result_BoneTransform = new apMatrix();

		private bool _result_CalculatedColor = false;

		private bool _isAnyRigging = false;
		private bool _isAnyVertLocal = false;
		private bool _isAnyTransformation = false;
		private bool _isAnyMeshColor = false;

		private bool _isAnyVertWorld = false;
		private bool _isAnyBoneTransform = false;

		private Color _color_Default = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		//private Vector3 _color_2XTmp_Prev = Vector3.zero;
		//private Vector3 _color_2XTmp_Next = Vector3.zero;

		//private int _tmpID = -1;
		//private int _hashCode_Rigging = 0;
		//private int _hashCode_VertLocal = 0;
		//private int _hashCode_MeshTransform = 0;
		//private int _hashCode_VertWorld = 0;
		//public int CalculateHashCode {  get { return _hashCode_Rigging + _hashCode_VertLocal + _hashCode_MeshTransform + _hashCode_VertWorld; } }

		//>>> CalculatedStackLayer를 넣어서 Edit를 할 수 있게 만들자
		//[NonSerialized]
		//private apCalculatedLog _calLog_0_Rigging = null;

		//[NonSerialized]
		//private apCalculatedLog _calLog_1_StaticMesh = null;

		//[NonSerialized]
		//private apCalculatedLog _calLog_2_VertLocal = null;

		//[NonSerialized]
		//private apCalculatedLog _calLog_3_MeshTransform = null;

		//[NonSerialized]
		//private apCalculatedLog _calLog_4_VertWorld = null;

		//public apCalculatedLog CalculateLog_0_Rigging { get { if (_calLog_0_Rigging == null) { _calLog_0_Rigging = new apCalculatedLog(apCalculatedLog.LOG_TYPE.CalResultStackLayer_0_Rigging, this); } return _calLog_0_Rigging; } }
		//public apCalculatedLog CalculateLog_1_StaticMesh { get { if (_calLog_1_StaticMesh == null) { _calLog_1_StaticMesh = new apCalculatedLog(apCalculatedLog.LOG_TYPE.CalResultStackLayer_1_StaticMesh, this); } return _calLog_1_StaticMesh; } }
		//public apCalculatedLog CalculateLog_2_VertLocal { get { if (_calLog_2_VertLocal == null) { _calLog_2_VertLocal = new apCalculatedLog(apCalculatedLog.LOG_TYPE.CalResultStackLayer_2_VertLocal, this); } return _calLog_2_VertLocal; } }
		//public apCalculatedLog CalculateLog_3_MeshTransform { get { if (_calLog_3_MeshTransform == null) { _calLog_3_MeshTransform = new apCalculatedLog(apCalculatedLog.LOG_TYPE.CalResultStackLayer_3_MeshTransform, this); } return _calLog_3_MeshTransform; } }
		//public apCalculatedLog CalculateLog_4_VertWorld { get { if (_calLog_4_VertWorld == null) { _calLog_4_VertWorld = new apCalculatedLog(apCalculatedLog.LOG_TYPE.CalResultStackLayer_4_VertWorld, this); } return _calLog_4_VertWorld; } }


		// Init
		//---------------------------------------------------
		public apCalculatedResultStack(apRenderUnit parentRenderUnit)
		{
			//_tmpID = UnityEngine.Random.Range(0, 1000);

			_parentRenderUnit = parentRenderUnit;
			//Debug.Log("[" + _tmpID + "] Init [R " + _parentRenderUnit._tmpName + "]");
			ClearResultParams();
		}



		// Add / Remove / Sort
		//---------------------------------------------------
		public void AddCalculatedResultParam(apCalculatedResultParam resultParam)
		{

			//Debug.Log("[" + _tmpID + "] AddCalculatedResultParam >> " + resultParam._resultType + "[R " + _parentRenderUnit._tmpName + "]");
			if ((int)(resultParam._calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
			{
				if (resultParam._targetBone == null)
				{
					if (resultParam._calculatedSpace == apCalculatedResultParam.CALCULATED_SPACE.Object)
					{
						if (!_resultParams_VertLocal.Contains(resultParam))
						{
							_resultParams_VertLocal.Add(resultParam);
						}
						_isAnyVertLocal = true;
					}
					else if (resultParam._calculatedSpace == apCalculatedResultParam.CALCULATED_SPACE.World)
					{
						if (!_resultParams_VertWorld.Contains(resultParam))
						{
							_resultParams_VertWorld.Add(resultParam);
						}
						_isAnyVertWorld = true;
					}
					else if (resultParam._calculatedSpace == apCalculatedResultParam.CALCULATED_SPACE.Rigging)//<<추가되었다.
					{
						if (!_resultParams_Rigging.Contains(resultParam))
						{
							_resultParams_Rigging.Add(resultParam);
						}
						_isAnyRigging = true;
					}
					else
					{
						Debug.LogError("허용되지 않은 데이터 타입 [Calculate : Vertex + Loca]");
					}
				}
			}
			if ((int)(resultParam._calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.TransformMatrix) != 0)
			{
				//변경 : Bone타입과 일반 Transform타입으로 나뉜다.
				if (resultParam._targetBone != null)
				{

					//Bone 타입이다.
					//Modifier + ResultParam Pair로 저장해야한다.
					BoneAndModParamPair modParamPair = _resultParams_BoneTransform.Find(delegate (BoneAndModParamPair a)
					{
						return a._keyBone == resultParam._targetBone;
					});
					if (modParamPair == null)
					{
						modParamPair = new BoneAndModParamPair(resultParam._targetBone);
						_resultParams_BoneTransform.Add(modParamPair);
					}

					modParamPair.AddCalculatedResultParam(resultParam);
					_isAnyBoneTransform = true;

					//이전 코드
					//if(!_resultParams_BoneTransform.Contains(resultParam))
					//{
					//	_resultParams_BoneTransform.Add(resultParam);
					//	_isAnyBoneTransform = true;
					//}
				}
				else
				{
					//Mesh/MeshGroup Transform 타입이다.
					if (!_resultParams_Transform.Contains(resultParam))
					{
						_resultParams_Transform.Add(resultParam);
						_isAnyTransformation = true;
					}
				}

			}
			if ((int)(resultParam._calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
			{
				//Bone 타입은 제외한다.
				if (resultParam._targetBone == null)
				{
					if (!_resultParams_MeshColor.Contains(resultParam))
					{
						_resultParams_MeshColor.Add(resultParam);
						_isAnyMeshColor = true;

					}
				}
			}

			//else
			//{
			//	Debug.LogError("apCalculatedResultStack / AddCalculatedResultParam : 알수없는 Result Type : " + resultParam._calculatedValueType);
			//}
			#region [미사용 코드] 변경되기 전의 Caculated Value
			//switch (resultParam._calculatedValueType)
			//{
			//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos:
			//		{

			//		}
			//		break;

			//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.TransformMatrix:
			//		if(!_resultParams_Transform.Contains(resultParam))
			//		{
			//			_resultParams_Transform.Add(resultParam);
			//			_isAnyTransformation = true;
			//		}
			//		break;

			//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.MeshGroup_Color:
			//		if(!_resultParams_MeshColor.Contains(resultParam))
			//		{
			//			_resultParams_MeshColor.Add(resultParam);
			//			_isAnyMeshColor = true;
			//		}
			//		break;

			//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.Vertex_World:
			//		if(!_resultParams_VertWorld.Contains(resultParam))
			//		{
			//			_resultParams_VertWorld.Add(resultParam);
			//			_isAnyVertWorld = true;
			//		}
			//		break;

			//	default:
			//		Debug.LogError("apCalculatedResultStack / AddCalculatedResultParam : 알수없는 Result Type : " + resultParam._calculatedValueType);
			//		break;
			//} 
			#endregion
		}

		public void RemoveCalculatedResultParam(apCalculatedResultParam resultParam)
		{
			_resultParams_Rigging.Remove(resultParam);
			_resultParams_VertLocal.Remove(resultParam);
			_resultParams_Transform.Remove(resultParam);
			_resultParams_MeshColor.Remove(resultParam);
			_resultParams_VertWorld.Remove(resultParam);

			//Bone Transform은 Pair된 SubList로 관리되므로, 해당 Pair를 먼저 찾고 거기서 체크를 해야한다.
			bool isAnyClearedBoneParam = false;
			for (int i = 0; i < _resultParams_BoneTransform.Count; i++)
			{
				if (_resultParams_BoneTransform[i].Remove(resultParam))
				{
					isAnyClearedBoneParam = true;
				}
			}

			if (isAnyClearedBoneParam)
			{
				//전체에서 Param 개수가 0인 것들을 빼자
				_resultParams_BoneTransform.RemoveAll(delegate (BoneAndModParamPair a)
				{
					return a._modParamPairs.Count == 0;
				});
			}

			_isAnyRigging = (_resultParams_Rigging.Count != 0);
			_isAnyVertLocal = (_resultParams_VertLocal.Count != 0);
			_isAnyTransformation = (_resultParams_Transform.Count != 0);
			_isAnyMeshColor = (_resultParams_MeshColor.Count != 0);
			_isAnyVertWorld = (_resultParams_VertWorld.Count != 0);
			_isAnyBoneTransform = (_resultParams_BoneTransform.Count != 0);
			//Debug.LogError("[" + _tmpID + "] <<Remove Result Params>>");
		}

		public void ClearResultParams()
		{
			//if(_resultParams_VertLocal.Count > 0 || _isAnyVertLocal)
			//{
			//	Debug.LogError("[" + _tmpID + "] <<Clear Result Params>> < Vert Local Count : " + _resultParams_VertLocal.Count);
			//}
			//Debug.LogError("[" + _tmpID + "] <<Clear Result Params>>");

			_resultParams_Rigging.Clear();
			_resultParams_VertLocal.Clear();
			_resultParams_Transform.Clear();
			_resultParams_MeshColor.Clear();
			_resultParams_VertWorld.Clear();
			_resultParams_BoneTransform.Clear();

			_isAnyRigging = false;
			_isAnyVertLocal = false;
			_isAnyTransformation = false;
			_isAnyMeshColor = false;
			_isAnyVertWorld = false;
			_isAnyBoneTransform = false;
		}

		public void Sort()
		{
			//>Opt 연동할 것
			//다른 RenderUnit에 대해서는
			//Level이 큰게(하위) 먼저 계산되도록 내림차순 정렬 > 변경 ) Level 낮은 상위가 먼저 계산되도록 (오름차순)

			//같은 RenderUnit에 대해서는
			//오름차순 정렬 (레이어 값이 낮은 것 부터 처리할 수 있도록)

			_resultParams_Rigging.Sort(delegate (apCalculatedResultParam a, apCalculatedResultParam b)
			{
				if (a._targetRenderUnit == b._targetRenderUnit)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetRenderUnit._level - b._targetRenderUnit._level; }
			});

			_resultParams_VertLocal.Sort(delegate (apCalculatedResultParam a, apCalculatedResultParam b)
			{
				if (a._targetRenderUnit == b._targetRenderUnit)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetRenderUnit._level - b._targetRenderUnit._level; }
			});

			_resultParams_Transform.Sort(delegate (apCalculatedResultParam a, apCalculatedResultParam b)
			{
				if (a._targetRenderUnit == b._targetRenderUnit)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetRenderUnit._level - b._targetRenderUnit._level; }
			});

			_resultParams_MeshColor.Sort(delegate (apCalculatedResultParam a, apCalculatedResultParam b)
			{
				if (a._targetRenderUnit == b._targetRenderUnit)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetRenderUnit._level - b._targetRenderUnit._level; }
			});

			_resultParams_VertWorld.Sort(delegate (apCalculatedResultParam a, apCalculatedResultParam b)
			{
				if (a._targetRenderUnit == b._targetRenderUnit)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetRenderUnit._level - b._targetRenderUnit._level; }
			});

			//_resultParams_BoneTransform.Sort(delegate (apCalculatedResultParam a, apCalculatedResultParam b)
			//{
			//	if(a._targetRenderUnit == b._targetRenderUnit)	{ return a.ModifierLayer - b.ModifierLayer; }
			//	return 0;//<이건 Sort가 그닥 필요하지 않다. Bone이니까..
			//});

			for (int i = 0; i < _resultParams_BoneTransform.Count; i++)
			{
				_resultParams_BoneTransform[i].Sort();
			}


			//어떤 Modifier들이 현재 Renderu Unit의 Calculate Stack에 적용되었는지 확인하는 디버그 출력 코드
			//Debug.LogError("Calculate Stack Sort - " + _parentRenderUnit.Name);
			//Debug.Log("1) Vert Local : " + _resultParams_VertLocal.Count);
			//for (int i = 0; i < _resultParams_VertLocal.Count; i++)
			//{
			//	Debug.Log("[" + _resultParams_VertLocal[i]._linkedModifier.DisplayName + " / " + _resultParams_VertLocal[i]._targetRenderUnit.Name + "]");
			//}

			//Debug.Log("2) Transform : " + _resultParams_Transform.Count);
			//for (int i = 0; i < _resultParams_Transform.Count; i++)
			//{
			//	Debug.Log("[" + _resultParams_Transform[i]._linkedModifier.DisplayName + " / " + _resultParams_Transform[i]._targetRenderUnit.Name + "]");
			//}

			//Debug.Log("3) Mesh Color : " + _resultParams_MeshColor.Count);
			//for (int i = 0; i < _resultParams_MeshColor.Count; i++)
			//{
			//	Debug.Log("[" + _resultParams_MeshColor[i]._linkedModifier.DisplayName + " / " + _resultParams_MeshColor[i]._targetRenderUnit.Name + "]");
			//}

			//Debug.Log("4) Vert World : " + _resultParams_VertWorld.Count);
			//for (int i = 0; i < _resultParams_VertWorld.Count; i++)
			//{
			//	Debug.Log("[" + _resultParams_VertWorld[i]._linkedModifier.DisplayName + " / " + _resultParams_VertWorld[i]._targetRenderUnit.Name + "]");
			//}
		}


		// Functions
		//---------------------------------------------------
		public void ReadyToCalculate()
		{
			int nVerts = _parentRenderUnit._renderVerts.Count;

			if(_isAnyVertLocal)
			{
				if(_result_VertLocal == null || _result_VertLocal.Length != nVerts)
				{
					_result_VertLocal = new Vector2[nVerts];
				}
				for (int i = 0; i < nVerts; i++)
				{
					_result_VertLocal[i] = Vector2.zero;
				}
			}

			if(_isAnyVertWorld)
			{
				if(_result_VertWorld == null || _result_VertWorld.Length != nVerts)
				{
					_result_VertWorld = new Vector2[nVerts];
				}
				for (int i = 0; i < nVerts; i++)
				{
					_result_VertWorld[i] = Vector2.zero;
				}
			}

			if(_isAnyRigging)
			{
				if(_result_RiggingMatrices == null || _result_RiggingMatrices.Length != nVerts)
				{
					_result_RiggingMatrices = new apMatrix3x3[nVerts];
					_result_Rigging = new Vector2[nVerts];
				}

				for (int i = 0; i < nVerts; i++)
				{
					_result_RiggingMatrices[i].SetIdentity();
					_result_Rigging[i] = Vector2.zero;
				}
				_result_RiggingWeight = 0.0f;
			}

			

			#region [미사용 코드]
			//if (_isAnyVertLocal || _isAnyVertWorld || _isAnyRigging)
			//{
			//	if (_result_VertLocal == null || _result_VertLocal.Length != _parentRenderUnit._renderVerts.Count)
			//	{
			//		//RenderUnit의 RenderVertex 개수 만큼 결과를 만들자
			//		//_result_VertLocal = new List<Vector2>();
			//		//_result_VertWorld = new List<Vector2>();

			//		_result_VertLocal = new Vector2[_parentRenderUnit._renderVerts.Count];
			//		_result_VertWorld = new Vector2[_parentRenderUnit._renderVerts.Count];
			//		_result_Rigging = new Vector2[_parentRenderUnit._renderVerts.Count];
			//		_result_RiggingMatrices = new apMatrix3x3[_parentRenderUnit._renderVerts.Count];
			//		_result_RiggingWeight = 0.0f;


			//		//_result_VertLocal.Initialize();
			//		//_result_VertWorld.Initialize();
			//		for (int i = 0; i < _parentRenderUnit._renderVerts.Count; i++)
			//		{
			//			_result_VertLocal[i] = Vector2.zero;
			//			_result_VertWorld[i] = Vector2.zero;
			//			_result_Rigging[i] = Vector2.zero;
			//			_result_RiggingMatrices[i].SetIdentity();
			//		}


			//	}
			//	else
			//	{
			//		//_result_VertLocal.Initialize();
			//		//_result_VertWorld.Initialize();
			//		//for (int i = 0; i < _result_VertLocal.Count; i++)
			//		//{
			//		//	_result_VertLocal[i] = Vector2.zero;
			//		//	_result_VertWorld[i] = Vector2.zero;
			//		//}



			//		if (_isAnyRigging)
			//		{
			//			for (int i = 0; i < _result_VertLocal.Length; i++)
			//			{
			//				_result_VertLocal[i] = Vector2.zero;
			//				_result_VertWorld[i] = Vector2.zero;
			//				_result_Rigging[i] = Vector2.zero;
			//				_result_RiggingMatrices[i].SetIdentity();
			//			}
			//		}
			//		else
			//		{
			//			//리깅이 없다면 불필요한 처리는 하지 말자
			//			for (int i = 0; i < _result_VertLocal.Length; i++)
			//			{
			//				_result_VertLocal[i] = Vector2.zero;
			//				_result_VertWorld[i] = Vector2.zero;
			//			}
			//		}
			//		_result_RiggingWeight = 0.0f;
			//	}
			//} 
			#endregion

			_result_BoneTransform.SetIdentity();
			_result_MeshTransform.SetIdentity();
			_result_MeshTransform.MakeMatrix();
			_result_Color = _color_Default;
			_result_IsVisible = true;
			_result_CalculatedColor = false;
		}

		/// <summary>
		/// Modifier등의 변동 사항이 있는 경우 RenderVert의 업데이트 데이터를 초기화한다.
		/// </summary>
		public void ResetRenderVerts()
		{

			if (_parentRenderUnit != null)
			{
				for (int i = 0; i < _parentRenderUnit._renderVerts.Count; i++)
				{
					_parentRenderUnit._renderVerts[i].ResetData();
				}
			}
		}

		/// <summary>
		/// RenderUnit의 CalculateStack을 업데이트한다.
		/// 기본 단계의 업데이트이며, Rigging, VertWorld는 Post Update에서 처리한다.
		/// </summary>
		/// <param name="tDelta"></param>
		/// <param name="isMakeHashCode"></param>
		//public void Calculate_Pre(float tDelta, bool isMakeHashCode)
		public void Calculate_Pre(float tDelta)
		{
			float prevWeight = 0.0f;
			float curWeight_Transform = 0.0f;
			float curWeight_Color = 0.0f;
			apCalculatedResultParam resultParam = null;

			//추가) 처음 실행되는 CalParam은 Additive로 작동하지 않도록 한다.
			int iCalculatedParam = 0;

			//추가 3.22
			//Parent RenderUnit에 대해서
			bool isExCalculated = false;
			bool isExEnabledOnly = false;
			bool isSubExEnabledOnly = false;

			if (_parentRenderUnit._exCalculateMode != apRenderUnit.EX_CALCULATE.Normal)
			{
				isExCalculated = true;
				//Normal 타입이 아닌 경우
				if (_parentRenderUnit._exCalculateMode == apRenderUnit.EX_CALCULATE.ExAdded)
				{
					//Ex Add 타입이다. : SubExEnabled는 Disabled로 간주한다.
					isExEnabledOnly = true;
				}
				else if (_parentRenderUnit._exCalculateMode == apRenderUnit.EX_CALCULATE.ExNotAdded)
				{
					//Ex Not Add 타입이다. : SubExEnabled만 Enabled로 간주한다.
					isSubExEnabledOnly = true;
				}

				//안내
				//isExCalculated = true인 경우 (Ex Edit가 실행중인 경우)
				//isExEnabledOnly => Modifier가 ExclusiveEnabled인 것만 실행
				//isSubExEnabledOnly => Modifier가 SubExEnabled인 것만 실행
				//그외에는 실행하지 않는다.
			}


			//--------------------------------------------------------------------
			// 1. Local Morph
			if (_isAnyVertLocal)
			{
				prevWeight = 0.0f;
				curWeight_Transform = 0.0f;
				resultParam = null;
				Vector2[] posVerts = null;

				iCalculatedParam = 0;

				for (int iParam = 0; iParam < _resultParams_VertLocal.Count; iParam++)
				{
					resultParam = _resultParams_VertLocal[iParam];
					
					curWeight_Transform = resultParam.ModifierWeight_Transform;

					if (!resultParam.IsModifierAvailable || curWeight_Transform <= 0.001f)
					{
						continue;
					}

					//추가 Ex Edit 3.22
					if(isExCalculated)
					{
						if((isExEnabledOnly && resultParam._linkedModifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled)
							|| (isSubExEnabledOnly && resultParam._linkedModifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.SubExEnabled))
						{
							//ExEdit 모드에 맞지 않는다.
							continue;
						}
					}

					posVerts = resultParam._result_Positions;
					if (posVerts.Length != _result_VertLocal.Length)
					{
						//결과가 잘못 들어왔다 갱신 필요
						Debug.LogError("Wrong Vert Local Result (Cal : " + posVerts.Length + " / Verts : " + _result_VertLocal.Length + ")");
						continue;
					}

					// Blend 방식에 맞게 Pos를 만들자
					if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
					{
						for (int i = 0; i < posVerts.Length; i++)
						{
							_result_VertLocal[i] = BlendPosition_ITP(_result_VertLocal[i], posVerts[i], curWeight_Transform);
						}

						prevWeight += curWeight_Transform;

						//>>>> Calculated Log - VertLog
						//resultParam.CalculatedLog.Calculate_CalParamResult(curWeight,
						//													iCalculatedParam,
						//													apModifierBase.BLEND_METHOD.Interpolation,
						//													CalculateLog_2_VertLocal);
					}
					else
					{
						for (int i = 0; i < posVerts.Length; i++)
						{
							_result_VertLocal[i] = BlendPosition_Add(_result_VertLocal[i], posVerts[i], curWeight_Transform);
						}

						//>>>> Calculated Log - VertLog
						//resultParam.CalculatedLog.Calculate_CalParamResult(curWeight,
						//													iCalculatedParam,
						//													apModifierBase.BLEND_METHOD.Additive,
						//													CalculateLog_2_VertLocal);
					}

					//Debug.Log("[" + resultParam._targetRenderUnit.Name + "] : " + resultParam._linkedModifier.DisplayName + " / " + resultParam._paramKeyValues.Count);
					iCalculatedParam++;

				}

				////HashCode를 만들자.
				//if (isMakeHashCode)
				//{
				//	if (_result_VertLocal.Length > 0)
				//	{
				//		_hashCode_VertLocal = _result_VertLocal[(_result_VertLocal.Length - 1) / 2].GetHashCode();
				//	}
				//}
			}

			//--------------------------------------------------------------------

			// 2. Mesh / MeshGroup Transformation
			if (_isAnyTransformation)
			{
				prevWeight = 0.0f;
				curWeight_Transform = 0.0f;
				resultParam = null;

				iCalculatedParam = 0;

				for (int iParam = 0; iParam < _resultParams_Transform.Count; iParam++)
				{
					resultParam = _resultParams_Transform[iParam];
					curWeight_Transform = resultParam.ModifierWeight_Transform;

					if (!resultParam.IsModifierAvailable || curWeight_Transform <= 0.001f)
					{ continue; }

					//추가 Ex Edit 3.22
					if(isExCalculated)
					{
						if((isExEnabledOnly && resultParam._linkedModifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled)
							|| (isSubExEnabledOnly && resultParam._linkedModifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.SubExEnabled))
						{
							//ExEdit 모드에 맞지 않는다.
							continue;
						}
					}


					// Blend 방식에 맞게 Matrix를 만들자 하자
					if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
					{
						BlendMatrix_ITP(_result_MeshTransform, resultParam._result_Matrix, curWeight_Transform);
						prevWeight += curWeight_Transform;

						//>>>> Calculated Log - VertLog
						//resultParam.CalculatedLog.Calculate_CalParamResult(curWeight,
						//													iCalculatedParam,
						//													apModifierBase.BLEND_METHOD.Interpolation,
						//													CalculateLog_3_MeshTransform);
					}
					else
					{
						BlendMatrix_Add(_result_MeshTransform, resultParam._result_Matrix, curWeight_Transform);

						//>>>> Calculated Log - VertLog
						//resultParam.CalculatedLog.Calculate_CalParamResult(curWeight,
						//													iCalculatedParam,
						//													apModifierBase.BLEND_METHOD.Additive,
						//													CalculateLog_3_MeshTransform);
					}

					iCalculatedParam++;
				}

				_result_MeshTransform.MakeMatrix();

				////HashCode를 만들자.
				//if (isMakeHashCode)
				//{
				//	_hashCode_MeshTransform = _result_MeshTransform.GetHashCode();
				//}
			}

			//--------------------------------------------------------------------

			// 3. Mesh Color
			if (_isAnyMeshColor)
			{
				prevWeight = 0.0f;
				curWeight_Color = 0.0f;
				resultParam = null;

				iCalculatedParam = 0;

				_result_IsVisible = false;
				_result_CalculatedColor = false;

				int nMeshColorCalculated = 0;

				for (int iParam = 0; iParam < _resultParams_MeshColor.Count; iParam++)
				{
					resultParam = _resultParams_MeshColor[iParam];
					curWeight_Color = resultParam.ModifierWeight_Color;

					if (!resultParam.IsModifierAvailable
						|| curWeight_Color <= 0.001f
						|| !resultParam.IsColorValueEnabled
						|| !resultParam._isColorCalculated//<<추가 : Color로 등록했지만 아예 계산이 안되었을 수도 있다.
						)
					{
						continue;
					}

					//추가: 색상은 ExMode에서 별도로 취급

					// Blend 방식에 맞게 Matrix를 만들자 하자
					if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
					{
						//_result_Color = BlendColor_ITP(_result_Color, resultParam._result_Color, prevWeight, curWeight);
						_result_Color = apUtil.BlendColor_ITP(_result_Color, resultParam._result_Color, Mathf.Clamp01(curWeight_Color));
						prevWeight += curWeight_Color;
					}
					else
					{
						_result_Color = apUtil.BlendColor_Add(_result_Color, resultParam._result_Color, curWeight_Color);
					}



					//Visible 여부도 결정
					_result_IsVisible |= resultParam._result_IsVisible;
					nMeshColorCalculated++;
					_result_CalculatedColor = true;//<<"계산된 MeshColor" Result가 있음을 알린다.

					//Debug.Log("MeshColor Update < " + resultParam._result_Color + " [" + resultParam._linkedModifier.DisplayName + "] (" + curWeight + ") >> " + _result_Color);

					iCalculatedParam++;
				}

				if (nMeshColorCalculated == 0)
				{
					//색상 처리값이 없다면 자동으로 True
					_result_IsVisible = true;
				}
			}
			else
			{
				//색상 처리값이 없다면 자동으로 True
				_result_IsVisible = true;
			}

			//--------------------------------------------------------------------

			//5. Bone을 업데이트 하자
			//Bone은 값 저장만 할게 아니라 직접 업데이트를 해야한다.
			if (_isAnyBoneTransform)
			//if(false)
			{
				prevWeight = 0.0f;
				curWeight_Transform = 0.0f;
				resultParam = null;


				//추가 3.22 ExEdit
				//본의 경우는 별도로 ExMode를 가진다.
				bool isExCalculated_Bone = false;
				bool isExEnabledOnly_Bone = false;
				bool isExSubEnabledOnly_Bone = false;


				for (int iBonePair = 0; iBonePair < _resultParams_BoneTransform.Count; iBonePair++)
				{
					BoneAndModParamPair boneModPair = _resultParams_BoneTransform[iBonePair];
					apBone targetBone = boneModPair._keyBone;
					List<ModifierAndResultParamListPair> modParamPairs = boneModPair._modParamPairs;
					if (targetBone == null || modParamPairs.Count == 0)
					{
						continue;
					}

					iCalculatedParam = 0;
					_result_BoneTransform.SetIdentity();

					isExCalculated_Bone = false;
					isExEnabledOnly_Bone = false;
					isExSubEnabledOnly_Bone = false;

					if (targetBone._exCalculateMode != apBone.EX_CALCULATE.Normal)
					{
						isExCalculated_Bone = true;
						if (targetBone._exCalculateMode == apBone.EX_CALCULATE.ExAdded)
						{
							isExEnabledOnly_Bone = true;
						}
						else if (targetBone._exCalculateMode == apBone.EX_CALCULATE.ExNotAdded)
						{
							isExSubEnabledOnly_Bone = true;
						}
					}

					for (int iModParamPair = 0; iModParamPair < modParamPairs.Count; iModParamPair++)
					{
						ModifierAndResultParamListPair modParamPair = modParamPairs[iModParamPair];

						for (int iParam = 0; iParam < modParamPair._resultParams.Count; iParam++)
						{
							resultParam = modParamPair._resultParams[iParam];
							curWeight_Transform = resultParam.ModifierWeight_Transform;

							if (!resultParam.IsModifierAvailable || curWeight_Transform <= 0.001f)
							{ continue; }


							//추가 Ex Edit 3.22
							if(isExCalculated_Bone)
							{
								if((isExEnabledOnly_Bone && resultParam._linkedModifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled)
									|| (isExSubEnabledOnly_Bone && resultParam._linkedModifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.SubExEnabled))
								{
									//ExEdit 모드에 맞지 않는다.
									continue;
								}
							}



							// Blend 방식에 맞게 Matrix를 만들자 하자
							if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
							{
								BlendMatrix_ITP(_result_BoneTransform, resultParam._result_Matrix, curWeight_Transform);
								prevWeight += curWeight_Transform;
							}
							else
							{
								BlendMatrix_Add(_result_BoneTransform, resultParam._result_Matrix, curWeight_Transform);
							}

							iCalculatedParam++;
						}
					}

					//참조된 본에 직접 값을 넣어주자
					targetBone.UpdateModifiedValue(_result_BoneTransform._pos, _result_BoneTransform._angleDeg, _result_BoneTransform._scale);
				}



			}
		}



		/// <summary>
		/// RenderUnit의 CalculateStack을 업데이트한다.
		/// 1차 업데이트 이후에 실행되며, Rigging, VertWorld를 처리한다.
		/// </summary>
		/// <param name="tDelta"></param>
		/// <param name="isMakeHashCode"></param>
		//public void Calculate_Post(float tDelta, bool isMakeHashCode)
		public void Calculate_Post(float tDelta)
		{
			float prevWeight = 0.0f;
			float curWeight_Transform = 0.0f;
			//float curWeight_Color = 0.0f;
			apCalculatedResultParam resultParam = null;

			//추가) 처음 실행되는 CalParam은 Additive로 작동하지 않도록 한다.
			int iCalculatedParam = 0;


			//--------------------------------------------------------------------
			// 0. Rigging
			if (_isAnyRigging)
			{
				prevWeight = 0.0f;
				curWeight_Transform = 0.0f;
				resultParam = null;
				Vector2[] posVerts = null;
				apMatrix3x3[] vertMatrice = null;

				iCalculatedParam = 0;

				_result_RiggingWeight = 0.0f;

				for (int iParam = 0; iParam < _resultParams_Rigging.Count; iParam++)
				{
					resultParam = _resultParams_Rigging[iParam];
					curWeight_Transform = resultParam.ModifierWeight_Transform;

					if (!resultParam.IsModifierAvailable || curWeight_Transform <= 0.001f)
					{
						continue;
					}


					posVerts = resultParam._result_Positions;
					vertMatrice = resultParam._result_VertMatrices;

					if(posVerts == null)
					{
						Debug.LogError("Pos Vert is NULL");
					}
					if (posVerts.Length != _result_Rigging.Length)
					{
						//결과가 잘못 들어왔다 갱신 필요
						Debug.LogError("Wrong Vert Local Result (Cal : " + posVerts.Length + " / Verts : " + _result_Rigging.Length + ")");
						continue;
					}

					_result_RiggingWeight += curWeight_Transform;

					// Blend 방식에 맞게 Pos를 만들자
					if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
					{
						for (int i = 0; i < posVerts.Length; i++)
						{
							_result_Rigging[i] = BlendPosition_ITP(_result_Rigging[i], posVerts[i], curWeight_Transform);
							_result_RiggingMatrices[i].SetMatrixWithWeight(vertMatrice[i], curWeight_Transform);//<<추가

							//if(i == 0)
							//{
							//	Debug.Log("Result RigMatrix\n" + _result_RiggingMatrices[i]);
							//}

						}

						prevWeight += curWeight_Transform;

						//>>>> Calculated Log - Rigging
						//resultParam.CalculatedLog.Calculate_CalParamResult(curWeight,
						//													iCalculatedParam,
						//													apModifierBase.BLEND_METHOD.Interpolation,
						//													CalculateLog_0_Rigging);
					}
					else
					{
						for (int i = 0; i < posVerts.Length; i++)
						{
							_result_Rigging[i] = BlendPosition_Add(_result_Rigging[i], posVerts[i], curWeight_Transform);
							_result_RiggingMatrices[i].AddMatrixWithWeight(vertMatrice[i], curWeight_Transform);//<<추가
						}

						//>>>> Calculated Log - Rigging
						//resultParam.CalculatedLog.Calculate_CalParamResult(curWeight,
						//													iCalculatedParam,
						//													apModifierBase.BLEND_METHOD.Additive,
						//													CalculateLog_0_Rigging);
					}

					//Debug.Log("[" + resultParam._targetRenderUnit.Name + "] : " + resultParam._linkedModifier.DisplayName + " / " + resultParam._paramKeyValues.Count);
					iCalculatedParam++;

				}

				//HashCode를 만들자.
				//if (isMakeHashCode)
				//{
				//	if (_result_Rigging.Length > 0)
				//	{
				//		_hashCode_Rigging = _result_Rigging[(_result_Rigging.Length - 1) / 2].GetHashCode();
				//	}
				//}

				if (_result_RiggingWeight > 1.0f)
				{
					_result_RiggingWeight = 1.0f;
				}
			}

			//--------------------------------------------------------------------
			// 4. World Morph
			if (_isAnyVertWorld)
			{

				//추가 3.22
				//Parent RenderUnit에 대해서
				bool isExCalculated = false;
				bool isExEnabledOnly = false;
				bool isSubExEnabledOnly = false;

				if (_parentRenderUnit._exCalculateMode != apRenderUnit.EX_CALCULATE.Normal)
				{
					isExCalculated = true;
					//Normal 타입이 아닌 경우
					if (_parentRenderUnit._exCalculateMode == apRenderUnit.EX_CALCULATE.ExAdded)
					{
						//Ex Add 타입이다. : SubExEnabled는 Disabled로 간주한다.
						isExEnabledOnly = true;
					}
					else if (_parentRenderUnit._exCalculateMode == apRenderUnit.EX_CALCULATE.ExNotAdded)
					{
						//Ex Not Add 타입이다. : SubExEnabled만 Enabled로 간주한다.
						isSubExEnabledOnly = true;
					}

					//안내
					//isExCalculated = true인 경우 (Ex Edit가 실행중인 경우)
					//isExEnabledOnly => Modifier가 ExclusiveEnabled인 것만 실행
					//isSubExEnabledOnly => Modifier가 SubExEnabled인 것만 실행
					//그외에는 실행하지 않는다.
				}

				prevWeight = 0.0f;
				curWeight_Transform = 0.0f;
				resultParam = null;
				Vector2[] posVerts = null;

				iCalculatedParam = 0;

				for (int iParam = 0; iParam < _resultParams_VertWorld.Count; iParam++)
				{
					resultParam = _resultParams_VertWorld[iParam];
					curWeight_Transform = resultParam.ModifierWeight_Transform;

					if (!resultParam.IsModifierAvailable || curWeight_Transform <= 0.001f)
					{ continue; }

					posVerts = resultParam._result_Positions;
					if (posVerts.Length != _result_VertWorld.Length)
					{
						//결과가 잘못 들어왔다 갱신 필요
						Debug.LogError("Wrong Vert World Result (Cal : " + posVerts.Length + " / Verts : " + _result_VertWorld.Length + ")");
						continue;
					}

					//추가 Ex Edit 3.22
					if(isExCalculated)
					{
						if((isExEnabledOnly && resultParam._linkedModifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled)
							|| (isSubExEnabledOnly && resultParam._linkedModifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.SubExEnabled))
						{
							//ExEdit 모드에 맞지 않는다.
							continue;
						}
					}


					// Blend 방식에 맞게 Pos를 만들자
					if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
					{
						for (int i = 0; i < posVerts.Length; i++)
						{
							_result_VertWorld[i] = BlendPosition_ITP(_result_VertWorld[i], posVerts[i], curWeight_Transform);
						}

						prevWeight += curWeight_Transform;

						//>>>> Calculated Log - VertWorld
						//resultParam.CalculatedLog.Calculate_CalParamResult(curWeight,
						//													iCalculatedParam,
						//													apModifierBase.BLEND_METHOD.Interpolation,
						//													CalculateLog_4_VertWorld);
					}
					else
					{
						for (int i = 0; i < posVerts.Length; i++)
						{
							_result_VertWorld[i] = BlendPosition_Add(_result_VertWorld[i], posVerts[i], curWeight_Transform);
						}

						//>>>> Calculated Log - VertWorld
						//resultParam.CalculatedLog.Calculate_CalParamResult(curWeight,
						//													iCalculatedParam,
						//													apModifierBase.BLEND_METHOD.Additive,
						//													CalculateLog_4_VertWorld);
					}

					iCalculatedParam++;
				}

				//HashCode를 만들자.
				//if (isMakeHashCode)
				//{
				//	if (_result_VertWorld.Length > 0)
				//	{
				//		_hashCode_VertWorld = _result_VertWorld[(_result_VertWorld.Length - 1) / 2].GetHashCode();
				//	}
				//}
			}
			//--------------------------------------------------------------------
		}


		//private Vector2 BlendPosition_ITP(Vector2 prevResult, Vector2 nextResult, float prevWeight, float nextWeight)
		private Vector2 BlendPosition_ITP(Vector2 prevResult, Vector2 nextResult, float nextWeight)//<<Prev를 삭제했다.
		{
			//return ((prevResult * prevWeight) + (nextResult * nextWeight)) / (prevWeight + nextWeight);
			return ((prevResult * (1.0f - nextWeight)) + (nextResult * nextWeight));
		}

		private Vector2 BlendPosition_Add(Vector2 prevResult, Vector2 nextResult, float nextWeight)
		{
			return prevResult + nextResult * nextWeight;
		}

		//private void BlendMatrix_ITP(apMatrix prevResult, apMatrix nextResult, float prevWeight, float nextWeight)
		private void BlendMatrix_ITP(apMatrix prevResult, apMatrix nextResult, float nextWeight)
		{
			//prevResult._pos = ((prevResult._pos * prevWeight) + (nextResult._pos * nextWeight)) / (prevWeight + nextWeight);
			//prevResult._angleDeg = ((prevResult._angleDeg * prevWeight) + (nextResult._angleDeg * nextWeight)) / (prevWeight + nextWeight);
			//prevResult._scale = ((prevResult._scale * prevWeight) + (nextResult._scale * nextWeight)) / (prevWeight + nextWeight);


			//이전 방식의 ITP 처리 방식
			//float totalWeight = prevWeight + nextWeight;
			//if(totalWeight <= 0.0f)
			//{
			//	return;
			//}

			if (nextWeight <= 0.0f)
			{
				return;
			}


			//prevResult.LerpMartix(nextResult, nextWeight / totalWeight);
			prevResult.LerpMartix(nextResult, nextWeight / 1.0f);
		}

		private void BlendMatrix_Add(apMatrix prevResult, apMatrix nextResult, float nextWeight)
		{
			prevResult._pos += nextResult._pos * nextWeight;
			prevResult._angleDeg += nextResult._angleDeg * nextWeight;
			//prevResult._scale += nextResult._scale * nextWeight;

			//prevResult._scale += nextResult._scale * nextWeight;//이건 오류가 있다. 100% + 100% => 200%으로 계산..

			prevResult._scale.x = (prevResult._scale.x * (1.0f - nextWeight)) + (prevResult._scale.x * nextResult._scale.x * nextWeight);
			prevResult._scale.y = (prevResult._scale.y * (1.0f - nextWeight)) + (prevResult._scale.y * nextResult._scale.y * nextWeight);
			//prevResult._scale.z = (prevResult._scale.z * (1.0f - nextWeight)) + (prevResult._scale.z * nextResult._scale.z * nextWeight);
		}




		// Get / Set
		//---------------------------------------------------
		public bool IsRigging { get { return _isAnyRigging; } }
		public bool IsVertexLocal { get { return _isAnyVertLocal; } }
		public bool IsVertexWorld { get { return _isAnyVertWorld; } }

		public apMatrix3x3 GetMatrixRigging(int vertexIndex)
		{
			return _result_RiggingMatrices[vertexIndex];
		}

		public Vector2 GetVertexRigging(int vertexIndex)
		{
			return _result_Rigging[vertexIndex];
		}

		public float GetRiggingWeight()
		{
			return _result_RiggingWeight;
		}

		public Vector2 GetVertexLocalPos(int vertexIndex)
		{
			return _result_VertLocal[vertexIndex];
		}

		public apMatrix3x3 MeshWorldMatrix
		{
			get
			{
				if (_isAnyTransformation)
				{
					return _result_MeshTransform.MtrxToSpace;
				}
				return apMatrix3x3.identity;
			}
		}

		public apMatrix MeshWorldMatrixWrap
		{
			get
			{
				if (_isAnyTransformation)
				{
					return _result_MeshTransform;
				}
				return null;
			}
		}

		public Vector2 GetVertexWorldPos(int vertexIndex)
		{
			return _result_VertWorld[vertexIndex];
		}

		/// <summary>
		/// MeshColor/Visible이 Modifier로 계산이 되었는가
		/// </summary>
		public bool IsAnyColorCalculated
		{
			get
			{
				return _isAnyMeshColor && _result_CalculatedColor;
			}
		}

		public Color MeshColor
		{
			get
			{
				if (_isAnyMeshColor)
				{
					return _result_Color;
				}
				return _color_Default;
			}
		}

		public bool IsMeshVisible
		{
			get
			{
				if (_isAnyMeshColor)
				{
					return _result_IsVisible;
				}
				return true;
			}
		}

	}

}