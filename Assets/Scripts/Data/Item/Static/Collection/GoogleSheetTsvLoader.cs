using System;
using UnityEngine;

/// <summary>
/// 구글 시트 TSV 문자열을 읽어서 SheetItemDatabase에 등록하는 로더.
/// 
/// 이 스크립트의 역할
/// 1. TextAsset 또는 문자열(TextArea)로 받은 TSV 원본을 읽는다.
/// 2. Animal / Fish / Gather 섹션을 구분한다.
/// 3. 각 행을 SheetItemRow로 변환한다.
/// 4. SheetItemDatabase에 등록한다.
/// 
/// 현재 지원하는 기본 구조
/// 
/// Animal
/// ID	NAME	등급	판매 금액	설명
/// ...
/// 
/// Fish
/// ID	NAME	등급	판매 금액	설명	물	깊은물
/// ...
/// 
/// Gather
/// ID	NAME	등급	판매 금액	설명
/// ...
/// 
/// 중요
/// - category는 Animal / Fish / Gather 로 저장된다.
/// - Fish는 물 / 깊은물 컬럼까지 읽어서 isWaterFish / isDeepWaterFish에 저장한다.
/// - 이 스크립트는 데이터 적재만 담당한다.
/// - 실제 낚시 / 도감 / UI는 SheetItemDatabase를 참조해서 사용한다.
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

        // [수정 포인트 1]
        // 인스펙터에서 DB를 연결하지 않았으면 같은 오브젝트에서 자동 탐색
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
        bool waitHeaderLine = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string trimmed = line.Trim();

            // [수정 포인트 2]
            // 섹션 헤더 감지
            if (IsCategoryHeader(trimmed))
            {
                currentCategory = trimmed;
                waitHeaderLine = true;
                continue;
            }

            // 섹션 바로 아래의 헤더 줄(ID, NAME, 등급...)은 건너뜀
            if (waitHeaderLine)
            {
                waitHeaderLine = false;
                continue;
            }

            // category를 아직 못 찾았으면 데이터로 보지 않음
            if (string.IsNullOrWhiteSpace(currentCategory))
            {
                continue;
            }

            ParseDataLine(currentCategory, line, i + 1);
        }
    }

    /// <summary>
    /// 한 줄의 데이터 행을 파싱해서 SheetItemDatabase에 등록
    /// </summary>
    private void ParseDataLine(string category, string line, int lineNumber)
    {
        string[] cols = line.Split('\t');

        // 최소 ID, NAME은 있어야 의미 있는 데이터로 본다.
        if (cols == null || cols.Length < 2)
        {
            if (_logEnabled)
            {
                Debug.LogWarning($"[GoogleSheetTsvLoader] 컬럼 수 부족으로 스킵. line={lineNumber}, raw={line}");
            }
            return;
        }

        string id = SafeGet(cols, 0);
        string name = SafeGet(cols, 1);
        string rarity = SafeGet(cols, 2);

        // iconKey는 우선 itemId와 동일하게 사용
        string iconKey = id;

        if (string.IsNullOrWhiteSpace(id))
        {
            if (_logEnabled)
            {
                Debug.LogWarning($"[GoogleSheetTsvLoader] ID가 비어 있어 스킵. line={lineNumber}");
            }
            return;
        }

        bool isWaterFish = false;
        bool isDeepWaterFish = false;

        // [수정 포인트 3]
        // Fish 카테고리일 때만 물 / 깊은물 컬럼 읽기
        if (string.Equals(category, "Fish", StringComparison.Ordinal))
        {
            isWaterFish = ParseBool01(SafeGet(cols, 5));
            isDeepWaterFish = ParseBool01(SafeGet(cols, 6));
        }

        SheetItemRow row = new SheetItemRow(
            id,
            name,
            category,
            rarity,
            iconKey,
            isWaterFish,
            isDeepWaterFish
        );

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
    /// "1" 이면 true, 나머지는 false
    /// Fish 시트의 물 / 깊은물 플래그용
    /// </summary>
    private bool ParseBool01(string value)
    {
        return value == "1";
    }
    #endregion
}
