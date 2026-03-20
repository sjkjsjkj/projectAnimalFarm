using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class ObjectPool
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Queue<PoolItem> _queue;
    private int _initCapacity;
    private bool _useOverFlow;
    private PoolItem _item;
    #endregion

    #region ─────────────────────────▶　 생성자  ◀─────────────────────────
    #endregion
    public ObjectPool(int capacity, PoolItem item, bool useOverFlow = true)
    {
        _queue = new Queue<PoolItem>();

        _initCapacity = capacity;
        _item = item;
        _useOverFlow = useOverFlow;


        AddPool(_initCapacity);
    }
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public Queue<PoolItem> Pool => _queue;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void AddPool(int capacity)
    {
        for(int i=0; i< capacity; i++)
        {
            PoolItem tempPoolItem = Object.Instantiate(_item,PoolManager.Ins.transform);

            tempPoolItem.OnDead -= ReCollect;
            tempPoolItem.OnDead += ReCollect;

            _queue.Enqueue(tempPoolItem);
        }
    }
    public PoolItem Get()
    {
        if (_queue.Count <= 0)
        {
            if(!_useOverFlow)
            {
                UDebug.Print("$풀이 비어있습니다. 해당 풀은 초과 생산을 허용하지 않습니다.");
                return null;
            }
            else
            {
                AddPool(10);
            }
        }

        PoolItem tempItem = _queue.Dequeue();
        tempItem.gameObject.SetActive(true);

        return tempItem;
    }
    public void ReCollect(PoolItem item)
    {
        item.gameObject.SetActive(false);
        _queue.Enqueue(item);
        //TODO : 풀의 크기 비교하며 최대치를 낮춤.
    }
    #endregion
}
