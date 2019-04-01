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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 본 리타겟을 위해 파일에 저장되는 단위
	/// "기본 구조"에 대한 파일인지 "애니메이션/정적 포즈"에 대한 파일인지 구분한다.
	/// 본 구조는 계층적으로 설정한다.
	/// 키값은 별도의 ID로 구분한다.
	/// </summary>
	public class apRetargetBoneUnit
	{
		// Members
		//---------------------------------------------------
		//기본적인 본 정보를 가지고 있다.
		//UniqueID만 제외하고 UnitID로 교체한다.
		public int _unitID = -1;
		
		//Export할때만 가지는 값. Import할때는 -1이다.
		public int _boneUniqueID = -1;
		public apBone _linkedBone = null;


		public string _name = "";
		public int _parentUnitID = -1;//<<BoneID가 아닌 RetargetBoneUnitID를 사용한다.
		public int _level = -1;
		public int _depth = -1;
		public List<int> _childUnitID = new List<int>();

		public apMatrix _defaultMatrix = new apMatrix();

		public Color _color = Color.white;
		public int _shapeWidth = 30;
		public int _shapeLength = 50;
		public int _shapeTaper = 100;

		public apBone.OPTION_IK _optionIK = apBone.OPTION_IK.Disabled;

		public bool _isIKTail = false;

		public int _IKTargetBoneUnitID = -1;
		public int _IKNextChainedBoneUnitID = -1;
		public int _IKHeaderBoneUnitID = -1;

		public bool _isIKAngleRange = false;
		public float _IKAngleRange_Lower = -90.0f;
		public float _IKAngleRange_Upper = 90.0f;
		public float _IKAnglePreferred = 0.0f;

		public bool _isSocketEnabled = false;

		//------------------------------------------------
		// 로드된 정보를 어떻게 적용할 것인가에 대한 정보
		public bool _isImportEnabled = true;
		public bool _isIKEnabled = true;
		public bool _isShapeEnabled = true;

		// Init
		//---------------------------------------------------
		public apRetargetBoneUnit()
		{
			
		}



		// Functions
		//---------------------------------------------------
		// Bone to File
		//---------------------------------------------------
		public void SetBone(int unitID, apBone bone, Dictionary<int, int> boneID2UnitIDs)
		{
			_unitID = unitID;

			_boneUniqueID = bone._uniqueID;
			_linkedBone = bone;


			_name = bone._name;

			if(bone._parentBoneID < 0)
			{
				_parentUnitID = -1;
			}
			else
			{
				_parentUnitID = boneID2UnitIDs[bone._parentBoneID];
			}
			 
			_level = bone._level;
			_depth = bone._depth;
			_childUnitID.Clear();
			if(bone._childBoneIDs != null)
			{
				for (int i = 0; i < bone._childBoneIDs.Count; i++)
				{
					int childID = bone._childBoneIDs[i];
					if(childID >= 0)
					{
						_childUnitID.Add(boneID2UnitIDs[childID]);
					}
				}
			}
			
			_defaultMatrix.SetMatrix(bone._defaultMatrix);

			_color = bone._color;
			_shapeWidth = bone._shapeWidth;
			_shapeLength = bone._shapeLength;
			_shapeTaper = bone._shapeTaper;

			_optionIK = bone._optionIK;

			_isIKTail = bone._isIKTail;

			_IKTargetBoneUnitID = -1;
			if(bone._IKTargetBoneID >= 0)
			{
				_IKTargetBoneUnitID = boneID2UnitIDs[bone._IKTargetBoneID];
			}

			_IKNextChainedBoneUnitID = -1;
			if(bone._IKNextChainedBoneID >= 0)
			{
				_IKNextChainedBoneUnitID = boneID2UnitIDs[bone._IKNextChainedBoneID];
			}
			_IKHeaderBoneUnitID = -1;
			if(bone._IKHeaderBoneID >= 0)
			{
				_IKHeaderBoneUnitID = boneID2UnitIDs[bone._IKHeaderBoneID];
			}

			_isIKAngleRange = bone._isIKAngleRange;
			_IKAngleRange_Lower = bone._IKAngleRange_Lower;
			_IKAngleRange_Upper = bone._IKAngleRange_Upper;
			_IKAnglePreferred = bone._IKAnglePreferred;
			_isSocketEnabled = false;
		}


		public string GetEncodingData()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			if (_name.Length < 10)
			{
				sb.Append("00" + _name.Length.ToString());
			}
			else if(_name.Length < 100)
			{
				sb.Append("0" + _name.Length.ToString());
			}
			else
			{
				sb.Append(_name.Length.ToString());
			}
			sb.Append(_name);
			sb.Append(_unitID);			sb.Append("/");
			sb.Append(_parentUnitID);	sb.Append("/");
			sb.Append(_level);			sb.Append("/");
			sb.Append(_depth);			sb.Append("/");

			sb.Append(_childUnitID.Count);			sb.Append("/");
			for (int i = 0; i < _childUnitID.Count; i++)
			{
				sb.Append(_childUnitID[i]);
				sb.Append("/");
			}

			sb.Append(_defaultMatrix._pos.x);		sb.Append("/");
			sb.Append(_defaultMatrix._pos.y);		sb.Append("/");
			sb.Append(_defaultMatrix._angleDeg);	sb.Append("/");
			sb.Append(_defaultMatrix._scale.x);		sb.Append("/");
			sb.Append(_defaultMatrix._scale.y);		sb.Append("/");

			sb.Append(_color.r);	sb.Append("/");
			sb.Append(_color.g);	sb.Append("/");
			sb.Append(_color.b);	sb.Append("/");
			sb.Append(_color.a);	sb.Append("/");

			sb.Append(_shapeWidth);		sb.Append("/");
			sb.Append(_shapeLength);	sb.Append("/");
			sb.Append(_shapeTaper);	sb.Append("/");

			sb.Append((int)_optionIK);	sb.Append("/");
			sb.Append((_isIKTail ? "1" : "0"));	sb.Append("/");

			sb.Append(_IKTargetBoneUnitID);			sb.Append("/");
			sb.Append(_IKNextChainedBoneUnitID);	sb.Append("/");
			sb.Append(_IKHeaderBoneUnitID);			sb.Append("/");

			sb.Append((_isIKAngleRange ? "1" : "0"));	sb.Append("/");
			sb.Append(_IKAngleRange_Lower);				sb.Append("/");
			sb.Append(_IKAngleRange_Upper);				sb.Append("/");
			sb.Append(_IKAnglePreferred);				sb.Append("/");

			sb.Append((_isSocketEnabled ? "1" : "0"));	sb.Append("/");

			return sb.ToString();
		}




		// File To Bone
		//---------------------------------------------------
		public bool DecodeData(string strSrc)
		{
			try
			{
				if(strSrc.Length < 3)
				{
					return false;
				}
				int nName = int.Parse(strSrc.Substring(0, 3));
				_name = strSrc.Substring(3, nName);

				strSrc = strSrc.Substring(3 + nName);

				//나머지는 델리미터를 이용한다.
				string[] strUnits = strSrc.Split(new string[] { "/" }, StringSplitOptions.None);
				int iStr = 0;
				_unitID = int.Parse(strUnits[iStr]);
				iStr++;

				_parentUnitID = int.Parse(strUnits[iStr]);
				iStr++;

				_level = int.Parse(strUnits[iStr]);
				iStr++;

				_depth = int.Parse(strUnits[iStr]);
				iStr++;

				_childUnitID.Clear();
				int nChild = int.Parse(strUnits[iStr]);
				iStr++;

				for (int i = 0; i < nChild; i++)
				{
					_childUnitID.Add(int.Parse(strUnits[iStr]));
					iStr++;
				}

				_defaultMatrix.SetIdentity();
				_defaultMatrix.SetTRS(
					new Vector2(float.Parse(strUnits[iStr]), float.Parse(strUnits[iStr + 1])),
					float.Parse(strUnits[iStr + 2]),
					new Vector2(float.Parse(strUnits[iStr + 3]), float.Parse(strUnits[iStr + 4]))
					);

				iStr += 5;

				_color.r = float.Parse(strUnits[iStr]);
				_color.g = float.Parse(strUnits[iStr + 1]);
				_color.b = float.Parse(strUnits[iStr + 2]);
				_color.a = float.Parse(strUnits[iStr + 3]);
				iStr += 4;

				_shapeWidth = int.Parse(strUnits[iStr]);
				_shapeLength = int.Parse(strUnits[iStr + 1]);
				_shapeTaper = int.Parse(strUnits[iStr + 2]);
				iStr += 3;

				_optionIK = (apBone.OPTION_IK)int.Parse(strUnits[iStr]);
				_isIKTail = (int.Parse(strUnits[iStr + 1]) == 1) ? true : false;
				iStr += 2;

				_IKTargetBoneUnitID = int.Parse(strUnits[iStr]);
				_IKNextChainedBoneUnitID = int.Parse(strUnits[iStr + 1]);
				_IKHeaderBoneUnitID = int.Parse(strUnits[iStr + 2]);
				iStr += 3;

				_isIKAngleRange = (int.Parse(strUnits[iStr]) == 1) ? true : false;
				_IKAngleRange_Lower = float.Parse(strUnits[iStr + 1]);
				_IKAngleRange_Upper = float.Parse(strUnits[iStr + 2]);
				_IKAnglePreferred = float.Parse(strUnits[iStr + 3]);
				iStr += 4;

				_isSocketEnabled = (int.Parse(strUnits[iStr]) == 1) ? true : false;

				_isImportEnabled = true;
				_isIKEnabled = true;
				_isShapeEnabled = true;
			}
			catch(Exception ex)
			{
				Debug.LogError("Decode Exception : " + ex);
				return false;
			}
			

			return true;
		}


		// Get / Set
		//---------------------------------------------------
		//Bone 정보를 어떻게 저장해야하나..
	}
}