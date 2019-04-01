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

	public class apDialog_SelectLinkedBone : EditorWindow
	{
		// Members
		//----------------------------------------------------------------------------
		public delegate void FUNC_SELECT_LINKED_BONE(bool isSuccess, object loadKey, bool isNullBone, apBone selectedBone, apBone targetBone, REQUEST_TYPE requestType);

		private static apDialog_SelectLinkedBone s_window = null;

		public enum REQUEST_TYPE
		{
			AttachChild,
			ChangeParent,
			SelectIKTarget,
		}


		private apEditor _editor = null;
		private object _loadKey = null;

		private FUNC_SELECT_LINKED_BONE _funcResult;
		private apMeshGroup _targetMeshGroup = null;
		private apBone _targetBone = null;
		private REQUEST_TYPE _requestType = REQUEST_TYPE.AttachChild;

		//private List<apBone> _selectableBones = new List<apBone>();
		private class BoneUnit
		{
			public apBone _bone = null;
			public bool _isSelectable = false;
			public bool _isTarget = false;
			public bool _isFoldable = false;
			public bool _isFolded = false;
			public BoneUnit _parentUnit = null;
			public string _name = "";
			public int _level = 0;
			public List<BoneUnit> _childUnits = new List<BoneUnit>();

			public BoneUnit(apBone bone, bool isSelectable, bool isTarget, BoneUnit parentUnit, int level)
			{
				_bone = bone;

				if (_bone != null)
				{
					_name = " " + _bone._name;
				}
				else
				{
					_name = " None";
				}
				_isSelectable = isSelectable;
				_isTarget = isTarget;
				_parentUnit = parentUnit;

				_level = level;

				_childUnits.Clear();

				_isFoldable = false;
				_isFolded = false;

				//if (_bone != null)
				//{
				//	if (_bone._childBones.Count > 0)
				//	{
				//		_isFoldable = true;
				//		_isFolded = false;
				//	}
				//	else
				//	{
				//		_isFoldable = false;
				//		_isFolded = false;
				//	}
				//}
				//else
				//{
				//	_isFoldable = false;
				//	_isFolded = false;
				//}
			}
			public void AddBoneUnit(BoneUnit childUnit)
			{
				_childUnits.Add(childUnit);

				_isFoldable = true;
				_isFolded = false;
			}
		}
		private List<BoneUnit> _boneUnits = new List<BoneUnit>();
		private List<BoneUnit> _boneUnits_Root = new List<BoneUnit>();

		private BoneUnit _selectedBoneUnit = null;

		private Vector2 _scrollList = new Vector2();

		private bool _isSearched = false;
		private string _strSearchKeyword = "";

		// Show Window / Close Dialog
		//------------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apBone targetBone, apMeshGroup targetMeshGroup, REQUEST_TYPE requestType, FUNC_SELECT_LINKED_BONE funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			string windowName = "";
			switch (requestType)
			{
				case REQUEST_TYPE.AttachChild:
					windowName = "Select a Bone as Child";
					break;

				case REQUEST_TYPE.ChangeParent:
					windowName = "Select a Bone as Parent";
					break;

				case REQUEST_TYPE.SelectIKTarget:
					windowName = "Select a Bone as IK Target";
					break;
			}
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectLinkedBone), true, windowName, true);
			apDialog_SelectLinkedBone curTool = curWindow as apDialog_SelectLinkedBone;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				//기본 Dialog보다 조금 더 크다. Hierarchy 방식으로 가로 스크롤이 포함되기 때문
				int width = 350;
				int height = 600;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, targetBone, targetMeshGroup, requestType, funcResult);

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
		//------------------------------------------------------------------------
		public void Init(apEditor editor, object loadKey, apBone targetBone, apMeshGroup targetMeshGroup, REQUEST_TYPE requestType, FUNC_SELECT_LINKED_BONE funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_funcResult = funcResult;
			_targetBone = targetBone;
			_targetMeshGroup = targetMeshGroup;
			_requestType = requestType;

			_selectedBoneUnit = null;
			_boneUnits.Clear();
			_boneUnits_Root.Clear();

			List<apBone> exclusiveBones = new List<apBone>();


			//None 유닛 선택
			//Parent에서만 가능
			if (_requestType == REQUEST_TYPE.ChangeParent)
			{
				BoneUnit nullUnit = new BoneUnit(null, true, false, null, 0);
				_boneUnits.Add(nullUnit);
				_boneUnits_Root.Add(nullUnit);
			}

			//기존 Rootqnxj 순회하는 방식으로 리스트를 만들되,
			//"제외"되는 것들을 음영처리하자
			//별도의 Unit 필요
			//모두 None Unit이 있다.

			switch (_requestType)
			{
				case REQUEST_TYPE.AttachChild://Parent 중 하나를 Child로 둬선 안된다. (Loop 발생)
				case REQUEST_TYPE.ChangeParent:
					{
						//전체 Bone 중에서
						//- 자기 자신 제외
						//- Parent 제외

						exclusiveBones.Add(_targetBone);
						if (_targetBone._parentBone != null)
						{
							exclusiveBones.Add(_targetBone._parentBone);
							if (_requestType == REQUEST_TYPE.AttachChild)
							{
								//재귀적인 Parent 제외
								AddExclusiveBoneRecursive(exclusiveBones, _targetBone._parentBone, false);
							}
						}
						for (int iChild = 0; iChild < _targetBone._childBones.Count; iChild++)
						{
							apBone childBone = _targetBone._childBones[iChild];
							if (childBone != null)
							{
								exclusiveBones.Add(childBone);
							}
							if (_requestType == REQUEST_TYPE.ChangeParent)
							{
								//재귀적인 Child 제외
								AddExclusiveBoneRecursive(exclusiveBones, childBone, true);
							}
						}
						//BoneUnit을 넣자
						for (int i = 0; i < _targetMeshGroup._boneList_Root.Count; i++)
						{
							AddBoneUnitRecursive(_targetMeshGroup._boneList_Root[i], exclusiveBones, _targetBone, null);
						}

					}
					break;

				case REQUEST_TYPE.SelectIKTarget:
					{
						//<재귀적인 Child 중에서>
						//-자기 자신 제외
						exclusiveBones.Add(_targetBone);
						for (int iChild = 0; iChild < _targetBone._childBones.Count; iChild++)
						{
							apBone childBone = _targetBone._childBones[iChild];
							if (childBone != null)
							{
								AddBoneUnitRecursive(childBone, exclusiveBones, _targetBone, null);
							}
						}
					}
					break;
			}

			_isSearched = false;
			_strSearchKeyword = "";

			_boneUnits_Root.Sort(delegate (BoneUnit a, BoneUnit b)
			{
				if (a._bone == null && b._bone == null)
				{
					return 0;
				}
				if (a._bone == null)
				{
					return -1;
				}
				else if (b._bone == null)
				{
					return 1;
				}
				return string.Compare(a._name, b._name);

			});
		}

		private void AddExclusiveBoneRecursive(List<apBone> exclusiveBones, apBone bone, bool toChild)
		{
			if (!exclusiveBones.Contains(bone))
			{
				exclusiveBones.Add(bone);
			}

			if (toChild)
			{
				if (bone._childBones.Count > 0)
				{
					for (int i = 0; i < bone._childBones.Count; i++)
					{
						AddExclusiveBoneRecursive(exclusiveBones, bone._childBones[i], true);
					}
				}
			}
			else
			{
				if (bone._parentBone != null)
				{
					AddExclusiveBoneRecursive(exclusiveBones, bone._parentBone, false);
				}
			}
		}

		private void AddBoneUnitRecursive(apBone bone, List<apBone> exclusiveBones, apBone targetBone, BoneUnit parentBoneUnit)
		{
			int nextLevel = 0;
			if (parentBoneUnit != null)
			{
				nextLevel = parentBoneUnit._level + 1;
			}
			BoneUnit boneUnit = new BoneUnit(bone, !exclusiveBones.Contains(bone), bone == targetBone, parentBoneUnit, nextLevel);

			_boneUnits.Add(boneUnit);
			if (parentBoneUnit != null)
			{
				parentBoneUnit.AddBoneUnit(boneUnit);
			}

			if (parentBoneUnit == null)
			{
				_boneUnits_Root.Add(boneUnit);
			}

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				AddBoneUnitRecursive(bone._childBones[i], exclusiveBones, targetBone, boneUnit);
			}

			//다 넣었으면 _boneUnit별로 Sort를 하자
			if (boneUnit._childUnits.Count > 0)
			{
				boneUnit._childUnits.Sort(delegate (BoneUnit a, BoneUnit b)
				{
					return string.Compare(a._bone._name, b._bone._name);
				});
			}
		}

		// GUI
		//------------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _funcResult == null)
			{
				return;
			}

			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(0, 35, width, height - (90 + 12)), "");
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();

			Texture2D iconImageCategory = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
			Texture2D iconBone = _editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging);

			Texture2D iconImage_FoldDown = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
			Texture2D iconImage_FoldRight = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight);

			GUIStyle guiStyle = new GUIStyle(GUIStyle.none);
			guiStyle.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Selected = new GUIStyle(GUIStyle.none);
			if(EditorGUIUtility.isProSkin)
			{
				guiStyle_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Selected.normal.textColor = Color.white;
			}
			guiStyle_Selected.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_NotSelectable = new GUIStyle(GUIStyle.none);
			guiStyle_NotSelectable.normal.textColor = Color.red;
			guiStyle_NotSelectable.alignment = TextAnchor.MiddleLeft;


			GUIStyle guiStyle_Center = new GUIStyle(GUIStyle.none);
			guiStyle_Center.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;

			GUIContent guiContent_Bone = new GUIContent(iconBone);

			GUILayout.Space(10);
			//GUILayout.Button("Select a Bone to Link", guiStyle_Center, GUILayout.Width(width), GUILayout.Height(15));//<투명 버튼
			//"Search  "
			_strSearchKeyword = EditorGUILayout.DelayedTextField(_editor.GetText(TEXT.DLG_Search) + "  ", _strSearchKeyword, GUILayout.Width(width - 20), GUILayout.Height(15));

			if (string.IsNullOrEmpty(_strSearchKeyword))
			{
				_isSearched = false;
			}
			else
			{
				_isSearched = true;
			}
			GUILayout.Space(10);

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height - (90)));

			//_targetMeshGroup._name + " Bones"
			GUILayout.Button(new GUIContent(string.Format("{0} {1}", _targetMeshGroup._name, _editor.GetText(TEXT.DLG_Bones)), iconImageCategory), guiStyle, GUILayout.Height(20));//<투명 버튼

			//GUILayout.Space(10);

			for (int i = 0; i < _boneUnits_Root.Count; i++)
			{
				DrawBoneUnit(_boneUnits_Root[i], 0, width, iconImage_FoldDown, iconImage_FoldRight, guiContent_Bone, guiStyle, guiStyle_Selected, guiStyle_NotSelectable, _scrollList.x);
			}

			GUILayout.Space(50);
			

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();


			bool isClose = false;
			bool isSelectBtnAvailable = (_selectedBoneUnit != null && _selectedBoneUnit._isSelectable);
			if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Select), false, isSelectBtnAvailable, (width / 2) - 8, 30))//"Select"
			{
				if (_selectedBoneUnit != null)
				{
					//선택 가능한 Unit이라면 Return
					if (_selectedBoneUnit._isSelectable)
					{
						_funcResult(true, _loadKey, _selectedBoneUnit._bone == null, _selectedBoneUnit._bone, _targetBone, _requestType);
						isClose = true;
					}
				}
			}

			if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Close), false, true, (width / 2) - 8, 30))//"Close"
			{
				//_funcResult(false, _loadKey, null, null);
				_funcResult(false, _loadKey, false, null, null, _requestType);
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if (isClose)
			{
				CloseDialog();
			}
		}

		private void DrawBoneUnit(BoneUnit boneUnit, int level, int width,
									Texture2D imgIcon_FoldDown, Texture2D imgIcon_FoldRight,
									GUIContent guiContent_Bone,
									GUIStyle guiStyle_None, GUIStyle guiStyle_Selected, GUIStyle guiStyle_NotSelectable,
									float scrollX)
		{
			//Search 옵션에 따라 다르다.
			bool isRenderable = true;
			if (_isSearched)
			{
				if (boneUnit._bone != null)
				{
					if (boneUnit._name.Contains(_strSearchKeyword))
					{
						isRenderable = true;
					}
					else
					{
						isRenderable = false;
					}
				}
			}

			if (isRenderable)
			{
				bool isNotSelectable = !boneUnit._isSelectable || boneUnit._isTarget;
				if (boneUnit == _selectedBoneUnit)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();
					Color prevColor = GUI.backgroundColor;

					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					}

					//GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width, 20), "");
					GUI.Box(new Rect(lastRect.x + scrollX, lastRect.y + 20, width, 20), "");
					GUI.backgroundColor = prevColor;
				}

				if (_isSearched)
				{
					if (boneUnit._parentUnit != null)
					{
						if (boneUnit._parentUnit._bone != null && !boneUnit._parentUnit._name.Contains(_strSearchKeyword))
						{
							//Parent Unit이 검색에 포함되지 않는 경우
							//realLevel -= boneUnit._parentUnit._parentUnit._level;
							level = 0;
						}
					}
				}

				

				EditorGUILayout.BeginHorizontal(GUILayout.Width((width - 50) + level * 10));
				GUILayout.Space(15 + (level * 10));


				//Fold 관련
				if (boneUnit._isFoldable)
				{
					Texture2D foldIcon = imgIcon_FoldDown;
					if (boneUnit._isFolded)
					{
						foldIcon = imgIcon_FoldRight;
					}
					if (GUILayout.Button(foldIcon, guiStyle_None, GUILayout.Width(20), GUILayout.Height(20)))
					{
						boneUnit._isFolded = !boneUnit._isFolded;
					}
				}
				else
				{
					if (boneUnit._bone != null)
					{
						EditorGUILayout.LabelField(guiContent_Bone, guiStyle_None, GUILayout.Width(20), GUILayout.Height(20));
					}
					else
					{
						EditorGUILayout.LabelField("", guiStyle_None, GUILayout.Width(20), GUILayout.Height(20));
					}
				}

				GUIStyle guiStyleLabel = guiStyle_None;
				if (isNotSelectable)
				{
					guiStyleLabel = guiStyle_NotSelectable;
				}
				else if(boneUnit == _selectedBoneUnit)
				{
					guiStyleLabel = guiStyle_Selected;
				}
				if (GUILayout.Button(boneUnit._name, guiStyleLabel, GUILayout.Width((width - 35) - 22), GUILayout.Height(20)))
				{
					//if(boneUnit._isSelectable && !boneUnit._isTarget)
					if (!isNotSelectable)
					{
						_selectedBoneUnit = boneUnit;
					}
				}

				EditorGUILayout.EndHorizontal();

			}
			if (!boneUnit._isFolded
				//|| _isSearched
				)
			{
				for (int i = 0; i < boneUnit._childUnits.Count; i++)
				{
					DrawBoneUnit(boneUnit._childUnits[i], level + 1, width, imgIcon_FoldDown, imgIcon_FoldRight, guiContent_Bone, guiStyle_None, guiStyle_Selected, guiStyle_NotSelectable, scrollX);
				}
			}

			//	EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
			//	GUILayout.Space(15);
			//	if(GUILayout.Button(new GUIContent(" " + _selectableMeshGroups[i]._name, iconMeshGroup), guiStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
			//	{
			//		_selectedMeshGroup = _selectableMeshGroups[i];
			//	}

			//	EditorGUILayout.EndHorizontal();
			//}
		}
	}

}