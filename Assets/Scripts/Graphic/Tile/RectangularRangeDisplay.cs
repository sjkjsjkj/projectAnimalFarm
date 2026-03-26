using UnityEngine;
/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class RectangularRangeDisplay : BaseMono
{
    [Header("맵 길이 설정")]
    [SerializeField] private int _width = 200;
    [SerializeField] private int _height = 200;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 topLeft = new Vector3(0, _height, 0);
        Vector3 topRight = new Vector3(_width, _height, 0);
        Vector3 botLeft = new Vector3(0, 0, 0);
        Vector3 botRight = new Vector3(_width, 0, 0);
        Gizmos.DrawLine(botLeft, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, botRight);
        Gizmos.DrawLine(botRight, botLeft);
    }
}
