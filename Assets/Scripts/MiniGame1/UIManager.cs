using UnityEngine;
using UnityEngine.UI;
using TMPro;

// All screen stuff lives here. RoundManager only calls these methods.
public class UIManager : MonoBehaviour
{
    [Header("Top")]
    public TMP_Text roundText;        // "Round N" / "Special Round N"
    public TMP_Text turnText;         // "Fly" / "Hand"

    [Header("Countdown (one shared bar, value 1 -> 0)")]
    public Slider timerBar;           // Direction: Left To Right, so the handle walks right -> left as time runs out
    public Image timerIcon;           // the bar's Handle image: shows who is moving
    public Sprite flyIcon;            // bug picture (fly turn)
    public Sprite handIcon;           // plant picture (hand turn)
    [Tooltip("Tick if the bug picture faces right; it gets mirrored so it walks facing left.")]
    public bool flyIconFacesRight = true;
    [Tooltip("Tick if the plant picture faces right; it gets mirrored so it walks facing left.")]
    public bool handIconFacesRight = false;
    [Tooltip("Bug icon box size (Width, Height) and offset from the handle (X, Y). Tune until it looks right.")]
    public Vector2 flyIconSize = new Vector2(80, 80);
    public Vector2 flyIconOffset = new Vector2(0, 40);
    [Tooltip("Plant icon box size (Width, Height) and offset from the handle (X, Y).")]
    public Vector2 handIconSize = new Vector2(80, 80);
    public Vector2 handIconOffset = new Vector2(0, 40);

    [Header("HP")]
    public TMP_Text flyHpText;
    public Slider handHpBar;          // hand HP as a bar: full = max HP, empty = 0
    public TMP_Text handHpText;       // optional, e.g. "3/3" on the bar. Leave empty if not used.

    [Header("Fly input box")]
    public TMP_Text flyPlanText;
    public GameObject flyStepsPanel;  // Steps_Fly: shown in the fly turn, hidden in the hand turn (no mask any more)

    [Header("Hand-side fly steps")]
    public TMP_Text handPlanText;
    public Slider handMask;           // 1 = fully covered, 0 = fully revealed

    [Header("Special round hints")]
    public GameObject specialFlyHint;   // only in special round, fly turn
    public GameObject specialHandHint;  // only in special round, hand turn

    void Awake()
    {
        if (timerBar != null)
        {
            timerBar.interactable = false; // display only
            timerBar.minValue = 0;
            timerBar.maxValue = 1;
            // don't let the bar swallow clicks meant for the board
            foreach (var g in timerBar.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;
        }

        if (handHpBar != null)
        {
            handHpBar.interactable = false;  // display only, players can't drag it
            handHpBar.wholeNumbers = true;
            handHpBar.minValue = 0;
            // don't let the bar's images swallow mouse clicks meant for the board
            foreach (var g in handHpBar.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;
        }
    }

    public void ShowFlyTurn(int round, bool special)
    {
        roundText.text = (special ? "Special Round " : "Round ") + round;
        turnText.text = "Fly";

        SetTimerIcon(flyIcon, flyIconFacesRight, flyIconSize, flyIconOffset);
        timerBar.value = 1;

        if (flyStepsPanel != null) flyStepsPanel.SetActive(true);
        handPlanText.text = "";
        handMask.value = 1;

        specialFlyHint.SetActive(special);
        specialHandHint.SetActive(false);
    }

    public void ShowHandTurn(GridMover fly, bool special)
    {
        turnText.text = "Hand";

        SetTimerIcon(handIcon, handIconFacesRight, handIconSize, handIconOffset);
        timerBar.value = 1;

        if (flyStepsPanel != null) flyStepsPanel.SetActive(false); // hand player must not see the fly's input
        handPlanText.text = PlanText(fly, special, false);
        handMask.value = 1;

        specialFlyHint.SetActive(false);
        specialHandHint.SetActive(special);
    }

    // Both turns drive the same bar (RoundManager still calls these two)
    public void SetFlyTimer(float t01) { timerBar.value = Mathf.Clamp01(t01); }
    public void SetHandTimer(float t01) { timerBar.value = Mathf.Clamp01(t01); }

    // Swap the walking icon on the bar's handle; each picture gets its own size/offset,
    // and is mirrored if the art faces right so it walks leftwards
    void SetTimerIcon(Sprite icon, bool facesRight, Vector2 size, Vector2 offset)
    {
        if (timerIcon == null) return;
        if (icon != null) timerIcon.sprite = icon;
        timerIcon.preserveAspect = true;            // never stretch the picture
        timerIcon.rectTransform.sizeDelta = size;   // works with the default center anchor
        timerIcon.rectTransform.anchoredPosition = offset;
        var sc = timerIcon.rectTransform.localScale;
        sc.x = Mathf.Abs(sc.x) * (facesRight ? -1f : 1f);
        timerIcon.rectTransform.localScale = sc;
    }

    public void ShowFlyPlan(GridMover fly, bool special) { flyPlanText.text = PlanText(fly, special, true); }

    // revealed01: 0 = nothing shown, 1 = all shown
    public void SetReveal(float revealed01) { handMask.value = 1f - Mathf.Clamp01(revealed01); }

    public void SetHP(int flyHP, int flyMax, int handHP, int handMax)
    {
        flyHpText.text = $"Fly HP: {flyHP}/{flyMax}";

        if (handHpBar != null)
        {
            handHpBar.maxValue = handMax;
            handHpBar.value = Mathf.Clamp(handHP, 0, handMax);
        }
        if (handHpText != null) handHpText.text = $"{handHP}/{handMax}";
    }

    // Show / hide every sprite on an object (the object itself stays active)
    public void SetVisible(GridMover m, bool visible)
    {
        foreach (var r in m.GetComponentsInChildren<SpriteRenderer>()) r.enabled = visible;
    }

    // forFly: saved steps, then the pending step in (), then "_" for what's left.
    // Hand side: second half placed at 50% width so the mask lines up.
    string PlanText(GridMover fly, bool special, bool forFly)
    {
        string s = "";
        int used = 0;
        foreach (var d in fly.plan)
        {
            s += forFly ? (used > 0 ? "  " : "") + DirName(d) : $"<pos={used * 50}%>{DirName(d)}";
            used += GridMover.Move(d);
        }
        if (forFly)
        {
            bool hasPending = fly.pending != Vector2Int.zero;
            if (hasPending) s += (s.Length > 0 ? "  " : "") + "(" + DirName(fly.pending) + ")";

            int free = special
                ? 1 - fly.plan.Count - (hasPending ? 1 : 0)
                : fly.maxStep - used - (hasPending ? GridMover.Move(fly.pending) : 0);
            for (int i = 0; i < free; i++) s += (s.Length > 0 ? "  " : "") + "_";
        }
        return s;
    }

    static string DirName(Vector2Int d)
    {
        if (d == Vector2Int.zero) return "Stay";
        string v = d.y > 0 ? "Up" : d.y < 0 ? "Down" : "";
        string h = d.x > 0 ? "Right" : d.x < 0 ? "Left" : "";
        return h + v; // e.g. "LeftUp"
    }
}