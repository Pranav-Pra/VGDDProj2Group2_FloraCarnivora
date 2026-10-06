using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class VineController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Images")]
    [Tooltip("idle vine.")]
    [SerializeField] private Sprite idleSprite;
    [Tooltip("vine smacking the ground.")]
    [SerializeField] private Sprite smackSprite;

    [Header("Smack")]
    [Tooltip("Lab: on. Minigame1: off (RoundManager calls PlaySmack when the hand strikes).")]
    [SerializeField] private bool useSmackKey = true;
    [SerializeField] private KeyCode smackKey = KeyCode.Space;
    [Tooltip("Seconds the smack image stays on screen.")]
    [SerializeField] private float smackDuration = 0.25f;

    [Header("Rise / Sink (minigame)")]
    [Tooltip("Seconds to rise up from below the screen, or sink back down.")]
    [SerializeField] private float slideDuration = 0.4f;
    [Tooltip("Extra distance below the screen edge when hidden, so no tip pokes out.")]
    [SerializeField] private float hiddenMargin = 50f;
    #endregion

    #region Private Fields
    private Image vineImage;
    private bool isSmacking;
    private RectTransform rect;
    private Vector2 shownPosition;   // where you placed it in the editor
    private Coroutine slideRoutine;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        vineImage = GetComponent<Image>();
        vineImage.sprite = idleSprite;
        rect = GetComponent<RectTransform>();
        shownPosition = rect.anchoredPosition;
    }

    private void Update()
    {
        if (useSmackKey && Input.GetKeyDown(smackKey))
        {
            PlaySmack();
        }
    }
    #endregion

    #region Public Methods
    public void PlaySmack()
    {
        if (!isSmacking) StartCoroutine(Smack());
    }

    /// Rise from below the screen to the editor position.
    public void Show() => SlideTo(shownPosition);

    /// Sink below the bottom of the screen.
    public void Hide() => SlideTo(HiddenPosition());

    /// Jump straight to the hidden spot (no animation), e.g. at game start.
    public void HideInstant()
    {
        if (slideRoutine != null) StopCoroutine(slideRoutine);
        rect.anchoredPosition = HiddenPosition();
    }
    #endregion

    #region Rise / Sink
    // Straight down by the vine's own height (+ margin): its top ends up under the screen's bottom edge
    private Vector2 HiddenPosition()
    {
        float height = rect.rect.height * rect.localScale.y;
        return shownPosition + Vector2.down * (height + hiddenMargin);
    }

    private void SlideTo(Vector2 target)
    {
        if (slideRoutine != null) StopCoroutine(slideRoutine); // turning mid-slide: continue from where it is
        slideRoutine = StartCoroutine(Slide(target));
    }

    private IEnumerator Slide(Vector2 target)
    {
        Vector2 from = rect.anchoredPosition;
        for (float t = 0f; t < 1f; t += Time.deltaTime / slideDuration)
        {
            rect.anchoredPosition = Vector2.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t)); // ease in & out
            yield return null;
        }
        rect.anchoredPosition = target;
        slideRoutine = null;
    }
    #endregion

    #region Smack
    private IEnumerator Smack()
    {
        isSmacking = true;
        vineImage.sprite = smackSprite;

        yield return new WaitForSeconds(smackDuration);

        vineImage.sprite = idleSprite;
        isSmacking = false;
    }
    #endregion
}