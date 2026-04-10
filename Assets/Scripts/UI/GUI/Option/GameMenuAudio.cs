using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 오디오 설정 로직을 제공합니다.
/// </summary>
public class GameMenuAudio : BaseMono, IEscClosable
{
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    [SerializeField] private Slider _masterSilder;
    [SerializeField] private Slider _bgmSilder;
    [SerializeField] private Slider _sfxSilder;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void CloseUi()
    {
        UObject.SetActive(gameObject, false);
    }

    public void CloseButton()
    {
        CloseUi();
        EscManager.Ins.Exit(this);
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
    private void OnEnable()
    {
        EscManager.Ins.Enter(this);
        // 초기값 설정
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
        _masterSilder.onValueChanged.RemoveAllListeners();
        _bgmSilder.onValueChanged.RemoveAllListeners();
        _sfxSilder.onValueChanged.RemoveAllListeners();
    }

    protected override void Awake()
    {
        UDebug.IsNull(_masterSilder);
        UDebug.IsNull(_bgmSilder);
        UDebug.IsNull(_sfxSilder);
    }
    #endregion
}
