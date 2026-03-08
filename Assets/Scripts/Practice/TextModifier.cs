using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 빈 오브젝트에 부착하는 C# 스크립트입니다.
/// </summary>
public class TextModifier : MonoBehaviour
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("필수 요소 등록")]
    [SerializeField] private TextMeshProUGUI tmp;
    #endregion

    private void Awake()
    {
        List<string> message = new List<string>();
        message.Add("안녕하세요 !");
        message.Add("ㅎㅎㅎ");
        message.Add("ㅎㅎㅎ");

        for (int i = 0; i < message.Count; ++i)
        {
            tmp.SetText(message[i]);
        }
    }
}