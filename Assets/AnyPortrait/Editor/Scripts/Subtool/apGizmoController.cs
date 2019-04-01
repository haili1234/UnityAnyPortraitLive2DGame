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

	public partial class apGizmoController
	{
		// Member
		//--------------------------------------------------
		private apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }


		// Init
		//--------------------------------------------------
		public apGizmoController()
		{

		}

		public void SetEditor(apEditor editor)
		{
			_editor = editor;

		}




		// Gizmo - MeshGroup : Setting
		//--------------------------------------------------
		public apGizmos.GizmoEventSet GetEventSet_MeshGroupSetting()
		{
			//MeshGroup 내의 Mesh의 기본 위치를 바꾼다.
			//다중 선택과 FFD Transform이 제한된다. (null...)
			return new apGizmos.GizmoEventSet(
				Select__MeshGroup_Setting,
				Unselect__MeshGroup_Setting,
				Move__MeshGroup_Setting,
				Rotate__MeshGroup_Setting,
				Scale__MeshGroup_Setting,
				TransformChanged_Position__MeshGroup_Setting,
				TransformChanged_Rotate__MeshGroup_Setting,
				TransformChanged_Scale__MeshGroup_Setting,
				TransformChanged_Color__MeshGroup_Setting,
				PivotReturn__MeshGroup_Setting,
				null,
				null,
				null,
				null,
				null,
				apGizmos.TRANSFORM_UI.TRS | apGizmos.TRANSFORM_UI.Color,
				FirstLink__MeshGroup_Setting);
		}

		public apGizmos.SelectResult FirstLink__MeshGroup_Setting()
		{
			if (Editor.Select.MeshGroup == null)
			{
				return null;
			}

			//if(Editor.Select.SubMeshGroupInGroup != null || 
			//	Editor.Select.SubMeshGroupInGroup != null)
			//{
			//	return 1;
			//}
			if (Editor.Select.SubMeshGroupInGroup != null)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.SubMeshGroupInGroup);
			}
			if (Editor.Select.SubMeshInGroup != null)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.SubMeshInGroup);
			}
			//return 0;
			return null;
		}

		public apGizmos.SelectResult Select__MeshGroup_Setting(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if (Editor.Select.MeshGroup == null)
			{
				return null;
			}

			apTransform_MeshGroup prevSelectedMeshGroupTransform = Editor.Select.SubMeshGroupInGroup;
			//apTransform_Mesh prevSelectedMeshTransform = Editor.Select.SubMeshInGroup;
			//apGizmos.SELECT_RESULT result = apGizmos.SELECT_RESULT.None;

			//int result = 0;
			object resultObj = null;


			if (Editor.Controller.IsMouseInGUI(mousePosGL))
			{
				apTransform_Mesh selectedMeshTransform = null;

				List<apRenderUnit> renderUnits = Editor.Select.MeshGroup._renderUnits_All;//<<정렬된 Render Unit
				for (int iUnit = 0; iUnit < renderUnits.Count; iUnit++)
				{
					apRenderUnit renderUnit = renderUnits[iUnit];
					if (renderUnit._meshTransform != null && renderUnit._meshTransform._mesh != null)
					{
						if (renderUnit._meshTransform._isVisible_Default)
						{
							//Debug.LogError("TODO : Mouse Picking 바꿀것");
							//bool isPick = apEditorUtil.IsMouseInMesh(
							//	mousePosGL,
							//	renderUnit._meshTransform._mesh,
							//	renderUnit.WorldMatrixOfNode.inverse
							//	);
							bool isPick = apEditorUtil.IsMouseInRenderUnitMesh(
								mousePosGL, renderUnit);

							if (isPick)
							{
								selectedMeshTransform = renderUnit._meshTransform;
								//찾았어도 계속 찾는다.
								//뒤의 아이템이 "앞쪽"에 있는 것이기 때문
							}
						}
					}
				}

				if (selectedMeshTransform != null)
				{
					//이전 버전
					//>> 만약 ChildMeshGroup에 속한 거라면, Mesh Group 자체를 선택해야 한다.
					//apMeshGroup parentMeshGroup = Editor.Select.MeshGroup.FindParentMeshGroupOfMeshTransform(selectedMeshTransform);
					//if (parentMeshGroup == null || parentMeshGroup == Editor.Select.MeshGroup)
					//{
					//	Editor.Select.SetSubMeshInGroup(selectedMeshTransform);
					//}
					//else
					//{
					//	apTransform_MeshGroup childMeshGroupTransform = Editor.Select.MeshGroup.FindChildMeshGroupTransform(parentMeshGroup);
					//	if (childMeshGroupTransform != null)
					//	{
					//		Editor.Select.SetSubMeshGroupInGroup(childMeshGroupTransform);
					//	}
					//	else
					//	{
					//		Editor.Select.SetSubMeshInGroup(selectedMeshTransform);
					//	}
					//}

					//수정된 버전
					//>> 그냥 MeshGroup Transform은 마우스로 선택 못하는 걸로 하자
					Editor.Select.SetSubMeshInGroup(selectedMeshTransform);

					//result = 1;
					resultObj = selectedMeshTransform;

					//if(prevSelectedMeshGroupTransform == Editor.Select.SubMeshGroupInGroup
					//	//&& prevSelectedMeshGroupTransform == Editor.Select.SubMeshGroupInGroup//<<이거 뭐야?
					//	)
					//{
					//	//isSameObject = true;
					//	//result = apGizmos.SELECT_RESULT.SameSelected;
					//	result = 1;
					//	resultObj = Editor.Select.SubMeshGroupInGroup;
					//}
					//else
					//{
					//	//isSameObject = false;
					//	//result = apGizmos.SELECT_RESULT.NewSelected;
					//	result = 1;
					//}


				}
				else
				{
					Editor.Select.SetSubMeshInGroup(null);
				}

				Editor.RefreshControllerAndHierarchy();
				//Editor.Repaint();
				Editor.SetRepaint();
			}

			if (resultObj == null)
			{
				resultObj = prevSelectedMeshGroupTransform;
			}
			//return result;
			return apGizmos.SelectResult.Main.SetSingle(resultObj);
		}


		public void Unselect__MeshGroup_Setting()
		{
			if (Editor.Select.MeshGroup == null)
			{
				return;
			}

			Editor.Select.SetSubMeshInGroup(null);
			Editor.RefreshControllerAndHierarchy();
			Editor.SetRepaint();
		}


		public void Move__MeshGroup_Setting(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingChangePivot)
			{
				return;
			}

			if (deltaMoveW.sqrMagnitude == 0.0f && !isFirstMove)
			{
				return;
			}

			apMatrix targetMatrix = null;
			object targetObj = null;
			apMatrix worldMatrix = null;
			apMatrix parentWorldMatrix = null;

			bool isRootMeshGroup = false;

			//Modifier가 적용이 안된 상태이므로
			//World Matrix = ParentWorld x ToParent(Default) 가 성립한다.
			if (Editor.Select.SubMeshInGroup != null)
			{
				targetMatrix = Editor.Select.SubMeshInGroup._matrix;//=ToParent
				targetObj = Editor.Select.SubMeshInGroup;
				worldMatrix = new apMatrix(Editor.Select.SubMeshInGroup._matrix_TFResult_World);
				parentWorldMatrix = Editor.Select.SubMeshInGroup._matrix_TF_ParentWorld;
				
				isRootMeshGroup = Editor.Select.MeshGroup._childMeshTransforms.Contains(Editor.Select.SubMeshInGroup);
			}
			else if (Editor.Select.SubMeshGroupInGroup != null)
			{
				targetMatrix = Editor.Select.SubMeshGroupInGroup._matrix;//=ToParent
				targetObj = Editor.Select.SubMeshGroupInGroup;
				worldMatrix = new apMatrix(Editor.Select.SubMeshGroupInGroup._matrix_TFResult_World);
				parentWorldMatrix = Editor.Select.SubMeshGroupInGroup._matrix_TF_ParentWorld;

				isRootMeshGroup = false;
			}
			else
			{
				return;
			}

			worldMatrix._pos += deltaMoveW;
			worldMatrix.MakeMatrix();
			worldMatrix.RInverse(parentWorldMatrix);//ParentWorld-1 x World = ToParent


			Vector2 newLocalPos = worldMatrix._pos;


			//Undo
			if (isFirstMove)
			{
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_Gizmo_MoveTransform, Editor, Editor.Select.MeshGroup, targetObj, false, !isRootMeshGroup);
			}

			//targetMatrix.SetPos(targetMatrix._pos.x + deltaMoveW.x, targetMatrix._pos.y + deltaMoveW.y);
			targetMatrix.SetPos(newLocalPos.x, newLocalPos.y);
			targetMatrix.MakeMatrix();

			//Editor.RefreshControllerAndHierarchy();
		}
		public void Rotate__MeshGroup_Setting(float deltaAngleW, bool isFirstRotate)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingChangePivot)
			{
				return;
			}

			if (deltaAngleW == 0.0f && !isFirstRotate)
			{
				return;
			}

			apMatrix targetMatrix = null;
			object targetObj = null;
			apMatrix worldMatrix = null;
			apMatrix parentWorldMatrix = null;

			bool isRootMeshGroup = false;


			if (Editor.Select.SubMeshInGroup != null)
			{
				targetMatrix = Editor.Select.SubMeshInGroup._matrix;
				targetObj = Editor.Select.SubMeshInGroup;
				worldMatrix = new apMatrix(Editor.Select.SubMeshInGroup._matrix_TFResult_World);
				parentWorldMatrix = Editor.Select.SubMeshInGroup._matrix_TF_ParentWorld;

				isRootMeshGroup = Editor.Select.MeshGroup._childMeshTransforms.Contains(Editor.Select.SubMeshInGroup);
			}
			else if (Editor.Select.SubMeshGroupInGroup != null)
			{
				targetMatrix = Editor.Select.SubMeshGroupInGroup._matrix;
				targetObj = Editor.Select.SubMeshGroupInGroup;
				worldMatrix = new apMatrix(Editor.Select.SubMeshGroupInGroup._matrix_TFResult_World);
				parentWorldMatrix = Editor.Select.SubMeshGroupInGroup._matrix_TF_ParentWorld;

				isRootMeshGroup = false;
			}
			else
			{
				return;
			}

			float nextAngle = worldMatrix._angleDeg + deltaAngleW;
			while (nextAngle < -180.0f)
			{
				nextAngle += 360.0f;
			}
			while (nextAngle > 180.0f)
			{
				nextAngle -= 360.0f;
			}
			worldMatrix._angleDeg = nextAngle;
			worldMatrix.MakeMatrix();
			worldMatrix.RInverse(parentWorldMatrix);//ParentWorld-1 x World = ToParent


			//Undo
			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_Gizmo_RotateTransform, Editor, Editor.Select.MeshGroup, targetObj, false, !isRootMeshGroup);
			}

			//targetMatrix.SetRotate(deltaAngleW + targetMatrix._angleDeg);
			targetMatrix.SetRotate(worldMatrix._angleDeg);
			targetMatrix.MakeMatrix();
		}


		public void Scale__MeshGroup_Setting(Vector2 deltaScaleW, bool isFirstScale)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingChangePivot)
			{
				return;
			}

			if (deltaScaleW.sqrMagnitude == 0.0f && !isFirstScale)
			{
				return;
			}

			apMatrix targetMatrix = null;
			object targetObj = null;
			apMatrix worldMatrix = null;
			apMatrix parentWorldMatrix = null;
			//Modifier가 적용이 안된 상태이므로
			//World Matrix = ParentWorld x ToParent(Default) 가 성립한다.

			bool isRootMeshGroup = false;

			if (Editor.Select.SubMeshInGroup != null)
			{
				targetMatrix = Editor.Select.SubMeshInGroup._matrix;
				targetObj = Editor.Select.SubMeshInGroup;
				worldMatrix = new apMatrix(Editor.Select.SubMeshInGroup._matrix_TFResult_World);
				parentWorldMatrix = Editor.Select.SubMeshInGroup._matrix_TF_ParentWorld;

				isRootMeshGroup = Editor.Select.MeshGroup._childMeshTransforms.Contains(Editor.Select.SubMeshInGroup);
			}
			else if (Editor.Select.SubMeshGroupInGroup != null)
			{
				targetMatrix = Editor.Select.SubMeshGroupInGroup._matrix;
				targetObj = Editor.Select.SubMeshGroupInGroup;
				worldMatrix = new apMatrix(Editor.Select.SubMeshGroupInGroup._matrix_TFResult_World);
				parentWorldMatrix = Editor.Select.SubMeshGroupInGroup._matrix_TF_ParentWorld;

				isRootMeshGroup = false;
			}
			else
			{
				return;
			}
			worldMatrix._scale += deltaScaleW;
			worldMatrix.MakeMatrix();
			worldMatrix.RInverse(parentWorldMatrix);//ParentWorld-1 x World = ToParent


			//Undo
			if (isFirstScale)
			{
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_Gizmo_ScaleTransform, Editor, Editor.Select.MeshGroup, targetObj, false, !isRootMeshGroup);
			}

			//Vector2 scale2 = new Vector2(targetMatrix._scale.x, targetMatrix._scale.y);
			//targetMatrix.SetScale(deltaScaleW + scale2);
			targetMatrix.SetScale(worldMatrix._scale);
			targetMatrix.MakeMatrix();
		}



		public void TransformChanged_Position__MeshGroup_Setting(Vector2 pos, int depth)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingChangePivot)
			{
				return;
			}

			if (Editor.Select.SubMeshInGroup == null && Editor.Select.SubMeshGroupInGroup == null)
			{ return; }

			apRenderUnit curRenderUnit = null;
			apMatrix curMatrixParam = null;

			object targetObj = null;
			if (Editor.Select.SubMeshInGroup != null)
			{
				curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.SubMeshInGroup);
				curMatrixParam = Editor.Select.SubMeshInGroup._matrix;
				targetObj = Editor.Select.SubMeshInGroup;
			}
			else if (Editor.Select.SubMeshGroupInGroup != null)
			{
				curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.SubMeshGroupInGroup);
				curMatrixParam = Editor.Select.SubMeshGroupInGroup._matrix;
				targetObj = Editor.Select.SubMeshGroupInGroup;
			}

			if (curRenderUnit == null)
			{ return; }

			//Undo
			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_Gizmo_MoveTransform, Editor, Editor.Select.MeshGroup, targetObj, false, true);

			bool bSort = false;
			if (curRenderUnit.GetDepth() != depth)
			{
				//curRenderUnit.SetDepth(depth);
				Editor.Select.MeshGroup.ChangeRenderUnitDetph(curRenderUnit, depth);

				bSort = true;
			}
			curMatrixParam.SetPos(pos);
			curMatrixParam.MakeMatrix();
			if (bSort)
			{
				Editor.Select.MeshGroup.SortRenderUnits(true);
			}
			Editor.SetRepaint();
		}



		public void TransformChanged_Rotate__MeshGroup_Setting(float angle)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingChangePivot)
			{
				return;
			}
			if (Editor.Select.SubMeshInGroup == null && Editor.Select.SubMeshGroupInGroup == null)
			{ return; }

			apRenderUnit curRenderUnit = null;
			apMatrix curMatrixParam = null;

			object targetObj = null;
			if (Editor.Select.SubMeshInGroup != null)
			{
				curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.SubMeshInGroup);
				curMatrixParam = Editor.Select.SubMeshInGroup._matrix;
				targetObj = Editor.Select.SubMeshInGroup;
			}
			else if (Editor.Select.SubMeshGroupInGroup != null)
			{
				curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.SubMeshGroupInGroup);
				curMatrixParam = Editor.Select.SubMeshGroupInGroup._matrix;
				targetObj = Editor.Select.SubMeshGroupInGroup;
			}

			if (curRenderUnit == null)
			{ return; }

			//Undo
			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_Gizmo_RotateTransform, Editor, Editor.Select.MeshGroup, targetObj, false, true);

			curMatrixParam.SetRotate(angle);
			curMatrixParam.MakeMatrix();
			Editor.SetRepaint();
		}



		public void TransformChanged_Scale__MeshGroup_Setting(Vector2 scale)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingChangePivot)
			{
				return;
			}
			if (Editor.Select.SubMeshInGroup == null && Editor.Select.SubMeshGroupInGroup == null)
			{ return; }

			apRenderUnit curRenderUnit = null;
			apMatrix curMatrixParam = null;
			object targetObj = null;
			if (Editor.Select.SubMeshInGroup != null)
			{
				curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.SubMeshInGroup);
				curMatrixParam = Editor.Select.SubMeshInGroup._matrix;
				targetObj = Editor.Select.SubMeshInGroup;
			}
			else if (Editor.Select.SubMeshGroupInGroup != null)
			{
				curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.SubMeshGroupInGroup);
				curMatrixParam = Editor.Select.SubMeshGroupInGroup._matrix;
				targetObj = Editor.Select.SubMeshGroupInGroup;
			}

			if (curRenderUnit == null)
			{ return; }

			//Undo
			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_Gizmo_ScaleTransform, Editor, Editor.Select.MeshGroup, targetObj, false, true);

			curMatrixParam.SetScale(scale);
			curMatrixParam.MakeMatrix();
			Editor.SetRepaint();
		}





		public void TransformChanged_Color__MeshGroup_Setting(Color color, bool isVisible)
		{
			if (Editor.Select.MeshGroup == null
				//|| !Editor.Select.IsMeshGroupSettingChangePivot//수정 : Pivot 변경 상태가 아니어도 변경 가능
				)
			{
				return;
			}
			if (Editor.Select.SubMeshInGroup == null && Editor.Select.SubMeshGroupInGroup == null)
			{ return; }

			apRenderUnit curRenderUnit = null;
			//apMatrix curMatrixParam = null;
			object targetObj = null;

			//Undo
			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_Gizmo_Color, Editor, Editor.Select.MeshGroup, targetObj, false, true);

			if (Editor.Select.SubMeshInGroup != null)
			{
				curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.SubMeshInGroup);
				//curMatrixParam = Editor.Select.SubMeshInGroup._matrix;
				targetObj = Editor.Select.SubMeshInGroup;
				Editor.Select.SubMeshInGroup._meshColor2X_Default = color;
				Editor.Select.SubMeshInGroup._isVisible_Default = isVisible;
			}
			else if (Editor.Select.SubMeshGroupInGroup != null)
			{
				curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.SubMeshGroupInGroup);
				//curMatrixParam = Editor.Select.SubMeshGroupInGroup._matrix;
				targetObj = Editor.Select.SubMeshGroupInGroup;
				Editor.Select.SubMeshGroupInGroup._meshColor2X_Default = color;
				Editor.Select.SubMeshGroupInGroup._isVisible_Default = isVisible;
			}

			if (curRenderUnit == null)
			{ return; }



			//curRenderUnit.SetColor(color);
			Editor.RefreshControllerAndHierarchy();//Show/Hide 아이콘 갱신 땜시
			Editor.SetRepaint();
		}

		public apGizmos.TransformParam PivotReturn__MeshGroup_Setting()
		{

			if (Editor.Select.MeshGroup == null)
			{
				return null;
			}

			if (Editor.Select.SubMeshInGroup == null && Editor.Select.SubMeshGroupInGroup == null)
			{
				return null;
			}
			apRenderUnit curRenderUnit = null;
			apMatrix curMatrixParam = null;
			apMatrix resultMatrix = null;
			Color meshColor2X = Color.gray;
			bool isVisible = true;

			if (Editor.Select.SubMeshInGroup != null)
			{
				curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.SubMeshInGroup);
				curMatrixParam = Editor.Select.SubMeshInGroup._matrix;
				meshColor2X = Editor.Select.SubMeshInGroup._meshColor2X_Default;
				isVisible = Editor.Select.SubMeshInGroup._isVisible_Default;
			}
			else if (Editor.Select.SubMeshGroupInGroup != null)
			{
				curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.SubMeshGroupInGroup);
				curMatrixParam = Editor.Select.SubMeshGroupInGroup._matrix;
				meshColor2X = Editor.Select.SubMeshGroupInGroup._meshColor2X_Default;
				isVisible = Editor.Select.SubMeshGroupInGroup._isVisible_Default;
			}

			if (curRenderUnit == null)
			{
				return null;
			}

			if (curRenderUnit._meshTransform != null)
			{
				resultMatrix = curRenderUnit._meshTransform._matrix_TFResult_World;
			}
			else if (curRenderUnit._meshGroupTransform != null)
			{
				resultMatrix = curRenderUnit._meshGroupTransform._matrix_TFResult_World;
			}
			else
			{
				return null;
			}

			//Root의 MeshGroupTransform을 추가

			apMatrix curMatrixParam_Result = new apMatrix(curMatrixParam);
			curMatrixParam_Result.RMultiply(Editor.Select.MeshGroup._rootMeshGroupTransform._matrix);

			//TODO : Pivot 수정중엔 Calculated 데이터가 제외되어야 한다.
			//Vector3 posW3 = curRenderUnit.WorldMatrixOfNode.GetPosition();
			Vector2 posW2 = resultMatrix._pos;

			if (!Editor.Select.IsMeshGroupSettingChangePivot)
			{
				return apGizmos.TransformParam.Make(
												posW2,//<<Calculate를 포함한다.
													  //curMatrixParam._pos, 
													  //curMatrixParam_Result._angleDeg,
													  //curMatrixParam_Result._scale,
												resultMatrix._angleDeg,
												resultMatrix._scale,
												curRenderUnit.GetDepth(),
												//curRenderUnit.GetColor(),
												meshColor2X,
												isVisible,
												curRenderUnit.WorldMatrix,
												false,
												apGizmos.TRANSFORM_UI.Color,//색상만 설정 가능
												curMatrixParam._pos,
												curMatrixParam._angleDeg,
												curMatrixParam._scale);
			}
			else
			{
				return apGizmos.TransformParam.Make(
												//curMatrixParam_Result._pos,//<<Calculate를 포함한다.
												posW2,//<<Calculate를 포함한다.
													  //curMatrixParam._pos, 
													  //curMatrixParam_Result._angleDeg,
													  //curMatrixParam_Result._scale,
												resultMatrix._angleDeg,
												resultMatrix._scale,

												curRenderUnit.GetDepth(),
												//curRenderUnit.GetColor(),
												meshColor2X,
												isVisible,
												//curMatrixParam_Result.MtrxToSpace,
												curRenderUnit.WorldMatrix,
												false,
												//apGizmos.TRANSFORM_UI.TRS,
												apGizmos.TRANSFORM_UI.TRS | apGizmos.TRANSFORM_UI.Color,//색상도 포함시킨다.
												curMatrixParam._pos,
												curMatrixParam._angleDeg,
												curMatrixParam._scale
												);
			}

		}




	}

}