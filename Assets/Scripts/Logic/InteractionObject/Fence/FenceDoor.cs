using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class FenceDoor : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("스프라이트")]
    [SerializeField] private SpriteRenderer _doorSpRenderer;
    [SerializeField] private SpriteRenderer _upDoorSpRenderer;

    [SerializeField] private Sprite _doorSpriteOpen;
    [SerializeField] private Sprite _doorSpriteClose;
    [SerializeField] private Sprite _upDoorSpriteOpen;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void SetState(bool isOpen)
    {
        if(isOpen)
        {
            _doorSpRenderer.sprite = _doorSpriteOpen;
            _upDoorSpRenderer.sprite = _upDoorSpriteOpen;
        }
        else
        {
            _doorSpRenderer.sprite = _doorSpriteClose;
            _upDoorSpRenderer.sprite = null;
        }
    }
    #endregion
}
