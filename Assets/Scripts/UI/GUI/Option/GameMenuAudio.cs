using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 오디오 설정 로직을 제공합니다.
/// </summary>
public class GameMenuAudio : BaseMono, IEscClosable
{
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    [SerializeField] private GameMenu _gameMenu;
    [SerializeField] private Slider _masterSilder;
    [SerializeField] private Slider _bgmSilder;
    [SerializeField] private Slider _sfxSilder;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public bool CanCloseWithEsc => true;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    /// <summary>
    /// 오디오 옵션 창을 닫고 게임 메뉴로 돌아갑니다.
    /// </summary>
    public void CloseUi()
    {
        EscManager.Ins.Exit(this);

        if (_gameMenu == null)
        {
            UObject.SetActive(gameObject, false);
            return;
        }

        _gameMenu.OpenMenu();
    }

    public void CloseButton()
    {
        CloseUi();
        //EscManager.Ins.Exit(this);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void SetDataMaster(float volume)
        => DataManager.Ins.Volume.Master = volume;
    private void SetDataBgm(float volume)
        => DataManager.Ins.Volume.Bgm = volume;
    private void SetDataSfx(float volume)
        => DataManager.Ins.Volume.Sfx = volume;
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        UDebug.IsNull(_gameMenu);
        UDebug.IsNull(_masterSilder);
        UDebug.IsNull(_bgmSilder);
        UDebug.IsNull(_sfxSilder);
    }

    private void OnEnable()
    {
        EscManager.Ins.Enter(this);
        // 초기값 설정
        if (_masterSilder == null || _bgmSilder == null || _sfxSilder == null)
        {
            return;
        }

        var volume = DataManager.Ins.Volume;

        _masterSilder.value = volume.Master;
        _bgmSilder.value = volume.Bgm;
        _sfxSilder.value = volume.Sfx;

        // 이벤트 리스너 등록
        _masterSilder.onValueChanged.AddListener(SetDataMaster);
        _bgmSilder.onValueChanged.AddListener(SetDataBgm);
        _sfxSilder.onValueChanged.AddListener(SetDataSfx);
    }

    private void OnDisable()
    {
        if (_masterSilder != null) // 방어코드 설정
        {
            _masterSilder.onValueChanged.RemoveAllListeners();
        }

        if (_bgmSilder != null)
        {
            _bgmSilder.onValueChanged.RemoveAllListeners();
        }

        if (_sfxSilder != null)
        {
            _sfxSilder.onValueChanged.RemoveAllListeners();
        }
    }
    
    #endregion
}
