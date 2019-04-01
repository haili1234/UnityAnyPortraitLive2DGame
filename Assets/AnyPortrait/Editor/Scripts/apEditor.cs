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
//using UnityEngine.Profiling;
using System.Collections;
using System;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{

	public class apEditor : EditorWindow
	{
		private static apEditor s_window = null;

		public static bool IsOpen()
		{
			return (s_window != null);
		}

		public static apEditor CurrentEditor
		{
			get
			{
				return s_window;
			}
		}

		

		/// <summary>
		/// 작업을 위해서 상세하게 디버그 로그를 출력할 것인가. (True인 경우 정상적인 처리 중에도 Debug가 나온다.)
		/// </summary>
		public static bool IS_DEBUG_DETAILED
		{
			get
			{
				return false;
			}
		}


		//--------------------------------------------------------------
		[MenuItem("Window/AnyPortrait/2D Editor", false, 1), ExecuteInEditMode]
		public static void ShowWindow()
		{
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apEditor), false, "AnyPortrait");
			apEditor curTool = curWindow as apEditor;

			//에러가 발생하는 Dialog는 여기서 미리 꺼준다.
			apDialog_PortraitSetting.CloseDialog();
			apDialog_Bake.CloseDialog();

			if (curTool != null)
			{
				curTool.LoadEditorPref();
			}
			if (curTool != null && curTool != s_window)
			//if(curTool != null)
			{
				Debug.ClearDeveloperConsole();
				s_window = curTool;
				//s_window.position = new Rect(0, 0, 200, 200);
				s_window.Init();
				s_window._isFirstOnGUI = true;
			}

		}

		public static void CloseEditor()
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

		//--------------------------------------------------------------
		private bool _isRepaintable = false;
		private bool _isUpdateWhenInputEvent = false;
		public void SetRepaint()
		{
			_isRepaintable = true;
			_isUpdateWhenInputEvent = true;
		}

		private bool _isRefreshClippingGL = false;//<<렌더링을 1프레임 쉰다. 모드 바뀔때 호출 필요
		public void RefreshClippingGL()
		{
			_isRefreshClippingGL = true;
		}

		//--------------------------------------------------------------
		private apEditorController _controller = null;
		public apEditorController Controller
		{
			get
			{
				if (_controller == null)
				{
					_controller = new apEditorController();
					_controller.SetEditor(this);
				}
				return _controller;
			}
		}


		private apVertexController _vertController = new apVertexController();
		public apVertexController VertController { get { return _vertController; } }

		private apGizmoController _gizmoController = new apGizmoController();
		public apGizmoController GizmoController { get { return _gizmoController; } }

		public apController ParamControl
		{
			get
			{
				if (_portrait == null)
				{
					return null;
				}
				else
				{
					return _portrait._controller;
				}
			}
		}

		private apPhysicsPreset _physicsPreset = new apPhysicsPreset();
		public apPhysicsPreset PhysicsPreset { get { return _physicsPreset; } }

		private apControlParamPreset _controlParamPreset = new apControlParamPreset();
		public apControlParamPreset ControlParamPreset {  get { return _controlParamPreset; } }

		private apHotKey _hotKey = new apHotKey();
		public apHotKey HotKey { get { return _hotKey; } }

		private apExporter _exporter = null;
		public apExporter Exporter { get { if (_exporter == null) { _exporter = new apExporter(this); } return _exporter; } }

		private apSequenceExporter _sequenceExporter = null;
		public apSequenceExporter SeqExporter { get { if (_sequenceExporter == null) { _sequenceExporter = new apSequenceExporter(this); } return _sequenceExporter; } }

		private apLocalization _localization = new apLocalization();
		public apLocalization Localization { get { return _localization; } }
		

		private apBackup _backup = new apBackup();
		public apBackup Backup {  get { return _backup; } }

		private apOnion _onion = new apOnion();
		public apOnion Onion {  get { return _onion; } }

		public apControlParam.CATEGORY _curParamCategory = apControlParam.CATEGORY.Head |
															apControlParam.CATEGORY.Body |
															apControlParam.CATEGORY.Face |
															apControlParam.CATEGORY.Hair |
															apControlParam.CATEGORY.Equipment |
															apControlParam.CATEGORY.Force |
															apControlParam.CATEGORY.Etc;



		public enum TAB_LEFT
		{
			Hierarchy = 0, Controller = 1
		}
		private TAB_LEFT _tabLeft = TAB_LEFT.Hierarchy;
		public TAB_LEFT LeftTab {  get { return _tabLeft; } }

		public Vector2 _scroll_MainLeft = Vector2.zero;
		public Vector2 _scroll_MainCenter = Vector2.zero;
		public Vector2 _scroll_MainRight = Vector2.zero;
		public Vector2 _scroll_MainRight2 = Vector2.zero;
		public Vector2 _scroll_Bottom = Vector2.zero;

		public Vector2 _scroll_MainRight_Lower_MG_Mesh = Vector2.zero;
		public Vector2 _scroll_MainRight_Lower_MG_Bone = Vector2.zero;
		public Vector2 _scroll_MainRight_Lower_Anim = Vector2.zero;

		public int _iZoomX100 = 11;//11 => 100
		public const int ZOOM_INDEX_DEFAULT = 11;
		public int[] _zoomListX100 = new int[] {    10, 20, 30, 40, 50, 60, 70, 80, 85, 90, 95, 100/*(11)*/,
												105, 110, 120, 140, 160, 180, 200, 250, 300, 350, 400, 450,
												500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000 };

		public Color _guiMainEditorColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
		public Color _guiSubEditorColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);

		public Material _mat_Color = null;
		public Material _mat_GUITexture = null;
		public Material[] _mat_Texture_Normal = null;//기본 White * Multiply (VColor 기본값이 White)
		public Material[] _mat_Texture_VertAdd = null;//Weight가 가능한 Add Vertex Color (VColor 기본값이 Black)
		//public Material[] _mat_MaskedTexture = null;//<<구형 버전
		public Material[] _mat_Clipped = null;
		public Material _mat_MaskOnly = null;
		//Onion을 위한 ToneColor
		public Material _mat_ToneColor_Normal = null;
		public Material _mat_ToneColor_Clipped = null;
		public Material _mat_Alpha2White = null;

		public enum BONE_RENDER_MODE
		{
			None,
			Render,
			RenderOutline,
		}
		/// <summary>
		/// Bone을 렌더링 하는가
		/// </summary>
		public BONE_RENDER_MODE _boneGUIRenderMode = BONE_RENDER_MODE.Render;



		//public Material _mat_Color2 = null;
		//public Material _mat_Texture2 = null;


		//private Texture2D _btnIcon_Select = null;
		//private Texture2D _btnIcon_Move = null;
		//private Texture2D _btnIcon_Rotate = null;
		//private Texture2D _btnIcon_Scale = null;
		public enum LANGUAGE
		{
			/// <summary>영어</summary>
			English = 0,
			/// <summary>한국어</summary>
			Korean = 1,
			/// <summary>프랑스어</summary>
			French = 2,
			/// <summary>독일어</summary>
			German = 3,
			/// <summary>스페인어</summary>
			Spanish = 4,
			/// <summary>이탈리아어</summary>
			Italian = 5,
			/// <summary>덴마크어</summary>
			Danish = 6,
			/// <summary>일본어</summary>
			Japanese = 7,
			/// <summary>중국어-번체</summary>
			Chinese_Traditional = 8,
			/// <summary>중국어-간체</summary>
			Chinese_Simplified = 9,
			/// <summary>폴란드어</summary>
			Polish = 10,
		}

		public LANGUAGE _language = LANGUAGE.English;

		//색상 옵션
		public Color _colorOption_Background = new Color(0.2f, 0.2f, 0.2f, 1.0f);
		public Color _colorOption_GridCenter = new Color(0.7f, 0.7f, 0.3f, 1.0f);
		public Color _colorOption_Grid = new Color(0.3f, 0.3f, 0.3f, 1.0f);


		public Color _colorOption_MeshEdge = new Color(1.0f, 0.5f, 0.0f, 0.9f);
		public Color _colorOption_MeshHiddenEdge = new Color(1.0f, 1.0f, 0.0f, 0.7f);
		public Color _colorOption_Outline = new Color(0.0f, 0.5f, 1.0f, 0.7f);
		public Color _colorOption_TransformBorder = new Color(0.0f, 1.0f, 1.0f, 1.0f);
		public Color _colorOption_AtlasBorder = new Color(0.0f, 1.0f, 1.0f, 0.5f);

		public Color _colorOption_VertColor_NotSelected = new Color(0.0f, 0.3f, 1.0f, 0.6f);
		public Color _colorOption_VertColor_Selected = new Color(1.0f, 0.0f, 0.0f, 1.0f);

		public Color _colorOption_GizmoFFDLine = new Color(1.0f, 0.5f, 0.2f, 0.9f);
		public Color _colorOption_GizmoFFDInnerLine = new Color(1.0f, 0.7f, 0.2f, 0.7f);

		public Color _colorOption_OnionToneColor = new Color(0.1f, 0.2f, 0.5f, 0.7f);

		public bool _guiOption_isFPSVisible = true;
		public bool _guiOption_isStatisticsVisible = false;

		//백업 옵션
		//자동 백업 옵션 처리
		public bool _backupOption_IsAutoSave = true;//자동 백업을 지원하는가
		public string _backupOption_BaseFolderName = "AnyPortraitBackup";//폴더를 지정해야한다. (프로젝트 폴더 기준 + 씬이름+에셋)
		public int _backupOption_Minute = 30;//기본은 30분마다 한번씩 저장한다.

		public string _bonePose_BaseFolderName = "AnyPortraitBonePose";


		//시작 화면 옵션
		//매번 시작할 것인가
		//마지막으로 열린 날짜 (날짜가 바뀌면 열린다.)
		public bool _startScreenOption_IsShowStartup = true;
		public int _startScreenOption_LastMonth = 0;
		public int _startScreenOption_LastDay = 0;

		public int _updateLogScreen_LastVersion = 0;

		//Bake시 Color Space를 어디에 맞출 것인가
		public bool _isBakeColorSpaceToGamma = true;

		//Modifier Lock 옵션
		public bool _modLockOption_CalculateIfNotAddedOther = false;
		public bool _modLockOption_ColorPreview_Lock = false;
		public bool _modLockOption_ColorPreview_Unlock = true;//<<
		public bool _modLockOption_BonePreview_Lock = false;
		public bool _modLockOption_BonePreview_Unlock = true;//<<
		//public bool _modLockOption_MeshPreview_Lock = false;
		//public bool _modLockOption_MeshPreview_Unlock = false;
		public bool _modLockOption_ModListUI_Lock = false;
		public bool _modLockOption_ModListUI_Unlock = false;

		//public Color _modLockOption_MeshPreviewColor = new Color(1.0f, 0.45f, 0.1f, 0.8f);
		public Color _modLockOption_BonePreviewColor = new Color(1.0f, 0.8f, 0.1f, 0.8f);


		//캡쳐 옵션
		//스샷용
		public int _captureFrame_PosX = 0;
		public int _captureFrame_PosY = 0;
		public int _captureFrame_SrcWidth = 500;
		public int _captureFrame_SrcHeight = 500;
		public int _captureFrame_DstWidth = 500;
		public int _captureFrame_DstHeight = 500;
		public int _captureFrame_SpriteUnitWidth = 500;
		public int _captureFrame_SpriteUnitHeight = 500;
		public int _captureFrame_SpriteMargin = 0;
		public Color _captureFrame_Color = Color.black;
		public bool _captureFrame_IsPhysics = false;
		public bool _isShowCaptureFrame = true;//Capture Frame을 GUI에서 보여줄 것인가
		public bool _isCaptureAspectRatioFixed = true;
		public int _captureFrame_GIFSampleQuality = 10;//낮을수록 용량이 높은거
		public int _captureFrame_GIFSampleLoopCount = 1;

		public enum CAPTURE_SPRITE_PACK_IMAGE_SIZE
		{
			s256 = 0,
			s512 = 1,
			s1024 = 2,
			s2048 = 3,
			s4096 = 4
		}
		public CAPTURE_SPRITE_PACK_IMAGE_SIZE _captureSpritePackImageWidth = CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024;
		public CAPTURE_SPRITE_PACK_IMAGE_SIZE _captureSpritePackImageHeight = CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024;

		//public enum CAPTURE_SPRITE_ANIM_FPS_METHOD
		//{
		//	AllFrames = 0,
		//	CommonFPS = 1
		//}
		//public CAPTURE_SPRITE_ANIM_FPS_METHOD _captureSpriteAnimFPSMethod = CAPTURE_SPRITE_ANIM_FPS_METHOD.AllFrames;
		//public int _captureSpriteAnimCommonFPS = 20;
		
		public enum CAPTURE_SPRITE_TRIM_METHOD
		{
			/// <summary>설정했던 크기 그대로</summary>
			Fixed = 0,
			/// <summary>설정 크기를 기준으로 여백이 있으면 크기가 줄어든다.</summary>
			Compressed = 1,
		}
		public CAPTURE_SPRITE_TRIM_METHOD _captureSpriteTrimSize = CAPTURE_SPRITE_TRIM_METHOD.Fixed;
		public bool _captureSpriteMeta_XML = false;
		public bool _captureSpriteMeta_JSON = false;
		public bool _captureSpriteMeta_TXT = false;
		

		//캡쳐시 위치/줌을 고정할 수 있다. (수동)
		public Vector2 _captureSprite_ScreenPos = Vector2.zero;
		public int _captureSprite_ScreenZoom = ZOOM_INDEX_DEFAULT;
		
		


		private List<apPortrait> _portraitsInScene = new List<apPortrait>();
		private bool _isPortraitListLoaded = false;

		//Portrait를 생성한다는 요청이 있다. => 이 요청은 Repaint 이벤트가 아닐때 실행된다.
		private bool _isMakePortraitRequest = false;
		private string _requestedNewPortraitName = "";

		private bool _isMakePortraitRequestFromBackupFile = false;
		private string _requestedLoadedBackupPortraitFilePath = "";

		// Gizmos
		//-------------------------------------------------------------
		private apGizmos _gizmos = null;
		public apGizmos Gizmos { get { return _gizmos; } }
		//-------------------------------------------------------------

		// Hierarchy
		//--------------------------------------------------------------
		private apEditorHierarchy _hierarchy = null;
		public apEditorHierarchy Hierarchy { get { return _hierarchy; } }


		private apEditorMeshGroupHierarchy _hierarchy_MeshGroup = null;
		public apEditorMeshGroupHierarchy Hierarchy_MeshGroup { get { return _hierarchy_MeshGroup; } }

		private apEditorAnimClipTargetHierarchy _hierarchy_AnimClip = null;
		public apEditorAnimClipTargetHierarchy Hierarchy_AnimClip { get { return _hierarchy_AnimClip; } }
		//--------------------------------------------------------------

		// Timeline GUI
		//--------------------------------------------------------------
		private apAnimClip _prevAnimClipForTimeline = null;
		private List<apTimelineLayerInfo> _timelineInfoList = new List<apTimelineLayerInfo>();
		public List<apTimelineLayerInfo> TimelineInfoList { get { return _timelineInfoList; } }

		public enum TIMELINE_INFO_SORT
		{
			Registered,//등록 순서대로..
			ABC,//가나다 순
			Depth,//깊이 순서대로 (RenderUnit 한정)
		}
		public TIMELINE_INFO_SORT _timelineInfoSortType = TIMELINE_INFO_SORT.Registered;//<<이게 기본값 (저장하자)
																						//--------------------------------------------------------------


		//Left, Right, Middle Mouse
		//-------------------------------------------------------------------------------
		public int _curMouseBtn = -1;
		public int MOUSE_BTN_LEFT { get { return 0; } }
		public int MOUSE_BTN_RIGHT { get { return 1; } }
		public int MOUSE_BTN_MIDDLE { get { return 2; } }
		public int MOUSE_BTN_LEFT_NOT_BOUND { get { return 3; } }
		public apMouse[] _mouseBtn = new apMouse[] { new apMouse(), new apMouse(), new apMouse(), new apMouse() };
		//------------------------------------------------------------------------------

		public Rect _mainGUIRect = new Rect();


		//Mesh Edit
		public enum BRUSH_TYPE
		{
			SetValue = 0, AddSubtract = 1,
		}
		public float[] _brushPreset_Size = new float[]
		{
			1, 2, 3, 4, 5, 6, 7, 8, 9,
			10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95,
			100, 110, 120, 130, 140, 150, 160, 170, 180, 190,
			200, 220, 240, 260, 280,
			300, 320, 340, 360, 380,
			400, 450, 500,
			//550, 600, 650, 700, 750, 800, 850, 900, 950,
			//1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000
		};
		public float BRUSH_MIN_SIZE { get { return _brushPreset_Size[0]; } }
		public float BRUSH_MAX_SIZE { get { return _brushPreset_Size[_brushPreset_Size.Length - 1]; } }

		#region [미사용 코드]
		//public float BRUSH_MIN_HARDNESS { get { return _brushPreset_Hardness[0]; } }
		//public float BRUSH_MAX_HARDNESS { get { return _brushPreset_Hardness[_brushPreset_Hardness.Length - 1]; } }
		//public float[] _brushPreset_Hardness = new float[]
		//{
		//	0.0f, 10.0f, 20.0f, 30.0f, 40.0f, 50.0f, 60.0f, 70.0f, 80.0f, 90.0f, 100.0f
		//};

		//public float[] _brushPreset_Alpha = new float[]
		//{
		//	0.0f, 10.0f, 20.0f, 30.0f, 40.0f, 50.0f, 60.0f, 70.0f, 80.0f, 90.0f, 100.0f
		//};
		//public float BRUSH_MIN_ALPHA { get { return _brushPreset_Alpha[0]; } }
		//public float BRUSH_MAX_ALPHA { get { return _brushPreset_Alpha[_brushPreset_Alpha.Length - 1]; } } 
		#endregion

		public BRUSH_TYPE _brushType_VolumeEdit = BRUSH_TYPE.SetValue;
		public float _brushValue_VolumeEdit = 50.0f;
		public float _brushSize_VolumeEdit = 20.0f;
		public float _brushHardness_VolumeEdit = 50.0f;
		public float _brushAlpha_VolumeEdit = 50.0f;
		public float _paintValue_VolumeEdit = 50.0f;


		public BRUSH_TYPE _brushType_Physic = BRUSH_TYPE.SetValue;
		public float _brushValue_Physic = 50.0f;
		public float _brushSize_Physic = 20.0f;
		public float _brushHardness_Physic = 50.0f;
		public float _brushAlpha_Physic = 50.0f;
		public float _paintValue_Physic = 50.0f;


		//Window 호출
		private enum DIALOG_SHOW_CALL
		{
			None,
			Setting,
			Bake,
			Capture,
		}
		private DIALOG_SHOW_CALL _dialogShowCall = DIALOG_SHOW_CALL.None;
		private EventType _curEventType = EventType.Ignore;
		//--------------------------------------------------------------

		public apPortrait _portrait = null;

		public Dictionary<string, int> _tmpValues = new Dictionary<string, int>();

		//--------------------------------------------------------------
		// 프레임 업데이트 변수
		private enum FRAME_TIMER_TYPE
		{
			Update, Repaint, None
		}
		#region [미사용 변수] apTimer로 옮겨서 처리한다.

		//private FRAME_TIMER_TYPE _frameTimerType = FRAME_TIMER_TYPE.None;
		//private DateTime _dateTime_Update = DateTime.Now;
		//private DateTime _dateTime_GUI = DateTime.Now;

		//private bool _isValidFrame = false;
		//private float _deltaTimePerValidFrame = 0.0f;
		//private bool _isCountDeltaTime = false;

		//private double _tDelta_Update = 0.0f;
		//private double _tDelta_GUI = 0.0f;

		////아주 짧은 시간동안 너무 큰 FPS로 시간 연산이 이루어진 경우,
		////tDelta를 0으로 리턴하는 대신, 그동안 시간을 누적시키고 있자.
		//private const double MIN_EDITOR_UPDATE_TIME = 0.01;//60FPS(0.0167), 100FPS
		//private float _tDeltaDelayed_Update = 0.0f;
		//private float _tDeltaDelayed_GUI = 0.0f;



		//public bool IsValidFrame {  get { return _isValidFrame; } }
		//public float DeltaFrameTime
		//{
		//	//수정 : Update와 OnGUI에서 값을 가져갈때
		//	//따로 체크를 해야한다. (Update에서 체크했던 타이머 값을 여러번 호출해서 사용하는 듯 하다)
		//	get
		//	{
		//		//if (_isCountDeltaTime)
		//		//{
		//		//	return _deltaTimePerValidFrame;
		//		//}
		//		switch (_frameTimerType)
		//		{
		//			case FRAME_TIMER_TYPE.Update:
		//				//return _tDeltaDelayed_Update;
		//				return apTimer.I.DeltaTime;

		//			case FRAME_TIMER_TYPE.GUIRepaint:
		//				//return _tDeltaDelayed_GUI;
		//				return 0.0f;

		//			case FRAME_TIMER_TYPE.None:
		//				return 0.0f;
		//		}
		//		return 0.0f;
		//	}
		//} 
		#endregion

		public float DeltaTime_Update { get { return apTimer.I.DeltaTime_Update; } }
		public float DeltaTime_UpdateAllFrame { get { return apTimer.I.DeltaTime_UpdateAllFrame; } }
		public float DeltaTime_Repaint { get { return apTimer.I.DeltaTime_Repaint; } }

		private float _avgFPS = 0.0f;
		private int _iAvgFPS = 0;
		public int FPS { get { return _iAvgFPS; } }

		//--------------------------------------------------------------
		// 단축키 관련 변수
		private bool _isHotKeyProcessable = true;
		private bool _isHotKeyEvent = false;
		private KeyCode _hotKeyCode = KeyCode.A;
		private bool _isHotKey_Ctrl = false;
		private bool _isHotKey_Alt = false;
		private bool _isHotKey_Shift = false;


		//--------------------------------------------------------------
		// OnGUI 이벤트 성격을 체크하기 위한 변수
		private bool _isGUIEvent = false;
		private Dictionary<string, bool> _delayedGUIShowList = new Dictionary<string, bool>();
		private Dictionary<string, bool> _delayedGUIToggledList = new Dictionary<string, bool>();

		//--------------------------------------------------------------
		// Inspector를 위한 변수들
		private apSelection _selection = null;
		public apSelection Select { get { return _selection; } }


		// 이미지 세트
		//--------------------------------------------------------------
		private apImageSet _imageSet = null;
		public apImageSet ImageSet { get { return _imageSet; } }


		//--------------------------------------------------------------
		// Mesh Edit 모드
		public enum MESH_EDIT_MODE
		{
			Setting,
			MakeMesh,//AddVertex + LinkEdge
			Modify,
			//AddVertex,//삭제합니더 => 
			//LinkEdge,//삭제
			PivotEdit,
			//VolumeWeight,//>삭제합니더
			//PhysicWeight,//>>삭제합니더

		}


		public enum MESH_EDIT_MODE_MAKEMESH
		{
			VertexAndEdge,
			VertexOnly,
			EdgeOnly,
			Polygon
		}

		public enum MESH_EDIT_RENDER_MODE
		{
			Normal, ZDepth
		}
		public MESH_EDIT_MODE _meshEditMode = MESH_EDIT_MODE.Setting;
		public MESH_EDIT_RENDER_MODE _meshEditZDepthView = MESH_EDIT_RENDER_MODE.Normal;
		public MESH_EDIT_MODE_MAKEMESH _meshEditeMode_MakeMesh = MESH_EDIT_MODE_MAKEMESH.VertexAndEdge;

		public enum ROOTUNIT_EDIT_MODE
		{
			Setting,
			Capture
		}
		public enum ROOTUNIT_CAPTURE_MODE
		{
			Thumbnail,
			ScreenShot,
			GIFAnimation,
			SpriteSheet
		}

		public ROOTUNIT_EDIT_MODE _rootUnitEditMode = ROOTUNIT_EDIT_MODE.Setting;
		public ROOTUNIT_CAPTURE_MODE _rootUnitCaptureMode = ROOTUNIT_CAPTURE_MODE.Thumbnail;

		// MeshGroup Edit 모드
		public enum MESHGROUP_EDIT_MODE
		{
			Setting,
			Bone,
			Modifier,
		}

		public MESHGROUP_EDIT_MODE _meshGroupEditMode = MESHGROUP_EDIT_MODE.Setting;

		public enum RIGHT_UPPER_LAYOUT
		{
			Show,
			Hide,
		}
		public RIGHT_UPPER_LAYOUT _right_UpperLayout = RIGHT_UPPER_LAYOUT.Show;

		// MeshGroup Right의 하위 메뉴
		public enum RIGHT_LOWER_LAYOUT
		{
			Hide,
			ChildList,
			//Add,//<<이거 삭제하자
			//AddMeshGroup
		}
		public RIGHT_LOWER_LAYOUT _rightLowerLayout = RIGHT_LOWER_LAYOUT.ChildList;


		public enum TIMELINE_LAYOUTSIZE
		{
			Size1, Size2, Size3,
		}
		public TIMELINE_LAYOUTSIZE _timelineLayoutSize = TIMELINE_LAYOUTSIZE.Size2;//<<기본값은 2

		public int[] _timelineZoomWPFPreset = new int[]
		//{ 1, 2, 5, 7, 10, 12, 15, 17, 20, 22, 25, 27, 30, 32, 35, 37, 40, 42, 45, 47, 50 };
		{ 50, 45, 40, 35, 30, 25, 22, 20, 17, 15, 12, 11, 10, 9, 8 };
		public const int DEFAULT_TIMELINE_ZOOM_INDEX = 10;
		public int _timelineZoom_Index = DEFAULT_TIMELINE_ZOOM_INDEX;
		public int WidthPerFrameInTimeline { get { return _timelineZoomWPFPreset[_timelineZoom_Index]; } }

		public bool _isAnimAutoScroll = true;


		//전체화면 기능 추가
		public bool _isFullScreenGUI = false;//<<기본값은 false이다.


		[Flags]
		public enum HIERARCHY_FILTER
		{
			None = 0,
			RootUnit = 1,
			Image = 2,
			Mesh = 4,
			MeshGroup = 8,
			Animation = 16,
			Param = 32,
			All = 63
		}
		public HIERARCHY_FILTER _hierarchyFilter = HIERARCHY_FILTER.All;

		





		//--------------------------------------------------------------
		// UI를 위한 변수들
		//private bool _isFold_PS_Main = false;
		//public bool _isFold_PS_Image = false;
		//public bool _isFold_PS_Mesh = false;

		//public bool _isFold_PS_MeshGroup = false;
		//public bool _isFold_PS_Bone = false;

		//public bool _isFold_PS_Face = false;
		//private bool _isMouseEvent = false;

		private bool _isNotification = false;
		private float _tNotification = 0.0f;
		private const float NOTIFICATION_TIME_LONG = 4.5f;
		private const float NOTIFICATION_TIME_SHORT = 1.2f;

		//GUI에 그려지는 작은 Noti를 출력하자
		private bool _isNotification_GUI = false;
		private float _tNotification_GUI = 0.0f;
		private string _strNotification_GUI = "";

		//GUI에 백업 처리를 하는 경우 업데이트를 하자
		private bool _isBackupProcessing = false;
		private float _tBackupProcessing_Label = 0.0f;
		private float _tBackupProcessing_Icon = 0.0f;
		private const float BACKUP_LABEL_TIME_LENGTH = 2.0f;
		private const float BACKUP_ICON_TIME_LENGTH = 0.8f;
		private Texture2D _imgBackupIcon_Frame1 = null;
		private Texture2D _imgBackupIcon_Frame2 = null;

		private bool _isUpdateAfterEditorRunning = false;

		public bool _isFirstOnGUI = false;

		/// <summary>
		/// Gizmo 등으로 강제로 업데이트를 한 경우에는 업데이트를 Skip한다.
		/// </summary>
		private bool _isUpdateSkip = false;

		public void SetUpdateSkip()
		{
			_isUpdateSkip = true;
			apTimer.I.ResetTime_Update();
		}


		// 화면 캡쳐 요청
		// 추가:맥버전은 매프레임마다 RenderTexture를 돌리면 안된다.
		// 카운트를 해야한다.
		private bool _isScreenCaptureRequest = false;
#if UNITY_EDITOR_OSX
		private bool _isScreenCaptureRequest_OSXReady = false;
		private int _screenCaptureRequest_Count = 0;
		private const int SCREEN_CAPTURE_REQUEST_OSX_COUNT = 5;
#endif
		private apScreenCaptureRequest _screenCaptureRequest = null;


		// 추가 4.1 : 객체의 추가/삭제가 있는 경우 전체 Link를 다시 해야한다.
		
		/// <summary>객체가 추가되거나 삭제된 경우 이 함수를 호출해야한다. 이후 Refresh될 때 다시 Link해야할 필요가 있기 때문</summary>
		public void OnAnyObjectAddedOrRemoved()
		{
			if(_portrait == null)
			{
				_recordList_TextureData.Clear();
				_recordList_Mesh.Clear();
				_recordList_MeshGroup.Clear();
				_recordList_AnimClip.Clear();
				_recordList_ControlParam.Clear();
				_recordList_Modifier.Clear();
				_recordList_AnimTimeline.Clear();
				_recordList_AnimTimelineLayer.Clear();
				_recordList_Transform.Clear();
				_recordList_Bone.Clear();
			}
			else
			{
				_recordList_TextureData.Clear();
				_recordList_Mesh.Clear();
				_recordList_MeshGroup.Clear();
				_recordList_AnimClip.Clear();
				_recordList_ControlParam.Clear();
				_recordList_Modifier.Clear();
				_recordList_AnimTimeline.Clear();
				_recordList_AnimTimelineLayer.Clear();
				_recordList_Transform.Clear();
				_recordList_Bone.Clear();

				if(_portrait._textureData != null && _portrait._textureData.Count > 0)
				{
					for (int i = 0; i < _portrait._textureData.Count; i++)
					{
						_recordList_TextureData.Add(_portrait._textureData[i]._uniqueID);
					}
				}

				if(_portrait._meshes != null && _portrait._meshes.Count > 0)
				{
					for (int i = 0; i < _portrait._meshes.Count; i++)
					{
						_recordList_Mesh.Add(_portrait._meshes[i]._uniqueID);
					}
				}

				apMeshGroup meshGroup = null;
				if(_portrait._meshGroups != null && _portrait._meshGroups.Count > 0)
				{
					for (int iMeshGroup = 0; iMeshGroup < _portrait._meshGroups.Count; iMeshGroup++)
					{
						meshGroup = _portrait._meshGroups[iMeshGroup];

						_recordList_MeshGroup.Add(meshGroup._uniqueID);

						//MeshGroup -> Modifier
						for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
						{
							_recordList_Modifier.Add(meshGroup._modifierStack._modifiers[iMod]._uniqueID);
						}

						//MeshGroup -> Transform
						for (int iMeshTF = 0; iMeshTF < meshGroup._childMeshTransforms.Count; iMeshTF++)
						{
							_recordList_Transform.Add(meshGroup._childMeshTransforms[iMeshTF]._transformUniqueID);
						}

						for (int iMeshGroupTF = 0; iMeshGroupTF < meshGroup._childMeshGroupTransforms.Count; iMeshGroupTF++)
						{
							_recordList_Transform.Add(meshGroup._childMeshGroupTransforms[iMeshGroupTF]._transformUniqueID);
						}

						for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
						{
							_recordList_Bone.Add(meshGroup._boneList_All[iBone]._uniqueID);
						}

					}
				}

				apAnimClip animClip = null;
				apAnimTimeline timeline = null;
				apAnimTimelineLayer timelineLayer = null;

				if(_portrait._animClips != null && _portrait._animClips.Count > 0)
				{
					for (int iAnimClip = 0; iAnimClip < _portrait._animClips.Count; iAnimClip++)
					{
						animClip = _portrait._animClips[iAnimClip];
						_recordList_AnimClip.Add(animClip._uniqueID);

						for (int iTimeline = 0; iTimeline < animClip._timelines.Count; iTimeline++)
						{
							timeline = animClip._timelines[iTimeline];
							_recordList_AnimTimeline.Add(timeline._uniqueID);

							for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
							{
								timelineLayer = timeline._layers[iLayer];
								_recordList_AnimTimelineLayer.Add(timelineLayer._uniqueID);
							}
						}
					}
				}
				
				if(_portrait._controller._controlParams != null && _portrait._controller._controlParams.Count > 0)
				{
					for (int i = 0; i < _portrait._controller._controlParams.Count; i++)
					{
						_recordList_ControlParam.Add(_portrait._controller._controlParams[i]._uniqueID);
					}
				}
			}
		}
		
		private List<int> _recordList_TextureData = new List<int>();
		private List<int> _recordList_Mesh = new List<int>();
		private List<int> _recordList_MeshGroup = new List<int>();
		private List<int> _recordList_AnimClip = new List<int>();
		private List<int> _recordList_AnimTimeline = new List<int>();
		private List<int> _recordList_AnimTimelineLayer = new List<int>();
		private List<int> _recordList_ControlParam = new List<int>();
		private List<int> _recordList_Modifier = new List<int>();
		private List<int> _recordList_Transform = new List<int>();
		private List<int> _recordList_Bone = new List<int>();

		
		
		
			

		//리소스 경로
		

		//--------------------------------------------------------------
		void OnDisable()
		{
			//재빌드 등을 통해서 다시 시작하는 경우 -> 다시 Init
			Init();
			SaveEditorPref();

			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
			//SceneView.onSceneGUIDelegate -= OnSceneViewEvent;
			//EditorApplication.modifierKeysChanged -= OnKeychanged;

			
			if(_backup != null)
			{
				_backup.StopForce();
			}

			//에디터를 끌때 EditorDirty를 수행한다.
			apEditorUtil.SetEditorDirty();
		}

		void OnEnable()
		{
			//Debug.Log("apEditor : OnEnable");

			if (s_window != this && s_window != null)
			{
				s_window.Close();
			}
			s_window = this;

			autoRepaintOnSceneChange = true;


			_isFirstOnGUI = true;
			if (apEditorUtil.IsGammaColorSpace())
			{
				Notification(apVersion.I.APP_VERSION, false, false);
			}
			else
			{
				Notification(apVersion.I.APP_VERSION + " (Linear Color Space Mode)", false, false);
			}

			//Debug.Log("Add Scene Delegate");
			Undo.undoRedoPerformed += OnUndoRedoPerformed;

			//SceneView.onSceneGUIDelegate += OnSceneViewEvent;
			//EditorApplication.modifierKeysChanged += OnKeychanged;
			PhysicsPreset.Load();
			ControlParamPreset.Load();

		}

		private void OnUndoRedoPerformed()
		{
			apEditorUtil.OnUndoRedoPerformed();

			
			if (_portrait != null)
			{
				//실제로 오브젝트가 복원되었거나 추가된게 사라지는 내역이 있는가
				apSelection.RestoredResult restoreResult = Select.SetAutoSelectWhenUndoPerformed(_portrait, 
											_recordList_TextureData,
											_recordList_Mesh,
											_recordList_MeshGroup,
											_recordList_AnimClip,
											_recordList_ControlParam,
											_recordList_Modifier,
											_recordList_AnimTimeline,
											_recordList_AnimTimelineLayer,
											_recordList_Transform,
											_recordList_Bone);
				


				//if(restoreResult._isAnyRestored)
				//{
				//	Debug.LogError("Undo에서 오브젝트가 추가되거나 삭제된 것을 되돌린다.");
				//}

				//4.1 추가
				//Undo 이후에 텍스쳐가 리셋되는 문제가 있다.
				for (int iTexture = 0; iTexture < _portrait._textureData.Count; iTexture++)
				{
					apTextureData textureData = _portrait._textureData[iTexture];
					if(textureData == null)
					{
						continue;
					}
					if(textureData._image == null)
					{
						//Debug.Log("Image가 없는 경우 발견 : " + textureData._name + " / [" + textureData._assetFullPath + "]");
						if(!string.IsNullOrEmpty(textureData._assetFullPath))
						{
							//Debug.Log("저장된 경로 : " + textureData._assetFullPath);
							Texture2D restoreImage = AssetDatabase.LoadAssetAtPath<Texture2D>(textureData._assetFullPath);
							if(restoreImage != null)
							{
								//Debug.Log("이미지 복원 완료");
								textureData._image = restoreImage;
							}
						}
					}
				}


				//Mesh의 TextureData를 다시 확인해봐야한다.
				for (int iMesh = 0; iMesh < _portrait._meshes.Count; iMesh++)
				{
					apMesh mesh = _portrait._meshes[iMesh];
					if(!mesh.IsTextureDataLinked)
					{
						//Link가 풀렸다면..
						mesh.SetTextureData(_portrait.GetTexture(mesh.LinkedTextureDataID));
					}
				}

				
				
				_portrait.LinkAndRefreshInEditor(restoreResult._isAnyRestored);
				
				if (Select.SelectionType == apSelection.SELECTION_TYPE.Mesh)
				{
					if (Select.Mesh != null)
					{
						Select.Mesh.MakeOffsetPosMatrix();
						Select.Mesh.RefreshPolygonsToIndexBuffer();
					}
				}
				else if(Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
				{
					if(Select.MeshGroup != null)
					{
						if (restoreResult._isAnyRestored)
						{
							Select.MeshGroup.SortRenderUnits(true);
							Select.MeshGroup.SetDirtyToReset();
							Select.MeshGroup.SetDirtyToSort();
						}
						Select.MeshGroup.RefreshForce(true, 0.0f);

						if(Select.MeshGroup._boneList_All != null &&
							Select.MeshGroup._boneList_All.Count > 0)
						{

							for (int i = 0; i < Select.MeshGroup._boneList_Root.Count; i++)
							{
								Select.MeshGroup._boneList_Root[i].MakeWorldMatrix(true);
								Select.MeshGroup._boneList_Root[i].GUIUpdate(true);
							}

							Select.MeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
							Select.MeshGroup.RefreshForce(true, 0.0f);
						}

						
						
					}
				}
				else if(Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
				{
					if(Select.AnimClip != null)
					{
						Select.AnimClip.RefreshTimelines();
						Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true, true);

						if (Select.AnimClip._targetMeshGroup != null)
						{
							if (restoreResult._isAnyRestored)
							{
								Select.AnimClip._targetMeshGroup.SortRenderUnits(true);
								Select.AnimClip._targetMeshGroup.SetDirtyToReset();
								Select.AnimClip._targetMeshGroup.SetDirtyToSort();
							}
							Select.AnimClip._targetMeshGroup.RefreshForce(true, 0.0f);

							if (Select.AnimClip._targetMeshGroup._boneList_All != null &&
								Select.AnimClip._targetMeshGroup._boneList_All.Count > 0)
							{
								
								for (int i = 0; i < Select.AnimClip._targetMeshGroup._boneList_Root.Count; i++)
								{
									Select.AnimClip._targetMeshGroup._boneList_Root[i].MakeWorldMatrix(true);
									Select.AnimClip._targetMeshGroup._boneList_Root[i].GUIUpdate(true);
								}

								Select.AnimClip._targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
								Select.AnimClip._targetMeshGroup.RefreshForce(true, 0.0f);
							}

							
						}
					}
				}


				//만약 FFD 모드 중이었다면 FFD 중단
				if (Gizmos.IsFFDMode)
				{
					Gizmos.RevertTransformObjects(null);
				}

				if(restoreResult._isAnyRestored)
				{
					Hierarchy.SetNeedReset();
					//+ ID 리셋해야함
					_portrait.RefreshAllUniqueIDs();
				}
				RefreshControllerAndHierarchy();
				if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
				{
					RefreshTimelineLayers(restoreResult._isAnyRestored);//<<이걸 True로 할 때가 있는데;
				}

				//자동으로 페이지 전환
				if(restoreResult._isAnyRestored)
				{
					Select.SetAutoSelectOrUnselectFromRestore(restoreResult, _portrait);
					RefreshControllerAndHierarchy();
				}

				OnAnyObjectAddedOrRemoved();

			}
			Repaint();


			Notification("Undo / Redo", true, false);

		}
		

		/// <summary>
		/// 화면내에 Notification Ballon을 출력합니다.
		/// </summary>
		/// <param name="strText"></param>
		/// <param name="_isShortTime"></param>
		public void Notification(string strText, bool _isShortTime, bool isDrawBalloon)
		{
			if (string.IsNullOrEmpty(strText))
			{
				return;

			}
			if (isDrawBalloon)
			{
				RemoveNotification();
				ShowNotification(new GUIContent(strText));
				_isNotification = true;
				if (_isShortTime)
				{
					_tNotification = NOTIFICATION_TIME_SHORT;
				}
				else
				{
					_tNotification = NOTIFICATION_TIME_LONG;
				}
			}
			else
			{
				_isNotification_GUI = true;
				if (_isShortTime)
				{
					_tNotification_GUI = NOTIFICATION_TIME_SHORT;
				}
				else
				{
					_tNotification_GUI = NOTIFICATION_TIME_LONG;
				}
				_strNotification_GUI = strText;
			}
		}

		private void Init()
		{
			//_tab = TAB.ProjectSetting;
			_portrait = null;
			_portraitsInScene.Clear();
			_selection = new apSelection(this);
			_controller = new apEditorController();
			_controller.SetEditor(this);

			_hierarchy = new apEditorHierarchy(this);
			_hierarchy_MeshGroup = new apEditorMeshGroupHierarchy(this);
			_hierarchy_AnimClip = new apEditorAnimClipTargetHierarchy(this);
			_imageSet = new apImageSet();

			_mat_Color = null;
			_mat_GUITexture = null;
			//_mat_MaskedTexture = null;
			_mat_Texture_Normal = null;
			_mat_Texture_VertAdd = null;

			_gizmos = new apGizmos(this);
			_gizmoController = new apGizmoController();
			_gizmoController.SetEditor(this);

			wantsMouseMove = true;

			_dialogShowCall = DIALOG_SHOW_CALL.None;

			//설정값을 로드하자
			LoadEditorPref();

			
			PhysicsPreset.Load();//<<로드!
			PhysicsPreset.Save();//그리고 바로 저장 한번 더

			ControlParamPreset.Load();
			ControlParamPreset.Save();


			_isMakePortraitRequest = false;
			_isMakePortraitRequestFromBackupFile = false;

			_isFullScreenGUI = false;

			apDebugLog.I.Clear();

			
		}

		//private DateTime _prevDateTime = DateTime.Now;
		private float _memGC = 0.0f;

		//private DateTime _prevUpdateFrameDateTime = DateTime.Now;
		//private float _tRepaintTimer = 0.0f;
		//private const float REPAINT_MIN_TIME = 0.01f;//1/100초 이내에서는 Repaint를 하지 않는다.
		private bool _isRepaintTimerUsable = false;//<<이게 True일때만 Repaint된다. Repaint하고나면 이 값은 False가 됨
		private bool _isValidGUIRepaint = false;//<<이게 True일때 OnGUI의 Repaint 이벤트가 "제어가능한 유효한 호출"임을 나타낸다. (False일땐 유니티가 자체적으로 Repaint 한 것)

		//private bool _isDelayedFrameSkip = false;//만약 강제로 업데이트를 했다면, 그 직후엔 업데이트를 할 필요가 없다.
		//private float _tDelayedFrameSkipCount = 0.0f;
		//private const float DELAYED_FRAME_SKIP_TIME = 0.1f;

		//public void SetDelayedFrameSkip()
		//{
		//	_isDelayedFrameSkip = true;
		//	_tDelayedFrameSkipCount = 0.0f;
		//}

		void Update()
		{
			if (EditorApplication.isPlaying)
			{
				return;
			}

			//_isCountDeltaTime = true;



			//업데이트 시간과 상관없이 Update를 호출하는 시간 간격을 모두 기록한다.
			//재생 시간과 별도로 "Repaint 하지 않아도 되는 불필요한 시간"을 체크하기 위함
			//float frameTime = (float)DateTime.Now.Subtract(_prevUpdateFrameDateTime).TotalSeconds;
			//_tRepaintTimer += frameTime;



			//if(_tRepaintTimer > REPAINT_MIN_TIME)
			//{
			//	_isRepaintTimerUsable = true;
			//	_tRepaintTimer = 0.0f;
			//}

			//Update 타입의 타이머를 작동한다.
			if (UpdateFrameCount(FRAME_TIMER_TYPE.Update))
			{
				//동기화가 되었다.
				//Update에 의한 Repaint가 유효하다.
				_isRepaintTimerUsable = true;
			}

			//Debug.Log("Update [" + _isRepaintTimerUsable + "]");

			_memGC += DeltaTime_UpdateAllFrame;
			if (_memGC > 30.0f)
			{
				//System.GC.AddMemoryPressure(1024 * 200);//200MB 정도 압박을 줘보자
				System.GC.Collect();

				_memGC = 0.0f;
			}

			if (_isRepaintable)
			{
				//바로 Repaint : 강제로 Repaint를 하는 경우
				_isRepaintable = false;
				//_prevDateTime = DateTime.Now;

				//강제로 호출한 건 Usable의 영향을 받지 않는다.
				_isValidGUIRepaint = true;
				Repaint();
				_isRepaintTimerUsable = false;
			}
			else
			{
				if (!_isUpdateSkip)
				{
					if (_isRepaintTimerUsable)
					{
						_isValidGUIRepaint = true;
						Repaint();
					}
					_isRepaintTimerUsable = false;
				}

				_isUpdateSkip = false;

				
			}

			//Notification이나 없애주자
			if (_isNotification)
			{
				_tNotification -= DeltaTime_UpdateAllFrame;
				if (_tNotification < 0.0f)
				{
					_isNotification = false;
					_tNotification = 0.0f;
					RemoveNotification();
				}
			}
			if (_isNotification_GUI)
			{
				_tNotification_GUI -= DeltaTime_UpdateAllFrame;
				if (_tNotification_GUI < 0.0f)
				{
					_isNotification_GUI = false;
					_tNotification_GUI = 0.0f;
				}
			}
			//백업 레이블 애니메이션 처리를 하자
			if (_isBackupProcessing != Backup.IsAutoSaveWorking())
			{
				_isBackupProcessing = Backup.IsAutoSaveWorking();
				_tBackupProcessing_Icon = 0.0f;
				_tBackupProcessing_Label = 0.0f;
			}
			if(_isBackupProcessing)
			{
				_tBackupProcessing_Icon += DeltaTime_UpdateAllFrame;
				_tBackupProcessing_Label += DeltaTime_UpdateAllFrame;

				if(_tBackupProcessing_Icon > BACKUP_ICON_TIME_LENGTH)
				{
					_tBackupProcessing_Icon -= BACKUP_ICON_TIME_LENGTH;
				}

				if(_tBackupProcessing_Label > BACKUP_LABEL_TIME_LENGTH)
				{
					_tBackupProcessing_Label -= BACKUP_LABEL_TIME_LENGTH;
				}
			}
			//_isCountDeltaTime = false;

			//_prevUpdateFrameDateTime = DateTime.Now;

			//타이머 종료
			//UpdateFrameCount(FRAME_TIMER_TYPE.None);


		}




		void LateUpdate()
		{



			if (EditorApplication.isPlaying)
			{
				return;
			}

			//Debug.Log("a");
			//Repaint();
		}


		private void CheckEditorResources()
		{
			if (_gizmos == null)
			{
				_gizmos = new apGizmos(this);
			}
			if (_selection == null)
			{
				_selection = new apSelection(this);
			}

			//apGUIPool.Init();

			//_hierarchy.ReloadImageResources();

			if (_imageSet == null)
			{
				_imageSet = new apImageSet();
			}

			bool isImageReload = _imageSet.ReloadImages();
			if (isImageReload)
			{
				apControllerGL.SetTexture(
					_imageSet.Get(apImageSet.PRESET.Controller_ScrollBtn),
					_imageSet.Get(apImageSet.PRESET.Controller_ScrollBtn_Recorded),
					_imageSet.Get(apImageSet.PRESET.Controller_SlotDeactive),
					_imageSet.Get(apImageSet.PRESET.Controller_SlotActive)
					);

				apTimelineGL.SetTexture(
					_imageSet.Get(apImageSet.PRESET.Anim_Keyframe),
					_imageSet.Get(apImageSet.PRESET.Anim_KeyframeDummy),
					_imageSet.Get(apImageSet.PRESET.Anim_KeySummary),
					_imageSet.Get(apImageSet.PRESET.Anim_KeySummaryMove),
					_imageSet.Get(apImageSet.PRESET.Anim_PlayBarHead),
					_imageSet.Get(apImageSet.PRESET.Anim_KeyLoopLeft),
					_imageSet.Get(apImageSet.PRESET.Anim_KeyLoopRight),
					_imageSet.Get(apImageSet.PRESET.Anim_TimelineBGStart),
					_imageSet.Get(apImageSet.PRESET.Anim_TimelineBGEnd),
					_imageSet.Get(apImageSet.PRESET.Anim_CurrentKeyframe),
					_imageSet.Get(apImageSet.PRESET.Anim_KeyframeCursor),
					_imageSet.Get(apImageSet.PRESET.Anim_EventMark),
					_imageSet.Get(apImageSet.PRESET.Anim_OnionMark)
					//_imageSet.Get(apImageSet.PRESET.Anim_KeyframeMoveSrc),
					//_imageSet.Get(apImageSet.PRESET.Anim_KeyframeMove),
					//_imageSet.Get(apImageSet.PRESET.Anim_KeyframeCopy)
					);

				apAnimCurveGL.SetTexture(
					_imageSet.Get(apImageSet.PRESET.Curve_ControlPoint)
					);

				apGL.SetTexture(_imageSet.Get(apImageSet.PRESET.Physic_VertMain),
					_imageSet.Get(apImageSet.PRESET.Physic_VertConst)
					);
			}



			if (_controller == null || _controller.Editor == null)
			{
				_controller = new apEditorController();
				_controller.SetEditor(this);
			}

			if (_gizmoController == null || _gizmoController.Editor == null)
			{
				_gizmoController = new apGizmoController();
				_gizmoController.SetEditor(this);
			}

			//수정 : 폴더 변경 ("Assets/Editor/AnyPortraitTool/" => apEditorUtil.ResourcePath_Material);

			bool isResetMat = false;
			
			//감마 / 선형 색상 공간에 따라 Shader를 다른 것을 사용해야한다.
			bool isGammaColorSpace = apEditorUtil.IsGammaColorSpace();
			

			if (_mat_Color == null)
			{
				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_Color = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_Color.mat");
				}
				else
				{
					_mat_Color = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_Color.mat");
				}
				
				isResetMat = true;
			}

			if(_mat_GUITexture == null)
			{
				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_GUITexture = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_GUITexture.mat");
				}
				else
				{
					_mat_GUITexture = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_GUITexture.mat");
				}
				isResetMat = true;
			}


			//AlphaBlend = 0,
			//Additive = 1,
			//SoftAdditive = 2,
			//Multiplicative = 3

			
			if (_mat_Texture_Normal == null || _mat_Texture_Normal.Length != 4)
			{
				_mat_Texture_Normal = new Material[4];

				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_Texture_Normal[0] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_Texture.mat");
					_mat_Texture_Normal[1] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_Texture Additive.mat");
					_mat_Texture_Normal[2] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_Texture SoftAdditive.mat");
					_mat_Texture_Normal[3] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_Texture Multiplicative.mat");
				}
				else
				{
					_mat_Texture_Normal[0] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_Texture.mat");
					_mat_Texture_Normal[1] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_Texture Additive.mat");
					_mat_Texture_Normal[2] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_Texture SoftAdditive.mat");
					_mat_Texture_Normal[3] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_Texture Multiplicative.mat");
				}

				isResetMat = true;
			}

			if (_mat_Texture_VertAdd == null || _mat_Texture_VertAdd.Length != 4)
			{
				_mat_Texture_VertAdd = new Material[4];

				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_Texture_VertAdd[0] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_Texture_VColorAdd.mat");
					_mat_Texture_VertAdd[1] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_Texture_VColorAdd Additive.mat");
					_mat_Texture_VertAdd[2] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_Texture_VColorAdd SoftAdditive.mat");
					_mat_Texture_VertAdd[3] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_Texture_VColorAdd Multiplicative.mat");
				}
				else
				{
					_mat_Texture_VertAdd[0] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_Texture_VColorAdd.mat");
					_mat_Texture_VertAdd[1] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_Texture_VColorAdd Additive.mat");
					_mat_Texture_VertAdd[2] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_Texture_VColorAdd SoftAdditive.mat");
					_mat_Texture_VertAdd[3] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_Texture_VColorAdd Multiplicative.mat");
				}
				isResetMat = true;
			}

			//if (_mat_MaskedTexture == null || _mat_MaskedTexture.Length != 4)
			//{
			//	_mat_MaskedTexture = new Material[4];
			//	_mat_MaskedTexture[0] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_MaskedTexture.mat");
			//	_mat_MaskedTexture[1] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_MaskedTexture Additive.mat");
			//	_mat_MaskedTexture[2] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_MaskedTexture SoftAdditive.mat");
			//	_mat_MaskedTexture[3] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_MaskedTexture Multiplicative.mat");
			//	isResetMat = true;
			//}

			if (_mat_Clipped == null || _mat_Clipped.Length != 4)
			{
				_mat_Clipped = new Material[4];

				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_Clipped[0] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_ClippedTexture.mat");
					_mat_Clipped[1] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_ClippedTexture Additive.mat");
					_mat_Clipped[2] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_ClippedTexture SoftAdditive.mat");
					_mat_Clipped[3] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_ClippedTexture Multiplicative.mat");
				}
				else
				{
					_mat_Clipped[0] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_ClippedTexture.mat");
					_mat_Clipped[1] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_ClippedTexture Additive.mat");
					_mat_Clipped[2] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_ClippedTexture SoftAdditive.mat");
					_mat_Clipped[3] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_ClippedTexture Multiplicative.mat");
				}

				isResetMat = true;
			}

			if (_mat_MaskOnly == null)
			{
				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_MaskOnly = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_MaskOnly.mat");
				}
				else
				{
					_mat_MaskOnly = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_MaskOnly.mat");
				}
				isResetMat = true;
			}


			if(_mat_ToneColor_Normal == null)
			{
				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_ToneColor_Normal = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_ToneColor_Texture.mat");
				}
				else
				{
					_mat_ToneColor_Normal = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_ToneColor_Texture.mat");
				}
			}

			if(_mat_ToneColor_Clipped == null)
			{
				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_ToneColor_Clipped = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_ToneColor_Clipped.mat");
				}
				else
				{
					_mat_ToneColor_Clipped = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_ToneColor_Clipped.mat");
				}
			}

			if(_mat_Alpha2White == null)
			{
				if(isGammaColorSpace)
				{
					_mat_Alpha2White = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "apMat_Alpha2White.mat");
				}
				else
				{
					_mat_Alpha2White = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/apMat_L_Alpha2White.mat");
				}
			}


			if (isResetMat)
			{
				Shader[] shaderSet_Normal = new Shader[4];
				Shader[] shaderSet_VertAdd = new Shader[4];
				//Shader[] shaderSet_Mask = new Shader[4];
				Shader[] shaderSet_Clipped = new Shader[4];
				for (int i = 0; i < 4; i++)
				{
					shaderSet_Normal[i] = _mat_Texture_Normal[i].shader;
					shaderSet_VertAdd[i] = _mat_Texture_VertAdd[i].shader;
					//shaderSet_Mask[i] = _mat_MaskedTexture[i].shader;
					shaderSet_Clipped[i] = _mat_Clipped[i].shader;
				}

				apGL.SetShader(_mat_Color.shader, shaderSet_Normal, shaderSet_VertAdd, /*shaderSet_Mask, */_mat_MaskOnly.shader, shaderSet_Clipped, _mat_GUITexture.shader, _mat_ToneColor_Normal.shader, _mat_ToneColor_Clipped.shader, _mat_Alpha2White.shader);

				apControllerGL.SetShader(_mat_Color.shader, shaderSet_Normal, shaderSet_VertAdd, /*shaderSet_Mask, */_mat_MaskOnly.shader, shaderSet_Clipped, _mat_GUITexture.shader, _mat_ToneColor_Normal.shader, _mat_ToneColor_Clipped.shader, _mat_Alpha2White.shader);

				apTimelineGL.SetShader(_mat_Color.shader, shaderSet_Normal, shaderSet_VertAdd, /*shaderSet_Mask, */_mat_MaskOnly.shader, shaderSet_Clipped, _mat_GUITexture.shader, _mat_ToneColor_Normal.shader, _mat_ToneColor_Clipped.shader, _mat_Alpha2White.shader);
				apAnimCurveGL.SetShader(_mat_Color.shader, shaderSet_Normal, shaderSet_VertAdd, /*shaderSet_Mask, */_mat_MaskOnly.shader, shaderSet_Clipped, _mat_GUITexture.shader, _mat_ToneColor_Normal.shader, _mat_ToneColor_Clipped.shader, _mat_Alpha2White.shader);
			}
			

			if (_localization == null)
			{
				_localization = new apLocalization();
			}
			if (!_localization.IsLoaded)
			{
				//경로 변경 : "Assets/Editor/AnyPortraitTool/Util/" => apEditorUtil.ResourcePath_Text
				TextAsset textAsset_Dialog = AssetDatabase.LoadAssetAtPath<TextAsset>(apEditorUtil.ResourcePath_Text + "apLangPack.txt");
				TextAsset textAsset_UI = AssetDatabase.LoadAssetAtPath<TextAsset>(apEditorUtil.ResourcePath_Text + "apLangPack_UI.txt");
				_localization.SetTextAsset(textAsset_Dialog, textAsset_UI);
			}


			if (_hierarchy == null)
			{
				_hierarchy = new apEditorHierarchy(this);
				_hierarchy.ResetAllUnits();
			}

			if (_hierarchy_MeshGroup == null)
			{
				_hierarchy_MeshGroup = new apEditorMeshGroupHierarchy(this);
			}

			if (_hierarchy_AnimClip == null)
			{
				_hierarchy_AnimClip = new apEditorAnimClipTargetHierarchy(this);
			}

			_gizmos.LoadResources();


			

			
		}




		// GUI 공통 입력 부분
		//--------------------------------------------------------------

		//--------------------------------------------------------------------------------------------
		void OnGUI()
		{

			if (Application.isPlaying)
			{
				int windowWidth = (int)position.width;
				int windowHeight = (int)position.height;

				EditorGUILayout.BeginVertical(GUILayout.Width((int)position.width), GUILayout.Height((int)position.height));

				GUILayout.Space((windowHeight / 2) - 10);
				EditorGUILayout.BeginHorizontal();
				GUIStyle guiStyle_CenterLabel = new GUIStyle(GUI.skin.label);
				guiStyle_CenterLabel.alignment = TextAnchor.MiddleCenter;

				EditorGUILayout.LabelField("Unity Editor is Playing.", guiStyle_CenterLabel, GUILayout.Width((int)position.width));

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.EndVertical();

				_isUpdateAfterEditorRunning = true;
				return;
			}

			//int nEvent = Event.GetEventCount();
			//List<EventType> guiEvents = new List<EventType>();
			//guiEvents.Add(Event.current.type);
			//Event nextEvent = new Event();
			//for (int i = 0; i < nEvent; i++)
			//{
			//	if(!Event.PopEvent(nextEvent))
			//	{
			//		guiEvents.Add(nextEvent.type);
			//		break;
			//	}
			//}
			//string strEvent = "Events[" + nEvent+ "] : ";
			//for (int i = 0; i < guiEvents.Count; i++)
			//{
			//	strEvent += guiEvents[i] + " / ";
			//}
			//Debug.Log(strEvent);

			//Undo 체크를 할 수 있도록 만들자
			//apEditorUtil.EnableUndoFrameCheck();

			if (_isUpdateAfterEditorRunning)
			{
				Init();
				_isUpdateAfterEditorRunning = false;
			}

			_curEventType = Event.current.type;

			if (Event.current.type == EventType.Repaint)
			{
				//_isCountDeltaTime = true;
				//GUI Repaint 타입의 타이머를 작동한다.
				UpdateFrameCount(FRAME_TIMER_TYPE.Repaint);
			}
			else
			{
				//_isCountDeltaTime = false;
				//이 호출에서는 타이머 종료
				//UpdateFrameCount(FRAME_TIMER_TYPE.None);
			}



			//언어팩 옵션 적용
			_localization.SetLanguage(_language);


			if (_portrait != null)
			{
				//포커스가 EditorWindow에 없다면 물리 사용을 해제한다.
				_portrait._isPhysicsSupport_Editor = (EditorWindow.focusedWindow == this);
			}





			////return;
			//Profiler.BeginSample("Editor Main GUI [" + Event.current.type + "]");

			try
			{

				CheckEditorResources();
				Controller.CheckInputEvent();

				HotKey.Clear();


				if(_isFirstOnGUI)
				{
					
					if(apVersion.I.IsDemo)
					{
						//데모인 경우 : 항상 나옴
						apDialog_StartPage.ShowDialog(this, ImageSet.Get(apImageSet.PRESET.StartPageLogo_Demo), false);
					}
					else
					{
						if(_startScreenOption_IsShowStartup)
						{
							//데모가 아닌 경우 : 옵션에 따라 완전 처음 또는 매일 한번 나온다.
							//옵션 True + 날짜가 달라야 나온다.
							if (DateTime.Now.Month != _startScreenOption_LastMonth ||
								DateTime.Now.Day != _startScreenOption_LastDay)
							{
								apDialog_StartPage.ShowDialog(this, ImageSet.Get(apImageSet.PRESET.StartPageLogo_Full), true);

								_startScreenOption_LastMonth = DateTime.Now.Month;
								_startScreenOption_LastDay = DateTime.Now.Day;

								SaveEditorPref();
							}
						}
					}

					//업데이트 로그도 띄우자
					if (_updateLogScreen_LastVersion != apVersion.I.APP_VERSION_INT)
					{
						apDialog_UpdateLog.ShowDialog(this);

						_updateLogScreen_LastVersion = apVersion.I.APP_VERSION_INT;

						SaveEditorPref();
					}

					_isFirstOnGUI = false;
				}

				//if (_mouseBtn[MOUSE_BTN_LEFT].Status == apMouse.MouseBtnStatus.Released &&
				//	_mouseBtn[MOUSE_BTN_MIDDLE].Status == apMouse.MouseBtnStatus.Released &&
				//	_mouseBtn[MOUSE_BTN_RIGHT].Status == apMouse.MouseBtnStatus.Released)
				//{
				//	_isMouseEvent = false;
				//}
				//else
				//{
				//	_isMouseEvent = true;
				//}

				//if(Event.current.isKey || Event.current.isMouse)
				//{ Repaint(); }




				//현재 GUI 이벤트가 Layout/Repaint 이벤트인지
				//또는 마우스/키보드 등의 다른 이벤트인지 판별
				if (Event.current.type != EventType.Layout
					&& Event.current.type != EventType.Repaint)
				{
					_isGUIEvent = false;
				}
				else
				{
					_isGUIEvent = true;
				}


				//자동 저장 기능
				Backup.CheckAutoBackup(this, Event.current.type);


				int windowWidth = (int)position.width;
				int windowHeight = (int)position.height;


				//int topHeight = 65;
				bool isTopVisible = true;
				bool isLeftVisible = true;
				bool isRightVisible = true;

				int topHeight = 45;

				//Bottom 레이아웃은 일부 기능에서만 나타난다.
				int bottomHeight = 0;
				bool isBottomVisible = false;

				//추가 Bottom위에 Edit 툴이 나오는 Bottom 2 Layout이 있다.
				int bottom2Height = 45;
				bool isBottom2Render = (Select.ExEditMode != apSelection.EX_EDIT_KEY_VALUE.None) && (_meshGroupEditMode == MESHGROUP_EDIT_MODE.Modifier);

				if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
				{
					//TODO : 애니메이션의 하단 레이아웃은 Summary / 4 Line / 7 Line으로 조절 가능하다
					switch (_timelineLayoutSize)
					{
						case TIMELINE_LAYOUTSIZE.Size1:
							//bottomHeight = 250;
							bottomHeight = 200;//<더축소
							break;

						case TIMELINE_LAYOUTSIZE.Size2:
							//bottomHeight = 375;
							bottomHeight = 340;
							break;

						case TIMELINE_LAYOUTSIZE.Size3:
							bottomHeight = 500;
							break;
					}
					isBottomVisible = true;
				}
				

				int marginHeight = 10;
				int mainHeight = windowHeight - (topHeight + marginHeight * 2 + bottomHeight);
				if (!isBottomVisible)
				{
					mainHeight = windowHeight - (topHeight + marginHeight);
				}
				if (isBottom2Render)
				{
					mainHeight -= (marginHeight + bottom2Height);
				}

				int topWidth = windowWidth;
				int bottomWidth = windowWidth;

				int mainLeftWidth = 250;
				int mainRightWidth = 250;
				int mainRight2Width = 250;
				int mainMarginWidth = 5;
				int mainCenterWidth = windowWidth - (mainLeftWidth + mainMarginWidth * 2 + mainRightWidth);

				if (_isFullScreenGUI)
				{
					//추가 : FullScreen이라면 -> Bottom 빼고 다 사라진다.
					//위, 양쪽의 길이는 물론이고, margin 계산도 하단 제외하고 다 빼준다.
					topHeight = 0;
					isTopVisible = false;
					isLeftVisible = false;
					isRightVisible = false;

					mainHeight = windowHeight - (marginHeight * 1 + bottomHeight);
					if (!isBottomVisible)
					{
						mainHeight = windowHeight;
					}
					if (isBottom2Render)
					{
						mainHeight -= (marginHeight + bottom2Height);
					}

					mainLeftWidth = 0;
					mainRightWidth = 0;
					mainRight2Width = 0;

					mainCenterWidth = windowWidth;
				}

				bool isRight2Visible = false;

				if (isRightVisible)
				{
					//Right 패널이 두개가 필요한 경우 (Right GUI가 보여진다는 가정하에)
					switch (Select.SelectionType)
					{
						case apSelection.SELECTION_TYPE.MeshGroup:
							isRight2Visible = true;
							break;

						case apSelection.SELECTION_TYPE.Animation:
							isRight2Visible = true;
							break;
					}
				}


				if (isRight2Visible)
				{
					mainCenterWidth = windowWidth - (mainLeftWidth + mainMarginWidth * 3 + mainRightWidth + mainRight2Width);
				}

				// Layout
				//-----------------------------------------
				// Top (Tab / Toolbar)
				//-----------------------------------------
				//		|						|
				//		|			Main		|
				// Func	|		GUI Editor		| Inspector
				//		|						|
				//		|						|
				//		|						|
				//		|						|
				//-----------------------------------------
				// Bottom (Timeline / Status) : 선택
				//-----------------------------------------

				//Portrait 생성 요청이 있다면 여기서 처리
				if (_isMakePortraitRequest && _curEventType == EventType.Layout)
				{
					MakeNewPortrait();
				}
				if(_isMakePortraitRequestFromBackupFile && _curEventType == EventType.Layout)
				{
					MakePortraitFromBackupFile();
				}

				Color guiBasicColor = GUI.backgroundColor;


				// Top UI : Tab Buttons / Basic Toolbar
				//-------------------------------------------------
				GUILayout.Space(5);

				if (isTopVisible)
				{
					GUI.Box(new Rect(0, 0, topWidth, topHeight), "");

					//Profiler.BeginSample("1. GUI Top");

					GUI_Top(windowWidth, topHeight);

					//Profiler.EndSample();
					//-------------------------------------------------

					GUILayout.Space(marginHeight);
				}

				//HotKey 처리를 두자
				GUI_HotKey_TopRight();//<<렌더여부 상관 없이..




				//Rect mainRect_LT = GUILayoutUtility.GetLastRect();

				//-------------------------------------------------
				// Left Func + Main GUI Editor + Right Inspector
				EditorGUILayout.BeginHorizontal(GUILayout.Height(mainHeight));

				if (isLeftVisible)
				{
					// Main Left Layout
					//------------------------------------
					EditorGUILayout.BeginVertical(GUILayout.Width(mainLeftWidth));

					Rect rectMainLeft = new Rect(0, topHeight + marginHeight, mainLeftWidth, mainHeight);
					GUI.Box(rectMainLeft, "");
					GUILayout.BeginArea(rectMainLeft, "");

					GUILayout.Space(10);
					EditorGUILayout.BeginHorizontal();
					if (apEditorUtil.ToggledButton(GetUIWord(UIWORD.Hierarchy), _tabLeft == TAB_LEFT.Hierarchy, (mainLeftWidth - 20) / 2, 20))//"Hierarchy"
					{
						_tabLeft = TAB_LEFT.Hierarchy;
					}
					if (apEditorUtil.ToggledButton(GetUIWord(UIWORD.Controller), _tabLeft == TAB_LEFT.Controller, (mainLeftWidth - 20) / 2, 20))//"Controller"
					{
						_tabLeft = TAB_LEFT.Controller;
					}
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(10);

					int leftUpperHeight = GUI_MainLeft_Upper(mainLeftWidth - 20);

					//int leftHeight = mainHeight - ((20 - 40) + leftUpperHeight);

					_scroll_MainLeft = EditorGUILayout.BeginScrollView(_scroll_MainLeft, false, true);

					EditorGUILayout.BeginVertical(GUILayout.Width(mainLeftWidth - 20), GUILayout.Height(mainHeight - ((20 - 40) + leftUpperHeight)));

					apControllerGL.SetLayoutSize(mainLeftWidth - 20, mainHeight - ((20 - 40) + leftUpperHeight + 60),
										(int)rectMainLeft.x, (int)rectMainLeft.y + leftUpperHeight + 38,
										(int)position.width, (int)position.height, _scroll_MainLeft);

					//ControllerGL의 Snap여부를 결정하자.
					//일반적으로는 True이지만, ControlParam을 제어하는 AnimTimeline 작업시에는 False가 된다.
					//bool isAnimControlParamEditing = false;

					if (_selection.SelectionType == apSelection.SELECTION_TYPE.Animation &&
							_selection.AnimClip != null &&
							_selection.ExAnimEditingMode != apSelection.EX_EDIT.None &&
							_selection.AnimTimeline != null &&
							_selection.AnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
					{
						apControllerGL.SetSnapWhenReleased(false);
					}
					else
					{
						apControllerGL.SetSnapWhenReleased(true);
					}

					//Profiler.BeginSample("2. GUI Left");

					GUI_MainLeft(mainLeftWidth - 20, mainHeight - 20, _scroll_MainLeft.x, _scroll_MainLeft.y, _isGUIEvent);

					//Profiler.EndSample();

					apControllerGL.EndUpdate();

					//switch (_tab)
					//{
					//	case TAB.ProjectSetting: GUI_MainLeft_ProjectSetting(mainLeftWidth - 20, mainHeight - 20); break;
					//	case TAB.MeshEdit: GUI_MainLeft_MeshEdit(mainLeftWidth - 20, mainHeight - 20); break;
					//	case TAB.FaceEdit: GUI_MainLeft_FaceEdit(mainLeftWidth - 20, mainHeight - 20); break;
					//	case TAB.RigEdit: GUI_MainLeft_RigEdit(mainLeftWidth - 20, mainHeight - 20); break;
					//	case TAB.Animation: GUI_MainLeft_Animation(mainLeftWidth - 20, mainHeight - 20); break;
					//}
					EditorGUILayout.EndVertical();

					GUILayout.Space(50);

					EditorGUILayout.EndScrollView();

					GUILayout.EndArea();

					EditorGUILayout.EndVertical();


					//------------------------------------


					GUILayout.Space(mainMarginWidth);
				}

				//Profiler.BeginSample("3. GUI Center");

				// Main Center Layout
				//------------------------------------
				//Rect mainRect_LB = GUILayoutUtility.GetLastRect();
				EditorGUILayout.BeginVertical();

				int guiViewBtnSize = 15;

				GUI.backgroundColor = _colorOption_Background;
				Rect rectMainCenter = new Rect(mainLeftWidth + mainMarginWidth, topHeight + marginHeight, mainCenterWidth, mainHeight);
				if(_isFullScreenGUI)
				{
					//전체 화면일때 화면 구성이 바뀐다.
					rectMainCenter = new Rect(0, 0, mainCenterWidth, mainHeight);
				}



				GUI.Box(new Rect(rectMainCenter.x, rectMainCenter.y, rectMainCenter.width - guiViewBtnSize, rectMainCenter.height - guiViewBtnSize), "", apEditorUtil.WhiteGUIStyle_Box);
				GUILayout.BeginArea(rectMainCenter, "");

				GUI.backgroundColor = guiBasicColor;
				//_scroll_MainCenter = EditorGUILayout.BeginScrollView(_scroll_MainCenter, true, true);

				//_scroll_MainCenter.y = GUI.VerticalScrollbar(new Rect(rectMainCenter.x + mainCenterWidth - 20, rectMainCenter.width, 20, rectMainCenter.height), _scroll_MainCenter.y, 1.0f, -20.0f, 20.0f);
				
				_scroll_MainCenter.y = GUI.VerticalScrollbar(new Rect(rectMainCenter.width - 15, 0, 20, rectMainCenter.height - guiViewBtnSize), _scroll_MainCenter.y, 5.0f, -500.0f, 500.0f + 5.0f);
				_scroll_MainCenter.x = GUI.HorizontalScrollbar(new Rect(guiViewBtnSize + 110, rectMainCenter.height - 15, rectMainCenter.width - (guiViewBtnSize + guiViewBtnSize + 110), 20), _scroll_MainCenter.x, 5.0f, -500.0f, 500.0f + 5.0f);

				GUIStyle guiStyle_GUIViewBtn = new GUIStyle(GUI.skin.button);
				guiStyle_GUIViewBtn.padding = GUI.skin.label.padding;

				//화면 중심으로 이동하는 버튼
				if (GUI.Button(new Rect(rectMainCenter.width - guiViewBtnSize, rectMainCenter.height - guiViewBtnSize, guiViewBtnSize, guiViewBtnSize), 
								new GUIContent(_imageSet.Get(apImageSet.PRESET.GUI_Center), "Reset Zoom and Position"), 
								guiStyle_GUIViewBtn))
				{
					_scroll_MainCenter = Vector2.zero;
					_iZoomX100 = ZOOM_INDEX_DEFAULT;
				}

				//전체 화면 기능 추가
				Color prevColor = GUI.backgroundColor;

				if(_isFullScreenGUI)
				{
					GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
				}


				//Alt+W를 눌러서 크기를 바꿀 수 있다.
				AddHotKeyEvent(OnHotKeyEvent_FullScreenToggle, "Toggle Workspace Size", KeyCode.W, false, true, false, null);

				if (GUI.Button(new Rect(0, rectMainCenter.height - guiViewBtnSize, guiViewBtnSize, guiViewBtnSize), 
					new GUIContent(_imageSet.Get(apImageSet.PRESET.GUI_FullScreen), "Toogle Workspace Size (Alt+W)"), 
					guiStyle_GUIViewBtn))
				{
					_isFullScreenGUI = !_isFullScreenGUI;
				}
				GUI.backgroundColor = prevColor;

				//추가 : GUI_Top에 있었던 Zoom이 여기로 왔다.
				int minZoom = 0;
				int maxZoom = _zoomListX100.Length - 1;
				
				//float fZoom = GUILayout.HorizontalSlider(_iZoomX100, minZoom, maxZoom + 0.5f, GUILayout.Width(60));
				float fZoom = GUI.HorizontalSlider(new Rect(guiViewBtnSize + 5, rectMainCenter.height - (guiViewBtnSize + 1), 40, guiViewBtnSize), _iZoomX100, minZoom, maxZoom + 0.5f);
				_iZoomX100 = Mathf.Clamp((int)fZoom, 0, _zoomListX100.Length - 1);

				GUI.Label(new Rect(guiViewBtnSize + 50, rectMainCenter.height - guiViewBtnSize, 60, guiViewBtnSize), _zoomListX100[_iZoomX100] + "%");

				//줌 관련 처리를 해보자
				//bool isZoomControlling = Controller.GUI_Input_ZoomAndScroll();
				Controller.GUI_Input_ZoomAndScroll();


				EditorGUILayout.BeginVertical(GUILayout.Width(mainCenterWidth - 20), GUILayout.Height(mainHeight - 20));

				_mainGUIRect = new Rect(rectMainCenter.x, rectMainCenter.y, rectMainCenter.width - 20, rectMainCenter.height - 20);

				//추가 4.10. 만약, Capture도중이면 화면 이동/줌이 강제다
				if(_isScreenCaptureRequest
#if UNITY_EDITOR_OSX
					|| _isScreenCaptureRequest_OSXReady
#endif
					)
				{
					if(_screenCaptureRequest != null)
					{
						_scroll_MainCenter= _screenCaptureRequest._screenPosition;
						_iZoomX100 = _screenCaptureRequest._screenZoomIndex;
					}
				}
			
				apGL.SetWindowSize(mainCenterWidth, mainHeight,
									_scroll_MainCenter,
									(float)(_zoomListX100[_iZoomX100]) * 0.01f,
									(int)rectMainCenter.x, (int)rectMainCenter.y,
									(int)position.width, (int)position.height);


				//CheckGizmoAvailable();
				if (Event.current.type == EventType.Repaint || Event.current.isMouse || Event.current.isKey)
				{
					_gizmos.ReadyToUpdate();
				}

				GUI_MainCenter(mainCenterWidth - 20, mainHeight - 20);

				if (Event.current.type == EventType.Repaint ||
					Event.current.isMouse ||
					Event.current.isKey ||
					_gizmos.IsDrawFlag)
				{
					_gizmos.EndUpdate();

					_gizmos.GUI_Render_Controller(this);
				}
				if (Event.current.type == EventType.Repaint)
				{
					_gizmos.CheckUpdate(DeltaTime_Repaint);
				}
				else
				{
					_gizmos.CheckUpdate(0.0f);
				}




				EditorGUILayout.EndVertical();
				//EditorGUILayout.EndScrollView();


				GUILayout.EndArea();

				EditorGUILayout.EndVertical();

				//------------------------------------

				//Profiler.EndSample();


				if (isRightVisible)
				{
					GUILayout.Space(mainMarginWidth);


					// Main Right Layout
					//------------------------------------

					//Profiler.BeginSample("4. GUI Right");

					EditorGUILayout.BeginVertical(GUILayout.Width(mainRightWidth));

					GUI.backgroundColor = guiBasicColor;

					bool isRightUpper_Scroll = true;

					bool isRightLower = false;
					bool isRightLower_Scroll = false;
					int rightLowerScrollType = -1;//0 : MeshGroup - Mesh, 1 : MeshGroup - Bone, 2 : Animation
					bool isRightLower_2LineHeader = false;
					int rightUpperHeight = mainHeight;
					int rightLowerHeight = 0;
					//Mesh Group인 경우 우측의 레이아웃이 2개가 된다.
					if (Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup
						|| Select.SelectionType == apSelection.SELECTION_TYPE.Animation)//<<추가 : 애니메이션 타입에서도 하단 Hierarchy가 필요하다
					{
						isRightLower = true;
						if (_rightLowerLayout == RIGHT_LOWER_LAYOUT.Hide)
						{
							//하단이 Hide일때
							if (_right_UpperLayout == RIGHT_UPPER_LAYOUT.Show)
							{
								//상단이 Show일때
								rightUpperHeight = mainHeight - (36 + marginHeight);
							}
							else
							{
								//상단이 Hide일때
								rightUpperHeight = 36;
								isRightUpper_Scroll = false;
							}


							rightLowerHeight = 36;
							isRightLower_Scroll = false;
							isRightLower_2LineHeader = false;
						}
						else
						{
							//하단이 Show일때
							if (_right_UpperLayout == RIGHT_UPPER_LAYOUT.Show)
							{
								//상단이 Show일때
								rightUpperHeight = mainHeight / 2;
							}
							else
							{
								//상단이 Hide일때
								rightUpperHeight = 36;
								isRightUpper_Scroll = false;
							}

							rightLowerHeight = mainHeight - (rightUpperHeight + marginHeight);
							isRightLower_Scroll = true;
							rightLowerScrollType = 2;//<<Animation

							if (Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
							{
								isRightLower_2LineHeader = true;

								if (Select._meshGroupChildHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
								{
									rightLowerScrollType = 0;
								}
								else
								{
									rightLowerScrollType = 1;
								}
							}
							else
							{
								isRightLower_2LineHeader = false;
							}
						}
					}

					Rect recMainRight = new Rect(mainLeftWidth + mainMarginWidth * 2 + mainCenterWidth, topHeight + marginHeight, mainRightWidth, rightUpperHeight);
					GUI.Box(recMainRight, "");
					GUILayout.BeginArea(recMainRight, "");

					EditorGUILayout.BeginVertical(GUILayout.Width(mainRightWidth - 20), GUILayout.Height(30));
					GUILayout.Space(6);
					GUI_MainRight_Header(mainRightWidth - 8, 25);
					EditorGUILayout.EndVertical();

					if (isRightUpper_Scroll)
					{
						_scroll_MainRight = EditorGUILayout.BeginScrollView(_scroll_MainRight, false, true);

						EditorGUILayout.BeginVertical(GUILayout.Width(mainRightWidth - 20), GUILayout.Height(rightUpperHeight - (20 + 30)));

						GUI_MainRight(mainRightWidth - 24, rightUpperHeight - (20 + 30));
#region [미사용 코드]
						//switch (_tab)
						//{
						//	case TAB.ProjectSetting: GUI_MainRight_ProjectSetting(mainRightWidth - 24, mainHeight - 20); break;
						//	case TAB.MeshEdit: GUI_MainRight_MeshEdit(mainRightWidth - 24, mainHeight - 20); break;
						//	case TAB.FaceEdit: GUI_MainRight_FaceEdit(mainRightWidth - 24, mainHeight - 20); break;
						//	case TAB.RigEdit: GUI_MainRight_RigEdit(mainRightWidth - 24, mainHeight - 20); break;
						//	case TAB.Animation: GUI_MainRight_Animation(mainRightWidth - 24, mainHeight - 20); break;
						//} 
#endregion
						EditorGUILayout.EndVertical();

						GUILayout.Space(500);

						EditorGUILayout.EndScrollView();
					}
					GUILayout.EndArea();


					if (isRightLower)
					{
						GUILayout.Space(marginHeight);

						Rect recMainRight_Lower = new Rect(mainLeftWidth + mainMarginWidth * 2 + mainCenterWidth,
															topHeight + marginHeight + rightUpperHeight + marginHeight,
															mainRightWidth,
															rightLowerHeight);

						GUI.Box(recMainRight_Lower, "");
						GUILayout.BeginArea(recMainRight_Lower, "");

						int rightLowerHeaderHeight = 30;
						if (isRightLower_2LineHeader)
						{
							rightLowerHeaderHeight = 65;
						}
						EditorGUILayout.BeginVertical(GUILayout.Width(mainRightWidth - 20), GUILayout.Height(rightLowerHeaderHeight));
						GUILayout.Space(6);
						if (Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
						{
							GUI_MainRight_Lower_MeshGroupHeader(mainRightWidth - 8, rightLowerHeaderHeight - 6, isRightLower_2LineHeader);
						}
						else if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
						{
							GUI_MainRight_Lower_AnimationHeader(mainRightWidth - 8, rightLowerHeaderHeight - 6);
						}
						EditorGUILayout.EndVertical();

						if (isRightLower_Scroll)
						{
							Vector2 lowerScrollValue = _scroll_MainRight_Lower_MG_Mesh;
							if (rightLowerScrollType == 0)
							{
								_scroll_MainRight_Lower_MG_Mesh = EditorGUILayout.BeginScrollView(_scroll_MainRight_Lower_MG_Mesh, false, true);
								lowerScrollValue = _scroll_MainRight_Lower_MG_Mesh;
							}
							else if (rightLowerScrollType == 1)
							{
								_scroll_MainRight_Lower_MG_Bone = EditorGUILayout.BeginScrollView(_scroll_MainRight_Lower_MG_Bone, false, true);
								lowerScrollValue = _scroll_MainRight_Lower_MG_Bone;
							}
							else
							{
								_scroll_MainRight_Lower_Anim = EditorGUILayout.BeginScrollView(_scroll_MainRight_Lower_Anim, false, true);
								lowerScrollValue = _scroll_MainRight_Lower_Anim;
							}


							EditorGUILayout.BeginVertical(GUILayout.Width(mainRightWidth - 20), GUILayout.Height(rightLowerHeight - 20));

							GUI_MainRight_Lower(mainRightWidth - 24, rightLowerHeight - 20, lowerScrollValue.x, _isGUIEvent);

							EditorGUILayout.EndVertical();

							GUILayout.Space(500);

							EditorGUILayout.EndScrollView();
						}
						GUILayout.EndArea();
					}


					EditorGUILayout.EndVertical();



					//------------------------------------
					if (isRight2Visible)
					{ SetGUIVisible("Right2GUI", true); }
					else
					{ SetGUIVisible("Right2GUI", false); }

					if (IsDelayedGUIVisible("Right2GUI"))
					{
						GUILayout.Space(mainMarginWidth);

						// Main Right Layout
						//------------------------------------
						EditorGUILayout.BeginVertical(GUILayout.Width(mainRight2Width));

						GUI.backgroundColor = guiBasicColor;

						Rect recMainRight2 = new Rect(mainLeftWidth + mainMarginWidth * 3 + mainCenterWidth + mainRightWidth, topHeight + marginHeight, mainRight2Width, mainHeight);
						GUI.Box(recMainRight2, "");
						GUILayout.BeginArea(recMainRight2, "");



						_scroll_MainRight2 = EditorGUILayout.BeginScrollView(_scroll_MainRight2, false, true);

						EditorGUILayout.BeginVertical(GUILayout.Width(mainRight2Width - 20), GUILayout.Height(mainHeight - 20));

						GUI_MainRight2(mainRight2Width - 24, mainHeight - 20);

						EditorGUILayout.EndVertical();

						GUILayout.Space(500);

						EditorGUILayout.EndScrollView();

						GUILayout.EndArea();


						EditorGUILayout.EndVertical();
					}

					//Profiler.EndSample();//Profiler GUI Right
				}

				EditorGUILayout.EndHorizontal();

				//Profiler.BeginSample("5. GUI Bottom");
				//-------------------------------------------------
				if (isBottom2Render)
				{
					GUILayout.Space(marginHeight);

					EditorGUILayout.BeginVertical(GUILayout.Height(bottom2Height));

					Rect rectBottom2 = new Rect(0, topHeight + marginHeight * 2 + mainHeight, bottomWidth, bottom2Height);
					if(_isFullScreenGUI)
					{
						//전체 화면인 경우 Top을 제외
						rectBottom2 = new Rect(0, marginHeight * 1 + mainHeight, bottomWidth, bottom2Height);
					}

					//Color prevGUIColor = GUI.backgroundColor;
					//if(Select.IsExEditing)
					//{
					//	GUI.backgroundColor = new Color(prevGUIColor.r * 1.5f, prevGUIColor.g * 0.5f, prevGUIColor.b * 0.5f, 1.0f);
					//}

					GUI.Box(rectBottom2, "");

					//GUI.backgroundColor = prevGUIColor;
					GUILayout.BeginArea(rectBottom2, "");

					GUILayout.Space(4);
					EditorGUILayout.BeginHorizontal(GUILayout.Width(bottomWidth), GUILayout.Height(bottom2Height - 10));

					GUI_Bottom2(bottomWidth, bottom2Height);

					EditorGUILayout.EndHorizontal();

					GUILayout.EndArea();

					EditorGUILayout.EndVertical();
				}

				if (isBottomVisible)
				{

					bool isBottomScroll = true;
					switch (Select.SelectionType)
					{
						case apSelection.SELECTION_TYPE.MeshGroup:
							isBottomScroll = false;
							break;

						case apSelection.SELECTION_TYPE.Animation:
							isBottomScroll = false;//<<스크롤은 따로 합니다.
							break;

						default:
							break;
					}

					GUILayout.Space(marginHeight);


					// Bottom Layout (Timeline / Status)
					//-------------------------------------------------
					EditorGUILayout.BeginVertical(GUILayout.Height(bottomHeight));

					int bottomPosY = topHeight + marginHeight * 2 + mainHeight;
					if(_isFullScreenGUI)
					{
						//전체화면인 경우 Top을 제외
						bottomPosY = marginHeight * 1 + mainHeight;
					}
					if (isBottom2Render)
					{
						bottomPosY += marginHeight + bottom2Height;
					}

					Rect rectBottom = new Rect(0, bottomPosY, bottomWidth, bottomHeight);



					GUI.Box(rectBottom, "");
					GUILayout.BeginArea(rectBottom, "");



					if (isBottomScroll)
					{
						_scroll_Bottom = EditorGUILayout.BeginScrollView(_scroll_Bottom, false, true);

						EditorGUILayout.BeginVertical(GUILayout.Width(bottomWidth - 20), GUILayout.Height(bottomHeight - 20));

						GUI_Bottom(bottomWidth - 20, bottomHeight - 20, (int)rectBottom.x, (int)rectBottom.y, (int)position.width, (int)position.height);

						EditorGUILayout.EndVertical();
						EditorGUILayout.EndScrollView();
					}
					else
					{


						EditorGUILayout.BeginVertical(GUILayout.Width(bottomWidth), GUILayout.Height(bottomHeight));

						GUI_Bottom(bottomWidth, bottomHeight, (int)rectBottom.x, (int)rectBottom.y, (int)position.width, (int)position.height);

						EditorGUILayout.EndVertical();
					}
					GUILayout.EndArea();

					EditorGUILayout.EndVertical();
				}

				//-------------------------------------------------
				//Profiler.EndSample();

				//Event e = Event.current;

				if (EditorWindow.focusedWindow == this)
				{
					//if (Event.current.type == EventType.ValidateCommand || Event.current.type == EventType.ExecuteCommand)
					//{
					//	Debug.Log("Command [" + Event.current.commandName + " / " + Event.current.keyCode + "]");
					//	//apEditorUtil.SetEditorDirty();
					//	//Undo.RecordObject(_portrait, "Dummy Record");
					//	//Undo.FlushUndoRecordObjects();
					//	Undo.
					//	Event.current.Use();

					//}
					if (Event.current.type == EventType.KeyUp)
					{
						if (GUIUtility.keyboardControl == 0)
						{
							//선택중인 GUI가 없다면..


							KeyCode keyCode = Event.current.keyCode;

							//TODO : Mac에서는 Command를 사용한다.
							//Debug.LogError("TODO : Mac에서는 Ctrl대신 Command를 써야한다.");



#if UNITY_EDITOR_OSX
						bool isCtrl = Event.current.command;
#else
							bool isCtrl = Event.current.control;
#endif

							bool isShift = Event.current.shift;
							bool isAlt = Event.current.alt;
							bool isHotKeyAction = false;



							//Debug.Log("Hot Key : " + keyCode + " / Count : " + Event.GetEventCount());

							apHotKey.HotKeyEvent hotkeyEvent = HotKey.CheckHotKeyEvent(keyCode, isShift, isAlt, isCtrl);
							if (hotkeyEvent != null)
							{
								string strHotKeyEvent = "[ " + hotkeyEvent._label + " ] - ";

								if (hotkeyEvent._isShift)
								{ strHotKeyEvent += "Shift+"; }
								if (hotkeyEvent._isCtrl)
								{
#if UNITY_EDITOR_OSX
								strHotKeyEvent += "Command+";//<<Mac용
#else
									strHotKeyEvent += "Ctrl+";
#endif
								}
								if (hotkeyEvent._isAlt)
								{ strHotKeyEvent += "Alt+"; }

								strHotKeyEvent += hotkeyEvent._keyCode;
								Notification(strHotKeyEvent, true, false);
								isHotKeyAction = true;
							}
#region [미사용 코드 : 테스트용 단축키]
							//else if (keyCode == KeyCode.L && isShift)
							//{
							//	string result = apDebugLog.I.Save("Editor");
							//	isHotKeyAction = true;

							//	if (result == null)
							//	{
							//		Notification("로그 저장에 실패했습니다.", true, false);
							//	}
							//	else
							//	{
							//		Notification("로그 저장[" + result + "]", true, false);
							//	}
							//}
							//else if (keyCode == KeyCode.F)
							//{
							//	if (_portrait != null)
							//	{
							//		//_portrait.AddForce_Direction(new Vector2(1.0f, 1.0f), 1.0f, 2.0f, 2.0f, 1.0f)
							//		//	.SetPower(10000)
							//		//	.EmitLoop();

							//		_portrait.AddForce_Point(new Vector2(-20, -100), 1000)
							//			.SetPower(10000, 10000, 2.0f)
							//			.EmitLoop();

							//		isHotKeyAction = true;
							//		Notification("힘 추가 (-20, 0)", true, false);
							//	}
							//}
							//else if (keyCode == KeyCode.G)
							//{
							//	if (_portrait != null)
							//	{
							//		_portrait.ClearForce();

							//		isHotKeyAction = true;
							//		Notification("힘 제거", true, false);
							//	}
							//} 
#endregion

							if (isHotKeyAction)
							{
								//키 이벤트를 사용했다고 알려주자
								Event.current.Use();
								//Repaint();
							}

							Event.current.Use();
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (ex is UnityEngine.ExitGUIException)
				{
					//걍 리턴
					//return;
					//무시하자
				}
				else
				{
					Debug.LogError("Exception : " + ex);
				}

			}



			//Profiler.EndSample();

			//_isCountDeltaTime = false;
			//타이머 종료
			UpdateFrameCount(FRAME_TIMER_TYPE.None);

			//GUIPool에서 사용했던 리소스들 모두 정리
			//apGUIPool.Reset();

			//일부 Dialog는 OnGUI가 지나고 호출해야한다.
			if (_dialogShowCall != DIALOG_SHOW_CALL.None && _curEventType == EventType.Layout)
			{
				try
				{
					//Debug.Log("Show Dialog [" + _curEventType + "]");
					switch (_dialogShowCall)
					{
						case DIALOG_SHOW_CALL.Bake:
							apDialog_Bake.ShowDialog(this, _portrait);
							break;

						case DIALOG_SHOW_CALL.Setting:
							apDialog_PortraitSetting.ShowDialog(this, _portrait);
							break;

						case DIALOG_SHOW_CALL.Capture:
							{
								if(apVersion.I.IsDemo)
								{
									EditorUtility.DisplayDialog(
										GetText(TEXT.DemoLimitation_Title),
										GetText(TEXT.DemoLimitation_Body),
										GetText(TEXT.Okay));
								}
								else
								{
									apDialog_CaptureScreen.ShowDialog(this, _portrait);
								}
							}
							
							break;

					}

				}
				catch (Exception ex)
				{
					Debug.LogError("Dialog Call Exception : " + ex);
				}
				_dialogShowCall = DIALOG_SHOW_CALL.None;
			}

			//Portrait 생성 요청이 들어왔을 때 => Repaint 이벤트 외의 루틴에서 함수를 연산해준다.

		}

		//--------------------------------------------------------------

		private bool _isGizmoGUIVisible_VTF_FFD_Prev = false;
		// Top UI : Tab Buttons / Basic Toolbar
		private void GUI_Top(int width, int height)
		{

			EditorGUILayout.BeginHorizontal(GUILayout.Height(height - 10));

			GUILayout.Space(20);

			if (_portrait == null)
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(250));
				EditorGUILayout.LabelField(GetUIWord(UIWORD.SelectPortraitFromScene), GUILayout.Width(200));//"Select Portrait From Scene"
				apPortrait nextPortrait = EditorGUILayout.ObjectField(_portrait, typeof(apPortrait), true, GUILayout.Width(200)) as apPortrait;

				//바뀌었다.
				if (_portrait != nextPortrait && nextPortrait != null)
				{
					if (nextPortrait._isOptimizedPortrait)
					{
						//Optimized Portrait는 편집이 불가능하다
						EditorUtility.DisplayDialog(GetText(TEXT.OptPortrait_LoadError_Title),
														GetText(TEXT.OptPortrait_LoadError_Body),
														GetText(TEXT.Okay));

					}
					else
					{
						_portrait = nextPortrait;//선택!

						Controller.InitTmpValues();
						_selection.SetPortrait(_portrait);

						//Portrait의 레퍼런스들을 연결해주자
						Controller.PortraitReadyToEdit();

						_hierarchy.ResetAllUnits();
						_hierarchy_MeshGroup.ResetSubUnits();
						_hierarchy_AnimClip.ResetSubUnits();

						//시작은 RootUnit
						_selection.SetOverallDefault();

						OnAnyObjectAddedOrRemoved();
					}
				}
				EditorGUILayout.EndVertical();

				GUILayout.Space(20);

				EditorGUILayout.EndHorizontal();

				//변경 : TOP에서 Portrait를 생성하는 코드는 삭제. Left GUI에서 실행한다.
				//if(GUILayout.Button("Make New Portrait On Scene", GUILayout.Width(250), GUILayout.Height(height - 10)))
				//{
				//	GameObject newPortraitObj = new GameObject("New Portrait");
				//	newPortraitObj.transform.position = Vector3.zero;
				//	newPortraitObj.transform.rotation = Quaternion.identity;
				//	newPortraitObj.transform.localScale = Vector3.one;

				//	_portrait = newPortraitObj.AddComponent<apPortrait>();
				//	//_portrait.ReadyToEdit();

				//	Controller.PortraitReadyToEdit();
				//	//Selection.activeGameObject = newPortraitObj;
				//	Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져
				//}

				return;
			}
			else//if (_portrait != null)
			{

				EditorGUILayout.BeginVertical(GUILayout.Width(150));
				EditorGUILayout.LabelField(GetUIWord(UIWORD.Portrait), GUILayout.Width(150));//"Portrait"
				apPortrait nextPortrait = EditorGUILayout.ObjectField(_portrait, typeof(apPortrait), true, GUILayout.Width(150)) as apPortrait;

				if (_portrait != nextPortrait)
				{
					//바뀌었다.
					if (nextPortrait != null)
					{
						if (nextPortrait._isOptimizedPortrait)
						{
							//Optimized Portrait는 편집이 불가능하다
							EditorUtility.DisplayDialog(GetText(TEXT.OptPortrait_LoadError_Title),
															GetText(TEXT.OptPortrait_LoadError_Body),
															GetText(TEXT.Okay));
						}
						else
						{
							//NextPortrait를 선택
							_portrait = nextPortrait;
							Controller.InitTmpValues();
							_selection.SetPortrait(_portrait);

							//Portrait의 레퍼런스들을 연결해주자
							Controller.PortraitReadyToEdit();


							//Selection.activeGameObject = _portrait.gameObject;
							Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

							//시작은 RootUnit
							_selection.SetOverallDefault();

							OnAnyObjectAddedOrRemoved();
						}
					}
					else
					{
						Controller.InitTmpValues();
						_selection.SetNone();
						_portrait = null;
					}

					_hierarchy.ResetAllUnits();
					_hierarchy_MeshGroup.ResetSubUnits();
					_hierarchy_AnimClip.ResetSubUnits();
				}
				EditorGUILayout.EndVertical();

				if (GUILayout.Button(new GUIContent(ImageSet.Get(apImageSet.PRESET.ToolBtn_Setting), "Settings of the Portrait and Editor"), GUILayout.Width(height - 14), GUILayout.Height(height - 14)))
				{
					//apDialog_PortraitSetting.ShowDialog(this, _portrait);
					_dialogShowCall = DIALOG_SHOW_CALL.Setting;//<<Delay된 호출을 한다.
				}

				//"  Bake"
				if (GUILayout.Button(new GUIContent("  " + GetUIWord(UIWORD.Bake), ImageSet.Get(apImageSet.PRESET.ToolBtn_Bake), "Bake to Scene"), GUILayout.Width(90), GUILayout.Height(height - 14)))
				{
					//_portrait.Bake();
					//Controller.PortraitBake();
					//Notification("[" + _portrait.name + "]\nis Baked", true);

					_dialogShowCall = DIALOG_SHOW_CALL.Bake;
					//apDialog_Bake.ShowDialog(this, _portrait);//<<이건 Delay 해야한다.
				}
			}

			GUILayout.Space(20);
			//GUILayout.Box("", GUILayout.Width(3), GUILayout.Height(height - 20));
			apEditorUtil.GUI_DelimeterBoxV(height - 15);
			GUILayout.Space(20);

			int tabBtnHeight = height - (15);
			int tabBtnWidth = tabBtnHeight + 10;

			bool isGizmoUpdatable = Gizmos.IsUpdatable;
			//if (Gizmos.IsUpdatable)
			if (apEditorUtil.ToggledButton(ImageSet.Get(apImageSet.PRESET.ToolBtn_Select), (_gizmos.ControlType == apGizmos.CONTROL_TYPE.Select), isGizmoUpdatable, tabBtnWidth, tabBtnHeight, "Select Tool (Q)"))
			{
				_gizmos.SetControlType(apGizmos.CONTROL_TYPE.Select);
			}
			if (apEditorUtil.ToggledButton(ImageSet.Get(apImageSet.PRESET.ToolBtn_Move), (_gizmos.ControlType == apGizmos.CONTROL_TYPE.Move), isGizmoUpdatable, tabBtnWidth, tabBtnHeight, "Move Tool (W)"))
			{
				_gizmos.SetControlType(apGizmos.CONTROL_TYPE.Move);
			}
			if (apEditorUtil.ToggledButton(ImageSet.Get(apImageSet.PRESET.ToolBtn_Rotate), (_gizmos.ControlType == apGizmos.CONTROL_TYPE.Rotate), isGizmoUpdatable, tabBtnWidth, tabBtnHeight, "Rotate Tool (E)"))
			{
				_gizmos.SetControlType(apGizmos.CONTROL_TYPE.Rotate);
			}
			if (apEditorUtil.ToggledButton(ImageSet.Get(apImageSet.PRESET.ToolBtn_Scale), (_gizmos.ControlType == apGizmos.CONTROL_TYPE.Scale), isGizmoUpdatable, tabBtnWidth, tabBtnHeight, "Scale Tool (R)"))
			{
				_gizmos.SetControlType(apGizmos.CONTROL_TYPE.Scale);
			}



			GUILayout.Space(10);

			EditorGUILayout.BeginVertical(GUILayout.Width(90));

			EditorGUILayout.LabelField(GetUIWord(UIWORD.Coordinate), GUILayout.Width(80));//"Coordinate"
			apGizmos.COORDINATE_TYPE nextCoordinate = (apGizmos.COORDINATE_TYPE)EditorGUILayout.EnumPopup(Gizmos.Coordinate, GUILayout.Width(90));
			if (nextCoordinate != Gizmos.Coordinate)
			{
				Gizmos.SetCoordinateType(nextCoordinate);
			}
			EditorGUILayout.EndVertical();

			GUILayout.Space(10);



			// Onion 버튼.
			bool isOnionButtonAvailable = false;
			if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation ||
				Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				isOnionButtonAvailable = true;
			}
			if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.ToolBtn_OnionView),
												Onion.IsVisible, isOnionButtonAvailable, tabBtnWidth, tabBtnHeight,
												"Show/Hide Onion Skin (O)"))
			{
				Onion.SetVisible(!Onion.IsVisible);
			}




			//만약 Onion이 켜졌다면 => Record 버튼이 활성화된다.
			bool isOnionRecordable = isOnionButtonAvailable && Onion.IsVisible;
			SetGUIVisible("GUI Top Onion Visible", isOnionRecordable);
			if (IsDelayedGUIVisible("GUI Top Onion Visible"))
			{
				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.ToolBtn_OnionRecord),
												false, true, tabBtnWidth, tabBtnHeight, "Record Onion Skin"))
				{
					//현재 상태를 기록한다.
					Onion.Record(this);
				}

				GUILayout.Space(5);
			}



			// Bone Visible 여부 버튼
			Texture2D iconImg_boneGUI = null;
			if (_boneGUIRenderMode == BONE_RENDER_MODE.RenderOutline)
			{
				iconImg_boneGUI = ImageSet.Get(apImageSet.PRESET.ToolBtn_BoneVisibleOutlineOnly);
			}
			else
			{
				iconImg_boneGUI = ImageSet.Get(apImageSet.PRESET.ToolBtn_BoneVisible);
			}


			bool isBoneVisibleButtonAvailable = _selection.SelectionType == apSelection.SELECTION_TYPE.MeshGroup ||
				_selection.SelectionType == apSelection.SELECTION_TYPE.Animation ||
				_selection.SelectionType == apSelection.SELECTION_TYPE.Overall;

			string strCtrlKey = "Ctrl";

#if UNITY_EDITOR_OSX
			strCtrlKey = "Command";
#endif
			if (apEditorUtil.ToggledButton_2Side(iconImg_boneGUI,
				_boneGUIRenderMode != BONE_RENDER_MODE.None,
				isBoneVisibleButtonAvailable, tabBtnWidth, tabBtnHeight,
				"Change Bone Visiblity (B) / If you press the button while holding down [" + strCtrlKey + "], the function works in reverse."))
			{
#if UNITY_EDITOR_OSX
				bool isCtrl = Event.current.command;
#else
				bool isCtrl = Event.current.control;
#endif

				//Control 키를 누르면 그냥 누른 것과 반대로 변경된다.
				//Ctrl : Outline -> Render -> None -> Outline
				//그냥 : None -> Render -> Outline -> None


				switch (_boneGUIRenderMode)
				{
					case BONE_RENDER_MODE.None:
						if (isCtrl)
						{
							_boneGUIRenderMode = BONE_RENDER_MODE.RenderOutline;
						}
						else
						{
							_boneGUIRenderMode = BONE_RENDER_MODE.Render;
						}

						break;

					case BONE_RENDER_MODE.Render:
						if (isCtrl)
						{
							_boneGUIRenderMode = BONE_RENDER_MODE.None;
						}
						else
						{
							_boneGUIRenderMode = BONE_RENDER_MODE.RenderOutline;
						}

						break;

					case BONE_RENDER_MODE.RenderOutline:
						if (isCtrl)
						{
							_boneGUIRenderMode = BONE_RENDER_MODE.Render;
						}
						else
						{
							_boneGUIRenderMode = BONE_RENDER_MODE.None;
						}

						break;

				}
				SaveEditorPref();
			}




			bool isPhysic = false;



			if (_portrait != null)
			{
				isPhysic = _portrait._isPhysicsPlay_Editor;
			}
			if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.ToolBtn_Physic),
				isPhysic,
				_selection.SelectionType == apSelection.SELECTION_TYPE.MeshGroup ||
				_selection.SelectionType == apSelection.SELECTION_TYPE.Animation ||
				_selection.SelectionType == apSelection.SELECTION_TYPE.Overall, tabBtnWidth, tabBtnHeight,
				"Enable/Disable Physical Effect"))
			{
				if (_portrait != null)
				{
					//물리 기능 토글
					_portrait._isPhysicsPlay_Editor = !isPhysic;
					if (_portrait._isPhysicsPlay_Editor)
					{
						//물리 값을 리셋합시다.
						Controller.ResetPhysicsValues();
					}
				}
			}




			GUILayout.Space(20);

			//Gizmo에 의해 어떤 UI가 나와야 하는지 판단하자.
			apGizmos.TRANSFORM_UI_VALID gizmoUI_VetexTF = Gizmos.TransformUI_VertexTransform;
			apGizmos.TRANSFORM_UI_VALID gizmoUI_Position = Gizmos.TransformUI_Position;
			apGizmos.TRANSFORM_UI_VALID gizmoUI_Rotation = Gizmos.TransformUI_Rotation;
			apGizmos.TRANSFORM_UI_VALID gizmoUI_Scale = Gizmos.TransformUI_Scale;
			apGizmos.TRANSFORM_UI_VALID gizmoUI_Color = Gizmos.TransformUI_Color;


			bool isGizmoGUIVisible_VertexTransform = false;

			bool isGizmoGUIVisible_VTF_FFD = false;
			bool isGizmoGUIVisible_VTF_Soft = false;
			bool isGizmoGUIVisible_VTF_Blur = false;

			bool isGizmoGUIVisible_Position = false;
			bool isGizmoGUIVisible_Rotation = false;
			bool isGizmoGUIVisible_Scale = false;
			bool isGizmoGUIVisible_Color = false;

			//Vertex Transform
			//1) FFD
			//2) Soft Selection
			//3) Blur가 있다.

			SetGUIVisible("Top UI - Vertex Transform", (gizmoUI_VetexTF != apGizmos.TRANSFORM_UI_VALID.Hide));

			SetGUIVisible("Top UI - Position", (gizmoUI_Position != apGizmos.TRANSFORM_UI_VALID.Hide));
			SetGUIVisible("Top UI - Rotation", (gizmoUI_Rotation != apGizmos.TRANSFORM_UI_VALID.Hide));
			SetGUIVisible("Top UI - Scale", (gizmoUI_Scale != apGizmos.TRANSFORM_UI_VALID.Hide));
			SetGUIVisible("Top UI - Color", (gizmoUI_Color != apGizmos.TRANSFORM_UI_VALID.Hide));

			SetGUIVisible("Top UI - VTF FFD", (gizmoUI_VetexTF != apGizmos.TRANSFORM_UI_VALID.Hide) && Gizmos.IsFFDMode);
			SetGUIVisible("Top UI - VTF Soft", (gizmoUI_VetexTF != apGizmos.TRANSFORM_UI_VALID.Hide) && Gizmos.IsSoftSelectionMode);
			SetGUIVisible("Top UI - VTF Blur", (gizmoUI_VetexTF != apGizmos.TRANSFORM_UI_VALID.Hide) && Gizmos.IsBlurMode);

			SetGUIVisible("Top UI - Overall", _selection.SelectionType == apSelection.SELECTION_TYPE.Overall);


			isGizmoGUIVisible_VertexTransform = IsDelayedGUIVisible("Top UI - Vertex Transform");

			isGizmoGUIVisible_VTF_FFD = IsDelayedGUIVisible("Top UI - VTF FFD");
			if (Event.current.type == EventType.Layout)
			{
				_isGizmoGUIVisible_VTF_FFD_Prev = isGizmoGUIVisible_VTF_FFD;
			}
			isGizmoGUIVisible_VTF_Soft = IsDelayedGUIVisible("Top UI - VTF Soft");
			isGizmoGUIVisible_VTF_Blur = IsDelayedGUIVisible("Top UI - VTF Blur");

			isGizmoGUIVisible_Position = IsDelayedGUIVisible("Top UI - Position");
			isGizmoGUIVisible_Rotation = IsDelayedGUIVisible("Top UI - Rotation");
			isGizmoGUIVisible_Scale = IsDelayedGUIVisible("Top UI - Scale");
			isGizmoGUIVisible_Color = IsDelayedGUIVisible("Top UI - Color");

			//1. FFD, Soft, Blur 선택 버튼
			if (isGizmoGUIVisible_VertexTransform)
			{
				apEditorUtil.GUI_DelimeterBoxV(height - 15);
				GUILayout.Space(20);

				if (apEditorUtil.ToggledButton(ImageSet.Get(apImageSet.PRESET.ToolBtn_Transform),
					Gizmos.IsFFDMode, Gizmos.IsFFDModeAvailable, tabBtnWidth, tabBtnHeight,
					"Free Form Deformation / If you press the button while holding down [" + strCtrlKey + "], a dialog appears allowing you to change the number of control points."))
				{

#if UNITY_EDITOR_OSX
					bool isCtrl = Event.current.command;
#else
					bool isCtrl = Event.current.control;
#endif
					if (isCtrl)
					{
						//커스텀 사이즈로 연다.
						_loadKey_FFDStart = apDialog_FFDSize.ShowDialog(this, _portrait, OnDialogEvent_FFDStart, _curFFDSizeX, _curFFDSizeY);
					}
					else
					{


						Gizmos.StartTransformMode(this);//원래는 <이거 기본 3X3
					}

				}

				//2-1) FFD
				if (isGizmoGUIVisible_VTF_FFD)
				{
					if (apEditorUtil.ToggledButton(ImageSet.Get(apImageSet.PRESET.ToolBtn_TransformAdapt),
						false, Gizmos.IsFFDMode, tabBtnWidth, tabBtnHeight, "Apply FFD"))
					{
						//_gizmos.SetControlType(apGizmos.CONTROL_TYPE.Transform);
						//Gizmos.StartTransformMode();
						if (Gizmos.IsFFDMode)
						{
							Gizmos.AdaptTransformObjects(this);
						}
					}

					if (apEditorUtil.ToggledButton(ImageSet.Get(apImageSet.PRESET.ToolBtn_TransformRevert),
						false, Gizmos.IsFFDMode, tabBtnWidth, tabBtnHeight, "Revert FFD"))
					{
						//_gizmos.SetControlType(apGizmos.CONTROL_TYPE.Transform);
						//Gizmos.StartTransformMode();
						if (Gizmos.IsFFDMode)
						{
							Gizmos.RevertTransformObjects(this);
						}
					}

					GUILayout.Space(10);

					_isGizmoGUIVisible_VTF_FFD_Prev = true;
				}
				else if (_isGizmoGUIVisible_VTF_FFD_Prev)
				{
					//만약 Layout이 아닌 이전 이벤트에서 FFD 편집 중이었는데
					//갑자기 사라지는 경우 -> 더미를 출력하자
					apEditorUtil.ToggledButton(ImageSet.Get(apImageSet.PRESET.ToolBtn_TransformAdapt), false, Gizmos.IsFFDMode, tabBtnWidth, tabBtnHeight, "Apply FFD");

					apEditorUtil.ToggledButton(ImageSet.Get(apImageSet.PRESET.ToolBtn_TransformRevert), false, Gizmos.IsFFDMode, tabBtnWidth, tabBtnHeight, "Revert FFD");

					GUILayout.Space(10);
				}

				if (Event.current.type == EventType.Layout)
				{
					_isGizmoGUIVisible_VTF_FFD_Prev = isGizmoGUIVisible_VTF_FFD;
				}

				GUILayout.Space(10);

				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.ToolBtn_SoftSelection),
					Gizmos.IsSoftSelectionMode, Gizmos.IsSoftSelectionModeAvailable, tabBtnWidth, tabBtnHeight,
					"Soft Selection (Adjust brush size with [, ])"))
				{
					if (Gizmos.IsSoftSelectionMode)
					{
						Gizmos.EndSoftSelection();
					}
					else
					{
						Gizmos.StartSoftSelection();
					}
				}


				int labelSize = 70;
				int sliderSize = 200;
				int sliderSetSize = labelSize + sliderSize + 4;

				//2-2) Soft Selection
				if (isGizmoGUIVisible_VTF_Soft)
				{
					//Radius와 Curve
					EditorGUILayout.BeginVertical(GUILayout.Width(sliderSetSize));
					int softRadius = Gizmos.SoftSelectionRadius;
					int softCurveRatio = Gizmos.SoftSelectionCurveRatio;

					EditorGUILayout.BeginHorizontal(GUILayout.Width(sliderSetSize));
					EditorGUILayout.LabelField(GetUIWord(UIWORD.Radius), GUILayout.Width(labelSize));//"Radius"
					softRadius = EditorGUILayout.IntSlider(softRadius, 1, apGizmos.MAX_SOFT_SELECTION_RADIUS, GUILayout.Width(sliderSize));
					EditorGUILayout.EndHorizontal();

					//string strCurveLabel = "Curve";
					string strCurveLabel = GetUIWord(UIWORD.Curve);
					EditorGUILayout.BeginHorizontal(GUILayout.Width(sliderSetSize));
					EditorGUILayout.LabelField(strCurveLabel, GUILayout.Width(labelSize));
					softCurveRatio = EditorGUILayout.IntSlider(softCurveRatio, -100, 100, GUILayout.Width(sliderSize));
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.EndVertical();

					GUILayout.Space(10);

					if (softRadius != Gizmos.SoftSelectionRadius || softCurveRatio != Gizmos.SoftSelectionCurveRatio)
					{
						//TODO : 브러시 미리보기 기동
						Gizmos.RefreshSoftSelectionValue(softRadius, softCurveRatio);
					}
				}


				GUILayout.Space(10);

				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.ToolBtn_Blur),
					Gizmos.IsBlurMode, Gizmos.IsBlurModeAvailable, tabBtnWidth, tabBtnHeight,
					"Blur (Adjust brush size with [, ]"))
				{
					if (Gizmos.IsBlurMode)
					{
						Gizmos.EndBlur();
					}
					else
					{
						Gizmos.StartBlur();
					}
				}

				//2-3) Blur
				if (isGizmoGUIVisible_VTF_Blur)
				{
					//Range와 Intensity
					//Radius와 Curve
					EditorGUILayout.BeginVertical(GUILayout.Width(sliderSetSize));
					int blurRadius = Gizmos.BlurRadius;
					int blurIntensity = Gizmos.BlurIntensity;

					EditorGUILayout.BeginHorizontal(GUILayout.Width(sliderSetSize));
					EditorGUILayout.LabelField(GetUIWord(UIWORD.Radius), GUILayout.Width(labelSize));//"Radius"
					blurRadius = EditorGUILayout.IntSlider(blurRadius, 1, apGizmos.MAX_BLUR_RADIUS, GUILayout.Width(sliderSize));
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(GUILayout.Width(sliderSetSize));
					EditorGUILayout.LabelField(GetUIWord(UIWORD.Intensity), GUILayout.Width(labelSize));//"Intensity"
					blurIntensity = EditorGUILayout.IntSlider(blurIntensity, 0, 100, GUILayout.Width(sliderSize));
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.EndVertical();

					//GUILayout.Space(10);

					if (blurRadius != Gizmos.BlurRadius || blurIntensity != Gizmos.BlurIntensity)
					{
						//TODO : 브러시 미리보기 기동
						Gizmos.RefreshBlurValue(blurRadius, blurIntensity);
					}


				}

				GUILayout.Space(20);
			}





			//bool isGizmoGUIVisible_Position = IsDelayedGUIVisible("Top UI - Position");
			//bool isGizmoGUIVisible_Rotation = IsDelayedGUIVisible("Top UI - Rotation");
			//bool isGizmoGUIVisible_Scale = IsDelayedGUIVisible("Top UI - Scale");
			//bool isGizmoGUIVisible_Color = IsDelayedGUIVisible("Top UI - Color");

			//if (Gizmos._isGizmoRenderable)
			if (isGizmoGUIVisible_Position || isGizmoGUIVisible_Rotation || isGizmoGUIVisible_Scale || isGizmoGUIVisible_Color)
			{
				//하나라도 Gizmo Transform 이 나타난다면 이 영역을 그려준다.
				apEditorUtil.GUI_DelimeterBoxV(height - 15);
				GUILayout.Space(20);


				//Transform

				//Position
				apGizmos.TransformParam curTransformParam = Gizmos.GetCurTransformParam();
				Vector2 prevPos = Vector2.zero;
				int prevDepth = 0;
				float prevRotation = 0.0f;
				Vector2 prevScale2 = Vector2.one;
				Color prevColor = Color.black;
				bool prevVisible = true;

				if (curTransformParam != null)
				{
					//prevPos = curTransformParam._posW;//<<GUI용으로 변경
					prevPos = curTransformParam._pos_GUI;
					prevDepth = curTransformParam._depth;
					//prevRotation = curTransformParam._angle;
					prevRotation = curTransformParam._angle_GUI;//<<GUI용으로 변경

					//prevScale2 = curTransformParam._scale;
					prevScale2 = curTransformParam._scale_GUI;//GUI 용으로 변경
					prevColor = curTransformParam._color;
					prevVisible = curTransformParam._isVisible;
				}

				Vector2 curPos = prevPos;
				int curDepth = prevDepth;
				float curRotation = prevRotation;
				Vector2 curScale = prevScale2;
				Color curColor = prevColor;
				bool curVisible = prevVisible;

				if (isGizmoGUIVisible_Position)
				{
					GUIStyle guiStyle_Label = new GUIStyle(GUI.skin.label);
					if (gizmoUI_Position != apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
					{
						guiStyle_Label.normal.textColor = Color.gray;
					}


					EditorGUILayout.LabelField(new GUIContent(ImageSet.Get(apImageSet.PRESET.Transform_Move)), GUILayout.Width(30), GUILayout.Height(30));
					EditorGUILayout.BeginVertical(GUILayout.Width(130));
					EditorGUILayout.LabelField(GetUIWord(UIWORD.Position), guiStyle_Label, GUILayout.Width(130));//"Position"
					if (gizmoUI_Position == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
					{
						curPos = apEditorUtil.DelayedVector2Field(prevPos, 130);
					}
					else
					{
						curPos = apEditorUtil.DelayedVector2Field(Vector2.zero, 130);
					}
					EditorGUILayout.EndVertical();

					GUILayout.Space(10);

					//Depth와 Position은 같이 묶인다.
					EditorGUILayout.LabelField(new GUIContent(ImageSet.Get(apImageSet.PRESET.Transform_Depth)), GUILayout.Width(30), GUILayout.Height(30));
					EditorGUILayout.BeginVertical(GUILayout.Width(60));
					EditorGUILayout.LabelField(GetUIWord(UIWORD.Depth), guiStyle_Label, GUILayout.Width(60));//"Depth"
					if (gizmoUI_Position == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
					{
						curDepth = EditorGUILayout.DelayedIntField("", prevDepth, GUILayout.Width(60));
					}
					else
					{
						EditorGUILayout.DelayedIntField("", 0, GUILayout.Width(60));
					}
					EditorGUILayout.EndVertical();

					GUILayout.Space(10);
				}

				if (isGizmoGUIVisible_Rotation)
				{
					GUIStyle guiStyle_Label = new GUIStyle(GUI.skin.label);
					if (gizmoUI_Rotation != apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
					{
						guiStyle_Label.normal.textColor = Color.gray;
					}


					EditorGUILayout.LabelField(new GUIContent(ImageSet.Get(apImageSet.PRESET.Transform_Rotate)), GUILayout.Width(30), GUILayout.Height(30));
					EditorGUILayout.BeginVertical(GUILayout.Width(70));
					EditorGUILayout.LabelField(GetUIWord(UIWORD.Rotation), guiStyle_Label, GUILayout.Width(70));//"Rotation"
					if (gizmoUI_Rotation == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
					{
						curRotation = EditorGUILayout.DelayedFloatField(prevRotation, GUILayout.Width(70));
					}
					else
					{
						EditorGUILayout.DelayedFloatField(0.0f, GUILayout.Width(70));
					}
					EditorGUILayout.EndVertical();

					GUILayout.Space(10);
				}

				if (isGizmoGUIVisible_Scale)
				{
					GUIStyle guiStyle_Label = new GUIStyle(GUI.skin.label);
					if (gizmoUI_Scale != apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
					{
						guiStyle_Label.normal.textColor = Color.gray;
					}


					EditorGUILayout.LabelField(new GUIContent(ImageSet.Get(apImageSet.PRESET.Transform_Scale)), GUILayout.Width(30), GUILayout.Height(30));
					EditorGUILayout.BeginVertical(GUILayout.Width(120));
					EditorGUILayout.LabelField(GetUIWord(UIWORD.Scaling), guiStyle_Label, GUILayout.Width(120));//"Scaling"
					if (gizmoUI_Scale == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
					{
						//curScale = EditorGUILayout.Vector2Field("", prevScale2, GUILayout.Width(120));
						EditorGUILayout.BeginHorizontal(GUILayout.Width(120));
						curScale.x = EditorGUILayout.DelayedFloatField(prevScale2.x, GUILayout.Width(60 - 2));
						curScale.y = EditorGUILayout.DelayedFloatField(prevScale2.y, GUILayout.Width(60 - 2));
						EditorGUILayout.EndHorizontal();
					}
					else
					{
						//EditorGUILayout.Vector2Field("", Vector2.zero, GUILayout.Width(120));
						EditorGUILayout.BeginHorizontal(GUILayout.Width(120));
						EditorGUILayout.DelayedFloatField(0, GUILayout.Width(60 - 2));
						EditorGUILayout.DelayedFloatField(0, GUILayout.Width(60 - 2));
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();

					GUILayout.Space(10);
				}


				if (isGizmoGUIVisible_Color)
				{
					GUIStyle guiStyle_Label = new GUIStyle(GUI.skin.label);
					if (gizmoUI_Color != apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
					{
						guiStyle_Label.normal.textColor = Color.gray;
					}

					EditorGUILayout.LabelField(new GUIContent(ImageSet.Get(apImageSet.PRESET.Transform_Color)), GUILayout.Width(30), GUILayout.Height(30));
					EditorGUILayout.BeginVertical(GUILayout.Width(60));
					EditorGUILayout.LabelField(GetUIWord(UIWORD.Color), guiStyle_Label, GUILayout.Width(60));//"Color"
					if (gizmoUI_Color == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
					{
						curColor = EditorGUILayout.ColorField("", prevColor, GUILayout.Width(60));
					}
					else
					{
						EditorGUILayout.ColorField("", Color.black, GUILayout.Width(60));
					}
					EditorGUILayout.EndVertical();
					EditorGUILayout.BeginVertical(GUILayout.Width(40));
					EditorGUILayout.LabelField(GetUIWord(UIWORD.Visible), guiStyle_Label, GUILayout.Width(40));//"Visible"
					if (gizmoUI_Color == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
					{
						curVisible = EditorGUILayout.Toggle(prevVisible, GUILayout.Width(40));
					}
					else
					{
						EditorGUILayout.Toggle(false, GUILayout.Width(40));
					}
					EditorGUILayout.EndVertical();
				}

				if (curTransformParam != null)
				{
					if (gizmoUI_Position == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled && (prevPos != curPos || curDepth != prevDepth))
					{
						Gizmos.OnTransformChanged_PositionDepth(curPos, curDepth);
					}
					if (gizmoUI_Rotation == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled && (prevRotation != curRotation))
					{
						Gizmos.OnTransformChanged_Rotate(curRotation);
					}
					if (gizmoUI_Scale == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled && (prevScale2 != curScale))
					{
						Gizmos.OnTransformChanged_Scale(curScale);
					}
					if (gizmoUI_Color == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled && (prevColor != curColor || prevVisible != curVisible))
					{
						Gizmos.OnTransformChanged_Color(curColor, curVisible);
					}
				}

				GUILayout.Space(20);
			}


			//변경 4.9
			//Show Frame
			//>> Capture 버튼 및 Dialog 호출은 삭제한다. Capture기능은 우측 탭으로 이동한다.
			if (IsDelayedGUIVisible("Top UI - Overall"))
			{
				apEditorUtil.GUI_DelimeterBoxV(height - 15);
				GUILayout.Space(20);

				//"Show Frame", "Show Frame"
				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Capture_Frame), 
														"  " + GetUIWord(UIWORD.ShowFrame), 
														"  " + GetUIWord(UIWORD.ShowFrame), 
														_isShowCaptureFrame, true, 130, tabBtnHeight))
				{
					_isShowCaptureFrame = !_isShowCaptureFrame;
				}

				//"Capture" >> 이게 삭제된다.
				//if (GUILayout.Button(GetUIWord(UIWORD.Capture), GUILayout.Width(80), GUILayout.Height(tabBtnHeight)))
				//{

				//	_dialogShowCall = DIALOG_SHOW_CALL.Capture;
				//}
				GUILayout.Space(20);
			}

			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);

		}

		private void GUI_HotKey_TopRight()
		{
			bool isGizmoUpdatable = Gizmos.IsUpdatable;
			//기즈모를 단축키로 넣자
			if (isGizmoUpdatable)
			{
				AddHotKeyEvent(Controller.OnHotKeyEvent_GizmoSelect, "Select", KeyCode.Q, false, false, false, null);
				AddHotKeyEvent(Controller.OnHotKeyEvent_GizmoMove, "Move", KeyCode.W, false, false, false, null);
				AddHotKeyEvent(Controller.OnHotKeyEvent_GizmoRotate, "Rotate", KeyCode.E, false, false, false, null);
				AddHotKeyEvent(Controller.OnHotKeyEvent_GizmoScale, "Scale", KeyCode.R, false, false, false, null);
			}

			//Onion
			bool isOnionButtonAvailable = false;
			if(Select.SelectionType == apSelection.SELECTION_TYPE.Animation ||
				Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				isOnionButtonAvailable = true;
			}

			//Onion 단축키
			if (isOnionButtonAvailable)
			{
				AddHotKeyEvent(Controller.OnHotKeyEvent_OnionVisibleToggle, "Onion Skin Toggle", KeyCode.O, false, false, false, null);
			}

			//Bone Visible
			bool isBoneVisibleButtonAvailable = _selection.SelectionType == apSelection.SELECTION_TYPE.MeshGroup ||
				_selection.SelectionType == apSelection.SELECTION_TYPE.Animation ||
				_selection.SelectionType == apSelection.SELECTION_TYPE.Overall;

			if (isBoneVisibleButtonAvailable)
			{
				//단축키 B로 Bone 렌더링 정보를 토글할 수 있다.
				AddHotKeyEvent(OnHotKeyEvent_BoneVisibleToggle, "Change Bone Visiblity", KeyCode.B, false, false, false, null);
			}

			//Vertex 제어시 단축키
			apGizmos.TRANSFORM_UI_VALID gizmoUI_VetexTF = Gizmos.TransformUI_VertexTransform;

			if (gizmoUI_VetexTF != apGizmos.TRANSFORM_UI_VALID.Hide && Gizmos.IsSoftSelectionMode)
			{
				//Vertex Transform - Soft 툴
				//크기 조절 단축키
				AddHotKeyEvent(Gizmos.IncreaseSoftSelectionRadius, "Increase Brush Size", KeyCode.RightBracket, false, false, false, null);
				AddHotKeyEvent(Gizmos.DecreaseSoftSelectionRadius, "Decrease Brush Size", KeyCode.LeftBracket, false, false, false, null);
			}
			else if (gizmoUI_VetexTF != apGizmos.TRANSFORM_UI_VALID.Hide && Gizmos.IsBlurMode)
			{
				//Vertex Transform - Blur 툴
				AddHotKeyEvent(Gizmos.IncreaseBlurRadius, "Increase Brush Size", KeyCode.RightBracket, false, false, false, null);
				AddHotKeyEvent(Gizmos.DecreaseBlurRadius, "Decreash Brush Size", KeyCode.LeftBracket, false, false, false, null);
			}
			

			if(Select.SelectionType == apSelection.SELECTION_TYPE.Mesh
				&& Select.Mesh != null
				&& _meshEditMode == MESH_EDIT_MODE.MakeMesh
				&& _meshEditeMode_MakeMesh == MESH_EDIT_MODE_MAKEMESH.Polygon)
			{
				//Delete 키로 Polygon을 삭제하는 단축키
				AddHotKeyEvent(Controller.RemoveSelectedMeshPolygon, "Remove Polygon", KeyCode.Delete, false, false, false, null);
			}
		}



		/// <summary>
		/// 단축키 B를 눌러서 Bone Visible을 바꾼다.
		/// </summary>
		/// <param name="paramObject"></param>
		private void OnHotKeyEvent_BoneVisibleToggle(object paramObject)
		{
			switch (_boneGUIRenderMode)
			{
				case BONE_RENDER_MODE.None: _boneGUIRenderMode = BONE_RENDER_MODE.Render; break;
				case BONE_RENDER_MODE.Render: _boneGUIRenderMode = BONE_RENDER_MODE.RenderOutline; break;
				case BONE_RENDER_MODE.RenderOutline: _boneGUIRenderMode = BONE_RENDER_MODE.None; break;
			}
			SaveEditorPref();
		}

		/// <summary>
		/// 단축키 Alt+W를 눌러서 FullScreen 모드를 바꾼다.
		/// </summary>
		/// <param name="paramObject"></param>
		private void OnHotKeyEvent_FullScreenToggle(object paramObject)
		{
			_isFullScreenGUI = !_isFullScreenGUI;
		}

		public void ResetHierarchyAll()
		{
			if (_portrait == null)
			{
				return;
			}

			Hierarchy.ResetAllUnits();
			_hierarchy_MeshGroup.ResetSubUnits();
			_hierarchy_AnimClip.ResetSubUnits();
		}

		public void RefreshControllerAndHierarchy()
		{
			if (_portrait == null)
			{
				//Repaint();
				SetRepaint();
				return;
			}

			//메시 그룹들을 체크한다.
			Controller.RefreshMeshGroups();
			Controller.CheckMeshEdgeWorkRemained();


			Hierarchy.RefreshUnits();
			_hierarchy_MeshGroup.RefreshUnits();
			_hierarchy_AnimClip.RefreshUnits();

			RefreshTimelineLayers(false);

			//통계 재계산 요청
			Select.SetStatisticsRefresh();
			

			//Repaint();
			SetRepaint();
		}


		public void RefreshTimelineLayers(bool isReset)
		{
			apAnimClip curAnimClip = Select.AnimClip;
			if (curAnimClip == null)
			{
				_prevAnimClipForTimeline = null;
				_timelineInfoList.Clear();
				
				//Common Keyframe을 갱신하자
				Select.RefreshCommonAnimKeyframes();
				return;
			}
			if (curAnimClip != _prevAnimClipForTimeline)
			{
				isReset = true;//강제로 리셋한다.
				_prevAnimClipForTimeline = curAnimClip;
			}

			//추가 : 타임라인값도 리프레시 (Sorting 등)
			curAnimClip.RefreshTimelines();

			if (isReset || _timelineInfoList.Count == 0)
			{
				_timelineInfoList.Clear();
				//AnimClip에 맞게 리스트를 다시 만든다.

				List<apAnimTimeline> timelines = curAnimClip._timelines;
				for (int iTimeline = 0; iTimeline < timelines.Count; iTimeline++)
				{
					apAnimTimeline timeline = timelines[iTimeline];
					apTimelineLayerInfo timelineInfo = new apTimelineLayerInfo(timeline);
					_timelineInfoList.Add(timelineInfo);


					List<apTimelineLayerInfo> subLayers = new List<apTimelineLayerInfo>();
					for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
					{
						apAnimTimelineLayer animLayer = timeline._layers[iLayer];
						subLayers.Add(new apTimelineLayerInfo(animLayer, timeline, timelineInfo));
					}

					//정렬을 여기서 한다.
					switch (_timelineInfoSortType)
					{
						case TIMELINE_INFO_SORT.Registered:
							//정렬을 안한다.
							break;

						case TIMELINE_INFO_SORT.Depth:
							if (timeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
							{
								//Modifier가 Transform을 지원하는 경우
								//Bone이 위쪽에 속한다.
								subLayers.Sort(delegate (apTimelineLayerInfo a, apTimelineLayerInfo b)
								{
									if (a._layerType == b._layerType)
									{
										if (a._layerType == apTimelineLayerInfo.LAYER_TYPE.Transform)
										{
											return b.Depth - a.Depth;
										}
										else
										{
											return a.Depth - b.Depth;
										}

									}
									else
									{
										return (int)a._layerType - (int)b._layerType;
									}
								});
							}
							break;

						case TIMELINE_INFO_SORT.ABC:
							subLayers.Sort(delegate (apTimelineLayerInfo a, apTimelineLayerInfo b)
							{
								return string.Compare(a.DisplayName, b.DisplayName);
							});
							break;
					}

					//정렬 된 걸 넣어주자
					for (int iSub = 0; iSub < subLayers.Count; iSub++)
					{
						_timelineInfoList.Add(subLayers[iSub]);
					}

				}
			}

			//키프레임-모디파이어 연동도 해주자
			for (int iTimeline = 0; iTimeline < curAnimClip._timelines.Count; iTimeline++)
			{
				apAnimTimeline timeline = curAnimClip._timelines[iTimeline];
				if (timeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier &&
					timeline._linkedModifier != null)
				{
					for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
					{
						apAnimTimelineLayer layer = timeline._layers[iLayer];

						apModifierParamSetGroup paramSetGroup = timeline._linkedModifier._paramSetGroup_controller.Find(delegate (apModifierParamSetGroup a)
						{
							return a._keyAnimTimelineLayer == layer;
						});

						if (paramSetGroup != null)
						{
							for (int iKey = 0; iKey < layer._keyframes.Count; iKey++)
							{
								apAnimKeyframe keyframe = layer._keyframes[iKey];
								apModifierParamSet paramSet = paramSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
								{
									return a.SyncKeyframe == keyframe;
								});

								if (paramSet != null && paramSet._meshData.Count > 0)
								{
									keyframe.LinkModMesh_Editor(paramSet, paramSet._meshData[0]);
								}
								else if (paramSet != null && paramSet._boneData.Count > 0)//<<추가 : boneData => ModBone
								{
									keyframe.LinkModBone_Editor(paramSet, paramSet._boneData[0]);
								}
								else
								{
									keyframe.LinkModMesh_Editor(null, null);//<<null, null을 넣으면 ModBone도 Null이 된다.
								}
							}
						}
					}

				}

			}


			//Select / Available 체크를 하자
			for (int i = 0; i < _timelineInfoList.Count; i++)
			{
				apTimelineLayerInfo info = _timelineInfoList[i];

				info._isSelected = false;
				info._isAvailable = false;

				if (info._isTimeline)
				{
					if (Select.ExAnimEditingMode != apSelection.EX_EDIT.None)
					{
						if (info._timeline == Select.AnimTimeline)
						{ info._isAvailable = true; }
					}
					else
					{
						info._isAvailable = true;
					}

					if (info._isAvailable)
					{
						if (Select.AnimTimelineLayer == null)
						{
							if (info._timeline == Select.AnimTimeline)
							{
								info._isSelected = true;
							}
						}
					}
				}
				else
				{
					if (Select.ExAnimEditingMode != apSelection.EX_EDIT.None)
					{
						if (info._parentTimeline == Select.AnimTimeline)
						{
							info._isAvailable = true;
							info._isSelected = (info._layer == Select.AnimTimelineLayer);

						}
					}
					else
					{
						info._isAvailable = true;
						info._isSelected = (info._layer == Select.AnimTimelineLayer);
					}
				}
			}

			//Common Keyframe을 갱신하자
			Select.RefreshCommonAnimKeyframes();


			//통계 재계산 요청
			Select.SetStatisticsRefresh();

			SetRepaint();
		}

		public void ShowAllTimelineLayers()
		{
			for (int i = 0; i < _timelineInfoList.Count; i++)
			{
				_timelineInfoList[i].ShowLayer();
			}
		}


#region [미사용 코드] 탭 변환 코드
		//private void ChangeTab(TAB nextTab)
		//{
		//	if (_portrait == null && _tab != TAB.ProjectSetting)
		//	{
		//		Debug.LogError("Portrait Object is not Implemented");

		//		_tab = TAB.ProjectSetting;
		//		_scroll_MainLeft = Vector2.zero;
		//		_scroll_MainCenter = Vector2.zero;
		//		_scroll_MainRight = Vector2.zero;
		//		_scroll_Bottom = Vector2.zero;

		//		Repaint();

		//		return;
		//	}
		//	if(nextTab != TAB.MeshEdit)
		//	{
		//		Controller.CheckMeshEdgeWorkRemained();
		//	}

		//	_tab = nextTab;
		//	_scroll_MainLeft = Vector2.zero;
		//	_scroll_MainCenter = Vector2.zero;
		//	_scroll_MainRight = Vector2.zero;
		//	_scroll_Bottom = Vector2.zero;

		//	_meshEditMode = MESH_EDIT_MODE.None;

		//	Hierarchy.RefreshUnits();
		//	Repaint();
		//} 
#endregion

#region [미사용 코드] : Gizmo 출력 여부 관련 코드
		//private void CheckGizmoAvailable()
		//{
		//	bool isLock = false;

		//	//TODO
		//	//언제 기즈모가 나오는지 결정하는 코드

		//	//switch (_tab)
		//	//{
		//	//	case TAB.ProjectSetting:
		//	//		isLock = true;
		//	//		break;

		//	//	case TAB.MeshEdit:
		//	//		switch (_meshEditMode)
		//	//		{
		//	//			case MESH_EDIT_MODE.None:
		//	//			case MESH_EDIT_MODE.Modify:
		//	//			case MESH_EDIT_MODE.AddVertex:
		//	//			case MESH_EDIT_MODE.LinkEdge:
		//	//			case MESH_EDIT_MODE.VolumeWeight:
		//	//			case MESH_EDIT_MODE.PhysicWeight:
		//	//				isLock = true;
		//	//				break;

		//	//			case MESH_EDIT_MODE.PivotEdit:
		//	//				//isLock = false;
		//	//				isLock = false;
		//	//				break;
		//	//		}
		//	//		break;

		//	//	case TAB.FaceEdit:
		//	//		isLock = true;
		//	//		break;

		//	//	case TAB.RigEdit:
		//	//		isLock = false;
		//	//		break;

		//	//	case TAB.Animation:
		//	//		isLock = false;
		//	//		break;
		//	//}
		//	//_gizmos.SetLock(isLock);
		//} 
#endregion
		//--------------------------------------------------------------


		//--------------------------------------------------------------
		private object _loadKey_MakeNewPortrait = null;
		// 각 레이아웃 별 GUI Render
		private int GUI_MainLeft_Upper(int width)
		{
			if (_portrait == null)
			{
				bool isRefresh = false;
				if (GUILayout.Button(new GUIContent("   " + GetUIWord(UIWORD.MakeNewPortrait), ImageSet.Get(apImageSet.PRESET.Hierarchy_MakeNewPortrait), "Create a new Portrait"), GUILayout.Height(45)))
				{
					//Portrait를 생성하는 Dialog를 띄우자
					_loadKey_MakeNewPortrait = apDialog_NewPortrait.ShowDialog(this, OnMakeNewPortraitResult);

					//GameObject newPortraitObj = new GameObject("New Portrait");
					//newPortraitObj.transform.position = Vector3.zero;
					//newPortraitObj.transform.rotation = Quaternion.identity;
					//newPortraitObj.transform.localScale = Vector3.one;

					//_portrait = newPortraitObj.AddComponent<apPortrait>();

					////Selection.activeGameObject = newPortraitObj;
					//Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져
				}

				GUILayout.Space(5);
				if (GUILayout.Button(new GUIContent("   " + GetUIWord(UIWORD.RefreshToLoad), ImageSet.Get(apImageSet.PRESET.Controller_Default), "Search Portraits again in the scene"), GUILayout.Height(25)))
				{
					isRefresh = true;
				}
				if(GUILayout.Button(new GUIContent(GetUIWord(UIWORD.LoadBackupFile), "Open a Portrait saved as a backup file. It will be created as a new Portrait"), GUILayout.Height(20)))
				{
					string strPath = EditorUtility.OpenFilePanel("Load Backup File", "", "bck");
					if(!string.IsNullOrEmpty(strPath))
					{
						//Debug.Log("Load Backup File [" + strPath + "]");

						_isMakePortraitRequestFromBackupFile = true;
						_requestedLoadedBackupPortraitFilePath = strPath;
					}
					
				}

				if (_portraitsInScene.Count == 0 && !_isPortraitListLoaded)
				{
					isRefresh = true;
				}

				if (isRefresh)
				{
					//씬에 있는 Portrait를 찾는다.
					//추가) 썸네일도 표시
					_portraitsInScene.Clear();
					apPortrait[] portraits = UnityEngine.Object.FindObjectsOfType<apPortrait>();
					if (portraits != null)
					{
						for (int i = 0; i < portraits.Length; i++)
						{
							//Opt Portrait는 생략한다.
							if(portraits[i]._isOptimizedPortrait)
							{
								continue;
							}

							//썸네일을 연결하자
							string thumnailPath = portraits[i]._imageFilePath_Thumbnail;
							if (string.IsNullOrEmpty(thumnailPath))
							{
								portraits[i]._thumbnailImage = null;
							}
							else
							{
								Texture2D thumnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(thumnailPath);
								portraits[i]._thumbnailImage = thumnailImage;
							}

							_portraitsInScene.Add(portraits[i]);
						}
					}

					_isPortraitListLoaded = true;
				}
				return 85;
			}

			if (_tabLeft == TAB_LEFT.Hierarchy)
			{
				int filterIconSize = (width / 8) - 2;

				//Hierarchy의 필터 선택 버튼들
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(filterIconSize + 5));
				GUILayout.Space(7);
				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_All), IsHierarchyFilterContain(HIERARCHY_FILTER.All), true, filterIconSize, filterIconSize, "Show All"))
				{ SetHierarchyFilter(HIERARCHY_FILTER.All, true); }//All
				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_Root), IsHierarchyFilterContain(HIERARCHY_FILTER.RootUnit), true, filterIconSize, filterIconSize, "Root Units"))
				{ SetHierarchyFilter(HIERARCHY_FILTER.RootUnit, !IsHierarchyFilterContain(HIERARCHY_FILTER.RootUnit)); } //Root Toggle
				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_Image), IsHierarchyFilterContain(HIERARCHY_FILTER.Image), true, filterIconSize, filterIconSize, "Images"))
				{ SetHierarchyFilter(HIERARCHY_FILTER.Image, !IsHierarchyFilterContain(HIERARCHY_FILTER.Image)); }//Image Toggle
				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh), IsHierarchyFilterContain(HIERARCHY_FILTER.Mesh), true, filterIconSize, filterIconSize, "Meshes"))
				{ SetHierarchyFilter(HIERARCHY_FILTER.Mesh, !IsHierarchyFilterContain(HIERARCHY_FILTER.Mesh)); }//Mesh Toggle
				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup), IsHierarchyFilterContain(HIERARCHY_FILTER.MeshGroup), true, filterIconSize, filterIconSize, "Mesh Groups"))
				{ SetHierarchyFilter(HIERARCHY_FILTER.MeshGroup, !IsHierarchyFilterContain(HIERARCHY_FILTER.MeshGroup)); }//MeshGroup Toggle
				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation), IsHierarchyFilterContain(HIERARCHY_FILTER.Animation), true, filterIconSize, filterIconSize, "Animation Clips"))
				{ SetHierarchyFilter(HIERARCHY_FILTER.Animation, !IsHierarchyFilterContain(HIERARCHY_FILTER.Animation)); }//Animation Toggle
				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_Param), IsHierarchyFilterContain(HIERARCHY_FILTER.Param), true, filterIconSize, filterIconSize, "Control Parameters"))
				{ SetHierarchyFilter(HIERARCHY_FILTER.Param, !IsHierarchyFilterContain(HIERARCHY_FILTER.Param)); }//Param Toggle
				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_None), IsHierarchyFilterContain(HIERARCHY_FILTER.None), true, filterIconSize, filterIconSize, "Hide All"))
				{ SetHierarchyFilter(HIERARCHY_FILTER.None, true); }//None

				EditorGUILayout.EndHorizontal();

				return filterIconSize + 5;
			}
			else
			{
				return Controller.GUI_Controller_Upper(width);
			}
		}


		private void OnMakeNewPortraitResult(bool isSuccess, object loadKey, string name)
		{
			if (!isSuccess || loadKey != _loadKey_MakeNewPortrait)
			{
				_loadKey_MakeNewPortrait = null;
				return;
			}
			_loadKey_MakeNewPortrait = null;

			//Portrait를 만들어준다.
			_isMakePortraitRequest = true;
			_requestedNewPortraitName = name;
			if (string.IsNullOrEmpty(_requestedNewPortraitName))
			{
				_requestedNewPortraitName = "<No Named Portrait>";
			}


		}

		



		private void GUI_MainLeft(int width, int height, float scrollX, float scrollY, bool isGUIEvent)
		{
#region [미사용 코드] 해당 내용은 Top으로 이동
			//if (_portrait == null)
			//{
			//	EditorGUILayout.LabelField("Portrait is Empty");
			//	EditorGUILayout.LabelField("Please, Select or Create new");

			//	EditorGUILayout.Space();
			//	EditorGUILayout.LabelField("Portrait Object From Scene");
			//	_portrait = EditorGUILayout.ObjectField(_portrait, typeof(apPortrait), true) as apPortrait;

			//	//바뀌었다.
			//	if (_portrait != null)
			//	{
			//		Controller.InitTmpValues();
			//		_selection.SetPortrait(_portrait);

			//		for (int iMeshes = 0; iMeshes < _portrait._meshes.Count; iMeshes++)
			//		{
			//			_portrait._meshes[iMeshes].ReadyToEdit();
			//		}

			//		_hierarchy.ResetAllUnits();

			//	}

			//	EditorGUILayout.Space();
			//	if(GUILayout.Button("Make New Portrait On Scene", GUILayout.Height(30)))
			//	{
			//		apEditorUtil.SetRecord("New Portrait", _portrait);

			//		GameObject newPortraitObj = new GameObject("New Portrait");
			//		newPortraitObj.transform.position = Vector3.zero;
			//		newPortraitObj.transform.rotation = Quaternion.identity;
			//		newPortraitObj.transform.localScale = Vector3.one;

			//		_portrait = newPortraitObj.AddComponent<apPortrait>();

			//		Selection.activeGameObject = newPortraitObj;


			//	}

			//	GUILayout.Space(height);
			//	return;
			//}

			//EditorGUILayout.Space();
			//EditorGUILayout.LabelField("Portrait Object");
			//apPortrait prevPortrait = _portrait;
			//_portrait = EditorGUILayout.ObjectField(_portrait, typeof(apPortrait), true) as apPortrait;
			//if(_portrait != prevPortrait)
			//{
			//	//바뀌었다.
			//	if (_portrait != null)
			//	{
			//		Controller.InitTmpValues();
			//		_selection.SetPortrait(_portrait);

			//		for (int iMeshes = 0; iMeshes < _portrait._meshes.Count; iMeshes++)
			//		{
			//			_portrait._meshes[iMeshes].ReadyToEdit();
			//		}
			//		Selection.activeGameObject = _portrait.gameObject;
			//	}
			//	else
			//	{
			//		Controller.InitTmpValues();
			//		_selection.SetNone();
			//	}

			//	_hierarchy.ResetAllUnits();
			//} 
#endregion

			GUILayout.Space(20);

			if (_portrait == null)
			{
				int portraitSelectWidth = width - 10;
				int thumbnailHeight = 60;
				int thumbnailWidth = thumbnailHeight * 2;
				

				GUIStyle guiStyle_Thumb = new GUIStyle(GUI.skin.box);
				guiStyle_Thumb.margin = GUI.skin.label.margin;
				guiStyle_Thumb.padding = GUI.skin.label.padding;


//#if UNITY_EDITOR_WIN

				int selectBtnWidth = portraitSelectWidth - (thumbnailWidth);
				int portraitSelectHeight = thumbnailHeight + 24;

				for (int i = 0; i < _portraitsInScene.Count; i++)
				{
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 10), GUILayout.Height(portraitSelectHeight));
					GUILayout.Space(5);
					EditorGUILayout.BeginVertical();



					EditorGUILayout.LabelField(_portraitsInScene[i].transform.name, GUILayout.Width(portraitSelectWidth));

					EditorGUILayout.BeginHorizontal();

					GUILayout.Box(_portraitsInScene[i]._thumbnailImage, guiStyle_Thumb, GUILayout.Width(thumbnailWidth), GUILayout.Height(thumbnailHeight));
					//EditorGUILayout.LabelField(_portraitsInScene[i].transform.name, GUILayout.Width(width - (5 + 75)));
					if (GUILayout.Button(GetUIWord(UIWORD.Select), GUILayout.Width(selectBtnWidth), GUILayout.Height(thumbnailHeight)))
					{
						//바뀌었다.
						if (!_portraitsInScene[i]._isOptimizedPortrait)
						{
							_portrait = _portraitsInScene[i];
							if (_portrait != null)
							{
								Controller.InitTmpValues();
								_selection.SetPortrait(_portrait);

								//Portrait의 레퍼런스들을 연결해주자
								Controller.PortraitReadyToEdit();


								//Selection.activeGameObject = _portrait.gameObject;
								Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

								//시작은 RootUnit
								_selection.SetOverallDefault();

								OnAnyObjectAddedOrRemoved();
							}
							else
							{
								Controller.InitTmpValues();
								_selection.SetNone();
							}

							_hierarchy.ResetAllUnits();
							_hierarchy_MeshGroup.ResetSubUnits();
							_hierarchy_AnimClip.ResetSubUnits();
						}
					}
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(10);
				}


				//#else
#region [미사용 코드 : 이제 Mac에서도 썸네일을 볼 수 있다]
				////OSX에서는 썸네일 없이 그냥 이름 + 버튼으로 바꾼다.
				//for (int i = 0; i < _portraitsInScene.Count; i++)
				//{
				//	EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 10), GUILayout.Height(45));
				//	GUILayout.Space(5);
				//	EditorGUILayout.BeginVertical();



				//	EditorGUILayout.LabelField(_portraitsInScene[i].transform.name, GUILayout.Width(portraitSelectWidth));

				//	if (GUILayout.Button(GetUIWord(UIWORD.Select), GUILayout.Width(portraitSelectWidth), GUILayout.Height(20)))
				//	{
				//		//바뀌었다.
				//		if (!_portraitsInScene[i]._isOptimizedPortrait)
				//		{
				//			_portrait = _portraitsInScene[i];
				//			if (_portrait != null)
				//			{
				//				Controller.InitTmpValues();
				//				_selection.SetPortrait(_portrait);

				//				//Portrait의 레퍼런스들을 연결해주자
				//				Controller.PortraitReadyToEdit();


				//				//Selection.activeGameObject = _portrait.gameObject;
				//				Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

				//				//시작은 RootUnit
				//				_selection.SetOverallDefault();
				//			}
				//			else
				//			{
				//				Controller.InitTmpValues();
				//				_selection.SetNone();
				//			}

				//			_hierarchy.ResetAllUnits();
				//			_hierarchy_MeshGroup.ResetSubUnits();
				//			_hierarchy_AnimClip.ResetSubUnits();
				//		}
				//	}
				//	EditorGUILayout.EndVertical();
				//	EditorGUILayout.EndHorizontal();

				//	GUILayout.Space(10);
				//} 
#endregion
				//#endif
				// 백업에서 만드는 기능 하나 추가

			}
			else
			{
				if (_tabLeft == TAB_LEFT.Hierarchy)
				{

					Hierarchy.GUI_RenderHierarchy(width, _hierarchyFilter, scrollX, isGUIEvent);
				}
				else
				{
					

					apControllerGL.SetMouseState(
						_mouseBtn[MOUSE_BTN_LEFT_NOT_BOUND].Status,
						apMouse.PosNotBound,
						Event.current.isMouse
						//&& string.IsNullOrEmpty(GUI.GetNameOfFocusedControl())
						);

					//Profiler.BeginSample("Left : Controller UI");
					//컨트롤 파라미터를 처리하자
					Controller.GUI_Controller(width, height, (int)scrollY);
					//Profiler.EndSample();

				}
			}
		}


		private void GUI_MainCenter(int width, int height)
		{
			if (Event.current.type != EventType.Repaint &&
				!Event.current.isMouse && !Event.current.isKey)
			{
				return;
			}

			float deltaTime = 0.0f;
			if (Event.current.type == EventType.Repaint)
			{
				deltaTime = DeltaTime_Repaint;
			}

			//Input은 여기서 미리 처리한다.
			//--------------------------------------------------
			bool isMeshGroupUpdatable = _isUpdateWhenInputEvent || Event.current.type == EventType.Repaint;
			if (_isUpdateWhenInputEvent)
			{
				_isUpdateWhenInputEvent = false;
			}



			//클릭 후 GUIFocust를 릴리즈하자
			Controller.GUI_Input_CheckClickInCenter();

			switch (_selection.SelectionType)
			{
				case apSelection.SELECTION_TYPE.None:
					break;

				case apSelection.SELECTION_TYPE.Overall:
					{
						if (Event.current.type == EventType.Repaint)//업데이트는 Repaint에서만
						{
							if (Select.RootUnit != null)
							{
								if (Select.RootUnitAnimClip != null)
								{
									//실행중인 AnimClip이 있다.
									int curFrame = Select.RootUnitAnimClip.CurFrame;

									if (_isValidGUIRepaint)
									{
										_portrait.ForceManager.Update(deltaTime);//<<힘 업데이트 추가

										//애니메이션 재생!!!
										//Profiler.BeginSample("Anim - Root Animation");
										Select.RootUnitAnimClip.Update_Editor(deltaTime, true);
										//Profiler.EndSample();
									}

									//프레임이 바뀌면 AutoScroll 옵션을 적용한다.
									if (curFrame != Select.RootUnitAnimClip.CurFrame)
									{
										if (_isAnimAutoScroll)
										{
											Select.SetAutoAnimScroll();
										}
									}
								}
								else
								{
									//AnimClip이 없다면 자체 실행
									Select.RootUnit.Update(deltaTime);
								}

							}
						}
					}
					break;

				case apSelection.SELECTION_TYPE.Param:
					break;

				case apSelection.SELECTION_TYPE.ImageRes:
					break;

				case apSelection.SELECTION_TYPE.Mesh:
					if (_selection.Mesh != null)
					{
						switch (_meshEditMode)
						{
							case MESH_EDIT_MODE.Setting:
								break;

							case MESH_EDIT_MODE.Modify:
								Controller.GUI_Input_Modify(deltaTime);
								break;

							case MESH_EDIT_MODE.MakeMesh:
								Controller.GUI_Input_MakeMesh(_meshEditeMode_MakeMesh);
								break;

							case MESH_EDIT_MODE.PivotEdit:
								Controller.GUI_Input_PivotEdit(deltaTime);
								break;
						}
					}
					break;

				case apSelection.SELECTION_TYPE.MeshGroup:
					{
						switch (_meshGroupEditMode)
						{
							case MESHGROUP_EDIT_MODE.Setting:
								Controller.GUI_Input_MeshGroup_Setting(deltaTime);
								break;

							case MESHGROUP_EDIT_MODE.Bone:
								//Debug.Log("Bone Edit : " + Event.current.type);
								Controller.GUI_Input_MeshGroup_Bone(deltaTime);
								break;

							case MESHGROUP_EDIT_MODE.Modifier:
								{
									apModifierBase.MODIFIER_TYPE modifierType = apModifierBase.MODIFIER_TYPE.Base;
									if (Select.Modifier != null)
									{
										modifierType = Select.Modifier.ModifierType;
									}
									Controller.GUI_Input_MeshGroup_Modifier(modifierType, deltaTime);

								}
								break;
						}

						if (Event.current.type == EventType.Repaint)//업데이트는 Repaint에서만
						{
							if (Select.MeshGroup != null)
							{
								if (_isValidGUIRepaint)
								{
									
									_portrait.ForceManager.Update(deltaTime);//<<힘 업데이트 추가

									//변경 : 업데이트 가능한 상태에서만 업데이트를 한다.
									if (isMeshGroupUpdatable)
									{
										//Profiler.BeginSample("MeshGroup Update");
										//1. Render Unit을 돌면서 렌더링을 한다.
										Select.MeshGroup.UpdateRenderUnits(deltaTime, true);
										//Profiler.EndSample();
									}
								}
							}
						}
					}
					break;

				case apSelection.SELECTION_TYPE.Animation:
					{
						Controller.GUI_Input_Animation(deltaTime);

						if (Event.current.type == EventType.Repaint)//업데이트는 Repaint에서만
						{
							if (Select.AnimClip != null)
							{
								if (_isValidGUIRepaint)
								{
									_portrait.ForceManager.Update(deltaTime);//<<힘 업데이트 추가

									//변경 : 업데이트 가능한 상태에서만 업데이트를 한다.
									if (isMeshGroupUpdatable)
									{
										int curFrame = Select.AnimClip.CurFrame;
										//애니메이션 업데이트를 해야..
										Select.AnimClip.Update_Editor(deltaTime, Select.ExAnimEditingMode != apSelection.EX_EDIT.None);

										//프레임이 바뀌면 AutoScroll 옵션을 적용한다.
										if (curFrame != Select.AnimClip.CurFrame)
										{
											if (_isAnimAutoScroll)
											{
												Select.SetAutoAnimScroll();
											}
										}
									}
								}
							}
						}
					}
					break;
			}



			if (_isValidGUIRepaint)
			{
				_isValidGUIRepaint = false;
			}



			////업데이트도 해주자
			//UseFrameCount();

			

			//렌더링은 Repaint에서만
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			
			//만약 렌더링 클리핑을 갱신할 필요가 있다면 처리할 것
			if(_isRefreshClippingGL)
			{
				_isRefreshClippingGL = false;
				apGL.RefreshScreenSizeToBatch();
			}

			apGL.DrawGrid(_colorOption_GridCenter, _colorOption_Grid);

			

			switch (_selection.SelectionType)
			{
				case apSelection.SELECTION_TYPE.None:
					break;

				case apSelection.SELECTION_TYPE.Overall:
					{
						//_portrait._rootUnit.Update(DeltaFrameTime);

						if (Select.RootUnit != null)
						{
							if (Select.RootUnit._childMeshGroup != null)
							{
								apMeshGroup rootMeshGroup = Select.RootUnit._childMeshGroup;
#region [미사용 코드] 함수로 대체한다.
								//for (int iUnit = 0; iUnit < rootMeshGroup._renderUnits_All.Count; iUnit++)
								//{
								//	apRenderUnit renderUnit = rootMeshGroup._renderUnits_All[iUnit];
								//	if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
								//	{
								//		if (renderUnit._meshTransform != null)
								//		{
								//			if(!renderUnit._meshTransform._isVisible_Default)
								//			{
								//				continue;
								//			}

								//			if (renderUnit._meshTransform._isClipping_Parent)
								//			{
								//				//Clipping이면 Child (최대3)까지 마스크 상태로 출력한다.
								//				apGL.DrawRenderUnit_ClippingParent(	renderUnit, 
								//													renderUnit._meshTransform._clipChildMeshTransforms, 
								//													renderUnit._meshTransform._clipChildRenderUnits,
								//													VertController, 
								//													Select);
								//			}
								//			else if (renderUnit._meshTransform._isClipping_Child)
								//			{
								//				//렌더링은 생략한다.
								//			}
								//			else
								//			{
								//				apGL.DrawRenderUnit(renderUnit, apGL.RENDER_TYPE.Default, VertController, Select);
								//			}
								//		}
								//	}
								//} 
#endregion

								RenderMeshGroup(rootMeshGroup, apGL.RENDER_TYPE.Default, apGL.RENDER_TYPE.Default, null, null, true, _boneGUIRenderMode);
							}
						}

					}
					break;

				case apSelection.SELECTION_TYPE.Param:
					if (_selection.Param != null)
					{
						apGL.DrawBox(Vector2.zero, 200, 100, Color.cyan, true);
						apGL.DrawText("Param Edit", new Vector2(-30, 30), 70, Color.yellow);
						apGL.DrawText(_selection.Param._keyName, new Vector2(-30, -15), 200, Color.yellow);
					}
					break;

				case apSelection.SELECTION_TYPE.ImageRes:
					if (_selection.TextureData != null)
					{
						apGL.DrawTexture(_selection.TextureData._image,
											Vector2.zero,
											_selection.TextureData._width,
											_selection.TextureData._height,
											new Color(0.5f, 0.5f, 0.5f, 1.0f));
					}
					break;

				case apSelection.SELECTION_TYPE.Mesh:
					if (_selection.Mesh != null)
					{
						apGL.RENDER_TYPE renderType = apGL.RENDER_TYPE.Default;

						bool isEdgeExpectRender = false;
						switch (_meshEditMode)
						{
							case MESH_EDIT_MODE.Setting:
								renderType |= apGL.RENDER_TYPE.Outlines;
								break;

							case MESH_EDIT_MODE.Modify:
								//Controller.GUI_Input_Modify();
								renderType |= apGL.RENDER_TYPE.Vertex;
								renderType |= apGL.RENDER_TYPE.AllEdges;
								renderType |= apGL.RENDER_TYPE.ShadeAllMesh;
								if(_meshEditZDepthView == MESH_EDIT_RENDER_MODE.ZDepth)
								{
									renderType |= apGL.RENDER_TYPE.VolumeWeightColor;
								}
								break;

							case MESH_EDIT_MODE.MakeMesh:
								//case MESH_EDIT_MODE.AddVertex:
								//Controller.GUI_Input_AddVertex();
								if (_meshEditeMode_MakeMesh == MESH_EDIT_MODE_MAKEMESH.Polygon)
								{
									renderType |= apGL.RENDER_TYPE.Vertex;
									renderType |= apGL.RENDER_TYPE.AllEdges;
									renderType |= apGL.RENDER_TYPE.ShadeAllMesh;
									renderType |= apGL.RENDER_TYPE.PolygonOutline;
								}
								else
								{
									renderType |= apGL.RENDER_TYPE.Vertex;
									renderType |= apGL.RENDER_TYPE.AllEdges;
									renderType |= apGL.RENDER_TYPE.ShadeAllMesh;
									if (_meshEditeMode_MakeMesh == MESH_EDIT_MODE_MAKEMESH.VertexAndEdge ||
										_meshEditeMode_MakeMesh == MESH_EDIT_MODE_MAKEMESH.EdgeOnly)
									{
										isEdgeExpectRender = true;
									}
								}
								break;

							case MESH_EDIT_MODE.PivotEdit:
								//Controller.GUI_Input_PivotEdit();
								renderType |= apGL.RENDER_TYPE.Vertex;
								renderType |= apGL.RENDER_TYPE.Outlines;
								break;

						}




						apGL.DrawMesh(_selection.Mesh,
								apMatrix3x3.identity,
								new Color(0.5f, 0.5f, 0.5f, 1.0f),
								renderType,
								VertController, this,
								apMouse.Pos);


						if(_meshEditMode == MESH_EDIT_MODE.PivotEdit)
						{
							//가운데 십자선
							apGL.DrawBoldLine(new Vector2(0, -10), new Vector2(0, 10), 5, new Color(0.3f, 0.7f, 1.0f, 0.7f), true);
							apGL.DrawBoldLine(new Vector2(-10, 0), new Vector2(10, 0), 5, new Color(0.3f, 0.7f, 1.0f, 0.7f), true);
						}

						if (isEdgeExpectRender && VertController.IsEdgeWireRenderable())
						{
							//마우스 위치에 맞게 Connect를 예측할 수 있게 만들자

							apGL.DrawMeshWorkEdgeWire(_selection.Mesh, apMatrix3x3.identity, VertController, VertController.IsEdgeWireCross(), VertController.IsEdgeWireMultipleCross());
						}

					}
					break;

				case apSelection.SELECTION_TYPE.MeshGroup:
					{
						apGL.RENDER_TYPE selectRenderType = apGL.RENDER_TYPE.Default;
						apGL.RENDER_TYPE meshRenderType = apGL.RENDER_TYPE.Default;

						switch (_meshGroupEditMode)
						{
							case MESHGROUP_EDIT_MODE.Setting:
								//Controller.GUI_Input_MeshGroup_Setting();
								selectRenderType |= apGL.RENDER_TYPE.Outlines;

								if (Select.IsMeshGroupSettingChangePivot)
								{
									selectRenderType |= apGL.RENDER_TYPE.TransformBorderLine;
								}

								break;
							case MESHGROUP_EDIT_MODE.Bone:
								//Controller.GUI_Input_MeshGroup_Bone();
								selectRenderType |= apGL.RENDER_TYPE.Vertex;
								selectRenderType |= apGL.RENDER_TYPE.AllEdges;
								break;
							case MESHGROUP_EDIT_MODE.Modifier:
								{
									apModifierBase.MODIFIER_TYPE modifierType = apModifierBase.MODIFIER_TYPE.Base;
									if (Select.Modifier != null)
									{
										modifierType = Select.Modifier.ModifierType;
										switch (Select.Modifier.ModifierType)
										{
											case apModifierBase.MODIFIER_TYPE.Volume:
												selectRenderType |= apGL.RENDER_TYPE.Vertex;
												selectRenderType |= apGL.RENDER_TYPE.Outlines;
												break;

											case apModifierBase.MODIFIER_TYPE.Morph:
												selectRenderType |= apGL.RENDER_TYPE.Vertex;
												selectRenderType |= apGL.RENDER_TYPE.AllEdges;
												break;

											case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
												selectRenderType |= apGL.RENDER_TYPE.Vertex;
												selectRenderType |= apGL.RENDER_TYPE.AllEdges;
												break;

											case apModifierBase.MODIFIER_TYPE.Rigging:
												selectRenderType |= apGL.RENDER_TYPE.Vertex;
												selectRenderType |= apGL.RENDER_TYPE.AllEdges;
												selectRenderType |= apGL.RENDER_TYPE.BoneRigWeightColor;

												meshRenderType |= apGL.RENDER_TYPE.BoneRigWeightColor;
												break;

											case apModifierBase.MODIFIER_TYPE.Physic:
												selectRenderType |= apGL.RENDER_TYPE.Vertex;
												selectRenderType |= apGL.RENDER_TYPE.PhysicsWeightColor;
												selectRenderType |= apGL.RENDER_TYPE.AllEdges;

												meshRenderType |= apGL.RENDER_TYPE.PhysicsWeightColor;
												break;

											case apModifierBase.MODIFIER_TYPE.TF:
												selectRenderType |= apGL.RENDER_TYPE.Vertex;
												selectRenderType |= apGL.RENDER_TYPE.AllEdges;
												selectRenderType |= apGL.RENDER_TYPE.TransformBorderLine;
												break;

											case apModifierBase.MODIFIER_TYPE.AnimatedTF:
												selectRenderType |= apGL.RENDER_TYPE.Vertex;
												selectRenderType |= apGL.RENDER_TYPE.AllEdges;
												selectRenderType |= apGL.RENDER_TYPE.TransformBorderLine;
												break;

											case apModifierBase.MODIFIER_TYPE.FFD:
												selectRenderType |= apGL.RENDER_TYPE.Vertex;
												selectRenderType |= apGL.RENDER_TYPE.AllEdges;
												break;

											case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
												selectRenderType |= apGL.RENDER_TYPE.Vertex;
												selectRenderType |= apGL.RENDER_TYPE.AllEdges;
												break;



											case apModifierBase.MODIFIER_TYPE.Base:
												selectRenderType |= apGL.RENDER_TYPE.Outlines;
												break;
										}
									}
									else
									{
										selectRenderType |= apGL.RENDER_TYPE.Outlines;
									}
									//Controller.GUI_Input_MeshGroup_Modifier(modifierType);

								}
								break;
						}

						if (Select.MeshGroup != null)
						{
#region [미사용 코드] 함수로 바꾸어서 정리했다.
							////Profiler.BeginSample("MeshGroup Update");
							//////1. Render Unit을 돌면서 렌더링을 한다.
							////Select.MeshGroup.Update(DeltaFrameTime);
							////Profiler.EndSample();


							//Profiler.BeginSample("MeshGroup Render");
							//List<apRenderUnit> selectedRenderUnits = new List<apRenderUnit>();
							//for (int iUnit = 0; iUnit < Select.MeshGroup._renderUnits_All.Count; iUnit++)
							//{
							//	apRenderUnit renderUnit = Select.MeshGroup._renderUnits_All[iUnit];
							//	if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
							//	{
							//		if (renderUnit._meshTransform != null)
							//		{
							//			if(!renderUnit._meshTransform._isVisible_Default)
							//			{
							//				continue;
							//			}

							//			if (renderUnit._meshTransform._isClipping_Parent)
							//			{
							//				Profiler.BeginSample("Render - Mask Unit");
							//				//Clipping이면 Child (최대3)까지 마스크 상태로 출력한다.
							//				apGL.DrawRenderUnit_ClippingParent(	renderUnit, 
							//													renderUnit._meshTransform._clipChildMeshTransforms, 
							//													renderUnit._meshTransform._clipChildRenderUnits,
							//													VertController, 
							//													Select);
							//				Profiler.EndSample();
							//			}
							//			else if (renderUnit._meshTransform._isClipping_Child)
							//			{
							//				//렌더링은 생략한다.
							//			}
							//			else
							//			{
							//				Profiler.BeginSample("Render - Normal Unit");
							//				apGL.DrawRenderUnit(renderUnit, apGL.RENDER_TYPE.Default, VertController, Select);
							//				Profiler.EndSample();
							//			}

							//			if (Select.SubMeshInGroup != null)
							//			{
							//				if (Select.SubMeshInGroup == renderUnit._meshTransform)
							//				{
							//					//현재 선택중인 메시다.
							//					selectedRenderUnits.Add(renderUnit);
							//				}
							//			}
							//			else if(Select.SubMeshGroupInGroup != null && Select.SubMeshGroupInGroup._meshGroup != null)
							//			{

							//				if(Select.SubMeshGroupInGroup._meshGroup == renderUnit._meshGroup)
							//				{
							//					selectedRenderUnits.Add(renderUnit);
							//				}
							//			}
							//		}
							//	}
							//}

							//if(selectedRenderUnits.Count > 0)
							//{
							//	Profiler.BeginSample("Render - Selected Unit");
							//	for (int i = 0; i < selectedRenderUnits.Count; i++)
							//	{
							//		apGL.DrawRenderUnit(selectedRenderUnits[i], renderType, VertController, Select);
							//	}
							//	Profiler.EndSample();

							//}
							//Profiler.EndSample(); 
#endregion

							_tmpSelectedMeshTransform.Clear();
							_tmpSelectedMeshGroupTransform.Clear();

							if (Select.SubMeshInGroup != null)
							{
								_tmpSelectedMeshTransform.Add(Select.SubMeshInGroup);
							}

							if (Select.SubMeshGroupInGroup != null)
							{
								_tmpSelectedMeshGroupTransform.Add(Select.SubMeshGroupInGroup);
							}



							RenderMeshGroup(Select.MeshGroup,
								meshRenderType,
								selectRenderType,
								_tmpSelectedMeshTransform,
								_tmpSelectedMeshGroupTransform,
								false,
								_boneGUIRenderMode);


							// Onion Render 처리를 한다.
							if(Onion.IsVisible && Onion.IsRecorded && Event.current.type == EventType.Repaint)
							{
								RenderOnion(Select.MeshGroup);

								//다시 업데이트
								Select.MeshGroup.UpdateRenderUnits(0.0f, true);
							}


							//추가:3.22
							//ExMod 편집중인 경우
							//Bone Preview를 할 수도 있다.
							if(Select.ExEditingMode != apSelection.EX_EDIT.None)
							{
								if(Select.Modifier != null)
								{
									if (Select.Modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
									{
										//리깅은 안된다.
										bool isBonePreivew = GetModLockOption_BonePreview(Select.ExEditingMode);
										if (isBonePreivew)
										{
											//Bone Preview를 하자
											RenderExEditBonePreview_Modifier(Select.MeshGroup, GetModLockOption_BonePreviewColor());
										}
									}
								}
							}



							//Bone Edit 중이라면
							if (Controller.IsBoneEditGhostBoneDraw)
							{
								apGL.DrawBoldLine(Controller.BoneEditGhostBonePosW_Start,
													Controller.BoneEditGhostBonePosW_End,
													7, new Color(1.0f, 0.0f, 0.5f, 0.8f), true
													);
							}
						}

						if (_meshGroupEditMode == MESHGROUP_EDIT_MODE.Modifier)
						{
							if (GetModLockOption_ListUI(Select.ExEditingMode))
							{
								//추가 3.22
								//현재 상태에 대해서 ListUI를 출력하자
								DrawModifierListUI(0, height / 2, Select.MeshGroup, Select.Modifier, null, null, Select.ExEditingMode);
							}
						}
					}

					if (Select.ExEditingMode != apSelection.EX_EDIT.None)
					{
						//Profiler.BeginSample("MeshGroup Borderline");

						//Editing이라면
						//화면 위/아래에 붉은 라인들 그려주자
						apGL.DrawEditingBorderline();

						//Profiler.EndSample();
					}

					if (Gizmos.IsBlurMode)
					{
						Controller.GUI_PrintBrushCursor(Gizmos.BlurRadius, apGL.GetWeightColor(Gizmos.BlurIntensity / 100.0f));
					}
					break;

				case apSelection.SELECTION_TYPE.Animation:
					{
						if (Select.AnimClip != null && Select.AnimClip._targetMeshGroup != null)
						{
							apGL.RENDER_TYPE renderType = apGL.RENDER_TYPE.Default;


							bool isVertexRender = false;
							if (Select.AnimTimeline != null)
							{
								if (Select.AnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier &&
									Select.AnimTimeline._linkedModifier != null)
								{
									if ((int)(Select.AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
									{
										//VertexPos 제어하는 모디파이어와 연결시
										//Vertex를 보여주자
										isVertexRender = true;

									}
								}
							}
							if (isVertexRender)
							{
								renderType |= apGL.RENDER_TYPE.Vertex;
								renderType |= apGL.RENDER_TYPE.AllEdges;
							}
							else
							{
								renderType |= apGL.RENDER_TYPE.Outlines;
							}


							_tmpSelectedMeshTransform.Clear();
							_tmpSelectedMeshGroupTransform.Clear();

							//TODO : 수정중인 Timeline의 종류에 따라 다르게 표시하자
							//TODO : 작업 중일때.. + 현재 Timeline의 종류에 따라..
							if (Select.SubMeshTransformOnAnimClip != null)
							{
								_tmpSelectedMeshTransform.Add(Select.SubMeshTransformOnAnimClip);
							}

							if (Select.SubMeshGroupTransformOnAnimClip != null)
							{
								_tmpSelectedMeshGroupTransform.Add(Select.SubMeshGroupTransformOnAnimClip);
							}

							RenderMeshGroup(Select.AnimClip._targetMeshGroup,
												apGL.RENDER_TYPE.Default,
												renderType,
												_tmpSelectedMeshTransform,
												_tmpSelectedMeshGroupTransform,
												false,
												_boneGUIRenderMode);

							// Onion Render 처리를 한다. + 재생중이 아닐때
							if(Onion.IsVisible && Onion.IsRecorded && !Select.AnimClip.IsPlaying_Editor && Event.current.type == EventType.Repaint)
							{
								RenderOnion(Select.AnimClip._targetMeshGroup);

								Select.AnimClip.Update_Editor(0.0f, true);
							}


							//추가:3.22
							//ExMod 편집중인 경우
							//Bone Preview를 할 수도 있다.
							if(Select.ExAnimEditingMode != apSelection.EX_EDIT.None)
							{
								if (Select.AnimClip != null && Select.AnimClip._targetMeshGroup != null)
								{
									//리깅은 안된다.
									bool isBonePreivew = GetModLockOption_BonePreview(Select.ExAnimEditingMode);
									if (isBonePreivew)
									{
										//Bone Preview를 하자
										RenderExEditBonePreview_Animation(Select.AnimClip, Select.AnimClip._targetMeshGroup, GetModLockOption_BonePreviewColor());

										Select.AnimClip.Update_Editor(0.0f, true);
									}
								}
							}
						}

						if(GetModLockOption_ListUI(Select.ExAnimEditingMode))
						{
							//추가 3.22
							//현재 상태에 대해서 ListUI를 출력하자
							DrawModifierListUI(0, height / 2, Select.AnimClip._targetMeshGroup, null, Select.AnimClip, Select.AnimTimeline, Select.ExAnimEditingMode);
						}

						//화면 위/아래 붉은 라인을 그려주자
						if (Select.ExAnimEditingMode != apSelection.EX_EDIT.None)
						{
							apGL.DrawEditingBorderline();
						}

						if (Gizmos.IsBlurMode)
						{
							Controller.GUI_PrintBrushCursor(Gizmos.BlurRadius, apGL.GetWeightColor(Gizmos.BlurIntensity / 100.0f));
						}

						
					}
					break;
			}

			//현재는 윈도우만 지원
			if (_isShowCaptureFrame && _portrait != null && Select.SelectionType == apSelection.SELECTION_TYPE.Overall)
			{

				Vector2 framePos_Center = new Vector2(_captureFrame_PosX + apGL.WindowSizeHalf.x, _captureFrame_PosY + apGL.WindowSizeHalf.y);
				Vector2 frameHalfSize = new Vector2(_captureFrame_SrcWidth / 2, _captureFrame_SrcHeight / 2);
				Vector2 framePos_LT = framePos_Center + new Vector2(-frameHalfSize.x, -frameHalfSize.y);
				Vector2 framePos_RT = framePos_Center + new Vector2(frameHalfSize.x, -frameHalfSize.y);
				Vector2 framePos_LB = framePos_Center + new Vector2(-frameHalfSize.x, frameHalfSize.y);
				Vector2 framePos_RB = framePos_Center + new Vector2(frameHalfSize.x, frameHalfSize.y);
				Color frameColor = new Color(0.3f, 1.0f, 1.0f, 0.7f);

				apGL.BeginBatch_ColoredLine();
				apGL.DrawLineGL(framePos_LT, framePos_RT, frameColor, false);
				apGL.DrawLineGL(framePos_RT, framePos_RB, frameColor, false);
				apGL.DrawLineGL(framePos_RB, framePos_LB, frameColor, false);
				apGL.DrawLineGL(framePos_LB, framePos_LT, frameColor, false);

				//<<< 추가 >>>
				//추가 : 썸네일 프레임을 만들자
				float preferAspectRatio = 2.0f; //256 x 128
				float srcAspectRatio = (float)_captureFrame_SrcWidth / (float)_captureFrame_SrcHeight;

				//긴쪽으로 캡쳐 크기를 맞춘다.
				int srcThumbWidth = _captureFrame_SrcWidth;
				int srcThumbHeight = _captureFrame_SrcHeight;

				//AspectRatio = W / H
				if (srcAspectRatio < preferAspectRatio)
				{
					srcThumbHeight = (int)((srcThumbWidth / preferAspectRatio) + 0.5f);
				}
				else
				{
					srcThumbWidth = (int)((srcThumbHeight * preferAspectRatio) + 0.5f);
				}
				srcThumbWidth /= 2;
				srcThumbHeight /= 2;

				
				
				Vector2 thumbFramePos_LT = framePos_Center + new Vector2(-srcThumbWidth, -srcThumbHeight);
				Vector2 thumbFramePos_RT = framePos_Center + new Vector2(srcThumbWidth, -srcThumbHeight);
				Vector2 thumbFramePos_LB = framePos_Center + new Vector2(-srcThumbWidth, srcThumbHeight);
				Vector2 thumbFramePos_RB = framePos_Center + new Vector2(srcThumbWidth, srcThumbHeight);

				Color thumbFrameColor = new Color(1.0f, 1.0f, 0.0f, 0.4f);
				apGL.DrawLineGL(thumbFramePos_LT, thumbFramePos_RT, thumbFrameColor, false);
				apGL.DrawLineGL(thumbFramePos_RT, thumbFramePos_RB, thumbFrameColor, false);
				apGL.DrawLineGL(thumbFramePos_RB, thumbFramePos_LB, thumbFrameColor, false);
				apGL.DrawLineGL(thumbFramePos_LB, thumbFramePos_LT, thumbFrameColor, false);
				apGL.EndBatch();
			}

			if (_guiOption_isFPSVisible)
			{
				apGL.DrawTextGL("FPS " + FPS, new Vector2(10, 20), 150, Color.yellow);
			}

			if (_isNotification_GUI)
			{
				Color notiColor = Color.yellow;
				if (_tNotification_GUI < 1.0f)
				{
					notiColor.a = _tNotification_GUI;
				}
				apGL.DrawTextGL(_strNotification_GUI, new Vector2(10, 35), 400, notiColor);
			}
			if(Backup.IsAutoSaveWorking() && _isBackupProcessing)
			{
				//자동 저장 중일때
				int iconSize = 28;
				int yPos = 35 + 8 + iconSize / 2;
				if(_isNotification_GUI)
				{
					yPos += 15;
				}

				
				Color labelColor = Color.yellow;
				float alpha = Mathf.Sin((_tBackupProcessing_Label / BACKUP_LABEL_TIME_LENGTH) * Mathf.PI * 2.0f);
				//-1 ~ 1
				//-0.2 ~ 0.2 (x0.2)
				//0.6 ~ 1(+0.8)
				alpha = (alpha * 0.2f) + 0.8f;
				labelColor.a = alpha;
				

				if(_imgBackupIcon_Frame1 == null || _imgBackupIcon_Frame2 == null)
				{
					_imgBackupIcon_Frame1 = ImageSet.Get(apImageSet.PRESET.AutoSave_Frame1);
					_imgBackupIcon_Frame2 = ImageSet.Get(apImageSet.PRESET.AutoSave_Frame2);
				}

				float scaledIconSize = (float)iconSize / apGL.Zoom;
				if(_tBackupProcessing_Icon < BACKUP_ICON_TIME_LENGTH * 0.5f)
				{
					apGL.DrawTextureGL(_imgBackupIcon_Frame1, new Vector2(10 + iconSize / 2, yPos), scaledIconSize, scaledIconSize, Color.gray, 0.0f);
				}
				else
				{
					apGL.DrawTextureGL(_imgBackupIcon_Frame2, new Vector2(10 + iconSize / 2, yPos), scaledIconSize, scaledIconSize, Color.gray, 0.0f);
				}
				apGL.DrawTextGL(Backup.Label, new Vector2(10 + iconSize + 10, yPos - 6), 400, labelColor);
			}
			if(Select.IsSelectionLockGUI)
			{
				float scaledIconSize = (float)28 / apGL.Zoom;
				apGL.DrawTextureGL(ImageSet.Get(apImageSet.PRESET.GUI_SelectionLock), new Vector2(width - 20, 30), scaledIconSize, scaledIconSize, Color.gray, 0.0f);
			}


			if(_guiOption_isStatisticsVisible)
			{
				//통계 정보를 출력한다.
				//통계 정보는 아래서 출력한다.
				//어떤 메뉴인가에 따라 다르다
				Select.CalculateStatistics();
				if(Select.IsStatisticsCalculated)
				{
					int posY = height - 30;
					if(Select.Statistics_NumKeyframe >= 0)
					{
						apGL.DrawTextGL("Keyframes", new Vector2(10, posY), 120, Color.yellow);
						apGL.DrawTextGL(Select.Statistics_NumKeyframe.ToString(), new Vector2(120, posY), 150, Color.yellow);
						posY -= 15;
					}
					if(Select.Statistics_NumTimelineLayer >= 0)
					{
						apGL.DrawTextGL("Timeline Layers", new Vector2(10, posY), 120, Color.yellow);
						apGL.DrawTextGL(Select.Statistics_NumTimelineLayer.ToString(), new Vector2(120, posY), 150, Color.yellow);
						posY -= 15;
					}
					if(Select.Statistics_NumClippedMesh >= 0)
					{
						apGL.DrawTextGL("Clipped Vertices", new Vector2(10, posY), 120, Color.yellow);
						apGL.DrawTextGL(Select.Statistics_NumClippedVertex.ToString(), new Vector2(120, posY), 150, Color.yellow);
						posY -= 15;

						apGL.DrawTextGL("Clipped Meshes", new Vector2(10, posY), 120, Color.yellow);
						apGL.DrawTextGL(Select.Statistics_NumClippedMesh.ToString(), new Vector2(120, posY), 150, Color.yellow);
						posY -= 15;
					}

					apGL.DrawTextGL("Triangles", new Vector2(10, posY), 120, Color.yellow);
					apGL.DrawTextGL(Select.Statistics_NumTri.ToString(), new Vector2(120, posY), 150, Color.yellow);
					posY -= 15;

					apGL.DrawTextGL("Edges", new Vector2(10, posY), 120, Color.yellow);
					apGL.DrawTextGL(Select.Statistics_NumEdge.ToString(), new Vector2(120, posY), 150, Color.yellow);
					posY -= 15;

					apGL.DrawTextGL("Vertices", new Vector2(10, posY), 120, Color.yellow);
					apGL.DrawTextGL(Select.Statistics_NumVertex.ToString(), new Vector2(120, posY), 150, Color.yellow);
					posY -= 15;

					if (Select.Statistics_NumMesh >= 0)
					{
						apGL.DrawTextGL("Meshes", new Vector2(10, posY), 120, Color.yellow);
						apGL.DrawTextGL(Select.Statistics_NumMesh.ToString(), new Vector2(120, posY), 150, Color.yellow);
						posY -= 15;
					}

					posY -= 5;
					apGL.DrawTextGL("Statistics", new Vector2(10, posY), 120, Color.yellow);
				}
			}

			//화면 캡쳐 이벤트
#if UNITY_EDITOR_OSX
			if(_isScreenCaptureRequest_OSXReady)
			{
				_screenCaptureRequest_Count--;
				if(_screenCaptureRequest_Count < 0)
				{
					_isScreenCaptureRequest = true;
					_isScreenCaptureRequest_OSXReady = false;
					_screenCaptureRequest_Count = 0;
				}
			}
#endif
			if(_isScreenCaptureRequest)
			{
				_isScreenCaptureRequest = false;
#if UNITY_EDITOR_OSX
				_isScreenCaptureRequest_OSXReady = false;
				_screenCaptureRequest_Count = 0;
#endif
				
				ProcessScreenCapture();
				SetRepaint();
			}
		}

		private List<apTransform_Mesh> _tmpSelectedMeshTransform = new List<apTransform_Mesh>();
		private List<apTransform_MeshGroup> _tmpSelectedMeshGroupTransform = new List<apTransform_MeshGroup>();
		private List<apRenderUnit> _tmpSelectedRenderUnits = new List<apRenderUnit>();

		/// <summary>
		/// MeshGroup을 렌더링한다.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="selectedRenderType"></param>
		/// <param name="selectedMeshes"></param>
		/// <param name="selectedMeshGroups"></param>
		/// <param name="isRenderOnlyVisible">단순 재생이면 True, 작업 중이면 False (Alpha가 0인 것도 일단 렌더링을 한다)</param>
		private void RenderMeshGroup(apMeshGroup meshGroup,
										apGL.RENDER_TYPE meshRenderType,
										apGL.RENDER_TYPE selectedRenderType,
										List<apTransform_Mesh> selectedMeshes,
										List<apTransform_MeshGroup> selectedMeshGroups,
										bool isRenderOnlyVisible,
										apEditor.BONE_RENDER_MODE boneRenderMode)
		{
			//Profiler.BeginSample("MeshGroup Render");

			_tmpSelectedRenderUnits.Clear();

			//선택된 렌더유닛을 먼저 선정. 그 후에 다시 렌더링하자
			for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			{
				apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						//if(!renderUnit._meshTransform._isVisible_Default)
						//{
						//	continue;
						//}

						if (selectedMeshes != null && selectedMeshes.Count > 0)
						{
							if (selectedMeshes.Contains(renderUnit._meshTransform))
							{
								//현재 선택중인 메시다.
								_tmpSelectedRenderUnits.Add(renderUnit);
							}
						}

						if (selectedMeshGroups != null && selectedMeshGroups.Count > 0)
						{
							if (selectedMeshGroups.Contains(renderUnit._meshGroupTransform))
							{
								_tmpSelectedRenderUnits.Add(renderUnit);
							}
						}
					}
				}
			}

			// Weight 갱신과 Vert Color 연동
			//----------------------------------
			if ((int)(meshRenderType & apGL.RENDER_TYPE.BoneRigWeightColor) != 0)
			{
				//Rig Weight를 집어넣자.
				//bool isBoneColor = Select._rigEdit_isBoneColorView;
				//apSelection.RIGGING_EDIT_VIEW_MODE rigViewMode = Select._rigEdit_viewMode;
				apRenderVertex renderVert = null;
				apModifiedMesh modMesh = Select.ModMeshOfMod;
				apModifiedVertexRig vertRig = null;
				apModifiedVertexRig.WeightPair weightPair = null;
				apBone selelcedBone = Select.Bone;

				Color colorBlack = Color.black;


				apModifierBase modifier = Select.Modifier;
				if (modifier != null)
				{
					if (modifier._paramSetGroup_controller.Count > 0 &&
						modifier._paramSetGroup_controller[0]._paramSetList.Count > 0)
					{
						List<apModifiedMesh> modMeshes = Select.Modifier._paramSetGroup_controller[0]._paramSetList[0]._meshData;
						for (int iMM = 0; iMM < modMeshes.Count; iMM++)
						{
							modMesh = modMeshes[iMM];
							if (modMesh != null)
							{
								modMesh.RefreshVertexRigs(_portrait);

								for (int iRU = 0; iRU < modMesh._renderUnit._renderVerts.Count; iRU++)
								{
									renderVert = modMesh._renderUnit._renderVerts[iRU];
									renderVert._renderColorByTool = colorBlack;
									renderVert._renderWeightByTool = 0.0f;
									renderVert._renderParam = 0;
								}

								for (int iVR = 0; iVR < modMesh._vertRigs.Count; iVR++)
								{
									vertRig = modMesh._vertRigs[iVR];
									if (vertRig._renderVertex != null)
									{
										for (int iWP = 0; iWP < vertRig._weightPairs.Count; iWP++)
										{
											weightPair = vertRig._weightPairs[iWP];
											vertRig._renderVertex._renderColorByTool += weightPair._bone._color * weightPair._weight;

											if (weightPair._bone == selelcedBone)
											{
												vertRig._renderVertex._renderWeightByTool += weightPair._weight;
											}
										}
									}
								}
							}
						}
					}
				}
			}

			//Physic/Volume Color를 집어넣어보자
			if ((int)(meshRenderType & apGL.RENDER_TYPE.PhysicsWeightColor) != 0 ||
				(int)(meshRenderType & apGL.RENDER_TYPE.VolumeWeightColor) != 0)
			{

				//Rig Weight를 집어넣자.
				//bool isBoneColor = Select._rigEdit_isBoneColorView;
				//apSelection.RIGGING_EDIT_VIEW_MODE rigViewMode = Select._rigEdit_viewMode;
				apRenderVertex renderVert = null;
				apModifiedMesh modMesh = Select.ModMeshOfMod;
				apModifiedVertexWeight vertWeight = null;

				Color colorBlack = Color.black;

				apModifierBase modifier = Select.Modifier;

				bool isPhysic = (int)(modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics) != 0;
				//bool isVolume = (int)(modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Volume) != 0;

				if (modifier != null)
				{
					if (modifier._paramSetGroup_controller.Count > 0 &&
						modifier._paramSetGroup_controller[0]._paramSetList.Count > 0)
					{
						List<apModifiedMesh> modMeshes = Select.Modifier._paramSetGroup_controller[0]._paramSetList[0]._meshData;
						for (int iMM = 0; iMM < modMeshes.Count; iMM++)
						{
							modMesh = modMeshes[iMM];
							if (modMesh != null)
							{
								//Refresh를 여기서 하진 말자
								//modMesh.RefreshVertexWeights(_portrait, isPhysic, isVolume);

								for (int iRU = 0; iRU < modMesh._renderUnit._renderVerts.Count; iRU++)
								{
									renderVert = modMesh._renderUnit._renderVerts[iRU];
									renderVert._renderColorByTool = colorBlack;
									renderVert._renderWeightByTool = 0.0f;
									renderVert._renderParam = 0;
								}

								for (int iVR = 0; iVR < modMesh._vertWeights.Count; iVR++)
								{
									vertWeight = modMesh._vertWeights[iVR];
									if (vertWeight._renderVertex != null)
									{
										//그라데이션을 위한 Weight 값을 넣어주자
										vertWeight._renderVertex._renderWeightByTool = vertWeight._weight;

										if (isPhysic)
										{
											if (vertWeight._isEnabled && vertWeight._physicParam._isMain)
											{
												vertWeight._renderVertex._renderParam = 1;//1 : Main
											}
											else if (!vertWeight._isEnabled && vertWeight._physicParam._isConstraint)
											{
												vertWeight._renderVertex._renderParam = 2;//2 : Constraint
											}
										}
									}
								}
							}
						}
					}
				}
			}


			//----------------------------------

			for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			{
				apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						//if(!renderUnit._meshTransform._isVisible_Default)
						//{
						//	continue;
						//}

						if (renderUnit._meshTransform._isClipping_Parent)
						{
							//Profiler.BeginSample("Render - Mask Unit");

							if (!isRenderOnlyVisible || renderUnit._isVisible)
							{
								
								apGL.DrawRenderUnit_ClippingParent_Renew(renderUnit,
																			meshRenderType,
																			renderUnit._meshTransform._clipChildMeshes,
																			//renderUnit._meshTransform._clipChildMeshTransforms,
																			//renderUnit._meshTransform._clipChildRenderUnits,
																			VertController,
																			Select);
							}

							//Profiler.EndSample();
						}
						else if (renderUnit._meshTransform._isClipping_Child)
						{
							//렌더링은 생략한다.
						}
						else
						{
							//Profiler.BeginSample("Render - Normal Unit");

							if (!isRenderOnlyVisible || renderUnit._isVisible)
							{
								//if(_tmpSelectedRenderUnits.Contains(renderUnit))
								apGL.DrawRenderUnit(renderUnit, meshRenderType, VertController, Select, this, apMouse.Pos);

								
							}

							//Profiler.EndSample();
						}

					}
				}
			}






			//Bone을 렌더링하자
			if (boneRenderMode != BONE_RENDER_MODE.None)
			{
				//선택 아웃라인을 밑에 그릴때
				//if(Select.Bone != null)
				//{
				//	apGL.DrawSelectedBone(Select.Bone);
				//}

				bool isDrawBoneOutline = (boneRenderMode == BONE_RENDER_MODE.RenderOutline);
				//Child MeshGroup의 Bone을 먼저 렌더링합니다. (그래야 렌더링시 뒤로 들어감)
				if (meshGroup._childMeshGroupTransformsWithBones.Count > 0)
				{
					for (int iChildMG = 0; iChildMG < meshGroup._childMeshGroupTransformsWithBones.Count; iChildMG++)
					{
						apTransform_MeshGroup meshGroupTransform = meshGroup._childMeshGroupTransformsWithBones[iChildMG];

						for (int iRoot = 0; iRoot < meshGroupTransform._meshGroup._boneList_Root.Count; iRoot++)
						{
							DrawBoneRecursive(meshGroupTransform._meshGroup._boneList_Root[iRoot], isDrawBoneOutline);
						}
					}
				}

				//Bone도 렌더링 합니당
				if (meshGroup._boneList_Root.Count > 0)
				{
					for (int iRoot = 0; iRoot < meshGroup._boneList_Root.Count; iRoot++)
					{
						//Root 렌더링을 한다.
						DrawBoneRecursive(meshGroup._boneList_Root[iRoot], isDrawBoneOutline);
					}
				}

				if (Select.Bone != null)
				{
					//선택 아웃라인을 위에 그릴때
					if (Select.BoneEditMode == apSelection.BONE_EDIT_MODE.Link)
					{
						if (Controller.BoneEditRollOverBone != null)
						{
							apGL.DrawSelectedBone(Controller.BoneEditRollOverBone, false);
						}
					}

					apGL.DrawSelectedBone(Select.Bone);
					if (!isDrawBoneOutline)
					{
						apGL.DrawSelectedBonePost(Select.Bone);
					}
				}


			}

			//선택된 Render Unit을 그려준다. (Vertex 등)
			if (_tmpSelectedRenderUnits.Count > 0)
			{
				//Profiler.BeginSample("Render - Selected Unit");
				for (int i = 0; i < _tmpSelectedRenderUnits.Count; i++)
				{
					apGL.DrawRenderUnit(_tmpSelectedRenderUnits[i], selectedRenderType, VertController, Select, this, apMouse.Pos);
				}
				//Profiler.EndSample();

			}
			//Profiler.EndSample();
		}




		/// <summary>
		/// MeshGroup을 렌더링한다.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="selectedRenderType"></param>
		/// <param name="selectedMeshes"></param>
		/// <param name="selectedMeshGroups"></param>
		/// <param name="isRenderOnlyVisible">단순 재생이면 True, 작업 중이면 False (Alpha가 0인 것도 일단 렌더링을 한다)</param>
		private void RenderBoneOutlineOnly(apMeshGroup meshGroup,
										Color boneLineColor)
		{

			//Bone을 렌더링하자
			//선택 아웃라인을 밑에 그릴때
			//if(Select.Bone != null)
			//{
			//	apGL.DrawSelectedBone(Select.Bone);
			//}

			//Child MeshGroup의 Bone을 먼저 렌더링합니다. (그래야 렌더링시 뒤로 들어감)
			if (meshGroup._childMeshGroupTransformsWithBones.Count > 0)
			{
				for (int iChildMG = 0; iChildMG < meshGroup._childMeshGroupTransformsWithBones.Count; iChildMG++)
				{
					apTransform_MeshGroup meshGroupTransform = meshGroup._childMeshGroupTransformsWithBones[iChildMG];

					for (int iRoot = 0; iRoot < meshGroupTransform._meshGroup._boneList_Root.Count; iRoot++)
					{
						DrawBoneOutlineRecursive(meshGroupTransform._meshGroup._boneList_Root[iRoot], boneLineColor);
					}
				}
			}

			//Bone도 렌더링 합니당
			if (meshGroup._boneList_Root.Count > 0)
			{
				for (int iRoot = 0; iRoot < meshGroup._boneList_Root.Count; iRoot++)
				{
					//Root 렌더링을 한다.
					DrawBoneOutlineRecursive(meshGroup._boneList_Root[iRoot], boneLineColor);
				}
			}


		}

		private void DrawBoneRecursive(apBone targetBone, bool isDrawOutline)
		{
			targetBone.GUIUpdate();
			//apGL.DrawBone(targetBone, _selection);
			
			if(targetBone.IsGUIVisible)
			{
				apGL.DrawBone(targetBone, isDrawOutline);	
			}

			for (int i = 0; i < targetBone._childBones.Count; i++)
			{
				DrawBoneRecursive(targetBone._childBones[i], isDrawOutline);
			}
		}


		private void DrawBoneOutlineRecursive(apBone targetBone, Color boneOutlineColor)
		{
			targetBone.GUIUpdate();
			
			if(targetBone.IsGUIVisible)
			{
				apGL.DrawBoneOutline(targetBone, boneOutlineColor);	
			}

			for (int i = 0; i < targetBone._childBones.Count; i++)
			{
				DrawBoneOutlineRecursive(targetBone._childBones[i], boneOutlineColor);
			}
		}


		private void RenderOnion(apMeshGroup meshGroup)
		{
			if(!Onion.IsVisible || !Onion.IsRecorded)
			{
				return;
			}

			//저장된 값을 적용
			Onion.AdaptRecord(this);

			//렌더링
			bool isPrevPhysics = _portrait._isPhysicsPlay_Editor;
			_portrait._isPhysicsPlay_Editor = false;

			meshGroup.RefreshForce();
			RenderMeshGroup(meshGroup, apGL.RENDER_TYPE.ToneColor, apGL.RENDER_TYPE.Default, null, null, true, BONE_RENDER_MODE.None);

			_portrait._isPhysicsPlay_Editor = isPrevPhysics;

			//원래의 값으로 복구 
			Onion.Recorver(this);
		}


		private void RenderExEditBonePreview_Modifier(apMeshGroup meshGroup, Color boneOutlineColor)
		{
			
			apSelection.EX_EDIT prevExEditMode = Select.ExEditingMode;

			bool isExLockSuccess = Select.SetModifierExclusiveEditing_Tmp(apSelection.EX_EDIT.None);
			if (!isExLockSuccess)
			{
				return;
			}
			Select.RefreshMeshGroupExEditingFlags(meshGroup, null, null, null, true);//<<일단 초기화
			
			meshGroup.RefreshForce();
			RenderBoneOutlineOnly(meshGroup, boneOutlineColor);

			//meshGroup.RefreshForce(false, 0.0f, false);//<ExCalculate를 Ignore한다.

			Select.SetModifierExclusiveEditing_Tmp(prevExEditMode);
			meshGroup.RefreshForce();
			
		}


		private void RenderExEditBonePreview_Animation(apAnimClip animClip, apMeshGroup meshGroup, Color boneOutlineColor)
		{
			if (animClip == null || animClip._targetMeshGroup == null)
			{
				return;
			}

			apSelection.EX_EDIT prevExEditMode = Select.ExAnimEditingMode;

			bool isExLockSuccess = Select.SetAnimExclusiveEditing_Tmp(apSelection.EX_EDIT.None, false);
			if (!isExLockSuccess)
			{
				Debug.Log("Failed");
				return;
			}

			apModifierBase curModifier = null;
			if (Select.AnimTimeline != null)
			{
				curModifier = Select.AnimTimeline._linkedModifier;
			}
			//Debug.Log("Is Modifier Valid : " + (curModifier != null));

			Select.RefreshMeshGroupExEditingFlags(meshGroup, null, null, animClip, true);//<<일단 초기화

			////meshGroup.RefreshForce();//<ExCalculate를 Ignore한다.
			animClip.Update_Editor(0.0f, true);
			RenderBoneOutlineOnly(meshGroup, boneOutlineColor);

			Select.SetAnimExclusiveEditing_Tmp(prevExEditMode, false);
			Select.RefreshMeshGroupExEditingFlags(meshGroup, curModifier, null, animClip, true);//<<일단 초기화

			animClip.Update_Editor(0.0f, true);
			for (int i = 0; i < animClip._targetMeshGroup._boneList_Root.Count; i++)
			{
				animClip._targetMeshGroup._boneList_Root[i].GUIUpdate(true);
			}
			
			//Select.RefreshAnimEditing(true);
			//meshGroup.RefreshForce();//<ExCalculate를 Ignore한다.

			//Select.SetModifierExclusiveEditing(prevExEditMode);
			//meshGroup.RefreshForce();

			
			
		}



		private void DrawModifierListUI(int posX, int posY, 
										apMeshGroup curMeshGroup, apModifierBase curModifier, 
										apAnimClip curAnimClip, apAnimTimeline curAnimTimeline,
										apSelection.EX_EDIT exMode)
		{
			if(curMeshGroup == null)
			{
				return;
			}

			if(curMeshGroup._modifierStack._modifiers.Count == 0)
			{
				return;
			}

			apModifierBase mod = null;
			int imgSize = 16;
			int imgSize_Half = imgSize / 2;
			int textWidth = 130;
			int textHeight = 16;
			int startPosY = posY + (textHeight * curMeshGroup._modifierStack._modifiers.Count) / 2;

			bool isCheckAnim = (curAnimClip != null);

			Texture2D imgCursor = null;
			if(exMode != apSelection.EX_EDIT.ExOnly_Edit)
			{
				imgCursor = ImageSet.Get(apImageSet.PRESET.SmallMod_CursorUnlocked);
			}
			else
			{
				imgCursor = ImageSet.Get(apImageSet.PRESET.SmallMod_CursorLocked);
			}

			
			int curPosY = 0;
			int posX_Cursor = posX + imgSize_Half;
			int posX_Icon = posX_Cursor + imgSize;
			int posX_Title = posX_Icon + imgSize;
			int posX_ExEnabled = posX_Title + 5 + textWidth;
			int posX_ColorEnabled = posX_ExEnabled + 5 + imgSize;
			Color colorGray = Color.gray;

			Color color_Selected = Color.yellow;
			Color color_NotSelected = new Color(0.8f, 0.8f, 0.8f, 1.0f);
			bool isSelected = false;

			Texture2D imgMod = null;
			Texture2D imgEx = null;
			Texture2D imgColor = null;

			float imgSize_Zoom = (float)imgSize / apGL.Zoom;
			for (int i = 0; i < curMeshGroup._modifierStack._modifiers.Count; i++)
			{
				mod = curMeshGroup._modifierStack._modifiers[i];
				curPosY = startPosY - (i * textHeight);

				isSelected = false;
				if(isCheckAnim)
				{
					if(curAnimTimeline != null && mod == curAnimTimeline._linkedModifier)
					{
						isSelected = true;
					}
				}
				else
				{
					if(curModifier == mod)
					{
						isSelected = true;
						
					}
				}
				if(isSelected)
				{
					apGL.DrawTextureGL(imgCursor, new Vector2(posX_Cursor, curPosY), imgSize_Zoom, imgSize_Zoom, colorGray, 0);
				}
				imgMod = ImageSet.Get(apEditorUtil.GetSmallModIconType(mod.ModifierType));
				apGL.DrawTextureGL(imgMod, new Vector2(posX_Icon, curPosY), imgSize_Zoom, imgSize_Zoom, colorGray, 0);
				if (isSelected)
				{
					apGL.DrawTextGL(mod.DisplayNameShort, new Vector2(posX_Title, curPosY - imgSize_Half), textWidth, color_Selected);
				}
				else
				{
					apGL.DrawTextGL(mod.DisplayNameShort, new Vector2(posX_Title, curPosY - imgSize_Half), textWidth, color_NotSelected);
				}

				switch (mod._editorExclusiveActiveMod)
				{
					case apModifierBase.MOD_EDITOR_ACTIVE.Disabled:
					case apModifierBase.MOD_EDITOR_ACTIVE.OnlyColorEnabled:
						imgEx = ImageSet.Get(apImageSet.PRESET.SmallMod_ExDisabled);
						break;

					case apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled:
					case apModifierBase.MOD_EDITOR_ACTIVE.Enabled:
						imgEx = ImageSet.Get(apImageSet.PRESET.SmallMod_ExEnabled);
						break;

					case apModifierBase.MOD_EDITOR_ACTIVE.SubExEnabled:
						imgEx = ImageSet.Get(apImageSet.PRESET.SmallMod_ExSubEnabled);
						break;

					default:
						imgEx = null;
						break;
				}

				if(imgEx != null)
				{
					apGL.DrawTextureGL(imgEx, new Vector2(posX_ExEnabled, curPosY), imgSize_Zoom, imgSize_Zoom, colorGray, 0);
				}

				if (mod._isColorPropertyEnabled &&
					(int)(mod.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
				{
					if (mod._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled)
					{
						imgColor = ImageSet.Get(apImageSet.PRESET.SmallMod_ColorDisabled);
					}
					else
					{
						imgColor = ImageSet.Get(apImageSet.PRESET.SmallMod_ColorEnabled);
					}
					apGL.DrawTextureGL(imgColor, new Vector2(posX_ColorEnabled, curPosY), imgSize_Zoom, imgSize_Zoom, colorGray, 0);
				}				



				curPosY -= textHeight;
			}
		}



		private void GUI_MainRight(int width, int height)
		{
			if (_portrait == null)
			{
				return;
			}
			if (!_selection.DrawEditor(width - 2, height))
			{
				if (_portrait != null)
				{
					_selection.SetPortrait(_portrait);

					//시작은 RootUnit
					_selection.SetOverallDefault();

					OnAnyObjectAddedOrRemoved();
				}
				else
				{
					_selection.Clear();
				}

				_hierarchy.ResetAllUnits();
				_hierarchy_MeshGroup.ResetSubUnits();
				_hierarchy_AnimClip.ResetSubUnits();
			}
		}

		private void GUI_MainRight_Header(int width, int height)
		{
			if (_portrait == null)
			{
				return;
			}

			_selection.DrawEditor_Header(width - 2, height);

		}


		private void GUI_MainRight_Lower_MeshGroupHeader(int width, int height, bool is2LineLayout)
		{
			width -= 2;
			if (_portrait == null)
			{
				return;
			}
			//1. 타이틀 + Show/Hide
			//수정) 타이틀이 한개가 아니라 Meshes / Bones로 나뉘며 버튼이 된다.
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(5);

			
			bool isChildMesh = Select._meshGroupChildHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes;
			bool isBones = Select._meshGroupChildHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.Bones;

			int toggleBtnWidth = ((width - (25 + 2)) / 2) - 2;
			//GUILayout.Box("Layer", guiStyle, GUILayout.Width(width - (25 + 2)), GUILayout.Height(20));
			if (apEditorUtil.ToggledButton(GetUIWord(UIWORD.Meshes), isChildMesh, toggleBtnWidth, 20))//"Meshes"
			{
				Select._meshGroupChildHierarchy = apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes;
			}
			if (apEditorUtil.ToggledButton(GetUIWord(UIWORD.Bones), isBones, toggleBtnWidth, 20))//"Bones"
			{
				Select._meshGroupChildHierarchy = apSelection.MESHGROUP_CHILD_HIERARCHY.Bones;
			}

			//GUI.backgroundColor = prevColor;

			bool isOpened = (_rightLowerLayout == RIGHT_LOWER_LAYOUT.ChildList);
			Texture2D btnImage = null;
			if (isOpened)
			{ btnImage = ImageSet.Get(apImageSet.PRESET.Hierarchy_OpenLayout); }
			else
			{ btnImage = ImageSet.Get(apImageSet.PRESET.Hierarchy_HideLayout); }

			GUIStyle guiStyle_Btn = new GUIStyle(GUI.skin.label);
			if (GUILayout.Button(btnImage, guiStyle_Btn, GUILayout.Width(25), GUILayout.Height(25)))
			{
				if (_rightLowerLayout == RIGHT_LOWER_LAYOUT.ChildList)
				{
					_rightLowerLayout = RIGHT_LOWER_LAYOUT.Hide;
				}
				else
				{
					_rightLowerLayout = RIGHT_LOWER_LAYOUT.ChildList;
				}
			}

			EditorGUILayout.EndHorizontal();

			//2. 카테고리
			if (is2LineLayout)
			{
				int btnSize = Mathf.Min(height - (25 + 4), (width / 5) - 5);
				//현재 레이어에 대한 버튼들을 출력한다.
				//1) 추가
				//2) 클리핑 (On/Off)
				//3, 4) 레이어 Up/Down
				//5) 삭제
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height - (25 + 4)));
				GUILayout.Space(5);

				apTransform_Mesh selectedMesh = Select.SubMeshInGroup;
				apTransform_MeshGroup selectedMeshGroup = Select.SubMeshGroupInGroup;
				apBone selectedBone = Select.Bone;

				int selectedType = 0;
				apMeshGroup curMeshGroup = Select.MeshGroup;
				if (curMeshGroup != null)
				{
					if (selectedMesh != null) { selectedType = 1; }
					else if (selectedMeshGroup != null) { selectedType = 2; }
					else if(selectedBone != null) { selectedType = 3; }
				}

				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_AddTransform), false, true, btnSize, btnSize, "Add Sub Mesh / Mesh Group"))
				{
					//1) 추가
					//Debug.LogError("TODO : Add Transform");
					_loadKey_AddChildTransform = apDialog_AddChildTransform.ShowDialog(this, curMeshGroup, OnAddChildTransformDialogResult);
				}

				bool isClipped = false;
				if (selectedType == 1)
				{
					isClipped = selectedMesh._isClipping_Child;
				}
				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_SetClipping), isClipped, selectedType == 1, btnSize, btnSize, "Set Clipping Mask"))
				{
					//2) 클리핑
					if (selectedType == 1)
					{
						if (selectedMesh._isClipping_Child)
						{
							//Clip -> Release
							Controller.ReleaseClippingMeshTransform(curMeshGroup, selectedMesh);
						}
						else
						{
							//Release -> Clip
							Controller.AddClippingMeshTransform(curMeshGroup, selectedMesh, true);
						}
					}
				}

				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Modifier_LayerUp), false, selectedType != 0, btnSize, btnSize, "Layer Up"))
				{
					//3) Layer Up
					if (selectedType == 1 || selectedType == 2)
					{
						apRenderUnit targetRenderUnit = null;
						if (selectedType == 1)		{ targetRenderUnit = curMeshGroup.GetRenderUnit(selectedMesh); }
						else if (selectedType == 2) { targetRenderUnit = curMeshGroup.GetRenderUnit(selectedMeshGroup); }

						if (targetRenderUnit != null)
						{
							curMeshGroup.ChangeRenderUnitDetph(targetRenderUnit, targetRenderUnit._depth + 1);
							RefreshControllerAndHierarchy();
						}
					}
					else
					{	
						curMeshGroup.ChangeBoneDepth(selectedBone, selectedBone._depth + 1);
						RefreshControllerAndHierarchy();
					}
				}

				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Modifier_LayerDown), false, selectedType != 0, btnSize, btnSize, "Layer Down"))
				{
					//4) Layer Down
					if (selectedType == 1 || selectedType == 2)
					{
						apRenderUnit targetRenderUnit = null;
						if (selectedType == 1)		{ targetRenderUnit = curMeshGroup.GetRenderUnit(selectedMesh); }
						else if (selectedType == 2)	{ targetRenderUnit = curMeshGroup.GetRenderUnit(selectedMeshGroup); }

						if (targetRenderUnit != null)
						{
							curMeshGroup.ChangeRenderUnitDetph(targetRenderUnit, targetRenderUnit._depth - 1);
							RefreshControllerAndHierarchy();
						}
					}
					else
					{
						curMeshGroup.ChangeBoneDepth(selectedBone, selectedBone._depth - 1);
						RefreshControllerAndHierarchy();
					}
				}

				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform), false, selectedType == 1 || selectedType == 2, btnSize, btnSize, "Remove Sub Mesh / Mesh Group"))
				{

					string strDialogInfo = Localization.GetText(TEXT.Detach_Body);
					if (selectedType == 1)
					{
						strDialogInfo = Controller.GetRemoveItemMessage(
															_portrait, 
															selectedMesh,
															5,
															Localization.GetText(TEXT.Detach_Body),
															Localization.GetText(TEXT.DLG_RemoveItemChangedWarning));
					}
					else if(selectedType == 2)
					{
						strDialogInfo = Controller.GetRemoveItemMessage(
															_portrait, 
															selectedMeshGroup,
															5,
															Localization.GetText(TEXT.Detach_Body),
															Localization.GetText(TEXT.DLG_RemoveItemChangedWarning));
					}

					//("Detach", "Do you want to detach it?", "Detach", "Cancel");
					bool isResult = EditorUtility.DisplayDialog(Localization.GetText(TEXT.Detach_Title),
																	//Localization.GetText(TEXT.Detach_Body),
																	strDialogInfo,
																	Localization.GetText(TEXT.Detach_Ok),
																	Localization.GetText(TEXT.Cancel)
																	);

					if (isResult)
					{
						//5) Layer Remove
						if (selectedType == 1)
						{
							Controller.DetachMeshInMeshGroup(selectedMesh, curMeshGroup);
						}
						else if (selectedType == 2)
						{
							Controller.DetachMeshGroupInMeshGroup(selectedMeshGroup, curMeshGroup);
						}
					}
				}

				EditorGUILayout.EndHorizontal();
				GUILayout.Space(5);
			}
#region [미사용 코드]
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));
			//bool isMeshGroupCategory_Hide = (_meshGroup_LowerLayout == MESHGROUP_LOWER_LAYOUT.Hide);
			//bool isMeshGroupCategory_ChildList = (_meshGroup_LowerLayout == MESHGROUP_LOWER_LAYOUT.ChildList);
			//bool isMeshGroupCategory_Add = (_meshGroup_LowerLayout == MESHGROUP_LOWER_LAYOUT.Add);

			//int btnWidth_Narrow = 50;
			////int btnWidth_Wide = ((width - btnWidth_Narrow) - 5) / 2;
			//int btnWidth_Children = (width / 2);
			//int btnWidth_Add = (width - (btnWidth_Children + btnWidth_Narrow + 6));
			//int btnHeight = 20;

			//GUILayout.Space(5);
			//if(apEditorUtil.ToggledButton("Children", isMeshGroupCategory_ChildList, btnWidth_Children, btnHeight))
			//{
			//	if(!isMeshGroupCategory_ChildList)
			//	{
			//		_meshGroup_LowerLayout = MESHGROUP_LOWER_LAYOUT.ChildList;
			//	}
			//}
			//if(apEditorUtil.ToggledButton("Add", isMeshGroupCategory_Add, btnWidth_Add, btnHeight))
			//{
			//	if(!isMeshGroupCategory_Add)
			//	{
			//		_meshGroup_LowerLayout = MESHGROUP_LOWER_LAYOUT.Add;
			//	}
			//}
			//if(apEditorUtil.ToggledButton("Hide", isMeshGroupCategory_Hide, btnWidth_Narrow, btnHeight))
			//{
			//	if(!isMeshGroupCategory_Hide)
			//	{
			//		_meshGroup_LowerLayout = MESHGROUP_LOWER_LAYOUT.Hide;
			//	}
			//}

			//EditorGUILayout.EndHorizontal(); 

#endregion
		}


		private void GUI_MainRight_Lower_AnimationHeader(int width, int height)
		{
			width -= 2;
			if (_portrait == null)
			{
				return;
			}

			if (Select.AnimClip == null)
			{
				return;
			}

			//1. 타이틀 + Show/Hide
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(5);

			GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
			guiStyle.normal.textColor = Color.white;
			guiStyle.alignment = TextAnchor.MiddleCenter;

			Color prevColor = GUI.backgroundColor;


			bool isTimelineSelected = false;
			apMeshGroup targetMeshGroup = Select.AnimClip._targetMeshGroup;

			if (Select.AnimTimeline != null)
			{
				isTimelineSelected = true;
			}

			//선택한 타임 라인의 타입에 따라 하단 하이라키가 바뀐다.
			string headerTitle = "";

			bool isBtnHeader = false;//버튼을 두어서 Header 타입을 보여줄 것인가, Box lable로 보여줄 것인가.

			apSelection.MESHGROUP_CHILD_HIERARCHY childHierarchy = Select._meshGroupChildHierarchy_Anim;
			bool isChildMesh = childHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes;
			if (targetMeshGroup == null)
			{
				//headerTitle = "(Select MeshGroup)";
				headerTitle = "(" + GetUIWord(UIWORD.SelectMeshGroup) + ")";
			}
			else
			{
				if (!isTimelineSelected)
				{
					if (childHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
					{
						//headerTitle = "Mesh Group Layers";
						headerTitle = GetUIWord(UIWORD.MeshGroup) + GetUIWord(UIWORD.Layer);
					}
					else
					{
						//TODO : Bone은 어떻게 처리하지?
						//AnimatedModifier인 경우에는 버튼을 두어서 Layer/Bone 처리할 수 있게 하자
						//headerTitle = "Bones";
						headerTitle = GetUIWord(UIWORD.Bones);
					}
					isBtnHeader = true;
				}
				else
				{
					switch (Select.AnimTimeline._linkType)
					{
						case apAnimClip.LINK_TYPE.AnimatedModifier:
							if (childHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
							{
								//headerTitle = "Mesh Group Layers";
								headerTitle = GetUIWord(UIWORD.MeshGroup) + GetUIWord(UIWORD.Layer);
							}
							else
							{
								//TODO : Bone은 어떻게 처리하지?
								//AnimatedModifier인 경우에는 버튼을 두어서 Layer/Bone 처리할 수 있게 하자
								//headerTitle = "Bones";
								headerTitle = GetUIWord(UIWORD.Bones);
							}
							isBtnHeader = true;
							break;


						//case apAnimClip.LINK_TYPE.Bone:
						//	headerTitle = "Bones";
						//	break;

						case apAnimClip.LINK_TYPE.ControlParam:
							//headerTitle = "Control Parameters";
							headerTitle = GetUIWord(UIWORD.ControlParameters);
							break;
					}
				}
			}

			if (isBtnHeader)
			{
				int toggleBtnWidth = ((width - (25 + 2)) / 2) - 2;
				if (apEditorUtil.ToggledButton(GetUIWord(UIWORD.Meshes), isChildMesh, toggleBtnWidth, 20))//"Meshes"
				{
					Select._meshGroupChildHierarchy_Anim = apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes;
				}
				if (apEditorUtil.ToggledButton(GetUIWord(UIWORD.Bones), !isChildMesh, toggleBtnWidth, 20))//"Bones"
				{
					Select._meshGroupChildHierarchy_Anim = apSelection.MESHGROUP_CHILD_HIERARCHY.Bones;
				}
			}
			else
			{
				//GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);
				GUI.backgroundColor = apEditorUtil.ToggleBoxColor_Selected;
				GUILayout.Box(headerTitle, guiStyle, GUILayout.Width(width - (25 + 2)), GUILayout.Height(20));
				GUI.backgroundColor = prevColor;
			}




			bool isOpened = (_rightLowerLayout == RIGHT_LOWER_LAYOUT.ChildList);
			Texture2D btnImage = null;
			if (isOpened)
			{ btnImage = ImageSet.Get(apImageSet.PRESET.Hierarchy_OpenLayout); }
			else
			{ btnImage = ImageSet.Get(apImageSet.PRESET.Hierarchy_HideLayout); }

			GUIStyle guiStyle_Btn = new GUIStyle(GUI.skin.label);
			if (GUILayout.Button(btnImage, guiStyle_Btn, GUILayout.Width(25), GUILayout.Height(25)))
			{
				if (_rightLowerLayout == RIGHT_LOWER_LAYOUT.ChildList)
				{
					_rightLowerLayout = RIGHT_LOWER_LAYOUT.Hide;
				}
				else
				{
					_rightLowerLayout = RIGHT_LOWER_LAYOUT.ChildList;
				}
			}

			EditorGUILayout.EndHorizontal();
		}

		private object _loadKey_AddChildTransform = null;
		private void OnAddChildTransformDialogResult(bool isSuccess, object loadKey, apMesh mesh, apMeshGroup meshGroup)
		{
			if (!isSuccess)
			{
				return;
			}

			if (_loadKey_AddChildTransform != loadKey)
			{
				return;
			}


			_loadKey_AddChildTransform = null;

			if (Select.MeshGroup == null)
			{
				return;
			}
			if (mesh != null)
			{
				apTransform_Mesh addedMeshTransform = Controller.AddMeshToMeshGroup(mesh);
				if(addedMeshTransform != null)
				{
					Select.SetSubMeshInGroup(addedMeshTransform);
					RefreshControllerAndHierarchy();
				}

			}
			else if (meshGroup != null)
			{
				apTransform_MeshGroup addedMeshGroupTransform = Controller.AddMeshGroupToMeshGroup(meshGroup);
				if(addedMeshGroupTransform != null)
				{
					Select.SetSubMeshGroupInGroup(addedMeshGroupTransform);
					RefreshControllerAndHierarchy();
				}
			}
		}


		private void GUI_MainRight_Lower(int width, int height, float scrollX, bool isGUIEvent)
		{
			if (_portrait == null)
			{
				return;
			}
			if (Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				GUILayout.Space(5);
				switch (_rightLowerLayout)
				{
					case RIGHT_LOWER_LAYOUT.Hide:
						break;

					case RIGHT_LOWER_LAYOUT.ChildList:

						if (Event.current.type == EventType.Layout)
						{
							SetGUIVisible("GUI MeshGroup Hierarchy Delayed", true);
						}
						if (IsDelayedGUIVisible("GUI MeshGroup Hierarchy Delayed"))
						{
							Hierarchy_MeshGroup.GUI_RenderHierarchy(	width, 
																		Select._meshGroupChildHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes, 
																		scrollX,
																		isGUIEvent);
						}
						break;

#region [미사용 코드] Child Mesh / MeshGroup 추가는 Dialog로 변경되었다.
						//case RIGHT_LOWER_LAYOUT.Add:
						//	// 리스트를 만들자
						//	{
						//		List<apMesh> meshes = _portrait._meshes;
						//		List<apMeshGroup> meshGroups = _portrait._meshGroups;

						//		EditorGUILayout.LabelField("Meshes");
						//		for (int i = 0; i < meshes.Count; i++)
						//		{
						//			EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
						//			EditorGUILayout.LabelField(new GUIContent(" " + meshes[i]._name, ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh)), GUILayout.Width(width - 50));
						//			if(GUILayout.Button("Add", GUILayout.Width(40)))
						//			{
						//				Controller.AddMeshToMeshGroup(meshes[i]);
						//				_rightLowerLayout = apEditor.RIGHT_LOWER_LAYOUT.ChildList;
						//			}
						//			EditorGUILayout.EndHorizontal();
						//		}

						//		GUILayout.Space(20);
						//		EditorGUILayout.LabelField("Mesh Groups");
						//		for (int i = 0; i < meshGroups.Count; i++)
						//		{
						//			if(Select.MeshGroup == meshGroups[i])
						//			{
						//				continue;
						//			}
						//			bool isExistInChildren = Select.MeshGroup._childMeshGroupTransforms.Exists(delegate (apTransform_MeshGroup a)
						//			{
						//				return a._meshGroup == meshGroups[i];
						//			});
						//			if(isExistInChildren)
						//			{
						//				continue;
						//			}

						//			EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
						//			EditorGUILayout.LabelField(new GUIContent(" " + meshGroups[i]._name, ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup)), GUILayout.Width(width - 50));
						//			if(GUILayout.Button("Add", GUILayout.Width(40)))
						//			{
						//				Controller.AddMeshGroupToMeshGroup(meshGroups[i]);
						//				_rightLowerLayout = apEditor.RIGHT_LOWER_LAYOUT.ChildList;
						//			}
						//			EditorGUILayout.EndHorizontal();
						//		}
						//	}
						//	//
						//	break;

#endregion
				}

			}
			else if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
			{
				GUILayout.Space(5);
				switch (_rightLowerLayout)
				{
					case RIGHT_LOWER_LAYOUT.Hide:
						break;

					case RIGHT_LOWER_LAYOUT.ChildList:
						if (Select.AnimTimeline == null)
						{
							if (Select._meshGroupChildHierarchy_Anim == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
							{
								//Mesh 리스트
								if(Event.current.type == EventType.Layout)
								{
									SetGUIVisible("GUI Anim Hierarchy Delayed - Meshes", true);
								}

								if (IsDelayedGUIVisible("GUI Anim Hierarchy Delayed - Meshes"))
								{
									Hierarchy_AnimClip.GUI_RenderHierarchy_Transform(width, scrollX, isGUIEvent);
								}
							}
							else
							{
								//Bone 리스트
								if(Event.current.type == EventType.Layout)
								{
									SetGUIVisible("GUI Anim Hierarchy Delayed - Bone", true);
								}
								if (IsDelayedGUIVisible("GUI Anim Hierarchy Delayed - Bone"))
								{
									Hierarchy_AnimClip.GUI_RenderHierarchy_Bone(width, scrollX, isGUIEvent);
								}
							}


						}
						else
						{
							switch (Select.AnimTimeline._linkType)
							{
								case apAnimClip.LINK_TYPE.AnimatedModifier:
									//Hierarchy_AnimClip.GUI_RenderHierarchy_Transform(width, scrollX);
									if (Select._meshGroupChildHierarchy_Anim == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
									{
										//Mesh 리스트
										if (Event.current.type == EventType.Layout)
										{
											SetGUIVisible("GUI Anim Hierarchy Delayed - Meshes", true);
										}

										if (IsDelayedGUIVisible("GUI Anim Hierarchy Delayed - Meshes"))
										{
											Hierarchy_AnimClip.GUI_RenderHierarchy_Transform(width, scrollX, isGUIEvent);
										}
									}
									else
									{
										//Bone 리스트
										if(Event.current.type == EventType.Layout)
										{
											SetGUIVisible("GUI Anim Hierarchy Delayed - Bone", true);
										}
										if (IsDelayedGUIVisible("GUI Anim Hierarchy Delayed - Bone"))
										{
											Hierarchy_AnimClip.GUI_RenderHierarchy_Bone(width, scrollX, isGUIEvent);
										}
									}
									break;

								case apAnimClip.LINK_TYPE.ControlParam:
									{
										if(Event.current.type == EventType.Layout)
										{
											SetGUIVisible("GUI Anim Hierarchy Delayed - ControlParam", true);
										}
										if (IsDelayedGUIVisible("GUI Anim Hierarchy Delayed - ControlParam"))
										{
											Hierarchy_AnimClip.GUI_RenderHierarchy_ControlParam(width, scrollX, isGUIEvent);
										}
									}
									
									break;



								default:
									//TODO.. 새로운 타입이 추가될 경우
									break;
							}
						}
						break;
				}
			}
		}


		private void GUI_MainRight2(int width, int height)
		{
			if (_portrait == null)
			{
				return;
			}
			Select.DrawEditor_Right2(width - 2, height);
			//if(!_selection.DrawEditor(width - 2, height))
			//{
			//	if(_portrait != null)
			//	{
			//		_selection.SetPortrait(_portrait);
			//	}
			//	else
			//	{
			//		_selection.Clear();
			//	}

			//	_hierarchy.ResetAllUnits();
			//	_hierarchy_MeshGroup.ResetMeshGroupSubUnits();
			//}
		}

		private void GUI_Bottom(int width, int height, int layoutX, int layoutY, int windowWidth, int windowHeight)
		{
			GUILayout.Space(5);

			Select.DrawEditor_Bottom(width, height - 8, layoutX, layoutY, windowWidth, windowHeight);
		}

		private void GUI_Bottom2(int width, int height)
		{
			GUILayout.Space(10);

			Select.DrawEditor_Bottom2Edit(width, height - 12);
		}

		//--------------------------------------------------------------










		// 프레임 카운트
		/// <summary>
		/// 프레임 시간을 계산한다.
		/// Update의 경우 60FPS 기준의 동기화된 시간을 사용한다.
		/// "60FPS으로 동기가 된 상태"인 경우에 true로 리턴을 한다.
		/// </summary>
		/// <param name="frameTimerType"></param>
		/// <returns></returns>
		private bool UpdateFrameCount(FRAME_TIMER_TYPE frameTimerType)
		{
			switch (frameTimerType)
			{
				case FRAME_TIMER_TYPE.None:
					return false;

				case FRAME_TIMER_TYPE.Repaint:
					apTimer.I.CheckTime_Repaint();

					_avgFPS = _avgFPS * 0.93f + ((float)apTimer.I.FPS) * 0.07f;
					_iAvgFPS = (int)(_avgFPS + 0.5f);

					return false;

				case FRAME_TIMER_TYPE.Update:
					return apTimer.I.CheckTime_Update();
			}
			return false;
			//_frameTimerType = frameTimerType;

			//switch (_frameTimerType)
			//{
			//	case FRAME_TIMER_TYPE.Update:
			//		//_tDelta_Update += DateTime.Now.Subtract(_dateTime_Update).TotalSeconds;
			//		_tDelta_Update += (double)((DateTime.Now.Ticks - _dateTime_Update.Ticks) / 10000) / 1000.0;
			//		_dateTime_Update = DateTime.Now;

			//		if(_tDelta_Update > MIN_EDITOR_UPDATE_TIME)
			//		{
			//			_tDeltaDelayed_Update = (float)MIN_EDITOR_UPDATE_TIME;
			//			_tDelta_Update -= MIN_EDITOR_UPDATE_TIME;
			//		}
			//		else
			//		{
			//			_tDeltaDelayed_Update = 0.0f;
			//		}

			//		break;

			//	case FRAME_TIMER_TYPE.GUIRepaint:
			//		//_tDelta_GUI += DateTime.Now.Subtract(_dateTime_GUI).TotalSeconds;
			//		_tDelta_GUI += (double)((DateTime.Now.Ticks - _dateTime_GUI.Ticks) / 10000) / 1000.0;

			//		_dateTime_GUI = DateTime.Now;

			//		if(_tDelta_GUI > MIN_EDITOR_UPDATE_TIME)
			//		{
			//			_tDeltaDelayed_GUI = (float)MIN_EDITOR_UPDATE_TIME;
			//			_tDelta_GUI -= MIN_EDITOR_UPDATE_TIME;
			//			if (_portrait != null)
			//			{
			//				_portrait.IncreaseUpdateKeyIndex();
			//			}
			//		}
			//		else
			//		{
			//			_tDeltaDelayed_GUI = 0.0f;
			//		}
			//		break;
			//}
		}

		//private void UseFrameCount()
		//{

		//	_deltaTimePerValidFrame = 0.0f;
		//}

		//-----------------------------------------------------------------------
		public void OnHotKeyDown(KeyCode keyCode, bool isCtrl, bool isAlt, bool isShift)
		{
			if (_isHotKeyProcessable)
			{
				_isHotKeyEvent = true;
				_hotKeyCode = keyCode;

				_isHotKey_Ctrl = isCtrl;
				_isHotKey_Alt = isAlt;
				_isHotKey_Shift = isShift;
				_isHotKeyProcessable = false;
			}
			else
			{
				if (!_isHotKeyEvent)
				{
					_isHotKeyProcessable = true;
				}
			}
		}

		public void OnHotKeyUp()
		{
			_isHotKeyProcessable = true;
			_isHotKeyEvent = false;
			_hotKeyCode = KeyCode.A;
			_isHotKey_Ctrl = false;
			_isHotKey_Alt = false;
			_isHotKey_Shift = false;
		}

		public void UseHotKey()
		{
			_isHotKeyEvent = false;
			_hotKeyCode = KeyCode.A;
			_isHotKey_Ctrl = false;
			_isHotKey_Alt = false;
			_isHotKey_Shift = false;
		}

		public bool IsHotKeyEvent { get { return _isHotKeyEvent; } }
		public KeyCode HotKeyCode { get { return _hotKeyCode; } }
		public bool IsHotKey_Ctrl { get { return _isHotKey_Ctrl; } }
		public bool IsHotKey_Alt { get { return _isHotKey_Alt; } }
		public bool IsHotKey_Shift { get { return _isHotKey_Shift; } }



		//-----------------------------------------------------------------------------
		// GUILayout의 Visible 여부가 외부에 의해 결정되는 경우
		// 바로 바뀌면 GUI 에러가 나므로 약간의 지연 처리를 해야한다.
		//-----------------------------------------------------------------------------

		/// <summary>
		/// GUILayout가 Show/Hide 토글시, 그 정보를 저장한다.
		/// 범용으로 쓸 수 있게 하기 위해서 String을 키값으로 Dictionary에 저장한다.
		/// </summary>
		/// <param name="keyName">Dictionary 키값</param>
		/// <param name="isVisible">Visible 값</param>
		public void SetGUIVisible(string keyName, bool isVisible)
		{
			if (_delayedGUIShowList.ContainsKey(keyName))
			{
				if (_delayedGUIShowList[keyName] != isVisible)
				{
					_delayedGUIShowList[keyName] = isVisible;//Visible 조건 값

					//_delayedGUIToggledList는 "Visible 값이 바뀌었을때 그걸 바로 GUI에 적용했는지"를 저장한다.
					//바뀐 순간엔 GUI에 적용 전이므로 "Visible Toggle이 완료되었는지"를 저장하는 리스트엔 False를 넣어둔다.
					_delayedGUIToggledList[keyName] = false;
				}
			}
			else
			{
				_delayedGUIShowList.Add(keyName, isVisible);
				_delayedGUIToggledList.Add(keyName, false);
			}
		}


		/// <summary>
		/// GUILayout 출력 가능한지 여부 알려주는 함수
		/// Hide -> Show 상태에서 GUI Event가 Layout/Repaint가 아니라면 잠시 Hide 상태를 유지해야한다.
		/// </summary>
		/// <param name="keyName">Dictionary 키값</param>
		/// <returns>True이면 GUILayout을 출력할 수 있다. False는 안됨</returns>
		public bool IsDelayedGUIVisible(string keyName)
		{
			//GUI Layout이 출력되려면
			//1. Visible 값이 True여야 한다.
			//2-1. GUI Event가 Layout/Repaint 여야 한다.
			//2-2. GUI Event 종류에 상관없이 계속 Visible 상태였다면 출력 가능하다.


			//1-1. GUI Layout의 Visible 여부를 결정하는 값이 없다면 -> False
			if (!_delayedGUIShowList.ContainsKey(keyName))
			{
				return false;
			}

			//1-2. GUI Layout의 Visible 값이 False라면 -> False
			if (!_delayedGUIShowList[keyName])
			{
				return false;
			}

			//2. (Toggle 처리가 완료되지 않은 상태에서..)
			if (!_delayedGUIToggledList[keyName])
			{
				//2-1. GUI Event가 Layout/Repaint라면 -> 다음 OnGUI까지 일단 보류합니다. False
				if (!_isGUIEvent)
				{
					return false;
				}

				// GUI Event가 유효하다면 Visible이 가능하다고 바꿔줍니다.
				//_delayedGUIToggledList [False -> True]
				_delayedGUIToggledList[keyName] = true;
			}

			//2-2. GUI Event 종류에 상관없이 계속 Visible 상태였다면 출력 가능하다. -> True
			return true;
		}

		/// <summary>
		/// Scroll Position을 리셋한다.
		/// </summary>
		public void ResetScrollPosition(bool isResetLeft, bool isResetCenter, bool isResetRight, bool isResetRight2, bool isResetBottom)
		{
			if (isResetLeft)
			{
				_scroll_MainLeft = Vector2.zero;
			}
			if (isResetCenter)
			{
				_scroll_MainCenter = Vector2.zero;
			}
			if (isResetRight)
			{
				_scroll_MainRight = Vector2.zero;
				_scroll_MainRight_Lower_MG_Mesh = Vector2.zero;
				_scroll_MainRight_Lower_MG_Bone = Vector2.zero;
				_scroll_MainRight_Lower_Anim = Vector2.zero;
			}

			if(isResetRight2)
			{
				_scroll_MainRight2 = Vector2.zero;
			}

			if (isResetBottom)
			{
				_scroll_Bottom = Vector2.zero;
			}

			
		}

		public void SetLeftTab(TAB_LEFT  leftTabType)
		{	
			if(_tabLeft != leftTabType)
			{
				ResetScrollPosition(true, false, false, false, false);
			}
			_tabLeft = leftTabType;
		}

		//------------------------------------------------------------------------
		// Hot Key
		//------------------------------------------------------------------------
		/// <summary>
		/// HotKey 이벤트를 등록합니다.
		/// </summary>
		/// <param name="funcEvent"></param>
		/// <param name="keyCode"></param>
		/// <param name="isShift"></param>
		/// <param name="isAlt"></param>
		/// <param name="isCtrl"></param>
		public void AddHotKeyEvent(apHotKey.FUNC_HOTKEY_EVENT funcEvent, string label, KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
		{
			HotKey.AddHotKeyEvent(funcEvent, label, keyCode, isShift, isAlt, isCtrl, paramObject);
		}


		//------------------------------------------------------------------------
		// Hierarchy Filter 제어.
		//------------------------------------------------------------------------
		/// <summary>
		/// Hierarchy Filter 제어.
		/// 수동으로 제어하거나, "어떤 객체가 추가"되었다면 Filter가 열린다.
		/// All, None은 isEnabled의 영향을 받지 않는다. (All은 모두 True, None은 모두 False)
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="isEnabled"></param>
		public void SetHierarchyFilter(HIERARCHY_FILTER filter, bool isEnabled)
		{
			HIERARCHY_FILTER prevFilter = _hierarchyFilter;
			bool isRootUnit = ((int)(_hierarchyFilter & HIERARCHY_FILTER.RootUnit) != 0);
			bool isImage = ((int)(_hierarchyFilter & HIERARCHY_FILTER.Image) != 0);
			bool isMesh = ((int)(_hierarchyFilter & HIERARCHY_FILTER.Mesh) != 0);
			bool isMeshGroup = ((int)(_hierarchyFilter & HIERARCHY_FILTER.MeshGroup) != 0);
			bool isAnimation = ((int)(_hierarchyFilter & HIERARCHY_FILTER.Animation) != 0);
			bool isParam = ((int)(_hierarchyFilter & HIERARCHY_FILTER.Param) != 0);

			switch (filter)
			{
				case HIERARCHY_FILTER.All:
					isRootUnit = true;
					isImage = true;
					isMesh = true;
					isMeshGroup = true;
					isAnimation = true;
					isParam = true;
					break;

				case HIERARCHY_FILTER.RootUnit:
					isRootUnit = isEnabled;
					break;


				case HIERARCHY_FILTER.Image:
					isImage = isEnabled;
					break;

				case HIERARCHY_FILTER.Mesh:
					isMesh = isEnabled;
					break;

				case HIERARCHY_FILTER.MeshGroup:
					isMeshGroup = isEnabled;
					break;

				case HIERARCHY_FILTER.Animation:
					isAnimation = isEnabled;
					break;

				case HIERARCHY_FILTER.Param:
					isParam = isEnabled;
					break;

				case HIERARCHY_FILTER.None:
					isRootUnit = false;
					isImage = false;
					isMesh = false;
					isMeshGroup = false;
					isAnimation = false;
					isParam = false;
					break;
			}

			_hierarchyFilter = HIERARCHY_FILTER.None;

			if (isRootUnit)		{ _hierarchyFilter |= HIERARCHY_FILTER.RootUnit; }
			if (isImage)		{ _hierarchyFilter |= HIERARCHY_FILTER.Image; }
			if (isMesh)			{ _hierarchyFilter |= HIERARCHY_FILTER.Mesh; }
			if (isMeshGroup)	{ _hierarchyFilter |= HIERARCHY_FILTER.MeshGroup; }
			if (isAnimation)	{ _hierarchyFilter |= HIERARCHY_FILTER.Animation; }
			if (isParam)		{ _hierarchyFilter |= HIERARCHY_FILTER.Param; }

			if (prevFilter != _hierarchyFilter && _tabLeft == TAB_LEFT.Hierarchy)
			{
				_scroll_MainLeft = Vector2.zero;
			}

			//이건 설정으로 저장해야한다.
			SaveEditorPref();
		}

		public bool IsHierarchyFilterContain(HIERARCHY_FILTER filter)
		{
			if (filter == HIERARCHY_FILTER.All)
			{
				return _hierarchyFilter == HIERARCHY_FILTER.All;
			}
			return (int)(_hierarchyFilter & filter) != 0;
		}

		//Dialog Event
		//----------------------------------------------------------------------------------
		private object _loadKey_FFDStart = null;
		private int _curFFDSizeX = 3;
		private int _curFFDSizeY = 3;
		private void OnDialogEvent_FFDStart(bool isSuccess, object loadKey, int numX, int numY)
		{
			if (_loadKey_FFDStart != loadKey || !isSuccess)
			{
				_loadKey_FFDStart = null;
				return;
			}
			_loadKey_FFDStart = null;

			_curFFDSizeX = numX;
			_curFFDSizeY = numY;

			Gizmos.StartTransformMode(this, _curFFDSizeX, _curFFDSizeY);//원래는 <이거
		}



		//---------------------------------------------------------------------------
		// Portrait 생성
		//---------------------------------------------------------------------------
		private void MakeNewPortrait()
		{
			if (!_isMakePortraitRequest)
			{
				return;
			}
			GameObject newPortraitObj = new GameObject(_requestedNewPortraitName);
			newPortraitObj.transform.position = Vector3.zero;
			newPortraitObj.transform.rotation = Quaternion.identity;
			newPortraitObj.transform.localScale = Vector3.one;

			_portrait = newPortraitObj.AddComponent<apPortrait>();

			//Selection.activeGameObject = newPortraitObj;
			Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

			_isMakePortraitRequest = false;
			_requestedNewPortraitName = "";


			//추가
			//초기화시 Important와 Opt 정보는 여기서 별도로 초기화를 하자
			_portrait._isOptimizedPortrait = false;
			_portrait._bakeTargetOptPortrait = null;
			_portrait._bakeSrcEditablePortrait = null;
			_portrait._isImportant = true;


			Controller.InitTmpValues();
			_selection.SetPortrait(_portrait);
			
			//Portrait의 레퍼런스들을 연결해주자
			Controller.PortraitReadyToEdit();




			//Selection.activeGameObject = _portrait.gameObject;
			Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

			//시작은 RootUnit
			_selection.SetOverallDefault();

			OnAnyObjectAddedOrRemoved();

			_hierarchy.ResetAllUnits();
			_hierarchy_MeshGroup.ResetSubUnits();
			_hierarchy_AnimClip.ResetSubUnits();
		}

		private void MakePortraitFromBackupFile()
		{
			if(!_isMakePortraitRequestFromBackupFile)
			{
				return;
			}

			apPortrait loadedPortrait = Backup.LoadBackup(_requestedLoadedBackupPortraitFilePath);
			if (loadedPortrait != null)
			{
				_portrait = loadedPortrait;

				//초기 설정 추가 (Important는 백업 설정을 따른다)
				_portrait._isOptimizedPortrait = false;
				_portrait._bakeTargetOptPortrait = null;
				_portrait._bakeSrcEditablePortrait = null;

				Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

				Controller.InitTmpValues();
				_selection.SetPortrait(_portrait);

				//Portrait의 레퍼런스들을 연결해주자
				Controller.PortraitReadyToEdit();


				//Selection.activeGameObject = _portrait.gameObject;
				Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

				//시작은 RootUnit
				_selection.SetOverallDefault();

				OnAnyObjectAddedOrRemoved();

				_hierarchy.ResetAllUnits();
				_hierarchy_MeshGroup.ResetSubUnits();
				_hierarchy_AnimClip.ResetSubUnits();

				Notification("Backup File [" + _requestedLoadedBackupPortraitFilePath + "] is loaded", false, false);
			}

			_isMakePortraitRequestFromBackupFile = false;
			_requestedLoadedBackupPortraitFilePath = "";

			

			
		}

		//----------------------------------------------------------------------------------
		// 에디터 옵션을 저장/로드하자
		//----------------------------------------------------------------------------------
		public void SaveEditorPref()
		{
			//Debug.Log("Save Editor Pref");

			EditorPrefs.SetInt("AnyPortrait_HierarchyFilter", (int)_hierarchyFilter);

			if (_selection != null)
			{
				EditorPrefs.SetBool("AnyPortrait_IsAutoNormalize", Select._rigEdit_isAutoNormalize);
				EditorPrefs.SetBool("AnyPortrait_IsBoneColorView", Select._rigEdit_isBoneColorView);
				EditorPrefs.SetInt("AnyPortrait_ViewMode", (int)Select._rigEdit_viewMode);
			}

			EditorPrefs.SetInt("AnyPortrait_Language", (int)_language);
			SaveColorPref("AnyPortrait_Color_Backgroud", _colorOption_Background);
			SaveColorPref("AnyPortrait_Color_GridCenter", _colorOption_GridCenter);
			SaveColorPref("AnyPortrait_Color_Grid", _colorOption_Grid);

			SaveColorPref("AnyPortrait_Color_MeshEdge", _colorOption_MeshEdge);
			SaveColorPref("AnyPortrait_Color_MeshHiddenEdge", _colorOption_MeshHiddenEdge);
			SaveColorPref("AnyPortrait_Color_Outline", _colorOption_Outline);
			SaveColorPref("AnyPortrait_Color_TFBorder", _colorOption_TransformBorder);

			SaveColorPref("AnyPortrait_Color_VertNotSelected", _colorOption_VertColor_NotSelected);
			SaveColorPref("AnyPortrait_Color_VertSelected", _colorOption_VertColor_Selected);

			SaveColorPref("AnyPortrait_Color_GizmoFFDLine", _colorOption_GizmoFFDLine);
			SaveColorPref("AnyPortrait_Color_GizmoFFDInnerLine", _colorOption_GizmoFFDInnerLine);
			SaveColorPref("AnyPortrait_Color_OnionToneColor", _colorOption_OnionToneColor);
			

			EditorPrefs.SetInt("AnyPortrait_AnimTimelineLayerSort", (int)_timelineInfoSortType);

			EditorPrefs.SetInt("AnyPortrait_Capture_PosX", _captureFrame_PosX);
			EditorPrefs.SetInt("AnyPortrait_Capture_PosY", _captureFrame_PosY);
			EditorPrefs.SetInt("AnyPortrait_Capture_SrcWidth", _captureFrame_SrcWidth);
			EditorPrefs.SetInt("AnyPortrait_Capture_SrcHeight", _captureFrame_SrcHeight);
			EditorPrefs.SetInt("AnyPortrait_Capture_DstWidth", _captureFrame_DstWidth);
			EditorPrefs.SetInt("AnyPortrait_Capture_DstHeight", _captureFrame_DstHeight);

			EditorPrefs.SetInt("AnyPortrait_Capture_SpriteUnitWidth", _captureFrame_SpriteUnitWidth);
			EditorPrefs.SetInt("AnyPortrait_Capture_SpriteUnitHeight", _captureFrame_SpriteUnitHeight);
			EditorPrefs.SetInt("AnyPortrait_Capture_SpriteMargin", _captureFrame_SpriteMargin);
			

			EditorPrefs.SetBool("AnyPortrait_Capture_IsShowFrame", _isShowCaptureFrame);
			SaveColorPref("AnyPortrait_Capture_BGColor", _captureFrame_Color);
			EditorPrefs.SetBool("AnyPortrait_Capture_IsPhysics", _captureFrame_IsPhysics);

			EditorPrefs.SetBool("AnyPortrait_Capture_IsAspectRatioFixed", _isCaptureAspectRatioFixed);
			EditorPrefs.SetInt("AnyPortrait_Capture_GIFSample", _captureFrame_GIFSampleQuality);
			EditorPrefs.SetInt("AnyPortrait_Capture_GIFLoopCount", _captureFrame_GIFSampleLoopCount);

			EditorPrefs.SetInt("AnyPortrait_Capture_SpritePackImageWidth", (int)_captureSpritePackImageWidth);
			EditorPrefs.SetInt("AnyPortrait_Capture_SpritePackImageHeight", (int)_captureSpritePackImageHeight);
			EditorPrefs.SetInt("AnyPortrait_Capture_SpriteTrimSize", (int)_captureSpriteTrimSize);
			EditorPrefs.SetBool("AnyPortrait_Capture_SpriteMeta_XML", _captureSpriteMeta_XML);
			EditorPrefs.SetBool("AnyPortrait_Capture_SpriteMeta_JSON", _captureSpriteMeta_JSON);
			EditorPrefs.SetBool("AnyPortrait_Capture_SpriteMeta_TXT", _captureSpriteMeta_TXT);
			
			EditorPrefs.SetFloat("AnyPortrait_Capture_SpriteScreenPosX", _captureSprite_ScreenPos.x);
			EditorPrefs.SetFloat("AnyPortrait_Capture_SpriteScreenPosY", _captureSprite_ScreenPos.y);
			EditorPrefs.SetInt("AnyPortrait_Capture_SpriteScreenZoomIndex", _captureSprite_ScreenZoom);
		



			EditorPrefs.SetInt("AnyPortrait_BoneRenderMode", (int)_boneGUIRenderMode);

			EditorPrefs.SetBool("AnyPortrait_GUI_FPSVisible", _guiOption_isFPSVisible);
			EditorPrefs.SetBool("AnyPortrait_GUI_StatisticsVisible", _guiOption_isStatisticsVisible);


			EditorPrefs.SetBool("AnyPortrait_AutoBackup_Enabled", _backupOption_IsAutoSave);
			EditorPrefs.SetString("AnyPortrait_AutoBackup_Path", _backupOption_BaseFolderName);
			EditorPrefs.SetInt("AnyPortrait_AutoBackup_Time", _backupOption_Minute);

			EditorPrefs.SetString("AnyPortrait_BonePose_Path", _bonePose_BaseFolderName);

			EditorPrefs.SetBool("AnyPortrait_StartScreen_IsShow", _startScreenOption_IsShowStartup);
			EditorPrefs.SetInt("AnyPortrait_StartScreen_LastMonth", _startScreenOption_LastMonth);
			EditorPrefs.SetInt("AnyPortrait_StartScreen_LastDay", _startScreenOption_LastDay);
			EditorPrefs.SetInt("AnyPortrait_UpdateLogScreen_LastVersion", _updateLogScreen_LastVersion);
			

			EditorPrefs.SetBool("AnyPortrait_IsBakeColorSpace_ToGamma", _isBakeColorSpaceToGamma);


			EditorPrefs.SetBool("AnyPortrait_ModLockOp_CalculateIfNotAddedOther",	_modLockOption_CalculateIfNotAddedOther);
			EditorPrefs.SetBool("AnyPortrait_ModLockOp_ColorPreview_Lock",			_modLockOption_ColorPreview_Lock);
			EditorPrefs.SetBool("AnyPortrait_ModLockOp_ColorPreview_Unlock",		_modLockOption_ColorPreview_Unlock);
			EditorPrefs.SetBool("AnyPortrait_ModLockOp_BonePreview_Lock",			_modLockOption_BonePreview_Lock);
			EditorPrefs.SetBool("AnyPortrait_ModLockOp_BonePreview_Unlock",			_modLockOption_BonePreview_Unlock);
			//EditorPrefs.SetBool("AnyPortrait_ModLockOp_MeshPreview_Lock",			_modLockOption_MeshPreview_Lock);
			//EditorPrefs.SetBool("AnyPortrait_ModLockOp_MeshPreview_Unlock",			_modLockOption_MeshPreview_Unlock);
			EditorPrefs.SetBool("AnyPortrait_ModLockOp_ModListUI_Lock",				_modLockOption_ModListUI_Lock);
			EditorPrefs.SetBool("AnyPortrait_ModLockOp_ModListUI_Unlock",			_modLockOption_ModListUI_Unlock);

			//SaveColorPref("AnyPortrait_ModLockOp_MeshPreviewColor", _modLockOption_MeshPreviewColor);
			SaveColorPref("AnyPortrait_ModLockOp_BonePreviewColor", _modLockOption_BonePreviewColor);
			//public Color _modLockOption_MeshPreviewColor = new Color(1.0f, 0.45f, 0.1f, 0.8f);
			//public Color _modLockOption_BonePreviewColor = new Color(1.0f, 0.8f, 0.1f, 0.8f);

			_localization.SetLanguage(_language);
		}

		public void LoadEditorPref()
		{
			//Debug.Log("Load Editor Pref");

			_hierarchyFilter = (HIERARCHY_FILTER)EditorPrefs.GetInt("AnyPortrait_HierarchyFilter", (int)HIERARCHY_FILTER.All);

			if (_selection != null)
			{
				Select._rigEdit_isAutoNormalize = EditorPrefs.GetBool("AnyPortrait_IsAutoNormalize", Select._rigEdit_isAutoNormalize);
				Select._rigEdit_isBoneColorView = EditorPrefs.GetBool("AnyPortrait_IsBoneColorView", Select._rigEdit_isBoneColorView);
				Select._rigEdit_viewMode = (apSelection.RIGGING_EDIT_VIEW_MODE)EditorPrefs.GetInt("AnyPortrait_ViewMode", (int)Select._rigEdit_viewMode);
			}

			_language = (LANGUAGE)EditorPrefs.GetInt("AnyPortrait_Language", (int)LANGUAGE.English);

			_colorOption_Background = LoadColorPref("AnyPortrait_Color_Backgroud", new Color(0.2f, 0.2f, 0.2f, 1.0f));
			_colorOption_GridCenter = LoadColorPref("AnyPortrait_Color_GridCenter", new Color(0.7f, 0.7f, 0.3f, 1.0f));
			_colorOption_Grid = LoadColorPref("AnyPortrait_Color_Grid", new Color(0.3f, 0.3f, 0.3f, 1.0f));

			_colorOption_MeshEdge = LoadColorPref("AnyPortrait_Color_MeshEdge", new Color(1.0f, 0.5f, 0.0f, 0.9f));
			_colorOption_MeshHiddenEdge = LoadColorPref("AnyPortrait_Color_MeshHiddenEdge", new Color(1.0f, 1.0f, 0.0f, 0.7f));
			_colorOption_Outline = LoadColorPref("AnyPortrait_Color_Outline", new Color(0.0f, 0.5f, 1.0f, 0.7f));
			_colorOption_TransformBorder = LoadColorPref("AnyPortrait_Color_TFBorder", new Color(0.0f, 1.0f, 1.0f, 1.0f));

			_colorOption_VertColor_NotSelected = LoadColorPref("AnyPortrait_Color_VertNotSelected", new Color(0.0f, 0.3f, 1.0f, 0.6f));
			_colorOption_VertColor_Selected = LoadColorPref("AnyPortrait_Color_VertSelected", new Color(1.0f, 0.0f, 0.0f, 1.0f));

			_colorOption_GizmoFFDLine = LoadColorPref("AnyPortrait_Color_GizmoFFDLine", new Color(1.0f, 0.5f, 0.2f, 0.9f));
			_colorOption_GizmoFFDInnerLine = LoadColorPref("AnyPortrait_Color_GizmoFFDInnerLine", new Color(1.0f, 0.7f, 0.2f, 0.7f));
			_colorOption_OnionToneColor = LoadColorPref("AnyPortrait_Color_OnionToneColor", new Color(0.1f, 0.2f, 0.5f, 0.7f));

			_timelineInfoSortType = (TIMELINE_INFO_SORT)EditorPrefs.GetInt("AnyPortrait_AnimTimelineLayerSort", (int)TIMELINE_INFO_SORT.Registered);

			_captureFrame_PosX = EditorPrefs.GetInt("AnyPortrait_Capture_PosX", 0);
			_captureFrame_PosY = EditorPrefs.GetInt("AnyPortrait_Capture_PosY", 0);
			_captureFrame_SrcWidth = EditorPrefs.GetInt("AnyPortrait_Capture_SrcWidth", 500);
			_captureFrame_SrcHeight = EditorPrefs.GetInt("AnyPortrait_Capture_SrcHeight", 500);
			_captureFrame_DstWidth = EditorPrefs.GetInt("AnyPortrait_Capture_DstWidth", 500);
			_captureFrame_DstHeight = EditorPrefs.GetInt("AnyPortrait_Capture_DstHeight", 500);

			_captureFrame_SpriteUnitWidth = EditorPrefs.GetInt("AnyPortrait_Capture_SpriteUnitWidth", 100);
			_captureFrame_SpriteUnitHeight = EditorPrefs.GetInt("AnyPortrait_Capture_SpriteUnitHeight", 100);
			_captureFrame_SpriteMargin = EditorPrefs.GetInt("AnyPortrait_Capture_SpriteMargin", 0);

			_isShowCaptureFrame = EditorPrefs.GetBool("AnyPortrait_Capture_IsShowFrame", true);
			_captureFrame_GIFSampleQuality = EditorPrefs.GetInt("AnyPortrait_Capture_GIFSample", 10);
			_captureFrame_GIFSampleLoopCount = EditorPrefs.GetInt("AnyPortrait_Capture_GIFLoopCount", 1);

			_captureFrame_Color = LoadColorPref("AnyPortrait_Capture_BGColor", Color.black);
			_captureFrame_IsPhysics = EditorPrefs.GetBool("AnyPortrait_Capture_IsPhysics", false);

			_captureSpritePackImageWidth = (CAPTURE_SPRITE_PACK_IMAGE_SIZE)EditorPrefs.GetInt("AnyPortrait_Capture_SpritePackImageWidth", (int)(CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024));
			_captureSpritePackImageHeight = (CAPTURE_SPRITE_PACK_IMAGE_SIZE)EditorPrefs.GetInt("AnyPortrait_Capture_SpritePackImageHeight", (int)(CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024));
			_captureSpriteTrimSize = (CAPTURE_SPRITE_TRIM_METHOD)EditorPrefs.GetInt("AnyPortrait_Capture_SpriteTrimSize", (int)(CAPTURE_SPRITE_TRIM_METHOD.Fixed));
			_captureSpriteMeta_XML = EditorPrefs.GetBool("AnyPortrait_Capture_SpriteMeta_XML", false);
			_captureSpriteMeta_JSON = EditorPrefs.GetBool("AnyPortrait_Capture_SpriteMeta_JSON", false);
			_captureSpriteMeta_TXT = EditorPrefs.GetBool("AnyPortrait_Capture_SpriteMeta_TXT", false);
			
			_captureSprite_ScreenPos.x = EditorPrefs.GetFloat("AnyPortrait_Capture_SpriteScreenPosX", 0.0f);
			_captureSprite_ScreenPos.y = EditorPrefs.GetFloat("AnyPortrait_Capture_SpriteScreenPosY", 0.0f);
			_captureSprite_ScreenZoom = EditorPrefs.GetInt("AnyPortrait_Capture_SpriteScreenZoomIndex", ZOOM_INDEX_DEFAULT);
			




			_isCaptureAspectRatioFixed = EditorPrefs.GetBool("AnyPortrait_Capture_IsAspectRatioFixed", true);

			_boneGUIRenderMode = (BONE_RENDER_MODE)EditorPrefs.GetInt("AnyPortrait_BoneRenderMode", (int)BONE_RENDER_MODE.Render);

			_guiOption_isFPSVisible = EditorPrefs.GetBool("AnyPortrait_GUI_FPSVisible", true);
			_guiOption_isStatisticsVisible = EditorPrefs.GetBool("AnyPortrait_GUI_StatisticsVisible", false);

			_backupOption_IsAutoSave =		EditorPrefs.GetBool("AnyPortrait_AutoBackup_Enabled", true);
			_backupOption_BaseFolderName =	EditorPrefs.GetString("AnyPortrait_AutoBackup_Path", "AnyPortraitBackup");
			_backupOption_Minute =			EditorPrefs.GetInt("AnyPortrait_AutoBackup_Time", 30);
			
			_bonePose_BaseFolderName = EditorPrefs.GetString("AnyPortrait_BonePose_Path", "AnyPortraitBonePose");

			_startScreenOption_IsShowStartup = EditorPrefs.GetBool("AnyPortrait_StartScreen_IsShow", true);
			_startScreenOption_LastMonth = EditorPrefs.GetInt("AnyPortrait_StartScreen_LastMonth", 0);
			_startScreenOption_LastDay = EditorPrefs.GetInt("AnyPortrait_StartScreen_LastDay", 0);

			_updateLogScreen_LastVersion = EditorPrefs.GetInt("AnyPortrait_UpdateLogScreen_LastVersion", 0);

			_isBakeColorSpaceToGamma = EditorPrefs.GetBool("AnyPortrait_IsBakeColorSpace_ToGamma", true);

			_modLockOption_CalculateIfNotAddedOther = EditorPrefs.GetBool("AnyPortrait_ModLockOp_CalculateIfNotAddedOther",	false);			
			_modLockOption_ColorPreview_Lock =		EditorPrefs.GetBool("AnyPortrait_ModLockOp_ColorPreview_Lock",		false);
			_modLockOption_ColorPreview_Unlock =	EditorPrefs.GetBool("AnyPortrait_ModLockOp_ColorPreview_Unlock",	true);//<< True 기본값
			_modLockOption_BonePreview_Lock =		EditorPrefs.GetBool("AnyPortrait_ModLockOp_BonePreview_Lock",		false);
			_modLockOption_BonePreview_Unlock =		EditorPrefs.GetBool("AnyPortrait_ModLockOp_BonePreview_Unlock",		true);//<< True 기본값
			//_modLockOption_MeshPreview_Lock =		EditorPrefs.GetBool("AnyPortrait_ModLockOp_MeshPreview_Lock",		false);
			//_modLockOption_MeshPreview_Unlock =		EditorPrefs.GetBool("AnyPortrait_ModLockOp_MeshPreview_Unlock",		false);
			_modLockOption_ModListUI_Lock =			EditorPrefs.GetBool("AnyPortrait_ModLockOp_ModListUI_Lock",			false);
			_modLockOption_ModListUI_Unlock =		EditorPrefs.GetBool("AnyPortrait_ModLockOp_ModListUI_Unlock",		false);

			//_modLockOption_MeshPreviewColor = LoadColorPref("AnyPortrait_ModLockOp_MeshPreviewColor", DefauleColor_ModLockOpt_MeshPreview);
			_modLockOption_BonePreviewColor = LoadColorPref("AnyPortrait_ModLockOp_BonePreviewColor", DefauleColor_ModLockOpt_BonePreview);

			
			apGL.SetToneColor(_colorOption_OnionToneColor);
		}

		private void SaveColorPref(string label, Color color)
		{
			EditorPrefs.SetFloat(label + "_R", color.r);
			EditorPrefs.SetFloat(label + "_G", color.g);
			EditorPrefs.SetFloat(label + "_B", color.b);
			EditorPrefs.SetFloat(label + "_A", color.a);
		}

		private Color LoadColorPref(string label, Color defaultValue)
		{
			Color result = Color.black;
			result.r = EditorPrefs.GetFloat(label + "_R", defaultValue.r);
			result.g = EditorPrefs.GetFloat(label + "_G", defaultValue.g);
			result.b = EditorPrefs.GetFloat(label + "_B", defaultValue.b);
			result.a = EditorPrefs.GetFloat(label + "_A", defaultValue.a);
			return result;
		}

		public void RestoreEditorPref()
		{
			_language = LANGUAGE.English;

			//색상 옵션
			//_colorOption_Background = new Color(0.2f, 0.2f, 0.2f, 1.0f);
			//_colorOption_GridCenter = new Color(0.7f, 0.7f, 0.3f, 1.0f);
			//_colorOption_Grid = new Color(0.3f, 0.3f, 0.3f, 1.0f);

			//_colorOption_MeshEdge = new Color(1.0f, 0.5f, 0.0f, 0.9f);
			//_colorOption_MeshHiddenEdge = new Color(1.0f, 1.0f, 0.0f, 0.7f);
			//_colorOption_Outline = new Color(0.0f, 0.5f, 1.0f, 0.7f);
			//_colorOption_TransformBorder = new Color(0.0f, 1.0f, 1.0f, 1.0f);

			//_colorOption_VertColor_NotSelected = new Color(0.0f, 0.3f, 1.0f, 0.6f);
			//_colorOption_VertColor_Selected = new Color(1.0f, 0.0f, 0.0f, 1.0f);

			//_colorOption_GizmoFFDLine = new Color(1.0f, 0.5f, 0.2f, 0.9f);
			//_colorOption_GizmoFFDInnerLine = new Color(1.0f, 0.7f, 0.2f, 0.7f);

			//_colorOption_AtlasBorder = new Color(0.0f, 1.0f, 1.0f, 0.5f);

			_colorOption_Background = DefaultColor_Background;
			_colorOption_GridCenter = DefaultColor_GridCenter;
			_colorOption_Grid = DefaultColor_Grid;

			_colorOption_MeshEdge = DefaultColor_MeshEdge;
			_colorOption_MeshHiddenEdge = DefaultColor_MeshHiddenEdge;
			_colorOption_Outline = DefaultColor_Outline;
			_colorOption_TransformBorder = DefaultColor_TransformBorder;

			_colorOption_VertColor_NotSelected = DefaultColor_VertNotSelected;
			_colorOption_VertColor_Selected = DefaultColor_VertSelected;

			_colorOption_GizmoFFDLine = DefaultColor_GizmoFFDLine;
			_colorOption_GizmoFFDInnerLine = DefaultColor_GizmoFFDInnerLine;
			_colorOption_OnionToneColor = DefaultColor_OnionToneColor;

			_colorOption_AtlasBorder = DefaultColor_AtlasBorder;

			_guiOption_isFPSVisible = true;
			_guiOption_isStatisticsVisible = false;

			

			_backupOption_IsAutoSave = true;//자동 백업을 지원하는가
			_backupOption_BaseFolderName = "AnyPortraitBackup";//폴더를 지정해야한다. (프로젝트 폴더 기준 + 씬이름+에셋)
			_backupOption_Minute = 30;//기본은 30분마다 한번씩 저장한다.

			_bonePose_BaseFolderName = "AnyPortraitBonePose";

			_startScreenOption_IsShowStartup = true;
			_startScreenOption_LastMonth = 0;//Restore하면 다음에 실행할때 다시 나오도록 하자
			_startScreenOption_LastDay = 0;
			

			SaveEditorPref();
		}

		public static Color DefaultColor_Background { get { return new Color(0.2f, 0.2f, 0.2f, 1.0f); } }
		public static Color DefaultColor_GridCenter { get { return new Color(0.7f, 0.7f, 0.3f, 1.0f); } }
		public static Color DefaultColor_Grid { get { return new Color(0.3f, 0.3f, 0.3f, 1.0f); } }

		public static Color DefaultColor_MeshEdge { get { return new Color(1.0f, 0.5f, 0.0f, 0.9f); } }
		public static Color DefaultColor_MeshHiddenEdge { get { return new Color(1.0f, 1.0f, 0.0f, 0.7f); } }
		public static Color DefaultColor_Outline { get { return new Color(0.0f, 0.5f, 1.0f, 0.7f); } }
		public static Color DefaultColor_TransformBorder { get { return new Color(0.0f, 1.0f, 1.0f, 1.0f); } }

		public static Color DefaultColor_VertNotSelected { get { return new Color(0.0f, 0.3f, 1.0f, 0.6f); } }
		public static Color DefaultColor_VertSelected { get { return new Color(1.0f, 0.0f, 0.0f, 1.0f); } }

		public static Color DefaultColor_GizmoFFDLine { get { return new Color(1.0f, 0.5f, 0.2f, 0.9f); } }
		public static Color DefaultColor_GizmoFFDInnerLine { get { return new Color(1.0f, 0.7f, 0.2f, 0.7f); } }

		public static Color DefaultColor_OnionToneColor { get { return new Color(0.1f, 0.43f, 0.5f, 0.7f); } }

		public static Color DefaultColor_AtlasBorder { get { return new Color(0.0f, 1.0f, 1.0f, 0.5f); } }

		public static Color DefauleColor_ModLockOpt_MeshPreview { get { return new Color(1.0f, 0.45f, 0.1f, 0.8f); } }
		public static Color DefauleColor_ModLockOpt_BonePreview { get { return new Color(1.0f, 0.8f, 0.1f, 0.8f); } }

		//-------------------------------------------------------------------------------
		public string GetText(TEXT textType)
		{
			return Localization.GetText(textType);
		}


		public string GetTextFormat(TEXT textType, params object[] paramList)
		{
			return string.Format(Localization.GetText(textType), paramList);
		}


		public string GetUIWord(UIWORD uiWordType)
		{
			return Localization.GetUIWord(uiWordType);
		}


		public string GetUIWordFormat(UIWORD uiWordType, params object[] paramList)
		{
			return string.Format(Localization.GetUIWord(uiWordType), paramList);
		}

		// 화면 캡쳐 이벤트 요청과 처리
		//-----------------------------------------------------------------------
		public void ScreenCaptureRequest(apScreenCaptureRequest screenCaptureRequest)
		{
			
				
			
#if UNITY_EDITOR_OSX
			//OSX에선 딜레이를 줘야한다.
			_isScreenCaptureRequest_OSXReady = true;
			_isScreenCaptureRequest = false;
			_screenCaptureRequest_Count = SCREEN_CAPTURE_REQUEST_OSX_COUNT;
#else
			//Window에선 바로 캡쳐가능
			_isScreenCaptureRequest = true;
#endif
			
			_screenCaptureRequest = screenCaptureRequest;
		}

		//GUI에서 처리할 것
		private void ProcessScreenCapture()
		{
			_isScreenCaptureRequest = false;

			if(_screenCaptureRequest == null)
			{
				return;
			}

			apScreenCaptureRequest curRequest = _screenCaptureRequest;
			_screenCaptureRequest = null;//<<이건 일단 null

			//유효성 체크
			bool isValid = true;
			if (curRequest._editor != this ||
				Select.SelectionType != apSelection.SELECTION_TYPE.Overall ||
				Select.RootUnit == null ||
				curRequest._meshGroup == null ||
				Select.RootUnit._childMeshGroup != curRequest._meshGroup ||
				curRequest._funcCaptureResult == null ||
				_portrait == null)
			{
				isValid = false;
				
			}

			Texture2D result = null;
			if(isValid)
			{
				apMeshGroup targetMeshGroup = curRequest._meshGroup;
				apAnimClip targetAnimClip = curRequest._animClip;
				

				bool prevPhysics = _portrait._isPhysicsPlay_Editor;
				

				//업데이트를 한다.
				if(curRequest._isAnimClipRequest &&
					targetAnimClip != null)
				{
					//Debug.LogWarning("Capture Frame : " + curRequest._animFrame);
					//_portrait._isPhysicsPlay_Editor = false;//<<물리 금지
					//변경 : 옵션에 따라 바뀐다.
					_portrait._isPhysicsPlay_Editor = curRequest._isPhysics;
					targetAnimClip.SetFrame_Editor(curRequest._animFrame);
				}
				else
				{
					targetMeshGroup.RefreshForce();
				}

				result = Exporter.RenderToTexture(
													targetMeshGroup,
													curRequest._winPosX,
													curRequest._winPosY,
													curRequest._srcSizeWidth,
													curRequest._srcSizeHeight,
													curRequest._dstSizeWidth,
													curRequest._dstSizeHeight,
													curRequest._clearColor
													);

				//물리 복구
				_portrait._isPhysicsPlay_Editor = prevPhysics;

				//리! 턴!
				try
				{
					if (result != null)
					{
						curRequest._funcCaptureResult(
													true,
													result,
													curRequest._iProcessStep,
													curRequest._filePath,
													curRequest._loadKey);
					}
					else
					{
						curRequest._funcCaptureResult(
													false,
													null,
													curRequest._iProcessStep,
													curRequest._filePath,
													curRequest._loadKey);
					}

					result = null;
				}
				catch (Exception ex)
				{
					Debug.LogError("Capture Exception : " + ex);

					if (result != null)
					{
						UnityEngine.Object.DestroyImmediate(result);
					}
				}
			}
			else
			{
				if(result != null)
				{
					UnityEngine.Object.DestroyImmediate(result);
				}
				
				//처리가 실패했다.
				if(curRequest != null &&
					curRequest._funcCaptureResult != null)
				{
					//리! 턴!
					try
					{
						curRequest._funcCaptureResult(
							false,
							null,
							curRequest._iProcessStep,
							curRequest._filePath,
							curRequest._loadKey);
					}
					catch(Exception ex)
					{
						Debug.LogError("Capture Exception : " + ex);
					}
				}
			}
				
			//처리 끝!
			

		}

		// Modifier Lock 설정값에 대한 요청
		public bool GetModLockOption_CalculateIfNotAddedOther(apSelection.EX_EDIT exEdit)
		{
			if(exEdit == apSelection.EX_EDIT.General_Edit)
			{
				return _modLockOption_CalculateIfNotAddedOther;
			}
			return false;
		}

		public bool GetModLockOption_ColorPreview(apSelection.EX_EDIT exEdit)
		{
			if(exEdit == apSelection.EX_EDIT.ExOnly_Edit)		{ return _modLockOption_ColorPreview_Lock; }
			else if(exEdit == apSelection.EX_EDIT.General_Edit)	{ return _modLockOption_ColorPreview_Unlock; }
			return false;
		}

		
		public bool GetModLockOption_BonePreview(apSelection.EX_EDIT exEdit)
		{
			if(exEdit == apSelection.EX_EDIT.ExOnly_Edit)		{ return _modLockOption_BonePreview_Lock; }
			else if(exEdit == apSelection.EX_EDIT.General_Edit)	{ return _modLockOption_BonePreview_Unlock; }
			return false;
		}

		//public bool GetModLockOption_MeshPreview(apSelection.EX_EDIT exEdit)
		//{
		//	if(exEdit == apSelection.EX_EDIT.ExOnly_Edit)		{ return _modLockOption_MeshPreview_Lock; }
		//	else if(exEdit == apSelection.EX_EDIT.General_Edit)	{ return _modLockOption_MeshPreview_Unlock; }
		//	return false;
		//}

		public bool GetModLockOption_ListUI(apSelection.EX_EDIT exEdit)
		{
			if(exEdit == apSelection.EX_EDIT.ExOnly_Edit)		{ return _modLockOption_ModListUI_Lock; }
			else if(exEdit == apSelection.EX_EDIT.General_Edit)	{ return _modLockOption_ModListUI_Unlock; }
			return false;
		}

		//public Color GetModLockOption_MeshPreviewColor()
		//{
		//	return _modLockOption_MeshPreviewColor;
		//}

		public Color GetModLockOption_BonePreviewColor()
		{
			return _modLockOption_BonePreviewColor;
		}
	}

}