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
    #endregion

    #region  ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private int _idx;
    private string _currentSeededId;
    private bool _isGrownUp;
    private EFarmlandState _state;
    #endregion

    #region  ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public bool CanInteract(GameObject player)
    {
        return true;
    }

    public string GetMessage()
    {
        UDebug.Print("생산 완료!");
        return "생산 완료!";
    }

    public void Interact(GameObject player)
    {
        _isGrownUp = false;

        //ItemCollectionCoordinator.Ins.TryCollectItem(_productItemId, 1);

        //_productFinishIcon.SetActive(false);
        //_isProductFinish = false;
        //_data.ProductReset();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

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
