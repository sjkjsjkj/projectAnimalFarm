using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 구글시트 TSV 데이터를 읽어서 SheetItemDatabase에 넣는 로더
/// 시트 구조가 달라도 최소한 ID / NAME은 공통으로 처리한다.
/// </summary>
public class GoogleSheetTsvLoader : MonoBehaviour
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("대상 DB")]
    [SerializeField] private SheetItemDatabase _database;

    [Header("Gather 시트 TSV URL")]
    [SerializeField] private string _gatherSheetUrl;

    [Header("Animal 시트 TSV URL")]
    [SerializeField] private string _animalSheetUrl;

    [Header("Fish 시트 TSV URL")]
    [SerializeField] private string _fishSheetUrl;

    [Header("자동 로드 여부")]
    [SerializeField] private bool _loadOnStart = true;

    [Header("로그 출력")]
    [SerializeField] private bool _logEnabled = true;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isLoaded = false;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool IsLoaded => _isLoaded;
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private IEnumerator Start()
    {
        if (_loadOnStart)
        {
            yield return LoadAllSheets();
        }
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public IEnumerator LoadAllSheets()
    {
        if (_database == null)
        {
            Debug.LogError("[GoogleSheetTsvLoader] SheetItemDatabase가 연결되지 않았습니다.");
            yield break;
        }

        _database.Clear();
        _isLoaded = false;

        yield return LoadSheet(_gatherSheetUrl, "Gather");
        yield return LoadSheet(_animalSheetUrl, "Animal");
        yield return LoadSheet(_fishSheetUrl, "Fish");

        _isLoaded = true;

        if (_logEnabled)
        {
            Debug.Log($"[GoogleSheetTsvLoader] 모든 시트 로드 완료. 총 {_database.AllItems.Count}개");
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private IEnumerator LoadSheet(string url, string category)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            Debug.LogWarning($"[GoogleSheetTsvLoader] {category} 시트 URL이 비어 있습니다.");
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool hasError = www.result != UnityWebRequest.Result.Success;
#else
            bool hasError = www.isNetworkError || www.isHttpError;
#endif

            if (hasError)
            {
                Debug.LogError($"[GoogleSheetTsvLoader] {category} 시트 로드 실패: {www.error}");
                yield break;
            }

            string tsvText = www.downloadHandler.text;

            if (string.IsNullOrWhiteSpace(tsvText))
            {
                Debug.LogWarning($"[GoogleSheetTsvLoader] {category} 시트 내용이 비어 있습니다.");
                yield break;
            }

            ParseTsv(tsvText, category);

            if (_logEnabled)
            {
                Debug.Log($"[GoogleSheetTsvLoader] {category} 시트 파싱 완료");
            }
        }
    }

    private void ParseTsv(string tsvText, string category)
    {
        string[] lines = tsvText.Split('\n');

        // 첫 줄은 헤더
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd('\r');

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] cols = line.Split('\t');

            // 최소한 ID, NAME은 있어야 함
            if (cols.Length < 2)
            {
                Debug.LogWarning($"[GoogleSheetTsvLoader] {category} 시트 {i + 1}번째 줄 컬럼 수 부족");
                continue;
            }

            string id = SafeGet(cols, 0);
            string name = SafeGet(cols, 1);

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
            {
                Debug.LogWarning($"[GoogleSheetTsvLoader] {category} 시트 {i + 1}번째 줄 ID 또는 NAME 비어 있음");
                continue;
            }

            string rarity = "Basic";
            string iconKey = id;

            bool isWaterFish = false;
            bool isDeepWaterFish = false;

            // 시트마다 구조가 다르므로 category별 처리
            switch (category)
            {
                case "Gather":
                    // ID, NAME, 등급
                    rarity = SafeGet(cols, 2, "Basic");
                    break;

                case "Animal":
                    // ID, NAME
                    rarity = "Basic";
                    break;

                case "Fish":
                    // ID, NAME, 물, 깊은물, 등급
                    isWaterFish = SafeGet(cols, 2, "0") == "1";
                    isDeepWaterFish = SafeGet(cols, 3, "0") == "1";
                    rarity = SafeGet(cols, 4, "Basic");
                    break;
            }

            SheetItemRow row = new SheetItemRow(
                id: id,
                name: name,
                category: category,
                rarity: rarity,
                iconKey: iconKey,
                isWaterFish: isWaterFish,
                isDeepWaterFish: isDeepWaterFish
            );

            _database.TryAddRow(row);
        }
    }

    private string SafeGet(string[] cols, int index, string defaultValue = "")
    {
        if (cols == null)
        {
            return defaultValue;
        }

        if (index < 0 || index >= cols.Length)
        {
            return defaultValue;
        }

        return cols[index].Trim();
    }
    #endregion
}
