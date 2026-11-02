# 필드 일반 몬스터 밸런스 및 귀환 포탈

## 작업 목적

일반 몬스터의 체력과 공격력을 30% 낮추고, 빠른 이동 도착지에서 마을로 돌아갈 수 있는 포탈을 추가한다. 필드보스 이동 포탈은 숲 맵 언덕 위치로 옮긴다.

---

## 분석

일반 몬스터 데이터는 `Enemy/SO/Normal`의 6개 `EnemyStatSO`에 있다. 제스터와 시계 박쥐는 별도 보스 에셋이므로 수정하지 않았다. 숲 씬의 빠른 이동은 `BossDungeonShortcutPortal`을 사용하며, 포탈을 복제해 귀환용으로 쓰되 체험 레벨 및 포션 지급은 실행하지 않도록 분기했다.

## 수정 파일

- `Assets/02. Scripts/Enemy/SO/Normal/WolfSO.asset`
- `Assets/02. Scripts/Enemy/SO/Normal/PumpkinGaySO.asset`
- `Assets/02. Scripts/Enemy/SO/Normal/OrcSO.asset`
- `Assets/02. Scripts/Enemy/SO/Normal/MiniClownSO.asset`
- `Assets/02. Scripts/Enemy/SO/Normal/GolemSO.asset`
- `Assets/02. Scripts/Enemy/SO/Normal/DevilGolemSO.asset`
- `Assets/02. Scripts/System/SceneMove/BossDungeonShortcutPortal.cs`
- `Assets/01. Scene/Map1-Forest.unity`

## 수정 클래스 / 함수

### BossDungeonShortcutPortal

- `Interact()` — `applyPortalTrialSetup`이 켜진 빠른 이동 포탈만 체험 세팅을 적용한다. 기존 포탈은 기본값 `true`를 유지한다.

## 주요 변경 내용

일반 몬스터의 기존 수치에 0.7을 곱해 정수 단위로 조정했다.

| 몬스터 | 체력 | 공격력 |
|---|---:|---:|
| 변이 늑대 | 300 → 210 | 40 → 28 |
| 펌킨 가이 | 4000 → 2800 | 180 → 126 |
| 오크 | 1500 → 1050 | 110 → 77 |
| 미니 크라운 | 400 → 280 | 80 → 56 |
| 일반 골렘 | 2000 → 1400 | 150 → 105 |
| 데빌 골렘 | 5000 → 3500 | 240 → 168 |

숲 씬의 제스터·골렘 도착 지점에 `JesterReturnPortal`, `GolemReturnPortal`을 배치했다. 두 포탈 모두 `G - 마을로 돌아가기`로 안내하고 마을 시작 지점으로 이동하며, 체험 레벨/아이템 초기화는 비활성화했다. 필드보스행 `GolemBossJumpPortal`은 마을 위치(194.95, 0.76, 207.28)에 두고, `GolemBossJumpPortalDestination`과 `GolemReturnPortal`은 언덕의 `Player-Start (2)` 위치(218.28, -0.06, 55.56)에 맞췄다.

## Public API 변경

없음. `applyPortalTrialSetup`은 private serialized 필드이며 기본값 `true`라 기존 포탈 동작은 유지된다.

## Multi-Agent 작업

- 사용 여부: 미사용
- Main Agent가 수정과 Unity 씬 검증을 담당했다.

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS — `Map1-Forest`에서 귀환 포탈 2개, 필드보스행 포탈, 언덕 도착/귀환 위치를 확인하고 씬을 저장했다.

### Compile

Result: PASS — 스크립트 AssetDatabase 갱신 후 컴파일 오류 로그는 확인되지 않았다.

### Console

Errors: Console에 남은 MCP 스크린샷 도구 오류 1건 (`Terrain` 대상에 Renderer가 없어 bounds 계산 실패). 게임 코드 오류와는 무관하다.

Warnings: 기존 미사용 이벤트/필드 경고가 있음.

Exceptions: 변경 코드 관련 예외 없음.

## EditMode Tests

Result: NOT RUN — 요청되지 않음.

## PlayMode Tests

Result: NOT RUN — 요청되지 않음.

## 빌드

NOT RUN — 별도 빌드 요청이 없어 실행하지 않음.

## 발견된 문제

씬 촬영 도구가 `Terrain` 오브젝트에 렌더러가 없다는 오류를 반환했다. 씬 저장 및 데이터 확인은 정상 완료했다.

## 남은 문제 / 미검증 항목

PlayMode에서 실제 왕복 이동과 포탈 상호작용 표시는 아직 확인하지 않았다.

## 최종 결과

일반 몬스터 6종을 30% 너프하고, 두 빠른 이동 도착지에 마을 귀환 포탈을 추가했다. 필드보스행 포탈은 마을에 두고 도착 지점과 귀환 포탈을 언덕 위치로 맞췄다.
