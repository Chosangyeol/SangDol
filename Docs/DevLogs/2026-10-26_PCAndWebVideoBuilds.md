# PC 및 동영상 포함 WebGL 빌드

## 작업 목적

현재 작업 트리에서 Windows PC용 빌드와 동영상 파일을 포함한 WebGL 배포용 빌드를 각각 생성한다.

---

## 분석

- 활성 Build Settings 씬 12개를 두 빌드에 사용했다.
- PC에서는 기존 VideoClip 참조를 사용하고, WebGL에서는 기존 `WebSubmissionBuild`가 `StreamingAssets/Videos`의 MP4 URL 메타데이터를 준비한다.
- 웹 배포 ZIP에는 `index.html`, `Build/`, `StreamingAssets/`가 ZIP 루트에 나란히 있어야 한다.

---

## 수정 파일

- 생성: `Build/PC_2026-10-26/`, `Build/PC_2026-10-26.zip`
- 갱신: `Build/WebSubmission/`
- 생성: `Build/WebSubmission_2026-10-26.zip`
- 생성: `Docs/DevLogs/2026-10-26_PCAndWebVideoBuilds.md`
- Production 코드, Scene, Prefab의 추가 수정 없음. 작업 전부터 존재한 변경 사항은 보존했다.

---

## 수정 클래스 / 함수

없음.

---

## 주요 변경 내용

- Windows 64비트 빌드를 `SangDol.exe`와 필요한 Data/Unity 런타임 파일이 있는 별도 폴더에 생성했다.
- 기존 `WebSubmissionBuild.BuildWebGL()`로 WebGL 빌드를 갱신하고 MP4 4개를 포함한 ZIP을 만들었다.
- 두 ZIP 모두 내부 경로를 `/`로 기록했다.

---

## Public API 변경

없음.

---

## Multi-Agent 작업

- 사용 여부: 사용.
- Main Agent: 빌드 실행, 결과 확인, 패키징, Unity 검증, DevLog 작성.
- Review Agent: 기존 빌드 설정과 웹 영상 배포 조건을 읽기 전용으로 확인. 파일 수정 및 빌드 실행 없음.
- 주요 지적사항: WebGL ZIP 루트에 `StreamingAssets/Videos`를 포함하고 기존 작업 트리 변경을 보호할 것. 모두 반영했다.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS. 최종 활성 대상 WebGL, Play Mode/컴파일/갱신 중 아님, `scriptCompilationFailed=false`. Title 씬이 열려 있고 dirty=false, 루트 7개다.

### Compile / Build

Result: PASS.

- PC BuildReport: Succeeded, Errors=0, Warnings=2, Bytes=1,923,141,815.
- WebGL BuildReport: Succeeded, Errors=0, Warnings=50, Bytes=885,129,454, Duration=00:35:40.7939329.
- WebGL 경고는 기존 코드·셰이더, 패키지 asmdef 및 WebGL VideoClip 지원 안내를 포함한다. 빌드 오류는 없었다.

### Console

Errors: 0. Exceptions: 0. MCP Console의 최근 45분 조회 결과.

Warnings: BuildReport 기준 PC 2건, WebGL 50건.

---

## EditMode Tests

Result: PASS. Unity MCP로 `PlayerMovement.EditModeTests` 실행: Passed 104, Failed 0, Skipped 0. `TotalTests=121`은 suite 노드를 포함한다.

---

## PlayMode Tests

Result: NOT RUN. 이번 작업은 코드 변경 없이 빌드와 패키징만 수행했다.

---

## 산출물 검증

- PC ZIP: 997,806,212 bytes. 실행 파일과 Data 폴더 포함 246개 파일의 경로와 크기가 출력 폴더와 일치한다.
- WebGL ZIP: 885,265,993 bytes. `index.html`, `Build/` 파일 4개, `StreamingAssets/Videos/` MP4 4개 포함. 9개 파일의 SHA-256이 출력 폴더와 일치한다.
- 웹 출력 MP4 4개의 SHA-256이 `Assets/StreamingAssets/Videos/`의 원본과 일치한다.

---

## 발견된 문제

- WebGL 빌드가 Unity MCP 단일 호출 제한인 300초를 초과했지만, Unity의 Brotli 압축은 계속 진행됐고 최종 BuildReport에 성공이 기록됐다.
- 처음 생성한 PC ZIP의 내부 경로가 Windows식 `\`여서 `/` 경로로 다시 생성하고 검증했다.

---

## 남은 문제 / 미검증 항목

- 실제 PC 실행 플레이와 HTTP(S) 배포 후 브라우저에서 영상 표시·소리 재생은 NOT VERIFIED. 이번 검증은 빌드, 영상 파일 포함 여부, 원본 일치성까지다.
- WebGL 빌드 경고 50건은 개별 수정 대상 여부를 이번 빌드 작업에서 분석하지 않았다.

---

## 최종 결과

PC 실행용 빌드와 동영상 4개가 포함된 WebGL 업로드용 ZIP을 생성하고 파일 구성을 검증했다.
