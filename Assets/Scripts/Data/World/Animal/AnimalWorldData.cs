/// <summary>
/// 동물의 런타임 데이터를 저장하는 구조체
/// </summary>
public struct AnimalWorldData
{
    public string animalId; // 동물 ID
    public float curFoodAmount; // 현재 만복도
    public float curProductTime; // 현재까지 진행된 생산 시간
    public bool isProductReady; // 생산 완료하여 대기중

    public AnimalWorldData(AniamlWorldSO so)
    {
        animalId = so.Id;
        curFoodAmount = so.MaxFeedAmount;
        curProductTime = 0f;
        isProductReady = false;
    }
}

