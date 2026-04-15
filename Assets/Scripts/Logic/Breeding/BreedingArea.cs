using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사육장 클래스 입니다.
/// </summary>
public class BreedingArea : Singleton<BreedingArea>, IFoodProvider
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("프리팹")]
    [SerializeField] private GameObject _foodBoxPrefab; //먹이통으로 사용될 프리팹
    //[SerializeField] private AnimalObject _animalPrefab; // 동물 프리팹 (테스트용 실제로는 사용하지 않을 녀석)

    [Header("사이즈")]
    [SerializeField] private int _witdh;
    [SerializeField] private int _height;

    [Header("테스트")]
    [SerializeField] private string _testId;
    [SerializeField] private FoodBox _foodBox;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Vector3 _foodBoxPos;        // 먹이통의 위치
    private List<AnimalObject> _animals; // 관리하는 동물들의 리스트
//    private FoodBox _foodBox;

    private bool _isInitialized = false;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    public FoodBox FoodBox => _foodBox;

    public void SetInfo(int idx, Transform foodboxTr)
    {
        if(UDebug.IsNull(_foodBoxPrefab))
        {
            return;
        }
        _animals = new List<AnimalObject>();

        if(!(InventoryManager.Ins.Inventories[idx] is FoodBox foodBox))
        {
            UDebug.Print($"current FoodBox's Idx : {idx}");
        }
        else
        {
            _foodBox = foodBox;
            _foodBoxPos = foodboxTr.localPosition;// foodboxPos;//new Vector3(K.FOOD_BOX_POS_X, K.FOOD_BOX_POS_Y, 0);
        }
    }
    public void TestFunction()
    {
        SpawnAnimal(Id.World_Animal_Cow);
        SpawnAnimal(Id.World_Animal_Chicken);
        SpawnAnimal(Id.World_Animal_Sheep);
        SpawnAnimal(Id.World_Animal_Ostrich);
        SpawnAnimal(Id.World_Animal_Horse);
        SpawnAnimal(Id.World_Animal_Duck);
        SpawnAnimal(Id.World_Animal_Pig);
        SpawnAnimal(Id.World_Animal_Goat);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // IFoodProvider 로 약속한 함수.
    // 동물에게 먹이통의 위치를 알려주는 함수이다.
    public Vector3 GetFoodBoxPosition()
    {
        return _foodBoxPos;
    }
    public FoodBox GetFoodBox()
    {
        return _foodBox;
    }

    //동물의 ID를 입력하여 객체를 소환하는 기능
    [ContextMenu("AnimalSpawnTest")]
    public void SpawnAnimal(string id)
    {
        GameObject tempGo;
        if (id == null)
        {
            tempGo = FactoryManager.Ins.Animal.Spawn(_testId);
        }
        else
        {
            tempGo = FactoryManager.Ins.Animal.Spawn(id);
        }

        AnimalObject tempAnimal = tempGo.GetComponent<AnimalObject>();
        if (tempAnimal == null)
        {
            UDebug.Print("잘못된 객체가 생성되고 있습니다. 여기에는 AnimalObject가 반환되어야 합니다. 팩토리 확인");
            return;
        }

        _animals.Add(tempAnimal);
        tempAnimal.SetFoodProvider(this);

        tempGo.transform.SetParent(this.transform);
        tempGo.transform.localPosition = GetRandomPos();
    }

    //소환되는 동물의 위치를 사육장 내 랜덤한 위치로 조정하기 위함.
    private Vector3 GetRandomPos()
    {
        float posX = Random.Range(-_witdh / 2+2, _witdh / 2 - 2);
        float posY = Random.Range(-_height / 2+2, _height / 2 - 2);
        return new Vector3(posX, posY);
    }

    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion
    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
