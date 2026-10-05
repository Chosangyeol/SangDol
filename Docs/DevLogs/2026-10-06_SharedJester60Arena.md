# Jester60 컷씬 맵과 배우를 중간보스 전투에 공유

## 작업 목적

60% 진입 영상에 Jester60Intro.mp4를 사용하고, 컷씬의 60%Map 및 Bat-Boss를 교체 없이 실제 중간보스 전투에 이어서 사용한다. 테스트 더미를 제거하고 실제 컷씬 배우를 보스 프리팹으로 저장한다.

---

## 분석

기존 구성은 Timeline 종료 시 연출 맵을 비활성화하고 별도 테스트 공간에 중간보스를 생성했다. 공유 구성에서는 연출 카메라와 대역 플레이어만 정리하고, 맵과 배우는 플레이어가 복귀할 때까지 유지해야 한다. 연출용으로 꺼둔 Agent/Collider/AI는 전투 시작 시 복구해야 하며, 씬 소유 배우는 처치 후에도 Destroy하면 안 된다.

---

## 수정 파일

- Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss.cs
- Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss60Presentation.cs
- Assets/02. Scripts/Enemy/Boss/Dungeon1/D1_MiddleBoss.cs
- Assets/03. Prefab/Enemy/Boss/D1/Final/Jester.prefab
- Assets/03. Prefab/Enemy/Boss/D1/Middle/ClockworkBatBoss.prefab 및 meta: 신규
- Assets/03. Prefab/Enemy/Boss/D1/Middle/ClockworkBatDeath.anim
- Assets/03. Prefab/Dungeon/Jester60Intro/Jester60Introduction.prefab
- Assets/03. Prefab/Dungeon/Jester60Intro/Jester60CombatNavMesh.asset 및 meta: 신규
- Assets/01. Scene/Circus-Main-Hall.unity
- Assets/Tests/Editor/Boss60CorrectionsTests.cs
- Assets/Tests/Editor/GameFlowRegressionTests.cs
- Assets/Tests/Editor/GameFlowTests.cs
- Assets/Tests/Editor/Jester60IntroductionTests.cs
- Assets/Tests/PlayMode/Jester60IntroductionPlayModeTests.cs

연결한 사용자 추가 파일: Assets/08. Sound/Jester60Intro.mp4.

삭제한 자산과 meta: D1_MiddleBoss_Test.prefab, D1_MiddleBoss_Test_Runtime.prefab, Jester60Intro/Special3ArenaNavMesh.asset. 참조를 새 구성으로 교체한 뒤 AssetDatabase로 삭제했으며, 이전 GUID와 파일 이름이 Assets의 Scene/Prefab/Asset/C#에 남지 않았음을 검색했다. 백업은 Temp/Boss60SharedBaseline/Removed에 보관했다.

---

## 수정 클래스 / 함수

### D1_FinalBoss

- Special_MiddleBoss(): 씬 배우 전투 인계, Boss UI 알림, 사망 연출 대기 및 복귀.
- StopSpecial3Timeline(), FinishSpecial3Arena(), OnActionsStopped(): 공유 맵 수명과 취소 시 복귀/정리.
- IsSpecial3Configured(): 씬 배우와 기존 프리팹 방식 모두 지원.

### D1_FinalBoss60Presentation

- BindBoss(), OnEnable(), OnDisable(): 배우/맵 앵커/영상 바인딩과 자신이 소유한 변경만 복원.
- OnPlayed(), OnStopped(), BeginEncounter(), FinishEncounter(): 연출, 전투, 정리 단계별 카메라/대역/맵 관리.

### D1_MiddleBoss

- PrepareForPresentation(), BeginCombat(): 동일 배우의 체력/사망 상태 초기화, 활성화 및 AI/Agent/Collider 전환.
- Start(): 모델 크기에 맞춘 주문 사거리 반영.

---

## 주요 변경 내용

- 원래 Bat-Boss 오브젝트와 Animator/뼈/모델/배우 Timeline을 유지하고 보스 프리팹으로 저장했다. 씬 인스턴스 이름도 Bat-Boss를 유지한다.
- 체력, 피격, 타깃 추적, 주문 공격, 사망 연출, 피해 텍스트 앵커, Enemy 레이어, Boss UI를 기존 D1_MiddleBoss/BossModel 기능에 연결했다. 다중 플레이어 Collider에도 주문 피해는 한 번만 적용된다.
- 공유 60%Map에 MeshCollider와 Ground 레이어를 지정하고 NavMesh를 제작했다. 진입 위치와 보스 위치 사이 완전한 경로를 검증했다. 기존 아트 배치를 유지했다.
- 맵 위치는 (2000, 0, 2000), 보스 전투 앵커는 약 (2009.66, -1.18, 2068.88), 플레이어 진입 앵커는 약 (2009.66, -1.18, 2054.88)이다.
- 영상 → 기존 26초 Timeline → 동일 배우 전투 순서다. 영상부터 중간보스 처치/복귀까지 BGM이 정지하고 최종보스 무적이 유지된다.
- Timeline 종료 시 시네마틱 카메라와 플레이어 대역을 끄고 공유 맵을 유지한다. 플레이어 복귀 후 맵을 비활성화하며 배우를 파괴하지 않는다.
- 전투 취소/컴포넌트 비활성화/재진입/정상 처치 후 재사용을 처리한다. Timeline 누락 시에도 공유 전투 맵은 활성화한다.
- 최종보스 프리팹과 씬에 신규 영상 및 배우 프리팹을 연결했다. Scene의 Special3_TestArena 루트는 제거했다. 독립 DungeonEnd_ReturnToTitle 오브젝트는 유지했다.
- 사용자 씬의 기존 미저장 변경을 복사 백업한 뒤 Unity API로 저장했다. 연출 맵을 살펴보던 활성 상태를 씬에 보존하며, 실행 시 Binder가 전투 시작 전 숨긴다.
- 원본 Assets/Jester60.unity, CameraTimeline.playable, Bat-Boss.playable의 SHA256이 이전 백업과 동일함을 확인했다.

---

## Public API 변경

- 변경 전: D1_Final_Special3Data는 prefab만으로 중간보스를 구성.
- 변경 후: public D1_MiddleBoss sceneMiddleBoss 필드를 추가. 지정하면 해당 씬 배우를 사용하고, 비어 있으면 기존 prefab 생성 방식을 유지.
- 이유: 연출 배우를 동일 오브젝트로 전투에 인계.
- 영향 범위: D1_FinalBoss 및 D1_FinalBoss60Presentation의 60% 구성. 기존 SerializeField/public 필드 이름과 기존 메서드 서명은 유지했다.
- 그 밖의 신규 전환 메서드와 상태 프로퍼티는 internal/private이며 외부 public API를 바꾸지 않았다.

---

## Multi-Agent 작업

- 사용 여부: 사용.
- Main Agent: 구현, 씬/프리팹/자산 수정, 실제 Unity 검증, 통합과 기록.
- Review Agent: 읽기 전용 코드 리뷰. 공유 오브젝트 파괴 방지, Agent 복구, 복귀 전 맵 유지, 바인딩 복원 확인.
- Test/QA Agent: 읽기 전용 테스트 검토. 배우 동일성/개수, 취소와 재시도, NavMesh, 영상 순서 및 검증 한계 확인.
- 주요 리뷰 지적사항: Binder 비활성화가 다른 특수 패턴까지 취소할 위험, Timeline 누락 시 부모 맵이 꺼지는 위험.
- 반영 여부: 두 문제 모두 수정하고 관련 PlayMode 회귀 테스트를 추가했다. 사망 후 동일 배우 재사용도 추가 검증했다. 에이전트는 Unity 테스트를 직접 실행하지 않았다.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

Main Agent가 Unity MCP로 확인. Circus-Main-Hall 저장 완료, roots=23, dirty=false, PlayMode=false. 공유 Presentation 내 D1_MiddleBoss 1개, 배우와 Animator 이름 Bat-Boss, 새 ClockworkBatBoss 프리팹 인스턴스 연결 확인. 테스트 공간 루트 제거 및 독립 던전 종료 오브젝트 유지 확인.

### Compile

Result: PASS

Asset Refresh 이후 isCompiling=false, scriptCompilationFailed=false. 중간 테스트 코드의 누락된 UnityEditor 자격명을 수정하고 재컴파일했다.

### Console

Errors: 0

Exceptions: 0

Warnings: 최종 재사용 테스트 후 스냅샷 7개. MCP 도메인 재연결/파일 로그 경고 4개, 테스트 리그의 NavMesh 초기화 경고 2개, 영상 매니저가 없는 리그의 의도된 컷씬 fallback 경고 1개였다. Warning 0으로 보고하지 않는다.

---

## EditMode Tests

Result: PASS

Passed: PlayerMovement.EditModeTests 81

Failed: 0

Not Run: 그 외 어셈블리. MCP 전체 발견 수 98과 실제 필터 실행 수 81을 구분한다.

최종 재사용 보완 후 전체 관련 EditMode 어셈블리도 다시 81개 통과했다.

---

## PlayMode Tests

Result: PASS

Passed: PlayerMovement.PlayModeTests 전체 77개, 4분 15.20초.

Failed: 0

Not Run: 관련 어셈블리 외 테스트.

최종 사망 후 재사용 보완 이후 MusicResumesOnlyAfterMiddleBossDeathAndReturn, IntroductionFinishesBeforeMiddleBossStarts, SharedActorSurvivesCancellationRetryAndBinderDisable을 재실행했고 각각 통과했다.

실제 Scene 루틴은 타이틀 → Forest → 퀘스트 3개 완료/레벨 1→7 → 던전 일반 몬스터 → 점프대 6개 착지 → 보스 일반 패턴/80% → 새 60% 영상 → 26초 Timeline 자연 완료 → 같은 박쥐 접근/플레이어 기본 공격 피격 → 중간보스 처치/복귀/BGM/무적 복구 → 최종보스 처치/Title만 남음 → 새 Forest 재시작까지 통과했다. 실행 기록: Temp/GameFlowVerification.txt.

신규 mp4의 실제 디코딩/재생도 확인했다. 자동 검증은 영상 재생 확인 후 SkipVideo를 사용한다. 개별 Timeline 테스트는 시간 이동으로 카메라와 순서를 검사하고, 실제 씬 루틴은 26초 Timeline을 자연 완료한다. 공격 및 사망 렌더 이미지도 캡처했다.

---

## 발견된 문제

- 모델 root scale 20 때문에 NavMeshAgent radius 2.5가 크게 확대되어 보스와 플레이어가 밀려나 공격을 시작하지 못했다. 테스트 당시 위치/상태를 기록해 원인을 확인하고 로컬 radius 0.125, height 1.6으로 수정했다. 공격/단일 피해/사망 테스트 재실행 PASS.
- 처치 후 비활성화된 배우가 다음 소개에서 다시 보이지 않을 수 있어 PrepareForPresentation에서 다시 활성화했다. 정상 처치 후 같은 배우의 등장과 전투 재사용 PASS.
- Binder 비활성화와 Timeline 누락 경계 문제는 리뷰 지적대로 수정했다.

---

## 남은 문제

이번 요구사항에서 확인된 기능 문제 없음. 아래 검증 한계는 유지한다.

---

## 미검증 항목

- 신규 동영상 전체 자연 종료 및 실제 스피커로 음성/BGM 믹스 청취: 자동 테스트는 재생 확인 후 스킵하고 음량을 0으로 실행.
- 40%/20% 특수 패턴의 전체 플레이: 이번 전체 흐름 검증은 60% 복귀 후 최종보스를 처치하므로 해당 후속 패턴의 전체 플레이를 포함하지 않는다.
- 다른 특수 패턴 보호 테스트는 상태 플래그 유지 확인이며 해당 패턴 코루틴의 전 시간 구간 재생은 별도로 검증하지 않았다.

---

## 최종 결과

컷씬에서 쓰던 맵과 Bat-Boss가 그대로 중간보스 전투에 이어지며 테스트 더미는 제거했다. Jester60Intro.mp4 연결, 공유 배우 프리팹 저장, 이동/공격/피격/사망/복귀/BGM/재사용 검증을 완료했다. 기존 작업 변경은 되돌리지 않았고 Git commit/push는 하지 않았다. C# 및 Docs diff --check 통과. Scene/Prefab은 Unity의 기본 직렬화 형식을 유지했다.
