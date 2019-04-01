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
//using UnityEngine.Profiling;

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


		//---------------------------------------------------------------------------
		//Animation의 현재 선택된 Timeline의 종류에 따라서 다른 이벤트 처리 방식을 가진다.
		// [타임라인이 없을때] - Transform 선택 (수정 불가)
		// [Modifier + Transform 계열] - Transform 선택
		// [Modifier + Vertex 계열] - Vertex / Transform(Lock 가능) 선택
		// [Bone] - Bone 선택
		// [ControlParam] - Transform 선택 (수정 불가)

		public apGizmos.GizmoEventSet GetEventSet__Animation_OnlySelectTransform()
		{
			//선택만 가능하다
			return new apGizmos.GizmoEventSet(Select__Animation,
												Unselect__Animation,
												null, null, null, null, null, null, null,
												PivotReturn__Animation_OnlySelect,
												null, null, null, null, null,
												apGizmos.TRANSFORM_UI.None,
												FirstLink__Animation);
		}

		public apGizmos.GizmoEventSet GetEventSet__Animation_EditTransform()
		{
			return new apGizmos.GizmoEventSet(Select__Animation,
												Unselect__Animation,
												Move__Animation_Transform,
												Rotate__Animation_Transform,
												Scale__Animation_Transform,
												TransformChanged_Position__Animation_Transform,
												TransformChanged_Rotate__Animation_Transform,
												TransformChanged_Scale__Animation_Transform,
												TransformChanged_Color__Animation_Transform,
												PivotReturn__Animation_Transform,
												null, null, null, null, null,
												apGizmos.TRANSFORM_UI.TRS | apGizmos.TRANSFORM_UI.Color,
												FirstLink__Animation
												);
		}

		public apGizmos.GizmoEventSet GetEventSet__Animation_EditVertex()
		{
			return new apGizmos.GizmoEventSet(Select__Animation,
				Unselect__Animation,
				Move__Animation_Vertex,
				Rotate__Animation_Vertex,
				Scale__Animation_Vertex,
				TransformChanged_Position__Animation_Vertex,
				TransformChanged_Rotate__Animation_Vertex,
				TransformChanged_Scale__Animation_Vertex,
				null,
				PivotReturn__Animation_Vertex,
				MultipleSelect__Animation_Vertex,
				FFDTransform__Animation_Vertex,
				StartFFDTransform__Animation_Vertex,
				SoftSelection_Animation_Vertex,
				PressBlur_Animation_Vertex,
				apGizmos.TRANSFORM_UI.TRS | apGizmos.TRANSFORM_UI.Vertex_Transform,
				FirstLink__Animation);
		}



		//---------------------------------------------------------------------------
		//----------------------------------------------------------------------------------------------
		// Select + TRS 제어 이벤트들
		//----------------------------------------------------------------------------------------------

		public apGizmos.SelectResult FirstLink__Animation()
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null || Editor.Select.AnimTimeline == null)
			{
				return null;
			}

			bool isTransformSelectable = false;
			bool isBoneSelectable = false;
			bool isVertexSelectable = false;//<<TODO
			int result = 0;
			object resultObj = null;

			if (Editor.Select.AnimTimeline != null)
			{
				switch (Editor.Select.AnimTimeline._linkType)
				{
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						isTransformSelectable = true;
						if (Editor.Select.AnimTimeline._linkedModifier != null)
						{
							if ((int)(Editor.Select.AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
							{
								//Vertex 데이터가 필요할 때
								isVertexSelectable = true;
							}
							if (Editor.Select.AnimTimeline._linkedModifier.IsTarget_Bone)
							{
								isBoneSelectable = true;//<<Bone 추가. 근데 이거 언제 써먹어요?
							}
						}
						break;

					case apAnimClip.LINK_TYPE.ControlParam:
						break;

					default:
						Debug.LogError("TODO : ???");
						break;
				}
			}
			else
			{
				isTransformSelectable = true;
				isBoneSelectable = true;
			}


			//apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
			apModifiedBone targetModBone = Editor.Select.ModBoneOfAnim;


			if (isVertexSelectable)
			{
				if (Editor.Select.ModRenderVertListOfAnim != null && workKeyframe != null && targetModMesh != null)
				{
					result = Editor.Select.ModRenderVertListOfAnim.Count;
				}
			}
			if (result != 0)
			{
				//return result;
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfAnim);
			}

			if (isTransformSelectable)
			{
				if (Editor.Select.SubMeshTransformOnAnimClip != null)
				{
					result = 1;
					resultObj = Editor.Select.SubMeshTransformOnAnimClip;
				}
				else if (Editor.Select.SubMeshGroupTransformOnAnimClip != null)
				{
					result = 1;
					resultObj = Editor.Select.SubMeshGroupTransformOnAnimClip;
				}
				else if (isBoneSelectable && targetModBone != null)
				{
					//Mod Bone 추가
					result = 1;
					resultObj = targetModBone._bone;
				}
				//if(Editor.Select.SubMeshTransformOnAnimClip != null ||
				//	Editor.Select.SubMeshGroupTransformOnAnimClip != null)
				//{
				//	result = 1;
				//}
			}

			//return result;
			return apGizmos.SelectResult.Main.SetSingle(resultObj);
		}

		// Select : 이건 통합
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집중의 Gizmo 이벤트 : [단일 선택] - 현재 Timeline에 따라 [Transform / Bone / Vertex]를 선택한다.
		/// </summary>
		/// <param name="mousePosGL"></param>
		/// <param name="mousePosW"></param>
		/// <param name="btnIndex"></param>
		/// <param name="selectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult Select__Animation(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return null;
			}



			//MeshTransform, MeshGroupTransform, Bone(TODO)을 선택해야한다.
			//어떤걸 선택할 수 있는지 제한을 걸자
			//음.. 코드를 분리할까
			//0) Timeline을 선택하지 않았다. / 또는 Editing이 아니다. => Transform/Bone을 선택할 수 있다.
			//1) ControlParam => 아무것도 선택할 수 없다.
			//2) AnimMod - Transform => Transform/Bone을 선택할 수 있다.
			//3) AnimMod - Vert => Vertex/Transform을 선택할 수 있다.

			//선택 순서 : (Transform 선택시) Vertex => Bone => Transform
			//0) Bone 선택 -> Transform 선택 -> 아니면 취소
			//1) 선택할 수 없다.
			//2) [Unlock] Bone -> Transform 순으로 선택 / [Lock] 둘다 선택 불가. 해제도 안된다.
			//3) [Unlock]
			//- Transform이 선택되어 있다면) Vertex를 선택한다.
			// -> Vertex 선택이 실패되었다면 -> Transform을 선택한다.
			// [Lock]
			//- Transform이 선택되어있다면 Vertex를 선택한다.


			bool isSelectionLock = Editor.Select.IsAnimSelectionLock;


			

			bool isVertexTarget = false;
			bool isTransformTarget = false;
			bool isBoneTarget = false;

			apAnimTimeline timeline = Editor.Select.AnimTimeline;
			apAnimTimelineLayer timelineLayer = Editor.Select.AnimTimelineLayer;
			bool isAnimEditing = Editor.Select.ExAnimEditingMode != apSelection.EX_EDIT.None;

			if (!isAnimEditing || timeline == null)
			{
				//0) Timeline을 선택하지 않았거나 Anim 작업중이 아니다.
				isTransformTarget = true;
				isBoneTarget = true;
			}
			else
			{
				//에디팅 상태일 때
				if (timeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
				{
					//1) ControlParam 타입
					//... 선택을 하지 않는다.
				}
				else if (timeline._linkedModifier != null)
				{
					if ((int)(Editor.Select.AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
					{
						isVertexTarget = true;
						isTransformTarget = true;
					}
					else if (Editor.Select.AnimTimeline._linkedModifier.IsTarget_Bone)
					{
						isTransformTarget = true;
						isBoneTarget = true;
					}
					else
					{
						isTransformTarget = true;
					}
				}
			}
			

			int prevSelected = 0;
			List<object> prevSelectedObjs = new List<object>();

			apBone prevSelectedBone = Editor.Select.Bone;

			if (Editor.Select.SubMeshGroupTransformOnAnimClip != null)
			{
				prevSelected++;
				prevSelectedObjs.Add(Editor.Select.SubMeshGroupTransformOnAnimClip);
			}
			else if (Editor.Select.SubMeshTransformOnAnimClip != null)
			{
				prevSelected++;
				prevSelectedObjs.Add(Editor.Select.SubMeshTransformOnAnimClip);
			}
			else if (prevSelectedBone != null && isBoneTarget)
			{
				prevSelectedObjs.Add(prevSelectedBone);
			}

			//이걸 왜 작성했는지 모르겠다.. 나중에 문제 생기면 열고 확인하자
			//>> 주석했을 당시의 문제 : 선택 후, 외부 클릭 한 뒤, 다시 선택하면 여기서 False를 해버려서 
			//선택은 해제되고 "아무것도 선택할 수 없는 상태"가 된다.
			//단 Gizmo에서는 선택된 걸로 나타나는 모순
			//if (isTransformTarget)
			//{
			//	if (Editor.Select.IsAnimSelectionLock)
			//	{
			//		//Lock이 걸려있고
			//		//현재 선택된게 있다면
			//		//선택 불가
			//		if (Editor.Select.SubMeshGroupTransformOnAnimClip != null ||
			//			Editor.Select.SubMeshTransformOnAnimClip != null)
			//		{
			//			isTransformTarget = false;
			//		}
			//	}
			//}
			//if (isBoneTarget)
			//{
			//	if (Editor.Select.IsAnimSelectionLock)
			//	{
			//		//Lock걸려있고 현재 선택된게 있다면 선택 불가
			//		if (Editor.Select.Bone != null)
			//		{
			//			isBoneTarget = false;

			//			Debug.LogError("Lock걸려있고 현재 선택된게 있다면 선택 불가");
			//		}
			//	}
			//}


			if (!Editor.Controller.IsMouseInGUI(mousePosGL))
			{
				//return prevSelected;
				return apGizmos.SelectResult.Main.SetMultiple(prevSelectedObjs);
			}


			//bool isAnySelect = false;
			bool isVertexSelected = false;
			bool isTransformSelected = false;
			bool isBoneSelected = false;
			object selectObj = null;

			_prevSelected_TransformBone = null;

			//apModifierBase linkedModifier = null;
			apAnimKeyframe workKeyframe = null;
			apModifiedMesh targetModMesh = null;
			//if (timeline != null)
			//{
			//	linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			//}

			workKeyframe = Editor.Select.AnimWorkKeyframe;
			targetModMesh = Editor.Select.ModMeshOfAnim;

			apTransform_Mesh curSelectedMeshTransform = Editor.Select.SubMeshTransformOnAnimClip;

			//선택 우선순위
			//Vertex -> Bone -> Transform

			if (isVertexTarget)
			{
				//일단 Vertex를 선택한다.
				if (Editor.Select.ModRenderVertListOfAnim != null && workKeyframe != null && targetModMesh != null)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertOfAnim;
					List<apSelection.ModRenderVert> modRenderVertList = Editor.Select.ModRenderVertListOfAnim;

					//1) 현재 선택한 Vertex가 클릭 가능한지 체크
					if (modRenderVert != null)
					{
						if (modRenderVertList.Count == 1)
						{
							//1개인 경우
							if (Editor.Controller.IsVertexClickable(apGL.World2GL(modRenderVert._renderVert._pos_World), mousePosGL))
							{
								if (selectType == apGizmos.SELECT_TYPE.Subtract)
								{
									Editor.Select.RemoveModVertexOfAnim(modRenderVert._modVert, modRenderVert._renderVert);
								}
								else
								{
									//그대로 ㄱㄱ
								}

								isVertexSelected = true;

								//Editor.Select.SetBoneForAnimClip(null);

								//return Editor.Select.ModRenderVertListOfAnim.Count;//<<바로 리턴?
								return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfAnim);
							}
						}
						else
						{
							//여러개인 경우
							//-> 그중 하나를 선택 (+/-/New)
							for (int iModRV = 0; iModRV < modRenderVertList.Count; iModRV++)
							{
								apSelection.ModRenderVert modRV = modRenderVertList[iModRV];

								if (Editor.Controller.IsVertexClickable(apGL.World2GL(modRV._renderVert._pos_World), mousePosGL))
								{
									if (selectType == apGizmos.SELECT_TYPE.Subtract)
									{
										//Editor.Select.SetBoneForAnimClip(null);

										//삭제인 경우
										//하나 지우자
										Editor.Select.RemoveModVertexOfAnim(modRV._modVert, modRV._renderVert);
										//Debug.Log("Remove Vertex");
									}
									else if (selectType == apGizmos.SELECT_TYPE.Add)
									{
										//Editor.Select.SetBoneForAnimClip(null);

										//추가인 경우
										//? 원래 있는걸 추가한다구요?
										//패스
										//Debug.Log("Remove Add - 이미 선택된 것");
									}
									else//if(selectType == apGizmos.SELECT_TYPE.New)
									{
										//New인 경우
										//다른건 초기화.
										//이것만 선택한다.
										//Editor.Select.SetBoneForAnimClip(null);

										apRenderVertex selectedRenderVert = modRV._renderVert;
										apModifiedVertex selectedModVert = modRV._modVert;
										Editor.Select.SetModVertexOfAnim(null, null);
										Editor.Select.SetModVertexOfAnim(selectedModVert, selectedRenderVert);

										//Debug.Log("Select New");
									}

									

									//어쨌거나 선택을 했으면 리턴
									//return Editor.Select.ModRenderVertListOfAnim.Count;//<<바로 리턴?
									return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfAnim);
								}
							}
						}
					}

					//2) 아직 선택했던걸 다시 클릭하진 않았다.
					//새롭게 클릭해서 추가 하는게 있는지 확인하자 (삭제는 예외)
					if (selectType == apGizmos.SELECT_TYPE.New)
					{
						//일단,  New 타입인데 클릭을 했으면 기존에 선택한건 날리자
						Editor.Select.SetModVertexOfAnim(null, null);
					}

					if (selectType != apGizmos.SELECT_TYPE.Subtract)
					{
						//
						apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;
						if (targetRenderUnit != null)
						{
							//해당 RenderUnit의 Vertex를 보고 추가할게 있는지 확인하자
							for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
							{
								apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];
								bool isClick = Editor.Controller.IsVertexClickable(apGL.World2GL(renderVert._pos_World), mousePosGL);
								if (isClick)
								{
									//선택한 RenderVert를 컨트롤하는 ModVert를 찾는다.
									apModifiedVertex selectedModVert = targetModMesh._vertices.Find(delegate (apModifiedVertex a)
									{
										return renderVert._vertex._uniqueID == a._vertexUniqueID;
									});

									if (selectedModVert != null)
									{
										//Editor.Select.SetBoneForAnimClip(null);

										if (selectType == apGizmos.SELECT_TYPE.New)
										{
											//New일때
											Editor.Select.SetModVertexOfAnim(selectedModVert, renderVert);
											//Debug.Log("New Vertex Select >> " + Editor.Select.ModRenderVertListOfAnim.Count);
										}
										else//if(selectType == apGizmos.SELECT_TYPE.Add)
										{
											//Add일때
											Editor.Select.AddModVertexOfAnim(selectedModVert, renderVert);
											//Debug.Log("Add Vertex Select >> " + Editor.Select.ModRenderVertListOfAnim.Count);
										}

										
										

										//return Editor.Select.ModRenderVertListOfAnim.Count;//<<바로 리턴?
										return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfAnim);
									}
								}

							}
						}
					}
				}
			}

			if (isVertexSelected)
			{
				//Vertex를 선택했다면
				Editor.Select.SetBoneForAnimClip(null);
			}
			else
			{
				//Vertex를 선택하지 않았다면
			}

			//그 다음엔 Bone이다.
			//Vertex가 선택되지 않았다면
			if (!isSelectionLock)
			{
				if (isBoneTarget && !isVertexSelected)
				{
					//TODO
					//<<<<< 나중에 여기에 Bone 선택 코드를 만듭시다.
					//선택..
					apMeshGroup mainMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

					List<apBone> boneList = mainMeshGroup._boneList_Root;

					if (selectType == apGizmos.SELECT_TYPE.New ||
						selectType == apGizmos.SELECT_TYPE.Add)
					{
						apBone bone = null;
						apBone resultBone = null;

						for (int i = 0; i < boneList.Count; i++)
						{
							bone = CheckBoneClick(boneList[i], mousePosW, mousePosGL, Editor._boneGUIRenderMode, -1);
							if (bone != null)
							{
								resultBone = bone;
							}
						}
						if (resultBone != null)
						{

							Editor.Select.SetSubMeshTransformForAnimClipEdit(null);
							Editor.Select.SetSubMeshGroupTransformForAnimClipEdit(null);
							Editor.Select.SetBoneForAnimClip(resultBone);
							
							selectObj = resultBone;
							isBoneSelected = true;
						}
					}
				}
				if (!isBoneSelected)
				{
					Editor.Select.SetBone(null);
					Editor.Select.SetBoneForAnimClip(null);
				}
			}

			if (!isSelectionLock)
			{
				if (isTransformTarget && !isVertexSelected && !isBoneSelected)
				{

					apTransform_Mesh selectedMeshTransform = null;
					//GUI에서는 MeshTransform만 선택이 가능하다

					if (selectType == apGizmos.SELECT_TYPE.New ||
						selectType == apGizmos.SELECT_TYPE.Add)
					{
						List<apRenderUnit> renderUnits = Editor.Select.AnimClip._targetMeshGroup._renderUnits_All;//<<정렬된 RenderUnit
						for (int iUnit = 0; iUnit < renderUnits.Count; iUnit++)
						{
							apRenderUnit renderUnit = renderUnits[iUnit];
							if (renderUnit._meshTransform != null && renderUnit._meshTransform._mesh != null)
							{
								if (renderUnit._meshTransform._isVisible_Default && renderUnit._meshColor2X.a > 0.1f)//Alpha 옵션 추가
								{
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
							//걍 선택을 하자
							//if (selectedMeshTransform != prevSelectedMeshTransform)
							//{
							//	Editor.Select.SetSubMeshTransformForAnimClipEdit(selectedMeshTransform);
							//}
							Editor.Select.SetBone(null);
							Editor.Select.SetBoneForAnimClip(null);
							Editor.Select.SetSubMeshGroupTransformForAnimClipEdit(null);
							Editor.Select.SetSubMeshTransformForAnimClipEdit(selectedMeshTransform);//<<수정 : 바뀐 여부 상관없이 그냥 선택한다.
							isTransformSelected = true;
							selectObj = selectedMeshTransform;
						}
						else
						{
							//일단 패스
						}
					}
				}

				//bool isSameMeshTransform = (curSelectedMeshTransform == Editor.Select.SubMeshTransformOnAnimClip);

				if (!isTransformSelected && isTransformTarget)
				{
					Editor.Select.SetSubMeshTransformForAnimClipEdit(null);
				}
			}

			Editor.RefreshControllerAndHierarchy();
			Editor.SetRepaint();

			//일단 이거 빼보자.
			//SelectResult로 "같은 오브젝트를 선택했는지"를 구분할 수 있다.
			//if (isSameMeshTransform)
			//{
			//	return 0;
			//}

			if (isBoneSelected || isTransformSelected)
			{
				//return 1;
				return apGizmos.SelectResult.Main.SetSingle(selectObj);
			}
			else
			{
				if (isVertexTarget)
				{
					//Vertex 선택 상태에서는 "Vertex가 선택된 여부"만 리턴한다.
					if (Editor.Select.ModRenderVertListOfAnim.Count == 0)
					{
						return null;
					}
					else if (Editor.Select.ModRenderVertListOfAnim.Count == 1)
					{
						return apGizmos.SelectResult.Main.SetSingle(Editor.Select.ModRenderVertListOfAnim[0]);
					}
					else
					{
						return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfAnim);
					}
				}
				//이미 선택된 상태라면
				if (Editor.Select.Bone != null && isBoneTarget)
				{
					return apGizmos.SelectResult.Main.SetSingle(Editor.Select.Bone);
				}
				if (Editor.Select.SubMeshGroupTransformOnAnimClip != null)
				{
					return apGizmos.SelectResult.Main.SetSingle(Editor.Select.SubMeshGroupTransformOnAnimClip);
				}
				if (Editor.Select.SubMeshTransformOnAnimClip != null)
				{
					return apGizmos.SelectResult.Main.SetSingle(Editor.Select.SubMeshTransformOnAnimClip);
				}

				//if (Editor.Select.SubMeshGroupTransformOnAnimClip != null ||
				//		Editor.Select.SubMeshTransformOnAnimClip != null)
				//{
				//	return 1;
				//}

				//return 0;
			}
			return null;
		}



		public void Unselect__Animation()
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}

			//Debug.Log("Unselect_Animation");
			if (Editor.Select.AnimTimeline == null)
			{
				//전부 해제
				Editor.Select.UnselectAllObjectsOfAnim();
			}
			else
			{
				//Debug.Log("Unselect_Animation -> Timeline Exist");
				if ((int)(Editor.Select.AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
				{
					//Debug.Log("Vertex Type");
					//Vertex 타입일 때

					//1. Vertex 먼저
					//2. Transform 다음 (락 안걸려있을 때)
					if(Editor.Gizmos.IsFFDMode)
					{
						//Debug.Log("IsFFD Mode");
						//FFD 모드가 켜진 상태에서는 해제가 안된다.
						return;
					}

					Editor.Select.SetModVertexOfAnim(null, null);
					Editor.Select.SetBone(null);

					if (!Editor.Select.IsAnimSelectionLock)
					{
						Editor.Select.SetSubMeshTransformForAnimClipEdit(null);
						Editor.Select.SetSubMeshGroupTransformForAnimClipEdit(null);
					}
				}
				else
				{
					//Debug.Log("Transform / Bone Type");

					//Transform / Bone 타입일 때
					//전부 해제
					Editor.Select.UnselectAllObjectsOfAnim();
				}
			}


			Editor.RefreshControllerAndHierarchy();
			Editor.SetRepaint();
		}

		// Move
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집중의 Gizmo 이벤트 : [이동] - (Autokey가 켜지고 / 임시 값이 열리면) 현재 Timeline에 따라 [Transform / Bone / Vertex]를 선택한다.
		/// </summary>
		/// <param name="curMouseGL"></param>
		/// <param name="curMousePosW"></param>
		/// <param name="deltaMoveW"></param>
		/// <param name="btnIndex"></param>
		public void Move__Animation_Transform(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if (Editor.Select.AnimClip == null ||
				Editor.Select.AnimClip._targetMeshGroup == null ||
				Editor.Select.AnimTimeline == null
				//|| deltaMoveW.sqrMagnitude == 0.0f
				)
			{
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			if(deltaMoveW.sqrMagnitude == 0.0f && !isFirstMove)
			{
				return;
			}

			
			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfAnim;
			apModifiedBone modBone = Editor.Select.ModBoneOfAnim;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			if (linkedModifier == null || workKeyframe == null)
			{
				//수정할 타겟이 없다.
				return;
			}

			bool isTargetTransform = modMesh != null && targetRenderUnit != null && (modMesh._transform_Mesh != null || modMesh._transform_MeshGroup != null);
			bool isTargetBone = linkedModifier.IsTarget_Bone && modBone != null && Editor.Select.Bone != null;

			if (!isTargetTransform && !isTargetBone)
			{
				//둘다 해당사항이 없다.
				return;
			}


			//마우스가 안에 없다.
			if (!Editor.Controller.IsMouseInGUI(curMouseGL))
			{
				return;
			}

			//Undo
			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_MoveTransform, Editor, linkedModifier, targetRenderUnit, false);
			}

			if (isTargetTransform)
			{
				apMatrix resultMatrix = null;
				//apMatrix resultMatrixWithoutMod = null;

				apMatrix matx_ToParent = null;
				//apMatrix matx_LocalModified = null;
				apMatrix matx_ParentWorld = null;

				object selectedTransform = null;

				if (modMesh._isMeshTransform)
				{
					if (modMesh._transform_Mesh != null)
					{
						resultMatrix = modMesh._transform_Mesh._matrix_TFResult_World;
						//resultMatrixWithoutMod = modMesh._transform_Mesh._matrix_TFResult_WorldWithoutMod;
						selectedTransform = modMesh._transform_Mesh;

						matx_ToParent = modMesh._transform_Mesh._matrix_TF_ToParent;
						//matx_LocalModified = modMesh._transform_Mesh._matrix_TF_LocalModified;
						matx_ParentWorld = modMesh._transform_Mesh._matrix_TF_ParentWorld;
					}
				}
				else
				{
					if (modMesh._transform_MeshGroup != null)
					{
						resultMatrix = modMesh._transform_MeshGroup._matrix_TFResult_World;
						//resultMatrixWithoutMod = modMesh._transform_MeshGroup._matrix_TFResult_WorldWithoutMod;
						selectedTransform = modMesh._transform_MeshGroup;

						matx_ToParent = modMesh._transform_MeshGroup._matrix_TF_ToParent;
						//matx_LocalModified = modMesh._transform_MeshGroup._matrix_TF_LocalModified;
						matx_ParentWorld = modMesh._transform_MeshGroup._matrix_TF_ParentWorld;
					}
				}

				if (resultMatrix == null)
				{
					return;
				}

				if (selectedTransform != _prevSelected_TransformBone || isFirstMove)
				{
					_prevSelected_TransformBone = selectedTransform;
					_prevSelected_MousePosW = curMousePosW;
					//_prevSelected_WorldPos = resultMatrix._pos;
				}


				apMatrix nextWorldMatrix = new apMatrix(resultMatrix);
				nextWorldMatrix.SetPos(resultMatrix._pos + deltaMoveW);
				
				//ToParent x LocalModified x ParentWorld = Result
				//LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
				apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(nextWorldMatrix, matx_ParentWorld));

				//TRS중에 Pos만 넣어도 된다.
				modMesh._transformMatrix.SetPos(nextLocalModifiedMatrix._pos);


				#region [미사용 코드 : CalLog를 이용한 이동 코드]
				//// 목표 WorldPos
				////Vector3 worldPos = _prevSelected_WorldPos + new Vector3(deltaMousePosWFromDown.x, deltaMousePosWFromDown.y, 0);
				//apCalculatedLog.InverseResult calResult = modMesh.CalculatedLog.World2ModLocalPos_TransformMove(curMousePosW, deltaMoveW);
				//if (calResult == null || !calResult._isSuccess)
				//{

				//	//Scale 없는 Matrix가 필요하다
				//	apMatrix noScaleWorldMatrix = new apMatrix();
				//	noScaleWorldMatrix.SetPos(resultMatrix._pos);
				//	noScaleWorldMatrix.SetRotate(resultMatrix._angleDeg);
				//	noScaleWorldMatrix.MakeMatrix();

				//	apMatrix noScaleWorldMatrixWithoutMod = new apMatrix();
				//	noScaleWorldMatrixWithoutMod.SetPos(resultMatrixWithoutMod._pos);
				//	noScaleWorldMatrixWithoutMod.SetRotate(resultMatrixWithoutMod._angleDeg);
				//	noScaleWorldMatrixWithoutMod.MakeMatrix();


				//	Vector2 worldPosPrev = resultMatrix._pos;
				//	Vector2 worldPosNext = worldPosPrev + deltaMoveW;

				//	//변경된 위치 W (Pos + Delta) => 변경된 위치 L (Pos + Delta)
				//	Vector2 localPosDelta = noScaleWorldMatrixWithoutMod.InvMulPoint2(worldPosNext);

				//	//Vector2 deltaModLocalPos = new Vector2(localPosDelta.x, localPosDelta.y);

				//	modMesh._transformMatrix.SetPos(localPosDelta);
				//	modMesh._transformMatrix.MakeMatrix();
				//}
				//else
				//{
				//	modMesh._transformMatrix.SetPos(calResult._posL_next);
				//	modMesh._transformMatrix.MakeMatrix();
				//} 
				#endregion

				modMesh.RefreshModifiedValues(Editor._portrait);

				//이건 너무 많이 Refresh한다.
				//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);//<이거 필수. 그래야 다음 프레임 전에 바로 적용이 되서 문제가 없음

				//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
				if (targetMeshGroup != null)
				{
					//targetMeshGroup.AddForceUpdateTarget(modMesh._renderUnit);
					targetMeshGroup.RefreshForce();
				}
			}
			else
			{
				//Bone 움직임을 제어하자.
				//ModBone에 값을 넣는다.
				apBone bone = Editor.Select.Bone;
				//apMeshGroup meshGroup = Editor.Select.AnimClip._targetMeshGroup;

				//Move로 제어 가능한 경우는
				//1. IK Tail일 때
				//2. Root Bone일때 (절대값)

				bool isAnimKeyAutoAdded = false;//편집 도중에 키프레임이 생성된 경우 -> Refresh를 해줘야 한다.


				


				if (bone._isIKTail)
				{
					//Debug.Log("Request IK : " + _boneSelectPosW);
					float weight = 1.0f;
					//bool successIK = bone.RequestIK(_boneSelectPosW, weight, !isFirstSelectBone);

					if (bone != _prevSelected_TransformBone || isFirstMove)
					{
						_prevSelected_TransformBone = bone;
						_prevSelected_MousePosW = bone._worldMatrix._pos;
					}

					_prevSelected_MousePosW += deltaMoveW;
					Vector2 bonePosW = _prevSelected_MousePosW;//DeltaPos + 절대 위치 절충
					//Vector2 bonePosW = bone._worldMatrix._pos + deltaMoveW;//DeltaPos 이용
					//Vector2 bonePosW = curMousePosW;//절대 위치 이용 << IK는 절대 위치를 이용하자..
					apBone limitedHeadBone = bone.RequestIK_Limited(bonePosW, weight, true, Editor.Select.ModRegistableBones);

					if (limitedHeadBone == null)
					{
						return;
					}


					//apBone headBone = bone._IKHeaderBone;//<<
					apBone curBone = bone;
					//위로 올라가면서 IK 결과값을 Default에 적용하자

					//중요
					//만약, IK Chain이 된 상태에서
					//Chain에 해당하는 Bone이 현재 Keyframe이 없는 경우 (Layer는 있으나 현재 프레임에 해당하는 Keyframe이 없음)
					//자동으로 생성해줘야 한다.

					int curFrame = Editor.Select.AnimClip.CurFrame;

					while (true)
					{
						float deltaAngle = curBone._IKRequestAngleResult;

						//apModifiedBone targetModBone = paramSet._boneData.Find(delegate (apModifiedBone a)
						//{
						//	return a._bone == curBone;
						//});
						apAnimTimelineLayer targetLayer = animTimeline._layers.Find(delegate (apAnimTimelineLayer a)
						{
							return a._linkedBone == curBone;
						});
						//현재 Bone과 ModBone이 있는가

						apAnimKeyframe targetKeyframe = targetLayer.GetKeyframeByFrameIndex(curFrame);

						if (targetKeyframe == null)
						{
							//해당 키프레임이 없다면
							//새로 생성한다.
							//단, Refresh는 하지 않는다.

							targetKeyframe = Editor.Controller.AddAnimKeyframe(curFrame, targetLayer, false, false, false, false);
							isAnimKeyAutoAdded = true;//True로 설정하고 나중에 한꺼번에 Refresh하자
						}

						apModifiedBone targetModBone = targetKeyframe._linkedModBone_Editor;

						if (targetModBone != null)
						{
							//float nextAngle = curBone._defaultMatrix._angleDeg + deltaAngle;
							//해당 Bone의 ModBone을 가져온다.

							float nextAngle = curBone._localMatrix._angleDeg + deltaAngle;
							while (nextAngle < -180.0f)
							{
								nextAngle += 360.0f;
							}
							while (nextAngle > 180)
							{
								nextAngle -= 360.0f;
							}

							//DefaultMatrix말고
							//curBone._defaultMatrix.SetRotate(nextAngle);

							//ModBone을 수정하자
							targetModBone._transformMatrix.SetRotate(nextAngle);
						}

						curBone._isIKCalculated = false;
						curBone._IKRequestAngleResult = 0.0f;

						if (curBone == limitedHeadBone)
						{
							break;
						}
						if (curBone._parentBone == null)
						{
							break;
						}
						curBone = curBone._parentBone;
					}

					//마지막으론 World Matrix 갱신
					//근데 이게 딱히 소용 없을 듯..
					//Calculated Refresh를 해야함
					//limitedHeadBone.MakeWorldMatrix(true);
					//limitedHeadBone.GUIUpdate(true);
				}
				else if (bone._parentBone == null
					|| (bone._parentBone._IKNextChainedBone != bone))
				{
					//수정 : Parent가 있지만 IK로 연결 안된 경우 / Parent가 없는 경우 2가지 모두 처리한다.

					apMatrix parentMatrix = null;
					
					if (bone._parentBone == null)
					{
						if (bone._renderUnit != null)
						{
							//Render Unit의 World Matrix를 참조하여
							//로컬 값을 Default로 적용하자
							parentMatrix = bone._renderUnit.WorldMatrixWrap;
						}
					}
					else
					{
						parentMatrix = bone._parentBone._worldMatrix;
					}
					

					//apMatrix localMatrix = bone._localMatrix;
					apMatrix newWorldMatrix = new apMatrix(bone._worldMatrix);
					newWorldMatrix.SetPos(newWorldMatrix._pos + deltaMoveW);
					//newWorldMatrix.SetPos(curMousePosW);

					if (parentMatrix != null)
					{
						newWorldMatrix.RInverse(parentMatrix);
					}
					//WorldMatrix에서 Local Space에 위치한 Matrix는
					//Default + Local + RigTest이다.

					newWorldMatrix.Subtract(bone._defaultMatrix);
					if (bone._isRigTestPosing)
					{
						newWorldMatrix.Subtract(bone._rigTestMatrix);
					}
					
					modBone._transformMatrix.SetPos(newWorldMatrix._pos);
				}


				if (targetMeshGroup != null)
				{
					//if(modBone._renderUnit != null)
					//{
					//	targetMeshGroup.AddForceUpdateTarget(modBone._renderUnit);
					//}

					targetMeshGroup.RefreshForce();
				}

				if (isAnimKeyAutoAdded)
				{
					//Key가 추가되었다면 Refresh를 해야한다.
					Editor._portrait.LinkAndRefreshInEditor(false);
					//Editor.RefreshControllerAndHierarchy();

					//Refresh 추가
					//Editor.Select.RefreshAnimEditing(true);
					Editor.RefreshTimelineLayers(false);
					Editor.Select.AutoSelectAnimTimelineLayer();
					Editor.Select.SetBone(bone);
				}
			}

		}


		/// <summary>
		/// Animation 편집중의 Gizmo 이벤트 : [이동] - (Autokey가 켜지고 / 임시 값이 열리면) 현재 Timeline에 따라 [Transform / Bone / Vertex]를 선택한다.
		/// </summary>
		/// <param name="curMouseGL"></param>
		/// <param name="curMousePosW"></param>
		/// <param name="deltaMoveW"></param>
		/// <param name="btnIndex"></param>
		public void Move__Animation_Vertex(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}
			if (deltaMoveW.sqrMagnitude == 0.0f && !isFirstMove)
			{
				return;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;

			if (linkedModifier == null || workKeyframe == null || targetModMesh == null || targetRenderUnit == null)
			{
				//수정할 타겟이 없다.
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			if (Editor.Select.ModRenderVertOfAnim == null)
			{
				//단 한개의 선택된 Vertex도 없다.
				return;
			}

			if (!Editor.Controller.IsMouseInGUI(curMouseGL))
			{
				return;
			}

			//Undo용 변수 처리
			bool isMultipleVerts = true;
			object targetVertex = null;
			if (Editor.Select.ModRenderVertListOfAnim.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			{
				targetVertex = Editor.Select.ModRenderVertOfAnim._renderVert;
				isMultipleVerts = false;
			}

			//Undo
			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_MoveVertex, Editor, linkedModifier, targetVertex, isMultipleVerts);
			}


			if (Editor.Select.ModRenderVertListOfAnim.Count == 1)
			{
				//1. 단일 선택일 때
				apRenderVertex renderVert = Editor.Select.ModRenderVertOfAnim._renderVert;
				renderVert.Calculate(0.0f);

				Vector2 prevDeltaPos2 = Editor.Select.ModRenderVertOfAnim._modVert._deltaPos;

				apMatrix3x3 martrixMorph = apMatrix3x3.TRS(prevDeltaPos2, 0, Vector2.one);
				Vector2 prevWorldPos2 = (renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform * martrixMorph * renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);
				Vector2 nextWorldPos = prevWorldPos2 + deltaMoveW;
				//Vector3 nextWorldPos3 = new Vector3(nextWorldPos.x, nextWorldPos.y, 0);

				Vector2 noneMorphedPosM = (renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);
				Vector2 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse).MultiplyPoint(nextWorldPos);

				Editor.Select.ModRenderVertOfAnim._modVert._deltaPos.x = (nextMorphedPosM.x - noneMorphedPosM.x);
				Editor.Select.ModRenderVertOfAnim._modVert._deltaPos.y = (nextMorphedPosM.y - noneMorphedPosM.y);
			}
			else
			{
				//2. 복수개 선택일 때
				for (int i = 0; i < Editor.Select.ModRenderVertListOfAnim.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfAnim[i];
					apRenderVertex renderVert = modRenderVert._renderVert;
					Vector2 nextWorldPos = renderVert._pos_World + deltaMoveW;

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVertListOfAnim_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVertListOfAnim_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfAnim_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos = (renderVert._pos_World + deltaMoveW) * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//이전 코드 : 전체 Refresh
			//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

			//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			if (targetMeshGroup != null)
			{
				//targetMeshGroup.AddForceUpdateTarget(targetModMesh._renderUnit);
				targetMeshGroup.RefreshForce();
			}


			Editor.SetRepaint();
		}




		// Rotate
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집중의 Gizmo 이벤트 : [회전] - 현재 Timeline에 따라 [Transform / Bone / Vertex]를 회전한다.
		/// </summary>
		/// <param name="deltaAngleW"></param>
		public void Rotate__Animation_Transform(float deltaAngleW, bool isFirstRotate)
		{
			if (Editor.Select.AnimClip == null ||
				Editor.Select.AnimClip._targetMeshGroup == null ||
				Editor.Select.AnimTimeline == null)
			{
				return;
			}

			if(deltaAngleW == 0.0f && !isFirstRotate)
			{
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}


			
			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfAnim;
			apModifiedBone modBone = Editor.Select.ModBoneOfAnim;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			if (linkedModifier == null || workKeyframe == null)
			{
				//수정할 타겟이 없다.
				return;
			}

			bool isTargetTransform = modMesh != null && targetRenderUnit != null && (modMesh._transform_Mesh != null || modMesh._transform_MeshGroup != null);
			bool isTargetBone = linkedModifier.IsTarget_Bone && modBone != null && Editor.Select.Bone != null;

			if (!isTargetTransform && !isTargetBone)
			{
				//둘다 해당사항이 없다.
				Debug.LogError("Rotate Failed - " + isTargetTransform + " / " + isTargetBone);
				return;
			}


			//Undo
			if(isFirstRotate)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_RotateTransform, Editor, linkedModifier, targetRenderUnit, false);
			}
			

			if (isTargetTransform)
			{
				apMatrix resultMatrix = null;
				
				apMatrix matx_ToParent = null;
				//apMatrix matx_LocalModified = null;
				apMatrix matx_ParentWorld = null;


				if (modMesh._isMeshTransform)
				{
					if (modMesh._transform_Mesh != null)
					{
						resultMatrix = modMesh._transform_Mesh._matrix_TFResult_World;

						matx_ToParent = modMesh._transform_Mesh._matrix_TF_ToParent;
						//matx_LocalModified = modMesh._transform_Mesh._matrix_TF_LocalModified;
						matx_ParentWorld = modMesh._transform_Mesh._matrix_TF_ParentWorld;
					}
				}
				else
				{
					if (modMesh._transform_MeshGroup != null)
					{
						resultMatrix = modMesh._transform_MeshGroup._matrix_TFResult_World;
						
						matx_ToParent = modMesh._transform_MeshGroup._matrix_TF_ToParent;
						//matx_LocalModified = modMesh._transform_MeshGroup._matrix_TF_LocalModified;
						matx_ParentWorld = modMesh._transform_MeshGroup._matrix_TF_ParentWorld;
					}
				}

				if (resultMatrix == null)
				{
					return;
				}


				apMatrix nextWorldMatrix = new apMatrix(resultMatrix);
				nextWorldMatrix.SetRotate(resultMatrix._angleDeg + deltaAngleW);//각도 변경

				//ToParent x LocalModified x ParentWorld = Result
				//LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
				apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(nextWorldMatrix, matx_ParentWorld));

				modMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
														apUtil.AngleTo180(nextLocalModifiedMatrix._angleDeg),
														modMesh._transformMatrix._scale);

				modMesh.RefreshModifiedValues(Editor._portrait);

				//apCalculatedLog.InverseResult calResult = modMesh.CalculatedLog.World2ModLocalPos_TransformRotationScaling(deltaAngleW, Vector2.zero);

				//modMesh._transformMatrix.SetRotate(modMesh._transformMatrix._angleDeg + deltaAngleW);


				////Pos 보정
				//if (calResult != null && calResult._isSuccess)
				//{
				//	modMesh._transformMatrix.SetPos(calResult._posL_next);
				//}


				//modMesh._transformMatrix.MakeMatrix();

				//이전 코드 : 전체 Refresh
				//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

				//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
				if (targetMeshGroup != null)
				{
					//targetMeshGroup.AddForceUpdateTarget(modMesh._renderUnit);
					targetMeshGroup.RefreshForce();
				}
			}
			else if (isTargetBone)
			{
				apBone bone = Editor.Select.Bone;


				//Default Angle은 -180 ~ 180 범위 안에 들어간다.
				//float nextAngle = bone._defaultMatrix._angleDeg + deltaAngleW;
				float nextAngle = modBone._transformMatrix._angleDeg + deltaAngleW;

				while (nextAngle < -180.0f)
				{
					nextAngle += 360.0f;
				}
				while (nextAngle > 180.0f)
				{
					nextAngle -= 360.0f;
				}

				modBone._transformMatrix.SetRotate(nextAngle);

				if (targetMeshGroup != null)
				{
					//if (modBone._renderUnit != null)
					//{
					//	targetMeshGroup.AddForceUpdateTarget(modBone._renderUnit);
					//}
					targetMeshGroup.RefreshForce();
				}
			}
		}






		/// <summary>
		/// Animation 편집중의 Gizmo 이벤트 : [회전] - 현재 Timeline에 따라 [Transform / Bone / Vertex]를 회전한다.
		/// </summary>
		/// <param name="deltaAngleW"></param>
		public void Rotate__Animation_Vertex(float deltaAngleW, bool isFirstRotate)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}

			if (deltaAngleW == 0.0f && !isFirstRotate)
			{
				return;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;

			if (linkedModifier == null ||
				workKeyframe == null ||
				targetModMesh == null ||
				targetRenderUnit == null ||
				Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None ||
				Editor.Select.IsAnimPlaying ||
				Editor.Select.ModRenderVertOfAnim == null ||
				Editor.Select.ModRenderVertListOfAnim == null)
			{
				//수정할 타겟이 없다. + 에디팅 가능한 상태가 아니다 + 선택한 Vertex가 없다.
				return;
			}


			//Vertex 계열에서 회전/크기수정하고자 하는 경우 -> 다중 선택에서만 가능
			if (Editor.Select.ModRenderVertListOfAnim.Count <= 1)
			{
				//단일 선택인 경우는 패스
				return;
			}

			Vector2 centerPos2 = Editor.Select.ModRenderVertsCenterPosOfAnim;

			//Gizmo의 +-180도 이내 제한.... 일단 빼보자
			//if(deltaAngleW > 180.0f) { deltaAngleW -= 360.0f; }
			//else if(deltaAngleW < -180.0f) { deltaAngleW += 360.0f; }

			//Quaternion quat = Quaternion.Euler(0.0f, 0.0f, deltaAngleW);

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos2, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
				* apMatrix3x3.TRS(-centerPos2, 0, Vector2.one);

			//Undo
			bool isMultipleVerts = true;
			object targetVert = null;
			if (Editor.Select.ModRenderVertListOfAnim.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			{
				targetVert = Editor.Select.ModRenderVertListOfAnim[0]._renderVert;
				isMultipleVerts = false;
			}
			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_RotateVertex, Editor, linkedModifier, targetVert, isMultipleVerts);
			}


			for (int i = 0; i < Editor.Select.ModRenderVertListOfAnim.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfAnim[i];
				Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(modRenderVert._renderVert._pos_World);

				modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos2);
			}


			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVertListOfAnim_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVertListOfAnim_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfAnim_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(modRenderVert._renderVert._pos_World);
					Vector2 nextWorldPos = (nextWorldPos2) * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}


			//이전 코드 : 전체 Refresh
			//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

			//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			if (targetMeshGroup != null)
			{
				//targetMeshGroup.AddForceUpdateTarget(targetModMesh._renderUnit);
				targetMeshGroup.RefreshForce();
			}

			Editor.SetRepaint();
		}




		// Scale
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집중의 Gizmo 이벤트 : [크기] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 크기를 변경한다.
		/// </summary>
		/// <param name="deltaScaleW"></param>
		public void Scale__Animation_Transform(Vector2 deltaScaleW, bool isFirstScale)
		{
			if (Editor.Select.AnimClip == null ||
				Editor.Select.AnimClip._targetMeshGroup == null ||
				Editor.Select.AnimTimeline == null)
			{
				return;
			}

			if(deltaScaleW.sqrMagnitude == 0.0f && !isFirstScale)
			{
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfAnim;
			apModifiedBone modBone = Editor.Select.ModBoneOfAnim;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			if (linkedModifier == null || workKeyframe == null)
			{
				//수정할 타겟이 없다.
				return;
			}

			bool isTargetTransform = modMesh != null && targetRenderUnit != null && (modMesh._transform_Mesh != null || modMesh._transform_MeshGroup != null);
			bool isTargetBone = linkedModifier.IsTarget_Bone && modBone != null && Editor.Select.Bone != null;

			if (!isTargetTransform && !isTargetBone)
			{
				//둘다 해당사항이 없다.
				return;
			}

			//Undo
			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_ScaleTransform, Editor, linkedModifier, targetRenderUnit, false);
			}

			if (isTargetTransform)
			{
				apMatrix resultMatrix = null;
				//apMatrix resultMatrixWithoutMod = null;

				apMatrix matx_ToParent = null;
				//apMatrix matx_LocalModified = null;
				apMatrix matx_ParentWorld = null;

				if (modMesh._isMeshTransform)
				{
					if (modMesh._transform_Mesh != null)
					{
						resultMatrix = modMesh._transform_Mesh._matrix_TFResult_World;
						//resultMatrixWithoutMod = modMesh._transform_Mesh._matrix_TFResult_WorldWithoutMod;

						matx_ToParent = modMesh._transform_Mesh._matrix_TF_ToParent;
						//matx_LocalModified = modMesh._transform_Mesh._matrix_TF_LocalModified;
						matx_ParentWorld = modMesh._transform_Mesh._matrix_TF_ParentWorld;
					}
				}
				else
				{
					if (modMesh._transform_MeshGroup != null)
					{
						resultMatrix = modMesh._transform_MeshGroup._matrix_TFResult_World;
						//resultMatrixWithoutMod = modMesh._transform_MeshGroup._matrix_TFResult_WorldWithoutMod;

						matx_ToParent = modMesh._transform_MeshGroup._matrix_TF_ToParent;
						//matx_LocalModified = modMesh._transform_MeshGroup._matrix_TF_LocalModified;
						matx_ParentWorld = modMesh._transform_MeshGroup._matrix_TF_ParentWorld;
					}
				}

				if (resultMatrix == null)
				{
					return;
				}

				apMatrix nextWorldMatrix = new apMatrix(resultMatrix);
				nextWorldMatrix.SetScale(resultMatrix._scale + deltaScaleW);
				
				//ToParent x LocalModified x ParentWorld = Result
				//LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
				apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(nextWorldMatrix, matx_ParentWorld));

				modMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
													nextLocalModifiedMatrix._angleDeg,
													nextLocalModifiedMatrix._scale);
				
				modMesh.RefreshModifiedValues(Editor._portrait);

				//Vector3 prevScale3 = modMesh._transformMatrix._scale;
				//Vector2 prevScale = new Vector2(prevScale3.x, prevScale3.y);

				//apCalculatedLog.InverseResult calResult = modMesh.CalculatedLog.World2ModLocalPos_TransformRotationScaling(0.0f, deltaScaleW);

				//modMesh._transformMatrix.SetScale(prevScale + deltaScaleW);

				////Pos 보정
				//if (calResult != null && calResult._isSuccess)
				//{
				//	modMesh._transformMatrix.SetPos(calResult._posL_next);
				//}


				//modMesh._transformMatrix.MakeMatrix();

				//이전 코드 : 전체 Refresh
				//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

				//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
				if (targetMeshGroup != null)
				{
					//targetMeshGroup.AddForceUpdateTarget(modMesh._renderUnit);
					targetMeshGroup.RefreshForce();
				}
			}
			else if (isTargetBone)
			{
				apBone bone = Editor.Select.Bone;

				Vector3 prevScale = modBone._transformMatrix._scale;
				Vector2 nextScale = new Vector2(prevScale.x + deltaScaleW.x, prevScale.y + deltaScaleW.y);

				modBone._transformMatrix.SetScale(nextScale);

				if (targetMeshGroup != null)
				{
					//if (modBone._renderUnit != null)
					//{
					//	targetMeshGroup.AddForceUpdateTarget(modBone._renderUnit);
					//}
					targetMeshGroup.RefreshForce();
				}
			}
		}

		/// <summary>
		/// Animation 편집중의 Gizmo 이벤트 : [크기] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 크기를 변경한다.
		/// </summary>
		/// <param name="deltaScaleW"></param>
		public void Scale__Animation_Vertex(Vector2 deltaScaleW, bool isFirstScale)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}

			if (deltaScaleW.sqrMagnitude == 0.0f && !isFirstScale)
			{ return; }

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;

			if (linkedModifier == null ||
				workKeyframe == null ||
				targetModMesh == null ||
				targetRenderUnit == null ||
				Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None ||
				Editor.Select.IsAnimPlaying ||
				Editor.Select.ModRenderVertOfAnim == null ||
				Editor.Select.ModRenderVertListOfAnim == null)
			{
				//수정할 타겟이 없다. + 에디팅 가능한 상태가 아니다 + 선택한 Vertex가 없다.
				return;
			}

			//Vertex 계열에서 회전/크기수정하고자 하는 경우 -> 다중 선택에서만 가능
			if (Editor.Select.ModRenderVertListOfAnim.Count <= 1)
			{
				//단일 선택인 경우는 패스
				return;
			}

			Vector2 centerPos2 = Editor.Select.ModRenderVertsCenterPosOfAnim;
			//Vector3 centerPos3 = new Vector3(centerPos2.x, centerPos2.y, 0);

			Vector2 scale = new Vector2(1.0f + deltaScaleW.x, 1.0f + deltaScaleW.y);

			apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos2, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, 0, scale)
				* apMatrix3x3.TRS(-centerPos2, 0, Vector2.one);

			//Undo
			bool isMultipleVerts = true;
			object targetVert = null;
			if (Editor.Select.ModRenderVertListOfAnim.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			{
				targetVert = Editor.Select.ModRenderVertListOfAnim[0]._renderVert;
				isMultipleVerts = false;
			}
			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_ScaleVertex, Editor, linkedModifier, targetVert, isMultipleVerts);
			}



			for (int i = 0; i < Editor.Select.ModRenderVertListOfAnim.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfAnim[i];

				Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(modRenderVert._renderVert._pos_World);

				modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos2);
			}

			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVertListOfAnim_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVertListOfAnim_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfAnim_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(modRenderVert._renderVert._pos_World);
					Vector2 nextWorldPos = nextWorldPos2 * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//이전 코드 : 전체 Refresh
			//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

			//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			if (targetMeshGroup != null)
			{
				//targetMeshGroup.AddForceUpdateTarget(targetModMesh._renderUnit);
				targetMeshGroup.RefreshForce();
			}
			Editor.SetRepaint();
		}



		//----------------------------------------------------------------------------------------------
		// Transform Changed 이벤트들
		//----------------------------------------------------------------------------------------------



		// TransformChanged - Position
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집 중의 값 변경 : [위치값 변경] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 위치를 변경한다.
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="depth"></param>
		public void TransformChanged_Position__Animation_Transform(Vector2 pos, int depth)
		{
			if (Editor.Select.AnimClip == null ||
				Editor.Select.AnimClip._targetMeshGroup == null ||
				Editor.Select.AnimTimeline == null)
			{
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfAnim;
			apModifiedBone modBone = Editor.Select.ModBoneOfAnim;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			if (linkedModifier == null || workKeyframe == null)
			{
				//수정할 타겟이 없다.
				return;
			}

			bool isTargetTransform = modMesh != null && targetRenderUnit != null && (modMesh._transform_Mesh != null || modMesh._transform_MeshGroup != null);
			bool isTargetBone = linkedModifier.IsTarget_Bone && modBone != null && Editor.Select.Bone != null;

			if (!isTargetTransform && !isTargetBone)
			{
				//둘다 해당사항이 없다.
				return;
			}

			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_MoveTransform, Editor, linkedModifier, targetRenderUnit, false);

			if (isTargetTransform)
			{
				//apMatrix localStaticMatrix = null;
				//apMatrix resultMatrix = null;
				//if (modMesh._isMeshTransform)
				//{
				//	if (modMesh._transform_Mesh != null)
				//	{
				//		localStaticMatrix = modMesh._transform_Mesh._matrix;
				//		resultMatrix = modMesh._transform_Mesh._matrix_TFResult_World;
				//	}
				//}
				//else
				//{
				//	if (modMesh._transform_MeshGroup != null)
				//	{
				//		localStaticMatrix = modMesh._transform_MeshGroup._matrix;
				//		resultMatrix = modMesh._transform_MeshGroup._matrix_TFResult_World;
				//	}
				//}

				//if (localStaticMatrix == null || resultMatrix == null)
				//{
				//	return;
				//}

				////새로 ModMesh의 Matrix 값을 만들어주자 //TODO : 해당 Matrix를 사용하는 Modifier 구현 필요

				//Vector3 pivotPos = localStaticMatrix.Pos3;
				//Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
				//Vector3 nextModWorldPos = worldPos - pivotPos;//<<<이게 Modifier의 Delta Pos [World]

				//float resultAngle = localStaticMatrix._angleDeg;//<<원점의 Transform은 Rotation만 보면 된다. (Scale 필요 없음)
				//Vector3 modLocalPos = apMatrix3x3.TRS(Vector3.zero, Quaternion.Euler(0, 0, -resultAngle), Vector3.one).MultiplyPoint3x4(nextModWorldPos);

				//modMesh._transformMatrix.SetPos(modLocalPos.x, modLocalPos.y);

				//수정
				//Local 값이 들어오므로 직접 넣자
				modMesh._transformMatrix.SetPos(pos);
				modMesh._transformMatrix.MakeMatrix();

				//이전 코드 : 전체 Refresh
				//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

				//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
				if (targetMeshGroup != null)
				{
					//targetMeshGroup.AddForceUpdateTarget(modMesh._renderUnit);
					targetMeshGroup.RefreshForce();
				}
			}
			else if (isTargetBone)
			{
				//Bone 움직임을 제어하자.
				//ModBone에 값을 넣는다.
				apBone bone = Editor.Select.Bone;
				//apMeshGroup meshGroup = Editor.Select.AnimClip._targetMeshGroup;

				//Move로 제어 가능한 경우는
				//1. IK Tail일 때
				//2. Root Bone일때 (절대값)
				//수정 : TransformChanged에서는 IK 연산을 하지 않는다.
				//음.. 그냥 넣자. 어떤 일이 필요할 지도 모르니까;;

				bool isAnimKeyAutoAdded = false;//편집 도중에 키프레임이 생성된 경우 -> Refresh를 해줘야 한다.
				int curFrame = Editor.Select.AnimClip.CurFrame;

				apAnimTimelineLayer targetLayer = animTimeline._layers.Find(delegate (apAnimTimelineLayer a)
				{
					return a._linkedBone == bone;
				});
				//현재 Bone과 ModBone이 있는가

				apAnimKeyframe targetKeyframe = targetLayer.GetKeyframeByFrameIndex(curFrame);

				if (targetKeyframe == null)
				{
					//해당 키프레임이 없다면
					//새로 생성한다.
					//단, Refresh는 하지 않는다.

					targetKeyframe = Editor.Controller.AddAnimKeyframe(curFrame, targetLayer, false, false, false, false);
					isAnimKeyAutoAdded = true;//True로 설정하고 나중에 한꺼번에 Refresh하자
				}

				apModifiedBone targetModBone = targetKeyframe._linkedModBone_Editor;
				targetModBone._transformMatrix.SetPos(pos);


				#region [미사용 코드 : pos가 World값으로 입력되고 IK로 작동하게 만드는 경우]


				//if (bone._isIKTail)
				//{
				//	//Debug.Log("Request IK : " + _boneSelectPosW);
				//	float weight = 1.0f;
				//	apBone limitedHeadBone = bone.RequestIK_Limited(pos, weight, true, Editor.Select.ModRegistableBones);

				//	if (limitedHeadBone == null)
				//	{
				//		return;
				//	}


				//	//apBone headBone = bone._IKHeaderBone;//<<
				//	apBone curBone = bone;
				//	//위로 올라가면서 IK 결과값을 Default에 적용하자

				//	//중요
				//	//만약, IK Chain이 된 상태에서
				//	//Chain에 해당하는 Bone이 현재 Keyframe이 없는 경우 (Layer는 있으나 현재 프레임에 해당하는 Keyframe이 없음)
				//	//자동으로 생성해줘야 한다.

				//	int curFrame = Editor.Select.AnimClip.CurFrame;

				//	while (true)
				//	{
				//		float deltaAngle = curBone._IKRequestAngleResult;

				//		//apModifiedBone targetModBone = paramSet._boneData.Find(delegate (apModifiedBone a)
				//		//{
				//		//	return a._bone == curBone;
				//		//});
				//		apAnimTimelineLayer targetLayer = animTimeline._layers.Find(delegate (apAnimTimelineLayer a)
				//		{
				//			return a._linkedBone == curBone;
				//		});
				//		//현재 Bone과 ModBone이 있는가

				//		apAnimKeyframe targetKeyframe = targetLayer.GetKeyframeByFrameIndex(curFrame);

				//		if(targetKeyframe == null)
				//		{
				//			//해당 키프레임이 없다면
				//			//새로 생성한다.
				//			//단, Refresh는 하지 않는다.

				//			targetKeyframe = Editor.Controller.AddAnimKeyframe(curFrame, targetLayer, false, false, false, false);
				//			isAnimKeyAutoAdded = true;//True로 설정하고 나중에 한꺼번에 Refresh하자
				//		}

				//		apModifiedBone targetModBone = targetKeyframe._linkedModBone_Editor;

				//		if (targetModBone != null)
				//		{
				//			//float nextAngle = curBone._defaultMatrix._angleDeg + deltaAngle;
				//			//해당 Bone의 ModBone을 가져온다.

				//			float nextAngle = curBone._localMatrix._angleDeg + deltaAngle;
				//			while (nextAngle < -180.0f)
				//			{
				//				nextAngle += 360.0f;
				//			}
				//			while (nextAngle > 180)
				//			{
				//				nextAngle -= 360.0f;
				//			}

				//			//DefaultMatrix말고
				//			//curBone._defaultMatrix.SetRotate(nextAngle);

				//			//ModBone을 수정하자
				//			targetModBone._transformMatrix.SetRotate(nextAngle);
				//		}

				//		curBone._isIKCalculated = false;
				//		curBone._IKRequestAngleResult = 0.0f;

				//		if (curBone == limitedHeadBone)
				//		{
				//			break;
				//		}
				//		if (curBone._parentBone == null)
				//		{
				//			break;
				//		}
				//		curBone = curBone._parentBone;
				//	}

				//	//마지막으론 World Matrix 갱신
				//	//근데 이게 딱히 소용 없을 듯..
				//	//Calculated Refresh를 해야함
				//	//limitedHeadBone.MakeWorldMatrix(true);
				//	//limitedHeadBone.GUIUpdate(true);
				//}
				//else if (bone._parentBone == null)
				//{
				//	apMatrix renderUnitMatrix = null;
				//	if (bone._renderUnit != null)
				//	{
				//		//Render Unit의 World Matrix를 참조하여
				//		//로컬 값을 Default로 적용하자
				//		renderUnitMatrix = bone._renderUnit.WorldMatrixWrap;
				//	}

				//	apMatrix localMatrix = bone._localMatrix;
				//	apMatrix newWorldMatrix = new apMatrix(bone._worldMatrix);
				//	//newWorldMatrix.SetPos(newWorldMatrix._pos + deltaMoveW);
				//	newWorldMatrix.SetPos(pos);

				//	if (renderUnitMatrix != null)
				//	{
				//		newWorldMatrix.RInverse(renderUnitMatrix);
				//	}
				//	//WorldMatrix에서 Local Space에 위치한 Matrix는
				//	//Default + Local + RigTest이다.

				//	newWorldMatrix.Subtract(bone._defaultMatrix);
				//	if(bone._isRigTestPosing)
				//	{
				//		newWorldMatrix.Subtract(bone._rigTestMatrix);
				//	}
				//	//newWorldMatrix.Subtract(localMatrix);//이건 Add로 연산된거라 Subtract해야한다.

				//	modBone._transformMatrix.SetPos(newWorldMatrix._pos);
				//} 
				#endregion


				if (targetMeshGroup != null)
				{
					//if(modBone._renderUnit != null)
					//{
					//	targetMeshGroup.AddForceUpdateTarget(modBone._renderUnit);
					//}

					targetMeshGroup.RefreshForce();
				}

				if (isAnimKeyAutoAdded)
				{
					//Key가 추가되었다면 Refresh를 해야한다.
					Editor._portrait.LinkAndRefreshInEditor(false);
					//Editor.RefreshControllerAndHierarchy();

					//Refresh 추가
					//Editor.Select.RefreshAnimEditing(true);
					Editor.RefreshTimelineLayers(false);
					Editor.Select.AutoSelectAnimTimelineLayer();
					Editor.Select.SetBone(bone);
				}
			}
			Editor.SetRepaint();
		}




		/// <summary>
		/// Animation 편집 중의 값 변경 : [위치값 변경] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 위치를 변경한다.
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="depth"></param>
		public void TransformChanged_Position__Animation_Vertex(Vector2 pos, int depth)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;

			if (linkedModifier == null ||
				workKeyframe == null ||
				targetModMesh == null ||
				targetRenderUnit == null ||
				Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None ||
				Editor.Select.IsAnimPlaying ||
				Editor.Select.ModRenderVertOfAnim == null ||
				Editor.Select.ModRenderVertListOfAnim == null)
			{
				//수정할 타겟이 없다. + 에디팅 가능한 상태가 아니다 + 선택한 Vertex가 없다.
				return;
			}

			//Vector2 deltaPosW2 = Vector2.zero;
			Vector2 deltaPosChanged = Vector2.zero;

			//Undo
			bool isMultipleVerts = true;
			object targetVert = null;
			if (Editor.Select.ModRenderVertListOfAnim.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			{
				isMultipleVerts = false;
				targetVert = Editor.Select.ModRenderVertListOfAnim[0]._renderVert;
			}
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_MoveVertex, Editor, linkedModifier, targetVert, isMultipleVerts);


			if (Editor.Select.ModRenderVertListOfAnim.Count == 1)
			{
				deltaPosChanged = pos - Editor.Select.ModRenderVertOfAnim._modVert._deltaPos;
				Editor.Select.ModRenderVertOfAnim._modVert._deltaPos = pos;//<<바로 LocalPos로 넣자


				#region [미사용 코드 : WorldPos를 입력값으로 받은 경우]
				//apRenderVertex renderVert = Editor.Select.ModRenderVertOfAnim._renderVert;
				//renderVert.Calculate(0.0f);

				//Vector2 prevDeltaPos2 = Editor.Select.ModRenderVertOfAnim._modVert._deltaPos;
				//Vector3 prevDeltaPos3 = new Vector3(prevDeltaPos2.x, prevDeltaPos2.y, 0);
				//apMatrix3x3 martrixMorph = apMatrix3x3.TRS(prevDeltaPos3, Quaternion.identity, Vector3.one);



				//Vector2 prevPosW2 = Editor.Select.ModRenderVertOfAnim._renderVert._pos_World;
				//Vector3 prevPosW3 = new Vector3(prevPosW2.x, prevPosW2.y, 0);
				//Vector3 nextPosW3 = new Vector3(pos.x, pos.y, 0);

				//deltaPosW2 = pos - prevPosW2;

				//Vector3 noneMorphedPosM = (renderVert._matrix_Static_Vert2Mesh).MultiplyPoint3x4(renderVert._vertex._pos);
				//Vector3 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse).MultiplyPoint3x4(nextPosW3);

				//Editor.Select.ModRenderVertOfAnim._modVert._deltaPos.x = (nextMorphedPosM.x - noneMorphedPosM.x);
				//Editor.Select.ModRenderVertOfAnim._modVert._deltaPos.y = (nextMorphedPosM.y - noneMorphedPosM.y); 
				#endregion
			}
			else
			{
				Vector2 avgDeltaPosPrev = Vector2.zero;
				for (int i = 0; i < Editor.Select.ModRenderVertListOfAnim.Count; i++)
				{
					avgDeltaPosPrev += Editor.Select.ModRenderVertListOfAnim[i]._modVert._deltaPos;
				}
				avgDeltaPosPrev /= Editor.Select.ModRenderVertListOfAnim.Count;

				//Prev로부터의 변화값을 모두 대입하자
				Vector2 deltaPos2Next = pos - avgDeltaPosPrev;
				deltaPosChanged = deltaPos2Next;

				for (int i = 0; i < Editor.Select.ModRenderVertListOfAnim.Count; i++)
				{
					Editor.Select.ModRenderVertListOfAnim[i]._modVert._deltaPos += deltaPos2Next;
				}

				#region [미사용 코드 : WorldPos로 입력 받아서 Center를 이동시키는 경우]
				//Vector2 prevCenterPosW = Editor.Select.ModRenderVertsCenterPosOfAnim;

				//deltaPosW2 = pos - prevCenterPosW;
				//Vector3 deltaPosW3 = new Vector3(deltaPosW2.x, deltaPosW2.y, 0);

				//for (int i = 0; i < Editor.Select.ModRenderVertListOfAnim.Count; i++)
				//{
				//	apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfAnim[i];
				//	apRenderVertex renderVert = modRenderVert._renderVert;

				//	Vector2 prevPosW = renderVert._pos_World;
				//	Vector3 prevPosW3 = new Vector3(prevPosW.x, prevPosW.y, 0);
				//	Vector3 nextPosW3 = prevPosW3 + deltaPosW3;

				//	Vector3 noneMorphedPosM = (renderVert._matrix_Static_Vert2Mesh).MultiplyPoint3x4(renderVert._vertex._pos);
				//	Vector3 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse).MultiplyPoint3x4(nextPosW3);

				//	modRenderVert._modVert._deltaPos.x = (nextMorphedPosM.x - noneMorphedPosM.x);
				//	modRenderVert._modVert._deltaPos.y = (nextMorphedPosM.y - noneMorphedPosM.y);
				//} 
				#endregion
			}

			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVertListOfAnim_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVertListOfAnim_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfAnim_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					//apRenderVertex renderVert = modRenderVert._renderVert;

					////Weight를 적용한 만큼만 움직이자
					//Vector2 nextWorldPos = (renderVert._pos_World + deltaPosW2) * weight + (renderVert._pos_World) * (1.0f - weight);

					//modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
					modRenderVert._modVert._deltaPos = ((modRenderVert._modVert._deltaPos + deltaPosChanged) * weight) + (modRenderVert._modVert._deltaPos * (1.0f - weight));
				}
			}

			//이전 코드 : 전체 Refresh
			//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

			//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			if (targetMeshGroup != null)
			{
				//targetMeshGroup.AddForceUpdateTarget(targetModMesh._renderUnit);
				targetMeshGroup.RefreshForce();
			}
		}


		// TransformChanged - Rotate
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집 중의 값 변경 : [회전값 변경] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 각도를 변경한다.
		/// </summary>
		/// <param name="angle"></param>
		public void TransformChanged_Rotate__Animation_Transform(float angle)
		{
			if (Editor.Select.AnimClip == null ||
				Editor.Select.AnimClip._targetMeshGroup == null ||
				Editor.Select.AnimTimeline == null)
			{
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfAnim;
			apModifiedBone modBone = Editor.Select.ModBoneOfAnim;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			if (linkedModifier == null || workKeyframe == null)
			{
				//수정할 타겟이 없다.
				return;
			}

			bool isTargetTransform = modMesh != null && targetRenderUnit != null && (modMesh._transform_Mesh != null || modMesh._transform_MeshGroup != null);
			bool isTargetBone = linkedModifier.IsTarget_Bone && modBone != null && Editor.Select.Bone != null;

			if (!isTargetTransform && !isTargetBone)
			{
				//둘다 해당사항이 없다.
				return;
			}

			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_RotateTransform, Editor, linkedModifier, targetRenderUnit, false);


			if (isTargetTransform)
			{
				modMesh._transformMatrix.SetRotate(angle);
				modMesh._transformMatrix.MakeMatrix();

				//이전 코드 : 전체 Refresh
				//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

				//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
				if (targetMeshGroup != null)
				{
					//targetMeshGroup.AddForceUpdateTarget(modMesh._renderUnit);
					targetMeshGroup.RefreshForce();
				}
			}
			else if (isTargetBone)
			{
				apBone bone = Editor.Select.Bone;

				//만약 범위 제한이 있으면 그 안으로 제한해야한다.
				//변경 : 절대값이 아니라 DefaultMatrix의 AngleDeg + 상대값으로 바꾼다.
				if (bone._isIKAngleRange)
				{
					if (angle < bone._defaultMatrix._angleDeg + bone._IKAngleRange_Lower)
					{
						angle = bone._defaultMatrix._angleDeg + bone._IKAngleRange_Lower;
					}
					else if (angle > bone._defaultMatrix._angleDeg + bone._IKAngleRange_Upper)
					{
						angle = bone._defaultMatrix._angleDeg + bone._IKAngleRange_Upper;
					}
				}

				modBone._transformMatrix.SetRotate(angle);

				////Default Angle은 -180 ~ 180 범위 안에 들어간다.
				////float nextAngle = bone._defaultMatrix._angleDeg + deltaAngleW;
				//apMatrix dummyWorldMatrix = new apMatrix(bone._worldMatrix);
				//dummyWorldMatrix.SetRotate(angle);

				////Parent - (Local) - (RigTest) 순으로 matrix 역 연산 후 남는 Scale 값으로 대입
				//apMatrix parentMatrix = null;
				//if(bone._parentBone != null)
				//{
				//	parentMatrix = bone._parentBone._worldMatrix;
				//}
				//else if(bone._renderUnit != null)
				//{
				//	parentMatrix = bone._renderUnit.WorldMatrixWrap;
				//}

				//if (parentMatrix != null)
				//{
				//	dummyWorldMatrix.RInverse(parentMatrix);
				//}
				////dummyWorldMatrix.Subtract(bone._localMatrix);
				//dummyWorldMatrix.Subtract(bone._defaultMatrix);
				//if(bone._isRigTestPosing)
				//{
				//	dummyWorldMatrix.Subtract(bone._rigTestMatrix);
				//}
				//dummyWorldMatrix.MakeMatrix();

				//modBone._transformMatrix.SetRotate(dummyWorldMatrix._angleDeg);

				if (modBone._renderUnit != null)
				{
					//targetMeshGroup.AddForceUpdateTarget(modBone._renderUnit);
					targetMeshGroup.RefreshForce();
				}
			}
		}





		/// <summary>
		/// Animation 편집 중의 값 변경 : [회전값 변경] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 각도를 변경한다.
		/// </summary>
		/// <param name="angle"></param>
		public void TransformChanged_Rotate__Animation_Vertex(float angle)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}

			//회전값은... Vertex에선 수정 못함
		}



		// TransformChanged - Scale
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집 중의 값 변경 : [크기값 변경] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 크기를 변경한다.
		/// </summary>
		/// <param name="scale"></param>
		public void TransformChanged_Scale__Animation_Transform(Vector2 scale)
		{
			if (Editor.Select.AnimClip == null ||
				Editor.Select.AnimClip._targetMeshGroup == null ||
				Editor.Select.AnimTimeline == null)
			{
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfAnim;
			apModifiedBone modBone = Editor.Select.ModBoneOfAnim;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			if (linkedModifier == null || workKeyframe == null)
			{
				//수정할 타겟이 없다.
				return;
			}

			bool isTargetTransform = modMesh != null && targetRenderUnit != null && (modMesh._transform_Mesh != null || modMesh._transform_MeshGroup != null);
			bool isTargetBone = linkedModifier.IsTarget_Bone && modBone != null && Editor.Select.Bone != null;

			if (!isTargetTransform && !isTargetBone)
			{
				//둘다 해당사항이 없다.
				return;
			}


			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_ScaleTransform, Editor, linkedModifier, targetRenderUnit, false);

			if (isTargetTransform)
			{
				modMesh._transformMatrix.SetScale(scale);
				modMesh._transformMatrix.MakeMatrix();

				//이전 코드 : 전체 Refresh
				//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

				//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
				if (targetMeshGroup != null)
				{
					//targetMeshGroup.AddForceUpdateTarget(modMesh._renderUnit);
					targetMeshGroup.RefreshForce();
				}
			}
			else if (isTargetBone)
			{
				apBone bone = Editor.Select.Bone;


				modBone._transformMatrix.SetScale(scale);//<<걍 직접 넣자

				//apMatrix dummyWorldMatrix = new apMatrix(bone._worldMatrix);
				//dummyWorldMatrix.SetScale(scale);

				////Parent - (Local) - (RigTest) 순으로 matrix 역 연산 후 남는 Scale 값으로 대입
				//apMatrix parentMatrix = null;
				//if(bone._parentBone != null)
				//{
				//	parentMatrix = bone._parentBone._worldMatrix;
				//}
				//else if(bone._renderUnit != null)
				//{
				//	parentMatrix = bone._renderUnit.WorldMatrixWrap;
				//}

				//if (parentMatrix != null)
				//{
				//	dummyWorldMatrix.RInverse(parentMatrix);
				//}
				////dummyWorldMatrix.Subtract(bone._localMatrix);
				//dummyWorldMatrix.Subtract(bone._defaultMatrix);
				//if(bone._isRigTestPosing)
				//{
				//	dummyWorldMatrix.Subtract(bone._rigTestMatrix);
				//}
				//dummyWorldMatrix.MakeMatrix();

				//modBone._transformMatrix.SetScale(dummyWorldMatrix.Scale2);

				if (modBone._renderUnit != null)
				{
					//targetMeshGroup.AddForceUpdateTarget(modBone._renderUnit);
					targetMeshGroup.RefreshForce();
				}
			}
		}




		/// <summary>
		/// Animation 편집 중의 값 변경 : [크기값 변경] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 크기를 변경한다.
		/// </summary>
		/// <param name="scale"></param>
		public void TransformChanged_Scale__Animation_Vertex(Vector2 scale)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}

			//크기값은... Vertex에선 수정 못함
		}



		// Transform Changed - Color
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집 중의 값 변경 : [색상 변경] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 색상을 변경한다.
		/// </summary>
		/// <param name="color"></param>
		public void TransformChanged_Color__Animation_Transform(Color color, bool isVisible)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;

			if (linkedModifier == null || workKeyframe == null || targetModMesh == null || targetRenderUnit == null)
			{
				//수정할 타겟이 없다.
				return;
			}

			if (targetModMesh._transform_Mesh == null && targetModMesh._transform_MeshGroup == null)
			{
				//대상이 되는 Mesh/MeshGroup이 없다?
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_Color, Editor, linkedModifier, targetRenderUnit, false);

			targetModMesh._meshColor = color;
			targetModMesh._isVisible = isVisible;
		}




		//----------------------------------------------------------------------------------------------
		// 다중 선택 / FFD 제어 이벤트들 (Vertex / Bone만 가능하다)
		//----------------------------------------------------------------------------------------------
		public apGizmos.SelectResult MultipleSelect__Animation_Vertex(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, apGizmos.SELECT_TYPE areaSelectType)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return null;
			}

			if (Editor.Select.AnimTimeline == null ||
				Editor.Select.AnimTimeline._linkType != apAnimClip.LINK_TYPE.AnimatedModifier ||
				Editor.Select.AnimTimeline._linkedModifier == null ||
				Editor.Select.ModRenderVertListOfAnim == null
				)
			{
				//Debug.LogError("실패 1");
				//return 0;
				return null;
			}

			if ((int)(Editor.Select.AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) == 0)
			{
				//VertexPos 계열 Modifier가 아니다.
				//Debug.LogError("실패 2");
				//return 0;
				return null;
			}

			//apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;

			if (workKeyframe == null || targetModMesh == null || targetRenderUnit == null)
			{
				//Debug.LogError("실패 3");
				//return 0;
				return null;
			}


			bool isAnyChanged = false;
			apRenderVertex renderVert = null;
			for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
			{
				renderVert = targetRenderUnit._renderVerts[iVert];

				bool isSelectable = (mousePosW_Min.x < renderVert._pos_World.x && renderVert._pos_World.x < mousePosW_Max.x)
									&& (mousePosW_Min.y < renderVert._pos_World.y && renderVert._pos_World.y < mousePosW_Max.y);
				if (isSelectable)
				{
					apModifiedVertex selectedModVert = targetModMesh._vertices.Find(delegate (apModifiedVertex a)
					{
						return renderVert._vertex._uniqueID == a._vertexUniqueID;
					});

					if (selectedModVert != null)
					{
						if (areaSelectType == apGizmos.SELECT_TYPE.Add ||
							areaSelectType == apGizmos.SELECT_TYPE.New)
						{
							//추가한다.
							Editor.Select.AddModVertexOfAnim(selectedModVert, renderVert);
						}
						else
						{
							//제외한다.
							Editor.Select.RemoveModVertexOfAnim(selectedModVert, renderVert);
						}

						isAnyChanged = true;//<<뭔가 선택에 변동이 생겼다.
					}
				}
			}


			if (isAnyChanged)
			{
				//Debug.LogError("성공");
				Editor.RefreshControllerAndHierarchy();
				Editor.SetRepaint();
			}


			//return Editor.Select.ModRenderVertListOfAnim.Count;
			return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfAnim);
		}



		public apGizmos.SelectResult MultipleSelect__Animation_Bone(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, apGizmos.SELECT_TYPE areaSelectType)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				//return 0;
				return null;
			}

			//TODO:
			//return 0;
			return null;
		}



		// FFD
		//-----------------------------------------------------------------------------

		public bool FFDTransform__Animation_Vertex(List<object> srcObjects, List<Vector2> posWorlds, bool isResultAssign)
		{
			//결과 적용이 아닌 일반 수정 작업시
			//-> 수정이 불가능한 경우에는 불가하다고 리턴한다.
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return false;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;

			if (!isResultAssign)
			{
				
				apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
				apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
				apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;

				if (linkedModifier == null ||
					workKeyframe == null ||
					targetModMesh == null ||
					targetRenderUnit == null ||
					Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None ||
					Editor.Select.IsAnimPlaying ||
					Editor.Select.ModRenderVertOfAnim == null ||
					Editor.Select.ModRenderVertListOfAnim == null)
				{
					//수정할 타겟이 없다. + 에디팅 가능한 상태가 아니다 + 선택한 Vertex가 없다.
					return false;
				}

				//Vertex 계열에서 FFD 수정시에는
				if (Editor.Select.ModRenderVertListOfAnim.Count <= 1)
				{
					//단일 선택인 경우는 패스
					return false;
				}
			}

			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_FFDVertex, Editor, linkedModifier, null, true);

			for (int i = 0; i < srcObjects.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = srcObjects[i] as apSelection.ModRenderVert;
				Vector2 worldPos = posWorlds[i];

				if (modRenderVert == null)
				{
					continue;
				}
				modRenderVert.SetWorldPosToModifier_VertLocal(worldPos);
			}

			//이전 코드 : 전체 Refresh
			//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

			//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시

			if (targetMeshGroup != null)
			{
				//targetMeshGroup.AddForceUpdateTarget(Editor.Select.RenderUnitOfAnim);
				targetMeshGroup.RefreshForce();
			}

			return true;
		}




		public bool StartFFDTransform__Animation_Vertex()
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return false;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;

			if (linkedModifier == null ||
				workKeyframe == null ||
				targetModMesh == null ||
				targetRenderUnit == null ||
				Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None ||
				Editor.Select.IsAnimPlaying ||
				Editor.Select.ModRenderVertOfAnim == null ||
				Editor.Select.ModRenderVertListOfAnim == null)
			{
				//수정할 타겟이 없다. + 에디팅 가능한 상태가 아니다 + 선택한 Vertex가 없다.
				return false;
			}

			//Vertex 계열에서 FFD 수정시에는
			if (Editor.Select.ModRenderVertListOfAnim.Count <= 1)
			{
				//단일 선택인 경우는 패스
				return false;
			}

			List<object> srcObjectList = new List<object>();
			List<Vector2> worldPosList = new List<Vector2>();

			for (int i = 0; i < Editor.Select.ModRenderVertListOfAnim.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfAnim[i];
				srcObjectList.Add(modRenderVert);
				worldPosList.Add(modRenderVert._renderVert._pos_World);
			}
			Editor.Gizmos.RegistTransformedObjectList(srcObjectList, worldPosList);

			return true;
		}



		public bool SoftSelection_Animation_Vertex()
		{
			Editor.Select.ModRenderVertListOfAnim_Weighted.Clear();

			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return false;
			}

			if (Editor.Select.ModRenderVertListOfAnim == null || Editor.Select.ModRenderVertListOfAnim.Count == 0)
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

			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;
			apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
			if (targetRenderUnit != null && targetModMesh != null)
			{
				for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
				{
					apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];

					//선택된 RenderVert는 제외한다.
					if (Editor.Select.ModRenderVertListOfAnim.Exists(delegate (apSelection.ModRenderVert a)
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

					for (int iSelectedRV = 0; iSelectedRV < Editor.Select.ModRenderVertListOfAnim.Count; iSelectedRV++)
					{
						apSelection.ModRenderVert selectedModRV = Editor.Select.ModRenderVertListOfAnim[iSelectedRV];
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
						apModifiedVertex modVert = targetModMesh._vertices.Find(delegate (apModifiedVertex a)
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

							Editor.Select.ModRenderVertListOfAnim_Weighted.Add(newModRenderVert);
						}
					}
				}
			}


			return true;
		}

		public bool PressBlur_Animation_Vertex(Vector2 pos, float tDelta, bool isFirstBlur)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null || Editor.Select.AnimTimeline == null)
			{
				return false;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;

			if (Editor.Select.ModRenderVertListOfAnim == null || Editor.Select.ModRenderVertListOfAnim.Count <= 1 || linkedModifier == null)
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

			if (isFirstBlur)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_BlurVertex, Editor, linkedModifier, null, false);
			}

			//1. 영역 안의 Vertex를 선택하자 + 마우스 중심점을 기준으로 Weight를 구하자 + ModValue의 가중치가 포함된 평균을 구하자
			//2. 가중치가 포함된 평균값만큼 tDelta * intensity * weight로 바꾸어주자

			//영역 체크는 GL값
			float dist = 0.0f;
			float weight = 0.0f;

			//선택된 Vert에 한해서만 처리하자
			for (int i = 0; i < Editor.Select.ModRenderVertListOfAnim.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfAnim[i];
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
					float itp = Mathf.Clamp01(_tmpBlurVertexWeights[i] * tDelta * intensity);

					_tmpBlurVertices[i]._deltaPos =
						_tmpBlurVertices[i]._deltaPos * (1.0f - itp) +
						totalModValue * itp;
				}
			}

			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			if (targetMeshGroup != null)
			{
				//targetMeshGroup.AddForceUpdateTarget(Editor.Select.RenderUnitOfAnim);
				targetMeshGroup.RefreshForce();
			}

			return true;
		}


		// Pivot Return 이벤트들
		//----------------------------------------------------------------------------------------------
		public apGizmos.TransformParam PivotReturn__Animation_OnlySelect()
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return null;
			}

			//? 없나

			return null;
		}


		public apGizmos.TransformParam PivotReturn__Animation_Transform()
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return null;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return null;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
			apModifiedBone targetModBone = Editor.Select.ModBoneOfAnim;

			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;

			if (linkedModifier == null || workKeyframe == null)
			{
				//수정할 타겟이 없다.
				
				return null;
			}
			bool isTransformPivot = false;
			bool isBonePivot = false;
			
			
			if (linkedModifier.IsTarget_Bone && Editor.Select.Bone != null && targetModBone != null)
			{
				isBonePivot = true;
			}

			if (targetModMesh != null && (targetModMesh._transform_Mesh != null || targetModMesh._transform_MeshGroup != null))
			{
				isTransformPivot = true;
			}

			//둘다 없으면 Null
			if (!isTransformPivot && !isBonePivot)
			{	
				return null;
			}

			if (isTransformPivot)
			{
				if (Editor.Select.SubMeshTransformOnAnimClip == null && Editor.Select.SubMeshGroupTransformOnAnimClip == null)
				{
					return null;
				}
				if (targetRenderUnit == null)
				{
					return null;
				}

				//apMatrix transformPivotMatrix = null;//기본 Pivot
				//apMatrix modifiedMatrix = targetModMesh._transformMatrix;
				apMatrix resultMatrix = null;
				int transformDepth = targetModMesh._renderUnit._depth;
				if (targetModMesh._renderUnit._meshTransform != null)
				{
					//transformPivotMatrix = targetModMesh._renderUnit._meshTransform._matrix;
					resultMatrix = targetModMesh._renderUnit._meshTransform._matrix_TFResult_World;
				}
				else if (targetModMesh._renderUnit._meshGroupTransform != null)
				{
					//transformPivotMatrix = targetModMesh._renderUnit._meshGroupTransform._matrix;
					resultMatrix = targetModMesh._renderUnit._meshGroupTransform._matrix_TFResult_World;
				}
				else
				{
					return null;
				}

				//Vector3 worldPos3 = resultMatrix.Pos3;
				Vector2 worldPos = resultMatrix._pos;

				float worldAngle = resultMatrix._angleDeg;
				Vector2 worldScale = resultMatrix._scale;

				apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.TRS;
				if (linkedModifier._isColorPropertyEnabled)
				{
					paramType |= apGizmos.TRANSFORM_UI.Color;//<Color를 지원하는 경우에만
				}

				return apGizmos.TransformParam.Make(
					worldPos,
					worldAngle,
					//modifiedMatrix._angleDeg,
					worldScale,
					//transformPivotMatrix._scale,
					transformDepth,
					targetModMesh._meshColor,
					targetModMesh._isVisible,
					//worldMatrix, 
					//modifiedMatrix.MtrxToSpace,
					resultMatrix.MtrxToSpace,
					false, paramType,
					targetModMesh._transformMatrix._pos,
					targetModMesh._transformMatrix._angleDeg,
					targetModMesh._transformMatrix._scale);
			}
			else if (isBonePivot)
			{
				if(Editor._boneGUIRenderMode == apEditor.BONE_RENDER_MODE.None)
				{
					//Bone GUI모드가 꺼져있으면 안보인다.
					return null;
				}

				apBone bone = Editor.Select.Bone;

				return apGizmos.TransformParam.Make(
					bone._worldMatrix._pos,
					bone._worldMatrix._angleDeg,
					bone._worldMatrix._scale,
					0, bone._color,
					true,
					bone._worldMatrix.MtrxToSpace,
					false, apGizmos.TRANSFORM_UI.TRS,
					targetModBone._transformMatrix._pos,
					targetModBone._transformMatrix._angleDeg,
					targetModBone._transformMatrix._scale);
			}
			return null;
		}


		public apGizmos.TransformParam PivotReturn__Animation_Vertex()
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return null;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
			apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;

			if (linkedModifier == null || workKeyframe == null || targetModMesh == null || targetRenderUnit == null)
			{
				//수정할 타겟이 없다.
				return null;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return null;
			}

			if (Editor.Select.ModRenderVertOfAnim == null)
			{
				//단 한개의 선택된 Vertex도 없다.
				return null;
			}

			apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.Position;
			if (linkedModifier._isColorPropertyEnabled)
			{
				paramType |= apGizmos.TRANSFORM_UI.Color;//<Color를 지원하는 경우에만
			}

			//1. Vertex를 먼저 출력한다. (수정 가능)
			if (Editor.Select.ModRenderVertListOfAnim.Count > 1)
			{
				paramType |= apGizmos.TRANSFORM_UI.Vertex_Transform;

				Vector2 avgDeltaPos = Vector2.zero;
				for (int i = 0; i < Editor.Select.ModRenderVertListOfAnim.Count; i++)
				{
					avgDeltaPos += Editor.Select.ModRenderVertListOfAnim[i]._modVert._deltaPos;
				}
				avgDeltaPos /= Editor.Select.ModRenderVertListOfAnim.Count;

				//다중 선택 중
				return apGizmos.TransformParam.Make(Editor.Select.ModRenderVertsCenterPosOfAnim,
														0.0f,
														Vector2.one,
														targetRenderUnit.GetDepth(),
														//targetRenderUnit.GetColor(),
														targetModMesh._meshColor,
														targetModMesh._isVisible,
														apMatrix3x3.TRS(Vector2.zero, 0, Vector2.one),
														true,
														paramType,
														avgDeltaPos,
														0.0f,
														Vector2.one
														//apGizmos.TRANSFORM_UI.Position |
														//apGizmos.TRANSFORM_UI.FFD_Transform |
														//apGizmos.TRANSFORM_UI.Color
														);
			}
			else
			{
				//한개만 선택했다.
				return apGizmos.TransformParam.Make(Editor.Select.ModRenderVertOfAnim._renderVert._pos_World,
														0.0f,
														Vector2.one,
														targetRenderUnit.GetDepth(),
														//targetRenderUnit.GetColor(),
														targetModMesh._meshColor,
														targetModMesh._isVisible,
														Editor.Select.ModRenderVertOfAnim._renderVert._matrix_ToWorld,
														false,
														paramType,
														Editor.Select.ModRenderVertOfAnim._modVert._deltaPos,
														0.0f,
														Vector2.one
														//apGizmos.TRANSFORM_UI.Position |
														//apGizmos.TRANSFORM_UI.Color
														);

			}


			//return null;
		}

		public apGizmos.TransformParam PivotReturn__Animation_Bone()
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return null;
			}


			return null;
		}
	}

}