using UnityEngine;

/// <summary>
/// 글로벌 캔버스를 부모로 삼기
/// </summary>
public class TestMovedGlobalCanvas : BaseMono
{
    [SerializeField] private bool _rectReset = true;

    private void Start()
    {
        GameObject globalCanvas = UObject.Find(K.NAME_GLOBAL_CANVAS_ROOT);
        if(globalCanvas != null)
        {
            transform.SetParent(globalCanvas.transform);
        }
        //
        var rect = GetComponent<RectTransform>();
        if (_rectReset && rect != null)
        {
            rect.localScale = Vector3.one;
            rect.offsetMin = Vector3.zero;
            rect.offsetMax = Vector3.zero;
        }
    }
}
