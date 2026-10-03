# 던전 완료 후 선택 가능한 Title 복귀

## 작업 목적

D1_FinalBoss 처치 및 던전 완료 후 현재 개발 진도 종료 지점인 시작 화면(Title)으로 돌아간다. 기능은 보스에 넣지 않고 독립 오브젝트/프리팹으로 추가·제거·이동할 수 있도록 구성한다.

---

## 분석

기존 FinalBossSector는 보스의 사망 연출 완료까지 기다리고 SectorController는 마지막 섹터의 클리어 Timeline이 있는 경우 그 재생 완료 후 DungeonManager.OnSectorCleared를 호출한다. 기존 DungeonManager의 최종 처리는 5초 후 Map1-Forest 복귀였다. 따라서 보스 사망 자체가 아니라 확정된 던전 완료 시점에 선택 가능한 복귀 처리를 연결했다.

Main은 게임플레이 공용 시스템 씬이다. 사용자에게 보여줄 메인화면은 게임 시작 UI가 있는 Title 씬으로 해석했다. 기존 SceneChanger의 필드 이동은 플레이어 유지 목적이므로 메뉴 복귀는 Title Single 로드로 구현했다. 유일한 게임플레이 DontDestroyOnLoad 객체인 PoolManager는 Main이 살아 있을 때 먼저 제거하고 다음 프레임에 씬을 전환한다.

---

## 수정 파일

- Assets/02. Scripts/Enemy/Dungeon/DungeonManager.cs
- Assets/02. Scripts/Enemy/Dungeon/DungeonCompletionReturnToTitle.cs 및 meta
- Assets/03. Prefab/Dungeon/DungeonEnd_ReturnToTitle.prefab 및 폴더/프리팹 meta
- Assets/01. Scene/Circus-Main-Hall.unity
- Assets/Tests/Editor/DungeonDepartureTests.cs 및 meta
- Assets/Tests/EditMode/DungeonDepartureEditModeTests.cs 및 meta
- Assets/Tests/PlayMode/DungeonDeparturePlayModeTests.cs 및 meta

기존 작업/사용자 변경은 유지했다. 이번 요청에서 보스 스크립트·보스 프리팹·패키지·ProjectSettings는 수정하지 않았다.

---

## 수정 클래스 / 함수

### DungeonManager

- OnSectorCleared(): null, 빈 목록, 다른 던전 섹터, 완료 중복 거부.
- OnDungeonComplete(): 첫 번째 수락한 완료 처리 컴포넌트가 복귀를 담당.
- ResumeDefaultDeparture(): 선택 컴포넌트가 없거나 취소되면 기존 필드 복귀 재개. 비활성/제거 중인 Manager에서는 코루틴 시작 금지.
- DungeonOut(): 기존 복귀 코루틴 추적.

### DungeonCompletionReturnToTitle

- BindDungeon(): 명시 참조 또는 같은 씬 DungeonManager 자동 연결.
- TryHandleCompletion(): 로드 가능한 메뉴 씬 검증, 중복 방지, 필요한 경우 플레이어 조작 잠금.
- ReturnToTitle()/LoadTitleScene(): 실시간 대기, timeScale 복구, 풀 제거, Title Single 로드.
- OnDisable(): 구독/대기 취소, 자신이 건 조작 잠금만 해제, 기존 복귀 재개.

---

## 주요 변경 내용 및 Inspector 사용법

현재 Circus-Main-Hall 씬 루트에 **DungeonEnd_ReturnToTitle** 프리팹 인스턴스를 배치했다. 루트는 22개에서 23개로 증가했으며 기존 Sector1/2/3/SectorFinal 순서를 유지한다.

프리팹 경로: `Assets/03. Prefab/Dungeon/DungeonEnd_ReturnToTitle.prefab`

| Inspector 항목 | 현재 값 | 사용법 |
| --- | --- | --- |
| Dungeon Manager | None | 같은 씬의 DungeonManager 자동 연결. 여러 Manager가 있거나 다른 씬의 Manager를 연결할 때 직접 지정. |
| Title Scene Name | Title | 복귀할 시작 화면 씬 이름. Build Settings에서 활성화되어 있어야 한다. |
| Return Delay | 5 | 보스 사망 및 마지막 섹터 연출 완료 후 기다릴 실제 시간(초). 0으로 즉시 복귀 가능. |

- 프리팹을 원하는 던전 씬에 드래그하거나 원하는 별도 GameObject에 컴포넌트를 추가할 수 있다.
- 같은 씬 안의 Transform 위치나 부모를 옮겨도 던전 완료 이벤트 연결은 유지된다. 위치 진입형 Trigger가 아니라 완료 이벤트를 듣는 오브젝트다.
- 오브젝트/컴포넌트를 끄거나 제거하면 Title 복귀 기능이 빠지고 기존 Map1-Forest 복귀를 사용한다.
- 대기 중 제거해도 복귀가 멈추지 않도록 기존 이동을 재개한다. 씬 전환이 이미 확정된 후에는 Manager가 전환 코루틴을 이어서 수행한다.
- Title 메뉴 로드 시 기존 플레이어·UI·필드 및 전역/로컬 풀을 정리해 새 게임 시작과 충돌하지 않도록 한다.
- 씬에 독립 오브젝트를 배치하는 용도이며 DDOL로 유지하며 여러 던전에 재사용하는 기능은 구현하지 않았다.

---

## Public API 변경

- 추가: public sealed class DungeonCompletionReturnToTitle : MonoBehaviour.
- 이유: Inspector에서 독립적으로 추가·제거할 컴포넌트 제공.
- 기존 public 메서드/필드/클래스 및 SerializeField 이름은 변경하지 않았다.
- DungeonManager.CompletionRequested, ResumeDefaultDeparture는 internal로 추가했다. 기존 OnSectorCleared 호출부는 그대로 동작한다.

---

## Multi-Agent 작업

- 사용 여부: 사용.
- Main Agent: 구현, 프리팹/씬 배치, 테스트 작성, Unity MCP 실행 및 최종 검증, 보고서.
- Review Agent: 프로덕션 코드 및 테스트 독립 검토. 직접 파일 수정/테스트 실행 없음.
- 별도 Test/QA Agent: 실행 슬롯 제한으로 사용하지 못함. Main Agent가 테스트 및 실패 로그 분석을 직접 수행.
- 리뷰 반영: 전환 확정 직후 오브젝트 삭제 검증, 실제 Title→StartGame 재시작 검증, assertion 실패 시에도 실행되는 UnityTearDown 정리, 서로 다른 중간/마지막 섹터, 다른 시스템이 소유한 조작 잠금 보존.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

Main Agent가 Unity MCP로 직접 확인: PlayMode 종료, 컴파일 종료, Circus-Main-Hall 활성, 저장 완료(dirty=false), 루트 23개, 새 컴포넌트 1개 활성, 프리팹 연결, Manager None/Title/5초. 마지막 SectorFinal에 FinalBossSector 1개 존재. 프리팹에 씬 Manager 참조가 없는 것을 확인.

### Compile

Result: PASS

최종 AssetDatabase.Refresh 성공 및 실제 Editor isCompiling=false. 최초 테스트 코드가 내부 프로퍼티에 직접 접근해 발생한 컴파일 오류는 public 조작 상태 검증으로 수정했다.

### Console

Errors: 0

Exceptions: 0 (최종 Error 카운트 및 테스트 실패 없음)

Warnings: 21 (Console 초기화 후 최종 EditMode/PlayMode 실행 누적)

경고 구분: 잘못된 Title 이름/사용 불가능한 컷씬/비디오 실패를 의도적으로 검증하는 테스트 경고, 기존 NavMesh fixture 경고, MCP 재연결/로그 저장소 경고, 실제 Title 비디오 색 정보 경고, 기존 Title→Main 시작 시 EventSystem 일시 중복 경고 및 Forest의 나무 shader/NavMesh 경고. 경고는 숨기지 않았으며 이 요청에서 씬/에셋 전반을 수정하지 않았다.

---

## EditMode Tests

Result: PASS

Passed: 75

Failed: 0

Not Run: 0 (선택한 PlayerMovement.EditModeTests assembly)

Duration: 1.7032351초

추가 3개: null/빈/외부 섹터 거부, 완료 단일 처리 및 handler 순서, 중간 섹터 완료로 복귀하지 않음.

MCP Summary.TotalTests=92는 트리 노드가 포함된 값이며 PassedTests=75를 실제 통과 개수로 기록했다.

---

## PlayMode Tests

Result: PASS

Passed: 61

Failed: 0

Not Run: 0 (선택한 PlayerMovement.PlayModeTests assembly)

Duration: 53.4369615초

추가 4개:

- 대기 중 비활성/제거 시 기본 복귀와 조작 복구. 다른 시스템의 잠금 유지.
- 컴포넌트 부재/잘못된 Title에서 기존 복귀 사용.
- 자동 연결, 위치 이동, 재활성화 후 단일 구독.
- timeScale=0에서도 실시간 대기 후 복귀, 전환 확정 직후 컴포넌트 삭제, 실제 Title Single 로드, 기존 풀과 sentinel 제거, Title.StartGame으로 Main/Map1-Forest 재시작 및 새 플레이어/UI/풀 확인.

기존 보스 사망·컷씬/60% 패턴·워프·스탯·인벤토리·퀘스트·대화 등의 관련 회귀 테스트도 같은 assembly에서 통과했다.

---

## 발견된 문제

최초 전체 PlayMode: 60 PASS / 1 FAIL. 부모 GameObject DestroyImmediate 중 isActiveAndEnabled만 검사하면 비활성 GameObject에서 복귀 코루틴 시작을 시도했다. 실제 activeInHierarchy 검사 추가 후 단독 및 전체 재실행 PASS.

테스트가 실제 씬 전환 뒤 실패하는 경우 후속 테스트를 오염시키는 정리 누락은 UnityTearDown으로 보강했다.

---

## 남은 문제

위 Console 항목의 기존 씬/에셋 및 MCP 경고. Title 복귀 기능의 실행 오류는 최종 검증에서 발견되지 않았다.

## 미검증 항목

- 실제 플레이로 던전 전 구간을 진행하여 D1_FinalBoss를 직접 처치하는 수동 검증: NOT RUN. 완료 연결과 기존 보스 사망/섹터 처리 회귀 테스트, Inspector 구조 및 실제 메뉴 왕복으로 검증했다.
- Standalone 빌드 실행: NOT RUN. Unity Editor의 실제 씬 로드 및 메뉴 재시작으로 검증했다.
- 선택 assembly 외의 테스트: NOT RUN.

---

## 최종 결과

던전 완료 후 선택적으로 Title로 돌아가는 별도 프리팹을 구현하고 현재 던전에 배치했다. 위치 이동/비활성/삭제가 가능하며 Inspector에서 복귀 화면 및 대기 시간을 변경할 수 있다. 최종 Compile PASS, Console Error 0, EditMode 75 PASS, PlayMode 61 PASS.
