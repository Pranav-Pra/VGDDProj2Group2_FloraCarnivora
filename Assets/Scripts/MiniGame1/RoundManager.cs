using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Runs the match: fly turn (bug plans with keys) -> hand turn (plant strikes with the mouse) -> settle.
public class RoundManager : MonoBehaviour
{
    [Header("Timing")]
    public float flyTurnTime = 10f;
    public float handTurnTime = 30f;   // per fly step
    public float stepDelay = 2f;       // seconds between fly steps during settle

    [Header("Rules")]
    public int flyMaxHP = 3;
    public int handMaxHP = 3;
    public Vector2Int burgerCell = new Vector2Int(2, 2);

    [Header("Pieces")]
    public GridMover fly;
    public GridMover hand;   // hidden; just remembers which cell the mouse is aiming at
    public GridMover ghost;  // bug's decoy: plan preview in fly turn, free move in hand turn

    [Header("Scene")]
    public UIManager ui;
    public Camera cam;               // the first-person camera; empty = Camera.main
    public LayerMask boardMask;      // tick only the Board layer
    public VineController vine;      // rises in the hand turn, smacks on a strike

    BoardGrid board;
    Bug_MG flyBody; // idle squeeze / step squeeze / facing of the bug body (optional)

    enum Turn { Fly, Hand, Settle }
    Turn turn;
    int round;
    float timer;
    int flyHP, handHP;
    bool gameOver;
    int step;          // which fly step the hand is facing now
    int revealedCost;  // points of fly steps already revealed
    bool special;      // this round is a special round
    bool nextSpecial;  // next round will be a special round

    void Start()
    {
        board = FindFirstObjectByType<BoardGrid>();
        if (cam == null) cam = Camera.main;
        flyBody = fly.GetComponent<Bug_MG>();

        fly.role = GridMover.Role.Fly;
        ghost.role = GridMover.Role.Ghost;
        hand.role = GridMover.Role.Hand;
        ui.SetVisible(hand, false); // the hand piece is never drawn; the hovered cell breathes instead

        flyHP = flyMaxHP;
        handHP = handMaxHP;
        ui.SetHP(flyHP, flyMaxHP, handHP, handMaxHP);
        if (vine != null) vine.HideInstant(); // game starts on the fly turn: vine already below the screen
        StartFlyTurn();
    }

    void Update()
    {
        if (gameOver || turn == Turn.Settle) return;

        timer -= Time.deltaTime;

        if (turn == Turn.Fly)
        {
            ui.SetFlyTimer(timer / flyTurnTime);
            ui.ShowFlyPlan(fly, special);
            ghost.SetCell(fly.PreviewCell()); // ghost follows the plan (X undo moves it back too)
            if (fly.Decided) FinishFlyTurn(false);
            else if (timer <= 0) FinishFlyTurn(true);
        }
        else
        {
            ui.SetHandTimer(timer / handTurnTime);
            HandInput();
        }
    }

    // ---------- Hand turn: mouse only ----------
    void HandInput()
    {
        bool hovering = TryGetHoveredCell(out var hovered);
        if (hovering) hand.SetCell(hovered);      // remembers the target cell for the hit check
        board.SetHover(hovering, hovered);        // the cell under the mouse breathes

        // left click on a cell = strike, right click = skip this step
        var mouse = Mouse.current;
        bool click = hovering && mouse != null && mouse.leftButton.wasPressedThisFrame && !PointerOverUI();
        bool skip = mouse != null && mouse.rightButton.wasPressedThisFrame;

        if (click)
        {
            if (vine != null) vine.PlaySmack();
            StartCoroutine(Settle(true));
        }
        else if (skip || timer <= 0) SkipStep();
    }

    // Mouse ray -> which board cell is under the cursor
    bool TryGetHoveredCell(out Vector2Int c)
    {
        c = default;
        var mouse = Mouse.current;
        if (mouse == null || cam == null) return false;
        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        return Physics.Raycast(ray, out var hit, 200f, boardMask)
               && board.WorldToCell(hit.point, out c);
    }

    // Don't strike when the click lands on UI that has Raycast Target on
    static bool PointerOverUI() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

    // ---------- Turn flow ----------
    void StartFlyTurn()
    {
        round++;
        special = nextSpecial;
        turn = Turn.Fly;
        timer = flyTurnTime;
        fly.ResetPlan();
        fly.specialRound = special;
        fly.canMove = true;
        ghost.canMove = false;
        if (flyBody != null) flyBody.SetIdle(true); // fly player deciding -> idle
        if (vine != null) vine.Hide();             // vine sinks below the screen during the fly turn
        board.SetBreathing(special);               // burger's 4 diagonal cells breathe in a special round
        ui.ShowFlyTurn(round, special);
        Debug.Log($"Round {round}: Fly turn" + (special ? " (SPECIAL)" : ""));
    }

    void FinishFlyTurn(bool timeout)
    {
        if (timeout && fly.pending != Vector2Int.zero) fly.SavePending(); // try to keep the unsaved step
        if (special)
        {
            if (fly.plan.Count == 0) fly.plan.Add(Vector2Int.zero);
        }
        else
        {
            while (fly.PlanCost() < fly.maxStep) fly.plan.Add(Vector2Int.zero); // unused points = Stay
        }
        ui.ShowFlyPlan(fly, special);
        StartHandTurn();
    }

    void StartHandTurn()
    {
        turn = Turn.Hand;
        timer = handTurnTime;
        fly.canMove = false;
        step = 0;
        revealedCost = 0;
        ResetGhost();
        if (vine != null) vine.Show();             // vine rises up for the hand turn
        ui.ShowHandTurn(fly, special);
        Debug.Log($"Round {round}: Hand turn, step 1/{fly.plan.Count}");
    }

    // Ghost goes back to the fly; the bug player moves it (WASD) inside the 3x3. X = back to the fly.
    void ResetGhost()
    {
        ghost.limitCenter = fly.cell;
        ghost.SetCell(fly.cell);
        ghost.canMove = true;
    }

    // Fly takes its current step and the hand-side mask reveals it
    void MoveFlyOneStep()
    {
        var d = fly.plan[step];
        fly.SetCell(fly.cell + d);
        if (flyBody != null) flyBody.OnStep(d); // face the way it moved + squeeze once
        revealedCost += GridMover.Move(d);
        step++;
        ui.SetReveal(step >= fly.plan.Count ? 1f : (float)revealedCost / fly.maxStep);
    }

    // Normal round: the fly stops as soon as it reaches the burger
    bool StoppedOnBurger() => !special && fly.cell == burgerCell;

    // Right click / timeout: no strike. Last step (or reached burger) -> settle.
    void SkipStep()
    {
        MoveFlyOneStep();
        if (step >= fly.plan.Count || StoppedOnBurger()) StartCoroutine(Settle(false));
        else
        {
            timer = handTurnTime;
            ResetGhost(); // new step: declare again around the fly's new cell
            Debug.Log($"Round {round}: Hand skipped, step {step + 1}/{fly.plan.Count}");
        }
    }

    IEnumerator Settle(bool struck)
    {
        turn = Turn.Settle;
        ghost.canMove = false;
        board.ClearHover();
        if (flyBody != null) flyBody.SetIdle(false); // walking out the steps: only step squeezes

        bool hit = false;
        if (struck)
        {
            MoveFlyOneStep(); // this step happens together with the strike
            hit = hand.cell == fly.cell;
        }
        if (!hit)
        {
            // missed (or skipped to the end): fly finishes the remaining steps
            while (step < fly.plan.Count && !StoppedOnBurger())
            {
                yield return new WaitForSeconds(stepDelay);
                MoveFlyOneStep();
            }
        }
        yield return new WaitForSeconds(stepDelay);

        nextSpecial = false;
        if (hit)
        {
            flyHP--;
            if (special || fly.cell == burgerCell) fly.SetCell(fly.startCell);
        }
        else if (fly.cell == burgerCell)
        {
            // normal or special round: the hand only loses HP if the fly ends on the burger
            handHP--;
            nextSpecial = true;
        }
        ui.SetHP(flyHP, flyMaxHP, handHP, handMaxHP);

        if (flyHP <= 0 || handHP <= 0)
        {
            gameOver = true;
            board.SetBreathing(false);
            yield break;
        }
        StartFlyTurn();
    }
}