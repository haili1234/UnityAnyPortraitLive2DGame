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
	/// 단순히 AnyPortrait에 의해 생성된 객체를 의미하는 스크립트
	/// Update는 되지 않는다. 일종의 플래그로서의 역할을 한다.
	/// Parent 스크립트 정보만 가지고 있다. (단순 참조용)
	/// </summary>
	public class apOptNode : MonoBehaviour
	{
		[HideInInspector]
		public int _param = -1;//<<별도로 지정하고자 하는 값

		void Start()
		{
			this.enabled = false;
		}

		// Update is called once per frame
		void Update()
		{
			//업데이트하면 안됩니더.
			this.enabled = false;
		}
	}
}
