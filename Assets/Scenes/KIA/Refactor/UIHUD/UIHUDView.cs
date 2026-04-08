using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD 상태 UI의 실제 표현을 담당하는 뷰 컴포넌트입니다.
/// 허기, 목마름, 골드 표시를 외부에서 갱신할 수 있도록 제공합니다.
/// </summary>
public class UIHUDView : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("HUD 상태 표시 참조")]
    [SerializeField] private Image _hungerFill;
    [SerializeField] private Image _thirstFill;
    [SerializeField] private TMP_Text _goldText;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public bool HasHungerFill => _hungerFill != null;
    public bool HasThirstFill => _thirstFill != null;
    public bool HasGoldText => _goldText != null;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 전달받은 수치를 0~1 범위로 보정합니다.
    /// Fill Amount에 안전하게 사용하기 위한 함수입니다.
    /// </summary>
    private float ClampNormalized(float value)
    {
        return Mathf.Clamp01(value);
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 허기 게이지 Fill Amount를 갱신합니다.
    /// </summary>
    /// <param name="normalized">0~1 범위의 허기 비율</param>
    public void SetHungerNormalized(float normalized)
    {
        if (_hungerFill == null)
        {
            return;
        }

        _hungerFill.fillAmount = ClampNormalized(normalized);
    }

    /// <summary>
    /// 목마름 게이지 Fill Amount를 갱신합니다.
    /// </summary>
    /// <param name="normalized">0~1 범위의 목마름 비율</param>
    public void SetThirstNormalized(float normalized)
    {
        if (_thirstFill == null)
        {
            return;
        }

        _thirstFill.fillAmount = ClampNormalized(normalized);
    }

    /// <summary>
    /// 현재 골드 텍스트를 갱신합니다.
    /// </summary>
    /// <param name="curMoney">현재 플레이어가 보유한 돈</param>
    public void SetGoldText(int curMoney)
    {
        if (_goldText == null)
        {
            return;
        }

        _goldText.text = curMoney.ToString();
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 인스펙터 연결 편의를 위해 같은 하이어라키 내 자식을 자동으로 탐색합니다.
    /// 오브젝트 이름이 맞을 때만 자동 연결됩니다.
    /// </summary>
    protected void Reset()
    {
        if (_hungerFill == null)
        {
            Transform hunger = transform.Find("HungerRow/HungerFill");
            if (hunger != null)
            {
                _hungerFill = hunger.GetComponent<Image>();
            }
        }

        if (_thirstFill == null)
        {
            Transform thirst = transform.Find("ThirstRow/ThirstFill");
            if (thirst != null)
            {
                _thirstFill = thirst.GetComponent<Image>();
            }
        }

        if (_goldText == null)
        {
            Transform gold = transform.Find("GoldRow/GoldText");
            if (gold != null)
            {
                _goldText = gold.GetComponent<TMP_Text>();
            }
        }
    }
    #endregion
}
