using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 구글 시트 TSV를 읽어서 Item SO 에셋을 자동 생성 / 업데이트하는 에디터 전용 Importer.
///
/// [이 스크립트의 목적]
/// - 팀원이 만들어둔 폴더 구조에 맞게 SO를 자동 분류 저장
/// - TYPE 값에 따라 올바른 SO 클래스를 생성
/// - 공통 필드(ID, TYPE, NAME, DESCRIPTION, RARITY, BUY_PRICE, SELL_PRICE, MAX_STACK, IMAGE)를 자동 적용
/// - 필요한 경우 타입별 추가 필드도 함께 적용
///
/// [현재 기준 저장 폴더 매핑]
/// - HarvestItem / OreItem / WoodItem -> Harvest 폴더, StaticItemSO 생성
/// - FishItem                       -> Fish 폴더, StaticItemSO 생성
/// - FeedItem                       -> Feed 폴더, FeedItemSO 생성
/// - SeedItem                       -> Seed 폴더, SeedItemSO 생성
/// - ToolItem                       -> Tool 폴더, ToolItemSO 생성
/// - BaitItem                       -> Bait 폴더, BaitItemSO 생성
/// - SpecialItem                    -> Special 폴더, SpecialItemSO 생성
///
/// [설정 에셋의 OutputFolder 권장값]
/// Assets/Resources/ScriptableObjects/Item
///
/// [주의]
/// - 이 스크립트는 Editor 폴더 안에 있어야 한다.
/// - Sprite Search Folders는 "폴더 경로"만 넣어야 한다.
/// - 구글 시트의 TYPE / RARITY 값은 enum과 최대한 동일해야 한다.
/// - 일부 흔한 오타(Basic, Prime, Masterwark 등)는 방어적으로 보정한다.
/// </summary>
public static class StaticItemSheetImporter
{
    #region ─────────────────────────▶ 내부 타입 ◀─────────────────────────
    /// <summary>
    /// 구글 시트 한 줄을 메모리로 옮긴 중간 데이터.
    /// 실제 SO에 넣기 전에 여기서 한 번 정리한다.
    /// </summary>
    private sealed class RowData
    {
        // ───────────────── 공통 필드 ─────────────────
        public string Id;
        public EType Type;
        public string Name;
        public string Description;
        public ERarity Rarity;
        public int BuyPrice;
        public int SellPrice;
        public int MaxStack;
        public string SpriteKey;

        // ───────────────── 타입별 선택 필드 ─────────────────
        // SeedItemSO
        public string PlaceCropId;
        public int NeedFarmingLevel;

        // ToolItemSO
        public int MaxDurability;
        public int CurrentLv;
        public float Range;
        public float Strength;

        // BaitItemSO
        public bool CanUseSea;
        public List<ERarity> CatchFishRarities = new List<ERarity>();
        public List<float> CatchFishWeights = new List<float>();

        // SpecialItemSO
        // 현재 팀 스크립트 상세 구조를 모르는 필드는 여기서 더 추가 가능
        public int MaxUseCount;
    }

    /// <summary>
    /// TYPE에 따라 어떤 SO 클래스를 만들지, 어느 폴더에 저장할지 묶어서 관리.
    /// </summary>
    private sealed class AssetTarget
    {
        public Type AssetType;
        public string FolderName;
        public string FilePrefix;
    }
    #endregion

    #region ─────────────────────────▶ 메뉴 ◀─────────────────────────
    [MenuItem("Tools/Google Sheet/Import Item SO From Selected Settings")]
    private static void ImportFromSelectedSettings()
    {
        StaticItemSheetImportSettingsSO settings = Selection.activeObject as StaticItemSheetImportSettingsSO;

        if (settings == null)
        {
            EditorUtility.DisplayDialog(
                "실패",
                "StaticItemSheetImportSettingsSO 에셋을 먼저 선택해주세요.",
                "확인");
            return;
        }

        if (settings.IsValid(out string reason) == false)
        {
            EditorUtility.DisplayDialog("설정 오류", reason, "확인");
            return;
        }

        try
        {
            Import(settings);
            EditorUtility.DisplayDialog("완료", "구글 시트 Item SO 가져오기가 끝났습니다.", "확인");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            EditorUtility.DisplayDialog("에러", ex.Message, "확인");
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 : 메인 흐름 ◀─────────────────────────
    /// <summary>
    /// 가져오기 전체 흐름.
    /// 1) TSV 다운로드
    /// 2) 헤더 해석
    /// 3) RowData 목록 생성
    /// 4) TYPE별 SO 생성 / 업데이트
    /// 5) 저장
    /// </summary>
    private static void Import(StaticItemSheetImportSettingsSO settings)
    {
        string tsvText = DownloadTsv(settings.TsvUrl);
        if (string.IsNullOrWhiteSpace(tsvText))
        {
            throw new InvalidOperationException("TSV 내용을 읽지 못했습니다.");
        }

        // OutputFolder는 루트 폴더로 사용한다.
        // 예: Assets/Resources/ScriptableObjects/Item
        EnsureFolderExists(settings.OutputFolder);

        // 스프라이트 맵을 먼저 만들어 두면 매 행마다 FindAssets를 반복하지 않아도 돼서 훨씬 빠르다.
        Dictionary<string, Sprite> spriteMap = BuildSpriteMap(settings.SpriteSearchFolders);

        // 시트 전체를 RowData 목록으로 변환
        List<RowData> rows = ParseRows(tsvText);

        int createdCount = 0;
        int updatedCount = 0;
        int skippedCount = 0;

        foreach (RowData row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.Id))
            {
                skippedCount++;
                continue;
            }

            bool created = CreateOrUpdateAsset(settings, row, spriteMap);

            if (created) createdCount++;
            else updatedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[StaticItemSheetImporter] 완료\n" +
            $"- 시트: {settings.SheetName}\n" +
            $"- 생성: {createdCount}\n" +
            $"- 수정: {updatedCount}\n" +
            $"- 스킵: {skippedCount}");
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 : 다운로드 / 파싱 ◀─────────────────────────
    /// <summary>
    /// 구글 시트 export TSV URL에서 문자열을 다운로드.
    /// </summary>
    private static string DownloadTsv(string url)
    {
        using (WebClient client = new WebClient())
        {
            byte[] data = client.DownloadData(url);
            return Encoding.UTF8.GetString(data);
        }
    }

    /// <summary>
    /// TSV 전체를 RowData 목록으로 바꾼다.
    /// 이 단계에서
    /// - 필수 헤더 존재 여부
    /// - enum 파싱
    /// - 숫자 파싱
    /// - 흔한 오타 보정
    /// - 중복 ID 체크
    /// 를 함께 처리한다.
    /// </summary>
    private static List<RowData> ParseRows(string tsvText)
    {
        List<RowData> result = new List<RowData>();

        string normalized = tsvText.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] lines = normalized.Split('\n');

        if (lines.Length < 2)
        {
            throw new InvalidOperationException("헤더 포함 최소 2줄 이상이어야 합니다.");
        }

        Dictionary<string, int> headerMap = BuildHeaderMap(lines[0]);

        // 공통 필수 헤더
        RequireHeader(headerMap, "ID");
        RequireHeader(headerMap, "TYPE");
        RequireHeader(headerMap, "NAME");
        RequireHeader(headerMap, "DESCRIPTION");
        RequireHeader(headerMap, "RARITY");
        RequireHeader(headerMap, "BUY_PRICE");
        RequireHeader(headerMap, "SELL_PRICE");
        RequireHeader(headerMap, "MAX_STACK");

        // 중복 ID 방지
        HashSet<string> duplicateCheck = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 1; i < lines.Length; ++i)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] cells = line.Split('\t');

            // ───────────────── 공통 필드 읽기 ─────────────────
            string id = NormalizeCell(GetCell(cells, headerMap, "ID"));
            if (string.IsNullOrWhiteSpace(id))
                continue;

            if (duplicateCheck.Contains(id))
            {
                Debug.LogWarning($"[StaticItemSheetImporter] 중복 ID 스킵: {id}");
                continue;
            }

            duplicateCheck.Add(id);

            string typeText = NormalizeCell(GetCell(cells, headerMap, "TYPE"));
            string name = NormalizeCell(GetCell(cells, headerMap, "NAME"));
            string description = NormalizeMultiline(GetCell(cells, headerMap, "DESCRIPTION"));
            string rarityText = NormalizeCell(GetCell(cells, headerMap, "RARITY"));

            // 숫자 칸은 "/", "-", 빈칸 등을 0으로 처리해서 방어
            string buyPriceText = NormalizeCell(GetCell(cells, headerMap, "BUY_PRICE"));
            string sellPriceText = NormalizeCell(GetCell(cells, headerMap, "SELL_PRICE"));
            string maxStackText = NormalizeCell(GetCell(cells, headerMap, "MAX_STACK"));
            string spriteKey = NormalizeCell(GetOptionalCell(cells, headerMap, "SPRITE_KEY"));

            if (TryParseType(typeText, out EType type) == false)
            {
                throw new InvalidOperationException($"TYPE 파싱 실패: {id} / {typeText}");
            }

            if (TryParseRarity(rarityText, out ERarity rarity) == false)
            {
                throw new InvalidOperationException($"RARITY 파싱 실패: {id} / {rarityText}");
            }

            int buyPrice = ParseIntSafe(buyPriceText);
            int sellPrice = ParseIntSafe(sellPriceText);
            int maxStack = ParseIntSafe(maxStackText, 1);

            if (string.IsNullOrWhiteSpace(spriteKey))
            {
                spriteKey = id;
            }

            RowData row = new RowData
            {
                Id = id,
                Type = type,
                Name = name,
                Description = description,
                Rarity = rarity,
                BuyPrice = buyPrice,
                SellPrice = sellPrice,
                MaxStack = maxStack,
                SpriteKey = spriteKey,

                // ───────────────── 선택 필드 파싱 ─────────────────
                // SeedItemSO
                PlaceCropId = NormalizeCell(GetOptionalCell(cells, headerMap, "PLACE_CROP_ID")),
                NeedFarmingLevel = ParseIntSafe(GetOptionalCell(cells, headerMap, "NEED_FARMING_LEVEL"), 0),

                // ToolItemSO
                MaxDurability = ParseIntSafe(GetOptionalCell(cells, headerMap, "MAX_DURABILITY"), 0),
                CurrentLv = ParseIntSafe(GetOptionalCell(cells, headerMap, "CURRENT_LV"), 0),
                Range = ParseFloatSafe(GetOptionalCell(cells, headerMap, "RANGE"), 0f),
                Strength = ParseFloatSafe(GetOptionalCell(cells, headerMap, "STRENGTH"), 0f),

                // BaitItemSO
                CanUseSea = ParseBoolSafe(GetOptionalCell(cells, headerMap, "CAN_USE_SEA")),

                // SpecialItemSO
                MaxUseCount = ParseIntSafe(GetOptionalCell(cells, headerMap, "MAX_USE_COUNT"), 0),
            };

            // Bait 배열 계열은 선택적으로 CSV처럼 "Basic,Prime" 형태 입력 가능
            ParseRarityList(
                GetOptionalCell(cells, headerMap, "CATCH_FISH_RARITIES"),
                row.CatchFishRarities);

            ParseFloatList(
                GetOptionalCell(cells, headerMap, "CATCH_FISH_WEIGHTS"),
                row.CatchFishWeights);

            ValidateRow(row);
            result.Add(row);
        }

        return result;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 : 검증 ◀─────────────────────────
    /// <summary>
    /// RowData 수준에서 기본 검증.
    /// 여기서 막히면 아예 SO 생성 전에 중단시켜서
    /// 잘못된 데이터가 에셋으로 들어가는 걸 방지한다.
    /// </summary>
    private static void ValidateRow(RowData row)
    {
        if (string.IsNullOrWhiteSpace(row.Id))
            throw new InvalidOperationException("ID가 비어 있습니다.");

        if (string.IsNullOrWhiteSpace(row.Name))
            throw new InvalidOperationException($"{row.Id} : NAME이 비어 있습니다.");

        if (string.IsNullOrWhiteSpace(row.Description))
            throw new InvalidOperationException($"{row.Id} : DESCRIPTION이 비어 있습니다.");

        if (row.Rarity == ERarity.None)
            throw new InvalidOperationException($"{row.Id} : RARITY가 None 입니다.");

        if (row.BuyPrice < 0)
            throw new InvalidOperationException($"{row.Id} : BUY_PRICE는 0 이상이어야 합니다.");

        if (row.SellPrice < 0)
            throw new InvalidOperationException($"{row.Id} : SELL_PRICE는 0 이상이어야 합니다.");

        // BUY_PRICE가 0이면 "구매 불가 / 획득 전용 아이템"으로 보고 허용
        if (row.BuyPrice > 0 && row.SellPrice > row.BuyPrice)
            throw new InvalidOperationException($"{row.Id} : SELL_PRICE가 BUY_PRICE보다 클 수 없습니다.");

        if (row.MaxStack <= 0)
            throw new InvalidOperationException($"{row.Id} : MAX_STACK은 1 이상이어야 합니다.");
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 : TYPE -> SO / 폴더 매핑 ◀─────────────────────────
    /// <summary>
    /// TYPE 값에 따라
    /// - 어떤 SO 클래스를 만들지
    /// - 어느 폴더에 저장할지
    /// - 파일 이름 prefix를 무엇으로 할지를 결정한다.
    ///
    /// 만들어둔 폴더 구조:
    /// Item/Bait
    /// Item/Feed
    /// Item/Fish
    /// Item/Harvest
    /// Item/Seed
    /// Item/Special
    /// Item/Tool
    /// </summary>
    private static AssetTarget ResolveAssetTarget(EType type)
    {
        switch (type)
        {
            // 일반 수확물 / 재료 / 광석 / 목재는 StaticItemSO로 묶되
            // 팀 폴더 구조상 Harvest 폴더에 넣는다.
            case EType.HarvestItem:
            case EType.OreItem:
            case EType.WoodItem:
                return new AssetTarget
                {
                    AssetType = typeof(StaticItemSO),
                    FolderName = "Harvest",
                    FilePrefix = "StaticItemSO"
                };

            // 물고기는 별도 Fish 폴더에 넣는다.
            case EType.FishItem:
                return new AssetTarget
                {
                    AssetType = typeof(StaticItemSO),
                    FolderName = "Fish",
                    FilePrefix = "StaticItemSO"
                };

            case EType.FeedItem:
                return new AssetTarget
                {
                    AssetType = typeof(FeedItemSO),
                    FolderName = "Feed",
                    FilePrefix = "FeedItemSO"
                };

            case EType.SeedItem:
                return new AssetTarget
                {
                    AssetType = typeof(SeedItemSO),
                    FolderName = "Seed",
                    FilePrefix = "SeedItemSO"
                };

           //case EType.ToolItem:
           //    return new AssetTarget
           //    {
           //        AssetType = typeof(ToolItemSO),
           //        FolderName = "Tool",
           //        FilePrefix = "ToolItemSO"
           //    };

            case EType.BaitItem:
                return new AssetTarget
                {
                    AssetType = typeof(BaitItemSO),
                    FolderName = "Bait",
                    FilePrefix = "BaitItemSO"
                };

            case EType.SpecialItem:
                return new AssetTarget
                {
                    AssetType = typeof(SpecialItemSO),
                    FolderName = "Special",
                    FilePrefix = "SpecialItemSO"
                };

            // ───────────────── 도구 아이템 ─────────────────
            case EType.SickleItem:
            case EType.ShovelItem:
            case EType.AxeItem:
            case EType.PickaxeItem:
            case EType.WateringCan:
            case EType.Fishingrod:
                return new AssetTarget
                {
                    AssetType = typeof(ToolItemSO),
                    FolderName = "Tool",
                    FilePrefix = "ToolItemSO"
                };

            default:
                throw new InvalidOperationException($"지원하지 않는 TYPE 입니다: {type}");
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 : SO 생성 / 업데이트 ◀─────────────────────────
    /// <summary>
    /// RowData를 실제 SO 에셋에 반영한다.
    ///
    /// 동작 순서
    /// 1) TYPE으로 SO 종류 / 폴더 결정
    /// 2) 폴더 생성
    /// 3) 같은 ID의 에셋이 있으면 업데이트
    /// 4) 없으면 새로 생성
    /// 5) 공통 필드 반영
    /// 6) 타입별 추가 필드 반영
    /// </summary>
    private static bool CreateOrUpdateAsset(
        StaticItemSheetImportSettingsSO settings,
        RowData row,
        Dictionary<string, Sprite> spriteMap)
    {
        AssetTarget target = ResolveAssetTarget(row.Type);

        // OutputFolder는 "루트"라고 생각하면 된다.
        // 예: Assets/Resources/ScriptableObjects/Item
        // 이 아래에 Harvest / Fish / Feed ... 를 붙인다.
        string folderPath = $"{settings.OutputFolder}/{target.FolderName}";
        EnsureFolderExists(folderPath);

        string assetPath = $"{folderPath}/{target.FilePrefix}_{SanitizeFileName(row.Id)}.asset";

        // 타입별로 에셋 로드
        ItemSO asset = AssetDatabase.LoadAssetAtPath(assetPath, target.AssetType) as ItemSO;
        bool isCreated = false;

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance(target.AssetType) as ItemSO;
            AssetDatabase.CreateAsset(asset, assetPath);
            isCreated = true;
        }

        ApplyCommonDataToAsset(asset, row, spriteMap, settings.OverwriteImageWhenSpriteFound);
        ApplySpecificDataToAsset(asset, row);

        EditorUtility.SetDirty(asset);
        return isCreated;
    }

    /// <summary>
    /// 모든 ItemSO가 공통으로 가지는 필드 반영.
    /// BaseSO / UnitSO / ItemSO에 들어있는 값들을 여기서 한 번에 넣는다.
    /// </summary>
    private static void ApplyCommonDataToAsset(
        ItemSO asset,
        RowData row,
        Dictionary<string, Sprite> spriteMap,
        bool overwriteImageWhenSpriteFound)
    {
        SerializedObject so = new SerializedObject(asset);

        // BaseSO
        SetStringIfExists(so, "_id", row.Id);
        SetEnumIfExists(so, "_type", (int)row.Type);

        // UnitSO
        SetStringIfExists(so, "_name", row.Name);
        SetStringIfExists(so, "_description", row.Description);
        SetEnumIfExists(so, "_rarity", (int)row.Rarity);

        // ItemSO
        SetIntIfExists(so, "_buyPrice", row.BuyPrice);
        SetIntIfExists(so, "_sellPrice", row.SellPrice);
        SetIntIfExists(so, "_maxStack", row.MaxStack);

        // 아이콘 자동 연결
        if (overwriteImageWhenSpriteFound)
        {
            Sprite sprite = FindSprite(spriteMap, row.SpriteKey, row.Id);
            if (sprite != null)
            {
                SetObjectIfExists(so, "_image", sprite);
            }
            else
            {
                Debug.LogWarning($"[StaticItemSheetImporter] 스프라이트를 찾지 못했습니다. ID={row.Id}, SpriteKey={row.SpriteKey}");
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>
    /// 타입별 추가 필드 반영.
    /// 현재 시트에 전용 컬럼이 없으면 기본값 그대로 남는다.
    /// 즉, 1차는 공통 필드만으로도 생성되고,
    /// 2차에서 전용 컬럼을 채우면 더 풍부하게 확장 가능하다.
    /// </summary>
    private static void ApplySpecificDataToAsset(ItemSO asset, RowData row)
    {
        SerializedObject so = new SerializedObject(asset);

        // ───────────────── SeedItemSO ─────────────────
        if (asset is SeedItemSO)
        {
            SetStringIfExists(so, "_placeCropId", row.PlaceCropId);
            SetIntIfExists(so, "_needFarmingLevel", row.NeedFarmingLevel);
        }

        // ───────────────── ToolItemSO ─────────────────
        if (asset is ToolItemSO)
        {
            SetIntIfExists(so, "_maxDurability", row.MaxDurability);
            SetIntIfExists(so, "_currentLv", row.CurrentLv);
            SetFloatIfExists(so, "_range", row.Range);
            SetFloatIfExists(so, "_strength", row.Strength);
        }

        // ───────────────── BaitItemSO ─────────────────
        if (asset is BaitItemSO)
        {
            SetBoolIfExists(so, "_canUseSea", row.CanUseSea);

            // 낚시 희귀도 배열
            SetEnumArrayIfExists(so, "_catchFishRarity", row.CatchFishRarities);

            // 낚시 가중치 배열
            SetFloatArrayIfExists(so, "_catchFishWeight", row.CatchFishWeights);
        }

        // ───────────────── SpecialItemSO ─────────────────
        // 현재 팀 스크립트 구조를 정확히 모르는 필드는 "있으면 넣는 방식"으로 처리
        if (asset is SpecialItemSO)
        {
            SetIntIfExists(so, "_maxUseCount", row.MaxUseCount);
        }

        // StaticItemSO / FeedItemSO는 현재 추가 필드 없음
        so.ApplyModifiedPropertiesWithoutUndo();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 : SerializedProperty 유틸 ◀─────────────────────────
    /// <summary>
    /// SerializedObject에 해당 이름의 프로퍼티가 있으면 문자열 값을 넣는다.
    /// 없으면 조용히 무시한다.
    /// 팀 스크립트 구조가 조금 바뀌어도 덜 깨지게 하려고 만든 방어 코드.
    /// </summary>
    private static void SetStringIfExists(SerializedObject so, string propertyName, string value)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null && prop.propertyType == SerializedPropertyType.String)
        {
            prop.stringValue = value ?? string.Empty;
        }
    }

    private static void SetIntIfExists(SerializedObject so, string propertyName, int value)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null && prop.propertyType == SerializedPropertyType.Integer)
        {
            prop.intValue = value;
        }
    }

    private static void SetFloatIfExists(SerializedObject so, string propertyName, float value)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null && prop.propertyType == SerializedPropertyType.Float)
        {
            prop.floatValue = value;
        }
    }

    private static void SetBoolIfExists(SerializedObject so, string propertyName, bool value)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null && prop.propertyType == SerializedPropertyType.Boolean)
        {
            prop.boolValue = value;
        }
    }

    private static void SetEnumIfExists(SerializedObject so, string propertyName, int enumIndex)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null && prop.propertyType == SerializedPropertyType.Enum)
        {
            prop.enumValueIndex = enumIndex;
        }
    }

    private static void SetObjectIfExists(SerializedObject so, string propertyName, UnityEngine.Object value)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
        {
            prop.objectReferenceValue = value;
        }
    }

    /// <summary>
    /// enum 배열 프로퍼티에 값 반영.
    /// 예: _catchFishRarity
    /// </summary>
    private static void SetEnumArrayIfExists(SerializedObject so, string propertyName, List<ERarity> values)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null || prop.isArray == false)
            return;

        prop.arraySize = values.Count;
        for (int i = 0; i < values.Count; ++i)
        {
            SerializedProperty element = prop.GetArrayElementAtIndex(i);
            if (element != null && element.propertyType == SerializedPropertyType.Enum)
            {
                element.enumValueIndex = (int)values[i];
            }
        }
    }

    /// <summary>
    /// float 배열 프로퍼티에 값 반영.
    /// 예: _catchFishWeight
    /// </summary>
    private static void SetFloatArrayIfExists(SerializedObject so, string propertyName, List<float> values)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null || prop.isArray == false)
            return;

        prop.arraySize = values.Count;
        for (int i = 0; i < values.Count; ++i)
        {
            SerializedProperty element = prop.GetArrayElementAtIndex(i);
            if (element != null && element.propertyType == SerializedPropertyType.Float)
            {
                element.floatValue = values[i];
            }
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 : 스프라이트 처리 ◀─────────────────────────
    /// <summary>
    /// 지정한 폴더들 아래의 모든 Sprite를 한 번에 모아서 딕셔너리로 만든다.
    /// 키는 비교 편하게 NormalizeKey()로 정규화한 이름.
    /// </summary>
    private static Dictionary<string, Sprite> BuildSpriteMap(string[] folders)
    {
        Dictionary<string, Sprite> map = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

        if (folders == null || folders.Length == 0)
        {
            return map;
        }

        List<string> validFolders = new List<string>();
        foreach (string folder in folders)
        {
            if (string.IsNullOrWhiteSpace(folder))
                continue;

            if (AssetDatabase.IsValidFolder(folder))
            {
                validFolders.Add(folder);
            }
            else
            {
                Debug.LogWarning($"[StaticItemSheetImporter] 잘못된 스프라이트 폴더 경로: {folder}");
            }
        }

        if (validFolders.Count == 0)
            return map;

        string[] guids = AssetDatabase.FindAssets("t:Sprite", validFolders.ToArray());
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                continue;

            string normalizedName = NormalizeKey(sprite.name);
            if (map.ContainsKey(normalizedName) == false)
            {
                map.Add(normalizedName, sprite);
            }
        }

        return map;
    }

    /// <summary>
    /// spriteKey -> id 순서로 스프라이트를 찾는다.
    /// spriteKey를 별도로 둔 이유는
    /// 시트 ID와 실제 이미지 파일 이름이 다를 수 있기 때문.
    /// </summary>
    private static Sprite FindSprite(Dictionary<string, Sprite> spriteMap, string spriteKey, string id)
    {
        if (spriteMap == null || spriteMap.Count == 0)
            return null;

        string key1 = NormalizeKey(spriteKey);
        if (spriteMap.TryGetValue(key1, out Sprite sprite))
            return sprite;

        string key2 = NormalizeKey(id);
        if (spriteMap.TryGetValue(key2, out sprite))
            return sprite;

        return null;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 : 헤더 / 셀 유틸 ◀─────────────────────────
    private static Dictionary<string, int> BuildHeaderMap(string headerLine)
    {
        string[] headers = headerLine.Split('\t');
        Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < headers.Length; ++i)
        {
            string key = headers[i].Trim();
            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (map.ContainsKey(key) == false)
            {
                map.Add(key, i);
            }
        }

        return map;
    }

    private static void RequireHeader(Dictionary<string, int> headerMap, string key)
    {
        if (headerMap.ContainsKey(key) == false)
        {
            throw new InvalidOperationException($"필수 헤더 누락: {key}");
        }
    }

    private static string GetCell(string[] cells, Dictionary<string, int> headerMap, string key)
    {
        if (headerMap.TryGetValue(key, out int index) == false)
            return string.Empty;

        if (index < 0 || index >= cells.Length)
            return string.Empty;

        return cells[index];
    }

    private static string GetOptionalCell(string[] cells, Dictionary<string, int> headerMap, string key)
    {
        return GetCell(cells, headerMap, key);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 : 파싱 유틸 ◀─────────────────────────
    /// <summary>
    /// 셀 문자열 정리.
    /// - 탭 제거
    /// - 양끝 공백 제거
    /// - null 방어
    /// </summary>
    private static string NormalizeCell(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Replace("\r", string.Empty).Replace("\t", " ").Trim();
    }

    /// <summary>
    /// 여러 줄 설명용 정리.
    /// </summary>
    private static string NormalizeMultiline(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
    }

    /// <summary>
    /// 스프라이트 / 키 비교용 문자열 정규화.
    /// 공백, 언더바, 하이픈 차이를 줄인다.
    /// </summary>
    private static string NormalizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value
            .Trim()
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .ToLowerInvariant();
    }

    /// <summary>
    /// TYPE 문자열 -> EType 변환.
    /// enum 이름과 완전히 같아도 되고,
    /// 흔한 공백 / 대소문자 차이는 어느 정도 허용한다.
    /// </summary>
    private static bool TryParseType(string text, out EType type)
    {
        type = default;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        // 1차 : 그대로 파싱
        if (Enum.TryParse(text, true, out type))
            return true;

        // 2차 : 공백 제거 후 비교
        string normalized = NormalizeKey(text);

        foreach (EType candidate in Enum.GetValues(typeof(EType)))
        {
            if (NormalizeKey(candidate.ToString()) == normalized)
            {
                type = candidate;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// RARITY 문자열 -> ERarity 변환.
    /// 흔한 오타(Masterwark)도 Masterwork로 보정한다.
    /// </summary>
    private static bool TryParseRarity(string text, out ERarity rarity)
    {
        rarity = default;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        string normalized = NormalizeKey(text);

        // 흔한 오타 방어
        if (normalized == "masterwark")
            normalized = "masterwork";

        foreach (ERarity candidate in Enum.GetValues(typeof(ERarity)))
        {
            if (NormalizeKey(candidate.ToString()) == normalized)
            {
                rarity = candidate;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 숫자 파싱.
    /// 빈칸, "/", "-", "x", "X" 같은 값은 0으로 처리한다.
    /// 시트에 기호를 섞어 넣어도 최대한 덜 터지게 만든 방어 코드.
    /// </summary>
    private static int ParseIntSafe(string text, int defaultValue = 0)
    {
        text = NormalizeCell(text);

        if (string.IsNullOrWhiteSpace(text))
            return defaultValue;

        if (text == "/" || text == "-" || text.Equals("x", StringComparison.OrdinalIgnoreCase))
            return defaultValue;

        text = text.Replace(",", string.Empty);

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
            return intValue;

        if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
            return Mathf.RoundToInt(floatValue);

        return defaultValue;
    }

    private static float ParseFloatSafe(string text, float defaultValue = 0f)
    {
        text = NormalizeCell(text);

        if (string.IsNullOrWhiteSpace(text))
            return defaultValue;

        if (text == "/" || text == "-" || text.Equals("x", StringComparison.OrdinalIgnoreCase))
            return defaultValue;

        text = text.Replace(",", string.Empty);

        if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            return value;

        return defaultValue;
    }

    private static bool ParseBoolSafe(string text)
    {
        text = NormalizeKey(text);

        return text == "1" || text == "true" || text == "yes" || text == "y";
    }

    /// <summary>
    /// "Basic, Prime, Masterwork" 같은 문자열을 ERarity 리스트로 바꾼다.
    /// BaitItemSO의 낚시 가능 희귀도 배열 같은 곳에 쓸 수 있다.
    /// </summary>
    private static void ParseRarityList(string text, List<ERarity> result)
    {
        result.Clear();

        text = NormalizeCell(text);
        if (string.IsNullOrWhiteSpace(text))
            return;

        string[] parts = text.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string raw in parts)
        {
            if (TryParseRarity(raw, out ERarity rarity))
            {
                result.Add(rarity);
            }
        }
    }

    /// <summary>
    /// "1, 2, 3.5" 같은 문자열을 float 리스트로 바꾼다.
    /// </summary>
    private static void ParseFloatList(string text, List<float> result)
    {
        result.Clear();

        text = NormalizeCell(text);
        if (string.IsNullOrWhiteSpace(text))
            return;

        string[] parts = text.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string raw in parts)
        {
            float value = ParseFloatSafe(raw, float.NaN);
            if (float.IsNaN(value) == false)
            {
                result.Add(value);
            }
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 : 파일 / 폴더 유틸 ◀─────────────────────────
    /// <summary>
    /// 파일 이름으로 쓰기 안전하게 정리.
    /// </summary>
    private static string SanitizeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Empty";

        foreach (char c in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(c.ToString(), "_");
        }

        return value.Replace(" ", "_");
    }

    /// <summary>
    /// Assets/... 경로가 실제로 없으면 생성한다.
    /// 루트부터 한 단계씩 만들어 올라간다.
    /// </summary>
    private static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string[] parts = folderPath.Split('/');
        if (parts.Length < 2 || parts[0] != "Assets")
        {
            throw new InvalidOperationException($"폴더 경로가 올바르지 않습니다: {folderPath}");
        }

        string current = "Assets";
        for (int i = 1; i < parts.Length; ++i)
        {
            string next = $"{current}/{parts[i]}";
            if (AssetDatabase.IsValidFolder(next) == false)
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }
    #endregion
}
