# Jester Special3 중간 보스 패턴

## 작업 목적

Jester의 체력 60% 특수 패턴을 기존 칩막기에서 중간 보스 조우 패턴으로 변경하고, 맵 구역 이동·영상·중간 보스 전투·복귀 흐름의 런타임 로직을 준비한다.

---

## 분석

기존 `Special_Chip()`은 공용 연출인 `ReadyForSpecial()`과 `EndSpecial()`만 호출했다. `Special3` 데이터에는 프리팹 슬롯 하나만 있었고, 중간 보스 구역 및 영상/위치 연결은 아직 만들어지지 않았다. 현재 Unity Editor의 열린 씬은 `Circus-Main-Hall`이며, 중간 보스와 대기/스폰 지점은 아직 존재하지 않는다.

---

## 수정 파일

- `Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss.cs`
- `Assets/02. Scripts/Enemy/BossModel.cs`
- `Assets/03. Prefab/Enemy/Boss/D1/Final/Jester.prefab`

---

## 수정 클래스 / 함수

### D1_FinalBoss

- `StartSpecialPattern()`
- `IsSpecial3Configured()`
- `MoveBossToCenter()`
- `WarpPlayerTo()`
- `Special_MiddleBoss()`
- `ResetBossState()`는 기존 사망 초기화 흐름을 유지

### BossModel

- `Start()`에서 사전에 지정된 `bossSpawnPoint`를 유지하도록 변경

---

## 주요 변경 내용

- `Special_Chip()`을 `Special_MiddleBoss()`로 교체하고 패턴 키를 `중간 보스`로 변경했다. Jester 프리팹의 HP 임계값 `0.6`은 그대로 유지했다.
- 패턴 시작 시 Jester가 NavMesh 경로를 따라 중앙으로 이동하고, 도착 후 영상 클립을 재생한다. 경로 계산이 실패하거나 시간 초과 시 중앙 위치로 보정한다.
- 영상 종료 후 중간 보스 프리팹을 지정 스폰 지점에 생성하고 플레이어를 대기 지점으로 이동시킨다. 플레이어와 중간 보스 사이의 수평 거리가 중간 보스 `attackRange` 이내가 되면 `isCombatStarted`를 켠다.
- 중간 보스 사망을 확인하면 플레이어를 지정 복귀 지점으로 이동시키고 Jester의 무적/특수 패턴 상태를 해제한다. 플레이어 사망 시에는 기존 `GameEvent.OnPlayerDie`와 `DungeonManager` 부활 흐름을 사용한다.
- 필수 참조가 아직 연결되지 않은 준비 상태에서는 경고를 남기고 패턴을 건너뛴다. Jester 프리팹의 기존 `Special3.prefab` 필드는 보존하고, 이후 중간 보스 프리팹으로 연결하도록 했다.
- `BossModel.Start()`는 호출자가 미리 지정한 보스 스폰 지점을 덮어쓰지 않도록 변경했다. 이 값은 중간 보스가 씬의 전역 `BossSpawnPos`로 이동하는 것을 막는 데 필요하다.

---

## Public API 변경

`D1_Final_Special3Data`에 다음 public 필드를 추가했다.

- `VideoClip cutsceneClip`
- `Transform waitingArea`
- `Transform middleBossSpawnPoint`
- `Transform returnPoint`

`prefab` 필드는 유지했다. 향후 Jester 프리팹 Inspector에서 영상 클립, `D1_MiddleBoss` 프리팹, 대기 지점, 스폰 지점 및 선택 복귀 지점을 연결해야 한다. 복귀 지점을 생략하면 기존 `PlayerStart` 위치를 사용한다.

---

## Unity 검증

### Compile

Result: PASS

Unity MCP AssetDatabase Refresh 완료. EditMode/PlayMode 테스트도 컴파일 후 실행 완료.

### Console

Errors: 최근 확인 결과 1건. Unity MCP가 읽기 전용 `.agents/skills`에 `assets-copy/SKILL.md`를 생성하려다 실패한 도구 시작 오류이며, 변경 코드의 컴파일/런타임 오류는 확인되지 않았다.

Warnings: 기존 미사용 이벤트/필드 컴파일 경고와 PlayMode 테스트 중 NavMesh 미구성 및 Animator 전환 경고가 확인됐다. 수정한 스크립트에서 발생한 컴파일 경고는 없다.

Exceptions: 변경 코드 관련 Exception 없음.

---

## EditMode Tests

Result: PASS

Passed: 20

Failed: 0

Not Run: 없음

---

## PlayMode Tests

Result: PASS

Passed: 7

Failed: 0

Not Run: 없음

---

## 발견된 문제

- 중간 보스 씬 구역, 프리팹, 영상 클립과 위치 참조가 아직 없어 Inspector 연결 전까지 실제 패턴 흐름은 의도적으로 건너뛴다.
- 현재 구현은 별도 Unity Scene을 로드하지 않고 동일 씬 안의 다른 구역으로 워프하는 구조다. 중간 보스 맵은 Jester 씬에 추가되는 것을 전제로 한다.

---

## 남은 문제

- 중간 보스 프리팹은 `D1_MiddleBoss`와 유효한 `EnemyStatSO`/NavMeshAgent를 포함해야 한다.
- 중간 보스의 전투 AI/공격 패턴 자체와 공격 범위 값은 프리팹 및 스탯 데이터 구성 시 확인해야 한다.
- 플레이어 사망 후 기존 던전 체크포인트 복귀는 유지되며, 패턴 재시작 시 재사용될 중간 보스는 대기 상태로 초기화된다.

---

## 미검증 항목

현재 씬에 필요한 맵/프리팹/영상/위치 데이터가 없어 Special3의 실제 인게임 연속 흐름과 사망 후 재진입을 PlayMode에서 검증하지 못했다. 기존 EditMode/PlayMode 테스트에는 보스 Special3 전용 테스트가 없다.

---

## 최종 결과

Special3 코드 흐름과 Inspector 설정 슬롯을 준비했다. HP 60% 임계값은 보존했고, 실제 영상 및 맵 자산이 연결되기 전 상태에서도 패턴이 입력을 고정하지 않도록 방어 처리를 추가했다. Unity Refresh, EditMode 20개, PlayMode 7개는 통과했다.
