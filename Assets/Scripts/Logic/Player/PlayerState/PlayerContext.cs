using UnityEngine;

/// <summary>
/// 매 프레임 작성하여 상태 머신으로 전달할 구조체
/// </summary>
public readonly struct PlayerContext
{
    public readonly Rigidbody2D rb;
    public readonly Transform tr;
    public readonly SpriteRenderer sprite;
    public readonly Animator anim;
    // 입력 데이터
    public readonly Vector2 inputMove;
    public readonly bool inputRun;
    public readonly bool inputFishing;
    public readonly bool inputMining;
    public readonly bool inputLogging;
    public readonly bool inputShovel;
    public readonly bool inputSickle;
    public readonly bool inputDrinking;
    public readonly bool inputEating;
    public readonly bool inputWatering;
    public readonly bool inputCrouching;
    public readonly bool inputWakeUp;
    // 부가 데이터
    public readonly Vector2 targetPos; // 바라봐야 할 좌표
    public readonly float duration; // 단발성 애니메이션 지속시간
    public readonly bool isSuccess; // 작업 성공
    public readonly bool isCanceled; // 애니메이션 취소 요청

    // 생성자
    public PlayerContext( Rigidbody2D rb, Transform tr, SpriteRenderer sprite, Animator anim,
        Vector2 inputMove, bool inputRun, bool inputFishing, bool inputMining, bool inputLogging,
        bool inputShovel, bool inputSickle, bool inputDrinking, bool inputEating, bool inputWatering,
        bool inputCrouching, bool inputWakeUp, Vector2 targetPos, float duration, bool isSuccess, bool isCanceled)
    {
        this.rb = rb;
        this.tr = tr;
        this.sprite = sprite;
        this.anim = anim;
        this.inputMove = inputMove;
        this.inputRun = inputRun;
        this.inputFishing = inputFishing;
        this.inputMining = inputMining;
        this.inputLogging = inputLogging;
        this.inputShovel = inputShovel;
        this.inputSickle = inputSickle;
        this.inputDrinking = inputDrinking;
        this.inputEating = inputEating;
        this.inputWatering = inputWatering;
        this.inputCrouching = inputCrouching;
        this.inputWakeUp = inputWakeUp;
        this.targetPos = targetPos;
        this.duration = duration;
        this.isSuccess = isSuccess;
        this.isCanceled = isCanceled;
    }
}
