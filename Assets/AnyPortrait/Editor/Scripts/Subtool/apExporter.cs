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
using System.IO;

using AnyPortrait;
using NGUIforUnity;

namespace AnyPortrait
{

	/// <summary>
	/// Editor에 포함되어서 Export를 담당한다.
	/// Texture Render / GIF Export / 백업용 Txt
	
	/// </summary>
	public class apExporter
	{
		// Members
		//----------------------------------------------
		private apEditor _editor = null;
		private RenderTexture _renderTexture = null;
		private RenderTexture _renderTexture_GrayscaleAlpha = null;


		//GIF용 변수
		//private int _gifWidth = 0;
		//private int _gifHeight = 0;
		//private byte[] _gifPixels = null;
		//private int _gifRepeatCount = 0;//-1 : No Repeat, 0 : Forever
		//private int _gifDelay = 0;//Frame Delay (ms / 10)
		//private byte[] _gifIndexedPixels = null;//Converted Frame Indexed To Palatte
		//private int _gifColorDepth = 0;//Number of bit Planes
		//private byte[] _gifColorTab = null;//RGB Palette
		//private bool[] _gifUsedEntry = new bool[256];//Active Palette Entries
		//private int _gifPalSize = 7; // color table size (bits-1)

		//Step 전용 변수
		private apNGIFforUnity _ngif = new apNGIFforUnity();
		private string _gif_FilePath = "";
		//private apAnimClip _gif_AnimClip = null;
		//private apMeshGroup _gif_MeshGroup = null;
		//private int _gif_LoopCount = 0;
		//private int _gif_WinPosX = -1;
		//private int _gif_WinPosY = -1;
		//private int _gif_SrcSizeWidth = -1;
		//private int _gif_SrcSizeHeight = -1;
		//private int _gif_DstSizeWidth = -1;
		//private int _gif_DstSizeHeight = -1;
		//private Color _gif_ClearColor = Color.black;
		//private int _gif_Quality = -1;

		//private FileStream _gif_FileStream = null;
		//private int _gif_totalProcessCount = -1;

		public string GIF_FilePath { get { return _gif_FilePath; } }

		private FileStream _gifFileStream = null;

		// Init
		//----------------------------------------------
		public apExporter(apEditor editor)
		{
			_editor = editor;
		}

		// Functions
		//----------------------------------------------
		public Texture2D RenderToTexture(apMeshGroup meshGroup,
										int winPosX, int winPosY,
										int srcSizeWidth, int srcSizeHeight,
										int dstSizeWidth, int dstSizeHeight,
										Color clearColor)
		{
			if (_editor == null)
			{
				return null;
			}

			//apGL의 Window Size를 바꾸어준다.
			int rtSizeWidth = ((int)_editor.position.width);
			int rtSizeHeight = ((int)_editor.position.height);
			
			//winPosY -= 10;
			int guiOffsetX = apGL._posX_NotCalculated;
			int guiOffsetY = apGL._posY_NotCalculated;

			int clipPosX = winPosX - (srcSizeWidth / 2);
			int clipPosY = winPosY - (srcSizeHeight / 2);

			clipPosX += guiOffsetX;
			clipPosY += guiOffsetY + 15;

			int clipPosX_Right = clipPosX + srcSizeWidth;
			int clipPosY_Bottom = clipPosY + srcSizeHeight;

			if (clipPosX < 0)		{ clipPosX = 0; }
			if (clipPosY < 0)		{ clipPosY = 0; }
			if (clipPosX_Right > rtSizeWidth)	{ clipPosX_Right = rtSizeWidth; }
			if (clipPosY_Bottom > rtSizeHeight)	{ clipPosY_Bottom = rtSizeHeight; }

			int clipWidth = (clipPosX_Right - clipPosX);
			int clipHeight = (clipPosY_Bottom - clipPosY);
			if (clipWidth <= 0 || clipHeight <= 0)
			{
				Debug.LogError("RenderToTexture Failed : Clip Area is over Screen");
				return null;
			}

			meshGroup.RefreshForce();
			meshGroup.UpdateRenderUnits(0.0f, true);



			//Pass-1. 일반 + MaskParent를 Alpha2White 렌더링. 이걸로 나중에 알파 채널용 텍스쳐를 만든다.
			//--------------------------------------------------------------------------------------------------------
			_renderTexture_GrayscaleAlpha = RenderTexture.GetTemporary(rtSizeWidth, rtSizeHeight, 8, RenderTextureFormat.ARGB32);
			_renderTexture_GrayscaleAlpha.antiAliasing = 1;
			_renderTexture_GrayscaleAlpha.wrapMode = TextureWrapMode.Clamp;
			
			RenderTexture.active = null;
			RenderTexture.active = _renderTexture_GrayscaleAlpha;

			//기본 
			Color maskClearColor = new Color(clearColor.a, clearColor.a, clearColor.a, 1.0f);
			GL.Clear(false, true, maskClearColor, 0.0f);//변경 : Mac에서도 작동 하려면..
			apGL.DrawBoxGL(Vector2.zero, 50000, 50000, maskClearColor, false, true);//<<이걸로 배경을 깔자
			GL.Flush();

			//System.Threading.Thread.Sleep(50);

			for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			{
				apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						if (renderUnit._meshTransform._isClipping_Parent)
						{
							if (renderUnit._isVisible)
							{
								//RenderTexture.active = _renderTexture_GrayscaleAlpha;
								apGL.DrawRenderUnit_Basic_Alpha2White(renderUnit);
							}
						}
						else if (renderUnit._meshTransform._isClipping_Child)
						{
							//Pass
							//Alpha 렌더링에서 Clipping Child는 제외한다. 어차피 Parent의 Alpha보다 많을 수 없으니..
						}
						else
						{
							if (renderUnit._isVisible)
							{
								//RenderTexture.active = _renderTexture_GrayscaleAlpha;
								apGL.DrawRenderUnit_Basic_Alpha2White(renderUnit);
							}
						}
					}
				}
			}

			System.Threading.Thread.Sleep(5);

			Texture2D resultTex_SrcSize_Alpha = new Texture2D(srcSizeWidth, srcSizeHeight, TextureFormat.ARGB32, false);
			resultTex_SrcSize_Alpha.ReadPixels(new Rect(clipPosX, clipPosY, clipWidth, clipHeight), 0, 0);
			resultTex_SrcSize_Alpha.Apply();


			//Pass-2. 기본 렌더링
			//--------------------------------------------------------------------------------------------------------
			//1. Clip Parent의 MaskTexture를 미리 구워서 Dictionary에 넣는다.
			Dictionary<apRenderUnit, Texture2D> bakedClipMaskTextures = new Dictionary<apRenderUnit, Texture2D>();

			//Debug.Log("-------------------------------------------------------------");
			//Debug.Log("RenderTextureSize : " + rtSizeWidth + " x " + rtSizeHeight);
			//Debug.Log("Capture Size : " + imageSizeWidth + " x " + imageSizeHeight);
			//Debug.Log("Capture Pos : " + winPosX + ", " + winPosY);
			//Debug.LogError("GL Size : " + apGL._totalEditorWidth + " x " + apGL._totalEditorHeight);

			for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			{
				apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						if (renderUnit._meshTransform._isClipping_Parent)
						{
							if (renderUnit._isVisible)
							{
								Texture2D clipMaskTex = apGL.GetMaskTexture_ClippingParent(renderUnit);
								if (clipMaskTex != null)
								{
									bakedClipMaskTextures.Add(renderUnit, clipMaskTex);
								}
								else
								{
									Debug.LogError("Clip Testure Bake Failed");
								}

							}
						}
					}
				}
			}

			System.Threading.Thread.Sleep(5);


			_renderTexture = RenderTexture.GetTemporary(rtSizeWidth, rtSizeHeight, 8, RenderTextureFormat.ARGB32);
			_renderTexture.antiAliasing = 1;
			_renderTexture.wrapMode = TextureWrapMode.Clamp;
			
			RenderTexture.active = null;
			RenderTexture.active = _renderTexture;

			Color opaqueClearColor = new Color(clearColor.r * clearColor.a, clearColor.g * clearColor.a, clearColor.b * clearColor.a, 1.0f);

			//GL.Clear(true, true, clearColor, -100.0f);//이전
			GL.Clear(false, true, opaqueClearColor, 0.0f);//변경 : Mac에서도 작동 하려면..
			apGL.DrawBoxGL(Vector2.zero, 50000, 50000, opaqueClearColor, false, true);//<<이걸로 배경을 깔자
			GL.Flush();

			//System.Threading.Thread.Sleep(50);

			for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			{
				apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						if (renderUnit._meshTransform._isClipping_Parent)
						{
							if (renderUnit._isVisible)
							{
								if (bakedClipMaskTextures.ContainsKey(renderUnit))
								{
									apGL.DrawRenderUnit_ClippingParent_Renew_WithoutRTT(renderUnit,
												renderUnit._meshTransform._clipChildMeshes,
												bakedClipMaskTextures[renderUnit]);
								}


								////RenderTexture.active = _renderTexture;//<<클리핑 뒤에는 다시 연결해줘야한다.
							}
						}
						else if (renderUnit._meshTransform._isClipping_Child)
						{
							//Pass
						}
						else
						{
							if (renderUnit._isVisible)
							{
								RenderTexture.active = _renderTexture;
								apGL.DrawRenderUnit_Basic(renderUnit);
							}
						}
					}
				}
			}



			

			System.Threading.Thread.Sleep(5);


			

			Texture2D resultTex_SrcSize = new Texture2D(srcSizeWidth, srcSizeHeight, TextureFormat.ARGB32, false);
			resultTex_SrcSize.ReadPixels(new Rect(clipPosX, clipPosY, clipWidth, clipHeight), 0, 0);
			
			//Texture2D resultTex_SrcSize = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false);
			//resultTex_SrcSize.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);

			resultTex_SrcSize.Apply();

			

			


			//System.Threading.Thread.Sleep(50);

			RenderTexture.active = null;

			RenderTexture.ReleaseTemporary(_renderTexture_GrayscaleAlpha);
			RenderTexture.ReleaseTemporary(_renderTexture);//<<
			//UnityEngine.Object.DestroyImmediate(_renderTexture);

			_renderTexture = null;
			_renderTexture_GrayscaleAlpha = null;

			
			Texture2D resultTex_DstSize = new Texture2D(dstSizeWidth, dstSizeHeight, TextureFormat.ARGB32, false);
			Color color_RGB = Color.black;
			Color color_A = Color.black;

			for (int iY = 0; iY < dstSizeHeight; iY++)
			{
				for (int iX = 0; iX < dstSizeWidth; iX++)
				{
					float u = (float)iX / (float)dstSizeWidth;
					float v = (float)iY / (float)dstSizeHeight;

					color_RGB = resultTex_SrcSize.GetPixelBilinear(u, v);
					color_A = resultTex_SrcSize_Alpha.GetPixelBilinear(u, v);
					resultTex_DstSize.SetPixel(iX, iY, new Color(color_RGB.r, color_RGB.g, color_RGB.b, color_A.r));
					//resultTex_DstSize.SetPixel(iX, iY, new Color(color_A.r, color_A.g, color_A.b, 1));
				}
			}

			System.Threading.Thread.Sleep(5);
			//기존 크기의 이미지는 삭제
			UnityEngine.Object.DestroyImmediate(resultTex_SrcSize);
			UnityEngine.Object.DestroyImmediate(resultTex_SrcSize_Alpha);

			resultTex_DstSize.Apply();

			return resultTex_DstSize;

		}


		public bool SaveTexture2DToPNG(Texture2D srcTexture2D, string filePathWithExtension, bool isAutoDestroy)
		{
			try
			{
				if (srcTexture2D == null)
				{
					return false;
				}

				File.WriteAllBytes(filePathWithExtension + ".png", srcTexture2D.EncodeToPNG());

				if (isAutoDestroy)
				{
					UnityEngine.Object.DestroyImmediate(srcTexture2D);
				}
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError("SaveTexture2DToPNG Exception : " + ex);

				if (isAutoDestroy)
				{
					UnityEngine.Object.Destroy(srcTexture2D);
				}
				return false;
			}
		}


		public bool MakeGIFHeader(	string filePath,
									apAnimClip animClip,
									int dstSizeWidth, int dstSizeHeight)
		{
			//일단 파일 스트림을 꺼준다.
			if(_gifFileStream != null)
			{
				try
				{
					_gifFileStream.Close();
				}
				catch(Exception) { }
				_gifFileStream = null;
			}

			float secPerFrame = 1.0f / (float)animClip.FPS;

			//파일 스트림을 만들고 GIF 헤더 작성
			try
			{
				_gifFileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

				_ngif.WriteHeader(_gifFileStream);
				_ngif.SetGIFSetting((int)((secPerFrame * 100.0f) + 0.5f), 0, dstSizeWidth, dstSizeHeight);
			}
			catch(Exception)
			{
				if(_gifFileStream != null)
				{
					_gifFileStream.Close();
				}
				_gifFileStream = null;
				return false;
			}

			return true;
		}


		public bool AddGIFFrame(Texture2D frameImage, bool isFirstFrame, int quality)
		{
			if(_gifFileStream == null)
			{
				//이미 처리가 끝났네요.
				return false;
			}

			try
			{
				_ngif.AddFrame(frameImage, _gifFileStream, isFirstFrame, quality);
			}
			catch(Exception)
			{

			}
			UnityEngine.Object.DestroyImmediate(frameImage);
			return true;
		}

		public void EndGIF()
		{
			if(_gifFileStream == null)
			{
				//이미 처리가 끝났네요.
				return;
			}

			try
			{
				_ngif.Finish(_gifFileStream);

				_gifFileStream.Close();
				_gifFileStream = null;
			}
			catch(Exception)
			{
				if (_gifFileStream != null)
				{
					_gifFileStream.Close();
				}
				_gifFileStream = null;
			}
		}


		/// <summary>
		/// GIF Animation을 만든다.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="meshGroup"></param>
		/// <param name="animClip"></param>
		/// <param name="loopCount"></param>
		/// <param name="winPosX"></param>
		/// <param name="winPosY"></param>
		/// <param name="srcSizeWidth"></param>
		/// <param name="srcSizeHeight"></param>
		/// <param name="dstSizeWidth"></param>
		/// <param name="dstSizeHeight"></param>
		/// <param name="clearColor"></param>
		/// <param name="quality">1 ~ 256</param>
		/// <returns></returns>
		public bool MakeGIFAnimation(string filePath,
										apMeshGroup meshGroup,
										apAnimClip animClip,
										int loopCount,
										int winPosX, int winPosY,
										int srcSizeWidth, int srcSizeHeight,
										int dstSizeWidth, int dstSizeHeight,
										Color clearColor,
										int quality)
		{
			if (_editor == null || _editor._portrait == null || meshGroup == null || animClip == null)
			{
				return false;
			}

			int startFrame = animClip.StartFrame;
			int endFrame = animClip.EndFrame;
			if (endFrame < startFrame)
			{
				endFrame = startFrame;
			}
			if (loopCount < 1)
			{
				loopCount = 1;
			}

			//모든 AnimClip 정지
			for (int i = 0; i < _editor._portrait._animClips.Count; i++)
			{
				_editor._portrait._animClips[i].Stop_Editor();
			}
			_editor._portrait._animPlayManager.Stop_Editor();
			_editor._portrait._animPlayManager.SetAnimClip_Editor(animClip);
			meshGroup.RefreshForce();

			int curFrame = startFrame;
			bool isLoop = animClip.IsLoop;
			//Loop라면 마지막 프레임을 생략한다.
			int lastFrame = endFrame;
			if (isLoop)
			{
				lastFrame = endFrame - 1;
			}
			if (lastFrame < startFrame)
			{
				lastFrame = startFrame;
			}

			float secPerFrame = 1.0f / (float)animClip.FPS;


			FileStream fs = null;
			try
			{
				fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);

				_ngif.WriteHeader(fs);
				_ngif.SetGIFSetting((int)((secPerFrame * 100.0f) + 0.5f), 0, dstSizeWidth, dstSizeHeight);

				//WriteString("GIF89a", fs); // header
				//_gifDelay = (int)((secPerFrame * 100.0f) + 0.5f);//Delay
				//_gifRepeatCount = 0;//반복
				//_gifWidth = dstSizeWidth;
				//_gifHeight = dstSizeHeight;
				//_gifPixels = null;
				//_gifIndexedPixels = null;
				//_gifColorDepth = 0;
				//_gifColorTab = null;
				//_gifPalSize = 7;

				//for (int i = 0; i < _gifUsedEntry.Length; i++)
				//{
				//	_gifUsedEntry[i] = false;
				//}

				bool isFirstFrame = true;



				//애니메이션을 돌면서 Bake를 한다.
				for (int iLoop = 0; iLoop < loopCount; iLoop++)
				{
					curFrame = startFrame;


					while (true)
					{
						animClip.SetFrame_Editor(curFrame);//메시가 자동으로 업데이트를 한다.
						meshGroup.UpdateRenderUnits(secPerFrame, true);

						Texture2D bakeImage = RenderToTexture(meshGroup, winPosX, winPosY, srcSizeWidth, srcSizeHeight, dstSizeWidth, dstSizeHeight, clearColor);

						_ngif.AddFrame(bakeImage, fs, isFirstFrame, quality);
						isFirstFrame = false;

						UnityEngine.Object.DestroyImmediate(bakeImage);

						curFrame++;
						if (curFrame > lastFrame)
						{
							break;
						}
					}
				}

				_ngif.Finish(fs);

				fs.Close();
				fs = null;
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError("GIF Exception : " + ex);
			}
			if (fs != null)
			{
				fs.Close();
				fs = null;
			}


			return false;


		}



		


		//GIF 함수
		//----------------------------------------------


		// Get / Set
		//----------------------------------------------
	}

}