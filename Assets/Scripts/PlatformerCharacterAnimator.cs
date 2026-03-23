using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class SpriteAnimationOptions
{
    public Texture2D spritesheetTexture;
    [Range(1f, 60f)] public float frameRate = 12f;
}

public enum PlatformerCharacterAnimationState { Idle, Walk, Run, Jump, Attack }

[RequireComponent(typeof(SpriteRenderer))]
public class PlatformerCharacterAnimator : MonoBehaviour
{
    [Header("Animation Options")]
    public SpriteAnimationOptions idle;
    public SpriteAnimationOptions walk;
    public SpriteAnimationOptions run;
    public SpriteAnimationOptions jump;
    public SpriteAnimationOptions attack;

    [Header("Movement Thresholds")]
    public float walkThreshold = 0.1f;
    public float runThreshold = 3f;

    private SpriteRenderer spriteRenderer;

    private Sprite[][] states;
    private float[] frameRates;
    private HashSet<int> warnedStates = new HashSet<int>();

    private Sprite[] frames;
    private PlatformerCharacterAnimationState currentState;

    private int currentFrame;
    private float frameTimer;

    private bool isGrounded = true;
    private float speed;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        LoadAllStates();
        ApplyState(PlatformerCharacterAnimationState.Idle);
    }

    private void LoadAllStates()
    {
        SpriteAnimationOptions[] options = { idle, walk, run, jump, attack };

        states = new Sprite[options.Length][];
        frameRates = new float[options.Length];

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i]?.spritesheetTexture == null) continue;

            var sprites = Resources.LoadAll<Sprite>("")
                .Where(s => s.texture == options[i].spritesheetTexture)
                .OrderBy(s => int.TryParse(s.name.Split('_').LastOrDefault(), out int n) ? n : 0)
                .ToArray();

            if (sprites.Length == 0) continue;

            states[i] = sprites;
            frameRates[i] = options[i].frameRate;
        }
    }

    void Update()
    {
        UpdateState();
        UpdateAnimation();
    }

    private void UpdateState()
    {
        PlatformerCharacterAnimationState target = isGrounded ? GetMovementState() : PlatformerCharacterAnimationState.Jump;

        if (currentState != target)
            ApplyState(target);
    }

    private PlatformerCharacterAnimationState GetMovementState()
    {
        if (speed < walkThreshold) return PlatformerCharacterAnimationState.Idle;
        if (speed < runThreshold) return PlatformerCharacterAnimationState.Walk;
        return PlatformerCharacterAnimationState.Run;
    }

    private void ApplyState(PlatformerCharacterAnimationState newState)
    {
        int index = (int)newState;
        currentState = newState;

        if (states[index] == null || states[index].Length == 0)
        {
            if (warnedStates.Add(index))
                Debug.LogWarning($"{gameObject.name}: No sprites for state {newState}.");

            frames = null;
            spriteRenderer.sprite = null;
            return;
        }

        frames = states[index];
        currentFrame = 0;
        frameTimer = 0f;
        spriteRenderer.sprite = frames[0];
    }

    private void UpdateAnimation()
    {
        if (frames == null || frames.Length == 0) return;

        frameTimer += Time.deltaTime;

        if (frameTimer >= 1f / frameRates[(int)currentState])
        {
            frameTimer = 0f;
            currentFrame = (currentFrame + 1) % frames.Length;
            spriteRenderer.sprite = frames[currentFrame];
        }
    }

    public void TriggerJump()
    {
        if (!isGrounded) return;
        isGrounded = false;
        ApplyState(PlatformerCharacterAnimationState.Jump);
    }

    public void SetGrounded(bool grounded) => isGrounded = grounded;
    public void SetSpeed(float newSpeed) => speed = Mathf.Abs(newSpeed);
    public void SetDirection(float input)
    {
        if (Mathf.Abs(input) < 0.01f) return;
        spriteRenderer.flipX = input < 0;
    }

    public void SetState(PlatformerCharacterAnimationState state) => ApplyState(state);
}