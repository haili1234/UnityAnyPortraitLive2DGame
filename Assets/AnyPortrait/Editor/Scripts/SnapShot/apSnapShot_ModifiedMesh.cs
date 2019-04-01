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
	public class apSnapShot_ModifiedMesh : apSnapShotBase
	{
		// Members
		//--------------------------------------------
		//키값은 바뀌지 않는다.
		private apMeshGroup _key_MeshGroupOfMod = null;
		private apMeshGroup _key_MeshGroupOfTransform = null;
		private apTransform_Mesh _key_MeshTransform = null;
		private apTransform_MeshGroup _key_MeshGroupTransform = null;
		private apRenderUnit _key_RenderUnit = null;

		//저장되는 멤버 데이터
		public class VertData
		{
			public apVertex _key_Vert = null;
			public Vector2 _deltaPos = Vector2.zero;

			public VertData(apVertex key_Vert, Vector2 deltaPos)
			{
				_key_Vert = key_Vert;
				_deltaPos = deltaPos;
			}
		}
		private List<VertData> _vertices = new List<VertData>();
		private apMatrix _transformMatrix = new apMatrix();
		private Color _meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		private bool _isVisible = true;


		// Init
		//--------------------------------------------
		public apSnapShot_ModifiedMesh() : base()
		{

		}



		// Functions
		//--------------------------------------------
		public override bool IsKeySyncable(object target)
		{
			//return base.IsKeySyncable(target);
			if (!(target is apModifiedMesh))
			{
				return false;
			}

			apModifiedMesh targetModMesh = target as apModifiedMesh;
			if (targetModMesh == null)
			{
				return false;
			}

			//Key들이 모두 같아야 한다.
			if (targetModMesh._meshGroupOfModifier != _key_MeshGroupOfMod)
			{
				return false;
			}

			if (targetModMesh._meshGroupOfTransform != _key_MeshGroupOfTransform)
			{
				return false;
			}

			if (targetModMesh._transform_MeshGroup != null)
			{
				if (targetModMesh._transform_MeshGroup != _key_MeshGroupTransform)
				{
					return false;
				}
			}
			if (targetModMesh._transform_Mesh != null)
			{
				if (targetModMesh._transform_Mesh != _key_MeshTransform)
				{
					return false;
				}
			}
			if (targetModMesh._renderUnit != _key_RenderUnit)
			{
				return false;
			}

			return true;
		}

		public override bool Save(object target, string strParam)
		{
			base.Save(target, strParam);

			apModifiedMesh modMesh = target as apModifiedMesh;
			if (modMesh == null)
			{
				return false;
			}

			_key_MeshGroupOfMod = modMesh._meshGroupOfModifier;
			_key_MeshGroupOfTransform = modMesh._meshGroupOfTransform;

			_key_MeshTransform = null;
			_key_MeshGroupTransform = null;
			_key_RenderUnit = null;

			if (modMesh._transform_Mesh != null)
			{
				_key_MeshTransform = modMesh._transform_Mesh;
			}
			if (modMesh._transform_MeshGroup != null)
			{
				_key_MeshGroupTransform = modMesh._transform_MeshGroup;
			}
			_key_RenderUnit = modMesh._renderUnit;

			_vertices.Clear();
			int nVert = modMesh._vertices.Count;

			for (int i = 0; i < nVert; i++)
			{
				apModifiedVertex modVert = modMesh._vertices[i];
				_vertices.Add(new VertData(modVert._vertex, modVert._deltaPos));
			}

			_transformMatrix = new apMatrix(modMesh._transformMatrix);
			_meshColor = modMesh._meshColor;
			_isVisible = modMesh._isVisible;

			return true;
		}


		public override bool Load(object targetObj)
		{
			apModifiedMesh modMesh = targetObj as apModifiedMesh;
			if (modMesh == null)
			{
				return false;
			}

			int nVert = _vertices.Count;
			bool isDifferentVert = false;
			//만약 하나라도 Vert가 변경된게 있으면 좀 오래 걸리는 로직으로 바뀌어야 한다.
			//미리 체크해주자
			if (modMesh._vertices.Count != nVert)
			{
				isDifferentVert = true;
			}
			else
			{
				for (int i = 0; i < nVert; i++)
				{
					if (_vertices[i]._key_Vert != modMesh._vertices[i]._vertex)
					{
						isDifferentVert = true;
						break;
					}
				}
			}

			if (isDifferentVert)
			{
				//1. 만약 Vertex 구성이 다르면
				//매번 Find로 찾아서 매칭해야한다.
				VertData vertData = null;
				apModifiedVertex modVert = null;
				for (int i = 0; i < nVert; i++)
				{
					vertData = _vertices[i];
					modVert = modMesh._vertices.Find(delegate (apModifiedVertex a)
					{
						return a._vertex == vertData._key_Vert;
					});

					if (modVert != null)
					{
						modVert._deltaPos = vertData._deltaPos;
					}
				}
			}
			else
			{
				//2. Vertex 구성이 같으면
				// 그냥 For 돌면서 넣어주자
				VertData vertData = null;
				apModifiedVertex modVert = null;
				for (int i = 0; i < nVert; i++)
				{
					vertData = _vertices[i];
					modVert = modMesh._vertices[i];

					modVert._deltaPos = vertData._deltaPos;
				}
			}

			modMesh._transformMatrix = new apMatrix(_transformMatrix);
			modMesh._meshColor = _meshColor;
			modMesh._isVisible = _isVisible;

			return true;
		}



		// Get / Set
		//--------------------------------------------
	}

}