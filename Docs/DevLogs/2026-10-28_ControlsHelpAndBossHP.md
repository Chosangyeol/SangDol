# 조작키 안내 및 보스 체력 설정

## 작업 목적

옵션의 조작키 안내를 채우고 숲 시작 지점에 안내 문구를 표시하며, 제스터와 시계 박쥐의 체력을 요청한 값으로 설정한다.

---

## 분석

옵션 UI에는 세 번째 탭이 있었지만 조작키 내용과 탭 동작이 연결되지 않았다. 숲 씬에는 시작 지점 안내 문구가 없었다. 보스 체력 값은 EnemyStatSO 에셋에서 관리된다.

---

## 수정 파일

- Assets/02. Scripts/UI/Option/OptionUI.cs
- Assets/Scenes/Map1-Forest.unity (Unity Editor에서 저장)
- Assets/02. Scripts/Enemy/SO/D1_FinalBoss.asset
- Assets/02. Scripts/Enemy/SO/D1_MiddleBoss.asset

## 수정 클래스 / 함수

### OptionUI

- Start()
- InitializeControlKeysPanel()
- ShowSoundPanel()
- ShowControlKeysPanel()
- Toggle()

## 주요 변경 내용

- 옵션의 조작키 탭에 좌클릭 공격, 우클릭 이동, Q/W/E/R/Z 스킬, ESC/P/K/L/I 키 안내를 표시하도록 했다.
- 조작키 안내 하단에 마을의 노란색/빨간색 포탈을 이용한 빠른 이동과 P/K키로 성흔·특성·스킬을 설정한 뒤 입장하라는 안내를 추가했다. 늘어난 내용이 패널에 맞도록 글자 크기를 24로 조정했다.
- 숲의 `Player-Start` 아래에 `ControlsInstructionText`를 추가하고 `ESC - 조작키를 눌러 조작키를 확인하세요`를 바닥에 표시하도록 했다.
- 제스터 최대 체력을 30,000으로 설정했다.
- 시계 박쥐 최대 체력을 사용자 후속 요청에 따라 2,500으로 롤백했다.

## Public API 변경

없음

## Multi-Agent 작업

- 사용 여부: 미사용
- Main Agent 역할: 요구사항 반영, Unity 에셋 설정, 최종 확인
- 주요 리뷰 지적사항: 없음
- 반영 여부와 이유: 해당 없음

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

Unity MCP에서 숲 씬을 열어 시작 지점 자식 안내 오브젝트를 확인하고 씬을 저장했다.

### Compile

Result: NOT VERIFIED

AssetDatabase ForceUpdate를 수행했다. Unity 컴파일 완료 상태를 별도로 확인하지 않았다.

### Console

Errors: 기존/작업 중 기록된 로그에 TMP 3D Text의 `outlineWidth` 편집으로 발생한 에디터 재질 경고가 포함되어 있다. 현재 `outlineWidth`는 0으로 되돌렸다. 이 항목은 런타임 컴파일 오류가 아니다.

Warnings: 기존 C# 미사용 멤버 경고와 숲 씬 트리 셰이더/NavMesh 경고가 있다.

Exceptions: 작업 변경과 직접 연관된 새 런타임 예외는 확인되지 않았다.

## EditMode Tests

Result: NOT RUN

## PlayMode Tests

Result: NOT RUN

## 발견된 문제

TMP 3D Text의 외곽선 두께를 에디터에서 설정할 때 Unity가 `renderer.material` 생성 경고를 남겼다. 외곽선은 0으로 복구했다.

## 남은 문제

컴파일, 테스트, 실제 플레이 화면은 이번에 확인하지 않았다.

## 미검증 항목

- Unity 컴파일 완료 및 Console 신규 로그 여부
- 조작키 탭의 런타임 표시
- 숲 시작 지점 바닥 문구의 플레이 화면 가독성
- EditMode / PlayMode 테스트
- PC 및 WebGL 빌드

## 최종 결과

조작키 안내와 하단 빠른 체험 문구, 숲 시작 지점 문구, 제스터 체력 30,000, 시계 박쥐 체력 2,500을 반영하고 숲 씬을 저장했다. 이번 추가 변경 후 AssetDatabase ForceUpdate를 실행했다. 빌드와 테스트는 실행하지 않았다.

## 추가 변경 (2026-10-28, ControlKeysPanel 연결)

- `OptionUI.InitializeControlKeysPanel()`에서 `Section/ControlKeysPanel`을 찾아 참조하도록 변경했다.
- 스크립트에서 ControlKeysPanel과 TextMeshPro 오브젝트를 만들고 문구를 덮어쓰던 코드를 제거했다.
- 조작키 탭은 기존 `ControlKeysPanel`을 활성화하고 사운드 패널을 비활성화하며, 사운드 탭은 반대로 동작한다.
- Unity MCP Hierarchy에서 `UIManager/OptionUI/MainBackground/Section/ControlKeysPanel` 경로와 자식 `ControlKeysText`를 확인했다.
- AssetDatabase ForceUpdate 후 최근 2분 구간의 Console Error는 0건이었다. Unity 컴파일 완료 상태는 별도로 확인하지 않았고, 테스트와 빌드는 실행하지 않았다.
