using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 도감 슬롯 1칸 UI.
/// 아이콘 하나만 사용해서 잠금 상태면 반투명, 해금 상태면 컬러로 표시한다.
/// 
/// 추가 기능
/// - 해금된 슬롯을 클릭하면 상세 정보 패널을 연다.
/// </summary>
public class UIPictorialBookSlot : BaseMono, IPointerClickHandler
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

    [Header("상세 정보")]
    [SerializeField] private UIPictorialBookDetailPanel _detailPanel;
    [SerializeField] private bool _showDetailOnlyUnlocked = true;

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
        SetData(bookSystem, entry, _detailPanel);
    }

    public void SetData(PictorialBookSystem bookSystem, PictorialBookEntry entry, UIPictorialBookDetailPanel detailPanel)
    {
        TryAutoBindReferences();

        _bookSystem = bookSystem;
        _entry = entry;

        if (detailPanel != null)
        {
            _detailPanel = detailPanel;
        }

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (!CanOpenDetail())
        {
            return;
        }

        if (_detailPanel == null)
        {
            Debug.LogWarning("[UIPictorialBookSlot] DetailPanel이 연결되지 않았습니다.");
            return;
        }

        _detailPanel.Show(_entry);
    }

    private bool CanOpenDetail()
    {
        if (_entry == null)
        {
            return false;
        }

        if (_showDetailOnlyUnlocked == false)
        {
            return true;
        }

        if (_bookSystem == null)
        {
            return false;
        }

        string itemId = NormalizeText(_entry.itemId);
        return _bookSystem.IsDiscovered(itemId);
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
