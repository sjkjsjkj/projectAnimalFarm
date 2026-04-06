using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 도감 슬롯 1칸 UI
/// 
/// 주의
/// - SheetItemRow는 소문자 필드(id, name, iconKey)를 사용한다.
/// - 기존 대문자 프로퍼티(Id, Name, IconKey)를 사용하면 컴파일 에러가 난다.
/// </summary>
public class UIPictorialBookSlot : BaseMono
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private GameObject _lockedOverlay;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private string _lockedNameText = "???";
    [SerializeField] private bool _hideNameWhenLocked = true;

    private string _itemId;
    private string _itemName;
    private string _iconKey;

    private PictorialBookSystem _bookSystem;
    private SpriteRegistry _spriteRegistry;

    public void SetData(PictorialBookSystem bookSystem, SpriteRegistry spriteRegistry, SheetItemRow row)
    {
        _bookSystem = bookSystem;
        _spriteRegistry = spriteRegistry;

        if (row == null)
        {
            _itemId = string.Empty;
            _itemName = string.Empty;
            _iconKey = string.Empty;
            Refresh();
            return;
        }

        _itemId = row.id;
        _itemName = row.name;
        _iconKey = row.iconKey;

        Refresh();
    }

    public void Refresh()
    {
        bool unlocked = _bookSystem != null && _bookSystem.IsDiscovered(_itemId);

        if (_lockedOverlay != null)
        {
            _lockedOverlay.SetActive(!unlocked);
        }

        if (_nameText != null)
        {
            if (unlocked)
            {
                _nameText.text = _itemName;
            }
            else
            {
                _nameText.text = _hideNameWhenLocked ? _lockedNameText : (string.IsNullOrWhiteSpace(_itemName) ? _lockedNameText : _itemName);
            }
        }

        if (_iconImage != null)
        {
            Sprite icon = GetBestSprite();
            _iconImage.sprite = icon;
            _iconImage.enabled = icon != null;
            _iconImage.color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.45f);
        }
    }

    private Sprite GetBestSprite()
    {
        if (DatabaseManager.Ins != null && string.IsNullOrWhiteSpace(_itemId) == false)
        {
            UnitSO unit = DatabaseManager.Ins.Unit(_itemId);
            if (unit != null && unit.Image != null)
            {
                return unit.Image;
            }
        }

        if (_spriteRegistry != null)
        {
            if (string.IsNullOrWhiteSpace(_iconKey) == false)
            {
                Sprite iconByKey = _spriteRegistry.GetSprite(_iconKey);
                if (iconByKey != null)
                {
                    return iconByKey;
                }
            }

            if (string.IsNullOrWhiteSpace(_itemId) == false)
            {
                Sprite iconById = _spriteRegistry.GetSprite(_itemId);
                if (iconById != null)
                {
                    return iconById;
                }
            }
        }

        return null;
    }
}
