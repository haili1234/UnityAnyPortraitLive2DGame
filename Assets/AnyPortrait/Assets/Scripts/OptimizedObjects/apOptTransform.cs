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

	//에디터의 apRenderUnit + [Transform_Mesh/Transform_MeshGroup]이 합쳐진 실행객체
	//Transform (Mesh/MG) 데이터와 RenderUnit의 Update 기능들이 여기에 모두 포함된다.
	/// <summary>
	/// This is a class that belongs hierarchically under "apOptRootUnit".
	/// This is the target of the modifier, has bones or contains meshes.
	/// (Although it can be controlled by a script, it takes precedence to process it from Modifier.)
	/// </summary>
	public class apOptTransform : MonoBehaviour
	{
		// Members
		//------------------------------------------------
		/// <summary>[Please do not use it] Parent Portrait</summary>
		public apPortrait _portrait = null;

		/// <summary>[Please do not use it] Parent Root Unit</summary>
		public apOptRootUnit _rootUnit = null;

		/// <summary>[Please do not use it] Unique ID</summary>
		public int _transformID = -1;

		/// <summary>[Please do not use it] Transform Name</summary>
		public string _name = "";


		/// <summary>[Please do not use it] Transform</summary>
		[HideInInspector]
		public Transform _transform = null;

		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public apMatrix _defaultMatrix;


		//RenderUnit 데이터
		
		public enum UNIT_TYPE { Group = 0, Mesh = 1 }

		/// <summary>[Please do not use it] Unit Type (MeshGroup / Mesh)</summary>
		public UNIT_TYPE _unitType = UNIT_TYPE.Group;

		/// <summary>[Please do not use it] UniqueID of MeshGroup</summary>
		public int _meshGroupUniqueID = -1;//Group 타입이면 meshGroupUniqueID를 넣어주자.

		/// <summary>[Please do not use it] Hierarchy Level</summary>
		public int _level = -1;

		/// <summary>[Please do not use it] Z Depth Value</summary>
		public int _depth = -1;

		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public bool _isVisible_Default = true;

		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public Color _meshColor2X_Default = Color.gray;


		/// <summary>Parent Opt-Transform</summary>
		public apOptTransform _parentTransform = null;

		/// <summary>Number of Children Opt-Transform</summary>
		public int _nChildTransforms = 0;

		/// <summary>Chilndren Array</summary>
		public apOptTransform[] _childTransforms = null;

		//Mesh 타입인 경우
		/// <summary>Opt Mesh (if it is MeshType)</summary>
		public apOptMesh _childMesh = null;//실제 Mesh MonoBehaviour
		
		//<참고>
		//원래 apRenderVertex는 renderUnit에 있지만, 여기서는 apOptMesh에 직접 포함되어 있다.



		//Modifier의 값을 전달받는 Stack
		[NonSerialized]
		private apOptCalculatedResultStack _calculatedStack = null;

		public apOptCalculatedResultStack CalculatedStack
		{
			get
			{
				if (_calculatedStack == null)
				{ _calculatedStack = new apOptCalculatedResultStack(this); }
				return _calculatedStack;
			}
		}

		/// <summary>[Please do not use it] It stores Modifiers</summary>
		[SerializeField]
		public apOptModifierSubStack _modifierStack = new apOptModifierSubStack();

		//업데이트 되는 변수
		//[NonSerialized]
		//public apMatrix3x3 _matrix_TF_Cal_ToWorld = apMatrix3x3.identity;

		//private apMatrix _calculateTmpMatrix = new apMatrix();
		//public apMatrix CalculatedTmpMatrix {  get { return _calculateTmpMatrix; } }


		//World Transform을 구하기 위해선
		// World Transform = [Parent World] x [To Parent] x [Modified]

		/// <summary>[Please do not use it] Updated Matrix</summary>
		[NonSerialized]
		public apMatrix _matrix_TF_ParentWorld = new apMatrix();

		/// <summary>[Please do not use it] Updated Matrix</summary>
		[NonSerialized]
		public apMatrix _matrix_TF_ParentWorld_NonModified = new apMatrix();

		/// <summary>[Please do not use it] Updated Matrix</summary>
		//Opt Transform은 기본 좌표에 ToParent가 반영되어 있다.
		[NonSerialized]
		public apMatrix _matrix_TF_ToParent = new apMatrix();

		/// <summary>[Please do not use it] Updated Matrix</summary>
		[NonSerialized]
		public apMatrix _matrix_TF_LocalModified = new apMatrix();

		/// <summary>[Please do not use it] Updated Matrix</summary>
		[NonSerialized]
		public apMatrix _matrix_TFResult_World = new apMatrix();

		/// <summary>[Please do not use it] Updated Matrix</summary>
		[NonSerialized]
		public bool _isCalculateWithoutMod = false;//WithoutMod 계열은 한번만 계산한다.

		/// <summary>[Please do not use it] Updated Matrix</summary>
		[NonSerialized]
		public apMatrix _matrix_TFResult_WorldWithoutMod = new apMatrix();

		/// <summary>[Please do not use it] Updated Matrix</summary>
		[NonSerialized]
		public Color _meshColor2X = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		/// <summary>[Please do not use it] Updated Matrix</summary>
		[NonSerialized]
		public bool _isAnyColorCalculated = false;

		/// <summary>[Please do not use it] Updated Matrix</summary>
		[NonSerialized]
		public bool _isVisible = false;

		private const float VISIBLE_ALPHA = 0.01f;
		/// <summary>[Please do not use it] Updated Matrix</summary>
		//Rigging을 위한 단축 식
		[NonSerialized]
		public apMatrix3x3 _vertLocal2MeshWorldMatrix = new apMatrix3x3();
		/// <summary>[Please do not use it] Updated Matrix</summary>
		[NonSerialized]
		public apMatrix3x3 _vertWorld2MeshLocalMatrix = new apMatrix3x3();

		/// <summary>[Please do not use it] Updated Matrix</summary>
		[NonSerialized]
		public apMatrix3x3 _vertMeshWorldNoModMatrix = new apMatrix3x3();

		/// <summary>[Please do not use it] Updated Matrix</summary>
		[NonSerialized]
		public apMatrix3x3 _vertMeshWorldNoModInverseMatrix = new apMatrix3x3();



		// OptBone을 추가한다.
		//OptBone의 GameObject가 저장되는 Transform (내용은 없다)
		/// <summary>Root Transform containing the Bones</summary>
		public Transform _boneGroup = null;

		/// <summary>Bones</summary>
		public apOptBone[] _boneList_All = null;

		/// <summary>Bones (Root Only)</summary>
		public apOptBone[] _boneList_Root = null;

		/// <summary>[Please do not use it]</summary>
		public bool _isBoneUpdatable = false;


		//Attach시 만들어지는 Socket
		//Socket 옵션은 MeshTransform/MeshGroupTransform에서 미리 세팅해야한다.
		/// <summary>Socket Transform (it is not null if "Socket Option" is enabled)</summary>
		public Transform _socketTransform = null;

		

		//스크립트로 TRS를 직접 제어할 수 있다.
		//단 Update마다 매번 설정해야한다.
		//좌표계는 WorldMatrix를 기준으로 한다.
		//값 자체는 절대값을 기준으로 한다.
		private bool _isExternalUpdate_Position = false;
		private bool _isExternalUpdate_Rotation = false;
		private bool _isExternalUpdate_Scaling = false;
		private float _externalUpdateWeight = 0.0f;
		private Vector2 _exUpdate_Pos = Vector2.zero;
		private float _exUpdate_Angle = 0.0f;
		private Vector2 _exUpdate_Scale = Vector2.zero;

		//처리된 TRS
		private Vector3 _updatedWorldPos = Vector3.zero;
		private float _updatedWorldAngle = 0.0f;
		private Vector3 _updatedWorldScale = Vector3.one;

		private Vector3 _updatedWorldPos_NoRequest = Vector3.zero;
		private float _updatedWorldAngle_NoRequest = 0.0f;
		private Vector3 _updatedWorldScale_NoRequest = Vector3.one;


		// Init
		//------------------------------------------------
		void Awake()
		{

		}

		void Start()
		{
			_isExternalUpdate_Position = false;
			_isExternalUpdate_Rotation = false;
			_isExternalUpdate_Scaling = false;

			_isCalculateWithoutMod = false;

			//업데이트 안합니더
			this.enabled = false;
		}

		// Update
		//------------------------------------------------
		void Update()
		{

		}

		void LateUpdate()
		{

		}




		// Update (외부에서 업데이트를 한다.)
		//------------------------------------------------
		public void UpdateModifier_Pre(float tDelta)
		{
			if (_modifierStack != null)
			{
				_modifierStack.Update_Pre(tDelta);
			}

			//자식 객체도 업데이트를 한다.
			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].UpdateModifier_Pre(tDelta);
				}
			}
		}

		public void UpdateModifier_Post(float tDelta)
		{

			if (_modifierStack != null)
			{
				_modifierStack.Update_Post(tDelta);
			}

			//자식 객체도 업데이트를 한다.
			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].UpdateModifier_Post(tDelta);
				}
			}
		}




		


		public void ReadyToUpdate()
		{
			//1. Child Mesh와 기본 Reday
			if (_childMesh != null)
			{
				_childMesh.ReadyToUpdate();
			}

			//2. Calculate Stack Ready
			if (_calculatedStack != null)
			{
				_calculatedStack.ReadyToCalculate();
			}

			//3. 몇가지 변수 초기화
			_meshColor2X = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			_isAnyColorCalculated = false;

			_isVisible = true;

			//Editor에서는 기본 matrix가 들어가지만, 여기서는 아예 Transform(Mono)에 들어가기 때문에 Identity가 된다.
			//_matrix_TF_Cal_ToWorld = apMatrix3x3.identity;
			//_calculateTmpMatrix.SetIdentity();


			//변경
			//[Parent World x To Parent x Local TF] 조합으로 변경

			if (_matrix_TF_ParentWorld == null)				{ _matrix_TF_ParentWorld = new apMatrix(); }
			if (_matrix_TF_ParentWorld_NonModified == null)	{ _matrix_TF_ParentWorld_NonModified = new apMatrix(); }
			if (_matrix_TF_ToParent == null)				{ _matrix_TF_ToParent = new apMatrix(); }
			if (_matrix_TF_LocalModified == null)			{ _matrix_TF_LocalModified = new apMatrix(); }
			if (_matrix_TFResult_World == null)				{ _matrix_TFResult_World = new apMatrix(); }
			if (_matrix_TFResult_WorldWithoutMod == null)	{ _matrix_TFResult_WorldWithoutMod = new apMatrix(); }


			_matrix_TF_ParentWorld.SetIdentity();
			_matrix_TF_ParentWorld_NonModified.SetIdentity();
			//_matrix_TF_ToParent.SetIdentity();
			_matrix_TF_LocalModified.SetIdentity();

			//Editor에서는 기본 matrix가 들어가지만, 여기서는 아예 Transform(Mono)에 들어가기 때문에 Identity가 된다.
			_matrix_TF_ToParent.SetMatrix(_defaultMatrix);

			_matrix_TFResult_World.SetIdentity();

			if (!_isCalculateWithoutMod)
			{
				_matrix_TFResult_WorldWithoutMod.SetIdentity();
			}

			

			//3. 자식 호출
			//자식 객체도 업데이트를 한다.
			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].ReadyToUpdate();
				}
			}
		}





		/// <summary>
		/// CalculateStack을 업데이트 한다.
		/// Pre-Update이다. Rigging, VertWorld는 제외된다.
		/// </summary>
		public void UpdateCalculate_Pre()
		{

//#if UNITY_EDITOR
//			Profiler.BeginSample("Transform - 1. Stack Calculate");
//#endif

			//1. Calculated Stack 업데이트
			if (_calculatedStack != null)
			{
				_calculatedStack.Calculate_Pre();
			}

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif


//#if UNITY_EDITOR
//			Profiler.BeginSample("Transform - 2. Matrix / Color");
//#endif

			//2. Calculated의 값 적용 + 계층적 Matrix 적용
			if (CalculatedStack.MeshWorldMatrixWrap != null)
			{
				//변경전
				//_calclateTmpMatrix.SRMultiply(_calculatedStack.MeshWorldMatrixWrap, true);
				//_matrix_TF_Cal_ToWorld = _calculateTmpMatrix.MtrxToSpace;

				//변경후
				_matrix_TF_LocalModified.SetMatrix(_calculatedStack.MeshWorldMatrixWrap);

				//if(_calculatedStack.MeshWorldMatrixWrap.Scale2.magnitude < 0.8f)
				//{
				//	Debug.Log(name + " : Low Scale : " + _calculatedStack.MeshWorldMatrixWrap.Scale2);
				//}

			}

			if (CalculatedStack.IsAnyColorCalculated)
			{
				_meshColor2X = CalculatedStack.MeshColor;
				_isVisible = CalculatedStack.IsMeshVisible;
				_isAnyColorCalculated = true;
			}
			else
			{
				_meshColor2X = _meshColor2X_Default;
				_isVisible = _isVisible_Default;
			}
			if (!_isVisible)
			{
				_meshColor2X.a = 0.0f;
				_isAnyColorCalculated = true;
			}


			if (_parentTransform != null)
			{
				//변경 전
				//_calculateTmpMatrix.SRMultiply(_parentTransform.CalculatedTmpMatrix, true);
				//_matrix_TF_Cal_ToWorld = _calculateTmpMatrix.MtrxToSpace;

				//변경 후
				_matrix_TF_ParentWorld.SetMatrix(_parentTransform._matrix_TFResult_World);
				_matrix_TF_ParentWorld_NonModified.SetMatrix(_parentTransform._matrix_TFResult_WorldWithoutMod);

				//색상은 2X 방식의 Add
				_meshColor2X.r = Mathf.Clamp01(((float)(_meshColor2X.r) - 0.5f) + ((float)(_parentTransform._meshColor2X.r) - 0.5f) + 0.5f);
				_meshColor2X.g = Mathf.Clamp01(((float)(_meshColor2X.g) - 0.5f) + ((float)(_parentTransform._meshColor2X.g) - 0.5f) + 0.5f);
				_meshColor2X.b = Mathf.Clamp01(((float)(_meshColor2X.b) - 0.5f) + ((float)(_parentTransform._meshColor2X.b) - 0.5f) + 0.5f);
				_meshColor2X.a *= _parentTransform._meshColor2X.a;

				if(_parentTransform._isAnyColorCalculated)
				{
					_isAnyColorCalculated = true;
				}
			}

			if (_meshColor2X.a < VISIBLE_ALPHA
				//|| !CalculatedStack.IsMeshVisible
				)
			{
				_isVisible = false;
				_meshColor2X.a = 0.0f;
				_isAnyColorCalculated = true;
			}

			//MakeTransformMatrix(); < 이 함수 부분
			//World Matrix를 만든다.
			_matrix_TFResult_World.RMultiply(_matrix_TF_ToParent);//변경 : ToParent -> LocalModified -> ParentWorld 순으로 바꾼다.
			_matrix_TFResult_World.RMultiply(_matrix_TF_LocalModified);//<<[R]


			//_matrix_TFResult_World.RMultiply(_matrix_TF_ToParent);//<<[R]

			_matrix_TFResult_World.RMultiply(_matrix_TF_ParentWorld);//<<[R]

			//_matrix_TFResult_WorldWithoutMod.SRMultiply(_matrix_TF_ToParent, true);//ToParent는 넣지 않는다.
			//_matrix_TFResult_WorldWithoutMod.SRMultiply(_matrix_TF_ParentWorld, true);//<<[SR]

			//Without Mod는 계산하지 않았을 경우에만 계산한다.
			//바뀌지 않으므로
			if (!_isCalculateWithoutMod)
			{
				_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ToParent);//<<[R]
				_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ParentWorld_NonModified);//<<[R]


				//리깅용 단축식을 추가한다.
				if (_childMesh != null)
				{
					_vertLocal2MeshWorldMatrix = _matrix_TFResult_WorldWithoutMod.MtrxToSpace;
					_vertLocal2MeshWorldMatrix *= _childMesh._matrix_Vert2Mesh;

					_vertWorld2MeshLocalMatrix = _childMesh._matrix_Vert2Mesh_Inverse;
					_vertWorld2MeshLocalMatrix *= _matrix_TFResult_WorldWithoutMod.MtrxToLowerSpace;

					_vertMeshWorldNoModMatrix = _matrix_TFResult_WorldWithoutMod.MtrxToSpace;
					_vertMeshWorldNoModInverseMatrix = _matrix_TFResult_WorldWithoutMod.MtrxToLowerSpace;
				}

				_isCalculateWithoutMod = true;
			}
			


			//처리된 TRS
			_updatedWorldPos_NoRequest.x = _matrix_TFResult_World._pos.x;
			_updatedWorldPos_NoRequest.y = _matrix_TFResult_World._pos.y;

			_updatedWorldAngle_NoRequest = _matrix_TFResult_World._angleDeg;

			_updatedWorldScale_NoRequest.x = _matrix_TFResult_World._scale.x;
			_updatedWorldScale_NoRequest.y = _matrix_TFResult_World._scale.y;

			_updatedWorldPos = _updatedWorldPos_NoRequest;
			_updatedWorldAngle = _updatedWorldAngle_NoRequest;
			_updatedWorldScale = _updatedWorldScale_NoRequest;



			//스크립트로 외부에서 제어한 경우
			if (_isExternalUpdate_Position)
			{
				_updatedWorldPos.x = (_exUpdate_Pos.x * _externalUpdateWeight) + (_updatedWorldPos.x * (1.0f - _externalUpdateWeight));
				_updatedWorldPos.y = (_exUpdate_Pos.y * _externalUpdateWeight) + (_updatedWorldPos.y * (1.0f - _externalUpdateWeight));
			}

			if (_isExternalUpdate_Rotation)
			{
				_updatedWorldAngle = (_exUpdate_Angle * _externalUpdateWeight) + (_updatedWorldAngle * (1.0f - _externalUpdateWeight));
			}

			if(_isExternalUpdate_Scaling)
			{ 
				_updatedWorldScale.x = (_exUpdate_Scale.x * _externalUpdateWeight) + (_updatedWorldScale.x * (1.0f - _externalUpdateWeight));
				_updatedWorldScale.y = (_exUpdate_Scale.y * _externalUpdateWeight) + (_updatedWorldScale.y * (1.0f - _externalUpdateWeight));
			}

			if (_isExternalUpdate_Position || _isExternalUpdate_Rotation || _isExternalUpdate_Scaling)
			{
				//WorldMatrix를 갱신해주자
				_matrix_TFResult_World.SetTRS(_updatedWorldPos.x, _updatedWorldPos.y,
										_updatedWorldAngle,
										_updatedWorldScale.x, _updatedWorldScale.y);

				_isExternalUpdate_Position = false;
				_isExternalUpdate_Rotation = false;
				_isExternalUpdate_Scaling = false;
			}
			
			





			//추가 : 소켓도 만들어준다.
			//Vert World를 아직 계산하지 않았지만 Socket 처리에는 문제가 없다.
			if(_socketTransform != null)
			{
				_socketTransform.localPosition = new Vector3(_matrix_TFResult_World._pos.x, _matrix_TFResult_World._pos.y, 0);
				_socketTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, _matrix_TFResult_World._angleDeg);
				_socketTransform.localScale = new Vector3(_matrix_TFResult_World._scale.x, _matrix_TFResult_World._scale.y, 1.0f);
			}

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif


			//[MeshUpdate]는 Post Update로 전달

			//3. 자식 호출
			//자식 객체도 업데이트를 한다.
			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].UpdateCalculate_Pre();
				}
			}

		}



		/// <summary>
		/// CalculateStack을 업데이트 한다.
		/// Post-Update이다. Rigging, VertWorld만 처리된다.
		/// </summary>
		public void UpdateCalculate_Post()
		{

//#if UNITY_EDITOR
//			Profiler.BeginSample("Transform - 1. Stack Calculate");
//#endif

			//1. Calculated Stack 업데이트
			if (_calculatedStack != null)
			{
				_calculatedStack.Calculate_Post();
			}

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif


//#if UNITY_EDITOR
//			Profiler.BeginSample("Transform - 3. Mesh Update");
//#endif

			//3. Mesh 업데이트 - 중요
			//실제 Vertex의 위치를 적용
			if (_childMesh != null)
			{
				_childMesh.UpdateCalculate(_calculatedStack.IsRigging,
											_calculatedStack.IsVertexLocal,
											_calculatedStack.IsVertexWorld,
											_isVisible);
			}

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif

			//3. 자식 호출
			//자식 객체도 업데이트를 한다.
			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].UpdateCalculate_Post();
				}
			}

		}


		




		//본 관련 업데이트 코드
		public void ReadyToUpdateBones()
		{
			//if(!_isBoneUpdatable)
			//{
			//	return;
			//}
			if (_boneList_Root != null)
			{
				for (int i = 0; i < _boneList_Root.Length; i++)
				{
					_boneList_Root[i].ReadyToUpdate(true);
				}
			}

			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].ReadyToUpdateBones();
				}
			}
		}


		public void UpdateBonesWorldMatrix()
		{
			if (_boneList_Root != null)
			{
				for (int i = 0; i < _boneList_Root.Length; i++)
				{
					_boneList_Root[i].MakeWorldMatrix(true);
				}
			}

			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].UpdateBonesWorldMatrix();
				}
			}
		}
		
		/// <summary>
		/// Bake용 UpdateBonesWorldMatrix() 함수
		/// </summary>
		public void UpdateBonesWorldMatrixForBake()
		{
			if (_boneList_Root != null)
			{
				for (int i = 0; i < _boneList_Root.Length; i++)
				{
					_boneList_Root[i].ResetBoneMatrixForBake(true);
				}
			}

			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].UpdateBonesWorldMatrixForBake();
				}
			}
		}



		public void RemoveAllCalculateResultParams()
		{
			_calculatedStack.ClearResultParams();
			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].RemoveAllCalculateResultParams();
				}
			}
		}

		public void ResetCalculateStackForBake()
		{
			if(_calculatedStack != null)
			{
				_calculatedStack.ResetVerticesOnBake();
			}

			//CalResultParam을 모두 삭제한다.


			RefreshModifierLink();

			

			if (_childTransforms != null && _childTransforms.Length > 0)
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].ReadyToUpdate();
					_childTransforms[i].ReadyToUpdateBones();
				}

			//UpdateBonesWorldMatrix();//>>TODO
			//Bone Matrix를 초기화 하는게 필요하다.
			if (_boneList_Root != null && _boneList_Root.Length > 0)
			{
				for (int i = 0; i < _boneList_Root.Length; i++)
				{
					_boneList_Root[i].ResetBoneMatrixForBake(true);
				}
			}

			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].ResetCalculateStackForBake();
				}
			}



			
		}


		//public void DebugBoneMatrix()
		//{
		//	//if (string.Equals(this.name, "Body"))
		//	//{
		//	//	Debug.Log("Transform Reset");
		//	//	Debug.Log("[ _matrix_TF_ParentWorld : " + _matrix_TF_ParentWorld.ToString() + "]");
		//	//	Debug.Log("[ _matrix_TF_ParentWorld_NonModified : " + _matrix_TF_ParentWorld_NonModified.ToString() + "]");
		//	//	Debug.Log("[ _matrix_TF_ToParent : " + _matrix_TF_ToParent.ToString() + "]");
		//	//	Debug.Log("[ _matrix_TF_LocalModified : " + _matrix_TF_LocalModified.ToString() + "]");
		//	//	Debug.Log("[ _matrix_TFResult_World : " + _matrix_TFResult_World.ToString() + "]");
		//	//	Debug.Log("[ _matrix_TFResult_WorldWithoutMod : " + _matrix_TFResult_WorldWithoutMod.ToString() + "]");
		//	//}

		//	if (_boneList_Root != null && _boneList_Root.Length > 0)
		//	{
		//		for (int i = 0; i < _boneList_Root.Length; i++)
		//		{
		//			_boneList_Root[i].DebugBoneMatrix();
		//		}
		//	}

		//	if (_childTransforms != null)
		//	{
		//		for (int i = 0; i < _childTransforms.Length; i++)
		//		{
		//			_childTransforms[i].DebugBoneMatrix();
		//		}
		//	}
		//}

	

		public void UpdateMaskMeshes()
		{
			if (_childMesh != null)
			{
				_childMesh.RefreshMaskMesh_WithoutUpdateCalculate();
			}
			

			//자식 객체도 업데이트를 한다.
			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].UpdateMaskMeshes();
				}
			}

		}

		// Functions
		//---------------------------------------------------------------
		// 외부 제어 코드를 넣자
		// <Portrait 기준으로 Local Space = Bone 기준으로 World Space 로 설정한다 >
		public void SetPosition(Vector2 worldPosition, float weight = 1.0f)
		{
			_isExternalUpdate_Position = true;
			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Pos = worldPosition;
		}

		public void SetRotation(float worldAngle, float weight = 1.0f)
		{
			_isExternalUpdate_Rotation = true;
			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Angle = worldAngle;
		}

		public void SetScale(Vector2 worldScale, float weight = 1.0f)
		{
			_isExternalUpdate_Scaling = true;
			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Scale = worldScale;
		}

		public void SetTRS(Vector2 worldPosition, float worldAngle, Vector2 worldScale, float weight = 1.0f)
		{
			_isExternalUpdate_Position = true;
			_isExternalUpdate_Rotation = true;
			_isExternalUpdate_Scaling = true;

			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Pos = worldPosition;
			_exUpdate_Angle = worldAngle;
			_exUpdate_Scale = worldScale;
		}

		// Editor Functions
		//------------------------------------------------
		public void Bake(apPortrait portrait, //apMeshGroup srcMeshGroup, 
							apOptTransform parentTransform,
							apOptRootUnit rootUnit,
							string name,
							int transformID, int meshGroupUniqueID, apMatrix defaultMatrix,
							bool isMesh, int level, int depth,
							bool isVisible_Default,
							Color meshColor2X_Default,
							float zScale
										)
		{
			_portrait = portrait;
			_rootUnit = rootUnit;
			_transformID = transformID;
			_name = name;
			_meshGroupUniqueID = meshGroupUniqueID;

			_parentTransform = parentTransform;

			_defaultMatrix = new apMatrix(defaultMatrix);
			_transform = transform;

			_level = level;
			_depth = depth;

			_isVisible_Default = isVisible_Default;
			_meshColor2X_Default = meshColor2X_Default;

			if (parentTransform != null)
			{
				_depth -= parentTransform._depth;
			}

			//이부분 실험 중
			//1. Default Matrix를 Transform에 적용하고, Modifier 계산에서는 제외하는 경우
			//결과 : Bake시에는 "Preview"를 위해서 DefaultMatrix 위치로 이동을 시키지만, 실행시에는 원점으로 이동시킨다.
			//_transform.localPosition = _defaultMatrix.Pos3 - new Vector3(0.0f, 0.0f, (float)_depth);
			//_transform.localRotation = Quaternion.Euler(0.0f, 0.0f, _defaultMatrix._angleDeg);
			//_transform.localScale = _defaultMatrix._scale;

			//2. Default Matrix를 Modifier에 포함시키고 Transform은 원점인 경우 (Editor와 동일)
			_transform.localPosition = -new Vector3(0.0f, 0.0f, (float)_depth * zScale);
			_transform.localRotation = Quaternion.identity;
			_transform.localScale = Vector3.one;

			if (isMesh)
			{
				_unitType = UNIT_TYPE.Mesh;
			}
			else
			{
				_unitType = UNIT_TYPE.Group;
			}

			_childTransforms = null;
			_childMesh = null;
		}

		public void BakeModifier(apPortrait portrait, apMeshGroup srcMeshGroup)
		{
			if (srcMeshGroup != null)
			{
				_modifierStack.Bake(srcMeshGroup._modifierStack, portrait);
			}
		}

		public void SetChildMesh(apOptMesh optMesh)
		{
			_childMesh = optMesh;
		}

		public void AddChildTransforms(apOptTransform childTransform)
		{
			if (_childTransforms == null)
			{
				_childTransforms = new apOptTransform[1];
				_childTransforms[0] = childTransform;
			}
			else
			{
				apOptTransform[] nextTransform = new apOptTransform[_childTransforms.Length + 1];
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					nextTransform[i] = _childTransforms[i];
				}
				nextTransform[nextTransform.Length - 1] = childTransform;

				_childTransforms = new apOptTransform[nextTransform.Length];
				for (int i = 0; i < nextTransform.Length; i++)
				{
					_childTransforms[i] = nextTransform[i];
				}
			}
		}

		public void ClearResultParams(bool isRecursive = false)
		{
			if (_calculatedStack == null)
			{
				_calculatedStack = new apOptCalculatedResultStack(this);
			}

			//Debug.Log("Clear Param : " + _transformID);
			_calculatedStack.ClearResultParams();
			_modifierStack.ClearAllCalculateParam();

			if(isRecursive)
			{
				if(_childTransforms != null && _childTransforms.Length > 0)
				{
					for (int i = 0; i < _childTransforms.Length; i++)
					{
						_childTransforms[i].ClearResultParams(true);
					}
				}
			}
			
		}

		/// <summary>
		/// [핵심 코드]
		/// Modifier를 업데이트할 수 있도록 연결해준다.
		/// </summary>
		public void RefreshModifierLink()
		{

			if (_calculatedStack == null)
			{
				_calculatedStack = new apOptCalculatedResultStack(this);
			}
			_modifierStack.LinkModifierStackToRenderUnitCalculateStack();
		}



		// Functions
		//------------------------------------------------
		public void Show(bool isChildShow)
		{
			if (_childMesh != null)
			{
				_childMesh.Show(true);
			}

			if (isChildShow)
			{
				if (_childTransforms != null)
				{
					for (int i = 0; i < _childTransforms.Length; i++)
					{
						_childTransforms[i].Show(true);
					}
				}
			}
		}




		public void Hide(bool isChildHide)
		{
			if (_childMesh != null)
			{
				_childMesh.Hide();
			}

			if (isChildHide)
			{
				if (_childTransforms != null)
				{
					for (int i = 0; i < _childTransforms.Length; i++)
					{
						_childTransforms[i].Hide(true);
					}
				}
			}
		}


		public void ShowWhenBake(bool isChildShow)
		{
			if (_childMesh != null)
			{
				_childMesh.SetVisibleByDefault();
			}

			if (isChildShow)
			{
				if (_childTransforms != null)
				{
					for (int i = 0; i < _childTransforms.Length; i++)
					{
						_childTransforms[i].ShowWhenBake(true);
					}
				}
			}
		}

		public void ResetCommandBuffer(bool isRegistToCamera)
		{
			if (_childMesh != null)
			{
				if (isRegistToCamera)
				{
					_childMesh.ResetMaskParentSetting();
				}
				else
				{
					_childMesh.CleanUpMaskParent();
				}
			}
			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].ResetCommandBuffer(isRegistToCamera);
				}
			}
		}

		// Get / Set
		//------------------------------------------------
		public apOptModifierUnitBase GetModifier(int uniqueID)
		{
			return _modifierStack.GetModifier(uniqueID);
		}



		public apOptTransform GetMeshTransform(int uniqueID)
		{
			for (int i = 0; i < _childTransforms.Length; i++)
			{
				if (_childTransforms[i]._unitType == UNIT_TYPE.Mesh
					&& _childTransforms[i]._transformID == uniqueID)
				{
					return _childTransforms[i];
				}
			}
			return null;
		}

		public apOptTransform GetMeshGroupTransform(int uniqueID)
		{
			for (int i = 0; i < _childTransforms.Length; i++)
			{
				if (_childTransforms[i]._unitType == UNIT_TYPE.Group
					&& _childTransforms[i]._transformID == uniqueID)
				{
					return _childTransforms[i];
				}
			}
			return null;
		}


		public apOptTransform GetMeshTransformRecursive(int uniqueID)
		{
			apOptTransform result = GetMeshTransform(uniqueID);
			if (result != null)
			{
				return result;
			}

			apOptTransform curGroupTransform = null;
			for (int i = 0; i < _childTransforms.Length; i++)
			{
				curGroupTransform = _childTransforms[i];
				if (curGroupTransform._unitType != UNIT_TYPE.Group)
				{
					continue;
				}

				result = curGroupTransform.GetMeshTransformRecursive(uniqueID);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		public apOptTransform GetMeshGroupTransformRecursive(int uniqueID)
		{
			apOptTransform result = GetMeshGroupTransform(uniqueID);
			if (result != null)
			{
				return result;
			}

			apOptTransform curGroupTransform = null;
			for (int i = 0; i < _childTransforms.Length; i++)
			{
				curGroupTransform = _childTransforms[i];
				if (curGroupTransform._unitType != UNIT_TYPE.Group)
				{
					continue;
				}

				result = curGroupTransform.GetMeshGroupTransformRecursive(uniqueID);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		public apOptBone GetBone(int uniqueID)
		{
			for (int i = 0; i < _boneList_All.Length; i++)
			{
				if (_boneList_All[i]._uniqueID == uniqueID)
				{
					return _boneList_All[i];
				}
			}

			return null;
		}

		public apOptBone GetBoneRecursive(int uniqueID)
		{
			for (int i = 0; i < _boneList_All.Length; i++)
			{
				if (_boneList_All[i]._uniqueID == uniqueID)
				{
					return _boneList_All[i];
				}
			}


			apOptBone result = null;
			for (int i = 0; i < _childTransforms.Length; i++)
			{
				result = _childTransforms[i].GetBoneRecursive(uniqueID);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}
	}
}