using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeField] private Sprite[] _whatSprites;         
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
        _seedSprites.Add(EHarvest.StrawBerry.ToSafeString(), _strawBerrySprites);
        _seedSprites.Add(EHarvest.GreenOnion.ToSafeString(), _greendOnionSprites);
        _seedSprites.Add(EHarvest.Potato.ToSafeString(), _potatoSprites);
        _seedSprites.Add(EHarvest.Onion.ToSafeString(), _onionSprites);
        _seedSprites.Add(EHarvest.Carrot.ToSafeString(), _carrotSprites);
        _seedSprites.Add(EHarvest.BlueBerry.ToSafeString(), _blueBerrySprites);
        _seedSprites.Add(EHarvest.Radish.ToSafeString(), _radishSprites);
        _seedSprites.Add(EHarvest.Cabbage.ToSafeString(), _cabbageSprites);
        _seedSprites.Add(EHarvest.Cauliflower.ToSafeString(), _whatSprites);
        _seedSprites.Add(EHarvest.Rice.ToSafeString(), _riceSprites);
        _seedSprites.Add(EHarvest.Broccoli.ToSafeString(), _broccoliSprites);
    }
    #endregion
}
