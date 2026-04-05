using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 싱글톤 클래스의 설계 의도입니다.
/// </summary>
public class FarmlandSpriteProvider : Singleton<FarmlandSpriteProvider>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    [SerializeField] private Sprite[] _soilSprites;
    [SerializeField] private Sprite[] _moistSprites;
    [SerializeField] private Dictionary<string, Sprite[]> _seedSprites;

    // Dictionary에 넣을 스프라이트들을 인스펙터로 받기 위해 만든 배열.
    // 어차피 참조만 늘어나는 것 이기 때문에 따로 지우진 않겠음.
    [SerializeField] private Sprite[] _strawBerrySprites;    
    [SerializeField] private Sprite[] _greendOnionSprites;  
    [SerializeField] private Sprite[] _potatoSprites;       
    [SerializeField] private Sprite[] _onionSprites;        
    [SerializeField] private Sprite[] _carrotSprites;       
    [SerializeField] private Sprite[] _blueBerrySprites;    
    [SerializeField] private Sprite[] _radishSprites;       
    [SerializeField] private Sprite[] _cabbageSprites;      
    [SerializeField] private Sprite[] _cauliFlowerSprites;         
    [SerializeField] private Sprite[] _riceSprites;         
    [SerializeField] private Sprite[] _broccoliSprites;     
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        _seedSprites = new Dictionary<string, Sprite[]>();

        SetDictionary();
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    public Sprite GetSoilSprite(uint index)
    {
        return _soilSprites[index];
    }
    public Sprite GetSeedSprite(string id, int grownProgress)
    {
        if (!_seedSprites.ContainsKey(id))
        {
            UDebug.Print($"{id}는 seedSprites에 존재하지 않습니다.", LogType.Assert);
            return null;
        }
        int index = Mathf.Clamp(grownProgress, 0, _seedSprites[id].Length - 1);
        /*if (grownProgress < 0 || _seedSprites[id].Length <= grownProgress)
        {
            UDebug.Print($"{grownProgress}는 {_seedSprites[id]}의 배열 크기를 벗어났습니다. (최대 {_seedSprites[id].Length})", LogType.Assert);
            return null;
        }*/
        return _seedSprites[id][index];
    }
    public Sprite GetMoistSprite(uint index)
    {
        return _moistSprites[index];
    }
    private void SetDictionary()
    {
        _seedSprites.Add(Id.Item_Seed_StrawBerry, _strawBerrySprites);
        _seedSprites.Add(Id.Item_Seed_GreenOnion, _greendOnionSprites);
        _seedSprites.Add(Id.Item_Seed_Potato,  _potatoSprites);
        _seedSprites.Add(Id.Item_Seed_Onion, _onionSprites);
        _seedSprites.Add(Id.Item_Seed_Carrot, _carrotSprites);
        _seedSprites.Add(Id.Item_Seed_BlueBerry, _blueBerrySprites);
        _seedSprites.Add(Id.Item_Seed_Radish, _radishSprites);
        _seedSprites.Add(Id.Item_Seed_Cabbage,     _cabbageSprites);
        _seedSprites.Add(Id.Item_Seed_Cauliflower,    _cauliFlowerSprites);
        _seedSprites.Add(Id.Item_Seed_Rice,  _riceSprites);
        _seedSprites.Add(Id.Item_Seed_Broccoli,_broccoliSprites);
    }
    #endregion
}
