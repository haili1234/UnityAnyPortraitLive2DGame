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
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
//using UnityEngine.Profiling;

using AnyPortrait;

namespace AnyPortrait
{

	[CustomEditor(typeof(apPortrait))]
	public class apInspector_Portrait : Editor
	{
		private apPortrait _targetPortrait = null;
		private apControlParam.CATEGORY _curControlCategory = apControlParam.CATEGORY.Etc;
		private bool _showBaseInspector = false;
		private List<apControlParam> _controlParams = null;


		private bool _isFold_RootPortraits = false;
		private bool _isFold_AnimationClips = false;
		private bool _isFold_ConrolParameters = false;
		



		void OnEnable()
		{
			_targetPortrait = null;

			_isFold_RootPortraits = true;
			_isFold_AnimationClips = true;
			_isFold_ConrolParameters = true;
		}

		public override void OnInspectorGUI()
		{
			//return;

			//base.OnInspectorGUI();
			apPortrait targetPortrait = target as apPortrait;

			if (targetPortrait != _targetPortrait)
			{
				_targetPortrait = targetPortrait;
				Init();
			}
			if (_targetPortrait == null)
			{
				//Profiler.EndSample();
				return;
			}

			//Profiler.BeginSample("anyPortrait Inspector GUI");


			//return;
			if (apEditor.IsOpen())
			{
				//에디터가 작동중에는 안보이도록 하자
				EditorGUILayout.LabelField("Editor is opened");

				//Profiler.EndSample();

				return;
			}

			try
			{
				bool prevImportant = _targetPortrait._isImportant;
				MonoBehaviour prevAnimEventListener = _targetPortrait._optAnimEventListener;
				int prevSortingLayerID = _targetPortrait._sortingLayerID;
				int prevSortingOrder = _targetPortrait._sortingOrder;

				_targetPortrait._isImportant = EditorGUILayout.Toggle("Is Important", _targetPortrait._isImportant);
				_targetPortrait._optAnimEventListener = (MonoBehaviour)EditorGUILayout.ObjectField("Event Listener", _targetPortrait._optAnimEventListener, typeof(MonoBehaviour), true);


				GUILayout.Space(5);
				//추가3.22
				//Sorting Layer
				string[] sortingLayerName = new string[SortingLayer.layers.Length];
				int layerIndex = -1;
				for (int i = 0; i < SortingLayer.layers.Length; i++)
				{
					sortingLayerName[i] = SortingLayer.layers[i].name;
					if (SortingLayer.layers[i].id == _targetPortrait._sortingLayerID)
					{
						layerIndex = i;
					}
				}
				int nextLayerIndex = EditorGUILayout.Popup("Sorting Layer", layerIndex, sortingLayerName);
				int nextLayerOrder = EditorGUILayout.IntField("Sorting Order", _targetPortrait._sortingOrder);

				if(nextLayerIndex != layerIndex)
				{
					//Sorting Layer를 바꾸자
					if(nextLayerIndex >= 0 && nextLayerIndex < SortingLayer.layers.Length)
					{
						string nextLayerName = SortingLayer.layers[nextLayerIndex].name;
						_targetPortrait.SetSortingLayer(nextLayerName);
					}
				}
				if(nextLayerOrder != _targetPortrait._sortingOrder)
				{
					_targetPortrait.SetSortingOrder(nextLayerOrder);
				}


				if(prevImportant != _targetPortrait._isImportant ||
					prevAnimEventListener != _targetPortrait._optAnimEventListener ||
					prevSortingLayerID != _targetPortrait._sortingLayerID ||
					prevSortingOrder != _targetPortrait._sortingOrder)
				{
					apEditorUtil.SetEditorDirty();
				}

				GUILayout.Space(5);
				
				_isFold_RootPortraits = EditorGUILayout.Foldout(_isFold_RootPortraits, "Root Portraits");
				if(_isFold_RootPortraits)
				{
					string strRootPortrait = "";
					if(_targetPortrait._optRootUnitList.Count == 0)
					{
						strRootPortrait = "No Baked Portrait";
					}
					else if(_targetPortrait._optRootUnitList.Count == 1)
					{
						strRootPortrait = "1 Baked Portrait";
					}
					else
					{
						strRootPortrait = _targetPortrait._optRootUnitList.Count + " Baked Portraits";
					}
					EditorGUILayout.LabelField(strRootPortrait);
					GUILayout.Space(5);
					for (int i = 0; i < _targetPortrait._optRootUnitList.Count; i++)
					{
						apOptRootUnit rootUnit = _targetPortrait._optRootUnitList[i];
						EditorGUILayout.ObjectField("[" + i + "]", rootUnit, typeof(apOptRootUnit), true);
					}

					GUILayout.Space(20);
				}


				
				

				_isFold_AnimationClips = EditorGUILayout.Foldout(_isFold_AnimationClips, "Animation Clips");
				if(_isFold_AnimationClips)
				{
					string strAnimClips = "";
					if(_targetPortrait._animClips.Count == 0)
					{
						strAnimClips = "No Animation Clip";
					}
					else if(_targetPortrait._animClips.Count == 1)
					{
						strAnimClips = "1 Animation Clip";
					}
					else
					{
						strAnimClips = _targetPortrait._animClips.Count + " Animation Clips";
					}
					EditorGUILayout.LabelField(strAnimClips);
					GUILayout.Space(5);
					for (int i = 0; i < _targetPortrait._animClips.Count; i++)
					{
						apAnimClip animClip = _targetPortrait._animClips[i];
						if(animClip._uniqueID == _targetPortrait._autoPlayAnimClipID)
						{
							EditorGUILayout.TextField("[" + i + "] (Auto Play)", animClip._name);
						}
						else
						{
							EditorGUILayout.TextField("[" + i + "]", animClip._name);
						}
						
					}

					GUILayout.Space(20);

				}

				
				bool isChanged = false;

				_isFold_ConrolParameters = EditorGUILayout.Foldout(_isFold_ConrolParameters, "Control Parameters");
				if (_isFold_ConrolParameters)
				{
#if UNITY_2017_3_OR_NEWER
					_curControlCategory = (apControlParam.CATEGORY)EditorGUILayout.EnumFlagsField(new GUIContent("Category"), _curControlCategory);
#else				
					_curControlCategory = (apControlParam.CATEGORY)EditorGUILayout.EnumMaskPopup(new GUIContent("Category"), _curControlCategory);
#endif

					EditorGUILayout.Space();
					//1. 컨르롤러를 제어할 수 있도록 하자
					
					if (_controlParams != null)
					{
						for (int i = 0; i < _controlParams.Count; i++)
						{
							if ((int)(_controlParams[i]._category & _curControlCategory) != 0)
							{
								if (GUI_ControlParam(_controlParams[i]))
								{
									isChanged = true;
								}
							}
						}
					}

					GUILayout.Space(20);
				}
				

				GUILayout.Space(10);

				//2. 토글 버튼을 두어서 기본 Inspector 출력 여부를 결정하자.
				string strBaseButton = "Show All Properties";
				if (_showBaseInspector)
				{
					strBaseButton = "Hide Properties";
				}

				if (GUILayout.Button(strBaseButton, GUILayout.Height(20)))
				{
					_showBaseInspector = !_showBaseInspector;
				}

				if (_showBaseInspector)
				{
					base.OnInspectorGUI();
				}


				if (!Application.isPlaying && isChanged)
				{
					//플레이 중이라면 자동으로 업데이트 될 것이다.
					_targetPortrait.UpdateForce();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("apInspector_Portrait Exception : " + ex);
			}

			//Profiler.EndSample();
		}

		private void Init()
		{
			_curControlCategory = apControlParam.CATEGORY.Head |
									apControlParam.CATEGORY.Body |
									apControlParam.CATEGORY.Face |
									apControlParam.CATEGORY.Hair |
									apControlParam.CATEGORY.Equipment |
									apControlParam.CATEGORY.Force |
									apControlParam.CATEGORY.Etc;

			_showBaseInspector = false;

			//_isFold_BasicSettings = true;
			_isFold_RootPortraits = true;
			//_isFold_AnimationSettings = true;
			_isFold_AnimationClips = true;
			_isFold_ConrolParameters = true;

			_controlParams = null;
			if (_targetPortrait._controller != null)
			{
				_controlParams = _targetPortrait._controller._controlParams;
			}
		}

		private bool GUI_ControlParam(apControlParam controlParam)
		{
			if (controlParam == null)
			{ return false; }

			bool isChanged = false;

			EditorGUILayout.LabelField(controlParam._keyName);

			switch (controlParam._valueType)
			{
				//case apControlParam.TYPE.Bool:
				//	{
				//		bool bPrev = controlParam._bool_Cur;
				//		controlParam._bool_Cur = EditorGUILayout.Toggle(controlParam._bool_Cur);
				//		if(bPrev != controlParam._bool_Cur)
				//		{
				//			isChanged = true;
				//		}
				//	}
				//	break;

				case apControlParam.TYPE.Int:
					{
						int iPrev = controlParam._int_Cur;
						controlParam._int_Cur = EditorGUILayout.IntSlider(controlParam._int_Cur, controlParam._int_Min, controlParam._int_Max);

						if (iPrev != controlParam._int_Cur)
						{
							isChanged = true;
						}
					}
					break;

				case apControlParam.TYPE.Float:
					{
						float fPrev = controlParam._float_Cur;
						controlParam._float_Cur = EditorGUILayout.Slider(controlParam._float_Cur, controlParam._float_Min, controlParam._float_Max);

						if (Mathf.Abs(fPrev - controlParam._float_Cur) > 0.0001f)
						{
							isChanged = true;
						}
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						Vector2 v2Prev = controlParam._vec2_Cur;
						controlParam._vec2_Cur.x = EditorGUILayout.Slider(controlParam._vec2_Cur.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
						controlParam._vec2_Cur.y = EditorGUILayout.Slider(controlParam._vec2_Cur.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);

						if (Mathf.Abs(v2Prev.x - controlParam._vec2_Cur.x) > 0.0001f ||
							Mathf.Abs(v2Prev.y - controlParam._vec2_Cur.y) > 0.0001f)
						{
							isChanged = true;
						}
					}
					break;

			}

			GUILayout.Space(5);

			return isChanged;
		}
	}

}