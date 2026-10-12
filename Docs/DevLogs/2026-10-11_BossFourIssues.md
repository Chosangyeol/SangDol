# D1 보스 엔딩·60% 카메라·박쥐 전투 수정

## 작업 목적

엔딩 컷씬 미연결, 60% 카메라 컷 누락, 박쥐의 과도한 전투 크기, 일반 공격 패턴 부족을 수정한다.

---

## 분석

- 종료 컴포넌트는 완료 후 5초 대기와 Title 로드만 수행했다. 사용자가 별도 엔딩 영상은 아직 없다고 확인했다.
- 게임용 Jester60Camera Timeline에는 원본의 7.15~13.333초 Virtual Camera (3) 컷이 없었다.
- 동일 박쥐가 연출과 전투에서 모두 루트 스케일 20을 사용했다.
- 일반 패턴은 Clockwork Bat Spell 한 종류였다.

---

## 수정 파일

기존 파일:
- Assets/02. Scripts/Enemy/Dungeon/DungeonCompletionReturnToTitle.cs
- Assets/02. Scripts/Enemy/Boss/Dungeon1/D1_MiddleBoss.cs
- Assets/02. Scripts/Enemy/Boss/Dungeon1/Final/D1_FinalBoss60Presentation.cs
- Assets/03. Prefab/Dungeon/Jester60Intro/Jester60Camera.playable
- Assets/03. Prefab/Dungeon/Jester60Intro/Jester60Introduction.prefab
- Assets/03. Prefab/Enemy/Boss/D1/Middle/ClockworkBatRuntime.controller
- Assets/01. Scene/Circus-Main-Hall.unity
- Assets/Tests/Editor/Boss60CorrectionsTests.cs
- Assets/Tests/Editor/DungeonDepartureTests.cs

신규 파일 및 Unity .meta:
- Assets/03. Prefab/Dungeon/Ending/DungeonEnding.playable
- Assets/03. Prefab/Dungeon/Ending/DungeonEndingCamera.anim
- Assets/03. Prefab/Dungeon/Ending/DungeonEndingOverlay.anim
- Assets/03. Prefab/Dungeon/Ending/DungeonEndingCutscene.prefab
- Assets/Tests/Editor/BossFourIssuesTests.cs
- Assets/Tests/EditMode/BossFourIssuesEditModeTests.cs
- Assets/Tests/PlayMode/BossFourIssuesPlayModeTests.cs

변경 전 백업: Docs/Recovery/2026-10-11_BossFourIssues/
기존 백업과 바이트 비교하여 변경 범위를 확인했다. 씬 YAML은 기존 블록 2개 변경, 신규 블록 2개 추가, 기존 블록 삭제 없음. 기존 사용자 변경은 되돌리지 않았다. exec_command 실행 도우미 실패로 git diff는 NOT AVAILABLE; Unity 내 파일 비교로 범위를 점검했다.

---

## 수정 클래스 / 함수

### DungeonCompletionReturnToTitle
- ReturnToTitle(): 대기 → 선택 영상 → 엔딩 Timeline → Title 로드.
- PlayEnding(), ReleaseEnding(): 자기 영상 세션/Director/목표 UI/BGM 소유권 정리.
- OnDisable(): 취소 시 기존 플레이어 잠금과 기본 던전 복귀 복원.

### D1_MiddleBoss
- CapturePresentationScale(), ApplyCombatScale(), RestorePresentationScale()
- PrepareForPresentation(), BeginCombat(), Awake(), Start()
- WingSwipe(), DoublePulse(), HitArea(), Telegraph(), RemoveTelegraph()

### D1_FinalBoss60Presentation
- FinishEncounter(): 동일 배우의 연출 크기 복원.

---

## 주요 변경 내용

### 엔딩
- 기존 독립 DungeonEnd_ReturnToTitle 컴포넌트를 유지했다. 보스 클래스에 엔딩 기능을 추가하지 않았다.
- endingVideo 슬롯은 비워 두었다. 추후 영상 연결 시 영상 종료/스킵 후 Timeline을 재생한다.
- 7초 기본 엔딩을 제작·연결했다: 보스 방 카메라 이동, DUNGEON CLEAR 문구, 페이드.
- 종료 오브젝트 이동이 카메라 좌표에 전파되지 않도록 DungeonEndingCutscene을 별도 씬 루트에 배치하고 endpoint에서 Director를 참조한다.
- 실제 시간으로 재생하며 마지막 검정 프레임을 Hold로 유지한 채 Title 로딩을 시작한다. 취소/씬 종료 시 원래 Director 설정을 복원한다.
- 컷씬 중 목표 UI 숨김 및 재생 중이던 BGM 일시 정지를 연결했다.

### 60% 카메라
- 게임용 Timeline을 4컷으로 복구했다.
- 0~7.15: Virtual Camera (1)
- 7.15~13.333: Virtual Camera (3)
- 12.0167~18.7667: Virtual Camera
- 17.3035~26: Virtual Camera
- 신규 exposed reference를 인트로 프리팹과 실제 씬 카메라에 연결했다. 영상/음성/master의 26초 종료 동기화는 유지했다.
- 시간 단위 변환 문제로 단정하지 않았다. 확인된 결함은 원본과 실행 복사본의 컷 차이였다.

### 박쥐 전투
- 연출 스케일 20은 보존하고 전투에서 원본의 10%인 스케일 2를 적용한다. 인스펙터 combatScaleMultiplier로 조정 가능하다.
- 활성화되지 않은 부모 아래에서는 Awake 전에 Prepare가 호출될 수 있으므로 최초 스케일을 지연 초기화하고 한 번만 저장한다.
- 반복 전투/취소 후 20으로 복원하며 누적 축소가 없다.
- 추가 패턴: 전방 130도 날개 공격(0.4초 예고), 플레이어의 시전 시작 위치에 고정한 2연속 마법 폭발(0.65초 예고/간격).
- 바닥 공격 범위를 표시하고 동일 대상의 복수 Collider에는 한 번만 피해를 준다. 기존 사망/중단 정리에 공격 표시를 등록했다.
- 기존 주문 애니메이션을 사용하는 빠른 WingSwipe/DoublePulse 상태를 추가했다.
- 기존 주문 테스트는 무작위 패턴 선택에 영향을 받지 않도록 주문만 선택하도록 격리했다.

---

## Public API 변경

Production public API 변경 없음. 기존 public/SerializeField 이름과 클래스 이름을 유지했다.
추가된 설정은 private SerializeField이며, RestorePresentationScale은 internal 메서드다.
신규 엔딩 프리팹/Timeline 참조를 추가했고 기존 참조를 제거하지 않았다.

---

## Multi-Agent 작업

- 사용 여부: 사용
- Main Agent: 요구사항 분석, 파일·자산 수정, 통합, 모든 Unity 테스트 실행 및 씬/Console 확인.
- Review Agent: 읽기 전용 독립 코드/자산 검토.
- Test/QA Agent: 읽기 전용 테스트 전략과 기존 테스트 가정 점검.
- 리뷰 반영:
  - 텔레그래프와 피해 부채꼴이 동일한 확정 방향을 사용하도록 수정.
  - 엔딩 Hold + 시간 종료 조건 + 소유 설정 복원 적용.
  - 종료 오브젝트와 카메라 배치 분리.
  - 초기화 전 스케일 보존 및 반복 복원 확인.
  - 기존 첫 공격 주문 가정과 중복 animationClips 가정을 수정.
- 에이전트가 실행했다고 가정한 테스트는 없음. 아래 PASS는 모두 Main의 Unity MCP 실제 결과다.

---

## Unity 검증

### Editor / Scene / Hierarchy
Result: PASS

Application.dataPath: D:/UnityProjects/SangDol/Assets.
Circus-Main-Hall 실제 로드 검사: 루트 24개, dirty false.
엔딩 Director 연결, 영상 슬롯 null, 엔딩 별도 씬 루트 확인.
게임용 4카메라 컷이 모두 실제 인트로 인스턴스 내부 카메라로 resolve됨.
최종 복원: Assets/01. Scene/Title.unity, dirty false, Play Mode 종료.

### Compile
Result: PASS
최종 isCompiling false, scriptCompilationFailed false.
개발 중 신규 테스트의 internal 메서드 직접 호출로 발생했던 컴파일 오류는 reflection 호출로 수정했다.

### Console
최종 로그 초기화 이후 전체 EditMode 및 실제 씬 확인:
Errors: 0
Exceptions: 0
Warnings: 6
기존 EditMode 경계 조건 테스트의 의도된 로그: 퀘스트 보상 공간 부족 4건, 없는 아이템 ID 99999 1건, 비어 있는 비디오 재생 1건. 새 연출/전투의 런타임 오류로 판단한 로그 없음.

---

## EditMode Tests

Result: PASS
PlayerMovement.EditModeTests 실행: Passed 85, Failed 0, Skipped 0.
도구 TotalTests 102는 검색된 전체 수이며 실제 통과 수는 85.
초기 1건 실패(같은 애니메이션의 상태별 중복 참조를 3개 고유 자산과 혼동)는 고유 자산을 검사하도록 수정 후 재실행 PASS.

---

## PlayMode Tests

Result: PASS (관련 10개 고유 테스트)

- BossFourFixes.BossFourIssuesPlayModeTests.EndingPlaysBeforeDepartureAndCancellationRestoresUI
- BossFourFixes.BossFourIssuesPlayModeTests.ScaleAndCameraRecoverAcrossRetries
- BossFourFixes.BossFourIssuesPlayModeTests.ShortAttacksHitOnceAndCancelCleanly
- DungeonDeparturePlayModeTests.ReturnsToActualTitleAndCanStartFreshGame
- DungeonDeparturePlayModeTests.DisabledOrRemovedEndpointRestoresDefaultDeparture
- DungeonDeparturePlayModeTests.MovingAndReenablingEndpointKeepsOneSubscription
- Jester60IntroductionPlayModeTests.IntroductionFinishesBeforeMiddleBossStarts
- Jester60IntroductionPlayModeTests.SharedActorSurvivesCancellationRetryAndBinderDisable
- Boss60CorrectionsPlayModeTests.ActualBatCastsOncePerTargetAndStopsOnDeath
- Boss60CorrectionsPlayModeTests.MusicResumesOnlyAfterMiddleBossDeathAndReturn

실제 검증:
- 9초 시점 ActiveVirtualCamera가 Virtual Camera (3).
- 동일 배우의 20 → 2 → 20 두 번 반복, NavMesh 배치, 전투 캡슐 반경 약 0.3/높이 약 3.2.
- 짤패턴 피해 횟수, 공격 중단 및 사망 시 후속 피해/표시 제거.
- timeScale 0에서도 엔딩 진행, 이동·회전·스케일 변경된 endpoint로 연출 전후 정리.
- 7초 엔딩 종료 프레임의 활성 카메라/오버레이/검정 alpha 유지 후 실제 Title 단일 씬 복귀.
- Title에서 새 게임 → Main + Map1-Forest 재진입, Pool/Player/UI 재생성.
- 영상 → 기존 60% 컷씬 → 전투 순서 및 중간보스 종료 후 BGM 복귀.

도구 TotalTests 84는 검색된 전체 수이며 전체 PlayMode 84개를 실행한 것으로 기록하지 않는다.
신규 테스트의 초기 실패는 Awake 이전 스케일 초기화 결함과 테스트 플레이어의 부족한 HP였으며 수정 후 3개 재실행 PASS.
독립 endpoint 이동 검증을 추가한 뒤 해당 테스트도 재실행 PASS.

---

## 발견된 문제

- 비활성 부모 아래 배우 준비는 Awake보다 먼저 호출될 수 있음: 지연 스케일 캡처로 수정.
- 추가 패턴 후 기존 주문 첫 선택 테스트가 무작위에 의존할 수 있음: 명시적으로 격리.
- 엔딩 Timeline 자연 종료 시 검정 오버레이가 꺼질 수 있음: 마지막 프레임 Hold 적용.

---

## 남은 문제 / 미검증 항목

- 엔딩 영상: 자산 없음(사용자 확인). 슬롯 비워 둠. 영상 자체의 디코딩/콘텐츠 재생은 NOT AVAILABLE.
- 전체 PlayMode 테스트와 빌드/실기기 검증: NOT RUN. 이번 변경에 관련된 10개 테스트를 실행했다.
- 타이틀부터 모든 퀘스트를 사람처럼 연속 플레이하는 전체 루틴: 이번 작업에서는 NOT RUN. 종료 및 새 게임 재진입은 실제 씬 통합 테스트로 확인했다.
- 전투 크기·공격 빈도의 최종 게임 밸런스는 인스펙터에서 조정 가능하며 장시간 수동 플레이 평가는 별도.

---

## 최종 결과

요청한 4개 항목을 수정·연결했다. 기존 독립 종료 흐름과 60% 영상/전투 전환을 유지했고, 실제 Unity 컴파일·관련 테스트·씬 참조 검증을 통과했다.
