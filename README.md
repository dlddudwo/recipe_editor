# Recipe Editor 매뉴얼

이 문서는 `RecipeEditorForm.cs` 코드를 기준으로 현재 구현된 **JSON 레시피 편집기**의 동작을 빠르게 파악할 수 있도록 만든 개발/운영 매뉴얼입니다.

## 1) 개요

`RecipeEditorForm`은 WinForms + Newtonsoft.Json 기반의 JSON 편집 UI입니다.

핵심 기능:
- JSON 파일 불러오기/저장
- TreeView 기반 구조 탐색
- 노드 추가(Object/Array/Value), 삭제, 이름/값 수정(F2)
- 노드 복사/붙여넣기
- 파일 폴더 스캔 후 DataGridView에서 더블클릭 로드
- 검색(트리/텍스트)

## 2) 주요 구조

### 클래스
- `RecipeEditorForm : MetroForm`

### 핵심 상태값
- `jsonFilePath`: 현재 열려 있는 JSON 파일 경로
- `jsonObject`: 현재 편집 대상 JSON 객체(`JObject`)
- `NodeSource` / `NodeTarget`: 트리 노드 복사/붙여넣기 출발/목적 노드
- `searchResults`, `currentIndex`: 검색 결과와 현재 인덱스
- `settingsFilePath`, `settingsFolderPath`: 마지막 파일/폴더 경로 저장용 설정 파일

### 내부 enum
- `json_type`: `Obejct`, `Array`, `Value` (노드 추가 타입)
- `Get_node_mode`: `Inform`, `Delete` (노드 경로 문자열 포맷)
- `Path_skip_mode`: `Skip`, `Noskip` (루트 경로 처리 모드)

## 3) 시작 시 동작

폼 생성자에서 아래를 수행합니다.
1. 커스텀 타이틀바 생성
2. 트리/컨텍스트 메뉴/라벨 수정 이벤트 바인딩
3. 마지막에 열었던 JSON 파일 자동 로드 (`LoadPreviousFilePath`)
4. 마지막에 열었던 폴더 자동 로드 (`LoadPreviousFolderPath`)

즉, 설정 파일이 존재하면 앱 실행 직후 편집 상태를 거의 바로 복원합니다.

## 4) JSON 로드/표시 로직

### `LoadJson(string json_path)`
- 파일이 있으면 텍스트를 읽고 `JObject.Parse`
- `richTextBox_json`에도 원문을 표시
- 파일이 없으면 빈 `JObject` 생성

### `PopulateTreeView()` + `AddNodes(JToken, TreeNode)`
- Root 노드를 만들고 JSON 전체를 재귀 순회해 트리 생성
- 값 타입별 아이콘 지정:
  - 숫자(Float/Integer)
  - 불리언(Boolean)
  - 문자열(String)
  - 그 외 Object/Array

## 5) 노드 편집 기능

### 5-1. 우클릭 추가
컨텍스트 메뉴로 다음 항목 추가 가능:
- Object 추가 (`objectToolStripMenuItem_Click`)
- Value 추가 (`valueToolStripMenuItem_Click`)
- Array 추가 (`arrayToolStripMenuItem_Click`)

공통 흐름:
1. 선택 노드의 JSON 경로 계산 (`Select_json_path`)
2. 선택 노드 타입 검증 (Object/Array만 허용)
3. TreeView에 임시 노드 생성
4. 실제 JSON에도 반영 (`Add_Json`)

### 5-2. 삭제
`deleteNodeToolStripMenuItem_Click`
- 확인 팝업 후 삭제
- 배열 요소인지 일반 프로퍼티인지 경로로 판별
- 배열이면 인덱스로 `RemoveAt`, 아니면 `select_token.Parent.Remove()`
- 마지막에 트리 노드도 제거

### 5-3. 라벨 수정(F2)
`treeViewJson_AfterLabelEdit`
- 노드 텍스트를 기반으로 key/value 변경 반영
- `key:value` 형태면 타입 자동 판별:
  - 숫자 → int/double
  - `true/false` → bool
  - 그 외 → string
- 중복 키 등 예외 발생 시 경고 메시지

> 중요: Value 노드 편집 시 `:` 구분자가 필수입니다.

## 6) 복사/붙여넣기

### 트리 구조 복사
- `copyNodeToolStripMenuItem_Click`: `NodeSource` 저장
- `pasteToolStripMenuItem_Click`: `NodeTarget`에 트리 노드 재귀 복사 (`CopyNodes`)

### JSON 데이터 복사
붙여넣기 시 트리만 복사하면 불일치가 생기므로, 아래를 추가로 수행:
- 원본/대상 JSON 토큰 획득
- `CopyToken(source, target)`으로 재귀 복사
  - Object: 프로퍼티 단위 복제
  - Array: 항목 순회 복제

## 7) 검색 기능

### 트리 검색
- `BtnSearch`: 첫 매칭 노드 선택 (`FindNode`)
- `BtnSearchNext`: 다음 노드 (`FindNextNode`)
- `BtnSearchPrevious`: 이전 노드 (`FindPrevNode`)

### 텍스트 검색
- `richTextBox_json`에서 라인 기준 매칭 결과를 `searchResults`에 저장
- `DisplayCurrentResult()`로 현재 매칭 텍스트 하이라이트/스크롤
- `SearchButton_Click`은 `listView_SearchResult`에 발생 위치 목록 추가

## 8) 파일/폴더 관리

### 파일 열기
- `BtnOpenFile` 클릭 시 `OpenFileDialog`
- 선택 파일 경로를 설정 파일에 저장 (`SaveFilePath`)
- JSON 로드 후 트리 갱신

### 저장
- `BtnSave` 클릭 시 확인 팝업
- `SaveJson()`에서
  1. 타임스탬프 백업 파일 생성 (`원본명_yyyyMMdd_HHmmss.json`)
  2. 원본 파일 overwrite
- 저장 후 마지막 폴더 기준으로 목록 재로딩

### 폴더 열기
- `BtnOpenFolder`에서 폴더 선택
- 폴더 내 `*.json` 파일만 DataGridView에 표시
- 수정일 기준 내림차순 정렬
- 목록 더블클릭 시 해당 파일 즉시 로드

## 9) 경로 처리 핵심

코드에서 경로 문자열을 자주 변환합니다.
- 트리 경로: `Root/부모/자식` 또는 `Root->부모->자식`
- JSON SelectToken용 경로: `a.b[0].c`

이를 위해 아래 유틸 사용:
- `GetNodePath(node, mode)`
- `Select_json_path(node, mode)`

배열 인덱스(`[...]`)와 key:value 노드 텍스트를 분리/정리하는 처리가 많으므로, 신규 기능 추가 시 이 두 함수의 규칙을 먼저 맞추는 것이 안전합니다.

## 10) UI 보조 기능

- `CreateCustomTitleBar()`
  - 기본 타이틀바 대신 커스텀 패널 + 최소화/최대화/닫기 버튼 구성
- `Form_Resize`
  - 폼 크기 변경 시 타이틀바 위치 재정렬
- `TreeViewJson_MouseMove`
  - 마우스가 가리키는 노드 경로를 `rtbNodeLocation`에 표시

## 11) 알려진 주의사항

1. `json_type.Obejct` 오타가 enum/호출부 전반에 존재합니다.
   - 현재 동작에는 영향이 없지만 유지보수성 측면에서 추후 정리 권장.
2. Apply/Save 순서가 UX적으로 강제됩니다.
   - Save 전에 Apply 했는지 팝업으로 확인함.
3. `valueToolStripMenuItem_Click`의 조건식은 읽기 어렵습니다.
   - 현재는 배열에 Value 직접 추가를 막는 의도이지만, 조건식 단순화 여지 있음.
4. 복사/붙여넣기는 대상 타입(Object/Array)에 따라 JSON 병합 결과가 달라질 수 있어 사전 선택이 중요합니다.

## 12) 운영자가 자주 쓰는 기본 시나리오

1. `Open Folder`로 레시피 폴더 지정
2. 목록에서 파일 더블클릭
3. 트리에서 원하는 노드 우클릭 → 추가/삭제/수정
4. `Apply`로 우측 JSON 텍스트 최신화 확인
5. `Save`로 백업 + 원본 저장
6. 필요 시 검색(키워드, Next/Previous)으로 검증

---

필요하시면 다음 단계로,
- 사용자용(비개발자용) 화면 중심 매뉴얼
- 개발자용(함수별 시퀀스 다이어그램 포함) 기술 문서
둘 중 원하는 스타일로 더 세분화해드리겠습니다.
