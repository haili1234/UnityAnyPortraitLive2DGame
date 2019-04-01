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

#if UNITY_EDITOR
using System.Diagnostics;//자체 타이머를 만들자
#endif


using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// It is an animation clip created in the editor.
	/// It has the data to play the animation and connects to apAnimPlayUnit to perform the update.
	/// (Since most functions are used in the editor or used in manger classes, it is recommended that you refer to "apAnimPlayUnit" instead of using this class.)
	/// </summary>
	[Serializable]
	public class apAnimClip
	{
		// Members
		//---------------------------------------------
		[SerializeField]
		public int _uniqueID = -1;

		[SerializeField]
		public string _name = "";


		//연결된 객체들
		[NonSerialized]
		public apPortrait _portrait = null;


		//연결된 객체들 - 에디터
		[SerializeField]
		public int _targetMeshGroupID = -1;

		[NonSerialized]
		public apMeshGroup _targetMeshGroup = null;

		[NonSerialized]
		public apOptTransform _targetOptTranform = null;


		public enum LINK_TYPE
		{
			AnimatedModifier = 0,
			//Bone,//<<이게 빠지고 AnimatedModifier에 포함된다. Transform(Animation) Modifier에서 Bone 제어가 가능하다
			ControlParam = 1
		}

		// 애니메이션 기본 정보
		[SerializeField]
		private int _FPS = 30;

		[SerializeField]
		private float _secPerFrame = 1.0f / 30.0f;

		[SerializeField]
		private int _startFrame = 0;
		[SerializeField]
		private int _endFrame = 100;
		[SerializeField]
		private bool _isLoop = false;

		public int FPS { get { return _FPS; } }
		public int StartFrame { get { return _startFrame; } }
		public int EndFrame { get { return _endFrame; } }
		public bool IsLoop { get { return _isLoop; } }



		//Timeline 리스트
		[SerializeField]
		public List<apAnimTimeline> _timelines = new List<apAnimTimeline>();


		private float _tUpdate = 0.0f;
		private float _tUpdateTotal = 0.0f;
		private int _curFrame = 0;
		public float TimePerFrame { get { return _secPerFrame; } }
		public float TotalPlayTime { get { return _tUpdateTotal; } }

		private bool _isPlaying = false;

		/// <summary>에디터에서 적용하는 [현재 프레임]</summary>
		public int CurFrame { get { return _curFrame; } }

		/// <summary>
		/// 실행시 정확한 보간이 되는 프레임 (실수형)
		/// 게임 프레임에 동기화된다. (정확한 정수형 프레임 값은 안나온다)
		/// </summary>
		public float CurFrameFloat { get { return _curFrame + (_tUpdate / TimePerFrame); } }

		public bool IsPlaying_Editor { get { return _isPlaying; } }
		public bool IsPlaying_Opt
		{
			get
			{
				//return _isPlaying;
				if(_parentPlayUnit == null)
				{
					return false;
				}
				return _parentPlayUnit.PlayStatus == apAnimPlayUnit.PLAY_STATUS.Play && !_parentPlayUnit._isPause;
			}
		}

		public float TimeLength { get { return (float)Mathf.Max(_endFrame - _startFrame, 0) * TimePerFrame; } }

		///// <summary>
		///// 재생된 결과가 반영되는 가중치값
		///// 단순 재생시에는 이 값이 1이지만 Layer, Queue 플레이시 Weight가 바뀌며, 그 값에 따라 데이터가 적용된다.
		///// </summary>
		//private float _playWeight = 1.0f;


		[SerializeField]
		public List<apAnimEvent> _animEvents = new List<apAnimEvent>();

		[NonSerialized]
		public List<apAnimControlParamResult> _controlParamResult = new List<apAnimControlParamResult>();


		//리얼타임에서
		/// <summary>
		/// 리얼타임에서 이 AnimClip을 사용중인 PlayUnit. 이 값은 PlayUnit 생성마다 갱신되며 소유권을 알려준다.
		/// </summary>
		[NonSerialized]
		public apAnimPlayUnit _parentPlayUnit = null;


		//에디터에서
		[NonSerialized]
		public bool _isSelectedInEditor = false;

#if UNITY_EDITOR
		private Stopwatch _stopWatch_Editor = new Stopwatch();
		private float _tDelta_Editor = -1;
#endif

		


		// Init
		//---------------------------------------------
		public apAnimClip()
		{

		}


		public void Init(apPortrait portrait, string name, int ID)
		{
			_portrait = portrait;
			_name = name;
			_uniqueID = ID;
			_targetMeshGroupID = -1;
			_targetMeshGroup = null;

			_timelines.Clear();
			_controlParamResult.Clear();

		}

		public void LinkEditor(apPortrait portrait)
		{
			_portrait = portrait;
			_targetMeshGroup = _portrait.GetMeshGroup(_targetMeshGroupID);


			//ID를 등록해주자
			//_portrait.RegistUniqueID_AnimClip(_uniqueID);
			_portrait.RegistUniqueID(apIDManager.TARGET.AnimClip, _uniqueID);

			//TODO : 멤버 추가시 값을 추가하자
			for (int i = 0; i < _timelines.Count; i++)
			{
				_timelines[i].Link(this);
			}
		}


		public void RemoveUnlinkedTimeline()
		{
			_timelines.RemoveAll(delegate (apAnimTimeline a)
			{
				if (a._linkType == LINK_TYPE.AnimatedModifier)
				{
					if (a._modifierUniqueID < 0)
					{
						return true;
					}
				}
				return false;

			});

			for (int i = 0; i < _timelines.Count; i++)
			{
				_timelines[i].RemoveUnlinkedLayer();
			}
		}

		public void LinkOpt(apPortrait portrait)
		{
			_portrait = portrait;

			
			if(_targetMeshGroupID < 0)
			{
				//연결되지 않았네요.
				UnityEngine.Debug.LogError("No MeshGroup Linked");
				return;
			}


			_targetOptTranform = _portrait.GetOptTransformAsMeshGroup(_targetMeshGroupID);


			if (_targetOptTranform == null)
			{
				//UnityEngine.Debug.LogError("AnimClip이 적용되는 Target Opt Transform이 Null이다. [" + _targetMeshGroupID + "] (" + _name + ")");
				//이 AnimClip을 사용하지 맙시다.
				return;
				
			}

			
			for (int i = 0; i < _timelines.Count; i++)
			{
				_timelines[i].LinkOpt(this);
			}
		}




		// Update / 플레이 제어
		//---------------------------------------------
#if UNITY_EDITOR
		private DateTime _sampleDateTime = new DateTime();
#endif

		/// <summary>
		/// [Editor] 업데이트를 한다.
		/// FPS에 맞게 프레임을 증가시킨다.
		/// MeshGroup은 자동으로 재생시킨다.
		/// (재생중이 아닐때는 MeshGroup 자체의 FPS에 맞추어 업데이트를 한다.)
		/// </summary>
		/// <param name="tDelta"></param>
		/// <param name="isUpdateVertsAlways">단순 재생에는 False, 작업시에는 True로 설정</param>
		public void Update_Editor(float tDelta, bool isUpdateVertsAlways)
		{


#if UNITY_EDITOR
			//시간을 따로 계산하자
			float multiply = 1.0f;
			if (_tDelta_Editor < 0)
			{
				_stopWatch_Editor.Start();
				_tDelta_Editor = tDelta;
				_sampleDateTime = DateTime.Now;

			}
			else
			{
				_stopWatch_Editor.Stop();
				_tDelta_Editor = (float)(_stopWatch_Editor.ElapsedMilliseconds / 1000.0);

				if (_tDelta_Editor > 0)
				{
					float dateTimeMillSec = (float)(DateTime.Now.Subtract(_sampleDateTime).TotalMilliseconds / 1000.0);
					multiply = dateTimeMillSec / _tDelta_Editor;
					//UnityEngine.Debug.Log("Update Anim / StopWatch : " + _tDelta_Editor + " / DateTime Span : " + dateTimeMillSec + " / Mul : " + multiply);
				}
				_sampleDateTime = DateTime.Now;

				_stopWatch_Editor.Reset();
				_stopWatch_Editor.Start();
			}
			tDelta = _tDelta_Editor * multiply;




#endif

			if (!_isPlaying)
			{
				UpdateMeshGroup_Editor(false, tDelta, isUpdateVertsAlways);//<<강제로 업데이트 하지 않는다.
				return;
			}


			_tUpdate += tDelta;
			if (_tUpdate >= TimePerFrame)
			{
				_curFrame++;
				//_tUpdate -= TimePerFrame;

				if (_curFrame >= _endFrame)
				{
					if (_isLoop)
					{
						//루프가 되었당
						//루프는 endframe을 찍지 않고, 바로 startFrame으로 가야한다.
						_curFrame = _startFrame;
					}
					else
					{
						//endframe에서 정지
						_curFrame = _endFrame;
						_isPlaying = false;
						//Debug.Log("Stop In Last Frame");
					}
				}


				//Debug.Log("Update AnimClip : " + _name);

				//1. Control Param을 먼저 업데이트를 하고 [Control Param]
				UpdateControlParam(true);


				//2. Mesh를 업데이트한다. [Animated Modifier + Bone]
				//UpdateMeshGroup_Editor(true, _tUpdate, isUpdateVertsAlways);//강제로 업데이트하자
				//UpdateMeshGroup_Editor(false, tDelta, isUpdateVertsAlways);//일반 업데이트

				//Debug.Log("Anim Update : " + (int)(1.0f / _tUpdate) + "FPS");

				_tUpdate -= TimePerFrame;
				
			}

			UpdateMeshGroup_Editor(true, tDelta, isUpdateVertsAlways);
		}

		/// <summary>
		/// [Editor] 플레이를 정지한다.
		/// 첫 프레임으로 자동으로 돌아간다.
		/// </summary>
		public void Stop_Editor(bool isRefreshMeshAndControlParam = true)
		{
			_isPlaying = false;
			_tUpdate = 0.0f;
			_curFrame = _startFrame;//<첫 프레임으로 돌아간다.

			if (isRefreshMeshAndControlParam)
			{
				UpdateControlParam(true);
				UpdateMeshGroup_Editor(true, 0.0f, true);//강제로 업데이트하자
			}
		}

		/// <summary>
		/// [Editor] 플레이를 일시중지한다.
		/// 프레임은 현재 위치에서 정지
		/// </summary>
		public void Pause_Editor()
		{
			_isPlaying = false;
			_curFrame = Mathf.Clamp(_curFrame, _startFrame, _endFrame);

			UpdateControlParam(true);
			UpdateMeshGroup_Editor(true, 0.0f, true);//강제로 업데이트하자
		}

		/// <summary>
		/// [Editor] 애니메이션을 재생한다.
		/// </summary>
		/// <param name="isResetFrame">True면 첫 프레임으로 돌려서 시작한다. False면 현재 프레임에서 재개</param>
		public void Play_Editor(bool isResetFrame = false)
		{
			_isPlaying = true;
			_tUpdate = 0.0f;
			_curFrame = Mathf.Clamp(_curFrame, _startFrame, _endFrame);

			UpdateControlParam(true);
			UpdateMeshGroup_Editor(true, 0.0f, true);//강제로 업데이트하자
		}

		/// <summary>
		/// [Editor] 프레임을 지정한다. (자동으로 메시 업데이트가 된다)
		/// </summary>
		/// <param name="frame"></param>
		public void SetFrame_Editor(int frame)
		{
			_curFrame = Mathf.Clamp(frame, _startFrame, _endFrame);
			_isPlaying = false;//<<Set Frame시에는 자동으로 Pause한다.
			_tUpdate = 0.0f;

			UpdateControlParam(true);
			UpdateMeshGroup_Editor(true, 0.0f, true);//강제로 업데이트하자
		}

		/// <summary>
		/// [Editor] 프레임을 지정한다. Min-Max를 가리지 않고,
		/// 재생 여부에 제한을 두지 않는다.
		/// </summary>
		/// <param name="frame"></param>
		public void SetFrame_EditorNotStop(int frame)
		{
			_curFrame = frame;
			
			UpdateControlParam(true);
			UpdateMeshGroup_Editor(true, 0.0f, true);//강제로 업데이트하자
		}


		/// <summary>
		/// [Editor] 업데이트 중 Control Param 제어 Timeline에 대해 업데이트 후 적용을 한다.
		/// [Runtime] isAdaptToWeight = false로 두고 나머지 처리를 한다.
		/// </summary>
		/// <param name="isAdaptToWeight1">[Editor]에서 Weight=1로 두고 적용을 한다</param>
		public void UpdateControlParam(bool isAdaptToWeight1, int optLayer = 0, float optWeight = 1.0f, apAnimPlayUnit.BLEND_METHOD optBlendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation)
		{
			if (_controlParamResult.Count == 0)
			{
				return;
			}

			for (int i = 0; i < _controlParamResult.Count; i++)
			{
				_controlParamResult[i].Init();
			}

			apAnimTimeline timeline = null;
			apAnimTimelineLayer layer = null;

			int curFrame = CurFrame;
			float curFrameF = CurFrameFloat;

			//apAnimKeyframe firstKeyframe = null;
			//apAnimKeyframe lastKeyframe = null;

			apAnimKeyframe curKeyframe = null;
			apAnimKeyframe prevKeyframe = null;
			apAnimKeyframe nextKeyframe = null;

			int lengthFrames = _endFrame - _startFrame;
			int tmpCurFrame = 0;

			apAnimControlParamResult cpResult = null;

			for (int iTL = 0; iTL < _timelines.Count; iTL++)
			{
				timeline = _timelines[iTL];
				if (timeline._linkType != LINK_TYPE.ControlParam)
				{
					continue;
				}

				for (int iL = 0; iL < timeline._layers.Count; iL++)
				{
					layer = timeline._layers[iL];
					if (layer._linkedControlParam == null || layer._linkedControlParamResult == null)
					{
						continue;
					}

					cpResult = layer._linkedControlParamResult;

					//firstKeyframe = layer._firstKeyFrame;
					//lastKeyframe = layer._lastKeyFrame;

					for (int iK = 0; iK < layer._keyframes.Count; iK++)
					{
						curKeyframe = layer._keyframes[iK];
						prevKeyframe = curKeyframe._prevLinkedKeyframe;
						nextKeyframe = curKeyframe._nextLinkedKeyframe;

						if (curFrame == curKeyframe._frameIndex ||
							((curKeyframe._isLoopAsStart || curKeyframe._isLoopAsEnd) && curKeyframe._loopFrameIndex == curFrame)
							)
						{
							cpResult.SetKeyframeResult(curKeyframe, 1.0f);
						}
						else if (curKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Prev))
						{
							//Prev - Cur 범위 안에 들었다.
							if (prevKeyframe != null)
							{
								//Prev 키가 있다면
								tmpCurFrame = curFrame;
								if (tmpCurFrame > curKeyframe._frameIndex)
								{
									//한바퀴 돌았다면
									tmpCurFrame -= lengthFrames;
								}

								//TODO : 여길 나중에 "정식 Curve로 변경"할 것 
								//float itp = apAnimCurve.GetCurvedRelativeInterpolation(curKeyframe._curveKey, prevKeyframe._curveKey, tmpCurFrame, true);

								//>> 변경
								float itp = curKeyframe._curveKey.GetItp_Int(tmpCurFrame, true);

								cpResult.SetKeyframeResult(curKeyframe, itp);
							}
							else
							{
								//Prev 키가 없다면 이게 100%다
								cpResult.SetKeyframeResult(curKeyframe, 1.0f);
							}
						}
						else if (curKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Next))
						{
							//Cur - Next 범위 안에 들었다.
							if (nextKeyframe != null)
							{
								//Next 키가 있다면
								tmpCurFrame = curFrame;
								if (tmpCurFrame < curKeyframe._frameIndex)
								{
									//한바퀴 돌았다면
									tmpCurFrame += lengthFrames;
								}

								//TODO : 여길 나중에 "정식 Curve로 변경"할 것 
								//float itp = apAnimCurve.GetCurvedRelativeInterpolation(curKeyframe._curveKey, nextKeyframe._curveKey, tmpCurFrame, false);

								//>> 변경
								float itp = curKeyframe._curveKey.GetItp_Int(tmpCurFrame, false);

								cpResult.SetKeyframeResult(curKeyframe, itp);
							}
							else
							{
								//Prev 키가 없다면 이게 100%다
								cpResult.SetKeyframeResult(curKeyframe, 1.0f);
							}
						}
					}
				}
			}


			//Control Param에 적용을 해야한다.
			if (isAdaptToWeight1)
			{
				//Editor인 경우 Weight 1로 강제한다.
				for (int i = 0; i < _controlParamResult.Count; i++)
				{
					_controlParamResult[i].AdaptToControlParam();
				}
			}
			else
			{
				//Runtime인 경우 지정된 Weight, Layer로 처리한다.
				for (int i = 0; i < _controlParamResult.Count; i++)
				{
					//Debug.Log("AnimClip [" + _name + " > Control Param : " + _controlParamResult[i]._targetControlParam._keyName + " ]");
					_controlParamResult[i].AdaptToControlParam_Opt(optWeight, optBlendMethod);
				}

			}
		}


		/// <summary>
		/// [Editor] 모든 애니메이션 처리를 포함한 MeshGroup 업데이트를 한다.
		/// </summary>
		/// <param name="isForce"></param>
		/// <param name="tDelta"></param>
		/// <param name="isUpdateVertsAlways">단순 재생시에는 False, 작업시에는 True로 설정한다.</param>
		public void UpdateMeshGroup_Editor(bool isForce, float tDelta, bool isUpdateVertsAlways, bool isDepthChanged = false)
		{
			if (_targetMeshGroup == null)
			{
				//Debug.LogError("Update Failed : No Target Mesh Group");
				return;
			}
			if (isForce)
			{
				//_targetMeshGroup.SetAllRenderUnitForceUpdate();
				_targetMeshGroup.RefreshForce(isDepthChanged, tDelta);
			}
			else
			{
				_targetMeshGroup.UpdateRenderUnits(tDelta, isUpdateVertsAlways);
			}

		}


		// Opt용 Update / Opt 플레이 제어
		//---------------------------------------------
		// Opt용 Update 함수들은 "플레이 상태"의 영향을 받지 않는다.
		// AnimPlayUnit에 래핑된 상태이므로 모든 제어에 대해서 바로 처리한다.
		// Editor와 달리 "실수형 CurFrame"을 이용하며, Reverse 처리도 가능하다
		// MeshGroup과 ControlParam을 직접 제어하진 않는다. (AnimPlayUnit이 한다)

		/// <summary>
		/// [Opt 실행] Delta만큼 업데이트를 한다.
		/// Keyframe Weight와 Control Param Result를 만든다.
		/// </summary>
		/// <param name="tDelta">재생 시간. 음수면 Reverse가 된다.</param>
		/// <returns>Update가 종료시 True, 그 외에는 False이다.</returns>
		public bool Update_Opt(float tDelta)
		{
			_tUpdate += tDelta;
			_tUpdateTotal += tDelta;
			bool isEnd = false;
			if (tDelta > 0)
			{
				//Speed Ratio가 크면 프레임이 한번에 여러개 이동할 수 있다.
				while (_tUpdate > TimePerFrame)
				{
					//프레임이 증가한다.
					_curFrame++;
					_tUpdate -= TimePerFrame;

					if (_curFrame >= _endFrame)
					{
						if (_isLoop)
						{
							//루프일 경우 -> 첫 프레임으로 돌아간다.
							_curFrame = _startFrame;
							_tUpdateTotal -= TimeLength;

							//Animation 이벤트도 리셋한다.
							if (_animEvents != null && _animEvents.Count > 0)
							{
								for (int i = 0; i < _animEvents.Count; i++)
								{
									_animEvents[i].ResetCallFlag();
								}
							}
						}
						else
						{
							//루프가 아닐 경우
							_curFrame = _endFrame;
							_tUpdate = 0.0f;
							_tUpdateTotal = TimeLength;
							isEnd = true;
							break;
						}
					}

					//UpdateControlParam(false, _parentPlayUnit._layer, _parentPlayUnit.UnitWeight, _parentPlayUnit.BlendMethod);
				}
			}
			else if (tDelta < 0)
			{
				while (_tUpdate < 0.0f)
				{
					//프레임이 감소한다.
					_curFrame--;
					_tUpdate += TimePerFrame;

					if (_curFrame <= _startFrame)
					{
						if (_isLoop)
						{
							//루프일 경우 -> 마지막 프레임으로 돌아간다.
							_curFrame = _endFrame;
							_tUpdateTotal += TimeLength;

							//Animation 이벤트도 리셋한다.
							if (_animEvents != null && _animEvents.Count > 0)
							{
								for (int i = 0; i < _animEvents.Count; i++)
								{
									_animEvents[i].ResetCallFlag();
								}
							}
						}
						else
						{
							//루프가 아닐 경우
							_curFrame = _startFrame;
							_tUpdate = 0.0f;
							_tUpdateTotal = 0.0f;
							isEnd = true;
							break;
						}
					}

					//UpdateControlParam(false, _parentPlayUnit._layer, _parentPlayUnit.UnitWeight, _parentPlayUnit.BlendMethod);
				}
			}

			float unitWeight = _parentPlayUnit.UnitWeight;

			//이거 문제 생기면 끈다.
			//if (_parentPlayUnit._playOrder == 0)
			//{
			//	unitWeight = 1.0f;
			//}
			//??이거요?

			//UpdateControlParam(false, _parentPlayUnit._layer, _parentPlayUnit.UnitWeight, _parentPlayUnit.BlendMethod);
			UpdateControlParam(false, _parentPlayUnit._layer, unitWeight, _parentPlayUnit.BlendMethod);

			//추가
			//AnimEvent도 업데이트 하자
			if(_animEvents != null && _animEvents.Count > 0)
			{
				apAnimEvent animEvent = null;
				for (int i = 0; i < _animEvents.Count; i++)
				{
					animEvent = _animEvents[i];
					animEvent.Calculate(CurFrameFloat, CurFrame, (tDelta > 0.0f));
					if(animEvent.IsEventCallable())
					{
						if(_portrait._optAnimEventListener != null)
						{
							//애니메이션 이벤트를 호출해줍시다.
							_portrait._optAnimEventListener.SendMessage(animEvent._eventName, animEvent.GetCalculatedParam(), SendMessageOptions.DontRequireReceiver);
							//UnityEngine.Debug.Log("Animation Event : " + animEvent._eventName);
						}

						
						
					}
				}
			}

			//if (isEnd)
			//{
			//	//Animation 이벤트도 리셋한다.
			//	if (_animEvents != null && _animEvents.Count > 0)
			//	{
			//		for (int i = 0; i < _animEvents.Count; i++)
			//		{
			//			_animEvents[i].ResetCallFlag();
			//		}
			//	}
			//}

			return isEnd;
		}




		/// <summary>
		/// [Opt 실행] 특정 프레임으로 이동한다.
		/// Keyframe Weight와 Control Param Result를 만든다.
		/// Start - End 프레임 사이의 값으로 강제된다.
		/// </summary>
		/// <param name="frame"></param>
		public void SetFrame_Opt(int frame)
		{
			_curFrame = Mathf.Clamp(frame, _startFrame, _endFrame);
			_tUpdate = 0.0f;

			_tUpdateTotal = (_curFrame - _startFrame) * TimePerFrame;

			UpdateControlParam(false, _parentPlayUnit._layer, _parentPlayUnit.UnitWeight, _parentPlayUnit.BlendMethod);

			//프레임 이동시에 AnimEvent를 다시 리셋한다.
			if (_animEvents != null && _animEvents.Count > 0)
			{
				for (int i = 0; i < _animEvents.Count; i++)
				{
					_animEvents[i].ResetCallFlag();
				}
			}
		}


		/// <summary>
		/// [Opt 실행] 플레이를 정지한다.
		/// 첫 프레임으로 자동으로 돌아간다.
		/// </summary>
		public void Stop_Opt(bool isRefreshMeshAndControlParam = true)
		{
			_isPlaying = false;
			_tUpdate = 0.0f;
			_curFrame = _startFrame;//<첫 프레임으로 돌아간다.

			if (isRefreshMeshAndControlParam)
			{
				UpdateControlParam(true);
			}

			//Animation 이벤트도 리셋한다.
			if (_animEvents != null && _animEvents.Count > 0)
			{
				for (int i = 0; i < _animEvents.Count; i++)
				{
					_animEvents[i].ResetCallFlag();
				}
			}
		}

		/// <summary>
		/// [Opt 실행] 플레이를 일시중지한다.
		/// 프레임은 현재 위치에서 정지
		/// </summary>
		public void Pause_Opt()
		{
			_isPlaying = false;
			_curFrame = Mathf.Clamp(_curFrame, _startFrame, _endFrame);

			UpdateControlParam(true);
		}

		/// <summary>
		/// [Opt 실행] 다른 처리 없이 프레임만 "시작 프레임"옮긴다.
		/// 애니메이션 처리가 끝났을 때, 처음 실행 될 때 이 함수를 먼저 호출해주자
		/// </summary>
		public void ResetFrame()
		{
			_curFrame = _startFrame;
			_tUpdate = 0.0f;
		}

		/// <summary>
		/// _isPlaying 변수를 제어 한다. 단지 그뿐
		/// </summary>
		public void SetPlaying_Opt(bool isPlaying)
		{
			_isPlaying = isPlaying;
		}
		// Functions
		//---------------------------------------------
		public void RefreshTimelines()
		{
			for (int i = 0; i < _timelines.Count; i++)
			{
				_timelines[i].RefreshLayers();
			}

			//추가
			//Control Param Result 객체와 연결을 하자
			MakeAndLinkControlParamResults();
		}


		/// <summary>
		/// AnimClip이 Control Param을 제어하기 위해서는 이 함수를 호출하여 업데이트를 할 수 있게 해야한다.
		/// [Opt에서는 AnimPlayData를 링크할때 만들어주자]
		/// </summary>
		public void MakeAndLinkControlParamResults()
		{
			if (_controlParamResult == null)
			{
				_controlParamResult = new List<apAnimControlParamResult>();
			}
			_controlParamResult.Clear();

			apAnimTimeline timeline = null;
			apAnimTimelineLayer layer = null;

			for (int iTL = 0; iTL < _timelines.Count; iTL++)
			{
				timeline = _timelines[iTL];
				if (timeline._linkType != LINK_TYPE.ControlParam)
				{
					continue;
				}

				for (int iL = 0; iL < timeline._layers.Count; iL++)
				{
					layer = timeline._layers[iL];

					if (layer._linkedControlParam == null)
					{
						continue;
					}

					apAnimControlParamResult cpResult = GetControlParamResult(layer._linkedControlParam);
					if (cpResult == null)
					{
						cpResult = new apAnimControlParamResult(layer._linkedControlParam);
						cpResult.Init();
						_controlParamResult.Add(cpResult);
					}

					//레이어와도 연동해주자
					//ControlParam <- CPResult <- Layer
					//     ^------------------------]

					layer._linkedControlParamResult = cpResult;
				}
			}
		}

		public apAnimControlParamResult GetControlParamResult(apControlParam targetControlParam)
		{
			return _controlParamResult.Find(delegate (apAnimControlParamResult a)
			{
				return a._targetControlParam == targetControlParam;
			});
		}



		// Get / Set
		//---------------------------------------------
		public bool IsTimelineContain(apAnimTimeline animTimeline)
		{
			return _timelines.Contains(animTimeline);
		}
		public bool IsTimelineContain(LINK_TYPE linkType, int modifierID)
		{
			return _timelines.Exists(delegate (apAnimTimeline a)
			{
				if (linkType == LINK_TYPE.AnimatedModifier)
				{
					return a._linkType == linkType && a._modifierUniqueID == modifierID;
				}
				else
				{
					return a._linkType == linkType;
				}
			});
		}

		public apAnimTimeline GetTimeline(int timelineID)
		{
			return _timelines.Find(delegate (apAnimTimeline a)
			{
				return a._uniqueID == timelineID;
			});
		}


		public void SetOption_FPS(int fps)
		{
			_FPS = fps;
			if (_FPS < 1)
			{
				_FPS = 1;
			}
			_secPerFrame = 1.0f / (float)_FPS;
		}

		public void SetOption_StartFrame(int startFrame)
		{
			_startFrame = startFrame;
		}

		public void SetOption_EndFrame(int endFrame)
		{
			_endFrame = endFrame;
		}

		public void SetOption_IsLoop(bool isLoop)
		{
			_isLoop = isLoop;
		}

		//---------------------------------------------------------------------------------------
		// Copy For Bake
		//---------------------------------------------------------------------------------------
		public void CopyFromAnimClip(apAnimClip srcAnimClip)
		{
			_uniqueID = srcAnimClip._uniqueID;
			_name = srcAnimClip._name;

			_targetMeshGroupID = srcAnimClip._targetMeshGroupID;
			_targetMeshGroup = null;
			_targetOptTranform = null;

			_FPS = srcAnimClip._FPS;
			_startFrame = srcAnimClip._startFrame;
			_endFrame = srcAnimClip._endFrame;
			_isLoop = srcAnimClip._isLoop;

			//Timeline 복사
			_timelines.Clear();
			for (int iTimeline = 0; iTimeline < srcAnimClip._timelines.Count; iTimeline++)
			{
				apAnimTimeline srcTimeline = srcAnimClip._timelines[iTimeline];

				//Timeline을 복사하자.
				//내부에서 차례로 Layer, Keyframe도 복사된다.
				apAnimTimeline newTimeline = new apAnimTimeline();
				newTimeline.CopyFromTimeline(srcTimeline);

				_timelines.Add(newTimeline);
			}

			//AnimEvent 복사
			_animEvents.Clear();
			for (int iEvent = 0; iEvent < srcAnimClip._animEvents.Count; iEvent++)
			{
				apAnimEvent srcEvent = srcAnimClip._animEvents[iEvent];

				//Event 복사하자
				apAnimEvent newEvent = new apAnimEvent();
				newEvent.CopyFromAnimEvent(srcEvent);

				_animEvents.Add(newEvent);
			}
		}
	}

}