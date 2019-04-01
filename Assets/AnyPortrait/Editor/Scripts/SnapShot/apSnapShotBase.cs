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

	public class apSnapShotBase
	{
		// Members
		//-----------------------------------------
		public object _target = null;
		public string _strParam = "";


		// Init
		//-----------------------------------------
		public apSnapShotBase()
		{

		}

		// Functions
		//-----------------------------------------
		public virtual bool IsKeySyncable(object target)
		{
			return false;
		}

		public virtual bool Save(object target, string strParam)
		{
			_target = target;
			_strParam = strParam;
			return true;
		}

		public virtual bool Load(object targetObj)
		{
			return false;
		}


		// Get / Set
		//-----------------------------------------
	}

}