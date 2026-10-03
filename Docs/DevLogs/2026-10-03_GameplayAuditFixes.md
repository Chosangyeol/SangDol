# 스탯·일반 몬스터·퀘스트·대화 감사 결과 수정

## 작업 목적

이전 감사에서 보고한 10개 항목을 모두 수정한다. 기존 사용자 변경과 직렬화 데이터, 공개 API를 보존하고 실제 Unity 컴파일 및 회귀 테스트로 확인한다.

---

## 분석

- 강인(S5)의 받는 피해 배율 보너스 부호가 반대였으며, 초기화는 S5 대신 S4 투자량을 사용했다.
- 유아독존(Lv10_B)은 해제 시 회피 쿨타임 감소 2를 제거하면서 장착 시 적용하지 않았다.
- 화권(Lv5_A)은 공격력 10% 대신 고정 0.1을 적용했고 재발동 쿨타임도 설명의 15초와 달리 25초였다.
- 최대 체력 보너스 제거 후 현재 체력이 새 최대 체력을 초과할 수 있었다.
- 평타의 HashSet에 실제 적을 등록하지 않거나 검사하지 않아 여러 콜라이더에서 피해·적중 이벤트·카운터가 중복됐다.
- PumpkinGay.Attack은 기본 로그만 호출했다. 기존 BoxMeleeEnemy의 공격 판정을 재사용할 수 있었다.
- 퀘스트 완료의 void API로 인해 대화에서 지급 실패 여부를 알 수 없었다.
- 다일의 첫 수락/거절 선택지 목적지가 반대였고, 마리아/다일의 거절 대사에는 미리보기 닫기 액션이 없었다.
- 일반 대화 버튼이 로그만 출력했고 일부 NPC 데이터는 하버 대사 ID를 참조했다.

---

## 수정 파일

### Production 코드

- Assets/02. Scripts/Character/C_SpecialStat.cs
- Assets/02. Scripts/Character/Stigma/C_Stigma.cs
- Assets/02. Scripts/Character/Data/CharacterStat.cs
- Assets/02. Scripts/Character/CharacterModel.cs
- Assets/02. Scripts/Enemy/EnemyModel.cs
- Assets/02. Scripts/Enemy/NormalEnemy/PumpkinGay.cs
- Assets/02. Scripts/System/QuestManager.cs
- Assets/02. Scripts/System/DialogManager.cs
- Assets/02. Scripts/UI/Npc/NpcDialogManager.cs
- Assets/02. Scripts/Interact/Npc/NpcBase.cs

### 대화 데이터

- Assets/Resources/NpcDialogDataBase.csv
- Assets/02. Scripts/Interact/Npc/SO/Maria.asset
- Assets/02. Scripts/Interact/Npc/SO/Shop.asset
- Assets/02. Scripts/Interact/Npc/SO/Ellen.asset
- Assets/02. Scripts/Interact/Npc/SO/Yakoon.asset

### 회귀 테스트

- Assets/Tests/Editor/GameplayAuditTests.cs 및 Unity 생성 .meta
- Assets/Tests/EditMode/GameplayAuditEditModeTests.cs 및 Unity 생성 .meta
- Assets/Tests/PlayMode/GameplayAuditPlayModeTests.cs 및 Unity 생성 .meta

---

## 수정 클래스 / 함수

- C_SpecialStat: ApplySpecialStat(), ResetSpecialStat()
- C_Stigma: ApplyStigmaStats(), HandleOnHitTarget()
- CharacterStat: RemoveMaxHp(), Damaged()
- CharacterModel: OnAttackHit(), HandleBasicAttack4(), Damaged()
- EnemyModel: PerformBoxMeleeAttack() 추가
- BoxMeleeEnemy: Attack()
- PumpkinGay: Attack()
- QuestManager: CompleteQuest(), TryCompleteQuest() 추가, RefuseQuest()
- DialogManager: PlayDialogue(), ExecuteAction(), EndDialogue()
- NpcDialogManager: OpenNpcUI(), CloseUI()
- NpcBase: Interact()

---

## 주요 변경 내용

1. 강인 투자당 피해 배율을 0.01 감소시키고 S5 투자량으로 정확히 되돌린다.
2. 유아독존의 장착 시 회피 쿨타임 감소 2를 적용하여 장착·교체·해제를 가역적으로 만든다.
3. 최대 체력 보너스 제거 후 현재 체력을 0~새 최대 체력으로 제한한다. 이미 부상한 캐릭터를 회복시키지는 않는다.
4. 평타 1~4타는 한 공격 호출에서 같은 적과 같은 카운터 대상에 각각 한 번만 적용한다. 자식 콜라이더의 부모 카운터 컴포넌트도 찾는다.
5. EnemyModel의 protected 근접 공격 helper를 BoxMeleeEnemy와 PumpkinGay에서 사용한다. 기존 BoxMeleeEnemy의 public 박스 설정 필드와 PumpkinGay의 상속 구조는 유지한다. 죽은 공격자·죽은 플레이어·누락된 타겟은 제외하고 같은 플레이어의 여러 콜라이더에도 피해는 한 번만 준다.
6. 퀘스트 지급 결과와 실패 이유를 대화에 전달한다. 실패 시 성공 대사/다음 대사로 진행하는 리스너를 연결하지 않고, 실패 안내와 재시도·나가기 선택지를 표시한다. 이미 완료된 소개 퀘스트는 성공/no-op로 처리하여 사과 퀘스트 재방문을 막지 않는다. 기존 보상 원자성·재진입 방지 및 지급 후 이벤트 예외 전파를 보존한다.
7. 다일 첫 선택지의 수락/거절 목적지를 수정한다.
8. 마리아/다일 거절 대사에 Quest_Refuse 액션을 지정하고 NPC 창 종료도 미리보기를 정리한다. 대화 종료 시 dialoguePanel을 직접 닫는다.
9. 대화하기 버튼에서 talkDialogID를 재생한다. 빈 값이면 defaultDialogID로 대체한다. 마리아/상인은 자신의 기존 대사를 사용하고, 엘린/야큔에는 임시 인사 대사를 추가한다. NPC ID는 변경하지 않는다.
10. 화권은 10초 동안 공격력 10%를 올리고 15초마다 재발동하도록 수정한다.

관련 경계 보완: 강인 101포인트 이상 투자로 배율이 음수가 되어 피격이 회복으로 변하는 문제를 막기 위해 실제 피해 계산과 피해 표시에서만 배율의 최솟값을 0으로 제한한다. 원본 스탯 보너스는 유지하여 초기화가 가역적이다. NpcBase의 isInteracting은 UI 열기 전에 설정하여 즉시 종료된 대화가 다시 캐릭터를 잠그지 않게 한다.

기존 CP949 파일 3개의 인코딩을 유지했다. 시작 시 백업의 CP949 byte round-trip이 모두 동일함을 확인했다. 기존 Serialize 필드명, 클래스명, namespace, Scene/Prefab 참조 구조는 바꾸지 않았다. SO 변경은 대사 ID 문자열 값에 한정된다.

---

## Public API 변경

없음. 기존 public void CompleteQuest(string questID)를 유지한다.

- 추가 내부 API: internal bool TryCompleteQuest(string questID, out string failureMessage)
- 추가 protected helper: EnemyModel.PerformBoxMeleeAttack(float, float, float)
- ExecuteAction 변경은 DialogManager의 private 메서드에 한정된다.
- 기존 CompleteQuest 호출부는 그대로 동작하며 DialogManager만 내부 결과 API를 사용한다.

---

## Multi-Agent 작업

- 사용 여부: 사용
- Main Agent: 모든 파일 수정, 범위 통합, 회귀 테스트 작성, Unity MCP 실행, 결과 확인 및 보고서 작성.
- audit_stats / Review Agent: 스탯·전투·근접 공격 변경의 버그, 경계, API/Serialize 위험을 시작 시 백업과 비교하여 검토.
- audit_dialogue / Review Agent: 퀘스트·대화·NPC 및 CSV/SO 변경의 흐름과 호환성 검토.
- audit_quest / Test/QA Agent: 퀘스트 완료 테스트 전략, 실제 테스트 코드의 격리와 커버리지 검토.
- 모든 서브에이전트는 읽기 전용 분석만 수행했고 테스트 PASS를 직접 주장하지 않았다.
- 주요 지적 반영: 음수 피해 배율 방어 및 101포인트 회귀 테스트, 이미 Completed인 소개 퀘스트 통과, 실패 UI 나가기 제공, 실제 Dialog/Choices/Buttons 계층 재현 및 activeInHierarchy 검증, 새 PlayMode wrapper의 Main 씬 root 활성 상태 복원.
- 별도 발견: 엘린/상인 NPC ID 중복은 이번에 임의 변경하지 않고 아래 남은 문제에 기록.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

Main Agent가 Unity MCP로 직접 확인:

- 컴파일/Asset 업데이트/PlayMode 모두 종료됨.
- 열린 씬은 Assets/01. Scene/Circus-Main-Hall.unity, root 22개.
- 테스트 사전 조건 충족을 위해 기존 미저장 씬을 scene-save로 저장했다. 저장 전 파일 백업과 저장 후 파일 비교에서 serialized 내용 변경은 없었다. 씬 구조·프리팹은 이 작업에서 수정하지 않았다.
- 테스트 후 같은 씬으로 복귀했고 IsDirty=false.

### Compile

Result: PASS

최종 Asset Refresh 성공. EditorApplication.isCompiling/isUpdating=false. 전체 테스트 직후 실제 Editor Console Error 0을 확인했고, 마지막 단일 테스트의 domain reload 이후 아래 MCP 인프라 오류가 남은 것을 추가 확인했다. Compile Error는 없다.

### Console

Errors: 전체 회귀 테스트 직후 0. 최종 단일 테스트 재검증 후 Console에는 MCP/SignalR 재연결 관련 6개 항목이 남음. Gameplay/컴파일 오류는 0.

Exceptions: 최종 gameplay 실행 오류 없음. MCP 연결 취소 OperationCanceledException은 위 6개에 포함됨. 최초 Pumpkin 테스트는 fixture에 피해 텍스트 참조가 없어 실패했고, 해당 UI 싱글턴을 테스트 동안 격리하여 수정·재검증했다.

Warnings: 최종 Console 8개. 테스트 전체 실행에는 의도적인 실패 경로 경고(가방 공간 부족, 잘못된 아이템 ID, 누락/오류/타임아웃 영상 및 컷신)와 임시 NavMesh fixture 경고가 있다. 최초 Pumpkin fixture의 AttackSpeed 파라미터 누락 경고는 임시 AnimatorController에 파라미터를 추가해 제거하고 해당 테스트를 재실행했다. 최종에는 MCP connection/dispose 경고도 있다. 로그 캐시의 과거 항목과 현재 Editor Console은 구분하여 확인했다.

MCP 로그 캐시는 domain reload 시 연결 오류를 반환하지 않아, Main Agent가 UnityEditor.LogEntries의 실제 현재 항목을 읽어 원인을 확인했다. Stack Trace는 com.ivanmurzak.unity.mcp / Microsoft.AspNetCore.SignalR.Client의 handshake canceled / Error starting connection / OperationCanceledException을 가리킨다. 이 프로젝트의 gameplay 변경과 다른 인프라 문제이며 패키지를 임의 변경하거나 Console 항목을 지우지 않았다.

---

## EditMode Tests

Result: PASS

Passed: 70

Failed: 0

Not Run: 지정한 테스트 어셈블리 외의 테스트.

- Unity MCP tests-run: testAssembly=PlayerMovement.EditModeTests.
- 새 GameplayAudit 12개 포함. 기존 인벤토리·보상·보스 전환·영상 복구 등 회귀 테스트 함께 실행.
- 도구의 TotalTests=87은 발견 목록 수이며, 실제 PassedTests=70/FailedTests=0을 실행 결과로 기록한다.
- 첫 실행의 재시도 fixture는 null SetItemAt으로 슬롯을 비우려 했으나 해당 API가 거부하여 실패했다. 실제 RemoveItemAt으로 수정했다. 신규 야큔 대사 표기도 기존 SO의 이름과 일치시켰다. 수정 후 전체 지정 어셈블리 재실행 통과.

검증 범위: 강인 투자·반복 초기화·101포인트 경계, 최대 HP 감소와 부상 HP 유지, 유아독존 장착/교체/반복 해제, 화권 10%/10초/15초, 보상 실패 안내·진행 차단·재시도·중복 지급 방지, 실제 마리아/다일 거절 및 다일 분기, 이미 완료된 소개 퀘스트 이후 수주, 대화하기 버튼/fallback, NPC별 대사 참조, 지급 확정 후 구독자 예외.

---

## PlayMode Tests

Result: PASS

Passed: 57

Failed: 0

Not Run: 지정한 테스트 어셈블리 외의 테스트.

- Unity MCP tests-run: testAssembly=PlayerMovement.PlayModeTests.
- 새 3개: 실제 Physics.OverlapSphere/OverlapBox로 평타 1~4타의 여러 콜라이더 피해·적중·카운터 1회 확인, Pumpkin 근접 공격·반대 방향·죽은 공격자·누락 타겟, NPC UI 재개방 시 버튼 중복 제거.
- 최종 전체 지정 어셈블리: 57 PASS / 0 FAIL.
- 이후 테스트 fixture Animator 파라미터 경고만 보완하고 영향을 받는 GameplayAuditPlayModeTests.PumpkinAttackHitsOnceAndRejectsInvalidTargets를 별도 재실행: 1 PASS / 0 FAIL. 이 실행의 TotalTests=57도 발견 수이며 실제 실행은 1개.

---

## 발견된 문제

- 강인 대량 투자 시 음수 배율 피격 회복 경계: 이번 작업에서 수정·검증함.
- 즉시 종료되는 NPC 대화의 isInteracting 재설정: 이번 작업에서 수정함.
- 엘린과 상인의 npcID가 모두 30003: 기존 데이터 문제. 엘린 대화가 상인 Talk 퀘스트 이벤트로 인식될 가능성이 있음.

---

## 남은 문제

- 엘린/상인의 중복 NPC ID는 이번 감사에서 사용자에게 보고한 10개 수정 범위 밖이다. 퀘스트 ID 참조와 저장 데이터 영향을 별도로 조사하고 식별자를 정리해야 한다. 신규 인사 대사의 Npc_ID도 현재 SO 값을 보존했다.
- 엘린/야큔의 신규 문구는 중립적인 임시 인사이므로 정식 시나리오 문구로 추후 교체할 수 있다.
- 기존 60% 중간보스 Cinemachine/Timeline은 사용자 요청대로 Inspector 참조를 비워두는 이전 작업 상태를 유지한다.
- MCP plugin의 domain reload 재연결 시 handshake 취소 오류는 별도 인프라 문제로 남아 있다. 도구 호출과 테스트 결과 조회는 가능했다.

---

## 미검증 항목

- 실제 플레이어가 마을 전체 퀘스트를 처음부터 끝까지 진행하는 수동 통합 플레이와 UI 화면 배치/마우스 클릭 시각 검증은 NOT VERIFIED. 테스트는 실제 CSV, 버튼 이벤트, Unity UI 활성 계층 및 별도의 runtime fixture를 사용한다.
- Pumpkin의 게임 씬 내 전체 AI 추적·Animator 공격 타이밍을 수동 플레이로 확인하지 않았다. 기존 Attack 이벤트의 실제 피해 판정은 PlayMode에서 검증했다.
- 자동 테스트에 포함되지 않은 스탯·몬스터·퀘스트·대화의 모든 조합은 검증했다고 주장하지 않는다.

---

## 최종 결과

보고한 10개 항목을 모두 반영했다. 추가 회귀 테스트 12개(EditMode)와 3개(PlayMode)를 작성했다. 실제 Unity 컴파일 성공, 지정 어셈블리 EditMode 70개 및 PlayMode 57개 통과와 이후 영향 테스트 1개 재통과를 확인했다. Gameplay/Compile Console Error는 없으며 마지막 domain reload의 MCP 재연결 오류 6개는 별도로 분류했다. 기존 사용자 변경, 공개 API와 직렬화 필드 구조를 보존했으며 commit/push/package 변경은 수행하지 않았다. 요청 관련 tracked 파일의 git diff --check는 Exit 0이다.

## 후속 해결 (2026-10-03)

사용자의 추가 수정 요청으로 위에 남긴 NPC ID 중복과 MCP 재연결 오류를 해결했다. Ellen만 30005로 변경하고 MCP 정리 콜백을 실제 Editor 종료 이벤트로 옮겼다. 후속 검증은 EditMode 72개·PlayMode 57개 PASS, PlayMode 종료 후 MCP Connected 및 Console Error 0이다. 상세 내용은 [추가 발견 사항 수정 보고서](2026-10-03_AdditionalAuditFixes.md)를 참고한다. 위의 남은 문제/Console 오류 기록은 최초 작업 시점의 기록이다.
