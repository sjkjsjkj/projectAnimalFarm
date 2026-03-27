using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 ID 또는 IconKey와 실제 스프라이트를 연결하는 레지스트리
/// 인스펙터에서 직접 매칭해두면 UI에서 쉽게 가져올 수 있다.
/// </summary>
public class SpriteRegistry : MonoBehaviour
{
    [System.Serializable]
    public class SpriteEntry
    {
        public string Key;
        public Sprite Icon;
    }

    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private List<SpriteEntry> _entries = new List<SpriteEntry>();
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Dictionary<string, Sprite> _table;
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
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

            if (string.IsNullOrWhiteSpace(entry.Key))
            {
                continue;
            }

            if (entry.Icon == null)
            {
                continue;
            }

            if (_table.ContainsKey(entry.Key))
            {
                Debug.LogWarning($"[SpriteRegistry] 중복 Key: {entry.Key}");
                continue;
            }

            _table.Add(entry.Key, entry.Icon);
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
}
