# 박쥐 보스 패턴 반복 수정

## 작업 목적

박쥐 보스가 공격 패턴을 하나만 반복하는 것처럼 보이는 현상을 줄이고, 준비된 패턴을 고르게 사용하도록 한다.

## 분석

- 네 패턴은 등록되어 있고 각자 사거리와 쿨타임 조건으로 사용 가능 여부를 판단한다.
- 사용 가능한 패턴을 가중치 무작위로 고르는 방식이라 직전 패턴이 다시 선택될 수 있었다.
- Animator의 `Attack`, `WingSwipe`, `DoublePulse` 상태는 같은 Cast 애니메이션 클립을 공유한다.
- 주문 공격에는 별도 예고 표시가 없어 다른 패턴과 시각적으로 더 비슷해 보였다.

## 수정 파일

- `Assets/02. Scripts/Enemy/BossModel.cs`
- `Assets/02. Scripts/Enemy/Boss/Dungeon1/D1_MiddleBoss.cs`

## 주요 변경 내용

- 직전 패턴 외에 사용 가능한 패턴이 있으면 박쥐 보스 선택 목록에서 직전 패턴을 제외한다. 직전 패턴만 사용 가능할 때는 정상적으로 다시 사용할 수 있다.
- 주문 공격에 원형 범위 예고 표시를 추가하고, 발동 시 빨간색으로 바뀌도록 했다. 날개 휘두르기의 부채꼴, 2연속 충격파의 원형, 급강하의 착지 지점 표시와 시각적 구분을 강화한다.

## Public API 변경

없음.

## Multi-Agent 작업

- 사용 여부: 미사용
- Main Agent 역할: 원인 분석, 수정, Unity 검증
- 리뷰 지적사항: 해당 없음

## Unity 검증

- AssetDatabase Refresh: PASS
- Compile: PASS (Unity refresh completed; no Console compile error reported)
- Console Errors: 최근 2분간 0건
- Console Warnings: 기존 미사용 이벤트/필드 관련 경고만 확인됨. 수정 파일 경고 없음.
- EditMode / PlayMode Tests: NOT RUN

## 최종 결과

대안 패턴이 준비되어 있으면 직전 패턴을 연속 선택하지 않도록 하고, 주문 패턴의 범위 예고를 추가했다. Unity 에셋 갱신/컴파일 확인은 완료했으며 실제 플레이와 자동 테스트는 아직 확인하지 않았다.
