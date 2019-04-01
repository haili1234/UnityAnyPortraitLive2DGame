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

	// MeshGroup에 포함되는 apBone의 Opt 버전
	// MeshGroup에 해당되는 Root OptTransform의 "Bones" GameObject에 포함된다.
	// Matrix 계산은 Bone과 동일하며, Transform에 반영되지 않는다. (Transform은 Local Pos와 Rotation만 계산된다)
	// Transform은 Rigging에 반영되지는 않지만, 만약 어떤 오브젝트를 Attachment 한다면 사용되어야 한다.
	// Opt Bone의 Transform은 외부 입력은 무시하며, Attachment를 하는 용도로만 사용된다.
	// Attachment를 하는 경우 하위에 Socket Transform을 생성한뒤, 거기서 WorldMatrix에 해당하는 TRS를 넣는다. (값 자체는 Local Matrix)
	
		/// <summary>
	/// A class in which "apBone" is baked
	/// It belongs to "apOptTransform".
	/// (It is recommended to use the functions of "apPortrait" to control the position of the bone, and it is recommended to use "Socket" when referring to the position.)
	/// </summary>
	public class apOptBone : MonoBehaviour
	{
		// Members
		//---------------------------------------------------------------
		// apBone 정보를 옮기자
		/// <summary>Bone Name</summary>
		public string _name = "";

		/// <summary>[Please do not use it] Bone's Unique ID</summary>
		public int _uniqueID = -1;

		/// <summary>[Please do not use it] Parent MeshGroup ID</summary>
		public int _meshGroupID = -1;

		//이건 Serialize가 된다.
		/// <summary>Parent Opt-Transform</summary>
		public apOptTransform _parentOptTransform = null;

		/// <summary>
		/// Parent Opt-Bone
		/// </summary>
		public apOptBone _parentBone = null;

		/// <summary>
		/// Children of a Bone
		/// </summary>
		public apOptBone[] _childBones = null;//<ChildBones의 배열버전

		/// <summary>
		/// [Please do not use it] Default Matrix
		/// </summary>
		[SerializeField]
		public apMatrix _defaultMatrix = new apMatrix();

		[NonSerialized]
		private Vector2 _deltaPos = Vector2.zero;

		[NonSerialized]
		private float _deltaAngle = 0.0f;

		[NonSerialized]
		private Vector2 _deltaScale = Vector2.one;

		/// <summary>[Please do not use it] Local Matrix</summary>
		[NonSerialized]
		public apMatrix _localMatrix = new apMatrix();
		
		/// <summary>[Please do not use it] World Matrix</summary>
		[NonSerialized]
		public apMatrix _worldMatrix = new apMatrix();

		/// <summary>
		/// [Please do not use it] World Matrix (Default)
		/// </summary>
		[NonSerialized]
		public apMatrix _worldMatrix_NonModified = new apMatrix();

		/// <summary>
		/// [Please do not use it] Rigging Matrix
		/// </summary>
		//리깅을 위한 통합 Matrix
		[NonSerialized]
		public apMatrix3x3 _vertWorld2BoneModWorldMatrix = new apMatrix3x3();//<<이게 문제

		


		//Shape 계열
		/// <summary>
		/// [Please do not use it] Bone Color in Editor / Gizmo
		/// </summary>
		[SerializeField]
		public Color _color = Color.white;

		public int _shapeWidth = 30;
		public int _shapeLength = 50;//<<이 값은 생성할 때 Child와의 거리로 판단한다.
		public int _shapeTaper = 100;//기본값은 뾰족

#if UNITY_EDITOR
		private Vector2 _shapePoint_End = Vector3.zero;

		private Vector2 _shapePoint_Mid1 = Vector3.zero;
		private Vector2 _shapePoint_Mid2 = Vector3.zero;
		private Vector2 _shapePoint_End1 = Vector3.zero;
		private Vector2 _shapePoint_End2 = Vector3.zero;
#endif

		//IK 정보
		/// <summary>[Please do not use it]</summary>
		public apBone.OPTION_LOCAL_MOVE _optionLocalMove = apBone.OPTION_LOCAL_MOVE.Disabled;

		/// <summary>[Please do not use it] Bone's IK Type</summary>
		public apBone.OPTION_IK _optionIK = apBone.OPTION_IK.IKSingle;

		// Parent로부터 IK의 대상이 되는가? IK Single일 때에도 Tail이 된다.
		// (자신이 IK를 설정하는 것과는 무관함)
		/// <summary> [Please do not use it] </summary>
		public bool _isIKTail = false;

		//IK의 타겟과 Parent
		/// <summary>[Please do not use it]</summary>
		public int _IKTargetBoneID = -1;

		/// <summary>[Please do not use it]</summary>
		public apOptBone _IKTargetBone = null;

		/// <summary>[Please do not use it]</summary>
		public int _IKNextChainedBoneID = -1;

		/// <summary>[Please do not use it]</summary>
		public apOptBone _IKNextChainedBone = null;


		// IK Tail이거나 IK Chained 상태라면 Header를 저장하고, Chaining 처리를 해야한다.
		/// <summary>[Please do not use it]</summary>
		public int _IKHeaderBoneID = -1;

		/// <summary>[Please do not use it]</summary>
		public apOptBone _IKHeaderBone = null;



		//IK시 추가 옵션

		// IK 적용시, 각도를 제한을 줄 것인가 (기본값 False)
		/// <summary>[Please do not use it] IK Angle Contraint Option</summary>
		public bool _isIKAngleRange = false;

		/// <summary>[Please do not use it]</summary>
		public float _IKAngleRange_Lower = -90.0f;//음수여야 한다.

		/// <summary>[Please do not use it]</summary>
		public float _IKAngleRange_Upper = 90.0f;//양수여야 한다.

		/// <summary>[Please do not use it]</summary>
		public float _IKAnglePreferred = 0.0f;//선호하는 각도 Offset



		// IK 연산이 되었는가
		/// <summary>
		/// Is IK Calculated
		/// </summary>
		[NonSerialized]
		public bool _isIKCalculated = false;

		// IK 연산이 발생했을 경우, World 좌표계에서 Angle을 어떻게 만들어야 하는지 계산 결과값
		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public float _IKRequestAngleResult = 0.0f;

		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public float _IKRequestWeight = 0.0f;
		

		/// <summary>
		/// IK 계산을 해주는 Chain Set.
		/// </summary>
		[SerializeField]
		private apOptBoneIKChainSet _IKChainSet = null;//<<이거 Opt 버전으로 만들자

		private bool _isIKChainInit = false;



		//추가 : 이건 나중에 세팅하자
		//Transform에 적용되는 Local Matrix 값 (Scale이 없다)
		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public apMatrix _transformLocalMatrix = new apMatrix();

		//Attach시 만들어지는 Socket
		//Socket 옵션은 Bone에서 미리 세팅해야한다.
		/// <summary>
		/// Socket Transform.
		/// In Unity World, this is a Socket that actually has the position, rotation, and size of the bone. 
		/// If you want to refer to the position or rotation of the bone from the outside, it is recommended to use Socket.
		/// </summary>
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
		//---------------------------------------------------------------
		void Start()
		{
			//업데이트 안합니더
			this.enabled = false;

			_isExternalUpdate_Position = false;
			_isExternalUpdate_Rotation = false;
			_isExternalUpdate_Scaling = false;
		}


		//Link 함수의 내용은 Bake 시에 진행해야한다.
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="bone"></param>
		public void Bake(apBone bone)
		{
			_name = bone._name;
			_uniqueID = bone._uniqueID;
			_meshGroupID = bone._meshGroupID;
			_defaultMatrix.SetMatrix(bone._defaultMatrix);


			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;

			_localMatrix.SetIdentity();

			_worldMatrix.SetIdentity();

			_worldMatrix_NonModified.SetIdentity();
			_vertWorld2BoneModWorldMatrix.SetIdentity();

			_color = bone._color;
			_shapeWidth = bone._shapeWidth;
			_shapeLength = bone._shapeLength;
			_shapeTaper = bone._shapeTaper;

			_optionLocalMove = bone._optionLocalMove;
			_optionIK = bone._optionIK;

			_isIKTail = bone._isIKTail;

			_IKTargetBoneID = bone._IKTargetBoneID;
			_IKTargetBone = null;//<<나중에 링크

			_IKNextChainedBoneID = bone._IKNextChainedBoneID;
			_IKNextChainedBone = null;//<<나중에 링크


			_IKHeaderBoneID = bone._IKHeaderBoneID;
			_IKHeaderBone = null;//<<나중에 링크


			_isIKAngleRange = bone._isIKAngleRange;
			//이게 기존 코드
			//_IKAngleRange_Lower = bone._IKAngleRange_Lower;
			//_IKAngleRange_Upper = bone._IKAngleRange_Upper;
			//_IKAnglePreferred = bone._IKAnglePreferred;

			//이게 변경된 IK 코드
			_IKAngleRange_Lower = bone._defaultMatrix._angleDeg + bone._IKAngleRange_Lower;
			_IKAngleRange_Upper = bone._defaultMatrix._angleDeg + bone._IKAngleRange_Upper;
			_IKAnglePreferred = bone._defaultMatrix._angleDeg + bone._IKAnglePreferred;


			_isIKCalculated = false;
			_IKRequestAngleResult = 0.0f;
			_IKRequestWeight = 0.0f;

			_socketTransform = null;

			_transformLocalMatrix.SetIdentity();

			_childBones = null;

			_isIKChainInit = false;
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="targetOptTransform"></param>
		public void Link(apOptTransform targetOptTransform)
		{
			_parentOptTransform = targetOptTransform;
			if (_parentOptTransform == null)
			{
				//??
				Debug.LogError("[" + transform.name + "] ParentOptTransform of OptBone is Null [" + _meshGroupID + "]");
				_IKTargetBone = null;
				_IKNextChainedBone = null;
				_IKHeaderBone = null;

				//LinkBoneChaining();


				return;
			}


			_IKTargetBone = _parentOptTransform.GetBone(_IKTargetBoneID);
			_IKNextChainedBone = _parentOptTransform.GetBone(_IKNextChainedBoneID);
			_IKHeaderBone = _parentOptTransform.GetBone(_IKHeaderBoneID);

			//LinkBoneChaining();

		}



		//여기서는 LinkBoneChaining만 진행
		// Bone Chaining 직후에 재귀적으로 호출한다.
		// Tail이 가지는 -> Head로의 IK 리스트를 만든다.
		/// <summary>
		/// [Please do not use it] IK Link
		/// </summary>
		public void LinkBoneChaining()
		{
			if (_localMatrix == null)
			{
				_localMatrix = new apMatrix();
			}
			if (_worldMatrix == null)
			{
				_worldMatrix = new apMatrix();
			}
			if (_worldMatrix_NonModified == null)
			{
				_worldMatrix_NonModified = new apMatrix();
			}


			if (_isIKTail)
			{
				apOptBone curParentBone = _parentBone;
				apOptBone headBone = _IKHeaderBone;

				bool isParentExist = (curParentBone != null);
				bool isHeaderExist = (headBone != null);
				bool isHeaderIsInParents = false;
				if (isParentExist && isHeaderExist)
				{
					isHeaderIsInParents = (GetParentRecursive(headBone._uniqueID) != null);
				}


				if (isParentExist && isHeaderExist && isHeaderIsInParents)
				{
					if (_IKChainSet == null)
					{
						_IKChainSet = new apOptBoneIKChainSet(this);
					}
					//Chain을 Refresh한다.
					//_IKChainSet.RefreshChain();//<<수정. 이건 Runtime에 해야한다.
				}
				else
				{
					_IKChainSet = null;

					Debug.LogError("[" + transform.name + "] IK Chaining Error : Parent -> Chain List Connection Error "
						+ "[ Parent : " + isParentExist
						+ " / Header : " + isHeaderExist
						+ " / IsHeader Is In Parent : " + isHeaderIsInParents + " ]");
				}
			}
			else
			{
				_IKChainSet = null;
			}

			if (_childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].LinkBoneChaining();
				}
			}

		}


		// Update
		//---------------------------------------------------------------
		// Update Transform Matrix를 초기화한다.
		public void ReadyToUpdate(bool isRecursive)
		{
			//_localModifiedTransformMatrix.SetIdentity();

			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;

			//_isIKCalculated = false;
			//_IKRequestAngleResult = 0.0f;

			//_worldMatrix.SetIdentity();
			if (isRecursive && _childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].ReadyToUpdate(true);
				}
			}
		}

		/// <summary>
		/// Bake를 위해서 BoneMatrix를 초기화한다.
		/// </summary>
		/// <param name="isRecursive"></param>
		public void ResetBoneMatrixForBake(bool isRecursive)
		{
			
			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;

			_localMatrix.SetIdentity();
			_worldMatrix.SetIdentity();

			_worldMatrix_NonModified.SetIdentity();
			_vertWorld2BoneModWorldMatrix.SetIdentity();

			
			if (_parentBone == null)
			{
				_worldMatrix.SetMatrix(_defaultMatrix);
				_worldMatrix.Add(_localMatrix);

				_worldMatrix_NonModified.SetMatrix(_defaultMatrix);//Local Matrix 없이 Default만 지정


				if (_parentOptTransform != null)
				{
					//Debug.Log("SetParentOptTransform Matrix : [" + _parentOptTransform.transform.name + "] : " + _parentOptTransform._matrix_TFResult_World.Scale2);
					//Non Modified도 동일하게 적용
					//렌더유닛의 WorldMatrix를 넣어주자
					_worldMatrix.RMultiply(_parentOptTransform._matrix_TFResult_WorldWithoutMod);//RenderUnit의 WorldMatrixWrap의 Opt 버전
					

					_worldMatrix_NonModified.RMultiply(_parentOptTransform._matrix_TFResult_WorldWithoutMod);

				}
			}
			else
			{
				_worldMatrix.SetMatrix(_defaultMatrix);
				_worldMatrix.Add(_localMatrix);
				_worldMatrix.RMultiply(_parentBone._worldMatrix_NonModified);

				_worldMatrix_NonModified.SetMatrix(_defaultMatrix);//Local Matrix 없이 Default만 지정
				_worldMatrix_NonModified.RMultiply(_parentBone._worldMatrix_NonModified);
			}

			_worldMatrix.SetMatrix(_worldMatrix_NonModified);

			_vertWorld2BoneModWorldMatrix = _worldMatrix_NonModified.MtrxToSpace;
			_vertWorld2BoneModWorldMatrix *= _worldMatrix_NonModified.MtrxToLowerSpace;


			
			//Debug.Log("Reset Bone Matrix [" + this.name + "]");
			//Debug.Log("World Matrix [ " + _worldMatrix.ToString() + "]");

			if (isRecursive)
			{
				if (_childBones != null && _childBones.Length > 0)
				{
					for (int i = 0; i < _childBones.Length; i++)
					{
						_childBones[i].ResetBoneMatrixForBake(true);
					}
				}
			}
			
		}



		//public void DebugBoneMatrix()
		//{
		//	//if (string.Equals(this.name, "Bone 1"))
		//	//{
		//	//	Debug.LogError("Debug Bone Matrix (After Update)");
		//	//	Debug.Log(this.name + " / Local Modified [ " + _localMatrix.ToString() + " ]");
		//	//	Debug.Log(this.name + " / World [ " + _worldMatrix.ToString() + " ]");

		//	//	Debug.Log("Delta : Pos : " + _deltaPos + " / Angle : " + _deltaAngle + " / Scale : " + _deltaScale);
		//	//}

		//	if (_childBones != null && _childBones.Length > 0)
		//	{
		//		for (int i = 0; i < _childBones.Length; i++)
		//		{
		//			_childBones[i].DebugBoneMatrix();
		//		}
		//	}
		//}






		// 2) Update된 TRS 값을 넣는다.
		public void UpdateModifiedValue(Vector2 deltaPos, float deltaAngle, Vector2 deltaScale)
		{
			
			_deltaPos = deltaPos;
			_deltaAngle = deltaAngle;
			_deltaScale = deltaScale;
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="IKAngle"></param>
		/// <param name="weight"></param>
		public void AddIKAngle(float IKAngle, float weight)
		{
			//Debug.Log("IK [" + _name + "] : " + IKAngle);
			_isIKCalculated = true;
			//_IKRequestAngleResult += IKAngle;
			_IKRequestWeight = weight;
			_IKRequestAngleResult += IKAngle;
		}

		// 4) World Matrix를 만든다.
		// 이 함수는 Parent의 MeshGroupTransform이 연산된 후 -> Vertex가 연산되기 전에 호출되어야 한다.
		public void MakeWorldMatrix(bool isRecursive)
		{
			_localMatrix.SetIdentity();
			_localMatrix._pos = _deltaPos;
			_localMatrix._angleDeg = _deltaAngle;
			_localMatrix._scale.x = _deltaScale.x;
			_localMatrix._scale.y = _deltaScale.y;

			_localMatrix.MakeMatrix();

			//World Matrix = ParentMatrix x LocalMatrix
			//Root인 경우에는 MeshGroup의 Matrix를 이용하자

			//_invWorldMatrix_NonModified.SetIdentity();

			if (_parentBone == null)
			{
				_worldMatrix.SetMatrix(_defaultMatrix);
				_worldMatrix.Add(_localMatrix);

				_worldMatrix_NonModified.SetMatrix(_defaultMatrix);//Local Matrix 없이 Default만 지정


				if (_parentOptTransform != null)
				{
					//Debug.Log("SetParentOptTransform Matrix : [" + _parentOptTransform.transform.name + "] : " + _parentOptTransform._matrix_TFResult_World.Scale2);
					//Non Modified도 동일하게 적용
					//렌더유닛의 WorldMatrix를 넣어주자
					_worldMatrix.RMultiply(_parentOptTransform._matrix_TFResult_World);//RenderUnit의 WorldMatrixWrap의 Opt 버전

					_worldMatrix_NonModified.RMultiply(_parentOptTransform._matrix_TFResult_WorldWithoutMod);

				}
			}
			else
			{
				_worldMatrix.SetMatrix(_defaultMatrix);
				_worldMatrix.Add(_localMatrix);
				_worldMatrix.RMultiply(_parentBone._worldMatrix);

				_worldMatrix_NonModified.SetMatrix(_defaultMatrix);//Local Matrix 없이 Default만 지정
				_worldMatrix_NonModified.RMultiply(_parentBone._worldMatrix_NonModified);
			}


			//처리된 TRS
			_updatedWorldPos_NoRequest.x = _worldMatrix._pos.x;
			_updatedWorldPos_NoRequest.y = _worldMatrix._pos.y;

			_updatedWorldAngle_NoRequest = _worldMatrix._angleDeg;

			_updatedWorldScale_NoRequest.x = _worldMatrix._scale.x;
			_updatedWorldScale_NoRequest.y = _worldMatrix._scale.y;

			_updatedWorldPos = _updatedWorldPos_NoRequest;
			_updatedWorldAngle = _updatedWorldAngle_NoRequest;
			_updatedWorldScale = _updatedWorldScale_NoRequest;


			if(_isIKCalculated)
			{
				_IKRequestAngleResult -= 90.0f;
				while(_IKRequestAngleResult > 180.0f)	{ _IKRequestAngleResult -= 360.0f; }
				while(_IKRequestAngleResult < -180.0f)	{ _IKRequestAngleResult += 360.0f; }

				//_updatedWorldAngle += _IKRequestAngleResult * _IKRequestWeight;
				_updatedWorldAngle = _updatedWorldAngle * (1.0f - _IKRequestWeight) + (_IKRequestAngleResult * _IKRequestWeight);
				//Debug.Log("Add IK [" + _name + "] : " + _IKRequestAngleResult);

				_IKRequestAngleResult = 0.0f;
				_IKRequestWeight = 0.0f;
			}



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

			if (_isIKCalculated || _isExternalUpdate_Position || _isExternalUpdate_Rotation || _isExternalUpdate_Scaling)
			{
				//WorldMatrix를 갱신해주자
				_worldMatrix.SetTRS(_updatedWorldPos.x, _updatedWorldPos.y,
										_updatedWorldAngle,
										_updatedWorldScale.x, _updatedWorldScale.y);

				_isIKCalculated = false;
				_isExternalUpdate_Position = false;
				_isExternalUpdate_Rotation = false;
				_isExternalUpdate_Scaling = false;
			}
			
			
			

			//World Matrix는 MeshGroup과 동일한 Space의 값을 가진다.
			//그러나 실제로 Bone World Matrix는
			//Root - MeshGroup...(Rec) - Bone Group - Bone.. (Rec <- 여기)
			//의 레벨을 가진다.
			//Root 밑으로는 모두 World에 대해서 동일한 Space를 가지므로
			//Root를 찾아서 Scale을 제어하자...?
			//일단 Parent에서 빼두자
			//_transformLocalMatrix.SetMatrix(_worldMatrix);


#if UNITY_EDITOR
			_shapePoint_End = new Vector2(0.0f, _shapeLength);


			_shapePoint_Mid1 = new Vector2(-_shapeWidth * 0.5f, _shapeLength * 0.2f);
			_shapePoint_Mid2 = new Vector2(_shapeWidth * 0.5f, _shapeLength * 0.2f);

			float taperRatio = Mathf.Clamp01((float)(100 - _shapeTaper) / 100.0f);

			_shapePoint_End1 = new Vector2(-_shapeWidth * 0.5f * taperRatio, _shapeLength);
			_shapePoint_End2 = new Vector2(_shapeWidth * 0.5f * taperRatio, _shapeLength);

			_shapePoint_End = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End);
			_shapePoint_Mid1 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_Mid1);
			_shapePoint_Mid2 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_Mid2);
			_shapePoint_End1 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End1);
			_shapePoint_End2 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End2);
#endif


			//Rigging을 위해서 Matrix 통합 식을 만들자
			//실제 식
			// world * default_inv * VertPos W
			_vertWorld2BoneModWorldMatrix = _worldMatrix.MtrxToSpace;
			_vertWorld2BoneModWorldMatrix *= _worldMatrix_NonModified.MtrxToLowerSpace;

			
			


			if (_socketTransform != null)
			{
				//소켓을 업데이트 하자
				_socketTransform.localPosition = new Vector3(_worldMatrix._pos.x, _worldMatrix._pos.y, 0);
				_socketTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, _worldMatrix._angleDeg);
				_socketTransform.localScale = new Vector3(_worldMatrix._scale.x, _worldMatrix._scale.y, 1.0f);
				
			}
			
			//Child도 호출해준다.
			if (isRecursive && _childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].MakeWorldMatrix(true);
				}
			}
		}





		// Functions
		//---------------------------------------------------------------
		// 외부 제어 코드를 넣자
		// <Portrait 기준으로 Local Space = Bone 기준으로 World Space 로 설정한다 >
		/// <summary>
		/// Set Position
		/// </summary>
		/// <param name="worldPosition"></param>
		/// <param name="weight"></param>
		public void SetPosition(Vector2 worldPosition, float weight = 1.0f)
		{
			_isExternalUpdate_Position = true;
			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Pos = worldPosition;
		}

		/// <summary>
		/// Set Rotation
		/// </summary>
		/// <param name="worldAngle"></param>
		/// <param name="weight"></param>
		public void SetRotation(float worldAngle, float weight = 1.0f)
		{
			_isExternalUpdate_Rotation = true;
			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Angle = worldAngle;
		}


		/// <summary>
		/// Set Scale
		/// </summary>
		/// <param name="worldScale"></param>
		/// <param name="weight"></param>
		public void SetScale(Vector2 worldScale, float weight = 1.0f)
		{
			_isExternalUpdate_Scaling = true;
			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Scale = worldScale;
		}

		/// <summary>
		/// Set TRS (Position, Rotation, Scale)
		/// </summary>
		/// <param name="worldPosition"></param>
		/// <param name="worldAngle"></param>
		/// <param name="worldScale"></param>
		/// <param name="weight"></param>
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


		// IK 요청을 하자
		//-------------------------------------------------------------------------------
		/// <summary>
		/// IK is calculated. Depending on the location requested, all Bones connected by IK move automatically.
		/// </summary>
		/// <param name="targetPosW"></param>
		/// <param name="weight"></param>
		/// <param name="isContinuous"></param>
		/// <returns></returns>
		public bool RequestIK(Vector2 targetPosW, float weight, bool isContinuous)
		{
			if (!_isIKTail || _IKChainSet == null)
			{
				//Debug.LogError("End -> _isIKTail : " + _isIKTail + " / _IKChainSet : " + _IKChainSet);
				return false;
			}

			if(!_isIKChainInit)
			{
				InitIKChain();
				_isIKChainInit = true;
			}


			bool isSuccess = _IKChainSet.SimulateIK(targetPosW, isContinuous);

			//IK가 실패하면 패스
			if (!isSuccess)
			{
				//Debug.LogError("Failed");
				return false;
			}

			//IK 결과값을 Bone에 넣어주자
			_IKChainSet.AdaptIKResultToBones(weight);

			//Debug.Log("Success");

			return true;
		}

		/// <summary>
		/// [Please do not use it] Initialize IK Chain
		/// </summary>
		public void InitIKChain()
		{
			if(_IKChainSet != null)
			{
				_IKChainSet.RefreshChain();
			}

			if (_childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].InitIKChain();
				}
			}
		}

		// Get / Set
		//---------------------------------------------------------------
		// boneID를 가지는 Bone을 자식 노드로 두고 있는가.
		// 재귀적으로 찾는다.
		public apOptBone GetChildBoneRecursive(int boneID)
		{
			if (_childBones == null)
			{
				return null;
			}
			//바로 아래의 자식 노드를 검색
			for (int i = 0; i < _childBones.Length; i++)
			{
				if (_childBones[i]._uniqueID == boneID)
				{
					return _childBones[i];
				}
			}

			//못찾았다면..
			//재귀적으로 검색해보자

			for (int i = 0; i < _childBones.Length; i++)
			{
				apOptBone result = _childBones[i].GetChildBoneRecursive(boneID);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		// 바로 아래의 자식 Bone을 검색한다.
		public apOptBone GetChildBone(int boneID)
		{
			//바로 아래의 자식 노드를 검색
			for (int i = 0; i < _childBones.Length; i++)
			{
				if (_childBones[i]._uniqueID == boneID)
				{
					return _childBones[i];
				}
			}

			return null;
		}

		// 자식 Bone 중에서 특정 Target Bone을 재귀적인 자식으로 가지는 시작 Bone을 찾는다.
		public apOptBone FindNextChainedBone(int targetBoneID)
		{
			//바로 아래의 자식 노드를 검색
			if (_childBones == null)
			{
				return null;
			}
			for (int i = 0; i < _childBones.Length; i++)
			{
				if (_childBones[i]._uniqueID == targetBoneID)
				{
					return _childBones[i];
				}
			}

			//못찾았다면..
			//재귀적으로 검색해서, 그 중에 실제로 Target Bone을 포함하는 Child Bone을 리턴하자

			for (int i = 0; i < _childBones.Length; i++)
			{
				apOptBone result = _childBones[i].GetChildBoneRecursive(targetBoneID);
				if (result != null)
				{
					//return result;
					return _childBones[i];//<<Result가 아니라, ChildBone을 리턴
				}
			}
			return null;
		}

		// 요청한 boneID를 가지는 Bone을 부모 노드로 두고 있는가.
		// 재귀적으로 찾는다.
		public apOptBone GetParentRecursive(int boneID)
		{
			if (_parentBone == null)
			{
				return null;
			}

			if (_parentBone._uniqueID == boneID)
			{
				return _parentBone;
			}

			//재귀적으로 검색해보자
			return _parentBone.GetParentRecursive(boneID);

		}


		//-----------------------------------------------------------------------------------------------
		/// <summary>Bone's Position</summary>
		public Vector3 Position { get { return _updatedWorldPos; } }

		/// <summary>Bone's Angle (Degree)</summary>
		public float Angle {  get { return _updatedWorldAngle; } }

		/// <summary>Bone's Scale</summary>
		public Vector3 Scale { get { return _updatedWorldScale; } }

		
		/// <summary>Bone's Position without User's external request</summary>
		public Vector3 PositionWithouEditing {  get { return _updatedWorldPos_NoRequest; } }
		
		/// <summary>Bone's Angle without User's external request</summary>
		public float AngleWithouEditing {  get { return _updatedWorldAngle_NoRequest; } }
		
		/// <summary>Bone's Scale without User's external request</summary>
		public Vector3 ScaleWithouEditing {  get { return _updatedWorldScale_NoRequest; } }


		//-----------------------------------------------------------------------------------------------


//		// Gizmo Event
//#if UNITY_EDITOR
//		void OnDrawGizmosSelected()
//		{
//			Gizmos.color = _color;

//			Matrix4x4 tfMatrix = transform.localToWorldMatrix;
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_worldMatrix._pos), tfMatrix.MultiplyPoint3x4(_shapePoint_End));

//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_worldMatrix._pos), tfMatrix.MultiplyPoint3x4(_shapePoint_Mid1));
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_worldMatrix._pos), tfMatrix.MultiplyPoint3x4(_shapePoint_Mid2));
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_shapePoint_Mid1), tfMatrix.MultiplyPoint3x4(_shapePoint_End1));
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_shapePoint_Mid2), tfMatrix.MultiplyPoint3x4(_shapePoint_End2));
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_shapePoint_End1), tfMatrix.MultiplyPoint3x4(_shapePoint_End2));
//		}
//#endif
	}

}