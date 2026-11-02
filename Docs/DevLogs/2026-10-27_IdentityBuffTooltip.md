# 아덴 버프 툴팁 잔류 수정

## 작업 목적

아덴 버프 슬롯에 마우스를 둔 상태에서 버프가 만료되거나 슬롯이 재사용될 때 설명 UI가 남는 문제를 수정한다.

---

## 분석

기존 `BuffSlot`은 `OnPointerExit`에서만 공유 툴팁을 닫았다. 버프 만료로 `BuffList`가 슬롯을 비활성화하거나 다른 버프로 재바인딩하면 포인터 종료 이벤트가 발생하지 않을 수 있다.

---

## 수정 파일

- `Assets/02. Scripts/UI/Buff/BuffSlot.cs`
- `Assets/02. Scripts/UI/Buff/BuffTooltip.cs`
- `Assets/Tests/Editor/BuffTooltipCases.cs`
- `Assets/Tests/EditMode/BuffTooltipEditModeTests.cs`
- `Assets/Tests/PlayMode/BuffTooltipPlayModeTests.cs`

Scene/Prefab 수정 없음.

---

## 수정 클래스 / 함수

### BuffSlot

- `Init()`, `OnPointerExit()`, `OnDisable()`

### BuffTooltip

- `ToggleTooltip()`, `HideFor()`

---

## 주요 변경 내용

공유 툴팁에 현재 표시 소유 슬롯을 기록하고, 해당 슬롯이 비활성화되거나 다른 버프로 재사용될 때만 툴팁을 닫는다. 다른 슬롯의 설명은 유지한다.

---

## Public API 변경

없음. 기존 public 메서드 시그니처와 SerializeField 이름을 유지했다. `HideFor()`는 internal로 추가했다.

---

## Multi-Agent 작업

- 사용 여부: 사용.
- Main Agent: 원인 분석, 수정, Unity 검증, 결과 통합.
- Review Agent: 버프 만료·슬롯 재사용 경로와 수정 후 소유자 처리, API/Serialize 위험을 읽기 전용으로 검토.
- 주요 지적사항: `OnDisable`뿐 아니라 `Init`의 재바인딩도 처리해야 하며 공유 툴팁의 다른 슬롯을 닫지 않아야 한다. 모두 반영했다.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS. Title 씬이 열려 있고 dirty=false, 루트 7개. PlayMode 테스트 후 Editor 모드로 복귀했다.

### Compile

Result: PASS. Unity Asset Refresh 후 테스트 어셈블리까지 로드됐고, `dotnet build Assembly-CSharp.csproj --no-restore`도 오류 0건이었다.

### Console

Errors: 0. Exceptions: 0.

Warnings: 테스트가 의도적으로 발생시킨 경고와 기존 NavMesh 경고가 기록됐다. 버프 툴팁 관련 경고는 없었다.

---

## EditMode Tests

Result: PASS. `PlayerMovement.EditModeTests`: Passed 106, Failed 0, Skipped 0. 신규 툴팁 테스트 2개 포함.

---

## PlayMode Tests

Result: PASS. `BuffTooltipPlayModeTests.RuntimeDisableHidesHoveredDescription`: Passed 1, Failed 0.

---

## 발견된 문제

EditMode에서는 runtime-only 컴포넌트의 `OnDisable`이 자동 호출되지 않아 테스트에서 해당 콜백을 직접 호출했다. 별도 PlayMode 테스트에서 Unity의 실제 비활성화 경로를 확인했다.

---

## 남은 문제 / 미검증 항목

실제 5초 아덴 버프 만료부터 `BuffList` 갱신까지 이어지는 게임 화면 조작은 수동 재현하지 않았다. 사용자 중단 요청 직전에 PC 빌드 호출을 시작했다. 호출을 취소했지만 Unity 내부 작업은 계속되어 `Build/PC_2026-10-27/` 빌드가 Succeeded(Errors=0, Warnings=23)로 완료됐다. 새 ZIP과 WebGL 빌드는 시작하지 않았고 추가 빌드를 진행하지 않는다.

---

## 최종 결과

버프 슬롯의 비활성화·재사용 시 설명 UI가 남는 경로를 수정하고 회귀 테스트를 통과했다.
