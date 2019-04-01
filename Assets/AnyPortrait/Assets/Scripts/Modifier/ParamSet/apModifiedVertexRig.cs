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
	/// apModifiedVertex와 유사하지만 Rigging 용으로만 따로 정의되었다.
	/// Bone 좌표계에 대한 Local Position과 Weight 쌍으로 구성되어있다.
	/// 최대 8개의 Bone에 연동될 수 있다. (리스트 사용 안함)
	/// 이 값은 RenderVertex에 직접 적용이 된다.
	/// </summary>
	[Serializable]
	public class apModifiedVertexRig
	{
		// Members
		//-----------------------------------------------
		[NonSerialized]
		public apModifiedMesh _modifiedMesh = null;

		public int _vertexUniqueID = -1;
		public int _vertIndex = -1;

		[NonSerialized]
		public apMesh _mesh = null;

		[NonSerialized]
		public apVertex _vertex = null;

		[NonSerialized]
		public apRenderVertex _renderVertex = null;//RenderUnit과 연동된 경우 RenderVert도 넣어주자



		[Serializable]
		public class WeightPair
		{
			[NonSerialized]
			public apBone _bone = null;

			[SerializeField]
			public int _boneID = -1;

			[NonSerialized]
			public apMeshGroup _meshGroup = null;

			[SerializeField]
			public int _meshGroupID = -1;

			[SerializeField]
			public float _weight = 0.0f;

			/// <summary>
			/// 백업용 생성자
			/// </summary>
			public WeightPair()
			{

			}


			public WeightPair(apBone bone)
			{
				_bone = bone;
				_boneID = _bone._uniqueID;

				_meshGroup = _bone._meshGroup;
				_meshGroupID = _meshGroup._uniqueID;

				_weight = 0.0f;
			}
		}
		[SerializeField]
		public List<WeightPair> _weightPairs = new List<WeightPair>();

		public float _totalWeight = 0.0f;


		// Init
		//-----------------------------------------------
		public apModifiedVertexRig()
		{

		}


		public void Init(int vertUniqueID, apVertex vertex)
		{
			_vertexUniqueID = vertUniqueID;
			_vertex = vertex;
			_vertIndex = _vertex._index;

			ResetWeightTable();
		}


		public void Link(apModifiedMesh modifiedMesh, apMesh mesh, apVertex vertex)
		{
			_modifiedMesh = modifiedMesh;
			_mesh = mesh;
			_vertex = vertex;
			if (_vertex != null)
			{
				_vertIndex = _vertex._index;
			}
			else
			{
				_vertIndex = -1;
			}

			_renderVertex = null;
			if (modifiedMesh._renderUnit != null && _vertex != null)
			{
				_renderVertex = modifiedMesh._renderUnit._renderVerts.Find(delegate (apRenderVertex a)
				{
					return a._vertex == _vertex;
				});
			}
		}

		public void RefreshModMeshAndRenderVertex(apModifiedMesh modifiedMesh)
		{
			if (_modifiedMesh != modifiedMesh || _renderVertex == null)
			{
				_modifiedMesh = modifiedMesh;
				if (_modifiedMesh != null && modifiedMesh._renderUnit != null && _vertex != null)
				{
					_renderVertex = modifiedMesh._renderUnit._renderVerts.Find(delegate (apRenderVertex a)
				{
					return a._vertex == _vertex;
				});
				}
			}
		}

		/// <summary>
		/// WeightTable의 값과 연동을 하고 Sort를 한다.
		/// </summary>
		/// <param name="portrait"></param>
		public void LinkWeightPair(apPortrait portrait)
		{
			_totalWeight = 0.0f;
			WeightPair weightPair = null;
			bool isAnyRemove = false;
			for (int i = 0; i < _weightPairs.Count; i++)
			{
				weightPair = _weightPairs[i];
				if (weightPair._meshGroupID >= 0)
				{
					weightPair._meshGroup = portrait.GetMeshGroup(weightPair._meshGroupID);
					if (weightPair._meshGroup != null)
					{
						weightPair._bone = weightPair._meshGroup.GetBone(weightPair._boneID);
						if (weightPair._bone == null)
						{
							isAnyRemove = true;
						}
						else
						{
							_totalWeight += weightPair._weight;
						}
					}
					else
					{
						weightPair._bone = null;
						isAnyRemove = true;
					}
				}
				else
				{
					weightPair._meshGroup = null;
					weightPair._bone = null;
					isAnyRemove = true;
				}

			}
			if (isAnyRemove)
			{
				//뭔가 삭제할게 생겼다. 삭제하자
				_weightPairs.RemoveAll(delegate (WeightPair a)
				{
					return a._meshGroup == null || a._bone == null;
				});
			}
		}




		// Functions
		//-----------------------------------------------
		/// <summary>
		/// Weight 정보를 모두 초기화한다.
		/// </summary>
		public void ResetWeightTable()
		{
			_weightPairs.Clear();
			_totalWeight = 0.0f;
		}


		public void CalculateTotalWeight()
		{
			_totalWeight = 0.0f;
			for (int i = 0; i < _weightPairs.Count; i++)
			{
				_totalWeight += _weightPairs[i]._weight;
			}
		}

		public void Normalize()
		{
			_totalWeight = 0.0f;
			for (int i = 0; i < _weightPairs.Count; i++)
			{
				_totalWeight += _weightPairs[i]._weight;
			}

			if (_totalWeight > 0.0f && _weightPairs.Count > 0)
			{
				for (int i = 0; i < _weightPairs.Count; i++)
				{
					_weightPairs[i]._weight /= _totalWeight;
				}

				_totalWeight = 1.0f;
			}
		}

		/// <summary>
		/// Normalize와 유사하지만, 해당 Pair를 일단 제쳐두고,
		/// "나머지 Weight"에 한해서 우선 Normalize
		/// 그리고 해당 Pair를 포함시킨다.
		/// 요청한 Pair의 Weight가 1이 넘으면 1로 맞추고 나머지는 0
		/// </summary>
		/// <param name="pair"></param>
		public void NormalizeExceptPair(WeightPair pair)
		{
			if (!_weightPairs.Contains(pair))
			{
				Normalize();
				return;
			}

			float reqWeight = Mathf.Clamp01(pair._weight);
			float remainedWeight = 1.0f - reqWeight;

			float totalWeightExceptReq = 0.0f;
			for (int i = 0; i < _weightPairs.Count; i++)
			{
				if (_weightPairs[i] == pair)
				{
					_weightPairs[i]._weight = reqWeight;
				}
				else
				{
					totalWeightExceptReq += _weightPairs[i]._weight;
				}
			}

			if (totalWeightExceptReq > 0.0f)
			{
				//totalWeightExceptReq -> remainedWeight
				float convertRatio = remainedWeight / totalWeightExceptReq;

				for (int i = 0; i < _weightPairs.Count; i++)
				{
					if (_weightPairs[i] == pair)
					{
						continue;
					}
					else
					{
						_weightPairs[i]._weight *= convertRatio;
					}
				}
			}

			//그리고 마지막으로 Normalize
			Normalize();


		}

		/// <summary>
		/// 일정값 이하의 Weight를 가지는 WeightPair를 삭제한다.
		/// Normalize를 자동으로 수행한다.
		/// </summary>
		public void Prune()
		{
			Normalize();

			_weightPairs.RemoveAll(delegate (WeightPair a)
			{
				return a._weight < 0.01f;//1%
		});

			Normalize();
		}

		// Get / Set 
		//-----------------------------------------------
	}
}