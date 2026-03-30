using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// 인벤토리 슬롯 UI를 드래그N드롭 할 때 마우스에 보여질 아이콘 입니다.
/// </summary>
public class DragItemIconUI : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("UI")]
    [SerializeField] private Image _icon;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void Show(Sprite sprite)
    {
        _icon.sprite = sprite;
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
    #endregion

    #region─────────────────────────▶ 메시지 함수◀─────────────────────────
    private void Awake()
    {
        gameObject.SetActive(false);
    }
    #endregion
}
