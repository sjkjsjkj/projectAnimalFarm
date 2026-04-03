using System;
using UnityEngine;

/// <summary>
/// 경작지 각각의 스프라이트를 관리하는 스크립트입니다.
/// </summary>
public class FarmlandSpriteObject : BaseMono , IAutoInteractable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("Sprite renderer")]
    [SerializeField] private SpriteRenderer _soilSprite;
    [SerializeField] private SpriteRenderer _moistSprite;
    [SerializeField] private SpriteRenderer _seedSprite;
    [SerializeField] private SpriteRenderer _harvestFinishIcon;
    #endregion

    #region  ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private FarmArea _area;
    private int _idx;
    private bool _isGrownUp;
    #endregion


    #region  ─────────────────────────▶외부 공개◀─────────────────────────
    public SpriteRenderer FinishIcon => _harvestFinishIcon;

    public event Action<int> OnInteract;


    public void SetInfo(FarmArea area, int idx)
    {
        _harvestFinishIcon.enabled = false;
        _area = area;
        _idx = idx;
    }
    public bool CanInteract(GameObject player)
    {
        UDebug.Print("경작지 인터랙트 시도");
        
        return _area.ReturnFarmLandCanInteract(_idx);
    }

    public string GetMessage()
    {
        UDebug.Print("경작지 인터랙트 확인.");
        return "경작지 인터랙트 확인했습니다.";
    }

    public void Interact(GameObject player)
    {
        OnInteract?.Invoke(_idx);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    
    public void ResetSprite()
    {
        _soilSprite.sprite = null;
        _moistSprite.sprite = null;
        _seedSprite.sprite = null;
        _harvestFinishIcon.enabled = false;
    }
    public void SetSoilSprite(uint connectDir)
    {
        _soilSprite.sprite = FarmlandSpriteProvider.Ins.GetSoilSprite(connectDir);
    }
    public void SetMoistSprite(uint connectDir)
    {
        _moistSprite.sprite = FarmlandSpriteProvider.Ins.GetMoistSprite(connectDir);
    }
    #endregion
    public void SetSeedSprite(string id, int grownProgress = 0)
    {
        Sprite changeSprite = FarmlandSpriteProvider.Ins.GetSeedSprite(id, grownProgress);
        UDebug.Print($"changeSprite Info\nSprite Name : {changeSprite.name}");
        _seedSprite.sprite = changeSprite;
    }
    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        if(_soilSprite == null || _moistSprite == null || _seedSprite == null)
        {
            UDebug.Print($"인스펙터 확인 필요.");
        }
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
