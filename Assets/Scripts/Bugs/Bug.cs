using UnityEngine;

// A 2D bug sprite that always faces the camera (billboarding)
// skitters around a small area near where it spawned, like a real bug

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(SpriteRenderer))]

public class Bug : MonoBehaviour
{
    #region Serialized Fields
    [Header("Lifetime")]
    [Tooltip("Seconds before the bug disappears")]
    [SerializeField] private float lifetime = 10f;
 
    [Header("Billboarding")]
    [Tooltip("On: bug stays standing upright and only turns left/right.\n" +
             "Off: bug also tilts to match the camera's up/down angle.")]
             
    [SerializeField] private bool keepUpright = true;
    [Tooltip("Turn on if the bug in your image faces right. Used to flip it toward where it's walking.")]
    [SerializeField] private bool artFacesRight = true;

    [Header("Wander Area")]
    [Tooltip("How far the bug can roam from where it spawned.")]
    [SerializeField] private float wanderRadius = 0.6f;

    [Header("Scurrying")]
    [Tooltip("Slowest and fastest scurry speed.")]
    [SerializeField] private Vector2 speedRange = new Vector2(0.8f, 2f);
    [Tooltip("Shortest and longest burst of movement, in seconds.")]
    [SerializeField] private Vector2 scurryTimeRange = new Vector2(0.15f, 0.5f);
    [Tooltip("Shortest and longest pause between bursts, in seconds.")]
    [SerializeField] private Vector2 pauseTimeRange = new Vector2(0.2f, 1f);
    [Tooltip("Biggest sudden turn between bursts, in degrees.")]
    [SerializeField] private float maxTurnAngle = 120f;

    [Header("Wobble")]
    [Tooltip("How much the bug wiggles side to side while moving, in degrees.")]
    [SerializeField] private float wobbleAmount = 25f;
    [Tooltip("How fast the wiggle is.")]
    [SerializeField] private float wobbleSpeed = 20f;

    [Header("Scare")]
    [Tooltip("How fast the bug darts away from a click.")]
    [SerializeField] private float scareSpeed = 3f;
    [Tooltip("How long the dart lasts, in seconds. Speed x time = how far it scatters.")]
    [SerializeField] private float scareRunTime = 0.4f;
    [Tooltip("Seconds before the bug settles into its new spot.")]
    [SerializeField] private float scareDuration = 2f;
 
    [Header("Movement")]
    [Tooltip("How quickly the bug starts and stops. Higher = twitchier.")]
    [SerializeField] private float acceleration = 20f;

    [Header("Hover")]
    [Tooltip("mouse is over the bug squashed frame")]
    [SerializeField] private Sprite hoverSprite;

    #endregion

    #region Private Fields
    private const float ReturnHomeThreshold = 0.6f; // fraction of wanderRadius
    private const float FlipDeadZone = 0.1f;

    private Transform cameraTransform;
    private SpriteRenderer spriteRenderer;
    private Sprite normalSprite;

    private Vector3 homePosition;
    private Vector3 heading;
    private Vector3 velocity;
    private float currentSpeed;
    private bool isScurrying;
    private float stateTimer;
    private float wobblePhase;
    private float scareTimer;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalSprite = spriteRenderer.sprite;
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        homePosition = transform.position;
 
        heading = RandomDirection();
        wobblePhase = Random.Range(0f, Mathf.PI * 2f);
        stateTimer = Random.Range(pauseTimeRange.x, pauseTimeRange.y);
 
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Count down the scare, when it ends, bugs new spot becomes home
        if (scareTimer > 0f)
        {
            scareTimer -= Time.deltaTime;
 
            if (scareTimer <= 0f)
            {
                homePosition = transform.position;
            }
        }
 

        UpdateState();
        Move();
    }
 
    private void LateUpdate()
    {
        // runs after the camera has turned this frame
        FaceCamera();
    }
 
    private void OnMouseDown()
    {
        Squash();
    }

    private void OnMouseEnter()
    {
        if (hoverSprite != null)
        {
            spriteRenderer.sprite = hoverSprite;
        }
    }

    private void OnMouseExit()
    {
        spriteRenderer.sprite = normalSprite;
    }
    #endregion

    #region Scare
    /// Makes the bug dart away from a clicked point.
    public void Scare(Vector3 point)
    {
        Vector3 away = Flatten(transform.position - point).normalized;
 
        // Point its wander area away from the click so it doesn't wander back
        homePosition = transform.position + away * (scareSpeed * scareRunTime);
 
        heading = away;
        currentSpeed = scareSpeed;
        isScurrying = true;
        stateTimer = scareRunTime;
        scareTimer = scareDuration;
    }
    #endregion

    #region Behavior
    private void UpdateState()
    {
        stateTimer -= Time.deltaTime;
 
        if (stateTimer > 0f)
        {
            return;
        }
 
        if (isScurrying)
        {
            StartPause();
        }
        else
        {
            StartScurry();
        }
    }
 
    private void StartScurry()
    {
        isScurrying = true;
        stateTimer = Random.Range(scurryTimeRange.x, scurryTimeRange.y);
        currentSpeed = Random.Range(speedRange.x, speedRange.y);
        heading = ChooseNewHeading();
    }
 
    private void StartPause()
    {
        isScurrying = false;
        stateTimer = Random.Range(pauseTimeRange.x, pauseTimeRange.y);
    }
 
    private Vector3 ChooseNewHeading()
    {
        Vector3 toHome = Flatten(homePosition - transform.position);
 
        // if  near edge of its area: head roughly back toward home
        if (toHome.magnitude > wanderRadius * ReturnHomeThreshold)
        {
            return Rotate(toHome.normalized, Random.Range(-45f, 45f));
        }
 
        // Otherwise: a sudden turn from the current heading
        return Rotate(heading, Random.Range(-maxTurnAngle, maxTurnAngle));
    }
    #endregion
 
    #region Movement
    private void Move()
    {
        Vector3 desiredVelocity = Vector3.zero;
 
        if (isScurrying)
        {
            // Little side-to-side wiggle while running
            wobblePhase += wobbleSpeed * Time.deltaTime;
            float wobble = Mathf.Sin(wobblePhase) * wobbleAmount;
            desiredVelocity = Rotate(heading, wobble) * currentSpeed;
        }
 
        // Ease toward the desired velocity so starts and stops feel twitchy but not robotic
        float blend = 1f - Mathf.Exp(-acceleration * Time.deltaTime);
        velocity = Vector3.Lerp(velocity, desiredVelocity, blend);
 
        Vector3 step = velocity * Time.deltaTime;
        transform.position += step;
 
        KeepInsideArea();
 
        if (step.sqrMagnitude > 0.0000001f)
        {
            UpdateSpriteFlip(step);
        }
    }
 
    private void KeepInsideArea()
    {
        Vector3 offset = Flatten(transform.position - homePosition);
 
        if (offset.magnitude <= wanderRadius)
        {
            return;
        }
 
        // Push back onto the edge and turn back inward
        Vector3 edge = homePosition + offset.normalized * wanderRadius;
        transform.position = new Vector3(edge.x, transform.position.y, edge.z);
 
        heading = Rotate(-offset.normalized, Random.Range(-60f, 60f));
        velocity = Vector3.zero;
    }
    #endregion

    #region Visuals
    private void FaceCamera()
    {
        Vector3 forward = cameraTransform.forward;
 
        if (keepUpright)
        {
            forward.y = 0f;
        }
 
        if (forward.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }
 
    private void UpdateSpriteFlip(Vector3 step)
    {
        // How much of the movement is left/right on screen
        float screenDirection = Vector3.Dot(step.normalized, Flatten(cameraTransform.right).normalized);
 
        if (Mathf.Abs(screenDirection) < FlipDeadZone)
        {
            return; // moving mostly toward/away from camera, keep current facing
        }
 
        bool movingLeft = screenDirection < 0f;
        spriteRenderer.flipX = artFacesRight ? movingLeft : !movingLeft;
    }
    #endregion
 

    // #region Billboarding
    // private void FaceCamera()
    // {
    //     Vector3 forward = cameraTransform.forward;
 
    //     if (keepUpright)
    //     {
    //         forward.y = 0f;
    //     }
 
    //     if (forward.sqrMagnitude > 0.001f)
    //     {
    //         transform.rotation = Quaternion.LookRotation(forward);
    //     }
    // }
    // #endregion
 
    #region Squash
    private void Squash()
    {
        if (ScoreCounter.Instance != null)
        {
            ScoreCounter.Instance.AddPoint();
        }
        
        Destroy(gameObject);
    }
    #endregion

    #region Helpers
 
    private static Vector3 Flatten(Vector3 vector)
    {
        vector.y = 0f;
        return vector;
    }
 
    private static Vector3 Rotate(Vector3 direction, float degrees)
    {
        return Quaternion.Euler(0f, degrees, 0f) * direction;
    }
 
    private static Vector3 RandomDirection()
    {
        return Rotate(Vector3.forward, Random.Range(0f, 360f));
    }
    #endregion
}

