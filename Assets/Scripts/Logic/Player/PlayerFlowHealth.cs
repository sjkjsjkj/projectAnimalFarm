using UnityEngine;

/// <summary>
/// 플레이어의 스태미나와 허기짐을 감소시킵니다.
/// </summary>
public class PlayerFlowHealth : Frameable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("허기짐 설정")]
    [SerializeField] private float _consumeHungerInterval = 0.75f;
    [SerializeField] private float _consumeHungerAmount = 0.6f;

    [Header("목마름 설정")]
    [SerializeField] private float _consumeThirstInterval = 0.75f;
    [SerializeField] private float _consumeThirstAmount = 0.6f;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private float _nextConsumeHungerTime;
    private float _nextConsumeThirstTime;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.First;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        var provider = DataManager.Ins.Player;
        if (provider == null) return;
        // 로직
        float curTime = Time.time;
        ConsumeHunger(provider, curTime);
        ConsumeThirst(provider, curTime);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 시간에 따라 자동으로 배고픔 소모
    private void ConsumeHunger(PlayerProvider provider, float curTime)
    {
        if (UMath.TryCooldownEnd(curTime, ref _nextConsumeHungerTime, _consumeHungerInterval))
        {
            provider.ConsumeHunger(_consumeHungerAmount);
        }
    }
    // 시간에 따라 자동으로 목마름 소모
    private void ConsumeThirst(PlayerProvider provider, float curTime)
    {
        if (UMath.TryCooldownEnd(curTime, ref _nextConsumeThirstTime, _consumeThirstInterval))
        {
            provider.ConsumeThirst(_consumeThirstAmount);
        }
    }
    #endregion
}
