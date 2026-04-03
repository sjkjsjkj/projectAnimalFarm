using System;
using UnityEngine;
/// <summary>
/// 동물의 데이터들을 담고있는 클래스입니다.
/// Monobehaviour 가 필요하지 않는 내부 데이터들 계산과 처리들을 담당합니다.
/// 이 클래스에서 계산된 결과로 Object에게 상태변화를 요청합니다.
/// </summary>
[System.Serializable]
public class AnimalData
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    [SerializeField] private string _id;
    [SerializeField] private string _animalName;
    [SerializeField] private int _age;
                     
    [SerializeField] private bool _needFood;           // 음식을 먹어야 하는 동물인지
    [SerializeField] private float _hunger;            // 현재 허기짐 정도
    [SerializeField] private float _foodConsumeAmount; // 음식을 얼마나 섭취하는지 (1틱당 소모되는 허기짐)
    [SerializeField] private int _productProgress;
    [SerializeField] private Vector2 _size;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string Id => _id;
    public string Name => _animalName;
    public int Age => _age;
    public Vector2 Size => _size;
    public int ProductProgress => _productProgress;

    public event Action OnHungry;
    public event Action OnProductFinish;
    #endregion

    #region ─────────────────────────▶ 생성자 ◀─────────────────────────
    public AnimalData()
    {

    }
    public AnimalData(AnimalWorldSO dataSO)
    {
        _id = dataSO.Id;
        _animalName = dataSO.Name;
        _age = 0;
        _needFood = true;
        _foodConsumeAmount = dataSO.TickFeedAmount;
        _hunger = 40.0f;
        _productProgress = 0;
    }
    public AnimalData(AnimalData data)
    {
        _id = data.Id;
        _animalName = data.Name;
        _age = data.Age;
        _needFood = true;
        _foodConsumeAmount = data._foodConsumeAmount;
        _hunger = data._hunger;
        _productProgress = data._productProgress;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void EatFood(float hunger)
    {
        UDebug.IsTrue(hunger == 0);

        _hunger += hunger;
    }
    // 틱당 처리되는 데이터
    // 오브젝트에서 특정 프레임마다 불러옴.
    public void Tick()
    {
        if (_needFood)
        {
            UDebug.Print($"before hunger = {_hunger}");
            _hunger = MathF.Max(0, _hunger - _foodConsumeAmount);
            UDebug.Print($"after hunger = {_hunger}");

            if (_hunger <= 0)
            {
                Dead();
                return;
            }

            if (_hunger <= K.ANIMAL_HUNGER_CONDITION)
            {
                UDebug.Print($"{Name} is so Hungry");
                OnHungry?.Invoke();
                return;
            }

            if(++_productProgress == K.MAX_PRODUCT_PROGRESS)
            {
                OnProductFinish?.Invoke();
            }
        }
    }
    public void ProductReset()
    {
        _productProgress = 0;
    }
    // 여러가지의 이유로 동물이 죽었을 때 호출되는 메서드
    public void Dead()
    {
        //To Do :자원 반납 및 연결 해제.
    }
   
    #endregion
    
}
