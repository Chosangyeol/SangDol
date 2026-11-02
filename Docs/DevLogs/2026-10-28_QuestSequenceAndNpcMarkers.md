# 퀘스트 순차 진행 및 NPC 표시

## 작업 목적

수락 가능한 NPC 위에 물음표를 표시하고, 해당 NPC에게 완료할 퀘스트가 있을 때 느낌표를 표시한다. 퀘스트가 선행 조건을 완료한 순서대로 수락되도록 한다.

---

## 분석

`NpcDialogManager`는 NPC 버튼을 구성할 때만 `NpcQuestData.requiredQuestID`를 확인했고, `QuestManager.AcceptQuest()`에는 선행 퀘스트 검사가 없었다. 다일의 늑대 사냥 퀘스트(Q_30001_003)는 NPC 데이터의 선행 퀘스트가 비어 있어 처음부터 시작 메뉴에 표시됐다.

NPC 이름표는 `NpcBase`가 카메라 방향으로 회전시키고 있으므로, 이름표에 색상 구분된 `?` / `!`를 표시하면 NPC 위의 기존 이름표 위치와 빌보드 동작을 재사용할 수 있다.

## 수정 파일

- Assets/02. Scripts/Interact/Npc/NpcBase.cs
- Assets/02. Scripts/Interact/Npc/SO/Dail.asset
- Assets/02. Scripts/System/QuestManager.cs
- Assets/02. Scripts/UI/Npc/NpcDialogManager.cs
- Assets/Resources/NpcQuestDataBase.csv

## 수정 클래스 / 함수

### NpcBase

- OnEnable()
- OnDisable()
- SubscribeQuestUpdates()
- UpdateQuestMarker()

### QuestManager

- LoadQuestCsv()
- AcceptQuest()
- CanAcceptQuest()

### NpcDialogManager

- OpenNpcUI()

## 주요 변경 내용

- 이름표 위에 수락 가능 `?`(노랑), 완료 가능 `!`(주황)을 표시한다. 완료 표시를 우선하며, 둘 다 해당하지 않으면 NPC 이름만 표시한다.
- 퀘스트 상태 변경 이벤트를 구독해 표시를 즉시 갱신하고, 비활성화 시 구독을 해제한다.
- CSV에 선행 퀘스트 ID 열을 추가하고 Q_30001_002에는 Q_30001_001, Q_30001_003에는 Q_30001_002를 설정했다.
- `QuestManager.CanAcceptQuest()`에서 미등록 퀘스트, 이미 시작한 퀘스트, 선행 퀘스트 미완료를 거른다. NPC 메뉴 구성과 실제 `AcceptQuest()` 양쪽에서 같은 조건을 사용한다.
- 다일 NPC 데이터의 requiredQuestID도 Q_30001_002로 맞췄다.

## Public API 변경

없음. `CanAcceptQuest()` 및 QuestData의 선행 퀘스트 필드는 internal이다. 기존 SerializeField/API 이름은 변경하지 않았다.

## Multi-Agent 작업

- 사용 여부: 사용
- Main Agent 역할: 코드 수정, 선행 조건 설계, 통합 및 Unity 검증
- Review Agent: 수락 조건 우회 원인, NPC 데이터 및 아이콘 갱신 구조 분석
- Test/QA Agent: 기존 퀘스트 체인 테스트 및 검증 누락 분석
- 주요 리뷰 지적사항: 수락 메뉴에서만 조건을 확인하면 대사 액션이 직접 수락을 호출해 우회할 수 있음. 다일의 requiredQuestID가 비어 있음.
- 반영 여부와 이유: CSV와 QuestManager에 공통 수락 검증을 추가하고 Dail 에셋 조건을 채웠다.

## Unity 검증

### Editor / Scene / Hierarchy

Result: NOT RUN

스크립트와 에셋 파일을 수정했으며 씬/프리팹은 변경하지 않았다. 현재 열린 Main 씬은 작업 전부터 dirty 상태라 저장하지 않았다.

### Compile

Result: NOT VERIFIED

AssetDatabase ForceUpdate는 성공했다. 별도의 Unity 컴파일 완료 상태는 확인할 수 없었다.

### Console

Errors: Asset refresh 후 최근 5분 구간 0건.

Warnings: Asset refresh 후 최근 5분 구간 0건.

Exceptions: 확인되지 않음.

## EditMode Tests

Result: NOT RUN

## PlayMode Tests

Result: NOT RUN

기존 `GameFlowPlayModeTests`에 Q_30001_001 → Q_30001_002 → Q_30001_003 순차 완료 흐름이 있으나 실행하지 않았다. 잠긴 Q3의 직접 수락 차단 및 NPC 머리표시는 해당 테스트에서 별도로 확인하지 않는다.

## 발견된 문제

없음.

## 남은 문제

NPC 이름표 위 마커의 실제 화면 배치와 폰트 가독성은 플레이 모드에서 확인하지 않았다.

## 미검증 항목

- Unity 컴파일 완료 상태
- EditMode / PlayMode 테스트
- NPC 마커의 실제 카메라 거리별 가독성

## 최종 결과

NPC 머리 위 퀘스트 상태 표시와 Q1 → Q2 → Q3 선행 조건을 반영했다. Unity AssetDatabase 갱신은 완료했고 최근 Console Error/Warning은 없었다. 빌드 및 테스트는 실행하지 않았다.
