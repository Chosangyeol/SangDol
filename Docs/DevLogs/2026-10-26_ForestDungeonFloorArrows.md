# 숲 시작점에서 던전까지 바닥 화살표

## 작업 목적

숲의 기본 시작점에서 던전 포털까지 실제 이동 가능한 길을 바닥 화살표로 안내한다.

---

## 분석

- `Map1-Forest`의 기본 시작점 `Player-Start` (`Spawn_Default`)은 `(196.31, 0, 204.76)`이다.
- `DungeonPortal`은 `(117.33, -7.78, 63.87)`에 있다.
- 두 지점 사이의 Unity NavMesh 경로는 `PathComplete`, 약 352.5m, 코너 36개이다. 직선 경로는 지형과 유적을 가로지르므로 NavMesh 경로를 사용했다.

---

## 수정 파일

- `Assets/01. Scene/Map1-Forest.unity`: 화살표 렌더 오브젝트 하나 추가.
- `Assets/07. Model/ForestDungeonFloorArrows.asset`: 35개 화살표의 결합 메시 추가.
- `Assets/04. Material/ForestDungeonArrowBorder.mat`, `ForestDungeonArrowGold.mat`: URP Unlit 재질 추가.
- 신규 에셋의 `.meta` 파일도 함께 생성됨.

---

## 수정 클래스 / 함수

없음. 런타임 스크립트나 기존 API는 수정하지 않았다.

---

## 주요 변경 내용

- 시작점에서 5m 지점부터 약 10m 간격으로 던전 방향 화살표 35개를 배치했다.
- 각 화살표의 꼭짓점을 주변 지형/유적 바닥의 높이에 맞췄다.
- 경사 구간에서 묻히던 7개 화살표를 필요한 만큼(최대 0.18m) 올렸다.
- 충돌체, NavMeshObstacle, 그림자를 추가하지 않아 이동 경로를 변경하지 않는다.
- 금색 중심과 어두운 테두리를 하나의 메시, 두 재질로 그린다.

---

## Public API 변경

없음.

---

## Multi-Agent 작업

- 사용 여부: 사용.
- Main Agent: 경로 계산, 메시와 재질 생성, 씬 배치, Unity 검증 및 저장.
- Research/Review Agent: 기본 스폰/포털 및 기존 씬 구조를 독립 조사하고 변경된 YAML·GUID·메시 구성을 읽기 전용 검토.
- 주요 지적: 경사진 중간 구간의 화살표 매몰/부유 위험. Main Agent가 각 삼각형의 바닥 간격을 검사해 매몰 0개로 보정했다.
- 기존 씬 데이터 변경, Serialize/API 파손, 참조 파손은 리뷰에서 발견되지 않았다.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS. `Map1-Forest`를 저장 후 다시 열었고 dirty=false, 루트 22개를 확인했다. 새 `ForestDungeonFloorArrows`의 MeshFilter/MeshRenderer 및 메시·재질 참조가 유지된다. 임시 미리보기 오브젝트는 제거했다.

### Compile

Result: PASS. 스크립트 변경 없음. Editor `isCompiling=false`, `isPlaying=false` 확인.

### Console

최근 2분 Error=0, Exception=0, Warning=0. 작업 초기에 Renderers가 없는 Terrain에 스크린샷을 요청해 MCP 도구 오류 1건이 기록됐지만 게임 코드 오류는 아니다. 이전의 tree shader 경고는 이 변경과 무관하다.

### 경로 및 표시

NavMesh: `PathComplete`, 약 352.5m. 메시 490 vertices, 두 submesh에 각각 105 triangles. 시작 화살표의 금색/테두리와 실제 지면 위 표시를 Unity 렌더 스크린샷으로 확인했다. 삼각형 중심 210개 지면 검사에서 매몰 0개, 최소 간격 0.04m.

---

## EditMode Tests

Result: NOT RUN. 실행 코드 변경이 없고 해당 정적 씬 표시를 검증하는 기존 테스트가 없다.

---

## PlayMode Tests

Result: NOT RUN. 에디터의 씬 재로드·NavMesh·렌더 확인으로 정적 배치를 검증했다. 실제 플레이 카메라를 통한 전 구간 시각 검사는 수행하지 않았다.

---

## 발견된 문제

없음. 변경 전부터 있던 다른 파일의 작업 트리 변경은 유지했다.

---

## 남은 문제 / 미검증 항목

- 모든 화살표의 실제 플레이 카메라 시야에서의 가시성: NOT VERIFIED.

---

## 최종 결과

기본 숲 시작점에서 던전 포털 앞까지 NavMesh 경로를 따라 35개의 바닥 화살표를 배치하고 저장·재로드·참조·지면 간격을 확인했다.
