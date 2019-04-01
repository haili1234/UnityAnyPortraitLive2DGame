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

	public class apMouse
	{
		// Members
		//------------------------------------------------
		// 마우스 이벤트
		public enum MouseBtnStatus
		{
			Down,
			Pressed,
			Up,
			Released
		}

		private MouseBtnStatus _mouseStatus = MouseBtnStatus.Released;

		private static Vector2 _mousePos = Vector2.zero;
		private Vector2 _mousePos_Down = Vector2.zero;

		private static Vector2 _mousePos_NotBound = Vector2.zero;

		private static Vector2 _mousePos_Last = Vector2.zero;

		//private int _btnIndex = 0;
		private int _wheelValue = 0;

		private bool _isUpdate = false;

		// Get / Set
		//------------------------------------------------

		public MouseBtnStatus Status { get { return _mouseStatus; } }
		public static Vector2 Pos { get { return _mousePos; } }
		public Vector2 PosDelta { get { return _mousePos - _mousePos_Down; } }
		public static Vector2 PosLast { get { return _mousePos_Last; } }

		public static Vector2 PosNotBound { get { return _mousePos_NotBound; } }
		public int Wheel { get { return _wheelValue; } }

		//------------------------------------------------
		public apMouse()
		{
			//_btnIndex = btnIndex;

			Init();
		}


		public void Init()
		{
			_mouseStatus = MouseBtnStatus.Released;
			_mousePos = Vector2.zero;
			_mousePos_Down = Vector2.zero;
		}

		public void ReadyToUpdate()
		{
			_isUpdate = false;
			_wheelValue = 0;
		}

		public static void SetMousePos(Vector2 mousePos, Vector2 mousePos_NotBound)
		{
			_mousePos = mousePos;
			_mousePos_Last = mousePos;
			_mousePos_NotBound = mousePos_NotBound;
		}



		public void Update_Pressed()
		{
			//_mousePos = mousePos;

			switch (_mouseStatus)
			{
				case MouseBtnStatus.Down:
				case MouseBtnStatus.Pressed:
					_mouseStatus = MouseBtnStatus.Pressed;

					//Debug.Log("Mouse Pressed [" + _btnIndex + "] : " + _mousePos);
					break;

				case MouseBtnStatus.Up:
				case MouseBtnStatus.Released:
					_mouseStatus = MouseBtnStatus.Down;
					_mousePos_Down = _mousePos;

					//Debug.LogWarning("Mouse Down [" + _btnIndex + "] : " + _mousePos_Down);
					break;
			}

			_isUpdate = true;
		}

		public void Update_Moved()
		{
			//_mousePos += mousePosDelta;

			switch (_mouseStatus)
			{
				case MouseBtnStatus.Down:
				case MouseBtnStatus.Pressed:
					_mouseStatus = MouseBtnStatus.Pressed;

					//Debug.Log("Mouse Pressed [" + _btnIndex + "] : " + _mousePos);
					break;

				case MouseBtnStatus.Up:
				case MouseBtnStatus.Released:
					_mouseStatus = MouseBtnStatus.Released;
					//_mousePos_Down = _mousePos;
					break;
			}

			//Debug.LogError("Mouse Moved [" + _btnIndex + "] : " + _mousePos);
			_isUpdate = true;
		}

		public void Update_Released()
		{
			//_mousePos = mousePos;

			switch (_mouseStatus)
			{
				case MouseBtnStatus.Down:
				case MouseBtnStatus.Pressed:
					_mouseStatus = MouseBtnStatus.Up;

					//Debug.LogWarning("Mouse Released [" + _btnIndex + "] : " + _mousePos_Down);
					break;

				case MouseBtnStatus.Up:
				case MouseBtnStatus.Released:
					_mouseStatus = MouseBtnStatus.Released;
					break;
			}

			_isUpdate = true;
		}

		public void Update_Wheel(int wheelOffset)
		{
			_wheelValue += wheelOffset;

			//Debug.LogWarning("Wheel [" + _btnIndex + "] : " + _wheelValue);
		}

		public void EndUpdate()
		{
			if (!_isUpdate)
			{
				switch (_mouseStatus)
				{
					case MouseBtnStatus.Down:
					case MouseBtnStatus.Pressed:
						_mouseStatus = MouseBtnStatus.Up;
						break;

					case MouseBtnStatus.Up:
					case MouseBtnStatus.Released:
						_mouseStatus = MouseBtnStatus.Released;
						break;
				}
			}
		}

		public void UseWheel()
		{
			_wheelValue = 0;
		}

		public void UseMouseDrag()
		{
			_mousePos_Down = _mousePos;
		}
	}
}