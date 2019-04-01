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
using AnyPortrait;

public class apTutorial_SDController : MonoBehaviour
{
	// Target AnyPortrait
	public apPortrait portrait;

	void Start ()
	{
		
	}
	
	void Update ()
	{
		if(Input.GetMouseButtonDown(0))
		{
			if(portrait.IsPlaying("Idle"))
			{	
				portrait.StopAll(0.3f);
			}
			else
			{
				portrait.CrossFade("Idle", 0.3f);
			}
		}
	}
}
