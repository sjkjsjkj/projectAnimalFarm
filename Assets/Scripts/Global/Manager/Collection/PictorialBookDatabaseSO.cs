using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도감 표시 전용 데이터베이스.
/// 기존 ItemSO 에셋들을 스캔해서 도감용 엔트리 목록으로 저장한다.
/// </summary>
[CreateAssetMenu(fileName = "PictorialBookDatabase", menuName = "ScriptableObjects/Collection/Pictorial Book Database", order = 10)]
public class PictorialBookDatabaseSO : ScriptableObject
{
    [Header("빌드용 소스 폴더")]
    [SerializeField] private string[] _animalSourceFolders;
    [SerializeField] private string[] _fishSourceFolders;
    [SerializeField] private string[] _gatherSourceFolders;

    [Header("생성된 도감 엔트리")]
    [SerializeField] private List<PictorialBookEntry> _entries = new List<PictorialBookEntry>();

    public List<PictorialBookEntry> Entries => _entries;

    public List<PictorialBookEntry> GetEntriesByCategory(string category)
    {
        List<PictorialBookEntry> result = new List<PictorialBookEntry>();

        if (_entries == null || _entries.Count == 0)
        {
            return result;
        }

        string normalizedCategory = NormalizeText(category);

        for (int i = 0; i < _entries.Count; i++)
        {
            PictorialBookEntry entry = _entries[i];
            if (entry == null)
            {
                continue;
            }

            if (string.Equals(NormalizeText(entry.category), normalizedCategory, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(entry);
            }
        }

        return result;
    }

    public void SetEntries(List<PictorialBookEntry> entries)
    {
        _entries = entries ?? new List<PictorialBookEntry>();
    }

    public string[] AnimalSourceFolders => _animalSourceFolders;
    public string[] FishSourceFolders => _fishSourceFolders;
    public string[] GatherSourceFolders => _gatherSourceFolders;

    private string NormalizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Trim();
    }
}

[Serializable]
public class PictorialBookEntry
{
    public string itemId;
    public string displayName;
    public Sprite icon;
    public string category;
    public string assetPath;
}
