# 박쥐 보스 진입 및 재도전 수정

## 작업 목적

박쥐 보스 점프 착지 거리를 줄이고, 전투 중 사망 후 재도전할 때 입장 연출과 전투 지역이 사라지는 문제 및 플레이어 스킬 입력 잠김을 수정한다.

---

## 분석

- 제스터 보스의 사망 초기화가 `OnActionsStopped()`를 호출하고, 이 경로에서 `FinishEncounter()`가 씬의 박쥐 전투 액터와 연출 맵을 비활성화했다.
- 사망 후 섹터가 보스를 다시 생성하면 입장 프레젠테이션 인스턴스에 연출 완료 상태가 남지 않아 동영상/타임라인이 다시 시작됐다.
- 던전 부활 루틴은 부활 대기 후 `SetControlable(false)` 상태를 해제하지 않아 스킬을 포함한 입력이 잠긴 채 남을 수 있었다.
- `MiddleBossJumpLanding`은 박쥐 보스 스폰 지점에서 약 80 유닛 떨어져 있었다.

---

## 수정 파일

- `Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss.cs`
- `Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss60Presentation.cs`
- `Assets/02. Scripts/Enemy/Dungeon/DungeonManager.cs`
- `Assets/03. Prefab/Dungeon/Jester60Intro/Jester60Introduction.prefab`
- `Assets/02. Scripts/Enemy/SO/D1_FinalBoss.asset`
- `Assets/01. Scene/Map1-Forest.unity`
- `Assets/02. Scripts/Character/Skill/Skills/Datas/Skill_1.asset`
- `Assets/02. Scripts/Character/Skill/Skills/Datas/Skill_2.asset`
- `Assets/02. Scripts/Character/Skill/Skills/Datas/Skill_3.asset`
- `Assets/02. Scripts/Character/Skill/Skills/Datas/Skill_4.asset`

---

## 수정 클래스 / 함수

### D1_FinalBoss

- `Special_MiddleBoss()`
- `OnActionsStopped()`

### D1_FinalBoss60Presentation

- `BeginEncounter()` 인접 상태/완료 기록
- `MarkMiddleBossIntroductionCompleted()`

### DungeonManager

- `ReplaceSequence()`

---

## 주요 변경 내용

- 박쥐 보스 입장 연출이 완료된 상태를 씬 프레젠테이션 오브젝트가 유지한다. 재도전은 입장 동영상과 타임라인을 건너뛰고 전투만 다시 시작한다.
- 플레이어 사망으로 보스 초기화가 발생할 때는 완료된 박쥐 전투 지역을 종료하지 않아 카메라/맵이 검게 사라지지 않도록 했다.
- 부활 대기 3초가 끝나면 플레이어 조작을 복구해 스킬 입력도 다시 가능하게 했다.
- 점프 착지 마커의 Z 좌표를 `-11.526245`에서 `48`로 이동했다. 보스 스폰 지점과의 거리는 약 80 유닛에서 약 21 유닛으로 줄어든다.
- 스킬 1~4 쿨타임을 순서대로 6초, 10초, 16초, 22초로 설정했다. 스페이스/아이덴티티 스킬의 쿨타임은 유지했다.
- 제스터 기본 체력을 10,000에서 20,000으로 높였다.
- Map1-Forest의 보스 던전 포탈 입장 레벨을 5에서 10으로 변경했다.

---

## Public API 변경

없음. 새 완료 기록 메서드와 프로퍼티는 `internal`이다.

---

## Multi-Agent 작업

- 사용 여부: 미사용
- Main Agent 역할: 분석, 수정, Unity 확인 및 통합
- 주요 리뷰 지적사항: 해당 없음
- 반영 여부와 이유: 해당 없음

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PARTIAL

Title 씬이 열려 있는 것을 확인했고, Circus-Main-Hall 씬을 Additive로 열어 `Jester60Introduction/Presentation/60%Map/MiddleBossDropJump`와 `MiddleBossJumpLanding` 계층을 확인했다. 해당 씬은 저장 변경 없이 닫았고, Editor에는 Title 씬만 남아 있다.

### Compile

Result: PASS — Unity AssetDatabase 갱신 성공. 갱신 후 기존 컴파일 경고가 출력됐고, 최근 2분 Console Error는 0건이다.

### Console

Errors: 최근 2분간 0건

Warnings: 기존 미사용 이벤트/필드 관련 C# 경고가 기록됨. 이번 수정 파일에서 발생한 경고는 확인되지 않음.

Exceptions: 최근 2분간 0건

---

## EditMode Tests

Result: NOT RUN

Not Run: 이번 작업에서 자동 테스트를 실행하지 않음.

## PlayMode Tests

Result: NOT RUN

Not Run: 이번 작업에서 자동 테스트를 실행하지 않음.

---

## 발견된 문제

부활 입력 복구가 빠져 있어 이동/공격/스킬 입력이 잠길 수 있는 경로를 확인했다.

## 남은 문제

실제 플레이에서 착지 위치, 스킬 입력, 사망 후 재도전 장면을 검증하지 않았다.

## 미검증 항목

- Console 전체 과거 이력 분류 (최근 2분 Error/Exception 0건은 확인)
- EditMode / PlayMode 테스트
- 런타임에서 박쥐 보스 진입 및 사망 후 재도전

## 최종 결과

코드, 스킬 쿨타임, 제스터 체력, 포탈 입장 레벨과 점프 착지 마커를 수정했다. Unity 에셋 갱신은 성공했고 최근 Console Error/Exception은 0건이다. 런타임 및 자동 테스트 검증은 아직 완료되지 않았다.
