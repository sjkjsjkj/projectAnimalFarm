using UnityEngine;

/// <summary>
/// 플레이어 상태 이벤트를 구독하여 HUD 뷰를 갱신하는 프리젠터입니다.
/// 시작 시 현재 플레이어 값을 한 번 반영하고,
/// 이후에는 이벤트를 통해 HUD를 갱신합니다.
/// </summary>
public class UIHUDPresenter : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    /// <summary>
    /// HUD 실제 표현을 담당하는 뷰 컴포넌트 참조입니다.
    /// </summary>
    [Header("HUD 뷰 참조")]
    [SerializeField] private UIHUDView _view;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 허기 이벤트를 받아 HUD 허기 게이지를 갱신합니다.
    /// </summary>
    private void HandlePlayerHungerChanged(OnPlayerHungerChanged channel)
    {
        if (_view == null)
        {
            return;
        }

        float normalized = 0f;
        if (channel.maxHunger > 0f)
        {
            normalized = channel.curHunger / channel.maxHunger;
        }

        _view.SetHungerNormalized(normalized);
    }

    /// <summary>
    /// 목마름 이벤트를 받아 HUD 목마름 게이지를 갱신합니다.
    /// </summary>
    private void HandlePlayerThirstChanged(OnPlayerThirstChanged channel)
    {
        if (_view == null)
        {
            return;
        }

        float normalized = 0f;

        if (channel.maxThirst > 0f)
        {
            normalized = channel.curThirst / channel.maxThirst;
        }

        _view.SetThirstNormalized(normalized);
    }

    /// <summary>
    /// 돈 이벤트를 받아 HUD 골드 텍스트를 갱신합니다.
    /// </summary>
    private void HandlePlayerMoneyChanged(OnPlayerMoneyChanged channel)
    {
        if (_view == null)
        {
            return;
        }

        _view.SetGoldText(channel.curMoney);
    }

    /// <summary>
    /// 현재 플레이어 프로바이더 값을 읽어 HUD에 즉시 반영합니다.
    /// 초기값을 보여주기 위해 사용합니다.
    /// </summary>
    private void RefreshFromPlayerProvider()
    {
        if (_view == null)
        {
            return;
        }

        if (DataManager.Ins == null)
        {
            return;
        }

        /// <summary>
        /// 현재 플레이어의 런타임 데이터를 보관하는 프로바이더 참조입니다.
        /// </summary>
        PlayerProvider provider = DataManager.Ins.Player;

        if (provider == null)
        {
            return;
        }

        float hungerNormalized = 0f;

        if (provider.MaxHunger > 0f)
        {
            hungerNormalized = provider.CurHunger / provider.MaxHunger;
        }

        float thirstNormalized = 0f;

        if (provider.MaxThirst > 0f)
        {
            thirstNormalized = provider.CurThirst / provider.MaxThirst;
        }

        _view.SetHungerNormalized(hungerNormalized);
        _view.SetThirstNormalized(thirstNormalized);
        _view.SetGoldText(provider.Money);
    }

    /// <summary>
    /// 필수 참조가 정상 연결되었는지 검사합니다.
    /// </summary>
    private bool ValidateReferences()
    {
        if (_view == null)
        {
            UDebug.Print("UIHUDPresenter에 UIHUDView가 연결되지 않았습니다.", LogType.Assert);
            return false;
        }

        return true;
    }

    private void SceneLoadEndHandle(OnSceneLoadEnd ctx)
    {
        RefreshFromPlayerProvider();
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        if (!ValidateReferences())
        {
            return;
        }

        EventBus<OnPlayerHungerChanged>.Subscribe(HandlePlayerHungerChanged);
        EventBus<OnPlayerThirstChanged>.Subscribe(HandlePlayerThirstChanged);
        EventBus<OnPlayerMoneyChanged>.Subscribe(HandlePlayerMoneyChanged);
        EventBus<OnSceneLoadEnd>.Subscribe(SceneLoadEndHandle);

        RefreshFromPlayerProvider();
    }
    private void OnDisable()
    {
        EventBus<OnPlayerHungerChanged>.Unsubscribe(HandlePlayerHungerChanged);
        EventBus<OnPlayerThirstChanged>.Unsubscribe(HandlePlayerThirstChanged);
        EventBus<OnPlayerMoneyChanged>.Unsubscribe(HandlePlayerMoneyChanged);
        EventBus<OnSceneLoadEnd>.Subscribe(SceneLoadEndHandle);
    }

    protected void Reset()
    {
        if (_view == null)
        {
            _view = GetComponent<UIHUDView>();
        }
    }
    #endregion
}
