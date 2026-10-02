using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoundManager : MonoBehaviour
{
    public float flyTurnTime = 10f;
    public float handTurnTime = 30f;
    public float stepDelay = 2f; // seconds between fly steps during settle

    public int flyMaxHP = 3;
    public int handMaxHP = 3;
    public Vector2Int burgerCell = new Vector2Int(2, 2);

    public GridMover fly;
    public GridMover hand;
    public GridMover ghost; // ghostfly: plan preview in fly turn, declaration in hand turn
    public UIManager ui;

    BoardGrid board;

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
        if (Keyboard.current == null)
            Debug.LogWarning("No keyboard found");

        board = FindFirstObjectByType<BoardGrid>();

        fly.planMode = true;
        hand.planMode = false;
        ghost.planMode = false;

        ghost.limitAround = true;
        ghost.canMove = false;
        round = 0;
        flyHP = flyMaxHP;
        handHP = handMaxHP;
        gameOver = false;
        nextSpecial = false;
        ui.SetHP(flyHP, flyMaxHP, handHP, handMaxHP);
        StartFlyTurn();
    }

    void Update()
    {
        if (gameOver || turn == Turn.Settle) return;

        var kb = Keyboard.current;
        timer -= Time.deltaTime;

        if (turn == Turn.Fly)
        {
            ui.SetFlyTimer(timer / flyTurnTime);
            ui.ShowFlyPlan(fly, special);
            ghost.SetCell(fly.PreviewCell()); // ghost follows the plan (X undo moves it back too)
            if (fly.planDone) FinishFlyTurn(false);
            else if (timer <= 0) FinishFlyTurn(true);
        }
        else
        {
            ui.SetHandTimer(timer / handTurnTime);
            bool enter = kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame);
            bool skip = kb != null && kb.backspaceKey.wasPressedThisFrame;
            if (enter) StartCoroutine(Settle(true));      // hand strikes on this step
            else if (skip || timer <= 0) SkipStep();      // no strike, fly takes this step
        }
    }

    void StartFlyTurn()
    {
        round++;
        special = nextSpecial;
        turn = Turn.Fly;
        timer = flyTurnTime;
        fly.ResetPlan();
        fly.specialMode = special;
        fly.canMove = true;
        hand.canMove = false;
        ghost.canMove = false;
        board.SetBreathing(special); // burger's 4 diagonal cells breathe during the special round
        ui.SetVisible(hand, false); // hand is hidden during the fly turn
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
            while (fly.PlanCost() < fly.maxCost) fly.plan.Add(Vector2Int.zero); // unused points = Stay
        }
        ui.ShowFlyPlan(fly, special);
        StartHandTurn();
    }

    void StartHandTurn()
    {
        turn = Turn.Hand;
        timer = handTurnTime;
        fly.canMove = false;
        hand.canMove = true;
        hand.SetCell(fly.cell);
        step = 0;
        revealedCost = 0;
        ResetGhost();
        ui.SetVisible(hand, true);
        ui.ShowHandTurn(fly, special);
        Debug.Log($"Round {round}: Hand turn, step 1/{fly.plan.Count}");
    }

    // Ghost goes back to the fly; the fly moves it (WASD) inside the 3x3 to declare. X = back to the fly.
    void ResetGhost()
    {
        ghost.limitCenter = fly.cell;
        ghost.SetCell(fly.cell);
        ghost.canMove = true;
    }

    // Fly takes its current step and the mask reveals it
    void MoveFlyOneStep()
    {
        var d = fly.plan[step];
        fly.SetCell(fly.cell + d);
        revealedCost += GridMover.Cost(d);
        step++;
        ui.SetReveal(step >= fly.plan.Count ? 1f : (float)revealedCost / fly.maxCost);
    }

    // Normal round: the fly stops as soon as it reaches the burger
    bool StoppedOnBurger() => !special && fly.cell == burgerCell;

    // Backspace / timeout: no strike. Last step (or reached burger) -> settle.
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
        hand.canMove = false;
        ghost.canMove = false;

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
            if (special || fly.cell == burgerCell)
            {
                fly.SetCell(fly.startCell);
            }
        }
        else if (special)
        {
            handHP--;
            nextSpecial = fly.cell == burgerCell;
        }
        else if (fly.cell == burgerCell)
        {
            handHP--;
            nextSpecial = true;
        }
        ui.SetHP(flyHP, flyMaxHP, handHP, handMaxHP);

        if (flyHP <= 0 || handHP <= 0)
        {
            gameOver = true;
            fly.canMove = false;
            hand.canMove = false;
            board.SetBreathing(false);
            yield break;
        }
        StartFlyTurn();
    }
}