using UnityEngine;

/// <summary>
/// 2D 상호작용 오브젝트의 공통 규칙을 관리하는 베이스 클래스
/// </summary>
public abstract class CInteractableBase2D : MonoBehaviour
{

    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("텍스트")]
    [SerializeField] private string _displayName = "상호작용 대상"; 
    [SerializeField] private string _verbText = "사용"; 
    [Header("규칙")]
    [SerializeField] private bool _useOnce = false; 
    [SerializeField] private float _cooldown = 0f; 
    [Header("힌트")]
    [SerializeField] private Transform _hintAnchor;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public string DisplayName => _displayName;
    public string VerbText => _verbText;
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _usedOnced = false;
    private float _nextInteractTimed = 0f;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion



    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
