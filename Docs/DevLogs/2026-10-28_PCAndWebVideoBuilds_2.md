# PC 및 동영상 포함 WebGL 빌드

## 작업 목적

요청에 따라 동일한 빌드 설정으로 Windows PC 빌드와 동영상이 포함된 WebGL 빌드를 생성하고 별도 ZIP으로 패키징했다.

---

## 분석

이전 빌드 설정을 확인했다. PC는 Windows x64이며, WebGL은 `WebSubmissionBuild` 전처리 및 Submission 템플릿을 사용한다. 활성화된 빌드 씬은 12개다. 기존 날짜별 산출물을 덮어쓰지 않도록 `_2` 접미사를 사용했다.

---

## 수정 파일

- `Build/PC_2026-10-28_2/` — PC 실행 파일 및 런타임 파일
- `Build/PC_2026-10-28_2.zip`
- `Build/WebSubmission_2026-10-28_2/`
- `Build/WebSubmission_2026-10-28_2.zip`
- `Docs/DevLogs/2026-10-28_PCAndWebVideoBuilds_2.md`

프로젝트 코드 및 Scene/Prefab은 이번 빌드 요청을 위해 직접 수정하지 않았다.

---

## 수정 클래스 / 함수

없음. 기존 빌드 설정과 WebGL 전처리 절차를 사용했다.

---

## 주요 변경 내용

- PC: Windows x64, 활성화된 12개 씬, `BuildOptions.None`.
- WebGL: 활성화된 12개 씬, Submission 템플릿, 960x600, 동영상 4개 포함, `BuildOptions.None`.
- 각 빌드 디렉터리를 별도 ZIP으로 패키징했다.
- Web ZIP 내 MP4 4개의 SHA-256이 빌드 폴더의 파일과 일치함을 확인했다.

---

## Public API 변경

없음

---

## Multi-Agent 작업

- 사용 여부: 미사용
- Main Agent 역할: 빌드 설정 확인, 두 빌드 실행, ZIP 검증, 최종 기록
- 주요 리뷰 지적사항: 해당 없음
- 반영 여부와 이유: 빌드 산출 작업이며 별도의 코드 리뷰 작업은 수행하지 않았다.

---

## Unity 검증

### Editor / Scene / Hierarchy

Result: PASS

WebGL 타깃 빌드 결과를 확인했다. 열린 Scene은 `Title`이며 Dirty 상태가 아니고, 빌드 대상 씬은 12개였다.

### Compile

Result: PASS

PC 및 WebGL BuildReport가 모두 `Succeeded`를 반환했다.

### Console

Errors: 0 (Unity Console Error 조회 결과 없음)

Warnings: PC BuildReport 22, WebGL BuildReport 3

Exceptions: 확인된 빌드 예외 없음

---

## EditMode Tests

Result: NOT RUN

Passed: 0

Failed: 0

Not Run: 전체

사유: 사용자는 빌드를 요청했고 별도 테스트는 요청하지 않았다.

---

## PlayMode Tests

Result: NOT RUN

Passed: 0

Failed: 0

Not Run: 전체

사유: 사용자는 빌드를 요청했고 별도 테스트는 요청하지 않았다.

---

## 발견된 문제

없음

---

## 남은 문제

없음

---

## 미검증 항목

- 실행 파일을 실제로 실행하는 런타임 smoke test는 수행하지 않았다.
- Git 변경 목록은 저장소 소유권 안전 검사(`dubious ownership`) 때문에 확인하지 못했다. 작업 중 기존 파일을 되돌리거나 변경하지 않았다.

---

## 최종 결과

### PC

- BuildReport: `Succeeded`, Errors 0, Warnings 22, 12 scenes, 1,923,213,823 bytes, 58.6초
- ZIP: `Build/PC_2026-10-28_2.zip`, 246개 파일, 엔트리 총 크기 1,923,252,561 bytes
- ZIP 내부 `SangDol.exe` 확인: PASS

### WebGL

- BuildReport: `Succeeded`, Errors 0, Warnings 3, 12 scenes, 885,135,097 bytes
- ZIP: `Build/WebSubmission_2026-10-28_2.zip`, 9개 파일, 엔트리 총 크기 885,135,097 bytes
- `index.html`, `.wasm.br`, `.data.br`, 동영상 MP4 4개 확인: PASS
- ZIP 안 MP4 4개의 SHA-256 검증: PASS

빌드 산출물은 지정된 별도 경로에 저장했다.
