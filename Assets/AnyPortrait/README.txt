------------------------------------------------------------
		AnyPortrait (Version 1.0.3)
------------------------------------------------------------


Thank you for using AnyPortrait.
AnyPortrait is an extension that helps you create 2D characters in Unity.
When you create a game, I hope that AnyPortrait will be a great help.

Here are some things to know before using AnyPortrait:


1. How to start

To use AnyPortrait, go to "Window > AnyPortrait > 2D Editor".
The work is done in the unit called Portrait.
You can create a new portrait or open an existing one.
For more information, please refer to the User Guide.



2. User Guide

The User's Guide is "AnyPortrait User Guide.pdf" in the Documentation folder.
This file contains two basic tutorials.

AnyPortrait has more features than that, so we recommend that you refer to the homepage.

Homepage with guides : https://www.rainyrizzle.com/



3. Languages

AnyPortrait supports 8 languages on a release basis.
(English, Korean, French, German, Spanish, Danish, Japanese, Chinese (Traditional / Simplified))

It is recommended to select the appropriate language from the Setting menu of AnyPortrait.

The homepage supports English and Korean.



4. Support

If you have any problems or problems with using AnyPortrait, please contact us.
You can also report the editor's typographical errors.
If you have the functionality you need, we will try to implement as much as possible.

You can contact us by using the web page or email us.

Report Page : 
https://www.rainyrizzle.com/anyportrait-report-eng (English)
https://www.rainyrizzle.com/anyportrait-report-kor (Korean)

EMail : contactrainyrizzle@gmail.com


Note: I would appreciate it if you acknowledge that it may take some time 
because there are not many developers on our team.



5. License

The license is written in the file "license.txt".
You can also check in "Setting > About" of AnyPortrait.



6. Target device and platform

AnyPortrait has been developed to support PC, mobile, web, and console.
Much has been optimized to be able to run in real games.
We have also made great efforts to ensure compatibility with graphical problems.


However, for practical reasons we can not actually test in all environments, there may be potential problems.
There may also be performance differences depending on your results.

Since we aim to run on any device in any case, 
please contact us for any issues that may be causing the problem.



7. Update Notes

1.0.1 (March 18, 2018)
- Added Italian and Polish.
- Supports Linear Color Space.
- You can change the texture asset setting in the editor.

1.0.2 (March 27, 2018)
- Fixed an issue where the bake could no longer be done with an error message if the mesh was modified after bake.
- Fixed an issue where the backup file could not be opened.
- Fixed a problem where rendering can not be done if Scale has negative value.
- Improved Modifier Lock function.
- Fixed an issue that the modifier is unlocked and multi-processing does not work properly.
- Added Sorting Layer / Order function. You can set it in the Bake dialog, Inspector.
- Sorting Layer / Order values ​​can be changed by script.
- If the target GameObject is Prefab, it is changed to apply automatically when Bake is done. This applies even if it is not Prefab Root.
- Fixed a bug in the number of error messages that users informed. Thank you.
- Fixed an error when importing a PSD file and a process failure.
- Fixed a problem where the shape of a character is distorted if Bake is continued.

1.0.3 (April 14, 2018)
- Significant improvements in Screen Capture
- Transparent color can be specified as background color (Except GIF animation)
- Added ability to save Sprite Sheet
- Screen capture Dialog is deleted and moved to the right screen to improve
- Support screen capture on Mac OSX
- Improved Physics Effects
- Corrected incorrectly calculated inertia when moving from outside
- Modify the gizmo to be inverted if the scale of the object is negative
- When replacing the texture of the mesh, Script Functions that can be replaced with an image registered in AnyPortrait has been added
- Fixed an issue that caused data errors to occur when undoing after creating or deleting objects
- Fixed a problem that when importing animation pose, data is missing while generating timeline automatically
- Fixed an issue where other vertices were selected when using the FFD tool
- Fixed an issue where vertex positions would be strange when undoing when using FFD tool
- Fixed an issue where the modifier did not recognize that the mesh was deleted, resulting in error code output
- Fixed an issue where the clipping mesh would not render properly if the Important option was turned off
- Fixed an issue where sub-mesh groups could not generate clipping meshes
- Fixed a problem where deleted mesh and mesh groups appeared as GameObjects
- Fixed a problem where the script does not change the texture of the mesh


------------------------------------------------------------
			한국어 설명
------------------------------------------------------------

AnyPortrait를 사용해주셔서 감사를 드립니다.
AnyPortrait는 2D 캐릭터를 유니티에서 직접 만들 수 있도록 개발된 확장 에디터입니다.
여러분이 게임을 만들 때, AnyPortrait가 많은 도움이 되기를 기대합니다.

아래는 AnyPortrait를 사용하기에 앞서서 알아두시면 좋을 내용입니다.


1. 시작하기

AnyPortrait를 실행하려면 "Window > AnyPortrait > 2D Editor"메뉴를 실행하시면 됩니다.
AnyPortrait는 Portrait라는 단위로 작업을 합니다.
새로운 Portrait를 만들거나 기존의 것을 여시면 됩니다.
더 많은 정보는 "사용 설명서"를 참고하시면 되겠습니다.



2. 사용 설명서

사용 설명서는 Documentation 폴더의 "AnyPortrait User Guide.pdf" 파일입니다.
이 문서에는 2개의 튜토리얼이 작성되어 있습니다.

AnyPortrait의 많은 기능을 사용하시려면 홈페이지를 참고하시길 권장합니다.

홈페이지 : https://www.rainyrizzle.com/



3. 언어

AnyPortrait는 출시를 기준으로 8개의 언어를 지원합니다.
(영어, 한국어, 프랑스어, 독일어, 스페인어, 덴마크어, 일본어, 중국어(번체/간체))

AnyPortrait의 설정 메뉴에서 언어를 선택할 수 있습니다.

홈페이지는 한국어와 영어를 지원합니다.



4. 고객 지원

AnyPortrait를 사용하시면서 겪은 문제점이나 개선할 점이 있다면, 저희에게 문의를 주시길 바랍니다.
에디터의 오탈자를 문의 주셔도 좋습니다.
추가적으로 구현되면 좋은 기능을 알려주신다면, 가능한 범위 내에서 구현을 하도록 노력하겠습니다.

문의는 홈페이지나 이메일로 주시면 됩니다.


문의 페이지 : 
https://www.rainyrizzle.com/anyportrait-report-eng (영어)
https://www.rainyrizzle.com/anyportrait-report-kor (한국어)

이메일 : contactrainyrizzle@gmail.com


참고: 저희 팀의 개발자가 많지 않아 처리에 시간이 걸릴 수 있으므로 양해부탁드립니다.



5. 저작권

AnyPortrait에 관련된 저작권은 "license.txt" 파일에 작성이 되어있습니다.
AnyPortrait의 "설정 > About"에서도 확인할 수 있습니다.



6. 대상 기기와 플랫폼

AnyPortrait는 PC, 모바일, 웹, 콘솔에서 구동되도록 개발되었습니다.
실제 게임에서 사용되도록 최적화 하였습니다.
그래픽적인 문제에 대한 높은 호환성을 가지도록 노력하였습니다.

그렇지만, 현실적인 이유로 모든 환경에서 테스트를 할 수 없었기에, 잠재적인 문제점이 있을 수 있습니다.
경우에 따라 사용자의 작업 결과물에 따라서 성능에 차이가 있을 수도 있습니다.

저희는 모든 기기에서 어떠한 경우라도 정상적으로 동작하는 것을 목표로 삼고 있기 때문에,
실행 과정에서 겪는 모든 이슈에 대해 연락을 주신다면 매우 감사하겠습니다.



7. 업데이트 노트

1.0.1 (2018년 3월 18일)
- 이탈리아어, 폴란드어를 추가하였습니다.
- Linear Color Space를 지원합니다.
- 에디터에서 텍스쳐 에셋 설정을 변경할 수 있습니다.

1.0.2 (2018년 3월 27일)
- Bake를 한 이후에 다시 메시를 수정한 경우, 에러 메시지와 함께 더이상 Bake를 할 수 없는 문제가 수정되었습니다.
- 백업 파일을 열 수 없는 문제를 수정하였습니다.
- Scale이 음수 값을 가지는 경우 렌더링이 안되는 문제를 수정하였습니다.
- 모디파이어 잠금(Modifier Lock) 기능을 개선하였습니다.
- 모디파이어 잠금을 해제하고 다중 처리시 제대로 결과가 나오지 않은 점을 수정하였습니다.
- Sorting Layer/Order 기능을 추가하였습니다. Bake 다이얼로그, Inspector에서 설정할 수 있습니다.
- Sorting Layer/Order 값을 스크립트를 이용하여 변경할 수 있습니다.
- 대상이 되는 GameObject가 Prefab인 경우, Bake를 하면 자동으로 Apply를 하도록 변경되었습니다. Prefab Root가 아니어도 적용됩니다.
- 사용자 분들이 알려주신 다수의 에러 메시지들에 대한 버그를 수정하였습니다. 감사합니다.
- PSD 파일을 가져올 때 발생하는 에러와 처리 실패 문제를 수정하였습니다.
- Bake를 계속할 경우 캐릭터의 형태가 왜곡되는 문제를 수정하였습니다.

1.0.3 (2018년 4월 14일)
- 화면 캡쳐 기능이 개선되었습니다.
- 투명색으로 배경으로 화면을 캡쳐하여 이미지로 저장할 수 있습니다. (GIF 제외)
- 스프라이트 시트(Sprite Sheet)로 저장할 수 있습니다.
- 화면 캡쳐 UI가 변경되었습니다.
- Mac OSX에서 화면 캡쳐 기능을 지원합니다.
- 물리 모디파이어가 수정되었습니다.
- 외부에서 위치를 수정할 경우 관성이 잘못 적용되는 문제가 수정되었습니다.
- 객체의 스케일이 음수인 경우 기즈모가 반전되어 나타나도록 수정했습니다.
- 메시의 텍스쳐를 교체할 때, AnyPortrait에 등록된 이미지를 사용할 수 있는 스크립트 함수가 추가되었습니다.
- 객체를 생성하거나 삭제한 이후 "실행 취소"를 할때 발생하는 오류를 수정하였습니다.
- 애니메이션 포즈를 Import하면서 자동으로 타임라인이 생성될 때 데이터가 누락되지 않도록 하였습니다.
- FFD 툴을 사용할 때 다른 버텍스가 선택되지 않도록 수정하였습니다.
- FFD 툴을 사용하고 "실행 취소"를 하면 버텍스의 위치가 이상해지는 문제가 수정되었습니다.
- 모디파이어가 삭제된 메시를 잘못 인식하여 발생시키는 에러를 수정하였습니다.
- Important 옵션이 꺼지면 클리핑 메시가 제대로 렌더링하지 못하는 문제가 수정되었습니다.
- 하위 메시 그룹에서 클리핑 메시를 생성할 수 없는 문제가 수정되었습니다.
- 삭제한 메시나 메시 그룹이 GameObject로 등장하는 문제가 수정되었습니다.
- 스크립트의 함수로 메시의 텍스쳐를 변경할때, 제대로 반영되지 않는 문제가 수정되었습니다.

