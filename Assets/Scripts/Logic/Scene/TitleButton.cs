using System.IO;
using UnityEngine;

/// <summary>
/// 타이틀 씬 스크립트
/// </summary>
public class TitleButton : BaseMono
{
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void GameStart()
    {
        UDebug.Print($"게임 스타트 버튼을 클릭했습니다.");
        ClearAllSaveData();
        DataManager.Ins.ResetGameData();
        InventoryManager.Ins.ClearAllInventories();
        // 도감
        PictorialBookSystem pictorialSystem = FindAnyObjectByType<PictorialBookSystem>();
        if (pictorialSystem != null)
        {
            
        }
        // 씬 로드
        var game = GameManager.Ins;
        game.LoadSceneAsyncWithFade((int)EScene.Main, 0f , 3f, 3f);
        game.IsPlayerWakeUp = true;
        USound.PlaySfx(Id.Sfx_Ui_Select_04);
    }

    public void GameLoad()
    {
        UDebug.Print($"게임 로드 버튼을 클릭했습니다.");
        if (HasSaveData())
        {
            EScene targetScene = DataManager.Ins.Player.CurPlayerScene();
            GameManager.Ins.LoadSceneAsyncWithFade((int)targetScene);
            USound.PlaySfx(Id.Sfx_Ui_Join);
        }
        else
        {
            USound.PlaySfx(Id.Sfx_Ui_Select_05);
        }
    }

    public void GameEnd()
    {
        USound.PlaySfx(Id.Sfx_Ui_Select_08);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 플레이 모드 종료
#else
        Application.Quit(); // 게임 종료
#endif
    }
    #endregion

    private static void ClearAllSaveData()
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

    private static bool HasSaveData()
    {
        string persistentPath = Application.persistentDataPath;
        if (Directory.Exists(persistentPath))
        {
            if (File.Exists($"{persistentPath}/PlayerProvider.json"))
            {
                return true;
            }
        }
        return false;
    }
}
