using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 프로젝트 내에 있는 오디오 클립을 모두 수집하여 SoundSO 생성
/// </summary>
public class SoundSOGenerator : EditorWindow
{
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    [MenuItem("Tools/사운드 SO 생성기")]
    public static void ShowWindow()
    {
        GetWindow<SoundSOGenerator>("Sound SO Generator");
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 진입점
    private static void EntrySoundSOGenerator()
    {
        string exportPath = string.Format("{0}/Sounds", K.AUTO_SO_EXPORT_PATH);
        string[] guids = AssetDatabase.FindAssets($"t:AudioClip"); // 오디오 클립 모두 수집
        if (guids == null || guids.Length == 0)
        {
            UDebug.Print($"프로젝트에 오디오 클립이 존재하지 않습니다.", LogType.Warning);
            return;
        }
        if (!Directory.Exists(Path.GetFullPath(exportPath)))
        {
            UDebug.Print($"경로({K.AUTO_SO_EXPORT_PATH})에 폴더가 존재하지 않습니다.", LogType.Error);
            return;
        }
        LoopAudioClips(guids, exportPath);
    }

    // 모든 오디오 클립을 순회하며 메서드 호출
    private static void LoopAudioClips(string[] guids, string exportPath)
    {
        int length = guids.Length;
        int success = 0;
        for (int i = 0; i < length; ++i)
        {
            string guid = guids[i];
            string path = AssetDatabase.GUIDToAssetPath(guid); // 클립의 경로 가져오기
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path); // 클립 가져오기
            if(clip == null)
            {
                UDebug.Print($"{path}에 오디오 클립이 존재하지 않는 알 수 없는 오류 발생", LogType.Assert);
            }
            CreateSoundSO(clip, exportPath);
            success++;
        }
        AssetDatabase.SaveAssets(); // 에셋을 디스크에 저장하기 위한 작업
        AssetDatabase.Refresh();
        UDebug.Print($"사운드 SO {success}개를 생성 완료했습니다.");
    }

    // 오디오 클립으로 SoundSO 인스턴스 생성
    private static void CreateSoundSO(AudioClip clip, string exportPath)
    {
        SoundSO so = ScriptableObject.CreateInstance<SoundSO>();
        so.InitSO(clip.name, clip);
        string savePath = string.Format("{0}/SoundSO_{1}.asset", exportPath, clip.name);
        string ioSavePath = Path.GetFullPath(savePath);
        if (File.Exists(ioSavePath)) // 존재한다면 생략
        {
            return;
        }
        AssetDatabase.CreateAsset(so, savePath);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnGUI()
    {
        if (GUILayout.Button("실행하기"))
        {
            EntrySoundSOGenerator();
        }
    }
    #endregion
}
