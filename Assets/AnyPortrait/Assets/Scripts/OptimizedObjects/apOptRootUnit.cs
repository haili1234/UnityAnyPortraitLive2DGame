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
//using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// This class is the basic unit for performing updates including meshes.
	/// (You can refer to this in your script, but we do not recommend using it directly.)
	/// </summary>
	public class apOptRootUnit : MonoBehaviour
	{
		// Members
		//------------------------------------------------
		public apPortrait _portrait = null;

		public apOptTransform _rootOptTransform = null;
		
		[HideInInspector]
		public Transform _transform = null;

		//빠른 처리를 위해 OptBone을 리스트로 저장해두자. 오직 참조용
		[SerializeField, HideInInspector]
		private List<apOptBone> _optBones = new List<apOptBone>();

		[SerializeField, HideInInspector]
		private List<apOptTransform> _optTransforms = new List<apOptTransform>();

		//빠른 참조를 위한 Dictionary
		[NonSerialized]
		private Dictionary<string, apOptBone> _optBonesMap = new Dictionary<string, apOptBone>();

		[NonSerialized]
		private Dictionary<string, apOptTransform> _optTransformMap = new Dictionary<string, apOptTransform>();

		public List<apOptTransform> OptTransforms
		{
			get
			{
				return _optTransforms;
			}
		}

		// Init
		//------------------------------------------------
		void Awake()
		{
			_transform = transform;
		}

		void Start()
		{
			this.enabled = false;//<<업데이트를 하진 않습니다.
		}

		// Update
		//------------------------------------------------
		void Update()
		{

		}

		void LateUpdate()
		{

		}


		// Bake
		//-----------------------------------------------
		public void ClearChildLinks()
		{
			if(_optBones == null)
			{
				_optBones = new List<apOptBone>();
			}
			_optBones.Clear();

			if(_optTransforms == null)
			{
				_optTransforms = new List<apOptTransform>();
			}
			_optTransforms.Clear();
		}

		public void AddChildBone(apOptBone bone)
		{
			_optBones.Add(bone);
		}

		public void AddChildTransform(apOptTransform optTransform)
		{
			_optTransforms.Add(optTransform);
		}

		// Functions
		//------------------------------------------------
		public void RemoveAllCalculateResultParams()
		{
			if (_rootOptTransform == null)
			{
				return;
			}
			_rootOptTransform.ClearResultParams(true);
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void ResetCalculateStackForBake()
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			//_rootOptTransform.ClearResultParams();
			
			_rootOptTransform.ResetCalculateStackForBake();
			
		}


		//public void DebugBoneMatrix()
		//{
		//	if (_rootOptTransform == null)
		//	{
		//		return;
		//	}

		//	//_rootOptTransform.ClearResultParams();
			
		//	_rootOptTransform.DebugBoneMatrix();
			
		//}

		public void UpdateTransforms(float tDelta)
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			//---------------------------------------------------------
//#if UNITY_EDITOR
//			Profiler.BeginSample("Root Unit - Ready To Update Bones");
//#endif
			//추가
			//본 업데이트 1단계
			_rootOptTransform.ReadyToUpdateBones();

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif
			//---------------------------------------------------------

//#if UNITY_EDITOR
//			Profiler.BeginSample("Root Unit - Update Modifier");
//#endif
			//1. Modifer부터 업데이트 (Pre)
			_rootOptTransform.UpdateModifier_Pre(tDelta);

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif

			//---------------------------------------------------------

//#if UNITY_EDITOR
//			Profiler.BeginSample("Root Unit - Calculate Pre");
//#endif
			//2. 실제로 업데이트
			_rootOptTransform.ReadyToUpdate();
			_rootOptTransform.UpdateCalculate_Pre();//Post 작성할 것

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif

			//---------------------------------------------------------

//#if UNITY_EDITOR
//			Profiler.BeginSample("Root Unit - Update Bones World Matrix");
//#endif

			//Bone World Matrix Update
			_rootOptTransform.UpdateBonesWorldMatrix();

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif

			//------------------------------------------------------------

//#if UNITY_EDITOR
//			Profiler.BeginSample("Root Unit - Calculate Post (Modifier)");
//#endif

			//Modifier 업데이트 (Post)
			_rootOptTransform.UpdateModifier_Post(tDelta);

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif

//#if UNITY_EDITOR
//			Profiler.BeginSample("Root Unit - Calculate Post (Update)");
//#endif
			_rootOptTransform.UpdateCalculate_Post();//Post Calculate

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif

		}




		/// <summary>
		/// UpdateTransform의 Bake버전
		/// Bone 관련 부분 처리가 조금 다르다.
		/// </summary>
		/// <param name="tDelta"></param>
		public void UpdateTransformsForBake(float tDelta)
		{
			if (_rootOptTransform == null)
			{
				return;
			}
			
			//추가
			//본 업데이트 1단계
			_rootOptTransform.ReadyToUpdateBones();

			//1. Modifer부터 업데이트 (Pre)
			_rootOptTransform.UpdateModifier_Pre(tDelta);
			//---------------------------------------------------------

			_rootOptTransform.ReadyToUpdateBones();

			//2. 실제로 업데이트
			_rootOptTransform.ReadyToUpdate();
			_rootOptTransform.UpdateCalculate_Pre();//Post 작성할 것


			//---------------------------------------------------------


			//Bone World Matrix Update
			_rootOptTransform.UpdateBonesWorldMatrix();
			//_rootOptTransform.UpdateBonesWorldMatrixForBake();//<<이게 다르다


			//------------------------------------------------------------


			//Modifier 업데이트 (Post)
			_rootOptTransform.UpdateModifier_Post(tDelta);

			_rootOptTransform.UpdateCalculate_Post();//Post Calculate


		}


		
		public void UpdateTransformsOnlyMaskMesh()
		{
			if (_rootOptTransform == null)
			{
				return;
			}
			_rootOptTransform.UpdateMaskMeshes();
		}



		public void Show()
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			_rootOptTransform.Show(true);
		}

		public void ShowWhenBake()
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			_rootOptTransform.ShowWhenBake(true);
		}



		public void Hide()
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			_rootOptTransform.Hide(true);
		}

		public void ResetCommandBuffer(bool isRegistToCamera)
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			_rootOptTransform.ResetCommandBuffer(isRegistToCamera);
		}


		// Get / Set
		//------------------------------------------------
		public apOptBone GetBone(string name)
		{
			//일단 빠른 검색부터
			if(_optBonesMap.ContainsKey(name))
			{
				return _optBonesMap[name];
			}

			apOptBone resultBone = _optBones.Find(delegate (apOptBone a)
			{
				return string.Equals(a._name, name);
			});

			if(resultBone == null)
			{
				return null;
			}

			//빠른 검색 리스트에 넣고
			_optBonesMap.Add(name, resultBone);

			return resultBone;
		}

		public apOptTransform GetTransform(string name)
		{
			//일단 빠른 검색부터
			if(_optTransformMap.ContainsKey(name))
			{
				return _optTransformMap[name];
			}

			apOptTransform resultTransform = _optTransforms.Find(delegate (apOptTransform a)
			{
				return string.Equals(a._name, name);
			});

			if(resultTransform == null)
			{
				return null;
			}

			//빠른 검색 리스트에 넣고
			_optTransformMap.Add(name, resultTransform);

			return resultTransform;
		}
		
	}

}