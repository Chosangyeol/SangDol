# 스킬창 목록에서 아덴 및 이동기 제외

## 작업 목적

스킬창에 표시되는 아덴(Z)과 이동기(Space) 항목을 숨긴다.

---

## 분석

`SkillTreeUI.Init()`은 `SkillDataList.asset`의 `skillList` 항목마다 UI 슬롯을 생성한다. 목록 끝의 두 항목이 아덴과 이동기였다.

---

## 수정 파일

- `Assets/02. Scripts/Character/Skill/SkillDataList.asset`
- `Docs/DevLogs/2026-10-24_SkillWindowList.md`

---

## 수정 클래스 / 함수

없음. ScriptableObject 목록 데이터만 수정했다.

---

## 주요 변경 내용

스킬창 목록에서 `Skill_Z.asset`과 `Skill_Space.asset` 참조를 제거했다. 두 에셋과 `CharacterModel`, `C_SkillSystem`의 Z/Space 스킬 참조는 유지된다.

---

## Public API 변경

없음.

---

## Multi-Agent 작업

- 사용 여부: 미사용
- Main Agent 역할: 원인 분석, 에셋 수정 및 Unity 검증
- 주요 리뷰 지적사항: 해당 없음

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

열린 Scene은 `Title`이며, Unity MCP로 수정된 `SkillDataList.asset`의 `skillList`에 기존 전투 스킬 4개만 남은 것을 확인했다. Hierarchy 변경은 수행하지 않았다.

### Compile

Result: NOT VERIFIED

코드 변경이 없어 재컴파일은 발생하지 않았다. AssetDatabase Refresh는 성공했다.

### Console

Errors: 최근 5분 로그에 없음

Warnings: 최근 5분 로그에 없음

Exceptions: 최근 5분 로그에 없음

## EditMode Tests

Result: NOT RUN

에셋 목록의 참조만 제거했고 Unity MCP로 목록을 직접 확인했다.

## PlayMode Tests

Result: NOT RUN

플레이 모드의 스킬창 화면은 열어 확인하지 않았다.

## 발견된 문제

없음.

## 남은 문제

없음.

## 미검증 항목

실제 게임 화면에서 스킬창 표시 결과는 확인하지 않았다.

## 최종 결과

스킬창 데이터 목록에는 전투 스킬 4개만 남았다.
