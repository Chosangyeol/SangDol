# 최종 보스 처치 후 엔딩 영상 연결

## 작업 목적

사용자가 추가한 `Assets/08. Sound/2026_10_18 00_56.mp4`를 최종 보스 처치 후 엔딩으로 재생한다.

## 분석

- 영상은 Unity VideoClip으로 정상 임포트되어 있으며 길이는 8초, 해상도는 854×480이다.
- `DungeonCompletionReturnToTitle`에 영상 → 기존 엔딩 Timeline → Title 복귀 흐름이 이미 구현되어 있지만 공통 종료 Prefab의 `endingVideo` 참조가 비어 있었다.
- 해당 종료 Prefab을 참조하는 던전은 Jester60Introduction이다. Circus-Main-Hall의 기존 `endingDirector` override는 유지한다.

## 수정 파일

- `Assets/03. Prefab/Dungeon/DungeonEnd_ReturnToTitle.prefab`
- `Assets/Tests/Editor/DungeonDepartureTests.cs`
- `Assets/Tests/PlayMode/DungeonDeparturePlayModeTests.cs`
- 본 DevLog. 사용자 추가 영상과 meta는 원래 위치에 유지한다.

## 수정 클래스 / 함수

- Production C# 수정 없음.
- `DungeonDepartureRuntimeCases.AssignedEndingVideoCompletesSkipsAndCancelsCleanly()` 추가.
- `DungeonDeparturePlayModeTests`에 위 테스트 실행 wrapper 추가.

## 주요 변경 내용

- 종료 Prefab의 `endingVideo`에 새 영상 GUID `1eab8bf75d3fc60419a778ca448268f3`을 연결했다. Unity 저장 시 기본 null `endingDirector`도 직렬화되며 실제 보스 씬의 기존 Director 참조는 정상 유지됨을 Editor에서 확인했다.
- 기존 사망 연출 후 대기 5초, 영상 8초, 기존 엔딩 Timeline 및 Title 복귀 순서를 사용한다.
- 실제 설정된 영상을 디코딩하여 자연 종료, 스킵, 재생 중 취소를 검증한다. timeScale=0에서도 진행하며 중복 완료 통지, 조작 잠금, 목표 UI 숨김 및 복원, Timeline 인계와 취소 정리를 확인한다.
- Scene 파일, 패키지, ProjectSettings, 기존 SerializeField 이름은 변경하지 않았다. 이전 작업의 수정사항은 보존했다.

## Public API 변경

Production API 변경 없음. 테스트 전용 public IEnumerator 메서드와 실행 wrapper를 추가했으며 기존 호출부 영향은 없다.

## Multi-Agent 작업

- 사용 여부: 미사용.
- Main Agent: 기존 슬롯 분석, Prefab 참조 연결, 회귀 테스트 작성, 실제 Unity 검증 및 변경 범위 검토.
- 기존 기능의 영상 참조 연결이므로 별도 Agent 없이 진행했다.
- 주요 자체 검토: 공통 Prefab 사용 범위, Scene Director override 보존, 실제 화면 RenderTexture 연결, 재생 및 취소 시 조작/UI 소유권 확인.

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

- Unity MCP로 Circus-Main-Hall을 Additive로 열어 실제 종료 컴포넌트의 새 VideoClip과 `DungeonEndingCutscene` Director 참조를 확인하고 저장 없이 닫았다.
- Main 씬의 활성 `AnimCutScenePlayer`에서 VideoPlayManager, VideoPlayer, RawImage 및 공유 RenderTexture 참조가 유효함을 확인했다.
- 검증 후 Title 씬을 유지한다. 마지막 상태 확인 시 play=False, compile=False, compileFailed=False, Title roots=7, dirty=False였다.

### Compile

Result: PASS

- Asset Refresh 요청 중 재연결 오류(`Failed ... after 10 retries`)가 1회 발생했다. 이후 MCP가 정상 응답했고 컴파일 완료 및 오류 없음, 추가한 테스트 탐색과 실행 성공을 실제 확인했다.

### Console

- Errors: 0 (최근 30분 MCP 조회).
- Exceptions: 0 (동일 조회).
- Warnings: 기존 테스트의 잘못된 입력 경고(제작 비용/공간 부족, 없는 아이템, 없는 영상/Title) 및 테스트 전후 NavMesh 없는 bootstrap 복원 경고가 관찰됐다. 새 영상 디코딩은 실제 자연 종료 테스트를 통과했다. 경고 0으로 보고하지 않는다.

## EditMode Tests

Result: PASS

Passed: 116 / Failed: 0 / Not Run: 0

전체 EditMode 테스트를 Unity MCP로 실행했다.

## PlayMode Tests

Result: PASS (관련 테스트 5개)

Passed: 5 / Failed: 0

- AssignedEndingVideoCompletesSkipsAndCancelsCleanly: PASS (실제 영상, 세 종료 경로).
- ReturnsToActualTitleAndCanStartFreshGame: PASS (기존 엔딩 Timeline 종료, 실제 Title 로드와 새 게임 시작).
- DisabledOrRemovedEndpointRestoresDefaultDeparture: PASS.
- MissingEndpointAndInvalidMenuUseDefaultDeparture: PASS.
- MovingAndReenablingEndpointKeepsOneSubscription: PASS.

모든 테스트는 Main Agent가 Unity MCP로 직접 실행했다. MCP Summary의 TotalTests=95는 발견된 전체 테스트 수이며, 필터별 실제 실행 결과는 각각 PassedTests=1이다. 위 다섯 메서드를 각각 실행했으며 전체 PlayMode 95개를 실행한 것은 아니다.

## 발견된 문제

Asset Refresh의 일시적 MCP 재연결 실패 및 기존 테스트 환경의 NavMesh 경고. 후속 연결, 컴파일, 관련 테스트는 정상 완료했다.

## 남은 문제

이번 요청 범위에서 확인된 미해결 문제 없음.

## 미검증 항목

- 보스를 직접 전투로 처치하는 수동 전체 플레이 및 빌드 실행은 NOT RUN.
- 전체 PlayMode suite는 NOT RUN. 영상 종료와 후속 Title 복귀는 별도의 관련 Runtime 테스트로 검증했다.

## 최종 결과

최종 보스 완료 시 새 영상이 기존 엔딩 흐름으로 연결된다. 실제 재생·스킵·취소, 후속 엔딩 및 Title 복귀 관련 테스트를 통과했다.
