# 플레이어 이동 및 스킬 연결 개선

## 작업 목적

이동 재개 실패, 급격한 방향 전환, 이동/스킬 잠금 중 입력 유실, 회피·점프 후 제어 복구를 개선하고 Unity MCP로 검증한다.

## 분석

- StopMove에서 Agent를 정지시키지만 RequestMove는 정지 상태를 해제하지 않았다.
- 이동 버튼 유지 중 매 프레임 목적지를 설정하고 클릭 지점을 즉시 바라봤다. 경로의 진행 방향과 시각적 회전이 일치하지 않을 수 있었다.
- 스킬 입력이 잠금 중 버려졌고 canSkill 검사를 하지 않았다.
- 입력 콜백에서 IsPointerOverGameObject를 호출하여 Unity 경고가 있었다.
- 회피 복구가 Dash.anim의 0.333초 SetCanMove 이벤트에 의존했다. 점프는 임의 착지 위치에서 Agent를 재활성화했다.
- 실제 PlayMode 테스트로 ResetPath 호출 후 정지 상태 재설정 필요성과 회전 완료 직전 0.5도 스냅을 발견했다.

## 수정 파일

- Assets/02. Scripts/Character/C_Controller.cs
- Assets/02. Scripts/Character/C_Input.cs
- Assets/02. Scripts/Character/C_SkillSystem.cs
- Assets/02. Scripts/Character/PlayerInputs.cs
- Assets/02. Scripts/Character/Skill/SkillBase.cs
- Assets/02. Scripts/Character/Skill/Skills/SkillBase/Skill_1.cs
- Assets/02. Scripts/Character/Skill/Skills/SkillBase/Skill_2.cs
- Assets/02. Scripts/Character/Skill/Skills/SkillBase/Skill_Space.cs
- Assets/Tests/Editor/PlayerMovementTests.cs
- Assets/Tests/EditMode/PlayerMovementEditModeTests.cs 및 PlayerMovement.EditModeTests.asmdef
- Assets/Tests/PlayMode/PlayerMovementPlayModeTests.cs 및 PlayerMovement.PlayModeTests.asmdef
- 위 테스트 파일/폴더의 Unity 생성 .meta
- Docs/DevLogs/2026-09-29_PlayerMovement.md
- Docs/DevLogs/2026-09-29_PlayerMovement_TestResults.json

## 수정 클래스 / 함수

- C_Controller: Tick, RequestMove, StopMove, FaceTo, 내부 InterruptAttackForSkill.
- C_Input: OnSkillKeyUp.
- C_SkillSystem: UseSkill, ReleaseSkill, UpdateSkills, 내부 버퍼 처리.
- PlayerInputs: Update, OnDisable, OnSkillSlotStarted, OnSkillSlotCanceled.
- SkillBase: UseSkill.
- Skill_1: SkillRoutine.
- Skill_2: UseSkill, JumpRoutine, Effect.
- Skill_Space: UseSkill, SkillActive.

## 주요 변경 내용

### 이동

- ResetPath 후 isStopped=true로 정지 상태를 확정한다. 유효한 이동 명령이 수락되면 isStopped=false로 재개한다.
- Agent 비활성/미배치 상태, 기절, 사망 중에는 이동 명령을 거부한다.
- 클릭 지점 즉시 회전 대신 Agent.desiredVelocity 방향으로 초당 360도 범위에서 회전한다. 공격/스킬의 FaceTo는 기존 즉시 조준을 유지한다.
- 목적지가 0.1m 이상 변하고 0.08초가 지났을 때 경로를 갱신한다. 정지 후 첫 이동은 대기하지 않는다.
- NavMesh에서 1m 이내 목적지를 확인한다. 가까운 목적지와 도착 판정에 작은 허용 오차를 사용한다.
- 스킬 진입 시 일반 공격 콤보 상태와 대기 회전을 정리한다.

### 입력 및 스킬 연계

- 스킬 누름/해제 콜백은 입력만 수집하고 Update에서 UI 여부를 검사한다. 해제 입력은 UI 위에서도 전달한다.
- 최신 유효 스킬 입력 1개를 0.18초 동안 보관한다. 기존 이동 잠금과 canSkill 잠금을 모두 존중하며, 잠금 해제 후 한 번 실행한다. 무제한 애니메이션 취소를 허용하지 않는다.
- 버퍼 만료, 사망, 기절, 슬롯 교체 시 폐기한다. 모델 Update가 잠시 멈춘 경우에도 Time.time 기준으로 만료된다.
- 버퍼 대기 중 키를 뗀 차지 입력은 실행 직후 해제를 전달한다.
- 마우스 지점을 구할 수 없는 경우에도 캐릭터 전방을 사용해 차지 해제를 전달한다.

### 회피·점프

- 회피 동안 이동·공격·스킬을 잠그고, 기존 Dash 애니메이션의 약 0.333초 회복 구간 후 복구한다. 기절/사망 시 이동을 중단하고, 사망 상태에서는 조작을 복구하지 않는다.
- 돌진 방향은 Y 성분을 제거한 뒤 정규화하여 높이 차이에 따른 수평 이동 거리 차이를 줄인다.
- 점프 목적지는 쿨다운 소비 전에 NavMesh에서 검증한다. 잘못된 착지는 사용을 거부한다.
- 점프 정상 종료 시 착지 및 Agent/조작 복구, 중단 시 유효했던 출발점으로 복귀한다. 사망 시 조작은 잠긴 상태를 유지한다.
- 점프 착지 이펙트의 0 길이 회전 벡터 대신 캐릭터 전방을 사용한다. 선택적 이펙트가 없는 데이터도 종료 복구를 수행한다.

## Public API 변경

기존 Production public 메서드/필드/클래스의 이름 및 시그니처 변경 없음. C_Controller에 같은 Assembly-CSharp 내부에서만 사용하는 internal InterruptAttackForSkill을 추가했다. 테스트용 public 타입은 테스트/Editor 코드에만 추가했다.

기존 SerializeField, namespace, Scene, Prefab, Animator Controller, 애니메이션 클립, Package manifest 변경 없음. Skill_2.cs의 기존 CP949 인코딩을 보존했다.

## Unity 검증

### Compile

Result: PASS

각 수정 후 MCP assets-refresh 실행. 최종 EditorApplication.isCompiling=False, isUpdating=False, EditorUtility.scriptCompilationFailed=False 확인. 실제 프로젝트 경로는 D:/UnityProjects/SangDol/Assets.

최종 상태는 편집 모드, Assets/01. Scene/Title.unity, IsDirty=False이다.

### Console

- 최종 EditMode/PlayMode 테스트 결과에 포함된 Logs: 각각 빈 배열. 최종 테스트 케이스 실행 중 Error/Exception/Warning 없음.
- 프로젝트 코드의 새 컴파일 Error: 0. 새 Exception: 0.
- Console 캐시 전체가 비어 있다는 의미는 아니다. 작업 전 Error 4, Exception 1, Warning 204가 있었다.
- 이번 작업 중 Error 1건은 첫 테스트 탐색 실패(No tests found matching class) 도구 로그이며, 테스트 assembly 등록 후 해결했다.
- 작업 중 추가 Warning 77건: 기존 미사용 필드/이벤트 10종 재컴파일 반복 60건, 기존 Main 부트스트랩 Animator/NavMesh 경고 8건, 초기 테스트 NavMesh에 기존 Main Agent가 반응한 경고 8건, 초기 비활성 테스트 Animator 경고 1건.
- 테스트 fixture의 비활성 Animator 접근을 제거하고, PlayMode 테스트 시작 시 Main 루트를 비활성화하여 테스트용 NavMesh와 분리했다. 기존 Main 부트스트랩 자체의 Animator/NavMesh 경고는 작업 범위 밖이므로 남아 있다.
- 과거 로그는 삭제하지 않았다.

## EditMode Tests

Result: PASS

Passed: 10 / Failed: 0 / Not Run: 0

MCP tests-run(testMode=EditMode, testAssembly=PlayerMovement.EditModeTests).

검증: 스킬 허용 조건, 버퍼 만료, 차지 해제 보존, 사망/기절 폐기, 슬롯 변경, 최신 입력 우선, 비활성 Agent 이동 거부, 카메라 없는 해제, 비활성 Agent 회피 거부/쿨다운 보존.

도구 Summary.TotalTests는 20으로 반환됐지만 PassedTests와 개별 Results는 10개이다. 보고서는 실제 개별 결과 10개를 기준으로 기록하며 원본 응답도 첨부한다.

## PlayMode Tests

Result: PASS

Passed: 7 / Failed: 0 / Not Run: 0

MCP tests-run(testMode=PlayMode, testAssembly=PlayerMovement.PlayModeTests).

1. 모델 업데이트 중단 동안 버퍼 만료.
2. 회피 중 사망 시 조작 잠금 유지.
3. 애니메이션 이벤트 없이 회피 복구 및 높이 차이 있는 목표로 수평 이동.
4. 회피 중 기절 시 이동 중단.
5. 점프 정상 착지 후 이동 재개.
6. 잘못된 착지 거부/쿨다운 보존 및 점프 중단 복구.
7. 실제 NavMesh 이동 재개, 반복 목적지 요청, 프레임당 회전량, 비활성 Agent 처리.

### 실패 및 재검증 이력

- 초기 Editor 기본 assembly의 테스트를 도구가 발견하지 못했다. 전용 테스트 asmdef와 브리지를 추가한 뒤 탐색 및 실행 성공.
- 첫 PlayMode: 2 PASS / 2 FAIL. 정지 상태 유지 실패 확인.
- 정지 순서 수정 후 PlayMode: 2 PASS / 2 FAIL. 회피 내부 ResetPath의 추가 정지 해제 및 회전 마지막 스냅을 확인하고 수정.
- 수정 후 4/4 PASS. 이후 사망·업데이트 중단·정상 착지 케이스를 추가하여 최종 7/7 PASS.
- 최종 코드에 대해 EditMode도 다시 실행하여 10/10 PASS.

### 테스트 구조

Production 코드가 asmdef 없는 Assembly-CSharp에 있으므로 대규모 assembly 재편을 피했다. 실제 코드에 접근하는 fixture는 기본 Editor assembly에 두고, Test Runner가 발견하는 작은 EditMode/PlayMode assembly에서 reflection 브리지로 호출한다. PlayMode 테스트는 Editor 내에서 실행하며 배포 Player 테스트가 아니다.

PlayMode는 테스트 전용 NavMesh와 실제 NavMeshAgent/CharacterModel.Awake/Coroutine을 사용한다. 모델의 일반 Start/Update는 꺼서 카메라·UI·필드 시스템 의존성을 분리하고 필요한 Tick은 명시적으로 호출한다. 실제 플레이어의 전체 애니메이션 그래프 테스트를 대신하지 않는다.

## 발견된 문제

- 위 실패 원인은 수정하고 재검증했다.
- MCP/Editor 동작 중 .codex/config.toml에 assets-refresh 승인 항목, ProjectSettings.asset에 runInBackground와 UNITY_MCP_READY/UNITY_MCP_DEPS_6 심볼 차이가 관찰됐다. 직접 수정 명령을 수행한 파일은 아니며, 게임플레이 변경과 구분한다. 사용자/도구가 관리하는 설정을 임의로 되돌리지 않았다.

## 남은 문제

- Player.prefab의 Root Motion은 공격 연출에도 영향을 주므로 일괄 해제하지 않았다. 실제 이동 클립과 Agent 위치 제어의 상호작용은 별도 확인이 필요하다.
- 기존 Animator IdenEnd 전환 및 부트스트랩 NavMesh 경고는 미수정이다.
- 스킬별 애니메이션 전환/캔슬 가능 구간과 회전 속도·버퍼 길이의 체감 튜닝은 실제 조작 확인이 필요하다.

## 미검증 항목

- 전체 게임의 직접 조작 및 실제 플레이어 애니메이션 그래프를 통한 모든 스킬 연계.
- 모든 지형·NavMesh 경계·적 밀집 상황, Root Motion 간섭, 입력 장치별 UI 전환.
- 모든 스킬 VFX·피해량 및 전체 전투 회귀.
- Player 빌드, 성능 프로파일러 수치 비교. FPS 개선율을 주장하지 않는다.

## 최종 결과

이동/스킬 제어의 확인된 문제를 최소 범위에서 수정했다. 컴파일 PASS, EditMode 10개 및 PlayMode 7개 PASS. 실패 원인을 수정한 뒤 최종 코드로 재검증했다. Scene/Prefab/애니메이션 데이터는 변경하지 않았고, 조작감 전체가 해결됐다고 추측하지 않는다.
