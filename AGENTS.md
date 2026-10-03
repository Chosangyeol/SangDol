# Unity + Codex + MCP 작업 규칙

이 프로젝트에서 Codex는 가능한 경우 Unity MCP를 적극적으로 사용하여 코드 수정뿐 아니라 Unity Editor 상태, 컴파일, Console, 테스트 결과까지 실제로 검증한다. 작업의 규모와 위험도에 따라 Main Agent와 서브에이전트의 역할을 나누고, 결과는 Main Agent가 검토하고 통합한다.

---

## 1. 기본 작업 원칙

모든 작업은 아래 흐름을 기본으로 한다. 작업 규모와 도구 사용 가능 여부에 따라 불필요한 단계는 생략할 수 있다.

1. 요구사항 분석
2. 관련 코드 및 프로젝트 구조 확인
3. 기존 아키텍처와 코딩 스타일 파악
4. 필요한 경우 작업 분해 및 서브에이전트 역할 지정
5. 수정 범위 결정 및 필요한 최소 범위만 수정
6. Unity Asset Refresh 및 재컴파일 상태 확인
7. Console Error / Warning 확인
8. 관련 EditMode 테스트 실행
9. 관련 PlayMode 테스트 실행
10. 실패한 경우 원인 분석
11. 필요한 수정 후 다시 컴파일 및 테스트
12. 최종 결과 보고서와 DevLog 작성

검증에 실패한 경우 바로 작업을 완료했다고 판단하지 않는다. 문제가 수정될 수 있는 범위라면 원인을 분석하고 수정한 뒤 다시 검증한다. 검증 도구를 사용할 수 없는 경우에는 성공 여부를 추측하지 말고 미검증으로 기록한다.

작은 작업에는 multi-agent 사용을 강제하지 않는다. 의미 있는 기능 구현, 버그 수정, 리팩터링, 또는 회귀 위험이 큰 작업은 가능한 경우 Review Agent와 Test/QA Agent의 검토를 활용한다.

---

## 2. 코드 수정 규칙

기존 프로젝트의 구조와 설계를 최대한 유지한다.

다음 작업은 명확한 이유가 없는 한 수행하지 않는다.

- 요청과 관계없는 리팩터링
- 클래스 구조 전체 변경
- 폴더 구조 변경
- 변수명 대량 변경
- namespace 대량 변경
- public API 임의 변경
- 기존 SerializeField 삭제 또는 이름 변경
- Scene 또는 Prefab 대량 수정
- ProjectSettings 임의 변경
- 패키지 설치 또는 삭제

작업 요청에 필요한 범위만 수정한다. 간단한 문제를 해결하기 위해 전체 시스템을 다시 작성하지 않는다. 기본적으로 파일 수정 권한은 Main Agent에 두며, 서브에이전트는 별도 지시가 없는 한 분석과 제안만 수행한다.

---

## 3. 기존 데이터 보호

Unity 프로젝트에서는 기존 Serialize 데이터가 손상되지 않도록 주의한다.

특히 다음 항목을 임의로 변경하지 않는다.

- `[SerializeField]` 필드 이름
- public 필드 이름
- MonoBehaviour 클래스 이름
- ScriptableObject 클래스 이름
- namespace
- Prefab 참조 구조
- Scene 참조 구조

이러한 변경이 반드시 필요한 경우 변경 전후와 이유, 영향을 받는 데이터 및 코드, 필요한 마이그레이션을 최종 보고서에 기록한다. Review Agent를 사용한 경우 Serialize 데이터 위험도 독립적으로 확인하도록 한다.

---

## 4. Public API 변경

다음 항목이 변경되는 경우 Public API 변경으로 간주한다.

- public 메서드
- public 프로퍼티
- public 필드
- public 클래스
- interface
- event
- delegate
- public enum

Public API를 변경한 경우 다음 내용을 보고서에 기록한다.

- 변경 전 API
- 변경 후 API
- 변경 이유
- 영향을 받을 수 있는 코드

가능하면 기존 API 호환성을 유지한다. Review Agent를 사용한 경우 호출부, 상속 관계 및 외부 사용 가능성에 대한 API 파손 여부도 확인하도록 한다.

---

## 5. Unity MCP 사용 원칙

Unity Editor가 실행 중이고 Unity MCP가 사용 가능한 경우 Unity 상태를 추측하지 않는다. 가능한 경우 Unity MCP를 사용하여 실제 상태를 확인한다.

우선적으로 활용할 기능:

- 현재 Unity Editor 상태 확인
- 현재 열린 Scene 확인
- Scene Hierarchy 확인
- GameObject 확인
- Component 확인
- Asset 상태 확인
- Unity Console 로그 확인
- Play Mode 시작 및 종료
- EditMode 테스트 실행
- PlayMode 테스트 실행
- AssetDatabase Refresh
- 컴파일 상태 확인

파일만 분석해서 Scene, Hierarchy, Console, Editor 상태, 테스트 결과 또는 Play Mode 상태를 단정하지 않는다. Unity MCP를 통한 상태 확인과 최종 검증의 실행 책임은 Main Agent에 있다. 서브에이전트가 제공한 분석은 참고 자료로 사용하고, Main Agent가 가능한 검증을 직접 수행한다.

---

## 6. 코드 수정 후 검증 순서

코드를 수정한 후 가능한 경우 다음 순서를 따른다.

### 1. Asset Refresh

Unity가 변경된 파일을 인식하도록 한다.

### 2. Compilation 확인

Unity 컴파일이 완료될 때까지 상태를 확인한다. 컴파일 중에는 테스트를 시작하지 않는다.

### 3. Compile Error 확인

Unity Console에서 컴파일 Error를 확인한다. Error가 존재하면 다음 단계로 넘어가지 않는다. 원인을 분석하고 먼저 수정한다.

### 4. Console 확인

현재 Console의 다음 항목을 확인한다.

- Error
- Exception
- Warning

기존 Warning과 이번 변경으로 발생한 Warning을 가능하면 구분한다.

### 5. EditMode 테스트

수정한 시스템과 관련된 EditMode 테스트가 존재하면 실행한다.

### 6. PlayMode 테스트

수정한 기능이 Runtime 동작과 관련되어 있다면 PlayMode 테스트를 실행한다.

### 7. 실패 처리

테스트 실패 시 다음 순서를 따른다.

1. 실패 메시지 확인
2. Stack Trace 확인
3. 관련 코드 확인
4. 원인 분석
5. 수정 범위와 해결 방법 결정
6. 코드 수정
7. Unity 재컴파일 및 Console 확인
8. 테스트 재실행

Test/QA Agent는 가능한 경우 실패 로그와 Stack Trace를 분석하고 원인 후보를 제시할 수 있다. 코드 수정과 재검증 여부는 Main Agent가 결정한다. 실제로 실행하지 않은 테스트를 PASS로 보고하지 않는다.

---

## 7. 테스트 작성

기능 변경으로 인해 회귀 가능성이 높은 경우 가능한 범위에서 테스트를 추가한다. Test/QA Agent를 사용하면 테스트 전략과 우선순위, 필요한 테스트 대상을 제안하도록 한다.

테스트는 다음을 우선적으로 검증한다.

- 정상 동작
- 경계값
- null 처리
- 상태 전환
- 잘못된 입력
- 반복 호출
- 초기화
- 데이터 변경

테스트를 위해 Production 코드를 불필요하게 크게 변경하지 않는다. Test/QA Agent는 기본적으로 Production 코드를 직접 수정하지 않는다. 테스트 코드 작성이 필요하더라도 Main Agent가 수정 범위와 반영 여부를 결정한다.

---

## 8. EditMode / PlayMode 테스트 기준

### EditMode 테스트가 적합한 경우

- 계산 로직
- 데이터 처리
- Stat 계산
- Inventory 로직
- Skill 데이터
- FSM 상태 전환 로직
- Utility 함수
- 순수 C# 로직

### PlayMode 테스트가 적합한 경우

- MonoBehaviour 동작
- Coroutine
- Physics
- GameObject 활성화
- Scene 동작
- NavMesh
- Animator
- 실제 Unity Lifecycle
- Runtime Component 상호작용

실행 가능한 테스트의 존재와 실행 결과는 Main Agent가 Unity MCP 등 실제 도구로 확인한다. Test/QA Agent의 테스트 제안만으로 테스트가 실행되었거나 통과했다고 간주하지 않는다.

---

## 9. Scene / Prefab 수정

Scene과 Prefab은 코드보다 영향 범위가 클 수 있으므로 최소한으로 수정한다.

다음 작업은 특별한 이유가 없으면 하지 않는다.

- Scene 전체 구조 재배치
- Prefab 대량 수정
- Component 대량 삭제
- Serialize 값 대량 변경
- 기존 참조 제거

Scene 또는 Prefab을 수정한 경우 반드시 보고서에 기록한다. 가능한 경우 Main Agent가 Unity MCP로 수정 후 Scene, Hierarchy, GameObject 및 참조 상태를 확인한다.

---

## 10. Package 관리

사용자가 명시적으로 요청하지 않는 한 새로운 Unity Package를 설치하지 않는다. 다음 작업 역시 임의로 수행하지 않는다.

- Package 삭제
- Package 버전 변경
- `manifest.json` 수정
- scoped registry 추가
- Unity Package Manager 설정 변경

패키지가 반드시 필요한 경우 먼저 기존 프로젝트에서 대체 가능한 기능이 있는지 확인한다.

---

## 11. 테스트 결과 추측 금지

실제로 실행하지 않은 테스트를 성공했다고 기록하지 않는다. 다음과 같이 구분해서 기록한다.

- PASS
- FAIL
- NOT RUN
- NOT AVAILABLE
- NOT VERIFIED

서브에이전트가 보고한 결과도 실제 실행 여부와 근거를 확인한다. 서브에이전트가 직접 실행하지 않았거나 실행 로그를 확인할 수 없는 테스트는 PASS로 기록하지 않는다. Unity MCP의 테스트 실행 결과는 Main Agent가 실제로 실행해 확인한다.

예:

```text
Compile: PASS
Console Error: 0
EditMode Tests: PASS
PlayMode Tests: NOT RUN
Reason: PlayMode test does not exist
```

---

## 12. Unity MCP 사용 불가 시

Unity MCP가 연결되지 않았거나 특정 기능을 지원하지 않는 경우 가능한 범위까지 작업한다. 이 경우 테스트 성공을 추측하지 않는다.

최종 보고서에 반드시 다음 내용을 기록한다.

- 수행한 검증
- 수행하지 못한 검증
- 수행하지 못한 이유

예:

```text
Unity MCP 연결 실패로 인해 다음 항목은 검증하지 못함:

- Unity Console
- PlayMode
- PlayMode Test
```

---

## 13. 변경 범위 확인

작업 완료 전 변경된 파일을 확인한다. 요청과 관련 없는 파일이 수정되어 있으면 작업 내용과 관련이 있는지 확인한다. 관련이 없다면 임의로 추가 변경하지 않는다. 사용자가 작업하기 전부터 존재하던 변경사항은 함부로 되돌리지 않는다.

서브에이전트의 분석이나 제안으로 변경 범위를 확대할 때도 Main Agent가 요구사항과의 관련성 및 필요성을 확인한다.

---

## 14. Git 관련 규칙

사용자가 명시적으로 요청하지 않는 한 다음 작업을 수행하지 않는다.

- commit
- push
- pull
- reset
- rebase
- merge
- branch 삭제
- force push

특히 기존 작업 내용을 임의로 되돌리지 않는다. `git reset --hard`, `git clean -fd`와 같은 파괴적인 명령은 사용하지 않는다.

---

## 15. 작업 결과 보고서

모든 의미 있는 코드 수정 작업이 완료되면 가능한 경우 DevLog를 작성한다.

저장 위치:

```text
Docs/DevLogs/
```

파일 이름:

```text
YYYY-MM-DD_<TaskName>.md
```

예:

```text
Docs/DevLogs/2026-09-29_PlayerDodge.md
```

DevLog에는 multi-agent 사용 여부, 각 Agent의 역할, 주요 리뷰 지적사항 및 반영 여부를 기록한다. 사용하지 않은 경우에도 그 사실을 명시한다.

---

## 16. DevLog 형식

보고서는 다음 형식을 기본으로 사용한다.

```markdown
# 작업 제목

## 작업 목적

이번 작업에서 해결하려는 문제 또는 구현 목표.

---

## 분석

기존 구조 및 문제 원인 요약.

---

## 수정 파일

- Assets/Scripts/...
- Assets/Tests/...

---

## 수정 클래스 / 함수

### ClassName

- MethodName()
- MethodName()

---

## 주요 변경 내용

변경된 로직 설명.

---

## Public API 변경

없음

또는

- 변경 전:
- 변경 후:
- 변경 이유:
- 영향 범위:

---

## Multi-Agent 작업

- 사용 여부: 사용 / 미사용
- Main Agent 역할: 요구사항 분석, 수정 범위 결정, 최종 통합 및 Unity 검증
- 사용한 Agent와 역할: Review Agent — 독립 코드 리뷰; Test/QA Agent — 테스트 전략 및 로그 분석
- 주요 리뷰 지적사항:
- 반영 여부와 이유:

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS / FAIL / NOT RUN / NOT AVAILABLE / NOT VERIFIED

확인한 Editor 상태, 열린 Scene, Hierarchy 등과 결과.

### Compile

Result: PASS / FAIL / NOT VERIFIED

### Console

Errors:

Warnings:

Exceptions:

---

## EditMode Tests

Result:

Passed:

Failed:

Not Run:

---

## PlayMode Tests

Result:

Passed:

Failed:

Not Run:

---

## 발견된 문제

작업 중 발견된 추가 문제.

---

## 남은 문제

아직 해결되지 않은 문제.

---

## 미검증 항목

실제로 검증하지 못한 항목과 이유.

---

## 최종 결과

작업 결과 요약.
```

---

## 17. 간단한 수정 작업

오타 수정, 주석 변경, 로그 문구 변경 등 실행 결과에 영향을 거의 주지 않는 매우 작은 변경은 모든 테스트를 강제로 실행할 필요는 없다. 작은 작업에는 multi-agent 사용도 강제하지 않는다.

하지만 코드 실행 흐름에 영향을 주는 변경이라면 가능한 범위에서 Unity 검증을 수행한다.

---

## 18. Gameplay 시스템 작업

다음과 같은 Gameplay 시스템은 특히 회귀 가능성을 주의한다.

- Player Controller
- Combat
- Skill
- Stat
- Inventory
- Equipment
- FSM
- Behavior Tree
- Enemy AI
- Boss AI
- Quest
- Save / Load
- UI
- Networking

이러한 시스템의 의미 있는 기능 구현, 버그 수정 또는 리팩터링에서는 가능한 경우 Review Agent와 Test/QA Agent를 활용한다. Review Agent는 버그, 회귀, API 파손, Serialize 데이터 위험, 테스트 누락 및 엣지케이스를 점검한다. Test/QA Agent는 테스트 전략과 EditMode/PlayMode 테스트 대상을 제안하고, 실행 결과가 제공된 경우 실패 로그와 Stack Trace를 분석한다.

수정한 시스템과 직접 관련된 테스트부터 실행한다. 전체 테스트 실행이 지나치게 비싼 경우 관련 테스트를 먼저 실행하고, 실행하지 않은 테스트는 명확히 기록한다.

---

## 19. 작업 완료 조건

다음 조건을 가능한 범위에서 만족해야 작업 완료로 판단한다.

- 요청한 기능이 구현되었음
- 컴파일 오류가 없음
- 새 Console Error가 없음
- 관련 테스트가 통과함
- 수정 범위가 요구사항과 일치함
- 불필요한 파일을 수정하지 않았음
- 미검증 항목이 명확하게 기록됨
- DevLog가 작성됨
- 사용한 multi-agent 역할과 주요 리뷰 반영 여부가 DevLog에 기록됨

하나라도 확인하지 못했다면 성공으로 추측하지 않고 미검증 상태를 명시한다. Unity MCP로 확인할 수 있는 Editor, Scene, Hierarchy, Console, Play Mode 및 테스트 상태의 최종 검증은 Main Agent가 실제로 수행하고 결과를 기록한다.

---

## 20. Multi-Agent 작업 규칙

Multi-agent는 작업을 나누어 독립 분석과 검토를 수행하는 수단으로 사용한다. 작은 작업에는 강제하지 않으며, 의미 있는 기능 구현, 버그 수정, 리팩터링 또는 회귀 위험이 큰 작업에서 가능한 경우 활용한다.

### Main Agent

Main Agent는 작업을 끝까지 책임지고 다음을 담당한다.

- 요구사항 분석 및 완료 조건 정리
- 작업 분해와 서브에이전트 역할 지정
- 수정 범위 결정 및 파일 수정 조정
- 서브에이전트 결과의 비판적 검토
- 필요한 제안만 반영하고 최종 변경사항 통합
- Unity MCP로 Editor 상태, Scene, Hierarchy, Console, 컴파일, 테스트 및 Play Mode를 실제 확인
- 실제 검증 결과 정리 및 DevLog 작성

### Review Agent

Review Agent는 변경사항을 독립적으로 검토하고 결과만 보고한다. 파일을 직접 수정하지 않는다. 다음 항목을 점검한다.

- 버그 및 회귀 가능성
- Public API 파손 및 호출부 영향
- Serialize 데이터, 필드 이름, Scene/Prefab 참조 위험
- 테스트 누락
- 경계 조건, 상태 전환, 예외 및 엣지케이스

### Test/QA Agent

Test/QA Agent는 가능한 경우 테스트 전략을 세우고 EditMode/PlayMode 테스트 대상을 제안하며, 제공된 실패 결과의 로그와 Stack Trace를 분석한다. 기본적으로 Production 코드를 직접 수정하지 않는다. 테스트를 실제 실행하지 않았다면 통과했다고 주장하지 않는다.

### Research/Analysis Agent

필요한 경우 큰 구조 분석, 의존성 파악 또는 영향 범위 조사만 수행한다. 직접 파일을 수정하지 않고 근거와 발견사항을 Main Agent에 전달한다.

### 협업 및 통합 원칙

- 서로 독립적인 분석과 리뷰는 병렬로 수행할 수 있다.
- 같은 파일을 여러 Agent가 동시에 수정하지 않는다.
- 파일 수정 권한은 기본적으로 Main Agent 하나에 둔다.
- 서브에이전트의 보고는 검증된 사실과 제안을 구분해 전달한다.
- Main Agent는 결과를 그대로 수용하지 않고 요구사항, 코드 및 실제 검증 결과와 대조한 뒤 필요한 내용만 반영한다.
- Unity MCP의 Editor, Scene, Hierarchy, Console, 컴파일, 테스트 실행 및 Play Mode 검증은 Main Agent가 최종 책임을 지고 실제로 수행한다.
- 서브에이전트가 실제로 실행하지 않은 테스트는 PASS로 보고하지 않는다.
- DevLog에 multi-agent 사용 여부, 각 Agent 역할, 주요 리뷰 지적사항 및 실제 반영 여부를 기록한다.

---

# 기본 작업 루프

```text
요구사항 분석
      ↓
관련 코드 및 프로젝트 구조 확인
      ↓
작업 분해 및 위험도 판단
      ↓
필요한 경우 독립 분석 / 리뷰 병렬 진행
      ↓
Main Agent가 결과 검토 및 수정 범위 결정
      ↓
최소 범위 수정
      ↓
Unity Refresh
      ↓
Compilation 확인
      ↓
Console 확인
      ↓
관련 EditMode / PlayMode Test
      ↓
실패?
 ┌────┴────┐
Yes        No
 ↓          ↓
원인 분석   결과 확인
 ↓          ↓
수정 및 재검증
 └────┬─────┘
      ↓
Main Agent가 최종 결과 통합
      ↓
DevLog 작성
```

Codex는 가능한 경우 이 흐름을 기본 개발 사이클로 사용한다. Unity MCP 또는 특정 테스트 도구를 사용할 수 없는 경우 수행하지 못한 단계와 이유를 최종 결과 및 DevLog에 기록한다.
