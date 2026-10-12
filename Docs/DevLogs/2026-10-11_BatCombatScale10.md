# 박쥐 중간보스 전투 스케일 10

## 작업 목적
사용자 요청대로 D1_MiddleBoss의 전투 스케일을 2에서 10으로 변경한다. 등장 연출 스케일은 20을 유지한다.

## 분석 / 주요 변경
combatScaleMultiplier를 0.1에서 0.5로 변경했다. 스크립트 기본값과 실제 인트로/단독 박쥐 프리팹 값을 모두 갱신했다.
기존 반복 전투 회귀 테스트의 기대 크기를 10으로, 해당 스케일에서의 캡슐 크기 기대값을 갱신했다. 공격 패턴이나 수치는 변경하지 않았다.

## 수정 파일 / 함수
- Assets/02. Scripts/Enemy/Boss/Dungeon1/D1_MiddleBoss.cs: combatScaleMultiplier 기본값.
- Assets/03. Prefab/Dungeon/Jester60Intro/Jester60Introduction.prefab: multiplier 0.5.
- Assets/03. Prefab/Enemy/Boss/D1/Middle/ClockworkBatBoss.prefab: multiplier 0.5.
- Assets/Tests/Editor/BossFourIssuesTests.cs: ScaleAndCameraRecoverAcrossRetries 기대값.

## Public API / Serialize 데이터
Public API 변경 없음. 기존 필드 이름/참조 유지.
Circus-Main-Hall 씬은 저장/수정하지 않고 프리팹 설정이 실제 배우에 반영되는지 조회했다.
변경 전 백업: Docs/Recovery/2026-10-11_BatCombatScale10/

## Multi-Agent 작업
미사용. 작은 수치 변경으로 Main이 수정과 Unity 검증을 수행했다.

## Unity 검증
- Asset Refresh: PASS.
- Compile: PASS. isCompiling false, scriptCompilationFailed false.
- 실제 Circus-Main-Hall 배우: presentation (20,20,20), multiplier 0.5, combat (10,10,10).
- 실제 열린 씬 복원: Title, dirty false, Play Mode 종료.
- Console Error: 0 (최종 2분 조회).
- 관련 PlayMode 테스트: PASS, 1 passed / 0 failed.
  BossFourFixes.BossFourIssuesPlayModeTests.ScaleAndCameraRecoverAcrossRetries
  두 번의 20 → 10 → 20 전환, 카메라 복구, NavMesh 배치, 캡슐 크기 검사 통과.
- 테스트 로그 경고 2건: inactive object Animator.Update, 초기 유효 NavMesh 없음. 테스트 초기 준비 구간의 경고이며 이후 실제 agent.isOnNavMesh 검사 통과. Error/Exception은 반환되지 않음.

## EditMode Tests
NOT RUN. 관련 변경은 스케일 설정으로 기존 PlayMode 회귀 테스트를 실행했다.

## 미검증 항목
전체 테스트/빌드/장시간 수동 전투는 NOT RUN. 새 테스트 파일은 추가하지 않았다.

## 최종 결과
전투 스케일 10 적용 및 반복 진입/연출 크기 복원 검증 완료.
