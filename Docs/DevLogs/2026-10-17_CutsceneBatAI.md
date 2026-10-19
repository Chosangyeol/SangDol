# 컷씬 카메라 전환 및 박쥐 전투 수정

## 작업 목적

컷씬 진입·복귀 시 카메라가 순간적으로 잘못된 위치를 비추는 문제를 수정하고, 박쥐의 플레이어 인식·추적과 신규 공격 패턴을 구현한다.

---

## 분석

- `ChangeCam(..., true)`는 가상 카메라를 껐다 켰지만 진행 중인 CinemachineBrain 블렌드와 출력 위치를 즉시 정리하지 않았다.
- 등장 연출은 출력 Camera만 켜고 끄므로 전용 Brain의 이전 프레임 상태가 재시도에 남을 수 있었다. 첫 Timeline 샷도 페이드 해제 전에 명시적으로 평가하지 않았다.
- 박쥐 전투 시작 조건이 최종 보스의 코루틴에 있었으며, 감지 거리 20 대신 공격 거리로 판정했다. 전투 시작 전에는 공통 보스 Update가 반환하므로 먼저 접근하지 않았다.
- 박쥐 Agent의 기존 정지 거리 5는 날개 공격 거리 3보다 길었다.
- 실제 이동 테스트에서 Agent 속도와 경로는 정상인데 위치가 제자리로 돌아오는 현상을 재현했다. 사망 클립에 있는 루트 Transform 곡선이 Animator 기본값 기록에 참여해 평상시 이동까지 덮어쓰는 것이 원인이었다.
- 기존 공격은 주문, 날개 휘두르기, 2연속 폭발 세 종류였다.

---

## 수정 파일

- `Assets/02. Scripts/Character/CharacterModel.cs`
- `Assets/02. Scripts/Enemy/BossModel.cs`
- `Assets/02. Scripts/Enemy/Boss/Dungeon1/D1_MiddleBoss.cs`
- `Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss.cs`
- `Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss60Presentation.cs`
- `Assets/03. Prefab/Enemy/Boss/D1/Middle/ClockworkBatDeath.anim`
- `Assets/Tests/Editor/BossFourIssuesTests.cs`
- `Assets/Tests/Editor/Boss60CorrectionsTests.cs`
- `Assets/Tests/EditMode/BossFourIssuesEditModeTests.cs`
- `Assets/Tests/PlayMode/BossFourIssuesPlayModeTests.cs`
- 본 DevLog

---

## 수정 클래스 / 함수

### CharacterModel

- `ChangeCam()`: 인덱스·참조 방어, 선택 카메라 활성화, 댐핑과 Brain 블렌드 제거 및 즉시 출력 갱신.
- 기존 파일의 문자 인코딩을 보존하고 해당 메서드만 변경했다.

### BossModel

- `Update()`: 타겟 없음·사망·비활성 상태에서 추적 중지.
- `Chase()`, `StopChase()`: 비활성 Agent 및 없는 Animator 방어.

### D1_MiddleBoss

- `Start()`, `BeginCombat()`, `ConfigureChase()`: 이동 속도와 근접 공격보다 짧은 정지 거리 설정.
- `LateUpdate()`: 감지 범위 내 플레이어를 인식해 전투 시작. 상층 대기 위치, 조작 잠금, 연출 및 사망 상태에서는 시작하지 않는다.
- `TryGetSwoopDestination()`, `Swoop()`, `SwoopPattern`: 공격 예고 후 고정된 표적 지점으로 이동하고 한 번 타격하는 신규 패턴.
- `CanHit`: 죽었거나 비활성·외부 조작 잠금 상태인 플레이어에게 피해를 주지 않는다.

### D1_FinalBoss / D1_FinalBoss60Presentation

- `PlaySpecial3Timeline()`, `PrepareCameraFrame()`: 첫 Timeline 프레임과 카메라 출력을 페이드 해제 전에 평가.
- `WarpPlayerTo()`: 순간이동 직후 기본 플레이어 카메라를 즉시 갱신.
- `Special_MiddleBoss()`: 공격 거리 기반의 별도 인식 로직 제거. 인식은 박쥐가 담당한다.
- `OnPlayed()`, `RestorePresentation()`: 연출 Brain 상태 초기화 및 종료 시 비활성화.

---

## 주요 변경 내용

1. 컷 전환에서 이전 블렌드·댐핑이 남지 않도록 하며, 순간이동 뒤 첫 카메라 출력도 새 위치를 사용한다.
2. 박쥐가 감지 거리에서 스스로 전투를 시작하고 근접 공격 거리까지 추적한다.
3. 신규 급강하형 돌진은 표적 위치를 먼저 예고한다. 플레이어가 이동하면 예고 지점을 그대로 공격하며, NavMesh 경계를 가로지르는 경로는 선택하지 않는다. 사망·중단 시 이동과 예고 오브젝트를 정리한다.
4. 사망 클립의 위치·회전 곡선 6개를 루트에서 시각용 `Armature`로 옮겼다. 기본 골격 회전과 전투 스케일 10을 반영해 위치·회전 값을 보정했다. 이로써 이동 루트와 Collider가 애니메이션에 끌려가지 않는다.
5. 기존 수동 공격 테스트는 자동 인식과 충돌하지 않도록 준비 단계에서만 AI를 억제했다. 실제 자동 인식은 별도 회귀 테스트에서 타겟·전투 시작 플래그 주입 없이 검증한다.

Scene 및 Prefab 파일 변경: 없음. AnimationClip 한 개의 바인딩 변경만 있으며 기존 참조, Serialize 필드 이름, 클래스 이름은 유지했다. 신규 패턴 설정 필드 6개는 추가만 했다.

---

## Public API 변경

Production public API의 추가·삭제·시그니처 변경 없음. 기존 `ChangeCam(int, bool)`의 컷 전환 동작을 보완했다. 내부 전용 `PrepareCameraFrame()`과 테스트 메서드를 추가했다.

---

## Multi-Agent 작업

- 사용 여부: 사용
- Main Agent: 원인 분석, 모든 파일 수정, Unity MCP 검증, 테스트 실행, 결과 통합 및 DevLog 작성.
- Review Agent: 감지 조건·정지 거리·카메라 상태·API·Serialize 데이터 위험의 독립 분석 및 코드 리뷰. 파일 수정 없음.
- Test/QA Agent: 기존 테스트 누락 분석, 실제 Brain 검증·자동 인식·상층 대기 차단·패턴 회피 및 취소 테스트 제안. 파일 수정·Unity 테스트 실행 없음.
- 주요 지적 및 반영: 공격 거리 기반 인식과 긴 정지 거리 수정, 기존 세 공격과 구분되는 패턴 추가, 실제 Brain을 사용하는 위치·블렌드 검증, 수동 타겟 주입 없는 인식·이동 검증, 루트 애니메이션 재발 방지 검사 반영.
- Unity 최종 검증과 PASS 판정은 Main Agent가 직접 실행한 결과만 사용했다.
- 최종 독립 리뷰 한계: Review Agent의 최종 diff 읽기가 사용자 중단으로 완료되지 않았다. 기존 코드 분석과 전달받은 결과에서 새 차단 결함은 보고하지 않았으나 최종 구현 전체를 독립 검토했다고 간주하지 않는다. Main Agent가 최종 diff, 애니메이션 경로·값, 생성된 사망 화면 및 실제 테스트 결과를 직접 확인했다.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

작업 전후 `Assets/01. Scene/Title.unity`, dirty=False, playing=False. Main Camera, Directional Light, TitleUI-Image, EventSystem, TitleManager, Canvas, TitleInput 루트를 확인했다. 테스트에서는 실제 등장 프리팹과 공유 박쥐 액터를 생성해 전환·취소·재시도를 검증했다.

### Compile

Result: PASS

Unity Asset Refresh 후 `isCompiling=False`, `scriptCompilationFailed=False` 확인.

### Console

- 최종 최근 5분 Error: 0
- 최종 최근 5분 Exception: 0
- Warning: 테스트용 Animator의 AttackSpeed/Die/Revive 파라미터 누락, 테스트 종료 시 NavMesh 제거, 비활성 Animator 갱신, 의도적으로 영상 재생기를 생략한 폴백 경고 등이 기록됐다. 관련 테스트는 통과했다. Warning 전체가 0이라고 판정하지 않았다.
- 전체 캐시에는 초기 테스트 클래스 필터를 지원되지 않는 방식으로 호출한 MCP 도구 오류가 남았다. 이후 전체 EditMode 실행 및 PlayMode의 정확한 메서드 필터를 사용해 실제 실행을 확인했다. 이 로그는 게임 Runtime/컴파일 오류와 구분한다.

---

## EditMode Tests

Result: PASS

Passed: 115

Failed: 0

Not Run: 0

전체 EditMode 테스트 최종 실행 완료. 신규 루트 애니메이션 바인딩 검사와 기존 컷씬·보스 설정·라이프사이클 테스트 포함.

---

## PlayMode Tests

Result: PASS (관련 테스트)

Passed: 14개 고유 테스트

Failed: 0 (각 테스트의 최종 실행 기준)

Not Run: 관련 범위 외 76개. 전체 PlayMode 90개를 실행했다고 해석하지 않는다. 도구의 Summary.TotalTests는 필터 적용 전 검색 개수이므로 실제 PassedTests와 개별 결과를 기준으로 집계했다.

- BossFourIssuesPlayModeTests: 7개 — 자동 감지·근접 추적, 급강하 단일 타격·회피·중단·사망 정리, 워프 후 카메라 컷, 컷씬 첫 프레임·재시도, 크기·카메라 복구, 기존 짧은 공격, 엔딩 정리.
- Jester60IntroductionPlayModeTests: 3개 — IntroductionFinishesBeforeMiddleBossStarts, SharedActorSurvivesCancellationRetryAndBinderDisable, PooledBossBindsAndCancellationRestoresCameraAndMusic.
- Boss60CorrectionsPlayModeTests: 2개 — ActualBatCastsOncePerTargetAndStopsOnDeath, MusicResumesOnlyAfterMiddleBossDeathAndReturn.
- BossLifecyclePlayModeTests: 2개 — DeathStopsAttacksAndWaitsBeforeReturn, PoolReuseResetsDeathAndSpecialState.

초기 이동·급강하 테스트 실패는 루트 애니메이션 결함을 발견하는 근거가 됐다. 수정 후 통과했다. 기존 주문 테스트는 자동 인식 전에 테스트 대상 패턴을 고정하도록 준비 순서를 수정했고 재실행에 통과했다. 중단 위치 검사는 NavMesh 표면 높이 보정 약 0.08을 허용하되 공격 이동은 허용하지 않도록 했다.

---

## 발견된 문제

사망 클립의 루트 바인딩이 평상시 이동도 고정하는 추가 결함을 발견해 함께 수정했다. 테스트 도구의 클래스 필터가 일부 전역 namespace 클래스에 매칭되지 않아 정확한 메서드 필터와 전체 EditMode 실행으로 검증했다.

---

## 남은 문제

관련 테스트에서 미해결 기능 실패 없음. 테스트용 설정에서 발생하는 기존 Warning 정리는 이번 범위에서 제외했다.

---

## 미검증 항목

- 던전을 처음부터 끝까지 직접 조작하는 전체 수동 플레이는 수행하지 않았다.
- 요청과 직접 관련 없는 PlayMode 76개는 실행하지 않았다.
- 모든 다른 컷씬의 화면 구도를 일괄 점검하지 않았다. 검증한 범위는 박쥐 등장 전환, 플레이어 워프·카메라 컷, 취소·재시도와 관련 보스 흐름이다.

---

## 최종 결과

카메라 전환 상태 초기화, 박쥐 자동 인식·추적, 신규 급강하형 돌진을 구현했다. 루트 애니메이션의 이동 방해도 제거했다. Unity 컴파일 및 전체 EditMode 115개, 관련 PlayMode 14개가 통과했다. Git diff whitespace 검사 통과. commit/push는 수행하지 않았다.
