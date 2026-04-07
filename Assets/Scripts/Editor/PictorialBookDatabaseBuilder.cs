using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 기존 ItemSO 에셋들을 스캔해서 도감용 DB를 만드는 에디터 빌더.
/// 
/// 사용 방법
/// 1. PictorialBookDatabaseSO 에셋 선택
/// 2. 소스 폴더 입력
/// 3. Tools/Pictorial Book/Build From Selected Database 실행
/// </summary>
public static class PictorialBookDatabaseBuilder
{
    [MenuItem("Tools/Pictorial Book/Build From Selected Database")]
    private static void BuildFromSelectedDatabase()
    {
        PictorialBookDatabaseSO database = Selection.activeObject as PictorialBookDatabaseSO;

        if (database == null)
        {
            EditorUtility.DisplayDialog(
                "실패",
                "PictorialBookDatabaseSO 에셋을 먼저 선택해주세요.",
                "확인");
            return;
        }

        try
        {
            List<PictorialBookEntry> entries = new List<PictorialBookEntry>();
            HashSet<string> duplicateCheck = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            CollectEntries(database.AnimalSourceFolders, "Animal", entries, duplicateCheck);
            CollectEntries(database.FishSourceFolders, "Fish", entries, duplicateCheck);
            CollectEntries(database.GatherSourceFolders, "Gather", entries, duplicateCheck);

            database.SetEntries(entries);

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "완료",
                $"도감 DB 빌드 완료\n총 엔트리 수: {entries.Count}",
                "확인");

            Debug.Log($"[PictorialBookDatabaseBuilder] 빌드 완료. entries={entries.Count}");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            EditorUtility.DisplayDialog("에러", ex.Message, "확인");
        }
    }

    private static void CollectEntries(
        string[] folders,
        string category,
        List<PictorialBookEntry> result,
        HashSet<string> duplicateCheck)
    {
        if (folders == null || folders.Length == 0)
        {
            return;
        }

        List<string> validFolders = new List<string>();

        for (int i = 0; i < folders.Length; i++)
        {
            string folder = folders[i];

            if (string.IsNullOrWhiteSpace(folder))
            {
                continue;
            }

            if (AssetDatabase.IsValidFolder(folder))
            {
                validFolders.Add(folder);
            }
            else
            {
                Debug.LogWarning($"[PictorialBookDatabaseBuilder] 잘못된 폴더 경로: {folder}");
            }
        }

        if (validFolders.Count == 0)
        {
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", validFolders.ToArray());

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);

            ItemSO itemSo = AssetDatabase.LoadAssetAtPath<ItemSO>(path);
            if (itemSo == null)
            {
                continue;
            }

            SerializedObject serializedObject = new SerializedObject(itemSo);

            string itemId = GetString(serializedObject, "_id");
            string displayName = GetString(serializedObject, "_name");
            Sprite icon = GetSprite(serializedObject, "_image");

            if (string.IsNullOrWhiteSpace(itemId))
            {
                Debug.LogWarning($"[PictorialBookDatabaseBuilder] ID 없는 ItemSO 스킵: {path}");
                continue;
            }

            if (duplicateCheck.Contains(itemId))
            {
                Debug.LogWarning($"[PictorialBookDatabaseBuilder] 중복 ID 스킵: {itemId} / {path}");
                continue;
            }

            duplicateCheck.Add(itemId);

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = itemSo.name;
            }

            PictorialBookEntry entry = new PictorialBookEntry
            {
                itemId = itemId.Trim(),
                displayName = displayName.Trim(),
                icon = icon,
                category = category,
                assetPath = path
            };

            result.Add(entry);
        }
    }

    private static string GetString(SerializedObject serializedObject, string propertyName)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null || property.propertyType != SerializedPropertyType.String)
        {
            return string.Empty;
        }

        return property.stringValue;
    }

    private static Sprite GetSprite(SerializedObject serializedObject, string propertyName)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
        {
            return null;
        }

        return property.objectReferenceValue as Sprite;
    }
}
