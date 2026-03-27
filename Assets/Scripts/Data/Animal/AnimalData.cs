using System;

/// <summary>
/// 동물의 데이터들을 담고있는 클래스입니다.
/// Monobehaviour 가 필요하지 않는 내부 데이터들 계산과 처리들을 담당합니다.
/// 이 클래스에서 계산된 결과로 Object에게 상태변화를 요청합니다.
/// </summary>
public class AnimalData
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private string _animalName;
    private int _age;

    private bool _needFood;           // 음식을 먹어야 하는 동물인지
    private float _hunger;            // 현재 허기짐 정도
    private float _foodConsumeAmount; // 음식을 얼마나 섭취하는지 (1틱당 소모되는 허기짐)
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string Name => _animalName;
    public int Age => _age;
    public event Action OnHungry;
    #endregion

    #region ─────────────────────────▶ 생성자 ◀─────────────────────────
    public AnimalData(AnimalSO dataSO)
    {
        _animalName = dataSO.Name;
        _age = 0;
        _needFood = true;
        _foodConsumeAmount = dataSO.FoodConsumeAmount;
        _hunger = 40.0f;
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
        if(_needFood)
        {
            UDebug.Print($"before hunger = {_hunger}");
            _hunger = MathF.Max(0, _hunger - _foodConsumeAmount);
            UDebug.Print($"after hunger = {_hunger}");

            if (_hunger <= 0)
            {
                Dead();
                return;
            }
            
            if(_hunger <= K.ANIMAL_HUNGER_CONDITION)
            {
                UDebug.Print($"{Name} is so Hungry");
                OnHungry?.Invoke();
            }
           
        }
    }
    // 여러가지의 이유로 동물이 죽었을 때 호출되는 메서드
    public void Dead()
    {
        //To Do :자원 반납 및 연결 해제.
    }
   
    #endregion
    
}
