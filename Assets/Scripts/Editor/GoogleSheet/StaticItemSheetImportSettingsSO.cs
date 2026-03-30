using UnityEngine;

[CreateAssetMenu(
    fileName = "StaticItemSheetImportSettings",
    menuName = "ScriptableObjects/GoogleSheet/Static Item Import Settings",
    order = 1)]
public class StaticItemSheetImportSettingsSO : ScriptableObject
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("구글 시트 정보")]
    [SerializeField] private string _sheetName = "ITEM_STATIC";
    [SerializeField][TextArea(2, 4)] private string _tsvUrl = string.Empty;

    [Header("출력 설정")]
    [SerializeField] private string _outputFolder = "Assets/Resources/ScriptableObject/Item/Static";
    [SerializeField] private bool _overwriteImageWhenSpriteFound = true;

    [Header("스프라이트 검색 폴더")]
    [SerializeField] private string[] _spriteSearchFolders;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public string SheetName => _sheetName;
    public string TsvUrl => _tsvUrl;
    public string OutputFolder => _outputFolder;
    public bool OverwriteImageWhenSpriteFound => _overwriteImageWhenSpriteFound;
    public string[] SpriteSearchFolders => _spriteSearchFolders;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public bool IsValid(out string reason)
    {
        if (string.IsNullOrWhiteSpace(_sheetName))
        {
            reason = "SheetName이 비어 있습니다.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(_tsvUrl))
        {
            reason = "TSV URL이 비어 있습니다.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(_outputFolder))
        {
            reason = "OutputFolder가 비어 있습니다.";
            return false;
        }

        if (_outputFolder.StartsWith("Assets/") == false)
        {
            reason = "OutputFolder는 Assets/ 로 시작해야 합니다.";
            return false;
        }

        reason = string.Empty;
        return true;
    }
    #endregion
}
