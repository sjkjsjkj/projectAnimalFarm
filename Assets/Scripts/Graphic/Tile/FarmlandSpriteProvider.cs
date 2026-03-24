using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 싱글톤 클래스의 설계 의도입니다.
/// </summary>
public class FarmlandSpriteProvider : Singleton<FarmlandSpriteProvider>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    [SerializeField] private Sprite[] _soilSprites;
    [SerializeField] private Sprite[] _moistSprites;
    [SerializeField] private Dictionary<string, Sprite> _seedSprites;
    #endregion


    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    public Sprite GetSoilSprite(uint index)
    {
        return _soilSprites[index];
    }
    public Sprite GetSeedSprite(string id)
    {
        return _seedSprites[id];
    }
    public Sprite GetMoistSprite(uint index)
    {
        return _moistSprites[index];
    }
    #endregion
}
