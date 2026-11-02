# 제스터 던전 포탈 G 키 상호작용 연결

## 작업 목적
기존 마을의 노란 제스터 던전 포탈이 새 공통 상호작용 방식에서도 정상 동작하도록 안내 UI와 G 키 이동을 연결한다.

---

## 분석
`BossDungeonShortcutPortal`이 `InteractableObject`를 상속하도록 바뀌면서 기존 노란 포탈은 상호작용 잠금(`IsLocked=true`) 상태였고 UI 참조도 없었다. 기존 목적지와 `targetLevel=10`은 유지되어 있었지만, 새 상호작용 흐름에서는 G 입력으로 던전에 입장할 수 없는 상태였다. 또한 안내 문구가 스크립트에서 고정되면 골렘/던전 포탈 간 표시를 구분할 수 없었다.

---

## 수정 파일
- Assets/01. Scene/Map1-Forest.unity
- Assets/02. Scripts/System/SceneMove/BossDungeonShortcutPortal.cs
- Docs/DevLogs/2026-10-28_JesterDungeonPortalInteraction.md

---

## 수정 클래스 / 함수
### BossDungeonShortcutPortal
- Start(): 인스턴스별 `interactName` 값을 안내 문구로 초기화

---

## 주요 변경 내용
- 노란 포탈에 골렘 포탈과 분리된 월드스페이스 안내 UI를 연결했다.
- 안내 문구를 `G - 제스터 던전 입장`으로 설정하고, `IsLocked=false`, Interactable 레이어(8), Portal 상호작용 유형을 지정했다.
- G 키 상호작용 후 기존 목적지 `(124.94, -9.36, 65.72)`로 이동하도록 연결했다.
- 기존 `targetLevel=10`을 유지했다.
- 공통 스크립트의 안내 문구를 고정 문자열 대신 인스턴스별 `interactName` 값으로 읽도록 변경해, 노란 포탈과 빨간 골렘 포탈 문구가 서로 덮어쓰지 않게 했다.

---

## Public API 변경
없음 (기존에 적용한 `BossDungeonShortcutPortal : InteractableObject` 상속 관계는 유지)

---

## Multi-Agent 작업
- 사용 여부: 미사용
- Main Agent 역할: 기존 포탈 설정 확인, UI 및 상호작용 설정, 씬 저장, Unity 상태 확인 및 DevLog 작성
- 주요 리뷰 지적사항: 해당 없음

---

## Unity 검증
### Editor / Scene / Hierarchy
Result: PASS
노란 포탈에 Canvas, 안내 패널, TMP 텍스트, 활성 상호작용 잠금 해제, Layer 8 및 기존 destination 참조를 확인했다. Scene 저장 후 `IsDirty=false`를 확인했다.

### Compile
Result: PASS
AssetDatabase Refresh 성공. 런타임 타입 확인에서 `BossDungeonShortcutPortal`의 기반 클래스가 `InteractableObject`이고 `IInteractable` 구현이 참임을 확인했다. 빌드는 실행하지 않았다.

### Console
Errors: 코드/컴파일 오류는 확인되지 않음. 최근 로그의 Error는 목적지 Transform을 포탈 GameObject의 컴포넌트처럼 조회하려 한 MCP 검사 호출 실패임.
Warnings: 확인하지 않음
Exceptions: 확인하지 않음

## EditMode Tests
Result: NOT RUN

## PlayMode Tests
Result: NOT RUN

## 발견된 문제
해당 Scene에 동일 스크립트를 사용하는 포탈 인스턴스가 둘 있어, 안내 문구는 각 인스턴스의 `interactName`을 사용해야 했다.

## 남은 문제
실제 플레이에서 G 키 입력과 던전 이동을 확인하지 않았다.

## 미검증 항목
실행 중 상호작용, Console Warning/Exception, 자동 빌드는 확인하지 않았다. 요청에 따라 빌드는 실행하지 않았다.

## 최종 결과
마을의 노란 포탈을 제스터 던전 입구로 가는 G 키 상호작용 포탈로 연결하고 기존 레벨 10 설정을 보존했다.
