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

	public class apPSDGL
	{
		// Members
		//------------------------------------------------------------------------
		//private int _windowPosX = 0;
		//private int _windowPosY = 0;
		private int _windowWidth = 0;
		private int _windowHeight = 0;

		private Vector2 _windowScroll = Vector2.zero;

		private float _zoom = 1.0f;
		public float Zoom { get { return _zoom; } }

		//private GUIStyle _textStyle = GUIStyle.none;

		private Vector4 _glScreenClippingSize = Vector4.zero;
		private apGL.MaterialBatch _matBatch = new apGL.MaterialBatch();

		// Init
		//------------------------------------------------------------------------
		public apPSDGL()
		{

		}

		//public void SetMaterial(Material mat_Color, Material mat_Texture, Material mat_MaskedTexture)
		public void SetShader(Shader shader_Color,
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

		public void SetWindowSize(int windowWidth, int windowHeight, Vector2 scroll, float zoom,
			int posX, int posY, int totalEditorWidth, int totalEditorHeight)
		{
			_windowWidth = windowWidth;
			_windowHeight = windowHeight;
			_windowScroll.x = scroll.x * _windowWidth * 0.1f;
			_windowScroll.y = scroll.y * windowHeight * 0.1f;
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
		}

		public Vector2 World2GL(Vector2 pos)
		{
			//(posX * Zoom) + (_windowWidth * 0.5f) - (ScrollX) = glX
			//(glX + ScrollX - (_windowWidth * 0.5f)) / Zoom
			return new Vector2(
				(pos.x * _zoom) + (_windowWidth * 0.5f)
				- _windowScroll.x,

				(_windowHeight - (pos.y * _zoom)) - (_windowHeight * 0.5f)
				- _windowScroll.y
				);
		}

		public Vector2 GL2World(Vector2 glPos)
		{
			return new Vector2(
				(glPos.x + (_windowScroll.x) - (_windowWidth * 0.5f)) / _zoom,
				(-1 * (glPos.y + _windowScroll.y + (_windowHeight * 0.5f) - (_windowHeight))) / _zoom
				);
		}

		// 최적화형
		//-------------------------------------------------------------------------------
		public void BeginBatch_ColoredPolygon()
		{
			_matBatch.SetPass_Color();
			_matBatch.SetClippingSize(_glScreenClippingSize);

			GL.Begin(GL.TRIANGLES);
		}

		public void BeginBatch_ColoredLine()
		{
			_matBatch.SetPass_Color();
			_matBatch.SetClippingSize(_glScreenClippingSize);

			GL.Begin(GL.LINES);
		}

		public void EndBatch()
		{
			GL.End();
		}

		//-------------------------------------------------------------------------------
		// Draw Line
		//-------------------------------------------------------------------------------
		public void DrawLine(Vector2 pos1, Vector2 pos2, Color color)
		{
			DrawLine(pos1, pos2, color, true);
		}

		public void DrawLine(Vector2 pos1, Vector2 pos2, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			if (Vector2.Equals(pos1, pos2))
			{
				return;
			}

			pos1 = World2GL(pos1);
			pos2 = World2GL(pos2);

			//Vector2 pos1_Real = pos1;
			//Vector2 pos2_Real = pos2;

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

		public void DrawBoldLine(Vector2 pos1, Vector2 pos2, float width, Color color, bool isNeedResetMat)
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

			float halfWidth = width * 0.5f;

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


		//-------------------------------------------------------------------------------
		// Draw Texture
		//-------------------------------------------------------------------------------
		private Color _lineColor_Outline = new Color(0.0f, 0.5f, 1.0f, 0.7f);
		public void DrawTexture(Texture2D image, Vector2 pos, float width, float height, Color color2X, bool isOutlineRender)
		{
			DrawTexture(image, pos, width, height, color2X, 0.0f, isOutlineRender);
		}

		public void DrawTexture(Texture2D image, Vector2 pos, float width, float height, Color color2X, float depth, bool isOutlineRender)
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
			Vector2 pos_0_Org = new Vector2(pos.x - realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_1_Org = new Vector2(pos.x + realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_2_Org = new Vector2(pos.x + realWidth_Half, pos.y + realHeight_Half);
			Vector2 pos_3_Org = new Vector2(pos.x - realWidth_Half, pos.y + realHeight_Half);

			Vector2 pos_0 = pos_0_Org;
			Vector2 pos_1 = pos_1_Org;
			Vector2 pos_2 = pos_2_Org;
			Vector2 pos_3 = pos_3_Org;


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
			//_mat_Texture.SetPass(0);
			//_mat_Texture.SetTexture("_MainTex", image);
			//_mat_Texture.SetColor("_Color", color2X);
			//_mat_Texture.SetVector("_ScreenSize", _glScreenClippingSize);
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


			if (isOutlineRender)
			{
				_matBatch.SetPass_Color();
				_matBatch.SetClippingSize(_glScreenClippingSize);
				GL.Begin(GL.TRIANGLES);

				DrawBoldLine(GL2World(pos_0), GL2World(pos_1), 6.0f, _lineColor_Outline, false);
				DrawBoldLine(GL2World(pos_1), GL2World(pos_2), 6.0f, _lineColor_Outline, false);
				DrawBoldLine(GL2World(pos_2), GL2World(pos_3), 6.0f, _lineColor_Outline, false);
				DrawBoldLine(GL2World(pos_3), GL2World(pos_0), 6.0f, _lineColor_Outline, false);

				GL.End();
			}
			//GL.Flush();
		}

		//------------------------------------------------------------------------------------------------
		// Draw Grie
		//------------------------------------------------------------------------------------------------
		public void DrawGrid()
		{
			int pixelSize = 50;

			Color lineColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
			Color lineColor_Center = new Color(0.7f, 0.7f, 0.3f, 0.5f);

			if (_zoom < 0.2f + 0.05f)
			{
				pixelSize = 100;
				lineColor.a = 0.4f;
			}
			else if (_zoom < 0.5f + 0.05f)
			{
				pixelSize = 50;
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
	}

}