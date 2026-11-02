# 포탈 입장 10레벨 및 QWER 기본 스킬 구성

## 작업 목적

제스터 던전 입장과 골렘 보스 포탈 이용 시 플레이어를 10레벨로 올리고, 스킬 1~4를 각각 1레벨로 만든 뒤 Q/W/E/R에 자동 장착한다.

## 분석

- 골렘 단축 포탈은 `BossDungeonShortcutPortal`에서 설정 레벨까지 `RaiseToLevel`을 호출했지만, 씬의 설정값은 1이었다.
- 제스터 포탈은 `DungeonPortalObject`에서 입장 UI를 열고, 실제 씬 전환은 `DungentEnterUI.EnterDungeon()`에서 시작한다.
- 스킬 트리 초기화 시 SkillDataList 순서대로 스킬 1~4 인스턴스를 만들며, 스킬 데이터의 요구 레벨은 5다.

## 수정 파일

- `Assets/02. Scripts/System/SceneMove/BossDungeonShortcutPortal.cs`
- `Assets/02. Scripts/System/SceneMove/PortalTrialSetup.cs`
- `Assets/02. Scripts/UI/DungeonEnter/DungentEnterUI.cs`
- `Assets/02. Scripts/UI/Skill/SkillTreeUI.cs`
- `Assets/01. Scene/Map1-Forest.unity`
- `Docs/DevLogs/2026-10-28_PortalTrialLoadout.md`

## 주요 변경 내용

- 공통 `PortalTrialSetup`이 플레이어 레벨을 설정값(최소 1)까지 정상 레벨 업 경로로 올리고, 스킬 트리에 QWER 기본 구성을 적용한다.
- 골렘 포탈 기본 및 씬 설정 레벨을 10으로 바꿨다.
- 던전 입장 확인 버튼의 레벨 설정값 기본값을 10으로 추가했다. 입장 UI만 여는 단계가 아니라 실제 `EnterDungeon()` 시점에 적용한다.
- SkillTreeUI는 목록의 첫 네 스킬(스킬 1~4)이 0레벨이면 레벨 요구 조건과 스킬 포인트를 확인해 1레벨을 올리고, Q/W/E/R 슬롯에 각각 등록한다. 이미 레벨이 있으면 추가 포인트를 쓰지 않고 장착한다.
- 실제 레벨 업을 사용하므로 레벨에 따른 스탯 포인트와 스킬 포인트 지급이 유지된다.

## Public API 변경

없음. 새 SkillTreeUI 메서드는 `internal`이고 포탈 레벨 값은 private 직렬화 필드다.

## Multi-Agent 작업

- 사용 여부: 미사용

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

- Unity MCP에서 `Title` 씬이 열려 있고 저장된 상태임을 확인했다.
- 골렘 포탈 씬 직렬화 설정값이 10임을 확인했다.

### Compile

Result: PASS

- Unity AssetDatabase ForceUpdate 성공.
- Refresh 후 Console compile Error 0건, Exception 0건.

### Console

- Error: 0건.
- Warning: 7건. 전부 이번 수정 범위 밖의 기존 미사용 이벤트/필드 경고(`CharacterModel`, 보스 스크립트, `SkillTreeSlot`, `BuffSlot`)다.
- Exception: 0건.

## EditMode Tests

Result: NOT RUN

## PlayMode Tests

Result: NOT RUN

## 미검증 항목

- 실제 포탈 이용 후 플레이어 레벨, 포인트, QWER 장착 상태를 플레이 모드에서 확인하지 않았다.
- 자동 스킬 레벨 업에 필요한 포인트가 남아 있지 않은 특수한 저장 상태에서는 자동 구성이 중단되고 경고가 출력된다.

## 최종 결과

제스터 던전 실제 입장과 골렘 보스 포탈 이동 경로 모두 10레벨과 스킬 1~4의 QWER 1레벨 기본 구성을 적용하도록 연결했다. 빌드와 테스트는 실행하지 않았다.
