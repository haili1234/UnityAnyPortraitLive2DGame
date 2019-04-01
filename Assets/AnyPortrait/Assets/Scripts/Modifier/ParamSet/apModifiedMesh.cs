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
	/// Modifier에 의해서 변동된 내역이 저장되는 클래스
	/// ParamSet이 키값이라면, 해당 키값에 해당하는 데이터이다.
	/// 키값 하나에 여러 대상에 대한 변동 내역이 저장되므로, 각각의 대상에 대해 작성된다.
	/// 이 클래스의 값을 이용하여 중간 보간을 한 뒤 Calculated 계열 인스턴스에게 값을 전달한다.
	/// </summary>

	[Serializable]
	public class apModifiedMesh
	{
		// Members
		//------------------------------------------
		//3개의 키 값으로 찾는다.
		//하나라도 안맞는다면 유효하지 않은 modify다
		//대상 타입을 정하자
		//[수정] 타겟 종류가 단일 종류가 아니라면.... 대신 Flag 말고 아예 값 자체를 케바케로 두자
		///// <summary>
		///// 저장되는 변경 값의 종류 (연동 대상에 영향을 준다.)
		///// </summary>
		//public enum TARGET_TYPE
		//{
		//	VertexWithMeshTransform,
		//	MeshTransformOnly,
		//	MeshGroupTransformOnly,
		//	Bone,
		//	MeshTransformOrMeshGroupTransform,
		//}
		//public TARGET_TYPE _targetType = TARGET_TYPE.MeshTransformOnly;

		// 이 키값으로 대상을 특정한다.
		//Parent MeshGroup

		public int _meshGroupUniqueID_Modifier = -1;



		// 적용되는 Child 객체
		public int _transformUniqueID = -1;
		public int _meshUniqueID = -1;
		//public int _boneUniqueID = -1;//<<이건 bone transform에 적용되는 것 (skinning에는 적용되지 않는다)

		public bool _isMeshTransform = true;



		/// <summary>ModMesh가 속한 Modifier의 MeshGroup</summary>
		[NonSerialized]
		public apMeshGroup _meshGroupOfModifier = null;

		/// <summary>Transform이 속한 MeshGroup (기본적으론 _meshGroup_ParentOfModifier와 동일하지만 하위 Transform인 경우 다르게 된다)</summary>
		[NonSerialized]
		public apMeshGroup _meshGroupOfTransform = null;

		[NonSerialized]
		public apRenderUnit _renderUnit = null;

		[NonSerialized]
		public apTransform_Mesh _transform_Mesh = null;

		[NonSerialized]
		public apTransform_MeshGroup _transform_MeshGroup = null;


		//추가
		//물리 파라미터
		[SerializeField]
		public bool _isUsePhysicParam = false;

		[SerializeField]
		private apPhysicsMeshParam _physicMeshParam = new apPhysicsMeshParam();

		public apPhysicsMeshParam PhysicParam
		{
			get
			{
				if (_isUsePhysicParam)
				{
					if (_physicMeshParam == null)
					{ _physicMeshParam = new apPhysicsMeshParam(); }
					return _physicMeshParam;
				}
				return null;
			}
		}



		// 저장되는 값
		[Flags]
		public enum MOD_VALUE_TYPE
		{
			Unknown = 1,
			VertexPosList = 2,//Morph
			TransformMatrix = 4,
			Color = 8,
			BoneVertexWeightList = 16,//Bone Rigging Weight인 경우
			VertexWeightList_Physics = 32,// Physic / Volume인경우
			VertexWeightList_Volume = 64,// Physic / Volume인경우
			FFD = 128,//FFD 타입인 경우 ( 처리후에는 Vertex Pos 리스트가 된다.)
		}


		public MOD_VALUE_TYPE _modValueType = MOD_VALUE_TYPE.Unknown;

		//TODO : Bone > Modified Bone으로 대체한다.



		//추가
		//만약 이 ModMesh가 속한 Modifier의 MeshGroup에 속한 Transform이 아닌 
		//Child.. 또는 그 Child MeshGroup/Mesh Transform인 경우
		public bool _isRecursiveChildTransform = false;
		/// <summary>
		/// Modifier가 속한 MeshGroup이 아닌 다른 MeshGroup의 Transform인 경우(_isRecursiveChildTransform == true),
		/// 그때의 "원래 속한 Parent MeshGroup"의 ID
		/// </summary>
		public int _meshGroupUniqueID_Transform = -1;




		// Vertex Morph인 경우
		[SerializeField]
		public List<apModifiedVertex> _vertices = new List<apModifiedVertex>();

		// Mesh Transform / MeshGroup Transform 인 경우
		[SerializeField]
		public apMatrix _transformMatrix = new apMatrix();

		[SerializeField]
		public Color _meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		/// <summary>
		/// Mesh의 Visible 여부. False면 _meshColor의 알파를 강제로 0으로 둔다.
		/// True일땐 meshColor의 a를 그대로 사용한다.
		/// </summary>
		[SerializeField]
		public bool _isVisible = true;


		//BoneVertexWeightList 인 경우 (Rigging)
		[SerializeField]
		public List<apModifiedVertexRig> _vertRigs = new List<apModifiedVertexRig>();

		//Physics / FFD / Vertex Weight (FFD는 FFD Point와 Vertex Weight를 모두 가진다.)
		[SerializeField]
		public List<apModifiedVertexWeight> _vertWeights = new List<apModifiedVertexWeight>();


		//Editor 제어를 위한 apLinkedMatrix
		//[NonSerialized]
		//private apLinkedMatrix _linkedMatrix = null;
		//public apLinkedMatrix LinkedMatrix
		//{
		//	get
		//	{
		//		if(_linkedMatrix == null)
		//		{
		//			_linkedMatrix = new apLinkedMatrix(this, "ModMesh", apLinkedMatrix.LINK_STEP.CalculateData);
		//		}
		//		return _linkedMatrix;
		//	}
		//}

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
		//------------------------------------------
		public apModifiedMesh()
		{

		}

		// Init - 값 넣기
		//--------------------------------------------------------
		public void Init(int meshGroupID_Modifier, int meshGroupID_Transform, MOD_VALUE_TYPE modValueType)
		{
			_meshGroupUniqueID_Modifier = meshGroupID_Modifier;
			_transformUniqueID = -1;
			_meshUniqueID = -1;
			//_boneUniqueID = -1;

			_modValueType = modValueType;

			_meshGroupUniqueID_Transform = meshGroupID_Transform;

			_isRecursiveChildTransform = (meshGroupID_Modifier != meshGroupID_Transform);

			_isUsePhysicParam = (int)(_modValueType & MOD_VALUE_TYPE.VertexWeightList_Physics) != 0;
			_meshColor = Color.gray;
			_isVisible = true;
		}

		public void SetTarget_MeshTransform(int meshTransformID, int meshID, Color meshColor_Default, bool isVisible_Default)
		{
			_transformUniqueID = meshTransformID;
			_meshUniqueID = meshID;
			_isMeshTransform = true;

			_meshColor = meshColor_Default;
			_isVisible = isVisible_Default;
		}

		public void SetTarget_MeshGroupTransform(int meshGroupTransformID, Color meshColor_Default, bool isVisible_Default)
		{
			_transformUniqueID = meshGroupTransformID;
			_isMeshTransform = false;

			_meshColor = meshColor_Default;
			_isVisible = isVisible_Default;
		}

		//public void SetTarget_Bone(int boneID)
		//{
		//	_boneUniqueID = boneID;
		//}

		#region [미사용 코드] 타입에 따른 초기화는 유연성이 떨어져서 패스.
		//public void Init_VertexMorph(int meshGroupID, int meshTransformID, int meshID)
		//{
		//	_targetType = TARGET_TYPE.VertexWithMeshTransform;
		//	_meshGroupUniqueID = meshGroupID;
		//	_transformUniqueID = meshTransformID;
		//	_meshUniqueID = meshID;
		//	_boneUniqueID = -1;


		//}

		//public void Init_MeshTransform(int meshGroupID, int meshTransformID, int meshID)
		//{
		//	_targetType = TARGET_TYPE.MeshTransformOnly;
		//	_meshGroupUniqueID = meshGroupID;
		//	_transformUniqueID = meshTransformID;
		//	_meshUniqueID = meshID;
		//	_boneUniqueID = -1;
		//}

		//public void Init_MeshGroupTransform(int meshGroupID, int meshGroupTransformID)
		//{
		//	_targetType = TARGET_TYPE.MeshGroupTransformOnly;
		//	_meshGroupUniqueID = meshGroupID;
		//	_transformUniqueID = meshGroupTransformID;
		//	_meshUniqueID = -1;
		//	_boneUniqueID = -1;
		//}

		//public void Init_BoneTransform(int meshGroupID, int boneID)
		//{
		//	_targetType = TARGET_TYPE.Bone;
		//	_meshGroupUniqueID = meshGroupID;
		//	_transformUniqueID = -1;
		//	_meshUniqueID = -1;
		//	_boneUniqueID = boneID;
		//} 
		#endregion


		// Init - ID에 맞게 세팅
		//--------------------------------------------------------
		//이건 날립니더
		//public void Link_VertexMorph(apMeshGroup meshGroup, apTransform_Mesh meshTransform, apRenderUnit renderUnit)
		//{
		//	_meshGroup = meshGroup;
		//	_transform_Mesh = meshTransform;
		//	_renderUnit = renderUnit;

		//	//RefreshVertices();

		//}

		/// <summary>
		/// MeshTransform과 ModMesh를 연결한다.
		/// </summary>
		/// <param name="meshGroupOfMod">Modifier가 속한 MeshGroup</param>
		///<param name="meshGroupOfTransform">Transform이 속한 MeshGroup</param>
		public void Link_MeshTransform(apMeshGroup meshGroupOfMod, apMeshGroup meshGroupOfTransform, apTransform_Mesh meshTransform, apRenderUnit renderUnit, apPortrait portrait)
		{

			_meshGroupOfModifier = meshGroupOfMod;
			_meshGroupOfTransform = meshGroupOfTransform;

			_transform_Mesh = meshTransform;
			_renderUnit = renderUnit;

			if (_isUsePhysicParam)
			{
				if (_physicMeshParam == null)
				{
					_physicMeshParam = new apPhysicsMeshParam();
				}
				_physicMeshParam.Link(portrait);
			}

			//Debug.Log("ModMesh Link RenderUnit");
			RefreshModifiedValues(meshGroupOfMod._parentPortrait);
		}

		/// <summary>
		/// MeshGroupTransform과 ModMesh를 연결한다.
		/// </summary>
		/// <param name="meshGroupOfMod">Modifier가 속한 MeshGroup</param>
		/// <param name="meshGroupOfTransform">Transform이 속한 MeshGroup</param>
		public void Link_MeshGroupTransform(apMeshGroup meshGroupOfMod, apMeshGroup meshGroupOfTransform, apTransform_MeshGroup meshGroupTransform, apRenderUnit renderUnit)
		{
			_meshGroupOfModifier = meshGroupOfMod;
			_meshGroupOfTransform = meshGroupOfTransform;

			_transform_MeshGroup = meshGroupTransform;
			_renderUnit = renderUnit;


			RefreshModifiedValues(meshGroupOfMod._parentPortrait);
		}



		public void Link_Bone()
		{
			//?
		}



		// 데이터 리셋
		//------------------------------------------------------------------
		/// <summary>
		/// 저장되는 ModifiedValue의 데이터들을 처리 준비하게 해준다.
		/// 값 초기화는 Reset에서 한다. 주의주의
		/// </summary>
		public void RefreshModifiedValues(apPortrait portrait)
		{
			if ((int)(_modValueType & MOD_VALUE_TYPE.VertexPosList) != 0)
			{
				RefreshVertices();
			}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.TransformMatrix) != 0)
			{
				_transformMatrix.MakeMatrix();
			}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.Color) != 0)
			{
				//_meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);//2X 칼라 초기화
			}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.BoneVertexWeightList) != 0)
			{
				//Debug.LogError("TODO : ModMesh BoneVertexWeightList 타입 정의 필요");
				//Debug.Log("ModMesh Link RenderUnit >> Bone Vertex Weight List");
				RefreshVertexRigs(portrait);
			}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.VertexWeightList_Physics) != 0)
			{
				//물리 타입의 Weight
				RefreshVertexWeights(portrait, true, false);
			}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.VertexWeightList_Volume) != 0)
			{
				//물리 값을 사용하지 않는 Weight
				RefreshVertexWeights(portrait, false, true);
			}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.FFD) != 0)
			{
				Debug.LogError("TODO : ModMesh FFD 타입 정의 필요");
			}
			else
			{
				Debug.LogError("TODO : 알 수 없는 ModMesh Value Type : " + _modValueType);
			}
		}

		// 버텍스 Refresh
		//---------------------------------------------------
		public void RefreshVertices()
		{
			if (_transform_Mesh._mesh != null)
			{
				bool isSameVerts = true;
				if (_vertices.Count == 0 || _vertices.Count != _transform_Mesh._mesh._vertexData.Count)
				{
					isSameVerts = false;
				}
				else
				{
					//전부 비교해볼까나..
					//빠르게 단순 링크를 시도해보고, 한번이라도 실패하면 다시 리스트를 만들어야한다.
					List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
					apVertex meshVert = null;
					apModifiedVertex modVert = null;
					for (int i = 0; i < meshVertList.Count; i++)
					{
						meshVert = meshVertList[i];
						modVert = _vertices[i];

						if (modVert._vertexUniqueID != meshVert._uniqueID)
						{
							//버텍스 리스트 갱신이 필요하다
							isSameVerts = false;
							break;
						}

						modVert.Link(this, _transform_Mesh._mesh, meshVert);
					}
				}

				if (!isSameVerts)
				{
					//유효한 Vertex만 찾아서 넣어준다.
					//유효하면 - Link
					//유효하지 않다면 - Pass (Link 안된거 삭제)
					//없는건 - Add
					//순서는.. Index를 넣어서



					//1. 일단 기존 데이터 복사 - 없어진 Vertex를 빼자
					if (_vertices.Count != 0)
					{
						apModifiedVertex modVert = null;
						for (int i = 0; i < _vertices.Count; i++)
						{
							modVert = _vertices[i];
							apVertex existVert = _transform_Mesh._mesh._vertexData.Find(delegate (apVertex a)
							{
								return a._uniqueID == modVert._vertexUniqueID;
							});

							if (existVert != null)
							{
								//유효하다면 Link
								modVert.Link(this, _transform_Mesh._mesh, existVert);
							}
							else
							{
								//유효하지 않다면.. Unlink -> 나중에 삭제됨
								modVert._vertex = null;
							}
						}

						//이제 존재하지 않는 Vertex에 대해서는 삭제
						_vertices.RemoveAll(delegate (apModifiedVertex a)
						{
							return a._vertex == null;
						});

						List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
						apVertex meshVert = null;

						for (int i = 0; i < meshVertList.Count; i++)
						{
							meshVert = meshVertList[i];
							//해당 Vertex가 있었는가
							bool isLinked = _vertices.Exists(delegate (apModifiedVertex a)
							{
								return a._vertex == meshVert;
							});

							//없으면 추가
							if (!isLinked)
							{
								apModifiedVertex newVert = new apModifiedVertex();
								newVert.Init(meshVert._uniqueID, meshVert);
								newVert.Link(this, _transform_Mesh._mesh, meshVert);

								_vertices.Add(newVert);//<<새로 추가할 리스트에 넣어준다.
							}
						}

						//Vertex Index에 맞게 정렬
						_vertices.Sort(delegate (apModifiedVertex a, apModifiedVertex b)
						{
							return a._vertIndex - b._vertIndex;
						});
					}
					else
					{
						//2. 아예 리스트가 없을 때
						_vertices.Clear();

						List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
						apVertex meshVert = null;

						for (int i = 0; i < meshVertList.Count; i++)
						{
							meshVert = meshVertList[i];

							apModifiedVertex newVert = new apModifiedVertex();
							newVert.Init(meshVert._uniqueID, meshVert);
							newVert.Link(this, _transform_Mesh._mesh, meshVert);

							_vertices.Add(newVert);//<<새로 추가할 리스트에 넣어준다.
						}
					}

				}
			}
		}


		public void RefreshVertexRigs(apPortrait portrait)
		{
			if (_transform_Mesh._mesh != null)
			{
				bool isSameVerts = true;
				if (_vertRigs.Count == 0 || _vertRigs.Count != _transform_Mesh._mesh._vertexData.Count)
				{
					isSameVerts = false;
				}
				else
				{
					//전부 비교해볼까나..
					//빠르게 단순 링크를 시도해보고, 한번이라도 실패하면 다시 리스트를 만들어야한다.
					List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
					apVertex meshVert = null;
					apModifiedVertexRig modVertRig = null;
					for (int i = 0; i < meshVertList.Count; i++)
					{
						meshVert = meshVertList[i];
						modVertRig = _vertRigs[i];

						if (modVertRig._vertexUniqueID != meshVert._uniqueID)
						{
							//버텍스 리스트 갱신이 필요하다
							isSameVerts = false;
							break;
						}

						modVertRig.Link(this, _transform_Mesh._mesh, meshVert);
						modVertRig.LinkWeightPair(portrait);
					}
				}

				if (!isSameVerts)
				{
					//유효한 Vertex만 찾아서 넣어준다.
					//유효하면 - Link
					//유효하지 않다면 - Pass (Link 안된거 삭제)
					//없는건 - Add
					//순서는.. Index를 넣어서



					//1. 일단 기존 데이터 복사 - 없어진 Vertex를 빼자
					if (_vertRigs.Count != 0)
					{
						apModifiedVertexRig modVertRig = null;
						for (int i = 0; i < _vertRigs.Count; i++)
						{
							modVertRig = _vertRigs[i];
							apVertex existVert = _transform_Mesh._mesh._vertexData.Find(delegate (apVertex a)
							{
								return a._uniqueID == modVertRig._vertexUniqueID;
							});

							if (existVert != null)
							{
								//유효하다면 Link
								modVertRig.Link(this, _transform_Mesh._mesh, existVert);
								modVertRig.LinkWeightPair(portrait);
							}
							else
							{
								//유효하지 않다면.. Unlink -> 나중에 삭제됨
								modVertRig._vertex = null;
							}
						}

						//이제 존재하지 않는 Vertex에 대해서는 삭제
						_vertRigs.RemoveAll(delegate (apModifiedVertexRig a)
						{
							return a._vertex == null;
						});

						List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
						apVertex meshVert = null;

						for (int i = 0; i < meshVertList.Count; i++)
						{
							meshVert = meshVertList[i];
							//해당 Vertex가 있었는가
							bool isLinked = _vertRigs.Exists(delegate (apModifiedVertexRig a)
							{
								return a._vertex == meshVert;
							});

							//없으면 추가
							if (!isLinked)
							{
								apModifiedVertexRig newVertRig = new apModifiedVertexRig();
								newVertRig.Init(meshVert._uniqueID, meshVert);
								newVertRig.Link(this, _transform_Mesh._mesh, meshVert);
								newVertRig.LinkWeightPair(portrait);

								_vertRigs.Add(newVertRig);//<<새로 추가할 리스트에 넣어준다.
							}
						}

						//Vertex Index에 맞게 정렬
						_vertRigs.Sort(delegate (apModifiedVertexRig a, apModifiedVertexRig b)
						{
							return a._vertIndex - b._vertIndex;
						});
					}
					else
					{
						//2. 아예 리스트가 없을 때
						_vertRigs.Clear();

						List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
						apVertex meshVert = null;

						for (int i = 0; i < meshVertList.Count; i++)
						{
							meshVert = meshVertList[i];

							apModifiedVertexRig newVertRig = new apModifiedVertexRig();
							newVertRig.Init(meshVert._uniqueID, meshVert);
							newVertRig.Link(this, _transform_Mesh._mesh, meshVert);
							newVertRig.LinkWeightPair(portrait);

							_vertRigs.Add(newVertRig);//<<새로 추가할 리스트에 넣어준다.
						}
					}

				}

				for (int i = 0; i < _vertRigs.Count; i++)
				{
					_vertRigs[i].RefreshModMeshAndRenderVertex(this);
				}
			}
		}


		public void RefreshVertexWeights(apPortrait portrait, bool isPhysics, bool isVolume)
		{

			if (_renderUnit != null)
			{
				//"Modifier가 연산되기 전"의 WorldPosition을 미리 계산하자
				_renderUnit.CalculateWorldPositionWithoutModifier();
			}
			//Debug.Log("<<< RefreshVertexWeights >>>");
			if (_transform_Mesh._mesh != null)
			{
				bool isSameVerts = true;
				if (_vertWeights.Count == 0 || _vertWeights.Count != _transform_Mesh._mesh._vertexData.Count)
				{
					isSameVerts = false;
				}
				else
				{
					//전부 비교해볼까나..
					//빠르게 단순 링크를 시도해보고, 한번이라도 실패하면 다시 리스트를 만들어야한다.
					List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
					apVertex meshVert = null;
					apModifiedVertexWeight modVertWeight = null;
					for (int i = 0; i < meshVertList.Count; i++)
					{
						meshVert = meshVertList[i];
						modVertWeight = _vertWeights[i];

						if (modVertWeight._vertexUniqueID != meshVert._uniqueID)
						{
							//버텍스 리스트 갱신이 필요하다
							isSameVerts = false;
							break;
						}
						modVertWeight.Link(this, _transform_Mesh._mesh, meshVert);
					}
				}

				if (!isSameVerts)
				{
					//유효한 Vertex만 찾아서 넣어준다.
					//유효하면 - Link
					//유효하지 않다면 - Pass (Link 안된거 삭제)
					//없는건 - Add
					//순서는.. Index를 넣어서



					//1. 일단 기존 데이터 복사 - 없어진 Vertex를 빼자
					if (_vertWeights.Count != 0)
					{
						apModifiedVertexWeight modVertWeight = null;
						for (int i = 0; i < _vertWeights.Count; i++)
						{
							modVertWeight = _vertWeights[i];
							apVertex existVert = _transform_Mesh._mesh._vertexData.Find(delegate (apVertex a)
							{
								return a._uniqueID == modVertWeight._vertexUniqueID;
							});

							if (existVert != null)
							{
								//유효하다면 Link
								modVertWeight.Link(this, _transform_Mesh._mesh, existVert);
							}
							else
							{
								//유효하지 않다면.. Unlink -> 나중에 삭제됨
								modVertWeight._vertex = null;
							}
						}

						//이제 존재하지 않는 Vertex에 대해서는 삭제
						_vertWeights.RemoveAll(delegate (apModifiedVertexWeight a)
						{
							return a._vertex == null;
						});

						List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
						apVertex meshVert = null;

						for (int i = 0; i < meshVertList.Count; i++)
						{
							meshVert = meshVertList[i];
							//해당 Vertex가 있었는가
							bool isLinked = _vertWeights.Exists(delegate (apModifiedVertexWeight a)
							{
								return a._vertex == meshVert;
							});

							//없으면 추가
							if (!isLinked)
							{
								apModifiedVertexWeight newVertWeight = new apModifiedVertexWeight();
								newVertWeight.Init(meshVert._uniqueID, meshVert);
								newVertWeight.SetDataType(isPhysics, isVolume);//<<어떤 타입인지 넣는다.
																			   //TODO:Modifier에 따라 특성 추가
								newVertWeight.Link(this, _transform_Mesh._mesh, meshVert);

								_vertWeights.Add(newVertWeight);//<<새로 추가할 리스트에 넣어준다.
							}
						}

						//Vertex Index에 맞게 정렬
						_vertWeights.Sort(delegate (apModifiedVertexWeight a, apModifiedVertexWeight b)
						{
							return a._vertIndex - b._vertIndex;
						});
					}
					else
					{
						//2. 아예 리스트가 없을 때
						_vertWeights.Clear();

						List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
						apVertex meshVert = null;

						for (int i = 0; i < meshVertList.Count; i++)
						{
							meshVert = meshVertList[i];

							apModifiedVertexWeight newVertWeight = new apModifiedVertexWeight();
							newVertWeight.Init(meshVert._uniqueID, meshVert);
							newVertWeight.SetDataType(isPhysics, isVolume);//<<어떤 타입인지 넣는다.
																		   //TODO:Modifier에 따라 특성 추가
							newVertWeight.Link(this, _transform_Mesh._mesh, meshVert);

							_vertWeights.Add(newVertWeight);//<<새로 추가할 리스트에 넣어준다.
						}
					}

				}

				for (int i = 0; i < _vertWeights.Count; i++)
				{
					_vertWeights[i].RefreshModMeshAndWeights(this);
				}
			}
			//물리 관련 Refresh를 한번 더 한다.
			if (isPhysics)
			{
				RefreshVertexWeight_Physics(true);
			}
		}

		/// <summary>
		/// ModVertexWeight를 사용하는 Modifier 중 Physics인 경우,
		/// Vertex의 Weight가 바뀌었다면 한번씩 이 함수를 호출해주자.
		/// Constraint(자동), isEnabled(자동), Main(수동) 등을 다시 세팅한다.
		/// </summary>
		private void RefreshVertexWeight_Physics(bool isForceRefresh)
		{
			if (_transform_Mesh == null || _transform_Mesh._mesh == null || _vertWeights.Count == 0)
			{
				return;
			}
			bool isAnyChanged = false;
			apModifiedVertexWeight vertWeight = null;
			float bias = 0.001f;
			for (int iVW = 0; iVW < _vertWeights.Count; iVW++)
			{
				vertWeight = _vertWeights[iVW];
				bool isNextEnabled = false;
				if (vertWeight._weight < bias)
				{
					isNextEnabled = false;
				}
				else
				{
					isNextEnabled = true;
				}
				//Weight 활성화 여부가 바뀌었는지 체크
				if (isNextEnabled != vertWeight._isEnabled)
				{
					vertWeight._isEnabled = isNextEnabled;
					isAnyChanged = true;
				}
			}
			if (!isAnyChanged && !isForceRefresh)
			{
				return;
			}

			for (int iVW = 0; iVW < _vertWeights.Count; iVW++)
			{
				vertWeight = _vertWeights[iVW];
				vertWeight.RefreshModMeshAndWeights(this);
			}
			for (int iVW = 0; iVW < _vertWeights.Count; iVW++)
			{
				vertWeight = _vertWeights[iVW];
				vertWeight.RefreshLinkedVertex();
			}
		}

		// Functions
		//------------------------------------------
		public void ResetValues()
		{
			for (int i = 0; i < _vertices.Count; i++)
			{
				_vertices[i]._deltaPos = Vector2.zero;
			}
			_transformMatrix.SetIdentity();
			_meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			_isVisible = true;
		}


		public void UpdateBeforeBake(apPortrait portrait, apMeshGroup mainMeshGroup, apTransform_MeshGroup mainMeshGroupTransform)
		{
			//Bake 전에 업데이트할게 있으면 여기서 업데이트하자

			//1. VertRig의 LocalPos 갱신을 여기서 하자
			#region [미사용 코드]
			//if(_vertRigs != null && _vertRigs.Count > 0)
			//{
			//	apModifiedVertexRig vertRig = null;
			//	//기존 링크 말고, Bake 직전의 Transform 등을 검색하여 값을 넣어주자
			//	apTransform_Mesh meshTransform = mainMeshGroup.GetMeshTransformRecursive(_transformUniqueID);
			//	if (meshTransform != null)
			//	{
			//		apMesh mesh = meshTransform._mesh;
			//		if (mesh != null)
			//		{
			//			for (int iVR = 0; iVR < _vertRigs.Count; iVR++)
			//			{
			//				vertRig = _vertRigs[iVR];
			//				apVertex vert = vertRig._vertex;

			//				for (int iW = 0; iW < vertRig._weightPairs.Count; iW++)
			//				{
			//					apModifiedVertexRig.WeightPair weightPair = vertRig._weightPairs[iW];
			//					weightPair.CalculateLocalPos(vert._pos, mesh.Matrix_VertToLocal, meshTransform._matrix_TFResult_WorldWithoutMod, weightPair._bone._defaultMatrix);
			//				}

			//			}
			//		}
			//	}
			//} 
			#endregion
		}

		// Get / Set
		//------------------------------------------
		public apModifiedVertexWeight GetVertexWeight(apVertex vertex)
		{
			if (vertex == null)
			{
				return null;
			}
			return _vertWeights.Find(delegate (apModifiedVertexWeight a)
			{
				return a._vertexUniqueID == vertex._uniqueID;
			});
		}

		// 비교 관련
		//------------------------------------------
		public bool IsContains_MeshTransform(apMeshGroup meshGroup, apTransform_Mesh meshTransform, apMesh mesh)
		{
			if (_meshGroupUniqueID_Modifier == meshGroup._uniqueID &&
				_transformUniqueID == meshTransform._transformUniqueID &&
				_transformUniqueID >= 0 &&
				_meshUniqueID == mesh._uniqueID &&
				_isMeshTransform
				)
			{
				return true;
			}
			return false;
		}

		public bool IsContains_MeshGroupTransform(apMeshGroup meshGroup, apTransform_MeshGroup meshGroupTransform)
		{
			if (_meshGroupUniqueID_Modifier == meshGroup._uniqueID &&
				_transformUniqueID == meshGroupTransform._transformUniqueID &&
				_transformUniqueID >= 0 &&
				!_isMeshTransform)
			{
				return true;
			}
			return false;
		}

		//public bool IsContains_Bone(apMeshGroup meshGroup, )
		//TODO : Bone
	}
}