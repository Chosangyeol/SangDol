# 일반 몬스터 공격력 및 스킬 이펙트/판정 수정

## 작업 목적

던전 일반 광대와 필드 일반 골렘의 공격력을 소폭 낮추고, 수라 이펙트가 버프 종료 후 남는 문제와 R 스킬 퍼펙트존 성공 공격의 피격 판정 누락을 수정한다. 조작키 안내에 회피기 키도 추가한다.

---

## 분석

- MiniClownSO 공격력은 90, GolemSO는 170이었다. 필드 보스 골렘은 ElderGolem 별도 데이터이므로 수정 대상에서 제외했다.
- 수라 버프 종료 시 연속 공격 중이면 오라 회수를 공격 종료 애니메이션 이벤트까지 미뤘다. 사망/애니메이션 전환으로 해당 이벤트가 누락되면 오라가 남을 수 있다.
- Skill_4_Perfect 애니메이션의 0.933초 이벤트는 함수 이름이 비어 있어 타격 판정이 호출되지 않았다. 기존 `PlayerAttackContainer.AnimEvent_ExecuteManagedSkillAttack` 경로를 사용할 수 있다.
- 조작키 안내는 UIManager 프리팹의 ControlKeysText에 저장되어 있다.

## 수정 파일

- Assets/02. Scripts/Enemy/SO/Normal/MiniClownSO.asset
- Assets/02. Scripts/Enemy/SO/Normal/GolemSO.asset
- Assets/02. Scripts/Character/CharacterModel.cs
- Assets/06. Animations/Character/Skill_4_Perfect.anim
- Assets/03. Prefab/Manager/UIManager.prefab

## 수정 클래스 / 함수

### CharacterModel

- IdenDisable()

### Skill_4 / PlayerAttackContainer

- 코드 변경 없음. `Skill_4_Perfect`의 기존 빈 Animation Event를 `AnimEvent_ExecuteManagedSkillAttack`에 연결했다.

## 주요 변경 내용

- MiniClown 공격력: 90 → 80
- 일반 Golem 공격력: 170 → 150
- 수라 버프가 비활성화될 때 애니메이션 종료 여부와 상관없이 오라를 즉시 회수하도록 했다.
- R 스킬 퍼펙트 애니메이션의 0.933초 타이밍에 관리형 스킬 공격 이벤트를 연결하고, 기존 회복 이벤트 3개를 유지했다.
- UIManager 프리팹의 조작키 안내에 `SPACE : 회피기`를 스킬 키 아래에 추가했다.

## Public API 변경

없음. SerializeField 이름이나 에셋 참조 구조는 변경하지 않았다.

## Multi-Agent 작업

- 사용 여부: 사용
- Main Agent 역할: 에셋/코드/애니메이션 수정, Unity 확인
- Review Agent: 수라 이펙트 종료 경로 및 대상 몬스터 에셋 조사
- Test/QA Agent: 몬스터 에셋 식별 및 변경 안전 범위 검토
- 주요 리뷰 지적사항: 사망 중 수라 버프가 해제될 때 피니셔 애니메이션 이벤트에 이펙트 회수가 의존할 수 있음. 필드 보스 골렘은 일반 골렘과 별도 에셋.
- 반영 여부와 이유: 버프 해제 시 즉시 회수하고 일반 GolemSO만 조정했다.

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

UIManager 프리팹 편집 모드에서 ControlKeysText를 수정하고 저장했다. 다른 씬은 저장하지 않았다.

### Compile

Result: PASS

AssetDatabase ForceUpdate 후 Unity 컴파일 로그에 Error는 없었다. 처음 코드 삽입 위치가 잘못되어 잠시 컴파일 오류가 났으나 올바른 함수 본문으로 이동한 후 재컴파일했다.

### Console

Errors: 최종 갱신 이후 0건.

Warnings: 기존 미사용 필드/이벤트 관련 C# 경고가 8건 확인됐다. 이번 수정 대상과 무관하다.

Exceptions: 확인되지 않음.

## EditMode Tests

Result: NOT RUN

## PlayMode Tests

Result: NOT RUN

## 발견된 문제

R 스킬 퍼펙트 애니메이션에 함수 이름이 비어 있는 타격 시점 이벤트가 있었다.

## 남은 문제

실제 게임 플레이에서 피격 거리/애니메이션 동기화와 수라 이펙트 표시 상태는 직접 확인하지 않았다.

## 미검증 항목

- EditMode / PlayMode 테스트
- R 퍼펙트 공격 실제 플레이 판정
- 수라 버프 만료 및 사망 상황에서의 실제 이펙트 소멸
- 몬스터 공격력 인게임 밸런스 체감

## 최종 결과

몬스터 공격력 두 값을 약 10% 낮추고, 수라 오라 회수 시점을 앞당기며, R 퍼펙트 애니메이션의 타격 이벤트를 연결했다. 조작키 안내에도 SPACE 회피기를 추가했다. Unity 갱신/컴파일은 오류 없이 끝났으며 테스트와 빌드는 실행하지 않았다.
