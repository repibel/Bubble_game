# Bubble Touch 60 - 최종 검수 및 변수 연결 가이드

이 문서는 현재 프로젝트 캡처본에서 보이는 UI 겹침 문제를 해결하고, `GameManager` 등에서 스크립트 제어가 가능하도록 Inspector를 최종 세팅하는 방법을 다룹니다.

---

## 1. UI 텍스트 가독성 및 배치 수정 (중요)

현재 화면 중앙에 텍스트가 겹치는 이유는 **Rect Transform의 크기(Width/Height)**가 작기 때문입니다.

### ① 상단 UI (Time, Score, Combo)
- **Time/Score/Combo 공통:**
  - `Width`: `300`, `Height`: `150` 정도로 충분히 넓혀주세요.
  - **TextMeshPro 컴포넌트:** `Alignment`를 'Center'와 'Middle'로 설정합니다.
- **위치(Pos Y):** 현재 너무 아래에 있습니다. `Pos Y`를 `-100`에서 `-150` 사이로 올려서 상단에 고정하세요.

### ② 게임 오버 패널 (Game_over)
- 현재 `Final_Score`와 `Max_Combo` 등이 패널 중앙에 뭉쳐 있습니다.
- **Final_Score:** `Pos Y: 100`, `Width: 800`, `Height: 200`
- **Max_Combo:** `Pos Y: -50`, `Width: 800`, `Height: 200`
- **Retry 버튼:** `Pos Y: -250` 정도로 내려서 텍스트와 겹치지 않게 하세요.

---

## 2. 스크립트 변수 연결 (Mapping)

`Gamemanager` 오브젝트를 클릭한 후, Inspector 창에 나타나는 스크립트 컴포넌트들에 하이어라키의 오브젝트들을 드래그 앤 드롭으로 연결해야 합니다.

### ① UIManager.cs 연결
Inspector의 `UIManager` 항목에 다음을 연결하세요:
- **Time Text:** `Canvas > Time` 오브젝트
- **Score Text:** `Canvas > Score` 오브젝트
- **Combo Text:** `Canvas > Combo` 오브젝트
- **Game Over Panel:** `Canvas > Game_over` 오브젝트 (처음엔 비활성화 해두세요)

### ② BubbleManager.cs 연결
- **Bubble Prefab:** `Assets > Prefabs > Bubble` (이미지 속 하얀 원형 프리팹)을 연결하세요.
- **Bubble Container:** 하이어라키의 `Bubble Container` 오브젝트를 연결하세요. 생성된 버블들이 이 아래로 들어갑니다.

---

## 3. 버블 프리팹(Bubble Prefab) 내부 텍스트 수정

현재 프리팹 안의 텍스트가 "New Text"로 되어 있을 것입니다.
1. `Assets > Prefabs > Bubble`을 더블클릭하여 프리팹 편집 모드로 들어갑니다.
2. 자식 오브젝트인 **Text (TMP)**의 설정을 확인합니다.
   - **Text:** 숫자 '1'을 임시로 적어 크기를 확인합니다.
   - **Extra Settings > Margins:** 모두 `0`인지 확인하세요.
   - **Wrapping:** `Disabled` (숫자가 가로로 길어질 때 줄바꿈 방지)

---

## 4. 최종 체크리스트 (CLI 입력용 데이터)

- **해상도 비율:** `Game` 뷰 상단에서 `16:9 Portrait` 혹은 `1080x1920`으로 설정되어 있는지 확인하세요.
- **버블 레이어:** 버블 프리팹의 레이어가 'UI'가 아닌 'Default'여야 마우스/터치 클릭(Raycast)이 물리 엔진에서 정상 작동합니다.