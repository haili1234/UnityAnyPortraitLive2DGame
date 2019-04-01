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
	// MeshGroup에 포함되어 본 애니메이션을 가능하게 한다.
	// 계층적으로 참조가 가능하며, 여러개의 Root를 가질 수 있다.
	// Opt 버전은 MonoBehaviour로 정의가 가능하다.
	// 기본적인 TRS와 달리, S 왜곡이 없는 계산법을 사용한다. (Scale은 계층적으로 계산되지 않는다)
	/// <summary>
	/// It is a class for Bone animation.
	/// </summary>
	[Serializable]
	public class apBone
	{
		// Members
		//--------------------------------------------
		public string _name = "Bone";

		public int _uniqueID = -1;

		public int _meshGroupID = -1;

		[NonSerialized]
		public apMeshGroup _meshGroup = null;

		/// <summary>
		/// 이 Bone이 속한 MeshGroup이 실제 렌더링/업데이트 되는 RenderUnit을 저장한다.
		/// (자주 갱신된다)
		/// Root Bone은 이 RenderUnit의 Transform을 ParentWorldMatrix로 삼는다.
		/// </summary>
		[NonSerialized]
		public apRenderUnit _renderUnit = null;

		//ParentBone이 없으면 Root이다.
		//그 외에는 Parent Bone을 연결한다.
		public int _parentBoneID = -1;
		public int _level = 0;
		public int _recursiveIndex = 0;

		[NonSerialized]
		public apBone _parentBone = null;



		[SerializeField]
		public List<int> _childBoneIDs = new List<int>();

		[NonSerialized]
		public List<apBone> _childBones = new List<apBone>();

		//Transform은 +Y를 Up으로 삼아서 판단한다.

		// Transform Matrix
		//1) Local Default Transform Matrix [직렬화]
		// : Parent를 기준으로 Matrix가 계산된다. Root인 경우 MeshGroup내에서의 위치와 기본 변환값이 저장된다.

		//2) Local Modified Transform Matrix
		// : Animation, Modifier 등으로 변경된 Matrix이다. 변형값을 따로 가지고 있으며, Default TF Matrix의 값을 포함한다! (<- 중요)

		//2) World Matrix
		// : Parent의 World Matrix * Local Transform Matrix를 구하면 이 Bone의 World Matrix를 구할 수 있다. 매번 업데이트된다.

		[SerializeField]
		public apMatrix _defaultMatrix = new apMatrix();

		//실제로 중요한 값은 아니고,
		//작업 화면에서 겹쳐 보이는 것을 조절할 때 사용된다.
		//렌더링 순서는 Recursive 우선 방식. 같은 Level에서 비교할때만 Depth를 이용한다.
		//Depth가 큰게 나중에(위에) 렌더링
		[SerializeField]
		public int _depth = 0;

		

		//RigTestMatrix : 에디터에서만 정의되는 변수. Rigging 작업시 Test Posing이 켜져있으면 동작한다.
		[NonSerialized]
		public apMatrix _rigTestMatrix = new apMatrix();


		//LocalMatrix : default(Local) Matrix에서 TRS를 각각 적용한 Matrix (곱하기가 아니다)
		[NonSerialized]
		public apMatrix _localMatrix = new apMatrix();

		[NonSerialized]
		private Vector2 _deltaPos = Vector2.zero;

		[NonSerialized]
		private float _deltaAngle = 0.0f;

		[NonSerialized]
		private Vector2 _deltaScale = Vector2.one;

		[NonSerialized]
		public apMatrix _worldMatrix = new apMatrix();

		[NonSerialized]
		public apMatrix _worldMatrix_NonModified = new apMatrix();

		

		///// <summary>
		///// Parent Bone이나 속한 MeshGroup (Transform)의 World Matrix.
		///// 단순 참조 용이므로 외부로부터 환성된 WorldMatrix를 받아야한다.
		///// </summary>
		//[NonSerialized]
		//private apMatrix _parentMatrix = null;

		// 제어 정보
		// IK 여부
		//1) Child가 하나인 Bone은 편집시에 IK가 1단계 적용이 된다.
		//2) /// TODO : IK는 나중에 합시다

		// UI에 보이는 정보
		//1) 색상 (이걸로 나중에 Weight를 표현한다)
		//2) 모양 (폭, 기본 길이(Scale 1일때의 길이), 뾰족한 정도 % (Taper : 100일때 뾰족, 0일때 직사각형))
		//- 모양에 따라서 GUI에서 클릭 처리 영역이 달라진다.
		//- 말단 노드의 경우 Length를 0으로 만들 수 있다.

		[SerializeField]
		public Color _color = Color.white;
		public int _shapeWidth = 30;
		public int _shapeLength = 50;//<<이 값은 생성할 때 Child와의 거리로 판단한다.
		public int _shapeTaper = 100;//기본값은 뾰족

		public bool _shapeHelper = false;//<<추가 : Helper 속성이 True이면 꼬리가 안보인다. Length는 유지한다.

		//GUI에 표시하기 위한 포인트
		//            [Mid1] (-x)                    [End1] (-x, +y)
		//              |
		// [Start]     <Length - 20%>     ------->   [End] (+y -> )
		//              |
		//            [Mid2] (+x)                    [End2] (+x, +y)
		public Vector2 _shapePoint_End = Vector2.zero;

		public Vector2 _shapePoint_Mid1 = Vector2.zero;
		public Vector2 _shapePoint_Mid2 = Vector2.zero;
		public Vector2 _shapePoint_End1 = Vector2.zero;
		public Vector2 _shapePoint_End2 = Vector2.zero;

		[NonSerialized]
		private bool _isBoneGUIVisible = true;//<<화면상에 숨길 수 있다. 어디까지나 임시. 메뉴를 열면 다시 True가 된다.

		//[NonSerialized]
		//private bool _isBoneGUIVisible_Parent = true;//<<Parent가 출력 중인가

		// 본 옵션
		/// <summary>
		/// Parent Bone으로부터 결정된 기본 Position을 변경할 수 있는가?
		/// Parent의 IK 옵션에 따라 불가능할 수 있다.
		/// </summary>
		public enum OPTION_LOCAL_MOVE
		{
			/// <summary>
			/// 기본값. Local Move가 불가능하다.
			/// Default 제어를 제외하고는 IK로 처리되거나 아예 처리되지 않는다.
			/// </summary>
			Disabled = 0,
			/// <summary>
			/// Local Move가 가능하다. 
			/// 단, Parent의 OPTION_IK가 Disabled이거나 자신을 가리키면 허용되지 않는다.
			/// </summary>
			Enabled = 1
		}

		/// <summary>
		/// IK가 설정되어 있는가. 
		/// 자신을 기준으로 Tail 방향으로 값을 설정한다. 기본값은 IKSingle
		/// </summary>
		public enum OPTION_IK
		{
			/// <summary>IK를 처리하지 않고 FK로만 처리한다.</summary>
			Disabled = 0,

			/// <summary>
			/// 기본값. Child 본 1개에 대해서 IK로 처리한다.
			/// IK Head와 동일하지만 Chain 처리가 없다는 점에서 구분된다.
			/// </summary>
			IKSingle = 1,

			/// <summary>
			/// Chain된 IK 본의 시작부분이다.
			/// Head 이후의 Tail 까지의 본들은 자동으로 Chained 상태가 된다.
			/// </summary>
			IKHead = 2,

			/// <summary>
			/// Head-Tail IK 설정에 의해 강제로 IK가 설정되는 옵션.
			/// 하위에는 Tail이 존재하고, 상위에 Head가 존재한다.
			/// </summary>
			IKChained = 3
		}

		public OPTION_LOCAL_MOVE _optionLocalMove = OPTION_LOCAL_MOVE.Disabled;
		public OPTION_IK _optionIK = OPTION_IK.IKSingle;

		/// <summary>
		/// Parent로부터 IK의 대상이 되는가? IK Single일 때에도 Tail이 된다.
		/// (자신이 IK를 설정하는 것과는 무관함)
		/// </summary>
		public bool _isIKTail = false;

		//IK의 타겟과 Parent
		public int _IKTargetBoneID = -1;
		[NonSerialized]
		public apBone _IKTargetBone = null;

		/// <summary>
		/// IK target이 설정된 경우, IK Target을 포함하고 있는 Child Bone의 ID.
		/// 빠른 검색과 Chain 처리 목적
		/// </summary>
		public int _IKNextChainedBoneID = -1;

		[NonSerialized]
		public apBone _IKNextChainedBone = null;

		/// <summary>
		/// IK Tail이거나 IK Chained 상태라면 Header를 저장하고, Chaining 처리를 해야한다.
		/// </summary>
		public int _IKHeaderBoneID = -1;

		[NonSerialized]
		public apBone _IKHeaderBone = null;

		//IK가 설정된 후, Tail, Tail의 Head 처리는 Chain Refresh때 해주자


		//IK시 추가 옵션

		/// <summary>IK 적용시, 각도를 제한을 줄 것인가 (기본값 False)</summary>
		public bool _isIKAngleRange = false;
		public float _IKAngleRange_Lower = -90.0f;//음수여야 한다. => Upper보다 작지만 -360 ~ 360 각도를 가질 수 있다.
		public float _IKAngleRange_Upper = 90.0f;//양수여야 한다.
		public float _IKAnglePreferred = 0.0f;//선호하는 각도 Offset


		/// <summary>IK 연산이 되었는가</summary>
		[NonSerialized]
		public bool _isIKCalculated = false;

		/// <summary>IK 연산이 발생했을 경우, World 좌표계에서 Angle을 어떻게 만들어야 하는지 계산 결과값</summary>
		[NonSerialized]
		public float _IKRequestAngleResult = 0.0f;

		///// <summary>
		///// IK Bone Chaining이 설정되었을 때의, Tail -> Head로의 리스트.
		///// 자기 자신은 포함하지 않는다. (자기 자신은 IK 각도 조절에서 제외되므로..)
		///// 리스트의 마지막에 Head가 들어온다.
		///// </summary>
		//[NonSerialized]
		//public List<apBone> _IKChainedBones = new List<apBone>();

		/// <summary>
		/// IK 계산을 해주는 Chain Set.
		/// </summary>
		private apBoneIKChainSet _IKChainSet = null;



		//에디터 변수 : Rigging Test가 작동중인가 (기본적으론 False)
		public bool _isRigTestPosing = false;

		//옵션
		public bool _isSocketEnabled = false;//런타임에서 소켓을 활성화할 것인가 (기본값 false)



		//에디터 변수
		//추가 3.22
		//Mod Lock 실행 여부를 저장한다.
		public enum EX_CALCULATE
		{
			/// <summary>기본 상태</summary>
			Normal,
			/// <summary>Ex Edit 상태 중 "선택된 Modifier"에 포함된 상태</summary>
			ExAdded,
			/// <summary>Ex Edit 상태 중, "선택된 Modifier"에 포함되지 않은 상태</summary>
			ExNotAdded
		}

		[NonSerialized]
		public EX_CALCULATE _exCalculateMode = EX_CALCULATE.Normal;

		// Init
		//--------------------------------------------
		/// <summary>
		/// 백업용 생성자.
		/// </summary>
		public apBone()
		{

		}


		public apBone(int uniqueID, int meshGroupID, string name)
		{
			//_name = "Bone " + uniqueID;
			_name = name;
			_uniqueID = uniqueID;
			_meshGroupID = meshGroupID;

			_childBoneIDs.Clear();
			_childBones.Clear();

			MakeRandomColor();
		}



		/// <summary>
		/// 각종 옵션을 초기화한다.
		/// 링크한 후에 처리할 것
		/// 단일 Bone의 옵션 초기화시에는 호출가능하지만 일괄 초기화 시에는 호출하지 말자
		/// </summary>
		public void InitOption()
		{
			_optionLocalMove = OPTION_LOCAL_MOVE.Disabled;

			_optionIK = OPTION_IK.IKSingle;

			_isIKTail = false;

			if (_parentBoneID == -1)
			{
				//Parent가 없다면..
				//IK Tail은 해제하고 Local Move는 활성화한다.
				_optionLocalMove = OPTION_LOCAL_MOVE.Enabled;
			}

			//Child가 하나 있다면
			if (_childBoneIDs.Count == 1)
			{
				//IK Single로 설정한다.
				_optionIK = OPTION_IK.IKSingle;
				_IKTargetBoneID = _childBoneIDs[0];
				_IKTargetBone = _childBones[0];

				_IKNextChainedBoneID = _IKTargetBoneID;
				_IKNextChainedBone = _IKTargetBone;
			}
			else
			{
				//Child가 없거나 그 이상이라면
				//기본값으로는 IK를 해제한다.
				_optionIK = OPTION_IK.Disabled;
				_IKTargetBoneID = -1;
				_IKTargetBone = null;

				_IKNextChainedBoneID = -1;
				_IKNextChainedBone = null;
			}

			_IKHeaderBoneID = -1;
			_IKHeaderBone = null;

			if (_parentBone != null)
			{
				if (_parentBone._optionIK != OPTION_IK.Disabled &&
					_parentBone._IKNextChainedBoneID == _uniqueID)
				{
					//이 Bone을 대상으로 IK가 설정되어있다.
					if (_parentBone._optionIK == OPTION_IK.IKSingle ||
						_parentBone._optionIK == OPTION_IK.IKHead)
					{
						//Parent가 Head 또는 Single이라면
						_IKHeaderBone = _parentBone;
						_IKHeaderBoneID = _parentBoneID;
					}
					else if (_parentBone._optionIK == OPTION_IK.IKChained)
					{
						//Parent가 Chained 상태라면 Header를 찾아야한다.
						_IKHeaderBone = _parentBone._IKHeaderBone;
						_IKHeaderBoneID = _parentBone._IKHeaderBoneID;
					}

					_isIKTail = true;
				}
			}
		}

		public void MakeRandomColor()
		{
			float red = UnityEngine.Random.Range(0.2f, 1.0f);
			float green = UnityEngine.Random.Range(0.2f, 1.0f);
			float blue = UnityEngine.Random.Range(0.2f, 1.0f);
			float totalWeight = red + green + blue;
			if (totalWeight < 0.5f)
			{
				red *= 0.5f / totalWeight;
				green *= 0.5f / totalWeight;
				blue *= 0.5f / totalWeight;
			}

			_color = new Color(red, green, blue, 1.0f);
		}

		/// <summary>
		/// Link를 해준다. 소속되는 MeshGroup은 물론이고, ParentBone도 링크해준다.
		/// ParentBone이 있다면 해당 Bone의 Child 리스트에 이 Bone을 체크 후 추가한다.
		/// (Link는 Child -> Parent 로 참조를 한다. Add와 반대)
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="parentBone"></param>
		public void Link(apMeshGroup meshGroup, apBone parentBone)
		{
			_meshGroup = meshGroup;
			_parentBone = parentBone;



			if (_childBones == null)
			{
				_childBones = new List<apBone>();
			}


			//일단 ID가 있는 Bone들은 meshGroup으로부터 값을 받아서 연결을 해준다.
			if (_IKTargetBoneID >= 0)
			{
				_IKTargetBone = _meshGroup.GetBone(_IKTargetBoneID);
				if (_IKTargetBone == null)
				{
					_IKTargetBoneID = -1;
				}
			}

			if (_IKNextChainedBoneID >= 0)
			{
				_IKNextChainedBone = _meshGroup.GetBone(_IKNextChainedBoneID);
				if (_IKNextChainedBone == null)
				{
					_IKNextChainedBoneID = -1;
				}
			}

			if (_IKHeaderBoneID >= 0)
			{
				_IKHeaderBone = _meshGroup.GetBone(_IKHeaderBoneID);
				if (_IKHeaderBone == null)
				{
					_IKHeaderBoneID = -1;
				}
			}

			if (_parentBone != null)
			{
				//호출 순서가 잘못되어서 parent의 List가 초기화가 안된 경우가 있다.
				if (_parentBone._childBones == null)
				{
					_parentBone._childBones = new List<apBone>();
				}

				if (!_parentBone._childBones.Contains(this))
				{
					_parentBone._childBones.Add(this);
				}
				if (!_parentBone._childBoneIDs.Contains(_uniqueID))
				{
					_parentBone._childBoneIDs.Add(_uniqueID);
				}
				_parentBoneID = _parentBone._uniqueID;
			}
			else
			{
				_parentBone = null;
				_parentBoneID = -1;
			}

			if (_rigTestMatrix == null)
			{
				_rigTestMatrix = new apMatrix();
			}
			_isRigTestPosing = false;

			if (_localMatrix == null)
			{
				_localMatrix = new apMatrix();
			}

			if (_worldMatrix == null)
			{
				_worldMatrix = new apMatrix();
			}

			if (_worldMatrix_NonModified == null)
			{
				_worldMatrix_NonModified = new apMatrix();
			}

			//if(_invWorldMatrix == null)
			//{
			//	_invWorldMatrix = new apMatrix();
			//}

			//if(_invWorldMatrix_NonModified == null)
			//{
			//	_invWorldMatrix_NonModified = new apMatrix();
			//}

			//Default Angle이 제대로 적용되지 않는 경우가 있다.
			//Default는 -180 ~ 180 안에 들어간다
			_defaultMatrix._angleDeg = apUtil.AngleTo180(_defaultMatrix._angleDeg);
		}

		/// <summary>
		/// 실제로 렌더링되는 RenderUnit을 넣어준다.
		/// RenderUnit의 Transform이 가장 기본이 되는 Root World Matrix다.
		/// </summary>
		/// <param name="renderUnit"></param>
		public void SetParentRenderUnit(apRenderUnit renderUnit)
		{
			_renderUnit = renderUnit;
		}

		/// <summary>
		/// Link시 Parent -> Child 순서로 호출하는 초기화 함수
		/// WorldMatrix의 레퍼런스를 전달해준다.
		/// </summary>
		public void LinkRecursive(int curLevel)
		{
			ReadyToUpdate(false);
			//if(_parentBone != null)
			//{
			//	SetParentMatrix(_parentBone._worldMatrix);
			//}
			if (_parentBone != null)
			{
				_renderUnit = _parentBone._renderUnit;
			}

			_level = curLevel;


			for (int i = 0; i < _childBones.Count; i++)
			{
				_childBones[i].LinkRecursive(curLevel + 1);
			}
		}
		public int SetBoneIndex(int index)
		{

			_recursiveIndex = index;

			int result = index;
			if (_childBones.Count > 0)
			{
				for (int i = 0; i < _childBones.Count; i++)
				{
					result = _childBones[i].SetBoneIndex(result + 1);
				}
			}
			return result;
		}



		/// <summary>
		/// Bone Chaining 직후에 재귀적으로 호출한다.
		/// Tail이 가지는 -> Head로의 IK 리스트를 만든다.
		/// 
		/// </summary>
		public void LinkBoneChaining()
		{


			if (_isIKTail)
			{
				//if(_IKChainedBones == null)
				//{
				//	_IKChainedBones = new List<apBone>();
				//}
				//_IKChainedBones.Clear();

				apBone curParentBone = _parentBone;
				apBone headBone = _IKHeaderBone;

				bool isParentExist = (curParentBone != null);
				bool isHeaderExist = (headBone != null);
				bool isHeaderIsInParents = false;
				if (isParentExist && isHeaderExist)
				{
					isHeaderIsInParents = (GetParentRecursive(headBone._uniqueID) != null);
				}


				if (isParentExist && isHeaderExist && isHeaderIsInParents)
				{
					if (_IKChainSet == null)
					{
						_IKChainSet = new apBoneIKChainSet(this);
					}
					//Chain을 Refresh한다.
					_IKChainSet.RefreshChain();

					//while(true)
					//{
					//	//Parent를 위로 탐색하면서 Chain을 연결하자
					//	_IKChainedBones.Add(curParentBone);
					//	if(curParentBone == headBone)
					//	{
					//		//도착
					//		break;
					//	}

					//	//하나씩 위로 올라가자
					//	curParentBone = curParentBone._parentBone;
					//	if(curParentBone == null)
					//	{
					//		//..? IK가 이상하게 연결되었는데요?
					//		Debug.LogError("[" + _name + "] IK Chaining Error : Parent -> Header 사이에서 중간에 끊어졌다.");
					//		break;
					//	}
					//}

				}
				else
				{
					_IKChainSet = null;

					Debug.LogError("[" + _name + "] IK Chaining Error : Parent -> Chain List 연결시 데이터가 누락되었다. "
						+ "[ Parent : " + isParentExist
						+ " / Header : " + isHeaderExist
						+ " / IsHeader Is In Parent : " + isHeaderIsInParents + " ]");
				}
			}
			else
			{
				_IKChainSet = null;
			}

			for (int i = 0; i < _childBones.Count; i++)
			{
				_childBones[i].LinkBoneChaining();
			}

		}

		/// <summary>
		/// Transform을 모두 초기화한다. Default 포함 (업데이트때는 호출하지 말것)
		/// </summary>
		public void InitTransform()
		{
			_defaultMatrix.SetIdentity();

			if (_localMatrix == null)
			{
				_localMatrix = new apMatrix();
			}
			_localMatrix.SetIdentity();

			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;
			if (_worldMatrix == null)
			{
				_worldMatrix = new apMatrix();
			}
			if (_worldMatrix_NonModified == null)
			{
				_worldMatrix_NonModified = new apMatrix();
			}

			//if(_invWorldMatrix == null)
			//{
			//	_invWorldMatrix = new apMatrix();
			//}

			//if(_invWorldMatrix_NonModified == null)
			//{
			//	_invWorldMatrix_NonModified = new apMatrix();
			//}

			_worldMatrix.SetIdentity();
			_worldMatrix_NonModified.SetIdentity();
			//_invWorldMatrix.SetIdentity();
			//_invWorldMatrix_NonModified.SetIdentity();

		}




		// Update
		//-----------------------------------------------------
		/// <summary>
		/// 1) Update Transform Matrix를 초기화한다.
		/// </summary>
		public void ReadyToUpdate(bool isRecursive)
		{
			//_localModifiedTransformMatrix.SetIdentity();

			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;

			_isIKCalculated = false;
			_IKRequestAngleResult = 0.0f;

			//_worldMatrix.SetIdentity();
			if (isRecursive)
			{
				for (int i = 0; i < _childBones.Count; i++)
				{
					_childBones[i].ReadyToUpdate(true);
				}
			}



		}

		/// <summary>
		/// 2) Update된 TRS 값을 넣는다.
		/// </summary>
		/// <param name="deltaPos"></param>
		/// <param name="deltaAngle"></param>
		/// <param name="deltaScale"></param>
		public void UpdateModifiedValue(Vector2 deltaPos, float deltaAngle, Vector2 deltaScale)
		{
			_deltaPos = deltaPos;
			_deltaAngle = deltaAngle;
			_deltaScale = deltaScale;
		}


		public void AddIKAngle(float IKAngle)
		{
			_isIKCalculated = true;
			_IKRequestAngleResult += IKAngle;
		}

		///// <summary>
		///// 초기화/3) Parent Matrix를 넣는다. 바뀐게 없다면 더 호출하지 않아도 된다.
		///// (에디터에서 메뉴에서 선택시, 처음 초기화시 호출해야함)
		///// </summary>
		///// <param name="parentMatrix"></param>
		//public void SetParentMatrix(apMatrix parentMatrix)
		//{
		//	_parentMatrix = parentMatrix;
		//}

		/// <summary>
		/// 4) World Matrix를 만든다.
		/// 이 함수는 Parent의 MeshGroupTransform이 연산된 후 -> Vertex가 연산되기 전에 호출되어야 한다.
		/// </summary>
		public void MakeWorldMatrix(bool isRecursive)
		{
			_localMatrix.SetIdentity();
			_localMatrix._pos = _deltaPos;
			_localMatrix._angleDeg = _deltaAngle;
			_localMatrix._scale.x = _deltaScale.x;
			_localMatrix._scale.y = _deltaScale.y;

			_localMatrix.MakeMatrix();

			//World Matrix = ParentMatrix x LocalMatrix
			//Root인 경우에는 MeshGroup의 Matrix를 이용하자

			//_invWorldMatrix_NonModified.SetIdentity();

			if (_parentBone == null)
			{
				_worldMatrix.SetMatrix(_defaultMatrix);
				_worldMatrix_NonModified.SetMatrix(_defaultMatrix);
				if (_isRigTestPosing)
				{
					_worldMatrix.Add(_rigTestMatrix);//Rig Test 추가
				}
				_worldMatrix.Add(_localMatrix);

				if (_renderUnit != null)
				{
					//Non Modified도 동일하게 적용
					//렌더유닛의 WorldMatrix를 넣어주자
					//_worldMatrix.RMultiply(_renderUnit._meshGroup._rootMeshGroupTransform._matrix);
					_worldMatrix.RMultiply(_renderUnit.WorldMatrixWrap);
					_worldMatrix_NonModified.RMultiply(_renderUnit.WorldMatrixWrapWithoutModified);
				}
			}
			else
			{
				_worldMatrix.SetMatrix(_defaultMatrix);
				_worldMatrix_NonModified.SetMatrix(_defaultMatrix);

				if (_isRigTestPosing)
				{
					_worldMatrix.Add(_rigTestMatrix);//Rig Test 추가
				}
				_worldMatrix.Add(_localMatrix);
				_worldMatrix.RMultiply(_parentBone._worldMatrix);

				//주의, Parent의 NonModifiedMatrix를 적용할 것
				_worldMatrix_NonModified.RMultiply(_parentBone._worldMatrix_NonModified);

				//Inverse를 적용하자
				//_invWorldMatrix.SetMatrix(_parentBone._invWorldMatrix);
				//_invWorldMatrix_NonModified.SetMatrix(_parentBone._invWorldMatrix_NonModified);

				//_invWorldMatrix_NonModified.SetIdentity();
				//_invWorldMatrix_NonModified.RInverse(_parentBone._invWorldMatrix_NonModified);
			}

			////Inverse 이어서 작업
			//_invWorldMatrix.RInverse(_localMatrix);
			//if(_isRigTestPosing)
			//{
			//	_invWorldMatrix.Subtract(_rigTestMatrix);
			//}

			//_invWorldMatrix.Subtract(_defaultMatrix);
			//_invWorldMatrix_NonModified.RInverse(_defaultMatrix);
			////_invWorldMatrix_NonModified.Subtract(_defaultMatrix);

			//if (string.Equals(_name, "Bone 2 Debug"))
			//{
			//	//디버그를 해보자
			//	Debug.Log("------- Bone Matrix [" + _name + "] ------- (Editor)");
			//	Debug.Log("Default Matrix [" + _defaultMatrix.ToString() + "]");
			//	Debug.Log("Local Matrix [" + _localMatrix.ToString() + "]");
			//	if (_parentBone != null)
			//	{
			//		Debug.Log("Parent(" + _parentBone._name + ")");
			//		Debug.Log(">> World Matrix [" + _parentBone._worldMatrix.ToString() + "]");
			//		Debug.Log(">> World Matrix No Mod [" + _parentBone._worldMatrix_NonModified.ToString() + "]");
			//	}
			//	Debug.Log("World Matrix [" + _worldMatrix.ToString() + "]");
			//	Debug.Log("World Matrix No Mod [" + _worldMatrix_NonModified.ToString() + "]");
			//	Debug.Log("-----------------------------------------");
			//}

			//Child도 호출해준다.
			if (isRecursive)
			{
				for (int i = 0; i < _childBones.Count; i++)
				{
					_childBones[i].MakeWorldMatrix(true);
				}
			}
		}


		/// <summary>
		/// 5) GUI 편집시에는 이 함수까지 호출한다. GUI용 Bone 이미지 데이터가 갱신된다.
		/// 이 부분은 Render 부분에서 호출해도 된다.
		/// </summary>
		public void GUIUpdate(bool isRecursive = false)
		{
			//Shape의 Local좌표를 설정하고, WorldMatrix를 적용한다.
			_shapePoint_End.x = 0;
			_shapePoint_End.y = _shapeLength;


			_shapePoint_Mid1.x = -_shapeWidth * 0.5f;
			_shapePoint_Mid1.y = _shapeLength * 0.2f;

			_shapePoint_Mid2 = new Vector2(_shapeWidth * 0.5f, _shapeLength * 0.2f);

			float taperRatio = Mathf.Clamp01((float)(100 - _shapeTaper) / 100.0f);

			_shapePoint_End1 = new Vector3(-_shapeWidth * 0.5f * taperRatio, _shapeLength, 0.0f);
			_shapePoint_End2 = new Vector3(_shapeWidth * 0.5f * taperRatio, _shapeLength, 0.0f);

			_shapePoint_End = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End);
			_shapePoint_Mid1 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_Mid1);
			_shapePoint_Mid2 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_Mid2);
			_shapePoint_End1 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End1);
			_shapePoint_End2 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End2);

			if (isRecursive)
			{
				for (int i = 0; i < _childBones.Count; i++)
				{
					_childBones[i].GUIUpdate(true);
				}
			}
		}

		// Matrix 연산
		//----------------------------------------------------------------
		//public Vector3 LocalToWorld(Vector3 pos, bool isModified)
		//{

		//}

		// Functions
		//--------------------------------------------
		/// <summary>
		/// 다른 Bone을 Child로 둔다.
		/// Child->Parent 연결도 자동으로 수행한다.
		/// </summary>
		/// <param name="bone"></param>
		public bool AddChildBone(apBone bone)
		{
			if (bone == null)
			{
				return false;
			}
			if (bone._meshGroup != _meshGroup)
			{
				//다른 MeshGroup에 속해있다면 실패
				return false;
			}

			int boneID = bone._uniqueID;
			if (!_childBoneIDs.Contains(boneID))
			{
				_childBoneIDs.Add(boneID);
			}

			if (!_childBones.Contains(bone))
			{
				_childBones.Add(bone);
			}

			bone._parentBone = this;
			bone._parentBoneID = _uniqueID;

			return true;
		}

		/// <summary>
		/// Child Bone 하나를 제외한다.
		/// </summary>
		/// <param name="bone"></param>
		/// <returns></returns>
		public bool ReleaseChildBone(apBone bone)
		{
			if (bone == null)
			{
				return false;
			}

			int boneID = bone._uniqueID;
			_childBoneIDs.Remove(boneID);
			_childBones.Remove(bone);

			//Parent 연결도 끊어준다.
			bone._parentBone = null;
			bone._parentBoneID = -1;

			return true;
		}


		public void ReleaseAllChildBones(apBone bone)
		{
			List<apBone> tmpChildBones = new List<apBone>();
			for (int i = 0; i < _childBones.Count; i++)
			{
				tmpChildBones.Add(_childBones[i]);
			}

			for (int i = 0; i < tmpChildBones.Count; i++)
			{
				ReleaseChildBone(tmpChildBones[i]);
			}

			_childBones.Clear();
			_childBoneIDs.Clear();

		}

		// Get / Set
		//--------------------------------------------
		/// <summary>
		/// boneID를 가지는 Bone을 자식 노드로 두고 있는가.
		/// 재귀적으로 찾는다.
		/// </summary>
		/// <param name="boneID"></param>
		/// <returns></returns>
		public apBone GetChildBoneRecursive(int boneID)
		{
			//바로 아래의 자식 노드를 검색
			for (int i = 0; i < _childBones.Count; i++)
			{
				if (_childBones[i]._uniqueID == boneID)
				{
					return _childBones[i];
				}
			}

			//못찾았다면..
			//재귀적으로 검색해보자

			for (int i = 0; i < _childBones.Count; i++)
			{
				apBone result = _childBones[i].GetChildBoneRecursive(boneID);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		/// <summary>
		/// 바로 아래의 자식 Bone을 검색한다.
		/// </summary>
		/// <param name="boneID"></param>
		/// <returns></returns>
		public apBone GetChildBone(int boneID)
		{
			//바로 아래의 자식 노드를 검색
			for (int i = 0; i < _childBones.Count; i++)
			{
				if (_childBones[i]._uniqueID == boneID)
				{
					return _childBones[i];
				}
			}

			return null;
		}

		/// <summary>
		/// 자식 Bone 중에서 특정 Target Bone을 재귀적인 자식으로 가지는 시작 Bone을 찾는다.
		/// </summary>
		/// <param name="targetBoneID"></param>
		/// <returns></returns>
		public apBone FindNextChainedBone(int targetBoneID)
		{
			//바로 아래의 자식 노드를 검색
			for (int i = 0; i < _childBones.Count; i++)
			{
				if (_childBones[i]._uniqueID == targetBoneID)
				{
					return _childBones[i];
				}
			}

			//못찾았다면..
			//재귀적으로 검색해서, 그 중에 실제로 Target Bone을 포함하는 Child Bone을 리턴하자

			for (int i = 0; i < _childBones.Count; i++)
			{
				apBone result = _childBones[i].GetChildBoneRecursive(targetBoneID);
				if (result != null)
				{
					//return result;
					return _childBones[i];//<<Result가 아니라, ChildBone을 리턴
				}
			}
			return null;
		}

		/// <summary>
		/// 요청한 boneID를 가지는 Bone을 부모 노드로 두고 있는가.
		/// 재귀적으로 찾는다.
		/// </summary>
		/// <param name="boneID"></param>
		/// <returns></returns>
		public apBone GetParentRecursive(int boneID)
		{
			if (_parentBone == null)
			{
				return null;
			}

			if (_parentBone._uniqueID == boneID)
			{
				return _parentBone;
			}

			//재귀적으로 검색해보자
			return _parentBone.GetParentRecursive(boneID);

		}


		public void SetRiggingTest(bool isRiggingTest)
		{
			_isRigTestPosing = isRiggingTest;
		}

		public void ResetRiggingTestPose()
		{
			_rigTestMatrix.SetIdentity();
		}

		//-------------------------------------------------------------------
		/// <summary>
		/// Bone을 기준으로 해당 위치를 바라보는 각도를 구한다.
		/// Bone의 현재 각도는 포함하지 않고 전체 각도만 계산하므로 따로 빼주어야 한다.
		/// Bone은 +Y로 향하므로 거기에 맞게 각도를 조절한다.
		/// 좌표계는 동일해야한다.
		/// </summary>
		/// <param name="targetPos"></param>
		/// <param name="bonePos"></param>
		/// <param name="prevAngle">연산 전의 결과값. LookAt 실패시 이 값을 리턴한다.</param>
		/// <returns></returns>
		public static float GetLookAtAngle(Vector2 targetPos, Vector2 bonePos, float prevAngle)
		{
			//두 점이 너무 가까우면 LookAt을 할 수 없다.
			if (Mathf.Abs(targetPos.y - bonePos.y) < 0.0001f && Mathf.Abs(targetPos.x - bonePos.x) < 0.0001f)
			{
				return prevAngle;
			}

			float angle = Mathf.Atan2(targetPos.y - bonePos.y, targetPos.x - bonePos.x) * Mathf.Rad2Deg;
			//angle += 90.0f;
			//angle += 180.0f;

			angle -= 90.0f;
			if (angle > 180.0f)
			{
				angle -= 360.0f;
			}
			else if (angle < -180.0f)
			{
				angle += 360.0f;
			}

			return angle;
		}


		//public bool _isIKtargetDebug = false;
		//public Vector2 _calculatedIKTargetPosDebug = Vector2.zero;
		//public List<Vector2> _calculatedIKBonePosDebug = new List<Vector2>();

		/// <summary>
		/// IK 요청을 한다. World 좌표계에서 얼마나 각도를 더 변경해야하는지 값이 변수로 저장된다.
		/// IK Chain의 Tail에서 호출해야한다. 연산 순서는 Tail -> Parent
		/// </summary>
		/// <param name="targetPosW">현재 Bone부터 </param>
		/// <param name="weight">IK가 적용되는 정도. 0~1</param>
		public bool RequestIK(Vector2 targetPosW, float weight, bool isContinuous)
		{
			if (!_isIKTail || _IKChainSet == null)
			{
				//Debug.LogError("[" + _name + "] Request IK Failed : Is Tail : " + _isIKTail + " / Chain Set Exist : " + (_IKChainSet != null));
				//_isIKtargetDebug = false;
				return false;
			}


			//Debug.Log("Request IK [" + _name + "] / Target PosW : " + targetPosW);
			bool isSuccess = _IKChainSet.SimulateIK(targetPosW, isContinuous);
			//IK가 실패하면 패스
			if (!isSuccess)
			{
				//Debug.LogError("[" + _name + "] Request IK Failed Calculate : " + targetPosW);
				//_isIKtargetDebug = false;
				return false;
			}

			//IK 결과값을 Bone에 넣어주자
			_IKChainSet.AdaptIKResultToBones(weight);

			//Debug.Log("[" + _name + "] Request IK Success : " + targetPosW);
			return true;
		}



		/// <summary>
		/// RequestIK의 제한된 버전
		/// limitedBones에 포함된 Bone으로만 IK를 만들어야한다.
		/// Chain을 검색해서 포함된 것의 Head를 검색해서 IK를 처리한다.
		/// RequestIK와 달리 "마지막으로 Head처럼 처리된 Bone"을 리턴한다.
		/// 실패시 null리턴
		/// </summary>
		/// <param name="targetPosW"></param>
		/// <param name="weight"></param>
		/// <param name="isContinuous"></param>
		/// <param name="limitedBones"></param>
		/// <returns></returns>
		public apBone RequestIK_Limited(Vector2 targetPosW, float weight, bool isContinuous, List<apBone> limitedBones)
		{
			if (!_isIKTail || _IKChainSet == null)
			{
				return null;
			}

			apBoneIKChainUnit lastCheckChain = null;
			apBoneIKChainUnit curCheckChain = null;
			//[Tail : 0] .... [Head : Count - 1]이므로
			//앞부터 갱신하면서 Head쪽으로 가는 가장 마지막 레퍼런스를 찾으면 된다.
			for (int i = 0; i < _IKChainSet._chainUnits.Count; i++)
			{
				curCheckChain = _IKChainSet._chainUnits[i];
				if (limitedBones.Contains(curCheckChain._baseBone))
				{
					//이건 포함된 BoneUnit이다.
					lastCheckChain = curCheckChain;
				}
				else
				{
					break;
				}
			}
			//잉... 하나도 해당 안되는데용..
			if (lastCheckChain == null)
			{
				return null;
			}


			//Debug.Log("Request IK [" + _name + "] / Target PosW : " + targetPosW);
			//bool isSuccess = _IKChainSet.SimulateIK(targetPosW, isContinuous);
			bool isSuccess = _IKChainSet.SimulateIK_Limited(targetPosW, isContinuous, lastCheckChain);
			//IK가 실패하면 패스
			if (!isSuccess)
			{
				//Debug.LogError("[" + _name + "] Request IK Failed Calculate : " + targetPosW);
				//_isIKtargetDebug = false;
				return null;
			}

			//IK 결과값을 Bone에 넣어주자
			_IKChainSet.AdaptIKResultToBones(weight);

			//Debug.Log("[" + _name + "] Request IK Success : " + targetPosW);
			//return true;
			return lastCheckChain._baseBone;
		}



		//--------------------------------------------------------------------------------------------------
		public bool IsGUIVisible
		{
			get
			{
				//return _isBoneGUIVisible && _isBoneGUIVisible_Parent;
				return _isBoneGUIVisible;
			}
		}

		/// <summary>
		/// Bone의 GUI상의 Visible을 Root로부터 Child로 갱신을 한다.
		/// GUI가 바뀌었을때 Root에서 호출을 한다.
		/// </summary>
		public void RefreshGUIVisibleRecursive()
		{
			//if(_parentBone != null)
			//{
			//	_isBoneGUIVisible_Parent = _parentBone.IsGUIVisible;
			//}
			//else
			//{
			//	_isBoneGUIVisible_Parent = true;
			//}

			if(_childBones.Count > 0)
			{
				for (int i = 0; i < _childBones.Count; i++)
				{
					_childBones[i].RefreshGUIVisibleRecursive();
				}
			}
		}

		/// <summary>
		/// Bone의 GUI상의 Visible을 리셋한다. 숨겨진 모든 본을 보이게 한다.
		/// </summary>
		public void ResetGUIVisibleRecursive()
		{
			_isBoneGUIVisible = true;

			//if(_parentBone != null)
			//{
			//	_isBoneGUIVisible_Parent = _parentBone.IsGUIVisible;
			//}
			//else
			//{
			//	_isBoneGUIVisible_Parent = true;
			//}

			if(_childBones.Count > 0)
			{
				for (int i = 0; i < _childBones.Count; i++)
				{
					_childBones[i].ResetGUIVisibleRecursive();
				}
			}
		}
		
		/// <summary>
		/// Bone의 GUI Visible을 지정한다.
		/// </summary>
		/// <param name="isVisible"></param>
		/// <param name="isRecursive"></param>
		public void SetGUIVisible(bool isVisible, bool isRecursive)
		{
			_isBoneGUIVisible = isVisible;

			//if(_parentBone != null)
			//{
			//	_isBoneGUIVisible_Parent = _parentBone.IsGUIVisible;
			//}
			//else
			//{
			//	_isBoneGUIVisible_Parent = true;
			//}

			if (isRecursive)
			{
				if (_childBones.Count > 0)
				{
					for (int i = 0; i < _childBones.Count; i++)
					{
						_childBones[i].RefreshGUIVisibleRecursive();
					}
				}
			}
		}

		public void SetGUIVisibleWithExceptBone(bool isVisible, bool isRecursive, apBone exceptBone)
		{
			if(exceptBone == this)
			{
				_isBoneGUIVisible = !isVisible;//<<반대로 적용
			}
			else
			{
				_isBoneGUIVisible = isVisible;
			}

			//if(_parentBone != null)
			//{
			//	_isBoneGUIVisible_Parent = _parentBone.IsGUIVisible;
			//}
			//else
			//{
			//	_isBoneGUIVisible_Parent = true;
			//}

			if (isRecursive)
			{
				if (_childBones.Count > 0)
				{
					for (int i = 0; i < _childBones.Count; i++)
					{
						_childBones[i].SetGUIVisibleWithExceptBone(isVisible, isRecursive, exceptBone);
					}
				}
			}
		}
	}

}