using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TestAnimalAndSeed : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("코디네이터 연결")]
    [SerializeField] private ItemCollectionCoordinator _itemCoordinatior;
    [SerializeField] private BreedingArea _breedingArea;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void GiveAllSeeds()
    {
        _itemCoordinatior.TryCollectItem(Id.Item_Seed_BlueBerry, 3);
        _itemCoordinatior.TryCollectItem(Id.Item_Seed_Broccoli, 3);
        _itemCoordinatior.TryCollectItem(Id.Item_Seed_Cabbage, 3);
        _itemCoordinatior.TryCollectItem(Id.Item_Seed_Carrot, 3);
        _itemCoordinatior.TryCollectItem(Id.Item_Seed_Cauliflower, 3);
        _itemCoordinatior.TryCollectItem(Id.Item_Seed_GreenOnion, 3);
        _itemCoordinatior.TryCollectItem(Id.Item_Seed_Onion, 3);
        _itemCoordinatior.TryCollectItem(Id.Item_Seed_Potato, 3);
        _itemCoordinatior.TryCollectItem(Id.Item_Seed_Radish, 3);
        _itemCoordinatior.TryCollectItem(Id.Item_Seed_Rice, 3);
        _itemCoordinatior.TryCollectItem(Id.Item_Seed_StrawBerry, 3);
    }

    public void SpawnAllAnimals()
    {
        _breedingArea.SpawnAnimal(Id.World_Animal_Cow);
        _breedingArea.SpawnAnimal(Id.World_Animal_Chicken);
        _breedingArea.SpawnAnimal(Id.World_Animal_Sheep);
        _breedingArea.SpawnAnimal(Id.World_Animal_Ostrich);
        _breedingArea.SpawnAnimal(Id.World_Animal_Horse);
        _breedingArea.SpawnAnimal(Id.World_Animal_Duck);
        _breedingArea.SpawnAnimal(Id.World_Animal_Pig);
        _breedingArea.SpawnAnimal(Id.World_Animal_Goat);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Start()
    {
        _itemCoordinatior = GameObject.Find("Item_Collection_Coordinator").GetComponent<ItemCollectionCoordinator>();
        SpawnAllAnimals();
    }

    private bool _canGetSeeds = true;
    private int _frame = 0;
    private void Update()
    {
        _frame++;
        if(_canGetSeeds && _frame > 100)
        {
            _canGetSeeds = false;
            GiveAllSeeds();
        }
    }
    #endregion
}
