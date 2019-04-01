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
using UnityEditor.SceneManagement;
using System.Collections;
using System;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{

	public static class apEditorUtil
	{


		//----------------------------------------------------------------------------------------------------------
		// GUI Delimeter
		//----------------------------------------------------------------------------------------------------------
		public static void GUI_DelimeterBoxV(int height)
		{
			Color prevColor = GUI.backgroundColor;

			if (EditorGUIUtility.isProSkin)	{ GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f); }
			else							{ GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1.0f); }
			
			GUILayout.Box("", GUILayout.Width(4), GUILayout.Height(height));
			GUI.backgroundColor = prevColor;
		}

		public static void GUI_DelimeterBoxH(int width)
		{
			Color prevColor = GUI.backgroundColor;

			if (EditorGUIUtility.isProSkin)	{ GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f); }
			else							{ GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1.0f); }

			GUILayout.Box("", GUILayout.Width(width), GUILayout.Height(4));
			GUI.backgroundColor = prevColor;
		}


		//----------------------------------------------------------------------------------------------------------
		// 색상 공간
		//----------------------------------------------------------------------------------------------------------
		public static ColorSpace GetColorSpace()
		{
			return QualitySettings.activeColorSpace;
		}

		public static bool IsGammaColorSpace()
		{
			return QualitySettings.activeColorSpace != ColorSpace.Linear;
		}

		//----------------------------------------------------------------------------------------------------------
		// Vector
		//----------------------------------------------------------------------------------------------------------
		private static Vector2 _infVector2 = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
		public static Vector2 InfVector2
		{
			get
			{
				return _infVector2;
			}
		}
		//----------------------------------------------------------------------------------------------------------
		// Set Record 계열 함수
		//----------------------------------------------------------------------------------------------------------

		//private static int _lastUndoID = -1;
		
		private static void SetRecordMeshGroupRecursive(apUndoGroupData.ACTION action, apMeshGroup meshGroup, apMeshGroup rootGroup)
		{
			if(meshGroup == null)
			{
				return;
			}
			if (meshGroup != rootGroup)
			{
				Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));
			}

			for (int i = 0; i < meshGroup._childMeshGroupTransforms.Count; i++)
			{
				apMeshGroup childMeshGroup = meshGroup._childMeshGroupTransforms[i]._meshGroup;
				if(childMeshGroup == meshGroup || childMeshGroup == rootGroup)
				{
					continue;
				}

				SetRecordMeshGroupRecursive(action, childMeshGroup, rootGroup);

			}

			//Prefab Apply
			SetPortraitPrefabApply(meshGroup._parentPortrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_Portrait(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			
			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(portrait);
			
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitMeshGroup(	apUndoGroupData.ACTION action,
														apEditor editor,
														apPortrait portrait,
														apMeshGroup meshGroup,
														object keyObject,
														bool isCallContinuous,
														bool isChildRecursive)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.AllMeshGroups);


			
			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			if(meshGroup == null)
			{
				return;
			}

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			if(isChildRecursive)
			{
				SetRecordMeshGroupRecursive(action, meshGroup, meshGroup);
			}


			//Undo.FlushUndoRecordObjects();
			
			//Prefab Apply
			SetPortraitPrefabApply(portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitAllMeshGroup(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.AllMeshGroups);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			//모든 MeshGroup을 Undo에 넣자
			for (int i = 0; i < portrait._meshGroups.Count; i++)
			{
				//Undo.RecordObject(portrait._meshGroups[i], apUndoGroupData.GetLabel(action));
				Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i], apUndoGroupData.GetLabel(action));
			}
			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(portrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									apMeshGroup meshGroup,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, meshGroup, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			if(meshGroup == null)
			{
				return;
			}
			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
			{
				//Undo.RecordObject(meshGroup._modifierStack._modifiers[iMod], apUndoGroupData.GetLabel(action));
				Undo.RegisterCompleteObjectUndo(meshGroup._modifierStack._modifiers[iMod], apUndoGroupData.GetLabel(action));
			}
			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									apMeshGroup meshGroup,
									apModifierBase modifier,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, meshGroup, modifier, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			if(meshGroup == null)
			{
				return;
			}

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			if (modifier == null)
			{
				return;
			}

			//Undo.RecordObject(modifier, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(modifier, apUndoGroupData.GetLabel(action));
			
			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitModifier(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									apModifierBase modifier,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, modifier, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			
			if (modifier == null)
			{
				return;
			}

			//Undo.RecordObject(modifier, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(modifier, apUndoGroupData.GetLabel(action));
			
			//Undo.FlushUndoRecordObjects();
			
			//Prefab Apply
			SetPortraitPrefabApply(portrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitAllMeshGroupAndAllModifiers(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.AllMeshGroups | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			//모든 MeshGroup을 Undo에 넣자
			for (int i = 0; i < portrait._meshGroups.Count; i++)
			{
				//MonoObject별로 다르게 Undo를 등록하자
				//Undo.RecordObject(portrait._meshGroups[i], apUndoGroupData.GetLabel(action));
				Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i], apUndoGroupData.GetLabel(action));

				for (int iMod = 0; iMod < portrait._meshGroups[i]._modifierStack._modifiers.Count; iMod++)
				{
					//Undo.RecordObject(portrait._meshGroups[i]._modifierStack._modifiers[iMod], apUndoGroupData.GetLabel(action));
					Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i]._modifierStack._modifiers[iMod], apUndoGroupData.GetLabel(action));
					
				}
			}

			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(portrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_Mesh(apUndoGroupData.ACTION action,
									apEditor editor,
									apMesh mesh,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, mesh, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Mesh);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(mesh, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(mesh, apUndoGroupData.GetLabel(action));

			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(editor._portrait);
		}


		


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_MeshAndMeshGroups(apUndoGroupData.ACTION action,
									apEditor editor,
									apMesh mesh,
									List<apMeshGroup> meshGroups,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, mesh, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Mesh | apUndoGroupData.SAVE_TARGET.AllMeshGroups);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(mesh, apUndoGroupData.GetLabel(action));
			List<UnityEngine.Object> recordObjects = new List<UnityEngine.Object>();
			recordObjects.Add(mesh);
			//Undo.RegisterCompleteObjectUndo(mesh, apUndoGroupData.GetLabel(action));

			if (meshGroups != null && meshGroups.Count > 0)
			{
				for (int i = 0; i < meshGroups.Count; i++)
				{
					//Undo.RegisterCompleteObjectUndo(meshGroups[i], apUndoGroupData.GetLabel(action));
					recordObjects.Add(meshGroups[i]);
				}
			}

			Undo.RegisterCompleteObjectUndo(recordObjects.ToArray(), apUndoGroupData.GetLabel(action));

			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(editor._portrait);
		}

		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_MeshGroup(apUndoGroupData.ACTION action,
									apEditor editor,
									apMeshGroup meshGroup,
									object keyObject,
									bool isCallContinuous,
									bool isChildRecursive)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, null, meshGroup, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.MeshGroup);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			if(isChildRecursive)
			{
				SetRecordMeshGroupRecursive(action, meshGroup, meshGroup);
			}
			
			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(editor._portrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION action,
									apEditor editor,
									apMeshGroup meshGroup,
									apModifierBase modifier,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, null, meshGroup, modifier, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.Modifier);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			if(modifier == null)
			{
				return;
			}
			//Undo.RecordObject(modifier, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(modifier, apUndoGroupData.GetLabel(action));
			
			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(editor._portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION action,
									apEditor editor,
									apMeshGroup meshGroup,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, null, meshGroup, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			for (int i = 0; i < meshGroup._modifierStack._modifiers.Count; i++)
			{
				//Undo.RecordObject(meshGroup._modifierStack._modifiers[i], apUndoGroupData.GetLabel(action));
				Undo.RegisterCompleteObjectUndo(meshGroup._modifierStack._modifiers[i], apUndoGroupData.GetLabel(action));
				
			}

			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(editor._portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_Modifier(apUndoGroupData.ACTION action,
									apEditor editor,
									apModifierBase modifier,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, null, null, modifier, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.Modifier);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(modifier, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(modifier, apUndoGroupData.GetLabel(action));
			

			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(editor._portrait);
		}


		public static void SetRecordBeforeCreateOrDestroyObject(apPortrait portrait, string label)
		{
			EditorSceneManager.MarkAllScenesDirty();
			Undo.IncrementCurrentGroup();

			//Portrait, Mesh, MeshGroup, Modifier를 저장하자
			Undo.RegisterCompleteObjectUndo(portrait, label);
			//Mesh와 MeshGroup 상태 저장
			for (int i = 0; i < portrait._meshes.Count; i++)
			{
				Undo.RegisterCompleteObjectUndo(portrait._meshes[i], label);
			}

			for (int i = 0; i < portrait._meshGroups.Count; i++)
			{
				//MonoObject별로 다르게 Undo를 등록하자
				Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i], label);

				for (int iMod = 0; iMod < portrait._meshGroups[i]._modifierStack._modifiers.Count; iMod++)
				{
					Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i]._modifierStack._modifiers[iMod], label);
				}
			}

			//Prefab Apply
			SetPortraitPrefabApply(portrait);
		}


		/// <summary>
		/// Monobehaviour 객체가 생성되니 Undo로 기록할 때 호출하는 함수
		/// </summary>
		/// <param name="createdMonoObject"></param>
		/// <param name="label"></param>
		public static void SetRecordCreateMonoObject(MonoBehaviour createdMonoObject, string label)
		{
			if (createdMonoObject == null)
			{
				return;
			}
			
			Undo.RegisterCreatedObjectUndo(createdMonoObject.gameObject, label);

			
			//Undo.FlushUndoRecordObjects();

			
		}



		public static void SetRecordDestroyMonoObject(MonoBehaviour destroyableMonoObject, string label)
		{
			if(destroyableMonoObject == null)
			{
				return;
			}
			
			Undo.DestroyObjectImmediate(destroyableMonoObject.gameObject);

			//Undo.FlushUndoRecordObjects();

			
		}


		public static void SetRecordDestroyMonoObjects(List<MonoBehaviour> destroyableMonoObjects, string label)
		{
			if(destroyableMonoObjects == null || destroyableMonoObjects.Count == 0)
			{
				return;
			}

			for (int i = 0; i < destroyableMonoObjects.Count; i++)
			{
				Undo.DestroyObjectImmediate(destroyableMonoObjects[i].gameObject);
			}
			

			//Undo.FlushUndoRecordObjects();
		}




		public static void SetEditorDirty()
		{
			EditorSceneManager.MarkAllScenesDirty();
		}

		/// <summary>
		/// Undo는 "같은 메뉴"에서만 가능하다. 메뉴를 전환할 때에는 Undo를 
		/// </summary>
		public static void ResetUndo(apEditor editor)
		{
			//apUndoManager.I.Clear();
			if (editor._portrait != null)
			{
				//Undo.ClearUndo(editor._portrait);//이건 일단 빼보자
				apUndoGroupData.I.Clear();
			}
		}


		public static void OnUndoRedoPerformed()
		{
			apUndoGroupData.I.Clear();
		}
		
		//----------------------------------------------------------------------------------------------------------
		// Prefab Check
		//----------------------------------------------------------------------------------------------------------
		public static void DisconnectPrefab(apPortrait portrait)
		{
			if (portrait == null || portrait.gameObject == null)
			{
				return;
			}

			PrefabType prefabType = PrefabUtility.GetPrefabType(portrait.gameObject);
			if(prefabType == PrefabType.DisconnectedPrefabInstance)
			{
				//이미 끊어졌다.
				Debug.LogError("Arleady Disconnected");
				return;
			}

			GameObject rootGameObj = PrefabUtility.FindRootGameObjectWithSameParentPrefab(portrait.gameObject);
			if (rootGameObj == null)
			{
				//Debug.LogError("루트 프리팹 GameObject가 없습니다.");
				return;
			}
			//Debug.Log("루트 프리팹 GameObject : " + rootGameObj.name);

			UnityEngine.Object prefabObj = PrefabUtility.GetCorrespondingObjectFromSource(rootGameObj);

			if(prefabObj == null)
			{
				//Debug.LogError("연결된 프리팹이 없습니다.");
				return;
			}

			PrefabUtility.DisconnectPrefabInstance(rootGameObj);
			EditorSceneManager.MarkAllScenesDirty();

		}
		/// <summary>
		/// Set Record를 하면서 Prefab인 경우 Apply를 자동으로 한다.
		/// </summary>
		/// <param name="portrait"></param>
		public static void SetPortraitPrefabApply(apPortrait portrait)
		{
			return;
			//if(portrait == null || portrait.gameObject == null)
			//{
			//	return;
			//}
			//if(!IsPrefab(portrait.gameObject))
			//{
			//	return;
			//}
			////ApplyPrefab(portrait.gameObject);

			//GameObject rootGameObj = PrefabUtility.FindRootGameObjectWithSameParentPrefab(portrait.gameObject);
			//if (rootGameObj == null)
			//{
			//	//Debug.LogError("루트 프리팹 GameObject가 없습니다.");
			//	return;
			//}
			////Debug.Log("루트 프리팹 GameObject : " + rootGameObj.name);

			//UnityEngine.Object prefabObj = PrefabUtility.GetPrefabParent(rootGameObj);

			//if(prefabObj == null)
			//{
			//	//Debug.LogError("연결된 프리팹이 없습니다.");
			//	return;
			//}

			//PrefabUtility.RecordPrefabInstancePropertyModifications(rootGameObj);
			//EditorSceneManager.MarkAllScenesDirty();
		}

		public static bool IsPrefab(GameObject gameObject)
		{
			
			PrefabType prefabType = PrefabUtility.GetPrefabType(gameObject);
			
			if(prefabType != PrefabType.PrefabInstance)
			{
				//Debug.LogError("프리팹이 아닙니다. : " + prefabType);
				return false;
			}
			return true;
		}

		public static void ApplyPrefab(GameObject gameObject, bool isReplaceNameBased = false)
		{
			
			GameObject rootGameObj = PrefabUtility.FindRootGameObjectWithSameParentPrefab(gameObject);
			if (rootGameObj == null)
			{
				//Debug.LogError("루트 프리팹 GameObject가 없습니다.");
				return;
			}
			//Debug.Log("루트 프리팹 GameObject : " + rootGameObj.name);

			UnityEngine.Object prefabObj = PrefabUtility.GetCorrespondingObjectFromSource(rootGameObj);

			if(prefabObj == null)
			{
				//Debug.LogError("연결된 프리팹이 없습니다.");
				return;
			}
			//Debug.Log("연결된 프리팹 : " + prefabObj.name);

			if (isReplaceNameBased)
			{
				
				PrefabUtility.ReplacePrefab(rootGameObj, prefabObj, ReplacePrefabOptions.ConnectToPrefab | ReplacePrefabOptions.ReplaceNameBased);
			}
			else
			{
				PrefabUtility.ReplacePrefab(rootGameObj, prefabObj, ReplacePrefabOptions.ConnectToPrefab);
			}

		}


		//----------------------------------------------------------------------------------------------------------
		// GUI : Toggle Button
		//----------------------------------------------------------------------------------------------------------


		public static void ReleaseGUIFocus()
		{
			GUI.FocusControl(null);
		}

		public static Color BoxTextColor
		{
			get
			{
				if (EditorGUIUtility.isProSkin)
				{
					//return Color.white;
					return GUI.skin.label.normal.textColor;
				}
				else
				{
					return GUI.skin.box.normal.textColor;
				}
			}
		}
		
		public static Color ToggleBoxColor_Selected
		{
			get
			{
				if (EditorGUIUtility.isProSkin)
				{
					return new Color(0.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					return new Color(0.0f, 0.2f, 0.3f, 1.0f);
				}
			}
		}

		public static Color ToggleBoxColor_SelectedWithImage
		{
			get
			{
				if (EditorGUIUtility.isProSkin)
				{
					return new Color(0.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					return new Color(0.3f, 1.0f, 1.0f, 1.0f);
				}
			}
		}



		public static Color ToggleBoxColor_NotAvailable
		{
			get
			{

				if (EditorGUIUtility.isProSkin)
				{
					return new Color(0.1f, 0.1f, 0.1f, 1.0f);
				}
				else
				{
					return new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}
			}
		}

		public static bool ToggledButton(string strText, bool isSelected, int width)
		{
			return ToggledButton(strText, isSelected, width, 20);
		}

		public static bool ToggledButton(string strText, bool isSelected, int width, int height)
		{
			if (isSelected)
			{
				//GUI.skin.box
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.normal.textColor = Color.white;
				guiStyle.alignment = TextAnchor.MiddleCenter;
				guiStyle.margin = GUI.skin.button.margin;

				Color prevColor = GUI.backgroundColor;
				if(EditorGUIUtility.isProSkin)
				{
					//밝은 파랑 + 하늘색
					guiStyle.normal.textColor = Color.cyan;
				}
				else
				{
					//짙은 남색
					//GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);
				}

				GUI.backgroundColor = ToggleBoxColor_Selected;
				

				GUILayout.Box(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				return GUILayout.Button(strText, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		public static bool ToggledButton(string strText, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						textColor = Color.black;
						//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					}
					else
					{
						textColor = Color.white;
						//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					if(EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						textColor = Color.cyan;
						//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//짙은 남색 + 흰색
						textColor = Color.white;
						//GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);
					}

					GUI.backgroundColor = ToggleBoxColor_Selected;
					
				}


				//GUI.skin.box
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.normal.textColor = textColor;
				guiStyle.alignment = TextAnchor.MiddleCenter;
				guiStyle.margin = GUI.skin.button.margin;

				//GUILayout.Box(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Button(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));//더미 버튼

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.alignment = TextAnchor.MiddleCenter;

				return GUILayout.Button(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton(string strText, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						textColor = Color.black;
						//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					}
					else
					{
						textColor = Color.white;
						//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					if(EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						textColor = Color.cyan;
						//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//짙은 남색 + 흰색
						textColor = Color.white;
						//GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);
					}

					GUI.backgroundColor = ToggleBoxColor_Selected;
					
				}


				//GUI.skin.box
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.normal.textColor = textColor;
				guiStyle.alignment = TextAnchor.MiddleCenter;
				guiStyle.margin = GUI.skin.button.margin;

				//GUILayout.Box(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Button(new GUIContent(strText, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));//더미 버튼

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.alignment = TextAnchor.MiddleCenter;

				return GUILayout.Button(new GUIContent(strText, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		public static bool ToggledButton(Texture2D texture, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						textColor = Color.black;
						//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					}
					else
					{
						textColor = Color.white;
						//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}

					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					if(EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						textColor = Color.cyan;
						//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//"밝은" 남색 + 흰색
						textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}
				

				//GUI.skin.box
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.normal.textColor = textColor;
				guiStyle.alignment = TextAnchor.MiddleCenter;
				guiStyle.margin = GUI.skin.button.margin;

				GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.alignment = TextAnchor.MiddleCenter;

				return GUILayout.Button(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton(Texture2D texture, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						textColor = Color.black;
						//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					}
					else
					{
						textColor = Color.white;
						//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}

					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					if(EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						textColor = Color.cyan;
						//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//"밝은" 남색 + 흰색
						textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}
				

				//GUI.skin.box
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.normal.textColor = textColor;
				guiStyle.alignment = TextAnchor.MiddleCenter;
				guiStyle.margin = GUI.skin.button.margin;

				GUILayout.Box(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.alignment = TextAnchor.MiddleCenter;

				return GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		public static bool ToggledButton(Texture2D texture, string strText, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						textColor = Color.black;
						//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					}
					else
					{
						textColor = Color.white;
						//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					if(EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						textColor = Color.cyan;
						//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//"밝은" 남색 + 흰색
						textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}


				//GUI.skin.box
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.normal.textColor = textColor;
				guiStyle.alignment = TextAnchor.MiddleCenter;
				guiStyle.margin = GUI.skin.button.margin;

				GUILayout.Box(new GUIContent(strText, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.alignment = TextAnchor.MiddleCenter;

				return GUILayout.Button(new GUIContent(strText, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}



		public static bool ToggledButton(Texture2D texture, string strText, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						textColor = Color.black;
						//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					}
					else
					{
						textColor = Color.white;
						//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					if(EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						textColor = Color.cyan;
						//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//"밝은" 남색 + 흰색
						textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}


				//GUI.skin.box
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.normal.textColor = textColor;
				guiStyle.alignment = TextAnchor.MiddleCenter;
				guiStyle.margin = GUI.skin.button.margin;

				GUILayout.Box(new GUIContent(strText, texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.alignment = TextAnchor.MiddleCenter;

				return GUILayout.Button(new GUIContent(strText, texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton_2Side(Texture2D texture, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
					
				}

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = Color.red;

				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.normal.textColor = textColor;
				guiStyle.padding = GUI.skin.box.padding;

				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				return GUILayout.Button(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton_2Side(Texture2D texture, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
					
				}

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = Color.red;

				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.normal.textColor = textColor;
				guiStyle.padding = GUI.skin.box.padding;

				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				return GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton_2Side(Texture2D textureSelected, Texture2D textureNotSelected, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = Color.red;

				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;

				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(textureSelected, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				return GUILayout.Button(textureNotSelected, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}



		public static bool ToggledButton_2Side(Texture2D textureSelected, Texture2D textureNotSelected, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = Color.red;

				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;

				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(new GUIContent(textureSelected, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;

				return GUILayout.Button(new GUIContent(textureNotSelected, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		public static bool ToggledButton_2Side(Texture2D texture, string strTextSelected, string strTextNotSelected, bool isSelected, bool isAvailable, int width, int height, GUIStyle alignmentStyle = null)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = Color.red;

				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.normal.textColor = textColor;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}



				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}


				return GUILayout.Button(new GUIContent(strTextNotSelected, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}



		public static bool ToggledButton_2Side(Texture2D texture, string strTextSelected, string strTextNotSelected, bool isSelected, bool isAvailable, int width, int height, string toolTip, GUIStyle alignmentStyle = null)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = Color.red;

				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.normal.textColor = textColor;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}



				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}


				return GUILayout.Button(new GUIContent(strTextNotSelected, texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton_2Side(string strTextSelected, string strTextNotSelected, bool isSelected, bool isAvailable, int width, int height, GUIStyle alignmentStyle = null)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = Color.red;

				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.normal.textColor = textColor;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}



				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(strTextSelected, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}


				return GUILayout.Button(strTextNotSelected, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton_2Side(Texture2D textureSelected, Texture2D textureNotSelected, Texture2D textureNotAvailable,
												string strTextSelected, string strTextNotSelected, string strTextNotAvailable,
												bool isSelected, bool isAvailable, int width, int height, GUIStyle alignmentStyle = null)
		{
			Color prevColor = GUI.backgroundColor;
			Color textColor = Color.white;

			if (!isAvailable)
			{
				if (EditorGUIUtility.isProSkin)
				{
					textColor = Color.black;
					GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
				}
				else
				{
					textColor = Color.white;
					GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}

				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.normal.textColor = textColor;
				guiStyle.margin = GUI.skin.button.margin;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}

				GUILayout.Box(new GUIContent(strTextNotAvailable, textureNotAvailable), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else if (isSelected)
			{
				if (EditorGUIUtility.isProSkin)
				{
					//밝은 파랑 + 하늘색
					textColor = Color.cyan;
					GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					//청록색 + 흰색
					textColor = Color.white;
					//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
				}

				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.normal.textColor = textColor;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}

				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, textureSelected), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtn;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}

				return GUILayout.Button(new GUIContent(strTextNotSelected, textureNotSelected), guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}




		public static bool ToggledButton_2Side(Texture2D textureSelected, Texture2D textureNotSelected, Texture2D textureNotAvailable,
												string strTextSelected, string strTextNotSelected, string strTextNotAvailable,
												bool isSelected, bool isAvailable, int width, int height, string tooltip, GUIStyle alignmentStyle = null)
		{
			Color prevColor = GUI.backgroundColor;
			Color textColor = Color.white;

			if (!isAvailable)
			{
				if (EditorGUIUtility.isProSkin)
				{
					textColor = Color.black;
					GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
				}
				else
				{
					textColor = Color.white;
					GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}

				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.normal.textColor = textColor;
				guiStyle.margin = GUI.skin.button.margin;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}

				GUILayout.Box(new GUIContent(strTextNotAvailable, textureNotAvailable, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else if (isSelected)
			{
				if (EditorGUIUtility.isProSkin)
				{
					//밝은 파랑 + 하늘색
					textColor = Color.cyan;
					GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					//청록색 + 흰색
					textColor = Color.white;
					//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
				}

				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.normal.textColor = textColor;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}

				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, textureSelected, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtn;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}

				return GUILayout.Button(new GUIContent(strTextNotSelected, textureNotSelected, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		//----------------------------------------------------------------------------------------------------------
		// Delayed Vector2Field
		//----------------------------------------------------------------------------------------------------------
		public static Vector2 DelayedVector2Field(Vector2 vectorValue, int width)
		{
			Vector2 result = vectorValue;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			if (width > 100)
			{
				int widthLabel = 15;
				int widthData = ((width - ((15 + 2) * 2)) / 2) - 2;
				EditorGUILayout.LabelField("X", GUILayout.Width(widthLabel));
				result.x = EditorGUILayout.DelayedFloatField(vectorValue.x, GUILayout.Width(widthData));

				EditorGUILayout.LabelField("Y", GUILayout.Width(widthLabel));
				result.y = EditorGUILayout.DelayedFloatField(vectorValue.y, GUILayout.Width(widthData));
			}
			else
			{
				int widthData = (width / 2) - 2;
				result.x = EditorGUILayout.DelayedFloatField(vectorValue.x, GUILayout.Width(widthData));
				result.y = EditorGUILayout.DelayedFloatField(vectorValue.y, GUILayout.Width(widthData));
			}



			EditorGUILayout.EndHorizontal();

			return result;
		}


		//----------------------------------------------------------------------------------------------------------
		// White Color Texture
		//----------------------------------------------------------------------------------------------------------
		//private static Texture2D _whiteSmallTexture = null;
		public static Texture2D WhiteTexture
		{
			get
			{
				return EditorGUIUtility.whiteTexture;
				//if(_whiteSmallTexture == null)
				//{
				//	_whiteSmallTexture = new Texture2D(4, 4);
				//	for (int iX = 0; iX < 4; iX++)
				//	{
				//		for (int iY = 0; iY < 4; iY++)
				//		{
				//			_whiteSmallTexture.SetPixel(iX, iY, Color.white);
				//		}
				//	}
				//	_whiteSmallTexture.Apply();
				//}

				//return _whiteSmallTexture;
			}
		}

		private static GUIStyle _whiteGUIStyle = null;
		public static GUIStyle WhiteGUIStyle
		{
			get
			{
				if(_whiteGUIStyle == null)
				{
					_whiteGUIStyle = new GUIStyle(GUIStyle.none);
					_whiteGUIStyle.normal.background = WhiteTexture;
				}

				return _whiteGUIStyle;
			}
		}

		private static GUIStyle _whiteGUIStyle_Box = null;
		public static GUIStyle WhiteGUIStyle_Box
		{
			get
			{
				if(_whiteGUIStyle_Box == null)
				{
					_whiteGUIStyle_Box = new GUIStyle(GUI.skin.box);
					_whiteGUIStyle_Box.normal.background = WhiteTexture;
				}

				return _whiteGUIStyle_Box;
			}
		}

		//----------------------------------------------------------------------------------------------------------
		// Graphics Functions
		//----------------------------------------------------------------------------------------------------------
		public static float DistanceFromLine(Vector2 posA, Vector2 posB, Vector2 posTarget)
		{
			//float lineLen = Vector2.Distance(posA, posB);
			//if(lineLen < 0.1f)
			//{
			//	return Vector2.Distance(posA, posTarget);
			//}

			//float proj = (posTarget.x - posA.x) * (posB.x - posA.x) + (posTarget.y - posA.y) * (posB.y - posA.y);
			//if(proj < 0)
			//{
			//	return Vector2.Distance(posA, posTarget);
			//}
			//else if(proj > lineLen)
			//{
			//	return Vector2.Distance(posB, posTarget);
			//}

			//return Mathf.Abs((-1) * (posTarget.x - posA.x) * (posB.y - posA.y) + (posTarget.y - posA.y) * (posB.x - posA.x)) / lineLen;

			//float lineLen = Vector2.Distance(posA, posB);
			float dotA = Vector2.Dot(posTarget - posA, (posB - posA).normalized);
			float dotB = Vector2.Dot(posTarget - posB, (posA - posB).normalized);

			if (dotA < 0.0f)
			{
				return Vector2.Distance(posA, posTarget);
			}

			if (dotB < 0.0f)
			{
				return Vector2.Distance(posB, posTarget);
			}

			return Vector2.Distance((posA + (posB - posA).normalized * dotA), posTarget);
		}

		public static bool IsMouseInMesh(Vector2 mousePos, apMesh targetMesh)
		{
			Vector2 mousePosW = apGL.GL2World(mousePos);

			Vector2 mousePosL = mousePosW + targetMesh._offsetPos;//<<이걸 추가해줘야 Local Pos가 된다.

			List<apMeshPolygon> polygons = targetMesh._polygons;
			for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
			{
				List<apMeshTri> tris = polygons[iPoly]._tris;
				for (int iTri = 0; iTri < tris.Count; iTri++)
				{
					apMeshTri tri = tris[iTri];
					if (tri.IsPointInTri(mousePosL))
					{
						return true;
					}
				}
			}
			return false;
		}


		public static bool IsMouseInMesh(Vector2 mousePos, apMesh targetMesh, apMatrix3x3 matrixWorldToMeshLocal)
		{
			Vector2 mousePosW = apGL.GL2World(mousePos);

			Vector2 mousePosL = matrixWorldToMeshLocal.MultiplyPoint(mousePosW);

			//Vector2 mousePosL = mousePosW + targetMesh._offsetPos;//<<이걸 추가해줘야 Local Pos가 된다.

			List<apMeshPolygon> polygons = targetMesh._polygons;
			for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
			{
				List<apMeshTri> tris = polygons[iPoly]._tris;
				for (int iTri = 0; iTri < tris.Count; iTri++)
				{
					apMeshTri tri = tris[iTri];
					if (tri.IsPointInTri(mousePosL))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool IsMouseInRenderUnitMesh(Vector2 mousePos, apRenderUnit meshRenderUnit)
		{
			if (meshRenderUnit._meshTransform == null)
			{
				return false;
			}

			if (meshRenderUnit._meshTransform._mesh == null || meshRenderUnit._renderVerts.Count == 0)
			{
				return false;
			}

			apMesh targetMesh = meshRenderUnit._meshTransform._mesh;
			List<apRenderVertex> rVerts = meshRenderUnit._renderVerts;

			Vector2 mousePosW = apGL.GL2World(mousePos);

			apRenderVertex rVert0, rVert1, rVert2;
			List<apMeshPolygon> polygons = targetMesh._polygons;
			for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
			{
				List<apMeshTri> tris = polygons[iPoly]._tris;
				for (int iTri = 0; iTri < tris.Count; iTri++)
				{
					apMeshTri tri = tris[iTri];
					rVert0 = rVerts[tri._verts[0]._index];
					rVert1 = rVerts[tri._verts[1]._index];
					rVert2 = rVerts[tri._verts[2]._index];

					if (apMeshTri.IsPointInTri(mousePosW,
												rVert0._pos_World,
												rVert1._pos_World,
												rVert2._pos_World))
					{
						return true;
					}
				}
			}
			return false;
		}


		public static bool IsPointInTri(Vector2 point, Vector2 triPoint0, Vector2 triPoint1, Vector2 triPoint2)
		{
			float s = triPoint0.y * triPoint2.x - triPoint0.x * triPoint2.y + (triPoint2.y - triPoint0.y) * point.x + (triPoint0.x - triPoint2.x) * point.y;
			float t = triPoint0.x * triPoint1.y - triPoint0.y * triPoint1.x + (triPoint0.y - triPoint1.y) * point.x + (triPoint1.x - triPoint0.x) * point.y;

			if ((s < 0) != (t < 0))
			{
				return false;
			}

			var A = -triPoint1.y * triPoint2.x + triPoint0.y * (triPoint2.x - triPoint1.x) + triPoint0.x * (triPoint1.y - triPoint2.y) + triPoint1.x * triPoint2.y;
			if (A < 0.0)
			{
				s = -s;
				t = -t;
				A = -A;
			}
			return s > 0 && t > 0 && (s + t) <= A;

		}
		//----------------------------------------------------------------------------------------------------
		public static apImageSet.PRESET GetModifierIconType(apModifierBase.MODIFIER_TYPE modType)
		{
			switch (modType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:
					return apImageSet.PRESET.Modifier_Volume;

				case apModifierBase.MODIFIER_TYPE.Volume:
					return apImageSet.PRESET.Modifier_Volume;

				case apModifierBase.MODIFIER_TYPE.Morph:
					return apImageSet.PRESET.Modifier_Morph;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					return apImageSet.PRESET.Modifier_AnimatedMorph;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					return apImageSet.PRESET.Modifier_Rigging;

				case apModifierBase.MODIFIER_TYPE.Physic:
					return apImageSet.PRESET.Modifier_Physic;

				case apModifierBase.MODIFIER_TYPE.TF:
					return apImageSet.PRESET.Modifier_TF;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					return apImageSet.PRESET.Modifier_AnimatedTF;

				case apModifierBase.MODIFIER_TYPE.FFD:
					return apImageSet.PRESET.Modifier_FFD;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					return apImageSet.PRESET.Modifier_AnimatedFFD;

			}
			return apImageSet.PRESET.Modifier_Volume;
		}

		public static apImageSet.PRESET GetPhysicsPresetIconType(apPhysicsPresetUnit.ICON iconType)
		{
			switch (iconType)
			{
				case apPhysicsPresetUnit.ICON.Cloth1:
					return apImageSet.PRESET.Physic_PresetCloth1;
				case apPhysicsPresetUnit.ICON.Cloth2:
					return apImageSet.PRESET.Physic_PresetCloth2;
				case apPhysicsPresetUnit.ICON.Cloth3:
					return apImageSet.PRESET.Physic_PresetCloth3;
				case apPhysicsPresetUnit.ICON.Flag:
					return apImageSet.PRESET.Physic_PresetFlag;
				case apPhysicsPresetUnit.ICON.Hair:
					return apImageSet.PRESET.Physic_PresetHair;
				case apPhysicsPresetUnit.ICON.Ribbon:
					return apImageSet.PRESET.Physic_PresetRibbon;
				case apPhysicsPresetUnit.ICON.RubberHard:
					return apImageSet.PRESET.Physic_PresetRubberHard;
				case apPhysicsPresetUnit.ICON.RubberSoft:
					return apImageSet.PRESET.Physic_PresetRubberSoft;
				case apPhysicsPresetUnit.ICON.Custom1:
					return apImageSet.PRESET.Physic_PresetCustom1;
				case apPhysicsPresetUnit.ICON.Custom2:
					return apImageSet.PRESET.Physic_PresetCustom2;
				case apPhysicsPresetUnit.ICON.Custom3:
					return apImageSet.PRESET.Physic_PresetCustom3;
			}
			return apImageSet.PRESET.Physic_PresetCustom3;
		}


		public static apImageSet.PRESET GetControlParamPresetIconType(apControlParam.ICON_PRESET iconType)
		{
			switch (iconType)
			{
				case apControlParam.ICON_PRESET.None:
					return apImageSet.PRESET.Hierarchy_Param;
				case apControlParam.ICON_PRESET.Head:
					return apImageSet.PRESET.ParamPreset_Head;
				case apControlParam.ICON_PRESET.Body:
					return apImageSet.PRESET.ParamPreset_Body;
				case apControlParam.ICON_PRESET.Hand:
					return apImageSet.PRESET.ParamPreset_Hand;
				case apControlParam.ICON_PRESET.Face:
					return apImageSet.PRESET.ParamPreset_Face;
				case apControlParam.ICON_PRESET.Eye:
					return apImageSet.PRESET.ParamPreset_Eye;
				case apControlParam.ICON_PRESET.Hair:
					return apImageSet.PRESET.ParamPreset_Hair;
				case apControlParam.ICON_PRESET.Equipment:
					return apImageSet.PRESET.ParamPreset_Equip;
				case apControlParam.ICON_PRESET.Cloth:
					return apImageSet.PRESET.ParamPreset_Cloth;
				case apControlParam.ICON_PRESET.Force:
					return apImageSet.PRESET.ParamPreset_Force;
				case apControlParam.ICON_PRESET.Etc:
					return apImageSet.PRESET.ParamPreset_Etc;
			}
			return apImageSet.PRESET.ParamPreset_Etc;
		}

		public static apControlParam.ICON_PRESET GetControlParamPresetIconTypeByCategory(apControlParam.CATEGORY category)
		{
			switch (category)
			{
				case apControlParam.CATEGORY.Head:
					return apControlParam.ICON_PRESET.Head;
				case apControlParam.CATEGORY.Body:
					return apControlParam.ICON_PRESET.Body;
				case apControlParam.CATEGORY.Face:
					return apControlParam.ICON_PRESET.Face;
				case apControlParam.CATEGORY.Hair:
					return apControlParam.ICON_PRESET.Hair;
				case apControlParam.CATEGORY.Equipment:
					return apControlParam.ICON_PRESET.Equipment;
				case apControlParam.CATEGORY.Force:
					return apControlParam.ICON_PRESET.Force;
				case apControlParam.CATEGORY.Etc:
					return apControlParam.ICON_PRESET.Etc;
			}
			return apControlParam.ICON_PRESET.Etc;

		}


		public static apImageSet.PRESET GetSmallModIconType(apModifierBase.MODIFIER_TYPE modType)
		{
			switch (modType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:
					return apImageSet.PRESET.SmallMod_ControlLayer;

				case apModifierBase.MODIFIER_TYPE.Volume:
					return apImageSet.PRESET.SmallMod_ControlLayer;

				case apModifierBase.MODIFIER_TYPE.Morph:
					return apImageSet.PRESET.SmallMod_Morph;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					return apImageSet.PRESET.SmallMod_AnimMorph;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					return apImageSet.PRESET.SmallMod_Rigging;

				case apModifierBase.MODIFIER_TYPE.Physic:
					return apImageSet.PRESET.SmallMod_Physics;

				case apModifierBase.MODIFIER_TYPE.TF:
					return apImageSet.PRESET.SmallMod_TF;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					return apImageSet.PRESET.SmallMod_AnimTF;

				case apModifierBase.MODIFIER_TYPE.FFD:
					return apImageSet.PRESET.SmallMod_ControlLayer;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					return apImageSet.PRESET.SmallMod_ControlLayer;
			}
			return apImageSet.PRESET.Modifier_Volume;
		}
		//----------------------------------------------------------------------------------------------------

		public class NameAndIndexPair
		{
			public string _strName = "";
			public int _index = 0;
			public int _indexStrLength = 0;
			public NameAndIndexPair(string strName, string strIndex)
			{
				_strName = strName;
				if (strIndex.Length > 0)
				{
					_index = Int32.Parse(strIndex);
					_indexStrLength = strIndex.Length;
				}
				else
				{
					_index = 0;
					_indexStrLength = 0;
				}
			}
			public string MakeNewName(int index)
			{
				string strIndex = index + "";
				if (strIndex.Length < _indexStrLength)
				{
					int dLength = _indexStrLength - strIndex.Length;
					//0을 붙여주자
					for (int i = 0; i < dLength; i++)
					{
						strIndex = "0" + strIndex;
					}
				}

				return _strName + strIndex;
			}
		}

		public static NameAndIndexPair ParseNumericName(string srcName)
		{
			if (string.IsNullOrEmpty(srcName))
			{
				return new NameAndIndexPair("<None>", "");
			}

			//1. 이름 내에 "숫자로 된 부분"이 있다면, 그중 가장 "뒤의 숫자"를 1 올려서 갱신한다.
			string strName_First = "", strName_Index = "";
			int strMode = 1;//0 : First, 1 : Index
			for (int i = srcName.Length - 1; i >= 0; i--)
			{
				string curStr = srcName.Substring(i, 1);
				switch (strMode)
				{
					case 1:
						{
							if (IsNumericString(curStr))
							{
								strName_Index = curStr + strName_Index;
							}
							else
							{
								strName_First = curStr + strName_First;
								strMode = 0;
							}
						}
						break;

					case 0:
						strName_First = curStr + strName_First;
						break;
				}
			}
			return new NameAndIndexPair(strName_First, strName_Index);
		}


		private static bool IsNumericString(string str)
		{
			if (str == "0" || str == "1" || str == "2" ||
				str == "3" || str == "4" || str == "5" ||
				str == "6" || str == "7" || str == "8" ||
				str == "9")
			{
				return true;
			}
			return false;
		}


		//---------------------------------------------------------------------------------------
		public static T[] AddItemToArray<T>(T addItem, T[] srcArray)
		{
			if (srcArray == null || srcArray.Length == 0)
			{
				return new T[] { addItem };
			}

			int prevArraySize = srcArray.Length;
			int nextArraySize = prevArraySize + 1;

			T[] nextArray = new T[nextArraySize];
			for (int i = 0; i < prevArraySize; i++)
			{
				nextArray[i] = srcArray[i];
			}
			nextArray[nextArraySize - 1] = addItem;
			return nextArray;
		}

		//---------------------------------------------------------------------------------------
		private static string[] s_renderTextureNames = null;
		public static string[] GetRenderTextureSizeNames()
		{
		//	public enum RENDER_TEXTURE_SIZE
		//{
		//	s_64, s_128, s_256, s_512, s_1024
		//}
			if(s_renderTextureNames == null)
			{
				s_renderTextureNames = new string[] { "64", "128", "256", "512", "1024" };
			}
			return s_renderTextureNames;
		}

		//---------------------------------------------------------------------------------------
		//private static System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
		//private static string _stopWatchMsg = "";
		//public static void StartCodePerformanceCheck(string stopWatchMsg)
		//{
		//	_stopWatchMsg = stopWatchMsg;
		//	_stopwatch.Reset();
		//	_stopwatch.Start();
		//}

		//public static void StopCodePerformanceCheck()
		//{
		//	_stopwatch.Stop();
		//	long mSec = _stopwatch.ElapsedMilliseconds;
		//	Debug.LogError("Performance [" + _stopWatchMsg + "] : " + (mSec / 1000) + "." + (mSec % 1000) + " secs");
		//	//return _stopwatch.ElapsedTicks + " Ticks";
		//}

		//-------------------------------------------------------------------------------------------
		/// <summary>
		/// "Assets/AnyPortrait/Editor/Materials/"
		/// </summary>
		/// <returns></returns>
		public static string ResourcePath_Material
		{
			get
			{
				return "Assets/AnyPortrait/Editor/Materials/";
			}
		}

		/// <summary>
		/// "Assets/AnyPortrait/Editor/Scripts/Util/"
		/// </summary>
		public static string ResourcePath_Text
		{
			get
			{
				return "Assets/AnyPortrait/Editor/Scripts/Util/";
			}
		}

		/// <summary>
		/// "AnyPortrait/Editor/Scripts/Util/"
		/// </summary>
		public static string ResourcePath_Text_WithoutAssets
		{
			get
			{
				return "AnyPortrait/Editor/Scripts/Util/";
			}
		}

		/// <summary>
		/// "Assets/AnyPortrait/Editor/Images/"
		/// </summary>
		public static string ResourcePath_Icon
		{
			get
			{
				return "Assets/AnyPortrait/Editor/Images/";
			}
		}


		//--------------------------------------------------------------------------------
		public static int GetAspectRatio_Height(int srcWidth, int targetWidth, int targetHeight)
		{
			float targetAspectRatio = (float)targetWidth / (float)targetHeight;
			//Aspect = W / H
			//W = H * Aspect
			//H = W / Aspect <<

			return (int)(((float)srcWidth / targetAspectRatio) + 0.5f);
		}

		public static int GetAspectRatio_Width(int srcHeight, int targetWidth, int targetHeight)
		{
			float targetAspectRatio = (float)targetWidth / (float)targetHeight;
			//Aspect = W / H
			//W = H * Aspect <<
			//H = W / Aspect

			return (int)(((float)srcHeight * targetAspectRatio) + 0.5f);
		}
	}
}