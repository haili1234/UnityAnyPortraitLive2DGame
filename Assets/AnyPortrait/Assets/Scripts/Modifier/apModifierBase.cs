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
//using UnityEngine.Profiling;


using AnyPortrait;

namespace AnyPortrait
{

	[Serializable]
	public class apModifierBase : MonoBehaviour
	{
		// Members
		//----------------------------------------------
		public enum MODIFIER_TYPE
		{
			Base = 0,
			Volume = 1,
			Morph = 2,
			AnimatedMorph = 3,
			Rigging = 4,
			Physic = 5,
			TF = 6,
			AnimatedTF = 7,
			FFD = 8,
			AnimatedFFD = 9,
		}

		/// <summary>
		/// 다른 Modifier와 Blend 되는 방식
		/// </summary>
		public enum BLEND_METHOD
		{
			/// <summary>기존 값을 유지하면서 변화값을 덮어 씌운다.</summary>
			Additive = 0,
			/// <summary>기존 값과 선형 보간을 하며 덮어씌운다.</summary>
			Interpolation = 1
		}

		[NonSerialized]
		public apPortrait _portrait = null;

		//고유 ID. 모디파이어도 고유 아이디를 갖는다.
		public int _uniqueID = -1;

		//레이어
		public int _layer = -1;//낮을수록 먼저 처리된다. (오름차순으로 배열)

		//레이어 병합시 가중치 (0~1)
		public float _layerWeight = 0.0f;

		//메인 MeshGroup
		public int _meshGroupUniqueID = -1;

		//지금 처리가 되는가
		[NonSerialized]
		public bool _isActive = true;


		public enum MOD_EDITOR_ACTIVE
		{
			Enabled = 0,
			ExclusiveEnabled = 1,
			Disabled = 2,
			SubExEnabled = 3,//<<추가 : 기본적으로는 Disabled이다. 다만, 대상이 "선택한 Mod"에 "등록되지 않은 경우"에는 이 Modifier가 계산될 수 있다.
			OnlyColorEnabled = 4,//<<추가 : 기본적으로는 Disabled이다. Color만 업데이트되는 ParamSetGroup이 한개 이상 존재하는 경우
		}

		[NonSerialized]
		public MOD_EDITOR_ACTIVE _editorExclusiveActiveMod = MOD_EDITOR_ACTIVE.Enabled;//<<에디터에서만 적용되는 배제에 따른 적용 여부



		[NonSerialized]
		public apMeshGroup _meshGroup = null;


		public BLEND_METHOD _blendMethod = BLEND_METHOD.Interpolation;

		//[SerializeField]
		//public List<apModifierParamSet> _paramSetList = new List<apModifierParamSet>();


		//수정 -> 이것도 Serialize + Layer로 정의합니다.
		[SerializeField]
		public List<apModifierParamSetGroup> _paramSetGroup_controller = new List<apModifierParamSetGroup>();

		//추가 : AnimClip을 키값으로 하는 ParamSetGroup들을 한번에 참조하는 AnimPack
		[NonSerialized]
		public List<apModifierParamSetGroupAnimPack> _paramSetGroupAnimPacks = new List<apModifierParamSetGroupAnimPack>();



		//계산 결과가 저장되는 변수들
		//[NonSerialized]
		//public apCalculatedRenderUnit _calculateResult = null;


		[NonSerialized]
		public Dictionary<apVertex, apMatrix3x3> _vertWorldMatrix = new Dictionary<apVertex, apMatrix3x3>();

		//각 RenderUnit으로 계산 결과를 보내주는 Param들
		[NonSerialized]
		public List<apCalculatedResultParam> _calculatedResultParams = new List<apCalculatedResultParam>();


		//추가
		//색상 값이 일괄적으로 들어가니 값 처리에 문제가 생긴다.
		//설정값으로 켜고 끄게 해야한다.
		/// <summary>
		/// 기본적으로 색상값이 적용되는 모디파이어에서 사용자가 선택적으로 색상은 설정하지 않도록 만들 수 있다.
		/// CalculatedValueType에 Color값이 없다면 이 변수는 의미가 없다.
		/// </summary>
		[SerializeField]
		public bool _isColorPropertyEnabled = true;

		[NonSerialized]
		protected System.Diagnostics.Stopwatch _stopwatch = null;

		[NonSerialized]
		protected float _tDeltaFixed = 0.0f;

		private const float PHYSIC_DELTA_TIME = 0.033f;//20FPS (0.05), 30FPS (0.033)//<<고정 프레임으로 하자

		// Init
		//----------------------------------------------
		//생성자가 없다.
		//public apModifierBase()
		//{
		//	Init();
		//}

		void Start()
		{
			//업데이트 하지 않습니다. 데이터로만 존재
			this.enabled = false;
		}

		public void LinkPortrait(apPortrait portrait)
		{
			_portrait = portrait;
			for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			{
				_paramSetGroup_controller[i].LinkPortrait(portrait, this);
			}

			_portrait.RegistUniqueID(apIDManager.TARGET.Modifier, _uniqueID);
		}

		//이 함수는 사용하지 않는다. 삭제
		//public virtual void Init()
		//{

		//}

		public virtual void SetInitSetting(int uniqueID, int layer, int meshGroupID, apMeshGroup meshGroup)
		{
			_uniqueID = uniqueID;
			_layer = layer;
			_layerWeight = 1.0f;
			_meshGroupUniqueID = meshGroupID;
			_meshGroup = meshGroup;

			//_paramSetGroup_controller.Clear();//클리어하면 안됩니더;;

			//_calculateResult = new apCalculatedRenderUnit();
			//_calculateResult.SetMeshGroup(_meshGroup);
			//_calculateResult.Clear();

			RefreshParamSet();
		}





		/// <summary>
		/// ParamSet의 추가 / 삭제시 한번씩 호출해주자
		/// + Editor / Realtime 첫 시작시 호출 필요
		/// </summary>
		public virtual void RefreshParamSet()
		{
			for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			{
				_paramSetGroup_controller[i].RemoveInvalidParamSet();
			}

			//ParamSet이 없는 경우
			_paramSetGroup_controller.RemoveAll(delegate (apModifierParamSetGroup a)
			{
				return a._paramSetList.Count == 0;
			});

			
			//추가 : SyncTarget에 따라서 삭제 여부를 체크하자
			_paramSetGroup_controller.RemoveAll(delegate (apModifierParamSetGroup a)
			{
				switch (a._syncTarget)
				{
					case apModifierParamSetGroup.SYNC_TARGET.Static:
					//자동으로 삭제 안됨
					break;

					case apModifierParamSetGroup.SYNC_TARGET.Controller:
						{
							if (a._keyControlParam == null)
							{
							//해당 ControlParam이 없다면 삭제
							return true;
							}
						//ControlParam이 있다고 해도, "현재" 없다면 삭제
						if (!_portrait._controller._controlParams.Contains(a._keyControlParam))
							{
								return true;
							}
						}
						break;

					case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
					//Debug.LogError("TODO : KeyFrame에 따라서 삭제 여부 체크");
					{
							if (a._keyAnimClip == null ||
								a._keyAnimTimeline == null ||
								a._keyAnimTimelineLayer == null)
							{
								return true;//<<삭제
						}
						}
						break;
				}
				return false;

			});

			//이것도 당연한거니 로그 출력을 하지 않는다.
			//if (nRemovedNoLink > 0)
			//{
			//	Debug.LogError("[" + nRemovedNoLink + "] 개의 링크되지 않은 ParamSetGroup을 삭제했습니다. (" + DisplayName + ")");
			//}

			#region [미사용 코드]
			//Refresh는 Control Param의 유무만 따지

			//_paramSetPerControlParam.Clear();
			//for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			//{
			//	_paramSetGroup_controller[i].ClearParamSet();//<<그동안 연동된 ParamSet을 비워두고 연동 준비
			//	_paramSetGroup_controller[i]._isSync = false;
			//}

			//for (int i = 0; i < _paramSetList.Count; i++)
			//{
			//	apModifierParamSet modParamSet = _paramSetList[i];
			//	if(modParamSet._syncTarget == apModifierParamSet.SYNC_TARGET.Controller)
			//	{
			//		if(modParamSet._controlParam != null)
			//		{
			//			apModifierParamSetGroup existGroup = GetParamSetGroup(modParamSet._controlParam);
			//			if(existGroup == null)
			//			{
			//				int nextGroupIndex = GetNextParamSetLayerIndex();
			//				existGroup = new apModifierParamSetGroup(this, nextGroupIndex);
			//				existGroup._keyControlParamName = modParamSet._controlParam._keyName;
			//				existGroup._keyControlParam = modParamSet._controlParam;
			//				existGroup._isSync = true;

			//				_paramSetGroup_controller.Add(existGroup);
			//			}
			//			else
			//			{
			//				existGroup._isSync = true;
			//			}

			//			if(!existGroup._paramSetList.Contains(modParamSet))
			//			{
			//				existGroup.AddParamSet(modParamSet);
			//			}
			//		}

			//	}
			//} 
			#endregion

			for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			{
				#region [미사용 코드]
				//_paramSetGroup_controller[i]._paramSetList.RemoveAll(delegate (apModifierParamSet a)
				//{
				//	//MeshData 또는 Bone 데이터가 없는 ParamSet은 제거한다.
				//	bool isAnyMeshDataOrBoneData = false;

				//	for (int iModMesh = 0; iModMesh < a._meshData.Count; iModMesh++)
				//	{
				//		apModifiedMesh modMesh = a._meshData[iModMesh];
				//		if (modMesh._transform_Mesh != null || modMesh._transform_MeshGroup != null)
				//		{

				//		}

				//		if(modMesh._transform_Mesh == null && 
				//			)
				//		{
				//			isAnyMeshDataOrBoneData = true;
				//		}
				//	}

				//	return !isValid;
				//	//bool isRealExist = _paramSetList.Exists(delegate (apModifierParamSet b)
				//	//{
				//	//	return a == b;
				//	//});

				//	//return !isRealExist;//<<존재하지 않는 값은 삭제
				//}); 
				#endregion

				_paramSetGroup_controller[i].SortParamSet();
				_paramSetGroup_controller[i].RefreshSync();
			}

			//추가 : AnimPack에 대한 ParamSetGroup을 다시 만들어주자
			if (_paramSetGroupAnimPacks == null)
			{
				_paramSetGroupAnimPacks = new List<apModifierParamSetGroupAnimPack>();
			}
			_paramSetGroupAnimPacks.RemoveAll(delegate (apModifierParamSetGroupAnimPack a)
			{
				if (!_portrait._animClips.Contains(a.LinkedAnimClip))
				{
				//AnimClip이 없으면 삭제
				return true;
				}
				return false;
			});
			for (int i = 0; i < _paramSetGroupAnimPacks.Count; i++)
			{
				//존재하지 않는 paramSetGroup을 삭제하자
				_paramSetGroupAnimPacks[i].RemoveInvalidParamSetGroup(_paramSetGroup_controller);
			}
			//paramSetGroup을 돌면서 새로 생성 또는 AnimPack에 추가를 해보자
			//Anim 타입인 경우에만
			for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			{
				if (_paramSetGroup_controller[i]._keyAnimClip == null)
				{
					continue;
				}

				apModifierParamSetGroup psg = _paramSetGroup_controller[i];
				apModifierParamSetGroupAnimPack targetAnimPack = GetParamSetGroupAnimPack(psg._keyAnimClip);
				if (targetAnimPack == null)
				{
					targetAnimPack = new apModifierParamSetGroupAnimPack(this, psg._keyAnimClip);
					_paramSetGroupAnimPacks.Add(targetAnimPack);
				}

				targetAnimPack.AddParamSetGroup(psg);
			}


			//그리고 정렬
			SortParamSetGroups();
			#region [미사용 코드]
			////계산 Result에 미리 Mesh를 넣어주고 계산할 준비를 한다.
			//if (_calculateResult == null)
			//{
			//	_calculateResult = new apCalculatedRenderUnit();
			//	_calculateResult.SetMeshGroup(_meshGroup);
			//}

			//_calculateResult.Clear();


			//for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			//{
			//	apModifierParamSet modParamSet = _paramSetList[i];
			//	List<apModifiedMesh> modifiedMeshes = modParamSet._meshData;
			//	for (int iMesh = 0; iMesh < modifiedMeshes.Count; iMesh++)
			//	{
			//		apModifiedMesh modMesh = modifiedMeshes[iMesh];
			//		//if(modMesh._mesh != null)
			//		//{
			//		//	//if(!_calculateResult.IsMeshContain(modMesh._mesh))
			//		//	//{
			//		//	//	_calculateResult.AddMesh(modMesh._mesh);
			//		//	//}
			//		//}
			//	}
			//} 
			#endregion
		}

		public apModifierParamSetGroup GetParamSetGroup(apControlParam keyControlParam)
		{
			return _paramSetGroup_controller.Find(delegate (apModifierParamSetGroup a)
			{
				return a._keyControlParam == keyControlParam;
			});
		}

		public apModifierParamSetGroupAnimPack GetParamSetGroupAnimPack(apAnimClip animClip)
		{
			return _paramSetGroupAnimPacks.Find(delegate (apModifierParamSetGroupAnimPack a)
			{
				return a.LinkedAnimClip == animClip;
			});
		}

		/// <summary>
		/// ParamSetGroup을 LayerIndex에 따라 Sort하고, Index를 넣어준다.
		/// </summary>
		public void SortParamSetGroups()
		{
			_paramSetGroup_controller.Sort(delegate (apModifierParamSetGroup a, apModifierParamSetGroup b)
			{
				return a._layerIndex - b._layerIndex;
			});

			for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			{
				_paramSetGroup_controller[i]._layerIndex = i;
			}
		}

		public int GetNextParamSetLayerIndex()
		{
			int maxIndex = -1;
			for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			{
				if (maxIndex < _paramSetGroup_controller[i]._layerIndex)
				{
					maxIndex = _paramSetGroup_controller[i]._layerIndex;
				}
			}
			return maxIndex + 1;
		}

		public void ChangeParamSetGroupLayerIndex(apModifierParamSetGroup paramSetGroup, int nextIndex)
		{
			bool isIncrese = true;
			if (nextIndex < paramSetGroup._layerIndex)
			{
				isIncrese = false;
			}

			paramSetGroup._layerIndex = nextIndex;

			//해당 Index를 기준으로 작으면 -1, 크면 1을 더한다.
			//해당 Index의 기존 객체에 대해서
			//Increase이면 1 감소,
			//Decrease이면 1 증가
			for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			{
				apModifierParamSetGroup curParamSetGroup = _paramSetGroup_controller[i];

				if (curParamSetGroup == paramSetGroup)
				{
					continue;
				}

				if (curParamSetGroup._layerIndex == nextIndex)
				{
					if (isIncrese)
					{
						curParamSetGroup._layerIndex--;
					}
					else
					{
						curParamSetGroup._layerIndex++;
					}
				}
				else if (curParamSetGroup._layerIndex < nextIndex)
				{
					curParamSetGroup._layerIndex--;
				}
				else
				{
					curParamSetGroup._layerIndex++;
				}

			}

			//그리고 재정렬
			SortParamSetGroups();
		}

		// Get / Set
		//----------------------------------------------
		public virtual MODIFIER_TYPE ModifierType
		{
			get { return MODIFIER_TYPE.Base; }
		}


		public virtual apModifierParamSetGroup.SYNC_TARGET SyncTarget
		{
			get { return apModifierParamSetGroup.SYNC_TARGET.Static; }
		}

		public virtual string DisplayName
		{
			get { return "Base Modifier"; }
		}

		public virtual string DisplayNameShort
		{
			get { return "Base Modifier"; }
		}

		/// <summary>
		/// Calculate 계산시 어떤 값을 사용하는가 (저장과 관련없이 처리 결과만 본다)
		/// </summary>
		public virtual apCalculatedResultParam.CALCULATED_VALUE_TYPE CalculatedValueType
		{
			get { return apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos; }
		}

		/// <summary>
		/// Calculate 계산시 어느 단계에서 값이 처리되는가
		/// </summary>
		public virtual apCalculatedResultParam.CALCULATED_SPACE CalculatedSpace
		{
			get { return apCalculatedResultParam.CALCULATED_SPACE.Object; }
		}

		/// <summary>
		/// Modified Mesh에 저장되는 데이터의 종류 (Calculated 처리 전이므로 범위가 조금 더 넓다)
		/// 중복 처리가 가능하다 (switch 불가)
		/// </summary>
		public virtual apModifiedMesh.MOD_VALUE_TYPE ModifiedValueType
		{
			get { return apModifiedMesh.MOD_VALUE_TYPE.Unknown; }
		}

		//public virtual apModifiedMesh.TARGET_TYPE ModifiedTargetType
		//{
		//	get { return apModifiedMesh.TARGET_TYPE.VertexWithMeshTransform; }
		//}

		public virtual bool IsTarget_MeshTransform { get { return false; } }
		public virtual bool IsTarget_MeshGroupTransform { get { return false; } }
		public virtual bool IsTarget_Bone { get { return false; } }
		public virtual bool IsTarget_ChildMeshTransform { get { return false; } }//<<객체 상관없이 Child MeshTransform에 대해서도 값을 넣을 수 있다.

		public virtual bool IsAnimated { get { return false; } }



		/// <summary>
		/// Update는 RenderUnit 갱신전에 하는 Pre-Update와 Bone Matrix까지 계산한 후에 처리되는 Post-Update가 있다.
		/// 대부분은 PreUpdate이며, Rigging, Physic과 같은 경우엔 Post Update이다.
		/// </summary>
		public virtual bool IsPreUpdate { get { return true; } }


		public bool IsPhysics { get { return (int)(ModifierType & MODIFIER_TYPE.Physic) != 0; } }
		public bool IsVolume { get { return (int)(ModifierType & MODIFIER_TYPE.Volume) != 0; } }
		public virtual bool IsUseParamSetWeight { get { return false; } }

		/// <summary>
		/// ExEdit 중 GeneralEdit 모드에서 "동시에 작업 가능하도록 허용 된 Modifier Type들"을 리턴한다.
		/// 매번 만들지 말고 멤버 변수로 만들어서 넣자
		/// </summary>
		/// <returns></returns>
		public virtual MODIFIER_TYPE[] GetGeneralExEditableModTypes()
		{
			return new MODIFIER_TYPE[] { ModifierType };
		}


		// Find
		//-------------------------------------------------------------
		//public List<apCalculatedResultParam> _calculatedResultParams = new List<apCalculatedResultParam>();
		/// <summary>
		/// CalculateParam을 찾는다.
		/// 적용되는 RenderUnit을 키값으로 삼으며, Bone은 Null인 대상만 고려한다.
		/// </summary>
		/// <param name="targetRenderUnit"></param>
		/// <returns></returns>
		public apCalculatedResultParam GetCalculatedResultParam(apRenderUnit targetRenderUnit)
		{
			return _calculatedResultParams.Find(delegate (apCalculatedResultParam a)
			{
				return a._targetRenderUnit == targetRenderUnit && a._targetBone == null;
			});
		}


		/// <summary>
		/// 추가 : GetCalculatedResultParam 타입의 ModBone 버전.
		/// Bone까지 비교하여 동일한 CalculateResultParam을 찾는다.
		/// </summary>
		/// <param name="targetRenderUnit"></param>
		/// <returns></returns>
		public apCalculatedResultParam GetCalculatedResultParam_Bone(apRenderUnit targetRenderUnit, apBone bone)
		{
			return _calculatedResultParams.Find(delegate (apCalculatedResultParam a)
			{
				return a._targetRenderUnit == targetRenderUnit && a._targetBone == bone;
			});
		}

		// Functions
		//----------------------------------------------

		public virtual void InitCalculate(float tDelta)
		{
			//계산이 불가능한 상황일 때, 계산 값만 초기화한다.

		}

		public virtual void Calculate(float tDelta)
		{
			//TODO
			//ParamSet을 계산한 후
			//Dictionay에 [Vertex / WorldMatrix] 를 만들어 넣는다.
			//
			//_calculateResult.ReadyToCalculate();
			//..오버라이드!
		}




		// Add / Remove
		//----------------------------------------------
		public virtual void AddParamSet()
		{

		}

		public virtual void RemoveParamSet(apModifiedVertex modVertex)
		{

		}


		//----------------------------------------------


		//TODO : Bone

		// 일부 파라미터에만 넣기
		//---------------------------------------------------------------
		/// <summary>
		/// MeshTransform을 해당 ParamSet에 ModMesh의 형태로 넣는다.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="meshTransform"></param>
		/// <param name="targetParamSet"></param>
		/// <param name="isExclusive">meshData리스트에 단 한개만 넣는 경우에는 True</param>
		/// <param name="isRecursiveAvailable">True이면 해당 MeshGroup이 아닌 하위 MeshGroup의 Transform을 허용한다. 기본값은 false</param>
		/// <returns></returns>
		public apModifiedMesh AddMeshTransform(apMeshGroup meshGroup, apTransform_Mesh meshTransform, apModifierParamSet targetParamSet,
												bool isExclusive = false, bool isRecursiveAvailable = false, bool isRefreshLink = true)
		{
			//현재 타입에서 추가 가능한가.
			if (!IsTarget_MeshTransform)
			{
				return null;
			}
			//if (ModifiedTargetType != apModifiedMesh.TARGET_TYPE.VertexWithMeshTransform &&
			//	ModifiedTargetType != apModifiedMesh.TARGET_TYPE.MeshTransformOnly)
			//{
			//	return null;
			//}

			apMeshGroup parentMeshGroupOfTransform = null;


			if (isRecursiveAvailable)
			{
				if (meshGroup._childMeshTransforms.Contains(meshTransform))
				{
					//이 MeshGroup에 포함된다면
					parentMeshGroupOfTransform = meshGroup;
				}
				else
				{
					//그렇지 않다면 모든 MeshGroup을 기준으로 검색하자
					for (int i = 0; i < _portrait._meshGroups.Count; i++)
					{
						if (_portrait._meshGroups[i]._childMeshTransforms.Contains(meshTransform))
						{
							//찾았다!
							parentMeshGroupOfTransform = _portrait._meshGroups[i];
							break;
						}
					}
				}

				if (parentMeshGroupOfTransform == null)
				{
					//못찾았다..
					return null;
				}
			}
			else
			{
				//Recursive한 Transform 접근을 허용하지 않느다.

				//MeshGroup이 Mesh Transform을 가지고 있지 않으면 실패
				if (!meshGroup._childMeshTransforms.Contains(meshTransform))
				{
					return null;
				}

				parentMeshGroupOfTransform = meshGroup;
			}

			if (meshTransform._mesh == null)
			{
				return null;
			}

			apRenderUnit renderUnit = null;

			//Child MeshTransform을 허용하늗가
			if (IsTarget_ChildMeshTransform)
			{
				//재귀적으로 모든 Child MeshTransform을 허용한다.
				renderUnit = meshGroup.GetRenderUnit(meshTransform);
			}
			else
			{
				//현재 MeshGroup의 MeshTransform만 허용한다.
				renderUnit = meshGroup.GetRenderUnit_NoRecursive(meshTransform);
			}

			if (renderUnit == null)
			{
				return null;
			}

			apModifiedMesh modMesh = targetParamSet._meshData.Find(delegate (apModifiedMesh a)
				{
					return a.IsContains_MeshTransform(meshGroup, meshTransform, meshTransform._mesh);
				});


			if (modMesh == null)
			{
				//Debug.Log("Add Mod Mesh - MeshTransform");

				modMesh = new apModifiedMesh();

				modMesh.Init(meshGroup._uniqueID, parentMeshGroupOfTransform._uniqueID, ModifiedValueType);

				if (IsTarget_MeshTransform)
				{
					modMesh.SetTarget_MeshTransform(meshTransform._transformUniqueID, meshTransform._mesh._uniqueID, meshTransform._meshColor2X_Default, meshTransform._isVisible_Default);
					modMesh.Link_MeshTransform(meshGroup, parentMeshGroupOfTransform, meshTransform, renderUnit, _portrait);
				}


				targetParamSet._meshData.Add(modMesh);
			}

			if (isExclusive)
			{
				//MeshTransform에 해당하지 않는 ModMesh는 아예 삭제한다.
				int nRemoved = targetParamSet._meshData.RemoveAll(delegate (apModifiedMesh a)
				{
					return a._transform_Mesh != meshTransform;
				});

				if (nRemoved > 0)
				{
					//Debug.LogError("ModMesh Removed (Exclusive/MeshTransform) : " + nRemoved + "[" + DisplayName + "]");
				}
			}

			if (isRefreshLink)
			{
				_meshGroup.RefreshModifierLink();
			}


			return modMesh;
		}

		/// <summary>
		/// MeshGroupTransform을 해당 ParamSet에 ModMesh의 형태로 넣는다.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="meshGroupTransform"></param>
		/// <param name="targetParamSet"></param>
		/// <param name="isExclusive">meshData리스트에 단 한개만 넣는 경우에는 True</param>
		/// <param name="isRecursiveAvailable">True이면 해당 MeshGroup이 아닌 하위 MeshGroup의 Transform을 허용한다. 기본값은 false</param>
		/// <returns></returns>
		public apModifiedMesh AddMeshGroupTransform(apMeshGroup meshGroup, apTransform_MeshGroup meshGroupTransform, apModifierParamSet targetParamSet,
													bool isExclusive = false, bool isRecursiveAvailable = false, bool isRefreshLink = true)
		{
			//현재 타입에서 추가 가능한가.
			if (!IsTarget_MeshGroupTransform)
			{
				return null;
			}
			//if(ModifiedTargetType != apModifiedMesh.TARGET_TYPE.MeshGroupTransformOnly)
			//{
			//	return null;
			//}



			apMeshGroup parentMeshGroupOfTransform = null;


			if (isRecursiveAvailable)
			{
				if (meshGroup._childMeshGroupTransforms.Contains(meshGroupTransform))
				{
					//이 MeshGroup에 포함된다면
					parentMeshGroupOfTransform = meshGroup;
				}
				else
				{
					//그렇지 않다면 모든 MeshGroup을 기준으로 검색하자
					for (int i = 0; i < _portrait._meshGroups.Count; i++)
					{
						if (_portrait._meshGroups[i]._childMeshGroupTransforms.Contains(meshGroupTransform))
						{
							//찾았다!
							parentMeshGroupOfTransform = _portrait._meshGroups[i];
							break;
						}
					}
				}

				if (parentMeshGroupOfTransform == null)
				{
					//못찾았다..
					return null;
				}
			}
			else
			{
				//Recursive한 Transform 접근을 허용하지 않느다.

				//MeshGroup이 Mesh Group Transform을 가지고 있지 않으면 실패
				if (!meshGroup._childMeshGroupTransforms.Contains(meshGroupTransform))
				{
					return null;
				}

				parentMeshGroupOfTransform = meshGroup;
			}



			apRenderUnit renderUnit = meshGroup.GetRenderUnit(meshGroupTransform);
			if (renderUnit == null)
			{
				return null;
			}

			apModifiedMesh modMesh = targetParamSet._meshData.Find(delegate (apModifiedMesh a)
			{
				return a.IsContains_MeshGroupTransform(meshGroup, meshGroupTransform);
			});

			if (modMesh == null)
			{
				//Debug.Log("Add Mod Mesh - MeshGroupTransform");

				modMesh = new apModifiedMesh();

				modMesh.Init(meshGroup._uniqueID, parentMeshGroupOfTransform._uniqueID, ModifiedValueType);

				modMesh.SetTarget_MeshGroupTransform(meshGroupTransform._transformUniqueID, meshGroupTransform._meshColor2X_Default, meshGroupTransform._isVisible_Default);

				//modMesh.Init_MeshGroupTransform(meshGroup._uniqueID,
				//								meshGroupTransform._transformUniqueID);

				modMesh.Link_MeshGroupTransform(meshGroup, parentMeshGroupOfTransform, meshGroupTransform, renderUnit);

				targetParamSet._meshData.Add(modMesh);
			}

			if (isExclusive)
			{
				//MeshTransform에 해당하지 않는 ModMesh는 아예 삭제한다.
				int nRemoved = targetParamSet._meshData.RemoveAll(delegate (apModifiedMesh a)
				{
					return a._transform_MeshGroup != meshGroupTransform;
				});

				if (nRemoved > 0)
				{
					//Debug.LogError("ModMesh Removed (Exclusive/MeshGroupTransform) : " + nRemoved + "[" + DisplayName + "]");
				}
			}

			if (isRefreshLink)
			{
				_meshGroup.RefreshModifierLink();
			}


			return modMesh;
		}



		public apModifiedBone AddBone(apBone bone, apModifierParamSet targetParamSet,
													bool isRecursiveAvailable = false, bool isRefreshLink = true)
		{
			if (!IsTarget_Bone)
			{
				return null;
			}

			if (bone == null)
			{
				return null;
			}


			apMeshGroup meshGroupOfBone = bone._meshGroup;
			//해당 MeshGroup이 Modifier의 MeshGroup에 포함되는가
			apMeshGroup meshGroupOfModifier = _meshGroup;

			if (meshGroupOfBone == null)
			{
				Debug.LogError("AnyPortrait : AddBone Failed : MeshGroup [" + (meshGroupOfBone != null) + "]");
				return null;
			}
			apTransform_MeshGroup meshGroupTransform = null;
			if (meshGroupOfBone == meshGroupOfModifier)
			{
				meshGroupTransform = meshGroupOfModifier._rootMeshGroupTransform;
			}
			else
			{
				meshGroupTransform = meshGroupOfModifier.FindChildMeshGroupTransform(meshGroupOfBone);
			}
			if (meshGroupTransform == null)
			{
				Debug.LogError("AnyPortrait : AddBone Failed : MeshGroupTF [" + (meshGroupTransform != null) + "] <" + meshGroupOfBone._name + " : " + meshGroupOfModifier._name + ">");
				return null;
			}

			apRenderUnit renderUnit = meshGroupOfModifier.GetRenderUnit(meshGroupTransform);
			if (renderUnit == null)
			{
				Debug.LogError("AnyPortrait : AddBone Failed : No Render Unit");
				return null;
			}

			//이미 존재하는지 확인
			apModifiedBone modBone = targetParamSet._boneData.Find(delegate (apModifiedBone a)
			{
				return a._boneID == bone._uniqueID;
			});

			if (modBone == null)
			{
				modBone = new apModifiedBone();
				modBone.Init(meshGroupOfModifier._uniqueID, meshGroupOfBone._uniqueID, meshGroupTransform._transformUniqueID, bone);
				modBone.Link(meshGroupOfModifier, meshGroupOfBone, bone, renderUnit, meshGroupTransform);

				targetParamSet._boneData.Add(modBone);
			}

			if (isRefreshLink)
			{
				_meshGroup.RefreshModifierLink();
			}

			return modBone;

		}
		//--------------------------------------------------------------------------------
		// Modifier에서 Calculate 함수에서 사용할 수 있는 공통 패턴
		//--------------------------------------------------------------------------------

		protected void CalculatePattern_Morph(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}


			apCalculatedResultParam calParam = null;
			Vector2[] posList = null;
			Vector2[] tmpPosList = null;
			List<apCalculatedResultParamSubList> subParamGroupList = null;
			List<apCalculatedResultParam.ParamKeyValueSet> subParamKeyValueList = null;
			float layerWeight = 0.0f;
			apModifierParamSetGroup keyParamSetGroup = null;
			apModifierParamSetGroupVertWeight weightedVertData = null;
			apCalculatedResultParamSubList curSubList = null;
			int nParamKeys = 0;
			apCalculatedResultParam.ParamKeyValueSet paramKeyValue = null;

			//색상을 지원하는 Modifier인가
			bool isColorProperty = _isColorPropertyEnabled && (int)((CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color)) != 0;

			//ParamSetWeight를 사용하는가
			bool isUseParamSetWeight = IsUseParamSetWeight;

			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				calParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크


				// 중요!
				//-------------------------------------------------------
				//1. Param Weight Calculate
				calParam.Calculate();
				//-------------------------------------------------------

				//>>> LinkedMatrix를 초기화
				//calParam.CalculatedLog.ReadyToRecord();//<<<<<<


				//추가 : 색상 처리 초기화
				calParam._isColorCalculated = false;


				posList = calParam._result_Positions;
				//tmpPosList = calParam._tmp_Positions;
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

				if (isColorProperty)
				{
					calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					calParam._result_IsVisible = false;//Alpha와 달리 Visible 값은 false -> OR 연산으로 작동한다.
				}
				else
				{
					calParam._result_IsVisible = true;
				}


				Color tmpColor = Color.clear;
				bool tmpVisible = false;

				int iCalculatedSubParam = 0;

				int iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				bool tmpIsColoredKeyParamSetGroup = false;

				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					if (curSubList._keyParamSetGroup == null ||
						!curSubList._keyParamSetGroup.IsCalculateEnabled)
					{
						//Debug.LogError("Modifier Cal Param Failed : " + DisplayName + " / " + calParam._linkedModifier.DisplayName);
						continue;
					}

					//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
					nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;



					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;

					
					tmpPosList = keyParamSetGroup._tmpPositions;


					//>>> LinkedMatrix
					//keyParamSetGroup.CalculatedLog.ReadyToRecord();//<<<<<<

					//추가 3.22
					//Transfrom / Color Update 여부를 따로 결정한다.
					bool isExCalculatable_Transform = curSubList._keyParamSetGroup.IsExCalculatable_Transform;
					bool isExCalculatable_Color = curSubList._keyParamSetGroup.IsExCalculatable_Color;
					

					//Vector2 calculatedValue = Vector2.zero;

					bool isFirstParam = true;

					//레이어 내부의 임시 데이터를 먼저 초기화
					if (tmpPosList == null ||
						tmpPosList.Length != posList.Length)
					{
						keyParamSetGroup._tmpPositions = new Vector2[posList.Length];
						tmpPosList = keyParamSetGroup._tmpPositions;

						for (int iPos = 0; iPos < posList.Length; iPos++)
						{
							tmpPosList[iPos] = Vector2.zero;
						}
					}
					else
					{
						for (int iPos = 0; iPos < posList.Length; iPos++)
						{
							tmpPosList[iPos] = Vector2.zero;
						}
					}

					tmpColor = Color.clear;
					tmpVisible = false;


					//if (isDebuggable)
					//{
					//	Debug.Log("--------------------------------------------");
					//	Debug.Log("[" + iCalParam + "] Params : " + nParamKeys);
					//	Debug.Log("--------------------------------------------");
					//}

					float totalParamSetWeight = 0.0f;
					int nCalculated = 0;


					//KeyParamSetGroup이 Color를 지원하는지 체크
					//tmpIsColoredKeyParamSetGroup = isColorProperty && keyParamSetGroup._isColorPropertyEnabled;
					tmpIsColoredKeyParamSetGroup = isColorProperty && keyParamSetGroup._isColorPropertyEnabled && isExCalculatable_Color; //<<추가

					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.
					for (int iPV = 0; iPV < nParamKeys; iPV++)
					{
						paramKeyValue = subParamKeyValueList[iPV];

						//>>>> Cal Log 초기화
						//paramKeyValue._modifiedMesh.CalculatedLog.ReadyToRecord();

						if (!paramKeyValue._isCalculated)
						{ continue; }


						totalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;



						//---------------------------- Pos List
						if (isExCalculatable_Transform)//<<추가
						{
							for (int iPos = 0; iPos < posList.Length; iPos++)
							{
								tmpPosList[iPos] += paramKeyValue._modifiedMesh._vertices[iPos]._deltaPos * paramKeyValue._weight;
							}
						}
						//---------------------------- Pos List

						//>>>> LinkedMatrix를 만들어서 GizmoEdit를 할 수 있게 만들자
						//paramKeyValue._modifiedMesh.CalculatedLog.CalculateModified(paramKeyValue._weight, keyParamSetGroup.CalculatedLog);


						if (isFirstParam)
						{
							isFirstParam = false;
						}

						if (tmpIsColoredKeyParamSetGroup)
						{
							if (paramKeyValue._modifiedMesh._isVisible)
							{
								tmpColor += paramKeyValue._modifiedMesh._meshColor * paramKeyValue._weight;
								tmpVisible = true;//하나라도 Visible이면 Visible이 된다.
							}
							else
							{
								//Visible이 False
								Color paramColor = paramKeyValue._modifiedMesh._meshColor;
								paramColor.a = 0.0f;
								tmpColor += paramColor * paramKeyValue._weight;
							}
						}
						nCalculated++;//Visible 계산을 위해 "paramKey 계산 횟수"를 카운트하자

					}//--- Params


					//이제 tmp값을 Result에 넘겨주자
					//처음 Layer라면 -> 100% 적용
					//그렇지 않다면 Blend를 해주자
					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.

					if (!isUseParamSetWeight)
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);
					}
					else
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight * Mathf.Clamp01(totalParamSetWeight));
					}


					//>>> Linked Matrix < KeyParamSetGroup >
					//keyParamSetGroup.LinkedMatrix.SetPassAndMerge(apLinkedMatrix.VALUE_TYPE.VertPos).SetWeight(layerWeight);


					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.
					// Transform과 Color를 나눔
					if(isExCalculatable_Transform)
					{
						calParam._totalParamSetGroupWeight_Transform += layerWeight;
					}
					if(isExCalculatable_Color)
					{
						calParam._totalParamSetGroupWeight_Color += layerWeight;
					}
					

					if (nCalculated == 0)
					{
						tmpVisible = true;
					}

					//if (keyParamSetGroup._layerIndex == 0)
					if (iCalculatedSubParam == 0)//<<변경
					{
						if (isExCalculatable_Transform)//<<추가
						{
							for (int iPos = 0; iPos < posList.Length; iPos++)
							{
								posList[iPos] = tmpPosList[iPos] * layerWeight;
							}
						}
						
						//아래 코드로 옮김
						//if (isColorProperty)
						//{
						//	calParam._result_Color = apUtil.BlendColor_ITP(calParam._result_Color, tmpColor, layerWeight);
						//	//Debug.Log("CP-Morph 0 Color : " + tmpColor + " > " + calParam._result_Color);
						//	calParam._result_IsVisible |= tmpVisible;
						//}
					}
					else
					{
						switch (keyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									if (isExCalculatable_Transform)//<<추가
									{
										if (weightedVertData != null)
										{
											//Vertex 가중치가 추가되었다.
											float vertWeight = 0.0f;
											for (int iPos = 0; iPos < posList.Length; iPos++)
											{
												vertWeight = layerWeight * weightedVertData._weightedVerts[iPos]._adaptWeight;

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


									//if (isColorProperty)
									//{
									//	calParam._result_Color = apUtil.BlendColor_Add(calParam._result_Color, tmpColor, layerWeight);
									//	//Debug.Log("CP-Morph Add Color : " + tmpColor + " > " + calParam._result_Color + "( " + layerWeight + " )");
									//	calParam._result_IsVisible |= tmpVisible;
									//}
								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									if (isExCalculatable_Transform)//<<추가
									{
										if (weightedVertData != null)
										{
											//Vertex 가중치가 추가되었다.
											float vertWeight = 0.0f;
											for (int iPos = 0; iPos < posList.Length; iPos++)
											{
												vertWeight = layerWeight * weightedVertData._weightedVerts[iPos]._adaptWeight;

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
									//if (isColorProperty)
									//{
									//	calParam._result_Color = apUtil.BlendColor_ITP(calParam._result_Color, tmpColor, layerWeight);
									//	//Debug.Log("CP-Morph ITP Color : " + tmpColor + " > " + calParam._result_Color + "( " + layerWeight + " )");
									//	calParam._result_IsVisible |= tmpVisible;
									//}
								}
								break;

							default:
								Debug.LogError("Mod-Morph : Unknown BLEND_METHOD : " + keyParamSetGroup._blendMethod);
								break;
						}

						//>>> CalculatedLog
						//keyParamSetGroup.CalculatedLog.CalculateParamSetGroup(
						//	layerWeight,
						//	iCalculatedSubParam,
						//	keyParamSetGroup._blendMethod,
						//	weightedVertData,
						//	calParam.CalculatedLog);
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

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)


				//? 처리된게 하나도 없어요?
				if (iCalculatedSubParam == 0)
				{
					//Active를 False로 날린다.
					calParam._isAvailable = false;
				}
				else
				{
					calParam._isAvailable = true;
				}


				
			}
		}




		protected void CalculatePattern_Transform(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			bool isBoneTarget = false;//Bone을 대상으로 하는가 (Bone 대상이면 ModBone을 사용해야한다)
			apCalculatedResultParam calParam = null;

			//색상을 지원하는 Modifier인가
			bool isColorProperty = _isColorPropertyEnabled && (int)((CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color)) != 0;

			//ParamSetWeight를 사용하는가
			bool isUseParamSetWeight = IsUseParamSetWeight;


			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				calParam = _calculatedResultParams[iCalParam];
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

				//Sub List를 돌면서 Weight 체크

				//----------------------------------------------
				//1. 계산!
				calParam.Calculate();
				//----------------------------------------------



				//>>> LinkedMatrix를 초기화
				//calParam.CalculatedLog.ReadyToRecord();


				List<apCalculatedResultParamSubList> subParamGroupList = calParam._subParamKeyValueList;
				List<apCalculatedResultParam.ParamKeyValueSet> subParamKeyValueList = null;
				apModifierParamSetGroup keyParamSetGroup = null;

				//결과 매트릭스를 만들자
				//apMatrix resultMatrix = new apMatrix();
				calParam._result_Matrix.SetIdentity();

				//추가 : 색상 처리 초기화
				calParam._isColorCalculated = false;

				if (!isBoneTarget)
				{
					if (isColorProperty)
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
				}

				//apMatrix tmpMatrix = new apMatrix();
				apMatrix tmpMatrix = null;
				Color tmpColor = Color.clear;
				bool tmpVisible = false;
				float layerWeight = 0.0f;

				int iCalculatedSubParam = 0;


				int iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				bool tmpIsColoredKeyParamSetGroup = false;

				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					apCalculatedResultParamSubList curSubList = subParamGroupList[iSubList];

					if (curSubList._keyParamSetGroup == null ||
						!curSubList._keyParamSetGroup.IsCalculateEnabled)
					{
						continue;
					}


					//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
					int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;

					apCalculatedResultParam.ParamKeyValueSet paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;



					//>>> LinkedMatrix
					//keyParamSetGroup.CalculatedLog.ReadyToRecord();//<<<<<<

					tmpMatrix = keyParamSetGroup._tmpMatrix;

					//추가 3.22
					//Transfrom / Color Update 여부를 따로 결정한다.
					bool isExCalculatable_Transform = curSubList._keyParamSetGroup.IsExCalculatable_Transform;
					bool isExCalculatable_Color = curSubList._keyParamSetGroup.IsExCalculatable_Color;

					
					bool isFirstParam = true;

					//레이어 내부의 임시 데이터를 먼저 초기화
					tmpMatrix.SetZero();
					tmpColor = Color.clear;
					tmpVisible = false;

					layerWeight = 0.0f;

					float totalParamSetWeight = 0.0f;
					int nCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					//tmpIsColoredKeyParamSetGroup = isColorProperty && keyParamSetGroup._isColorPropertyEnabled && !isBoneTarget;
					tmpIsColoredKeyParamSetGroup = isColorProperty && keyParamSetGroup._isColorPropertyEnabled && !isBoneTarget && isExCalculatable_Color;


					if (!isBoneTarget)
					{
						//ModMesh를 활용하는 타입인 경우
						for (int iPV = 0; iPV < nParamKeys; iPV++)
						{
							//paramKeyValue = calParam._paramKeyValues[iPV];
							paramKeyValue = subParamKeyValueList[iPV];
							//layerWeight = Mathf.Clamp01(paramKeyValue._keyParamSetGroup._layerWeight);

							//>>>> Cal Log 초기화
							//paramKeyValue._modifiedMesh.CalculatedLog.ReadyToRecord();

							if (!paramKeyValue._isCalculated)
							{ continue; }

							//ParamSetWeight를 추가
							totalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;


							if (isExCalculatable_Transform)//<<추가
							{
								//Weight에 맞게 Matrix를 만들자

								if (paramKeyValue._isAnimRotationBias)
								{
									//추가 : RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
									tmpMatrix.AddMatrix(paramKeyValue.AnimRotationBiasedMatrix, paramKeyValue._weight, false);
								}
								else
								{
									//기본 식
									tmpMatrix.AddMatrix(paramKeyValue._modifiedMesh._transformMatrix, paramKeyValue._weight, false);
								}
							}


							//>>>> LinkedMatrix를 만들어서 GizmoEdit를 할 수 있게 만들자
							//paramKeyValue._modifiedMesh.CalculatedLog.CalculateModified(paramKeyValue._weight, keyParamSetGroup.CalculatedLog);


							//Modifier + KeyParamSetGroup 모두 Color를 지원해야함
							if (tmpIsColoredKeyParamSetGroup)
							{
								if (paramKeyValue._modifiedMesh._isVisible)
								{
									tmpColor += paramKeyValue._modifiedMesh._meshColor * paramKeyValue._weight;
									tmpVisible = true;
								}
								else
								{
									//Visible이 False
									Color paramColor = paramKeyValue._modifiedMesh._meshColor;
									paramColor.a = 0.0f;
									tmpColor += paramColor * paramKeyValue._weight;
								}
							}

							if (isFirstParam)
							{
								isFirstParam = false;
							}
							nCalculated++;//Visible 계산을 위해 "ParamKey 계산 횟수"를 카운트하자
						}
					}
					else
					{
						//ModBone을 활용하는 타입인 경우
						for (int iPV = 0; iPV < nParamKeys; iPV++)
						{
							//paramKeyValue = calParam._paramKeyValues[iPV];
							paramKeyValue = subParamKeyValueList[iPV];
							//layerWeight = Mathf.Clamp01(paramKeyValue._keyParamSetGroup._layerWeight);

							if (!paramKeyValue._isCalculated)
							{ continue; }

							//ParamSetWeight를 추가
							totalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;


							//Weight에 맞게 Matrix를 만들자
							if (isExCalculatable_Transform)
							{
								if (paramKeyValue._isAnimRotationBias)
								{
									//추가 : RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
									tmpMatrix.AddMatrix(paramKeyValue.AnimRotationBiasedMatrix, paramKeyValue._weight, false);
								}
								else
								{
									tmpMatrix.AddMatrix(paramKeyValue._modifiedBone._transformMatrix, paramKeyValue._weight, false);
								}
							}

							//TODO : ModBone도 CalculateLog를 기록해야하나..


							if (isFirstParam)
							{
								isFirstParam = false;
							}
							nCalculated++;//Visible 계산을 위해 "ParamKey 계산 횟수"를 카운트하자
						}
					}

					//이제 레이어순서에 따른 보간을 해주자
					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.

					if (!isUseParamSetWeight)
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);
					}
					else
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight * Mathf.Clamp01(totalParamSetWeight));
					}


					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.
					// Transform과 Color를 나눔
					if(isExCalculatable_Transform)
					{
						calParam._totalParamSetGroupWeight_Transform += layerWeight;
					}
					if(isExCalculatable_Color)
					{
						calParam._totalParamSetGroupWeight_Color += layerWeight;
					}



					if ((nCalculated == 0 && isColorProperty) || isBoneTarget)
					{
						tmpVisible = true;
					}


					//if (keyParamSetGroup._layerIndex == 0)
					if (iCalculatedSubParam == 0)
					{
						if (isExCalculatable_Transform)//<<추가
						{
							//calParam._result_Matrix.SetMatrix(tmpMatrix);
							calParam._result_Matrix.SetPos(tmpMatrix._pos * layerWeight);
							calParam._result_Matrix.SetRotate(tmpMatrix._angleDeg * layerWeight);
							calParam._result_Matrix.SetScale(tmpMatrix._scale * layerWeight + Vector2.one * (1.0f - layerWeight));
							calParam._result_Matrix.MakeMatrix();
						}

						//>>> CalculatedLog
						//keyParamSetGroup.CalculatedLog.CalculateParamSetGroup(layerWeight,
						//														iCalculatedSubParam,
						//														apModifierParamSetGroup.BLEND_METHOD.Interpolation,
						//														null,
						//														calParam.CalculatedLog);

						//아래 코드로 옮김
						//if (isColorProperty && tmpIsColoredKeyParamSetGroup)
						//{
						//	calParam._result_Color = apUtil.BlendColor_ITP(calParam._result_Color, tmpColor, layerWeight);
						//	calParam._result_IsVisible |= tmpVisible;
						//}
					}
					else
					{
						switch (keyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									if (isExCalculatable_Transform)//<<추가
									{
										calParam._result_Matrix.AddMatrix(tmpMatrix, layerWeight, true);
									}
									//if (isColorProperty)
									//{
									//	calParam._result_Color = apUtil.BlendColor_Add(calParam._result_Color, tmpColor, layerWeight);
									//	calParam._result_IsVisible |= tmpVisible;
									//}
								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									if (isExCalculatable_Transform)//<<추가
									{
										calParam._result_Matrix.LerpMartix(tmpMatrix, layerWeight);
									}
									//if (isColorProperty)
									//{
									//	calParam._result_Color = apUtil.BlendColor_ITP(calParam._result_Color, tmpColor, layerWeight);
									//	calParam._result_IsVisible |= tmpVisible;
									//}
								}
								break;

							default:
								Debug.LogError("Mod-Morph : Unknown BLEND_METHOD : " + keyParamSetGroup._blendMethod);
								break;
						}

						//>>> CalculatedLog
						//keyParamSetGroup.CalculatedLog.CalculateParamSetGroup(layerWeight,
						//														iCalculatedSubParam,
						//														keyParamSetGroup._blendMethod,
						//														null,
						//														calParam.CalculatedLog);
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

				//? 처리된게 하나도 없어요?
				if (iCalculatedSubParam == 0)
				{
					//Active를 False로 날린다.
					calParam._isAvailable = false;
				}
				else
				{
					calParam._isAvailable = true;
					calParam._result_Matrix.MakeMatrix();
				}

			}
		}

		protected void CalculatePattern_Rigging(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				//Debug.LogError("Result Param Count : 0");
				return;
			}

			//Profiler.BeginSample("Rigging Calculate");

			apCalculatedResultParam calParam = null;
			Vector2[] posList = null;
			Vector2[] tmpPosList = null;

			//Pos대신 Matrix
			apMatrix3x3[] vertMatrixList = null;
			apMatrix3x3[] tmpVertMatrixList = null;

			List<apCalculatedResultParamSubList> subParamGroupList = null;
			List<apCalculatedResultParam.ParamKeyValueSet> subParamKeyValueList = null;
			float layerWeight = 0.0f;
			apModifierParamSetGroup keyParamSetGroup = null;
			//apModifierParamSetGroupVertWeight weigetedVertData = null;
			apCalculatedResultParamSubList curSubList = null;
			int nParamKeys = 0;
			apCalculatedResultParam.ParamKeyValueSet paramKeyValue = null;


			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				//Profiler.BeginSample("1. Basic Calculate");

				calParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크

				// 중요! -> Static은 Weight 계산이 필요없어염
				//-------------------------------------------------------
				//1. Param Weight Calculate
				calParam.Calculate();
				//-------------------------------------------------------

				//Profiler.EndSample();

				//Profiler.BeginSample("2. Record Log");

				//>>> LinkedMatrix를 초기화
				//calParam.CalculatedLog.ReadyToRecord();//<<<<<<

				//Profiler.EndSample();


				//Profiler.BeginSample("3. Init");

				posList = calParam._result_Positions;
				vertMatrixList = calParam._result_VertMatrices;
			
				//tmpPosList = calParam._tmp_Positions;
				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				layerWeight = 0.0f;

				//일단 초기화
				for (int iPos = 0; iPos < posList.Length; iPos++)
				{
					posList[iPos] = Vector2.zero;
				}

				calParam._result_IsVisible = true;


				Color tmpColor = Color.clear;
				//bool tmpVisible = false;

				int iCalculatedSubParam = 0;

				//Profiler.EndSample();

				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{

					//Profiler.BeginSample("4. ParamSetGroup Calculate");

					curSubList = subParamGroupList[iSubList];

					nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;


					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;//<<

					//>>> LinkedMatrix
					//keyParamSetGroup.CalculatedLog.ReadyToRecord();//<<<<<<

					//Profiler.BeginSample("4-1. Tmp Pos List Init");

					//레이어 내부의 임시 데이터를 먼저 초기화
					tmpPosList = keyParamSetGroup._tmpPositions;
					tmpVertMatrixList = keyParamSetGroup._tmpVertMatrices;

					//리깅은 Ex 편집이 아예 없다.

					if (tmpPosList == null ||
						tmpVertMatrixList == null ||
						tmpPosList.Length != posList.Length || 
						tmpVertMatrixList.Length != vertMatrixList.Length)
					{
						keyParamSetGroup._tmpPositions = new Vector2[posList.Length];
						keyParamSetGroup._tmpVertMatrices = new apMatrix3x3[vertMatrixList.Length];

						tmpPosList = keyParamSetGroup._tmpPositions;
						tmpVertMatrixList = keyParamSetGroup._tmpVertMatrices;

						for (int iPos = 0; iPos < posList.Length; iPos++)
						{
							tmpPosList[iPos] = Vector2.zero;
							tmpVertMatrixList[iPos] = apMatrix3x3.zero3x2;
						}
					}
					else
					{
						for (int iPos = 0; iPos < posList.Length; iPos++)
						{
							tmpPosList[iPos] = Vector2.zero;
							tmpVertMatrixList[iPos] = apMatrix3x3.zero3x2;
						}
					}

					//Profiler.EndSample();


					tmpColor = Color.clear;
					//tmpVisible = false;

					float totalWeight = 0.0f;
					int nCalculated = 0;


					//Profiler.BeginSample("4-2. ParamKey Calculate");

					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.
					for (int iPV = 0; iPV < nParamKeys; iPV++)
					{
						paramKeyValue = subParamKeyValueList[iPV];

						//>>>> Cal Log 초기화
						//paramKeyValue._modifiedMesh.CalculatedLog.ReadyToRecord();

						paramKeyValue._weight = 1.0f;

						totalWeight += paramKeyValue._weight;

						//Modified가 안된 Vert World Pos + Bone의 Modified 안된 World Matrix + Bone의 World Matrix (변형됨) 순으로 계산한다.
						apMatrix3x3 matx_Vert2Local = paramKeyValue._modifiedMesh._renderUnit._meshTransform._mesh.Matrix_VertToLocal;
						apMatrix3x3 matx_Vert2Local_Inv = matx_Vert2Local.inverse;
						apMatrix matx_MeshW_NoMod = paramKeyValue._modifiedMesh._renderUnit._meshTransform._matrix_TFResult_WorldWithoutMod;
						//string modMeshName = paramKeyValue._modifiedMesh._renderUnit._meshTransform._nickName;

						//Profiler.BeginSample("4-2-1. Vert Pos Calculate");

						//---------------------------- Pos List

						for (int iPos = 0; iPos < posList.Length; iPos++)
						{
							//1. Mod가 적용안된 Vert의 World Pos
							apModifiedVertexRig vertRig = paramKeyValue._modifiedMesh._vertRigs[iPos];
							Vector2 vertPosW_NoMod = matx_MeshW_NoMod.MulPoint2(matx_Vert2Local.MultiplyPoint(vertRig._vertex._pos));


							//2. Bone의 (Mod가 적용 안된) World Matrix의 역행렬을 계산하여 Local Vert by Bone을 만든다.
							//3. Bone의 World Matrix를 계산하여 연산한다.
							float totalBoneWeight = 0.0f;
							apModifiedVertexRig.WeightPair weightPair = null;
							apMatrix matx_boneWorld_Mod = null;
							apMatrix matx_boneWorld_Default = null;
							Vector2 vertPos_BoneLocal;
							Vector2 vertPosW_BoneWorld;
							//Vector2 vertPos_OnlyReverseMesh;
							Vector2 vertPosL_Result;

							//수정 : Rigging을 vertPos가 아닌 Matrix의 합으로 계산한다.
							apMatrix3x3 matx_Result = apMatrix3x3.identity;
							 
							for (int iWeight = 0; iWeight < vertRig._weightPairs.Count; iWeight++)
							{
								weightPair = vertRig._weightPairs[iWeight];

								if (weightPair._weight <= 0.0001f)
								{
									continue;
								}

								//Profiler.BeginSample("4-2-1-1. Matrix Calculate");

								matx_boneWorld_Mod = weightPair._bone._worldMatrix;

								matx_boneWorld_Default = weightPair._bone._worldMatrix_NonModified;

								//World -> Bone Local
								vertPos_BoneLocal = matx_boneWorld_Default.InvMulPoint2(vertPosW_NoMod);

								//Bone Local -> World
								vertPosW_BoneWorld = matx_boneWorld_Mod.MulPoint2(vertPos_BoneLocal);

								//vertPos_OnlyReverseMesh = matx_Vert2Local_Inv.MultiplyPoint(matx_MeshW_NoMod.InvMulPoint2(vertPosW_NoMod));

								//다시 이것의 Local Pos를 구한다.
								vertPosL_Result = matx_Vert2Local_Inv.MultiplyPoint(matx_MeshW_NoMod.InvMulPoint2(vertPosW_BoneWorld));

								
								//<Vert2Local> 단계를 제외한 Bone matrix 계산식
								matx_Result = matx_MeshW_NoMod.MtrxToLowerSpace
									* matx_boneWorld_Mod.MtrxToSpace
									* matx_boneWorld_Default.MtrxToLowerSpace
									* matx_MeshW_NoMod.MtrxToSpace
									;


								//Vert에 저장하는 방식
								tmpPosList[iPos] += new Vector2(vertPosL_Result.x, vertPosL_Result.y) * weightPair._weight;
								
								//Matrix에 저장하는 방식
								tmpVertMatrixList[iPos] += matx_Result * weightPair._weight;
								//if(iPos == 0)
								//{
								//	Debug.Log("Matx Result (" + iWeight + " /  " + weightPair._weight + ") \n" + matx_Result.ToString() + "\n>>\n" + tmpVertMatrixList[iPos].ToString());
								//}

								//Profiler.EndSample();

								totalBoneWeight += weightPair._weight;
							}

							//if(iPos == 0)
							//{
							//	Debug.Log("Rigging Mod Tmp (" +  totalBoneWeight + " )\n" + tmpVertMatrixList[iPos].ToString());
							//}

							if (totalBoneWeight > 0.0f)
							{
								tmpPosList[iPos] = new Vector2(tmpPosList[iPos].x / totalBoneWeight, tmpPosList[iPos].y / totalBoneWeight);
								tmpVertMatrixList[iPos] /= totalBoneWeight;
							}
							else
							{
								//Bone Weight가 지정되지 않았을 때
								tmpPosList[iPos] = vertRig._vertex._pos;
								tmpVertMatrixList[iPos].SetIdentity();
							}
						}
						//---------------------------- Pos List

						//Profiler.EndSample();

						//Profiler.BeginSample("4-2-2. Mod Mesh Log Save");

						//>>>> LinkedMatrix를 만들어서 GizmoEdit를 할 수 있게 만들자
						//paramKeyValue._modifiedMesh.CalculatedLog.CalculateModified(paramKeyValue._weight, keyParamSetGroup.CalculatedLog);

						//Profiler.EndSample();

						nCalculated++;//Visible 계산을 위해 "paramKey 계산 횟수"를 카운트하자

					}//--- Params


					//Profiler.EndSample();

					//이제 tmp값을 Result에 넘겨주자
					//처음 Layer라면 -> 100% 적용
					//그렇지 않다면 Blend를 해주자

					layerWeight = 1.0f;

					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.
					calParam._totalParamSetGroupWeight_Transform += layerWeight;

					//if (nCalculated == 0)
					//{
					//	tmpVisible = true;

					//}

					//Profiler.BeginSample("4-3. Pos List Result");

					//Debug.Log("Set Pos [" + posList.Count + "]");
					for (int iPos = 0; iPos < posList.Length; iPos++)
					{
						posList[iPos] = tmpPosList[iPos] * layerWeight;
						
						//if (iPos == 0)
						//{
						//	Debug.Log("Prev Matx : \n" + vertMatrixList[iPos].ToString());
						//}
						//추가
						vertMatrixList[iPos].SetMatrixWithWeight(tmpVertMatrixList[iPos], layerWeight);
						//vertMatrixList[iPos] = apMatrix3x3.identity;
						//vertMatrixList[iPos] = vertMatrixList[iPos] * (1-layerWeight) + (tmpVertMatrixList[iPos] * layerWeight);

						//if (iPos == 0)
						//{
						//	Debug.Log("Cal RigMatx -> Result (" + layerWeight + " )\n" + tmpVertMatrixList[iPos].ToString() + " \n>>\n" + vertMatrixList[iPos].ToString());
						//}
					}

					//Profiler.EndSample();

					//Profiler.BeginSample("4-3. Save Log");

					//>>> CalculatedLog
					//keyParamSetGroup.CalculatedLog.CalculateParamSetGroup(layerWeight,
					//														iCalculatedSubParam,
					//														apModifierParamSetGroup.BLEND_METHOD.Interpolation,
					//														null,
					//														calParam.CalculatedLog);

					iCalculatedSubParam++;
					//Profiler.EndSample();


					//Profiler.EndSample();

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)
				calParam._isAvailable = true;


			}

			//Profiler.EndSample();
		}



		//초당 얼마나 업데이트 요청을 받는지 체크
		private int _nUpdateCall = 0;
		private float _tUpdateCall = 0.0f;
		private int _nUpdateValid = 0;

		protected void CalculatePattern_Physics(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			bool isValidFrame = false;//유효한 프레임[물리 처리를 한다], 유효하지 않은 

			if (_stopwatch == null)
			{
				_stopwatch = new System.Diagnostics.Stopwatch();
				_stopwatch.Start();
				_tDeltaFixed = 0.0f;
			}

			//tDelta를 별도로 받자
			tDelta = (float)(_stopwatch.ElapsedMilliseconds / 1000.0f);
			_tDeltaFixed += tDelta;
			_tUpdateCall += tDelta;
			_nUpdateCall++;


			if (_tDeltaFixed > PHYSIC_DELTA_TIME)
			{
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

			_stopwatch.Stop();
			_stopwatch.Reset();
			_stopwatch.Start();



			apCalculatedResultParam calParam = null;
			Vector2[] posList = null;
			Vector2[] tmpPosList = null;
			List<apCalculatedResultParamSubList> subParamGroupList = null;
			List<apCalculatedResultParam.ParamKeyValueSet> subParamKeyValueList = null;
			float layerWeight = 0.0f;
			apModifierParamSetGroup keyParamSetGroup = null;
			apModifierParamSetGroupVertWeight weigetedVertData = null;
			apCalculatedResultParamSubList curSubList = null;
			int nParamKeys = 0;
			apCalculatedResultParam.ParamKeyValueSet paramKeyValue = null;

			//지역 변수를 여기서 일괄 선언하자
			apModifiedVertexWeight modVertWeight = null;
			apPhysicsVertParam physicVertParam = null;
			apPhysicsMeshParam physicMeshParam = null;
			int nVert = 0;
			float mass = 0.0f;

			Vector2 F_gravity = Vector2.zero;
			Vector2 F_wind = Vector2.zero;
			Vector2 F_stretch = Vector2.zero;
			//Vector2 F_airDrag = Vector2.zero;
			//Vector2 F_inertia = Vector2.zero;
			Vector2 F_recover = Vector2.zero;

			Vector2 F_ext = Vector2.zero;//<<추가된 "외부 힘"

			Vector2 F_sum = Vector2.zero;
			Vector2 F_viscosity = Vector2.zero;


			apPhysicsVertParam.LinkedVertex linkedVert = null;
			bool isViscosity = false;

			Vector2 srcVertPos_NoMod = Vector2.zero;
			Vector2 linkVertPos_NoMod = Vector2.zero;
			Vector2 srcVertPos_Cur = Vector2.zero;
			Vector2 linkVertPos_Cur = Vector2.zero;
			Vector2 deltaVec_0 = Vector2.zero;
			Vector2 deltaVec_Cur = Vector2.zero;

			//bool isFirstDebug = true;

			//Profiler.BeginSample("Modifier : Physics");

			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				calParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크

				// 중요!
				//-------------------------------------------------------
				//1. Param Weight Calculate
				calParam.Calculate();
				//-------------------------------------------------------


				//>>> LinkedMatrix를 초기화
				//calParam.CalculatedLog.ReadyToRecord();//<<<<<<



				posList = calParam._result_Positions;
				//tmpPosList = calParam._tmp_Positions;
				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				layerWeight = 0.0f;
				keyParamSetGroup = null;

				weigetedVertData = calParam._weightedVertexData;

				//일단 초기화
				for (int iPos = 0; iPos < posList.Length; iPos++)
				{
					posList[iPos] = Vector2.zero;
				}

				calParam._result_IsVisible = true;

				int iCalculatedSubParam = 0;

				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					if (curSubList._keyParamSetGroup == null ||
						!curSubList._keyParamSetGroup.IsCalculateEnabled)
					{
						//Debug.LogError("Modifier Cal Param Failed : " + DisplayName + " / " + calParam._linkedModifier.DisplayName);
						continue;
					}

					//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
					nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;



					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;


					//>>> LinkedMatrix
					//keyParamSetGroup.CalculatedLog.ReadyToRecord();//<<<<<<


					//Vector2 calculatedValue = Vector2.zero;

					bool isFirstParam = true;

					//레이어 내부의 임시 데이터를 먼저 초기화
					tmpPosList = keyParamSetGroup._tmpPositions;

					if (tmpPosList == null ||
						tmpPosList.Length != posList.Length)
					{
						keyParamSetGroup._tmpPositions = new Vector2[posList.Length];
						tmpPosList = keyParamSetGroup._tmpPositions;

						for (int iPos = 0; iPos < posList.Length; iPos++)
						{
							tmpPosList[iPos] = Vector2.zero;
						}
					}
					else
					{
						for (int iPos = 0; iPos < posList.Length; iPos++)
						{
							tmpPosList[iPos] = Vector2.zero;
						}
					}

					float totalWeight = 0.0f;
					int nCalculated = 0;


					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.

					//Debug.Log("Physic " + _portrait._isPhysicsPlay_Editor + " / " + _portrait._isPhysicsSupport_Editor + " / " + tDelta);
					for (int iPV = 0; iPV < nParamKeys; iPV++)
					{
						paramKeyValue = subParamKeyValueList[iPV];


						//>>>> Cal Log 초기화
						//paramKeyValue._modifiedMesh.CalculatedLog.ReadyToRecord();


						if (!paramKeyValue._isCalculated)
						{ continue; }

						totalWeight += paramKeyValue._weight;

						//물리 계산 순서
						//Vertex 각각의 이전프레임으로 부터의 속력 계산
						
						if (posList.Length > 0 
							&& _portrait._isPhysicsPlay_Editor 
							&& _portrait._isPhysicsSupport_Editor//<<Portrait에서 지원하는 경우만
							)
						{
							modVertWeight = null;
							physicVertParam = null;
							physicMeshParam = paramKeyValue._modifiedMesh.PhysicParam;
							nVert = posList.Length;
							mass = physicMeshParam._mass;
							if (mass < 0.001f)
							{
								mass = 0.001f;
							}

							//Vertex에 상관없이 적용되는 힘
							// 중력, 바람
							//1) 중력 : mg
							F_gravity = mass * physicMeshParam.GetGravityAcc();

							//2) 바람 : ma
							F_wind = mass * physicMeshParam.GetWindAcc(tDelta);

							F_stretch = Vector2.zero;
							//F_airDrag = Vector2.zero;

							//F_inertia = Vector2.zero;
							F_recover = Vector2.zero;
							F_ext = Vector2.zero;
							F_sum = Vector2.zero;

							linkedVert = null;
							isViscosity = physicMeshParam._viscosity > 0.0f;



							//---------------------------- Pos List



							for (int iPos = 0; iPos < posList.Length; iPos++)
							{
								//여기서 물리 계산을 하자
								modVertWeight = paramKeyValue._modifiedMesh._vertWeights[iPos];
								modVertWeight.UpdatePhysicVertex(tDelta, isValidFrame);//<<RenderVert의 위치와 속도를 계산한다.



								F_stretch = Vector2.zero;
								//F_airDrag = Vector2.zero;

								//F_inertia = Vector2.zero;
								F_recover = Vector2.zero;
								F_sum = Vector2.zero;


								if (!modVertWeight._isEnabled)
								{
									//처리 안함다
									modVertWeight._calculatedDeltaPos = Vector2.zero;
									continue;
								}
								if (modVertWeight._renderVertex == null)
								{
									//Debug.LogError("Render Vertex is Not linked");
									break;
								}

								//최적화는 나중에 하고 일단 업데이트만이라도 하자

								physicVertParam = modVertWeight._physicParam;

								//이동 제한 범위를 초기화
								modVertWeight._isLimitPos = false;
								modVertWeight._limitScale = -1.0f;

								//추가
								//> 유효한 프레임 : 물리 계산을 한다.
								//> 생략하는 프레임 : 이전 속도를 그대로 이용한다.
								if (isValidFrame)
								{
									//1) 유효한 프레임이다.
									//Velocity_Next를 계산하자
									F_stretch = Vector2.zero;


									//Profiler.BeginSample("Physics - F-Stretch");

									//1) 장력 Strech : -k * (<delta Dist> * 기존 UnitVector)
									for (int iLinkVert = 0; iLinkVert < physicVertParam._linkedVertices.Count; iLinkVert++)
									{
										linkedVert = physicVertParam._linkedVertices[iLinkVert];
										float linkWeight = linkedVert._distWeight;

										srcVertPos_NoMod = modVertWeight._pos_World_NoMod;
										linkVertPos_NoMod = linkedVert._modVertWeight._pos_World_NoMod;
										linkedVert._deltaPosToTarget_NoMod = srcVertPos_NoMod - linkVertPos_NoMod;


										srcVertPos_Cur = modVertWeight._pos_Real;
										linkVertPos_Cur = linkedVert._modVertWeight._pos_Real;

										deltaVec_0 = srcVertPos_NoMod - linkVertPos_NoMod;
										deltaVec_Cur = srcVertPos_Cur - linkVertPos_Cur;


										//F_stretch += -1.0f * physicMeshParam._stretchK * (deltaVec_Cur - deltaVec_0) * linkWeight;//<<기존
										//F_stretch += -1.0f * physicMeshParam._stretchK * (deltaVec_Cur - deltaVec_0);
										//totalStretchWeight += linkWeight;

										//길이 차이로 힘을 만들고
										//방향은 현재 Delta

										//<추가> 만약 장력 벡터가 완전히 뒤집힌 경우
										//면이 뒤집혔다.
										if (Vector2.Dot(deltaVec_0, deltaVec_Cur) < 0)
										{
											F_stretch += physicMeshParam._stretchK * (deltaVec_0 - deltaVec_Cur) * linkWeight;
										}
										else
										{
											F_stretch += -1.0f * physicMeshParam._stretchK * (deltaVec_Cur.magnitude - deltaVec_0.magnitude) * deltaVec_Cur.normalized * linkWeight;
										}
										
									}

									//Profiler.EndSample();
									//if (totalStretchWeight > 0.0f)
									//{
									//	F_stretch /= totalStretchWeight;
									//}
									//어차피 Normalize된거라 필요없다.



									//3) 공기 저항 : "현재 이동 방향의 반대 방향"
									//F_airDrag = -1.0f * physicMeshParam._airDrag * modVertWeight._velocity_Real;

#region [미사용 코드]
									//4) 관성력 (탄성력) : "속도 변화에 따른 힘"
									//F_elastic = physicMeshParam._elasticK * ((modVertWeight._velocity_Cur - modVertWeight._velocity_Prev) / Mathf.Clamp(_tDelayedDelta, 0.1f, 0.5f)) * massPerVert;
									//F_inertia = physicMeshParam._inertiaK * ((modVertWeight._velocity_Cur - modVertWeight._velocity_Prev) / tDelta) * mass;
									//F_inertia = physicMeshParam._inertiaK * modVertWeight._acc_Cur * mass;//미리 계산된 가속도를 이용
									//F_inertia = -1.0f * physicMeshParam._inertiaK * modVertWeight._acc_Ex * mass;//미리 계산된 가속도를 이용

									//관성력을 지속하도록 하자
									//새로운 관성력이 => "방향이 비슷하고, 크기가 작다면" -> 이전 관성력을 사용
									//새로운 관성력이 => "방향이 다르거나 크기가 크다면" -> 이 관성력으로 대체하고, 타이머를 리셋
									//Vector2 unitF_inertia_Prev = modVertWeight._F_inertia_Prev.normalized;
									//Vector2 unitF_inertia_Next = F_inertia.normalized;
									//float dotProductInertia = Vector2.Dot(unitF_inertia_Prev, unitF_inertia_Next);
									//if (dotProductInertia > 0.6f && F_inertia.sqrMagnitude < modVertWeight._F_inertia_Prev.sqrMagnitude)
									//{
									//	// 방향만 바꿔주자
									//	F_inertia = unitF_inertia_Next * modVertWeight._F_inertia_Prev.magnitude;
									//}
									//else
									//{
									//	//아예 갱신
									//	modVertWeight._F_inertia_RecordMax = F_inertia;
									//	modVertWeight._tReduceInertia = 0.0f;
									//	modVertWeight._isUsePrevInertia = true;
									//}


									//F_inertia = physicMeshParam._inertiaK * (modVertWeight._velocity_Cur - modVertWeight._velocity_Prev); 
#endregion

									//5) 복원력
									F_recover = -1.0f * physicMeshParam._restoring * modVertWeight._calculatedDeltaPos;

									//6) 추가 : 외부 힘
									//이전 프레임에서의 힘을 이용한다.
									F_ext = _portrait.GetForce(modVertWeight._pos_1F);

									float inertiaK = Mathf.Clamp01(physicMeshParam._inertiaK);
									
									

									//5) 힘의 합력을 구한다.
									if (modVertWeight._physicParam._isMain)
									{
										//F_sum = F_gravity + F_wind + F_stretch + F_airDrag + F_recover + F_ext;//관성 제외
										F_sum = F_gravity + F_wind + F_stretch + F_recover + F_ext;//관성 제외 + 공기 저항도 제외
									}
									else
									{
										//F_sum = F_gravity + F_wind + F_stretch + ((F_airDrag + F_recover + F_ext) * 0.5f);//관성 제외
										F_sum = F_gravity + F_wind + F_stretch + ((F_recover + F_ext) * 0.5f);//관성 제외 + 공기 저항도 제외 //<<
										

										inertiaK *= 0.5f;//<<관성 감소
									}

#region [미사용 코드]
									//if(isFirstDebug && F_sum.magnitude > 0.1f)
									//{
									//	Debug.Log("F_sum > 0"
									//		+ "\r\n > F_gravity : " + F_gravity
									//		+ "\r\n > F_wind : " + F_wind
									//		+ "\r\n > F_stretch : " + F_stretch
									//		+ "\r\n > F_airDrag : " + F_airDrag
									//		+ "\r\n > F_inertia : " + F_inertia
									//		+ "\r\n > F_recover : " + F_recover);

									//	isFirstDebug = false;
									//}

									//F = ma
									//a = F / m
									//Vector2 acc = F_sum / mass;

									//S = vt + S0
									//modVertWeight._velocity_Next = modVertWeight._velocity_Cur + (F_sum / mass) * tDelta;//<<여기가 문제다
									//modVertWeight._velocity_Next = (F_sum / mass) * tDelta;//<<여기가 문제다

									//디버깅용으로 저장을 하자
									//modVertWeight._dbgF_sum = F_sum;
									//modVertWeight._dbgF_gravity = F_gravity;
									//modVertWeight._dbgF_wind = F_wind;
									//modVertWeight._dbgF_stretch = F_stretch;
									//modVertWeight._dbgF_airDrag = F_airDrag;
									//modVertWeight._dbgF_recover = F_recover; 
#endregion


									
									modVertWeight._velocity_Next = 
										//(modVertWeight._velocity_Real * inertiaK + modVertWeight._velocity_1F * (1.0f - inertiaK))
										modVertWeight._velocity_1F
										+ (modVertWeight._velocity_1F - modVertWeight._velocity_Real) * inertiaK
										+ (F_sum / mass) * tDelta
										;

									//Air Drag식 수정
									if(physicMeshParam._airDrag > 0.0f)
									{
										modVertWeight._velocity_Next *= Mathf.Clamp01((1.0f - (physicMeshParam._airDrag * tDelta) / (mass + 0.5f)));
									}
#region [미사용 코드]
									//modVertWeight._velocity_Next = modVertWeight._velocity_Real + (F_sum / mass) * tDelta;


									//modVertWeight._velocity_Next = (modVertWeight._velocity_Cur * Mathf.Clamp01(0.3f * physicMeshParam._inertiaK)) + (F_sum / mass) * tDelta
									////modVertWeight._velocity_Next = (F_sum / mass) * tDelta
									//	+ (-1.0f * 0.5f * physicMeshParam._restoring * modVertWeight._calculatedDeltaPos * tDelta);

									//Vel_Cur은 외부 힘에서도 자동으로 바뀌는 값 (위치가 바뀌면 그걸 속도로 인식하므로)
									//즉, 그 자체로 관성이 자동으로 작동하는 상태이다.
									//(그래서 Vel_Cur이 자동으로 계산되는 로직이 가장 크게 작동하는 셈)

									//if(isFirstDebug && modVertWeight._velocity_Next.magnitude > 0.1f)
									//{
									//	Debug.Log("Velocity Next > 0"
									//		+ "\r\n > modVertWeight._velocity_Next : " + modVertWeight._velocity_Next
									//		+ "\r\n > modVertWeight._velocity_Cur : " + modVertWeight._velocity_Cur
									//		+ "\r\n > F_sum : " + F_sum
									//		+ "\r\n > mass : " + mass
									//		+ "\r\n > tDelta : " + tDelta
									//		);

									//	isFirstDebug = false;
									//}

									//if (isFirstDebug && physicVertParam._isMain && F_sum.magnitude > 100000)
									//{
									//	float frameRate = 0.0f;
									//	if (tDelta > 0.0f)
									//	{
									//		frameRate = 1.0f / tDelta;
									//	}
									//	Vector2 acc = (F_sum / mass);
									//	Debug.Log("Editor F_sum : " + F_sum + " (" + F_sum.magnitude + ") / New Acc : " + acc + "(" + acc.magnitude + ") [FPS " + frameRate + "]"
									//		+ "\r\nNext Velocity : " + modVertWeight._velocity_Next + "(" + modVertWeight._velocity_Next.magnitude + ")"
									//		);
									//	//Debug.Log("Editor Velocity : L : " + modVertWeight._velocity_Cur.magnitude + " / (FPS " + frameRate + ")"
									//	//	+ "\r\nCal Pos : " + modVertWeight._calculatedDeltaPos + " / Prev Inertia : " + modVertWeight._F_inertia_Prev
									//	//	+ "\r\nPos Delta 0 : " + (modVertWeight._pos_World_Records[0] - modVertWeight._pos_World_Records[1]).magnitude + " / Time : " + modVertWeight._tDelta_Records[0]
									//	//	+ "\r\nPos Delta 1 : " + (modVertWeight._pos_World_Records[1] - modVertWeight._pos_World_Records[2]).magnitude + " / Time : " + modVertWeight._tDelta_Records[1]
									//	//	+ "\r\nPos Delta 2 : " + (modVertWeight._pos_World_Records[2] - modVertWeight._pos_World_Records[3]).magnitude + " / Time : " + modVertWeight._tDelta_Records[2]
									//	//	);
									//	isFirstDebug = false;
									//} 
#endregion

								}
								else
								{
									//modVertWeight._velocity_Next = modVertWeight._velocity_Real;
									modVertWeight._velocity_Next = modVertWeight._velocity_1F;
								}

								//변경.
								//여기서 일단 속력을 미리 적용하자
								if (isValidFrame)
								{
									Vector2 nextVelocity = modVertWeight._velocity_Next;

									//V += at
									//마음대로 증가하지 않도록 한다.
									Vector2 limitedNextCalPos = modVertWeight._calculatedDeltaPos + (nextVelocity * tDelta);

									//이동 제한이 걸려있다면
									if (physicMeshParam._isRestrictMoveRange)
									{
										//Profiler.BeginSample("Physics - Move Range");

										float radiusFree = physicMeshParam._moveRange * 0.5f;
										float radiusMax = physicMeshParam._moveRange;

										if (radiusMax <= radiusFree)
										{
											nextVelocity *= 0.0f;
											//둘다 0이라면 아예 이동이 불가
											if (!modVertWeight._isLimitPos)
											{
												modVertWeight._isLimitPos = true;
												modVertWeight._limitScale = 0.0f;
											}
										}
										else
										{
											float curDeltaPosSize = (limitedNextCalPos).magnitude;

											if (curDeltaPosSize < radiusFree)
											{
												//별일 없슴다
											}
											else
											{
												//기본은 선형의 사이즈이지만,
												//돌아가는 힘은 유지해야한다.
												//[deltaPos unitVector dot newVelocity] = 1일때 : 바깥으로 나가려는 힘
												// = -1일때 : 안으로 들어오려는 힘
												// -1 ~ 1 => 0 ~ 1 : 0이면 moveRatio가 1, 1이면 moveRatio가 거리에 따라 1>0
												float dotVector = Vector2.Dot(modVertWeight._calculatedDeltaPos.normalized, nextVelocity.normalized);
												dotVector = (dotVector * 0.5f) + 0.5f; //0: 속도 느려짐 없음 (안쪽으로 들어가려고 함), 1:증가하는 방향

												float outerItp = Mathf.Clamp01((curDeltaPosSize - radiusFree) / (radiusMax - radiusFree));//0 : 속도 느려짐 없음, 1:속도 0

												nextVelocity *= Mathf.Clamp01(1.0f - (dotVector * outerItp));//적절히 느려지게 만들자
																											 //limitedNextCalPos = modVertWeight._calculatedDeltaPos + (nextVelocity * tDelta);

												if (curDeltaPosSize > radiusMax)
												{
													//limitedNextCalPos = modVertWeight._calculatedDeltaPos.normalized * radiusMax;
													if (!modVertWeight._isLimitPos || radiusMax < modVertWeight._limitScale)
													{
														modVertWeight._isLimitPos = true;
														modVertWeight._limitScale = radiusMax;
													}
												}
											}
										}

										//Profiler.EndSample();
									}

									//장력에 의한 길이 제한도 처리한다.
									if (physicMeshParam._isRestrictStretchRange)
									{

										//Profiler.BeginSample("Physics - Stretch Range");

										bool isLimitVelocity2Max = false;
										Vector2 stretchLimitPos = Vector2.zero;
										float limitCalPosDist = 0.0f;
										for (int iLinkVert = 0; iLinkVert < physicVertParam._linkedVertices.Count; iLinkVert++)
										{
											linkedVert = physicVertParam._linkedVertices[iLinkVert];

											//길이의 Min/Max가 있다.
											float distStretchBase = linkedVert._deltaPosToTarget_NoMod.magnitude;

											float stretchRangeMax = (physicMeshParam._stretchRangeRatio_Max) * distStretchBase;
											float stretchRangeMax_Half = (physicMeshParam._stretchRangeRatio_Max * 0.5f) * distStretchBase;

											Vector2 curDeltaFromLinkVert = limitedNextCalPos - linkedVert._modVertWeight._calculatedDeltaPos_Prev;
											float curDistFromLinkVert = curDeltaFromLinkVert.magnitude;

											//너무 멀면 제한한다.
											//단, 제한 권장은 Weight에 맞게

											//float weight = Mathf.Clamp01(linkedVert._distWeight);
											isLimitVelocity2Max = false;

											if (curDistFromLinkVert > stretchRangeMax_Half)
											{
												isLimitVelocity2Max = true;//늘어나는 한계점으로 이동하는 중
												stretchLimitPos = linkedVert._modVertWeight._calculatedDeltaPos_Prev + curDeltaFromLinkVert.normalized * stretchRangeMax;

												if (curDistFromLinkVert > stretchRangeMax)
												{
													limitCalPosDist = (stretchLimitPos).magnitude;
												}
											}

											if (isLimitVelocity2Max)
											{
												//LinkVert간의 벡터를 기준으로 nextVelocity가 확대/축소하는 방향이라면 그 반대의 값을 넣는다.
												float dotVector = Vector2.Dot(curDeltaFromLinkVert.normalized, nextVelocity.normalized);
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

														if (!modVertWeight._isLimitPos || limitCalPosDist < modVertWeight._limitScale)
														{
															modVertWeight._isLimitPos = true;
															modVertWeight._limitScale = limitCalPosDist;
														}
													}

												}

												nextVelocity *= Mathf.Clamp01(1.0f - (dotVector * outerItp));//적절히 느려지게 만들자

											}
										}
										//nextVelocity *= velRatio;

										//Profiler.EndSample();

										//limitedNextCalPos = modVertWeight._calculatedDeltaPos + (nextVelocity * tDelta);
									}

									limitedNextCalPos = modVertWeight._calculatedDeltaPos + (nextVelocity * tDelta);

									modVertWeight._calculatedDeltaPos_Prev = modVertWeight._calculatedDeltaPos;
									modVertWeight._calculatedDeltaPos = limitedNextCalPos;
								}
							}

							//1차로 계산된 값을 이용하여 점성력을 체크한다.
							//수정 : 이미 위치는 계산되었다. 위치를 중심으로 처리를 하자 점성/이동한계를 계산하자
							for (int iPos = 0; iPos < posList.Length; iPos++)
							{
								modVertWeight = paramKeyValue._modifiedMesh._vertWeights[iPos];
								physicVertParam = modVertWeight._physicParam;

								if (!modVertWeight._isEnabled)
								{
									//처리 안함다
									modVertWeight._calculatedDeltaPos = Vector2.zero;
									continue;
								}
								if (modVertWeight._renderVertex == null)
								{
									//Debug.LogError("Render Vertex is Not linked");
									break;
								}

								if (isValidFrame)
								{
									Vector2 nextVelocity = modVertWeight._velocity_Next;
									Vector2 nextCalPos = modVertWeight._calculatedDeltaPos;

									//[점성도]를 계산한다.
									if (isViscosity && !modVertWeight._physicParam._isMain)
									{
										//Profiler.BeginSample("Physics - Viscosity");

										//ID가 같으면 DeltaPos가 비슷해야한다.
										float linkedViscosityWeight = 0.0f;
										//Vector2 linkedViscosityNextVelocity = Vector2.zero;
										Vector2 linkedTotalCalPos = Vector2.zero;

										int curViscosityID = modVertWeight._physicParam._viscosityGroupID;

										for (int iLinkVert = 0; iLinkVert < physicVertParam._linkedVertices.Count; iLinkVert++)
										{
											linkedVert = physicVertParam._linkedVertices[iLinkVert];
											float linkWeight = linkedVert._distWeight;

											if ((linkedVert._modVertWeight._physicParam._viscosityGroupID & curViscosityID) != 0)
											{
												//float subWeight = 1.0f;
												//if(!linkedVert._modVertWeight._physicParam._isMain)
												//{
												//	//subWeight *= 0.3f;
												//}
												//linkedViscosityNextVelocity += linkedVert._modVertWeight._velocity_Next * linkWeight * subWeight;//사실 Vertex의 호출 순서에 따라 값이 좀 다르다.
												linkedTotalCalPos += linkedVert._modVertWeight._calculatedDeltaPos * linkWeight;
												linkedViscosityWeight += linkWeight;
											}
										}

										//점성도를 추가한다.
										if (linkedViscosityWeight > 0.0f)
										{
											//linkedViscosityNextVelocity /= linkedViscosityWeight;
											float clampViscosity = Mathf.Clamp01(physicMeshParam._viscosity) * 0.7f;

											//if(modVertWeight._physicParam._isMain)
											//{
											//	clampViscosity *= 0.8f;
											//}

											//nextVelocity = nextVelocity * (1.0f - clampViscosity) + linkedViscosityNextVelocity * clampViscosity;
											nextCalPos = nextCalPos * (1.0f - clampViscosity) + linkedTotalCalPos * clampViscosity;
										}

										//Profiler.EndSample();

									}


									//이동 한계 한번 더 계산
									if (modVertWeight._isLimitPos && nextCalPos.magnitude > modVertWeight._limitScale)
									{
										nextCalPos = nextCalPos.normalized * modVertWeight._limitScale;
									}


									modVertWeight._calculatedDeltaPos = nextCalPos;



									//속도를 다시 계산해주자
									nextVelocity = (modVertWeight._calculatedDeltaPos - modVertWeight._calculatedDeltaPos_Prev) / tDelta;

									//-----------------------------------------------------------------------------------------
									// 속도 갱신
									modVertWeight._velocity_Next = nextVelocity;

									//modVertWeight._velocity_1F = nextVelocity;//이전 코드
									//속도 차이가 크다면 Real의 비중이 커야 한다.
									//같은 방향이면 -> 버티기 관성이 더 잘보이는게 좋다
									//다른 방향이면 Real을 관성으로 사용해야한다. (그래야 다음 프레임에 관성이 크게 보임)
									//속도 변화에 따라서 체크
									float velocityRefreshITP_X = Mathf.Clamp01(Mathf.Abs( ((modVertWeight._velocity_Real.x - modVertWeight._velocity_Real1F.x) / (Mathf.Abs(modVertWeight._velocity_Real1F.x) + 0.1f)) * 0.5f ) );
									float velocityRefreshITP_Y = Mathf.Clamp01(Mathf.Abs( ((modVertWeight._velocity_Real.y - modVertWeight._velocity_Real1F.y) / (Mathf.Abs(modVertWeight._velocity_Real1F.y) + 0.1f)) * 0.5f ) );

									modVertWeight._velocity_1F.x = nextVelocity.x * (1.0f - velocityRefreshITP_X) + (nextVelocity.x * 0.5f + modVertWeight._velocity_Real.x * 0.5f) * velocityRefreshITP_X;
									modVertWeight._velocity_1F.y = nextVelocity.y * (1.0f - velocityRefreshITP_Y) + (nextVelocity.y * 0.5f + modVertWeight._velocity_Real.y * 0.5f) * velocityRefreshITP_Y;


									modVertWeight._pos_1F = modVertWeight._pos_Real;


									//Damping
									if ((modVertWeight._calculatedDeltaPos.sqrMagnitude < physicMeshParam._damping * physicMeshParam._damping
										&& nextVelocity.sqrMagnitude < physicMeshParam._damping * physicMeshParam._damping)
										|| !modVertWeight._isPhysicsCalculatedPrevFrame)
									{
										modVertWeight._calculatedDeltaPos = Vector2.zero;
										modVertWeight.DampPhysicVertex();

										modVertWeight._isPhysicsCalculatedPrevFrame = true;
									}

								}



								tmpPosList[iPos] +=
										(modVertWeight._calculatedDeltaPos * modVertWeight._weight)
										* paramKeyValue._weight;//<<이 값을 이용한다.




							}
							//---------------------------- Pos List
						}


						//>>>> LinkedMatrix를 만들어서 GizmoEdit를 할 수 있게 만들자
						//paramKeyValue._modifiedMesh.CalculatedLog.CalculateModified(paramKeyValue._weight, keyParamSetGroup.CalculatedLog);


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


					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.
					calParam._totalParamSetGroupWeight_Transform += layerWeight;

					//if(nCalculated == 0)
					//{
					//	tmpVisible = true;
					//}

					//if (keyParamSetGroup._layerIndex == 0)
					if (iCalculatedSubParam == 0)//<<변경
					{
						for (int iPos = 0; iPos < posList.Length; iPos++)
						{
							posList[iPos] = tmpPosList[iPos] * layerWeight;
						}

						//>>> CalculatedLog
						//keyParamSetGroup.CalculatedLog.CalculateParamSetGroup(
						//	layerWeight,
						//	iCalculatedSubParam,
						//	apModifierParamSetGroup.BLEND_METHOD.Interpolation,
						//	null,
						//	calParam.CalculatedLog);
					}
					else
					{
						switch (keyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									if (weigetedVertData != null)
									{
										//Vertex 가중치가 추가되었다.
										float vertWeight = 0.0f;
										for (int iPos = 0; iPos < posList.Length; iPos++)
										{
											vertWeight = layerWeight * weigetedVertData._weightedVerts[iPos]._adaptWeight;

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
									if (weigetedVertData != null)
									{
										//Vertex 가중치가 추가되었다.
										float vertWeight = 0.0f;
										for (int iPos = 0; iPos < posList.Length; iPos++)
										{
											vertWeight = layerWeight * weigetedVertData._weightedVerts[iPos]._adaptWeight;

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
								UnityEngine.Debug.LogError("Mod-Physics : Unknown BLEND_METHOD : " + keyParamSetGroup._blendMethod);
								break;
						}

						//>>> CalculatedLog
						//keyParamSetGroup.CalculatedLog.CalculateParamSetGroup(
						//	layerWeight,
						//	iCalculatedSubParam,
						//	keyParamSetGroup._blendMethod,
						//	null,
						//	calParam.CalculatedLog);
					}

					iCalculatedSubParam++;

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)
				calParam._isAvailable = true;


			}

			//Profiler.EndSample();
		}
	}

}