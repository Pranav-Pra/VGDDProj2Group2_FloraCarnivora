using UnityEngine;
using UnityEngine.UI;
using TMPro;

// All screen stuff lives here. RoundManager only calls these methods.
public class UIManager : MonoBehaviour
{
    [Header("Top")]
    public TMP_Text roundText;        // "Round N" / "Special Round N"
    public TMP_Text turnText;         // "Fly" / "Hand"

    [Header("Countdown bars (only the active side is shown)")]
    public Slider flyTimerBar;
    public Slider handTimerBar;

    [Header("HP")]
    public TMP_Text flyHpText;
    public TMP_Text handHpText;

    [Header("Fly input box")]
    public TMP_Text flyPlanText;
    public GameObject flyMask;        // covers the fly input box in the hand turn

    [Header("Hand-side fly steps")]
    public TMP_Text handPlanText;
    public Slider handMask;           // 1 = fully covered, 0 = fully revealed

    [Header("Special round hints")]
    public GameObject specialFlyHint;   // only in special round, fly turn
    public GameObject specialHandHint;  // only in special round, hand turn

    public void ShowFlyTurn(int round, bool special)
    {
        roundText.text = (special ? "Special Round " : "Round ") + round;
        turnText.text = "Fly";

        flyTimerBar.gameObject.SetActive(true);
        flyTimerBar.value = 1;
        handTimerBar.gameObject.SetActive(false);

        flyMask.SetActive(false);
        handPlanText.text = "";
        handMask.value = 1;

        specialFlyHint.SetActive(special);
        specialHandHint.SetActive(false);
    }

    public void ShowHandTurn(GridMover fly, bool special)
    {
        turnText.text = "Hand";

        flyTimerBar.gameObject.SetActive(false);
        handTimerBar.gameObject.SetActive(true);
        handTimerBar.value = 1;

        flyMask.SetActive(true);
        handPlanText.text = PlanText(fly, special, false);
        handMask.value = 1;

        specialFlyHint.SetActive(false);
        specialHandHint.SetActive(special);
    }

    public void SetFlyTimer(float t01) { flyTimerBar.value = Mathf.Clamp01(t01); }
    public void SetHandTimer(float t01) { handTimerBar.value = Mathf.Clamp01(t01); }

    public void ShowFlyPlan(GridMover fly, bool special) { flyPlanText.text = PlanText(fly, special, true); }

    // revealed01: 0 = nothing shown, 1 = all shown
    public void SetReveal(float revealed01) { handMask.value = 1f - Mathf.Clamp01(revealed01); }

    public void SetHP(int flyHP, int flyMax, int handHP, int handMax)
    {
        flyHpText.text = $"Fly HP: {flyHP}/{flyMax}";
        handHpText.text = $"Hand HP: {handHP}/{handMax}";
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
            used += GridMover.Cost(d);
        }
        if (forFly)
        {
            bool hasPending = fly.pending != Vector2Int.zero;
            if (hasPending) s += (s.Length > 0 ? "  " : "") + "(" + DirName(fly.pending) + ")";

            int free = special
                ? 1 - fly.plan.Count - (hasPending ? 1 : 0)
                : fly.maxCost - used - (hasPending ? GridMover.Cost(fly.pending) : 0);
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