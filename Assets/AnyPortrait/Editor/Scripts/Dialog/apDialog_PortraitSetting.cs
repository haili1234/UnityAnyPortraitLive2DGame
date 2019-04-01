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

	public class apDialog_PortraitSetting : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_PortraitSetting s_window = null;

		private apEditor _editor = null;
		private apPortrait _targetPortrait = null;
		//private object _loadKey = null;


		private enum TAB
		{
			PortriatSetting,
			EditorSetting,
			About
		}

		private TAB _tab = TAB.PortriatSetting;
		private Vector2 _scroll = Vector2.zero;
		

		private string[] _strLanguageName = new string[]
		{
			"English",//"English" 0
			"한국어",//Korean 1
			"Français",//French 2
			"Deutsch",//German 3
			"Español",//Spanish 4
			"Dansk",//Danish 6
			"日本語",//Japanese 7
			"繁體中文",//Chinese_Traditional 8
			"簡體中文",//Chinese_Simplified 9
			"Italiano",//Italian 5 -> 현재 미지원
			"Polski",//Polish 10 -> 현재 미지원

		};

		//실제로 지원하는 언어 인덱스를 적는다.
		//0 -> 0 (English) 이런 방식
		//현재 Italian (5), Polish (10) 제외됨
		private int[] _validLanguageIndex = new int[]
		{
			0,	//English
			1,	//Korean
			2,	//French
			3,	//German
			4,	//Spanish
			6,	//Danish
			7,	//Japanese
			8,	//Chinese-Trad
			9,	//Chinese-Simp
			5,	//Italian
			10,	//Polish
		};


		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apPortrait portrait)
		{
			//Debug.Log("Show Dialog - Portrait Setting");
			CloseDialog();


			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_PortraitSetting), true, "Setting", true);
			apDialog_PortraitSetting curTool = curWindow as apDialog_PortraitSetting;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 400;
				int height = 500;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);


				s_window.Init(editor, portrait, loadKey);

				return loadKey;
			}
			else
			{
				return null;
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
		public void Init(apEditor editor, apPortrait portrait, object loadKey)
		{
			_editor = editor;
			//_loadKey = loadKey;
			_targetPortrait = portrait;
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _targetPortrait == null)
			{
				//Debug.LogError("Exit - Editor / Portrait is Null");
				CloseDialog();
				return;
			}

			//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
			if (_editor != apEditor.CurrentEditor || _targetPortrait != apEditor.CurrentEditor._portrait)
			{
				//Debug.LogError("Exit - Editor / Portrait Missmatch");
				CloseDialog();
				return;

			}

			int tabBtnHeight = 25;
			int tabBtnWidth = ((width - 10) / 3) - 4;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(tabBtnHeight));
			GUILayout.Space(5);
			if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Portrait), _tab == TAB.PortriatSetting, tabBtnWidth, tabBtnHeight))//"Portrait"
			{
				_tab = TAB.PortriatSetting;
			}
			if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Editor), _tab == TAB.EditorSetting, tabBtnWidth, tabBtnHeight))//"Editor"
			{
				_tab = TAB.EditorSetting;
			}
			if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_About), _tab == TAB.About, tabBtnWidth, tabBtnHeight))//"About"
			{
				_tab = TAB.About;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			int scrollHeight = height - 40;
			_scroll = EditorGUILayout.BeginScrollView(_scroll, false, true, GUILayout.Width(width), GUILayout.Height(scrollHeight));
			width -= 25;
			GUILayout.BeginVertical(GUILayout.Width(width));

			switch (_tab)
			{
				case TAB.PortriatSetting:
					{
						//Portrait 설정
						EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PortraitSetting));//"Portrait Settings"
						GUILayout.Space(10);
						string nextName = EditorGUILayout.DelayedTextField(_editor.GetText(TEXT.DLG_Name), _targetPortrait.name);//"Name"
						if (nextName != _targetPortrait.name)
						{
							apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
							_targetPortrait.name = nextName;
							
						}

						//"FPS (Important Off)"
						int nextFPS = EditorGUILayout.DelayedIntField(new GUIContent(_editor.GetText(TEXT.DLG_Setting_FPS), "This setting is used when <Important> is off"), _targetPortrait._FPS);
						if (_targetPortrait._FPS != nextFPS)
						{
							apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
							if (nextFPS < 10)
							{
								nextFPS = 10;
							}
							_targetPortrait._FPS = nextFPS;
						}

						//"Is Important"
						bool nextImportant = EditorGUILayout.Toggle(new GUIContent(_editor.GetText(TEXT.DLG_Setting_IsImportant), "When this setting is on, it always updates and the physics effect works."), _targetPortrait._isImportant);
						if(nextImportant != _targetPortrait._isImportant)
						{
							apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
							_targetPortrait._isImportant = nextImportant;
						}


						GUILayout.Space(10);
						//수동으로 백업하기
						if(GUILayout.Button(_editor.GetText(TEXT.DLG_Setting_ManualBackUp), GUILayout.Height(30)))//"Save Backup (Manual)"
						{
							if (_editor.Backup.IsAutoSaveWorking())
							{
								EditorUtility.DisplayDialog(_editor.GetText(TEXT.BackupError_Title),
															_editor.GetText(TEXT.BackupError_Body),
															_editor.GetText(TEXT.Okay));
							}
							else
							{
								string defaultBackupFileName = _targetPortrait.name + "_backup_" + apBackup.GetCurrentTimeString();
								string savePath = EditorUtility.SaveFilePanel("Backup File Path", "", defaultBackupFileName, "bck");
								if (string.IsNullOrEmpty(savePath))
								{
									_editor.Notification("Backup Canceled", true, false);
								}
								else
								{
									_editor.Backup.SaveBackup(savePath, _targetPortrait);
									_editor.Notification("Backup Saved [" + savePath + "]", false, true);
								}

								CloseDialog();
							}
						}

					}
					break;

				case TAB.EditorSetting:
					{
						EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_EditorSetting));//"Editor Settings"
						GUILayout.Space(10);

						apEditor.LANGUAGE prevLanguage = _editor._language;
						int prevLangIndex = -1;
						for (int i = 0; i < _validLanguageIndex.Length; i++)
						{
							if(_validLanguageIndex[i] == (int)prevLanguage)
							{
								prevLangIndex = i;
							}
						}
						if(prevLangIndex < 0)
						{
							prevLangIndex = 0;//English 강제
						}

						bool prevGUIFPS = _editor._guiOption_isFPSVisible;
						bool prevGUIStatistics = _editor._guiOption_isStatisticsVisible;

						Color prevColor_Background = _editor._colorOption_Background;
						Color prevColor_GridCenter = _editor._colorOption_GridCenter;
						Color prevColor_Grid = _editor._colorOption_Grid;

						Color prevColor_MeshEdge = _editor._colorOption_MeshEdge;
						Color prevColor_MeshHiddenEdge = _editor._colorOption_MeshHiddenEdge;
						Color prevColor_Outline = _editor._colorOption_Outline;
						Color prevColor_TFBorder = _editor._colorOption_TransformBorder;
						Color prevColor_VertNotSelected = _editor._colorOption_VertColor_NotSelected;
						Color prevColor_VertSelected = _editor._colorOption_VertColor_Selected;

						Color prevColor_GizmoFFDLine = _editor._colorOption_GizmoFFDLine;
						Color prevColor_GizmoFFDInnerLine = _editor._colorOption_GizmoFFDInnerLine;

						Color prevColor_ToneColor = _editor._colorOption_OnionToneColor;

						bool prevBackup_IsAutoSave = _editor._backupOption_IsAutoSave;
						string prevBackup_Path = _editor._backupOption_BaseFolderName;
						int prevBackup_Time = _editor._backupOption_Minute;


						string prevBonePose_Path = _editor._bonePose_BaseFolderName;

						//"Language"
						//이전 방식은 Enum을 모두 조회
						//_editor._language = (apEditor.LANGUAGE)EditorGUILayout.Popup(_editor.GetText(TEXT.DLG_Setting_Language), (int)_editor._language, _strLanguageName);

						//사용 가능한 Language만 따로 조회
						int nextLangIndex = EditorGUILayout.Popup(_editor.GetText(TEXT.DLG_Setting_Language), prevLangIndex, _strLanguageName);
						_editor._language = (apEditor.LANGUAGE)_validLanguageIndex[nextLangIndex];

						GUILayout.Space(10);
						_editor._guiOption_isFPSVisible = EditorGUILayout.Toggle(_editor.GetText(TEXT.DLG_Setting_ShowFPS), _editor._guiOption_isFPSVisible);//"Show FPS"
						_editor._guiOption_isStatisticsVisible = EditorGUILayout.Toggle(_editor.GetText(TEXT.DLG_Setting_ShowStatistics), _editor._guiOption_isStatisticsVisible);// "Show Statistics"


						GUILayout.Space(10);
						EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_AutoBackupSetting));//"Auto Backup Option"
						_editor._backupOption_IsAutoSave = EditorGUILayout.Toggle(_editor.GetText(TEXT.DLG_Setting_AutoBackup), _editor._backupOption_IsAutoSave);//"Auto Backup"

						if (_editor._backupOption_IsAutoSave)
						{
							//경로와 시간
							//"Time (Min)"
							_editor._backupOption_Minute = EditorGUILayout.IntField(_editor.GetText(TEXT.DLG_Setting_BackupTime), _editor._backupOption_Minute, GUILayout.Width(width));

							EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(18));
							GUILayout.Space(5);
							//"Save Path"
							_editor._backupOption_BaseFolderName = EditorGUILayout.TextField(_editor.GetText(TEXT.DLG_Setting_BackupPath), _editor._backupOption_BaseFolderName, GUILayout.Width(width - 100), GUILayout.Height(18));
							if(GUILayout.Button(_editor.GetText(TEXT.DLG_Change), GUILayout.Width(90), GUILayout.Height(18)))//"Change"
							{
								string pathResult = EditorUtility.SaveFolderPanel("Set the Backup Folder", _editor._backupOption_BaseFolderName, "");
								if(!string.IsNullOrEmpty(pathResult))
								{
									//Debug.Log("백업 폴더 경로 [" + pathResult + "] - " + Application.dataPath);
									Uri targetUri = new Uri(pathResult);
									Uri baseUri = new Uri(Application.dataPath);

									string relativePath = baseUri.MakeRelativeUri(targetUri).ToString();
									_editor._backupOption_BaseFolderName = relativePath;
									//Debug.Log("상대 경로 [" + relativePath + "]");
									apEditorUtil.SetEditorDirty();

								}
							}
							EditorGUILayout.EndHorizontal();
						}

						GUILayout.Space(10);
						EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_PoseSnapshotSetting));//"Pose Snapshot Option"
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(18));
						GUILayout.Space(5);
						//"Save Path"
						_editor._bonePose_BaseFolderName = EditorGUILayout.TextField(_editor.GetText(TEXT.DLG_Setting_BackupPath), _editor._bonePose_BaseFolderName, GUILayout.Width(width - 100), GUILayout.Height(18));

						if (GUILayout.Button(_editor.GetText(TEXT.DLG_Change), GUILayout.Width(90), GUILayout.Height(18)))//"Change"
						{
							string pathResult = EditorUtility.SaveFolderPanel("Set the Pose Folder", _editor._bonePose_BaseFolderName, "");
							if (!string.IsNullOrEmpty(pathResult))
							{
								Uri targetUri = new Uri(pathResult);
								Uri baseUri = new Uri(Application.dataPath);

								string relativePath = baseUri.MakeRelativeUri(targetUri).ToString();

								_editor._bonePose_BaseFolderName = relativePath;

								apEditorUtil.SetEditorDirty();

							}
						}
						EditorGUILayout.EndHorizontal();
						



						GUILayout.Space(10);
						try
						{
							//int width_Btn = 65;
							//int width_Color = width - (width_Btn + 8);

							//int height_Color = 18;
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_BackgroundColors));//"Background Colors"
							
							//"Background"
							_editor._colorOption_Background = ColorUI(_editor.GetText(TEXT.DLG_Setting_Background), _editor._colorOption_Background, width, apEditor.DefaultColor_Background);

							//"Grid Center"
							_editor._colorOption_GridCenter = ColorUI(_editor.GetText(TEXT.DLG_Setting_GridCenter), _editor._colorOption_GridCenter, width, apEditor.DefaultColor_GridCenter);

							//"Grid"
							_editor._colorOption_Grid = ColorUI(_editor.GetText(TEXT.DLG_Setting_Grid), _editor._colorOption_Grid, width, apEditor.DefaultColor_Grid);

							//"Atlas Border"
							_editor._colorOption_AtlasBorder = ColorUI(_editor.GetText(TEXT.DLG_Setting_AtlasBorder), _editor._colorOption_AtlasBorder, width, apEditor.DefaultColor_AtlasBorder);


							GUILayout.Space(5);
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_MeshGUIColors));//"Mesh GUI Colors"
							
							//"Mesh Edge"
							_editor._colorOption_MeshEdge = ColorUI(_editor.GetText(TEXT.DLG_Setting_MeshEdge), _editor._colorOption_MeshEdge, width, apEditor.DefaultColor_MeshEdge);

							//"Mesh Hidden Edge"
							_editor._colorOption_MeshHiddenEdge = ColorUI(_editor.GetText(TEXT.DLG_Setting_MeshHiddenEdge), _editor._colorOption_MeshHiddenEdge, width, apEditor.DefaultColor_MeshHiddenEdge);

							//"Outline"
							_editor._colorOption_Outline = ColorUI(_editor.GetText(TEXT.DLG_Setting_Outline), _editor._colorOption_Outline, width, apEditor.DefaultColor_Outline);

							//"Transform Border"
							_editor._colorOption_TransformBorder = ColorUI(_editor.GetText(TEXT.DLG_Setting_TransformBorder), _editor._colorOption_TransformBorder, width, apEditor.DefaultColor_TransformBorder);

							//"Vertex"
							_editor._colorOption_VertColor_NotSelected = ColorUI(_editor.GetText(TEXT.DLG_Setting_Vertex), _editor._colorOption_VertColor_NotSelected, width, apEditor.DefaultColor_VertNotSelected);

							//"Selected Vertex"
							_editor._colorOption_VertColor_Selected = ColorUI(_editor.GetText(TEXT.DLG_Setting_SelectedVertex), _editor._colorOption_VertColor_Selected, width, apEditor.DefaultColor_VertSelected);


							GUILayout.Space(5);
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_GizmoColors));//"Gizmo Colors"
							
							//"FFD Line"
							_editor._colorOption_GizmoFFDLine = ColorUI(_editor.GetText(TEXT.DLG_Setting_FFDLine), _editor._colorOption_GizmoFFDLine, width, apEditor.DefaultColor_GizmoFFDLine);

							//"FFD Inner Line"
							_editor._colorOption_GizmoFFDInnerLine = ColorUI(_editor.GetText(TEXT.DLG_Setting_FFDInnerLine), _editor._colorOption_GizmoFFDInnerLine, width, apEditor.DefaultColor_GizmoFFDInnerLine);

							GUILayout.Space(5);
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_OnionSkinColor));//"Onion Skin Color"

							//"Onion Skin (2x)"
							_editor._colorOption_OnionToneColor = ColorUI(_editor.GetText(TEXT.DLG_Setting_OnionSkinColor2X), _editor._colorOption_OnionToneColor, width, apEditor.DefaultColor_OnionToneColor);


						}
						catch (Exception)
						{

						}

						GUILayout.Space(10);
						bool prevStartupScreen = _editor._startScreenOption_IsShowStartup;
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
						GUILayout.Space(5);
						EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_StartPage_AlawysOn), GUILayout.Width((width - (10 + 30))));
						_editor._startScreenOption_IsShowStartup = EditorGUILayout.Toggle(_editor._startScreenOption_IsShowStartup, GUILayout.Width(30));
						EditorGUILayout.EndHorizontal();

						GUILayout.Space(20);
						
						//"Restore Editor Default Setting"
						if (GUILayout.Button(_editor.GetText(TEXT.DLG_Setting_RestoreDefaultSetting), GUILayout.Height(20)))
						{
							_editor.RestoreEditorPref();
						}


						if (prevLanguage != _editor._language ||
							prevGUIFPS != _editor._guiOption_isFPSVisible ||
							prevGUIStatistics != _editor._guiOption_isStatisticsVisible ||
							prevColor_Background != _editor._colorOption_Background ||
							prevColor_GridCenter != _editor._colorOption_GridCenter ||
							prevColor_Grid != _editor._colorOption_Grid ||

							prevColor_MeshEdge != _editor._colorOption_MeshEdge ||
							prevColor_MeshHiddenEdge != _editor._colorOption_MeshHiddenEdge ||
							prevColor_Outline != _editor._colorOption_Outline ||
							prevColor_TFBorder != _editor._colorOption_TransformBorder ||
							prevColor_VertNotSelected != _editor._colorOption_VertColor_NotSelected ||
							prevColor_VertSelected != _editor._colorOption_VertColor_Selected ||

							prevColor_GizmoFFDLine != _editor._colorOption_GizmoFFDLine ||
							prevColor_GizmoFFDInnerLine != _editor._colorOption_GizmoFFDInnerLine ||
							prevColor_ToneColor != _editor._colorOption_OnionToneColor ||
							prevBackup_IsAutoSave != _editor._backupOption_IsAutoSave ||
							!prevBackup_Path.Equals(_editor._backupOption_BaseFolderName) ||
							prevBackup_Time != _editor._backupOption_Minute ||
							!prevBonePose_Path.Equals(_editor._bonePose_BaseFolderName) ||
							
							prevStartupScreen != _editor._startScreenOption_IsShowStartup
								)
						{
							bool isLanguageChanged = (prevLanguage != _editor._language);

							_editor.SaveEditorPref();
							apEditorUtil.SetEditorDirty();

							apGL.SetToneColor(_editor._colorOption_OnionToneColor);

							if(isLanguageChanged)
							{
								
								_editor.ResetHierarchyAll();
								_editor.RefreshTimelineLayers(true);
								_editor.RefreshControllerAndHierarchy();
							}
						}
					}
					break;

				case TAB.About:
					{
						EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_About));//"About"

						GUILayout.Space(20);
						apEditorUtil.GUI_DelimeterBoxH(width);
						GUILayout.Space(20);

						EditorGUILayout.LabelField("[AnyPortrait]");
						EditorGUILayout.LabelField("Build : " + apVersion.I.APP_VERSION_NUMBER_ONLY);
						


						GUILayout.Space(20);
						apEditorUtil.GUI_DelimeterBoxH(width);
						GUILayout.Space(20);

						EditorGUILayout.LabelField("[Open Source Library License]");
						GUILayout.Space(20);

						EditorGUILayout.LabelField("[PSD File Import Library]");
						GUILayout.Space(10);
						EditorGUILayout.LabelField("Ntreev Photoshop Document Parser for .Net");
						GUILayout.Space(10);

						EditorGUILayout.LabelField("Released under the MIT License.");
						GUILayout.Space(10);

						EditorGUILayout.LabelField("Copyright (c) 2015 Ntreev Soft co., Ltd.");
						GUILayout.Space(10);

						EditorGUILayout.LabelField("Permission is hereby granted, free of charge,");
						EditorGUILayout.LabelField("to any person obtaining a copy of this software");
						EditorGUILayout.LabelField("and associated documentation files (the \"Software\"),");
						EditorGUILayout.LabelField("to deal in the Software without restriction,");
						EditorGUILayout.LabelField("including without limitation the rights ");
						EditorGUILayout.LabelField("to use, copy, modify, merge, publish, distribute,");
						EditorGUILayout.LabelField("sublicense, and/or sell copies of the Software, ");
						EditorGUILayout.LabelField("and to permit persons to whom the Software is furnished");
						EditorGUILayout.LabelField("to do so, subject to the following conditions:");
						GUILayout.Space(10);

						EditorGUILayout.LabelField("The above copyright notice and ");
						EditorGUILayout.LabelField("this permission notice shall be included");
						EditorGUILayout.LabelField("in all copies or substantial portions of the Software.");
						GUILayout.Space(10);

						EditorGUILayout.LabelField("THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT ");
						EditorGUILayout.LabelField("WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, ");
						EditorGUILayout.LabelField("INCLUDING BUT NOT LIMITED TO THE WARRANTIES ");
						EditorGUILayout.LabelField("OF MERCHANTABILITY, FITNESS FOR A PARTICULAR ");
						EditorGUILayout.LabelField("PURPOSE AND NONINFRINGEMENT. ");
						EditorGUILayout.LabelField("IN NO EVENT SHALL THE AUTHORS OR ");
						EditorGUILayout.LabelField("COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES ");
						EditorGUILayout.LabelField("OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, ");
						EditorGUILayout.LabelField("TORT OR OTHERWISE, ARISING FROM, OUT OF OR ");
						EditorGUILayout.LabelField("IN CONNECTION WITH THE SOFTWARE OR ");
						EditorGUILayout.LabelField("THE USE OR OTHER DEALINGS IN THE SOFTWARE.");

						GUILayout.Space(20);
						apEditorUtil.GUI_DelimeterBoxH(width);
						GUILayout.Space(20);

						EditorGUILayout.LabelField("[GIF Export Library]");
						GUILayout.Space(10);
						EditorGUILayout.LabelField("NGif, Animated GIF Encoder for .NET");
						GUILayout.Space(10);
						EditorGUILayout.LabelField("Released under the CPOL 1.02.");
						GUILayout.Space(10);



					}
					break;
			}



			GUILayout.Space(height);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
		}


		private Color ColorUI(string label, Color srcColor, int width, Color defaultColor)
		{
			int width_Btn = 65;
			int width_Color = width - (width_Btn + 8);

			int height_Color = 18;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Color));
			GUILayout.Space(5);
			Color result = srcColor;
			try
			{
				result = EditorGUILayout.ColorField(label, srcColor, GUILayout.Width(width_Color), GUILayout.Height(height_Color));
			}
			catch (Exception)
			{
			}

			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Default), GUILayout.Width(width_Btn), GUILayout.Height(height_Color)))//"Default"
			{
				result = defaultColor;
			}
			EditorGUILayout.EndHorizontal();
			return result;
		}


	}


}