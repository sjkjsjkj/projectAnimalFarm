/// <summary>
/// Json 파일을 만들기 위해 임시로 데이터를 담을 클래스
/// </summary>
[System.Serializable]
public class TileIdMapSaveData
{
    public int width;
    public int height;
    public int startX;
    public int startY;
    public int[] tiles;

    public TileIdMapSaveData(int width, int height, int startX, int startY, int[] tiles)
    {
        this.width = width;
        this.height = height;
        this.startX = startX;
        this.startY = startY;
        this.tiles = tiles;
    }
}
