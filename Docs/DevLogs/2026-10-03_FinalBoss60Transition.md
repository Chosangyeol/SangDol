# D1_FinalBoss 60% 중간 보스 전환 순서

## 작업 목적

진행 중인 일반 패턴의 종료 후 그네로 중앙 이동 → 동영상 → Cinemachine Timeline → 중간 보스 조우 순서로 60% 패턴을 실행한다. 사용자의 추가 지시에 따라 아직 제작되지 않은 Timeline의 Inspector 슬롯은 비워 둔다.

---

## 분석

- `BossModel.HandleCheckSpecial()`에는 이미 `currentPattern != null`이면 전환하지 않는 조건이 있다. 일반 패턴 코루틴들은 마지막에 `OnPatternEnd()`를 호출한다.
- `D1_Final_Normal2`는 `Normal2End` 트리거 직후 완료를 통지한다. 실제 Jester Animator의 Idle 전환에는 0.25초 블렌딩이 있어, 코루틴 완료만 확인하면 일반 패턴 애니메이션 종료 전에 연출을 시작할 수 있다. 사용자 현상의 전체 인게임 재현은 하지 않았으므로 이 경로를 확인된 전환 경계 문제로 구분한다.
- 기존 `ReadyForSpecial()`과 `EndSpecial()`에 그네 하강, 점프, 상승, 메시 숨기기, 중앙 하강과 착지 연출이 있다. 다만 `EndSpecial()`은 특수 패턴 상태를 해제하므로 그대로 재사용하면 영상 전에 일반 패턴이 다시 시작된다.
- 기존 Special3에는 Cinemachine Timeline 슬롯과 재생 단계가 없었다.

---

## 수정 파일

- `Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss.cs`
- `Assets/Tests/Editor/FinalBossTransitionTests.cs` 및 Unity 생성 `.meta`
- `Assets/Tests/EditMode/FinalBossTransitionEditModeTests.cs` 및 Unity 생성 `.meta`
- `Assets/Tests/PlayMode/FinalBossTransitionPlayModeTests.cs` 및 Unity 생성 `.meta`
- 본 DevLog

Scene, Prefab, Package, ProjectSettings는 이번 작업에서 수정하지 않았다. 작업 전부터 존재하던 변경사항은 보존했다.

---

## 수정 클래스 / 함수

### D1_Final_Special3Data

- `cutsceneDirector` 추가

### D1_FinalBoss

- `WaitForNormalPatternEnd()`
- `ReadyForSpecial()` / `EndSpecial(bool completePattern = true)`
- `MoveBossToCenter()`
- `PlaySpecial3Timeline()` / `StopSpecial3Timeline()`
- `RestoreSpecial3Movement()`
- `Special_MiddleBoss()` / `OnActionsStopped()`

---

## 주요 변경 내용

- 기존 일반 패턴 실행 중 대기 조건은 유지하고, Special3 진입에서 Animator가 종료 트리거를 처리해 Idle로 돌아오며 블렌딩까지 끝날 때까지 기다린다. Move를 끄므로 추적 이동 상태도 Idle로 복귀한다. Animator/Controller가 없는 경우 애니메이션 대기는 생략한다.
- NavMesh 보행 대신 기존 그네 상승과 중앙 하강 코루틴을 순서대로 재사용한다. 점프 마지막 위치를 정확하게 맞추고 중앙 지점의 높이를 보존한다.
- `EndSpecial(false)`로 착지해도 특수 패턴/무적 상태 및 Agent 비활성 상태를 유지한다. 중간 보스 조우 종료 후 복원한다. 기존 다른 패턴의 `EndSpecial()` 호출은 기본 옵션을 사용한다.
- 동영상 세션이 끝나거나 스킵/실패 처리된 뒤에만 연결된 Timeline을 재생한다. Timeline이 끝난 뒤 중간 보스를 생성하고 플레이어를 대기 지점으로 옮긴다.
- Timeline 재생 중 UI를 숨기고 종료/중단 시 UI와 기본 카메라를 복구한다. 재생 시 WrapMode를 None으로 설정해 반복 재생으로 인한 대기를 방지하고 종료 시 원래 설정을 복구한다.
- 연출 중 강제 정지 시 Timeline, 영상 세션, 메시, 보스 중앙 위치/Agent 활성 상태와 플레이어 조작을 복구한다. 반복 정지도 검증했다.
- 영상/Timeline 미연결 시 경고 후 계속 진행하는 기존 복구 정책을 유지한다. 그네 데이터 누락 시에도 경고와 직접 중앙 이동을 사용한다.

### Inspector 연결 안내

Jester의 `D1_FinalBoss` 컴포넌트에서 **각 특수 패턴 변수 → Special3**을 펼친다.

| 슬롯 | 넣을 대상 | 현재 확인 |
| --- | --- | --- |
| Prefab | `D1_MiddleBoss`와 유효한 `EnemyStatSO`가 있는 중간 보스 프리팹 | `D1_MiddleBoss_Test_Runtime.prefab` 연결됨 |
| Cutscene Clip | 중앙 이동 후 재생할 `VideoClip` | 현재 `Assets/Title/Title-Images/Title-Images.mp4` 연결됨. 전용 영상 제작 시 교체 |
| Cutscene Director | Cinemachine Track이 포함된 Timeline을 재생하는 **씬 오브젝트의 PlayableDirector 컴포넌트** | 사용자 지시대로 비어 있음 |
| Waiting Area | 중간 보스 구역의 플레이어 대기 지점 Transform | 기존 TEST 앵커 탐색 유지 |
| Middle Boss Spawn Point | 중간 보스 스폰 지점 Transform | 기존 TEST 앵커 탐색 유지 |
| Return Point | 중간 보스 처치 후 플레이어 복귀 Transform | 생략 시 `playerStartPos` 사용 |

Timeline 제작 시 전용 연출 오브젝트에 PlayableDirector를 추가하고 Playable 슬롯에 TimelineAsset을 넣는다. Cinemachine Track의 각 Shot에는 연출용 가상 카메라를 연결하고 메인 카메라의 CinemachineBrain을 트랙 바인딩으로 지정한다. Play On Awake는 끈다. 완성한 PlayableDirector 컴포넌트를 Special3의 Cutscene Director 슬롯에 드래그한다. `.playable` 자산 자체나 가상 카메라 자체를 이 슬롯에 넣는 것이 아니다. 체크메이트용 `D1_Special5CutScene`은 연결하지 않았다.

기존 **각 일반 패턴 변수 → Pattern3**의 Swing Prefab과 Jump Curve가 이동 연출에 재사용된다. Unity에서 기존 `D1_Final_N3.prefab` 참조를 확인했다.

---

## Public API 변경

- 변경 전: `D1_Final_Special3Data`에 Timeline 참조 없음.
- 변경 후: `public PlayableDirector cutsceneDirector` 추가.
- 변경 이유: 영상 이후 실행할 Cinemachine Timeline을 Inspector에서 지정.
- 영향 범위: Special3 데이터/Inspector의 추가 슬롯. 기존 필드 이름, 클래스 이름, namespace와 기존 공개 메서드는 유지했다. 기존 Serialize 데이터 마이그레이션은 필요 없다. 슬롯 기본값은 null이다.

---

## Multi-Agent 작업

- 사용 여부: 사용.
- Main Agent: 요구사항 분석, 수정, 실제 Unity MCP 검증, 테스트 실행과 DevLog 작성.
- Review Agent: 읽기 전용으로 일반 패턴 완료/Animator 경계, 기존 그네 재사용, 공개 API/Serialize 위험, 중단 복구를 독립 검토. 최종 코드에서 차단 문제 없음.
- Test/QA Agent: 읽기 전용으로 기존 테스트 구조와 영상 복구 계약을 분석하고 임계값, 연출 순서, 중단 테스트 제안.
- 주요 리뷰 반영: 단순 currentPattern 조건 추가 대신 Animator 종료 대기, `EndSpecial(false)` 상태 유지, 이동 중 Agent/중앙 위치 복원, Timeline 루프 방지, 영상 없는 설정 허용을 반영.
- 서브에이전트는 파일 수정이나 Unity 검증을 실행하지 않았다. 아래 결과는 모두 Main Agent가 실행했다.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS (상태/참조 확인 범위)

- Unity MCP로 `Circus-Main-Hall`이 열려 있고 루트 22개, 씬 dirty=false를 확인.
- 최종 Editor는 PlayMode=false, compiling=false, updating=false.
- 현재 씬의 Director는 체크메이트용 `D1_Special5CutScene`/Chess이며 Special3 전용 Timeline은 없다.
- Jester 프리팹의 Special3 Timeline 슬롯이 null이며 HP 0.6 조건, 영상/그네/중간 보스 프리팹 연결을 실제 Unity 객체에서 확인.
- 테스트는 임시 GameObject/NavMesh/Timeline으로 실행했고 실제 씬 구조는 수정하지 않았다.

### Compile

Result: PASS

AssetDatabase Refresh 후 컴파일 완료를 확인했고 신규 테스트 및 전체 검증 대상 어셈블리 실행이 완료됐다. `git diff --check`도 통과했다.

### Console

Errors: 최종 회귀 테스트 구간과 최근 2분 MCP 조회에서 0.

Warnings: 전체 PlayMode 실행 결과 9건. 테스트 NavMesh 추가 중 기존 씬 Agent의 위치 경고 5건, 의도적으로 비운 영상 경고 1건, 기존 영상 오류/누락/timeout 복구 테스트 경고 3건. MCP 재연결 구간의 연결 timeout과 테스트 진입 시 NavMesh 경고도 Console에 남아 있다. 실제 씬의 기존 Special3 NavMesh 경고는 작업 전에도 확인됐다.

Exceptions: 최종 최근 2분 MCP 조회에서 0.

---

## EditMode Tests

Result: PASS

Passed: 58 (`PlayerMovement.EditModeTests` 어셈블리, 1.326초)

Failed: 0

Not Run: 별도 전체 프로젝트/Package 테스트는 실행하지 않음.

신규 임계값 61/60/59%, 진행 중 패턴 대기 및 중복 발동 방지 테스트 포함. 도구의 TotalTests=75는 발견 수와 실행된 Passed=58이 달라 실제 통과 개수 58로 기록한다.

---

## PlayMode Tests

Result: PASS

Passed: 54 (`PlayerMovement.PlayModeTests` 어셈블리, 43.958초)

Failed: 0

Not Run: 다른 어셈블리/Package 테스트는 실행하지 않음.

신규 테스트 5개:

- `NormalAnimationFinishesBeforeSwingStarts`
- `SwingLandsAtCenterBeforeTimelineAndEncounter`
- `VideoFinishesBeforeTimelineAndEncounter`
- `CancellationRestoresAirborneBossAndControls`
- `CancellationStopsTimelineWithoutStartingEncounter`

기존 BossLifecycle 4개와 VideoRecovery 5개도 포함해 모두 통과했다. 영상 순서 테스트는 실제 연결된 VideoClip/VideoPlayManager 재생 세션을 시작한 뒤 스킵하고, 그동안 Timeline/중간 보스가 시작되지 않다가 영상 종료 처리 후 진행되는 것을 검증한다.

---

## 발견된 문제

- 첫 그네 순서 테스트에서 메시 숨기기/다시 표시하기가 동일 프레임에 진행되어 프레임별 폴링이 숨김 상태를 놓쳤다. 보스 숨김 이벤트를 관찰하도록 테스트를 수정하고 동일 테스트 및 전체 어셈블리를 재실행해 통과했다. 이 초기 FAIL을 최종 결과와 구분한다.
- Unity MCP의 testClass 필터 호출은 테스트 미발견/0개 실행을 반환했다. 해당 호출은 PASS로 간주하지 않고 개별 testMethod 및 전체 테스트 어셈블리 실행으로 검증했다.
- 테스트 진입/종료 중 MCP 연결이 잠시 끊겼으나 이후 재연결되어 최종 검증을 완료했다.

---

## 남은 문제

- Special3 전용 Cinemachine Timeline 제작/Shot 및 Brain 바인딩은 아직 없으며 사용자 요청에 따라 슬롯을 비워 둔다.
- 현재 Special3 영상은 Title 영상이고, 중간 보스 구역에는 기존 테스트용 앵커/프리팹을 사용한다. 전용 콘텐츠 제작 후 슬롯을 교체할 수 있다.

---

## 미검증 항목

- 실제 전용 Cinemachine 카메라 연출의 구도/블렌드/Timeline 바인딩: NOT AVAILABLE. 아직 제작되지 않음.
- 실제 영상 전체의 자연 종료와 최종 제작 씬 전체 플레이: NOT VERIFIED. 영상 스킵 후 순서와 기존 VideoRecovery 종료 처리는 자동 테스트로 검증했지만, 전체 영상 감상/전용 콘텐츠 연출 검수는 수행하지 않음.
- 타 특수 패턴의 실제 인게임 전체 연출: NOT RUN. 공용 EndSpecial의 기본 옵션은 기존 상태 해제 흐름을 유지했지만 전용 플레이 테스트는 없다.

---

## 최종 결과

60% 전환은 일반 패턴 코루틴 및 종료 애니메이션 이후 시작되며, 그네로 중앙 이동 후 영상과 지정된 Timeline을 순서대로 기다리고 중간 보스 조우를 시작한다. Unity 컴파일, EditMode 58개와 PlayMode 54개가 통과했다. Timeline 슬롯은 비어 있고 연결 안내를 기록했다.
