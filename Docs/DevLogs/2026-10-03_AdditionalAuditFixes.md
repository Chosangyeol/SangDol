# 추가 발견 사항: NPC ID 중복과 MCP PlayMode 종료 오류 수정

## 작업 목적

이전 GameplayAuditFixes 보고서에서 남긴 추가 발견 두 가지를 사용자 요청에 따라 수정한다.

- 엘린과 상인의 NPC ID 중복.
- PlayMode 종료/테스트 재연결 시 발생한 MCP SignalR 연결 취소 오류.

---

## 분석

### NPC ID

6개 NpcSO는 30001, 30002, 30003, 30004, 30003, 30006을 사용하여 엘린과 상인이 30003을 공유했다. NpcBase.Interact는 SO의 npcID를 GameEvent.OnTalkNpc로 전달하고 QuestManager는 questTarget과 비교한다. 따라서 두 NPC를 구분할 수 없었다.

30005는 미사용이다. 현재 NpcQuestDataBase.csv에는 마리아(30002) Talk 퀘스트와 마리아/다일(30002/30004) 완료 NPC 참조만 있고 엘린 관련 퀘스트는 없다. Ellen.npcQuests도 비어 있다. 실제 production 코드에서 NPC/퀘스트 저장·복원 기능을 발견하지 못했으며 PlayerPrefs 사용은 옵션 값에 한정된다.

Ellen의 Scene/Prefab 참조는 asset GUID를 사용한다. Ellen.asset GUID eb344c6a3a7d78c458bd9dd6498c6194와 대화 키 D_Ellen_001을 유지하여 참조를 보존한다. 기존 30003을 일괄 변환하면 상인까지 바뀌므로 이러한 변환은 추가하지 않는다.

### MCP 연결 오류

설치된 com.ivanmurzak.unity.mcp의 Startup.SubscribeOnEditorEvents는 Application.quitting에 OnApplicationQuitting을 연결한다. 해당 정리 메서드는 StopDevControl, DisconnectImmediate, DisposeLogCollector, runtime instance 정리를 수행한다.

Application.quitting은 PlayMode 종료 때도 발생한다. 테스트 결과를 전달하기 위한 재연결이 진행 중일 때 이 정리 경로가 실행되어 SignalR transport/handshake/negotiation 취소 오류가 발생했다. 수정 전 단일 Pumpkin PlayMode 테스트는 PASS였지만 종료 후 현재 Console에 8개 MCP 취소/연결 오류가 남는 상황을 실제로 재현했다. 오류 내용과 Stack Trace는 Temp/McpLifecycleErrorsBefore.txt에 보존했다.

---

## 수정 파일

- Assets/02. Scripts/Interact/Npc/SO/Ellen.asset
- Assets/Resources/NpcDialogDataBase.csv
- Assets/Editor/McpEditorQuitLifecycle.cs 및 Unity 생성 .meta
- Assets/Tests/Editor/GameplayAuditTests.cs
- Assets/Tests/EditMode/GameplayAuditEditModeTests.cs
- Assets/Tests/Editor/McpLifecycleTests.cs 및 Unity 생성 .meta
- Assets/Tests/EditMode/McpLifecycleEditModeTests.cs 및 Unity 생성 .meta
- Docs/DevLogs/2026-10-03_GameplayAuditFixes.md: 후속 해결 상태 추가
- Docs/DevLogs/2026-10-03_AdditionalAuditFixes.md

Scene, Prefab, Packages/manifest.json, package 버전과 Library/PackageCache 소스는 수정하지 않았다. 기존 사용자 변경을 되돌리지 않았다.

---

## 수정 클래스 / 함수

- McpEditorQuitLifecycle: static constructor, Install()
- GameplayAuditCases: NpcDialogueAssetReferencesUseTheirOwnNames(), TalkingToEllenDoesNotCompleteMerchantQuest()
- McpLifecycleCases: CleanupUsesEditorQuitAndInstallationIsIdempotent()
- EditMode reflection bridge 테스트

---

## 주요 변경 내용

### NPC 데이터 수정

- Ellen.asset npcID: 30003 → 30005.
- D_Ellen_001 CSV 행의 Npc_ID: 30003 → 30005.
- 상인의 30003, Ellen의 D_Ellen_001 대화 키 및 asset GUID는 유지한다.
- 영향: 엘린의 OnTalkNpc 이벤트 값은 30005가 되어 상인과 구분된다. 기존 퀘스트 데이터와 Scene/Prefab 참조 변경은 필요 없다.
- 저장 마이그레이션: 현재 프로젝트에서 NPC/퀘스트 저장 기능을 확인하지 못했고, 30003은 여전히 상인의 유효한 번호이므로 일괄 변환을 추가하지 않는다.

### MCP Editor 전용 호환 코드

- [InitializeOnLoad]로 설치 패키지 Startup 초기화를 먼저 완료한다. InitializeOnLoad 클래스 실행 순서에 의존하지 않는다.
- 원래 private static zero-argument void OnApplicationQuitting delegate를 Application.quitting에서 제거하고 EditorApplication.quitting에 연결한다.
- 기존 cleanup 구현을 그대로 호출하면서 실제 Editor 종료와 PlayMode 종료를 구분한다.
- 재설치 시 -= / +=로 중복 구독을 방지하고 다른 Application.quitting 리스너는 유지한다.
- AssemblyReloadEvents, Application.unloading, PlayMode 재연결과 사용자 KeepConnected 설정은 유지한다.
- private 패키지 훅이 없어지거나 signature가 달라지면 적용 실패 경고를 출력한다. optional Editor 타입은 Type.GetType으로 해석한다.
- 로그 필터·오류 숨김으로 해결하지 않았다. 검증 직전 Console을 초기화한 뒤 같은 PlayMode 종료 경로에서 새 오류가 발생하는지 직접 확인했다.
- Editor 폴더의 internal 정적 클래스이므로 Player 빌드에 포함되지 않는다.

---

## Public API 변경

없음. runtime public API, Serialize 필드명·클래스명·namespace 변경 없음.

직렬화 값 변경은 Ellen의 NPC 번호 한 개와 동일 CSV 행의 NPC 번호에 한정된다. 외부 사용자 코드가 잘못된 Ellen 번호 30003을 하드코딩하고 있다면 30005로 수정해야 하지만, 현재 프로젝트 코드에는 해당 참조가 없다.

---

## Multi-Agent 작업

- 사용 여부: 사용.
- Main Agent: 요구사항·참조 분석, 모든 코드/데이터 수정, 테스트 작성, 오류 재현, Unity MCP 최종 실행과 검증, 보고서 작성.
- Review Agent (/root/review): NPC 참조/저장 영향과 MCP lifecycle 수정 독립 읽기 전용 검토.
- Test/QA Agent 사용 시도는 세션 agent thread limit으로 실행할 수 없었다. 테스트 전략·작성·실행과 실패 분석은 Main Agent가 수행했다.
- 주요 리뷰 반영: Ellen만 30005로 변경하고 상인 번호·대화 키·GUID 보존, 모호한 30003 일괄 마이그레이션 금지, Startup 초기화 순서와 원래 cleanup 구독 유지, 실제 Editor 종료 미실행과 private 훅 호환성 한계 명시.
- 독립 리뷰에서 현재 설치 패키지에 대한 차단 결함은 발견하지 않았다. 서브에이전트는 파일 수정이나 Unity 테스트 실행을 하지 않았다.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

Main Agent가 실제 Unity MCP와 script-execute로 확인했다.

- Scene: Circus-Main-Hall, root 22개, IsDirty=false.
- EditorApplication.isCompiling/isUpdating/isPlaying 모두 false.
- AssetDatabase에서 EllenID=30005, ShopID=30003 및 Ellen GUID 보존 확인.
- Application.quitting에 MCP cleanup 구독이 없는 것을 실제 확인.
- PlayMode 종료 후 MCP Connection=Connected, KeepConnected=true. 연결 설정을 변경하지 않았다.

### Compile

Result: PASS

최종 Asset Refresh 성공. 컴파일 오류 없음.

### Console

Errors: 최종 0.

Warnings: 전체 PlayMode 회귀 실행 후 현재 Console 15개. 영상 오류/누락/타임아웃 및 보스 cutscene 실패 경로 테스트와 임시 NavMesh fixture의 기존 예상 경고가 있다.

Exceptions: 최종 0.

- 수정 전 재현: 단일 PlayMode 테스트 PASS 후 MCP 취소/연결 오류 8개.
- 수정 후 동일 단일 PlayMode 테스트: PASS, 종료 후 Console Error 0, MCP Connected.
- 수정 후 전체 PlayMode 어셈블리: 57 PASS, 종료 후 Console Error 0, MCP Connected.
- Console clear MCP tool은 Temp/mcp-server/ai-editor-logs.txt 파일 잠금으로 실패했다. 원인 로그를 보존한 뒤 UnityEditor.LogEntries.Clear로 Editor Console만 분리하고 실제 현재 항목과 counts를 읽어 검증했다. 잠긴 로그 파일이나 패키지 캐시를 임의 삭제/수정하지 않았다.

---

## EditMode Tests

Result: PASS

Passed: 72

Failed: 0

Not Run: 지정 어셈블리 외 테스트.

Unity MCP tests-run, testAssembly=PlayerMovement.EditModeTests. 도구 TotalTests=89는 발견 수이며 실제 실행 수는 PassedTests=72이다.

- 기존 NPC 대사 데이터 검증에 6개 NPC ID 고유성과 default/talk CSV 행의 NPC 번호 일치를 추가.
- 새 TalkingToEllenDoesNotCompleteMerchantQuest: 엘린과 대화해도 상인 Talk 퀘스트는 InProgress/미달성, 상인 대화 시 CanClear/달성.
- 새 CleanupUsesEditorQuitAndInstallationIsIdempotent: Install 반복 후 다른 Application 종료 리스너 보존, MCP Application 종료 리스너 부재, 원래 MCP Editor 종료 리스너 정확히 1개 확인.
- 첫 MCP 구독 테스트는 Unity EventWithPerformanceTracker의 내부 delegate 포장 구조를 단순 Delegate 필드로 검사하여 0개로 오판했다. 실제 GetEnumerator를 사용해 구독 목록을 읽도록 테스트를 수정하고 전체 어셈블리 재실행 통과.

---

## PlayMode Tests

Result: PASS

Passed: 57

Failed: 0

Not Run: 지정 어셈블리 외 테스트.

Unity MCP tests-run, testAssembly=PlayerMovement.PlayModeTests. 전체 기존 회귀 어셈블리를 재실행했다. 이 변경으로 새 PlayMode 테스트는 추가하지 않았으며 실제 기존 테스트 실행의 domain reload/PlayMode 종료를 MCP 재연결 검증에 사용했다. 동일 단일 Pumpkin 테스트도 수정 전후 각각 실제 실행했다.

---

## 발견된 문제

- NPC ID 중복과 MCP PlayMode 종료 시 취소 오류: 수정 및 재검증 완료.
- MCP Console clear 도구의 로그 파일 잠금: 검증 도구의 별도 제약. 현재 Console 검사 및 이번 수정 검증은 직접 Editor API로 완료했다.

---

## 남은 문제

요청한 두 추가 발견 사항은 해결했다.

- 패키지 업데이트로 private Startup 훅이 변경되면 적용 실패 경고를 보고 호환 코드를 재검토해야 한다. 현재 패키지는 성공적으로 적용·검증됨.
- Console clear MCP 도구의 파일 잠금은 이번 lifecycle 오류와 별도이며, 해당 도구의 파일 관리까지 수정하지 않았다.

---

## 미검증 항목

- 실제 Unity Editor 프로그램 종료: NOT RUN. 사용자의 Editor 세션을 종료하지 않았다. 원래 cleanup 메서드가 Editor 종료 이벤트에 정확히 한 번 연결된 것은 테스트로 확인했지만 실제 종료 실행 및 다른 package 서버 종료 리스너와의 순서는 미검증이다.
- Map1-Forest에서 엘린과 상인을 직접 클릭하는 수동 플레이: NOT RUN. 실제 SO·CSV 데이터를 사용한 퀘스트 조건과 AssetDatabase 참조를 검증했다.
- 외부 사용자 저장 파일/별도 도구: NOT VERIFIED. 프로젝트 내부에서 NPC/퀘스트 저장 로직은 확인되지 않았다.

---

## 최종 결과

엘린과 상인을 고유한 번호로 구분했고, MCP cleanup을 실제 Editor 종료 이벤트로 옮겨 PlayMode 종료 재연결 오류를 해결했다. 실제 컴파일 성공, EditMode 72개·PlayMode 57개 PASS, 종료 후 MCP Connected 및 Console Error 0을 확인했다. Package 버전/캐시·Scene/Prefab·public API는 유지했다.
