# 스킬 레벨 제한 및 보스 던전 지름길 포탈

## 작업 목적

일반 스킬 습득을 5레벨부터 허용하고, 마을 입구에서 던전 입구로 바로 이동하며 5레벨 보상을 받도록 한다.

---

## 분석

스킬 데이터에는 `requireLevel`이 있었지만 습득 로직과 UI에서 사용하지 않았다. 기존 레벨업은 경험치 처리 시 스킬 포인트, 스탯 포인트, 기본 능력치를 함께 지급한다. 마을 시작점과 기존 던전 입구는 모두 `Map1-Forest` 씬에 있다.

---

## 수정 파일

- `Assets/02. Scripts/Character/C_SkillSystem.cs`, `C_Stat.cs`
- `Assets/02. Scripts/UI/Skill/SkillTreeSlot.cs`, `SkillTreeUI.cs`
- `Assets/02. Scripts/Character/Skill/Skills/Datas/Skill_1.asset` ~ `Skill_4.asset`
- `Assets/02. Scripts/System/SceneMove/BossDungeonShortcutPortal.cs`
- `Assets/01. Scene/Map1-Forest.unity`
- `Assets/Tests/Editor/SkillLevelAndShortcutTests.cs`
- `Assets/Tests/EditMode/SkillLevelAndShortcutEditModeTests.cs`

---

## 수정 클래스 / 함수

- `C_SkillSystem.LevelUpSkill()`, `MeetsRequiredLevel()`, `CanLevelUpSkill()`: 레벨, 잔여 포인트, 최대 스킬 레벨을 확인하고 나서 포인트를 차감한다.
- `SkillTreeSlot.Refresh()`: 요구 레벨 미달일 때 아이콘과 텍스트를 60% 밝기로 표시하고 레벨업 버튼을 잠근다.
- `SkillTreeUI.HandleStatChanged()`: 레벨과 포인트 변경 직후 슬롯을 갱신한다.
- `C_Stat.RaiseToLevel()`: 목표 레벨까지 기존 경험치 레벨업 경로를 반복한다.
- `BossDungeonShortcutPortal.OnTriggerEnter()`: 플레이어의 레벨을 목표치까지 올리고 던전 입구 앞으로 이동한다.

---

## 주요 변경 내용

일반 스킬 4개의 요구 레벨을 각각 5로 지정했다. 회피 및 아이덴티티 기본 스킬은 유지했다. 마을 입구 근처 `(200, -0.45, 208)`에 포탈 시각 효과와 트리거를 추가하고 기존 던전 입구 앞 `Player-Start (1)`을 목적지로 연결했다. 포탈 컴포넌트의 `targetLevel`은 Inspector에서 변경 가능하며 기본값은 5다. 목표 레벨보다 높은 플레이어의 레벨은 내리지 않는다.

---

## Public API 변경

- 변경 전: 지름길 포탈 컴포넌트 없음.
- 변경 후: 새 `public BossDungeonShortcutPortal` MonoBehaviour 추가. 기존 Public API 변경 없음.
- 변경 이유: 씬 포탈에 붙일 직렬화 가능한 동작이 필요함.
- 영향 범위: `Map1-Forest`의 새 포탈 오브젝트.

기존 SerializeField 이름, Scene/Prefab 참조 및 패키지는 변경하지 않았다.

---

## Multi-Agent 작업

- 사용 여부: 사용.
- Main Agent: 구현, 씬 수정, Unity 검증 및 통합.
- Skill Review Agent: `requireLevel` 재사용, 레벨 변경 이벤트와 최대 레벨 포인트 차감 문제 제안. 반영함.
- Portal Review Agent: 동일 씬 목적지와 기존 경험치 레벨업 보상 경로, NavMesh 위치 제안. 목적지 탐색 또는 이동 실패 시 레벨만 지급될 수 있다는 최종 지적을 반영하여 이동 확인 후 레벨을 지급함.
- Skill Review Agent의 최종 검토: 데이터 4개 및 UI 갱신 연결을 확인했고, 실제 UI 색과 버튼 표시의 시각 검증 공백을 지적함.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS. 저장된 `Map1-Forest` 씬에서 포탈 1개, 자식 시각 효과, 트리거 SphereCollider, 목적지 Transform 및 목표 레벨 5를 확인했다.

### Compile

Result: PASS. AssetDatabase Refresh 이후 `isCompiling=false`, `scriptCompilationFailed=false`.

### Console

최종 컴파일 오류: 0, 최근 Console Error/Exception: 0. 테스트 작성 중 발생한 접근 제한 컴파일 오류는 테스트를 수정한 후 해소했다. 기존 Unity MCP 연결 관련 오류 로그는 이번 코드와 무관하다. PlayMode에서 `PoolManager`가 NavMesh에서 먼 위치에 에이전트를 생성한다는 Warning이 발생했으며 포탈 코드의 호출 경로는 아니다.

---

## EditMode Tests

Result: PASS. 재검증에서 전체 126/126 통과. 새 테스트 3개는 레벨 4 잠금과 포인트 유지, 레벨 5 습득, 최대 스킬 레벨 포인트 유지, 목표 레벨 보상 및 반복 호출, 실제 슬롯의 색상과 버튼 상태 전환을 확인했다.

---

## PlayMode Tests

Result: NOT VERIFIED. 전체 PlayMode 테스트 실행이 Unity MCP 도구의 300초 제한으로 종료되어 최종 요약을 받지 못했다.

별도 PlayMode 동작 확인: PASS. 재검증에서 `Map1-Forest`의 실제 플레이어를 포탈 위치로 이동시켜 물리 트리거가 자동 발동하는 것을 확인했다. 레벨 1→5, 잔여 스킬 포인트 1→5, 스탯 포인트 0→4, 위치 `(124.94, -9.61, 65.72)` 이동이 확인됐다. 스킬 슬롯 관련 PlayMode 테스트 1개도 개별 실행하여 통과했다.

---

## 발견된 문제

Unity MCP의 테스트 클래스 필터로 개별 EditMode 테스트를 찾지 못해 전체 EditMode 테스트로 검증했다.

---

## 남은 문제 / 미검증 항목

PlayMode 전체 테스트의 최종 결과와 실제 키보드 조작으로 포탈까지 이동하는 과정은 확인하지 못했다. 스킬 UI의 색상과 버튼 상태는 자동 테스트로 확인했으며 실제 게임 화면의 육안 검사는 수행하지 않았다. 이전 PlayMode 전체 실행은 Unity MCP의 300초 제한으로 결과를 받지 못했다.

---

## 최종 결과

스킬 요구 레벨과 UI 잠금, 마을 지름길 포탈 및 정상 레벨업 보상을 구현했다. 빌드는 사용자 요청에 따라 실행하지 않았다.
