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
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public class apImageSet
	{
		// Members
		//---------------------------------------------
		public enum PRESET
		{
			ToolBtn_Select,
			ToolBtn_Move,
			ToolBtn_Rotate,
			ToolBtn_Scale,
			ToolBtn_Transform,
			ToolBtn_TransformAdapt,
			ToolBtn_TransformRevert,
			ToolBtn_SoftSelection,
			ToolBtn_Blur,
			ToolBtn_BoneVisible,
			ToolBtn_BoneVisibleOutlineOnly,
			ToolBtn_Bake,
			ToolBtn_Setting,
			ToolBtn_Physic,
			ToolBtn_OnionRecord,
			ToolBtn_OnionView,

			Gizmo_OriginNone,
			Gizmo_OriginAxis,
			Gizmo_Transform_Move,
			Gizmo_Transform_Rotate,
			Gizmo_Transform_RotateBar,
			Gizmo_Transform_Scale,
			Gizmo_Helper,
			Gizmo_Bone_Origin,
			Gizmo_Bone_Body,

			Gizmo_TFBorder,

			Hierarchy_MakeNewPortrait,

			Hierarchy_Root,
			Hierarchy_Image,
			Hierarchy_Mesh,
			Hierarchy_MeshGroup,
			Hierarchy_Face,
			Hierarchy_Animation,
			Hierarchy_Param,
			Hierarchy_Modifier,
			Hierarchy_Bone,
			Hierarchy_Add,
			Hierarchy_AddPSD,
			Hierarchy_FoldDown,
			Hierarchy_FoldRight,
			Hierarchy_Folder,
			Hierarchy_Registered,

			Hierarchy_All,
			Hierarchy_None,

			//수정 : Visible 아이콘의 기존 방식에서 새로운 출력 방식으로 변경
			//Hierarchy_Visible,
			//Hierarchy_NonVisible,
			//Hierarchy_Visible_Mod,
			//Hierarchy_NonVisible_Mod,
			//Hierarchy_Visible_NoKey,
			//Hierarchy_NonVisible_NoKey,
			Hierarchy_Visible_Current,
			Hierarchy_NonVisible_Current,
			Hierarchy_Visible_TmpWork,
			Hierarchy_NonVisible_TmpWork,
			Hierarchy_Visible_ModKey,
			Hierarchy_NonVisible_ModKey,
			Hierarchy_Visible_Default,
			Hierarchy_NonVisible_Default,
			Hierarchy_NoKey,
			
			Hierarchy_Clipping,

			Hierarchy_SetClipping,
			Hierarchy_OpenLayout,
			Hierarchy_HideLayout,
			Hierarchy_AddTransform,
			Hierarchy_RemoveTransform,

			Hierarchy_Setting,

			ControlParam_Palette,


			Transform_Move,
			Transform_Rotate,
			Transform_Scale,
			Transform_Depth,
			Transform_Color,

			UI_Zoom,
			GUI_Center,
			GUI_FullScreen,

			Modifier_LayerUp,
			Modifier_LayerDown,
			Modifier_Volume,
			Modifier_Morph,
			Modifier_AnimatedMorph,
			Modifier_Rigging,
			Modifier_Physic,
			Modifier_TF,
			Modifier_AnimatedTF,
			Modifier_FFD,
			Modifier_AnimatedFFD,
			Modifier_BoneTF,
			Modifier_AnimBoneTF,

			Modifier_Active,
			Modifier_Deactive,

			Modifier_ColorVisibleOption,

			Modifier_AddNewMod,
			Modifier_AddToControlParamKey,
			Modifier_AddToPhysics,
			Modifier_AddToRigging,
			Modifier_RemoveFromControlParamKey,
			Modifier_RemoveFromPhysics,
			Modifier_RemoveFromRigging,

			Controller_Default,
			Controller_Edit,
			Controller_MakeRecordKey,
			Controller_RemoveRecordKey,
			Controller_ScrollBtn,
			Controller_ScrollBtn_Recorded,
			Controller_SlotDeactive,
			Controller_SlotActive,

			Edit_Lock,
			Edit_Unlock,
			Edit_Record,
			Edit_Recording,
			Edit_NoRecord,
			Edit_Vertex,
			Edit_Edge,
			Edit_ExEdit,

			Edit_ModLock,
			Edit_ModUnlock,
			Edit_SelectionLock,
			Edit_SelectionUnlock,

			Edit_Copy,
			Edit_Paste,

			Edit_MouseLeft,
			Edit_MouseMiddle,
			Edit_MouseRight,
			Edit_KeyDelete,
			Edit_KeyCtrl,
			Edit_KeyShift,

			Edit_MeshGroupDefaultTransform,

			MeshEdit_VertexEdge,
			MeshEdit_VertexOnly,
			MeshEdit_EdgeOnly,
			MeshEdit_Polygon,
			MeshEdit_AutoLink,
			MeshEdit_MakePolygon,

			MeshEdit_MeshEditMenu,
			MeshEdit_ModifyMenu,
			MeshEdit_PivotMenu,

			TransformControlPoint,

			Anim_Play,
			Anim_Pause,
			Anim_PrevFrame,
			Anim_NextFrame,
			Anim_FirstFrame,
			Anim_LastFrame,
			Anim_Loop,
			Anim_KeyOn,
			Anim_KeyOff,
			Anim_Keyframe,
			Anim_KeyframeDummy,
			Anim_KeySummary,
			Anim_KeySummaryMove,
			Anim_PlayBarHead,
			Anim_TimelineSize1,
			Anim_TimelineSize2,
			Anim_TimelineSize3,

			Anim_TimelineBGStart,
			Anim_TimelineBGEnd,

			Anim_EventMark,
			Anim_OnionMark,

			Anim_AutoZoom,
			Anim_WithMod,
			Anim_WithControlParam,
			Anim_WithBone,
			Anim_MoveToCurrentFrame,
			Anim_MoveToNextFrame,
			Anim_MoveToPrevFrame,
			Anim_KeyLoopLeft,
			Anim_KeyLoopRight,
			Anim_AddKeyframe,
			Anim_CurrentKeyframe,
			Anim_KeyframeCursor,
			Anim_KeyframeMoveSrc,
			Anim_KeyframeMove,
			Anim_KeyframeCopy,

			Anim_ValueMode,
			Anim_CurveMode,

			Anim_AutoScroll,

			Anim_HideLayer,
			Anim_SortABC,
			Anim_SortDepth,
			Anim_SortRegOrder,

			Anim_Load,
			Anim_Save,

			Anim_AddTimeline,
			Anim_AddAllBonesToLayer,
			Anim_AddAllMeshesToLayer,
			Anim_RemoveTimelineLayer,

			Anim_CurvePreset_Acc,
			Anim_CurvePreset_Dec,
			Anim_CurvePreset_Default,
			Anim_CurvePreset_Hard,
			

			Curve_ControlPoint,
			Curve_Linear,
			Curve_Smooth,
			Curve_Stepped,
			Curve_Prev,
			Curve_Next,

			Rig_Add,
			Rig_Select,
			Rig_Link,
			Rig_EditMode,

			Rig_IKDisabled,
			Rig_IKHead,
			Rig_IKChained,
			Rig_IKSingle,

			Rig_SaveLoad,
			Rig_LoadBones,

			Rig_HierarchyIcon_IKHead,
			Rig_HierarchyIcon_IKChained,
			Rig_HierarchyIcon_IKSingle,

			Rig_EditBinding,
			Rig_AutoNormalize,
			Rig_Auto,
			Rig_Blend,
			Rig_Normalize,
			Rig_Prune,
			Rig_AddWeight,
			Rig_MultiplyWeight,
			Rig_SubtractWeight,
			Rig_TestPosing,
			Rig_WeightColorOnly,
			Rig_WeightColorWithTexture,
			Rig_Grow,
			Rig_Shrink,
			

			Physic_Stretch,
			Physic_Bend,
			Physic_Gravity,
			Physic_Wind,
			Physic_SetMainVertex,
			Physic_VertConst,
			Physic_VertMain,
			Physic_BasicSetting,
			Physic_Inertia,
			Physic_Recover,
			Physic_Viscosity,
			Physic_Palette,

			Physic_PresetCloth1,
			Physic_PresetCloth2,
			Physic_PresetCloth3,
			Physic_PresetFlag,
			Physic_PresetHair,
			Physic_PresetRibbon,
			Physic_PresetRubberHard,
			Physic_PresetRubberSoft,
			Physic_PresetCustom1,
			Physic_PresetCustom2,
			Physic_PresetCustom3,

			ParamPreset_Body,
			ParamPreset_Cloth,
			ParamPreset_Equip,
			ParamPreset_Etc,
			ParamPreset_Eye,
			ParamPreset_Face,
			ParamPreset_Force,
			ParamPreset_Hair,
			ParamPreset_Hand,
			ParamPreset_Head,

			Demo_Logo,

			AutoSave_Frame1,
			AutoSave_Frame2,

			GUI_SelectionLock,

			StartPageLogo_Full,
			StartPageLogo_Demo,

			SmallMod_AnimMorph,
			SmallMod_Morph,
			SmallMod_AnimTF,
			SmallMod_TF,
			SmallMod_Physics,
			SmallMod_Rigging,
			SmallMod_ControlLayer,
			SmallMod_ExEnabled,
			SmallMod_ExSubEnabled,
			SmallMod_ExDisabled,
			SmallMod_ColorEnabled,
			SmallMod_ColorDisabled,
			
			SmallMod_CursorLocked,
			SmallMod_CursorUnlocked,

			Capture_Frame,
			Capture_Tab,
			Capture_Thumbnail,
			Capture_Image,
			Capture_GIF,
			Capture_Sprite,
			Capture_ExportThumb,
			Capture_ExportScreenshot,
			Capture_ExportGIF,
			Capture_ExportSprite,
			Capture_ExportSequence,

		}

		private Dictionary<PRESET, Texture2D> _images = new Dictionary<PRESET, Texture2D>();


		private bool _isAllLoaded = false;

		//Pro/Personal 스킨이 바뀐다면 리셋할 필요가 있다.
		private bool _isProSkin = false;

		// Init
		//---------------------------------------------------------
		public apImageSet()
		{
			_isAllLoaded = false;
			_images.Clear();

			_isProSkin = EditorGUIUtility.isProSkin;
		}


		public bool ReloadImages()
		{
			//만약 ProSkin 여부가 변경되었다면 리셋
			if (_isAllLoaded)
			{
				if (_isProSkin != EditorGUIUtility.isProSkin)
				{
					Debug.LogError("스킨이 바뀌었다.");
					_isProSkin = EditorGUIUtility.isProSkin;

					_isAllLoaded = false;
					_images.Clear();
				}
			}
			

			if (_isAllLoaded)
			{
				return false;
			}

			_isAllLoaded = true;
			CheckImageAndLoad(PRESET.ToolBtn_Select, "ButtonIcon_Select", true);
			CheckImageAndLoad(PRESET.ToolBtn_Move, "ButtonIcon_Move", true);
			CheckImageAndLoad(PRESET.ToolBtn_Rotate, "ButtonIcon_Rotate", true);
			CheckImageAndLoad(PRESET.ToolBtn_Scale, "ButtonIcon_Scale", true);
			CheckImageAndLoad(PRESET.ToolBtn_Transform, "ButtonIcon_Transform", true);

			CheckImageAndLoad(PRESET.ToolBtn_TransformAdapt, "ButtonIcon_TransformAdapt");
			CheckImageAndLoad(PRESET.ToolBtn_TransformRevert, "ButtonIcon_TransformRevert");

			CheckImageAndLoad(PRESET.ToolBtn_SoftSelection, "ButtonIcon_SoftSelection", true);
			CheckImageAndLoad(PRESET.ToolBtn_Blur, "ButtonIcon_Blur", true);
			CheckImageAndLoad(PRESET.ToolBtn_BoneVisible, "ButtonIcon_BoneVisible", true);
			CheckImageAndLoad(PRESET.ToolBtn_BoneVisibleOutlineOnly, "ButtonIcon_BoneVisibleOutlineOnly", true);


			CheckImageAndLoad(PRESET.ToolBtn_Bake, "ButtonIcon_Bake", true);
			CheckImageAndLoad(PRESET.ToolBtn_Setting, "ButtonIcon_Setting", true);
			CheckImageAndLoad(PRESET.ToolBtn_Physic, "ButtonIcon_Physics", true);
			CheckImageAndLoad(PRESET.ToolBtn_OnionRecord, "ButtonIcon_OnionRecord", true);
			CheckImageAndLoad(PRESET.ToolBtn_OnionView, "ButtonIcon_OnionView", true);

			CheckImageAndLoad(PRESET.Gizmo_OriginNone, "Gizmo_Origin_None");
			CheckImageAndLoad(PRESET.Gizmo_OriginAxis, "Gizmo_Origin_Axis");
			CheckImageAndLoad(PRESET.Gizmo_Transform_Move, "Gizmo_Transform_Move");
			CheckImageAndLoad(PRESET.Gizmo_Transform_Rotate, "Gizmo_Transform_Rotate");
			CheckImageAndLoad(PRESET.Gizmo_Transform_RotateBar, "Gizmo_Transform_RotateBar");
			CheckImageAndLoad(PRESET.Gizmo_Transform_Scale, "Gizmo_Transform_Scale");
			CheckImageAndLoad(PRESET.Gizmo_Helper, "Gizmo_Helper");
			CheckImageAndLoad(PRESET.Gizmo_Bone_Origin, "Gizmo_Bone_Origin");
			CheckImageAndLoad(PRESET.Gizmo_Bone_Body, "Gizmo_Bone_Body");
			CheckImageAndLoad(PRESET.Gizmo_TFBorder, "Gizmo_TFBorder");

			CheckImageAndLoad(PRESET.Hierarchy_MakeNewPortrait, "HierarchyIcon_MakeNewPortrait");

			CheckImageAndLoad(PRESET.Hierarchy_Root, "HierarchyIcon_Root");
			CheckImageAndLoad(PRESET.Hierarchy_Image, "HierarchyIcon_Image");
			CheckImageAndLoad(PRESET.Hierarchy_Mesh, "HierarchyIcon_Mesh");
			CheckImageAndLoad(PRESET.Hierarchy_MeshGroup, "HierarchyIcon_MeshGroup");
			CheckImageAndLoad(PRESET.Hierarchy_Face, "HierarchyIcon_Face");
			CheckImageAndLoad(PRESET.Hierarchy_Animation, "HierarchyIcon_Animation");
			CheckImageAndLoad(PRESET.Hierarchy_Param, "HierarchyIcon_Param");
			CheckImageAndLoad(PRESET.Hierarchy_Add, "HierarchyIcon_Add", true);
			CheckImageAndLoad(PRESET.Hierarchy_AddPSD, "HierarchyIcon_AddPSD", true);
			CheckImageAndLoad(PRESET.Hierarchy_FoldDown, "HierarchyIcon_FoldDown", true);
			CheckImageAndLoad(PRESET.Hierarchy_FoldRight, "HierarchyIcon_FoldRight", true);
			CheckImageAndLoad(PRESET.Hierarchy_Folder, "HierarchyIcon_Folder", true);
			CheckImageAndLoad(PRESET.Hierarchy_Registered, "HierarchyIcon_Registered");

			CheckImageAndLoad(PRESET.Hierarchy_Modifier, "HierarchyIcon_Modifier");
			CheckImageAndLoad(PRESET.Hierarchy_Bone, "HierarchyIcon_Bone");

			CheckImageAndLoad(PRESET.Hierarchy_All, "HierarchyIcon_All", true);
			CheckImageAndLoad(PRESET.Hierarchy_None, "HierarchyIcon_None", true);

			
			CheckImageAndLoad(PRESET.Hierarchy_Visible_Current, "HierarchyIcon_Visible_Current", true);
			CheckImageAndLoad(PRESET.Hierarchy_NonVisible_Current, "HierarchyIcon_NonVisible_Current", true);
			CheckImageAndLoad(PRESET.Hierarchy_Visible_TmpWork, "HierarchyIcon_Visible_TmpWork");
			CheckImageAndLoad(PRESET.Hierarchy_NonVisible_TmpWork, "HierarchyIcon_NonVisible_TmpWork");
			CheckImageAndLoad(PRESET.Hierarchy_Visible_ModKey, "HierarchyIcon_Visible_ModKey");
			CheckImageAndLoad(PRESET.Hierarchy_NonVisible_ModKey, "HierarchyIcon_NonVisible_ModKey");
			CheckImageAndLoad(PRESET.Hierarchy_Visible_Default, "HierarchyIcon_Visible_Default");
			CheckImageAndLoad(PRESET.Hierarchy_NonVisible_Default, "HierarchyIcon_NonVisible_Default");
			CheckImageAndLoad(PRESET.Hierarchy_NoKey, "HierarchyIcon_NoKey");


			CheckImageAndLoad(PRESET.Hierarchy_Clipping, "HierarchyIcon_Clipping", true);


			CheckImageAndLoad(PRESET.Hierarchy_SetClipping, "HierarchyIcon_SetClipping", true);
			CheckImageAndLoad(PRESET.Hierarchy_OpenLayout, "HierarchyIcon_OpenLayout", true);
			CheckImageAndLoad(PRESET.Hierarchy_HideLayout, "HierarchyIcon_HideLayout", true);
			CheckImageAndLoad(PRESET.Hierarchy_AddTransform, "HierarchyIcon_AddTransform", true);
			CheckImageAndLoad(PRESET.Hierarchy_RemoveTransform, "HierarchyIcon_RemoveTransform", true);

			CheckImageAndLoad(PRESET.Hierarchy_Setting, "HierarchyIcon_Setting");

			CheckImageAndLoad(PRESET.ControlParam_Palette, "ControlParam_Palette");


			CheckImageAndLoad(PRESET.Transform_Move, "TransformIcon_Move", true);
			CheckImageAndLoad(PRESET.Transform_Rotate, "TransformIcon_Rotate", true);
			CheckImageAndLoad(PRESET.Transform_Scale, "TransformIcon_Scale", true);
			CheckImageAndLoad(PRESET.Transform_Depth, "TransformIcon_Depth", true);
			CheckImageAndLoad(PRESET.Transform_Color, "TransformIcon_Color", true);
			CheckImageAndLoad(PRESET.UI_Zoom,			"TransformIcon_Zoom", true);
			CheckImageAndLoad(PRESET.GUI_Center,		"GUI_Center", true);
			CheckImageAndLoad(PRESET.GUI_FullScreen,	"GUI_FullScreen", true);

			CheckImageAndLoad(PRESET.Modifier_LayerUp, "Modifier_LayerUp", true);
			CheckImageAndLoad(PRESET.Modifier_LayerDown, "Modifier_LayerDown", true);
			CheckImageAndLoad(PRESET.Modifier_Volume, "Modifier_Volume");
			CheckImageAndLoad(PRESET.Modifier_Morph, "Modifier_Morph");
			CheckImageAndLoad(PRESET.Modifier_AnimatedMorph, "Modifier_AnimatedMorph");
			CheckImageAndLoad(PRESET.Modifier_Rigging, "Modifier_Rigging");
			CheckImageAndLoad(PRESET.Modifier_Physic, "Modifier_Physic");

			CheckImageAndLoad(PRESET.Modifier_TF, "Modifier_TF");
			CheckImageAndLoad(PRESET.Modifier_AnimatedTF, "Modifier_AnimatedTF");
			CheckImageAndLoad(PRESET.Modifier_FFD, "Modifier_FFD", true);
			CheckImageAndLoad(PRESET.Modifier_AnimatedFFD, "Modifier_AnimatedFFD");

			CheckImageAndLoad(PRESET.Modifier_BoneTF, "Modifier_BoneTF", true);
			CheckImageAndLoad(PRESET.Modifier_AnimBoneTF, "Modifier_AnimBoneTF", true);


			CheckImageAndLoad(PRESET.Modifier_Active, "Modifier_Active");
			CheckImageAndLoad(PRESET.Modifier_Deactive, "Modifier_Deactive");

			CheckImageAndLoad(PRESET.Modifier_ColorVisibleOption, "Modifier_ColorVisibleOption", true);

			CheckImageAndLoad(PRESET.Modifier_AddNewMod, "Modifier_AddNewMod", true);
			CheckImageAndLoad(PRESET.Modifier_AddToControlParamKey, "Modifier_AddToControlParamKey", true);
			CheckImageAndLoad(PRESET.Modifier_AddToPhysics, "Modifier_AddToPhysics", true);
			CheckImageAndLoad(PRESET.Modifier_AddToRigging, "Modifier_AddToRigging", true);
			CheckImageAndLoad(PRESET.Modifier_RemoveFromControlParamKey, "Modifier_RemoveFromControlParamKey", true);
			CheckImageAndLoad(PRESET.Modifier_RemoveFromPhysics, "Modifier_RemoveFromPhysics", true);
			CheckImageAndLoad(PRESET.Modifier_RemoveFromRigging, "Modifier_RemoveFromRigging", true);

			CheckImageAndLoad(PRESET.Controller_Default, "Controller_Default", true);
			CheckImageAndLoad(PRESET.Controller_Edit, "Controller_Edit", true);
			CheckImageAndLoad(PRESET.Controller_MakeRecordKey, "Controller_MakeRecordKey");
			CheckImageAndLoad(PRESET.Controller_RemoveRecordKey, "Controller_RemoveRecordKey");
			CheckImageAndLoad(PRESET.Controller_ScrollBtn, "Controller_ScrollBtn");
			CheckImageAndLoad(PRESET.Controller_ScrollBtn_Recorded, "Controller_ScrollBtn_Recorded");
			CheckImageAndLoad(PRESET.Controller_SlotDeactive, "Controller_Slot_Deactive");
			CheckImageAndLoad(PRESET.Controller_SlotActive, "Controller_Slot_Active");

			CheckImageAndLoad(PRESET.Edit_Lock, "Edit_Lock");
			CheckImageAndLoad(PRESET.Edit_Unlock, "Edit_Unlock");
			CheckImageAndLoad(PRESET.Edit_Record, "Edit_Record", true);
			CheckImageAndLoad(PRESET.Edit_NoRecord, "Edit_NoRecord", true);
			CheckImageAndLoad(PRESET.Edit_Recording, "Edit_Recording", true);
			CheckImageAndLoad(PRESET.Edit_Vertex, "Edit_Vertex", true);
			CheckImageAndLoad(PRESET.Edit_Edge, "Edit_Edge", true);
			CheckImageAndLoad(PRESET.Edit_ExEdit, "Edit_ExEdit");

			CheckImageAndLoad(PRESET.Edit_ModLock, "Edit_ModLock");
			CheckImageAndLoad(PRESET.Edit_ModUnlock, "Edit_ModUnlock");
			CheckImageAndLoad(PRESET.Edit_SelectionLock, "Edit_SelectionLock");
			CheckImageAndLoad(PRESET.Edit_SelectionUnlock, "Edit_SelectionUnlock");

			CheckImageAndLoad(PRESET.Edit_Copy, "Edit_Copy", true);
			CheckImageAndLoad(PRESET.Edit_Paste, "Edit_Paste", true);

			CheckImageAndLoad(PRESET.Edit_MouseLeft, "Edit_MouseLeft", true);
			CheckImageAndLoad(PRESET.Edit_MouseMiddle, "Edit_MouseMiddle", true);
			CheckImageAndLoad(PRESET.Edit_MouseRight, "Edit_MouseRight", true);
			CheckImageAndLoad(PRESET.Edit_KeyDelete, "Edit_KeyDelete", true);
			
#if UNITY_EDITOR_OSX
			CheckImageAndLoad(PRESET.Edit_KeyCtrl,			"Edit_KeyCommand", true);//Mac에서는 Ctrl대신 Command 단축키를 사용한다.
#else
			CheckImageAndLoad(PRESET.Edit_KeyCtrl, "Edit_KeyCtrl", true);
#endif
			CheckImageAndLoad(PRESET.Edit_KeyShift, "Edit_KeyShift", true);


			CheckImageAndLoad(PRESET.Edit_MeshGroupDefaultTransform, "Edit_MeshGroupDefaultTransform", true);

			CheckImageAndLoad(PRESET.MeshEdit_VertexEdge, "MeshEdit_VertexEdge");
			CheckImageAndLoad(PRESET.MeshEdit_VertexOnly, "MeshEdit_VertexOnly");
			CheckImageAndLoad(PRESET.MeshEdit_EdgeOnly, "MeshEdit_EdgeOnly");
			CheckImageAndLoad(PRESET.MeshEdit_Polygon, "MeshEdit_Polygon");
			CheckImageAndLoad(PRESET.MeshEdit_AutoLink, "MeshEdit_AutoLink");
			CheckImageAndLoad(PRESET.MeshEdit_MakePolygon, "MeshEdit_MakePolygon");

			CheckImageAndLoad(PRESET.MeshEdit_MeshEditMenu, "MeshEdit_MeshEditMenu");
			CheckImageAndLoad(PRESET.MeshEdit_ModifyMenu, "MeshEdit_ModifyMenu");
			CheckImageAndLoad(PRESET.MeshEdit_PivotMenu, "MeshEdit_PivotMenu");

			CheckImageAndLoad(PRESET.TransformControlPoint, "TransformControlPoint");


			CheckImageAndLoad(PRESET.Anim_Play, "Anim_Play", true);
			CheckImageAndLoad(PRESET.Anim_Pause, "Anim_Pause", true);
			CheckImageAndLoad(PRESET.Anim_PrevFrame, "Anim_PrevFrame", true);
			CheckImageAndLoad(PRESET.Anim_NextFrame, "Anim_NextFrame", true);
			CheckImageAndLoad(PRESET.Anim_FirstFrame, "Anim_FirstFrame", true);
			CheckImageAndLoad(PRESET.Anim_LastFrame, "Anim_LastFrame", true);
			CheckImageAndLoad(PRESET.Anim_Loop, "Anim_Loop", true);
			CheckImageAndLoad(PRESET.Anim_KeyOn, "Anim_KeyOn");
			CheckImageAndLoad(PRESET.Anim_KeyOff, "Anim_KeyOff");
			CheckImageAndLoad(PRESET.Anim_Keyframe, "Anim_Keyframe");
			CheckImageAndLoad(PRESET.Anim_KeyframeDummy, "Anim_KeyframeDummy");
			CheckImageAndLoad(PRESET.Anim_KeySummary, "Anim_KeySummary");
			CheckImageAndLoad(PRESET.Anim_KeySummaryMove, "Anim_KeySummaryMove");
			CheckImageAndLoad(PRESET.Anim_PlayBarHead, "Anim_PlayBarHead");

			CheckImageAndLoad(PRESET.Anim_TimelineSize1, "Anim_TimelineSize1", true);
			CheckImageAndLoad(PRESET.Anim_TimelineSize2, "Anim_TimelineSize2", true);
			CheckImageAndLoad(PRESET.Anim_TimelineSize3, "Anim_TimelineSize3", true);

			CheckImageAndLoad(PRESET.Anim_TimelineBGStart, "Anim_TimelineBGStart");
			CheckImageAndLoad(PRESET.Anim_TimelineBGEnd, "Anim_TimelineBGEnd");

			CheckImageAndLoad(PRESET.Anim_EventMark, "Anim_EventMark");
			CheckImageAndLoad(PRESET.Anim_OnionMark, "Anim_OnionMark");
			

			CheckImageAndLoad(PRESET.Anim_AutoZoom, "Anim_AutoZoom", true);
			CheckImageAndLoad(PRESET.Anim_WithMod, "Anim_WithMod");
			CheckImageAndLoad(PRESET.Anim_WithControlParam, "Anim_WithControlParam");
			CheckImageAndLoad(PRESET.Anim_WithBone, "Anim_WithBone");


			CheckImageAndLoad(PRESET.Anim_MoveToCurrentFrame, "Anim_MoveToCurrentFrame", true);
			CheckImageAndLoad(PRESET.Anim_MoveToNextFrame, "Anim_MoveToNextFrame", true);
			CheckImageAndLoad(PRESET.Anim_MoveToPrevFrame, "Anim_MoveToPrevFrame", true);
			CheckImageAndLoad(PRESET.Anim_KeyLoopLeft, "Anim_KeyLoopLeft");
			CheckImageAndLoad(PRESET.Anim_KeyLoopRight, "Anim_KeyLoopRight");
			CheckImageAndLoad(PRESET.Anim_AddKeyframe, "Anim_AddKeyframe");
			CheckImageAndLoad(PRESET.Anim_CurrentKeyframe, "Anim_CurrentKeyframe");
			CheckImageAndLoad(PRESET.Anim_KeyframeCursor, "Anim_KeyframeCursor");

			CheckImageAndLoad(PRESET.Anim_KeyframeMoveSrc, "Anim_KeyFrameMoveSrc");
			CheckImageAndLoad(PRESET.Anim_KeyframeMove, "Anim_KeyFrameMove");
			CheckImageAndLoad(PRESET.Anim_KeyframeCopy, "Anim_KeyFrameCopy");

			CheckImageAndLoad(PRESET.Anim_ValueMode, "Anim_ValueMode", true);
			CheckImageAndLoad(PRESET.Anim_CurveMode, "Anim_CurveMode");

			CheckImageAndLoad(PRESET.Anim_AutoScroll, "Anim_AutoScroll");

			CheckImageAndLoad(PRESET.Anim_HideLayer, "Anim_HideLayer", true);
			CheckImageAndLoad(PRESET.Anim_SortABC, "Anim_SortABC");
			CheckImageAndLoad(PRESET.Anim_SortDepth, "Anim_SortDepth");
			CheckImageAndLoad(PRESET.Anim_SortRegOrder, "Anim_SortRegOrder");

			CheckImageAndLoad(PRESET.Anim_Load, "Anim_Load");
			CheckImageAndLoad(PRESET.Anim_Save, "Anim_Save");

			CheckImageAndLoad(PRESET.Anim_AddTimeline, "Anim_AddTimeline", true);
			CheckImageAndLoad(PRESET.Anim_AddAllBonesToLayer, "Anim_AddAllBonesToLayer", true);
			CheckImageAndLoad(PRESET.Anim_AddAllMeshesToLayer, "Anim_AddAllMeshesToLayer", true);
			CheckImageAndLoad(PRESET.Anim_RemoveTimelineLayer, "Anim_RemoveTimelineLayer", true);

			CheckImageAndLoad(PRESET.Anim_CurvePreset_Acc, "Anim_CurvePreset_Acc");
			CheckImageAndLoad(PRESET.Anim_CurvePreset_Dec, "Anim_CurvePreset_Dec");
			CheckImageAndLoad(PRESET.Anim_CurvePreset_Default, "Anim_CurvePreset_Default");
			CheckImageAndLoad(PRESET.Anim_CurvePreset_Hard, "Anim_CurvePreset_Hard");

			CheckImageAndLoad(PRESET.Curve_ControlPoint, "Curve_ControlPoint");
			CheckImageAndLoad(PRESET.Curve_Linear, "Curve_Linear");
			CheckImageAndLoad(PRESET.Curve_Smooth, "Curve_Smooth");
			CheckImageAndLoad(PRESET.Curve_Stepped, "Curve_Stepped");
			CheckImageAndLoad(PRESET.Curve_Prev, "Curve_Prev");
			CheckImageAndLoad(PRESET.Curve_Next, "Curve_Next");

			CheckImageAndLoad(PRESET.Rig_Add, "Rig_Add");
			CheckImageAndLoad(PRESET.Rig_EditMode, "Rig_EditMode");
			CheckImageAndLoad(PRESET.Rig_Select, "Rig_Select");
			CheckImageAndLoad(PRESET.Rig_Link, "Rig_Link");

			CheckImageAndLoad(PRESET.Rig_IKDisabled, "Rig_IKDisabled");
			CheckImageAndLoad(PRESET.Rig_IKHead, "Rig_IKHead");
			CheckImageAndLoad(PRESET.Rig_IKChained, "Rig_IKChained");
			CheckImageAndLoad(PRESET.Rig_IKSingle, "Rig_IKSingle");

			CheckImageAndLoad(PRESET.Rig_SaveLoad, "Rig_SaveLoad");
			CheckImageAndLoad(PRESET.Rig_LoadBones, "Rig_LoadBones");

			CheckImageAndLoad(PRESET.Rig_HierarchyIcon_IKHead, "Rig_HierarchyIcon_IKHead");
			CheckImageAndLoad(PRESET.Rig_HierarchyIcon_IKChained, "Rig_HierarchyIcon_IKChained");
			CheckImageAndLoad(PRESET.Rig_HierarchyIcon_IKSingle, "Rig_HierarchyIcon_IKSingle");

			CheckImageAndLoad(PRESET.Rig_EditBinding, "Rig_EditBinding");
			CheckImageAndLoad(PRESET.Rig_AutoNormalize, "Rig_AutoNormalize");
			CheckImageAndLoad(PRESET.Rig_Auto, "Rig_Auto");
			CheckImageAndLoad(PRESET.Rig_Blend, "Rig_Blend");
			CheckImageAndLoad(PRESET.Rig_Normalize, "Rig_Normalize");
			CheckImageAndLoad(PRESET.Rig_Prune, "Rig_Prune");
			CheckImageAndLoad(PRESET.Rig_AddWeight, "Rig_AddWeight", true);
			CheckImageAndLoad(PRESET.Rig_MultiplyWeight, "Rig_MultiplyWeight", true);
			CheckImageAndLoad(PRESET.Rig_SubtractWeight, "Rig_SubtractWeight", true);
			CheckImageAndLoad(PRESET.Rig_TestPosing, "Rig_TestPosing");
			CheckImageAndLoad(PRESET.Rig_WeightColorOnly, "Rig_WeightColorOnly");
			CheckImageAndLoad(PRESET.Rig_WeightColorWithTexture, "Rig_WeightColorWithTexture");

			CheckImageAndLoad(PRESET.Rig_Grow, "Rig_Grow");
			CheckImageAndLoad(PRESET.Rig_Shrink, "Rig_Shrink");

			CheckImageAndLoad(PRESET.Physic_Stretch, "Physic_Stretch");
			CheckImageAndLoad(PRESET.Physic_Bend, "Physic_Bend");
			CheckImageAndLoad(PRESET.Physic_Gravity, "Physic_Gravity");
			CheckImageAndLoad(PRESET.Physic_Wind, "Physic_Wind");
			CheckImageAndLoad(PRESET.Physic_SetMainVertex, "Physic_SetMainVertex");
			CheckImageAndLoad(PRESET.Physic_VertConst, "Physic_VertConst");
			CheckImageAndLoad(PRESET.Physic_VertMain, "Physic_VertMain");

			CheckImageAndLoad(PRESET.Physic_BasicSetting, "Physic_BasicSetting", true);
			CheckImageAndLoad(PRESET.Physic_Inertia, "Physic_Inertia");
			CheckImageAndLoad(PRESET.Physic_Recover, "Physic_Recover");
			CheckImageAndLoad(PRESET.Physic_Viscosity, "Physic_Viscosity");
			CheckImageAndLoad(PRESET.Physic_Palette, "Physic_Palette");

			CheckImageAndLoad(PRESET.Physic_PresetCloth1, "Physic_PresetCloth1");
			CheckImageAndLoad(PRESET.Physic_PresetCloth2, "Physic_PresetCloth2");
			CheckImageAndLoad(PRESET.Physic_PresetCloth3, "Physic_PresetCloth3");
			CheckImageAndLoad(PRESET.Physic_PresetFlag, "Physic_PresetFlag");
			CheckImageAndLoad(PRESET.Physic_PresetHair, "Physic_PresetHair");
			CheckImageAndLoad(PRESET.Physic_PresetRibbon, "Physic_PresetRibbon");
			CheckImageAndLoad(PRESET.Physic_PresetRubberHard, "Physic_PresetRubberHard");
			CheckImageAndLoad(PRESET.Physic_PresetRubberSoft, "Physic_PresetRubberSoft");
			CheckImageAndLoad(PRESET.Physic_PresetCustom1, "Physic_PresetCustom1");
			CheckImageAndLoad(PRESET.Physic_PresetCustom2, "Physic_PresetCustom2");
			CheckImageAndLoad(PRESET.Physic_PresetCustom3, "Physic_PresetCustom3");

			CheckImageAndLoad(PRESET.ParamPreset_Body, "ParamPreset_Body");
			CheckImageAndLoad(PRESET.ParamPreset_Cloth, "ParamPreset_Cloth");
			CheckImageAndLoad(PRESET.ParamPreset_Equip, "ParamPreset_Equip");
			CheckImageAndLoad(PRESET.ParamPreset_Etc, "ParamPreset_Etc");
			CheckImageAndLoad(PRESET.ParamPreset_Eye, "ParamPreset_Eye");
			CheckImageAndLoad(PRESET.ParamPreset_Face, "ParamPreset_Face");
			CheckImageAndLoad(PRESET.ParamPreset_Force, "ParamPreset_Force");
			CheckImageAndLoad(PRESET.ParamPreset_Hair, "ParamPreset_Hair");
			CheckImageAndLoad(PRESET.ParamPreset_Hand, "ParamPreset_Hand");
			CheckImageAndLoad(PRESET.ParamPreset_Head, "ParamPreset_Head");

			CheckImageAndLoad(PRESET.Demo_Logo, "Demo_Logo");

			CheckImageAndLoad(PRESET.AutoSave_Frame1, "AutoSave_Frame1");
			CheckImageAndLoad(PRESET.AutoSave_Frame2, "AutoSave_Frame2");

			CheckImageAndLoad(PRESET.GUI_SelectionLock, "GUI_SelectionLock");

			CheckImageAndLoad(PRESET.StartPageLogo_Full, "StartPageLogo_Full");
			CheckImageAndLoad(PRESET.StartPageLogo_Demo, "StartPageLogo_Demo");

			CheckImageAndLoad(PRESET.SmallMod_AnimMorph,		"SmallMod_AnimMorph");
			CheckImageAndLoad(PRESET.SmallMod_Morph,			"SmallMod_Morph");
			CheckImageAndLoad(PRESET.SmallMod_AnimTF,			"SmallMod_AnimTF");
			CheckImageAndLoad(PRESET.SmallMod_TF,				"SmallMod_TF");
			CheckImageAndLoad(PRESET.SmallMod_Physics,			"SmallMod_Physics");
			CheckImageAndLoad(PRESET.SmallMod_Rigging,			"SmallMod_Rigging");
			CheckImageAndLoad(PRESET.SmallMod_ControlLayer,		"SmallMod_ControlLayer");
			CheckImageAndLoad(PRESET.SmallMod_ExEnabled,		"SmallMod_ExEnabled");
			CheckImageAndLoad(PRESET.SmallMod_ExSubEnabled,		"SmallMod_ExSubEnabled");
			CheckImageAndLoad(PRESET.SmallMod_ExDisabled,		"SmallMod_ExDisabled");
			CheckImageAndLoad(PRESET.SmallMod_ColorEnabled,		"SmallMod_ColorEnabled");
			CheckImageAndLoad(PRESET.SmallMod_ColorDisabled,	"SmallMod_ColorDisabled");

			CheckImageAndLoad(PRESET.SmallMod_CursorLocked,		"SmallMod_CursorLocked");
			CheckImageAndLoad(PRESET.SmallMod_CursorUnlocked,	"SmallMod_CursorUnlocked");

			CheckImageAndLoad(PRESET.Capture_Frame,				"Capture_Frame", true);
			CheckImageAndLoad(PRESET.Capture_Tab,				"Capture_Tab");
			CheckImageAndLoad(PRESET.Capture_Thumbnail,			"Capture_Thumbnail");
			CheckImageAndLoad(PRESET.Capture_Image,				"Capture_Image");
			CheckImageAndLoad(PRESET.Capture_GIF,				"Capture_GIF");
			CheckImageAndLoad(PRESET.Capture_Sprite,			"Capture_Sprite");

			CheckImageAndLoad(PRESET.Capture_ExportThumb,		"Capture_ExportThumb");
			CheckImageAndLoad(PRESET.Capture_ExportScreenshot,	"Capture_ExportScreenshot");
			CheckImageAndLoad(PRESET.Capture_ExportGIF,			"Capture_ExportGIF");
			CheckImageAndLoad(PRESET.Capture_ExportSprite,		"Capture_ExportSprite");
			CheckImageAndLoad(PRESET.Capture_ExportSequence,	"Capture_ExportSequence");

			return true;

		}

		private void CheckImageAndLoad(PRESET imageType, string strFileNameWOExp, bool isProSkinVersion = false)
		{
			if (_images.ContainsKey(imageType))
			{
				if (_images[imageType] == null)
				{
					//기본 경로 변경
					//"Assets/Editor/AnyPortraitTool/Images/" => apEditorUtil.ResourcePath_Icon
					if (EditorGUIUtility.isProSkin && isProSkinVersion)
					{
						_images[imageType] = AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.ResourcePath_Icon + "ProSkin/" + strFileNameWOExp + ".png");
					}
					else
					{
						_images[imageType] = AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.ResourcePath_Icon + strFileNameWOExp + ".png");
					}
				}
			}
			else
			{
				if (EditorGUIUtility.isProSkin && isProSkinVersion)
				{
					_images.Add(imageType, AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.ResourcePath_Icon + "ProSkin/" + strFileNameWOExp + ".png"));
				}
				else
				{
					_images.Add(imageType, AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.ResourcePath_Icon + strFileNameWOExp + ".png"));
				}
				
			}

			if (_images[imageType] == null)
			{
				Debug.LogError("Editor Image Load Faile : " + imageType);
				_isAllLoaded = false;
			}
		}


		

		//----------------------------------------------------------------------------
		public Texture2D Get(PRESET imageType)
		{
			if (!_images.ContainsKey(imageType))
			{
				_isAllLoaded = false;
				return null;
			}

			if (_images[imageType] == null)
			{
				_isAllLoaded = false;
				return null;
			}

			return _images[imageType];
		}
	}
}