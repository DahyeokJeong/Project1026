/*# 『도사 전우치』 Excel 데이터 연동 작업 인수인계 및 구현 요청

## 1. 프로젝트 개요

using System;
using System.Collections.Generic;

Unity 6 기반 2D 탑다운 뱀파이어 서바이버류 게임 『도사 전우치』를 개발하고 있습니다.

현재 프로젝트에는 다음 시스템이 구현되어 있습니다.

- MVC + State Pattern 기반 플레이어 이동
- `PlayerAutoAttack` 기반 자동 공격
- `IAttack` 인터페이스 및 `ProjectileAttack` 공격 실행
- `UnityEngine.Pool` 기반 오브젝트 풀링
- `AttackConfigSO` 기반 공격 프리팹 설정
- Excel → JSON → Addressables → DataManager 데이터 파이프라인

**이번 담당 작업은 Excel 데이터를 실제 게임 로직에 연결하는 것입니다.**

SO 에셋 제작 및 공격 종류 확장은 다른 담당자가 진행하므로, 해당 영역은 임의로 변경하지 마세요.

## 2. 현재 완성된 데이터 파이프라인

```text
Excel
  ↓
ExcelReader
  ↓
ExcelImporter
  ↓
JSON 파일 생성
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
```

Unity 메뉴 `Tools → Data → Read Excel`을 실행하면 Excel 데이터가 JSON으로 변환되고 Addressables에 등록됩니다.

다음 테스트까지 완료했습니다.

- JSON 삭제 후 Tools 실행 시 자동 재생성
- Excel 데이터 변경 시 JSON 갱신
- Addressables 등록 확인
- 게임 실행 시 `DataManager 초기화 완료` 로그 확인

따라서 기존 데이터 변환 및 로딩 시스템을 새로 구현할 필요가 없습니다.

## 3. 데이터 관련 파일 구조

```text
Assets/Project1026/
├── 01.Scripts/
│   ├── 06.Managers/
│   │   ├── Managers.cs
│   │   └── DataManager.cs
│   │
│   └── 10.Data/
│       ├── Attribute/
│       │   └── ExcelSheetAttribute.cs
│       ├── Editor/
│       │   ├── ExcelReader.cs
│       │   ├── ExcelImporter.cs
│       │   └── AddressableRegistrar.cs
│       ├── Loader/
│       │   ├── IDataLoader.cs
│       │   ├── PlayerStatLoader.cs
│       │   ├── AugmentLoader.cs
│       │   └── LocalizationLoader.cs
│       └── Models/
│           ├── PlayerStatData.cs
│           ├── AugmentData.cs
│           └── LocalizationData.cs
│
└── 06.Datas/
    ├── Excel/
    └── JSON/
        ├── PlayerStat.json
        ├── Augment.json
        └── Localization_KO.json
```

## 4. DataManager 사용 방법

현재 DataManager는 다음 Dictionary를 제공합니다.

```csharp
public Dictionary<string, PlayerStatData> PlayerStatDic { get; private set; }

public Dictionary<string, AugmentData> AugmentDic { get; private set; }

public Dictionary<string, LocalizationData> LocalizationDic { get; private set; }
```

접근 방법:

```csharp
Managers.Data.PlayerStatDic
Managers.Data.AugmentDic
Managers.Data.LocalizationDic
```

예시:

```csharp
if (Managers.Data.PlayerStatDic.TryGetValue(
    "STAT001",
    out PlayerStatData stat))
{
    float baseValue = stat.BaseValue;
}
```

`STAT001`은 조회 방법을 설명하기 위한 예시입니다. 실제 스탯과 ID의 연결은 Excel 데이터를 확인한 뒤 결정해야 합니다.

### 비동기 로딩 주의사항

DataManager는 Addressables를 비동기로 로드합니다.

다음 속성과 이벤트를 활용할 수 있습니다.

```csharp
Managers.Data.IsLoaded
Managers.Data.DataLoadCompleted
Managers.Data.DataLoadFailed
```

데이터 로딩이 끝나기 전에 Dictionary를 조회하면 안 됩니다.

이벤트를 구독할 때는 이미 로딩이 완료된 경우도 처리하고, 컴포넌트가 파괴될 때 이벤트 구독을 해제해 주세요.

현재 `Managers.Start()`에서 임시로 `DataManager.Init()`을 호출하고 있습니다.

향후 LoadingManager가 초기화를 담당할 예정이므로 새로운 초기화 호출을 여러 곳에 중복 추가하지 마세요.

## 5. Excel 데이터 구조

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

플레이어의 기본 스탯 수치를 관리합니다.

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

증강 관련 수치와 설정 연결 키를 관리합니다.

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

한국어 이름과 설명을 관리합니다.

**Excel 데이터에 없는 필드나 ID를 임의로 가정하지 마세요.**

## 6. 작업 1 — PlayerStat과 PlayerModel 연결

가장 먼저 진행할 작업입니다.

현재 `PlayerModel`은 게임 진행 중 사용하는 HP, 공격력, 공격속도 등의 스탯을 관리합니다.

목표는 Excel의 `PlayerStat` 데이터를 `PlayerModel`에 적용하는 것입니다.

### 요구사항

1. 기존 `PlayerModel.cs`의 필드와 프로퍼티를 분석합니다.
2. Excel의 스탯 ID와 PlayerModel의 스탯 간 대응 관계를 확인합니다.
3. `DataManager.PlayerStatDic`에서 기본 수치를 조회합니다.
4. 데이터 로딩 완료 후 PlayerModel의 초기 스탯에 적용합니다.
5. 게임 중 증강이나 피해로 변경되는 현재 스탯은 별도로 관리합니다.
6. 원본 Dictionary에 저장된 데이터를 직접 수정하지 않습니다.
7. 존재하지 않는 스탯 ID에 대한 오류 처리를 구현합니다.

예상 흐름:

```text
PlayerStat.xlsx
    ↓
PlayerStat.json
    ↓
DataManager.PlayerStatDic
    ↓
PlayerModel 초기화
    ↓
PlayerMovement / PlayerAutoAttack
```

실제 Excel은 `.xlsx` 통합문서 안의 `PlayerStat` 시트를 의미하며, 파일명은 프로젝트에서 확인해야 합니다.

### 검증

Excel에서 공격력 또는 이동속도 관련 수치를 변경한 뒤 Tools를 실행합니다.

게임을 재실행했을 때 PlayerModel에 변경된 수치가 반영되고 실제 게임 동작에도 적용되는지 확인합니다.

## 7. 작업 2 — AugmentData 연결

PlayerStat 연동이 완료되면 증강 데이터를 연결합니다.

현재 공격 시스템은 다음과 같습니다.

```text
PlayerAutoAttack
    ↓
List<IAttack>
    ↓
ProjectileAttack
    ↓
AttackConfigSO
    ↓
PoolManager
    ↓
ProjectileBase
```

`PlayerAutoAttack`은 `AttackSlot` 목록으로 공격 설정을 관리하며, `AttackConfigSO`는 공격 프리팹을 참조합니다.

### 요구사항

1. 기존 증강 시스템 구현 여부를 먼저 확인합니다.
2. `AugmentDic`에서 증강 데이터를 조회하는 방식을 구현합니다.
3. `ConfigKey`와 `AttackConfigKey`가 실제 SO 설정과 어떻게 연결되는지 확인합니다.
4. `Value`의 의미를 Excel 기획 데이터와 비교해 확인합니다.
5. 증강 수치는 실제 게임의 런타임 스탯이나 공격 상태에 적용합니다.
6. 원본 SO 에셋과 DataManager의 원본 데이터를 직접 수정하지 않습니다.
7. 공격 개수 증가 등 증강 적용에 필요한 인터페이스를 공격 시스템 담당자와 협의합니다.

특히 현재 `ProjectileAttack`은 발사 시 투사체 하나를 생성하므로, 투사체 개수 증가 증강은 별도 구현이 필요합니다.

이 작업은 다른 담당자의 공격 시스템 작업과 충돌하지 않도록 연결 지점을 먼저 협의해 주세요.

## 8. 작업 3 — LocalizationData 연결

`LocalizationDic`을 활용하여 게임 UI에 한국어 이름과 설명을 표시합니다.

### 요구사항

- Key를 이용한 로컬라이징 데이터 조회
- `NameKO`, `DescriptionKO` 활용
- 존재하지 않는 Key에 대한 처리
- 기존 UI 구조 유지
- 증강 선택 UI 등에서 재사용 가능한 조회 방식 구성

UI 자체가 아직 구현되지 않았다면 데이터 조회 기능만 우선 준비하고, 실제 UI 연결은 후속 작업으로 분리해도 됩니다.

## 9. 기존 시스템과의 역할 분리

| 시스템 | 담당 역할 |
|---|---|
| Excel / JSON | 기본 수치와 텍스트 원본 |
| DataManager | JSON 로딩 및 Dictionary 보관 |
| PlayerModel | 플레이어의 현재 런타임 스탯 |
| PlayerController | 상태 전환 |
| PlayerAutoAttack | 자동 공격 실행 관리 |
| ProjectileAttack | 공격 주기 및 투사체 발사 |
| AttackConfigSO | 공격 프리팹 설정 |
| PoolManager | 오브젝트 풀링 |
| 증강 시스템 | 런타임 효과 적용 |
| UI 시스템 | 로컬라이징 텍스트 표시 |

## 10. 향후 LoadingManager 연동

현재 데이터 초기화는 임시로 `Managers.Start()`에서 수행합니다.

향후 다음 구조로 변경할 예정입니다.

```text
LoadingManager
    ↓
DataManager.Init()
    ↓
DataLoadCompleted
    ↓
기타 필수 로딩 완료 확인
    ↓
LoadingCompleted
    ↓
SceneManager 씬 전환
```

이번 작업에서는 LoadingManager와 SceneManager를 새로 구현하지 마세요.

다만 추후 해당 구조로 변경하더라도 정상 작동하도록 데이터 로딩 완료 이벤트를 기준으로 초기화하는 방식을 유지해 주세요.

## 11. 개발 시 준수사항

- 기존 Excel 변환 시스템과 DataManager를 불필요하게 수정하지 않습니다.
- 기존 MVC + State Pattern 구조를 유지합니다.
- SO 제작 및 공격 패턴 확장은 다른 담당자의 작업으로 남겨둡니다.
- 수치 데이터는 가능한 한 Excel에서 관리합니다.
- 게임 중 변경되는 값은 런타임 객체에서 관리합니다.
- 하드코딩된 수치와 중복 데이터 관리를 최소화합니다.
- 기존 스크립트를 변경하기 전 영향 범위를 설명합니다.
- 새로운 기능을 구현할 때 기존 시스템과의 연결 관계를 먼저 확인합니다.
- 코드는 가급적 전체 스크립트 단위로 제공하고, 코드 내부의 불필요한 주석은 추가하지 않습니다.

## 12. 작업 진행 순서

**한 번에 모든 시스템을 수정하지 말고 다음 순서로 진행해 주세요.**

1. 현재 PlayerModel과 Excel PlayerStat 데이터를 분석합니다.
2. PlayerStat ID와 런타임 스탯의 연결 방식을 제안합니다.
3. PlayerModel에 Excel 기본 스탯을 적용합니다.
4. Excel 수정 후 실제 게임 반영 테스트를 진행합니다.
5. AugmentData 연결 구조를 검토하고 구현합니다.
6. LocalizationData 조회 기능을 구현합니다.
7. 전체 데이터 연동 결과를 검증합니다.

각 단계가 정상 작동하는 것을 확인한 후 다음 단계로 넘어갑니다.

**최종 목표는 Excel에서 밸런스 데이터를 수정하고 Tools를 실행하면, 게임 재실행 시 해당 변경 사항이 실제 게임 플레이에 반영되는 구조를 완성하는 것입니다.**

우선 `PlayerModel.cs`, `DataManager.cs`, Excel의 `PlayerStat` 시트부터 확인하고, 기존 코드를 분석한 후 첫 번째 구현 방향을 제안해 주세요.*/