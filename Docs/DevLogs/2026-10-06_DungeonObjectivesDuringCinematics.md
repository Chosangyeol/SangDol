# 영상·컷씬 동안 던전 목표 UI 숨김

## 작업 목적

영상 앞에 던전 목표 패널이 표시되는 문제를 수정하고, 컷씬 동안에도 해당 패널을 숨긴다. 연출 종료·스킵·취소 시에는 이전 표시 상태 또는 연출 중 갱신된 목표 표시 요청을 복원한다.

---

## 분석

DungeonManager.dungeonUI는 UIManager의 일반 HUD 숨김 대상과 별개다. UpdateDungeonUI()에서도 패널을 직접 활성화하므로 연출 중 섹터 진행 상태가 갱신되면 목표가 다시 나타날 수 있었다.

영상은 완료·스킵 후에도 페이드 전환을 위해 마지막 프레임을 유지한다. Timeline 역시 종료 직전 Pause 후 페이드하고 Stop하므로 재생 상태가 Playing이 아니라는 이유만으로 목표를 복원하면 안 된다.

---

## 수정 파일

- Assets/02. Scripts/Enemy/Dungeon/DungeonManager.cs
- Assets/02. Scripts/UI/VideoPlayManager.cs
- Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss.cs
- Assets/02. Scripts/Enemy/Dungeon/SectorController.cs
- Assets/Tests/Editor/VideoRecoveryTests.cs
- Assets/Tests/EditMode/VideoRecoveryEditModeTests.cs
- Assets/Tests/PlayMode/VideoRecoveryPlayModeTests.cs
- Assets/Tests/Editor/Jester60IntroductionTests.cs

Scene/Prefab/Package 변경 없음. 이전 작업의 미커밋 변경은 되돌리지 않았다.

---

## 수정 클래스 / 함수

### DungeonManager

- SuppressObjectives(), RestoreObjectives(): 소유자별 숨김 요청 및 마지막 요청 해제 시 복원.
- PlayCutscene(): 기존 Timeline 재생을 연결하고 played/stopped 이벤트로 목표 숨김을 관리.
- LateUpdate(): 비활성화·파괴된 연출 소유자의 잔여 요청 정리.
- ClearObjectiveSuppression(), OnDisable(): 이벤트 구독과 숨김 상태 정리.
- SetObjectivesVisible(), Start(), UpdateDungeonUI(): 연출 중에는 목표 내용만 갱신하며 표시 요청을 저장.

### VideoPlayManager

- TryPlayVideo(): 영상 이미지 활성화 전에 현재 DungeonManager의 목표를 숨김.
- ReleaseFrame(): 시작 당시 DungeonManager의 숨김 요청 해제. held frame은 완료·스킵 후에도 해제 전까지 숨김 유지.

### D1_FinalBoss / SectorController

- PlaySpecial3Timeline(), Special_Chess(), PlayCutsceneSequence(): 기존 Play 호출을 DungeonManager.PlayCutscene으로 연결. 던전 매니저가 없으면 기존 Director.Play 사용.

---

## 주요 변경 내용

- 일반 HUD 및 NPC 대화 표시 이벤트를 추가 변경하지 않고 던전 목표 패널에만 적용했다.
- 여러 연출이 겹치면 마지막 숨김 요청이 끝날 때 복원한다. 같은 소유자의 중복 요청·해제는 안전하다.
- 영상 완료·스킵 후 마지막 화면 유지, Timeline Pause 동안도 숨김을 유지한다.
- 실패·취소·영상 비활성화·Timeline Stop/비활성화/파괴 및 던전 매니저 종료 시 정리한다.
- 처음부터 숨겨진 목표 UI를 연출 종료만으로 켜지 않는다. 연출 중 정상적인 UpdateDungeonUI 표시 요청이 발생하면 끝난 후 최신 내용으로 표시한다.
- 재생할 Timeline 자산이 없거나 컴포넌트가 비활성인 경우 숨김 요청을 만들지 않는다.

---

## Public API 변경

없음. internal 메서드와 private 런타임 필드만 추가했다. 기존 public/Serialize 필드 이름·Scene/Prefab 참조 유지.

---

## Multi-Agent 작업

- 사용 여부: 사용
- Main Agent 역할: 코드·테스트 작성, 변경 통합, Unity MCP 컴파일·Console·테스트 실행 및 기록.
- Review Agent: 읽기 전용 구현·호출 경로·구독 수명·Serialize/API 검토.
- Test/QA Agent: 읽기 전용 중첩·취소·일시정지·held frame·목표 갱신 테스트 전략.
- 주요 지적사항 및 반영: null Timeline의 영구 숨김 방지 사전 검사 추가. Pause와 held frame을 완료로 오인하지 않도록 유지하고, 중복/중첩 및 원래 숨김 상태를 테스트했다.
- 직접 테스트 실행 책임은 Main Agent가 수행했다. 서브에이전트는 테스트를 실행했다고 보고하지 않았다.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

최종 MCP 실제 조회: Circus-Main-Hall, Dirty=false, 루트 23개. DungeonManager의 DungeonUI 및 SectorGoal 참조 정상. PlayMode=false, Paused=false.

### Compile

Result: PASS

AssetDatabase.Refresh 이후 재컴파일 확인. 최종 isCompiling=false, scriptCompilationFailed=false.

### Console

Errors: 0 (최종 실제 Editor Console)

Warnings: 7 (최종 실제 Editor Console)

Exceptions: 0 (MCP 조회)

작업 중 class 필터를 사용한 테스트 검색은 'No tests found'로 실패했고 MCP 도구 자체가 Error 로그 1개를 기록했다. 완전한 테스트 메서드 이름으로 다시 실행해 필요한 테스트를 모두 수행했다. MCP 과거 로그 캐시에는 그 도구 오류가 남아 있지만 최종 실제 Editor Console Error는 0개다. 테스트 검색 실패를 테스트 실행 PASS로 취급하지 않았다.

코드/테스트 범위 git diff --check: PASS.

---

## EditMode Tests

Result: PASS

Passed: 83

Failed: 0

Not Run: 지정한 PlayerMovement.EditModeTests 어셈블리 밖 테스트

실행 시간 00:00:01.7758449. MCP Summary의 TotalTests=100은 검색 수이며 실제 통과 수는 83이다. 신규 검증: 중복·중첩 숨김 요청, 초기 비활성 상태 복원, 연출 중 목표 텍스트 갱신 및 패널 숨김 유지.

---

## PlayMode Tests

Result: PASS

Passed: 4 (관련 메서드 개별 실행)

Failed: 0

Not Run: 전체 PlayMode 재실행

- DungeonObjectivesStayHiddenUntilVideoFrameIsReleased: PASS, 0.083초. 실제 영상 시작 경로, 완료·스킵·실패·취소, held frame, 오래된 세션 취소 및 비활성화 복원.
- OverlappingTimelinesPauseAndCancellationRestoreObjectives: PASS, 0.336초. 두 Timeline 중첩, Pause, Stop, 목표 갱신, 비활성화·파괴, 매니저 종료와 재구독, 자연 종료 및 null Timeline.
- PooledBossBindsAndCancellationRestoresCameraAndMusic: PASS, 0.950초. 실제 Jester60 컷씬 취소 시 목표·카메라·BGM 복원.
- IntroductionFinishesBeforeMiddleBossStarts: PASS, 10.850초. 실제 Jester60Intro 영상 디코딩, 스킵 후 프레임 유지, 실제 카메라 Timeline 동안 숨김, 전투 진입 후 목표 표시 복원.

각 실행의 검색 총수 81을 전체 81개 통과로 기록하지 않았다.

---

## 발견된 문제

null Timeline을 먼저 숨긴 뒤 재생하면 stopped 이벤트 없이 숨김이 남을 가능성이 있어 사전 검사로 방지했다. MCP class 테스트 검색 문제는 메서드 필터로 우회해 실제 결과를 확인했다.

---

## 남은 문제

현재 요청 경로의 실패 테스트나 컴파일 오류는 없다. 매니저만 비활성화한 채 연출을 계속 재생하고 다시 켜는 특수 경로는 기존 숨김 소유권을 자동 재취득하지 않는다. 매니저 종료는 숨김 상태도 정리하며, 다음 정상 컷씬 재생에서 다시 구독한다.

---

## 미검증 항목

- 실제 체스 전투 전체 및 별도 DungeonTest 섹터의 수동 플레이: 호출 연결은 코드 검토하고 공통 재생 경로를 테스트했으나 해당 전투·씬 전체는 이번 작업에서 실행하지 않았다.
- 전체 타이틀~던전 클리어 통합 흐름 재실행: 직전 작업에서 검증했고 이번에는 직접 관련된 테스트를 실행했다.
- 사람이 직접 화면을 감상하는 수동 검수: 실제 영상·Timeline을 자동화 실행하고 목표 GameObject 활성 상태를 검증했다.

---

## 최종 결과

영상 재생과 컷씬 동안 던전 목표가 앞에 나타나지 않도록 수정했다. 종료·스킵·취소 및 영상 마지막 프레임/Timeline Pause 상태를 처리하고 기존 목표 표시를 복원한다. 씬·프리팹 추가 설정 없이 적용되며, 83개 EditMode 및 관련 4개 PlayMode 테스트가 통과했다.
