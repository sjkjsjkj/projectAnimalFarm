using System.Net;
using UnityEngine;
using static UnityEditor.PlayerSettings;

/// <summary>
/// 경작지들 전체를 관리하는 클래스
/// </summary>
public class FarmArea : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("프리팹")]
    //[SerializeField] private GameObject _cagePrefab;    //울타리로 사용될 프리팹
    [SerializeField] private GameObject _farmSpritePrefab; //경작지의 스프라이트들을 표시해줄 프리팹
    //경작지의 타일/씨앗/자란 농작물 스프라이트를 불러와줄 스프라이트 베이스가 필요할 것 같음.
    //

    [Header("사이즈")]
    [SerializeField] private int _width;
    [SerializeField] private int _height;

    [Header("테스트")]
    [SerializeField] private float _tickTime = 10.0f;   // 테스트용
#endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Farmland[] _farmlands;
    private GameObject[] _farmlandsSprites;
    private float _timer=0;
    //private float _tickTime = 10.0f;                  //테스트 끝나면 이걸 활성화.              
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    //경작지를 생성하는 메서드
    private void MakeFarmlands()
    {
       
        for (int i=0; i< _height; i++)
        {
            for(int j=0; j<_width; j++)
            {
                _farmlands[i * _height + j] = new Farmland(i * _height + j);

                EventBus<OnFarmStateChange>.Unsubscribe(SetFarmLandState);
                EventBus<OnFarmStateChange>.Subscribe(SetFarmLandState);

                EventBus<OnFarmlandConnetionChange>.Unsubscribe(SetFarmLandSprite);
                EventBus<OnFarmlandConnetionChange>.Subscribe(SetFarmLandSprite);

                _farmlandsSprites[i * _height + j] = Instantiate(_farmSpritePrefab);
                _farmlandsSprites[i * _height + j].transform.SetParent(this.transform);
                _farmlandsSprites[i * _height + j].transform.localPosition = new Vector3(i, j);
            }
        }
    }
    //경작지의 상태가 변화할 때 실행되는 이벤트 액션.
    private void SetFarmLandState(OnFarmStateChange context)
    {
        EFarmlandState state = context.state;

        switch (state)
        {
            case EFarmlandState.SeededLand:
                _farmlandsSprites[context.pos].GetComponent<FarmlandSpriteObject>().SetSeedSprite(context.seedId);
                return;
            case EFarmlandState.SoiledLand:
            case EFarmlandState.MoistLand:

                int pos = context.pos;

                string seedId = context.seedId;

                UDebug.Print($"pos : {pos} | State : {state}");

                _farmlands[pos].SetConnect((uint)EConnectionDir.None);

                CheckConnectionDirNearFarmland(pos, state);
                break;
            default:
                return;
        }    
    }
    //주변 경작지의 상태를 불러와 같은 상태인 스프라이트들을 연결시켜주는 로직
    private void CheckConnectionDirNearFarmland(int pos, EFarmlandState state)
    {
        //Down Connection Check
        if (pos % _height != 0)
        {
            CheckConnectDir(pos, pos - 1, EConnectionDir.Down);
        }
        //Up Connection Check
        if (pos % _height != _height - 1)
        {
            CheckConnectDir(pos, pos + 1, EConnectionDir.Up);
        }
        //LEft Connection Check
        if (pos / _height != 0)
        {
            CheckConnectDir(pos, pos - _height, EConnectionDir.Left);
        }
        //Right Connection Check
        if (pos / _height != _width - 1)
        {
            CheckConnectDir(pos, pos + _height, EConnectionDir.Right);
        }

        //uint sameState = _farmlands[pos].StateTest;  //_farmlands[pos].StateTest & _farmlands[pos - 1].StateTest;
 

        //if ((sameState & ((uint)EFarmlandStateTest.MoistLand)) != 0)
        //{
        //    //UDebug.Print($"farmlandState Code : {sameState} | StateEnum : {_farmlands[pos].State.ToString()}");
        //    //UDebug.Print("상태확인 moist");
        //}
        //if ((sameState & ((uint)EFarmlandStateTest.SoiledLand)) != 0)
        //{
        //    //UDebug.Print($"farmlandState Code : {sameState} | StateEnum : {_farmlands[pos].State.ToString()}");
        //    //UDebug.Print("상태확인 Soiled");
        //}
    }
    //각각 
    private void CheckConnectDir(int pos1, int pos2, EConnectionDir dir)
    {
        uint sameState = _farmlands[pos1].StateFlag & _farmlands[pos2].StateFlag;
        uint revDir = GetReverseDir((uint)dir);
        if ((sameState & (uint)EFarmlandState.SoiledLand) != 0)
        {
            _farmlands[pos2].SetConnect(revDir, EFarmlandState.SoiledLand);
            _farmlands[pos1].SetConnect((uint)dir, EFarmlandState.SoiledLand);
        }

        if ((sameState & (uint)EFarmlandState.MoistLand) != 0)
        {
            _farmlands[pos2].SetConnect(revDir, EFarmlandState.MoistLand);
            _farmlands[pos1].SetConnect((uint)dir, EFarmlandState.MoistLand);
        }
    }

    //방향의 반전 (좌<>우 // 상<>하) 
    private uint GetReverseDir(uint dir)
    {
        uint revDir;

        uint mask1, mask2;
        mask1 = 0x5;        //0101
        mask2 = 0xA;       //1010

        mask1 = mask1 & dir; //0101
        mask2 = mask2 & dir; //0010 ?? 왜 0임=?

        //1010      //0001    > 1011
        revDir = mask1 << 1 | mask2 >> 1;

        return revDir;
    }
    //주변 경작지의 상태의 의해 스프라이트가 변경될 때 실행되는 이벤트 액션
    private void SetFarmLandSprite(OnFarmlandConnetionChange context)
    {
        switch(context.state)
        {
            case EFarmlandState.IdleLand:
                break;
            case EFarmlandState.SoiledLand:
                _farmlandsSprites[context.pos].GetComponent<FarmlandSpriteObject>().SetSoilSprite(context.pos, context.connectionDir);
                break;
            case EFarmlandState.SeededLand:
  
                break;
            case EFarmlandState.MoistLand:
                _farmlandsSprites[context.pos].GetComponent<FarmlandSpriteObject>().SetMoistSprite(context.pos, context.connectionDir);
                break;
            case EFarmlandState.GrownUp:
                break;
        }
        UDebug.Print($"Result State : {_farmlands[context.pos].State}");
       
    }
    //경작지는 일반 클래스이기 때문에 Update가 불가능함.
    //그래서 경작지를 관리하는 이 클래스에서 모든 경작지의 성장? 타이머를 체크해준다고 생각하면 된다.
    private void OnTick()
    {
        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                _farmlands[i * _height + j].Tick();
            }
        }
        _timer = 0;
    }


    public void TestFunction(int pos, string seedId)
    {
        _farmlands[pos].Interact(seedId);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
    {
        _farmlands = new Farmland[_width* _height];
        _farmlandsSprites = new GameObject[_width * _height];

        MakeFarmlands();
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
