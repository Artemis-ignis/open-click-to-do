# Open Click to Do

**모든 Windows PC에서 Win+Q로 화면 OCR, 복사, 빠른 번역을 사용하세요.**

![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-2563eb)
![.NET 8](https://img.shields.io/badge/.NET-8-512bd4)
![AutoHotkey v2](https://img.shields.io/badge/AutoHotkey-v2-059669)
![License MIT](https://img.shields.io/badge/License-MIT-111827)

> Microsoft Click to Do를 강제로 켜는 도구가 아니라, 비슷한 사용 경험을 제공하는 오픈소스 대체 앱입니다.

![Open Click to Do 오버레이 자리표시자](docs/screenshots/overlay-demo-placeholder.svg)

Open Click to Do는 일반 Windows PC에서도 `Win+Q`로 화면 속 텍스트를 OCR로 인식하고, 복사하고, 빠르게 번역하거나 검색할 수 있게 해 주는 Windows 전용 유틸리티입니다. Copilot+ PC 하드웨어가 필요하지 않습니다.

일반 윈도우에서 Click to Do 쓰는 법, Win+Q OCR, 화면 텍스트 복사, 화면 번역, PowerToys Text Extractor 대체 도구를 찾고 있었다면 이 프로젝트가 그 용도입니다.

## 빠른 시작

1. 저장소를 다운로드하거나 클론합니다.
2. PowerShell을 관리자 권한으로 실행합니다.
3. 다음 명령을 실행합니다.

```powershell
Set-ExecutionPolicy -Scope Process Bypass -Force
.\install.ps1
```

설치 후 `Win+Q`를 누르면 오버레이가 열립니다.

## 주요 기능

- AutoHotkey v2 기반 `Win+Q` 오버레이
- Windows 내장 OCR API 사용
- 화면 어디서든 텍스트 복사
- 브라우저 기반 빠른 번역
- 선택 텍스트 웹 검색
- Copilot+ 하드웨어 없이 동작
- 설정 파일 위치: `%APPDATA%\OpenClickToDo\settings.json`

## Microsoft Click to Do가 아닙니다

Open Click to Do는 Microsoft Click to Do를 활성화, 패치, 잠금 해제, 비활성화하거나 수정하지 않습니다. Copilot, WSAIFabricSvc, WorkloadsSessionHost, WindowsWorkload 패키지, WindowsApps, 관련 정책도 건드리지 않습니다.

이 프로젝트는 화면을 캡처하고, Windows OCR을 실행하고, 감지된 텍스트 영역에 액션 메뉴를 제공하는 별도 오픈소스 앱입니다.

또한 이 저장소는 `Copilot AI Memory Saver`와 별도입니다. Open Click to Do는 일반 Windows 사용자용 생산성 앱이고, Copilot AI Memory Saver는 Copilot+ PC 메모리 문제를 다루는 다른 프로젝트입니다.

## 대상 사용자

- Copilot+ PC가 아니어서 Microsoft Click to Do를 사용할 수 없는 사용자
- `Win+Q`로 OCR 액션을 실행하고 싶은 사용자
- 스크린샷, 앱, 이미지, PDF, 영상 속 텍스트를 빠르게 복사하거나 번역하고 싶은 사용자
- Copilot AI 기능은 원하지 않지만 화면 액션은 원하는 사용자
- PowerToys Text Extractor보다 액션 중심의 UX를 원하는 사용자

## 사용 방법

AutoHotkey 스크립트가 실행 중일 때 `Win+Q`를 누릅니다.

앱이 기본 모니터를 캡처하고 Windows OCR을 실행한 뒤, 감지된 텍스트 영역을 반투명 오버레이로 표시합니다. 텍스트 블록을 클릭한 다음 아래 액션을 선택합니다.

- `Copy`
- `Translate`
- `Search`
- `Close`

키보드:

- `Ctrl+C`: 선택 텍스트 복사
- `Enter`: 기본 액션 실행, 현재 기본값은 Copy
- `Esc`: 오버레이 닫기

핫키 설치 없이 테스트하려면 앱을 일반 실행한 뒤 `Capture Now`를 누르거나 다음처럼 실행합니다.

```powershell
.\src\OpenClickToDo\bin\Release\net8.0-windows10.0.19041.0\win-x64\OpenClickToDo.exe --capture
```

## 핫키 변경

기본 핫키는 AutoHotkey v2 기준 `#q`, 즉 `Win+Q`입니다.

`Win+Shift+Q`로 바꾸려면 `scripts/WinQ-OpenClickToDo.ahk`에서 핫키를 다음처럼 바꿉니다.

```ahk
#+q:: {
    Run '"' ExePath '" --capture', InstallDir
}
```

수정 후 다시 설치하거나 `%LOCALAPPDATA%\OpenClickToDo\scripts\WinQ-OpenClickToDo.ahk`에 복사합니다.

## 빌드

필요 항목:

- Windows 10 또는 Windows 11
- .NET 8 SDK
- Windows OCR 언어 지원
- 전역 단축키용 AutoHotkey v2

빌드:

```powershell
dotnet build .\src\OpenClickToDo\OpenClickToDo.csproj
```

배포 빌드:

```powershell
dotnet publish .\src\OpenClickToDo\OpenClickToDo.csproj -c Release -r win-x64 --self-contained false -o .\artifacts\publish\win-x64
```

## 설치

관리자 PowerShell에서 실행합니다.

```powershell
Set-ExecutionPolicy -Scope Process Bypass -Force
.\install.ps1
```

설치 스크립트는 다음 작업만 수행합니다.

- AutoHotkey v2 확인, 없으면 winget 설치 시도
- .NET publish 또는 기존 publish 산출물 확인
- `%LOCALAPPDATA%\OpenClickToDo`에 앱과 AHK 스크립트 복사
- 시작프로그램에 Open Click to Do 전용 바로가기 생성
- 전용 AutoHotkey 스크립트 실행

Microsoft Click to Do, WSAIFabricSvc, WorkloadsSessionHost, WindowsWorkload 패키지는 건드리지 않습니다.

## 삭제

관리자 PowerShell에서 실행합니다.

```powershell
.\uninstall.ps1
```

사용자 설정까지 지우려면:

```powershell
.\uninstall.ps1 -RemoveSettings
```

## 개인정보

- OCR은 기본적으로 Windows OCR로 로컬에서 실행됩니다.
- Copy는 로컬 클립보드에만 복사합니다.
- Browser Translate는 선택 텍스트를 번역 웹사이트로 보낼 수 있습니다.
- API 번역 provider는 선택 텍스트를 해당 provider로 보냅니다.
- Ollama 같은 로컬 provider는 텍스트를 로컬에 둘 수 있습니다.
- 로그에는 원문 전체를 남기지 않고 provider와 텍스트 길이 정도만 남깁니다.
- 설정 파일은 `%APPDATA%\OpenClickToDo\settings.json`에 저장됩니다.

자세한 내용은 [docs/privacy.md](docs/privacy.md)를 보세요.

## 제한사항

- OCR 정확도는 Windows OCR 언어팩과 화면 품질에 좌우됩니다.
- 손글씨, 작은 글자, 저해상도 이미지, 꾸밈이 심한 글자는 실패할 수 있습니다.
- Browser Translate는 완전한 인앱 즉시 번역 UI가 아닙니다.
- 인앱 번역은 provider 설정이 필요합니다.
- AHK 스크립트가 실행 중이면 `Win+Q`가 기존 Windows 단축키를 덮을 수 있습니다.
- MVP는 기본 모니터 안정 동작을 우선합니다.
- 드래그 선택은 로드맵에 포함되어 있습니다.

## 로드맵

- OCR 영역 드래그 선택
- 인라인 번역 UI
- 로컬 LLM 액션
- 요약, 설명, 다시 쓰기
- 다중 모니터 개선
- 포터블 릴리스
- Microsoft Store / winget 패키지

자세한 내용은 [docs/roadmap.md](docs/roadmap.md)를 보세요.

## 라이선스

MIT
