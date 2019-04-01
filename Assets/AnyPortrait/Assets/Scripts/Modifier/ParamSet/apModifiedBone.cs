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
	/// ParamSet에 포함되며, ModifiedMesh와 동등한 레벨에서 처리된다.
	/// MeshGroup 내의 Bone 한개에 대한 정보를 가지고 있다.
	/// </summary>
	[Serializable]
	public class apModifiedBone
	{
		// Members
		//------------------------------------------
		public int _meshGroupUniqueID_Modifier = -1;
		public int _meshGropuUniqueID_Bone = -1;

		public int _transformUniqueID = -1;//_meshGropuUniqueID_Bone의 MeshGroupTransform이다.

		[NonSerialized]
		public apMeshGroup _meshGroup_Modifier = null;

		[NonSerialized]
		public apMeshGroup _meshGroup_Bone = null;

		/// <summary>
		/// 선택된 Bone의 MeshGroup(RenderUnit)이 포함된 루트 MeshGroupTransform
		/// </summary>
		[NonSerialized]
		public apTransform_MeshGroup _meshGroupTransform = null;

		public int _boneID = -1;

		[NonSerialized]
		public apBone _bone = null;

		[NonSerialized]
		public apRenderUnit _renderUnit = null;//Parent MeshGroup의 RenderUnit

		/// <summary>
		/// Bone 제어 정보.
		/// </summary>
		[SerializeField]
		public apMatrix _transformMatrix = new apMatrix();


		// Init
		//------------------------------------------
		public apModifiedBone()
		{

		}
		public void Init(int meshGroupID_Modifier, int meshGroupID_Bone, int meshGroupTransformID, apBone bone)
		{
			_meshGroupUniqueID_Modifier = meshGroupID_Modifier;
			_meshGropuUniqueID_Bone = meshGroupID_Bone;
			_transformUniqueID = meshGroupTransformID;

			_bone = bone;
			_boneID = bone._uniqueID;
		}

		//TODO Link 등등
		//에디터에서 제대로 Link를 해야한다.
		public void Link(apMeshGroup meshGroup_Modifier, apMeshGroup meshGroup_Bone, apBone bone, apRenderUnit renderUnit, apTransform_MeshGroup meshGroupTransform)
		{
			_meshGroup_Modifier = meshGroup_Modifier;
			_meshGroup_Bone = meshGroup_Bone;
			_bone = bone;
			_renderUnit = renderUnit;

			_meshGroupTransform = meshGroupTransform;
		}




		// Functions
		//------------------------------------------
		public void UpdateBeforeBake(apPortrait portrait, apMeshGroup mainMeshGroup, apTransform_MeshGroup mainMeshGroupTransform)
		{

		}


		// Get / Set
		//------------------------------------------

	}
}