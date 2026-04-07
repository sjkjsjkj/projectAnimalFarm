using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 도감 슬롯 1칸 UI.
/// 아이콘 하나만 사용해서 잠금 상태면 반투명, 해금 상태면 컬러로 표시한다.
/// </summary>
public class UIPictorialBookSlot : BaseMono
{
    [Header("UI 참조")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;

    [Header("자동 연결")]
    [SerializeField] private bool _autoBindReferences = true;
    [SerializeField] private string _iconObjectName = "UnlockedIcon";
    [SerializeField] private string _nameTextObjectName = "Text_Name";

    [Header("잠금 상태 표시")]
    [SerializeField] private string _lockedNameText = "???";
    [SerializeField] private bool _hideNameWhenLocked = true;
    [SerializeField] private Color _lockedColor = new Color(1f, 1f, 1f, 0.45f);
    [SerializeField] private Color _unlockedColor = Color.white;

    private PictorialBookSystem _bookSystem;
    private PictorialBookEntry _entry;

    private void Reset()
    {
        TryAutoBindReferences();
    }

    private void OnValidate()
    {
        TryAutoBindReferences();
    }

    public void SetData(PictorialBookSystem bookSystem, PictorialBookEntry entry)
    {
        TryAutoBindReferences();

        _bookSystem = bookSystem;
        _entry = entry;

        Refresh();
    }

    public void Refresh()
    {
        TryAutoBindReferences();

        string itemId = _entry != null ? NormalizeText(_entry.itemId) : string.Empty;
        string displayName = _entry != null ? NormalizeText(_entry.displayName) : string.Empty;
        Sprite icon = _entry != null ? _entry.icon : null;

        bool unlocked = _bookSystem != null && _bookSystem.IsDiscovered(itemId);

        if (_nameText != null)
        {
            if (unlocked)
            {
                _nameText.text = displayName;
            }
            else
            {
                _nameText.text = _hideNameWhenLocked
                    ? _lockedNameText
                    : (string.IsNullOrWhiteSpace(displayName) ? _lockedNameText : displayName);
            }
        }

        if (_iconImage != null)
        {
            _iconImage.sprite = icon;
            _iconImage.enabled = icon != null;
            _iconImage.color = unlocked ? _unlockedColor : _lockedColor;
        }

        Debug.Log($"[UIPictorialBookSlot] id={itemId}, unlocked={unlocked}");
    }

    private void TryAutoBindReferences()
    {
        if (_autoBindReferences == false)
        {
            return;
        }

        if (_iconImage == null)
        {
            Transform iconTransform = FindDeepChildByName(transform, _iconObjectName);
            if (iconTransform != null)
            {
                _iconImage = iconTransform.GetComponent<Image>();
            }
        }

        if (_nameText == null)
        {
            Transform nameTextTransform = FindDeepChildByName(transform, _nameTextObjectName);
            if (nameTextTransform != null)
            {
                _nameText = nameTextTransform.GetComponent<TMP_Text>();
            }
        }
    }

    private Transform FindDeepChildByName(Transform parent, string targetName)
    {
        if (parent == null || string.IsNullOrWhiteSpace(targetName))
        {
            return null;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == targetName)
            {
                return child;
            }

            Transform found = FindDeepChildByName(child, targetName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private string NormalizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Trim();
    }
}
