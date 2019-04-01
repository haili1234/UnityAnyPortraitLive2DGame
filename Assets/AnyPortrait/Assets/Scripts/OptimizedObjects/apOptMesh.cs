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
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// Rendered Class that has a Mesh.
	/// </summary>
	public class apOptMesh : MonoBehaviour
	{
		// Members
		//------------------------------------------------
		/// <summary>[Please do not use it] Parent Portrait</summary>
		public apPortrait _portrait = null;

		/// <summary>[Please do not use it] Unique ID</summary>
		public int _uniqueID = -1;//meshID가 아니라 meshTransform의 ID를 사용한다.

		/// <summary>
		/// Paranet Opt Transform
		/// </summary>
		public apOptTransform _parentTransform;

		// Components
		//------------------------------------------------
		[HideInInspector]
		public MeshFilter _meshFilter = null;

		[HideInInspector]
		public MeshRenderer _meshRenderer = null;

		/// <summary>
		/// 현재 사용중인 Material이다. 
		/// Instanced인지 Shared인지는 자동으로 결정된다. (기본값은 Shared)
		/// 저장은 안되는 값이다.
		/// </summary>
		[NonSerialized, HideInInspector]
		private Material _material = null;
		
		/// <summary>[Please do not use it] Baked Shared Material to Batch Rendering</summary>
		[HideInInspector]
		public Material _sharedMaterial = null;

		[NonSerialized]
		private Material _instanceMaterial = null;

		private bool _isUseSharedMaterial = true;

		//기본 설정이 Batch Material을 사용하는 것이라면 별도의 값을 저장한다.
		//이때는 SharedMaterial이 null로 Bake된다
		/// <summary>[Please do not use it] Is Batch Target</summary>
		public bool _isBatchedMaterial = false;

		/// <summary>[Please do not use it] Batch Material ID</summary>
		public int _batchedMatID = -1;

		public bool _isDefaultColorGray = false;

		/// <summary>[Please do not use it] Current Rendered Texture (Read Only)</summary>
		[HideInInspector]
		public Texture2D _texture = null;

		/// <summary>[Please do not use it] Unique ID of Linked Texture Data</summary>
		[HideInInspector]
		public int _textureID = -1;


		/// <summary>[Please do not use it]</summary>
		[NonSerialized, HideInInspector]
		public Mesh _mesh = null;//<변경 : 저장 안됩니더

		// Vertex 값들
		//apRenderVertex에 해당하는 apOptRenderVertex의 배열 (리스트 아닙니더)로 저장한다.

		//<기본값>
		[SerializeField]
		private apOptRenderVertex[] _renderVerts = null;



		//RenderVert의 
		[SerializeField]
		private Vector3[] _vertPositions = null;

		[SerializeField]
		private Vector2[] _vertUVs = null;

		[SerializeField]
		private int[] _vertUniqueIDs = null;

		[SerializeField]
		private int[] _vertTris = null;

		[SerializeField]
		private int[] _vertTris_Flipped = null;//<<추가 : Flipped된 경우 Reverse된 값을 사용한다.

		[SerializeField]
		private int _nVert = 0;
		//TODO : Vertex에 직접 값을 입력하는건 ModVert에서 하자

		/// <summary>Rendered Vertices</summary>
		public apOptRenderVertex[] RenderVertices { get { return _renderVerts; } }

		public Vector3[] LocalVertPositions { get { return _vertPositions; } }


		//<업데이트>
		[SerializeField]
		private Vector3[] _vertPositions_Updated = null;

		[SerializeField]
		private Vector3[] _vertPositions_Local = null;

		[SerializeField]
		private Vector2[] _vertPositions_World = null;

		//[SerializeField]
		//private Texture2D _texture_Updated = null;

		[SerializeField, HideInInspector]
		public Transform _transform = null;

		[NonSerialized]
		private bool _isInitMesh = false;

		[NonSerialized]
		private bool _isInitMaterial = false;

		[SerializeField]
		private Vector2 _pivotPos = Vector2.zero;

		[SerializeField]
		private bool _isVisibleDefault = true;

		[NonSerialized]
		private bool _isVisible = false;

		/// <summary>기본값이 아닌 외부에서 숨기려고 할 때 설정된다. RootUnit이 Show 될때 해제된다.</summary>
		[NonSerialized]
		private bool _isHide_External = false;


		//Mask인 경우
		//Child는 업데이트는 하지만 렌더링은 하지 않는다.
		//렌더링을 하지 않으므로 Mesh 갱신을 하지 않음
		//Parent는 업데이트 후 렌더링은 잠시 보류한다.
		//"통합" Vertex으로 정의된 SubMeshData에서 통합 작업을 거친 후에 Vertex 업데이트를 한다.
		//MaskMesh 업데이트는 Portrait에서 Calculate 후 일괄적으로 한다. (List로 관리한다.)
		/// <summary>[Please do not use it] Is Parent Mesh of Clipping Masking</summary>
		public bool _isMaskParent = false;

		/// <summary>[Please do not use it] Is Child Mesh of Clipping Masking</summary>
		public bool _isMaskChild = false;

		//Child인 경우
		/// <summary>[Please do not use it] Masking Parent Mesh ID if clipped</summary>
		public int _clipParentID = -1;

		/// <summary>[Please do not use it] Is Masking Parent Mesh if clipped</summary>
		public apOptMesh _parentOptMesh = null;

		//Parent인 경우
		/// <summary>[Please do not use it] Children if clipping mask </summary>
		public int[] _clipChildIDs = null;
		//public apOptMesh[] _childOptMesh = null;

		[NonSerialized]
		private Color _multiplyColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		[NonSerialized]
		private bool _isAnyMeshColorRequest = false;

		[NonSerialized]
		private bool _isAnyTextureRequest = false;

		[NonSerialized]
		private bool _isAnyCustomPropertyRequest = false;

		

		//수정 : Clipping시 처리가 바뀜
		//이전 >> 스텐실을 이용해서 Child에서 Parent의 버텍스를 받아서 AlphaTest (3-pass)
		//변경 
		//	>> Parent는 커맨드 버퍼를 카메라에 등록해서 RTT를 수행
		//	>> Child는 Parent의 RTT를 받아서 마스크 렌더링을 수행한다.
		// 병합된 버텍스는 삭제한다.



		#region [미사용 코드]
		//// Mask를 만들 경우 => 통합 데이터를 만든다.
		////자신을 포함한 서브메시 데이터
		////인덱스를 합쳐야하므로 처리가 다르다.
		//[Serializable]
		//public class SubMeshData
		//{
		//	public int _meshIndex = -1;//Parent는 0 (RGB 각각 1, 2, 3)
		//	public apOptMesh _optMesh = null;
		//	public Material _material = null;
		//	//public Vector3[] _verts_World = null;
		//	//public Vector3[] _verts_Local = null;
		//	//public int[] _triangles = null;
		//	//public Vector2[] _uvs = null;
		//	public int _nVert = 0;
		//	public int _nTri = 0;

		//	public int _vertIndexOffset = 0;
		//	//public Color _color = Color.clear;
		//	public Texture _texture = null;
		//	public bool _isVisible = false;

		//	public SubMeshData(int meshIndex, apOptMesh targetOptMesh, int vertexIndexOffset)
		//	{
		//		_meshIndex = meshIndex;
		//		_optMesh = targetOptMesh;
		//		_material = targetOptMesh._material;

		//		_nVert = targetOptMesh._renderVerts.Length;
		//		_nTri = targetOptMesh._vertTris.Length;

		//		//_verts_World = new Vector3[targetOptMesh._renderVerts.Length];
		//		//_verts_Local = new Vector3[targetOptMesh._renderVerts.Length];
		//		//_triangles = new int[targetOptMesh._vertTris.Length];
		//		//_uvs = new Vector2[targetOptMesh._vertUVs.Length];

		//		//for (int i = 0; i < _verts_World.Length; i++)
		//		//{
		//		//	_verts_World[i] = targetOptMesh._renderVerts[i]._vertPos3_World;
		//		//	_verts_Local[i] = Vector3.zero;//<<이건 계산 후에 적용
		//		//}

		//		//for (int i = 0; i < _triangles.Length; i++)
		//		//{
		//		//	_triangles[i] = targetOptMesh._vertTris[i];
		//		//}

		//		//for (int i = 0; i < _uvs.Length; i++)
		//		//{
		//		//	_uvs[i] = targetOptMesh._vertUVs[i];
		//		//}

		//		//_color = _material.color;
		//		_texture = targetOptMesh._texture;

		//		_vertIndexOffset = vertexIndexOffset;
		//	}


		//	public void SetVisible(bool isVisible)
		//	{
		//		_isVisible = isVisible;
		//	}

		//	public Color MeshColor
		//	{
		//		get
		//		{
		//			return _optMesh.MeshColor;
		//		}
		//	}
		//} 
		#endregion


		#region [미사용 코드]
		//[SerializeField]
		//public SubMeshData[] _subMeshes = null;//<<Parnet일때만 만든다.
		//private const int SUBMESH_BASE = 0;
		//private const int SUBMESH_CLIP1 = 1;
		//private const int SUBMESH_CLIP2 = 2;
		//private const int SUBMESH_CLIP3 = 3;

		//private static Color VertexColor_Base = new Color(0.0f, 0.0f, 0.0f, 1.0f);// Black
		//private static Color VertexColor_Clip1 = new Color(1.0f, 0.0f, 0.0f, 1.0f); //Red
		//private static Color VertexColor_Clip2 = new Color(0.0f, 1.0f, 0.0f, 1.0f); //Green
		//private static Color VertexColor_Clip3 = new Color(0.0f, 0.0f, 1.0f, 1.0f); //Blue

		//[SerializeField]
		//private Vector3[] _vertPosList_ForMask = null;//전체 Vertex 위치 (Local)

		//[SerializeField]
		//private Color[] _vertColorList_ForMask = null;//전체 Vertex의 VertColor (Black - RGB) 
		#endregion

		//[SerializeField]
		//private Vector3[] _vertPosList_ClippedMerge = null;

		//[SerializeField]
		//private Color[] _vertColorList_ClippedMerge = null;

		//[SerializeField]
		//private int _nVertParent = 0;

		/// <summary>[Please do not use it] Updated Matrix</summary>
		public apMatrix3x3 _matrix_Vert2Mesh = apMatrix3x3.identity;

		/// <summary>[Please do not use it] Updated Matrix</summary>
		public apMatrix3x3 _matrix_Vert2Mesh_Inverse = apMatrix3x3.identity;

		/// <summary>[Please do not use it] Rendering Shader Type</summary>
		[SerializeField]
		public apPortrait.SHADER_TYPE _shaderType = apPortrait.SHADER_TYPE.AlphaBlend;

		/// <summary>[Please do not use it] Shader (not Clipped)</summary>
		[SerializeField]
		public Shader _shaderNormal = null;

		/// <summary>[Please do not use it] Shader (Clipped)</summary>
		[SerializeField]
		public Shader _shaderClipping = null;

		//추가
		/// <summary>[Please do not use it] Shader (Mask Parent)</summary>
		[SerializeField]
		public Shader _shader_AlphaMask = null;//<< Mask Shader를 저장하고 나중에 생성한다.

		[NonSerialized]
		private Material _materialAlphaMask = null;

		/// <summary>[Please do not use it] Mask Texture Size</summary>
		[SerializeField]
		public int _clippingRenderTextureSize = 256;

		#region [미사용 코드]
		//private static Color[] ShaderTypeColor = new Color[] {  new Color(1.0f, 0.0f, 0.0f, 0.0f),
		//															new Color(0.0f, 1.0f, 0.0f, 0.0f),
		//															new Color(0.0f, 0.0f, 1.0f, 0.0f),
		//															new Color(0.0f, 0.0f, 0.0f, 1.0f)}; 
		#endregion

		//클리핑 Parent인 경우
		//한개의 카메라에 대해서만 검색한다.
		[NonSerialized]
		private bool _isRenderTextureCreated = false;

		[NonSerialized]
		private RenderTexture _maskRenderTexture = null;

		[NonSerialized]
		private RenderTargetIdentifier _maskRenderTargetID = -1;

		[NonSerialized]
		private Camera _targetCamera = null;

		[NonSerialized]
		private Transform cameraTransform = null;

		[NonSerialized]
		private CommandBuffer _commandBuffer = null;

		public RenderTexture MaskRenderTexture
		{
			get
			{
				if(!_isRenderTextureCreated || !_isVisible)
				{
					return null;
				}
				return _maskRenderTexture;
			}
		}

		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public Vector4 _maskScreenSpaceOffset = Vector4.zero;


		private RenderTexture _prevParentRenderTexture = null;
		private RenderTexture _curParentRenderTexture = null;

		//효율적인 Mask를 렌더링하기 위한 변수.
		//딱 렌더링 부분만 렌더링하자
		private Vector3 _vertPosCenter = Vector3.zero;
		//private float _vertRangeMax = 0.0f;

		private float _vertRange_XMin = 0.0f;
		private float _vertRange_YMax = 0.0f;
		private float _vertRange_XMax = 0.0f;
		private float _vertRange_YMin = 0.0f;

		
		private int _shaderID_MainTex = -1;
		private int _shaderID_Color = -1;
		private int _shaderID_MaskTexture = -1;
		private int _shaderID_MaskScreenSpaceOffset = -1;

		//계산용 변수들
		private Vector3 _cal_localPos_LT = Vector3.zero;
		private Vector3 _cal_localPos_RB = Vector3.zero;
		private Vector3 _cal_vertWorldPos_Center = Vector3.zero;
		private Vector3 _cal_vertWorldPos_LT = Vector3.zero;
		private Vector3 _cal_vertWorldPos_RB = Vector3.zero;
		private Vector3 _cal_screenPos_Center = Vector3.zero;
		private Vector3 _cal_screenPos_LT = Vector3.zero;
		private Vector3 _cal_screenPos_RB = Vector3.zero;
		private float _cal_prevSizeWidth = 0.0f;
		private float _cal_prevSizeHeight = 0.0f;
		private float _cal_zoomScale = 0.0f;
		private float _cal_aspectRatio = 0.0f;
		private float _cal_newOrthoSize = 0.0f;
		private Vector2 _cal_centerMoveOffset = Vector2.zero;
		private float _cal_distCenterToCamera = 0.0f;
		private Vector3 _cal_nextCameraPos = Vector3.zero;
		//private Vector3 _cal_camOffset = Vector3.zero;
		private Matrix4x4 _cal_customWorldToCamera = Matrix4x4.identity;
		private Matrix4x4 _cal_customCullingMatrix = Matrix4x4.identity;
		private Matrix4x4 _cal_newLocalToProjMatrix = Matrix4x4.identity;
		private Matrix4x4 _cal_newWorldMatrix = Matrix4x4.identity;
		private Vector3 _cal_screenPosOffset = Vector3.zero;

		private Color _cal_MeshColor = Color.gray;

		private bool _cal_isVisibleRequest = false;
		private bool _cal_isVisibleRequest_Masked = false;

		//추가 3.22
		//Transform이 Flipped된 경우 -> Vertex 배열을 역으로 계산해야한다.
		private bool _cal_isFlipped = false;
		private bool _cal_isFlipped_Prev = false;

		// Init
		//------------------------------------------------
		void Awake()
		{	
			_transform = transform;
			_cal_isFlipped = false;
			_cal_isFlipped_Prev = false;

			_shaderID_MainTex = Shader.PropertyToID("_MainTex");
			_shaderID_Color = Shader.PropertyToID("_Color");
			_shaderID_MaskTexture = Shader.PropertyToID("_MaskTex");
			_shaderID_MaskScreenSpaceOffset = Shader.PropertyToID("_MaskScreenSpaceOffset");
		}

		void Start()
		{
			//InitMesh(false);
			//InstantiateMesh();

			//this.enabled = true;
			this.enabled = false;
		}


		void OnEnable()
		{
			if (_isInitMesh && _isInitMaterial)
			{
				CleanUpMaskParent();
			}
		}

		void OnDisable()
		{
			if (_isInitMesh && _isInitMaterial)
			{
				CleanUpMaskParent();
			}
		}

		void OnWillRenderObject()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}
#endif
			if(_isMaskParent && _isInitMesh && _isInitMaterial)
			{
				RegistCommandBuffer();
			}
		}

		// Bake
		//------------------------------------------------
		/// <summary>[Please do not use it] Bake Functions</summary>
		public void BakeMesh(Vector3[] vertPositions,
								Vector2[] vertUVs,
								int[] vertUniqueIDs,
								int[] vertTris,
								float[] depths,
								Vector2 pivotPos,
								apOptTransform parentTransform,
								Texture2D texture, int textureID,
								apPortrait.SHADER_TYPE shaderType,
								Shader shaderNormal, Shader shaderClipping, 
								Shader alphaMask, int maskRenderTextureSize,
								bool isVisibleDefault,
								bool isMaskParent, bool isMaskChild,
								int batchedMatID, Material batchedMaterial)
		{
			_parentTransform = parentTransform;

			_vertPositions = vertPositions;
			_vertUVs = vertUVs;
			_vertUniqueIDs = vertUniqueIDs;
			_vertTris = vertTris;

			//추가 : Flipped Tris를 만들자
			_vertTris_Flipped = new int[_vertTris.Length];
			for (int i = 0; i < _vertTris.Length - 2; i+=3)
			{
				_vertTris_Flipped[i + 0] = _vertTris[i + 2];
				_vertTris_Flipped[i + 1] = _vertTris[i + 1];
				_vertTris_Flipped[i + 2] = _vertTris[i + 0];
			}

			_texture = texture;
			_textureID = textureID;

			_pivotPos = pivotPos;
			_nVert = _vertPositions.Length;

			_isVisibleDefault = isVisibleDefault;

			transform.localPosition += new Vector3(-_pivotPos.x, -_pivotPos.y, 0.0f);

			_matrix_Vert2Mesh = apMatrix3x3.TRS(new Vector2(-_pivotPos.x, -_pivotPos.y), 0, Vector2.one);
			_matrix_Vert2Mesh_Inverse = _matrix_Vert2Mesh.inverse;

			_shaderType = shaderType;
			_shaderNormal = shaderNormal;
			_shaderClipping = shaderClipping;

			_shader_AlphaMask = alphaMask;//MaskShader를 넣는다.

			//_materialAlphaMask = new Material(alphaMask);이건 나중에 처리
			_clippingRenderTextureSize = maskRenderTextureSize;

			_isMaskParent = isMaskParent;
			_isMaskChild = isMaskChild;

			//Batch가 가능한 경우
			//1. Mask Child가 아닐 경우
			//2. Parent Tranform의 Default Color가 Gray인 경우
			_isDefaultColorGray =	Mathf.Abs(_parentTransform._meshColor2X_Default.r - 0.5f) < 0.001f &&
									Mathf.Abs(_parentTransform._meshColor2X_Default.g - 0.5f) < 0.001f &&
									Mathf.Abs(_parentTransform._meshColor2X_Default.b - 0.5f) < 0.001f &&
									Mathf.Abs(_parentTransform._meshColor2X_Default.a - 1.0f) < 0.001f;
			
			if(!_isDefaultColorGray)
			{
				//Debug.Log("Gray가 아닌 mesh : " + this.name);
			}
			_isBatchedMaterial = !isMaskChild && _isDefaultColorGray;

			_batchedMatID = batchedMatID;

			if (_shaderNormal == null)
			{
				Debug.LogError("Shader Normal is Null");
			}
			if (_shaderClipping == null)
			{
				Debug.LogError("Shader Clipping is Null");
			}

			//RenderVert를 만들어주자
			_renderVerts = new apOptRenderVertex[_nVert];
			for (int i = 0; i < _nVert; i++)
			{
				_renderVerts[i] = new apOptRenderVertex(
											_parentTransform, this,
											_vertUniqueIDs[i], i,
											new Vector2(vertPositions[i].x, vertPositions[i].y),
											_vertUVs[i],
											depths[i]);

				_renderVerts[i].SetMatrix_1_Static_Vert2Mesh(_matrix_Vert2Mesh);
				_renderVerts[i].SetMatrix_3_Transform_Mesh(parentTransform._matrix_TFResult_WorldWithoutMod.MtrxToSpace);
				_renderVerts[i].Calculate();
			}

			if (_meshFilter == null || _mesh == null)
			{
				//일단 만들어두기는 한다.
				_meshFilter = GetComponent<MeshFilter>();
				_mesh = new Mesh();
				_mesh.name = this.name + "_Mesh";
				_mesh.Clear();

				_mesh.vertices = _vertPositions;
				_mesh.uv = _vertUVs;
				_mesh.triangles = _vertTris;

				_mesh.RecalculateNormals();
				_mesh.RecalculateBounds();

				

				_meshFilter.sharedMesh = _mesh;
			}

			_mesh.Clear();
			_cal_isFlipped = false;
			_cal_isFlipped_Prev = false;

			//재질 설정을 해주자
			if(_sharedMaterial != null)
			{
				try
				{
					UnityEngine.Object.DestroyImmediate(_sharedMaterial);
				}
				catch(Exception) { }
				_sharedMaterial = null;
			}

			_material = null;
			_instanceMaterial = null;
			_isUseSharedMaterial = true;

			//Batch된 재질을 사용하는 경우는 재질을 받아온다.
			//그렇지 않은 경우는 Null로 둔다. (Mask Child인 경우이다)
			if(!_isBatchedMaterial)
			{	
				//isMaskChild인 경우에는 SharedMaterial을 이 단계에서 만들지 않는다. (바로 뒤에 만들 것이므로)
				_sharedMaterial = null;

				//..아니다 Bake후에 보여질게 없다. 여기서 만들자
				_sharedMaterial = new Material(_shaderNormal);
				_sharedMaterial.SetColor("_Color", _parentTransform._meshColor2X_Default);
				_sharedMaterial.SetTexture("_MainTex", _texture);

				_material = _sharedMaterial;


			}
			else
			{
				//Batch 대상이면 일단 Null로 둔다.
				_sharedMaterial = null;

				//현재 Material 정보는 Batch된 재질을 설정한다.
				_material = batchedMaterial;
			}

			

			if (_meshRenderer == null)
			{
				_meshRenderer = GetComponent<MeshRenderer>();
			}
			_meshRenderer.sharedMaterial = _material;//<<현재 Bake된 Material 값을 넣어준다.

			//그림자 설정은 제외
			_meshRenderer.receiveShadows = false;
			_meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			_meshRenderer.enabled = _isVisibleDefault;
			_meshRenderer.lightProbeUsage = LightProbeUsage.Off;

			
			//Mask 연결 정보는 일단 리셋
			_clipParentID = -1;
			_clipChildIDs = null;

			_vertPositions_Updated = new Vector3[_vertPositions.Length];
			_vertPositions_Local = new Vector3[_vertPositions.Length];
			_vertPositions_World = new Vector2[_vertPositions.Length];

			for (int i = 0; i < _vertPositions.Length; i++)
			{
				//Calculate 전에는 직접 Pivot Pos를 적용해주자 (Calculate에서는 자동 적용)
				_vertPositions_Updated[i] = _renderVerts[i]._vertPos3_LocalUpdated;
			}
			

			_transform = transform;

			_shaderID_MainTex = Shader.PropertyToID("_MainTex");
			_shaderID_Color = Shader.PropertyToID("_Color");
			_shaderID_MaskTexture = Shader.PropertyToID("_MaskTex");
			_shaderID_MaskScreenSpaceOffset = Shader.PropertyToID("_MaskScreenSpaceOffset");


			InitMesh(true);

			RefreshMesh();

			if(_isVisibleDefault)
			{
				_meshRenderer.enabled = true;
				_isVisible = true;
				
			}
			else
			{
				_meshRenderer.enabled = false;
				_isVisible = false;
			}
		}



		//-----------------------------------------------------------------------
		//Bake되지 않는 Mesh의 초기화를 호출한다.
		/// <summary>
		/// [Please do not use it]
		/// It is called by "Portrait"
		/// </summary>
		public void InitMesh(bool isForce)
		{
			if (!isForce && _isInitMesh)
			{
				return;
			}

			_transform = transform;

			if(_mesh == null)
			{
				_mesh = new Mesh();
				_mesh.name = this.name + "_Mesh (Instance)";
				_mesh.Clear();

				_mesh.vertices = _vertPositions;
				_mesh.triangles = _vertTris;
				_mesh.uv = _vertUVs;

				_mesh.RecalculateNormals();
				_mesh.RecalculateBounds();
			}

			
			
			if (_meshFilter == null)
			{
				_meshFilter = GetComponent<MeshFilter>();
			}

			_meshFilter.mesh = _mesh;
			

			if (_meshRenderer == null)
			{
				_meshRenderer = GetComponent<MeshRenderer>();
			}

			_meshRenderer.material = _material;
			_meshRenderer.enabled = _isVisibleDefault;
			_isVisible = _isVisibleDefault;
			
			if (_vertPositions_Updated == null || _vertPositions_Local == null)
			{
				_vertPositions_Updated = new Vector3[_vertPositions.Length];
				_vertPositions_Local = new Vector3[_vertPositions.Length];
				for (int i = 0; i < _vertPositions.Length; i++)
				{
					_vertPositions_Updated[i] = _vertPositions[i];
				}
			}

			//_texture_Updated = _texture;

			_isInitMesh = true;

			_cal_isFlipped = false;
			_cal_isFlipped_Prev = false;
		}

		/// <summary>
		/// [Please do not use it]
		/// Initialize Mesh
		/// </summary>
		public void InstantiateMesh()
		{	
			if(_mesh == null || _meshFilter == null || _meshFilter.mesh == null)
			{
				return;
			}

			_meshFilter.mesh = Instantiate<Mesh>(_meshFilter.mesh);
			_mesh = _meshFilter.mesh;
		}

		/// <summary>
		/// [Please do not use it]
		/// Initialize Materials
		/// </summary>
		public void InstantiateMaterial(apOptBatchedMaterial batchedMaterial)
		{
			if(_isInitMaterial)
			{
				return;
			}

			_material = null;
			_instanceMaterial = null;
			//_sharedMaterial = null;

			if(_isBatchedMaterial)
			{
				_sharedMaterial = batchedMaterial.GetMaterial(_batchedMatID);
			}
			else if(_isMaskChild)
			{
				_sharedMaterial = new Material(_shaderClipping);
				_sharedMaterial.SetColor("_Color", _parentTransform._meshColor2X_Default);
				_sharedMaterial.SetTexture("_MainTex", _texture);
			}
			//else if(!_isDefaultColorGray)
			//{
			//	Debug.Log("Make Shared Material (Not Batched : " + this.name + ")");
			//	_sharedMaterial = new Material(_shaderNormal);
			//	_sharedMaterial.SetColor("_Color", _parentTransform._meshColor2X_Default);
			//	_sharedMaterial.SetTexture("_MainTex", _texture);
			//}

			if(_sharedMaterial == null)
			{
				//Debug.LogError("Null Material [" + this.name + "]");

				//어쩔수 없이 하나 만들어서 사용한다.
				_sharedMaterial = new Material(_shaderNormal);
				_sharedMaterial.SetColor("_Color", _parentTransform._meshColor2X_Default);
				_sharedMaterial.SetTexture("_MainTex", _texture);
			}



			_instanceMaterial = Instantiate<Material>(_sharedMaterial);
			if(_isMaskParent)
			{
				//_materialAlphaMask = Instantiate<Material>(_materialAlphaMask);
				if(_materialAlphaMask == null)
				{
					_materialAlphaMask = new Material(_shader_AlphaMask);
				}
			}

			if (!_isMaskChild && _isDefaultColorGray)
			{
				//기본값은 Batch가 되도록 처리
				_isUseSharedMaterial = true;
				_material = _sharedMaterial;

				_meshRenderer.material = _material;
			}
			else
			{
				//Mask Child는 필연적으로 Instance 값만 사용한다.
				_isUseSharedMaterial = false;
				_material = _instanceMaterial;

				_meshRenderer.material = _material;
			}

			_isInitMaterial = true;
		}

		//---------------------------------------------------------------------------
		// Mask 관련 초기화
		//---------------------------------------------------------------------------
		/// <summary>
		/// [Please do not use it]
		/// Initialize if it is Mask Parent
		/// </summary>
		public void SetMaskBasicSetting_Parent(List<int> clipChildIDs)
		{
			if (clipChildIDs == null || clipChildIDs.Count == 0)
			{
				return;
			}
			_isMaskParent = true;
			_clipParentID = -1;
			_isMaskChild = false;


			if (_clipChildIDs == null || _clipChildIDs.Length != clipChildIDs.Count)
			{
				_clipChildIDs = new int[clipChildIDs.Count];
			}

			for (int i = 0; i < clipChildIDs.Count; i++)
			{
				_clipChildIDs[i] = clipChildIDs[i];
			}
			//_clipChildIDs[0] = clipChildIDs[0];
			//_clipChildIDs[1] = clipChildIDs[1];
			//_clipChildIDs[2] = clipChildIDs[2];
		}

		/// <summary>
		/// [Please do not use it]
		/// Initialize if it is Mask Child
		/// </summary>
		public void SetMaskBasicSetting_Child(int parentID)
		{
			_isMaskParent = false;
			_clipParentID = parentID;
			_isMaskChild = true;

			_clipChildIDs = null;
		}


		#region [미사용 코드] Parent 중심의 Clipping
		//public void LinkAsMaskParent(apOptMesh[] childMeshes)
		//{
		//	if(_childOptMesh == null || _childOptMesh.Length != 3)
		//	{
		//		_childOptMesh = new apOptMesh[3];
		//	}

		//	_childOptMesh[0] = childMeshes[0];
		//	_childOptMesh[1] = childMeshes[1];
		//	_childOptMesh[2] = childMeshes[2];


		//	_meshRenderer.enabled = true;

		//	//이제 SubMesh 데이터를 만들어주자
		//	_subMeshes = new SubMeshData[4];
		//	//1. 자기 자신을 넣는다.
		//	int vertexIndexOffset = 0;
		//	_subMeshes[0] = new SubMeshData(SUBMESH_BASE, this, 0);
		//	_subMeshes[0].SetVisible(true);

		//	vertexIndexOffset += _subMeshes[0]._nVert;

		//	//2. 자식 Mesh를 넣는다.
		//	int iChildMesh = 1;
		//	for (int i = 0; i < 3; i++)
		//	{
		//		if(_childOptMesh[i] == null)
		//		{
		//			_subMeshes[iChildMesh] = null;
		//			iChildMesh++;
		//			continue;
		//		}

		//		_subMeshes[iChildMesh] = new SubMeshData(iChildMesh, _childOptMesh[i], vertexIndexOffset);
		//		vertexIndexOffset += _subMeshes[iChildMesh]._nVert;

		//		iChildMesh++;
		//	}

		//	int nTotalVerts = vertexIndexOffset;//<<전체 Vertex의 개수
		//										//이제 전체 Mesh를 만들자

		//	_vertPosList_ForMask = new Vector3[nTotalVerts];
		//	_vertColorList_ForMask = new Color[nTotalVerts];
		//	List<int> vertIndexList_ForMask = new List<int>();
		//	List<Vector2> vertUVs_ForMask = new List<Vector2>();

		//	for (int iSM = 0; iSM < 4; iSM++)
		//	{
		//		SubMeshData subMesh = _subMeshes[iSM];
		//		if(subMesh == null)
		//		{
		//			continue;
		//		}
		//		//Vertex 먼저
		//		Color vertColor = Color.clear;
		//		switch (iSM)
		//		{
		//			case SUBMESH_BASE: vertColor = VertexColor_Base; break;
		//			case SUBMESH_CLIP1: vertColor = VertexColor_Clip1; break;
		//			case SUBMESH_CLIP2: vertColor = VertexColor_Clip2; break;
		//			case SUBMESH_CLIP3: vertColor = VertexColor_Clip3; break;

		//		}
		//		for (int iVert = 0; iVert < subMesh._nVert; iVert++)
		//		{
		//			_vertPosList_ForMask[iVert + subMesh._vertIndexOffset] = subMesh._optMesh._renderVerts[iVert]._pos3_Local;
		//			_vertColorList_ForMask[iVert + subMesh._vertIndexOffset] = vertColor;
		//			vertUVs_ForMask.Add(subMesh._optMesh._vertUVs[iVert]);
		//		}

		//		for (int iTri = 0; iTri < subMesh._nTri; iTri++)
		//		{
		//			vertIndexList_ForMask.Add(subMesh._optMesh._vertTris[iTri] + subMesh._vertIndexOffset);
		//		}
		//	}

		//	_mesh.Clear();
		//	_mesh.vertices = _vertPosList_ForMask;
		//	_mesh.triangles = vertIndexList_ForMask.ToArray();
		//	_mesh.uv = vertUVs_ForMask.ToArray();
		//	_mesh.colors = _vertColorList_ForMask;



		//	//재질도 다시 세팅하자
		//	Color color_Base = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		//	Color color_Clip1 = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		//	Color color_Clip2 = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		//	Color color_Clip3 = new Color(0.0f, 0.0f, 0.0f, 0.0f);

		//	Texture texture_Base = null;
		//	Texture texture_Clip1 = null;
		//	Texture texture_Clip2 = null;
		//	Texture texture_Clip3 = null;



		//	apPortrait.SHADER_TYPE shaderType_Clip1 = apPortrait.SHADER_TYPE.AlphaBlend;
		//	apPortrait.SHADER_TYPE shaderType_Clip2 = apPortrait.SHADER_TYPE.AlphaBlend;
		//	apPortrait.SHADER_TYPE shaderType_Clip3 = apPortrait.SHADER_TYPE.AlphaBlend;

		//	if(_subMeshes[SUBMESH_BASE] != null)
		//	{
		//		if (_subMeshes[SUBMESH_BASE]._isVisible)
		//		{
		//			color_Base = _subMeshes[SUBMESH_BASE].MeshColor;
		//		}
		//		else
		//		{
		//			color_Base = Color.clear;
		//		}
		//		texture_Base = _subMeshes[SUBMESH_BASE]._texture;
		//	}

		//	if(_subMeshes[SUBMESH_CLIP1] != null)
		//	{
		//		if(_subMeshes[SUBMESH_CLIP1]._isVisible)
		//		{	
		//			color_Clip1 = _subMeshes[SUBMESH_CLIP1].MeshColor;
		//		}
		//		else
		//		{
		//			color_Clip1 = Color.clear;
		//		}

		//		texture_Clip1 = _subMeshes[SUBMESH_CLIP1]._texture;
		//		shaderType_Clip1 = _subMeshes[SUBMESH_CLIP1]._optMesh._shaderType;
		//	}

		//	if(_subMeshes[SUBMESH_CLIP2] != null)
		//	{
		//		if(_subMeshes[SUBMESH_CLIP2]._isVisible)
		//		{
		//			color_Clip2 = _subMeshes[SUBMESH_CLIP2].MeshColor;
		//		}
		//		else
		//		{
		//			color_Clip2 = Color.clear;
		//		}

		//		texture_Clip2 = _subMeshes[SUBMESH_CLIP2]._texture;
		//		shaderType_Clip2 = _subMeshes[SUBMESH_CLIP2]._optMesh._shaderType;
		//	}

		//	if(_subMeshes[SUBMESH_CLIP3] != null)
		//	{
		//		if (_subMeshes[SUBMESH_CLIP3]._isVisible)
		//		{
		//			color_Clip3 = _subMeshes[SUBMESH_CLIP3].MeshColor;
		//		}
		//		else
		//		{
		//			color_Clip3 = Color.clear;
		//		}
		//		texture_Clip3 = _subMeshes[SUBMESH_CLIP3]._texture;
		//		shaderType_Clip3 = _subMeshes[SUBMESH_CLIP3]._optMesh._shaderType;
		//	}

		//	//_material = new Material(Shader.Find("AnyPortrait/Transparent/Masked Colored Texture (2X)"));
		//	//Debug.Log("Link Mask Clip - " + _shaderName_Clipping);
		//	//_material = new Material(Shader.Find(_shaderName_Clipping));
		//	_material = new Material(_shaderClipping);

		//	_material.SetColor("_Color", color_Base);
		//	_material.SetColor("_Color1", color_Clip1);
		//	_material.SetColor("_Color2", color_Clip2);
		//	_material.SetColor("_Color3", color_Clip3);

		//	_material.SetTexture("_MainTex", texture_Base);
		//	_material.SetTexture("_ClipTexture1", texture_Clip1);
		//	_material.SetTexture("_ClipTexture2", texture_Clip2);
		//	_material.SetTexture("_ClipTexture3", texture_Clip3);


		//	Debug.Log("Link Clip : " + shaderType_Clip1 + " / " + shaderType_Clip2 + " / " + shaderType_Clip3);

		//	_material.SetColor("_BlendOpt1", ShaderTypeColor[(int)shaderType_Clip1]);
		//	_material.SetColor("_BlendOpt2", ShaderTypeColor[(int)shaderType_Clip2]);
		//	_material.SetColor("_BlendOpt3", ShaderTypeColor[(int)shaderType_Clip3]);

		//	_meshRenderer.sharedMaterial = _material;

		//	RefreshMaskedMesh();

		//	_mesh.RecalculateBounds();
		//	_mesh.RecalculateNormals();


		//} 
		#endregion


		/// <summary>
		/// [Please do not use it]
		/// Initialize reference
		/// </summary>
		public void LinkAsMaskChild(apOptMesh parentMesh)
		{
			_parentOptMesh = parentMesh;

			if(_sharedMaterial != null)
			{
				UnityEngine.Object.DestroyImmediate(_sharedMaterial);
				_sharedMaterial = null;
			}
			_sharedMaterial = new Material(_shaderClipping);
			//_sharedMaterial.SetColor("_Color", MeshColor);
			_sharedMaterial.SetColor("_Color", _parentTransform._meshColor2X_Default);
			_sharedMaterial.SetTexture("_MainTex", _texture);

			_material = _sharedMaterial;
			_instanceMaterial = null;

			_meshRenderer.sharedMaterial = _material;
			_isUseSharedMaterial = true;


			//이부분 코드를 날린다.
			#region [미사용 코드 : 스텐실 병합 방식의 코드]
			////Child라면 Rendering이 되지 않는다.
			////_meshRenderer.enabled = false;
			////> 수정
			////Child도 렌더링을 다 한다.
			//_meshRenderer.enabled = true;

			////일반 재질이 아니라, Parent로 부터 Mask를 받는 스텐실 재질로 전환한다.
			//_nVertParent = _parentOptMesh._renderVerts.Length;

			//int nTotalVert = _nVert + _nVertParent;
			//_vertPosList_ClippedMerge = new Vector3[nTotalVert];
			//_vertColorList_ClippedMerge = new Color[nTotalVert];
			////Parent -> 자기 자신 순으로 Vertex를 넣는다.
			//Color vertColor_Parent = new Color(1.0f, 0.0f, 0.0f, 0.0f);
			//Color vertColor_Self = new Color(0.0f, 0.0f, 0.0f, 0.0f);

			//List<int> vertIndexList_ClippedMerge = new List<int>();
			//List<Vector2> vertUVs_ClippedMerge = new List<Vector2>();

			//apOptMesh[] subMeshes = new apOptMesh[] { _parentOptMesh, this };
			//int iVertOffset = 0;
			//for (int iMesh = 0; iMesh < 2; iMesh++)
			//{
			//	apOptMesh subMesh = subMeshes[iMesh];
			//	Color vertColor = vertColor_Parent;
			//	iVertOffset = 0;
			//	if (iMesh == 1)
			//	{
			//		vertColor = vertColor_Self;
			//		iVertOffset = _nVertParent;
			//	}

			//	for (int iVert = 0; iVert < subMesh._renderVerts.Length; iVert++)
			//	{
			//		_vertPosList_ClippedMerge[iVert + iVertOffset] = subMesh._renderVerts[iVert]._pos_Local;
			//		_vertColorList_ClippedMerge[iVert + iVertOffset] = vertColor;
			//		vertUVs_ClippedMerge.Add(subMesh._vertUVs[iVert]);
			//	}

			//	int nTri = subMesh._vertTris.Length;
			//	for (int iTri = 0; iTri < nTri; iTri++)
			//	{
			//		vertIndexList_ClippedMerge.Add(subMesh._vertTris[iTri] + iVertOffset);
			//	}
			//}

			//_mesh.Clear();
			//_mesh.vertices = _vertPosList_ClippedMerge;
			//_mesh.triangles = vertIndexList_ClippedMerge.ToArray();
			//_mesh.uv = vertUVs_ClippedMerge.ToArray();
			//_mesh.colors = _vertColorList_ClippedMerge;

			//_material = new Material(_shaderClipping);
			//_material.SetColor("_Color", MeshColor);
			//_material.SetTexture("_MainTex", _texture);

			//_material.SetColor("_MaskColor", _parentOptMesh.MeshColor);
			//_material.SetTexture("_MaskTex", _parentOptMesh._texture);

			//_meshRenderer.sharedMaterial = _material;

			//_mesh.RecalculateBounds();
			//_mesh.RecalculateNormals();
			//_mesh.MarkDynamic();

			//RefreshClippedMesh(); 
			#endregion
		}

		//Mask Parent의 세팅을 리셋한다.
		//카메라 설정이나 씬이 변경되었을 때 호출해야한다.
		/// <summary>
		/// If it is Mask Parent, reset Command Buffers to Camera
		/// </summary>
		public void ResetMaskParentSetting()
		{
			CleanUpMaskParent();

#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}
#endif

			RegistCommandBuffer();
		}

		
		//---------------------------------------------------------------------------


		// Update
		//------------------------------------------------
		void Update()
		{
			
		}

		void LateUpdate()
		{

		}
		





		// 외부 업데이트
		//------------------------------------------------
		public void ReadyToUpdate()
		{
			//?
		}

		/// <summary>
		/// [Please do not use it]
		/// Update Shape of Mesh
		/// </summary>
		/// <param name="isRigging"></param>
		/// <param name="isVertexLocal"></param>
		/// <param name="isVertexWorld"></param>
		/// <param name="isVisible"></param>
		public void UpdateCalculate(bool isRigging, bool isVertexLocal, bool isVertexWorld, bool isVisible)
		{
			_cal_isVisibleRequest = _isVisible;
			
			if (!_isHide_External)
			{
				_cal_isVisibleRequest = isVisible;
			}
			else
			{
				//강제로 Hide하는 요청이 있었다면?
				_cal_isVisibleRequest = false;
			}

			_cal_isVisibleRequest_Masked = _cal_isVisibleRequest;

			//추가
			//Mask 메시가 있다면
			if(_isMaskChild)
			{
				if(_parentOptMesh != null)
				{
					_cal_isVisibleRequest_Masked = _parentOptMesh._isVisible;
				}
			}

			//둘다 True일때 Show, 하나만 False여도 Hide
			//상태가 바뀔때에만 Show/Hide 토글
			if(_cal_isVisibleRequest && _cal_isVisibleRequest_Masked)
			{
				if(!_isVisible)
				{
					Show();
				}
			}
			else
			{
				if(_isVisible)
				{
					Hide();
				}
			}


			//안보이는건 업데이트하지 말자
			if (_isVisible)
			{

				apOptRenderVertex rVert = null;

//#if UNITY_EDITOR
//					Profiler.BeginSample("Opt Mesh - Update calculate Render Vertices");
//#endif

					apOptCalculatedResultStack calculateStack = _parentTransform.CalculatedStack;

					for (int i = 0; i < _nVert; i++)
					{

						rVert = _renderVerts[i];

						//리깅 추가
						if (isRigging)
						{
							//지연 코드 버전
							rVert._matrix_Rigging.SetMatrixWithWeight(calculateStack.GetDeferredRiggingMatrix(i), calculateStack._result_RiggingWeight);
						}

						if (isVertexLocal)
						{
							rVert._matrix_Cal_VertLocal.SetTRS(calculateStack.GetDeferredLocalPos(i));
						}

						rVert.SetMatrix_3_Transform_Mesh(_parentTransform._matrix_TFResult_World.MtrxToSpace);

						if (isVertexWorld)
						{
							rVert._matrix_Cal_VertWorld.SetTRS(calculateStack._result_VertWorld[i]);
						}

						rVert.Calculate();

						//업데이트 데이터를 넣어준다.
						_vertPositions_Updated[i] = rVert._vertPos3_LocalUpdated;
						_vertPositions_World[i] = rVert._vertPos_World;

					}
//#if UNITY_EDITOR
//						Profiler.EndSample();
//#endif
			}


			//_material.SetColor("_Color", _multiplyColor * _parentTransform._meshColor2X);
			

			//색상을 제어할 때
			//만약 Color 기본값인 경우 Batch를 위해 Shared로 교체해야한다.
			//색상 지정은 Instance일때만 가능하다

			if((_isAnyMeshColorRequest || _parentTransform._isAnyColorCalculated || !_isDefaultColorGray) && _instanceMaterial != null)
			{
				if(_isUseSharedMaterial && !_isMaskChild)
				{
					//Shared를 쓰는 중이라면 교체해야함
					AutoSelectMaterial();
				}

				_cal_MeshColor.r = _multiplyColor.r * _parentTransform._meshColor2X.r * 2;
				_cal_MeshColor.g = _multiplyColor.g * _parentTransform._meshColor2X.g * 2;
				_cal_MeshColor.b = _multiplyColor.b * _parentTransform._meshColor2X.b * 2;
				_cal_MeshColor.a = _multiplyColor.a * _parentTransform._meshColor2X.a;//Alpha는 2X가 아니다.

				_instanceMaterial.SetColor(_shaderID_Color, _cal_MeshColor);
			}
			else
			{
				if(!_isUseSharedMaterial && !_isMaskChild)
				{
					//반대로 색상 선택이 없는데 Instance Material을 사용중이라면 Batch를 해야하는 건 아닌지 확인해보자
					AutoSelectMaterial();
				}
			}

			
			if(_isMaskChild)
			{
				//Parent의 Mask를 받아서 넣자
				if (_parentOptMesh != null)
				{
					_curParentRenderTexture = _parentOptMesh.MaskRenderTexture;
				}
				else
				{
					_curParentRenderTexture = null;
					Debug.LogError("Null Parent");
				}

				if(_curParentRenderTexture != _prevParentRenderTexture)
				{
					//Debug.Log("Set Parent RenderTexture : " + (_curParentRenderTexture != null));
					//_material.SetTexture("_MaskTex", _curParentRenderTexture);
					_material.SetTexture(_shaderID_MaskTexture, _curParentRenderTexture);
					_prevParentRenderTexture = _curParentRenderTexture;
				}
				//_material.SetVector("_MaskScreenSpaceOffset", _parentOptMesh._maskScreenSpaceOffset);
				_material.SetVector(_shaderID_MaskScreenSpaceOffset, _parentOptMesh._maskScreenSpaceOffset);
			}

//#if UNITY_EDITOR
//			Profiler.BeginSample("Opt Mesh - Refresh Mesh");
//#endif

			#region [미사용 코드 : 스텐실 병합 방식에서는 MaskChild는 Refresh를 생략했다.]
			//if (!_isMaskChild)
			//{
			//	//Mask와 관련이 없는 경우만 갱신해준다.
			//	//메시 갱신
			//	RefreshMesh();
			//} 
			#endregion

			RefreshMesh();

			if(_isMaskParent)
			{
				//MaskParent면 CommandBuffer를 갱신한다.
				UpdateCommandBuffer();
			}

			if(_mesh != null)
			{
				_mesh.RecalculateNormals();
			}

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif
		}


		public void RefreshMaskMesh_WithoutUpdateCalculate()
		{
			//Calculate는 하지 않고
			if(_isMaskParent)
			{
				//MaskParent면 CommandBuffer를 갱신한다.
				UpdateCommandBuffer();
			}

			if(_isMaskChild)
			{
				//Parent의 Mask를 받아서 넣자
				if (_parentOptMesh != null)
				{
					_curParentRenderTexture = _parentOptMesh.MaskRenderTexture;
				}
				else
				{
					_curParentRenderTexture = null;
					Debug.LogError("Null Parent");
				}

				if(_curParentRenderTexture != _prevParentRenderTexture)
				{
					//Debug.Log("Set Parent RenderTexture : " + (_curParentRenderTexture != null));
					//_material.SetTexture("_MaskTex", _curParentRenderTexture);
					_material.SetTexture(_shaderID_MaskTexture, _curParentRenderTexture);
					_prevParentRenderTexture = _curParentRenderTexture;
				}
				//_material.SetVector("_MaskScreenSpaceOffset", _parentOptMesh._maskScreenSpaceOffset);
				_material.SetVector(_shaderID_MaskScreenSpaceOffset, _parentOptMesh._maskScreenSpaceOffset);
			}
		}


		// Vertex Refresh
		//------------------------------------------------
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void RefreshMesh()
		{
			//if(_isMaskChild || _isMaskParent)
			//{
			//	return;
			//}

			//변경 -> MaskParent는 그대로 업데이트 가능하고, Child만 따로 업데이트하자
			#region [미사용 코드 : 스텐실 병합 방식에서는 MaskChild는 Refresh를 생략했다.]
			//if (_isMaskChild)
			//{
			//	return;
			//} 
			#endregion

			//TODO : 이전에 먼저 transform을 수정해야한다.
			//Debug.Log("Refresh Mesh");

			//추가 : Flipped된 경우
			_cal_isFlipped = (_parentTransform._matrix_TFResult_World._scale.x *
								_parentTransform._matrix_TFResult_World._scale.y) < 0.0f;

			//Transform 제어 -> Vert 제어
			if (_isMaskParent)
			{
				//마스크 처리를 위해서 Vertex의 위치나 분포를 저장해야한다.
				_vertPosCenter = Vector3.zero;
				//_vertRangeMax = -1.0f;

				//Left < Right
				//Bottom < Top

				_vertRange_XMin = float.MaxValue;//Max -> Min
				_vertRange_XMax = float.MinValue;//Min -> Max
				_vertRange_YMin = float.MaxValue;//Max -> Min
				_vertRange_YMax = float.MinValue;//Min -> Max

				for (int i = 0; i < _nVert; i++)
				{
					//_vertPositions_Local[i] = _transform.InverseTransformPoint(_vertPositions_Updated[i]);
					_vertPositions_Local[i] = _vertPositions_Updated[i];

					_vertRange_XMin = Mathf.Min(_vertRange_XMin, _vertPositions_Local[i].x);
					_vertRange_XMax = Mathf.Max(_vertRange_XMax, _vertPositions_Local[i].x);
					_vertRange_YMin = Mathf.Min(_vertRange_YMin, _vertPositions_Local[i].y);
					_vertRange_YMax = Mathf.Max(_vertRange_YMax, _vertPositions_Local[i].y);
				}

				//마스크를 만들 영역을 잡아준다.
				_vertPosCenter.x = (_vertRange_XMin + _vertRange_XMax) * 0.5f;
				_vertPosCenter.y = (_vertRange_YMin + _vertRange_YMax) * 0.5f;
				//_vertRangeMax = Mathf.Max(_vertRange_XMax - _vertRange_XMin, _vertRange_YMax - _vertRange_YMin);
			}
			else
			{
				for (int i = 0; i < _nVert; i++)
				{
					//_vertPositions_Local[i] = _transform.InverseTransformPoint(_vertPositions_Updated[i]);
					_vertPositions_Local[i] = _vertPositions_Updated[i];
				}
			}
			

			_mesh.vertices = _vertPositions_Local;
			_mesh.uv = _vertUVs;
			//추가3.22 : Flip 여부에 따라서 다른 Vertex 배열을 사용한다.
			if(_cal_isFlipped)
			{
				_mesh.triangles = _vertTris_Flipped;
			}
			else
			{
				_mesh.triangles = _vertTris;
				
			}
			
			if(_cal_isFlipped_Prev != _cal_isFlipped)
			{
				//Flip 여부가 바뀔 대
				//Normal을 다시 계산한다.
				_mesh.RecalculateNormals();
				_cal_isFlipped_Prev = _cal_isFlipped;
			}

			_mesh.RecalculateBounds();

		}



		#region [미사용 코드 : 스텐실 병합 방식]
		//public void RefreshClippedMesh()
		//{
		//	if (!_isMaskChild)
		//	{
		//		return;
		//	}
		//	apOptMesh targetMesh = null;
		//	int iVertOffset = 0;
		//	for (int i = 0; i < 2; i++)
		//	{

		//		if (i == 0)
		//		{
		//			targetMesh = _parentOptMesh;
		//			iVertOffset = 0;
		//		}
		//		else
		//		{
		//			targetMesh = this;
		//			iVertOffset = _nVertParent;
		//		}
		//		int nVert = targetMesh._nVert;
		//		for (int iVert = 0; iVert < nVert; iVert++)
		//		{
		//			_vertPosList_ClippedMerge[iVert + iVertOffset] =
		//				_transform.InverseTransformPoint(
		//					targetMesh._transform.TransformPoint(
		//						targetMesh._vertPositions_Updated[iVert]));
		//		}
		//	}


		//	_material.SetColor("_Color", MeshColor);
		//	_material.SetColor("_MaskColor", _parentOptMesh.MeshColor);
		//	_mesh.vertices = _vertPosList_ClippedMerge;



		//	_mesh.RecalculateBounds();
		//	_mesh.RecalculateNormals();

		//} 
		#endregion

		//--------------------------------------------------------------------------------
		// 클리핑 Mask Parent
		//--------------------------------------------------------------------------------
		//MaskParent일때, 커맨드 버퍼를 초기화한다.
		/// <summary>
		/// Clean up Command Buffers if it is Mask Parent
		/// </summary>
		public void CleanUpMaskParent()
		{
			if (!_isMaskParent)
			{
				return;
			}

			_isRenderTextureCreated = false;
			if (_targetCamera != null && _commandBuffer != null)
			{
				_targetCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, _commandBuffer);
			}
			_targetCamera = null;
			cameraTransform = null;
			_commandBuffer = null;

			_maskRenderTargetID = -1;
			if (_maskRenderTexture != null)
			{
				RenderTexture.ReleaseTemporary(_maskRenderTexture);
				_maskRenderTexture = null;
			}
		}

		private void RegistCommandBuffer()
		{
			if(!_isMaskParent)
			{
				CleanUpMaskParent();
				return;
			}

			//Debug.Log("RegistCommandBuffer [" + this.name + " - " + _isRenderTextureCreated + "]");
			if(_isRenderTextureCreated)
			{
				//이미 생성되었으면 포기
				//만약 다시 세팅하고 싶다면 RenderTexture 초기화 함수를 호출하자
				return;
			}

			

			if(_material == null || _materialAlphaMask == null || _mesh == null)
			{
				//재질 생성이 안되었다면 포기
				//Debug.LogError("No Init [" + this.name + "]");
				return;
			}
			
			Camera[] cameras = Camera.allCameras;
			if(cameras == null || cameras.Length == 0)
			{
				//Debug.LogError("NoCamera");
				return;
			}
			
			Camera targetCam = null;
			Camera cam = null;
			int layer = gameObject.layer;
			//이걸 바라보는 카메라가 하나 있으면 이걸로 설정.
			for (int i = 0; i < cameras.Length; i++)
			{
				cam = cameras[i];
				if(cam.cullingMask == (cam.cullingMask | (1 << gameObject.layer)))
				{
					//이 카메라는 이 객체를 바라본다.
					targetCam = cam;
					break;
				}
			}

			if(targetCam == null)
			{
				//잉?
				//유효한 카메라가 없네요.
				//Clean하고 초기화
				//Debug.LogError("NoCamera To Render");
				CleanUpMaskParent();
				return;
			}

			_targetCamera = targetCam;
			cameraTransform = _targetCamera.transform;
			
			
			if(_maskRenderTexture == null)
			{
				_maskRenderTexture = RenderTexture.GetTemporary(_clippingRenderTextureSize, _clippingRenderTextureSize, 24, RenderTextureFormat.Default);
				_maskRenderTargetID = new RenderTargetIdentifier(_maskRenderTexture);
			}

			_materialAlphaMask.SetTexture(_shaderID_MainTex, _material.mainTexture);
			_materialAlphaMask.SetColor(_shaderID_Color, _material.color);

			_commandBuffer = new CommandBuffer();
			_commandBuffer.name = "AP Clipping Mask [" + name + "]";
			_commandBuffer.SetRenderTarget(_maskRenderTargetID, 0);
			_commandBuffer.ClearRenderTarget(true, true, Color.clear);

			//일단은 기본값
			_vertPosCenter = Vector2.zero;
			//_vertRangeMax = -1.0f;

			_maskScreenSpaceOffset.x = 0;
			_maskScreenSpaceOffset.y = 0;
			_maskScreenSpaceOffset.z = 1;
			_maskScreenSpaceOffset.w = 1;



			_commandBuffer.DrawMesh(_mesh, transform.localToWorldMatrix, _materialAlphaMask);

			_targetCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _commandBuffer);

			//Debug.Log("[" + name + "] OptMesh Parent Make Render Texture");

			_isRenderTextureCreated = true;
		}

		private void UpdateCommandBuffer()
		{
			if (!_isVisible
				|| !_isMaskParent
				|| !_isRenderTextureCreated
				|| _commandBuffer == null
				|| _targetCamera == null)
			{
				return;
			}

			//현재 Mesh의 화면상의 위치를 체크하여 적절히 "예쁘게 찍히도록" 만든다.
			//크기 비율
			//여백을 조금 추가한다.
			_vertPosCenter.z = 0;

			_cal_localPos_LT = new Vector3(_vertRange_XMin, _vertRange_YMax, 0);
			_cal_localPos_RB = new Vector3(_vertRange_XMax, _vertRange_YMin, 0);


			_cal_vertWorldPos_Center = transform.TransformPoint(_vertPosCenter);

			_cal_vertWorldPos_LT = transform.TransformPoint(_cal_localPos_LT);
			_cal_vertWorldPos_RB = transform.TransformPoint(_cal_localPos_RB);

			_cal_screenPos_Center = _targetCamera.WorldToScreenPoint(_cal_vertWorldPos_Center);
			_cal_screenPos_LT = _targetCamera.WorldToScreenPoint(_cal_vertWorldPos_LT);
			_cal_screenPos_RB = _targetCamera.WorldToScreenPoint(_cal_vertWorldPos_RB);

			

			//모든 버텍스가 화면안에 들어온다면 Sceen 좌표계 Scale이 0~1의 값을 가진다.
			_cal_prevSizeWidth = Mathf.Abs(_cal_screenPos_LT.x - _cal_screenPos_RB.x) / (float)Screen.width;
			_cal_prevSizeHeight = Mathf.Abs(_cal_screenPos_LT.y - _cal_screenPos_RB.y) / (float)Screen.height;

			if (_cal_prevSizeWidth < 0.001f) { _cal_prevSizeWidth = 0.001f; }
			if (_cal_prevSizeHeight < 0.001f) { _cal_prevSizeHeight = 0.001f; }


			//화면에 가득 찰 수 있도록 확대하는 비율은 W, H 중에서 "덜 확대하는 비율"로 진행한다.
			_cal_zoomScale = Mathf.Min(1.0f / _cal_prevSizeWidth, 1.0f / _cal_prevSizeHeight);

			//메시 자체를 평행이동하여 화면 중앙에 위치시켜야 한다.

			//<<이거 속도 빠르게 하자
			_materialAlphaMask.SetTexture(_shaderID_MainTex, _material.mainTexture);
			_materialAlphaMask.SetColor(_shaderID_Color, _material.color);
			

			_cal_aspectRatio = (float)Screen.width / (float)Screen.height;
			_cal_newOrthoSize = _targetCamera.orthographicSize / _cal_zoomScale;

			_cal_centerMoveOffset = new Vector2(_cal_screenPos_Center.x - (Screen.width / 2), _cal_screenPos_Center.y - (Screen.height / 2));
			_cal_centerMoveOffset.x /= (float)Screen.width;
			_cal_centerMoveOffset.y /= (float)Screen.height;

			_cal_centerMoveOffset.x *= _cal_aspectRatio * _cal_newOrthoSize;
			_cal_centerMoveOffset.y *= _cal_newOrthoSize;

			//다음 카메라 위치는
			//카메라가 바라보는 Ray를 역으로 쐈을때 Center -> Ray*Dist 만큼의 위치
			_cal_distCenterToCamera = Vector3.Distance(_cal_vertWorldPos_Center, _targetCamera.transform.position);
			_cal_nextCameraPos = _cal_vertWorldPos_Center + _targetCamera.transform.forward * (-_cal_distCenterToCamera);
			//_cal_camOffset = _cal_vertWorldPos_Center - _targetCamera.transform.position;

			_cal_customWorldToCamera = Matrix4x4.TRS(_cal_nextCameraPos, cameraTransform.rotation, Vector3.one).inverse;
			_cal_customWorldToCamera.m20 *= -1f;
			_cal_customWorldToCamera.m21 *= -1f;
			_cal_customWorldToCamera.m22 *= -1f;
			_cal_customWorldToCamera.m23 *= -1f;

			// CullingMatrix = Projection * WorldToCamera
			_cal_customCullingMatrix = Matrix4x4.Ortho(	-_cal_aspectRatio * _cal_newOrthoSize,    //Left
													_cal_aspectRatio * _cal_newOrthoSize,     //Right
													-_cal_newOrthoSize,                  //Bottom
													_cal_newOrthoSize,                   //Top
													//_targetCamera.nearClipPlane, _targetCamera.farClipPlane
													_cal_distCenterToCamera - 10,        //Near
													_cal_distCenterToCamera + 50         //Far
													)
								* _cal_customWorldToCamera;

			
			_cal_newLocalToProjMatrix = _cal_customCullingMatrix * transform.localToWorldMatrix;
			_cal_newWorldMatrix = _targetCamera.cullingMatrix.inverse * _cal_newLocalToProjMatrix;
			
			_commandBuffer.Clear();
			_commandBuffer.SetRenderTarget(_maskRenderTargetID, 0);
			_commandBuffer.ClearRenderTarget(true, true, Color.clear);

			_commandBuffer.DrawMesh(_mesh, _cal_newWorldMatrix, _materialAlphaMask);
			
			//ScreenSpace가 얼마나 바뀌었는가
			_cal_screenPosOffset = new Vector3(Screen.width / 2, Screen.height / 2, 0) - _cal_screenPos_Center;
			
			_maskScreenSpaceOffset.x = (_cal_screenPosOffset.x / (float)Screen.width);
			_maskScreenSpaceOffset.y = (_cal_screenPosOffset.y / (float)Screen.height);
			_maskScreenSpaceOffset.z = _cal_zoomScale;
			_maskScreenSpaceOffset.w = _cal_zoomScale;

		}

		// Functions
		//------------------------------------------------
		/// <summary>
		/// Show Mesh
		/// </summary>
		/// <param name="isResetHideFlag"></param>
		public void Show(bool isResetHideFlag = false)
		{	
			if(isResetHideFlag)
			{
				_isHide_External = false;
			}
			_meshRenderer.enabled = true;
			_isVisible = true;

			if (_isMaskParent)
			{
				CleanUpMaskParent();

#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}
#endif

				RegistCommandBuffer();
			}
		}

		/// <summary>
		/// Hide Mesh
		/// </summary>
		public void Hide()
		{
			_meshRenderer.enabled = false;
			_isVisible = false;

			if (_isMaskParent)
			{
				CleanUpMaskParent();
			}
		}

		/// <summary>
		/// Show or Hide by default
		/// </summary>
		public void SetVisibleByDefault()
		{
			if(_isVisibleDefault)
			{
				Show(true);
			}
			else
			{
				Hide();
			}
		}

		/// <summary>
		/// Hide Mesh ignoring the result
		/// </summary>
		/// <param name="isHide"></param>
		public void SetHideForce(bool isHide)
		{
			_isHide_External = isHide;

			//실제 Visible 갱신은 다음 프레임의 업데이트때 수행된다.
		}

		



		//---------------------------------------------------------
		// Shader 제어 함수들
		//---------------------------------------------------------
		/// <summary>
		/// Set Main Color (2X)
		/// </summary>
		/// <param name="color2X"></param>
		public void SetMeshColor(Color color2X)
		{
			_multiplyColor = color2X;
			
			if(Mathf.Abs(_multiplyColor.r - 0.5f) < 0.001f &&
				Mathf.Abs(_multiplyColor.g - 0.5f) < 0.001f &&
				Mathf.Abs(_multiplyColor.b - 0.5f) < 0.001f &&
				Mathf.Abs(_multiplyColor.a - 1.0f) < 0.001f)
			{
				//기본 값이라면
				_isAnyMeshColorRequest = false;
				_multiplyColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			}
			else
			{
				_isAnyMeshColorRequest = true;
			}

			_cal_MeshColor.r = _multiplyColor.r * _parentTransform._meshColor2X.r * 2;
			_cal_MeshColor.g = _multiplyColor.g * _parentTransform._meshColor2X.g * 2;
			_cal_MeshColor.b = _multiplyColor.b * _parentTransform._meshColor2X.b * 2;
			_cal_MeshColor.a = _multiplyColor.a * _parentTransform._meshColor2X.a;//Alpha는 2X가 아니다.

			AutoSelectMaterial();
		}

		public void SetMeshAlpha(float alpha)
		{
			_multiplyColor.a = alpha;
			
			if(Mathf.Abs(_multiplyColor.r - 0.5f) < 0.001f &&
				Mathf.Abs(_multiplyColor.g - 0.5f) < 0.001f &&
				Mathf.Abs(_multiplyColor.b - 0.5f) < 0.001f &&
				Mathf.Abs(_multiplyColor.a - 1.0f) < 0.001f)
			{
				//기본 값이라면
				_isAnyMeshColorRequest = false;
				_multiplyColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			}
			else
			{
				_isAnyMeshColorRequest = true;
			}

			_cal_MeshColor.r = _multiplyColor.r * _parentTransform._meshColor2X.r * 2;
			_cal_MeshColor.g = _multiplyColor.g * _parentTransform._meshColor2X.g * 2;
			_cal_MeshColor.b = _multiplyColor.b * _parentTransform._meshColor2X.b * 2;
			_cal_MeshColor.a = _multiplyColor.a * _parentTransform._meshColor2X.a;//Alpha는 2X가 아니다.

			AutoSelectMaterial();
		}

		/// <summary>
		/// Set Main Texture
		/// </summary>
		/// <param name="texture"></param>
		public void SetMeshTexture(Texture2D texture)
		{
			if(_sharedMaterial.mainTexture == texture)
			{
				_isAnyTextureRequest = false;

				if(!_isUseSharedMaterial)
				{
					//일단 이미지를 넣어준다.
					_instanceMaterial.SetTexture(_shaderID_MainTex, texture);
				}
			}
			else
			{
				//새로운 텍스쳐를 요청했다.
				_isAnyTextureRequest = true;

				//Instance Material에 넣어준다.
				_instanceMaterial.SetTexture(_shaderID_MainTex, texture);
			}

			AutoSelectMaterial();
		}

		/// <summary>
		/// Set Color as shader property (not Main Color)
		/// </summary>
		/// <param name="color"></param>
		/// <param name="propertyName"></param>
		public void SetCustomColor(Color color, string propertyName)
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			_instanceMaterial.SetColor(propertyName, color);

			AutoSelectMaterial();
		}

		/// <summary>
		/// Set Alpha as shader property (not Main Color)
		/// </summary>
		/// <param name="color"></param>
		/// <param name="propertyName"></param>
		public void SetCustomAlpha(float alpha, string propertyName)
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			Color color = _instanceMaterial.GetColor(propertyName);
			color.a = alpha;
			_instanceMaterial.SetColor(propertyName, color);

			AutoSelectMaterial();
		}



		/// <summary>
		/// Set Texture as shader property (not Main Texture)
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="propertyName"></param>
		public void SetCustomTexture(Texture2D texture, string propertyName)
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			_instanceMaterial.SetTexture(propertyName, texture);

			AutoSelectMaterial();
		}

		/// <summary>
		/// Set Float Value as shader property
		/// </summary>
		/// <param name="floatValue"></param>
		/// <param name="propertyName"></param>
		public void SetCustomFloat(float floatValue, string propertyName)
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			_instanceMaterial.SetFloat(propertyName, floatValue);

			AutoSelectMaterial();
		}

		/// <summary>
		/// Set Int Value as shader property
		/// </summary>
		/// <param name="intValue"></param>
		/// <param name="propertyName"></param>
		public void SetCustomInt(int intValue, string propertyName)
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			_instanceMaterial.SetInt(propertyName, intValue);

			AutoSelectMaterial();
		}

		/// <summary>
		/// Set Vector4 Value as shader property
		/// </summary>
		/// <param name="vector4Value"></param>
		/// <param name="propertyName"></param>
		public void SetCustomVector4(Vector4 vector4Value, string propertyName)
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			_instanceMaterial.SetVector(propertyName, vector4Value);

			AutoSelectMaterial();
		}


		private void AutoSelectMaterial()
		{
			if(_isMaskChild)
			{	
				return;
			}
			if(_isAnyMeshColorRequest
				|| _isAnyTextureRequest
				|| _isAnyCustomPropertyRequest
				|| _parentTransform._isAnyColorCalculated
				|| !_isDefaultColorGray)
			{
				//Instance Material을 선택해야한다.
				if(_isUseSharedMaterial)
				{
					//Shared -> Instance
					_isUseSharedMaterial = false;
					_material = _instanceMaterial;

					_meshRenderer.material = _material;
				}
			}
			else
			{
				//Shared Material을 선택해야한다.
				if(!_isUseSharedMaterial)
				{
					//Instance -> Shared
					_isUseSharedMaterial = true;
					_material = _sharedMaterial;

					_meshRenderer.material = _material;
				}
			}
		}


		//Material Property 값들을 초기화한다.
		//이 함수를 호출하면 MaskChild를 제외하면 Batch를 위해 SharedMaterial로 변경된다.
		/// <summary>
		/// Return the material value to its initial state. Batch rendering is enabled.
		/// </summary>
		public void ResetMaterialToBatch()
		{
			if(_isMaskChild)
			{
				return;
			}
			if(_isUseSharedMaterial)
			{
				return;
			}
			_isUseSharedMaterial = true;
			_material = _sharedMaterial;

			//일단 InstanceMat도 복사를 해서 리셋을 해준다.
			_instanceMaterial.CopyPropertiesFromMaterial(_sharedMaterial);

			_isAnyMeshColorRequest = false;
			_isAnyTextureRequest = false;
			_isAnyCustomPropertyRequest = false;

			_meshRenderer.material = _material;
		}


		// Get / Set
		//------------------------------------------------
		/// <summary>
		/// Calculated Mesh Color (2X)
		/// </summary>
		public Color MeshColor
		{
			get
			{
				//return _multiplyColor * 2.0f * _parentTransform._meshColor2X;
				return _cal_MeshColor;
			}
		}

		//---------------------------------------------------------
		// Mesh Renderer 의 Sorting Order 제어
		//---------------------------------------------------------
		public void SetSortingLayer(string sortingLayerName, int sortingLayerID)
		{
			_meshRenderer.sortingLayerName = sortingLayerName;
			_meshRenderer.sortingLayerID = sortingLayerID;
		}

		public string GetSortingLayerName()
		{
			return _meshRenderer.sortingLayerName;
		}

		public int GetSortingLayerID()
		{
			return _meshRenderer.sortingLayerID;
		}

		public void SetSortingOrder(int sortingOrder)
		{
			_meshRenderer.sortingOrder = sortingOrder;
		}

		public int GetSortingOrder()
		{
			return _meshRenderer.sortingOrder;
		}
	}
}