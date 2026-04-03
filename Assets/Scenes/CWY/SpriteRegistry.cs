using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 ID 또는 IconKey와 실제 스프라이트를 연결하는 레지스트리
/// 인스펙터에서 직접 매칭해두면 UI에서 쉽게 가져올 수 있다.
/// </summary>
public class SpriteRegistry : BaseMono
{
    [SerializeField] private List<SpriteEntry> _entries = new List<SpriteEntry>();

    private Dictionary<string, Sprite> _table;

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        BuildTable();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void BuildTable()
    {
        _table = new Dictionary<string, Sprite>();

        for (int i = 0; i < _entries.Count; i++)
        {
            SpriteEntry entry = _entries[i];

            if (entry == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.key))
            {
                continue;
            }

            if (entry.icon == null)
            {
                continue;
            }

            if (_table.ContainsKey(entry.key))
            {
                Debug.LogWarning($"[SpriteRegistry] 중복 Key: {entry.key}");
                continue;
            }

            _table.Add(entry.key, entry.icon);
        }
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public Sprite GetSprite(string key)
    {
        if (_table == null)
        {
            BuildTable();
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        if (_table.TryGetValue(key, out Sprite sprite))
        {
            return sprite;
        }

        return null;
    }
    #endregion

    [System.Serializable]
    public class SpriteEntry
    {
        public string key;
        public Sprite icon;
    }
}
