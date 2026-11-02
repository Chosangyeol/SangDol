# WebGL 제출 화면과 동영상 재생 대응

## 작업 목적

1920×1080 기준으로 제작한 게임을 960×600 제출 영역에서 비율을 유지해 표시하고, WebGL에서 타이틀·보스·워프·엔딩 영상을 URL로 재생한다.

## 분석

- Unity 2022.3.62f3의 기존 WebGL 기본 크기는 960×600이었다. 제작 기준 16:9와 제출 영역 16:10의 비율이 달랐다.
- 타이틀과 UIManager의 화면 Canvas가 Constant Pixel Size여서 작은 창에서 UI가 함께 축소되지 않았다.
- VideoPlayer와 공용 VideoPlayManager가 임베디드 VideoClip을 사용했다. WebGL에서는 영상 원본의 HTTP URL 재생이 필요하다.
- 기존 Serialize 참조와 PC 재생은 보존하고, WebGL용 상대 경로·길이·오디오 트랙 수를 별도로 저장했다.

## 수정 파일

- Assets/02. Scripts/UI/VideoPlayManager.cs
- Assets/02. Scripts/UI/WebVideoSource.cs (신규)
- Assets/02. Scripts/UI/WebVideoPlayerSource.cs (신규)
- Assets/02. Scripts/Enemy/BossModel.cs
- Assets/02. Scripts/Enemy/Dungeon/DungeonManager.cs
- Assets/02. Scripts/Enemy/Dungeon/DungeonCompletionReturnToTitle.cs
- Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss.cs
- Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss60Presentation.cs
- Assets/Editor/WebSubmissionBuild.cs (신규)
- Assets/WebGLTemplates/Submission/index.html (신규)
- Assets/StreamingAssets/Videos/ (영상 원본 복사본 및 meta 신규)
- Assets/01. Scene/Title.unity
- Assets/01. Scene/Circus-Main-Hall.unity
- Assets/01. Scene/MainTest.unity
- Assets/03. Prefab/Manager/UIManager.prefab
- Assets/03. Prefab/Dungeon/DungeonEnd_ReturnToTitle.prefab
- Assets/03. Prefab/Dungeon/Jester60Intro/Jester60Introduction.prefab
- Assets/03. Prefab/Enemy/Boss/D1/Final/Jester.prefab
- Assets/Jester-S/CutScene.prefab
- ProjectSettings/ProjectSettings.asset (WebGL 템플릿 선택)
- Assets/Tests/Editor/WebSubmissionTests.cs (신규)
- Assets/Tests/EditMode/WebSubmissionEditModeTests.cs (신규)

새 Unity 에셋에는 Unity가 생성한 meta를 함께 유지한다. 작업 전부터 있던 다른 변경은 되돌리지 않았다.

Unity 작업 중 미수정 Material 2개의 legacy _Color가 _BaseColor 값으로 약 10^-7 단위 자동 정규화된 것을 최종 diff에서 확인했다. MiniClown/clown_teeth_mat.mat과 Orc/Monster.mat의 해당 1줄씩만 작업 전 값으로 복원해 변경 범위에서 제외했다. 배포 파일에는 Unity 빌드 당시의 자동 정규화 값이 들어갈 수 있으며 8bit 색상 차이는 없다. 코드/영상/화면 배치 검증에 영향 없는 부동소수점 직렬화 차이이므로 이 때문에 전체 빌드와 테스트를 반복하지 않았다.

## 수정 클래스 / 함수

- VideoPlayManager: TryPlayVideoSource(), WatchPlayback(), ReleaseFrame(). 기존 TryPlayVideo()/PlayVideo() 유지.
- WebVideoSource: HasVideo(), Configure(), BuildUrl().
- WebVideoPlayerSource: Awake(), Update().
- WebSubmissionBuild: Prepare(), BuildWebGL(), OnPreprocessBuild(), OnProcessScene(), PrepareRoot(), WriteSource().
- BossModel.DefeatSequence(), DungeonManager.WarpSequence(), DungeonCompletionReturnToTitle.PlayEnding(): WebGL용 소스를 함께 전달.
- D1_FinalBoss.Special_MiddleBoss(), D1_FinalBoss60Presentation.BindBoss()/OnDisable(): 웹 경로 전달 및 풀링 바인딩 복원.

## 주요 변경 내용

### 화면

- 외곽 영역 960×600, 게임 Canvas 960×540. 중앙 정렬로 상하 30px 여백을 둔다.
- 창 크기 변경과 전체 화면에서도 16:9 비율을 유지한다.
- devicePixelRatio=1로 표시 영역과 렌더 크기를 맞춘다.
- 타이틀/UIManager CanvasScaler를 Scale With Screen Size, 기준 1920×1080, Match 0.5로 설정했다.
- 이 구성은 960×600을 제출 컨테이너 크기로 해석한다. 규정이 실제 게임 영역 자체를 960×600으로 채우도록 요구하면 16:10 구도 대응이 별도로 필요하다.

### 영상

- 원본 GUID를 파일명으로 사용해 공백·한글·동일 파일명 충돌을 피한다.
- StreamingAssets/Videos에 원본 파일을 복사하고 WebGL에서는 Application.streamingAssetsPath 기반 URL을 사용한다. 별도 영상 서비스가 필요하지 않다.
- PC/Editor에서는 기존 VideoClip을 우선 사용한다.
- 보스·엔딩의 clip!=null 선행 조건도 WebGL 소스를 인식하도록 수정했다.
- 빌드 씬 복사본에서는 VideoClip 참조를 제거하되, authoring Scene/Prefab 원본의 참조는 유지한다. 런타임 프리팹은 사전 저장한 웹 경로를 사용한다.
- 타이틀 같은 독립 VideoPlayer에는 WebGL 전용 bridge를 연결한다. 소리가 있는 자동 재생 영상은 무음으로 시작하고 입력 후 음소거를 해제한다. 현재 타이틀 영상에는 음성 트랙이 없다.
- URL 영상도 기존 세션 소유권, 스킵·실패·취소, 마지막 프레임 유지, 던전 목표 UI 복원 흐름을 사용한다.
- 준비 완료 후 실제 재생 길이를 반영하도록 watchdog을 보완했다.
- Prepare 메뉴 및 WebGL 빌드 전처리에서 메타데이터를 갱신한다. 클립 제거 시 이전 경로도 초기화한다.
- Timeline VideoClip 참조는 현재 에셋에서 발견되지 않아 DefaultPlayables 라이브러리는 수정하지 않았다.

## Public API 변경

- 기존 public 메서드·필드·클래스 이름과 SerializeField 이름 삭제/변경 없음.
- 신규 MonoBehaviour WebVideoPlayerSource, Editor 전용 WebSubmissionBuild 및 Prepare()/BuildWebGL() 메뉴 진입점 추가.
- 기존 PlayVideo(VideoClip), TryPlayVideo(VideoClip,out int) 유지. 프로젝트 내부 호출부는 companion을 전달하는 internal TryPlayVideoSource()를 사용한다.

## Multi-Agent 작업

- 사용 여부: 사용.
- Main Agent: 설계, 모든 파일 수정과 통합, Unity MCP 컴파일·Console·테스트·빌드 검증.
- Review Agent: Serialize/API 보존, 영상 null 가드, 바인딩 복원, URL/타임아웃, 빌드와 템플릿 검토. 파일 수정 없음.
- Test/QA Agent: 기존 테스트 구조, 회귀 우선순위, WebGL 전용 코드와 브라우저 검증의 공백 검토. 파일 수정/Unity 테스트 실행 없음.
- 반영: 클립 제거 후 이전 웹 경로 초기화, 빌드 복사본과 원본 보존 테스트, 오디오 트랙 수 저장.
- 독립 에이전트의 제안과 Main의 실제 실행 결과를 구분했다.

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

Main이 Unity MCP로 확인: 초기 Title 씬 로드, dirty=false, 루트 7개, Play Mode=false. 타이틀 VideoPlayer의 VideoClip 소스 및 CanvasScaler 원래 설정 확인. 수정 후 Prepare 및 관련 Scene/Prefab 저장 수행. WebGL 대상으로 전환 완료.

빌드 후 최종 MCP 재확인: WebGL 대상, compiling=false, scriptCompilationFailed=false, playing=false. Title 씬 dirty=false, 루트 7개 유지. 타이틀 원본 VideoClip 참조 보존, WebVideoPlayerSource bridge 존재. CanvasScaler=ScaleWithScreenSize, 기준 1920×1080, 템플릿 PROJECT:Submission 확인.

### Compile

Result: PASS

Asset Refresh 후 Editor 컴파일 완료 및 scriptCompilationFailed=false 확인. WebGL 플랫폼 전환 후에도 compiling=false, scriptCompilationFailed=false, 해당 구간 Console Error 0 확인.

### Console

- 기존 미사용 이벤트/필드 컴파일 Warning 8건 확인.
- 테스트에서 의도한 누락 영상/timeout 경고, 테스트 씬 AudioListener 부재 로그가 있다.
- MCP 클래스 필터가 네임스페이스 없는 fixture를 못 찾는 도구 오류 3건이 있었다. 도구 소스에서 패턴을 확인한 뒤 어셈블리 필터로 실제 테스트를 실행했다. 게임 컴파일/런타임 오류와 구분한다.
- WebGL BuildReport: Errors=0, Warnings=3.
- 빌드 후 최근 Console Error=0, Exception=0 (Main MCP 조회).
- 빌드 경고 3개는 이번에 수정하지 않은 tree11_LOD1의 Nature/Soft Occlusion shader 안내 1건과, MCP InputSystem/Timeline 패키지의 빈 Runtime asmdef 안내 2건이다. 패키지나 해당 나무 에셋은 변경하지 않았다.

## EditMode Tests

Result: PASS

- PlayerMovement.EditModeTests 어셈블리: 최종 104 Passed, 0 Failed, 0 Skipped. Unity MCP 실제 실행.
- 신규 5개: URL escaping/상위 경로 차단, imported clip 없는 URL 설정, 원본과 복사 영상/메타데이터 일치, 빌드 복사본 URL 보존과 원본 clip 보존, 클립 제거 시 경로 초기화.
- MCP TotalTests=121은 suite 노드를 포함하므로 leaf Passed=104와 구분한다.

## PlayMode Tests

Result: PASS

- PlayerMovement.PlayModeTests 어셈블리: 95 Passed, 0 Failed, 0 Skipped, 376.0879669초.
- 영상 복구·세션·프레임 유지·보스/엔딩 전환·실제 게임 진행 회귀 포함.
- MCP 호출은 300초에서 응답 timeout이 발생했지만 Unity 실행은 계속됐다. Main이 Unity TestRunnerApi 완료 콜백을 연결해 전체 RunFinished XML과 요약을 직접 수집했다.
- 근거: Temp/WebSubmissionPlayModeResults.xml, Temp/WebSubmissionPlayModeSummary.txt.
- Editor 전용 테스트 결과이며 WebGL 브라우저 영상 재생 성공을 대신하지 않는다.

## 추가 검증

- Node로 템플릿 스크립트의 5개 화면 크기/비율 계산 케이스 PASS: 960×600, 1920×1080, 좁은 창, 낮은 창, 넓은 창.
- 배포 영상 4개의 MP4 sample description을 읽어 avc1(H.264), 오디오가 있는 2개는 mp4a 확인. H.264 High profile, level 3.0/3.1/4.1.
- 영상 복사본은 원본 바이트 그대로 유지한다. 재인코딩은 하지 않았다.

## WebGL Build

Result: PASS

- Unity BuildPipeline.BuildPlayer 결과 Succeeded, Errors=0, Warnings=3.
- 출력: Build/WebSubmission/index.html, Build/, StreamingAssets/Videos/.
- BuildReport 전체 파일 크기: 885,116,965 bytes (약 885MB).
- WebSubmission.data.br: 856,907,541 bytes, 압축 해제 1,002,175,016 bytes.
- WebSubmission.wasm.br: 9,136,174 bytes, 압축 해제 50,152,488 bytes.
- WebSubmission.framework.js.br: 74,015 bytes, 압축 해제 430,795 bytes.
- Main이 Node 스트림으로 3개 Brotli 파일을 끝까지 압축 해제하고 UnityWebData/Wasm 헤더를 검사: PASS.
- 생성 HTML의 JavaScript 구문, macro 치환 완료, 참조 파일 존재/비어 있지 않음: PASS.
- 빌드에 포함된 영상 4개의 SHA-256 원본 일치: PASS.
- 생성된 최종 HTML에 대해서도 화면 비율 계산 5개 재실행: PASS.
- 큰 기존 데이터와 Brotli 압축으로 실제 빌드 대기가 길었다. BuildReport가 반환한 totalTime은 파일에 원문 보관하며, 그 값으로 전체 대기 시간을 단정하지 않는다.
- 제출용 ZIP: Build/WebSubmission_2026-10-20.zip, 884,970,211 bytes (약 885MB).
- ZIP 검증 PASS: 파일 9개, 루트 index.html 존재, ZIP 내부 모든 파일을 스트림으로 읽어 빌드 원본과 SHA-256/길이 일치 확인.
- 검증 근거 원문: 2026-10-20_WebSubmission_PlayMode.xml, 2026-10-20_WebSubmission_Build.txt, 2026-10-20_WebSubmission_Artifacts.json, 2026-10-20_WebSubmission_Zip.json (이 DevLog와 같은 폴더).

## 발견된 문제

- MCP의 class 필터 패턴은 네임스페이스 없는 테스트 이름과 맞지 않는다. 프로젝트 테스트나 패키지는 변경하지 않고 assembly 필터로 우회했다.
- 현재 세션 브라우저 inventory가 비어 있고 Chrome 생성도 Browser is not available로 반환된다.
- 기존 데이터가 크다. 최종 WebGL 데이터 압축 해제 크기가 약 1GB이며, 이번 작업에서는 씬 제외·텍스처 일괄 변경 등 별도 용량 최적화는 수행하지 않았다.
- ZIP 검사 PowerShell 파일 실행이 시스템 ExecutionPolicy로 차단되어 실행하지 못했다. 정책을 변경하지 않고 기존 Node 환경의 읽기 전용 검사로 ZIP 전체 검증을 완료했다.

## 남은 문제 / 미검증 항목

- 실제 제출 사이트에서 브라우저 영상 디코딩·소리·완료·스킵 동작: NOT VERIFIED (연결된 브라우저 없음).
- 브라우저의 실제 UI 클릭 위치, resize/fullscreen 화면, 자동 재생 정책: NOT VERIFIED. 계산 검사와 Editor 테스트만 수행했다.
- 제출 규정이 960×600 외곽 영역인지 실제 게임 영역인지 원문 확인하지 못했다. 현재 구현은 외곽 영역 960×600 + 내부 16:9 보존이다.

## 사용 방법

- Unity 메뉴 Tools > Web Submission > Prepare Videos and Screen: 영상과 화면 설정 갱신.
- Tools > Web Submission > Build WebGL: enabled Build Settings 씬으로 Build/WebSubmission 생성.
- 배포 시 index.html, Build, StreamingAssets를 함께 포함한다. file:// 더블클릭이 아닌 HTTP(S) 호스팅에서 실행해야 한다.
- 기존 Brotli 압축 설정을 유지했다. 호스팅은 .br 파일에 Content-Encoding: br, WebAssembly에 Content-Type: application/wasm을 제공해야 한다. 실제 제출 호스팅의 응답 헤더는 이번 세션에서 확인하지 못했다.

## 최종 결과

화면/영상 대응 코드와 에셋 설정을 수정했고 Editor/WebGL 컴파일, EditMode 104개, PlayMode 95개, 화면 계산 5개, 실제 WebGL 빌드 및 압축/영상/ZIP 파일 검사를 통과했다. 제출용 ZIP 생성을 완료했다. 실제 제출 브라우저의 재생·소리·UI 시각 검증은 미완료다.
