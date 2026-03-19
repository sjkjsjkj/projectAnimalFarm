using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 타일의 데이터베이스 입니다.
/// </summary>
public class DatabaseTile : BaseMono
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Dictionary<int, TileData> _dataList;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void MakeDB(string url)
    {
        _dataList = new Dictionary<int, TileData>();
        StartCoroutine(SetDatabaseByURL(url));
        
    }
  
    private IEnumerator SetDatabaseByURL(string url)
    {
        string sheetData="";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.isDone)
            {
                sheetData = www.downloadHandler.text;
            }
            SetDB(sheetData);
        }
    }
    private void SetDB(string sheetData)
    {
        if (string.IsNullOrEmpty(sheetData))
        {
            UDebug.Print($"DB URL Error", LogType.Warning);
            return;
        }

        string[] row = sheetData.Split('\n');
        for (int i = 0; i < row.Length; i++)
        {
            string[] col = row[i].Split('\t');
            int id = int.Parse(col[0]);
            string name = col[1];
            int size = int.Parse(col[2]);
            uint state = SetTileState(col);
            TileData tempTileDataStruct = new TileData(id, name, size, state);

            _dataList.Add(id, tempTileDataStruct);
        }
    }
    private uint SetTileState(string[] tileInfo)
    {
        uint state = 0;
        for(int i=3;i< tileInfo.Length;i++)
        {
            if (tileInfo[i]=="1")
            {
                state |= (uint)1 << (i - 3);
            }
        }
        return state;
    }
    public TileData FindData(int id)
    {
        return _dataList[id];
    }

    #endregion
    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
    {
        _dataList = new Dictionary<int, TileData>();
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
