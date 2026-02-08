# 🫧 Bubble Touch 60

**Bubble Touch 60**은 제한된 시간 내에 순서대로 거품을 터뜨려 높은 점수와 콤보를 달성하는 박진감 넘치는 2D 캐주얼 모바일 게임입니다.

## 🎮 게임 특징
- **동적 타겟팅 시스템**: 화면에 표시된 숫자를 순서대로 찾아 터뜨려야 합니다.
- **콤보 시스템**: 연속으로 빠르게 터뜨리면 콤보 게이지가 쌓이며, 점수 효율이 높아집니다.
- **실시간 UI 피드백**: 남은 시간, 현재 타겟, 콤보 진행 상황을 직관적인 프로그레스 바와 텍스트로 제공합니다.
- **사운드 연동**: 리드미컬한 배경음악과 터지는 손맛을 살린 효과음, 그리고 UI를 통한 세밀한 볼륨 조절 기능을 포함합니다.

## 🛠 기술 스택
- **Engine**: Unity 2022.3+ (URP 기반)
- **Language**: C#
- **UI System**: TextMeshPro, Unity UI (UGUI)
- **Input**: Input System Package

## 📂 주요 코드 구조 (`Assets/Scripts`)

### 🧠 Managers
- **`GameManager.cs`**: 전체 게임 루프(시작, 종료, 상태 관리) 및 점수/콤보 로직 담당.
- **`BubbleManager.cs`**: 거품의 생성, 그리드 배치, 숫자 할당 및 초기화 관리.
- **`SoundManager.cs`**: BGM 및 SFX 재생, 볼륨 조절 기능 제공.
- **`UIManager.cs`**: 실시간 점수 업데이트, 타이머 바, 게임 오버 패널 및 설정 메뉴 연동.

### 🎈 Gameplay
- **`Bubble.cs`**: 개별 거품의 클릭 감지, 애니메이션 효과 및 숫자 표시 처리.

## 🚀 설치 및 시작 방법
1. 이 저장소를 클론합니다.
2. Unity Hub에서 프로젝트를 추가하고 엽니다.
3. `Assets/Scenes/SampleScene.unity`를 엽니다.
4. **TextMeshPro** 자산이 없는 경우, 안내창에 따라 `Import TMP Essentials`를 실행합니다.
5. 상단의 **Play** 버튼을 눌러 게임을 시작하세요!

## ⚙️ 설정 (Inspector)
- `SoundManager`의 AudioSource와 Clip들을 연결해야 소리가 출력됩니다.
- `BubbleManager`의 `Bubble Prefab` 필드에 `Prefabs/Circle`이 할당되어 있는지 확인하세요.

---
*Developed as part of the Gemini CLI Project.*
