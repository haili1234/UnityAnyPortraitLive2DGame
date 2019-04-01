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
//using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// Vertex를 업데이트하는 함수가 담겨있는 객체이다.
	/// Vertex 처리에 대해서 기존의 Modifier -> CalculateStack -> Mesh -> Vertex Update 방식의 문제를 해결하기 위해
	/// 각 처리 단계에서 중요 데이터와 레이어, Weight를 for문 없이 담아두고
	/// 단 한번의 루프에서 처리할 수 있도록 한다.
	/// Calculated 연결시 미리 다 연결해둔다.
	/// </summary>
	public class apOptVertexRequest
	{
		// Members
		//-----------------------------------------------

		//Local Morph의 경우
		public class ModWeightPair
		{
			public bool _isCalculated = false;
			public apOptModifiedMesh _modMesh = null;
			public float _weight = 0.0f;

			public ModWeightPair(apOptModifiedMesh modMesh)
			{
				_isCalculated = false;
				_modMesh = modMesh;
				_weight = 0.0f;
			}

			public void InitCalculate()
			{
				_isCalculated = false;
				_weight = 0.0f;
			}

			public void SetWeight(float weight)
			{
				_isCalculated = true;
				_weight = weight;
			}
		}

		
		public List<ModWeightPair> _modWeightPairs = new List<ModWeightPair>();
		public int _nModWeightPairs = 0;


		//Rigging의 경우
		//Bone + Weight는 크게 바뀌는 경우가 없다..
		//Transform, Bone만 연결해두고 Weight를 저장한 뒤 처리한다.
		//..Rigging은 첫 연결 후 weight가 변할리는 없으니 Modifier 처리가 필요 없을지도..
		public class RigBoneWeightPair
		{
			public apOptTransform _optTransform = null;
			public apOptBone _bone = null;

			public apMatrix3x3 _boneMatrix = apMatrix3x3.identity;

			public float _weight = 0.0f;

			public RigBoneWeightPair(apOptTransform optTransform, apOptBone bone, float weight)
			{
				_optTransform = optTransform;
				_bone = bone;
				_weight = weight;
			}

			public void CalculateMatrix()
			{
				//_boneMatrix = _optTransform._vertMeshWorldNoModInverseMatrix
				//	* _bone._vertWorld2BoneModWorldMatrix
				//	* _optTransform._vertMeshWorldNoModMatrix;
				_boneMatrix.SetMatrix(_optTransform._vertMeshWorldNoModInverseMatrix);
				_boneMatrix.Multiply(_bone._vertWorld2BoneModWorldMatrix);
				_boneMatrix.Multiply(_optTransform._vertMeshWorldNoModMatrix);
			}
		}

		//버텍스별로 WeightPair를 가진다. 빠른 처리를 위해 배열로 저장
		public class VertRigWeightTable
		{
			public RigBoneWeightPair[] _rigTable = null;
			public int _nRigTable = 0;

			public VertRigWeightTable(int vertIndex, apOptModifiedMesh modMesh)
			{
				apOptModifiedVertexRig.OptWeightPair[] weightPairs = modMesh._vertRigs[vertIndex]._weightPairs;


				_rigTable = new RigBoneWeightPair[weightPairs.Length];

				float totalWeight = 0.0f;
				for (int i = 0; i < weightPairs.Length; i++)
				{
					_rigTable[i] = new RigBoneWeightPair(modMesh._targetTransform, weightPairs[i]._bone, weightPairs[i]._weight);
					totalWeight += weightPairs[i]._weight;
				}

				if (totalWeight < 1.0f && totalWeight > 0.0f)
				{
					for (int i = 0; i < _rigTable.Length; i++)
					{
						_rigTable[i]._weight /= totalWeight;
					}
				}
				_nRigTable = _rigTable.Length;
				
			}
		}

		//각 Vertex마다 Rig 정보를 넣자
		public VertRigWeightTable[] _rigBoneWeightTables = null;
		
		public float _totalWeight = 1.0f;
		public bool _isCalculated = false;

		public enum REQUEST_TYPE
		{
			VertLocal,
			Rigging
		}

		private REQUEST_TYPE _requestType = REQUEST_TYPE.VertLocal;

		// Init
		//-----------------------------------------------
		public apOptVertexRequest(REQUEST_TYPE requestType)
		{
			_requestType = requestType;
			Clear();
		}

		public void Clear()
		{
			_modWeightPairs.Clear();
			_nModWeightPairs = 0;

			_rigBoneWeightTables = null;

			_totalWeight = 1.0f;
			_isCalculated = false;
		}

		// Functions
		//-----------------------------------------------
		public void AddModMesh(apOptModifiedMesh modMesh)
		{
			if (_requestType == REQUEST_TYPE.VertLocal)
			{
				_modWeightPairs.Add(new ModWeightPair(modMesh));
				_nModWeightPairs = _modWeightPairs.Count;
			}
			else if(_requestType == REQUEST_TYPE.Rigging)
			{
				if(_rigBoneWeightTables != null)
				{
					//??
					//Rigging은 Static 타입이어서 ModMesh가 하나만 생성된다.
					Debug.LogError("Overwritten Mod Mesh To Rigging");
					return;
				}
				_rigBoneWeightTables = new VertRigWeightTable[modMesh._vertRigs.Length];

				for (int i = 0; i < modMesh._vertRigs.Length; i++)
				{
					_rigBoneWeightTables[i] = new VertRigWeightTable(i, modMesh);
				}
			}
		}

		

		public void InitCalculate()
		{
			if (_requestType == REQUEST_TYPE.VertLocal)
			{
				for (int i = 0; i < _nModWeightPairs; i++)
				{
					_modWeightPairs[i].InitCalculate();
				}
			}

			_totalWeight = 1.0f;
			_isCalculated = false;
		}

		public void SetCalculated()
		{
			_isCalculated = true;
		}


		public void MultiplyWeight(float weight)
		{
			_totalWeight *= weight;
		}

		// Get / Set
		//-----------------------------------------------
	}
}