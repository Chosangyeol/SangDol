# WebGL 빌드와 영상 배포 확인

## 작업 목적

현재 작업 트리에서 WebGL 빌드를 만들고, 배포 사이트에서 타이틀과 컷신 영상이 나오지 않는 문제의 배포 조건을 확인한다.

---

## 분석

- 프로젝트의 `WebSubmissionBuild.BuildWebGL()`은 `Build/WebSubmission`에 결과를 생성하며, 영상 4개는 `StreamingAssets/Videos`에 별도 파일로 들어간다.
- 사용자가 확인한 사이트에서는 게임이 실행되지만 타이틀과 컷신 영상이 나오지 않고, 개발자 도구에 MP4 요청이나 관련 로그가 없다.
- 사용자는 확인한 `Build` 폴더 안에 `StreamingAssets`가 없다고 보고했다. `StreamingAssets`는 안쪽 `Build`의 하위가 아닌 형제 폴더이므로, 실제 업로드 파일의 루트에 빠졌는지는 확인이 필요하다. MP4 요청 자체가 없어 배포된 게임 코드 버전 차이도 가능하며, 재업로드 전까지 직접 원인을 확정할 수 없다.
- 사이트는 별도 자산 서버의 인증된 임시 실행 주소를 사용한다. 이 환경에서는 실행 중 iframe에 접근해 MP4 응답을 직접 검증하지 못했다.

---

## 수정 파일

- 코드, Scene, Prefab 수정 없음.
- 생성: `Build/WebSubmission/` 및 `Build/WebSubmission_2026-10-25.zip`.
- 빌드 중 Unity가 변경한 무관한 `Assets/Settings/URP-HighFidelity.asset`의 한 값은 작업 전 값으로 복원했다. 기존 다른 변경은 보존했다.

---

## 수정 클래스 / 함수

없음.

---

## 주요 변경 내용

- Unity의 기존 `WebSubmissionBuild.BuildWebGL()`로 WebGL 빌드를 실행했다.
- `index.html`, `Build/`, `StreamingAssets/`가 ZIP 루트에 나란히 오도록 패키징했다.
- ZIP 내부 경로는 `/`를 사용한다.

---

## Public API 변경

없음.

---

## Multi-Agent 작업

- 사용 여부: 미사용. 코드 수정 없이 기존 빌드와 배포 패키지 검증을 수행했다.
- Main Agent 역할: Unity 빌드, 결과와 Console 확인, EditMode 테스트, 파일 및 ZIP 검증.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS. Unity MCP에서 Title 씬 로드, dirty=false, 루트 7개 확인. 빌드 후 Editor가 컴파일 또는 Play Mode 상태가 아님을 확인했다.

### Compile / Build

Result: PASS. `BuildPipeline.BuildPlayer` Succeeded, Errors=0, Warnings=3, Bytes=885149967. 결과 원문은 `Temp/WebSubmissionBuildResult.txt`에 있다.

### Console

Errors: 0. Exceptions: 0. 빌드 경고 3건은 기존 tree shader 안내 1건과 MCP 패키지의 빈 asmdef 안내 2건이다. EditMode 테스트의 의도된 실패 조건 경고도 Console에서 확인됐다.

---

## EditMode Tests

Result: PASS. `PlayerMovement.EditModeTests` 어셈블리: leaf 104 Passed, 0 Failed, 0 Skipped. Unity MCP의 TotalTests=121은 suite 노드를 포함한다.

---

## PlayMode Tests

Result: NOT RUN. 이번 작업에서 게임 코드 변경이 없고, 이전 테스트 실행은 이번 빌드의 브라우저 영상 재생을 입증하지 못한다.

---

## 추가 산출물 검증

- ZIP: `Build/WebSubmission_2026-10-25.zip`, 885286506 bytes.
- ZIP 루트 `index.html`, `Build/` 파일 4개, `StreamingAssets/Videos/` MP4 4개: PASS.
- ZIP 내부 9개 파일의 길이 및 SHA-256을 출력 폴더의 원본과 비교: PASS.
- 출력 MP4 4개의 SHA-256을 `Assets/StreamingAssets/Videos/`와 비교: PASS.
- ZIP SHA-256: `321599DF8005679A799D1FECE6246F097E27FF40D634B9A2D34076F2CF2786D1`.

---

## 발견된 문제

- 처음 빌드 호출은 MCP의 300초 제한으로 응답이 끊겼고, Unity 재시작으로 완료 보고가 남지 않았다.
- 예약해 둔 빌드 호출이 첫 재시도 직후 한 번 더 실행됐다. 마지막 실행은 Succeeded로 완료됐고 이 결과를 최종 산출물로 검증했다.
- 일반 PowerShell `Compress-Archive`는 ZIP 내부에 Windows식 경로 구분자를 넣어, `/` 경로로 새 ZIP을 생성했다.

---

## 남은 문제 / 미검증 항목

- 사이트에 새 ZIP을 업로드하는 작업과 업로드 후 타이틀·컷신 재생: NOT VERIFIED.
- 브라우저 제어 도구가 현재 Edge의 URL을 안전하게 확인하지 못해 UI 조작을 중단했다. 사이트의 임시 실행 주소는 인증이 필요해 서버의 실제 MP4 응답 상태를 직접 읽지 못했다.

---

## 최종 결과

WebGL 빌드와 영상 4개를 포함한 업로드용 ZIP을 생성·검증했다. 사이트 반영 후에는 MP4 요청과 실제 영상 재생을 다시 확인해야 한다.
