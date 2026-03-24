using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class FarmlandSpriteObject : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("Sprite renderer")]
    [SerializeField] private SpriteRenderer _soilSprite;
    [SerializeField] private SpriteRenderer _moistSprite;
    [SerializeField] private SpriteRenderer _seedSprite;
    #endregion


    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    public void SetSoilSprite(int pos, uint connectDir)
    {
        _soilSprite.sprite = FarmlandSpriteProvider.Ins.GetSoilSprite(connectDir);
    }
    public void SetMoistSprite(int pos, uint connectDir)
    {
        _moistSprite.sprite = FarmlandSpriteProvider.Ins.GetMoistSprite(connectDir);
    }
    #endregion
    public void SetSeedSprite(string id)
    {
        _seedSprite.sprite = FarmlandSpriteProvider.Ins.GetSeedSprite(id,0);
    }
    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
    {
        if(_soilSprite == null || _moistSprite == null || _seedSprite == null)
        {
            UDebug.Print($"인스펙터 확인 필요.");
        }
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
