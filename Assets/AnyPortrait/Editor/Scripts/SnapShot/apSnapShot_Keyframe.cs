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

	public class apSnapShot_Keyframe : apSnapShotBase
	{
		// Members
		//--------------------------------------------
		//키값 (같은 키값일때 복사가 가능하다.
		private apAnimTimelineLayer _key_TimelineLayer = null;
		//<< 다른 AnimClip간에는 복사가 안되나?

		//저장되는 멤버 데이터
		//ModMesh 정보와 키프레임의 기본 정보를 모두 저장해야한다.
		public apAnimCurve _animCurve = null;
		public bool _isKeyValueSet = false;

		//public bool _conSyncValue_Bool = false;
		public int _conSyncValue_Int = 0;
		public float _conSyncValue_Float = 0.0f;
		public Vector2 _conSyncValue_Vector2 = Vector2.zero;
		//public Vector3 _conSyncValue_Vector3 = Vector3.zero;
		//public Color _conSyncValue_Color = Color.black;

		//ModMesh의 값도 넣어준다.
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
		public apSnapShot_Keyframe() : base()
		{

		}

		// Functions
		//--------------------------------------------
		public override bool IsKeySyncable(object target)
		{
			if (!(target is apAnimKeyframe))
			{
				return false;
			}

			apAnimKeyframe keyframe = target as apAnimKeyframe;
			if (keyframe == null)
			{
				return false;
			}

			//Key가 같아야 한다.
			if (keyframe._parentTimelineLayer != _key_TimelineLayer)
			{
				return false;
			}

			return true;
		}

		public override bool Save(object target, string strParam)
		{
			base.Save(target, strParam);

			apAnimKeyframe keyframe = target as apAnimKeyframe;
			if (keyframe == null)
			{
				return false;
			}

			_key_TimelineLayer = keyframe._parentTimelineLayer;
			if (_key_TimelineLayer == null)
			{
				return false;
			}

			_animCurve = new apAnimCurve(keyframe._curveKey, keyframe._frameIndex);
			_isKeyValueSet = keyframe._isKeyValueSet;

			//_conSyncValue_Bool = keyframe._conSyncValue_Bool;
			_conSyncValue_Int = keyframe._conSyncValue_Int;
			_conSyncValue_Float = keyframe._conSyncValue_Float;
			_conSyncValue_Vector2 = keyframe._conSyncValue_Vector2;
			//_conSyncValue_Vector3 = keyframe._conSyncValue_Vector3;
			//_conSyncValue_Color = keyframe._conSyncValue_Color;

			_vertices.Clear();
			_transformMatrix = new apMatrix();
			_meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			_isVisible = true;

			if (keyframe._linkedModMesh_Editor != null)
			{
				apModifiedMesh modMesh = keyframe._linkedModMesh_Editor;
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
			}
			else if (keyframe._linkedModBone_Editor != null)
			{
				apModifiedBone modBone = keyframe._linkedModBone_Editor;

				_transformMatrix = new apMatrix(modBone._transformMatrix);
			}
			return true;
		}

		public override bool Load(object targetObj)
		{
			apAnimKeyframe keyframe = targetObj as apAnimKeyframe;
			if (keyframe == null)
			{
				return false;
			}

			keyframe._curveKey = new apAnimCurve(_animCurve, keyframe._frameIndex);
			keyframe._isKeyValueSet = _isKeyValueSet;

			//keyframe._conSyncValue_Bool = _conSyncValue_Bool;
			keyframe._conSyncValue_Int = _conSyncValue_Int;
			keyframe._conSyncValue_Float = _conSyncValue_Float;
			keyframe._conSyncValue_Vector2 = _conSyncValue_Vector2;
			//keyframe._conSyncValue_Vector3 = _conSyncValue_Vector3;
			//keyframe._conSyncValue_Color = _conSyncValue_Color;


			if (keyframe._linkedModMesh_Editor != null)
			{
				apModifiedMesh modMesh = keyframe._linkedModMesh_Editor;

				VertData vertData = null;
				apModifiedVertex modVert = null;
				int nVert = _vertices.Count;
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

				modMesh._transformMatrix.SetMatrix(_transformMatrix);
				modMesh._meshColor = _meshColor;
				modMesh._isVisible = _isVisible;
			}
			else if (keyframe._linkedModBone_Editor != null)
			{
				apModifiedBone modBone = keyframe._linkedModBone_Editor;
				modBone._transformMatrix.SetMatrix(_transformMatrix);
			}

			return true;
		}
	}

}