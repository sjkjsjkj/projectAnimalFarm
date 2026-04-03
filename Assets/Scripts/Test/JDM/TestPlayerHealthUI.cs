using UnityEngine;
using TMPro;

/// <summary>
/// 플레이어의 배고픔 / 목마름 임시 UI
/// </summary>
public class TestPlayerHealthUI : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("텍스트 UI 연결")]
    [SerializeField] private TextMeshProUGUI _textUi;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private float _curHunger;
    private float _curThirst;
    private float _maxHunger;
    private float _maxThirst;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void HungerChangedHandle(OnPlayerHungerChanged ctx)
    {
        _curHunger = ctx.curHunger;
        _maxHunger = ctx.maxHunger;
        ChangeHealthUI();
    }
    private void ThirstChangedHandle(OnPlayerThirstChanged ctx)
    {
        _curThirst = ctx.curThirst;
        _maxThirst = ctx.maxThirst;
        ChangeHealthUI();
    }
    private void ChangeHealthUI()
    {
        _textUi.SetText("[ 배고픔 = {0:1}/{1:1}, 목마름 = {2:1}/{3:1} ]", _curHunger, _maxHunger, _curThirst, _maxThirst);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnEnable()
    {
        EventBus<OnPlayerHungerChanged>.Subscribe(HungerChangedHandle);
        EventBus<OnPlayerThirstChanged>.Subscribe(ThirstChangedHandle);
    }
    private void OnDisable()
    {
        EventBus<OnPlayerHungerChanged>.Unsubscribe(HungerChangedHandle);
        EventBus<OnPlayerThirstChanged>.Unsubscribe(ThirstChangedHandle);
    }
    // 자동으로 캔버스를 부모로 삼기
    private void Start()
    {
        GameObject go = UObject.Find(K.NAME_UI_ROOT);
        if (go == null) return;
        // 캔버스 발견했으니 들어가기
        transform.SetParent(go.transform);
    }
    #endregion
}
