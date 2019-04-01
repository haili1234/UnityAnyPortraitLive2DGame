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

		//Bone 관련 Gizmo는 Default / Riggine Test / Modifier / Animation Modifier 네가지 종류가 있다.
		//처리 로직은 유사하나 적용되는 변수가 다름..

		//Select, Move, Rotate, Scale, Transform TRS가 적용된다. (Color빼고 Transform과 동일)
		//다중 선택은 되지 않는다.

		//주의
		// Bone 제어는 "회전"을 제외하고는 대부분 제약이 걸려있다.
		// Move : Local Move / IK 설정이 되어 있을 때에만 이동이 가능하다. 에디터에서 수정시 어떻게 조작할 지 선택해야한다.
		// Scale : 본마다 Lock On / Off 가 되어 있어서, On인 상태에서만 제어가 가능하다. 

		//상세
		// Local Move : Parent를 기준으로 Position을 처음 설정한 뒤, 이 값을 바꿀 수 있다. Root Node만 기본적으로 이 옵션이 On이다. (나머지는 Off)
		// IK : Parent 중 하나를 기준으로 IK Chain을 설정할 수 있다. (Parent ~ Child 사이의 모든 Bone들은 IK Chained 설정이 된다.). 기본값은 Off이며, 이 경우는 Level 1 IK로 인식을 한다.
		// IK Chain : Head ~ Chained ~ Tail로 나뉜다. Head - Tail 간격을 Level로 두고, Head를 제외한 다른 모든 Chained는 Head 까지의 Level을 가지는 각각의 IK를 수행할 수 있다. (꼭 Tail만 IK Solver가 되는건 아니다)

		// 추가) Local Move는 IK Chain이 Disabled된 Parent 노드를 가질 경우에만 가능하다
		// IK Chain은 "자신"이 "어떤 Child를 IK로 삼고 있는가"이다.
		// Header를 기준으로 Chained된 하위 Bone들은 선택권이 없다.
		// Diabled라면, Child Bone은 Local Move 옵션을 "켤 수" 있다. (항상 켜는건 아니다)
		// Chained 된 "사이에 낀" Bone들은 Header와 동일한 Tail을 공유한다.
		// Tail의 입장에서는 누군가로부터 IK가 적용된 것을 알 수 없기에, 별도의 bool 변수를 둔다.
		// IK 옵션의 기본값은 Single (자기 자식 1개에 대해서 Level 1의 IK 적용) 이다. 즉, 자식의 Local Move는 Off이다.
		// Child가 복수인 경우를 위해서 Single IK라도, "어떤 Child를 향해서 IK"를 사용하는지 명시해야한다.
		// 이 경우, 다른 Child는 Local Move가 가능하다. (Parent의 IK가 On이라도, 자신을 향해서 있지 않다면 패스)
		// 따라서, Parent의 IK 대상은, "최종적인 Tail Bone"과 "그 Bone이 있거나 Single IK의 대상이 되는 Child Bone" 2개를 저장해야한다.

		//------------------------------------------------------------------------------------------------------------
		// Bone - 이벤트 리턴 (Default / Rigging Test / Modifier / AnimClip Modifier)
		//------------------------------------------------------------------------------------------------------------
		public apGizmos.GizmoEventSet GetEventSet_Bone_SelectOnly()
		{
			//Debug.Log("GetEventSet_Bone_Default");
			return new apGizmos.GizmoEventSet(Select__Bone_MeshGroupMenu,
												Unselect__Bone_MeshGroupMenu,
												null,
												null,
												null,
												null,
												null,
												null,
												null,
												PivotReturn__Bone_Default,
												null, null, null, null, null,
												apGizmos.TRANSFORM_UI.None,
												FirstLink__Bone);
		}

		public apGizmos.GizmoEventSet GetEventSet_Bone_Default()
		{
			//Debug.Log("GetEventSet_Bone_Default");
			return new apGizmos.GizmoEventSet(Select__Bone_MeshGroupMenu,
												Unselect__Bone_MeshGroupMenu,
												Move__Bone_Default,
												Rotate__Bone_Default,
												Scale__Bone_Default,
												TransformChanged_Position__Bone_Default,
												TransformChanged_Rotate__Bone_Default,
												TransformChanged_Scale__Bone_Default,
												null,
												PivotReturn__Bone_Default,
												null, null, null, null, null,
												apGizmos.TRANSFORM_UI.TRS,
												FirstLink__Bone);
		}



		#region [미사용 코드]
		//public apGizmos.GizmoEventSet GetEventSet_Bone_Modifier()
		//{
		//	return new apGizmos.GizmoEventSet(	Select__Bone_MeshGroupMenu,
		//										Move__Bone_Modifier,
		//										Rotate__Bone_Modifier,
		//										Scale__Bone_Modifier,
		//										TransformChanged_Position__Bone_Modifier,
		//										TransformChanged_Rotate__Bone_Modifier,
		//										TransformChanged_Scale__Bone_Modifier,
		//										null,
		//										PivotReturn__Bone_Modifier,
		//										null, null, null, null, null,
		//										apGizmos.TRANSFORM_UI.TRS,
		//										FirstLink__Bone);
		//}

		//public apGizmos.GizmoEventSet GetEventSet_Bone_Animation()
		//{
		//	return new apGizmos.GizmoEventSet(	Select__Bone_AnimClipMenu,
		//										Move__Bone_Animation,
		//										Rotate__Bone_Animation,
		//										Scale__Bone_Animation,
		//										TransformChanged_Position__Bone_Animation,
		//										TransformChanged_Rotate__Bone_Animation,
		//										TransformChanged_Scale__Bone_Animation,
		//										null,
		//										PivotReturn__Bone_Animation,
		//										null, null, null, null, null,
		//										apGizmos.TRANSFORM_UI.TRS,
		//										FirstLink__Bone);
		//} 
		#endregion



		//------------------------------------------------------------------------------------------------------------
		// Bone - 기본
		//------------------------------------------------------------------------------------------------------------

		public apGizmos.SelectResult FirstLink__Bone()
		{
			if (Editor.Select.Bone != null)
			{
				//return 1;
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.Bone);
			}
			//return 0;
			return null;
		}

		//------------------------------------------------------------------------------------------------------------
		// Bone - Select
		//------------------------------------------------------------------------------------------------------------
		//현재 본을 처음 선택했을 때의 Bone PosW
		//IK 땜시 본 위치와 마우스로 제어하는 위치가 다르다.
		public Vector2 _boneSelectPosW = Vector2.zero;
		public bool _isBoneSelect_MovePosReset = false;
		public apGizmos.SelectResult Select__Bone_MeshGroupMenu(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			//Default에서는 MeshGroup 선택 상태에서 제어한다.
			//Default Matrix 편집 가능한 상태가 아니더라도 일단 선택은 가능하다.
			//다만 MeshGroup이 없으면 실패
			//Debug.Log("Select__Bone_MeshGroupMenu : MousePosW : " + mousePosW + " [ " + btnIndex + " ]");


			if (Editor.Select.MeshGroup == null || Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render)
			{
				return null;
			}
			bool isAnyClick = false;
			apBone prevBone = Editor.Select.Bone;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;

			//TODO : 마우스 클릭 체크
			//Debug.Log("Select__Bone_MeshGroupMenu : MousePosW : " + mousePosW);
			List<apBone> boneList = meshGroup._boneList_Root;
			apBone bone = null;
			apBone resultBone = null;
			//수정 -> 렌더링 순서에 맞게 처리하자.
			//for (int i = 0; i < boneList.Count; i++)
			//{
			//	bone = boneList[i];
			//	if(IsBoneClick(bone, mousePosW))
			//	{
			//		//Debug.Log("Selected : " + bone._name);
			//		Editor.Select.SetBone(bone);
			//		isAnyClick = true;
			//		break;
			//	}
			//}
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
				isAnyClick = true;
			}

			if (!isAnyClick)
			{
				Editor.Select.SetBone(null);
			}

			if (prevBone != Editor.Select.Bone)
			{
				_isBoneSelect_MovePosReset = true;
				Editor.RefreshControllerAndHierarchy();
			}

			if (Editor.Select.Bone != null)
			{
				_boneSelectPosW = Editor.Select.Bone._worldMatrix._pos;
				//return 1;
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.Bone);
			}
			//return 0;
			return null;
		}

		private apBone CheckBoneClick(apBone targetBone, Vector2 mousePosW, Vector2 mousePosGL, apEditor.BONE_RENDER_MODE boneRenderMode, int curBoneDepth)
		{
			apBone resultBone = null;
			apBone bone = null;
			//Child가 먼저 렌더링되므로, 여기가 우선순위
			for (int i = 0; i < targetBone._childBones.Count; i++)
			{
				bone = CheckBoneClick(targetBone._childBones[i], mousePosW, mousePosGL, boneRenderMode, curBoneDepth);
				if (bone != null)
				{
					if (bone._depth > curBoneDepth)//Depth 처리를 해야한다.
					{
						resultBone = bone;
						curBoneDepth = bone._depth;//처리된 Depth 갱신
					}
				}
			}
			if (resultBone != null)
			{
				return resultBone;
			}
			if (resultBone == null)
			{
				if (IsBoneClick(targetBone, mousePosW, mousePosGL, boneRenderMode))
				{
					return targetBone;
				}
			}
			return null;
		}


		public void Unselect__Bone_MeshGroupMenu()
		{
			if (Editor.Select.MeshGroup == null || Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render)
			{
				return;
			}

			Editor.Select.SetBone(null);
			Editor.RefreshControllerAndHierarchy();
		}


		//public apGizmos.SelectResult Select__Bone_AnimClipMenu(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		//{
		//	//AnimClip 편집시에 Bone을 선택할 수 있다.

		//	if(Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null || Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render)
		//	{
		//		return null;
		//	}
		//	apMeshGroup meshGroup = Editor.Select.AnimClip._targetMeshGroup;

		//	//TODO : 마우스 클릭 체크

		//	if(Editor.Select.Bone != null)
		//	{
		//		//return 1;
		//		return apGizmos.SelectResult.Main.SetSingle(Editor.Select.Bone);
		//	}
		//	return null;
		//}



		public bool IsBoneClick(apBone bone, Vector2 mousePosW, Vector2 mousePosGL, apEditor.BONE_RENDER_MODE boneRenderMode)
		{
			if(!bone.IsGUIVisible)
			{
				return false;
			}

			if(boneRenderMode == apEditor.BONE_RENDER_MODE.None)
			{
				return false;
			}

			if (boneRenderMode == apEditor.BONE_RENDER_MODE.Render)
			{

				//뿔 모양의 Bone Shape를 클릭하는지 체크하자 (헬퍼가 아닐 경우)
				if (!bone._shapeHelper)
				{
					if (apEditorUtil.IsPointInTri(mousePosW, bone._worldMatrix._pos, bone._shapePoint_Mid1, bone._shapePoint_End1))
					{
						return true;
					}

					if (apEditorUtil.IsPointInTri(mousePosW, bone._worldMatrix._pos, bone._shapePoint_Mid2, bone._shapePoint_End2))
					{
						return true;
					}
					if (bone._shapeTaper < 100)
					{
						if (apEditorUtil.IsPointInTri(mousePosW, bone._worldMatrix._pos, bone._shapePoint_End1, bone._shapePoint_End2))
						{
							return true;
						}
					}
				}

				//가운데 원점도 체크
				//Size는 10
				Vector2 orgPos = apGL.World2GL(bone._worldMatrix._pos);
				float orgSize = apGL.Zoom * 10.0f;
				if((orgPos - mousePosGL).sqrMagnitude < orgSize * orgSize)
				{
					return true;
				}
				//이전 코드
				//if ((bone._worldMatrix._pos - mousePosW).sqrMagnitude < 10 * 10)
				//{
				//	return true;
				//}
			}
			else
			{
				//End1 _ End2
				// |     |
				//Mid1  Mid2
				//    Pos
				//2. Outline 상태에서는 선을 체크해야한다.
				//1> 헬퍼가 아닌 경우 : 뿔 모양만 체크
				//2> 헬퍼인 경우 : 다이아몬드만 체크

				if (!bone._shapeHelper)
				{
					Vector2 posGL_Org = apGL.World2GL(bone._worldMatrix._pos);
					Vector2 posGL_Mid1 = apGL.World2GL(bone._shapePoint_Mid1);
					Vector2 posGL_Mid2 = apGL.World2GL(bone._shapePoint_Mid2);
					Vector2 posGL_End1 = apGL.World2GL(bone._shapePoint_End1);
					Vector2 posGL_End2 = apGL.World2GL(bone._shapePoint_End2);

					if (IsPointInEdge(posGL_Org, posGL_Mid1, mousePosGL))
					{
						return true;
					}
					if (IsPointInEdge(posGL_Org, posGL_Mid2, mousePosGL))
					{
						return true;
					}
					if (IsPointInEdge(posGL_Mid1, posGL_End1, mousePosGL))
					{
						return true;
					}
					if (IsPointInEdge(posGL_Mid2, posGL_End2, mousePosGL))
					{
						return true;
					}
					if (bone._shapeTaper < 100)
					{
						if (IsPointInEdge(posGL_End1, posGL_End2, mousePosGL))
						{
							return true;
						}
					}
				}
				else
				{
					//가운데 원점 체크
					//여기서는 적당히 체크하자
					//Size는 10
					Vector2 orgPos = apGL.World2GL(bone._worldMatrix._pos);
					float orgSize = apGL.Zoom * 10.0f;
					float minDist = orgSize * 0.5f;
					float maxDist = orgSize * 1.1f;
					float dist = (orgPos - mousePosGL).sqrMagnitude;
					if(dist < maxDist * maxDist && dist > minDist * minDist)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsPointInEdge(Vector2 vertGL_1, Vector2 vertGL_2, Vector2 point_GL)
		{
			float distEdge = apEditorUtil.DistanceFromLine(vertGL_1, vertGL_2, point_GL);
			return distEdge < 3.0f;
		}

		//------------------------------------------------------------------------------------------------------------
		// Bone - Move (Default / Rigging Test / Modifier / AnimClip Modifier)
		// Local Move / IK 처리 옵션에 따라 변경 값이 다르다
		// Local Move / IK는 "허용하는 경우" 서로 배타적으로 수정한다. (토글 버튼을 두자)
		// (1) Local Move : Parent에 상관없이 Local Position을 바꾼다. RootNode를 제외하고는 모두 Off.
		// (2) IK : 제어한 마우스 위치에 맞게 Parent들을 회전한다. 실제로 Local Postion이 바뀌진 않다. 
		//          IK가 설정되어있다면 다중 IK 처리를 하며, 그렇지 않다면 바로 위의 Parent만 IK를 적용한다. (Root 제외)
		//------------------------------------------------------------------------------------------------------------
		/// <summary>
		/// MeshGroup 메뉴에서 Bone의 Default 편집 중의 이동
		/// Edit Mode가 Select로 활성화되어있어야 한다.
		/// 선택한 Bone이 현재 선택한 MeshGroup에 속해있어야 한다.
		/// IK Lock 여부에 따라 값이 바뀐다.
		/// IK Lock이 걸려있을 때) Parent 본의 Default Rotate가 바뀐다. (해당 Bone의 World를 만들어주기 위해서)
		/// IK Lock이 걸려있지않을 때) 해당 Bone의 Default Pos가 바뀐다.
		/// </summary>
		/// <param name="curMouseGL"></param>
		/// <param name="curMousePosW"></param>
		/// <param name="deltaMoveW"></param>
		/// <param name="btnIndex"></param>
		/// <param name="isFirstMove"></param>
		public void Move__Bone_Default(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Bone == null
				|| Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render
				|| Editor.Select.BoneEditMode != apSelection.BONE_EDIT_MODE.SelectAndTRS
				|| !Editor.Controller.IsMouseInGUI(curMouseGL)
				)
			{
				return;
			}

			if(deltaMoveW.sqrMagnitude == 0.0f && !isFirstMove)
			{
				return;
			}

			apBone bone = Editor.Select.Bone;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			if (bone._meshGroup != meshGroup)
			{
				//직접 속하지 않고 자식 MeshGroup의 Transform인 경우 제어할 수 없다.
				return;
			}

			if (isFirstMove)
			{
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneDefaultEdit, Editor, bone._meshGroup, null, false, false);
			}

			//bool isFirstSelectBone = false;
			//TODO..
			if (_isBoneSelect_MovePosReset)
			{
				_boneSelectPosW = bone._worldMatrix._pos;
				_isBoneSelect_MovePosReset = false;
				//isFirstSelectBone = true;
			}
			else
			{
				_boneSelectPosW += deltaMoveW;//<<여기에 IK를 걸어주자
			}
			_boneSelectPosW = curMousePosW;

			//Move로 제어 가능한 경우는
			//1. IK Tail일 때
			//2. Root Bone일때 (절대값)
			if (bone._isIKTail)
			{
				//Debug.Log("Request IK : " + _boneSelectPosW);
				float weight = 1.0f;
				if (deltaMoveW.sqrMagnitude < 5.0f)
				{
					//weight = 0.2f;
				}
				//bool successIK = bone.RequestIK(_boneSelectPosW, weight, !isFirstSelectBone);
				bool successIK = bone.RequestIK(_boneSelectPosW, weight, true);

				if (!successIK)
				{
					return;
				}
				apBone headBone = bone._IKHeaderBone;
				if (headBone != null)
				{
					apBone curBone = bone;
					//위로 올라가면서 IK 결과값을 Default에 적용하자
					while (true)
					{
						float deltaAngle = curBone._IKRequestAngleResult;
						//if(Mathf.Abs(deltaAngle) > 30.0f)
						//{
						//	deltaAngle *= deltaAngle * 0.1f;
						//}
						float nextAngle = curBone._defaultMatrix._angleDeg + deltaAngle;

						nextAngle = apUtil.AngleTo180(nextAngle);

						curBone._defaultMatrix.SetRotate(nextAngle);

						curBone._isIKCalculated = false;
						curBone._IKRequestAngleResult = 0.0f;

						if (curBone == headBone)
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
					headBone.MakeWorldMatrix(true);
					headBone.GUIUpdate(true);
				}
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

				apMatrix localMatrix = bone._localMatrix;
				apMatrix newWorldMatrix = new apMatrix(bone._worldMatrix);
				newWorldMatrix.SetPos(newWorldMatrix._pos + deltaMoveW);

				if (parentMatrix != null)
				{
					newWorldMatrix.RInverse(parentMatrix);
				}
				newWorldMatrix.Subtract(localMatrix);//이건 Add로 연산된거라 Subtract해야한다.
				
				bone._defaultMatrix.SetPos(newWorldMatrix._pos);


				bone.MakeWorldMatrix(true);
				bone.GUIUpdate(true);
			}
		}






		#region [미사용 코드]
		///// <summary>
		///// Rigging Modifier를 제외한 Modifier에서 실제 Mod 값을 변경한다.
		///// 이동값을 수정하며, IK 옵션이 있다면 Parent 또는 그 이상의 Parent부터 계산을 한다.
		///// IK Lock이 걸려있으므로 IK 계산을 해야한다.
		///// </summary>
		///// <param name="curMouseGL"></param>
		///// <param name="curMousePosW"></param>
		///// <param name="deltaMoveW"></param>
		///// <param name="btnIndex"></param>
		///// <param name="isFirstMove"></param>
		//public void Move__Bone_Modifier(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		//{
		//	if(Editor.Select.MeshGroup == null 
		//		|| Editor.Select.Bone == null
		//		|| !Editor._isBoneGUIVisible 
		//		|| Editor.Select.Modifier == null //<<Modifier가 null이면 안된다.
		//		|| !Editor.Controller.IsMouseInGUI(curMouseGL)
		//		|| deltaMoveW.sqrMagnitude == 0.0f
		//		)
		//	{
		//		return;
		//	}

		//	apBone bone = Editor.Select.Bone;
		//	apMeshGroup meshGroup = Editor.Select.MeshGroup;
		//	apMeshGroup boneParentMeshGroup = bone._meshGroup;//Bone이 속한 MeshGroup. 다를 수 있다.
		//	apModifierBase modifier = Editor.Select.Modifier;//선택한 Modifier

		//	if(modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
		//	{
		//		//리깅 Modifier가 아니라면 패스
		//		return;
		//	}

		//	//Editing 상태가 아니면 패스 + ModMesh가 없다면 패스
		//	if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || Editor.Select.ExValue_ModMesh == null || Editor.Select.ExKey_ModParamSet == null)
		//	{
		//		return;
		//	}

		//	apModifiedMesh targetModMesh = Editor.Select.ExValue_ModMesh;

		//	//TODO..
		//}

		///// <summary>
		///// AnimClip 수정 중에 현재 AnimKeyFrame의 ModMesh값을 수정한다.
		///// 이동값을 수정하며, IK 옵션이 있다면 Parent 또는 그 이상의 Parent부터 계산을 한다.
		///// IK Lock이 걸려있으므로 IK 계산을 해야한다.
		///// </summary>
		///// <param name="curMouseGL"></param>
		///// <param name="curMousePosW"></param>
		///// <param name="deltaMoveW"></param>
		///// <param name="btnIndex"></param>
		///// <param name="isFirstMove"></param>
		//public void Move__Bone_Animation(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		//{
		//	if (Editor.Select.AnimClip == null 
		//		|| Editor.Select.AnimClip._targetMeshGroup == null
		//		|| deltaMoveW.sqrMagnitude == 0.0f)
		//	{
		//		return;
		//	}

		//	apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
		//	apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
		//	apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
		//	apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;

		//	if (linkedModifier == null || workKeyframe == null || targetModMesh == null || targetRenderUnit == null)
		//	{
		//		//수정할 타겟이 없다.
		//		return;
		//	}

		//	if (targetModMesh._transform_Mesh == null && targetModMesh._transform_MeshGroup == null)
		//	{
		//		//대상이 되는 Mesh/MeshGroup이 없다?
		//		return;
		//	}

		//	if (!Editor.Select.IsAnimEditing || Editor.Select.IsAnimPlaying)
		//	{
		//		//에디팅 중이 아니다.
		//		return;
		//	}

		//	//마우스가 안에 없다.
		//	if (!Editor.Controller.IsMouseInGUI(curMouseGL))
		//	{
		//		return;
		//	}

		//	//보이는게 없는데 수정하려고 한다;
		//	if(!Editor._isBoneGUIVisible || Editor.Select.Bone == null)
		//	{
		//		return;
		//	}

		//	apBone bone = Editor.Select.Bone;
		//	apMeshGroup meshGroup = Editor.Select.MeshGroup;
		//	apMeshGroup boneParentMeshGroup = bone._meshGroup;//Bone이 속한 MeshGroup. 다를 수 있다.
		//	apModifierBase modifier = Editor.Select.Modifier;//선택한 Modifier

		//	//TODO..
		//} 
		#endregion




		//------------------------------------------------------------------------------------------------------------
		// Bone - Rotate (Default / Rigging Test / Modifier / AnimClip Modifier)
		// Rotate는 IK와 관계가 없다. 값을 수정하자!
		//------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// MeshGroup 메뉴에서 Bone의 Default 편집 중의 회전
		/// Edit Mode가 Select로 활성화되어있어야 한다.
		/// 선택한 Bone이 현재 선택한 MeshGroup에 속해있어야 한다.
		/// IK의 영향을 받지 않는다.
		/// </summary>
		/// <param name="deltaAngleW"></param>
		public void Rotate__Bone_Default(float deltaAngleW, bool isFirstRotate)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Bone == null
				|| Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render
				|| Editor.Select.BoneEditMode != apSelection.BONE_EDIT_MODE.SelectAndTRS
				)
			{
				return;
			}

			if(deltaAngleW == 0.0f && !isFirstRotate)
			{
				return;
			}

			apBone bone = Editor.Select.Bone;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			if (bone._meshGroup != meshGroup)
			{
				//직접 속하지 않고 자식 MeshGroup의 Transform인 경우 제어할 수 없다.
				return;
			}

			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneDefaultEdit, Editor, bone._meshGroup, null, false, false);
			}

			//Default Angle은 -180 ~ 180 범위 안에 들어간다.
			float nextAngle = bone._defaultMatrix._angleDeg + deltaAngleW;
			nextAngle = apUtil.AngleTo180(nextAngle);


			bone._defaultMatrix.SetRotate(nextAngle);
			bone.MakeWorldMatrix(true);
			bone.GUIUpdate(true);
		}



		#region [미사용 코드]
		///// <summary>
		///// Rigging Modifier를 제외한 Modifier에서 실제 Mod값을 변경한다.
		///// 회전값을 수정한다.
		///// IK의 영향을 받지 않는다.
		///// </summary>
		///// <param name="deltaAngleW"></param>
		//public void Rotate__Bone_Modifier(float deltaAngleW)
		//{
		//	if(Editor.Select.MeshGroup == null 
		//		|| Editor.Select.Bone == null
		//		|| !Editor._isBoneGUIVisible 
		//		|| Editor.Select.Modifier == null //<<Modifier가 null이면 안된다.
		//		|| deltaAngleW == 0.0f
		//		)
		//	{
		//		return;
		//	}

		//	apBone bone = Editor.Select.Bone;
		//	apMeshGroup meshGroup = Editor.Select.MeshGroup;
		//	apMeshGroup boneParentMeshGroup = bone._meshGroup;//Bone이 속한 MeshGroup. 다를 수 있다.
		//	apModifierBase modifier = Editor.Select.Modifier;//선택한 Modifier

		//	if(modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
		//	{
		//		//리깅 Modifier가 아니라면 패스
		//		return;
		//	}

		//	//Editing 상태가 아니면 패스 + ModMesh가 없다면 패스
		//	if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || Editor.Select.ExValue_ModMesh == null || Editor.Select.ExKey_ModParamSet == null)
		//	{
		//		return;
		//	}

		//	apModifiedMesh targetModMesh = Editor.Select.ExValue_ModMesh;

		//	//TODO..
		//}


		///// <summary>
		///// AnimClip 수정 중에 현재 AnimKeyFrame의 ModMesh값을 수정한다.
		///// 회전값을 수정한다.
		///// IK의 영향을 받지 않는다.
		///// </summary>
		///// <param name="deltaAngleW"></param>
		//public void Rotate__Bone_Animation(float deltaAngleW)
		//{
		//	if (Editor.Select.AnimClip == null 
		//		|| Editor.Select.AnimClip._targetMeshGroup == null
		//		|| deltaAngleW == 0.0f)
		//	{
		//		return;
		//	}

		//	apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
		//	apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
		//	apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
		//	apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;

		//	if (linkedModifier == null || workKeyframe == null || targetModMesh == null || targetRenderUnit == null)
		//	{
		//		//수정할 타겟이 없다.
		//		return;
		//	}

		//	if (targetModMesh._transform_Mesh == null && targetModMesh._transform_MeshGroup == null)
		//	{
		//		//대상이 되는 Mesh/MeshGroup이 없다?
		//		return;
		//	}

		//	if (!Editor.Select.IsAnimEditing || Editor.Select.IsAnimPlaying)
		//	{
		//		//에디팅 중이 아니다.
		//		return;
		//	}

		//	//보이는게 없는데 수정하려고 한다;
		//	if(!Editor._isBoneGUIVisible || Editor.Select.Bone == null)
		//	{
		//		return;
		//	}

		//	apBone bone = Editor.Select.Bone;
		//	apMeshGroup meshGroup = Editor.Select.MeshGroup;
		//	apMeshGroup boneParentMeshGroup = bone._meshGroup;//Bone이 속한 MeshGroup. 다를 수 있다.
		//	apModifierBase modifier = Editor.Select.Modifier;//선택한 Modifier

		//	//TODO..
		//} 
		#endregion


		//------------------------------------------------------------------------------------------------------------
		// Bone - Scale (Default / Rigging Test / Modifier / AnimClip Modifier)
		// Scale은 IK와 관계가 없다. 값을 수정하자!
		// Scale은 옵션을 On으로 둔 Bone만 가능하다. 옵션 확인할 것
		//------------------------------------------------------------------------------------------------------------
		/// <summary>
		/// MeshGroup 메뉴에서 Bone의 Default 편집 중의 스케일
		/// Edit Mode가 Select로 활성화되어있어야 한다.
		/// 선택한 Bone이 현재 선택한 MeshGroup에 속해있어야 한다.
		/// IK의 영향을 받지 않는다.
		/// </summary>
		/// <param name="deltaScaleW"></param>
		public void Scale__Bone_Default(Vector2 deltaScaleW, bool isFirstScale)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Bone == null
				|| Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render
				|| Editor.Select.BoneEditMode != apSelection.BONE_EDIT_MODE.SelectAndTRS
				)
			{
				return;
			}

			if(deltaScaleW.sqrMagnitude == 0.0f && !isFirstScale)
			{
				return;
			}

			apBone bone = Editor.Select.Bone;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			if (bone._meshGroup != meshGroup)
			{
				//직접 속하지 않고 자식 MeshGroup의 Transform인 경우 제어할 수 없다.
				return;
			}

			if (isFirstScale)
			{
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneDefaultEdit, Editor, bone._meshGroup, null, false, false);
			}

			Vector3 prevScale = bone._defaultMatrix._scale;
			Vector2 nextScale = new Vector2(prevScale.x + deltaScaleW.x, prevScale.y + deltaScaleW.y);

			bone._defaultMatrix.SetScale(nextScale);
			bone.MakeWorldMatrix(true);
			bone.GUIUpdate(true);
		}





		#region [미사용 코드]
		///// <summary>
		///// Rigging Modifier를 제외한 Modifier에서 실제 Mod값을 변경한다.
		///// 크기를 수정한다.
		///// IK의 영향을 받지 않는다.
		///// </summary>
		///// <param name="deltaScaleW"></param>
		//public void Scale__Bone_Modifier(Vector2 deltaScaleW)
		//{
		//	if(Editor.Select.MeshGroup == null 
		//		|| Editor.Select.Bone == null
		//		|| !Editor._isBoneGUIVisible 
		//		|| Editor.Select.Modifier == null //<<Modifier가 null이면 안된다.
		//		|| deltaScaleW.sqrMagnitude == 0.0f
		//		)
		//	{
		//		return;
		//	}

		//	apBone bone = Editor.Select.Bone;
		//	apMeshGroup meshGroup = Editor.Select.MeshGroup;
		//	apMeshGroup boneParentMeshGroup = bone._meshGroup;//Bone이 속한 MeshGroup. 다를 수 있다.
		//	apModifierBase modifier = Editor.Select.Modifier;//선택한 Modifier

		//	if(modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
		//	{
		//		//리깅 Modifier가 아니라면 패스
		//		return;
		//	}

		//	//Editing 상태가 아니면 패스 + ModMesh가 없다면 패스
		//	if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || Editor.Select.ExValue_ModMesh == null || Editor.Select.ExKey_ModParamSet == null)
		//	{
		//		return;
		//	}

		//	apModifiedMesh targetModMesh = Editor.Select.ExValue_ModMesh;

		//	//TODO..
		//}



		///// <summary>
		///// AnimClip 수정 중에 현재 AnimKeyFrame의 ModMesh값을 수정한다.
		///// 크기값을 수정한다.
		///// IK의 영향을 받지 않는다.
		///// </summary>
		///// <param name="deltaScaleW"></param>
		//public void Scale__Bone_Animation(Vector2 deltaScaleW)
		//{
		//	if (Editor.Select.AnimClip == null 
		//		|| Editor.Select.AnimClip._targetMeshGroup == null
		//		|| deltaScaleW.sqrMagnitude == 0.0f)
		//	{
		//		return;
		//	}

		//	apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
		//	apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe;
		//	apModifiedMesh targetModMesh = Editor.Select.ModMeshOfAnim;
		//	apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfAnim;

		//	if (linkedModifier == null || workKeyframe == null || targetModMesh == null || targetRenderUnit == null)
		//	{
		//		//수정할 타겟이 없다.
		//		return;
		//	}

		//	if (targetModMesh._transform_Mesh == null && targetModMesh._transform_MeshGroup == null)
		//	{
		//		//대상이 되는 Mesh/MeshGroup이 없다?
		//		return;
		//	}

		//	if (!Editor.Select.IsAnimEditing || Editor.Select.IsAnimPlaying)
		//	{
		//		//에디팅 중이 아니다.
		//		return;
		//	}

		//	//보이는게 없는데 수정하려고 한다;
		//	if(!Editor._isBoneGUIVisible || Editor.Select.Bone == null)
		//	{
		//		return;
		//	}

		//	apBone bone = Editor.Select.Bone;
		//	apMeshGroup meshGroup = Editor.Select.MeshGroup;
		//	apMeshGroup boneParentMeshGroup = bone._meshGroup;//Bone이 속한 MeshGroup. 다를 수 있다.
		//	apModifierBase modifier = Editor.Select.Modifier;//선택한 Modifier

		//	//TODO..
		//} 
		#endregion


		//------------------------------------------------------------------------------------------
		// Bone - TransformChanged (Default / Rigging Test / Modifier / AnimClip Modifier)
		// Move 값 직접 수정. IK, Local Move 옵션에 따라 무시될 수 있다.
		// World 값이 아니라 Local 값을 수정한다. Local Move가 Lock이 걸린 경우엔 값이 적용되지 않는다.
		//------------------------------------------------------------------------------------------
		public void TransformChanged_Position__Bone_Default(Vector2 pos, int depth)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Bone == null
				|| Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render
				|| Editor.Select.BoneEditMode != apSelection.BONE_EDIT_MODE.SelectAndTRS
				)
			{
				return;
			}
			//Pos는 World 좌표계이다.
			apBone bone = Editor.Select.Bone;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			if (bone._meshGroup != meshGroup)
			{
				//직접 속하지 않고 자식 MeshGroup의 Transform인 경우 제어할 수 없다.
				return;
			}

			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneDefaultEdit, Editor, bone._meshGroup, null, false, false);

			_boneSelectPosW = pos;

			//수정 : 걍 직접 넣자
			bone._defaultMatrix._pos = pos;
			bone.MakeWorldMatrix(true);
			bone.GUIUpdate(true);

			//Depth가 추가되었다.
			if(bone._depth != depth)
			{
				//Depth를 바꾸면 전체적으로 다시 정렬한다.
				//bone._depth = depth;
				bone._meshGroup.ChangeBoneDepth(bone, depth);
				Editor.RefreshControllerAndHierarchy();
			}
		
			

			#region [미사용 코드]
			////Move로 제어 가능한 경우는
			////1. IK Tail일 때
			////2. Root Bone일때 (절대값)
			//if (bone._isIKTail)
			//{
			//	//Debug.Log("Request IK : " + _boneSelectPosW);
			//	bool successIK = bone.RequestIK(_boneSelectPosW, 1.0f, false);//Continuous가 아니다

			//	if (!successIK)
			//	{
			//		return;
			//	}
			//	apBone headBone = bone._IKHeaderBone;
			//	if (headBone != null)
			//	{
			//		apBone curBone = bone;
			//		//위로 올라가면서 IK 결과값을 Default에 적용하자
			//		while (true)
			//		{
			//			float deltaAngle = curBone._IKRequestAngleResult;
			//			float nextAngle = curBone._defaultMatrix._angleDeg + deltaAngle;

			//			if (nextAngle < -180.0f)
			//			{
			//				nextAngle += 360.0f;
			//			}
			//			if (nextAngle > 180.0f)
			//			{
			//				nextAngle -= 360.0f;
			//			}

			//			curBone._defaultMatrix.SetRotate(nextAngle);

			//			curBone._isIKCalculated = false;
			//			curBone._IKRequestAngleResult = 0.0f;

			//			if (curBone == headBone)
			//			{
			//				break;
			//			}
			//			if (curBone._parentBone == null)
			//			{
			//				break;
			//			}
			//			curBone = curBone._parentBone;
			//		}

			//		//마지막으론 World Matrix 갱신
			//		headBone.MakeWorldMatrix(true);
			//		headBone.GUIUpdate(true);
			//	}
			//}
			//else if(bone._parentBone == null)
			//{
			//	apMatrix renderUnitMatrix = null;
			//	if(bone._renderUnit != null)
			//	{
			//		//Render Unit의 World Matrix를 참조하여
			//		//로컬 값을 Default로 적용하자
			//		renderUnitMatrix = bone._renderUnit.WorldMatrixWrap;
			//	}

			//	apMatrix localMatrix = bone._localMatrix;
			//	apMatrix newWorldMatrix = new apMatrix(bone._worldMatrix);
			//	newWorldMatrix.SetPos(pos);//< 직접 입력

			//	if(renderUnitMatrix != null)
			//	{
			//		newWorldMatrix.RInverse(renderUnitMatrix);
			//	}
			//	newWorldMatrix.RInverse(localMatrix);

			//	bone._defaultMatrix.SetPos(newWorldMatrix._pos);


			//	bone.MakeWorldMatrix(true);
			//	bone.GUIUpdate(true);
			//} 
			#endregion
		}



		#region [미사용 코드]
		//public void TransformChanged_Position__Bone_Modifier(Vector2 pos, int depth)
		//{

		//}

		//public void TransformChanged_Position__Bone_Animation(Vector2 pos, int depth)
		//{

		//} 
		#endregion


		//------------------------------------------------------------------------------------------
		// Bone - TransformChanged (Default / Rigging Test / Modifier / AnimClip Modifier)
		// Rotate 값 직접 수정 (IK Range 확인)
		//------------------------------------------------------------------------------------------
		public void TransformChanged_Rotate__Bone_Default(float angle)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Bone == null
				|| Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render
				|| Editor.Select.BoneEditMode != apSelection.BONE_EDIT_MODE.SelectAndTRS
				)
			{
				return;
			}

			apBone bone = Editor.Select.Bone;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			if (bone._meshGroup != meshGroup)
			{
				//직접 속하지 않고 자식 MeshGroup의 Transform인 경우 제어할 수 없다.
				return;
			}

			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneDefaultEdit, Editor, bone._meshGroup, null, false, false);

			//직접 넣어주자
			angle = apUtil.AngleTo180(angle);

			bone._defaultMatrix.SetRotate(angle);//World -> Default로 수정된 값
			bone.MakeWorldMatrix(true);
			bone.GUIUpdate(true);

			#region [미사용 코드]
			////Default Angle은 -180 ~ 180 범위 안에 들어간다.
			////일단 값을 넣어주자. 나중에 Link시에 자동으로 -180 ~ 180 영역으로 돌아감
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
			//dummyWorldMatrix.Subtract(bone._localMatrix);
			//if(bone._isRigTestPosing)
			//{
			//	dummyWorldMatrix.Subtract(bone._rigTestMatrix);
			//}
			//dummyWorldMatrix.MakeMatrix();


			////bone._defaultMatrix.SetRotate(angle);
			//bone._defaultMatrix.SetRotate(dummyWorldMatrix._angleDeg);//World -> Default로 수정된 값
			//bone.MakeWorldMatrix(true);
			//bone.GUIUpdate(true); 
			#endregion
		}



		#region [미사용 코드]
		//public void TransformChanged_Rotate__Bone_Modifier(float angle)
		//{

		//}

		//public void TransformChanged_Rotate__Bone_Animation(float angle)
		//{

		//}

		#endregion
		//------------------------------------------------------------------------------------------
		// Bone - TransformChanged (Default / Rigging Test / Modifier / AnimClip Modifier)
		// Scale 값 직접 수정 (Scale Lock 체크)
		//------------------------------------------------------------------------------------------
		public void TransformChanged_Scale__Bone_Default(Vector2 scale)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Bone == null
				|| Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render
				|| Editor.Select.BoneEditMode != apSelection.BONE_EDIT_MODE.SelectAndTRS
				)
			{
				return;
			}

			apBone bone = Editor.Select.Bone;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			if (bone._meshGroup != meshGroup)
			{
				//직접 속하지 않고 자식 MeshGroup의 Transform인 경우 제어할 수 없다.
				return;
			}

			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneDefaultEdit, Editor, bone._meshGroup, null, false, false);

			//직접 넣자
			bone._defaultMatrix.SetScale(scale);//<<변경된 값

			bone.MakeWorldMatrix(true);
			bone.GUIUpdate(true);


			#region [미사용 코드]
			////Scale 값이 World Matrix이다.
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
			//dummyWorldMatrix.Subtract(bone._localMatrix);
			//if(bone._isRigTestPosing)
			//{
			//	dummyWorldMatrix.Subtract(bone._rigTestMatrix);
			//}
			//dummyWorldMatrix.MakeMatrix();

			////bone._defaultMatrix.SetScale(scale);

			//bone._defaultMatrix.SetScale(dummyWorldMatrix.Scale2);//<<변경된 값

			//bone.MakeWorldMatrix(true);
			//bone.GUIUpdate(true); 
			#endregion
		}


		#region [미사용 코드]
		//public void TransformChanged_Scale__Bone_Modifier(Vector2 scale)
		//{

		//}

		//public void TransformChanged_Scale__Bone_Animation(Vector2 scale)
		//{

		//} 
		#endregion

		//Color는 생략


		//------------------------------------------------------------------------------------------
		// Bone - Pivot Return (Default / Rigging Test / Modifier / AnimClip Modifier)
		//------------------------------------------------------------------------------------------
		public apGizmos.TransformParam PivotReturn__Bone_Default()
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Bone == null
				|| Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render)
			{
				return null;
			}

			apBone bone = Editor.Select.Bone;
			if (Editor.Select.BoneEditMode == apSelection.BONE_EDIT_MODE.SelectAndTRS)
			{
				return apGizmos.TransformParam.Make(
					bone._worldMatrix._pos,
					bone._worldMatrix._angleDeg,
					bone._worldMatrix._scale,
					bone._depth,
					bone._color,
					true,
					bone._worldMatrix.MtrxToSpace,
					false, apGizmos.TRANSFORM_UI.TRS,
					bone._defaultMatrix._pos,
					bone._defaultMatrix._angleDeg,
					bone._defaultMatrix._scale);
			}
			else
			{
				return apGizmos.TransformParam.Make(
					bone._worldMatrix._pos,
					bone._worldMatrix._angleDeg,
					bone._worldMatrix._scale,
					bone._depth,
					bone._color,
					true,
					bone._worldMatrix.MtrxToSpace,
					false, apGizmos.TRANSFORM_UI.None,
					bone._defaultMatrix._pos,
					bone._defaultMatrix._angleDeg,
					bone._defaultMatrix._scale);
			}
		}

		#region [미사용 코드]
		//public apGizmos.TransformParam PivotReturn__Bone_Modifier()
		//{
		//	return null;
		//}

		//public apGizmos.TransformParam PivotReturn__Bone_Animation()
		//{
		//	return null;
		//} 
		#endregion
	}

}