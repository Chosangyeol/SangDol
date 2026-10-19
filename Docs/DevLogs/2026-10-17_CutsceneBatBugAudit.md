# 컷씬·박쥐 추가 버그 점검

후속 작업: [세 결함 코드 수정 및 검증 결과](2026-10-17_ChessCutsceneAndFacingFix.md). 아래의 '수정 전' 설명은 점검 당시 기록이며, 후속 작업에서 코드를 수정했다. 최초 MCP 타임아웃 이후 연결이 복구되어 Unity 컴파일, 전체 EditMode 116개와 관련 PlayMode 11개의 통과를 확인했다.

## 작업 목적 및 범위

사용자의 추가 버그 점검 요청에 따라 최근 변경한 카메라·박쥐 전투와 연결된 체스 컷씬을 검토했다. 이번 점검에서는 Production 코드와 에셋을 추가 수정하지 않았다. 이전 작업의 미커밋 변경은 유지했다.

## 확인한 버그

### 1. 체스 컷씬의 이동 잠금이 애니메이션 이벤트로 해제됨 — P2

- 위치: `Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss.cs:973`
- `Special_Chess()`는 `Target.canMove=false`만 설정한다. 공격·스킬 권한과 외부 조작 잠금은 그대로다.
- 공격 애니메이션 등에서 `CharacterModel.SetCanMove()`를 호출하면 이동을 다시 허용한다.
- Main Agent가 실제 `MovementRig(false)`와 CharacterModel 메서드로 확인한 결과: `canAttack=True`, `IsExternalControlLocked=False`, `SetCanMove()` 호출 뒤 `canMove=True`.
- 수정 방향: 외부 조작 잠금을 사용하고 정상 종료 및 컷씬 활성화 전 취소 구간에서도 잠금을 해제한다. 다른 동작이 소유한 잠금을 잘못 해제하지 않도록 소유 상태를 기록해야 한다.
- 실제 체스 컷씬에서 마우스로 평타를 입력하는 전체 수동 재현은 수행하지 않았다. 권한 값과 복구 이벤트 경로는 직접 검증했다.

### 2. 체스 전환에서 카메라 갱신 뒤 플레이어를 순간이동함 — P2

- 위치: `D1_FinalBoss.cs:993`의 `ChangeCam(0,true)` 이후 `:997`에서 플레이어를 체스 시작 위치로 이동한다.
- 현재 ChangeCam은 호출 즉시 댐핑 상태와 카메라 출력을 현재 플레이어 위치에 맞춘다. 그 뒤 이동하면 가상 카메라는 새 위치를 연속 이동으로 처리한다.
- Main Agent가 실제 CinemachineTransposer에 XYZ 댐핑 1을 설정하고 같은 순서로 10m 이동시킨 결과, 다음 프레임의 목표 대비 오차가 `9.261187m`였다.
- 최근 ChangeCam의 즉시 출력 갱신과 체스 경로의 기존 호출 순서가 충돌하는 회귀 위험이다. 60% 박쥐 전환의 WarpPlayerTo는 이동 뒤 갱신하지만 체스 경로는 이 처리가 빠져 있다.
- 수정 방향: NavMesh 이동 완료 뒤 카메라 컷 갱신을 수행한다.
- 실제 체스 화면 전체 수동 재현은 수행하지 않았다. 해당 순서의 카메라 댐핑 현상은 Unity에서 직접 재현했다.

### 3. 플레이어와 보스가 같은 수평 위치에 있으면 임의 방향으로 회전함 — P2

- 위치: `Assets/02. Scripts/Enemy/Boss/BossPatternBase.cs:41`
- `SmoothLookAtRoutine()`이 수평 방향 벡터가 0인 상태에서도 `Quaternion.LookRotation()`을 호출한다.
- 박쥐 급강하가 플레이어 위치에 도착한 뒤 다음 패턴을 시작할 때 발생할 수 있다. 위아래로만 떨어진 타겟도 동일 조건이다.
- Main Agent가 실제 메서드를 reflection으로 실행하고 보간 시간 0으로 종료 분기를 확인해, 같은 위치에서 기존 Y=90도 방향이 90도 변하는 것을 확인했다. Unity 로그는 `Log: Look rotation viewing vector is zero`였으며 Error나 Exception으로 잘못 분류하지 않았다. 기본 보간 시간 0.2의 같은 영벡터 계산도 확인했으나 EditMode의 Time.deltaTime=0이어서 해당 프로브로 실시간 보간 완료까지 검증했다고 간주하지 않는다.
- 수정 방향: 수평 벡터 길이가 매우 작으면 현재 회전을 유지한다. 동일 XZ 좌표의 회귀 테스트를 추가한다.

## Multi-Agent 작업

- Main Agent: 현행 코드 확인, Unity 상태 확인, 전체 PlayMode 실행, 실패 분석 및 단독 재실행, 세 문제의 최소 재현, 최종 결과 통합.
- Camera Review Agent: 체스 전환의 카메라 호출 순서와 공격·이동 권한 복구 경로 분석. 읽기 전용.
- Bat Review Agent: 영벡터 회전 결함과 기존 테스트 누락 분석. 읽기 전용.
- 서브에이전트는 파일 수정이나 Unity 테스트 실행을 하지 않았다. 위 실행 증거는 Main Agent가 직접 확인했다.

## Unity 및 테스트 검증

- 점검 시작: Title 씬, dirty=False, playing=False, compiling=False, compileFailed=False.
- 전체 PlayMode 실행: 90개 중 89 PASS, 1 FAIL.
- 실패 항목: `GameFlowPlayModeTests.TitleForestQuestsDungeonBossAndReturn`.
- 실패 stack trace는 게임 로직이 아닌 `UnityEngine.TestTools.Logging.LogScope.EvaluateLogScope()`에서의 `Collection was modified; enumeration operation may not execute`였다. 이 결과만으로 게임 버그라고 판정하지 않는다.
- 실행 도중의 진단 도구 로그가 영향을 주었을 가능성이 있어 해당 통합 테스트를 진단 도구 병행 없이 단독 재실행했다. 최종 결과는 아래에 기록한다.
- 최소 재현용 임시 오브젝트는 삭제했다. Title 씬의 원래 7개 루트가 일치하는 것을 확인했다. 진단 후 저장에서 생긴 직렬화·줄바꿈 차이도 작업 시작 시 clean이었던 Title 원본 바이트로 복구해 Scene 변경을 남기지 않았다.
- 별도 EditMode 전체 재실행: NOT RUN. 이전 수정 작업에서 115개 통과한 기록은 이전 DevLog의 결과이며 이번 점검에서 새로 실행한 것으로 집계하지 않는다.

## 단독 재실행 결과

Result: PASS

`GameFlowPlayModeTests.TitleForestQuestsDungeonBossAndReturn` 단독 재실행: 1 PASS, 0 FAIL, 약 2분 44초. 타이틀→필드 퀘스트→던전→보스→복귀 통합 흐름이 통과했다. 최초 전체 실행의 프레임워크 예외는 재현되지 않았다. 이로써 90개 고유 PlayMode 테스트에 대해 각각 통과 결과를 확보했으나, 최초 한 번의 전체 실행이 90/90이었다고 기록하지 않는다.

최종 Editor 상태: playing=False, compiling=False, compileFailed=False, Title 씬 dirty=False. Git diff whitespace 검사 통과.

## 남은 문제 및 미검증 항목

- 위 세 건은 이번 점검에서 발견·검증한 수정 전 이슈다.
- 실제 체스 컷씬의 전체 수동 플레이는 미실행이다.
- 저장된 Production 코드·Serialize 필드·Scene·Prefab 변경 없음. 본 점검 문서만 새로 추가했다.
