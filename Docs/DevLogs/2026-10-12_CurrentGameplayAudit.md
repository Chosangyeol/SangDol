# 현재 버그 및 사용자 조작·경험 감사 (2026-10-12)

## 작업 목적

사용자 요청: 현재 개발 상태에서 남은 버그와 조작/경험 개선 후보를 확인한다.
이번 작업은 감사와 재현이며 게임 코드·Scene·Prefab 수정은 수행하지 않았다.

---

## 검사 범위 / 방법

- 실제 SangDol Unity: Application.dataPath=D:/UnityProjects/SangDol/Assets, ProductName=BackUp.
- Title / Main / Map1-Forest / Circus-Main-Hall의 실제 Editor 로드 검사.
- 등록된 EditMode 및 PlayMode 전체 실행.
- 플레이어 조작/상호작용/UI, 퀘스트/대화, 스탯/장비/제작에 대한 독립 읽기 전용 리뷰.
- 기존 코드로 임시 Editor 씬에서 별도 기능 재현. 기존 자산과 게임 데이터는 저장하지 않았으며 생성한 오브젝트/ScriptableObject와 임시 씬은 정리했다.

셸 실행 도우미가 실행 이전 helper_unknown_error로 실패하여 rg/git 점검은 NOT AVAILABLE. Unity MCP 스크립트 실행으로 파일과 실제 Editor를 조회했다.

---

## 실제 재현으로 확인한 버그

### P1 — 무적 플래그가 피해를 차단하지 않음
- CharacterModel.cs:799–803은 Buff.isInvincibility를 검사하지 않고 Stat.Damaged를 호출.
- InvincibilityBuff.cs:20은 InvincibilityEnable만 호출하고 C_Buff.cs:189에서 true 설정.
- 실제 CharacterModel/C_Buff 호출: flag=True, HP 100 → 90 (10 피해).
- 피해 적용의 공통 진입점에서 무적 및 사망 상태를 검사해야 한다.
- 근거: Assets/02. Scripts/Character/CharacterModel.cs, Character/C_Buff.cs, Character/Buff/InvincibilityBuff.cs.

### P1 — 공간이 부족한 제작에서 비용만 소모되고 결과 유실
- NpcCraftManager.cs:65 UseGold → :69 재료 제거 → :74 AddItem 순서.
- C_Inventory.cs:57은 AddItemWithResult 결과를 버린다. 빈 슬롯이 없으면 결과를 지급하지 않는다.
- 실제 재현: 2칸을 모두 채움, 재료 스택 10개에서 1개 소모하는 새 장비 제작. 골드 100 → 80, 재료 10 → 9, 결과 아이템 없음.
- 제작 결과/재료/골드를 한 번에 검증·교환해야 한다. 비용 사용 직전에 재검증하고 실패 시 소비가 없어야 한다.
- 근거: Assets/02. Scripts/UI/Npc/NpcCraftManager.cs:61, Character/C_Inventory.cs:57.

### P1/P2 — 옵션이 닫히지 않고 UI 종료 목록과 화면 상태가 어긋남
- UIManager.cs:130은 openedUIList.Count == 0일 때만 OptionUI.Toggle을 호출.
- 첫 열기 후 목록에 Option이 추가되므로 닫기 호출은 실제 패널을 닫지 않고 목록만 제거.
- :133은 실제 MainBackground 패널 대신 항상 활성인 OptionUI 컴포넌트 루트를 읽는다. UIManager.prefab의 실제 참조 구조도 확인.
- 실제 호출 재현: 첫 열기 panel=True, CloseTopUI 후 panel=True.
- PlayerInputs.cs:152와 UIManager.cs:49의 두 ESC 처리 경로도 현재 inputactions Escape→UI 연결로 확인.
- HideGameUIs():87–96은 패널만 숨기고 목록을 비우지 않는다. 숨긴 Inventory가 다음 ESC에서 다시 열리는 소스 경로가 있다.
- 개선: 입력 처리 경로 통합, 실제 보이는 패널 상태 기반 종료, 숨김과 목록 정리 동기화.
- 근거: Assets/02. Scripts/UI/UIManager.cs:87, :128, :160; UI/Option/OptionUI.cs:104; Character/PlayerInputs.cs:152; Assets/InputSystem_Actions.inputactions.

### P2 — HP 장비 교체가 임시 최대 HP를 기준으로 현재 HP를 깎음
- C_Equipment.cs:54 기존 보너스 제거 → :55 새 보너스 추가.
- CharacterStat.RemoveMaxHp():308은 제거 즉시 현재 HP를 clamp.
- 실제 EquipItem 호출: 기본 HP100, 기존 장비 +50/현재 HP150에서 새 장비 +100으로 교체. 최종 최대 HP200인데 현재 HP100.
- 장비 교환의 최종 스탯을 계산한 뒤 HP를 한 번 제한해야 한다.
- 근거: Assets/02. Scripts/Character/C_Equipment.cs:54; Character/Data/CharacterStat.cs:301.

실행 로그: Docs/DevLogs/2026-10-12_FunctionalRepros.txt.

---

## 소스·자산에서 확인한 추가 결함 (새 경로의 실제 입력 재현은 NOT VERIFIED)

### P1 — 사망/연출 잠금 중 G 상호작용 경로에 상태 검사 누락
- PlayerInputs.Update의 사망 gate는 직접 호출되는 OnInteract 콜백에는 적용되지 않는다.
- C_Controller.RequestInteract 및 CharacterModel.TryInteract에 사망/외부 잠금 검사가 없다.
- NpcBase도 이미 대화 중인지 여부만 검사한다. 사망 후 부활 대기 또는 연출 중 NPC/퀘스트 이벤트를 시작할 수 있는 경로다.
- 홀딩 루틴도 사망/외부 잠금으로 취소하지 않는다.
- 근거: Character/PlayerInputs.cs:126; Character/C_Controller.cs:156; Character/CharacterModel.cs:1065, :1123.

### P2 — Z 아이덴티티 충전 연결 없음
- CharacterStat 생성 시 idenCurrent=100, ResetIden에서 0으로 설정.
- Skill_Z.cs:14는 게이지 100을 요구하고 사용 후 ResetIden.
- Main이 모든 Production .cs의 idenCurrent/GainIden 사용을 재검색: UI 표시, 초기화, Reset/Gain 메서드 및 래퍼뿐. 실제 적중/시간 경과로 GainIden을 호출하는 곳 없음.
- Review가 모든 .anim/.prefab/.unity/.controller/.asset에도 GainIden 참조가 없는 것을 확인.
- 최초 사용 이후 자연 재충전될 연결이 없다. 충전 방식은 설계로 정하고 해당 사건에 연결해야 한다.
- 근거: Character/Skill/Skills/SkillBase/Skill_Z.cs:14; Character/Data/CharacterStat.cs:175; Character/CharacterModel.cs:922.

### P2 — 분쇄(S3) 스탯이 무력화 계산에 소비되지 않음
- C_SpecialStat.cs:71은 DownPower를 추가하지만 전체 Production의 downPower 사용은 선언/초기화/추가/제거/재계산뿐.
- 실제 공격의 무력화 피해 계산에 특성 값이 연결되지 않는다.
- 근거: Character/C_SpecialStat.cs:70; Character/Data/CharacterStat.cs:353.

### P2 — 가장 가까운 잠긴 오브젝트가 다른 유효 상호작용을 막음
- CharacterModel.TryInteract():1078에서 거리만으로 한 collider를 선택하고 :1090에서 잠긴 경우 종료.
- 가까운 잠긴 대상 1m / 먼 사용 가능한 대상 2m이면 사용 가능한 대상의 안내가 보여도 입력이 무시되는 구조.
- 후보를 사용 가능 여부로 필터한 뒤 가장 가까운 유효 대상을 선택할 필요.
- 근거: Character/CharacterModel.cs:1065.

### 확정 안내 오류 — 민첩 효과 설명이 실제 적용과 반대
- StatusUI.cs:178–179: 공격속도 2%, 이동속도 4%.
- C_SpecialStat.cs:66–67 실제: 이동속도 2%, 공격속도 4%.
- 둘 중 설계값을 확정하고 설명/계산 일치 필요.
- 근거: UI/Stat/StatusUI.cs:178; Character/C_SpecialStat.cs:66.

---

## 사용자 조작·경험 개선

### 진행도와 상호작용 피드백
- QuestPreview는 선택 시에만 수량을 표시하고 진행 이벤트를 구독하지 않는다. 열린 상세창과 실시간 QuestTracker 수치가 달라질 수 있다.
- NPC 대화 성공 시 InteractableObject.cs:108이 G 안내를 숨긴다. NpcDialogManager.CloseUI():147–153은 안내를 다시 표시하지 않는다. 범위를 벗어났다 재진입할 때까지 재대화 안내가 사라지는 경로다.
- 진행 중 퀘스트를 다시 설명하는 선택지, NPC 머리 위 시작/완료 표식, 보고할 NPC/목적지 안내를 추가할 가치가 있다.

### 전투 예고
- 실제 Circus 씬의 D1_MiddleBoss 설정을 SerializedObject로 확인: combatScaleMultiplier=0.5 (연출20/전투10), spellRadius=8, swipeRadius=3, pulseRadius=2, pulseRange=7.
- 새 짤패턴에는 표시가 있지만 기존 반경 8m 주문에는 표시가 없다. 바닥 범위/시전 예고와 실제 피해 범위를 일치시키면 회피 타이밍을 알기 쉬워진다.
- 전투 스케일10만으로 근접 공격 불가능이라고 단정하지 않는다. 크기와 공격 도달 거리의 장시간 체감 평가는 NOT VERIFIED.

### 성장/던전/종료 안내
- 던전 입장 UI에 권장 레벨/장비/퀘스트 상태를 보여주면 성장 목표가 명확해진다. 현재 요구사항에 없던 입장 제한을 버그로 세지 않았다.
- 몬스터 구간별 난이도와 레벨별 경험치 곡선은 밸런스 점검 후보이며, 수치 변경은 이번 작업에서 수행하지 않았다.
- 현재 개발 데모가 보스 클리어 후 종료되므로 타이틀 복귀와 진행 초기화 정책을 사용자에게 알려야 한다. 진행 저장 미구현은 기존 데모 목표와 다를 수 있어 기능 요구사항으로 분류한다.
- 옵션을 실제 일시정지로 쓸지, 게임이 계속되는 설정창으로 쓸지 명확히 결정해야 한다.

---

## 후보 / 미검증 사항

- 점프 도중 JumpObject를 끄거나 삭제하면 현재 공중 위치에서 Agent만 다시 켜는 취소 경로다. NavMesh 밖에서 이동 불능으로 남는지 실제 중간 취소 재현 필요.
  근거: Interact/JumpObject.cs:79, Character/CharacterModel.cs:1224. 정상 착지 테스트 통과와 별개.
- Forest 로드 중 tree11_LOD1의 Soft Occlusion/billboarding 경고. 실제 거리별 나무 표시·조명 이상은 화면 검증 필요.
- 빌드/실기기, 사람이 실제 입력으로 전체 퀘스트 루틴을 연속 플레이, 모든 해상도/UI 겹침 및 장시간 밸런스 평가는 NOT RUN.
- 이번 감사가 모든 가능한 버그가 없음을 증명하지 않는다. 확인 범위와 재현 결과로 판단한다.

---

## Unity 검증

### Editor / Scene / Hierarchy
Result: PASS
- 처음 Title 열린 상태, Play/Compile false, dirty false.
- Title(18 objects), Main(890), Forest(2086), Circus(605) 조회: MissingScripts=0 모두.
- 영구 Button.onClick target 누락 0.
- 실제 SerializedObject로 Title/SceneChanger/던전 핵심 참조 확인. EditorJsonUtility의 scene reference instanceID:0 출력은 null 근거로 쓰지 않음.
- 종료 후 Title dirty가 올라와 별도 Temp/Audit20261012/TitleLiveCopy.unity를 saveAsCopy하여 원본과 정확히 동일함 확인. 실제 변경이 없음을 입증한 뒤 원본 Title 재열어 dirty false로 복원했다.
- 최종 Title, Play false, compileFailed false. 게임 씬/프리팹을 저장하지 않았다.

### Compile
Result: PASS. 새 코드/패키지 변경 없음.

### Console
검사 전 Error/Exception=0. 기존 테스트/사용하지 않는 필드 컴파일 경고가 있었다.
전체 테스트 이후 조회 Error2/Exception1은 아래 진단 도구의 Editor.log sharing violation에서 나온 동일 오류다.
최종 단독 재실행 테스트는 Error 로그 0. Game runtime의 새로운 Error로 확인한 것은 없음.
Warning65에는 의도된 실패 경로(없는 장면, 비디오 강제 decode failure, 빈 clip/보상 공간), 테스트 rig Animator/NavMesh 준비 경고 등이 포함. 나무 billboard 경고는 별도 화면 검증 후보로 남겼다.

---

## EditMode Tests

Result: PASS
전체 EditMode 실행: Passed102 / Failed0 / Skipped0, 2.714초.
Assembly 한 개만 실행한 85개 결과와 혼동하지 않는다.

---

## PlayMode Tests

최초 전체 실행:
- Passed83 / Failed1 / Skipped0 / Total84.
- 5분34초 후 실제 종료. MCP 호출은 300초 제한으로 먼저 시간초과.
- Main이 종료 후 Tool_Tests._resultCollector의 실제 Summary 및 84개 기록을 조회해 결과를 회수했다.
- 최초 Result: FAIL, 단독 실패는 GameFlowRegressionPlayModeTests.BoxPatternYieldsAndEndsWhenGroundIsMissing.
- 원인: Review가 진행 상황 조회 시 Editor.log를 기본 File.ReadAllLines로 읽어 Sharing violation Exception을 발생시켰으며, Unity TestRunner가 이를 해당 실행 중 테스트의 unexpected log로 처리했다. 게임 패턴의 assertion 실패가 아니다.
- 안전한 FileShare.ReadWrite 읽기로 진단 방식을 정정했다.

단독 재실행:
- GameFlowRegressionPlayModeTests.BoxPatternYieldsAndEndsWhenGroundIsMissing PASS.
- Passed1 / Failed0 / Logs(error filter)0, 2.764초.
- 따라서 84개 테스트를 모두 실행했고 각 테스트에 대한 PASS 근거가 있지만, 최초 전체 실행을 단일 깨끗한 PASS로 기록하지 않는다.

실행된 suite에는 실제 Title→Forest 재진입/성장·퀘스트·던전 연결, 보스/60% 연출·공유 배우·BGM·사망/종료, 이동/점프/스킬/인벤토리/보상/풀/비디오 회귀 테스트가 포함된다.
현재 남은 네 가지 핵심 결함은 해당 자동 테스트에 없던 경로를 별도 재현해 확인했다.

결과 파일:
- Docs/DevLogs/2026-10-12_PlayModeFullResults.txt
- Docs/DevLogs/2026-10-12_FunctionalRepros.txt

---

## 수정 파일 / Public API / Serialized 데이터

게임 코드·테스트·Scene·Prefab 변경 없음.
추가 산출물은 이 감사 보고서와 실행/재현 기록뿐.
Public API 변경 없음. 기존 Serialize 데이터 및 사용자 변경을 되돌리지 않음.

---

## Multi-Agent 작업

사용.
- Main: 범위 결정, 실제 Unity 상태/씬 조회, 전체 테스트 실행 및 timeout 결과 회수, 실패 재실행, 핵심 4건 실제 기능 재현, 최종 검토/보고.
- Review: 조작/상호작용/UI, 입력 자산/옵션 프리팹 읽기 전용 검토.
- Audit Quest: 퀘스트/대화/진행 안내 검토.
- Audit Stats: 장비/스탯/제작/성장 검토.
- 각 Agent는 게임 파일 수정이나 테스트 실행을 하지 않았고, 직접 재현하지 않은 항목은 미검증으로 구분했다.
- 중복 UI 발견을 통합했고, 구매량 제한이 있는 shop overflow 의심은 일반 사용자 재현 근거가 약해 핵심 결함에서 제외했다.
- 리뷰 도구의 로그 읽기 오류가 전체 테스트 한 건에 영향을 준 것을 숨기지 않고 별도 재실행으로 확인했다.

---

## 최종 결과

남은 버그가 확인되었다. 무적 피해 차단, 제작 트랜잭션, 옵션/UI 종료 처리, HP 장비 교환을 우선 수정할 필요가 있다.
조작 잠금 검증, 게이지 충전/분쇄 효과 연결, 퀘스트/NPC 안내 갱신과 민첩 설명도 후속 수정 대상이다.
이번 요청은 확인이므로 게임 코드 수정은 하지 않았다.
