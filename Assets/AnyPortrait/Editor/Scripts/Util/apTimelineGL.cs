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
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public static class apTimelineGL
	{
		private static int _layoutPosX = 0;
		private static int _layoutPosY_Header = 0;
		//private static int _layoutPosY_Main = 0;
		private static int _layoutWidth = 0;
		private static int _layoutHeight_Header = 0;
		private static int _layoutHeight_Main = 0;
		private static int _layoutHeight_Total = 0;
		private static Vector4 _glScreenClippingSize_Total = Vector4.zero;
		private static Vector4 _glScreenClippingSize_Main = Vector4.zero;
		private static Vector4 _glScreenClippingSize_Header = Vector4.zero;
		private static Vector2 _scrollPos = Vector2.zero;

		private static apGL.MaterialBatch _matBatch_Total = new apGL.MaterialBatch();
		private static apGL.MaterialBatch _matBatch_Main = new apGL.MaterialBatch();
		private static apGL.MaterialBatch _matBatch_Header = new apGL.MaterialBatch();


		private static Texture2D _img_Keyframe = null;
		private static Texture2D _img_KeyframeDummy = null;
		private static Texture2D _img_KeySummary = null;
		private static Texture2D _img_KeySummaryMove = null;
		private static Texture2D _img_PlayBarHead = null;
		private static Texture2D _img_KeyLoopLeft = null;
		private static Texture2D _img_KeyLoopRight = null;
		private static Texture2D _img_CurKeyframe = null;
		private static Texture2D _img_KeyframeCursor = null;
		private static Texture2D _img_EventMark = null;
		private static Texture2D _img_OnionMark = null;

		
		//이동/복사용
		//private static Texture2D _img_KeyframeMoveSrc = null;
		//private static Texture2D _img_KeyframeMove = null;
		//private static Texture2D _img_KeyframeCopy = null;

		private static Texture2D _img_TimelineBGStart = null;
		private static Texture2D _img_TimelineBGEnd = null;


		private static bool _isMouseEvent = false;
		private static bool _isMouseEventUsed = false;
		
		private static apMouse.MouseBtnStatus _leftBtnStatus = apMouse.MouseBtnStatus.Released;
		private static apMouse.MouseBtnStatus _rightBtnStatus = apMouse.MouseBtnStatus.Released;
		private static Vector2 _mousePos = Vector2.zero;
		private static Vector2 _mousePos_Down = Vector2.zero;
		private static Vector2 _scroll_Down = Vector2.zero;
		private static bool _isRefreshScrollDown = false;

		private static float _keyDragStartPos = 0.0f;
		private static float _keyDragCurPos = 0.0f;
		private static int _keyDragFrameIndex_Down = -1;
		private static int _keyDragFrameIndex_Cur = -1;

		//private static EventType _curEventType;
		private static apSelection _selection;

		private static bool _isShift = false;
		private static bool _isCtrl = false;
		private static bool _isAlt = false;


		//private static bool _isMainVisible = false;

		public enum CLIP_TYPE
		{
			Header,
			Main,
			Total
		}

		private static GUIStyle _textStyle = GUIStyle.none;

		//마우스 이벤트
		private enum TIMELINE_EVENT
		{
			None,//아무것도 안하고 있다.
			Select,//빈칸에서 클릭 -> 영역 선택
			ReadyToDrag,//선택을 하였으나 계속 마우스 입력이 되는 중
			DragFrame,//키 프레임 선택 -> 드래그 이동
			DragPlayBar,//플레이 바 선택 -> 드래그 이동
		}

		private static TIMELINE_EVENT _timelineEvent = TIMELINE_EVENT.None;

		public static bool IsKeyframeDragging
		{
			get
			{
				return (_timelineEvent == TIMELINE_EVENT.DragFrame || _timelineEvent == TIMELINE_EVENT.DragPlayBar) 
					&& _leftBtnStatus == apMouse.MouseBtnStatus.Pressed;
			}
		}


		private enum SELECT_TYPE
		{
			New, Add, Subtract
		}

		private static SELECT_TYPE _selectType = SELECT_TYPE.New;
		private static CLIP_TYPE _downClipArea = CLIP_TYPE.Main;

		//외부에서 입력이 들어온 상태에서는 입력을 무시하고 있어야 한다.
		private static bool _isMouseIgnored_ExternalDrag = false;
		private static bool _isMouseIgnored_UpOrUsed = false;//Up 이벤트가 발생했거나 이번 스테이트에서 사용이 되었다면 다음 Down 전까진 무시한다.

		//private enum KEYFRAME_STATUS { Inactive, Normal, Selected, Working, }

		private static Color _keyColor_Normal = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		private static Color _keyColor_Selected = new Color(1.0f, 0.1f, 0.1f, 1.0f);
		private static Color _keyColor_Working = new Color(0.1f, 0.3f, 1.0f, 1.0f);
		private static Color _keyColor_Inactive = new Color(0.3f, 0.3f, 0.3f, 1.0f);

		private static Color _keyColor_Copy = new Color(1.0f, 0.1f, 1.0f, 1.0f);
		private static Color _keyColor_Move = new Color(0.1f, 1.0f, 0.1f, 1.0f);
		private static Color _keyColor_Copy_Line = new Color(1.0f, 0.1f, 1.0f, 0.7f);
		private static Color _keyColor_Move_Line = new Color(0.1f, 1.0f, 0.1f, 0.7f);

		private static Color _keyColor_EventOnce = new Color(0.7f, 0.5f, 0.0f, 1.0f);
		private static Color _keyColor_EventCntPrev = new Color(0.0f, 0.7f, 0.7f, 1.0f);
		private static Color _keyColor_EventCntNext = new Color(0.1f, 0.7f, 0.3f, 0.7f);


		//Keyframe을 이동/복사 할때
		//한번에 하는게 아니라 임시 데이터를 만들어서 
		//드래그 중에는 임시 값을 이용한다.
		private class MoveKeyframe
		{
			public apAnimKeyframe _srcKeyframe = null;
			public int _startFrameIndex = -1;//처음 데이터가 입력되었을 때의 FrameIndex
			public int _nextFrameIndex = -1;
			public apAnimTimelineLayer ParentLayer { get { return _srcKeyframe._parentTimelineLayer; } }

			public MoveKeyframe(apAnimKeyframe srcKeyframe)
			{
				_srcKeyframe = srcKeyframe;
				_startFrameIndex = _srcKeyframe._frameIndex;
				_nextFrameIndex = _startFrameIndex;
			}
		}
		private static List<MoveKeyframe> _moveKeyframeList = new List<MoveKeyframe>();
		private static MoveKeyframe GetMoveKeyframe(apAnimKeyframe srcKeyframe)
		{
			if (_moveKeyframeList.Count == 0)
			{
				return null;
			}

			return _moveKeyframeList.Find(delegate (MoveKeyframe a)
			{
				return a._srcKeyframe == srcKeyframe;
			});
		}


		private class MoveCommonKeyframe
		{
			public apAnimCommonKeyframe _srcCommonKeyframe = null;
			public int _startFrameIndex = -1;
			public int _nextFrameIndex = -1;
			
			public MoveCommonKeyframe(apAnimCommonKeyframe srcCommonKeyframe)
			{
				_srcCommonKeyframe = srcCommonKeyframe;
				_startFrameIndex = _srcCommonKeyframe._frameIndex;
				_nextFrameIndex = _startFrameIndex;
			}
		}

		private static List<MoveCommonKeyframe> _moveCommonKeyframeList = new List<MoveCommonKeyframe>();
		private static MoveCommonKeyframe GetMoveKeyframe(apAnimCommonKeyframe srcCommonKeyframe)
		{
			if (_moveCommonKeyframeList.Count == 0)
			{
				return null;
			}

			return _moveCommonKeyframeList.Find(delegate (MoveCommonKeyframe a)
			{
				return a._srcCommonKeyframe == srcCommonKeyframe;
			});
		}



		// 렌더링을 최적화하기 위한 렌더링 Request 클래스
		// 텍스쳐를 키값으로 하며, 위치, 색상정보를 가진다.
		private class RenderRequest
		{
			public bool _isImage = false;
			public Texture2D _image = null;
			public Vector2 _imageSize = Vector2.zero;
			public List<Vector2> _posList = new List<Vector2>();
			public List<Color> _colorList = new List<Color>();

			//BoldLine인 경우
			public List<Vector2> _linePos1List = new List<Vector2>();
			public List<Vector2> _linePos2List = new List<Vector2>();

			public int _nRequest = 0;

			public RenderRequest(Texture2D image)
			{
				if(image == null)
				{
					_isImage = false;
					_image = null;
				}
				else
				{
					_isImage = true;
					_image = image;
				}

				_nRequest = 0;
			}

			public void SetImageSize(float sizeW, float sizeH)
			{
				_imageSize.x = sizeW;
				_imageSize.y = sizeH;
			}

			public void Clear()
			{
				_posList.Clear();
				_colorList.Clear();
				_linePos1List.Clear();
				_linePos2List.Clear();
				_nRequest = 0;
			}

			public void Add(Vector2 pos, Color color)
			{
				_posList.Add(pos);
				_colorList.Add(color);
				_nRequest++;
			}

			public void Add(Vector2 linePos1, Vector2 linePos2, Color color)
			{
				_linePos1List.Add(linePos1);
				_linePos2List.Add(linePos2);
				_colorList.Add(color);
				_nRequest++;
			}
		}


		private static RenderRequest _renderRequest_Keyframe = null;
		private static RenderRequest _renderRequest_KeyframeDummy = null;
		private static RenderRequest _renderRequest_KeyframeLoopLeft = null;
		private static RenderRequest _renderRequest_KeyframeLoopRight = null;

		private static RenderRequest _renderRequest_Cursor = null;

		private static RenderRequest _renderRequest_MoveBoldLine = null;
		private static RenderRequest _renderRequest_KeyframeMove = null;

		// Init
		//-------------------------------------------------------------------------------------------------------
		public static void SetShader(Shader shader_Color,
									Shader[] shader_Texture_Normal_Set,
									Shader[] shader_Texture_VColorAdd_Set,
									//Shader[] shader_MaskedTexture_Set,
									Shader shader_MaskOnly,
									Shader[] shader_Clipped_Set,
									Shader shader_GUITexture,
									Shader shader_ToneColor_Normal,
									Shader shader_ToneColor_Clipped,
									Shader shader_Alpha2White)
		{
			_matBatch_Total.SetShader(shader_Color, shader_Texture_Normal_Set, shader_Texture_VColorAdd_Set, /*shader_MaskedTexture_Set, */shader_MaskOnly, shader_Clipped_Set, shader_GUITexture, shader_ToneColor_Normal, shader_ToneColor_Clipped, shader_Alpha2White);
			_matBatch_Main.SetShader(shader_Color, shader_Texture_Normal_Set, shader_Texture_VColorAdd_Set, /*shader_MaskedTexture_Set, */shader_MaskOnly, shader_Clipped_Set, shader_GUITexture, shader_ToneColor_Normal, shader_ToneColor_Clipped, shader_Alpha2White);
			_matBatch_Header.SetShader(shader_Color, shader_Texture_Normal_Set, shader_Texture_VColorAdd_Set, /*shader_MaskedTexture_Set, */shader_MaskOnly, shader_Clipped_Set, shader_GUITexture, shader_ToneColor_Normal, shader_ToneColor_Clipped, shader_Alpha2White);
		}

		public static void SetTexture(Texture2D img_KeyFrame,
										Texture2D img_KeyFrameDummy,
										Texture2D img_KeySummary,
										Texture2D img_KeySummaryMove,
										Texture2D img_PlayBarHead,
										Texture2D img_KeyLoopLeft,
										Texture2D img_KeyLoopRight,
										Texture2D img_TimelineBGStart,
										Texture2D img_TimelineBGEnd,
										Texture2D img_CurKeyframe,
										Texture2D img_KeyframeCursor,
										Texture2D img_EventMark,
										Texture2D img_OnionMark
										//Texture2D img_KeyframeMoveSrc,
										//Texture2D img_KeyframeMove,
										//Texture2D img_KeyframeCopy
										)
		{
			_img_Keyframe = img_KeyFrame;
			_img_KeyframeDummy = img_KeyFrameDummy;
			_img_KeySummary = img_KeySummary;
			_img_KeySummaryMove = img_KeySummaryMove;

			_img_PlayBarHead = img_PlayBarHead;
			_img_KeyLoopLeft = img_KeyLoopLeft;
			_img_KeyLoopRight = img_KeyLoopRight;
			_img_CurKeyframe = img_CurKeyframe;
			_img_KeyframeCursor = img_KeyframeCursor;

			_img_EventMark = img_EventMark;
			_img_OnionMark = img_OnionMark;

			//_img_KeyframeMoveSrc = img_KeyframeMoveSrc;
			//_img_KeyframeMove = img_KeyframeMove;
			//_img_KeyframeCopy = img_KeyframeCopy;

			_img_TimelineBGStart = img_TimelineBGStart;
			_img_TimelineBGEnd = img_TimelineBGEnd;

			_renderRequest_Keyframe = new RenderRequest(_img_Keyframe);
			_renderRequest_KeyframeDummy = new RenderRequest(_img_KeyframeDummy);
			_renderRequest_KeyframeLoopLeft = new RenderRequest(_img_KeyLoopLeft);
			_renderRequest_KeyframeLoopRight = new RenderRequest(_img_KeyLoopRight);

			_renderRequest_Cursor = new RenderRequest(_img_KeyframeCursor);

			_renderRequest_MoveBoldLine = new RenderRequest(null);
			_renderRequest_KeyframeMove = new RenderRequest(_img_Keyframe);//<<이미지는 같지만 렌더링 순서가 다르다
		}

		public static void SetLayoutSize(int layoutWidth, int layoutHeight_Header, int layoutHeight_Main,
											int posX, int posY_Header, int posY_Main,
											int totalEditorWidth, int totalEditorHeight,
											bool isMainVisible,
											Vector2 scrollPos)
		{
			_layoutPosX = posX;
			_layoutPosY_Header = posY_Header;
			//_layoutPosY_Main = posY_Main;
			_layoutWidth = layoutWidth;
			_layoutHeight_Header = layoutHeight_Header + 1;
			_layoutHeight_Main = layoutHeight_Main + 2;

			_layoutHeight_Total = _layoutHeight_Header + _layoutHeight_Main;

			//_isMainVisible = isMainVisible;

			_scrollPos = scrollPos;
			if(_isRefreshScrollDown)
			{
				_scroll_Down = _scrollPos;
				_isRefreshScrollDown = false;
			}

			//원래는 30
			totalEditorHeight += 28;
			posY_Header += 28;
			posY_Main += 28;

			posX += 5;
			//layoutWidth -= 25;
			layoutWidth -= 17;
			//layoutHeight -= 20; //?

			//헤더
			_glScreenClippingSize_Header.x = (float)posX / (float)totalEditorWidth;
			_glScreenClippingSize_Header.y = (float)(posY_Header) / (float)totalEditorHeight;
			_glScreenClippingSize_Header.z = (float)(posX + layoutWidth) / (float)totalEditorWidth;
			_glScreenClippingSize_Header.w = (float)(posY_Header + _layoutHeight_Header) / (float)totalEditorHeight;

			//메인
			_glScreenClippingSize_Main.x = (float)posX / (float)totalEditorWidth;
			_glScreenClippingSize_Main.y = (float)(posY_Main) / (float)totalEditorHeight;
			_glScreenClippingSize_Main.z = (float)(posX + layoutWidth) / (float)totalEditorWidth;
			_glScreenClippingSize_Main.w = (float)(posY_Main + _layoutHeight_Main) / (float)totalEditorHeight;

			//전체
			_glScreenClippingSize_Total.x = (float)posX / (float)totalEditorWidth;
			_glScreenClippingSize_Total.y = (float)(posY_Header) / (float)totalEditorHeight;
			_glScreenClippingSize_Total.z = (float)(posX + layoutWidth) / (float)totalEditorWidth;
			_glScreenClippingSize_Total.w = (float)(posY_Header + _layoutHeight_Total) / (float)totalEditorHeight;

			//_isNeedPreRender = true;
			_isMouseEvent = false;
			_isMouseEventUsed = false;
			_isRefreshScrollDown = false;
		}


		public static void SetMouseValue(bool isLeftBtnPressed,
											bool isRightBtnPressed,
											Vector2 mousePos,
											bool isShift, bool isCtrl, bool isAlt,
											EventType curEventType,
											apSelection selection)
		{
			_isMouseEvent = true;
			_isMouseEventUsed = false;

			_mousePos = mousePos;

			_mousePos.x -= _layoutPosX;
			_mousePos.y -= _layoutPosY_Header;

			_isShift = isShift;
			_isCtrl = isCtrl;
			_isAlt = isAlt;

			bool isMouseDown =		Event.current.rawType == EventType.MouseDown;

			bool isMousePressed =	Event.current.rawType == EventType.MouseDown ||
									Event.current.rawType == EventType.MouseDrag;

			bool isMouseEvent =		Event.current.rawType == EventType.MouseDown ||
									Event.current.rawType == EventType.MouseMove ||
									Event.current.rawType == EventType.MouseUp ||
									Event.current.rawType == EventType.MouseDrag;


			//_curEventType = curEventType;
			_selection = selection;

			_isMouseEvent = isMouseEvent;

			if (_selection.SelectionType != apSelection.SELECTION_TYPE.Animation)
			{
				_isMouseEvent = false;
				_leftBtnStatus = apMouse.MouseBtnStatus.Released;
				_rightBtnStatus = apMouse.MouseBtnStatus.Released;
			}


			if(!isLeftBtnPressed && !isRightBtnPressed)
			{
				isMouseDown = false;
				isMousePressed = false;
			}

			
			
			if (isMouseEvent)
			{
				//if (isLeftBtnPressed || isRightBtnPressed)
				if(isMousePressed)
				{
					if (_isMouseIgnored_ExternalDrag)
					{
						//무시..
					}
					else
					{
						//첫 클릭때 체크하자
						//bool isMouseDown = false;
						//if (isLeftBtnPressed && (_leftBtnStatus == apMouse.MouseBtnStatus.Up || _leftBtnStatus == apMouse.MouseBtnStatus.Released))
						//{
						//	isMouseDown = true;
						//}

						//if (isRightBtnPressed && (_rightBtnStatus == apMouse.MouseBtnStatus.Up || _rightBtnStatus == apMouse.MouseBtnStatus.Released))
						//{
						//	isMouseDown = true;
						//}

						//막 눌리기 시작했을때
						if (isMouseDown)
						{
							if (IsMouseInLayout(_mousePos))
							{
								//유효한 마우스 이벤트이다.
								//새로운 Down이벤트에서는 Up/Drag 이벤트를 무시하는 변수를 초기화
								_isMouseIgnored_UpOrUsed = false;
								
							}
							else
							{
								_isMouseIgnored_ExternalDrag = true;//<<이 부분에서 마우스 입력이 시작되지 않았다. 다음 업데이트는 무시
							}
						}
					}
				}
				else
				{
					if (_isMouseIgnored_ExternalDrag)
					{
						_isMouseIgnored_ExternalDrag = false;
					}
				}
			}

			//apMouse.MouseBtnStatus prevLeftBtnStatus = _leftBtnStatus;

			if (isMouseEvent)
			{
				if (!_isMouseIgnored_ExternalDrag && !_isMouseIgnored_UpOrUsed)
				{
					//유효한 마우스 이벤트일때
					if (isLeftBtnPressed)
					{
						if (_leftBtnStatus == apMouse.MouseBtnStatus.Down || _leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
						{
							_leftBtnStatus = apMouse.MouseBtnStatus.Pressed;
						}
						else
						{
							if (isMouseDown)
							{
								_leftBtnStatus = apMouse.MouseBtnStatus.Down;
							}
						}
					}
					else
					{
						if (_leftBtnStatus == apMouse.MouseBtnStatus.Down || _leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
						{
							_leftBtnStatus = apMouse.MouseBtnStatus.Up;
						}
						else
						{
							_leftBtnStatus = apMouse.MouseBtnStatus.Released;
						}
					}


					if (isRightBtnPressed)
					{
						if (_rightBtnStatus == apMouse.MouseBtnStatus.Down || _rightBtnStatus == apMouse.MouseBtnStatus.Pressed)
						{
							_rightBtnStatus = apMouse.MouseBtnStatus.Pressed;
						}
						else
						{
							if (isMouseDown)
							{
								_rightBtnStatus = apMouse.MouseBtnStatus.Down;
							}
						}
					}
					else
					{
						if (_rightBtnStatus == apMouse.MouseBtnStatus.Down || _rightBtnStatus == apMouse.MouseBtnStatus.Pressed)
						{
							_rightBtnStatus = apMouse.MouseBtnStatus.Up;
						}
						else
						{
							_rightBtnStatus = apMouse.MouseBtnStatus.Released;
						}
					}

					//마우스 Down시 입력 위치 갱신
					if(isMouseDown)
					{
						_mousePos_Down = _mousePos;

						_scroll_Down = _scrollPos;

						if (_mousePos_Down.y < _layoutHeight_Header)
						{
							_downClipArea = CLIP_TYPE.Header;
						}
						else
						{
							_downClipArea = CLIP_TYPE.Main;
						}
					}
				}
			}
			else
			{
				//마우스 이벤트가 아닐때 => Repaint로 한정
				if (curEventType == EventType.Repaint)
				{
					//Down -> Pressed
					//Up -> Release로 전환하자
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Down)
					{
						_leftBtnStatus = apMouse.MouseBtnStatus.Pressed;
					}
					else if (_leftBtnStatus == apMouse.MouseBtnStatus.Up)
					{
						_leftBtnStatus = apMouse.MouseBtnStatus.Released;
					}

					if(_rightBtnStatus == apMouse.MouseBtnStatus.Down)
					{
						_rightBtnStatus = apMouse.MouseBtnStatus.Pressed;
					}
					else if (_rightBtnStatus == apMouse.MouseBtnStatus.Up)
					{
						_rightBtnStatus = apMouse.MouseBtnStatus.Released;
					}
				}
			}




			if (_isShift || _isCtrl)
			{
				_selectType = SELECT_TYPE.Add;
			}
			else if (_isAlt)
			{
				_selectType = SELECT_TYPE.Subtract;
			}
			else
			{
				_selectType = SELECT_TYPE.New;
			}

			
			//if(isMouseDown)
			//{
			//	Debug.Log("Mouse Down : Ext = " + _isMouseIgnored_ExternalDrag + " / Used = " + _isMouseIgnored_UpOrUsed);
			//	Debug.Log("Is Mouse Event : " + _isMouseEvent + " / Mouse Used : " + _isMouseEventUsed);
			//}

			if (_isMouseIgnored_ExternalDrag || _isMouseIgnored_UpOrUsed)
			{
				_isMouseEvent = false;
				_isMouseEventUsed = true;
				
			}

			//if (prevLeftBtnStatus != _leftBtnStatus || (_isMouseEventUsed && _leftBtnStatus == apMouse.MouseBtnStatus.Pressed))
			//{
			//	if (_isMouseEventUsed)
			//	{
			//		Debug.LogError(_leftBtnStatus + " : EventUsed : " + _isMouseEventUsed);
			//	}
			//	else
			//	{
			//		Debug.Log(_leftBtnStatus.ToString());
			//	}

			//}
		}

		/// <summary>
		/// 외부에서 Mouse이벤트를 차단할때의 함수. 주로 휠클릭 처리시 호출한다.
		/// </summary>
		public static void SetMouseUse()
		{
			_isMouseEventUsed = true;
			_isRefreshScrollDown = true;
			_isMouseIgnored_UpOrUsed = true;
		}

		public static void RefreshScrollDown()
		{
			_isRefreshScrollDown = true;
		}
		
		

		//public static void UpdateMouseEvent()
		//{
		//	if(_isMouseEventUsed || !_isMouseEvent
		//		|| _selection == null
		//		|| _selection.AnimClip == null)
		//	{
		//		_timelineEvent = TIMELINE_EVENT.None;//걍 취소
		//		return;
		//	}

		//	switch (_timelineEvent)
		//	{
		//		case TIMELINE_EVENT.None:
		//			{
		//				if(_leftBtnStatus == apMouse.MouseBtnStatus.Down)
		//				{
		//					//Down 체크 순서
		//					//PlayBar -> Frame -> 빈칸

		//				}
		//			}
		//			break;

		//		case TIMELINE_EVENT.Select:
		//			{

		//			}
		//			break;

		//		case TIMELINE_EVENT.DragFrame:
		//			{

		//			}
		//			break;

		//		case TIMELINE_EVENT.DragPlayBar:
		//			{

		//			}
		//			break;
		//	}
		//}


		//-------------------------------------------------------------------------------------------------------
		private static apGL.MaterialBatch GetMatBatch(CLIP_TYPE clipType)
		{
			switch (clipType)
			{
				case CLIP_TYPE.Total:	return _matBatch_Total;
				case CLIP_TYPE.Main:	return _matBatch_Main;
				case CLIP_TYPE.Header:	return _matBatch_Header;
			}
			return null;
		}

		// Begin / End Batch
		//--------------------------------------------------------------------------------------------------
		public static void BeginBatch_GUITexture(Texture2D texture, CLIP_TYPE clipType)
		{
			apGL.MaterialBatch matBatch = GetMatBatch(clipType);
			if (matBatch == null) { return; }
			if (matBatch.IsNotReady()) { return; }

			matBatch.SetPass_GUITexture(texture);
			switch (clipType)
			{
				case CLIP_TYPE.Header:
					matBatch.SetClippingSize(_glScreenClippingSize_Header);
					break;

				case CLIP_TYPE.Main:
					matBatch.SetClippingSize(_glScreenClippingSize_Main);
					break;

				case CLIP_TYPE.Total:
					matBatch.SetClippingSize(_glScreenClippingSize_Total);
					break;
			}

			GL.Begin(GL.TRIANGLES);
		}

		public static void BeginBatch_Color(CLIP_TYPE clipType)
		{
			apGL.MaterialBatch matBatch = GetMatBatch(clipType);
			if (matBatch == null) { return; }
			if (matBatch.IsNotReady()) { return; }

			matBatch.SetPass_Color();
			switch (clipType)
			{
				case CLIP_TYPE.Header:
					matBatch.SetClippingSize(_glScreenClippingSize_Header);
					break;

				case CLIP_TYPE.Main:
					matBatch.SetClippingSize(_glScreenClippingSize_Main);
					break;

				case CLIP_TYPE.Total:
					matBatch.SetClippingSize(_glScreenClippingSize_Total);
					break;
			}


			GL.Begin(GL.TRIANGLES);
		}

		public static void EndBatch()
		{
			GL.End();
		}

		// Draw Line
		//-------------------------------------------------------------------------------------------------------
		public static void DrawLine(Vector2 pos1, Vector2 pos2, Color color, bool isNeedResetMat, CLIP_TYPE clipType)
		{
			apGL.MaterialBatch matBatch = GetMatBatch(clipType);
			if (matBatch == null)
			{ return; }
			if (matBatch.IsNotReady())
			{ return; }

			if (Vector2.Equals(pos1, pos2))
			{
				return;
			}

			if (isNeedResetMat)
			{
				matBatch.SetPass_Color();
				switch (clipType)
				{
					case CLIP_TYPE.Header:
						matBatch.SetClippingSize(_glScreenClippingSize_Header);
						break;

					case CLIP_TYPE.Main:
						matBatch.SetClippingSize(_glScreenClippingSize_Main);
						break;

					case CLIP_TYPE.Total:
						matBatch.SetClippingSize(_glScreenClippingSize_Total);
						break;
				}


				GL.Begin(GL.LINES);


			}

			GL.Color(color);
			GL.Vertex(new Vector3(pos1.x, pos1.y, 0.0f));
			GL.Vertex(new Vector3(pos2.x, pos2.y, 0.0f));

			if (isNeedResetMat)
			{
				GL.End();
			}
		}

		// Draw Box
		//---------------------------------------------------------------------------------------------------------
		public static void DrawBox(Vector2 pos, float width, float height, Color color, bool isNeedResetMat, CLIP_TYPE clipType)
		{
			apGL.MaterialBatch matBatch = GetMatBatch(clipType);
			if (matBatch == null)
			{ return; }
			if (matBatch.IsNotReady())
			{ return; }

			float halfWidth = width * 0.5f;
			float halfHeight = height * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector3 pos_0 = new Vector3(pos.x - halfWidth, pos.y - halfHeight, 0);
			Vector3 pos_1 = new Vector3(pos.x + halfWidth, pos.y - halfHeight, 0);
			Vector3 pos_2 = new Vector3(pos.x + halfWidth, pos.y + halfHeight, 0);
			Vector3 pos_3 = new Vector3(pos.x - halfWidth, pos.y + halfHeight, 0);

			//CW
			// -------->
			// | 0   1
			// | 		
			// | 3   2

			if (isNeedResetMat)
			{
				matBatch.SetPass_Color();
				switch (clipType)
				{
					case CLIP_TYPE.Header:
						matBatch.SetClippingSize(_glScreenClippingSize_Header);
						break;

					case CLIP_TYPE.Main:
						matBatch.SetClippingSize(_glScreenClippingSize_Main);
						break;

					case CLIP_TYPE.Total:
						matBatch.SetClippingSize(_glScreenClippingSize_Total);
						break;
				}

				GL.Begin(GL.TRIANGLES);
			}
			GL.Color(color);
			GL.Vertex(pos_0); // 0
			GL.Vertex(pos_1); // 1
			GL.Vertex(pos_2); // 2

			GL.Vertex(pos_2); // 2
			GL.Vertex(pos_3); // 3
			GL.Vertex(pos_0); // 0

			if (isNeedResetMat)
			{
				GL.End();
			}
		}



		// Draw Texture
		//--------------------------------------------------------------------------------------------------
		public static void DrawTexture(Texture2D image, Vector2 pos, float width, float height, Color color2X, bool isNeedResetMat, CLIP_TYPE clipType)
		{
			apGL.MaterialBatch matBatch = GetMatBatch(clipType);
			if (matBatch == null) { return; }
			if (matBatch.IsNotReady()) { return; }

			float realWidth = width;
			float realHeight = height;

			float realWidth_Half = realWidth * 0.5f;
			float realHeight_Half = realHeight * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0 = new Vector2(pos.x - realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_1 = new Vector2(pos.x + realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_2 = new Vector2(pos.x + realWidth_Half, pos.y + realHeight_Half);
			Vector2 pos_3 = new Vector2(pos.x - realWidth_Half, pos.y + realHeight_Half);

			float widthResize = (pos_1.x - pos_0.x);
			float heightResize = (pos_3.y - pos_0.y);

			if (widthResize < 1.0f || heightResize < 1.0f)
			{
				return;
			}

			float u_left = 0.0f;
			float u_right = 1.0f;

			float v_top = 0.0f;
			float v_bottom = 1.0f;

			Vector3 uv_0 = new Vector3(u_left, v_bottom, 0.0f);
			Vector3 uv_1 = new Vector3(u_right, v_bottom, 0.0f);
			Vector3 uv_2 = new Vector3(u_right, v_top, 0.0f);
			Vector3 uv_3 = new Vector3(u_left, v_top, 0.0f);

			//CW
			// -------->
			// | 0   1
			// | 		
			// | 3   2
			if (isNeedResetMat)
			{
				matBatch.SetPass_GUITexture(image);
				switch (clipType)
				{
					case CLIP_TYPE.Header:
						matBatch.SetClippingSize(_glScreenClippingSize_Header);
						break;

					case CLIP_TYPE.Main:
						matBatch.SetClippingSize(_glScreenClippingSize_Main);
						break;

					case CLIP_TYPE.Total:
						matBatch.SetClippingSize(_glScreenClippingSize_Total);
						break;
				}

				GL.Begin(GL.TRIANGLES);
			}
			GL.Color(color2X);	
			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, 0)); // 0
			GL.TexCoord(uv_1);	GL.Vertex(new Vector3(pos_1.x, pos_1.y, 0)); // 1
			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, 0)); // 2

			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, 0)); // 2
			GL.TexCoord(uv_3);	GL.Vertex(new Vector3(pos_3.x, pos_3.y, 0)); // 3
			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, 0)); // 0

			if (isNeedResetMat)
			{
				GL.End();
			}

			//GL.Flush();
		}


		public static void DrawBoldLine(Vector2 pos1, Vector2 pos2, float width, Color color, bool isNeedResetMat, CLIP_TYPE clipType)
		{
			apGL.MaterialBatch matBatch = null;
			if (isNeedResetMat)
			{
				matBatch = GetMatBatch(clipType);
				if (matBatch == null)
				{ return; }
				if (matBatch.IsNotReady())
				{ return; }
			}

			if (pos1 == pos2)
			{
				return;
			}

			//float halfWidth = width * 0.5f / _zoom;
			float halfWidth = width * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)

			// -------->
			// |    1
			// | 0     2
			// | 
			// | 
			// | 
			// | 5     3
			// |    4

			Vector2 dir = (pos1 - pos2).normalized;
			Vector2 dirRev = new Vector2(-dir.y, dir.x);

			Vector2 pos_0 = pos1 - dirRev * halfWidth;
			Vector2 pos_1 = pos1 + dir * halfWidth;
			//Vector2 pos_1 = pos1;
			Vector2 pos_2 = pos1 + dirRev * halfWidth;

			Vector2 pos_3 = pos2 + dirRev * halfWidth;
			Vector2 pos_4 = pos2 - dir * halfWidth;
			//Vector2 pos_4 = pos2;
			Vector2 pos_5 = pos2 - dirRev * halfWidth;

			if (isNeedResetMat)
			{
				//_mat_Color.SetPass(0);
				//_mat_Color.SetVector("_ScreenSize", _glScreenClippingSize);
				matBatch.SetPass_Color();
				switch (clipType)
				{
					case CLIP_TYPE.Header:
						matBatch.SetClippingSize(_glScreenClippingSize_Header);
						break;

					case CLIP_TYPE.Main:
						matBatch.SetClippingSize(_glScreenClippingSize_Main);
						break;

					case CLIP_TYPE.Total:
						matBatch.SetClippingSize(_glScreenClippingSize_Total);
						break;
				}


				GL.Begin(GL.TRIANGLES);
			}

			GL.Color(color);
			GL.Vertex(pos_0); // 0
			GL.Vertex(pos_1); // 1
			GL.Vertex(pos_2); // 2

			GL.Vertex(pos_2); // 2
			GL.Vertex(pos_1); // 1
			GL.Vertex(pos_0); // 0

			GL.Vertex(pos_0); // 0
			GL.Vertex(pos_2); // 2
			GL.Vertex(pos_3); // 3

			GL.Vertex(pos_3); // 3
			GL.Vertex(pos_2); // 2
			GL.Vertex(pos_0); // 0

			GL.Vertex(pos_3); // 3
			GL.Vertex(pos_5); // 5
			GL.Vertex(pos_0); // 0

			GL.Vertex(pos_0); // 0
			GL.Vertex(pos_5); // 5
			GL.Vertex(pos_3); // 3

			GL.Vertex(pos_3); // 3
			GL.Vertex(pos_4); // 4
			GL.Vertex(pos_5); // 5

			GL.Vertex(pos_5); // 5
			GL.Vertex(pos_4); // 4
			GL.Vertex(pos_3); // 3

			if (isNeedResetMat)
			{
				GL.End();
			}
		}


		public static void DrawBoldArea(Vector2 startPos, Vector2 endPos, float lineThickness, Color color, CLIP_TYPE clipType)
		{
			apGL.MaterialBatch matBatch = GetMatBatch(clipType);
			if (matBatch == null)
			{ return; }
			if (matBatch.IsNotReady())
			{ return; }

			float min_X = Mathf.Max(startPos.x, endPos.x);
			float max_X = Mathf.Min(startPos.x, endPos.x);

			float min_Y = Mathf.Max(startPos.y, endPos.y);
			float max_Y = Mathf.Min(startPos.y, endPos.y);

			matBatch.SetPass_Color();
			switch (clipType)
			{
				case CLIP_TYPE.Header:
					matBatch.SetClippingSize(_glScreenClippingSize_Header);
					break;

				case CLIP_TYPE.Main:
					matBatch.SetClippingSize(_glScreenClippingSize_Main);
					break;

				case CLIP_TYPE.Total:
					matBatch.SetClippingSize(_glScreenClippingSize_Total);
					break;
			}


			GL.Begin(GL.TRIANGLES);

			DrawBoldLine(new Vector2(min_X, min_Y), new Vector2(max_X, min_Y), lineThickness, color, false, clipType);
			DrawBoldLine(new Vector2(max_X, min_Y), new Vector2(max_X, max_Y), lineThickness, color, false, clipType);
			DrawBoldLine(new Vector2(max_X, max_Y), new Vector2(min_X, max_Y), lineThickness, color, false, clipType);
			DrawBoldLine(new Vector2(min_X, max_Y), new Vector2(min_X, min_Y), lineThickness, color, false, clipType);

			GL.End();
		}

		//----------------------------------------------------------------------------------------
		// Draw Text
		//----------------------------------------------------------------------------------------
		public static void DrawText(string text, Vector2 pos, float width, Color color, CLIP_TYPE clipType)
		{
			if (pos.x < 0)
			{
				return;
			}

			if (pos.x + (width) > _layoutWidth)
			{
				return;
			}
			_textStyle.normal.textColor = color;
			GUI.Label(new Rect(pos.x, pos.y, width, 30), text, _textStyle);
		}

		public static void DrawNumber(string number, Vector2 pos, Color color, CLIP_TYPE clipType)
		{
			//float width = GetIntWidth(number);
			float width = number.Length * 7;
			DrawText(number, pos - new Vector2(width / 2, 0), width, color, clipType);
		}

		#region [미사용 코드]
		//private static float GetIntWidth(int number)
		//{
		//	float length = 0.0f;

		//	int subNumber = 0;
		//	float baseWidth = 7.0f;
		//	while(true)
		//	{
		//		subNumber = number % 10;
		//		switch (subNumber)
		//		{	
		//			case 1: length += 1.0f;
		//				break;
		//			case 0:
		//			case 2:
		//			case 3:
		//			case 4:
		//			case 5:
		//			case 6:
		//			case 7:
		//			case 8:
		//			case 9:
		//				length += 1.0f;
		//				break;
		//		}

		//		if(number < 10)
		//		{
		//			break;
		//		}

		//		number /= 10;
		//	}

		//	return length * baseWidth;
		//} 
		#endregion

		//----------------------------------------------------------------------------------------

		//----------------------------------------------------------------------------------------
		//private static int _curFrame = 0;
		private static int _startFrame = 0;
		private static int _endFrame = 0;
		private static int _widthPerFrame = 0;
		//private static bool _isLoop = false;
		private static int _mainFrameUnit = 5;
		public static void SetTimelineSetting(int curFrame, int startFrame, int endFrame, int widthPerFrame, bool isLoop)
		{
			//_curFrame = curFrame;
			_startFrame = startFrame;
			_endFrame = endFrame;
			_widthPerFrame = widthPerFrame;
			//_isLoop = isLoop;

			//4자리 수를 기준으로 하자 //(4자리수 숫자의 길이 = 4 x 7 = 28)
			if (28 < widthPerFrame * 1)
			{
				_mainFrameUnit = 1;
			}
			else if (28 < widthPerFrame * 2)
			{
				_mainFrameUnit = 2;
			}
			else if (28 < widthPerFrame * 5)
			{
				_mainFrameUnit = 5;
			}
			else if (28 < widthPerFrame * 10)
			{
				_mainFrameUnit = 10;
			}
			else if (28 < widthPerFrame * 50)
			{
				_mainFrameUnit = 50;
			}
			else
			{
				_mainFrameUnit = 100;
			}

		}

		private const int X_OFFSET = 30;


		public static void DrawTimelineAreaBG(bool isEditing)
		{
			float startPosX = X_OFFSET - _scrollPos.x;
			float endPosX = FrameToPosX_Main(_endFrame);

			if (endPosX < startPosX)
			{
				return;
			}

			float baseSize = 120;
			if (endPosX - startPosX < baseSize)
			{
				baseSize = endPosX - startPosX;
			}

			Vector2 startPos = new Vector2(startPosX + baseSize / 2, _layoutHeight_Header / 2);
			Vector2 endPos = new Vector2(endPosX - baseSize / 2, _layoutHeight_Header / 2);
			Color bgColor = new Color(0.1f, 0.3f, 0.6f, 0.7f);
			if (isEditing)
			{
				bgColor = new Color(0.6f, 0.1f, 0.1f, 0.7f);
			}
			DrawTexture(_img_TimelineBGStart, startPos, baseSize, _layoutHeight_Header, bgColor, true, CLIP_TYPE.Header);
			DrawTexture(_img_TimelineBGEnd, endPos, baseSize, _layoutHeight_Header, bgColor, true, CLIP_TYPE.Header);
		}


		public static void DrawTimeGrid(Color lineColorMain, Color lineColorSub, Color numberColor)
		{
			Vector2 startPos = new Vector2(X_OFFSET - _scrollPos.x, 0);
			Vector2 curPos = startPos;
			int nCnt = 0;
			int startNumber = _startFrame;
			int curNumber = startNumber;
			//Batch를 하자

			_matBatch_Total.SetPass_Color();
			_matBatch_Total.SetClippingSize(_glScreenClippingSize_Total);
			GL.Begin(GL.LINES);


			Vector3 linePos1 = Vector3.zero, linePos2 = Vector3.zero;
			while (true)
			{
				if (curPos.x > 0)
				{
					if (curNumber % _mainFrameUnit == 0)
					{
						//DrawNumber(curNumber.ToString(), curPos, numberColor, CLIP_TYPE.Header);
						//DrawLine(curPos + new Vector2(0, 14), curPos + new Vector2(0, _layoutHeight_Total), lineColor, false, CLIP_TYPE.Total);
						GL.Color(lineColorMain);
						linePos1 = curPos + new Vector2(0, 14);
						linePos2 = curPos + new Vector2(0, _layoutHeight_Total);
					}
					else
					{
						//DrawLine(curPos + new Vector2(0, 20), curPos + new Vector2(0, _layoutHeight_Total), lineColor, false, CLIP_TYPE.Total);
						GL.Color(lineColorSub);
						linePos1 = curPos + new Vector2(0, 20);
						linePos2 = curPos + new Vector2(0, _layoutHeight_Total);
					}

					GL.Vertex(linePos1);
					GL.Vertex(linePos2);

					nCnt++;
				}

				curPos.x += _widthPerFrame;

				curNumber++;

				if (nCnt > 500)
				{
					break;
				}

				if (curPos.x > _layoutWidth)
				{
					break;
				}
			}

			GL.End();


			//Number도 출력하자
			curPos = startPos;
			nCnt = 0;
			startNumber = _startFrame;
			curNumber = startNumber;

			while (true)
			{
				if (curPos.x > 0)
				{
					if (curNumber % _mainFrameUnit == 0)
					{
						DrawNumber(curNumber.ToString(), curPos, numberColor, CLIP_TYPE.Header);
					}

					nCnt++;
				}

				curPos.x += _widthPerFrame;

				curNumber++;

				if (nCnt > 500)
				{
					break;
				}

				if (curPos.x > _layoutWidth)
				{
					break;
				}
			}

		}
		public static void DrawTimeBars_Header(Color lineColor)
		{
			DrawBox(new Vector2(_layoutWidth / 2, _layoutHeight_Header - 2), _layoutWidth, 4, lineColor, true, CLIP_TYPE.Header);

			//DrawBox(new Vector2(_layoutWidth / 2, _layoutHeight_Header / 2 - _scrollPos.y), _layoutWidth, _layoutHeight_Header, Color.green, true, CLIP_TYPE.Header);
			//DrawBox(new Vector2(_layoutWidth / 2, _layoutHeight_Header + (_layoutHeight_Main / 2) - _scrollPos.y), _layoutWidth, _layoutHeight_Main, Color.red, true, CLIP_TYPE.Main);

		}

		public static void DrawTimeBars_MainBG(Color bgColor, int posY, int height)
		{
			DrawBox(new Vector2(_layoutWidth / 2, posY + _layoutHeight_Header - (height / 2)), _layoutWidth, height - 1, bgColor, true, CLIP_TYPE.Main);
		}

		public static void DrawTimeBars_MainLine(Color lineColor, int posY)
		{
			DrawLine(new Vector2(0, posY + _layoutHeight_Header), new Vector2(_layoutWidth, posY + _layoutHeight_Header), lineColor, true, CLIP_TYPE.Main);
		}

		private static Vector2 _dragAreaStartPos = Vector2.zero;
		private static Vector2 _dragAreaEndPos = Vector2.zero;


		public static void DrawAndUpdateSelectArea()
		{
			
			if (_isMouseEventUsed)
			{
				//무시
				return;
			}

			//DrawBox(_mousePos, 20, 20, Color.red, true, CLIP_TYPE.Total);
			//Vector2 areaStartPos = Vector2.zero;
			//Vector2 areaEndPos = Vector2.zero;

			if (_timelineEvent == TIMELINE_EVENT.Select)
			{
				_dragAreaStartPos = _mousePos_Down - (_scrollPos - _scroll_Down);
				_dragAreaEndPos = _mousePos;

				if (_downClipArea == CLIP_TYPE.Header)
				{
					_dragAreaStartPos.y = Mathf.Clamp(_dragAreaStartPos.y, 0, _layoutHeight_Header - 3);
					_dragAreaEndPos.y = Mathf.Clamp(_dragAreaEndPos.y, 0, _layoutHeight_Header - 3);
				}
				else if (_downClipArea == CLIP_TYPE.Main)
				{
					//areaStartPos.y = Mathf.Max(areaStartPos.y, _layoutHeight_Header + 2);
					_dragAreaEndPos.y = Mathf.Max(_dragAreaEndPos.y, _layoutHeight_Header + 2);
				}
			}

			if (_isMouseEvent && !_isMouseEventUsed)
			{
				if (_timelineEvent == TIMELINE_EVENT.None)
				{
					//1. 선택이 안되었을 때 -> 
					if (IsMouseUpdatable_Down(true))
					{
						SetTimelineEvent(TIMELINE_EVENT.Select);
					}
				}
				else if (_timelineEvent == TIMELINE_EVENT.Select || _timelineEvent == TIMELINE_EVENT.ReadyToDrag)
				{
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Up || _leftBtnStatus == apMouse.MouseBtnStatus.Released)
					{
						SetTimelineEvent(TIMELINE_EVENT.None);
					}
					else
					{
						//프레임 선택을 해야한다.
						//일단 그리기부터
					}
				}
			}

			if (_timelineEvent == TIMELINE_EVENT.Select && !_isKeyframeClicked)
			{
				Color lineColor = Color.black;
				switch (_selectType)
				{
					case SELECT_TYPE.New:
						lineColor = new Color(0.0f, 1.0f, 0.5f, 0.9f);
						break;

					case SELECT_TYPE.Add:
						lineColor = new Color(0.0f, 0.5f, 1.0f, 0.9f);
						break;

					case SELECT_TYPE.Subtract:
						lineColor = new Color(1.0f, 0.0f, 0.0f, 0.9f);
						break;
				}

				DrawBoldArea(_dragAreaStartPos, _dragAreaEndPos, 3.0f, lineColor, _downClipArea);
			}
		}

		//--------------------------------------------------------------------------
		/// <summary>
		/// PlayBar를 출력한다.
		/// 만약, 마우스 입력으로 Frame을 바꾸었다면 True를 리턴한다.
		/// </summary>
		/// <param name="frame"></param>
		/// <returns></returns>
		public static bool DrawPlayBar(int frame)
		{
			Vector2 pos = FrameToPos_Main(frame, 0);
			pos.y = 0;
			//14:30
			//12:26
			int imgSizeWidth = 12;
			int imgSizeHeight = 26;
			//int yOffset = 12;
			int yOffset = 18;
			pos.y += (imgSizeHeight / 2) + yOffset;

			Color playBarColor = new Color(0.2f, 1.0f, 0.2f, 1.0f);
			Color lineColor = new Color(0.2f, 1.0f, 0.2f, 1.0f);

			if (_timelineEvent == TIMELINE_EVENT.DragPlayBar)
			{
				playBarColor = new Color(1.0f, 0.2f, 0.2f, 1.0f);
				lineColor = new Color(1.0f, 0.2f, 0.2f, 1.0f);
			}

			bool isChangeFrame = false;


			DrawLine(pos, pos + new Vector2(0.0f, _layoutHeight_Total), lineColor, true, CLIP_TYPE.Total);
			DrawTexture(_img_PlayBarHead, pos, imgSizeWidth, imgSizeHeight, playBarColor, true, CLIP_TYPE.Total);


			Vector2 cursorSelectPos = pos + new Vector2(0.0f, -yOffset / 2);

			float cursorSelectWidth = imgSizeWidth * 4;
			float cursorSelectHeight = imgSizeHeight + 8 + (yOffset / 2);
			AddCursorRect(cursorSelectPos, cursorSelectWidth, cursorSelectHeight, MouseCursor.ResizeHorizontal);

			if (_isMouseEvent && !_isMouseEventUsed)
			{
				//1. 선택이 안될때 -> 선택 체크
				if (_timelineEvent == TIMELINE_EVENT.None)
				{
					if (IsMouseUpdatable_Down(true))
					{
						if (IsTargetSelectable(_mousePos, cursorSelectPos, cursorSelectWidth, cursorSelectHeight))
						{
							SetTimelineEvent(TIMELINE_EVENT.DragPlayBar);
							_isMouseEventUsed = true;
						}
					}
				}

				//2. 드래그를 해보자
				else if (_timelineEvent == TIMELINE_EVENT.DragPlayBar)
				{
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Up || _leftBtnStatus == apMouse.MouseBtnStatus.Released)
					{
						SetTimelineEvent(TIMELINE_EVENT.None);
					}
					else
					{
						int posToFrame = Mathf.Clamp(PosToFrame(_mousePos.x), _startFrame, _endFrame);
						if (_selection != null && _selection.AnimClip != null)
						{
							int prevFrame = _selection.AnimClip.CurFrame;
							if (prevFrame != posToFrame)
							{
								_selection.AnimClip.SetFrame_Editor(posToFrame);
								isChangeFrame = true;
							}
						}
					}
				}
			}

			return isChangeFrame;
		}


		public static void DrawKeySummry(List<apAnimCommonKeyframe> commonKeyframes, float posY)
		{
			float posX = 0.0f;
			apAnimCommonKeyframe commonKeyframe = null;
			float size = 14;
			float clickSize = 8;
			Vector2 keyPos = Vector2.zero;
			Color keyColor = Color.black;

			//Batch하여 렌더링을 하자
			BeginBatch_GUITexture(_img_KeySummary, CLIP_TYPE.Header);

			for (int i = 0; i < commonKeyframes.Count; i++)
			{
				commonKeyframe = commonKeyframes[i];

				posX = FrameToPosX_Main(commonKeyframe._frameIndex);

				if (posX < -size || posX > _layoutWidth + size)
				{
					//화면 영역 밖으로 나갔다.
					continue;
				}

				//위치
				keyPos = new Vector2(posX - 0.5f, posY);

				//색상
				if(commonKeyframe._isSelected)
				{
					keyColor = _keyColor_Selected;	
				}
				else
				{
					keyColor = Color.gray;	
				}

				//일단 ResetMat 방식으로 하자
				//Batch를 해야하는데..
				DrawTexture(_img_KeySummary, keyPos, size, size, keyColor, false, CLIP_TYPE.Header);

				AddCursorRect(keyPos, clickSize, clickSize, MouseCursor.MoveArrow);


				if (_isMouseEvent && !_isMouseEventUsed)
				{
					if(_keyframeControlType == KEYFRAME_CONTROL_TYPE.SingleSelect)
					{
						//클릭으로 선택
						if(_selectableCommonKeyframes.Count == 0)
						{
							//아무것도 선택된게 없을 때
							if(_mousePos.y < _layoutHeight_Header)
							{
								if(_leftBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									if (!_isKeyframeClicked)
									{
										if (IsTargetSelectable(_mousePos, keyPos, clickSize, clickSize))
										{
											_selectableCommonKeyframes.Add(commonKeyframe);

											//선택된걸 다시 클릭했는가
											if (commonKeyframe._isSelected)
											{
												_isSelectedKeyframeClick = true;
											}
											else
											{
												_isSelectedKeyframeClick = false;
											}

											_isKeyframeClicked = true;
											_isSelectLoopDummy = false;

											_targetSelectType = TARGET_SELECT_TYPE.CommonKeyframe;
										}
									}
								}
							}
						}
					}
					else if(_keyframeControlType == KEYFRAME_CONTROL_TYPE.MultipleSelect)
					{
						//영역으로 선택
						if(_downClipArea == CLIP_TYPE.Header)
						{
							bool isAnySelected = false;
							if(!_isKeyframeClicked)
							{
								if(IsTargetSelectable_Area(_dragAreaStartPos, _dragAreaEndPos, keyPos, clickSize, clickSize))
								{
									_selectableCommonKeyframes.Add(commonKeyframe);
									isAnySelected = true;
									

									_targetSelectType = TARGET_SELECT_TYPE.CommonKeyframe;
								}
							}

							if(!isAnySelected)
							{
								if (!_isKeyframeClicked)
								{
									if (IsTargetSelectable(_dragAreaEndPos, keyPos, clickSize, clickSize) && _mousePos.y < _layoutHeight_Header)
									{

										
										_selectableCommonKeyframes.Add(commonKeyframe);

										if (commonKeyframe._isSelected)
										{
											_isSelectedKeyframeClick = true;
										}

										isAnySelected = true;

										if (IsTargetSelectable(_mousePos_Down, keyPos, clickSize, clickSize) && _mousePos.y < _layoutHeight_Header)
										{
											//클릭한 위치에서 선택된 것이라면
											//=> 단순 클릭
											_isKeyframeClicked = true;
										}

										_targetSelectType = TARGET_SELECT_TYPE.CommonKeyframe;

									}
								}
							}
						}
					}
					
				}
			}

			EndBatch();//Batch 끝


			bool isMoveOrCopy = (_moveCommonKeyframeList.Count != 0);
			if(isMoveOrCopy)
			{
				for (int i = 0; i < _moveCommonKeyframeList.Count; i++)
				{
					MoveCommonKeyframe moveCopyFrame = _moveCommonKeyframeList[i];

					//이동/복사하는거 그리자
					DrawMoveCopySummary(moveCopyFrame._startFrameIndex, moveCopyFrame._nextFrameIndex, posY, size, (_selectType != SELECT_TYPE.Add));
				}
			}
		}



		private static void DrawMoveCopySummary(int srcKeyframeIndex, int targetKeyframeIndex, float posY, float size, bool isMove)
		{
			if(srcKeyframeIndex == targetKeyframeIndex)
			{
				return;
			}

			float posX_Src = FrameToPosX_Main(srcKeyframeIndex) - 0.5f;
			float posX_Target = FrameToPosX_Main(targetKeyframeIndex) - 0.5f;

			if (posX_Src + size < 0 && posX_Target + size < 0)
			{
				return;
			}

			if (posX_Src - size > _layoutWidth && posX_Target - size > _layoutWidth)
			{
				return;
			}

			Vector2 keyPos = new Vector2(posX_Target, posY);
			Vector2 srcPos = new Vector2(posX_Src, posY);

			//이건 Batch 안함.. 일시적이니까요
			if (isMove)
			{
				DrawBoldLine(srcPos, keyPos, 3, _keyColor_Move_Line, true, CLIP_TYPE.Header);
				DrawTexture(_img_KeySummaryMove, keyPos, size, size, _keyColor_Move, true, CLIP_TYPE.Header);
			}
			else
			{
				DrawBoldLine(srcPos, keyPos, 3, _keyColor_Copy_Line, true, CLIP_TYPE.Header);
				DrawTexture(_img_KeySummaryMove, keyPos, size, size, _keyColor_Copy, true, CLIP_TYPE.Header);
			}
		}



		public static void DrawEventMarkers(List<apAnimEvent> animEvents, float posY)
		{
			if(animEvents == null || animEvents.Count == 0)
			{
				return;
			}

			float posX = 0.0f;
			float size = 13;
			Vector2 keyPos = Vector2.zero;
			BeginBatch_GUITexture(_img_EventMark, CLIP_TYPE.Header);

			

			apAnimEvent animEvent = null;
			for (int i = 0; i < animEvents.Count; i++)
			{
				animEvent = animEvents[i];

				posX = FrameToPosX_Main(animEvent._frameIndex);

				if (posX > -size && posX < _layoutWidth + size)
				{
					//위치
					keyPos = new Vector2(posX - 0.5f, posY);

					if (animEvent._callType == apAnimEvent.CALL_TYPE.Once)
					{
						DrawTexture(_img_EventMark, keyPos, size, size, _keyColor_EventOnce, false, CLIP_TYPE.Header);
					}
					else
					{
						DrawTexture(_img_EventMark, keyPos, size, size, _keyColor_EventCntPrev, false, CLIP_TYPE.Header);
					}
				}



				if (animEvent._callType == apAnimEvent.CALL_TYPE.Continuous)
				{
					//End 지점도 표시
					posX = FrameToPosX_Main(animEvent._frameIndex_End);

					if (posX > -size && posX < _layoutWidth + size)
					{
						//위치
						keyPos = new Vector2(posX - 0.5f, posY);

						DrawTexture(_img_EventMark, keyPos, size, size, _keyColor_EventCntNext, false, CLIP_TYPE.Header);
					}
				}
			}

			EndBatch();
		}



		public static void DrawOnionMarkers(int frame, Color color, float posY)
		{
			float posX = 0.0f;
			float size = 13;
			Vector2 keyPos = Vector2.zero;
			BeginBatch_GUITexture(_img_OnionMark, CLIP_TYPE.Header);

			posX = FrameToPosX_Main(frame);

			if (posX > -size && posX < _layoutWidth + size)
			{
				//위치
				keyPos = new Vector2(posX - 0.5f, posY);

				DrawTexture(_img_OnionMark, keyPos, size, size, color, false, CLIP_TYPE.Header);
			}

			EndBatch();
		}



		private enum KEYFRAME_CONTROL_TYPE
		{
			None,
			SingleSelect,
			MultipleSelect,
			MoveCopy,
			//Copy
		}
		private static KEYFRAME_CONTROL_TYPE _keyframeControlType = KEYFRAME_CONTROL_TYPE.None;

		private enum TARGET_SELECT_TYPE
		{
			Keyframe,
			CommonKeyframe
		}
		private static TARGET_SELECT_TYPE _targetSelectType = TARGET_SELECT_TYPE.Keyframe;

		//선택 여부 처리할 Keyframe 리스트
		private static List<apAnimKeyframe> _selectableKeyframes = new List<apAnimKeyframe>();
		private static List<apAnimCommonKeyframe> _selectableCommonKeyframes = new List<apAnimCommonKeyframe>();


		private static bool _isSelectedKeyframeClick = false;
		private static bool _isKeyframeClicked = false;//단순 클릭으로 선택한게 있다면 => 영역 선택은 무시된다.
		private static bool _isSelectLoopDummy = false;

		public static void BeginKeyframeControl()
		{
			_keyframeControlType = KEYFRAME_CONTROL_TYPE.None;

			//Select? Move? Copy? 체크
			// Event.None -> [단일 선택] 체크 [Select] -> (성공시) Event.DragFrame (클릭한 위치의 Frame)
			// Event.Select -> [복수 선택] 체크 [Select] (바로 결과가 나오는건 아니고, 대상 프레임에 넣는다)
			// Event.DragFrame -> 드래그 처리 중(클릭했을때의 Frame

			if (_isMouseEvent && !_isMouseEventUsed)
			{
				if (_timelineEvent == TIMELINE_EVENT.DragFrame || _timelineEvent == TIMELINE_EVENT.ReadyToDrag)
				{
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Up ||
						_leftBtnStatus == apMouse.MouseBtnStatus.Released)
					{
						//Debug.Log("Mouse Up : " + _timelineEvent + " -> None");
						if (_timelineEvent == TIMELINE_EVENT.DragFrame)
						{
							OnDragKeyframeUp();
						}
						SetTimelineEvent(TIMELINE_EVENT.None);

						_isSelectedKeyframeClick = false;
					}
				}

				if (_timelineEvent == TIMELINE_EVENT.None)
				{
					//1. 선택이 안될때 -> 선택 체크 -> (이어서 Drag 할 수 있다.)
					if (IsMouseUpdatable_Down(true, true))
					{
						_keyframeControlType = KEYFRAME_CONTROL_TYPE.SingleSelect;
						_selectableKeyframes.Clear();
						_selectableCommonKeyframes.Clear();
						_isSelectedKeyframeClick = false;
						_isSelectLoopDummy = false;

						_targetSelectType = TARGET_SELECT_TYPE.Keyframe;
					}
				}
				else if (_timelineEvent == TIMELINE_EVENT.Select)
				{
					_keyframeControlType = KEYFRAME_CONTROL_TYPE.MultipleSelect;
					_selectableKeyframes.Clear();
					_selectableCommonKeyframes.Clear();
					_isSelectedKeyframeClick = false;
					_isSelectLoopDummy = false;

					_targetSelectType = TARGET_SELECT_TYPE.Keyframe;
				}
				else if (_timelineEvent == TIMELINE_EVENT.DragFrame)
				{
					_keyframeControlType = KEYFRAME_CONTROL_TYPE.MoveCopy;
				}


				//TODO
			}
		}

		//public static void DrawKeyframes(List<apAnimKeyframe> keyFrames, float posY, Color baseColor, bool isAvailable, int lineHeight)
		public static void DrawKeyframes(apAnimTimelineLayer timelineLayer,
											float posY,
											Color baseColor,
											bool isAvailable,
											int lineHeight,
											bool isSelectedTimelineLayer,
											int curFrame,
											bool isCurveEdit,
											apSelection.ANIM_SINGLE_PROPERTY_CURVE_UI curveEditUIType

											)
		{
			List<apAnimKeyframe> keyFrames = timelineLayer._keyframes;
			apAnimKeyframe firstFrame = timelineLayer._firstKeyFrame;
			apAnimKeyframe lastFrame = timelineLayer._lastKeyFrame;
			bool isDummyFirstFrame = false;
			bool isDummyLastFrame = false;
			if (firstFrame != null && firstFrame._isLoopAsStart)
			{
				isDummyFirstFrame = true;
			}

			if (lastFrame != null && lastFrame._isLoopAsEnd)
			{
				isDummyLastFrame = true;
			}


			float sizeH = (lineHeight - 2);
			float sizeW = (sizeH / 4) + 3;

			//float halfSizeW = sizeW / 2.0f;
			float halfSizeH = sizeH / 2.0f;

			apAnimKeyframe curKeyFrame = null;

			baseColor.r *= 0.8f;
			baseColor.g *= 0.8f;
			baseColor.b *= 0.8f;
			baseColor.a = 1.0f;

			//Color selectedColor = new Color(1.0f, 0.05f, 0.05f, 1.0f);
			//Color workSelectedColor = new Color(0.2f, 0.3f, 0.7f, 1.0f);
			//Color inactiveColor = new Color(baseColor.r * 0.4f, baseColor.g * 0.4f, baseColor.b * 0.4f, 1.0f);

			//if (!isAvailable)
			//{
			//	baseColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
			//}

			posY = (posY + _layoutHeight_Header) - _scrollPos.y;
			//Vector2 keyPos = Vector2.zero;
			//Color keyColor = Color.black;
			bool isDrawY = false;

			int loopIconSize = 32;
			//Color grayColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

			if (posY + halfSizeH >= _layoutHeight_Header && posY - halfSizeH < _layoutHeight_Total)
			{
				isDrawY = true;
			}
			//영역에 포함되는 경우에만 렌더링을 하자
			//float posX = -1;
			bool isWorkingKeyframeExist = false;

			//렌더 Batch를 위해서 Render Request를 세팅하자.
			_renderRequest_Keyframe.Clear();
			_renderRequest_KeyframeDummy.Clear();
			_renderRequest_KeyframeLoopLeft.Clear();
			_renderRequest_KeyframeLoopRight.Clear();

			_renderRequest_Cursor.Clear();

			_renderRequest_MoveBoldLine.Clear();
			_renderRequest_KeyframeMove.Clear();

			_renderRequest_Keyframe.SetImageSize(sizeW, sizeH);
			_renderRequest_KeyframeDummy.SetImageSize(sizeW, sizeH);
			_renderRequest_KeyframeLoopLeft.SetImageSize(loopIconSize, loopIconSize);
			_renderRequest_KeyframeLoopRight.SetImageSize(loopIconSize, loopIconSize);
			_renderRequest_KeyframeMove.SetImageSize(sizeW, sizeH);

			float cursizeW = 22.0f * (sizeW / 14.0f);
			float cursizeH = 56.0f * (sizeH / 48.0f);

			_renderRequest_Cursor.SetImageSize(cursizeW, cursizeH);

			//TODO : 키프레임 렌더링을 일일이 하면 드로우콜이 엄청 늘어난다.
			//텍스쳐에 따라 나누고 Render Request 리스트를 만들어서
			//묶어서 Batch를 하자.

			if (isCurveEdit && isSelectedTimelineLayer)
			{
				BeginBatch_Color(CLIP_TYPE.Main);//Batch 시작

				apAnimKeyframe selectedKeyframe = null;
				if (_selection.AnimKeyframes.Count == 1 && _selection.AnimKeyframe != null && keyFrames.Contains(_selection.AnimKeyframe))
				{
					selectedKeyframe = _selection.AnimKeyframe;
				}

				Color curveColor = Color.black;
				for (int i = 0; i < keyFrames.Count; i++)
				{
					curKeyFrame = keyFrames[i];

					//시작 키는 Prev + Next Curve를 출력한다.
					//나머지는 Next만 출력한다.
					if (curKeyFrame == firstFrame)
					{
						DrawCurve(curKeyFrame, true, selectedKeyframe, curveEditUIType, posY, false);
					}

					DrawCurve(curKeyFrame, false, selectedKeyframe, curveEditUIType, posY, ((curKeyFrame == lastFrame) && isDummyLastFrame));
				}

				EndBatch();//Batch 끝
			}



			for (int i = 0; i < keyFrames.Count; i++)
			{
				curKeyFrame = keyFrames[i];

				if (_selection.AnimWorkKeyframe == curKeyFrame)
				{
					isWorkingKeyframeExist = true;
				}

				//DrawSingleKeyframe(curKeyFrame, isAvailable, sizeW, sizeH, isDrawY, posY, selectedColor, inactiveColor, baseColor, grayColor, workSelectedColor, loopIconSize, false);
				DrawSingleKeyframe(curKeyFrame, isAvailable, sizeW, sizeH, isDrawY, posY, baseColor, loopIconSize, false);
			}

			if (isDummyLastFrame)
			{
				//더미 프레임은 조금 더 신중하게 처리한다.
				DrawSingleKeyframe(lastFrame, isAvailable, sizeW, sizeH, isDrawY, posY, baseColor, loopIconSize, true);
			}


			if (isDummyFirstFrame)
			{
				//더미 프레임은 조금 더 신중하게 처리한다.
				DrawSingleKeyframe(firstFrame, isAvailable, sizeW, sizeH, isDrawY, posY, baseColor, loopIconSize, true);
			}


			//배치 요청에 따라 1차적으로 렌더링을 하자
			if(_renderRequest_Keyframe._nRequest > 0)
			{
				//1. 일반 Keyframe
				BeginBatch_GUITexture(_renderRequest_Keyframe._image, CLIP_TYPE.Main);

				float batSizeW = _renderRequest_Keyframe._imageSize.x;
				float batSizeH = _renderRequest_Keyframe._imageSize.y;

				for (int i = 0; i < _renderRequest_Keyframe._nRequest; i++)
				{
					DrawTexture(	_renderRequest_Keyframe._image,
									_renderRequest_Keyframe._posList[i],
									batSizeW, batSizeH,
									_renderRequest_Keyframe._colorList[i],
									false, CLIP_TYPE.Main);
				}

				EndBatch();
			}

			if(_renderRequest_KeyframeDummy._nRequest > 0)
			{
				//2. 더미 Keyframe
				BeginBatch_GUITexture(_renderRequest_KeyframeDummy._image, CLIP_TYPE.Main);

				float batSizeW = _renderRequest_KeyframeDummy._imageSize.x;
				float batSizeH = _renderRequest_KeyframeDummy._imageSize.y;


				for (int i = 0; i < _renderRequest_KeyframeDummy._nRequest; i++)
				{
					DrawTexture(	_renderRequest_KeyframeDummy._image,
									_renderRequest_KeyframeDummy._posList[i],
									batSizeW, batSizeH,
									_renderRequest_KeyframeDummy._colorList[i],
									false, CLIP_TYPE.Main);
				}

				EndBatch();
			}

			if(_renderRequest_KeyframeLoopLeft._nRequest > 0)
			{
				//3. Loop Left 아이콘
				BeginBatch_GUITexture(_renderRequest_KeyframeLoopLeft._image, CLIP_TYPE.Main);

				float batSizeW = _renderRequest_KeyframeLoopLeft._imageSize.x;
				float batSizeH = _renderRequest_KeyframeLoopLeft._imageSize.y;


				for (int i = 0; i < _renderRequest_KeyframeLoopLeft._nRequest; i++)
				{
					DrawTexture(	_renderRequest_KeyframeLoopLeft._image,
									_renderRequest_KeyframeLoopLeft._posList[i],
									batSizeW, batSizeH,
									_renderRequest_KeyframeLoopLeft._colorList[i],
									false, CLIP_TYPE.Main);
				}

				EndBatch();
			}

			if(_renderRequest_KeyframeLoopRight._nRequest > 0)
			{
				//4. Loop Right 아이콘
				BeginBatch_GUITexture(_renderRequest_KeyframeLoopRight._image, CLIP_TYPE.Main);

				float batSizeW = _renderRequest_KeyframeLoopRight._imageSize.x;
				float batSizeH = _renderRequest_KeyframeLoopRight._imageSize.y;


				for (int i = 0; i < _renderRequest_KeyframeLoopRight._nRequest; i++)
				{
					DrawTexture(	_renderRequest_KeyframeLoopRight._image,
									_renderRequest_KeyframeLoopRight._posList[i],
									batSizeW, batSizeH,
									_renderRequest_KeyframeLoopRight._colorList[i],
									false, CLIP_TYPE.Main);
				}

				EndBatch();
			}

			if(_renderRequest_Cursor._nRequest > 0)
			{
				//5. 현재 선택된 Frame 커서
				BeginBatch_GUITexture(_renderRequest_Cursor._image, CLIP_TYPE.Main);

				float batSizeW = _renderRequest_Cursor._imageSize.x;
				float batSizeH = _renderRequest_Cursor._imageSize.y;


				for (int i = 0; i < _renderRequest_Cursor._nRequest; i++)
				{
					DrawTexture(	_renderRequest_Cursor._image,
									_renderRequest_Cursor._posList[i],
									batSizeW, batSizeH,
									_renderRequest_Cursor._colorList[i],
									false, CLIP_TYPE.Main);
				}

				EndBatch();
			}



			if (!isWorkingKeyframeExist && isSelectedTimelineLayer)
			{
				//현재 타임라인에서 + 재생중인 키프레임이 없다면 => 임시 키프레임을 보여주자
				DrawCurrentKeyframe(curFrame, sizeW, sizeH, posY, new Color(0.5f, 0.5f, 0.5f, 1.0f));
			}

			bool isMoveOrCopy = (_moveKeyframeList.Count != 0);
			if (isMoveOrCopy)
			{
				for (int i = 0; i < keyFrames.Count; i++)
				{
					curKeyFrame = keyFrames[i];

					MoveKeyframe moveCopyKeyframe = GetMoveKeyframe(curKeyFrame);
					if (moveCopyKeyframe != null)
					{
						DrawMoveCopyKeyframe(moveCopyKeyframe._startFrameIndex, moveCopyKeyframe._nextFrameIndex, sizeW, sizeH, posY, (_selectType != SELECT_TYPE.Add));
					}
				}
			}

			//Batch를 하자
			if(_renderRequest_MoveBoldLine._nRequest > 0)
			{
				//6. Move/Copy의 라인
				BeginBatch_Color(CLIP_TYPE.Main);


				for (int i = 0; i < _renderRequest_MoveBoldLine._nRequest; i++)
				{
					DrawBoldLine(	_renderRequest_MoveBoldLine._linePos1List[i], 
									_renderRequest_MoveBoldLine._linePos2List[i], 
									3, 
									_renderRequest_MoveBoldLine._colorList[i],
									false, CLIP_TYPE.Main);
				}

				EndBatch();
			}

			if(_renderRequest_KeyframeMove._nRequest > 0)
			{
				//7. Move/Copy의 키프레임 잔상
				BeginBatch_GUITexture(_renderRequest_KeyframeMove._image, CLIP_TYPE.Main);

				float batSizeW = _renderRequest_KeyframeMove._imageSize.x;
				float batSizeH = _renderRequest_KeyframeMove._imageSize.y;


				for (int i = 0; i < _renderRequest_KeyframeMove._nRequest; i++)
				{
					DrawTexture(	_renderRequest_KeyframeMove._image,
									_renderRequest_KeyframeMove._posList[i],
									batSizeW, batSizeH,
									_renderRequest_KeyframeMove._colorList[i],
									false, CLIP_TYPE.Main);
				}

				EndBatch();
			}

			//GL.End();
		}



		private static void DrawSingleKeyframe(apAnimKeyframe keyframe,
												bool isAvailable,
												float sizeW, float sizeH,
												bool isDrawY, float posY,
												Color baseColor,
												//Color selectedColor, Color inactiveColor, Color baseColor, Color grayColor, Color workSelectedColor,
												int loopIconSize,
												bool isDummyFrame
											)
		{
			float posX = 0.0f;
			if (!isDummyFrame)
			{
				posX = FrameToPosX_Main(keyframe._frameIndex);
			}
			else
			{
				posX = FrameToPosX_Main(keyframe._loopFrameIndex);
			}

			bool isDrawX = false;
			if (posX < -sizeW || posX > _layoutWidth + sizeW)
			{
				isDrawX = false;
			}
			else
			{
				isDrawX = true;
			}
			Vector2 keyPos = new Vector2(posX - 0.5f, posY);
			Color keyColor = Color.black;

			//선택되었는지 여부
			bool isCursorDraw = false;
			if (!keyframe._isActive)
			{
				keyColor = _keyColor_Inactive;
			}
			else if (_selection.IsSelectedKeyframe(keyframe))
			{
				keyColor = _keyColor_Selected;
			}
			else if (_selection.AnimWorkKeyframe == keyframe)
			{
				keyColor = _keyColor_Working;
			}
			else
			{
				keyColor = baseColor;
			}

			if (_selection.AnimWorkKeyframe == keyframe)
			{
				isCursorDraw = true;
			}

			//이동/복사중이면 거기에 맞게 다르게 표현해야한다.
			//bool isMoveOrCopy = (_moveKeyframeList.Count != 0);
			//bool isDrawMoveCopyKey = isMoveOrCopy && (!isDummyFrame);//더미키가 아닌 Render에서 Move/Copy 상태일때


			//DrawTexture(_img_Keyframe, keyPos, sizeW, sizeH, keyColor, false, CLIP_TYPE.Main);
			if (isDrawX && isDrawY)
			{
				if (!isDummyFrame)
				{
					//더미가 아닐때 -> 복사 정보를 같이 출력해야한다.

					//Batch 전
					//DrawTexture(_img_Keyframe, keyPos, sizeW, sizeH, keyColor, true, CLIP_TYPE.Main);

					//Batch 후
					_renderRequest_Keyframe.Add(keyPos, keyColor);
				}
				else
				{
					//Batch 전
					//DrawTexture(_img_KeyframeDummy, keyPos, sizeW, sizeH, keyColor, true, CLIP_TYPE.Main);

					//Batch 후
					_renderRequest_KeyframeDummy.Add(keyPos, keyColor);
				}
				if (keyframe._isLoopAsStart && keyframe._prevLinkedKeyframe != null)
				{
					//Batch 전
					//DrawTexture(_img_KeyLoopLeft, keyPos, loopIconSize, loopIconSize, _keyColor_Normal, true, CLIP_TYPE.Main);

					//Batch 후
					_renderRequest_KeyframeLoopLeft.Add(keyPos, _keyColor_Normal);
				}
				if (keyframe._isLoopAsEnd && keyframe._nextLinkedKeyframe != null)
				{
					//Batch 전
					//DrawTexture(_img_KeyLoopRight, keyPos, loopIconSize, loopIconSize, _keyColor_Normal, true, CLIP_TYPE.Main);

					//Batch 후
					_renderRequest_KeyframeLoopRight.Add(keyPos, _keyColor_Normal);
				}

				if (isCursorDraw)
				{
					//배치 전
					//float cursizeW = 22.0f * (sizeW / 14.0f);
					//float cursizeH = 56.0f * (sizeH / 48.0f);
					//DrawTexture(_img_KeyframeCursor, keyPos, cursizeW, cursizeH, _keyColor_Normal, true, CLIP_TYPE.Main);

					//배치 후
					_renderRequest_Cursor.Add(keyPos, _keyColor_Normal);
				}

				if (isAvailable)
				{
					AddCursorRect(keyPos, sizeW, sizeH + 4, MouseCursor.MoveArrow);
				}
			}

			//if(isDrawMoveCopyKey)
			//{
			//	MoveKeyframe moveCopyKeyframe = GetMoveKeyframe(keyframe);
			//	if(moveCopyKeyframe != null)
			//	{
			//		DrawMoveCopyKeyframe(moveCopyKeyframe._startFrameIndex, moveCopyKeyframe._nextFrameIndex, sizeW, sizeH, posY, (_selectType != SELECT_TYPE.Add));
			//	}
			//}

			if (_isMouseEvent && isAvailable && !_isMouseEventUsed)
			{
				if (_keyframeControlType == KEYFRAME_CONTROL_TYPE.SingleSelect)
				{
					//클릭으로 선택
					if (_selectableKeyframes.Count == 0)
					{
						//아무것도 아직 선택된게 없을 때
						if (_mousePos.y > _layoutHeight_Header)
						{
							if (_leftBtnStatus == apMouse.MouseBtnStatus.Down)
							{
								if (IsTargetSelectable(_mousePos, keyPos, sizeW + 8, sizeH + 4))
								{
									_selectableKeyframes.Add(keyframe);
									//선택된 키프레임을 다시 클릭했는가
									if (_selection.IsSelectedKeyframe(keyframe))
									{
										_isSelectedKeyframeClick = true;
									}
									else
									{
										_isSelectedKeyframeClick = false;
									}
									_isKeyframeClicked = true;
									_isSelectLoopDummy = isDummyFrame;

									_targetSelectType = TARGET_SELECT_TYPE.Keyframe;
								}
							}
						}
					}
				}
				else if (_keyframeControlType == KEYFRAME_CONTROL_TYPE.MultipleSelect)
				{
					//영역으로 선택
					if (_downClipArea == CLIP_TYPE.Main)
					{
						bool isAnySelected = false;


						if (!_isKeyframeClicked)
						{
							if (IsTargetSelectable_Area(_dragAreaStartPos, _dragAreaEndPos, keyPos, sizeW, sizeH))
							{
								_selectableKeyframes.Add(keyframe);
								isAnySelected = true;

								_targetSelectType = TARGET_SELECT_TYPE.Keyframe;
							}
						}

						if (!isAnySelected)
						{
							//if (_leftBtnStatus == apMouse.MouseBtnStatus.Down || _leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
							if (_leftBtnStatus == apMouse.MouseBtnStatus.Down)
							{
								if (IsTargetSelectable(_dragAreaEndPos, keyPos, sizeW + 6, sizeH + 4) && _mousePos.y > _layoutHeight_Header)
								{
									_selectableKeyframes.Add(keyframe);

									if (_selection.IsSelectedKeyframe(keyframe))
									{
										_isSelectedKeyframeClick = true;
									}
									//_isKeyframeClicked = true;
									isAnySelected = true;

									if (IsTargetSelectable(_mousePos_Down, keyPos, sizeW + 6, sizeH + 4) && _mousePos.y > _layoutHeight_Header)
									{
										//클릭한 위치에서 선택된 것이라면
										//=> 단순 클릭
										_isKeyframeClicked = true;
									}

									_targetSelectType = TARGET_SELECT_TYPE.Keyframe;
								}
							}
						}


					}
				}
			}
		}


		/// <summary>
		/// 현재 재생중인 Frame에 대해서 선택된 키프레임이 없을 경우, 가상의 키프레임 이미지를 출력한다.
		/// </summary>
		/// <param name="keyframe"></param>
		/// <param name="sizeW"></param>
		/// <param name="sizeH"></param>
		/// <param name="posY"></param>
		/// <param name="color"></param>
		private static void DrawCurrentKeyframe(int frameIndex,
													float sizeW, float sizeH,
													float posY,
													Color color
											)
		{
			float posX = FrameToPosX_Main(frameIndex);

			if (posX < -sizeW || posX > _layoutWidth + sizeW)
			{
				return;
			}

			Vector2 keyPos = new Vector2(posX - 0.5f, posY);
			//선택되었는지 여부

			
			DrawTexture(_img_CurKeyframe, keyPos, sizeW, sizeH, color, true, CLIP_TYPE.Main);
		}

		private static void DrawMoveCopyKeyframe(int srcKeyframeIndex, int targetKeyframeIndex, float sizeW, float sizeH, float posY, bool isMove)
		{
			if (srcKeyframeIndex == targetKeyframeIndex)
			{
				return;
			}
			float posX_Src = FrameToPosX_Main(srcKeyframeIndex) - 0.5f;
			float posX_Target = FrameToPosX_Main(targetKeyframeIndex) - 0.5f;

			if (posX_Src + sizeW < 0 && posX_Target + sizeW < 0)
			{
				return;
			}

			if (posX_Src - sizeW > _layoutWidth && posX_Target - sizeW > _layoutWidth)
			{
				return;
			}


			Vector2 keyPos = new Vector2(posX_Target, posY);
			Vector2 srcPos = new Vector2(posX_Src, posY);

			if (isMove)
			{
				//Batch 전
				//DrawBoldLine(srcPos, keyPos, 3, _keyColor_Move_Line, true, CLIP_TYPE.Main);
				//DrawTexture(_img_Keyframe, keyPos, sizeW, sizeH, _keyColor_Move, true, CLIP_TYPE.Main);

				//Batch 후
				_renderRequest_MoveBoldLine.Add(srcPos, keyPos, _keyColor_Move_Line);
				_renderRequest_KeyframeMove.Add(keyPos, _keyColor_Move);
			}
			else
			{
				//Batch 전
				//DrawBoldLine(srcPos, keyPos, 3, _keyColor_Copy_Line, true, CLIP_TYPE.Main);
				//DrawTexture(_img_Keyframe, keyPos, sizeW, sizeH, _keyColor_Copy, true, CLIP_TYPE.Main);

				//Batch 후
				_renderRequest_MoveBoldLine.Add(srcPos, keyPos, _keyColor_Copy_Line);
				_renderRequest_KeyframeMove.Add(keyPos, _keyColor_Copy);
			}

		}

		private static Color _curveColor_Linear = new Color(1.0f, 0.2f, 0.2f, 1.0f);
		private static Color _curveColor_Smooth = new Color(0.2f, 0.5f, 1.0f, 1.0f);
		private static Color _curveColor_Constant = new Color(0.2f, 1.0f, 0.2f, 1.0f);

		private static void DrawCurve(apAnimKeyframe keyframe, bool isPrevDraw, apAnimKeyframe selectedKeyframe, apSelection.ANIM_SINGLE_PROPERTY_CURVE_UI curveEditUIType, float posY, bool isDummy)
		{
			apAnimCurveResult curveResult = null;
			if (isPrevDraw)
			{
				if (keyframe._curveKey._prevLinkedCurveKey == null)
				{ return; }
				curveResult = keyframe._curveKey._prevCurveResult;
			}
			else
			{
				if (keyframe._curveKey._nextLinkedCurveKey == null)
				{ return; }
				curveResult = keyframe._curveKey._nextCurveResult;
			}
			if (curveResult == null)
			{ return; }
			if (curveResult._curveKeyA == null || curveResult._curveKeyB == null)
			{ return; }

			//int frameIndex_Start = keyframe._frameIndex;

			float posX_CurveStart = FrameToPosX_Main(keyframe._frameIndex);
			float posX_CurveEnd = 0.0f;
			//bool isCurveDrawable = false;
			if (isPrevDraw)
			{
				posX_CurveEnd = FrameToPosX_Main(keyframe._curveKey._prevIndex);
			}
			else
			{
				posX_CurveEnd = FrameToPosX_Main(keyframe._curveKey._nextIndex);
			}




			Color curveColor = Color.black;
			switch (curveResult.CurveTangentType)
			{
				case apAnimCurve.TANGENT_TYPE.Linear:
					curveColor = _curveColor_Linear;
					break;

				case apAnimCurve.TANGENT_TYPE.Smooth:
					curveColor = _curveColor_Smooth;
					break;

				case apAnimCurve.TANGENT_TYPE.Constant:
					curveColor = _curveColor_Constant;
					break;
			}

			bool isSelected = false;
			if (curveEditUIType == apSelection.ANIM_SINGLE_PROPERTY_CURVE_UI.Prev)
			{
				//[ <- ]으로 그릴때
				//CurveResult의 Next가 Selected Keyframe이라면 Selected
				if (selectedKeyframe != null)
				{
					isSelected = (curveResult._curveKeyB == selectedKeyframe._curveKey);
				}
			}
			else
			{
				//[ -> ]으로 그릴때
				//CurveResult의 Prev가 Selected Keyframe이라면 Selected
				if (selectedKeyframe != null)
				{
					isSelected = (curveResult._curveKeyA == selectedKeyframe._curveKey);
				}
			}

			if (!isSelected)
			{
				curveColor.a = 0.3f;
			}

			DrawBoldLine(new Vector2(posX_CurveStart, posY),
									new Vector2(posX_CurveEnd, posY),
									7, curveColor, false, CLIP_TYPE.Main);//<<변경 : Batch 되도록 Reset Mat : True->False


			if (isDummy)
			{
				float posX_Loop = FrameToPosX_Main(keyframe._loopFrameIndex);
				posX_CurveEnd = (posX_CurveEnd - posX_CurveStart) + posX_Loop;

				DrawBoldLine(new Vector2(posX_Loop, posY),
									new Vector2(posX_CurveEnd, posY),
									7, curveColor, false, CLIP_TYPE.Main);//<<변경 : Batch 되도록 Reset Mat : True->False
			}
		}


		public static bool EndKeyframeControl()
		{
			
			bool isEventOccurred = false;
			if (_isMouseEvent && !_isMouseEventUsed)
			{	
				switch (_keyframeControlType)
				{
					case KEYFRAME_CONTROL_TYPE.None:
						//패스
						break;


					case KEYFRAME_CONTROL_TYPE.SingleSelect:
						{
							bool isAnySelected = false;
							if (_targetSelectType == TARGET_SELECT_TYPE.Keyframe)
							{

								if (_selectableKeyframes.Count > 0)
								{
									isAnySelected = true;
									//선택한게 있다.
									//선택한 이후엔 바로 이어서 DragFrame 상태로 넘어간다.

									//하나만 추가
									apGizmos.SELECT_TYPE selectType = apGizmos.SELECT_TYPE.Add;
									switch (_selectType)
									{
										case SELECT_TYPE.New:
											selectType = apGizmos.SELECT_TYPE.New;
											break;
										case SELECT_TYPE.Add:
											selectType = apGizmos.SELECT_TYPE.Add;
											break;
										case SELECT_TYPE.Subtract:
											selectType = apGizmos.SELECT_TYPE.Subtract;
											break;
									}

									if (_isSelectedKeyframeClick)
									{
										//"선택되었던 걸" 다시 눌렀다면 얘기가 다르다.
										//Add/Subtract를 제외하고는 Drag를 해야함
										//Drag를 해야할때는 Selection에 선택 이벤트를 날리지 않는다.
										//그 외에는 동일
										if (selectType != apGizmos.SELECT_TYPE.New)
										{
											_selection.SetAnimKeyframe(_selectableKeyframes[0], true, selectType);
										}


									}
									else
									{
										//Selection Type에 맞게 "키프레임 한개"를 선택 (또는 해제) 한다.
										//추가 : 한개 선택시에는 Frame을 이동한다.
										_selection.SetAnimKeyframe(_selectableKeyframes[0], true, selectType, _isSelectLoopDummy);
									}

									EditorRepaint();


									//마우스를 클릭하믄
									if (_leftBtnStatus != apMouse.MouseBtnStatus.Up && _leftBtnStatus != apMouse.MouseBtnStatus.Released)
									{
										if (_selectType != SELECT_TYPE.Subtract)
										{
											if (_selection.AnimKeyframes.Count == 1 ||
												(_selection.AnimKeyframes.Count > 1 && _isSelectedKeyframeClick)
												)
											{
												//if (_isSelectedKeyframeClick)//이걸 키고 else를 켜면 "첫 클릭시에는 Drag가 안되고, 다시 클릭할때 Drag"
												//if(true)//이 상태에서는 바로 Drag가 허용된다.
												{
													//_dragAreaStartPos = _mousePos_Down - (_scrollPos - _scroll_Down);



													_keyDragStartPos = _mousePos.x - (_scrollPos.x - _scroll_Down.x);
													_keyDragFrameIndex_Down = PosToFrame(_keyDragStartPos);
													_keyDragFrameIndex_Cur = _keyDragFrameIndex_Down;
													//Debug.Log("Start Drag Keyframe [Down : " + _keyDragFrameIndex_Down + "]");

													_moveKeyframeList.Clear();
													for (int i = 0; i < _selection.AnimKeyframes.Count; i++)
													{
														_moveKeyframeList.Add(new MoveKeyframe(_selection.AnimKeyframes[i]));
													}


													SetTimelineEvent(TIMELINE_EVENT.DragFrame);
												}
											}
										}
									}
									else
									{
										//선택한게 없다. => 마우스 오버
										SetTimelineEvent(TIMELINE_EVENT.None);
										_isMouseEventUsed = true;
									}
								}
							}
							else if(_targetSelectType == TARGET_SELECT_TYPE.CommonKeyframe)
							{
								if (_selectableCommonKeyframes.Count > 0)
								{
									//추가
									//Keyframe 대신 Common Keyframe을 선택했다면..
									isAnySelected = true;

									//TODO
									apGizmos.SELECT_TYPE selectType = apGizmos.SELECT_TYPE.Add;
									switch (_selectType)
									{
										case SELECT_TYPE.New:
											selectType = apGizmos.SELECT_TYPE.New;
											break;
										case SELECT_TYPE.Add:
											selectType = apGizmos.SELECT_TYPE.Add;
											break;
										case SELECT_TYPE.Subtract:
											selectType = apGizmos.SELECT_TYPE.Subtract;
											break;
									}

									if(_isSelectedKeyframeClick)
									{
										//선택된걸 다시 눌렀다.
										if (selectType != apGizmos.SELECT_TYPE.New)
										{
											//_selection.SetAnimKeyframe(_selectableKeyframes[0], true, selectType);
											_selection.SetAnimCommonKeyframe(_selectableCommonKeyframes[0], selectType);
										}
									}
									else
									{
										//Selection 타입에 맞게 
										_selection.SetAnimCommonKeyframe(_selectableCommonKeyframes[0], selectType);
									}

									EditorRepaint();

									//마우스를 클릭할 때
									//TODO : MoveCommonKeyframe 작성 필요
									//같은 코드의 2070줄 참조

									if (_leftBtnStatus != apMouse.MouseBtnStatus.Up && _leftBtnStatus != apMouse.MouseBtnStatus.Released)
									{
										if (_selectType != SELECT_TYPE.Subtract)
										{
											if (_selection.AnimCommonKeyframes_Selected.Count == 1 ||
												(_selection.AnimCommonKeyframes_Selected.Count > 1 && _isSelectedKeyframeClick)
												)
											{
												_keyDragStartPos = _mousePos.x - (_scrollPos.x - _scroll_Down.x);
												_keyDragFrameIndex_Down = PosToFrame(_keyDragStartPos);
												_keyDragFrameIndex_Cur = _keyDragFrameIndex_Down;

												_moveCommonKeyframeList.Clear();


												for (int i = 0; i < _selection.AnimCommonKeyframes_Selected.Count; i++)
												{
													_moveCommonKeyframeList.Add(new MoveCommonKeyframe(_selection.AnimCommonKeyframes_Selected[i]));
												}

												SetTimelineEvent(TIMELINE_EVENT.DragFrame);
											}
										}
									}
									else
									{
										SetTimelineEvent(TIMELINE_EVENT.None);
										_isMouseEventUsed = true;
									}
								}
							}

							if(!isAnySelected)
							{
								//선택한게 없다.
								if (_timelineEvent != TIMELINE_EVENT.None)
								{
									SetTimelineEvent(TIMELINE_EVENT.None);
								}
							}
							isEventOccurred = true;
						}
						break;

					case KEYFRAME_CONTROL_TYPE.MultipleSelect:
						{
							
							if (_leftBtnStatus == apMouse.MouseBtnStatus.Up || _leftBtnStatus == apMouse.MouseBtnStatus.Released)
							{
								if (_targetSelectType == TARGET_SELECT_TYPE.Keyframe)
								{
									//Debug.Log("Multiple Select [" + _targetSelectableKeyframes.Count + "]");
									//마우스를 떼었으면 끝
									switch (_selectType)
									{
										case SELECT_TYPE.New:
											_selection.SetAnimMultipleKeyframe(_selectableKeyframes, apGizmos.SELECT_TYPE.New, true);
											break;

										case SELECT_TYPE.Add:
											_selection.SetAnimMultipleKeyframe(_selectableKeyframes, apGizmos.SELECT_TYPE.Add, true);
											break;

										case SELECT_TYPE.Subtract:
											_selection.SetAnimMultipleKeyframe(_selectableKeyframes, apGizmos.SELECT_TYPE.Subtract, true);
											break;
									}
								}
								else
								{
									//추가 : Common Keyframe
									switch (_selectType)
									{
										case SELECT_TYPE.New:
											_selection.SetAnimCommonKeyframes(_selectableCommonKeyframes, apGizmos.SELECT_TYPE.New);
											break;

										case SELECT_TYPE.Add:
											_selection.SetAnimCommonKeyframes(_selectableCommonKeyframes, apGizmos.SELECT_TYPE.Add);
											break;

										case SELECT_TYPE.Subtract:
											_selection.SetAnimCommonKeyframes(_selectableCommonKeyframes, apGizmos.SELECT_TYPE.Subtract);
											break;
									}
								}

								isEventOccurred = true;

								//SetTimelineEvent(TIMELINE_EVENT.None);
								//_isMouseEventUsed = true;

								EditorRepaint();
								

								//다중 선택시 마우스 이벤트를 일시 종료
								ReleaseMouseEvent();
							}
						}
						break;

					case KEYFRAME_CONTROL_TYPE.MoveCopy:
						//드래그해서 키 복사/이동
						if (_leftBtnStatus == apMouse.MouseBtnStatus.Up || _leftBtnStatus == apMouse.MouseBtnStatus.Released)
						{
							OnDragKeyframeUp();
							SetTimelineEvent(TIMELINE_EVENT.None);
							_isMouseEventUsed = true;

							isEventOccurred = true;

							EditorRepaint();

							//이동 끝난 이후에 마우스 이벤트를 일시 종료
							ReleaseMouseEvent();
						}
						else
						{
							_keyDragCurPos = _mousePos.x - (_scrollPos.x - _scroll_Down.x);
							int curFrame = PosToFrame(_keyDragCurPos);

							//if(_selection.AnimKeyframes != null && _selection.AnimKeyframes.Count > 0)
							if (_moveKeyframeList.Count > 0)
							{
								if (curFrame != _keyDragFrameIndex_Cur)
								{
									//변동 사항이 있었다.
									//Debug.Log("Next Move Keyframe [" + _keyDragFrameIndex_Cur + " > " + curFrame + "]");

									//int deltaKeyframeFromDown = Mathf.Clamp(curFrame, _startFrame, _endFrame) - Mathf.Clamp(_keyDragFrameIndex_Down, _startFrame, _endFrame);
									int deltaKeyframeFromDown = curFrame - _keyDragFrameIndex_Down;

									//체크 : deltaKeyframe대로 이동하다가 Start / End Frame을 벗어나면 안된다.
									//다중 선택을 포함해서 "한계점에선 다같이 이동을 못함" 상태로 만들어야 한다.
									int maxDeltaMove = deltaKeyframeFromDown;

									//1. 체크하여 얼마나 이동 가능한지 먼저 본다.
									//2. 그 Delta값 만큼 이동 또는 복사를 한다.

									for (int iKeyframe = 0; iKeyframe < _moveKeyframeList.Count; iKeyframe++)
									{
										MoveKeyframe moveKey = _moveKeyframeList[iKeyframe];

										//apAnimKeyframe movedKeyframe = _selection.AnimKeyframes[iKeyframe];
										int nextFrameIndex = moveKey._startFrameIndex + deltaKeyframeFromDown;

										if (nextFrameIndex < _startFrame)
										{
											//startFrame = frameIndex + deltaX
											//deltaX = startFrame - frameIndex;
											int deltaLimit = _startFrame - moveKey._startFrameIndex;
											if (Mathf.Abs(deltaLimit) <= Mathf.Abs(maxDeltaMove))
											{
												maxDeltaMove = deltaLimit;
											}
										}
										else if (nextFrameIndex > _endFrame)
										{
											//_endFrame = frameIndex + deltaX
											//deltaX = _endFrame - frameIndex;
											int deltaLimit = _endFrame - moveKey._startFrameIndex;
											if (Mathf.Abs(deltaLimit) <= Mathf.Abs(maxDeltaMove))
											{
												maxDeltaMove = deltaLimit;
											}
										}
									}

									//만약 이동거리 조절 후 부호가 바뀌거나 0이 되면 처리를 안한다.
									if (deltaKeyframeFromDown * maxDeltaMove <= 0)
									{
										maxDeltaMove = 0;
									}

									for (int iKeyframe = 0; iKeyframe < _moveKeyframeList.Count; iKeyframe++)
									{
										MoveKeyframe moveKey = _moveKeyframeList[iKeyframe];

										//apAnimKeyframe movedKeyframe = _selection.AnimKeyframes[iKeyframe];
										int nextFrameIndex = moveKey._startFrameIndex + maxDeltaMove;
										moveKey._nextFrameIndex = nextFrameIndex;

										apAnimTimelineLayer parentLayer = moveKey.ParentLayer;
										if (parentLayer == null)
										{
											continue;
										}
									}
								}
								_keyDragFrameIndex_Cur = curFrame;
							}

							if(_moveCommonKeyframeList.Count > 0)
							{
								if(curFrame != _keyDragFrameIndex_Cur)
								{
									int deltaKeyframeFromDown = curFrame - _keyDragFrameIndex_Down;

									//체크 : deltaKeyframe대로 이동하다가 Start / End Frame을 벗어나면 안된다.
									//다중 선택을 포함해서 "한계점에선 다같이 이동을 못함" 상태로 만들어야 한다.
									int maxDeltaMove = deltaKeyframeFromDown;

									//1. 체크하여 얼마나 이동 가능한지 먼저 본다.
									//2. 그 Delta값 만큼 이동 또는 복사를 한다.

									for (int iKeyframe = 0; iKeyframe < _moveCommonKeyframeList.Count; iKeyframe++)
									{
										MoveCommonKeyframe moveKey = _moveCommonKeyframeList[iKeyframe];

										//apAnimKeyframe movedKeyframe = _selection.AnimKeyframes[iKeyframe];
										int nextFrameIndex = moveKey._startFrameIndex + deltaKeyframeFromDown;

										if (nextFrameIndex < _startFrame)
										{
											//startFrame = frameIndex + deltaX
											//deltaX = startFrame - frameIndex;
											int deltaLimit = _startFrame - moveKey._startFrameIndex;
											if (Mathf.Abs(deltaLimit) <= Mathf.Abs(maxDeltaMove))
											{
												maxDeltaMove = deltaLimit;
											}
										}
										else if (nextFrameIndex > _endFrame)
										{
											//_endFrame = frameIndex + deltaX
											//deltaX = _endFrame - frameIndex;
											int deltaLimit = _endFrame - moveKey._startFrameIndex;
											if (Mathf.Abs(deltaLimit) <= Mathf.Abs(maxDeltaMove))
											{
												maxDeltaMove = deltaLimit;
											}
										}
									}

									//만약 이동거리 조절 후 부호가 바뀌거나 0이 되면 처리를 안한다.
									if (deltaKeyframeFromDown * maxDeltaMove <= 0)
									{
										maxDeltaMove = 0;
									}


									for (int iKeyframe = 0; iKeyframe < _moveCommonKeyframeList.Count; iKeyframe++)
									{
										MoveCommonKeyframe moveKey = _moveCommonKeyframeList[iKeyframe];

										//apAnimKeyframe movedKeyframe = _selection.AnimKeyframes[iKeyframe];
										int nextFrameIndex = moveKey._startFrameIndex + maxDeltaMove;
										moveKey._nextFrameIndex = nextFrameIndex;
									}
								}

								_keyDragFrameIndex_Cur = curFrame;
							}
						}
						break;
				}

				if (_isKeyframeClicked &&
					(_leftBtnStatus == apMouse.MouseBtnStatus.Up || _leftBtnStatus == apMouse.MouseBtnStatus.Released)
					)
				{
					_isKeyframeClicked = false;

					//키프레임 선택했는데 Up이벤트로 끝났군여?
					//마우스 이벤트를 일시 종료
					ReleaseMouseEvent();
				}


			}


			if(_leftBtnStatus == apMouse.MouseBtnStatus.Pressed || _rightBtnStatus == apMouse.MouseBtnStatus.Pressed)
			{
				isEventOccurred = false;
			}
			
			return isEventOccurred;
		}


		private static void OnDragKeyframeUp()
		{

			List<apAnimTimelineLayer> refreshLayer = new List<apAnimTimelineLayer>();

			//1. 기본적인 Keyframe 이동/복사에 대한 처리

			//Debug.Log("OnDragKeyframeUp [" + _moveKeyframeList.Count + "] (" + _selectType + ")");
			//이동한 이후에
			//Frame Index가 겹친게 있다면 Selection을 기준으로 남기고 나머지는 삭제해야한다.
			//if (_selection.AnimKeyframes != null && _selection.AnimKeyframes.Count > 0)
			if (_moveKeyframeList.Count > 0)
			{
				
				if (_selectType != SELECT_TYPE.Add)
				{
					//1) 이동
					//Undo 등록
					//Keyframe의 위치만 바꾼 것이므로
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_MoveKeyframe, _selection.Editor, _selection.Editor._portrait, null, false);

					//Debug.LogError(">>> Keryframe Move <<<");
					//선택한 프레임중 "현재 재생 프레임"과 같은게 있다면
					//"현재 재생 프레임"을 이동해야한다.
					int movePlayFrame = -1;
					bool isMovePlayFrame = false;
					if (_selection.AnimClip != null)
					{
						movePlayFrame = _selection.AnimClip.CurFrame;
					}


					for (int iKeyframe = 0; iKeyframe < _moveKeyframeList.Count; iKeyframe++)
					{
						MoveKeyframe moveKeyframe = _moveKeyframeList[iKeyframe];
						//apAnimKeyframe movedKeyframe = _selection.AnimKeyframes[iKeyframe];
						apAnimTimelineLayer parentLayer = moveKeyframe.ParentLayer;

						if (parentLayer == null)
						{ continue; }

						//키프레임 이동
						if (movePlayFrame >= 0 && moveKeyframe._srcKeyframe._frameIndex == movePlayFrame && !isMovePlayFrame)
						{
							movePlayFrame = moveKeyframe._nextFrameIndex;
							isMovePlayFrame = true;
						}
						moveKeyframe._srcKeyframe._frameIndex = moveKeyframe._nextFrameIndex;



						//겹쳐있고, 현재 키프레임 + 선택되지 않은 키프레임은 삭제한다.
						int nRemoved = parentLayer._keyframes.RemoveAll(delegate (apAnimKeyframe a)
						{
							if (a._frameIndex == moveKeyframe._nextFrameIndex)
							{
								if (a != moveKeyframe._srcKeyframe && !_selection.AnimKeyframes.Contains(a))
								{
									return true;
								}
							}
							return false;
						});

						if (nRemoved > 0)
						{
							parentLayer.SortAndRefreshKeyframes();//여기서 일시적으로 Refresh를 해주자
						}

						if (!refreshLayer.Contains(parentLayer))
						{
							refreshLayer.Add(parentLayer);
						}
					}

					if (isMovePlayFrame)
					{
						_selection.AnimClip.SetFrame_Editor(movePlayFrame);
					}
				}
				else
				{
					//2) 복사
					//Undo 등록
					//이건 Mod의 값도 바뀌는 것이다.
					apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_CopyKeyframe, 
											_selection.Editor,
											_selection.Editor._portrait, 
											_selection.AnimClip._targetMeshGroup, 
											null, false);


					//선택한 프레임중 "현재 재생 프레임"과 같은게 있다면
					//"현재 재생 프레임"을 이동해야한다.
					int movePlayFrame = -1;
					bool isMovePlayFrame = false;
					if (_selection.AnimClip != null)
					{
						movePlayFrame = _selection.AnimClip.CurFrame;
					}

					List<apAnimKeyframe> copiedKeyframes = new List<apAnimKeyframe>();

					//Debug.LogError(">>> Keryframe Copy <<<");
					//이동한 키에 맞게 복사하자
					//단, 이동 키값이 같으면 무시
					for (int iKeyframe = 0; iKeyframe < _moveKeyframeList.Count; iKeyframe++)
					{
						MoveKeyframe moveKeyframe = _moveKeyframeList[iKeyframe];

						if (moveKeyframe._startFrameIndex == moveKeyframe._nextFrameIndex)
						{
							continue;
						}

						//apAnimKeyframe movedKeyframe = _selection.AnimKeyframes[iKeyframe];
						apAnimTimelineLayer parentLayer = moveKeyframe.ParentLayer;
						if (parentLayer == null)
						{ continue; }

						//키프레임 이동
						if (movePlayFrame >= 0 && moveKeyframe._srcKeyframe._frameIndex == movePlayFrame && !isMovePlayFrame)
						{
							movePlayFrame = moveKeyframe._nextFrameIndex;
							isMovePlayFrame = true;
						}

						//겹쳐있고, 현재 키프레임 + 선택되지 않은 키프레임은 삭제한다.
						int nRemoved = parentLayer._keyframes.RemoveAll(delegate (apAnimKeyframe a)
						{
							if (a._frameIndex == moveKeyframe._nextFrameIndex)
							{
								if (a != moveKeyframe._srcKeyframe && !_selection.AnimKeyframes.Contains(a))
								{
									return true;
								}
							}
							return false;
						});

						if (nRemoved > 0)
						{
							parentLayer.SortAndRefreshKeyframes();//여기서 일시적으로 Refresh를 해주자
						}

						if (!refreshLayer.Contains(parentLayer))
						{
							refreshLayer.Add(parentLayer);
						}

						//복사한다.
						apAnimKeyframe copiedKeyframe = _selection.Editor.Controller.AddCopiedAnimKeyframe(
																moveKeyframe._nextFrameIndex,
																parentLayer,
																true,
																moveKeyframe._srcKeyframe,
																false,
																false);

						if(copiedKeyframe != null)
						{
							copiedKeyframes.Add(copiedKeyframe);
						}
					}

					if (copiedKeyframes.Count > 0)
					{
						//복사를 했다면 복사한 키프레임을 선택하자
						_selection.SetAnimMultipleKeyframe(copiedKeyframes, apGizmos.SELECT_TYPE.New, true);
					}

					//복사 후에 재생 프레임을 이동하자
					if (isMovePlayFrame)
					{
						_selection.AnimClip.SetFrame_Editor(movePlayFrame);
					}
				}

				
			}

			//2. Summary Common Keyframe에 대해서도 처리한다.
			if(_moveCommonKeyframeList.Count > 0)
			{
				//처리 전에 먼저,
				//이동 대상이 되는 전체 키프레임을 리스트로 정리한다.
				List<apAnimKeyframe> targetKeyframes = new List<apAnimKeyframe>();

				for (int iCommon = 0; iCommon < _moveCommonKeyframeList.Count; iCommon++)
				{
					MoveCommonKeyframe moveCommonKeyframe = _moveCommonKeyframeList[iCommon];

					for (int iSubKeyframe = 0; iSubKeyframe < moveCommonKeyframe._srcCommonKeyframe._keyframes.Count; iSubKeyframe++)
					{
						targetKeyframes.Add(moveCommonKeyframe._srcCommonKeyframe._keyframes[iSubKeyframe]);
					}
				}

				if(_selectType != SELECT_TYPE.Add)
				{
					//Undo 등록
					apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_MoveKeyframe, 
																_selection.Editor,
																_selection.Editor._portrait, 
																_selection.AnimClip._targetMeshGroup, null, false);

					//선택한 프레임중 "현재 재생 프레임"과 같은게 있다면
					//"현재 재생 프레임"을 이동해야한다.
					int movePlayFrame = -1;
					bool isMovePlayFrame = false;
					if (_selection.AnimClip != null)
					{
						movePlayFrame = _selection.AnimClip.CurFrame;
					}
					

					for (int iCommon = 0; iCommon < _moveCommonKeyframeList.Count; iCommon++)
					{
						MoveCommonKeyframe moveCommonKeyframe = _moveCommonKeyframeList[iCommon];

						for (int iSubKeyframe = 0; iSubKeyframe < moveCommonKeyframe._srcCommonKeyframe._keyframes.Count; iSubKeyframe++)
						{
							apAnimKeyframe keyframe = moveCommonKeyframe._srcCommonKeyframe._keyframes[iSubKeyframe];

							//apAnimKeyframe movedKeyframe = _selection.AnimKeyframes[iKeyframe];
							apAnimTimelineLayer parentLayer = keyframe._parentTimelineLayer;

							if (parentLayer == null)
							{ continue; }

							//키프레임 이동
							if (movePlayFrame >= 0 && keyframe._frameIndex == movePlayFrame && !isMovePlayFrame)
							{
								movePlayFrame = moveCommonKeyframe._nextFrameIndex;
								isMovePlayFrame = true;
							}


							keyframe._frameIndex = moveCommonKeyframe._nextFrameIndex;

							//겹쳐있고, 현재 키프레임 + 선택되지 않은 키프레임은 삭제한다.
							int nRemoved = parentLayer._keyframes.RemoveAll(delegate (apAnimKeyframe a)
							{
								if (a._frameIndex == moveCommonKeyframe._nextFrameIndex)
								{
									if (a != keyframe && !targetKeyframes.Contains(a))
									{
										return true;
									}
								}
								return false;
							});

							if (nRemoved > 0)
							{
								parentLayer.SortAndRefreshKeyframes();//여기서 일시적으로 Refresh를 해주자
							}


							if (!refreshLayer.Contains(parentLayer))
							{
								refreshLayer.Add(parentLayer);
							}
						}
						
					}

					if (isMovePlayFrame)
					{
						_selection.AnimClip.SetFrame_Editor(movePlayFrame);
					}
				}
				else
				{
					//2) 복사
					//Undo 등록
					apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_CopyKeyframe, 
						_selection.Editor,
						_selection.Editor._portrait, 
						_selection.AnimClip._targetMeshGroup, null, false);


					//선택한 프레임중 "현재 재생 프레임"과 같은게 있다면
					//"현재 재생 프레임"을 이동해야한다.
					int movePlayFrame = -1;
					bool isMovePlayFrame = false;
					if (_selection.AnimClip != null)
					{
						movePlayFrame = _selection.AnimClip.CurFrame;
					}


					List<apAnimKeyframe> copiedKeyframes = new List<apAnimKeyframe>();

					for (int iCommon = 0; iCommon < _moveCommonKeyframeList.Count; iCommon++)
					{
						MoveCommonKeyframe moveCommonKeyframe = _moveCommonKeyframeList[iCommon];

						if (moveCommonKeyframe._startFrameIndex == moveCommonKeyframe._nextFrameIndex)
						{
							continue;
						}

						for (int iSubKeyframe = 0; iSubKeyframe < moveCommonKeyframe._srcCommonKeyframe._keyframes.Count; iSubKeyframe++)
						{
							apAnimKeyframe keyframe = moveCommonKeyframe._srcCommonKeyframe._keyframes[iSubKeyframe];

							apAnimTimelineLayer parentLayer = keyframe._parentTimelineLayer;
							if (parentLayer == null)
							{ continue; }


							if (movePlayFrame >= 0 && keyframe._frameIndex == movePlayFrame && !isMovePlayFrame)
							{
								movePlayFrame = moveCommonKeyframe._nextFrameIndex;
								isMovePlayFrame = true;
							}


							//겹쳐있고, 현재 키프레임 + 선택되지 않은 키프레임은 삭제한다.
							int nRemoved = parentLayer._keyframes.RemoveAll(delegate (apAnimKeyframe a)
							{
								if (a._frameIndex == moveCommonKeyframe._nextFrameIndex)
								{
									if (a != keyframe && !targetKeyframes.Contains(a))
									{
										return true;
									}
								}
								return false;
							});

							if (nRemoved > 0)
							{
								parentLayer.SortAndRefreshKeyframes();//여기서 일시적으로 Refresh를 해주자
							}

							if (!refreshLayer.Contains(parentLayer))
							{
								refreshLayer.Add(parentLayer);
							}

							//복사한다.
							apAnimKeyframe copiedKeyframe = _selection.Editor.Controller.AddCopiedAnimKeyframe(
																		moveCommonKeyframe._nextFrameIndex,
																		parentLayer,
																		true,
																		keyframe,
																		false,
																		false);

							if(copiedKeyframe != null)
							{
								copiedKeyframes.Add(copiedKeyframe);
							}
						}

						
					}

					if (copiedKeyframes.Count > 0)
					{
						//복사를 했다면 복사한 키프레임을 선택하자
						_selection.SetAnimMultipleKeyframe(copiedKeyframes, apGizmos.SELECT_TYPE.New, true);
					}

					//복사 후에 재생 프레임을 이동하자
					if (isMovePlayFrame)
					{
						_selection.AnimClip.SetFrame_Editor(movePlayFrame);
					}

				}

				
			}

			

			for (int i = 0; i < refreshLayer.Count; i++)
			{
				refreshLayer[i].SortAndRefreshKeyframes();
			}


			//Common AnimKeyframe을 갱신
			_selection.RefreshCommonAnimKeyframes();
			_selection.AutoSelectAnimWorkKeyframe();

			_moveKeyframeList.Clear();
			_moveCommonKeyframeList.Clear();

			//_selection.Editor.RefreshControllerAndHierarchy();

			//Refresh 추가
			//_selection.RefreshAnimEditing(true);
			//_selection.RefreshAnimEditing(false);

			//드래그가 끝났으면 무조건 마우스 이벤트 종료
			ReleaseMouseEvent();
		}

		private static Vector2 FrameToPos_Main(int frame, int posY)
		{
			return new Vector2(
				X_OFFSET + (((frame - _startFrame) * _widthPerFrame) - _scrollPos.x),
				(_layoutHeight_Header + posY) - _scrollPos.y
				);
		}

		private static float FrameToPosX_Main(int frame)
		{
			return X_OFFSET + (((frame - _startFrame) * _widthPerFrame) - _scrollPos.x);
		}


		private static int PosToFrame(float posX)
		{
			//posX = X_OFFSET + (((frame - _startFrame) * _widthPerFrame) - _scrollPos.x)
			//posX - X_OFFSET + _scrollPos.X = ((frame - _startFrame) * _widthPerFrame)
			//(posX - X_OFFSET + _scrollPos.X) / _widthPerFrame = frame - _startFrame
			//(posX - X_OFFSET + _scrollPos.X) / _widthPerFrame + _startFrame = frame

			return (int)(((posX - X_OFFSET + _scrollPos.x) / (float)_widthPerFrame) + 0.5f) + _startFrame;
		}

		public static int FrameOnMouseX
		{
			get
			{
				return PosToFrame(_mousePos.x);
			}
		}

		

		//------------------------------------------------------------------
		private static bool IsMouseUpdatable_Down(bool isLeft, bool isPressedAllowed = false)
		{
			if (!_isMouseEvent || _isMouseEventUsed || _selection == null)
			{
				return false;
			}

			if (isLeft)
			{
				if (isPressedAllowed)
				{
					if (_leftBtnStatus != apMouse.MouseBtnStatus.Down &&
						_leftBtnStatus != apMouse.MouseBtnStatus.Pressed)
					{
						return false;
					}
				}
				else
				{
					if (_leftBtnStatus != apMouse.MouseBtnStatus.Down)
					{
						return false;
					}
				}

			}
			else
			{
				if (isPressedAllowed)
				{
					if (_rightBtnStatus != apMouse.MouseBtnStatus.Down &&
						_rightBtnStatus != apMouse.MouseBtnStatus.Pressed)
					{
						return false;
					}
				}
				else
				{
					if (_rightBtnStatus != apMouse.MouseBtnStatus.Down)
					{
						return false;
					}
				}
			}

			if (_mousePos.x > 0 && _mousePos.x < _layoutWidth &&
				_mousePos.y > 0 && _mousePos.y < _layoutHeight_Total)
			{
				return true;
			}
			return false;
		}


		private static bool IsTargetSelectable(Vector2 mousePos, Vector2 targetPos, float width, float height)
		{
			float halfWidth = width / 2.0f;
			float halfHeight = height / 2.0f;
			if (mousePos.x >= (targetPos.x - (halfWidth + 0.5f)) && mousePos.x <= (targetPos.x + halfWidth + 0.5f) &&
				mousePos.y >= (targetPos.y - (halfHeight + 0.5f)) && mousePos.y <= (targetPos.y + halfHeight + 0.5f))
			{
				return true;
			}
			return false;
		}

		private static bool IsTargetSelectable_Area(Vector2 startPos, Vector2 endPos, Vector2 targetPos, float targetSizeW, float targetSizeH)
		{
			float halfWidth = targetSizeW / 2.0f;
			float halfHeight = targetSizeH / 2.0f;

			//조금이라도 걸치면 선택되어야 한다.

			float min_X = Mathf.Min(startPos.x, endPos.x);
			float max_X = Mathf.Max(startPos.x, endPos.x);
			float min_Y = Mathf.Min(startPos.y, endPos.y);
			float max_Y = Mathf.Max(startPos.y, endPos.y);

			if (min_X < targetPos.x + halfWidth && targetPos.x - halfWidth < max_X &&
				min_Y < targetPos.y + halfHeight && targetPos.y - halfHeight < max_Y)
			{
				return true;
			}
			return false;
		}

		private static void SetTimelineEvent(TIMELINE_EVENT timelineEvent)
		{
			//if(timelineEvent == TIMELINE_EVENT.DragFrame)
			//{
			//	Debug.LogError("Start Drag Frame : " + _timelineEvent + " >> Drag");
			//}

			if (timelineEvent == TIMELINE_EVENT.None)
			{
				if (_moveKeyframeList.Count > 0)
				{
					_moveKeyframeList.Clear();
				}
				if (_moveCommonKeyframeList.Count > 0)
				{
					_moveCommonKeyframeList.Clear();
				}

				_isKeyframeClicked = false;
				_isSelectedKeyframeClick = false;
			}

			//if (_timelineEvent != timelineEvent)
			//{
			//	if (timelineEvent == TIMELINE_EVENT.None)
			//	{
			//		_moveKeyframeList.Clear();
			//		_moveCommonKeyframeList.Clear();
			//	}

			//	if (timelineEvent != TIMELINE_EVENT.None)
			//	{
			//		//기존 코드 >> 변경 : Down 이벤트 받는 곳으로 직접 이동
			//		//_mousePos_Down = _mousePos;

			//		//_scroll_Down = _scrollPos;

			//		//if (_mousePos_Down.y < _layoutHeight_Header)
			//		//{
			//		//	_downClipArea = CLIP_TYPE.Header;
			//		//}
			//		//else
			//		//{
			//		//	_downClipArea = CLIP_TYPE.Main;
			//		//}
			//		//_isMouseEventUsed = true;//<<??

			//		//Debug.Log("SetTimelineEvent : " + timelineEvent);
			//	}
			//}

			_timelineEvent = timelineEvent;
		}


		public static void AddCursorRect(Vector2 pos, float width, float height, MouseCursor cursorType)
		{
			if (pos.x < 0 || pos.x > _layoutWidth || pos.y < 0 || pos.y > _layoutHeight_Total)
			{
				return;
			}
			//pos.x += _layoutPosX;
			//pos.y += _layoutPosY_Header;
			pos.x -= width / 2;
			pos.y -= height / 2;

			//Debug.Log("AddCursorRect [ " + pos + " ]");
			//EditorGUI.DrawRect(new Rect(pos.x, pos.y, width, height), Color.yellow);
			EditorGUIUtility.AddCursorRect(new Rect(pos.x, pos.y, width, height), cursorType);
		}


		private static bool IsMouseInLayout(Vector2 mousePos)
		{

			if (mousePos.x < 0.0f || mousePos.x > (_layoutWidth - 12) ||//스크롤 두께만큼 영역이 좁아진다.
				mousePos.y < 0.0f || mousePos.y > _layoutHeight_Total)
			{
				return false;
			}

			return true;
		}


		private static void ReleaseMouseEvent()
		{
			_isMouseEventUsed = true;
			_isMouseIgnored_UpOrUsed = true;
			//_leftBtnStatus = apMouse.MouseBtnStatus.Released;
			//_rightBtnStatus = apMouse.MouseBtnStatus.Released;

			//Debug.Log("Release Mouse Event : " + strDebug);

			SetTimelineEvent(TIMELINE_EVENT.None);
		}

		private static void EditorRepaint()
		{
			if(_selection == null)
			{
				return;
			}

			//_selection.Editor.SetRepaint();
		}
	}

}