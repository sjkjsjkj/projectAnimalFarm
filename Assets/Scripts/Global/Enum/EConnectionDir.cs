/// <summary>
/// 타일 주변들과 비교하여 같은 상태인 타일들과 연결되어있는 상태를 나타내는 열거형
/// ex : 해당 타일 주변의 경작된 땅을 찾고, 있다면 그 방향과 연결된 상태를 나타내게 함.
/// </summary>
public enum EConnectionDir
{
    None =0,
    Up = 1<<0,           // 1
    Down = 1<<1,          // 2
    Left = 1<<2,          // 4 
    Right = 1<<3        // 8
}


// UPLeft = 0101            5
// UPRight = 1001           9
// UPLeftright = 1101       13
// UPLeftDown = 0111        7
// UpRightDown = 1011       11
// DownLeft = 0110          6
// DownRight= 1010          10
// DownLeftRight = 1110     14
// LeftRight = 1100         12
// UpDown = 0011            3
// ALL = 1111               15
