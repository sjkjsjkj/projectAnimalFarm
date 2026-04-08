
using UnityEngine;

/// <summary>
/// 게임 오디오 설정 로직을 제공합니다.
/// </summary>
public class GameMenuAudio : BaseMono, IEscClosable
{
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void CloseUi()
    {
        UObject.SetActive(gameObject, false);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    
    #endregion

    private void OnEnable()
    {
        EscManager.Ins.Enter(this);
    }
}
