using System.Collections;
using UnityEngine;

/// <summary>
/// 인터랙션오브젝트 중 울타리문에게 부착될 스크립트 입니다.
/// </summary>
[RequireComponent (typeof(Collider2D))]
public class FenceObject : BaseMono , IInteractable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("문 오브젝트")]
    [SerializeField] private SpriteRenderer _mainDoorSpRenderer;

    [SerializeField] private FenceDoor _leftDoor;
    [SerializeField] private FenceDoor _rightDoor;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Collider2D _col;
    private bool _isOpen = false;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool CanInteract(GameObject player)
    {
        return true;
    }
    public string GetMessage()
    {
        UDebug.Print("문과 상호작용했다.");
        return new string("문과 상호작용 했다.");
    }

    public void Interact(GameObject player)
    {
        OpenDoor();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void OpenDoor()
    {
        _isOpen = true;
        StartCoroutine(CoDoorCoroutine());
    }
    private void DoorSetting(bool isOpen)
    {
        _leftDoor.SetState(isOpen);
        _rightDoor.SetState(isOpen);
        _col.enabled = !isOpen;
        _mainDoorSpRenderer.enabled = !isOpen;
    }
    private IEnumerator CoDoorCoroutine()
    {
        //UDebug.Print("문 열렸다.");
        DoorSetting(true);
        USound.PlaySfx(Id.Sfx_Player_Door_1, transform);
        yield return new WaitForSeconds(0.5f);

        //UDebug.Print("문 닫혔다.");
        USound.PlaySfx(Id.Sfx_Player_Door_2, transform);
        DoorSetting(false);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected new void Awake()
    {
        _isOpen = false;
        _col = GetComponent<Collider2D>();
        DoorSetting(_isOpen);
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
