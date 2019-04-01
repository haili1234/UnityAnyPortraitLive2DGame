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

		// FirstLink : int

		// Multiple Select : int - (Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, SELECT_TYPE areaSelectType)
		// FFD Style Transform : void - (List<object> srcObjects, List<Vector2> posWorlds)
		// FFD Style Transform Start : bool - ()

		//------------------------------------------------------------------
		// Gizmo
		//------------------------------------------------------------------
		public apGizmos.GizmoEventSet GetEventSet_MeshEdit_Modify()
		{
			return new apGizmos.GizmoEventSet(	Select__MeshEdit_Modify,
												Unselect__MeshEdit_Modify,
												null, null, null, null, null, null, null,
												PivotReturn__MeshEdit_Modify,
												MultipleSelect__MeshEdit_Modify,
												null, null, null, null,
												apGizmos.TRANSFORM_UI.None,
												FirstLink__MeshEdit_Modify);
		}



		//-----------------------------------------------------------------------
		public apGizmos.SelectResult FirstLink__MeshEdit_Modify()
		{
			if(Editor.Select.Mesh == null || Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Modify)
			{
				return null;
			}
			if(Editor.VertController.Vertices.Count == 0)
			{
				return null;
			}

			if(Editor.VertController.Vertices.Count == 1)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.VertController.Vertex);
			}
			else
			{
				return apGizmos.SelectResult.Main.SetMultiple<apVertex>(Editor.VertController.Vertices);
			}
		}

		public apGizmos.SelectResult Select__MeshEdit_Modify(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if(Editor.Select.Mesh == null || Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Modify)
			{
				return null;
			}

			Vector2 meshOffset = Editor.Select.Mesh._offsetPos;
			if (Editor.Controller.IsMouseInGUI(mousePosGL))
			{
				List<apVertex> vertices = Editor.Select.Mesh._vertexData;
				List<apVertex> selectedVertices = new List<apVertex>();
				apVertex vert = null;
				Vector2 vPosW = Vector2.zero;
				for (int i = 0; i < vertices.Count; i++)
				{
					vert = vertices[i];
					vPosW = vert._pos - meshOffset;

					Vector2 vertPosGL = apGL.World2GL(vPosW);

					if(Editor.Controller.IsVertexClickable(vertPosGL, mousePosGL))
					{
						selectedVertices.Add(vert);
					}
				}

				Editor.VertController.SelectVertices(selectedVertices, selectType);

				Editor.SetRepaint();
				
			}
			
			if(Editor.VertController.Vertices.Count == 0)
			{
				return null;
			}

			if(Editor.VertController.Vertices.Count == 1)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.VertController.Vertex);
			}
			else
			{
				return apGizmos.SelectResult.Main.SetMultiple<apVertex>(Editor.VertController.Vertices);
			}
		}

		public void Unselect__MeshEdit_Modify()
		{
			if(Editor.Select.Mesh == null || Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Modify)
			{
				return;
			}

			Editor.VertController.UnselectVertex();
			Editor.SetRepaint();
		}

		//--------------------------------------------------------------------------
		public apGizmos.SelectResult MultipleSelect__MeshEdit_Modify(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, apGizmos.SELECT_TYPE areaSelectType)
		{
			if (Editor.Select.Mesh == null || Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Modify)
			{
				return null;
			}

			Vector2 meshOffset = Editor.Select.Mesh._offsetPos;

			List<apVertex> vertices = Editor.Select.Mesh._vertexData;
			List<apVertex> selectedVertices = new List<apVertex>();
			apVertex vert = null;
			Vector2 vPosW = Vector2.zero;


			for (int i = 0; i < vertices.Count; i++)
			{
				vert = vertices[i];
				vPosW = vert._pos - meshOffset;

				Vector2 vertPosGL = apGL.World2GL(vPosW);

				bool isSelectable = (mousePosGL_Min.x < vertPosGL.x && vertPosGL.x < mousePosGL_Max.x)
									&& (mousePosGL_Min.y < vertPosGL.y && vertPosGL.y < mousePosGL_Max.y);
				if (isSelectable)
				{
					selectedVertices.Add(vert);
				}
			}

			Editor.VertController.SelectVertices(selectedVertices, areaSelectType);

			Editor.SetRepaint();

			if (Editor.VertController.Vertices.Count == 0)
			{
				return null;
			}

			if (Editor.VertController.Vertices.Count == 1)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.VertController.Vertex);
			}
			else
			{
				return apGizmos.SelectResult.Main.SetMultiple<apVertex>(Editor.VertController.Vertices);
			}
		}

		//-------------------------------------------------------------------------------
		public apGizmos.TransformParam PivotReturn__MeshEdit_Modify()
		{
			if (Editor.Select.Mesh == null || Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Modify)
			{
				return null;
			}

			if (Editor.VertController.Vertices.Count == 0)
			{
				return null;
			}

			if (Editor.VertController.Vertices.Count == 1 && Editor.VertController.Vertex != null)
			{
				return apGizmos.TransformParam.Make(Editor.VertController.Vertex._pos,
					0.0f, Vector2.one, 0,
					Color.white,
					true,
					apMatrix3x3.identity,
					false,
					apGizmos.TRANSFORM_UI.None,
					Editor.VertController.Vertex._pos,
					0.0f, Vector2.one);
			}
			else
			{
				Vector2 posCenter = Vector2.zero;
				for (int i = 0; i < Editor.VertController.Vertices.Count; i++)
				{
					posCenter += Editor.VertController.Vertices[i]._pos;
				}

				posCenter.x /= Editor.VertController.Vertices.Count;
				posCenter.y /= Editor.VertController.Vertices.Count;

				return apGizmos.TransformParam.Make(posCenter,
					0.0f, Vector2.one, 0,
					Color.white,
					true,
					apMatrix3x3.identity,
					true,
					apGizmos.TRANSFORM_UI.None,
					posCenter,
					0.0f, Vector2.one);
			}
		}
	}
}
