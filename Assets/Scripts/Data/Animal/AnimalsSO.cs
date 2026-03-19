using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "AnimalSO_", menuName = "AnimalSO", order = 1)]
public class AnimalSO : DatabaseUnitSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────

    [Header("세부 정보")]
    [SerializeField] private bool _needFood;                    // 음식 섭취를 필요로 하는 동물인지.
    [SerializeField] private float _foodConsumeAmount;          // 음식을 얼마나 섭취하는지 (1틱당 소모되는 허기짐)
    [SerializeField] private AnimatorController _animatorController;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀────────────────────────
    public bool NeedFood => _needFood;
    public float FoodConsumeAmount => _foodConsumeAmount;
    public AnimatorController Anim => _animatorController;
    #endregion
}
