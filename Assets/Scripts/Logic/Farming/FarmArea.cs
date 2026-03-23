using UnityEngine;

/// <summary>
/// 경작지들 전체를 관리하는 클래스
/// </summary>
public class FarmArea : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("프리팹")]
    [SerializeField] private GameObject _cagePrefab;    //울타리로 사용될 프리팹
    //경작지의 타일/씨앗/자란 농작물 스프라이트를 불러와줄 스프라이트 베이스가 필요할 것 같음.
    //

    [Header("사이즈")]
    [SerializeField] private int _witdh;
    [SerializeField] private int _height;

    [Header("테스트")]
    [SerializeField] private float _tickTime = 10.0f;   // 테스트용
#endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Farmland[,] _farmlands;
    private float _timer=0;
    //private float _tickTime = 10.0f;                  //테스트 끝나면 이걸 활성화.              
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    //경작지를 생성하는 메서드
    private void MakeFarmlands()
    {
        for(int i=0; i<_height; i++)
        {
            for(int j=0; j<_witdh; j++)
            {
                _farmlands[i, j] = new Farmland( (i,j) );
                EventBus<OnFarmStateChange>.Subscribe(SetFarmLand);
            }
        }
    }
    //경작지의 상태가 변화할 때 실행되는 이벤트 액션.
    private void SetFarmLand(OnFarmStateChange context)
    {
        int posX = context.pos.Item1;
        int posY = context.pos.Item2;
        EFarmlandState state = context.state;
        string seedId = context.seedId;

        //ToDo : 상태에 따라 다른 세팅
        //주변 경작지의 상태에 따라 다른 스프라이트를 입히면 완성도는 높아질텐데.. 재귀 사용해야할듯?
        //
    }
    //경작지는 일반 클래스이기 때문에 Update가 불가능함.
    //그래서 경작지를 관리하는 이 클래스에서 모든 경작지의 성장? 타이머를 체크해준다고 생각하면 된다.
    private void OnTick()
    {
        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _witdh; j++)
            {
                _farmlands[i, j].Tick();
            }
        }
        _timer = 0;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
    {
        _farmlands = new Farmland[_height, _witdh];
    }
    private void Update()
    {
        _timer += Time.deltaTime;

        if(_timer >= _tickTime)
        {
            OnTick();
        }
    }
    #endregion
}
