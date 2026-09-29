# 프로젝트 구조 및 Unity 상태 분석

## 작업 목적

게임 코드를 수정하지 않고 프로젝트 구조, 주요 시스템, 실제 Unity Editor 상태와 테스트 가용성을 분석한다.

## 분석

### 환경 및 폴더

- Unity 2022.3.62f3. MCP가 반환한 Application.dataPath는 D:/UnityProjects/SangDol/Assets로 작업 경로와 일치한다.
- manifest 기준 URP 14.0.12, Input System 1.14.2, Cinemachine 2.10.7, AI Navigation 1.1.7, Test Framework 1.1.33을 사용한다. 패키지는 변경하지 않았다.
- Assets/01. Scene: 주요 게임 씬. Build Settings에는 Title, Main, Map1-Forest, Circus-corridor, Circus-Main-Hall 및 테스트용 씬을 포함한 12개 씬이 활성화되어 있다.
- Assets/02. Scripts: C# 파일 196개. Character 45, Enemy 59, Interact 11, Items 13, System 20, UI 46, 루트 2개.
- Assets/03. Prefab 및 번호별 Material/Sprite/Animations/Model/Sound/Textures/Timeline 폴더: 게임 리소스.
- Assets/Resources: 아이템 ScriptableObject, NPC 대화·퀘스트·상점 CSV와 원본 XLSX.
- Assets의 여러 외부 에셋 및 데모 폴더와 별도 Scenes/MainGame/Skill 폴더가 함께 존재한다. 실제 빌드 씬 목록을 기준으로 게임 진입점을 구분해야 한다.
- Assets 아래 asmdef/asmref 및 NUnit/UnityTest 테스트 선언은 검색에서 발견하지 못했다. 모듈은 폴더와 클래스 단위로 나뉘어 있다.

### 실행 흐름

1. TitleManager.StartGameRoutine이 Main 씬을 Additive로 로드한다.
2. SceneChanger.LoadScene이 첫 필드를 로드한다. Title 씬의 targetScene은 Map1-Forest이다.
3. Title 씬을 언로드하고 Main을 유지한 채 필드·던전을 교체한다.
4. SystemBootstrapper.Execute는 Title 외 씬에서 시작할 때 Main이 없으면 Additive로 붙인다.
5. SceneChanger는 이전 필드 언로드, 비동기 로딩, 스폰 위치 이동, 카메라 스택 갱신을 담당한다.

### 주요 시스템

| 시스템 | 핵심 코드 | 역할 및 연결 |
|---|---|---|
| 플레이어 | CharacterModel, PlayerInputs, C_Input, C_Controller | MonoBehaviour가 입력·이동·전투와 일반 C# 하위 시스템을 구성한다. NavMeshAgent, Animator, Cinemachine을 사용한다. |
| 스탯·성장 | C_Stat, CharacterStat, StatValue, C_SpecialStat | 기본값과 고정/비율 보너스, 체력·경험치·레벨·재화를 관리한다. StatValue 계산식은 (baseValue + flatBonus) * (1 + percentBonus)이다. |
| 스킬·버프 | C_SkillSystem, SkillBase/SkillBaseSO, C_Buff, C_Stigma | 슬롯 등록, 차지·해제, 쿨다운, 스킬 레벨, 버프와 낙인 효과를 관리한다. CharacterModel.Update에서 갱신한다. |
| 아이템·장비 | C_Inventory, C_Equipment, ItemBase/ItemBaseSO | 슬롯·스택·소비 아이템·장비 스탯을 관리한다. 아이템 데이터와 런타임 인스턴스를 분리한다. |
| 일반 적 | EnemyBase, EnemyModel, StateMachine, State 파생 클래스 | Idle/Patrol/Chase/Attack/Return/Die 등의 FSM을 사용한다. |
| 보스 | BossModel, BossPatternBase, 개별 보스 패턴 | 일반 적 FSM과 별도로 패턴의 쿨다운·거리 및 보스별 동작을 구성한다. |
| 필드·던전 | FieldManager, EnemySpawner, DungeonManager, SectorController | 스폰·리스폰, 구역 활성화 및 클리어, 워프·부활·연출을 관리한다. |
| NPC·퀘스트 | DialogManager, QuestManager, NpcBase | Resources CSV를 읽고 대화 분기, 처치·아이템·대화 목표와 퀘스트 상태를 관리한다. |
| UI | UIManager 및 Inventory/Skill/Stat/Quest/Npc UI | 싱글톤과 이벤트를 통해 모델 상태를 표시하고 상점·제작·강화 등을 연결한다. |
| 공통 기반 | GameEvent, PoolManager, AudioManager, OptionManager | 정적 Action 기반 알림, 글로벌/스테이지 풀, 오디오 및 PlayerPrefs 기반 음량 설정을 제공한다. |

CharacterModel은 1,174줄이며 여러 하위 C# 클래스로 기능을 나눴지만 전투·상호작용·이펙트와 외부 매니저 연결이 집중되어 있다. GameEvent와 싱글톤은 시스템 연결을 단순화하지만 초기화 순서와 독립 테스트에 영향을 준다. 전체 재설계보다 관련 기능의 경계와 테스트부터 확인하는 접근이 적합하다.

## 수정 파일

- Docs/DevLogs/2026-09-29_ProjectAnalysis.md 신규 작성.
- 게임 코드, Scene, Prefab, ProjectSettings, Package 수정 없음.
- 작업 전부터 있던 manifest.json, packages-lock.json, PackageManagerSettings.asset 변경 및 .agents/, .codex/, AGENTS.md, Assets/Plugins 관련 미추적 항목을 보존했다.

## 수정 클래스 / 함수

없음. MCP script-execute는 파일에 저장하지 않는 Editor 상태 조회에만 사용했다.

## 주요 변경 내용

분석 보고서만 추가했다. 사용자의 '아직 코드는 수정하지 말라'는 범위에 따라 프로젝트 문제는 수정하지 않았다.

## Public API 변경

없음. Serialize 필드 및 Scene/Prefab 참조 변경 없음.

## Unity 검증

### Compile

Result: PASS (현재 Editor 컴파일 상태 확인 범위)

- 조회 전후 IsPlaying=False, IsCompiling=False, IsUpdating=False, EditorUtility.scriptCompilationFailed=False.
- 테스트 도구는 AssetDatabase Refresh를 수행하는 도구이며 두 모드 호출 후에도 같은 정상 상태를 확인했다.
- 코드 변경이 없으므로 강제 전체 재컴파일와 Player 빌드는 수행하지 않았다. 이 결과는 전체 빌드 성공을 의미하지 않는다.
- 활성 씬은 Assets/01. Scene/Title.unity, IsDirty=False, 루트 7개.
- 루트: Main Camera, Directional Light, TitleUI-Image, EventSystem, TitleManager, Canvas, TitleInput.

### Console

MCP LogCollector의 조회 가능한 캐시를 severity별 최대 10,000건으로 조회했다. Console을 지우지 않았다.

- Errors: 4. 모두 이번 진단 도구 호출에서 발생했다. 상태 조회의 Newtonsoft 참조 실패로 2건, 두 테스트 모드의 No tests found로 각 1건이다.
- Exceptions: 1. 위 임시 진단 코드의 Newtonsoft 참조 실패와 동일한 원인이다.
- 프로젝트 게임 코드에서 발생한 Error/Exception은 해당 캐시에서 발견하지 못했다. 모든 과거 실행의 무오류를 보장하지 않는다.
- 진단 코드 실패는 외부 JSON 라이브러리 대신 문자열 반환으로 수정한 뒤 재실행에 성공했다. 프로젝트 파일을 수정하거나 패키지를 설치하지 않았다.
- Warnings: 204. 모두 분석 전 22:09:09~22:10:23 KST 기록이다.

| 기존 경고 | 건수 |
|---|---:|
| EventSystem 중복: 2 event systems / only one active | 163 |
| InputAction 콜백 안 IsPointerOverGameObject 사용 | 32 |
| 유효 NavMesh 없음 또는 Agent가 NavMesh에서 멂 | 6 |
| Animator IdenFinish 파라미터 없음 | 1 |
| IdenEnd → Idle 전환에 조건/Exit Time 없음 | 1 |
| tree11_LOD1 셰이더 관련 | 1 |

현재 열린 Title 씬의 상태와 과거 Play 실행 로그를 구분했다. 과거 경고의 재현 테스트는 수행하지 않았다.

## EditMode Tests

Result: NOT AVAILABLE

MCP tests-run(testMode=EditMode)을 필터 없이 호출했으나 No tests found를 반환했다.
Passed: 0 / Failed: 0 (실행된 테스트 케이스 없음) / Not Run: 실행할 테스트 발견 안 됨.

## PlayMode Tests

Result: NOT AVAILABLE

MCP tests-run(testMode=PlayMode)을 필터 없이 호출했으나 No tests found를 반환했다.
Passed: 0 / Failed: 0 (실행된 테스트 케이스 없음) / Not Run: 실행할 테스트 발견 안 됨.

테스트용 이름의 씬은 존재하지만 자동 테스트와는 다르다. 테스트 신규 작성은 코드 변경에 해당하므로 이번 분석에서는 수행하지 않았다.

## 발견된 문제

### 실제 로그로 확인한 문제

- PlayerInputs.OnSkillSlotStarted (111행): InputAction 콜백에서 UI 포인터 상태를 읽어 이전 프레임 상태가 사용된다는 경고.
- CharacterModel.IdenDisable (684행): IdenFinish 트리거가 Animator에 없다는 경고.
- EventSystem 중복, NavMesh 배치, Animator 전환 설정은 해당 필드/런타임 조합을 재현하여 확인할 필요가 있다.

### 정적 분석상의 위험: 런타임 재현 전

- C_Inventory.AddItem은 가득 찬 경우 반환하지만 성공 수량을 반환하지 않는다. DropItemModel.Interact는 호출 후 무조건 풀로 회수한다. 일부 또는 전부를 수납하지 못하면 바닥 아이템이 사라질 가능성이 있다.
- C_Equipment.UnequipItem도 AddItem 성공 여부를 확인하지 않고 장비 슬롯과 스탯을 해제한다. 가득 찬 인벤토리에서 아이템 보존 검증이 필요하다.
- StatValue.ClearAll은 보너스 필드만 초기화하고 finalValue를 재계산하지 않는다. 직후 FinalValue는 이전 값일 수 있다. GetValue/Recalculate 호출 시 갱신된다. 현재 ClearAll 호출부는 검색되지 않아 실제 플레이 영향은 미확인이다.
- CharacterModel.cs:7, BossModel.cs:3, D1_Rook.cs:3의 using UnityEditor가 UNITY_EDITOR 조건 밖에 있다. Editor 정상 컴파일과 별도로 Player 빌드 호환성 확인이 필요하다.
- PoolManager.Pop에서 Reset을 수행한 뒤 EnemySpawner.SpawnEnemy가 Reset을 다시 호출한다. 중복 초기화는 코드 및 과거 로그에서 확인되지만 실제 부작용은 미검증이다.
- GameEvent 공개 정적 Action 및 매니저 직접 참조에 대한 초기화·해제·씬 재진입 회귀 테스트가 없다.
- Resources에 XLSX 원본과 Excel 임시 파일(~$ItemDataBase.xlsx)이 함께 있다. 런타임 리소스 범위 정리 여부는 추후 판단 사항이다.

## 남은 문제

위 기존 경고와 정적 위험 지점은 수정하지 않았다. 자동 테스트 기반이 없다. 저장/로드는 음량 PlayerPrefs 사용을 확인했으나 게임 진행 전체의 영속 저장 시스템은 이번 검색 범위에서 확인하지 못했다.

## 미검증 항목

- 전체 Player 빌드, 강제 전체 재컴파일.
- 플레이 직접 조작, 씬 전환·보스·아이템 경계 상황 재현.
- 모든 Scene/Prefab의 누락 참조 및 모든 에셋 전수검사.
- 런타임 성능과 메모리 프로파일링.
- 자동 테스트 통과 여부: 두 모드 모두 실행 가능한 테스트가 없다.

## 최종 결과

구조 및 주요 연결을 분석하고 실제 MCP 연결·Editor 컴파일 상태·Console·테스트 가용성을 확인했다. 현재 컴파일 실패는 없으나 기존 Warning과 자동 테스트 부재가 확인되어 프로젝트 전체 정상 동작을 선언하지 않는다. 다음 수정 단계의 우선순위는 아이템 보존 경계값, 입력/Animator/NavMesh 경고, Player 빌드 의존성, 핵심 로직의 회귀 테스트이다.
