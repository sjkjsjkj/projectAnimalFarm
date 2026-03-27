using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class StringIdGenerator : EditorWindow
{
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    [MenuItem("Tools/문자열 ID 생성기")]
    public static void ShowWindow()
    {
        GetWindow<StringIdGenerator>("String ID Generator");
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 진입점
    private static void EntryIdGenerator(Type soType)
    {
        string[] guids = AssetDatabase.FindAssets($"t:{soType.Name}"); // 해당 타입의 모든 에셋 GUID 수집
        if (guids == null || guids.Length == 0)
        {
            UDebug.Print($"타입({soType})에 해당하는 에셋이 존재하지 않습니다.", LogType.Warning);
            return;
        }
        UDebug.Print($"타입({soType})의 GUID를 {guids.Length}개 수집했습니다.");
        // 스크립트에 작성할 문자열 생성
        var soList = BuildSoList(soType, guids);
        StringBuilder sb = BuildScript(soType, soList);
        // 스크립트 파일 생성
        if (sb == null)
        {
            UDebug.Print($"문자열 생성을 실패했습니다.", LogType.Assert);
        }
        CreateScriptFile(sb, soType);
    }

    // 타입이 일치하는 모든 SO를 긁어옵니다.
    private static List<ScriptableObject> BuildSoList(Type soType, string[] guids)
    {
        List<ScriptableObject> soList = new();
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
            soList.Add(so);
        }
        return soList;
    }

    // 각 SO 에셋의 ID를 읽어서 스크립트 파일 내용 생성
    private static StringBuilder BuildScript(Type soType, List<ScriptableObject> soList)
    {
        StringBuilder sb = new();
        sb.AppendLine("#pragma warning disable IDE1006");
        sb.AppendLine("");
        sb.AppendLine("public static partial class Id");
        sb.AppendLine("{");
        // 상수 작성 시작
        int length = soList.Count;
        // ID 필드 가져오기
        FieldInfo idField = soType.GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic);
        if (UDebug.IsNull(idField))
        {
            return null;
        }
        // 해당 타입의 SO 에셋 모두 순회
        for (int i = 0; i < length; ++i)
        {
            object id = idField.GetValue(soList[i]);
            string safeId = id.ToString().Replace(" ", "_");
            sb.AppendLine($"    public const string {safeId} = \"{id}\";");
        }
        // 상수 작성 끝
        sb.AppendLine("}");
        return sb;
    }

    // 경로에 스크립트 파일 생성
    private static void CreateScriptFile(StringBuilder sb, Type soType)
    {
        string name = soType.Name;
        string path = $"{K.STRING_ID_EXPORT_PATH}/IdDefine.{name}.cs";
        // 파일 내용이 동일한지 검사
        // 동일할 때 덮어씌울 경우 에러 로그 도배 발생
        string newCode = sb.ToString();
        if (File.Exists(path))
        {
            string oldCode = File.ReadAllText(path);
            if(newCode == oldCode)
            {
                UDebug.Print($"{name}.cs의 내용이 동일하므로 작업을 생략했습니다.");
                return;
            }
        }
        // StreamWriter로 파일 쓰기
        using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
        {
            sw.Write(newCode);
            UDebug.Print($"경로({K.STRING_ID_EXPORT_PATH})에 {name}을 작성했습니다.");
            AssetDatabase.Refresh(); // 파일을 코드로 생성했음을 알려주기
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnGUI()
    {
        Type soType = null;
        GUILayout.Label("▷ 아이템 SO ◁", EditorStyles.boldLabel);
        if (GUILayout.Button("미끼 ID")) soType = typeof(BaitItemSO);
        if (GUILayout.Button("수확물 ID")) soType = typeof(HarvestItemSO);
        if (GUILayout.Button("씨앗 ID")) soType = typeof(SeedItemSO);
        if (GUILayout.Button("먹이 ID")) soType = typeof(FeedItemSO);
        if (GUILayout.Button("특수 ID")) soType = typeof(SpecialItemSO);
        if (GUILayout.Button("도구 ID")) soType = typeof(ToolItemSO);
        GUILayout.Label("▷ 월드 SO ◁", EditorStyles.boldLabel);
        if (GUILayout.Button("동물 ID")) soType = typeof(AnimalWorldSO);
        if (GUILayout.Button("작물 ID")) soType = typeof(CropWorldSO);
        if (GUILayout.Button("물고기 ID")) soType = typeof(FishWorldSO);
        if (GUILayout.Button("NPC ID")) soType = typeof(NpcWorldSO);
        GUILayout.Label("▷ 사운드 SO ◁", EditorStyles.boldLabel);
        if (GUILayout.Button("오디오 ID")) soType = typeof(SoundSO);
        // 버튼을 누르지 않았음
        if (soType == null)
        {
            return;
        }
        // 하드코딩했으므로 그럴 일 없겠지만 혹시나 검사
        if (soType.IsSubclassOf(typeof(ScriptableObject)))
        {
            UDebug.Print($"문자열 Id 스크립트 생성을 시작합니다. (대상 : {soType.Name})");
            EntryIdGenerator(soType);
        }
        else
        {
            UDebug.Print($"등록한 클래스가 스크립터블 오브젝트를 상속받지 않습니다.", LogType.Assert);
        }
    }
    #endregion
}
