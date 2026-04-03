using System.IO;
using UnityEditor;
using UnityEngine;

public static class SaveDataClear
{
    [MenuItem("Tools/세이브 데이터 전체 삭제")]
    public static void ShowWindow()
    {
        ClearAllSaveData();
    }

    // 인게임 내 옵션 창이나 디버그 버튼에 연결할 때 쓰는 퍼블릭 메서드
    public static void ClearAllSaveData()
    {
        string persistentPath = Application.persistentDataPath;

        // 오브젝트 폴더 정리
        string objectDataPath = $"{persistentPath}/{K.PRESISTENT_OBJECT_PATH}";
        if (Directory.Exists(objectDataPath))
        {
            Directory.Delete(objectDataPath, true); // 하위 폴더까지 모두 삭제
            UDebug.Print($"오브젝트 폴더 삭제 완료: {objectDataPath}");
        }
        // 데이터 정리
        string[] jsonFiles = Directory.GetFiles(persistentPath, "*.json");
        foreach (string file in jsonFiles)
        {
            File.Delete(file);
            UDebug.Print($"글로벌 데이터 삭제 완료: {Path.GetFileName(file)}");
        }
        UDebug.Print($"세이브 데이터를 모두 삭제했습니다.");
    }
}
