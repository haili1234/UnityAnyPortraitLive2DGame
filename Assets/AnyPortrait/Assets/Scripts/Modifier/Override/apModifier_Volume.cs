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
	[Serializable]
	public class apModifier_Volume : apModifierBase
	{
		// Members
		//----------------------------------------------


		// Init
		//----------------------------------------------
		//public apModifier_Volume() : base()
		//{
		//}

		//public override void Init()
		//{

		//}


		// Get / Set
		//----------------------------------------------
		public override MODIFIER_TYPE ModifierType
		{
			get { return MODIFIER_TYPE.Volume; }
		}

		public override apModifierParamSetGroup.SYNC_TARGET SyncTarget
		{
			get { return apModifierParamSetGroup.SYNC_TARGET.ControllerWithoutKey; }
		}

		public override string DisplayName
		{
			get { return "Volume (Controller)"; }
		}

		public override string DisplayNameShort
		{
			get { return "Volume (Ctrl)"; }
		}
		/// <summary>
		/// Calculate 계산시 어느 단계에서 적용되는가
		/// </summary>
		public override apCalculatedResultParam.CALCULATED_VALUE_TYPE CalculatedValueType
		{
			get { return apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos; }
		}

		public override apCalculatedResultParam.CALCULATED_SPACE CalculatedSpace
		{
			get { return apCalculatedResultParam.CALCULATED_SPACE.Object; }
		}

		public override apModifiedMesh.MOD_VALUE_TYPE ModifiedValueType
		{
			get
			{
				return apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Volume;
			}
		}

		// MeshTransform에만 적용한다.
		public override bool IsTarget_MeshTransform { get { return true; } }
		public override bool IsTarget_MeshGroupTransform { get { return false; } }
		public override bool IsTarget_Bone { get { return false; } }
		public override bool IsTarget_ChildMeshTransform { get { return false; } }
	}
}