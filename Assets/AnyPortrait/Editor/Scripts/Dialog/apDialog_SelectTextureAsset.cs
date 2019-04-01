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

	public class apDialog_SelectTextureAsset : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		public delegate void FUNC_SELECT_TEXTUREASSET_RESULT(bool isSuccess, apTextureData targetTextureData, object loadKey, Texture2D resultTexture2D);

		private static apDialog_SelectTextureAsset s_window = null;

		private apEditor _editor = null;
		private apTextureData _targetTextureData = null;
		private object _loadKey = null;
		private FUNC_SELECT_TEXTUREASSET_RESULT _funcResult = null;

		private List<Texture2D> _texture2Ds = new List<Texture2D>();
		private Vector2 _scrollList = new Vector2();
		private Texture2D _curSelectedTexture2D = null;

		private bool _isSearched = false;
		private string _strSearchKeyword = "";



		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apTextureData targetTextureData, FUNC_SELECT_TEXTUREASSET_RESULT funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectTextureAsset), true, "Select Texture2D", true);
			apDialog_SelectTextureAsset curTool = curWindow as apDialog_SelectTextureAsset;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 500;
				int height = 610;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, targetTextureData, loadKey, funcResult);

				return loadKey;
			}
			else
			{
				return null;
			}
		}

		private static void CloseDialog()
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
		public void Init(apEditor editor, apTextureData targetTextureData, object loadKey, FUNC_SELECT_TEXTUREASSET_RESULT funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_targetTextureData = targetTextureData;

			_funcResult = funcResult;

			_curSelectedTexture2D = null;

			_isSearched = false;
			_strSearchKeyword = "";
			RefreshTextureAssets();
		}
		private void RefreshTextureAssets()
		{
			_texture2Ds.Clear();
			string[] guids = AssetDatabase.FindAssets("t:Texture2D");
			for (int i = 0; i < guids.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

				Texture2D textureAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
				if (textureAsset != null)
				{
					if (textureAsset.width <= 64 || textureAsset.height <= 64)
					{
						//너무 작은건 패스한다.
						continue;
					}

					if (_isSearched)
					{
						if (!textureAsset.name.Contains(_strSearchKeyword))
						{
							//검색이 되지 않는다면 패스
							continue;
						}
					}

					_texture2Ds.Add(textureAsset);
				}
			}
			if (_curSelectedTexture2D != null)
			{
				if (!_texture2Ds.Contains(_curSelectedTexture2D))
				{
					_curSelectedTexture2D = null;
				}
			}

		}


		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _funcResult == null)
			{
				return;
			}
			int preferImageWidth = 120;
			int scrollWidth = width - 20;
			int nImagePerRow = (scrollWidth / preferImageWidth);
			if (nImagePerRow < 1)
			{
				nImagePerRow = 1;
			}
			int imageUnitWidth = (scrollWidth / nImagePerRow) - 14;

			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(0, 37, width, height - 90), "");
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();

			GUIStyle guiStyle = new GUIStyle(GUIStyle.none);
			guiStyle.normal.textColor = GUI.skin.label.normal.textColor;

			GUIStyle guiStyle_Center = new GUIStyle(GUIStyle.none);
			guiStyle_Center.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;

			GUILayout.Space(10);
			//EditorGUILayout.LabelField("Select Texture", guiStyle_Center, GUILayout.Width(width), GUILayout.Height(15));//<투명 버튼
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(15));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Search), GUILayout.Width(80));//"Search"
			string prevSearchKeyword = _strSearchKeyword;
			_strSearchKeyword = EditorGUILayout.DelayedTextField(_strSearchKeyword, GUILayout.Width(width - (100 + 110)), GUILayout.Height(15));
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Refresh), GUILayout.Width(100), GUILayout.Height(15)))//"Refresh"
			{
				RefreshTextureAssets();
			}

			if (prevSearchKeyword != _strSearchKeyword)
			{
				if (string.IsNullOrEmpty(_strSearchKeyword))
				{
					_isSearched = false;
				}
				else
				{
					_isSearched = true;
				}

				RefreshTextureAssets();
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(10);

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height - (90 + 10)));

			GUILayout.Space(20);

			//int imageUnitHeight = 200;
			int imageUnitHeight = imageUnitWidth + 30;

			//int scrollWidth = width - 16;
			//int imageUnitWidth = (scrollWidth / 3) - 12;
			for (int iRow = 0; iRow < _texture2Ds.Count; iRow += nImagePerRow)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(scrollWidth), GUILayout.Height(imageUnitHeight + 8));
				for (int iCol = 0; iCol < nImagePerRow; iCol++)
				{
					int iTex = iRow + iCol;
					if (iTex >= _texture2Ds.Count)
					{
						break;
					}


					GUILayout.Space(5);
					EditorGUILayout.BeginVertical(GUILayout.Width(imageUnitWidth), GUILayout.Height(imageUnitHeight));
					DrawTextureUnit(_texture2Ds[iTex], imageUnitWidth, imageUnitHeight);
					EditorGUILayout.EndVertical();

					if (iCol < nImagePerRow - 1)
					{
						GUILayout.Space(2);
					}
				}
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(20);
			}

			GUILayout.Space(height - 90);
			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			bool isClose = false;
			//string strSelectBtn = "Set Texture";
			string strSelectBtn = _editor.GetText(TEXT.DLG_SetTexture);
			if (_curSelectedTexture2D != null)
			{
				if (_curSelectedTexture2D.name.Length < 20)
				{
					//strSelectBtn = "Set [" + _curSelectedTexture2D.name + "]";
					strSelectBtn = string.Format("{0}\n[{1}]", _editor.GetText(TEXT.DLG_Select), _curSelectedTexture2D.name);
				}
				else
				{
					//strSelectBtn = "Set [" + _curSelectedTexture2D.name.Substring(0, 15) + "..]";
					strSelectBtn = string.Format("{0}\n[{1}]", _editor.GetText(TEXT.DLG_Select), _curSelectedTexture2D.name.Substring(0, 20));
				}

			}
			if (GUILayout.Button(strSelectBtn, GUILayout.Height(40), GUILayout.Width(width / 2 - 6)))
			{
				_funcResult(true, _targetTextureData, _loadKey, _curSelectedTexture2D);
				isClose = true;
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(40), GUILayout.Width(width / 2 - 6)))//"Close"
			{
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if (isClose)
			{
				CloseDialog();
			}
		}


		private void DrawTextureUnit(Texture2D texture, int width, int height)
		{
			int btnHeight = 25;
			int imageSlotHeight = height - (btnHeight + 2);

			float baseAspectRatio = (float)width / (float)imageSlotHeight;

			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(imageSlotHeight));
			if (texture == null)
			{
				GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
				guiStyle_Box.alignment = TextAnchor.MiddleCenter;
				guiStyle_Box.normal.textColor = apEditorUtil.BoxTextColor;

				GUILayout.Box("Empty Image", guiStyle_Box, GUILayout.Width(width), GUILayout.Height(imageSlotHeight));
			}
			else
			{
				int imgWidth = texture.width;
				if (imgWidth <= 0)
				{ imgWidth = 1; }

				int imgHeight = texture.height;
				if (imgHeight <= 0)
				{ imgHeight = 1; }

				float aspectRatio = (float)imgWidth / (float)imgHeight;

				//가로를 채울 것인가, 세로를 채울 것인가
				if (aspectRatio > baseAspectRatio)
				{
					//비율상 가로가 더 길다.
					//가로에 맞추고 세로를 줄이자
					imgWidth = width;
					imgHeight = (int)((float)imgWidth / aspectRatio);
				}
				else
				{
					//비율상 세로가 더 길다.
					//세로에 맞추고 가로를 줄이다.
					imgHeight = imageSlotHeight;
					imgWidth = (int)((float)imageSlotHeight * aspectRatio);
				}
				int margin = (imageSlotHeight - imgHeight) / 2;
				if (margin > 0)
				{
					//GUILayout.Space(margin);
				}
				GUIStyle guiStyle_Img = new GUIStyle(GUI.skin.box);
				guiStyle_Img.alignment = TextAnchor.MiddleCenter;

				Color prevColor = GUI.backgroundColor;
				Color boxColor = prevColor;
				if (_curSelectedTexture2D == texture)
				{
					boxColor.r = prevColor.r * 0.8f;
					boxColor.g = prevColor.g * 0.8f;
					boxColor.b = prevColor.b * 1.2f;
				}

				GUI.backgroundColor = boxColor;

				//if(GUILayout.Button(new GUIContent(textureData._image), guiStyle_Img, GUILayout.Width(imgWidth), GUILayout.Height(imgHeight)))
				if (GUILayout.Button(new GUIContent(texture), guiStyle_Img, GUILayout.Width(width), GUILayout.Height(imageSlotHeight)))
				{
					_curSelectedTexture2D = texture;
				}

				GUI.backgroundColor = prevColor;
			}
			EditorGUILayout.EndVertical();

			GUIStyle guiStyle_label = new GUIStyle(GUI.skin.label);
			guiStyle_label.alignment = TextAnchor.MiddleCenter;

			EditorGUILayout.LabelField(texture.name, guiStyle_label, GUILayout.Width(width), GUILayout.Height(20));

			//if(apEditorUtil.ToggledButton(textureData._name, textureData == _curSelectedTextureData, width, btnHeight))
			//{
			//	_curSelectedTextureData = textureData;
			//}
		}
		// 
		//------------------------------------------------------------------
	}


}