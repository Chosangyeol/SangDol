# 박쥐 보스 공격 간격 조정

## 작업 목적

박쥐 보스 공격 사이에 대기시간을 추가해 공격이 연달아 이어지는 현상을 완화한다.

## 분석

각 공격 코루틴은 회복 직후 `OnPatternEnd()`를 호출했고, 보스 AI는 다음 프레임에 즉시 다음 사용 가능 패턴을 골랐다.

## 수정 파일

- `Assets/02. Scripts/Enemy/Boss/Dungeon1/D1_MiddleBoss.cs`
- `Assets/03. Prefab/Enemy/Boss/D1/Middle/ClockworkBatBoss.prefab`

## 주요 변경 내용

- 박쥐 보스 공격 코루틴마다 회복 뒤 1.2초 공통 대기시간을 추가했다.
- 대기시간은 `postAttackDelay` 직렬화 값으로 조정할 수 있고, 박쥐 보스 프리팹에는 1.2초로 설정했다.

## Public API 변경

없음. 새 설정 필드는 private SerializeField다.

## Multi-Agent 작업

- 사용 여부: 미사용
- Main Agent 역할: 코드 분석, 수정 및 Unity 검증
- 주요 리뷰 지적사항: 해당 없음

## Unity 검증

- AssetDatabase Refresh: PASS
- Compile: PASS (Unity compile completed; Console has no errors)
- Console Warnings: 기존 미사용 이벤트/필드 경고가 기록됨. 이번 수정 파일 경고 없음.
- PlayMode / EditMode Tests: NOT RUN

## 최종 결과

각 공격 종료 후 다음 공격까지 1.2초 간격이 생기도록 수정했다. Unity AssetDatabase 갱신과 컴파일 확인은 성공했고 Console Error는 0건이다. 자동 테스트와 실제 플레이 검증은 수행하지 않았다.
