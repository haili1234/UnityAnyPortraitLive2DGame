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
	/// (Root)MeshGroup -> ModiferStack -> Modifier -> ParamSet...으로 이어지는 단계중 [Modifier]의 리얼타임
	/// </summary>
	[Serializable]
	public class apOptModifierUnitBase
	{
		// Members
		//--------------------------------------------
		[NonSerialized]
		public apPortrait _portrait = null;

		//고유 ID. 모디파이어도 고유 아이디를 갖는다.
		public int _uniqueID = -1;

		//레이어
		public int _layer = -1;//낮을수록 먼저 처리된다. (오름차순으로 배열)

		//레이어 병합시 가중치 (0~1)
		public float _layerWeight = 0.0f;

		public string _name = "";

		[NonSerialized]
		public bool _isActive = true;

		[SerializeField]
		public apCalculatedResultParam.CALCULATED_VALUE_TYPE _calculatedValueType = apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos;

		[SerializeField]
		public apCalculatedResultParam.CALCULATED_SPACE _calculatedSpace = apCalculatedResultParam.CALCULATED_SPACE.Object;

		public apModifierBase.BLEND_METHOD _blendMethod = apModifierBase.BLEND_METHOD.Interpolation;

		[SerializeField]
		public List<apOptParamSetGroup> _paramSetGroupList = new List<apOptParamSetGroup>();


		[NonSerialized]
		public Dictionary<apVertex, apMatrix3x3> _vertWorldMatrix = new Dictionary<apVertex, apMatrix3x3>();

		//각 RenderUnit으로 계산 결과를 보내주는 Param들
		[NonSerialized]
		public List<apOptCalculatedResultParam> _calculatedResultParams = new List<apOptCalculatedResultParam>();


		[SerializeField]
		public bool _isColorPropertyEnabled = true;

		//Editor에는 없는 변수
		//Color 타입의 CalculateValueType + _isColorPropertyEnabled일때 True이다.
		[SerializeField]
		public bool _isColorProperty = false;

		//다형성이 안되니
		//Enum을 넣고 아예 다 로직을 여기에 넣자
		[SerializeField]
		public apModifierBase.MODIFIER_TYPE _modifierType = apModifierBase.MODIFIER_TYPE.Base;


		//계산용 변수
		private Vector2[] posList = null;
		private Vector2[] tmpPosList = null;

		//private apMatrix3x3[] vertMatrixList = null;
		//private apMatrix3x3[] tmpVertMatrixList = null;


		private apMatrix tmpMatrix = new apMatrix();
		private List<apOptCalculatedResultParamSubList> subParamGroupList = null;
		private List<apOptCalculatedResultParam.OptParamKeyValueSet> subParamKeyValueList = null;
		private float layerWeight = 0.0f;
		private apOptParamSetGroup keyParamSetGroup = null;

		private apOptCalculatedResultParamSubList curSubList = null;
		private apOptCalculatedResultParam.OptParamKeyValueSet paramKeyValue = null;

		private List<apOptVertexRequest> vertRequestList = null;
		private apOptVertexRequest vertRequest = null;
		private apOptVertexRequest tmpVertRequest = null;


		private apOptParamSetGroupVertWeight weightedVertData = null;
		//private apOptModifiedVertexRig tmpVertRig = null;
		private Color tmpColor = Color.clear;
		private bool tmpVisible = false;
		private int nColorCalculated = 0;
		private float tmpTotalParamSetWeight = 0.0f;

		private int iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
		private bool tmpIsColoredKeyParamSetGroup = false;
		private Color tmpParamColor = Color.black;


		//private float tmpTotalBoneWeight = 0.0f;
		//private apOptModifiedVertexRig.OptWeightPair tmpWeightPair = null;
		//private apMatrix tmpMatx_boneWorld_Mod = null;
		//private Vector2 tmpVertPos_BoneLocal;
		//private Vector2 tmpVertPosW_BoneWorld;
		//private Vector2 tmpVertPosL_Result;
		//private apMatrix3x3 tmpMatx_Vert2Local = apMatrix3x3.identity;
		//private apMatrix3x3 tmpMatx_Vert2LocalInv = apMatrix3x3.identity;
		//private apMatrix tmpMatx_MeshW_NoMod = null;
		//private Vector2 tmpVertLocal = Vector2.zero;
		//private Vector2 tmpVertPosW_NoMod = Vector2.zero;
		//private apMatrix tmpMatx_boneWorld_Default = null;

		private apOptModifiedVertexWeight tmpModVertWeight = null;
		private apOptPhysicsVertParam tmpPhysicVertParam = null;
		private apOptPhysicsMeshParam tmpPhysicMeshParam = null;
		private int tmpNumVert = 0;
		private float tmpMass = 0.0f;

		private Vector2 tmpF_gravity = Vector2.zero;
		private Vector2 tmpF_wind = Vector2.zero;
		private Vector2 tmpF_stretch = Vector2.zero;
		//private Vector2 tmpF_airDrag = Vector2.zero;
		//private Vector2 tmpF_inertia = Vector2.zero;
		private Vector2 tmpF_recover = Vector2.zero;
		private Vector2 tmpF_ext = Vector2.zero;
		private Vector2 tmpF_sum = Vector2.zero;

		private apOptPhysicsVertParam.OptLinkedVertex tmpLinkedVert = null;
		private bool tmpIsViscosity = false;

		private Vector2 tmpNextVelocity = Vector2.zero;
		private float tmpLinkedViscosityWeight = 0.0f;
		//private Vector2 tmpLinkedViscosityNextVelocity = Vector2.zero;

		private Vector2 tmpSrcVertPos_NoMod = Vector2.zero;
		private Vector2 tmpLinkVertPos_NoMod = Vector2.zero;
		private Vector2 tmpSrcVertPos_Cur = Vector2.zero;
		private Vector2 tmpLinkVertPos_Cur = Vector2.zero;
		private Vector2 tmpDeltaVec_0 = Vector2.zero;
		private Vector2 tmpDeltaVec_Cur = Vector2.zero;
		private Vector2 tmpNextCalPos = Vector2.zero;
		private Vector2 tmpLinkedTotalCalPos = Vector2.zero;

		//터치에 의한 외력을 계산하기 위한 코드 변수
		[NonSerialized]
		private int _tmpTouchProcessCode = 0;



		private static Color _defaultColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		//Modifier의 설정들을 저장하자
		[SerializeField]
		private bool _isTarget_Bone = false;

		[SerializeField]
		private bool _isTarget_MeshTransform = false;

		[SerializeField]
		private bool _isTarget_MeshGroupTransform = false;

		[SerializeField]
		private bool _isTarget_ChildMeshTransform = false;

		[SerializeField]
		private bool _isAnimated = false;

		[SerializeField]
		private bool _isPreUpdate = false;

		public bool IsTarget_MeshTransform { get { return _isTarget_MeshTransform; } }
		public bool IsTarget_MeshGroupTransform { get { return _isTarget_MeshGroupTransform; } }
		public bool IsTarget_Bone { get { return _isTarget_Bone; } }
		public bool IsTarget_ChildMeshTransform { get { return _isTarget_ChildMeshTransform; } }
		public bool IsAnimated { get { return _isAnimated; } }
		public bool IsPreUpdate { get { return _isPreUpdate; } }

		//ParamSetWeight를 사용하는가
		[SerializeField]
		private bool _isUseParamSetWeight = false;

		[NonSerialized]
		protected float _tDeltaFixed = 0.0f;

		private const float PHYSIC_DELTA_TIME = 0.033f;//20FPS (0.05), 30FPS (0.033), 15FPS (0.067), 40FPS (0.025)

		private System.Diagnostics.Stopwatch _stopWatch = null;

		#region [미사용 코드 : 빠른 Transform / Bone 접근을 위해 만들었지만 더 빠른 방법을 생각해내었다]
		////Rigging을 위한 값
		////ModMesh와 Bone을 조회하면 WorldMatrix를 얻을 수 있게 만든다.
		////Bake때 만들면 된다! 올ㅋ
		////리스트 형태로 저장하고, Link할때 Dictionary 형태로 만들어두자
		//[Serializable]
		//private class TransformBoneMatrixPair
		//{
		//	[SerializeField]
		//	public apOptTransform _optTransform = null;

		//	[SerializeField]
		//	public apOptBone _bone = null;

		//	[NonSerialized]
		//	public apMatrix3x3 _boneMatrix = apMatrix3x3.identity;

		//	public TransformBoneMatrixPair(apOptTransform optTransform, apOptBone bone)
		//	{
		//		_optTransform = optTransform;
		//		_bone = bone;
		//	}

		//	public void UpdateBoneWorldMatrix()
		//	{
		//		_boneMatrix = _optTransform._vertMeshWorldNoModInverseMatrix
		//			* _bone._vertWorld2BoneModWorldMatrix
		//			* _optTransform._vertMeshWorldNoModMatrix;
		//	}
		//}

		//[SerializeField]
		//private List<TransformBoneMatrixPair> _transformBoneMatrixPair_List = new List<TransformBoneMatrixPair>();

		//[NonSerialized]
		//private Dictionary<apOptTransform, Dictionary<apOptBone, TransformBoneMatrixPair>> _transformBoneMatrixPair_Dict = new Dictionary<apOptTransform, Dictionary<apOptBone, TransformBoneMatrixPair>>(); 
		#endregion


		// Init
		//--------------------------------------------
		public apOptModifierUnitBase()
		{
			Init();
		}

		public virtual void Init()
		{

		}

		public void Link(apPortrait portrait)
		{
			_portrait = portrait;

			for (int i = 0; i < _paramSetGroupList.Count; i++)
			{
				_paramSetGroupList[i].LinkPortrait(portrait, this);
			}
		}

		public void Bake(apModifierBase srcModifier, apPortrait portrait)
		{
			_uniqueID = srcModifier._uniqueID;
			_layer = srcModifier._layer;
			_layerWeight = srcModifier._layerWeight;
			_isActive = srcModifier._isActive;

			_blendMethod = srcModifier._blendMethod;
			_calculatedValueType = srcModifier.CalculatedValueType;
			_calculatedSpace = srcModifier.CalculatedSpace;
			_modifierType = srcModifier.ModifierType;

			_name = srcModifier.DisplayName;

			_isColorPropertyEnabled = srcModifier._isColorPropertyEnabled;

			//이 부분이 추가되었다. 실제로 Color 연산을 하는지는 이 변수를 활용하자
			_isColorProperty = (int)(_calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0
								&& _isColorPropertyEnabled;

			//Modifier의 설정을 저장하자
			_isTarget_Bone = srcModifier.IsTarget_Bone;
			_isTarget_MeshTransform = srcModifier.IsTarget_MeshTransform;
			_isTarget_MeshGroupTransform = srcModifier.IsTarget_MeshGroupTransform;
			_isTarget_ChildMeshTransform = srcModifier.IsTarget_ChildMeshTransform;
			_isAnimated = srcModifier.IsAnimated;


			_isPreUpdate = srcModifier.IsPreUpdate;
			//Debug.LogError(">>> Bake - [" + srcModifier.ModifierType + "] Is Pre Update : " + _isPreUpdate);

			//ParamSetWeight를 사용하는가
			_isUseParamSetWeight = srcModifier.IsUseParamSetWeight;

			_paramSetGroupList.Clear();
			for (int i = 0; i < srcModifier._paramSetGroup_controller.Count; i++)
			{
				apModifierParamSetGroup srcParamSetGroup = srcModifier._paramSetGroup_controller[i];

				apOptParamSetGroup optParamSetGroup = new apOptParamSetGroup();
				optParamSetGroup.Bake(portrait, this, srcParamSetGroup, _isAnimated);

				_paramSetGroupList.Add(optParamSetGroup);
				
			}

			
		}

		// Functions
		//--------------------------------------------
		public void InitCalcualte(float tDelta)
		{
			//계산이 불가능한 상황일 때, 계산 값만 초기화한다.
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			apOptCalculatedResultParam calParam = null;
			for (int i = 0; i < _calculatedResultParams.Count; i++)
			{
				calParam = _calculatedResultParams[i];

				calParam.InitCalculate();
				calParam._isAvailable = false;
			}

			//_tDeltaFixed = 0.0f;

		}

		public void Calculate(float tDelta)
		{
			switch (_modifierType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:
					break;

				case apModifierBase.MODIFIER_TYPE.Volume:
					break;

				case apModifierBase.MODIFIER_TYPE.Morph:
//#if UNITY_EDITOR
//					Profiler.BeginSample("Opt Mod Calculate - Morph");
//#endif
					Calculate_Morph(tDelta);
//#if UNITY_EDITOR
//					Profiler.EndSample();
//#endif
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
//#if UNITY_EDITOR
//			Profiler.BeginSample("Opt Mod Calculate - Animated Morph");
//#endif
					Calculate_Morph(tDelta);
//#if UNITY_EDITOR
//					Profiler.EndSample();
//#endif
					break;

				case apModifierBase.MODIFIER_TYPE.Rigging:
//#if UNITY_EDITOR
//			Profiler.BeginSample("Opt Mod Calculate - Rigging");
//#endif
					Calculate_Rigging(tDelta);
//#if UNITY_EDITOR
//					Profiler.EndSample();
//#endif
					break;

				case apModifierBase.MODIFIER_TYPE.Physic:
//#if UNITY_EDITOR
//			Profiler.BeginSample("Opt Mod Calculate - Physics");
//#endif
					Calculate_Physics(tDelta);
//#if UNITY_EDITOR
//					Profiler.EndSample();
//#endif
					break;

				case apModifierBase.MODIFIER_TYPE.TF:
//#if UNITY_EDITOR
//			Profiler.BeginSample("Opt Mod Calculate - TF");
//#endif
					Calculate_TF(tDelta);
//#if UNITY_EDITOR
//					Profiler.EndSample();
//#endif
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
//#if UNITY_EDITOR
//			Profiler.BeginSample("Opt Mod Calculate - Animated TF");
//#endif
					Calculate_TF(tDelta);
//#if UNITY_EDITOR
//					Profiler.EndSample();
//#endif
					break;

				case apModifierBase.MODIFIER_TYPE.FFD:
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					break;
			}
		}


		//--------------------------------------------------------------------------
		// Sub 로직들
		//--------------------------------------------------------------------------


		//--------------------------------------------------------------------------
		// Morph
		//--------------------------------------------------------------------------
		private void Calculate_Morph(float tDelta)
		{

//#if UNITY_EDITOR
//			Profiler.BeginSample("Modifier - Calculate Morph");
//#endif

			//bool isFirstDebug = true;
			apOptCalculatedResultParam calParam = null;
			bool isUpdatable = false;
			for (int i = 0; i < _calculatedResultParams.Count; i++)
			{
				calParam = _calculatedResultParams[i];



				//1. 계산 [중요]
				isUpdatable = calParam.Calculate();
				if (!isUpdatable)
				{
					calParam._isAvailable = false;
					continue;
				}
				else
				{
					calParam._isAvailable = true;
				}

				//추가 : 색상 처리 초기화
				calParam._isColorCalculated = false;


				//계산 결과를 Vertex에 넣어줘야 한다.
				//구버전
				//posList = calParam._result_Positions;
				//tmpPosList = calParam._tmp_Positions;

				//신버전
				vertRequestList = calParam._result_VertLocalPairs;


				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				layerWeight = 0.0f;
				keyParamSetGroup = null;
				weightedVertData = calParam._weightedVertexData;

				//일단 초기화
				//구버전
				//for (int iPos = 0; iPos < posList.Length; iPos++)
				//{
				//	posList[iPos] = Vector2.zero;
				//}

				//신버전
				for (int iVR = 0; iVR < vertRequestList.Count; iVR++)
				{
					vertRequestList[iVR].InitCalculate();
				}


				if (_isColorProperty)
				{
					calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					calParam._result_IsVisible = false;//Alpha와 달리 Visible 값은 false -> OR 연산으로 작동한다.
				}
				else
				{
					calParam._result_IsVisible = true;
				}

				int iCalculatedSubParam = 0;

				iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				tmpIsColoredKeyParamSetGroup = false;

				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;


					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;
					

					//레이어 내부의 임시 데이터를 먼저 초기화
					//구버전
					//for (int iPos = 0; iPos < posList.Length; iPos++)
					//{
					//	tmpPosList[iPos] = Vector2.zero;
					//}

					//신버전
					//Vertex Pos 대신 Vertex Requst를 보간하자
					vertRequest = curSubList._vertexRequest;
					vertRequest.SetCalculated();//<<일단 계산하기 위해 참조 했음을 알린다.


					tmpColor = Color.clear;
					tmpVisible = false;

					tmpTotalParamSetWeight = 0.0f;
					nColorCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					tmpIsColoredKeyParamSetGroup = _isColorProperty && keyParamSetGroup._isColorPropertyEnabled;

//#if UNITY_EDITOR
//					Profiler.BeginSample("Modifier - Calculate Morph > Add Pos List");
//#endif

					//-------------------------------------------
					// 여기가 과부하가 가장 심한 곳이다! 우오오오
					//-------------------------------------------

					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.
					for (int iPV = 0; iPV < nParamKeys; iPV++)
					{
						paramKeyValue = subParamKeyValueList[iPV];

						if (!paramKeyValue._isCalculated)
						{ continue; }

						tmpTotalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;

//#if UNITY_EDITOR
//						Profiler.BeginSample("Modifier - Calculate Morph > 2. Pos List Loop");
//#endif

						//최적화해야할 부분 1)
						//구버전)
						//Pos를 일일이 돌게 아니라 VertexRequst의 Weight만 지정하자
						////---------------------------- Pos List
						//for (int iPos = 0; iPos < posList.Length; iPos++)
						//{
						//	//calculatedValue = paramKeyValue._modifiedValue._vertices[iPos]._deltaPos * paramKeyValue._weight;
						//	tmpPosList[iPos] += paramKeyValue._modifiedMesh._vertices[iPos]._deltaPos * paramKeyValue._weight;
						//}
						////---------------------------- Pos List

						//>> 최적화 코드)
						vertRequest._modWeightPairs[iPV].SetWeight(paramKeyValue._weight);



						//---------------------------- Color
						if (tmpIsColoredKeyParamSetGroup)
						{
							if (paramKeyValue._modifiedMesh._isVisible)
							{
								tmpColor += paramKeyValue._modifiedMesh._meshColor * paramKeyValue._weight;
								tmpVisible = true;//하나라도 Visible이면 Visible이 된다.
							}
							else
							{
								tmpParamColor = paramKeyValue._modifiedMesh._meshColor;
								tmpParamColor.a = 0.0f;
								tmpColor += tmpParamColor * paramKeyValue._weight;
							}
							//paramKeyValue._modifiedValue._isMeshTransform
						}

						nColorCalculated++;
						//---------------------------- Color

//#if UNITY_EDITOR
//						Profiler.EndSample();
//#endif
					}//--- Params

//#if UNITY_EDITOR
//					Profiler.EndSample();
//#endif

					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.
					if (!_isUseParamSetWeight)
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);
					}
					else
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight * Mathf.Clamp01(tmpTotalParamSetWeight));
					}


					if (layerWeight < 0.001f)
					{
						continue;
					}

					calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.

					if (nColorCalculated == 0)
					{
						tmpVisible = true;
						tmpColor = _defaultColor;
					}

					//if (keyParamSetGroup._layerIndex == 0)
					if (iCalculatedSubParam == 0)
					{
						//구버전 : Vertex Pos를 직접 수정
						//for (int iPos = 0; iPos < posList.Length; iPos++)
						//{
						//	posList[iPos] = tmpPosList[iPos] * layerWeight;
						//}

						//신버전 : VertexRequest에 넣자
						vertRequest.MultiplyWeight(layerWeight);
					}
					else
					{

//#if UNITY_EDITOR
//						Profiler.BeginSample("Modifier - Calculate Morph > Overlap Pos List");
//#endif

						switch (keyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									//구버전
									//if (weightedVertData != null)
									//{
									//	//Vertex 가중치가 추가되었다.
									//	float vertWeight = 0.0f;
									//	for (int iPos = 0; iPos < posList.Length; iPos++)
									//	{
									//		vertWeight = layerWeight * weightedVertData._vertWeightList[iPos];

									//		posList[iPos] += tmpPosList[iPos] * vertWeight;
									//	}
									//}
									//else
									//{
									//	for (int iPos = 0; iPos < posList.Length; iPos++)
									//	{
									//		posList[iPos] += tmpPosList[iPos] * layerWeight;
									//	}
									//}

									//신버전 : VertexRequest에 넣자
									//Additive : Prev + Next * weight이므로
									//Next에만 weight를 곱한다.
									vertRequest.MultiplyWeight(layerWeight);

								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									//if (weightedVertData != null)
									//{
									//	//Vertex 가중치가 추가되었다.
									//	float vertWeight = 0.0f;
									//	for (int iPos = 0; iPos < posList.Length; iPos++)
									//	{
									//		vertWeight = layerWeight * weightedVertData._vertWeightList[iPos];

									//		posList[iPos] = (posList[iPos] * (1.0f - vertWeight)) +
									//						(tmpPosList[iPos] * vertWeight);
									//	}
									//}
									//else
									//{
									//	for (int iPos = 0; iPos < posList.Length; iPos++)
									//	{
									//		posList[iPos] = (posList[iPos] * (1.0f - layerWeight)) +
									//						(tmpPosList[iPos] * layerWeight);
									//	}
									//}

									//신버전 : VertexRequest에 넣자
									//Interpolation : Prev * (1-weight) + Next * weight이므로
									//Prev에 1-weight
									//Next에 weight
									//단, 계산 안한건 제외한다.
									float invWeight = 1.0f - layerWeight;
									for (int iVR = 0; iVR < vertRequestList.Count; iVR++)
									{
										tmpVertRequest = vertRequestList[iVR];
										if(!tmpVertRequest._isCalculated)
										{
											//아직 계산 안한건 패스
											continue;
										}
										if(tmpVertRequest == vertRequest)
										{
											//Next엔 * weight
											tmpVertRequest.MultiplyWeight(layerWeight);
										}
										else
										{
											//Prev엔 * (1-weight)
											tmpVertRequest.MultiplyWeight(invWeight);
										}
									}
								}
								break;

							default:
								Debug.LogError("Mod-Morph : Unknown BLEND_METHOD : " + keyParamSetGroup._blendMethod);
								break;
						}

//#if UNITY_EDITOR
//						Profiler.EndSample();
//#endif

					}


					if (tmpIsColoredKeyParamSetGroup)
					{
						if (iColoredKeyParamSetGroup == 0 || keyParamSetGroup._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
						{
							//색상 Interpolation
							calParam._result_Color = apUtil.BlendColor_ITP(calParam._result_Color, tmpColor, layerWeight);
							calParam._result_IsVisible |= tmpVisible;
						}
						else
						{
							//색상 Additive
							calParam._result_Color = apUtil.BlendColor_Add(calParam._result_Color, tmpColor, layerWeight);
							calParam._result_IsVisible |= tmpVisible;
						}
						iColoredKeyParamSetGroup++;
						calParam._isColorCalculated = true;
					}

					iCalculatedSubParam++;

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)




				if (iCalculatedSubParam == 0)
				{
					calParam._isAvailable = false;
				}
				else
				{
					calParam._isAvailable = true;
					calParam._result_Matrix.MakeMatrix();
				}

			}

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif
		}





		//--------------------------------------------------------------------------
		// TF (Transform)
		//--------------------------------------------------------------------------
		private void Calculate_TF(float tDelta)
		{
			apOptCalculatedResultParam calParam = null;
			bool isUpdatable = false;

			//추가 : Bone을 대상으로 하는가
			//Bone대상이면 ModBone을 사용해야한다.
			bool isBoneTarget = false;

			for (int i = 0; i < _calculatedResultParams.Count; i++)
			{
				calParam = _calculatedResultParams[i];

				if (calParam._targetBone != null)
				{
					//ModBone을 참조하는 Param이다.
					isBoneTarget = true;
				}
				else
				{
					//ModMesh를 참조하는 Param이다.
					isBoneTarget = false;
				}
				//1. 계산 [중요]
				isUpdatable = calParam.Calculate();

				if (!isUpdatable)
				{
					calParam._isAvailable = false;
					continue;
				}
				else
				{
					calParam._isAvailable = true;
				}

				//초기화
				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				keyParamSetGroup = null;

				calParam._result_Matrix.SetIdentity();

				calParam._isColorCalculated = false;

				if (!isBoneTarget)
				{
					if (_isColorProperty)
					{
						calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
						calParam._result_IsVisible = false;
					}
					else
					{
						calParam._result_IsVisible = true;
					}
				}
				else
				{
					calParam._result_IsVisible = true;
					calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}


				tmpMatrix.SetIdentity();
				layerWeight = 0.0f;

				tmpVisible = false;

				int iCalculatedSubParam = 0;

				iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				tmpIsColoredKeyParamSetGroup = false;



				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;

					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;

					//레이어 내부의 임시 데이터를 먼저 초기화
					tmpMatrix.SetZero();//<<TF에서 추가됨
					tmpColor = Color.clear;

					tmpVisible = false;

					tmpTotalParamSetWeight = 0.0f;
					nColorCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					tmpIsColoredKeyParamSetGroup = _isColorProperty && keyParamSetGroup._isColorPropertyEnabled && !isBoneTarget;

					if (!isBoneTarget)
					{
						//ModMesh를 활용하는 타입인 경우
						for (int iPV = 0; iPV < nParamKeys; iPV++)
						{
							paramKeyValue = subParamKeyValueList[iPV];

							if (!paramKeyValue._isCalculated)
							{ continue; }

							//ParamSetWeight를 추가
							tmpTotalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;

							//Weight에 맞게 Matrix를 만들자
							if(paramKeyValue._isAnimRotationBias)
							{
								//추가 : RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
								tmpMatrix.AddMatrix(paramKeyValue.AnimRotationBiasedMatrix, paramKeyValue._weight, false);
							}
							else
							{
								//기본 식
								tmpMatrix.AddMatrix(paramKeyValue._modifiedMesh._transformMatrix, paramKeyValue._weight, false);
							}
							

							if (tmpIsColoredKeyParamSetGroup)
							{

								if (paramKeyValue._modifiedMesh._isVisible)
								{
									tmpColor += paramKeyValue._modifiedMesh._meshColor * paramKeyValue._weight;
									tmpVisible = true;
								}
								else
								{
									//Visible False
									tmpParamColor = paramKeyValue._modifiedMesh._meshColor;
									tmpParamColor.a = 0.0f;
									tmpColor += tmpParamColor * paramKeyValue._weight;
								}
							}

							nColorCalculated++;
						}
					}
					else
					{
						//float totalParamWeight = 0.0f;
						//int nAddedWeight = 0;
						//int nSkipWeight = 0;

						//ModBone을 활용하는 타입인 경우
						for (int iPV = 0; iPV < nParamKeys; iPV++)
						{
							//paramKeyValue = calParam._paramKeyValues[iPV];
							paramKeyValue = subParamKeyValueList[iPV];
							//layerWeight = Mathf.Clamp01(paramKeyValue._keyParamSetGroup._layerWeight);
							//Debug.Log("Param Key : " + paramKeyValue._weight + " / Cal : " + paramKeyValue._isCalculated);
							if (!paramKeyValue._isCalculated)
							{
								//nSkipWeight++;
								continue;
							}

							//ParamSetWeight를 추가
							tmpTotalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;
							//nAddedWeight++;

							//Weight에 맞게 Matrix를 만들자
							
							if(paramKeyValue._isAnimRotationBias)
							{
								//추가 : RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
								tmpMatrix.AddMatrix(paramKeyValue.AnimRotationBiasedMatrix, paramKeyValue._weight, false);
							}
							else
							{
								tmpMatrix.AddMatrix(paramKeyValue._modifiedBone._transformMatrix, paramKeyValue._weight, false);
							}

							//if(paramKeyValue._modifiedBone._transformMatrix._scale.magnitude < 0.3f)
							//{
							//	Debug.LogError("Modifier Cal Param [너무 작은 Bone Transform - " + paramKeyValue._modifiedBone._bone.name + "]");
							//}

							nColorCalculated++;//Visible 계산을 위해 "ParamKey 계산 횟수"를 카운트하자

							//totalParamWeight += paramKeyValue._weight;
						}

						//if (totalParamWeight < 0.3f)
						//{
						//	Debug.LogError("Modifier Cal Param [Weight 합이 너무 작다 : " + totalParamWeight + " - 계산된 Param 개수 : " + nAddedWeight + " / 계산안된 Param 개수 : " + nSkipWeight + " (Total : " + nParamKeys +")]");
						//	Debug.LogError("Sub List [" + iSubList + " / " + subParamGroupList.Count + "]");
						//}
						//else
						//{
						//	Debug.Log(">>>Modifier Cal Param [Weight 합이 적절 : " + totalParamWeight + " - 계산된 Param 개수 : " + nAddedWeight + " / 계산안된 Param 개수 : " + nSkipWeight + " (Total : " + nParamKeys +")]");
						//}
					}

					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.

					if (!_isUseParamSetWeight)
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);
					}
					else
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight * Mathf.Clamp01(tmpTotalParamSetWeight));
					}


					//if (tmpTotalParamSetWeight < 0.3f && layerWeight > 0.001f && IsAnimated)
					//{
					//	Debug.LogError("Param Weight가 작다 : " + tmpTotalParamSetWeight + " / Layer Weight : " + layerWeight + " [ Skip : " + (layerWeight < 0.001f) + " ]");
					//	if(keyParamSetGroup._keyAnimClip != null)
					//	{
					//		Debug.LogError("연결된 AnimClip : " + keyParamSetGroup._keyAnimClip._name + " [Anim 계산된 LayerWeight : " + keyParamSetGroup._isAnimUpdatedLayerWeight + " / AnimEnabled " + keyParamSetGroup.IsAnimEnabled + " ]");
					//		Debug.Log("PlayUnit : " + (keyParamSetGroup._keyAnimClip._parentPlayUnit != null));
					//		Debug.Log("Anim Clip ID : " + keyParamSetGroup._keyAnimClip._uniqueID);
					//	}
					//	else
					//	{
					//		Debug.LogError("연결된 AnimClip이 없다? : " + keyParamSetGroup._keyAnimClipID + " [Anim 계산된 LayerWeight : " + keyParamSetGroup._isAnimUpdatedLayerWeight + " / AnimEnabled " + keyParamSetGroup.IsAnimEnabled + " ]");
					//	}
					//}


					if (layerWeight < 0.001f)
					{
						continue;
					}

					if ((nColorCalculated == 0 && _isColorProperty) || isBoneTarget)
					{
						tmpVisible = true;
						tmpColor = _defaultColor;
						if (!isBoneTarget)
						{
							tmpMatrix.SetIdentity();
							tmpColor = _defaultColor;
						}
					}

					calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.

					//디버그용 코드
					//if(isBoneTarget 
					//		&& calParam._isAnimModifier 
					//		//&& layerWeight < 0.3f
					//		&& calParam._targetBone.name.Contains("Pelvis"))
					//{
						
					//	Debug.Log("[" + iCalculatedSubParam + "] Layer Weight가 낮은 Anim Bone | Weight : " + layerWeight + " / " + tmpMatrix.ToString());
					//}

					//if(tmpMatrix._scale.magnitude < 0.3f)
					//{
					//	Debug.LogError("Modifier 계산 중 tmpMatrix가 너무 작다 [Mod Bone? : " + isBoneTarget +"]");
					//}

					//if (keyParamSetGroup._layerIndex == 0)
					if (iCalculatedSubParam == 0)
					{
						//calParam._result_Matrix.SetMatrix(tmpMatrix);

						//이 코드는 Make Matrix를 계속 호출한다. >> 삭제 예정
						//calParam._result_Matrix.SetPos(tmpMatrix._pos * layerWeight);
						//calParam._result_Matrix.SetRotate(tmpMatrix._angleDeg * layerWeight);
						//calParam._result_Matrix.SetScale(tmpMatrix._scale * layerWeight + Vector2.one * (1.0f - layerWeight));
						//calParam._result_Matrix.MakeMatrix();

						//위 코드를 하나로 합쳤다.
						calParam._result_Matrix.SetTRS(	tmpMatrix._pos * layerWeight,
														tmpMatrix._angleDeg * layerWeight,
														tmpMatrix._scale * layerWeight + Vector2.one * (1.0f - layerWeight));

						
						

					}
					else
					{
						switch (keyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									calParam._result_Matrix.AddMatrix(tmpMatrix, layerWeight, true);

								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									calParam._result_Matrix.LerpMartix(tmpMatrix, layerWeight);

								}
								break;
						}

					}


					//변경 : 색상은 별도로 카운팅해서 처리하자
					if (tmpIsColoredKeyParamSetGroup)
					{
						if (iColoredKeyParamSetGroup == 0 || keyParamSetGroup._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
						{
							//색상 Interpolation
							calParam._result_Color = apUtil.BlendColor_ITP(calParam._result_Color, tmpColor, layerWeight);
							calParam._result_IsVisible |= tmpVisible;
						}
						else
						{
							//색상 Additive
							calParam._result_Color = apUtil.BlendColor_Add(calParam._result_Color, tmpColor, layerWeight);
							calParam._result_IsVisible |= tmpVisible;
						}
						iColoredKeyParamSetGroup++;
						calParam._isColorCalculated = true;
					}

					iCalculatedSubParam++;
				}

				if (iCalculatedSubParam == 0)
				{
					calParam._isAvailable = false;
				}
				else
				{
					calParam._isAvailable = true;
					calParam._result_Matrix.MakeMatrix();//<?
				}
			}
		}



		//----------------------------------------------------------------------
		// Rigging
		//----------------------------------------------------------------------
		private void Calculate_Rigging(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				//Debug.LogError("Result Param Count : 0");
				return;
			}


			apOptCalculatedResultParam calParam = null;

			//최적화 코드 추가
			//처리 전에 Transform + Bone 리스트를 돌면서 아예 WorldMatrix를 만들어준다.
			//for (int i = 0; i < _transformBoneMatrixPair_List.Count; i++)
			//{
			//	_transformBoneMatrixPair_List[i].UpdateBoneWorldMatrix();
			//}

			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				calParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크


//#if UNITY_EDITOR
//				Profiler.BeginSample("Rigging - 1. Param Calculate");
//#endif
				// 중요! -> Static은 Weight 계산이 필요없어염
				//-------------------------------------------------------
				//1. Param Weight Calculate
				calParam.Calculate();
				//-------------------------------------------------------
//#if UNITY_EDITOR
//				Profiler.EndSample();
//#endif
				//수정 : Rigging의 VertexPos 대신 Matrix를 설정해주자
				//posList = calParam._result_Positions;
				//tmpPosList = calParam._tmp_Positions;

				//추가됨.
				//구버전
				//vertMatrixList = calParam._result_VertMatrices;
				//tmpVertMatrixList = calParam._tmp_VertMatrices;

				//신버전
				vertRequestList = calParam._result_VertLocalPairs;


				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				layerWeight = 0.0f;
				//keyParamSetGroup = null;

				//weigetedVertData = calParam._weightedVertexData;

				//일단 초기화 -> Vertex Pos는 뺀다.
				//for (int iPos = 0; iPos < posList.Length; iPos++)
				//{
				//	posList[iPos] = Vector2.zero;
				//}

				//신버전
				for (int iVR = 0; iVR < vertRequestList.Count; iVR++)
				{
					vertRequestList[iVR].InitCalculate();
				}



				calParam._result_IsVisible = true;

				//apMatrix3x3 tmpBoneMatrix;
				//Color tmpColor = Color.clear;
				//bool tmpVisible = false;


				int iCalculatedSubParam = 0;



				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
					int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;


					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;//<<
																	//keyParamSetGroup._isEnabled = true;

					//레이어 내부의 임시 데이터를 먼저 초기화
					//변경 : Rigging Vertex는 사용하지 않습니다.
					//for (int iPos = 0; iPos < posList.Count; iPos++)
					//{
					//	tmpPosList[iPos] = Vector2.zero;
					//}

					//이것도 생략
					//for (int iPos = 0; iPos < vertMatrixList.Length; iPos++)
					//{
					//	tmpVertMatrixList[iPos].SetZero3x2();
					//}

					//신버전
					//Vertex Pos 대신 Vertex Requst를 보간하자
					vertRequest = curSubList._vertexRequest;
					vertRequest.SetCalculated();//<<일단 계산하기 위해 참조 했음을 알린다.


					tmpColor = Color.clear;
					tmpVisible = false;

					float totalWeight = 0.0f;
					int nCalculated = 0;
					//tmpVertRig = null;

					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.

					//Dictionary<apOptBone, TransformBoneMatrixPair> boneMatrixPair = null;

					for (int iPV = 0; iPV < nParamKeys; iPV++)
					{
						paramKeyValue = subParamKeyValueList[iPV];

						//if (!paramKeyValue._isCalculated) { continue; }

						paramKeyValue._weight = 1.0f;

						totalWeight += paramKeyValue._weight;

						//Modified가 안된 Vert World Pos + Bone의 Modified 안된 World Matrix + Bone의 World Matrix (변형됨) 순으로 계산한다.
						//Editor
						//apMatrix3x3 matx_Vert2Local = paramKeyValue._modifiedMesh._renderUnit._meshTransform._mesh.Matrix_VertToLocal;
						//apMatrix matx_MeshW_NoMod = paramKeyValue._modifiedMesh._renderUnit._meshTransform._matrix_TFResult_WorldWithoutMod;

						//Opt
						//사용안함 -> 단축 식을 직접 이용함. 
						//tmpMatx_Vert2Local = paramKeyValue._modifiedMesh._targetMesh._matrix_Vert2Mesh;
						//tmpMatx_Vert2LocalInv = paramKeyValue._modifiedMesh._targetMesh._matrix_Vert2Mesh_Inverse;

						//tmpMatx_MeshW_NoMod = paramKeyValue._modifiedMesh._targetTransform._matrix_TFResult_WorldWithoutMod;

						//이게 중간 버전 최적화
						//boneMatrixPair = _transformBoneMatrixPair_Dict[paramKeyValue._modifiedMesh._targetTransform];

						

//#if UNITY_EDITOR
//						Profiler.BeginSample("Rigging - 2. Pos Calculate");
//#endif
						

						//>> 최적화 코드)
						//vertRequest._[iPV].SetWeight(paramKeyValue._weight);


						nCalculated++;//Visible 계산을 위해 "paramKey 계산 횟수"를 카운트하자

//#if UNITY_EDITOR
//						Profiler.EndSample();
//#endif

					}//--- Params


					//이제 tmp값을 Result에 넘겨주자
					//처음 Layer라면 -> 100% 적용
					//그렇지 않다면 Blend를 해주자

					layerWeight = 1.0f;

					calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.

					if (nCalculated == 0)
					{
						tmpVisible = true;
					}


					//이전 코드
					//for (int iPos = 0; iPos < posList.Count; iPos++)
					//{
					//	posList[iPos] = tmpPosList[iPos] * layerWeight;
					//}

					//중간 버전 최적화
					//for(int iPos = 0; iPos < vertMatrixList.Length; iPos++)
					//{
					//	vertMatrixList[iPos].SetMatrixWithWeight(tmpVertMatrixList[iPos], layerWeight);
					//}

					//신버전 코드
					//VertexRequest에 넣자
					//vertRequest.MultiplyWeight(layerWeight);//??어차피 LayerWeight가 1인데..

					iCalculatedSubParam++;

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)
				calParam._isAvailable = true;


			}

			//Rigging을 그 이후의 보간이 없다.
		}


		//초당 얼마나 업데이트 요청을 받는지 체크
		private int _nUpdateCall = 0;
		private float _tUpdateCall = 0.0f;
		private int _nUpdateValid = 0;

		private void Calculate_Physics(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			if (_stopWatch == null)
			{
				_stopWatch = new System.Diagnostics.Stopwatch();
				_stopWatch.Start();
			}



			tDelta = (float)(_stopWatch.ElapsedMilliseconds) / 1000.0f;


			//tDelta *= 0.5f;
			bool isValidFrame = false;
			_tDeltaFixed += tDelta;
			_tUpdateCall += tDelta;
			_nUpdateCall++;



			if (_tDeltaFixed > PHYSIC_DELTA_TIME)
			{
				//Debug.Log("Delta Time : " + tDelta + " >> " + PHYSIC_DELTA_TIME);
				tDelta = PHYSIC_DELTA_TIME;
				_tDeltaFixed -= PHYSIC_DELTA_TIME;
				isValidFrame = true;
			}
			else
			{
				tDelta = 0.0f;
				isValidFrame = false;
			}

			if (isValidFrame)
			{
				_nUpdateValid++;
			}


			if (_tUpdateCall > 1.0f)
			{
				//Debug.Log("초당 Update Call 횟수 : " + _nUpdateCall + " / Valid : " + _nUpdateValid + " (" + _tUpdateCall + ")");
				_tUpdateCall = 0.0f;
				_nUpdateCall = 0;
				_nUpdateValid = 0;
			}

			_stopWatch.Stop();
			_stopWatch.Reset();
			_stopWatch.Start();


			//tDelta *= 0.5f;

			apOptCalculatedResultParam calParam = null;

			//지역 변수를 여기서 일괄 선언하자

			//bool isFirstDebug = true;
			//외부 힘을 업데이트해야하는지를 여기서 체크하자
			bool isExtTouchProcessing = false;
			bool isExtTouchWeightRefresh = false;
			if (_portrait.IsAnyTouchEvent)
			{
				isExtTouchProcessing = true;//터치 이벤트 중이다.
				if (_tmpTouchProcessCode != _portrait.TouchProcessCode)
				{
					//처리중인 터치 이벤트가 바뀌었다.
					//새로운 터치라면 Weight를 새로 만들어야하고, 아니면 Weight를 초기화해야함
					_tmpTouchProcessCode = _portrait.TouchProcessCode;
					isExtTouchWeightRefresh = true;

				}
			}
			else
			{
				_tmpTouchProcessCode = 0;
			}



			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				calParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크

				// 중요!
				//-------------------------------------------------------
				//1. Param Weight Calculate
				calParam.Calculate();
				//-------------------------------------------------------

				posList = calParam._result_Positions;
				tmpPosList = calParam._tmp_Positions;
				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				layerWeight = 0.0f;
				keyParamSetGroup = null;

				weightedVertData = calParam._weightedVertexData;

				//일단 초기화
				for (int iPos = 0; iPos < posList.Length; iPos++)
				{
					posList[iPos] = Vector2.zero;
				}

				calParam._result_IsVisible = true;

				int iCalculatedSubParam = 0;

				//bool isFirstDebug = true;

				//bool isDebugStretchFirst = true;

				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					if (curSubList._keyParamSetGroup == null)
					{
						//Debug.LogError("Modifier Cal Param Failed : " + DisplayName + " / " + calParam._linkedModifier.DisplayName);
						continue;
					}

					//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
					int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;



					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;


					//Vector2 calculatedValue = Vector2.zero;

					bool isFirstParam = true;

					//레이어 내부의 임시 데이터를 먼저 초기화
					for (int iPos = 0; iPos < posList.Length; iPos++)
					{
						tmpPosList[iPos] = Vector2.zero;
					}

					float totalWeight = 0.0f;
					int nCalculated = 0;



					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.

					for (int iPV = 0; iPV < nParamKeys; iPV++)
					{
						paramKeyValue = subParamKeyValueList[iPV];

						//if (!paramKeyValue._isCalculated) { continue; }

						totalWeight += paramKeyValue._weight;



						//물리 계산 순서
						//Vertex 각각의 이전프레임으로 부터의 속력 계산
						//
						if (posList.Length > 0 
							&& _portrait._isPhysicsPlay_Opt//<<Portrait에서 지원하는 경우만
							&& _portrait._isImportant//<<Important 설정이 붙은 객체만
							
#if IS_APDEMO
							&& false//데모 버전에서는 씬에서 물리 기능이 작동하지 않습니다.
#endif
							)
						{



							tmpModVertWeight = null;
							tmpPhysicVertParam = null;
							tmpPhysicMeshParam = paramKeyValue._modifiedMesh.PhysicParam;
							tmpNumVert = posList.Length;
							tmpMass = tmpPhysicMeshParam._mass;
							if (tmpMass < 0.001f)
							{
								tmpMass = 0.001f;
							}
							//Debug.Log("Mass : " + tmpMass);

							//Vertex에 상관없이 적용되는 힘
							// 중력, 바람
							//1) 중력 : mg
							tmpF_gravity = tmpMass * tmpPhysicMeshParam.GetGravityAcc();

							//2) 바람 : ma
							tmpF_wind = tmpMass * tmpPhysicMeshParam.GetWindAcc(tDelta);


							tmpF_stretch = Vector2.zero;
							//tmpF_airDrag = Vector2.zero;

							//tmpF_inertia = Vector2.zero;
							tmpF_recover = Vector2.zero;
							tmpF_ext = Vector2.zero;
							tmpF_sum = Vector2.zero;

							tmpLinkedVert = null;
							tmpIsViscosity = tmpPhysicMeshParam._viscosity > 0.0f;


							//수정
							// "잡아 당기는 코드"를 미리 만들고, Weight를 지정한다.
							//Weight에 따라서 힘의 결과가 속도로 계산되는 비율이 결정된다.
							//Touch Weight가 클수록 Velocity는 0이 된다.


							//Debug.Log("Wind : " + tmpF_wind + " / Gravity : " + tmpF_gravity);
							//---------------------------- Pos List

							//bool isFirstDebug = true;
							//int iDebugLog = -1;
							bool isTouchCalculated = false;
							float touchCalculatedWeight = 0.0f;
							Vector2 touchCalculatedDeltaPos = Vector2.zero;

							for (int iPos = 0; iPos < tmpNumVert; iPos++)
							{
								//여기서 물리 계산을 하자
								tmpModVertWeight = paramKeyValue._modifiedMesh._vertWeights[iPos];
								tmpModVertWeight.UpdatePhysicVertex(tDelta, isValidFrame);//<<RenderVert의 위치와 속도를 계산한다.

								tmpF_stretch = Vector2.zero;
								//tmpF_airDrag = Vector2.zero;

								tmpF_recover = Vector2.zero;
								tmpF_ext = Vector2.zero;
								tmpF_sum = Vector2.zero;

								if (!tmpModVertWeight._isEnabled)
								{
									//처리 안함다
									tmpModVertWeight._calculatedDeltaPos = Vector2.zero;
									continue;
								}
								if (tmpModVertWeight._vertex == null)
								{
									//Debug.LogError("Render Vertex is Not linked");
									break;
								}

								//최적화는 나중에 하고 일단 업데이트만이라도 하자

								tmpPhysicVertParam = tmpModVertWeight._physicParam;


								tmpModVertWeight._isLimitPos = false;
								tmpModVertWeight._limitScale = -1.0f;

								//터치 이벤트 초기화
								isTouchCalculated = false;
								touchCalculatedWeight = 0.0f;
								touchCalculatedDeltaPos = Vector2.zero;

								//"잡아 당김"을 구현하자
								if (isExtTouchProcessing)
								{
									Vector2 pullTouchPos = Vector2.zero;
									float pullTouchTotalWeight = 0.0f;
									//Weight를 새로 갱신하자
									for (int i = 0; i < apForceManager.MAX_TOUCH_UNIT; i++)
									{
										apPullTouch touch = _portrait.GetTouch(i);
										Vector2 touchPos = touch.Position;
										//touchPos *= -1;

										if (isExtTouchWeightRefresh)
										{
											if (touch.IsLive)
											{
												//pos 1F 위치에 의한 Weight를 새로 갱신해야한다.
												tmpModVertWeight._touchedWeight[i] = touch.GetTouchedWeight(tmpModVertWeight._pos_1F);
												tmpModVertWeight._touchedPosDelta[i] = tmpModVertWeight._pos_1F - touch.Position;

												//Debug.Log("Touch Pos Check : " + touch.Position + " / Vert : " + tmpModVertWeight._pos_1F 
												//	+ " (Local : " + tmpModVertWeight._vertex._vertPos_World + ")"
												//	+ " / Weight : " + tmpModVertWeight._touchedWeight[i]);
											}
											else
											{
												tmpModVertWeight._touchedWeight[i] = -1.0f;//Weight를 초기화
											}
										}

										if (touch.IsLive)
										{
											//Weight를 이용하여 보간을 하자
											//이후 누적 후 평균값을 넣자
											//pullTouchPos += touch.GetPulledPos(tmpModVertWeight._pos_1F, tmpModVertWeight._touchedWeight[i]);
											pullTouchPos += (tmpModVertWeight._touchedPosDelta[i] + touch.Position - tmpModVertWeight._pos_1F) * tmpModVertWeight._touchedWeight[i];
											//pullTouchPos += (tmpModVertWeight._touchedPosDelta[i]) * tmpModVertWeight._touchedWeight[i];
											pullTouchTotalWeight += tmpModVertWeight._touchedWeight[i];
										}
									}

									if (pullTouchTotalWeight > 0.0f)
									{
										pullTouchPos /= pullTouchTotalWeight;
										pullTouchPos = paramKeyValue._modifiedMesh._targetTransform._rootUnit._transform.InverseTransformVector(pullTouchPos);
										pullTouchPos.x = -pullTouchPos.x;
										pullTouchPos.y = -pullTouchPos.y;

										float itpPull = Mathf.Clamp01(pullTouchTotalWeight);

										//Debug.Log("Touch DeltaPos (" + pullTouchTotalWeight + ") " + limitedNextCalPos + " >> " + pullTouchPos + " / ITP : " + itpPull);

										touchCalculatedDeltaPos = pullTouchPos;
										isTouchCalculated = true;
										touchCalculatedWeight = itpPull;


									}
								}


								//추가
								//> 유효한 프레임 : 물리 계산을 한다.
								//> 생략하는 프레임 : 이전 속도를 그대로 이용한다.
								if (isValidFrame)
								{




									tmpF_stretch = Vector2.zero;
									//F_bend = Vector2.zero;
									//float totalStretchWeight = 0.0f;



									//1) 장력 Strech : -k * (<delta Dist> * 기존 UnitVector)
									//int iVert_Src = tmpModVertWeight._vertIndex;
									for (int iLinkVert = 0; iLinkVert < tmpPhysicVertParam._linkedVertices.Count; iLinkVert++)
									{
										tmpLinkedVert = tmpPhysicVertParam._linkedVertices[iLinkVert];
										float linkWeight = tmpLinkedVert._distWeight;

										tmpSrcVertPos_NoMod = tmpModVertWeight._pos_World_NoMod;
										tmpLinkVertPos_NoMod = tmpLinkedVert._modVertWeight._pos_World_NoMod;
										tmpLinkedVert._deltaPosToTarget_NoMod = tmpSrcVertPos_NoMod - tmpLinkVertPos_NoMod;

										//tmpSrcVertPos_Cur = paramKeyValue._modifiedMesh._targetTransform._rootUnit._transform.InverseTransformPoint(tmpModVertWeight._pos_Real);
										//tmpLinkVertPos_Cur = paramKeyValue._modifiedMesh._targetTransform._rootUnit._transform.InverseTransformPoint(tmpLinkedVert._modVertWeight._pos_Real);
										tmpSrcVertPos_Cur = tmpModVertWeight._pos_World_LocalTransform;
										tmpLinkVertPos_Cur = tmpLinkedVert._modVertWeight._pos_World_LocalTransform;

										tmpDeltaVec_0 = tmpSrcVertPos_NoMod - tmpLinkVertPos_NoMod;
										tmpDeltaVec_Cur = tmpSrcVertPos_Cur - tmpLinkVertPos_Cur;


										//tmpF_stretch += -1.0f * tmpPhysicMeshParam._stretchK * (tmpDeltaVec_Cur - tmpDeltaVec_0) * linkWeight;
										//totalStretchWeight += linkWeight;
										//길이 차이로 힘을 만들고
										//방향은 현재 Delta

										//<추가> 만약 장력 벡터가 완전히 뒤집힌 경우
										//면이 뒤집혔다.
										if(Vector2.Dot(tmpDeltaVec_0, tmpDeltaVec_Cur) < 0)
										{
											//면이 뒤집혔다.
											tmpF_stretch += tmpPhysicMeshParam._stretchK * (tmpDeltaVec_0 - tmpDeltaVec_Cur) * linkWeight;
										}
										else
										{
											//정상 면
											tmpF_stretch += -1.0f * tmpPhysicMeshParam._stretchK * (tmpDeltaVec_Cur.magnitude - tmpDeltaVec_0.magnitude) * tmpDeltaVec_Cur.normalized * linkWeight;
										}
										
										

									}
									tmpF_stretch *= -1;//<<위치기반인 경우 좌표계가 반대여서 -1을 넣는다. <<< 이게 왜이리 힘들던지;;




									//3) 공기 저항 : "현재 이동 방향의 반대 방향"
									//수정 : 이게 너무 약하다.
									//tmpF_airDrag = -1.0f * tmpPhysicMeshParam._airDrag * tmpModVertWeight._velocity_Real;
									//tmpF_airDrag = -1.0f * tmpPhysicMeshParam._airDrag * tmpModVertWeight._velocity_Real / tDelta;



									//5) 복원력
									tmpF_recover = -1.0f * tmpPhysicMeshParam._restoring * tmpModVertWeight._calculatedDeltaPos;



									//변동
									//중력과 바람은 크기는 그대로 두고, 방향은 World였다고 가정
									//Local로 오기 위해서는 Inverse를 해야한다.
									float gravitySize = tmpF_gravity.magnitude;
									float windSize = tmpF_wind.magnitude;
									Vector2 tmpF_gravityL = Vector2.zero;
									Vector2 tmpF_windL = Vector2.zero;
									if (gravitySize > 0.0f)
									{
										tmpF_gravityL = paramKeyValue._modifiedMesh._targetTransform._rootUnit._transform.InverseTransformVector(tmpF_gravity.normalized).normalized * gravitySize;
										tmpF_gravityL.y = -tmpF_gravityL.y;
										tmpF_gravityL.x = -tmpF_gravityL.x;
										//tmpF_gravityL *= 10000.0f;
									}
									if (windSize > 0.0f)
									{
										tmpF_windL = paramKeyValue._modifiedMesh._targetTransform._rootUnit._transform.InverseTransformVector(tmpF_wind.normalized).normalized * windSize;
										tmpF_windL.y = -tmpF_windL.y;
										tmpF_windL.x = -tmpF_windL.x;
										//tmpF_windL *= 10000.0f;
									}

									//if(tmpModVertWeight._weight > 0.5f && isFirstDebug)
									//{
									//	Debug.Log("Wind Local : " + tmpF_windL + " / Gravity Local : " + tmpF_gravityL);
									//	isFirstDebug = false;
									//}

									//6) 추가 : 외부 힘
									if (_portrait.IsAnyForceEvent)
									{
										//이전 프레임에서의 힘을 이용한다.
										//해당 위치가 Local이고, 요청된 힘은 World이다.
										//World로 계산한 뒤의 위치를 잡자...는 이미 World였네요.
										//그대로 하고, 힘만 로컬로 바구면 될 듯
										Vector2 F_extW = _portrait.GetForce(tmpModVertWeight._pos_1F);
										float powerSize = F_extW.magnitude;
										tmpF_ext = paramKeyValue._modifiedMesh._targetTransform._rootUnit._transform.InverseTransformVector(F_extW).normalized * powerSize;
										tmpF_ext.x = -tmpF_ext.x;
										tmpF_ext.y = -tmpF_ext.y;
									}

									float inertiaK = Mathf.Clamp01(tmpPhysicMeshParam._inertiaK);

									//5) 힘의 합력을 구한다.
									//-------------------------------------------
									if (tmpModVertWeight._physicParam._isMain)
									{
										//tmpF_sum = tmpF_gravityL + tmpF_windL + tmpF_stretch + tmpF_airDrag + tmpF_recover + tmpF_ext;//관성 제외 (중력, 바람 W2L)
										tmpF_sum = tmpF_gravityL + tmpF_windL + tmpF_stretch + tmpF_recover + tmpF_ext;//관성 제외 (중력, 바람 W2L) - 공기 저항 제외
									}
									else
									{
										//tmpF_sum = tmpF_gravityL + tmpF_windL + tmpF_stretch + ((tmpF_airDrag + tmpF_recover + tmpF_ext) * 0.5f);//관성 제외 (중력, 바람 W2L)
										tmpF_sum = tmpF_gravityL + tmpF_windL + tmpF_stretch + ((tmpF_recover + tmpF_ext) * 0.5f);//관성 제외 (중력, 바람 W2L) - 공기저항 제외

										inertiaK *= 0.5f;//<<관성 감소
									}
									//tmpF_sum = tmpF_gravityL + tmpF_windL + tmpF_recover + tmpF_airDrag;
									//-------------------------------------------

									//tmpF_sum *= tmpPhysicMeshParam._optPhysicScale;//<<Opt에선 적당히 Scale을 줘야한다.

									if (isTouchCalculated)
									{
										tmpF_sum *= (1.0f - touchCalculatedWeight);
									}

									//F = ma
									//a = F / m
									//Vector2 acc = F_sum / mass;

									//S = vt + S0
									//-------------------------------
									

									//<<수정>>
									tmpModVertWeight._velocity_Next = 
											//(tmpModVertWeight._velocity_Real * inertiaK + tmpModVertWeight._velocity_1F * (1.0f - inertiaK))//관성
											//+ 
											//tmpModVertWeight._velocity_1F + (tmpModVertWeight._velocity_1F - tmpModVertWeight._velocity_Real) * inertiaK
											//+ (tmpF_sum / tmpMass) * tDelta
											//tmpModVertWeight._velocity_Real + (tmpModVertWeight._velocity_1F - tmpModVertWeight._velocity_Real) * inertiaK

											tmpModVertWeight._velocity_1F 
											+ (tmpModVertWeight._velocity_1F - tmpModVertWeight._velocity_Real) * inertiaK
											+ (tmpF_sum / tmpMass) * tDelta											
											;

									
									//Air Drag식 수정
									if(tmpPhysicMeshParam._airDrag > 0.0f)
									{
										tmpModVertWeight._velocity_Next *= Mathf.Clamp01((1.0f - (tmpPhysicMeshParam._airDrag * tDelta) / (tmpMass + 0.5f)));
									}
									//-------------------------------
								}
								else
								{
									//-------------------------------------
									//tmpModVertWeight._velocity_Next = tmpModVertWeight._velocity_Real;
									tmpModVertWeight._velocity_Next = tmpModVertWeight._velocity_1F;
									//-------------------------------------
								}



								//변경.
								//여기서 일단 속력을 미리 적용하자
								if (isValidFrame)
								{
									tmpNextVelocity = tmpModVertWeight._velocity_Next;

									//if(tmpModVertWeight._vertIndex == 0 && tmpNextVelocity.sqrMagnitude > 0)
									//{
									//	Debug.LogError("Next Vel : " + tmpNextVelocity + " / Vel 1F : " + tmpModVertWeight._velocity_1F);
									//}
									Vector2 limitedNextCalPos = tmpModVertWeight._calculatedDeltaPos + (tmpNextVelocity * tDelta);

									//터치 이벤트에 의해서 속도가 보간된다.
									if (isTouchCalculated)
									{
										limitedNextCalPos = (limitedNextCalPos * (1.0f - touchCalculatedWeight)) + (touchCalculatedDeltaPos * touchCalculatedWeight);
										tmpNextVelocity = (limitedNextCalPos - tmpModVertWeight._calculatedDeltaPos) / tDelta;
									}

									//V += at
									//마음대로 증가하지 않도록 한다.
									if (tmpPhysicMeshParam._isRestrictMoveRange)
									{
										float radiusFree = tmpPhysicMeshParam._moveRange * 0.5f;
										float radiusMax = tmpPhysicMeshParam._moveRange;

										if (radiusMax <= radiusFree)
										{
											tmpNextVelocity *= 0.0f;
											//둘다 0이라면 아예 이동이 불가
											if (!tmpModVertWeight._isLimitPos)
											{
												tmpModVertWeight._isLimitPos = true;
												tmpModVertWeight._limitScale = 0.0f;
											}
										}
										else
										{
											float curDeltaPosSize = (limitedNextCalPos).magnitude;

											if (curDeltaPosSize < radiusFree)
											{
												//moveRatio = 1.0f;
												//별일 없슴다
											}
											else
											{
												//기본은 선형의 사이즈이지만,
												//돌아가는 힘은 유지해야한다.
												//[deltaPos unitVector dot newVelocity] = 1일때 : 바깥으로 나가려는 힘
												// = -1일때 : 안으로 들어오려는 힘
												// -1 ~ 1 => 0 ~ 1 : 0이면 moveRatio가 1, 1이면 moveRatio가 거리에 따라 1>0
												float dotVector = Vector2.Dot(tmpModVertWeight._calculatedDeltaPos.normalized, tmpNextVelocity.normalized);
												dotVector = (dotVector * 0.5f) + 0.5f; //0: 속도 느려짐 없음 (안쪽으로 들어가려고 함), 1:증가하는 방향

												float outerItp = Mathf.Clamp01((curDeltaPosSize - radiusFree) / (radiusMax - radiusFree));//0 : 속도 느려짐 없음, 1:속도 0

												tmpNextVelocity *= Mathf.Clamp01(1.0f - (dotVector * outerItp));//적절히 느려지게 만들자

												if (curDeltaPosSize > radiusMax)
												{
													if (!tmpModVertWeight._isLimitPos || radiusMax < tmpModVertWeight._limitScale)
													{
														tmpModVertWeight._isLimitPos = true;
														tmpModVertWeight._limitScale = radiusMax;
													}
												}
											}
											//else
											//{
											//	//tmpNextCalPos = calPosUnitVec * radiusMax;
											//	limitedNextCalPos = limitedNextCalPos.normalized * radiusMax;//<<최대치만 이동한다.
											//}
										}
									}

									//장력에 의한 길이 제한도 처리한다.
									if (tmpPhysicMeshParam._isRestrictStretchRange)
									{

										bool isLimitVelocity2Max = false;
										Vector2 stretchLimitPos = Vector2.zero;
										float limitCalPosDist = 0.0f;
										for (int iLinkVert = 0; iLinkVert < tmpPhysicVertParam._linkedVertices.Count; iLinkVert++)
										{
											tmpLinkedVert = tmpPhysicVertParam._linkedVertices[iLinkVert];
											//길이의 Min/Max가 있다.
											float distStretchBase = tmpLinkedVert._deltaPosToTarget_NoMod.magnitude;

											float stretchRangeMax = (tmpPhysicMeshParam._stretchRangeRatio_Max) * distStretchBase;
											float stretchRangeMax_Half = (tmpPhysicMeshParam._stretchRangeRatio_Max * 0.5f) * distStretchBase;

											Vector2 curDeltaFromLinkVert = limitedNextCalPos - tmpLinkedVert._modVertWeight._calculatedDeltaPos_Prev;
											float curDistFromLinkVert = curDeltaFromLinkVert.magnitude;

											//너무 멀면 제한한다.
											//단, 제한 권장은 Weight에 맞게

											//float weight = Mathf.Clamp01(tmpLinkedVert._distWeight);
											isLimitVelocity2Max = false;

											if (curDistFromLinkVert > stretchRangeMax_Half)
											{
												isLimitVelocity2Max = true;//늘어나는 한계점으로 이동하는 중
												stretchLimitPos = tmpLinkedVert._modVertWeight._calculatedDeltaPos_Prev + curDeltaFromLinkVert.normalized * stretchRangeMax;
												stretchLimitPos -= tmpModVertWeight._calculatedDeltaPos_Prev;


												//limitCalPosDist = stretchRangeMax;
												limitCalPosDist = (stretchLimitPos).magnitude;
												//if (curDistFromLinkVert >= stretchRangeMax)
												//{
												//	limitCalPosDist = (stretchLimitPos).magnitude;
												//}
											}

											if (isLimitVelocity2Max)
											{
												//LinkVert간의 벡터를 기준으로 nextVelocity가 확대/축소하는 방향이라면 그 반대의 값을 넣는다.
												float dotVector = Vector2.Dot(curDeltaFromLinkVert.normalized, tmpNextVelocity.normalized);
												//-1 : 축소하려는 방향으로 이동하는 중
												//1 : 확대하려는 방향으로 이동하는 중


												float outerItp = 0.0f;
												if (isLimitVelocity2Max)
												{
													//너무 바깥으로 이동하려고 할때, 속도를 줄인다.
													dotVector = Mathf.Clamp01(dotVector);
													if (stretchRangeMax > stretchRangeMax_Half)
													{
														outerItp = Mathf.Clamp01((curDistFromLinkVert - stretchRangeMax_Half) / (stretchRangeMax - stretchRangeMax_Half));
													}
													else
													{
														outerItp = 1.0f;//무조건 속도 0

														if (!tmpModVertWeight._isLimitPos || limitCalPosDist < tmpModVertWeight._limitScale)
														{
															tmpModVertWeight._isLimitPos = true;
															tmpModVertWeight._limitScale = limitCalPosDist;
														}
													}

												}

												tmpNextVelocity *= Mathf.Clamp01(1.0f - (dotVector * outerItp));//적절히 느려지게 만들자
											}


										}
										//nextVelocity *= velRatio;

										//Profiler.EndSample();

										//limitedNextCalPos = modVertWeight._calculatedDeltaPos + (nextVelocity * tDelta);
									}
									limitedNextCalPos = tmpModVertWeight._calculatedDeltaPos + (tmpNextVelocity * tDelta);

									//이걸 한번더 해서 위치 보정
									if (isTouchCalculated)
									{
										Vector2 nextTouchPos = (limitedNextCalPos * (1.0f - touchCalculatedWeight)) + (touchCalculatedDeltaPos * touchCalculatedWeight);

										//limitedNextCalPos = nextTouchPos.normalized * limitedNextCalPos.magnitude;
										limitedNextCalPos = nextTouchPos;
										//tmpNextVelocity *= (1.0f - touchCalculatedWeight);
										tmpNextVelocity = (limitedNextCalPos - tmpModVertWeight._calculatedDeltaPos) / tDelta;
									}

									tmpModVertWeight._velocity_Next = tmpNextVelocity;
									tmpModVertWeight._calculatedDeltaPos_Prev = tmpModVertWeight._calculatedDeltaPos;
									//tmpModVertWeight._calculatedDeltaPos += tmpModVertWeight._velocity_Next * tDelta;
									tmpModVertWeight._calculatedDeltaPos = limitedNextCalPos;
								}
								else
								{
									tmpModVertWeight._calculatedDeltaPos_Prev = tmpModVertWeight._calculatedDeltaPos;

									tmpNextVelocity = tmpModVertWeight._velocity_Next;
									tmpModVertWeight._calculatedDeltaPos = tmpModVertWeight._calculatedDeltaPos + (tmpNextVelocity * tDelta);
								}
							}

							//1차로 계산된 값을 이용하여 점성력을 체크한다.
							//수정 : 이미 위치는 계산되었다. 위치를 중심으로 처리를 하자 점성/이동한계를 계산하자
							for (int iPos = 0; iPos < tmpNumVert; iPos++)
							{
								tmpModVertWeight = paramKeyValue._modifiedMesh._vertWeights[iPos];
								tmpPhysicVertParam = tmpModVertWeight._physicParam;

								if (!tmpModVertWeight._isEnabled)
								{
									//처리 안함다
									tmpModVertWeight._calculatedDeltaPos = Vector2.zero;
									continue;
								}
								if (tmpModVertWeight._vertex == null)
								{
									Debug.LogError("Render Vertex is Not linked");
									break;
								}

								if (isValidFrame)
								{

									tmpNextVelocity = tmpModVertWeight._velocity_Next;
									tmpNextCalPos = tmpModVertWeight._calculatedDeltaPos;


									if (tmpIsViscosity && !tmpModVertWeight._physicParam._isMain)
									{

										//점성 로직 추가
										//ID가 같으면 DeltaPos가 비슷해야한다.
										tmpLinkedViscosityWeight = 0.0f;
										//tmpLinkedViscosityNextVelocity = Vector2.zero;
										tmpLinkedTotalCalPos = Vector2.zero;

										int curViscosityID = tmpModVertWeight._physicParam._viscosityGroupID;

										for (int iLinkVert = 0; iLinkVert < tmpPhysicVertParam._linkedVertices.Count; iLinkVert++)
										{
											tmpLinkedVert = tmpPhysicVertParam._linkedVertices[iLinkVert];
											float linkWeight = tmpLinkedVert._distWeight;

											if ((tmpLinkedVert._modVertWeight._physicParam._viscosityGroupID & curViscosityID) != 0)
											{
												//float subWeight = 1.0f;
												//tmpLinkedViscosityNextVelocity += tmpLinkedVert._modVertWeight._velocity_Next * linkWeight * subWeight;//사실 Vertex의 호출 순서에 따라 값이 좀 다르다.
												tmpLinkedTotalCalPos += tmpLinkedVert._modVertWeight._calculatedDeltaPos * linkWeight;//<<Vel 대신 Pos로 바꾸자
												tmpLinkedViscosityWeight += linkWeight;
											}
										}

										//점성도를 추가한다.
										if (tmpLinkedViscosityWeight > 0.0f)
										{
											//tmpLinkedViscosityNextVelocity /= tmpLinkedViscosityWeight;
											//tmpLinkedTotalCalPos /= tmpLinkedViscosityWeight;
											float clampViscosity = Mathf.Clamp01(tmpPhysicMeshParam._viscosity) * 0.7f;


											//tmpNextVelocity = tmpNextVelocity * (1.0f - clampViscosity) + tmpLinkedViscosityNextVelocity * clampViscosity;
											tmpNextCalPos = tmpNextCalPos * (1.0f - clampViscosity) + tmpLinkedTotalCalPos * clampViscosity;
										}

									}

									//이동 한계 한번 더 계산
									if (tmpModVertWeight._isLimitPos && tmpNextCalPos.magnitude > tmpModVertWeight._limitScale)
									{
										tmpNextCalPos = tmpNextCalPos.normalized * tmpModVertWeight._limitScale;
										//Debug.Log("Limit Scale : " + tmpModVertWeight._limitScale);
									}


									//계산 끝!
									//새로운 변위를 넣어주자
									tmpModVertWeight._calculatedDeltaPos = tmpNextCalPos;


									//속도를 다시 계산해주자
									tmpNextVelocity = (tmpModVertWeight._calculatedDeltaPos - tmpModVertWeight._calculatedDeltaPos_Prev) / tDelta;

									
									//-----------------------------------------------------------------------------------------
									//속도 갱신
									tmpModVertWeight._velocity_Next = tmpNextVelocity;
									
									
									//<<수정>
									//tmpModVertWeight._velocity_1F = tmpNextVelocity;//이게 관성을 수정한 버전
									//tmpModVertWeight._velocity_1F = tmpModVertWeight._velocity_Real;//<<이게 이전 버전
									
									//속도 차이가 크다면 Real의 비중이 커야 한다.
									//같은 방향이면 -> 버티기 관성이 더 잘보이는게 좋다
									//다른 방향이면 Real을 관성으로 사용해야한다. (그래야 다음 프레임에 관성이 크게 보임)
									//속도 변화에 따라서 체크
									float velocityRefreshITP_X = Mathf.Clamp01(Mathf.Abs( ((tmpModVertWeight._velocity_Real.x - tmpModVertWeight._velocity_Real1F.x) / (Mathf.Abs(tmpModVertWeight._velocity_Real1F.x) + 0.1f)) * 0.5f ) );
									float velocityRefreshITP_Y = Mathf.Clamp01(Mathf.Abs( ((tmpModVertWeight._velocity_Real.y - tmpModVertWeight._velocity_Real1F.y) / (Mathf.Abs(tmpModVertWeight._velocity_Real1F.y) + 0.1f)) * 0.5f ) );

									//tmpModVertWeight._velocity_1F = tmpNextVelocity * (1.0f - inertiaK) + (inertiaK * (tmpNextVelocity * 0.7f + tmpModVertWeight._velocity_Real * 0.3f));//<<대충 섞어서..
									tmpModVertWeight._velocity_1F.x = tmpNextVelocity.x * (1.0f - velocityRefreshITP_X) + (tmpNextVelocity.x * 0.5f + tmpModVertWeight._velocity_Real.x * 0.5f) * velocityRefreshITP_X;
									tmpModVertWeight._velocity_1F.y = tmpNextVelocity.y * (1.0f - velocityRefreshITP_Y) + (tmpNextVelocity.y * 0.5f + tmpModVertWeight._velocity_Real.y * 0.5f) * velocityRefreshITP_Y;

									tmpModVertWeight._pos_1F = tmpModVertWeight._pos_Real;

									//-----------------------------------------------------------------------------------------


									//Damping
									if (tmpModVertWeight._calculatedDeltaPos.sqrMagnitude < tmpPhysicMeshParam._damping * tmpPhysicMeshParam._damping
										&& tmpNextVelocity.sqrMagnitude < tmpPhysicMeshParam._damping * tmpPhysicMeshParam._damping)
									{
										tmpModVertWeight._calculatedDeltaPos = Vector2.zero;
										tmpModVertWeight.DampPhysicVertex();
									}
								}

								//if (iPos == 0)
								//{
								//if (!isValidFrame)
								//{
								//	Debug.Log("Physics : " + tmpModVertWeight._calculatedDeltaPos + " (" + isValidFrame + " / " + tDelta + ")");
								//}
								//}
								tmpPosList[iPos] +=
										(tmpModVertWeight._calculatedDeltaPos * tmpModVertWeight._weight)
										* paramKeyValue._weight;//<<이 값을 이용한다.


							}
							//---------------------------- Pos List
						}
						if (isFirstParam)
						{
							isFirstParam = false;
						}


						nCalculated++;//Visible 계산을 위해 "paramKey 계산 횟수"를 카운트하자

					}//--- Params



					//이제 tmp값을 Result에 넘겨주자
					//처음 Layer라면 -> 100% 적용
					//그렇지 않다면 Blend를 해주자

					layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);


					calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.


					//if (keyParamSetGroup._layerIndex == 0)
					if (iCalculatedSubParam == 0)//<<변경
					{
						for (int iPos = 0; iPos < posList.Length; iPos++)
						{
							posList[iPos] = tmpPosList[iPos] * layerWeight;
						}
					}
					else
					{
						switch (keyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									if (weightedVertData != null)
									{
										//Vertex 가중치가 추가되었다.
										float vertWeight = 0.0f;
										for (int iPos = 0; iPos < posList.Length; iPos++)
										{
											vertWeight = layerWeight * weightedVertData._vertWeightList[iPos];

											posList[iPos] += tmpPosList[iPos] * vertWeight;
										}
									}
									else
									{
										for (int iPos = 0; iPos < posList.Length; iPos++)
										{
											posList[iPos] += tmpPosList[iPos] * layerWeight;
										}
									}
								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									if (weightedVertData != null)
									{
										//Vertex 가중치가 추가되었다.
										float vertWeight = 0.0f;
										for (int iPos = 0; iPos < posList.Length; iPos++)
										{
											vertWeight = layerWeight * weightedVertData._vertWeightList[iPos];

											posList[iPos] = (posList[iPos] * (1.0f - vertWeight)) +
															(tmpPosList[iPos] * vertWeight);
										}
									}
									else
									{
										for (int iPos = 0; iPos < posList.Length; iPos++)
										{
											posList[iPos] = (posList[iPos] * (1.0f - layerWeight)) +
															(tmpPosList[iPos] * layerWeight);
										}
									}
								}
								break;

							default:
								Debug.LogError("Mod-Physics : Unknown BLEND_METHOD : " + keyParamSetGroup._blendMethod);
								break;
						}
					}

					iCalculatedSubParam++;

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)
				calParam._isAvailable = true;


			}
		}


		// Get / Set
		//---------------------------------------------------------------------------------------
		/// <summary>
		/// CalculatedResultParam을 찾는다.
		/// Bone은 Null인 대상만을 고려한다.
		/// </summary>
		/// <param name="targetOptTransform"></param>
		/// <returns></returns>
		public apOptCalculatedResultParam GetCalculatedResultParam(apOptTransform targetOptTransform)
		{
			return _calculatedResultParams.Find(delegate (apOptCalculatedResultParam a)
			{
				return a._targetOptTransform == targetOptTransform && a._targetBone == null;
			});
		}

		/// <summary>
		/// GetCalculatedResultParam의 ModBone 버전.
		/// Bone까지 비교하여 동일한 CalculatedResultParam을 찾는다.
		/// </summary>
		/// <param name="targetOptTransform"></param>
		/// <param name="bone"></param>
		/// <returns></returns>
		public apOptCalculatedResultParam GetCalculatedResultParam_Bone(apOptTransform targetOptTransform, apOptBone bone)
		{
			return _calculatedResultParams.Find(delegate (apOptCalculatedResultParam a)
			{
				return a._targetOptTransform == targetOptTransform && a._targetBone == bone;
			});
		}


	}

}