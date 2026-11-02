# 아이덴티티 게이지 적중 및 사용 조건 수정

## 작업 목적

Z 아이덴티티는 게이지가 가득 찼을 때만 사용하고, 사용 시 전량 소모한다. 기본 공격과 스킬의 적중 보상을 지정한 수치로 적용하며 다단히트 스킬은 시전당 한 번만 지급한다.

---

## 분석

- 공격 적중 경로에서 `GainIden()`을 호출하지 않았다.
- `Skill_Z.UseSkill()`은 게이지 확인 전에 기본 스킬 사용 처리와 쿨다운 소비를 수행했고 최대치를 100으로 고정했다.
- 실제 기본 공격 콤보는 1~4이지만 기존 판정에서 3타가 4타 경로로 처리됐다.
- 스킬 1~4의 기존 `idenGain` 에셋 값은 요청 수치와 달랐다.

---

## 수정 파일

- `Assets/02. Scripts/Character/CharacterModel.cs`
- `Assets/02. Scripts/Character/PlayerAttackContainer.cs`
- `Assets/02. Scripts/Character/Skill/SkillBase.cs`
- `Assets/02. Scripts/Character/Skill/Skills/SkillBase/Skill_2.cs`
- `Assets/02. Scripts/Character/Skill/Skills/SkillBase/Skill_4.cs`
- `Assets/02. Scripts/Character/Skill/Skills/SkillBase/Skill_Z.cs`
- `Assets/02. Scripts/Character/Skill/Skills/Datas/Skill_1.asset` ~ `Skill_4.asset`
- `Assets/Tests/Editor/GameplayAuditTests.cs`
- `Assets/Tests/PlayMode/GameplayAuditPlayModeTests.cs`

---

## 수정 클래스 / 함수

- `CharacterModel.OnAttackHit()` / `HandleBasicAttack4()` / `GainIden()` / `ResetIden()` / `IdenEnable()`
- `PlayerAttackContainer.AnimEvent_ExevuteSkillAttack()`
- `SkillBase.UseSkill()` / `GrantIdentityOnHit()` / `CancelIdentityGain()` / `InterruptExecution()`
- `Skill_2.JumpRoutine()` / `Skill_4.ExecuteHoldingTick()` / `Skill_Z.UseSkill()`

---

## 주요 변경 내용

- 기본 공격 1~3타는 살아 있는 적의 유효 적중마다 2, 4타는 4를 지급한다. 같은 공격에서 한 적의 여러 콜라이더는 기존 중복 제거를 유지한다.
- 스킬 1~4 에셋의 `idenGain`을 각각 5, 10, 15, 20으로 설정했다. 스킬 시전 성공 시 지급 상태를 열고 첫 유효 적중에만 지급한다. 반복 적중과 여러 대상은 추가 지급하지 않는다.
- 죽은 적과 무적 보스는 게이지 지급 대상에서 제외한다. 2번 스킬의 점프가 중단되면 남은 지급 상태를 닫는다.
- Z는 현재 게이지가 `idenMax` 이상일 때만 기본 스킬 사용 처리를 하고, 게이지를 0으로 만든 뒤 버프를 적용한다.
- 게이지 증감 시 `OnStatChange`를 발행해 UI를 갱신한다. 아이덴티티 이펙트 풀이 없을 때도 버프 사용 흐름이 예외로 중단되지 않도록 했다.

---

## Public API 변경

없음. 기존 public 메서드와 직렬화 필드 이름을 유지했다. `SkillBase` 내부 메서드만 추가했다.

---

## Multi-Agent 작업

- 사용 여부: 사용
- Main Agent: 수정 범위 결정, 구현, Unity 검증 및 통합
- Review Agent: 적중 조건, 중단 시 상태, API 및 직렬화 위험 독립 검토. 2번 스킬 취소 후 늦은 이벤트 위험을 반영했다.
- Test/QA Agent: 적중 경로와 콤보 번호를 추적하고 테스트 대상을 제안했다. 3타 판정 오류를 반영했다.
- 서브에이전트는 파일을 수정하거나 테스트를 실행하지 않았다.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

- Unity MCP로 `Title` Scene이 열려 있고 저장된 상태이며 루트 오브젝트가 7개임을 확인했다.
- 이 작업에서 Scene 또는 Prefab은 수정하지 않았다.

### Compile

Result: PASS

- Unity Asset Refresh 성공. `EditorUtility.scriptCompilationFailed == false` 확인.
- `dotnet build PlayerMovement.PlayModeTests.csproj --no-restore -clp:ErrorsOnly`: 오류 0, 경고 0.
- `dotnet build PlayerMovement.EditModeTests.csproj --no-restore -clp:ErrorsOnly`: 오류 0, 경고 0.

### Console

Errors: 게임 코드 컴파일 오류 0건. Unity MCP 스킬 파일 생성 과정에서 `.agents/skills/inputsystem-action-add/SKILL.md` 공유 위반 오류 1건이 기록됐다.

Warnings: 테스트 장면 복구 과정에서 기존 NavMesh agent 생성 경고가 기록됐다. 앞선 테스트에서 발생한 테스트용 Animator의 `AttackSpeed` 파라미터 경고는 fixture를 보완한 재실행에서 재발하지 않았다.

Exceptions: Unity MCP 스킬 파일 생성 과정의 공유 위반 예외 1건. 게임 실행 코드에서 발생한 예외는 확인되지 않았다.

---

## EditMode Tests

Result: NOT RUN

이번 변경은 실제 공격 애니메이션, 콜라이더와 버프 적용을 확인하는 PlayMode 경로가 핵심이므로 관련 PlayMode 테스트를 실행했다.

---

## PlayMode Tests

Result: PASS

- `GameplayAuditPlayModeTests.BasicAttackDamagesAndCountersEachEnemyOnce`: PASS
- `GameplayAuditPlayModeTests.IdentitySkillConsumesOnlyAtFullGauge`: PASS
- `GameplayAuditPlayModeTests.SpinningSkillGrantsIdentityOnceAcrossRepeatedHits`: PASS
- `GameplayAuditPlayModeTests.SkillHitGrantsIdentityOnceAfterFirstValidTarget`: PASS

Unity MCP 테스트 도구의 class 필터는 실행 결과 0건을 반환하여, 위 4개는 method 필터로 각각 실제 실행해 결과를 확인했다. Unity AssetDatabase에서 스킬 1~4의 에셋 지급값도 5/10/15/20으로 확인했다.

---

## 발견된 문제

기존 콤보 3타가 4타 판정 경로를 사용하고 있었다. 이번 수정에 포함했다.

---

## 남은 문제

Unity MCP가 재시작될 때 스킬 파일 생성 공유 위반이 간헐적으로 기록된다. 이번 게임 코드 변경과 무관하며 관련 PlayMode 테스트 실행은 가능했다.

---

## 미검증 항목

실제 플레이 화면에서 모든 스킬 1~4를 직접 조작하는 수동 확인은 수행하지 않았다. 스킬 1·3은 공통 적중 경로와 에셋 지급값을 확인했으나 개별 PlayMode 사례는 추가하지 않았다.

---

## 최종 결과

요청한 게이지 소모·획득 수치와 다단히트 시전당 1회 지급을 구현했다. 관련 PlayMode 테스트 4개가 통과했다.
