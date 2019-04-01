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

using AnyPortrait;

namespace AnyPortrait
{

	[Serializable]
	public class apRootUnit
	{
		// Members
		//--------------------------------------
		[NonSerialized]
		public apPortrait _portrait = null;

		[NonSerialized]
		public apMeshGroup _childMeshGroup = null;

		[NonSerialized]
		public apTransform_MeshGroup _childMeshGroupTransform = null;//<<이건 기본형으로만 사용한다.

		[NonSerialized]
		public apRenderUnit _renderUnit = null;//<<RenderUnit은 시리얼라이즈가 안된다.
											   //TODO
											   //Root 유닛에서 애니메이션이 포함된다.


		// Init
		//--------------------------------------
		public apRootUnit()
		{

		}

		public void SetPortrait(apPortrait portrait)
		{
			_portrait = portrait;
		}


		// Update
		//--------------------------------------
		public void Update(float tDelta)
		{
			if (_childMeshGroup == null)
			{
				return;
			}

			_childMeshGroup.UpdateRenderUnits(tDelta, false);
		}


		// Functions
		//--------------------------------------
		public void SetMeshGroup(apMeshGroup meshGroup)
		{
			if (meshGroup == null)
			{
				_childMeshGroup = null;

				//ID를 반납하자
				if (_childMeshGroupTransform != null)
				{
					//_portrait.PushUniqueID_Transform(_childMeshGroupTransform._transformUniqueID);
					_portrait.PushUnusedID(apIDManager.TARGET.Transform, _childMeshGroupTransform._transformUniqueID);

					_childMeshGroupTransform = null;
				}
				_renderUnit = null;
			}
			if (meshGroup != null)
			{

				_childMeshGroup = meshGroup;

				bool resetMeshGroupTransform = false;
				if (_childMeshGroupTransform == null)
				{
					resetMeshGroupTransform = true;
				}
				else if (_childMeshGroupTransform._meshGroupUniqueID != meshGroup._uniqueID)
				{
					//_portrait.PushUniqueID_Transform(_childMeshGroupTransform._transformUniqueID);
					_portrait.PushUnusedID(apIDManager.TARGET.Transform, _childMeshGroupTransform._transformUniqueID);

					_childMeshGroupTransform = null;
					resetMeshGroupTransform = true;
				}

				if (resetMeshGroupTransform)
				{
					//Root는 별도의 Transform_MeshGroup을 가진다.

					//새로운 ID로 Transform을 만들자
					//_childMeshGroupTransform = new apTransform_MeshGroup(_portrait.MakeUniqueID_Transform());
					_childMeshGroupTransform = new apTransform_MeshGroup(_portrait.MakeUniqueID(apIDManager.TARGET.Transform));

					_childMeshGroupTransform._meshGroupUniqueID = meshGroup._uniqueID;
					_childMeshGroupTransform._nickName = meshGroup._name;
					_childMeshGroupTransform._meshGroup = meshGroup;
					_childMeshGroupTransform._matrix = new apMatrix();
					_childMeshGroupTransform._isVisible_Default = true;

					_childMeshGroupTransform._depth = 1;

					//추가
					//Root Transform_MeshGroup에 해당하는 RenderUnit
					_renderUnit = new apRenderUnit(_portrait, "Root");
					_renderUnit.SetGroup(meshGroup, _childMeshGroupTransform, null);
					_renderUnit._childRenderUnits.Add(meshGroup._rootRenderUnit);

				}
				else
				{
					_childMeshGroupTransform._meshGroup = meshGroup;//Link만 하자
					if (_renderUnit == null)
					{
						_renderUnit = new apRenderUnit(_portrait, "Root");
						_renderUnit.SetGroup(meshGroup, _childMeshGroupTransform, null);
						_renderUnit._childRenderUnits.Add(meshGroup._rootRenderUnit);
					}
					else
					{
						_renderUnit._meshGroupTransform = _childMeshGroupTransform;
						_renderUnit._meshGroup = meshGroup;
						_renderUnit._childRenderUnits.Clear();
						_renderUnit._childRenderUnits.Add(meshGroup._rootRenderUnit);
					}
				}
			}
		}


		// Get / Set
		//--------------------------------------


	}

}