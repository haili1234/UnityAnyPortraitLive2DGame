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
//using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// RenderUnit의 ResultStack의 Opt 버전
	/// OptTransform에 포함되어 Calculate를 한다.
	/// Blend를 통해 최종 결과를 만들어낸다.
	/// </summary>
	public class apOptCalculatedResultStack
	{
		// Members
		//--------------------------------------------
		private apOptTransform _parentOptTransform = null;
		private apOptMesh _targetOptMesh = null;

		private List<apOptCalculatedResultParam> _resultParams_VertLocal = new List<apOptCalculatedResultParam>();
		private List<apOptCalculatedResultParam> _resultParams_Transform = new List<apOptCalculatedResultParam>();
		private List<apOptCalculatedResultParam> _resultParams_MeshColor = new List<apOptCalculatedResultParam>();
		private List<apOptCalculatedResultParam> _resultParams_VertWorld = new List<apOptCalculatedResultParam>();
		private List<apOptCalculatedResultParam> _resultParams_Rigging = new List<apOptCalculatedResultParam>();

		//BoneTransform은 바로 apCalculatedResultParam 리스트를 만드는게 아니라 2중으로 묶어야 한다.
		//키값은 Bone
		private List<OptBoneAndModParamPair> _resultParams_BoneTransform = new List<OptBoneAndModParamPair>();

		//public List<Vector2> _result_VertLocal = null;
		public Vector2[] _result_VertLocal = null;//<<최적화를 위해 변경
		public apMatrix _result_MeshTransform = new apMatrix();
		public Color _result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		//public List<Vector2> _result_VertWorld = null;


		public Vector2[] _result_VertWorld = null;

		private Vector2 _tmpPos = Vector2.zero;
		private apMatrix3x3 _tmpMatrix = apMatrix3x3.identity;
		private apOptVertexRequest.VertRigWeightTable _tmpVertRigWeightTable = null;
		//추가
		//Rigging Result
		//public Vector2[] _result_Rigging = null;//<<이건 사용하지 않는다.
		public float _result_RiggingWeight = 0.0f;
		public apMatrix3x3[] _result_RiggingMatrices = null;

		//Bone Transform
		//값을 계속 초기화해서 사용하는 지역변수의 역할
		private apMatrix _result_BoneTransform = new apMatrix();

		private bool _result_CalculatedColor = false;



		public bool _result_IsVisible = true;
		private int _nMeshColorCalculated = 0;

		private bool _isAnyVertLocal = false;
		private bool _isAnyTransformation = false;
		private bool _isAnyMeshColor = false;
		private bool _isAnyVertWorld = false;
		private bool _isAnyRigging = false;
		private bool _isAnyBoneTransform = false;


		//private Color _color_Default = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		//private Vector3 _color_2XTmp_Prev = Vector3.zero;
		//private Vector3 _color_2XTmp_Next = Vector3.zero;

		private int _iCalculatedParam = 0;

		/// <summary>
		/// Bone 처리에 대한 Pair
		/// Bone을 키값으로 하여 Modifier -> CalculateResultParam List를 저장한다.
		/// </summary>
		public class OptBoneAndModParamPair
		{
			public apOptBone _keyBone = null;
			public Dictionary<apOptModifierUnitBase, OptModifierAndResultParamListPair> _modParamPairs_ModKey = new Dictionary<apOptModifierUnitBase, OptModifierAndResultParamListPair>();
			public List<OptModifierAndResultParamListPair> _modParamPairs = new List<OptModifierAndResultParamListPair>();

			public OptBoneAndModParamPair(apOptBone bone)
			{
				_keyBone = bone;
			}

			public void AddCalculatedResultParam(apOptCalculatedResultParam calculatedResultParam)
			{
				apOptModifierUnitBase modifier = calculatedResultParam._linkedModifier;
				if (modifier == null)
				{ return; }

				OptModifierAndResultParamListPair modParamPair = null;
				if (!_modParamPairs_ModKey.ContainsKey(modifier))
				{
					modParamPair = new OptModifierAndResultParamListPair(modifier);
					_modParamPairs_ModKey.Add(modifier, modParamPair);
					_modParamPairs.Add(modParamPair);
				}
				else
				{
					modParamPair = _modParamPairs_ModKey[modifier];
				}
				modParamPair.AddCalculatedResultParam(calculatedResultParam);
			}

			public bool Remove(apOptCalculatedResultParam calculatedResultParam)
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
					_modParamPairs.RemoveAll(delegate (OptModifierAndResultParamListPair a)
					{
						return a._resultParams.Count == 0;
					});

					for (int i = 0; i < _modParamPairs.Count; i++)
					{
						OptModifierAndResultParamListPair modPair = _modParamPairs[i];

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
				_modParamPairs.Sort(delegate (OptModifierAndResultParamListPair a, OptModifierAndResultParamListPair b)
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
		public class OptModifierAndResultParamListPair
		{
			public apOptModifierUnitBase _keyModifier;
			public List<apOptCalculatedResultParam> _resultParams = new List<apOptCalculatedResultParam>();

			public OptModifierAndResultParamListPair(apOptModifierUnitBase modifier)
			{
				_keyModifier = modifier;
			}

			public void AddCalculatedResultParam(apOptCalculatedResultParam calculatedResultParam)
			{
				if (!_resultParams.Contains(calculatedResultParam))
				{
					_resultParams.Add(calculatedResultParam);
				}
			}

			public void Remove(apOptCalculatedResultParam calculatedResultParam)
			{
				_resultParams.Remove(calculatedResultParam);
			}
		}

		// Init
		//--------------------------------------------
		public apOptCalculatedResultStack(apOptTransform parentOptTransform)
		{
			_parentOptTransform = parentOptTransform;
			_targetOptMesh = _parentOptTransform._childMesh;

		}

		// Functions
		//--------------------------------------------

		// Add / Remove / Sort
		//----------------------------------------------------------------------
		public void AddCalculatedResultParam(apOptCalculatedResultParam resultParam)
		{
			if ((int)(resultParam._calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
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
				else if (resultParam._calculatedSpace == apCalculatedResultParam.CALCULATED_SPACE.Rigging)//<<추가
				{
					if (!_resultParams_Rigging.Contains(resultParam))
					{
						_resultParams_Rigging.Add(resultParam);
					}
					_isAnyRigging = true;
				}
			}
			if ((int)(resultParam._calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.TransformMatrix) != 0)
			{
				//변경 : Bone타입과 일반 Transform타입으로 나뉜다.
				if (resultParam._targetBone != null)
				{

					//Bone 타입이다.
					//Modifier + ResultParam Pair로 저장해야한다.
					OptBoneAndModParamPair modParamPair = _resultParams_BoneTransform.Find(delegate (OptBoneAndModParamPair a)
					{
						return a._keyBone == resultParam._targetBone;
					});
					if (modParamPair == null)
					{
						modParamPair = new OptBoneAndModParamPair(resultParam._targetBone);
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
				if (!_resultParams_MeshColor.Contains(resultParam))
				{
					_resultParams_MeshColor.Add(resultParam);
					_isAnyMeshColor = true;
				}
			}
		}


		public void ClearResultParams()
		{
			//Debug.LogError("[" + _tmpID + "] Clear Result Params");
			_resultParams_Rigging.Clear();
			_resultParams_VertLocal.Clear();
			_resultParams_Transform.Clear();
			_resultParams_MeshColor.Clear();
			_resultParams_VertWorld.Clear();
			_resultParams_BoneTransform.Clear();


			_isAnyVertLocal = false;
			_isAnyTransformation = false;
			_isAnyMeshColor = false;
			_isAnyVertWorld = false;

			_isAnyRigging = false;
			_isAnyBoneTransform = false;
		}


		public void Sort()
		{
			//다른 RenderUnit에 대해서는
			//Level이 큰게(하위) 먼저 계산되도록 내림차순 정렬 > 변경 ) Level 낮은 상위가 먼저 계산되도록 (오름차순)

			//같은 RenderUnit에 대해서는
			//오름차순 정렬 (레이어 값이 낮은 것 부터 처리할 수 있도록)
			_resultParams_Rigging.Sort(delegate (apOptCalculatedResultParam a, apOptCalculatedResultParam b)
			{
				if (a._targetOptTransform == b._targetOptTransform)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetOptTransform._level - b._targetOptTransform._level; }
			});

			_resultParams_VertLocal.Sort(delegate (apOptCalculatedResultParam a, apOptCalculatedResultParam b)
			{
				if (a._targetOptTransform == b._targetOptTransform)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetOptTransform._level - b._targetOptTransform._level; }
			});

			_resultParams_Transform.Sort(delegate (apOptCalculatedResultParam a, apOptCalculatedResultParam b)
			{
				if (a._targetOptTransform == b._targetOptTransform)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetOptTransform._level - b._targetOptTransform._level; }
			});

			_resultParams_MeshColor.Sort(delegate (apOptCalculatedResultParam a, apOptCalculatedResultParam b)
			{
				if (a._targetOptTransform == b._targetOptTransform)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetOptTransform._level - b._targetOptTransform._level; }
			});

			_resultParams_VertWorld.Sort(delegate (apOptCalculatedResultParam a, apOptCalculatedResultParam b)
			{
				if (a._targetOptTransform == b._targetOptTransform)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetOptTransform._level - b._targetOptTransform._level; }
			});

			for (int i = 0; i < _resultParams_BoneTransform.Count; i++)
			{
				_resultParams_BoneTransform[i].Sort();
			}
		}

		// Calculate Update
		//----------------------------------------------------------------------
		/// <summary>
		/// [Please do not use it]
		/// Ready To Calulate의 함수를 Bake에서 실행
		/// 배열 초기화 코드가 들어가있다.
		/// </summary>
		public void ResetVerticesOnBake()
		{
			if (_targetOptMesh == null)
			{
				_result_VertLocal = null;
				_result_VertWorld = null;
				_result_RiggingMatrices = null;
				_result_RiggingWeight = 0.0f;
				return;
			}

			int nVerts = _targetOptMesh.RenderVertices.Length;
			if(_isAnyVertLocal)
			{
				_result_VertLocal = new Vector2[nVerts];

				for (int i = 0; i < nVerts; i++)
				{
					_result_VertLocal[i] = Vector2.zero;
				}
			}
			if(_isAnyVertWorld)
			{
				_result_VertWorld = new Vector2[nVerts];

				for (int i = 0; i < nVerts; i++)
				{
					_result_VertWorld[i] = Vector2.zero;
				}
			}
			if(_isAnyRigging)
			{
				_result_RiggingMatrices = new apMatrix3x3[nVerts];
				_result_RiggingWeight = 0.0f;

				for (int i = 0; i < nVerts; i++)
				{
					_result_RiggingMatrices[i].SetIdentity();
				}
			}

			if(_resultParams_VertLocal != null)
			{
				for (int i = 0; i < _resultParams_VertLocal.Count; i++)
				{
					_resultParams_VertLocal[i].ResetVerticesOnBake();
				}
			}

			if(_resultParams_Transform != null)
			{
				for (int i = 0; i < _resultParams_Transform.Count; i++)
				{
					_resultParams_Transform[i].ResetVerticesOnBake();
				}
			}
			if(_resultParams_MeshColor != null)
			{
				for (int i = 0; i < _resultParams_MeshColor.Count; i++)
				{
					_resultParams_MeshColor[i].ResetVerticesOnBake();
				}
			}
			if(_resultParams_VertWorld != null)
			{
				for (int i = 0; i < _resultParams_VertWorld.Count; i++)
				{
					_resultParams_VertWorld[i].ResetVerticesOnBake();
				}
			}
			if(_resultParams_Rigging != null)
			{
				for (int i = 0; i < _resultParams_Rigging.Count; i++)
				{
					_resultParams_Rigging[i].ResetVerticesOnBake();
				}
			}
		}

		public void ReadyToCalculate()
		{
			//TODO : 여기서부터 작성

			if (_targetOptMesh == null)
			{
				if (_isAnyVertLocal)
				{
					Debug.LogError("AnyPortrait : Error No Mesh");
				}
				return;
			}
			
			int nRenderVerts = _targetOptMesh.RenderVertices.Length;

			//수정 3.22
			//따로 리셋하자.
			if(_isAnyVertLocal)
			{
				if (_result_VertLocal == null || _result_VertLocal.Length != nRenderVerts)
				{
					_result_VertLocal = new Vector2[nRenderVerts];
				}

				for (int i = 0; i < nRenderVerts; i++)
				{
					_result_VertLocal[i] = Vector2.zero;
				}
			}

			
			if(_isAnyVertWorld)
			{
				if (_result_VertWorld == null || _result_VertWorld.Length != nRenderVerts)
				{
					_result_VertWorld = new Vector2[nRenderVerts];
				}

				for (int i = 0; i < nRenderVerts; i++)
				{
					_result_VertWorld[i] = Vector2.zero;
				}
			}

			if (_isAnyRigging)
			{
				if (_result_RiggingMatrices == null || _result_RiggingMatrices.Length != nRenderVerts)
				{
					_result_RiggingMatrices = new apMatrix3x3[nRenderVerts];
				}

				for (int i = 0; i < nRenderVerts; i++)
				{
					_result_RiggingMatrices[i].SetIdentity();
				}

				_result_RiggingWeight = 0.0f;
			}

			

			#region [미사용 코드 : 모디파이어에 따라 다 분리시켰다]
			//if (_isAnyVertLocal || _isAnyVertWorld || _isAnyRigging)
			//{

			//	if (_result_VertLocal == null || _result_VertLocal.Length != _targetOptMesh.RenderVertices.Length)
			//	{
			//		//RenderUnit의 RenderVertex 개수 만큼 결과를 만들자
			//		_result_VertLocal = new Vector2[_targetOptMesh.RenderVertices.Length];
			//		_result_VertWorld = new Vector2[_targetOptMesh.RenderVertices.Length];
			//		//_result_Rigging = new Vector2[_targetOptMesh.RenderVertices.Length];
			//		_result_RiggingMatrices = new apMatrix3x3[_targetOptMesh.RenderVertices.Length];
			//		_result_RiggingWeight = 0.0f;


			//		for (int i = 0; i < nRenderVerts; i++)
			//		{
			//			_result_VertLocal[i] = Vector2.zero;
			//			_result_VertWorld[i] = Vector2.zero;
			//			//_result_Rigging[i] = Vector2.zero;
			//			_result_RiggingMatrices[i].SetIdentity();
			//		}
			//	}
			//	else
			//	{
			//		_result_RiggingWeight = 0.0f;

			//		for (int i = 0; i < _result_VertLocal.Length; i++)
			//		{
			//			_result_VertLocal[i] = Vector2.zero;
			//			_result_VertWorld[i] = Vector2.zero;
			//			//_result_Rigging[i] = Vector2.zero;
			//			_result_RiggingMatrices[i].SetIdentity();
			//		}
			//	}
			//}

			//_result_BoneTransform.SetIdentity();
			//_result_MeshTransform.SetIdentity();
			//_result_MeshTransform.MakeMatrix();
			////_result_Color = _color_Default;
			//_result_Color = _parentOptTransform._meshColor2X_Default;
			//if (!_parentOptTransform._isVisible_Default)
			//{
			//	_result_Color.a = 0.0f;
			//} 
			#endregion

			_result_MeshTransform.SetIdentity();
			_result_MeshTransform.MakeMatrix();
			_result_Color = _parentOptTransform._meshColor2X_Default;
			if (!_parentOptTransform._isVisible_Default)
			{
				_result_Color.a = 0.0f;
			}
			_result_BoneTransform.SetIdentity();

			_result_IsVisible = true;
			_result_CalculatedColor = false;
		}


		private float _cal_prevWeight = 0.0f;
		private float _cal_curWeight = 0.0f;
		private apOptCalculatedResultParam _cal_resultParam = null;
		private Vector2[] _cal_posVerts = null;
		//private apMatrix3x3[] _cal_vertMatrices = null;
		private List<apOptVertexRequest> _cal_vertRequestList = null;
		private apOptVertexRequest _cal_vertRequest = null;
		private apOptVertexRequest.ModWeightPair _cal_vertRequestModWeightPair = null;


		/// <summary>
		/// Calculate Result Statck의 업데이트 부분
		/// Pre-Update로서 VertWorld와 Rigging이 제외된다.
		/// </summary>
		public void Calculate_Pre()
		{
			//bool isFirstDebug = true;
			_cal_prevWeight = 0.0f;
			_cal_curWeight = 0.0f;
			_cal_resultParam = null;
			_cal_posVerts = null;


			//Debug.Log("Is Any Vert Local : " + _isAnyVertLocal + " [" + _resultParams_VertLocal.Count +"]");
			// 1. Local Morph
			if (_isAnyVertLocal)
			{

//#if UNITY_EDITOR
//				Profiler.BeginSample("Calcuate Result Stack - 1. Vert Local");
//#endif
				_cal_prevWeight = 0.0f;
				_cal_curWeight = 0.0f;
				_cal_resultParam = null;
				_cal_posVerts = null;
				_cal_vertRequestList = null;
				_cal_vertRequest = null;

				_iCalculatedParam = 0;//<<추가 : 첫 모디파이어는 무조건 Interpolation으로 만들자


				#region [미사용 코드 : 최적화 전]
				//이전 코드
				//for (int iParam = 0; iParam < _resultParams_VertLocal.Count; iParam++)
				//{
				//	_cal_resultParam = _resultParams_VertLocal[iParam];
				//	_cal_curWeight = _cal_resultParam.ModifierWeight;

				//	if (!_cal_resultParam.IsModifierAvailable || _cal_curWeight <= 0.001f)
				//	{ continue; }


				//	_cal_posVerts = _cal_resultParam._result_Positions;
				//	if (_result_VertLocal == null)
				//	{
				//		Debug.LogError("Result Vert Local is Null");
				//	}
				//	if (_cal_posVerts == null)
				//	{
				//		Debug.LogError("Cal Pos Vert is Null");
				//	}
				//	if (_cal_posVerts.Length != _result_VertLocal.Length)
				//	{
				//		//결과가 잘못 들어왔다 갱신 필요
				//		Debug.LogError("Wrong Vert Local Result (Cal : " + _cal_posVerts.Length + " / Verts : " + _result_VertLocal.Length + ")");
				//		continue;
				//	}

				//	// Blend 방식에 맞게 Pos를 만들자
				//	if (_cal_resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || _iCalculatedParam == 0)
				//	{
				//		for (int i = 0; i < _cal_posVerts.Length; i++)
				//		{
				//			_result_VertLocal[i] = BlendPosition_ITP(_result_VertLocal[i], _cal_posVerts[i], _cal_curWeight);
				//		}

				//		_cal_prevWeight += _cal_curWeight;
				//	}
				//	else
				//	{
				//		for (int i = 0; i < _cal_posVerts.Length; i++)
				//		{
				//			_result_VertLocal[i] = BlendPosition_Add(_result_VertLocal[i], _cal_posVerts[i], _cal_curWeight);
				//		}
				//	}
				//	_iCalculatedParam++;
				//} 
				#endregion

				//최적화된 코드.
				//Vertex Pos를 건드리지 않고 보간식을 중첩한다.
				for (int iParam = 0; iParam < _resultParams_VertLocal.Count; iParam++)
				{
					_cal_resultParam = _resultParams_VertLocal[iParam];
					_cal_resultParam._resultWeight = 0.0f;
					_cal_curWeight = _cal_resultParam.ModifierWeight;

					if (!_cal_resultParam.IsModifierAvailable || _cal_curWeight <= 0.001f)
					{ continue; }

					_cal_resultParam._resultWeight = 1.0f;//<<일단 Weight를 1로 두고 계산 시작
					

					// Blend 방식에 맞게 Pos를 만들자
					if (_cal_resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || _iCalculatedParam == 0)
					{
						//Interpolation : Prev * (1-weight) + Next * weight
						_cal_resultParam._resultWeight = _cal_curWeight;
						for (int iPrev = 0; iPrev < iParam - 1; iPrev++)
						{
							_resultParams_VertLocal[iPrev]._resultWeight *= (1.0f - _cal_curWeight);
						}
					}
					else
					{
						//Additive : Prev + Next * weight
						_cal_resultParam._resultWeight = _cal_curWeight;
					}
					_iCalculatedParam++;
				}

				//이제 계산된 Weight를 모두 입력해주자
				for (int iParam = 0; iParam < _resultParams_VertLocal.Count; iParam++)
				{
					_cal_resultParam = _resultParams_VertLocal[iParam];
					_cal_vertRequestList = _cal_resultParam._result_VertLocalPairs;
					for (int iVR = 0; iVR < _cal_vertRequestList.Count; iVR++)
					{
						_cal_vertRequestList[iVR].MultiplyWeight(_cal_resultParam._resultWeight);
					}
				}

//#if UNITY_EDITOR
//				Profiler.EndSample();
//#endif
			}

			// 2. Mesh / MeshGroup Transformation
			if (_isAnyTransformation)
			{
//#if UNITY_EDITOR
//				Profiler.BeginSample("Calcuate Result Stack - 2. MeshGroup Transformation");
//#endif
				_cal_prevWeight = 0.0f;
				_cal_curWeight = 0.0f;
				_cal_resultParam = null;

				_iCalculatedParam = 0;

				//Debug.Log("Update TF - OPT");
				for (int iParam = 0; iParam < _resultParams_Transform.Count; iParam++)
				{
					_cal_resultParam = _resultParams_Transform[iParam];
					_cal_curWeight = _cal_resultParam.ModifierWeight;

					if (!_cal_resultParam.IsModifierAvailable || _cal_curWeight <= 0.001f)
					{ continue; }



					// Blend 방식에 맞게 Matrix를 만들자 하자
					if (_cal_resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || _iCalculatedParam == 0)
					{
						//Debug.Log("Cal TF [ITP] : " + _cal_resultParam._result_Matrix.Pos3 + " (Weight : " + _cal_curWeight + ")");

						BlendMatrix_ITP(_result_MeshTransform, _cal_resultParam._result_Matrix, _cal_curWeight);

						//if (_cal_resultParam._result_Matrix.Scale2.magnitude < 0.5f || _cal_curWeight < 0.5f)
						//{
						//	Debug.Log("Cal TF [ITP] : " + _cal_resultParam._result_Matrix.Scale2 + " > " + _result_MeshTransform.Scale2 + " (Weight : " + _cal_curWeight + ")");
						//}

						_cal_prevWeight += _cal_curWeight;

					}
					else
					{
						BlendMatrix_Add(_result_MeshTransform, _cal_resultParam._result_Matrix, _cal_curWeight);
					}

					_iCalculatedParam++;
				}

				_result_MeshTransform.MakeMatrix();

				if (_result_MeshTransform._scale.magnitude < 0.5f)
				{
					Debug.Log("Cal TF [ITP] : " + _result_MeshTransform._scale + " (Total Weight : " + _cal_prevWeight + ")");
				}

//#if UNITY_EDITOR
//				Profiler.EndSample();
//#endif
			}

			// 3. Mesh Color
			if (_isAnyMeshColor)
			{
//#if UNITY_EDITOR
//				Profiler.BeginSample("Calcuate Result Stack - 3. Mesh Color");
//#endif
				_cal_prevWeight = 0.0f;
				_cal_curWeight = 0.0f;
				_cal_resultParam = null;

				_iCalculatedParam = 0;

				_result_IsVisible = false;
				_nMeshColorCalculated = 0;
				_result_CalculatedColor = false;

				for (int iParam = 0; iParam < _resultParams_MeshColor.Count; iParam++)
				{
					_cal_resultParam = _resultParams_MeshColor[iParam];
					_cal_curWeight = Mathf.Clamp01(_cal_resultParam.ModifierWeight);

					if (!_cal_resultParam.IsModifierAvailable
						|| _cal_curWeight <= 0.001f
						|| !_cal_resultParam.IsColorValueEnabled
						|| !_cal_resultParam._isColorCalculated//<<추가 : Color로 등록했지만 아예 계산이 안되었을 수도 있다.
						)
					{
						continue;
					}//<<TODO


					// Blend 방식에 맞게 Matrix를 만들자 하자
					if (_cal_resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || _iCalculatedParam == 0)
					{
						//_result_Color = BlendColor_ITP(_result_Color, _cal_resultParam._result_Color, _cal_prevWeight, _cal_curWeight);
						_result_Color = apUtil.BlendColor_ITP(_result_Color, _cal_resultParam._result_Color, _cal_curWeight);
						_cal_prevWeight += _cal_curWeight;
					}
					else
					{
						_result_Color = apUtil.BlendColor_Add(_result_Color, _cal_resultParam._result_Color, _cal_curWeight);
					}

					_result_IsVisible |= _cal_resultParam._result_IsVisible;
					_nMeshColorCalculated++;

					_result_CalculatedColor = true;//<<"계산된 MeshColor" Result가 있음을 알린다.

					_iCalculatedParam++;
				}

				if (_nMeshColorCalculated == 0)
				{
					_result_IsVisible = true;
				}


//#if UNITY_EDITOR
//				Profiler.EndSample();
//#endif
			}
			else
			{
				_result_IsVisible = true;
			}

			//AnyBoneTransform
			if (_isAnyBoneTransform)
			{
//#if UNITY_EDITOR
//				Profiler.BeginSample("Calcuate Result Stack - 5. Bone Transform");
//#endif
				_cal_prevWeight = 0.0f;
				_cal_curWeight = 0.0f;
				_cal_resultParam = null;


				OptBoneAndModParamPair boneModPair = null;
				apOptBone targetBone = null;
				List<OptModifierAndResultParamListPair> modParamPairs = null;

				
				for (int iBonePair = 0; iBonePair < _resultParams_BoneTransform.Count; iBonePair++)
				{
					boneModPair = _resultParams_BoneTransform[iBonePair];
					targetBone = boneModPair._keyBone;
					modParamPairs = boneModPair._modParamPairs;

					if (targetBone == null || modParamPairs.Count == 0)
					{
						continue;
					}

					
					_iCalculatedParam = 0;
					_result_BoneTransform.SetIdentity();

					for (int iModParamPair = 0; iModParamPair < modParamPairs.Count; iModParamPair++)
					{
						OptModifierAndResultParamListPair modParamPair = modParamPairs[iModParamPair];

						for (int iParam = 0; iParam < modParamPair._resultParams.Count; iParam++)
						{
							_cal_resultParam = modParamPair._resultParams[iParam];

							_cal_curWeight = _cal_resultParam.ModifierWeight;

							if (!_cal_resultParam.IsModifierAvailable || _cal_curWeight <= 0.001f)
							{ continue; }

							//if(_cal_resultParam._result_Matrix._scale.magnitude < 0.3f)
							//{
							//	Debug.LogError("[" + targetBone.name + "] 너무 작은 Cal Matrix : " + _cal_resultParam._linkedModifier._name);
							//}
							// Blend 방식에 맞게 Matrix를 만들자 하자
							if (_cal_resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || _iCalculatedParam == 0)
							{
								BlendMatrix_ITP(_result_BoneTransform, _cal_resultParam._result_Matrix, _cal_curWeight);
								_cal_prevWeight += _cal_curWeight;
							}
							else
							{
								BlendMatrix_Add(_result_BoneTransform, _cal_resultParam._result_Matrix, _cal_curWeight);
							}
							
							_iCalculatedParam++;
						}
					}

					//참조된 본에 직접 값을 넣어주자
					targetBone.UpdateModifiedValue(_result_BoneTransform._pos, _result_BoneTransform._angleDeg, _result_BoneTransform._scale);

					
				}

//#if UNITY_EDITOR
//				Profiler.EndSample();
//#endif
			}

		}



		/// <summary>
		/// Calculate Result Statck의 업데이트 부분
		/// Pre-Update로서 VertWorld와 Rigging이 제외된다.
		/// </summary>
		public void Calculate_Post()
		{
			//bool isFirstDebug = true;
			_cal_prevWeight = 0.0f;
			_cal_curWeight = 0.0f;
			_cal_resultParam = null;
			_cal_posVerts = null;


			// Rigging
			if (_isAnyRigging)
			{
//#if UNITY_EDITOR
//				Profiler.BeginSample("Calcuate Result Stack - 0. Rigging");
//#endif
				_cal_prevWeight = 0.0f;
				_cal_curWeight = 0.0f;
				_cal_resultParam = null;
				//_cal_posVerts = null;
				//_cal_vertMatrices = null;

				_iCalculatedParam = 0;


				_cal_vertRequestList = null;
				_cal_vertRequest = null;

				_result_RiggingWeight = 0.0f;

				

				for (int iParam = 0; iParam < _resultParams_Rigging.Count; iParam++)
				{
					_cal_resultParam = _resultParams_Rigging[iParam];
					_cal_resultParam._resultWeight = 0.0f;
					_cal_curWeight = _cal_resultParam.ModifierWeight;

					if (!_cal_resultParam.IsModifierAvailable || _cal_curWeight <= 0.001f)
					{
						continue;
					}

					_cal_resultParam._resultWeight = 1.0f;//<<일단 Weight를 1로 두고 계산 시작
					
					

					_result_RiggingWeight += _cal_curWeight;

					// Blend 방식에 맞게 Pos를 만들자
					if (_cal_resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || _iCalculatedParam == 0)
					{
						//Interpolation : Prev * (1-weight) + Next * weight
						_cal_resultParam._resultWeight = _cal_curWeight;
						for (int iPrev = 0; iPrev < iParam - 1; iPrev++)
						{
							_resultParams_Rigging[iPrev]._resultWeight *= (1.0f - _cal_curWeight);
						}
					}
					else
					{
						//Additive : Prev + Next * weight
						_cal_resultParam._resultWeight = _cal_curWeight;
					}

					_iCalculatedParam++;

				}

				if (_result_RiggingWeight > 1.0f)
				{
					_result_RiggingWeight = 1.0f;
				}

				//이제 계산된 Weight를 모두 입력해주자
				for (int iParam = 0; iParam < _resultParams_Rigging.Count; iParam++)
				{
					_cal_resultParam = _resultParams_Rigging[iParam];
					_cal_vertRequestList = _cal_resultParam._result_VertLocalPairs;
					for (int iVR = 0; iVR < _cal_vertRequestList.Count; iVR++)
					{
						_cal_vertRequestList[iVR].MultiplyWeight(_cal_resultParam._resultWeight);
					}
				}

//#if UNITY_EDITOR
//				Profiler.EndSample();
//#endif
			}



			// 4. World Morph
			if (_isAnyVertWorld)
			{
				
//#if UNITY_EDITOR
//				Profiler.BeginSample("Calcuate Result Stack - 4. Vert World");
//#endif
				_cal_prevWeight = 0.0f;
				_cal_curWeight = 0.0f;
				_cal_resultParam = null;
				_cal_posVerts = null;

				_iCalculatedParam = 0;

				for (int iParam = 0; iParam < _resultParams_VertWorld.Count; iParam++)
				{
					_cal_resultParam = _resultParams_VertWorld[iParam];
					_cal_curWeight = _cal_resultParam.ModifierWeight;

					if (!_cal_resultParam.IsModifierAvailable || _cal_curWeight <= 0.001f)
					{ continue; }

					//Debug.Log("Vert World [" + iParam + "] (" + _cal_curWeight + ")");

					_cal_posVerts = _cal_resultParam._result_Positions;
					if (_cal_posVerts.Length != _result_VertWorld.Length)
					{
						//결과가 잘못 들어왔다 갱신 필요
						Debug.LogError("Wrong Vert World Result (Cal : " + _cal_posVerts.Length + " / Verts : " + _result_VertWorld.Length + ")");
						continue;
					}

					// Blend 방식에 맞게 Pos를 만들자
					if (_cal_resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || _iCalculatedParam == 0)
					{
						for (int i = 0; i < _cal_posVerts.Length; i++)
						{
							_result_VertWorld[i] = BlendPosition_ITP(_result_VertWorld[i], _cal_posVerts[i], _cal_curWeight);
						}

						_cal_prevWeight += _cal_curWeight;
					}
					else
					{
						for (int i = 0; i < _cal_posVerts.Length; i++)
						{
							_result_VertWorld[i] = BlendPosition_Add(_result_VertWorld[i], _cal_posVerts[i], _cal_curWeight);
						}
					}


					_iCalculatedParam++;


				}

//#if UNITY_EDITOR
//				Profiler.EndSample();
//#endif
				//Debug.Log(" >> Vert World");

			}
		}



		// Blend ITP
		//------------------------------------------------------------------------
		//private Vector2 BlendPosition_ITP(Vector2 prevResult, Vector2 nextResult, float prevWeight, float nextWeight)
		private Vector2 BlendPosition_ITP(Vector2 prevResult, Vector2 nextResult, float nextWeight)
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

			//float totalWeight = prevWeight + nextWeight;
			//if(totalWeight <= 0.0f)
			//{
			//	return;
			//}

			//float totalWeight = 1.0f;

			//prevResult.LerpMartix(nextResult, nextWeight / totalWeight);
			prevResult.LerpMartix(nextResult, nextWeight);
		}

		private void BlendMatrix_Add(apMatrix prevResult, apMatrix nextResult, float nextWeight)
		{
			prevResult._pos += nextResult._pos * nextWeight;
			prevResult._angleDeg += nextResult._angleDeg * nextWeight;
			//prevResult._scale += nextResult._scale * nextWeight;

			prevResult._scale.x = (prevResult._scale.x * (1.0f - nextWeight)) + (prevResult._scale.x * nextResult._scale.x * nextWeight);
			prevResult._scale.y = (prevResult._scale.y * (1.0f - nextWeight)) + (prevResult._scale.y * nextResult._scale.y * nextWeight);
			//prevResult._scale.z = (prevResult._scale.z * (1.0f - nextWeight)) + (prevResult._scale.z * nextResult._scale.z * nextWeight);
		}



		// Get / Set
		//--------------------------------------------
		public bool IsVertexLocal { get { return _isAnyVertLocal; } }
		public bool IsVertexWorld { get { return _isAnyVertWorld; } }
		public bool IsRigging { get { return _isAnyRigging; } }


		public Vector2 GetDeferredLocalPos(int vertexIndex)
		{
			_tmpPos = Vector2.zero;

			for (int iParam = 0; iParam < _resultParams_VertLocal.Count; iParam++)
			{
				_cal_resultParam = _resultParams_VertLocal[iParam];
				for (int iVR = 0; iVR < _cal_resultParam._result_VertLocalPairs.Count; iVR++)
				{
					_cal_vertRequest = _cal_resultParam._result_VertLocalPairs[iVR];
					if(!_cal_vertRequest._isCalculated ||_cal_vertRequest._totalWeight == 0.0f)
					{
						continue;
					}

					for (int iModPair = 0; iModPair < _cal_vertRequest._nModWeightPairs; iModPair++)
					{
						_cal_vertRequestModWeightPair = _cal_vertRequest._modWeightPairs[iModPair];
						if(!_cal_vertRequestModWeightPair._isCalculated)
						{
							continue;
							
						}
						_tmpPos += _cal_vertRequestModWeightPair._modMesh._vertices[vertexIndex]._deltaPos * _cal_vertRequestModWeightPair._weight;
						
					}
				}
				
			}

			return _tmpPos;
		}

		public Vector2 GetVertexLocalPos(int vertexIndex)
		{
			return _result_VertLocal[vertexIndex];
		}

		//public Vector2 GetVertexRigging(int vertexIndex)
		//{
		//	return _result_Rigging[vertexIndex];
		//}

		public float GetRiggingWeight()
		{
			return _result_RiggingWeight;
		}


		public apMatrix3x3 GetDeferredRiggingMatrix(int vertexIndex)
		{
			_tmpMatrix.SetZero3x2();

			for (int iParam = 0; iParam < _resultParams_Rigging.Count; iParam++)
			{
				_cal_resultParam = _resultParams_Rigging[iParam];
				for (int iVR = 0; iVR < _cal_resultParam._result_VertLocalPairs.Count; iVR++)
				{
					_cal_vertRequest = _cal_resultParam._result_VertLocalPairs[iVR];
					if (!_cal_vertRequest._isCalculated || _cal_vertRequest._totalWeight == 0.0f)
					{
						continue;
					}

					_tmpVertRigWeightTable = _cal_vertRequest._rigBoneWeightTables[vertexIndex];
					if(_tmpVertRigWeightTable._nRigTable == 0)
					{
						continue;
					}

					for (int iRig = 0; iRig < _tmpVertRigWeightTable._nRigTable; iRig++)
					{
						_tmpVertRigWeightTable._rigTable[iRig].CalculateMatrix();
						_tmpMatrix.AddMatrixWithWeight(_tmpVertRigWeightTable._rigTable[iRig]._boneMatrix, _tmpVertRigWeightTable._rigTable[iRig]._weight);
					}
				}
			}

			return _tmpMatrix;
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
				return _parentOptTransform._meshColor2X_Default;
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