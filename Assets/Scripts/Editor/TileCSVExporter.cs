using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 프로젝트 내의 타일 스프라이트를 읽어서 CSV 파일을 생성
/// </summary>
public class TileCSVExporter : EditorWindow
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private const string TILE_PATH = "Assets/Farm RPG - Tiny Asset Pack - (All in One)/Farm/Tileset/Modular";
    private const string EXPORT_PATH = "Assets/Export";
    private const string EXPORT_NAME = "TileData.csv";
    private const int TILE_STATE_COUNT = 11;
    private string _targetPath = TILE_PATH;
    private string _exportPath = EXPORT_PATH;
    private string _exportName = EXPORT_NAME;
    private StringBuilder _buffer = new();
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 유니티 상단 메뉴바에 버튼 만들기
    [MenuItem("Tools/타일 CSV 추출")]
    public static void ShowWindow()
    {
        // 해당 클래스 타입을 찾아서 탭 생성
        GetWindow<TileCSVExporter>("CSV 추출기");
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void ExportCSV(string targetPath, string exportPath, string fileName)
    {
        // 파일 경로가 유효한지 검사
        if (!Directory.Exists(targetPath)
            || !Directory.Exists(exportPath)
            || string.IsNullOrEmpty(fileName)
            )
        {
            UDebug.Print($"[TileIO] 경로가 유효하지 않거나 파일 이름이 비어있습니다.");
            return;
        }
        // 파일 경로 배열 생성
        string[] filePaths = Directory.GetFiles(targetPath, "*.*", SearchOption.TopDirectoryOnly);
        // StreamWriter로 파일 쓰기
        // 파일 이름, 덮어쓰기 / 이어쓰기, 인코딩
        using (StreamWriter sw = new StreamWriter($"{exportPath}/{fileName}", false, Encoding.UTF8))
        {
            sw.WriteLine("ID,이름,크기,이동 가능,경작 가능,낚시 가능,건설 가능,파괴 가능,상호작용 가능,바닥,벽,물,깊은 물,공기");
            int length = filePaths.Length;
            int id = 0;
            for (int i = 0; i < length; ++i)
            {
                ref string path = ref filePaths[i];
                // 메타 파일 거르기
                if (path.EndsWith(".meta"))
                {
                    continue;
                }
                // OS 절대 경로를 유니티 경로로 변환
                // Directory.GetFiles는 파일 경로의 구분자로 \를 반환
                // AssetDatabase는 파일 경로의 구분자로 /를 사용하며 프로젝트 루트 기준 상대 경로를 필요
                string assetPath = path.Replace("\\", "/");
                int startIndex = assetPath.IndexOf("Assets/");
                if (startIndex < 0)
                {
                    UDebug.Print("유효한 유니티 경로가 아닙니다.", LogType.Assert);
                    continue;
                }
                assetPath = assetPath.Substring(startIndex);
                // 슬라이스된 스프라이트 전부 가져오기
                Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                if (objs == null)
                {
                    UDebug.Print("파일 경로에 에셋이 하나도 없습니다.", LogType.Assert);
                    continue;
                }
                // 파일 이름 검출 시작
                int objLength = objs.Length;
                for (int j = 0; j < objLength; ++j)
                {
                    if (objs[j] is Sprite sprite && sprite != null)
                    {
                        // id, 이름, 크기, 넓게 미리미리 상태(31)
                        _buffer.Clear();
                        _buffer.Append(id); // ID
                        _buffer.Append(",");
                        _buffer.Append(sprite.name); // 이름
                        _buffer.Append(",");
                        _buffer.Append(1); // 크기
                        for (int k = 0; k < TILE_STATE_COUNT; ++k)
                        {
                            _buffer.Append(",");
                            _buffer.Append(0); // 상태
                        }
                        sw.WriteLine(_buffer);
                        id++;
                    }
                }
                // 완료
                UDebug.Print($"스프라이트 파일 개수 총 {id}개를 발견하고 csv 파일로 추출했습니다.");
                AssetDatabase.Refresh(); // 파일을 코드로 생성했음을 알려주기
            }
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnGUI()
    {
        GUILayout.Label("타일 CSV 출력 장치", EditorStyles.boldLabel); // 제목
        _targetPath = EditorGUILayout.TextField(TILE_PATH); // 텍스트 필드
        _exportPath = EditorGUILayout.TextField(EXPORT_PATH);
        _exportName = EditorGUILayout.TextField(EXPORT_NAME);
        if (GUILayout.Button("타일 CSV 생성")) // 버튼이 클릭된 순간 true
        {
            ExportCSV(_targetPath, _exportPath, _exportName);
        }
    }
    #endregion
}
