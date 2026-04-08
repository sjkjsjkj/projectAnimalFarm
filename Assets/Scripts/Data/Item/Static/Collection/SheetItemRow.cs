/// <summary>
/// 구글 시트의 한 줄 데이터를 런타임에서 담아두는 클래스.
/// 
/// 이 클래스의 역할
/// 1. GoogleSheetTsvLoader가 한 줄씩 파싱한 결과를 저장한다.
/// 2. SheetItemDatabase가 이 데이터를 보관한다.
/// 3. 낚시 / 도감 / UI가 공통으로 참조한다.
/// 
/// 현재 사용하는 기본 데이터
/// - id               : 고유 ID
/// - name             : 표시 이름
/// - category         : Animal / Fish / Gather
/// - rarity           : Basic / Prime / Rare / Legendary 등
/// - description      : 설명
/// - sellPrice        : 판매 금액
/// - buyPrice         : 구매 금액
/// - iconKey          : 아이콘 스프라이트를 찾을 때 쓸 키
/// - isWaterFish      : 담수 낚시 가능 여부
/// - isDeepWaterFish  : 깊은물 / 바다 낚시 가능 여부
/// 
/// 중요
/// - Fish가 아닌 데이터는 isWaterFish / isDeepWaterFish가 false여도 상관없다.
/// - 이 클래스는 데이터 보관용이므로 로직은 최대한 넣지 않는다.
/// - sellPrice / buyPrice는 값이 없으면 -1 로 저장한다.
/// </summary>
public class SheetItemRow
{
    /// <summary>
    /// 아이템 고유 ID
    /// 예: All Fish_0
    /// </summary>
    public string id;

    /// <summary>
    /// 아이템 표시 이름
    /// 예: 붕어
    /// </summary>
    public string name;

    /// <summary>
    /// 카테고리
    /// 예: Fish / Gather / Animal
    /// </summary>
    public string category;

    /// <summary>
    /// 희귀도
    /// 예: Basic / Prime / Rare / Legendary
    /// </summary>
    public string rarity;

    /// <summary>
    /// 아이템 설명
    /// </summary>
    public string description;

    /// <summary>
    /// 판매 금액
    /// 값이 없으면 -1
    /// </summary>
    public int sellPrice;

    /// <summary>
    /// 구매 금액
    /// 값이 없으면 -1
    /// </summary>
    public int buyPrice;

    /// <summary>
    /// 아이콘 키
    /// 보통은 id와 동일하게 써도 된다.
    /// </summary>
    public string iconKey;

    /// <summary>
    /// 담수 낚시 가능 여부
    /// Fish 카테고리에서 "물" 컬럼을 읽어온 값
    /// </summary>
    public bool isWaterFish;

    /// <summary>
    /// 깊은물 / 바다 낚시 가능 여부
    /// Fish 카테고리에서 "깊은물" 컬럼을 읽어온 값
    /// </summary>
    public bool isDeepWaterFish;

    /// <summary>
    /// SheetItemRow 생성자
    /// GoogleSheetTsvLoader가 TSV를 파싱한 뒤
    /// 이 생성자를 통해 한 줄 데이터를 객체로 만든다.
    /// </summary>
    public SheetItemRow(
        string id,
        string name,
        string category,
        string rarity,
        string description,
        int sellPrice,
        int buyPrice,
        string iconKey,
        bool isWaterFish = false,
        bool isDeepWaterFish = false)
    {
        this.id = id;
        this.name = name;
        this.category = category;
        this.rarity = rarity;
        this.description = description;
        this.sellPrice = sellPrice;
        this.buyPrice = buyPrice;
        this.iconKey = iconKey;
        this.isWaterFish = isWaterFish;
        this.isDeepWaterFish = isDeepWaterFish;
    }
}
