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

	public class apEditorMeshGroupHierarchy
	{
		// Members
		//---------------------------------------------
		private apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }



		public enum CATEGORY
		{
			MainName,
			//Transform,
			//Mesh_Name,
			Mesh_Item,
			//Mesh_Load,
			//MeshGroup_Name,
			MeshGroup_Item,
			//MeshGroup_Load,

			MainName_Bone,
			SubName_Bone,
			Bone_Item
		}

		//루트들만 따로 적용
		//private apEditorHierarchyUnit _rootUnit_Mesh = null;
		//private apEditorHierarchyUnit _rootUnit_MeshGroup = null;

		//Mesh / Bone으로 나눔
		private apEditorHierarchyUnit _rootUnit_Meshes = null;
		private apEditorHierarchyUnit _rootUnit_Bones_Main = null;
		private List<apEditorHierarchyUnit> _rootUnit_Bones_Sub = new List<apEditorHierarchyUnit>();

		public List<apEditorHierarchyUnit> _units_All = new List<apEditorHierarchyUnit>();
		public List<apEditorHierarchyUnit> _units_Root_Meshes = new List<apEditorHierarchyUnit>();
		public List<apEditorHierarchyUnit> _units_Root_Bones = new List<apEditorHierarchyUnit>();

		public enum HIERARCHY_TYPE
		{
			Meshes, Bones
		}


		//Visible Icon에 대한 GUIContent
		private GUIContent _guiContent_Visible_Current = null;
		private GUIContent _guiContent_NonVisible_Current = null;

		private GUIContent _guiContent_Visible_TmpWork = null;
		private GUIContent _guiContent_NonVisible_TmpWork = null;

		private GUIContent _guiContent_Visible_Default = null;
		private GUIContent _guiContent_NonVisible_Default = null;

		private GUIContent _guiContent_Visible_ModKey = null;
		private GUIContent _guiContent_NonVisible_ModKey = null;

		private GUIContent _guiContent_NoKey = null;

		// Init
		//------------------------------------------------------------------------
		public apEditorMeshGroupHierarchy(apEditor editor)
		{
			_editor = editor;
		}


		private void ReloadGUIContent()
		{
			if (_editor == null)
			{
				return;
			}
			if (_guiContent_Visible_Current == null)		{ _guiContent_Visible_Current =		new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Current)); }
			if (_guiContent_NonVisible_Current == null)		{ _guiContent_NonVisible_Current =	new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Current)); }
			if (_guiContent_Visible_TmpWork == null)		{ _guiContent_Visible_TmpWork =		new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_TmpWork)); }
			if (_guiContent_NonVisible_TmpWork == null)		{ _guiContent_NonVisible_TmpWork =	new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_TmpWork)); }
			if (_guiContent_Visible_Default == null)		{ _guiContent_Visible_Default =		new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Default)); }
			if (_guiContent_NonVisible_Default == null)		{ _guiContent_NonVisible_Default =	new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Default)); }
			if (_guiContent_Visible_ModKey == null)			{ _guiContent_Visible_ModKey =		new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_ModKey)); }
			if (_guiContent_NonVisible_ModKey == null)		{ _guiContent_NonVisible_ModKey =	new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_ModKey)); }
			if (_guiContent_NoKey == null)					{ _guiContent_NoKey =				new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NoKey)); }
		}


		public void ResetSubUnits()
		{
			_units_All.Clear();
			_units_Root_Meshes.Clear();
			_units_Root_Bones.Clear();


			_rootUnit_Meshes = null;
			_rootUnit_Bones_Main = null;
			_rootUnit_Bones_Sub.Clear();

			//_rootUnit_Mesh = AddUnit_Label(null, "Child Meshes", CATEGORY.Mesh_Name, null, true, null);
			//_rootUnit_MeshGroup = AddUnit_Label(null, "Child Mesh Groups", CATEGORY.MeshGroup_Name, null, true, null);

			if (Editor == null || Editor._portrait == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			string meshGroupName = Editor.Select.MeshGroup._name;
			if(meshGroupName.Length > 16)
			{
				meshGroupName = meshGroupName.Substring(0, 14) + "..";
			}
			_rootUnit_Meshes = AddUnit_Label(null, meshGroupName, CATEGORY.MainName, null, true, null, HIERARCHY_TYPE.Meshes);
			_rootUnit_Bones_Main = AddUnit_Label(null, meshGroupName, CATEGORY.MainName_Bone, null, true, null, HIERARCHY_TYPE.Bones);



			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			if (meshGroup._childMeshGroupTransformsWithBones.Count > 0)
			{
				for (int i = 0; i < meshGroup._childMeshGroupTransformsWithBones.Count; i++)
				{
					apTransform_MeshGroup meshGroupTransformWithBones = meshGroup._childMeshGroupTransformsWithBones[i];

					//Bone을 가지고 있는 Child MeshGroup Transform을 Sub 루트로 삼는다.
					//나중에 구분하기 위해 meshGroupTransform을 SavedObj에 넣는다.
					_rootUnit_Bones_Sub.Add(
						AddUnit_Label(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup),
										meshGroupTransformWithBones._nickName,
										CATEGORY.SubName_Bone,
										meshGroupTransformWithBones, //<Saved Obj
										true, null,
										HIERARCHY_TYPE.Bones));

				}
			}

			//List<apTransform_Mesh> childMeshTransforms = Editor.Select.MeshGroup._childMeshTransforms;
			//List<apTransform_MeshGroup> childMeshGroupTransforms = Editor.Select.MeshGroup._childMeshGroupTransforms;

			//구버전 코드
			#region [미사용 코드]
			////> 재귀적인 Hierarchy를 허용하지 않는다.
			//for (int i = 0; i < childMeshTransforms.Count; i++)
			//{
			//	apTransform_Mesh meshTransform = childMeshTransforms[i];
			//	Texture2D iconImage = null;
			//	if(meshTransform._isClipping_Child)
			//	{
			//		iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
			//	}
			//	else
			//	{
			//		iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			//	}

			//	AddUnit_ToggleButton_Visible(	iconImage, 
			//									meshTransform._nickName, 
			//									CATEGORY.Mesh_Item, 
			//									meshTransform, 
			//									false, 
			//									//_rootUnit_Mesh,
			//									_rootUnit,
			//									meshTransform._isVisible_Default);
			//}

			//for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			//{
			//	apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];
			//	AddUnit_ToggleButton_Visible(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup), 
			//									meshGroupTransform._nickName, 
			//									CATEGORY.MeshGroup_Item, 
			//									meshGroupTransform, 
			//									false, 
			//									//_rootUnit_MeshGroup,
			//									_rootUnit,
			//									meshGroupTransform._isVisible_Default);
			//} 
			#endregion

			//변경된 코드
			//재귀적인 구조를 만들고 싶다면 여길 수정하자
			AddTransformOfMeshGroup(Editor.Select.MeshGroup, _rootUnit_Meshes);

			//Bone도 만들어주자
			//Main과 Sub 모두
			AddBoneUnitsOfMeshGroup(meshGroup, _rootUnit_Bones_Main);

			if (_rootUnit_Bones_Sub.Count > 0)
			{
				for (int i = 0; i < _rootUnit_Bones_Sub.Count; i++)
				{
					apTransform_MeshGroup mgTranform = _rootUnit_Bones_Sub[i]._savedObj as apTransform_MeshGroup;
					if (mgTranform != null && mgTranform._meshGroup != null)
					{
						AddBoneUnitsOfMeshGroup(mgTranform._meshGroup, _rootUnit_Bones_Sub[i]);
					}
				}
			}
		}

		// Functions
		//------------------------------------------------------------------------

		private apEditorHierarchyUnit AddUnit_Label(Texture2D icon,
														string text,
														CATEGORY savedKey,
														object savedObj,
														bool isRoot,
														apEditorHierarchyUnit parent,
														HIERARCHY_TYPE hierarchyType)
		{
			apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
										Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
										Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));
			newUnit.SetEvent(OnUnitClick);
			newUnit.SetLabel(icon, text, (int)savedKey, savedObj);
			newUnit.SetModRegistered(false);

			_units_All.Add(newUnit);
			if (isRoot)
			{
				if (hierarchyType == HIERARCHY_TYPE.Meshes)
				{
					_units_Root_Meshes.Add(newUnit);
				}
				else
				{
					_units_Root_Bones.Add(newUnit);
				}
			}

			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}
			return newUnit;
		}


		private apEditorHierarchyUnit AddUnit_ToggleButton_Visible(Texture2D icon,
																		string text,
																		CATEGORY savedKey,
																		object savedObj,
																		bool isRoot,
																		apEditorHierarchyUnit parent,
																		apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Prefix,
																		apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Postfix,
																		bool isModRegisted,
																		HIERARCHY_TYPE hierarchyType)
		{
			apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
										Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
										Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));

			ReloadGUIContent();

			newUnit.SetVisibleIconImage(
				_guiContent_Visible_Current, _guiContent_NonVisible_Current,
				_guiContent_Visible_TmpWork, _guiContent_NonVisible_TmpWork,
				_guiContent_Visible_Default, _guiContent_NonVisible_Default,
				_guiContent_Visible_ModKey, _guiContent_NonVisible_ModKey,
				_guiContent_NoKey
				);

			newUnit.SetEvent(OnUnitClick, OnUnitVisibleClick);
			newUnit.SetToggleButton_Visible(icon, text, (int)savedKey, savedObj, visibleType_Prefix, visibleType_Postfix);

			newUnit.SetModRegistered(isModRegisted);

			_units_All.Add(newUnit);
			if (isRoot)
			{
				if (hierarchyType == HIERARCHY_TYPE.Meshes)
				{
					_units_Root_Meshes.Add(newUnit);
				}
				else
				{
					_units_Root_Bones.Add(newUnit);
				}

			}

			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}
			return newUnit;
		}


		private apEditorHierarchyUnit AddUnit_ToggleButton(Texture2D icon,
																		string text,
																		CATEGORY savedKey,
																		object savedObj,
																		bool isRoot, bool isModRegisted,
																		apEditorHierarchyUnit parent,
																		HIERARCHY_TYPE hierarchyType)
		{
			apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
										Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
										Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));
			//newUnit.SetVisibleIconImage(
			//	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible), 
			//	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible));
			//newUnit.SetEvent(OnUnitClick, OnUnitVisibleClick);
			//newUnit.SetToggleButton_Visible(icon, text, (int)savedKey, savedObj, isVisible);

			newUnit.SetEvent(OnUnitClick);
			newUnit.SetToggleButton(icon, text, (int)savedKey, savedObj);

			newUnit.SetModRegistered(isModRegisted);

			_units_All.Add(newUnit);
			if (isRoot)
			{
				if (hierarchyType == HIERARCHY_TYPE.Meshes)
				{
					_units_Root_Meshes.Add(newUnit);
				}
				else
				{
					_units_Root_Bones.Add(newUnit);
				}

			}

			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}
			return newUnit;
		}


		// 추가 : Transform 리스트를 재귀적으로 만든다.
		//-----------------------------------------------------------------------------------------
		private void AddTransformOfMeshGroup(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit)
		{
			List<apTransform_Mesh> childMeshTransforms = targetMeshGroup._childMeshTransforms;
			List<apTransform_MeshGroup> childMeshGroupTransforms = targetMeshGroup._childMeshGroupTransforms;

			bool isModRegisted = false;
			apModifierBase modifier = Editor.Select.Modifier;

			for (int i = 0; i < childMeshTransforms.Count; i++)
			{
				apTransform_Mesh meshTransform = childMeshTransforms[i];
				Texture2D iconImage = null;
				if (meshTransform._isClipping_Child)
				{
					iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
				}
				else
				{
					iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
				}

				isModRegisted = false;
				if (modifier != null)
				{
					isModRegisted = IsModRegistered(meshTransform);
				}

				//apEditorHierarchyUnit.VISIBLE_ICON_TYPE visibleType = (meshTransform._isVisible_Default) ? apEditorHierarchyUnit.VISIBLE_ICON_TYPE.Visible : apEditorHierarchyUnit.VISIBLE_ICON_TYPE.NonVisible;



				AddUnit_ToggleButton_Visible(iconImage,
														meshTransform._nickName,
														CATEGORY.Mesh_Item,
														meshTransform,
														false,
														//_rootUnit_Mesh,
														parentUnit,
														GetVisibleIconType(meshTransform, isModRegisted, true),
														GetVisibleIconType(meshTransform, isModRegisted, false),
														isModRegisted,
														HIERARCHY_TYPE.Meshes
														);


			}

			for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			{
				apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];

				isModRegisted = false;
				if (modifier != null)
				{
					isModRegisted = IsModRegistered(meshGroupTransform);
				}

				apEditorHierarchyUnit newUnit = AddUnit_ToggleButton_Visible(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup),
													meshGroupTransform._nickName,
													CATEGORY.MeshGroup_Item,
													meshGroupTransform,
													false,
													//_rootUnit_MeshGroup,
													parentUnit,
													//meshGroupTransform._isVisible_Default,
													GetVisibleIconType(meshGroupTransform, isModRegisted, true),
													GetVisibleIconType(meshGroupTransform, isModRegisted, false),
													isModRegisted,
													HIERARCHY_TYPE.Meshes);


				if (meshGroupTransform._meshGroup != null)
				{
					AddTransformOfMeshGroup(meshGroupTransform._meshGroup, newUnit);
				}
			}
		}


		// 본 리스트를 만든다.
		// 본은 Child MeshGroup의 본에 상위의 메시가 리깅되는 경우가 없다.
		// 다만, Transform계열 Modifier (Anim 포함)에서 Child MeshGroup의 Bone을 제어할 수 있다.
		// Root Node를 여러개두자.
		//------------------------------------------------------------------------------------

		private void AddBoneUnitsOfMeshGroup(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit)
		{
			if (targetMeshGroup._boneList_Root == null || targetMeshGroup._boneList_Root.Count == 0)
			{
				return;
			}

			Texture2D iconImage_Normal = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging);
			Texture2D iconImage_IKHead = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKHead);
			Texture2D iconImage_IKChained = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKChained);
			Texture2D iconImage_IKSingle = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKSingle);

			//Root 부터 재귀적으로 호출한다.
			for (int i = 0; i < targetMeshGroup._boneList_Root.Count; i++)
			{
				AddBoneUnit(targetMeshGroup._boneList_Root[i], parentUnit,
					iconImage_Normal, iconImage_IKHead, iconImage_IKChained, iconImage_IKSingle);
			}

		}

		private void AddBoneUnit(apBone bone, apEditorHierarchyUnit parentUnit,
								Texture2D iconNormal, Texture2D iconIKHead, Texture2D iconIKChained, Texture2D iconIKSingle)
		{
			Texture2D icon = iconNormal;

			switch (bone._optionIK)
			{
				case apBone.OPTION_IK.IKHead:
					icon = iconIKHead;
					break;

				case apBone.OPTION_IK.IKChained:
					icon = iconIKChained;
					break;

				case apBone.OPTION_IK.IKSingle:
					icon = iconIKSingle;
					break;
			}

			bool isModRegisted = IsModRegistered(bone);
			apEditorHierarchyUnit.VISIBLE_TYPE visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;
			if(bone.IsGUIVisible)
			{
				visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible;
			}
			apEditorHierarchyUnit addedUnit = AddUnit_ToggleButton_Visible(icon,
														bone._name,
														CATEGORY.Bone_Item,
														bone,
														false, 
														//_rootUnit_Mesh,
														parentUnit,
														visibleType,
														apEditorHierarchyUnit.VISIBLE_TYPE.None,
														isModRegisted,
														HIERARCHY_TYPE.Bones
														);

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				AddBoneUnit(bone._childBones[i], addedUnit, iconNormal, iconIKHead, iconIKChained, iconIKSingle);
			}
		}

		// Refresh (without Reset)
		//-----------------------------------------------------------------------------------------
		public void RefreshUnits()
		{
			if (Editor == null || Editor._portrait == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			List<apEditorHierarchyUnit> deletedUnits = new List<apEditorHierarchyUnit>();
			//AddUnit_ToggleButton(null, "Select Overall", CATEGORY.Overall_item, null, false, _rootUnit_Overall);


			#region [미사용 코드] 재귀적인 구조가 없는 기존 코드
			//1. 메시 들을 검색하자
			//구버전 : 단일 레벨의 Child Transform에 대해서 Refresh
			//List<apTransform_Mesh> childMeshTransforms = Editor.Select.MeshGroup._childMeshTransforms;
			//List<apTransform_MeshGroup> childMeshGroupTransforms = Editor.Select.MeshGroup._childMeshGroupTransforms;

			//for (int i = 0; i < childMeshTransforms.Count; i++)
			//{
			//	apTransform_Mesh meshTransform = childMeshTransforms[i];
			//	Texture2D iconImage = null;
			//	if(meshTransform._isClipping_Child)
			//	{
			//		iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
			//	}
			//	else
			//	{
			//		iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			//	}
			//	RefreshUnit(	CATEGORY.Mesh_Item, 
			//					iconImage, 
			//					meshTransform, 
			//					meshTransform._nickName,
			//					Editor.Select.SubMeshInGroup, 
			//					meshTransform._isVisible_Default,
			//					//_rootUnit_Mesh
			//					_rootUnit
			//					);
			//}

			//CheckRemovableUnits<apTransform_Mesh>(deletedUnits, CATEGORY.Mesh_Item, childMeshTransforms);

			////2. Mesh Group들을 검색하자
			//for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			//{
			//	apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];
			//	RefreshUnit(	CATEGORY.MeshGroup_Item, 
			//					Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup), 
			//					meshGroupTransform, 
			//					meshGroupTransform._nickName,
			//					Editor.Select.SubMeshGroupInGroup, 
			//					meshGroupTransform._isVisible_Default,
			//					//_rootUnit_MeshGroup
			//					_rootUnit
			//					);
			//}

			//CheckRemovableUnits<apTransform_MeshGroup>(deletedUnits, CATEGORY.MeshGroup_Item, childMeshGroupTransforms); 
			#endregion


			//신버전 : 재귀적으로 탐색 및 갱신을 한다.
			List<apTransform_Mesh> childMeshTransforms = new List<apTransform_Mesh>();
			List<apTransform_MeshGroup> childMeshGroupTransforms = new List<apTransform_MeshGroup>();

			SearchMeshGroupTransforms(Editor.Select.MeshGroup, _rootUnit_Meshes, childMeshTransforms, childMeshGroupTransforms);

			CheckRemovableUnits<apTransform_Mesh>(deletedUnits, CATEGORY.Mesh_Item, childMeshTransforms);
			CheckRemovableUnits<apTransform_MeshGroup>(deletedUnits, CATEGORY.MeshGroup_Item, childMeshGroupTransforms);

			//본도 Refresh한다.
			//본의 경우 Child MeshGroup Transform에 속한 Bone도 있으므로, 이 ChildMeshGroup이 현재도 유효한지 체크하는 것이 중요하다

			List<apBone> resultBones = new List<apBone>();
			List<apTransform_MeshGroup> resultMeshGroupTransformWithBones = new List<apTransform_MeshGroup>();

			//일단 메인부터
			SearchBones(Editor.Select.MeshGroup, _rootUnit_Bones_Main, resultBones);

			//서브는 Child MeshGroup Transform 부터 체크한다.
			List<apTransform_MeshGroup> childMeshGroupTransformWithBones = Editor.Select.MeshGroup._childMeshGroupTransformsWithBones;

			for (int i = 0; i < _rootUnit_Bones_Sub.Count; i++)
			{
				apEditorHierarchyUnit subBoneRootUnit = _rootUnit_Bones_Sub[i];
				apTransform_MeshGroup mgTranform = subBoneRootUnit._savedObj as apTransform_MeshGroup;
				if (mgTranform != null && mgTranform._meshGroup != null && childMeshGroupTransformWithBones.Contains(mgTranform))
				{
					//유효하게 포함되어있는 mgTransform이네염
					resultMeshGroupTransformWithBones.Add(mgTranform);
					SearchBones(mgTranform._meshGroup, subBoneRootUnit, resultBones);
				}
			}

			CheckRemovableUnits<apTransform_MeshGroup>(deletedUnits, CATEGORY.SubName_Bone, resultMeshGroupTransformWithBones);
			CheckRemovableUnits<apBone>(deletedUnits, CATEGORY.Bone_Item, resultBones);


			for (int i = 0; i < deletedUnits.Count; i++)
			{
				//1. 먼저 All에서 없앤다.
				//2. Parent가 있는경우,  Parent에서 없애달라고 한다.
				apEditorHierarchyUnit dUnit = deletedUnits[i];
				if (dUnit._parentUnit != null)
				{
					dUnit._parentUnit._childUnits.Remove(dUnit);
				}

				_units_All.Remove(dUnit);
			}

			//전체 Sort를 한다.
			//재귀적으로 실행
			for (int i = 0; i < _units_Root_Meshes.Count; i++)
			{
				SortUnit_Recv(_units_Root_Meshes[i]);
			}

			for (int i = 0; i < _units_Root_Bones.Count; i++)
			{
				SortUnit_Recv_Bones(_units_Root_Bones[i]);
			}
		}

		private apEditorHierarchyUnit RefreshUnit(CATEGORY category,
													Texture2D iconImage,
													object obj, string objName, object selectedObj,
													apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Prefix,
													apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Postfix,
													bool isModRegistered,
													apEditorHierarchyUnit parentUnit,
													HIERARCHY_TYPE hierarchyType)
		{
			apEditorHierarchyUnit unit = _units_All.Find(delegate (apEditorHierarchyUnit a)
				{
					if (obj != null)
					{
						return (CATEGORY)a._savedKey == category && a._savedObj == obj;
					}
					else
					{
						return (CATEGORY)a._savedKey == category;
					}
				});

			if (objName == null)
			{
				objName = "";
			}

			if (unit != null)
			{
				if (selectedObj != null && unit._savedObj == selectedObj)
				{
					//unit._isSelected = true;
					unit.SetSelected(true);
				}
				else
				{
					//unit._isSelected = false;
					unit.SetSelected(false);
				}

				unit.SetModRegistered(isModRegistered);

				unit._visibleType_Prefix = visibleType_Prefix;
				unit._visibleType_Postfix = visibleType_Postfix;

				if (!unit._text.Equals(objName))
				{
					unit.ChangeText(objName);
				}

				if (unit._icon != iconImage)
				{
					unit.ChangeIcon(iconImage);
				}
			}
			else
			{

				if (hierarchyType == HIERARCHY_TYPE.Meshes)
				{
					unit = AddUnit_ToggleButton_Visible(iconImage, objName, category, obj, false, parentUnit, visibleType_Prefix, visibleType_Postfix, isModRegistered, HIERARCHY_TYPE.Meshes);
				}
				else
				{
					//unit = AddUnit_ToggleButton(iconImage, objName, category, obj, false, isModRegistered, parentUnit, HIERARCHY_TYPE.Bones);
					unit = AddUnit_ToggleButton_Visible(iconImage, objName, category, obj, false, parentUnit, visibleType_Prefix, visibleType_Postfix, isModRegistered, HIERARCHY_TYPE.Bones);
					
				}
				if (selectedObj == obj)
				{
					//unit._isSelected = true;
					unit.SetSelected(true);
				}
			}
			return unit;
		}


		private void CheckRemovableUnits<T>(List<apEditorHierarchyUnit> deletedUnits, CATEGORY category, List<T> objList)
		{
			List<apEditorHierarchyUnit> deletedUnits_Sub = _units_All.FindAll(delegate (apEditorHierarchyUnit a)
			{
				if ((CATEGORY)a._savedKey == category)
				{
					if (a._savedObj == null || !(a._savedObj is T))
					{
						return true;
					}

					T savedData = (T)a._savedObj;
					if (!objList.Contains(savedData))
					{
					//리스트에 없는 경우 (무효한 경우)
					return true;
					}
				}
				return false;
			});
			for (int i = 0; i < deletedUnits_Sub.Count; i++)
			{
				deletedUnits.Add(deletedUnits_Sub[i]);
			}
		}

		/// <summary>
		/// 재귀적으로 검색을 하여 존재하는 Transform인지 찾고, Refresh 또는 제거 리스트에 넣는다.
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		/// <param name="parentUnit"></param>
		/// <param name="resultMeshTransforms"></param>
		/// <param name="resultMeshGroupTransforms"></param>
		private void SearchMeshGroupTransforms(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit, List<apTransform_Mesh> resultMeshTransforms, List<apTransform_MeshGroup> resultMeshGroupTransforms)
		{
			List<apTransform_Mesh> childMeshTransforms = targetMeshGroup._childMeshTransforms;
			List<apTransform_MeshGroup> childMeshGroupTransforms = targetMeshGroup._childMeshGroupTransforms;

			bool isModRegistered = false;
			apModifierBase modifier = Editor.Select.Modifier;

			for (int i = 0; i < childMeshTransforms.Count; i++)
			{
				apTransform_Mesh meshTransform = childMeshTransforms[i];

				Texture2D iconImage = null;
				if (meshTransform._isClipping_Child)
				{
					iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
				}
				else
				{
					iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
				}

				resultMeshTransforms.Add(meshTransform);

				isModRegistered = false;
				if (modifier != null)
				{
					isModRegistered = IsModRegistered(meshTransform);
				}


				RefreshUnit(CATEGORY.Mesh_Item,
								iconImage,
								meshTransform,
								meshTransform._nickName,
								//Editor.Select.SubMeshInGroup, 
								Editor.Select.SubMeshInGroup,
								//meshTransform._isVisible_Default,
								GetVisibleIconType(meshTransform, isModRegistered, true),
								GetVisibleIconType(meshTransform, isModRegistered, false),
								isModRegistered,
								//_rootUnit_Mesh
								parentUnit,
								HIERARCHY_TYPE.Meshes
								);
			}

			for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			{
				apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];

				resultMeshGroupTransforms.Add(meshGroupTransform);


				isModRegistered = false;
				if (modifier != null)
				{
					isModRegistered = IsModRegistered(meshGroupTransform);
				}

				apEditorHierarchyUnit existUnit = RefreshUnit(CATEGORY.MeshGroup_Item,
													Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup),
													meshGroupTransform,
													meshGroupTransform._nickName,
													//Editor.Select.SubMeshGroupInGroup, 
													Editor.Select.SubMeshGroupInGroup,
													//meshGroupTransform._isVisible_Default,
													GetVisibleIconType(meshGroupTransform, isModRegistered, true),
													GetVisibleIconType(meshGroupTransform, isModRegistered, false),
													isModRegistered,
													//_rootUnit_MeshGroup
													parentUnit,
													HIERARCHY_TYPE.Meshes

													);

				if (meshGroupTransform._meshGroup != null)
				{
					SearchMeshGroupTransforms(meshGroupTransform._meshGroup, existUnit, resultMeshTransforms, resultMeshGroupTransforms);
				}
			}
		}







		/// <summary>
		/// 재귀적으로 검색을 하여 존재하는 Bone인지 찾고, Refresh 또는 제거 리스트에 넣는다.
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		private void SearchBones(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit, List<apBone> resultBones)
		{


			List<apBone> rootBones = targetMeshGroup._boneList_Root;

			if (rootBones.Count == 0)
			{
				return;
			}

			//if(Editor.Select.Bone == null)
			//{
			//	Debug.Log("Mesh Group Hierarchy - Bone Refresh [ Not Selected ]");
			//}
			//else
			//{
			//	Debug.Log("Mesh Group Hierarchy - Bone Refresh [" + Editor.Select.Bone._name + "]");
			//}

			Texture2D iconImage_Normal = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging);
			Texture2D iconImage_IKHead = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKHead);
			Texture2D iconImage_IKChained = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKChained);
			Texture2D iconImage_IKSingle = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKSingle);

			for (int i = 0; i < rootBones.Count; i++)
			{
				apBone rootBone = rootBones[i];

				SearchAndRefreshBone(rootBone, parentUnit, resultBones, iconImage_Normal, iconImage_IKHead, iconImage_IKChained, iconImage_IKSingle);
			}
		}

		private void SearchAndRefreshBone(apBone bone, apEditorHierarchyUnit parentUnit, List<apBone> resultBones,
			Texture2D iconNormal, Texture2D iconIKHead, Texture2D iconIKChained, Texture2D iconIKSingle)
		{
			resultBones.Add(bone);

			Texture2D icon = iconNormal;
			switch (bone._optionIK)
			{
				case apBone.OPTION_IK.IKHead:
					icon = iconIKHead;
					break;

				case apBone.OPTION_IK.IKChained:
					icon = iconIKChained;
					break;

				case apBone.OPTION_IK.IKSingle:
					icon = iconIKSingle;
					break;
			}

			bool isModRegisted = IsModRegistered(bone);

			apEditorHierarchyUnit.VISIBLE_TYPE boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;
			if(bone.IsGUIVisible)
			{
				boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible;
			}

			apEditorHierarchyUnit curUnit = RefreshUnit(CATEGORY.Bone_Item,
															icon,
															bone,
															bone._name,
															//Editor.Select.SubMeshInGroup, 
															Editor.Select.Bone,
															//true,
															boneVisible,
															apEditorHierarchyUnit.VISIBLE_TYPE.None,
															isModRegisted,
															//_rootUnit_Mesh
															parentUnit,
															HIERARCHY_TYPE.Bones
															);

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				SearchAndRefreshBone(bone._childBones[i], curUnit, resultBones, iconNormal,
					iconIKHead, iconIKChained, iconIKSingle);
			}
		}









		private void SortUnit_Recv(apEditorHierarchyUnit unit)
		{
			if (unit._childUnits.Count > 0)
			{
				unit._childUnits.Sort(delegate (apEditorHierarchyUnit a, apEditorHierarchyUnit b)
				{
					if ((CATEGORY)(a._savedKey) == CATEGORY.MainName)
					{
						return 1;
					}
					if ((CATEGORY)(b._savedKey) == CATEGORY.MainName)
					{
						return -1;
					}

					int depthA = -1;
					int depthB = -1;
					int indexPerParentA = a._indexPerParent;
					int indexPerParentB = b._indexPerParent;

					if (a._savedObj is apTransform_MeshGroup)
					{
						apTransform_MeshGroup meshGroup_a = a._savedObj as apTransform_MeshGroup;
						depthA = meshGroup_a._depth;
					}
					else if (a._savedObj is apTransform_Mesh)
					{
						apTransform_Mesh mesh_a = a._savedObj as apTransform_Mesh;
						depthA = mesh_a._depth;
					}
					

					if (b._savedObj is apTransform_MeshGroup)
					{
						apTransform_MeshGroup meshGroup_b = b._savedObj as apTransform_MeshGroup;
						depthB = meshGroup_b._depth;
					}
					else if (b._savedObj is apTransform_Mesh)
					{
						apTransform_Mesh mesh_b = b._savedObj as apTransform_Mesh;
						depthB = mesh_b._depth;
					}
					

					if (depthA == depthB)
					{
						return indexPerParentA - indexPerParentB;
					}

					return depthB - depthA;

				#region [미사용 코드]

				//if(a._savedKey == b._savedKey)
				//{	
				//	if(a._savedObj is apTransform_MeshGroup && b._savedObj is apTransform_MeshGroup)
				//	{
				//		apTransform_MeshGroup meshGroup_a = a._savedObj as apTransform_MeshGroup;
				//		apTransform_MeshGroup meshGroup_b = b._savedObj as apTransform_MeshGroup;

				//		if(Mathf.Abs(meshGroup_b._depth - meshGroup_a._depth) < 0.0001f)
				//		{
				//			return a._indexPerParent - b._indexPerParent;
				//		}
				//		return (int)((meshGroup_b._depth - meshGroup_a._depth) * 1000.0f);
				//	}
				//	else if(a._savedObj is apTransform_Mesh && b._savedObj is apTransform_Mesh)
				//	{
				//		apTransform_Mesh mesh_a = a._savedObj as apTransform_Mesh;
				//		apTransform_Mesh mesh_b = b._savedObj as apTransform_Mesh;

				//		//Clip인 경우
				//		//서로 같은 Parent를 가지는 Child인 경우 -> Index의 역순
				//		//하나가 Parent인 경우 -> Parent가 아래쪽으로
				//		//그 외에는 Depth 비교
				//		if (mesh_a._isClipping_Child && mesh_b._isClipping_Child &&
				//			mesh_a._clipParentMeshTransform == mesh_b._clipParentMeshTransform)
				//		{
				//			return (mesh_b._clipIndexFromParent - mesh_a._clipIndexFromParent);
				//		}
				//		else if (	mesh_a._isClipping_Child && mesh_b._isClipping_Parent &&
				//					mesh_a._clipParentMeshTransform == mesh_b)
				//		{
				//			//b가 Parent -> b가 뒤로 가야함
				//			return -1;
				//		}
				//		else if (	mesh_b._isClipping_Child && mesh_a._isClipping_Parent &&
				//					mesh_b._clipParentMeshTransform == mesh_a)
				//		{
				//			//a가 Parent -> a가 뒤로 가야함
				//			return 1;
				//		}
				//		else
				//		{
				//			if (Mathf.Abs(mesh_b._depth - mesh_a._depth) < 0.0001f)
				//			{
				//				return a._indexPerParent - b._indexPerParent;
				//			}
				//			return (int)((mesh_b._depth - mesh_a._depth) * 1000.0f);
				//		}
				//	}
				//	return a._indexPerParent - b._indexPerParent;
				//}
				//return a._savedKey - b._savedKey; 
				#endregion
			});

				for (int i = 0; i < unit._childUnits.Count; i++)
				{
					SortUnit_Recv(unit._childUnits[i]);
				}
			}
		}


		private void SortUnit_Recv_Bones(apEditorHierarchyUnit unit)
		{
			if (unit._childUnits.Count > 0)
			{
				unit._childUnits.Sort(delegate (apEditorHierarchyUnit a, apEditorHierarchyUnit b)
				{
					if ((CATEGORY)(a._savedKey) == CATEGORY.MainName_Bone)
					{
						return 1;
					}
					if ((CATEGORY)(b._savedKey) == CATEGORY.MainName_Bone)
					{
						return -1;
					}

					int depthA = -1;
					int depthB = -1;

					if(a._savedObj is apBone)
					{
						apBone bone_a = a._savedObj as apBone;
						depthA = bone_a._depth;
					}

					if(b._savedObj is apBone)
					{
						apBone bone_b = b._savedObj as apBone;
						depthB = bone_b._depth;
					}

					if (depthA == depthB)
					{
						//그 외에는 그냥 문자열 순서로 매기자
						int compare = string.Compare(a._text, b._text);
						if (compare == 0)
						{
							return a._indexPerParent - b._indexPerParent;
						}
						return compare;
					}

					return depthB - depthA;
				});

				for (int i = 0; i < unit._childUnits.Count; i++)
				{
					SortUnit_Recv_Bones(unit._childUnits[i]);
				}
			}
		}

		// Click Event
		//-----------------------------------------------------------------------------------------
		public void OnUnitClick(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj)
		{
			if (Editor == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			apEditorHierarchyUnit selectedUnit = null;

			CATEGORY category = (CATEGORY)savedKey;
			switch (category)
			{
				case CATEGORY.Mesh_Item:
					{
						apTransform_Mesh meshTransform = savedObj as apTransform_Mesh;
						if (meshTransform != null)
						{
							Editor.Select.SetSubMeshInGroup(meshTransform);
							if (Editor.Select.SubMeshInGroup == meshTransform)
							{
								selectedUnit = eventUnit;
							}
						}
					}
					break;

				case CATEGORY.MeshGroup_Item:
					{
						apTransform_MeshGroup meshGroupTransform = savedObj as apTransform_MeshGroup;
						if (meshGroupTransform != null)
						{
							Editor.Select.SetSubMeshGroupInGroup(meshGroupTransform);
							if (Editor.Select.SubMeshGroupInGroup == meshGroupTransform)
							{
								selectedUnit = eventUnit;
							}
						}
					}
					break;

				case CATEGORY.Bone_Item:
					{
						apBone bone = savedObj as apBone;
						if (bone != null)
						{
							Editor.Select.SetBone(bone);
							if (Editor.Select.Bone == bone)
							{
								selectedUnit = eventUnit;
							}
						}
					}
					break;
			}

			if (selectedUnit != null)
			{
				for (int i = 0; i < _units_All.Count; i++)
				{
					if (_units_All[i] == selectedUnit)
					{
						//_units_All[i]._isSelected = true;
						_units_All[i].SetSelected(true);
					}
					else
					{
						//_units_All[i]._isSelected = false;
						_units_All[i].SetSelected(false);
					}
				}
			}
			else
			{
				for (int i = 0; i < _units_All.Count; i++)
				{
					//_units_All[i]._isSelected = false;
					_units_All[i].SetSelected(false);
				}
			}
		}


		public void OnUnitVisibleClick(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isVisible, bool isPrefixButton)
		{
			if (Editor == null || Editor.Select.MeshGroup == null)
			{
				return;
			}


			CATEGORY category = (CATEGORY)savedKey;

			apTransform_Mesh meshTransform = null;
			apTransform_MeshGroup meshGroupTransform = null;
			apBone bone = null;

			bool isMeshTransform = false;

			bool isVisibleDefault = false;

			bool isCtrl = false;
			if(Event.current != null)
			{
#if UNITY_EDITOR_OSX
				isCtrl = Event.current.command;
#else
				isCtrl = Event.current.control;
#endif		
			}
			

			switch (category)
			{
				case CATEGORY.Mesh_Item:
					{
						meshTransform = savedObj as apTransform_Mesh;
						isVisibleDefault = meshTransform._isVisible_Default;
						isMeshTransform = true;
					}
					break;

				case CATEGORY.MeshGroup_Item:
					{
						meshGroupTransform = savedObj as apTransform_MeshGroup;
						isVisibleDefault = meshGroupTransform._isVisible_Default;
						isMeshTransform = false;
					}
					break;

				case CATEGORY.Bone_Item:
					{
						bone = savedObj as apBone;
						isVisibleDefault = bone.IsGUIVisible;
					}
					break;
				default:
					return;
			}
			if (meshTransform == null && meshGroupTransform == null && bone == null)
			{
				//?? 뭘 선택했나염..
				return;
			}


			if (category == CATEGORY.Mesh_Item || category == CATEGORY.MeshGroup_Item)
			{
				if (isPrefixButton)
				{
					//Prefix : TmpWorkVisible을 토글한다.
					apRenderUnit linkedRenderUnit = null;

					if (meshTransform != null)
					{
						linkedRenderUnit = meshTransform._linkedRenderUnit;
					}
					else if (meshGroupTransform != null)
					{
						linkedRenderUnit = meshGroupTransform._linkedRenderUnit;
					}

					if (linkedRenderUnit != null)
					{
						bool isTmpWorkToShow = false;
						//Editor.Select.MeshGroup.
						if (linkedRenderUnit._isVisible_WithoutParent == linkedRenderUnit._isVisibleCalculated)
						{

							//TmpWork가 꺼져있다. (실제 Visible과 값이 같다)
							if (linkedRenderUnit._isVisible_WithoutParent)
							{
								//Show -> Hide
								linkedRenderUnit._isVisibleWorkToggle_Show2Hide = true;
								linkedRenderUnit._isVisibleWorkToggle_Hide2Show = false;

								isTmpWorkToShow = false;
							}
							else
							{
								//Hide -> Show
								linkedRenderUnit._isVisibleWorkToggle_Show2Hide = false;
								linkedRenderUnit._isVisibleWorkToggle_Hide2Show = true;

								isTmpWorkToShow = true;
							}
						}
						else
						{
							//TmpWork가 켜져있다. 꺼야한다. (같은 값으로 바꾼다)
							linkedRenderUnit._isVisibleWorkToggle_Show2Hide = false;
							linkedRenderUnit._isVisibleWorkToggle_Hide2Show = false;

							isTmpWorkToShow = !linkedRenderUnit._isVisible_WithoutParent;
						}

						if (isCtrl)
						{
							//Ctrl을 눌렀으면 반대로 행동
							//Debug.Log("TmpWork를 다른 RenderUnit에 반대로 적용 : Show : " + !isTmpWorkToShow);
							Editor.Controller.SetMeshGroupTmpWorkVisibleAll(Editor.Select.MeshGroup, !isTmpWorkToShow, linkedRenderUnit);
						}
					}


					//그냥 Refresh
					Editor.Select.MeshGroup.RefreshForce();

					Editor.RefreshControllerAndHierarchy();
				}
				else
				{
					//Postfix :
					//Setting : isVisibleDefault를 토글한다.
					//Modifier 선택중 + ParamSetGroup을 선택하고 있다. : ModMesh의 isVisible을 변경한다.
					bool isModVisibleSetting = Editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup
													&& Editor.Select.Modifier != null
													&& Editor.Select.SubEditedParamSetGroup != null
													&& Editor.Select.ParamSetOfMod != null;


					if (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Setting)
					{
						//Setting 탭에서는 Default Visible을 토글한다.
						apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, Editor, Editor.Select.MeshGroup, savedObj, false, true);

						if (meshTransform != null)
						{
							meshTransform._isVisible_Default = !meshTransform._isVisible_Default;
						}
						else if (meshGroupTransform != null)
						{
							meshGroupTransform._isVisible_Default = !meshGroupTransform._isVisible_Default;
						}

						Editor.Select.MeshGroup.RefreshForce();
						Editor.RefreshControllerAndHierarchy();
					}
					else if (isModVisibleSetting)
					{

						//Mod Visible을 조절한다.
						//가능한지 여부 체크
						if ((int)(Editor.Select.Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.Color) != 0 &&
								Editor.Select.Modifier._isColorPropertyEnabled)
						{

							//apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor.Select.MeshGroup, savedObj, false, Editor);

							//MeshGroup이 아닌 Modifier를 저장할것
							apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Editor.Select.Modifier, null, false);

							//색상을 지원한다면
							//현재 객체를 선택하고,
							//ModMesh가 있는지 확인
							//없으면 -> 추가 후 받아온다.
							//ModMesh의 isVisible 값을 지정한다.

							apModifiedMesh targetModMesh = null;


							if (meshTransform != null)
							{
								Editor.Select.SetSubMeshInGroup(meshTransform);
							}
							else
							{
								Editor.Select.SetSubMeshGroupInGroup(meshGroupTransform);
							}



							Editor.Select.AutoSelectModMeshOrModBone();

							targetModMesh = Editor.Select.ModMeshOfMod;

							if (targetModMesh == null)
							{
								//ModMesh가 등록이 안되어있다면
								//추가를 시도한다.

								Editor.Controller.AddModMesh_WithSubMeshOrSubMeshGroup();

								//여기서 주의 : "현재 ParamSet"의 ModMesh가 아니라 "모든 ParamSet"의 ModMesh에 대해서 처리를 해야한다.
								List<apModifierParamSet> paramSets = Editor.Select.SubEditedParamSetGroup._paramSetList;
								for (int i = 0; i < paramSets.Count; i++)
								{
									apModifierParamSet paramSet = paramSets[i];
									apModifiedMesh addedMesh = paramSet._meshData.Find(delegate (apModifiedMesh a)
									{
										if (isMeshTransform)
										{
											return a._transform_Mesh == meshTransform;
										}
										else
										{
											return a._transform_MeshGroup == meshGroupTransform;
										}
									});
									if (addedMesh != null)
									{
										addedMesh._isVisible = !isVisibleDefault;
									}
								}

								targetModMesh = Editor.Select.ModMeshOfMod;
								if (targetModMesh == null)
								{
									return;
								}
								targetModMesh._isVisible = !isVisibleDefault;

							}
							else
							{
								//Visible을 변경한다.
								targetModMesh._isVisible = !targetModMesh._isVisible;
							}

							Editor.Select.MeshGroup.RefreshForce();
							Editor.RefreshControllerAndHierarchy();


						}
					}
				}
			}
			else if(category == CATEGORY.Bone_Item)
			{
				if(isPrefixButton)
				{
					//Bone의 GUI Visible을 토글한다.
					if(isCtrl)
					{
						Editor.Select.MeshGroup.SetBoneGUIVisibleAll(isVisibleDefault, bone);
					}
					else
					{
						bone.SetGUIVisible(!isVisibleDefault, true);
					}
					
					Editor.RefreshControllerAndHierarchy();
				}
			}
		}


		//------------------------------------------------------------------------------------------------------------
		// Modifier에 등록되었는지 체크
		//------------------------------------------------------------------------------------------------------------
		private bool IsModRegistered(apTransform_Mesh meshTransform)
		{
			apModifierBase modifier = Editor.Select.Modifier;
			if (modifier == null)
			{
				return false;
			}

			if (modifier.IsAnimated)
			{
				//타임라인 기준으로 처리하자
				if (Editor.Select.AnimTimeline != null)
				{
					return Editor.Select.AnimTimeline.IsObjectAddedInLayers(meshTransform);
				}
			}
			else
			{
				//현재 선택한 Modifier의 ParamSetGroup에 포함되어있는가
				if (Editor.Select.SubEditedParamSetGroup != null)
				{
					return Editor.Select.SubEditedParamSetGroup.IsMeshTransformContain(meshTransform);
				}
			}

			return false;
		}



		private bool IsModRegistered(apTransform_MeshGroup meshGroupTransform)
		{
			apModifierBase modifier = Editor.Select.Modifier;
			if (modifier == null)
			{
				return false;
			}

			if (modifier.IsAnimated)
			{
				//타임라인 기준으로 처리하자
				if (Editor.Select.AnimTimeline != null)
				{
					return Editor.Select.AnimTimeline.IsObjectAddedInLayers(meshGroupTransform);
				}
			}
			else
			{
				//현재 선택한 Modifier의 ParamSetGroup에 포함되어있는가
				if (Editor.Select.SubEditedParamSetGroup != null)
				{
					return Editor.Select.SubEditedParamSetGroup.IsMeshGroupTransformContain(meshGroupTransform);
				}
			}

			return false;
		}


		private bool IsModRegistered(apBone bone)
		{
			apModifierBase modifier = Editor.Select.Modifier;
			if (modifier == null)
			{
				return false;
			}

			if (modifier.IsAnimated)
			{
				//타임라인 기준으로 처리하자
				if (Editor.Select.AnimTimeline != null)
				{
					return Editor.Select.AnimTimeline.IsObjectAddedInLayers(bone);
				}
			}
			else
			{
				//현재 선택한 Modifier의 ParamSetGroup에 포함되어있는가
				if (Editor.Select.SubEditedParamSetGroup != null)
				{
					return Editor.Select.SubEditedParamSetGroup.IsBoneContain(bone);
				}
			}

			return false;
		}


		private apEditorHierarchyUnit.VISIBLE_TYPE GetVisibleIconType(object targetObject, bool isModRegistered, bool isPrefix)
		{

			apRenderUnit linkedRenderUnit = null;
			apTransform_Mesh meshTransform = null;
			apTransform_MeshGroup meshGroupTransform = null;
			if (targetObject is apTransform_Mesh)
			{
				meshTransform = targetObject as apTransform_Mesh;
				linkedRenderUnit = meshTransform._linkedRenderUnit;
			}
			else if (targetObject is apTransform_MeshGroup)
			{
				meshGroupTransform = targetObject as apTransform_MeshGroup;
				linkedRenderUnit = meshGroupTransform._linkedRenderUnit;
			}
			else
			{
				return apEditorHierarchyUnit.VISIBLE_TYPE.None;
			}


			if (linkedRenderUnit == null)
			{
				return apEditorHierarchyUnit.VISIBLE_TYPE.None;
			}


			if (isPrefix)
			{
				//Prefix는
				//1) RenderUnit의 현재 렌더링 상태 -> Current
				//2) MeshTransform/MeshGroupTransform의 
				bool isVisible = linkedRenderUnit._isVisible_WithoutParent;//Visible이 아닌 VisibleParent를 출력한다.
																		   //TmpWork에 의해서 Visible 값이 바뀌는가)
																		   //Calculate != WOParent 인 경우 (TmpWork의 영향을 받았다)
				if (linkedRenderUnit._isVisibleCalculated != linkedRenderUnit._isVisible_WithoutParent)
				{
					if (isVisible)
					{ return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible; }
					else
					{ return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible; }
				}
				else
				{
					//TmpWork의 영향을 받지 않았다.
					if (isVisible)
					{ return apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible; }
					else
					{ return apEditorHierarchyUnit.VISIBLE_TYPE.Current_NonVisible; }
				}
			}
			else
			{
				//PostFix는
				//1) MeshGroup Setting에서는 Default를 표시하고,
				//2) Modifier/AnimClip 상태에서 ModMesh가 발견 되었을때 ModKey 상태를 출력한다.
				//아무것도 아닐때는 None 리턴
				if (_editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Setting)
				{
					if (meshTransform != null)
					{
						if (meshTransform._isVisible_Default)
						{ return apEditorHierarchyUnit.VISIBLE_TYPE.Default_Visible; }
						else
						{ return apEditorHierarchyUnit.VISIBLE_TYPE.Default_NonVisible; }
					}
					else if (meshGroupTransform != null)
					{
						if (meshGroupTransform._isVisible_Default)
						{ return apEditorHierarchyUnit.VISIBLE_TYPE.Default_Visible; }
						else
						{ return apEditorHierarchyUnit.VISIBLE_TYPE.Default_NonVisible; }
					}
				}
				else if (_editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Modifier)
				{
					if (_editor.Select.Modifier != null && _editor.Select.SubEditedParamSetGroup != null)
					{
						//Modifier가 Color를 지원하는 경우
						if (_editor.Select.Modifier._isColorPropertyEnabled
							&& (int)(_editor.Select.Modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0
							&& _editor.Select.SubEditedParamSetGroup._isColorPropertyEnabled)
						{
							if (_editor.Select.ParamSetOfMod != null)
							{
								if (isModRegistered)
								{
									apModifiedMesh modMesh = _editor.Select.ParamSetOfMod._meshData.Find(delegate (apModifiedMesh a)
									{
										return a._transform_Mesh == meshTransform;
									});
									//if (linkedRenderUnit._isVisible_WithoutParent != meshTransform._isVisible_Default)
									if (modMesh != null)
									{
										//Mod 아이콘
										if (modMesh._isVisible)
										{ return apEditorHierarchyUnit.VISIBLE_TYPE.ModKey_Visible; }
										else
										{ return apEditorHierarchyUnit.VISIBLE_TYPE.ModKey_NonVisible; }
									}
									else
									{
										//ModMesh가 없다면 => NoKey를 리턴한다.
										return apEditorHierarchyUnit.VISIBLE_TYPE.NoKey;//<<키가 등록 안되어 있네염
									}
								}
								else
								{
									//Mod에 등록이 안되어도 NoKey 출력
									return apEditorHierarchyUnit.VISIBLE_TYPE.NoKey;
								}
							}
						}
					}
				}

			}

			return apEditorHierarchyUnit.VISIBLE_TYPE.None;
		}




		// GUI
		//---------------------------------------------
		//Hierarchy 레이아웃 출력
		public void GUI_RenderHierarchy(int width, bool isMeshHierarchy, float scrollX, bool isGUIEvent)
		{
			if (isMeshHierarchy)
			{
				//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
				for (int i = 0; i < _units_Root_Meshes.Count; i++)
				{
					GUI_RenderUnit(_units_Root_Meshes[i], 0, width, scrollX, isGUIEvent);
					GUILayout.Space(10);
				}
			}
			else
			{
				//Bone 루트를 출력합시다.
				//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
				for (int i = 0; i < _units_Root_Bones.Count; i++)
				{
					GUI_RenderUnit(_units_Root_Bones[i], 0, width, scrollX, isGUIEvent);
					GUILayout.Space(10);
				}
			}

		}

		//재귀적으로 Hierarchy 레이아웃을 출력
		//Child에 진입할때마다 Level을 높인다. (여백과 Fold의 기준이 됨)
		private void GUI_RenderUnit(apEditorHierarchyUnit unit, int level, int width, float scrollX, bool isGUIEvent)
		{
			unit.GUI_Render(level * 10, width, 20, scrollX, isGUIEvent);
			//if (unit._isFoldOut)
			if (unit.IsFoldOut)
			{
				if (unit._childUnits.Count > 0)
				{
					for (int i = 0; i < unit._childUnits.Count; i++)
					{
						//재귀적으로 호출
						GUI_RenderUnit(unit._childUnits[i], level + 1, width, scrollX, isGUIEvent);
					}
				}
			}
		}
	}

}