using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 타일 시트 데이터를 가져와서 딕셔너리화합니다.
/// </summary>
public class TileDatabase : GlobalSingleton<TileDatabase>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Dictionary<int, ETileState> _tileDict = new();
    private Dictionary<int, string> _tileID = new();
    private Dictionary<int, string> _tileName = new();
    private bool _isInitialized = false;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        MakeDatabase();
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 진입점
    private void MakeDatabase()
    {
        StartCoroutine(CoLoadDatabase(K.TILE_URL));
    }

    // 구글 스프레드 시트에서 문자열을 가져옵니다.
    private IEnumerator CoLoadDatabase(string url)
    {
        string sheetData = "";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            // 로드 완료
            if (www.isDone)
            {
                sheetData = www.downloadHandler.text;
            }
            BuildDatabase(sheetData);
        }
    }

    // 문자열을 받아서 데이터베이스를 빌드합니다.
    private void BuildDatabase(string sheetData)
    {
        string[] rows = sheetData.Split('\n');
        UDebug.Print($"타일 개수 : {rows.Length}");
        // 세로 행 순회
        for (int i = 0; i < rows.Length; i++)
        {
            // ID

            // 이름

            // 속성 구하기
            string[] cols = rows[i].Split('\t');
            ETileState state = BuildTileState(cols);
            UDebug.Print($"{i}번째 타일 : {rows[i]}");
            UDebug.Print($"타일 속성 : {state}");
            _tileDict.Add(rows[i].GetHashCode(), state);
        }
    }

    // 열 데이터를 읽어서 타일 속성 빌드
    private ETileState BuildTileState(string[] cols)
    {
        int state = 0;
        int length = cols.Length;
        for (int i = 2; i < length; i++)
        {
            if (int.TryParse(cols[i], out int result))
            {
                state |= result;
                UDebug.Print($"속성 발견 : {i}");
            }
            else
            {
                UDebug.Print($"타일 속성 값을 읽는 도중 변환할 수 없는 값을 받았습니다.", LogType.Assert);
            }
        }
        return (ETileState)state;
    }
    #endregion
}
