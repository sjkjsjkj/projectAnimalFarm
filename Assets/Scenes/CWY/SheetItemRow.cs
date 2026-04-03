/// <summary>
/// 구글 시트의 한 줄 데이터를 런타임에서 담아두는 클래스
/// 인벤토리 / 도감 / UI가 공통으로 참조하는 기본 데이터
/// </summary>
public class SheetItemRow
{
    #region ─────────────────────────▶ 기본 데이터 ◀─────────────────────────
    public string id;               // 고유 ID (EX : River_1) 
    public string name;             // 표시 이름
    public string category;         // Gater / Animal / Fish
    public string rarity;           // Basic / Prime 등
    public string iconKey;          // 스프라이트 매칭용 키
    #endregion

    #region ─────────────────────────▶ 선택 데이터 ◀─────────────────────────
    public bool isWaterFish;        // 물고기 시트 전용
    public bool isDeepWaterFish;
    #endregion

    #region ─────────────────────────▶ 생 성 자 ◀─────────────────────────
    public SheetItemRow(
        string id,
        string name,
        string category,
        string rarity,
        string iconKey,
        bool isWaterFish = false,
        bool isDeepWaterFish = false)
    {
        this.id = id;
        this.name = name;
        this.category = category;
        this.rarity = rarity;
        this.iconKey = iconKey;
        this.isWaterFish = isWaterFish;
        this.isDeepWaterFish = isDeepWaterFish;
    }
    #endregion

}
