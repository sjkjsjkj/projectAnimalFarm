using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데이터베이스들의 베이스가 되어줄 스크립트 입니다.
/// 제네릭에는 형식에 따라 구조체, SO를 넣어주시면 됩니다.
/// </summary>
public class DatabaseSO<T> where T : DatabaseUnitSO
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Dictionary<string, T> _dataList;
    #endregion

    #region ─────────────────────────▶  생성자  ◀─────────────────────────
    public DatabaseSO()
    {
        _dataList = new Dictionary<string, T>();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void MakeDB(string path)
    {
        
        SetDatabaseByPath(path);
        
    }
    private void SetDatabaseByPath(string path)
    {
        T[] datas = Resources.LoadAll<T>(path);

        UDebug.Print($"data's length : {datas.Length}");
        UDebug.IsNull(_dataList);
        for(int i=0; i< datas.Length; i++)
        {
            _dataList.Add(datas[i].Id, datas[i]);
        }
    }
    public T FindData(string id)
    {
        return _dataList[id];
    }
    #endregion
}
