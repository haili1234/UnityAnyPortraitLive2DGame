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
using System;
using System.Text;
using System.Collections.Generic;
using AnyPortrait;

namespace AnyPortrait
{
	//업데이트 로그를 출력하고 알려준다.
	public class apDialog_UpdateLog : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_UpdateLog s_window = null;

		private apEditor.LANGUAGE _language = apEditor.LANGUAGE.English;
		//내용은 그냥 코드로 적자

		private Vector2 _scroll = Vector2.zero;
		private string _info = "";

		private string _str_GotoHomepage = "";
		private string _str_Close = "";


		// Show Window
		//------------------------------------------------------------------
		[MenuItem("Window/AnyPortrait/Update Log", false, 51)]
		public static void ShowDialog()
		{
			ShowDialog(null);
		}
		
		public static void ShowDialog(apEditor editor)
		{
			
			CloseDialog();

			
			
			
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_UpdateLog), true, "Update Log", true);
			apDialog_UpdateLog curTool = curWindow as apDialog_UpdateLog;

			//object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 500;
				int height = 700;
				s_window = curTool;
				if (editor != null)
				{
					s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
													(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
													width, height);
				}
				s_window.Init();
			}
		}

		public static void CloseDialog()
		{
			if (s_window != null)
			{
				try
				{
					s_window.Close();
				}
				catch (Exception ex)
				{
					Debug.LogError("Close Exception : " + ex);
				}
				s_window = null;
			}
		}

		// Init
		//------------------------------------------------------------------
		public void Init()
		{
			_language = (apEditor.LANGUAGE)EditorPrefs.GetInt("AnyPortrait_Language", (int)apEditor.LANGUAGE.English);

			GetText();
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			width -= 10;

			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
			_scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Width(width + 10), GUILayout.Height(height - 60));
			EditorGUILayout.BeginVertical(GUILayout.Width(width - 15));
			GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
			guiStyle.richText = true;
			guiStyle.wordWrap = true;
			
			EditorGUILayout.TextArea(_info, guiStyle, GUILayout.Width(width - 25));
			GUILayout.Space(height + 500);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();

			bool isClose = false;
			if(GUILayout.Button(_str_GotoHomepage, GUILayout.Height(25)))
			{
				//홈페이지를 엽시다.
				if(_language == apEditor.LANGUAGE.Korean)
				{
					Application.OpenURL("https://www.rainyrizzle.com/anyportrait-updatenote-kor");
				}
				else
				{
					Application.OpenURL("https://www.rainyrizzle.com/anyportrait-updatenote-eng");
				}
			}
			if(GUILayout.Button(_str_Close, GUILayout.Height(25)))
			{
				isClose = true;
			}

			EditorGUILayout.EndVertical();

			if (isClose)
			{
				CloseDialog();
			}
		}


		private void GetText()
		{
			_info = "";

			TextAsset textAsset_Dialog = AssetDatabase.LoadAssetAtPath<TextAsset>(apEditorUtil.ResourcePath_Text + "apUpdateLog.txt");
			string strDelimeter = "-----------------------------------------";
			string[] strInfoPerLanguages = textAsset_Dialog.text.Split(new string[] { strDelimeter }, StringSplitOptions.None);

			if (strInfoPerLanguages.Length < (int)apEditor.LANGUAGE.Polish + 1)
			{
				//개수가 부족한데염
				Debug.Log("UpdateLog 개수 부족");
				return;
			}

			_info = strInfoPerLanguages[(int)_language];
			_info = _info.Replace("\r\n", "\n");
			
			if(EditorGUIUtility.isProSkin)
			{
				_info = _info.Replace("<color=blue>", "<color=yellow>");
				_info = _info.Replace("<color=red>", "<color=lime>");
			}
			if (_info.Length > 0)
			{
				if (_info.Substring(0, 1) == "\n")
				{
					_info = _info.Substring(1);
				}
			}

			//첫줄을 삭제한다. (언어 이름이 써있다.)
			int firstCR = _info.IndexOf("\n");
			_info = _info.Substring(firstCR);

			switch (_language)
			{
				case apEditor.LANGUAGE.English:
					_str_GotoHomepage = "Go to Homepage";
					_str_Close = "Close";
					break;

				case apEditor.LANGUAGE.Korean:
					_str_GotoHomepage = "홈페이지로 가기";
					_str_Close = "닫기";
					break;

				case apEditor.LANGUAGE.French:
					_str_GotoHomepage = "Aller à la page d'accueil";
					_str_Close = "Fermer";
					break;

				case apEditor.LANGUAGE.German:
					_str_GotoHomepage = "Gehe zur Startseite";
					_str_Close = "Schließen";
					break;

				case apEditor.LANGUAGE.Spanish:
					_str_GotoHomepage = "Ir a la página de inicio";
					_str_Close = "Cerca";
					break;

				case apEditor.LANGUAGE.Italian:
					_str_GotoHomepage = "Vai alla pagina principale";
					_str_Close = "Vicino";
					break;

				case apEditor.LANGUAGE.Danish:
					_str_GotoHomepage = "Gå til Hjemmeside";
					_str_Close = "Tæt";
					break;

				case apEditor.LANGUAGE.Japanese:
					_str_GotoHomepage = "ホームページへ";
					_str_Close = "閉じる";
					break;

				case apEditor.LANGUAGE.Chinese_Traditional:
					_str_GotoHomepage = "去首頁";
					_str_Close = "關";
					break;

				case apEditor.LANGUAGE.Chinese_Simplified:
					_str_GotoHomepage = "去首页";
					_str_Close = "关";
					break;

				case apEditor.LANGUAGE.Polish:
					_str_GotoHomepage = "Wróć do strony głównej";
					_str_Close = "Blisko";
					break;
			}

			
		}
	}
}