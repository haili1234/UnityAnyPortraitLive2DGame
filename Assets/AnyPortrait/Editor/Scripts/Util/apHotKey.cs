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
	/// 에디터의 단축키를 처리하는 객체.
	/// 단축키 처리는 OnGUI이 후반부에 해야하는데,
	/// UI별로 단축키에 대한 처리 요구가 임의의 위치에서 이루어지므로, 이를 대신 받아서 지연시키는 객체.
	/// 모든 함수 요청은 OnGUI마다 리셋되고 다시 받는다.
	/// 이벤트에 따라 묵살될 수 있다.
	/// </summary>
	public class apHotKey
	{
		public delegate void FUNC_HOTKEY_EVENT(object paramObject);

		// Unit Class
		public class HotKeyEvent
		{
			public KeyCode _keyCode = KeyCode.None;
			public string _label = null;
			public bool _isShift = false;
			public bool _isAlt = false;
			public bool _isCtrl = false;
			public object _paramObject = null;
			public FUNC_HOTKEY_EVENT _funcEvent = null;

			public HotKeyEvent(FUNC_HOTKEY_EVENT funcEvent, string label, KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
			{
				_funcEvent = funcEvent;
				_label = label;
				_keyCode = keyCode;
				_isShift = isShift;
				_isAlt = isAlt;
				_isCtrl = isCtrl;
				_paramObject = paramObject;
			}

		}

		// Members
		//---------------------------------------------
		private List<HotKeyEvent> _hotKeyEvents = new List<HotKeyEvent>();
		private bool _isAnyEvent = false;


		// Init
		//---------------------------------------------
		public apHotKey()
		{
			_isAnyEvent = false;
			_hotKeyEvents.Clear();
		}


		/// <summary>
		/// OnGUI 초기에 호출해주자
		/// </summary>
		public void Clear()
		{
			if (_isAnyEvent)
			{
				_isAnyEvent = false;
				_hotKeyEvents.Clear();
			}
		}


		// Functions
		//---------------------------------------------
		public void AddHotKeyEvent(FUNC_HOTKEY_EVENT funcEvent, string label, KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
		{
			_hotKeyEvents.Add(new HotKeyEvent(funcEvent, label, keyCode, isShift, isAlt, isCtrl, paramObject));
			_isAnyEvent = true;
		}

		/// <summary>
		/// OnGUI 후반부에 체크해준다.
		/// Event가 used가 아니라면 호출 가능
		/// </summary>
		/// <param name=""></param>
		public HotKeyEvent CheckHotKeyEvent(KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl)
		{
			if (!_isAnyEvent)
			{
				return null;
			}
			HotKeyEvent hkEvent = null;
			for (int i = 0; i < _hotKeyEvents.Count; i++)
			{
				hkEvent = _hotKeyEvents[i];
				if (hkEvent._keyCode == keyCode &&
					hkEvent._isShift == isShift &&
					hkEvent._isAlt == isAlt &&
					hkEvent._isCtrl == isCtrl)
				{
					try
					{
						//저장된 이벤트를 실행하자
						hkEvent._funcEvent(hkEvent._paramObject);

						return hkEvent;
					}
					catch (Exception ex)
					{
						Debug.LogError("HotKey Event Exception : " + ex);
						return null;
					}
				}
			}
			return null;
		}





		// Get / Set
		//---------------------------------------------
	}

}