using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class CsvExporterBySO : EditorWindow
{
    private MonoScript _soScript;
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    [MenuItem("Tools/Build SO.csv")]
    public static void ShowWindow()
    {
        GetWindow<CsvExporterBySO>("SO Script.cs → *.csv");
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 진입점
    private static void EntryCsvBuilder(Type soType)
    {
        string[] guids = AssetDatabase.FindAssets($"t:{soType.Name}"); // 해당 타입의 모든 에셋 GUID 수집
        if (guids == null || guids.Length == 0)
        {
            UDebug.Print($"타입({soType})에 해당하는 에셋이 존재하지 않습니다.", LogType.Assert);
            return;
        }
        UDebug.Print($"타입({soType})의 GUID를 {guids.Length}개 수집했습니다.");
        // 타입 수집하기
        var fieldList = GetFieldTypes(soType);
        StringBuilder sb = new();
        InputFieldTypes(sb, fieldList);
        InputFields(sb, soType, guids, fieldList);
        CreateCsvFile(sb, soType);
    }

    // 수집할 필드 타입이 담긴 배열을 반환
    private static List<FieldInfo> GetFieldTypes(Type soType)
    {
        // 리플렉션으로 해당 클래스 변수 긁어오기
        FieldInfo[] fields = soType.GetFields // 내부 변수만 가져오기
            (BindingFlags.Instance | BindingFlags.NonPublic);
        // 변수 준비
        int length = fields.Length;
        List<FieldInfo> fieldList = new(length);
        int valid = 0;
        UDebug.Print($"타입({soType})의 필드를 {length}개 수집했습니다.");
        // 긁어온 필드 타입 순회
        for (int i = 0; i < length; ++i)
        {
            // 어트리뷰트를 명시해둔 변수라면 거르기
            if (Attribute.IsDefined(fields[i], typeof(CsvIgnoreAttribute)))
            {
                UDebug.Print($"{i}번째 타입({fields[i].Name})은 거부 어트리뷰트를 명시했습니다.");
                continue;
            }
            // 수집 완료
            fieldList.Add(fields[i]);
            valid++;
            UDebug.Print($"{i}번째 타입({fields[i].Name})을 수집했습니다.");
        }
        UDebug.Print($"타입({soType})에서 유효한 필드 타입 {valid}개를 수집했습니다.");
        return fieldList;
    }

    // 해당 클래스의 필드를 StringBuilder에 채웁니다. (헤더)
    private static void InputFieldTypes(StringBuilder sb, List<FieldInfo> fieldList)
    {
        // 긁어온 필드 타입 순회
        int length = fieldList.Count;
        for (int i = 0; i < length; ++i)
        {
            // 헤더 채우기
            sb.Append(fieldList[i].Name);
            sb.Append(',');
        }
        sb.Remove(sb.Length - 1, 1); // 마지막 콤마는 제거
        sb.AppendLine();
        UDebug.Print($"Csv 헤더를 모두 채웠습니다.");
    }

    // 타입이 일치하는 모든 SO를 긁어와서 StringBuilder에 채웁니다.
    private static void InputFields(StringBuilder sb, Type soType, string[] guids, List<FieldInfo> fieldList)
    {
        // 검색된 모든 GUID(SO 파일)을 순회
        for (int i = 0; i < guids.Length; ++i)
        {
            // GUID를 실제 파일 경로로 변환
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            // 경로에 있는 에셋(SO 스크립트)을 로드하여 SO 객체로 가져오기
            ScriptableObject so = AssetDatabase.LoadAssetAtPath(path, soType) as ScriptableObject;
            if (so == null)
            {
                UDebug.Print($"알 수 없는 오류 : 경로({path})에서 SO를 로드했지만 비어있습니다.", LogType.Assert);
                continue;
            }
            // 모아둔 필드 타입 순회
            int length = fieldList.Count;
            for (int j = 0; j < length; ++j)
            {
                // 객체에서 해당 필드 타입 가져와서 문자열 추가
                object fieldValue = fieldList[j].GetValue(so);
                sb.Append(fieldValue);
                sb.Append(',');
            }
            sb.Remove(sb.Length - 1, 1); // 마지막 콤마는 제거
            sb.AppendLine();
            UDebug.Print($"{so.name}의 필드 {length}개를 모두 수집했습니다.");
        }
    }

    // 경로에 csv 파일 생성
    private static void CreateCsvFile(StringBuilder sb, Type soType)
    {
        // StreamWriter로 파일 쓰기
        string fileName = $"{K.AUTO_SO_EXPORT_PATH}/{soType.Name}.csv";
        using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
        {
            sw.Write(sb);
            UDebug.Print($"경로({K.AUTO_SO_EXPORT_PATH})에 {fileName}를 작성했습니다.");
            AssetDatabase.Refresh(); // 파일을 코드로 생성했음을 알려주기
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnGUI()
    {
        GUILayout.Label("SO → csv", EditorStyles.boldLabel); // 제목
        _soScript = (MonoScript)EditorGUILayout.ObjectField("SO 스크립트", _soScript, typeof(MonoScript), false);
        // 버튼이 클릭된 순간 true
        if (GUILayout.Button("버튼") && _soScript != null)
        {
            Type soType = _soScript.GetClass(); // 스크립트의 클래스 타입 가져오기
            // 스크립터블 오브젝트의 자식 클래스인지
            if (soType != null && soType.IsSubclassOf(typeof(ScriptableObject)))
            {
                EntryCsvBuilder(soType);
            }
            else
            {
                UDebug.Print($"등록한 클래스가 스크립터블 오브젝트를 상속받지 않습니다.", LogType.Assert);
            }
        }
    }
    #endregion
}
