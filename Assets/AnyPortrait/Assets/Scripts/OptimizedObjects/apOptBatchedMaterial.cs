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
	//Material Batch를 위해서 Material정보를 ID와 함께 저장해놓는다.
	//Bake 후에 다시 Link를 하면 이 정보를 참조하여 Material를 가져간다.
	//CustomShader를 포함하는 반면 Clipped Mesh는 예외로 둔다. (알아서 Shader로 만들 것)
	/// <summary>
	/// Material manager class for batch rendering
	/// This is done automatically, so it is recommended that you do not control it with scripts.
	/// </summary>
	[Serializable]
	public class apOptBatchedMaterial
	{
		// Members
		//----------------------------------------------------
		[Serializable]
		public class MaterialUnit
		{
			[SerializeField]
			public int _uniqueID = -1;

			[SerializeField]
			public Material _material = null;

			//일종의 키값이 되는 데이터
			[SerializeField]
			private Texture2D _texture = null;

			[SerializeField]
			private int _textureID = -1;

			[SerializeField]
			private Shader _shader = null;

			public MaterialUnit()
			{

			}

			public MaterialUnit(int uniqueID, Texture2D texture, int textureID, Shader shader)
			{
				_uniqueID = uniqueID;

				_texture = texture;
				_textureID = textureID;
				_shader = shader;

				_material = new Material(_shader);
				_material.SetTexture("_MainTex", _texture);
				_material.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 1.0f));
			}

			public bool IsEqualMaterial(Texture2D texture, int textureID, Shader shader)
			{
				return _texture == texture
					&& _textureID == textureID
					&& _shader == shader;
			}

			public void MakeMaterial()
			{
				_material = new Material(_shader);
				_material.SetTexture("_MainTex", _texture);
				_material.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 1.0f));
			}
		}

		[SerializeField]
		public List<MaterialUnit> _matUnits = new List<MaterialUnit>();
		
		

		// Init
		//----------------------------------------------------
		public apOptBatchedMaterial()
		{

		}

		public void Clear(bool isDestroyMaterial)
		{
			if (isDestroyMaterial)
			{
				for (int i = 0; i < _matUnits.Count; i++)
				{
					try
					{
						if (_matUnits[i]._material != null)
						{
							UnityEngine.Object.DestroyImmediate(_matUnits[i]._material);
						}
					}
					catch (Exception)
					{

					}
				}
			}
			_matUnits.Clear();
		}

		

		// Functions
		//----------------------------------------------------
		public MaterialUnit MakeBatchedMaterial(Texture2D texture, int textureID, Shader shader)
		{
			MaterialUnit result = _matUnits.Find(delegate (MaterialUnit a)
			{
				return a.IsEqualMaterial(texture, textureID, shader);
			});
			if(result != null)
			{
				Material resultMat = result._material;
				if(resultMat == null)
				{
					result.MakeMaterial();
					resultMat = result._material;
				}
				return result;
			}

			//새로 만들자
			int newID = _matUnits.Count + 1;

			result = new MaterialUnit(newID, texture, textureID, shader);
			_matUnits.Add(result);

			return result;
		}


		public Material GetMaterial(int materialID)
		{
			MaterialUnit result = _matUnits.Find(delegate (MaterialUnit a)
			{
				return a._uniqueID == materialID;
			});
			if(result != null)
			{
				if(result._material == null)
				{
					result.MakeMaterial();
				}
				return result._material;
			}
			return null;
		}

		// Get / Set
		//----------------------------------------------------
	}
}