using System;
using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TitleButton : BaseMono
{
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void GameStart()
    {
        GameManager.Ins.LoadSceneAsync((int)EScene.Main, OnLoadComplete, OnProgress);
    }
    #endregion

    private void OnProgress(float progress)
    {
        UDebug.Print($"로딩 중입니다. {progress * 100}%");
    }

    private void OnLoadComplete()
    {
        UDebug.Print($"씬 로드 완료!");
    }
}
