/*
# 『도사 전우치』 데이터 시스템 연동 가이드 및 구현 요청

## 1. 프로젝트 개요

using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

현재 Unity 6 기반 2D 탑다운 뱀파이어 서바이버류 게임 『도사 전우치』를 팀 프로젝트로 개발하고 있습니다.

Excel 데이터를 JSON으로 변환하고 Addressables에 등록한 뒤, 게임 실행 시 C# 객체로 역직렬화하는 데이터 시스템은 이미 구현되어 있습니다.

**이번 작업의 목적은 기존 데이터 시스템을 새로 만드는 것이 아니라, 이미 로드된 데이터를 실제 게임 로직에 적용하는 것입니다.**

## 2. 현재 구현된 데이터 파이프라인

```text
Excel
  ↓
ExcelReader
  ↓
ExcelImporter
  ↓
JSON 생성
  ↓
AddressableRegistrar
  ↓
Addressables 등록
  ↓
DataManager.Init()
  ↓
Addressables.LoadAssetAsync<TextAsset>()
  ↓
JsonUtility.FromJson<T>()
  ↓
C# 데이터 객체 생성
  ↓
Dictionary 저장
  ↓
게임 시스템에서 참조
```

Excel에서 데이터를 수정하고 Unity 상단의 `Tools → Data → Read Excel`을 실행하면 JSON이 갱신되고 Addressables에 등록됩니다.

이 과정은 이미 정상 작동하는 것을 확인했습니다.

## 3. 현재 데이터 구조

### PlayerStatData

```csharp
[Serializable]
[ExcelSheet("PlayerStat")]
public class PlayerStatData
{
    public string Id;
    public float BaseValue;
}
```

- `Id`: 스탯 데이터 식별자
- `BaseValue`: 기본 스탯 수치

### AugmentData

```csharp
[Serializable]
[ExcelSheet("Augment")]
public class AugmentData
{
    public string Id;
    public string ConfigKey;
    public string AttackConfigKey;
    public int Value;
}
```

- `Id`: 증강 데이터 식별자
- `ConfigKey`: 증강 설정 데이터 연결 키
- `AttackConfigKey`: 공격 설정 데이터 연결 키
- `Value`: 증강 관련 수치

### LocalizationData

```csharp
[Serializable]
[ExcelSheet("Localization_KO")]
public class LocalizationData
{
    public string Key;
    public string NameKO;
    public string DescriptionKO;
    public string Category;
}
```

- `Key`: 로컬라이징 데이터 식별자
- `NameKO`: 한국어 이름
- `DescriptionKO`: 한국어 설명
- `Category`: 분류

실제 필드의 게임 내 의미와 적용 방식은 Excel 기획 데이터 및 기존 게임 로직을 확인한 뒤 결정해야 합니다.

## 4. DataManager 사용 방법

`DataManager`는 다음 Dictionary를 제공합니다.

```csharp
public Dictionary<string, PlayerStatData> PlayerStatDic { get; private set; }

public Dictionary<string, AugmentData> AugmentDic { get; private set; }

public Dictionary<string, LocalizationData> LocalizationDic { get; private set; }
```

게임 로직에서는 `Managers.Data`를 통해 접근할 수 있습니다.

예시:

```csharp
if (Managers.Data.PlayerStatDic.TryGetValue(
    "STAT001",
    out PlayerStatData stat))
{
    float baseValue = stat.BaseValue;
}
```

위 코드는 데이터 조회 예시이며, 실제 ID와 해당 스탯의 의미는 Excel 데이터를 확인해야 합니다.

**주의: `DataManager`는 Addressables를 비동기로 로드하므로 데이터 로딩이 완료되기 전에는 Dictionary를 사용하지 않아야 합니다.**

다음 속성과 이벤트를 활용할 수 있습니다.

```csharp
Managers.Data.IsLoaded

Managers.Data.DataLoadCompleted

Managers.Data.DataLoadFailed
```

## 5. 구현 요청 사항

### A. 플레이어 스탯 적용

현재 플레이어의 체력, 이동속도, 공격 관련 수치 등을 관리하는 스크립트를 먼저 분석해 주세요.

이후 기존에 코드로 직접 지정한 수치 중 Excel 데이터와 대응되는 항목을 찾아 `PlayerStatDic`에서 읽어 적용하도록 구현해 주세요.

- 데이터 로딩 완료 후 스탯 적용
- Dictionary에서 ID 기반 데이터 조회
- 기존 플레이어 스탯 관리 구조 최대한 유지
- 데이터 누락 시 오류 처리
- 기본 스탯과 게임 진행 중 변경되는 현재 스탯 구분

특히 `PlayerStatData`는 원본 설정 데이터이므로 게임 중 체력 감소나 버프 등으로 값이 변경되는 경우, 원본 Dictionary 값을 직접 수정하지 않고 별도의 런타임 상태에서 관리하도록 해 주세요.

### B. 증강 시스템 적용

현재 구현된 공격 시스템 및 증강 관련 스크립트를 확인하고 `AugmentDic`과 연결해 주세요.

- `AugmentData`를 통해 증강 데이터 조회
- `ConfigKey`, `AttackConfigKey`를 기존 설정 시스템과 연결할 수 있는지 확인
- `Value`가 실제 어떤 방식으로 적용되어야 하는지 기존 코드와 기획 데이터 분석
- 공격 로직과 데이터 로딩 로직의 책임 분리
- 추후 증강 데이터가 늘어나더라도 기존 구조를 확장하기 쉽게 구현

`Value`가 증가량인지, 초기값인지, 다른 설정값인지는 임의로 가정하지 말고 확인해 주세요.

### C. 로컬라이징 데이터 적용

`LocalizationDic`을 활용하여 UI에서 이름과 설명을 표시할 수 있도록 구현해 주세요.

예를 들어 증강 선택 UI에서 로컬라이징 Key를 이용해 한국어 이름과 설명을 가져오는 구조를 검토해 주세요.

기존 UI 시스템이 있다면 해당 구조를 유지하면서 연결해 주세요.

## 6. 반드시 지켜야 할 사항

1. 기존 `ExcelReader`, `ExcelImporter`, `AddressableRegistrar`, `DataManager`를 중복 구현하지 마세요.
2. JSON 파일을 게임 로직에서 직접 다시 파싱하지 마세요.
3. 데이터 접근은 가급적 `Managers.Data`를 통해 통일해 주세요.
4. Addressables 비동기 로딩 완료 여부를 반드시 확인해 주세요.
5. 기존 MVC 및 State Pattern 구조를 가능한 한 유지해 주세요.
6. 기존 공격 시스템, 오브젝트 풀링, ScriptableObject 설정 구조가 있다면 우선 분석하고 재사용해 주세요.
7. 새로운 데이터 필드나 ID를 임의로 추가하거나 가정하지 마세요.
8. 기존 동작을 변경해야 한다면 먼저 변경 이유와 영향 범위를 설명해 주세요.

## 7. 향후 LoadingManager 연동 예정

현재는 `Managers.Start()`에서 임시로 `DataManager.Init()`을 호출하고 있습니다.

추후 다음 흐름을 구현할 예정입니다.

```text
LoadingManager
    ↓
DataManager.Init()
    ↓
DataLoadCompleted
    ↓
기타 필수 로딩 작업 완료 확인
    ↓
LoadingCompleted
    ↓
SceneManager 씬 전환
```

따라서 이번 작업에서는 새로운 로딩 시스템을 임의로 만들지 말고, 기존 `IsLoaded` 및 `DataLoadCompleted`를 활용해 데이터 사용 시점을 안전하게 처리해 주세요.

## 8. 작업 진행 방식

**바로 코드를 작성하기보다 다음 순서로 진행해 주세요.**

1. 현재 프로젝트에서 플레이어 스탯, 공격, 증강, UI 관련 스크립트를 확인합니다.
2. Excel 데이터와 기존 C# 코드의 연결 관계를 분석합니다.
3. 어떤 스크립트에 어떤 데이터를 적용할지 제안합니다.
4. 기존 구조를 최대한 유지하면서 필요한 코드만 수정합니다.
5. Unity에서 실제로 Excel 값 변경 → Tools 실행 → 게임 내 수치 반영까지 검증합니다.

최종적으로 **Excel 데이터만 수정해도 게임 내 스탯, 증강 수치, UI 텍스트 등에 변경 사항이 반영되는 데이터 기반 구조**를 만드는 것이 목표입니다.
*/