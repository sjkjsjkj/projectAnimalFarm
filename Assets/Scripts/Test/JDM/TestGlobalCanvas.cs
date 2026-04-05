using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TestGlobalCanvas : BaseMono
{
    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void CreateGlobalCanvas()
    {
        GameObject go = UObject.Create("@GlobalCanvas");
        UObject.AddComponent<Canvas>(go);
        DontDestroyOnLoad(go);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private new void Awake()
    {
        CreateGlobalCanvas();
    }
    #endregion
}
