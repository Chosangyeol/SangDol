# 일반 공격 중 이동 입력 보류 수정

## 작업 목적
일반 공격 중 이동을 입력했을 때 공격 회복 구간이 있어도 이동이 시작되지 않는 간헐 현상을 수정한다.

---

## 분석
`C_Controller.RequestMove()`는 공격 애니메이션이 이동을 막는 구간에서 목적지를 보류한다. 기존 `Tick()`은 보류 이동을 재시도할 때 `!isAttacking && canMove`를 모두 요구했다. 연속 기본 공격에서는 공격이 진행 중인 채로 애니메이션 이벤트가 `canMove`를 잠시 허용하기 때문에, 그 회복 구간이 이동 요청을 처리하지 못하고 지나갈 수 있었다.

---

## 수정 파일
- Assets/02. Scripts/Character/C_Controller.cs
- Docs/DevLogs/2026-10-28_BasicAttackMovementBuffer.md

---

## 수정 클래스 / 함수
### C_Controller
- Tick()
- RequestMove()

---

## 주요 변경 내용
- 보류된 이동 목적지는 `canMove`가 허용되는 순간 재시도한다. `RequestMove()`가 공격 중 회복 구간에서 호출되면 기존 로직이 공격을 취소하고 이동을 시작한다.
- 새 이동 경로 설정에 성공하면 이전에 보류된 목적지를 제거해 오래된 목적지가 다음 프레임에 덮어쓰지 않게 했다.
- 공격 잠금 중에는 새 목적지를 보류하는 기존 동작을 유지한다.

---

## Public API 변경
없음

---

## Multi-Agent 작업
- 사용 여부: 사용
- Main Agent 역할: 수정 범위 결정, 코드 반영, Unity Refresh 및 Console 확인, 최종 통합
- Review Agent: 공격/이동 상태 전환과 버퍼 처리 분석. 보류 목적지 재시도 조건 및 NavMesh 실패 시 목적지 소실 가능성을 지적함.
- Test/QA Agent: 기존 `MoveClickDuringAttackLockStartsAfterAttackRecovers` PlayMode 테스트를 확인하고, 실제 애니메이션 이벤트와 연속 공격 중 입력 조합이 기존 테스트에서 다뤄지지 않는다고 보고함.
- 주요 리뷰 반영: 회복 구간에서 보류 이동을 실행하도록 조건을 완화하고, 성공한 새 경로가 이전 버퍼를 지우도록 반영함. 별도 테스트는 추가/실행하지 않음.

---

## Unity 검증
### Editor / Scene / Hierarchy
Result: NOT RUN
이번 수정은 C# 이동 상태 처리이며 Scene/Hierarchy는 변경하지 않았다. Editor 상태 확인 도구는 이번 세션에서 사용할 수 없어 확인하지 않았다.

### Compile
Result: NOT VERIFIED
Unity AssetDatabase Refresh는 성공했다. 전용 컴파일 상태 확인 도구나 빌드는 사용하지 않아 컴파일 완료 여부를 독립적으로 확인하지 않았다.

### Console
Errors: Refresh 이후 최근 3분 Error 로그 0건.
Warnings: NOT RUN
Exceptions: NOT RUN

## EditMode Tests
Result: NOT RUN

## PlayMode Tests
Result: NOT RUN

## 발견된 문제
NavMesh가 목적지를 수용하지 못할 경우 이동 요청이 실패할 수 있다. 이번 변경은 공격 회복 구간에서 정상 경로인 보류 목적지가 누락되는 문제를 대상으로 한다.

## 남은 문제
실제 연속 공격 중 클릭 이동을 플레이하며 재현 여부를 확인하지 않았다.

## 미검증 항목
Unity 컴파일 상태, 테스트, 플레이 중 실제 입력 및 Warning/Exception을 확인하지 않았다. 사용자 지시에 따라 빌드는 실행하지 않았다.

## 최종 결과
연속 공격 중 `canMove`가 허용되는 회복 구간에 보류된 이동 요청을 처리하도록 수정하고 AssetDatabase Refresh를 수행했다. 빌드와 테스트는 실행하지 않았다.
