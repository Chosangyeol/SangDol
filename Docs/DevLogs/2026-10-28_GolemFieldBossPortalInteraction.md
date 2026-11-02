# 골렘 포탈 위치 및 G 키 상호작용

## 작업 목적
빨간 골렘 포탈이 바닥에 묻히는 문제를 보정하고, 마을 중앙에서 마을 입구 동선으로 옮긴다. 가까이 다가가면 목적지 안내를 표시하고 G 키를 눌러 이동하도록 한다.

---

## 분석
기존 `BossDungeonShortcutPortal`은 트리거에 닿자마자 이동하는 `MonoBehaviour`였다. 프로젝트의 `InteractableObject`가 근접 안내 UI 표시와 G 키 상호작용 처리를 제공하지만, 포탈에는 해당 흐름과 안내 Canvas 연결이 없었다. 기존 Scene에는 빨간 포탈 비주얼 인스턴스가 원점에 하나 더 남아 있었고, 기존 포탈은 마을 시작 지점 인근 중앙에 배치되어 있었다.

---

## 수정 파일
- Assets/01. Scene/Map1-Forest.unity
- Assets/02. Scripts/Interact/InteractableObject.cs
- Assets/02. Scripts/System/SceneMove/BossDungeonShortcutPortal.cs
- Docs/DevLogs/2026-10-28_GolemFieldBossPortalInteraction.md

---

## 수정 클래스 / 함수
### InteractableObject
- OnTriggerEnter(Collider): 파생 클래스에서 오버라이드할 수 있도록 protected virtual로 변경
- OnTriggerExit(Collider): 파생 클래스에서 오버라이드할 수 있도록 protected virtual로 변경

### BossDungeonShortcutPortal
- Start(): 상호작용 초기화 후 골렘 지역 안내 문구 지정
- Interact(Transform): G 키 상호작용 성공 시 NavMesh 도착 지점으로 이동하고 카메라 워프 및 레벨 처리를 수행
- OnTriggerEnter/OnTriggerExit: 기반 상호작용 범위 이벤트에 연결

---

## 주요 변경 내용
- 빨간 포탈을 마을 중앙에서 마을 외곽 진입 동선의 NavMesh 지점 `(208, -0.64, 194)` 부근으로 이동했다. 포탈 오브젝트 높이는 지면 위쪽으로 약간 보정한 `(208, -0.34, 194)`로 설정했다.
- 접근 UI로 기존 던전 포탈의 월드스페이스 Canvas 구조를 복제해 빨간 포탈에 연결하고, 문구를 `G - 골렘 보스 지역 이동`으로 지정했다.
- 포탈 루트 레이어를 Interactable(8)로 지정하고, 기존 SphereCollider 트리거를 상호작용 범위로 사용한다.
- 터치 즉시 이동은 제거하고, 주변 진입 후 G 키를 눌러야 이동하도록 바꿨다.
- 빨간 포탈 프리팹의 원점 중복 인스턴스를 제거했다.
- 이동 목적지와 레벨 설정(targetLevel=1)은 유지했다.

---

## Public API 변경
- 변경 전: `BossDungeonShortcutPortal : MonoBehaviour`
- 변경 후: `BossDungeonShortcutPortal : InteractableObject`
- 변경 이유: 프로젝트에 이미 있는 근접 UI 및 G 키 상호작용 흐름을 재사용한다.
- 영향 범위: 씬의 컴포넌트 클래스명과 파일 ID는 유지했고 `destination`, `targetLevel` 필드는 유지했다. 새 기반 클래스가 제공하는 `IInteractable` 구현과 상호작용 UI 필드가 추가됐다. 다른 Scene/Prefab에서 이 컴포넌트가 사용되는지 검색해 Map1-Forest 단일 Scene 인스턴스만 확인했다.

---

## Multi-Agent 작업
- 사용 여부: 미사용
- Main Agent 역할: 기존 상호작용 구조 분석, 스크립트 및 Scene 수정, Unity 확인, DevLog 작성
- 주요 리뷰 지적사항: 해당 없음

---

## Unity 검증
### Editor / Scene / Hierarchy
Result: PASS
`Map1-Forest`에서 새 Canvas, 안내 텍스트, 트리거, 이동 스크립트 및 destination 참조를 확인했다. 저장 후 Scene `IsDirty=false`를 확인했다.

### Compile
Result: PASS
AssetDatabase Refresh 성공. Unity 런타임 타입 확인에서 `BossDungeonShortcutPortal`의 기반 클래스가 `InteractableObject`이고 `IInteractable` 구현이 참임을 확인했다. 빌드는 실행하지 않았다.

### Console
Errors: 마지막 Refresh 및 확인 후 0건
Warnings: 확인하지 않음
Exceptions: 확인하지 않음

## EditMode Tests
Result: NOT RUN

## PlayMode Tests
Result: NOT RUN

## 발견된 문제
없음

## 남은 문제
실제 플레이에서 접근 안내 UI의 화면상 위치/표시와 G 키 이동 동작은 확인하지 않았다.

## 미검증 항목
PlayMode 상호작용 및 Console Warning/Exception은 확인하지 않았다. 요청에 따라 빌드는 실행하지 않았다.

## 최종 결과
마을 입구 쪽에 지면에서 살짝 올라온 빨간 골렘 포탈을 배치했다. 접근 시 골렘 지역 안내를 표시하고 G 키 입력으로 이동하도록 연결했다.
