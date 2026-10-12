# 직접 재현된 Gameplay 버그 4건 수정

## 작업 목적
2026-10-12 감사에서 실제 재현한 무적 피해 처리, 제작 비용/결과 유실, 옵션 닫기, HP 장비 교체 시 체력 손실을 수정한다.

---

## 분석
- 무적 플래그를 설정해도 CharacterModel.Damaged와 C_Stat.Damaged가 피해를 적용했다.
- 제작은 골드/재료를 먼저 사용하고 결과 AddItem 실패를 무시했다.
- UIManager와 PlayerInputs가 ESC를 각각 처리했고, OptionUI 실제 패널 대신 활성인 컴포넌트 루트를 검사했다. 숨김/닫기 시 스택도 어긋났다.
- 장비 교체의 기존 보너스 제거 단계에서 임시 최대 HP로 현재 HP를 clamp했다.

---

## 수정 파일
- Assets/02. Scripts/Character/CharacterModel.cs
- Assets/02. Scripts/Character/C_Stat.cs
- Assets/02. Scripts/Character/Data/CharacterStat.cs
- Assets/02. Scripts/Character/C_Equipment.cs
- Assets/02. Scripts/Character/C_Inventory.cs
- Assets/02. Scripts/Character/C_Controller.cs
- Assets/02. Scripts/UI/UIManager.cs
- Assets/02. Scripts/UI/Option/OptionUI.cs
- Assets/02. Scripts/UI/Npc/NpcCraftManager.cs
- Assets/02. Scripts/UI/Npc/Craft/CraftUI.cs

신규 테스트 및 Unity .meta:
- Assets/Tests/Editor/ReproducedFixTests.cs
- Assets/Tests/EditMode/ReproducedFixEditModeTests.cs
- Assets/Tests/PlayMode/ReproducedFixPlayModeTests.cs

변경 전 백업: Docs/Recovery/2026-10-12_ReproducedFixes/
백업과 비교해 수정한 Production 파일 10개를 확인했다. 기존 소스 인코딩을 보존했다(CraftUI는 기존 UTF-8 유지).
셸 도우미 helper_unknown_error로 rg/git 조회는 NOT AVAILABLE. Unity MCP에서 직접 파일/백업 비교 수행. 기존 사용자 작업을 되돌리지 않았다.
Scene/Prefab/ProjectSettings/Package 변경 없음.

---

## 수정 클래스 / 함수와 주요 변경

### CharacterModel.Damaged / C_Stat.Damaged
- 무적 또는 사망 상태에서는 피해/상호작용 취소/피해 알림 이전에 반환.
- 상태이상 면역(isImmunity)과 피해 무적(isInvincibility)은 구분.
- 무적 해제/버프 만료 뒤에는 정상 피해 적용.

### C_Equipment.TryEquipItem / C_Stat.UpdateEquipmentStats / CharacterStat.RemoveMaxHp
- 기존 remove → add 순서를 유지하되 교체 동안 HP clamp와 스탯 알림을 유예.
- finally에서 최종 최대 HP 기준으로 현재 HP를 한 번 clamp하고 최종 스탯 알림 한 번 발생.
- 최대 HP 상승/동일 시 현재 HP를 보존하며, 하락/해제는 최종 최대 HP까지만 제한. 교체로 자동 회복하지 않음.
- 반복/비율형/다른 스탯 장비 교환과 기존 드래그 경로 유지.

### C_Inventory.ExchangeItems / NpcCraftManager.ExecuteCraft
- 기존 TryAddRewards를 유지하면서 같은 계획 로직을 재료 소비+결과 지급으로 확장.
- 재료 소비 후의 가상 슬롯/스택을 기준으로 결과 수용 여부 결정. preview는 실제 참조/수량을 변경하지 않음.
- 모든 검증 성공 시 전체 인벤토리를 반영한 다음 레시피 선택을 완료 상태로 바꾸고 골드를 차감. 외부 이벤트는 완전히 커밋된 상태만 관찰.
- 재료 일부만 소비되어 빈칸이 없는 제작은 비용 보존하며 실패.
- 재료 전량 소비로 빈칸이 생기면 가득 찬 가방에서도 제작 가능.
- 부족한 골드/재료, 중복 재료 ID 합산, 배열 불일치/유효하지 않은 결과를 검사.
- crafting guard와 커밋 시 selectedRecipe=null로 재진입/중복 지급 방지.

### CraftUI
- 결과/재료/UI 배열 검증 후 렌더링. 잘못된 레시피의 null/범위 오류 방지.
- 공간/골드 부족 원인을 기존 UI 문구에 표시하고 실행 버튼 비활성화.
- 활성 미리보기에서 InventoryUpdated와 해당 캐릭터 StatChange를 구독하여 공간/골드 확보 시 버튼 자동 갱신.
- Disable/Destroy 및 대상 변경 시 중복 안전하게 구독 해제. 기존 public 메서드와 Serialize 필드 유지.

### UIManager / OptionUI / C_Controller.RequestUI
- UIManager의 legacy ESC Update 경로 제거. 기존 PlayerInputs InputAction → C_Controller 경로에서 한 번만 처리.
- RequestUI의 StopMove 유지.
- 실제 OptionUI.optionUI 패널 상태를 검사.
- CloseTopUI는 오래된 항목을 제거하고 보이는 최상위 패널을 명시적으로 닫음.
- HideGameUIs에서 UI 종료 목록도 정리. 숨긴 인벤토리를 ESC로 다시 토글하지 않음.
- 옵션 버튼 닫기 후 남은 오래된 목록도 다음 ESC에서 정리.

---

## Public API / Serialize 데이터
Production public API 변경 없음.
기존 public 메서드/필드, Serialize 필드 이름, 클래스 이름 유지.
추가된 교환 계획/HP 배치/표시 상태 접근은 internal 또는 private.
UIManager.CloseTopUI는 private에서 internal로 바꿔 기존 입력 경로에서 사용. public API 추가/삭제는 아님.
Scene/Prefab 참조 수정이나 마이그레이션 없음.

---

## Multi-Agent 작업
- 사용 여부: 사용.
- Main: 분석, 최소 변경, 테스트 코드 작성, 실제 모든 Unity 검증 및 최종 통합.
- Review: 읽기 전용 독립 코드 리뷰. 테스트 중 Unity 조회/로그 읽기 금지하여 감사 때의 도구 로그 혼입을 방지.
- QA: 기존 테스트 helper 재사용 및 경계/재진입 회귀 전략 제안.
- 반영:
  - 새 장비를 먼저 더하는 대신 기존 순서를 보존하며 중간 HP clamp/알림 유예.
  - 제작 재료로 비워지는 슬롯을 계획에 반영.
  - 실제 패널 상태와 ESC 단일 처리, 이동 정지 유지.
  - 잘못된 레시피 UI와 공간 확보 후 버튼 자동 복원 보완.
- 에이전트는 파일 수정/테스트 실행을 하지 않았으며 PASS는 Main 실행 결과만 기록.

---

## Unity 검증

### Editor / Scene
Result: PASS
Application.dataPath=D:/UnityProjects/SangDol/Assets.
최종 Assets/01. Scene/Title.unity, dirty=false, Play Mode 종료.
Scene/Prefab 직접 수정 없음.

### Compile
Result: PASS
Assets Refresh 완료, isCompiling=false, scriptCompilationFailed=false.

### Console
최종 전체 흐름 테스트 전 로그 초기화 후:
Errors: 0
Exceptions: 0
Warnings: 15
종류: NavMesh 생성/초기 배치 경고와 기존 tree11_LOD1 Soft Occlusion/billboarding 경고.
이번 감사에서 이미 보았던 범주이며 실제 경로의 이동/장면 진행 테스트는 통과. 나무 거리별 시각 품질은 별도 미검증으로 유지.

---

## EditMode Tests
Result: PASS
전체 114 passed / 0 failed / 0 skipped (1.920초).
기존 102개 + 신규 12개:
- 무적/해제/상태이상 면역/사망과 Model·Stat 직접 피해 경로
- flat/percent HP 장비 교환, 반복, 최종 알림, 다른 스탯/장비 해제
- 제작 실패 비용/참조 보존과 재시도
- 재료 소비로 슬롯 확보, 동일 아이템 결과 병합, 중복 재료 합산
- 잘못된 데이터/골드 부족 no-op
- 알림 중 재진입/중복 결제 차단과 최종 상태 관찰
- 옵션 반복/버튼 닫기/오래된 패널/LIFO
- 실제 UIManager 프리팹의 Hide/ESC 목록
- 제작 미리보기 공간/골드 변경 갱신과 잘못된 배열

초기 신규 테스트 실패 1건은 BuffBase에 null BuffSO를 준 fixture 오류였으며 유효한 임시 BuffSO로 수정 후 PASS.
Production의 null BuffSO 허용 정책을 변경하지 않았다.

---

## PlayMode Tests
Result: PASS (관련 6개 고유 테스트)
- ReproducedFixes.ReproducedFixPlayModeTests.InvincibilityExpiresAndRestoresRuntimeDamage
- ReproducedFixes.ReproducedFixPlayModeTests.RealEscapeInputTogglesOnceAndStopsMovement
- InventoryTransferPlayModeTests.GearDropSwapsAndUpdatesIcons
- QuestRewardPlayModeTests.RewardNotificationsObserveFullCommitAndCannotReenter
- QuestRewardPlayModeTests.StackMergeAndRemovalUpdateQuestState
- GameFlowPlayModeTests.TitleForestQuestsDungeonBossAndReturn

새 두 테스트: 2 passed / 0 failed / Error logs 0.
실제 Keyboard 장치와 InputActionReference로 ESC를 누르고 떼는 두 주기를 검사. 옵션 open/close 한 번씩 및 이동 정지 확인.
실제 C_Buff의 적용/만료 뒤 피해 정상 복귀 확인.
기존 장비 UI 드래그/보상/퀘스트 이벤트 경로 확인.
전체 게임 흐름 통합 테스트: PASS, 2분42초, Error logs 0. Title → Forest 성장/퀘스트 → Dungeon → Boss → Return.

초기 fixture 오류 수정:
- standalone InputAction을 InputActionReference로 만들 수 없음 → 임시 InputActionAsset/Map을 구성.
- MovementRig에는 damage text anchor가 없는데 전역 DamageTextManager가 남아 있었음 → 해당 fixture에서 전역만 격리/복원.
두 오류는 테스트 코드에서 해결했으며 이후 새 Runtime 2개 재실행 PASS.
도구 TotalTests86은 검색된 전체 PlayMode 수. 86개 전체를 다시 실행했다고 기록하지 않음.

---

## 남은 문제 / 미검증 항목
- 이전 감사의 사망/연출 중 G 입력, Z 충전 연결, 분쇄 특성 계산, NPC/퀘스트 안내 등 직접 재현하지 않은 별도 항목은 이번 4건 수정 범위에 포함하지 않음.
- 전체 PlayMode86개 재실행/빌드/실기기/장시간 수동 체감: NOT RUN.
- 사람의 전체 조작으로 모든 분기를 연속 재현한 것은 아님. 전체 흐름은 실제 씬 통합 테스트로 확인.
- 기존 나무 billboard/초기 NavMesh 경고의 장시간 시각 품질 검증: NOT VERIFIED.

---

## 최종 결과
직접 재현한 4건을 수정했고 기존 보상/드래그/전체 흐름을 유지했다.
컴파일·전체 EditMode114개·관련 PlayMode6개 통과, 최종 새 Console Error/Exception0.
