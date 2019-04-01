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
	public class apDialog_AnimationEvents : EditorWindow
	{
		// Members
		//--------------------------------------------------------------
		private static apDialog_AnimationEvents s_window = null;
		
		private apEditor _editor = null;
		private apPortrait _portrait = null;
		private apAnimClip _animClip = null;
		
		private Vector2 _scrollList = new Vector2();
		private Vector2 _scrollList_Param = new Vector2();
		private apAnimEvent _curSelectedEvent = null;

		private int _prevNumSubParams = -1;
		private int _defaultFrame = -1;

		

		// Show Window
		//--------------------------------------------------------------
		public static void ShowDialog(apEditor editor, apPortrait portrait, apAnimClip animClip)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_AnimationEvents), true, "Animation Events", true);
			apDialog_AnimationEvents curTool = curWindow as apDialog_AnimationEvents;

			if (curTool != null && curTool != s_window)
			{
				int width = 500;
				int height = 700;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, portrait, animClip);
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
		//--------------------------------------------------------------
		public void Init(apEditor editor, apPortrait portrait, apAnimClip animClip)
		{
			_editor = editor;
			_portrait = portrait;
			_animClip = animClip;

			_curSelectedEvent = null;
			_defaultFrame = animClip.CurFrame;

			
		}


		// GUI
		//--------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _editor._portrait == null || _editor._portrait != _portrait)
			{
				return;
			}

			bool isGUIEvent = (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout);
			

			EditorGUILayout.BeginVertical();

			Texture2D iconImageAnimation = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation);
			Texture2D iconImageCategory = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
			Texture2D iconImageAddParam = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Add);
			Texture2D iconImageLayerUp = _editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerUp);
			Texture2D iconImageLayerDown = _editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerDown);
			Texture2D iconImageRemove = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform);

			GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
			guiStyleBox.alignment = TextAnchor.MiddleCenter;
			guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;
			
			//"  [ " + _animClip._name + " ] Animation Events"
			GUILayout.Box(new GUIContent(string.Format("  [ {0} ] {1}", _animClip._name, _editor.GetText(TEXT.DLG_AnimationEvents)), iconImageAnimation), guiStyleBox, GUILayout.Width(width - 10), GUILayout.Height(35));

			GUILayout.Space(5);
			//"Range : " + _animClip.StartFrame + " ~ " + _animClip.EndFrame
			EditorGUILayout.LabelField(string.Format("{0} : {1} ~ {2}", _editor.GetText(TEXT.DLG_Range), _animClip.StartFrame, _animClip.EndFrame));

			//"Is Loop Animation : " + _animClip.IsLoop
			EditorGUILayout.LabelField(string.Format("{0} : {1}", _editor.GetText(TEXT.DLG_IsLoopAnimation), _animClip.IsLoop));

			GUILayout.Space(10);

			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_None.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)
			{
				guiStyle_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Selected.normal.textColor = Color.white;
			}
			guiStyle_Selected.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Center = new GUIStyle(GUIStyle.none);
			guiStyle_Center.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;
			

			int topHeight = 120;
			int bottomHeight = 380;
			

			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(0, topHeight - 26, width, height - (topHeight + bottomHeight)), "");
			GUI.backgroundColor = prevColor;

			
			
			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height - (topHeight + bottomHeight)));
			GUILayout.Space(5);
			//"Animation Events"
			GUILayout.Button(new GUIContent(_editor.GetText(TEXT.DLG_AnimationEvents), iconImageCategory), guiStyle_None, GUILayout.Height(20));//<투명 버튼//

			int nAnimEvents = 0;
			if(_animClip._animEvents != null)
			{
				nAnimEvents = _animClip._animEvents.Count;
			}

			//GUILayout.Space(10);
			apAnimEvent animEvent = null;
			for (int i = 0; i < nAnimEvents; i++)
			{
				GUIStyle curGUIStyle = guiStyle_None;
				animEvent = _animClip._animEvents[i];

				if (animEvent == _curSelectedEvent)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();
					prevColor = GUI.backgroundColor;

					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					}

					GUI.Box(new Rect(lastRect.x, lastRect.y + 21, width, 20), "");
					GUI.backgroundColor = prevColor;

					curGUIStyle = guiStyle_Selected;
				}


				EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
				GUILayout.Space(15);
				if (GUILayout.Button("[" + i + "] " + animEvent._eventName, curGUIStyle, GUILayout.Width(width - 80), GUILayout.Height(20)))
				{
					_curSelectedEvent = animEvent;
				}

				if (animEvent._callType == apAnimEvent.CALL_TYPE.Once)
				{
					if(GUILayout.Button(animEvent._frameIndex.ToString(), curGUIStyle, GUILayout.Width(45), GUILayout.Height(20)))
					{
						_curSelectedEvent = animEvent;
					}
				}
				else
				{
					if(GUILayout.Button(animEvent._frameIndex + " ~ " + animEvent._frameIndex_End, curGUIStyle, GUILayout.Width(45), GUILayout.Height(20)))
					{
						_curSelectedEvent = animEvent;
					}
				}

				EditorGUILayout.EndHorizontal();
			}

			GUILayout.Space((height - (topHeight + bottomHeight)) + 100);
			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal(GUILayout.Height(30));
			GUILayout.Space(5);

			//"Add Event"
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_AddEvent), GUILayout.Width(width - (10 + 4 + 80)), GUILayout.Height(30)))
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_AddEvent, _editor, _animClip._portrait, null, false);

				if(_animClip._animEvents == null)
				{
					_animClip._animEvents = new List<apAnimEvent>();
				}

				apAnimEvent newEvent = new apAnimEvent();
				//새로운 이름을 찾자
				int iNewName = 0;
				string newName = "NewAnimEvent_" + iNewName;

				int cnt = 0;
				while(true)
				{
					if(cnt > 500)
					{
						newName = "NewAnimEvent_Infinity";
						break;
					}
					//중복되는 이름이 있는가
					newName = "NewAnimEvent_" + iNewName;
					bool isExist = _animClip._animEvents.Exists(delegate (apAnimEvent a)
					{
						return string.Equals(a._eventName, newName);
					});
					if(!isExist)
					{
						//중복되지 않는 이름이다.
						break;
					}

					//이름이 중복되는 군염
					cnt++;
					iNewName++;
				}

				newEvent._eventName = newName;
				newEvent._frameIndex = _defaultFrame;

				_animClip._animEvents.Add(newEvent);

				_curSelectedEvent = newEvent;

			}
			//"Sort"
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_Sort), GUILayout.Width(80), GUILayout.Height(30)))
			{
				//프레임 순으로 정렬을 한다.
				if (_animClip._animEvents != null)
				{
					_animClip._animEvents.Sort(delegate (apAnimEvent a, apAnimEvent b)
					{
						if(a._frameIndex == b._frameIndex)
						{
							return string.Compare(a._eventName, b._eventName);
						}
						return a._frameIndex - b._frameIndex;
					});
				}
			}
			EditorGUILayout.EndHorizontal();

			//선택된 AnimEvent에 대한 설정을 하자
			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width - 10);
			GUILayout.Space(5);

			//선택이 안되었다면 더미 데이터로 채워야함
			int curFrameIndex = 0;
			int curFrameIndex_End = 0;
			string curName = "<None>";
			apAnimEvent.CALL_TYPE curCallType = apAnimEvent.CALL_TYPE.Once;
			List<apAnimEvent.SubParameter> curSubParams = null;
			int curNumSubParams = 0;

			bool isSelected = _curSelectedEvent != null && _animClip._animEvents.Contains(_curSelectedEvent);

			if (isSelected)
			{
				curFrameIndex = _curSelectedEvent._frameIndex;
				curFrameIndex_End = _curSelectedEvent._frameIndex_End;
				curName = _curSelectedEvent._eventName;
				curCallType = _curSelectedEvent._callType;
				curSubParams = _curSelectedEvent._subParams;
				curNumSubParams = curSubParams.Count;
			}

			if(isSelected)
			{
				GUI.backgroundColor = new Color(0.6f, 0.8f, 0.9f, 1.0f);
			}
			
			//"(Not Selected)"
			GUILayout.Box((isSelected) ? curName : "(" + _editor.GetText(TEXT.DLG_NotSelected) + ")", guiStyleBox, GUILayout.Width(width - 10), GUILayout.Height(25));

			GUI.backgroundColor = prevColor;

			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(GUILayout.Height(90));
			curName = EditorGUILayout.DelayedTextField(_editor.GetText(TEXT.DLG_EventName), curName);//"Event(Function) Name"
			curCallType = (apAnimEvent.CALL_TYPE)EditorGUILayout.EnumPopup(_editor.GetText(TEXT.DLG_CallMethod), curCallType);//"Call Method"
			if (curCallType == apAnimEvent.CALL_TYPE.Once)
			{
				curFrameIndex = EditorGUILayout.DelayedIntField(_editor.GetText(TEXT.DLG_TargetFrame), curFrameIndex);//"Target Frame"
			}
			else
			{
				curFrameIndex = EditorGUILayout.DelayedIntField(_editor.GetText(TEXT.DLG_StartFrame), curFrameIndex);//"Start Frame"
				curFrameIndex_End = EditorGUILayout.DelayedIntField(_editor.GetText(TEXT.DLG_EndFrame), curFrameIndex_End);//"End Frame"
			}
			EditorGUILayout.EndVertical();

			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Parameters));//"Parameters"
			GUILayout.Space(5);


			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(0, (height - (bottomHeight - 171)) + 22, width, 100), "");
			GUI.backgroundColor = prevColor;

			if(!isSelected)
			{
				if(!isGUIEvent)
				{
					curNumSubParams = _prevNumSubParams;
				}
			}

			_scrollList_Param = EditorGUILayout.BeginScrollView(_scrollList_Param, GUILayout.Width(width), GUILayout.Height(100));

			GUILayout.Space(5);
			int valueWidth = width - (10 + 35 + 130 + 36 + 36 + 36 + 20);
			int valueHalfWidth = ((valueWidth / 2) - 10);
			int midWaveWidth = 20;
			valueWidth += 7;

			GUIStyle guiStyleListBtn = new GUIStyle(GUI.skin.button);
			guiStyleListBtn.margin = GUI.skin.textField.margin;

			apAnimEvent.SubParameter targetSubParam = null;
			bool isLayerUp = false;
			bool isLayerDown = false;
			bool isRemoveParam = false;

			//SubParam 리스트를 출력하자
			if(curNumSubParams > 0)
			{
				if (isSelected && curSubParams != null && curSubParams.Count == curNumSubParams)
				{
					for (int i = 0; i < curNumSubParams; i++)
					{
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50), GUILayout.Height(24));
						GUILayout.Space(15);
						GUILayout.Label("[" + i + "]", GUILayout.Width(30), GUILayout.Height(20));

						curSubParams[i]._paramType = (apAnimEvent.PARAM_TYPE)EditorGUILayout.EnumPopup(curSubParams[i]._paramType, GUILayout.Width(120), GUILayout.Height(20));

						switch (curSubParams[i]._paramType)
						{
							case apAnimEvent.PARAM_TYPE.Bool:
								{	
									bool nextValue = EditorGUILayout.Toggle(curSubParams[i]._boolValue, GUILayout.Width(valueWidth));

									if(curSubParams[i]._boolValue != nextValue)
									{
										apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged, _editor, _animClip._portrait, null, false);
										curSubParams[i]._boolValue = nextValue;
									}
									
								}
								break;

							case apAnimEvent.PARAM_TYPE.Integer:
								{
									if (curCallType == apAnimEvent.CALL_TYPE.Once)
									{
										int nextValue = EditorGUILayout.DelayedIntField(curSubParams[i]._intValue, GUILayout.Width(valueWidth));

										if (curSubParams[i]._intValue != nextValue)
										{
											apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged, _editor, _animClip._portrait, null, false);
											curSubParams[i]._intValue = nextValue;
										}
									}
									else
									{
										int nextValue_Prev = EditorGUILayout.DelayedIntField(curSubParams[i]._intValue, GUILayout.Width(valueHalfWidth));
										EditorGUILayout.LabelField(" ~ ", GUILayout.Width(midWaveWidth));
										int nextValue_Next = EditorGUILayout.DelayedIntField(curSubParams[i]._intValue_End, GUILayout.Width(valueHalfWidth));

										if(curSubParams[i]._intValue != nextValue_Prev || 
											curSubParams[i]._intValue_End != nextValue_Next)
										{
											apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged, _editor, _animClip._portrait, null, false);
											curSubParams[i]._intValue = nextValue_Prev;
											curSubParams[i]._intValue_End = nextValue_Next;
										}
									}
								}
								break;

							case apAnimEvent.PARAM_TYPE.Float:
								{
									if (curCallType == apAnimEvent.CALL_TYPE.Once)
									{
										float nextValue = EditorGUILayout.DelayedFloatField(curSubParams[i]._floatValue, GUILayout.Width(valueWidth));
										if(curSubParams[i]._floatValue != nextValue)
										{
											apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged, _editor, _animClip._portrait, null, false);
											curSubParams[i]._floatValue = nextValue;
										}
										
									}
									else
									{
										float nextValue_Prev = EditorGUILayout.DelayedFloatField(curSubParams[i]._floatValue, GUILayout.Width(valueHalfWidth));
										EditorGUILayout.LabelField(" ~ ", GUILayout.Width(midWaveWidth));
										float nextVelue_Next = EditorGUILayout.DelayedFloatField(curSubParams[i]._floatValue_End, GUILayout.Width(valueHalfWidth));

										if (curSubParams[i]._floatValue != nextValue_Prev ||
											curSubParams[i]._floatValue_End != nextVelue_Next)
										{
											apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged, _editor, _animClip._portrait, null, false);

											curSubParams[i]._floatValue = nextValue_Prev;
											curSubParams[i]._floatValue_End = nextVelue_Next;
										}
										
									}
								}
								break;

							case apAnimEvent.PARAM_TYPE.Vector2:
								{
									
									if (curCallType == apAnimEvent.CALL_TYPE.Once)
									{
										Vector2 nextValue = apEditorUtil.DelayedVector2Field(curSubParams[i]._vec2Value, valueWidth);
										if(curSubParams[i]._vec2Value.x != nextValue.x ||
											curSubParams[i]._vec2Value.y != nextValue.y)
										{
											apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged, _editor, _animClip._portrait, null, false);
											curSubParams[i]._vec2Value = nextValue;
										}
									}
									else
									{
										Vector2 nextValue_Prev = apEditorUtil.DelayedVector2Field(curSubParams[i]._vec2Value, valueHalfWidth);
										EditorGUILayout.LabelField(" ~ ", GUILayout.Width(midWaveWidth));
										Vector2 nextValue_Next = apEditorUtil.DelayedVector2Field(curSubParams[i]._vec2Value_End, valueHalfWidth);


										if(curSubParams[i]._vec2Value.x != nextValue_Prev.x ||
											curSubParams[i]._vec2Value.y != nextValue_Prev.y ||
											curSubParams[i]._vec2Value_End.x != nextValue_Next.x ||
											curSubParams[i]._vec2Value_End.y != nextValue_Next.y)
										{
											apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged, _editor, _animClip._portrait, null, false);

											curSubParams[i]._vec2Value = nextValue_Prev;
											curSubParams[i]._vec2Value_End = nextValue_Next;
										}
										
										
									}
								}
								break;

							case apAnimEvent.PARAM_TYPE.String:
								{
									string nextValue = EditorGUILayout.DelayedTextField(curSubParams[i]._strValue, GUILayout.Width(valueWidth));
									if(!string.Equals(curSubParams[i]._strValue, nextValue))
									{
										apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged, _editor, _animClip._portrait, null, false);
										curSubParams[i]._strValue = nextValue;
									} 
								}
								break;
						}

						if(GUILayout.Button(iconImageLayerUp, guiStyleListBtn, GUILayout.Width(30), GUILayout.Height(20)))
						{
							targetSubParam = curSubParams[i];
							isLayerUp = true;
						}
						if(GUILayout.Button(iconImageLayerDown, guiStyleListBtn, GUILayout.Width(30), GUILayout.Height(20)))
						{
							targetSubParam = curSubParams[i];
							isLayerDown = true;
						}
						if(GUILayout.Button(iconImageRemove, guiStyleListBtn, GUILayout.Width(30), GUILayout.Height(20)))
						{
							targetSubParam = curSubParams[i];
							isRemoveParam = true;
						}


						EditorGUILayout.EndHorizontal();
					}
				}
				else
				{
					for (int i = 0; i < curNumSubParams; i++)
					{
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
						GUILayout.Space(15);
						EditorGUILayout.EndHorizontal();
					}
				}
				
			}
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
			GUILayout.Space(15);
			if (isSelected)
			{
				//" Add Parameter"
				if (GUILayout.Button(new GUIContent(" " + _editor.GetText(TEXT.DLG_AddParameter), iconImageAddParam), guiStyle_None, GUILayout.Height(20)))
				{
					if (isSelected && curSubParams != null && curSubParams.Count == curNumSubParams)
					{
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged, _editor, _animClip._portrait, null, false);

						curSubParams.Add(new apAnimEvent.SubParameter());
					}
				}
			}
			else
			{
				if (GUILayout.Button("", guiStyle_None, GUILayout.Height(20)))
				{

				}
			}
			EditorGUILayout.EndHorizontal();



			GUILayout.Space(150);

			EditorGUILayout.EndScrollView();

			GUILayout.Space(10);

			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(40));
			if (isSelected)
			{
				//"Remove Event [" + curName + "]"
				if (GUILayout.Button(string.Format("{0} [{1}]", _editor.GetText(TEXT.DLG_RemoveEvent), curName), GUILayout.Height(20)))
				{
					if(_curSelectedEvent != null && _animClip._animEvents.Contains(_curSelectedEvent))
					{
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_RemoveEvent, _editor, _animClip._portrait, null, false);
						_animClip._animEvents.Remove(_curSelectedEvent);

						_curSelectedEvent = null;
						isSelected = false;
					}
				}
			}
			else
			{
				if (GUILayout.Button("", guiStyle_None, GUILayout.Height(20)))
				{

				}
			}
			EditorGUILayout.EndVertical();

			



			if (isSelected && _curSelectedEvent != null)
			{
				//순서를 바꾸거나 SubParam을 삭제하는 요청이 있으면 처리해주자
				if(targetSubParam != null && _curSelectedEvent._subParams.Contains(targetSubParam)) 
				{
					if(isLayerUp)
					{
						//Index -1
						
						int index = _curSelectedEvent._subParams.IndexOf(targetSubParam);
						if(index > 0)
						{
							apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged, _editor, _animClip._portrait, null, false);
							_curSelectedEvent._subParams.Remove(targetSubParam);
							_curSelectedEvent._subParams.Insert(index - 1, targetSubParam);
						}
					}
					else if(isLayerDown)
					{
						//Index +1
						int index = _curSelectedEvent._subParams.IndexOf(targetSubParam);
						if(index < _curSelectedEvent._subParams.Count - 1)
						{
							apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged, _editor, _animClip._portrait, null, false);
							_curSelectedEvent._subParams.Remove(targetSubParam);
							_curSelectedEvent._subParams.Insert(index + 1, targetSubParam);
						}
					}
					else if(isRemoveParam)
					{
						//삭제한다.
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged, _editor, _animClip._portrait, null, false);
						_curSelectedEvent._subParams.Remove(targetSubParam);
					}

				}


				if(	_curSelectedEvent._frameIndex != curFrameIndex ||
					_curSelectedEvent._frameIndex_End != curFrameIndex_End ||
					_curSelectedEvent._eventName != curName ||
					_curSelectedEvent._callType != curCallType)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged, _editor, _animClip._portrait, null, false);

					_curSelectedEvent._frameIndex = curFrameIndex;
					_curSelectedEvent._frameIndex_End = curFrameIndex_End;
					_curSelectedEvent._eventName = curName;
					_curSelectedEvent._callType = curCallType;
				}
			}


			EditorGUILayout.BeginHorizontal();
			bool isClose = false;
			
			//"Close"
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(30)))
			{
				isClose = true;
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			
			if(isGUIEvent)
			{
				_prevNumSubParams = curNumSubParams;
			}

			if (isClose)
			{
				CloseDialog();
			}
		}

		// Functions
		//--------------------------------------------------------------



		// Get / Set
		//--------------------------------------------------------------
	}
}