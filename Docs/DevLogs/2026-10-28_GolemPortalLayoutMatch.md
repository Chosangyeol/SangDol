# 골렘 포탈 설정을 제스터 포탈과 일치

## 작업 목적

골렘 필드 보스 포탈과 제스터 던전 포탈의 색상 외 설정 차이를 줄여, 안내 UI 위치와 상호작용 영역이 일관되게 보이도록 수정한다.

## 분석

Unity MCP에서 두 포탈을 비교한 결과 골렘 포탈의 안내 Canvas가 로컬 Y +2.5에 있었고, 트리거 SphereCollider 중심은 `(0, 1, 0)`, 반경은 `1.5`였다. 제스터 포탈은 Canvas 위치 `(0, 0, 0)`, collider 중심 `(0, -1.34, 0)`, 반경 `2.8`이었다. 텍스트와 부모 UI의 pivot은 모두 `(0.5, 0.5)`로 같았다.

## 수정 파일

- `Assets/01. Scene/Map1-Forest.unity`
- `Docs/DevLogs/2026-10-28_GolemPortalLayoutMatch.md`

## 주요 변경 내용

- 골렘 포탈 SphereCollider 중심을 `(0, -1.34, 0)`, 반경을 `2.8`로 맞췄다.
- 골렘 포탈 안내 Canvas의 anchored position을 `(0, 0)`으로 맞췄다.
- 안내 Canvas와 텍스트 pivot을 `(0.5, 0.5)`로 고정했다. 텍스트 위치와 pivot 값은 제스터 포탈과 동일하게 유지했다.
- 포탈의 맵 내 위치와 보스 이동 기능, 포탈 색상은 건드리지 않았다.

## Public API 변경

없음.

## Multi-Agent 작업

- 사용 여부: 미사용

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

- Unity MCP로 `Map1-Forest` 씬의 두 포탈을 비교하고 골렘 포탈의 collider와 Canvas 값을 확인했다.
- 씬 저장 후 `IsDirty=false`를 확인했다.

### Compile

Result: NOT RUN

- 코드 변경이 없어 컴파일을 실행하지 않았다.

### Console

- 최근 5분 내 Error: 0건.
- Warning / Exception: 조회하지 않음.

## EditMode Tests

Result: NOT RUN

## PlayMode Tests

Result: NOT RUN

## 미검증 항목

- 실제 플레이 장면에서 안내 텍스트의 시각적 높이와 접근 시 상호작용을 확인하지 않았다.

## 최종 결과

골렘 포탈의 안내 위치와 상호작용 콜라이더를 제스터 던전 포탈과 같은 값으로 맞췄다. 씬과 계층 구조는 Unity MCP에서 확인하고 저장했다.
