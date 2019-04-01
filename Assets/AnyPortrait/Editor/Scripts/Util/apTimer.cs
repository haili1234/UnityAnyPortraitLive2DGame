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
using System.Diagnostics;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public class apTimer
	{
		// SingleTone
		//----------------------------------------
		private static apTimer _instance = new apTimer();
		public static apTimer I
		{
			get
			{
				return _instance;
			}
		}

		// Members
		//----------------------------------------

		public enum TIMER_TYPE
		{
			Update = 0,//최대 60FPS를 지원하는 동기화된 프레임의 시간 [동기화 처리를 한다.]
			UpdateAllFrame = 1,//모든 Update 프레임 시간을 계산하여 매번 연산된 시간
			Repaint = 2//Repaint 이벤트마다 계산된 시간
		}

		private const int NUM_TIME_TYPE = 3;
		private const int UPDATE = 0;
		private const int UPDATE_ALL_FRAME = 1;
		private const int REPAINT = 2;


		private Stopwatch[] _stopWatch = new Stopwatch[NUM_TIME_TYPE];
		private long[] _deltaTimeCount = new long[NUM_TIME_TYPE];//누적되는 값(msec)
		private double[] _prevDeltaTime = new double[NUM_TIME_TYPE];//<<결과값에 사용되는 값은 이 값이다.

		//각 스톱워치별로 추가적인 누적형 타이머를 두어서 배율을 계산한다.
		//특정 시간 간격으로 StopWatch에 의한 누적 시간과 DateTime에 의한 누적 시간을 비교하여 타이머 비율을 계산한다.
		//(짧은 시간은 StopWatch가 정확하고 긴 시간은 DateTime이 정확하기 때문)
		private class SubTimer
		{
			private DateTime _recordTime;
			private double _timeMultiply = 0.0f;
			private long _curTotalStopwatchTime = 0;

			private const long CHECK_TIME_UNIT_LONG_MS = 5000;//<<2초마다 정확도 갱신
			public SubTimer()
			{
				Reset();
			}
			public void Reset()
			{
				_recordTime = DateTime.Now;
				_timeMultiply = 1.0f;
				_curTotalStopwatchTime = 0;
			}
			public void UpdateTime(long addedStopwatchTimeMs)
			{
				_curTotalStopwatchTime += addedStopwatchTimeMs;
				if(_curTotalStopwatchTime > CHECK_TIME_UNIT_LONG_MS)
				{
					double dateTime = DateTime.Now.Subtract(_recordTime).TotalSeconds;

					_timeMultiply = (dateTime / ((double)_curTotalStopwatchTime / 1000.0));//시간 비율 = 실제 시간(DateTime) / Stopwatch 누적 시간

					//UnityEngine.Debug.Log("Sub Timer : " + _timeMultiply + " : " + dateTime + " / " + ((double)_curTotalStopwatchTime / 1000.0));
					_recordTime = DateTime.Now;
					_curTotalStopwatchTime = 0;
				}
			}

			public double TimeMultiply { get { return _timeMultiply; } }
		}
		private SubTimer[] _subTimer = new SubTimer[NUM_TIME_TYPE];


		private int _fps = 0;//Repaint 타입의 연산 시간을 계산한다.

		private const long MIN_UPDATE_DELTA_TIME = 16;//(16)60FPS보다 작으면 강제 업데이트 갱신을 막아야 한다.

		private bool _isValidFrame = false;
		// Init
		//----------------------------------------
		private apTimer()
		{
			for (int i = 0; i < NUM_TIME_TYPE; i++)
			{
				_stopWatch[i] = new Stopwatch();
				_stopWatch[i].Start();
				_deltaTimeCount[i] = 0;
				_prevDeltaTime[i] = 0.0f;

				_subTimer[i] = new SubTimer();
			}
			_fps = 0;
		}

		// Functions
		//----------------------------------------
		public bool CheckTime_Update()
		{
			if(apVersion.I.IsDemoViolation)
			{
				//재생할 수 없다.
				//UnityEngine.Debug.LogError("Timer DemoViolation");
				return false;
			}
			_isValidFrame = false;

			_stopWatch[UPDATE].Stop();
			_stopWatch[UPDATE_ALL_FRAME].Stop();

			long deltaTime_Update = _stopWatch[UPDATE].ElapsedMilliseconds;
			long deltaTime_UpdateAllFrame = _stopWatch[UPDATE_ALL_FRAME].ElapsedMilliseconds;

			//Sub Time도 계산
			_subTimer[UPDATE].UpdateTime(deltaTime_Update);
			_subTimer[UPDATE_ALL_FRAME].UpdateTime(deltaTime_UpdateAllFrame);


			_deltaTimeCount[UPDATE] += deltaTime_Update;
			_deltaTimeCount[UPDATE_ALL_FRAME] += deltaTime_UpdateAllFrame;

			//Update는 60FPS보다 높으면 프레임 스킵을 해야한다.
			if (_deltaTimeCount[UPDATE] > MIN_UPDATE_DELTA_TIME)
			{
				_prevDeltaTime[UPDATE] = (_deltaTimeCount[UPDATE] / 1000.0);
				_deltaTimeCount[UPDATE] = 0;
				_isValidFrame = true;
			}

			//Update All Frame은 프레임 스킵 없이 매번 경과 시간을 리턴한다.
			_prevDeltaTime[UPDATE_ALL_FRAME] = (_deltaTimeCount[UPDATE_ALL_FRAME] / 1000.0);
			_deltaTimeCount[UPDATE_ALL_FRAME] = 0;


			_stopWatch[UPDATE].Reset();
			_stopWatch[UPDATE].Start();

			_stopWatch[UPDATE_ALL_FRAME].Reset();
			_stopWatch[UPDATE_ALL_FRAME].Start();

			return _isValidFrame;
		}

		public void CheckTime_Repaint()
		{
			if(apVersion.I.IsDemoViolation)
			{
				//재생할 수 없다.
				return;
			}

			//Repaint 상에서의 경과 시간을 리턴한다.
			_stopWatch[REPAINT].Stop();

			long deltaTime_Repaint = _stopWatch[REPAINT].ElapsedMilliseconds;

			//SubTimer 갱신
			_subTimer[REPAINT].UpdateTime(deltaTime_Repaint);

			_deltaTimeCount[REPAINT] = deltaTime_Repaint;
			_prevDeltaTime[REPAINT] = (_deltaTimeCount[REPAINT] / 1000.0f);

			if (_prevDeltaTime[REPAINT] > 0.0f)
			{
				_fps = (int)(1.0f / (_prevDeltaTime[REPAINT] * _subTimer[REPAINT].TimeMultiply));
			}

			_stopWatch[REPAINT].Reset();
			_stopWatch[REPAINT].Start();
		}

		//강제로 다른 곳에서 업데이트를 하면 초기화한다.
		public void ResetTime_Update()
		{
			//매번 연산하는 값이 아닌 누적 연산을 하는 Update 타입은 외부에서 중복 처리시 타이머를 리셋할 수 있다.
			_stopWatch[UPDATE].Stop();
			_stopWatch[UPDATE].Reset();
			_stopWatch[UPDATE].Start();

			_deltaTimeCount[UPDATE] = 0;
			_prevDeltaTime[UPDATE] = 0.0f;
		}


		// Get / Set
		//----------------------------------------
		public float DeltaTime_Update { get { return (float)(_prevDeltaTime[UPDATE] * _subTimer[UPDATE].TimeMultiply); } }
		public float DeltaTime_UpdateAllFrame { get { return (float)(_prevDeltaTime[UPDATE_ALL_FRAME] * _subTimer[UPDATE_ALL_FRAME].TimeMultiply); } }
		public float DeltaTime_Repaint { get { return (float)(_prevDeltaTime[REPAINT] * _subTimer[REPAINT].TimeMultiply); } }
		

		public int FPS { get { return _fps; } }
	}

}