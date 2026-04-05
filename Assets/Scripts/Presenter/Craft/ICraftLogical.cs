/// <summary>
/// 제작 로직이 UI와 소통하기 위해 준수해야 할 규격
/// </summary>
public interface ICraftLogical
{
    /// <summary>
    /// 해당 레시피의 재료를 얼마나 가지고 있는지 반환하는 함수
    /// </summary>
    WorkbenchReturnStruct[] GetMaterials(string recipeId);

    /// <summary>
    /// 아이템 제작 요청이 들어왔을 경우 처리하여 제작 성공 여부를 반환하는 함수
    /// </summary>
    bool TryCraftItem(string recipeId, out string message);

}
