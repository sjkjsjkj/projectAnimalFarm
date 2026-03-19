using UnityEngine;
using UnityEngine.Animations;
/// <summary>
/// 동물의 MonoBehaivour를 관리하는 객체입니다.
/// 데이터는 .Data로 접근할 수 있습니다.
/// </summary>
public class AnimalObject : BaseMono 
{
    #region ─────────────────────────▶ 인스 펙터 ◀─────────────────────────
    [Header("MonoBehaviour")]
    [SerializeField] private SpriteRenderer _spRenderer;
    [SerializeField] private AnimalSO _testData;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private AnimalData _data;
    private EAnimalState _state;
    private float _timer = 0;
    private Animator _animator;
    private bool IsHungry => Data.IsHungry;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public AnimalData Data => _data;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void SetState(EAnimalState nextState)
    {
        EAnimalState prevState = _state;
        _state = nextState;
    }
    public void SetInfo(AnimalSO dataSO)
    {
        _spRenderer.sprite = dataSO.Image;
        _data = new AnimalData(dataSO);
        _animator.runtimeAnimatorController = dataSO.Anim;
    }
    /// <summary>
    /// 동물의 피곤함 / 배고픔 등의 수치를 체크하는 메서드 이것으로 동물의 다음 state를 관리합니다.
    /// </summary>
    private void CheckHealth()
    {
        //if(피곤하다면)
        // SetState(EAnimalState.GoToBed);

        if (IsHungry)
        {
            SetState(EAnimalState.MoveToEat);
        }
    }
    /*
    public enum EAnimalState
    {
        Idle = 0,
        Move = 1,
        Sleep = 2,
        Eat = 3,
        MoveToEat = 4,
        MoveToBed= 5,
        Dead = 6,
    }

     */
    private void UpdateMoveToEat()
    {

    }
    private void UpdateMove()
    {

    }
    private void UpdateSleep()
    {

    }
    private void UpdateEat()
    {

    }
#endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
    {
        _spRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        UDebug.IsNull(_spRenderer, LogType.Warning);
        UDebug.IsNull(_animator, LogType.Warning);
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if(_timer >= 5)
        {
            _timer = 0;
            _data.Tick();
        }

        CheckHealth();

        switch(_state)
        {
            case EAnimalState.MoveToEat:
                UpdateMoveToEat();
                break;
            case EAnimalState.Move:
                UpdateMove();
                break;
            case EAnimalState.Sleep:
                UpdateSleep();
                break;
            case EAnimalState.Eat:
                UpdateEat();
                break;
            default:
                break;
        }

    }
    private void OnEnable()
    {
        
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
