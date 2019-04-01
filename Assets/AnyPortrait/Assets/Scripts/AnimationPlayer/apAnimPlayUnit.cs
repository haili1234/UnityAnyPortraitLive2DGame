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


using AnyPortrait;

namespace AnyPortrait
{
	//AnimClip을 감싸고 Runtime에서 재생이 되는 유닛.
	//Layer 정보를 가지고 블렌딩의 기준이 된다.
	//Queue의 실행 순서에 따라서 대기->페이드인(재생)->재생->페이드아웃(재생)->끝의 생명 주기를 가진다.
	//"자동 재생 종료"옵션에 따라 "Loop가 아닌 AnimClip"은 자동으로 재생이 끝나기도 한다.
	/// <summary>
	/// A class that is the unit for playing animation according to Animation Clip information.
	/// It is controlled by the "apAnimPlayQueue" and is updated.
	/// (It is recommended to use "apAnimPlayManager" to control, and it is possible to read play state.)
	/// </summary>
	public class apAnimPlayUnit
	{
		// Members
		//-----------------------------------------------
		/// <summary>[Please do not use it] Parent apAnimPlayQueue</summary>
		public apAnimPlayQueue _parentQueue = null;

		/// <summary>[Please do not use it] Linked Animation Clip</summary>
		public apAnimClip _linkedAnimClip = null;

		/// <summary>[Please do not use it] Linked Target Root Unit</summary>
		public apOptRootUnit _targetRootUnit = null;//렌더링 대상이 되는 루트 유닛

		//최종적으로 제어하고 있는 Request를 저장한다.
		//Weight 지정은 여러 Request에서 중첩적으로 하지만, 스테이트 제어는 마지막에 생성된 Request만 가능하다.
		private apAnimPlayRequest _ownerRequest_Prev = null;
		private apAnimPlayRequest _ownerRequest_Next = null;

		/// <summary>[Please do not use it] Animation Request (Prev)</summary>
		public apAnimPlayRequest PrevOwnerRequest {  get { return _ownerRequest_Prev; } }

		/// <summary>[Please do not use it] Animation Request (Next)</summary>
		public apAnimPlayRequest NextOwnerRequest {  get { return _ownerRequest_Next; } }

		/// <summary>[Please do not use it] Animation Layer</summary>
		public int _layer = -1;

		/// <summary>[Please do not use it] Animation Play Order in a layer</summary>
		public int _playOrder = -1;//<<이게 재생 순서. (Layer에 따라 증가하며, 동일 Layer에서는 Queue의 재생 순서에 따라 매겨진다.

		/// <summary>[Please do not use it] Parent Request Order</summary>
		public int _requestedOrder = -1;//재생순서와 달리, Request의 순서에 따라 매겨진다. List의 인덱스와 다를 수 있다.

		/// <summary>
		/// 대기/페이드/플레이 상태
		/// (Pause는 별도의 변수로 체크하며 여기서는 Play에 포함된다)
		/// </summary>
		public enum PLAY_STATUS
		{
			/// <summary>Ready : 등록만 되고 아무런 처리가 되지 않았다. Queue 대기 상태인 경우</summary>
			Ready = 0,
			/// <summary>Play : 플레이가 되고 있는 중</summary>
			Play = 1,
			/// <summary>End : 플레이가 모두 끝났다. (삭제 대기)</summary>
			End = 2
		}

		private PLAY_STATUS _playStatus = PLAY_STATUS.Ready;

		/// <summary>Play Status (Ready -> Play -> End)</summary>
		public PLAY_STATUS PlayStatus { get { return _playStatus; } }

		/// <summary>[Please do not use it] Pause Status</summary>
		public bool _isPause = false;


		
		public enum BLEND_METHOD
		{
			Interpolation = 0,
			Additive = 1
		}

		private BLEND_METHOD _blendMethod = BLEND_METHOD.Interpolation;

		/// <summary>[Please do not use it] Blend Method</summary>
		public BLEND_METHOD BlendMethod { get { return _blendMethod; } }

		//AnimClip이 Loop 타입이 아니라면 자동으로 종료한다.
		/// <summary>End Automatically (If it is not loop animation)</summary>
		private bool _isAutoEnd = false;

		//배속 비율 (기본값 1)
		/// <summary>Animation Play Speed Ratio (Default : 1.0)</summary>
		private float _speedRatio = 1.0f;



		// 내부 스테이트 처리 변수
		private PLAY_STATUS _nextPlayStatus = PLAY_STATUS.Ready;
		private bool _isFirstFrame = false;
		//private float _tFade = 0.0f;

		//총 재생 시간.
		//private float _tAnimClipLength = 0.0f;

		private float _unitWeight = 0.0f;
		private bool _isWeightCalculated = false;
		private float _totalRequestWeights = 0.0f;

		
		/// <summary>
		/// [Please do not use it] Blend Weight of Requests
		/// </summary>
		public float TotalRequestWeights {  get { return _totalRequestWeights; } }
		
		/// <summary>
		/// [Please do not use it] Blend Weight
		/// </summary>
		public float UnitWeight
		{
			get
			{
				if (_playStatus != PLAY_STATUS.Play)
				{
					return 0.0f;
				}


				if (_totalRequestWeights > 0.0f)
				{
					return _unitWeight / _totalRequestWeights;
				}
				if (_isWeightCalculated)
				{
					//일단 빼자
					//Debug.LogError("Calculated가 된 Play Unit : " + _unitWeight + " / Total : " + _totalRequestWeights);
				}

				return 1.0f;
			}
		}

		private bool _tmpIsEnd = false;

		private bool _isLoop = false;

		private bool _isPlayStartEventCalled = false;
		private bool _isEndEventCalled = false;

		//public float FadeInTime { get { return _fadeInTime; } }
		//public float FadeOutTime { get { return _fadeOutTime; } }
		//public float DelayToPlayTime { get { return _delayToPlayTime; } }
		//public float DelayToEndTime {  get { return _delayToEndTime; } }


		private int _linkKey = -1;

		/// <summary>[Please do not use it] Request Key</summary>
		public int LinkKey { get { return _linkKey; }  }


		



		// Init
		//-----------------------------------------------
		public apAnimPlayUnit(apAnimPlayQueue parentQueue, int requestedOrder, int linkKey)
		{
			_parentQueue = parentQueue;
			_requestedOrder = requestedOrder;
			_linkKey = linkKey;

			
		}

		

		/// <summary>
		/// [Please do not use it] Set AnimClip to get data
		/// </summary>
		/// <param name="playData"></param>
		/// <param name="layer"></param>
		/// <param name="blendMethod"></param>
		/// <param name="isAutoEndIfNotLoop"></param>
		/// <param name="isEditor"></param>
		public void SetAnimClip(apAnimPlayData playData, int layer, BLEND_METHOD blendMethod, bool isAutoEndIfNotLoop, bool isEditor)
		{
			_linkedAnimClip = playData._linkedAnimClip;
			_targetRootUnit = playData._linkedOptRootUnit;

			//추가
			if (_linkedAnimClip._parentPlayUnit != null
				&& _linkedAnimClip._parentPlayUnit != this)
			{
				//이미 다른 PlayUnit이 사용중이었다면..
				_linkedAnimClip._parentPlayUnit.SetEnd();
				//_linkedAnimClip._parentPlayUnit._linkedAnimClip = null;
			}
			_linkedAnimClip._parentPlayUnit = this;

			_layer = layer;

			_isLoop = _linkedAnimClip.IsLoop;
			_isAutoEnd = isAutoEndIfNotLoop;
			if (_isLoop)
			{
				_isAutoEnd = false;//<<Loop일때 AutoEnd는 불가능하다
			}


			_blendMethod = blendMethod;

			_isPause = false;
			_playStatus = PLAY_STATUS.Ready;
			_isPlayStartEventCalled = false;
			_isEndEventCalled = false;

			//_fadeInTime = 0.0f;
			//_fadeOutTime = 0.0f;

			//_delayToPlayTime = 0.0f;
			//_delayToEndTime = 0.0f;

			_speedRatio = 1.0f;

			_isFirstFrame = true;
			_nextPlayStatus = _playStatus;

			if (isEditor)
			{
				_linkedAnimClip.Stop_Editor(false);//Stop은 하되 업데이트는 하지 않는다. (false)
			}
			else
			{
				_linkedAnimClip.Stop_Opt(false);
			}

			//_tAnimClipLength = _linkedAnimClip.TimeLength;
			_unitWeight = 0.0f;
			_isWeightCalculated = false;
			_totalRequestWeights = 0.0f;
			//_prevUnitWeight = 0.0f;

			//_debugWeight1 = 0.0f;
			//_debugWeight2 = 0.0f;
			//_debugWeight3 = 0.0f;

			//_isDelayIn = false;
			//_isDelayOut = false;

			//_tDelay = 0.0f;

		}


		/// <summary>
		/// [Please do not use it] Set Playing option
		/// </summary>
		/// <param name="blendMethod"></param>
		/// <param name="isAutoEndIfNotLoop"></param>
		/// <param name="newRequestedOrder"></param>
		/// <param name="newLinkKey"></param>
		public void SetSubOption(BLEND_METHOD blendMethod, bool isAutoEndIfNotLoop, int newRequestedOrder, int newLinkKey)
		{
			_blendMethod = blendMethod;
			_isAutoEnd = isAutoEndIfNotLoop;
			_requestedOrder = newRequestedOrder;
			_linkKey = newLinkKey;

			if (_isLoop)
			{
				_isAutoEnd = false;//<<Loop일때 AutoEnd는 불가능하다
			}
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="request"></param>
		public void SetOwnerRequest_Prev(apAnimPlayRequest request)
		{
			_ownerRequest_Prev = request;
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="request"></param>
		public void SetOwnerRequest_Next(apAnimPlayRequest request)
		{
			_ownerRequest_Next = request;
		}

		// Update
		//-----------------------------------------------
		#region [미사용 코드] UnitWeight를 계산하는건 외부에서 일괄적으로 한다. 자체적으로 하면 문제가 많다.
		///// <summary>
		///// Update 직전에 UnitWeight를 계산한다.
		///// 유효하지 않을 경우 -1 리턴.
		///// 꼭 Update 직전에 호출해야한다.
		///// 실제 Clip 업데이트 전에 타이머/스테이트 처리등을 수행한다.
		///// </summary>
		///// <returns></returns>
		//public float CalculateUnitWeight(float tDelta)
		//{
		//	_tmpIsEnd = false;

		//	if(_linkedAnimClip._parentPlayUnit != this)
		//	{
		//		return -1.0f;
		//	}

		//	PLAY_STATUS requestedNextPlayStatus = _nextPlayStatus;

		//	switch (_playStatus)
		//	{
		//		case PLAY_STATUS.Ready:
		//			{
		//				if (_isFirstFrame)
		//				{
		//					_unitWeight = 0.0f;
		//					//_prevUnitWeight = 0.0f;
		//				}
		//				//if (!_isPause)
		//				//{
		//				//	if (_isDelayIn)
		//				//	{
		//				//		//딜레이 후에 플레이된다.
		//				//		_tDelay += tDelta;
		//				//		if (_tDelay > _delayToPlayTime)
		//				//		{
		//				//			_unitWeight = 0.0f;
		//				//			_isDelayIn = false;
		//				//			ChangeNextStatus(PLAY_STATUS.PlayWithFadeIn);//<<플레이 된다.
		//				//		}
		//				//	}
		//				//}
		//			}
		//			break;


		//		//case PLAY_STATUS.PlayWithFadeIn:
		//		//	{
		//		//		if(_isFirstFrame)
		//		//		{
		//		//			_tFade = 0.0f;
		//		//			_prevUnitWeight = _unitWeight;
		//		//		}
		//		//		if (!_isPause)
		//		//		{
		//		//			_tFade += tDelta;

		//		//			if (_tFade < _fadeInTime)
		//		//			{
		//		//				_unitWeight = (_prevUnitWeight * (_fadeInTime - _tFade) + 1.0f * _tFade) / _fadeInTime;
		//		//			}
		//		//			else
		//		//			{
		//		//				_unitWeight = 1.0f;
		//		//				//Fade가 끝났으면 Play
		//		//				ChangeNextStatus(PLAY_STATUS.Play);
		//		//			}
		//		//		}
		//		//	}
		//		//	break;

		//		case PLAY_STATUS.Play:
		//			{
		//				if(_isFirstFrame)
		//				{
		//					_unitWeight = 1.0f;
		//					//_prevUnitWeight = 1.0f;
		//				}

		//				if (!_isPause)
		//				{
		//					//if (_isDelayOut)
		//					//{
		//					//	//딜레이 후에 FadeOut된다.
		//					//	_tDelay += tDelta;
		//					//	if (_tDelay > _delayToEndTime)
		//					//	{
		//					//		_isDelayOut = false;
		//					//		_unitWeight = 1.0f;
		//					//		ChangeNextStatus(PLAY_STATUS.PlayWithFadeOut);//<<플레이 종료를 위한 FadeOut
		//					//	}
		//					//}
		//				}
		//			}
		//			break;

		//		case PLAY_STATUS.PlayWithFadeOut:
		//			{
		//				if(_isFirstFrame)
		//				{
		//					_tFade = 0.0f;
		//					_prevUnitWeight = _unitWeight;
		//				}

		//				if (!_isPause)
		//				{
		//					_tFade += tDelta;

		//					if (_tFade < _fadeOutTime)
		//					{
		//						_unitWeight = (_prevUnitWeight * (_fadeOutTime - _tFade) + 0.0f * _tFade) / _fadeOutTime;
		//					}
		//					else
		//					{
		//						_unitWeight = 0.0f;
		//						ChangeNextStatus(PLAY_STATUS.End);
		//					}
		//				}
		//			}
		//			break;


		//		case PLAY_STATUS.End:
		//			{
		//				//아무것도 안합니더
		//				if(_isFirstFrame)
		//				{
		//					//Debug.Log("End");
		//					_unitWeight = 0.0f;
		//				}

		//			}
		//			break;
		//	}

		//	if(_playOrder == 0)
		//	{
		//		return 1.0f;
		//	}
		//	return _unitWeight;
		//} 
		#endregion

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="weight"></param>
		/// <param name="isCalculated"></param>
		public void SetWeight(float weight, bool isCalculated)
		{
			//외부에서 Weight를 지정한다.
			_unitWeight = weight;
			_isWeightCalculated = isCalculated;
			_totalRequestWeights = 0.0f;

			//_debugWeight1 = _unitWeight;
			//_debugWeight2 = _unitWeight;
			//_debugWeight3 = _unitWeight;
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="multiplyUnitWeight"></param>
		/// <param name="requestWeight"></param>
		public void AddWeight(float multiplyUnitWeight, float requestWeight)
		{
			//외부에서 Weight를 지정한다.
			//_unitWeight = Mathf.Clamp01(_unitWeight * multiplyRatio);
			//_unitWeight = Mathf.Clamp01((_unitWeight * multiplyUnitWeight * requestWeight) + (_unitWeight * (1-requestWeight)));
			_unitWeight = _unitWeight + (multiplyUnitWeight * requestWeight);
			_totalRequestWeights += requestWeight;

			_isWeightCalculated = true;

			//if(iDebugType == 1)
			//{
			//	_debugWeight1 += multiplyUnitWeight * requestWeight;
			//}
			//else if(iDebugType == 2)
			//{
			//	_debugWeight2 += multiplyUnitWeight * requestWeight;
			//}
			//else
			//{
			//	_debugWeight3 += multiplyUnitWeight * requestWeight;
			//}
			
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="normalizeWeight"></param>
		public void NormalizeWeight(float normalizeWeight)
		{
			_unitWeight *= normalizeWeight;
		}

		/// <summary>
		/// [Please do not use it] Update Animation
		/// </summary>
		/// <param name="tDelta"></param>
		public void Update(float tDelta)
		{
			//_isUpdated = false;

			_tmpIsEnd = false;

			//_unitWeight *= weightCorrectRatio;//<<이거 안해요

			if (_linkedAnimClip._parentPlayUnit != this)
			{
				//PlayUnit이 더이상 이 AnimClip을 제어할 수 없게 되었다
				//Link Release를 하고 업데이트도 막는다.
				//Debug.LogError("AnimPlayUnit Invalid End");
				ReleaseLink();
				return;
			}

			PLAY_STATUS requestedNextPlayStatus = _nextPlayStatus;

			switch (_playStatus)
			{
				case PLAY_STATUS.Ready:
					{
						if (_isFirstFrame)
						{
							//_unitWeight = 0.0f;
							//_prevUnitWeight = 0.0f;
							_linkedAnimClip.SetPlaying_Opt(false);
							_linkedAnimClip.SetFrame_Opt(_linkedAnimClip.StartFrame);
							//Debug.Log("Ready");
						}
						//if (!_isPause)
						//{
						//	if (_isDelayIn)
						//	{
						//		//딜레이 후에 플레이된다.
						//		_tDelay += tDelta;
						//		if (_tDelay > _delayToPlayTime)
						//		{
						//			_unitWeight = 0.0f;
						//			_isDelayIn = false;
						//			ChangeNextStatus(PLAY_STATUS.PlayWithFadeIn);//<<플레이 된다.
						//		}
						//	}
						//}
					}
					break;


				//case PLAY_STATUS.PlayWithFadeIn:
				//	{
				//		if(_isFirstFrame)
				//		{
				//			//_tFade = 0.0f;
				//			//_prevUnitWeight = _unitWeight;

				//			//플레이 시작했다고 알려주자
				//			if (!_isPlayStartEventCalled)
				//			{
				//				_parentQueue.OnAnimPlayUnitPlayStart(this);
				//				_isPlayStartEventCalled = true;
				//			}
				//			//Debug.Log("Play With Fade In");
				//		}
				//		if (!_isPause)
				//		{
				//			//_tFade += tDelta;

				//			if (_tFade < _fadeInTime)
				//			{
				//				//_unitWeight = (_prevUnitWeight * (_fadeInTime - _tFade) + 1.0f * _tFade) / _fadeInTime;

				//				_tmpIsEnd = _linkedAnimClip.Update_Opt(tDelta * _speedRatio);
				//			}
				//			//else
				//			//{
				//			//	_unitWeight = 1.0f;
				//			//	//Fade가 끝났으면 Play
				//			//	ChangeNextStatus(PLAY_STATUS.Play);
				//			//}
				//		}
				//	}
				//	break;

				case PLAY_STATUS.Play:
					{
						if (_isFirstFrame)
						{
							//_unitWeight = 1.0f;
							//_prevUnitWeight = 1.0f;

							//플레이 시작했다고 알려주자
							if (!_isPlayStartEventCalled)
							{
								_parentQueue.OnAnimPlayUnitPlayStart(this);
								_isPlayStartEventCalled = true;
							}
							//Debug.Log("Play");
							_linkedAnimClip.SetPlaying_Opt(true);
						}

						if (!_isPause)
						{
							_linkedAnimClip.SetPlaying_Opt(true);
							_tmpIsEnd = _linkedAnimClip.Update_Opt(tDelta * _speedRatio);
							//_isUpdated = true;

							//if (_isDelayOut)
							//{
							//	//딜레이 후에 FadeOut된다.
							//	_tDelay += tDelta;
							//	if (_tDelay > _delayToEndTime)
							//	{
							//		_isDelayOut = false;
							//		_unitWeight = 1.0f;
							//		ChangeNextStatus(PLAY_STATUS.PlayWithFadeOut);//<<플레이 종료를 위한 FadeOut
							//	}
							//}
						}
						else
						{
							_linkedAnimClip.SetPlaying_Opt(false);
						}
					}
					break;

				//case PLAY_STATUS.PlayWithFadeOut:
				//	{
				//		if(_isFirstFrame)
				//		{
				//			//_tFade = 0.0f;
				//			//_prevUnitWeight = _unitWeight;
				//			//Debug.Log("Play With Fade Out");
				//		}

				//		if (!_isPause)
				//		{
				//			//_tFade += tDelta;

				//			if (_tFade < _fadeOutTime)
				//			{
				//				//_unitWeight = (_prevUnitWeight * (_fadeOutTime - _tFade) + 0.0f * _tFade) / _fadeOutTime;

				//				_tmpIsEnd = _linkedAnimClip.Update_Opt(tDelta * _speedRatio);
				//			}
				//			//else
				//			//{
				//			//	_unitWeight = 0.0f;
				//			//	ChangeNextStatus(PLAY_STATUS.End);
				//			//}
				//		}
				//	}
				//	break;


				case PLAY_STATUS.End:
					{
						//아무것도 안합니더
						if (_isFirstFrame)
						{
							//Debug.Log("End");
							//_unitWeight = 0.0f;
							ReleaseLink();
						}

					}
					break;
			}

			if (_tmpIsEnd && _isAutoEnd)
			{
				//종료가 되었다면 (일단 Loop는 아니라는 것)
				//조건에 따라 End로 넘어가자
				SetEnd();
			}

			//스테이트 처리
			//if(_nextPlayStatus != _playStatus)
			if (requestedNextPlayStatus != _playStatus)
			{
				_playStatus = requestedNextPlayStatus;
				_nextPlayStatus = _playStatus;
				_isFirstFrame = true;
			}
			else if (_isFirstFrame)
			{
				_isFirstFrame = false;
			}
		}


		private void ChangeNextStatus(PLAY_STATUS nextStatus)
		{
			_nextPlayStatus = nextStatus;
		}





		// Functions
		//-----------------------------------------------
		/// <summary>
		/// [Please do not use it]
		/// "Play()" is called by apAnimPlayManager
		/// </summary>
		public void Play()
		{
			if (_playStatus == PLAY_STATUS.Ready)
			{
				//Debug.Log(_linkedAnimClip._name + " >> Play [ Fade : " + fadeTime + " / Delay : " + delayTime + " ]");
				_isPause = false;
				//_fadeInTime = fadeTime;
				_unitWeight = 0.0f;
				//_debugWeight1 = 0.0f;
				//_debugWeight2 = 0.0f;
				//_debugWeight3 = 0.0f;
				//_delayToPlayTime = delayTime;

				//_isDelayIn = true;
				//_isDelayOut = false;

				//_tDelay = 0.0f;

				_isPlayStartEventCalled = false;
				_isEndEventCalled = false;

				//if (delayTime < 0.001f)
				//{
				//	_delayToPlayTime = 0.0f;
				//	_isDelayIn = false;

				//	//딜레이가 없으면 바로 스테이트를 이동한다.
				//	if (_fadeInTime > 0.001f)
				//	{
				//		//Fade In을 하며 시작한다.
				//		ChangeNextStatus(PLAY_STATUS.PlayWithFadeIn);
				//	}
				//	else
				//	{
				//		//Debug.Log("Direct Play");
				//		//바로 시작
				//		ChangeNextStatus(PLAY_STATUS.Play);
				//	}
				//}

				//Debug.Log("Direct Play");
				//바로 시작
				ChangeNextStatus(PLAY_STATUS.Play);
			}
		}

		//일반적인 Play와 달리 강제로 재시작을 한다.
		
		/// <summary>
		/// [Please do not use it]
		/// "ResetPlay()" is called by apAnimPlayManager
		/// </summary>
		public void ResetPlay()
		{
			_isPause = false;
			_isPlayStartEventCalled = false;
			_isEndEventCalled = false;
			ChangeNextStatus(PLAY_STATUS.Play);
			_isFirstFrame = true;

			_linkedAnimClip.SetFrame_Opt(_linkedAnimClip.StartFrame);
		}

		/// <summary>
		/// [Please do not use it]
		/// ResetFrame() without other processing
		/// </summary>
		public void ResetFrame()
		{
			_linkedAnimClip.ResetFrame();
		}

		/// <summary>
		/// [Please do not use it]
		/// "Resume" is called by apAnimPlayManager
		/// </summary>
		public void Resume()
		{
			_isPause = false;
			
		}

		/// <summary>
		/// [Please do not use it]
		/// "Pause()" is called by apAnimPlayManager
		/// </summary>
		public void Pause()
		{
			_isPause = true;
		}

		/// <summary>
		/// Set SpeedRatio (Defulat : 1.0)
		/// </summary>
		/// <param name="speedRatio"></param>
		public void SetSpeed(float speedRatio)
		{
			_speedRatio = speedRatio;
		}


		
		/// <summary>
		/// [Please do not use it]
		/// "SetEnd()" is called by apAnimPlayManager
		/// </summary>
		public void SetEnd()
		{
			_unitWeight = 0.0f;
			
			_totalRequestWeights = 1.0f;
			_isWeightCalculated = true;

			_isPlayStartEventCalled = false;
			ChangeNextStatus(PLAY_STATUS.End);
		}


		// SetEnd와 비슷하지만 PlayStatus를 Ready로 바꾼다.
		/// <summary>
		/// [Please do not use it] Initialize Weight
		/// </summary>
		public void SetWeightZero()
		{
			_unitWeight = 0.0f;
			_totalRequestWeights = 1.0f;
			_isWeightCalculated = true;

			_isPlayStartEventCalled = false;
			ChangeNextStatus(PLAY_STATUS.Ready);
		}


		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void ReleaseLink()
		{
			//연결된 Calculate와 연동을 끊는다.
			if (!_isEndEventCalled)
			{
				_parentQueue.OnAnimPlayUnitEnded(this);
				_isEndEventCalled = true;
				_playStatus = PLAY_STATUS.End;
			}
		}


		// Get / Set
		//-----------------------------------------------
		// 재생이 끝나고 삭제를 해야하는가
		/// <summary>
		/// [Please do not use it] Is Ended
		/// </summary>
		public bool IsRemovable { get { return _playStatus == PLAY_STATUS.End; } }

		/// <summary>
		/// [Please do not use it] Is Updatable status (Ready or Play)
		/// </summary>
		public bool IsUpdatable
		{
			get
			{
				return _playStatus == PLAY_STATUS.Ready ||
					//_playStatus == PLAY_STATUS.PlayWithFadeIn ||
					_playStatus == PLAY_STATUS.Play;
				//_playStatus == PLAY_STATUS.PlayWithFadeOut;
			}
		}

		/// <summary>
		/// Is Loop Animation
		/// </summary>
		public bool IsLoop { get { return _isLoop; } }


		//PlayUnit이 자동으로 종료가 되는가. 이게 True여야 Queued Play가 가능하다
		//[Loop가 아니어야 하며, isAutoEndIfNotLoop = true여야 한다]
		/// <summary>
		/// Is it not a Loop animation and has an automatic end request?
		/// </summary>
		public bool IsEndAutomaticallly
		{
			get
			{
				if (_isLoop)
				{
					return false;
				}
				return _isAutoEnd;
			}
		}

		/// <summary>
		/// Remaining playing time. (Return -1 if it is loop animation).
		/// </summary>
		public float RemainPlayTime
		{
			get
			{
				if (_isLoop)
				{
					return -1.0f;
				}
				return _linkedAnimClip.TimeLength - _linkedAnimClip.TotalPlayTime;
			}
		}

		/// <summary>
		/// Total played time
		/// </summary>
		public float TotalPlayTime
		{
			get
			{
				return _linkedAnimClip.TotalPlayTime;
			}
		}

		/// <summary>
		/// Animation Time Length
		/// </summary>
		public float TimeLength
		{
			get
			{
				return _linkedAnimClip.TimeLength;
			}
		}

		public int Frame
		{
			get
			{
				return _linkedAnimClip.CurFrame;
			}
		}
		public int StartFrame
		{
			get
			{
				return _linkedAnimClip.StartFrame;
			}
		}
		public int EndFrame
		{
			get
			{
				return _linkedAnimClip.EndFrame;
			}
		}

		/// <summary>
		/// [Please do not use it] Set Play Order
		/// </summary>
		/// <param name="playOrder"></param>
		public void SetPlayOrder(int playOrder)
		{
			_playOrder = playOrder;
		}
	}

}