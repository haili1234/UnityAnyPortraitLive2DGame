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

	public class apDialog_Bake : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_Bake s_window = null;

		private apEditor _editor = null;
		private apPortrait _targetPortrait = null;
		//private object _loadKey = null;

		private string[] _colorSpaceNames = new string[] { "Gamma", "Linear" };

		private string[] _sortingLayerNames = null;
		private int[] _sortingLayerIDs = null;

		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apPortrait portrait)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_Bake), true, "Bake", true);
			apDialog_Bake curTool = curWindow as apDialog_Bake;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 350;
				int height = 350;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				s_window.Init(editor, portrait, loadKey);

				return loadKey;
			}
			else
			{
				return null;
			}
		}

		public static void CloseDialog()
		{
			if (s_window != null)
			{
				try
				{
					s_window.Close();
				}
				catch (Exception ex)
				{
					Debug.LogError("Close Exception : " + ex);
				}
				s_window = null;
			}
		}

		// Init
		//------------------------------------------------------------------
		public void Init(apEditor editor, apPortrait portrait, object loadKey)
		{
			_editor = editor;
			//_loadKey = loadKey;
			_targetPortrait = portrait;


		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _targetPortrait == null)
			{
				CloseDialog();
				return;
			}

			//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
			if (_editor != apEditor.CurrentEditor || _targetPortrait != apEditor.CurrentEditor._portrait)
			{
				CloseDialog();
				return;
			}

			//Sorting Layer를 추가하자
			if (_sortingLayerNames == null || _sortingLayerIDs == null)
			{
				_sortingLayerNames = new string[SortingLayer.layers.Length];
				_sortingLayerIDs = new int[SortingLayer.layers.Length];
			}
			else if (_sortingLayerNames.Length != SortingLayer.layers.Length
				|| _sortingLayerIDs.Length != SortingLayer.layers.Length)
			{
				_sortingLayerNames = new string[SortingLayer.layers.Length];
				_sortingLayerIDs = new int[SortingLayer.layers.Length];
			}

			for (int i = 0; i < SortingLayer.layers.Length; i++)
			{
				_sortingLayerNames[i] = SortingLayer.layers[i].name;
				_sortingLayerIDs[i] = SortingLayer.layers[i].id;
			}


			//Bake 설정
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_BakeSetting));//"Bake Setting"
			GUILayout.Space(5);

			EditorGUILayout.ObjectField(_editor.GetText(TEXT.DLG_Portrait), _targetPortrait, typeof(apPortrait), true);//"Portait"

			GUILayout.Space(5);

			//"Bake Scale"
			float prevBakeScale = _targetPortrait._bakeScale;
			_targetPortrait._bakeScale = EditorGUILayout.FloatField(_editor.GetText(TEXT.DLG_BakeScale), _targetPortrait._bakeScale);



			//"Z Per Depth"
			float prevBakeZSize = _targetPortrait._bakeZSize;
			_targetPortrait._bakeZSize = EditorGUILayout.FloatField(_editor.GetText(TEXT.DLG_ZPerDepth), _targetPortrait._bakeZSize);


			if (_targetPortrait._bakeZSize < 0.5f)
			{
				_targetPortrait._bakeZSize = 0.5f;
			}

			GUILayout.Space(5);



			//Gamma Space Space			
			bool prevBakeGamma = _editor._isBakeColorSpaceToGamma;
			int iPrevColorSpace = prevBakeGamma ? 0 : 1;
			int iNextColorSpace = EditorGUILayout.Popup(_editor.GetUIWord(UIWORD.ColorSpace), iPrevColorSpace, _colorSpaceNames);
			if (iNextColorSpace != iPrevColorSpace)
			{
				if (iNextColorSpace == 0)
				{
					//Gamma
					_editor._isBakeColorSpaceToGamma = true;
				}
				else
				{
					//Linear
					_editor._isBakeColorSpaceToGamma = false;
				}
			}
			//_editor._isBakeColorSpaceToGamma = EditorGUILayout.Toggle("Gamma Color Space", _editor._isBakeColorSpaceToGamma);

			//GUILayout.Space(5);
			//float nextPhysicsScale = EditorGUILayout.DelayedFloatField("Physic Scale", _targetPortrait._physicBakeScale);

			GUILayout.Space(5);

			//Sorting Layer
			int prevSortingLayerID = _editor._portrait._sortingLayerID;
			int prevSortingOrder = _editor._portrait._sortingOrder;

			int layerIndex = -1;
			for (int i = 0; i < SortingLayer.layers.Length; i++)
			{
				if (SortingLayer.layers[i].id == _editor._portrait._sortingLayerID)
				{
					//찾았다.
					layerIndex = i;
					break;
				}
			}
			if (layerIndex < 0)
			{
				//어라 레이어가 없는데용..
				//초기화해야겠다.
				_editor._portrait._sortingLayerID = -1;
				if (SortingLayer.layers.Length > 0)
				{
					_editor._portrait._sortingLayerID = SortingLayer.layers[0].id;
					layerIndex = 0;
				}
			}
			int nextIndex = EditorGUILayout.Popup("Sorting Layer", layerIndex, _sortingLayerNames);
			if (nextIndex != layerIndex)
			{
				//레이어가 변경되었다.
				if (nextIndex >= 0 && nextIndex < SortingLayer.layers.Length)
				{
					//LayerID 변경
					_editor._portrait._sortingLayerID = SortingLayer.layers[nextIndex].id;
				}
			}
			_editor._portrait._sortingOrder = EditorGUILayout.IntField("Sorting Order", _editor._portrait._sortingOrder);




			//CheckChangedProperties(nextRootScale, nextZScale);
			if (prevBakeScale != _targetPortrait._bakeScale ||
				prevBakeZSize != _targetPortrait._bakeZSize ||
				prevSortingLayerID != _editor._portrait._sortingLayerID ||
				prevSortingOrder != _editor._portrait._sortingOrder)
			{
				apEditorUtil.SetEditorDirty();
			}

			if (prevBakeGamma != _editor._isBakeColorSpaceToGamma
				
				)
			{
				apEditorUtil.SetEditorDirty();
				_editor.SaveEditorPref();
			}

			GUILayout.Space(10);


			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Bake), GUILayout.Height(45)))//"Bake"
			{
				GUI.FocusControl(null);

				//CheckChangedProperties(nextRootScale, nextZScale);
				apEditorUtil.SetEditorDirty();

				//-------------------------------------
				// Bake 함수를 실행한다. << 중요오오오오
				//-------------------------------------
				apBakeResult bakeResult = _editor.Controller.Bake();


				_editor.Notification("[" + _targetPortrait.name + "] is Baked", false, false);

				if(bakeResult.NumUnlinkedExternalObject > 0)
				{
					EditorUtility.DisplayDialog(_editor.GetText(TEXT.BakeWarning_Title),
						_editor.GetTextFormat(TEXT.BakeWarning_Body, bakeResult.NumUnlinkedExternalObject),
						_editor.GetText(TEXT.Okay));
				}
			}

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width - 10);
			GUILayout.Space(10);

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_OptimizedBaking));//"Optimized Baking"

			//"Target"
			apPortrait nextOptPortrait = (apPortrait)EditorGUILayout.ObjectField(_editor.GetText(TEXT.DLG_Target), _targetPortrait._bakeTargetOptPortrait, typeof(apPortrait), true);

			if(nextOptPortrait != _targetPortrait._bakeTargetOptPortrait)
			{
				//타겟을 바꾸었다.
				bool isChanged = false;
				if (nextOptPortrait != null)
				{
					//1. 다른 Portrait를 선택했다.
					if (!nextOptPortrait._isOptimizedPortrait)
					{
						//1-1. 최적화된 객체가 아니다.
						EditorUtility.DisplayDialog(_editor.GetText(TEXT.OptBakeError_Title),
													_editor.GetText(TEXT.OptBakeError_NotOptTarget_Body),
													_editor.GetText(TEXT.Close));
					}
					else if(nextOptPortrait._bakeSrcEditablePortrait != _targetPortrait)
					{
						//1-2. 다른 대상으로부터 Bake된 Portrait같다. (물어보고 계속)
						bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.OptBakeError_Title),
													_editor.GetText(TEXT.OptBakeError_SrcMatchError_Body),
													_editor.GetText(TEXT.Okay),
													_editor.GetText(TEXT.Cancel));

						if(isResult)
						{
							//뭐 선택하겠다는데요 뭐..
							isChanged = true;
							
						}
					}
					else
					{
						//1-3. 오케이. 변경 가능
						isChanged = true;
					}
				}
				else
				{
					//2. 선택을 해제했다.
					isChanged = true;
				}
				
				if(isChanged)
				{
					//Target을 변경한다.
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
					_targetPortrait._bakeTargetOptPortrait = nextOptPortrait;
				}
				
			}

			string optBtnText = "";
			if(_targetPortrait._bakeTargetOptPortrait != null)
			{
				//optBtnText = "Optimized Bake to\n[" + _targetPortrait._bakeTargetOptPortrait.gameObject.name + "]";
				optBtnText = string.Format("{0}\n[{1}]", _editor.GetText(TEXT.DLG_OptimizedBakeTo), _targetPortrait._bakeTargetOptPortrait.gameObject.name);
			}
			else
			{
				//optBtnText = "Optimized Bake\n(Make New GameObject)";
				optBtnText = _editor.GetText(TEXT.DLG_OptimizedBakeMakeNew);
			}
			GUILayout.Space(10);

			if (GUILayout.Button(optBtnText, GUILayout.Height(45)))
			{
				GUI.FocusControl(null);

				//CheckChangedProperties(nextRootScale, nextZScale);

				//Optimized Bake를 하자
				apBakeResult bakeResult = _editor.Controller.OptimizedBake(_targetPortrait, _targetPortrait._bakeTargetOptPortrait);

				if(bakeResult.NumUnlinkedExternalObject > 0)
				{
					EditorUtility.DisplayDialog(_editor.GetText(TEXT.BakeWarning_Title),
						_editor.GetTextFormat(TEXT.BakeWarning_Body, bakeResult.NumUnlinkedExternalObject),
						_editor.GetText(TEXT.Okay));
				}

				_editor.Notification("[" + _targetPortrait.name + "] is Baked (Optimized)", false, false);
			}


		}

		//private void CheckChangedProperties(float nextRootScale, float nextZScale)
		//{
		//	bool isChanged = false;
		//	if (nextRootScale != _targetPortrait._bakeScale
		//		|| nextZScale != _targetPortrait._bakeZSize)
		//	{
		//		isChanged = true;
		//	}

		//	if (isChanged)
		//	{
		//		apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_BakeOptionChanged, _editor, _targetPortrait, null, false);

		//		if (nextRootScale < 0.0001f)
		//		{
		//			nextRootScale = 0.0001f;
		//		}
		//		if(nextZScale < 0.5f)
		//		{
		//			nextZScale = 0.5f;
		//		}

		//		_targetPortrait._bakeScale = nextRootScale;
		//		_targetPortrait._bakeZSize = nextZScale;

				

		//		//if(nextPhysicsScale < 0.0f)
		//		//{
		//		//	nextPhysicsScale = 0.0f;
		//		//}
		//		//_targetPortrait._physicBakeScale = nextPhysicsScale;

		//		GUI.FocusControl(null);
		//	}
		//}
	}

}