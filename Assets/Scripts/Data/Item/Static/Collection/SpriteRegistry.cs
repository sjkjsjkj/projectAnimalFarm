using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 ID 또는 아이콘 키에 대응하는 Sprite를 보관하는 레지스트리.
/// 
/// 이 스크립트의 역할
/// 1. 도감 UI에서 사용할 스프라이트를 key 기준으로 찾을 수 있게 한다.
/// 2. GoogleSheetTsvLoader / SheetItemRow의 iconKey 와 연결해서 사용할 수 있다.
/// 3. UIPictorialBookSlot이 iconKey 또는 itemId로 스프라이트를 조회할 때 사용한다.
/// 
/// 사용 방법
/// - 인스펙터에서 key 와 sprite를 짝으로 등록
/// - GetSprite("All Fish_0") 또는 GetSprite("붕어") 같은 방식으로 조회
/// 
/// 중요
/// - 중복 key가 있으면 뒤의 값을 무시하고 경고를 출력한다.
/// - 이 스크립트는 "아이콘 보관소" 역할만 한다.
/// </summary>
public class SpriteRegistry : BaseMono
{
    [System.Serializable]
    private class SpriteEntry
    {
        [Tooltip("아이템 ID 또는 아이콘 키")]
        public string key;

        [Tooltip("해당 key에 대응하는 스프라이트")]
        public Sprite sprite;
    }

    [Header("스프라이트 목록")]
    [SerializeField] private List<SpriteEntry> _entries = new List<SpriteEntry>();

    [Header("옵션")]
    [SerializeField] private bool _buildOnAwake = true;
    [SerializeField] private bool _logEnabled = true;

    private readonly Dictionary<string, Sprite> _table = new Dictionary<string, Sprite>();

    /// <summary>
    /// 현재 등록된 key 개수
    /// </summary>
    public int Count => _table.Count;

    protected override void Awake()
    {
        base.Awake();

        if (_buildOnAwake)
        {
            Rebuild();
        }
    }

    /// <summary>
    /// 인스펙터에 설정된 목록을 기준으로 내부 딕셔너리를 다시 생성
    /// </summary>
    public void Rebuild()
    {
        _table.Clear();

        if (_entries == null || _entries.Count == 0)
        {
            if (_logEnabled)
            {
                Debug.LogWarning("[SpriteRegistry] 등록된 엔트리가 없습니다.");
            }
            return;
        }

        for (int i = 0; i < _entries.Count; i++)
        {
            SpriteEntry entry = _entries[i];

            if (entry == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.key))
            {
                if (_logEnabled)
                {
                    Debug.LogWarning($"[SpriteRegistry] key가 비어 있어 스킵합니다. index={i}");
                }
                continue;
            }

            if (entry.sprite == null)
            {
                if (_logEnabled)
                {
                    Debug.LogWarning($"[SpriteRegistry] sprite가 비어 있어 스킵합니다. key={entry.key}");
                }
                continue;
            }

            if (_table.ContainsKey(entry.key))
            {
                if (_logEnabled)
                {
                    Debug.LogWarning($"[SpriteRegistry] 중복 key 발견: {entry.key}");
                }
                continue;
            }

            _table.Add(entry.key, entry.sprite);
        }

        if (_logEnabled)
        {
            Debug.Log($"[SpriteRegistry] 등록 완료. Count={_table.Count}");
        }
    }

    /// <summary>
    /// key로 스프라이트 조회
    /// 못 찾으면 null 반환
    /// </summary>
    public Sprite GetSprite(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        if (_table.Count == 0 && _entries != null && _entries.Count > 0)
        {
            Rebuild();
        }

        if (_table.TryGetValue(key, out Sprite sprite))
        {
            return sprite;
        }

        return null;
    }

    /// <summary>
    /// key가 등록되어 있는지 확인
    /// </summary>
    public bool ContainsKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        if (_table.Count == 0 && _entries != null && _entries.Count > 0)
        {
            Rebuild();
        }

        return _table.ContainsKey(key);
    }

    /// <summary>
    /// 런타임에서 key / sprite를 새로 추가
    /// 이미 있으면 false 반환
    /// </summary>
    public bool Add(string key, Sprite sprite)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        if (sprite == null)
        {
            return false;
        }

        if (_table.ContainsKey(key))
        {
            return false;
        }

        _table.Add(key, sprite);
        return true;
    }

    /// <summary>
    /// 내부 테이블 초기화
    /// </summary>
    public void Clear()
    {
        _table.Clear();
    }
}
