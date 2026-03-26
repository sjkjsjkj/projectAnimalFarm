using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class UGoogleSheetReader : MonoBehaviour
{
    private string _sheetData;
    //public Text _displayText;
    private const string GOOGLE_SHEET_URL =
        "https://docs.google.com/spreadsheets/d/12dSRC2d-tfEtGsveerWi7wymgL4xyFlt1byC9EE0-to/export?format=tsv&range=A2:B";

    
    private IEnumerator Start()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(GOOGLE_SHEET_URL))
        {
            yield return www.SendWebRequest();

            if(www.isDone)
            {
                _sheetData = www.downloadHandler.text;
            }
            ReadText();
        }    
    }
    private void ReadText()
    {
        string[] rows = _sheetData.Split('\n');
        string[] columns = rows[0].Split('\t');

        for (int i = 0; i < columns.Length; i++)
        {
            UDebug.Print($"{columns[i]}");
        }
    }
}
