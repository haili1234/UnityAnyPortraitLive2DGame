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

	//GizmoController -> Modifier [TF]에 대한 내용이 담겨있다.
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

		// FirstLink : int

		// Multiple Select : int - (Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, SELECT_TYPE areaSelectType)
		// FFD Style Transform : void - (List<object> srcObjects, List<Vector2> posWorlds)
		// FFD Style Transform Start : bool - ()

		//----------------------------------------------------------------
		// Gizmo - MeshGroup : Modifier / TF
		//----------------------------------------------------------------
		public apGizmos.GizmoEventSet GetEventSet_Modifier_TF()
		{
			//Morph는 Vertex / VertexPos 계열 이벤트를 사용하며, Color 처리를 한다.
			return new apGizmos.GizmoEventSet(
				Select__Modifier_Transform,
				Unselect__Modifier_Transform,
				Move__Modifier_Transform,
				Rotate__Modifier_Transform,
				Scale__Modifier_Transform,
				TransformChanged_Position__Modifier_Transform,
				TransformChanged_Rotate__Modifier_Transform,
				TransformChanged_Scale__Modifier_Transform,
				TransformChanged_Color__Modifier_Transform,
				PivotReturn__Modifier_Transform,
				null, null, null, null, null,
				apGizmos.TRANSFORM_UI.TRS | apGizmos.TRANSFORM_UI.Color,
				FirstLink__Modifier_Transform);
		}


		public apGizmos.SelectResult FirstLink__Modifier_Transform()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			if (Editor.Select.SubMeshGroupInGroup != null)
			{
				Editor.Select.SetBone(null);
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.SubMeshGroupInGroup);
			}
			if (Editor.Select.SubMeshInGroup != null)
			{
				Editor.Select.SetBone(null);
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.SubMeshInGroup);
			}
			if (Editor.Select.Modifier.IsTarget_Bone && Editor.Select.Bone != null)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.Bone);
			}
			//if(Editor.Select.SubMeshGroupInGroup != null ||
			//	Editor.Select.SubMeshInGroup != null)
			//{
			//	return 1;
			//}
			//return 0;
			return null;
		}


		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Transform 계열 선택시 [단일 선택]
		/// </summary>
		/// <param name="mousePosGL"></param>
		/// <param name="mousePosW"></param>
		/// <param name="btnIndex"></param>
		/// <param name="selectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult Select__Modifier_Transform(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			// (Editing 상태일때)

			//추가 : Bone 선택
			//렌더링이 Bone이 먼저라서,
			//Bone이 가능하면, Bone을 먼저 선택한다.


			// 1. (Lock 걸리지 않았다면) 다른 Mesh Transform을 선택
			// 그 외에는 선택하는 것이 없다.

			// (Editing 상태가 아닐때)
			// (Lock 걸리지 않았다면) 다른 MeshTransform 선택
			//... Lock만 생각하면 될 것 같다.
			bool isBoneTarget = Editor.Select.Modifier.IsTarget_Bone;

			bool isChildMeshTransformSelectable = Editor.Select.Modifier.IsTarget_ChildMeshTransform;

			apTransform_MeshGroup prevSelectedMeshGroupTransform = Editor.Select.SubMeshGroupInGroup;
			apTransform_Mesh prevSelectedMeshTransform = Editor.Select.SubMeshInGroup;
			apBone prevSelectedBone = Editor.Select.Bone;

			//int prevSelected = 0;
			object prevSelectedObj = null;
			if (prevSelectedMeshGroupTransform != null)
			{
				prevSelectedObj = prevSelectedMeshGroupTransform;
			}
			else if (prevSelectedMeshTransform != null)
			{
				prevSelectedObj = prevSelectedMeshTransform;
			}
			else if (prevSelectedBone != null && isBoneTarget)
			{
				prevSelectedObj = prevSelectedBone;
			}
			//if(prevSelectedMeshGroupTransform != null || prevSelectedMeshTransform != null)
			//{
			//	prevSelected = 1;
			//}

			if (!Editor.Controller.IsMouseInGUI(mousePosGL))
			{
				//return prevSelected;
				return apGizmos.SelectResult.Main.SetSingle(prevSelectedObj);
			}


			_prevSelected_TransformBone = null;

			//GUI에서는 MeshGroup을 선택할 수 없다.
			//리스트 UI에서만 선택 가능함
			//단, Child Mesh Transform을 허용하지 않는다면 얘기는 다르다.

			//int result = prevSelected;

			if (!Editor.Select.IsLockExEditKey)
			{
				//Lock이 걸려있지 않다면 새로 선택할 수 있다.
				//여러개를 선택할 수도 있다....단일 선택만 할까.. => 단일 선택만 하자

				apTransform_Mesh selectedMeshTransform = null;
				apBone selectedBone = null;
				apMeshGroup meshGroup = Editor.Select.MeshGroup;


				if (selectType == apGizmos.SELECT_TYPE.New ||
					selectType == apGizmos.SELECT_TYPE.Add)
				{
					//Bone 먼저 선택하자
					if (isBoneTarget)
					{
						List<apBone> boneList = meshGroup._boneList_Root;
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
							Editor.Select.SetBone(resultBone);
							Editor.Select.SetSubMeshGroupInGroup(null);
							Editor.Select.SetSubMeshInGroup(null);
							selectedBone = resultBone;
							prevSelectedObj = selectedBone;

							//Debug.Log("Select Bone");
						}
					}


					if (selectedBone == null)
					{
						//Bone이 선택 안되었다면 MeshTransform을 선택
						List<apRenderUnit> renderUnits = Editor.Select.MeshGroup._renderUnits_All;//<<정렬된 Render Unit
						for (int iUnit = 0; iUnit < renderUnits.Count; iUnit++)
						{
							apRenderUnit renderUnit = renderUnits[iUnit];

							if (renderUnit._meshTransform != null && renderUnit._meshTransform._mesh != null)
							{
								if (renderUnit._meshTransform._isVisible_Default && renderUnit._meshColor2X.a > 0.1f)//Alpha 옵션 추가
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
					}
				}


				if (selectedMeshTransform != null)
				{
					//만약 ChildMeshGroup에 속한 거라면,
					//Mesh Group 자체를 선택해야 한다.
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

					Editor.Select.SetBone(null);

					//result = 1;
					prevSelectedObj = selectedMeshTransform;
				}
				else
				{
					//Editor.Select.SetBone(null);
					Editor.Select.SetSubMeshInGroup(null);
					//result = 0;
				}


				if (selectedMeshTransform == null && selectedBone == null)
				{
					Editor.Select.SetBone(null);
					Editor.Select.SetSubMeshGroupInGroup(null);
					Editor.Select.SetSubMeshInGroup(null);
				}

			}



			Editor.RefreshControllerAndHierarchy();
			//Editor.Repaint();
			Editor.SetRepaint();

			//return result;
			return apGizmos.SelectResult.Main.SetSingle(prevSelectedObj);
		}



		public void Unselect__Modifier_Transform()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}
			if (!Editor.Select.IsLockExEditKey)
			{
				//락이 풀려있어야 한다.
				Editor.Select.SetBone(null);
				Editor.Select.SetSubMeshGroupInGroup(null);
				Editor.Select.SetSubMeshInGroup(null);


				Editor.RefreshControllerAndHierarchy();
				//Editor.Repaint();
				Editor.SetRepaint();
			}
		}

		//정확한 처리를 위한 작업

		private object _prevSelected_TransformBone = null;
		private Vector2 _prevSelected_MousePosW = Vector2.zero;
		//private Vector2 _prevSelected_WorldPos = Vector2.zero;


		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Transform 계열 위치값을 수정할 때
		/// </summary>
		/// <param name="curMouseGL"></param>
		/// <param name="curMousePosW"></param>
		/// <param name="deltaMoveW"></param>
		/// <param name="btnIndex"></param>
		public void Move__Modifier_Transform(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null
				)
			{
				return;
			}

			if(deltaMoveW.sqrMagnitude == 0.0f && !isFirstMove)
			{
				return;
			}


			//Editing 상태가 아니면 패스 + ParamSet이 없으면 패스
			if (Editor.Select.ExKey_ModParamSet == null || Editor.Select.ExEditingMode == apSelection.EX_EDIT.None)
			{
				return;
			}

			bool isTargetTransform = Editor.Select.ExValue_ModMesh != null;
			bool isTargetBone = Editor.Select.Bone != null
									&& Editor.Select.ModBoneOfMod != null
									&& Editor.Select.Modifier.IsTarget_Bone
									&& Editor.Select.ModBoneOfMod._bone == Editor.Select.Bone;

			//우선 순위는 ModMesh
			if (!isTargetTransform && !isTargetBone)
			{
				return;
			}
			if (!Editor.Controller.IsMouseInGUI(curMouseGL))
			{
				return;
			}

			//새로 ModMesh의 Matrix 값을 만들어주자 //TODO : 해당 Matrix를 사용하는 Modifier 구현 필요
			//Undo
			object targetObj = null;
			if (isTargetTransform)	{ targetObj = Editor.Select.ExValue_ModMesh; }
			else					{ targetObj = Editor.Select.ModBoneOfMod; }

			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_MoveTransform, Editor, Editor.Select.Modifier, targetObj, false);
			}

			if (isTargetTransform)
			{
				//기존의 Pivot이 포함된 Marix를 확인해야한다.
				//여기서 제어하는 건 Local 값이지만, 노출되는건 World이다.
				//값이 이상해질테니 수정을 하자

				apModifiedMesh targetModMesh = Editor.Select.ExValue_ModMesh;

				apMatrix resultMatrix = null;
				
				apMatrix matx_ToParent = null;
				//apMatrix matx_LocalModified = null;
				apMatrix matx_ParentWorld = null;

				object selectedTransform = null;
				if (Editor.Select.ExValue_ModMesh._isMeshTransform)
				{
					if (Editor.Select.ExValue_ModMesh._transform_Mesh != null)
					{
						resultMatrix = targetModMesh._transform_Mesh._matrix_TFResult_World;

						selectedTransform = Editor.Select.ExValue_ModMesh._transform_Mesh;

						matx_ToParent = targetModMesh._transform_Mesh._matrix_TF_ToParent;
						//matx_LocalModified = targetModMesh._transform_Mesh._matrix_TF_LocalModified;
						matx_ParentWorld = targetModMesh._transform_Mesh._matrix_TF_ParentWorld;
					}
				}
				else
				{
					if (Editor.Select.ExValue_ModMesh._transform_MeshGroup != null)
					{
						//localStaticMatrix = Editor.Select.ExValue_ModMesh._transform_MeshGroup._matrix;
						//resultMatrix = Editor.Select.ExValue_ModMesh._transform_MeshGroup._matrix_TFResult_World;
						selectedTransform = Editor.Select.ExValue_ModMesh._transform_MeshGroup;

						resultMatrix = targetModMesh._transform_MeshGroup._matrix_TFResult_World;
						//resultMatrixWithoutMod = targetModMesh._transform_MeshGroup._matrix_TFResult_WorldWithoutMod;

						matx_ToParent = targetModMesh._transform_MeshGroup._matrix_TF_ToParent;
						//matx_LocalModified = targetModMesh._transform_MeshGroup._matrix_TF_LocalModified;
						matx_ParentWorld = targetModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
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

				#region [미사용 코드]
				//apCalculatedLog.InverseResult calResult = targetModMesh.CalculatedLog.World2ModLocalPos_TransformMove(curMousePosW, deltaMoveW);
				//if (calResult == null || !calResult._isSuccess)
				//{


				//	//apMatrix noScaleWorldMatrix = new apMatrix();
				//	//noScaleWorldMatrix.SetPos(resultMatrix._pos);
				//	//noScaleWorldMatrix.SetRotate(resultMatrix._angleDeg);
				//	//noScaleWorldMatrix.MakeMatrix();

				//	apMatrix noScaleWorldMatrixWithoutMod = new apMatrix();
				//	noScaleWorldMatrixWithoutMod.SetPos(resultMatrixWithoutMod._pos);
				//	noScaleWorldMatrixWithoutMod.SetRotate(resultMatrixWithoutMod._angleDeg);
				//	noScaleWorldMatrixWithoutMod.MakeMatrix();

				//	Vector2 worldPosPrev = resultMatrix._pos;
				//	Vector2 worldPosNext = worldPosPrev + deltaMoveW;

				//	//변경된 위치 W (Pos + Delta) => 변경된 위치 L (Pos + Delta)
				//	Vector2 localPosPrev = noScaleWorldMatrixWithoutMod._pos;
				//	Vector2 localPosDelta = noScaleWorldMatrixWithoutMod.InvMulPoint2(worldPosNext);




				//	//Vector2 deltaModLocalPos = new Vector2(localPosDelta.x, localPosDelta.y);

				//	//Vector2 deltaMousePosWFromDown = curMousePosW - _prevSelected_MousePosW;
				//	_prevSelected_MousePosW = curMousePosW;

				//	//Debug.Log("Mouse DeltaW : " + deltaMoveW + " (" + deltaMoveW.magnitude + ") / Checked DeltaW : " + deltaMousePosWFromDown + " (" + deltaMousePosWFromDown.magnitude + ")");

				//	//Debug.Log("Mouse DeltaPos W : " + deltaMoveW + "(" + deltaMoveW.magnitude + ")"
				//	//	+ " / World Pos : " + worldPosPrev + " -> " + worldPosNext + "(" + (worldPosNext - worldPosPrev).magnitude + ")"
				//	//	+ " / Local Pos Delta : " + (localPosDelta) + "(" + (localPosDelta).magnitude + ")");

				//	//targetModMesh._transformMatrix.SetPos(localPosDelta);
				//	//targetModMesh._transformMatrix.MakeMatrix();

				//	//_matrix_TFResult_World.RMultiply(_matrix_TF_ToParent);
				//	//_matrix_TFResult_World.RMultiply(_matrix_TF_LocalModified);
				//	//_matrix_TFResult_World.RMultiply(_matrix_TF_ParentWorld);

				//	//  resultMatrixWithoutMod : ToParent (Default) -> Local Modified(Mod) -> Parent World ==> Pos
				//	//  Pos + Mouse 이동 => NewPos
				//	//  NewPos x ParentWorld-Inv => New-(ToParent x Mod)
				//	Vector2 prevWorldPos = resultMatrix._pos;
				//	Vector2 nextWorldPos = prevWorldPos + deltaMoveW;
				//	Vector2 nextTPLMPos = matx_ParentWorld.RInverseMatrix.MulPoint2(nextWorldPos);

				//	//	ToParent x Modified
				//	apMatrix nextResultMatrix = new apMatrix(resultMatrix);
				//	nextResultMatrix.SetPos(nextResultMatrix._pos + deltaMoveW);

				//	apMatrix nextLocalModified = new apMatrix(matx_ToParent.RInverseMatrix);
				//	nextLocalModified.RMultiply(nextResultMatrix);
				//	nextLocalModified.RInverse(matx_ParentWorld);

				//	Vector2 nextLocalModPos = nextLocalModified._pos;

				//	Debug.Log("Modified : " + targetModMesh._transformMatrix._pos + " >> " + nextLocalModPos + "  (" + deltaMoveW + ")");
				//	targetModMesh._transformMatrix.SetPos(nextLocalModPos);
				//	targetModMesh._transformMatrix.MakeMatrix();

				//}
				//else
				//{

				//	Profiler.BeginSample("Gizmo - ModTransform Move");//>>>>>

				//	//Debug.Log("Cal Process : Delta Move W : " + deltaMoveW + " / CalResult : " + calResult._posL_prev + " >> " + calResult._posL_next + " (" + calResult._deltaPosL + ")");
				//	targetModMesh._transformMatrix.SetPos(calResult._posL_next);
				//	targetModMesh._transformMatrix.MakeMatrix();

				//	Profiler.EndSample();//<<<<<<<<

				//} 
				#endregion

				apMatrix nextWorldMatrix = new apMatrix(resultMatrix);
				nextWorldMatrix.SetPos(resultMatrix._pos + deltaMoveW);
				
				//ToParent x LocalModified x ParentWorld = Result
				//LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
				apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(nextWorldMatrix, matx_ParentWorld));

				//TRS중에 Pos만 넣어도 된다.
				targetModMesh._transformMatrix.SetPos(nextLocalModifiedMatrix._pos);
				
				targetModMesh.RefreshModifiedValues(Editor._portrait);


				Editor.Select.MeshGroup.RefreshForce();

				Editor.SetRepaint();
			}
			else if (isTargetBone)
			{
				//Bone 움직임을 제어하자.
				//ModBone에 값을 넣는다.
				apModifiedBone modBone = Editor.Select.ModBoneOfMod;
				apBone bone = Editor.Select.Bone;
				apMeshGroup meshGroup = Editor.Select.MeshGroup;
				apModifierParamSet paramSet = Editor.Select.ExKey_ModParamSet;

				if (bone._meshGroup != meshGroup)
				{
					return;
				}

				//Move로 제어 가능한 경우는
				//1. IK Tail일 때
				//2. Root Bone일때 (절대값)
				if (bone._isIKTail)
				{
					//Debug.Log("Request IK : " + _boneSelectPosW);
					float weight = 1.0f;
					
					if (bone != _prevSelected_TransformBone || isFirstMove)
					{
						_prevSelected_TransformBone = bone;
						_prevSelected_MousePosW = bone._worldMatrix._pos;
					}
					
					//bool successIK = bone.RequestIK(_boneSelectPosW, weight, !isFirstSelectBone);

					_prevSelected_MousePosW += deltaMoveW;
					Vector2 bonePosW = _prevSelected_MousePosW;//DeltaPos + 절대 위치 절충

					//Vector2 bonePosW = bone._worldMatrix._pos + deltaMoveW;//DeltaPos 이용
					//Vector2 bonePosW = curMousePosW;//절대 위치 이용
					apBone limitedHeadBone = bone.RequestIK_Limited(bonePosW, weight, true, Editor.Select.ModRegistableBones);

					if (limitedHeadBone == null)
					{
						return;
					}


					//apBone headBone = bone._IKHeaderBone;//<<
					apBone curBone = bone;
					//위로 올라가면서 IK 결과값을 Default에 적용하자



					while (true)
					{
						float deltaAngle = curBone._IKRequestAngleResult;
						//if(Mathf.Abs(deltaAngle) > 30.0f)
						//{
						//	deltaAngle *= deltaAngle * 0.1f;
						//}

						apModifiedBone targetModBone = paramSet._boneData.Find(delegate (apModifiedBone a)
						{
							return a._bone == curBone;
						});
						if (targetModBone != null)
						{
							//float nextAngle = curBone._defaultMatrix._angleDeg + deltaAngle;
							//해당 Bone의 ModBone을 가져온다.

							//if(curBone == limitedHeadBone)
							//{
							//	//ModBone의 TransformMatrix 값과
							//	//그 값이 100%로 반영되어야 할 Local Matrix의 값을 비교하자
							//	Debug.Log("[" + curBone._name + "] Bone IK [Mod Angle : " + targetModBone._transformMatrix._angleDeg 
							//		+ " / Local Angle : " + curBone._localMatrix._angleDeg + " ] (Delta : " + deltaAngle + ")");
							//}
							//float nextAngle = targetModBone._transformMatrix._angleDeg + deltaAngle;
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


				//if(modBone._renderUnit != null)
				//{
				//	Editor.Select.MeshGroup.AddForceUpdateTarget(modBone._renderUnit);
				//}
				Editor.Select.MeshGroup.RefreshForce();//<<이거 필수. 이게 있어야 이번 Repaint에서 바로 적용이 된다.
			}

			//Editor.SetDelayedFrameSkip();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Transform 계열 회전값을 수정할 때
		/// </summary>
		/// <param name="deltaAngleW"></param>
		public void Rotate__Modifier_Transform(float deltaAngleW, bool isFirstRotate)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null
				)
			{
				return;
			}
			if(deltaAngleW == 0.0f && !isFirstRotate)
			{
				return;
			}

			//Editing 상태가 아니면 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || Editor.Select.ExKey_ModParamSet == null)
			{
				return;
			}

			//ModMesh와 ModBone을 선택했는지 여부 확인후 처리한다.
			bool isTargetTransform = Editor.Select.ExValue_ModMesh != null;
			bool isTargetBone = Editor.Select.Bone != null
									&& Editor.Select.ModBoneOfMod != null
									&& Editor.Select.Modifier.IsTarget_Bone
									&& Editor.Select.ModBoneOfMod._bone == Editor.Select.Bone;

			//우선 순위는 ModMesh
			if (!isTargetTransform && !isTargetBone)
			{
				return;
			}

			//Undo
			object targetObj = null;
			if (isTargetTransform) { targetObj = Editor.Select.ExValue_ModMesh; }
			else { targetObj = Editor.Select.ModBoneOfMod; }

			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_RotateTransform, Editor, Editor.Select.Modifier, targetObj, false);
			}

			if (isTargetTransform)
			{
				//apCalculatedLog.InverseResult calResult = Editor.Select.ExValue_ModMesh.CalculatedLog.World2ModLocalPos_TransformRotationScaling(deltaAngleW, Vector2.zero);

				//Editor.Select.ExValue_ModMesh._transformMatrix.SetRotate(Editor.Select.ExValue_ModMesh._transformMatrix._angleDeg + deltaAngleW);

				////Pos 보정
				//if (calResult != null && calResult._isSuccess)
				//{
				//	Editor.Select.ExValue_ModMesh._transformMatrix.SetPos(calResult._posL_next);
				//}

				apModifiedMesh targetModMesh = Editor.Select.ExValue_ModMesh;
				apMatrix resultMatrix = null;
				apMatrix matx_ToParent = null;
				//apMatrix matx_LocalModified = null;
				apMatrix matx_ParentWorld = null;

				if(targetModMesh._isMeshTransform)
				{
					if(targetModMesh._transform_Mesh != null)
					{
						//Mesh Transform
						resultMatrix = targetModMesh._transform_Mesh._matrix_TFResult_World;
						matx_ToParent = targetModMesh._transform_Mesh._matrix_TF_ToParent;
						//matx_LocalModified = targetModMesh._transform_Mesh._matrix_TF_LocalModified;
						matx_ParentWorld = targetModMesh._transform_Mesh._matrix_TF_ParentWorld;
					}
				}
				else
				{
					if(Editor.Select.ExValue_ModMesh._transform_MeshGroup != null)
					{
						//Mesh Group Transform
						resultMatrix = targetModMesh._transform_MeshGroup._matrix_TFResult_World;
						matx_ToParent = targetModMesh._transform_MeshGroup._matrix_TF_ToParent;
						//matx_LocalModified = targetModMesh._transform_MeshGroup._matrix_TF_LocalModified;
						matx_ParentWorld = targetModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
					}
				}

				if(resultMatrix == null)
				{
					return;
				}

				apMatrix nextWorldMatrix = new apMatrix(resultMatrix);
				nextWorldMatrix.SetRotate(resultMatrix._angleDeg + deltaAngleW);//각도 변경

				//ToParent x LocalModified x ParentWorld = Result
				//LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
				apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(nextWorldMatrix, matx_ParentWorld));

				targetModMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
														apUtil.AngleTo180(nextLocalModifiedMatrix._angleDeg),
														targetModMesh._transformMatrix._scale);
			}
			else if (isTargetBone)
			{
				apModifiedBone modBone = Editor.Select.ModBoneOfMod;
				apBone bone = Editor.Select.Bone;
				apMeshGroup meshGroup = Editor.Select.MeshGroup;
				apModifierParamSet paramSet = Editor.Select.ExKey_ModParamSet;

				if (bone._meshGroup != meshGroup)
				{
					return;
				}


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

				//if(modBone._renderUnit != null)
				//{
				//	Editor.Select.MeshGroup.AddForceUpdateTarget(modBone._renderUnit);
				//}

			}
			//강제로 업데이트할 객체를 선택하고 Refresh

			Editor.Select.MeshGroup.RefreshForce();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Transform 계열 크기값을 수정할 때
		/// </summary>
		/// <param name="deltaScaleW"></param>
		public void Scale__Modifier_Transform(Vector2 deltaScaleW, bool isFirstScale)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			if(deltaScaleW.sqrMagnitude == 0.0f && !isFirstScale)
			{
				return;
			}

			//Editing 상태가 아니면 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || Editor.Select.ExKey_ModParamSet == null)
			{
				return;
			}

			//ModMesh와 ModBone을 선택했는지 여부 확인후 처리한다.
			bool isTargetTransform = Editor.Select.ExValue_ModMesh != null;
			bool isTargetBone = Editor.Select.Bone != null
									&& Editor.Select.ModBoneOfMod != null
									&& Editor.Select.Modifier.IsTarget_Bone
									&& Editor.Select.ModBoneOfMod._bone == Editor.Select.Bone;

			//우선 순위는 ModMesh
			if (!isTargetTransform && !isTargetBone)
			{
				return;
			}

			//Undo
			object targetObj = null;
			if (isTargetTransform) { targetObj = Editor.Select.ExValue_ModMesh; }
			else { targetObj = Editor.Select.ModBoneOfMod; }
			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_ScaleTransform, Editor, Editor.Select.Modifier, targetObj, false);
			}


			if (isTargetTransform)
			{

				apModifiedMesh targetModMesh = Editor.Select.ExValue_ModMesh;
				
				apMatrix resultMatrix = null;
				
				apMatrix matx_ToParent = null;
				//apMatrix matx_LocalModified = null;
				apMatrix matx_ParentWorld = null;

				if (Editor.Select.ExValue_ModMesh._isMeshTransform)
				{
					if (Editor.Select.ExValue_ModMesh._transform_Mesh != null)
					{
						resultMatrix = targetModMesh._transform_Mesh._matrix_TFResult_World;
						
						matx_ToParent = targetModMesh._transform_Mesh._matrix_TF_ToParent;
						//matx_LocalModified = targetModMesh._transform_Mesh._matrix_TF_LocalModified;
						matx_ParentWorld = targetModMesh._transform_Mesh._matrix_TF_ParentWorld;
					}
				}
				else
				{
					if (Editor.Select.ExValue_ModMesh._transform_MeshGroup != null)
					{
						resultMatrix = targetModMesh._transform_MeshGroup._matrix_TFResult_World;
						
						matx_ToParent = targetModMesh._transform_MeshGroup._matrix_TF_ToParent;
						//matx_LocalModified = targetModMesh._transform_MeshGroup._matrix_TF_LocalModified;
						matx_ParentWorld = targetModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
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

				targetModMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
														nextLocalModifiedMatrix._angleDeg,
														nextLocalModifiedMatrix._scale);
				
				targetModMesh.RefreshModifiedValues(Editor._portrait);


				//Vector3 prevScale3 = Editor.Select.ExValue_ModMesh._transformMatrix._scale;
				//Vector2 prevScale = new Vector2(prevScale3.x, prevScale3.y);

				//apCalculatedLog.InverseResult calResult = Editor.Select.ExValue_ModMesh.CalculatedLog.World2ModLocalPos_TransformRotationScaling(0.0f, deltaScaleW);

				//Editor.Select.ExValue_ModMesh._transformMatrix.SetScale(prevScale + deltaScaleW);

				////Pos 보정
				//if (calResult != null && calResult._isSuccess)
				//{
				//	Editor.Select.ExValue_ModMesh._transformMatrix.SetPos(calResult._posL_next);
				//}

				//Editor.Select.ExValue_ModMesh._transformMatrix.MakeMatrix();

				////강제로 업데이트할 객체를 선택하고 Refresh
				////Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExValue_ModMesh._renderUnit);
			}
			else if (isTargetBone)
			{
				apModifiedBone modBone = Editor.Select.ModBoneOfMod;
				apBone bone = Editor.Select.Bone;
				apMeshGroup meshGroup = Editor.Select.MeshGroup;
				apModifierParamSet paramSet = Editor.Select.ExKey_ModParamSet;

				if (bone._meshGroup != meshGroup)
				{
					return;
				}


				Vector3 prevScale = modBone._transformMatrix._scale;
				Vector2 nextScale = new Vector2(prevScale.x + deltaScaleW.x, prevScale.y + deltaScaleW.y);

				modBone._transformMatrix.SetScale(nextScale);

				//if(modBone._renderUnit != null)
				//{
				//	Editor.Select.MeshGroup.AddForceUpdateTarget(modBone._renderUnit);
				//}
			}
			Editor.Select.MeshGroup.RefreshForce();
			Editor.SetRepaint();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Transform의 위치값 [Position]
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="depth"></param>
		public void TransformChanged_Position__Modifier_Transform(Vector2 pos, int depth)
		{
			//Depth는 무시한다.

			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			//Editing 상태가 아니면 패스 + ParamSet이 없으면 패스
			if (Editor.Select.ExKey_ModParamSet == null || Editor.Select.ExEditingMode == apSelection.EX_EDIT.None)
			{
				return;
			}

			bool isTargetTransform = Editor.Select.ExValue_ModMesh != null;
			bool isTargetBone = Editor.Select.Bone != null
									&& Editor.Select.ModBoneOfMod != null
									&& Editor.Select.Modifier.IsTarget_Bone
									&& Editor.Select.ModBoneOfMod._bone == Editor.Select.Bone;

			//우선 순위는 ModMesh
			if (!isTargetTransform && !isTargetBone)
			{
				return;
			}

			//새로 ModMesh의 Matrix 값을 만들어주자 //TODO : 해당 Matrix를 사용하는 Modifier 구현 필요
			//Undo
			object targetObj = null;
			if (isTargetTransform)
			{ targetObj = Editor.Select.ExValue_ModMesh; }
			else
			{ targetObj = Editor.Select.ModBoneOfMod; }
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_MoveTransform, Editor, Editor.Select.Modifier, targetObj, false);

			if (isTargetTransform)
			{
				apMatrix localStaticMatrix = null;
				apMatrix resultMatrix = null;
				if (Editor.Select.ExValue_ModMesh._isMeshTransform)
				{
					if (Editor.Select.ExValue_ModMesh._transform_Mesh != null)
					{
						localStaticMatrix = Editor.Select.ExValue_ModMesh._transform_Mesh._matrix;
						resultMatrix = Editor.Select.ExValue_ModMesh._transform_Mesh._matrix_TFResult_World;
					}
				}
				else
				{
					if (Editor.Select.ExValue_ModMesh._transform_MeshGroup != null)
					{
						localStaticMatrix = Editor.Select.ExValue_ModMesh._transform_MeshGroup._matrix;
						resultMatrix = Editor.Select.ExValue_ModMesh._transform_MeshGroup._matrix_TFResult_World;
					}
				}

				if (localStaticMatrix == null || resultMatrix == null)
				{
					return;
				}




				////Vector2 prevPos = Editor.Select.ExValue_ModMesh._transformMatrix._pos;
				////Vector2 nextPos = new Vector2(prevPos.x + deltaMoveW.x, prevPos.y + deltaMoveW.y);
				//Vector3 pivotPos = localStaticMatrix.Pos3;
				//Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
				//Vector3 nextModWorldPos = worldPos - pivotPos;//<<<이게 Modifier의 Delta Pos [World]

				//float resultAngle = localStaticMatrix._angleDeg;//<<원점의 Transform은 Rotation만 보면 된다. (Scale 필요 없음)
				//Vector3 modLocalPos = apMatrix3x3.TRS(Vector3.zero, Quaternion.Euler(0, 0, -resultAngle), Vector3.one).MultiplyPoint3x4(nextModWorldPos);

				//Editor.Select.ExValue_ModMesh._transformMatrix.SetPos(modLocalPos.x, modLocalPos.y);
				//Editor.Select.ExValue_ModMesh._transformMatrix.MakeMatrix();

				//>> 직접 적용한다.
				Editor.Select.ExValue_ModMesh._transformMatrix.SetPos(pos);

				Editor.SetRepaint();

				//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExValue_ModMesh._renderUnit);
			}
			else if (isTargetBone)
			{
				//Bone 움직임을 제어하자.
				//ModBone에 값을 넣는다.
				apModifiedBone modBone = Editor.Select.ModBoneOfMod;
				apBone bone = Editor.Select.Bone;
				apMeshGroup meshGroup = Editor.Select.MeshGroup;
				apModifierParamSet paramSet = Editor.Select.ExKey_ModParamSet;

				if (bone._meshGroup != meshGroup)
				{
					return;
				}

				//그냥 직접 제어합니다.
				modBone._transformMatrix.SetPos(pos);

				////Move로 제어 가능한 경우는
				////1. IK Tail일 때
				////2. Root Bone일때 (절대값)
				//if (bone._isIKTail)
				//{
				//	//Debug.Log("Request IK : " + _boneSelectPosW);
				//	float weight = 1.0f;

				//	//bool successIK = bone.RequestIK(_boneSelectPosW, weight, !isFirstSelectBone);
				//	apBone limitedHeadBone = bone.RequestIK_Limited(pos, weight, true, Editor.Select.ModRegistableBones);

				//	if (limitedHeadBone == null)
				//	{
				//		return;
				//	}


				//	//apBone headBone = bone._IKHeaderBone;//<<
				//	apBone curBone = bone;
				//	//위로 올라가면서 IK 결과값을 Default에 적용하자



				//	while (true)
				//	{
				//		float deltaAngle = curBone._IKRequestAngleResult;
				//		//if(Mathf.Abs(deltaAngle) > 30.0f)
				//		//{
				//		//	deltaAngle *= deltaAngle * 0.1f;
				//		//}

				//		apModifiedBone targetModBone = paramSet._boneData.Find(delegate (apModifiedBone a)
				//		{
				//			return a._bone == curBone;
				//		});
				//		if (targetModBone != null)
				//		{
				//			//float nextAngle = curBone._defaultMatrix._angleDeg + deltaAngle;
				//			//해당 Bone의 ModBone을 가져온다.

				//			//if(curBone == limitedHeadBone)
				//			//{
				//			//	//ModBone의 TransformMatrix 값과
				//			//	//그 값이 100%로 반영되어야 할 Local Matrix의 값을 비교하자
				//			//	Debug.Log("[" + curBone._name + "] Bone IK [Mod Angle : " + targetModBone._transformMatrix._angleDeg 
				//			//		+ " / Local Angle : " + curBone._localMatrix._angleDeg + " ] (Delta : " + deltaAngle + ")");
				//			//}
				//			//float nextAngle = targetModBone._transformMatrix._angleDeg + deltaAngle;
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
				//	if (bone._isRigTestPosing)
				//	{
				//		newWorldMatrix.Subtract(bone._rigTestMatrix);
				//	}
				//	//newWorldMatrix.Subtract(localMatrix);//이건 Add로 연산된거라 Subtract해야한다.



				//	//bone._defaultMatrix.SetPos(newWorldMatrix._pos);
				//	modBone._transformMatrix.SetPos(newWorldMatrix._pos);


				//	//bone.MakeWorldMatrix(true);
				//	//bone.GUIUpdate(true);
				//}


				//if (modBone._renderUnit != null)
				//{
				//	Editor.Select.MeshGroup.AddForceUpdateTarget(modBone._renderUnit);
				//}
			}
			Editor.Select.MeshGroup.RefreshForce(true);
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Transform의 위치값 [Rotation]
		/// </summary>
		/// <param name="angle"></param>
		public void TransformChanged_Rotate__Modifier_Transform(float angle)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			//Editing 상태가 아니면 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || Editor.Select.ExKey_ModParamSet == null)
			{
				return;
			}

			//ModMesh와 ModBone을 선택했는지 여부 확인후 처리한다.
			bool isTargetTransform = Editor.Select.ExValue_ModMesh != null;
			bool isTargetBone = Editor.Select.Bone != null
									&& Editor.Select.ModBoneOfMod != null
									&& Editor.Select.Modifier.IsTarget_Bone
									&& Editor.Select.ModBoneOfMod._bone == Editor.Select.Bone;

			//우선 순위는 ModMesh
			if (!isTargetTransform && !isTargetBone)
			{
				return;
			}

			//Undo
			object targetObj = null;
			if (isTargetTransform)
			{ targetObj = Editor.Select.ExValue_ModMesh; }
			else
			{ targetObj = Editor.Select.ModBoneOfMod; }
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_RotateTransform, Editor, Editor.Select.Modifier, targetObj, false);

			if (isTargetTransform)
			{
				Editor.Select.ExValue_ModMesh._transformMatrix.SetRotate(angle);
				Editor.Select.ExValue_ModMesh._transformMatrix.MakeMatrix();

				//강제로 업데이트할 객체를 선택하고 Refresh
				//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExValue_ModMesh._renderUnit);
			}
			else if (isTargetBone)
			{
				apModifiedBone modBone = Editor.Select.ModBoneOfMod;
				apBone bone = Editor.Select.Bone;
				apMeshGroup meshGroup = Editor.Select.MeshGroup;
				apModifierParamSet paramSet = Editor.Select.ExKey_ModParamSet;

				if (bone._meshGroup != meshGroup)
				{
					return;
				}

				//직접 적용한다.
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

				//if (modBone._renderUnit != null)
				//{
				//	Editor.Select.MeshGroup.AddForceUpdateTarget(modBone._renderUnit);
				//}
			}
			Editor.Select.MeshGroup.RefreshForce();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Transform의 위치값 [Scale]
		/// </summary>
		/// <param name="scale"></param>
		public void TransformChanged_Scale__Modifier_Transform(Vector2 scale)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			//Editing 상태가 아니면 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || Editor.Select.ExKey_ModParamSet == null)
			{
				return;
			}

			//ModMesh와 ModBone을 선택했는지 여부 확인후 처리한다.
			bool isTargetTransform = Editor.Select.ExValue_ModMesh != null;
			bool isTargetBone = Editor.Select.Bone != null
									&& Editor.Select.ModBoneOfMod != null
									&& Editor.Select.Modifier.IsTarget_Bone
									&& Editor.Select.ModBoneOfMod._bone == Editor.Select.Bone;

			//우선 순위는 ModMesh
			if (!isTargetTransform && !isTargetBone)
			{
				return;
			}


			//Undo
			object targetObj = null;
			if (isTargetTransform)
			{ targetObj = Editor.Select.ExValue_ModMesh; }
			else
			{ targetObj = Editor.Select.ModBoneOfMod; }
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_ScaleTransform, Editor, Editor.Select.Modifier, targetObj, false);

			if (isTargetTransform)
			{
				Editor.Select.ExValue_ModMesh._transformMatrix.SetScale(scale);
				Editor.Select.ExValue_ModMesh._transformMatrix.MakeMatrix();


				//강제로 업데이트할 객체를 선택하고 Refresh
				//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExValue_ModMesh._renderUnit);
			}
			else if (isTargetBone)
			{
				apModifiedBone modBone = Editor.Select.ModBoneOfMod;
				apBone bone = Editor.Select.Bone;
				apMeshGroup meshGroup = Editor.Select.MeshGroup;
				apModifierParamSet paramSet = Editor.Select.ExKey_ModParamSet;

				if (bone._meshGroup != meshGroup)
				{
					return;
				}


				//직접 적용
				modBone._transformMatrix.SetScale(scale);

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

				//if(modBone._renderUnit != null)
				//{
				//	Editor.Select.MeshGroup.AddForceUpdateTarget(modBone._renderUnit);
				//}
			}
			Editor.Select.MeshGroup.RefreshForce();
		}


		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Transform의 색상값 [Color]
		/// </summary>
		/// <param name="color"></param>
		public void TransformChanged_Color__Modifier_Transform(Color color, bool isVisible)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			//Editing 상태가 아니면 패스 + ModMesh가 없다면 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || Editor.Select.ExValue_ModMesh == null || Editor.Select.ExKey_ModParamSet == null)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_Color, Editor, Editor.Select.Modifier, Editor.Select.ExValue_ModMesh, false);

			Editor.Select.ExValue_ModMesh._meshColor = color;
			Editor.Select.ExValue_ModMesh._isVisible = isVisible;


			//강제로 업데이트할 객체를 선택하고 Refresh
			//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExValue_ModMesh._renderUnit);
			Editor.Select.MeshGroup.RefreshForce();
		}

		public apGizmos.TransformParam PivotReturn__Modifier_Transform()
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return null;
			}

			bool isTransformPivot = false;
			bool isBonePivot = false;

			//Editing 상태가 아니면..
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None)
			{
				return null;
			}


			if (Editor.Select.Modifier.IsTarget_Bone && Editor.Select.Bone != null && Editor.Select.ModBoneOfMod != null)
			{
				isBonePivot = true;
			}

			if (Editor.Select.ExValue_ModMesh != null && Editor.Select.ExValue_ModMesh._renderUnit != null)
			{
				if (Editor.Select.ExValue_ModMesh._renderUnit._meshTransform != null ||
					Editor.Select.ExValue_ModMesh._renderUnit._meshGroupTransform != null)
				{
					isTransformPivot = true;
				}
			}
			//둘다 없으면 Null
			if (!isTransformPivot && !isBonePivot)
			{
				return null;
			}

			//Editing 상태가 아니면

			if (isTransformPivot)
			{
				//apMatrix transformPivotMatrix = null;//기본 Pivot
				//apMatrix modifiedMatrix = Editor.Select.ExValue_ModMesh._transformMatrix;
				apMatrix resultMatrix = null;
				int transformDepth = Editor.Select.ExValue_ModMesh._renderUnit._depth;

				if (Editor.Select.ExValue_ModMesh._renderUnit._meshTransform != null)
				{
					//transformPivotMatrix = Editor.Select.ExValue_ModMesh._renderUnit._meshTransform._matrix;
					resultMatrix = Editor.Select.ExValue_ModMesh._renderUnit._meshTransform._matrix_TFResult_World;
				}
				else if (Editor.Select.ExValue_ModMesh._renderUnit._meshGroupTransform != null)
				{
					//transformPivotMatrix = Editor.Select.ExValue_ModMesh._renderUnit._meshGroupTransform._matrix;
					resultMatrix = Editor.Select.ExValue_ModMesh._renderUnit._meshGroupTransform._matrix_TFResult_World;
				}
				else
				{
					return null;
				}

				//TODO : 이거 맞나.. 모르겠다;;;



				Vector2 worldPos = resultMatrix._pos;
				//Vector2 worldPos = new Vector2(worldPos3.x, worldPos3.y);

				float worldAngle = resultMatrix._angleDeg;
				Vector2 worldScale = resultMatrix._scale;

				apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.TRS;
				if (Editor.Select.Modifier._isColorPropertyEnabled)
				{
					paramType |= apGizmos.TRANSFORM_UI.Color;//<<칼라 옵션이 있는 경우에!
				}

				return apGizmos.TransformParam.Make(
					worldPos,
					worldAngle,
					//modifiedMatrix._angleDeg,
					worldScale,
					//transformPivotMatrix._scale,
					transformDepth,
					Editor.Select.ExValue_ModMesh._meshColor,
					Editor.Select.ExValue_ModMesh._isVisible,
					//worldMatrix, 
					//modifiedMatrix.MtrxToSpace,
					resultMatrix.MtrxToSpace,
					false, paramType,
					Editor.Select.ExValue_ModMesh._transformMatrix._pos,
					Editor.Select.ExValue_ModMesh._transformMatrix._angleDeg,
					Editor.Select.ExValue_ModMesh._transformMatrix._scale);
			}

			if (isBonePivot)
			{
				apBone bone = Editor.Select.Bone;

				if(Editor._boneGUIRenderMode == apEditor.BONE_RENDER_MODE.None)
				{
					//Bone GUI모드가 꺼져있으면 안보인다.
					return null;
				}

				return apGizmos.TransformParam.Make(
					bone._worldMatrix._pos,
					bone._worldMatrix._angleDeg,
					bone._worldMatrix._scale,
					0, bone._color,
					true,
					bone._worldMatrix.MtrxToSpace,
					false, apGizmos.TRANSFORM_UI.TRS,
					Editor.Select.ModBoneOfMod._transformMatrix._pos,
					Editor.Select.ModBoneOfMod._transformMatrix._angleDeg,
					Editor.Select.ModBoneOfMod._transformMatrix._scale);
			}
			return null;

		}
	}
}