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
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// Unity의 Undo 기능을 사용할 때, 불필요한 호출을 막는 용도
	/// "연속된 동일한 요청"을 방지한다.
	/// 중복 체크만 하는 것이므로 1개의 값만 가진다.
	/// </summary>
	public class apUndoGroupData
	{
		// Singletone
		//---------------------------------------------------
		private static apUndoGroupData _instance = new apUndoGroupData();
		public static apUndoGroupData I { get { return _instance; } }

		// Members
		//--------------------------------------------------

		private ACTION _action = ACTION.None;
		[Flags]
		public enum SAVE_TARGET : int
		{
			None = 0,
			Portrait = 1,
			Mesh = 2,
			MeshGroup = 4,
			AllMeshGroups = 8,
			Modifier = 16,
			AllModifiers = 32
		}
		private SAVE_TARGET _saveTarget = SAVE_TARGET.None;
		private apPortrait _portrait = null;
		private apMesh _mesh = null;
		private apMeshGroup _meshGroup = null;
		private apModifierBase _modifier = null;

		private object _keyObject = null;
		private bool _isCallContinuous = false;//여러 항목을 동시에 처리하는 Batch 액션 중인가

		private DateTime _lastUndoTime = new DateTime();
		private bool _isFirstAction = true;

		public enum ACTION
		{
			None,
			Main_AddImage,
			Main_RemoveImage,
			Main_AddMesh,
			Main_RemoveMesh,
			Main_AddMeshGroup,
			Main_RemoveMeshGroup,
			Main_AddAnimation,
			Main_RemoveAnimation,
			Main_AddParam,
			Main_RemoveParam,

			Portrait_SettingChanged,
			Portrait_BakeOptionChanged,
			Portrait_SetMeshGroup,
			Portrait_ReleaseMeshGroup,

			

			Image_SettingChanged,

			MeshEdit_AddVertex,
			MeshEdit_EditVertex,
			MeshEdit_RemoveVertex,
			MeshEdit_ResetVertices,
			MeshEdit_RemoveAllVertices,
			MeshEdit_AddEdge,
			MeshEdit_EditEdge,
			MeshEdit_RemoveEdge,
			MeshEdit_MakeEdges,
			MeshEdit_EditPolygons,
			MeshEdit_SetImage,
			MeshEdit_SetPivot,
			MeshEdit_SettingChanged,

			MeshGroup_AttachMesh,
			MeshGroup_AttachMeshGroup,
			MeshGroup_DetachMesh,
			MeshGroup_DetachMeshGroup,
			MeshGroup_ClippingChanged,
			MeshGroup_AddBone,
			MeshGroup_RemoveBone,
			MeshGroup_RemoveAllBones,
			MeshGroup_BoneSettingChanged,
			MeshGroup_BoneDefaultEdit,
			MeshGroup_AttachBoneToChild,
			MeshGroup_DetachBoneFromChild,
			MeshGroup_SetBoneAsParent,
			MeshGroup_SetBoneAsIKTarget,
			MeshGroup_AddBoneFromRetarget,


			MeshGroup_Gizmo_MoveTransform,
			MeshGroup_Gizmo_RotateTransform,
			MeshGroup_Gizmo_ScaleTransform,
			MeshGroup_Gizmo_Color,

			MeshGroup_AddModifier,
			MeshGroup_RemoveModifier,
			MeshGroup_RemoveParamSet,

			MeshGroup_DefaultSettingChanged,


			Modifier_LinkControlParam,
			Modifier_UnlinkControlParam,
			Modifier_AddStaticParamSetGroup,

			Modifier_LayerChanged,
			Modifier_SettingChanged,
			Modifier_SetBoneWeight,
			Modifier_RemoveBoneWeight,
			Modifier_RemoveBoneRigging,
			Modifier_RemovePhysics,
			Modifier_SetPhysicsWeight,
			Modifier_SetVolumeWeight,
			Modifier_SetPhysicsProperty,

			Modifier_Gizmo_MoveTransform,
			Modifier_Gizmo_RotateTransform,
			Modifier_Gizmo_ScaleTransform,
			Modifier_Gizmo_MoveVertex,
			Modifier_Gizmo_RotateVertex,
			Modifier_Gizmo_ScaleVertex,
			Modifier_Gizmo_FFDVertex,
			Modifier_Gizmo_Color,
			Modifier_Gizmo_BlurVertex,

			Modifier_ModMeshValuePaste,
			Modifier_ModMeshValueReset,
			Modifier_AddModMeshToParamSet,
			Modifier_RemoveModMeshFromParamSet,

			Modifier_FFDStart,
			Modifier_FFDAdapt,
			Modifier_FFDRevert,

			Anim_SetMeshGroup,
			Anim_DupAnimClip,
			Anim_ImportAnimClip,
			Anim_AddTimeline,
			Anim_RemoveTimeline,
			Anim_AddTimelineLayer,
			Anim_RemoveTimelineLayer,
			Anim_AddKeyframe,
			Anim_MoveKeyframe,
			Anim_CopyKeyframe,
			Anim_RemoveKeyframe,
			Anim_DupKeyframe,
			Anim_KeyframeValueChanged,
			Anim_AddEvent,
			Anim_RemoveEvent,
			Anim_EventChanged,

			Anim_Gizmo_MoveTransform,
			Anim_Gizmo_RotateTransform,
			Anim_Gizmo_ScaleTransform,

			Anim_Gizmo_MoveVertex,
			Anim_Gizmo_RotateVertex,
			Anim_Gizmo_ScaleVertex,
			Anim_Gizmo_FFDVertex,
			Anim_Gizmo_BlurVertex,

			Anim_Gizmo_Color,

			ControlParam_SettingChanged,

			Retarget_ImportSinglePoseToMod,
			Retarget_ImportSinglePoseToAnim
		}


		public static string GetLabel(ACTION action)
		{
			switch (action)
			{
				case ACTION.None:					return "None";

				case ACTION.Main_AddImage:			return "Add Image";
				case ACTION.Main_RemoveImage:		return "Remove Image";
				case ACTION.Main_AddMesh:			return "Add Mesh";
				case ACTION.Main_RemoveMesh:		return "Remove Mesh";
				case ACTION.Main_AddMeshGroup:		return "Add MeshGroup";
				case ACTION.Main_RemoveMeshGroup:	return "Remove MeshGroup";
				case ACTION.Main_AddAnimation:		return "Add Animation";
				case ACTION.Main_RemoveAnimation:	return "Remove Animation";
				case ACTION.Main_AddParam:			return "Add Parameter";
				case ACTION.Main_RemoveParam:		return "Remove Parameter";

				case ACTION.Portrait_SettingChanged:		return "Portrait Setting Changed";
				case ACTION.Portrait_BakeOptionChanged:		return "Bake Option Changed";
				case ACTION.Portrait_SetMeshGroup:			return "Set Main MeshGroup";
				case ACTION.Portrait_ReleaseMeshGroup:		return "Release Main MeshGroup";

				case ACTION.Image_SettingChanged:	return "Set Image Property";

				case ACTION.MeshEdit_AddVertex:				return "Add Vertex";
				case ACTION.MeshEdit_EditVertex:			return "Edit Vertex";
				case ACTION.MeshEdit_RemoveVertex:			return "Remove Vertex";
				case ACTION.MeshEdit_ResetVertices:			return "Reset Vertices";
				case ACTION.MeshEdit_RemoveAllVertices:		return "Remove All Vertices";
				case ACTION.MeshEdit_AddEdge:				return "Add Edge";
				case ACTION.MeshEdit_EditEdge:				return "Edit Edge";
				case ACTION.MeshEdit_RemoveEdge:			return "Remove Edge";
				case ACTION.MeshEdit_MakeEdges:				return "Make Edges";
				case ACTION.MeshEdit_EditPolygons:			return "Edit Polygons";
				case ACTION.MeshEdit_SetImage:				return "Set Image";
				case ACTION.MeshEdit_SetPivot:				return "Set Mesh Pivot";
				case ACTION.MeshEdit_SettingChanged:		return "Mesh Setting Changed";

				case ACTION.MeshGroup_AttachMesh:			return "Attach Mesh";
				case ACTION.MeshGroup_AttachMeshGroup:		return "Attach MeshGroup";
				case ACTION.MeshGroup_DetachMesh:			return "Detach Mesh";
				case ACTION.MeshGroup_DetachMeshGroup:		return "Detach MeshGroup";
				case ACTION.MeshGroup_ClippingChanged:		return "Clipping Changed";
				case ACTION.MeshGroup_AddBone:				return "Add Bone";
				case ACTION.MeshGroup_RemoveBone:			return "Remove Bone";
				case ACTION.MeshGroup_RemoveAllBones:		return "Remove All Bones";
				case ACTION.MeshGroup_BoneSettingChanged:	return "Bone Setting Changed";
				case ACTION.MeshGroup_BoneDefaultEdit:		return "Bone Edit";
				case ACTION.MeshGroup_AttachBoneToChild:	return "Attach Bone to Child";
				case ACTION.MeshGroup_DetachBoneFromChild:	return "Detach Bone from Child";
				case ACTION.MeshGroup_SetBoneAsParent:		return "Set Bone as Parent";
				case ACTION.MeshGroup_SetBoneAsIKTarget:	return "Set Bone as IK target";
				case ACTION.MeshGroup_AddBoneFromRetarget:	return "Add Bones from File";

				case ACTION.MeshGroup_Gizmo_MoveTransform:		return "Default Position";
				case ACTION.MeshGroup_Gizmo_RotateTransform:	return "Default Rotation";
				case ACTION.MeshGroup_Gizmo_ScaleTransform:		return "Default Scaling";
				case ACTION.MeshGroup_Gizmo_Color:				return "Default Color";

				case ACTION.MeshGroup_AddModifier:		return "Add Modifier";
				case ACTION.MeshGroup_RemoveModifier:	return "Remove Modifier";
				case ACTION.MeshGroup_RemoveParamSet:	return "Remove Modified Key";

				case ACTION.MeshGroup_DefaultSettingChanged:	return "Default Setting Changed";

				case ACTION.Modifier_LinkControlParam:			return "Link Control Parameter";
				case ACTION.Modifier_UnlinkControlParam:		return "Unlink Control Parameter";
				case ACTION.Modifier_AddStaticParamSetGroup:	return "Add StaticPSG";

				case ACTION.Modifier_LayerChanged:			return "Change Layer Order";
				case ACTION.Modifier_SettingChanged:		return "Change Layer Setting";
				case ACTION.Modifier_SetBoneWeight:			return "Set Bone Weight";
				case ACTION.Modifier_RemoveBoneWeight:		return "Remove Bone Weight";
				case ACTION.Modifier_RemoveBoneRigging:		return "Remove Bone Rigging";
				case ACTION.Modifier_RemovePhysics:			return "Remove Physics";
				case ACTION.Modifier_SetPhysicsWeight:		return "Set Physics Weight";
				case ACTION.Modifier_SetVolumeWeight:		return "Set Volume Weight";
				case ACTION.Modifier_SetPhysicsProperty:	return "Set Physics Property";

				case ACTION.Modifier_Gizmo_MoveTransform:		return "Move Transform";
				case ACTION.Modifier_Gizmo_RotateTransform:		return "Rotate Transform";
				case ACTION.Modifier_Gizmo_ScaleTransform:		return "Scale Transform";
				case ACTION.Modifier_Gizmo_MoveVertex:			return "Move Vertex";
				case ACTION.Modifier_Gizmo_RotateVertex:		return "Rotate Vertex";
				case ACTION.Modifier_Gizmo_ScaleVertex:			return "Scale Vertex";
				case ACTION.Modifier_Gizmo_FFDVertex:			return "Freeform Vertices";
				case ACTION.Modifier_Gizmo_Color:				return "Set Color";
				case ACTION.Modifier_Gizmo_BlurVertex:			return "Blur Vertices";

				case ACTION.Modifier_ModMeshValuePaste:		return "Paste Modified Value";
				case ACTION.Modifier_ModMeshValueReset:		return "Reset Modified Value";

				case ACTION.Modifier_AddModMeshToParamSet:			return "Add To Key";
				case ACTION.Modifier_RemoveModMeshFromParamSet:		return "Remove From Key";

				case ACTION.Modifier_FFDStart:				return "Edit FFD";
				case ACTION.Modifier_FFDAdapt:				return "Adapt FFD";
				case ACTION.Modifier_FFDRevert:				return "Revert FFD";

				case ACTION.Anim_SetMeshGroup:			return "Set MeshGroup";
				case ACTION.Anim_DupAnimClip:			return "Duplicate AnimClip";
				case ACTION.Anim_ImportAnimClip:		return "Import AnimClip";
				case ACTION.Anim_AddTimeline:			return "Add Timeline";
				case ACTION.Anim_RemoveTimeline:		return "Remove Timeline";
				case ACTION.Anim_AddTimelineLayer:		return "Add Timeline Layer";
				case ACTION.Anim_RemoveTimelineLayer:	return "Remove Timeline Layer";
				case ACTION.Anim_AddKeyframe:			return "Add Keyframe";

				case ACTION.Anim_MoveKeyframe:		return "Move Keyframe";
				case ACTION.Anim_CopyKeyframe:		return "Copy Keyframe";
				case ACTION.Anim_RemoveKeyframe:	return "Remove Keyframe";
				case ACTION.Anim_DupKeyframe:		return "Duplicate Keyframe";

				case ACTION.Anim_KeyframeValueChanged:	return "Keyframe Value Changed";
				case ACTION.Anim_AddEvent:				return "Event Added";
				case ACTION.Anim_RemoveEvent:			return "Event Removed";
				case ACTION.Anim_EventChanged:			return "Event Changed";

				case ACTION.Anim_Gizmo_MoveTransform:		return "Move Transform";
				case ACTION.Anim_Gizmo_RotateTransform:		return "Rotate Transform";
				case ACTION.Anim_Gizmo_ScaleTransform:		return "Scale Transform";

				case ACTION.Anim_Gizmo_MoveVertex:		return "Move Vertex";
				case ACTION.Anim_Gizmo_RotateVertex:	return "Rotate Vertex";
				case ACTION.Anim_Gizmo_ScaleVertex:		return "Scale Vertex";
				case ACTION.Anim_Gizmo_FFDVertex:		return "Freeform Vertices";
				case ACTION.Anim_Gizmo_BlurVertex:		return "Blur Vertices";
				case ACTION.Anim_Gizmo_Color:			return "Set Color";

				case ACTION.ControlParam_SettingChanged:	return "Control Param Setting";

				case ACTION.Retarget_ImportSinglePoseToMod:		return "Import Pose";
				case ACTION.Retarget_ImportSinglePoseToAnim:	return "Import Pose";

				default:
					Debug.LogError("정의되지 않은 Undo Action");
					return action.ToString();
			}
		}

		// Init
		//--------------------------------------------------
		private apUndoGroupData()
		{
			_lastUndoTime = DateTime.Now;
			_isFirstAction = true;
		}

		public void Clear()
		{
			_action = ACTION.None;
			_saveTarget = SAVE_TARGET.None;
			_portrait = null;
			_mesh = null;
			_meshGroup = null;
			_modifier = null;

			_keyObject = null;
			_isCallContinuous = false;//여러 항목을 동시에 처리하는 Batch 액션 중인가

			_lastUndoTime = DateTime.Now;
		}


		// Functions
		//--------------------------------------------------
		/// <summary>
		/// Undo 전에 중복을 체크하기 위해 Action을 등록한다.
		/// 리턴값이 True이면 "새로운 Action"이므로 Undo 등록을 해야한다.
		/// 만약 Action 타입이 Add, New.. 계열이면 targetObject가 null일 수 있다. (parent는 null이 되어선 안된다)
		/// </summary>
		public bool SetAction(ACTION action, apPortrait portrait, apMesh mesh, apMeshGroup meshGroup, apModifierBase modifier, object keyObject, bool isCallContinuous, SAVE_TARGET saveTarget)
		{
			bool isTimeOver = false;
			if(DateTime.Now.Subtract(_lastUndoTime).TotalSeconds > 1.0f || _isFirstAction)
			{
				//1초가 넘었다면 강제 Undo ID 증가
				isTimeOver = true;
				_lastUndoTime = DateTime.Now;
				_isFirstAction = false;
			}

			//특정 조건에서는 UndoID가 증가하지 않는다.
			//유효한 Action이고 시간이 지나지 않았다면
			if(_action != ACTION.None && !isTimeOver)
			{
				//이전과 값이 같을 때에만 Multiple 처리가 된다.
				if(	action == _action &&
					saveTarget == _saveTarget &&
					portrait == _portrait &&
					mesh == _mesh &&
					meshGroup == _meshGroup &&
					modifier == _modifier &&
					isCallContinuous == _isCallContinuous
					)
				{
					if(isCallContinuous)
					{
						//연속 호출이면 KeyObject가 달라도 Undo를 묶는다.
						return false;
					}
					else if(keyObject == _keyObject && keyObject != null)
					{
						//연속 호출이 아니더라도 KeyObject가 같으면 Undo를 묶는다.
						return false;
					}
				}
			}
			#region [미사용 코드]
			//if (_action != ACTION.None && _parentMonoObject != null)
			//{
			//	if (_action == action && _parentMonoObject == parentMonoObject && isMultiple == _isMultiple)
			//	{
			//		if (_isMultiple)
			//		{
			//			//다중 처리 타입이면 -> targetObject가 달라도 연속된 액션이다.
			//			return false;
			//		}
			//		else
			//		{
			//			//Multiple 타입이 아니라면 targetObject도 동일해야한다.
			//			//단, 둘다 Null이라면 연속된 타입일 수 없다.
			//			if (targtObject == _keyObject && targtObject != null && _keyObject != null)
			//			{
			//				if (targetObject2 != null)
			//				{
			//					if(targetObject2 == _targetObject2)
			//					{
			//						return false;//연속된 Action이다.
			//					}
			//				}
			//				else
			//				{
			//					return false;//연속된 Action이다.
			//				}
			//			}
			//		}
			//	}
			//} 
			#endregion
			_action = action;

			_saveTarget = saveTarget;
			_portrait = portrait;
			_mesh = mesh;
			_meshGroup = meshGroup;
			_modifier = modifier;

			_keyObject = keyObject;
			_isCallContinuous = isCallContinuous;//여러 항목을 동시에 처리하는 Batch 액션 중인가

			//_parentMonoObject = parentMonoObject;
			//_keyObject = targtObject;
			//_targetObject2 = targetObject2;
			//_isMultiple = isMultiple;

			//Debug.Log("Undo Regist [" + action + "]");
			return true;
		}

	}
}