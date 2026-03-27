using UnityEngine;

/// <summary>
/// 싱글톤 클래스의 설계 의도입니다.
/// </summary>
public class TestManagerSJW : Singleton<TestManagerSJW>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("테스트 ID")]
    [SerializeField] private string _testID;
    [SerializeField] private int _testTileID;

    [Header("동물 테스트")]
    [SerializeField] private GameObject _animalObjectPrefab;

    [Header("농장 테스트")]
    [SerializeField] private GameObject _farmAreaPrefab;
    [SerializeField] private int _pos;
    [SerializeField] private EHarvest _seedId;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private AnimalSO _tempDataUnitSO;
    private FarmArea _testFarmArea;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }

        InitSetting();

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    private void InitSetting()
    {
        TestSetting();
    }
    private void TestSetting()
    {
        GameObject testFarmAreaPrefabGo = Instantiate(_farmAreaPrefab);
        _testFarmArea = testFarmAreaPrefabGo.GetComponent<FarmArea>();
        _testFarmArea.transform.position = Vector3.zero;
    }
    public void TestFunction()
    {
        _tempDataUnitSO = DatabaseManager.Ins.Animal.FindData(_testID);
        //TileData tempTileData = Database.Ins.Tile.FindData(_testTileID);
        
        //UDebug.Print($"{_testID} : {_tempDataUnitSO.Name}\ntempTileData's State : {tempTileData.State}");
    }
    public void TestFunction2()
    {
        GameObject tempGo = Instantiate(_animalObjectPrefab);
        if(!(_tempDataUnitSO as AnimalSO))
        {
            UDebug.Print($"읽어온 데이터에 AnimalSO가 없음.", LogType.Warning);
        }
        tempGo.GetComponent<AnimalObject>().SetInfo(_tempDataUnitSO as AnimalSO);
        tempGo.transform.SetParent(this.transform);
        tempGo.transform.localPosition = Vector3.zero;
    }

    /*public void TestFunction3()
    {
        PoolItem tempGo = FactoryManager.Ins.VFX.Spawn();
        tempGo.transform.SetParent(this.transform);
        tempGo.transform.localPosition = Vector3.zero;
    }*/

    public void TestFunction4()
    {
        _testFarmArea.TestFunction(_pos, _seedId.ToString());

        
    }
    #endregion
}
