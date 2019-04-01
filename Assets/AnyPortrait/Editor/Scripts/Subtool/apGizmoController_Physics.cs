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
		// Gizmo - Physics Modifier에서 Vertex 선택한다. 제어는 없음
		// Area 선택이 가능하다
		// < ModRenderVert 선택시 ModVertWeight를 선택하도록 주의 >
		//----------------------------------------------------------------
		/// <summary>
		/// Modifier [Physics]에 대한 Gizmo Event의 Set이다.
		/// </summary>
		/// <returns></returns>
		public apGizmos.GizmoEventSet GetEventSet_Modifier_Physics()
		{
			//Morph는 Vertex / VertexPos 계열 이벤트를 사용하며, Color 처리를 한다.
			return new apGizmos.GizmoEventSet(
				Select__Modifier_Physics,
				Unselect__Modifier_Physics,
				null, null, null,
				null, null, null, null,
				PivotReturn__Modifier_Physics,
				MultipleSelect__Modifier_Physics,
				null,
				null,
				null,
				null,
				apGizmos.TRANSFORM_UI.None,
				FirstLink__Modifier_Physic);
		}




		public apGizmos.SelectResult FirstLink__Modifier_Physic()
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
		/// Physic Modifier내에서의 Gizmo 이벤트 : Vertex 계열 선택시 [단일 선택]
		/// </summary>
		/// <param name="mousePosGL"></param>
		/// <param name="mousePosW"></param>
		/// <param name="btnIndex"></param>
		/// <param name="selectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult Select__Modifier_Physics(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
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
									//삭제인 경우 : ModVertWeight를 선택한다.
									Editor.Select.RemoveModVertexOfModifier(null, null, Editor.Select.ModRenderVertOfMod._modVertWeight, Editor.Select.ModRenderVertOfMod._renderVert);
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
										Editor.Select.RemoveModVertexOfModifier(null, null, modRenderVert._modVertWeight, modRenderVert._renderVert);
									}
									else if (selectType == apGizmos.SELECT_TYPE.Add)
									{
										//Add 상태에서 원래 선택된걸 누른다면
										//추가인 경우 => 그대로
										selectVertex = true;
									}
									else
									{
										//만약... new 라면?
										//다른건 초기화하고
										//얘만 선택해야함
										apRenderVertex selectedRenderVert = modRenderVert._renderVert;
										apModifiedVertexWeight selectedModVertWeight = modRenderVert._modVertWeight;
										Editor.Select.SetModVertexOfModifier(null, null, null, null);
										Editor.Select.SetModVertexOfModifier(null, null, selectedModVertWeight, selectedRenderVert);
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
										apModifiedVertexWeight selectedModVertWeight = Editor.Select.ExKey_ModMesh._vertWeights.Find(delegate (apModifiedVertexWeight a)
										{
											return renderVert._vertex._uniqueID == a._vertexUniqueID;
										});

										if (selectedModVertWeight != null)
										{
											if (selectType == apGizmos.SELECT_TYPE.New)
											{
												Editor.Select.SetModVertexOfModifier(null, null, selectedModVertWeight, renderVert);
											}
											else if (selectType == apGizmos.SELECT_TYPE.Add)
											{
												Editor.Select.AddModVertexOfModifier(null, null, selectedModVertWeight, renderVert);
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


		public void Unselect__Modifier_Physics()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}

			Editor.Select.SetModVertexOfModifier(null, null, null, null);

			if (!Editor.Select.IsLockExEditKey)
			{
				Editor.Select.SetSubMeshInGroup(null);
			}

			Editor.RefreshControllerAndHierarchy();
			Editor.SetRepaint();
		}


		/// <summary>
		/// Physics Modifier내에서의 Gizmo 이벤트 : Vertex 계열 선택시 [복수 선택]
		/// </summary>
		/// <param name="mousePosGL_Min"></param>
		/// <param name="mousePosGL_Max"></param>
		/// <param name="mousePosW_Min"></param>
		/// <param name="mousePosW_Max"></param>
		/// <param name="areaSelectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult MultipleSelect__Modifier_Physics(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, apGizmos.SELECT_TYPE areaSelectType)
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
							apModifiedVertexWeight selectedModVertWeight = Editor.Select.ExKey_ModMesh._vertWeights.Find(delegate (apModifiedVertexWeight a)
							{
								return renderVert._vertex._uniqueID == a._vertexUniqueID;
							});

							if (selectedModVertWeight != null)
							{
								if (areaSelectType == apGizmos.SELECT_TYPE.Add ||
									areaSelectType == apGizmos.SELECT_TYPE.New)
								{
									Editor.Select.AddModVertexOfModifier(null, null, selectedModVertWeight, renderVert);
								}
								else
								{
									Editor.Select.RemoveModVertexOfModifier(null, null, selectedModVertWeight, renderVert);
								}

								isAnyChanged = true;
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


		public apGizmos.TransformParam PivotReturn__Modifier_Physics()
		{
			//Weight는 Pivot이 없다.
			return null;
		}
	}
}