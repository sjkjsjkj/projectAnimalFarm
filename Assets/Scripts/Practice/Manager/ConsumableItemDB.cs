using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class ConsumableItemDB : BaseMono
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Dictionary<string, string[]> _dataList;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion


    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
   
    public void MakeDB(string url)
    {
        StartCoroutine(CoSetDB(url));
    }
    private void SetDB(string sheetData)
    {
        string[] rows = sheetData.Split('\n');

        Debug.Log($"rows count : {rows.Length}");

        for (int i = 0; i < rows.Length; i++)
        {
            string[] cols = rows[i].Split('\t');
            for (int j=0; j<cols.Length; j++)
            {
                //여기서 itemSo 구축
            }
            Debug.Log($"rows i : {rows[i]}");
            Debug.Log($"cols : {cols}");
            _dataList.Add(rows[i], cols); // 여기에 만든 itemSo 넘김
        }
    }
    private IEnumerator CoSetDB(string url)
    {
        string sheetData="";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if(www.isDone)
            {
                sheetData = www.downloadHandler.text;
            }
            SetDB(sheetData);
        }
    }
    public string[] FindData(string id)
    {
        return _dataList[id];
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
    {
        _dataList = new Dictionary<string, string[]>();
    }
    #endregion
}
