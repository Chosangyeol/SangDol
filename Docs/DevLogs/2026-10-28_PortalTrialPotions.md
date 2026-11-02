# 포탈 체험용 회복 포션 지급

## 작업 목적

마을 포탈을 통해 던전 또는 필드 보스 체험 지역으로 이동할 때 체력 포션 10개를 지급하고, 1번 소모품 슬롯에 등록한다.

---

## 분석

던전 입장과 보스 바로가기 포탈은 `PortalTrialSetup.Apply`에서 공통으로 레벨과 스킬 구성을 준비한다. 체력 포션 데이터는 Resources의 `20001_hpPotion_Basic`이며, 소모품 단축 슬롯은 인벤토리 인덱스를 참조한다. 슬롯 UI는 이 인덱스를 별도로 보관하므로 UI와 인벤토리 매핑을 함께 갱신해야 한다.

---

## 수정 파일

- `Assets/02. Scripts/System/SceneMove/PortalTrialSetup.cs`
- `Assets/02. Scripts/UI/MainUI.cs`
- `Assets/02. Scripts/UI/UseItemSlot.cs`

---

## 주요 변경 내용

- 포탈 체험 설정 시 체력 포션 10개를 인벤토리에 추가한다. 기존 포션과는 기존 스택 규칙에 따라 합쳐진다.
- 포션이 들어간 인벤토리 칸을 1번 소모품 슬롯에 연결하고 UI를 갱신한다.
- 소모품 슬롯 초기화 시 저장된 인벤토리 인덱스를 읽도록 해, 포탈을 다시 탈 때도 슬롯 표시와 실제 키 동작이 맞도록 했다.

## Public API 변경

없음. UI 연결 메서드는 internal이다.

## Multi-Agent 작업

- 사용 여부: 미사용
- 단일 포탈 보상/UI 연결 변경으로 Main Agent가 직접 수정했다.

## Unity 검증

### Editor / Scene / Hierarchy

Result: NOT RUN — 이 변경은 코드와 인벤토리/소모품 UI 흐름만 수정했다.

### Compile

Result: PASS — Unity AssetDatabase Refresh 후 Console Error 0건.

### Console

Errors: 0

Warnings: 기존 코드의 미사용 이벤트/필드 경고가 표시됨. 이번 수정 파일 관련 경고는 없음.

Exceptions: 0

## EditMode Tests

Result: NOT RUN — 요청되지 않음.

## PlayMode Tests

Result: NOT RUN — 요청되지 않음.

## 빌드

NOT RUN — 별도 요청이 없어 빌드하지 않음.

## 최종 결과

던전/필드 보스 포탈 체험 진입 시 체력 포션 10개를 받고, 1번 슬롯에서 사용할 수 있도록 연결했다.
