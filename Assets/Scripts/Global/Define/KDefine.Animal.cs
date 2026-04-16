#pragma warning disable IDE1006

public static partial class K
{
    // 개체 수 제한
    public static readonly int ANIMAL_MAX_COUNT = 200;
    public static readonly float ANIMAL_MAX_HUNGER = 100.0f;        //최대 만복도
    public static readonly float ANIMAL_HUNGER_CONDITION = 30.0f;   //배고픔을 느끼는 조건
    public static readonly float ANIMAL_ACTION_INTERVAL = 3.0f;     //동물이 몇 초마다 랜덤 액션을 시도하는가
    public static readonly float ANIMAL_TICK_INTERVAL = 20.0f;       //몇 초에 한 번 동물의 Tick이 발생하는가
    public static readonly float FOOD_BOX_POS_X = 0;
    public static readonly float FOOD_BOX_POS_Y = 4.5f;
    //public static readonly int MAX_PRODUCT_PROGRESS = 3;
}
