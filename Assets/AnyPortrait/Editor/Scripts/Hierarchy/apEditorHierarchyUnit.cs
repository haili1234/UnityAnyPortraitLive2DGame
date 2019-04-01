/*
*	Copyright (c) 2017-2018. RainyRizzle. All rights reserved
*	contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of AnyPortrait.
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of Seungjik Lee.
*/

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public class apEditorHierarchyUnit
	{
		// Member
		//--------------------------------------------------------------------------
		public delegate void FUNC_UNIT_CLICK(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj);
		public delegate void FUNC_UNIT_CLICK_VISIBLE(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isVisible, bool isPostfixIcon);


		public enum UNIT_TYPE
		{
			Label,
			ToggleButton,
			ToggleButton_Visible,
			OnlyButton,
		}
		public UNIT_TYPE _unitType = UNIT_TYPE.Label;
		public Texture2D _icon = null;
		public string _text = "";

		public int _level = 0;
		public int _savedKey = -1;
		public object _savedObj = null;

		public enum VISIBLE_TYPE
		{
			None,//<<안보입니더
			NoKey,//MoKey는 없지만 출력은 됩니다.
			Current_Visible,
			Current_NonVisible,
			TmpWork_Visible,
			TmpWork_NonVisible,
			ModKey_Visible,
			ModKey_NonVisible,
			Default_Visible,
			Default_NonVisible

		}
		public VISIBLE_TYPE _visibleType_Prefix = VISIBLE_TYPE.None;//Visible 속성이 붙은 경우는 이것도 세팅해야한다.
		public VISIBLE_TYPE _visibleType_Postfix = VISIBLE_TYPE.None;//Visible 속성이 붙은 경우는 이것도 세팅해야한다.

		public apEditorHierarchyUnit _parentUnit = null;
		public List<apEditorHierarchyUnit> _childUnits = new List<apEditorHierarchyUnit>();

		private bool _isFoldOut = true;
		private bool _isSelected = false;
		private bool _isModRegistered = false;//추가) 현재 선택한 Mod 등에서 등록된

		public void SetFoldOut(bool isFoldOut) { _isFoldOut = isFoldOut; }
		public void SetSelected(bool isSelected) { _isSelected = isSelected; }
		public void SetModRegistered(bool isModRegistered)
		{
			_isModRegistered = isModRegistered;
			RefreshPrevRender();
		}

		public bool IsFoldOut { get { return _isFoldOut; } }
		public bool IsSelected { get { return _isSelected; } }

		public FUNC_UNIT_CLICK _funcClick = null;
		public FUNC_UNIT_CLICK_VISIBLE _funcClickVisible = null;

		private GUIContent _guiContent_Text = new GUIContent();
		private GUIContent _guiContent_Icon = new GUIContent();
		//private GUIContent _guiContent_Folded = new GUIContent();
		private GUIStyle _guiStyle_None;
		private GUIStyle _guiStyle_Selected;
		private GUIStyle _guiStyle_ModIcon;
		private bool _isGUIStyleCreated = false;

		private GUIContent _guiContent_FoldDown = new GUIContent();
		private GUIContent _guiContent_FoldRight = new GUIContent();

		private enum VISIBLE_ICON
		{
			Current,
			TmpWork,
			Default,
			ModKey
		}
		private GUIContent _guiContent_NoKey = null;
		private GUIContent[] _guiContent_Visible = new GUIContent[4];
		private GUIContent[] _guiContent_Nonvisible = new GUIContent[4];

		private GUIContent _guiContent_ModRegisted = new GUIContent();

		public int _indexPerParent = -1;
		private int _indexCountForChild = 0;

		//추가된 내용
		//일부 버튼들은 나오거나 안나올 수 있다.
		//GUIEvent (Layout)을 기준으로 모두 갱신되는데,
		//그 외의 이벤트에서는 "이전 프레임의 기록"을 따라야 한다.
		//이전 프레임에서 렌더링 되었을때 -> 현재 안될때
		// >> 더미 렌더링을 한다. 클릭 이벤트는 발생하지 않는다.
		//이전 프레임에서 렌더링 안되고 -> 현재 될때
		// >> 렌더링하지 않는다.

		//리셋할때 체크한다.
		//GUIEvent에서는 이 변수를 무시한다. (렌더 여부를 갱신한다.)
		private bool _isPrevRender_ModRegBox = false;
		private bool _isPrevRender_VisiblePrefix = false;
		private bool _isPrevRender_VisiblePostfix = false;
		private bool _isPrevRender_Fold = false;

		private bool _isCurRender_ModRegBox;
		private bool _isCurRender_VisiblePrefix;
		private bool _isCurRenderFoldBtn;
		private bool _isCurRender_VisiblePostFix;

		private bool _isNextRender_ModRegBox;
		private bool _isNextRender_VisiblePrefix;
		private bool _isNextRenderFoldBtn;
		private bool _isNextRender_VisiblePostFix;

		// Init
		//--------------------------------------------------------------------------
		public apEditorHierarchyUnit()
		{
			_isSelected = false;
			_isGUIStyleCreated = false;

			//수정 : GUIStyle을 함부로 만드는게 아니다.
			//GUI 이벤트일 때에만 만드는 것
			//렌더링에서 체크해야한다.
			//_guiStyle_None = new GUIStyle(GUIStyle.none);

			//if (EditorGUIUtility.isProSkin)
			//{
			//	_guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;
			//	_guiStyle_None.onHover.textColor = Color.cyan;
			//}
			//else
			//{
			//	_guiStyle_None.normal.textColor = Color.black;
			//	_guiStyle_None.onHover.textColor = Color.cyan;
			//}

			
			//_guiStyle_None.alignment = TextAnchor.MiddleLeft;

			//_guiStyle_Selected = new GUIStyle(GUIStyle.none);
			//if (EditorGUIUtility.isProSkin)
			//{
			//	_guiStyle_Selected.normal.textColor = Color.cyan;
			//	_guiStyle_Selected.onHover.textColor = Color.cyan;
			//}
			//else
			//{
			//	_guiStyle_Selected.normal.textColor = Color.white;
			//	_guiStyle_Selected.onHover.textColor = Color.cyan;
			//}
			
			//_guiStyle_Selected.alignment = TextAnchor.MiddleLeft;

			//_guiStyle_ModIcon = new GUIStyle(GUIStyle.none);
			//_guiStyle_ModIcon.alignment = TextAnchor.MiddleCenter;



			_indexPerParent = -1;

			InitPrevRender();
		}

		private void InitPrevRender()
		{
			_isPrevRender_ModRegBox = false;
			_isPrevRender_VisiblePrefix = false;
			_isPrevRender_VisiblePostfix = false;
			_isPrevRender_Fold = false;
		}

		//추가
		/// <summary>
		/// GUIStyle이 있는지 체크하고 없으면 생성합니다.
		/// </summary>
		private void CheckAndCreateGUIStyle()
		{
			if(_isGUIStyleCreated)
			{
				return;
			}
			_isGUIStyleCreated = true;

			

			_guiStyle_None = new GUIStyle(GUIStyle.none);

			if (EditorGUIUtility.isProSkin)
			{
				_guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;
				_guiStyle_None.onHover.textColor = Color.cyan;
			}
			else
			{
				_guiStyle_None.normal.textColor = Color.black;
				_guiStyle_None.onHover.textColor = Color.cyan;
			}

			
			_guiStyle_None.alignment = TextAnchor.MiddleLeft;

			_guiStyle_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)
			{
				_guiStyle_Selected.normal.textColor = Color.cyan;
				_guiStyle_Selected.onHover.textColor = Color.cyan;
			}
			else
			{
				_guiStyle_Selected.normal.textColor = Color.white;
				_guiStyle_Selected.onHover.textColor = Color.cyan;
			}
			
			_guiStyle_Selected.alignment = TextAnchor.MiddleLeft;

			_guiStyle_ModIcon = new GUIStyle(GUIStyle.none);
			_guiStyle_ModIcon.alignment = TextAnchor.MiddleCenter;
		}

		// Common
		//--------------------------------------------------------------------------

		public void SetBasicIconImg(Texture2D imgFoldDown, Texture2D imgFoldRight, Texture2D imgModRegisted)
		{
			_guiContent_FoldDown = new GUIContent(imgFoldDown);
			_guiContent_FoldRight = new GUIContent(imgFoldRight);
			_guiContent_ModRegisted = new GUIContent(imgModRegisted);
		}

		//TODO : Visible 속성이 붙은 경우는 이걸 호출해서 세팅해줘야 한다.
		public void SetVisibleIconImage(GUIContent guiVisible_Current, GUIContent guiNonVisible_Current,
											GUIContent guiVisible_TmpWork, GUIContent guiNonVisible_TmpWork,
											GUIContent guiVisible_Default, GUIContent guiNonVisible_Default,
											GUIContent guiVisible_ModKey, GUIContent guiNonVisible_ModKey,
											GUIContent gui_NoKey
											)
		{
			if (_guiContent_Visible == null)
			{
				_guiContent_Visible = new GUIContent[4];
			}
			if (_guiContent_Nonvisible == null)
			{
				_guiContent_Nonvisible = new GUIContent[4];
			}

			_guiContent_Visible[(int)VISIBLE_ICON.Current] = guiVisible_Current;
			_guiContent_Visible[(int)VISIBLE_ICON.TmpWork] = guiVisible_TmpWork;
			_guiContent_Visible[(int)VISIBLE_ICON.Default] = guiVisible_Default;
			_guiContent_Visible[(int)VISIBLE_ICON.ModKey] = guiVisible_ModKey;

			_guiContent_Nonvisible[(int)VISIBLE_ICON.Current] = guiNonVisible_Current;
			_guiContent_Nonvisible[(int)VISIBLE_ICON.TmpWork] = guiNonVisible_TmpWork;
			_guiContent_Nonvisible[(int)VISIBLE_ICON.Default] = guiNonVisible_Default;
			_guiContent_Nonvisible[(int)VISIBLE_ICON.ModKey] = guiNonVisible_ModKey;

			_guiContent_NoKey = gui_NoKey;
		}

		public void SetEvent(FUNC_UNIT_CLICK funcUnitClick)
		{
			_funcClick = funcUnitClick;
		}

		//TODO : Visible 속성이 붙은 경우는 위 함수(SetEvent)대신 이걸 호출해야한다.
		public void SetEvent(FUNC_UNIT_CLICK funcUnitClick, FUNC_UNIT_CLICK_VISIBLE funcClickVisible)
		{
			_funcClick = funcUnitClick;
			_funcClickVisible = funcClickVisible;
		}

		public void SetParent(apEditorHierarchyUnit parentUnit)
		{
			_parentUnit = parentUnit;
		}

		public void AddChild(apEditorHierarchyUnit childUnit)
		{
			childUnit._indexPerParent = _indexCountForChild;
			_indexCountForChild++;

			_childUnits.Add(childUnit);

			RefreshPrevRender();
		}



		// Set
		//--------------------------------------------------------------------------
		public void ChangeText(string text)
		{
			_text = text;
			MakeGUIContent();
		}
		public void ChangeIcon(Texture2D icon)
		{
			_icon = icon;
			MakeGUIContent();
		}

		public void SetLabel(Texture2D icon, string text, int savedKey, object savedObj)
		{
			_unitType = UNIT_TYPE.Label;
			_icon = icon;
			_text = text;
			_savedKey = savedKey;
			_savedObj = savedObj;

			MakeGUIContent();
		}

		public void SetToggleButton(Texture2D icon, string text, int savedKey, object savedObj)
		{
			_unitType = UNIT_TYPE.ToggleButton;
			_icon = icon;
			_text = text;
			_savedKey = savedKey;
			_savedObj = savedObj;

			MakeGUIContent();
		}

		public void SetToggleButton_Visible(Texture2D icon, string text, int savedKey, object savedObj, VISIBLE_TYPE visibleType_Prefix, VISIBLE_TYPE visibleType_Postfix)
		{
			_unitType = UNIT_TYPE.ToggleButton_Visible;
			_icon = icon;
			_text = text;
			_savedKey = savedKey;
			_savedObj = savedObj;
			_visibleType_Prefix = visibleType_Prefix;
			_visibleType_Postfix = visibleType_Postfix;

			MakeGUIContent();
		}

		public void SetOnlyButton(Texture2D icon, string text, int savedKey, object savedObj)
		{
			_unitType = UNIT_TYPE.OnlyButton;
			_icon = icon;
			_text = text;
			_savedKey = savedKey;
			_savedObj = savedObj;

			MakeGUIContent();
		}

		private void MakeGUIContent()
		{
			if (_icon != null)
			{
				_guiContent_Icon = new GUIContent(_icon);
			}
			else
			{
				_guiContent_Icon = null;
			}

			if (!string.IsNullOrEmpty(_text))
			{
				_guiContent_Text = new GUIContent(" " + _text + "  ");
			}
			else
			{
				_guiContent_Text = new GUIContent(" <No Name>  ");
			}

			RefreshPrevRender();
		}



		public void RefreshPrevRender()
		{
			_isPrevRender_ModRegBox = _isModRegistered;
			_isPrevRender_VisiblePrefix = (_unitType == UNIT_TYPE.ToggleButton_Visible && _visibleType_Prefix != VISIBLE_TYPE.None);
			_isPrevRender_VisiblePostfix = (_unitType == UNIT_TYPE.ToggleButton_Visible && _visibleType_Postfix != VISIBLE_TYPE.None);
			_isPrevRender_Fold = (_childUnits.Count > 0 || (_parentUnit == null && _unitType == UNIT_TYPE.Label));
		}

		// GUI
		//--------------------------------------------------------------------------
		public void GUI_Render(int leftWidth, int width, int height, float scrollX, bool isGUIEvent)
		{
			CheckAndCreateGUIStyle();//<<추가 GUI Style 생성

			Rect lastRect = GUILayoutUtility.GetLastRect();

			//배경 렌더링
			if (_isSelected)
			{
				Color prevColor = GUI.backgroundColor;

				if(EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
				}
				

				GUI.Box(new Rect(lastRect.x + scrollX, lastRect.y + height, width + 10, height), "");
				GUI.backgroundColor = prevColor;
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Height(height));


			GUILayout.Space(2);

			_isCurRender_ModRegBox = _isModRegistered;
			_isCurRender_VisiblePrefix = (_unitType == UNIT_TYPE.ToggleButton_Visible && _visibleType_Prefix != VISIBLE_TYPE.None);
			_isCurRenderFoldBtn = (_childUnits.Count > 0 || (_parentUnit == null && _unitType == UNIT_TYPE.Label));
			_isCurRender_VisiblePostFix = (_unitType == UNIT_TYPE.ToggleButton_Visible && _visibleType_Postfix != VISIBLE_TYPE.None);

			if (isGUIEvent)
			{
				//GUIEvent에서는 Prev를 무시한다.
				_isNextRender_ModRegBox = _isCurRender_ModRegBox;
				_isNextRender_VisiblePrefix = _isCurRender_VisiblePrefix;
				_isNextRenderFoldBtn = _isCurRenderFoldBtn;
				_isNextRender_VisiblePostFix = _isCurRender_VisiblePostFix;
			}
			else
			{
				//GUIEvent가 아닐 때에는 Prev의 값을 따른다.
				_isNextRender_ModRegBox = _isPrevRender_ModRegBox;
				_isNextRender_VisiblePrefix = _isPrevRender_VisiblePrefix;
				_isNextRenderFoldBtn = _isPrevRender_Fold;
				_isNextRender_VisiblePostFix = _isPrevRender_VisiblePostfix;
			}
			
			

			// Modifier 등록 박스 렌더링
			//if (_isCurRender_ModRegBox)
			if(_isNextRender_ModRegBox)
			{
				GUILayout.Box(_guiContent_ModRegisted, _guiStyle_ModIcon, GUILayout.Width(8), GUILayout.Height(height));
			}
			else
			{
				GUILayout.Space(8);
			}



			// 앞쪽의 "보기" 버튼
			//if (_unitType == UNIT_TYPE.ToggleButton_Visible && _visibleType_Prefix != VISIBLE_TYPE.None)
			if(_isNextRender_VisiblePrefix)
			{
				//앞쪽에도 Visible Button을 띄워야겠다면
				GUIContent visibleGUIContent = null;

				if (!_isCurRender_VisiblePrefix)
				{
					//만약 더미를 렌더링 하는 경우
					visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Current];
				}
				else
				{
					//정식 렌더링인 경우
					switch (_visibleType_Prefix)
					{
						case VISIBLE_TYPE.Current_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.Current]; break;
						case VISIBLE_TYPE.Current_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Current]; break;
						case VISIBLE_TYPE.TmpWork_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.TmpWork]; break;
						case VISIBLE_TYPE.TmpWork_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.TmpWork]; break;
						case VISIBLE_TYPE.Default_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.Default]; break;
						case VISIBLE_TYPE.Default_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Default]; break;
						case VISIBLE_TYPE.ModKey_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.ModKey]; break;
						case VISIBLE_TYPE.ModKey_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.ModKey]; break;
						case VISIBLE_TYPE.NoKey:				visibleGUIContent = _guiContent_NoKey; break;

					}
				}

				if (GUILayout.Button(visibleGUIContent, _guiStyle_None, GUILayout.Width(20), GUILayout.Height(height)))
				{
					if (_isCurRender_VisiblePrefix)
					{
						if (_funcClickVisible != null)
						{
							_funcClickVisible(this, _savedKey, _savedObj,
								_visibleType_Prefix == VISIBLE_TYPE.Current_Visible ||
								_visibleType_Prefix == VISIBLE_TYPE.Default_Visible ||
								_visibleType_Prefix == VISIBLE_TYPE.TmpWork_Visible ||
								_visibleType_Prefix == VISIBLE_TYPE.ModKey_Visible, true);
						}
					}
				}
				leftWidth -= 22;
				if (leftWidth < 0)
				{
					leftWidth = 0;
				}
			}
			GUILayout.Space(leftWidth);


			//맨 앞에 ▼/▶ 아이콘을 보이고, 작동시킬지를 결정
			//bool isFoldVisible = false;
			//if (_childUnits.Count > 0 || (_parentUnit == null && _unitType == UNIT_TYPE.Label))
			//{
			//	isFoldVisible = true;
			//}

			int width_FoldBtn = height - 4;
			//int width_Icon = height - 2;
			int width_Icon = height - 6;

			//GUIContent guiContent = null;
			//if (isFoldVisible)
			if(_isNextRenderFoldBtn)
			{
				//Fold 아이콘을 출력하고 Button 기능을 추가한다.
				GUIContent btnContent = _guiContent_FoldDown;
				if (!_isFoldOut)
				{
					btnContent = _guiContent_FoldRight;
				}
				if (GUILayout.Button(btnContent, _guiStyle_None, GUILayout.Width(width_FoldBtn), GUILayout.Height(height)))
				{
					if (_isCurRenderFoldBtn)
					{
						//정식 렌더링인 경우에 바꿔주자
						_isFoldOut = !_isFoldOut;
					}
				}
			}
			else
			{
				GUILayout.Space(width_FoldBtn);
			}


			// 기본 아이콘
			if (_guiContent_Icon != null)
			{
				if (GUILayout.Button(_guiContent_Icon, _guiStyle_None, GUILayout.Width(width_Icon), GUILayout.Height(height)))
				{
					if (_unitType == UNIT_TYPE.Label)
					{
						//if (isFoldVisible)
						if(_isNextRenderFoldBtn && _isCurRenderFoldBtn)
						{
							_isFoldOut = !_isFoldOut;
						}
					}
					else
					{
						if (_funcClick != null)
						{
							_funcClick(this, _savedKey, _savedObj);
						}
					}
				}
			}


			//유닛의 타입에 따라 다르게 출력한다.
			switch (_unitType)
			{
				//Label : 별도의 버튼 기능 없이 아이콘+텍스트만 보인다.
				//만약, Fold가 가능한 경우 버튼으로 바뀌는데, Fold Toggle에 사용된다.
				case UNIT_TYPE.Label:
					//if (isFoldVisible)
					if(_isNextRenderFoldBtn)
					{
						if (GUILayout.Button(_guiContent_Text, _guiStyle_None, GUILayout.Height(height)))
						{
							if (_isNextRenderFoldBtn && _isCurRenderFoldBtn)
							{
								_isFoldOut = !_isFoldOut;
							}
						}
					}
					else
					{
						EditorGUILayout.LabelField(_guiContent_Text, GUILayout.Height(height));
					}
					break;

				//OnlyButton : Toggle 기능 없이 항상 버튼의 역할을 한다.
				case UNIT_TYPE.OnlyButton:
					if (GUILayout.Button(_guiContent_Text, _guiStyle_None, GUILayout.Height(height)))
					{
						if (_funcClick != null)
						{
							_funcClick(this, _savedKey, _savedObj);
						}
					}
					break;

				//ToggleButton : Off된 상태에서는 On하기 위한 버튼이며, On이 된 경우는 단순히 아이콘+텍스트만 출력한다.
				case UNIT_TYPE.ToggleButton:
					if (!_isSelected)
					{
						if (GUILayout.Button(_guiContent_Text, _guiStyle_None, GUILayout.Height(height)))
						{
							if (_funcClick != null)
							{
								_funcClick(this, _savedKey, _savedObj);
							}
						}
					}
					else
					{

						GUILayout.Label(_guiContent_Text, _guiStyle_Selected, GUILayout.Height(height));
					}

					break;

				//ToggleButton
				case UNIT_TYPE.ToggleButton_Visible:
					if (!_isSelected)
					{
						if (GUILayout.Button(_guiContent_Text, _guiStyle_None, GUILayout.Height(height)))
						{
							if (_funcClick != null)
							{
								_funcClick(this, _savedKey, _savedObj);
							}
						}
					}
					else
					{
						GUILayout.Label(_guiContent_Text, _guiStyle_Selected, GUILayout.Height(height));
					}


					//if (_visibleType_Postfix != VISIBLE_TYPE.None)
					if(_isNextRender_VisiblePostFix)
					{
						GUIContent visibleGUIContent = null;

						if (!_isCurRender_VisiblePostFix)
						{
							//더미 렌더링이라면
							visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Current];
						}
						else
						{
							switch (_visibleType_Postfix)
							{
								case VISIBLE_TYPE.Current_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.Current]; break;
								case VISIBLE_TYPE.Current_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Current]; break;
								case VISIBLE_TYPE.TmpWork_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.TmpWork]; break;
								case VISIBLE_TYPE.TmpWork_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.TmpWork]; break;
								case VISIBLE_TYPE.Default_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.Default]; break;
								case VISIBLE_TYPE.Default_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Default]; break;
								case VISIBLE_TYPE.ModKey_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.ModKey]; break;
								case VISIBLE_TYPE.ModKey_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.ModKey]; break;
								case VISIBLE_TYPE.NoKey:				visibleGUIContent = _guiContent_NoKey; break;

							}
						}

						if (GUILayout.Button(visibleGUIContent, _guiStyle_None, GUILayout.Width(20), GUILayout.Height(height)))
						{
							if (_isCurRender_VisiblePostFix)
							{
								if (_funcClickVisible != null)
								{

									_funcClickVisible(this, _savedKey, _savedObj,
										_visibleType_Postfix == VISIBLE_TYPE.Current_Visible ||
										_visibleType_Postfix == VISIBLE_TYPE.Default_Visible ||
										_visibleType_Postfix == VISIBLE_TYPE.TmpWork_Visible ||
										_visibleType_Postfix == VISIBLE_TYPE.ModKey_Visible, false);
								}
							}
						}
					}
					break;
			}

			EditorGUILayout.EndHorizontal();

			if (isGUIEvent)
			{
				//이전 프레임과 렌더링 동기화
				//설정에 의해 Prev를 갱신합니다.
				RefreshPrevRender();
			}
			
		}
	}

}