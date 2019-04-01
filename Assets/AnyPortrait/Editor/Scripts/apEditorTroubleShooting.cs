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
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{

	public class apEditorTroubleShooting : EditorWindow
	{
		//private static apEditor2 s_window = null;

		//--------------------------------------------------------------
		//우선순위가 10 이상 차이가 나면 구분자가 생긴다.

		[MenuItem("Window/AnyPortrait/Editor Reset", false, 21)]
		public static void ShowWindow()
		{
			apEditor.CloseEditor();
			Debug.LogWarning("AnyPortrait Editor is Closed.");
		}
		
		//--------------------------------------------------------------
		public static bool IsKorean()
		{
			apEditor.LANGUAGE language = (apEditor.LANGUAGE)EditorPrefs.GetInt("AnyPortrait_Language", (int)apEditor.LANGUAGE.English);
			return language == apEditor.LANGUAGE.Korean;
		}


		[MenuItem("Window/AnyPortrait/Homepage", false, 41)]
		public static void OpenHomepage()
		{
			Application.OpenURL("https://www.rainyrizzle.com/");
		}

		[MenuItem("Window/AnyPortrait/Getting Started", false, 42)]
		public static void OpenGettingStarted()
		{
			string url = "";
			if(IsKorean())
			{
				url = "https://www.rainyrizzle.com/ap-gettingstarted-kor";
			}
			else
			{
				url = "https://www.rainyrizzle.com/ap-gettingstarted-eng";
			}
			Application.OpenURL(url);
		}

		[MenuItem("Window/AnyPortrait/Scripting", false, 43)]
		public static void OpenScripting()
		{
			string url = "";
			if(IsKorean())
			{
				url = "https://www.rainyrizzle.com/ap-scripting-kor";
			}
			else
			{
				url = "https://www.rainyrizzle.com/ap-scripting-eng";
			}
			Application.OpenURL(url);
		}


		//이 기능은 뺍시다.
		//[MenuItem("Window/AnyPortrait/Submit a Survey (Demo)", false, 81)]
		//public static void OpenSubmitASurvey()
		//{
		//	Application.OpenURL("https://goo.gl/forms/xZqTaXTesYq6v1Ba2");
		//}


		[MenuItem("Window/AnyPortrait/Report a Bug or Suggestion", false, 82)]
		public static void OpenReportABug()
		{
			string url = "";
			if(IsKorean())
			{
				url = "https://www.rainyrizzle.com/anyportrait-report-kor";
			}
			else
			{
				url = "https://www.rainyrizzle.com/anyportrait-report-eng";
			}
			//Application.OpenURL("https://goo.gl/forms/f03CdFRr58VTCqv53");
			Application.OpenURL(url);
		}
	}

}