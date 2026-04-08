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
    public readonly bool inputDrinking;
    public readonly bool inputEating;
    public readonly Vector2 targetPos;

    // 생성자
    public PlayerContext(
        Rigidbody2D rb, Transform tr, SpriteRenderer sprite, Animator anim,
        Vector2 inputMove, bool inputRun,
        bool inputFishing, bool inputMining, bool inputLogging,
        bool inputDrinking, bool inputEating, Vector2 targetPos)
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
        this.inputDrinking = inputDrinking;
        this.inputEating = inputEating;
        this.targetPos = targetPos;
    }
}
