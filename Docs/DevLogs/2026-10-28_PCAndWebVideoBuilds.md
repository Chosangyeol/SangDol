# PC 및 동영상 포함 WebGL 빌드

## 작업 목적

현재 프로젝트 코드로 Windows PC 실행본과 동영상 파일을 포함한 WebGL 제출본을 각각 생성한다.

---

## 분석

PC는 기존 VideoClip 참조를 사용하는 Windows x64 빌드 설정을 사용했다. WebGL은 프로젝트의 `WebSubmissionBuild.BuildWebGL()` 절차로 URL 메타데이터를 준비하고 `StreamingAssets/Videos` 파일을 패키지에 넣는다.

---

## 생성 파일

- `Build/PC_2026-10-28/`
- `Build/PC_2026-10-28.zip`
- `Build/WebSubmission/`
- `Build/WebSubmission_2026-10-28.zip`
- `Docs/DevLogs/2026-10-28_PCAndWebVideoBuilds.md`

---

## 수정 코드 / Public API

없음. 기존 Build Settings 씬과 WebGL 전처리 절차를 사용했다.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS. 최종 Editor 플랫폼은 WebGL, Play Mode/컴파일 진행 중 아님, `scriptCompilationFailed=false`. Title 씬이 열려 있으며 dirty=false, 루트 7개.

### PC Build

Result: PASS. BuildReport `Succeeded`, Errors=0, Warnings=2, Bytes=1,924,478,439. 출력은 `Build/PC_2026-10-28/SangDol.exe` 및 데이터/런타임 파일. PC ZIP 크기: 952,834,489 bytes.

### WebGL Build

Result: PASS. BuildReport `Succeeded`, Errors=0, Warnings=43, Bytes=885,145,671, Duration=00:41:03.8831706. WebGL 산출물은 `Build/WebSubmission/`에 저장. WebGL ZIP 크기: 885,282,210 bytes.

### Console

최근 Error=0, Exception=0. WebGL BuildReport 경고 43건은 셰이더, 패키지 asmdef, 트리 셰이더 안내 및 `Assets/08. Sound/Jester60Intro.mp4`의 WebGL 내장 VideoClip 미지원 안내를 포함한다. 마지막 안내는 해당 클립이 웹 플레이어에서 재생되지 않을 가능성을 뜻한다.

---

## ZIP 검증

- PC ZIP: 246개 항목의 파일 수와 크기가 출력 디렉터리와 일치하고 루트에 `SangDol.exe`가 있음.
- WebGL ZIP: 9개 파일의 SHA-256이 출력 파일과 일치. 루트 `index.html`, `Build/` 파일 4개, `StreamingAssets/Videos/` MP4 4개를 포함.
- WebGL 출력 MP4 4개의 SHA-256이 `Assets/StreamingAssets/Videos/` 원본과 각각 일치.

---

## 테스트

EditMode/PlayMode 테스트는 빌드 요청 범위에 포함되지 않아 실행하지 않았다. Unity BuildReport에서 양쪽 빌드 모두 성공했고 오류는 0건이다.

---

## Multi-Agent 작업

- 사용 여부: 미사용. 이번 작업은 기존 설정을 이용한 산출물 생성 및 검증이다.

---

## 남은 문제

WebGL BuildReport에 `Jester60Intro.mp4` VideoClip 경고가 남아 있다. Web ZIP은 준비된 MP4 4개를 정확히 포함하지만, 이 별도 클립의 WebGL 재생 여부는 확인하지 않았다.

---

## 최종 결과

Windows x64 빌드와 동영상 4개를 포함한 WebGL 빌드 및 각각의 ZIP을 생성하고 파일 구성을 검증했다.
