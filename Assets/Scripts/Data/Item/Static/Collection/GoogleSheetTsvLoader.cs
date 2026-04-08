using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 구글 시트 TSV 문자열을 읽어서 SheetItemDatabase에 등록하는 로더.
/// 
/// 이 스크립트의 역할
/// 1. TextAsset 또는 문자열(TextArea)로 받은 TSV 원본을 읽는다.
/// 2. Animal / Fish / Gather 섹션을 구분한다.
/// 3. 각 섹션의 헤더를 읽고 컬럼 위치를 자동으로 매핑한다.
/// 4. 각 행을 SheetItemRow로 변환한다.
/// 5. SheetItemDatabase에 등록한다.
/// 
/// 지원 컬럼 예시
/// - ID
/// - NAME
/// - 등급
/// - 판매 금액 / 판매금액
/// - 구매 금액 / 구매금액
/// - 설명
/// - 물
/// - 깊은물
/// 
/// 중요
/// - category는 Animal / Fish / Gather 로 저장된다.
/// - Fish는 물 / 깊은물 컬럼까지 읽어서 isWaterFish / isDeepWaterFish에 저장한다.
/// - 헤더 기반 파싱이므로 컬럼 순서가 바뀌어도 비교적 안전하다.
/// - 이 스크립트는 데이터 적재만 담당한다.
/// </summary>
public class GoogleSheetTsvLoader : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("TSV 원본")]
    [Tooltip("TextAsset으로 직접 넣을 TSV 파일")]
    [SerializeField] private TextAsset _tsvTextAsset;

    [Tooltip("직접 붙여넣는 TSV 문자열. TextAsset이 없을 때 사용")]
    [TextArea(10, 40)]
    [SerializeField] private string _tsvRawText;

    [Header("대상 DB")]
    [Tooltip("파싱 결과를 저장할 런타임 DB")]
    [SerializeField] private SheetItemDatabase _database;

    [Header("옵션")]
    [Tooltip("Awake에서 자동 로드할지")]
    [SerializeField] private bool _loadOnAwake = true;

    [Tooltip("기존 DB 내용을 비우고 다시 로드할지")]
    [SerializeField] private bool _clearBeforeLoad = true;

    [Tooltip("로그 출력 여부")]
    [SerializeField] private bool _logEnabled = true;
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        if (_database == null)
        {
            _database = GetComponent<SheetItemDatabase>();
        }

        if (_loadOnAwake)
        {
            Load();
        }
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 현재 설정된 TextAsset 또는 RawText를 기준으로 DB를 로드한다.
    /// </summary>
    public void Load()
    {
        if (_database == null)
        {
            Debug.LogError("[GoogleSheetTsvLoader] SheetItemDatabase가 연결되지 않았습니다.");
            return;
        }

        string sourceText = GetSourceText();

        if (string.IsNullOrWhiteSpace(sourceText))
        {
            Debug.LogWarning("[GoogleSheetTsvLoader] TSV 원본이 비어 있습니다.");
            return;
        }

        if (_clearBeforeLoad)
        {
            _database.Clear();
        }

        ParseTsv(sourceText);

        if (_logEnabled)
        {
            Debug.Log($"[GoogleSheetTsvLoader] TSV 로드 완료. 총 {_database.AllItems.Count}개 등록");
        }
    }

    /// <summary>
    /// 외부에서 문자열을 직접 넘겨 즉시 다시 로드하고 싶을 때 사용
    /// </summary>
    public void LoadFromString(string rawText)
    {
        _tsvRawText = rawText;
        Load();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 사용할 원본 텍스트를 결정한다.
    /// 우선순위
    /// 1. TextAsset
    /// 2. RawText
    /// </summary>
    private string GetSourceText()
    {
        if (_tsvTextAsset != null && string.IsNullOrWhiteSpace(_tsvTextAsset.text) == false)
        {
            return _tsvTextAsset.text;
        }

        return _tsvRawText;
    }

    /// <summary>
    /// TSV 전체를 줄 단위로 파싱
    /// </summary>
    private void ParseTsv(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return;
        }

        string normalized = raw.Replace("\r\n", "\n").Replace('\r', '\n');
        string[] lines = normalized.Split('\n');

        string currentCategory = string.Empty;
        Dictionary<string, int> currentHeaderMap = null;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string trimmed = line.Trim();

            if (IsCategoryHeader(trimmed))
            {
                currentCategory = trimmed;
                currentHeaderMap = null;
                continue;
            }

            if (string.IsNullOrWhiteSpace(currentCategory))
            {
                continue;
            }

            if (currentHeaderMap == null)
            {
                currentHeaderMap = BuildHeaderMap(line);

                if (_logEnabled)
                {
                    Debug.Log($"[GoogleSheetTsvLoader] 헤더 감지. category={currentCategory}, headerCount={currentHeaderMap.Count}");
                }

                continue;
            }

            ParseDataLine(currentCategory, line, currentHeaderMap, i + 1);
        }
    }

    /// <summary>
    /// 헤더 한 줄을 읽어서 컬럼명 -> 인덱스 맵을 만든다.
    /// </summary>
    private Dictionary<string, int> BuildHeaderMap(string headerLine)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return result;
        }

        string[] cols = headerLine.Split('\t');

        for (int i = 0; i < cols.Length; i++)
        {
            string normalizedHeader = NormalizeHeader(cols[i]);

            if (string.IsNullOrWhiteSpace(normalizedHeader))
            {
                continue;
            }

            if (result.ContainsKey(normalizedHeader))
            {
                continue;
            }

            result.Add(normalizedHeader, i);
        }

        return result;
    }

    /// <summary>
    /// 한 줄의 데이터 행을 파싱해서 SheetItemDatabase에 등록
    /// </summary>
    private void ParseDataLine(string category, string line, Dictionary<string, int> headerMap, int lineNumber)
    {
        string[] cols = line.Split('\t');

        if (cols == null || cols.Length < 2)
        {
            if (_logEnabled)
            {
                Debug.LogWarning($"[GoogleSheetTsvLoader] 컬럼 수 부족으로 스킵. line={lineNumber}, raw={line}");
            }
            return;
        }

        string id = FirstNonEmpty(
            GetByHeaders(cols, headerMap, "ID", "ItemID"),
            SafeGet(cols, 0));

        string name = FirstNonEmpty(
            GetByHeaders(cols, headerMap, "NAME", "이름"),
            SafeGet(cols, 1));

        string rarity = FirstNonEmpty(
            GetByHeaders(cols, headerMap, "등급", "Rarity"),
            SafeGet(cols, 2));

        string description = FirstNonEmpty(
            GetByHeaders(cols, headerMap, "설명", "Description", "Desc"),
            SafeGet(cols, 4));

        string sellPriceRaw = FirstNonEmpty(
            GetByHeaders(cols, headerMap, "판매 금액", "판매금액", "SellPrice", "Sell"),
            SafeGet(cols, 3));

        string buyPriceRaw = GetByHeaders(cols, headerMap, "구매 금액", "구매금액", "BuyPrice", "Buy");

        string iconKey = FirstNonEmpty(
            GetByHeaders(cols, headerMap, "아이콘", "IconKey", "Icon"),
            id);

        if (string.IsNullOrWhiteSpace(id))
        {
            if (_logEnabled)
            {
                Debug.LogWarning($"[GoogleSheetTsvLoader] ID가 비어 있어 스킵. line={lineNumber}");
            }
            return;
        }

        int sellPrice = ParseIntSafe(sellPriceRaw, -1);
        int buyPrice = ParseIntSafe(buyPriceRaw, -1);

        bool isWaterFish = false;
        bool isDeepWaterFish = false;

        if (string.Equals(category, "Fish", StringComparison.OrdinalIgnoreCase))
        {
            string waterRaw = FirstNonEmpty(
                GetByHeaders(cols, headerMap, "물", "담수", "Water"),
                SafeGet(cols, 5));

            string deepWaterRaw = FirstNonEmpty(
                GetByHeaders(cols, headerMap, "깊은물", "바다", "Sea", "DeepWater"),
                SafeGet(cols, 6));

            isWaterFish = ParseBoolFlexible(waterRaw);
            isDeepWaterFish = ParseBoolFlexible(deepWaterRaw);
        }

        SheetItemRow row = new SheetItemRow(
            id,
            name,
            category,
            rarity,
            description,
            sellPrice,
            buyPrice,
            iconKey,
            isWaterFish,
            isDeepWaterFish);

        bool added = _database.AddRow(row);

        if (_logEnabled && !added)
        {
            Debug.LogWarning($"[GoogleSheetTsvLoader] DB 등록 실패 또는 중복. id={id}, line={lineNumber}");
        }
    }

    /// <summary>
    /// 현재 줄이 Animal / Fish / Gather 섹션 헤더인지 검사
    /// </summary>
    private bool IsCategoryHeader(string value)
    {
        return value == "Animal" ||
               value == "Fish" ||
               value == "Gather";
    }

    /// <summary>
    /// 후보 헤더명을 순서대로 검사해서 첫 번째 값을 반환한다.
    /// </summary>
    private string GetByHeaders(string[] cols, Dictionary<string, int> headerMap, params string[] headerCandidates)
    {
        if (cols == null || headerMap == null || headerCandidates == null)
        {
            return string.Empty;
        }

        for (int i = 0; i < headerCandidates.Length; i++)
        {
            string key = NormalizeHeader(headerCandidates[i]);

            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            if (headerMap.TryGetValue(key, out int index))
            {
                return SafeGet(cols, index);
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 컬럼명을 비교하기 좋은 형태로 정규화
    /// </summary>
    private string NormalizeHeader(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string normalized = value.Trim();
        normalized = normalized.Replace(" ", string.Empty);
        normalized = normalized.Replace("_", string.Empty);
        normalized = normalized.Replace("-", string.Empty);
        normalized = normalized.Replace("\uFEFF", string.Empty);
        return normalized.ToLowerInvariant();
    }

    /// <summary>
    /// 여러 값 중 첫 번째 유효한 문자열을 반환
    /// </summary>
    private string FirstNonEmpty(params string[] values)
    {
        if (values == null)
        {
            return string.Empty;
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(values[i]) == false)
            {
                return values[i].Trim();
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 컬럼 안전 접근
    /// </summary>
    private string SafeGet(string[] cols, int index)
    {
        if (cols == null)
        {
            return string.Empty;
        }

        if (index < 0 || index >= cols.Length)
        {
            return string.Empty;
        }

        return cols[index] == null ? string.Empty : cols[index].Trim();
    }

    /// <summary>
    /// 정수 파싱
    /// 실패 시 defaultValue 반환
    /// </summary>
    private int ParseIntSafe(string value, int defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (int.TryParse(value.Trim(), out int result))
        {
            return result;
        }

        return defaultValue;
    }

    /// <summary>
    /// 1 / true / y / yes 를 true 로 처리
    /// </summary>
    private bool ParseBoolFlexible(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string normalized = value.Trim().ToLowerInvariant();

        return normalized == "1" ||
               normalized == "true" ||
               normalized == "y" ||
               normalized == "yes";
    }
    #endregion
}
