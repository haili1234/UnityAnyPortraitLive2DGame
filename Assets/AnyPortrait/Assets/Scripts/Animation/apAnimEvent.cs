/*
*	Copyright (c) 2017-2018. RainyRizzle. All rights reserved
*	contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of AnyPortrait.
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of Seungjik Lee.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using AnyPortrait;


namespace AnyPortrait
{
	// AnimClip에 저장하여 함수를 발생시킬 수 있다.
	// 단일 함수 호출, 구간 호출의 방식이 있다.
	// 이벤트 이름, 설명, 파라미터 타입/값을 설정한다.
	// 이름을 받을 Monobehaviour를 외부에서 지정을 해줘야한다.
	// 스킵되는걸 막기 위해 프레임을 지나쳐도 처리가 될 수 있다.
	
	/// <summary>
	/// A class that is stored in "apAnimClip" and invokes an event.
	/// </summary>
	[Serializable]
	public class apAnimEvent
	{
		// Members
		//---------------------------------------------
		public int _frameIndex = -1;
		public int _frameIndex_End = -1;//Continuous 타입일 때

		public string _eventName = "";//<<이벤트 이름이자 함수 이름. 독립적일 필요가 없다. (같은 함수를 여러번 호출할 수 있으므로)
		

		public enum CALL_TYPE
		{
			/// <summary>한번만 호출된다.</summary>
			Once = 0,
			/// <summary>구간에 들어서는 내내 호출이 된다.</summary>
			Continuous = 1,
		}

		[SerializeField]
		public CALL_TYPE _callType = CALL_TYPE.Once;

		//함수 호출시 같이 호출되는 인자이다.
		//1개를 호출할 경우 바로 처리되며, 여러개를 호출할 경우 배열 형태로 들어간다.
		public enum PARAM_TYPE
		{
			Bool = 0,
			Integer = 1,
			Float = 2,
			Vector2 = 3,
			String = 4,
		}
		
		[Serializable]
		public class SubParameter
		{
			[SerializeField]
			public PARAM_TYPE _paramType = PARAM_TYPE.Integer;

			public bool _boolValue = false;//<<이것도 보간이 안된다.
			public int _intValue = 0;
			public float _floatValue = 0.0f;
			public Vector2 _vec2Value = Vector2.zero;
			public string _strValue = "";//<<이건 보간이 안된다.

			public int _intValue_End = 0;
			public float _floatValue_End = 0.0f;
			public Vector2 _vec2Value_End = Vector2.zero;

			public SubParameter()
			{
				_paramType = PARAM_TYPE.Integer;

				_boolValue = false;
				_intValue = 0;
				_floatValue = 0.0f;
				_vec2Value = Vector2.zero;
				_strValue = "";//<<이건 보간이 안된다.

				_intValue_End = 0;
				_floatValue_End = 0.0f;
				_vec2Value_End = Vector2.zero;
			}
		}

		[SerializeField]
		public List<SubParameter> _subParams = new List<SubParameter>();

		//실행을 위한 변수들

		//이벤트 파라미터들 (2개 이상일 때)
		[NonSerialized]
		private object[] _subParamsToCallMultiple = null;

		private object _subParamToCallSingle = null;//<<한개일 때


		[NonSerialized]
		private int _nSubParams = -1;

		[NonSerialized]
		private bool _isEventCalled = false;//이벤트가 호출이 되었는가

		[NonSerialized]
		private bool _isCalculated = false;

		[NonSerialized]
		private bool _isPrevForwardPlay = true;//이전 프레임에서의 재생 방향. 바뀌게 된 경우 이벤트 호출이 바뀐다.


		// Init
		//---------------------------------------------
		public apAnimEvent()
		{

		}
		
		// Functions
		//---------------------------------------------
		/// <summary>
		/// 이벤트를 다시 호출할 수 있다. 이 함수를 호출하지 않으면 Loop 이후에 다시 호출되지 않는다.
		/// </summary>
		public void ResetCallFlag()
		{
			_isEventCalled = false;
		}

		/// <summary>
		/// 애니메이션 재생 후 이벤트 호출을 해야할지 말지 결정하기 위한 함수.
		/// 이 함수를 호출한 후, IsEventCallable, GetCalculatedParam를 순서대로 호출한다.
		/// </summary>
		/// <param name="frame"></param>
		public void Calculate(float fFrame, int iFrame, bool isForwardPlay)
		{
			_isCalculated = IsCalculatable(fFrame, iFrame, isForwardPlay);

			if(!_isCalculated)
			{	
				return;
			}

			if(_nSubParams < 0)
			{
				_nSubParams = _subParams.Count;
				if (_nSubParams == 1)
				{
					//1개일 때
					switch (_subParams[0]._paramType)
					{
						case PARAM_TYPE.Bool:		_subParamToCallSingle = _subParams[0]._boolValue; break;
						case PARAM_TYPE.Integer:	_subParamToCallSingle = _subParams[0]._intValue; break; 
						case PARAM_TYPE.Float:		_subParamToCallSingle = _subParams[0]._floatValue; break;
						case PARAM_TYPE.Vector2:	_subParamToCallSingle = _subParams[0]._vec2Value; break;
						case PARAM_TYPE.String:		_subParamToCallSingle = _subParams[0]._strValue; break;
					}
				}
				else if (_nSubParams >= 2)
				{
					_subParamsToCallMultiple = new object[_nSubParams];

					if (_callType == CALL_TYPE.Once)
					{
						//2개 이상일 때
						//Once는 한번만 파라미터를 넣으면 된다.

						for (int i = 0; i < _nSubParams; i++)
						{
							switch (_subParams[i]._paramType)
							{
								case PARAM_TYPE.Bool:		_subParamsToCallMultiple[i] = _subParams[i]._boolValue; break;
								case PARAM_TYPE.Integer:	_subParamsToCallMultiple[i] = _subParams[i]._intValue; break;
								case PARAM_TYPE.Float:		_subParamsToCallMultiple[i] = _subParams[i]._floatValue; break;
								case PARAM_TYPE.Vector2:	_subParamsToCallMultiple[i] = _subParams[i]._vec2Value; break;
								case PARAM_TYPE.String:		_subParamsToCallMultiple[i] = _subParams[i]._strValue; break;
							}

						}

					}
				}
			}

			//보간 계산을 합시다.
			if(_callType == CALL_TYPE.Once)
			{
				//Once 타입은 이미 값이 저장되어 있다.
				_isCalculated = true;
				_isEventCalled = true;//처리가 완료되어 리셋 전까지는 처리되지 않는다.
			}
			else
			{
				//Contious 타입은 Frame 길이에 따라 보간을 한다.
				float itp = 0.0f;
				if(_frameIndex < _frameIndex_End)//정상적으로 Start < End 일때
				{
					//if (iFrame == _frameIndex)
					//{
					//	itp = 0.0f;
					//	Debug.Log("[" + _eventName + "] Start [" + iFrame + " / " + fFrame + "] (" + _frameIndex + " ~ " + _frameIndex_End + ")");
					//}
					//else if (iFrame == _frameIndex_End)
					//{
					//	itp = 1.0f;
					//}
					//else
					//{
						
					//}
					itp = Mathf.Clamp01((float)(fFrame - _frameIndex) / (float)(_frameIndex_End - _frameIndex));
				}

				if (_nSubParams == 1)
				{
					switch (_subParams[0]._paramType)
						{
							case PARAM_TYPE.Bool:
								_subParamToCallSingle = _subParams[0]._boolValue;//<<Bool은 보간이 안된다.
								break;

							case PARAM_TYPE.Integer:
								_subParamToCallSingle = (int)(((float)_subParams[0]._intValue * ( 1- itp)) + ((float)_subParams[0]._intValue_End * itp) + 0.5f);
								break;

							case PARAM_TYPE.Float:
								_subParamToCallSingle = (_subParams[0]._floatValue * (1- itp)) + (_subParams[0]._floatValue_End * itp);
								break;

							case PARAM_TYPE.Vector2:
								_subParamToCallSingle = (_subParams[0]._vec2Value * (1- itp)) + (_subParams[0]._vec2Value_End * itp);
								break;

							case PARAM_TYPE.String:
								_subParamToCallSingle = _subParams[0]._strValue;//String도 보간이 안된다.
								break;
						}
				}
				else if (_nSubParams >= 2)
				{
					for (int i = 0; i < _nSubParams; i++)
					{
						switch (_subParams[i]._paramType)
						{
							case PARAM_TYPE.Bool:
								_subParamsToCallMultiple[i] = _subParams[i]._boolValue;//<<Bool은 보간이 안된다.
								break;

							case PARAM_TYPE.Integer:
								_subParamsToCallMultiple[i] = (int)(((float)_subParams[i]._intValue * (1- itp)) + ((float)_subParams[i]._intValue_End * itp) + 0.5f);
								break;

							case PARAM_TYPE.Float:
								_subParamsToCallMultiple[i] = (_subParams[i]._floatValue * (1- itp)) + (_subParams[i]._floatValue_End * itp);
								break;

							case PARAM_TYPE.Vector2:
								_subParamsToCallMultiple[i] = (_subParams[i]._vec2Value * (1- itp)) + (_subParams[i]._vec2Value_End * itp);
								break;

							case PARAM_TYPE.String:
								_subParamsToCallMultiple[i] = _subParams[i]._strValue;//String도 보간이 안된다.
								break;
						}

					}
				}

				
				if (isForwardPlay)
				{
					if ((int)fFrame >= _frameIndex_End)
					{
						//프레임이 지났으면 더이상 호출하지 않는다.
						_isEventCalled = true;
					}
				}
				else
				{
					//애니메이션 재생이 반대라면 End가 아닌 Start 지점에서 처리해야한다.
					if ((int)(fFrame + 0.5f) <= _frameIndex)
					{
						//프레임이 지났으면 더이상 호출하지 않는다.
						_isEventCalled = true;
					}
				}
			}


			if(_isPrevForwardPlay != isForwardPlay)
			{
				//만약 재생 방향이 바뀌었다면
				//영역 밖에서 EventCalled를 초기화한다.
				//단 영역의 범위는 조금 넓게 본다. (계속 반복해서 재생될 수 있으므로)
				if(_isEventCalled)
				{
					if(_callType == CALL_TYPE.Once)
					{
						if((int)(_frameIndex + 0.5f) < (_frameIndex - 3) || 
							(int)(_frameIndex + 0.5f) > (_frameIndex + 3))
						{
							ResetCallFlag();
						}
					}
					else
					{
						if((int)(_frameIndex + 0.5f) < (_frameIndex - 3) || 
							(int)(_frameIndex + 0.5f) > (_frameIndex_End + 3))
						{
							ResetCallFlag();
						}
					}
				}
			}
			_isPrevForwardPlay = isForwardPlay;
		}

		/// <summary>
		/// 해당 프레임에 대해서 이벤트를 호출할 수 있는가.
		/// 이미 했거나 범위에서 벗어나면 제외
		/// </summary>
		/// <param name="frame"></param>
		/// <returns></returns>
		private bool IsCalculatable(float fFrame, int iFrame, bool isForwardPlay)
		{
			if(_isEventCalled)
			{
				return false;
			}

			if(_isPrevForwardPlay != isForwardPlay)
			{
				//만약 재생 방향이 바뀌었다면
				//이벤트는 한정적으로만 처리해야한다.
				if(_callType == CALL_TYPE.Once)
				{
					//if((int)(fFrame + 0.5f) == _frameIndex)
					if(iFrame == _frameIndex)
					{
						return true;
					}
				}
				else
				{
					//if ((int)fFrame >= _frameIndex && (int)(fFrame + 0.5f) <= _frameIndex_End)
					if (iFrame >= _frameIndex && iFrame <= _frameIndex_End)
					{
						//호출 가능하다.
						return true;
					}
				}
			}

			if(_callType == CALL_TYPE.Once)
			{
				if (isForwardPlay)
				{
					if ((int)fFrame >= _frameIndex)
					//if (iFrame >= _frameIndex)
					{
						//호출 가능하다.
						return true;
					}
				}
				else
				{
					//if ((int)(fFrame + 0.5f) <= _frameIndex)
					if (iFrame <= _frameIndex)
					{
						//호출 가능하다.
						return true;
					}
				}
			}
			else
			{
				//Continuous에서는 이벤트가 0.5프레임 넓게 인식된다.
				if (isForwardPlay)
				{
					if ((int)(fFrame + 0.5f) >= _frameIndex)
					//if (iFrame >= _frameIndex)
					{
						//호출 가능하다.
						return true;
					}
				}
				else
				{
					if ((int)(fFrame) <= _frameIndex_End)
					//if (iFrame <= _frameIndex_End)
					{
						//호출 가능하다.
						return true;
					}
				}
			}
			//그 외에는 호출 불가능함
			return false;
		}

		public bool IsEventCallable()
		{
			return _isCalculated;
		}
		


		public object GetCalculatedParam()
		{
			if(_nSubParams <= 0)
			{
				return null;
			}
			else if(_nSubParams == 1)
			{
				return _subParamToCallSingle;
			}
			else
			{
				return _subParamsToCallMultiple;
			}
		}

		//------------------------------------------------------------------------------
		// Copy For Bake
		//------------------------------------------------------------------------------
		public void CopyFromAnimEvent(apAnimEvent srcEvent)
		{
			_frameIndex = srcEvent._frameIndex;
			_frameIndex_End = srcEvent._frameIndex_End;

			_eventName = srcEvent._eventName;
			_callType = srcEvent._callType;

			_subParams.Clear();
			for (int iParam = 0; iParam < srcEvent._subParams.Count; iParam++)
			{
				SubParameter srcParam = srcEvent._subParams[iParam];

				//파라미터 복사
				SubParameter newParam = new SubParameter();

				newParam._paramType = srcParam._paramType;

				newParam._boolValue = srcParam._boolValue;
				newParam._intValue = srcParam._intValue;
				newParam._floatValue = srcParam._floatValue;
				newParam._vec2Value = srcParam._vec2Value;
				newParam._strValue = srcParam._strValue;

				newParam._intValue_End = srcParam._intValue_End;
				newParam._floatValue_End = srcParam._floatValue_End;
				newParam._vec2Value_End = srcParam._vec2Value_End;

				_subParams.Add(newParam);
			}
		}
	}

}