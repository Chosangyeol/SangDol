# Gameplay 버그 수정

## 작업 목적

확인된 스킬 슬롯 교체, 치명타 피해, 스킬 다중 콜라이더 피해, 씬 전환 스폰 처리, 박쥐 보스 패턴 선택 문제를 수정한다.

---

## 분석

- 스킬 슬롯에서 드롭 교환 후 드래그 종료 코드가 출발 슬롯을 다시 비우고 있었다.
- 스킬 공격 코드가 치명타 여부만 전달하고 피해량에 치명타 배율을 반영하지 않았다.
- 한 적에 여러 Enemy 레이어 콜라이더가 겹치면 Overlap 결과의 각 콜라이더마다 피해를 적용했다.
- `SceneChanger`는 로딩 중 비활성화한 NavMeshAgent에 `Warp`를 호출했고 반환값을 확인하지 않았다.
- 박쥐 보스는 직전 일반 패턴을 후보에서 제외한 뒤에도 난수 범위를 이전 가중치 합계로 유지했다.

---

## 수정 파일

- `Assets/02. Scripts/UI/Skill/SkillSlot.cs`
- `Assets/02. Scripts/Character/PlayerAttackContainer.cs`
- `Assets/02. Scripts/Character/Skill/Skills/SkillBase/Skill_4.cs`
- `Assets/02. Scripts/System/SceneChanger.cs`
- `Assets/02. Scripts/Enemy/BossModel.cs`
- `Docs/DevLogs/2026-10-28_GameplayBugFixes.md`

작업 전부터 여러 기존 변경사항이 작업 트리에 있었으며, 이를 되돌리지 않았다.

---

## 수정 클래스 / 함수

### SkillSlot

- `OnDrop()`
- `OnEndDrag()` 동작을 보장하는 드롭 상태 갱신

### PlayerAttackContainer

- `AnimEvent_ExevuteSkillAttack()`

### Skill_4

- `ExecuteHoldingTick()`

### SceneChanger

- `MovePlayerToSpawnPoint()`

### BossModel

- `SelectNextNormalPattern()`

---

## 주요 변경 내용

- 유효한 스킬 슬롯 드롭은 출발 슬롯의 드래그 완료 플래그를 설정해 교환된 스킬이 지워지지 않게 했다. 자기 슬롯에 놓아도 유지하며, 변경 불가 슬롯에 거부된 드롭도 출발 스킬을 지우지 않는다.
- 공통 스킬 타격과 Skill 4 홀딩 타격에서 치명타가 나오면 기본 공격과 같이 `criticalDamage` 배율을 적용한다.
- 공통 스킬 타격과 Skill 4의 각 타격 틱에 `HashSet<EnemyBase>` 중복 검사를 추가해 한 타격 이벤트에서 적당 한 번만 피해를 준다.
- 씬 로딩 중 비활성 상태인 NavMeshAgent에는 `Warp`를 시도하지 않는다. 워프를 실행할 수 없거나 실패하면 Agent를 끈 상태로 스폰 위치를 적용하고, 2m 안의 유효 NavMesh 위치를 우선 선택한다. 가까운 NavMesh가 없으면 설정된 SpawnPoint 위치를 적용하고 경고를 남긴다.
- 박쥐 보스의 직전 일반 패턴을 후보 목록에서 실제로 제거한 경우 해당 패턴의 가중치도 총합에서 뺐다. 후보가 없거나 총 가중치가 0 이하인 경우 선택하지 않는다.

---

## Public API 변경

없음

---

## Multi-Agent 작업

- 사용 여부: 사용
- Main Agent 역할: 코드 수정, 범위 결정, Unity Refresh/컴파일 확인 및 최종 검토
- 사용한 Agent와 역할:
  - Review Agent: 슬롯 드롭, 스킬 치명타/중복 피해, 스폰 폴백 코드 검토
  - Test/QA Agent: 관련 테스트 범위 및 보스 패턴 가중치 결함 분석
- 주요 리뷰 지적사항:
  - 변경 불가 슬롯에 놓는 경우 출발 스킬이 삭제될 수 있음
  - SpawnPoint와 NavMesh가 2m 이상 떨어져 있으면 Agent가 NavMesh에 올라오지 않을 수 있음
  - 직전 패턴 제외 후 가중치 총합도 조정해야 함
- 반영 여부와 이유:
  - 변경 불가 슬롯 드롭도 출발 스킬을 유지하도록 반영했다.
  - 실패 폴백에서 2m 이내 NavMesh 위치를 샘플링하도록 반영했다. 2m 안에 NavMesh가 없으면 설정된 위치로 이동시키고 경고를 남긴다.
  - 제외한 패턴의 가중치를 총합에서 빼도록 반영했다.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

Unity MCP로 확인한 열린 Scene은 `Main`이며 Dirty 상태가 아니었다.

### Compile

Result: PASS

Unity AssetDatabase ForceUpdate 후 Refresh 성공을 확인했다. 최종 상태 조회에서 `isCompiling=False`, `isUpdating=False`였다.

### Console

Errors: Console 조회에는 첫 Refresh 시점의 `Skill_4.cs` `HashSet<>` using 누락 오류 2건이 남아 있다. using 추가 후 실행한 Refresh는 성공했으며, 조회된 오류 기록에는 새로운 컴파일 오류가 없었다.

Warnings: 이번 수정으로 새 경고가 발생했다는 기록은 확인되지 않았다.

Exceptions: 확인된 예외 없음

---

## EditMode Tests

Result: NOT RUN

Passed: 0

Failed: 0

Not Run: 전체

---

## PlayMode Tests

Result: NOT RUN

Passed: 0

Failed: 0

Not Run: 전체

---

## 발견된 문제

- Scene spawn 위치가 NavMesh에서 2m 이상 벗어나 있으면 원래 SpawnPoint를 사용하므로 Agent가 NavMesh에 재연결되지 않을 수 있다. 현재 씬에서 해당 상태가 발생하는지는 런타임 검증하지 않았다.
- 기존 테스트에는 다중 콜라이더 적의 실제 피해 횟수, 스킬 치명타 배율, 박쥐 패턴의 직전 항목 제외 가중치 합계를 직접 검증하는 항목이 없다.

---

## 남은 문제

런타임 플레이와 씬 스폰 지점/NavMesh 일치 여부는 검증하지 않았다.

---

## 미검증 항목

- EditMode/PlayMode 테스트는 요청되지 않아 실행하지 않았다.
- PC/WebGL 빌드는 요청되지 않아 실행하지 않았다.
- 보스 런타임 패턴 빈도와 씬 전환 후 실제 스폰 위치는 플레이 검증하지 않았다.

---

## 최종 결과

5개 production 파일에서 5종 버그 경로를 수정했다. Unity Refresh와 컴파일 상태는 통과했으며 게임 테스트와 빌드는 실행하지 않았다.
