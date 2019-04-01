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
	/// AnimClip 데이터 계산 후, 어떤 Control Param을 컨트롤하여 어떤 값을 가지게 할지 결정하는 데이터
	/// Keyframe 보간 계산 결과값이 들어간다.
	/// 멤버는 Sort시 미리 만든다.
	/// </summary>
	public class apAnimControlParamResult
	{
		// Members
		//--------------------------------------------------------------------
		public apControlParam _targetControlParam = null;
		//public float _weight = 1.0f;
		public bool _isCalculated = false;
		//public bool _value_Bool = false;
		public int _value_Int = 0;
		public float _value_Float = 0.0f;
		public Vector2 _value_Vec2 = Vector2.zero;
		//public Vector3 _value_Vec3 = Vector3.zero;
		//public Color _value_Color = Color.black;


		// Init
		//--------------------------------------------------------------------
		public apAnimControlParamResult(apControlParam targetControlParam)
		{
			_targetControlParam = targetControlParam;
		}


		//--------------------------------------------------------------------
		public void Init()
		{
			//_weight = 0.0f;
			_isCalculated = false;
			//_value_Bool = false;
			_value_Int = 0;
			_value_Float = 0.0f;
			_value_Vec2 = Vector2.zero;
			//_value_Vec3 = Vector3.zero;
			//_value_Color = Color.black;
		}


		// Set Calculated Value
		//--------------------------------------------------------------------
		public void SetKeyframeResult(apAnimKeyframe keyframe, float weight)
		{
			//_weight = Mathf.Clamp01(_weight + weight);


			switch (_targetControlParam._valueType)
			{
				//case apControlParam.TYPE.Bool:
				//	if(!_isCalculated)
				//	{
				//		//계산이 안되어있다면 -> Weight 상관없이 넣는다.
				//		_value_Bool = keyframe._conSyncValue_Bool;
				//	}
				//	else
				//	{
				//		if(weight > 0.5f)
				//		{
				//			//Weight가 0.5 이상일때의 값을 넣는다.
				//			_value_Bool = keyframe._conSyncValue_Bool;
				//		}
				//	}
				//	break;


				case apControlParam.TYPE.Int:
					_value_Int += (int)(keyframe._conSyncValue_Int * weight + 0.5f);
					break;


				case apControlParam.TYPE.Float:
					_value_Float += keyframe._conSyncValue_Float * weight;
					break;

				case apControlParam.TYPE.Vector2:
					_value_Vec2 += keyframe._conSyncValue_Vector2 * weight;
					break;

					//case apControlParam.TYPE.Vector3:
					//	_value_Vec3 += keyframe._conSyncValue_Vector3 * weight;
					//	break;


					//case apControlParam.TYPE.Color:
					//	_value_Color += keyframe._conSyncValue_Color * weight;
					//	break;
			}

			_isCalculated = true;
		}


		public void AdaptToControlParam()
		{
			if (!_isCalculated)
			{
				return;
			}

			switch (_targetControlParam._valueType)
			{
				//case apControlParam.TYPE.Bool:
				//	_targetControlParam._bool_Cur = _value_Bool;
				//	break;

				case apControlParam.TYPE.Int:
					_targetControlParam._int_Cur = _value_Int;
					break;

				case apControlParam.TYPE.Float:
					_targetControlParam._float_Cur = _value_Float;
					break;

				case apControlParam.TYPE.Vector2:
					_targetControlParam._vec2_Cur = _value_Vec2;
					break;

					//case apControlParam.TYPE.Vector3:
					//	_targetControlParam._vec3_Cur = _value_Vec3;
					//	break;


					//case apControlParam.TYPE.Color:
					//	_targetControlParam._color_Cur = _value_Color;
					//	break;
			}

		}


		public void AdaptToControlParam_Opt(float weight, apAnimPlayUnit.BLEND_METHOD blendMethod)
		{
			if (!_isCalculated)
			{
				return;
			}

			switch (_targetControlParam._valueType)
			{
				//case apControlParam.TYPE.Bool:
				//	_targetControlParam.SetCalculated_Bool(_value_Bool, weight, blendMethod);
				//	break;

				case apControlParam.TYPE.Int:
					_targetControlParam.SetCalculated_Int(_value_Int, weight, blendMethod);
					break;

				case apControlParam.TYPE.Float:
					_targetControlParam.SetCalculated_Float(_value_Float, weight, blendMethod);
					break;

				case apControlParam.TYPE.Vector2:
					_targetControlParam.SetCalculated_Vector2(_value_Vec2, weight, blendMethod);
					break;

					//case apControlParam.TYPE.Vector3:
					//	_targetControlParam.SetCalculated_Vector3(_value_Vec3, weight, blendMethod);
					//	break;


					//case apControlParam.TYPE.Color:
					//	_targetControlParam.SetCalculated_Color(_value_Color, weight, blendMethod);
					//	break;
			}
		}
	}

}