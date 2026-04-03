using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TestPlayerSJW : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    //[Header("주제")]
    //[SerializeField] private Class _class;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnTriggerEnter2D(Collider2D collision)
    {
        UDebug.Print($"Something Hit : {collision.name}");
        IInteractable tempInteractInterface = collision.GetComponent<IInteractable>();

        if (tempInteractInterface == null)
        {
            UDebug.Print("123123");
        }
        else
        {
            tempInteractInterface.OnInteract();
        }
        //if (collision is IInteractable interactable)
        //{
        
        //    UDebug.Print("인터랙트 가능함.");
        //    interactable.OnInteract();
        //}
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
