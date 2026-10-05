# Jester60 등장 연출 적용 및 전체 게임 진행 검증

## 작업 목적

추가된 Jester60 에셋을 60% 중간보스 등장 연출로 적용한다. 기존 중간보스 전투 공간과 전투를 유지하고, 타이틀 → Map1-Forest 퀘스트·성장 → 던전 → 보스 → 클리어 → 타이틀 → 새 게임 재시작 경로를 실제 Unity Editor에서 검증하며 진행을 막는 문제를 수정한다.

---

## 분석

- Jester60 원본 씬에 카메라·박쥐·음성 연출이 있지만 바로 전투 씬에 연결되는 master Timeline은 없었다.
- 풀링된 최종보스는 DontDestroyOnLoad 씬에 있으므로 씬 소속으로만 검색하면 등장 연출 연결을 놓친다.
- 단위 테스트만으로 발견되지 않았던 실제 UI 참조, 퀘스트 순서, 씬 전환, 중간보스 이동·피격 문제가 있었다.
- 자동 전투 회피 목적지의 높이가 실제 바닥보다 높아 NavMesh 검색이 실패했던 테스트 문제도 수정했다. 이 실패를 보스 밸런스 문제로 처리하지 않았다.

---

## 수정 파일

### 등장 연출 및 씬/프리팹

- `Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss60Presentation.cs` 및 meta
- `Assets/03. Prefab/Dungeon/Jester60Intro/Jester60Introduction.prefab` 및 meta
- `Assets/03. Prefab/Dungeon/Jester60Intro/Jester60Introduction.playable` 및 meta
- `Assets/03. Prefab/Dungeon/Jester60Intro/Jester60Camera.playable` 및 meta
- `Assets/03. Prefab/Dungeon/Jester60Intro/Special3ArenaNavMesh.asset` 및 meta
- `Assets/01. Scene/Circus-Main-Hall.unity`
- `Assets/03. Prefab/Enemy/Boss/D1/Middle/D1_MiddleBoss_Test_Runtime.prefab`

### 진행 문제 수정

- `Assets/02. Scripts/CameraRay.cs`
- `Assets/02. Scripts/Character/Buff/HealBuff.cs`
- `Assets/02. Scripts/Character/C_Inventory.cs`
- `Assets/02. Scripts/Items/UseItem/Heal/Heal.cs`
- `Assets/02. Scripts/Enemy/EnemyBase.cs`
- `Assets/02. Scripts/Enemy/EnemyModel.cs`
- `Assets/02. Scripts/Enemy/AI/AttackState.cs`
- `Assets/02. Scripts/Enemy/AI/DieState.cs`
- `Assets/02. Scripts/Enemy/AI/PatrolState.cs`
- `Assets/02. Scripts/Enemy/AI/ReturnState.cs`
- `Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_Bullet.cs`
- `Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FianlPatterns.cs`
- `Assets/02. Scripts/Enemy/Dungeon/Sector/EnemySector.cs`
- `Assets/02. Scripts/System/FiedlSystem/EnemySpawner.cs`
- `Assets/02. Scripts/Interact/Npc/SO/Maria.asset`
- `Assets/02. Scripts/Interact/Object/JumpObject.cs`
- `Assets/02. Scripts/UI/DungeonEnter/DungentEnterUI.cs`
- `Assets/02. Scripts/UI/Npc/NpcDialogManager.cs`
- `Assets/02. Scripts/UI/Title/TitleManager.cs`

### 테스트

- `Assets/Tests/Editor/Jester60IntroductionTests.cs`
- `Assets/Tests/EditMode/Jester60IntroductionEditModeTests.cs`
- `Assets/Tests/PlayMode/Jester60IntroductionPlayModeTests.cs`
- `Assets/Tests/Editor/GameFlowTests.cs`
- `Assets/Tests/PlayMode/GameFlowPlayModeTests.cs`
- `Assets/Tests/Editor/GameFlowRegressionTests.cs`
- `Assets/Tests/EditMode/GameFlowRegressionEditModeTests.cs`
- `Assets/Tests/PlayMode/GameFlowRegressionPlayModeTests.cs`
- 위 신규 테스트의 meta 파일

---

## 수정 클래스 / 함수

- `D1_FinalBoss60Presentation`: `OnEnable()`, `BindBoss()`, `OnPlayed()`, `OnStopped()`, `RestorePresentation()`, `OnDisable()`
- `CameraRay.HandleMouseHover()`, `EnemyBase.Damaged()`
- `EnemyModel.Update()/Reset()`, AI 상태의 `ExitState()`, `DieState.EnterState()`
- `EnemySector.SpawnRoutine()/ExecuteSwingMotion()`, `EnemySpawner.ActiveSpawner()/DeactiveSpawner()/OnDisable()`
- `D1_Final_Normal1.SpawnBox()`, `D1_Bullet.Init()/OnTriggerEnter()`
- `JumpObject.Interact()/JumpSequence()/OnDisable()` 및 조작 반환 처리
- `Heal.OnUseItem()`, `C_Inventory.UseItem()`, `HealBuff.OnUpdate()`
- `DungentEnterUI.UpdateDungeonEnterUI()`, `NpcDialogManager.OpenNpcUI()`, `TitleManager.StartGameRoutine()`
- 신규 테스트 클래스의 에셋 검증, 회귀 검증, 실제 씬 진행 코루틴

---

## 주요 변경 내용

### 60% 등장 연출

- 기존 보스의 일반 패턴 종료 대기 → 그네로 중앙 이동 → 영상 → Timeline → 중간보스 전투 순서에 새 연출을 연결했다.
- 26초 master Timeline이 카메라, 박쥐/음성, 음악을 함께 재생한다. 원본 박쥐 Timeline을 재사용하고 카메라 Timeline은 복사본을 수정했다.
- 활성 연결 컴포넌트와 비활성 `Presentation` 자식을 분리했다. 일반 전투 중 카메라·조명·음악이 자동 재생되지 않는다.
- 연출 중 게임 카메라와 BGM을 잠시 멈추고 완료·취소 시 복구한다. 종료 콜백에서 payload를 바로 끄므로 다음 프레임의 카메라 선택과 충돌하지 않는다.
- 검게 보이던 시계 카메라 구간을 제거하고, Timeline 트랙·클립을 실제 sub-asset으로 저장했다. 재임포트 후에도 3개 트랙과 7개 sub-asset이 유지되는 것을 테스트했다.
- Jester60 원본 씬, CameraTimeline, Bat-Boss 파일의 SHA-256이 적용 전과 동일함을 확인했다.

### 퀘스트·이동·소모품

- Maria 방문 순서에 따라 소개 퀘스트를 완료하지 못하거나 사과 퀘스트가 먼저 노출되던 데이터 연결을 수정했다. 완료 전용 대화 버튼도 지원한다.
- 던전 진입 UI의 선택적인 골드 라벨이 비어 있을 때 발생하던 예외를 수정했다.
- 점프 후 조작이 돌아오지 않던 문제와 중복 실행·사망·비활성화 경계 조건을 수정했다.
- 지속 회복 포션 사용이 성공으로 반환되지 않아 소모되지 않던 문제를 수정했다. 실제 20001 포션의 총 회복량 60, 3초 지속, 15초 쿨다운을 적용한다.
- 아이템 ID별 쿨다운을 기록해 다른 스택이나 퀵슬롯으로 바꿔도 재사용을 막는다. 큰 프레임과 소수점 지속 시간에서도 회복 횟수가 초과하지 않는다. 무한 회복 버프는 기존 간격과 회복값 의미를 유지한다.
- 타이틀에서 Main을 additive 로드할 때 타이틀 AudioListener/EventSystem 중복을 정리한다.

### 몬스터·보스·씬 전환

- 비활성/미연결 NavMeshAgent에서 AI 이동 API가 실행되지 않도록 하고, 죽은 적의 상태 처리는 계속 진행하도록 했다.
- 그네 스폰 중 죽거나 파괴된 적이 마지막 착지 프레임에 살아나거나 다시 초기화되지 않도록 했다.
- 풀에서 대여한 적에 새 던전 스폰 지점을 첫 yield 전에 연결한다. 숲의 삭제된 Transform을 순찰 AI가 참조하던 예외를 해결했다.
- 필드 spawner 비활성화 시 재생성 코루틴과 남은 적을 정리한다. 비활성 spawner의 재활성 요청, PoolManager 선행 파괴, 중복 반환도 방어한다.
- 일반 상자 패턴의 지면 탐색이 실패해도 무한 루프에 빠지지 않고 제한 횟수 후 종료한다.
- 투사체가 같은 프레임 여러 Collider에 닿아 중복 피해를 주지 않도록 했다.
- 연출 중 `Camera.main`이 없을 때 호버 코드가 예외를 내지 않도록 했다.
- 중간보스의 데미지 숫자 표시 앵커가 비어 있어도 보스 위치를 사용해 정상적으로 피해를 처리한다.
- 기존 `Special3_TestArena`에 자식 Collider만 수집하는 NavMeshSurface와 베이크 데이터를 추가했다. 대기 지점과 보스 스폰 지점 모두 이동 가능하다.
- 기존 Runtime 중간보스 프리팹의 Collider를 Enemy 레이어로 설정하여 플레이어 스킬 검색 대상에 포함했다.
- 별도 `DungeonEnd_ReturnToTitle` 오브젝트에 의한 클리어/타이틀 복귀 흐름은 유지했다. 보스에 타이틀 이동 코드를 추가하지 않았다.

### Inspector 사용

- `Circus-Main-Hall/Jester60Introduction`의 `D1_FinalBoss60Presentation`은 연결 컴포넌트다.
- `Director`: 자식 `Presentation`의 26초 `Jester60Introduction` master PlayableDirector.
- `Presentation Camera`: 복사된 연출용 Main Camera. `Music Source`: master 음악 트랙의 AudioSource. `Voice Source`: 박쥐 Timeline의 음성 AudioSource.
- 자식 `Presentation`은 기본 비활성, 모든 Director의 Play On Awake와 AudioSource의 Play On Awake는 꺼 둔다. 복사된 연출용 AudioListener도 꺼 둔다.
- root를 끄거나 제거하면 연결 컴포넌트가 자신이 변경한 보스의 Timeline 참조를 이전 값으로 돌려놓는다. 연출만 교체하려면 Director와 해당 Timeline 바인딩을 교체한다.
- 타이틀 종료 기능은 별도 `DungeonEnd_ReturnToTitle` 오브젝트에서 관리한다.

---

## Public API 변경

- 변경 전: 씬 단위 Jester60 연출 연결 컴포넌트 없음.
- 변경 후: 새 public sealed MonoBehaviour `D1_FinalBoss60Presentation` 추가. 연결 필드는 private SerializeField.
- 이유: 풀링 보스와 씬의 등장 연출을 연결하고 제거·비활성화 시 복구하기 위해 추가했다.
- 영향: 신규 프리팹/씬 컴포넌트만 사용한다. 기존 public API, 클래스 이름, SerializeField 이름을 삭제하거나 변경하지 않았다.
- Maria의 기존 직렬화 데이터 항목과 중간보스 프리팹의 레이어 override, 기존 전투 공간의 NavMeshSurface 추가는 의도한 데이터 변경이다.
- 기존 인코딩을 유지하여 CP949 스크립트를 일괄 UTF-8 변환하지 않았다.

---

## Multi-Agent 작업

- 사용 여부: 사용.
- Main Agent: 요구사항 분석, 모든 코드/데이터/씬/프리팹 수정, 최종 통합, 실제 Unity MCP 검증 및 보고서 작성.
- Review Agent(`/root/review`): 독립 읽기 전용 코드·에셋·씬 참조 리뷰. 파일 수정과 Unity 테스트 실행은 하지 않음.
- 별도 Test/QA Agent: 이번 작업에서는 사용하지 않음. 이전 작업의 agent 결과를 이번 실행 성공 근거로 사용하지 않음.
- 주요 지적 및 반영: pooled boss의 씬 소속 필터 제거, Timeline 영속 저장 확인, payload 즉시 비활성화, 상자 탐색 제한, Maria 선행 퀘스트 순서, 착지 마지막 프레임 검사, 포션 쿨다운 기록·틱 상한, 비활성 spawner 재활성 방지, 중간보스 Enemy 레이어 수정.
- 리뷰가 발견한 자동 회피 목적지 높이 문제를 테스트에서 수정했다. 평타는 레이어 없이 검색하지만 스킬은 Enemy 레이어를 사용한다는 정정도 검토·반영했다.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

- Unity MCP로 직접 확인. 최종 Editor는 Play Mode 아님, 컴파일 중 아님.
- 전체 테스트 직후 열린 씬: Circus-Main-Hall. 저장 완료, dirty=false, root 24개, Missing Script 0개.
- Jester60Introduction root 활성, Presentation payload 비활성, master Play On Awake=false.
- master Timeline 26초, root track 3개, 영속 sub-asset 7개.
- 기존 중간보스 대기·스폰 지점 NavMesh.SamplePosition 성공.
- 연출 2/9/15/23초 카메라 캡처를 확인했다. 캡처 위치: `Temp/Jester60Applied_*.png`.
- 원본 Jester60 주요 3개 파일 해시 동일, scoped git diff --check PASS.

검증 후 마지막 재확인에서는 현재 Editor 씬이 dirty=true/root 23개이며 `DungeonEnd_ReturnToTitle`이 빠진 미저장 변경이 확인됐다. 현재 Editor 변경을 저장하거나 덮어쓰지 않았다. 저장된 씬 파일은 별도 preview scene으로 읽어 root 24개, 타이틀 복귀 오브젝트와 등장 연출 존재, Missing Script 0개를 다시 확인했다. 위 진행 테스트가 통과한 대상은 이 저장된 씬이다.

### Compile

Result: PASS

Asset Refresh 완료 후 실제 EditMode/PlayMode 테스트를 실행했다. 최종 컴파일 상태와 Console을 다시 확인했다.

### Console

Errors: 0

Warnings: 36

Exceptions: 0

최종 테스트 전 Console을 초기화하고 전체 테스트 후 실제 Editor LogEntries 카운트를 확인했다. Warning에는 의도적인 영상 오류·타임아웃·미설정 씬/연출 테스트, 지면 없는 상자 테스트, NavMesh/Animator 테스트 fixture, MCP 재연결 로그와 기존 영상 색상 정보·숲 나무 shader 경고 등이 포함된다. 실제 중간보스 콘텐츠의 Animator Controller가 없는 경고도 있다. Warning을 0으로 보고하지 않는다.

후속 저장 씬 점검용 임시 MCP 코드에서 Unity 2022의 internal OpenPreviewScene API를 public으로 호출해 CS0117 도구 컴파일 오류 로그 3개가 발생했다. 프로젝트 파일의 컴파일 오류가 아니며 reflection 호출로 수정하여 저장 씬 점검에 성공했다. 진단 로그를 확인·정리한 후 fresh Console Error/Warning 0개, 컴파일 중 아님을 확인했다. 전체 테스트 직후 Warning 36개 기록은 위에 유지한다.

---

## EditMode Tests

Result: PASS

Passed: 80

Failed: 0

Not Run: 0 (PlayerMovement.EditModeTests assembly)

Skipped: 0

Duration: 1.6828253초. MCP Summary의 TotalTests=97에는 suite 항목이 포함되므로 실제 PassedTests=80을 결과로 기록했다.

---

## PlayMode Tests

Result: PASS

Passed: 70

Failed: 0

Not Run: 0 (PlayerMovement.PlayModeTests assembly)

Skipped: 0

Duration: 223.9685815초. Main이 Unity MCP로 전체 assembly를 실제 실행했다.

### 실제 씬 진행 테스트 결과

`GameFlowPlayModeTests.TitleForestQuestsDungeonBossAndReturn`: PASS

1. 실제 Title의 StartGame → Main/Map1-Forest 로드. 플레이어, EventSystem, AudioListener 중복 없음.
2. Maria 선행 방문 → Haber 소개 퀘스트 → Maria 완료 → 실제 상점 사과 구매 → 완료 → Dail 늑대 5마리 처치/완료.
3. 퀘스트 3개 Completed, 레벨 1→7, 공격력·최대 HP 증가, 골드 1750 확인.
4. 실제 Forest 던전 포털과 UI 버튼 → Circus-Main-Hall 진입. 플레이어와 성장 상태 유지.
5. 첫 적 섹터 5마리 처치 → 총 7번의 authored 점프 → 이동 섹터 → 다음 적 섹터 5마리 처치.
6. 두 실제 워프 포털 → 최종보스 방 → 시작 패턴 및 실제 일반 패턴.
7. HP 80% 카드 패턴의 표시를 읽고 안전 구역 이동, 생존 및 보스 복귀.
8. HP 60% 그네 중앙 이동 → 영상 → 실제 Jester60 Timeline 26초 자연 재생 완료 → 기존 중간보스 전투.
9. Controller.RequestBasicAttack과 실제 평타 애니메이션 이벤트로 중간보스 HP 감소 확인. Enemy 레이어 확인.
10. 중간보스 처치 → 최종보스 전투·플레이어 조작·게임 카메라 복구.
11. 최종보스 처치 → 사망 연출 대기 → 던전 완료 → Title Single 로드.
12. 이전 플레이어·PoolManager 제거, Time.timeScale=1, Title 씬만 존재 확인.
13. Title에서 새 게임 시작 → 새 Forest/플레이어, 소개 퀘스트 NotStart, NavMesh 이동 가능 확인.

UI 선택과 상호작용은 실제 컴포넌트의 버튼·포털·트리거를 사용한다. 긴 이동은 기존 NavMesh 위에서 자동 Warp하고, 카드 전 대기 구간은 Controller 이동 요청으로 회피한다. 전투 진행과 최종 처치는 실제 Damaged API에 플레이어 source를 전달하여 자동화한다. 퀘스트 상태, 섹터 완료, 보스 특수패턴 완료 플래그를 강제로 성공시켜 검증하지 않는다. 영상은 실제 VideoPlayManager의 skip 경로를 사용한다. 최종 실행 trace: `Temp/GameFlowVerification.txt`.

---

## 발견된 문제

던전 보상 UI null 참조, Maria 선행 순서, 점프 후 조작 잠금, 지속 포션 미소모/쿨다운 미기록, 회복 틱 초과, NavMeshAgent 상태 접근, 죽은 적의 그네 착지 재초기화, 삭제된 필드 스폰 Transform 재사용, 연출 중 Camera.main null 참조, 중간보스 공간 NavMesh 누락, 피격 표시 앵커 누락, 중간보스 스킬 피격 레이어 누락을 수정했다.

---

## 남은 문제

- 기존 중간보스는 테스트용 캡슐 콘텐츠이며 전투 패턴과 Animator Controller가 비어 있다. 새 박쥐 에셋은 사용자가 선택한 등장 연출 범위로 적용했고 실제 전투 보스로 교체하지 않았다.
- 던전 UI에 표시된 보상 지급은 기존 콘텐츠 범위다. 이번 타이틀 종료 경로 검증에서 신규 보상·저장 정책을 설계하거나 추가하지 않았다.
- 기존 영상 색상 정보 및 숲 나무 shader Warning은 남아 있다.

---

## 미검증 항목

- 40% 야바위·20% 체스 패턴의 전체 실제 플레이: NOT RUN. 해당 구간은 최종 처치 피해로 건너뛰었다. 전체 보스의 모든 패턴을 끝까지 플레이했다고 보고하지 않는다.
- 사람의 키보드/마우스 조작으로 모든 이동·공격·영상 자연 종료를 포함한 완주와 전투 난이도/밸런스: NOT VERIFIED. 자동 UI/이동/피해 시나리오이며 평타 입력·애니메이션 피격은 별도로 확인했다.
- 독립 실행 빌드 및 배포 환경: NOT RUN. Editor에서 검증했다.
- 중간보스 신규 공격 애니메이션/박쥐 전투 교체: 이번 요청 범위 아님.
- 현재 Editor의 종료 오브젝트 삭제 미저장 변경을 반영한 진행 루틴: NOT RUN. 그 상태는 사용자 작업 보호를 위해 저장/복원하지 않았다.
- 후속 UI 확인용 computer-use는 app approval timeout으로 창 상태를 얻지 못했다. 이후 Unity MCP 상태 확인은 다시 성공했으며, Windows UI 조작은 수행하지 않았다.

---

## 최종 결과

Jester60를 실제 60% 등장 연출에 연결했다. 실제 씬 진행에서 발견된 차단 문제를 수정했고, 타이틀부터 퀘스트 성장·던전·80%/60% 패턴·중간보스·최종보스 클리어·타이틀 종료 및 새 게임 재시작 경로가 통과했다. EditMode 80개와 PlayMode 70개 통과, 최종 Console Error/Exception 0개. 기존 타이틀 복귀 기능은 별도 오브젝트로 유지한다.
