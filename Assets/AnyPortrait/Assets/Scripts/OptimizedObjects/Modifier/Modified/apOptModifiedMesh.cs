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
	/// Modifier에서 작성된 "변경 사항" 중 Mesh에 대한 데이터가 저장되는 클래스
	/// Modifier Vertex를 가지고 있어서 계산시 데이터를 제공한다.
	/// RealTime 전용
	/// </summary>
	[Serializable]
	public class apOptModifiedMesh
	{
		// Members
		//--------------------------------------------
		public apPortrait _portrait = null;

		//public apModifiedMesh.TARGET_TYPE _targetType = apModifiedMesh.TARGET_TYPE.MeshTransformOnly;
		public apModifiedMesh.MOD_VALUE_TYPE _modValueType = apModifiedMesh.MOD_VALUE_TYPE.Unknown;

		//적용 대상
		//에디터와 달리 바로 Monobehaviour를 저장하자.
		public apOptMesh _targetMesh = null;
		public apOptTransform _targetTransform = null;
		public apOptTransform _rootTransform = null;

		public int _rootMeshGroupUniqueID = -1;

		public int _meshUniqueID = -1;
		public int _transformUniqueID = -1;

		//public int _boneUniqueID = -1;

		public bool _isMeshTransform = true;

		//TODO : Bone

		//1. Mesh 타입인 경우
		//-> Vertex 리스트 (배열로 한다)
		public int _nVerts = 0;
		public int _nVertRigs = 0;
		public int _nVertWeights = 0;

		[SerializeField]
		public apOptModifiedVertex[] _vertices = null;

		[SerializeField]
		public apOptModifiedVertexRig[] _vertRigs = null;

		[SerializeField]
		public apOptModifiedVertexWeight[] _vertWeights = null;


		//2. Transform 타입인 경우
		//-> Transform 변동사항
		[SerializeField]
		public apMatrix _transformMatrix = new apMatrix();

		[SerializeField]
		public Color _meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		[SerializeField]
		public bool _isVisible = true;


		//추가
		//물리 파라미터
		[SerializeField]
		public bool _isUsePhysicParam = false; //<<Bake하자

		[SerializeField]
		private apOptPhysicsMeshParam _physicMeshParam = new apOptPhysicsMeshParam();

		public apOptPhysicsMeshParam PhysicParam
		{
			get
			{
				if (_isUsePhysicParam)
				{
					if (_physicMeshParam == null)
					{ _physicMeshParam = new apOptPhysicsMeshParam(); }
					return _physicMeshParam;
				}
				return null;
			}
		}

		// Init
		//--------------------------------------------
		public apOptModifiedMesh()
		{

		}

		public void Link(apPortrait portrait)
		{
			//Portrait를 기준으로 Link를 해야한다.
			_portrait = portrait;

			//필요한 경우 Link 추가

			if (_physicMeshParam != null && _isUsePhysicParam)
			{
				_physicMeshParam.Link(_portrait);
			}

			if (_nVertWeights > 0)
			{
				for (int i = 0; i < _nVertWeights; i++)
				{
					_vertWeights[i].Link(this,
											_targetTransform,
											_targetMesh,
											_targetMesh.RenderVertices[_vertWeights[i]._vertIndex]);
				}
			}
		}

		// Init - Bake
		//--------------------------------------------
		public bool Bake(apModifiedMesh srcModMesh, apPortrait portrait)
		{
			_portrait = portrait;
			_rootMeshGroupUniqueID = srcModMesh._meshGroupUniqueID_Modifier;

			_meshUniqueID = srcModMesh._meshUniqueID;
			_transformUniqueID = srcModMesh._transformUniqueID;

			//_boneUniqueID = srcModMesh._boneUniqueID;

			_isMeshTransform = srcModMesh._isMeshTransform;

			apOptTransform rootTransform = _portrait.GetOptTransformAsMeshGroup(_rootMeshGroupUniqueID);
			apOptTransform targetTransform = _portrait.GetOptTransform(_transformUniqueID);

			if (targetTransform == null)
			{
				Debug.LogError("Bake 실패 : 찾을 수 없는 연결된 OptTransform [" + _transformUniqueID + "]");
				Debug.LogError("이미 삭제된 객체에 연결된 ModMesh가 아닌지 확인해보세염");
				return false;
			}
			apOptMesh targetMesh = null;
			if (targetTransform._unitType == apOptTransform.UNIT_TYPE.Mesh)
			{
				targetMesh = targetTransform._childMesh;
			}



			if (rootTransform == null)
			{
				Debug.LogError("ModifiedMesh 연동 에러 : 알수 없는 RootTransform");
				return false;
			}

			//_targetType = srcModMesh._targetType;
			_modValueType = srcModMesh._modValueType;

			//switch (srcModMesh._targetType)
			Color meshColor = srcModMesh._meshColor;
			if (!srcModMesh._isVisible)
			{
				meshColor.a = 0.0f;
			}

			_isUsePhysicParam = srcModMesh._isUsePhysicParam;
			if (_isUsePhysicParam)
			{
				_physicMeshParam = new apOptPhysicsMeshParam();
				_physicMeshParam.Bake(srcModMesh.PhysicParam);
				_physicMeshParam.Link(_portrait);
			}

			//Modifier Value에 맞게 Bake를 하자
			if ((int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexPosList) != 0)
			{

				Bake_VertexMorph(rootTransform,
									targetTransform,
									targetMesh,
									srcModMesh._vertices,
									meshColor,
									srcModMesh._isVisible);
			}
			else if ((int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.TransformMatrix) != 0)
			{
				if (srcModMesh._isMeshTransform)
				{
					Bake_MeshTransform(rootTransform,
										targetTransform,
										targetMesh,
										srcModMesh._transformMatrix,
										meshColor,
										srcModMesh._isVisible);
				}
				else
				{
					Bake_MeshGroupTransform(rootTransform,
											targetTransform,
											srcModMesh._transformMatrix,
											meshColor,
											srcModMesh._isVisible);
				}
			}
			else if ((int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.BoneVertexWeightList) != 0)
			{
				//추가 : VertRig 데이터를 넣는다.
				Bake_VertexRigs(rootTransform, targetTransform, targetMesh, srcModMesh._vertRigs);
			}
			else if ((int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics) != 0
				|| (int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Volume) != 0)
			{
				Bake_VertexWeights(rootTransform, targetTransform, targetMesh, srcModMesh._vertWeights);
			}
			else
			{
				Debug.LogError("연동 에러 : 알 수 없는 ModifierMesh 타입 : " + srcModMesh._modValueType);
				return false;
			}
			#region [미사용 코드] ModValue를 고정된 타입으로 Bake할때 (코드 손상됨)
			//switch(srcModMesh._modValueType)
			//{
			//	//case apModifiedMesh.TARGET_TYPE.VertexWithMeshTransform:
			//	case apModifiedMesh.MOD_VALUE_TYPE.VertexPosList:
			//		{
			//			//TODO : 여기서부터 연동하자.

			//		}
			//		break;

			//	//case apModifiedMesh.TARGET_TYPE.MeshTransformOnly:
			//	case apModifiedMesh.MOD_VALUE_TYPE.TransformMatrix:
			//		{

			//		}
			//		break;

			//	case apModifiedMesh.MOD_VALUE_TYPE.Color:
			//		{
			//			Debug.LogError("TODO : OptModMesh에서 Color에 대한 처리를 해야한다.");
			//		}
			//		break;

			//	case apModifiedMesh.MOD_VALUE_TYPE.BoneVertexWeightList:
			//		{
			//			Debug.LogError("TODO : 본 연동");
			//		}
			//		break;

			//	//case apModifiedMesh.TARGET_TYPE.Bone:
			//	case apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList:
			//		{
			//			Debug.LogError("TODO : 버텍스 가중시 연동");
			//		}
			//		break;

			//	default:
			//		//Debug.LogError("연동 에러 : 알수 없는 ModifierMesh 타입 : " + srcModMesh._targetType);
			//		Debug.LogError("연동 에러 : 알 수 없는 ModifierMesh 타입 : " + srcModMesh._modValueType);
			//		break;
			//} 
			#endregion

			return true;
		}

		// Init - 값 넣기 (값복사)
		//--------------------------------------------
		//연동을 해주자 (apModifiedMesh에서 Init/Link 계열 함수)
		public void Bake_VertexMorph(apOptTransform rootTransform, apOptTransform targetTransform,
										apOptMesh targetMesh, List<apModifiedVertex> modVerts, Color meshColor, bool isVisible)
		{
			//_targetType = apModifiedMesh.TARGET_TYPE.VertexWithMeshTransform;

			_rootTransform = rootTransform;
			_targetTransform = targetTransform;
			_targetMesh = targetMesh;

			if (_targetMesh == null)
			{
				Debug.LogError("Vert Morph인데 Target Mesh가 Null");
				Debug.LogError("Target Transform [" + _targetTransform.transform.name + "]");
			}

			_nVerts = modVerts.Count;
			_vertices = new apOptModifiedVertex[_nVerts];
			for (int i = 0; i < _nVerts; i++)
			{
				apOptModifiedVertex optModVert = new apOptModifiedVertex();
				apModifiedVertex srcModVert = modVerts[i];
				optModVert.Bake(srcModVert, _targetMesh);

				_vertices[i] = optModVert;
			}

			_meshColor = meshColor;
			_isVisible = isVisible;
		}

		public void Bake_MeshTransform(apOptTransform rootTransform, apOptTransform targetTransform,
										apOptMesh targetMesh, apMatrix transformMatrix, Color meshColor, bool isVisible)
		{
			//_targetType = apModifiedMesh.TARGET_TYPE.MeshTransformOnly;

			_rootTransform = rootTransform;
			_targetTransform = targetTransform;
			_targetMesh = targetMesh;

			_transformMatrix = new apMatrix(transformMatrix);
			_meshColor = meshColor;
			_isVisible = isVisible;
		}

		public void Bake_MeshGroupTransform(apOptTransform rootTransform, apOptTransform targetTransform,
												apMatrix transformMatrix, Color meshColor, bool isVisible)
		{
			//_targetType = apModifiedMesh.TARGET_TYPE.MeshGroupTransformOnly;

			_rootTransform = rootTransform;
			_targetTransform = targetTransform;

			_transformMatrix = new apMatrix(transformMatrix);

			_meshColor = meshColor;
			_isVisible = isVisible;
		}


		public void Bake_VertexRigs(apOptTransform rootTransform, apOptTransform targetTransform,
										apOptMesh targetMesh, List<apModifiedVertexRig> modVertRigs)
		{
			//_targetType = apModifiedMesh.TARGET_TYPE.VertexWithMeshTransform;

			_rootTransform = rootTransform;
			_targetTransform = targetTransform;
			_targetMesh = targetMesh;

			if (_targetMesh == null)
			{
				Debug.LogError("Vert Rig인데 Target Mesh가 Null");
				Debug.LogError("Target Transform [" + _targetTransform.transform.name + "]");
			}

			_nVertRigs = modVertRigs.Count;
			_vertRigs = new apOptModifiedVertexRig[_nVertRigs];
			for (int i = 0; i < _nVertRigs; i++)
			{
				apOptModifiedVertexRig optModVertRig = new apOptModifiedVertexRig();
				apModifiedVertexRig srcModVertRig = modVertRigs[i];
				optModVertRig.Bake(srcModVertRig, _targetMesh, _portrait);

				_vertRigs[i] = optModVertRig;
			}

			_meshColor = Color.gray;
			_isVisible = true;
		}


		public void Bake_VertexWeights(apOptTransform rootTransform, apOptTransform targetTransform,
										apOptMesh targetMesh, List<apModifiedVertexWeight> modVertWeights)
		{
			_rootTransform = rootTransform;
			_targetTransform = targetTransform;
			_targetMesh = targetMesh;

			if (_targetMesh == null)
			{
				Debug.LogError("Vert Rig인데 Target Mesh가 Null");
				Debug.LogError("Target Transform [" + _targetTransform.transform.name + "]");
			}

			_nVertWeights = modVertWeights.Count;
			_vertWeights = new apOptModifiedVertexWeight[_nVertWeights];
			for (int i = 0; i < _nVertWeights; i++)
			{
				apOptModifiedVertexWeight optModVertWeight = new apOptModifiedVertexWeight();
				apModifiedVertexWeight srcModVertWeight = modVertWeights[i];
				optModVertWeight.Bake(srcModVertWeight);

				_vertWeights[i] = optModVertWeight;
			}

			_meshColor = Color.gray;
			_isVisible = true;

		}

		// Functions
		//--------------------------------------------



		// Get / Set
		//--------------------------------------------
	}

}