# 2026-09-30 개발 내용 통합 보고서

## 작업 목적

프로젝트 구조와 주요 시스템을 파악하고, 플레이어 이동·스킬 연결 문제를 개선한 뒤 D1_FinalBoss의 Special3 중간 보스 패턴을 준비하고 Unity에서 확인한다. 요청에 따라 Special3 테스트용 공간과 Capsule 더미 보스를 추가해 흐름을 점검했다.

## 프로젝트 분석

Unity Editor에서 `Circus-Main-Hall` 씬을 확인했다. 프로젝트는 Character, Enemy, Boss, Dungeon, Skill, UI, System 영역으로 나뉘며, 플레이어는 `CharacterModel`, `PlayerInputs`, `C_Controller`, 스킬 시스템으로 구성된다. 일반 적은 Enemy FSM을 사용하고 보스는 `BossModel`의 패턴 처리와 보스별 스크립트로 동작한다. 씬 전환과 던전 진행은 `SceneChanger`, `DungeonManager`, `SectorController` 등이 담당한다.

초기 검토에서 우선 관찰 대상으로 삼은 영역은 플레이어 이동과 스킬 상태 동기화, 인벤토리·장비 처리, 보스 패턴과 씬 진행 연결이었다. 실제 수정은 사용자가 지적한 이동의 끊김 및 스킬과의 연계 문제, D1_FinalBoss Special3 흐름에 한정했다.

## 플레이어 이동 및 스킬 연계

### 문제 및 변경 요약

이동 명령 정지 후 NavMeshAgent 상태가 남거나, 클릭 이동 목표와 회전 방향이 불일치하는 문제가 있었다. 스킬 입력이 UI 포인터 판정·행동 잠금 상태와 일관되게 처리되지 않았고, 회피 애니메이션 종료 시 이동 복구가 애니메이션 이벤트에 과도하게 의존했다.

이동 중단과 재개 시 Agent 경로 상태를 정리하고, 목표 변경을 안정적으로 반영하도록 조정했다. 플레이어 회전과 Dash 이동 방향을 보정했으며, 스킬 시작·취소 입력을 모델 업데이트와 분리해 잠금 상태를 확인하고 유효 입력을 처리하도록 정리했다. 회피 종료 시 이동·Agent 상태 복구도 보완했다.

### 관련 변경 파일

- `Assets/02. Scripts/Character/C_Controller.cs`
- `Assets/02. Scripts/Character/C_Input.cs`
- `Assets/02. Scripts/Character/C_SkillSystem.cs`
- `Assets/02. Scripts/Character/PlayerInputs.cs`
- `Assets/02. Scripts/Character/Skill/SkillBase.cs`
- `Assets/02. Scripts/Character/Skill/Skills/SkillBase/Skill_1.cs`
- `Assets/02. Scripts/Character/Skill/Skills/SkillBase/Skill_2.cs`
- `Assets/02. Scripts/Character/Skill/Skills/SkillBase/Skill_Space.cs`
- `Assets/Tests/`의 플레이어 이동 테스트

프로젝트 설정·패키지 변경은 의도된 기능 변경 사항이 아니며, 기존 작업 트리에는 별도 수정 파일들이 포함되어 있다. 작업 전부터 있던 변경 사항은 이 보고서 작성 과정에서 되돌리지 않았다.

## D1_FinalBoss Special3 중간 보스 패턴

### 목표 동작

Jester의 체력이 60%에 도달하면 Special3가 시작된다. 보스가 중앙으로 이동해 영상을 재생하고, 영상 종료 뒤 플레이어를 중간 보스 대기 지점으로 이동시킨다. 플레이어가 중간 보스의 공격 범위에 들어오면 전투를 시작하고, 중간 보스 처치 후 플레이어를 보스전 복귀 지점으로 이동시켜 기존 전투를 재개하는 구조다. 중간 보스 전투 중 플레이어 사망은 기존 BossModel 및 게임 사망 이벤트 흐름에 맡긴다.

### 구현 및 에디터 준비

- 기존 `Special_Chip()` 흐름을 Special3 중간 보스 패턴 `Special_MiddleBoss()`로 교체했다.
- 특수 패턴 데이터에 컷씬, 대기 지점, 생성 지점, 복귀 지점을 연결하는 필드를 추가하고 Jester 프리팹에 영상 및 중간 보스 프리팹을 지정했다. HP 60% 설정은 기존 프리팹 설정을 유지했다.
- 중간 보스가 런타임에 생성되어도 미리 설정한 생성 지점을 보존할 수 있도록 BossModel 초기화 흐름을 조정했다.
- NavMeshAgent가 유효하지 않거나 NavMesh 위에 있지 않은 경우 보스 이동 위치를 직접 보정하는 fallback을 마련했다.
- `Circus-Main-Hall` 씬에 `Special3_TestArena`를 만들고 Plane과 대기·생성·복귀 마커를 배치했다.
- Capsule 기반 `D1_MiddleBoss` 테스트 프리팹을 만들었다. 실제 보스 애니메이션·공격·피격 기능을 갖춘 적이 아닌 패턴 연결을 위한 더미다.

### 관련 변경 파일

- `Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss.cs`
- `Assets/02. Scripts/Enemy/BossModel.cs`
- `Assets/03. Prefab/Enemy/Boss/D1/Final/Jester.prefab`
- `Assets/03. Prefab/Enemy/Boss/D1/Middle/D1_MiddleBoss_Test_Runtime.prefab`
- `Assets/01. Scene/Circus-Main-Hall.unity`

### Public API 변경

Special3 설정 데이터에 다음 public 필드를 추가했다.

- `VideoClip cutsceneClip`
- `Transform waitingArea`
- `Transform middleBossSpawnPoint`
- `Transform returnPoint`

기존 prefab 필드는 유지했다. 이 데이터 타입을 사용하는 다른 보스 프리팹이나 코드가 있다면 새 참조 필드를 설정하지 않을 경우 지정된 fallback 동작을 확인해야 한다.

## Unity 검증 결과

### Compile

Result: PASS — AssetDatabase 강제 동기화 후 컴파일을 확인했다. 최근 확인에서 컴파일 Error는 없었다.

### Console

- 마지막 테스트 실행에서 Error: 0건.
- 기존 코드의 사용되지 않는 이벤트·필드 관련 C# 경고가 확인됐다.
- Animator 전이 설정 및 NavMesh 미설정 경고가 추가로 확인됐다.
- 통합 프로브가 컷씬 종료 대기 중 제한 시간에 도달하면서 timeout Error 로그를 한 건 남겼다. Console Clear 도구는 MCP 로그 파일 잠금으로 실패했다. timeout 기록은 검증 한계로 남겼다.

### EditMode Tests

Result: PASS  
Passed: 20 / Failed: 0 / Skipped: 0

### PlayMode Tests

Result: PASS  
Passed: 7 / Failed: 0 / Skipped: 0

이 테스트들은 현재 등록된 테스트 모음을 검증한다. Special3 전체 흐름만을 대상으로 하는 전용 자동 테스트는 아니다.

## 통합 플레이 점검과 미검증 범위

Unity Editor 통합 프로브에서 Special3 트리거, 보스 중앙 위치 이동, 컷씬 시작까지 진행한 것을 확인했다. 프로브 당시 Unity 프레임은 진행했으나 영상 종료 상태가 제한 시간 내에 바뀌지 않아 기록은 `video=True`, `dummy=False`에서 끝났다.

따라서 다음 단계는 통합 검증을 완료하지 못했다.

- 영상 종료 이벤트 후 더미 보스 생성
- 플레이어가 대기 지점에서 공격 범위에 들어갔을 때 전투 시작
- 중간 보스 처치 후 복귀 지점 이동과 기존 보스 전투 재개
- 중간 보스 전투 중 플레이어 사망 처리

또한 테스트 씬에는 유효 NavMesh가 없어 Agent 경로 기반의 실제 보스 이동은 확인하지 못했다. Capsule은 더미여서 실제 전투와 사망 판정도 검증 대상에서 제외된다.

## 남은 작업

- VideoPlayer 종료 이벤트가 테스트 중 완료되지 않은 원인을 확인하고, 영상 종료 후 스폰·전투 시작·처치·복귀를 다시 통합 검증한다.
- 플레이어 사망 분기와 기존 BossModel 사망 처리의 연결을 확인한다.
- 실제 아레나와 중간 보스가 준비되면 임시 Plane·마커·더미를 실제 리소스로 교체하고 NavMesh 이동을 검증한다.
- 개발 중 Console Clear가 로그 파일 잠금으로 실패한 원인은 별도 환경 이슈로 남아 있다.

## 최종 결과

플레이어 이동 및 스킬 입력 처리의 끊김을 줄이는 변경, Special3의 중간 보스 왕복 로직, 씬 테스트 아레나와 더미 보스를 준비했다. Unity 컴파일과 등록된 EditMode/PlayMode 테스트는 통과했다. Special3의 영상 종료 이후 구간은 아직 통합 검증되지 않아 해당 기능 전체를 완료 상태로 판정하지 않는다.



## 공격 직후 우클릭 이동 입력 유실 수정 (2026-09-30)

### 원인

`PlayerInputs.Update()`가 `model.canMove`가 true일 때만 이동 입력을 컨트롤러에 전달했다. 공격 애니메이션 중 이동 잠금 프레임에 들어온 클릭은 전달되지 않고 사라졌다. 또한 이동 명령 직후 NavMeshAgent가 경로를 아직 계산하지 않은 한 프레임을 경로 실패로 판단해 `StopMove()`가 목적지를 지우는 경우가 재현됐다.

### 변경

- `PlayerInputs`가 UI 포인터 위가 아닌 동안 이동 입력을 전달하고, 공격/사망/기절 상태와 실제 이동 가능 여부는 `C_Controller`에서 판정하도록 했다.
- 공격 중 `canMove=false`인 시점에 받은 목적지를 임시 저장한다. 공격이 끝나고 이동이 허용되면 다음 Controller Tick에서 목적지로 이동을 시작한다.
- 사망 또는 기절 중에는 대기 목적지를 버리고, `StopMove()` 호출 시 남아 있는 목적지도 정리한다.
- 목적지 설정 직후 NavMesh 경로 계산이 끝나기 전에 이동을 중단하던 조건을 바꿨다. 경로 계산 중에는 기다리고, 짧은 계산 유예 뒤에도 경로가 없거나 도착했을 때만 정지한다.
- 공격 잠금 중 이동 클릭을 저장하고 잠금 해제 후 실제 NavMesh 경로를 생성하는 PlayMode 회귀 테스트를 추가했다.

### 수정 파일

- `Assets/02. Scripts/Character/C_Controller.cs`
- `Assets/02. Scripts/Character/PlayerInputs.cs`
- `Assets/Tests/Editor/PlayerMovementTests.cs`
- `Assets/Tests/PlayMode/PlayerMovementPlayModeTests.cs`

### 검증

- AssetDatabase ForceSynchronousImport 및 컴파일 상태 확인: PASS, `isCompiling=False`, `isUpdating=False`, `scriptCompilationFailed=False`.
- EditMode: PASS, 20/20.
- PlayMode: PASS, 8/8. 새 공격 잠금 중 이동 클릭 테스트 포함.
- Console Error: 프로젝트 코드 컴파일 Error는 없었다. Unity MCP 시작 중 `.agents/skills/assets-copy/SKILL.md` 기록 실패 로그 1건은 프로젝트 쓰기 권한/플러그인 스킬 생성 관련이며 이번 변경 코드에서 발생한 Error는 아니다.
- Console Warning: 기존 Iden 애니메이터 전이 경고와 테스트 씬의 NavMesh 없는 Agent 경고가 확인됐다.


## 아덴 마지막 타격 후 일반 공격 복구 수정

### 작업 목적

Z로 아덴을 발동하고 변화된 기본 공격의 마지막 타격까지 수행한 뒤 일반 공격이 막히는 오류를 해결한다.

### 분석

- 아덴 종료 코드가 PlayerAnimController에 없는 IdenFinish 트리거를 호출했다. 실제 Controller는 IsIden=false에 의해 IdenEndCharge와 IdenEnd로 전이하는 구조다.
- IdenEnd에서 Idle로 나가는 전이는 조건도 Exit Time도 없어 Unity에서 무시됐다.
- 기존 마지막 타격 이벤트는 canAttack/canMove만 복구하고 isWaitingForRelease와 isAttacking를 정리하지 않았다. 마우스를 계속 누르면 컨트롤러의 공격 입력이 대기 분기에 남았다.

### 수정 파일 / 함수

- Assets/02. Scripts/Character/CharacterModel.cs: OnAttackEnd(), IdenDisable(), Die(), StunEnable(), StunDisable(), 신규 private ClearIdenAttackState(), OnIdenAttackFinished().
- Assets/06. Animations/Character/IdenEnd.anim: 조작 복구 이벤트를 OnIdenAttackFinished 하나로 교체.
- Assets/06. Animations/Character/PlayerAnimController.controller: IdenEnd→Idle/Run의 hasExitTime=true, exitTime=1 설정.
- Assets/Tests/Editor/PlayerMovementTests.cs, Assets/Tests/EditMode/PlayerMovementEditModeTests.cs, Assets/Tests/PlayMode/PlayerMovementPlayModeTests.cs: 회복 상태 및 실제 애니메이터 전환 회귀 테스트.
- Packages/manifest.json, Packages/packages-lock.json: 사용자 확인에 따라 Cinemachine 3 전용 MCP 확장을 제거하고 기존 Cinemachine 2.10.7로 복구. 추가된 Animation/InputSystem 확장은 유지.

### 주요 변경 내용

마지막 타격 진행 중에는 마우스를 놓아도 종료 대기 상태를 임의 해제하지 않는다. 마지막 타격의 기존 조작 허용 시점(1.333초)에 완료 이벤트가 공격·콤보·대기 상태를 함께 정리한다. 사망·기절·NavMeshAgent가 비활성인 상태에서는 제어를 다시 허용하지 않는다. 사망 처리에서도 대기·공격 상태를 정리해 타격 애니메이션이 중단돼도 잔여 잠금이 남지 않도록 했다.

조작 허용 이벤트 시점은 기존 게임 반응 시간을 보존했다. 실제 타격 이벤트는 0.266초이고, 마지막 타격 뒤의 회복 구간에서 일반 공격 입력을 다시 받을 수 있다. 자동 Idle/Run 복귀는 클립 종료에 맞춘다.

CharacterModel.cs의 기존 인코딩을 보존했고 public/Serialize 필드 이름과 참조는 변경하지 않았다. 애니메이션의 VFX 타격 이벤트와 곡선 데이터도 보존했다.

### Public API 변경

없음. 새 완료 이벤트와 상태 정리 메서드는 private이며 기존 public 메서드 시그니처와 Serialize 필드 이름은 유지했다.

### Multi-Agent 작업

- 사용 여부: 사용.
- Main Agent: 코드/자산 수정, Unity MCP 상태 조회, 실제 테스트 실행, 결과 통합 및 보고서 작성.
- Review Agent(iden_review): 상태 전환, API/Serialize/인코딩 위험, 사망 중단 경계 조건 검토.
- Test/QA Agent(iden_qa): 실패 로그, AnimatorOverrideController fixture, 이벤트와 저장 자산 불일치 분석.
- 리뷰 반영: 저장 자산을 재조회하라는 지적을 반영하고 실제 이벤트/전이 값까지 확인했다. 사망으로 완료 이벤트가 취소될 경우의 잔여 대기 상태도 정리했다. 기절 중 타격 중단 시 StunEnable에서 대기 상태와 아덴 이펙트를 정리하고, StunDisable에서 살아 있고 Agent가 활성일 때만 조작을 복구하도록 했다. 추가 리뷰에서 지적된 블렌딩 중 늦은 종료 이벤트가 기절 복구 예약을 지우는 경우도 보완했다. 기절 직후 완료 콜백을 호출하고 기절 해제 후 실제 일반 공격 복귀를 확인하는 PlayMode 사례를 실행해 통과했다.
- 회복 이벤트를 클립 끝으로 옮기는 제안은 기존 조작 허용 시점을 보존하기 위해 채택하지 않았다. 타격 이벤트 이후 회복 시점에 상태를 정리하고 클립 종료 전이를 별도로 설정했다.
- 서브에이전트는 분석만 수행했으며 테스트 PASS 결과는 Main Agent가 Unity MCP로 직접 실행해 확인한 결과다.

### Unity 검증

Editor / Scene / Hierarchy: PASS(확인 범위). 열린 씬 Map1-Forest, 이번 아덴 수정으로 씬 변경 없음. Application.dataPath=D:/UnityProjects/SangDol/Assets. 패키지 복구 및 모든 수정 후 최종 MCP 조회는 isPlaying=false, isCompiling=false, isUpdating=false, scriptCompilationFailed=false이다. 실제 등록 Cinemachine 버전은 2.10.7이다. 전체 Hierarchy 전수 검사는 NOT RUN.

Compile: PASS(최종 재검증). 검증 중 발생한 패키지 충돌은 사용자 확인에 따라 복구한 뒤 다시 검증했다.

Console: 패키지 복구 후 확인 구간 및 최종 테스트 실행 Error 0, Exception 0. 최종 조회에는 기존 부트스트랩 Agent의 유효 NavMesh 없음 Warning 1건이 남았다. IdenEnd→Idle 전이 무효 경고 및 IdenFinish 파라미터 부재 경고도 재발하지 않았다. 전체 기록에는 복구 전 CS0246 5개 위치의 반복 로그, 기존 미사용 이벤트/필드 경고, MCP 스킬 파일 생성 권한 오류, 임시 진단 스크립트 오류가 남아 있다. 임시 진단 스크립트는 수정 후 재조회에 성공했다.

EditMode Tests: PASS(최종 수정 이후 재실행) — 도구 Summary 기준 26 passed, 0 failed, 0 skipped. 기존 fixture와 reflection bridge가 같은 사례를 포함하므로 독립 사례 수와 도구 총계는 다를 수 있다.

PlayMode Tests: PASS(최종 수정 이후 재실행) — 11 passed, 0 failed, 0 skipped. 실제 PlayerAnimController와 제어 이벤트를 이용해 아덴 종료→완료 이벤트→Idle→일반 공격 Atk1 진입을 검증했다. 마우스 유지, 마지막 타격 중 해제, 기절 중단 및 늦은 완료 이벤트 후 기절 해제 경우를 모두 확인했다.

### 실패 분석 및 재검증

처음 PlayMode 2개 사례는 실패했다. Unity API 저장 성공 응답과 달리 자산 이벤트 및 전이가 파일에 남지 않은 것을 디스크와 MCP 재조회로 확인했다. 의도하지 않은 곡선 재직렬화 변경을 제거한 뒤 이벤트와 전이 블록만 최소 수정해 재임포트했다. 최종 로드된 이벤트는 IdenFinalAttack/OnIdenAttackFinished이고 Idle/Run 전이는 모두 Exit Time=1인 것을 MCP로 확인했다.

이후 실제 일반 공격 상태 이름이 Atk1임을 확인해 테스트의 대소문자를 수정했고 PlayMode 10/10, EditMode 26/26으로 재검증했다. 이 통과 결과는 추가 기절 처리 및 패키지 충돌 이전의 결과다.

추가 검증 시 Packages/manifest.json와 packages-lock.json에 Main이 수행하지 않은 변경을 확인했다. 새 com.ivanmurzak.unity.mcp.cinemachine@1.0.19는 com.unity.cinemachine@3.1.6에 의존한다. manifest의 직접 참조는 기존 2.10.7이지만 잠금 파일과 실제 Editor 등록 버전은 3.1.6으로 확인됐다. 기존 코드가 사용하는 Cinemachine 2 API가 사라져 컴파일 오류가 발생했다. AGENTS.md 10절의 패키지 변경 제한에 따라 임의 삭제·버전 변경·전체 API 마이그레이션은 수행하지 않았다.

사용자가 "기존 Cinemachine 2 유지가 목적"이라고 확인한 뒤 Unity PackageManager.Client.Remove로 Cinemachine MCP 확장만 제거했다. 실제 Editor 등록 버전이 2.10.7로 돌아온 것을 확인하고 ForceSynchronousImport, 컴파일 상태, Console 및 EditMode 26/26·PlayMode 11/11을 다시 검증했다. 패키지 충돌 이전의 통과 결과와 최종 통과 결과를 구분해 기록했다.

### 남은 문제 / 미검증 항목

실제 키보드 Z 입력, VFX와 마지막 타격 피해·대상 반응의 전체 플레이 흐름은 별도로 검증하지 않았다. 테스트는 UI/오디오/VFX/피해 의존성을 격리하고 실제 애니메이터 제어 이벤트와 정상 공격 복귀를 검증한다. 사망·제어 비활성 시 복구 방지는 EditMode로 확인했으며 사망→전체 씬 부활 과정의 통합 테스트는 NOT RUN. 아덴 이펙트 풀 반환의 시각 결과도 별도 통합 검증하지 않았다.

### 최종 결과

아덴 마지막 타격 이후 공격 해제 대기 상태와 Animator 복귀 전이를 수정했다. 기절 중단 및 늦은 이벤트 순서도 보완했고 사용자 의도에 맞게 Cinemachine 2를 복구했다. 최종 Unity 컴파일, EditMode 26개 및 PlayMode 11개 테스트는 모두 통과했다. 변경된 AGENTS.md의 리뷰·QA·DevLog 규칙을 적용했으며 전체 Z 입력·VFX·피해 흐름의 미검증 범위는 위에 명시했다.

## 프로젝트 전반 점검 및 수정 우선순위

### 작업 목적 / 범위

추가 수정이 필요한 부분을 찾고 우선순위를 정리한다. Assets/02. Scripts의 196개 C# 파일 목록과 위험 패턴을 검색하고, 플레이어 제어·스킬·타격, 인벤토리·장비·퀘스트·버프 UI, 보스·던전·풀링·씬 전환의 주요 실행 경로를 읽었다. 모든 파일의 모든 경로와 전체 게임 플레이를 전수 검증한 것은 아니다.

Production 코드, Scene, Prefab, Package는 이번 점검에서 수정하지 않았다. 이 문서에 점검 결과만 추가했다. Public API 변경 없음.

### Multi-Agent 작업

- 사용 여부: 사용.
- Main Agent: 플레이어/전투 경로 분석, 다른 리뷰 결과의 원본 확인, 실제 Unity 상태 조회, 기존 테스트 실행, 메모리 내 인벤토리 프로브 실행 및 보고서 통합.
- Review Agent(iden_review): 보스·던전·영상·풀링·빌드 의존성 읽기 리뷰.
- Test/QA Agent(iden_qa): 인벤토리·장비·퀘스트·버프 UI 결함 및 테스트 누락 읽기 리뷰.
- 서브에이전트는 파일 수정이나 Unity 테스트를 수행하지 않았다. Main이 근거를 대조했으며 패치 제안은 이번에 적용하지 않았다. 던전 재스폰에서 Agent를 다시 켜는 경로는 확인하여 해당 경로의 Agent 비활성 문제는 우선 결함 목록에서 제외했다.

### 우선 수정 후보

P1은 데이터 소실 또는 진행 중단 위험, P2는 판정·표시 오류를 뜻한다. 아래 정적 결함을 실제 플레이에서 모두 재현한 것은 아니며 실행한 점검은 별도로 기록한다.

#### 1. P1 — 일반 스킬 중단 시 상태 및 조작 복구

- 근거: CharacterModel.cs:156,634,648; Skill_1.cs:25; Skill_3.cs:18; Skill_4.cs:26; SkillBase.cs:238.
- 일반 스킬은 canMove/canAttack/canSkill을 잠그고 애니메이션 이벤트로 복구한다. 현재 StunDisable의 별도 복구는 아덴 마지막 타격용이다. 스킬 애니메이션이 기절로 취소되면 종료 이벤트가 사라져 잠금이 남을 수 있다. 실제 Animator 자산에는 AnyState→Stun 전이가 있으며 Stun 클립에는 제어 복구 이벤트가 없다.
- CharacterModel.Update는 기절 여부를 검사하기 전에 UpdateSkills를 실행하고, 일반 차징/홀딩의 취소 경로도 없다. 기절 중 홀딩 틱이나 자동 발사가 계속될 위험이 있다. 사망의 ResetSkillCool도 차징 상태는 초기화하지 않는다.
- 권장: 스킬 시작·종료·기절·사망·컷씬 중단에 공통 정리 경로를 두고, 해당 행동이 소유한 제어 잠금만 해제한다. Skill1/3/4 각각 중단 전후 회귀 테스트를 추가한다. 아덴 수정은 보존한다.
- Runtime 재현: NOT RUN.

#### 2. P1 — 인벤토리·장비 이동의 아이템 소실

- 근거: C_Equipment.cs:74,79; C_Inventory.cs:87,195; InventorySlot.cs:167; DropItemModel.cs:40,43.
- 인벤이 가득 찼을 때 장비 해제가 AddItem 실패를 확인하지 않고 장비 참조를 비운다. 장비를 점유된 슬롯에 드롭하면 SetItemAt이 원래 아이템을 덮어쓴다.
- 바닥 아이템도 AddItem 후 성공 여부와 관계없이 풀로 반환된다. 수용량이 없거나 일부 스택만 받을 수 있으면 전체 또는 잔여 수량이 월드에서 사라진다. 같은 종류 장비 교체도 기존 장비를 반환하기 전에 새 장비 슬롯을 비우지 않아 가득 찬 인벤에서 기존 장비가 소실될 수 있다.
- 권장: 이동 전 용량/점유 여부를 확인하고, 수용 수량 또는 성공 결과를 반환받아 이동을 확정한다. 장비 교체는 원자적으로 처리하고 드롭 잔여 수량을 보존한다.
- 메모리 내 실제 프로브: 점유된 슬롯에서 SetItemAt이 기존 아이템을 제거하는 결과 확인. 전체 장비/UI/드롭 흐름은 NOT VERIFIED.

#### 3. P1 — 보스 처치 연출 및 재전투 초기화

- 근거: BossModel.cs:176,186; EnemyBase.cs:46; D1_FinalBoss.cs:172.
- 기본 보스 사망은 Die 트리거 직후 객체를 풀로 반환해 비활성화한다. Jester 사망 경로에는 보스 처치 동영상 호출이 연결되어 있지 않다. 현재 확인한 처치 경로에서는 이전에 요청한 처치 컷씬을 재생하지 않는다. Special3 영상은 별도 패턴 영상이다.
- 사망 시 패턴 행동 정리가 없고, 재사용 Reset은 HP/Animator 위주다. 종료되지 않은 currentPattern 및 specialPatterns.hasDone이 남아 재전투 AI가 정지하거나 기믹을 생략할 위험이 있다. ElderGolem의 별도 사망 정리와 구분한다.
- 권장: 처치 확정→공격/생성물 정리→사망 연출/컷씬→풀 반환 순서를 명확히 하고, 재사용 시 보스 패턴 상태를 초기화한다.
- 실제 처치·재전투 재현: NOT RUN.

#### 4. P1 — 풀의 중복 반환과 맵 전환 정리

- 근거: Skill_4.cs:117,130,149,153; Pool.cs:43,49; PoolManager.cs:27,106; FinalBossSector.cs:85,93; BossModel.cs:437.
- Skill4는 해제 시 spinEffect를 반환하지만, 3초 뒤 Effect 코루틴도 같은 객체를 다시 반환한다. 그 사이 재사용됐다면 다른 효과를 중간에 끄거나 동일 객체를 풀 스택에 두 번 넣을 수 있다. Pool.Push에는 중복 방어가 없다. 활성 최종 보스 섹터 Reset에도 보스를 두 번 반환하는 경로가 있다.
- LocalPool은 DontDestroyOnLoad 대상인데 ClearStagePools는 반환된 비활성 스택만 파괴한다. 활성 로컬 몬스터/이펙트가 남은 채 맵을 전환하면 이전 객체가 살아 있고, 등록이 삭제돼 이후 반환도 실패할 수 있다.
- 권장: 반환 책임을 한 곳에 두고 임대 여부/세대 또는 활성 집합을 추적한다. 스테이지 해제 시 활성·비활성 객체를 함께 정리한다.
- 실제 중복 Pop 및 맵 왕복 재현: NOT RUN.

#### 5. P1 — 영상 실패 시 던전 / Special3 진행 중단

- 근거: VideoPlayManager.cs:25,36,47; D1_FinalBoss.cs:547,551; DungeonManager.cs:155,171.
- isPlaying=true 이후 정상 영상 종료 이벤트만 처리한다. 디코딩 오류, 종료 이벤트 미발생에 대한 errorReceived 처리와 시간 제한이 없다. Special3는 플레이어 조작을 잠근 상태에서 대기하므로 영상 실패가 영구 대기로 이어질 수 있다.
- 권장: 성공·실패·취소를 구분하고 시간 제한, 실패 시 제어 복구 및 패턴 종료 정책을 추가한다. 정상 영상 종료 이후 중간 보스 처치·복귀까지 전용 통합 테스트가 필요하다.
- 실제 영상 오류 재현: NOT RUN. 이전 Special3 영상 대기 프로브의 미완료 기록과 연결되는 위험이지만 이번 정적 분석만으로 그 이전 timeout의 원인을 확정하지 않는다.

#### 6. P1/P2 — 퀘스트 보상 및 인벤토리 갱신

- 근거: QuestManager.cs:88,324,340,343; C_Inventory.cs:75,79,100; NormalItemBase.cs:24.
- 퀘스트 완료 시 경험치/골드는 지급하지만 아이템 보상은 로그만 남기며 실제 지급 코드는 주석이다. 실제 CSV의 제철 사과 퀘스트는 HP 포션 20개, 늑대 사냥은 장비 1개 보상이 있다.
- 기존 스택에 아이템을 합치면 OnAddInventory/OnGetItem과 OnInventoryUpdated가 발생하지 않아 수집 진행과 UI가 갱신되지 않는다. 아이템 제거 후에도 수집 상태를 다시 검증하지 않으면 실제 인벤과 완료 가능 상태가 달라질 수 있다.
- 권장: 실제 보상 지급과 공간 부족 정책을 연결하고 보상 처리 후 완료를 확정한다. 수량 증감의 단일 이벤트를 발행하고 퀘스트 완료 시 실제 인벤을 재검증한다.
- 메모리 내 실제 프로브: 동일 아이템 1+1의 수량은 2지만 두 번째 Add 알림 0, Update 알림 0 확인. 실제 퀘스트 완료 UI는 NOT RUN.

#### 7. P2 — 타격 중복 및 스킬 치명타 계산

- 근거: CharacterModel.cs:469,475,486,551,558; PlayerAttackContainer.cs:51,78,83; EnemyStat.cs:55.
- 일반 공격은 HashSet을 생성하지만 피격 적을 Add하지 않아 여러 Collider가 같은 EnemyBase를 가리키면 중복 피해를 줄 수 있다. 4타와 스킬 타격/홀딩 틱도 EnemyBase 단위 중복 제거가 없다.
- 스킬은 isCritical을 설정하지만 criticalDamage 배율을 피해에 적용하지 않는다. EnemyStat도 치명타 배율을 추가하지 않아 일반 공격과 처리 방식이 다르다.
- 권장: 타격 1회 또는 의도된 틱마다 적 단위 중복 제거를 하고, 일반 공격/스킬의 치명타 배율 계산 책임을 일관되게 둔다. OverlapNonAlloc 및 버퍼 재사용도 성능 측정 후 검토할 수 있다.
- 실제 MCP 프리팹 조회: Assets/03. Prefab/Enemy 아래 현재 EnemyBase 하나에 여러 Collider가 연결된 프리팹은 찾지 못했다. 중복 피해는 그런 리소스를 추가할 때 드러나는 조건부 결함이며 현재 플레이 재현으로 단정하지 않는다. 치명타 피해량 비교 Runtime 테스트는 NOT RUN.

#### 8. P2 — 전체 버프 제거 후 UI 잔존

- 근거: C_Buff.cs:111,113; BuffList.cs:42,53; CharacterModel.cs:778.
- RemoveAllBuff가 리스트를 비우기 전에 제거 이벤트를 보내고 Clear 후에는 이벤트가 없다. UI는 제거 이벤트 때 아직 존재하는 전체 버프 목록을 읽어 사망 후 아이콘이 남을 수 있다.
- 권장: 목록 변경 후 알림을 발행하고 전체 제거 후 슬롯 수가 0인지 검증한다.
- 실제 UI 재현: NOT RUN.

#### 9. 배포 전 확인 — Runtime 코드의 UnityEditor 참조

- 근거: CharacterModel.cs:7, BossModel.cs:3, D1_Rook.cs:3.
- Editor 전용 Handles 사용부는 조건부로 감싸져 있지만 using UnityEditor는 조건부 밖이다. Editor 컴파일 성공이 Player 빌드 성공을 보장하지 않는다.
- 권장: Editor 의존성 전체를 UNITY_EDITOR 조건 안으로 제한하고 실제 Player 빌드를 검증한다.
- Player Build: NOT RUN. 이번 점검에서는 빌드 실패를 실제 실행 결과로 보고하지 않는다.

### Unity 검증

- Compile: PASS. 실제 MCP 조회에서 isPlaying=false, isCompiling=false, isUpdating=false, scriptCompilationFailed=false.
- Scene/Hierarchy: Map1-Forest, 저장 상태, GameObject 2,086개, Missing Script 0. 현재 맵의 NavMesh 삼각형 9,178개. 이 씬 단독 Editor 상태에는 NavMeshAgent/CharacterModel이 없으며 Main 부트스트랩과 별개다. 이 씬 조회 결과를 프로젝트 전체의 참조 정상 판정으로 확대하지 않는다.
- Console: 최종 점검 구간 Error 0, Exception 0. 테스트 중 유효 NavMesh 없는 Agent Warning 1건. 더 이전 조회에는 Unity MCP InputSystem 스킬 생성 파일 쓰기 오류가 남아 있었으며 게임 로직 오류와 구분했다.
- EditMode: PASS — 26 passed, 0 failed, 0 skipped.
- PlayMode: PASS — 11 passed, 0 failed, 0 skipped.
- 기존 테스트는 이동·회피·점프·스킬 입력 버퍼·아덴 종료를 검증한다. 위 인벤토리·장비·퀘스트·일반 스킬 중단·보스·풀링·영상 실패·BuffUI 문제의 전용 회귀 테스트는 NOT AVAILABLE.
- 메모리 내 InventoryAuditProbe: PASS(현재 동작 확인). 임시 데이터와 임시 ItemBase 구현만 사용하고 종료 시 ScriptableObject를 삭제했다. 확인 결과는 정상성 PASS가 아니라 갱신 누락/덮어쓰기 재현 근거다. 저장 자산이나 실제 플레이어 인벤토리는 건드리지 않았다.
- Profiler를 통한 프레임/GC 원인 측정 및 Player Build: NOT RUN.

### 최종 결과

점검과 우선순위 보고를 완료했다. 수정 추천의 출발점은 일반 스킬 중단 복구, 아이템 이동의 데이터 보존, 보스 종료·풀 수명·영상 실패 복구다. 확인된 정적 결함 및 조건부 위험을 기록했으며 이번에는 코드를 수정하지 않았다. 기존 테스트가 모두 통과한 사실과 프로젝트 전체 기능이 검증됐다는 주장은 구분한다.

---

## 전체 점검 후 1번 개선 — 일반 스킬 중단과 조작 복구

### 작업 목적

전체 점검의 첫 번째 항목인 Skill1/3/4의 기절·사망·외부 조작 잠금 시 중단 처리를 수정한다. 차징/홀딩, 돌진, 지연 효과가 남거나 이전 애니메이션 이벤트가 새 스킬 또는 컷씬의 조작 잠금을 해제하지 않도록 한다.

### 분석

- 일반 스킬의 canMove/canAttack/canSkill 복구는 주로 애니메이션 이벤트에 의존했다. 기절 전이로 해당 이벤트가 실행되지 않으면 차징 및 조작 잠금이 남을 수 있었다.
- 스킬 효과와 이동 코루틴이 CharacterModel에서 시작되지만 취소 시 일괄 정리할 소유 관계가 없었다.
- Skill4는 키 해제 시 spinEffect를 반환한 뒤 3초 코루틴에서도 같은 객체에 접근·반환했다. 그 사이 다른 사용자가 재임대하면 새 사용자의 효과를 변경할 수 있었다.
- 정상 조작 복구 이후에도 남는 지연 효과는 사망 시 별도 취소가 필요했다. CharacterModel의 사망 후 Update 조기 종료 때문에 매 프레임 스킬 검사만으로는 충분하지 않았다.

### 수정 파일

- Assets/02. Scripts/Character/Skill/SkillBase.cs
- Assets/02. Scripts/Character/C_SkillSystem.cs
- Assets/02. Scripts/Character/CharacterModel.cs
- Assets/02. Scripts/Character/PlayerAttackContainer.cs
- Assets/02. Scripts/Character/Skill/Skills/SkillBase/Skill_1.cs
- Assets/02. Scripts/Character/Skill/Skills/SkillBase/Skill_3.cs
- Assets/02. Scripts/Character/Skill/Skills/SkillBase/Skill_4.cs
- Assets/06. Animations/Character/Skill_1.anim
- Assets/06. Animations/Character/Skill_3_ChargeEnd.anim
- Assets/06. Animations/Character/Skill_3_Shot.anim
- Assets/06. Animations/Character/Skill_4_End.anim
- Assets/06. Animations/Character/Skill_4_End2.anim
- Assets/06. Animations/Character/Skill_4_Perfect.anim
- Assets/Tests/Editor/PlayerMovementTests.cs
- Assets/Tests/EditMode/PlayerMovementEditModeTests.cs
- Assets/Tests/PlayMode/PlayerMovementPlayModeTests.cs
- Docs/DevLogs/2026-09-30_DailyDevelopment.md — 이 단일 일일 보고서에 추가.

### 수정 클래스 / 함수

- SkillBase: BeginExecution(), StartExecutionRoutine(), TakeExecutionEffect(), ReturnExecutionEffect(), InterruptExecution(), OwnsAnimationEvent(), UpdateSkill().
- C_SkillSystem: BeginSkillAction(), CompleteSkillAction(), InterruptActiveSkill(), TryRecoverInterruptedControls(), UpdateSkills(), UseSkill(), ReleaseSkill().
- CharacterModel: SetControlable(), ControlEnable(), ControlDisable(), SetCanMove/Attack/Skill(), OnSkillCanMove/Attack/Skill(), OnSkillCantMove(), StunEnable/Disable(), Die(), OnDestroy().
- Skill_1/3/4: UseSkill(), ReleaseSkill(), 이동 및 효과 코루틴의 실행·정리 경로.
- PlayerAttackContainer: AnimEvent_ExevuteSkillAttack(), AnimEvent_ExecuteManagedSkillAttack().
- 테스트: 이동 fixture의 실제 Stat/공격 컴포넌트 준비, 중단용 상태 probe, 실제 Animator/NavMesh/Physics/Pool을 사용하는 일반 스킬 fixture와 회귀 케이스.

### 주요 변경 내용

1. Skill1/3/4만 실행 수명 관리를 적용한다. 시작 시 잠금 전 권한을 저장하고 코루틴·효과를 추적한다. 기절·사망·외부 잠금 시 차징/홀딩 시간과 게이지, 코루틴, 현재 스킬 타격 참조, 해당 스킬의 트리거를 정리한다.
2. 기절 종료 시 시작 전 이동·공격·스킬 권한을 복구한다. 기존에 잠겨 있던 권한을 무조건 열지 않는다. 사망·외부 잠금은 이 자동 복구를 허용하지 않는다. 외부 제어 활성화가 기절 중 요청되면 기절 종료까지 실제 입력 허용을 지연한다.
3. 외부 중단 시 실행 중인 일반 스킬 애니메이션을 Idle로 전환해 차징 상태가 계속 남지 않도록 한다. 기절 및 사망 애니메이션에는 이 강제 전이를 적용하지 않는다.
4. 중단 후 키 해제와 반복 키 해제를 무시한다. 기절·외부 중단은 이미 소비한 쿨타임을 유지하며, 실제 Die()는 기존 ResetSkillCooldown 정책을 유지한다.
5. 6개 애니메이션 클립의 해당 이벤트 이름만 변경했다. 이벤트의 원본 클립이 실행 스킬에 속하는지 확인하고, 취소된 스킬의 타격/조작 복구 이벤트를 무시한다. 정상 Skill3는 ChargeEnd 회복과 Shot 타격이 겹칠 수 있으므로 정상 종료 시 타격 참조는 즉시 제거하지 않는다.
6. 효과 반환은 소유한 객체에 대해 한 번만 수행한다. parent와 원래 scale의 정리는 이 반환 함수에서 수행한다. 반환된 Skill4 효과의 늦은 타이머는 재임대한 객체를 조작하지 않는다.
7. 조작 복구가 끝난 일반 스킬도 실행 목록에 남겨 사망·외부 잠금 시 잔여 효과를 취소한다. 지연 생성 직전에도 실행 가능 상태를 확인한다. 효과 미할당 및 AudioManager 부재도 허용한다.

### Public API 및 Serialize 변경

- 기존 Production public 메서드·필드·클래스 이름과 서명 변경 없음. 실행 관리는 internal/protected/private 보조 경로로 추가했다.
- 기존 SerializeField/public 직렬화 필드 이름, Scene/Prefab 참조 변경 없음. CharacterModel의 기존 CP949 인코딩을 유지했다.
- 클립 이벤트 콜백: SetCanMove/Attack/Skill → OnSkillCanMove/Attack/Skill, SetCantMove → OnSkillCantMove, AnimEvent_ExevuteSkillAttack → AnimEvent_ExecuteManagedSkillAttack. 해당 6개 클립의 콜백만 바꿨으며 기존 public 콜백은 다른 클립과의 호환성을 위해 유지한다.
- 이 작업에서 Scene/Prefab/AnimatorController/Package/ProjectSettings 수정 없음. 이전 작업 및 사용자 변경을 되돌리지 않았다.

### Multi-Agent 작업

- 사용 여부: 사용. AGENTS.md의 Gameplay 리뷰 규칙에 따라 Main이 수정과 실제 Unity 검증을 담당했다.
- Review Agent(iden_review): API·Serialize·중단 경계의 읽기 전용 리뷰. 반환된 Skill4 객체의 늦은 parent 변경, 정상 회복 후 사망 시 남은 지연 효과를 지적했다. 두 항목 모두 수정하고 전용 PlayMode 회귀 테스트로 확인했다. 최종 리뷰에서 추가 중대한 결함은 발견하지 못했다.
- Test/QA Agent(iden_qa): 읽기 전용 테스트 전략 제안. 실제 Die 쿨타임 정책, 반복 키 해제, 외부 잠금과 기절 중첩, 실제 애니메이션 이벤트 및 재임대한 효과를 확인하도록 제안했고 관련 케이스를 반영했다.
- 서브에이전트는 파일 수정 및 Unity 테스트 실행을 하지 않았다. 아래 결과는 Main의 Unity MCP 실행 결과다.

### Unity 검증

#### Editor / Scene / Hierarchy

Result: PASS. 최종 MCP 조회에서 isPlaying=false, isCompiling=false, isUpdating=false. Map1-Forest 씬은 저장 상태이며 GameObject 2,086개, Missing Script 0을 확인했다. 테스트 종료 후 원래 씬으로 돌아왔다.

#### Compile

Result: PASS. ForceSynchronousImport Refresh 후 컴파일 완료 및 EditorUtility.scriptCompilationFailed=false를 확인하고 테스트를 실행했다. 수정 도중 파일 간 반영 시점에 발생했던 컴파일 오류를 최종 성공 판정에 포함하지 않았다.

#### Console

- Errors: 0 — 최종 컴파일/재검증 구간의 최근 5분 MCP 조회.
- Exceptions: 0 — 동일 구간.
- Warnings: 10개 로그. 기존 미사용 이벤트/필드 컴파일 경고 9개(C_Equipment, C_SpecialStat, CharacterModel.OnTakeDamage, D1_Chess, BuffSlot, SkillTreeSlot, D1_Yabawe)와 테스트 시작 시 유효 NavMesh 없는 Agent 경고 1개. Agent 경고는 이전 테스트에서도 관찰된 항목이며 실제 테스트 fixture는 별도 NavMesh를 만들어 실행한다. 경고를 모두 0으로 보고하지 않는다.
- 최종 EditMode/PlayMode 결과에 수집된 Error 로그는 모두 빈 목록이다. 과거 Console 캐시 전체를 삭제하거나 과거 오류가 없었다고 주장하지 않았다.

### EditMode Tests

Result: PASS. 최종 Unity MCP 전체 실행: Passed 34, Failed 0, Skipped 0, Duration 1.003873초.

새 상태 전환 케이스는 중단 시 쿨타임 및 기존 권한 보존, 사망 중 제어 복구 방지, 외부 잠금 중 늦은 복구 방지, 과거 취소된 슬롯이 새 시전을 취소하지 않는 상황을 검증한다. 34개는 기존 Editor fixture 및 EditMode bridge에 중복 등록된 논리 케이스를 포함한 실제 Runner 집계다.

### PlayMode Tests

Result: PASS. 최종 Unity MCP 전체 실행: Passed 24, Failed 0, Skipped 0, Duration 18.7810353초.

- 새 일반 스킬 검증 13개: Skill1 돌진 기절, Skill3 차징 및 해제 후 기절, Skill4 홀딩 기절, 외부 잠금/기절 중첩과 늦은 이벤트, 실제 사망 쿨타임 초기화, 정상 Skill1/3/4 복구 및 Skill1/3 타격, 외부 중단 후 Idle와 기본 공격 복귀, Skill4 효과 재임대 보존, 정상 회복 후 사망의 지연 효과 취소, 과거 Skill3 클립 이벤트가 새 Skill4의 잠금/타격을 변경하지 않는 상황.
- 기존 11개 이동·입력 버퍼·회피·점프·아덴 종료 회귀 테스트도 통과했다.
- 실제 프로젝트 Animator/클립을 메모리 내 override로 실행했다. 조작/타격 이벤트는 유지하고 테스트와 관계없는 오디오·VFX 애니메이션 이벤트는 제외했다. 실제 일반 스킬 코드의 효과는 테스트용 PoolManager와 임시 PoolableMono로 확인했다. 이 테스트는 실제 맵의 모든 리소스를 사용하는 수동 전투와 구분한다.

### 실패와 재검증

- 첫 PlayMode 확장 실행은 20개 중 19 PASS, Skill3 정상 타격 케이스 1 FAIL이었다. 조작 회복 직후 즉시 피해를 판정한 테스트가 ChargeEnd→Shot 블렌드의 뒤따르는 타격 이벤트를 기다리지 않았기 때문이다. 조작 회복 뒤 실제 타격 이벤트까지 제한 시간 내 대기하도록 테스트를 수정했다. 정상 시전에서 타격 참조를 유지하는 설계도 확인했다.
- 단일 메서드 재실행 요청 한 번은 MCP tools/call이 300초 timeout으로 종료했다. 해당 호출은 NOT VERIFIED로 취급했다. Editor가 EditMode·컴파일 완료 상태임을 다시 조회하고, 마지막 수정 후 전체 EditMode 및 PlayMode를 다시 실행해 위 PASS 결과를 확보했다.
- Review의 두 효과 소유/취소 경계 지적을 수정한 뒤 추가 4개 PlayMode 경계 케이스를 포함해 최종 24개를 재검증했다.

### 발견된 문제 / 남은 문제

- 이 작업의 일반 스킬 중단·제어 복구 회귀 테스트는 모두 통과했다.
- 전역 Pool.Push 중복 방어, 보스 종료/영상 실패 복구, 인벤토리·장비 데이터 보존 등 앞선 전체 점검의 다른 항목은 별도 작업으로 남는다. Skill4의 스킬 내부 중복 반환 경로만 이번에 수정했다.
- 실제 Skill4 피니시 피해 이벤트 구성, 치명타 배율, 여러 Collider의 중복 타격 등 피해 설계 변경은 이번 수정에 포함하지 않았다.

### 미검증 항목

- 실제 보스 컷씬과 일반 스킬 시전이 겹치는 맵 전체 수동 플레이: NOT RUN. 공통 ControlDisable/SetControlable 경로를 실제 Animator fixture에서 검증했다.
- 실제 스킬 VFX/오디오 전체 리소스의 시각·음향 품질과 프레임/GC 측정: NOT RUN.
- Player Build: NOT RUN. Unity Editor 컴파일 성공을 배포 빌드 성공으로 확대하지 않는다.

### 변경 범위 확인 / 최종 결과

Git status 및 diff를 확인했다. diff --check 통과, 6개 클립의 diff는 콜백 이름 교체에 해당하는 17줄 추가/17줄 삭제다. 기존 이동/보스/패키지/씬 등 작업 시작 전 변경을 유지했다.

1번 일반 스킬 중단 복구를 완료했다. Unity Compile PASS, 최종 Console Error/Exception 0, EditMode 34/34 및 PlayMode 24/24 PASS를 실제로 확인했으며 결과를 이 일일 문서에 통합했다.

