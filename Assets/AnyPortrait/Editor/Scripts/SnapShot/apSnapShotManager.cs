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

	//에디터에서 작업 객체의 값 복사나 저장을 위한 기능을 제공하는 매니저
	//Stack 방식으로 저장을 한다.
	//각 SnapShot 데이터는 실제로 적용되는 객체에서 관리한다.
	public class apSnapShotManager
	{
		// Singletone
		//-------------------------------------------
		private static apSnapShotManager _instance = new apSnapShotManager();
		private static readonly object _obj = new object();
		public static apSnapShotManager I { get { lock (_obj) { return _instance; } } }



		// Members
		//-------------------------------------------
		public enum SNAPSHOT_TARGET
		{
			Mesh, MeshGroup, ModifiedMesh, Portrait,//ETC.. Keyframe?
		}

		public enum SAVE_TYPE
		{
			Copy,
			Record
		}


		//Copy 타입 (Clipboard)
		private apSnapShotStackUnit _clipboard_ModMesh = null;//<<이건 따로 저장해주자
		private apSnapShotStackUnit _clipboard_Keyframe = null;//<<이건 따로 저장해주자
		private apSnapShotStackUnit _clipboard_VertRig = null;
		private apSnapShotStackUnit _clipboard_ModBone = null;//<<이건 따로 저장해주자

		//Record 타입
		private const int MAX_RECORD = 10;
		private List<apSnapShotStackUnit> _snapShotList = new List<apSnapShotStackUnit>();
		//이건 나중에 처리하자
		//private apSnapShotStackUnit _curSnapShot = null;
		//private int _iCurSnapShot = 0;
		//private bool _restoredSnapShot = false;


		// Init
		//-------------------------------------------
		private apSnapShotManager()
		{

		}



		public void Clear()
		{
			_clipboard_ModMesh = null;
			_clipboard_Keyframe = null;
			_clipboard_VertRig = null;
			_clipboard_ModBone = null;

			_snapShotList.Clear();
			//_curSnapShot = null;
			//_iCurSnapShot = -1;
			//_restoredSnapShot = false;
		}


		// Functions
		//-------------------------------------------

		// Copy / Paste
		//--------------------------------------------------------------------
		// 1. ModMesh
		//--------------------------------------------------------------------
		public void Copy_ModMesh(apModifiedMesh modMesh, string snapShotName)
		{
			_clipboard_ModMesh = new apSnapShotStackUnit(snapShotName);
			bool result = _clipboard_ModMesh.SetSnapShot_ModMesh(modMesh, "Clipboard");
			if (!result)
			{
				_clipboard_ModMesh = null;//<<저장 불가능하다.
			}
		}

		public bool Paste_ModMesh(apModifiedMesh targetModMesh)
		{
			if (targetModMesh == null)
			{ return false; }
			if (_clipboard_ModMesh == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_ModMesh.IsKeySyncable(targetModMesh);
			if (!isKeySync)
			{
				return false;
			}

			return _clipboard_ModMesh.Load(targetModMesh);
		}

		public string GetClipboardName_ModMesh()
		{
			if (_clipboard_ModMesh == null)
			{
				return "";
			}
			return _clipboard_ModMesh._unitName;
		}

		public bool IsPastable(apModifiedMesh targetModMesh)
		{
			if (targetModMesh == null)
			{ return false; }
			if (_clipboard_ModMesh == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_ModMesh.IsKeySyncable(targetModMesh);
			if (!isKeySync)
			{
				return false;
			}
			return true;
		}


		//--------------------------------------------------------------------
		// 1-2. ModBone
		//--------------------------------------------------------------------
		public void Copy_ModBone(apModifiedBone modBone, string snapShotName)
		{
			_clipboard_ModBone = new apSnapShotStackUnit(snapShotName);
			bool result = _clipboard_ModBone.SetSnapShot_ModBone(modBone, "Clipboard");
			if (!result)
			{
				_clipboard_ModBone = null;//<<저장 불가능하다.
			}
		}

		public bool Paste_ModBone(apModifiedBone targetModBone)
		{
			if (targetModBone == null)
			{ return false; }
			if (_clipboard_ModBone == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_ModBone.IsKeySyncable(targetModBone);
			if (!isKeySync)
			{
				return false;
			}

			return _clipboard_ModBone.Load(targetModBone);
		}

		public string GetClipboardName_ModBone()
		{
			if (_clipboard_ModBone == null)
			{
				return "";
			}
			return _clipboard_ModBone._unitName;
		}

		public bool IsPastable(apModifiedBone targetModBone)
		{
			if (targetModBone == null)
			{ return false; }
			if (_clipboard_ModBone == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_ModBone.IsKeySyncable(targetModBone);
			if (!isKeySync)
			{
				return false;
			}
			return true;
		}

		//--------------------------------------------------------------------
		// 2. Keyframe
		//--------------------------------------------------------------------
		public void Copy_Keyframe(apAnimKeyframe keyframe, string snapShotName)
		{
			_clipboard_Keyframe = new apSnapShotStackUnit(snapShotName);
			bool result = _clipboard_Keyframe.SetSnapShot_Keyframe(keyframe, "Clipboard");
			if (!result)
			{
				_clipboard_Keyframe = null;//<<저장 불가능하다.
			}
		}

		public bool Paste_Keyframe(apAnimKeyframe targetKeyframe)
		{
			if (targetKeyframe == null)
			{ return false; }
			if (_clipboard_Keyframe == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_Keyframe.IsKeySyncable(targetKeyframe);
			if (!isKeySync)
			{
				return false;
			}

			return _clipboard_Keyframe.Load(targetKeyframe);
		}

		public string GetClipboardName_Keyframe()
		{
			if (_clipboard_Keyframe == null)
			{
				return "";
			}
			return _clipboard_Keyframe._unitName;
		}

		public bool IsPastable(apAnimKeyframe keyframe)
		{
			if (keyframe == null)
			{ return false; }
			if (_clipboard_Keyframe == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_Keyframe.IsKeySyncable(keyframe);
			if (!isKeySync)
			{
				return false;
			}
			return true;
		}


		//--------------------------------------------------------------------
		// 3. Vertex Rigging
		//--------------------------------------------------------------------
		public void Copy_VertRig(apModifiedVertexRig modVertRig, string snapShotName)
		{
			_clipboard_VertRig = new apSnapShotStackUnit(snapShotName);
			bool result = _clipboard_VertRig.SetSnapShot_VertRig(modVertRig, "Clipboard");
			if (!result)
			{
				_clipboard_VertRig = null;//<<저장 불가능하다.
			}
		}

		public bool Paste_VertRig(apModifiedVertexRig targetModVertRig)
		{
			if (targetModVertRig == null)
			{ return false; }
			if (_clipboard_VertRig == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_VertRig.IsKeySyncable(targetModVertRig);
			if (!isKeySync)
			{
				return false;
			}

			return _clipboard_VertRig.Load(targetModVertRig);
		}

		public bool IsPastable(apModifiedVertexRig vertRig)
		{
			if (vertRig == null)
			{ return false; }
			if (_clipboard_VertRig == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_VertRig.IsKeySyncable(vertRig);
			if (!isKeySync)
			{
				return false;
			}
			return true;
		}

		// Save / Load
		//--------------------------------------------------------------------




		// Get / Set
		//--------------------------------------------
	}
}