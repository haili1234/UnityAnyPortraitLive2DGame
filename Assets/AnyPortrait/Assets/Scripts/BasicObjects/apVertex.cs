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
	/// <summary>
	/// 메시를 이루는 버텍스 정보
	/// 기본 세팅 값 + 애니메이션 정보를 위한 처리 등을 포함한다.
	/// </summary>
	[Serializable]
	public class apVertex
	{
		// Members
		//------------------------------------
		// Default Setting
		public int _index;//Index Buffer에 들어가는 ID (배열 ID)
		public int _uniqueID = -1;//<<고유번호를 발급하자

		[SerializeField]
		public Vector2 _pos;

		[SerializeField]
		public Vector2 _uv;

		// Z값을 별도로 관리한다.
		//Z는 스케일이 되지 않고 고정 값으로 적용된다.
		//Opt에서는 Mesh간 간격의 50%를 이동한다.
		//0~1의 값을 가진다.
		[SerializeField]
		public float _zDepth = 0.0f;
		//Weight 들
		//public float _volumeWeight = 0.0f;//양각 Weight (0~1)
		//public float _physicsWeight = -1.0f;//물리 Weight (-1 / 0~1)
		//									//public float[] _boneWeights = new float[4];
		//									//public apBone[] _bones = new apBone[4];




		// Init
		//------------------------------------
		/// <summary>
		/// 기본 생성자 -> 이건 백업 파싱때에만 사용한다. 그 외에는 이 생성자를 사용하지 않는다.
		/// </summary>
		public apVertex()
		{
			
		}

		public apVertex(int index, int uniqueID, Vector3 pos, Vector2 uv)
		{
			SetDefaultSetting(index, uniqueID, pos, uv);
		}




		// Functions
		//------------------------------------
		public void SetDefaultSetting(int index, int uniqueID, Vector3 pos, Vector2 uv)
		{
			_index = index;
			_uniqueID = uniqueID;
			_pos = pos;
			_uv = uv;

			_zDepth = 0.0f;

			//_volumeWeight = 0.0f;
			//_physicsWeight = -1.0f;
			//for (int i = 0; i < 4; i++)
			//{
			//	_boneWeights[i] = 0.0f;
			//	_bones[i] = null;
			//}
		}


		// To String <-> From String
		//------------------------------------
		//public override string ToString()
		//{
		//	return "(" +
		//		"index:" + _index + "/" +
		//		"pos:" + _pos.x + ":" + _pos.y + "/" +
		//		"uv:" + _uv.x + ":" + _uv.y + //"/" +
		//		")";
		//}

		//public void FromString(string strData)
		//{
		//	_index = -1;
		//	_pos = Vector3.zero;
		//	_uv = Vector2.zero;

		//	strData = strData.Replace("(", "");
		//	strData = strData.Replace(")", "");

		//	string[] strUnits = strData.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
		//	for (int iUnit = 0; iUnit < strUnits.Length; iUnit++)
		//	{
		//		string[] strValues = strUnits[iUnit].Split(new string[] { ":" }, StringSplitOptions.None);
		//		if (strValues == null || strValues.Length <= 1)
		//		{
		//			continue;
		//		}

		//		int nValue = strValues.Length - 1;

		//		string strKey = strValues[0];
		//		if (strKey.Equals("index"))
		//		{
		//			if (nValue >= 1)
		//			{
		//				_index = int.Parse(strValues[1]);
		//			}
		//		}
		//		else if (strKey.Equals("pos"))
		//		{
		//			if (nValue >= 3)
		//			{
		//				_pos.x = float.Parse(strValues[1]);
		//				_pos.y = float.Parse(strValues[2]);
		//			}
		//		}
		//		else if (strKey.Equals("uv"))
		//		{
		//			if (nValue >= 2)
		//			{
		//				_uv.x = float.Parse(strValues[1]);
		//				_uv.y = float.Parse(strValues[2]);
		//			}
		//		}
		//		//TODO..
		//	}
		//}
	}

}