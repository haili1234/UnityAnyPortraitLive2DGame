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

	public class apEditorAnimClipTargetHierarchy
	{
		// Members
		//----------------------------------------------------------
		private apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }


		public enum HIERARCHY_TYPE
		{
			Transform,
			Bone,
			ControlParam
		}

		public enum CATEGORY
		{
			MainName,
			Mesh_Item,
			MeshGroup_Item,
			Bone_Item,
			ControlParam
		}


		//private HIERARCHY_TYPE _hierarchyType = HIERARCHY_TYPE.Transform;

		//루트들만 따로 적용
		//private apEditorHierarchyUnit _rootUnit_Mesh = null;
		//private apEditorHierarchyUnit _rootUnit_MeshGroup = null;

		private apEditorHierarchyUnit _rootUnit_Transform = null;
		private apEditorHierarchyUnit _rootUnit_Bone = null;
		private apEditorHierarchyUnit _rootUnit_ControlParam = null;

		public List<apEditorHierarchyUnit> _units_All = new List<apEditorHierarchyUnit>();
		public List<apEditorHierarchyUnit> _units_Root_Transform = new List<apEditorHierarchyUnit>();
		public List<apEditorHierarchyUnit> _units_Root_Bone = new List<apEditorHierarchyUnit>();
		public List<apEditorHierarchyUnit> _units_Root_ControlParam = new List<apEditorHierarchyUnit>();

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
		public apEditorAnimClipTargetHierarchy(apEditor editor)
		{
			_editor = editor;



		}


		private void ReloadGUIContent()
		{
			if (_editor == null)
			{
				return;
			}
			if (_guiContent_Visible_Current == null)	{ _guiContent_Visible_Current = new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Current)); }
			if (_guiContent_NonVisible_Current == null) { _guiContent_NonVisible_Current = new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Current)); }
			if (_guiContent_Visible_TmpWork == null)	{ _guiContent_Visible_TmpWork = new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_TmpWork)); }
			if (_guiContent_NonVisible_TmpWork == null) { _guiContent_NonVisible_TmpWork = new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_TmpWork)); }
			if (_guiContent_Visible_Default == null)	{ _guiContent_Visible_Default = new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Default)); }
			if (_guiContent_NonVisible_Default == null) { _guiContent_NonVisible_Default = new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Default)); }
			if (_guiContent_Visible_ModKey == null)		{ _guiContent_Visible_ModKey = new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_ModKey)); }
			if (_guiContent_NonVisible_ModKey == null)	{ _guiContent_NonVisible_ModKey = new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_ModKey)); }
			if (_guiContent_NoKey == null)				{ _guiContent_NoKey = new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NoKey)); }
		}

		public void ResetSubUnits()
		{
			_units_All.Clear();
			_units_Root_Transform.Clear();
			_units_Root_Bone.Clear();
			_units_Root_ControlParam.Clear();

			_rootUnit_Transform = null;
			_rootUnit_Bone = null;
			_rootUnit_ControlParam = null;

			if (Editor == null || Editor._portrait == null || Editor.Select.AnimClip == null)
			{
				return;
			}




			//1. Linked MeshGroup의 Child List부터 만들어주자
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			if (targetMeshGroup == null)
			{
				//MeshGroup이 연결이 안되었네용
				_rootUnit_Transform = AddUnit_Label(null, "[No MeshGroup]", CATEGORY.MainName, null, null);
			}
			else
			{
				//MeshGroup에 연결해서 세팅
				string meshGroupName = targetMeshGroup._name;
				if (meshGroupName.Length > 16)
				{
					meshGroupName = meshGroupName.Substring(0, 14) + "..";
				}
				_rootUnit_Transform = AddUnit_Label(null, meshGroupName, CATEGORY.MainName, null, null);

				AddTransformOfMeshGroup(targetMeshGroup, _rootUnit_Transform);
			}

			//2. Bone List를 만들자
			if (targetMeshGroup == null)
			{
				_rootUnit_Bone = AddUnit_Label(null, "[No MeshGroup]", CATEGORY.MainName, null, null);
			}
			else
			{
				//"Bones"
				_rootUnit_Bone = AddUnit_Label(null, _editor.GetUIWord(UIWORD.Bones), CATEGORY.MainName, null, null);
				
				//_units_Root_Bone.Add(addedUnit);
				AddBoneUnitsOfMeshGroup(targetMeshGroup, _rootUnit_Bone);
			}


			//3. Control Param 리스트를 만들자
			//"Control Parameters"
			_rootUnit_ControlParam = AddUnit_Label(null, _editor.GetUIWord(UIWORD.ControlParameters), CATEGORY.MainName, null, null);

			//Texture2D iconImage_Control = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param);

			List<apControlParam> controlParams = Editor._portrait._controller._controlParams;
			for (int i = 0; i < controlParams.Count; i++)
			{
				apControlParam curParam = controlParams[i];

				AddUnit_ToggleButton(
												//iconImage_Control,
												Editor.ImageSet.Get(apEditorUtil.GetControlParamPresetIconType(curParam._iconPreset)),
												curParam._keyName,
												CATEGORY.ControlParam,
												curParam,
												IsModRegistered(curParam),
												_rootUnit_ControlParam);
			}

			_units_Root_Transform.Add(_rootUnit_Transform);
			_units_Root_Bone.Add(_rootUnit_Bone);
			_units_Root_ControlParam.Add(_rootUnit_ControlParam);
		}


		private void AddTransformOfMeshGroup(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit)
		{
			List<apTransform_Mesh> childMeshTransforms = targetMeshGroup._childMeshTransforms;
			List<apTransform_MeshGroup> childMeshGroupTransforms = targetMeshGroup._childMeshGroupTransforms;

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

				bool isModRegistered = IsModRegistered(meshTransform);

				AddUnit_ToggleButton_Visible(iconImage,
														meshTransform._nickName,
														CATEGORY.Mesh_Item,
														meshTransform,
														false,
														parentUnit,
														GetVisibleIconType(meshTransform, isModRegistered, true),
														GetVisibleIconType(meshTransform, isModRegistered, false),
														isModRegistered
														//_rootUnit_Mesh,

														);


			}

			for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			{
				apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];

				bool isModRegistered = IsModRegistered(meshGroupTransform);

				apEditorHierarchyUnit newUnit = AddUnit_ToggleButton_Visible(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup),
													meshGroupTransform._nickName,
													CATEGORY.MeshGroup_Item,
													meshGroupTransform,
													false,
													parentUnit,
													GetVisibleIconType(meshGroupTransform, isModRegistered, true),
													GetVisibleIconType(meshGroupTransform, isModRegistered, false),
													isModRegistered
													//_rootUnit_MeshGroup,
													);


				if (meshGroupTransform._meshGroup != null)
				{
					AddTransformOfMeshGroup(meshGroupTransform._meshGroup, newUnit);
				}
			}
		}

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

			//자식 객체도 호출해주자
			for (int i = 0; i < targetMeshGroup._childMeshGroupTransforms.Count; i++)
			{
				apMeshGroup meshGroup = targetMeshGroup._childMeshGroupTransforms[i]._meshGroup;
				if (meshGroup != null)
				{
					AddBoneUnitsOfMeshGroup(meshGroup, parentUnit);
				}
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
														isModRegisted
														);

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				AddBoneUnit(bone._childBones[i], addedUnit, iconNormal, iconIKHead, iconIKChained, iconIKSingle);
			}
		}



		// Functions
		//------------------------------------------------------------------------

		private apEditorHierarchyUnit AddUnit_Label(Texture2D icon, string text, CATEGORY savedKey, object savedObj, apEditorHierarchyUnit parent)
		{
			apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
										Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
										Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));
			newUnit.SetEvent(OnUnitClick);
			newUnit.SetLabel(icon, text, (int)savedKey, savedObj);
			newUnit.SetModRegistered(false);

			_units_All.Add(newUnit);

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
															bool isModRegistered,
															apEditorHierarchyUnit parent)
		{
			apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
										Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
										Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));
			newUnit.SetEvent(OnUnitClick, OnUnitVisibleClick);
			newUnit.SetToggleButton(icon, text, (int)savedKey, savedObj);
			newUnit.SetModRegistered(isModRegistered);

			_units_All.Add(newUnit);

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
																		bool isModRegisted
																		)
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


			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}
			return newUnit;
		}


		// Refresh (without Reset)
		//-----------------------------------------------------------------------------------------
		public void RefreshUnits()
		{
			if (Editor == null || Editor._portrait == null || Editor.Select.AnimClip == null)
			{
				return;
			}

			List<apEditorHierarchyUnit> deletedUnits = new List<apEditorHierarchyUnit>();
			//AddUnit_ToggleButton(null, "Select Overall", CATEGORY.Overall_item, null, false, _rootUnit_Overall);

			//1. Transform에 대해서 Refresh를 하자
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			if (targetMeshGroup == null)
			{
				//MeshGroup이 없네염
				//기존 Transform 관련 데이터 다 날려야함 + Bone도
			}
			else
			{
				//재귀적으로 만든 경우는 단일 리스트로 만들기 힘들다.
				//단일 리스트 조회 후 한번에 처리해야한다. (근데 Refresh는 또 재귀 호출일세 ㅜㅜ)

				List<apTransform_Mesh> childMeshTransforms = new List<apTransform_Mesh>();
				List<apTransform_MeshGroup> childMeshGroupTransforms = new List<apTransform_MeshGroup>();

				SearchMeshGroupTransforms(targetMeshGroup, _rootUnit_Transform, childMeshTransforms, childMeshGroupTransforms);

				CheckRemovableUnits<apTransform_Mesh>(deletedUnits, CATEGORY.Mesh_Item, childMeshTransforms);
				CheckRemovableUnits<apTransform_MeshGroup>(deletedUnits, CATEGORY.MeshGroup_Item, childMeshGroupTransforms);
			}

			List<apBone> resultBones = new List<apBone>();

			//2. Bone에 대해서 Refresh를 하자
			if (targetMeshGroup != null)
			{
				SearchBones(targetMeshGroup, _rootUnit_Bone, resultBones);
			}

			CheckRemovableUnits<apBone>(deletedUnits, CATEGORY.Bone_Item, resultBones);


			//3. Control Param에 대해서 Refresh를 하자
			List<apControlParam> controlParams = Editor._portrait._controller._controlParams;
			//Texture2D iconImage_Control = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param);

			for (int i = 0; i < controlParams.Count; i++)
			{
				apControlParam curParam = controlParams[i];

				bool isModRegistered = IsModRegistered(curParam);
				RefreshUnit(CATEGORY.ControlParam,
								//iconImage_Control, 
								Editor.ImageSet.Get(apEditorUtil.GetControlParamPresetIconType(curParam._iconPreset)),
								curParam,
								curParam._keyName,
								Editor.Select.SubControlParamOnAnimClip,
								apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible,
								apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible,
								isModRegistered,
								//_rootUnit_Mesh
								_rootUnit_ControlParam
								);
			}

			CheckRemovableUnits<apControlParam>(deletedUnits, CATEGORY.ControlParam, controlParams);




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
			for (int i = 0; i < _units_Root_Transform.Count; i++)
			{
				SortUnit_Recv(_units_Root_Transform[i]);
			}

			for (int i = 0; i < _units_Root_Bone.Count; i++)
			{
				SortUnit_Recv(_units_Root_Bone[i]);
			}

			for (int i = 0; i < _units_Root_ControlParam.Count; i++)
			{
				SortUnit_Recv(_units_Root_ControlParam[i]);
			}
		}

		private void SearchMeshGroupTransforms(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit, List<apTransform_Mesh> resultMeshTransforms, List<apTransform_MeshGroup> resultMeshGroupTransforms)
		{
			List<apTransform_Mesh> childMeshTransforms = targetMeshGroup._childMeshTransforms;
			List<apTransform_MeshGroup> childMeshGroupTransforms = targetMeshGroup._childMeshGroupTransforms;

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

				bool isModRegistered = IsModRegistered(meshTransform);
				RefreshUnit(CATEGORY.Mesh_Item,
								iconImage,
								meshTransform,
								meshTransform._nickName,
								//Editor.Select.SubMeshInGroup, 
								Editor.Select.SubMeshTransformOnAnimClip,
								GetVisibleIconType(meshTransform, isModRegistered, true),
								GetVisibleIconType(meshTransform, isModRegistered, false),
								isModRegistered,
								//_rootUnit_Mesh
								parentUnit
								);
			}

			for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			{
				apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];

				resultMeshGroupTransforms.Add(meshGroupTransform);

				bool isModRegistered = IsModRegistered(meshGroupTransform);
				apEditorHierarchyUnit existUnit = RefreshUnit(CATEGORY.MeshGroup_Item,
													Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup),
													meshGroupTransform,
													meshGroupTransform._nickName,
													//Editor.Select.SubMeshGroupInGroup, 
													Editor.Select.SubMeshGroupTransformOnAnimClip,
													GetVisibleIconType(meshGroupTransform, isModRegistered, true),
													GetVisibleIconType(meshGroupTransform, isModRegistered, false),
													isModRegistered,
													//_rootUnit_MeshGroup
													parentUnit
													);

				if (meshGroupTransform._meshGroup != null)
				{
					SearchMeshGroupTransforms(meshGroupTransform._meshGroup, existUnit, resultMeshTransforms, resultMeshGroupTransforms);
				}
			}
		}


		private void SearchBones(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit, List<apBone> resultBones)
		{
			List<apBone> rootBones = targetMeshGroup._boneList_Root;

			Texture2D iconImage_Normal = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging);
			Texture2D iconImage_IKHead = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKHead);
			Texture2D iconImage_IKChained = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKChained);
			Texture2D iconImage_IKSingle = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKSingle);

			for (int i = 0; i < rootBones.Count; i++)
			{
				SearchAndRefreshBone(rootBones[i], parentUnit, resultBones, iconImage_Normal, iconImage_IKHead, iconImage_IKChained, iconImage_IKSingle);
			}

			//Child Mesh도 체크한다.
			for (int i = 0; i < targetMeshGroup._childMeshGroupTransforms.Count; i++)
			{
				apMeshGroup childMeshGroup = targetMeshGroup._childMeshGroupTransforms[i]._meshGroup;
				if (childMeshGroup != null)
				{
					if (childMeshGroup._boneList_Root.Count > 0)
					{
						for (int iRootBone = 0; iRootBone < childMeshGroup._boneList_Root.Count; iRootBone++)
						{
							SearchAndRefreshBone(childMeshGroup._boneList_Root[iRootBone], parentUnit, resultBones, iconImage_Normal, iconImage_IKHead, iconImage_IKChained, iconImage_IKSingle);
						}
					}

				}
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

			apEditorHierarchyUnit.VISIBLE_TYPE visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;
			if(bone.IsGUIVisible)
			{
				visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible;
			}

			apEditorHierarchyUnit curUnit = RefreshUnit(CATEGORY.Bone_Item,
															icon,
															bone,
															bone._name,
															//Editor.Select.SubMeshInGroup, 
															Editor.Select.Bone,
															visibleType,
															apEditorHierarchyUnit.VISIBLE_TYPE.None,
															isModRegisted,
															//_rootUnit_Mesh
															parentUnit
															);

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				SearchAndRefreshBone(bone._childBones[i], curUnit, resultBones, iconNormal,
					iconIKHead, iconIKChained, iconIKSingle);
			}
		}




		private apEditorHierarchyUnit RefreshUnit(CATEGORY category,
													Texture2D iconImage,
													object obj, string objName, object selectedObj,
													apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Pre,
													apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Post,
													bool isModRegistered,
													apEditorHierarchyUnit parentUnit)
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

				//unit._isVisible = isVisible;
				unit._visibleType_Prefix = visibleType_Pre;
				unit._visibleType_Postfix = visibleType_Post;

				if (!unit._text.Equals(objName))
				{
					unit.ChangeText(objName);
				}

				if (unit._icon != iconImage)
				{
					unit.ChangeIcon(iconImage);
				}

				unit.SetModRegistered(isModRegistered);
			}
			else
			{
				if (category == CATEGORY.Mesh_Item || category == CATEGORY.MeshGroup_Item)
				{
					unit = AddUnit_ToggleButton_Visible(iconImage, objName, category, obj, false, parentUnit, visibleType_Pre, visibleType_Post, isModRegistered);
				}
				else
				{
					//unit = AddUnit_ToggleButton(iconImage, objName, category, obj, isModRegistered, parentUnit);
					unit = AddUnit_ToggleButton_Visible(iconImage, objName, category, obj, false, parentUnit, visibleType_Pre, visibleType_Post, isModRegistered);
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


		private void SortUnit_Recv(apEditorHierarchyUnit unit)
		{
			if (unit._childUnits.Count > 0)
			{
				unit._childUnits.Sort(delegate (apEditorHierarchyUnit a, apEditorHierarchyUnit b)
				{
					int depthA = -1;
					int depthB = -1;
					int indexPerParentA = a._indexPerParent;
					int indexPerParentB = b._indexPerParent;

					if ((CATEGORY)(a._savedKey) == CATEGORY.MainName)
					{
						return 1;
					}
					if ((CATEGORY)(b._savedKey) == CATEGORY.MainName)
					{
						return -1;
					}
					if ((CATEGORY)(a._savedKey) == CATEGORY.ControlParam && (CATEGORY)(b._savedKey) == CATEGORY.ControlParam)
					{
						apControlParam cpA = a._savedObj as apControlParam;
						apControlParam cpB = b._savedObj as apControlParam;
						return string.Compare(cpA._keyName, cpB._keyName);
					}
					if ((CATEGORY)(a._savedKey) == CATEGORY.Bone_Item && (CATEGORY)(b._savedKey) == CATEGORY.Bone_Item)
					{
						apBone bone_a = a._savedObj as apBone;
						apBone bone_b = b._savedObj as apBone;

						depthA = bone_a._depth;
						depthB = bone_b._depth;

						if (depthA == depthB)
						{
							int compare = string.Compare(a._text, b._text);
							if (compare == 0)
							{
								return a._indexPerParent - b._indexPerParent;
							}
							return compare;
						}
					}


					

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

		// Click Event
		//-----------------------------------------------------------------------------------------
		public void OnUnitClick(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj)
		{
			if (Editor == null || Editor.Select.AnimClip == null)
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
							Editor.Select.SetSubMeshTransformForAnimClipEdit(meshTransform);
							if (Editor.Select.SubMeshTransformOnAnimClip == meshTransform)
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
							Editor.Select.SetSubMeshGroupTransformForAnimClipEdit(meshGroupTransform);
							if (Editor.Select.SubMeshGroupTransformOnAnimClip == meshGroupTransform)
							{
								selectedUnit = eventUnit;
							}
						}
					}
					break;

				case CATEGORY.ControlParam:
					{
						apControlParam controlParam = savedObj as apControlParam;
						if (controlParam != null)
						{
							Editor.Select.SetSubControlParamForAnimClipEdit(controlParam);
							if (Editor.Select.SubControlParamOnAnimClip == controlParam)
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
							Editor.Select.SetBoneForAnimClip(bone);
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
			if (Editor == null || Editor.Select.AnimClip == null)
			{
				return;
			}


			CATEGORY category = (CATEGORY)savedKey;

			apTransform_Mesh meshTransform = null;
			apTransform_MeshGroup meshGroupTransform = null;
			apBone bone = null;

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
					}
					break;

				case CATEGORY.MeshGroup_Item:
					{
						meshGroupTransform = savedObj as apTransform_MeshGroup;
						isVisibleDefault = meshGroupTransform._isVisible_Default;
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

			//수정
			//Prefix : TmpWorkVisible을 토글한다.
			if (category == CATEGORY.Mesh_Item || category == CATEGORY.MeshGroup_Item)
			{
				if (isPrefixButton)
				{
					//TmpWorkVisible을 토글하자
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
						if (linkedRenderUnit._isVisible_WithoutParent == linkedRenderUnit._isVisibleCalculated)
						{
							//TmpWork가 꺼져있다.
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
							//TmpWork가 켜져있다. 꺼야한다.
							linkedRenderUnit._isVisibleWorkToggle_Show2Hide = false;
							linkedRenderUnit._isVisibleWorkToggle_Hide2Show = false;

							isTmpWorkToShow = !linkedRenderUnit._isVisible_WithoutParent;
						}

						if (isCtrl)
						{
							//Ctrl을 눌렀으면 반대로 행동
							//Debug.Log("TmpWork를 다른 RenderUnit에 반대로 적용 : Show : " + !isTmpWorkToShow);
							Editor.Controller.SetMeshGroupTmpWorkVisibleAll(Editor.Select.AnimClip._targetMeshGroup, !isTmpWorkToShow, linkedRenderUnit);
						}
					}
				}
				else
				{
					//Postfix :
					//여기서는 AnimClip에서는 isVisibleDefault를 수정하지는 않는다.
					//Visible을 눌렀을때
					//Timeline이 있다면 + AnimatedModifier 타입일때 + 그 Modifier가 Color를 지원할 때
					//Layer가 등록되었는가?
					//>> 1. Layer가 등록이 되었다.
					//       >>> 1-1. 재생상태의 현재 키프레임이 있다. -> 현재 프레임을 찾고 Visible을 세팅한다.
					//       >>> 1-2. 현재 키 프레임이 없는 곳이다. -> 키프레임을 추가하고 Visible을 세팅한다.
					//>> 2. Layer가 등록이 되지 않았다.
					//       -> 레이어를 등록하고, 맨 앞프레임에 키프레임을 추가하여 Visible 값을 넣는다.

					bool isVisibleChangable = Editor.Select.AnimTimeline != null &&
						Editor.Select.AnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier &&
						Editor.Select.AnimTimeline._linkedModifier != null &&
						(int)(Editor.Select.AnimTimeline._linkedModifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.Color) != 0;

					if (!isVisibleChangable)
					{
						return;
					}

					//1. TimelineLayer를 찾자



					apAnimTimelineLayer targetTimelineLayer = Editor.Select.AnimTimeline.GetTimelineLayer(savedObj);
					if (targetTimelineLayer != null)
					{
						//>> 1. Layer가 등록이 되었다.
						apEditorUtil.SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION.Anim_KeyframeValueChanged,
																	Editor,
																	Editor.Select.AnimClip._targetMeshGroup,
																	Editor.Select.AnimTimeline._linkedModifier, savedObj, false);


						apAnimKeyframe curKeyframe = targetTimelineLayer.GetKeyframeByFrameIndex(Editor.Select.AnimClip.CurFrame);
						if (curKeyframe != null)
						{
							//>>> 1-1. 재생상태의 현재 키프레임이 있다. -> 현재 프레임의 Visible을 세팅한다.
							if (curKeyframe._linkedModMesh_Editor != null)
							{
								curKeyframe._linkedModMesh_Editor._isVisible = !curKeyframe._linkedModMesh_Editor._isVisible;
							}
						}
						else
						{
							//>>> 1-2. 현재 키 프레임이 없는 곳이다. -> 키프레임을 추가하고 Visible을 세팅한다.
							curKeyframe = Editor.Controller.AddAnimKeyframe(Editor.Select.AnimClip.CurFrame, targetTimelineLayer, false, false, false, true);
							if (curKeyframe != null && curKeyframe._linkedModMesh_Editor != null)
							{
								curKeyframe._linkedModMesh_Editor._isVisible = !isVisibleDefault;
							}
						}
					}
					else
					{
						//>> 2. Layer가 등록이 되지 않았다.
						//       -> 레이어를 등록하고, 맨 앞프레임에 키프레임을 추가하여 Visible 값을 넣는다.	
						//TODO : 여기서부터 작업하자
						targetTimelineLayer = Editor.Controller.AddAnimTimelineLayer(savedObj, Editor.Select.AnimTimeline);
						if (targetTimelineLayer != null)
						{
							apAnimKeyframe curKeyframe = Editor.Controller.AddAnimKeyframe(Editor.Select.AnimClip.StartFrame, targetTimelineLayer, false, false, false, true);
							if (curKeyframe != null)
							{
								if (curKeyframe._linkedModMesh_Editor != null)
								{
									curKeyframe._linkedModMesh_Editor._isVisible = !isVisibleDefault;
								}
							}
						}
					}

					Editor.Select.SetAnimTimelineLayer(targetTimelineLayer, false);
				}
			}
			else
			{
				if(bone != null)
				{
					if(isCtrl)
					{
						Editor.Select.AnimClip._targetMeshGroup.SetBoneGUIVisibleAll(isVisibleDefault, bone);
					}
					else
					{
						bone.SetGUIVisible(!isVisibleDefault, true);
					}
					
				}
			}

			if (Editor.Select.AnimClip._targetMeshGroup != null)
			{
				Editor.Select.AnimClip._targetMeshGroup.RefreshForce();
			}
			Editor.RefreshControllerAndHierarchy();
		}

		// Mod Registered 체크
		//------------------------------------------------------------------------------------------
		private bool IsModRegistered(object obj)
		{
			if (Editor.Select.AnimTimeline == null)
			{
				return false;
			}
			return Editor.Select.AnimTimeline.IsObjectAddedInLayers(obj);
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

				if (_editor.Select.AnimClip != null
					&& _editor.Select.AnimTimeline != null
					&& _editor.Select.AnimTimeline._linkedModifier != null
					&& linkedRenderUnit != null
					)
				{
					//해당 Modifier가 Color/Visible을 지원하는가
					if (_editor.Select.AnimTimeline._linkedModifier._isColorPropertyEnabled
						&& (int)(_editor.Select.AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
					{
						if (isModRegistered)
						{
							//1. 자신의 타임라인 레이어를 찾는다.
							//2. 현재의 키프레임을 찾는다.
							//3. (키프레임이 있다면) 그 키프레임에 해당하는 ModMesh를 찾는다.
							apModifiedMesh modMesh = null;
							apAnimTimelineLayer timelineLayer = _editor.Select.AnimTimeline.GetTimelineLayer(targetObject);
							if (timelineLayer != null && timelineLayer._targetParamSetGroup != null && timelineLayer._targetParamSetGroup._isColorPropertyEnabled)
							{
								apAnimKeyframe curKeyframe = timelineLayer.GetKeyframeByFrameIndex(_editor.Select.AnimClip.CurFrame);
								if (curKeyframe != null)
								{
									modMesh = curKeyframe._linkedModMesh_Editor;
								}
							}
							if (modMesh != null)
							{
								//다르다면
								//Mod 아이콘
								if (modMesh._isVisible)
								{ return apEditorHierarchyUnit.VISIBLE_TYPE.ModKey_Visible; }
								else
								{ return apEditorHierarchyUnit.VISIBLE_TYPE.ModKey_NonVisible; }
							}
							else
							{
								//ModMesh가 없다면 => NoKey를 리턴한다.
								return apEditorHierarchyUnit.VISIBLE_TYPE.NoKey;
							}
						}
						else
						{
							//Mod 등록이 안되어도 NoKey 출력
							return apEditorHierarchyUnit.VISIBLE_TYPE.NoKey;
						}
					}
				}
			}
			return apEditorHierarchyUnit.VISIBLE_TYPE.None;
		}




		// GUI
		//---------------------------------------------
		//Hierarchy 레이아웃 출력
		public int GUI_RenderHierarchy_Transform(int width, float scrollX, bool isGUIEvent)
		{
			int maxLevel = 0;
			//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
			for (int i = 0; i < _units_Root_Transform.Count; i++)
			{
				int curLevel = GUI_RenderUnit(_units_Root_Transform[i], 0, width, scrollX, isGUIEvent);
				if (curLevel > maxLevel)
				{
					maxLevel = curLevel;
				}
				GUILayout.Space(10);
			}

			return maxLevel;
		}

		public int GUI_RenderHierarchy_Bone(int width, float scrollX, bool isGUIEvent)
		{
			int maxLevel = 0;
			//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
			for (int i = 0; i < _units_Root_Bone.Count; i++)
			{
				int curLevel = GUI_RenderUnit(_units_Root_Bone[i], 0, width, scrollX, isGUIEvent);
				if (curLevel > maxLevel)
				{
					maxLevel = curLevel;
				}
				GUILayout.Space(10);
			}

			return maxLevel;
		}

		public int GUI_RenderHierarchy_ControlParam(int width, float scrollX, bool isGUIEvent)
		{
			int maxLevel = 0;
			//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
			for (int i = 0; i < _units_Root_ControlParam.Count; i++)
			{
				int curLevel = GUI_RenderUnit(_units_Root_ControlParam[i], 0, width, scrollX, isGUIEvent);
				if (curLevel > maxLevel)
				{
					maxLevel = curLevel;
				}
				GUILayout.Space(10);
			}

			return maxLevel;
		}

		//재귀적으로 Hierarchy 레이아웃을 출력
		//Child에 진입할때마다 Level을 높인다. (여백과 Fold의 기준이 됨)
		private int GUI_RenderUnit(apEditorHierarchyUnit unit, int level, int width, float scrollX, bool isGUIEvent)
		{
			int maxLevel = level;
			unit.GUI_Render(level * 10, width, 20, scrollX, isGUIEvent);
			//if (unit._isFoldOut)
			if (unit.IsFoldOut)
			{
				if (unit._childUnits.Count > 0)
				{
					for (int i = 0; i < unit._childUnits.Count; i++)
					{
						//재귀적으로 호출
						int curLevel = GUI_RenderUnit(unit._childUnits[i], level + 1, width, scrollX, isGUIEvent);
						if (curLevel > maxLevel)
						{
							maxLevel = curLevel;
						}
					}
				}
			}
			return maxLevel;
		}
	}

}