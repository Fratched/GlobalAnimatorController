
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SpriteAnimationOptions
{
    public Texture2D spritesheetTexture;
    [Range(1f, 60f)] public float frameRate = 12f;
}

public enum SpriteState
{
    Idle,
    Walk,
    Run,
    Jump,
    Attack
}

public class SpriteAnimator : MonoBehaviour
{
    [Header("Animation Options")] public SpriteAnimationOptions idle;
    public SpriteAnimationOptions walk;
    public SpriteAnimationOptions run;
    public SpriteAnimationOptions jump;
    public SpriteAnimationOptions attack;

    [Header("Movement Thresholds")] public float walkThreshold = 0.1f;
    public float runThreshold = 3f;

    private SpriteRenderer spriteRenderer;

    private Sprite[][] states;
    private float[] frameRates;

    private Sprite[] frames;
    private SpriteState currentState;
    private SpriteState nextState;

    private int currentFrame;
    private float frameTimer;

    private bool isGrounded = true;
    private float speed = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        LoadAllStates();

        currentState = SpriteState.Idle;
        nextState = currentState;

        ApplyState(currentState);
    }

    private void LoadAllStates()
    {
        SpriteAnimationOptions[] options = { idle, walk, run, jump, attack };

        states = new Sprite[options.Length][];
        frameRates = new float[options.Length];

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == null || options[i].spritesheetTexture == null)
            {
                Debug.LogError($"[SpriteAnimator] Missing texture for state {(SpriteState)i} on {gameObject.name}");
                continue;
            }

            var sprites = Resources.LoadAll<Sprite>("")
                .Where(s => s.texture == options[i].spritesheetTexture)
                .OrderBy(s =>
                {
                    int result;
                    return int.TryParse(s.name.Split('_').LastOrDefault(), out result) ? result : 0;
                })
                .ToArray();

            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogError(
                    $"[SpriteAnimator] No sprites found for state {(SpriteState)i} using texture {options[i].spritesheetTexture.name} on {gameObject.name}");
                continue;
            }

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
        // ✅ If in air → ALWAYS stay in jump
        if (!isGrounded)
        {
            if (currentState != SpriteState.Jump)
            {
                ApplyState(SpriteState.Jump);
            }
            return;
        }

        // ✅ Only switch when grounded
        SpriteState movementState = GetMovementState();

        if (currentState != movementState)
        {
            ApplyState(movementState);
        }
    }

    private SpriteState GetMovementState()
    {
        if (speed < walkThreshold)
            return SpriteState.Idle;
        else if (speed < runThreshold)
            return SpriteState.Walk;
        else
            return SpriteState.Run;
    }

    private void ApplyState(SpriteState newState)
    {
        int index = (int)newState;


        currentState = newState;
        frames = states[index];
        currentFrame = 0;
        frameTimer = 0f;

        spriteRenderer.sprite = frames[0];
    }

    private void UpdateAnimation()
    {
        if (frames == null || frames.Length == 0) return;

        frameTimer += Time.deltaTime;

        float rate = frameRates[(int)currentState];
        if (frameTimer >= 1f / rate)
        {
            frameTimer = 0f;
            currentFrame = (currentFrame + 1) % frames.Length;
            spriteRenderer.sprite = frames[currentFrame];
        }
    }

    // ✅ Manual jump trigger
    public void TriggerJump()
    {
        if (!isGrounded) return;

        isGrounded = false;
        nextState = SpriteState.Jump;
        ApplyState(SpriteState.Jump);
    }

    // ✅ Called by your controller / physics
    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Abs(newSpeed);
    }

    // Optional override
    public void SetState(SpriteState state)
    {
        nextState = state;
        
    }
    public void SetDirection(float input)
    {
        if (Mathf.Abs(input) < 0.01f) return;

        spriteRenderer.flipX = input < 0;
    }
}
