/// <summary>
/// 저장될 수 있는 객체
/// </summary>
public interface ISaveable
{
    string UniqueId { get; }
    string SaveData();
    void LoadData(string stateJson);
}
