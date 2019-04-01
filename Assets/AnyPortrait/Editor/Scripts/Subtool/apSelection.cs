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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// Editor
	/// </summary>
	public class apSelection
	{
		// Members
		//-------------------------------------
		public apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }

		public enum SELECTION_TYPE
		{
			None,
			ImageRes,
			Mesh,
			Face,
			MeshGroup,
			Animation,
			Overall,
			Param
		}

		private SELECTION_TYPE _selectionType = SELECTION_TYPE.None;

		public SELECTION_TYPE SelectionType { get { return _selectionType; } }

		private apPortrait _portrait = null;
		private apRootUnit _rootUnit = null;
		private apTextureData _image = null;
		private apMesh _mesh = null;
		private apMeshGroup _meshGroup = null;
		private apControlParam _param = null;
		private apAnimClip _animClip = null;

		//Overall 선택시, 선택가능한 AnimClip 리스트
		private List<apAnimClip> _rootUnitAnimClips = new List<apAnimClip>();
		private apAnimClip _curRootUnitAnimClip = null;


		//Texture 선택시
		private Texture2D _imageImported = null;
		private TextureImporter _imageImporter = null;

		//Anim Clip 내에서 단일 선택시
		private apAnimTimeline _subAnimTimeline = null;//<<타임라인 단일 선택시
		private apAnimTimelineLayer _subAnimTimelineLayer = null;//타임 라인의 레이어 단일 선택시
		private apAnimKeyframe _subAnimKeyframe = null;//단일 선택한 키프레임
		private apAnimKeyframe _subAnimWorkKeyframe = null;//<<자동으로 선택되는 키프레임이다. "현재 프레임"에 위치한 "레이어의 프레임"이다.
		private bool _isAnimTimelineLayerGUIScrollRequest = false;


		private List<apAnimKeyframe> _subAnimKeyframeList = new List<apAnimKeyframe>();//여러개의 키프레임을 선택한 경우 (주로 복불/이동 할때)
		private EX_EDIT _exAnimEditingMode = EX_EDIT.None;//<애니메이션 수정 작업을 하고 있는가
		public EX_EDIT ExAnimEditingMode { get { if (IsAnimEditable) { return _exAnimEditingMode; } return EX_EDIT.None; } }

		//추가 : 레이어에 상관없이 키프레임을 관리하고자 하는 경우
		//Common 선택 -> 각각의 Keyframe 선택 (O)
		//각각의 Keyframe 선택 -> Common 선택 (X)
		//각각의 Keyframe 선택 -> 해당 FrameIndex의 모든 Keyframe이 선택되었는지 확인 -> Common 선택 (O)
		private List<apAnimCommonKeyframe> _subAnimCommonKeyframeList = new List<apAnimCommonKeyframe>();
		private List<apAnimCommonKeyframe> _subAnimCommonKeyframeList_Selected = new List<apAnimCommonKeyframe>();//<<선택된 Keyframe만 따로 표시한다.
		
		public List<apAnimCommonKeyframe> AnimCommonKeyframes {  get { return _subAnimCommonKeyframeList; } }
		public List<apAnimCommonKeyframe> AnimCommonKeyframes_Selected {  get { return _subAnimCommonKeyframeList_Selected; } }


		private bool _isAnimLock = false;


		private apTransform_Mesh _subMeshTransformOnAnimClip = null;
		private apTransform_MeshGroup _subMeshGroupTransformOnAnimClip = null;
		private apControlParam _subControlParamOnAnimClip = null;

		//AnimClip에서 ModMesh를 선택하고 Vertex 수정시
		private apModifiedMesh _modMeshOfAnim = null;
		private apRenderUnit _renderUnitOfAnim = null;
		private ModRenderVert _modRenderVertOfAnim = null;
		private List<ModRenderVert> _modRenderVertListOfAnim = new List<ModRenderVert>();//<<1개만 선택해도 리스트엔 들어가있다.
		private List<ModRenderVert> _modRenderVertListOfAnim_Weighted = new List<ModRenderVert>();//<<Soft Selection, Blur, Volume 등에 포함되는 "Weight가 포함된 리스트"
		private apModifiedBone _modBoneOfAnim = null;


		/// <summary>애니메이션 수정 작업이 가능한가?</summary>
		private bool IsAnimEditable
		{
			get
			{
				if (_selectionType != SELECTION_TYPE.Animation || _animClip == null || _subAnimTimeline == null)
				{
					return false;
				}
				if (_animClip._targetMeshGroup == null)
				{
					return false;
				}
				return true;
			}
		}
		public bool IsAnimPlaying
		{
			get
			{
				if (AnimClip == null)
				{
					return false;
				}
				return AnimClip.IsPlaying_Editor;
			}
		}




		public apModifiedMesh ModMeshOfAnim { get { if (_selectionType == SELECTION_TYPE.Animation) { return _modMeshOfAnim; } return null; } }
		public apModifiedBone ModBoneOfAnim { get { if (_selectionType == SELECTION_TYPE.Animation) { return _modBoneOfAnim; } return null; } }
		public apRenderUnit RenderUnitOfAnim { get { if (_selectionType == SELECTION_TYPE.Animation) { return _renderUnitOfAnim; } return null; } }
		public ModRenderVert ModRenderVertOfAnim { get { if (_selectionType == SELECTION_TYPE.Animation) { return _modRenderVertOfAnim; } return null; } }
		public List<ModRenderVert> ModRenderVertListOfAnim { get { if (_selectionType == SELECTION_TYPE.Animation) { return _modRenderVertListOfAnim; } return null; } }
		public List<ModRenderVert> ModRenderVertListOfAnim_Weighted { get { if (_selectionType == SELECTION_TYPE.Animation) { return _modRenderVertListOfAnim_Weighted; } return null; } }

		public Vector2 ModRenderVertsCenterPosOfAnim
		{
			get
			{
				if (_selectionType != SELECTION_TYPE.Animation || _modRenderVertListOfAnim.Count == 0)
				{
					return Vector2.zero;
				}
				Vector2 centerPos = Vector2.zero;
				for (int i = 0; i < _modRenderVertListOfAnim.Count; i++)
				{
					centerPos += _modRenderVertListOfAnim[i]._renderVert._pos_World;
				}
				centerPos /= _modRenderVertListOfAnim.Count;
				return centerPos;
			}
		}


		//Bone


		private apTransform_Mesh _subMeshTransformInGroup = null;
		private apTransform_MeshGroup _subMeshGroupTransformInGroup = null;

		private apModifierBase _modifier = null;

		//Modifier 작업시 선택하는 객체들
		private apModifierParamSet _paramSetOfMod = null;


		private apModifiedMesh _modMeshOfMod = null;
		private apModifiedBone _modBoneOfMod = null;//<추가
		private apRenderUnit _renderUnitOfMod = null;

		//추가
		//modBone으로 등록 가능한 apBone 리스트
		private List<apBone> _modRegistableBones = new List<apBone>();
		public List<apBone> ModRegistableBones { get { return _modRegistableBones; } }

		//Mod Vert와 Render Vert는 동시에 선택이 된다.
		public class ModRenderVert
		{
			public apModifiedVertex _modVert = null;
			public apRenderVertex _renderVert = null;
			//추가
			//ModVert가 아니라 ModVertRig가 매칭되는 경우도 있다.
			//Gizmo에서 주로 사용하는데 에러 안나게 주의할 것
			public apModifiedVertexRig _modVertRig = null;

			public apModifiedVertexWeight _modVertWeight = null;


			/// <summary>
			/// SoftSelection, Blur, Volume등의 "편집 과정에서의 Weight"를 임시로 결정하는 경우의 값
			/// </summary>
			public float _vertWeightByTool = 1.0f;

			public ModRenderVert(apModifiedVertex modVert, apRenderVertex renderVert)
			{
				_modVert = modVert;
				_modVertRig = null;
				_modVertWeight = null;

				_renderVert = renderVert;
				_vertWeightByTool = 1.0f;

			}

			public ModRenderVert(apModifiedVertexRig modVertRig, apRenderVertex renderVert)
			{
				_modVert = null;
				_modVertRig = modVertRig;
				_modVertWeight = null;

				_renderVert = renderVert;
				_vertWeightByTool = 1.0f;
			}

			public ModRenderVert(apModifiedVertexWeight modVertWeight, apRenderVertex renderVert)
			{
				_modVert = null;
				_modVertRig = null;
				_modVertWeight = modVertWeight;

				_renderVert = renderVert;
				_vertWeightByTool = _modVertWeight._weight;//<<이건 갱신해야할 것
			}

			//다음 World 좌표값을 받아서 ModifiedVertex의 값을 수정하자
			public void SetWorldPosToModifier_VertLocal(Vector2 nextWorldPos)
			{
				//NextWorld Pos에서 -> [VertWorld] -> [MeshTransform] -> Vert Local 적용 후의 좌표 -> Vert Local 적용 전의 좌표 
				//적용 전-후의 좌표 비교 = 그 차이값을 ModVert에 넣자
				apMatrix3x3 matToAfterVertLocal = (_renderVert._matrix_Cal_VertWorld * _renderVert._matrix_MeshTransform).inverse;
				Vector2 nextLocalMorphedPos = matToAfterVertLocal.MultiplyPoint(nextWorldPos);
				Vector2 beforeLocalMorphedPos = (_renderVert._matrix_Cal_VertLocal * _renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(_renderVert._vertex._pos);

				_modVert._deltaPos.x += (nextLocalMorphedPos.x - beforeLocalMorphedPos.x);
				_modVert._deltaPos.y += (nextLocalMorphedPos.y - beforeLocalMorphedPos.y);
			}
		}

		//버텍스에 대해서
		//단일 선택일때
		//복수개의 선택일때
		private ModRenderVert _modRenderVertOfMod = null;
		private List<ModRenderVert> _modRenderVertListOfMod = new List<ModRenderVert>();//<<1개만 선택해도 리스트엔 들어가있다.
		private List<ModRenderVert> _modRenderVertListOfMod_Weighted = new List<ModRenderVert>();//<<Soft Selection, Blur, Volume 등에 포함되는 "Weight가 포함된 리스트"


		//메시/메시그룹 트랜스폼에 대해서
		//복수 선택도 가능하게 해주자
		private List<apTransform_Mesh> _subMeshTransformListInGroup = new List<apTransform_Mesh>();
		private List<apTransform_MeshGroup> _subMeshGroupTransformListInGroup = new List<apTransform_MeshGroup>();



		/// <summary>Modifier에서 현재 선택중인 ParamSetGroup [주의 : Animated Modifier에서는 이 값을 사용하지 말고 다른 값을 사용해야한다]</summary>
		private apModifierParamSetGroup _subEditedParamSetGroup = null;

		/// <summary>Animated Modifier에서 현재 선택중인 ParamSetGroup의 Pack. [주의 : Animataed Modifier에서만 사용가능하다]</summary>
		private apModifierParamSetGroupAnimPack _subEditedParamSetGroupAnimPack = null;


		public apPortrait Portrait { get { return _portrait; } }

		public apRootUnit RootUnit { get { if (_selectionType == SELECTION_TYPE.Overall && _portrait != null) { return _rootUnit; } return null; } }
		public List<apAnimClip> RootUnitAnimClipList { get { if (_selectionType == SELECTION_TYPE.Overall && _portrait != null) { return _rootUnitAnimClips; } return null; } }
		public apAnimClip RootUnitAnimClip { get { if (_selectionType == SELECTION_TYPE.Overall && _portrait != null) { return _curRootUnitAnimClip; } return null; } }


		public apTextureData TextureData { get { if (_selectionType == SELECTION_TYPE.ImageRes) { return _image; } return null; } }
		public apMesh Mesh { get { if (_selectionType == SELECTION_TYPE.Mesh) { return _mesh; } return null; } }
		public apMeshGroup MeshGroup { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _meshGroup; } return null; } }
		public apControlParam Param { get { if (_selectionType == SELECTION_TYPE.Param) { return _param; } return null; } }
		public apAnimClip AnimClip { get { if (_selectionType == SELECTION_TYPE.Animation) { return _animClip; } return null; } }

		//Mesh Group에서 서브 선택
		//Mesh/MeshGroup Transform
		public apTransform_Mesh SubMeshInGroup { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _subMeshTransformInGroup; } return null; } }
		public apTransform_MeshGroup SubMeshGroupInGroup { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _subMeshGroupTransformInGroup; } return null; } }

		//ParamSetGroup / ParamSetGroupAnimPack
		/// <summary>Modifier에서 현재 선택중인 ParamSetGroup [주의 : Animated Modifier에서는 이 값을 사용하지 말고 다른 값을 사용해야한다]</summary>
		public apModifierParamSetGroup SubEditedParamSetGroup { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _subEditedParamSetGroup; } return null; } }
		/// <summary>Animated Modifier에서 현재 선택중인 ParamSetGroup의 Pack. [주의 : Animataed Modifier에서만 사용가능하다]</summary>
		public apModifierParamSetGroupAnimPack SubEditedParamSetGroupAnimPack { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _subEditedParamSetGroupAnimPack; } return null; } }


		//MeshGroup Setting에서 Pivot을 바꿀 때
		private bool _isMeshGroupSetting_ChangePivot = false;
		public bool IsMeshGroupSettingChangePivot { get { return _isMeshGroupSetting_ChangePivot; } }

		//현재 선택된 Modifier
		public apModifierBase Modifier { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _modifier; } return null; } }

		//Modifier 작업식 선택하는 객체들
		public apModifierParamSet ParamSetOfMod { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _paramSetOfMod; } return null; } }
		public apModifiedMesh ModMeshOfMod { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _modMeshOfMod; } return null; } }

		public apModifiedBone ModBoneOfMod { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _modBoneOfMod; } return null; } }

		public apRenderUnit RenderUnitOfMod { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _renderUnitOfMod; } return null; } }

		//public apModifiedVertex ModVertOfMod { get { if(_selectionType == SELECTION_TYPE.MeshGroup) { return _modVertOfMod; } return null; } }
		//public apRenderVertex RenderVertOfMod { get { if(_selectionType == SELECTION_TYPE.MeshGroup) { return _renderVertOfMod; } return null; } }
		public ModRenderVert ModRenderVertOfMod { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _modRenderVertOfMod; } return null; } }
		public List<ModRenderVert> ModRenderVertListOfMod { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _modRenderVertListOfMod; } return null; } }
		public List<ModRenderVert> ModRenderVertListOfMod_Weighted { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _modRenderVertListOfMod_Weighted; } return null; } }

		public Vector2 ModRenderVertsCenterPosOfMod
		{
			get
			{
				if (_selectionType != SELECTION_TYPE.MeshGroup || _modRenderVertListOfMod.Count == 0)
				{
					return Vector2.zero;
				}
				Vector2 centerPos = Vector2.zero;
				for (int i = 0; i < _modRenderVertListOfMod.Count; i++)
				{
					centerPos += _modRenderVertListOfMod[i]._renderVert._pos_World;
				}
				centerPos /= _modRenderVertListOfMod.Count;
				return centerPos;
			}
		}

		//public apControlParam ControlParamOfMod { get { if(_selectionType == SELECTION_TYPE.MeshGroup) { return _subControlParamOfMod; } return null; } }
		//public apControlParam ControlParamEditingMod { get { if(_selectionType == SELECTION_TYPE.MeshGroup) { return _subControlParamEditingMod; } return null; } }

		//Mesh Group을 본격적으로 수정할 땐, 다른 기능이 잠겨야 한다.
		public enum EX_EDIT_KEY_VALUE
		{
			None,//<<별 제한없이 컨트롤 가능하며 별도의 UI가 등장하지 않는다.
			ModMeshAndParamKey_ModVert,
			ParamKey_ModMesh,
			ParamKey_Bone
		}
		//private bool _isExclusiveModifierEdit = false;//<true이면 몇가지 기능이 잠긴다.
		private EX_EDIT_KEY_VALUE _exEditKeyValue = EX_EDIT_KEY_VALUE.None;
		public EX_EDIT_KEY_VALUE ExEditMode { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _exEditKeyValue; } return EX_EDIT_KEY_VALUE.None; } }

		/// <summary>
		/// Modifier / Animation 작업시 다른 Modifier/AnimLayer를 제외시킬 것인가에 대한 타입.
		/// </summary>
		public enum EX_EDIT
		{
			None,
			/// <summary>수동으로 제한시키지 않는한 최소한의 제한만 작동하는 모드</summary>
			General_Edit,
			/// <summary>수동으로 제한하여 1개의 Modifier(ParamSet)/AnimLayer만 허용하는 모드</summary>
			ExOnly_Edit,
		}
		private EX_EDIT _exclusiveEditing = EX_EDIT.None;//해당 모드에서 제한적 에디팅 중인가
		public EX_EDIT ExEditingMode { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _exclusiveEditing; } return EX_EDIT.None; } }




		private bool _isLockExEditKey = false;
		public bool IsLockExEditKey { get { return _isLockExEditKey; } }


		public bool IsExEditable
		{
			get
			{
				if (_selectionType != SELECTION_TYPE.MeshGroup)
				{
					return false;
				}

				if (_meshGroup == null || _modifier == null)
				{
					return false;
				}

				switch (ExEditMode)
				{
					case EX_EDIT_KEY_VALUE.None:
						return false;

					case EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert:
						return (ExKey_ModParamSetGroup != null) && (ExKey_ModParamSet != null)
							//&& (ExKey_ModMesh != null)
							;

					case EX_EDIT_KEY_VALUE.ParamKey_Bone:
					case EX_EDIT_KEY_VALUE.ParamKey_ModMesh:
						return (ExKey_ModParamSetGroup != null) && (ExKey_ModParamSet != null);

					default:
						Debug.LogError("TODO : IsExEditable에 정의되지 않는 타입이 들어왔습니다. [" + ExEditMode + "]");
						break;
				}
				return false;
			}
		}

		//키값으로 사용할 것 - 키로 사용하는 것들
		public apModifierParamSetGroup ExKey_ModParamSetGroup
		{
			get
			{
				if (ExEditMode == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert
					|| ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_Bone
					|| ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_ModMesh)
				{
					return SubEditedParamSetGroup;
				}
				return null;
			}
		}

		public apModifierParamSet ExKey_ModParamSet
		{
			get
			{
				if (ExEditMode == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert
					|| ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_Bone
					|| ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_ModMesh)
				{
					return ParamSetOfMod;
				}
				return null;
			}
		}

		public apModifiedMesh ExKey_ModMesh
		{
			get
			{
				if (ExEditMode == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert)
				{
					return ModMeshOfMod;
				}
				return null;
			}
		}

		public apModifiedMesh ExValue_ModMesh
		{
			get
			{
				if (ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_ModMesh)
				{
					return ModMeshOfMod;
				}
				return null;
			}
		}

		public ModRenderVert ExValue_ModVert
		{
			get
			{
				if (ExEditMode == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert)
				{
					return ModRenderVertOfMod;
				}
				return null;
			}
		}
		//TODO : 여러개가 선택되었다면?


		//리깅 전용 변수 
		private bool _rigEdit_isBindingEdit = false;//Rig 작업중인가
		private bool _rigEdit_isTestPosing = false;//Rig 중에 Test Pose를 제어하고 있는가
		public enum RIGGING_EDIT_VIEW_MODE
		{
			WeightColorOnly,
			WeightWithTexture,
		}
		public RIGGING_EDIT_VIEW_MODE _rigEdit_viewMode = RIGGING_EDIT_VIEW_MODE.WeightWithTexture;
		public bool _rigEdit_isBoneColorView = true;
		private float _rigEdit_setWeightValue = 0.5f;
		private float _rigEdit_scaleWeightValue = 0.95f;
		public bool _rigEdit_isAutoNormalize = true;
		public bool IsRigEditTestPosing { get { return _rigEdit_isTestPosing; } }
		public bool IsRigEditBinding { get { return _rigEdit_isBindingEdit; } }

		private float _physics_setWeightValue = 0.5f;
		private float _physics_scaleWeightValue = 0.95f;
		private float _physics_windSimulationScale = 1000.0f;
		private Vector2 _physics_windSimulationDir = new Vector2(1.0f, 0.5f);

		//추가
		//본 생성시 Width를 한번 수정했으면 그 값이 이어지도록 한다.
		//단, Parent -> Child로 추가되서 자동으로 변경되는 경우는 제외 (직접 수정하는 경우만 적용)
		public int _lastBoneShapeWidth = 30;
		public bool _isLastBoneShapeWidthChanged = false;

		//Mesh Edit 변수
		private float _meshEdit_zDepthWeight = 0.5f;

		/// <summary>
		/// Rigging 시에 "현재 Vertex에 연결된 Bone 정보"를 저장한다.
		/// 복수의 Vertex를 선택할 경우를 대비해서 몇가지 변수가 추가
		/// </summary>
		public class VertRigData
		{
			public apBone _bone = null;
			public int _nRig = 0;
			public float _weight = 0.0f;
			public float _weight_Min = 0.0f;
			public float _weight_Max = 0.0f;
			public VertRigData(apBone bone, float weight)
			{
				_bone = bone;
				_nRig = 1;
				_weight = weight;
				_weight_Min = _weight;
				_weight_Max = _weight;
			}
			public void AddRig(float weight)
			{
				_weight = ((_weight * _nRig) + weight) / (_nRig + 1);
				_nRig++;
				_weight_Min = Mathf.Min(weight, _weight_Min);
				_weight_Max = Mathf.Max(weight, _weight_Max);
			}
		}
		private List<VertRigData> _rigEdit_vertRigDataList = new List<VertRigData>();

		// 애니메이션 선택 정보
		public apAnimTimeline AnimTimeline { get { if (AnimClip != null) { return _subAnimTimeline; } return null; } }
		public apAnimTimelineLayer AnimTimelineLayer { get { if (AnimClip != null) { return _subAnimTimelineLayer; } return null; } }
		public apAnimKeyframe AnimKeyframe { get { if (AnimClip != null) { return _subAnimKeyframe; } return null; } }
		public apAnimKeyframe AnimWorkKeyframe { get { if (AnimTimelineLayer != null) { return _subAnimWorkKeyframe; } return null; } }
		public List<apAnimKeyframe> AnimKeyframes { get { if (AnimClip != null) { return _subAnimKeyframeList; } return null; } }
		public bool IsAnimKeyframeMultipleSelected { get { if (AnimClip != null) { return _subAnimKeyframeList.Count > 1; } return false; } }
		//public bool IsAnimAutoKey						{ get { return _isAnimAutoKey; } }
		//public bool IsAnimEditing { get { return _isAnimEditing; } }//<<ExEditing으로 변경
		public bool IsSelectedKeyframe(apAnimKeyframe keyframe)
		{
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				Debug.LogError("Not Animation Type");
				return false;
			}
			return _subAnimKeyframeList.Contains(keyframe);
		}

		public void CancelAnimEditing() { _exAnimEditingMode = EX_EDIT.None; _isAnimLock = false; }
		

		public enum ANIM_SINGLE_PROPERTY_UI { Value, Curve }
		public ANIM_SINGLE_PROPERTY_UI _animPropertyUI = ANIM_SINGLE_PROPERTY_UI.Value;

		public enum ANIM_SINGLE_PROPERTY_CURVE_UI { Prev, Next }
		public ANIM_SINGLE_PROPERTY_CURVE_UI _animPropertyCurveUI = ANIM_SINGLE_PROPERTY_CURVE_UI.Next;

		public apTransform_Mesh SubMeshTransformOnAnimClip { get { if (AnimClip != null) { return _subMeshTransformOnAnimClip; } return null; } }
		public apTransform_MeshGroup SubMeshGroupTransformOnAnimClip { get { if (AnimClip != null) { return _subMeshGroupTransformOnAnimClip; } return null; } }
		public apControlParam SubControlParamOnAnimClip { get { if (AnimClip != null) { return _subControlParamOnAnimClip; } return null; } }

		public bool IsAnimSelectionLock { get { if (AnimClip != null) { return _isAnimLock; } return false; } }

		private float _animKeyframeAutoScrollTimer = 0.0f;

		//Bone 편집
		private apBone _bone = null;//현재 선택한 Bone (어떤 모드에서든지 참조 가능)
		public apBone Bone { get { return _bone; } }

		private bool _isBoneDefaultEditing = false;
		public bool IsBoneDefaultEditing { get { return _isBoneDefaultEditing; } }

		public enum BONE_EDIT_MODE
		{
			None,
			SelectOnly,
			Add,
			SelectAndTRS,
			Link
		}
		private BONE_EDIT_MODE _boneEditMode = BONE_EDIT_MODE.None;
		//public BONE_EDIT_MODE BoneEditMode { get { if (!_isBoneDefaultEditing) { return BONE_EDIT_MODE.None; } return _boneEditMode; } }
		public BONE_EDIT_MODE BoneEditMode { get { return _boneEditMode; } }

		public enum MESHGROUP_CHILD_HIERARCHY { ChildMeshes, Bones }
		public MESHGROUP_CHILD_HIERARCHY _meshGroupChildHierarchy = MESHGROUP_CHILD_HIERARCHY.ChildMeshes;
		public MESHGROUP_CHILD_HIERARCHY _meshGroupChildHierarchy_Anim = MESHGROUP_CHILD_HIERARCHY.ChildMeshes;



		// 통계 GUI
		private bool _isStatisticsNeedToRecalculate = true;//재계산이 필요하다.
		private bool _isStatisticsAvailable = false;

		private int _statistics_NumMesh = 0;
		private int _statistics_NumVert = 0;
		private int _statistics_NumEdge = 0;
		private int _statistics_NumTri = 0;
		private int _statistics_NumClippedMesh = 0;
		private int _statistics_NumClippedVert = 0;//클리핑은 따로 계산(Parent+Child)
		private int _statistics_NumTimelineLayer = 0;
		private int _statistics_NumKeyframes = 0;


		//추가 : Ex Edit를 위한 RenderUnit Flag 갱신시, 중복 처리를 막기 위함
		private apMeshGroup _prevExFlag_MeshGroup = null;
		private apModifierBase _prevExFlag_Modifier = null;
		private apModifierParamSetGroup _prevExFlag_ParamSetGroup = null;
		private apAnimClip _prevExFlag_AnimClip = null;


		
		//캡쳐 변수
		private enum CAPTURE_MODE
		{
			None,
			Capturing_Thumbnail,//<<썸네일 캡쳐중
			Capturing_ScreenShot,//<<ScreenShot 캡쳐중
			Capturing_GIF_Animation,//GIF 애니메이션 캡쳐중
			Capturing_Spritesheet
		}
		private CAPTURE_MODE _captureMode = CAPTURE_MODE.None;
		private object _captureLoadKey = null;
		private string _capturePrevFilePath_Directory = "";
		private apAnimClip _captureSelectedAnimClip = null;
		private bool _captureGIF_IsProgressDialog = false;

		private bool _captureSprite_IsAnimClipInit = false;
		private List<apAnimClip> _captureSprite_AnimClips = new List<apAnimClip>();
		private List<bool> _captureSprite_AnimClipFlags = new List<bool>();


		// Init
		//-------------------------------------
		public apSelection(apEditor editor)
		{
			_editor = editor;
			Clear();
		}

		public void Clear()
		{
			_selectionType = SELECTION_TYPE.None;

			_portrait = null;
			_image = null;
			_mesh = null;
			_meshGroup = null;
			_param = null;
			_modifier = null;
			_animClip = null;

			_bone = null;

			_subMeshTransformInGroup = null;
			_subMeshGroupTransformInGroup = null;
			_subEditedParamSetGroup = null;
			_subEditedParamSetGroupAnimPack = null;

			_exEditKeyValue = EX_EDIT_KEY_VALUE.None;
			_exclusiveEditing = EX_EDIT.None;
			_isLockExEditKey = false;

			_renderUnitOfMod = null;
			//_renderVertOfMod = null;

			_modRenderVertOfMod = null;
			_modRenderVertListOfMod.Clear();
			_modRenderVertListOfMod_Weighted.Clear();

			_subMeshTransformListInGroup.Clear();
			_subMeshGroupTransformListInGroup.Clear();

			_isMeshGroupSetting_ChangePivot = false;

			_subAnimTimeline = null;
			_subAnimTimelineLayer = null;
			_subAnimKeyframe = null;
			_subAnimWorkKeyframe = null;

			_subMeshTransformOnAnimClip = null;
			_subMeshGroupTransformOnAnimClip = null;
			_subControlParamOnAnimClip = null;

			_subAnimKeyframeList.Clear();
			//_isAnimEditing = false;
			_exAnimEditingMode = EX_EDIT.None;
			//_isAnimAutoKey = false;
			_isAnimLock = false;

			_subAnimCommonKeyframeList.Clear();
			_subAnimCommonKeyframeList_Selected.Clear();


			_modMeshOfAnim = null;
			_modBoneOfAnim = null;
			_renderUnitOfAnim = null;
			_modRenderVertOfAnim = null;
			_modRenderVertListOfAnim.Clear();
			_modRenderVertListOfAnim_Weighted.Clear();

			_isBoneDefaultEditing = false;


			_rigEdit_isBindingEdit = false;//Rig 작업중인가
			_rigEdit_isTestPosing = false;//Rig 중에 Test Pose를 제어하고 있는가
										  //_rigEdit_viewMode = RIGGING_EDIT_VIEW_MODE.WeightWithTexture//<<이건 초기화 안된다.

			_imageImported = null;
			_imageImporter = null;
		}


		// Functions
		//-------------------------------------
		public void SetPortrait(apPortrait portrait)
		{
			if (portrait != _portrait)
			{
				Clear();
				_portrait = portrait;
			}

			if (_portrait != null)
			{
				try
				{
					if (apEditorUtil.IsPrefab(_portrait.gameObject))
					{
						//Prefab 해제 안내
						if (EditorUtility.DisplayDialog(
										Editor.GetText(TEXT.DLG_PrefabDisconn_Title),
										Editor.GetText(TEXT.DLG_PrefabDisconn_Body),
										Editor.GetText(TEXT.Okay)))
						{
							apEditorUtil.DisconnectPrefab(_portrait);
						}
					}
				}
				catch(Exception ex)
				{
					Debug.LogError("Prefab Check Error : " + ex);
				}
				
			}
			//통계 재계산 요청
			SetStatisticsRefresh();
		}


		public void SetNone()
		{
			_selectionType = SELECTION_TYPE.None;

			//_portrait = null;
			_rootUnit = null;
			_rootUnitAnimClips.Clear();
			_curRootUnitAnimClip = null;

			_image = null;
			_mesh = null;
			_meshGroup = null;
			_param = null;
			_animClip = null;

			_subMeshTransformInGroup = null;
			_subMeshGroupTransformInGroup = null;

			_subMeshTransformListInGroup.Clear();
			_subMeshGroupTransformListInGroup.Clear();

			_modifier = null;

			_isMeshGroupSetting_ChangePivot = false;

			_paramSetOfMod = null;
			_modMeshOfMod = null;
			//_modVertOfMod = null;
			_modBoneOfMod = null;
			_modRegistableBones.Clear();
			//_subControlParamOfMod = null;
			//_subControlParamEditingMod = null;
			_subEditedParamSetGroup = null;
			_subEditedParamSetGroupAnimPack = null;

			_exEditKeyValue = EX_EDIT_KEY_VALUE.None;
			_exclusiveEditing = EX_EDIT.None;
			_isLockExEditKey = false;

			_renderUnitOfMod = null;
			//_renderVertOfMod = null;

			_modRenderVertOfMod = null;
			_modRenderVertListOfMod.Clear();
			_modRenderVertListOfMod_Weighted.Clear();


			_subAnimTimeline = null;
			_subAnimTimelineLayer = null;
			_subAnimKeyframe = null;
			_subAnimWorkKeyframe = null;

			_subMeshTransformOnAnimClip = null;
			_subMeshGroupTransformOnAnimClip = null;
			_subControlParamOnAnimClip = null;

			_subAnimKeyframeList.Clear();
			//_isAnimEditing = false;
			_exAnimEditingMode = EX_EDIT.None;
			//_isAnimAutoKey = false;
			_isAnimLock = false;

			_subAnimCommonKeyframeList.Clear();
			_subAnimCommonKeyframeList_Selected.Clear();

			_modMeshOfAnim = null;
			_modBoneOfAnim = null;
			_renderUnitOfAnim = null;
			_modRenderVertOfAnim = null;
			_modRenderVertListOfAnim.Clear();
			_modRenderVertListOfAnim_Weighted.Clear();

			_bone = null;
			_isBoneDefaultEditing = false;

			_rigEdit_isBindingEdit = false;//Rig 작업중인가
			_rigEdit_isTestPosing = false;//Rig 중에 Test Pose를 제어하고 있는가

			_imageImported = null;
			_imageImporter = null;

			SetBoneRiggingTest();
			Editor.Hierarchy_MeshGroup.ResetSubUnits();

			if (Editor._portrait != null)
			{
				for (int i = 0; i < Editor._portrait._animClips.Count; i++)
				{
					Editor._portrait._animClips[i]._isSelectedInEditor = false;
				}
			}

			Editor.Gizmos.RefreshFFDTransformForce();//<추가

			//기즈모 일단 초기화
			Editor.Gizmos.Unlink();

			apEditorUtil.ReleaseGUIFocus();

			apEditorUtil.ResetUndo(Editor);//메뉴가 바뀌면 Undo 기록을 초기화한다.

			//통계 재계산 요청
			SetStatisticsRefresh();

			//스크롤 초기화
			Editor.ResetScrollPosition(false, true, true, true, true);

			//Onion 초기화
			Editor.Onion.Clear();

			//Capture 변수 초기화
			_captureSelectedAnimClip = null;
			_captureMode = CAPTURE_MODE.None;
			_captureLoadKey = null;
			_captureSelectedAnimClip = null;

			//_captureGIF_IsLoopAnimation = false;
			//_captureGIF_IsAnimFirstFrame = false;
			//_captureGIF_CurAnimFrame = 0;
			//_captureGIF_StartAnimFrame = 0;
			//_captureGIF_LastAnimFrame = 0;
			//_captureGIF_CurAnimLoop = 0;
			//_captureGIF_AnimLoopCount = 0;
			//_captureGIF_CurAnimProcess = 0;
			//_captureGIF_TotalAnimProcess = 0;
			//_captureGIF_GifAnimQuality = 0;
			_captureGIF_IsProgressDialog = false;

			_captureSprite_IsAnimClipInit = false;
			_captureSprite_AnimClips.Clear();
			_captureSprite_AnimClipFlags.Clear();
			//_captureSprite_CurAnimClipIndex = 0;
			//_captureSprite_CurFrame = 0;
			//_captureSprite_StartFrame = 0;
			//_captureSprite_LastFrame = 0;
			//_captureSprite_IsLoopAnimation = false;
			//_captureSprite_CurFPS = 0;
			//_captureSprite_TotalAnimFrames = 0;
			//_captureSprite_CurAnimFrameOnTotal = 0;


			//Mesh Edit 모드도 초기화
			//수정 > 일단 없던 일로
			//Editor._meshEditMode = apEditor.MESH_EDIT_MODE.Setting;
			//Editor._meshEditZDepthView = apEditor.MESH_EDIT_RENDER_MODE.Normal;
			//Editor._meshEditeMode_MakeMesh = apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge;

			//당장 다음 1프레임은 쉰다.
			Editor.RefreshClippingGL();

		}


		public void SetImage(apTextureData image)
		{
			SetNone();

			_selectionType = SELECTION_TYPE.ImageRes;

			_image = image;

			//이미지의 Asset 정보는 매번 갱신한다. (언제든 바뀔 수 있으므로)
			if (image._image != null)
			{
				string fullPath = AssetDatabase.GetAssetPath(image._image);
				//Debug.Log("Image Path : " + fullPath);

				if (string.IsNullOrEmpty(fullPath))
				{
					image._assetFullPath = "";
					//image._isPSDFile = false;
				}
				else
				{
					image._assetFullPath = fullPath;
					//if (fullPath.Contains(".psd") || fullPath.Contains(".PSD"))
					//{
					//	image._isPSDFile = true;
					//}
					//else
					//{
					//	image._isPSDFile = false;
					//}
				}
			}
			else
			{
				//주의
				//만약 assetFullPath가 유효하다면 그걸 이용하자
				bool isRestoreImageFromPath = false;
				if(!string.IsNullOrEmpty(image._assetFullPath))
				{
					Texture2D restoreImage = AssetDatabase.LoadAssetAtPath<Texture2D>(image._assetFullPath);
					if(restoreImage != null)
					{
						isRestoreImageFromPath = true;
						image._image = restoreImage;
						Debug.Log("사라진 이미지를 경로로 복구했다. [" + image._assetFullPath + "]");
					}
				}
				if(!isRestoreImageFromPath)
				{
					image._assetFullPath = "";
				}
				//image._isPSDFile = false;
			}

			//통계 재계산 요청
			SetStatisticsRefresh();
		}

		public void SetMesh(apMesh mesh)
		{
			SetNone();

			_selectionType = SELECTION_TYPE.Mesh;

			_mesh = mesh;
			//_prevMesh_Name = _mesh._name;

			//통계 재계산 요청
			SetStatisticsRefresh();

			//현재 MeshEditMode에 따라서 Gizmo 처리를 해야한다.
			switch (Editor._meshEditMode)
			{
				case apEditor.MESH_EDIT_MODE.Setting:
					Editor.Gizmos.Unlink();
					break;

				case apEditor.MESH_EDIT_MODE.MakeMesh:
					Editor.Gizmos.Unlink();
					break;

				case apEditor.MESH_EDIT_MODE.Modify:
					Editor.Gizmos.Unlink();
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshEdit_Modify());
					break;

				case apEditor.MESH_EDIT_MODE.PivotEdit:
					Editor.Gizmos.Unlink();
					break;
			}

		}

		public void SetMeshGroup(apMeshGroup meshGroup)
		{
			SetNone();

			_selectionType = SELECTION_TYPE.MeshGroup;

			bool isChanged = false;
			if (_meshGroup != meshGroup)
			{
				isChanged = true;
			}
			_meshGroup = meshGroup;
			//_prevMeshGroup_Name = _meshGroup._name;

			//_meshGroup.SortRenderUnits(true);//Sort를 다시 해준다. (RenderUnit 세팅때문)
			//Mesh Group을 선택하면 이 초기화를 전부 실행해야한다.

			


			_meshGroup.SetDirtyToReset();
			_meshGroup.SetDirtyToSort();
			//_meshGroup.SetAllRenderUnitForceUpdate();
			_meshGroup.RefreshForce(true);//Depth 바뀌었다고 강제한다.



			Editor.Hierarchy_MeshGroup.ResetSubUnits();
			Editor.Hierarchy_MeshGroup.RefreshUnits();

			Editor._meshGroupEditMode = apEditor.MESHGROUP_EDIT_MODE.Setting;

			if (isChanged)
			{
				_meshGroup.LinkModMeshRenderUnits();
				_meshGroup.RefreshModifierLink();
				_meshGroup._modifierStack.InitModifierCalculatedValues();//<<값 초기화

				_meshGroup._modifierStack.RefreshAndSort(true);
				Editor.Gizmos.RefreshFFDTransformForce();
			}

			Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_meshGroup);

			//추가 : Bone의 GUI Visible을 리셋한다.
			_meshGroup.ResetBoneGUIVisible();

			Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshGroupSetting());

			SetModifierExclusiveEditing(EX_EDIT.None);
			SetModifierExclusiveEditKeyLock(false);
			SetModifierEditMode(EX_EDIT_KEY_VALUE.None);

			//통계 재계산 요청
			SetStatisticsRefresh();
		}




		public void SetSubMeshInGroup(apTransform_Mesh subMeshTransformInGroup)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup)
			{
				_subMeshTransformInGroup = null;

				_renderUnitOfMod = null;
				//_renderVertOfMod = null;

				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();

				_subMeshTransformListInGroup.Clear();
				_subMeshGroupTransformListInGroup.Clear();

				return;
			}

			bool isChanged = (_subMeshTransformInGroup != subMeshTransformInGroup);

			_subMeshTransformInGroup = subMeshTransformInGroup;
			_subMeshGroupTransformInGroup = null;

			_subMeshTransformListInGroup.Clear();
			_subMeshTransformListInGroup.Add(_subMeshTransformInGroup);//<<MeshTransform 한개만 넣어주자

			_subMeshGroupTransformListInGroup.Clear();

			//여기서 만약 Modifier 선택중이며, 특정 ParamKey를 선택하고 있다면
			//자동으로 ModifierMesh를 선택해보자
			AutoSelectModMeshOrModBone();

			if (isChanged)
			{
				Editor.Gizmos.RefreshFFDTransformForce();
			}
		}

		public void SetSubMeshGroupInGroup(apTransform_MeshGroup subMeshGroupTransformInGroup)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup)
			{
				_subMeshTransformInGroup = null;

				_renderUnitOfMod = null;
				//_renderVertOfMod = null;

				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();

				_subMeshTransformListInGroup.Clear();
				_subMeshGroupTransformListInGroup.Clear();
				return;
			}

			bool isChanged = (_subMeshGroupTransformInGroup != subMeshGroupTransformInGroup);

			_subMeshTransformInGroup = null;
			_subMeshGroupTransformInGroup = subMeshGroupTransformInGroup;


			_subMeshTransformListInGroup.Clear();
			_subMeshGroupTransformListInGroup.Clear();
			_subMeshGroupTransformListInGroup.Add(_subMeshGroupTransformInGroup);//<<MeshGroupTransform 한개만 넣어주자

			//여기서 만약 Modifier 선택중이며, 특정 ParamKey를 선택하고 있다면
			//자동으로 ModifierMesh를 선택해보자
			AutoSelectModMeshOrModBone();

			if (isChanged)
			{
				Editor.Gizmos.RefreshFFDTransformForce();
			}
		}

		public void SetModifier(apModifierBase modifier)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup)
			{
				_modifier = null;
				return;
			}

			bool isChanged = false;
			if (_modifier != modifier || modifier == null)
			{
				_paramSetOfMod = null;
				_modMeshOfMod = null;
				//_modVertOfMod = null;
				_modBoneOfMod = null;
				_modRegistableBones.Clear();
				//_subControlParamOfMod = null;
				//_subControlParamEditingMod = null;
				_subEditedParamSetGroup = null;
				_subEditedParamSetGroupAnimPack = null;

				_renderUnitOfMod = null;
				//_renderVertOfMod = null;

				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();

				_exEditKeyValue = EX_EDIT_KEY_VALUE.None;
				_exclusiveEditing = EX_EDIT.None;

				_modifier = modifier;
				isChanged = true;

				_rigEdit_isBindingEdit = false;//Rig 작업중인가
				_rigEdit_isTestPosing = false;//Rig 중에 Test Pose를 제어하고 있는가



				SetBoneRiggingTest();

				//스크롤 초기화 (오른쪽과 아래쪽)
				Editor.ResetScrollPosition(false, false, false, true, true);

			}

			_modifier = modifier;

			if (modifier != null)
			{
				if ((int)(modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
				{
					SetModifierEditMode(EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert);
				}
				else
				{
					SetModifierEditMode(EX_EDIT_KEY_VALUE.ParamKey_ModMesh);
				}
				#region [미사용 코드]
				//switch (modifier.CalculatedValueType)
				//{
				//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.Vertex:
				//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.Vertex_World:
				//		SetModifierEditMode(EXCLUSIVE_EDIT_MODE.ModMeshAndParamKey_ModVert);
				//		break;

				//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.MeshGroup_Transform:
				//		SetModifierEditMode(EXCLUSIVE_EDIT_MODE.ParamKey_ModMesh);
				//		break;

				//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.MeshGroup_Color:
				//		SetModifierEditMode(EXCLUSIVE_EDIT_MODE.ParamKey_ModMesh);
				//		break;

				//	default:
				//		Debug.LogError("TODO : Modfier -> ExEditMode 세팅 필요");
				//		break;
				//} 
				#endregion


				//ParamSetGroup이 선택되어 있다면 Modifier와의 유효성 체크
				bool isSubEditedParamSetGroupInit = false;
				if (_subEditedParamSetGroup != null)
				{
					if (!_modifier._paramSetGroup_controller.Contains(_subEditedParamSetGroup))
					{
						isSubEditedParamSetGroupInit = true;

					}
				}
				else if (_subEditedParamSetGroupAnimPack != null)
				{
					if (!_modifier._paramSetGroupAnimPacks.Contains(_subEditedParamSetGroupAnimPack))
					{
						isSubEditedParamSetGroupInit = true;
					}
				}
				if (isSubEditedParamSetGroupInit)
				{
					_paramSetOfMod = null;
					_modMeshOfMod = null;
					//_modVertOfMod = null;
					_modBoneOfMod = null;
					_modRegistableBones.Clear();
					//_subControlParamOfMod = null;
					//_subControlParamEditingMod = null;
					_subEditedParamSetGroup = null;
					_subEditedParamSetGroupAnimPack = null;

					_renderUnitOfMod = null;
					//_renderVertOfMod = null;

					_modRenderVertOfMod = null;
					_modRenderVertListOfMod.Clear();
					_modRenderVertListOfMod_Weighted.Clear();
				}



				if (MeshGroup != null)
				{
					//Exclusive 모두 해제
					MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
					RefreshMeshGroupExEditingFlags(MeshGroup, null, null, null, true);//<<추가
				}


				//각 타입에 따라 Gizmo를 넣어주자
				if (_modifier is apModifier_Morph)
				{
					//Morph
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_Morph());
				}
				else if (_modifier is apModifier_TF)
				{
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_TF());
				}
				else if (_modifier is apModifier_Rigging)
				{
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_Rigging());
					_rigEdit_isTestPosing = false;//Modifier를 선택하면 TestPosing은 취소된다.

					SetBoneRiggingTest();
				}
				else if (_modifier is apModifier_Physic)
				{
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_Physics());
				}
				else
				{
					if (!_modifier.IsAnimated)
					{
						Debug.LogError("Modifier를 선택하였으나 Animation 타입이 아닌데도 Gizmo에 지정되지 않은 타입 : " + _modifier.GetType());
					}
					//아니면 말고 >> Gizmo 초기화
					Editor.Gizmos.Unlink();
				}

				//AutoSelect하기 전에
				//현재 타입이 Static이라면
				//ParamSetGroup/ParamSet은 자동으로 선택한다.
				//ParamSetGroup, ParamSet은 각각 한개씩 존재한다.
				if (_modifier.SyncTarget == apModifierParamSetGroup.SYNC_TARGET.Static)
				{
					apModifierParamSetGroup paramSetGroup = null;
					apModifierParamSet paramSet = null;
					if (_modifier._paramSetGroup_controller.Count == 0)
					{
						Editor.Controller.AddStaticParamSetGroupToModifier();
					}

					paramSetGroup = _modifier._paramSetGroup_controller[0];

					if (paramSetGroup._paramSetList.Count == 0)
					{
						paramSet = new apModifierParamSet();
						paramSet.LinkParamSetGroup(paramSetGroup);
						paramSetGroup._paramSetList.Add(paramSet);
					}

					paramSet = paramSetGroup._paramSetList[0];

					SetParamSetGroupOfModifier(paramSetGroup);
					SetParamSetOfModifier(paramSet);
				}
				else if (!_modifier.IsAnimated)
				{
					if (_subEditedParamSetGroup == null)
					{
						if (_modifier._paramSetGroup_controller.Count > 0)
						{
							//마지막으로 입력된 PSG를 선택
							SetParamSetGroupOfModifier(_modifier._paramSetGroup_controller[_modifier._paramSetGroup_controller.Count - 1]);
						}
					}
					//맨 위의 ParamSetGroup을 선택하자
				}

				if (_modifier.SyncTarget == apModifierParamSetGroup.SYNC_TARGET.Controller)
				{
					Editor.SetLeftTab(apEditor.TAB_LEFT.Controller);
				}
			}
			else
			{
				if (MeshGroup != null)
				{
					//Exclusive 모두 해제
					MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
					RefreshMeshGroupExEditingFlags(MeshGroup, null, null, null, true);//<<추가
				}

				SetModifierEditMode(EX_EDIT_KEY_VALUE.None);

				//아니면 말고 >> Gizmo 초기화
				Editor.Gizmos.Unlink();
			}


			RefreshModifierExclusiveEditing();//<<Mod Lock 갱신
			AutoSelectModMeshOrModBone();

			if (isChanged)
			{
				Editor.Gizmos.RefreshFFDTransformForce();
			}

			//추가 : MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();
		}

		public void SetParamSetGroupOfModifier(apModifierParamSetGroup paramSetGroup)
		{
			//AnimPack 선택은 여기서 무조건 해제된다.
			_subEditedParamSetGroupAnimPack = null;

			if (_selectionType != SELECTION_TYPE.MeshGroup || _modifier == null)
			{
				_subEditedParamSetGroup = null;
				return;
			}
			bool isCheck = false;

			bool isChangedTarget = (_subEditedParamSetGroup != paramSetGroup);
			if (_subEditedParamSetGroup != paramSetGroup)
			{
				_paramSetOfMod = null;
				_modMeshOfMod = null;
				//_modVertOfMod = null;
				_modBoneOfMod = null;
				//_subControlParamOfMod = null;
				//_subControlParamEditingMod = null;

				_renderUnitOfMod = null;
				//_renderVertOfMod = null;

				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();

				//_exclusiveEditMode = EXCLUSIVE_EDIT_MODE.None;
				//_isExclusiveEditing = false;

				if (ExEditingMode == EX_EDIT.ExOnly_Edit)
				{
					//SetModifierExclusiveEditing(false);
					SetModifierExclusiveEditing(EX_EDIT.None);
				}

				isCheck = true;
			}
			_subEditedParamSetGroup = paramSetGroup;

			if (isCheck && SubEditedParamSetGroup != null)
			{
				bool isChanged = SubEditedParamSetGroup.RefreshSync();
				if (isChanged)
				{
					MeshGroup.LinkModMeshRenderUnits();//<<이걸 먼저 선언한다.
					MeshGroup.RefreshModifierLink();
				}
			}

			//추가 : MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();

			RefreshModifierExclusiveEditing();//<<Mod Lock 갱신
			AutoSelectModMeshOrModBone();

			if (isChangedTarget)
			{
				Editor.Gizmos.RefreshFFDTransformForce();
			}
		}

		/// <summary>
		/// Animated Modifier인 경우, ParamSetGroup 대신 ParamSetGroupAnimPack을 선택하고 보여준다.
		/// </summary>
		public void SetParamSetGroupAnimPackOfModifier(apModifierParamSetGroupAnimPack paramSetGroupAnimPack)
		{
			//일반 선택은 여기서 무조건 해제된다.
			_subEditedParamSetGroup = null;

			if (_selectionType != SELECTION_TYPE.MeshGroup || _modifier == null)
			{
				_subEditedParamSetGroupAnimPack = null;
				return;
			}
			bool isCheck = false;

			bool isChangedTarget = (_subEditedParamSetGroupAnimPack != paramSetGroupAnimPack);
			if (_subEditedParamSetGroupAnimPack != paramSetGroupAnimPack)
			{
				_paramSetOfMod = null;
				_modMeshOfMod = null;
				//_modVertOfMod = null;
				_modBoneOfMod = null;
				//_subControlParamOfMod = null;
				//_subControlParamEditingMod = null;

				_renderUnitOfMod = null;
				//_renderVertOfMod = null;

				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();


				//_exclusiveEditMode = EXCLUSIVE_EDIT_MODE.None;
				//_isExclusiveEditing = false;
				if (ExEditingMode == EX_EDIT.ExOnly_Edit)
				{
					//SetModifierExclusiveEditing(false);
					SetModifierExclusiveEditing(EX_EDIT.None);
				}

				//SetModifierExclusiveEditing(false);

				isCheck = true;
			}
			_subEditedParamSetGroupAnimPack = paramSetGroupAnimPack;

			if (isCheck && SubEditedParamSetGroup != null)
			{
				bool isChanged = SubEditedParamSetGroup.RefreshSync();
				if (isChanged)
				{
					MeshGroup.LinkModMeshRenderUnits();//<<이걸 먼저 선언한다.
					MeshGroup.RefreshModifierLink();
				}
			}

			AutoSelectModMeshOrModBone();

			if (isChangedTarget)
			{
				Editor.Gizmos.RefreshFFDTransformForce();
			}
		}



		public void SetParamSetOfModifier(apModifierParamSet paramSetOfMod)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup || _modifier == null)
			{
				_paramSetOfMod = null;
				return;
			}

			bool isChanged = false;
			if (_paramSetOfMod != paramSetOfMod)
			{

				//_subControlParamOfMod = null;
				//_subControlParamEditingMod = null;
				//_modMeshOfMod = null;
				//_modVertOfMod = null;
				//_modBoneOfMod = null;
				//_renderUnitOfMod = null;
				//_renderVertOfMod = null;
				isChanged = true;
			}
			_paramSetOfMod = paramSetOfMod;

			RefreshModifierExclusiveEditing();//<<Mod Lock 갱신

			AutoSelectModMeshOrModBone();

			if (isChanged)
			{
				//Editor.Gizmos.RefreshFFDTransformForce();//<추가
				Editor.Gizmos.RevertTransformObjects(null);//<<변경 : Refresh -> Revert (강제)
			}
		}

		/// <summary>
		/// MeshGroup->Modifier->ParamSetGroup을 선택한 상태에서 ParamSet을 선택하지 않았다면,
		/// Modifier의 종류에 따라 ParamSet을 선택한다. (라고 하지만 Controller 입력 타입만 해당한다..)
		/// </summary>
		public void AutoSelectParamSetOfModifier()
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _portrait == null
				|| _meshGroup == null
				|| _modifier == null
				|| _subEditedParamSetGroup == null
				|| _paramSetOfMod != null)//<<ParamSet이 이미 선택되어도 걍 리턴한다.
			{
				return;
			}

			apEditorUtil.ReleaseGUIFocus();

			apModifierParamSet targetParamSet = null;
			switch (_modifier.SyncTarget)
			{
				case apModifierParamSetGroup.SYNC_TARGET.Controller:
					{
						if (_subEditedParamSetGroup._keyControlParam != null)
						{
							apControlParam controlParam = _subEditedParamSetGroup._keyControlParam;
							//해당 ControlParam이 위치한 곳과 같은 값을 가지는 ParamSet이 있으면 이동한다.
							switch (_subEditedParamSetGroup._keyControlParam._valueType)
							{
								case apControlParam.TYPE.Int:
									{
										targetParamSet = _subEditedParamSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
										{
											return controlParam._int_Cur == a._conSyncValue_Int;
										});

										//선택할만한게 있으면 아예 Control Param값을 동기화
										if (targetParamSet != null)
										{
											controlParam._int_Cur = targetParamSet._conSyncValue_Int;
										}
									}
									break;

								case apControlParam.TYPE.Float:
									{
										float fSnapSize = Mathf.Abs(controlParam._float_Max - controlParam._float_Min) / controlParam._snapSize;
										targetParamSet = _subEditedParamSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
										{
											return Mathf.Abs(controlParam._float_Cur - a._conSyncValue_Float) < (fSnapSize * 0.25f);
										});

										//선택할만한게 있으면 아예 Control Param값을 동기화
										if (targetParamSet != null)
										{
											controlParam._float_Cur = targetParamSet._conSyncValue_Float;
										}
									}
									break;

								case apControlParam.TYPE.Vector2:
									{
										float vSnapSizeX = Mathf.Abs(controlParam._vec2_Max.x - controlParam._vec2_Min.x) / controlParam._snapSize;
										float vSnapSizeY = Mathf.Abs(controlParam._vec2_Max.y - controlParam._vec2_Min.y) / controlParam._snapSize;

										targetParamSet = _subEditedParamSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
										{
											return Mathf.Abs(controlParam._vec2_Cur.x - a._conSyncValue_Vector2.x) < (vSnapSizeX * 0.25f)
												&& Mathf.Abs(controlParam._vec2_Cur.y - a._conSyncValue_Vector2.y) < (vSnapSizeY * 0.25f);
										});

										//선택할만한게 있으면 아예 Control Param값을 동기화
										if (targetParamSet != null)
										{
											controlParam._vec2_Cur = targetParamSet._conSyncValue_Vector2;
										}
									}
									break;
							}
						}
					}
					break;
				default:
					//그 외에는.. 적용되는게 없어요
					break;
			}

			if (targetParamSet != null)
			{
				_paramSetOfMod = targetParamSet;

				AutoSelectModMeshOrModBone();

				//Editor.RefreshControllerAndHierarchy();
				Editor.Gizmos.RefreshFFDTransformForce();//<추가
			}

		}

		// Mod-Mesh, Vert, Bone 선택
		public bool SetModMeshOfModifier(apModifiedMesh modMeshOfMod)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup || _modifier == null || _paramSetOfMod == null)
			{
				_modMeshOfMod = null;
				_modBoneOfMod = null;
				return false;
			}

			if (_modMeshOfMod != modMeshOfMod)
			{
				//_modVertOfMod = null;
				//_renderVertOfMod = null;
				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();
			}
			_modMeshOfMod = modMeshOfMod;
			_modBoneOfMod = null;
			return true;

		}

		public bool SetModBoneOfModifier(apModifiedBone modBoneOfMod)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup || _modifier == null || _paramSetOfMod == null)
			{
				_modMeshOfMod = null;
				_modBoneOfMod = null;
				return false;
			}

			_modBoneOfMod = modBoneOfMod;

			_modMeshOfMod = null;
			_modRenderVertOfMod = null;
			_modRenderVertListOfMod.Clear();
			_modRenderVertListOfMod_Weighted.Clear();

			apEditorUtil.ReleaseGUIFocus();

			return true;
		}

		/// <summary>
		/// Mod-Render Vertex를 선택한다. [Modifier 수정작업시]
		/// ModVert, ModVertRig, ModVertWeight 중 값 하나를 넣어줘야 한다.
		/// </summary>
		/// <param name="modVertOfMod"></param>
		/// <param name="renderVertOfMod"></param>
		public void SetModVertexOfModifier(apModifiedVertex modVertOfMod, apModifiedVertexRig modVertRigOfMod, apModifiedVertexWeight modVertWeight, apRenderVertex renderVertOfMod)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				|| _modMeshOfMod == null)
			{
				//_modVertOfMod = null;
				//_renderVertOfMod = null;
				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();
				return;
			}

			AutoSelectModMeshOrModBone();

			bool isInitReturn = false;
			if (renderVertOfMod == null)
			{
				isInitReturn = true;
			}
			else if (modVertOfMod == null && modVertRigOfMod == null && modVertWeight == null)
			{
				isInitReturn = true;
			}

			//if (modVertOfMod == null || renderVertOfMod == null)
			if (isInitReturn)
			{
				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();
				return;
			}


			//_modVertOfMod = modVertOfMod;
			//_renderVertOfMod = renderVertOfMod;
			bool isChangeModVert = false;
			//기존의 ModRenderVert를 유지할 것인가 또는 새로 선택(생성)할 것인가
			if (_modRenderVertOfMod != null)
			{
				if (_modRenderVertOfMod._renderVert != renderVertOfMod)
				{
					isChangeModVert = true;
				}
				else if (modVertOfMod != null)
				{
					if (_modRenderVertOfMod._modVert != modVertOfMod)
					{
						isChangeModVert = true;
					}
				}
				else if (modVertRigOfMod != null)
				{
					if (_modRenderVertOfMod._modVertRig != modVertRigOfMod)
					{
						isChangeModVert = true;
					}
				}
				else if (modVertWeight != null)
				{
					if (_modRenderVertOfMod._modVertWeight != modVertWeight)
					{
						isChangeModVert = true;
					}
				}
			}
			else
			{
				isChangeModVert = true;
			}

			if (isChangeModVert)
			{
				if (modVertOfMod != null)
				{
					//Vert
					_modRenderVertOfMod = new ModRenderVert(modVertOfMod, renderVertOfMod);
				}
				else if (modVertRigOfMod != null)
				{
					//VertRig
					_modRenderVertOfMod = new ModRenderVert(modVertRigOfMod, renderVertOfMod);
				}
				else
				{
					//VertWeight
					_modRenderVertOfMod = new ModRenderVert(modVertWeight, renderVertOfMod);
				}

				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod.Add(_modRenderVertOfMod);

				_modRenderVertListOfMod_Weighted.Clear();
			}
		}

		/// <summary>
		/// Mod-Render Vertex를 추가한다. [Modifier 수정작업시]
		/// ModVert, ModVertRig, ModVertWeight 중 값 하나를 넣어줘야 한다.
		/// </summary>
		public void AddModVertexOfModifier(apModifiedVertex modVertOfMod, apModifiedVertexRig modVertRigOfMod, apModifiedVertexWeight modVertWeight, apRenderVertex renderVertOfMod)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				|| _modMeshOfMod == null)
			{
				return;
			}

			//AutoSelectModMesh();//<<여기선 생략

			if (renderVertOfMod == null)
			{
				return;
			}

			if (modVertOfMod == null && modVertRigOfMod == null && modVertWeight == null)
			{
				//셋다 없으면 안된다.
				return;
			}
			bool isExistSame = _modRenderVertListOfMod.Exists(delegate (ModRenderVert a)
			{
				return a._renderVert == renderVertOfMod
				|| (a._modVert == modVertOfMod && modVertOfMod != null)
				|| (a._modVertRig == modVertRigOfMod && modVertRigOfMod != null)
				|| (a._modVertWeight == modVertWeight && modVertWeight != null);
			});

			if (!isExistSame)
			{
				ModRenderVert newModRenderVert = null;
				//ModVert에 연동할지, ModVertRig와 연동할지 결정한다.
				if (modVertOfMod != null)
				{
					newModRenderVert = new ModRenderVert(modVertOfMod, renderVertOfMod);
				}
				else if (modVertRigOfMod != null)
				{
					newModRenderVert = new ModRenderVert(modVertRigOfMod, renderVertOfMod);
				}
				else
				{
					newModRenderVert = new ModRenderVert(modVertWeight, renderVertOfMod);
				}

				_modRenderVertListOfMod.Add(newModRenderVert);

				if (_modRenderVertListOfMod.Count == 1)
				{
					_modRenderVertOfMod = newModRenderVert;
				}
			}
		}



		/// <summary>
		/// Mod-Render Vertex를 삭제한다. [Modifier 수정작업시]
		/// ModVert, ModVertRig, ModVertWeight 중 값 하나를 넣어줘야 한다.
		/// </summary>
		public void RemoveModVertexOfModifier(apModifiedVertex modVertOfMod, apModifiedVertexRig modVertRigOfMod, apModifiedVertexWeight modVertWeight, apRenderVertex renderVertOfMod)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				|| _modMeshOfMod == null)
			{
				return;
			}

			//AutoSelectModMesh();//<<여기선 생략

			if (renderVertOfMod == null)
			{
				return;
			}
			if (modVertOfMod == null && modVertRigOfMod == null && modVertWeight == null)
			{
				//셋다 없으면 안된다.
				return;
			}
			//if (modVertOfMod == null || renderVertOfMod == null)
			//{
			//	return;
			//}

			_modRenderVertListOfMod.RemoveAll(delegate (ModRenderVert a)
			{
				return a._renderVert == renderVertOfMod
				|| (a._modVert == modVertOfMod && modVertOfMod != null)
				|| (a._modVertRig == modVertRigOfMod && modVertRigOfMod != null)
				|| (a._modVertWeight == modVertWeight && modVertWeight != null);
			});

			if (_modRenderVertListOfMod.Count == 1)
			{
				_modRenderVertOfMod = _modRenderVertListOfMod[0];
			}
			else if (_modRenderVertListOfMod.Count == 0)
			{
				_modRenderVertOfMod = null;
			}
			else if (!_modRenderVertListOfAnim.Contains(_modRenderVertOfMod))
			{
				_modRenderVertOfMod = null;
				_modRenderVertOfMod = _modRenderVertListOfMod[0];
			}
		}




		//MeshTransform(MeshGroupT)이 선택되어있다면 자동으로 ParamSet 내부의 ModMesh를 선택한다.
		public void AutoSelectModMeshOrModBone()
		{
			//0. ParamSet까지 선택이 안되었다면 아무것도 선택 불가
			//1. ModMesh를 선택할 수 있는가
			//2. ModMesh의 유효한 선택이 없다면 ModBone 선택이 가능한가
			//거기에 맞게 처리
			apEditorUtil.ReleaseGUIFocus();

			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _meshGroup == null
				|| _modifier == null
				|| _paramSetOfMod == null
				|| _subEditedParamSetGroup == null
				)
			{
				//아무것도 선택하지 못할 경우
				_modMeshOfMod = null;
				_modBoneOfMod = null;
				//_modVertOfMod = null;
				_renderUnitOfMod = null;
				//_renderVertOfMod = null;

				_modRegistableBones.Clear();

				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();
				//Debug.LogError("AutoSelectModMesh -> Clear 1");

				return;
			}

			//1. ModMesh부터 선택하자
			bool isModMeshSelected = false;

			if (_subMeshTransformInGroup != null || _subMeshGroupTransformInGroup != null)
			{
				//bool isModMeshValid = false;
				for (int i = 0; i < _paramSetOfMod._meshData.Count; i++)
				{
					apModifiedMesh modMesh = _paramSetOfMod._meshData[i];
					if (_subMeshTransformInGroup != null)
					{
						if (modMesh._transform_Mesh == _subMeshTransformInGroup)
						{
							if (SetModMeshOfModifier(modMesh))
							{
								//isModMeshValid = true;
								isModMeshSelected = true;
							}
							break;
						}
					}
					else if (_subMeshGroupTransformInGroup != null)
					{
						if (modMesh._transform_MeshGroup == _subMeshGroupTransformInGroup)
						{
							if (SetModMeshOfModifier(modMesh))
							{
								//isModMeshValid = true;
								isModMeshSelected = true;
							}
							break;
						}
					}
				}

				if (!isModMeshSelected)
				{
					//선택된 ModMesh가 없네용..
					_modMeshOfMod = null;
					_renderUnitOfMod = null;

					_modRenderVertOfMod = null;
					_modRenderVertListOfMod.Clear();
					_modRenderVertListOfMod_Weighted.Clear();
				}



				if (_subMeshTransformInGroup != null)
				{
					apRenderUnit nextSelectUnit = MeshGroup.GetRenderUnit(_subMeshTransformInGroup);
					if (nextSelectUnit != _renderUnitOfMod)
					{
						//_modVertOfMod = null;
						//_renderVertOfMod = null;
						_modRenderVertOfMod = null;
						_modRenderVertListOfMod.Clear();
						_modRenderVertListOfMod_Weighted.Clear();
					}
					_renderUnitOfMod = nextSelectUnit;
				}
				else if (_subMeshGroupTransformInGroup != null)
				{
					apRenderUnit nextSelectUnit = MeshGroup.GetRenderUnit(_subMeshGroupTransformInGroup);
					if (nextSelectUnit != _renderUnitOfMod)
					{
						//_modVertOfMod = null;
						//_renderVertOfMod = null;
						_modRenderVertOfMod = null;
						_modRenderVertListOfMod.Clear();
						_modRenderVertListOfMod_Weighted.Clear();
					}
					_renderUnitOfMod = nextSelectUnit;
				}
				else
				{
					_modMeshOfMod = null;
					//_modVertOfMod = null;
					_renderUnitOfMod = null;
					//_renderVertOfMod = null;

					_modRenderVertOfMod = null;
					_modRenderVertListOfMod.Clear();
					_modRenderVertListOfMod_Weighted.Clear();
					//Debug.LogError("AutoSelectModMesh -> Clear 2");
					isModMeshSelected = false;
				}
			}

			if (!isModMeshSelected)
			{
				_modMeshOfMod = null;
			}
			else
			{
				_modBoneOfMod = null;
			}

			//2. ModMesh 선택한게 없다면 ModBone을 선택해보자
			if (!isModMeshSelected)
			{
				_modBoneOfMod = null;

				if (Bone != null)
				{
					//선택한 Bone이 있다면
					for (int i = 0; i < _paramSetOfMod._boneData.Count; i++)
					{
						apModifiedBone modBone = _paramSetOfMod._boneData[i];
						if (modBone._bone == Bone)
						{
							if (SetModBoneOfModifier(modBone))
							{
								break;
							}
						}
					}
				}
			}

			//추가
			//ModBone으로 선택 가능한 Bone 리스트를 만들어준다.
			_modRegistableBones.Clear();

			for (int i = 0; i < _paramSetOfMod._boneData.Count; i++)
			{
				_modRegistableBones.Add(_paramSetOfMod._boneData[i]._bone);
			}



			//MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();
		}



		public bool SetModifierEditMode(EX_EDIT_KEY_VALUE editMode)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null)
			{
				_exEditKeyValue = EX_EDIT_KEY_VALUE.None;
				_exclusiveEditing = EX_EDIT.None;
				return false;
			}

			if (_exEditKeyValue != editMode)
			{
				_exclusiveEditing = EX_EDIT.None;
				_isLockExEditKey = false;

				if (MeshGroup != null)
				{
					//Exclusive 모두 해제
					MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
					RefreshMeshGroupExEditingFlags(MeshGroup, null, null, null, false);//<<추가
				}
			}
			_exEditKeyValue = editMode;

			RefreshModifierExclusiveEditing();//<<Mod Lock 갱신

			Editor.RefreshControllerAndHierarchy();

			return true;
		}


		/// <summary>
		/// Modifier 편집시 Mod Lock을 갱신한다.
		/// SetModifierExclusiveEditing() 함수를 호출하는 것과 같으나,
		/// Lock-Unlock이 전환되지는 않는다.
		/// </summary>
		public void RefreshModifierExclusiveEditing()
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _subEditedParamSetGroup == null
				|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None)
			{
				_exclusiveEditing = EX_EDIT.None;
			}


			SetModifierExclusiveEditing(_exclusiveEditing);
		}

		//모디파이어의 Exclusive Editing (Modifier Lock)
		public bool SetModifierExclusiveEditing(EX_EDIT exclusiveEditing)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _subEditedParamSetGroup == null
				|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None)
			{
				_exclusiveEditing = EX_EDIT.None;
				return false;
			}

			

			bool isExEditable = IsExEditable;
			if (MeshGroup == null || Modifier == null || SubEditedParamSetGroup == null)
			{
				isExEditable = false;
			}

			if (isExEditable)
			{
				_exclusiveEditing = exclusiveEditing;
			}
			else
			{
				_exclusiveEditing = EX_EDIT.None;
			}

			bool isModLock_ColorUpdate = Editor.GetModLockOption_ColorPreview(_exclusiveEditing);
			bool isModLock_OtherMod = Editor.GetModLockOption_CalculateIfNotAddedOther(_exclusiveEditing);

			//작업중인 Modifier 외에는 일부 제외를 하자
			switch (_exclusiveEditing)
			{
				case EX_EDIT.None:
					//모든 Modifier를 활성화한다.
					{
						if (MeshGroup != null)
						{
							//Exclusive 모두 해제
							MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
							Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
							RefreshMeshGroupExEditingFlags(MeshGroup, null, null, null, false);//<<추가
						}

						//_modVertOfMod = null;
						//_renderVertOfMod = null;

						_modRenderVertOfMod = null;
						_modRenderVertListOfMod.Clear();
						_modRenderVertListOfMod_Weighted.Clear();
					}
					break;

				case EX_EDIT.General_Edit:
					//연동 가능한 Modifier를 활성화한다. (Mod Unlock)
					MeshGroup._modifierStack.SetExclusiveModifierInEditingGeneral(_modifier, isModLock_ColorUpdate, isModLock_OtherMod);
					RefreshMeshGroupExEditingFlags(MeshGroup, _modifier, SubEditedParamSetGroup, null, false);//<<추가
					break;

				case EX_EDIT.ExOnly_Edit:
					//작업중인 Modifier만 활성화한다. (Mod Lock)
					MeshGroup._modifierStack.SetExclusiveModifierInEditing(_modifier, SubEditedParamSetGroup, isModLock_ColorUpdate);
					RefreshMeshGroupExEditingFlags(MeshGroup, _modifier, SubEditedParamSetGroup, null, false);//<<추가
					break;
			}

			Editor.RefreshControllerAndHierarchy();

			return true;
		}



		//Ex Bone 렌더링용 함수
		//많은 내용이 빠져있다.
		public bool SetModifierExclusiveEditing_Tmp(EX_EDIT exclusiveEditing)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _subEditedParamSetGroup == null
				|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None)
			{
				_exclusiveEditing = EX_EDIT.None;
				return false;
			}

			

			bool isExEditable = IsExEditable;
			if (MeshGroup == null || Modifier == null || SubEditedParamSetGroup == null)
			{
				isExEditable = false;
			}

			if (isExEditable)
			{
				_exclusiveEditing = exclusiveEditing;
			}
			else
			{
				_exclusiveEditing = EX_EDIT.None;
			}

			bool isModLock_ColorUpdate = Editor.GetModLockOption_ColorPreview(_exclusiveEditing);
			bool isModLock_OtherMod = Editor.GetModLockOption_CalculateIfNotAddedOther(_exclusiveEditing);

			//작업중인 Modifier 외에는 일부 제외를 하자
			switch (_exclusiveEditing)
			{
				case EX_EDIT.None:
					//모든 Modifier를 활성화한다.
					{
						if (MeshGroup != null)
						{
							//Exclusive 모두 해제
							MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
							//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
							RefreshMeshGroupExEditingFlags(MeshGroup, null, null, null, false);//<<추가
						}

						//_modVertOfMod = null;
						//_renderVertOfMod = null;

						//_modRenderVertOfMod = null;
						//_modRenderVertListOfMod.Clear();
						//_modRenderVertListOfMod_Weighted.Clear();
					}
					break;

				case EX_EDIT.General_Edit:
					//연동 가능한 Modifier를 활성화한다. (Mod Unlock)
					MeshGroup._modifierStack.SetExclusiveModifierInEditingGeneral(_modifier, isModLock_ColorUpdate, isModLock_OtherMod);
					RefreshMeshGroupExEditingFlags(MeshGroup, _modifier, SubEditedParamSetGroup, null, false);//<<추가
					break;

				case EX_EDIT.ExOnly_Edit:
					//작업중인 Modifier만 활성화한다. (Mod Lock)
					MeshGroup._modifierStack.SetExclusiveModifierInEditing(_modifier, SubEditedParamSetGroup, isModLock_ColorUpdate);
					RefreshMeshGroupExEditingFlags(MeshGroup, _modifier, SubEditedParamSetGroup, null, false);//<<추가
					break;
			}

			//Editor.RefreshControllerAndHierarchy();

			return true;
		}


		/// <summary>
		/// 특정 메시그룹의 RenderUnit과 Bone의 Ex Edit에 대한 Flag를 갱신한다.
		/// Ex Edit가 변경되는 모든 시점에서 이 함수를 호출한다.
		/// AnimClip이 선택되어 있다면 animClip이 null이 아닌 값을 넣어준다.
		/// AnimClip이 없다면 ParamSetGroup이 있어야 한다. (Static 타입 제외)
		/// 둘다 null이라면 Ex Edit가 아닌 것으로 처리한다.
		/// Child MeshGroup으로 재귀적으로 호출한다.
		/// </summary>
		/// <param name="targetModifier"></param>
		/// <param name="targetAnimClip"></param>
		public void RefreshMeshGroupExEditingFlags(	apMeshGroup targetMeshGroup, 
													apModifierBase targetModifier, 
													apModifierParamSetGroup targetParamSetGroup, 
													apAnimClip targetAnimClip, 
													bool isForce,
													bool isRecursiveCall = false)
		{
			//재귀적인 호출이 아니라면
			if (!isRecursiveCall)
			{
				if (!isForce)
				{
					if (targetMeshGroup == _prevExFlag_MeshGroup
						&& targetModifier == _prevExFlag_Modifier
						&& targetParamSetGroup == _prevExFlag_ParamSetGroup
						&& targetAnimClip == _prevExFlag_AnimClip)
					{
						//바뀐게 없다.
						// 전부 null이라면 => 그래도 실행
						// 하나라도 null이 아니라면 => 중복 실행이다.
						if (targetMeshGroup != null ||
							targetModifier != null ||
							targetParamSetGroup != null ||
							targetAnimClip != null)
						{
							//Debug.LogError("중복 요청");
							return;
						}
					}
				}

				_prevExFlag_MeshGroup = targetMeshGroup;
				_prevExFlag_Modifier = targetModifier;
				_prevExFlag_ParamSetGroup = targetParamSetGroup;
				_prevExFlag_AnimClip = targetAnimClip;
			}

			//Debug.Log("RefreshMeshGroupExEditingFlags "
			//	+ "- Mod (" + (targetModifier != null ? "O" : "X") + ")"
			//	+ " / PSG (" + (targetParamSetGroup != null ? "O" : "X") + ")"
			//	+ " / Anim (" + (targetAnimClip != null ? "O" : "X") + ")"
			//	);

			if(targetMeshGroup == null)
			{
				//Debug.LogError("Target MeshGroup is Null");
				return;
			}
			apRenderUnit renderUnit = null;
			apBone bone = null;
			apModifierParamSetGroup paramSetGroup = null;
			bool isExMode = (targetModifier != null);
			bool isMeshTF = false;
			bool isMeshGroupTF = false;

			//if (targetModifier != null)
			//{
			//	Debug.Log("Is Animated : " + targetModifier.IsAnimated);
			//}

			//RenderUnit (MeshTransform / MeshGroupTransform)을 체크하자
			for (int i = 0; i < targetMeshGroup._renderUnits_All.Count; i++)
			{
				renderUnit = targetMeshGroup._renderUnits_All[i];

				isMeshTF = (renderUnit._meshTransform != null);
				isMeshGroupTF = (renderUnit._meshGroupTransform != null);

				if (!isExMode)
				{
					//Ex Mode가 아니다. (기본값)
					renderUnit._exCalculateMode = apRenderUnit.EX_CALCULATE.Normal;
					//Debug.Log("Render Unit : " + renderUnit.Name + " -- Normal");
				}
				else
				{
					//Ex Mode이다.
					//(포함 여부 체크해야함)
					bool isContained = false;
					for (int iPSG = 0; iPSG < targetModifier._paramSetGroup_controller.Count; iPSG++)
					{
						paramSetGroup = targetModifier._paramSetGroup_controller[iPSG];
						if (targetModifier.IsAnimated)
						{
							if (targetAnimClip != null && paramSetGroup._keyAnimClip != targetAnimClip)
							{
								//AnimClip 타입의 Modifier의 경우, 
								//현재 AnimClip과 같아야 한다.
								continue;
							}
						}
						else if (targetModifier.SyncTarget != apModifierParamSetGroup.SYNC_TARGET.Static)
						{
							//AnimType은 아니고 Static 타입도 아닌 경우
							if(targetParamSetGroup != null && paramSetGroup != targetParamSetGroup)
							{
								//ParamSetGroup이 다르다면 패스
								continue;
							}
						}
						

						if(isMeshTF)
						{
							if(paramSetGroup.IsMeshTransformContain(renderUnit._meshTransform))
							{
								//MeshTransform이 포함된당.
								isContained = true;
								break;
							}
						}
						else if(isMeshGroupTF)
						{
							if(paramSetGroup.IsMeshGroupTransformContain(renderUnit._meshGroupTransform))
							{
								//MeshGroupTransform이 포함된당.
								isContained = true;
								break;
							}
						}
						
					}

					if(isContained)
					{
						//ExEdit에 포함되었다.
						renderUnit._exCalculateMode = apRenderUnit.EX_CALCULATE.ExAdded;
						//Debug.Log("Render Unit : " + renderUnit.Name + " -- ExAdded");
					}
					else
					{
						//ExEdit에 포함되지 않았다.
						renderUnit._exCalculateMode = apRenderUnit.EX_CALCULATE.ExNotAdded;
						//Debug.Log("Render Unit : " + renderUnit.Name + " -- Ex Not Added");
					}
				}
			}

			//Bone도 체크하자
			for (int i = 0; i < targetMeshGroup._boneList_All.Count; i++)
			{
				bone = targetMeshGroup._boneList_All[i];

				if (!isExMode)
				{
					//Ex Mode가 아니다. (기본값)
					bone._exCalculateMode = apBone.EX_CALCULATE.Normal;
				}
				else
				{
					//Ex Mode이다.
					//(포함 여부 체크해야함)
					bool isContained = false;
					for (int iPSG = 0; iPSG < targetModifier._paramSetGroup_controller.Count; iPSG++)
					{
						paramSetGroup = targetModifier._paramSetGroup_controller[iPSG];
						if(targetModifier.IsAnimated && targetAnimClip != null && paramSetGroup._keyAnimClip != targetAnimClip)
						{
							//AnimClip 타입의 Modifier의 경우, 
							//현재 AnimClip과 같아야 한다.
							continue;
						}

						if(paramSetGroup.IsBoneContain(bone))
						{
							//Bone이 포함된당.
							isContained = true;
							break;
						}
					}

					if(isContained)
					{
						//ExEdit에 포함되었다.
						bone._exCalculateMode = apBone.EX_CALCULATE.ExAdded;
					}
					else
					{
						//ExEdit에 포함되지 않았다.
						bone._exCalculateMode = apBone.EX_CALCULATE.ExNotAdded;
					}
				}
			}

			if(targetMeshGroup._childMeshGroupTransforms != null &&
				targetMeshGroup._childMeshGroupTransforms.Count > 0)
			{
				for (int i = 0; i < targetMeshGroup._childMeshGroupTransforms.Count; i++)
				{
					apMeshGroup childMeshGroup = targetMeshGroup._childMeshGroupTransforms[i]._meshGroup;
					if(childMeshGroup != targetMeshGroup)
					{
						RefreshMeshGroupExEditingFlags(childMeshGroup, targetModifier, targetParamSetGroup, targetAnimClip, isForce, true);
					}
				}
			}
			
					
		}








		/// <summary>
		/// 단축키 [A]를 눌러서 Editing 상태를 토글하자
		/// </summary>
		/// <param name="paramObject"></param>
		public void OnHotKeyEvent_ToggleModifierEditing(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None)
			{
				return;
			}

			bool isRiggingModifier = (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging);

			if (isRiggingModifier)
			{
				//1. Rigging 타입의 Modifier인 경우
				_rigEdit_isBindingEdit = !_rigEdit_isBindingEdit;
				_rigEdit_isTestPosing = false;

				//작업중인 Modifier 외에는 일부 제외를 하자
				if (_rigEdit_isBindingEdit)
				{
					MeshGroup._modifierStack.SetExclusiveModifierInEditing(_modifier, SubEditedParamSetGroup, false);
					_isLockExEditKey = true;
				}
				else
				{
					if (MeshGroup != null)
					{
						//Exclusive 모두 해제
						MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
						Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
						RefreshMeshGroupExEditingFlags(MeshGroup, null, null, null, false);//<<추가
					}
					_isLockExEditKey = false;
				}
			}
			else
			{
				//2. 일반 Modifier일때
				EX_EDIT nextResult = EX_EDIT.None;
				if (_exclusiveEditing == EX_EDIT.None && IsExEditable)
				{
					//None -> ExOnly로 바꾼다.
					//General은 특별한 경우
					nextResult = EX_EDIT.ExOnly_Edit;
				}
				//if (IsExEditable || !isNextResult)
				//{
				//	//SetModifierExclusiveEditing(isNextResult);
				//}
				SetModifierExclusiveEditing(nextResult);
				if (nextResult == EX_EDIT.ExOnly_Edit)
				{
					_isLockExEditKey = true;//처음 Editing 작업시 Lock을 거는 것으로 변경
				}
				else
				{
					_isLockExEditKey = false;//Editing 해제시 Lock 해제
				}
			}


			Editor.RefreshControllerAndHierarchy();
		}

		/// <summary>
		/// 단축키 [S]에 의해서도 SelectionLock(Modifier)를 바꿀 수 있다.
		/// </summary>
		public void OnHotKeyEvent_ToggleExclusiveEditKeyLock(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None)
			{
				return;
			}
			_isLockExEditKey = !_isLockExEditKey;
		}

		/// <summary>
		/// 단축키 [D]에 의해서 ModifierLock(Modifier)을 바꿀 수 있다.
		/// </summary>
		/// <param name="paramObject"></param>
		public void OnHotKeyEvent_ToggleExclusiveModifierLock(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None)
			{
				return;
			}

			if (IsExEditable && _exclusiveEditing != EX_EDIT.None)
			{
				//None이 아닐때
				//General <-> Exclusive 사이에서 토글
				EX_EDIT nextEditMode = EX_EDIT.ExOnly_Edit;
				if (_exclusiveEditing == EX_EDIT.ExOnly_Edit)
				{
					nextEditMode = EX_EDIT.General_Edit;
				}
				SetModifierExclusiveEditing(nextEditMode);//<<이거 수정해야한다.
			}
		}





		public void SetModifierExclusiveEditKeyLock(bool isLock)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None)
			{
				_isLockExEditKey = false;
				return;
			}
			_isLockExEditKey = isLock;
		}

		//이 함수가 호출되면 첫번째 RootUnit을 자동으로 호출한다.
		public void SetOverallDefault()
		{
			if(_portrait == null)
			{
				return;
			}

			if(_portrait._rootUnits.Count == 0)
			{
				SetNone();

				Editor.Gizmos.Unlink();
				Editor.RefreshControllerAndHierarchy();
				Editor.RefreshTimelineLayers(true);
				return;
			}

			//첫번째 유닛을 호출
			SetOverall(_portrait._rootUnits[0]);
		}

		public void SetOverall(apRootUnit rootUnit)
		{
			SetNone();

			if (_rootUnit != rootUnit)
			{
				_curRootUnitAnimClip = null;
			}

			_rootUnitAnimClips.Clear();

			_selectionType = SELECTION_TYPE.Overall;

			if (rootUnit != null)
			{
				_rootUnit = rootUnit;

				//이 RootUnit에 적용할 AnimClip이 뭐가 있는지 확인하자
				for (int i = 0; i < _portrait._animClips.Count; i++)
				{
					apAnimClip animClip = _portrait._animClips[i];
					if (_rootUnit._childMeshGroup == animClip._targetMeshGroup)
					{
						_rootUnitAnimClips.Add(animClip);//<<연동되는 AnimClip이다.
					}
				}

				if (_rootUnit._childMeshGroup != null)
				{
					//Mesh Group을 선택하면 이 초기화를 전부 실행해야한다.
					_rootUnit._childMeshGroup.SetDirtyToReset();
					_rootUnit._childMeshGroup.SetDirtyToSort();
					//_rootUnit._childMeshGroup.SetAllRenderUnitForceUpdate();
					_rootUnit._childMeshGroup.RefreshForce(true);

					_rootUnit._childMeshGroup.LinkModMeshRenderUnits();
					_rootUnit._childMeshGroup.RefreshModifierLink();
					_rootUnit._childMeshGroup._modifierStack.InitModifierCalculatedValues();//<<값 초기화

					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_rootUnit._childMeshGroup);

					_rootUnit._childMeshGroup._modifierStack.RefreshAndSort(true);


				}
			}

			if (_curRootUnitAnimClip != null)
			{
				if (!_rootUnitAnimClips.Contains(_curRootUnitAnimClip))
				{
					_curRootUnitAnimClip = null;//<<이건 포함되지 않습니더
				}
			}

			if (_curRootUnitAnimClip != null)
			{
				_curRootUnitAnimClip._isSelectedInEditor = true;
			}

			Editor.Gizmos.Unlink();

			//통계 재계산 요청
			SetStatisticsRefresh();

		}





		public void SetParam(apControlParam controlParam)
		{
			SetNone();

			_selectionType = SELECTION_TYPE.Param;

			_param = controlParam;

			//통계 재계산 요청
			SetStatisticsRefresh();
		}

		public void SetAnimClip(apAnimClip animClip)
		{
			SetNone();

			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip != animClip
				|| _animClip == null)
			{
				Editor.RefreshTimelineLayers(false);
			}

			bool isResetInfo = false;

			for (int i = 0; i < Editor._portrait._animClips.Count; i++)
			{
				Editor._portrait._animClips[i]._isSelectedInEditor = false;
			}

			bool isChanged = false;
			if (_animClip != animClip)
			{
				_animClip = animClip;

				

				_subAnimTimeline = null;
				_subAnimTimelineLayer = null;
				_subAnimKeyframe = null;
				_subAnimWorkKeyframe = null;

				_subMeshTransformOnAnimClip = null;
				_subMeshGroupTransformOnAnimClip = null;
				_subControlParamOnAnimClip = null;

				_subAnimKeyframeList.Clear();
				//_isAnimEditing = false;
				_exAnimEditingMode = EX_EDIT.None;
				//_isAnimAutoKey = false;
				_isAnimLock = false;

				_subAnimCommonKeyframeList.Clear();
				_subAnimCommonKeyframeList_Selected.Clear();

				_modMeshOfAnim = null;
				_modBoneOfAnim = null;
				_renderUnitOfAnim = null;
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();

				isResetInfo = true;
				isChanged = true;


				if (_animClip._targetMeshGroup != null)
				{
					//Mesh Group을 선택하면 이 초기화를 전부 실행해야한다.
					_animClip._targetMeshGroup.SetDirtyToReset();
					_animClip._targetMeshGroup.SetDirtyToSort();
					//_animClip._targetMeshGroup.SetAllRenderUnitForceUpdate();
					_animClip._targetMeshGroup.RefreshForce(true);

					_animClip._targetMeshGroup.LinkModMeshRenderUnits();
					_animClip._targetMeshGroup.RefreshModifierLink();
					_animClip._targetMeshGroup._modifierStack.InitModifierCalculatedValues();//<<값 초기화

					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_animClip._targetMeshGroup);

					_animClip._targetMeshGroup._modifierStack.RefreshAndSort(true);

					_animClip._targetMeshGroup.ResetBoneGUIVisible();
				}

				_animClip.Pause_Editor();

				Editor.Gizmos.RefreshFFDTransformForce();

			}
			else
			{
				//같은 거라면?
				//패스
			}
			_animClip = animClip;
			_animClip._isSelectedInEditor = true;

			_selectionType = SELECTION_TYPE.Animation;
			//_prevAnimClipName = _animClip._name;

			if(isChanged && _animClip != null)
			{
				//타임라인을 자동으로 선택해주자
				if (_animClip._timelines.Count > 0)
				{
					apAnimTimeline firstTimeline = _animClip._timelines[0];
					SetAnimTimeline(firstTimeline, true, true, false);
				}
			}

			AutoSelectAnimWorkKeyframe();

			if (isResetInfo)
			{
				//Sync를 한번 돌려주자
				_animPropertyUI = ANIM_SINGLE_PROPERTY_UI.Value;
				_animPropertyCurveUI = ANIM_SINGLE_PROPERTY_CURVE_UI.Next;
				Editor.Controller.AddAndSyncAnimClipToModifier(_animClip);
			}

			Editor.RefreshTimelineLayers(isResetInfo);

			Editor.Hierarchy_AnimClip.ResetSubUnits();
			Editor.Hierarchy_AnimClip.RefreshUnits();

			//Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_Morph());

			SetAnimClipGizmoEvent(isResetInfo);//Gizmo 이벤트 연결

			//통계 재계산 요청
			SetStatisticsRefresh();
			
			//Common Keyframe을 갱신하자
			RefreshCommonAnimKeyframes();
		}

		/// <summary>
		/// AnimClip 상태에서 현재 상태에 맞는 GizmoEvent를 등록한다.
		/// </summary>
		private void SetAnimClipGizmoEvent(bool isForceReset)
		{
			if (_animClip == null)
			{
				Editor.Gizmos.Unlink();
				return;
			}

			if (isForceReset)
			{
				Editor.Gizmos.Unlink();

			}

			if (AnimTimeline == null)
			{
				//타임라인이 없으면 선택만 가능하다
				Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_OnlySelectTransform());
			}
			else
			{
				switch (AnimTimeline._linkType)
				{
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						if (AnimTimeline._linkedModifier != null)
						{
							if ((int)(AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
							{
								//Vertex와 관련된 Modifier다.
								Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_EditVertex());
							}
							else
							{
								//Transform과 관련된 Modifier다.
								Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_EditTransform());
							}
						}
						else
						{
							Debug.LogError("Error : 선택된 Timeline의 Modifier가 연결되지 않음");
							Editor.Gizmos.Unlink();
						}
						break;

					//이거 삭제하고, 
					//GetEventSet__Animation_EditTransform에서 Bone을 제어하는 코드를 추가하자
					//case apAnimClip.LINK_TYPE.Bone:
					//	Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_EditBone());
					//	break;

					case apAnimClip.LINK_TYPE.ControlParam:
						//Control Param일땐 선택만 가능
						Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_OnlySelectTransform());
						break;

					default:
						Debug.LogError("TODO : 알 수 없는 Timeline LinkType [" + AnimTimeline._linkType + "]");
						Editor.Gizmos.Unlink();
						break;
				}
			}

		}


		/// <summary>
		/// Animation 편집시 - AnimClip -> Timeline 을 선택한다. (단일 선택)
		/// </summary>
		/// <param name="timeLine"></param>
		public void SetAnimTimeline(	apAnimTimeline timeLine, 
										bool isKeyframeSelectReset, 
										bool isIgnoreLock = false,
										bool isAutoChangeLeftTab = true)
		{
			//통계 재계산 요청
			SetStatisticsRefresh();

			if (!isIgnoreLock)
			{
				//현재 작업중 + Lock이 걸리면 바꾸지 못한다.
				if (ExAnimEditingMode != EX_EDIT.None && IsAnimSelectionLock)
				{
					return;
				}
			}



			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null ||
				timeLine == null ||
				!_animClip.IsTimelineContain(timeLine))
			{
				_subAnimTimeline = null;
				_subAnimTimelineLayer = null;
				_subAnimKeyframe = null;
				_subAnimWorkKeyframe = null;

				_subAnimKeyframeList.Clear();
				_exAnimEditingMode = EX_EDIT.None;
				//_isAnimAutoKey = false;
				_isAnimLock = false;

				_modMeshOfAnim = null;
				_modBoneOfAnim = null;
				_renderUnitOfAnim = null;
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();

				AutoSelectAnimWorkKeyframe();
				RefreshAnimEditing(true);

				Editor.RefreshTimelineLayers(false);
				SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결

				//우측 Hierarchy GUI 변동이 있을 수 있으니 리셋
				Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Meshes", false);
				Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Bone", false);
				Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - ControlParam", false);
				//Editor.SetGUIVisible("AnimationRight2GUI_Timeline_Layers", false);

				return;
			}

			if (_subAnimTimeline != timeLine)
			{
				_subAnimTimelineLayer = null;
				_subAnimWorkKeyframe = null;

				if (isKeyframeSelectReset)
				{
					_subAnimKeyframe = null;

					_subAnimKeyframeList.Clear();
				}

				_modMeshOfAnim = null;
				_modBoneOfAnim = null;
				_renderUnitOfAnim = null;
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();

				AutoSelectAnimWorkKeyframe();

				//Editing에서 바꿀 수 있으므로 AnimEditing를 갱신한다.
				RefreshAnimEditing(true);

				//스크롤 초기화 (오른쪽2)
				Editor.ResetScrollPosition(false, false, false, true, false);
			}

			_subAnimTimeline = timeLine;


			AutoSelectAnimTimelineLayer(isAutoChangeLeftTab);

			Editor.RefreshTimelineLayers(false);

			SetAnimClipGizmoEvent(false);//Gizmo 이벤트 연결

			//추가 : MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();
			Editor.Hierarchy_AnimClip.RefreshUnits();

			//우측 Hierarchy GUI 변동이 있을 수 있으니 리셋
			Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Meshes", false);
			Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Bone", false);
			Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - ControlParam", false);
			//Editor.SetGUIVisible("AnimationRight2GUI_Timeline_Layers", false);

			apEditorUtil.ReleaseGUIFocus();
		}

		public void SetAnimTimelineLayer(apAnimTimelineLayer timelineLayer, bool isKeyframeSelectReset, bool isAutoSelectTargetObject = false, bool isIgnoreLock = false)
		{
			apAnimTimeline prevTimeline = _subAnimTimeline;

			//처리 후 이전 레이어
			//통계 재계산 요청
			SetStatisticsRefresh();

			//현재 작업중+Lock이 걸리면 바꾸지 못한다.
			if (!isIgnoreLock)
			{
				if (ExAnimEditingMode != EX_EDIT.None && IsAnimSelectionLock)
				{
					return;
				}
			}

			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null ||
				_subAnimTimeline == null ||
				timelineLayer == null ||
				!_subAnimTimeline.IsTimelineLayerContain(timelineLayer)
				)
			{
				_subAnimTimelineLayer = null;
				_subAnimKeyframe = null;

				_subAnimKeyframeList.Clear();

				_modMeshOfAnim = null;
				_modBoneOfAnim = null;
				_renderUnitOfAnim = null;
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();

				AutoSelectAnimWorkKeyframe();

				//Editing에서 바꿀 수 있으므로 AnimEditing를 갱신한다.
				RefreshAnimEditing(true);

				Editor.RefreshTimelineLayers(false);
				SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결

				return;
			}

			

			if (_subAnimTimelineLayer != timelineLayer && isKeyframeSelectReset)
			{
				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				_subAnimTimelineLayer = timelineLayer;

				

				AutoSelectAnimWorkKeyframe();

				RefreshAnimEditing(true);
			}

			_subAnimTimelineLayer = timelineLayer;

			if (isAutoSelectTargetObject)
			{
				AutoSelectAnimTargetObject();
			}

			Editor.RefreshTimelineLayers(false);

			SetAnimClipGizmoEvent(false);//Gizmo 이벤트 연결

			//만약 처리 이전-이후의 타임라인이 그대로라면 GUI가 깜빡이는걸 막자
			if(prevTimeline == _subAnimTimeline)
			{
				_isIgnoreAnimTimelineGUI = true;
			}

			apEditorUtil.ReleaseGUIFocus();
		}

		/// <summary>
		/// Timeline GUI에서 Keyframe을 선택한다.
		/// AutoSelect를 켜면 선택한 Keyframe에 맞게 다른 TimelineLayer / Timeline을 선택한다.
		/// 단일 선택이므로 "다중 선택"은 항상 현재 선택한 것만 가지도록 한다.
		/// </summary>
		/// <param name="keyframe"></param>
		/// <param name="isTimelineAutoSelect"></param>
		public void SetAnimKeyframe(apAnimKeyframe keyframe, bool isTimelineAutoSelect, apGizmos.SELECT_TYPE selectType, bool isSelectLoopDummy = false)
		{
			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null)
			{
				_subAnimTimeline = null;
				_subAnimTimelineLayer = null;
				_subAnimKeyframe = null;

				_subAnimKeyframeList.Clear();

				AutoSelectAnimWorkKeyframe();//<Work+Mod 자동 연결

				SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결

				Editor.RefreshTimelineLayers(false);
				return;
			}

			apAnimTimeline prevTimeline = _subAnimTimeline;

			if (selectType != apGizmos.SELECT_TYPE.New)
			{
				List<apAnimKeyframe> singleKeyframes = new List<apAnimKeyframe>();
				if (keyframe != null)
				{
					singleKeyframes.Add(keyframe);
				}

				SetAnimMultipleKeyframe(singleKeyframes, selectType, isTimelineAutoSelect);
				return;
			}

			if (keyframe == null)
			{
				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결
				return;
			}

			bool isKeyframeChanged = (keyframe != _subAnimKeyframe);

			if (isTimelineAutoSelect)
			{

				//Layer가 선택되지 않았거나, 선택된 Layer에 포함되지 않을 때
				apAnimTimelineLayer parentLayer = keyframe._parentTimelineLayer;
				if (parentLayer == null)
				{
					_subAnimKeyframe = null;
					_subAnimKeyframeList.Clear();

					AutoSelectAnimWorkKeyframe();
					return;
				}
				apAnimTimeline parentTimeline = parentLayer._parentTimeline;
				if (parentTimeline == null || !_animClip.IsTimelineContain(parentTimeline))
				{
					//유효하지 않은 타임라인일때
					_subAnimKeyframe = null;
					_subAnimKeyframeList.Clear();

					SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결

					AutoSelectAnimWorkKeyframe();
					return;
				}

				//자동으로 체크해주자
				_subAnimTimeline = parentTimeline;
				_subAnimTimelineLayer = parentLayer;

				_subAnimKeyframe = keyframe;

				_subAnimKeyframeList.Clear();
				_subAnimKeyframeList.Add(keyframe);

				
				AutoSelectAnimWorkKeyframe();
				SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결

				Editor.RefreshTimelineLayers(false);
			}
			else
			{
				//TimelineLayer에 있는 키프레임만 선택할 때
				if (_subAnimTimeline == null ||
					_subAnimTimelineLayer == null)
				{
					_subAnimKeyframe = null;
					_subAnimKeyframeList.Clear();

					SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결
					return;//처리 못함
				}


				if (_subAnimTimelineLayer.IsKeyframeContain(keyframe))
				{
					//Layer에 포함된 Keyframe이다.

					_subAnimKeyframe = keyframe;
					_subAnimKeyframeList.Clear();
					_subAnimKeyframeList.Add(_subAnimKeyframe);
				}
				else
				{
					//Layer에 포함되지 않은 Keyframe이다. => 처리 못함
					_subAnimKeyframe = null;
					_subAnimKeyframeList.Clear();
				}
				SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결
				

			}

			_subAnimKeyframe._parentTimelineLayer.SortAndRefreshKeyframes();

			
			//키프레임 선택시 자동으로 Frame을 이동한다.
			if (_subAnimKeyframe != null)
			{
				int selectedFrameIndex = _subAnimKeyframe._frameIndex;
				if (_animClip.IsLoop &&
					(selectedFrameIndex < _animClip.StartFrame || selectedFrameIndex > _animClip.EndFrame))
				{
					selectedFrameIndex = _subAnimKeyframe._loopFrameIndex;
				}

				if (selectedFrameIndex >= _animClip.StartFrame
					&& selectedFrameIndex <= _animClip.EndFrame)
				{
					_animClip.SetFrame_Editor(selectedFrameIndex);
				}
				
				
				SetAutoAnimScroll();
			}

			if(isKeyframeChanged)
			{
				AutoSelectAnimTargetObject();
			}

			AutoSelectAnimWorkKeyframe();//<Work+Mod 자동 연결
			SetAnimClipGizmoEvent(isKeyframeChanged);//Gizmo 이벤트 연결

			//Common Keyframe을 갱신하자
			RefreshCommonAnimKeyframes();

			//만약 처리 이전-이후의 타임라인이 그대로라면 GUI가 깜빡이는걸 막자
			if(prevTimeline == _subAnimTimeline)
			{
				_isIgnoreAnimTimelineGUI = true;
			}

			apEditorUtil.ReleaseGUIFocus();
		}


		/// <summary>
		/// Keyframe 다중 선택을 한다.
		/// 이때는 Timeline, Timelinelayer는 변동이 되지 않는다. (다만 다중 선택시에는 Timeline, Timelinelayer를 별도로 수정하지 못한다)
		/// </summary>
		/// <param name="keyframes"></param>
		/// <param name="selectType"></param>
		public void SetAnimMultipleKeyframe(List<apAnimKeyframe> keyframes, apGizmos.SELECT_TYPE selectType, bool isTimelineAutoSelect)
		{
			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null)
			{
				_subAnimTimeline = null;
				_subAnimTimelineLayer = null;
				_subAnimKeyframe = null;

				_subAnimKeyframeList.Clear();

				AutoSelectAnimWorkKeyframe();//<Work+Mod 자동 연결

				SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결
				return;
			}

			apAnimKeyframe curKeyframe = null;
			if (selectType == apGizmos.SELECT_TYPE.New)
			{
				_subAnimWorkKeyframe = null;
				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();
			}


			//공통의 타임라인을 가지는가
			apAnimTimeline commonTimeline = null;
			apAnimTimelineLayer commonTimelineLayer = null;



			if (isTimelineAutoSelect)
			{
				List<apAnimKeyframe> checkCommonKeyframes = new List<apAnimKeyframe>();
				if (selectType == apGizmos.SELECT_TYPE.Add ||
					selectType == apGizmos.SELECT_TYPE.New)
				{
					for (int i = 0; i < keyframes.Count; i++)
					{
						checkCommonKeyframes.Add(keyframes[i]);
					}
				}

				if (selectType == apGizmos.SELECT_TYPE.Add ||
					selectType == apGizmos.SELECT_TYPE.Subtract)
				{
					//기존에 선택했던것도 추가하자
					for (int i = 0; i < _subAnimKeyframeList.Count; i++)
					{
						checkCommonKeyframes.Add(_subAnimKeyframeList[i]);
					}
				}

				if (selectType == apGizmos.SELECT_TYPE.Subtract)
				{
					//기존에 선택했던 것에서 빼자
					for (int i = 0; i < keyframes.Count; i++)
					{
						checkCommonKeyframes.Remove(keyframes[i]);
					}
				}


				for (int i = 0; i < checkCommonKeyframes.Count; i++)
				{
					curKeyframe = checkCommonKeyframes[i];
					if (commonTimelineLayer == null)
					{
						commonTimelineLayer = curKeyframe._parentTimelineLayer;
						commonTimeline = commonTimelineLayer._parentTimeline;
					}
					else
					{
						if (commonTimelineLayer != curKeyframe._parentTimelineLayer)
						{
							commonTimelineLayer = null;
							break;
						}
					}
				}
			}

			for (int i = 0; i < keyframes.Count; i++)
			{
				curKeyframe = keyframes[i];
				if (curKeyframe == null ||
					curKeyframe._parentTimelineLayer == null ||
					curKeyframe._parentTimelineLayer._parentAnimClip != _animClip)
				{
					continue;
				}

				if (selectType == apGizmos.SELECT_TYPE.Add ||
					selectType == apGizmos.SELECT_TYPE.New)
				{
					//Debug.Log("Add");
					if(!_subAnimKeyframeList.Contains(curKeyframe))
					{
						_subAnimKeyframeList.Add(curKeyframe);
					}
				}
				else
				{
					_subAnimKeyframeList.Remove(curKeyframe);
				}
			}

			if (_subAnimKeyframeList.Count > 0)
			{
				if (!_subAnimKeyframeList.Contains(_subAnimKeyframe))
				{
					_subAnimKeyframe = _subAnimKeyframeList[0];
				}
			}
			else
			{
				_subAnimKeyframe = null;
			}

			if (isTimelineAutoSelect)
			{

				if (commonTimelineLayer != null)
				{
					if (commonTimelineLayer != _subAnimTimelineLayer)
					{
						_subAnimTimelineLayer = commonTimelineLayer;

						if (ExAnimEditingMode == EX_EDIT.None)
						{
							_subAnimTimeline = commonTimeline;
						}

						Editor.RefreshTimelineLayers(false);
					}
				}
				else
				{
					_subAnimTimelineLayer = null;
					if (ExAnimEditingMode == EX_EDIT.None)
					{
						_subAnimTimeline = null;
					}

					Editor.RefreshTimelineLayers(false);
				}

				
			}
			else
			{
				Editor.RefreshTimelineLayers(false);
			}

			List<apAnimTimelineLayer> refreshLayer = new List<apAnimTimelineLayer>();
			for (int i = 0; i < _subAnimKeyframeList.Count; i++)
			{
				if (!refreshLayer.Contains(_subAnimKeyframeList[i]._parentTimelineLayer))
				{
					refreshLayer.Add(_subAnimKeyframeList[i]._parentTimelineLayer);
				}
			}
			for (int i = 0; i < refreshLayer.Count; i++)
			{
				refreshLayer[i].SortAndRefreshKeyframes();
			}



			//키프레임 선택시 자동으로 Frame을 이동한다.
			//단, 공통 프레임이 있는 경우에만 이동한다.
			if (_subAnimKeyframeList.Count > 0 && selectType == apGizmos.SELECT_TYPE.New)
			{
				bool isCommonKeyframe = true;
				
				int selectedFrameIndex = -1;
				for (int iKey = 0; iKey < _subAnimKeyframeList.Count; iKey++)
				{
					apAnimKeyframe subKeyframe = _subAnimKeyframeList[iKey];
					if(iKey == 0)
					{
						selectedFrameIndex = subKeyframe._frameIndex;
						isCommonKeyframe = true;
					}
					else
					{
						if(subKeyframe._frameIndex != selectedFrameIndex)
						{
							//선택한 키프레임이 다 다르군요. 자동 이동 포기
							isCommonKeyframe = false;
							break;
						}
					}

				}
				if (isCommonKeyframe)
				{
					//모든 키프레임이 공통의 프레임을 갖는다.
					//이동하자
					if (_animClip.IsLoop &&
						(selectedFrameIndex < _animClip.StartFrame || selectedFrameIndex > _animClip.EndFrame))
					{
						selectedFrameIndex = _subAnimKeyframe._loopFrameIndex;
					}

					if (selectedFrameIndex >= _animClip.StartFrame
						&& selectedFrameIndex <= _animClip.EndFrame)
					{
						_animClip.SetFrame_Editor(selectedFrameIndex);
					}


					SetAutoAnimScroll();
				}
			}

			
			AutoSelectAnimTargetObject();

			AutoSelectAnimWorkKeyframe();//<Work+Mod 자동 연결
			
			SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결

			//Common Keyframe을 갱신하자
			RefreshCommonAnimKeyframes();

			apEditorUtil.ReleaseGUIFocus();
		}



		private void AutoSelectAnimTargetObject()
		{
			//자동으로 타겟을 정하자
			_subControlParamOnAnimClip = null;
			_subMeshTransformOnAnimClip = null;
			_subMeshGroupTransformOnAnimClip = null;


			if (_subAnimTimelineLayer != null && _subAnimTimelineLayer._parentTimeline != null)
			{
				apAnimTimeline parentTimeline = _subAnimTimelineLayer._parentTimeline;
				switch (parentTimeline._linkType)
				{
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						{
							switch (_subAnimTimelineLayer._linkModType)
							{
								case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
									if (_subAnimTimelineLayer._linkedMeshTransform != null)
									{
										_subMeshTransformOnAnimClip = _subAnimTimelineLayer._linkedMeshTransform;
									}
									break;

								case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
									if (_subAnimTimelineLayer._linkedMeshGroupTransform != null)
									{
										_subMeshGroupTransformOnAnimClip = _subAnimTimelineLayer._linkedMeshGroupTransform;
									}
									break;

								case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
									if (_subAnimTimelineLayer._linkedBone != null)
									{
										_bone = _subAnimTimelineLayer._linkedBone;
									}
									break;

								case apAnimTimelineLayer.LINK_MOD_TYPE.None:
									break;
							}
						}
						break;


					case apAnimClip.LINK_TYPE.ControlParam:
						if (_subAnimTimelineLayer._linkedControlParam != null)
						{
							_subControlParamOnAnimClip = _subAnimTimelineLayer._linkedControlParam;
						}
						break;

					default:
						Debug.LogError("에러 : 알 수 없는 타입 : [" + parentTimeline._linkType + "]");
						break;
				}
			}
		}

		//---------------------------------------------------------------

		/// <summary>
		/// Keyframe의 변동사항이 있을때 Common Keyframe을 갱신한다.
		/// </summary>
		public void RefreshCommonAnimKeyframes()
		{
			

			if(_animClip == null)
			{
				_subAnimCommonKeyframeList.Clear();
				_subAnimCommonKeyframeList_Selected.Clear();	
				return;
			}

			//0. 전체 Keyframe과 FrameIndex를 리스트로 모은다.
			List<int> commFrameIndexList = new List<int>();
			List<apAnimKeyframe> totalKeyframes = new List<apAnimKeyframe>();
			apAnimTimeline timeline = null;
			apAnimTimelineLayer timelineLayer = null;
			apAnimKeyframe keyframe = null;
			for (int iTimeline = 0; iTimeline < _animClip._timelines.Count; iTimeline++)
			{
				timeline = _animClip._timelines[iTimeline];
				for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
				{
					timelineLayer = timeline._layers[iLayer];
					for (int iKeyframe = 0; iKeyframe < timelineLayer._keyframes.Count; iKeyframe++)
					{
						keyframe = timelineLayer._keyframes[iKeyframe];

						//키프레임과 프레임 인덱스를 저장
						totalKeyframes.Add(keyframe);

						if(!commFrameIndexList.Contains(keyframe._frameIndex))
						{
							commFrameIndexList.Add(keyframe._frameIndex);
						}
					}
				}
			}

			//기존의 AnimCommonKeyframe에서 불필요한 것들을 먼저 없애고, 일단 Keyframe을 클리어한다.
			_subAnimCommonKeyframeList.RemoveAll(delegate (apAnimCommonKeyframe a)
			{
				//공통적으로 존재하지 않는 FrameIndex를 가진다면 삭제
				return !commFrameIndexList.Contains(a._frameIndex);
			});

			for (int i = 0; i < _subAnimCommonKeyframeList.Count; i++)
			{
				_subAnimCommonKeyframeList[i].Clear();
				_subAnimCommonKeyframeList[i].ReadyToAdd();
			}




			//1. Keyframe들의 공통 Index를 먼저 가져온다.
			for (int iKF = 0; iKF < totalKeyframes.Count; iKF++)
			{
				keyframe = totalKeyframes[iKF];

				apAnimCommonKeyframe commonKeyframe = GetCommonKeyframe(keyframe._frameIndex);

				if (commonKeyframe == null)
				{
					commonKeyframe = new apAnimCommonKeyframe(keyframe._frameIndex);
					commonKeyframe.ReadyToAdd();

					_subAnimCommonKeyframeList.Add(commonKeyframe);
				}

				//Common Keyframe에 추가한다.
				commonKeyframe.AddAnimKeyframe(keyframe, _subAnimKeyframeList.Contains(keyframe));
			}


			_subAnimCommonKeyframeList_Selected.Clear();

			//선택된 Common Keyframe만 처리한다.
			for (int i = 0; i < _subAnimCommonKeyframeList.Count; i++)
			{
				if(_subAnimCommonKeyframeList[i]._isSelected)
				{
					_subAnimCommonKeyframeList_Selected.Add(_subAnimCommonKeyframeList[i]);
				}
			}
			
		}

		public apAnimCommonKeyframe GetCommonKeyframe(int frameIndex)
		{
			return _subAnimCommonKeyframeList.Find(delegate (apAnimCommonKeyframe a)
			{
				return a._frameIndex == frameIndex;
			});
		}


		public void SetAnimCommonKeyframe(apAnimCommonKeyframe commonKeyframe, apGizmos.SELECT_TYPE selectType)
		{
			List<apAnimCommonKeyframe> commonKeyframes = new List<apAnimCommonKeyframe>();
			commonKeyframes.Add(commonKeyframe);
			SetAnimCommonKeyframes(commonKeyframes, selectType);
		}

		/// <summary>
		/// SetAnimKeyframe과 비슷하지만 CommonKeyframe을 선택하여 다중 선택을 한다.
		/// SelectionType에 따라서 다르게 처리를 한다.
		/// TimelineAutoSelect는 하지 않는다.
		/// </summary>
		public void SetAnimCommonKeyframes(List<apAnimCommonKeyframe> commonKeyframes, apGizmos.SELECT_TYPE selectType)
		{
			if(_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null)
			{
				_subAnimTimeline = null;
				_subAnimTimelineLayer = null;
				_subAnimKeyframe = null;

				_subAnimKeyframeList.Clear();


				_subAnimCommonKeyframeList.Clear();
				_subAnimCommonKeyframeList_Selected.Clear();

				AutoSelectAnimWorkKeyframe();//<Work+Mod 자동 연결

				SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결

				Editor.RefreshTimelineLayers(false);
				return;
			}

			if(selectType == apGizmos.SELECT_TYPE.New)
			{
				
				//New라면 다른 AnimKeyframe은 일단 취소해야하므로..
				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				//Refresh는 처리 후 일괄적으로 한다.

				//New에선
				//일단 모든 CommonKeyframe의 Selected를 false로 돌린다.
				for (int i = 0; i < _subAnimCommonKeyframeList.Count; i++)
				{
					_subAnimCommonKeyframeList[i]._isSelected = false;
				}
				_subAnimCommonKeyframeList_Selected.Clear();
			}

			

			apAnimCommonKeyframe commonKeyframe = null;
			for (int iCK = 0; iCK < commonKeyframes.Count; iCK++)
			{
				commonKeyframe = commonKeyframes[iCK];
				if (selectType == apGizmos.SELECT_TYPE.New ||
					selectType == apGizmos.SELECT_TYPE.Add)
				{

					commonKeyframe._isSelected = true;
					for (int iSubKey = 0; iSubKey < commonKeyframe._keyframes.Count; iSubKey++)
					{
						apAnimKeyframe keyframe = commonKeyframe._keyframes[iSubKey];
						//Add / New에서는 리스트에 더해주자
						if (!_subAnimKeyframeList.Contains(keyframe))
						{
							_subAnimKeyframeList.Add(keyframe);
						}
					}
				}
				else
				{
					//Subtract에서는 선택된 걸 제외한다.
					commonKeyframe._isSelected = false;

					for (int iSubKey = 0; iSubKey < commonKeyframe._keyframes.Count; iSubKey++)
					{
						apAnimKeyframe keyframe = commonKeyframe._keyframes[iSubKey];

						_subAnimKeyframeList.Remove(keyframe);
					}
				}
			}

			if (_subAnimKeyframeList.Count > 0)
			{
				if (!_subAnimKeyframeList.Contains(_subAnimKeyframe))
				{
					_subAnimKeyframe = _subAnimKeyframeList[0];
				}

				if(_subAnimKeyframeList.Count == 1)
				{
					_subAnimTimelineLayer = _subAnimKeyframe._parentTimelineLayer;
					_subAnimTimeline = _subAnimTimelineLayer._parentTimeline;
				}
				else
				{
					_subAnimTimelineLayer = null;
					//_subAnimTimeline//<<이건 건들지 않는다.
				}
			}
			else
			{
				_subAnimKeyframe = null;
			}

			//Common Keyframe을 갱신하자
			RefreshCommonAnimKeyframes();

			
			Editor.RefreshTimelineLayers(false);
			
			List<apAnimTimelineLayer> refreshLayer = new List<apAnimTimelineLayer>();
			for (int i = 0; i < _subAnimKeyframeList.Count; i++)
			{
				if (!refreshLayer.Contains(_subAnimKeyframeList[i]._parentTimelineLayer))
				{
					refreshLayer.Add(_subAnimKeyframeList[i]._parentTimelineLayer);
				}
			}
			for (int i = 0; i < refreshLayer.Count; i++)
			{
				refreshLayer[i].SortAndRefreshKeyframes();
			}

			AutoSelectAnimWorkKeyframe();//<Work+Mod 자동 연결
			SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결


			
		}
		//---------------------------------------------------------------


		private void SetAnimEditingToggle()
		{
			if (ExAnimEditingMode != EX_EDIT.None)
			{
				//>> Off
				//_isAnimEditing = false;
				_exAnimEditingMode = EX_EDIT.None;
				//_isAnimAutoKey = false;
				_isAnimLock = false;
			}
			else
			{
				if (IsAnimEditable)
				{
					//_isAnimEditing = true;//<<편집 시작!
					//_isAnimAutoKey = false;
					_exAnimEditingMode = EX_EDIT.ExOnly_Edit;//<<배타적 Mod 선택이 기본값이다.
					_isAnimLock = true;//기존의 False에서 True로 변경

					bool isVertexTarget = false;
					bool isControlParamTarget = false;
					bool isTransformTarget = false;
					bool isBoneTarget = false;

					//현재 객체가 현재 Timeline에 맞지 않다면 선택을 해제해야한다.
					if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
					{
						isControlParamTarget = true;
					}
					else if (_subAnimTimeline._linkedModifier != null)
					{
						if ((int)(_subAnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
						{
							isVertexTarget = true;
							isTransformTarget = true;
						}
						else if (_subAnimTimeline._linkedModifier.IsTarget_Bone)
						{
							isTransformTarget = true;
							isBoneTarget = true;
						}
						else
						{
							isTransformTarget = true;
						}
					}
					else
					{
						//?? 뭘 선택할까요.
						Debug.LogError("Anim Toggle Error : Animation Modifier 타입인데 Modifier가 연결 안됨");
					}

					if (!isVertexTarget)
					{
						_modRenderVertOfAnim = null;
						_modRenderVertListOfAnim.Clear();
					}
					if (!isControlParamTarget)
					{
						_subControlParamOnAnimClip = null;
					}
					if (!isTransformTarget)
					{
						_subMeshTransformOnAnimClip = null;
						_subMeshGroupTransformOnAnimClip = null;
					}
					if (!isBoneTarget)
					{
						_bone = null;
					}

					
				}
			}


			RefreshAnimEditing(true);

			Editor.RefreshControllerAndHierarchy();
		}

		public bool SetAnimExclusiveEditing_Tmp(EX_EDIT exEditing, bool isGizmoReset)
		{
			if(!IsAnimEditable && exEditing != EX_EDIT.None)
			{
				//편집중이 아니라면 None으로 강제한다.
				exEditing = EX_EDIT.None;
				return false;
			}


			if(_exAnimEditingMode == exEditing)
			{
				return true;
			}

			_exAnimEditingMode = exEditing;
			//if(_exAnimEditingMode == EX_EDIT.None)
			//{
			//	_isAnimLock = false;
			//}
			//else
			//{
			//	_isAnimLock = true;
			//}


			//Editing 상태에 따라 Refresh 코드가 다르다
			if (ExAnimEditingMode != EX_EDIT.None)
			{

				bool isModLock_ColorUpdate = Editor.GetModLockOption_ColorPreview(ExAnimEditingMode);
				bool isModLock_OtherMod = Editor.GetModLockOption_CalculateIfNotAddedOther(ExAnimEditingMode);


				//현재 선택한 타임라인에 따라서 Modifier를 On/Off할지 결정한다.
				bool isExclusiveActive = false;
				if (_subAnimTimeline != null)
				{
					if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						if (_subAnimTimeline._linkedModifier != null && _animClip._targetMeshGroup != null)
						{
							if (ExAnimEditingMode == EX_EDIT.ExOnly_Edit)
							{
								//현재의 AnimTimeline에 해당하는 ParamSet만 선택하자
								List<apModifierParamSetGroup> exParamSetGroups = new List<apModifierParamSetGroup>();
								List<apModifierParamSetGroup> linkParamSetGroups = _subAnimTimeline._linkedModifier._paramSetGroup_controller;
								for (int iP = 0; iP < linkParamSetGroups.Count; iP++)
								{
									apModifierParamSetGroup linkPSG = linkParamSetGroups[iP];
									if (linkPSG._keyAnimTimeline == _subAnimTimeline &&
										linkPSG._keyAnimClip == _animClip)
									{
										exParamSetGroups.Add(linkPSG);
									}
								}

								//Debug.Log("Set Anim Editing > Exclusive Enabled [" + _subAnimTimeline._linkedModifier.DisplayName + "]");

								_animClip._targetMeshGroup._modifierStack.SetExclusiveModifierInEditing_MultipleParamSetGroup(
																			_subAnimTimeline._linkedModifier, 
																			exParamSetGroups,
																			isModLock_ColorUpdate);
								isExclusiveActive = true;
							}
							else if (ExAnimEditingMode == EX_EDIT.General_Edit)
							{
								//추가 : General Edit 모드
								//선택한 것과 허용되는 Modifier는 모두 허용한다.
								_animClip._targetMeshGroup._modifierStack.SetExclusiveModifierInEditing_MultipleParamSetGroup_General(
																			_subAnimTimeline._linkedModifier, 
																			_animClip,
																			isModLock_ColorUpdate,
																			isModLock_OtherMod);
								isExclusiveActive = true;
							}
						}
					}
				}

				if (!isExclusiveActive)
				{
					//Modifier와 연동된게 아니라면
					if (_animClip._targetMeshGroup != null)
					{
						_animClip._targetMeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
						//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_animClip._targetMeshGroup);
						RefreshMeshGroupExEditingFlags(_animClip._targetMeshGroup, null, null, null, false);//<<추가
					}
				}
				else
				{
					RefreshMeshGroupExEditingFlags(_animClip._targetMeshGroup, _subAnimTimeline._linkedModifier, null, _animClip, false);//<<추가
				}
			}
			else
			{
				//모든 Modifier의 Exclusive 선택을 해제하고 모두 활성화한다.
				if (_animClip._targetMeshGroup != null)
				{
					_animClip._targetMeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_animClip._targetMeshGroup);
					RefreshMeshGroupExEditingFlags(_animClip._targetMeshGroup, null, null, null, false);//<<추가
				}
			}

			//if (ExAnimEditingMode != EX_EDIT.None)
			//{
			//	//>> Off
			//	//_isAnimEditing = false;
			//	_exAnimEditingMode = EX_EDIT.None;
			//	//_isAnimAutoKey = false;
			//	_isAnimLock = false;
			//}
			//else
			//{
			//	if (IsAnimEditable)
			//	{
			//		//_isAnimEditing = true;//<<편집 시작!
			//		//_isAnimAutoKey = false;
			//		_exAnimEditingMode = EX_EDIT.ExOnly_Edit;//<<배타적 Mod 선택이 기본값이다.
			//		_isAnimLock = true;//기존의 False에서 True로 변경

			//		//bool isVertexTarget = false;
			//		//bool isControlParamTarget = false;
			//		//bool isTransformTarget = false;
			//		//bool isBoneTarget = false;

			//		////현재 객체가 현재 Timeline에 맞지 않다면 선택을 해제해야한다.
			//		//if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
			//		//{
			//		//	isControlParamTarget = true;
			//		//}
			//		//else if (_subAnimTimeline._linkedModifier != null)
			//		//{
			//		//	if ((int)(_subAnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
			//		//	{
			//		//		isVertexTarget = true;
			//		//		isTransformTarget = true;
			//		//	}
			//		//	else if (_subAnimTimeline._linkedModifier.IsTarget_Bone)
			//		//	{
			//		//		isTransformTarget = true;
			//		//		isBoneTarget = true;
			//		//	}
			//		//	else
			//		//	{
			//		//		isTransformTarget = true;
			//		//	}
			//		//}
			//		//else
			//		//{
			//		//	//?? 뭘 선택할까요.
			//		//	Debug.LogError("Anim Toggle Error : Animation Modifier 타입인데 Modifier가 연결 안됨");
			//		//}

			//		//if (!isVertexTarget)
			//		//{
			//		//	_modRenderVertOfAnim = null;
			//		//	_modRenderVertListOfAnim.Clear();
			//		//}
			//		//if (!isControlParamTarget)
			//		//{
			//		//	_subControlParamOnAnimClip = null;
			//		//}
			//		//if (!isTransformTarget)
			//		//{
			//		//	_subMeshTransformOnAnimClip = null;
			//		//	_subMeshGroupTransformOnAnimClip = null;
			//		//}
			//		//if (!isBoneTarget)
			//		//{
			//		//	_bone = null;
			//		//}

					
			//	}
			//}

			//RefreshAnimEditing(isGizmoReset);
			return true;
		}


		/// <summary>
		/// Mod Lock을 갱신한다.
		/// Animation Clip 선택시 이걸 호출한다.
		/// SetAnimEditingLayerLockToggle() 함수를 다시 호출한 것과 같다.
		/// </summary>
		public void RefreshAnimEditingLayerLock()
		{
			if (_animClip == null ||
				SelectionType != SELECTION_TYPE.Animation)
			{
				return;
			}

			if(ExAnimEditingMode == EX_EDIT.None)
			{
				_exAnimEditingMode = EX_EDIT.None;
			}

			RefreshAnimEditing(true);
		}

		private void SetAnimEditingLayerLockToggle()
		{
			if (ExAnimEditingMode == EX_EDIT.None)
			{
				return;
			}

			if (ExAnimEditingMode == EX_EDIT.ExOnly_Edit)
			{
				_exAnimEditingMode = EX_EDIT.General_Edit;
			}
			else
			{
				_exAnimEditingMode = EX_EDIT.ExOnly_Edit;
			}

			RefreshAnimEditing(true);
		}

		/// <summary>
		/// 애니메이션 작업 도중 타임라인 추가/삭제, 키프레임 추가/삭제/이동과 같은 변동사항이 있을때 호출되어야 하는 함수
		/// </summary>
		public void RefreshAnimEditing(bool isGizmoEventReset)
		{
			if (_animClip == null)
			{
				return;
			}

			//Editing 상태에 따라 Refresh 코드가 다르다
			if (ExAnimEditingMode != EX_EDIT.None)
			{

				bool isModLock_ColorUpdate = Editor.GetModLockOption_ColorPreview(ExAnimEditingMode);
				bool isModLock_OtherMod = Editor.GetModLockOption_CalculateIfNotAddedOther(ExAnimEditingMode);


				//현재 선택한 타임라인에 따라서 Modifier를 On/Off할지 결정한다.
				bool isExclusiveActive = false;
				if (_subAnimTimeline != null)
				{
					if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						if (_subAnimTimeline._linkedModifier != null && _animClip._targetMeshGroup != null)
						{
							if (ExAnimEditingMode == EX_EDIT.ExOnly_Edit)
							{
								//현재의 AnimTimeline에 해당하는 ParamSet만 선택하자
								List<apModifierParamSetGroup> exParamSetGroups = new List<apModifierParamSetGroup>();
								List<apModifierParamSetGroup> linkParamSetGroups = _subAnimTimeline._linkedModifier._paramSetGroup_controller;
								for (int iP = 0; iP < linkParamSetGroups.Count; iP++)
								{
									apModifierParamSetGroup linkPSG = linkParamSetGroups[iP];
									if (linkPSG._keyAnimTimeline == _subAnimTimeline &&
										linkPSG._keyAnimClip == _animClip)
									{
										exParamSetGroups.Add(linkPSG);
									}
								}

								//Debug.Log("Set Anim Editing > Exclusive Enabled [" + _subAnimTimeline._linkedModifier.DisplayName + "]");

								_animClip._targetMeshGroup._modifierStack.SetExclusiveModifierInEditing_MultipleParamSetGroup(
																			_subAnimTimeline._linkedModifier, 
																			exParamSetGroups,
																			isModLock_ColorUpdate);
								isExclusiveActive = true;
							}
							else if (ExAnimEditingMode == EX_EDIT.General_Edit)
							{
								//추가 : General Edit 모드
								//선택한 것과 허용되는 Modifier는 모두 허용한다.
								_animClip._targetMeshGroup._modifierStack.SetExclusiveModifierInEditing_MultipleParamSetGroup_General(
																			_subAnimTimeline._linkedModifier, 
																			_animClip,
																			isModLock_ColorUpdate,
																			isModLock_OtherMod);
								isExclusiveActive = true;
							}
						}
					}
				}

				if (!isExclusiveActive)
				{
					//Modifier와 연동된게 아니라면
					if (_animClip._targetMeshGroup != null)
					{
						_animClip._targetMeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
						Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_animClip._targetMeshGroup);
						RefreshMeshGroupExEditingFlags(_animClip._targetMeshGroup, null, null, null, false);//<<추가
					}
				}
				else
				{
					RefreshMeshGroupExEditingFlags(_animClip._targetMeshGroup, _subAnimTimeline._linkedModifier, null, _animClip, false);//<<추가
				}
			}
			else
			{
				//모든 Modifier의 Exclusive 선택을 해제하고 모두 활성화한다.
				if (_animClip._targetMeshGroup != null)
				{
					_animClip._targetMeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_animClip._targetMeshGroup);
					RefreshMeshGroupExEditingFlags(_animClip._targetMeshGroup, null, null, null, false);//<<추가
				}
			}

			AutoSelectAnimTimelineLayer();
			Editor.RefreshTimelineLayers(false);
			SetAnimClipGizmoEvent(isGizmoEventReset);
		}




		private int _timlineGUIWidth = -1;
		/// <summary>
		/// Is Auto Scroll 옵션이 켜져있으면 스크롤을 자동으로 선택한다.
		/// 재생중에도 스크롤을 움직인다.
		/// </summary>
		public void SetAutoAnimScroll()
		{
			int curFrame = 0;
			int startFrame = 0;
			int endFrame = 0;
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null || _timlineGUIWidth <= 0)
			{
				return;
			}
			if(!_animClip.IsPlaying_Editor)
			{
				return;
			}

			curFrame = _animClip.CurFrame;
			startFrame = _animClip.StartFrame;
			endFrame = _animClip.EndFrame;

			int widthPerFrame = Editor.WidthPerFrameInTimeline;
			int nFrames = Mathf.Max((endFrame - startFrame) + 1, 1);
			int widthForTotalFrame = nFrames * widthPerFrame;
			int widthForScrollFrame = widthForTotalFrame;

			//화면에 보여지는 프레임 범위는?
			int startFrame_Visible = (int)((float)(_scroll_Timeline.x / (float)widthPerFrame) + startFrame);
			int endFrame_Visible = (int)(((float)_timlineGUIWidth / (float)widthPerFrame) + startFrame_Visible);

			int marginFrame = 10;
			int targetFrame = -1;


			startFrame_Visible += marginFrame;
			endFrame_Visible -= marginFrame;

			//"이동해야할 범위와 실제로 이동되는 범위는 다르다"
			if (curFrame < startFrame_Visible)
			{
				//커서가 화면 왼쪽에 붙도록 하자
				targetFrame = curFrame - marginFrame;
			}
			else if (curFrame > endFrame_Visible)
			{
				//커서가 화면 오른쪽에 붙도록 하자
				targetFrame = (curFrame + marginFrame) - (int)((float)_timlineGUIWidth / (float)widthPerFrame);
			}
			else
			{
				return;
			}

			targetFrame -= startFrame;
			float nextScroll = Mathf.Clamp((targetFrame * widthPerFrame), 0, widthForScrollFrame);
			//Debug.Log("Auto Scroll [Curframe : " + curFrame + " [, Scroll Frame : " + targetFrame + "[" + startFrame_Visible + " ~ " + endFrame_Visible + "], " +
			//	"Scroll : " + _scroll_Timeline.x + " >> " + nextScroll);

			_scroll_Timeline.x = nextScroll;
		}

		/// <summary>
		/// 마우스 편집 중에 스크롤을 자동으로 해야하는 경우
		/// AnimClip의 프레임은 수정하지 않는다. (마우스 위치에 따른 TargetFrame을 넣어주자)
		/// </summary>
		public void AutoAnimScrollWithoutFrameMoving(int requestFrame, int marginFrame)
		{
			
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null || _timlineGUIWidth <= 0)
			{
				return;
			}
			
			int startFrame = _animClip.StartFrame;
			int endFrame = _animClip.EndFrame;

			if (requestFrame < startFrame)
			{
				requestFrame = startFrame;
			}
			else if (requestFrame > endFrame)
			{
				requestFrame = endFrame;
			}

			int widthPerFrame = Editor.WidthPerFrameInTimeline;
			int nFrames = Mathf.Max((endFrame - startFrame) + 1, 1);
			int widthForTotalFrame = nFrames * widthPerFrame;
			int widthForScrollFrame = widthForTotalFrame;

			//화면에 보여지는 프레임 범위는?
			int startFrame_Visible = (int)((float)(_scroll_Timeline.x / (float)widthPerFrame) + startFrame);
			int endFrame_Visible = (int)(((float)_timlineGUIWidth / (float)widthPerFrame) + startFrame_Visible);

			//int marginFrame = 10;


			startFrame_Visible += marginFrame;
			endFrame_Visible -= marginFrame;

			int targetFrame = 0;

			//"이동해야할 범위와 실제로 이동되는 범위는 다르다"
			if (requestFrame < startFrame_Visible)
			{
				//커서가 화면 왼쪽에 붙도록 하자
				targetFrame = requestFrame - marginFrame;
			}
			else if (requestFrame > endFrame_Visible)
			{
				//커서가 화면 오른쪽에 붙도록 하자
				targetFrame = (requestFrame + marginFrame) - (int)((float)_timlineGUIWidth / (float)widthPerFrame);
			}
			else
			{
				return;
			}

			targetFrame -= startFrame;
			float nextScroll = Mathf.Clamp((targetFrame * widthPerFrame), 0, widthForScrollFrame);
			//Debug.Log("Auto Scroll [Curframe : " + curFrame + " [, Scroll Frame : " + targetFrame + "[" + startFrame_Visible + " ~ " + endFrame_Visible + "], " +
			//	"Scroll : " + _scroll_Timeline.x + " >> " + nextScroll);

			_scroll_Timeline.x = nextScroll;
		}

		/// <summary>
		/// AnimClip 작업을 위해 MeshTransform을 선택한다.
		/// 해당 데이터가 Timeline에 없어도 선택 가능하다.
		/// </summary>
		/// <param name="meshTransform"></param>
		public void SetSubMeshTransformForAnimClipEdit(apTransform_Mesh meshTransform)
		{
			if (meshTransform != null)
			{
				_bone = null;
			}
			_subMeshGroupTransformOnAnimClip = null;
			_subControlParamOnAnimClip = null;

			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				_subMeshTransformOnAnimClip = null;
				return;
			}
			_subMeshTransformOnAnimClip = meshTransform;

			AutoSelectAnimTimelineLayer();
		}

		/// <summary>
		/// AnimClip 작업을 위해 MeshGroupTransform을 선택한다.
		/// 해당 데이터가 Timeline에 없어도 선택 가능하다.
		/// </summary>
		/// <param name="meshGroupTransform"></param>
		public void SetSubMeshGroupTransformForAnimClipEdit(apTransform_MeshGroup meshGroupTransform)
		{
			if (meshGroupTransform != null)
			{
				_bone = null;
			}
			_subMeshTransformOnAnimClip = null;
			_subControlParamOnAnimClip = null;

			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				_subMeshGroupTransformOnAnimClip = null;
				return;
			}

			_subMeshGroupTransformOnAnimClip = meshGroupTransform;

			AutoSelectAnimTimelineLayer();
		}

		/// <summary>
		/// AnimClip 작업을 위해 Control Param을 선택한다.
		/// 해당 데이터가 Timeline에 없어도 선택 가능하다
		/// </summary>
		/// <param name="controlParam"></param>
		public void SetSubControlParamForAnimClipEdit(apControlParam controlParam)
		{
			_bone = null;
			_subMeshTransformOnAnimClip = null;
			_subMeshGroupTransformOnAnimClip = null;

			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				_subControlParamOnAnimClip = null;
				return;
			}

			_subControlParamOnAnimClip = controlParam;

			AutoSelectAnimTimelineLayer();
		}

		/// <summary>
		/// 선택된 객체(Transform/Bone/ControlParam) 중에서 "현재 타임라인"이 선택할 수 있는 객체를 리턴한다.
		/// </summary>
		/// <returns></returns>
		public object GetSelectedAnimTimelineObject()
		{
			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null ||
				_subAnimTimeline == null)
			{
				return null;
			}

			switch (_subAnimTimeline._linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					if (SubMeshTransformOnAnimClip != null)
					{
						return SubMeshTransformOnAnimClip;
					}
					if (SubMeshGroupTransformOnAnimClip != null)
					{
						return SubMeshGroupTransformOnAnimClip;
					}
					if (Bone != null)
					{
						return Bone;
					}
					break;

				case apAnimClip.LINK_TYPE.ControlParam:
					if (SubControlParamOnAnimClip != null)
					{
						return SubControlParamOnAnimClip;
					}
					break;

			}
			return null;
		}


		/// <summary>
		/// 현재 선택한 Sub 객체 (Transform, Bone, ControlParam)에 따라서
		/// 자동으로 Timeline의 Layer를 선택해준다.
		/// </summary>
		/// <param name="isAutoChangeLeftTab">이 값이 True이면 Timeline이 ControlParam 타입일때 자동으로 왼쪽 탭이 Controller로 바뀐다.</param>
		public void AutoSelectAnimTimelineLayer(bool isAutoChangeLeftTab = true)
		{
			//수정 :
			//Timeline을 선택하지 않았다 하더라도 자동으로 선택을 할 수 있다.
			//수정작업중이 아니며 + 해당 오브젝트를 포함하는 Layer를 가진 Timeline의 개수가 1개일 땐 그것을 선택한다.
			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null)
			{
				// 아예 작업 불가
				_subAnimTimeline = null;
				_subAnimTimelineLayer = null;
				_subAnimKeyframe = null;
				_subAnimWorkKeyframe = null;

				_subAnimKeyframeList.Clear();

				AutoSelectAnimWorkKeyframe();

				//우측 Hierarchy GUI 변동이 있을 수 있으니 리셋
				Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Meshes", false);
				Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Bone", false);
				Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - ControlParam", false);
				//Editor.SetGUIVisible("AnimationRight2GUI_Timeline_Layers", false);
				return;
			}


			if(_subAnimTimeline == null)
			{
				
				//1. 선택한 Timeline이 없네용
				//아예 새로 찾아야 한다.
				_subAnimWorkKeyframe = null;

				bool isFindTimeline = false;
				object selectedObject = null;


				if (SubMeshTransformOnAnimClip != null)
				{
					selectedObject = SubMeshTransformOnAnimClip;
				}
				if (SubMeshGroupTransformOnAnimClip != null)
				{
					selectedObject = SubMeshGroupTransformOnAnimClip;
				}
				if (Bone != null)
				{
					selectedObject = Bone;
				}

				if(selectedObject != null && ExAnimEditingMode == EX_EDIT.None)
				{
					//선택 대상이 될법한 Timeline들을 찾자
					List<apAnimTimelineLayer> resultTimelineLayers = new List<apAnimTimelineLayer>();

					//Control Param을 제외하고 Timeline을 찾자
					for (int i = 0; i < _animClip._timelines.Count; i++)
					{
						apAnimTimeline curTimeline = _animClip._timelines[i];
						if (curTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam
							|| curTimeline._linkedModifier == null)
						{
							continue;
						}

						apAnimTimelineLayer nextLayer = curTimeline.GetTimelineLayer(selectedObject);
						if(nextLayer != null)
						{
							resultTimelineLayers.Add(nextLayer);
						}
					}

					if (resultTimelineLayers.Count == 1)
					{
						//한개인 경우에만 선택이 가능하다
						isFindTimeline = true;

						apAnimTimelineLayer nextLayer = resultTimelineLayers[0];
						_subAnimTimeline = nextLayer._parentTimeline;
						SetAnimTimelineLayer(nextLayer, false);

						AutoSelectAnimWorkKeyframe();

						//여기서는 아예 Work Keyframe 뿐만아니라 Keyframe으로도 선택을 한다.
						SetAnimKeyframe(AnimWorkKeyframe, false, apGizmos.SELECT_TYPE.New);

						_modRegistableBones.Clear();//<<이것도 갱신해주자 [타입라인에 등록된 Bone]
						if (_subAnimTimeline != null)
						{
							for (int i = 0; i < _subAnimTimeline._layers.Count; i++)
							{
								apAnimTimelineLayer timelineLayer = _subAnimTimeline._layers[i];
								if (timelineLayer._linkedBone != null)
								{
									_modRegistableBones.Add(timelineLayer._linkedBone);
								}
							}

						}


						//우측 Hierarchy GUI 변동이 있을 수 있으니 리셋
						Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Meshes", false);
						Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Bone", false);
						Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - ControlParam", false);
						//Editor.SetGUIVisible("AnimationRight2GUI_Timeline_Layers", false);
						return;//<끝!
					}
					
				}


				if (!isFindTimeline)
				{
					//결국 Timeline을 찾지 못했다.
					_subAnimTimeline = null;
					_subAnimTimelineLayer = null;
					_subAnimKeyframe = null;
					_subAnimWorkKeyframe = null;

					_subAnimKeyframeList.Clear();

					AutoSelectAnimWorkKeyframe();

					//우측 Hierarchy GUI 변동이 있을 수 있으니 리셋
					Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Meshes", false);
					Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Bone", false);
					Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - ControlParam", false);
					//Editor.SetGUIVisible("AnimationRight2GUI_Timeline_Layers", false);
				
					return;
				}
			}
			else
			{
				//2. 선택한 Timeline이 있으면 거기서 찾자
				_subAnimWorkKeyframe = null;

				//timeline이 ControlParam계열이라면 에디터의 탭을 변경
				if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam
					&& isAutoChangeLeftTab)
				{
					Editor.SetLeftTab(apEditor.TAB_LEFT.Controller);
				}

				//자동으로 스크롤을 해주자
				_isAnimTimelineLayerGUIScrollRequest = true;

				//다중 키프레임 작업중에는 단일 선택 불가
				if (_subAnimKeyframeList.Count > 1)
				{

					AutoSelectAnimWorkKeyframe();

					//우측 Hierarchy GUI 변동이 있을 수 있으니 리셋
					Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Meshes", false);
					Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Bone", false);
					Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - ControlParam", false);
					//Editor.SetGUIVisible("AnimationRight2GUI_Timeline_Layers", false);

					return;
				}




				object selectedObject = GetSelectedAnimTimelineObject();
				if (selectedObject == null)
				{
					AutoSelectAnimWorkKeyframe();
					if (AnimWorkKeyframe == null)
					{
						SetAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
					}

					//Debug.LogError("Object Select -> 선택된 객체가 없다.");

					//우측 Hierarchy GUI 변동이 있을 수 있으니 리셋
					Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Meshes", false);
					Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Bone", false);
					Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - ControlParam", false);
					//Editor.SetGUIVisible("AnimationRight2GUI_Timeline_Layers", false);

					return;//선택된게 없다면 일단 패스
				}


				apAnimTimelineLayer nextLayer = null;
				nextLayer = _subAnimTimeline.GetTimelineLayer(selectedObject);
				if (nextLayer != null)
				{
					SetAnimTimelineLayer(nextLayer, false);
				}

				#region [미사용 코드]
				////만약 이미 선택된 레이어가 있다면 "유효"한지 테스트한다
				//bool isLayerIsAlreadySelected = false;
				//if (_subAnimTimelineLayer != null)
				//{
				//	if (_subAnimTimelineLayer.IsContainTargetObject(selectedObject))
				//	{
				//		//현재 레이어에 포함되어 있다면 패스
				//		//AutoSelectAnimWorkKeyframe();
				//		//return;
				//		isLayerIsAlreadySelected = true;
				//	}
				//}
				//apAnimTimelineLayer nextLayer = null;
				//if (!isLayerIsAlreadySelected)
				//{
				//	nextLayer = _subAnimTimeline.GetTimelineLayer(selectedObject);

				//	if (nextLayer != null)
				//	{
				//		SetAnimTimelineLayer(nextLayer, false);
				//	}
				//}
				//else
				//{
				//	nextLayer = _subAnimTimelineLayer;
				//} 
				#endregion


				AutoSelectAnimWorkKeyframe();

				//여기서는 아예 Work Keyframe 뿐만아니라 Keyframe으로도 선택을 한다.
				SetAnimKeyframe(AnimWorkKeyframe, false, apGizmos.SELECT_TYPE.New);

				_modRegistableBones.Clear();//<<이것도 갱신해주자 [타입라인에 등록된 Bone]
				if (_subAnimTimeline != null)
				{
					for (int i = 0; i < _subAnimTimeline._layers.Count; i++)
					{
						apAnimTimelineLayer timelineLayer = _subAnimTimeline._layers[i];
						if (timelineLayer._linkedBone != null)
						{
							_modRegistableBones.Add(timelineLayer._linkedBone);
						}
					}

				}

				//우측 Hierarchy GUI 변동이 있을 수 있으니 리셋
				Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Meshes", false);
				Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - Bone", false);
				Editor.SetGUIVisible("GUI Anim Hierarchy Delayed - ControlParam", false);
				//Editor.SetGUIVisible("AnimationRight2GUI_Timeline_Layers", false);
			}
		}

		/// <summary>
		/// 현재 재생중인 프레임에 맞게 WorkKeyframe을 자동으로 선택한다.
		/// 키프레임을 바꾸거나 레이어를 바꿀때 자동으로 호출한다.
		/// 수동으로 선택하는 키프레임과 다르다.
		/// </summary>
		public void AutoSelectAnimWorkKeyframe()
		{
			Editor.Gizmos.SetUpdate();

			

			//apAnimKeyframe prevWorkKeyframe = _subAnimWorkKeyframe;
			if (_subAnimTimelineLayer == null || IsAnimPlaying)//<<플레이 중에는 모든 선택이 초기화된다.
			{
				
				if (_subAnimWorkKeyframe != null)
				{
					_subAnimWorkKeyframe = null;
					_modMeshOfAnim = null;
					_modBoneOfAnim = null;
					_renderUnitOfAnim = null;
					_modRenderVertOfAnim = null;
					_modRenderVertListOfAnim.Clear();
					_modRenderVertListOfAnim_Weighted.Clear();

					//추가 : 기즈모 갱신이 필요한 경우 (주로 FFD)
					Editor.Gizmos.RefreshFFDTransformForce();
				}

				Editor.Hierarchy_AnimClip.RefreshUnits();
				return;
			}
			int curFrame = _animClip.CurFrame;
			_subAnimWorkKeyframe = _subAnimTimelineLayer.GetKeyframeByFrameIndex(curFrame);

			if (_subAnimWorkKeyframe == null)
			{
				
				_modMeshOfAnim = null;
				_modBoneOfAnim = null;
				_renderUnitOfAnim = null;
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();

				Editor.Gizmos.RefreshFFDTransformForce();//<기즈모 갱신

				Editor.Hierarchy_AnimClip.RefreshUnits();
				return;
			}

			bool isResetMod = true;
			//if (_subAnimWorkKeyframe != prevWorkKeyframe)//강제
			{
				if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					
					if (_subAnimTimeline._linkedModifier != null)
					{
						
						apModifierParamSet targetParamSet = _subAnimTimeline.GetModifierParamSet(_subAnimTimelineLayer, _subAnimWorkKeyframe);
						if (targetParamSet != null)
						{
							if (targetParamSet._meshData.Count > 0)
							{
								
								isResetMod = false;
								//중요!
								//>>여기서 Anim용 ModMesh를 선택한다.<<
								_modMeshOfAnim = targetParamSet._meshData[0];
								if (_modMeshOfAnim._transform_Mesh != null)
								{
									_renderUnitOfAnim = _animClip._targetMeshGroup.GetRenderUnit(_modMeshOfAnim._transform_Mesh);
								}
								else if (_modMeshOfAnim._transform_MeshGroup != null)
								{
									_renderUnitOfAnim = _animClip._targetMeshGroup.GetRenderUnit(_modMeshOfAnim._transform_MeshGroup);
								}
								else
								{
									_renderUnitOfAnim = null;
								}

								_modRenderVertOfAnim = null;
								_modRenderVertListOfAnim.Clear();
								_modRenderVertListOfAnim_Weighted.Clear();

								_modBoneOfAnim = null;//<<Mod Bone은 선택 해제
							}
							else if (targetParamSet._boneData.Count > 0)
							{
								
								isResetMod = false;

								//ModBone이 있다면 그걸 선택하자
								_modBoneOfAnim = targetParamSet._boneData[0];
								_renderUnitOfAnim = _modBoneOfAnim._renderUnit;
								if (_modBoneOfAnim != null)
								{
									_bone = _modBoneOfAnim._bone;
								}


								//Mod Mesh 변수는 초기화
								_modMeshOfAnim = null;
								_modRenderVertOfAnim = null;
								_modRenderVertListOfAnim.Clear();
								_modRenderVertListOfAnim_Weighted.Clear();
							}
						}
						

					}
				}
			}
			//else
			//{
			//	//변동된 것이 없다.
			//	isResetMod = false;
			//}

			if (isResetMod)
			{
				_modMeshOfAnim = null;
				_modBoneOfAnim = null;
				_renderUnitOfAnim = null;
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();
			}

			Editor.Gizmos.RefreshFFDTransformForce();//<기즈모 갱신

			Editor.Hierarchy_AnimClip.RefreshUnits();
		}


		/// <summary>
		/// Anim 편집시 모든 선택된 오브젝트를 해제한다.
		/// </summary>
		public void UnselectAllObjectsOfAnim()
		{
			_modRenderVertOfAnim = null;
			_modRenderVertListOfAnim.Clear();
			_modRenderVertListOfAnim_Weighted.Clear();

			_bone = null;
			_modBoneOfAnim = null;
			_modMeshOfAnim = null;

			_subControlParamOnAnimClip = null;
			_subMeshTransformOnAnimClip = null;
			_subMeshGroupTransformOnAnimClip = null;

			
			
			SetAnimTimelineLayer(null, true, false, true);//TImelineLayer의 선택을 취소해야 AutoSelect가 정상작동한다.
			AutoSelectAnimTimelineLayer();
		}

		/// <summary>
		/// Mod-Render Vertex를 선택한다. [Animation 수정작업시]
		/// </summary>
		/// <param name="modVertOfAnim">Modified Vertex of Anim Keyframe</param>
		/// <param name="renderVertOfAnim">Render Vertex of Anim Keyframe</param>
		public void SetModVertexOfAnim(apModifiedVertex modVertOfAnim, apRenderVertex renderVertOfAnim)
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				|| AnimWorkKeyframe == null
				|| ModMeshOfAnim == null)
			{
				return;
			}

			if (modVertOfAnim == null || renderVertOfAnim == null)
			{
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();
				return;
			}

			if (ModMeshOfAnim != modVertOfAnim._modifiedMesh)
			{
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();
				return;
			}
			bool isChangeModVert = false;
			if (_modRenderVertOfAnim != null)
			{
				if (_modRenderVertOfAnim._modVert != modVertOfAnim || _modRenderVertOfAnim._renderVert != renderVertOfAnim)
				{
					isChangeModVert = true;
				}
			}
			else
			{
				isChangeModVert = true;
			}

			if (isChangeModVert)
			{
				_modRenderVertOfAnim = new ModRenderVert(modVertOfAnim, renderVertOfAnim);
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim.Add(_modRenderVertOfAnim);

				_modRenderVertListOfAnim_Weighted.Clear();

			}
		}



		/// <summary>
		/// Mod-Render Vertex를 추가한다. [Animation 수정작업시]
		/// </summary>
		public void AddModVertexOfAnim(apModifiedVertex modVertOfAnim, apRenderVertex renderVertOfAnim)
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				|| AnimWorkKeyframe == null
				|| ModMeshOfAnim == null)
			{
				return;
			}

			if (modVertOfAnim == null || renderVertOfAnim == null)
			{
				//추가/제거없이 생략
				return;
			}

			bool isExistSame = _modRenderVertListOfAnim.Exists(delegate (ModRenderVert a)
			{
				return a._modVert == modVertOfAnim || a._renderVert == renderVertOfAnim;
			});

			if (!isExistSame)
			{
				//새로 생성+추가해야할 필요가 있다.
				ModRenderVert newModRenderVert = new ModRenderVert(modVertOfAnim, renderVertOfAnim);
				_modRenderVertListOfAnim.Add(newModRenderVert);

				if (_modRenderVertListOfAnim.Count == 1)
				{
					_modRenderVertOfAnim = newModRenderVert;
				}
			}
		}

		/// <summary>
		/// Mod-Render Vertex를 삭제한다. [Animation 수정작업시]
		/// </summary>
		public void RemoveModVertexOfAnim(apModifiedVertex modVertOfAnim, apRenderVertex renderVertOfAnim)
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				|| AnimWorkKeyframe == null
				|| ModMeshOfAnim == null)
			{
				return;
			}

			if (modVertOfAnim == null || renderVertOfAnim == null)
			{
				//추가/제거없이 생략
				return;
			}

			_modRenderVertListOfAnim.RemoveAll(delegate (ModRenderVert a)
			{
				return a._modVert == modVertOfAnim || a._renderVert == renderVertOfAnim;
			});

			if (_modRenderVertListOfAnim.Count == 1)
			{
				_modRenderVertOfAnim = _modRenderVertListOfAnim[0];
			}
			else if (_modRenderVertListOfAnim.Count == 0)
			{
				_modRenderVertOfAnim = null;
			}
			else if (!_modRenderVertListOfAnim.Contains(_modRenderVertOfAnim))
			{
				_modRenderVertOfAnim = null;
				_modRenderVertOfAnim = _modRenderVertListOfAnim[0];
			}

		}





		public void SetBone(apBone bone)
		{
			_bone = bone;
			if (SelectionType == SELECTION_TYPE.MeshGroup &&
				Modifier != null)
			{
				AutoSelectModMeshOrModBone();
			}
			if (SelectionType == SELECTION_TYPE.Animation && AnimClip != null)
			{
				AutoSelectAnimTimelineLayer();
			}
		}

		/// <summary>
		/// AnimClip 작업시 Bone을 선택하면 SetBone대신 이 함수를 호출한다.
		/// </summary>
		/// <param name="bone"></param>
		public void SetBoneForAnimClip(apBone bone)
		{
			_bone = bone;

			if (bone != null)
			{
				_subControlParamOnAnimClip = null;
				_subMeshTransformOnAnimClip = null;
				_subMeshGroupTransformOnAnimClip = null;
			}

			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				_bone = null;
				return;
			}

			SetAnimTimelineLayer(null, true);//TImelineLayer의 선택을 취소해야 AutoSelect가 정상작동한다.
			AutoSelectAnimTimelineLayer();
			if(_bone != bone && bone != null)
			{
				//bone은 유지하자
				_bone = bone;
				_modBoneOfAnim = null;
			}
		}

		/// <summary>
		/// isEditing : Default Matrix를 수정하는가
		/// isBoneMenu : 현재 Bone Menu인가
		/// </summary>
		/// <param name="isEditing"></param>
		/// <param name="isBoneMenu"></param>
		public void SetBoneEditing(bool isEditing, bool isBoneMenu)
		{
			//bool isChanged = _isBoneDefaultEditing != isEditing;

			_isBoneDefaultEditing = isEditing;

			//if (isChanged)
			{
				if (_isBoneDefaultEditing)
				{
					SetBoneEditMode(BONE_EDIT_MODE.SelectAndTRS, isBoneMenu);
					//Debug.LogError("TODO : Default Bone Tranform을 활성화할 때에는 다른 Rig Modifier를 꺼야한다.");

					//Editor.Gizmos.LinkObject()
				}
				else
				{
					if (isBoneMenu)
					{
						SetBoneEditMode(BONE_EDIT_MODE.SelectOnly, isBoneMenu);
					}
					else
					{
						SetBoneEditMode(BONE_EDIT_MODE.None, isBoneMenu);
					}
					//Debug.LogError("TODO : Default Bone Tranform을 종료할 때에는 다른 Rig Modifier를 켜야한다.");
				}
			}
		}

		public void SetBoneEditMode(BONE_EDIT_MODE boneEditMode, bool isBoneMenu)
		{
			_boneEditMode = boneEditMode;

			if (!_isBoneDefaultEditing)
			{
				if (isBoneMenu)
				{
					_boneEditMode = BONE_EDIT_MODE.SelectOnly;
				}
				else
				{
					_boneEditMode = BONE_EDIT_MODE.None;
				}
			}

			Editor.Controller.SetBoneEditInit();
			//Gizmo 이벤트를 설정하자
			switch (_boneEditMode)
			{
				case BONE_EDIT_MODE.None:
					Editor.Gizmos.Unlink();
					break;

				case BONE_EDIT_MODE.SelectOnly:
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Bone_SelectOnly());
					break;

				case BONE_EDIT_MODE.SelectAndTRS:
					//Select에서는 Gizmo 이벤트를 받는다.
					//Transform 제어를 해야하기 때문
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Bone_Default());
					break;

				case BONE_EDIT_MODE.Add:
					Editor.Gizmos.Unlink();
					break;

				case BONE_EDIT_MODE.Link:
					Editor.Gizmos.Unlink();
					break;
			}
		}

		/// <summary>
		/// Rigging시 Pose Test를 하는지 여부를 설정한다.
		/// 모든 MeshGroup에 대해서 설정한다.
		/// _rigEdit_isTestPosing값을 먼저 설정한다.
		/// </summary>
		public void SetBoneRiggingTest()
		{
			if (Editor._portrait == null)
			{
				return;
			}
			for (int i = 0; i < Editor._portrait._meshGroups.Count; i++)
			{
				apMeshGroup meshGroup = Editor._portrait._meshGroups[i];
				meshGroup.SetBoneRiggingTest(_rigEdit_isTestPosing);
			}
		}

		/// <summary>
		/// Rigging시, Test중인 Pose를 리셋한다.
		/// </summary>
		public void ResetRiggingTestPose()
		{
			if (Editor._portrait == null)
			{
				return;
			}
			for (int i = 0; i < Editor._portrait._meshGroups.Count; i++)
			{
				apMeshGroup meshGroup = Editor._portrait._meshGroups[i];
				meshGroup.ResetRiggingTestPose();
			}
			Editor.RefreshControllerAndHierarchy();
			Editor.SetRepaint();
		}

		// Editor View
		//-------------------------------------
		public bool DrawEditor(int width, int height)
		{
			if (_portrait == null)
			{
				//Debug.LogError("Selection Portrait is Null");
				return false;
			}
			//EditorGUILayout.LabelField("Properties");

			//EditorGUILayout.Space();
			EditorGUILayout.Space();
			switch (_selectionType)
			{
				case SELECTION_TYPE.None:
					Draw_None(width, height);
					break;

				case SELECTION_TYPE.ImageRes:
					Draw_ImageRes(width, height);
					break;
				case SELECTION_TYPE.Mesh:
					Draw_Mesh(width, height);
					break;
				case SELECTION_TYPE.Face:
					Draw_Face(width, height);
					break;
				case SELECTION_TYPE.MeshGroup:
					Draw_MeshGroup(width, height);
					break;
				case SELECTION_TYPE.Animation:
					Draw_Animation(width, height);
					break;
				case SELECTION_TYPE.Overall:
					Draw_Overall(width, height);
					break;
				case SELECTION_TYPE.Param:
					Draw_Param(width, height);
					break;
			}

			EditorGUILayout.Space();

			return true;
		}

		//private apPortrait _portrait = null;
		//private apTextureData _image = null;
		//private apMesh _mesh = null;
		public void DrawEditor_Header(int width, int height)
		{
			switch (_selectionType)
			{
				case SELECTION_TYPE.None:
					DrawTitle(Editor.GetUIWord(UIWORD.NotSelected), width);//"Not Selected"
					break;

				case SELECTION_TYPE.ImageRes:
					DrawTitle(Editor.GetUIWord(UIWORD.Image), width);//"Image"
					break;
				case SELECTION_TYPE.Mesh:
					DrawTitle(Editor.GetUIWord(UIWORD.Mesh), width);//"Mesh"
					break;
				case SELECTION_TYPE.Face:
					DrawTitle("Face", width);
					break;
				case SELECTION_TYPE.MeshGroup:
					DrawTitle(Editor.GetUIWord(UIWORD.MeshGroup), width);//"Mesh Group"
					break;
				case SELECTION_TYPE.Animation:
					DrawTitle(Editor.GetUIWord(UIWORD.AnimationClip), width);//"Animation Clip"
					break;
				case SELECTION_TYPE.Overall:
					DrawTitle(Editor.GetUIWord(UIWORD.RootUnit), width);//"Root Unit"
					break;
				case SELECTION_TYPE.Param:
					DrawTitle(Editor.GetUIWord(UIWORD.ControlParameter), width);//"Control Parameter"
					break;
			}
		}

		//public apPortrait Portrait { get { return _portrait; } }
		//public apTextureData TextureData {  get { if (_selectionType == SELECTION_TYPE.ImageRes) { return _image; } return null; } }
		//public apMesh Mesh {  get { if (_selectionType 

		private void Draw_None(int width, int height)
		{
			//GUILayout.Box("Not Selected", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Not Selected", width);
			EditorGUILayout.Space();
		}

		private void Draw_ImageRes(int width, int height)
		{
			//GUILayout.Box("Image", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Image", width);
			EditorGUILayout.Space();

			apTextureData textureData = _image;
			if (textureData == null)
			{
				SetNone();
				return;
			}

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ImageAsset));//"Image Asset"



			//textureData._image = EditorGUILayout.ObjectField(textureData._image, typeof(Texture2D), true, GUILayout.Width(width), GUILayout.Height(50)) as Texture2D;
			Texture2D nextImage = EditorGUILayout.ObjectField(textureData._image, typeof(Texture2D), true) as Texture2D;
			

			if (GUILayout.Button(Editor.GetUIWord(UIWORD.SelectImage), GUILayout.Height(30)))//"Select Image"
			{
				_loadKey_SelectTextureAsset = apDialog_SelectTextureAsset.ShowDialog(Editor, textureData, OnTextureAssetSelected);
			}

			if (textureData._image != nextImage)
			{
				//이미지가 추가되었다.
				if (nextImage != null)
				{
					//Undo
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Image_SettingChanged, Editor, Editor._portrait, textureData._image, false);

					textureData._image = nextImage;//이미지 추가
					textureData._name = textureData._image.name;
					textureData._width = textureData._image.width;
					textureData._height = textureData._image.height;

					//이미지 에셋의 Path를 확인하고, PSD인지 체크한다.
					if (textureData._image != null)
					{
						string fullPath = AssetDatabase.GetAssetPath(textureData._image);
						//Debug.Log("Image Path : " + fullPath);

						if (string.IsNullOrEmpty(fullPath))
						{
							textureData._assetFullPath = "";
							//textureData._isPSDFile = false;
						}
						else
						{
							textureData._assetFullPath = fullPath;
							//if (fullPath.Contains(".psd") || fullPath.Contains(".PSD"))
							//{
							//	textureData._isPSDFile = true;
							//}
							//else
							//{
							//	textureData._isPSDFile = false;
							//}
						}
					}
					else
					{
						textureData._assetFullPath = "";
						//textureData._isPSDFile = false;
					}
				}
				//Editor.Hierarchy.RefreshUnits();
				Editor.RefreshControllerAndHierarchy();
			}

			EditorGUILayout.Space();


			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Name));//"Name"
			string nextName = EditorGUILayout.DelayedTextField(textureData._name);

			EditorGUILayout.Space();

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Size));//"Size"

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Width), GUILayout.Width(40));//"Width"
			int nextWidth = EditorGUILayout.DelayedIntField(textureData._width);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Height), GUILayout.Width(40));//"Height"
			int nextHeight = EditorGUILayout.DelayedIntField(textureData._height);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();


			//변경값이 있으면 Undo 등록하고 변경
			if (!string.Equals(nextName, textureData._name) ||
				nextWidth != textureData._width ||
				nextHeight != textureData._height)
			{
				//Undo
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Image_SettingChanged, Editor, Editor._portrait, textureData, false);

				textureData._name = nextName;
				textureData._width = nextWidth;
				textureData._height = nextHeight;

				Editor.RefreshControllerAndHierarchy();
			}

			GUILayout.Space(20);
			if(textureData._image != null)
			{
				if (textureData._image != _imageImported || _imageImporter == null)
				{
					string path = AssetDatabase.GetAssetPath(textureData._image);
					_imageImported = textureData._image;
					_imageImporter = (TextureImporter)TextureImporter.GetAtPath(path);
				}
			}
			else
			{
				_imageImported = null;
				_imageImporter = null;
			}

			

			//텍스쳐 설정을 할 수 있다.
			if(_imageImporter != null)
			{
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(20);

				bool prev_sRGB = _imageImporter.sRGBTexture;
				TextureImporterCompression prev_compressed = _imageImporter.textureCompression;

				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ColorSpace));
				EditorGUILayout.LabelField("( " + Editor.GetUIWord(UIWORD.Current) + " : " + (apEditorUtil.IsGammaColorSpace() ? "Gamma" : "Linear") + " )");
				//sRGB True => Gamma Color Space이다.
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(24));
				GUILayout.Space(5);
				string[] colorSpaceNames = new string[] { "Gamma", "Linear" };
				int iColorSpace = _imageImporter.sRGBTexture ? 0 : 1;
				int nextColorSpace = EditorGUILayout.Popup(iColorSpace, colorSpaceNames);
				if(nextColorSpace != iColorSpace)
				{
					if(nextColorSpace == 0)
					{
						//Gamma : sRGB 사용
						_imageImporter.sRGBTexture = true;
					}
					else
					{
						//Linear : sRGB 사용 안함
						_imageImporter.sRGBTexture = false;
					}
				}
				EditorGUILayout.EndHorizontal();
				
				

				GUILayout.Space(5);
				int prevQuality = 0;
				if(_imageImporter.textureCompression == TextureImporterCompression.CompressedLQ)
				{
					prevQuality = 0;
				}
				else if(_imageImporter.textureCompression == TextureImporterCompression.Compressed)
				{
					prevQuality = 1;
				}
				else if(_imageImporter.textureCompression == TextureImporterCompression.CompressedHQ)
				{
					prevQuality = 2;
				}
				else if(_imageImporter.textureCompression == TextureImporterCompression.Uncompressed)
				{
					prevQuality = 3;
				}

				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Compression));//"Compression"
				string[] qualityNames = new string[] { "Compressed [Low Quality]", "Compressed [Default]", "Compressed [High Quality]", "Uncompressed" };
				int nextQuality = EditorGUILayout.Popup(prevQuality, qualityNames);

				GUILayout.Space(5);
				bool prevMipmap = _imageImporter.mipmapEnabled;
				_imageImporter.mipmapEnabled = EditorGUILayout.Toggle(Editor.GetUIWord(UIWORD.UseMipmap), _imageImporter.mipmapEnabled);//"Use Mipmap"

				if(nextQuality != prevQuality)
				{
					switch (nextQuality)
					{
						case 0://ComLQ
							_imageImporter.textureCompression = TextureImporterCompression.CompressedLQ;
							break;

						case 1://Com
							_imageImporter.textureCompression = TextureImporterCompression.Compressed;
							break;

						case 2://ComHQ
							_imageImporter.textureCompression = TextureImporterCompression.CompressedHQ;
							break;

						case 3://Uncom
							_imageImporter.textureCompression = TextureImporterCompression.Uncompressed;
							break;
					}
				}

				if (nextQuality != prevQuality ||
					_imageImporter.sRGBTexture != prev_sRGB ||
					_imageImporter.mipmapEnabled != prevMipmap)
				{

					_imageImporter.SaveAndReimport();
					_imageImporter = null;
					_imageImported = null;
					AssetDatabase.Refresh();
				}

				GUILayout.Space(20);
			}


			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(20);

			if (GUILayout.Button(Editor.GetUIWord(UIWORD.RefreshImageProperty), GUILayout.Height(30)))//"Refresh Image Property"
			{
				//Undo
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Image_SettingChanged, Editor, Editor._portrait, textureData, false);

				if (textureData._image != null)
				{
					textureData._name = textureData._image.name;
					textureData._width = textureData._image.width;
					textureData._height = textureData._image.height;
				}
				else
				{
					textureData._name = "";
					textureData._width = 0;
					textureData._height = 0;
				}
				//Editor.Hierarchy.RefreshUnits();
				Editor.RefreshControllerAndHierarchy();
			}

			// Remove
			GUILayout.Space(30);
			if (GUILayout.Button(	new GUIContent(	"  " + Editor.GetUIWord(UIWORD.RemoveImage),
													Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
													),
									GUILayout.Height(24)))//"  Remove Image"
			{

				//bool isResult = EditorUtility.DisplayDialog("Remove Image", "Do you want to remove [" + textureData._name + "]?", "Remove", "Cancel");
				
				//Texture를 삭제하면 영향을 받는 메시들을 확인하자
				string strDialogInfo = Editor.Controller.GetRemoveItemMessage(
															_portrait, 
															textureData,
															5,
															Editor.GetTextFormat(TEXT.RemoveImage_Body, textureData._name),
															Editor.GetText(TEXT.DLG_RemoveItemChangedWarning));

				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveImage_Title),
																strDialogInfo,
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel));


				if (isResult)
				{
					Editor.Controller.RemoveTexture(textureData);
					//_portrait._textureData.Remove(textureData);

					SetNone();
				}
				//Editor.Hierarchy.RefreshUnits();
				Editor.RefreshControllerAndHierarchy();
			}
		}

		private object _loadKey_SelectTextureAsset = null;
		private void OnTextureAssetSelected(bool isSuccess, apTextureData targetTextureData, object loadKey, Texture2D resultTexture2D)
		{
			if (_loadKey_SelectTextureAsset != loadKey || !isSuccess)
			{
				_loadKey_SelectTextureAsset = null;
				return;
			}
			_loadKey_SelectTextureAsset = null;
			if (targetTextureData == null)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Image_SettingChanged, Editor, Editor._portrait, targetTextureData, false);

			targetTextureData._image = resultTexture2D;
			//이미지가 추가되었다.
			if (targetTextureData._image != null)
			{
				

				targetTextureData._name = targetTextureData._image.name;
				targetTextureData._width = targetTextureData._image.width;
				targetTextureData._height = targetTextureData._image.height;

				//이미지 에셋의 Path를 확인하고, PSD인지 체크한다.
				if (targetTextureData._image != null)
				{
					string fullPath = AssetDatabase.GetAssetPath(targetTextureData._image);
					//Debug.Log("Image Path : " + fullPath);

					if (string.IsNullOrEmpty(fullPath))
					{
						targetTextureData._assetFullPath = "";
						//targetTextureData._isPSDFile = false;
					}
					else
					{
						targetTextureData._assetFullPath = fullPath;
						//if (fullPath.Contains(".psd") || fullPath.Contains(".PSD"))
						//{
						//	targetTextureData._isPSDFile = true;
						//}
						//else
						//{
						//	targetTextureData._isPSDFile = false;
						//}
					}
				}
				else
				{
					targetTextureData._assetFullPath = "";
					//targetTextureData._isPSDFile = false;
				}
			}
			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy();
		}





		//private bool _isShowTextureDataList = false;
		private void Draw_Mesh(int width, int height)
		{
			//GUILayout.Box("Mesh", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Mesh", width);
			EditorGUILayout.Space();

			if (_mesh == null)
			{
				SetNone();
				return;
			}

			//탭
			bool isEditMeshMode_None = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.Setting);
			bool isEditMeshMode_MakeMesh = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.MakeMesh);
			bool isEditMeshMode_Modify = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.Modify);

			//bool isEditMeshMode_AddVertex = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.AddVertex);
			//bool isEditMeshMode_LinkEdge = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.LinkEdge);

			bool isEditMeshMode_Pivot = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.PivotEdit);
			//bool isEditMeshMode_Volume = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.VolumeWeight);
			//bool isEditMeshMode_Physic = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.PhysicWeight);

			int subTabWidth = (width / 2) - 5;
			int subTabHeight = 24;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(subTabHeight));
			//int tabBtnHeight = 30;
			GUILayout.Space(5);

			//" Setting"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Setting), " " + Editor.GetUIWord(UIWORD.Setting), isEditMeshMode_None, true, subTabWidth, subTabHeight, "Settings of Mesh"))
			{
				if (!isEditMeshMode_None)
				{
					Editor.Controller.CheckMeshEdgeWorkRemained();
					Editor._meshEditMode = apEditor.MESH_EDIT_MODE.Setting;

					Editor.Gizmos.Unlink();
				}
			}
			//" Make Mesh"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MeshEditMenu), " " + Editor.GetUIWord(UIWORD.MakeMesh), isEditMeshMode_MakeMesh, true, subTabWidth, subTabHeight, "Make Vertices and Polygons"))
			{
				if (!isEditMeshMode_MakeMesh)
				{
					Editor.Controller.CheckMeshEdgeWorkRemained();
					Editor._meshEditMode = apEditor.MESH_EDIT_MODE.MakeMesh;
					Editor.Controller.StartMeshEdgeWork();
					Editor.VertController.SetMesh(_mesh);
					Editor.VertController.UnselectVertex();

					Editor.Gizmos.Unlink();
				}
			}


			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(subTabHeight));
			GUILayout.Space(5);

			//" Pivot"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_PivotMenu), " " + Editor.GetUIWord(UIWORD.Pivot), isEditMeshMode_Pivot, true, subTabWidth, subTabHeight, "Edit Pivot of Mesh"))
			{
				if (!isEditMeshMode_Pivot)
				{
					Editor.Controller.CheckMeshEdgeWorkRemained();
					Editor._meshEditMode = apEditor.MESH_EDIT_MODE.PivotEdit;

					Editor.Gizmos.Unlink();
				}
			}

			//" Modify"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_ModifyMenu), " " + Editor.GetUIWord(UIWORD.Modify), isEditMeshMode_Modify, true, subTabWidth, subTabHeight, "Modify Vertices"))
			{
				if (!isEditMeshMode_Modify)
				{
					Editor.Controller.CheckMeshEdgeWorkRemained();
					Editor._meshEditMode = apEditor.MESH_EDIT_MODE.Modify;

					Editor.Gizmos.Unlink();
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshEdit_Modify());
				}
			}

			#region [미사용 코드] Edge 수정은 Vertex와 통합되어 MakeMesh로 바뀜
			//if(apEditorUtil.ToggledButton("Edge", isEditMeshMode_LinkEdge, subTabWidth))
			//{
			//	if(!isEditMeshMode_LinkEdge)
			//	{
			//		Editor.Controller.StartMeshEdgeWork();
			//		Editor._meshEditMode = apEditor.MESH_EDIT_MODE.LinkEdge;
			//	}
			//} 
			#endregion
			EditorGUILayout.EndHorizontal();

			

			switch (Editor._meshEditMode)
			{
				case apEditor.MESH_EDIT_MODE.Setting:
					MeshProperty_None(width, height);
					break;

				case apEditor.MESH_EDIT_MODE.Modify:
					MeshProperty_Modify(width, height);
					break;

				case apEditor.MESH_EDIT_MODE.MakeMesh:
					MeshProperty_MakeMesh(width, height);
					break;
				//case apEditor.MESH_EDIT_MODE.AddVertex:
				//	MeshProperty_AddVertex(width, height);
				//	break;

				//case apEditor.MESH_EDIT_MODE.LinkEdge:
				//	MeshProperty_LinkEdge(width, height);
				//	break;

				case apEditor.MESH_EDIT_MODE.PivotEdit:
					MeshProperty_Pivot(width, height);
					break;

					//case apEditor.MESH_EDIT_MODE.VolumeWeight:
					//	MeshProperty_Volume(width, height);
					//	break;

					//case apEditor.MESH_EDIT_MODE.PhysicWeight:
					//	MeshProperty_Physic(width, height);
					//	break;
			}
			#region [미사용 코드] Sub 코드로 옮겨졌다.
			////0. 기본 정보
			//EditorGUILayout.LabelField("Name");
			//_mesh.transform.name = EditorGUILayout.TextField(_mesh.transform.name);

			//EditorGUILayout.Space();

			////1. 어느 텍스쳐를 사용할 것인가
			//EditorGUILayout.LabelField("Image");
			//apTextureData textureData = _mesh._textureData;

			//string strTextureName = "(No Image)";
			//Texture2D curTextureImage = null;
			//int selectedImageHeight = 20;
			//if(_mesh._textureData != null)
			//{
			//	strTextureName = _mesh._textureData._name;
			//	curTextureImage = _mesh._textureData._image;

			//	if(curTextureImage != null && _mesh._textureData._width > 0 && _mesh._textureData._height > 0)
			//	{
			//		selectedImageHeight = (int)((float)(width * _mesh._textureData._height) / (float)(_mesh._textureData._width));
			//	}
			//}

			//if (curTextureImage != null)
			//{
			//	//EditorGUILayout.TextField(strTextureName);
			//	EditorGUILayout.LabelField(strTextureName);
			//	EditorGUILayout.ObjectField(curTextureImage, typeof(Texture2D), false, GUILayout.Height(selectedImageHeight));
			//}
			//else
			//{
			//	EditorGUILayout.LabelField("(No Image)");
			//}

			//if(GUILayout.Button("Change Image", GUILayout.Height(30)))
			//{
			//	_isShowTextureDataList = !_isShowTextureDataList;
			//}

			//EditorGUILayout.Space();
			//if(_isShowTextureDataList)
			//{
			//	int nImage = _portrait._textureData.Count;
			//	for (int i = 0; i < nImage; i++)
			//	{
			//		if (i % 2 == 0)
			//		{
			//			EditorGUILayout.BeginHorizontal();
			//		}

			//		EditorGUILayout.BeginVertical(GUILayout.Width((width / 2) - 4));

			//		apTextureData curTextureData = _portrait._textureData[i];
			//		if(curTextureData == null)
			//		{
			//			continue;
			//		}

			//		//EditorGUILayout.LabelField("[" + (i + 1) + "] : " + curTextureData._name);
			//		//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//		int imageHeight = 20;
			//		if(curTextureData._image != null && curTextureData._width > 0 && curTextureData._height > 0)
			//		{
			//			//w : h = w' : h'
			//			//(w ' * h) / w = h'
			//			imageHeight = (int)((float)((width / 2 - 4) * curTextureData._height) / (float)(curTextureData._width));
			//		}
			//		EditorGUILayout.ObjectField(curTextureData._image, typeof(Texture2D), false, GUILayout.Height(imageHeight));
			//		if(GUILayout.Button("Select", GUILayout.Height(25)))
			//		{
			//			apEditorUtil.SetRecord("Change Image of Mesh", _mesh);

			//			bool isCheckToResetVertex = false;
			//			if(_mesh._vertexData == null || _mesh._vertexData.Count == 0)
			//			{
			//				isCheckToResetVertex = true;
			//			}

			//			_mesh._textureData = curTextureData;
			//			_isShowTextureDataList = false;

			//			//if(isCheckToResetVertex)
			//			//{
			//			//	if (EditorUtility.DisplayDialog("Reset Vertex", "Do you want to make Vertices automatically?", "Reset", "Stay"))
			//			//	{
			//			//		_mesh._vertexData.Clear();
			//			//		_mesh._indexBuffer.Clear();

			//			//		_mesh.ResetVerticesByImageOutline();
			//			//	}
			//			//}
			//		}
			//		//EditorGUILayout.EndHorizontal();

			//		EditorGUILayout.EndVertical();


			//		if(i % 2 == 1)
			//		{
			//			EditorGUILayout.EndHorizontal();
			//			GUILayout.Space(10);
			//		}
			//	}
			//	if(nImage % 2 == 1)
			//	{
			//		EditorGUILayout.EndHorizontal();
			//		GUILayout.Space(10);
			//	}

			//}

			//EditorGUILayout.Space();
			////_mesh._textureData = EditorGUILayout.ObjectField(_mesh._textureData, typeof(apTextureData), false);

			////2. 버텍스 세팅
			//if(GUILayout.Button("Reset Vertices"))
			//{
			//	if(_mesh._textureData != null && _mesh._textureData._image != null)
			//	{
			//		bool isConfirmReset = false;
			//		if(_mesh._vertexData != null && _mesh._vertexData.Count > 0 &&
			//			_mesh._indexBuffer != null && _mesh._indexBuffer.Count > 0)
			//		{
			//			isConfirmReset = EditorUtility.DisplayDialog("Reset Vertex", "If you reset vertices, All data is reset. Do not undo.", "Reset", "Cancel");
			//		}
			//		else
			//		{
			//			isConfirmReset = true;
			//		}

			//		if (isConfirmReset)
			//		{
			//			apEditorUtil.SetRecord("Reset Vertex", _mesh);

			//			_mesh._vertexData.Clear();
			//			_mesh._indexBuffer.Clear();
			//			_mesh._edges.Clear();
			//			_mesh._polygons.Clear();
			//			_mesh.MakeEdgesToPolygonAndIndexBuffer();

			//			_mesh.ResetVerticesByImageOutline();
			//		}
			//	}
			//}

			//// Remove
			//GUILayout.Space(30);
			//if(GUILayout.Button("Remove Mesh"))
			//{
			//	if(EditorUtility.DisplayDialog("Remove Mesh", "Do you want to remove [" + _mesh.name + "]?", "Remove", "Cancel"))
			//	{
			//		//apEditorUtil.SetRecord("Remove Mesh", _portrait);

			//		//MonoBehaviour.DestroyImmediate(_mesh.gameObject);
			//		//_portrait._meshes.Remove(_mesh);
			//		Editor.Controller.RemoveMesh(_mesh);

			//		SetNone();
			//	}
			//} 
			#endregion
		}

		private void Draw_Face(int width, int height)
		{
			//GUILayout.Box("Face", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Face", width);
			EditorGUILayout.Space();

		}

		private void Draw_MeshGroup(int width, int height)
		{
			//DrawTitle("Mesh Group", width);
			EditorGUILayout.Space();

			if (_meshGroup == null)
			{
				SetNone();
				return;
			}

			bool isEditMeshGroupMode_Setting = (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Setting);
			bool isEditMeshGroupMode_Bone = (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Bone);
			bool isEditMeshGroupMode_Modifier = (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Modifier);
			int subTabWidth = (width / 2) - 4;
			int subTabHeight = 24;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(subTabHeight));
			GUILayout.Space(5);

			//" Setting"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Setting), " " + Editor.GetUIWord(UIWORD.Setting), isEditMeshGroupMode_Setting, true, subTabWidth, subTabHeight, "Settings of Mesh Group"))
			{
				if (!isEditMeshGroupMode_Setting)
				{
					Editor._meshGroupEditMode = apEditor.MESHGROUP_EDIT_MODE.Setting;



					SetModMeshOfModifier(null);
					SetSubMeshGroupInGroup(null);
					SetSubMeshInGroup(null);
					SetModifier(null);

					SetBoneEditing(false, false);//Bone 처리는 종료 

					//Gizmo 컨트롤 방식을 Setting에 맞게 바꾸자
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshGroupSetting());



					SetModifierEditMode(EX_EDIT_KEY_VALUE.None);

					_rigEdit_isBindingEdit = false;
					_rigEdit_isTestPosing = false;
					SetBoneRiggingTest();

					//스크롤 초기화 (오른쪽2)
					Editor.ResetScrollPosition(false, false, false, true, false);
				}
			}

			//" Bone"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Bone), " " + Editor.GetUIWord(UIWORD.Bone), isEditMeshGroupMode_Bone, true, subTabWidth, subTabHeight, "Bones of Mesh Group"))
			{
				if (!isEditMeshGroupMode_Bone)
				{
					Editor._meshGroupEditMode = apEditor.MESHGROUP_EDIT_MODE.Bone;

					SetModMeshOfModifier(null);
					SetSubMeshGroupInGroup(null);
					SetSubMeshInGroup(null);
					SetModifier(null);

					//일단 Gizmo 초기화
					Editor.Gizmos.Unlink();

					_meshGroupChildHierarchy = MESHGROUP_CHILD_HIERARCHY.Bones;//하단 UI도 변경

					SetModifierEditMode(EX_EDIT_KEY_VALUE.ParamKey_Bone);

					_rigEdit_isBindingEdit = false;
					_rigEdit_isTestPosing = false;
					SetBoneRiggingTest();

					SetBoneEditing(false, true);

					//스크롤 초기화 (오른쪽2)
					Editor.ResetScrollPosition(false, false, false, true, false);
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(subTabHeight));
			GUILayout.Space(5);

			//Modifer
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Modifier), " " + Editor.GetUIWord(UIWORD.Modifier), isEditMeshGroupMode_Modifier, true, width - 5, subTabHeight, "Modifiers of Mesh Group"))
			{
				if (!isEditMeshGroupMode_Modifier)
				{
					SetBoneEditing(false, false);//Bone 처리는 종료 

					Editor._meshGroupEditMode = apEditor.MESHGROUP_EDIT_MODE.Modifier;

					bool isSelectMod = false;
					if (Modifier == null)
					{
						//이전에 선택했던 Modifier가 없다면..
						if (_meshGroup._modifierStack != null)
						{
							if (_meshGroup._modifierStack._modifiers.Count > 0)
							{
								//맨 위의 Modifier를 자동으로 선택해주자
								int nMod = _meshGroup._modifierStack._modifiers.Count;
								apModifierBase lastMod = _meshGroup._modifierStack._modifiers[nMod - 1];
								SetModifier(lastMod);
								isSelectMod = true;
							}
						}
					}
					else
					{
						SetModifier(Modifier);

						isSelectMod = true;
					}

					if (!isSelectMod)
					{
						SetModifier(null);
					}

					//스크롤 초기화 (오른쪽2)
					Editor.ResetScrollPosition(false, false, false, true, false);

				}
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);
			if (Editor._meshGroupEditMode != apEditor.MESHGROUP_EDIT_MODE.Setting)
			{
				_isMeshGroupSetting_ChangePivot = false;
			}

			switch (Editor._meshGroupEditMode)
			{
				case apEditor.MESHGROUP_EDIT_MODE.Setting:
					MeshGroupProperty_Setting(width, height);
					break;

				case apEditor.MESHGROUP_EDIT_MODE.Bone:
					MeshGroupProperty_Bone(width, height);
					break;

				case apEditor.MESHGROUP_EDIT_MODE.Modifier:
					MeshGroupProperty_Modify(width, height);
					break;
			}
		}





		//private string _prevAnimClipName = "";
		private object _loadKey_SelectMeshGroupToAnimClip = null;
		private object _loadKey_AddTimelineToAnimClip = null;

		private void Draw_Animation(int width, int height)
		{
			//GUILayout.Box("Animation", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Animation", width);
			EditorGUILayout.Space();

			if (_animClip == null)
			{
				SetNone();
				return;
			}

			//왼쪽엔 기본 세팅/ 우측 (Right2)엔 편집 도구들 + 생성된 Timeline리스트
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Name));//"Name"
			
			string nextAnimClipName = EditorGUILayout.DelayedTextField(_animClip._name, GUILayout.Width(width));

			if (!string.Equals(nextAnimClipName, _animClip._name))
			{
				_animClip._name = nextAnimClipName;
				Editor.RefreshControllerAndHierarchy();
			}

			#region [미사용 코드] Delayed Text Field를 사용하지 않았을 경우
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));

			//_prevAnimClipName = EditorGUILayout.TextField(_prevAnimClipName);
			//if (GUILayout.Button("Change", GUILayout.Width(80)))
			//{
			//	if (!string.IsNullOrEmpty(_prevAnimClipName))
			//	{
			//		_animClip._name = _prevAnimClipName;

			//		//Editor.Hierarchy.RefreshUnits();
			//		Editor.RefreshControllerAndHierarchy();
			//	}
			//}
			//EditorGUILayout.EndHorizontal(); 
			#endregion

			GUILayout.Space(5);
			//MeshGroup에 연동해야한다.

			GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
			guiStyle_Box.alignment = TextAnchor.MiddleCenter;
			guiStyle_Box.normal.textColor = apEditorUtil.BoxTextColor;

			Color prevColor = GUI.backgroundColor;

			if (_animClip._targetMeshGroup == null)
			{
				//GUI.color = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				//GUILayout.Box("Linked Mesh Group\n[ None ]", guiStyle_Box, GUILayout.Width(width), GUILayout.Height(40));
				//GUI.color = prevColor;

				//GUILayout.Space(2);

				//" Select MeshGroup"
				if (GUILayout.Button(new GUIContent(" " + Editor.GetUIWord(UIWORD.SelectMeshGroup), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup)), GUILayout.Width(width), GUILayout.Height(35)))
				{
					_loadKey_SelectMeshGroupToAnimClip = apDialog_SelectLinkedMeshGroup.ShowDialog(Editor, _animClip, OnSelectMeshGroupToAnimClip);
				}
			}
			else
			{
				//GUI.color = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				//GUILayout.Box("Linked Mesh Group\n[ " + _animClip._targetMeshGroup._name +" ]", guiStyle_Box, GUILayout.Width(width), GUILayout.Height(40));
				//GUI.color = prevColor;

				//GUILayout.Space(2);

				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.TargetMeshGroup));//"Target Mesh Group"
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				string strMeshGroupName = _animClip._targetMeshGroup._name;
				if(strMeshGroupName.Length > 16)
				{
					//이름이 너무 기네용.
					strMeshGroupName = strMeshGroupName.Substring(0, 14) + "..";
				}
				GUILayout.Box(strMeshGroupName, guiStyle_Box, GUILayout.Width(width - (80 + 2)), GUILayout.Height(18));
				GUI.backgroundColor = prevColor;
				if (GUILayout.Button(Editor.GetUIWord(UIWORD.Change), GUILayout.Width(80)))//"Change"
				{
					_loadKey_SelectMeshGroupToAnimClip = apDialog_SelectLinkedMeshGroup.ShowDialog(Editor, _animClip, OnSelectMeshGroupToAnimClip);
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);
				if (GUILayout.Button(Editor.GetUIWord(UIWORD.Duplicate), GUILayout.Width(width)))//"Duplicate"
				{
					Editor.Controller.DuplicateAnimClip(_animClip);
					Editor.RefreshControllerAndHierarchy();
				}
				GUILayout.Space(5);

				//Timeline을 추가하자
				//Timeline은 ControlParam, Modifier, Bone에 연동된다.
				//TimelineLayer은 각 Timeline에서 어느 Transform(Mesh/MeshGroup), Bone, ControlParam 에 적용 될지를 결정한다.
				//" Add Timeline"
				if (GUILayout.Button(new GUIContent(" " + Editor.GetUIWord(UIWORD.AddTimeline), Editor.ImageSet.Get(apImageSet.PRESET.Anim_AddTimeline)), GUILayout.Width(width), GUILayout.Height(30)))
				{
					_loadKey_AddTimelineToAnimClip = apDialog_AddAnimTimeline.ShowDialog(Editor, _animClip, OnAddTimelineToAnimClip);
				}

				//등록된 Timeline 리스트를 보여주자
				GUILayout.Space(10);
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(2);
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Timelines), GUILayout.Height(25));//"Timelines"

				GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
				GUILayout.Button("", guiStyle_None, GUILayout.Width(20), GUILayout.Height(20));//<레이아웃 정렬을 위한의미없는 숨은 버튼
				EditorGUILayout.EndHorizontal();

				//등록된 Modifier 리스트를 출력하자
				if (_animClip._timelines.Count > 0)
				{
					for (int i = 0; i < _animClip._timelines.Count; i++)
					{
						DrawTimelineUnit(_animClip._timelines[i], width, 25);
					}
				}
			}

			GUILayout.Space(20);

			apEditorUtil.GUI_DelimeterBoxH(width - 10);

			//등등
			GUILayout.Space(30);
			//"  Remove Animation"
			if (GUILayout.Button(new GUIContent(	"  " + Editor.GetUIWord(UIWORD.RemoveAnimation),
													Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
													), GUILayout.Height(24)))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Animation", "Do you want to remove [" + _animClip._name + "]?", "Remove", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveAnimClip_Title),
																Editor.GetTextFormat(TEXT.RemoveAnimClip_Body, _animClip._name),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel));
				if (isResult)
				{
					Editor.Controller.RemoveAnimClip(_animClip);

					SetNone();
					Editor.RefreshControllerAndHierarchy();
					Editor.RefreshTimelineLayers(true);
				}
			}
		}

		private void OnSelectMeshGroupToAnimClip(bool isSuccess, object loadKey, apMeshGroup meshGroup, apAnimClip targetAnimClip)
		{
			if (!isSuccess || _loadKey_SelectMeshGroupToAnimClip != loadKey
				|| meshGroup == null || _animClip != targetAnimClip)
			{
				_loadKey_SelectMeshGroupToAnimClip = null;
				return;
			}

			_loadKey_SelectMeshGroupToAnimClip = null;

			if (_animClip._targetMeshGroup != null)
			{
				if (_animClip._targetMeshGroup == meshGroup)
				{
					//바뀐게 없다 => Pass
					return;
				}

				//bool isResult = EditorUtility.DisplayDialog("Is Change Mesh Group", "Is Change Mesh Group?", "Change", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimClipMeshGroupChanged_Title),
																Editor.GetText(TEXT.AnimClipMeshGroupChanged_Body),
																Editor.GetText(TEXT.Okay),
																Editor.GetText(TEXT.Cancel)
																);
				if (!isResult)
				{
					//기존 것에서 변경을 하지 않는다 => Pass
					return;
				}
			}
			//Undo
			apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_SetMeshGroup, Editor, Editor._portrait, meshGroup, null, false);

			//기존의 Timeline이 있다면 다 날리자

			//_isAnimAutoKey = false;
			//_isAnimEditing = false;
			_exAnimEditingMode = EX_EDIT.None;
			_isAnimLock = false;

			SetAnimTimeline(null, true);
			SetSubMeshTransformForAnimClipEdit(null);//하나만 null을 하면 모두 선택이 취소된다.

			_animClip._timelines.Clear();
			bool isChanged = _animClip._targetMeshGroup != meshGroup;
			_animClip._targetMeshGroup = meshGroup;
			_animClip._targetMeshGroupID = meshGroup._uniqueID;


			if (meshGroup != null)
			{
				meshGroup._modifierStack.RefreshAndSort(true);
				meshGroup.ResetBoneGUIVisible();
			}
			if (isChanged)
			{
				//MeshGroup 선택 후 초기화
				if (_animClip._targetMeshGroup != null)
				{
					_animClip._targetMeshGroup.SetDirtyToReset();
					_animClip._targetMeshGroup.SetDirtyToSort();
					//_animClip._targetMeshGroup.SetAllRenderUnitForceUpdate();
					_animClip._targetMeshGroup.RefreshForce(true);

					_animClip._targetMeshGroup.LinkModMeshRenderUnits();
					_animClip._targetMeshGroup.RefreshModifierLink();

					_animClip._targetMeshGroup._modifierStack.RefreshAndSort(true);
				}


				Editor.Hierarchy_AnimClip.ResetSubUnits();
			}
			Editor.RefreshControllerAndHierarchy();

		}

		//Dialog 이벤트에 의해서 Timeline을 추가하자
		private void OnAddTimelineToAnimClip(bool isSuccess, object loadKey, apAnimClip.LINK_TYPE linkType, int modifierUniqueID, apAnimClip targetAnimClip)
		{
			if (!isSuccess || _loadKey_AddTimelineToAnimClip != loadKey ||
				_animClip != targetAnimClip)
			{
				_loadKey_AddTimelineToAnimClip = null;
				return;
			}

			_loadKey_AddTimelineToAnimClip = null;

			Editor.Controller.AddAnimTimeline(linkType, modifierUniqueID, targetAnimClip);
		}


		private void DrawTimelineUnit(apAnimTimeline timeline, int width, int height)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();
			Color textColor = GUI.skin.label.normal.textColor;

			if (AnimTimeline == timeline)
			{
				Color prevColor = GUI.backgroundColor;

				if(EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					textColor = Color.cyan;
				}
				else
				{
					GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					textColor = Color.white;
				}

				GUI.Box(new Rect(lastRect.x, lastRect.y + height, width + 15, height), "");
				GUI.backgroundColor = prevColor;
			}

			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			guiStyle_None.normal.textColor = textColor;

			apImageSet.PRESET iconType = apImageSet.PRESET.Anim_WithMod;
			switch (timeline._linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					iconType = apImageSet.PRESET.Anim_WithMod;
					break;

				case apAnimClip.LINK_TYPE.ControlParam:
					iconType = apImageSet.PRESET.Anim_WithControlParam;
					break;

					//case apAnimClip.LINK_TYPE.Bone:
					//	iconType = apImageSet.PRESET.Anim_WithBone;
					//	break;
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));
			GUILayout.Space(10);
			if (GUILayout.Button(new GUIContent(" " + timeline.DisplayName, Editor.ImageSet.Get(iconType)), guiStyle_None, GUILayout.Width(width - 40), GUILayout.Height(height)))
			{
				SetAnimTimeline(timeline, true);
				SetAnimTimelineLayer(null, true);
				SetAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
			}

			Texture2D activeBtn = null;
			bool isActiveMod = false;
			if (timeline._isActiveInEditing)
			{
				activeBtn = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Active);
				isActiveMod = true;
			}
			else
			{
				activeBtn = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Deactive);
				isActiveMod = false;
			}
			if (GUILayout.Button(activeBtn, guiStyle_None, GUILayout.Width(height), GUILayout.Height(height)))
			{
				//일단 토글한다.
				timeline._isActiveInEditing = !isActiveMod;
			}
			EditorGUILayout.EndHorizontal();
		}





		private void Draw_Overall(int width, int height)
		{
			//GUILayout.Box("Overall", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Overall", width);
			EditorGUILayout.Space();

			apRootUnit rootUnit = RootUnit;
			if (rootUnit == null)
			{
				SetNone();
				return;
			}

			Color prevColor = GUI.backgroundColor;

			//Setting / Capture Tab
			bool isRootUnitTab_Setting = (Editor._rootUnitEditMode == apEditor.ROOTUNIT_EDIT_MODE.Setting);
			bool isRootUnitTab_Capture = (Editor._rootUnitEditMode == apEditor.ROOTUNIT_EDIT_MODE.Capture);

			int subTabWidth = (width / 2) - 5;
			int subTabHeight = 24;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(subTabHeight));
			GUILayout.Space(5);

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Setting), " " + Editor.GetUIWord(UIWORD.Setting), isRootUnitTab_Setting, true, subTabWidth, subTabHeight, "Settings of Root Unit"))
			{
				if (!isRootUnitTab_Setting)
				{
					Editor._rootUnitEditMode = apEditor.ROOTUNIT_EDIT_MODE.Setting;
				}
			}

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Capture_Tab), " " + Editor.GetUIWord(UIWORD.Capture), isRootUnitTab_Capture, true, subTabWidth, subTabHeight, "Capturing the screenshot"))
			{
				if (!isRootUnitTab_Capture)
				{
					Editor._rootUnitEditMode = apEditor.ROOTUNIT_EDIT_MODE.Capture;
				}
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);
			
			if (Editor._rootUnitEditMode == apEditor.ROOTUNIT_EDIT_MODE.Setting)
			{
				//1. Setting 메뉴
				//------------------------------------------------
				//1. 연결된 MeshGroup 설정 (+ 해제)
				apMeshGroup targetMeshGroup = rootUnit._childMeshGroup;
				string strMeshGroupName = "";
				Color bgColor = Color.black;
				if (targetMeshGroup != null)
				{
					strMeshGroupName = "[" + targetMeshGroup._name + "]";
					bgColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				}
				else
				{
					strMeshGroupName = "Error! No MeshGroup Linked";
					bgColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				}
				GUI.backgroundColor = bgColor;

				GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
				guiStyleBox.alignment = TextAnchor.MiddleCenter;
				guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

				GUILayout.Box(strMeshGroupName, guiStyleBox, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;

				GUILayout.Space(20);
				apEditorUtil.GUI_DelimeterBoxH(width - 10);
				GUILayout.Space(20);

				//2. 애니메이션 제어

				apAnimClip curAnimClip = RootUnitAnimClip;
				bool isAnimClipAvailable = (curAnimClip != null);


				Texture2D icon_FirstFrame = Editor.ImageSet.Get(apImageSet.PRESET.Anim_FirstFrame);
				Texture2D icon_PrevFrame = Editor.ImageSet.Get(apImageSet.PRESET.Anim_PrevFrame);

				Texture2D icon_NextFrame = Editor.ImageSet.Get(apImageSet.PRESET.Anim_NextFrame);
				Texture2D icon_LastFrame = Editor.ImageSet.Get(apImageSet.PRESET.Anim_LastFrame);

				Texture2D icon_PlayPause = null;
				if (curAnimClip != null)
				{
					if (curAnimClip.IsPlaying_Editor) { icon_PlayPause = Editor.ImageSet.Get(apImageSet.PRESET.Anim_Pause); }
					else { icon_PlayPause = Editor.ImageSet.Get(apImageSet.PRESET.Anim_Play); }
				}
				else
				{
					icon_PlayPause = Editor.ImageSet.Get(apImageSet.PRESET.Anim_Play);
				}

				int btnSize = 30;
				int btnWidth_Play = 45;
				int btnWidth_PrevNext = 35;
				int btnWidth_FirstLast = (width - (btnWidth_Play + btnWidth_PrevNext * 2 + 4 * 3 + 5)) / 2;
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(btnSize));
				GUILayout.Space(2);
				if (apEditorUtil.ToggledButton_2Side(icon_FirstFrame, false, isAnimClipAvailable, btnWidth_FirstLast, btnSize))
				{
					if (curAnimClip != null)
					{
						curAnimClip.SetFrame_Editor(curAnimClip.StartFrame);
						curAnimClip.Pause_Editor();
					}
				}
				if (apEditorUtil.ToggledButton_2Side(icon_PrevFrame, false, isAnimClipAvailable, btnWidth_PrevNext, btnSize))
				{
					if (curAnimClip != null)
					{
						int prevFrame = curAnimClip.CurFrame - 1;
						if (prevFrame < curAnimClip.StartFrame && curAnimClip.IsLoop)
						{
							prevFrame = curAnimClip.EndFrame;
						}
						curAnimClip.SetFrame_Editor(prevFrame);
						curAnimClip.Pause_Editor();
					}
				}
				if (apEditorUtil.ToggledButton_2Side(icon_PlayPause, false, isAnimClipAvailable, btnWidth_Play, btnSize))
				{
					if (curAnimClip != null)
					{
						if (curAnimClip.IsPlaying_Editor)
						{
							curAnimClip.Pause_Editor();
						}
						else
						{
							if (curAnimClip.CurFrame == curAnimClip.EndFrame &&
								!curAnimClip.IsLoop)
							{
								curAnimClip.SetFrame_Editor(curAnimClip.StartFrame);
							}

							curAnimClip.Play_Editor();
						}
					}
				}
				if (apEditorUtil.ToggledButton_2Side(icon_NextFrame, false, isAnimClipAvailable, btnWidth_PrevNext, btnSize))
				{
					if (curAnimClip != null)
					{
						int nextFrame = curAnimClip.CurFrame + 1;
						if (nextFrame > curAnimClip.EndFrame && curAnimClip.IsLoop)
						{
							nextFrame = curAnimClip.StartFrame;
						}
						curAnimClip.SetFrame_Editor(nextFrame);
						curAnimClip.Pause_Editor();
					}
				}
				if (apEditorUtil.ToggledButton_2Side(icon_LastFrame, false, isAnimClipAvailable, btnWidth_FirstLast, btnSize))
				{
					if (curAnimClip != null)
					{
						curAnimClip.SetFrame_Editor(curAnimClip.EndFrame);
						curAnimClip.Pause_Editor();
					}
				}

				EditorGUILayout.EndHorizontal();

				int curFrame = 0;
				int startFrame = 0;
				int endFrame = 10;
				if (curAnimClip != null)
				{
					curFrame = curAnimClip.CurFrame;
					startFrame = curAnimClip.StartFrame;
					endFrame = curAnimClip.EndFrame;
				}
				int sliderFrame = EditorGUILayout.IntSlider(curFrame, startFrame, endFrame, GUILayout.Width(width));
				if (sliderFrame != curFrame)
				{
					curAnimClip.SetFrame_Editor(sliderFrame);
					curAnimClip.Pause_Editor();
				}

				GUILayout.Space(5);

				//추가 : 자동 플레이하는 AnimClip을 선택한다.
				bool isAutoPlayAnimClip = false;
				if (curAnimClip != null)
				{
					isAutoPlayAnimClip = (_portrait._autoPlayAnimClipID == curAnimClip._uniqueID);
				}
				if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.AutoPlayEnabled), Editor.GetUIWord(UIWORD.AutoPlayDisabled), isAutoPlayAnimClip, curAnimClip != null, width, 25))//"Auto Play Enabled", "Auto Play Disabled"
				{
					if (curAnimClip != null)
					{
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, Editor, _portrait, null, false);

						if (_portrait._autoPlayAnimClipID == curAnimClip._uniqueID)
						{
							//선택됨 -> 선택 해제
							_portrait._autoPlayAnimClipID = -1;

						}
						else
						{
							//선택 해제 -> 선택
							_portrait._autoPlayAnimClipID = curAnimClip._uniqueID;
						}


					}

				}


				GUILayout.Space(20);
				apEditorUtil.GUI_DelimeterBoxH(width - 10);
				GUILayout.Space(20);

				//3. 애니메이션 리스트
				List<apAnimClip> subAnimClips = RootUnitAnimClipList;
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.AnimationClips), GUILayout.Width(width));//"Animation Clips"
				GUILayout.Space(5);
				if (subAnimClips != null && subAnimClips.Count > 0)
				{
					apAnimClip nextSelectedAnimClip = null;

					GUIStyle guiNone = new GUIStyle(GUIStyle.none);
					guiNone.normal.textColor = GUI.skin.label.normal.textColor;

					GUIStyle guiSelected = new GUIStyle(GUIStyle.none);
					if (EditorGUIUtility.isProSkin)
					{
						guiSelected.normal.textColor = Color.cyan;
					}
					else
					{
						guiSelected.normal.textColor = Color.white;
					}



					Rect lastRect = GUILayoutUtility.GetLastRect();

					int scrollWidth = width - 20;

					Texture2D icon_Anim = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation);

					for (int i = 0; i < subAnimClips.Count; i++)
					{
						GUIStyle curGUIStyle = guiNone;

						apAnimClip subAnimClip = subAnimClips[i];
						if (subAnimClip == curAnimClip)
						{
							lastRect = GUILayoutUtility.GetLastRect();

							if (EditorGUIUtility.isProSkin)
							{
								GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
							}
							else
							{
								GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
							}

							//int offsetHeight = 20 + 3;
							int offsetHeight = 1 + 3;
							if (i == 0)
							{
								offsetHeight = 4 + 3;
							}

							GUI.Box(new Rect(lastRect.x, lastRect.y + offsetHeight, scrollWidth + 35, 24), "");

							GUI.backgroundColor = prevColor;

							curGUIStyle = guiSelected;
						}
						EditorGUILayout.BeginHorizontal(GUILayout.Width(scrollWidth - 5));
						GUILayout.Space(5);

						if (GUILayout.Button(new GUIContent(" " + subAnimClip._name, icon_Anim),
										curGUIStyle,
										GUILayout.Width(scrollWidth - 5), GUILayout.Height(24)))
						{
							nextSelectedAnimClip = subAnimClip;
						}
						EditorGUILayout.EndHorizontal();
						GUILayout.Space(4);

					}

					if (nextSelectedAnimClip != null)
					{
						for (int i = 0; i < Editor._portrait._animClips.Count; i++)
						{
							Editor._portrait._animClips[i]._isSelectedInEditor = false;
						}

						_curRootUnitAnimClip = nextSelectedAnimClip;
						_curRootUnitAnimClip.LinkEditor(Editor._portrait);
						_curRootUnitAnimClip.RefreshTimelines();
						_curRootUnitAnimClip.SetFrame_Editor(_curRootUnitAnimClip.StartFrame);
						_curRootUnitAnimClip.Pause_Editor();

						_curRootUnitAnimClip._isSelectedInEditor = true;


						//통계 재계산 요청
						SetStatisticsRefresh();

						//Debug.Log("Select Root Unit Anim Clip : " + _curRootUnitAnimClip._name);
					}
				}



				GUILayout.Space(20);
				apEditorUtil.GUI_DelimeterBoxH(width - 10);
				GUILayout.Space(20);
				//MainMesh에서 해제
				if (GUILayout.Button(Editor.GetUIWord(UIWORD.UnregistRootUnit), GUILayout.Width(width), GUILayout.Height(20)))//"Unregist Root Unit"
				{
					//Debug.LogError("TODO : MainMeshGroup 해제");
					apMeshGroup targetRootMeshGroup = rootUnit._childMeshGroup;
					if (targetRootMeshGroup != null)
					{
						apEditorUtil.SetRecord_PortraitMeshGroup(apUndoGroupData.ACTION.Portrait_SetMeshGroup, Editor, _portrait, targetRootMeshGroup, null, false, true);

						_portrait._mainMeshGroupIDList.Remove(targetRootMeshGroup._uniqueID);
						_portrait._mainMeshGroupList.Remove(targetRootMeshGroup);

						_portrait._rootUnits.Remove(rootUnit);

						SetNone();

						Editor.RefreshControllerAndHierarchy();
						Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.RootUnit, true);
					}
				}
			}
			else
			{
				//2. Capture 메뉴
				//-------------------------------------------
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(subTabHeight));
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Capture_Thumbnail), " " + Editor.GetUIWord(UIWORD.CaptureTabThumbnail), Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.Thumbnail, true, subTabWidth, subTabHeight, "Make a Thumbnail"))
				{
					//"Thumbnail"
					Editor._rootUnitCaptureMode = apEditor.ROOTUNIT_CAPTURE_MODE.Thumbnail;
				}

				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Capture_Image), " " + Editor.GetUIWord(UIWORD.CaptureTabScreenshot), Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.ScreenShot, true, subTabWidth, subTabHeight, "Make a Screenshot"))
				{
					//"Screen Shot"
					Editor._rootUnitCaptureMode = apEditor.ROOTUNIT_CAPTURE_MODE.ScreenShot;
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(subTabHeight));
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Capture_GIF), " " + Editor.GetUIWord(UIWORD.CaptureTabGIFAnim), Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.GIFAnimation, true, subTabWidth, subTabHeight, "Make a GIF Animation"))
				{
					//"GIF Anim"
					Editor._rootUnitCaptureMode = apEditor.ROOTUNIT_CAPTURE_MODE.GIFAnimation;
				}

				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Capture_Sprite), " " + Editor.GetUIWord(UIWORD.CaptureTabSpritesheet), Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.SpriteSheet, true, subTabWidth, subTabHeight, "Make Spritesheets"))
				{
					//"Spritesheet"
					Editor._rootUnitCaptureMode = apEditor.ROOTUNIT_CAPTURE_MODE.SpriteSheet;
				}
				EditorGUILayout.EndHorizontal();
				
				GUILayout.Space(10);
				

				int settingWidth_Label = 80;
				int settingWidth_Value = width - (settingWidth_Label + 8);

				//각 캡쳐별로 설정을 한다.
				//공통 설정도 있고 아닌 경우도 있다.

				//Setting
				//------------------------
				EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Setting));//"Setting"
				GUILayout.Space(5);
				
				//Position
				//------------------------
				EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Position));//"Position"

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField("X", GUILayout.Width(settingWidth_Label));
				int posX = EditorGUILayout.DelayedIntField(Editor._captureFrame_PosX, GUILayout.Width(settingWidth_Value));
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField("Y", GUILayout.Width(settingWidth_Label));
				int posY = EditorGUILayout.DelayedIntField(Editor._captureFrame_PosY, GUILayout.Width(settingWidth_Value));
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);

				//Capture Size
				//------------------------
				//Thumbnail인 경우 Width만 설정한다. (Height는 자동 계산)
				EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_CaptureSize));//"Capture Size"
				
				//Src Width
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				//"Width"
				EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Width), GUILayout.Width(settingWidth_Label));
				int srcSizeWidth = EditorGUILayout.DelayedIntField(Editor._captureFrame_SrcWidth, GUILayout.Width(settingWidth_Value));
				EditorGUILayout.EndHorizontal();


				int srcSizeHeight = Editor._captureFrame_SrcHeight;
				//Src Height : Tumbnail이 아닌 경우만
				if (Editor._rootUnitCaptureMode != apEditor.ROOTUNIT_CAPTURE_MODE.Thumbnail)
				{
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));

					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Height), GUILayout.Width(settingWidth_Label));//"Height"

					srcSizeHeight = EditorGUILayout.DelayedIntField(Editor._captureFrame_SrcHeight, GUILayout.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();
				}

				if (srcSizeWidth < 8) { srcSizeWidth = 8; }
				if (srcSizeHeight < 8) { srcSizeHeight = 8; }

				GUILayout.Space(5);

				//File Size
				//-------------------------------
				int dstSizeWidth = Editor._captureFrame_DstWidth;
				int dstSizeHeight = Editor._captureFrame_DstHeight;
				int spriteUnitSizeWidth = Editor._captureFrame_SpriteUnitWidth;
				int spriteUnitSizeHeight = Editor._captureFrame_SpriteUnitHeight;
				apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE spritePackImageWidth = Editor._captureSpritePackImageWidth;
				apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE spritePackImageHeight = Editor._captureSpritePackImageHeight;
				apEditor.CAPTURE_SPRITE_TRIM_METHOD spriteTrimSize = Editor._captureSpriteTrimSize;
				int spriteMargin = Editor._captureFrame_SpriteMargin;
				bool isPhysicsEnabled = Editor._captureFrame_IsPhysics;

				//Screenshot / GIF Animation은 Dst Image Size를 결정한다.
				if (Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.ScreenShot ||
					Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.GIFAnimation)
				{
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_ImageSize));//"Image Size"

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					//"Width"
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Width), GUILayout.Width(settingWidth_Label));
					dstSizeWidth = EditorGUILayout.DelayedIntField(Editor._captureFrame_DstWidth, GUILayout.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					//"Height"
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Height), GUILayout.Width(settingWidth_Label));
					dstSizeHeight = EditorGUILayout.DelayedIntField(Editor._captureFrame_DstHeight, GUILayout.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();
					
					GUILayout.Space(5);
				}
				else if(Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.SpriteSheet)
				{
					//Sprite Sheet는 Capture Unit과 Pack Image 사이즈, 압축 방식을 결정한다.
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ImageSizePerFrame));//"Image Size"
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					//"Width"
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Width), GUILayout.Width(settingWidth_Label));
					spriteUnitSizeWidth = EditorGUILayout.DelayedIntField(Editor._captureFrame_SpriteUnitWidth, GUILayout.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					//"Height"
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Height), GUILayout.Width(settingWidth_Label));
					spriteUnitSizeHeight = EditorGUILayout.DelayedIntField(Editor._captureFrame_SpriteUnitHeight, GUILayout.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();
					
					GUILayout.Space(5);

					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SizeofSpritesheet));//"Size of Sprite Sheet"
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					//"Width"
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Width), GUILayout.Width(settingWidth_Label));
					spritePackImageWidth = (apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE)EditorGUILayout.Popup((int)Editor._captureSpritePackImageWidth, new string[] { "256", "512", "1024", "2048", "4096"}, GUILayout.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					//"Height"
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Height), GUILayout.Width(settingWidth_Label));
					spritePackImageHeight = (apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE)EditorGUILayout.Popup((int)Editor._captureSpritePackImageHeight, new string[] { "256", "512", "1024", "2048", "4096"}, GUILayout.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();

					if((int)spritePackImageWidth < 0) { spritePackImageWidth = apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s256; }
					else if((int)spritePackImageWidth > 4) {  spritePackImageWidth = apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s4096; }

					if ((int)spritePackImageHeight < 0) { spritePackImageHeight = apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s256; }
					else if((int)spritePackImageHeight > 4) {  spritePackImageHeight = apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s4096; }
					
					GUILayout.Space(5);
					
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SpriteSizeCompression));//"Image size compression method"
					spriteTrimSize = (apEditor.CAPTURE_SPRITE_TRIM_METHOD)EditorGUILayout.EnumPopup(Editor._captureSpriteTrimSize, GUILayout.Width(width));

					GUILayout.Space(5);

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					//"Width"
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SpriteMargin), GUILayout.Width(settingWidth_Label));//"Margin"
					spriteMargin = EditorGUILayout.DelayedIntField(Editor._captureFrame_SpriteMargin, GUILayout.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();

				}

				
				
				//Color와 물리와 AspectRatio
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
				GUILayout.Space(5);

				EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_BGColor), GUILayout.Width(settingWidth_Label));//"BG Color"
				Color prevCaptureColor = Editor._captureFrame_Color;
				try
				{
					Editor._captureFrame_Color = EditorGUILayout.ColorField(Editor._captureFrame_Color, GUILayout.Width(settingWidth_Value));
				}
				catch (Exception) { }
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);

				if (Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.GIFAnimation ||
					Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.SpriteSheet)
				{
					//GIF, Spritesheet인 경우 물리 효과를 정해야 한다.
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
					GUILayout.Space(5);
					
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_CaptureIsPhysics), GUILayout.Width(width - (10 + 30)));
					isPhysicsEnabled = EditorGUILayout.Toggle(Editor._captureFrame_IsPhysics, GUILayout.Width(30));
					EditorGUILayout.EndHorizontal();
					GUILayout.Space(5);
				}

				GUILayout.Space(5);

				if (Editor._rootUnitCaptureMode != apEditor.ROOTUNIT_CAPTURE_MODE.Thumbnail)
				{
					//Thumbnail이 아니라면 Aspect Ratio가 중요하다
					//Aspect Ratio
					if (apEditorUtil.ToggledButton_2Side(Editor.GetText(TEXT.DLG_FixedAspectRatio), Editor.GetText(TEXT.DLG_NotFixedAspectRatio), Editor._isCaptureAspectRatioFixed, true, width, 20))
					{
						Editor._isCaptureAspectRatioFixed = !Editor._isCaptureAspectRatioFixed;

						if (Editor._isCaptureAspectRatioFixed)
						{
							//AspectRatio를 굳혔다.
							//Dst계열 변수를 Src에 맞춘다.
							//Height를 고정, Width를 맞춘다.
							if (Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.SpriteSheet)
							{
								//Spritesheet라면 Unit 사이즈를 변경
								Editor._captureFrame_SpriteUnitWidth = apEditorUtil.GetAspectRatio_Width(
																							Editor._captureFrame_SpriteUnitHeight, 
																							Editor._captureFrame_SrcWidth, 
																							Editor._captureFrame_SrcHeight);
								spriteUnitSizeWidth = Editor._captureFrame_SpriteUnitWidth;
							}
							else
							{
								//Screenshot과 GIF Animation이라면 Dst 사이즈를 변경
								Editor._captureFrame_DstWidth = apEditorUtil.GetAspectRatio_Width(
																							Editor._captureFrame_DstHeight, 
																							Editor._captureFrame_SrcWidth, 
																							Editor._captureFrame_SrcHeight);
								dstSizeWidth = Editor._captureFrame_DstWidth;
							}
							
						}

						Editor.SaveEditorPref();
						apEditorUtil.ReleaseGUIFocus();
					}

					GUILayout.Space(5);
				}

				


				//AspectRatio를 맞추어보자
				if (Editor._isCaptureAspectRatioFixed)
				{
					if (Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.ScreenShot || 
						Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.GIFAnimation)
					{
						//Screenshot / GIFAnimation은 Src, Dst를 서로 맞춘다.
						if (srcSizeWidth != Editor._captureFrame_SrcWidth)
						{
							//Width가 바뀌었다. => Height를 맞추자
							srcSizeHeight = apEditorUtil.GetAspectRatio_Height(srcSizeWidth, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
							//>> Dst도 바꾸자 => Width
							dstSizeWidth = apEditorUtil.GetAspectRatio_Width(dstSizeHeight, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
						}
						else if (srcSizeHeight != Editor._captureFrame_SrcHeight)
						{
							//Height가 바뀌었다. => Width를 맞추자
							srcSizeWidth = apEditorUtil.GetAspectRatio_Width(srcSizeHeight, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
							//>> Dst도 바꾸자 => Height
							dstSizeHeight = apEditorUtil.GetAspectRatio_Height(dstSizeWidth, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
						}
						else if (dstSizeWidth != Editor._captureFrame_DstWidth)
						{
							//Width가 바뀌었다. => Height를 맞추자
							dstSizeHeight = apEditorUtil.GetAspectRatio_Height(dstSizeWidth, Editor._captureFrame_DstWidth, Editor._captureFrame_DstHeight);
							//>> Src도 바꾸다 => Width
							srcSizeWidth = apEditorUtil.GetAspectRatio_Width(srcSizeHeight, Editor._captureFrame_DstWidth, Editor._captureFrame_DstHeight);
						}
						else if (dstSizeHeight != Editor._captureFrame_DstHeight)
						{
							//Height가 바뀌었다. => Width를 맞추자
							dstSizeWidth = apEditorUtil.GetAspectRatio_Width(dstSizeHeight, Editor._captureFrame_DstWidth, Editor._captureFrame_DstHeight);
							//>> Dst도 바꾸자 => Height
							srcSizeHeight = apEditorUtil.GetAspectRatio_Height(srcSizeWidth, Editor._captureFrame_DstWidth, Editor._captureFrame_DstHeight);
						}
					}
					else if(Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.SpriteSheet)
					{
						//Sprite sheet는 Src, Unit을 맞춘다.
						if (srcSizeWidth != Editor._captureFrame_SrcWidth)
						{
							//Width가 바뀌었다. => Height를 맞추자
							srcSizeHeight = apEditorUtil.GetAspectRatio_Height(srcSizeWidth, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
							//>> Dst도 바꾸자 => Width
							spriteUnitSizeWidth = apEditorUtil.GetAspectRatio_Width(spriteUnitSizeHeight, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
						}
						else if (srcSizeHeight != Editor._captureFrame_SrcHeight)
						{
							//Height가 바뀌었다. => Width를 맞추자
							srcSizeWidth = apEditorUtil.GetAspectRatio_Width(srcSizeHeight, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
							//>> Dst도 바꾸자 => Height
							spriteUnitSizeHeight = apEditorUtil.GetAspectRatio_Height(spriteUnitSizeWidth, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
						}
						else if (spriteUnitSizeWidth != Editor._captureFrame_SpriteUnitWidth)
						{
							//Width가 바뀌었다. => Height를 맞추자
							spriteUnitSizeHeight = apEditorUtil.GetAspectRatio_Height(spriteUnitSizeWidth, Editor._captureFrame_SpriteUnitWidth, Editor._captureFrame_SpriteUnitHeight);
							//>> Src도 바꾸다 => Width
							srcSizeWidth = apEditorUtil.GetAspectRatio_Width(srcSizeHeight, Editor._captureFrame_SpriteUnitWidth, Editor._captureFrame_SpriteUnitHeight);
						}
						else if (spriteUnitSizeHeight != Editor._captureFrame_SpriteUnitHeight)
						{
							//Height가 바뀌었다. => Width를 맞추자
							spriteUnitSizeWidth = apEditorUtil.GetAspectRatio_Width(spriteUnitSizeHeight, Editor._captureFrame_SpriteUnitWidth, Editor._captureFrame_SpriteUnitHeight);
							//>> Dst도 바꾸자 => Height
							srcSizeHeight = apEditorUtil.GetAspectRatio_Height(srcSizeWidth, Editor._captureFrame_SpriteUnitWidth, Editor._captureFrame_SpriteUnitHeight);
						}
					}
				}

				if (posX != Editor._captureFrame_PosX
					|| posY != Editor._captureFrame_PosY
					|| srcSizeWidth != Editor._captureFrame_SrcWidth
					|| srcSizeHeight != Editor._captureFrame_SrcHeight
					|| dstSizeWidth != Editor._captureFrame_DstWidth
					|| dstSizeHeight != Editor._captureFrame_DstHeight
					|| spriteUnitSizeWidth != Editor._captureFrame_SpriteUnitWidth
					|| spriteUnitSizeHeight != Editor._captureFrame_SpriteUnitHeight
					|| spritePackImageWidth != Editor._captureSpritePackImageWidth
					|| spritePackImageHeight != Editor._captureSpritePackImageHeight
					|| spriteTrimSize != Editor._captureSpriteTrimSize
					|| spriteMargin != Editor._captureFrame_SpriteMargin
					|| isPhysicsEnabled != Editor._captureFrame_IsPhysics
					)
				{
					Editor._captureFrame_PosX = posX;
					Editor._captureFrame_PosY = posY;

					if (srcSizeWidth < 10) { srcSizeWidth = 10; }
					if (srcSizeHeight < 10) { srcSizeHeight = 10; }
					Editor._captureFrame_SrcWidth = srcSizeWidth;
					Editor._captureFrame_SrcHeight = srcSizeHeight;

					if(dstSizeWidth < 10) { dstSizeWidth = 10; }
					if(dstSizeHeight < 10) { dstSizeHeight = 10; }
					Editor._captureFrame_DstWidth = dstSizeWidth;
					Editor._captureFrame_DstHeight = dstSizeHeight;

					if (spriteUnitSizeWidth < 10) { spriteUnitSizeWidth = 10; }
					if (spriteUnitSizeHeight < 10) { spriteUnitSizeHeight = 10; }
					Editor._captureFrame_SpriteUnitWidth = spriteUnitSizeWidth;
					Editor._captureFrame_SpriteUnitHeight = spriteUnitSizeHeight;

					Editor._captureSpritePackImageWidth = spritePackImageWidth;
					Editor._captureSpritePackImageHeight = spritePackImageHeight;
					Editor._captureSpriteTrimSize = spriteTrimSize;

					if(spriteMargin < 0) { spriteMargin = 0; }
					Editor._captureFrame_SpriteMargin = spriteMargin;
					
					Editor._captureFrame_IsPhysics = isPhysicsEnabled;

					Editor.SaveEditorPref();
					apEditorUtil.ReleaseGUIFocus();
				}

				if (Mathf.Abs(prevCaptureColor.r - Editor._captureFrame_Color.r) > 0.01f
					|| Mathf.Abs(prevCaptureColor.g - Editor._captureFrame_Color.g) > 0.01f
					|| Mathf.Abs(prevCaptureColor.b - Editor._captureFrame_Color.b) > 0.01f
					|| Mathf.Abs(prevCaptureColor.a - Editor._captureFrame_Color.a) > 0.01f)
				{
					_editor.SaveEditorPref();
					//색상은 GUIFocus를 null로 만들면 안되기에..
				}

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);

				switch (Editor._rootUnitCaptureMode)
				{
					case apEditor.ROOTUNIT_CAPTURE_MODE.Thumbnail:
						{
							//1. 썸네일 캡쳐
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_ThumbnailCapture));//"Thumbnail Capture"
							GUILayout.Space(5);
							string prev_ImageFilePath = _editor._portrait._imageFilePath_Thumbnail;
							
							//Preview 이미지
							GUILayout.Box(_editor._portrait._thumbnailImage, GUI.skin.label, GUILayout.Width(width), GUILayout.Height(width / 2));

							//File Path
							GUILayout.Space(5);
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_FilePath));//"File Path"
							EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
							GUILayout.Space(5);
							_editor._portrait._imageFilePath_Thumbnail = EditorGUILayout.TextField(_editor._portrait._imageFilePath_Thumbnail, GUILayout.Width(width - (68)));
							if (GUILayout.Button(_editor.GetText(TEXT.DLG_Change), GUILayout.Width(60)))//"Change"
							{
								string fileName = EditorUtility.SaveFilePanelInProject("Thumbnail File Path", _editor._portrait.name + "_Thumb.png", "png", "Please Enter a file name to save Thumbnail to");
								if (!string.IsNullOrEmpty(fileName))
								{
									_editor._portrait._imageFilePath_Thumbnail = fileName;
									apEditorUtil.ReleaseGUIFocus();
								}
							}
							EditorGUILayout.EndHorizontal();

							if (!_editor._portrait._imageFilePath_Thumbnail.Equals(prev_ImageFilePath))
							{
								//경로가 바뀌었다. -> 저장
								apEditorUtil.SetEditorDirty();

							}

							//썸네일 만들기 버튼
							if (GUILayout.Button(new GUIContent(" " + _editor.GetText(TEXT.DLG_MakeThumbnail), Editor.ImageSet.Get(apImageSet.PRESET.Capture_ExportThumb)), GUILayout.Width(width), GUILayout.Height(30)))
							{
								if (string.IsNullOrEmpty(_editor._portrait._imageFilePath_Thumbnail))
								{
									//EditorUtility.DisplayDialog("Thumbnail Creating Failed", "File Name is Empty", "Close");
									EditorUtility.DisplayDialog(_editor.GetText(TEXT.ThumbCreateFailed_Title),
																	_editor.GetText(TEXT.ThumbCreateFailed_Body_NoFile),
																	_editor.GetText(TEXT.Close)
																	);
								}
								else
								{
									//RequestExport(EXPORT_TYPE.Thumbnail);//<<이전 코드
									StartMakeThumbnail();//<<새로운 코드

								}
							}
						}
						break;

					case apEditor.ROOTUNIT_CAPTURE_MODE.ScreenShot:
						{
							//2. 스크린샷 캡쳐
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_ScreenshotCapture));//"Screenshot Capture"
							GUILayout.Space(5);

							if (GUILayout.Button(new GUIContent(" " + _editor.GetText(TEXT.DLG_TakeAScreenshot), Editor.ImageSet.Get(apImageSet.PRESET.Capture_ExportScreenshot)), GUILayout.Width(width), GUILayout.Height(30)))
							{
								StartTakeScreenShot();
							}
						}
						break;

					case apEditor.ROOTUNIT_CAPTURE_MODE.GIFAnimation:
						{
							//3. GIF 애니메이션 캡쳐
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_GIFAnimation));//"GIF Animation"
							GUILayout.Space(5);

							List<apAnimClip> subAnimClips = RootUnitAnimClipList;

							string animName = _editor.GetText(TEXT.DLG_NotAnimation);
							Color animBGColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
							if (_captureSelectedAnimClip != null)
							{
								animName = _captureSelectedAnimClip._name;
								animBGColor = new Color(0.7f, 1.0f, 0.7f, 1.0f);
							}

							Color prevGUIColor = GUI.backgroundColor;
							GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
							guiStyleBox.alignment = TextAnchor.MiddleCenter;
							guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

							GUI.backgroundColor = animBGColor;

							GUILayout.Box(animName, guiStyleBox, GUILayout.Width(width), GUILayout.Height(30));

							GUI.backgroundColor = prevGUIColor;

							GUILayout.Space(5);

							
							bool isDrawProgressBar = Editor.IsDelayedGUIVisible("Capture GIF ProgressBar");
							bool isDrawGIFAnimClips = Editor.IsDelayedGUIVisible("Capture GIF Clips");
							try
							{
								if (_captureMode != CAPTURE_MODE.None)
								{
									if (isDrawProgressBar)
									{
										//캡쳐 중에는 다른 UI 제어 불가

										if (_captureMode == CAPTURE_MODE.Capturing_GIF_Animation)
										{
											EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SpriteGIFWait));//"Please wait until finished. - TODO"
											//Rect lastRect = GUILayoutUtility.GetLastRect();

											//Rect barRect = new Rect(lastRect.x + 10, lastRect.y + 30, width - 20, 20);
											//float barRatio = (float)(_captureGIF_CurAnimProcess) / (float)(_captureGIF_TotalAnimProcess);

											float barRatio = Editor.SeqExporter.ProcessRatio;
											string barLabel = (int)(Mathf.Clamp01(barRatio) * 100.0f) + " %";

											//EditorGUI.ProgressBar(barRect, barRatio, barLabel);
											EditorUtility.DisplayProgressBar("Exporting to GIF", "Processing... " + barLabel, barRatio);
											_captureGIF_IsProgressDialog = true;
										}
									}

								}
								else
								{
									if (_captureGIF_IsProgressDialog)
									{
										EditorUtility.ClearProgressBar();
										_captureGIF_IsProgressDialog = false;
									}

									if (isDrawGIFAnimClips)
									{
										GUILayout.Space(10);
										apEditorUtil.GUI_DelimeterBoxH(width);
										GUILayout.Space(10);

										EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.CaptureScreenPosZoom));//"Screen Position and Zoom"
										GUILayout.Space(5);

										//화면 위치
										int width_ScreenPos = ((width - (10 + 30)) / 2) - 20;
										GUILayout.Space(5);
										EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
										GUILayout.Space(4);
										EditorGUILayout.LabelField("X", GUILayout.Width(15));
										Editor._captureSprite_ScreenPos.x = EditorGUILayout.DelayedFloatField(Editor._captureSprite_ScreenPos.x, GUILayout.Width(width_ScreenPos));
										EditorGUILayout.LabelField("Y", GUILayout.Width(15));
										Editor._captureSprite_ScreenPos.y = EditorGUILayout.DelayedFloatField(Editor._captureSprite_ScreenPos.y, GUILayout.Width(width_ScreenPos));
										GUIStyle guiStyle_SetBtn = new GUIStyle(GUI.skin.button);
										guiStyle_SetBtn.margin = GUI.skin.textField.margin;

										if(GUILayout.Button("Set", guiStyle_SetBtn, GUILayout.Width(30), GUILayout.Height(18)))
										{
											Editor._scroll_MainCenter = Editor._captureSprite_ScreenPos * 0.01f;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}

										EditorGUILayout.EndHorizontal();
										//Zoom

										Rect lastRect = GUILayoutUtility.GetLastRect();
										lastRect.x += 5;
										lastRect.y += 25;
										lastRect.width = width - (30 + 10 + 60 + 10);
										lastRect.height = 20;

										EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
										GUILayout.Space(6);
										GUILayout.Space(width - (30 + 10 + 60));
										//Editor._captureSprite_ScreenZoom = EditorGUILayout.IntSlider(Editor._captureSprite_ScreenZoom, 0, Editor._zoomListX100.Length - 1, GUILayout.Width(width - (30 + 10 + 40)));
										float fScreenZoom = GUI.HorizontalSlider(lastRect, Editor._captureSprite_ScreenZoom, 0, Editor._zoomListX100.Length - 1);
										Editor._captureSprite_ScreenZoom = Mathf.Clamp((int)fScreenZoom, 0, Editor._zoomListX100.Length - 1);

										EditorGUILayout.LabelField(Editor._zoomListX100[Editor._captureSprite_ScreenZoom] + "%", GUILayout.Width(60));
										if(GUILayout.Button("Set", guiStyle_SetBtn, GUILayout.Width(30), GUILayout.Height(18)))
										{
											Editor._iZoomX100 = Editor._captureSprite_ScreenZoom;
											if(Editor._iZoomX100 < 0)
											{
												Editor._iZoomX100 = 0;
											}
											else if(Editor._iZoomX100 >= Editor._zoomListX100.Length)
											{
												Editor._iZoomX100 = Editor._zoomListX100.Length - 1;
											}
											Editor._captureSprite_ScreenZoom = Editor._iZoomX100;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}
										
										EditorGUILayout.EndHorizontal();
										
										//"Focus To Center"
										if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.CaptureMoveToCenter), Editor.GetUIWord(UIWORD.CaptureMoveToCenter), false, true, width, 20))
										{
											Editor._scroll_MainCenter = Vector2.zero;
											Editor._captureSprite_ScreenPos = Vector2.zero;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}
										EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
										GUILayout.Space(4);
										//"Zoom -"
										if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.CaptureZoom) + " -", Editor.GetUIWord(UIWORD.CaptureZoom) + " -", false, true, width / 2 - 2, 20))
										{
											Editor._iZoomX100--;
											if (Editor._iZoomX100 < 0) { Editor._iZoomX100 = 0; }
											Editor._captureSprite_ScreenZoom = Editor._iZoomX100;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}
										//"Zoom +"
										if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.CaptureZoom) + " +", Editor.GetUIWord(UIWORD.CaptureZoom) + " +", false, true, width / 2 - 2, 20))
										{
											Editor._iZoomX100++;
											if (Editor._iZoomX100 >= Editor._zoomListX100.Length) { Editor._iZoomX100 = Editor._zoomListX100.Length - 1; }
											Editor._captureSprite_ScreenZoom = Editor._iZoomX100;
											Editor.SaveEditorPref();
										}
										EditorGUILayout.EndHorizontal();


										GUILayout.Space(10);
										apEditorUtil.GUI_DelimeterBoxH(width);
										GUILayout.Space(10);

										//Quality의 Min : 0, Max : 246이다.
										int gifQuality_Prev = Mathf.Clamp((int)(((float)(Mathf.Clamp(256 - _editor._captureFrame_GIFSampleQuality, 0, 246)) * 100.0f) / 246.0f), 0, 100);
										int gifQuality = gifQuality_Prev;

										string strQuality = "";
										if (gifQuality > 80)
										{
											//strQuality = "Quality [ High ]";
											strQuality = _editor.GetText(TEXT.DLG_QualityHigh);
										}
										else if (gifQuality > 30)
										{
											//strQuality = "Quality [ Medium ]";
											strQuality = _editor.GetText(TEXT.DLG_QualityMedium);
										}
										else
										{
											//strQuality = "Quality [ Low ]";
											strQuality = _editor.GetText(TEXT.DLG_QualityLow);
										}
										EditorGUILayout.LabelField(strQuality);

										//10 ~ 256
										//246 ~ 0
										gifQuality = EditorGUILayout.IntSlider(gifQuality, 0, 100, GUILayout.Width(width));

										//gifQuality = 256 - gifQuality;
										//if (_editor._captureFrame_GIFSampleQuality != gifQuality)
										if (gifQuality != gifQuality_Prev)
										{
											//0~100 => 0~246

											//_editor._captureFrame_GIFSampleQuality = gifQuality;
											_editor._captureFrame_GIFSampleQuality = 256 - Mathf.Clamp((int)((gifQuality * 246.0f) / 100.0f), 0, 246);
											_editor.SaveEditorPref();
										}

										GUILayout.Space(5);
										EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_LoopCount), GUILayout.Width(width));//"Loop Count"
										int loopCount = EditorGUILayout.DelayedIntField(_editor._captureFrame_GIFSampleLoopCount, GUILayout.Width(width));
										if (loopCount != _editor._captureFrame_GIFSampleLoopCount)
										{
											loopCount = Mathf.Clamp(loopCount, 1, 10);
											_editor._captureFrame_GIFSampleLoopCount = loopCount;
											_editor.SaveEditorPref();
										}

										GUILayout.Space(5);

										string strTakeAGIFAnimation = " " + _editor.GetText(TEXT.DLG_TakeAGIFAnimation);
										//"Take a GIF Animation", "Take a GIF Animation"
										if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Capture_ExportGIF), strTakeAGIFAnimation, strTakeAGIFAnimation, false, (_captureSelectedAnimClip != null), width, 30))
										{
											StartGIFAnimation();
										}

										GUILayout.Space(10);

										GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
										guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;

										GUIStyle guiStyle_Selected = new GUIStyle(GUIStyle.none);
										if (EditorGUIUtility.isProSkin)
										{
											guiStyle_Selected.normal.textColor = Color.cyan;
										}
										else
										{
											guiStyle_Selected.normal.textColor = Color.white;
										}

										//"Animation Clips"
										GUILayout.Button("  " + _editor.GetText(TEXT.DLG_AnimationClips), guiStyle_None, GUILayout.Width(width), GUILayout.Height(20));//투명 버튼

										//애니메이션 클립 리스트를 만들어야 한다.
										if (subAnimClips.Count > 0)
										{

											Texture2D iconImage = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation);

											apAnimClip nextSelectedAnimClip = null;
											for (int i = 0; i < subAnimClips.Count; i++)
											{
												GUIStyle curGUIStyle = guiStyle_None;

												apAnimClip animClip = subAnimClips[i];

												if (animClip == _captureSelectedAnimClip)
												{
													lastRect = GUILayoutUtility.GetLastRect();
													prevCaptureColor = GUI.backgroundColor;

													if (EditorGUIUtility.isProSkin)
													{
														GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
													}
													else
													{
														GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
													}

													GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width + 20, 20), "");
													GUI.backgroundColor = prevGUIColor;

													curGUIStyle = guiStyle_Selected;
												}

												EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
												GUILayout.Space(15);
												if (GUILayout.Button(new GUIContent(" " + animClip._name, iconImage), curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
												{
													nextSelectedAnimClip = animClip;
												}

												EditorGUILayout.EndHorizontal();
											}

											if (nextSelectedAnimClip != null)
											{
												for (int i = 0; i < _editor._portrait._animClips.Count; i++)
												{
													_editor._portrait._animClips[i]._isSelectedInEditor = false;
												}

												nextSelectedAnimClip.LinkEditor(_editor._portrait);
												nextSelectedAnimClip.RefreshTimelines();
												nextSelectedAnimClip.SetFrame_Editor(nextSelectedAnimClip.StartFrame);
												nextSelectedAnimClip.Pause_Editor();
												nextSelectedAnimClip._isSelectedInEditor = true;

												_captureSelectedAnimClip = nextSelectedAnimClip;

												_editor._portrait._animPlayManager.SetAnimClip_Editor(_captureSelectedAnimClip);
											}
										}
									}
								}
							}
							catch(Exception)
							{
								//Debug.LogError("GUI Exception : " + ex);
								//Debug.Log("Capture Mode : " + _captureMode);
								//Debug.Log("isDrawProgressBar : " + isDrawProgressBar);
								//Debug.Log("isDrawGIFAnimClips : " + isDrawGIFAnimClips);
								//Debug.Log("Event : " + Event.current.type);
							}

							Editor.SetGUIVisible("Capture GIF ProgressBar", _captureMode != CAPTURE_MODE.None);
							Editor.SetGUIVisible("Capture GIF Clips", _captureMode == CAPTURE_MODE.None);
						}
						break;

					case apEditor.ROOTUNIT_CAPTURE_MODE.SpriteSheet:
						{
							//4. 스프라이트 시트 캡쳐
							EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SpriteSheet));//"Sprite Sheet"
							GUILayout.Space(5);

							bool isDrawProgressBar = Editor.IsDelayedGUIVisible("Capture Spritesheet ProgressBar");
							bool isDrawSpritesheetSettings = Editor.IsDelayedGUIVisible("Capture Spritesheet Settings");

							try
							{
								if (_captureMode != CAPTURE_MODE.None)
								{
									if (isDrawProgressBar)
									{
										//캡쳐 중에는 다른 UI 제어 불가

										if (_captureMode == CAPTURE_MODE.Capturing_Spritesheet)
										{
											EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SpriteGIFWait));//"Please wait until finished. - TODO"
											//Rect lastRect = GUILayoutUtility.GetLastRect();

											//Rect barRect = new Rect(lastRect.x + 10, lastRect.y + 30, width - 20, 20);
											//float barRatio = (float)(_captureGIF_CurAnimProcess) / (float)(_captureGIF_TotalAnimProcess);

											float barRatio = Editor.SeqExporter.ProcessRatio;
											string barLabel = (int)(Mathf.Clamp01(barRatio) * 100.0f) + " %";

											//EditorGUI.ProgressBar(barRect, barRatio, barLabel);
											EditorUtility.DisplayProgressBar("Exporting to Sprite Sheet", "Processing... " + barLabel, barRatio);
											_captureGIF_IsProgressDialog = true;
										}
									}
								}
								else
								{
									if (_captureGIF_IsProgressDialog)
									{
										EditorUtility.ClearProgressBar();
										_captureGIF_IsProgressDialog = false;
									}

									if (isDrawSpritesheetSettings)
									{
										List<apAnimClip> subAnimClips = RootUnitAnimClipList;

										//그 전에 AnimClip 갱신부터
										if (!_captureSprite_IsAnimClipInit)
										{
											_captureSprite_AnimClips.Clear();
											_captureSprite_AnimClipFlags.Clear();
											for (int i = 0; i < subAnimClips.Count; i++)
											{
												_captureSprite_AnimClips.Add(subAnimClips[i]);
												_captureSprite_AnimClipFlags.Add(false);
											}

											_captureSprite_IsAnimClipInit = true;
										}

										Color prevGUIColor = GUI.backgroundColor;
										GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
										guiStyleBox.alignment = TextAnchor.MiddleCenter;
										guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

										GUI.backgroundColor = new Color(0.7f, 1.0f, 0.7f, 1.0f);

										string strNumOfSprites = Editor.GetUIWord(UIWORD.ExpectedNumSprites) + "\n";//"Expected number of sprites - TODO\n";
										int spriteTotalSize_X = 0;
										int spriteTotalSize_Y = 0;
										switch (Editor._captureSpritePackImageWidth)
										{
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s256: spriteTotalSize_X = 256; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s512: spriteTotalSize_X = 512; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024: spriteTotalSize_X = 1024; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s2048: spriteTotalSize_X = 2048; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s4096: spriteTotalSize_X = 4096; break;
											default: spriteTotalSize_X = 256; break;
										}

										switch (Editor._captureSpritePackImageHeight)
										{
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s256: spriteTotalSize_Y = 256; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s512: spriteTotalSize_Y = 512; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024: spriteTotalSize_Y = 1024; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s2048: spriteTotalSize_Y = 2048; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s4096: spriteTotalSize_Y = 4096; break;
											default: spriteTotalSize_Y = 256; break;
										}
										//X축 개수
										int numXOfSprite = -1;
										if (Editor._captureFrame_SpriteUnitWidth > 0 || Editor._captureFrame_SpriteUnitWidth < spriteTotalSize_X)
										{
											numXOfSprite = spriteTotalSize_X / Editor._captureFrame_SpriteUnitWidth;
										}

										//Y축 개수
										int numYOfSprite = -1;
										if (Editor._captureFrame_SpriteUnitHeight > 0 || Editor._captureFrame_SpriteUnitHeight < spriteTotalSize_Y)
										{
											numYOfSprite = spriteTotalSize_Y / Editor._captureFrame_SpriteUnitHeight;
										}
										if (numXOfSprite <= 0 || numYOfSprite <= 0)
										{
											strNumOfSprites += Editor.GetUIWord(UIWORD.InvalidSpriteSizeSettings);//"Invalid size settings";
											GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
										}
										else
										{
											strNumOfSprites += numXOfSprite + " X " + numYOfSprite;
										}
										GUILayout.Box(strNumOfSprites, guiStyleBox, GUILayout.Width(width), GUILayout.Height(40));

										GUI.backgroundColor = prevGUIColor;

										GUILayout.Space(5);

										

										//Export Format
										int width_ToggleLabel = width - (10 + 30);
										EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ExportMetaFile));//"Export Meta File - TODO"
										EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
										GUILayout.Space(5);
										EditorGUILayout.LabelField("XML", GUILayout.Width(width_ToggleLabel));
										bool isMetaXML = EditorGUILayout.Toggle(Editor._captureSpriteMeta_XML, GUILayout.Width(30));
										EditorGUILayout.EndHorizontal();

										EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
										GUILayout.Space(5);
										EditorGUILayout.LabelField("JSON", GUILayout.Width(width_ToggleLabel));
										bool isMetaJSON = EditorGUILayout.Toggle(Editor._captureSpriteMeta_JSON, GUILayout.Width(30));
										EditorGUILayout.EndHorizontal();

										EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
										GUILayout.Space(5);
										EditorGUILayout.LabelField("TXT", GUILayout.Width(width_ToggleLabel));
										bool isMetaTXT = EditorGUILayout.Toggle(Editor._captureSpriteMeta_TXT, GUILayout.Width(30));
										EditorGUILayout.EndHorizontal();

										if (isMetaXML != Editor._captureSpriteMeta_XML
											|| isMetaJSON != Editor._captureSpriteMeta_JSON
											|| isMetaTXT != Editor._captureSpriteMeta_TXT
											)
										{
											Editor._captureSpriteMeta_XML = isMetaXML;
											Editor._captureSpriteMeta_JSON = isMetaJSON;
											Editor._captureSpriteMeta_TXT = isMetaTXT;
											
											_editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}

										GUILayout.Space(5);

										EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.CaptureScreenPosZoom));//"Screen Position and Zoom"
										GUILayout.Space(5);

										//화면 위치
										int width_ScreenPos = ((width - (10 + 30)) / 2) - 20;
										GUILayout.Space(5);
										EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
										GUILayout.Space(4);
										EditorGUILayout.LabelField("X", GUILayout.Width(15));
										Editor._captureSprite_ScreenPos.x = EditorGUILayout.DelayedFloatField(Editor._captureSprite_ScreenPos.x, GUILayout.Width(width_ScreenPos));
										EditorGUILayout.LabelField("Y", GUILayout.Width(15));
										Editor._captureSprite_ScreenPos.y = EditorGUILayout.DelayedFloatField(Editor._captureSprite_ScreenPos.y, GUILayout.Width(width_ScreenPos));
										GUIStyle guiStyle_SetBtn = new GUIStyle(GUI.skin.button);
										guiStyle_SetBtn.margin = GUI.skin.textField.margin;

										if(GUILayout.Button("Set", guiStyle_SetBtn, GUILayout.Width(30), GUILayout.Height(18)))
										{
											Editor._scroll_MainCenter = Editor._captureSprite_ScreenPos * 0.01f;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}

										EditorGUILayout.EndHorizontal();
										//Zoom

										Rect lastRect = GUILayoutUtility.GetLastRect();
										lastRect.x += 5;
										lastRect.y += 25;
										lastRect.width = width - (30 + 10 + 60 + 10);
										lastRect.height = 20;

										EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
										GUILayout.Space(6);
										GUILayout.Space(width - (30 + 10 + 60));
										//Editor._captureSprite_ScreenZoom = EditorGUILayout.IntSlider(Editor._captureSprite_ScreenZoom, 0, Editor._zoomListX100.Length - 1, GUILayout.Width(width - (30 + 10 + 40)));
										float fScreenZoom = GUI.HorizontalSlider(lastRect, Editor._captureSprite_ScreenZoom, 0, Editor._zoomListX100.Length - 1);
										Editor._captureSprite_ScreenZoom = Mathf.Clamp((int)fScreenZoom, 0, Editor._zoomListX100.Length - 1);

										EditorGUILayout.LabelField(Editor._zoomListX100[Editor._captureSprite_ScreenZoom] + "%", GUILayout.Width(60));
										if(GUILayout.Button("Set", guiStyle_SetBtn, GUILayout.Width(30), GUILayout.Height(18)))
										{
											Editor._iZoomX100 = Editor._captureSprite_ScreenZoom;
											if(Editor._iZoomX100 < 0)
											{
												Editor._iZoomX100 = 0;
											}
											else if(Editor._iZoomX100 >= Editor._zoomListX100.Length)
											{
												Editor._iZoomX100 = Editor._zoomListX100.Length - 1;
											}
											Editor._captureSprite_ScreenZoom = Editor._iZoomX100;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}
										
										EditorGUILayout.EndHorizontal();
										
										//"Focus To Center"
										if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.CaptureMoveToCenter), Editor.GetUIWord(UIWORD.CaptureMoveToCenter), false, true, width, 20))
										{
											Editor._scroll_MainCenter = Vector2.zero;
											Editor._captureSprite_ScreenPos = Vector2.zero;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}
										EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
										GUILayout.Space(4);
										//"Zoom -"
										if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.CaptureZoom) + " -", Editor.GetUIWord(UIWORD.CaptureZoom) + " -", false, true, width / 2 - 2, 20))
										{
											Editor._iZoomX100--;
											if (Editor._iZoomX100 < 0) { Editor._iZoomX100 = 0; }
											Editor._captureSprite_ScreenZoom = Editor._iZoomX100;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}
										//"Zoom +"
										if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.CaptureZoom) + " +", Editor.GetUIWord(UIWORD.CaptureZoom) + " +", false, true, width / 2 - 2, 20))
										{
											Editor._iZoomX100++;
											if (Editor._iZoomX100 >= Editor._zoomListX100.Length) { Editor._iZoomX100 = Editor._zoomListX100.Length - 1; }
											Editor._captureSprite_ScreenZoom = Editor._iZoomX100;
											Editor.SaveEditorPref();
										}
										EditorGUILayout.EndHorizontal();

										GUILayout.Space(10);
										apEditorUtil.GUI_DelimeterBoxH(width);
										GUILayout.Space(10);

										int nAnimClipToExport = 0;
										for (int i = 0; i < _captureSprite_AnimClipFlags.Count; i++)
										{
											if(_captureSprite_AnimClipFlags[i])
											{
												nAnimClipToExport++;
											}
										}

										string strTakeSpriteSheets = " " + Editor.GetUIWord(UIWORD.CaptureExportSpriteSheets);//"Export Sprite Sheets - TODO";
										string strTakeSequenceFiles = " " + Editor.GetUIWord(UIWORD.CaptureExportSeqFiles);//"Export Sequence Files - TODO";
										if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Capture_ExportSprite), strTakeSpriteSheets, strTakeSpriteSheets, false, (numXOfSprite > 0 && numYOfSprite > 0 && nAnimClipToExport > 0), width, 30))
										{
											StartSpriteSheet(false);
										}
										if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Capture_ExportSequence), strTakeSequenceFiles, strTakeSequenceFiles, false, (nAnimClipToExport > 0), width, 25))
										{
											StartSpriteSheet(true);
										}
										GUILayout.Space(5);

										GUILayout.Space(10);
										apEditorUtil.GUI_DelimeterBoxH(width);
										GUILayout.Space(10);

										EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
										GUILayout.Space(4);
										//"Select All"
										if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.CaptureSelectAll), Editor.GetUIWord(UIWORD.CaptureSelectAll), false, true, width / 2 - 2, 20))
										{
											for (int i = 0; i < _captureSprite_AnimClipFlags.Count; i++)
											{
												_captureSprite_AnimClipFlags[i] = true;
											}
										}
										//"Deselect All"
										if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.CaptureDeselectAll), Editor.GetUIWord(UIWORD.CaptureDeselectAll), false, true, width / 2 - 2, 20))
										{
											for (int i = 0; i < _captureSprite_AnimClipFlags.Count; i++)
											{
												_captureSprite_AnimClipFlags[i] = false;
											}
										}
										EditorGUILayout.EndHorizontal();

										GUILayout.Space(10);

										//애니메이션 클립별로 "Export"할 것인지 지정
										GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
										guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;


										//"Animation Clips"
										GUILayout.Button("  " + _editor.GetText(TEXT.DLG_AnimationClips), guiStyle_None, GUILayout.Width(width), GUILayout.Height(20));//투명 버튼



										//애니메이션 클립 리스트를 만들어야 한다.
										if (subAnimClips.Count > 0)
										{

											Texture2D iconImage = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation);

											for (int i = 0; i < subAnimClips.Count; i++)
											{
												GUIStyle curGUIStyle = guiStyle_None;

												apAnimClip animClip = subAnimClips[i];

												//if (animClip == _captureSelectedAnimClip)
												//{
												//	Rect lastRect = GUILayoutUtility.GetLastRect();
												//	prevCaptureColor = GUI.backgroundColor;

												//	if (EditorGUIUtility.isProSkin)
												//	{
												//		GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
												//	}
												//	else
												//	{
												//		GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
												//	}

												//	GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width + 20, 20), "");
												//	GUI.backgroundColor = prevGUIColor;

												//	curGUIStyle = guiStyle_Selected;
												//}

												EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
												GUILayout.Space(15);
												if (GUILayout.Button(new GUIContent(" " + animClip._name, iconImage), curGUIStyle, GUILayout.Width((width - 35) - 35), GUILayout.Height(20)))
												{
													//nextSelectedAnimClip = animClip;
												}
												_captureSprite_AnimClipFlags[i] = EditorGUILayout.Toggle(_captureSprite_AnimClipFlags[i], GUILayout.Width(30), GUILayout.Height(20));

												EditorGUILayout.EndHorizontal();
											}


										}
									}
								}
							}
							catch(Exception ex)
							{
								Debug.LogError("GUI Exception : " + ex);
								Debug.Log("Capture Mode : " + _captureMode);
								Debug.Log("isDrawProgressBar : " + isDrawProgressBar);
								Debug.Log("isDrawSpritesheetSettings : " + isDrawSpritesheetSettings);
								Debug.Log("Event : " + Event.current.type);
							}
									
							Editor.SetGUIVisible("Capture Spritesheet ProgressBar", _captureMode != CAPTURE_MODE.None);
							Editor.SetGUIVisible("Capture Spritesheet Settings", _captureMode == CAPTURE_MODE.None);
						}
						break;
				}

				//이것도 Thumbnail이 아닌 경우
				//Screenshot + GIF Animation : _captureFrame_DstWidth x _captureFrame_DstHeight을 사용한다.
				//Sprite Sheet : 단위 유닛 크기 / 전체 이미지 파일 크기 (_captureFrame_DstWidth)의 두가지를 이용한다.

				//1) Setting
				//Position + Capture Size + File Size / BG Color / Aspect Ratio Fixed
				
				//<Export 방식은 탭으로 구분한다>
				//2) Thumbnail
				// Size (Width) Preview / Path + Change / Make Thumbnail
				//3) Screen Shot
				// Size (Width / Height x Src + Dst) / Take a Screenshot
				//4) GIF Animation
				// Size (Width / Height x Src + Dst) Animation Clip Name / Quality / Loop Count / Animation Clips / Take a GIF Animation + ProgressBar
				//5) Sprite
				//- Size (개별 캡쳐 크기 / 전체 이미지 크기 / 
				//- 출력 방식 : 스프라이트 시트 Only / Sprite + XML / Sprite + JSON
				//- 
			}
		}

		
		private void StartMakeThumbnail()
		{
			_captureMode = CAPTURE_MODE.Capturing_Thumbnail;

			//썸네일 크기
			int thumbnailWidth = 256;
			int thumbnailHeight = 128;

			float preferAspectRatio = (float)thumbnailWidth / (float)thumbnailHeight;

			float srcAspectRatio = (float)_editor._captureFrame_SrcWidth / (float)_editor._captureFrame_SrcHeight;
			//긴쪽으로 캡쳐 크기를 맞춘다.
			int srcThumbWidth = _editor._captureFrame_SrcWidth;
			int srcThumbHeight = _editor._captureFrame_SrcHeight;

			//AspectRatio = W / H
			if (srcAspectRatio < preferAspectRatio)
			{
				//가로가 더 길군요. 가로를 자릅시다.
				//H = W / AspectRatio;
				srcThumbHeight = (int)((srcThumbWidth / preferAspectRatio) + 0.5f);
			}
			else
			{
				//세로가 더 길군요. 세로를 자릅시다.
				//W = AspectRatio * H
				srcThumbWidth = (int)((srcThumbHeight * preferAspectRatio) + 0.5f);
			}

			//Request를 만든다.
			apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
			_captureLoadKey = newRequest.MakeScreenShot(OnThumbnailCaptured,
														_editor,
														_editor.Select.RootUnit._childMeshGroup,
														(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x), 
														(int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
														srcThumbWidth, srcThumbHeight,
														thumbnailWidth, thumbnailHeight,
														Editor._scroll_MainCenter, Editor._iZoomX100,
														_editor._captureFrame_Color, 0, "");

			//에디터에 대신 렌더링해달라고 요청을 합시다.
			Editor.ScreenCaptureRequest(newRequest);
			Editor.SetRepaint();
		}


		// 2. PNG 스크린샷
		private void StartTakeScreenShot()
		{
			try
			{
				string defFileName = "ScreenShot_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".png";
				string saveFilePath = EditorUtility.SaveFilePanel("Save Screenshot as PNG", _capturePrevFilePath_Directory, defFileName, "png");
				if (!string.IsNullOrEmpty(saveFilePath))
				{
					_captureMode = CAPTURE_MODE.Capturing_ScreenShot;

					//Request를 만든다.
					apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
					_captureLoadKey = newRequest.MakeScreenShot(OnScreeenShotCaptured,
																_editor,
																_editor.Select.RootUnit._childMeshGroup,
																(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x), 
																(int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
																_editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight,
																_editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight,
																Editor._scroll_MainCenter, Editor._iZoomX100,
																_editor._captureFrame_Color, 0, saveFilePath);

					//에디터에 대신 렌더링해달라고 요청을 합시다.
					Editor.ScreenCaptureRequest(newRequest);
					Editor.SetRepaint();
				}
			}
			catch (Exception)
			{
				
			}
		}


		//3. GIF 애니메이션 만들기
		private void StartGIFAnimation()
		{
			if (_captureSelectedAnimClip == null || _editor.Select.RootUnit._childMeshGroup == null)
			{
				return;
			}
			
			string defFileName = "GIF_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".gif";
			string saveFilePath = EditorUtility.SaveFilePanel("Save GIF Animation", _capturePrevFilePath_Directory, defFileName, "gif");
			if (!string.IsNullOrEmpty(saveFilePath))
			{
				

				bool isResult = Editor.SeqExporter.StartGIFAnimation(_editor.Select.RootUnit, _captureSelectedAnimClip, _editor._captureFrame_GIFSampleLoopCount, _editor._captureFrame_GIFSampleQuality, saveFilePath, OnGIFAnimationSaved);

				if (isResult)
				{
					System.IO.FileInfo fi = new System.IO.FileInfo(saveFilePath);
					_capturePrevFilePath_Directory = fi.Directory.FullName;
					_captureMode = CAPTURE_MODE.Capturing_GIF_Animation;
				}
				#region [미사용 코드 : Sequence Exporter를 이용하자]
				////애니메이션 정보를 저장한다.
				//_captureGIF_IsLoopAnimation = _captureSelectedAnimClip.IsLoop;
				//_captureGIF_IsAnimFirstFrame = true;
				//_captureGIF_StartAnimFrame = _captureSelectedAnimClip.StartFrame;
				//_captureGIF_LastAnimFrame = _captureSelectedAnimClip.EndFrame;

				//_captureGIF_AnimLoopCount = _editor._captureFrame_GIFSampleLoopCount;
				//if (_captureGIF_AnimLoopCount < 1)
				//{
				//	_captureGIF_AnimLoopCount = 1;
				//}

				//_captureGIF_GifAnimQuality = _editor._captureFrame_GIFSampleQuality;

				//if (_captureGIF_IsLoopAnimation)
				//{
				//	_captureGIF_LastAnimFrame--;//루프인 경우 마지막 프레임은 제외
				//}

				//if (_captureGIF_LastAnimFrame < _captureGIF_StartAnimFrame)
				//{
				//	_captureGIF_LastAnimFrame = _captureGIF_StartAnimFrame;
				//}

				//_editor._portrait._animPlayManager.Stop_Editor();
				//_editor._portrait._animPlayManager.SetAnimClip_Editor(_captureSelectedAnimClip);

				//_captureGIF_CurAnimLoop = 0;
				//_captureGIF_CurAnimFrame = _captureGIF_StartAnimFrame;

				//_captureGIF_CurAnimProcess = 0;
				//_captureGIF_TotalAnimProcess = (Mathf.Abs(_captureGIF_LastAnimFrame - _captureGIF_StartAnimFrame) + 1) * _captureGIF_AnimLoopCount;


				////1. GIF 헤더를 만들고
				////2. 이제 프레임을 하나씩 렌더링하기 시작하자

				//_captureMode = CAPTURE_MODE.Capturing_GIF_Animation;

				////GIF 헤더
				//bool isHeaderResult = _editor.Exporter.MakeGIFHeader(saveFilePath, _captureSelectedAnimClip, _editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight);

				//if(!isHeaderResult)
				//{
				//	//실패한 경우
				//	_captureMode = CAPTURE_MODE.None;
				//	return;
				//}


				////첫번째 프레임
				////Request를 만든다.
				//apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
				//_captureLoadKey = newRequest.MakeAnimCapture(OnGIFFrameCaptured,
				//											_editor,
				//											_editor.Select.RootUnit._childMeshGroup,
				//											true,
				//											_captureSelectedAnimClip, _captureGIF_CurAnimFrame,
				//											(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x),
				//											(int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
				//											_editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight,
				//											_editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight,
				//											_editor._captureFrame_Color, _captureGIF_CurAnimProcess, saveFilePath);

				////에디터에 대신 렌더링해달라고 요청을 합시다.
				//_editor.ScreenCaptureRequest(newRequest);
				//_editor.SetRepaint(); 
				#endregion
			}
		}


		//4. Sprite Sheet로 만들기
		private void StartSpriteSheet(bool isSequenceFiles)
		{
			if(RootUnitAnimClipList.Count != _captureSprite_AnimClipFlags.Count || _editor.Select.RootUnit._childMeshGroup == null)
			{
				return;
			}
			string defFileName = "";
			string saveFileDialogTitle = "";
			if(!isSequenceFiles)
			{
				defFileName = "Spritesheet_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".png";
				saveFileDialogTitle = "Save Spritesheet";
			}
			else
			{
				defFileName = "Sequence_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".png";
				saveFileDialogTitle = "Save Sequence Files";
			}
			
			string saveFilePath = EditorUtility.SaveFilePanel(saveFileDialogTitle, _capturePrevFilePath_Directory, defFileName, "png");
			if (!string.IsNullOrEmpty(saveFilePath))
			{


				bool isResult = Editor.SeqExporter.StartSpritesheet(_editor.Select.RootUnit,
													RootUnitAnimClipList,
													_captureSprite_AnimClipFlags,
													saveFilePath,
													_editor._captureSpriteTrimSize == apEditor.CAPTURE_SPRITE_TRIM_METHOD.Compressed,
													isSequenceFiles,
													Editor._captureFrame_SpriteMargin,
													Editor._captureSpriteMeta_XML,
													Editor._captureSpriteMeta_JSON,
													Editor._captureSpriteMeta_TXT,
													OnSpritesheetSaved);

				if (isResult)
				{
					System.IO.FileInfo fi = new System.IO.FileInfo(saveFilePath);
					_capturePrevFilePath_Directory = fi.Directory.FullName;
					_captureMode = CAPTURE_MODE.Capturing_Spritesheet;
				}
			}
		}



		private void OnThumbnailCaptured(bool isSuccess, Texture2D captureImage, int iProcessStep, string filePath, object loadKey)
		{
			_captureMode = CAPTURE_MODE.None;

			//우왕 왔당
			if (!isSuccess || captureImage == null)
			{
				//Debug.LogError("Failed..");
				if(captureImage != null)
				{
					UnityEngine.GameObject.DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;
				
				return;
			}
			if(_captureLoadKey != loadKey)
			{
				//Debug.LogError("LoadKey Mismatched");

				if(captureImage != null)
				{
					UnityEngine.GameObject.DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;
				return;
			}


			//이제 처리합시당 (Destroy도 포함되어있다)
			string filePathWOExtension = _editor._portrait._imageFilePath_Thumbnail.Substring(0, _editor._portrait._imageFilePath_Thumbnail.Length - 4);
			bool isSaveSuccess = _editor.Exporter.SaveTexture2DToPNG(captureImage, filePathWOExtension, true);

			if (isSaveSuccess)
			{
				AssetDatabase.Refresh();

				_editor._portrait._thumbnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(_editor._portrait._imageFilePath_Thumbnail);
			}
		}


		private void OnScreeenShotCaptured(bool isSuccess, Texture2D captureImage, int iProcessStep, string filePath, object loadKey)
		{
			_captureMode = CAPTURE_MODE.None;

			//우왕 왔당
			if (!isSuccess || captureImage == null || string.IsNullOrEmpty(filePath))
			{
				//Debug.LogError("Failed..");
				if(captureImage != null)
				{
					UnityEngine.GameObject.DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;
				
				return;
			}
			if(_captureLoadKey != loadKey)
			{
				//Debug.LogError("LoadKey Mismatched");

				if(captureImage != null)
				{
					UnityEngine.GameObject.DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;
				return;
			}

			//이제 파일로 저장하자
			try
			{
				string filePathWOExtension = filePath.Substring(0, filePath.Length - 4);

				//AutoDestroy = true
				bool isSaveSuccess = _editor.Exporter.SaveTexture2DToPNG(captureImage, filePathWOExtension, true);

				if (isSaveSuccess)
				{
					System.IO.FileInfo fi = new System.IO.FileInfo(filePath);

					Application.OpenURL("file://" + fi.Directory.FullName);
					Application.OpenURL("file://" + filePath);

					//_prevFilePath = filePath;
					_capturePrevFilePath_Directory = fi.Directory.FullName;
				}
			}
			catch(Exception)
			{

			}
		}




		

		private void OnGIFAnimationSaved(bool isResult)
		{
			_captureMode = CAPTURE_MODE.None;
		}

		private void OnSpritesheetSaved(bool isResult)
		{
			//Debug.LogError("OnSpritesheetSaved : " + isResult);
			_captureMode = CAPTURE_MODE.None;
		}

		//------------------------------------------------------------------------------------------------------------------------
		private apControlParam _prevParam = null;
		//private string _prevParamName = "";
		private void Draw_Param(int width, int height)
		{
			EditorGUILayout.Space();

			apControlParam cParam = _param;
			if (cParam == null)
			{
				SetNone();
				return;
			}
			if (_prevParam != cParam)
			{
				_prevParam = cParam;
				//_prevParamName = cParam._keyName;
			}
			if (cParam._isReserved)
			{
				GUIStyle guiStyle_RedTextColor = new GUIStyle(GUI.skin.label);
				guiStyle_RedTextColor.normal.textColor = Color.red;
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ReservedParameter), guiStyle_RedTextColor);//"Reserved Parameter"
				GUILayout.Space(10);
			}

			//bool isChanged = false;
			apControlParam.CATEGORY next_category = cParam._category;
			apControlParam.ICON_PRESET next_iconPreset = cParam._iconPreset;
			apControlParam.TYPE next_valueType = cParam._valueType;

			string next_label_Min = cParam._label_Min;
			string next_label_Max = cParam._label_Max;
			int next_snapSize = cParam._snapSize;

			int next_int_Def = cParam._int_Def;
			float next_float_Def = cParam._float_Def;
			Vector2 next_vec2_Def = cParam._vec2_Def;
			int next_int_Min = cParam._int_Min;
			int next_int_Max = cParam._int_Max;
			float next_float_Min = cParam._float_Min;
			float next_float_Max = cParam._float_Max;
			Vector2 next_vec2_Min = cParam._vec2_Min;
			Vector2 next_vec2_Max = cParam._vec2_Max;





			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.NameUnique));//"Name (Unique)"

			if (cParam._isReserved)
			{
				EditorGUILayout.LabelField(cParam._keyName, GUI.skin.textField, GUILayout.Width(width));
			}
			else
			{
				string nextKeyName = EditorGUILayout.DelayedTextField(cParam._keyName, GUILayout.Width(width));
				if (!string.Equals(nextKeyName, cParam._keyName))
				{
					if (string.IsNullOrEmpty(nextKeyName))
					{
						//이름이 빈칸이다
						//EditorUtility.DisplayDialog("Error", "Empty Name is not allowed", "Okay");

						EditorUtility.DisplayDialog(Editor.GetText(TEXT.ControlParamNameError_Title),
													Editor.GetText(TEXT.ControlParamNameError_Body_Wrong),
													Editor.GetText(TEXT.Close));
					}
					else if (Editor.ParamControl.FindParam(nextKeyName) != null)
					{
						//이미 사용중인 이름이다.
						//EditorUtility.DisplayDialog("Error", "It is used Name", "Okay");
						EditorUtility.DisplayDialog(Editor.GetText(TEXT.ControlParamNameError_Title),
												Editor.GetText(TEXT.ControlParamNameError_Body_Used),
												Editor.GetText(TEXT.Close));
					}
					else
					{
						

						Editor.Controller.ChangeParamName(cParam, nextKeyName);
						cParam._keyName = nextKeyName;
					}
				}
			}
			#region [미사용 코드] DelayedTextField를 사용하기 전
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//if (cParam._isReserved)
			//{
			//	//TextField의 Skin을 사용하지만 작동은 불가능한 Label
			//	EditorGUILayout.LabelField(cParam._keyName, GUI.skin.textField);
			//	//EditorGUILayout.TextField(cParam._keyName);
			//}
			//else
			//{
			//	_prevParamName = EditorGUILayout.TextField(_prevParamName);
			//	if (GUILayout.Button("Change", GUILayout.Width(60)))
			//	{
			//		if (!_prevParamName.Equals(cParam._keyName))
			//		{
			//			if (string.IsNullOrEmpty(_prevParamName))
			//			{
			//				EditorUtility.DisplayDialog("Error", "Empty Name is not allowed", "Okay");

			//				_prevParamName = cParam._keyName;
			//			}
			//			else
			//			{
			//				if (Editor.ParamControl.FindParam(_prevParamName) != null)
			//				{
			//					EditorUtility.DisplayDialog("Error", "It is used Name", "Okay");

			//					_prevParamName = cParam._keyName;
			//				}
			//				else
			//				{
			//					//cParam._keyName = _prevParamName;

			//					//수정
			//					//링크가 깨지지 않도록 전체적으로 검색하여 키 이름을 바꾸어주자
			//					Editor.Controller.ChangeParamName(cParam, _prevParamName);
			//					cParam._keyName = _prevParamName;
			//				}
			//			}


			//		}
			//		GUI.FocusControl("");
			//		//Editor.Hierarchy.RefreshUnits();
			//		Editor.RefreshControllerAndHierarchy();
			//	}
			//}
			//EditorGUILayout.EndHorizontal(); 

			#endregion
			EditorGUILayout.Space();

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ValueType));//"Type"
			if (cParam._isReserved)
			{
				EditorGUILayout.EnumPopup(cParam._valueType);
			}
			else
			{
				next_valueType = (apControlParam.TYPE)EditorGUILayout.EnumPopup(cParam._valueType);
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Category));//"Category"
			if (cParam._isReserved)
			{
				EditorGUILayout.EnumPopup(cParam._category);
			}
			else
			{
				next_category = (apControlParam.CATEGORY)EditorGUILayout.EnumPopup(cParam._category);
			}
			GUILayout.Space(10);

			int iconSize = 32;
			int iconPresetHeight = 32;
			int presetCategoryWidth = width - (iconSize + 8 + 5);
			Texture2D imgIcon = Editor.ImageSet.Get(apEditorUtil.GetControlParamPresetIconType(cParam._iconPreset));

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconPresetHeight));
			GUILayout.Space(2);

			EditorGUILayout.BeginVertical(GUILayout.Width(presetCategoryWidth), GUILayout.Height(iconPresetHeight));

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IconPreset), GUILayout.Width(presetCategoryWidth));//"Icon Preset"
			next_iconPreset = (apControlParam.ICON_PRESET)EditorGUILayout.EnumPopup(cParam._iconPreset, GUILayout.Width(presetCategoryWidth));

			EditorGUILayout.EndVertical();
			GUILayout.Space(2);
			EditorGUILayout.LabelField(new GUIContent(imgIcon), GUILayout.Width(iconSize), GUILayout.Height(iconPresetHeight));


			EditorGUILayout.EndHorizontal();


			EditorGUILayout.Space();


			string strRangeLabelName_Min = Editor.GetUIWord(UIWORD.Min);//"Min"
			string strRangeLabelName_Max = Editor.GetUIWord(UIWORD.Max);//"Max"
			switch (cParam._valueType)
			{
				case apControlParam.TYPE.Int:
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Param_IntegerType));//"Integer Type"
					EditorGUILayout.Space();

					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Param_DefaultValue));//"Default Value"
					next_int_Def = EditorGUILayout.DelayedIntField(cParam._int_Def);
					break;

				case apControlParam.TYPE.Float:
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Param_FloatType));//"Float Number Type"
					EditorGUILayout.Space();

					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Param_DefaultValue));//"Default Value"
					next_float_Def = EditorGUILayout.DelayedFloatField(cParam._float_Def);
					break;

				case apControlParam.TYPE.Vector2:
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Param_Vector2Type));//"Vector2 Type"
					EditorGUILayout.Space();

					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Param_DefaultValue));//"Default Value"

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					next_vec2_Def.x = EditorGUILayout.DelayedFloatField(cParam._vec2_Def.x, GUILayout.Width((width / 2) - 2));
					next_vec2_Def.y = EditorGUILayout.DelayedFloatField(cParam._vec2_Def.y, GUILayout.Width((width / 2) - 2));
					EditorGUILayout.EndHorizontal();

					strRangeLabelName_Min = Editor.GetUIWord(UIWORD.Param_Axis1);//"Axis 1"
					strRangeLabelName_Max = Editor.GetUIWord(UIWORD.Param_Axis2);//"Axis 2"
					break;
			}
			GUILayout.Space(25);

			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(25);


			GUILayoutOption opt_Label = GUILayout.Width(50);
			GUILayoutOption opt_Data = GUILayout.Width(width - (50 + 5));
			GUILayoutOption opt_SubData2 = GUILayout.Width((width - (50 + 5)) / 2 - 2);
			GUIStyle guiStyle_LabelRight = new GUIStyle(GUI.skin.label);
			guiStyle_LabelRight.alignment = TextAnchor.MiddleRight;

			GUILayout.Space(25);
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.RangeValueLabel));//"Range Value Label" -> Name of value Range

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(strRangeLabelName_Min, opt_Label);
			next_label_Min = EditorGUILayout.DelayedTextField(cParam._label_Min, opt_Data);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(strRangeLabelName_Max, opt_Label);
			next_label_Max = EditorGUILayout.DelayedTextField(cParam._label_Max, opt_Data);
			EditorGUILayout.EndHorizontal();


			GUILayout.Space(25);
			
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Range));//"Range Value" -> "Range"


			switch (cParam._valueType)
			{
				case apControlParam.TYPE.Int:
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Min), opt_Label);//"Min"
					next_int_Min = EditorGUILayout.DelayedIntField(cParam._int_Min, opt_Data);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Max), opt_Label);//"Max"
					next_int_Max = EditorGUILayout.DelayedIntField(cParam._int_Max, opt_Data);
					EditorGUILayout.EndHorizontal();
					break;

				case apControlParam.TYPE.Float:
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Min), opt_Label);//"Min"
					next_float_Min = EditorGUILayout.DelayedFloatField(cParam._float_Min, opt_Data);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Max), opt_Label);//"Max"
					next_float_Max = EditorGUILayout.DelayedFloatField(cParam._float_Max, opt_Data);
					EditorGUILayout.EndHorizontal();
					break;

				case apControlParam.TYPE.Vector2:
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField("", opt_Label);
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Min), opt_SubData2);//"Min"
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Max), guiStyle_LabelRight, opt_SubData2);//"Max"
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField("X", opt_Label);
					next_vec2_Min.x = EditorGUILayout.DelayedFloatField(cParam._vec2_Min.x, opt_SubData2);
					next_vec2_Max.x = EditorGUILayout.DelayedFloatField(cParam._vec2_Max.x, opt_SubData2);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField("Y", opt_Label);
					next_vec2_Min.y = EditorGUILayout.DelayedFloatField(cParam._vec2_Min.y, opt_SubData2);
					next_vec2_Max.y = EditorGUILayout.DelayedFloatField(cParam._vec2_Max.y, opt_SubData2);
					EditorGUILayout.EndHorizontal();
					break;

			}


			if (cParam._valueType == apControlParam.TYPE.Float ||
				cParam._valueType == apControlParam.TYPE.Vector2)
			{
				GUILayout.Space(10);



				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SnapSize));//"Snap Size"
				next_snapSize = EditorGUILayout.DelayedIntField(cParam._snapSize, GUILayout.Width(width));
				//if (next_snapSize != cParam._snapSize)
				//{
				//	cParam._snapSize = nextSnapSize;
				//	if (cParam._snapSize < 1)
				//	{
				//		cParam._snapSize = 1;
				//	}
				//	GUI.FocusControl(null);
				//}
			}



			if (next_category != cParam._category ||
				next_iconPreset != cParam._iconPreset ||
				next_valueType != cParam._valueType ||

				next_label_Min != cParam._label_Min ||
				next_label_Max != cParam._label_Max ||
				next_snapSize != cParam._snapSize ||

				next_int_Def != cParam._int_Def ||
				next_float_Def != cParam._float_Def ||
				next_vec2_Def.x != cParam._vec2_Def.x ||
				next_vec2_Def.y != cParam._vec2_Def.y ||

				next_int_Min != cParam._int_Min ||
				next_int_Max != cParam._int_Max ||

				next_float_Min != cParam._float_Min ||
				next_float_Max != cParam._float_Max ||

				next_vec2_Min.x != cParam._vec2_Min.x ||
				next_vec2_Min.y != cParam._vec2_Min.y ||
				next_vec2_Max.x != cParam._vec2_Max.x ||
				next_vec2_Max.y != cParam._vec2_Max.y
				)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.ControlParam_SettingChanged, Editor, Editor._portrait, null, false);

				if (next_snapSize < 1)
				{
					next_snapSize = 1;
				}

				if (cParam._iconPreset != next_iconPreset)
				{
					cParam._isIconChanged = true;
				}
				else if (cParam._category != next_category && !cParam._isIconChanged)
				{
					//아이콘을 한번도 바꾸지 않았더라면 자동으로 다음 아이콘을 추천해주자
					next_iconPreset = apEditorUtil.GetControlParamPresetIconTypeByCategory(next_category);
				}

				cParam._category = next_category;
				cParam._iconPreset = next_iconPreset;
				cParam._valueType = next_valueType;

				cParam._label_Min = next_label_Min;
				cParam._label_Max = next_label_Max;
				cParam._snapSize = next_snapSize;

				cParam._int_Def = next_int_Def;
				cParam._float_Def = next_float_Def;
				cParam._vec2_Def = next_vec2_Def;

				cParam._int_Min = next_int_Min;
				cParam._int_Max = next_int_Max;

				cParam._float_Min = next_float_Min;
				cParam._float_Max = next_float_Max;

				cParam._vec2_Min = next_vec2_Min;
				cParam._vec2_Max = next_vec2_Max;

				cParam.MakeInterpolationRange();
				GUI.FocusControl(null);
			}


			GUILayout.Space(30);

			//"Presets"
			if(GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.Presets), Editor.ImageSet.Get(apImageSet.PRESET.ControlParam_Palette)), GUILayout.Height(30)))
			{
				_loadKey_OnSelectControlParamPreset = apDialog_ControlParamPreset.ShowDialog(Editor, cParam, OnSelectControlParamPreset);
			}

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			if (!cParam._isReserved)
			{
				//"Remove Parameter"
				if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.RemoveParameter),
												Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
												),
								GUILayout.Height(24)))
				{
					string strRemoveParamText = Editor.Controller.GetRemoveItemMessage(_portrait,
														cParam,
														5,
														Editor.GetTextFormat(TEXT.RemoveControlParam_Body, cParam._keyName),
														Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
														);

					//bool isResult = EditorUtility.DisplayDialog("Warning", "If this param removed, some motion data may be not worked correctly", "Remove it!", "Cancel");
					bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveControlParam_Title),
																	//Editor.GetTextFormat(TEXT.RemoveControlParam_Body, cParam._keyName),
																	strRemoveParamText,
																	Editor.GetText(TEXT.Remove),
																	Editor.GetText(TEXT.Cancel));
					if (isResult)
					{
						Editor.Controller.RemoveParam(cParam);
					}
				}
			}
		}

		private object _loadKey_OnSelectControlParamPreset = null;
		private void OnSelectControlParamPreset(bool isSuccess, object loadKey, apControlParamPresetUnit controlParamPresetUnit, apControlParam controlParam)
		{
			if(!isSuccess 
				|| _loadKey_OnSelectControlParamPreset != loadKey
				|| controlParamPresetUnit == null
				|| controlParam != Param)
			{
				_loadKey_OnSelectControlParamPreset = null;
				return;
			}
			_loadKey_OnSelectControlParamPreset = null;

			//ControlParam에 프리셋 정보를 넣어주자
			Editor.Controller.SetControlParamPreset(controlParam, controlParamPresetUnit);
		}


		private void DrawTitle(string strTitle, int width)
		{
			int titleWidth = width;
			bool isShowHideBtn = false;
			if (_selectionType == SELECTION_TYPE.MeshGroup || _selectionType == SELECTION_TYPE.Animation)
			{
				titleWidth = width - (25 + 2);
				isShowHideBtn = true;
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(5);
			GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
			guiStyle.normal.textColor = Color.white;
			guiStyle.alignment = TextAnchor.MiddleCenter;

			Color prevColor = GUI.backgroundColor;
			//GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);
			GUI.backgroundColor = apEditorUtil.ToggleBoxColor_Selected;

			GUILayout.Box(strTitle, guiStyle, GUILayout.Width(titleWidth), GUILayout.Height(20));

			GUI.backgroundColor = prevColor;

			if (isShowHideBtn)
			{
				bool isOpened = (Editor._right_UpperLayout == apEditor.RIGHT_UPPER_LAYOUT.Show);
				Texture2D btnImage = null;
				if (isOpened)
				{ btnImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_OpenLayout); }
				else
				{ btnImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_HideLayout); }

				GUIStyle guiStyle_Btn = new GUIStyle(GUI.skin.label);
				if (GUILayout.Button(btnImage, guiStyle_Btn, GUILayout.Width(25), GUILayout.Height(25)))
				{
					if (Editor._right_UpperLayout == apEditor.RIGHT_UPPER_LAYOUT.Show)
					{
						Editor._right_UpperLayout = apEditor.RIGHT_UPPER_LAYOUT.Hide;
					}
					else
					{
						Editor._right_UpperLayout = apEditor.RIGHT_UPPER_LAYOUT.Show;
					}
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		//---------------------------------------------------------------------------


		//---------------------------------------------------------------------
		/// <summary>
		/// Mesh Property GUI에서 "조작 방법"에 대한 안내 UI를 보여준다.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="msgMouseLeft">마우스 좌클릭에 대한 설명 (없을 경우 null)</param>
		/// <param name="msgMouseMiddle">마우스 휠클릭에 대한 설명 (없을 경우 null)</param>
		/// <param name="msgMouseRight">마우스 우클릭에 대한 설명 (없을 경우 null)</param>
		/// <param name="msgKeyboardList">키보드 입력에 대한 설명. 여러개 가능</param>
		private void DrawHowToControl(int width, string msgMouseLeft, string msgMouseMiddle, string msgMouseRight, string msgKeyboardDelete = null, string msgKeyboardCtrl = null, string msgKeyboardShift = null)
		{
			bool isMouseLeft = !string.IsNullOrEmpty(msgMouseLeft);
			bool isMouseMiddle = !string.IsNullOrEmpty(msgMouseMiddle);
			bool isMouseRight = !string.IsNullOrEmpty(msgMouseRight);
			bool isKeyDelete = !string.IsNullOrEmpty(msgKeyboardDelete);
			bool isKeyCtrl = !string.IsNullOrEmpty(msgKeyboardCtrl);
			bool isKeyShift = !string.IsNullOrEmpty(msgKeyboardShift);
			//int nKeyMsg = 0;
			//if (msgKeyboardList != null)
			//{
			//	nKeyMsg = msgKeyboardList.Length;
			//}

			GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
			guiStyle_Icon.margin = GUI.skin.box.margin;

			GUIStyle guiStyle_Label = new GUIStyle(GUI.skin.label);
			guiStyle_Label.alignment = TextAnchor.LowerLeft;
			guiStyle_Label.normal.textColor = apEditorUtil.BoxTextColor;

			//GUILayout.Space(20);

			int labelSize = 30;
			int subTextWidth = width - (labelSize + 8);
			if (isMouseLeft)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_MouseLeft)), guiStyle_Icon, GUILayout.Width(labelSize), GUILayout.Height(labelSize));

				EditorGUILayout.BeginVertical(GUILayout.Width(subTextWidth), GUILayout.Height(labelSize));
				GUILayout.Space(8);
				EditorGUILayout.LabelField(msgMouseLeft, GUILayout.Width(subTextWidth), GUILayout.Height(20));
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}

			if (isMouseMiddle)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_MouseMiddle)), guiStyle_Icon, GUILayout.Width(labelSize), GUILayout.Height(labelSize));

				EditorGUILayout.BeginVertical(GUILayout.Width(subTextWidth), GUILayout.Height(labelSize));
				GUILayout.Space(8);
				EditorGUILayout.LabelField(msgMouseMiddle, GUILayout.Width(subTextWidth), GUILayout.Height(20));
				EditorGUILayout.EndVertical();


				EditorGUILayout.EndHorizontal();
			}

			if (isMouseRight)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_MouseRight)), guiStyle_Icon, GUILayout.Width(labelSize), GUILayout.Height(labelSize));

				EditorGUILayout.BeginVertical(GUILayout.Width(subTextWidth), GUILayout.Height(labelSize));
				GUILayout.Space(8);
				EditorGUILayout.LabelField(msgMouseRight, GUILayout.Width(subTextWidth), GUILayout.Height(20));
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}

			if (isKeyDelete)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_KeyDelete)), guiStyle_Icon, GUILayout.Width(labelSize), GUILayout.Height(labelSize));

				EditorGUILayout.BeginVertical(GUILayout.Width(subTextWidth), GUILayout.Height(labelSize));
				GUILayout.Space(8);
				EditorGUILayout.LabelField(msgKeyboardDelete, GUILayout.Width(subTextWidth), GUILayout.Height(20));
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}

			if (isKeyCtrl)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_KeyCtrl)), guiStyle_Icon, GUILayout.Width(labelSize), GUILayout.Height(labelSize));

				EditorGUILayout.BeginVertical(GUILayout.Width(subTextWidth), GUILayout.Height(labelSize));
				GUILayout.Space(8);
				EditorGUILayout.LabelField(msgKeyboardCtrl, GUILayout.Width(subTextWidth), GUILayout.Height(20));
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}

			if (isKeyShift)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_KeyShift)), guiStyle_Icon, GUILayout.Width(labelSize), GUILayout.Height(labelSize));

				EditorGUILayout.BeginVertical(GUILayout.Width(subTextWidth), GUILayout.Height(labelSize));
				GUILayout.Space(8);
				EditorGUILayout.LabelField(msgKeyboardShift, GUILayout.Width(subTextWidth), GUILayout.Height(20));
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}
			//if (nKeyMsg > 0)
			//{
			//	for (int i = 0; i < nKeyMsg; i++)
			//	{
			//		EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
			//		GUILayout.Space(5);
			//		EditorGUILayout.LabelField(msgKeyboardList[i], GUILayout.Width(width - 10));
			//		EditorGUILayout.EndHorizontal();
			//	}
			//}
			GUILayout.Space(20);
		}

		//private string _prevMesh_Name = "";

		private void MeshProperty_None(int width, int height)
		{
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Name));//"Name"

			string nextMeshName = EditorGUILayout.DelayedTextField(_mesh._name, GUILayout.Width(width));
			if (!string.Equals(nextMeshName, _mesh._name))
			{
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_SettingChanged, Editor, _mesh, null, false);
				_mesh._name = nextMeshName;
				Editor.RefreshControllerAndHierarchy();
			}

			

			EditorGUILayout.Space();

			//1. 어느 텍스쳐를 사용할 것인가
			//[수정]
			//다이얼로그를 보여주자

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Image));//"Image"
			//apTextureData textureData = _mesh._textureData;

			string strTextureName = "(No Image)";
			Texture2D curTextureImage = null;
			int selectedImageHeight = 20;
			if (_mesh.LinkedTextureData != null)
			{
				strTextureName = _mesh.LinkedTextureData._name;
				curTextureImage = _mesh.LinkedTextureData._image;

				if (curTextureImage != null && _mesh.LinkedTextureData._width > 0 && _mesh.LinkedTextureData._height > 0)
				{
					selectedImageHeight = (int)((float)(width * _mesh.LinkedTextureData._height) / (float)(_mesh.LinkedTextureData._width));
				}
			}

			if (curTextureImage != null)
			{
				//EditorGUILayout.TextField(strTextureName);
				EditorGUILayout.LabelField(strTextureName);
				GUILayout.Space(10);
				EditorGUILayout.LabelField(new GUIContent(curTextureImage), GUILayout.Height(selectedImageHeight));
				//EditorGUILayout.ObjectField(curTextureImage, typeof(Texture2D), false, GUILayout.Height(selectedImageHeight));
				GUILayout.Space(10);
			}
			else
			{
				EditorGUILayout.LabelField("(No Image)");
			}

			if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.ChangeImage), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image)), GUILayout.Height(30)))//"  Change Image"
			{
				//_isShowTextureDataList = !_isShowTextureDataList;
				_loadKey_SelectTextureDataToMesh = apDialog_SelectTextureData.ShowDialog(Editor, _mesh, OnSelectTextureDataToMesh);
			}

			EditorGUILayout.Space();
			#region [미사용 코드]
			//if(_isShowTextureDataList)
			//{
			//	int nImage = _portrait._textureData.Count;
			//	for (int i = 0; i < nImage; i++)
			//	{
			//		if (i % 2 == 0)
			//		{
			//			EditorGUILayout.BeginHorizontal();
			//		}

			//		EditorGUILayout.BeginVertical(GUILayout.Width((width / 2) - 4));

			//		apTextureData curTextureData = _portrait._textureData[i];
			//		if(curTextureData == null)
			//		{
			//			continue;
			//		}

			//		//EditorGUILayout.LabelField("[" + (i + 1) + "] : " + curTextureData._name);
			//		//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//		int imageHeight = 20;
			//		if(curTextureData._image != null && curTextureData._width > 0 && curTextureData._height > 0)
			//		{
			//			//w : h = w' : h'
			//			//(w ' * h) / w = h'
			//			imageHeight = (int)((float)((width / 2 - 4) * curTextureData._height) / (float)(curTextureData._width));
			//		}
			//		EditorGUILayout.ObjectField(curTextureData._image, typeof(Texture2D), false, GUILayout.Height(imageHeight));
			//		if(GUILayout.Button("Select", GUILayout.Height(25)))
			//		{
			//			apEditorUtil.SetRecord("Change Image of Mesh", _portrait);

			//			//bool isCheckToResetVertex = false;
			//			//if(_mesh._vertexData == null || _mesh._vertexData.Count == 0)
			//			//{
			//			//	isCheckToResetVertex = true;
			//			//}

			//			_mesh._textureData = curTextureData;
			//			_isShowTextureDataList = false;

			//			//if(isCheckToResetVertex)
			//			//{
			//			//	if (EditorUtility.DisplayDialog("Reset Vertex", "Do you want to make Vertices automatically?", "Reset", "Stay"))
			//			//	{
			//			//		_mesh._vertexData.Clear();
			//			//		_mesh._indexBuffer.Clear();

			//			//		_mesh.ResetVerticesByImageOutline();
			//			//	}
			//			//}
			//		}
			//		//EditorGUILayout.EndHorizontal();

			//		EditorGUILayout.EndVertical();


			//		if(i % 2 == 1)
			//		{
			//			EditorGUILayout.EndHorizontal();
			//			GUILayout.Space(10);
			//		}
			//	}
			//	if(nImage % 2 == 1)
			//	{
			//		EditorGUILayout.EndHorizontal();
			//		GUILayout.Space(10);
			//	}

			//} 
			#endregion

			//2. 버텍스 세팅
			if (GUILayout.Button(new GUIContent(Editor.GetUIWord(UIWORD.ResetVertices), "Remove all Vertices and Polygons")))//"Reset Vertices"
			{
				if (_mesh.LinkedTextureData != null && _mesh.LinkedTextureData._image != null)
				{
					bool isConfirmReset = false;
					if (_mesh._vertexData != null && _mesh._vertexData.Count > 0 &&
						_mesh._indexBuffer != null && _mesh._indexBuffer.Count > 0)
					{
						//isConfirmReset = EditorUtility.DisplayDialog("Reset Vertex", "If you reset vertices, All data is reset.", "Reset", "Cancel");
						isConfirmReset = EditorUtility.DisplayDialog(Editor.GetText(TEXT.ResetMeshVertices_Title),
																		Editor.GetText(TEXT.ResetMeshVertices_Body),
																		Editor.GetText(TEXT.ResetMeshVertices_Okay),
																		Editor.GetText(TEXT.Cancel));


					}
					else
					{
						isConfirmReset = true;
					}

					if (isConfirmReset)
					{
						apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_ResetVertices, Editor, _mesh, _mesh, false);

						_mesh._vertexData.Clear();
						_mesh._indexBuffer.Clear();
						_mesh._edges.Clear();
						_mesh._polygons.Clear();
						_mesh.MakeEdgesToPolygonAndIndexBuffer();

						_mesh.ResetVerticesByImageOutline();
						_mesh.MakeEdgesToPolygonAndIndexBuffer();

						Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
					}
				}
			}

			// Remove
			GUILayout.Space(30);

			if (GUILayout.Button(	new GUIContent(	"  " + Editor.GetUIWord(UIWORD.RemoveMesh),
													Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
													),
									GUILayout.Height(24)))//"  Remove Mesh"
			{
				string strRemoveDialogInfo = Editor.Controller.GetRemoveItemMessage(
																_portrait,
																_mesh,
																5,
																Editor.GetTextFormat(TEXT.RemoveMesh_Body, _mesh._name),
																Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																);

				//bool isResult = EditorUtility.DisplayDialog("Remove Mesh", "Do you want to remove [" + _mesh._name + "]?", "Remove", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveMesh_Title),
																//Editor.GetTextFormat(TEXT.RemoveMesh_Body, _mesh._name),
																strRemoveDialogInfo,
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel));

				if (isResult)
				{
					//apEditorUtil.SetRecord("Remove Mesh", _portrait);

					//MonoBehaviour.DestroyImmediate(_mesh.gameObject);
					//_portrait._meshes.Remove(_mesh);
					Editor.Controller.RemoveMesh(_mesh);

					SetNone();
				}
			}
		}

		private object _loadKey_SelectTextureDataToMesh = null;
		private void OnSelectTextureDataToMesh(bool isSuccess, apMesh targetMesh, object loadKey, apTextureData resultTextureData)
		{
			if (!isSuccess || resultTextureData == null || _mesh != targetMesh || _loadKey_SelectTextureDataToMesh != loadKey)
			{
				_loadKey_SelectTextureDataToMesh = null;
				return;
			}

			_loadKey_SelectTextureDataToMesh = null;

			//Undo
			apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_SetImage, Editor, targetMesh, resultTextureData, false);

			//이전 코드
			//_mesh._textureData = resultTextureData;

			//변경 코드 4.1
			_mesh.SetTextureData(resultTextureData);

			//_isShowTextureDataList = false;

		}



		private void MeshProperty_Modify(int width, int height)
		{
			GUILayout.Space(10);
			//EditorGUILayout.LabelField("Left Click : Select Vertex");
			DrawHowToControl(width, Editor.GetUIWord(UIWORD.SelectVertex), null, null);//"Select Vertex"

			EditorGUILayout.Space();

			bool isSingleVertex = Editor.VertController.Vertex != null && Editor.VertController.Vertices.Count == 1;
			bool isMultipleVertex = Editor.VertController.Vertices.Count > 1;
			
			Editor.SetGUIVisible("Mesh Property Modify UI Single", isSingleVertex);
			Editor.SetGUIVisible("Mesh Property Modify UI Multiple", isMultipleVertex);
			Editor.SetGUIVisible("Mesh Property Modify UI No Info", (Editor.VertController.Vertex == null));

			if (isSingleVertex)
			{
				if (Editor.IsDelayedGUIVisible("Mesh Property Modify UI Single"))
				{
					EditorGUILayout.LabelField("Index : " + Editor.VertController.Vertex._index);

					GUILayout.Space(5);

					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Position));//"Position"
					Vector2 prevPos2 = Editor.VertController.Vertex._pos;
					Vector2 nextPos2 = apEditorUtil.DelayedVector2Field(Editor.VertController.Vertex._pos, width);

					GUILayout.Space(5);

					EditorGUILayout.LabelField("UV");//<<이건 고유명사라..
					Vector2 prevUV = Editor.VertController.Vertex._uv;
					Vector2 nextUV = apEditorUtil.DelayedVector2Field(Editor.VertController.Vertex._uv, width);

					GUILayout.Space(5);

					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Z_Depth) + " (0~1)");//"Z-Depth (0~1)"
					float prevDepth = Editor.VertController.Vertex._zDepth;
					//float nextDepth = EditorGUILayout.DelayedFloatField(Editor.VertController.Vertex._zDepth, GUILayout.Width(width));
					float nextDepth = EditorGUILayout.Slider(Editor.VertController.Vertex._zDepth, 0.0f, 1.0f, GUILayout.Width(width));

					if(nextPos2.x != prevPos2.x ||
						nextPos2.y != prevPos2.y ||
						nextUV.x != prevUV.x ||
						nextUV.y != prevUV.y ||
						nextDepth != prevDepth)
					{
						//Vertex 정보가 바뀌었다.
						apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_EditVertex, Editor, Mesh, Editor.VertController.Vertex, false);

						Editor.VertController.Vertex._pos = nextPos2;
						Editor.VertController.Vertex._uv = nextUV;
						Editor.VertController.Vertex._zDepth = nextDepth;

						//Mesh.RefreshPolygonsToIndexBuffer();
						Editor.SetRepaint();

					}
				}
			}
			else if(isMultipleVertex)
			{
				if (Editor.IsDelayedGUIVisible("Mesh Property Modify UI Multiple"))
				{
					EditorGUILayout.LabelField(Editor.VertController.Vertices.Count + " Vertices Selected");

					GUILayout.Space(5);
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Z_Depth) + " (0~1)");//"Z-Depth (0~1)"

					float prevDepth_Avg = 0.0f;
					float prevDepth_Min = -1.0f;
					float prevDepth_Max = -1.0f;

					apVertex vert = null;
					for (int i = 0; i < Editor.VertController.Vertices.Count; i++)
					{
						vert = Editor.VertController.Vertices[i];

						prevDepth_Avg += vert._zDepth;
						if (prevDepth_Min < 0.0f || vert._zDepth < prevDepth_Min)
						{
							prevDepth_Min = vert._zDepth;
						}

						if (prevDepth_Max < 0.0f || vert._zDepth > prevDepth_Max)
						{
							prevDepth_Max = vert._zDepth;
						}
					}

					prevDepth_Avg /= Editor.VertController.Vertices.Count;

					EditorGUILayout.LabelField("Min : " + prevDepth_Min);
					EditorGUILayout.LabelField("Max : " + prevDepth_Max);
					EditorGUILayout.LabelField("Average : " + prevDepth_Avg);

					GUILayout.Space(5);

					int heightSetWeight = 25;
					int widthSetBtn = 90;
					int widthIncDecBtn = 30;
					int widthValue = width - (widthSetBtn + widthIncDecBtn * 2 + 2 * 5 + 5);

					bool isDepthChanged = false;
					float nextDepth = 0.0f;
					int calculateType = 0;


					EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightSetWeight));
					GUILayout.Space(5);

					EditorGUILayout.BeginVertical(GUILayout.Width(widthValue), GUILayout.Height(heightSetWeight - 2));
					GUILayout.Space(8);
					_meshEdit_zDepthWeight = EditorGUILayout.DelayedFloatField(_meshEdit_zDepthWeight);
					
					EditorGUILayout.EndVertical();

					//"Set Weight"
					if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.SetWeight), false, true, widthSetBtn, heightSetWeight, "Specify the Z value of the vertex. The larger the value, the more in front."))
					{
						isDepthChanged = true;
						nextDepth = _meshEdit_zDepthWeight;
						calculateType = 1;
						GUI.FocusControl(null);
					}

					if (apEditorUtil.ToggledButton("+", false, true, widthIncDecBtn, heightSetWeight))
					{
						////0.05 단위로 올라가거나 내려온다. (5%)
						isDepthChanged = true;
						nextDepth = 0.05f;
						calculateType = 2;

						GUI.FocusControl(null);
					}
					if (apEditorUtil.ToggledButton("-", false, true, widthIncDecBtn, heightSetWeight))
					{
						//0.05 단위로 올라가거나 내려온다. (5%)
						isDepthChanged = true;
						nextDepth = -0.05f;
						calculateType = 2;

						GUI.FocusControl(null);
					}
					EditorGUILayout.EndHorizontal();


					if(isDepthChanged)
					{
						apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_EditVertex, Editor, Mesh, Editor.VertController.Vertex, false);

						if(calculateType == 1)
						{
							//SET : 선택된 모든 Vertex의 값을 지정한다.
							for (int i = 0; i < Editor.VertController.Vertices.Count; i++)
							{
								vert = Editor.VertController.Vertices[i];
								vert._zDepth = Mathf.Clamp01(nextDepth);
							}
						}
						else if(calculateType == 2)
						{
							//ADD : 선택된 Vertex 각각의 값을 증감한다.
							for (int i = 0; i < Editor.VertController.Vertices.Count; i++)
							{
								vert = Editor.VertController.Vertices[i];
								vert._zDepth = Mathf.Clamp01(vert._zDepth + nextDepth);
							}
						}

						//Mesh.RefreshPolygonsToIndexBuffer();
						Editor.SetRepaint();
					}

				}


			}
			else
			{
				if (Editor.IsDelayedGUIVisible("Mesh Property Modify UI No Info"))
				{
					EditorGUILayout.LabelField("No vertex selected");
				}
			}

			GUILayout.Space(20);

			//"Z-Depth Rendering"
			if(apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.Z_DepthRendering), Editor.GetUIWord(UIWORD.Z_DepthRendering), Editor._meshEditZDepthView == apEditor.MESH_EDIT_RENDER_MODE.ZDepth, true, width, 30))
			{
				if(Editor._meshEditZDepthView == apEditor.MESH_EDIT_RENDER_MODE.Normal)
				{
					Editor._meshEditZDepthView = apEditor.MESH_EDIT_RENDER_MODE.ZDepth;
				}
				else
				{
					Editor._meshEditZDepthView = apEditor.MESH_EDIT_RENDER_MODE.Normal;
				}
			}
			GUILayout.Space(5);
			//"Make Polygons"
			if (GUILayout.Button(new GUIContent(Editor.GetUIWord(UIWORD.MakePolygons), Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MakePolygon), "Make Polygons and Refresh Mesh"), GUILayout.Width(width), GUILayout.Height(40)))
			{
				//Undo
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MakeEdges, Editor, Editor.Select.Mesh, Editor.Select.Mesh, false);

				//Editor.VertController.StopEdgeWire();

				Editor.Select.Mesh.MakeEdgesToPolygonAndIndexBuffer();
				Editor.Select.Mesh.RefreshPolygonsToIndexBuffer();
				Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
			}
		}

		private void MeshProperty_MakeMesh(int width, int height)
		{
			GUILayout.Space(10);

			switch (Editor._meshEditeMode_MakeMesh)
			{
				case apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge:
					//DrawHowToControl(width, "Add or Move Vertex with Edges", "Move View", "Remove Vertex or Edge", null, "Snap to Vertex", "L:Cut Edge / R:Delete Vertex");
					DrawHowToControl(width, 
									Editor.GetUIWord(UIWORD.AddOrMoveVertexWithEdges), 
									Editor.GetUIWord(UIWORD.MoveView), 
									Editor.GetUIWord(UIWORD.RemoveVertexorEdge), 
									null, 
									Editor.GetUIWord(UIWORD.SnapToVertex), 
									Editor.GetUIWord(UIWORD.LCutEdge_RDeleteVertex));
					break;

				case apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly:
					//DrawHowToControl(width, "Add or Move Vertex", "Move View", "Remove Vertex");
					DrawHowToControl(width, 
									Editor.GetUIWord(UIWORD.AddOrMoveVertex), 
									Editor.GetUIWord(UIWORD.MoveView), 
									Editor.GetUIWord(UIWORD.RemoveVertex));
					break;

				case apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly:
					//DrawHowToControl(width, "Link Vertices / Turn Edge", "Move View", "Remove Edge", null, "Snap to Vertex", "Cut Edge");
					DrawHowToControl(width, 
									Editor.GetUIWord(UIWORD.LinkVertices_TurnEdge),
									Editor.GetUIWord(UIWORD.MoveView),
									Editor.GetUIWord(UIWORD.RemoveEdge), 
									null, 
									Editor.GetUIWord(UIWORD.SnapToVertex),
									Editor.GetUIWord(UIWORD.CutEdge));
					break;

				case apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon:
					//DrawHowToControl(width, "Select Polygon", "Move View", null, "Remove Polygon");
					DrawHowToControl(width, 
									Editor.GetUIWord(UIWORD.SelectPolygon),
									Editor.GetUIWord(UIWORD.MoveView),
									null, 
									Editor.GetUIWord(UIWORD.RemovePolygon));
					break;
			}

			EditorGUILayout.Space();

			Texture2D icon_EditVertexWithEdge = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_VertexEdge);
			Texture2D icon_EditVertexOnly = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_VertexOnly);
			Texture2D icon_EditEdgeOnly = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_EdgeOnly);
			Texture2D icon_EditPolygon = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_Polygon);

			bool isSubEditMode_VE = (Editor._meshEditeMode_MakeMesh == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge);
			bool isSubEditMode_Vertex = (Editor._meshEditeMode_MakeMesh == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly);
			bool isSubEditMode_Edge = (Editor._meshEditeMode_MakeMesh == apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly);
			bool isSubEditMode_Polygon = (Editor._meshEditeMode_MakeMesh == apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon);

			//int btnWidth = (width / 3) - 4;
			int btnWidth = (width / 4) - 4;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(45));
			GUILayout.Space(5);
			bool nextEditMode_VE = apEditorUtil.ToggledButton(icon_EditVertexWithEdge, isSubEditMode_VE, true, btnWidth, 35, "Add Vertex / Link Edge");
			bool nextEditMode_Vertex = apEditorUtil.ToggledButton(icon_EditVertexOnly, isSubEditMode_Vertex, true, btnWidth, 35, "Add Vertex");
			bool nextEditMode_Edge = apEditorUtil.ToggledButton(icon_EditEdgeOnly, isSubEditMode_Edge, true, btnWidth, 35, "Link Edge");
			bool nextEditMode_Polygon = apEditorUtil.ToggledButton(icon_EditPolygon, isSubEditMode_Polygon, true, btnWidth, 35, "Select Polygon");

			EditorGUILayout.EndHorizontal();

			if (nextEditMode_VE && !isSubEditMode_VE)
			{
				Editor._meshEditeMode_MakeMesh = apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge;
				Editor.VertController.UnselectVertex();
			}

			if (nextEditMode_Vertex && !isSubEditMode_Vertex)
			{
				Editor._meshEditeMode_MakeMesh = apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly;
				Editor.VertController.UnselectVertex();
			}

			if (nextEditMode_Edge && !isSubEditMode_Edge)
			{
				Editor._meshEditeMode_MakeMesh = apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly;
				Editor.VertController.UnselectVertex();
			}

			if (nextEditMode_Polygon && !isSubEditMode_Polygon)
			{
				Editor._meshEditeMode_MakeMesh = apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon;
				Editor.VertController.UnselectVertex();
			}

			GUILayout.Space(5);

			Color makeMeshModeColor = Color.black;
			string strMakeMeshModeInfo = "";
			switch (Editor._meshEditeMode_MakeMesh)
			{
				case apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge:
					//strMakeMeshModeInfo = "Add Vertex / Link Edge";
					strMakeMeshModeInfo = Editor.GetUIWord(UIWORD.AddVertexLinkEdge);
					makeMeshModeColor = new Color(0.87f, 0.57f, 0.92f, 1.0f);
					break;

				case apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly:
					//strMakeMeshModeInfo = "Add Vertex";
					strMakeMeshModeInfo = Editor.GetUIWord(UIWORD.AddVertex);
					makeMeshModeColor = new Color(0.57f, 0.82f, 0.95f, 1.0f);
					break;

				case apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly:
					//strMakeMeshModeInfo = "Link Edge";
					strMakeMeshModeInfo = Editor.GetUIWord(UIWORD.LinkEdge);
					makeMeshModeColor = new Color(0.95f, 0.65f, 0.65f, 1.0f);
					break;

				case apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon:
					//strMakeMeshModeInfo = "Polygon";
					strMakeMeshModeInfo = Editor.GetUIWord(UIWORD.Polygon);
					makeMeshModeColor = new Color(0.65f, 0.95f, 0.65f, 1.0f);
					break;
			}
			//Polygon HotKey 이벤트 추가 -> 변경
			//이건 Layout이 안나타나면 처리가 안된다. 다른 곳으로 옮기자
			//if (Editor._meshEditeMode_MakeMesh == apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon)
			//{
			//	Editor.AddHotKeyEvent(Editor.Controller.RemoveSelectedMeshPolygon, "Remove Polygon", KeyCode.Delete, false, false, false, null);
			//}


			GUIStyle guiStyle_Info = new GUIStyle(GUI.skin.box);
			guiStyle_Info.alignment = TextAnchor.MiddleCenter;
			guiStyle_Info.normal.textColor = apEditorUtil.BoxTextColor;

			Color prevColor = GUI.backgroundColor;

			GUI.backgroundColor = makeMeshModeColor;
			GUILayout.Box(strMakeMeshModeInfo, guiStyle_Info, GUILayout.Width(width - 8), GUILayout.Height(34));

			GUI.backgroundColor = prevColor;

			GUILayout.Space(20);
			//"Auto Link Edge"
			if (GUILayout.Button(new GUIContent(Editor.GetUIWord(UIWORD.AutoLinkEdge), Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_AutoLink), "Automatically creates edges connecting vertices"), GUILayout.Height(30)))
			{
				//Undo
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MakeEdges, Editor, Editor.Select.Mesh, Editor.Select.Mesh, false);

				//Editor.VertController.StopEdgeWire();
				Editor.Select.Mesh.AutoLinkEdges();
			}
			GUILayout.Space(20);
			//"Make Polygons"
			if (GUILayout.Button(new GUIContent(Editor.GetUIWord(UIWORD.MakePolygons), Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MakePolygon), "Make Polygons and Refresh Mesh"), GUILayout.Height(40)))
			{
				//Undo
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MakeEdges, Editor, Editor.Select.Mesh, Editor.Select.Mesh, false);

				//Editor.VertController.StopEdgeWire();

				Editor.Select.Mesh.MakeEdgesToPolygonAndIndexBuffer();
				Editor.Select.Mesh.RefreshPolygonsToIndexBuffer();
				Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
			}

			GUILayout.Space(30);

			//"Remove All Vertices"
			if (GUILayout.Button(new GUIContent(Editor.GetUIWord(UIWORD.RemoveAllVertices), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform), "Remove all Vertices and Polygons"), GUILayout.Height(24)))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove All Vertices", "Do you want to remove All vertices? (Not Undo)", "Remove All", "Cancel");

				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveMeshVertices_Title),
																Editor.GetText(TEXT.RemoveMeshVertices_Body),
																Editor.GetText(TEXT.RemoveMeshVertices_Okay),
																Editor.GetText(TEXT.Cancel));

				if (isResult)
				{
					//Undo
					apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_RemoveAllVertices, Editor, _mesh, null, false);

					_mesh._vertexData.Clear();
					_mesh._indexBuffer.Clear();
					_mesh._edges.Clear();
					_mesh._polygons.Clear();

					_mesh.MakeEdgesToPolygonAndIndexBuffer();

					Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영

					Editor.VertController.UnselectVertex();
					Editor.VertController.UnselectNextVertex();
				}
			}

		}



		private void MeshProperty_Pivot(int width, int height)
		{
			GUILayout.Space(10);
			//EditorGUILayout.LabelField("Left Drag : Change Pivot To Origin");
			//DrawHowToControl(width, "Move Pivot", null, null, null);
			DrawHowToControl(width, Editor.GetUIWord(UIWORD.MovePivot), null, null, null);

			EditorGUILayout.Space();

			//"Reset Pivot"
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.ResetPivot), GUILayout.Height(40)))
			{
				//아예 함수로 만들것
				//이전 코드
				//>> OffsetPos만 바꾸는 코드
				//apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_SetPivot, Editor, _mesh, _mesh._offsetPos, false);

				//Editor.Select.Mesh._offsetPos = Vector2.zero;//<TODO : 이걸 사용하는 MeshGroup들의 DefaultPos를 역연산해야한다.

				//Editor.Select.Mesh.MakeOffsetPosMatrix();//<<OffsetPos를 수정하면 이걸 바꿔주자

				Editor.Controller.SetMeshPivot(Editor.Select.Mesh, Vector2.zero);
			}
		}




		//----------------------------------------------------------------------------
		//private string _prevMeshGroup_Name = "";

		private void MeshGroupProperty_Setting(int width, int height)
		{
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Name));//"Name"
			string nextMeshGroupName = EditorGUILayout.DelayedTextField(_meshGroup._name, GUILayout.Width(width));
			if (!string.Equals(nextMeshGroupName, _meshGroup._name))
			{
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, Editor, _meshGroup, null, false, false);
				_meshGroup._name = nextMeshGroupName;
				Editor.RefreshControllerAndHierarchy();
			}

			#region [미사용 코드] DelayedTextField 사용 전 코드
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//_prevMeshGroup_Name = EditorGUILayout.TextField(_prevMeshGroup_Name);
			//if (GUILayout.Button("Change", GUILayout.Width(80)))
			//{
			//	if (!string.IsNullOrEmpty(_prevMeshGroup_Name))
			//	{
			//		_meshGroup._name = _prevMeshGroup_Name;

			//		//Editor.Hierarchy.RefreshUnits();
			//		Editor.RefreshControllerAndHierarchy();
			//	}
			//}
			//EditorGUILayout.EndHorizontal(); 
			#endregion
			//EditorGUILayout.Space();

			GUILayout.Space(20);

			//" Editing.." / " Edit Default Transform"
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_MeshGroupDefaultTransform),
				" " + Editor.GetUIWord(UIWORD.EditingDefaultTransform),
				" " + Editor.GetUIWord(UIWORD.EditDefaultTransform),
				_isMeshGroupSetting_ChangePivot, true, width, 30, "Edit Default Transforms of Sub Meshs/MeshGroups"))
			{
				_isMeshGroupSetting_ChangePivot = !_isMeshGroupSetting_ChangePivot;
				if (_isMeshGroupSetting_ChangePivot)
				{
					//Modifier 모두 비활성화
					MeshGroup._modifierStack.SetExclusiveModifierInEditing(null, null, false);
				}
				else
				{
					//Modifier 모두 활성화
					MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
				}

				RefreshMeshGroupExEditingFlags(MeshGroup, null, null, null, false);//<<추가
			}

			GUILayout.Space(20);

			//프리셋 타입은 사용하지 않는다.
			#region [미사용 코드]
			//EditorGUILayout.LabelField("Preset Type");
			//apMeshGroup.PRESET_TYPE nextPreset = (apMeshGroup.PRESET_TYPE)EditorGUILayout.EnumPopup(_meshGroup._presetType);
			//if (nextPreset != _meshGroup._presetType)
			//{
			//	_meshGroup._presetType = nextPreset;
			//	//Refresh?
			//}
			//EditorGUILayout.Space(); 
			#endregion


			//MainMesh에 포함되는가
			bool isMainMeshGroup = _portrait._mainMeshGroupList.Contains(MeshGroup);
			if (isMainMeshGroup)
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.alignment = TextAnchor.MiddleCenter;
				guiStyle.normal.textColor = apEditorUtil.BoxTextColor;

				Color prevColor = GUI.backgroundColor;
				GUI.backgroundColor = new Color(0.5f, 0.7f, 0.9f, 1.0f);

				//"Root Unit"
				GUILayout.Box(Editor.GetUIWord(UIWORD.RootUnit), guiStyle, GUILayout.Width(width), GUILayout.Height(30));

				GUI.backgroundColor = prevColor;
			}
			else
			{
				//" Set Root Unit"
				if (GUILayout.Button(new GUIContent(" " + Editor.GetUIWord(UIWORD.SetRootUnit), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Root), "Make Mesh Group as Root Unit"), GUILayout.Width(width), GUILayout.Height(30)))
				{
					apEditorUtil.SetRecord_PortraitMeshGroup(apUndoGroupData.ACTION.Portrait_SetMeshGroup, Editor, _portrait, MeshGroup, null, false, true);

					_portrait._mainMeshGroupIDList.Add(MeshGroup._uniqueID);
					_portrait._mainMeshGroupList.Add(MeshGroup);

					apRootUnit newRootUnit = new apRootUnit();
					newRootUnit.SetPortrait(_portrait);
					newRootUnit.SetMeshGroup(MeshGroup);

					_portrait._rootUnits.Add(newRootUnit);

					//_portrait._mainMeshGroup = MeshGroup;
					//_portrait._mainMeshGroupID = MeshGroup._uniqueID;
					//_portrait._rootUnit._childMeshGroup = MeshGroup;
					//_portrait._rootUnit.SetMeshGroup(MeshGroup);
					Editor.RefreshControllerAndHierarchy();

					//Root Hierarchy Filter를 활성화한다.
					Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.RootUnit, true);
				}
			}




			GUILayout.Space(20);

			apEditorUtil.GUI_DelimeterBoxH(width - 10);

			//등등
			GUILayout.Space(30);
			//"  Remove Mesh Group"
			if (GUILayout.Button(	new GUIContent(	"  " + Editor.GetUIWord(UIWORD.RemoveMeshGroup),
													Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
													),
									GUILayout.Height(24)))
			{

				string strRemoveDialogInfo = Editor.Controller.GetRemoveItemMessage(
																_portrait,
																_meshGroup,
																5,
																Editor.GetTextFormat(TEXT.RemoveMeshGroup_Body, _meshGroup._name),
																Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																);

				//bool isResult = EditorUtility.DisplayDialog("Remove Mesh Group", "Do you want to remove [" + _meshGroup._name + "]?", "Remove", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveMeshGroup_Title),
																//Editor.GetTextFormat(TEXT.RemoveMeshGroup_Body, _meshGroup._name),
																strRemoveDialogInfo,
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);
				if (isResult)
				{
					Editor.Controller.RemoveMeshGroup(_meshGroup);

					SetNone();
				}
			}


		}



		private void MeshGroupProperty_Bone(int width, int height)
		{
			GUILayout.Space(10);

			Editor.SetGUIVisible("BoneEditMode - Editable", _isBoneDefaultEditing);

			//" Editing Bones", " Start Editing Bones"
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_EditMode),
												" " + Editor.GetUIWord(UIWORD.EditingBones), " " + Editor.GetUIWord(UIWORD.StartEditingBones),
												IsBoneDefaultEditing, true, width, 30, "Edit Bones"))
			{
				//Bone을 수정할 수 있다.
				SetBoneEditing(!_isBoneDefaultEditing, true);
			}

			GUILayout.Space(5);

			//Add 툴과 Select 툴 On/Off

			Editor.SetGUIVisible("BoneEditMode - Select", _boneEditMode == BONE_EDIT_MODE.SelectAndTRS);
			Editor.SetGUIVisible("BoneEditMode - Add", _boneEditMode == BONE_EDIT_MODE.Add);
			Editor.SetGUIVisible("BoneEditMode - Link", _boneEditMode == BONE_EDIT_MODE.Link);

			bool isBoneEditable = Editor.IsDelayedGUIVisible("BoneEditMode - Editable");
			bool isBoneEditMode_Select = Editor.IsDelayedGUIVisible("BoneEditMode - Select");
			bool isBoneEditMode_Add = Editor.IsDelayedGUIVisible("BoneEditMode - Add");
			bool isBoneEditMode_Link = Editor.IsDelayedGUIVisible("BoneEditMode - Link");

			int subTabWidth = (width / 3) - 4;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));



			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Select),
											isBoneEditMode_Select, _isBoneDefaultEditing,
											subTabWidth, 40, Editor.GetUIWord(UIWORD.SelectBones)))//"Select Bones"
			{
				SetBoneEditMode(BONE_EDIT_MODE.SelectAndTRS, true);
			}

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Add),
											isBoneEditMode_Add, _isBoneDefaultEditing,
											subTabWidth, 40, Editor.GetUIWord(UIWORD.AddBones)))//"Add Bones"
			{
				SetBoneEditMode(BONE_EDIT_MODE.Add, true);
			}

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Link),
											isBoneEditMode_Link, _isBoneDefaultEditing,
											subTabWidth, 40, Editor.GetUIWord(UIWORD.LinkBones)))//"Link Bones"
			{
				SetBoneEditMode(BONE_EDIT_MODE.Link, true);
			}

			EditorGUILayout.EndHorizontal();


			GUILayout.Space(5);

			if (isBoneEditable)
			{
				string strBoneEditInfo = "";
				Color prevColor = GUI.backgroundColor;
				Color colorBoneEdit = Color.black;
				switch (_boneEditMode)
				{
					case BONE_EDIT_MODE.None:
						strBoneEditInfo = "No Editable";
						colorBoneEdit = new Color(0.6f, 0.6f, 0.6f, 1.0f);
						break;

					case BONE_EDIT_MODE.SelectOnly:
						strBoneEditInfo = Editor.GetUIWord(UIWORD.SelectBones);//"Select Bones"
						colorBoneEdit = new Color(0.6f, 0.9f, 0.9f, 1.0f);
						break;

					case BONE_EDIT_MODE.SelectAndTRS:
						strBoneEditInfo = Editor.GetUIWord(UIWORD.SelectBones);//"Select Bones"
						colorBoneEdit = new Color(0.5f, 0.9f, 0.6f, 1.0f);
						break;

					case BONE_EDIT_MODE.Add:
						strBoneEditInfo = Editor.GetUIWord(UIWORD.AddBones);//"Add Bones"
						colorBoneEdit = new Color(0.95f, 0.65f, 0.65f, 1.0f);
						break;

					case BONE_EDIT_MODE.Link:
						strBoneEditInfo = Editor.GetUIWord(UIWORD.LinkBones);//"Link Bones"
						colorBoneEdit = new Color(0.57f, 0.82f, 0.95f, 1.0f);
						break;
				}

				GUIStyle guiStyle_Info = new GUIStyle(GUI.skin.box);
				guiStyle_Info.alignment = TextAnchor.MiddleCenter;
				guiStyle_Info.normal.textColor = apEditorUtil.BoxTextColor;

				GUI.backgroundColor = colorBoneEdit;
				GUILayout.Box(strBoneEditInfo, guiStyle_Info, GUILayout.Width(width - 8), GUILayout.Height(34));

				GUI.backgroundColor = prevColor;

				GUILayout.Space(5);
				switch (_boneEditMode)
				{
					case BONE_EDIT_MODE.None:
						DrawHowToControl(width, "None", "Move View", "None", null);
						break;

						//"Select Bones", "Move View", "Deselect"
					case BONE_EDIT_MODE.SelectOnly:
						DrawHowToControl(width, Editor.GetUIWord(UIWORD.SelectBones), Editor.GetUIWord(UIWORD.MoveView), Editor.GetUIWord(UIWORD.Deselect), null);//<<삭제 포함해야할 듯?
						break;

						//"Select Bones", "Move View", "Deselect"
					case BONE_EDIT_MODE.SelectAndTRS:
						DrawHowToControl(width, Editor.GetUIWord(UIWORD.SelectBones), Editor.GetUIWord(UIWORD.MoveView), Editor.GetUIWord(UIWORD.Deselect), null);
						break;

						//"Add Bones", "Move View", "Deselect"
					case BONE_EDIT_MODE.Add:
						DrawHowToControl(width, Editor.GetUIWord(UIWORD.AddBones), Editor.GetUIWord(UIWORD.MoveView), Editor.GetUIWord(UIWORD.Deselect), null);
						break;

						//"Select and Link Bones", "Move View", "Deselect"
					case BONE_EDIT_MODE.Link:
						DrawHowToControl(width, Editor.GetUIWord(UIWORD.SelectAndLinkBones), Editor.GetUIWord(UIWORD.MoveView), Editor.GetUIWord(UIWORD.Deselect), null);
						break;
				}

			}

			GUILayout.Space(10);
			//" Export/Import Bones"
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_SaveLoad),
												" " + Editor.GetUIWord(UIWORD.ExportImportBones),
												" " + Editor.GetUIWord(UIWORD.ExportImportBones),
												false, true, width, 26))
			{
				//Bone을 파일로 저장하거나 열수 있는 다이얼로그를 호출한다.
				_loadKey_OnBoneStructureLoaded = apDialog_RetargetBase.ShowDialog(Editor, _meshGroup, OnBoneStruceLoaded);
			}

			GUILayout.Space(20);

			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(20);

			//"Remove All Bones"
			if (GUILayout.Button(new GUIContent(" " + Editor.GetUIWord(UIWORD.RemoveAllBones), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)), GUILayout.Width(width), GUILayout.Height(24)))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Bones", "Remove All Bones?", "Remove", "Cancel");
				//이건 관련 메시지가 없다.
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveBonesAll_Title),
																Editor.GetText(TEXT.RemoveBonesAll_Body),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);
				if (isResult)
				{
					Editor.Controller.RemoveAllBones(MeshGroup);
				}
			}
		}


		private object _loadKey_OnBoneStructureLoaded = null;
		private void OnBoneStruceLoaded(bool isSuccess, object loadKey, apRetarget retargetData, apMeshGroup targetMeshGroup)
		{
			if(!isSuccess || _loadKey_OnBoneStructureLoaded != loadKey || _meshGroup != targetMeshGroup || targetMeshGroup == null)
			{
				_loadKey_OnBoneStructureLoaded = null;
				return;
			}
			_loadKey_OnBoneStructureLoaded = null;

			if(retargetData.IsBaseFileLoaded)
			{
				Editor.Controller.ImportBonesFromRetargetBaseFile(targetMeshGroup, retargetData);
			}



		}





		private object _loadKey_AddModifier = null;
		private void MeshGroupProperty_Modify(int width, int height)
		{
			//EditorGUILayout.LabelField("Presets");
			GUILayout.Space(10);

			//"  Add Modifier"
			if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.AddModifier), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_AddNewMod), "Add a New Modifier"), GUILayout.Height(30)))
			{
				_loadKey_AddModifier = apDialog_AddModifier.ShowDialog(Editor, MeshGroup, OnAddModifier);
			}

			#region [미사용 코드] 여기서 만든 리스트 대신 다이얼로그로 대체합니다.
			//Rect lastRect = GUILayoutUtility.GetLastRect();
			//Color prevColor = GUI.backgroundColor;

			//string[] strModifiers = new string[] { "Volume", "Morph", "Animated Morph", "Rigging", "Physic" };

			//GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
			//GUI.Box(new Rect(lastRect.x + 5, lastRect.y + 10, width - 10, strModifiers.Length * 22 + 3), "");
			//GUI.backgroundColor = prevColor;

			//GUIStyle _guiStyle_None = new GUIStyle(GUIStyle.none);
			//_guiStyle_None.normal.textColor = Color.black;
			//_guiStyle_None.onHover.textColor = Color.cyan;

			//int iAddModifier = -1;
			//for (int i = 0; i < strModifiers.Length; i++)
			//{
			//	apModifierBase.MODIFIER_TYPE modType = (apModifierBase.MODIFIER_TYPE)(i + 1);
			//	apImageSet.PRESET iconType = apEditorUtil.GetModifierIconType(modType);

			//	EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
			//	GUILayout.Space(15);
			//	EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(iconType)), GUILayout.Width(20), GUILayout.Height(20));
			//	GUILayout.Space(5);

			//	EditorGUILayout.LabelField(strModifiers[i], GUILayout.Width(width - (25 + 50 + 25)));
			//	if(GUILayout.Button("Add", GUILayout.Width(45)))
			//	{
			//		iAddModifier = i;
			//	}
			//	EditorGUILayout.EndHorizontal();
			//}

			//if(iAddModifier >= 0)
			//{
			//	apModifierBase.MODIFIER_TYPE modType = apModifierBase.MODIFIER_TYPE.Base;
			//	switch (iAddModifier)
			//	{
			//		case 0: modType = apModifierBase.MODIFIER_TYPE.Volume; break;
			//		case 1: modType = apModifierBase.MODIFIER_TYPE.Morph; break;
			//		case 2: modType = apModifierBase.MODIFIER_TYPE.AnimatedMorph; break;
			//		case 3: modType = apModifierBase.MODIFIER_TYPE.Rigging; break;
			//		case 4: modType = apModifierBase.MODIFIER_TYPE.Physic; break;
			//	}

			//	if(modType != apModifierBase.MODIFIER_TYPE.Base)
			//	{
			//		Editor.Controller.AddModifier(modType);
			//	}
			//}
			//GUILayout.Space(15); 
			#endregion

			GUILayout.Space(20);
			//EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(2);
			
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ModifierStack), GUILayout.Height(25));//"Modifier Stack"

			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			GUILayout.Button("", guiStyle_None, GUILayout.Width(20), GUILayout.Height(20));//<레이아웃 정렬을 위한의미없는 숨은 버튼
			EditorGUILayout.EndHorizontal();
			apModifierStack modStack = MeshGroup._modifierStack;


			//등록된 Modifier 리스트를 출력하자
			if (modStack._modifiers.Count > 0)
			{
				//int iLayerSortChange = -1;
				//bool isLayerUp = false;//<<Up : 레이어값을 올린다.

				//역순으로 출력한다.
				for (int i = modStack._modifiers.Count - 1; i >= 0; i--)
				{
					DrawModifierLayerUnit(modStack._modifiers[i], width, 25);

					#region [미사용 코드]
					////bool isLayerMoveUp = (i < modStack._modifiers.Count - 1);
					////bool isLayerMoveDown = i > 0;

					////int layerChangeResult = DrawModifierLayerUnit(modStack._modifiers[i], width, 25, isLayerMoveUp, isLayerMoveDown);
					//int layerChangeResult = DrawModifierLayerUnit(modStack._modifiers[i], width, 25);
					//if (layerChangeResult != 0)
					//{
					//	//iLayerSortChange = i;
					//	//if(layerChangeResult > 0)
					//	//{
					//	//	isLayerUp = true;
					//	//}
					//	//else
					//	//{
					//	//	isLayerUp = false;
					//	//}
					//} 
					#endregion
				}

				//레이어 바꾸는 기능은 다른 곳에서..
				//if(iLayerSortChange >= 0)
				//{
				//	Editor.Controller.LayerChange(modStack._modifiers[iLayerSortChange], isLayerUp);
				//}
			}


		}

		private void OnAddModifier(bool isSuccess, object loadKey, apModifierBase.MODIFIER_TYPE modifierType, apMeshGroup targetMeshGroup, int validationKey)
		{
			if (!isSuccess || _loadKey_AddModifier != loadKey || MeshGroup != targetMeshGroup)
			{
				_loadKey_AddModifier = null;
				return;
			}

			if (modifierType != apModifierBase.MODIFIER_TYPE.Base)
			{	
				Editor.Controller.AddModifier(modifierType, validationKey);
			}
			_loadKey_AddModifier = null;
		}

		//private int DrawModifierLayerUnit(apModifierBase modifier, int width, int height, bool isLayerUp, bool isLayerDown)
		private int DrawModifierLayerUnit(apModifierBase modifier, int width, int height)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();

			Color texColor = GUI.skin.label.normal.textColor;

			if (Modifier == modifier)
			{
				Color prevColor = GUI.backgroundColor;

				if(EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					texColor = Color.cyan;
				}
				else
				{
					GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					texColor = Color.white;
				}

				GUI.Box(new Rect(lastRect.x, lastRect.y + height, width + 15, height), "");
				GUI.backgroundColor = prevColor;
			}

			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			guiStyle_None.normal.textColor = texColor;

			apImageSet.PRESET iconType = apEditorUtil.GetModifierIconType(modifier.ModifierType);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));
			GUILayout.Space(10);
			if (GUILayout.Button(new GUIContent(" " + modifier.DisplayName, Editor.ImageSet.Get(iconType)), guiStyle_None, GUILayout.Width(width - 40), GUILayout.Height(height)))
			{
				SetModifier(modifier);
			}

			int iResult = 0;

			Texture2D activeBtn = null;
			bool isActiveMod = false;
			if (modifier._isActive && modifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled)
			{
				activeBtn = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Active);
				isActiveMod = true;
			}
			else
			{
				activeBtn = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Deactive);
				isActiveMod = false;
			}
			if (GUILayout.Button(activeBtn, guiStyle_None, GUILayout.Width(height), GUILayout.Height(height)))
			{
				//일단 토글한다.
				modifier._isActive = !isActiveMod;

				if (ExEditingMode != EX_EDIT.None)
				{
					if (modifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled)
					{
						//작업이 허용된 Modifier가 아닌데 Active를 제어했다면
						//ExEdit를 해제해야한다.
						SetModifierExclusiveEditing(EX_EDIT.None);
					}
				}


				//if (!ExEditingMode)
				//{
				//	//Debug.LogError("TODO : Active를 바꾸면, 녹화 기능이 무조건 비활성화되어야 한다.");
				//	SetModifierExclusiveEditing(false);
				//}
			}
			EditorGUILayout.EndHorizontal();

			return iResult;
		}

		//------------------------------------------------------------------------------------
		public void DrawEditor_Right2(int width, int height)
		{
			if (Editor == null || Editor.Select.Portrait == null)
			{
				return;
			}

			EditorGUILayout.Space();

			switch (_selectionType)
			{
				case SELECTION_TYPE.MeshGroup:
					{
						switch (Editor._meshGroupEditMode)
						{
							case apEditor.MESHGROUP_EDIT_MODE.Setting:
								DrawEditor_Right2_MeshGroupRight_Setting(width, height);
								break;

							case apEditor.MESHGROUP_EDIT_MODE.Bone:
								DrawEditor_Right2_MeshGroup_Bone(width, height);
								break;

							case apEditor.MESHGROUP_EDIT_MODE.Modifier:
								DrawEditor_Right2_MeshGroup_Modifier(width, height);
								break;
						}
					}
					break;

				case SELECTION_TYPE.Animation:
					{
						DrawEditor_Right2_Animation(width, height);
					}
					break;
			}
		}


		//--------------------------------------------------------------------------------------
		public void DrawEditor_Bottom2Edit(int width, int height)
		{
			if (Editor == null || Editor.Select.Portrait == null || Modifier == null)
			{
				return;
			}

			GUIStyle btnGUIStyle = new GUIStyle(GUI.skin.button);
			btnGUIStyle.alignment = TextAnchor.MiddleLeft;

			bool isRiggingModifier = (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging);
			bool isWeightedVertModifier = (int)(Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics) != 0
										|| (int)(Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Volume) != 0;
			//기본 Modifier가 있고
			//Rigging용 Modifier UI가 따로 있다.
			//추가 : Weight값을 사용하는 Physic/Volume도 따로 설정
			int editToggleWidth = 160;//140 > 180
			if (isRiggingModifier)
			{
				//리깅 타입인 경우
				//리깅 편집 툴 / 보기 버튼들이 나온다.
				//1. Rigging On/Off
				//+ 선택된 Mesh Transform
				//2. View 모드
				//3. Test Posing On/Off
				//"  Binding..", "  Start Binding"
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_EditBinding),
														"  " + Editor.GetUIWord(UIWORD.ModBinding), 
														"  " + Editor.GetUIWord(UIWORD.ModStartBinding), 
														_rigEdit_isBindingEdit, true, editToggleWidth, height, 
														"Enable/Disable Bind Mode (A)", btnGUIStyle))
				{
					_rigEdit_isBindingEdit = !_rigEdit_isBindingEdit;
					_rigEdit_isTestPosing = false;

					//작업중인 Modifier 외에는 일부 제외를 하자
					if (_rigEdit_isBindingEdit)
					{
						MeshGroup._modifierStack.SetExclusiveModifierInEditing(_modifier, SubEditedParamSetGroup, false);
						RefreshMeshGroupExEditingFlags(MeshGroup, _modifier, null, null, false);//<<추가
						_isLockExEditKey = true;
					}
					else
					{
						if (MeshGroup != null)
						{
							//Exclusive 모두 해제
							MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
							Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
							RefreshMeshGroupExEditingFlags(MeshGroup, null, null, null, false);//<<추가
						}
						_isLockExEditKey = false;
					}

					Editor.RefreshControllerAndHierarchy();
				}
				GUILayout.Space(10);

			}
			else
			{
				//그외의 Modifier
				//편집 On/Off와 현재 선택된 Key/Value가 나온다.
				//"  Editing..", "  Start Editing", "  Not Editiable"
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Recording),
													Editor.ImageSet.Get(apImageSet.PRESET.Edit_Record),
													Editor.ImageSet.Get(apImageSet.PRESET.Edit_NoRecord),
													"  " + Editor.GetUIWord(UIWORD.ModEditing), 
													"  " + Editor.GetUIWord(UIWORD.ModStartEditing), 
													"  " + Editor.GetUIWord(UIWORD.ModNotEditable),
													_exclusiveEditing != EX_EDIT.None, IsExEditable, editToggleWidth, height, 
													"Enable/Disable Edit Mode (A)", btnGUIStyle))
				{
					EX_EDIT nextResult = EX_EDIT.None;
					if (_exclusiveEditing == EX_EDIT.None && IsExEditable)
					{
						//None -> ExOnly로 바꾼다.
						//General은 특별한 경우
						nextResult = EX_EDIT.ExOnly_Edit;
					}
					//if (IsExEditable || !isNextResult)
					//{
					//	//SetModifierExclusiveEditing(isNextResult);
					//}
					SetModifierExclusiveEditing(nextResult);
					if (nextResult == EX_EDIT.ExOnly_Edit)
					{
						_isLockExEditKey = true;//처음 Editing 작업시 Lock을 거는 것으로 변경
					}
					else
					{
						_isLockExEditKey = false;//Editing 해제시 Lock 해제
					}
				}


				

				GUILayout.Space(10);
				//Lock 걸린 키 / 수정중인 객체 / 그 값을 각각 표시하자

			}
			


			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_SelectionLock),
												Editor.ImageSet.Get(apImageSet.PRESET.Edit_SelectionUnlock),
												IsLockExEditKey, true, height, height,
												"Selection Lock/Unlock (S)"
												))
			{
				SetModifierExclusiveEditKeyLock(!IsLockExEditKey);
			}

			

			GUILayout.Space(10);

#if UNITY_EDITOR_OSX
			string strCtrlKey = "Command";
#else
			string strCtrlKey = "Ctrl";
#endif
			
				

			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_ModLock),
												Editor.ImageSet.Get(apImageSet.PRESET.Edit_ModUnlock),
												_exclusiveEditing == EX_EDIT.ExOnly_Edit,
												IsExEditable && _exclusiveEditing != EX_EDIT.None,
												height, height,
												"Modifier Lock/Unlock (D) / If you press the button while holding down [" + strCtrlKey + "], the Setting dialog opens"))
			{
				//여기서 ExOnly <-> General 사이를 바꾼다.

				//변경 3.22 : Ctrl 키를 누르고 클릭하면 설정 Dialog가 뜬다.
#if UNITY_EDITOR_OSX
				bool isCtrl = Event.current.command;
#else
				bool isCtrl = Event.current.control;
#endif
				if (isCtrl)
				{
					apDialog_ModifierLockSetting.ShowDialog(Editor, _portrait);
				}
				else
				{
					if (IsExEditable && _exclusiveEditing != EX_EDIT.None)
					{
						EX_EDIT nextEditMode = EX_EDIT.ExOnly_Edit;
						if (_exclusiveEditing == EX_EDIT.ExOnly_Edit)
						{
							nextEditMode = EX_EDIT.General_Edit;
						}
						SetModifierExclusiveEditing(nextEditMode);
					}
				}
			}


			//토글 단축키를 입력하자
			//[A : Editor Toggle]
			//[S (Space에서 S로 변경) : Selection Lock]
			//[D : Modifier Lock)
			Editor.AddHotKeyEvent(OnHotKeyEvent_ToggleModifierEditing, "Toggle Editing Mode", KeyCode.A, false, false, false, null);
			Editor.AddHotKeyEvent(OnHotKeyEvent_ToggleExclusiveEditKeyLock, "Toggle Selection Lock", KeyCode.S, false, false, false, null);
			Editor.AddHotKeyEvent(OnHotKeyEvent_ToggleExclusiveModifierLock, "Toggle Modifier Lock", KeyCode.D, false, false, false, null);
			


			GUILayout.Space(10);

			apImageSet.PRESET modImagePreset = apEditorUtil.GetModifierIconType(Modifier.ModifierType);

			GUIStyle guiStyle_Key = new GUIStyle(GUI.skin.label);
			if (IsLockExEditKey)
			{
				guiStyle_Key.normal.textColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
			}

			GUIStyle guiStyle_NotSelected = new GUIStyle(GUI.skin.label);
			guiStyle_NotSelected.normal.textColor = new Color(0.0f, 0.5f, 1.0f, 1.0f);

			int paramSetWidth = 140;//100 -> 140
			int modValueWidth = 200;//170 -> 200

			switch (_exEditKeyValue)
			{
				case EX_EDIT_KEY_VALUE.None:
					break;

				case EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert:
				case EX_EDIT_KEY_VALUE.ParamKey_ModMesh://ModVert와 ModMesh는 비슷하다
					{
						//Key
						EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(modImagePreset)), GUILayout.Width(height), GUILayout.Height(height));


						Texture2D selectedImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);

						string strKey_ParamSetGroup = Editor.GetUIWord(UIWORD.ModNoParam);//"<No Parameter>"
						string strKey_ParamSet = Editor.GetUIWord(UIWORD.ModNoKey);//"<No Key>"
						string strKey_ModMesh = Editor.GetUIWord(UIWORD.ModNoSelected);//"<Not Selected>"
						string strKey_ModMeshLabel = Editor.GetUIWord(UIWORD.ModSubObject);//"Sub Object"

						GUIStyle guiStyle_ParamSetGroup = guiStyle_NotSelected;
						GUIStyle guiStyle_ParamSet = guiStyle_NotSelected;
						GUIStyle guiStyle_Transform = guiStyle_NotSelected;

						if (ExKey_ModParamSetGroup != null)
						{
							if (ExKey_ModParamSetGroup._keyControlParam != null)
							{
								strKey_ParamSetGroup = ExKey_ModParamSetGroup._keyControlParam._keyName;
								guiStyle_ParamSetGroup = guiStyle_Key;
							}
						}

						if (ExKey_ModParamSet != null)
						{
							//TODO : 컨트롤 타입이 아니면 다른 이름을 쓰자
							strKey_ParamSet = ExKey_ModParamSet.ControlParamValue;
							guiStyle_ParamSet = guiStyle_Key;
						}

						apModifiedMesh modMesh = null;
						if (_exEditKeyValue == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert)
						{
							modMesh = ExKey_ModMesh;
						}
						else
						{
							modMesh = ExValue_ModMesh;
						}

						if (modMesh != null)
						{
							if (modMesh._transform_Mesh != null)
							{
								strKey_ModMeshLabel = Editor.GetUIWord(UIWORD.Mesh);//>그냥 Mesh로 표현
								strKey_ModMesh = modMesh._transform_Mesh._nickName;
								selectedImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
								guiStyle_Transform = guiStyle_Key;
							}
							else if (modMesh._transform_MeshGroup != null)
							{
								strKey_ModMeshLabel = Editor.GetUIWord(UIWORD.MeshGroup);//>그냥 MeshGroup으로 표현
								strKey_ModMesh = modMesh._transform_MeshGroup._nickName;
								selectedImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);
								guiStyle_Transform = guiStyle_Key;
							}
						}
						else
						{
							if (ExKey_ModParamSet == null)
							{
								//Key를 먼저 선택할 것을 알려야한다.
								strKey_ModMesh = Editor.GetUIWord(UIWORD.ModSelectKeyFirst);//"<Select Key First>"
							}
						}

						if (Modifier.SyncTarget != apModifierParamSetGroup.SYNC_TARGET.Static)
						{
							EditorGUILayout.BeginVertical(GUILayout.Width(paramSetWidth), GUILayout.Height(height));
							EditorGUILayout.LabelField(strKey_ParamSetGroup, guiStyle_ParamSetGroup, GUILayout.Width(paramSetWidth));
							EditorGUILayout.LabelField(strKey_ParamSet, guiStyle_ParamSet, GUILayout.Width(paramSetWidth));
							EditorGUILayout.EndVertical();
						}
						else
						{
							EditorGUILayout.BeginVertical(GUILayout.Width(paramSetWidth), GUILayout.Height(height));
							EditorGUILayout.LabelField(Modifier.DisplayName, guiStyle_Key, GUILayout.Width(paramSetWidth));
							//EditorGUILayout.LabelField(strKey_ParamSet, guiStyle_ParamSet, GUILayout.Width(100));
							EditorGUILayout.EndVertical();
						}

						EditorGUILayout.LabelField(new GUIContent(selectedImage), GUILayout.Width(height), GUILayout.Height(height));
						EditorGUILayout.BeginVertical(GUILayout.Width(modValueWidth), GUILayout.Height(height));
						EditorGUILayout.LabelField(strKey_ModMeshLabel, GUILayout.Width(modValueWidth));
						EditorGUILayout.LabelField(strKey_ModMesh, guiStyle_Transform, GUILayout.Width(modValueWidth));
						EditorGUILayout.EndVertical();


						GUILayout.Space(10);
						apEditorUtil.GUI_DelimeterBoxV(height - 6);
						GUILayout.Space(10);

						//Value
						//(선택한 Vert의 값을 출력하자. 단, Rigging Modifier가 아닐때)
						if (_exEditKeyValue == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert && !isRiggingModifier && !isWeightedVertModifier)
						{

							bool isModVertSelected = (ExValue_ModVert != null);
							Editor.SetGUIVisible("Bottom2 Transform Mod Vert", isModVertSelected);

							if (Editor.IsDelayedGUIVisible("Bottom2 Transform Mod Vert"))
							{
								EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Vertex)), GUILayout.Width(height), GUILayout.Height(height));
								EditorGUILayout.BeginVertical(GUILayout.Width(150), GUILayout.Height(height));

								//"Vertex : " + ExValue_ModVert._modVert._vertexUniqueID
								EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Vertex) + " : " + ExValue_ModVert._modVert._vertexUniqueID, GUILayout.Width(150));

								//Vector2 newDeltaPos = EditorGUILayout.Vector2Field("", ExValue_ModVert._modVert._deltaPos, GUILayout.Width(150));
								Vector2 newDeltaPos = apEditorUtil.DelayedVector2Field(ExValue_ModVert._modVert._deltaPos, 150);
								if (ExEditingMode != EX_EDIT.None)
								{
									ExValue_ModVert._modVert._deltaPos = newDeltaPos;
								}
								EditorGUILayout.EndVertical();
							}
						}


					}
					break;

				case EX_EDIT_KEY_VALUE.ParamKey_Bone:
					{
						EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(modImagePreset)), GUILayout.Width(height), GUILayout.Height(height));
					}
					break;

			}

			if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
			{
				//리깅 타입이면 몇가지 제어 버튼이 추가된다.
				//2. View 모드
				//3. Test Posing On/Off
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_WeightColorWithTexture), _rigEdit_viewMode == RIGGING_EDIT_VIEW_MODE.WeightWithTexture, true, height + 5, height, "Render Rigging Weights with Images"))
				{
					_rigEdit_viewMode = RIGGING_EDIT_VIEW_MODE.WeightWithTexture;
				}

				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_WeightColorOnly), _rigEdit_viewMode == RIGGING_EDIT_VIEW_MODE.WeightColorOnly, true, height + 5, height, "Render Rigging Weights Only"))
				{
					_rigEdit_viewMode = RIGGING_EDIT_VIEW_MODE.WeightColorOnly;
				}

				GUILayout.Space(2);

				//"Bone Color", "Bone Color"
				if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.RigBoneColor), Editor.GetUIWord(UIWORD.RigBoneColor), _rigEdit_isBoneColorView, true, 120, height))
				{
					_rigEdit_isBoneColorView = !_rigEdit_isBoneColorView;
					Editor.SaveEditorPref();//<<이것도 Save 요건
				}

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxV(height - 6);
				GUILayout.Space(10);

				//"  Pose Test", "  Pose Test"
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_TestPosing),
													"  " + Editor.GetUIWord(UIWORD.RigPoseTest), "  " + Editor.GetUIWord(UIWORD.RigPoseTest), 
													_rigEdit_isTestPosing, _rigEdit_isBindingEdit, 130, height,
													"Enable/Disable Pose Test Mode"))
				{
					_rigEdit_isTestPosing = !_rigEdit_isTestPosing;
					SetBoneRiggingTest();

				}

				if (GUILayout.Button(Editor.GetUIWord(UIWORD.RigResetPose), GUILayout.Width(120), GUILayout.Height(height)))//"Reset Pose"
				{
					ResetRiggingTestPose();
				}
			}
			else if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic)
			{
				//테스트로 시뮬레이션을 할 수 있다.
				//바람을 켜고 끌 수 있다.
				EditorGUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(height));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.PxDirection), GUILayout.Width(100));//"Direction"
				_physics_windSimulationDir = apEditorUtil.DelayedVector2Field(_physics_windSimulationDir, 100 - 4);
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(height));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.PxPower), GUILayout.Width(100));//"Power"
				_physics_windSimulationScale = EditorGUILayout.DelayedFloatField(_physics_windSimulationScale, GUILayout.Width(100));
				EditorGUILayout.EndVertical();

				//"Wind On"
				if (GUILayout.Button(new GUIContent(Editor.GetUIWord(UIWORD.PxWindOn), "Simulate wind forces"), GUILayout.Width(110), GUILayout.Height(height)))
				{
					GUI.FocusControl(null);

					if (_portrait != null)
					{
						_portrait.ClearForce();
						_portrait.AddForce_Direction(_physics_windSimulationDir,
							0.3f,
							0.3f,
							3, 5)
							.SetPower(_physics_windSimulationScale, _physics_windSimulationScale * 0.3f, 4.0f)
							.EmitLoop();
					}
				}
				//"Wind Off"
				if (GUILayout.Button(new GUIContent(Editor.GetUIWord(UIWORD.PxWindOff), "Clear wind forces"), GUILayout.Width(110), GUILayout.Height(height)))
				{
					GUI.FocusControl(null);
					if (_portrait != null)
					{
						_portrait.ClearForce();
					}
				}


			}

			return;
		}
		//------------------------------------------------------------------------------------
		private Vector2 _scroll_Timeline = new Vector2();

		public void DrawEditor_Bottom(int width, int height, int layoutX, int layoutY, int windowWidth, int windowHeight)
		{
			if (Editor == null || Editor.Select.Portrait == null)
			{
				return;
			}

			switch (_selectionType)
			{
				case SELECTION_TYPE.Animation:
					{
						DrawEditor_Bottom_Animation(width, height, layoutX, layoutY, windowWidth, windowHeight);
					}
					break;


				case SELECTION_TYPE.MeshGroup:
					{

					}
					break;
			}

			return;
		}


		private bool _isTimelineWheelDrag = false;
		private Vector2 _prevTimelineWheelDragPos = Vector2.zero;
		private Vector2 _scrollPos_BottomAnimationRightProperty = Vector2.zero;

		private void DrawEditor_Bottom_Animation(int width, int height, int layoutX, int layoutY, int windowWidth, int windowHeight)
		{
			//좌우 두개의 탭으로 나뉜다. [타임라인 - 선택된 객체 정보]
			int rightTabWidth = 300;
			int margin = 5;
			int mainTabWidth = width - (rightTabWidth + margin);
			Rect lastRect = GUILayoutUtility.GetLastRect();

			List<apTimelineLayerInfo> timelineInfoList = Editor.TimelineInfoList;
			apTimelineLayerInfo nextSelectLayerInfo = null;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));
			EditorGUILayout.BeginVertical(GUILayout.Width(mainTabWidth), GUILayout.Height(height));
			//1. [좌측] 타임라인 레이아웃

			//1-1 요약부 : [레코드] + [타임과 통합 키프레임]
			//1-2 메인 타임라인 : [레이어] + [타임라인 메인]
			//1-3 하단 컨트롤과 스크롤 : [컨트롤러] + [스크롤 + 애니메이션 설정]
			int leftTabWidth = 280;
			int timelineWidth = mainTabWidth - (leftTabWidth + 4);

			if (Event.current.type == EventType.Repaint)
			{
				_timlineGUIWidth = timelineWidth;
			}

			//int recordAndSummaryHeight = 45;
			int recordAndSummaryHeight = 70;
			int bottomControlHeight = 54;
			int timelineHeight = height - (recordAndSummaryHeight + bottomControlHeight + 4);
			int guiHeight = height - bottomControlHeight;

			//자동 스크롤 이벤트 요청이 들어왔다.
			//처리를 해주자
			if (_isAnimTimelineLayerGUIScrollRequest)
			{
				//_scroll_Timeline.y
				//일단 어느 TimelineInfo인지 찾고,
				//그 값으로 이동
				apTimelineLayerInfo targetInfo = null;
				if (_subAnimTimelineLayer != null)
				{
					targetInfo = timelineInfoList.Find(delegate (apTimelineLayerInfo a)
					{
						return a._layer == _subAnimTimelineLayer && a.IsVisibleLayer;
					});
				}
				else if (_subAnimTimeline != null)
				{
					targetInfo = timelineInfoList.Find(delegate (apTimelineLayerInfo a)
					{
						return a._timeline == _subAnimTimeline && a._isTimeline;
					});
				}

				if (targetInfo != null)
				{
					if (targetInfo._guiLayerPosY - _scroll_Timeline.y < 0 ||
						targetInfo._guiLayerPosY - _scroll_Timeline.y > timelineHeight)
					{
						_scroll_Timeline.y = targetInfo._guiLayerPosY;
					}
				}

				_isAnimTimelineLayerGUIScrollRequest = false;

				
			}




			//if(Editor._timelineLayoutSize == apEditor.TIMELINE_LAYOUTSIZE.Size1)
			//{
			//	guiHeight = viewAndSummaryHeight;
			//}

			bool isDrawMainTimeline = (Editor._timelineLayoutSize != apEditor.TIMELINE_LAYOUTSIZE.Size1);

			//스크롤 값을 넣어주자
			int startFrame = AnimClip.StartFrame;
			int endFrame = AnimClip.EndFrame;
			int widthPerFrame = Editor.WidthPerFrameInTimeline;
			int nFrames = Mathf.Max((endFrame - startFrame) + 1, 1);
			int widthForTotalFrame = nFrames * widthPerFrame;
			int widthForScrollFrame = widthForTotalFrame;


			//출력할 레이어 개수

			int timelineLayers = Mathf.Max(10, Editor.TimelineInfoList.Count);

			//레이어의 높이
			int heightPerTimeline = 24;
			//int heightPerLayer = 32;
			int heightPerLayer = 28;//조금 작게 만들자

			int heightForScrollLayer = (timelineLayers * heightPerLayer);

			//이벤트가 발생했다면 Repaint하자
			bool isEventOccurred = false;


			//GL에 크기값을 넣어주자
			apTimelineGL.SetLayoutSize(timelineWidth, recordAndSummaryHeight, timelineHeight,
											layoutX + leftTabWidth,
											layoutY, layoutY + recordAndSummaryHeight,
											windowWidth, windowHeight,
											isDrawMainTimeline, _scroll_Timeline);

			//GL에 마우스 값을 넣고 업데이트를 하자

			bool isLeftBtnPressed = false;
			bool isRightBtnPressed = false;
			
			if (Event.current.rawType == EventType.MouseDown ||
				Event.current.rawType == EventType.MouseDrag)
			{
				if (Event.current.button == 0)			{ isLeftBtnPressed = true; }
				else if (Event.current.button == 1)		{ isRightBtnPressed = true; }
			}

#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			apTimelineGL.SetMouseValue(isLeftBtnPressed,
										isRightBtnPressed,
										apMouse.PosNotBound,
										Event.current.shift, isCtrl, Event.current.alt,
										Event.current.rawType,
										this);


			//TODO

			//GUI의 배경 색상
			Color prevColor = GUI.backgroundColor;
			if(EditorGUIUtility.isProSkin)
			{
				GUI.backgroundColor = new Color(	Editor._guiMainEditorColor.r * 0.8f,
										Editor._guiMainEditorColor.g * 0.8f,
										Editor._guiMainEditorColor.b * 0.8f,
										1.0f);
			}
			else
			{
				GUI.backgroundColor = Editor._guiMainEditorColor;
			}
			
			Rect timelineRect = new Rect(lastRect.x + leftTabWidth + 4, lastRect.y, timelineWidth, guiHeight + 15);
			GUI.Box(timelineRect, "", apEditorUtil.WhiteGUIStyle_Box);

			if(EditorGUIUtility.isProSkin)
			{
				GUI.backgroundColor = new Color(	Editor._guiSubEditorColor.r * 0.8f,
										Editor._guiSubEditorColor.g * 0.8f,
										Editor._guiSubEditorColor.b * 0.8f,
										1.0f);
			}
			else
			{
				GUI.backgroundColor = Editor._guiSubEditorColor;
			}
			
			Rect timelineBottomRect = new Rect(lastRect.x + leftTabWidth + 4, lastRect.y + guiHeight + 15, timelineWidth, height - (guiHeight));
			GUI.Box(timelineBottomRect, "", apEditorUtil.WhiteGUIStyle_Box);

			GUI.backgroundColor = prevColor;

			//추가 : 하단 GUI도 넣어주자

			bool isWheelDrag = false;
			//마우스 휠 이벤트를 직접 주자
			if (Event.current.rawType == EventType.ScrollWheel)
			{
				//휠 드르륵..
				Vector2 mousePos = Event.current.mousePosition;

				if (mousePos.x > 0 && mousePos.x < lastRect.x + leftTabWidth + timelineWidth &&
					mousePos.y > lastRect.y + recordAndSummaryHeight && mousePos.y < lastRect.y + guiHeight)
				{
					_scroll_Timeline += Event.current.delta * 7;
					Event.current.Use();
					apTimelineGL.SetMouseUse();

					isEventOccurred = true;
				}
			}

			if (Event.current.isMouse && Event.current.type != EventType.Used)
			{
				//휠 클릭 후 드래그
				if (Event.current.button == 2)
				{
					if (Event.current.type == EventType.MouseDown)
					{
						Vector2 mousePos = Event.current.mousePosition;

						if (mousePos.x > leftTabWidth && mousePos.x < lastRect.x + leftTabWidth + timelineWidth &&
							mousePos.y > lastRect.y + recordAndSummaryHeight && mousePos.y < lastRect.y + guiHeight)
						{
							//휠클릭 드래그 시작
							_isTimelineWheelDrag = true;
							_prevTimelineWheelDragPos = mousePos;

							isWheelDrag = true;
							Event.current.Use();
							apTimelineGL.SetMouseUse();

							isEventOccurred = true;
						}
					}
					else if (Event.current.type == EventType.MouseDrag && _isTimelineWheelDrag)
					{
						Vector2 mousePos = Event.current.mousePosition;
						Vector2 deltaPos = mousePos - _prevTimelineWheelDragPos;

						//_scroll_Timeline -= deltaPos * 1.0f;
						_scroll_Timeline.x -= deltaPos.x * 1.0f;//X만 움직이자

						_prevTimelineWheelDragPos = mousePos;
						isWheelDrag = true;
						Event.current.Use();
						apTimelineGL.SetMouseUse();

						isEventOccurred = true;
					}
				}
			}

			if (!isWheelDrag && Event.current.isMouse)
			{
				_isTimelineWheelDrag = false;
			}

			// ┌──┬─────┬──┐
			// │ㅁㅁ│	  v      │ inf│
			// ├──┼─────┤    │
			// │~~~~│  ㅁ  ㅁ  │    │
			// │~~~~│    ㅁ    │    │
			// ├──┼─────┤    │
			// │ >  │Zoom      │    │
			// └──┴─────┴──┘

			//1-1 요약부 : [레코드] + [타임과 통합 키프레임]
			EditorGUILayout.BeginHorizontal(GUILayout.Width(mainTabWidth), GUILayout.Height(recordAndSummaryHeight));

			int animEditBtnGroupHeight = 30;

			EditorGUILayout.BeginVertical(GUILayout.Width(leftTabWidth), GUILayout.Height(recordAndSummaryHeight));
			EditorGUILayout.BeginHorizontal(GUILayout.Width(leftTabWidth), GUILayout.Height(animEditBtnGroupHeight));
			GUILayout.Space(5);

			//Texture2D imgAutoKey = null;
			//if (IsAnimAutoKey)	{ imgAutoKey = Editor.ImageSet.Get(apImageSet.PRESET.Anim_KeyOn); }
			//else					{ imgAutoKey = Editor.ImageSet.Get(apImageSet.PRESET.Anim_KeyOff); }

			Texture2D imgKeyLock = null;
			if (IsAnimSelectionLock)	{ imgKeyLock = Editor.ImageSet.Get(apImageSet.PRESET.Edit_SelectionLock); }
			else						{ imgKeyLock = Editor.ImageSet.Get(apImageSet.PRESET.Edit_SelectionUnlock); }

			Texture2D imgLayerLock = null;
			if (ExAnimEditingMode == EX_EDIT.General_Edit)	{ imgLayerLock = Editor.ImageSet.Get(apImageSet.PRESET.Edit_ModUnlock); }
			else											{ imgLayerLock = Editor.ImageSet.Get(apImageSet.PRESET.Edit_ModLock); }

			Texture2D imgAddKeyframe = Editor.ImageSet.Get(apImageSet.PRESET.Anim_AddKeyframe);

			// 요약부 + 왼쪽의 [레코드] 부분
			//1. Start / Stop Editing (Toggle)
			//2. Auto Key (Toggle)
			//3. Set Key
			//4. Lock (Toggle)로 이루어져 있다.

			
			GUIStyle btnGUIStyle = new GUIStyle(GUI.skin.button);
			btnGUIStyle.alignment = TextAnchor.MiddleLeft;

			Texture2D editIcon = null;
			string strButtonName = "";
			bool isEditable = false;


			if (ExAnimEditingMode != EX_EDIT.None)
			{
				//현재 애니메이션 수정 작업중이라면..
				editIcon = Editor.ImageSet.Get(apImageSet.PRESET.Edit_Recording);
				//strButtonName = " Editing";
				strButtonName = " " + Editor.GetUIWord(UIWORD.EditingAnim);
				isEditable = true;
			}
			else
			{
				//현재 애니메이션 수정 작업을 하고 있지 않다면..
				if (IsAnimEditable)
				{
					editIcon = Editor.ImageSet.Get(apImageSet.PRESET.Edit_Record);
					//strButtonName = " Start Edit";
					strButtonName = " " + Editor.GetUIWord(UIWORD.StartEdit);
					isEditable = true;
				}
				else
				{
					editIcon = Editor.ImageSet.Get(apImageSet.PRESET.Edit_NoRecord);
					//strButtonName = " No-Editable";
					strButtonName = " " + Editor.GetUIWord(UIWORD.NoEditable);
				}
			}

			

			// Anim 편집 On/Off
			//Animation Editing On / Off
			if (apEditorUtil.ToggledButton_2Side(editIcon, strButtonName, strButtonName, ExAnimEditingMode != EX_EDIT.None, isEditable, 105, animEditBtnGroupHeight, "Animation Edit Mode (A)", btnGUIStyle))
			{
				//AnimEditing을 On<->Off를 전환하고 기즈모 이벤트를 설정한다.
				SetAnimEditingToggle();
			}

			//2개의 Lock 버튼
			if (apEditorUtil.ToggledButton_2Side(imgKeyLock, IsAnimSelectionLock, ExAnimEditingMode != EX_EDIT.None, 35, animEditBtnGroupHeight, "Selection Lock/Unlock (S)"))
			{
				_isAnimLock = !_isAnimLock;

				Editor.RefreshTimelineLayers(false);
			}

			
			
#if UNITY_EDITOR_OSX
			string strCtrlKey = "Command";
#else
			string strCtrlKey = "Ctrl";
#endif
			
			if (apEditorUtil.ToggledButton_2Side(
				imgLayerLock, 
				ExAnimEditingMode == EX_EDIT.ExOnly_Edit, 
				ExAnimEditingMode != EX_EDIT.None, 
				35, 
				animEditBtnGroupHeight, 
				"Timeline Layer Lock/Unlock (D) / If you press the button while holding down [" + strCtrlKey + "], the Setting dialog opens"
				))
			{
				//변경 3.22 : Ctrl 키를 누르고 클릭하면 설정 Dialog가 뜬다.

				if (isCtrl)
				{
					apDialog_ModifierLockSetting.ShowDialog(Editor, _portrait);
				}
				else
				{
					SetAnimEditingLayerLockToggle();//Mod Layer Lock을 토글
				}
			}

			//"Add Key"
			if (apEditorUtil.ToggledButton_2Side(imgAddKeyframe, Editor.GetUIWord(UIWORD.AddKey), Editor.GetUIWord(UIWORD.AddKey), false, ExAnimEditingMode != EX_EDIT.None, 85, animEditBtnGroupHeight, "Add Keyframe"))
			{
				//Debug.LogError("TODO : Set Key");
				if (AnimTimelineLayer != null)
				{
					apAnimKeyframe addedKeyframe = Editor.Controller.AddAnimKeyframe(AnimClip.CurFrame, AnimTimelineLayer, true);
					if(addedKeyframe != null)
					{
						//프레임을 이동하자
						_animClip.SetFrame_Editor(addedKeyframe._frameIndex);
						SetAnimKeyframe(addedKeyframe, true, apGizmos.SELECT_TYPE.New);
					}
				}
			}

			

			//단축키 [A]로 Editing 상태 토글
			//단축키 [S]에 의해서 Seletion Lock을 켜고 끌 수 있다.
			//단축키 [D]로 Layer Lock을 토글
			Editor.AddHotKeyEvent(OnHotKey_AnimEditingToggle, "Toggle Editing Mode", KeyCode.A, false, false, false, null);
			Editor.AddHotKeyEvent(OnHotKey_AnimSelectionLockToggle, "Toggle Selection Lock", KeyCode.S, false, false, false, null);
			Editor.AddHotKeyEvent(OnHotKey_AnimLayerLockToggle, "Toggle Layer Lock", KeyCode.D, false, false, false, null);
			Editor.AddHotKeyEvent(OnHotKey_AnimAddKeyframe, "Add New Keyframe", KeyCode.F, false, false, false, null);


			//if(GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_AutoZoom), GUILayout.Width(30), GUILayout.Height(30)))

			EditorGUILayout.EndHorizontal();

			//"Add Keyframes to All Layers"
			if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.AddKeyframesToAllLayers), Editor.GetUIWord(UIWORD.AddKeyframesToAllLayers), false, ExAnimEditingMode != EX_EDIT.None, leftTabWidth - (10), 20))
			{
				//현재 프레임의 모든 레이어에 Keyframe을 추가한다.
				//이건 다이얼로그로 꼭 물어보자
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.AddKeyframeToAllLayer_Title),
														Editor.GetText(TEXT.AddKeyframeToAllLayer_Body),
														Editor.GetText(TEXT.Okay),
														Editor.GetText(TEXT.Cancel));

				if(isResult)
				{
					Editor.Controller.AddAnimKeyframeToAllLayer(AnimClip.CurFrame, AnimClip, true);
				}
				
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(timelineWidth), GUILayout.Height(recordAndSummaryHeight));

			// 요약부 + 오른쪽의 [시간 / 통합 키 프레임]
			// 이건 GUI로 해야한다.
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();



			//1-2 메인 타임라인 : [레이어] + [타임라인 메인]
			EditorGUILayout.BeginHorizontal(GUILayout.Width(mainTabWidth), GUILayout.Height(timelineHeight));

			EditorGUILayout.BeginVertical(GUILayout.Width(leftTabWidth), GUILayout.Height(timelineHeight));
			GUILayout.BeginArea(new Rect(lastRect.x, lastRect.y + recordAndSummaryHeight, leftTabWidth, timelineHeight));
			// 메인 + 왼쪽의 [레이어] 부분

			// 레이어에 대한 렌더링 (정보 부분)
			//--------------------------------------------------------------
			int nTimelines = AnimClip._timelines.Count;
			//apAnimTimeline curTimeline = null;
			int curLayerY = 0;

			//GUIStyle guiStyle_layerInfoBox = new GUIStyle(GUI.skin.label);
			//GUIStyle guiStyle_layerInfoBox = new GUIStyle(GUI.skin.box);
			GUIStyle guiStyle_layerInfoBox = new GUIStyle(GUI.skin.label);
			guiStyle_layerInfoBox.alignment = TextAnchor.MiddleLeft;
			guiStyle_layerInfoBox.padding = GUI.skin.button.padding;

			int baseLeftPadding = GUI.skin.button.padding.left;

			int btnWidth_Layer = leftTabWidth + 4;
			//Texture2D img_HideLayer = Editor.ImageSet.Get(apImageSet.PRESET.Anim_HideLayer);
			Texture2D img_TimelineFolded = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight);
			Texture2D img_TimelineNotFolded = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);

			Texture2D img_CurFold = null;

			GUIStyle guiStyle_LeftBtn = new GUIStyle(GUI.skin.button);
			guiStyle_LeftBtn.padding = new RectOffset(0, 0, 0, 0);

			for (int iLayer = 0; iLayer < timelineInfoList.Count; iLayer++)
			{
				apTimelineLayerInfo info = timelineInfoList[iLayer];

				//일단 렌더링 여부를 초기화한다.
				//Layer Info는 GUI에서 그리도록 하고, 나중에 TimelineGL에서 렌더링을 할지 결정한다.
				info._isRenderable = false;

				if (!info._isTimeline && !info.IsVisibleLayer)
				{
					//숨겨진 레이어이다.
					info._guiLayerPosY = 0.0f;
					continue;
				}
				int layerHeight = heightPerLayer;
				int leftPadding = baseLeftPadding + 20;
				if (info._isTimeline)
				{
					layerHeight = heightPerTimeline;
					leftPadding = baseLeftPadding;
				}

				//배경 / 텍스트 색상을 정하자
				Color layerBGColor = info.GUIColor;
				Color textColor = Color.black;

				info._guiLayerPosY = curLayerY;

				float relativeY = info._guiLayerPosY - _scroll_Timeline.y;
				if(relativeY + layerHeight < 0 || relativeY > timelineHeight)
				{
					//렌더링 영역 바깥에 있다.
					//TimelineGL에서는 출력하지 않도록 한다. 에디터가 빨라지겠져
					info._isRenderable = false;
				}
				else
				{
					info._isRenderable = true;
				}
				

				if (!info._isAvailable)
				{
					textColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}
				else
				{
					float grayScale = (layerBGColor.r + layerBGColor.g + layerBGColor.b) / 3.0f;
					if (grayScale < 0.3f)
					{
						textColor = Color.white;
					}
				}

				//아이콘을 결정하자

				guiStyle_layerInfoBox.normal.textColor = textColor;
				guiStyle_layerInfoBox.padding.left = leftPadding;


				Texture2D layerIcon = Editor.ImageSet.Get(info.IconImgType);




				//[ 레이어 선택 ]

				if (info._isTimeline)
				{
					GUI.backgroundColor = layerBGColor;
					GUI.Box(new Rect(0, curLayerY - _scroll_Timeline.y, btnWidth_Layer, layerHeight), "", apEditorUtil.WhiteGUIStyle_Box);

					int yOffset = (layerHeight - 18) / 2;

					if (info.IsTimelineFolded)
					{
						img_CurFold = img_TimelineFolded;
					}
					else
					{
						img_CurFold = img_TimelineNotFolded;
					}
					if (GUI.Button(new Rect(2, (curLayerY + yOffset) - _scroll_Timeline.y, 18, 18), img_CurFold, guiStyle_LeftBtn))
					{
						if (info._timeline != null)
						{
							info._timeline._guiTimelineFolded = !info._timeline._guiTimelineFolded;
						}
					}

					GUI.backgroundColor = prevColor;

					if (GUI.Button(new Rect(19, curLayerY - _scroll_Timeline.y, btnWidth_Layer, layerHeight),
					new GUIContent("  " + info.DisplayName, layerIcon), guiStyle_layerInfoBox))
					{
						nextSelectLayerInfo = info;//<<선택!
					}
				}
				else
				{
					//[ Hide 버튼]
					//int xOffset = (btnWidth_Layer - (layerHeight + 4)) + 2;
					//int xOffset = 18;
					int yOffset = (layerHeight - 18) / 2;

					GUI.backgroundColor = layerBGColor;
					GUI.Box(new Rect(0, curLayerY - _scroll_Timeline.y, btnWidth_Layer, layerHeight), "", apEditorUtil.WhiteGUIStyle_Box);

					if (GUI.Button(new Rect(2, (curLayerY + yOffset) - _scroll_Timeline.y, 18, 18), "-", guiStyle_LeftBtn))
					{
						//Hide
						info._layer._guiLayerVisible = false;//<<숨기자!
					}

					GUI.backgroundColor = prevColor;


					//2 + 18 + 2 = 22
					if (GUI.Button(new Rect(19, curLayerY - _scroll_Timeline.y, btnWidth_Layer - 22, layerHeight),
					new GUIContent("  " + info.DisplayName, layerIcon), guiStyle_layerInfoBox))
					{
						nextSelectLayerInfo = info;//<<선택!
					}
				}





				//GUI.backgroundColor = prevColor;

				curLayerY += layerHeight;
			}




			//--------------------------------------------------------------

			GUILayout.EndArea();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(timelineWidth), GUILayout.Height(timelineHeight));
			GUILayout.BeginArea(timelineRect);
			// 메인 + 오른쪽의 [메인 타임라인]
			// 이건 GUI로 해야한다.

			//기본 타임라인 GL 세팅
			apTimelineGL.SetTimelineSetting(0, AnimClip.StartFrame, AnimClip.EndFrame, Editor.WidthPerFrameInTimeline, AnimClip.IsLoop);
			//apTimelineGL.DrawTimeBars_Header(new Color(0.4f, 0.4f, 0.4f, 1.0f));

			// 레이어에 대한 렌더링 (타임라인 부분 - BG)
			//--------------------------------------------------------------
			curLayerY = 0;
			for (int iLayer = 0; iLayer < timelineInfoList.Count; iLayer++)
			{
				apTimelineLayerInfo info = timelineInfoList[iLayer];
				int layerHeight = heightPerLayer;

				if (!info._isTimeline && !info.IsVisibleLayer)
				{
					continue;
				}
				if (info._isTimeline)
				{
					layerHeight = heightPerTimeline;
				}
				if (info._isSelected)
				{
					apTimelineGL.DrawTimeBars_MainBG(info.TimelineColor, curLayerY + layerHeight - (int)_scroll_Timeline.y, layerHeight);
				}
				curLayerY += layerHeight;
			}

			//Grid를 그린다.
			apTimelineGL.DrawTimelineAreaBG(ExAnimEditingMode != EX_EDIT.None);
			apTimelineGL.DrawTimeGrid(new Color(0.4f, 0.4f, 0.4f, 1.0f), new Color(0.3f, 0.3f, 0.3f, 1.0f), new Color(0.7f, 0.7f, 0.7f, 1.0f));
			apTimelineGL.DrawTimeBars_Header(new Color(0.4f, 0.4f, 0.4f, 1.0f));



			// 레이어에 대한 렌더링 (타임라인 부분 - Line + Frames)
			//--------------------------------------------------------------
			curLayerY = 0;
			bool isAnyHidedLayer = false;
			apTimelineGL.BeginKeyframeControl();

			for (int iLayer = 0; iLayer < timelineInfoList.Count; iLayer++)
			{
				apTimelineLayerInfo info = timelineInfoList[iLayer];
				if (!info._isTimeline && !info.IsVisibleLayer)
				{
					//숨겨진 레이어
					isAnyHidedLayer = true;
					continue;
				}
				int layerHeight = heightPerLayer;
				if (info._isTimeline)
				{
					layerHeight = heightPerTimeline;
				}

				if (info._isRenderable)
				{
					apTimelineGL.DrawTimeBars_MainLine(new Color(0.3f, 0.3f, 0.3f, 1.0f), curLayerY + layerHeight - (int)_scroll_Timeline.y);

					if (!info._isTimeline)
					{
						Color curveEditColor = Color.black;
						//if (_animPropertyUI == ANIM_SINGLE_PROPERTY_UI.Curve)
						//{
						//	apAnimCurveResult curveResult = null;
						//	if (AnimKeyframe != null)
						//	{
						//		if (_animPropertyCurveUI == ANIM_SINGLE_PROPERTY_CURVE_UI.Prev)
						//		{
						//			curveResult = AnimKeyframe._curveKey._prevCurveResult;
						//		}
						//		else
						//		{
						//			curveResult = AnimKeyframe._curveKey._nextCurveResult;
						//		}
						//	}
						//}
						apTimelineGL.DrawKeyframes(info._layer,
													curLayerY + layerHeight / 2,
													info.GUIColor,
													info._isAvailable,
													layerHeight,
													(AnimTimelineLayer == info._layer),
													AnimClip.CurFrame,
													_animPropertyUI == ANIM_SINGLE_PROPERTY_UI.Curve,
													_animPropertyCurveUI
													//curveEditColor
													);
					}
				}
				curLayerY += layerHeight;


			}
			


			// Play Bar를 그린다.
			//int prevClipFrame = AnimClip.CurFrame;
			//bool isAutoRefresh = false;
			apTimelineGL.DrawKeySummry(_subAnimCommonKeyframeList, 58);
			apTimelineGL.DrawEventMarkers(_animClip._animEvents, 30);

			if(Editor.Onion.IsVisible && Editor.Onion.IsRecorded)
			{
				apTimelineGL.DrawOnionMarkers(
					Editor.Onion.RecordAnimFrame,
					new Color(
						Mathf.Clamp01(Editor._colorOption_OnionToneColor.r * 2),
						Mathf.Clamp01(Editor._colorOption_OnionToneColor.g * 2),
						Mathf.Clamp01(Editor._colorOption_OnionToneColor.b * 2),
						1.0f), 
					44
					);
			}


			bool isChangeFrame = apTimelineGL.DrawPlayBar(AnimClip.CurFrame);
			if (isChangeFrame)
			{
				AutoSelectAnimWorkKeyframe();
				//isAutoRefresh = true;
			}

			bool isKeyframeEvent = apTimelineGL.EndKeyframeControl();//<<제어용 함수
			if (isKeyframeEvent) { isEventOccurred = true; }

			//if(prevClipFrame != AnimClip.CurFrame)
			//{
			//	Debug.Log("Frame Changed [" + isAutoRefresh + "] : " + (AnimWorkKeyframe != null));
			//}

			apTimelineGL.DrawAndUpdateSelectArea();

			//키프레임+타임 슬라이더를 화면 끝으로 이동한 경우 자동 스크롤
			if(apTimelineGL.IsKeyframeDragging)
			{
				float rightBound = timelineRect.xMin + (timelineWidth - rightTabWidth) + 30;
				float leftBound = 30;
				if(apMouse.Pos.x > rightBound || apMouse.Pos.x < leftBound)
				{	
					_animKeyframeAutoScrollTimer += apTimer.I.DeltaTime_Repaint;
					if(_animKeyframeAutoScrollTimer > 0.1f)
					{
						_animKeyframeAutoScrollTimer = 0.0f;
						AutoAnimScrollWithoutFrameMoving(apTimelineGL.FrameOnMouseX, 1);
						apTimelineGL.RefreshScrollDown();
					}
				}
				
			}


			//--------------------------------------------------------------

			GUILayout.EndArea();
			EditorGUILayout.EndVertical();


			EditorGUILayout.EndHorizontal();


			//TODO : 스크롤은 현재 키프레임의 범위, 레이어의 개수에 따라 바뀐다.
			float prevScrollTimelineY = _scroll_Timeline.y;
			_scroll_Timeline.y = GUI.VerticalScrollbar(new Rect(lastRect.x + leftTabWidth + 4 + timelineWidth - 15, lastRect.y, 15, timelineHeight + recordAndSummaryHeight + 4), _scroll_Timeline.y, 20.0f, 0.0f, heightForScrollLayer);
			if (Mathf.Abs(prevScrollTimelineY - _scroll_Timeline.y) > 0.5f)
			{
				//Debug.Log("Scroll Y");
				Event.current.Use();
				apTimelineGL.SetMouseUse();
			}

			//Anim 레이어를 선택하자
			if (nextSelectLayerInfo != null)
			{
				_isIgnoreAnimTimelineGUI = true;//<깜빡이지 않게..
				if (nextSelectLayerInfo._isTimeline)
				{
					//Timeline을 선택하기 전에
					//Anim객체를 초기화한다. (안그러면 자동으로 선택된 오브젝트에 의해서 TimelineLayer를 선택하게 된다.)
					SetBoneForAnimClip(null);
					SetSubControlParamForAnimClipEdit(null);
					SetSubMeshTransformForAnimClipEdit(null);
					SetSubMeshGroupTransformForAnimClipEdit(null);

					SetAnimTimeline(nextSelectLayerInfo._timeline, true, true);
					SetAnimTimelineLayer(null, true, true, true);
					SetAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
				}
				else
				{
					SetAnimTimeline(nextSelectLayerInfo._parentTimeline, true, true);
					SetAnimTimelineLayer(nextSelectLayerInfo._layer, true, true, true);
					SetAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
				}
				AutoSelectAnimWorkKeyframe();

				Editor.RefreshControllerAndHierarchy();
			}

			float prevScrollTimelineX = _scroll_Timeline.x;
			_scroll_Timeline.x = GUI.HorizontalScrollbar(new Rect(lastRect.x + leftTabWidth + 4, lastRect.y + recordAndSummaryHeight + timelineHeight + 4, timelineWidth - 15, 15),
															_scroll_Timeline.x,
															20.0f, 0.0f,
															widthForScrollFrame);

			if (Mathf.Abs(prevScrollTimelineX - _scroll_Timeline.x) > 0.5f)
			{
				//Debug.Log("Scroll X");
				Event.current.Use();
				apTimelineGL.SetMouseUse();
			}

			if (GUI.Button(new Rect(lastRect.x + leftTabWidth + 4 + timelineWidth - 15, lastRect.y + recordAndSummaryHeight + timelineHeight + 4, 15, 15), ""))
			{
				_scroll_Timeline.x = 0;
				_scroll_Timeline.y = 0;
			}

			//1-3 하단 컨트롤과 스크롤 : [컨트롤러] + [스크롤 + 애니메이션 설정]
			int ctrlBtnSize_Small = 30;
			int ctrlBtnSize_Large = 30;
			int ctrlBtnSize_LargeUnder = bottomControlHeight - (ctrlBtnSize_Large + 6);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(mainTabWidth), GUILayout.Height(bottomControlHeight));
			EditorGUILayout.BeginVertical(GUILayout.Width(leftTabWidth), GUILayout.Height(bottomControlHeight));
			EditorGUILayout.BeginHorizontal(GUILayout.Width(leftTabWidth), GUILayout.Height(ctrlBtnSize_Large + 2));
			GUILayout.Space(5);


			//플레이 제어 단축키
			//단축키 [<, >]로 키프레임을 이동할 수 있다.
			// Space : 재생/정지
			// <, > : 1프레임 이동
			// Shift + <, > : 첫프레임, 끝프레임으로 이동
			Editor.AddHotKeyEvent(OnHotKey_AnimMoveFrame, "Play/Pause", KeyCode.Space, false, false, false, 0);
			Editor.AddHotKeyEvent(OnHotKey_AnimMoveFrame, "Previous Frame", KeyCode.Comma, false, false, false, 1);
			Editor.AddHotKeyEvent(OnHotKey_AnimMoveFrame, "Next Frame", KeyCode.Period, false, false, false, 2);
			Editor.AddHotKeyEvent(OnHotKey_AnimMoveFrame, "First Frame", KeyCode.Comma, true, false, false, 3);
			Editor.AddHotKeyEvent(OnHotKey_AnimMoveFrame, "Last Frame", KeyCode.Period, true, false, false, 4);


			//플레이 제어
			if (GUILayout.Button(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Anim_FirstFrame), "Move to First Frame (Shift + <)"), GUILayout.Width(ctrlBtnSize_Large), GUILayout.Height(ctrlBtnSize_Large)))
			{
				//제어 : 첫 프레임으로 이동
				AnimClip.SetFrame_Editor(AnimClip.StartFrame);
				AutoSelectAnimWorkKeyframe();
			}

			if (GUILayout.Button(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Anim_PrevFrame), "Move to Previous Frame (<)"), GUILayout.Width(ctrlBtnSize_Large + 10), GUILayout.Height(ctrlBtnSize_Large)))
			{
				//제어 : 이전 프레임으로 이동
				int prevFrame = AnimClip.CurFrame - 1;
				if (prevFrame < AnimClip.StartFrame)
				{
					if (AnimClip.IsLoop)
					{
						prevFrame = AnimClip.EndFrame;
					}
				}
				AnimClip.SetFrame_Editor(prevFrame);
				AutoSelectAnimWorkKeyframe();
			}

			Texture2D playIcon = null;
			if (AnimClip.IsPlaying_Editor)
			{
				//플레이중 -> Pause 버튼
				playIcon = Editor.ImageSet.Get(apImageSet.PRESET.Anim_Pause);
			}
			else
			{
				//일시 정지 -> 플레이 버튼
				playIcon = Editor.ImageSet.Get(apImageSet.PRESET.Anim_Play);
			}

			if (GUILayout.Button(new GUIContent(playIcon, "Play/Pause (Space Bar)"), GUILayout.Width(ctrlBtnSize_Large + 30), GUILayout.Height(ctrlBtnSize_Large)))
			{
				//제어 : 플레이 / 일시정지
				if (AnimClip.IsPlaying_Editor)
				{
					// 플레이 -> 일시 정지
					AnimClip.Pause_Editor();
				}
				else
				{
					//마지막 프레임이라면 첫 프레임으로 이동하여 재생한다.
					if (AnimClip.CurFrame == AnimClip.EndFrame)
					{
						AnimClip.SetFrame_Editor(AnimClip.StartFrame);
					}
					// 일시 정지 -> 플레이
					AnimClip.Play_Editor();
				}

				//Play 전환 여부에 따라서도 WorkKeyframe을 전환한다.
				AutoSelectAnimWorkKeyframe();
				Editor.SetRepaint();
				Editor.Gizmos.SetUpdate();

			}

			if (GUILayout.Button(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Anim_NextFrame), "Move to Next Frame (>)"), GUILayout.Width(ctrlBtnSize_Large + 10), GUILayout.Height(ctrlBtnSize_Large)))
			{
				//제어 : 다음 프레임으로 이동
				int nextFrame = AnimClip.CurFrame + 1;
				if (nextFrame > AnimClip.EndFrame)
				{
					if (AnimClip.IsLoop)
					{
						nextFrame = AnimClip.StartFrame;
					}
				}
				AnimClip.SetFrame_Editor(nextFrame);
				AutoSelectAnimWorkKeyframe();
			}

			if (GUILayout.Button(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Anim_LastFrame), "Move to Last Frame (Shift + >)"), GUILayout.Width(ctrlBtnSize_Large), GUILayout.Height(ctrlBtnSize_Large)))
			{
				//제어 : 마지막 프레임으로 이동
				AnimClip.SetFrame_Editor(AnimClip.EndFrame);
				AutoSelectAnimWorkKeyframe();
			}

			GUILayout.Space(10);
			bool isLoopPlay = AnimClip.IsLoop;
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Anim_Loop), isLoopPlay, true, ctrlBtnSize_Large, ctrlBtnSize_Large, "Enable/Disable Loop"))
			{
				//AnimClip._isLoop = !AnimClip._isLoop;
				AnimClip.SetOption_IsLoop(!AnimClip.IsLoop);
				AnimClip.SetFrame_Editor(AnimClip.CurFrame);
				Editor.RefreshTimelineLayers(false);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.Width(leftTabWidth), GUILayout.Height(ctrlBtnSize_LargeUnder + 2));

			GUILayout.Space(5);

			//현재 프레임 + 세밀 조정
			//"Frame"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Frame), GUILayout.Width(80), GUILayout.Height(ctrlBtnSize_LargeUnder));
			int curFrame = AnimClip.CurFrame;
			int nextCurFrame = EditorGUILayout.IntSlider(curFrame, AnimClip.StartFrame, AnimClip.EndFrame, GUILayout.Width(leftTabWidth - 95), GUILayout.Height(ctrlBtnSize_LargeUnder));
			if (nextCurFrame != curFrame)
			{
				AnimClip.SetFrame_Editor(nextCurFrame);
				AutoSelectAnimWorkKeyframe();
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(mainTabWidth - leftTabWidth), GUILayout.Height(bottomControlHeight));
			GUILayout.Space(18);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(mainTabWidth - leftTabWidth), GUILayout.Height(bottomControlHeight - 18));

			//맨 하단은 키 복붙이나 View, 영역 등에 관련된 정보를 출력한다.
			GUILayout.Space(10);

			//Timeline 정렬
			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Anim_SortRegOrder), 
											Editor._timelineInfoSortType == apEditor.TIMELINE_INFO_SORT.Registered, 
											true, ctrlBtnSize_Small, ctrlBtnSize_Small,
											"Sort by registeration order"))
			{
				Editor._timelineInfoSortType = apEditor.TIMELINE_INFO_SORT.Registered;
				Editor.SaveEditorPref();
				Editor.RefreshTimelineLayers(true);
			}

			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Anim_SortABC), 
											Editor._timelineInfoSortType == apEditor.TIMELINE_INFO_SORT.ABC, 
											true, ctrlBtnSize_Small, ctrlBtnSize_Small,
											"Sort by name"))
			{
				Editor._timelineInfoSortType = apEditor.TIMELINE_INFO_SORT.ABC;
				Editor.SaveEditorPref();
				Editor.RefreshTimelineLayers(true);
			}

			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Anim_SortDepth), 
											Editor._timelineInfoSortType == apEditor.TIMELINE_INFO_SORT.Depth, 
											true, ctrlBtnSize_Small, ctrlBtnSize_Small,
											"Sort by Depth"))
			{
				Editor._timelineInfoSortType = apEditor.TIMELINE_INFO_SORT.Depth;
				Editor.SaveEditorPref();
				Editor.RefreshTimelineLayers(true);

			}

			GUILayout.Space(20);

			//"Unhide Layers"
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.UnhideLayers), !isAnyHidedLayer, 120, ctrlBtnSize_Small))
			{
				Editor.ShowAllTimelineLayers();
			}

			GUILayout.Space(20);

			// 타임라인 사이즈 (1, 2, 3)
			apEditor.TIMELINE_LAYOUTSIZE nextLayoutSize = Editor._timelineLayoutSize;

			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Anim_TimelineSize1), 
											Editor._timelineLayoutSize == apEditor.TIMELINE_LAYOUTSIZE.Size1, 
											true, ctrlBtnSize_Small, ctrlBtnSize_Small,
											"Timeline UI Size [Small]"))
			{
				nextLayoutSize = apEditor.TIMELINE_LAYOUTSIZE.Size1;
			}
			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Anim_TimelineSize2), 
											Editor._timelineLayoutSize == apEditor.TIMELINE_LAYOUTSIZE.Size2, 
											true, ctrlBtnSize_Small, ctrlBtnSize_Small,
											"Timeline UI Size [Medium]"))
			{
				nextLayoutSize = apEditor.TIMELINE_LAYOUTSIZE.Size2;
			}
			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Anim_TimelineSize3), 
											Editor._timelineLayoutSize == apEditor.TIMELINE_LAYOUTSIZE.Size3, 
											true, ctrlBtnSize_Small, ctrlBtnSize_Small,
											"Timeline UI Size [Large]"))
			{
				nextLayoutSize = apEditor.TIMELINE_LAYOUTSIZE.Size3;
			}


			//Zoom
			GUILayout.Space(4);

			EditorGUILayout.BeginVertical(GUILayout.Width(90));
			//EditorGUILayout.LabelField("Zoom", GUILayout.Width(100), GUILayout.Height(15));
			GUILayout.Space(7);
			int timelineLayoutSize_Min = 0;
			int timelineLayoutSize_Max = Editor._timelineZoomWPFPreset.Length - 1;

			int nextTimelineIndex = (int)(GUILayout.HorizontalSlider(Editor._timelineZoom_Index, timelineLayoutSize_Min, timelineLayoutSize_Max, GUILayout.Width(90), GUILayout.Height(20)) + 0.5f);
			if (nextTimelineIndex != Editor._timelineZoom_Index)
			{
				if (nextTimelineIndex < timelineLayoutSize_Min)			{ nextTimelineIndex = timelineLayoutSize_Min; }
				else if (nextTimelineIndex > timelineLayoutSize_Max)	{ nextTimelineIndex = timelineLayoutSize_Max; }

				Editor._timelineZoom_Index = nextTimelineIndex;
			}
			EditorGUILayout.EndVertical();

			//Fit은 유지
			if (GUILayout.Button(new GUIContent(" Fit", Editor.ImageSet.Get(apImageSet.PRESET.Anim_AutoZoom), "Zoom to fit the animation length"),
									GUILayout.Width(80), GUILayout.Height(ctrlBtnSize_Small)))
			{
				//Debug.LogError("TODO : Timeline AutoZoom");
				//Width / 전체 Frame수 = 목표 WidthPerFrame
				int numFrames = Mathf.Max(AnimClip.EndFrame - AnimClip.StartFrame, 1);
				int targetWidthPerFrame = (int)((float)timelineWidth / (float)numFrames + 0.5f);
				_scroll_Timeline.x = 0;
				//적절한 값을 찾자
				int optWPFIndex = -1;
				for (int i = 0; i < Editor._timelineZoomWPFPreset.Length; i++)
				{
					int curWPF = Editor._timelineZoomWPFPreset[i];
					if (curWPF < targetWidthPerFrame)
					{
						optWPFIndex = i;
						break;
					}
				}
				if (optWPFIndex < 0)
				{
					Editor._timelineZoom_Index = Editor._timelineZoomWPFPreset.Length - 1;
				}
				else
				{
					Editor._timelineZoom_Index = optWPFIndex;
				}
			}

			GUILayout.Space(4);

			//Auto Scroll
			//" Auto Scroll"
			string strAutoScroll = " " + Editor.GetUIWord(UIWORD.AutoScroll);
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Anim_AutoScroll),
													strAutoScroll, strAutoScroll,
													Editor._isAnimAutoScroll, true,
													140, ctrlBtnSize_Small,
													"Scrolls automatically according to the frame of the animation"))
			{
				Editor._isAnimAutoScroll = !Editor._isAnimAutoScroll;
			}



			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();


			EditorGUILayout.EndHorizontal();


			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(rightTabWidth), GUILayout.Height(height));
			//2. [우측] 선택된 레이어/키 정보

			_scrollPos_BottomAnimationRightProperty = EditorGUILayout.BeginScrollView(_scrollPos_BottomAnimationRightProperty, false, true, GUILayout.Width(rightTabWidth), GUILayout.Height(height));

			//int rightPropertyWidth = rightTabWidth - 24;
			int rightPropertyWidth = rightTabWidth - 28;

			EditorGUILayout.BeginVertical(GUILayout.Width(rightPropertyWidth));


			//프로퍼티 타이틀
			//프로퍼티는 (KeyFrame -> Layer -> Timeline -> None) 순으로 정보를 보여준다.
			string propertyTitle = "";
			int propertyType = 0;
			if (AnimKeyframe != null)
			{
				if (IsAnimKeyframeMultipleSelected)
				{
					//propertyTitle = "Keyframes [ " + AnimKeyframes.Count + " Selected ]";
					propertyTitle = string.Format("{0} [ {1} {2} ]", Editor.GetUIWord(UIWORD.Keyframes), AnimKeyframes.Count, Editor.GetUIWord(UIWORD.Selected));
					propertyType = 1;
				}
				else
				{
					//propertyTitle = "Keyframe [ " + AnimKeyframe._frameIndex + " ]";
					propertyTitle = string.Format("{0} [ {1} ]", Editor.GetUIWord(UIWORD.Keyframe), AnimKeyframe._frameIndex);
					propertyType = 2;
				}

			}
			else if (AnimTimelineLayer != null)
			{
				//propertyTitle = "Layer [" + AnimTimelineLayer.DisplayName + " ]";
				propertyTitle = string.Format("{0} [ {1} ]", Editor.GetUIWord(UIWORD.Layer), AnimTimelineLayer.DisplayName);
				propertyType = 3;
			}
			else if (AnimTimeline != null)
			{
				//propertyTitle = "Timeline [ " + AnimTimeline.DisplayName + " ]";
				propertyTitle = string.Format("{0} [ {1} ]", Editor.GetUIWord(UIWORD.Timeline), AnimTimeline.DisplayName);
				propertyType = 4;
			}
			else
			{
				//propertyTitle = "Not Selected";
				propertyTitle = Editor.GetUIWord(UIWORD.NotSelected);
			}

			GUIStyle guiStyleProperty = new GUIStyle(GUI.skin.box);
			guiStyleProperty.normal.textColor = Color.white;
			guiStyleProperty.alignment = TextAnchor.MiddleCenter;

			//GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);
			GUI.backgroundColor = apEditorUtil.ToggleBoxColor_Selected;

			GUILayout.Box(propertyTitle, guiStyleProperty, GUILayout.Width(rightPropertyWidth), GUILayout.Height(20));
			GUI.backgroundColor = prevColor;

			GUILayout.Space(5);


			Editor.SetGUIVisible("Animation Bottom Property - MK", propertyType == 1);
			Editor.SetGUIVisible("Animation Bottom Property - SK", propertyType == 2);
			Editor.SetGUIVisible("Animation Bottom Property - L", propertyType == 3);
			Editor.SetGUIVisible("Animation Bottom Property - T", propertyType == 4);

			switch (propertyType)
			{
				case 1:

					if (Editor.IsDelayedGUIVisible("Animation Bottom Property - MK"))
					{
						DrawEditor_Bottom_AnimationProperty_MultipleKeyframes(AnimKeyframes, rightPropertyWidth);
					}
					break;

				case 2:
					if (Editor.IsDelayedGUIVisible("Animation Bottom Property - SK"))
					{
						DrawEditor_Bottom_AnimationProperty_SingleKeyframe(
							AnimKeyframe,
							rightPropertyWidth,
							windowWidth,
							windowHeight,
							(layoutX + leftTabWidth + margin + mainTabWidth + margin),
							//layoutX + margin + mainTabWidth + margin, 
							//leftTabWidth + margin + mainTabWidth + margin, 
							(int)(layoutY),
							(int)(_scrollPos_BottomAnimationRightProperty.y)
							//(int)(layoutY)
							);
					}
					break;

				case 3:
					if (Editor.IsDelayedGUIVisible("Animation Bottom Property - L"))
					{
						DrawEditor_Bottom_AnimationProperty_TimelineLayer(AnimTimelineLayer, rightPropertyWidth);
					}
					break;

				case 4:
					if (Editor.IsDelayedGUIVisible("Animation Bottom Property - T"))
					{
						DrawEditor_Bottom_AnimationProperty_Timeline(AnimTimeline, rightPropertyWidth);
					}
					break;
			}



			GUILayout.Space(height);

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();


			EditorGUILayout.EndHorizontal();


			if (Editor._timelineLayoutSize != nextLayoutSize)
			{
				Editor._timelineLayoutSize = nextLayoutSize;
			}

			if (isEventOccurred)
			{
				Editor.SetRepaint();
			}
		}

		private apAnimKeyframe _tmpPrevSelectedAnimKeyframe = null;
		private object _loadKey_SinglePoseImport_Anim = null;

		//화면 우측의 UI 중 : 키프레임을 "1개 선택할 때" 출력되는 UI
		private void DrawEditor_Bottom_AnimationProperty_SingleKeyframe(apAnimKeyframe keyframe, int width, int windowWidth, int windowHeight, int layoutX, int layoutY, int scrollValue)
		{
			//TODO : 커브 조절


			//프레임 이동
			//EditorGUILayout.LabelField("Frame [" + keyframe._frameIndex + "]", GUILayout.Width(width));
			//GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));

			GUILayout.Space(5);

			Texture2D imgPrev = Editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToPrevFrame);
			Texture2D imgNext = Editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToNextFrame);
			Texture2D imgCurKey = Editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToCurrentFrame);

			int btnWidthSide = ((width - (10 + 80)) / 2) - 4;
			int btnWidthCenter = 90;
			bool isPrevKey = false;
			bool isNextKey = false;
			bool isCurKey = (AnimClip.CurFrame == keyframe._frameIndex);
			if (keyframe._prevLinkedKeyframe != null)
			{
				isPrevKey = true;
			}
			if (keyframe._nextLinkedKeyframe != null)
			{
				isNextKey = true;
			}

			if (apEditorUtil.ToggledButton_2Side(imgPrev, false, isPrevKey, btnWidthSide, 20))
			{
				//연결된 이전 프레임으로 이동한다.
				if (isPrevKey)
				{
					AnimClip.SetFrame_Editor(keyframe._prevLinkedKeyframe._frameIndex);
					SetAnimKeyframe(keyframe._prevLinkedKeyframe, true, apGizmos.SELECT_TYPE.New);
				}
			}
			if (apEditorUtil.ToggledButton_2Side(imgCurKey, isCurKey, true, btnWidthCenter, 20))
			{
				//현재 프레임으로 이동한다.
				AnimClip.SetFrame_Editor(keyframe._frameIndex);
				AutoSelectAnimWorkKeyframe();
				SetAutoAnimScroll();
			}
			if (apEditorUtil.ToggledButton_2Side(imgNext, false, isNextKey, btnWidthSide, 20))
			{
				//연결된 다음 프레임으로 이동한다.
				if (isNextKey)
				{
					AnimClip.SetFrame_Editor(keyframe._nextLinkedKeyframe._frameIndex);
					SetAnimKeyframe(keyframe._nextLinkedKeyframe, true, apGizmos.SELECT_TYPE.New);
				}
			}


			EditorGUILayout.EndHorizontal();


			//Value / Curve에 따라서 다른 UI가 나온다.
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(22));
			GUILayout.Space(5);
			//"Transform"
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.Transform),
											(_animPropertyUI == ANIM_SINGLE_PROPERTY_UI.Value),
											(width / 2) - 2
										))
			{
				_animPropertyUI = ANIM_SINGLE_PROPERTY_UI.Value;
			}
			//"Curve"
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.Curve),
											(_animPropertyUI == ANIM_SINGLE_PROPERTY_UI.Curve),
											(width / 2) - 2
										))
			{
				_animPropertyUI = ANIM_SINGLE_PROPERTY_UI.Curve;
			}

			EditorGUILayout.EndHorizontal();


			//키프레임 타입인 경우
			bool isControlParamUI = (AnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam &&
									AnimTimelineLayer._linkedControlParam != null);
			bool isModifierUI = (AnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier &&
									AnimTimeline._linkedModifier != null);

			Editor.SetGUIVisible("Bottom Right Anim Property - ControlParamUI", isControlParamUI);
			Editor.SetGUIVisible("Bottom Right Anim Property - ModifierUI", isModifierUI);

			bool isDrawControlParamUI = Editor.IsDelayedGUIVisible("Bottom Right Anim Property - ControlParamUI");
			bool isDrawModifierUI = Editor.IsDelayedGUIVisible("Bottom Right Anim Property - ModifierUI");


			apControlParam controlParam = AnimTimelineLayer._linkedControlParam;

			Editor.SetGUIVisible("Anim Property - SameKeyframe", _tmpPrevSelectedAnimKeyframe == keyframe);
			bool isSameKP = Editor.IsDelayedGUIVisible("Anim Property - SameKeyframe");

			//if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint)
			if (Event.current.type != EventType.Layout)
			{
				_tmpPrevSelectedAnimKeyframe = keyframe;
			}

			Color prevColor = GUI.backgroundColor;


			if (_animPropertyUI == ANIM_SINGLE_PROPERTY_UI.Value)
			{
				//1. Value Mode
				if (isDrawControlParamUI && isSameKP)
				{
					#region Control Param UI 그리는 코드
					GUILayout.Space(10);
					apEditorUtil.GUI_DelimeterBoxH(width);
					GUILayout.Space(10);

					GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);

					GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
					guiStyleBox.alignment = TextAnchor.MiddleCenter;
					guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

					//"Control Parameter Value"
					GUILayout.Box(Editor.GetUIWord(UIWORD.ControlParameterValue),
									guiStyleBox,
									GUILayout.Width(width), GUILayout.Height(30));

					GUI.backgroundColor = prevColor;


					GUIStyle guiStyle_LableMin = new GUIStyle(GUI.skin.label);
					guiStyle_LableMin.alignment = TextAnchor.MiddleLeft;

					GUIStyle guiStyle_LableMax = new GUIStyle(GUI.skin.label);
					guiStyle_LableMax.alignment = TextAnchor.MiddleRight;
					int widthLabelRange = (width / 2) - 2;

					GUILayout.Space(5);

					bool isChanged = false;

					switch (controlParam._valueType)
					{
						case apControlParam.TYPE.Int:
							{
								int iNext = keyframe._conSyncValue_Int;

								EditorGUILayout.LabelField(controlParam._keyName, GUILayout.Width(width));
								EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
								EditorGUILayout.LabelField(controlParam._label_Min, guiStyle_LableMin, GUILayout.Width(widthLabelRange));
								EditorGUILayout.LabelField(controlParam._label_Max, guiStyle_LableMax, GUILayout.Width(widthLabelRange));
								EditorGUILayout.EndHorizontal();
								iNext = EditorGUILayout.IntSlider(keyframe._conSyncValue_Int, controlParam._int_Min, controlParam._int_Max, GUILayout.Width(width));


								if (iNext != keyframe._conSyncValue_Int)
								{
									apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, Editor._portrait, keyframe, true);

									keyframe._conSyncValue_Int = iNext;
									isChanged = true;
								}
							}
							break;

						case apControlParam.TYPE.Float:
							{
								float fNext = keyframe._conSyncValue_Float;

								EditorGUILayout.LabelField(controlParam._keyName, GUILayout.Width(width));
								EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
								EditorGUILayout.LabelField(controlParam._label_Min, guiStyle_LableMin, GUILayout.Width(widthLabelRange));
								EditorGUILayout.LabelField(controlParam._label_Max, guiStyle_LableMax, GUILayout.Width(widthLabelRange));
								EditorGUILayout.EndHorizontal();
								fNext = EditorGUILayout.Slider(keyframe._conSyncValue_Float, controlParam._float_Min, controlParam._float_Max, GUILayout.Width(width));

								if (fNext != keyframe._conSyncValue_Float)
								{
									apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, Editor._portrait, keyframe, true);

									keyframe._conSyncValue_Float = fNext;
									isChanged = true;
								}
							}
							break;

						case apControlParam.TYPE.Vector2:
							{
								Vector2 v2Next = keyframe._conSyncValue_Vector2;
								EditorGUILayout.LabelField(controlParam._keyName, GUILayout.Width(width));

								EditorGUILayout.LabelField(controlParam._label_Min, GUILayout.Width(width));
								v2Next.x = EditorGUILayout.Slider(keyframe._conSyncValue_Vector2.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x, GUILayout.Width(width));

								EditorGUILayout.LabelField(controlParam._label_Max, GUILayout.Width(width));
								v2Next.y = EditorGUILayout.Slider(keyframe._conSyncValue_Vector2.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y, GUILayout.Width(width));

								if (v2Next.x != keyframe._conSyncValue_Vector2.x ||
									v2Next.y != keyframe._conSyncValue_Vector2.y)
								{
									apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, Editor._portrait, keyframe, true);

									keyframe._conSyncValue_Vector2 = v2Next;
									isChanged = true;
								}
							}
							break;

					}

					if (isChanged)
					{
						AnimClip.UpdateControlParam(true);
					}
					#endregion
				}

				if (isDrawModifierUI && isSameKP)
				{
					GUILayout.Space(10);
					apEditorUtil.GUI_DelimeterBoxH(width);
					GUILayout.Space(10);

					GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);

					GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
					guiStyleBox.alignment = TextAnchor.MiddleCenter;
					guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

					apModifierBase linkedModifier = AnimTimeline._linkedModifier;


					string boxText = "";
					bool isMod_Morph = ((int)(linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0);
					bool isMod_TF = ((int)(linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.TransformMatrix) != 0);
					bool isMod_Color = ((int)(linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0);

					if (isMod_Morph)
					{
						//boxText = "Morph Modifier Value";
						boxText = Editor.GetUIWord(UIWORD.MorphModifierValue);
					}
					else
					{
						//boxText = "Transform Modifier Value";
						boxText = Editor.GetUIWord(UIWORD.TransformModifierValue);
					}

					GUILayout.Box(boxText,
									guiStyleBox,
									GUILayout.Width(width), GUILayout.Height(30));

					GUI.backgroundColor = prevColor;

					//apModifierParamSet paramSet = keyframe._linkedParamSet_Editor;
					apModifiedMesh modMesh = keyframe._linkedModMesh_Editor;
					apModifiedBone modBone = keyframe._linkedModBone_Editor;
					if (modMesh == null)
					{
						isMod_Morph = false;
						isMod_Color = false;
					}
					if (modBone == null && modMesh == null)
					{
						//TF 타입은 Bone 타입이 적용될 수 있다.
						isMod_TF = false;
					}
					//TODO : 여기서부터 작성하자

					bool isChanged = false;

					if (isMod_Morph)
					{
						GUILayout.Space(5);
					}

					if (isMod_TF)
					{
						GUILayout.Space(5);

						Texture2D img_Pos = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Move);
						Texture2D img_Rot = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Rotate);
						Texture2D img_Scale = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Scale);

						Vector2 nextPos = Vector2.zero;
						float nextAngle = 0.0f;
						Vector2 nextScale = Vector2.one;

						if (modMesh != null)
						{
							nextPos = modMesh._transformMatrix._pos;
							nextAngle = modMesh._transformMatrix._angleDeg;
							nextScale = modMesh._transformMatrix._scale;
						}
						else if (modBone != null)
						{
							nextPos = modBone._transformMatrix._pos;
							nextAngle = modBone._transformMatrix._angleDeg;
							nextScale = modBone._transformMatrix._scale;
						}

						int iconSize = 30;
						int propertyWidth = width - (iconSize + 8);

						//Position
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
						EditorGUILayout.BeginVertical(GUILayout.Width(iconSize));
						EditorGUILayout.LabelField(new GUIContent(img_Pos), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginVertical(GUILayout.Width(propertyWidth), GUILayout.Height(iconSize));
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Position));//"Position"
						//nextPos = EditorGUILayout.Vector2Field("", nextPos, GUILayout.Width(propertyWidth));
						nextPos = apEditorUtil.DelayedVector2Field(nextPos, propertyWidth);
						EditorGUILayout.EndVertical();
						EditorGUILayout.EndHorizontal();

						//Rotation
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
						EditorGUILayout.BeginVertical(GUILayout.Width(iconSize));
						EditorGUILayout.LabelField(new GUIContent(img_Rot), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginVertical(GUILayout.Width(propertyWidth), GUILayout.Height(iconSize));
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Rotation));//"Rotation"

						nextAngle = EditorGUILayout.DelayedFloatField(nextAngle, GUILayout.Width(propertyWidth));
						EditorGUILayout.EndVertical();
						EditorGUILayout.EndHorizontal();

						//추가 : CW, CCW 옵션을 표시한다.
						int rotationEnumWidth = 80;
						int rotationValueWidth = (((width - 10) / 2) - rotationEnumWidth) - 4;
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
						GUILayout.Space(5);
						apAnimKeyframe.ROTATION_BIAS prevRotationBias = (apAnimKeyframe.ROTATION_BIAS)EditorGUILayout.EnumPopup(keyframe._prevRotationBiasMode, GUILayout.Width(rotationEnumWidth));
						//GUIStyle guiStyle_DisabledIntBox = new GUIStyle(GUI.skin.textField);
						int prevRotationBiasCount = EditorGUILayout.IntField(keyframe._prevRotationBiasCount, GUILayout.Width(rotationValueWidth));
						apAnimKeyframe.ROTATION_BIAS nextRotationBias = (apAnimKeyframe.ROTATION_BIAS)EditorGUILayout.EnumPopup(keyframe._nextRotationBiasMode, GUILayout.Width(rotationEnumWidth));
						int nextRotationBiasCount = EditorGUILayout.IntField(keyframe._nextRotationBiasCount, GUILayout.Width(rotationValueWidth));
						EditorGUILayout.EndHorizontal();

						

						//Scaling
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
						EditorGUILayout.BeginVertical(GUILayout.Width(iconSize));
						EditorGUILayout.LabelField(new GUIContent(img_Scale), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginVertical(GUILayout.Width(propertyWidth), GUILayout.Height(iconSize));
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Scaling));//"Scaling"

						//nextScale = EditorGUILayout.Vector2Field("", nextScale, GUILayout.Width(propertyWidth));
						nextScale = apEditorUtil.DelayedVector2Field(nextScale, propertyWidth);
						EditorGUILayout.EndVertical();
						EditorGUILayout.EndHorizontal();

						GUILayout.Space(10);

						


						if (modMesh != null)
						{
							if (nextPos.x != modMesh._transformMatrix._pos.x ||
								nextPos.y != modMesh._transformMatrix._pos.y ||
								nextAngle != modMesh._transformMatrix._angleDeg ||
								nextScale.x != modMesh._transformMatrix._scale.x ||
								nextScale.y != modMesh._transformMatrix._scale.y
								)
							{
								isChanged = true;

								apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, linkedModifier, null, false);

								modMesh._transformMatrix.SetPos(nextPos);
								modMesh._transformMatrix.SetRotate(nextAngle);
								modMesh._transformMatrix.SetScale(nextScale);
								modMesh._transformMatrix.MakeMatrix();

								apEditorUtil.ReleaseGUIFocus();
							}
						}
						else if (modBone != null)
						{
							if (nextPos.x != modBone._transformMatrix._pos.x ||
								nextPos.y != modBone._transformMatrix._pos.y ||
								nextAngle != modBone._transformMatrix._angleDeg ||
								nextScale.x != modBone._transformMatrix._scale.x ||
								nextScale.y != modBone._transformMatrix._scale.y)
							{
								isChanged = true;

								apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, linkedModifier, null, false);

								modBone._transformMatrix.SetPos(nextPos);
								modBone._transformMatrix.SetRotate(nextAngle);
								modBone._transformMatrix.SetScale(nextScale);
								modBone._transformMatrix.MakeMatrix();

								apEditorUtil.ReleaseGUIFocus();
							}
						}

						if (prevRotationBias != keyframe._prevRotationBiasMode ||
							prevRotationBiasCount != keyframe._prevRotationBiasCount ||
							nextRotationBias != keyframe._nextRotationBiasMode ||
							nextRotationBiasCount != keyframe._nextRotationBiasCount)
						{
							isChanged = true;

							apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, Editor._portrait, null, false);

							if(prevRotationBiasCount < 0) { prevRotationBiasCount = 0; }
							if(nextRotationBiasCount < 0) { nextRotationBiasCount = 0; }

							keyframe._prevRotationBiasMode = prevRotationBias;
							keyframe._prevRotationBiasCount = prevRotationBiasCount;
							keyframe._nextRotationBiasMode = nextRotationBias;
							keyframe._nextRotationBiasCount = nextRotationBiasCount;

							
						}

					}

					if (isMod_Color)
					{
						GUILayout.Space(5);

						if (linkedModifier._isColorPropertyEnabled)
						{
							Texture2D img_Color = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Color);

							Color nextColor = modMesh._meshColor;
							bool isMeshVisible = modMesh._isVisible;

							int iconSize = 30;
							int propertyWidth = width - (iconSize + 8);

							//Color
							EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
							EditorGUILayout.BeginVertical(GUILayout.Width(iconSize));
							EditorGUILayout.LabelField(new GUIContent(img_Color), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
							EditorGUILayout.EndVertical();

							EditorGUILayout.BeginVertical(GUILayout.Width(propertyWidth), GUILayout.Height(iconSize));
							EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Color2X));//"Color (2X)"
							try
							{
								nextColor = EditorGUILayout.ColorField("", modMesh._meshColor, GUILayout.Width(propertyWidth));
							}
							catch (Exception)
							{

							}

							EditorGUILayout.EndVertical();
							EditorGUILayout.EndHorizontal();


							//Visible
							EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
							GUILayout.Space(5);
							EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IsVisible) + " : ", GUILayout.Width(propertyWidth));//"Is Visible..
							isMeshVisible = EditorGUILayout.Toggle(isMeshVisible, GUILayout.Width(iconSize));
							EditorGUILayout.EndHorizontal();



							if (nextColor.r != modMesh._meshColor.r ||
								nextColor.g != modMesh._meshColor.g ||
								nextColor.b != modMesh._meshColor.b ||
								nextColor.a != modMesh._meshColor.a ||
								isMeshVisible != modMesh._isVisible)
							{
								isChanged = true;

								apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, linkedModifier, null, false);

								modMesh._meshColor = nextColor;
								modMesh._isVisible = isMeshVisible;

								//apEditorUtil.ReleaseGUIFocus();
							}
						}
						else
						{
							GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);

							//"Color Property is disabled"
							GUILayout.Box(Editor.GetUIWord(UIWORD.ColorPropertyIsDisabled),
											guiStyleBox,
											GUILayout.Width(width), GUILayout.Height(25));

							GUI.backgroundColor = prevColor;
						}
					}


					if (isChanged)
					{
						AnimClip.UpdateControlParam(true);
					}
				}
			}
			else
			{
				//2. Curve Mode
				//1) Prev 커브를 선택할 것인지, Next 커브를 선택할 것인지 결정해야한다.
				//2) 양쪽의 컨트롤 포인트의 설정을 결정한다. (Linear / Smooth / Constant(Stepped))
				//3) 커브 GUI

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));

				GUILayout.Space(5);

				int curveTypeBtnSize = 30;
				int curveBtnSize = (width - (curveTypeBtnSize * 3 + 2 * 5)) / 2 - 6;

				apAnimCurve curveA = null;
				apAnimCurve curveB = null;
				apAnimCurveResult curveResult = null;

				string strPrevKey = "";
				string strNextKey = "";

				Color colorLabel_Prev = Color.black;
				Color colorLabel_Next = Color.black;

				if (_animPropertyCurveUI == ANIM_SINGLE_PROPERTY_CURVE_UI.Prev)
				{
					curveA = keyframe._curveKey._prevLinkedCurveKey;
					curveB = keyframe._curveKey;
					curveResult = keyframe._curveKey._prevCurveResult;

					if (keyframe._prevLinkedKeyframe != null)
					{
						//strPrevKey = "Prev [" + keyframe._prevLinkedKeyframe._frameIndex + "]";
						strPrevKey = string.Format("{0} [{1}]", Editor.GetUIWord(UIWORD.Prev), keyframe._prevLinkedKeyframe._frameIndex);
					}
					//strNextKey = "Current [" + keyframe._frameIndex + "]";
					strNextKey = string.Format("{0} [{1}]", Editor.GetUIWord(UIWORD.Current), keyframe._frameIndex);
					colorLabel_Next = Color.red;
				}
				else
				{
					curveA = keyframe._curveKey;
					curveB = keyframe._curveKey._nextLinkedCurveKey;
					curveResult = keyframe._curveKey._nextCurveResult;


					//strPrevKey = "Current [" + keyframe._frameIndex + "]";
					strNextKey = string.Format("{0} [{1}]", Editor.GetUIWord(UIWORD.Current), keyframe._frameIndex);

					colorLabel_Prev = Color.red;
					if (keyframe._nextLinkedKeyframe != null)
					{
						//strNextKey = "Next [" + keyframe._nextLinkedKeyframe._frameIndex + "]";
						strPrevKey = string.Format("{0} [{1}]", Editor.GetUIWord(UIWORD.Next), keyframe._nextLinkedKeyframe._frameIndex);
					}

				}



				if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.Prev), _animPropertyCurveUI == ANIM_SINGLE_PROPERTY_CURVE_UI.Prev, curveBtnSize, 30))//"Prev"
				{
					_animPropertyCurveUI = ANIM_SINGLE_PROPERTY_CURVE_UI.Prev;
				}
				if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.Next), _animPropertyCurveUI == ANIM_SINGLE_PROPERTY_CURVE_UI.Next, curveBtnSize, 30))//"Next"
				{
					_animPropertyCurveUI = ANIM_SINGLE_PROPERTY_CURVE_UI.Next;
				}
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Curve_Linear), curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Linear, true, curveTypeBtnSize, 30,
												"Linear Curve"))
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, _portrait, curveResult, false);
					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Linear);
				}
				if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Curve_Smooth), curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Smooth, true, curveTypeBtnSize, 30,
												"Smooth Curve"))
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, _portrait, curveResult, false);
					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Smooth);
				}
				if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Curve_Stepped), curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Constant, true, curveTypeBtnSize, 30,
												"Constant Curve"))
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, _portrait, curveResult, false);
					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Constant);
				}



				EditorGUILayout.EndHorizontal();
				GUILayout.Space(5);

				if (isSameKP)
				{

					if (curveA == null || curveB == null)
					{
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.KeyframeIsNotLinked));//"Keyframe is not linked"
					}
					else
					{

						int curveUI_Width = width - 1;
						int curveUI_Height = 200;
						prevColor = GUI.backgroundColor;

						Rect lastRect = GUILayoutUtility.GetLastRect();

						if(EditorGUIUtility.isProSkin)
						{
							GUI.backgroundColor = new Color(	Editor._guiMainEditorColor.r * 0.8f,
													Editor._guiMainEditorColor.g * 0.8f,
													Editor._guiMainEditorColor.b * 0.8f,
													1.0f);
						}
						else
						{
							GUI.backgroundColor = Editor._guiMainEditorColor;
						}
						
						Rect curveRect = new Rect(lastRect.x + 5, lastRect.y, curveUI_Width, curveUI_Height);

						curveUI_Width -= 2;
						curveUI_Height -= 4;

						//int layoutY_Clip = layoutY - Mathf.Min(scrollValue, 115);
						//int layoutY_Clip = layoutY - Mathf.Clamp(scrollValue, 0, 115);
						//int layoutY_Clip = (layoutY - (scrollValue + (115 - scrollValue));//scrollValue > 115
						//int clipPosY = 115 - scrollValue;
						

						//Debug.Log("Lyout Y / layoutY : " + layoutY + " / scrollValue : " + scrollValue + " => " + layoutY_Clip);
						apAnimCurveGL.SetLayoutSize(
							curveUI_Width,
							curveUI_Height,
							(int)(lastRect.x) + layoutX - (curveUI_Width + 10),
							//(int)(lastRect.y) + layoutY_Clip,
							(int)(lastRect.y) + layoutY,
							scrollValue,
							Mathf.Min(scrollValue, 115),
							windowWidth, windowHeight);

						bool isLeftBtnPressed = false;
						if (Event.current.rawType == EventType.MouseDown ||
							Event.current.rawType == EventType.MouseDrag)
						{
							if (Event.current.button == 0)
							{ isLeftBtnPressed = true; }
						}

						apAnimCurveGL.SetMouseValue(isLeftBtnPressed, apMouse.PosNotBound, Event.current.rawType, this);

						GUI.Box(curveRect, "", apEditorUtil.WhiteGUIStyle_Box);
						//EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(curveUI_Height));
						GUILayout.BeginArea(new Rect(lastRect.x + 8, lastRect.y + 124, curveUI_Width - 2, curveUI_Height - 2));

						Color curveGraphColorA = Color.black;
						Color curveGraphColorB = Color.black;

						if (curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Linear)
						{
							curveGraphColorA = new Color(1.0f, 0.1f, 0.1f, 1.0f);
							curveGraphColorB = new Color(1.0f, 1.0f, 0.1f, 1.0f);
						}
						else if (curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Smooth)
						{
							curveGraphColorA = new Color(0.2f, 0.2f, 1.0f, 1.0f);
							curveGraphColorB = new Color(0.2f, 1.0f, 1.0f, 1.0f);
						}
						else
						{
							curveGraphColorA = new Color(0.2f, 1.0f, 0.1f, 1.0f);
							curveGraphColorB = new Color(0.1f, 1.0f, 0.6f, 1.0f);
						}


						apAnimCurveGL.DrawCurve(curveA, curveB, curveResult, curveGraphColorA, curveGraphColorB);


						GUILayout.EndArea();
						//EditorGUILayout.EndVertical();



						//GUILayout.Space(10);

						GUI.backgroundColor = prevColor;


						GUILayout.Space(curveUI_Height - 2);


						EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));

						GUIStyle guiStyle_FrameLabel_Prev = new GUIStyle(GUI.skin.label);
						GUIStyle guiStyle_FrameLabel_Next = new GUIStyle(GUI.skin.label);
						guiStyle_FrameLabel_Next.alignment = TextAnchor.MiddleRight;

						guiStyle_FrameLabel_Prev.normal.textColor = colorLabel_Prev;
						guiStyle_FrameLabel_Next.normal.textColor = colorLabel_Next;

						GUILayout.Space(5);
						EditorGUILayout.LabelField(strPrevKey, guiStyle_FrameLabel_Prev, GUILayout.Width(width / 2 - 4));

						EditorGUILayout.LabelField(strNextKey, guiStyle_FrameLabel_Next, GUILayout.Width(width / 2 - 4));
						EditorGUILayout.EndHorizontal();

						if (curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Smooth)
						{
							GUILayout.Space(5);

							EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
							GUILayout.Space(5);

							int smoothPresetBtnWidth = ((width - 10) / 4) - 1;
							if(GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_CurvePreset_Default), GUILayout.Width(smoothPresetBtnWidth), GUILayout.Height(28)))
							{
								//커브 프리셋 : 기본
								apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, _portrait, _animClip._targetMeshGroup, false);
								curveResult.SetCurvePreset_Default();
							}
							if(GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_CurvePreset_Hard), GUILayout.Width(smoothPresetBtnWidth), GUILayout.Height(28)))
							{
								//커브 프리셋 : 하드
								apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, _portrait, _animClip._targetMeshGroup, false);
								curveResult.SetCurvePreset_Hard();
							}
							if(GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_CurvePreset_Acc), GUILayout.Width(smoothPresetBtnWidth), GUILayout.Height(28)))
							{
								//커브 프리셋 : 가속 (느리다가 빠르게)
								apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, _portrait, _animClip._targetMeshGroup, false);
								curveResult.SetCurvePreset_Acc();
							}
							if(GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_CurvePreset_Dec), GUILayout.Width(smoothPresetBtnWidth), GUILayout.Height(28)))
							{
								//커브 프리셋 : 감속 (빠르다가 느리게)
								apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, _portrait, _animClip._targetMeshGroup, false);
								curveResult.SetCurvePreset_Dec();
							}

							EditorGUILayout.EndHorizontal();

							if (GUILayout.Button(Editor.GetUIWord(UIWORD.ResetSmoothSetting), GUILayout.Width(width), GUILayout.Height(25)))//"Reset Smooth Setting"
							{
								//Curve는 Anim 고유의 값이다. -> Portrait
								apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, _portrait, _animClip._targetMeshGroup, false);
								curveResult.ResetSmoothSetting();

								Editor.SetRepaint();
								//Editor.Repaint();
							}
						}
						GUILayout.Space(5);
						if (GUILayout.Button(Editor.GetUIWord(UIWORD.CopyCurveToAllKeyframes), GUILayout.Width(width), GUILayout.Height(25)))//"Copy Curve to All Keyframes"
						{

							Editor.Controller.CopyAnimCurveToAllKeyframes(curveResult, keyframe._parentTimelineLayer, keyframe._parentTimelineLayer._parentAnimClip);
							Editor.SetRepaint();
						}


					}
				}
			}



			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			if (isSameKP)
			{
				//복사 / 붙여넣기 / 삭제 // (복붙은 모든 타입에서 등장한다)
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
				GUILayout.Space(5);
				//int editBtnWidth = ((width) / 2) - 3;
				int editBtnWidth_Copy = 80;
				int editBtnWidth_Paste = width - (80 + 4);
				//if (GUILayout.Button(new GUIContent(" Copy", Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy)), GUILayout.Width(editBtnWidth), GUILayout.Height(25)))

				string strCopy = " " + Editor.GetUIWord(UIWORD.Copy);

				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy), strCopy, strCopy, false, true, editBtnWidth_Copy, 25))//" Copy"
				{
					//Debug.LogError("TODO : Copy Keyframe");
					if (keyframe != null)
					{
						string copyName = "";
						if (keyframe._parentTimelineLayer != null)
						{
							copyName += keyframe._parentTimelineLayer.DisplayName + " ";
						}
						copyName += "[ " + keyframe._frameIndex + " ]";
						apSnapShotManager.I.Copy_Keyframe(keyframe, copyName);
					}
				}

				string pasteKeyName = apSnapShotManager.I.GetClipboardName_Keyframe();
				bool isPastable = apSnapShotManager.I.IsPastable(keyframe);
				if (string.IsNullOrEmpty(pasteKeyName) || !isPastable)
				{
					//pasteKeyName = "Paste";
					pasteKeyName = Editor.GetUIWord(UIWORD.Paste);
				}
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Paste), " " + pasteKeyName, " " + pasteKeyName, false, isPastable, editBtnWidth_Paste, 25))
				{
					if (keyframe != null)
					{
						//붙여넣기
						//Anim (portrait) + Keyframe+LinkedMod (Modifier = nullable)
						apEditorUtil.SetRecord_PortraitModifier(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, _portrait, keyframe._parentTimelineLayer._parentTimeline._linkedModifier, null, false);
						apSnapShotManager.I.Paste_Keyframe(keyframe);
						RefreshAnimEditing(true);
					}
				}
				EditorGUILayout.EndHorizontal();

				
				//Pose Export / Import
				if(keyframe._parentTimelineLayer._linkedBone != null
					&& keyframe._parentTimelineLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier
					)
				{
					//Bone 타입인 경우
					//Pose 복사 / 붙여넣기를 할 수 있다.

					GUILayout.Space(5);
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.PoseExportImportLabel), GUILayout.Width(width));//"Pose Export / Import"

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
					GUILayout.Space(5);

					string strExport = " " + Editor.GetUIWord(UIWORD.Export);
					string strImport = " " + Editor.GetUIWord(UIWORD.Import);

					if(apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_SaveLoad), strExport, strExport, false, true, ((width) / 2) - 2, 25))
					{
						if (keyframe._parentTimelineLayer._parentAnimClip._targetMeshGroup != null)
						{
							apDialog_RetargetSinglePoseExport.ShowDialog(Editor, keyframe._parentTimelineLayer._parentAnimClip._targetMeshGroup, keyframe._parentTimelineLayer._linkedBone);
						}
					}
					if(apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_LoadBones), strImport, strImport, false, true, ((width) / 2) - 2, 25))
					{
						if (keyframe._parentTimelineLayer._parentAnimClip._targetMeshGroup != null)
						{
							_loadKey_SinglePoseImport_Anim = apDialog_RetargetSinglePoseImport.ShowDialog(
								OnRetargetSinglePoseImportAnim, Editor,
								keyframe._parentTimelineLayer._parentAnimClip._targetMeshGroup,
								keyframe._parentTimelineLayer._parentAnimClip,
								keyframe._parentTimelineLayer._parentTimeline,
								keyframe._parentTimelineLayer._parentAnimClip.CurFrame
								);
						}
					}
					EditorGUILayout.EndHorizontal();
				}


				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);


				//삭제 단축키 이벤트를 넣자
				Editor.AddHotKeyEvent(OnHotKeyRemoveKeyframes, "Remove Keyframe", KeyCode.Delete, false, false, false, keyframe);

				//키 삭제
				//"Remove Keyframe"
				if (GUILayout.Button(	new GUIContent(	"  " + Editor.GetUIWord(UIWORD.RemoveKeyframe),
													Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
													),
									GUILayout.Width(width), GUILayout.Height(24)))
				{

					bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveKeyframe1_Title),
																Editor.GetText(TEXT.RemoveKeyframe1_Body),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);
					if (isResult)
					{
						Editor.Controller.RemoveKeyframe(keyframe);
					}
				}

				
			}

			
		}



		

		private void OnRetargetSinglePoseImportAnim(object loadKey, bool isSuccess, apRetarget resultRetarget,
																apMeshGroup targetMeshGroup,
																apAnimClip targetAnimClip, 
																apAnimTimeline targetTimeline, int targetFrame)
		{
			if(loadKey != _loadKey_SinglePoseImport_Anim || !isSuccess)
			{
				_loadKey_SinglePoseImport_Anim = null;
				return;
			}

			_loadKey_SinglePoseImport_Anim = null;
			
			//Pose Import 처리를 하자
			Editor.Controller.ImportBonePoseFromRetargetSinglePoseFileToAnimClip(targetMeshGroup, resultRetarget, targetAnimClip, targetTimeline, targetFrame);

		}




		private void DrawEditor_Bottom_AnimationProperty_MultipleKeyframes(List<apAnimKeyframe> keyframes, int width)
		{
			//keyframes.Count + " Keyframes Selected"
			EditorGUILayout.LabelField(Editor.GetUIWordFormat(UIWORD.RemoveNumKeyframes, keyframes.Count));

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//삭제 단축키 이벤트를 넣자
			Editor.AddHotKeyEvent(OnHotKeyRemoveKeyframes, Editor.GetUIWord(UIWORD.RemoveKeyframes), KeyCode.Delete, false, false, false, keyframes);//"Remove Keyframes"


			//키 삭제
			//"  Remove " + keyframes.Count +" Keyframes"
			if (GUILayout.Button(	new GUIContent(	"  " + Editor.GetUIWordFormat(UIWORD.RemoveNumKeyframes, keyframes.Count),
													Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
													),
									GUILayout.Width(width), GUILayout.Height(24)))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Keyframes", "Remove " + keyframes.Count + "s Keyframes?", "Remove", "Cancel");

				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveKeyframes_Title),
																Editor.GetTextFormat(TEXT.RemoveKeyframes_Body, keyframes.Count),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);

				if (isResult)
				{
					Editor.Controller.RemoveKeyframes(keyframes);
				}
			}
		}

		private void DrawEditor_Bottom_AnimationProperty_TimelineLayer(apAnimTimelineLayer timelineLayer, int width)
		{
			//EditorGUILayout.LabelField("Timeline Layer");
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.TimelineLayer));

			GUILayout.Space(10);
			if (timelineLayer._targetParamSetGroup != null &&
				timelineLayer._parentTimeline != null &&
				timelineLayer._parentTimeline._linkedModifier != null
				)
			{
				apModifierParamSetGroup keyParamSetGroup = timelineLayer._targetParamSetGroup;
				apModifierBase modifier = timelineLayer._parentTimeline._linkedModifier;
				//apAnimTimeline timeline = timelineLayer._parentTimeline;

				//이름
				//설정
				Color prevColor = GUI.backgroundColor;

				GUI.backgroundColor = timelineLayer._guiColor;
				GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
				guiStyle_Box.alignment = TextAnchor.MiddleCenter;
				guiStyle_Box.normal.textColor = apEditorUtil.BoxTextColor;

				GUILayout.Box(timelineLayer.DisplayName, guiStyle_Box, GUILayout.Width(width), GUILayout.Height(30));

				GUI.backgroundColor = prevColor;

				GUILayout.Space(10);

				//1. 색상 Modifier라면 색상 옵션을 설정한다.
				if ((int)(modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
				{
					//" Color Option On", " Color Option Off",
					if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ColorVisibleOption),
															" " + Editor.GetUIWord(UIWORD.ColorOptionOn), " " + Editor.GetUIWord(UIWORD.ColorOptionOff),
															keyParamSetGroup._isColorPropertyEnabled, true,
															width, 24))
					{
						apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, modifier, _animClip._targetMeshGroup, false);
						keyParamSetGroup._isColorPropertyEnabled = !keyParamSetGroup._isColorPropertyEnabled;

						_animClip._targetMeshGroup.RefreshForce();
						Editor.RefreshControllerAndHierarchy();
					}

					GUILayout.Space(10);
				}
			}

			//2. GUI Color를 설정
			try
			{
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.LayerGUIColor));//"Layer GUI Color"
				Color nextGUIColor = EditorGUILayout.ColorField(timelineLayer._guiColor, GUILayout.Width(width));
				if (nextGUIColor != timelineLayer._guiColor)
				{
					apEditorUtil.SetEditorDirty();
					timelineLayer._guiColor = nextGUIColor;
				}
			}
			catch (Exception) { }

			GUILayout.Space(10);

			//Pose Export / Import
			if (timelineLayer._linkedBone != null
				&& timelineLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier
				)
			{
				//Bone 타입인 경우
				//Pose 복사 / 붙여넣기를 할 수 있다.

				GUILayout.Space(5);
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.PoseExportImportLabel), GUILayout.Width(width));//"Pose Export / Import"

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
				GUILayout.Space(5);

				string strExport = " " + Editor.GetUIWord(UIWORD.Export);
				string strImport = " " + Editor.GetUIWord(UIWORD.Import);

				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_SaveLoad), strExport, strExport, false, true, ((width) / 2) - 2, 25))
				{
					if (timelineLayer._parentAnimClip._targetMeshGroup != null)
					{
						apDialog_RetargetSinglePoseExport.ShowDialog(Editor, timelineLayer._parentAnimClip._targetMeshGroup, timelineLayer._linkedBone);
					}
				}
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_LoadBones), strImport, strImport, false, true, ((width) / 2) - 2, 25))
				{
					if (timelineLayer._parentAnimClip._targetMeshGroup != null)
					{
						_loadKey_SinglePoseImport_Anim = apDialog_RetargetSinglePoseImport.ShowDialog(
							OnRetargetSinglePoseImportAnim, Editor,
							timelineLayer._parentAnimClip._targetMeshGroup,
							timelineLayer._parentAnimClip,
							timelineLayer._parentTimeline,
							timelineLayer._parentAnimClip.CurFrame
							);
					}
				}
				EditorGUILayout.EndHorizontal();
			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);
		}

		private void DrawEditor_Bottom_AnimationProperty_Timeline(apAnimTimeline timeline, int width)
		{
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Timeline));//"Timeline"

			GUILayout.Space(10);

			if (timeline._linkedModifier != null
				)
			{
				apModifierBase modifier = timeline._linkedModifier;


				//이름
				//설정
				Color prevColor = GUI.backgroundColor;

				GUI.backgroundColor = timeline._guiColor;
				GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
				guiStyle_Box.alignment = TextAnchor.MiddleCenter;
				guiStyle_Box.normal.textColor = apEditorUtil.BoxTextColor;

				GUILayout.Box(timeline.DisplayName, guiStyle_Box, GUILayout.Width(width), GUILayout.Height(30));

				GUI.backgroundColor = prevColor;

				GUILayout.Space(10);

				//" Color Option On", " Color Option Off",
				//1. 색상 Modifier라면 색상 옵션을 설정한다.
				if ((int)(modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
				{
					if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ColorVisibleOption),
															" " + Editor.GetUIWord(UIWORD.ColorOptionOn), " " + Editor.GetUIWord(UIWORD.ColorOptionOff),
															modifier._isColorPropertyEnabled, true,
															width, 24))
					{
						apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, modifier, _animClip._targetMeshGroup, false);

						modifier._isColorPropertyEnabled = !modifier._isColorPropertyEnabled;
						_animClip._targetMeshGroup.RefreshForce();
						Editor.RefreshControllerAndHierarchy();
					}
				}
				GUILayout.Space(10);

			}


			//Pose Export / Import
			if (timeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier
				&& timeline._linkedModifier != null
				&& timeline._linkedModifier.IsTarget_Bone)
			{
				//Bone 타입인 경우
				//Pose 복사 / 붙여넣기를 할 수 있다.

				GUILayout.Space(5);
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.PoseExportImportLabel), GUILayout.Width(width));//"Pose Export / Import"

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
				GUILayout.Space(5);

				string strExport = " " + Editor.GetUIWord(UIWORD.Export);
				string strImport = " " + Editor.GetUIWord(UIWORD.Import);

				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_SaveLoad), strExport, strExport, false, true, ((width) / 2) - 2, 25))// " Export"
				{
					if (timeline._parentAnimClip._targetMeshGroup != null)
					{
						apDialog_RetargetSinglePoseExport.ShowDialog(Editor, timeline._parentAnimClip._targetMeshGroup, null);
					}
				}
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_LoadBones), strImport, strImport, false, true, ((width) / 2) - 2, 25))//" Import"
				{
					if (timeline._parentAnimClip._targetMeshGroup != null)
					{
						_loadKey_SinglePoseImport_Anim = apDialog_RetargetSinglePoseImport.ShowDialog(
							OnRetargetSinglePoseImportAnim, Editor,
							timeline._parentAnimClip._targetMeshGroup,
							timeline._parentAnimClip,
							timeline,
							timeline._parentAnimClip.CurFrame
							);
					}
				}
				EditorGUILayout.EndHorizontal();
			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);
		}



		private void OnHotKeyRemoveKeyframes(object paramObject)
		{
			if(SelectionType != SELECTION_TYPE.Animation ||
				AnimClip == null)
			{
				return;
			}

			if(paramObject is apAnimKeyframe)
			{
				apAnimKeyframe keyframe = paramObject as apAnimKeyframe;

				if(keyframe != null)
				{
					Editor.Controller.RemoveKeyframe(keyframe);
				}
			}
			else if(paramObject is List<apAnimKeyframe>)
			{
				List<apAnimKeyframe> keyframes = paramObject as List<apAnimKeyframe>;
				if (keyframes != null && keyframes.Count > 0)
				{
					Editor.Controller.RemoveKeyframes(keyframes);
				}
			}
		}


		//------------------------------------------------------------------------------------
		private void DrawEditor_Right2_MeshGroupRight_Setting(int width, int height)
		{
			bool isMeshTransform = false;
			bool isValidSelect = false;

			if (SubMeshInGroup != null)
			{
				if (SubMeshInGroup._mesh != null)
				{
					isMeshTransform = true;
					isValidSelect = true;
				}
			}
			else if (SubMeshGroupInGroup != null)
			{
				if (SubMeshGroupInGroup._meshGroup != null)
				{
					isMeshTransform = false;
					isValidSelect = true;
				}
			}

			//if (isValidSelect)
			//{
			//	//1-1. 선택된 객체가 존재하여 [객체 정보]를 출력할 수 있다.
			//	Editor.SetGUIVisible("MeshGroupBottom_Setting", true);
			//}
			//else
			//{
			//	//1-2. 선택된 객체가 없어서 우측 상세 정보 UI를 출력하지 않는다.
			//	//수정 -> 기본 루트 MeshGroupTransform을 출력한다.
			//	Editor.SetGUIVisible("MeshGroupBottom_Setting", false);

			//	return; //바로 리턴
			//}

			////2. 출력할 정보가 있다 하더라도
			////=> 바로 출력 가능한게 아니라 경우에 따라 Hide 상태를 조금 더 유지할 필요가 있다.
			//if (!Editor.IsDelayedGUIVisible("MeshGroupBottom_Setting"))
			//{
			//	//아직 출력하면 안된다.
			//	return;
			//}

			Editor.SetGUIVisible("MeshGroupRight_Setting_ObjectSelected", isValidSelect);
			Editor.SetGUIVisible("MeshGroupRight_Setting_ObjectNotSelected", !isValidSelect);

			bool isSelectedObjectRender = Editor.IsDelayedGUIVisible("MeshGroupRight_Setting_ObjectSelected");
			bool isNotSelectedObjectRender = Editor.IsDelayedGUIVisible("MeshGroupRight_Setting_ObjectNotSelected");

			if (!isSelectedObjectRender && !isNotSelectedObjectRender)
			{
				return;
			}

			//1. 오브젝트가 선택이 되었다.
			if (isSelectedObjectRender)
			{
				string objectName = "";
				string strType = "";
				string prevNickName = "";
				bool isSocket = false;
				if (isMeshTransform)
				{
					strType = Editor.GetUIWord(UIWORD.Mesh);//"Sub Mesh" -> "Mesh"
					objectName = SubMeshInGroup._mesh._name;
					prevNickName = SubMeshInGroup._nickName;
					isSocket = SubMeshInGroup._isSocket;
				}
				else
				{
					strType = Editor.GetUIWord(UIWORD.MeshGroup);//"Sub Mesh Group" -> "Mesh Group"
					objectName = SubMeshGroupInGroup._meshGroup._name;
					prevNickName = SubMeshGroupInGroup._nickName;
					isSocket = SubMeshGroupInGroup._isSocket;
				}
				//EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));

				//1. 아이콘 / 타입
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(50));
				GUILayout.Space(10);
				if (isMeshTransform)
				{
					EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh)), GUILayout.Width(50), GUILayout.Height(50));
				}
				else
				{
					EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup)), GUILayout.Width(50), GUILayout.Height(50));
				}
				EditorGUILayout.BeginVertical(GUILayout.Width(width - (50 + 10)));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(strType, GUILayout.Width(width - (50 + 10)));
				EditorGUILayout.LabelField(objectName, GUILayout.Width(width - (50 + 10)));


				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);

				//2. 닉네임
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Name), GUILayout.Width(80));//"Name"
				string nextNickName = EditorGUILayout.DelayedTextField(prevNickName, GUILayout.Width(width));
				if (!string.Equals(nextNickName, prevNickName))
				{
					
					if (isMeshTransform)
					{
						apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, Editor, _meshGroup, SubMeshInGroup, false, true);
						SubMeshInGroup._nickName = nextNickName;
					}
					else
					{
						apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, Editor, _meshGroup, SubMeshGroupInGroup, false, true);
						SubMeshGroupInGroup._nickName = nextNickName;
					}

					Editor.RefreshControllerAndHierarchy();
				}
				

				GUILayout.Space(10);

				//"Socket Enabled", "Socket Disabled"
				if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.SocketEnabled), Editor.GetUIWord(UIWORD.SocketDisabled), isSocket, true, width, 25))
				{
					if (isMeshTransform)
					{
						apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, Editor, _meshGroup, SubMeshInGroup, false, true);
						SubMeshInGroup._isSocket = !SubMeshInGroup._isSocket;
					}
					else
					{
						apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, Editor, _meshGroup, SubMeshGroupInGroup, false, true);
						SubMeshGroupInGroup._isSocket = !SubMeshGroupInGroup._isSocket;
					}
				}

				GUILayout.Space(10);

				apEditorUtil.GUI_DelimeterBoxH(width - 10);
				GUILayout.Space(10);

				Editor.SetGUIVisible("Render Unit Detail Status - MeshTransform", isMeshTransform);
				Editor.SetGUIVisible("Render Unit Detail Status - MeshGroupTransform", !isMeshTransform);

				bool isMeshTransformDetailRendererable = Editor.IsDelayedGUIVisible("Render Unit Detail Status - MeshTransform");
				bool isMeshGroupTransformDetailRendererable = Editor.IsDelayedGUIVisible("Render Unit Detail Status - MeshGroupTransform");

				//3. Mesh Transform Setting
				if (isMeshTransform && isMeshTransformDetailRendererable)
				{
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ShaderSetting));//"Shader Setting"
					SubMeshInGroup._shaderType = (apPortrait.SHADER_TYPE)EditorGUILayout.EnumPopup(SubMeshInGroup._shaderType);
					GUILayout.Space(5);
					SubMeshInGroup._isCustomShader = EditorGUILayout.Toggle(Editor.GetUIWord(UIWORD.UseCustomShader), SubMeshInGroup._isCustomShader);//"Use Custom Shader"
					if (SubMeshInGroup._isCustomShader)
					{
						GUILayout.Space(5);
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.CustomShader));//"Custom Shader"
						SubMeshInGroup._customShader = (Shader)EditorGUILayout.ObjectField(SubMeshInGroup._customShader, typeof(Shader), false);
					}
					GUILayout.Space(20);

					GUIStyle guiStyle_ClipStatus = new GUIStyle(GUI.skin.box);
					guiStyle_ClipStatus.alignment = TextAnchor.MiddleCenter;
					guiStyle_ClipStatus.normal.textColor = apEditorUtil.BoxTextColor;

					Editor.SetGUIVisible("Mesh Transform Detail Status - Clipping Child", SubMeshInGroup._isClipping_Child);
					Editor.SetGUIVisible("Mesh Transform Detail Status - Clipping Parent", SubMeshInGroup._isClipping_Parent);
					Editor.SetGUIVisible("Mesh Transform Detail Status - Clipping None", (!SubMeshInGroup._isClipping_Parent && !SubMeshInGroup._isClipping_Child));

					if (SubMeshInGroup._isClipping_Parent)
					{
						if (Editor.IsDelayedGUIVisible("Mesh Transform Detail Status - Clipping Parent"))
						{
							//1. 자식 메시를 가지는 Clipping의 Base Parent이다.
							//- Mask 사이즈를 보여준다.
							//- 자식 메시 리스트들을 보여준다.
							//-> 레이어 순서를 바꾼다. / Clip을 해제한다..

							//"Parent Mask Mesh"
							GUILayout.Box(Editor.GetUIWord(UIWORD.ParentMaskMesh), guiStyle_ClipStatus, GUILayout.Width(width), GUILayout.Height(25));
							GUILayout.Space(5);

							EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.MaskTextureSize), GUILayout.Width(width));//"Mask Texture Size"
							int prevRTTIndex = (int)SubMeshInGroup._renderTexSize;
							if(prevRTTIndex < 0 || prevRTTIndex >= apEditorUtil.GetRenderTextureSizeNames().Length)
							{
								prevRTTIndex = (int)(apTransform_Mesh.RENDER_TEXTURE_SIZE.s_256);
							}
							int nextRTTIndex = EditorGUILayout.Popup(prevRTTIndex, apEditorUtil.GetRenderTextureSizeNames(), GUILayout.Width(width));
							if(nextRTTIndex != prevRTTIndex)
							{
								apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_ClippingChanged, Editor, MeshGroup, null, false, true);
								SubMeshInGroup._renderTexSize = (apTransform_Mesh.RENDER_TEXTURE_SIZE)nextRTTIndex;
							}


							GUILayout.Space(5);
							

							//Texture2D btnImg_Down = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerDown);
							//Texture2D btnImg_Up = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerUp);
							Texture2D btnImg_Delete = Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey);

							int iBtn = -1;
							//int btnRequestType = -1;
							

							for (int iChild = 0; iChild < SubMeshInGroup._clipChildMeshes.Count; iChild++)
							{
								apTransform_Mesh childMesh = SubMeshInGroup._clipChildMeshes[iChild]._meshTransform;
								EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
								if (childMesh != null)
								{
									EditorGUILayout.LabelField("[" + iChild + "] " + childMesh._nickName, GUILayout.Width(width - (20 + 5)), GUILayout.Height(20));
									if (GUILayout.Button(btnImg_Delete, GUILayout.Width(20), GUILayout.Height(20)))
									{
										iBtn = iChild;
										//btnRequestType = 2;//2 : Delete
										
									}
								}
								else
								{
									EditorGUILayout.LabelField("[" + iChild + "] <Empty>", GUILayout.Width(width), GUILayout.Height(20));
								}
								EditorGUILayout.EndHorizontal();
							}


							if (iBtn >= 0)
							{
								//Debug.LogError("TODO : Mesh 삭제");
								apTransform_Mesh targetChildTransform = SubMeshInGroup._clipChildMeshes[iBtn]._meshTransform;
								if(targetChildTransform != null)
								{
									//해당 ChildMesh를 Release하자
									Editor.Controller.ReleaseClippingMeshTransform(MeshGroup, targetChildTransform);
								}
							}
						}
					}
					else if (SubMeshInGroup._isClipping_Child)
					{


						if (Editor.IsDelayedGUIVisible("Mesh Transform Detail Status - Clipping Child"))
						{
							//2. Parent를 Mask로 삼는 자식 Mesh이다.
							//- 부모 메시를 보여준다.
							//-> 순서 바꾸기를 요청한다
							//-> Clip을 해제한다.
							//"Child Clipped Mesh" ->"Clipped Child Mesh"
							GUILayout.Box(Editor.GetUIWord(UIWORD.ClippedChildMesh), guiStyle_ClipStatus, GUILayout.Width(width), GUILayout.Height(25));
							GUILayout.Space(5);

							string strParentName = "<No Mask Parent>";
							if (SubMeshInGroup._clipParentMeshTransform != null)
							{
								strParentName = SubMeshInGroup._clipParentMeshTransform._nickName;
							}

							//"Mask Parent" -> "Mask Mesh"
							EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.MaskMesh) + " : " + strParentName, GUILayout.Width(width));
							EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ClippedIndex) + " : " + SubMeshInGroup._clipIndexFromParent, GUILayout.Width(width));//"Clipped Index : "
							EditorGUILayout.BeginHorizontal(GUILayout.Width(width));

							//int btnRequestType = -1;
							//"Release"
							if (GUILayout.Button(Editor.GetUIWord(UIWORD.Release), GUILayout.Width(width), GUILayout.Height(25)))
							{
								//btnRequestType = 2;//2 : Delete
								Editor.Controller.ReleaseClippingMeshTransform(MeshGroup, SubMeshInGroup);
							}
							EditorGUILayout.EndHorizontal();


						}
					}
					else
					{
						//3. 기본 상태의 Mesh이다.
						//Clip을 요청한다.
						//"Clipping To Below Mesh" -> "Clip to Below Mesh"
						if (GUILayout.Button(Editor.GetUIWord(UIWORD.ClipToBelowMesh), GUILayout.Width(width), GUILayout.Height(25)))
						{
							Editor.Controller.AddClippingMeshTransform(MeshGroup, SubMeshInGroup, true);
						}
					}
				}
				else if (!isMeshTransform && isMeshGroupTransformDetailRendererable)
				{

				}

				if (isMeshTransformDetailRendererable || isMeshGroupTransformDetailRendererable)
				{
					GUILayout.Space(20);
					//4. Detach

					apEditorUtil.GUI_DelimeterBoxH(width - 10);
					GUILayout.Space(10);

					if (GUILayout.Button(Editor.GetUIWord(UIWORD.Detach) + " [" + strType + "]"))//"Detach [" + strType + "]"
					{
						string strDialogInfo = Editor.GetText(TEXT.Detach_Body);
						if (isMeshTransform)
						{
							strDialogInfo = Editor.Controller.GetRemoveItemMessage(
																_portrait,
																SubMeshInGroup,
																5,
																Editor.GetText(TEXT.Detach_Body),
																Editor.GetText(TEXT.DLG_RemoveItemChangedWarning));
						}
						else
						{
							strDialogInfo = Editor.Controller.GetRemoveItemMessage(
																_portrait,
																SubMeshGroupInGroup,
																5,
																Editor.GetText(TEXT.Detach_Body),
																Editor.GetText(TEXT.DLG_RemoveItemChangedWarning));
						}

						//bool isResult = EditorUtility.DisplayDialog("Detach", "Detach it?", "Detach", "Cancel");
						bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.Detach_Title),
																		//Editor.GetText(TEXT.Detach_Body),
																		strDialogInfo,
																		Editor.GetText(TEXT.Detach_Ok),
																		Editor.GetText(TEXT.Cancel)
																		);
						if (isResult)
						{
							if (isMeshTransform)
							{
								Editor.Controller.DetachMeshInMeshGroup(SubMeshInGroup, MeshGroup);
								Editor.Select.SetSubMeshInGroup(null);
							}
							else
							{
								Editor.Controller.DetachMeshGroupInMeshGroup(SubMeshGroupInGroup, MeshGroup);
								Editor.Select.SetSubMeshGroupInGroup(null);
							}
						}
						MeshGroup.SetDirtyToSort();//TODO : Sort에서 자식 객체 변한것 체크 : Clip 그룹 체크
						MeshGroup.RefreshForce();
						Editor.SetRepaint();
					}
					//EditorGUILayout.EndVertical();
				}
			}
			else if (isNotSelectedObjectRender)
			{
				//2. 오브젝트가 선택이 안되었다.
				//기본 정보를 출력하고, 루트 MeshGroupTransform의 Transform 값을 설정한다.
				apTransform_MeshGroup rootMeshGroupTransform = MeshGroup._rootMeshGroupTransform;

				//1. 아이콘 / 타입
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(50));
				GUILayout.Space(10);
				EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup)), GUILayout.Width(50), GUILayout.Height(50));

				EditorGUILayout.BeginVertical(GUILayout.Width(width - (50 + 10)));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.MeshGroup), GUILayout.Width(width - (50 + 12)));//"Mesh Group"
				EditorGUILayout.LabelField(MeshGroup._name, GUILayout.Width(width - (50 + 12)));

				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);
				apEditorUtil.GUI_DelimeterBoxH(width - 10);
				GUILayout.Space(10);


				EditorGUILayout.BeginVertical(GUILayout.Width(width));


				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.RootTransform));//"Root Transform"
				
				Texture2D img_Pos = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Move);
				Texture2D img_Rot = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Rotate);
				Texture2D img_Scale = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Scale);

				int iconSize = 30;
				int propertyWidth = width - (iconSize + 12);

				//Position
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
				EditorGUILayout.BeginVertical(GUILayout.Width(iconSize));
				EditorGUILayout.LabelField(new GUIContent(img_Pos), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(GUILayout.Width(propertyWidth), GUILayout.Height(iconSize));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Position), GUILayout.Width(propertyWidth));//"Position"
				//nextPos = EditorGUILayout.Vector2Field("", nextPos, GUILayout.Width(propertyWidth));
				Vector2 rootPos = apEditorUtil.DelayedVector2Field(rootMeshGroupTransform._matrix._pos, propertyWidth);
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				//Rotation
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
				EditorGUILayout.BeginVertical(GUILayout.Width(iconSize));
				EditorGUILayout.LabelField(new GUIContent(img_Rot), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(GUILayout.Width(propertyWidth), GUILayout.Height(iconSize));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Rotation), GUILayout.Width(propertyWidth));//"Rotation"

				float rootAngle = EditorGUILayout.DelayedFloatField(rootMeshGroupTransform._matrix._angleDeg, GUILayout.Width(propertyWidth));
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				//Scaling
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
				EditorGUILayout.BeginVertical(GUILayout.Width(iconSize));
				EditorGUILayout.LabelField(new GUIContent(img_Scale), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(GUILayout.Width(propertyWidth), GUILayout.Height(iconSize));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Scaling), GUILayout.Width(propertyWidth));//"Scaling"

				//nextScale = EditorGUILayout.Vector2Field("", nextScale, GUILayout.Width(propertyWidth));
				Vector2 rootScale = apEditorUtil.DelayedVector2Field(rootMeshGroupTransform._matrix._scale, propertyWidth);
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.EndVertical();

				//테스트용
				//rootMeshGroupTransform._isVisible_Default = EditorGUILayout.Toggle("Is Visible", rootMeshGroupTransform._isVisible_Default, GUILayout.Width(width));
				//EditorGUILayout.ColorField("Color2x", rootMeshGroupTransform._meshColor2X_Default);


				if (rootPos != rootMeshGroupTransform._matrix._pos
					|| rootAngle != rootMeshGroupTransform._matrix._angleDeg
					|| rootScale != rootMeshGroupTransform._matrix._scale)
				{
					apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, Editor, MeshGroup, MeshGroup, false, true);

					rootMeshGroupTransform._matrix.SetTRS(rootPos.x, rootPos.y, rootAngle, rootScale.x, rootScale.y);
					MeshGroup.RefreshForce();
					apEditorUtil.ReleaseGUIFocus();
				}
			}
		}

		//private string _prevName_BoneProperty = "";
		private apBone _prevBone_BoneProperty = null;
		private int _prevChildBoneCount = 0;
		private void DrawEditor_Right2_MeshGroup_Bone(int width, int height)
		{
			//int subWidth = 250;
			apBone curBone = Bone;

			bool isRefresh = false;
			bool isAnyGUIAction = false;

			//bool isChildBoneChanged = false;

			bool isBoneChanged = (_prevBone_BoneProperty != curBone);
			//if (curBone != null)
			//{
			//	isChildBoneChanged = (_prevChildBoneCount != curBone._childBones.Count);
			//}


			if (_prevBone_BoneProperty != curBone)
			{
				_prevBone_BoneProperty = curBone;
				if (curBone != null)
				{
					//_prevName_BoneProperty = curBone._name;
					_prevChildBoneCount = curBone._childBones.Count;
				}
				else
				{
					//_prevName_BoneProperty = "";
					_prevChildBoneCount = 0;
				}

				Editor.SetGUIVisible("Update Child Bones", false);
			}

			if (curBone != null)
			{
				if (_prevChildBoneCount != curBone._childBones.Count)
				{
					Editor.SetGUIVisible("Update Child Bones", true);
					if (Editor.IsDelayedGUIVisible("Update Child Bones"))
					{
						//Debug.Log("Child Bone Count Changed : " + _prevChildBoneCount + " -> " + curBone._childBones.Count);
						_prevChildBoneCount = curBone._childBones.Count;
					}
				}
			}

			Editor.SetGUIVisible("MeshGroupRight2 Bone", curBone != null
				&& !isBoneChanged
				//&& !isChildBoneChanged
				);
			Editor.SetGUIVisible("MeshGroup Bone - Child Bone Drawable", true);
			if (!Editor.IsDelayedGUIVisible("MeshGroupRight2 Bone")
				//|| !Editor.IsDelayedGUIVisible("MeshGroup Bone - Child Bone Drawable")
				)
			{
				return;
			}



			//1. 아이콘 / 타입
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(50));
			GUILayout.Space(10);

			//모디파이어 아이콘
			EditorGUILayout.LabelField(
				new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)),
				GUILayout.Width(50), GUILayout.Height(50));

			int nameWidth = width - (50 + 10);
			EditorGUILayout.BeginVertical(GUILayout.Width(nameWidth));
			GUILayout.Space(5);

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Bone), GUILayout.Width(nameWidth));//"Bone"

			string nextBoneName = EditorGUILayout.DelayedTextField(curBone._name, GUILayout.Width(nameWidth));
			if (!string.Equals(nextBoneName, curBone._name))
			{
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, Editor, curBone._meshGroup, curBone, false, false);

				curBone._name = nextBoneName;
				isRefresh = true;
				isAnyGUIAction = true;
			}
			#region [미사용 코드] DelayedTextField 사용 전 코드
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(nameWidth));
			//_prevName_BoneProperty = EditorGUILayout.TextField(_prevName_BoneProperty, GUILayout.Width(nameWidth - 62));
			//if (GUILayout.Button("Change", GUILayout.Width(60)))
			//{
			//	curBone._name = _prevName_BoneProperty;
			//	isRefresh = true;
			//	isAnyGUIAction = true;
			//}

			//EditorGUILayout.EndHorizontal(); 
			#endregion

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(20);

			//Default Matrix 설정
			Vector2 defPos = curBone._defaultMatrix._pos;
			float defAngle = curBone._defaultMatrix._angleDeg;
			Vector2 defScale = curBone._defaultMatrix._scale;

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.BasePoseTransformation), GUILayout.Width(width));//"Base Pose Transformation"
			int widthValue = width - 80;

			if (!IsBoneDefaultEditing)
			{
				//여기서는 보여주기만
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Position), GUILayout.Width(70));//"Position"
				EditorGUILayout.LabelField(defPos.x + ", " + defPos.y, GUILayout.Width(widthValue));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Rotation), GUILayout.Width(70));//"Rotation"
				EditorGUILayout.LabelField(defAngle + "", GUILayout.Width(widthValue));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Scaling), GUILayout.Width(70));//"Scaling"
				EditorGUILayout.LabelField(defScale.x + ", " + defScale.y, GUILayout.Width(widthValue));
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				//여기서는 설정이 가능하다

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Position), GUILayout.Width(70));//"Position"
				defPos = apEditorUtil.DelayedVector2Field(defPos, widthValue);

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Rotation), GUILayout.Width(70));//"Rotation"
				defAngle = EditorGUILayout.DelayedFloatField(defAngle, GUILayout.Width(widthValue + 4));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Scaling), GUILayout.Width(70));//"Scaling"
				defScale = apEditorUtil.DelayedVector2Field(defScale, widthValue);

				EditorGUILayout.EndHorizontal();

				if (defPos != curBone._defaultMatrix._pos ||
					defAngle != curBone._defaultMatrix._angleDeg ||
					defScale != curBone._defaultMatrix._scale)
				{
					apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, Editor, curBone._meshGroup, curBone, false, false);

					defAngle = apUtil.AngleTo180(defAngle);

					curBone._defaultMatrix.SetPos(defPos);
					curBone._defaultMatrix.SetRotate(defAngle);
					curBone._defaultMatrix.SetScale(defScale);

					curBone.MakeWorldMatrix(true);
					//isRefresh = true;
					isAnyGUIAction = true;
				}
			}
			GUILayout.Space(10);
			//"Socket Enabled", "Socket Disabled"
			if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.SocketEnabled), Editor.GetUIWord(UIWORD.SocketDisabled), curBone._isSocketEnabled, true, width, 25))
			{
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, Editor, curBone._meshGroup, curBone, false, false);
				curBone._isSocketEnabled = !curBone._isSocketEnabled;
			}

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//IK 설정
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IKSetting), GUILayout.Width(width));//"IK Setting"

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(40));
			int IKModeBtnSize = (width / 4) - 4;
			//EditorGUILayout.LabelField("IK Option", GUILayout.Width(70));
			GUILayout.Space(5);
			apBone.OPTION_IK nextOptionIK = curBone._optionIK;

			//apBone.OPTION_IK nextOptionIK = (apBone.OPTION_IK)EditorGUILayout.EnumPopup(curBone._optionIK, GUILayout.Width(widthValue));

			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_IKSingle), curBone._optionIK == apBone.OPTION_IK.IKSingle, true, IKModeBtnSize, 40, "IK Single"))
			{
				nextOptionIK = apBone.OPTION_IK.IKSingle;
				isAnyGUIAction = true;
			}
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_IKHead), curBone._optionIK == apBone.OPTION_IK.IKHead, true, IKModeBtnSize, 40, "IK Head"))
			{
				nextOptionIK = apBone.OPTION_IK.IKHead;
				isAnyGUIAction = true;
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_IKChained), curBone._optionIK == apBone.OPTION_IK.IKChained, curBone._optionIK == apBone.OPTION_IK.IKChained, IKModeBtnSize, 40, "IK Chain"))
			{
				//nextOptionIK = apBone.OPTION_IK.IKSingle;//Chained는 직접 설정할 수 있는게 아니다.
				isAnyGUIAction = true;
			}
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_IKDisabled), curBone._optionIK == apBone.OPTION_IK.Disabled, true, IKModeBtnSize, 40, "Disabled"))
			{
				nextOptionIK = apBone.OPTION_IK.Disabled;
				isAnyGUIAction = true;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			string strIKInfo = "";
			Color prevColor = GUI.backgroundColor;

			Color boxColor = Color.black;
			switch (curBone._optionIK)
			{
				case apBone.OPTION_IK.IKSingle:
					//strIKInfo = "[IK Single]\nIK is applied to One Child Bone";
					strIKInfo = Editor.GetUIWord(UIWORD.IKInfo_Single);
					boxColor = new Color(1.0f, 0.6f, 0.5f, 1.0f);
					break;

				case apBone.OPTION_IK.IKHead:
					//strIKInfo = "[IK Head]\nIK is applied to Chained Bones";
					strIKInfo = Editor.GetUIWord(UIWORD.IKInfo_Head);
					boxColor = new Color(1.0f, 0.5f, 0.6f, 1.0f);
					break;

				case apBone.OPTION_IK.IKChained:
					//strIKInfo = "[IK Chain]\nLocated in the middle of IK Chain";
					strIKInfo = Editor.GetUIWord(UIWORD.IKInfo_Chain);
					boxColor = new Color(0.7f, 0.5f, 1.0f, 1.0f);
					break;

				case apBone.OPTION_IK.Disabled:
					//strIKInfo = "[Disabled]\nIK is not applied";
					strIKInfo = Editor.GetUIWord(UIWORD.IKInfo_Disabled);
					boxColor = new Color(0.6f, 0.8f, 1.0f, 1.0f);
					break;
			}
			GUI.backgroundColor = boxColor;
			GUIStyle guiStyleInfoBox = new GUIStyle(GUI.skin.box);
			guiStyleInfoBox.alignment = TextAnchor.MiddleCenter;
			guiStyleInfoBox.normal.textColor = apEditorUtil.BoxTextColor;

			GUILayout.Box(strIKInfo, guiStyleInfoBox, GUILayout.Width(width), GUILayout.Height(40));

			GUI.backgroundColor = prevColor;

			GUILayout.Space(10);


			if (nextOptionIK != curBone._optionIK)
			{
				//Debug.Log("IK Change : " + curBone._optionIK + " > " + nextOptionIK);

				bool isIKOptionChangeValid = false;

				
				//이제 IK 옵션에 맞는지 체크해주자
				if (curBone._optionIK == apBone.OPTION_IK.IKChained)
				{
					//Chained 상태에서는 아예 바꿀 수 없다.
					//EditorUtility.DisplayDialog("IK Option Information",
					//	"<IK Chained> setting has been forced.\nTo Change, change the IK setting in the <IK Header>.",
					//	"Close");

					EditorUtility.DisplayDialog(Editor.GetText(TEXT.IKOption_Title),
													Editor.GetText(TEXT.IKOption_Body_Chained),
													Editor.GetText(TEXT.Close));
				}
				else
				{
					apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, Editor, curBone._meshGroup, curBone, false, false);

					//그외에는 변경이 가능하다
					switch (nextOptionIK)
					{
						case apBone.OPTION_IK.Disabled:
							//끄는 건 쉽다.
							isIKOptionChangeValid = true;
							break;

						case apBone.OPTION_IK.IKChained:
							//IK Chained는 직접 할 수 있는게 아니다.
							//EditorUtility.DisplayDialog("IK Option Information",
							//"<IK Chained> setting is set automatically.\nTo change, change the setting in the <IK Header>.",
							//"Close");

							EditorUtility.DisplayDialog(Editor.GetText(TEXT.IKOption_Title),
												Editor.GetText(TEXT.IKOption_Body_Chained),
												Editor.GetText(TEXT.Close));
							break;

						case apBone.OPTION_IK.IKHead:
							{
								//자식으로 연결된게 없으면 일단 바로 아래 자식을 연결하자.
								//자식이 없으면 실패

								apBone nextChainedBone = curBone._IKNextChainedBone;
								apBone targetBone = curBone._IKTargetBone;

								bool isRefreshNeed = true;
								if (nextChainedBone != null && targetBone != null)
								{
									//이전에 연결된 값이 존재하고, 재귀적인 연결도 유효한 경우는 패스
									if (curBone.GetChildBone(nextChainedBone._uniqueID) != null
										&& curBone.GetChildBoneRecursive(targetBone._uniqueID) != null)
									{
										//유효한 설정이다.
										isRefreshNeed = false;
									}
								}

								if (isRefreshNeed)
								{
									//자식 Bone의 하나를 연결하자
									if (curBone._childBones.Count > 0)
									{
										curBone._IKNextChainedBone = curBone._childBones[0];
										curBone._IKTargetBone = curBone._childBones[0];

										curBone._IKNextChainedBoneID = curBone._IKNextChainedBone._uniqueID;
										curBone._IKTargetBoneID = curBone._IKTargetBone._uniqueID;

										isIKOptionChangeValid = true;//기본값을 넣어서 변경 가능
									}
									else
									{
										//EditorUtility.DisplayDialog("IK Option Information",
										//"<IK Head> setting requires one or more child Bones.",
										//"Close");

										EditorUtility.DisplayDialog(Editor.GetText(TEXT.IKOption_Title),
													Editor.GetText(TEXT.IKOption_Body_Head),
													Editor.GetText(TEXT.Close));
									}
								}
								else
								{
									isIKOptionChangeValid = true;
								}
							}
							break;

						case apBone.OPTION_IK.IKSingle:
							{
								//IK Target과 NextChained가 다르면 일단 그것부터 같게 하자.
								//나머지는 Head와 동일
								curBone._IKTargetBone = curBone._IKNextChainedBone;
								curBone._IKTargetBoneID = curBone._IKNextChainedBoneID;

								apBone nextChainedBone = curBone._IKNextChainedBone;

								bool isRefreshNeed = true;
								if (nextChainedBone != null)
								{
									//이전에 연결된 값이 존재하고, 재귀적인 연결도 유효한 경우는 패스
									if (curBone.GetChildBone(nextChainedBone._uniqueID) != null)
									{
										//유효한 설정이다.
										isRefreshNeed = false;
									}
								}

								if (isRefreshNeed)
								{
									//자식 Bone의 하나를 연결하자
									if (curBone._childBones.Count > 0)
									{
										curBone._IKNextChainedBone = curBone._childBones[0];
										curBone._IKTargetBone = curBone._childBones[0];

										curBone._IKNextChainedBoneID = curBone._IKNextChainedBone._uniqueID;
										curBone._IKTargetBoneID = curBone._IKTargetBone._uniqueID;

										isIKOptionChangeValid = true;//기본값을 넣어서 변경 가능
									}
									else
									{
										//EditorUtility.DisplayDialog("IK Option Information",
										//"<IK Single> setting requires a child Bone.",
										//"Close");

										EditorUtility.DisplayDialog(Editor.GetText(TEXT.IKOption_Title),
													Editor.GetText(TEXT.IKOption_Body_Single),
													Editor.GetText(TEXT.Close));
									}
								}
								else
								{
									isIKOptionChangeValid = true;
								}
							}
							break;
					}
				}



				if (isIKOptionChangeValid)
				{
					curBone._optionIK = nextOptionIK;

					isRefresh = true;
				}
				//TODO : 너무 자동으로 Bone Chain을 하는것 같다;
				//옵션이 적용이 안된다;
			}



			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IKHeader), GUILayout.Width(width));//"IK Header"
			string headerBoneName = "<None>";
			if (curBone._IKHeaderBone != null)
			{
				headerBoneName = curBone._IKHeaderBone._name;
			}
			EditorGUILayout.LabelField(new GUIContent(" " + headerBoneName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width));

			GUILayout.Space(5);
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IKNextChainToTarget), GUILayout.Width(width));//"IK Next Chain To Target"
			string nextChainedBoneName = "<None>";
			if (curBone._IKNextChainedBone != null)
			{
				nextChainedBoneName = curBone._IKNextChainedBone._name;
			}
			EditorGUILayout.LabelField(new GUIContent(" " + nextChainedBoneName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width));
			GUILayout.Space(5);


			if (curBone._optionIK != apBone.OPTION_IK.Disabled)
			{
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IKTarget), GUILayout.Width(width));//"IK Target"

				apBone targetBone = curBone._IKTargetBone;

				string targetBoneName = "<None>";

				if (targetBone != null)
				{
					targetBoneName = targetBone._name;
				}

				EditorGUILayout.LabelField(new GUIContent(" " + targetBoneName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width));



				//Target을 설정하자.
				if (curBone._optionIK == apBone.OPTION_IK.IKHead)
				{
					//"Change IK Target"
					if (GUILayout.Button(Editor.GetUIWord(UIWORD.ChangeIKTarget), GUILayout.Width(width), GUILayout.Height(20)))
					{
						//Debug.LogError("TODO : IK Target을 Dialog를 열어서 설정하자.");
						_loadKey_SelectBone = apDialog_SelectLinkedBone.ShowDialog(Editor, curBone, curBone._meshGroup, apDialog_SelectLinkedBone.REQUEST_TYPE.SelectIKTarget, OnDialogSelectBone);
						isAnyGUIAction = true;
					}
				}



				GUILayout.Space(15);
				//"IK Angle Constraint"
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IKAngleConstraint), GUILayout.Width(width));

				//"Constraint On", "Constraint Off"
				if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.ConstraintOn), Editor.GetUIWord(UIWORD.ConstraintOff), curBone._isIKAngleRange, true, width, 25))
				{
					apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, Editor, curBone._meshGroup, curBone, false, false);

					curBone._isIKAngleRange = !curBone._isIKAngleRange;
					isAnyGUIAction = true;
				}
				
				if (curBone._isIKAngleRange)
				{
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Range), GUILayout.Width(70));//"Range"
					
					//변경전 Lower : -180 ~ 0, Uppder : 0 ~ 180
					//변경후 Lower : -360 ~ 360, Upper : -360 ~ 360 (크기만 맞춘다.)
					float nextLowerAngle = curBone._IKAngleRange_Lower;
					float nextUpperAngle = curBone._IKAngleRange_Upper;
					//EditorGUILayout.MinMaxSlider(ref nextLowerAngle, ref nextUpperAngle, -360, 360, GUILayout.Width(widthValue));
					EditorGUILayout.MinMaxSlider(ref nextLowerAngle, ref nextUpperAngle, -360, 360, GUILayout.Width(width));

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Min), GUILayout.Width(70));//"Min"
					nextLowerAngle = EditorGUILayout.FloatField(nextLowerAngle, GUILayout.Width(widthValue));
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Max), GUILayout.Width(70));//"Max"
					nextUpperAngle = EditorGUILayout.FloatField(nextUpperAngle, GUILayout.Width(widthValue));
					EditorGUILayout.EndHorizontal();

					//EditorGUILayout.EndHorizontal();

					
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Preferred), GUILayout.Width(70));//"Preferred"
					float nextPreferredAngle = EditorGUILayout.Slider(curBone._IKAnglePreferred, -360, 360, GUILayout.Width(widthValue));
					EditorGUILayout.EndHorizontal();

					if (nextLowerAngle != curBone._IKAngleRange_Lower ||
						nextUpperAngle != curBone._IKAngleRange_Upper ||
						nextPreferredAngle != curBone._IKAnglePreferred)
					{

						//apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, curBone._meshGroup, null, false, Editor);
						apEditorUtil.SetEditorDirty();

						curBone._IKAngleRange_Lower = nextLowerAngle;
						curBone._IKAngleRange_Upper = nextUpperAngle;
						curBone._IKAnglePreferred = nextPreferredAngle;
						//isRefresh = true;
						isAnyGUIAction = true;
					}
				}
			}

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//Hierarchy 설정
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Hierarchy), GUILayout.Width(width));//"Hierarchy"
			//Parent와 Child List를 보여주자.
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ParentBone), GUILayout.Width(width));//"Parent Bone"
			string parentName = "<None>";
			if (curBone._parentBone != null)
			{
				parentName = curBone._parentBone._name;
			}
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(new GUIContent(" " + parentName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width - 60));
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.Change), GUILayout.Width(58)))//"Change"
			{
				//Debug.LogError("TODO : Change Parent Dialog 구현할 것");
				isAnyGUIAction = true;
				_loadKey_SelectBone = apDialog_SelectLinkedBone.ShowDialog(Editor, curBone, curBone._meshGroup, apDialog_SelectLinkedBone.REQUEST_TYPE.ChangeParent, OnDialogSelectBone);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			int nChildList = curBone._childBones.Count;
			if (_prevChildBoneCount != nChildList)
			{
				Debug.Log("AnyPortrait : Count is not matched : " + _prevChildBoneCount + " > " + nChildList);
			}
			//"Children Bones"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ChildrenBones) + " [" + nChildList + "]", GUILayout.Width(width));

			//Detach가 
			apBone detachedBone = null;

			for (int iChild = 0; iChild < _prevChildBoneCount; iChild++)
			{
				if (iChild >= nChildList)
				{
					//리스트를 벗어났다.
					//더미 Layout을 그리자
					//유니티 레이아웃 처리방식때문..
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField("", GUILayout.Width(width - 60));
					if (GUILayout.Button(Editor.GetUIWord(UIWORD.Detach), GUILayout.Width(58)))//"Detach"
					{

					}
					EditorGUILayout.EndHorizontal();
				}
				else
				{
					apBone childBone = curBone._childBones[iChild];
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField(new GUIContent(" " + childBone._name, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width - 60));
					if (GUILayout.Button(Editor.GetUIWord(UIWORD.Detach), GUILayout.Width(58)))//"Detach"
					{
						//Debug.LogError("TODO : Change Parent Dialog 구현할 것");
						//bool isResult = EditorUtility.DisplayDialog("Detach Child Bone", "Detach Bone? [" + childBone._name + "]", "Detach", "Cancel")
						bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.DetachChildBone_Title),
																		Editor.GetTextFormat(TEXT.DetachChildBone_Body, childBone._name),
																		Editor.GetText(TEXT.Detach_Ok),
																		Editor.GetText(TEXT.Cancel)
																		);

						if (isResult)
						{
							//Debug.LogError("TODO : Detach Child Bone 구현할 것");
							//Detach Child Bone 선택
							detachedBone = childBone;
							isAnyGUIAction = true;
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.AttachChildBone), GUILayout.Width(width), GUILayout.Height(20)))//"Attach Child Bone"
			{
				isAnyGUIAction = true;
				_loadKey_SelectBone = apDialog_SelectLinkedBone.ShowDialog(Editor, curBone, curBone._meshGroup, apDialog_SelectLinkedBone.REQUEST_TYPE.AttachChild, OnDialogSelectBone);
			}
			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);


			//Shape 설정
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Shape), GUILayout.Width(width));//"Shape"

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Color), GUILayout.Width(70));//"Color"
			try
			{
				Color nextColor = EditorGUILayout.ColorField(curBone._color, GUILayout.Width(widthValue)); 
				if(nextColor != curBone._color)
				{
					apEditorUtil.SetEditorDirty();
					curBone._color = nextColor;
				}
			}
			catch (Exception) { }
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Width), GUILayout.Width(70));//"Width"
			int nextShapeWidth = EditorGUILayout.DelayedIntField(curBone._shapeWidth, GUILayout.Width(widthValue));
			if(nextShapeWidth != curBone._shapeWidth)
			{
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, Editor, curBone._meshGroup, curBone, false, false);
				curBone._shapeWidth = nextShapeWidth;

				//추가 : 다음 본을 생성시에 지금 값을 이용하도록 값을 저장하자
				_lastBoneShapeWidth = nextShapeWidth;
				_isLastBoneShapeWidthChanged = true;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Taper), GUILayout.Width(70));//"Taper"
			int nextShapeTaper = EditorGUILayout.DelayedIntField(curBone._shapeTaper, GUILayout.Width(widthValue));
			if(nextShapeTaper != curBone._shapeTaper)
			{
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, Editor, curBone._meshGroup, curBone, false, false);
				curBone._shapeTaper = nextShapeTaper;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Helper), GUILayout.Width(70));//"Helper"
			bool nextHelper = EditorGUILayout.Toggle(curBone._shapeHelper, GUILayout.Width(widthValue));
			if(nextHelper != curBone._shapeHelper)
			{
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, Editor, curBone._meshGroup, curBone, false, false);
				curBone._shapeHelper = nextHelper;
			}
			EditorGUILayout.EndHorizontal();




			//Detach 요청이 있으면 수행 후 Refresh를 하자
			if (detachedBone != null)
			{
				isAnyGUIAction = true;
				Editor.Controller.DetachBoneFromChild(curBone, detachedBone);
				Editor.SetGUIVisible("MeshGroup Bone - Child Bone Drawable", false);
				isRefresh = true;
			}


			if (isAnyGUIAction)
			{
				//여기서 뭔가 처리를 했으면 Select 모드로 강제된다.
				if (_boneEditMode != BONE_EDIT_MODE.SelectAndTRS)
				{
					SetBoneEditMode(BONE_EDIT_MODE.SelectAndTRS, true);
				}
			}

			if (isRefresh)
			{
				Editor.RefreshControllerAndHierarchy();
				Editor._portrait.LinkAndRefreshInEditor(false);
			}

			GUILayout.Space(20);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(20);
			
			//"Remove Bone"
			if (GUILayout.Button(	new GUIContent(	"  " + Editor.GetUIWord(UIWORD.RemoveBone),
													Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
													),
									GUILayout.Width(width), GUILayout.Height(24)))
			{
				isAnyGUIAction = true;
				SetBoneEditMode(BONE_EDIT_MODE.SelectAndTRS, true);
				

				string strRemoveBoneText = Editor.Controller.GetRemoveItemMessage(
																	_portrait,
																	curBone,
																	5,
																	Editor.GetTextFormat(TEXT.RemoveBone_Body, curBone._name),
																	Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																	);
				//int btnIndex = EditorUtility.DisplayDialogComplex("Remove Bone", "Remove Bone [" + curBone._name + "] ?", "Remove", "Remove All Child Bones", "Cancel");


				int btnIndex = EditorUtility.DisplayDialogComplex(
																	Editor.GetText(TEXT.RemoveBone_Title), 
																	strRemoveBoneText, 
																	Editor.GetText(TEXT.Remove), 
																	Editor.GetText(TEXT.RemoveBone_RemoveAllChildren), 
																	Editor.GetText(TEXT.Cancel));
				if (btnIndex == 0)
				{
					//Bone을 삭제한다.
					Editor.Controller.RemoveBone(curBone, false);
				}
				else if (btnIndex == 1)
				{
					//Bone과 자식을 모두 삭제한다.
					Editor.Controller.RemoveBone(curBone, true);
				}
			}

			
		}

		private object _loadKey_SelectBone = null;
		private void OnDialogSelectBone(bool isSuccess, object loadKey, bool isNullBone, apBone selectedBone, apBone targetBone, apDialog_SelectLinkedBone.REQUEST_TYPE requestType)
		{
			if (_loadKey_SelectBone != loadKey)
			{
				_loadKey_SelectBone = null;
				return;
			}
			if (!isSuccess)
			{
				_loadKey_SelectBone = null;
				return;
			}


			_loadKey_SelectBone = null;
			switch (requestType)
			{
				case apDialog_SelectLinkedBone.REQUEST_TYPE.AttachChild:
					{
						Editor.Controller.AttachBoneToChild(targetBone, selectedBone);
					}
					break;

				case apDialog_SelectLinkedBone.REQUEST_TYPE.ChangeParent:
					{
						Editor.Controller.SetBoneAsParent(targetBone, selectedBone);
					}
					break;

				case apDialog_SelectLinkedBone.REQUEST_TYPE.SelectIKTarget:
					{
						Editor.Controller.SetBoneAsIKTarget(targetBone, selectedBone);
					}
					break;
			}
		}


		private void DrawEditor_Right2_MeshGroup_Modifier(int width, int height)
		{
			if (Modifier != null)
			{
				//1-1. 선택된 객체가 존재하여 [객체 정보]를 출력할 수 있다.
				Editor.SetGUIVisible("MeshGroupBottom_Modifier", true);
			}
			else
			{
				//1-2. 선택된 객체가 없어서 하단 UI를 출력하지 않는다.
				Editor.SetGUIVisible("MeshGroupBottom_Modifier", false);

				return; //바로 리턴
			}

			//2. 출력할 정보가 있다 하더라도
			//=> 바로 출력 가능한게 아니라 경우에 따라 Hide 상태를 조금 더 유지할 필요가 있다.
			if (!Editor.IsDelayedGUIVisible("MeshGroupBottom_Modifier"))
			{
				//아직 출력하면 안된다.
				return;
			}
			//1. 아이콘 / 타입
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(50));
			GUILayout.Space(10);

			//모디파이어 아이콘
			EditorGUILayout.LabelField(
				new GUIContent(Editor.ImageSet.Get(apEditorUtil.GetModifierIconType(Modifier.ModifierType))),
				GUILayout.Width(50), GUILayout.Height(50));

			EditorGUILayout.BeginVertical(GUILayout.Width(width - (50 + 10)));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(Modifier.DisplayName, GUILayout.Width(width - (50 + 10)));
			//"Layer"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Layer) + " : " + Modifier._layer, GUILayout.Width(width - (50 + 10)));


			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			//추가
			//만약 색상 옵션이 있는 경우 설정을 하자
			if ((int)(Modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
			{
				//" Color Option On", " Color Option Off"
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ColorVisibleOption),
														" " + Editor.GetUIWord(UIWORD.ColorOptionOn),
														" " + Editor.GetUIWord(UIWORD.ColorOptionOff),
														Modifier._isColorPropertyEnabled, true,
														width, 24
													))
				{
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Modifier, Modifier, true);

					Modifier._isColorPropertyEnabled = !Modifier._isColorPropertyEnabled;
					Editor.RefreshControllerAndHierarchy();
				}
				GUILayout.Space(10);
			}


			//2. 기본 블렌딩 설정
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Blend), GUILayout.Width(width));//"Blend"

			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Method), GUILayout.Width(70));//"Method"
			apModifierBase.BLEND_METHOD blendMethod = (apModifierBase.BLEND_METHOD)EditorGUILayout.EnumPopup(Modifier._blendMethod, GUILayout.Width(width - (70 + 5)));
			if (blendMethod != Modifier._blendMethod)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Modifier, Modifier, true);
				Modifier._blendMethod = blendMethod;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Weight), GUILayout.Width(70));//"Weight"
			float layerWeight = EditorGUILayout.DelayedFloatField(Modifier._layerWeight, GUILayout.Width(width - (70 + 5)));

			layerWeight = Mathf.Clamp01(layerWeight);
			if (layerWeight != Modifier._layerWeight)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Modifier, Modifier, true);
				Modifier._layerWeight = layerWeight;
			}
			EditorGUILayout.EndHorizontal();

			//레이어 이동
			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.LayerUp), GUILayout.Width(width / 2 - 5)))//"Layer Up"
			{
				Editor.Controller.LayerChange(Modifier, true);
			}
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.LayerDown), GUILayout.Width(width / 2 - 5)))//"Layer Down"
			{
				Editor.Controller.LayerChange(Modifier, false);
			}
			EditorGUILayout.EndHorizontal();


			GUILayout.Space(10);

			//3. 각 프로퍼티 렌더링
			// 수정
			//일괄적으로 호출하자
			DrawModifierPropertyGUI(width, height);
			//switch (Modifier.ModifierType)
			//{
			//	case apModifierBase.MODIFIER_TYPE.Morph:
			//		MeshGroupBottomStatus_Modifier(width, height);
			//		break;

			//	case apModifierBase.MODIFIER_TYPE.Volume:
			//		MeshGroupBottomStatus_Modifier_Volume(width, height);
			//		break;

			//		//TODO : 새로운 모디파이어에 맞게 프로퍼티를 렌더링해야한다.

			//	default:
			//		GUILayout.Space(5);
			//		break;
			//}

			GUILayout.Space(20);


			//4. Modifier 삭제
			apEditorUtil.GUI_DelimeterBoxH(width - 10);
			GUILayout.Space(10);

			//"  Remove Modifier"
			if (GUILayout.Button(	new GUIContent(	"  " + Editor.GetUIWord(UIWORD.RemoveModifier),
													Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
													),
									GUILayout.Height(24)))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove", "Remove Modifier [" + Modifier.DisplayName + "]?", "Remove", "Cancel");


				string strRemoveModifierText = Editor.Controller.GetRemoveItemMessage(
																_portrait,
																Modifier,
																5,
																Editor.GetTextFormat(TEXT.RemoveModifier_Body, Modifier.DisplayName),
																Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																);

				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveModifier_Title),
																//Editor.GetTextFormat(TEXT.RemoveModifier_Body, Modifier.DisplayName),
																strRemoveModifierText,
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);

				if (isResult)
				{
					Editor.Controller.RemoveModifier(Modifier);
				}
			}


			//삭제 직후라면 출력 에러가 발생한다.
			if (Modifier == null)
			{
				return;
			}

		}

		private Vector2 _scrollBottom_Status = Vector2.zero;

		//private object _controlPramDialog_LoadKey = null;

		private void DrawModifierPropertyGUI(int width, int height)
		{
			if (Modifier != null)
			{
				string strRecordName = Modifier.DisplayName;


				if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					//Rigging UI를 작성
					DrawModifierPropertyGUI_Rigging(width, height, strRecordName);
				}
				else if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic)
				{
					//Physic UI를 작성
					DrawModifierPropertyGUI_Physics(width, height);
				}
				else
				{
					//그 외에는 ParamSetGroup에 따라서 UI를 구성하면 된다.
					switch (Modifier.SyncTarget)
					{
						case apModifierParamSetGroup.SYNC_TARGET.Bones:
							break;

						case apModifierParamSetGroup.SYNC_TARGET.Controller:
							{
								//Control Param 리스트
								apDialog_SelectControlParam.PARAM_TYPE paramFilter = apDialog_SelectControlParam.PARAM_TYPE.All;
								DrawModifierPropertyGUI_ControllerParamSet(width, height, paramFilter, strRecordName);
							}
							break;

						case apModifierParamSetGroup.SYNC_TARGET.ControllerWithoutKey:
							break;

						case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
							{
								//Keyframe 리스트
								DrawModifierPropertyGUI_KeyframeParamSet(width, height, strRecordName);
							}
							break;

						case apModifierParamSetGroup.SYNC_TARGET.Static:
							break;
					}
				}

			}


			GUILayout.Space(20);


		}


		#region [미사용 코드]
		//private void MeshGroupBottomStatus_Modifier_Volume(int width, int height)
		//{
		//	apDialog_SelectControlParam.PARAM_TYPE paramFilter =
		//	apDialog_SelectControlParam.PARAM_TYPE.Float |
		//			apDialog_SelectControlParam.PARAM_TYPE.Int |
		//			apDialog_SelectControlParam.PARAM_TYPE.Vector2 |
		//			apDialog_SelectControlParam.PARAM_TYPE.Vector3;

		//	DrawModifierPropertyGUI_ControllerParamSet(width, height, paramFilter, "Volume");

		//	GUILayout.Space(20);
		//} 
		#endregion

		private object _loadKey_SinglePoseImport_Mod = null;

		// Modifier 보조 함수들
		//------------------------------------------------------------------------------------
		private void DrawModifierPropertyGUI_ControllerParamSet(int width, int height, apDialog_SelectControlParam.PARAM_TYPE paramFilter, string recordName)
		{
			
			// SyncTarget으로 Control Param을 받아서 Modifier를 제어하는 경우
			GUIStyle guiNone = new GUIStyle(GUIStyle.none);
			guiNone.normal.textColor = GUI.skin.label.normal.textColor;

			GUIStyle guiSelected = new GUIStyle(GUIStyle.none);
			if(EditorGUIUtility.isProSkin)
			{
				guiSelected.normal.textColor = Color.cyan;
			}
			else
			{
				guiSelected.normal.textColor = Color.white;
			}
			


			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ControlParameters), GUILayout.Width(width));//"Control Parameters"

			GUILayout.Space(5);


			// 생성된 Morph Key (Parameter Group)를 선택하자
			//------------------------------------------------------------------
			// Control Param에 따른 Param Set Group 리스트
			//------------------------------------------------------------------
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(120));
			GUILayout.Space(5);

			Rect lastRect = GUILayoutUtility.GetLastRect();

			Color prevColor = GUI.backgroundColor;

			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);

			GUI.Box(new Rect(lastRect.x + 5, lastRect.y, width, 120), "");
			GUI.backgroundColor = prevColor;

			//처리 역순으로 보여준다.
			List<apModifierParamSetGroup> paramSetGroups = new List<apModifierParamSetGroup>();
			if (Modifier._paramSetGroup_controller.Count > 0)
			{
				for (int i = Modifier._paramSetGroup_controller.Count - 1; i >= 0; i--)
				{
					paramSetGroups.Add(Modifier._paramSetGroup_controller[i]);
				}
			}

			//등록된 Control Param Group 리스트를 출력하자
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(120));
			_scrollBottom_Status = EditorGUILayout.BeginScrollView(_scrollBottom_Status, false, true);
			GUILayout.Space(2);
			int scrollWidth = width - (30);
			EditorGUILayout.BeginVertical(GUILayout.Width(scrollWidth), GUILayout.Height(120));
			GUILayout.Space(3);

			Texture2D paramIconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param);
			Texture2D visibleIconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Current);
			Texture2D nonvisibleIconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Current);

			//현재 선택중인 파라미터 그룹
			apModifierParamSetGroup curParamSetGroup = SubEditedParamSetGroup;

			for (int i = 0; i < paramSetGroups.Count; i++)
			{
				GUIStyle curGUIStyle = guiNone;
				if (curParamSetGroup == paramSetGroups[i])
				{
					lastRect = GUILayoutUtility.GetLastRect();

					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					}


					int offsetHeight = 18 + 3;
					if (i == 0)
					{
						offsetHeight = 1 + 3;
					}

					GUI.Box(new Rect(lastRect.x, lastRect.y + offsetHeight, scrollWidth + 35, 20), "");
					GUI.backgroundColor = prevColor;

					curGUIStyle = guiSelected;
				}


				EditorGUILayout.BeginHorizontal(GUILayout.Width(scrollWidth - 5));
				GUILayout.Space(5);
				if (GUILayout.Button(new GUIContent(" " + paramSetGroups[i]._keyControlParam._keyName, paramIconImage),
									curGUIStyle,
									GUILayout.Width(scrollWidth - (5 + 25)), GUILayout.Height(20)))
				{
					//ParamSetGroup을 선택했다.
					SetParamSetGroupOfModifier(paramSetGroups[i]);
					AutoSelectParamSetOfModifier();//<자동 선택까지

					Editor.RefreshControllerAndHierarchy();
				}

				Texture2D imageVisible = visibleIconImage;

				if (!paramSetGroups[i]._isEnabled)
				{
					imageVisible = nonvisibleIconImage;
				}
				if (GUILayout.Button(imageVisible, guiNone, GUILayout.Width(20), GUILayout.Height(20)))
				{
					paramSetGroups[i]._isEnabled = !paramSetGroups[i]._isEnabled;
				}
				EditorGUILayout.EndHorizontal();
			}


			EditorGUILayout.EndVertical();

			GUILayout.Space(120);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			//------------------------------------------------------------------ < Param Set Group 리스트



			//-----------------------------------------------------------------------------------
			// Param Set Group 선택시 / 선택된 Param Set Group 정보와 포함된 Param Set 리스트
			//-----------------------------------------------------------------------------------



			GUILayout.Space(10);

			Editor.SetGUIVisible("CP Selected ParamSetGroup", (SubEditedParamSetGroup != null));

			if (!Editor.IsDelayedGUIVisible("CP Selected ParamSetGroup"))
			{
				return;
			}
			//ParamSetGroup에 레이어 옵션이 추가되었다.
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SetOfKeys));//"Parameters Setting" -> "Set of Keys"
			GUILayout.Space(2);
			//"Blend Method" -> "Blend"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Blend));
			apModifierParamSetGroup.BLEND_METHOD psgBlendMethod = (apModifierParamSetGroup.BLEND_METHOD)EditorGUILayout.EnumPopup(SubEditedParamSetGroup._blendMethod, GUILayout.Width(width));
			if (psgBlendMethod != SubEditedParamSetGroup._blendMethod)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Modifier, null, false);
				SubEditedParamSetGroup._blendMethod = psgBlendMethod;
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Weight), GUILayout.Width(80));//"Weight"
			float psgLayerWeight = EditorGUILayout.Slider(SubEditedParamSetGroup._layerWeight, 0.0f, 1.0f, GUILayout.Width(width - 85));
			if (psgLayerWeight != SubEditedParamSetGroup._layerWeight)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Modifier, null, false);
				SubEditedParamSetGroup._layerWeight = psgLayerWeight;
			}

			EditorGUILayout.EndHorizontal();

			if ((int)(Modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
			{
				//색상 옵션을 넣어주자
				//" Color Option On", " Color Option Off"
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ColorVisibleOption),
													" " + Editor.GetUIWord(UIWORD.ColorOptionOn), " " + Editor.GetUIWord(UIWORD.ColorOptionOff),
													SubEditedParamSetGroup._isColorPropertyEnabled, true,
													width, 24))
				{
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Modifier, null, false);
					SubEditedParamSetGroup._isColorPropertyEnabled = !SubEditedParamSetGroup._isColorPropertyEnabled;
					Editor.RefreshControllerAndHierarchy();
				}
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.LayerUp), GUILayout.Width(width / 2 - 2)))//"Layer Up"
			{
				Modifier.ChangeParamSetGroupLayerIndex(SubEditedParamSetGroup, SubEditedParamSetGroup._layerIndex + 1);
			}
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.LayerDown), GUILayout.Width(width / 2 - 2)))//"Layer Down"
			{
				Modifier.ChangeParamSetGroupLayerIndex(SubEditedParamSetGroup, SubEditedParamSetGroup._layerIndex - 1);
			}
			EditorGUILayout.EndHorizontal();

			//TODO : ModMeshOfMod만 작성되어있다.
			//ModBoneOfMod도 작성되어야 한다.

			GUILayout.Space(5);
			//변경 : Copy&Paste는 ModMesh가 선택되어있느냐, ModBone이 선택되어있느냐에 따라 다르다

			bool isModMeshSelected = ModMeshOfMod != null;
			bool isModBoneSelected = ModBoneOfMod != null && Modifier.IsTarget_Bone;

			//복사 가능한가
			bool isModPastable = false;

			if (isModMeshSelected)		{ isModPastable = apSnapShotManager.I.IsPastable(ModMeshOfMod); }
			else if (isModBoneSelected) { isModPastable = apSnapShotManager.I.IsPastable(ModBoneOfMod); }

			//Color prevColor = GUI.backgroundColor;

			GUIStyle guiStyle_Center = new GUIStyle(GUI.skin.box);
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;
			guiStyle_Center.normal.textColor = apEditorUtil.BoxTextColor;

			if (isModPastable)
			{
				GUI.backgroundColor = new Color(0.2f, 0.5f, 0.7f, 1.0f);
				guiStyle_Center.normal.textColor = Color.white;
			}

			//Clipboard 이름 설정
			string strClipboardKeyName = "";

			if (isModMeshSelected)		{ strClipboardKeyName = apSnapShotManager.I.GetClipboardName_ModMesh(); }
			else if (isModBoneSelected)	{ strClipboardKeyName = apSnapShotManager.I.GetClipboardName_ModBone(); }

			if (string.IsNullOrEmpty(strClipboardKeyName))
			{
				strClipboardKeyName = "<Empty Clipboard>";
			}


			GUILayout.Box(strClipboardKeyName, guiStyle_Center, GUILayout.Width(width), GUILayout.Height(32));
			GUI.backgroundColor = prevColor;

			//추가
			//선택된 키가 있다면 => Copy / Paste / Reset 버튼을 만든다.
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));

			//" Copy"
			if (GUILayout.Button(new GUIContent(" " + Editor.GetUIWord(UIWORD.Copy), Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy)), GUILayout.Width(width / 2 - 4), GUILayout.Height(24)))
			{
				//Debug.LogError("TODO : Copy Morph Key");
				//ModMesh를 복사할 것인지, ModBone을 복사할 것인지 결정
				if (SubEditedParamSetGroup != null && ParamSetOfMod != null)
				{
					if (isModMeshSelected && ParamSetOfMod._meshData.Contains(ModMeshOfMod))
					{
						//ModMesh 복사
						string clipboardName = "";
						if (ModMeshOfMod._transform_Mesh != null)
						{ clipboardName = ModMeshOfMod._transform_Mesh._nickName; }
						else if (ModMeshOfMod._transform_MeshGroup != null)
						{ clipboardName = ModMeshOfMod._transform_MeshGroup._nickName; }

						//clipboardName += "\n" + ParamSetOfMod._controlKeyName + "( " + ParamSetOfMod.ControlParamValue + " )";
						string controlParamName = "[Unknown Param]";
						if (SubEditedParamSetGroup._keyControlParam != null)
						{
							controlParamName = SubEditedParamSetGroup._keyControlParam._keyName;
						}
						clipboardName += "\n" + controlParamName + "( " + ParamSetOfMod.ControlParamValue + " )";

						apSnapShotManager.I.Copy_ModMesh(ModMeshOfMod, clipboardName);
					}
					else if (isModBoneSelected && ParamSetOfMod._boneData.Contains(ModBoneOfMod))
					{
						//ModBone 복사
						string clipboardName = "";
						if (ModBoneOfMod._bone != null)
						{ clipboardName = ModBoneOfMod._bone._name; }

						//clipboardName += "\n" + ParamSetOfMod._controlKeyName + "( " + ParamSetOfMod.ControlParamValue + " )";
						string controlParamName = "[Unknown Param]";
						if (SubEditedParamSetGroup._keyControlParam != null)
						{
							controlParamName = SubEditedParamSetGroup._keyControlParam._keyName;
						}
						clipboardName += "\n" + controlParamName + "( " + ParamSetOfMod.ControlParamValue + " )";

						apSnapShotManager.I.Copy_ModBone(ModBoneOfMod, clipboardName);
					}
				}
			}

			//" Paste"
			if (GUILayout.Button(new GUIContent(" " + Editor.GetUIWord(UIWORD.Paste), Editor.ImageSet.Get(apImageSet.PRESET.Edit_Paste)), GUILayout.Width(width / 2 - 4), GUILayout.Height(24)))
			{
				//ModMesh를 복사할 것인지, ModBone을 복사할 것인지 결정
				if (SubEditedParamSetGroup != null && ParamSetOfMod != null)
				{
					object targetObj = ModMeshOfMod;
					if (isModBoneSelected)
					{
						targetObj = ModBoneOfMod;
					}
					apEditorUtil.SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION.Modifier_ModMeshValuePaste, Editor, MeshGroup, Modifier, targetObj, false);

					if (isModMeshSelected && ParamSetOfMod._meshData.Contains(ModMeshOfMod))
					{
						//ModMesh 붙여넣기를 하자
						bool isResult = apSnapShotManager.I.Paste_ModMesh(ModMeshOfMod);
						if (!isResult)
						{
							//EditorUtility.DisplayDialog("Paste Failed", "Paste Failed", "Okay");
							Editor.Notification("Paste Failed", true, false);
						}
						//MeshGroup.AddForceUpdateTarget(ModMeshOfMod._renderUnit);
						MeshGroup.RefreshForce();
					}
					else if (isModBoneSelected && ParamSetOfMod._boneData.Contains(ModBoneOfMod))
					{
						//ModBone 붙여넣기를 하자
						bool isResult = apSnapShotManager.I.Paste_ModBone(ModBoneOfMod);
						if (!isResult)
						{
							//EditorUtility.DisplayDialog("Paste Failed", "Paste Failed", "Okay");
							Editor.Notification("Paste Failed", true, false);
						}
						//if(ModBoneOfMod._renderUnit != null)
						//{
						//	MeshGroup.AddForceUpdateTarget(ModBoneOfMod._renderUnit);
						//}
						MeshGroup.RefreshForce();
					}

				}
			}
			EditorGUILayout.EndHorizontal();
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.ResetValue), GUILayout.Width(width - 4), GUILayout.Height(20)))//"Reset Value"
			{
				if (ParamSetOfMod != null)
				{
					object targetObj = ModMeshOfMod;
					if (ModBoneOfMod != null)
					{
						targetObj = ModBoneOfMod;
					}

					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_ModMeshValueReset, Editor, Modifier, targetObj, false);

					if (ModMeshOfMod != null)
					{
						//ModMesh를 리셋한다.

						ModMeshOfMod.ResetValues();

						//MeshGroup.AddForceUpdateTarget(ModMeshOfMod._renderUnit);
						MeshGroup.RefreshForce();
					}
					else if (ModBoneOfMod != null)
					{
						//ModBone을 리셋한다.
						ModBoneOfMod._transformMatrix.SetIdentity();
						//if(ModBoneOfMod._renderUnit != null)
						//{
						//	MeshGroup.AddForceUpdateTarget(ModBoneOfMod._renderUnit);
						//}
						MeshGroup.RefreshForce();
					}
				}
			}

			//추가 : Transform(Controller)에 한해서 Pose를 저장할 수 있다.
			if(Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.TF)
			{
				GUILayout.Space(10);
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ExportImportPose));//"Export/Import Pose"
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);

				//" Export"
				if(GUILayout.Button(new GUIContent(" " + Editor.GetUIWord(UIWORD.Export), Editor.ImageSet.Get(apImageSet.PRESET.Rig_SaveLoad)), GUILayout.Width((width / 2) - 4), GUILayout.Height(25)))
				{
					//Export Dialog 호출
					apDialog_RetargetSinglePoseExport.ShowDialog(Editor, MeshGroup, Bone);
				}

				//" Import"
				if(GUILayout.Button(new GUIContent(" " + Editor.GetUIWord(UIWORD.Import), Editor.ImageSet.Get(apImageSet.PRESET.Rig_LoadBones)), GUILayout.Width((width / 2) - 4), GUILayout.Height(25)))
				{
					//Import Dialog 호출
					if (SubEditedParamSetGroup != null && ParamSetOfMod != null)
					{
						_loadKey_SinglePoseImport_Mod = apDialog_RetargetSinglePoseImport.ShowDialog(OnRetargetSinglePoseImportMod, Editor, MeshGroup, Modifier, ParamSetOfMod);
					}
					
					
				}

				EditorGUILayout.EndHorizontal();
			}


			GUILayout.Space(12);



			//--------------------------------------------------------------
			// Param Set 중 하나를 선택했을 때
			// 타겟을 등록 / 해제한다.
			// Transform 등록 / 해제
			//--------------------------------------------------------------
			bool isAnyTargetSelected = false;
			bool isContain = false;
			string strTargetName = "";
			object selectedObj = null;

			bool isTarget_Bone = Modifier.IsTarget_Bone;
			bool isTarget_MeshTransform = Modifier.IsTarget_MeshTransform;
			bool isTarget_MeshGroupTransform = Modifier.IsTarget_MeshGroupTransform;
			bool isTarget_ChildMeshTransform = Modifier.IsTarget_ChildMeshTransform;

			bool isBoneTarget = false;

			// 타겟을 선택하자
			bool isAddable = false;
			if (isTarget_Bone && !isAnyTargetSelected)
			{
				//1. Bone 선택
				//TODO : Bone 체크
				if (Bone != null)
				{
					isAnyTargetSelected = true;
					isAddable = true;
					isContain = SubEditedParamSetGroup.IsBoneContain(Bone);
					strTargetName = Bone._name;
					selectedObj = Bone;
					isBoneTarget = true;
				}
			}
			if (isTarget_MeshTransform && !isAnyTargetSelected)
			{
				//2. Mesh Transform 선택
				//Child 체크가 가능할까
				if (SubMeshInGroup != null)
				{
					apRenderUnit targetRenderUnit = null;
					//Child Mesh를 허용하는가
					if (isTarget_ChildMeshTransform)
					{
						//Child를 허용한다.
						targetRenderUnit = MeshGroup.GetRenderUnit(SubMeshInGroup);
					}
					else
					{
						//Child를 허용하지 않는다.
						targetRenderUnit = MeshGroup.GetRenderUnit_NoRecursive(SubMeshInGroup);
					}
					if (targetRenderUnit != null)
					{
						//유효한 선택인 경우
						isContain = SubEditedParamSetGroup.IsMeshTransformContain(SubMeshInGroup);
						isAnyTargetSelected = true;
						strTargetName = SubMeshInGroup._nickName;
						selectedObj = SubMeshInGroup;

						isAddable = true;
					}
				}
			}
			if (isTarget_MeshGroupTransform && !isAnyTargetSelected)
			{
				if (SubMeshGroupInGroup != null)
				{
					//3. MeshGroup Transform 선택
					isContain = SubEditedParamSetGroup.IsMeshGroupTransformContain(SubMeshGroupInGroup);
					isAnyTargetSelected = true;
					strTargetName = SubMeshGroupInGroup._nickName;
					selectedObj = SubMeshGroupInGroup;

					isAddable = true;
				}
			}


			Editor.SetGUIVisible("Modifier_Add Transform Check", isAnyTargetSelected);
			Editor.SetGUIVisible("Modifier_Add Transform Check_Inverse", !isAnyTargetSelected);

			bool isGUI_TargetSelected = Editor.IsDelayedGUIVisible("Modifier_Add Transform Check");
			bool isGUI_TargetUnSelected = Editor.IsDelayedGUIVisible("Modifier_Add Transform Check_Inverse");

			if (isGUI_TargetSelected || isGUI_TargetUnSelected)
			{
				if (isGUI_TargetSelected)
				{
					//Color prevColor = GUI.backgroundColor;
					GUIStyle boxGUIStyle = new GUIStyle(GUI.skin.box);
					boxGUIStyle.alignment = TextAnchor.MiddleCenter;
					boxGUIStyle.normal.textColor = apEditorUtil.BoxTextColor;

					if (isContain)
					{
						GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
						//"[" + strTargetName + "]\nSelected"
						GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.Selected), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

						GUI.backgroundColor = prevColor;

						//"  Remove From Keys"
						if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.RemoveFromKeys), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_RemoveFromControlParamKey)), GUILayout.Width(width), GUILayout.Height(35)))
						{

							//bool result = EditorUtility.DisplayDialog("Remove From Keys", "Remove From Keys [" + strTargetName + "]", "Remove", "Cancel");

							bool result = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveFromKeys_Title),
																Editor.GetTextFormat(TEXT.RemoveFromKeys_Body, strTargetName),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);

							if (result)
							{
								object targetObj = null;
								if (SubMeshInGroup != null && selectedObj == SubMeshInGroup)
								{
									targetObj = SubMeshInGroup;
								}
								else if (SubMeshGroupInGroup != null && selectedObj == SubMeshGroupInGroup)
								{
									targetObj = SubMeshGroupInGroup;
								}
								else if (Bone != null && selectedObj == Bone)
								{
									targetObj = Bone;
								}

								//Undo
								apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_RemoveModMeshFromParamSet, Editor, Modifier, targetObj, false);

								if (SubMeshInGroup != null && selectedObj == SubMeshInGroup)
								{
									SubEditedParamSetGroup.RemoveModifierMeshes(SubMeshInGroup);
								}
								else if (SubMeshGroupInGroup != null && selectedObj == SubMeshGroupInGroup)
								{
									SubEditedParamSetGroup.RemoveModifierMeshes(SubMeshGroupInGroup);
								}
								else if (Bone != null && selectedObj == Bone)
								{
									SubEditedParamSetGroup.RemoveModifierBones(Bone);
								}
								else
								{
									//?
								}

								Editor._portrait.LinkAndRefreshInEditor(false);
								AutoSelectModMeshOrModBone();
								Editor.RefreshControllerAndHierarchy();

								Editor.SetRepaint();
							}
						}
					}
					else if (!isAddable)
					{
						//추가 가능하지 않다.
						GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
						//"[" + strTargetName + "]\nNot able to be Added"
						GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.NotAbleToBeAdded), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

						GUI.backgroundColor = prevColor;
					}
					else
					{
						//아직 추가하지 않았다. 추가하자
						GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
						//"[" + strTargetName + "]\nNot Added to Edit"
						GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.NotAddedtoEdit), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

						GUI.backgroundColor = prevColor;

						//"  Add To Keys"
						if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.AddToKeys), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_AddToControlParamKey)), GUILayout.Width(width), GUILayout.Height(50)))
						{
							//ModMesh또는 ModBone으로 생성 후 추가한다.
							if (isBoneTarget)
							{
								//Bone
								Editor.Controller.AddModBone_WithSelectedBone();
							}
							else
							{
								//MeshTransform, MeshGroup
								Editor.Controller.AddModMesh_WithSubMeshOrSubMeshGroup();
							}

							Editor.SetRepaint();
						}
					}
					GUI.backgroundColor = prevColor;
				}

				EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(10));
				EditorGUILayout.EndVertical();
				GUILayout.Space(11);

				//ParamSetWeight를 사용하는 Modifier인가
				bool isUseParamSetWeight = Modifier.IsUseParamSetWeight;


				// Param Set 리스트를 출력한다.
				//-------------------------------------
				int iRemove = -1;
				for (int i = 0; i < SubEditedParamSetGroup._paramSetList.Count; i++)
				{
					bool isRemove = DrawModParamSetProperty(i, SubEditedParamSetGroup, SubEditedParamSetGroup._paramSetList[i], width - 10, ParamSetOfMod, isUseParamSetWeight);
					if (isRemove)
					{
						iRemove = i;
					}
				}
				if (iRemove >= 0)
				{
					Editor.Controller.RemoveRecordKey(SubEditedParamSetGroup._paramSetList[iRemove], null);
				}
			}


			//-----------------------------------------------------------------------------------
		}




		private void OnRetargetSinglePoseImportMod(	object loadKey, bool isSuccess, apRetarget resultRetarget,
													apMeshGroup targetMeshGroup,
													apModifierBase targetModifier, apModifierParamSet targetParamSet)
		{
			if(loadKey != _loadKey_SinglePoseImport_Mod || !isSuccess)
			{
				_loadKey_SinglePoseImport_Mod = null;
				return;
			}

			_loadKey_SinglePoseImport_Mod = null;

			//Import 처리를 하자
			Editor.Controller.ImportBonePoseFromRetargetSinglePoseFileToModifier(targetMeshGroup, resultRetarget, targetModifier, targetParamSet);
		}


		private bool DrawModParamSetProperty(int index, apModifierParamSetGroup paramSetGroup, apModifierParamSet paramSet, int width, apModifierParamSet selectedParamSet, bool isUseParamSetWeight)
		{
			bool isRemove = false;
			Rect lastRect = GUILayoutUtility.GetLastRect();
			Color prevColor = GUI.backgroundColor;

			bool isSelect = false;
			if (paramSet == selectedParamSet)
			{
				GUI.backgroundColor = new Color(0.9f, 0.7f, 0.7f, 1.0f);
				isSelect = true;
			}
			else
			{
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
			}

			int heightOffset = 18;
			if (index == 0)
			{
				//heightOffset = 5;
				heightOffset = 9;
			}

			GUI.Box(new Rect(lastRect.x, lastRect.y + heightOffset, width + 10, 30), "");
			GUI.backgroundColor = prevColor;



			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));

			GUILayout.Space(10);

			int compWidth = width - (55 + 20 + 5 + 10);
			if (isUseParamSetWeight)
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.textField);
				guiStyle.alignment = TextAnchor.MiddleLeft;

				//ParamSetWeight를 출력/수정할 수 있게 한다.
				float paramSetWeight = EditorGUILayout.DelayedFloatField(paramSet._overlapWeight, guiStyle, GUILayout.Width(30), GUILayout.Height(20));
				if (paramSetWeight != paramSet._overlapWeight)
				{
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Modifier, null, false);
					paramSet._overlapWeight = Mathf.Clamp01(paramSetWeight);
					apEditorUtil.ReleaseGUIFocus();
					MeshGroup.RefreshForce();
					Editor.RefreshControllerAndHierarchy();
				}
				compWidth -= 34;
			}

			switch (paramSetGroup._keyControlParam._valueType)
			{
				//case apControlParam.TYPE.Bool:
				//	{
				//		GUIStyle guiStyle = new GUIStyle(GUI.skin.toggle);
				//		guiStyle.alignment = TextAnchor.MiddleLeft;
				//		paramSet._conSyncValue_Bool = EditorGUILayout.Toggle(paramSet._conSyncValue_Bool, guiStyle, GUILayout.Width(compWidth), GUILayout.Height(20));
				//	}

				//	break;

				case apControlParam.TYPE.Int:
					{
						GUIStyle guiStyle = new GUIStyle(GUI.skin.textField);
						guiStyle.alignment = TextAnchor.MiddleLeft;
						int conInt = EditorGUILayout.DelayedIntField(paramSet._conSyncValue_Int, guiStyle, GUILayout.Width(compWidth), GUILayout.Height(20));
						if (conInt != paramSet._conSyncValue_Int)
						{
							//이건 Dirty만 하자
							apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Modifier, null, false);
							paramSet._conSyncValue_Int = conInt;
							apEditorUtil.ReleaseGUIFocus();
						}

					}
					break;

				case apControlParam.TYPE.Float:
					{
						GUIStyle guiStyle = new GUIStyle(GUI.skin.textField);
						guiStyle.alignment = TextAnchor.MiddleLeft;
						float conFloat = EditorGUILayout.DelayedFloatField(paramSet._conSyncValue_Float, guiStyle, GUILayout.Width(compWidth), GUILayout.Height(20));
						if (conFloat != paramSet._conSyncValue_Float)
						{
							apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Modifier, null, false);
							paramSet._conSyncValue_Float = conFloat;
							apEditorUtil.ReleaseGUIFocus();
						}
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						GUIStyle guiStyle = new GUIStyle(GUI.skin.textField);
						guiStyle.alignment = TextAnchor.MiddleLeft;
						float conVec2X = EditorGUILayout.DelayedFloatField(paramSet._conSyncValue_Vector2.x, guiStyle, GUILayout.Width(compWidth / 2 - 2), GUILayout.Height(20));
						float conVec2Y = EditorGUILayout.DelayedFloatField(paramSet._conSyncValue_Vector2.y, guiStyle, GUILayout.Width(compWidth / 2 - 2), GUILayout.Height(20));
						if (conVec2X != paramSet._conSyncValue_Vector2.x || conVec2Y != paramSet._conSyncValue_Vector2.y)
						{
							apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Modifier, null, false);
							paramSet._conSyncValue_Vector2.x = conVec2X;
							paramSet._conSyncValue_Vector2.y = conVec2Y;
							apEditorUtil.ReleaseGUIFocus();
						}

					}
					break;
			}

			if (isSelect)
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.normal.textColor = Color.white;
				guiStyle.alignment = TextAnchor.UpperCenter;
				GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
				GUILayout.Box(Editor.GetUIWord(UIWORD.Selected), guiStyle, GUILayout.Width(55), GUILayout.Height(20));//"Editing" -> Selected
				GUI.backgroundColor = prevColor;
			}
			else
			{
				//"Select"
				if (GUILayout.Button(Editor.GetUIWord(UIWORD.Select), GUILayout.Width(55), GUILayout.Height(20)))
				{
					if (Editor.LeftTab != apEditor.TAB_LEFT.Controller)
					{
						Editor.SetLeftTab(apEditor.TAB_LEFT.Controller);
					}
					SetParamSetOfModifier(paramSet);
					if (ParamSetOfMod != null)
					{
						apControlParam targetControlParam = paramSetGroup._keyControlParam;
						if (targetControlParam != null)
						{
							//switch (ParamSetOfMod._controlParam._valueType)
							switch (targetControlParam._valueType)
							{
								//case apControlParam.TYPE.Bool:
								//	targetControlParam._bool_Cur = paramSet._conSyncValue_Bool;
								//	break;

								case apControlParam.TYPE.Int:
									targetControlParam._int_Cur = paramSet._conSyncValue_Int;
									//if (targetControlParam._isRange)
									{
										targetControlParam._int_Cur =
											Mathf.Clamp(targetControlParam._int_Cur,
														targetControlParam._int_Min,
														targetControlParam._int_Max);
									}
									break;

								case apControlParam.TYPE.Float:
									targetControlParam._float_Cur = paramSet._conSyncValue_Float;
									//if (targetControlParam._isRange)
									{
										targetControlParam._float_Cur =
											Mathf.Clamp(targetControlParam._float_Cur,
														targetControlParam._float_Min,
														targetControlParam._float_Max);
									}
									break;

								case apControlParam.TYPE.Vector2:
									targetControlParam._vec2_Cur = paramSet._conSyncValue_Vector2;
									//if (targetControlParam._isRange)
									{
										targetControlParam._vec2_Cur.x =
											Mathf.Clamp(targetControlParam._vec2_Cur.x,
														targetControlParam._vec2_Min.x,
														targetControlParam._vec2_Max.x);

										targetControlParam._vec2_Cur.y =
											Mathf.Clamp(targetControlParam._vec2_Cur.y,
														targetControlParam._vec2_Min.y,
														targetControlParam._vec2_Max.y);
									}
									break;


							}
						}
					}
				}
			}

			if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey), GUILayout.Width(20), GUILayout.Height(20)))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Record Key", "Remove Record Key?", "Remove", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveRecordKey_Title),
																Editor.GetText(TEXT.RemoveRecordKey_Body),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel));
				if (isResult)
				{
					//삭제시 true 리턴
					isRemove = true;
				}
			}



			EditorGUILayout.EndHorizontal();
			GUILayout.Space(20);

			return isRemove;
		}

		




		private void DrawModifierPropertyGUI_KeyframeParamSet(int width, int height, string recordName)
		{
			GUIStyle guiNone = new GUIStyle(GUIStyle.none);
			guiNone.normal.textColor = GUI.skin.label.normal.textColor;

			GUIStyle guiSelected = new GUIStyle(GUIStyle.none);
			if(EditorGUIUtility.isProSkin)
			{
				guiSelected.normal.textColor = Color.cyan;
			}
			else
			{
				guiSelected.normal.textColor = Color.white;
			}
			
			//"Animation Clips"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.AnimationClips), GUILayout.Width(width));

			GUILayout.Space(5);

			// 생성된 ParamSet Group을 선택하자
			//------------------------------------------------------------------
			// AnimClip에 따른 Param Set Group Anim Pack 리스트
			//------------------------------------------------------------------
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(120));
			GUILayout.Space(5);

			Rect lastRect = GUILayoutUtility.GetLastRect();

			Color prevColor = GUI.backgroundColor;

			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);

			GUI.Box(new Rect(lastRect.x + 5, lastRect.y, width, 120), "");
			GUI.backgroundColor = prevColor;

			#region [미사용 코드] 이건 처리 역순 필요 없다. 동적 순서 배열이므로
			////처리 역순으로 보여준다. // 
			//List<apModifierParamSetGroup> paramSetGroups = new List<apModifierParamSetGroup>();
			//if (Modifier._paramSetGroup_controller.Count > 0)
			//{
			//	for (int i = Modifier._paramSetGroup_controller.Count - 1; i >= 0; i--)
			//	{
			//		paramSetGroups.Add(Modifier._paramSetGroup_controller[i]);
			//	}
			//} 
			#endregion
			List<apModifierParamSetGroupAnimPack> paramSetGroupAnimPacks = Modifier._paramSetGroupAnimPacks;


			//등록된 Keyframe Param Group 리스트를 출력하자
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(120));
			_scrollBottom_Status = EditorGUILayout.BeginScrollView(_scrollBottom_Status, false, true);
			GUILayout.Space(2);
			int scrollWidth = width - (30);
			EditorGUILayout.BeginVertical(GUILayout.Width(scrollWidth), GUILayout.Height(120));
			GUILayout.Space(3);

			Texture2D animIconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation);

			//현재 선택중인 파라미터 그룹
			apModifierParamSetGroupAnimPack curParamSetGroupAnimPack = SubEditedParamSetGroupAnimPack;

			for (int i = 0; i < paramSetGroupAnimPacks.Count; i++)
			{
				GUIStyle curGUIStyle = guiNone;

				if (curParamSetGroupAnimPack == paramSetGroupAnimPacks[i])
				{
					lastRect = GUILayoutUtility.GetLastRect();

					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					}

					int offsetHeight = 18 + 3;
					if (i == 0)
					{
						offsetHeight = 1 + 3;
					}

					GUI.Box(new Rect(lastRect.x, lastRect.y + offsetHeight, scrollWidth + 35, 20), "");
					GUI.backgroundColor = prevColor;

					curGUIStyle = guiSelected;
				}

				EditorGUILayout.BeginHorizontal(GUILayout.Width(scrollWidth - 5));
				GUILayout.Space(5);

				if (GUILayout.Button(new GUIContent(" " + paramSetGroupAnimPacks[i].LinkedAnimClip._name, animIconImage),
									curGUIStyle,
									GUILayout.Width(scrollWidth - (5)), GUILayout.Height(20)))
				{
					SetParamSetGroupAnimPackOfModifier(paramSetGroupAnimPacks[i]);

					Editor.RefreshControllerAndHierarchy();
				}
				EditorGUILayout.EndHorizontal();
			}


			EditorGUILayout.EndVertical();

			GUILayout.Space(120);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			//------------------------------------------------------------------ < Param Set Group 리스트

			//-----------------------------------------------------------------------------------
			// Param Set Group 선택시 / 선택된 Param Set Group 정보와 포함된 Param Set 리스트
			//-----------------------------------------------------------------------------------

			GUILayout.Space(10);

			//>> 여기서 ParamSetGroup 설정을 할 순 없다. (ParamSetGroup이 TimelineLayer이므로.
			//AnimClip 기준으로는 ParamSetGroup을 묶은 가상의 그룹(SubEditedParamSetGroupAnimPack)을 설정해야하는데,
			//이건 묶음이므로 실제로는 Animation 설정에서 Timeline에서 해야한다. (Timelinelayer = ParamSetGroup이므로)
			//Editor.SetGUIVisible("Anim Selected ParamSetGroup", (SubEditedParamSetGroupAnimPack. != null));

			//if (!Editor.IsDelayedGUIVisible("Anim Selected ParamSetGroup"))
			//{
			//	return;
			//}


			//EditorGUILayout.LabelField("Selected Animation Clip");
			//EditorGUILayout.LabelField(_subEditedParamSetGroupAnimPack._keyAnimClip._name);
			//GUILayout.Space(5);

			//if ((int)(Modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
			//{
			//	//색상 옵션을 넣어주자
			//	EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//	EditorGUILayout.LabelField("Color Option", GUILayout.Width(160));
			//	_subEditedParamSetGroupAnimPack._isColorPropertyEnabled = EditorGUILayout.Toggle(_subEditedParamSetGroupAnimPack._isColorPropertyEnabled, GUILayout.Width(width - 85));
			//	EditorGUILayout.EndHorizontal();
			//}
		}


		private object _riggingModifier_prevSelectedTransform = null;
		private bool _riggingModifier_prevIsContained = false;
		private int _riggingModifier_prevNumBoneWeights = 0;
		//Rigging Modifier UI를 출력한다.
		private void DrawModifierPropertyGUI_Rigging(int width, int height, string recordName)
		{
			GUIStyle guiNone = new GUIStyle(GUIStyle.none);
			guiNone.normal.textColor = GUI.skin.label.normal.textColor;
			guiNone.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiSelected = new GUIStyle(GUIStyle.none);
			if(EditorGUIUtility.isProSkin)
			{
				guiSelected.normal.textColor = Color.cyan;
			}
			else
			{
				guiSelected.normal.textColor = Color.white;
			}
			guiSelected.alignment = TextAnchor.MiddleLeft;

			//"Target Mesh Transform"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.TargetMeshTransform), GUILayout.Width(width));
			//1. Mesh Transform 등록 체크
			//2. Weight 툴
			// 선택한 Vertex
			// Auto Normalize
			// Set Weight, +/- Weight, * Weight
			// Blend, Auto Rigging, Normalize, Prune,
			// Copy / Paste
			// Bone (Color, Remove)

			bool isTarget_MeshTransform = Modifier.IsTarget_MeshTransform;
			bool isTarget_ChildMeshTransform = Modifier.IsTarget_ChildMeshTransform;

			bool isContainInParamSetGroup = false;
			string strTargetName = "";
			object selectedObj = null;
			bool isAnyTargetSelected = false;
			bool isAddable = false;

			apTransform_Mesh targetMeshTransform = SubMeshInGroup;
			apModifierParamSetGroup paramSetGroup = SubEditedParamSetGroup;
			if (paramSetGroup == null)
			{
				//? Rigging에서는 ParamSetGroup이 있어야 한다.
				Editor.Controller.AddStaticParamSetGroupToModifier();

				if (Modifier._paramSetGroup_controller.Count > 0)
				{
					SetParamSetGroupOfModifier(Modifier._paramSetGroup_controller[0]);
				}
				paramSetGroup = SubEditedParamSetGroup;
				if (paramSetGroup == null)
				{
					Debug.LogError("AnyPortrait : ParamSet Group Is Null (" + Modifier._paramSetGroup_controller.Count + ")");
					return;
				}

				AutoSelectModMeshOrModBone();
			}
			apModifierParamSet paramSet = ParamSetOfMod;
			if (paramSet == null)
			{
				//Rigging에서는 1개의 ParamSetGroup과 1개의 ParamSet이 있어야 한다.
				//선택된게 없다면, ParamSet이 1개 있는지 확인
				//그후 선택한다.

				if (paramSetGroup._paramSetList.Count == 0)
				{
					paramSet = new apModifierParamSet();
					paramSet.LinkParamSetGroup(paramSetGroup);
					paramSetGroup._paramSetList.Add(paramSet);
				}
				else
				{
					paramSet = paramSetGroup._paramSetList[0];
				}
				SetParamSetOfModifier(paramSet);
			}



			//1. Mesh Transform 등록 체크
			if (targetMeshTransform != null)
			{
				apRenderUnit targetRenderUnit = null;
				//Child Mesh를 허용하는가
				if (isTarget_ChildMeshTransform)
				{
					//Child를 허용한다.
					targetRenderUnit = MeshGroup.GetRenderUnit(targetMeshTransform);
				}
				else
				{
					//Child를 허용하지 않는다.
					targetRenderUnit = MeshGroup.GetRenderUnit_NoRecursive(targetMeshTransform);
				}
				if (targetRenderUnit != null)
				{
					//유효한 선택인 경우
					isContainInParamSetGroup = paramSetGroup.IsMeshTransformContain(targetMeshTransform);
					isAnyTargetSelected = true;
					strTargetName = targetMeshTransform._nickName;
					selectedObj = targetMeshTransform;

					isAddable = true;
				}
			}

			if (Event.current.type == EventType.Layout ||
				Event.current.type == EventType.Repaint)
			{
				_riggingModifier_prevSelectedTransform = targetMeshTransform;
				_riggingModifier_prevIsContained = isContainInParamSetGroup;
			}
			bool isSameSetting = (targetMeshTransform == _riggingModifier_prevSelectedTransform)
								&& (isContainInParamSetGroup == _riggingModifier_prevIsContained);


			Editor.SetGUIVisible("Modifier_Add Transform Check [Rigging]", isSameSetting);



			if (!Editor.IsDelayedGUIVisible("Modifier_Add Transform Check [Rigging]"))
			{
				return;
			}

			Color prevColor = GUI.backgroundColor;

			GUIStyle boxGUIStyle = new GUIStyle(GUI.skin.box);
			boxGUIStyle.alignment = TextAnchor.MiddleCenter;
			boxGUIStyle.normal.textColor = apEditorUtil.BoxTextColor;

			if (targetMeshTransform == null)
			{
				//선택된 MeshTransform이 없다.
				GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				//"No Mesh is Selected"
				GUILayout.Box(Editor.GetUIWord(UIWORD.NoMeshIsSelected), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;
			}
			else if (isContainInParamSetGroup)
			{
				GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				//"[" + strTargetName + "]\nSelected"
				GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.Selected), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;

				//"  Remove From Rigging"
				if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.RemoveFromRigging), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_RemoveFromRigging)), GUILayout.Width(width), GUILayout.Height(30)))
				{

					//bool result = EditorUtility.DisplayDialog("Remove From Rigging", "Remove From Rigging [" + strTargetName + "]", "Remove", "Cancel");

					bool result = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveFromRigging_Title),
																Editor.GetTextFormat(TEXT.RemoveFromRigging_Body, strTargetName),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);

					if (result)
					{
						object targetObj = SubMeshInGroup;
						if (SubMeshGroupInGroup != null && selectedObj == SubMeshGroupInGroup)
						{
							targetObj = SubMeshGroupInGroup;
						}

						apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_RemoveBoneRigging, Editor, Modifier, targetObj, false);

						if (SubMeshInGroup != null && selectedObj == SubMeshInGroup)
						{
							SubEditedParamSetGroup.RemoveModifierMeshes(SubMeshInGroup);
						}
						else if (SubMeshGroupInGroup != null && selectedObj == SubMeshGroupInGroup)
						{
							SubEditedParamSetGroup.RemoveModifierMeshes(SubMeshGroupInGroup);
						}
						else
						{
							//TODO : Bone 제거
						}

						Editor._portrait.LinkAndRefreshInEditor(false);
						AutoSelectModMeshOrModBone();

						Editor.Hierarchy_MeshGroup.RefreshUnits();
						Editor.RefreshControllerAndHierarchy();

						Editor.SetRepaint();
					}
				}
			}
			else if (!isAddable)
			{
				//추가 가능하지 않다.
				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				//"[" + strTargetName + "]\nNot able to be Added"
				GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.NotAbleToBeAdded), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;
			}
			else
			{
				//아직 추가하지 않았다. 추가하자
				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				//"[" + strTargetName + "]\nNot Added to Edit"
				GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.NotAddedtoEdit), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;

				//"  Add Rigging" -> "  Add to Rigging"
				if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.AddToRigging), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_AddToRigging)), GUILayout.Width(width), GUILayout.Height(30)))
				{
					Editor.Controller.AddModMesh_WithSubMeshOrSubMeshGroup();

					Editor.Hierarchy_MeshGroup.RefreshUnits();

					Editor.SetRepaint();
				}
			}
			GUI.backgroundColor = prevColor;

			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);

			List<ModRenderVert> selectedVerts = Editor.Select.ModRenderVertListOfMod;
			//bool isAnyVertSelected = (selectedVerts != null && selectedVerts.Count > 0);


			//2. Weight 툴
			// 선택한 Vertex
			// Auto Normalize
			// Set Weight, +/- Weight, * Weight
			// Blend, Auto Rigging, Normalize, Prune,
			// Copy / Paste
			// Bone (Color, Remove)

			//어떤 Vertex가 선택되었는지 표기한다.

			_rigEdit_vertRigDataList.Clear();

			if (!isAnyTargetSelected)
			{
				//선택된게 없다.
				GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				//"No Vetex is Selected"
				GUILayout.Box(Editor.GetUIWord(UIWORD.NoVertexisSelected), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(25));

				GUI.backgroundColor = prevColor;


			}
			else if (selectedVerts.Count == 1)
			{
				//1개의 Vertex
				GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				//"[Vertex " + selectedVerts[0]._renderVert._vertex._index + "] is Selected"
				GUILayout.Box(Editor.GetUIWordFormat(UIWORD.SingleVertexSelected, selectedVerts[0]._renderVert._vertex._index), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(25));

				GUI.backgroundColor = prevColor;

			}
			else
			{
				GUI.backgroundColor = new Color(0.4f, 0.5f, 1.0f, 1.0f);
				//selectedVerts.Count + " Verts are Selected"
				GUILayout.Box(Editor.GetUIWordFormat(UIWORD.NumVertsareSelected, selectedVerts.Count), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(25));

				GUI.backgroundColor = prevColor;
			}
			int nSelectedVerts = 0;
			if (isAnyTargetSelected)
			{
				nSelectedVerts = selectedVerts.Count;

				//리스트에 넣을 Rig 리스트를 완성하자
				for (int i = 0; i < selectedVerts.Count; i++)
				{
					apModifiedVertexRig modVertRig = selectedVerts[i]._modVertRig;
					if (modVertRig == null)
					{
						// -ㅅ-?
						continue;
					}
					for (int iPair = 0; iPair < modVertRig._weightPairs.Count; iPair++)
					{
						apModifiedVertexRig.WeightPair pair = modVertRig._weightPairs[iPair];
						VertRigData sameBoneData = _rigEdit_vertRigDataList.Find(delegate (VertRigData a)
						{
							return a._bone == pair._bone;
						});
						if (sameBoneData != null)
						{
							sameBoneData.AddRig(pair._weight);
						}
						else
						{
							_rigEdit_vertRigDataList.Add(new VertRigData(pair._bone, pair._weight));
						}
					}
				}
			}


			// 기본 토대는 3ds Max와 유사하게 가자

			// Edit가 활성화되지 않으면 버튼 선택불가
			bool isBtnAvailable = _rigEdit_isBindingEdit;

			int CALCULATE_SET = 0;
			int CALCULATE_ADD = 1;
			int CALCULATE_MULTIPLY = 2;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			GUILayout.Space(5);
			//고정된 Weight 값
			//0, 0.1, 0.3, 0.5, 0.7, 0.9, 1 (7개)
			int widthPresetWeight = ((width - 2 * 7) / 7) - 2;
			bool isPresetAdapt = false;
			float presetWeight = 0.0f;
			if (apEditorUtil.ToggledButton("0", false, isBtnAvailable, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.0f;
			}
			if (apEditorUtil.ToggledButton(".1", false, isBtnAvailable, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.1f;
			}
			if (apEditorUtil.ToggledButton(".3", false, isBtnAvailable, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.3f;
			}
			if (apEditorUtil.ToggledButton(".5", false, isBtnAvailable, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.5f;
			}
			if (apEditorUtil.ToggledButton(".7", false, isBtnAvailable, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.7f;
			}
			if (apEditorUtil.ToggledButton(".9", false, isBtnAvailable, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.9f;
			}
			if (apEditorUtil.ToggledButton("1", false, isBtnAvailable, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 1f;
			}
			EditorGUILayout.EndHorizontal();

			if (isPresetAdapt)
			{
				Editor.Controller.SetBoneWeight(presetWeight, CALCULATE_SET);
			}

			int heightSetWeight = 25;
			int widthSetBtn = 90;
			int widthIncDecBtn = 30;
			int widthValue = width - (widthSetBtn + widthIncDecBtn * 2 + 2 * 5 + 5);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightSetWeight));
			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(GUILayout.Width(widthValue), GUILayout.Height(heightSetWeight - 2));
			GUILayout.Space(8);
			_rigEdit_setWeightValue = EditorGUILayout.DelayedFloatField(_rigEdit_setWeightValue);
			EditorGUILayout.EndVertical();

			//"Set Weight"
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.SetWeight), false, isBtnAvailable, widthSetBtn, heightSetWeight))
			{
				//Debug.LogError("TODO : Weight 적용 - Set");
				Editor.Controller.SetBoneWeight(_rigEdit_setWeightValue, CALCULATE_SET);
				GUI.FocusControl(null);
			}

			if (apEditorUtil.ToggledButton("+", false, isBtnAvailable, widthIncDecBtn, heightSetWeight))
			{
				////0.05 단위로 올라가거나 내려온다. (5%)
				////현재 값에서 "int형 반올림"을 수행하고 처리
				//_rigEdit_setWeightValue = Mathf.Clamp01((float)((int)(_rigEdit_setWeightValue * 20.0f + 0.5f) + 1) / 20.0f);
				//이게 아니었다..
				//0.05 추가
				Editor.Controller.SetBoneWeight(0.05f, CALCULATE_ADD);

				GUI.FocusControl(null);
			}
			if (apEditorUtil.ToggledButton("-", false, isBtnAvailable, widthIncDecBtn, heightSetWeight))
			{
				//0.05 단위로 올라가거나 내려온다. (5%)
				//현재 값에서 "int형 반올림"을 수행하고 처리
				//_rigEdit_setWeightValue = Mathf.Clamp01((float)((int)(_rigEdit_setWeightValue * 20.0f + 0.5f) - 1) / 20.0f);
				//0.05 빼기
				Editor.Controller.SetBoneWeight(-0.05f, CALCULATE_ADD);

				GUI.FocusControl(null);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(3);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightSetWeight));
			GUILayout.Space(5);


			EditorGUILayout.BeginVertical(GUILayout.Width(widthValue), GUILayout.Height(heightSetWeight - 2));
			GUILayout.Space(8);
			_rigEdit_scaleWeightValue = EditorGUILayout.DelayedFloatField(_rigEdit_scaleWeightValue);
			EditorGUILayout.EndVertical();

			//"Scale Weight"
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.ScaleWeight), false, isBtnAvailable, widthSetBtn, heightSetWeight))
			{
				//Debug.LogError("TODO : Weight 적용 - Set");
				Editor.Controller.SetBoneWeight(_rigEdit_scaleWeightValue, CALCULATE_MULTIPLY);//Multiply 방식
				GUI.FocusControl(null);
			}

			if (apEditorUtil.ToggledButton("+", false, isBtnAvailable, widthIncDecBtn, heightSetWeight))
			{
				//0.01 단위로 올라가거나 내려온다. (1%)
				//현재 값에서 반올림을 수행하고 처리
				//Scale은 Clamp가 걸리지 않는다.
				//_rigEdit_scaleWeightValue = (float)((int)(_rigEdit_scaleWeightValue * 100.0f + 0.5f) + 1) / 100.0f;
				//x1.05
				Editor.Controller.SetBoneWeight(1.05f, CALCULATE_MULTIPLY);//Multiply 방식

				GUI.FocusControl(null);
			}
			if (apEditorUtil.ToggledButton("-", false, isBtnAvailable, widthIncDecBtn, heightSetWeight))
			{
				//0.01 단위로 올라가거나 내려온다. (1%)
				//현재 값에서 반올림을 수행하고 처리
				//_rigEdit_scaleWeightValue = (float)((int)(_rigEdit_scaleWeightValue * 100.0f + 0.5f) - 1) / 100.0f;
				//if(_rigEdit_scaleWeightValue < 0.0f)
				//{
				//	_rigEdit_scaleWeightValue = 0.0f;
				//}
				//x0.95
				Editor.Controller.SetBoneWeight(0.95f, CALCULATE_MULTIPLY);//Multiply 방식

				GUI.FocusControl(null);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(8);

			int heightToolBtn = 25;
			//int width4Btn = ((width - 5) / 4) - (2);

			//Blend, Prune, Normalize, Auto Rigging
			//Normalize On/Off
			//Copy / Paste

			int width2Btn = (width - 5) / 2;

			//Auto Rigging
			//"  Auto Normalize", "  Auto Normalize"
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_AutoNormalize), "  " + Editor.GetUIWord(UIWORD.AutoNormalize), "  " + Editor.GetUIWord(UIWORD.AutoNormalize), _rigEdit_isAutoNormalize, isBtnAvailable, width, 28))
			{
				_rigEdit_isAutoNormalize = !_rigEdit_isAutoNormalize;

				//Off -> On 시에 Normalize를 적용하자
				if (_rigEdit_isAutoNormalize)
				{
					Editor.Controller.SetBoneWeightNormalize();
				}
				//Auto Normalize는 에디터 옵션으로 저장된다.
				Editor.SaveEditorPref();
			}


			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightToolBtn));
			GUILayout.Space(5);
			//" Blend"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Blend), " " + Editor.GetUIWord(UIWORD.Blend), false, isBtnAvailable, width2Btn, heightToolBtn, "The weights of vertices are blended"))
			{
				//Blend
				Editor.Controller.SetBoneWeightBlend();
			}
			//" Normalize"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Normalize), " " + Editor.GetUIWord(UIWORD.Normalize), false, isBtnAvailable, width2Btn, heightToolBtn, "Normalize rigging weight"))
			{
				//Normalize
				Editor.Controller.SetBoneWeightNormalize();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightToolBtn));
			GUILayout.Space(5);
			//" Prune"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Prune), " " + Editor.GetUIWord(UIWORD.Prune), false, isBtnAvailable, width2Btn, heightToolBtn, "Remove rigging bones its weight is under 0.01"))
			{
				//Prune
				Editor.Controller.SetBoneWeightPrune();
			}
			//" Auto Rig"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Auto), " " + Editor.GetUIWord(UIWORD.AutoRig), false, isBtnAvailable, width2Btn, heightToolBtn, "Rig Automatically"))
			{
				//Auto
				Editor.Controller.SetBoneAutoRig();
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);


			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightToolBtn));
			GUILayout.Space(5);
			//" Grow"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Grow), " " + Editor.GetUIWord(UIWORD.Grow), false, isBtnAvailable, width2Btn, heightToolBtn, "Select more of the surrounding vertices"))
			{
				Editor.Controller.SelectVertexRigGrowOrShrink(true);
			}
			//" Shrink"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Shrink), " " + Editor.GetUIWord(UIWORD.Shrink), false, isBtnAvailable, width2Btn, heightToolBtn, "Reduce selected vertices"))
			{
				Editor.Controller.SelectVertexRigGrowOrShrink(false);
			}
			EditorGUILayout.EndHorizontal();


			bool isCopyAvailable = isBtnAvailable && selectedVerts.Count == 1;
			bool isPasteAvailable = false;
			if (isCopyAvailable)
			{
				if (apSnapShotManager.I.IsPastable(selectedVerts[0]._modVertRig))
				{
					isPasteAvailable = true;
				}
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightToolBtn));
			GUILayout.Space(5);
			//" Copy"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy), " " + Editor.GetUIWord(UIWORD.Copy), false, isCopyAvailable, width2Btn, heightToolBtn))
			{
				//Copy	
				apSnapShotManager.I.Copy_VertRig(selectedVerts[0]._modVertRig, "Mod Vert Rig");
			}
			//" Paste"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Paste), " " + Editor.GetUIWord(UIWORD.Paste), false, isPasteAvailable, width2Btn, heightToolBtn))
			{
				//Paste
				if (apSnapShotManager.I.Paste_VertRig(selectedVerts[0]._modVertRig))
				{
					MeshGroup.RefreshForce();
				}
			}
			EditorGUILayout.EndHorizontal();
			//이제 리스트를 불러오자

			int nRigDataList = _rigEdit_vertRigDataList.Count;
			if (_riggingModifier_prevNumBoneWeights != nRigDataList)
			{
				Editor.SetGUIVisible("Rig Mod - RigDataCount Refreshed", true);
				if (Editor.IsDelayedGUIVisible("Rig Mod - RigDataCount Refreshed"))
				{
					_riggingModifier_prevNumBoneWeights = nRigDataList;
				}
			}
			else
			{
				Editor.SetGUIVisible("Rig Mod - RigDataCount Refreshed", false);
			}

			//if(selectedVerts)
			//List<apModifiedVertexRig> vertRigList = 
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(200));
			GUILayout.Space(5);

			Rect lastRect = GUILayoutUtility.GetLastRect();

			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);

			GUI.Box(new Rect(lastRect.x + 5, lastRect.y, width, 200), "");
			GUI.backgroundColor = prevColor;


			//Weight 리스트를 출력하자
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(200));
			_scrollBottom_Status = EditorGUILayout.BeginScrollView(_scrollBottom_Status, false, true);
			GUILayout.Space(2);
			int scrollWidth = width - (30);
			EditorGUILayout.BeginVertical(GUILayout.Width(scrollWidth), GUILayout.Height(200));
			GUILayout.Space(3);

			Texture2D imgRemove = Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey);

			VertRigData vertRigData = null;
			string strLabel = "";

			VertRigData removeRigData = null;
			int widthLabel_Name = scrollWidth - (5 + 25 + 14 + 2 + 60);
			//for (int i = 0; i < _rigEdit_vertRigDataList.Count; i++)
			for (int i = 0; i < _riggingModifier_prevNumBoneWeights; i++)
			{
				if (i < _rigEdit_vertRigDataList.Count)
				{
					GUIStyle curGUIStyle = guiNone;
					vertRigData = _rigEdit_vertRigDataList[i];
					if (vertRigData._bone == Bone)
					{
						lastRect = GUILayoutUtility.GetLastRect();

						if (EditorGUIUtility.isProSkin)
						{
							GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
						}
						else
						{
							GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
						}

						int offsetHeight = 18 + 3;
						if (i == 0)
						{
							offsetHeight = 1 + 3;
						}

						GUI.Box(new Rect(lastRect.x, lastRect.y + offsetHeight, scrollWidth + 35, 20), "");
						GUI.backgroundColor = prevColor;

						curGUIStyle = guiSelected;
					}


					EditorGUILayout.BeginHorizontal(GUILayout.Width(scrollWidth - 5));
					GUILayout.Space(5);

					//Bone의 색상, 이름, Weight, X를 출력
					GUI.backgroundColor = vertRigData._bone._color;
					GUILayout.Box("", apEditorUtil.WhiteGUIStyle_Box, GUILayout.Width(14), GUILayout.Height(14));
					GUI.backgroundColor = prevColor;


					if (nSelectedVerts > 1 && (vertRigData._weight_Max - vertRigData._weight_Min) > 0.01f)
					{
						//여러개가 섞여서 Weight가 의미가 없어졌다.
						//Min + 로 표현하자
						//int iMin = (int)vertRigData._weight_Min;
						//int iMax = (int)vertRigData._weight_Max;
						//int iMin_Float = (int)(vertRigData._weight_Min * 10.0f + 0.5f) % 10;
						//int iMax_Float = (int)(vertRigData._weight_Max * 10.0f + 0.5f) % 10;

						strLabel = string.Format("{0:N2}~{1:N2}", vertRigData._weight_Min, vertRigData._weight_Max);
						//strLabel = ((int)vertRigData._weight_Min) + "." + ((int)(vertRigData._weight_Min * 10.0f + 0.5f) % 10)
						//	+ "~" + ((int)vertRigData._weight_Max) + "." + ((int)(vertRigData._weight_Max * 10.0f + 0.5f) % 10);
					}
					else
					{
						//Weight를 출력한다.
						//strLabel = ((int)vertRigData._weight) + "." + ((int)(vertRigData._weight * 1000.0f + 0.5f) % 1000);
						strLabel = string.Format("{0:N3}", vertRigData._weight);
					}

					string rigName = vertRigData._bone._name;
					if (rigName.Length > 14)
					{
						rigName = rigName.Substring(0, 12) + "..";
					}
					if (GUILayout.Button(rigName,
										curGUIStyle,
										GUILayout.Width(widthLabel_Name), GUILayout.Height(20)))
					{
						Editor.Select.SetBone(vertRigData._bone);
					}
					if (GUILayout.Button(strLabel,
										curGUIStyle,
										GUILayout.Width(60), GUILayout.Height(20)))
					{
						Editor.Select.SetBone(vertRigData._bone);
					}

					if (GUILayout.Button(imgRemove, curGUIStyle, GUILayout.Width(20), GUILayout.Height(20)))
					{
						//Debug.LogError("TODO : Bone Remove From Rigging");
						removeRigData = vertRigData;
					}

					EditorGUILayout.EndHorizontal();
				}
				else
				{
					//GUI 렌더 문제로 더미 렌더링
					EditorGUILayout.BeginHorizontal(GUILayout.Width(scrollWidth - 5));
					GUILayout.Space(5);

					GUILayout.Box("", GUILayout.Width(14), GUILayout.Height(14));

					if (GUILayout.Button("",
										guiNone,
										GUILayout.Width(widthLabel_Name), GUILayout.Height(20)))
					{
						//Dummy
					}
					if (GUILayout.Button("",
										guiNone,
										GUILayout.Width(60), GUILayout.Height(20)))
					{
						//Dummy
					}

					if (GUILayout.Button(imgRemove, guiNone, GUILayout.Width(20), GUILayout.Height(20)))
					{
						//Debug.LogError("TODO : Bone Remove From Rigging");
						//removeRigData = vertRigData;
						//Dummy
					}


					EditorGUILayout.EndHorizontal();
				}
			}


			if (removeRigData != null)
			{
				Editor.Controller.RemoveVertRigData(selectedVerts, removeRigData._bone);
			}

			EditorGUILayout.EndVertical();

			GUILayout.Space(120);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}




		private object _physicModifier_prevSelectedTransform = null;
		private bool _physicModifier_prevIsContained = false;

		private void DrawModifierPropertyGUI_Physics(int width, int height)
		{
			GUIStyle guiNone = new GUIStyle(GUIStyle.none);
			guiNone.normal.textColor = GUI.skin.label.normal.textColor;
			guiNone.alignment = TextAnchor.MiddleLeft;

			//"Target Mesh Transform"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.TargetMeshTransform), GUILayout.Width(width));
			//1. Mesh Transform 등록 체크
			//2. Weight 툴
			//3. Mesh Physics 툴

			bool isTarget_MeshTransform = Modifier.IsTarget_MeshTransform;
			bool isTarget_ChildMeshTransform = Modifier.IsTarget_ChildMeshTransform;

			bool isContainInParamSetGroup = false;
			string strTargetName = "";
			object selectedObj = null;
			bool isAnyTargetSelected = false;
			bool isAddable = false;

			apTransform_Mesh targetMeshTransform = SubMeshInGroup;
			apModifierParamSetGroup paramSetGroup = SubEditedParamSetGroup;
			if (paramSetGroup == null)
			{
				//? Physics에서는 1개의 ParamSetGroup이 있어야 한다.
				Editor.Controller.AddStaticParamSetGroupToModifier();

				if (Modifier._paramSetGroup_controller.Count > 0)
				{
					SetParamSetGroupOfModifier(Modifier._paramSetGroup_controller[0]);
				}
				paramSetGroup = SubEditedParamSetGroup;
				if (paramSetGroup == null)
				{
					Debug.LogError("AnyPortrait : ParamSet Group Is Null (" + Modifier._paramSetGroup_controller.Count + ")");
					return;
				}

				AutoSelectModMeshOrModBone();
			}

			apModifierParamSet paramSet = ParamSetOfMod;
			if (paramSet == null)
			{
				//Rigging에서는 1개의 ParamSetGroup과 1개의 ParamSet이 있어야 한다.
				//선택된게 없다면, ParamSet이 1개 있는지 확인
				//그후 선택한다.

				if (paramSetGroup._paramSetList.Count == 0)
				{
					paramSet = new apModifierParamSet();
					paramSet.LinkParamSetGroup(paramSetGroup);
					paramSetGroup._paramSetList.Add(paramSet);
				}
				else
				{
					paramSet = paramSetGroup._paramSetList[0];
				}
				SetParamSetOfModifier(paramSet);
			}

			//1. Mesh Transform 등록 체크
			if (targetMeshTransform != null)
			{
				apRenderUnit targetRenderUnit = null;
				//Child Mesh를 허용하는가
				if (isTarget_ChildMeshTransform)
				{
					//Child를 허용한다.
					targetRenderUnit = MeshGroup.GetRenderUnit(targetMeshTransform);
				}
				else
				{
					//Child를 허용하지 않는다.
					targetRenderUnit = MeshGroup.GetRenderUnit_NoRecursive(targetMeshTransform);
				}
				if (targetRenderUnit != null)
				{
					//유효한 선택인 경우
					isContainInParamSetGroup = paramSetGroup.IsMeshTransformContain(targetMeshTransform);
					isAnyTargetSelected = true;
					strTargetName = targetMeshTransform._nickName;
					selectedObj = targetMeshTransform;

					isAddable = true;
				}
			}

			

			Editor.SetGUIVisible("Modifier_Add Transform Check [Physic] Valid", targetMeshTransform != null);
			Editor.SetGUIVisible("Modifier_Add Transform Check [Physic] Invalid", targetMeshTransform == null);


			bool isMeshTransformValid = Editor.IsDelayedGUIVisible("Modifier_Add Transform Check [Physic] Valid");
			bool isMeshTransformInvalid = Editor.IsDelayedGUIVisible("Modifier_Add Transform Check [Physic] Invalid");

			
			bool isDummyTransform = false;

			if (!isMeshTransformValid && !isMeshTransformInvalid)
			{
				//둘중 하나는 true여야 GUI를 그릴 수 있다.
				isDummyTransform = true;//<<더미로 출력해야한다...
			}
			else
			{
				_physicModifier_prevSelectedTransform = targetMeshTransform;
				_physicModifier_prevIsContained = isContainInParamSetGroup;
			}



			Color prevColor = GUI.backgroundColor;

			GUIStyle boxGUIStyle = new GUIStyle(GUI.skin.box);
			boxGUIStyle.alignment = TextAnchor.MiddleCenter;
			boxGUIStyle.normal.textColor = apEditorUtil.BoxTextColor;

			if (targetMeshTransform == null)
			{
				//선택된 MeshTransform이 없다.
				GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				//"No Mesh is Selected"
				GUILayout.Box(Editor.GetUIWord(UIWORD.NoMeshIsSelected), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;

				if (isDummyTransform)
				{
					//"  Add Physics"
					if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.AddToPhysics), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_AddToPhysics)), GUILayout.Width(width), GUILayout.Height(25)))
					{
						//더미용 버튼
					}
				}
			}
			else if (isContainInParamSetGroup)
			{
				GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				//"[" + strTargetName + "]\nSelected"
				GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.Selected), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;

				if (!isDummyTransform)
				{
					//더미 처리 중이 아닐때 버튼이 등장한다
					//"  Remove From Physics".
					if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.RemoveFromPhysics), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_RemoveFromPhysics)), GUILayout.Width(width), GUILayout.Height(30)))
					{

						//bool result = EditorUtility.DisplayDialog("Remove From Physics", "Remove From Physics [" + strTargetName + "]", "Remove", "Cancel");

						bool result = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveFromPhysics_Title),
																Editor.GetTextFormat(TEXT.RemoveFromPhysics_Body, strTargetName),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);

						if (result)
						{
							object targetObj = SubMeshInGroup;
							if (SubMeshGroupInGroup != null && selectedObj == SubMeshGroupInGroup)
							{
								targetObj = SubMeshGroupInGroup;
							}

							apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_RemovePhysics, Editor, Modifier, targetObj, false);

							if (SubMeshInGroup != null && selectedObj == SubMeshInGroup)
							{
								SubEditedParamSetGroup.RemoveModifierMeshes(SubMeshInGroup);
								SetModMeshOfModifier(null);
							}
							else if (SubMeshGroupInGroup != null && selectedObj == SubMeshGroupInGroup)
							{
								SubEditedParamSetGroup.RemoveModifierMeshes(SubMeshGroupInGroup);
								SetModMeshOfModifier(null);

							}



							if (MeshGroup != null)
							{
								MeshGroup.RefreshModifierLink();
							}

							SetSubMeshGroupInGroup(null);
							SetSubMeshInGroup(null);

							Editor._portrait.LinkAndRefreshInEditor(false);
							AutoSelectModMeshOrModBone();

							SetModifierExclusiveEditing(EX_EDIT.None);

							if (ModMeshOfMod != null)
							{
								ModMeshOfMod.RefreshVertexWeights(Editor._portrait, true, false);
							}

							Editor.Hierarchy_MeshGroup.RefreshUnits();
							Editor.RefreshControllerAndHierarchy();

							Editor.SetRepaint();

							isContainInParamSetGroup = false;
						}
					}
				}
			}
			else if (!isAddable)
			{
				//추가 가능하지 않다.
				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				//"[" + strTargetName + "]\nNot able to be Added"
				GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.NotAbleToBeAdded), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;

				if (isDummyTransform)
				{
					if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.AddToPhysics), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_AddToPhysics)), GUILayout.Width(width), GUILayout.Height(25)))
					{
						//더미용 버튼
					}
				}
			}
			else
			{
				//아직 추가하지 않았다. 추가하자
				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				//"[" + strTargetName + "]\nNot Added to Edit"
				GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.NotAddedtoEdit), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;

				if (!isDummyTransform)
				{
					//더미 처리 중이 아닐때 버튼이 등장한다.
					if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.AddToPhysics), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_AddToPhysics)), GUILayout.Width(width), GUILayout.Height(30)))
					{
						Editor.Controller.AddModMesh_WithSubMeshOrSubMeshGroup();

						Editor.Hierarchy_MeshGroup.RefreshUnits();

						Editor.SetRepaint();
					}
				}
			}
			GUI.backgroundColor = prevColor;

			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);

			List<ModRenderVert> selectedVerts = Editor.Select.ModRenderVertListOfMod;
			//bool isAnyVertSelected = (selectedVerts != null && selectedVerts.Count > 0);

			bool isExEditMode = ExEditingMode != EX_EDIT.None;

			//2. Weight 툴
			// 선택한 Vertex
			// Set Weight, +/- Weight, * Weight
			// Blend
			// Grow, Shrink

			//어떤 Vertex가 선택되었는지 표기한다.
			if (!isAnyTargetSelected || selectedVerts.Count == 0)
			{
				//선택된게 없다.
				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				//"No Vetex is Selected"
				GUILayout.Box(Editor.GetUIWord(UIWORD.NoVertexisSelected), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(25));

				GUI.backgroundColor = prevColor;


			}
			else if (selectedVerts.Count == 1)
			{
				//1개의 Vertex
				GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				//"[Vertex " + selectedVerts[0]._renderVert._vertex._index + "] : " + selectedVerts[0]._modVertWeight._weight
				GUILayout.Box(string.Format("[ {0} {1} ] : {2}", Editor.GetUIWord(UIWORD.Vertex), selectedVerts[0]._renderVert._vertex._index, selectedVerts[0]._modVertWeight._weight), 
								boxGUIStyle, GUILayout.Width(width), GUILayout.Height(25));

				GUI.backgroundColor = prevColor;

			}
			else
			{
				GUI.backgroundColor = new Color(0.4f, 1.0f, 1.0f, 1.0f);
				//selectedVerts.Count + " Verts are Selected"
				GUILayout.Box(Editor.GetUIWordFormat(UIWORD.NumVertsareSelected, selectedVerts.Count), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(25));

				GUI.backgroundColor = prevColor;
			}
			int nSelectedVerts = selectedVerts.Count;

			bool isMainVert = false;
			bool isMainVertSwitchable = false;
			if (nSelectedVerts == 1)
			{
				if (selectedVerts[0]._modVertWeight._isEnabled)
				{
					isMainVert = selectedVerts[0]._modVertWeight._physicParam._isMain;
					isMainVertSwitchable = true;
				}
			}
			else if (nSelectedVerts > 1)
			{
				//전부다 MainVert인가
				bool isAllMainVert = true;
				for (int iVert = 0; iVert < selectedVerts.Count; iVert++)
				{
					if (!selectedVerts[iVert]._modVertWeight._physicParam._isMain)
					{
						isAllMainVert = false;
						break;
					}
				}
				isMainVert = isAllMainVert;
				isMainVertSwitchable = true;
			}
			//" Important Vertex", " Set Important",
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Physic_SetMainVertex),
												" " + Editor.GetUIWord(UIWORD.ImportantVertex), " " + Editor.GetUIWord(UIWORD.SetImportant),
												isMainVert, isMainVertSwitchable && isExEditMode, width, 25,
												"Force calculation is performed based on the [Important Vertex]"))
			{
				if (isMainVertSwitchable)
				{
					for (int i = 0; i < selectedVerts.Count; i++)
					{
						selectedVerts[i]._modVertWeight._physicParam._isMain = !isMainVert;
					}

					ModMeshOfMod.RefreshVertexWeights(Editor._portrait, true, false);
				}
			}

			//Weight Tool
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			GUILayout.Space(5);
			//고정된 Weight 값
			//0, 0.1, 0.3, 0.5, 0.7, 0.9, 1 (7개)
			int CALCULATE_SET = 0;
			int CALCULATE_ADD = 1;
			int CALCULATE_MULTIPLY = 2;

			int widthPresetWeight = ((width - 2 * 7) / 7) - 2;
			bool isPresetAdapt = false;
			float presetWeight = 0.0f;

			if (apEditorUtil.ToggledButton("0", false, isExEditMode, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.0f;
			}
			if (apEditorUtil.ToggledButton(".1", false, isExEditMode, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.1f;
			}
			if (apEditorUtil.ToggledButton(".3", false, isExEditMode, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.3f;
			}
			if (apEditorUtil.ToggledButton(".5", false, isExEditMode, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.5f;
			}
			if (apEditorUtil.ToggledButton(".7", false, isExEditMode, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.7f;
			}
			if (apEditorUtil.ToggledButton(".9", false, isExEditMode, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.9f;
			}
			if (apEditorUtil.ToggledButton("1", false, isExEditMode, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 1f;
			}
			EditorGUILayout.EndHorizontal();

			if (isPresetAdapt)
			{
				//고정 Weight를 지정하자
				Editor.Controller.SetPhyVolWeight(presetWeight, CALCULATE_SET);
				isPresetAdapt = false;
			}



			int heightSetWeight = 25;
			int widthSetBtn = 90;
			int widthIncDecBtn = 30;
			int widthValue = width - (widthSetBtn + widthIncDecBtn * 2 + 2 * 5 + 5);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightSetWeight));
			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(GUILayout.Width(widthValue), GUILayout.Height(heightSetWeight - 2));
			GUILayout.Space(8);
			_physics_setWeightValue = EditorGUILayout.DelayedFloatField(_physics_setWeightValue);
			EditorGUILayout.EndVertical();
			
			//"Set Weight"
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.SetWeight), false, isExEditMode, widthSetBtn, heightSetWeight))
			{
				Editor.Controller.SetPhyVolWeight(_physics_setWeightValue, CALCULATE_SET);
				GUI.FocusControl(null);
			}

			if (apEditorUtil.ToggledButton("+", false, isExEditMode, widthIncDecBtn, heightSetWeight))
			{
				////0.05 단위로 올라가거나 내려온다. (5%)
				Editor.Controller.SetPhyVolWeight(0.05f, CALCULATE_ADD);

				GUI.FocusControl(null);
			}
			if (apEditorUtil.ToggledButton("-", false, isExEditMode, widthIncDecBtn, heightSetWeight))
			{
				//0.05 단위로 올라가거나 내려온다. (5%)
				Editor.Controller.SetPhyVolWeight(-0.05f, CALCULATE_ADD);

				GUI.FocusControl(null);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(3);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightSetWeight));
			GUILayout.Space(5);


			EditorGUILayout.BeginVertical(GUILayout.Width(widthValue), GUILayout.Height(heightSetWeight - 2));
			GUILayout.Space(8);
			_physics_scaleWeightValue = EditorGUILayout.DelayedFloatField(_physics_scaleWeightValue);
			EditorGUILayout.EndVertical();

			//"Scale Weight"
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.ScaleWeight), false, isExEditMode, widthSetBtn, heightSetWeight))
			{
				Editor.Controller.SetPhyVolWeight(_physics_scaleWeightValue, CALCULATE_MULTIPLY);//Multiply 방식
				GUI.FocusControl(null);
			}

			if (apEditorUtil.ToggledButton("+", false, isExEditMode, widthIncDecBtn, heightSetWeight))
			{
				//x1.05
				//Debug.LogError("TODO : Physic Weight 적용 - x1.05");
				Editor.Controller.SetPhyVolWeight(1.05f, CALCULATE_MULTIPLY);//Multiply 방식

				GUI.FocusControl(null);
			}
			if (apEditorUtil.ToggledButton("-", false, isExEditMode, widthIncDecBtn, heightSetWeight))
			{
				//x0.95
				//Debug.LogError("TODO : Physic Weight 적용 - x0.95");
				//Editor.Controller.SetBoneWeight(0.95f, CALCULATE_MULTIPLY);//Multiply 방식
				Editor.Controller.SetPhyVolWeight(0.95f, CALCULATE_MULTIPLY);//Multiply 방식

				GUI.FocusControl(null);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(8);

			int heightToolBtn = 25;
			int width2Btn = (width - 5) / 2;
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Blend), " Blend", false, isExEditMode, width, heightToolBtn,
											"The weights of vertices are blended"))
			{
				//Blend
				Editor.Controller.SetPhyVolWeightBlend();
			}

			GUILayout.Space(5);


			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightToolBtn));
			GUILayout.Space(5);
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Grow), " Grow", false, isExEditMode, width2Btn, heightToolBtn,
											"Select more of the surrounding vertices"))
			{
				//Grow
				Editor.Controller.SelectVertexWeightGrowOrShrink(true);
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Shrink), " Shrink", false, isExEditMode, width2Btn, heightToolBtn,
											"Reduce selected vertices"))
			{
				//Shrink
				Editor.Controller.SelectVertexWeightGrowOrShrink(false);
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);

			//추가
			//Viscosity를 위한 그룹
			int viscosityGroupID = 0;
			bool isViscosityAvailable = false;
			if (isExEditMode && nSelectedVerts > 0)
			{
				for (int i = 0; i < selectedVerts.Count; i++)
				{
					viscosityGroupID |= selectedVerts[i]._modVertWeight._physicParam._viscosityGroupID;
				}
				isViscosityAvailable = true;
			}
			int iViscosityChanged = -1;
			bool isViscosityAdd = false;

			int heightVisTool = 20;
			int widthVisTool = ((width - 5) / 5) - 2;

			//5줄씩 총 10개 (0은 모두 0으로 만든다.)

			//"Viscosity Group ID"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ViscosityGroupID));
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightVisTool));
			GUILayout.Space(5);
			for (int i = 0; i < 10; i++)
			{
				if (i == 5)
				{
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightVisTool));
					GUILayout.Space(5);
				}


				string label = "";
				int iResult = 0;
				switch (i)
				{
					case 0:	label = "X";	iResult = 0;	break;
					case 1:	label = "1";	iResult = 1;	break;
					case 2:	label = "2";	iResult = 2;	break;
					case 3:	label = "3";	iResult = 4;	break;
					case 4:	label = "4";	iResult = 8;	break;
					case 5:	label = "5";	iResult = 16;	break;
					case 6:	label = "6";	iResult = 32;	break;
					case 7:	label = "7";	iResult = 64;	break;
					case 8:	label = "8";	iResult = 128;	break;
					case 9:	label = "9";	iResult = 256;	break;
				}
				bool isSelected = (viscosityGroupID & iResult) != 0;
				if (apEditorUtil.ToggledButton_2Side(label, label, isSelected, isViscosityAvailable, widthVisTool, heightVisTool))
				{
					iViscosityChanged = iResult;
					isViscosityAdd = !isSelected;
				}
			}
			EditorGUILayout.EndHorizontal();

			if (iViscosityChanged > -1)
			{
				Editor.Controller.SetPhysicsViscostyGroupID(iViscosityChanged, isViscosityAdd);
			}



			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);

			//메시 설정
			apPhysicsMeshParam physicMeshParam = null;
			if (ModMeshOfMod != null && ModMeshOfMod.PhysicParam != null)
			{
				physicMeshParam = ModMeshOfMod.PhysicParam;
			}
			if ((physicMeshParam == null && !isDummyTransform)
				|| (physicMeshParam != null && isDummyTransform))
			{
				//Mesh도 없고, Dummy도 없으면..
				//또는 Mesh가 있는데도 Dummy 판정이 났다면.. 
				return;
			}

			//여기서부턴 Dummy가 있으면 그 값을 이용한다.
			if (physicMeshParam != null)
			{
				isDummyTransform = false;
			}

			if (isDummyTransform && (_physicModifier_prevSelectedTransform == null || !_physicModifier_prevIsContained))
			{
				return;
			}

			int labelHeight = 30;

			apPhysicsPresetUnit presetUnit = null;
			if (!isDummyTransform)
			{
				if (physicMeshParam._presetID >= 0)
				{
					presetUnit = Editor.PhysicsPreset.GetPresetUnit(physicMeshParam._presetID);
					if (presetUnit == null)
					{
						physicMeshParam._presetID = -1;
					}
				}
			}
			//EditorGUILayout.LabelField("Physical Material");
			GUIStyle guiStyle_BoxStyle = new GUIStyle(GUI.skin.box);
			guiStyle_BoxStyle.alignment = TextAnchor.MiddleCenter;
			guiStyle_BoxStyle.normal.textColor = apEditorUtil.BoxTextColor;

			if (presetUnit != null)
			{
				bool isPropertySame = presetUnit.IsSameProperties(physicMeshParam);
				if (isPropertySame)
				{
					GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				}
				else
				{
					GUI.backgroundColor = new Color(0.4f, 1.0f, 1.1f, 1.0f);
				}

				GUILayout.Box(
					new GUIContent("  " + presetUnit._name,
									Editor.ImageSet.Get(apEditorUtil.GetPhysicsPresetIconType(presetUnit._icon))),
					guiStyle_BoxStyle, GUILayout.Width(width), GUILayout.Height(30));

				GUI.backgroundColor = prevColor;
			}
			else
			{
				//"Physical Material"
				GUILayout.Box(Editor.GetUIWord(UIWORD.PhysicalMaterial), guiStyle_BoxStyle, GUILayout.Width(width), GUILayout.Height(30));
			}

			GUILayout.Space(5);
			//TODO : Preset
			//값이 바뀌었으면 Dirty
			//"  Basic Setting"
			EditorGUILayout.LabelField(new GUIContent("  " + Editor.GetUIWord(UIWORD.BasicSetting), Editor.ImageSet.Get(apImageSet.PRESET.Physic_BasicSetting)), GUILayout.Height(labelHeight));
			
			float nextMass = EditorGUILayout.DelayedFloatField(Editor.GetUIWord(UIWORD.Mass), (!isDummyTransform) ? physicMeshParam._mass : 0.0f);//"Mass"
			float nextDamping = EditorGUILayout.DelayedFloatField(Editor.GetUIWord(UIWORD.Damping), (!isDummyTransform) ? physicMeshParam._damping : 0.0f);//"Damping"
			float nextAirDrag = EditorGUILayout.DelayedFloatField(Editor.GetUIWord(UIWORD.AirDrag), (!isDummyTransform) ? physicMeshParam._airDrag : 0.0f);//"Air Drag"
			bool nextIsRestrictMoveRange = EditorGUILayout.Toggle(Editor.GetUIWord(UIWORD.SetMoveRange), (!isDummyTransform) ? physicMeshParam._isRestrictMoveRange : false);//"Set Move Range"
			float nextMoveRange = (!isDummyTransform) ? physicMeshParam._moveRange : 0.0f;
			if (nextIsRestrictMoveRange)
			{
				nextMoveRange = EditorGUILayout.DelayedFloatField(Editor.GetUIWord(UIWORD.MoveRange), (!isDummyTransform) ? physicMeshParam._moveRange : 0.0f);//"Move Range"
			}
			else
			{
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.MoveRangeUnlimited));//"Move Range : Unlimited"
			}

			GUILayout.Space(5);

			int valueWidth = 74;//캬... 꼼꼼하다
			int labelWidth = width - (valueWidth + 2 + 5);
			int leftMargin = 3;
			int topMargin = 10;


			EditorGUILayout.LabelField(new GUIContent("  " + Editor.GetUIWord(UIWORD.Stretchiness), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Stretch)), GUILayout.Height(labelHeight));//"  Stretchiness"
			EditorGUILayout.BeginVertical(GUILayout.Width(valueWidth), GUILayout.Height(labelHeight));
			float nextStretchK = EditorGUILayout.DelayedFloatField(Editor.GetUIWord(UIWORD.K_Value), (!isDummyTransform) ? physicMeshParam._stretchK : 0.0f);//"K-Value"
			bool nextIsRestrictStretchRange = EditorGUILayout.Toggle(Editor.GetUIWord(UIWORD.SetStretchRange), (!isDummyTransform) ? physicMeshParam._isRestrictStretchRange : false);//"Set Stretch Range"
			float nextStretchRange_Max = (!isDummyTransform) ? physicMeshParam._stretchRangeRatio_Max : 0.0f;
			if (nextIsRestrictStretchRange)
			{
				//"Lengthen Ratio"
				nextStretchRange_Max = EditorGUILayout.DelayedFloatField(Editor.GetUIWord(UIWORD.LengthenRatio), (!isDummyTransform) ? physicMeshParam._stretchRangeRatio_Max : 0.0f);
			}
			else
			{
				//"Lengthen Ratio : Unlimited"
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.LengthenRatioUnlimited));
			}
			GUILayout.Space(5);


			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelHeight));
			GUILayout.Space(leftMargin);
			//"  Inertia"
			EditorGUILayout.LabelField(new GUIContent("  " + Editor.GetUIWord(UIWORD.Inertia), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Inertia)), GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));
			EditorGUILayout.BeginVertical(GUILayout.Width(valueWidth), GUILayout.Height(labelHeight));
			GUILayout.Space(topMargin);
			float nextInertiaK = EditorGUILayout.DelayedFloatField((!isDummyTransform) ? physicMeshParam._inertiaK : 0.0f, GUILayout.Width(valueWidth));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelHeight));
			GUILayout.Space(leftMargin);
			//"  Restoring"
			EditorGUILayout.LabelField(new GUIContent("  " + Editor.GetUIWord(UIWORD.Restoring), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Recover)), GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));
			EditorGUILayout.BeginVertical(GUILayout.Width(valueWidth), GUILayout.Height(labelHeight));
			GUILayout.Space(topMargin);
			float nextRestoring = EditorGUILayout.DelayedFloatField((!isDummyTransform) ? physicMeshParam._restoring : 0.0f, GUILayout.Width(valueWidth));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);


			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelHeight));
			GUILayout.Space(leftMargin);
			//"  Viscosity"
			EditorGUILayout.LabelField(new GUIContent("  " + Editor.GetUIWord(UIWORD.Viscosity), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Viscosity)), GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));
			EditorGUILayout.BeginVertical(GUILayout.Width(valueWidth), GUILayout.Height(labelHeight));
			GUILayout.Space(topMargin);
			float nextViscosity = EditorGUILayout.DelayedFloatField((!isDummyTransform) ? physicMeshParam._viscosity : 0.0f, GUILayout.Width(valueWidth));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);

			//Bend는 삭제 -> Elastic을 추가한다.
			//EditorGUILayout.LabelField(new GUIContent("  Bendiness", Editor.ImageSet.Get(apImageSet.PRESET.Physic_Bend)), GUILayout.Height(labelHeight));
			//physicMeshParam._bendK = EditorGUILayout.DelayedFloatField("Constant", physicMeshParam._bendK);
			//physicMeshParam._bendRange = EditorGUILayout.DelayedFloatField("Range", physicMeshParam._bendRange);


			//값이 바뀌었으면 적용
			if (!isDummyTransform)
			{
				if (nextMass != physicMeshParam._mass
					|| nextDamping != physicMeshParam._damping
					|| nextAirDrag != physicMeshParam._airDrag
					|| nextMoveRange != physicMeshParam._moveRange
					|| nextStretchK != physicMeshParam._stretchK
					//|| nextStretchRange_Min != physicMeshParam._stretchRangeRatio_Min
					|| nextStretchRange_Max != physicMeshParam._stretchRangeRatio_Max
					|| nextInertiaK != physicMeshParam._inertiaK
					|| nextRestoring != physicMeshParam._restoring
					|| nextViscosity != physicMeshParam._viscosity
					|| nextIsRestrictStretchRange != physicMeshParam._isRestrictStretchRange
					|| nextIsRestrictMoveRange != physicMeshParam._isRestrictMoveRange)
				{
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Modifier, ModMeshOfMod, false);

					physicMeshParam._mass = nextMass;
					physicMeshParam._damping = nextDamping;
					physicMeshParam._airDrag = nextAirDrag;
					physicMeshParam._moveRange = nextMoveRange;
					physicMeshParam._stretchK = nextStretchK;

					//physicMeshParam._stretchRangeRatio_Min = Mathf.Clamp01(nextStretchRange_Min);
					physicMeshParam._stretchRangeRatio_Max = nextStretchRange_Max;
					if (physicMeshParam._stretchRangeRatio_Max < 0.0f)
					{
						physicMeshParam._stretchRangeRatio_Max = 0.0f;
					}

					physicMeshParam._isRestrictStretchRange = nextIsRestrictStretchRange;
					physicMeshParam._isRestrictMoveRange = nextIsRestrictMoveRange;


					physicMeshParam._inertiaK = nextInertiaK;
					physicMeshParam._restoring = nextRestoring;
					physicMeshParam._viscosity = nextViscosity;

					apEditorUtil.ReleaseGUIFocus();
				}
			}

			//GUILayout.Space(5);

			EditorGUILayout.LabelField(new GUIContent("  " + Editor.GetUIWord(UIWORD.Gravity), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Gravity)), GUILayout.Height(labelHeight));//"  Gravity"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.InputType));//"Input Type"
			apPhysicsMeshParam.ExternalParamType nextGravityParam = (apPhysicsMeshParam.ExternalParamType)EditorGUILayout.EnumPopup((!isDummyTransform) ? physicMeshParam._gravityParamType : apPhysicsMeshParam.ExternalParamType.Constant);

			Vector2 nextGravityConstValue = (!isDummyTransform) ? physicMeshParam._gravityConstValue : Vector2.zero;

			apPhysicsMeshParam.ExternalParamType curGravityParam = (physicMeshParam != null) ? physicMeshParam._gravityParamType : apPhysicsMeshParam.ExternalParamType.Constant;

			if (curGravityParam == apPhysicsMeshParam.ExternalParamType.Constant)
			{
				nextGravityConstValue = apEditorUtil.DelayedVector2Field((!isDummyTransform) ? physicMeshParam._gravityConstValue : Vector2.zero, width - 4);
			}
			else
			{
				//?
				//TODO : GravityControlParam 링크할 것
				apControlParam controlParam = physicMeshParam._gravityControlParam;
				if (controlParam == null && physicMeshParam._gravityControlParamID > 0)
				{
					physicMeshParam._gravityControlParam = Editor._portrait._controller.FindParam(physicMeshParam._gravityControlParamID);
					controlParam = physicMeshParam._gravityControlParam;
					if (controlParam == null)
					{
						physicMeshParam._gravityControlParamID = -1;
					}
				}

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);
				if (controlParam != null)
				{
					GUI.backgroundColor = new Color(0.5f, 1.0f, 1.0f, 1.0f);
					GUILayout.Box("[" + controlParam._keyName + "]", boxGUIStyle, GUILayout.Width(width - 34), GUILayout.Height(25));

					GUI.backgroundColor = prevColor;
				}
				else
				{
					GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
					GUILayout.Box(Editor.GetUIWord(UIWORD.NoControlParam), boxGUIStyle, GUILayout.Width(width - 34), GUILayout.Height(25));//"No ControlParam

					GUI.backgroundColor = prevColor;
				}

				if (GUILayout.Button(Editor.GetUIWord(UIWORD.Set), GUILayout.Width(30), GUILayout.Height(25)))//"Set"
				{
					//Control Param을 선택하는 Dialog를 호출하자
					_loadKey_SelectControlParamToPhyGravity = apDialog_SelectControlParam.ShowDialog(Editor, apDialog_SelectControlParam.PARAM_TYPE.Vector2, OnSelectControlParamToPhysicGravity);
				}
				EditorGUILayout.EndHorizontal();
			}

			GUILayout.Space(5);

			EditorGUILayout.LabelField(new GUIContent("  " + Editor.GetUIWord(UIWORD.Wind), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Wind)), GUILayout.Height(labelHeight));//"  Wind"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.InputType));//"Input Type"
			apPhysicsMeshParam.ExternalParamType nextWindParamType = (apPhysicsMeshParam.ExternalParamType)EditorGUILayout.EnumPopup((!isDummyTransform) ? physicMeshParam._windParamType : apPhysicsMeshParam.ExternalParamType.Constant);

			Vector2 nextWindConstValue = (!isDummyTransform) ? physicMeshParam._windConstValue : Vector2.zero;
			Vector2 nextWindRandomRange = (!isDummyTransform) ? physicMeshParam._windRandomRange : Vector2.zero;

			apPhysicsMeshParam.ExternalParamType curWindParamType = (physicMeshParam != null) ? physicMeshParam._windParamType : apPhysicsMeshParam.ExternalParamType.Constant;

			if (curWindParamType == apPhysicsMeshParam.ExternalParamType.Constant)
			{
				nextWindConstValue = apEditorUtil.DelayedVector2Field((!isDummyTransform) ? physicMeshParam._windConstValue : Vector2.zero, width - 4);
			}
			else
			{
				//?
				//TODO : GravityControlParam 링크할 것
				apControlParam controlParam = physicMeshParam._windControlParam;
				if (controlParam == null && physicMeshParam._windControlParamID > 0)
				{
					physicMeshParam._windControlParam = Editor._portrait._controller.FindParam(physicMeshParam._windControlParamID);
					controlParam = physicMeshParam._windControlParam;
					if (controlParam == null)
					{
						physicMeshParam._windControlParamID = -1;
					}
				}

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);
				if (controlParam != null)
				{
					GUI.backgroundColor = new Color(0.5f, 1.0f, 1.0f, 1.0f);
					GUILayout.Box("[" + controlParam._keyName + "]", boxGUIStyle, GUILayout.Width(width - 34), GUILayout.Height(25));

					GUI.backgroundColor = prevColor;
				}
				else
				{
					GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
					GUILayout.Box(Editor.GetUIWord(UIWORD.NoControlParam), boxGUIStyle, GUILayout.Width(width - 34), GUILayout.Height(25));//"No ControlParam"

					GUI.backgroundColor = prevColor;
				}

				if (GUILayout.Button(Editor.GetUIWord(UIWORD.Set), GUILayout.Width(30), GUILayout.Height(25)))//"Set"
				{
					//Control Param을 선택하는 Dialog를 호출하자
					_loadKey_SelectControlParamToPhyWind = apDialog_SelectControlParam.ShowDialog(Editor, apDialog_SelectControlParam.PARAM_TYPE.Vector2, OnSelectControlParamToPhysicWind);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.WindRandomRangeSize));//"Wind Random Range Size"
			nextWindRandomRange = apEditorUtil.DelayedVector2Field((!isDummyTransform) ? physicMeshParam._windRandomRange : Vector2.zero, width - 4);

			GUILayout.Space(10);




			//Preset 창을 열자
			//" Physics Presets"
			if (apEditorUtil.ToggledButton_2Side(	Editor.ImageSet.Get(apImageSet.PRESET.Physic_Palette), 
													" " + Editor.GetUIWord(UIWORD.PhysicsPresets), 
													" " + Editor.GetUIWord(UIWORD.PhysicsPresets), 
													false, physicMeshParam != null, width, 32))
			{
				_loadKey_SelectPhysicsParam = apDialog_PhysicsPreset.ShowDialog(Editor, ModMeshOfMod, OnSelectPhysicsPreset);
			}


			if (!isDummyTransform)
			{
				if (nextGravityParam != physicMeshParam._gravityParamType
					|| nextGravityConstValue.x != physicMeshParam._gravityConstValue.x
					|| nextGravityConstValue.y != physicMeshParam._gravityConstValue.y
					|| nextWindParamType != physicMeshParam._windParamType
					|| nextWindConstValue.x != physicMeshParam._windConstValue.x
					|| nextWindConstValue.y != physicMeshParam._windConstValue.y
					|| nextWindRandomRange.x != physicMeshParam._windRandomRange.x
					|| nextWindRandomRange.y != physicMeshParam._windRandomRange.y
					)
				{
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Modifier, ModMeshOfMod, false);

					physicMeshParam._gravityParamType = nextGravityParam;
					physicMeshParam._gravityConstValue = nextGravityConstValue;
					physicMeshParam._windParamType = nextWindParamType;
					physicMeshParam._windConstValue = nextWindConstValue;
					physicMeshParam._windRandomRange = nextWindRandomRange;
					apEditorUtil.ReleaseGUIFocus();
				}
			}
		}


		//Physic Modifier에서 Gravity/Wind를 Control Param에 연결할 때, Dialog를 열어서 선택하도록 한다.
		private object _loadKey_SelectControlParamToPhyGravity = null;
		public void OnSelectControlParamToPhysicGravity(bool isSuccess, object loadKey, apControlParam resultControlParam)
		{
			Debug.Log("Select Control Param : OnSelectControlParamToPhysicGravity (" + isSuccess + ")");
			if (_loadKey_SelectControlParamToPhyGravity != loadKey || !isSuccess)
			{
				Debug.LogError("AnyPortrait : Wrong loadKey");
				_loadKey_SelectControlParamToPhyGravity = null;
				return;
			}

			_loadKey_SelectControlParamToPhyGravity = null;
			if (Modifier == null
				|| (Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics) == 0
				|| ModMeshOfMod == null)
			{
				return;
			}
			if (ModMeshOfMod.PhysicParam == null)
			{
				return;
			}

			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Modifier, ModMeshOfMod, false);

			ModMeshOfMod.PhysicParam._gravityControlParam = resultControlParam;
			if (resultControlParam == null)
			{
				ModMeshOfMod.PhysicParam._gravityControlParamID = -1;
			}
			else
			{
				ModMeshOfMod.PhysicParam._gravityControlParamID = resultControlParam._uniqueID;
			}
		}

		private object _loadKey_SelectControlParamToPhyWind = null;
		public void OnSelectControlParamToPhysicWind(bool isSuccess, object loadKey, apControlParam resultControlParam)
		{
			if (_loadKey_SelectControlParamToPhyWind != loadKey || !isSuccess)
			{
				_loadKey_SelectControlParamToPhyWind = null;
				return;
			}

			_loadKey_SelectControlParamToPhyWind = null;
			if (Modifier == null
				|| (Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics) == 0
				|| ModMeshOfMod == null)
			{
				return;
			}
			if (ModMeshOfMod.PhysicParam == null)
			{
				return;
			}

			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Modifier, ModMeshOfMod, false);

			ModMeshOfMod.PhysicParam._windControlParam = resultControlParam;
			if (resultControlParam == null)
			{
				ModMeshOfMod.PhysicParam._windControlParamID = -1;
			}
			else
			{
				ModMeshOfMod.PhysicParam._windControlParamID = resultControlParam._uniqueID;
			}
		}

		private object _loadKey_SelectPhysicsParam = null;
		private void OnSelectPhysicsPreset(bool isSuccess, object loadKey, apPhysicsPresetUnit physicsUnit, apModifiedMesh targetModMesh)
		{
			if (!isSuccess || physicsUnit == null || targetModMesh == null || loadKey != _loadKey_SelectPhysicsParam || targetModMesh != ModMeshOfMod)
			{
				_loadKey_SelectPhysicsParam = null;
				return;
			}
			_loadKey_SelectPhysicsParam = null;
			if (targetModMesh.PhysicParam == null || SelectionType != SELECTION_TYPE.MeshGroup)
			{
				return;
			}
			//값 복사를 해주자
			
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SetPhysicsProperty, Editor, Modifier, null, false);

			apPhysicsMeshParam physicsMeshParam = targetModMesh.PhysicParam;

			physicsMeshParam._presetID = physicsUnit._uniqueID;
			physicsMeshParam._moveRange = physicsUnit._moveRange;

			physicsMeshParam._isRestrictMoveRange = physicsUnit._isRestrictMoveRange;
			physicsMeshParam._isRestrictStretchRange = physicsUnit._isRestrictStretchRange;

			//physicsMeshParam._stretchRangeRatio_Min = physicsUnit._stretchRange_Min;
			physicsMeshParam._stretchRangeRatio_Max = physicsUnit._stretchRange_Max;
			physicsMeshParam._stretchK = physicsUnit._stretchK;
			physicsMeshParam._inertiaK = physicsUnit._inertiaK;
			physicsMeshParam._damping = physicsUnit._damping;
			physicsMeshParam._mass = physicsUnit._mass;

			physicsMeshParam._gravityConstValue = physicsUnit._gravityConstValue;
			physicsMeshParam._windConstValue = physicsUnit._windConstValue;
			physicsMeshParam._windRandomRange = physicsUnit._windRandomRange;

			physicsMeshParam._airDrag = physicsUnit._airDrag;
			physicsMeshParam._viscosity = physicsUnit._viscosity;
			physicsMeshParam._restoring = physicsUnit._restoring;

		}

		private object _prevSelectedAnimObject = null;
		private object _prevSelectedAnimTimeline = null;
		private object _prevSelectedAnimTimelineLayer = null;
		private bool _isIgnoreAnimTimelineGUI = false;
		// Animation Right 2 GUI
		//------------------------------------------------------------------------------------
		private void DrawEditor_Right2_Animation(int width, int height)
		{



			// 상단부는 AnimClip의 정보를 출력하며,
			// 하단부는 선택된 Timeline의 정보를 출력한다.

			// AnimClip 정보 출력 부분

			Editor.SetGUIVisible("AnimationRight2GUI_AnimClip", (AnimClip != null));
			Editor.SetGUIVisible("AnimationRight2GUI_Timeline", (AnimTimeline != null));

			if (AnimClip == null)
			{
				return;
			}

			if (!Editor.IsDelayedGUIVisible("AnimationRight2GUI_AnimClip"))
			{
				//아직 출력하면 안된다.
				return;
			}

			apAnimClip animClip = AnimClip;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(50));
			GUILayout.Space(10);

			EditorGUILayout.LabelField(
				new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation)),
				GUILayout.Width(50), GUILayout.Height(50));

			EditorGUILayout.BeginVertical(GUILayout.Width(width - (50 + 10)));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(animClip._name, GUILayout.Width(width - (50 + 10)));

			if (animClip._targetMeshGroup != null)
			{
				//"Target : " + animClip._targetMeshGroup._name
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Target) + " : " + animClip._targetMeshGroup._name, GUILayout.Width(width - (50 + 10)));
			}
			else
			{
				EditorGUILayout.LabelField(string.Format("{0} : {1}", Editor.GetUIWord(UIWORD.Target), Editor.GetUIWord(UIWORD.NoMeshGroup)), GUILayout.Width(width - (50 + 10)));
			}


			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			//애니메이션 기본 정보
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.AnimationSettings), GUILayout.Width(width));//"Animation Settings"
			GUILayout.Space(2);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.StartFrame), GUILayout.Width(110));//"Start Frame"
			int nextStartFrame = EditorGUILayout.DelayedIntField(animClip.StartFrame, GUILayout.Width(width - (110 + 5)));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.EndFrame), GUILayout.Width(110));//"End Frame"
			int nextEndFrame = EditorGUILayout.DelayedIntField(animClip.EndFrame, GUILayout.Width(width - (110 + 5)));
			EditorGUILayout.EndHorizontal();

			bool isNextLoop = animClip.IsLoop;
			//" Loop On", " Loop Off"
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Anim_Loop),
													" " + Editor.GetUIWord(UIWORD.LoopOn),
													" " + Editor.GetUIWord(UIWORD.LoopOff),
													animClip.IsLoop, true, width, 24))
			{
				isNextLoop = !animClip.IsLoop;
				//값 적용은 아래에서
			}

			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("FPS", GUILayout.Width(110));//<<이건 고정
			int nextFPS = EditorGUILayout.DelayedIntField(animClip.FPS, GUILayout.Width(width - (110 + 5)));
			//int nextFPS = EditorGUILayout.IntSlider("FPS", animClip._FPS, 1, 240, GUILayout.Width(width));
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			//추가 : 애니메이션 이벤트
			int nAnimEvents = 0;
			if (animClip._animEvents != null)
			{
				nAnimEvents = animClip._animEvents.Count;
			}

			//"Animation Events..
			if (GUILayout.Button(string.Format("{0} [{1}]", Editor.GetUIWord(UIWORD.AnimationEvents), nAnimEvents), GUILayout.Height(22)))
			{
				apDialog_AnimationEvents.ShowDialog(Editor, Editor._portrait, animClip);
			}

			//" Export/Import"
			if (GUILayout.Button(new GUIContent(" " + Editor.GetUIWord(UIWORD.ExportImport), Editor.ImageSet.Get(apImageSet.PRESET.Anim_Save)), GUILayout.Height(22)))
			{
				//AnimClip을 Export/Import 하자
				_loadKey_ImportAnimClipRetarget = apDialog_RetargetPose.ShowDialog(Editor, _animClip._targetMeshGroup, _animClip, OnImportAnimClipRetarget);
			}

			if (nextStartFrame != animClip.StartFrame
				|| nextEndFrame != animClip.EndFrame
				|| nextFPS != animClip.FPS
				|| isNextLoop != animClip.IsLoop)
			{
				//바뀌었다면 타임라인 GUI를 세팅할 필요가 있을 수 있다.
				//Debug.Log("Anim Setting Changed");

				apEditorUtil.SetEditorDirty();

				//Start Frame과 Next Frame의 값이 뒤집혀져있는지 확인
				if (nextStartFrame > nextEndFrame)
				{
					int tmp = nextStartFrame;
					nextStartFrame = nextEndFrame;
					nextEndFrame = tmp;
				}

				animClip.SetOption_StartFrame(nextStartFrame);
				animClip.SetOption_EndFrame(nextEndFrame);
				animClip.SetOption_FPS(nextFPS);
				animClip.SetOption_IsLoop(isNextLoop);
			}



			GUILayout.Space(20);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(20);


			// Timeline 정보 출력 부분

			if (AnimTimeline == null)
			{
				return;
			}

			if (!Editor.IsDelayedGUIVisible("AnimationRight2GUI_Timeline"))
			{
				//아직 출력하면 안된다.
				return;
			}

			apAnimTimeline animTimeline = AnimTimeline;
			apAnimTimelineLayer animTimelineLayer = AnimTimelineLayer;


			//Timeline 정보 출력
			Texture2D iconTimeline = null;
			switch (animTimeline._linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					iconTimeline = Editor.ImageSet.Get(apImageSet.PRESET.Anim_WithMod);
					break;

				//case apAnimClip.LINK_TYPE.Bone:
				//	iconTimeline = Editor.ImageSet.Get(apImageSet.PRESET.Anim_WithBone);
				//	break;

				case apAnimClip.LINK_TYPE.ControlParam:
					iconTimeline = Editor.ImageSet.Get(apImageSet.PRESET.Anim_WithControlParam);
					break;

				default:
					iconTimeline = Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy);//<<이상한 걸 넣어서 나중에 수정할 수 있게 하자
					break;
			}

			//1. 아이콘 / 타입
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(50));
			GUILayout.Space(10);

			EditorGUILayout.LabelField(new GUIContent(iconTimeline), GUILayout.Width(50), GUILayout.Height(50));

			EditorGUILayout.BeginVertical(GUILayout.Width(width - (50 + 10)));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(animTimeline.DisplayName, GUILayout.Width(width - (50 + 10)));


			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			//GUILayout.Space(10);


			//현재 선택한 객체를 레이어로 만들 수 있다.
			//상태 : 선택한게 없다. / 선택은 했으나 레이어에 등록이 안되었다. (등록할 수 있다) / 선택한게 이미 등록한 객체다. (
			//bool isAnyTargetObjectSelected = false;
			bool isAddableType = false;
			bool isAddable = false;
			string targetObjectName = "";
			object targetObject = null;
			bool isAddingLayerOnce = false;
			bool isAddChildTransformAddable = false;
			switch (animTimeline._linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					//Transform이 속해있는지 확인하자
					if (SubMeshTransformOnAnimClip != null)
					{
						//isAnyTargetObjectSelected = true;
						targetObjectName = SubMeshTransformOnAnimClip._nickName;
						targetObject = SubMeshTransformOnAnimClip;

						//레이어로 등록가능한가
						isAddableType = animTimeline.IsLayerAddableType(SubMeshTransformOnAnimClip);
						isAddable = !animTimeline.IsObjectAddedInLayers(SubMeshTransformOnAnimClip);
					}
					else if (SubMeshGroupTransformOnAnimClip != null)
					{
						//isAnyTargetObjectSelected = true;
						targetObjectName = SubMeshGroupTransformOnAnimClip._nickName;
						targetObject = SubMeshGroupTransformOnAnimClip;

						//레이어로 등록가능한가.
						isAddableType = animTimeline.IsLayerAddableType(SubMeshGroupTransformOnAnimClip);
						isAddable = !animTimeline.IsObjectAddedInLayers(SubMeshGroupTransformOnAnimClip);
					}
					else if (Bone != null)
					{
						//isAnyTargetObjectSelected = true;
						targetObjectName = Bone._name;
						targetObject = Bone;

						isAddableType = animTimeline.IsLayerAddableType(Bone);
						isAddable = !animTimeline.IsObjectAddedInLayers(Bone);
					}
					isAddingLayerOnce = true;//한번에 레이어를 추가할 수 있다.
					isAddChildTransformAddable = animTimeline._linkedModifier.IsTarget_ChildMeshTransform;
					break;


				case apAnimClip.LINK_TYPE.ControlParam:
					if (SubControlParamOnAnimClip != null)
					{
						//isAnyTargetObjectSelected = true;
						targetObjectName = SubControlParamOnAnimClip._keyName;
						targetObject = SubControlParamOnAnimClip;

						isAddableType = animTimeline.IsLayerAddableType(SubControlParamOnAnimClip);
						isAddable = !animTimeline.IsObjectAddedInLayers(SubControlParamOnAnimClip);
					}

					isAddingLayerOnce = false;
					break;
			}
			bool isRemoveTimeline = false;

			bool isRemoveTimelineLayer = false;
			apAnimTimelineLayer removeLayer = null;

			//추가 : 추가 가능한 모든 객체에 대해서 TimelineLayer를 추가한다.
			if (isAddingLayerOnce)
			{
				string strTargetObject = "";
				bool isTargetTF = true;
				Texture2D addIconImage = null;
				if (_meshGroupChildHierarchy_Anim == MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
				{
					strTargetObject = Editor.GetUIWord(UIWORD.Meshes);//"Meshes"
					isTargetTF = true;
					addIconImage = Editor.ImageSet.Get(apImageSet.PRESET.Anim_AddAllMeshesToLayer);
				}
				else
				{
					strTargetObject = Editor.GetUIWord(UIWORD.Bones);//"Bones"
					isTargetTF = false;
					addIconImage = Editor.ImageSet.Get(apImageSet.PRESET.Anim_AddAllBonesToLayer);
				}


				if (GUILayout.Button(new GUIContent(Editor.GetUIWordFormat(UIWORD.AllObjectToLayers, strTargetObject), addIconImage), GUILayout.Height(30)))
				{
					//bool isResult = EditorUtility.DisplayDialog("Add to Timelines", "All " + strTargetObject + " are added to Timeline Layers?", "Add All", "Cancel");

					bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.AddAllObjects2Timeline_Title),
																Editor.GetTextFormat(TEXT.AddAllObjects2Timeline_Body, strTargetObject),
																Editor.GetText(TEXT.Okay),
																Editor.GetText(TEXT.Cancel)
																);

					if (isResult)
					{
						//모든 객체를 TimelineLayer로 등록한다.
						Editor.Controller.AddAnimTimelineLayerForAllTransformObject(animClip._targetMeshGroup,
																						isTargetTF,
																						isAddChildTransformAddable,
																						animTimeline);
					}
				}
			}

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//"  Remove Timeline"
			if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.RemoveTimeline),
													Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
													),
									GUILayout.Height(24)))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Timeline", "Is Really Remove Timeline?", "Remove", "Cancel");

				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveTimeline_Title),
																Editor.GetTextFormat(TEXT.RemoveTimeline_Body, animTimeline.DisplayName),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);

				if (isResult)
				{
					isRemoveTimeline = true;
				}
			}
			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(20);

			Color prevColor = GUI.backgroundColor;



			//Editor.SetGUIVisible("AnimationRight2GUI_Timeline_SelectedObject", _prevSelectedAnimObject == targetObject || _isIgnoreAnimTimelineGUI);
			Editor.SetGUIVisible("AnimationRight2GUI_Timeline_SelectedObject", (_prevSelectedAnimObject != null) == (targetObject != null));
			bool isGUI_TargetSelected = Editor.IsDelayedGUIVisible("AnimationRight2GUI_Timeline_SelectedObject");

			Editor.SetGUIVisible("AnimationRight2GUI_Timeline_Layers", (_prevSelectedAnimTimeline == _subAnimTimeline && _prevSelectedAnimTimelineLayer == _subAnimTimelineLayer) || _isIgnoreAnimTimelineGUI);
			bool isGUI_SameLayer = Editor.IsDelayedGUIVisible("AnimationRight2GUI_Timeline_Layers");
			

			if (Event.current.type == EventType.Repaint && Event.current.type != EventType.Ignore)
			{
				_prevSelectedAnimTimeline = _subAnimTimeline;
				_prevSelectedAnimTimelineLayer = _subAnimTimelineLayer;
				_prevSelectedAnimObject = targetObject;

				if (_isIgnoreAnimTimelineGUI)
				{
					_isIgnoreAnimTimelineGUI = false;
				}
			}
			// -----------------------------------------
			if (isGUI_TargetSelected)
			{
				GUIStyle boxGUIStyle = new GUIStyle(GUI.skin.box);
				boxGUIStyle.alignment = TextAnchor.MiddleCenter;
				boxGUIStyle.normal.textColor = apEditorUtil.BoxTextColor;

				if (isAddableType)
				{

					if (isAddable)
					{
						//아직 레이어로 추가가 되지 않았다.
						GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
						//"[" + targetObjectName + "]\nNot Added to Edit"
						GUILayout.Box("[" + targetObjectName + "]\n" + Editor.GetUIWord(UIWORD.NotAddedtoEdit), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

						GUI.backgroundColor = prevColor;

						//"  Add Timeline Layer to Edit"
						if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.AddTimelineLayerToEdit), Editor.ImageSet.Get(apImageSet.PRESET.Anim_AddTimeline)), GUILayout.Height(35)))
						{
							//Debug.LogError("TODO ; Layer 추가하기");
							Editor.Controller.AddAnimTimelineLayer(targetObject, animTimeline);
						}
					}
					else
					{
						//레이어에 이미 있다.
						GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
						//"[" + targetObjectName + "]\nSelected"
						GUILayout.Box("[" + targetObjectName + "]\n" + Editor.GetUIWord(UIWORD.Selected), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

						GUI.backgroundColor = prevColor;


						//"  Remove Timeline Layer"
						if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.RemoveTimelineLayer),
												Editor.ImageSet.Get(apImageSet.PRESET.Anim_RemoveTimelineLayer)
												),
								GUILayout.Height(24)))
						{
							//bool isResult = EditorUtility.DisplayDialog("Remove TimelineLayer", "Is Really Remove Timeline Layer?", "Remove", "Cancel");

							bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveTimelineLayer_Title),
															Editor.GetTextFormat(TEXT.RemoveTimelineLayer_Body, animTimelineLayer.DisplayName),
															Editor.GetText(TEXT.Remove),
															Editor.GetText(TEXT.Cancel)
															);

							if (isResult)
							{
								isRemoveTimelineLayer = true;
								removeLayer = animTimelineLayer;
							}
						}
					}
				}
				else
				{
					if (targetObject != null)
					{
						//추가할 수 있는 타입이 아니다.
						GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
						//"[" + targetObjectName + "]\nUnable to be Added"
						GUILayout.Box("[" + targetObjectName + "]\n" + Editor.GetUIWord(UIWORD.NotAbleToBeAdded), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

						GUI.backgroundColor = prevColor;
					}
					else
					{
						//객체가 없다.
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
						//"[" + targetObjectName + "]\nUnable to be Added"
						GUILayout.Box(Editor.GetUIWord(UIWORD.NotAbleToBeAdded), boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

						GUI.backgroundColor = prevColor;
					}
				}


				//EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(10));
				//EditorGUILayout.EndVertical();
				GUILayout.Space(11);



				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.TimelineLayers));//"Timeline Layers"
				GUILayout.Space(8);


				//현재의 타임라인 레이어 리스트를 만들어야한다.
				List<apAnimTimelineLayer> timelineLayers = animTimeline._layers;
				apAnimTimelineLayer curLayer = null;

				//레이어 정보가 Layout 이벤트와 동일한 경우에만 작동



				if (isGUI_SameLayer)
				{
					for (int i = 0; i < timelineLayers.Count; i++)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();

						curLayer = timelineLayers[i];
						if (animTimelineLayer == curLayer)
						{
							//선택된 레이어다.
							GUI.backgroundColor = new Color(0.9f, 0.7f, 0.7f, 1.0f);
						}
						else
						{
							//선택되지 않은 레이어다.
							GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
						}

						int heightOffset = 18;
						if (i == 0)
						{
							heightOffset = 8;//9
						}

						GUI.Box(new Rect(lastRect.x, lastRect.y + heightOffset, width + 10, 30), "");
						GUI.backgroundColor = prevColor;

						int compWidth = width - (55 + 20 + 5 + 10);

						GUIStyle guiStyle_Label = new GUIStyle(GUI.skin.label);
						guiStyle_Label.alignment = TextAnchor.MiddleLeft;

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
						GUILayout.Space(10);
						EditorGUILayout.LabelField(curLayer.DisplayName, guiStyle_Label, GUILayout.Width(compWidth), GUILayout.Height(20));

						if (animTimelineLayer == curLayer)
						{
							GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
							guiStyle.normal.textColor = Color.white;
							guiStyle.alignment = TextAnchor.UpperCenter;
							GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
							GUILayout.Box(Editor.GetUIWord(UIWORD.Selected), guiStyle, GUILayout.Width(55), GUILayout.Height(20));//"Selected"
							GUI.backgroundColor = prevColor;
						}
						else
						{
							if (GUILayout.Button(Editor.GetUIWord(UIWORD.Select), GUILayout.Width(55), GUILayout.Height(20)))//"Select"
							{
								_isIgnoreAnimTimelineGUI = true;//<깜빡이지 않게..
								SetAnimTimelineLayer(curLayer, true);
							}
						}

						if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey), GUILayout.Width(20), GUILayout.Height(20)))
						{
							//bool isResult = EditorUtility.DisplayDialog("Remove Timeline Layer", "Remove Timeline Layer?", "Remove", "Cancel");

							bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveTimelineLayer_Title),
																	Editor.GetTextFormat(TEXT.RemoveTimelineLayer_Body, curLayer.DisplayName),
																	Editor.GetText(TEXT.Remove),
																	Editor.GetText(TEXT.Cancel)
																	);

							if (isResult)
							{
								isRemoveTimelineLayer = true;
								removeLayer = curLayer;
							}
						}
						EditorGUILayout.EndHorizontal();
						GUILayout.Space(20);
					}
				}


			}


			//----------------------------------
			// 삭제 플래그가 있다.
			if (isRemoveTimelineLayer)
			{
				Editor.Controller.RemoveAnimTimelineLayer(removeLayer);
				SetAnimTimelineLayer(null, true, true);
				SetAnimClipGizmoEvent(true);
			}
			else if (isRemoveTimeline)
			{
				Editor.Controller.RemoveAnimTimeline(animTimeline);
				SetAnimTimeline(null, true);
				SetAnimClipGizmoEvent(true);

			}

		}

		private object _loadKey_ImportAnimClipRetarget = null;
		private void OnImportAnimClipRetarget(bool isSuccess, object loadKey, apRetarget retargetData, apMeshGroup targetMeshGroup, apAnimClip targetAnimClip, bool isMerge)
		{
			if(!isSuccess 
				|| loadKey != _loadKey_ImportAnimClipRetarget 
				|| retargetData == null 
				|| targetMeshGroup == null
				|| targetAnimClip == null
				|| AnimClip != targetAnimClip
				|| AnimClip == null)
			{
				_loadKey_ImportAnimClipRetarget = null;
				return;
			}

			_loadKey_ImportAnimClipRetarget = null;

			if(AnimClip._targetMeshGroup != targetMeshGroup)
			{
				return;
			}

			//로드를 합시다.
			if(retargetData.IsAnimFileLoaded)
			{
				Editor.Controller.ImportAnimClip(retargetData, targetMeshGroup, targetAnimClip, isMerge);
			}
		}

		/// <summary>
		/// 단축키 [A]로 Anim의 Editing 상태를 토글한다.
		/// </summary>
		/// <param name="paramObject"></param>
		public void OnHotKey_AnimEditingToggle(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				return;
			}

			SetAnimEditingToggle();
		}

		//단축키 [S]로 Anim의 SelectionLock을 토글한다.
		public void OnHotKey_AnimSelectionLockToggle(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null || _exAnimEditingMode == EX_EDIT.None)
			{
				return;
			}
			_isAnimLock = !_isAnimLock;
		}


		/// <summary>
		/// 단축키 [D]로 Anim의 LayerLock을 토글한다.
		/// </summary>
		/// <param name="paramObject"></param>
		public void OnHotKey_AnimLayerLockToggle(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null || _exAnimEditingMode == EX_EDIT.None)
			{
				return;
			}

			SetAnimEditingLayerLockToggle();//Mod Layer Lock을 토글
		}


		private void OnHotKey_AnimAddKeyframe(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null)
			{
				return;
			}

			//Debug.LogError("TODO : Set Key");
			if (AnimTimelineLayer != null)
			{
				apAnimKeyframe addedKeyframe = Editor.Controller.AddAnimKeyframe(AnimClip.CurFrame, AnimTimelineLayer, true);
				if (addedKeyframe != null)
				{
					//프레임을 이동하자
					_animClip.SetFrame_Editor(addedKeyframe._frameIndex);
					SetAnimKeyframe(addedKeyframe, true, apGizmos.SELECT_TYPE.New);
				}
			}
		}

		private void OnHotKey_AnimMoveFrame(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.Animation 
				|| _animClip == null 
				)
			{
				Debug.LogError("애니메이션 단축키 처리 실패");
				return;
			}

			if(paramObject is int)
			{
				int iParam = (int)paramObject;

				switch (iParam)
				{
					case 0:
						//Play/Pause Toggle
						{
							if (AnimClip.IsPlaying_Editor)
							{
								// 플레이 -> 일시 정지
								AnimClip.Pause_Editor();
							}
							else
							{
								//마지막 프레임이라면 첫 프레임으로 이동하여 재생한다.
								if (AnimClip.CurFrame == AnimClip.EndFrame)
								{
									AnimClip.SetFrame_Editor(AnimClip.StartFrame);
								}
								// 일시 정지 -> 플레이
								AnimClip.Play_Editor();
							}

							//Play 전환 여부에 따라서도 WorkKeyframe을 전환한다.
							AutoSelectAnimWorkKeyframe();
							Editor.SetRepaint();
							Editor.Gizmos.SetUpdate();
						}
						break;

					case 1:
						//Move [Prev Frame]
						{
							int prevFrame = AnimClip.CurFrame - 1;
							if (prevFrame < AnimClip.StartFrame)
							{
								if (AnimClip.IsLoop)
								{
									prevFrame = AnimClip.EndFrame;
								}
							}
							AnimClip.SetFrame_Editor(prevFrame);
							AutoSelectAnimWorkKeyframe();
						}
						break;

					case 2:
						//Move [Next Frame]
						{
							int nextFrame = AnimClip.CurFrame + 1;
							if (nextFrame > AnimClip.EndFrame)
							{
								if (AnimClip.IsLoop)
								{
									nextFrame = AnimClip.StartFrame;
								}
							}
							AnimClip.SetFrame_Editor(nextFrame);
							AutoSelectAnimWorkKeyframe();
						}
						break;

					case 3:
						//Move [First Frame]
						{
							AnimClip.SetFrame_Editor(AnimClip.StartFrame);
							AutoSelectAnimWorkKeyframe();
						}
						break;

					case 4:
						//Move [Last Frame]
						{
							AnimClip.SetFrame_Editor(AnimClip.EndFrame);
							AutoSelectAnimWorkKeyframe();
						}
						break;

					default:
						Debug.LogError("애니메이션 단축키 처리 실패 - 알 수 없는 코드");
						break;
				}
			}
			else
			{
				Debug.LogError("애니메이션 단축키 처리 실패 - 알 수 없는 파라미터");
			}
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// 객체 통계를 다시 계산할 필요가 있을때 호출한다.
		/// </summary>
		public void SetStatisticsRefresh()
		{
			_isStatisticsNeedToRecalculate = true;
		}
		public void CalculateStatistics()
		{
			if(!_isStatisticsNeedToRecalculate)
			{
				//재계산이 필요없으면 생략
				return;
			}
			_isStatisticsNeedToRecalculate = false;

			_isStatisticsAvailable = false;
			_statistics_NumMesh = 0;
			_statistics_NumVert = 0;
			_statistics_NumEdge = 0;
			_statistics_NumTri = 0;
			_statistics_NumClippedMesh = 0;
			_statistics_NumClippedVert = 0;

			_statistics_NumTimelineLayer = -1;
			_statistics_NumKeyframes = -1;

			if(Editor._portrait == null)
			{	
				return;
			}
			
			//apMesh mesh = null;
			//apTransform_Mesh meshTransform = null;
			switch (_selectionType)
			{
				case SELECTION_TYPE.Overall:
					{
						if (_rootUnit == null || _rootUnit._childMeshGroup == null)
						{
							return;
						}

						CalculateStatisticsMeshGroup(_rootUnit._childMeshGroup);

						if (_curRootUnitAnimClip != null)
						{
							_statistics_NumTimelineLayer = 0;
							_statistics_NumKeyframes = 0;

							apAnimTimeline timeline = null;
							for (int i = 0; i < _curRootUnitAnimClip._timelines.Count; i++)
							{
								timeline = _curRootUnitAnimClip._timelines[i];

								_statistics_NumTimelineLayer += timeline._layers.Count;
								for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
								{
									_statistics_NumKeyframes += timeline._layers[iLayer]._keyframes.Count;
								}
							}
						}
					}
					break;

				case SELECTION_TYPE.Mesh:
					{
						if (_mesh == null)
						{
							return;
						}

						_statistics_NumMesh = -1;//<<어차피 1개인데 이건 출력 생략
						_statistics_NumClippedVert = -1;
						_statistics_NumVert = _mesh._vertexData.Count;
						_statistics_NumEdge = _mesh._edges.Count;
						_statistics_NumTri = (_mesh._indexBuffer.Count / 3);
					}
					
					break;

				case SELECTION_TYPE.MeshGroup:
					{
						if (_meshGroup == null)
						{
							return;
						}

						CalculateStatisticsMeshGroup(_meshGroup);
					}
					break;

				case SELECTION_TYPE.Animation:
					{
						if(_animClip == null)
						{
							return;
						}

						if(_animClip._targetMeshGroup == null)
						{
							return;
						}

						CalculateStatisticsMeshGroup(_animClip._targetMeshGroup);

						_statistics_NumTimelineLayer = 0;
						_statistics_NumKeyframes = 0;

						apAnimTimeline timeline = null;
						for (int i = 0; i < _animClip._timelines.Count; i++)
						{
							timeline = _animClip._timelines[i];

							_statistics_NumTimelineLayer += timeline._layers.Count;
							for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
							{
								_statistics_NumKeyframes += timeline._layers[iLayer]._keyframes.Count;
							}
						}
						
					}
					break;

				default:
					return;
			}

			if(_statistics_NumClippedMesh == 0)
			{
				_statistics_NumClippedMesh = -1;
				_statistics_NumClippedVert = -1;
			}

			_isStatisticsAvailable = true;
		}

		private void CalculateStatisticsMeshGroup(apMeshGroup targetMeshGroup)
		{
			if (targetMeshGroup == null)
			{
				return;
			}

			apMesh mesh = null;
			apTransform_Mesh meshTransform = null;

			for (int i = 0; i < targetMeshGroup._childMeshTransforms.Count; i++)
			{
				meshTransform = targetMeshGroup._childMeshTransforms[i];
				if (meshTransform == null)
				{
					continue;
				}

				mesh = meshTransform._mesh;
				if (mesh == null)
				{
					continue;
				}
				_statistics_NumMesh += 1;
				_statistics_NumVert += mesh._vertexData.Count;
				_statistics_NumEdge += mesh._edges.Count;
				_statistics_NumTri += (mesh._indexBuffer.Count / 3);

				//클리핑이 되는 경우 Vert를 따로 계산해준다.
				//Parent도 같이 포함한다. (렌더링은 같이 되므로)
				if (meshTransform._isClipping_Child)
				{
					_statistics_NumClippedMesh +=1;
					_statistics_NumClippedVert += mesh._vertexData.Count;

					if(meshTransform._clipParentMeshTransform != null &&
						meshTransform._clipParentMeshTransform._mesh != null)
					{
						_statistics_NumClippedVert += meshTransform._clipParentMeshTransform._mesh._vertexData.Count;
					}
				}
			}

			//Child MeshGroupTransform이 있으면 재귀적으로 호출하자
			for (int i = 0; i < targetMeshGroup._childMeshGroupTransforms.Count; i++)
			{
				CalculateStatisticsMeshGroup(targetMeshGroup._childMeshGroupTransforms[i]._meshGroup);
			}
		}

		//_isStatisticsAvailable = false;
		//	_statistics_NumMesh = 0;
		//	_statistics_NumVert = 0;
		//	_statistics_NumEdge = 0;
		//	_statistics_NumTri = 0;
		//	_statistics_NumClippedVert = 0;

		//	_statistics_NumTimelineLayer = -1;
		//	_statistics_NumKeyframes = -1;

		public bool IsStatisticsCalculated		{  get { return _isStatisticsAvailable; } }
		public int Statistics_NumMesh			{  get { return _statistics_NumMesh; } }
		public int Statistics_NumVertex			{  get { return _statistics_NumVert; } }
		public int Statistics_NumEdge			{  get { return _statistics_NumEdge; } }
		public int Statistics_NumTri			{  get { return _statistics_NumTri; } }
		public int Statistics_NumClippedMesh	{  get { return _statistics_NumClippedMesh; } }
		public int Statistics_NumClippedVertex	{  get { return _statistics_NumClippedVert; } }
		public int Statistics_NumTimelineLayer	{  get { return _statistics_NumTimelineLayer; } }
		public int Statistics_NumKeyframe		{  get { return _statistics_NumKeyframes; } }


		public bool IsSelectionLockGUI
		{
			get
			{
				if(_selectionType == SELECTION_TYPE.Animation)
				{
					return IsAnimSelectionLock;
				}
				else if(_selectionType == SELECTION_TYPE.MeshGroup)
				{
					return IsLockExEditKey;
				}
				return false;
			}
		}


		//--------------------------------------------------------------------------------------------
		public class RestoredResult
		{
			public bool _isAnyRestored = false;
			public bool _isRestoreToAdded = false;//삭제된 것이 복원되었다.
			public bool _isRestoreToRemoved = false;//추가되었던 것이 다시 없어졌다.

			public apTextureData _restoredTextureData = null;
			public apMesh _restoredMesh = null;
			public apMeshGroup _restoredMeshGroup = null;
			public apAnimClip _restoredAnimClip = null;
			public apControlParam _restoredControlParam = null;
			public apModifierBase _restoredModifier = null;

			public SELECTION_TYPE _changedType = SELECTION_TYPE.None;

			private static RestoredResult _instance = null;
			public static RestoredResult I { get { if(_instance == null) { _instance = new RestoredResult(); } return _instance; } }

			private RestoredResult()
			{

			}

			public void Init()
			{
				_isAnyRestored = false;
				_isRestoreToAdded = false;//삭제된 것이 복원되었다.
				_isRestoreToRemoved = false;//추가되었던 것이 다시 없어졌다.

				_restoredTextureData = null;
				_restoredMesh = null;
				_restoredMeshGroup = null;
				_restoredAnimClip = null;
				_restoredControlParam = null;
				_restoredModifier = null;

				_changedType = SELECTION_TYPE.None;
			}
		}

		

		/// <summary>
		/// Editor에서 Undo가 수행될 때, Undo 직전의 상태를 확인하여 자동으로 페이지를 전환한다.
		/// RestoredResult를 리턴한다.
		/// </summary>
		/// <param name="portrait"></param>
		/// <param name="recordList_TextureData"></param>
		/// <param name="recordList_Mesh"></param>
		/// <param name="recordList_MeshGroup"></param>
		/// <param name="recordList_AnimClip"></param>
		/// <param name="recordList_ControlParam"></param>
		/// <returns></returns>
		public RestoredResult SetAutoSelectWhenUndoPerformed(		apPortrait portrait,
																	List<int> recordList_TextureData,
																	List<int> recordList_Mesh,
																	List<int> recordList_MeshGroup,
																	List<int> recordList_AnimClip,
																	List<int> recordList_ControlParam,
																	List<int> recordList_Modifier,
																	List<int> recordList_AnimTimeline,
																	List<int> recordList_AnimTimelineLayer,
																	List<int> recordList_Transform,
																	List<int> recordList_Bone)
		{
			//추가. 만약 개수가 변경된 경우, 그것이 삭제 되거나 추가된 경우이다.
			//Prev  <-- ( Undo ) --- Next
			// 있음        <-        없음 : 삭제된 것이 복원 되었다. 해당 메뉴를 찾아서 이동해야한다.
			// 없음        <-        있음 : 새로 추가되었다. 

			RestoredResult.I.Init();

			if(portrait == null)
			{
				return RestoredResult.I;
			}

			
			//개수로 체크하면 빠르다.

			//1. 텍스쳐
			if (portrait._textureData != null && portrait._textureData.Count != recordList_TextureData.Count)
			{
				//텍스쳐 리스트와 개수가 다른 경우
				if (portrait._textureData.Count > recordList_TextureData.Count)
				{
					//Restored > Record : 삭제된 것이 다시 복원되어 추가 되었다.
					RestoredResult.I._isRestoreToAdded = true;
					RestoredResult.I._changedType = SELECTION_TYPE.ImageRes;

					
					//복원된 것을 찾자
					for (int i = 0; i < portrait._textureData.Count; i++)
					{
						int uniqueID = portrait._textureData[i]._uniqueID;
						if(!recordList_TextureData.Contains(uniqueID))
						{
							//Undo 전에 없었던 ID이다. 새로 추가되었다.
							RestoredResult.I._restoredTextureData = portrait._textureData[i];
							break;
						}
					}
				}
				else
				{
					//Restored < Record : 추가되었던 것이 삭제되었다.
					RestoredResult.I._isRestoreToRemoved = true;
					RestoredResult.I._changedType = SELECTION_TYPE.ImageRes;
				}
			}

			//2. 메시
			if (portrait._meshes != null)
			{
				//실제 Monobehaviour를 체크
				if(portrait._subObjectGroup_Mesh != null)
				{
					//Unity에서 제공하는 childMeshes를 기준으로 동기화를 해야한다.
					apMesh[] childMeshes = portrait._subObjectGroup_Mesh.GetComponentsInChildren<apMesh>();

					int nMeshesInList = 0;
					int nMeshesInGameObj = 0;
					for (int i = 0; i < portrait._meshes.Count; i++)
					{
						if (portrait._meshes[i] != null)
						{
							nMeshesInList++;
						}
					}
					if(portrait._meshes.Count != nMeshesInList)
					{
						//실제 데이터와 다르다. (Null이 포함되어 있다.)
						portrait._meshes.RemoveAll(delegate(apMesh a)
						{
							return a == null;
						});
					}

					if(childMeshes == null)
					{
						//Debug.LogError("Child Mesh가 없다.");
					}
					else
					{
						//Debug.LogError("Child Mesh의 개수 [" + childMeshes.Length + "] / 리스트 데이터 상의 개수 [" + portrait._meshes.Count + "]");
						nMeshesInGameObj = childMeshes.Length;
					}

					if(nMeshesInList != nMeshesInGameObj)
					{
						if(nMeshesInGameObj > 0)
						{
							for (int i = 0; i < childMeshes.Length; i++)
							{
								apMesh childMesh = childMeshes[i];
								if(!portrait._meshes.Contains(childMesh))
								{
									portrait._meshes.Add(childMesh);
								}
							}
						}
					}
				}

				if (portrait._meshes.Count != recordList_Mesh.Count)
				{
					//Mesh 리스트와 개수가 다른 경우
					if (portrait._meshes.Count > recordList_Mesh.Count)
					{
						//Restored > Record : 삭제된 것이 다시 복원되어 추가 되었다.
						RestoredResult.I._isRestoreToAdded = true;
						RestoredResult.I._changedType = SELECTION_TYPE.Mesh;

						//복원된 것을 찾자
						for (int i = 0; i < portrait._meshes.Count; i++)
						{
							int uniqueID = portrait._meshes[i]._uniqueID;
							if (!recordList_Mesh.Contains(uniqueID))
							{
								//Undo 전에 없었던 ID이다. 새로 추가되었다.
								RestoredResult.I._restoredMesh = portrait._meshes[i];
								break;
							}
						}
					}
					else
					{
						//Restored < Record : 추가되었던 것이 삭제되었다.
						RestoredResult.I._isRestoreToRemoved = true;
						RestoredResult.I._changedType = SELECTION_TYPE.Mesh;
					}
				}
			}


			//3. 메시 그룹
			if (portrait._meshGroups != null)
			{
				if (portrait._subObjectGroup_MeshGroup != null)
				{
					//Unity에서 제공하는 childMeshGroups를 기준으로 동기화를 해야한다.
					apMeshGroup[] childMeshGroups = portrait._subObjectGroup_MeshGroup.GetComponentsInChildren<apMeshGroup>();

					int nMeshGroupsInList = 0;
					int nMeshGroupsInGameObj = 0;
					for (int i = 0; i < portrait._meshGroups.Count; i++)
					{
						if(portrait._meshGroups[i] != null)
						{
							nMeshGroupsInList++;
						}
					}
					if(portrait._meshGroups.Count != nMeshGroupsInList)
					{
						//실제 데이터와 다르다. (Null이 포함되어 있다.)
						portrait._meshGroups.RemoveAll(delegate(apMeshGroup a)
						{
							return a == null;
						});
					}

					if(childMeshGroups != null)
					{
						nMeshGroupsInGameObj = childMeshGroups.Length;
					}

					if(nMeshGroupsInList != nMeshGroupsInGameObj)
					{
						if(nMeshGroupsInGameObj > 0)
						{
							for (int i = 0; i < childMeshGroups.Length; i++)
							{
								apMeshGroup childMeshGroup = childMeshGroups[i];
								if(!portrait._meshGroups.Contains(childMeshGroup))
								{
									portrait._meshGroups.Add(childMeshGroup);
								}
							}
						}
					}

				}

				if (portrait._meshGroups.Count != recordList_MeshGroup.Count)
				{
					//메시 그룹 리스트와 다른 경우
					if (portrait._meshGroups.Count > recordList_MeshGroup.Count)
					{
						//Restored > Record : 삭제된 것이 다시 복원되어 추가 되었다.
						RestoredResult.I._isRestoreToAdded = true;
						RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;

						
						//복원된 것을 찾자
						for (int i = 0; i < portrait._meshGroups.Count; i++)
						{
							int uniqueID = portrait._meshGroups[i]._uniqueID;
							if (!recordList_MeshGroup.Contains(uniqueID))
							{
								//Undo 전에 없었던 ID이다. 새로 추가되었다.
								RestoredResult.I._restoredMeshGroup = portrait._meshGroups[i];
								break;
							}
						}
					}
					else
					{
						//Restored < Record : 추가되었던 것이 삭제되었다.
						RestoredResult.I._isRestoreToRemoved = true;
						RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;
					}
				}
			}


			//4. 애니메이션 클립
			if (portrait._animClips != null)
			{
				if (portrait._animClips.Count != recordList_AnimClip.Count)
				{
					//Anim 리스트와 개수가 다른 경우
					if (portrait._animClips.Count > recordList_AnimClip.Count)
					{
						//Restored > Record : 삭제된 것이 다시 복원되어 추가 되었다.
						RestoredResult.I._isRestoreToAdded = true;
						RestoredResult.I._changedType = SELECTION_TYPE.Animation;

						
						//복원된 것을 찾자
						for (int i = 0; i < portrait._animClips.Count; i++)
						{
							int uniqueID = portrait._animClips[i]._uniqueID;
							if (!recordList_AnimClip.Contains(uniqueID))
							{
								//Undo 전에 없었던 ID이다. 새로 추가되었다.
								RestoredResult.I._restoredAnimClip = portrait._animClips[i];
								break;
							}
						}
					}
					else
					{
						//Restored < Record : 추가되었던 것이 삭제되었다.
						RestoredResult.I._isRestoreToRemoved = true;
						RestoredResult.I._changedType = SELECTION_TYPE.Animation;
					}
				}
				else
				{
					//만약, AnimClip 개수는 변동이 없는데, 타임라인 개수에 변동이 있다면
					//최소한 Refresh는 해야한다.
					int nTimeline = 0;
					int nTimelineLayer = 0;
					apAnimClip animClip = null;
					for (int iAnimClip = 0; iAnimClip < portrait._animClips.Count; iAnimClip++)
					{
						animClip = portrait._animClips[iAnimClip];
						nTimeline += animClip._timelines.Count;

						for (int iTimeline = 0; iTimeline < animClip._timelines.Count; iTimeline++)
						{
							nTimelineLayer += animClip._timelines[iTimeline]._layers.Count;
						}
					}

					if(nTimeline > recordList_AnimTimeline.Count
						|| nTimelineLayer > recordList_AnimTimelineLayer.Count)
					{
						//타임라인이나 타임라인 레이어가 증가했다.
						RestoredResult.I._isRestoreToAdded = true;
						RestoredResult.I._changedType = SELECTION_TYPE.Animation;
					}
					else if(nTimeline < recordList_AnimTimeline.Count
							|| nTimelineLayer < recordList_AnimTimelineLayer.Count)
					{
						//Restored < Record : 추가되었던 것이 삭제되었다.
						RestoredResult.I._isRestoreToRemoved = true;
						RestoredResult.I._changedType = SELECTION_TYPE.Animation;
					}
				}
				
			}


			//5. 컨트롤 파라미터
			if (portrait._controller._controlParams != null 
				&& portrait._controller._controlParams.Count != recordList_ControlParam.Count)
			{
				//Param 리스트와 개수가 다른 경우
				if (portrait._controller._controlParams.Count > recordList_ControlParam.Count)
				{
					//Restored > Record : 삭제된 것이 다시 복원되어 추가 되었다.
					RestoredResult.I._isRestoreToAdded = true;
					RestoredResult.I._changedType = SELECTION_TYPE.Param;

					//복원된 것을 찾자
					for (int i = 0; i < portrait._controller._controlParams.Count; i++)
					{
						int uniqueID = portrait._controller._controlParams[i]._uniqueID;
						if(!recordList_ControlParam.Contains(uniqueID))
						{
							//Undo 전에 없었던 ID이다. 새로 추가되었다.
							RestoredResult.I._restoredControlParam = portrait._controller._controlParams[i];
							break;
						}
					}
				}
				else
				{
					//Restored < Record : 추가되었던 것이 삭제되었다.
					RestoredResult.I._isRestoreToRemoved = true;
					RestoredResult.I._changedType = SELECTION_TYPE.Param;
				}
			}

			//6. 모디파이어 > TODO
			if(RestoredResult.I._changedType != SELECTION_TYPE.MeshGroup)
			{
				//MeshGroup에서 복원 기록이 없는 경우에 한해서 Modifier의 추가가 있었는지 확인한다.
				//MeshGroup의 복원 기록이 있다면 Modifier는 자동으로 포함되기 때문
				//모든 모디파이어를 모아야 한다.
				List<apModifierBase> allModifiers = new List<apModifierBase>();

				apMeshGroup meshGroup = null;
				apModifierBase modifier = null;

				for (int iMG = 0; iMG < portrait._meshGroups.Count; iMG++)
				{
					meshGroup = portrait._meshGroups[iMG];
					if(meshGroup == null)
					{
						continue;
					}

					for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
					{
						modifier = meshGroup._modifierStack._modifiers[iMod];
						if(modifier == null)
						{
							continue;
						}
						allModifiers.Add(modifier);
					}
				}

				//이제 실제 포함된 Modifier를 비교해야한다.
				//이건 데이터 누락이 있을 수 있다.
				if(portrait._subObjectGroup_Modifier != null)
				{
					//Unity에서 제공하는 childModifer기준으로 동기화를 해야한다.
					apModifierBase[] childModifiers = portrait._subObjectGroup_Modifier.GetComponentsInChildren<apModifierBase>();

					int nModInList = allModifiers.Count;
					int nModInGameObj = 0;
					
					if(childModifiers != null)
					{
						nModInGameObj = childModifiers.Length;
					}

					if(nModInList != nModInGameObj)
					{
						if(nModInGameObj > 0)
						{
							for (int i = 0; i < childModifiers.Length; i++)
							{
								apModifierBase childModifier = childModifiers[i];
								//이제 어느 MeshGroup의 Modifier인지 찾아야 한다 ㅜㅜ

								if(childModifier._meshGroup == null)
								{
									//연결이 안되었다면 찾자
									int meshGroupUniqueID = childModifier._meshGroupUniqueID;
									childModifier._meshGroup = portrait.GetMeshGroup(meshGroupUniqueID);
								}

								if(childModifier._meshGroup != null)
								{
									if(!childModifier._meshGroup._modifierStack._modifiers.Contains(childModifier))
									{
										childModifier._meshGroup._modifierStack._modifiers.Add(childModifier);
									}

									//체크용 allModifiers 리스트에도 넣자
									if (!allModifiers.Contains(childModifier))
									{
										allModifiers.Add(childModifier);
									}

								}

							}
						}
					}
				}

				if(allModifiers.Count != recordList_Modifier.Count)
				{
					//모디파이어 리스트와 다른 경우 => 뭔가 복원 되었거나 삭제된 것이다.
					
					if(allModifiers.Count > recordList_Modifier.Count)
					{
						//Restored > Record : 삭제된 것이 다시 복원되어 추가 되었다.
						RestoredResult.I._isRestoreToAdded = true;
						RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;

						//복원된 것을 찾자
						for (int i = 0; i < allModifiers.Count; i++)
						{
							int uniqueID = allModifiers[i]._uniqueID;
							if(!recordList_Modifier.Contains(uniqueID))
							{
								RestoredResult.I._restoredModifier = allModifiers[i];
								break;
							}
						}
					}
					else
					{
						//Restored < Record : 추가되었던 것이 삭제되었다.
						RestoredResult.I._isRestoreToRemoved = true;
						RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;
					}
				}
			}

			//7. RootUnit -> MeshGroup의 변동 사항이 없다면 RootUnit을 체크해볼 필요가 있다.
			if(RestoredResult.I._changedType != SELECTION_TYPE.MeshGroup)
			{
				//RootUnit의 ID와 실제 RootUnit이 같은지 확인한다.
				int nRootUnit = portrait._rootUnits.Count;
				int nMainMeshGroup = portrait._mainMeshGroupList.Count;
				int nMainMeshGroupID = portrait._mainMeshGroupIDList.Count;

				if (nRootUnit != nMainMeshGroup ||
					nMainMeshGroup != nMainMeshGroupID ||
					nRootUnit != nMainMeshGroupID)
				{
					//3개의 값이 다르다.
					//ID를 기준으로 하자
					if(nRootUnit < nMainMeshGroupID ||
						nMainMeshGroup < nMainMeshGroupID ||
						nRootUnit < nMainMeshGroup)
					{
						//ID가 더 많다. -> 복원할게 있다.
						RestoredResult.I._changedType = SELECTION_TYPE.Overall;
						RestoredResult.I._isRestoreToAdded = true;
					}
					else
					{
						//ID가 더 적다. -> 삭제할게 있다.
						RestoredResult.I._changedType = SELECTION_TYPE.Overall;
						RestoredResult.I._isRestoreToRemoved = true;
					}
				}
				else
				{
					//개수는 같은데, 데이터가 빈게 있나.. 아니면 다를수도
					apRootUnit rootUnit = null;
					apMeshGroup mainMeshGroup = null;
					int mainMeshGroupID = -1;
					for (int i = 0; i < nRootUnit; i++)
					{
						rootUnit = portrait._rootUnits[i];
						mainMeshGroup = portrait._mainMeshGroupList[i];
						mainMeshGroupID = portrait._mainMeshGroupIDList[i];

						if(rootUnit == null || mainMeshGroup == null)
						{
							//데이터가 없다.
							if(mainMeshGroupID >= 0)
							{
								//유효한 ID가 있다. -> 복원할게 있다.
								RestoredResult.I._changedType = SELECTION_TYPE.Overall;
								RestoredResult.I._isRestoreToAdded = true;
							}
							else
							{
								//유효하지 않는 ID와 데이터가 있다. -> 삭제할 게 있다.
								RestoredResult.I._changedType = SELECTION_TYPE.Overall;
								RestoredResult.I._isRestoreToRemoved = true;
							}
						}
						else if(rootUnit._childMeshGroup == null 
							|| rootUnit._childMeshGroup != mainMeshGroup
							|| rootUnit._childMeshGroup._uniqueID != mainMeshGroupID)
						{
							//데이터가 맞지 않다.
							//삭제인지 추가인지 모르지만 일단 갱신 필요
							RestoredResult.I._changedType = SELECTION_TYPE.Overall;
							RestoredResult.I._isRestoreToRemoved = true;
						}
					}
				}
			}

			if (!RestoredResult.I._isRestoreToAdded && !RestoredResult.I._isRestoreToRemoved)
			{
				//MeshGroup의 변동이 없을 때
				//-> 1. Transform에 변동이 있는가
				//-> 2. Bone에 변동이 있는가
				//만약, MeshGroup은 그대로지만, Trasnform이 다른 경우 -> 갱신 필요
				List<int> allTransforms = new List<int>();
				List<int> allBones = new List<int>();
				for (int iMSG = 0; iMSG < portrait._meshGroups.Count; iMSG++)
				{
					apMeshGroup meshGroup = portrait._meshGroups[iMSG];
					for (int iMeshTF = 0; iMeshTF < meshGroup._childMeshTransforms.Count; iMeshTF++)
					{
						allTransforms.Add(meshGroup._childMeshTransforms[iMeshTF]._transformUniqueID);
					}
					for (int iMeshGroupTF = 0; iMeshGroupTF < meshGroup._childMeshGroupTransforms.Count; iMeshGroupTF++)
					{
						allTransforms.Add(meshGroup._childMeshGroupTransforms[iMeshGroupTF]._transformUniqueID);
					}
					for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
					{
						allBones.Add(meshGroup._boneList_All[iBone]._uniqueID);
					}
				}

				//1. Transform 체크
				if(allTransforms.Count != recordList_Transform.Count)
				{
					//Transform 개수가 Undo를 전후로 바뀌었다.
					if(allTransforms.Count > recordList_Transform.Count)
					{
						//삭제 -> 복원
						RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;
						RestoredResult.I._isRestoreToAdded = true;
					}
					else
					{
						//추가 -> 삭제
						RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;
						RestoredResult.I._isRestoreToRemoved = true;
					}
				}

				//2. Bone 체크
				if(allBones.Count != recordList_Bone.Count)
				{
					//Bone 개수가 Undo를 전후로 바뀌었다.
					if(allBones.Count > recordList_Bone.Count)
					{
						//삭제 -> 복원
						RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;
						RestoredResult.I._isRestoreToAdded = true;
					}
					else
					{
						//추가 -> 삭제
						RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;
						RestoredResult.I._isRestoreToRemoved = true;
					}
				}

			}


			if (!RestoredResult.I._isRestoreToAdded && !RestoredResult.I._isRestoreToRemoved)
			{
				RestoredResult.I._isAnyRestored = false;
			}
			else
			{
				RestoredResult.I._isAnyRestored = true;
			}

			return RestoredResult.I;
			
		}

		public void SetAutoSelectOrUnselectFromRestore(RestoredResult restoreResult, apPortrait portrait)
		{
			if (!restoreResult._isRestoreToAdded && !restoreResult._isRestoreToRemoved)
			{
				//아무것도 바뀐게 없다면
				return;
			}

			if (restoreResult._isRestoreToAdded)
			{
				// 삭제 -> 복원해서 새로운게 생겼을 경우 : 그걸 선택해야한다.
				switch (restoreResult._changedType)
				{
					case SELECTION_TYPE.ImageRes:
						if (restoreResult._restoredTextureData != null)
						{
							SetImage(restoreResult._restoredTextureData);
						}
						break;

					case SELECTION_TYPE.Mesh:
						if (restoreResult._restoredMesh != null)
						{
							SetMesh(restoreResult._restoredMesh);
						}
						break;

					case SELECTION_TYPE.MeshGroup:
						if (restoreResult._restoredMeshGroup != null)
						{
							SetMeshGroup(restoreResult._restoredMeshGroup);
						}
						else if(restoreResult._restoredModifier != null)
						{
							if(restoreResult._restoredModifier._meshGroup != null)
							{
								SetMeshGroup(restoreResult._restoredModifier._meshGroup);
							}
						}
						break;

					case SELECTION_TYPE.Animation:
						if (restoreResult._restoredAnimClip != null)
						{
							SetAnimClip(restoreResult._restoredAnimClip);
						}
						break;

					case SELECTION_TYPE.Param:
						if (restoreResult._restoredControlParam != null)
						{
							SetParam(restoreResult._restoredControlParam);
						}
						break;

					case SELECTION_TYPE.Overall:
						//RootUnit은 새로 복원되어도 별도의 행동을 취하지 않는다.
						break;

					default:
						//뭐징..
						restoreResult.Init();
						return;
				}
			}

			if (restoreResult._isRestoreToRemoved)
			{
				// 추가 -> 취소해서 삭제되었을 경우 : 타입을 보고 해당 페이지의 것이 이미 사라진 것인지 확인
				//페이지를 나와야 한다.
				bool isRemovedPage = false;
				if (SelectionType == restoreResult._changedType)
				{
					switch (restoreResult._changedType)
					{
						case SELECTION_TYPE.ImageRes:
							if (_image != null)
							{
								if (!portrait._textureData.Contains(_image))
								{
									//삭제되어 없는 이미지를 가리키고 있다.
									isRemovedPage = true;
								}
							}
							break;

						case SELECTION_TYPE.Mesh:
							if (_mesh != null)
							{
								if (!portrait._meshes.Contains(_mesh))
								{
									//삭제되어 없는 메시를 가리키고 있다.
									isRemovedPage = true;
								}
							}
							break;

						case SELECTION_TYPE.MeshGroup:
							if (_meshGroup != null)
							{
								if (!portrait._meshGroups.Contains(_meshGroup))
								{
									//삭제되어 없는 메시 그룹을 가리키고 있다.
									isRemovedPage = true;
								}
							}
							break;

						case SELECTION_TYPE.Animation:
							if (_animClip != null)
							{
								if (!portrait._animClips.Contains(_animClip))
								{
									//삭제되어 없는 AnimClip을 가리키고 있다.
									isRemovedPage = true;
								}
							}
							break;

						case SELECTION_TYPE.Param:
							if (_param != null)
							{
								if (!portrait._controller._controlParams.Contains(_param))
								{
									//삭제되어 없는 Param을 가리키고 있다.
									isRemovedPage = true;
								}
							}
							break;

						case SELECTION_TYPE.Overall:
							{
								if(_rootUnit != null)
								{
									if(!portrait._rootUnits.Contains(_rootUnit))
									{
										isRemovedPage = true;
									}
								}
							}
							break;
						
						default:
							//뭐징..
							restoreResult.Init();
							return;
					}
				}

				if (isRemovedPage)
				{
					SetNone();
				}
			}

			restoreResult.Init();
		}
	}
}