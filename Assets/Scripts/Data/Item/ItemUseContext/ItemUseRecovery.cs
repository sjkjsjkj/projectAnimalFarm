using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "ItemUseRecovery_", menuName = "ScriptableObjects/ItemUseContext/Recovery", order = 1)]
public class ItemUseRecovery : ItemUseContextSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] protected int _hungerRecoveryAmount;
    [SerializeField] protected int _thirstRecoveryAmount;

    [Header("isDrink가 true면 마시는 동작 / false면 먹는 동작")]
    [SerializeField] bool _isDrink;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public int HungerRecoveryAmount => _hungerRecoveryAmount;
    public int ThirstRecoveryAmount => _thirstRecoveryAmount;
    public override bool TryUse()
    {
        DataManager.Ins.Player.RecoverHunger(_hungerRecoveryAmount);
        DataManager.Ins.Player.RecoverThirst(_thirstRecoveryAmount);
        if(_isDrink)
        {
            OnPlayerDrinking.Publish(0.3f);
        }
        else
        {
            OnPlayerEating.Publish(0.3f);
        }
            return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
  
    #endregion
}
