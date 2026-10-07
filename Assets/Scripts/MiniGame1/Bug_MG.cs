using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Bug_MG : MonoBehaviour
{
    #region Serialized Fields
    [Header("Sprites")]
    [SerializeField] private Sprite squeezeSprite;
    [SerializeField] private float squeezeDuration = 0.15f;

    [Header("Facing")]
    [Tooltip("Turn on if faces right.")]
    [SerializeField] private bool artFacesRight = true;

    [Header("Idle")]
    [SerializeField] private Vector2 idleIntervalRange = new Vector2(1f, 2.5f);
    [Range(0f, 1f)]
    [SerializeField] private float idleFlipChance = 0.5f;
    #endregion

    #region Private Fields
    private SpriteRenderer spriteRenderer;
    private Sprite normalSprite;
    private bool idle;
    private float idleTimer;    // counts down to the next idle squeeze
    private float squeezeLeft;  // > 0 while the squeezed frame is showing
    private bool facingLeft;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalSprite = spriteRenderer.sprite;
        ResetIdleTimer();
    }

    private void Update()
    {
        // end of a squeeze -> back to the normal frame
        if (squeezeLeft > 0f)
        {
            squeezeLeft -= Time.deltaTime;
            if (squeezeLeft <= 0f) spriteRenderer.sprite = normalSprite;
        }

        if (!idle) return;

        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
        {
            if (Random.value < idleFlipChance) SetFacingLeft(!facingLeft);
            Squeeze();
            ResetIdleTimer();
        }
    }
    #endregion

    #region Public Methods (called by RoundManager)
    /// On while a player is deciding; off while the fly is walking out its steps.
    public void SetIdle(bool on)
    {
        idle = on;
        ResetIdleTimer();
    }

    /// The fly just moved by d: face that way (if it moved sideways) and squeeze once.
    public void OnStep(Vector2Int d)
    {
        if (d.x != 0) SetFacingLeft(d.x < 0);
        Squeeze();
        ResetIdleTimer(); // don't idle-squeeze right on top of a step squeeze
    }

    public void SetFacingLeft(bool left)
    {
        facingLeft = left;
        spriteRenderer.flipX = artFacesRight ? left : !left;
    }

    public void Squeeze()
    {
        if (squeezeSprite == null) return;
        spriteRenderer.sprite = squeezeSprite;
        squeezeLeft = squeezeDuration;
    }
    #endregion

    private void ResetIdleTimer()
    {
        idleTimer = Random.Range(idleIntervalRange.x, idleIntervalRange.y);
    }
}