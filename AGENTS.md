# Unity + Codex + MCP 작업 규칙

이 프로젝트에서 Codex는 가능한 경우 Unity MCP를 적극적으로 사용하여 코드 수정뿐 아니라 Unity Editor 상태, 컴파일, Console, 테스트 결과까지 실제로 검증한다.

---

## 1. 기본 작업 원칙

모든 작업은 아래 순서를 기본 흐름으로 따른다.

1. 요구사항 분석
2. 관련 코드 및 프로젝트 구조 확인
3. 기존 아키텍처와 코딩 스타일 파악
4. 필요한 최소 범위만 수정
5. Unity 재컴파일 및 상태 확인
6. Console Error / Warning 확인
7. 관련 EditMode 테스트 실행
8. 관련 PlayMode 테스트 실행
9. 실패한 경우 원인 분석
10. 코드 수정
11. 다시 컴파일 및 테스트
12. 최종 결과 보고서 작성

검증에 실패한 경우 바로 작업을 완료했다고 판단하지 않는다.

문제가 수정될 수 있는 범위라면 원인을 분석하고 수정한 뒤 다시 검증한다.

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

작업 요청에 필요한 범위만 수정한다.

간단한 문제를 해결하기 위해 전체 시스템을 다시 작성하지 않는다.

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

이러한 변경이 반드시 필요한 경우 최종 보고서에 반드시 기록한다.

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

가능하면 기존 API 호환성을 유지한다.

---

## 5. Unity MCP 사용 원칙

Unity Editor가 실행 중이고 Unity MCP가 사용 가능한 경우 Unity 상태를 추측하지 않는다.

가능한 경우 Unity MCP를 사용하여 실제 상태를 확인한다.

우선적으로 활용할 기능:

- 현재 Unity Editor 상태 확인
- 현재 열린 Scene 확인
- Scene Hierarchy 확인
- GameObject 확인
- Component 확인
- Asset 상태 확인
- Unity Console 로그 확인
- Play Mode 시작
- Play Mode 종료
- EditMode 테스트 실행
- PlayMode 테스트 실행
- AssetDatabase Refresh
- 컴파일 상태 확인

파일만 분석해서 Unity 상태를 추측하지 않는다.

---

## 6. 코드 수정 후 검증 순서

코드를 수정한 후 가능한 경우 다음 순서를 따른다.

### 1. Asset Refresh

Unity가 변경된 파일을 인식하도록 한다.

### 2. Compilation 확인

Unity 컴파일이 완료될 때까지 상태를 확인한다.

컴파일 중에는 테스트를 시작하지 않는다.

### 3. Compile Error 확인

Unity Console에서 컴파일 Error를 확인한다.

Error가 존재하면 다음 단계로 넘어가지 않는다.

원인을 분석하고 먼저 수정한다.

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
5. 코드 수정
6. Unity 재컴파일
7. Console 확인
8. 테스트 재실행

테스트가 통과할 때까지 합리적인 범위 내에서 반복한다.

---

## 7. 테스트 작성

기능 변경으로 인해 회귀 가능성이 높은 경우 가능한 범위에서 테스트를 추가한다.

테스트는 다음을 우선적으로 검증한다.

- 정상 동작
- 경계값
- null 처리
- 상태 전환
- 잘못된 입력
- 반복 호출
- 초기화
- 데이터 변경

테스트를 위해 Production 코드를 불필요하게 크게 변경하지 않는다.

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

---

## 9. Scene / Prefab 수정

Scene과 Prefab은 코드보다 영향 범위가 클 수 있으므로 최소한으로 수정한다.

다음 작업은 특별한 이유가 없으면 하지 않는다.

- Scene 전체 구조 재배치
- Prefab 대량 수정
- Component 대량 삭제
- Serialize 값 대량 변경
- 기존 참조 제거

Scene 또는 Prefab을 수정한 경우 반드시 보고서에 기록한다.

---

## 10. Package 관리

사용자가 명시적으로 요청하지 않는 한 새로운 Unity Package를 설치하지 않는다.

다음 작업 역시 임의로 수행하지 않는다.

- Package 삭제
- Package 버전 변경
- manifest.json 수정
- scoped registry 추가
- Unity Package Manager 설정 변경

패키지가 반드시 필요한 경우 먼저 기존 프로젝트에서 대체 가능한 기능이 있는지 확인한다.

---

## 11. 테스트 결과 추측 금지

실제로 실행하지 않은 테스트를 성공했다고 기록하지 않는다.

다음과 같이 구분해서 기록한다.

- PASS
- FAIL
- NOT RUN
- NOT AVAILABLE
- NOT VERIFIED

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

Unity MCP가 연결되지 않았거나 특정 기능을 지원하지 않는 경우 가능한 범위까지 작업한다.

이 경우 테스트 성공을 추측하지 않는다.

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

작업 완료 전 변경된 파일을 확인한다.

요청과 관련 없는 파일이 수정되어 있으면 작업 내용과 관련이 있는지 확인한다.

관련이 없다면 임의로 추가 변경하지 않는다.

사용자가 작업하기 전부터 존재하던 변경사항은 함부로 되돌리지 않는다.

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

특히 기존 작업 내용을 임의로 되돌리지 않는다.

`git reset --hard`, `git clean -fd`와 같은 파괴적인 명령은 사용하지 않는다.

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

---

## Unity 검증

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

오타 수정, 주석 변경, 로그 문구 변경 등 실행 결과에 영향을 거의 주지 않는 매우 작은 변경은 모든 테스트를 강제로 실행할 필요는 없다.

하지만 코드 실행 흐름에 영향을 주는 변경이라면 가능한 범위에서 반드시 Unity 검증을 수행한다.

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

가능하면 수정한 시스템과 직접 관련된 테스트부터 실행한다.

전체 테스트 실행이 지나치게 비싼 경우 관련 테스트를 먼저 실행한다.

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

하나라도 확인하지 못했다면 성공으로 추측하지 않고 미검증 상태를 명시한다.

---

# 기본 작업 루프

```text
요구사항 분석
      ↓
관련 코드 확인
      ↓
기존 구조 분석
      ↓
최소 범위 수정
      ↓
Unity Refresh
      ↓
Compilation 확인
      ↓
Console 확인
      ↓
EditMode Test
      ↓
PlayMode Test
      ↓
실패?
 ┌────┴────┐
Yes        No
 ↓          ↓
원인 분석   결과 확인
 ↓          ↓
수정       DevLog 작성
 ↓
재컴파일
 ↓
재테스트
```

Codex는 가능한 경우 이 흐름을 기본 개발 사이클로 사용한다.