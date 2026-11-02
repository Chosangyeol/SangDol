# 포탈 포션 1회 지급 및 레벨업 회복

## 작업 목적

포탈 체험용 체력 포션은 캐릭터당 한 번만 지급하고, 포탈에서 레벨을 올린 뒤 체력을 최대치로 회복한다.

## 분석

포탈 이동은 씬을 바꾸지만 플레이어 인스턴스와 인벤토리는 유지된다. 지급 여부를 플레이어 인스턴스에 약한 참조로 연결해 동일 플레이어가 재입장해도 재지급되지 않게 했다. 기존 인벤토리 보상 추가는 공간을 먼저 확인하므로 포션 10개를 모두 넣을 수 없으면 지급 완료로 처리하지 않는다.

## 수정 파일

- `Assets/02. Scripts/System/SceneMove/PortalTrialSetup.cs`

## 수정 클래스 / 함수

### PortalTrialSetup

- `Apply()` — 지정 레벨 적용 후 최대 HP만큼 회복한다.
- `GrantTrialPotions()` — 플레이어별 첫 지급만 수행하고, 지급 후에도 보유 중인 포션을 1번 슬롯에 연결한다.

## 주요 변경 내용

- 레벨 설정 직후 `player.Heal(maxHp)`를 호출해 최대 체력으로 채운다. Stat 변경 이벤트가 발생해 UI 체력도 갱신된다.
- `ConditionalWeakTable<CharacterModel, PotionGrantMarker>`로 플레이어 인스턴스별 지급 완료를 기록한다. 플레이어가 유지되는 동안 포탈 재사용으로 포션이 반복 지급되지 않는다.
- 인벤토리 공간이 부족하면 10개 보상 추가에 실패하고, 다음 진입 시 다시 시도할 수 있다.

## Public API 변경

없음

## Multi-Agent 작업

- 사용 여부: 미사용

## Unity 검증

### Editor / Scene / Hierarchy

Result: NOT RUN — 코드 로직 변경.

### Compile

Result: PASS — AssetDatabase 갱신 후 최근 Console 컴파일 오류 0건.

### Console

Errors: 0

Warnings: Editor 씬 초기화 중 NavMesh/에셋 관련 경고가 표시됨. 이번 수정 파일과 관련된 경고는 없음.

Exceptions: 0

## EditMode Tests

Result: NOT RUN — 요청되지 않음.

## PlayMode Tests

Result: NOT RUN — 요청되지 않음.

## 빌드

NOT RUN — 빌드는 요청되지 않음.

## 최종 결과

빠른 이동 포탈을 반복해서 타도 포션은 캐릭터당 최초 10개만 지급하고, 포탈에서 레벨이 설정될 때마다 최대 체력으로 회복한다.

## 후속 수정: 체력 포션 회복량

- 대상 에셋: `Assets/Resources/ItemSOs/UseItems/20001_hpPotion_Basic.asset`
- `isPercent`를 켜고 `effectAmount`를 `0.5`로 설정해 최대 체력의 50%를 3초에 걸쳐 회복하도록 했다.
- 아이템 설명도 새 회복량에 맞게 갱신했다.
- Unity Asset 데이터 재조회: `isPercent=true`, `effectAmount=0.5`, `itemDuration=3` 확인.
- 테스트 및 빌드: NOT RUN.
