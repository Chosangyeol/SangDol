# 스킬 툴팁 종료 및 피해량 표기 수정

## 작업 목적

스킬 창을 닫아도 툴팁이 남는 문제와 스킬 피해량이 설명 데이터 및 실제 계산과 다르게 보이는 문제를 수정한다.

---

## 분석

UIManager의 강제 닫기 경로는 `SkillTreeUI.Toggle(true)`에서 패널을 비활성화하지만 툴팁을 숨기지 않았다. `Skill_1` 설명에는 `%%%` 치환 토큰이 남아 있었고, 툴팁은 모든 기술에 첫 피해 배율만 단일 퍼센트로 표시했다. 차징 배율과 Skill_4의 실제 홀딩 틱 피해도 표시값에 반영되지 않았다.

---

## 수정 파일

- `Assets/02. Scripts/UI/Skill/SkillTreeUI.cs`
- `Assets/02. Scripts/UI/Skill/SkillToolTip.cs`
- `Assets/02. Scripts/Character/Skill/Skills/SkillBase/Skill_4.cs`

---

## 수정 내용

- `SkillTreeUI.Toggle()`에서 숨길 때 툴팁을 닫고, 다른 경로로 패널이 비활성화되어도 `OnDisable()`에서 닫는다.
- 툴팁을 닫거나 새로 열 때 이전 표시 코루틴을 취소해 오래된 팝업이 다시 나타나지 않게 했다.
- 피해량 표기를 `공격력의 N%` 형식으로 통일했다. 차징 기술은 데이터의 차징 배율을 반영한 최소·최대 피해 범위를 표시한다.
- Skill_4는 실제 구현과 같은 `공격력의 200% (0.5초마다)`로 표시하도록 틱 배율을 상수로 공유했다.
- 설명의 `%%%` 토큰을 해당 스킬의 배율 퍼센트로 치환한다. 피해 배율 배열이 비어 있으면 피해 줄을 숨긴다.

---

## Public API 변경

없음. `Skill_4`에 내부 상수만 추가했다.

---

## Unity 검증

### Editor / Scene

Result: PASS. Title 씬이 열려 있고 dirty=false. Play Mode=false.

### Compile

Result: PASS. AssetDatabase Refresh 후 `isCompiling=false`, `scriptCompilationFailed=false`.

### Console

최근 Error=0, Exception=0.

### EditMode / PlayMode Tests

Result: NOT RUN. 이번 요청은 테스트 실행을 요청하지 않아 테스트 러너는 실행하지 않았다.

---

## Multi-Agent 작업

- 사용 여부: 미사용.

---

## 남은 문제

실제 플레이 화면에서 포인터를 올린 채 스킬 창을 닫는 수동 재현은 수행하지 않았다. 종료 경로는 `Toggle(true)`와 `OnDisable()` 양쪽에서 숨기도록 처리했다.

---

## 최종 결과

스킬 창 종료 시 툴팁이 남는 경로를 차단하고, 스킬 설명과 실제 배율에 맞춰 툴팁 피해량을 표시한다.
