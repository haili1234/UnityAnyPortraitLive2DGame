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
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public static class apGL
	{
		//private static Material _mat_Color = null;
		//private static Material _mat_Texture = null;

		//private static int _windowPosX = 0;
		//private static int _windowPosY = 0;
		public static int _windowWidth = 0;
		public static int _windowHeight = 0;
		public static int _totalEditorWidth = 0;
		public static int _totalEditorHeight = 0;
		public static Vector2 _scrol_NotCalculated = Vector2.zero;
		public static int _posX_NotCalculated = 0;
		public static int _posY_NotCalculated = 0;

		public static Vector2 _windowScroll = Vector2.zero;

		public static float _zoom = 1.0f;
		public static float Zoom { get { return _zoom; } }

		public static Vector2 WindowSize { get { return new Vector2(_windowWidth, _windowHeight); } }
		public static Vector2 WindowSizeHalf { get { return new Vector2(_windowWidth / 2, _windowHeight / 2); } }

		private static GUIStyle _textStyle = GUIStyle.none;

		private static Vector4 _glScreenClippingSize = Vector4.zero;

		[Flags]
		public enum RENDER_TYPE : int
		{
			Default = 0,

			ShadeAllMesh = 1,
			AllMesh = 2,

			Vertex = 4,

			Outlines = 8,
			AllEdges = 16,

			VolumeWeightColor = 32,
			PhysicsWeightColor = 64,
			BoneRigWeightColor = 128,

			TransformBorderLine = 256,
			PolygonOutline = 512,

			ToneColor = 1024,
			BoneOutlineOnly = 2048
		}

		private static Color _textureColor_Gray = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		private static Color _textureColor_Shade = new Color(0.3f, 0.3f, 0.3f, 1.0f);

		//private static Color _vertColor_NotSelected = new Color(0.0f, 0.3f, 1.0f, 0.6f);
		//private static Color _vertColor_Selected = new Color(1.0f, 0.0f, 0.0f, 1.0f);
		private static Color _vertColor_NextSelected = new Color(1.0f, 0.0f, 1.0f, 0.6f);
		private static Color _vertColor_Outline = new Color(0.0f, 0.0f, 0.0f, 0.8f);
		private static Color _vertColor_Outline_White = new Color(1.0f, 1.0f, 1.0f, 0.8f);

		//Weight인 경우 보(0)-파(25)-초(50)-노(75)-빨(100)로 이어진다.
		private static Color _vertColor_Weighted_0 = new Color(1.0f, 0.0f, 1.0f, 1.0f);
		private static Color _vertColor_Weighted_25 = new Color(0.0f, 0.5f, 1.0f, 1.0f);
		private static Color _vertColor_Weighted_50 = new Color(0.0f, 1.0f, 0.5f, 1.0f);
		private static Color _vertColor_Weighted_75 = new Color(1.0f, 1.0f, 0.0f, 1.0f);


		private static Color _vertColor_Weighted3_0 = new Color(0.0f, 0.0f, 0.0f, 1.0f);
		private static Color _vertColor_Weighted3_25 = new Color(0.0f, 0.0f, 1.0f, 1.0f);
		private static Color _vertColor_Weighted3_50 = new Color(1.0f, 1.0f, 0.0f, 1.0f);
		private static Color _vertColor_Weighted3_75 = new Color(1.0f, 0.5f, 0.0f, 1.0f);
		private static Color _vertColor_Weighted3_100 = new Color(1.0f, 0.0f, 0.0f, 1.0f);

		private static Color _vertColor_Weighted3Vert_0 = new Color(0.2f, 0.2f, 0.2f, 1.0f);
		private static Color _vertColor_Weighted3Vert_25 = new Color(0.2f, 0.2f, 1.0f, 1.0f);
		private static Color _vertColor_Weighted3Vert_50 = new Color(1.0f, 1.0f, 0.2f, 1.0f);
		private static Color _vertColor_Weighted3Vert_75 = new Color(1.0f, 0.5f, 0.2f, 1.0f);
		private static Color _vertColor_Weighted3Vert_100 = new Color(1.0f, 0.2f, 0.2f, 1.0f);


		private static Color _vertColor_Weighted4_0_Null = new Color(0.0f, 0.0f, 0.0f, 1.0f);
		private static Color _vertColor_Weighted4_0 = new Color(1.0f, 0.5f, 0.0f, 1.0f);
		private static Color _vertColor_Weighted4_33 = new Color(0.0f, 1.0f, 0.0f, 1.0f);
		private static Color _vertColor_Weighted4_66 = new Color(0.0f, 1.0f, 1.0f, 1.0f);
		private static Color _vertColor_Weighted4_100 = new Color(1.0f, 0.0f, 1.0f, 1.0f);

		private static Color _vertColor_Weighted4Vert_Null = new Color(0.2f, 0.2f, 0.2f, 1.0f);
		private static Color _vertColor_Weighted4Vert_0 = new Color(1.0f, 0.5f, 0.2f, 1.0f);
		private static Color _vertColor_Weighted4Vert_33 = new Color(0.2f, 1.0f, 0.2f, 1.0f);
		private static Color _vertColor_Weighted4Vert_66 = new Color(0.2f, 1.0f, 1.0f, 1.0f);
		private static Color _vertColor_Weighted4Vert_100 = new Color(1.0f, 0.2f, 1.0f, 1.0f);


		//private static Color _lineColor_Tri = new Color(1.0f, 0.5f, 0.0f, 0.9f);
		//private static Color _lineColor_HiddenEdge = new Color(1.0f, 1.0f, 0.0f, 0.7f);
		//private static Color _lineColor_Outline = new Color(0.0f, 0.5f, 1.0f, 0.7f);
		//private static Color _lineColor_TFBorder = new Color(0.0f, 1.0f, 1.0f, 1.0f);

		private static Color _lineColor_BoneOutline = new Color(1.0f, 0.0f, 0.2f, 0.8f);
		private static Color _lineColor_BoneOutlineRollOver = new Color(1.0f, 0.2f, 0.0f, 0.5f);

		private static Texture2D _img_VertPhysicMain = null;
		private static Texture2D _img_VertPhysicConstraint = null;

		private static Color _toneColor = new Color(0.1f, 0.3f, 0.5f, 0.7f);



		//------------------------------------------------------------------------
		public class MaterialBatch
		{
			public enum MatType
			{
				None, Color,
				Texture_Normal, Texture_VColorAdd,
				//MaskedTexture,//<<구형 방식
				MaskOnly, Clipped,
				GUITexture,
				ToneColor_Normal, ToneColor_Clipped,
				Alpha2White,//Capture용 Shader
			}
			private Material _mat_Color = null;
			private Material _mat_MaskOnly = null;

			//추가 : 일반 Texture Transparent같지만 GUI 전용이며 _Color가 없고 Vertex Color를 사용하여 Batch하기에 좋다.
			private Material _mat_GUITexture = null;

			private Material[] _mat_Texture_Normal = null;
			private Material[] _mat_Texture_VColorAdd = null;
			//private Material[] _mat_MaskedTexture = null;
			private Material[] _mat_Clipped = null;

			private Material _mat_ToneColor_Normal = null;
			private Material _mat_ToneColor_Clipped = null;

			private Material _mat_Alpha2White = null;



			private MatType _matType = MatType.None;

			//마지막 입력 값
			private Vector4 _glScreenClippingSize = Vector4.zero;

			public Color _color = Color.black;
			private Texture2D _texture = null;

			//마스크 버전은 좀 많다..
			private RenderTexture _renderTexture = null;
			private int _renderTextureSize_Width = -1;
			private int _renderTextureSize_Height = -1;
			public RenderTexture RenderTex { get { return _renderTexture; } }

			//private Color _clipColor_1 = Color.black;
			//private Color _clipColor_2 = Color.black;
			//private Color _clipColor_3 = Color.black;

			//private Texture2D _clipTexture_1 = null;
			//private Texture2D _clipTexture_2 = null;
			//private Texture2D _clipTexture_3 = null;

			public const int ALPHABLEND = 0;
			public const int ADDITIVE = 1;
			public const int SOFT_ADDITIVE = 2;
			public const int MULTIPLICATIVE = 3;

			//private static Color[] ShaderTypeColor = new Color[] {  new Color(1.0f, 0.0f, 0.0f, 0.0f),
			//													new Color(0.0f, 1.0f, 0.0f, 0.0f),
			//													new Color(0.0f, 0.0f, 1.0f, 0.0f),
			//													new Color(0.0f, 0.0f, 0.0f, 1.0f)};

			private int _shaderType_Main = 0;
			//private int _shaderType_Clip1 = 0;
			//private int _shaderType_Clip2 = 0;
			//private int _shaderType_Clip3 = 0;
			//private Color _shaderTypeColor_Clip1 = new Color(1.0f, 0.0f, 0.0f, 0.0f);
			//private Color _shaderTypeColor_Clip2 = new Color(1.0f, 0.0f, 0.0f, 0.0f);
			//private Color _shaderTypeColor_Clip3 = new Color(1.0f, 0.0f, 0.0f, 0.0f);

			//private bool _isNeedReset = true;

			public MaterialBatch()
			{

			}

			public void SetShader(Shader shader_Color,
									Shader[] shader_Texture_Normal_Set,
									Shader[] shader_Texture_VColorAdd_Set,
									//Shader[] shader_MaskedTexture_Set,
									Shader shader_MaskOnly,
									Shader[] shader_Clipped_Set,
									Shader shader_GUITexture,
									Shader shader_ToneColor_Normal,
									Shader shader_ToneColor_Clipped,
									Shader shader_Alpha2White
									)
			{
				//_mat_Color = mat_Color;
				//_mat_Texture = mat_Texture;
				//_mat_MaskedTexture = mat_MaskedTexture;

				_mat_Color = new Material(shader_Color);
				_mat_Color.color = new Color(1, 1, 1, 1);

				_mat_MaskOnly = new Material(shader_MaskOnly);
				_mat_MaskOnly.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

				//추가 : GUI용 텍스쳐
				_mat_GUITexture = new Material(shader_GUITexture);


				//AlphaBlend, Add, SoftAdditive
				_mat_Texture_Normal = new Material[4];
				_mat_Texture_VColorAdd = new Material[4];
				//_mat_MaskedTexture = new Material[4];
				_mat_Clipped = new Material[4];

				for (int i = 0; i < 4; i++)
				{
					_mat_Texture_Normal[i] = new Material(shader_Texture_Normal_Set[i]);
					_mat_Texture_Normal[i].color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

					_mat_Texture_VColorAdd[i] = new Material(shader_Texture_VColorAdd_Set[i]);
					_mat_Texture_VColorAdd[i].color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

					//_mat_MaskedTexture[i] = new Material(shader_MaskedTexture_Set[i]);
					//_mat_MaskedTexture[i].color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

					_mat_Clipped[i] = new Material(shader_Clipped_Set[i]);
					_mat_Clipped[i].color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}

				_mat_ToneColor_Normal = new Material(shader_ToneColor_Normal);
				_mat_ToneColor_Normal.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

				_mat_ToneColor_Clipped = new Material(shader_ToneColor_Clipped);
				_mat_ToneColor_Clipped.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

				_mat_Alpha2White = new Material(shader_Alpha2White);
				_mat_Alpha2White.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			}

			#region [미사용 코드]
			//public void SetMaterialType_Color()
			//{
			//	_matType = MatType.Color;
			//}
			//public void SetMaterialType_Texture_Normal(apPortrait.SHADER_TYPE shaderType)
			//{
			//	_matType = MatType.Texture_Normal;
			//	_shaderType_Main = (int)shaderType;
			//}
			//public void SetMaterialType_Texture_VColorAdd(apPortrait.SHADER_TYPE shaderType)
			//{
			//	_matType = MatType.Texture_VColorAdd;
			//	_shaderType_Main = (int)shaderType;
			//}
			//public void SetMaterialType_MaskedTexture(apPortrait.SHADER_TYPE shaderTypeMain,
			//											apPortrait.SHADER_TYPE shaderTypeClip1,
			//											apPortrait.SHADER_TYPE shaderTypeClip2,
			//											apPortrait.SHADER_TYPE shaderTypeClip3)
			//{
			//	_matType = MatType.MaskedTexture;
			//	_shaderType_Main = (int)shaderTypeMain;
			//	_shaderType_Clip1 = (int)shaderTypeClip1;
			//	_shaderType_Clip2 = (int)shaderTypeClip2;
			//	_shaderType_Clip3 = (int)shaderTypeClip3;

			//	_shaderTypeColor_Clip1 = ShaderTypeColor[_shaderType_Clip1];
			//	_shaderTypeColor_Clip2 = ShaderTypeColor[_shaderType_Clip2];
			//	_shaderTypeColor_Clip3 = ShaderTypeColor[_shaderType_Clip3];
			//}

			//public void SetMaterialType_MaskAndClipped(apPortrait.SHADER_TYPE shaderTypeMain,
			//											apPortrait.SHADER_TYPE shaderTypeClip1,
			//											apPortrait.SHADER_TYPE shaderTypeClip2,
			//											apPortrait.SHADER_TYPE shaderTypeClip3)
			//{
			//	_shaderType_Main = (int)shaderTypeMain;
			//	_shaderType_Clip1 = (int)shaderTypeClip1;
			//	_shaderType_Clip2 = (int)shaderTypeClip2;
			//	_shaderType_Clip3 = (int)shaderTypeClip3;

			//	_shaderTypeColor_Clip1 = ShaderTypeColor[_shaderType_Clip1];
			//	_shaderTypeColor_Clip2 = ShaderTypeColor[_shaderType_Clip2];
			//	_shaderTypeColor_Clip3 = ShaderTypeColor[_shaderType_Clip3];
			//} 
			#endregion

			public void SetClippingSize(Vector4 screenSize)
			{
				_glScreenClippingSize = screenSize;

				switch (_matType)
				{
					case MatType.Color:
						_mat_Color.SetVector("_ScreenSize", _glScreenClippingSize);
						break;

					case MatType.Texture_Normal:
						_mat_Texture_Normal[_shaderType_Main].SetVector("_ScreenSize", _glScreenClippingSize);
						break;

					case MatType.Texture_VColorAdd:
						_mat_Texture_VColorAdd[_shaderType_Main].SetVector("_ScreenSize", _glScreenClippingSize);
						break;

					//case MatType.MaskedTexture:
					//	_mat_MaskedTexture[_shaderType_Main].SetVector("_ScreenSize", _glScreenClippingSize);
					//	break;

					case MatType.Clipped:
						_mat_Clipped[_shaderType_Main].SetVector("_ScreenSize", _glScreenClippingSize);
						break;

					case MatType.MaskOnly:
						_mat_MaskOnly.SetVector("_ScreenSize", _glScreenClippingSize);
						break;

					case MatType.GUITexture:
						_mat_GUITexture.SetVector("_ScreenSize", _glScreenClippingSize);
						break;

					case MatType.ToneColor_Normal:
						_mat_ToneColor_Normal.SetVector("_ScreenSize", _glScreenClippingSize);
						break;

					case MatType.ToneColor_Clipped:
						_mat_ToneColor_Clipped.SetVector("_ScreenSize", _glScreenClippingSize);
						break;

					case MatType.Alpha2White:
						_mat_Alpha2White.SetVector("_ScreenSize", _glScreenClippingSize);
						break;
				}



				//GL.Flush();
			}


			public void SetClippingSizeToAllMaterial(Vector4 screenSize)
			{
				_glScreenClippingSize = screenSize;

				_mat_Color.SetVector("_ScreenSize", _glScreenClippingSize);
				_mat_Texture_Normal[_shaderType_Main].SetVector("_ScreenSize", _glScreenClippingSize);
				_mat_Texture_VColorAdd[_shaderType_Main].SetVector("_ScreenSize", _glScreenClippingSize);
				//_mat_MaskedTexture[_shaderType_Main].SetVector("_ScreenSize", _glScreenClippingSize);
				_mat_Clipped[_shaderType_Main].SetVector("_ScreenSize", _glScreenClippingSize);
				_mat_MaskOnly.SetVector("_ScreenSize", _glScreenClippingSize);
				_mat_GUITexture.SetVector("_ScreenSize", _glScreenClippingSize);
				_mat_ToneColor_Normal.SetVector("_ScreenSize", _glScreenClippingSize);
				_mat_ToneColor_Clipped.SetVector("_ScreenSize", _glScreenClippingSize);
				_mat_Alpha2White.SetVector("_ScreenSize", _glScreenClippingSize);
			}

			/// <summary>
			/// RenderTexture를 사용하는 GL계열에서는 이 함수를 윈도우 크기 호출시에 같이 호출한다.
			/// </summary>
			/// <param name="windowWidth"></param>
			/// <param name="windowHeight"></param>
			public void CheckMaskTexture(int windowWidth, int windowHeight)
			{
				//if(_renderTexture == null || _renderTextureSize_Width != windowWidth || _renderTextureSize_Height != windowHeight)
				//{
				//	if(_renderTexture != null)
				//	{
				//		//UnityEngine.Object.DestroyImmediate(_renderTexture);
				//		RenderTexture.ReleaseTemporary(_renderTexture);
				//		_renderTexture = null;
				//	}
				//	//_renderTexture = new RenderTexture(windowWidth, windowHeight, 24);
				//	_renderTexture = RenderTexture.GetTemporary(windowWidth, windowHeight, 24);
				//	_renderTexture.wrapMode = TextureWrapMode.Clamp;
				//	_renderTextureSize_Width = windowWidth;
				//	_renderTextureSize_Height = windowHeight;
				//}

				_renderTextureSize_Width = windowWidth;
				_renderTextureSize_Height = windowHeight;
			}

			public void SetPass_Color()
			{
				_mat_Color.SetPass(0);

				_mat_Color.color = new Color(1, 1, 1, 1);
				_matType = MatType.Color;

				//GL.sRGBWrite = true;
			}

			public void SetPass_GUITexture(Texture2D texture)
			{
				_texture = texture;
				_mat_GUITexture.SetTexture("_MainTex", _texture);

				_mat_GUITexture.SetPass(0);
				_matType = MatType.GUITexture;

				//GL.sRGBWrite = true;
			}

			public void SetPass_Texture_Normal(Color color, Texture2D texture, apPortrait.SHADER_TYPE shaderType)
			{
				_shaderType_Main = (int)shaderType;
				_color = color;
				_mat_Texture_Normal[_shaderType_Main].SetColor("_Color", _color);

				_texture = texture;
				_mat_Texture_Normal[_shaderType_Main].SetTexture("_MainTex", _texture);

				_mat_Texture_Normal[_shaderType_Main].SetPass(0);
				_matType = MatType.Texture_Normal;

				//GL.sRGBWrite = true;
				//_isNeedReset = false;
			}

			public void SetPass_ToneColor_Normal(Color color, Texture2D texture)
			{
				_matType = MatType.ToneColor_Normal;
				_color = color;
				
				_mat_ToneColor_Normal.SetColor("_Color", _color);

				_texture = texture;
				_mat_ToneColor_Normal.SetTexture("_MainTex", _texture);
				_mat_ToneColor_Normal.SetPass(0);
				
				//GL.sRGBWrite = true;

				//_isNeedReset = false;
			}

			public void SetPass_Texture_VColor(Color color, Texture2D texture, float vertColorRatio, apPortrait.SHADER_TYPE shaderType)
			{
				_shaderType_Main = (int)shaderType;

				_color = color;
				_mat_Texture_VColorAdd[_shaderType_Main].SetColor("_Color", _color);

				_texture = texture;
				_mat_Texture_VColorAdd[_shaderType_Main].SetTexture("_MainTex", _texture);

				_mat_Texture_VColorAdd[_shaderType_Main].SetFloat("_vColorITP", vertColorRatio);
				//_isNeedReset = true;

				_mat_Texture_VColorAdd[_shaderType_Main].SetPass(0);
				_matType = MatType.Texture_VColorAdd;

				//GL.sRGBWrite = true;
				//_isNeedReset = false;
			}

			

			public void SetPass_Mask(Color color, Texture2D texture,
									float vertColorRatio, apPortrait.SHADER_TYPE shaderType,
									bool isRenderMask)
			{
				_shaderType_Main = (int)shaderType;

				_color = color;
				_texture = texture;

				if (isRenderMask)
				{
					//RenderTexture로 만든다.
					_matType = MatType.MaskOnly;

					//RenderTexture를 활성화한다.
					_renderTexture = RenderTexture.GetTemporary(_renderTextureSize_Width, _renderTextureSize_Height, 8);
					_renderTexture.wrapMode = TextureWrapMode.Clamp;

					//RenderTexture를 사용
					RenderTexture.active = _renderTexture;

					//[중요] Temp RenderTexture는 색상 초기화가 안되어있다. 꼭 해준다.
					GL.Clear(true, true, Color.clear, 0.0f);


					_mat_MaskOnly.SetColor("_Color", _color);
					_mat_MaskOnly.SetTexture("_MainTex", _texture);
					_mat_MaskOnly.SetFloat("_vColorITP", vertColorRatio);

					_mat_MaskOnly.SetPass(0);
				}
				else
				{
					_matType = MatType.Texture_VColorAdd;

					_mat_Texture_VColorAdd[_shaderType_Main].SetColor("_Color", _color);
					_mat_Texture_VColorAdd[_shaderType_Main].SetTexture("_MainTex", _texture);
					_mat_Texture_VColorAdd[_shaderType_Main].SetFloat("_vColorITP", vertColorRatio);

					_mat_Texture_VColorAdd[_shaderType_Main].SetPass(0);
				}
			}

			public void SetPass_Clipped(Color color, Texture2D texture, float vertColorRatio, apPortrait.SHADER_TYPE shaderType, Color parentColor)
			{
				_matType = MatType.Clipped;
				_shaderType_Main = (int)shaderType;

				_color = color;
				_texture = texture;
				_mat_Clipped[_shaderType_Main].SetColor("_Color", _color);
				_mat_Clipped[_shaderType_Main].SetTexture("_MainTex", _texture);
				_mat_Clipped[_shaderType_Main].SetFloat("_vColorITP", vertColorRatio);

				_mat_Clipped[_shaderType_Main].SetTexture("_MaskRenderTexture", _renderTexture);//<<Mask를 넣자
				_mat_Clipped[_shaderType_Main].SetColor("_MaskColor", parentColor);


				_mat_Clipped[_shaderType_Main].SetPass(0);

				//Debug.Log("SetPass Clipped");
			}




			public void SetPass_Mask_ToneColor(Color color, Texture2D texture,
									bool isRenderMask)
			{
				_color = color;
				_texture = texture;

				if (isRenderMask)
				{
					//RenderTexture로 만든다.
					_matType = MatType.MaskOnly;

					//RenderTexture를 활성화한다.
					_renderTexture = RenderTexture.GetTemporary(_renderTextureSize_Width, _renderTextureSize_Height, 8);
					_renderTexture.wrapMode = TextureWrapMode.Clamp;

					//RenderTexture를 사용
					RenderTexture.active = _renderTexture;

					//[중요] Temp RenderTexture는 색상 초기화가 안되어있다. 꼭 해준다.
					GL.Clear(true, true, Color.clear, 0.0f);


					_mat_MaskOnly.SetColor("_Color", _color);
					_mat_MaskOnly.SetTexture("_MainTex", _texture);
					_mat_MaskOnly.SetFloat("_vColorITP", 0.0f);

					_mat_MaskOnly.SetPass(0);
				}
				else
				{
					_matType = MatType.ToneColor_Normal;

					_mat_ToneColor_Normal.SetColor("_Color", _color);
					_mat_ToneColor_Normal.SetTexture("_MainTex", _texture);
					
					_mat_ToneColor_Normal.SetPass(0);
				}
			}



			public void SetPass_Clipped_ToneColor(Color color, Texture2D texture, Color parentColor)
			{
				_matType = MatType.ToneColor_Clipped;

				_color = color;
				_texture = texture;
				_mat_ToneColor_Clipped.SetColor("_Color", _color);
				_mat_ToneColor_Clipped.SetTexture("_MainTex", _texture);
				_mat_ToneColor_Clipped.SetTexture("_MaskRenderTexture", _renderTexture);//<<Mask를 넣자
				_mat_ToneColor_Clipped.SetColor("_MaskColor", parentColor);


				_mat_ToneColor_Clipped.SetPass(0);

				//Debug.Log("SetPass Clipped");
			}

			public void SetPass_ClippedWithMaskedTexture(Color color, Texture2D texture, float vertColorRatio,
														apPortrait.SHADER_TYPE shaderType, Color parentColor,
														Texture2D maskedTexture
				)
			{
				_matType = MatType.Clipped;
				_shaderType_Main = (int)shaderType;

				_color = color;
				_texture = texture;
				_mat_Clipped[_shaderType_Main].SetColor("_Color", _color);
				_mat_Clipped[_shaderType_Main].SetTexture("_MainTex", _texture);
				_mat_Clipped[_shaderType_Main].SetFloat("_vColorITP", vertColorRatio);

				_mat_Clipped[_shaderType_Main].SetTexture("_MaskRenderTexture", maskedTexture);//<<Mask를 넣자
				_mat_Clipped[_shaderType_Main].SetColor("_MaskColor", parentColor);

				_mat_Clipped[_shaderType_Main].SetPass(0);

				//Debug.Log("SetPass Clipped");
			}

			public void SetPass_Alpha2White(Color color, Texture2D texture)
			{
				_matType = MatType.Alpha2White;
				_shaderType_Main = 0;

				_color = color;
				_texture = texture;

				_mat_Alpha2White.SetColor("_Color", _color);
				_mat_Alpha2White.SetTexture("_MainTex", _texture);
				_mat_Alpha2White.SetPass(0);
			}

			/// <summary>
			/// MultiPass에서 사용한 RenderTexture를 해제한다.
			/// 다만, 삭제하지는 않는다.
			/// </summary>
			public void DeactiveRenderTexture()
			{
				RenderTexture.active = null;
			}
			/// <summary>
			/// MultiPass의 모든 과정이 끝나면 사용했던 RenderTexture를 해제한다.
			/// </summary>
			public void ReleaseRenderTexture()
			{
				if (_renderTexture != null)
				{
					RenderTexture.active = null;
					RenderTexture.ReleaseTemporary(_renderTexture);
					_renderTexture = null;
				}
			}
			public bool IsNotReady()
			{
				return (_mat_Color == null
					|| _mat_Texture_Normal == null
					|| _mat_Texture_VColorAdd == null
					//|| _mat_MaskedTexture == null
					|| _mat_Clipped == null
					|| _mat_MaskOnly == null
					|| _mat_GUITexture == null
					|| _mat_ToneColor_Normal == null
					|| _mat_ToneColor_Clipped == null
					|| _mat_Alpha2White == null);
			}

			

		}

		private static MaterialBatch _matBatch = new MaterialBatch();
		public static MaterialBatch MatBatch { get { return _matBatch; } }
		//------------------------------------------------------------------------

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
			//_mat_Color = mat_Color;
			//_mat_Texture = mat_Texture;

			//_matBatch.SetMaterial(mat_Color, mat_Texture, mat_MaskedTexture);
			_matBatch.SetShader(shader_Color,
								shader_Texture_Normal_Set,
								shader_Texture_VColorAdd_Set,
								//shader_MaskedTexture_Set,
								shader_MaskOnly,
								shader_Clipped_Set,
								shader_GUITexture,
								shader_ToneColor_Normal,
								shader_ToneColor_Clipped,
								shader_Alpha2White);
		}

		public static void SetTexture(Texture2D img_VertPhysicMain, Texture2D img_VertPhysicConstraint)
		{
			_img_VertPhysicMain = img_VertPhysicMain;
			_img_VertPhysicConstraint = img_VertPhysicConstraint;
		}


		public static void SetWindowSize(int windowWidth, int windowHeight, Vector2 scroll, float zoom,
			int posX, int posY, int totalEditorWidth, int totalEditorHeight)
		{
			_windowWidth = windowWidth;
			_windowHeight = windowHeight;
			_scrol_NotCalculated = scroll;
			_windowScroll.x = scroll.x * _windowWidth * 0.1f;
			_windowScroll.y = scroll.y * windowHeight * 0.1f;
			_totalEditorWidth = totalEditorWidth;
			_totalEditorHeight = totalEditorHeight;

			_posX_NotCalculated = posX;
			_posY_NotCalculated = posY;

			_zoom = zoom;

			totalEditorHeight += 30;
			posY += 30;
			posX += 5;
			windowWidth -= 25;
			windowHeight -= 20;

			//_windowPosX = posX;
			//_windowPosY = posY;

			//float leftMargin = posX;
			//float rightMargin = totalEditorWidth - (posX + windowWidth);
			//float topMargin = posY;
			//float bottomMargin = totalEditorHeight - (posY + windowHeight);

			_glScreenClippingSize.x = (float)posX / (float)totalEditorWidth;
			_glScreenClippingSize.y = (float)(posY) / (float)totalEditorHeight;
			_glScreenClippingSize.z = (float)(posX + windowWidth) / (float)totalEditorWidth;
			_glScreenClippingSize.w = (float)(posY + windowHeight) / (float)totalEditorHeight;

			_matBatch.CheckMaskTexture(_windowWidth, _windowHeight);
		}

		public static void SetToneColor(Color toneColor)
		{
			_toneColor = toneColor;
		}


		public static Vector2 World2GL(Vector2 pos)
		{
			return new Vector2(
				(pos.x * _zoom) + (_windowWidth * 0.5f)
				- _windowScroll.x,

				(_windowHeight - (pos.y * _zoom)) - (_windowHeight * 0.5f)
				- _windowScroll.y
				);
		}

		public static Vector2 GL2World(Vector2 glPos)
		{
			return new Vector2(
				(glPos.x + (_windowScroll.x) - (_windowWidth * 0.5f)) / _zoom,
				(-1.0f * (glPos.y + _windowScroll.y + (_windowHeight * 0.5f) - (_windowHeight))) / _zoom
				);
		}

		private static bool IsVertexClipped(Vector2 posGL)
		{
			return (posGL.x < 1.0f || posGL.x > _windowWidth - 1 ||
									posGL.y < 1.0f || posGL.y > _windowHeight - 1);
		}

		private static bool Is2VertexClippedAll(Vector2 pos1GL, Vector2 pos2GL)
		{
			bool isPos1Clipped = IsVertexClipped(pos1GL);

			bool isPos2Clipped = IsVertexClipped(pos2GL);


			if (!isPos1Clipped || !isPos2Clipped)
			{
				//둘중 하나라도 안에 들어있다.
				return false;
			}


			//두 점이 밖에 나갔어도, 중간 점이 걸쳐서 들어올 수 있다.
			Vector2 posDir = pos2GL - pos1GL;
			for (int i = 1; i < 5; i++)
			{
				Vector2 posSub = pos1GL + posDir * ((float)i / 5.0f);

				bool isPosSubClipped = IsVertexClipped(posSub);

				//중간점 하나가 들어와있다.
				if (!isPosSubClipped)
				{
					return false;
				}
			}
			return true;
		}

		private static Vector2 GetClippedVertex(Vector2 posTargetGL, Vector2 posBaseGL)
		{
			Vector2 pos1_Real = posTargetGL;
			Vector2 pos2_Real = posBaseGL;

			Vector2 dir1To2 = (pos2_Real - pos1_Real).normalized;
			Vector2 dir2To1 = -dir1To2;

			if (pos1_Real.x < 0.0f || pos1_Real.x > _windowWidth ||
				pos1_Real.y < 0.0f || pos1_Real.y > _windowHeight)
			{
				//2 + dir(2 -> 1) * t = 1'
				//dir * t = 1' - 2
				//t = (1' - 2) / dir

				float tX = 0.0f;
				float tY = 0.0f;
				float tResult = 0.0f;

				bool isClipX = false;
				bool isClipY = false;


				if (posTargetGL.x < 0.0f)
				{
					pos1_Real.x = 0.0f;
					isClipX = true;
				}
				else if (posTargetGL.x > _windowWidth)
				{
					pos1_Real.x = _windowWidth;
					isClipX = true;
				}

				if (posTargetGL.y < 0.0f)
				{
					pos1_Real.y = 0.0f;
					isClipY = true;
				}
				else if (posTargetGL.y > _windowHeight)
				{
					pos1_Real.y = _windowHeight;
					isClipY = true;
				}

				if (isClipX)
				{
					if (Mathf.Abs(dir2To1.x) > 0.0f)
					{ tX = (pos1_Real.x - pos2_Real.x) / dir2To1.x; }
					else
					{ return new Vector2(-100.0f, -100.0f); }//둘다 나갔다...
				}

				if (isClipY)
				{
					if (Mathf.Abs(dir2To1.y) > 0.0f)
					{ tY = (pos1_Real.y - pos2_Real.y) / dir2To1.y; }
					else
					{ return new Vector2(-100.0f, -100.0f); }//둘다 나갔다...
				}
				if (isClipX && isClipY)
				{
					if (Mathf.Abs(tX) < Mathf.Abs(tY))
					{
						tResult = tX;
					}
					else
					{
						tResult = tY;
					}
				}
				else if (isClipX)
				{
					tResult = tX;
				}
				else if (isClipY)
				{
					tResult = tY;
				}

				//2 + dir(2 -> 1) * t = 1'
				pos1_Real = pos2_Real + dir2To1 * tResult;
				return pos1_Real;
			}
			else
			{
				return pos1_Real;
			}
		}


		private static Vector2 GetClippedVertexNoBase(Vector2 posTargetGL)
		{
			Vector2 pos1_Real = posTargetGL;

			if (pos1_Real.x < 0.0f || pos1_Real.x > _windowWidth ||
				pos1_Real.y < 0.0f || pos1_Real.y > _windowHeight)
			{
				if (posTargetGL.x < 0.0f)
				{
					pos1_Real.x = 0.0f;
				}
				else if (posTargetGL.x > _windowWidth)
				{
					pos1_Real.x = _windowWidth;
				}

				if (posTargetGL.y < 0.0f)
				{
					pos1_Real.y = 0.0f;
				}
				else if (posTargetGL.y > _windowHeight)
				{
					pos1_Real.y = _windowHeight;
				}
				return pos1_Real;
			}
			else
			{
				return pos1_Real;
			}
		}

		// 최적화형
		//-------------------------------------------------------------------------------
		public static void BeginBatch_ColoredPolygon()
		{
			_matBatch.SetPass_Color();
			_matBatch.SetClippingSize(_glScreenClippingSize);

			GL.Begin(GL.TRIANGLES);
		}

		public static void BeginBatch_ColoredLine()
		{
			_matBatch.SetPass_Color();
			_matBatch.SetClippingSize(_glScreenClippingSize);

			GL.Begin(GL.LINES);
		}

		public static void EndBatch()
		{
			GL.End();
		}

		public static void RefreshScreenSizeToBatch()
		{
			_matBatch.SetClippingSizeToAllMaterial(_glScreenClippingSize);
		}
		//-------------------------------------------------------------------------------
		// Draw Line
		//-------------------------------------------------------------------------------
		public static void DrawLine(Vector2 pos1, Vector2 pos2, Color color)
		{
			DrawLine(pos1, pos2, color, true);
		}

		public static void DrawLine(Vector2 pos1, Vector2 pos2, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{ return; }

			if (Vector2.Equals(pos1, pos2))
			{ return; }

			pos1 = World2GL(pos1);
			pos2 = World2GL(pos2);

			if (isNeedResetMat)
			{
				_matBatch.SetPass_Color();
				_matBatch.SetClippingSize(_glScreenClippingSize);

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

		public static void DrawLineGL(Vector2 pos1_GL, Vector2 pos2_GL, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{ return; }

			if (Vector2.Equals(pos1_GL, pos2_GL))
			{ return; }


			if (isNeedResetMat)
			{
				_matBatch.SetPass_Color();
				_matBatch.SetClippingSize(_glScreenClippingSize);

				GL.Begin(GL.LINES);
			}

			GL.Color(color);
			GL.Vertex(new Vector3(pos1_GL.x, pos1_GL.y, 0.0f));
			GL.Vertex(new Vector3(pos2_GL.x, pos2_GL.y, 0.0f));

			if (isNeedResetMat)
			{
				GL.End();
			}
		}



		//-------------------------------------------------------------------------------
		// Draw Box
		//-------------------------------------------------------------------------------
		public static void DrawBox(Vector2 pos, float width, float height, Color color, bool isWireframe)
		{
			DrawBox(pos, width, height, color, isWireframe, true);
		}
		public static void DrawBox(Vector2 pos, float width, float height, Color color, bool isWireframe, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{ return; }

			pos = World2GL(pos);

			float halfWidth = width * 0.5f * _zoom;
			float halfHeight = height * 0.5f * _zoom;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0 = new Vector2(pos.x - halfWidth, pos.y - halfHeight);
			Vector2 pos_1 = new Vector2(pos.x + halfWidth, pos.y - halfHeight);
			Vector2 pos_2 = new Vector2(pos.x + halfWidth, pos.y + halfHeight);
			Vector2 pos_3 = new Vector2(pos.x - halfWidth, pos.y + halfHeight);

			if (isWireframe)
			{
				if (isNeedResetMat)
				{
					_matBatch.SetPass_Color();
					_matBatch.SetClippingSize(_glScreenClippingSize);

					GL.Begin(GL.LINES);
				}

				GL.Color(color);
				GL.Vertex(pos_0);
				GL.Vertex(pos_1);

				GL.Vertex(pos_1);
				GL.Vertex(pos_2);

				GL.Vertex(pos_2);
				GL.Vertex(pos_3);

				GL.Vertex(pos_3);
				GL.Vertex(pos_0);
				if (isNeedResetMat)
				{
					GL.End();
				}
			}
			else
			{
				//CW
				// -------->
				// | 0   1
				// | 		
				// | 3   2
				if (isNeedResetMat)
				{
					_matBatch.SetPass_Color();
					_matBatch.SetClippingSize(_glScreenClippingSize);


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

			//GL.Flush();
		}


		public static void DrawBoxGL(Vector2 pos, float width, float height, Color color, bool isWireframe, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{ return; }

			float halfWidth = width * 0.5f * _zoom;
			float halfHeight = height * 0.5f * _zoom;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0 = new Vector2(pos.x - halfWidth, pos.y - halfHeight);
			Vector2 pos_1 = new Vector2(pos.x + halfWidth, pos.y - halfHeight);
			Vector2 pos_2 = new Vector2(pos.x + halfWidth, pos.y + halfHeight);
			Vector2 pos_3 = new Vector2(pos.x - halfWidth, pos.y + halfHeight);

			if (isWireframe)
			{
				if (isNeedResetMat)
				{
					_matBatch.SetPass_Color();
					_matBatch.SetClippingSize(_glScreenClippingSize);

					GL.Begin(GL.LINES);
				}

				GL.Color(color);
				GL.Vertex(pos_0);
				GL.Vertex(pos_1);
				GL.Vertex(pos_1);
				GL.Vertex(pos_2);
				GL.Vertex(pos_2);
				GL.Vertex(pos_3);
				GL.Vertex(pos_3);
				GL.Vertex(pos_0);
				if (isNeedResetMat)
				{
					GL.End();
				}
			}
			else
			{
				//CW
				// -------->
				// | 0   1
				// | 		
				// | 3   2
				if (isNeedResetMat)
				{
					_matBatch.SetPass_Color();
					_matBatch.SetClippingSize(_glScreenClippingSize);


					GL.Begin(GL.TRIANGLES);
				}
				GL.Color(color);
				// 0 - 1 - 2
				GL.Vertex(pos_0);
				GL.Vertex(pos_1);
				GL.Vertex(pos_2);

				// 2 - 3 - 0
				GL.Vertex(pos_2);
				GL.Vertex(pos_3);
				GL.Vertex(pos_0);

				if (isNeedResetMat)
				{
					GL.End();
				}
			}

			//GL.Flush();
		}



		public static void DrawCircle(Vector2 pos, float radius, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			pos = World2GL(pos);

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)

			if (isNeedResetMat)
			{
				_matBatch.SetPass_Color();
				_matBatch.SetClippingSize(_glScreenClippingSize);

				GL.Begin(GL.LINES);
			}

			float radiusGL = radius * _zoom;
			GL.Color(color);
			for (int i = 0; i < 36; i++)
			{
				float angleRad_0 = (i / 36.0f) * Mathf.PI * 2.0f;
				float angleRad_1 = ((i + 1) / 36.0f) * Mathf.PI * 2.0f;

				Vector2 pos0 = pos + new Vector2(Mathf.Cos(angleRad_0) * radiusGL, Mathf.Sin(angleRad_0) * radiusGL);
				Vector2 pos1 = pos + new Vector2(Mathf.Cos(angleRad_1) * radiusGL, Mathf.Sin(angleRad_1) * radiusGL);

				GL.Vertex(pos0);
				GL.Vertex(pos1);
			}
			if (isNeedResetMat)
			{
				GL.End();
			}


			//GL.Flush();
		}



		public static void DrawBoldLine(Vector2 pos1, Vector2 pos2, float width, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			pos1 = World2GL(pos1);
			pos2 = World2GL(pos2);

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
				_matBatch.SetPass_Color();
				_matBatch.SetClippingSize(_glScreenClippingSize);

				GL.Begin(GL.TRIANGLES);
			}
			GL.Color(color);
			// 0 - 1 - 2
			GL.Vertex(pos_0);
			GL.Vertex(pos_1);
			GL.Vertex(pos_2);
			GL.Vertex(pos_2);
			GL.Vertex(pos_1);
			GL.Vertex(pos_0);

			// 0 - 2 - 3
			GL.Vertex(pos_0);
			GL.Vertex(pos_2);
			GL.Vertex(pos_3);
			GL.Vertex(pos_3);
			GL.Vertex(pos_2);
			GL.Vertex(pos_0);

			// 3 - 5 - 0
			GL.Vertex(pos_3);
			GL.Vertex(pos_5);
			GL.Vertex(pos_0);
			GL.Vertex(pos_0);
			GL.Vertex(pos_5);
			GL.Vertex(pos_3);

			// 3 - 4 - 5
			GL.Vertex(pos_3);
			GL.Vertex(pos_4);
			GL.Vertex(pos_5);
			GL.Vertex(pos_5);
			GL.Vertex(pos_4);
			GL.Vertex(pos_3);

			if (isNeedResetMat)
			{
				GL.End();
			}
		}


		public static void DrawBoldLineGL(Vector2 pos1, Vector2 pos2, float width, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			if (pos1 == pos2)
			{ return; }

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
				_matBatch.SetPass_Color();
				_matBatch.SetClippingSize(_glScreenClippingSize);

				GL.Begin(GL.TRIANGLES);
			}
			GL.Color(color);
			// 0 - 1 - 2
			GL.Vertex(pos_0);
			GL.Vertex(pos_1);
			GL.Vertex(pos_2);
			GL.Vertex(pos_2);
			GL.Vertex(pos_1);
			GL.Vertex(pos_0);

			// 0 - 2 - 3
			GL.Vertex(pos_0);
			GL.Vertex(pos_2);
			GL.Vertex(pos_3);
			GL.Vertex(pos_3);
			GL.Vertex(pos_2);
			GL.Vertex(pos_0);

			// 3 - 5 - 0
			GL.Vertex(pos_3);
			GL.Vertex(pos_5);
			GL.Vertex(pos_0);
			GL.Vertex(pos_0);
			GL.Vertex(pos_5);
			GL.Vertex(pos_3);

			// 3 - 4 - 5
			GL.Vertex(pos_3);
			GL.Vertex(pos_4);
			GL.Vertex(pos_5);
			GL.Vertex(pos_5);
			GL.Vertex(pos_4);
			GL.Vertex(pos_3);

			if (isNeedResetMat)
			{
				GL.End();
			}
		}
		//-------------------------------------------------------------------------------
		// Draw Text
		//-------------------------------------------------------------------------------
		public static void DrawText(string text, Vector2 pos, float width, Color color)
		{
			//if(_mat_Color == null || _mat_Texture == null)
			//{
			//	return;
			//}
			if (_matBatch.IsNotReady())
			{
				return;
			}

			pos = World2GL(pos);

			if (IsVertexClipped(pos))
			{
				return;
			}

			if (IsVertexClipped(pos + new Vector2(width * _zoom, 15)))
			{
				return;
			}
			_textStyle.normal.textColor = color;


			GUI.Label(new Rect(pos.x, pos.y, 100.0f, 30.0f), text, _textStyle);
		}


		public static void DrawTextGL(string text, Vector2 pos, float width, Color color)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			if (IsVertexClipped(pos))
			{
				return;
			}

			if (IsVertexClipped(pos + new Vector2(width, 15)))
			{
				return;
			}
			_textStyle.normal.textColor = color;


			GUI.Label(new Rect(pos.x, pos.y, width + 50, 30.0f), text, _textStyle);
		}


		//-------------------------------------------------------------------------------
		// Draw Texture
		//-------------------------------------------------------------------------------
		public static void DrawTexture(Texture2D image, Vector2 pos, float width, float height, Color color2X)
		{
			DrawTexture(image, pos, width, height, color2X, 0.0f);
		}

		public static void DrawTexture(Texture2D image, Vector2 pos, float width, float height, Color color2X, float depth)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			pos = World2GL(pos);


			float realWidth = width * _zoom;
			float realHeight = height * _zoom;

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
			_matBatch.SetPass_Texture_Normal(color2X, image, apPortrait.SHADER_TYPE.AlphaBlend);
			_matBatch.SetClippingSize(_glScreenClippingSize);


			GL.Begin(GL.TRIANGLES);

			GL.TexCoord(uv_0);
			GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
			GL.TexCoord(uv_1);
			GL.Vertex(new Vector3(pos_1.x, pos_1.y, depth)); // 1
			GL.TexCoord(uv_2);
			GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2

			GL.TexCoord(uv_2);
			GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2
			GL.TexCoord(uv_3);
			GL.Vertex(new Vector3(pos_3.x, pos_3.y, depth)); // 3
			GL.TexCoord(uv_0);
			GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
			GL.End();

			//GL.Flush();
		}






		public static void DrawTexture(Texture2D image, apMatrix3x3 matrix, float width, float height, Color color2X, float depth)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			float width_Half = width * 0.5f;
			float height_Half = height * 0.5f;

			//Zero 대신 mesh Pivot 위치로 삼자
			Vector2 pos_0 = World2GL(matrix.MultiplyPoint(new Vector2(-width_Half, +height_Half)));
			Vector2 pos_1 = World2GL(matrix.MultiplyPoint(new Vector2(+width_Half, +height_Half)));
			Vector2 pos_2 = World2GL(matrix.MultiplyPoint(new Vector2(+width_Half, -height_Half)));
			Vector2 pos_3 = World2GL(matrix.MultiplyPoint(new Vector2(-width_Half, -height_Half)));

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
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
			_matBatch.SetPass_Texture_Normal(color2X, image, apPortrait.SHADER_TYPE.AlphaBlend);
			_matBatch.SetClippingSize(_glScreenClippingSize);


			GL.Begin(GL.TRIANGLES);

			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
			GL.TexCoord(uv_1);	GL.Vertex(new Vector3(pos_1.x, pos_1.y, depth)); // 1
			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2

			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2
			GL.TexCoord(uv_3);	GL.Vertex(new Vector3(pos_3.x, pos_3.y, depth)); // 3
			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0


			GL.End();
		}

		public static void DrawTextureGL(Texture2D image, Vector2 pos, float width, float height, Color color2X, float depth)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			float realWidth = width * _zoom;
			float realHeight = height * _zoom;

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
			_matBatch.SetPass_Texture_Normal(color2X, image, apPortrait.SHADER_TYPE.AlphaBlend);
			_matBatch.SetClippingSize(_glScreenClippingSize);


			GL.Begin(GL.TRIANGLES);

			GL.TexCoord(uv_0);
			GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
			GL.TexCoord(uv_1);
			GL.Vertex(new Vector3(pos_1.x, pos_1.y, depth)); // 1
			GL.TexCoord(uv_2);
			GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2

			GL.TexCoord(uv_2);
			GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2
			GL.TexCoord(uv_3);
			GL.Vertex(new Vector3(pos_3.x, pos_3.y, depth)); // 3
			GL.TexCoord(uv_0);
			GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
			GL.End();

			//GL.Flush();
		}



		public static void DrawTextureGL(Texture2D image, Vector2 pos, float width, float height, Color color2X, float depth, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			float realWidth = width * _zoom;
			float realHeight = height * _zoom;

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
				_matBatch.SetPass_Texture_Normal(color2X, image, apPortrait.SHADER_TYPE.AlphaBlend);
				_matBatch.SetClippingSize(_glScreenClippingSize);


				GL.Begin(GL.TRIANGLES);
			}

			GL.TexCoord(uv_0);
			GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
			GL.TexCoord(uv_1);
			GL.Vertex(new Vector3(pos_1.x, pos_1.y, depth)); // 1
			GL.TexCoord(uv_2);
			GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2

			GL.TexCoord(uv_2);
			GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2
			GL.TexCoord(uv_3);
			GL.Vertex(new Vector3(pos_3.x, pos_3.y, depth)); // 3
			GL.TexCoord(uv_0);
			GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0

			if (isNeedResetMat)
			{
				GL.End();
			}


			//GL.Flush();
		}
		//-------------------------------------------------------------------------------
		// Draw Mesh
		//-------------------------------------------------------------------------------
		public static void DrawMesh(apMesh mesh, apMatrix3x3 matrix, Color color2X, RENDER_TYPE renderType, apVertexController vertexController, apEditor editor, Vector2 mousePosition)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				//if (mesh == null || mesh._textureData == null || mesh._textureData._image == null)//이전 코드
				if (mesh == null || mesh.LinkedTextureData == null || mesh.LinkedTextureData._image == null)//변경 코드
				{
					DrawBox(Vector2.zero, 512, 512, Color.red, true);
					DrawText("No Image", Vector2.zero, 80, Color.cyan);
					return;
				}

				//1. 모든 메시를 보여줄때 (또는 클리핑된 메시가 없을 때) => 
				bool isShowAllTexture = false;
				Color textureColor = _textureColor_Gray;
				if ((renderType & RENDER_TYPE.ShadeAllMesh) != 0 || mesh._indexBuffer.Count < 3)
				{
					isShowAllTexture = true;
					textureColor = _textureColor_Shade;
				}
				else if ((renderType & RENDER_TYPE.AllMesh) != 0)
				{
					isShowAllTexture = true;
				}

				matrix *= mesh.Matrix_VertToLocal;

				if (isShowAllTexture)
				{
					//DrawTexture(mesh._textureData._image, matrix, mesh._textureData._width, mesh._textureData._height, textureColor, -10);
					DrawTexture(mesh.LinkedTextureData._image, matrix, mesh.LinkedTextureData._width, mesh.LinkedTextureData._height, textureColor, -10);
				}

				apVertex selectedVertex = null;
				List<apVertex> selectedVertices = null;
				apVertex nextSelectedVertex = null;
				apBone selectedBone = null;
				apMeshPolygon selectedPolygon = null;
				if (vertexController != null)
				{
					selectedVertex = vertexController.Vertex;
					selectedVertices = vertexController.Vertices;
					nextSelectedVertex = vertexController.LinkedNextVertex;
					selectedBone = vertexController.Bone;
					selectedPolygon = vertexController.Polygon;
				}


				Vector2 pos2_0 = Vector2.zero;
				Vector2 pos2_1 = Vector2.zero;
				Vector2 pos2_2 = Vector2.zero;

				Vector3 pos_0 = Vector3.zero;
				Vector3 pos_1 = Vector3.zero;
				Vector3 pos_2 = Vector3.zero;

				Vector2 uv_0 = Vector2.zero;
				Vector2 uv_1 = Vector2.zero;
				Vector2 uv_2 = Vector2.zero;

				//2. 메시를 렌더링하자
				if (mesh._indexBuffer.Count >= 3)
				{
					//Debug.Log("Draw Mesh.. [" + (mesh.LinkedTextureData._image != null) + "]");
					//Debug.Log("Size : " + mesh.LinkedTextureData._uniqueID + " / " + mesh.LinkedTextureData._name + " / " + mesh.LinkedTextureData._width + "x" + mesh.LinkedTextureData._height);
					//Debug.Log("Texture : " + mesh.LinkedTextureData._image.name + " / " + mesh.LinkedTextureData._image.width + "x" + mesh.LinkedTextureData._image.height);
					//------------------------------------------
					// Drawcall Batch를 했을때
					// <참고> Weight를 출력하고 싶다면 Normal 대신 VColor를 넣고, VertexColor를 넣어주자
					if ((renderType & RENDER_TYPE.VolumeWeightColor) != 0)
					{
						//_matBatch.SetPass_Texture_VColor(_textureColor_Gray, mesh._textureData._image, 1.0f, apPortrait.SHADER_TYPE.AlphaBlend);
						_matBatch.SetPass_Texture_VColor(_textureColor_Gray, mesh.LinkedTextureData._image, 1.0f, apPortrait.SHADER_TYPE.AlphaBlend);
					}
					else
					{
						//_matBatch.SetPass_Texture_Normal(color2X, mesh._textureData._image, apPortrait.SHADER_TYPE.AlphaBlend);
						_matBatch.SetPass_Texture_Normal(color2X, mesh.LinkedTextureData._image, apPortrait.SHADER_TYPE.AlphaBlend);

					}
					
					_matBatch.SetClippingSize(_glScreenClippingSize);

					GL.Begin(GL.TRIANGLES);
					//------------------------------------------
					apVertex vert0, vert1, vert2;
					//Color color0 = Color.black, color1 = Color.black, color2 = Color.black;
					Color color0 = Color.white, color1 = Color.white, color2 = Color.white;
					int iVertColor = 0;
					if ((renderType & RENDER_TYPE.VolumeWeightColor) != 0)
					{
						iVertColor = 1;
					}
					else
					{
						iVertColor = 0;
						color0 = Color.white;
						color1 = Color.white;
						color2 = Color.white;
					}
					for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
					{
						if (i + 2 >= mesh._indexBuffer.Count)
						{ break; }

						if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
						{
							break;
						}

						vert0 = mesh._vertexData[mesh._indexBuffer[i + 0]];
						vert1 = mesh._vertexData[mesh._indexBuffer[i + 1]];
						vert2 = mesh._vertexData[mesh._indexBuffer[i + 2]];

						
						pos2_0 = World2GL(matrix.MultiplyPoint(vert0._pos));
						pos2_1 = World2GL(matrix.MultiplyPoint(vert1._pos));
						pos2_2 = World2GL(matrix.MultiplyPoint(vert2._pos));

						pos_0 = new Vector3(pos2_0.x, pos2_0.y, vert0._zDepth * 0.1f);
						pos_1 = new Vector3(pos2_1.x, pos2_1.y, vert1._zDepth * 0.5f);
						pos_2 = new Vector3(pos2_2.x, pos2_2.y, vert2._zDepth * 0.5f);//<<Z값이 반영되었다.

						uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
						uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
						uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;

						switch (iVertColor)
						{
							case 1: //VolumeWeightColor
								color0 = GetWeightGrayscale(vert0._zDepth);
								color1 = GetWeightGrayscale(vert1._zDepth);
								color2 = GetWeightGrayscale(vert2._zDepth);
								break;
						}
						////------------------------------------------

						GL.Color(color0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						GL.Color(color1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(color2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

						//Back Side
						GL.Color(color2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
						GL.Color(color1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(color0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0

						

						////------------------------------------------
					}
					GL.End();

				}

				if (mesh._isPSDParsed)
				{
					Vector2 pos_LT = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_LT.x, mesh._atlasFromPSD_LT.y));
					Vector2 pos_RT = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_RB.x, mesh._atlasFromPSD_LT.y));
					Vector2 pos_LB = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_LT.x, mesh._atlasFromPSD_RB.y));
					Vector2 pos_RB = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_RB.x, mesh._atlasFromPSD_RB.y));


					_matBatch.SetPass_Color();
					_matBatch.SetClippingSize(_glScreenClippingSize);
					GL.Begin(GL.LINES);

					DrawLine(pos_LT, pos_RT, editor._colorOption_AtlasBorder, false);
					DrawLine(pos_RT, pos_RB, editor._colorOption_AtlasBorder, false);
					DrawLine(pos_RB, pos_LB, editor._colorOption_AtlasBorder, false);
					DrawLine(pos_LB, pos_LT, editor._colorOption_AtlasBorder, false);

					GL.End();
				}

				//외곽선을 그려주자
				//float imageWidthHalf = mesh._textureData._width * 0.5f;
				//float imageHeightHalf = mesh._textureData._height * 0.5f;

				float imageWidthHalf = mesh.LinkedTextureData._width * 0.5f;
				float imageHeightHalf = mesh.LinkedTextureData._height * 0.5f;

				Vector2 pos_TexOutline_LT = matrix.MultiplyPoint(new Vector2(-imageWidthHalf, -imageHeightHalf));
				Vector2 pos_TexOutline_RT = matrix.MultiplyPoint(new Vector2(imageWidthHalf, -imageHeightHalf));
				Vector2 pos_TexOutline_LB = matrix.MultiplyPoint(new Vector2(-imageWidthHalf, imageHeightHalf));
				Vector2 pos_TexOutline_RB = matrix.MultiplyPoint(new Vector2(imageWidthHalf, imageHeightHalf));


				_matBatch.SetPass_Color();
				_matBatch.SetClippingSize(_glScreenClippingSize);
				GL.Begin(GL.LINES);

				DrawLine(pos_TexOutline_LT, pos_TexOutline_RT, editor._colorOption_AtlasBorder, false);
				DrawLine(pos_TexOutline_RT, pos_TexOutline_RB, editor._colorOption_AtlasBorder, false);
				DrawLine(pos_TexOutline_RB, pos_TexOutline_LB, editor._colorOption_AtlasBorder, false);
				DrawLine(pos_TexOutline_LB, pos_TexOutline_LT, editor._colorOption_AtlasBorder, false);

				GL.End();


				//3. Edge를 렌더링하자 (전체 / Ouline)
				if ((renderType & RENDER_TYPE.AllEdges) != 0)
				{
					Vector2 pos0 = Vector2.zero, pos1 = Vector2.zero;
					if (mesh._edges.Count > 0)
					{
						_matBatch.SetPass_Color();
						_matBatch.SetClippingSize(_glScreenClippingSize);
						GL.Begin(GL.LINES);
						for (int i = 0; i < mesh._edges.Count; i++)
						{
							pos0 = matrix.MultiplyPoint(mesh._edges[i]._vert1._pos);
							pos1 = matrix.MultiplyPoint(mesh._edges[i]._vert2._pos);

							DrawLine(pos0, pos1, editor._colorOption_MeshEdge, false);
						}

						for (int iPoly = 0; iPoly < mesh._polygons.Count; iPoly++)
						{
							for (int iHE = 0; iHE < mesh._polygons[iPoly]._hidddenEdges.Count; iHE++)
							{
								apMeshEdge hiddenEdge = mesh._polygons[iPoly]._hidddenEdges[iHE];

								pos0 = matrix.MultiplyPoint(hiddenEdge._vert1._pos);
								pos1 = matrix.MultiplyPoint(hiddenEdge._vert2._pos);

								DrawLine(pos0, pos1, editor._colorOption_MeshHiddenEdge, false);
							}

						}

						GL.End();
					}
				}
				else if ((renderType & RENDER_TYPE.Outlines) != 0)
				{
					Vector2 pos0 = Vector2.zero, pos1 = Vector2.zero;
					if (mesh._edges.Count > 0)
					{
						_matBatch.SetPass_Color();
						_matBatch.SetClippingSize(_glScreenClippingSize);
						//GL.Begin(GL.LINES);
						GL.Begin(GL.TRIANGLES);
						for (int i = 0; i < mesh._edges.Count; i++)
						{
							if (!mesh._edges[i]._isOutline)
							{ continue; }
							pos0 = matrix.MultiplyPoint(mesh._edges[i]._vert1._pos);
							pos1 = matrix.MultiplyPoint(mesh._edges[i]._vert2._pos);

							//DrawLine(pos0, pos1, _lineColor_Outline, false);
							DrawBoldLine(pos0, pos1, 6.0f, editor._colorOption_Outline, false);
						}
						GL.End();
					}


				}

				if ((renderType & RENDER_TYPE.PolygonOutline) != 0)
				{
					if (selectedPolygon != null)
					{
						if (selectedPolygon._edges.Count > 0)
						{
							Vector2 pos0 = Vector2.zero, pos1 = Vector2.zero;

							_matBatch.SetPass_Color();
							_matBatch.SetClippingSize(_glScreenClippingSize);
							//GL.Begin(GL.LINES);
							GL.Begin(GL.TRIANGLES);
							for (int i = 0; i < selectedPolygon._edges.Count; i++)
							{
								pos0 = matrix.MultiplyPoint(selectedPolygon._edges[i]._vert1._pos);
								pos1 = matrix.MultiplyPoint(selectedPolygon._edges[i]._vert2._pos);

								//DrawLine(pos0, pos1, _lineColor_Outline, false);
								DrawBoldLine(pos0, pos1, 6.0f, editor._colorOption_Outline, false);
							}
							GL.End();
						}
					}
				}

				//3. 버텍스를 렌더링하자
				if ((renderType & RENDER_TYPE.Vertex) != 0)
				{
					bool isWireFramePoint = false;
					if (isWireFramePoint)
					{
						_matBatch.SetPass_Color();
						_matBatch.SetClippingSize(_glScreenClippingSize);

						GL.Begin(GL.LINES);
					}
					else
					{
						_matBatch.SetPass_Color();
						_matBatch.SetClippingSize(_glScreenClippingSize);

						GL.Begin(GL.TRIANGLES);
					}


					float pointSize = 10.0f / _zoom;
					Vector2 pos = Vector2.zero;
					for (int i = 0; i < mesh._vertexData.Count; i++)
					{
						Color vColor = editor._colorOption_VertColor_NotSelected;
						if (mesh._vertexData[i] == selectedVertex)
						{
							vColor = editor._colorOption_VertColor_Selected;
						}
						else if (mesh._vertexData[i] == nextSelectedVertex)
						{
							vColor = _vertColor_NextSelected;
						}
						else if(selectedVertices != null)
						{
							if(selectedVertices.Contains(mesh._vertexData[i]))
							{
								vColor = editor._colorOption_VertColor_Selected;
							}
						}
						pos = matrix.MultiplyPoint(mesh._vertexData[i]._pos);
						DrawBox(pos, pointSize, pointSize, vColor, isWireFramePoint, false);

						AddCursorRect(mousePosition, World2GL(pos), 10, 10, MouseCursor.MoveArrow);
					}

					GL.End();
				}



			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}


		//------------------------------------------------------------------------------------------------
		// Draw Mesh의 Edge Wire
		//------------------------------------------------------------------------------------------------
		public static void DrawMeshWorkEdgeWire(apMesh mesh, apMatrix3x3 matrix, apVertexController vertexController, bool isCross, bool isCrossMultiple)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				//if (mesh == null || mesh._textureData == null || mesh._textureData._image == null)
				if (mesh == null || mesh.LinkedTextureData == null || mesh.LinkedTextureData._image == null)
				{
					return;
				}

				if (vertexController.Vertex == null)
				{
					return;
				}

				matrix *= mesh.Matrix_VertToLocal;

				Vector2 mouseW = GL2World(vertexController.TmpEdgeWirePos);
				Vector2 vertPosW = matrix.MultiplyPoint(vertexController.Vertex._pos);

				Color lineColor = Color.green;
				if (isCross)
				{
					lineColor = Color.red;
				}
				else if (isCrossMultiple)
				{
					lineColor = new Color(0.2f, 0.8f, 1.0f, 1.0f);
				}

				DrawLine(vertPosW, mouseW, lineColor, true);

				if (vertexController.LinkedNextVertex != null && vertexController.LinkedNextVertex != vertexController.Vertex)
				{
					Vector2 linkedVertPosW = matrix.MultiplyPoint(vertexController.LinkedNextVertex._pos);
					float size = 20.0f / _zoom;
					DrawBox(linkedVertPosW, size, size, lineColor, true);
				}
				if (isCross)
				{
					Vector2 crossPointW = matrix.MultiplyPoint(vertexController.EdgeWireCrossPoint());
					float size = 20.0f / _zoom;
					DrawBox(crossPointW, size, size, Color.cyan, true);
				}
				else if (isCrossMultiple)
				{
					List<Vector2> crossVerts = vertexController.EdgeWireMultipleCrossPoints();
					float size = 20.0f / _zoom;

					for (int i = 0; i < crossVerts.Count; i++)
					{
						Vector2 crossPointW = matrix.MultiplyPoint(crossVerts[i]);
						DrawBox(crossPointW, size, size, Color.yellow, true);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}


		//------------------------------------------------------------------------------------------------
		// Draw Render Unit (Mesh / Outline)
		//------------------------------------------------------------------------------------------------
		private static List<apRenderVertex> _tmpSelectedVertices = new List<apRenderVertex>();
		private static List<apRenderVertex> _tmpSelectedVertices_Weighted = new List<apRenderVertex>();
		//private static List<float> _tmpSelectedVertices_WeightedValue = new List<float>();

		public static void DrawRenderUnit(apRenderUnit renderUnit,
											RENDER_TYPE renderType,
											apVertexController vertexController,
											apSelection select,
											apEditor editor,
											Vector2 mousePos)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
				{
					return;
				}

				if (renderUnit._renderVerts.Count == 0)
				{
					return;
				}
				Color textureColor = renderUnit._meshColor2X;

				
				apMesh mesh = renderUnit._meshTransform._mesh;
				bool isVisible = renderUnit._isVisible;

				//if (mesh._textureData == null)
				if (mesh.LinkedTextureData == null)
				{
					return;
				}

				//미리 GL 좌표를 연산하고, 나중에 중복 연산(World -> GL)을 하지 않도록 하자
				apRenderVertex rVert = null;
				for (int i = 0; i < renderUnit._renderVerts.Count; i++)
				{
					rVert = renderUnit._renderVerts[i];
					rVert._pos_GL = World2GL(rVert._pos_World);
				}


				bool isAnyVertexSelected = false;
				//apBone selectedBone = null;
				bool isWeightedSelected = false;

				bool isBoneWeightColor = (renderType & RENDER_TYPE.BoneRigWeightColor) != 0;
				bool isPhyVolumeWeightColor = (renderType & RENDER_TYPE.PhysicsWeightColor) != 0 || (renderType & RENDER_TYPE.VolumeWeightColor) != 0;
				bool isBoneColor = false;
				float vertexColorRatio = 0.0f;

				if (select != null)
				{
					_tmpSelectedVertices.Clear();
					_tmpSelectedVertices_Weighted.Clear();
					//_tmpSelectedVertices_WeightedValue.Clear();

					//Soft Selection + TODO 나중에 Volume 등에서 Weighted 설정을 하자
					if (select.Editor.Gizmos.IsSoftSelectionMode)
					{
						isWeightedSelected = true;
					}

					isBoneColor = select._rigEdit_isBoneColorView;
					if (isBoneWeightColor)
					{
						if (select._rigEdit_viewMode == apSelection.RIGGING_EDIT_VIEW_MODE.WeightColorOnly)
						{
							vertexColorRatio = 1.0f;
						}
						else
						{
							vertexColorRatio = 0.5f;
						}
					}
					else if (isPhyVolumeWeightColor)
					{
						vertexColorRatio = 0.7f;
					}

					isAnyVertexSelected = true;
					if (select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
					{
						if (select.ModRenderVertListOfMod != null && select.ModRenderVertListOfMod.Count > 0)
						{
							for (int i = 0; i < select.ModRenderVertListOfMod.Count; i++)
							{
								_tmpSelectedVertices.Add(select.ModRenderVertListOfMod[i]._renderVert);
							}

							if (isWeightedSelected)
							{
								for (int i = 0; i < select.ModRenderVertListOfMod_Weighted.Count; i++)
								{
									_tmpSelectedVertices_Weighted.Add(select.ModRenderVertListOfMod_Weighted[i]._renderVert);
									//_tmpSelectedVertices_WeightedValue.Add(select.ModRenderVertListOfMod_Weighted[i]._vertWeightByTool);
									select.ModRenderVertListOfMod_Weighted[i]._renderVert._renderWeightByTool = select.ModRenderVertListOfMod_Weighted[i]._vertWeightByTool;
								}
							}
						}


					}
					else if (select.SelectionType == apSelection.SELECTION_TYPE.Animation)
					{
						if (select.ModRenderVertListOfAnim != null && select.ModRenderVertListOfAnim.Count > 0)
						{
							for (int i = 0; i < select.ModRenderVertListOfAnim.Count; i++)
							{
								_tmpSelectedVertices.Add(select.ModRenderVertListOfAnim[i]._renderVert);
							}

							if (isWeightedSelected)
							{
								for (int i = 0; i < select.ModRenderVertListOfAnim_Weighted.Count; i++)
								{
									_tmpSelectedVertices_Weighted.Add(select.ModRenderVertListOfAnim_Weighted[i]._renderVert);
									//_tmpSelectedVertices_WeightedValue.Add(select.ModRenderVertListOfAnim_Weighted[i]._vertWeightByTool);
									select.ModRenderVertListOfAnim_Weighted[i]._renderVert._renderWeightByTool = select.ModRenderVertListOfAnim_Weighted[i]._vertWeightByTool;
								}
							}
						}
					}
				}


				//렌더링 방식은 Mesh (with Color) 또는 Vertex / Outline이 있다.
				bool isMeshRender = false;
				bool isVertexRender = ((renderType & RENDER_TYPE.Vertex) != 0);
				bool isOutlineRender = ((renderType & RENDER_TYPE.Outlines) != 0);
				bool isAllEdgeRender = ((renderType & RENDER_TYPE.AllEdges) != 0);
				bool isToneColor = ((renderType & RENDER_TYPE.ToneColor) != 0);
				if (!isVertexRender && !isOutlineRender)
				{
					isMeshRender = true;
				}


				bool isDrawTFBorderLine = ((int)(renderType & RENDER_TYPE.TransformBorderLine) != 0);

				//2. 메시를 렌더링하자
				if (mesh._indexBuffer.Count >= 3 && isMeshRender && isVisible)
				{
					//------------------------------------------
					// Drawcall Batch를 했을때
					// Debug.Log("Texture Color : " + textureColor);
					Color color0 = Color.black, color1 = Color.black, color2 = Color.black;

					int iVertColor = 0;

					if ((renderType & RENDER_TYPE.VolumeWeightColor) != 0)
					{
						iVertColor = 1;
					}
					else if ((renderType & RENDER_TYPE.PhysicsWeightColor) != 0)
					{
						iVertColor = 2;
					}
					else if ((renderType & RENDER_TYPE.BoneRigWeightColor) != 0)
					{
						iVertColor = 3;
					}
					else
					{
						iVertColor = 0;
						color0 = Color.black;
						color1 = Color.black;
						color2 = Color.black;
					}

					if(isToneColor)
					{
						//_matBatch.SetPass_ToneColor_Normal(_toneColor, mesh._textureData._image);
						_matBatch.SetPass_ToneColor_Normal(_toneColor, mesh.LinkedTextureData._image);
					}
					else if (!isBoneWeightColor && !isPhyVolumeWeightColor)
					{
						//_matBatch.SetPass_Texture_VColor(textureColor, mesh._textureData._image, 0.0f, renderUnit.ShaderType);
						_matBatch.SetPass_Texture_VColor(textureColor, mesh.LinkedTextureData._image, 0.0f, renderUnit.ShaderType);
					}
					else
					{
						//_matBatch.SetPass_Texture_VColor(textureColor, mesh._textureData._image, vertexColorRatio, renderUnit.ShaderType);
						_matBatch.SetPass_Texture_VColor(textureColor, mesh.LinkedTextureData._image, vertexColorRatio, renderUnit.ShaderType);
					}
					_matBatch.SetClippingSize(_glScreenClippingSize);


					GL.Begin(GL.TRIANGLES);
					//------------------------------------------
					//apVertex vert0, vert1, vert2;
					apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;


					Vector3 pos_0 = Vector3.zero;
					Vector3 pos_1 = Vector3.zero;
					Vector3 pos_2 = Vector3.zero;


					Vector2 uv_0 = Vector2.zero;
					Vector2 uv_1 = Vector2.zero;
					Vector2 uv_2 = Vector2.zero;

					for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
					{
						if (i + 2 >= mesh._indexBuffer.Count)
						{ break; }

						if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
						{
							break;
						}

						rVert0 = renderUnit._renderVerts[mesh._indexBuffer[i + 0]];
						rVert1 = renderUnit._renderVerts[mesh._indexBuffer[i + 1]];
						rVert2 = renderUnit._renderVerts[mesh._indexBuffer[i + 2]];

						pos_0.x = rVert0._pos_GL.x;
						pos_0.y = rVert0._pos_GL.y;
						pos_0.z = rVert0._vertex._zDepth * 0.5f;

						pos_1.x = rVert1._pos_GL.x;
						pos_1.y = rVert1._pos_GL.y;
						pos_1.z = rVert1._vertex._zDepth * 0.5f;

						pos_2.x = rVert2._pos_GL.x;
						pos_2.y = rVert2._pos_GL.y;
						pos_2.z = rVert2._vertex._zDepth * 0.5f;


						uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
						uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
						uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;

						switch (iVertColor)
						{
							case 1: //VolumeWeightColor
								color0 = GetWeightGrayscale(rVert0._renderWeightByTool);
								color1 = GetWeightGrayscale(rVert1._renderWeightByTool);
								color2 = GetWeightGrayscale(rVert2._renderWeightByTool);
								break;

							case 2: //PhysicsWeightColor
								color0 = GetWeightColor4(rVert0._renderWeightByTool);
								color1 = GetWeightColor4(rVert1._renderWeightByTool);
								color2 = GetWeightColor4(rVert2._renderWeightByTool);
								break;

							case 3: //BoneRigWeightColor
									//TODO : 본 리스트를 받아서 해야하는디..
								if (isBoneColor)
								{
									color0 = rVert0._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
									color1 = rVert1._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
									color2 = rVert2._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
								}
								else
								{
									color0 = GetWeightColor3(rVert0._renderWeightByTool);
									color1 = GetWeightColor3(rVert1._renderWeightByTool);
									color2 = GetWeightColor3(rVert2._renderWeightByTool);
								}
								color0.a = 1.0f;
								color1.a = 1.0f;
								color2.a = 1.0f;
								//color0 = Color.black;
								//color1 = Color.black;
								//color2 = Color.black;
								//color0 = _weightColor_Gray;
								//color1 = _weightColor_Gray;
								//color2 = _weightColor_Gray;

								//TODO : 여기서부터

								//if (selectedBone != null)
								//{
								//	for (int iBone = 0; iBone < 4; iBone++)
								//	{
								//		//if(vert0._bones[iBone] != selectedBone) { color0 = GetWeightColor(vert0._boneWeights[iBone]); }
								//		//if(vert1._bones[iBone] != selectedBone) { color1 = GetWeightColor(vert1._boneWeights[iBone]); }
								//		//if(vert2._bones[iBone] != selectedBone) { color2 = GetWeightColor(vert2._boneWeights[iBone]); }
								//	}
								//}
								break;
						}
						////------------------------------------------

						GL.Color(color0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						GL.Color(color1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(color2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

						// Back Side
						GL.Color(color2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
						GL.Color(color1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(color0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0

						////------------------------------------------
					}
					GL.End();

				}

				//3. Edge를 렌더링하자
				if (isAllEdgeRender)
				{
					Vector2 pos0 = Vector2.zero, pos1 = Vector2.zero;

					apRenderVertex rVert0 = null, rVert1 = null;
					if (mesh._edges.Count > 0)
					{
						_matBatch.SetPass_Color();
						_matBatch.SetClippingSize(_glScreenClippingSize);
						GL.Begin(GL.LINES);
						for (int i = 0; i < mesh._edges.Count; i++)
						{
							rVert0 = renderUnit._renderVerts[mesh._edges[i]._vert1._index];
							rVert1 = renderUnit._renderVerts[mesh._edges[i]._vert2._index];

							//pos0 = matrixToWorld.MultiplyPoint3x4(mesh._edges[i]._vert1._pos);
							//pos1 = matrixToWorld.MultiplyPoint3x4(mesh._edges[i]._vert2._pos);

							//pos0 = rVert0._pos_World;
							//pos1 = rVert1._pos_World;

							//DrawLine(pos0, pos1, _lineColor_Tri, false);

							pos0 = rVert0._pos_GL;
							pos1 = rVert1._pos_GL;

							DrawLineGL(pos0, pos1, editor._colorOption_MeshEdge, false);
						}

						//렌더 유닛에서는 숨겨진 Edge는 표시하지 말자
						//for (int iPoly = 0; iPoly < mesh._polygons.Count; iPoly++)
						//{
						//	for (int iHE = 0; iHE < mesh._polygons[iPoly]._hidddenEdges.Count; iHE++)
						//	{
						//		apMeshEdge hiddenEdge = mesh._polygons[iPoly]._hidddenEdges[iHE];

						//		pos0 = matrixToWorld.MultiplyPoint3x4(hiddenEdge._vert1._pos);
						//		pos1 = matrixToWorld.MultiplyPoint3x4(hiddenEdge._vert2._pos);

						//		DrawLine(pos0, pos1, _lineColor_HiddenEdge, false);
						//	}

						//}

						GL.End();
					}
				}
				else if (isOutlineRender)
				{
					Vector2 pos0 = Vector2.zero, pos1 = Vector2.zero;
					apRenderVertex rVert0 = null, rVert1 = null;
					if (mesh._edges.Count > 0)
					{
						_matBatch.SetPass_Color();
						_matBatch.SetClippingSize(_glScreenClippingSize);

						GL.Begin(GL.TRIANGLES);

						for (int i = 0; i < mesh._edges.Count; i++)
						{
							if (!mesh._edges[i]._isOutline)
							{ continue; }

							rVert0 = renderUnit._renderVerts[mesh._edges[i]._vert1._index];
							rVert1 = renderUnit._renderVerts[mesh._edges[i]._vert2._index];

							//pos0 = rVert0._pos_World;
							//pos1 = rVert1._pos_World;

							//DrawBoldLine(pos0, pos1, 6.0f, _lineColor_Outline, false);

							pos0 = rVert0._pos_GL;
							pos1 = rVert1._pos_GL;

							DrawBoldLineGL(pos0, pos1, 6.0f, editor._colorOption_Outline, false);
						}

						GL.End();
					}
				}

				if (isDrawTFBorderLine)
				{
					float minPosLocal_X = float.MaxValue;
					float maxPosLocal_X = float.MinValue;
					float minPosLocal_Y = float.MaxValue;
					float maxPosLocal_Y = float.MinValue;

					Vector2 pos0 = Vector2.zero, pos1 = Vector2.zero;
					apRenderVertex rVert0 = null, rVert1 = null;

					if (mesh._edges.Count > 0)
					{
						for (int i = 0; i < mesh._edges.Count; i++)
						{
							if (!mesh._edges[i]._isOutline)
							{ continue; }

							rVert0 = renderUnit._renderVerts[mesh._edges[i]._vert1._index];
							rVert1 = renderUnit._renderVerts[mesh._edges[i]._vert2._index];

							pos0 = rVert0._pos_World;
							pos1 = rVert1._pos_World;

							if (rVert0._pos_LocalOnMesh.x < minPosLocal_X) { minPosLocal_X = rVert0._pos_LocalOnMesh.x; }
							if (rVert0._pos_LocalOnMesh.x > maxPosLocal_X) { maxPosLocal_X = rVert0._pos_LocalOnMesh.x; }
							if (rVert0._pos_LocalOnMesh.y < minPosLocal_Y) { minPosLocal_Y = rVert0._pos_LocalOnMesh.y; }
							if (rVert0._pos_LocalOnMesh.y > maxPosLocal_Y) { maxPosLocal_Y = rVert0._pos_LocalOnMesh.y; }

							if (rVert1._pos_LocalOnMesh.x < minPosLocal_X) { minPosLocal_X = rVert1._pos_LocalOnMesh.x; }
							if (rVert1._pos_LocalOnMesh.x > maxPosLocal_X) { maxPosLocal_X = rVert1._pos_LocalOnMesh.x; }
							if (rVert1._pos_LocalOnMesh.y < minPosLocal_Y) { minPosLocal_Y = rVert1._pos_LocalOnMesh.y; }
							if (rVert1._pos_LocalOnMesh.y > maxPosLocal_Y) { maxPosLocal_Y = rVert1._pos_LocalOnMesh.y; }
						}


						DrawTransformBorderFormOfRenderUnit(editor._colorOption_TransformBorder, minPosLocal_X, maxPosLocal_X, maxPosLocal_Y, minPosLocal_Y, renderUnit.WorldMatrix);
					}
				}


				//3. 버텍스를 렌더링하자
				if (isVertexRender)
				{
					bool isWireFramePoint = false;
					if (isWireFramePoint)
					{
						_matBatch.SetPass_Color();
						_matBatch.SetClippingSize(_glScreenClippingSize);

						GL.Begin(GL.LINES);
					}
					else
					{
						_matBatch.SetPass_Color();
						_matBatch.SetClippingSize(_glScreenClippingSize);

						GL.Begin(GL.TRIANGLES);
					}


					float pointSize = 10.0f / _zoom;
					float pointSizeOutline = 14.0f / _zoom;
					Vector2 pos = Vector2.zero;
					bool isVertSelected = false;

					if (isAnyVertexSelected)
					{
						Color vColor = Color.black;
						Color vColorOutline = _vertColor_Outline;
						for (int i = 0; i < renderUnit._renderVerts.Count; i++)
						{
							vColor = editor._colorOption_VertColor_NotSelected;
							vColorOutline = _vertColor_Outline;
							rVert = renderUnit._renderVerts[i];
							isVertSelected = false;

							if (isBoneWeightColor)
							{
								if (isBoneColor)
								{
									vColor = rVert._renderColorByTool;
								}
								else
								{
									vColor = GetWeightColor3_Vert(rVert._renderWeightByTool);
								}
							}
							else if (isPhyVolumeWeightColor)
							{
								vColor = GetWeightColor4_Vert(rVert._renderWeightByTool);
							}

							if (_tmpSelectedVertices != null)
							{
								if (_tmpSelectedVertices.Contains(rVert))
								{
									//선택된 경우
									isVertSelected = true;

									if (isBoneWeightColor || isPhyVolumeWeightColor)
									{
										//vColorOutline = vColor;
										vColorOutline = _vertColor_Outline_White;
									}
									else
									{
										vColor = editor._colorOption_VertColor_Selected;
									}
									
								}
								else if (isWeightedSelected && _tmpSelectedVertices_Weighted != null)
								{
									if (_tmpSelectedVertices_Weighted.Contains(rVert))
									{
										vColor = GetWeightColor2(rVert._renderWeightByTool, editor);
									}
								}

							}


							//pos = rVert._pos_World;
							//DrawBox(pos, pointSize, pointSize, vColor, isWireFramePoint, false);

							pos = rVert._pos_GL;
							if (isVertSelected || isBoneWeightColor || isPhyVolumeWeightColor)
							{
								DrawBoxGL(pos, pointSizeOutline, pointSizeOutline, vColorOutline, isWireFramePoint, false);
							}
							DrawBoxGL(pos, pointSize, pointSize, vColor, isWireFramePoint, false);

							AddCursorRect(mousePos, pos, 10, 10, MouseCursor.MoveArrow);
						}
					}
					GL.End();

					if (isPhyVolumeWeightColor)
					{
						float pointSize_PhysicImg = 40.0f / _zoom;
						//추가적인 Vertex 이미지를 추가한다.
						//RenderVertex의 Param으로 이미지를 추가한다.

						//1. Physic Main
						_matBatch.SetPass_Texture_Normal(_textureColor_Gray, _img_VertPhysicMain, apPortrait.SHADER_TYPE.AlphaBlend);
						_matBatch.SetClippingSize(_glScreenClippingSize);
						GL.Begin(GL.TRIANGLES);

						for (int i = 0; i < renderUnit._renderVerts.Count; i++)
						{
							rVert = renderUnit._renderVerts[i];
							if (rVert._renderParam == 1)
							{
								DrawTextureGL(_img_VertPhysicMain, rVert._pos_GL, pointSize_PhysicImg, pointSize_PhysicImg, _textureColor_Gray, 0.0f, false);
							}
						}

						GL.End();


						//2. Physic Constraint
						_matBatch.SetPass_Texture_Normal(_textureColor_Gray, _img_VertPhysicConstraint, apPortrait.SHADER_TYPE.AlphaBlend);
						_matBatch.SetClippingSize(_glScreenClippingSize);
						GL.Begin(GL.TRIANGLES);

						for (int i = 0; i < renderUnit._renderVerts.Count; i++)
						{
							rVert = renderUnit._renderVerts[i];
							if (rVert._renderParam == 2)
							{
								DrawTextureGL(_img_VertPhysicConstraint, rVert._pos_GL, pointSize_PhysicImg, pointSize_PhysicImg, _textureColor_Gray, 0.0f, false);
							}
						}

						GL.End();
					}
				}

				//DrawText("<-[" + renderUnit.Name + "_" + renderUnit._debugID + "]", renderUnit.WorldMatrixWrap._pos + new Vector2(10.0f, 0.0f), 100, Color.yellow);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}



		public static void DrawRenderUnit_Basic(apRenderUnit renderUnit)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
				{
					return;
				}

				if (renderUnit._renderVerts.Count == 0)
				{
					return;
				}
				Color textureColor = renderUnit._meshColor2X;
				apMesh mesh = renderUnit._meshTransform._mesh;
				bool isVisible = renderUnit._isVisible;

				//if (mesh._textureData == null)
				if (mesh.LinkedTextureData == null)
				{
					return;
				}

				//미리 GL 좌표를 연산하고, 나중에 중복 연산(World -> GL)을 하지 않도록 하자
				apRenderVertex rVert = null;
				for (int i = 0; i < renderUnit._renderVerts.Count; i++)
				{
					rVert = renderUnit._renderVerts[i];
					rVert._pos_GL = World2GL(rVert._pos_World);
				}



				//2. 메시를 렌더링하자
				if (mesh._indexBuffer.Count >= 3 && isVisible)
				{
					//------------------------------------------
					// Drawcall Batch를 했을때
					// Debug.Log("Texture Color : " + textureColor);
					Color color0 = Color.black, color1 = Color.black, color2 = Color.black;

					//int iVertColor = 0;
					color0 = Color.black;
					color1 = Color.black;
					color2 = Color.black;


					//_matBatch.SetPass_Texture_VColor(textureColor, mesh._textureData._image, 0.0f, renderUnit.ShaderType);
					_matBatch.SetPass_Texture_VColor(textureColor, mesh.LinkedTextureData._image, 0.0f, renderUnit.ShaderType);
					//_matBatch.SetClippingSize(_glScreenClippingSize);
					_matBatch.SetClippingSize(new Vector4(0, 0, 1, 1));


					GL.Begin(GL.TRIANGLES);
					//------------------------------------------
					//apVertex vert0, vert1, vert2;
					apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;

					Vector3 pos_0 = Vector3.zero;
					Vector3 pos_1 = Vector3.zero;
					Vector3 pos_2 = Vector3.zero;


					Vector2 uv_0 = Vector2.zero;
					Vector2 uv_1 = Vector2.zero;
					Vector2 uv_2 = Vector2.zero;


					for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
					{
						if (i + 2 >= mesh._indexBuffer.Count)
						{ break; }

						if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
						{
							break;
						}

						rVert0 = renderUnit._renderVerts[mesh._indexBuffer[i + 0]];
						rVert1 = renderUnit._renderVerts[mesh._indexBuffer[i + 1]];
						rVert2 = renderUnit._renderVerts[mesh._indexBuffer[i + 2]];

						//Vector3 pos_0 = World2GL(rVert0._pos_World3);
						//Vector3 pos_1 = World2GL(rVert1._pos_World3);
						//Vector3 pos_2 = World2GL(rVert2._pos_World3);

						pos_0.x = rVert0._pos_GL.x;
						pos_0.y = rVert0._pos_GL.y;
						pos_0.z = rVert0._vertex._zDepth * 0.5f;

						pos_1.x = rVert1._pos_GL.x;
						pos_1.y = rVert1._pos_GL.y;
						pos_1.z = rVert1._vertex._zDepth * 0.5f;

						pos_2.x = rVert2._pos_GL.x;
						pos_2.y = rVert2._pos_GL.y;
						pos_2.z = rVert2._vertex._zDepth * 0.5f;


						uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
						uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
						uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;


						////------------------------------------------

						GL.Color(color0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						GL.Color(color1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(color2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

						// Back Side
						GL.Color(color2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
						GL.Color(color1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(color0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0

						////------------------------------------------
					}
					GL.End();

				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}


		public static void DrawRenderUnit_Basic_Alpha2White(apRenderUnit renderUnit)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
				{
					return;
				}

				if (renderUnit._renderVerts.Count == 0)
				{
					return;
				}
				Color textureColor = renderUnit._meshColor2X;
				apMesh mesh = renderUnit._meshTransform._mesh;
				bool isVisible = renderUnit._isVisible;

				//if (mesh._textureData == null)
				if (mesh.LinkedTextureData == null)
				{
					return;
				}

				//미리 GL 좌표를 연산하고, 나중에 중복 연산(World -> GL)을 하지 않도록 하자
				apRenderVertex rVert = null;
				for (int i = 0; i < renderUnit._renderVerts.Count; i++)
				{
					rVert = renderUnit._renderVerts[i];
					rVert._pos_GL = World2GL(rVert._pos_World);
				}



				//2. 메시를 렌더링하자
				if (mesh._indexBuffer.Count >= 3 && isVisible)
				{
					//------------------------------------------
					// Drawcall Batch를 했을때
					// Debug.Log("Texture Color : " + textureColor);
					Color color0 = Color.black, color1 = Color.black, color2 = Color.black;

					//int iVertColor = 0;
					color0 = Color.black;
					color1 = Color.black;
					color2 = Color.black;


					//_matBatch.SetPass_Texture_VColor(textureColor, mesh.LinkedTextureData._image, 0.0f, renderUnit.ShaderType);
					_matBatch.SetPass_Alpha2White(textureColor, mesh.LinkedTextureData._image);//<<Shader를 Alpha2White로 한다.
					_matBatch.SetClippingSize(new Vector4(0, 0, 1, 1));


					GL.Begin(GL.TRIANGLES);
					//------------------------------------------
					//apVertex vert0, vert1, vert2;
					apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;

					Vector3 pos_0 = Vector3.zero;
					Vector3 pos_1 = Vector3.zero;
					Vector3 pos_2 = Vector3.zero;


					Vector2 uv_0 = Vector2.zero;
					Vector2 uv_1 = Vector2.zero;
					Vector2 uv_2 = Vector2.zero;


					for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
					{
						if (i + 2 >= mesh._indexBuffer.Count)
						{ break; }

						if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
						{
							break;
						}

						rVert0 = renderUnit._renderVerts[mesh._indexBuffer[i + 0]];
						rVert1 = renderUnit._renderVerts[mesh._indexBuffer[i + 1]];
						rVert2 = renderUnit._renderVerts[mesh._indexBuffer[i + 2]];

						//Vector3 pos_0 = World2GL(rVert0._pos_World3);
						//Vector3 pos_1 = World2GL(rVert1._pos_World3);
						//Vector3 pos_2 = World2GL(rVert2._pos_World3);

						pos_0.x = rVert0._pos_GL.x;
						pos_0.y = rVert0._pos_GL.y;
						pos_0.z = rVert0._vertex._zDepth * 0.5f;

						pos_1.x = rVert1._pos_GL.x;
						pos_1.y = rVert1._pos_GL.y;
						pos_1.z = rVert1._vertex._zDepth * 0.5f;

						pos_2.x = rVert2._pos_GL.x;
						pos_2.y = rVert2._pos_GL.y;
						pos_2.z = rVert2._vertex._zDepth * 0.5f;


						uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
						uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
						uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;


						////------------------------------------------

						GL.Color(color0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						GL.Color(color1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(color2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

						// Back Side
						GL.Color(color2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
						GL.Color(color1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(color0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0

						////------------------------------------------
					}
					GL.End();

				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}


		//---------------------------------------------------------------------------------------
		// Draw Render Unit : Clipping
		// RenderType은 MeshColor에 영향을 주는 것들만 허용한다.
		//---------------------------------------------------------------------------------------
		#region [미사용 코드] : Parent가 Clip Mesh를 그려주는 방식은 사용하지 않는다.
		//public static void DrawRenderUnit_ClippingParent(	apRenderUnit renderUnit, 
		//													RENDER_TYPE renderType,
		//													apTransform_Mesh[] childMeshTransforms, 
		//													apRenderUnit[] childRenderUnits, 
		//													apVertexController vertexController, 
		//													apSelection select)
		//{
		//	try
		//	{
		//		//0. 메시, 텍스쳐가 없을 때
		//		if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
		//		{
		//			return;
		//		}

		//		if(renderUnit._renderVerts.Count == 0)
		//		{
		//			return;
		//		}
		//		Color textureColor = renderUnit._meshColor2X;
		//		apMesh mesh = renderUnit._meshTransform._mesh;

		//		Color[] clipColors = new Color[] { Color.clear, Color.clear, Color.clear };
		//		Texture2D[] clipTextures = new Texture2D[] { null, null, null };
		//		apMesh[] clipMeshes = new apMesh[] { null, null, null };

		//		if(mesh._textureData == null)
		//		{
		//			return;
		//		}

		//		for (int i = 0; i < 3; i++)
		//		{
		//			apTransform_Mesh childMeshTransform = childMeshTransforms[i];
		//			if(childMeshTransform != null)
		//			{
		//				if(childMeshTransform._mesh != null && childMeshTransform._mesh._textureData != null)
		//				{
		//					clipMeshes[i] = childMeshTransform._mesh;
		//					clipTextures[i] = clipMeshes[i]._textureData._image;
		//					clipColors[i] = childRenderUnits[i]._meshColor2X;
		//				}
		//			}
		//		}


		//		bool isBoneWeightColor = (int)(renderType & RENDER_TYPE.BoneRigWeightColor) != 0;


		//		bool isBoneColor = false;
		//		float vertexColorRatio = 0.0f;

		//		if(select != null)
		//		{
		//			isBoneColor = select._rigEdit_isBoneColorView;
		//			if (isBoneWeightColor)
		//			{
		//				if (select._rigEdit_viewMode == apSelection.RIGGING_EDIT_VIEW_MODE.WeightColorOnly)
		//				{
		//					vertexColorRatio = 1.0f;
		//				}
		//				else
		//				{
		//					vertexColorRatio = 0.5f;
		//				}
		//			}
		//		}

		//		apPortrait.SHADER_TYPE shaderType_Clip1 = apPortrait.SHADER_TYPE.AlphaBlend;
		//		apPortrait.SHADER_TYPE shaderType_Clip2 = apPortrait.SHADER_TYPE.AlphaBlend;
		//		apPortrait.SHADER_TYPE shaderType_Clip3 = apPortrait.SHADER_TYPE.AlphaBlend;


		//		if(childMeshTransforms[0] != null) { shaderType_Clip1 = childMeshTransforms[0]._shaderType; }
		//		if(childMeshTransforms[1] != null) { shaderType_Clip2 = childMeshTransforms[1]._shaderType; }
		//		if(childMeshTransforms[2] != null) { shaderType_Clip3 = childMeshTransforms[2]._shaderType; }

		//		//렌더링 방식은 Mesh (with Color) 또는 Vertex / Outline이 있다.

		//		//GL.RenderTargetBarrier();
		//		//2. 메시를 렌더링하자
		//		if(mesh._indexBuffer.Count >= 3)
		//		{
		//			_matBatch.SetMaterialType_MaskedTexture(renderUnit.ShaderType, shaderType_Clip1, shaderType_Clip2, shaderType_Clip3);
		//			_matBatch.SetClippingSize(_glScreenClippingSize);
		//			//------------------------------------------
		//			// Drawcall Batch를 했을때

		//			for (int iPass = 0; iPass < 2; iPass++)
		//			{
		//				// 이건 MultiPass라서 2번 돌려야 한다.
		//				//(여기서는 RenderTexture를 이용했다)

		//				_matBatch.SetPass_MaskedTexture(textureColor, mesh._textureData._image,
		//												clipColors[0], clipTextures[0],
		//												clipColors[1], clipTextures[1],
		//												clipColors[2], clipTextures[2],
		//												iPass, 
		//												vertexColorRatio);


		//				GL.Begin(GL.TRIANGLES);
		//				//------------------------------------------
		//				//1. Mask 먼저 그린다.
		//				apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;

		//				Color vertexChannelColor = Color.black;
		//				Color vColor0 = Color.black, vColor1 = Color.black, vColor2 = Color.black;

		//				//Vector2 posOffset = Vector2.zero;
		//				////posOffset.z = 0.5f;

		//				for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
		//				{
		//					if (i + 2 >= mesh._indexBuffer.Count)
		//					{ break; }

		//					if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
		//						mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
		//						mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
		//					{
		//						break;
		//					}

		//					rVert0 = renderUnit._renderVerts[mesh._indexBuffer[i + 0]];
		//					rVert1 = renderUnit._renderVerts[mesh._indexBuffer[i + 1]];
		//					rVert2 = renderUnit._renderVerts[mesh._indexBuffer[i + 2]];

		//					vColor0 = Color.black;
		//					vColor1 = Color.black;
		//					vColor2 = Color.black;

		//					if(isBoneWeightColor)
		//					{
		//						if(isBoneColor)
		//						{
		//							vColor0 = rVert0._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
		//							vColor1 = rVert1._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
		//							vColor2 = rVert2._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
		//						}
		//						else
		//						{
		//							vColor0 = GetWeightColor3(rVert0._renderWeightByTool);
		//							vColor1 = GetWeightColor3(rVert1._renderWeightByTool);
		//							vColor2 = GetWeightColor3(rVert2._renderWeightByTool);
		//						}
		//					}

		//					Vector3 pos_0 = World2GL(rVert0._pos_World);
		//					Vector3 pos_1 = World2GL(rVert1._pos_World);
		//					Vector3 pos_2 = World2GL(rVert2._pos_World);

		//					pos_0.z = 0.5f;
		//					pos_1.z = 0.5f;
		//					pos_2.z = 0.5f;
		//					//pos_0 += posOffset;
		//					//pos_1 += posOffset;
		//					//pos_2 += posOffset;

		//					Vector2 uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
		//					Vector2 uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
		//					Vector2 uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;


		//					GL.Color(vertexChannelColor); GL.TexCoord(uv_0); GL.MultiTexCoord3(1, vColor0.r, vColor0.g, vColor0.b); GL.Vertex(pos_0); // 0
		//					GL.Color(vertexChannelColor); GL.TexCoord(uv_1); GL.MultiTexCoord3(1, vColor1.r, vColor1.g, vColor1.b); GL.Vertex(pos_1); // 1
		//					GL.Color(vertexChannelColor); GL.TexCoord(uv_2); GL.MultiTexCoord3(1, vColor2.r, vColor2.g, vColor2.b); GL.Vertex(pos_2); // 2

		//					// Back Side
		//					GL.Color(vertexChannelColor); GL.TexCoord(uv_2); GL.MultiTexCoord3(1, vColor2.r, vColor2.g, vColor2.b); GL.Vertex(pos_2); // 2
		//					GL.Color(vertexChannelColor); GL.TexCoord(uv_1); GL.MultiTexCoord3(1, vColor1.r, vColor1.g, vColor1.b); GL.Vertex(pos_1); // 1
		//					GL.Color(vertexChannelColor); GL.TexCoord(uv_0); GL.MultiTexCoord3(1, vColor0.r, vColor0.g, vColor0.b); GL.Vertex(pos_0); // 0


		//					////if (iPass == 1)
		//					////{
		//					////	GL.MultiTexCoord3(1, 1.0f, 0.0f, 1.0f); GL.Color(vertexColor); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
		//					////	GL.MultiTexCoord3(1, 1.0f, 0.0f, 1.0f); GL.Color(vertexColor); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
		//					////	GL.MultiTexCoord3(1, 1.0f, 0.0f, 1.0f); GL.Color(vertexColor); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

		//					////	// Back Side
		//					////	GL.MultiTexCoord3(1, 1.0f, 0.0f, 1.0f); GL.Color(vertexColor); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
		//					////	GL.MultiTexCoord3(1, 1.0f, 0.0f, 1.0f); GL.Color(vertexColor); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
		//					////	GL.MultiTexCoord3(1, 1.0f, 0.0f, 1.0f); GL.Color(vertexColor); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
		//					////}
		//					////else
		//					////{
		//					//	GL.Color(vertexColor); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
		//					//	GL.Color(vertexColor); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
		//					//	GL.Color(vertexColor); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

		//					//	// Back Side
		//					//	GL.Color(vertexColor); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
		//					//	GL.Color(vertexColor); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
		//					//	GL.Color(vertexColor); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
		//					////}
		//				}


		//				//2. Clip Child를 그린다.
		//				for (int iClip = 0; iClip < 3; iClip++)
		//				{
		//					apMesh clipMesh = clipMeshes[iClip];
		//					apRenderUnit clipRenderUnit = childRenderUnits[iClip];
		//					if (clipMesh == null || clipRenderUnit == null)			{ continue; }
		//					if (clipRenderUnit._meshTransform == null)				{ continue; }
		//					if (!clipRenderUnit._isVisible)	{ continue; }


		//					switch (iClip)
		//					{
		//						case 0:
		//							vertexChannelColor = Color.red;
		//							posOffset.z = 0.4f;
		//							break;

		//						case 1:
		//							vertexChannelColor = Color.green;
		//							posOffset.z = 0.3f;
		//							break;

		//						case 2:
		//							vertexChannelColor = Color.blue;
		//							posOffset.z = 0.2f;
		//							break;
		//					}

		//					for (int i = 0; i < clipMesh._indexBuffer.Count; i += 3)
		//					{
		//						if (i + 2 >= clipMesh._indexBuffer.Count)
		//						{ break; }

		//						if (clipMesh._indexBuffer[i + 0] >= clipMesh._vertexData.Count ||
		//							clipMesh._indexBuffer[i + 1] >= clipMesh._vertexData.Count ||
		//							clipMesh._indexBuffer[i + 2] >= clipMesh._vertexData.Count)
		//						{
		//							break;
		//						}

		//						rVert0 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 0]];
		//						rVert1 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 1]];
		//						rVert2 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 2]];


		//						vColor0 = Color.black;
		//						vColor1 = Color.black;
		//						vColor2 = Color.black;

		//						if(isBoneWeightColor)
		//						{
		//							if(isBoneColor)
		//							{
		//								vColor0 = rVert0._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
		//								vColor1 = rVert1._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
		//								vColor2 = rVert2._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
		//							}
		//							else
		//							{
		//								vColor0 = GetWeightColor3(rVert0._renderWeightByTool);
		//								vColor1 = GetWeightColor3(rVert1._renderWeightByTool);
		//								vColor2 = GetWeightColor3(rVert2._renderWeightByTool);
		//							}
		//						}


		//						Vector3 pos_0 = World2GL(rVert0._pos_World3);
		//						Vector3 pos_1 = World2GL(rVert1._pos_World3);
		//						Vector3 pos_2 = World2GL(rVert2._pos_World3);

		//						pos_0 += posOffset;
		//						pos_1 += posOffset;
		//						pos_2 += posOffset;

		//						Vector2 uv_0 = clipMesh._vertexData[clipMesh._indexBuffer[i + 0]]._uv;
		//						Vector2 uv_1 = clipMesh._vertexData[clipMesh._indexBuffer[i + 1]]._uv;
		//						Vector2 uv_2 = clipMesh._vertexData[clipMesh._indexBuffer[i + 2]]._uv;


		//						GL.Color(vertexChannelColor); GL.TexCoord(uv_0); GL.MultiTexCoord3(1, vColor0.r, vColor0.g, vColor0.b); GL.Vertex(pos_0); // 0
		//						GL.Color(vertexChannelColor); GL.TexCoord(uv_1); GL.MultiTexCoord3(1, vColor1.r, vColor1.g, vColor1.b); GL.Vertex(pos_1); // 1
		//						GL.Color(vertexChannelColor); GL.TexCoord(uv_2); GL.MultiTexCoord3(1, vColor2.r, vColor2.g, vColor2.b); GL.Vertex(pos_2); // 2

		//						//Back Side
		//						GL.Color(vertexChannelColor); GL.TexCoord(uv_2); GL.MultiTexCoord3(1, vColor2.r, vColor2.g, vColor2.b); GL.Vertex(pos_2); // 2
		//						GL.Color(vertexChannelColor); GL.TexCoord(uv_1); GL.MultiTexCoord3(1, vColor1.r, vColor1.g, vColor1.b); GL.Vertex(pos_1); // 1
		//						GL.Color(vertexChannelColor); GL.TexCoord(uv_0); GL.MultiTexCoord3(1, vColor0.r, vColor0.g, vColor0.b); GL.Vertex(pos_0); // 0


		//						////if(iPass == 1)
		//						////{
		//						////	GL.MultiTexCoord3(2, 0.0f, 1.0f, 1.0f); GL.Color(vertexColor); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
		//						////	GL.MultiTexCoord3(2, 0.0f, 1.0f, 1.0f); GL.Color(vertexColor); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
		//						////	GL.MultiTexCoord3(2, 0.0f, 1.0f, 1.0f); GL.Color(vertexColor); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

		//						////	// Back Side
		//						////	GL.MultiTexCoord3(2, 0.0f, 1.0f, 1.0f); GL.Color(vertexColor); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
		//						////	GL.MultiTexCoord3(2, 0.0f, 1.0f, 1.0f); GL.Color(vertexColor); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
		//						////	GL.MultiTexCoord3(2, 0.0f, 1.0f, 1.0f); GL.Color(vertexColor); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
		//						////}
		//						////else
		//						////{
		//						//	GL.Color(vertexColor); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
		//						//	GL.Color(vertexColor); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
		//						//	GL.Color(vertexColor); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

		//						//	// Back Side
		//						//	GL.Color(vertexColor); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
		//						//	GL.Color(vertexColor); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
		//						//	GL.Color(vertexColor); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
		//						////}

		//					}
		//				}
		//				//------------------------------------------
		//				GL.End();
		//			}

		//			//사용했던 RenderTexture를 해제한다.
		//			_matBatch.ReleaseRenderTexture();

		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		Debug.LogException(ex);
		//	}
		//} 
		#endregion






		public static void DrawRenderUnit_ClippingParent_Renew(apRenderUnit renderUnit,
																RENDER_TYPE renderType,
																List<apTransform_Mesh.ClipMeshSet> childClippedSet,
																//List<apTransform_Mesh> childMeshTransforms, 
																//List<apRenderUnit> childRenderUnits, 
																apVertexController vertexController,
																apSelection select,
																RenderTexture externalRenderTexture = null)
		{
			//렌더링 순서
			//Parent - 기본
			//Parent - Mask
			//(For) Child - Clipped
			//Release RenderMask
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
				{
					return;
				}

				if (renderUnit._renderVerts.Count == 0)
				{
					return;
				}
				Color textureColor = renderUnit._meshColor2X;
				apMesh mesh = renderUnit._meshTransform._mesh;


				//if (mesh._textureData == null)
				if (mesh.LinkedTextureData == null)
				{
					return;
				}

				int nClipMeshes = childClippedSet.Count;


				bool isBoneWeightColor = (int)(renderType & RENDER_TYPE.BoneRigWeightColor) != 0;
				bool isPhyVolumeWeightColor = (renderType & RENDER_TYPE.PhysicsWeightColor) != 0 || (renderType & RENDER_TYPE.VolumeWeightColor) != 0;

				bool isBoneColor = false;
				float vertexColorRatio = 0.0f;

				bool isToneColor = (int)(renderType & RENDER_TYPE.ToneColor) != 0;

				if (select != null)
				{
					isBoneColor = select._rigEdit_isBoneColorView;
					if (isBoneWeightColor)
					{
						if (select._rigEdit_viewMode == apSelection.RIGGING_EDIT_VIEW_MODE.WeightColorOnly)
						{
							vertexColorRatio = 1.0f;
						}
						else
						{
							vertexColorRatio = 0.5f;
						}
					}
					else if (isPhyVolumeWeightColor)
					{
						vertexColorRatio = 0.7f;
					}
				}


				//렌더링 방식은 Mesh (with Color) 또는 Vertex / Outline이 있다.

				//1. Parent의 기본 렌더링을 하자
				//+2. Parent의 마스크를 렌더링하자
				if (mesh._indexBuffer.Count < 3)
				{
					return;
				}

				apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;

				Color vertexChannelColor = Color.black;
				Color vColor0 = Color.black, vColor1 = Color.black, vColor2 = Color.black;

				Vector2 posGL_0 = Vector2.zero;
				Vector2 posGL_1 = Vector2.zero;
				Vector2 posGL_2 = Vector2.zero;

				Vector3 pos_0 = Vector3.zero;
				Vector3 pos_1 = Vector3.zero;
				Vector3 pos_2 = Vector3.zero;
		
				Vector2 uv_0 = Vector2.zero;
				Vector2 uv_1 = Vector2.zero;
				Vector2 uv_2 = Vector2.zero;


				
				RenderTexture.active = null;

				for (int iPass = 0; iPass < 2; iPass++)
				{
					bool isRenderTexture = false;
					if (iPass == 1)
					{
						isRenderTexture = true;
					}
					if(isToneColor)
					{
						// ToneColor Mask
						//_matBatch.SetPass_Mask_ToneColor(_toneColor, mesh._textureData._image, isRenderTexture);
						_matBatch.SetPass_Mask_ToneColor(_toneColor, mesh.LinkedTextureData._image, isRenderTexture);
					}
					else
					{
						//일반적인 Mask
						//_matBatch.SetPass_Mask(textureColor, mesh._textureData._image, vertexColorRatio, renderUnit.ShaderType, isRenderTexture);
						_matBatch.SetPass_Mask(textureColor, mesh.LinkedTextureData._image, vertexColorRatio, renderUnit.ShaderType, isRenderTexture);
					}
					
					_matBatch.SetClippingSize(_glScreenClippingSize);


					GL.Begin(GL.TRIANGLES);
					//------------------------------------------
					for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
					{

						if (i + 2 >= mesh._indexBuffer.Count)
						{ break; }

						if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
						{
							break;
						}

						rVert0 = renderUnit._renderVerts[mesh._indexBuffer[i + 0]];
						rVert1 = renderUnit._renderVerts[mesh._indexBuffer[i + 1]];
						rVert2 = renderUnit._renderVerts[mesh._indexBuffer[i + 2]];

						vColor0 = Color.black;
						vColor1 = Color.black;
						vColor2 = Color.black;

						if (isBoneWeightColor)
						{
							if (isBoneColor)
							{
								vColor0 = rVert0._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
								vColor1 = rVert1._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
								vColor2 = rVert2._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
							}
							else
							{
								vColor0 = GetWeightColor3(rVert0._renderWeightByTool);
								vColor1 = GetWeightColor3(rVert1._renderWeightByTool);
								vColor2 = GetWeightColor3(rVert2._renderWeightByTool);
							}
						}
						else if (isPhyVolumeWeightColor)
						{
							vColor0 = GetWeightGrayscale(rVert0._renderWeightByTool);
							vColor1 = GetWeightGrayscale(rVert1._renderWeightByTool);
							vColor2 = GetWeightGrayscale(rVert2._renderWeightByTool);
						}

						posGL_0 = World2GL(rVert0._pos_World);
						posGL_1 = World2GL(rVert1._pos_World);
						posGL_2 = World2GL(rVert2._pos_World);

						pos_0.x = posGL_0.x;
						pos_0.y = posGL_0.y;
						pos_0.z = rVert0._vertex._zDepth * 0.5f;

						pos_1.x = posGL_1.x;
						pos_1.y = posGL_1.y;
						pos_1.z = rVert1._vertex._zDepth * 0.5f;

						pos_2.x = posGL_2.x;
						pos_2.y = posGL_2.y;
						pos_2.z = rVert2._vertex._zDepth * 0.5f;
						
						uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
						uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
						uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;

						
						GL.Color(vColor0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						GL.Color(vColor1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(vColor2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

						// Back Side
						GL.Color(vColor2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
						GL.Color(vColor1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(vColor0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
					}



					//------------------------------------------
					GL.End();
				}

				if (externalRenderTexture == null)
				{
					_matBatch.DeactiveRenderTexture();
				}
				else
				{
					RenderTexture.active = externalRenderTexture;
				}

				

				//3. Child를 렌더링하자
				//for (int iClip = 0; iClip < 3; iClip++)
				for (int iClip = 0; iClip < nClipMeshes; iClip++)
				{
					//posOffset.z += 0.5f;
					//apMesh clipMesh = clipMeshes[iClip];
					//apRenderUnit clipRenderUnit = childRenderUnits[iClip];
					if(childClippedSet[iClip] == null || childClippedSet[iClip]._meshTransform == null)
					{
						continue;
					}
					apMesh clipMesh = childClippedSet[iClip]._meshTransform._mesh;
					apRenderUnit clipRenderUnit = childClippedSet[iClip]._renderUnit;

					if (clipMesh == null || clipRenderUnit == null) { continue; }
					if (clipRenderUnit._meshTransform == null) { continue; }
					if (!clipRenderUnit._isVisible) { continue; }

					if (clipMesh._indexBuffer.Count < 3)
					{
						continue;
					}

					if(isToneColor)
					{
						//Onion ToneColor Clipping
						//_matBatch.SetPass_Clipped_ToneColor(_toneColor, clipMesh._textureData._image, renderUnit._meshColor2X);
						_matBatch.SetPass_Clipped_ToneColor(_toneColor, clipMesh.LinkedTextureData._image, renderUnit._meshColor2X);
					}
					else
					{
						//일반 Clipping
						//_matBatch.SetPass_Clipped(clipRenderUnit._meshColor2X, clipMesh._textureData._image, vertexColorRatio, clipRenderUnit.ShaderType, renderUnit._meshColor2X);
						_matBatch.SetPass_Clipped(clipRenderUnit._meshColor2X, clipMesh.LinkedTextureData._image, vertexColorRatio, clipRenderUnit.ShaderType, renderUnit._meshColor2X);
					}
					
					_matBatch.SetClippingSize(_glScreenClippingSize);

					GL.Begin(GL.TRIANGLES);
					//------------------------------------------
					//try
					//{
						for (int i = 0; i < clipMesh._indexBuffer.Count; i += 3)
						{
							if (i + 2 >= clipMesh._indexBuffer.Count)
							{ break; }

							if (clipMesh._indexBuffer[i + 0] >= clipMesh._vertexData.Count ||
								clipMesh._indexBuffer[i + 1] >= clipMesh._vertexData.Count ||
								clipMesh._indexBuffer[i + 2] >= clipMesh._vertexData.Count)
							{
								break;
							}

							rVert0 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 0]];
							rVert1 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 1]];
							rVert2 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 2]];


							vColor0 = Color.black;
							vColor1 = Color.black;
							vColor2 = Color.black;

							if (isBoneWeightColor)
							{
								if (isBoneColor)
								{
									vColor0 = rVert0._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
									vColor1 = rVert1._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
									vColor2 = rVert2._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
								}
								else
								{
									vColor0 = GetWeightColor3(rVert0._renderWeightByTool);
									vColor1 = GetWeightColor3(rVert1._renderWeightByTool);
									vColor2 = GetWeightColor3(rVert2._renderWeightByTool);
								}
							}
							else if (isPhyVolumeWeightColor)
							{
								vColor0 = GetWeightGrayscale(rVert0._renderWeightByTool);
								vColor1 = GetWeightGrayscale(rVert1._renderWeightByTool);
								vColor2 = GetWeightGrayscale(rVert2._renderWeightByTool);
							}



							posGL_0 = World2GL(rVert0._pos_World);
							posGL_1 = World2GL(rVert1._pos_World);
							posGL_2 = World2GL(rVert2._pos_World);

							pos_0.x = posGL_0.x;
							pos_0.y = posGL_0.y;
							pos_0.z = rVert0._vertex._zDepth * 0.5f;

							pos_1.x = posGL_1.x;
							pos_1.y = posGL_1.y;
							pos_1.z = rVert1._vertex._zDepth * 0.5f;

							pos_2.x = posGL_2.x;
							pos_2.y = posGL_2.y;
							pos_2.z = rVert2._vertex._zDepth * 0.5f;

							uv_0 = clipMesh._vertexData[clipMesh._indexBuffer[i + 0]]._uv;
							uv_1 = clipMesh._vertexData[clipMesh._indexBuffer[i + 1]]._uv;
							uv_2 = clipMesh._vertexData[clipMesh._indexBuffer[i + 2]]._uv;


							GL.Color(vColor0);
							GL.TexCoord(uv_0);
							GL.Vertex(pos_0); // 0
							GL.Color(vColor1);
							GL.TexCoord(uv_1);
							GL.Vertex(pos_1); // 1
							GL.Color(vColor2);
							GL.TexCoord(uv_2);
							GL.Vertex(pos_2); // 2

							//Back Side
							GL.Color(vColor2);
							GL.TexCoord(uv_2);
							GL.Vertex(pos_2); // 2
							GL.Color(vColor1);
							GL.TexCoord(uv_1);
							GL.Vertex(pos_1); // 1
							GL.Color(vColor0);
							GL.TexCoord(uv_0);
							GL.Vertex(pos_0); // 0


						}
					//}
					//catch(Exception ex)
					//{
					//	Debug.LogError("Draw Render Unit Clipping Parent Exception : " + ex);
					//	Debug.LogError("Index Buffer : " + clipMesh._indexBuffer.Count);
					//	Debug.LogError("Mesh Vertex : " + clipMesh._vertexData.Count);
					//	Debug.LogError("Render Unit Vertex : " + clipRenderUnit._renderVerts.Count);
						
					//}
					//------------------------------------------------
					GL.End();

				}

				//사용했던 RenderTexture를 해제한다.
				_matBatch.ReleaseRenderTexture();
				//_matBatch.DeactiveRenderTexture();

			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}




		/// <summary>
		/// Clipping Render의 Mask Texture만 취하는 함수
		/// RTT 후 실제 Texture2D로 굽기 때문에 실시간으로는 사용하기 힘들다.
		/// 클리핑을 하지 않는다.
		/// </summary>
		/// <param name="renderUnit"></param>
		/// <returns></returns>
		public static Texture2D GetMaskTexture_ClippingParent(apRenderUnit renderUnit)
		{
			//렌더링 순서
			//Parent - 기본
			//Parent - Mask
			//(For) Child - Clipped
			//Release RenderMask
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
				{
					return null;
				}

				if (renderUnit._renderVerts.Count == 0)
				{
					return null;
				}
				apMesh mesh = renderUnit._meshTransform._mesh;


				//if (mesh._textureData == null)
				if (mesh.LinkedTextureData == null)
				{
					return null;
				}

				//렌더링 방식은 Mesh (with Color) 또는 Vertex / Outline이 있다.

				//1. Parent의 기본 렌더링을 하자
				//+2. Parent의 마스크를 렌더링하자
				if (mesh._indexBuffer.Count < 3)
				{
					return null;
				}

				apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;

				//Pass는 RTT용 Pass 한개만 둔다.
				bool isRenderTexture = true; //<<RTT만 한다.
				//_matBatch.SetPass_Mask(Color.gray, mesh._textureData._image, 0.0f, renderUnit.ShaderType, isRenderTexture);
				_matBatch.SetPass_Mask(Color.gray, mesh.LinkedTextureData._image, 0.0f, renderUnit.ShaderType, isRenderTexture);

				_matBatch.SetClippingSize(new Vector4(0, 0, 1, 1));//<<클리핑을 하지 않는다.

				Vector2 posGL_0 = Vector2.zero;
				Vector2 posGL_1 = Vector2.zero;
				Vector2 posGL_2 = Vector2.zero;

				Vector3 pos_0 = Vector3.zero;
				Vector3 pos_1 = Vector3.zero;
				Vector3 pos_2 = Vector3.zero;

				Vector2 uv_0 = Vector2.zero;
				Vector2 uv_1 = Vector2.zero;
				Vector2 uv_2 = Vector2.zero;

				GL.Begin(GL.TRIANGLES);
				//------------------------------------------
				for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
				{

					if (i + 2 >= mesh._indexBuffer.Count)
					{ break; }

					if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
						mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
						mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
					{
						break;
					}

					rVert0 = renderUnit._renderVerts[mesh._indexBuffer[i + 0]];
					rVert1 = renderUnit._renderVerts[mesh._indexBuffer[i + 1]];
					rVert2 = renderUnit._renderVerts[mesh._indexBuffer[i + 2]];

					posGL_0 = World2GL(rVert0._pos_World);
					posGL_1 = World2GL(rVert1._pos_World);
					posGL_2 = World2GL(rVert2._pos_World);


					pos_0.x = posGL_0.x;
					pos_0.y = posGL_0.y;
					pos_0.z = rVert0._vertex._zDepth * 0.5f;

					pos_1.x = posGL_1.x;
					pos_1.y = posGL_1.y;
					pos_1.z = rVert1._vertex._zDepth * 0.5f;			   

					pos_2.x = posGL_2.x;
					pos_2.y = posGL_2.y;
					pos_2.z = rVert2._vertex._zDepth * 0.5f;			   

					uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
					uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
					uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;


					GL.Color(Color.black); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
					GL.Color(Color.black); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
					GL.Color(Color.black); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

					// Back Side
					GL.Color(Color.black); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
					GL.Color(Color.black); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
					GL.Color(Color.black); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
				}



				//------------------------------------------
				GL.End();

				//Texture2D로 굽자
				Texture2D resultTex = new Texture2D(_matBatch.RenderTex.width, _matBatch.RenderTex.height, TextureFormat.RGBA32, false);
				resultTex.ReadPixels(new Rect(0, 0, _matBatch.RenderTex.width, _matBatch.RenderTex.height), 0, 0);
				resultTex.Apply();

				//사용했던 RenderTexture를 해제한다.
				_matBatch.ReleaseRenderTexture();
				//_matBatch.DeactiveRenderTexture();

				return resultTex;

			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return null;
		}


		/// <summary>
		/// RTT 없이 "이미 구워진 MaskTexture"를 이용해서 Clipping 렌더링을 한다.
		/// 클리핑을 하지 않는다.
		/// </summary>
		/// <param name="renderUnit"></param>
		/// <param name="renderType"></param>
		/// <param name="childClippedSet"></param>
		/// <param name="vertexController"></param>
		/// <param name="select"></param>
		/// <param name="externalRenderTexture"></param>
		public static void DrawRenderUnit_ClippingParent_Renew_WithoutRTT(apRenderUnit renderUnit,
																			List<apTransform_Mesh.ClipMeshSet> childClippedSet,
																			Texture2D maskedTexture
																		)
		{
			//렌더링 순서
			//Parent - 기본
			//Parent - Mask
			//(For) Child - Clipped
			//Release RenderMask
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
				{
					return;
				}

				if (renderUnit._renderVerts.Count == 0)
				{
					return;
				}
				Color textureColor = renderUnit._meshColor2X;
				apMesh mesh = renderUnit._meshTransform._mesh;


				//if (mesh._textureData == null)
				if (mesh.LinkedTextureData == null)
				{
					return;
				}

				int nClipMeshes = childClippedSet.Count;


				//렌더링 방식은 Mesh (with Color) 또는 Vertex / Outline이 있다.

				//1. Parent의 기본 렌더링을 하자
				//+2. Parent의 마스크를 렌더링하자
				if (mesh._indexBuffer.Count < 3)
				{
					return;
				}

				apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;

				Color vertexChannelColor = Color.black;
				Color vColor0 = Color.black, vColor1 = Color.black, vColor2 = Color.black;


				Vector2 posGL_0 = Vector2.zero;
				Vector2 posGL_1 = Vector2.zero;
				Vector2 posGL_2 = Vector2.zero;

				Vector3 pos_0 = Vector3.zero;
				Vector3 pos_1 = Vector3.zero;
				Vector3 pos_2 = Vector3.zero;

				Vector2 uv_0 = Vector2.zero;
				Vector2 uv_1 = Vector2.zero;
				Vector2 uv_2 = Vector2.zero;

				//RTT 관련 코드는 모두 뺀다. Pass도 한번이고 기본 렌더링
				//_matBatch.SetPass_Texture_VColor(textureColor, mesh._textureData._image, 0.0f, renderUnit.ShaderType);
				_matBatch.SetPass_Texture_VColor(textureColor, mesh.LinkedTextureData._image, 0.0f, renderUnit.ShaderType);
				
				//_matBatch.SetClippingSize(_glScreenClippingSize);
				_matBatch.SetClippingSize(new Vector4(0, 0, 1, 1));//<<클리핑을 하지 않는다.


				GL.Begin(GL.TRIANGLES);
				//------------------------------------------
				for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
				{

					if (i + 2 >= mesh._indexBuffer.Count)
					{ break; }

					if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
						mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
						mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
					{
						break;
					}

					rVert0 = renderUnit._renderVerts[mesh._indexBuffer[i + 0]];
					rVert1 = renderUnit._renderVerts[mesh._indexBuffer[i + 1]];
					rVert2 = renderUnit._renderVerts[mesh._indexBuffer[i + 2]];

					vColor0 = Color.black;
					vColor1 = Color.black;
					vColor2 = Color.black;

					posGL_0 = World2GL(rVert0._pos_World);
					posGL_1 = World2GL(rVert1._pos_World);
					posGL_2 = World2GL(rVert2._pos_World);

					pos_0.x = posGL_0.x;
					pos_0.y = posGL_0.y;
					pos_0.z = rVert0._vertex._zDepth * 0.5f;

					pos_1.x = posGL_1.x;
					pos_1.y = posGL_1.y;
					pos_1.z = rVert1._vertex._zDepth * 0.5f;

					pos_2.x = posGL_2.x;
					pos_2.y = posGL_2.y;
					pos_2.z = rVert2._vertex._zDepth * 0.5f;
					


					uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
					uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
					uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;


					GL.Color(vColor0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
					GL.Color(vColor1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
					GL.Color(vColor2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

					// Back Side
					GL.Color(vColor2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
					GL.Color(vColor1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
					GL.Color(vColor0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
				}



				//------------------------------------------
				GL.End();


				//3. Child를 렌더링하자. MaskedTexture를 직접 이용
				for (int iClip = 0; iClip < nClipMeshes; iClip++)
				{
					apMesh clipMesh = childClippedSet[iClip]._meshTransform._mesh;
					apRenderUnit clipRenderUnit = childClippedSet[iClip]._renderUnit;

					if (clipMesh == null || clipRenderUnit == null)		{ continue; }
					if (clipRenderUnit._meshTransform == null)			{ continue; }
					if (!clipRenderUnit._isVisible)						{ continue; }

					if (clipMesh._indexBuffer.Count < 3)
					{
						continue;
					}

					_matBatch.SetPass_ClippedWithMaskedTexture(clipRenderUnit._meshColor2X,
																//clipMesh._textureData._image,
																clipMesh.LinkedTextureData._image,
																//maskedTexture,
																0.0f,
																clipRenderUnit.ShaderType,
																renderUnit._meshColor2X,
																maskedTexture);

					//_matBatch.SetClippingSize(_glScreenClippingSize);
					_matBatch.SetClippingSize(new Vector4(0, 0, 1, 1));//<<클리핑을 하지 않는다.

					GL.Begin(GL.TRIANGLES);
					//------------------------------------------
					for (int i = 0; i < clipMesh._indexBuffer.Count; i += 3)
					{
						if (i + 2 >= clipMesh._indexBuffer.Count)
						{ break; }

						if (clipMesh._indexBuffer[i + 0] >= clipMesh._vertexData.Count ||
							clipMesh._indexBuffer[i + 1] >= clipMesh._vertexData.Count ||
							clipMesh._indexBuffer[i + 2] >= clipMesh._vertexData.Count)
						{
							break;
						}

						rVert0 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 0]];
						rVert1 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 1]];
						rVert2 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 2]];


						vColor0 = Color.black;
						vColor1 = Color.black;
						vColor2 = Color.black;


						posGL_0 = World2GL(rVert0._pos_World);
						posGL_1 = World2GL(rVert1._pos_World);
						posGL_2 = World2GL(rVert2._pos_World);

						pos_0.x = posGL_0.x;
						pos_0.y = posGL_0.y;
						pos_0.z = rVert0._vertex._zDepth * 0.5f;

						pos_1.x = posGL_1.x;
						pos_1.y = posGL_1.y;
						pos_1.z = rVert1._vertex._zDepth * 0.5f;

						pos_2.x = posGL_2.x;
						pos_2.y = posGL_2.y;
						pos_2.z = rVert2._vertex._zDepth * 0.5f;


						uv_0 = clipMesh._vertexData[clipMesh._indexBuffer[i + 0]]._uv;
						uv_1 = clipMesh._vertexData[clipMesh._indexBuffer[i + 1]]._uv;
						uv_2 = clipMesh._vertexData[clipMesh._indexBuffer[i + 2]]._uv;


						GL.Color(vColor0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						GL.Color(vColor1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(vColor2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

						//Back Side
						GL.Color(vColor2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
						GL.Color(vColor1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(vColor0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0


					}
					//------------------------------------------------
					GL.End();

				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}


		//------------------------------------------------------------------------------------------------
		// Draw Transform Border Form of Render Unit
		//------------------------------------------------------------------------------------------------
		public static void DrawTransformBorderFormOfRenderUnit(Color lineColor, float posL, float posR, float posT, float posB, apMatrix3x3 worldMatrix)
		{
			float marginOffset = 10;
			posL -= marginOffset;
			posR += marginOffset;
			posT += marginOffset;
			posB -= marginOffset;

			//Vector3 pos3W_LT = worldMatrix.MultiplyPoint3x4(new Vector3(posL, posT, 0));
			//Vector3 pos3W_RT = worldMatrix.MultiplyPoint3x4(new Vector3(posR, posT, 0));
			//Vector3 pos3W_LB = worldMatrix.MultiplyPoint3x4(new Vector3(posL, posB, 0));
			//Vector3 pos3W_RB = worldMatrix.MultiplyPoint3x4(new Vector3(posR, posB, 0));

			//Vector2 posW_LT = new Vector2(pos3W_LT.x, pos3W_LT.y);
			//Vector2 posW_RT = new Vector2(pos3W_RT.x, pos3W_RT.y);
			//Vector2 posW_LB = new Vector2(pos3W_LB.x, pos3W_LB.y);
			//Vector2 posW_RB = new Vector2(pos3W_RB.x, pos3W_RB.y);


			Vector2 posW_LT = worldMatrix.MultiplyPoint(new Vector2(posL, posT));
			Vector2 posW_RT = worldMatrix.MultiplyPoint(new Vector2(posR, posT));
			Vector2 posW_LB = worldMatrix.MultiplyPoint(new Vector2(posL, posB));
			Vector2 posW_RB = worldMatrix.MultiplyPoint(new Vector2(posR, posB));


			float tfFormLineLength = 32.0f;

			_matBatch.SetPass_Color();
			_matBatch.SetClippingSize(_glScreenClippingSize);
			GL.Begin(GL.LINES);

			DrawLine(posW_LT, GetUnitLineEndPoint(posW_LT, posW_RT, tfFormLineLength), lineColor, false);
			DrawLine(posW_RT, GetUnitLineEndPoint(posW_RT, posW_RB, tfFormLineLength), lineColor, false);
			DrawLine(posW_RB, GetUnitLineEndPoint(posW_RB, posW_LB, tfFormLineLength), lineColor, false);
			DrawLine(posW_LB, GetUnitLineEndPoint(posW_LB, posW_LT, tfFormLineLength), lineColor, false);

			DrawLine(posW_LT, GetUnitLineEndPoint(posW_LT, posW_LB, tfFormLineLength), lineColor, false);
			DrawLine(posW_LB, GetUnitLineEndPoint(posW_LB, posW_RB, tfFormLineLength), lineColor, false);
			DrawLine(posW_RB, GetUnitLineEndPoint(posW_RB, posW_RT, tfFormLineLength), lineColor, false);
			DrawLine(posW_RT, GetUnitLineEndPoint(posW_RT, posW_LT, tfFormLineLength), lineColor, false);

			GL.End();
		}

		private static Vector2 GetUnitLineEndPoint(Vector2 startPos, Vector2 endPos, float maxLength)
		{
			Vector2 dir = endPos - startPos;
			if (dir.sqrMagnitude <= maxLength * maxLength)
			{
				return endPos;
			}
			return startPos + dir.normalized * maxLength;
		}


		//------------------------------------------------------------------------------------------------
		// Draw Bone
		//------------------------------------------------------------------------------------------------
		//private static Color _boneColor_Selected = new Color(1.0f, 0.1f, 0.0f, 1.0f);

		//public static void DrawBone(apBone bone, apSelection select)
		public static void DrawBone(apBone bone, bool isDrawOutline)
		{
			if (bone == null)
			{
				return;
			}

			Color boneColor = bone._color;

			Color boneOutlineColor = boneColor * 0.5f;
			boneOutlineColor.a = 1.0f;

			
			apMatrix worldMatrix = bone._worldMatrix;
			Vector2 posW_Start = worldMatrix._pos;

			bool isHelperBone = bone._shapeHelper;
			Vector2 posGL_Start = apGL.World2GL(posW_Start);
			Vector2 posGL_Mid1 = apGL.World2GL(bone._shapePoint_Mid1);
			Vector2 posGL_Mid2 = apGL.World2GL(bone._shapePoint_Mid2);
			Vector2 posGL_End1 = apGL.World2GL(bone._shapePoint_End1);
			Vector2 posGL_End2 = apGL.World2GL(bone._shapePoint_End2);


			float orgSize = 10.0f * Zoom;
			Vector3 orgPos_Up = new Vector3(posGL_Start.x, posGL_Start.y + orgSize, 0);
			Vector3 orgPos_Left = new Vector3(posGL_Start.x - orgSize, posGL_Start.y, 0);
			Vector3 orgPos_Down = new Vector3(posGL_Start.x, posGL_Start.y - orgSize, 0);
			Vector3 orgPos_Right = new Vector3(posGL_Start.x + orgSize, posGL_Start.y, 0);

			if (!isDrawOutline)
			{
				//1. 전부다 그릴때
				_matBatch.SetPass_Color();
				_matBatch.SetClippingSize(_glScreenClippingSize);

				if (!isHelperBone)//<헬퍼가 아닐때
				{
					GL.Begin(GL.TRIANGLES);

					GL.Color(boneColor);

					//1. 사다리꼴 모양을 먼저 그리자
					//    [End1]    [End2]
					//
					//
					//
					//[Mid1]            [Mid2]
					//        [Start]

					//1) Start - Mid1 - End1
					//2) Start - Mid2 - End2
					//3) Start - End1 - End2

					//1) Start - Mid1 - End1
					GL.Vertex(posGL_Start);
					GL.Vertex(posGL_Mid1);
					GL.Vertex(posGL_End1);
					GL.Vertex(posGL_Start);
					GL.Vertex(posGL_End1);
					GL.Vertex(posGL_Mid1);

					//2) Start - Mid2 - End2
					GL.Vertex(posGL_Start);
					GL.Vertex(posGL_Mid2);
					GL.Vertex(posGL_End2);
					GL.Vertex(posGL_Start);
					GL.Vertex(posGL_End2);
					GL.Vertex(posGL_Mid2);

					//3) Start - End1 - End2 (taper가 100 미만일 때)
					if (bone._shapeTaper < 100)
					{
						GL.Vertex(posGL_Start);
						GL.Vertex(posGL_End1);
						GL.Vertex(posGL_End2);
						GL.Vertex(posGL_Start);
						GL.Vertex(posGL_End2);
						GL.Vertex(posGL_End1);
					}
					GL.End();

					GL.Begin(GL.LINES);

					DrawLineGL(posGL_Start, posGL_Mid1, boneOutlineColor, false);
					DrawLineGL(posGL_Mid1, posGL_End1, boneOutlineColor, false);
					DrawLineGL(posGL_End1, posGL_End2, boneOutlineColor, false);
					DrawLineGL(posGL_End2, posGL_Mid2, boneOutlineColor, false);
					DrawLineGL(posGL_Mid2, posGL_Start, boneOutlineColor, false);

					GL.End();
				}

				GL.Begin(GL.TRIANGLES);

				GL.Color(boneColor);

				//2. 원점 부분은 다각형 형태로 다시 그려주자
				//다이아몬드 형태로..



				//       Up
				// Left  |   Right
				//      Down

				GL.Vertex(orgPos_Up);
				GL.Vertex(orgPos_Left);
				GL.Vertex(orgPos_Down);
				GL.Vertex(orgPos_Up);
				GL.Vertex(orgPos_Down);
				GL.Vertex(orgPos_Left);

				GL.Vertex(orgPos_Up);
				GL.Vertex(orgPos_Right);
				GL.Vertex(orgPos_Down);
				GL.Vertex(orgPos_Up);
				GL.Vertex(orgPos_Down);
				GL.Vertex(orgPos_Right);

				GL.End();

				GL.Begin(GL.LINES);

				DrawLineGL(orgPos_Up, orgPos_Left, boneOutlineColor, false);
				DrawLineGL(orgPos_Left, orgPos_Down, boneOutlineColor, false);
				DrawLineGL(orgPos_Down, orgPos_Right, boneOutlineColor, false);
				DrawLineGL(orgPos_Right, orgPos_Up, boneOutlineColor, false);

				GL.End();
			}
			else
			{
				_matBatch.SetPass_Color();
				_matBatch.SetClippingSize(_glScreenClippingSize);

				//2. Outline만 그릴때
				//1> 헬퍼가 아니라면 사다리꼴만
				//2> 헬퍼라면 다이아몬드만
				GL.Begin(GL.LINES);
				if (!isHelperBone)
				{
					DrawLineGL(posGL_Start, posGL_Mid1, boneColor, false);
					DrawLineGL(posGL_Mid1, posGL_End1, boneColor, false);
					DrawLineGL(posGL_End1, posGL_End2, boneColor, false);
					DrawLineGL(posGL_End2, posGL_Mid2, boneColor, false);
					DrawLineGL(posGL_Mid2, posGL_Start, boneColor, false);
				}
				else
				{
					DrawLineGL(orgPos_Up, orgPos_Left, boneColor, false);
					DrawLineGL(orgPos_Left, orgPos_Down, boneColor, false);
					DrawLineGL(orgPos_Down, orgPos_Right, boneColor, false);
					DrawLineGL(orgPos_Right, orgPos_Up, boneColor, false);
				}

				GL.End();
			}


		}

		public static void DrawSelectedBone(apBone bone, bool isMainSelect = true)
		{
			if (bone == null)
			{
				return;
			}

			apMatrix worldMatrix = bone._worldMatrix;
			Vector2 posW_Start = worldMatrix._pos;

			bool isHelperBone = bone._shapeHelper;
			Vector2 posGL_Start = apGL.World2GL(posW_Start);
			Vector2 posGL_Mid1 = apGL.World2GL(bone._shapePoint_Mid1);
			Vector2 posGL_Mid2 = apGL.World2GL(bone._shapePoint_Mid2);
			Vector2 posGL_End1 = apGL.World2GL(bone._shapePoint_End1);
			Vector2 posGL_End2 = apGL.World2GL(bone._shapePoint_End2);

			//2. 원점 부분은 다각형 형태로 다시 그려주자
			//다이아몬드 형태로..
			float orgSize = 10.0f * Zoom;
			Vector3 orgPos_Up = new Vector3(posGL_Start.x, posGL_Start.y + orgSize, 0);
			Vector3 orgPos_Left = new Vector3(posGL_Start.x - orgSize, posGL_Start.y, 0);
			Vector3 orgPos_Down = new Vector3(posGL_Start.x, posGL_Start.y - orgSize, 0);
			Vector3 orgPos_Right = new Vector3(posGL_Start.x + orgSize, posGL_Start.y, 0);

			_matBatch.SetPass_Color();
			_matBatch.SetClippingSize(_glScreenClippingSize);

			GL.Begin(GL.TRIANGLES);
			Color lineColor = _lineColor_BoneOutline;
			if (!isMainSelect)
			{
				lineColor = _lineColor_BoneOutlineRollOver;
			}
			float lineThickness = 8.0f;

			if (!isHelperBone)
			{
				//헬퍼가 아닐때
				//1. 사다리꼴 모양을 먼저 그리자
				//    [End1]    [End2]
				//
				//
				//
				//[Mid1]            [Mid2]
				//        [Start]

				//1) Start - Mid1 - End1
				//2) Start - Mid2 - End2
				//3) Start - End1 - End2

				//1) Start - Mid1 - End1
				

				
				DrawBoldLineGL(posGL_Start, posGL_Mid1, lineThickness, lineColor, false);
				DrawBoldLineGL(posGL_Mid1, posGL_End1, lineThickness, lineColor, false);

				if (bone._shapeTaper < 100)
				{
					DrawBoldLineGL(posGL_End1, posGL_End2, lineThickness, lineColor, false);
				}
				DrawBoldLineGL(posGL_End2, posGL_Mid2, lineThickness, lineColor, false);
				DrawBoldLineGL(posGL_Mid2, posGL_Start, lineThickness, lineColor, false);
			}
			DrawBoldLineGL(orgPos_Up, orgPos_Left, lineThickness, lineColor, false);
			DrawBoldLineGL(orgPos_Left, orgPos_Down, lineThickness, lineColor, false);
			DrawBoldLineGL(orgPos_Down, orgPos_Right, lineThickness, lineColor, false);
			DrawBoldLineGL(orgPos_Right, orgPos_Up, lineThickness, lineColor, false);

			GL.End();


		}


		public static void DrawSelectedBonePost(apBone bone)
		{
			if (bone == null)
			{
				return;
			}

			if (bone._isIKAngleRange && bone._parentBone != null)
			{
				apMatrix worldMatrix = bone._parentBone._worldMatrix;
				Vector2 posW_Start = bone._worldMatrix._pos;

				Vector2 unitVector = worldMatrix.MtrxOnlyRotation.MultiplyPoint(new Vector2(0, 1));
				Vector2 unitVector_Lower = apMatrix3x3.TRS(Vector2.zero, bone._defaultMatrix._angleDeg + bone._IKAngleRange_Lower, Vector2.one).MultiplyPoint(unitVector);
				Vector2 unitVector_Upper = apMatrix3x3.TRS(Vector2.zero, bone._defaultMatrix._angleDeg + bone._IKAngleRange_Upper, Vector2.one).MultiplyPoint(unitVector);
				Vector2 unitVector_Pref = apMatrix3x3.TRS(Vector2.zero, bone._defaultMatrix._angleDeg + bone._IKAnglePreferred, Vector2.one).MultiplyPoint(unitVector);

				unitVector_Lower.Normalize();
				unitVector_Upper.Normalize();
				unitVector_Pref.Normalize();

				unitVector_Lower *= bone._shapeLength * bone._worldMatrix._scale.y * 1.2f;
				unitVector_Upper *= bone._shapeLength * bone._worldMatrix._scale.y * 1.2f;
				unitVector_Pref *= bone._shapeLength * bone._worldMatrix._scale.y * 1.5f;

				DrawBoldLine(posW_Start, posW_Start + new Vector2(unitVector_Lower.x, unitVector_Lower.y), 3, Color.magenta, true);
				DrawBoldLine(posW_Start, posW_Start + new Vector2(unitVector_Upper.x, unitVector_Upper.y), 3, Color.magenta, true);
				DrawBoldLine(posW_Start, posW_Start + new Vector2(unitVector_Pref.x, unitVector_Pref.y), 3, Color.green, true);


			}

			//if(bone._isIKtargetDebug)
			//{
			//	DrawBox(bone._calculatedIKTargetPosDebug, 30, 30, new Color(1.0f, 0.0f, 1.0f, 1.0f), false);
			//	int nBosDebug = bone._calculatedIKBonePosDebug.Count;
			//	for (int i = 0; i < nBosDebug; i++)
			//	{
			//		Color debugColor = (new Color(0.0f, 1.0f, 0.0f, 1.0f) * ((nBosDebug - 1)- i) + new Color(0.0f, 0.0f, 1.0f, 1.0f) * i) / (float)(nBosDebug - 1);

			//		DrawBox(bone._calculatedIKBonePosDebug[i], 20 + i * 5, 20 + i * 5, debugColor, false);
			//	}

			//}
		}



		public static void DrawBoneOutline(apBone bone, Color outlineColor)
		{
			if (bone == null)
			{
				return;
			}

			
			apMatrix worldMatrix = bone._worldMatrix;
			Vector2 posW_Start = worldMatrix._pos;

			bool isHelperBone = bone._shapeHelper;
			Vector2 posGL_Start = apGL.World2GL(posW_Start);
			Vector2 posGL_Mid1 = apGL.World2GL(bone._shapePoint_Mid1);
			Vector2 posGL_Mid2 = apGL.World2GL(bone._shapePoint_Mid2);
			Vector2 posGL_End1 = apGL.World2GL(bone._shapePoint_End1);
			Vector2 posGL_End2 = apGL.World2GL(bone._shapePoint_End2);


			float orgSize = 10.0f * Zoom;
			Vector3 orgPos_Up = new Vector3(posGL_Start.x, posGL_Start.y + orgSize, 0);
			Vector3 orgPos_Left = new Vector3(posGL_Start.x - orgSize, posGL_Start.y, 0);
			Vector3 orgPos_Down = new Vector3(posGL_Start.x, posGL_Start.y - orgSize, 0);
			Vector3 orgPos_Right = new Vector3(posGL_Start.x + orgSize, posGL_Start.y, 0);

			_matBatch.SetPass_Color();
			_matBatch.SetClippingSize(_glScreenClippingSize);

			//2. Outline만 그릴때
			//1> 헬퍼가 아니라면 사다리꼴만
			//2> 헬퍼라면 다이아몬드만
			float width = 3.0f;
			GL.Begin(GL.TRIANGLES);
			if (!isHelperBone)
			{
				DrawBoldLineGL(posGL_Start, posGL_Mid1, width, outlineColor, false);
				DrawBoldLineGL(posGL_Mid1, posGL_End1, width, outlineColor, false);
				if (Mathf.Abs(posGL_End1.x - posGL_End2.x) > 2f
					&& Mathf.Abs(posGL_End1.y - posGL_End2.y) > 2f)
				{
					DrawBoldLineGL(posGL_End1, posGL_End2, width, outlineColor, false);
				}
				DrawBoldLineGL(posGL_End2, posGL_Mid2, width, outlineColor, false);
				DrawBoldLineGL(posGL_Mid2, posGL_Start, width, outlineColor, false);
			}
			else
			{
				DrawBoldLineGL(orgPos_Up, orgPos_Left, width, outlineColor, false);
				DrawBoldLineGL(orgPos_Left, orgPos_Down, width, outlineColor, false);
				DrawBoldLineGL(orgPos_Down, orgPos_Right, width, outlineColor, false);
				DrawBoldLineGL(orgPos_Right, orgPos_Up, width, outlineColor, false);
			}

			GL.End();


		}


		//------------------------------------------------------------------------------------------------
		// Draw Grid
		//------------------------------------------------------------------------------------------------
		public static void DrawGrid(Color lineColor_Center, Color lineColor)
		{
			int pixelSize = 50;

			//Color lineColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
			//Color lineColor_Center = new Color(0.7f, 0.7f, 0.3f, 1.0f);

			if (_zoom < 0.2f + 0.05f)
			{
				pixelSize = 200;
				lineColor.a = 0.4f;
			}
			else if (_zoom < 0.5f + 0.05f)
			{
				pixelSize = 100;
				lineColor.a = 0.7f;
			}

			//Vector2 centerPos = World2GL(Vector2.zero);

			//Screen의 Width, Height에 해당하는 극점을 찾자
			//Vector2 pos_LT = GL2World(new Vector2(0, 0));
			//Vector2 pos_RB = GL2World(new Vector2(_windowWidth, _windowHeight));
			Vector2 pos_LT = GL2World(new Vector2(-500, -500));
			Vector2 pos_RB = GL2World(new Vector2(_windowWidth + 500, _windowHeight + 500));

			float yWorld_Max = Mathf.Max(pos_LT.y, pos_RB.y) + 100;
			float yWorld_Min = Mathf.Min(pos_LT.y, pos_RB.y) - 200;
			float xWorld_Max = Mathf.Max(pos_LT.x, pos_RB.x);
			float xWorld_Min = Mathf.Min(pos_LT.x, pos_RB.x);

			// 가로줄 먼저 (+- Y로 움직임)
			Vector2 curPos = Vector2.zero;
			//Vector2 curPosGL = Vector2.zero;
			Vector2 posA, posB;

			curPos.y = (int)(yWorld_Min / pixelSize) * pixelSize;

			// + Y 방향 (아래)
			while (true)
			{
				//curPosGL = World2GL(curPos);

				//if(curPosGL.y < 0 || curPosGL.y > _windowHeight)
				//{
				//	break;
				//}
				if (curPos.y > yWorld_Max)
				{
					break;
				}


				posA.x = pos_LT.x;
				posA.y = curPos.y;

				posB.x = pos_RB.x;
				posB.y = curPos.y;

				DrawLine(posA, posB, lineColor);

				curPos.y += pixelSize;
			}


			curPos = Vector2.zero;
			curPos.x = (int)(xWorld_Min / pixelSize) * pixelSize;

			// + X 방향 (오른쪽)
			while (true)
			{
				//curPosGL = World2GL(curPos);

				//if(curPosGL.x < 0 || curPosGL.x > _windowWidth)
				//{
				//	break;
				//}
				if (curPos.x > xWorld_Max)
				{
					break;
				}

				posA.y = pos_LT.y;
				posA.x = curPos.x;

				posB.y = pos_RB.y;
				posB.x = curPos.x;

				DrawLine(posA, posB, lineColor);

				curPos.x += pixelSize;
			}

			//중앙선

			curPos = Vector2.zero;

			posA.x = pos_LT.x;
			posA.y = curPos.y;

			posB.x = pos_RB.x;
			posB.y = curPos.y;

			DrawLine(posA, posB, lineColor_Center);


			posA.y = pos_LT.y;
			posA.x = curPos.x;

			posB.y = pos_RB.y;
			posB.x = curPos.x;

			DrawLine(posA, posB, lineColor_Center);

		}


		// Editing Border
		public static void DrawEditingBorderline()
		{
			//Vector2 pos = new Vector2(_windowPosX + (_windowWidth / 2), _windowPosY + (_windowHeight / 2));
			Vector2 pos = new Vector2((_windowWidth / 2), (_windowHeight));

			Color borderColor = new Color(0.7f, 0.0f, 0.0f, 0.8f);
			DrawBox(GL2World(pos), (float)(_windowWidth + 100) / _zoom, 50.0f / _zoom, borderColor, false);

			pos.y = -12;

			DrawBox(GL2World(pos), (float)(_windowWidth + 100) / _zoom, 50.0f / _zoom, borderColor, false);
		}

		//-----------------------------------------------------------------------------------------
		/// <summary>
		/// 마우스 커서를 나오게 하자
		/// </summary>
		/// <param name="mousePos"></param>
		/// <param name="pos"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="cursorType"></param>
		private static void AddCursorRect(Vector2 mousePos, Vector2 pos, float width, float height, MouseCursor cursorType)
		{
			if (pos.x < 0 || pos.x > _windowWidth || pos.y < 0 || pos.y > _windowHeight)
			{
				return;
			}

			if (mousePos.x < pos.x - width * 2 ||
				mousePos.x > pos.x + width * 2 ||
				mousePos.y < pos.y - height * 2 ||
				mousePos.y > pos.y + height * 2)
			{
				//영역을 벗어났다.
				return;
			}

			pos.x -= width / 2;
			pos.y -= height / 2;

			EditorGUIUtility.AddCursorRect(new Rect(pos.x, pos.y, width, height), cursorType);
		}



		//-----------------------------------------------------------------------------------------
		private static Color _weightColor_Gray = new Color(0.2f, 0.2f, 0.2f, 1.0f);
		private static Color _weightColor_Blue = new Color(0.0f, 0.2f, 1.0f, 1.0f);
		private static Color _weightColor_Yellow = new Color(1.0f, 1.0f, 0.0f, 1.0f);
		private static Color _weightColor_Red = new Color(1.0f, 0.0f, 0.0f, 1.0f);
		public static Color GetWeightColor(float weight)
		{
			if (weight < 0.0f)
			{
				return _weightColor_Gray;
			}
			else if (weight < 0.5f)
			{
				return _weightColor_Blue * (1.0f - weight * 2.0f) + _weightColor_Yellow * (weight * 2.0f);
			}
			else if (weight < 1.0f)
			{
				return _weightColor_Yellow * (1.0f - (weight - 0.5f) * 2.0f) + _weightColor_Red * ((weight - 0.5f) * 2.0f);
			}
			else
			{
				return _weightColor_Red;
			}
		}

		public static Color GetWeightColor2(float weight, apEditor editor)
		{
			if (weight < 0.0f)
			{
				return editor._colorOption_VertColor_NotSelected;
			}
			else if (weight < 0.25f)
			{
				return (_vertColor_Weighted_0 * (0.25f - weight) + _vertColor_Weighted_25 * (weight)) / 0.25f;
			}
			else if (weight < 0.5f)
			{
				return (_vertColor_Weighted_25 * (0.25f - (weight - 0.25f)) + _vertColor_Weighted_50 * (weight - 0.25f)) / 0.25f;
			}
			else if (weight < 0.75f)
			{
				return (_vertColor_Weighted_50 * (0.25f - (weight - 0.5f)) + _vertColor_Weighted_75 * (weight - 0.5f)) / 0.25f;
			}
			else if (weight < 1.0f)
			{
				return (_vertColor_Weighted_75 * (0.25f - (weight - 0.75f)) + editor._colorOption_VertColor_Selected * (weight - 0.75f)) / 0.25f;
			}
			else
			{
				//return _weightColor_Red;
				return editor._colorOption_VertColor_Selected;
			}
		}

		public static Color GetWeightColor3(float weight)
		{
			if (weight < 0.0f)
			{
				return _vertColor_Weighted3_0;
			}
			else if (weight < 0.25f)
			{
				return (_vertColor_Weighted3_0 * (0.25f - weight) + _vertColor_Weighted3_25 * (weight)) / 0.25f;
			}
			else if (weight < 0.5f)
			{
				return (_vertColor_Weighted3_25 * (0.25f - (weight - 0.25f)) + _vertColor_Weighted3_50 * (weight - 0.25f)) / 0.25f;
			}
			else if (weight < 0.75f)
			{
				return (_vertColor_Weighted3_50 * (0.25f - (weight - 0.5f)) + _vertColor_Weighted3_75 * (weight - 0.5f)) / 0.25f;
			}
			else if (weight < 1.0f)
			{
				return (_vertColor_Weighted3_75 * (0.25f - (weight - 0.75f)) + _vertColor_Weighted3_100 * (weight - 0.75f)) / 0.25f;
			}
			else
			{
				//return _weightColor_Red;
				return _vertColor_Weighted3_100;
			}
		}


		public static Color GetWeightColor3_Vert(float weight)
		{
			if (weight < 0.0f)
			{
				return _vertColor_Weighted3Vert_0;
			}
			else if (weight < 0.25f)
			{
				return (_vertColor_Weighted3Vert_0 * (0.25f - weight) + _vertColor_Weighted3Vert_25 * (weight)) / 0.25f;
			}
			else if (weight < 0.5f)
			{
				return (_vertColor_Weighted3Vert_25 * (0.25f - (weight - 0.25f)) + _vertColor_Weighted3Vert_50 * (weight - 0.25f)) / 0.25f;
			}
			else if (weight < 0.75f)
			{
				return (_vertColor_Weighted3Vert_50 * (0.25f - (weight - 0.5f)) + _vertColor_Weighted3Vert_75 * (weight - 0.5f)) / 0.25f;
			}
			else if (weight < 1.0f)
			{
				return (_vertColor_Weighted3Vert_75 * (0.25f - (weight - 0.75f)) + _vertColor_Weighted3Vert_100 * (weight - 0.75f)) / 0.25f;
			}
			else
			{
				//return _weightColor_Red;
				return _vertColor_Weighted3Vert_100;
			}
		}



		public static Color GetWeightColor4(float weight)
		{
			if (weight <= 0.0001f)
			{
				return _vertColor_Weighted4_0_Null;
			}
			else if (weight < 0.33f)
			{
				return (_vertColor_Weighted4_0 * (0.33f - weight) + _vertColor_Weighted4_33 * (weight)) / 0.33f;
			}
			else if (weight < 0.66f)
			{
				return (_vertColor_Weighted4_33 * (0.33f - (weight - 0.33f)) + _vertColor_Weighted4_66 * (weight - 0.33f)) / 0.33f;
			}
			else if (weight < 1.0f)
			{
				return (_vertColor_Weighted4_66 * (0.34f - (weight - 0.66f)) + _vertColor_Weighted4_100 * (weight - 0.66f)) / 0.34f;
			}
			else
			{
				return _vertColor_Weighted4_100;
			}
		}


		public static Color GetWeightColor4_Vert(float weight)
		{
			if (weight <= 0.0001f)
			{
				return _vertColor_Weighted4Vert_Null;
			}
			else if (weight < 0.33f)
			{
				return (_vertColor_Weighted4Vert_0 * (0.33f - weight) + _vertColor_Weighted4Vert_33 * (weight)) / 0.33f;
			}
			else if (weight < 0.66f)
			{
				return (_vertColor_Weighted4Vert_33 * (0.33f - (weight - 0.33f)) + _vertColor_Weighted4Vert_66 * (weight - 0.33f)) / 0.33f;
			}
			else if (weight < 1.0f)
			{
				return (_vertColor_Weighted4Vert_66 * (0.34f - (weight - 0.66f)) + _vertColor_Weighted4Vert_100 * (weight - 0.66f)) / 0.34f;
			}
			else
			{
				return _vertColor_Weighted4Vert_100;
			}
		}

		public static Color GetWeightGrayscale(float weight)
		{
			//return _weightColor_Gray * (1.0f - weight) + Color.black * weight;
			return Color.black * (1.0f - weight) + Color.white * weight;
		}
	}

}