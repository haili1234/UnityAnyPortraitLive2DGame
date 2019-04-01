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

	//GizmoController -> Modifier [Vertex를 선택하는 타입]에 대한 내용이 담겨있다.
	public partial class apGizmoController
	{
		// 작성해야하는 함수
		// Select : int - (Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		// Move : void - (Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex)
		// Rotate : void - (float deltaAngleW)
		// Scale : void - (Vector2 deltaScaleW)

		//	TODO : 현재 Transform이 가능한지도 알아야 할 것 같다.
		// Transform Position : void - (Vector2 pos, int depth)
		// Transform Rotation : void - (float angle)
		// Transform Scale : void - (Vector2 scale)
		// Transform Color : void - (Color color)

		// Pivot Return : apGizmos.TransformParam - ()

		// Multiple Select : int - (Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, SELECT_TYPE areaSelectType)
		// FFD Style Transform : void - (List<object> srcObjects, List<Vector2> posWorlds)
		// FFD Style Transform Start : bool - ()

		// Vertex 전용 툴
		// SoftSelection() : bool
		// PressBlur(Vector2 pos, float tDelta) : bool



		//----------------------------------------------------------------
		// Gizmo - MeshGroup : Modifier / Morph계열 및 Vertex를 선택하는 Weight 계열의 모디파이어
		//----------------------------------------------------------------
		/// <summary>
		/// Modifier [Morph]에 대한 Gizmo Event의 Set이다.
		/// </summary>
		/// <returns></returns>
		public apGizmos.GizmoEventSet GetEventSet_Modifier_Morph()
		{
			//Morph는 Vertex / VertexPos 계열 이벤트를 사용하며, Color 처리를 한다.
			return new apGizmos.GizmoEventSet(
				Select__Modifier_Vertex,
				Unselect__Modifier_Vertex,
				Move__Modifier_VertexPos,
				Rotate__Modifier_VertexPos,
				Scale__Modifier_VertexPos,
				TransformChanged_Position__Modifier_VertexPos,
				null,
				null,
				TransformChanged_Color__Modifier_Vertex,
				PivotReturn__Modifier_Vertex,
				MultipleSelect__Modifier_Vertex,
				FFDTransform__Modifier_VertexPos,
				StartFFDTransform__Modifier_VertexPos,
				SoftSelection__Modifier_VertexPos,
				PressBlur__Modifier_VertexPos,
				apGizmos.TRANSFORM_UI.Position | apGizmos.TRANSFORM_UI.Vertex_Transform | apGizmos.TRANSFORM_UI.Color,
				FirstLink__Modifier_Vertex);
		}




		public apGizmos.SelectResult FirstLink__Modifier_Vertex()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			if (Editor.Select.ModRenderVertListOfMod != null)
			{
				//return Editor.Select.ModRenderVertListOfMod.Count;
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
			}
			return null;
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex 계열 선택시 [단일 선택]
		/// </summary>
		/// <param name="mousePosGL"></param>
		/// <param name="mousePosW"></param>
		/// <param name="btnIndex"></param>
		/// <param name="selectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult Select__Modifier_Vertex(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			//(Editing 상태일 때)
			//1. Vertex 선택
			//2. (Lock 걸리지 않았다면) 다른 Transform을 선택

			//(Editing 상태가 아닐 때)
			//(Lock 걸리지 않았다면) Transform을 선택한다.
			// Child 선택이 가능하면 MeshTransform을 선택. 그렇지 않아면 MeshGroupTransform을 선택해준다.

			if (Editor.Select.ModRenderVertListOfMod == null)
			{
				return null;
			}

			int prevSelectedCount = Editor.Select.ModRenderVertListOfMod.Count;

			if (!Editor.Controller.IsMouseInGUI(mousePosGL))
			{
				//return prevSelectedCount;
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
			}

			bool isChildMeshTransformSelectable = Editor.Select.Modifier.IsTarget_ChildMeshTransform;

			//apGizmos.SELECT_RESULT result = apGizmos.SELECT_RESULT.None;

			bool isTransformSelectable = false;
			if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None)
			{
				//(Editing 상태일 때)
				//1. Vertex 선택
				//2. (Lock 걸리지 않았다면) 다른 Transform을 선택
				bool selectVertex = false;
				if (Editor.Select.ExKey_ModMesh != null && Editor.Select.MeshGroup != null)
				{
					//일단 선택한 Vertex가 클릭 가능한지 체크
					if (Editor.Select.ModRenderVertOfMod != null)
					{
						if (Editor.Select.ModRenderVertListOfMod.Count == 1)
						{
							if (Editor.Controller.IsVertexClickable(apGL.World2GL(Editor.Select.ModRenderVertOfMod._renderVert._pos_World), mousePosGL))
							{
								if (selectType == apGizmos.SELECT_TYPE.Subtract)
								{
									//삭제인 경우
									Editor.Select.RemoveModVertexOfModifier(Editor.Select.ModRenderVertOfMod._modVert, null, null, Editor.Select.ModRenderVertOfMod._renderVert);
									
									//return apGizmos.SELECT_RESULT.None;
									if(Editor.Select.ModRenderVertListOfMod.Count > 0)
									{
										selectVertex = true;
									}
								}
								else
								{
									//그 외에는 => 그대로 갑시다.
									selectVertex = true;
									//return apGizmos.SELECT_RESULT.SameSelected;
								}
								//return Editor.Select.ModRenderVertListOfMod.Count;
								return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
							}
						}
						else
						{
							//여러개라고 하네요.
							List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;
							for (int iModRenderVert = 0; iModRenderVert < modRenderVerts.Count; iModRenderVert++)
							{
								apSelection.ModRenderVert modRenderVert = modRenderVerts[iModRenderVert];

								if (Editor.Controller.IsVertexClickable(apGL.World2GL(modRenderVert._renderVert._pos_World), mousePosGL))
								{
									if (selectType == apGizmos.SELECT_TYPE.Subtract)
									{
										//삭제인 경우
										//하나 지우고 끝
										//결과는 List의 개수
										Editor.Select.RemoveModVertexOfModifier(modRenderVert._modVert, null, null, modRenderVert._renderVert);
										//return apGizmos.SELECT_RESULT.None;
										//return Editor.Select.ModRenderVertOfModList.Count;
										//if(Editor.Select.ModRenderVertOfModList.Count == 0)
										//{
										//	return apGizmos.SELECT_RESULT.None;
										//}
										//else
										//{
										//	return apGizmos.SELECT_RESULT.NewSelected;
										//}
										
										if(Editor.Select.ModRenderVertListOfMod.Count > 0)
										{
											selectVertex = true;
										}
									}
									else if (selectType == apGizmos.SELECT_TYPE.Add)
									{
										//Add 상태에서 원래 선택된걸 누른다면
										//추가인 경우 => 그대로
										selectVertex = true;
										//return apGizmos.SELECT_RESULT.SameSelected;
										//return Editor.Select.ModRenderVertOfModList.Count;
									}
									else
									{
										//만약... new 라면?
										//다른건 초기화하고
										//얘만 선택해야함
										apRenderVertex selectedRenderVert = modRenderVert._renderVert;
										apModifiedVertex selectedModVert = modRenderVert._modVert;
										Editor.Select.SetModVertexOfModifier(null, null, null, null);
										Editor.Select.SetModVertexOfModifier(selectedModVert, null, null, selectedRenderVert);
										//return apGizmos.SELECT_RESULT.NewSelected;
										//return Editor.Select.ModRenderVertOfModList.Count;
									}

									//return Editor.Select.ModRenderVertListOfMod.Count;
									return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
								}
							}
						}

					}

					if (selectType == apGizmos.SELECT_TYPE.New)
					{
						//Add나 Subtract가 아닐땐, 잘못 클릭하면 선택을 해제하자 (전부)
						Editor.Select.SetModVertexOfModifier(null, null, null, null);
					}

					if (selectType != apGizmos.SELECT_TYPE.Subtract)
					{
						if (Editor.Select.ExKey_ModMesh._transform_Mesh != null &&
								Editor.Select.ExKey_ModMesh._vertices != null)
						{
							//선택된 RenderUnit을 고르자
							apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ExKey_ModMesh._transform_Mesh);

							if (targetRenderUnit != null)
							{
								for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
								{
									apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];
									bool isClick = Editor.Controller.IsVertexClickable(apGL.World2GL(renderVert._pos_World), mousePosGL);
									if (isClick)
									{
										apModifiedVertex selectedModVert = Editor.Select.ExKey_ModMesh._vertices.Find(delegate (apModifiedVertex a)
										{
											return renderVert._vertex._uniqueID == a._vertexUniqueID;
										});

										if (selectedModVert != null)
										{
											if (selectType == apGizmos.SELECT_TYPE.New)
											{
												Editor.Select.SetModVertexOfModifier(selectedModVert, null, null, renderVert);
											}
											else if (selectType == apGizmos.SELECT_TYPE.Add)
											{
												Editor.Select.AddModVertexOfModifier(selectedModVert, null, null, renderVert);
											}

											selectVertex = true;
											//result = apGizmos.SELECT_RESULT.NewSelected;
											break;
										}

									}
								}
							}
						}
					}
				}

				//Vertex를 선택한게 없다면
				//+ Lock 상태가 아니라면
				if (!selectVertex && !Editor.Select.IsLockExEditKey)
				{
					//Transform을 선택
					isTransformSelectable = true;
				}
			}
			else
			{
				//(Editing 상태가 아닐때)
				isTransformSelectable = true;

				if (Editor.Select.ExKey_ModMesh != null && Editor.Select.IsLockExEditKey)
				{
					//뭔가 선택된 상태에서 Lock이 걸리면 다른건 선택 불가
					isTransformSelectable = false;
				}
			}

			if (isTransformSelectable && selectType == apGizmos.SELECT_TYPE.New)
			{
				//(Editing 상태가 아닐 때)
				//Transform을 선택한다.

				apTransform_Mesh selectedMeshTransform = null;

				List<apRenderUnit> renderUnits = Editor.Select.MeshGroup._renderUnits_All;//<<정렬된 Render Unit
				for (int iUnit = 0; iUnit < renderUnits.Count; iUnit++)
				{
					apRenderUnit renderUnit = renderUnits[iUnit];
					if (renderUnit._meshTransform != null && renderUnit._meshTransform._mesh != null)
					{
						if (renderUnit._meshTransform._isVisible_Default && renderUnit._meshColor2X.a > 0.1f)//Alpha 옵션 추가
						{
							//Debug.LogError("TODO : Mouse Picking 바꿀것");
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
					//만약 ChildMeshGroup에 속한 거라면,
					//Mesh Group 자체를 선택해야 한다. <- 추가 : Child Mesh Transform이 허용되는 경우 그럴 필요가 없다.
					apMeshGroup parentMeshGroup = Editor.Select.MeshGroup.FindParentMeshGroupOfMeshTransform(selectedMeshTransform);
					if (parentMeshGroup == null || parentMeshGroup == Editor.Select.MeshGroup || isChildMeshTransformSelectable)
					{
						Editor.Select.SetSubMeshInGroup(selectedMeshTransform);
					}
					else
					{
						apTransform_MeshGroup childMeshGroupTransform = Editor.Select.MeshGroup.FindChildMeshGroupTransform(parentMeshGroup);
						if (childMeshGroupTransform != null)
						{
							Editor.Select.SetSubMeshGroupInGroup(childMeshGroupTransform);
						}
						else
						{
							Editor.Select.SetSubMeshInGroup(selectedMeshTransform);
						}
					}
				}
				else
				{
					Editor.Select.SetSubMeshInGroup(null);
				}

				Editor.RefreshControllerAndHierarchy();
				//Editor.Repaint();
				Editor.SetRepaint();
			}

			//개수에 따라 한번더 결과 보정
			if (Editor.Select.ModRenderVertListOfMod != null)
			{
				//return Editor.Select.ModRenderVertListOfMod.Count;
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
			}
			return null;
		}



		public void Unselect__Modifier_Vertex()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}
			if(Editor.Gizmos.IsFFDMode)
			{
				//Debug.Log("IsFFD Mode");
				//추가 : FFD 모드에서는 버텍스 취소가 안된다.
				return;
			}

			Editor.Select.SetModVertexOfModifier(null, null, null, null);
			if (!Editor.Select.IsLockExEditKey)
			{
				//SubMesh 해제를 위해서는 Lock이 풀려있어야함
				Editor.Select.SetSubMeshInGroup(null);
			}

			Editor.RefreshControllerAndHierarchy();
			Editor.SetRepaint();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex 계열 선택시 [복수 선택]
		/// </summary>
		/// <param name="mousePosGL_Min"></param>
		/// <param name="mousePosGL_Max"></param>
		/// <param name="mousePosW_Min"></param>
		/// <param name="mousePosW_Max"></param>
		/// <param name="areaSelectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult MultipleSelect__Modifier_Vertex(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, apGizmos.SELECT_TYPE areaSelectType)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}


			if (Editor.Select.ModRenderVertListOfMod == null)
			{
				return null;
			}
			// 이건 다중 버텍스 선택밖에 없다.
			//Transform 선택은 없음

			//if (!Editor.Controller.IsMouseInGUI(mousePosGL))
			//{
			//	return apGizmos.SELECT_RESULT.None;
			//}

			//apGizmos.SELECT_RESULT result = apGizmos.SELECT_RESULT.None;

			bool isAnyChanged = false;
			if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None && Editor.Select.ExKey_ModMesh != null && Editor.Select.MeshGroup != null)
			{
				//선택된 RenderUnit을 고르자
				apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ExKey_ModMesh._transform_Mesh);

				if (targetRenderUnit != null)
				{
					for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
					{
						apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];
						bool isSelectable = (mousePosW_Min.x < renderVert._pos_World.x && renderVert._pos_World.x < mousePosW_Max.x)
									&& (mousePosW_Min.y < renderVert._pos_World.y && renderVert._pos_World.y < mousePosW_Max.y);
						if (isSelectable)
						{
							apModifiedVertex selectedModVert = Editor.Select.ExKey_ModMesh._vertices.Find(delegate (apModifiedVertex a)
							{
								return renderVert._vertex._uniqueID == a._vertexUniqueID;
							});

							if (selectedModVert != null)
							{
								if (areaSelectType == apGizmos.SELECT_TYPE.Add ||
									areaSelectType == apGizmos.SELECT_TYPE.New)
								{
									Editor.Select.AddModVertexOfModifier(selectedModVert, null, null, renderVert);
								}
								else
								{
									Editor.Select.RemoveModVertexOfModifier(selectedModVert, null, null, renderVert);
								}

								isAnyChanged = true;
								//result = apGizmos.SELECT_RESULT.NewSelected;
								//break;
							}

						}
					}

					Editor.RefreshControllerAndHierarchy();
					//Editor.Repaint();
					Editor.SetRepaint();
				}


			}


			if (isAnyChanged)
			{
				Editor.Select.AutoSelectModMeshOrModBone();
			}

			//return Editor.Select.ModRenderVertListOfMod.Count;
			return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
		}


		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex의 위치값을 수정할 때 [Move]
		/// </summary>
		/// <param name="curMouseGL"></param>
		/// <param name="curMousePosW"></param>
		/// <param name="deltaMoveW"></param>
		/// <param name="btnIndex"></param>
		public void Move__Modifier_VertexPos(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}

			if (deltaMoveW.sqrMagnitude == 0.0f && !isFirstMove)
			{
				return;
			}

			//(Editing 상태일 때)
			//1. 선택된 Vertex가 있다면
			//2. 없다면 -> 패스

			//(Editng 상태가 아니면)
			// 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || Editor.Select.ExKey_ModMesh == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			if (!Editor.Controller.IsMouseInGUI(curMouseGL))
			{
				return;
			}

			if (Editor.Select.ModRenderVertOfMod == null)
			{
				return;
			}

			//Undo
			bool isMultipleVerts = true;
			object targetVert = null;
			if (Editor.Select.ModRenderVertListOfMod.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			{
				targetVert = Editor.Select.ModRenderVertListOfMod[0];
				isMultipleVerts = false;
			}

			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_MoveVertex, Editor, Editor.Select.Modifier, targetVert, isMultipleVerts);
			}



			if (Editor.Select.ModRenderVertListOfMod.Count == 1)
			{
				//1. 단일 선택일 때
				apRenderVertex renderVert = Editor.Select.ModRenderVertOfMod._renderVert;
				renderVert.Calculate(0.0f);

				Vector2 prevDeltaPos2 = Editor.Select.ModRenderVertOfMod._modVert._deltaPos;
				//Vector3 prevDeltaPos3 = new Vector3(prevDeltaPos2.x, prevDeltaPos2.y, 0);

				apMatrix3x3 martrixMorph = apMatrix3x3.TRS(prevDeltaPos2, 0, Vector2.one);
				Vector2 prevWorldPos2 = (renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform * martrixMorph * renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);
				Vector2 nextWorldPos = new Vector2(prevWorldPos2.x, prevWorldPos2.y) + deltaMoveW;
				//Vector3 nextWorldPos3 = new Vector3(nextWorldPos.x, nextWorldPos.y, 0);

				//NextWorld Pos에서 -> [VertWorld] -> [MeshTransform] -> Vert Local 적용 후의 좌표 -> Vert Local 적용 전의 좌표 
				//적용 전-후의 좌표 비교 = 그 차이값을 ModVert에 넣자
				//기존 계산 : Matrix를 구해서 일일이 계산한다.
				Vector2 noneMorphedPosM = (renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);
				Vector2 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse).MultiplyPoint(nextWorldPos);

				Editor.Select.ModRenderVertOfMod._modVert._deltaPos.x = (nextMorphedPosM.x - noneMorphedPosM.x);
				Editor.Select.ModRenderVertOfMod._modVert._deltaPos.y = (nextMorphedPosM.y - noneMorphedPosM.y);
			}
			else
			{
				//2. 복수개 선택일 때
				for (int i = 0; i < Editor.Select.ModRenderVertListOfMod.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfMod[i];

					apRenderVertex renderVert = modRenderVert._renderVert;

					Vector2 nextWorldPos = renderVert._pos_World + deltaMoveW;

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
					//apMatrix3x3 matToAfterVertLocal = (renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse;
					//Vector3 nextLocalMorphedPos = matToAfterVertLocal.MultiplyPoint3x4(new Vector3(nextWorldPos.x, nextWorldPos.y, 0));
					//Vector3 beforeLocalMorphedPos = (renderVert._matrix_Cal_VertLocal * renderVert._matrix_Static_Vert2Mesh).MultiplyPoint3x4(renderVert._vertex._pos);


					//modRenderVert._modVert._deltaPos.x += (nextLocalMorphedPos.x - beforeLocalMorphedPos.x);
					//modRenderVert._modVert._deltaPos.y += (nextLocalMorphedPos.y - beforeLocalMorphedPos.y);
				}
			}

			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVertListOfMod_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVertListOfMod_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfMod_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos = (renderVert._pos_World + deltaMoveW) * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
			Editor.Select.MeshGroup.RefreshForce();
			//Editor.RefreshControllerAndHierarchy();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex의 위치값을 수정할 때 [Rotate]
		/// </summary>
		/// <param name="deltaAngleW"></param>
		public void Rotate__Modifier_VertexPos(float deltaAngleW, bool isFirstRotate)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}

			if (deltaAngleW == 0.0f && !isFirstRotate)
			{
				return;
			}

			//(Editng 상태가 아니면)
			// 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || Editor.Select.ExKey_ModMesh == null)
			{
				return;
			}

			if (Editor.Select.ModRenderVertListOfMod == null)
			{
				return;
			}

			if (Editor.Select.ModRenderVertListOfMod.Count <= 1)
			{
				return;
			}

			Vector2 centerPos = Editor.Select.ModRenderVertsCenterPosOfMod;
			//Vector3 centerPos3 = new Vector3(centerPos.x, centerPos.y, 0.0f);

			if (deltaAngleW > 180.0f)
			{ deltaAngleW -= 360.0f; }
			else if (deltaAngleW < -180.0f)
			{ deltaAngleW += 360.0f; }

			//Quaternion quat = Quaternion.Euler(0.0f, 0.0f, deltaAngleW);

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);


			//Undo
			bool isMultipleVerts = true;
			object targetVert = null;
			if (Editor.Select.ModRenderVertListOfMod.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			{
				targetVert = Editor.Select.ModRenderVertListOfMod[0];
				isMultipleVerts = false;
			}

			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_RotateVertex, Editor, Editor.Select.Modifier, targetVert, isMultipleVerts);
			}

			//선택된 RenderVert의 Mod 값을 바꾸자
			for (int i = 0; i < Editor.Select.ModRenderVertListOfMod.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfMod[i];

				Vector2 nextWorldPos = matrix_Rotate.MultiplyPoint(modRenderVert._renderVert._pos_World);

				modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
			}


			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVertListOfMod_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVertListOfMod_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfMod_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(modRenderVert._renderVert._pos_World);
					Vector2 nextWorldPos = (nextWorldPos2) * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
			Editor.Select.MeshGroup.RefreshForce();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex의 위치값을 수정할 때 [Scale]
		/// </summary>
		/// <param name="deltaScaleW"></param>
		public void Scale__Modifier_VertexPos(Vector2 deltaScaleW, bool isFirstScale)
		{
			if (Editor.Select.MeshGroup == null)
			{
				return;
			}

			if (deltaScaleW.sqrMagnitude == 0.0f && isFirstScale)
			{
				return;
			}

			//(Editng 상태가 아니면)
			// 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || Editor.Select.ExKey_ModMesh == null)
			{
				return;
			}

			if (Editor.Select.ModRenderVertListOfMod == null)
			{
				return;
			}

			if (Editor.Select.ModRenderVertListOfMod.Count <= 1)
			{
				return;
			}

			Vector2 centerPos = Editor.Select.ModRenderVertsCenterPosOfMod;
			//Vector3 centerPos3 = new Vector3(centerPos.x, centerPos.y, 0.0f);

			Vector2 scale = new Vector2(1.0f + deltaScaleW.x, 1.0f + deltaScaleW.y);

			apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, 0, scale)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);


			//Undo
			bool isMultipleVerts = true;
			object targetVert = null;
			if (Editor.Select.ModRenderVertListOfMod.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			{
				targetVert = Editor.Select.ModRenderVertListOfMod[0];
				isMultipleVerts = false;
			}

			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_ScaleVertex, Editor, Editor.Select.Modifier, targetVert, isMultipleVerts);
			}



			//선택된 RenderVert의 Mod 값을 바꾸자
			for (int i = 0; i < Editor.Select.ModRenderVertListOfMod.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfMod[i];

				Vector2 nextWorldPos = matrix_Scale.MultiplyPoint(modRenderVert._renderVert._pos_World);

				modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
			}

			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVertListOfMod_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVertListOfMod_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfMod_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(modRenderVert._renderVert._pos_World);
					Vector2 nextWorldPos = (nextWorldPos2) * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
			Editor.Select.MeshGroup.RefreshForce();

			//apMatrix targetMatrix;
			//Vector2 scale2 = new Vector2(targetMatrix._scale.x, targetMatrix._scale.y);
			//targetMatrix.SetScale(deltaScaleW + scale2);
			//targetMatrix.MakeMatrix();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Vertex의 위치값 [Position]
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="depth"></param>
		public void TransformChanged_Position__Modifier_VertexPos(Vector2 pos, int depth)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.ExEditingMode == apSelection.EX_EDIT.None ||
				Editor.Select.ExKey_ModMesh == null ||
				Editor.Select.ModRenderVertOfMod == null ||
				Editor.Select.RenderUnitOfMod == null ||
				Editor.Select.ModRenderVertListOfMod.Count == 0)
			{
				//편집 가능한 상태가 아니면 패스
				return;
			}

			if (Editor.Select.SubMeshInGroup == null && Editor.Select.SubMeshGroupInGroup == null)
			{
				return;
			}

			//Vector2 deltaPosW = Vector2.zero;
			Vector2 deltaPosChanged = Vector2.zero;

			//Undo
			bool isMultipleVerts = true;
			object targetVert = null;
			if (Editor.Select.ModRenderVertListOfMod.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			{
				targetVert = Editor.Select.ModRenderVertListOfMod[0];
				isMultipleVerts = false;
			}
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_MoveVertex, Editor, Editor.Select.Modifier, targetVert, isMultipleVerts);


			//Depth는 신경쓰지 말자
			if (Editor.Select.ModRenderVertListOfMod.Count == 1)
			{
				//수정 : 직접 대입한다.
				deltaPosChanged = pos - Editor.Select.ModRenderVertOfMod._modVert._deltaPos;

				Editor.Select.ModRenderVertOfMod._modVert._deltaPos = pos;


				////단일 선택시
				////원래 위치와 입력된 위치의 변화값을 넣어주자
				//apRenderVertex renderVert = Editor.Select.ModRenderVertOfMod._renderVert;
				//renderVert.Calculate(0.0f);

				//Vector2 prevDeltaPos2 = Editor.Select.ModRenderVertOfMod._modVert._deltaPos;
				//Vector3 prevDeltaPos3 = new Vector3(prevDeltaPos2.x, prevDeltaPos2.y, 0);
				//apMatrix3x3 martrixMorph = apMatrix3x3.TRS(prevDeltaPos3, Quaternion.identity, Vector3.one);

				//Vector2 prevPosW = Editor.Select.ModRenderVertOfMod._renderVert._pos_World;

				//deltaPosW = pos - prevPosW;

				//Vector3 prevPosW3 = new Vector3(prevPosW.x, prevPosW.y, 0);
				//Vector3 nextPosW3 = new Vector3(pos.x, pos.y, 0);

				//Vector3 noneMorphedPosM = (renderVert._matrix_Static_Vert2Mesh).MultiplyPoint3x4(renderVert._vertex._pos);
				//Vector3 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse).MultiplyPoint3x4(nextPosW3);

				//Editor.Select.ModRenderVertOfMod._modVert._deltaPos.x = (nextMorphedPosM.x - noneMorphedPosM.x);
				//Editor.Select.ModRenderVertOfMod._modVert._deltaPos.y = (nextMorphedPosM.y - noneMorphedPosM.y);

				//강제로 업데이트할 객체 선택하고 Refresh
				//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
				Editor.Select.MeshGroup.RefreshForce();
			}
			else
			{
				//복수 선택시
				//수정 : 
				//AvgCenterDeltaPos의 변화값을 대입한다.
				Vector2 avgDeltaPos = Vector2.zero;
				for (int i = 0; i < Editor.Select.ModRenderVertListOfMod.Count; i++)
				{
					avgDeltaPos += Editor.Select.ModRenderVertListOfMod[i]._modVert._deltaPos;
				}
				avgDeltaPos /= Editor.Select.ModRenderVertListOfMod.Count;

				Vector2 deltaPos2Next = pos - avgDeltaPos;
				deltaPosChanged = deltaPos2Next;

				for (int i = 0; i < Editor.Select.ModRenderVertListOfMod.Count; i++)
				{
					Editor.Select.ModRenderVertListOfMod[i]._modVert._deltaPos += deltaPos2Next;
				}

				//음... 전체 위치 Delta값을 이용해서 처리를 해야겠넹
				//Vector2 prevCenterPosW = Editor.Select.ModRenderVertsCenterPosOfMod;

				//deltaPosW = pos - prevCenterPosW;
				//Vector3 deltaPosW3 = new Vector3(deltaPosW.x, deltaPosW.y, 0);

				//for (int i = 0; i < Editor.Select.ModRenderVertListOfMod.Count; i++)
				//{
				//	apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfMod[i];

				//	apRenderVertex renderVert = modRenderVert._renderVert;

				//	Vector2 prevPosW = renderVert._pos_World;
				//	Vector3 prevPosW3 = new Vector3(prevPosW.x, prevPosW.y, 0);
				//	Vector3 nextPosW3 = prevPosW3 + deltaPosW3;

				//	Vector3 noneMorphedPosM = (renderVert._matrix_Static_Vert2Mesh).MultiplyPoint3x4(renderVert._vertex._pos);
				//	Vector3 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse).MultiplyPoint3x4(nextPosW3);

				//	modRenderVert._modVert._deltaPos.x = (nextMorphedPosM.x - noneMorphedPosM.x);
				//	modRenderVert._modVert._deltaPos.y = (nextMorphedPosM.y - noneMorphedPosM.y);
				//}
			}

			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVertListOfMod_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVertListOfMod_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfMod_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					//apRenderVertex renderVert = modRenderVert._renderVert;

					////Weight를 적용한 만큼만 움직이자
					//Vector2 nextWorldPos = (renderVert._pos_World + deltaPosW) * weight + (renderVert._pos_World) * (1.0f - weight);

					//modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);

					//변경 : DeltaPos의 변경 값으로만 계산한다.
					modRenderVert._modVert._deltaPos = ((modRenderVert._modVert._deltaPos + deltaPosChanged) * weight) + (modRenderVert._modVert._deltaPos * (1.0f - weight));
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
			Editor.Select.MeshGroup.RefreshForce();
		}



		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Transform의 색상값 [Color]
		/// </summary>
		/// <param name="color"></param>
		public void TransformChanged_Color__Modifier_Vertex(Color color, bool isVisible)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			//Editing 상태가 아니면 패스 + ModMesh가 없다면 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || Editor.Select.ExKey_ModMesh == null || Editor.Select.ExKey_ModParamSet == null)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_Color, Editor, Editor.Select.Modifier, Editor.Select.ExKey_ModMesh, false);

			Editor.Select.ExKey_ModMesh._meshColor = color;
			Editor.Select.ExKey_ModMesh._isVisible = isVisible;

			//강제로 업데이트할 객체 선택하고 Refresh
			//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
			Editor.Select.MeshGroup.RefreshForce();
		}


		public apGizmos.TransformParam PivotReturn__Modifier_Vertex()
		{

			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			if (Editor.Select.SubMeshInGroup == null && Editor.Select.SubMeshGroupInGroup == null)
			{
				return null;
			}



			if (Editor.Select.RenderUnitOfMod == null)
			{
				return null;
			}



			//(Editng 상태가 아니면)
			// 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || Editor.Select.ExKey_ModMesh == null)
			{
				return null;
			}

			if (Editor.Select.ModRenderVertOfMod == null)
			{
				//Vert 선택한게 없다면
				//Color 옵션만이라도 설정하게 하자
				if (Editor.Select.Modifier._isColorPropertyEnabled)
				{
					return apGizmos.TransformParam.Make(apEditorUtil.InfVector2, 0.0f, Vector2.one, 0,
													Editor.Select.ExKey_ModMesh._meshColor,
													Editor.Select.ExKey_ModMesh._isVisible,
													apMatrix3x3.identity,
													false,
													apGizmos.TRANSFORM_UI.Color,
													Vector2.zero, 0.0f, Vector2.one
													);
				}
				return null;
			}

			//TODO : 여러개의 Vert를 수정할 수 있도록 한다.
			apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.Position;
			if (Editor.Select.Modifier._isColorPropertyEnabled)
			{
				paramType |= apGizmos.TRANSFORM_UI.Color;//<Color를 지원하는 경우에만
			}

			if (Editor.Select.ModRenderVertListOfMod.Count > 1)
			{
				paramType |= apGizmos.TRANSFORM_UI.Vertex_Transform;

				Vector2 avgDeltaPos = Vector2.zero;
				for (int i = 0; i < Editor.Select.ModRenderVertListOfMod.Count; i++)
				{
					avgDeltaPos += Editor.Select.ModRenderVertListOfMod[i]._modVert._deltaPos;
				}
				avgDeltaPos /= Editor.Select.ModRenderVertListOfMod.Count;

				
				return apGizmos.TransformParam.Make(Editor.Select.ModRenderVertsCenterPosOfMod,
													0.0f,
													Vector2.one,
													Editor.Select.RenderUnitOfMod.GetDepth(),
													//Editor.Select.RenderUnitOfMod.GetColor(), 
													Editor.Select.ExKey_ModMesh._meshColor,
													Editor.Select.ExKey_ModMesh._isVisible,
													apMatrix3x3.identity,
													true,
													paramType,
													//apGizmos.TRANSFORM_UI.Position |
													//apGizmos.TRANSFORM_UI.FFD_Transform |
													//apGizmos.TRANSFORM_UI.Color,
													avgDeltaPos,
													0.0f,
													Vector2.one
													);
			}
			else
			{
				return apGizmos.TransformParam.Make(Editor.Select.ModRenderVertOfMod._renderVert._pos_World,
													0.0f,
													Vector2.one,
													Editor.Select.RenderUnitOfMod.GetDepth(),
													//Editor.Select.RenderUnitOfMod.GetColor(), 
													Editor.Select.ExKey_ModMesh._meshColor,
													Editor.Select.ExKey_ModMesh._isVisible,
													Editor.Select.ModRenderVertOfMod._renderVert._matrix_ToWorld,
													false,
													paramType,
													Editor.Select.ModRenderVertOfMod._modVert._deltaPos,
													0.0f,
													Vector2.one
													//apGizmos.TRANSFORM_UI.Position |
													//apGizmos.TRANSFORM_UI.Color
													);
			}
		}

		public bool FFDTransform__Modifier_VertexPos(List<object> srcObjects, List<Vector2> posWorlds, bool isResultAssign)
		{
			if (!isResultAssign)
			{
				//결과 적용이 아닌 일반 수정 작업시
				//-> 수정이 불가능한 경우에는 불가하다고 리턴한다.
				if (Editor.Select.ModRenderVertListOfMod == null)
				{
					return false;
				}

				if (Editor.Select.ModRenderVertListOfMod.Count <= 1)
				{
					return false;
				}
			}

			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_FFDVertex, Editor, Editor.Select.Modifier, null, true);

			for (int i = 0; i < srcObjects.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = srcObjects[i] as apSelection.ModRenderVert;
				Vector2 worldPos = posWorlds[i];

				if (modRenderVert == null)
				{
					continue;
				}

				//if (Mathf.Abs(modRenderVert._renderVert._pos_World.x - worldPos.x) > 0.0001f ||
				//	Mathf.Abs(modRenderVert._renderVert._pos_World.y - worldPos.y) > 0.0001f)
				//{
				//	modRenderVert.SetWorldPosToModifier_VertLocal(worldPos);
				//}

				modRenderVert.SetWorldPosToModifier_VertLocal(worldPos);
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			//if (Editor.Select.ExKey_ModMesh != null)
			//{
			//	Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
			//}
			Editor.Select.MeshGroup.RefreshForce();
			return true;
		}

		public bool StartFFDTransform__Modifier_VertexPos()
		{
			if (Editor.Select.MeshGroup == null)
			{
				return false;
			}
			if (Editor.Select.ModRenderVertListOfMod == null)
			{
				return false;
			}

			if (Editor.Select.ModRenderVertListOfMod.Count <= 1)
			{
				return false;
			}

			List<object> srcObjectList = new List<object>();
			List<Vector2> worldPosList = new List<Vector2>();
			for (int i = 0; i < Editor.Select.ModRenderVertListOfMod.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfMod[i];
				srcObjectList.Add(modRenderVert);
				worldPosList.Add(modRenderVert._renderVert._pos_World);
			}
			Editor.Gizmos.RegistTransformedObjectList(srcObjectList, worldPosList);//<<True로 리턴할거면 이 함수를 호출해주자
			return true;
		}

		public bool SoftSelection__Modifier_VertexPos()
		{
			Editor.Select.ModRenderVertListOfMod_Weighted.Clear();

			if (Editor.Select.MeshGroup == null)
			{
				return false;
			}

			if (Editor.Select.ModRenderVertListOfMod == null)
			{
				return false;
			}

			if (Editor.Select.ModRenderVertListOfMod.Count <= 0)
			{
				return false;
			}

			float radius = (float)Editor.Gizmos.SoftSelectionRadius;
			if (radius <= 0.0f)
			{
				return false;
			}

			bool isConvex = Editor.Gizmos.SoftSelectionCurveRatio >= 0;
			float curveRatio = Mathf.Clamp01(Mathf.Abs((float)Editor.Gizmos.SoftSelectionCurveRatio / 100.0f));//0이면 직선, 1이면 커브(볼록/오목)

			//선택되지 않은 Vertex 중에서
			//"기본 위치 값"을 기준으로 영역을 선택해주자
			float minDist = 0.0f;
			float dist = 0.0f;
			apSelection.ModRenderVert minRV = null;

			if (Editor.Select.ExKey_ModMesh._transform_Mesh != null && Editor.Select.ExKey_ModMesh._vertices != null)
			{
				//선택된 RenderUnit을 고르자
				apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ExKey_ModMesh._transform_Mesh);

				if (targetRenderUnit != null)
				{
					for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
					{
						apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];

						//선택된 RenderVert는 제외한다.
						if (Editor.Select.ModRenderVertListOfMod.Exists(delegate (apSelection.ModRenderVert a)
						 {
							 return a._renderVert == renderVert;
						 }))
						{
							continue;
						}


						//가장 가까운 RenderVert를 찾는다.
						minDist = 0.0f;
						dist = 0.0f;
						minRV = null;

						for (int iSelectedRV = 0; iSelectedRV < Editor.Select.ModRenderVertListOfMod.Count; iSelectedRV++)
						{
							apSelection.ModRenderVert selectedModRV = Editor.Select.ModRenderVertListOfMod[iSelectedRV];
							//현재 World위치로 선택해보자.

							dist = Vector2.Distance(selectedModRV._renderVert._pos_World, renderVert._pos_World);
							if (dist < minDist || minRV == null)
							{
								minRV = selectedModRV;
								minDist = dist;
							}
						}

						if (minRV != null && minDist <= radius)
						{
							apModifiedVertex modVert = Editor.Select.ExKey_ModMesh._vertices.Find(delegate (apModifiedVertex a)
							{
								return renderVert._vertex._uniqueID == a._vertexUniqueID;
							});

							if (modVert != null)
							{

								//radius에 들어가는 Vert 발견.
								//Weight는 CurveRatio에 맞게 (minDist가 0에 가까울수록 Weight는 1이 된다.)
								float itp_Linear = minDist / radius;
								float itp_Curve = 0.0f;
								if (isConvex)
								{
									//Weight가 더 1에 가까워진다. => minDist가 0이 되는 곳에 Control Point를 넣자
									itp_Curve = (1.0f * (itp_Linear * itp_Linear))
										+ (2.0f * 0.0f * itp_Linear * (1.0f - itp_Linear))
										+ (0.0f * (1.0f - itp_Linear) * (1.0f - itp_Linear));
								}
								else
								{
									//Weight가 더 0에 가까워진다. => minDist가 radius가 되는 곳에 Control Point를 넣자
									itp_Curve = (1.0f * (itp_Linear * itp_Linear))
										+ (2.0f * 1.0f * itp_Linear * (1.0f - itp_Linear))
										+ (0.0f * (1.0f - itp_Linear) * (1.0f - itp_Linear));
								}
								float itp = itp_Linear * (1.0f - curveRatio) + itp_Curve * curveRatio;
								float weight = 0.0f * itp + 1.0f * (1.0f - itp);

								apSelection.ModRenderVert newModRenderVert = new apSelection.ModRenderVert(modVert, renderVert);
								//Weight를 추가로 넣어주고 리스트에 넣자
								newModRenderVert._vertWeightByTool = weight;

								Editor.Select.ModRenderVertListOfMod_Weighted.Add(newModRenderVert);
							}
						}
					}
				}
			}

			return true;
		}

		private List<apModifiedVertex> _tmpBlurVertices = new List<apModifiedVertex>();
		private List<float> _tmpBlurVertexWeights = new List<float>();

		public bool PressBlur__Modifier_VertexPos(Vector2 pos, float tDelta, bool isFirstBlur)
		{
			if (Editor.Select.ExKey_ModMesh._transform_Mesh == null
				|| Editor.Select.ExKey_ModMesh._vertices == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count <= 1)
			{
				return false;
			}

			float radius = Editor.Gizmos.BlurRadius;
			float intensity = Mathf.Clamp01((float)Editor.Gizmos.BlurIntensity / 100.0f);

			if (radius <= 0.0f || intensity <= 0.0f)
			{
				return false;
			}

			_tmpBlurVertices.Clear();
			_tmpBlurVertexWeights.Clear();


			Vector2 totalModValue = Vector2.zero;
			float totalWeight = 0.0f;


			//1. 영역 안의 Vertex를 선택하자 + 마우스 중심점을 기준으로 Weight를 구하자 + ModValue의 가중치가 포함된 평균을 구하자
			//2. 가중치가 포함된 평균값만큼 tDelta * intensity * weight로 바꾸어주자

			//영역 체크는 GL값
			float dist = 0.0f;
			float weight = 0.0f;
			if(isFirstBlur)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_BlurVertex, Editor, Editor.Select.Modifier, null, false);
			}

			//선택된 Vert에 한해서만 처리하자
			for (int i = 0; i < Editor.Select.ModRenderVertListOfMod.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfMod[i];
				dist = Vector2.Distance(modRenderVert._renderVert._pos_GL, pos);
				if (dist > radius)
				{
					continue;
				}

				weight = (radius - dist) / radius;
				totalModValue += modRenderVert._modVert._deltaPos * weight;
				totalWeight += weight;

				_tmpBlurVertices.Add(modRenderVert._modVert);
				_tmpBlurVertexWeights.Add(weight);
			}

			if (_tmpBlurVertices.Count > 0 && totalWeight > 0.0f)
			{
				//Debug.Log("Blur : " + _tmpBlurVertices.Count + "s Verts / " + totalWeight);

				totalModValue /= totalWeight;

				for (int i = 0; i < _tmpBlurVertices.Count; i++)
				{
					//0이면 유지, 1이면 변경
					float itp = Mathf.Clamp01(_tmpBlurVertexWeights[i] * tDelta * intensity * 5.0f);

					_tmpBlurVertices[i]._deltaPos =
						_tmpBlurVertices[i]._deltaPos * (1.0f - itp) +
						totalModValue * itp;
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
			Editor.Select.MeshGroup.RefreshForce();

			return true;
		}
	}

}