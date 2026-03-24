/// <summary>
/// 경작지 각각의 타일의 정보를 담고있는 클래스
/// 플레이어가 타일의 데이터를 읽어오며 경작 가능여부를 확인할 수 있다면 일반 클래스로, 그렇지 않다면 Mono로 해서 collider Check 방식으로
/// </summary>
public class Farmland
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private EFarmlandState _state;       // 땅의 단계  0:기본 흙 | 1: 일궈진 흙 | 2:
    private string _seededId;           //심어진 씨앗의 id
    private int _pos;            //경작지의 배열좌표
    private float _grownUpTick;
    private float _currentTick;
    private uint _connectDir;
    private uint _stateFlag;
    #endregion

    #region ─────────────────────────▶  외부 공개 변수  ◀─────────────────────────
    public uint ConnectDir => _connectDir;
    public EFarmlandState State => _state;
    public uint StateFlag => _stateFlag;
    #endregion

    #region ─────────────────────────▶  생성자  ◀─────────────────────────
    public Farmland(int pos)
    {
        //경작지의 배열좌표
        _pos = pos;
        _seededId = "";
        _grownUpTick = 0;
        _currentTick = 0;
        _connectDir = 0;
        _state = EFarmlandState.IdleLand;
        _stateFlag |= (uint)EFarmlandState.IdleLand;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    //인터랙트 시도에서 경작지의 상태가 변할 때 불러와질 메서드
    //private void SetState(EFarmlandState nextState)
    private void SetState(EFarmlandState nextState , EFarmlandState nextStateTest)
    {
        EFarmlandState beforeState = _state;

        _state = nextState;
        //UDebug.Print($"before State : {_stateTest}");

        _stateFlag |= (uint)nextStateTest;

        //UDebug.Print($"After State : {_stateTest}");

        //경작지 전체를 관리하는 FarmArea에게 나의 좌표(배열 좌표)와 상태를 전달한다.
        OnFarmStateChange.Publish(_state,_pos, _seededId);
    }
    //외부에서 인터랙트를 시도했을 때 불러와질 메서드
    public void Interact(string seedid = "")
    {
        UDebug.Print("Interact");
        
        switch (_state)
        {
            case EFarmlandState.IdleLand:
                UDebug.Print("check1");
                SetState(EFarmlandState.SoiledLand, EFarmlandState.SoiledLand);
                break;
            case EFarmlandState.SoiledLand:
                UDebug.Print("check2");
                if(string.IsNullOrEmpty(seedid))
                {
                    UDebug.Print("씨앗의 Id가 비어있음. 확인");
                    return;
                }
                if(seedid.CompareTo("None")==0)
                {
                    UDebug.Print("씨앗의 Id가 비어있음. 확인");
                    return;
                }
                _seededId = seedid;
                SetState(EFarmlandState.SeededLand, EFarmlandState.SeededLand);
                //인벤토리 확인
                //씨앗이 있다면 플레이어의 농사 레벨에 맞는 씨앗인지 확인
                //레벨에 맞다면 씨앗의 개수 1감소
                //_seededId 를 해당 씨앗의 id로 설정
                //SetState(SeededLand);
                break;
            case EFarmlandState.SeededLand:

                SetState(EFarmlandState.MoistLand, EFarmlandState.MoistLand);
                //플레이어의 장비 확인
                //씨앗의 등급과 플레이어의 장비 등급 비교
                //플레이어의 장비 등급이 씨앗의 등급 이상이라면
                //_grownUpTime = 심은 씨앗의 등급과 비례.
                //SetState(MoistLand);
                break;
            case EFarmlandState.MoistLand:
                //아무런 효과 없음.
                break;
            case EFarmlandState.GrownUp:
                //인벤토리에 농작물 추가 시도.
                //성공하면
                //SetState(Idleland);
                //실패하면
                //메서드 호출 스택 초기화.
                break;
        }
    }
    //성장 타이머
    public void Tick()
    {
        if(_state != EFarmlandState.MoistLand)
        {
            return;
        }

        if (++_currentTick >= _grownUpTick)
        {
            SetState(EFarmlandState.GrownUp, EFarmlandState.GrownUp);
        }
        
    }
    public void SetConnect(uint connection)
    {
        _connectDir |= connection;

        OnFarmlandConnetionChange.Publish(_connectDir, _state, _pos);
    }
    public void SetConnect(uint connection, EFarmlandState state)
    {
        _connectDir |= connection;
        //Todo 스위치로 state에 맞는 스프라이트를 불러오는 이벤트
        OnFarmlandConnetionChange.Publish(_connectDir, state, _pos);
    }
    #endregion
}
