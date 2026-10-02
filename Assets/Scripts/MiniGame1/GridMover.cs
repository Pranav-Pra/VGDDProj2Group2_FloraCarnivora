using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridMover : MonoBehaviour
{
    public Vector2Int startCell = new Vector2Int(4, 0);

    public Key up = Key.W;
    public Key down = Key.S;
    public Key left = Key.A;
    public Key right = Key.D;

    //for fly
    public Key confirm = Key.Z;
    public Key undo = Key.X;

    public Vector2Int cell;
    public bool canMove = true;   // set by RoundManager
    public bool planMode = false;
    public int maxCost = 2;
    public List<Vector2Int> plan = new List<Vector2Int>();
    public Vector2Int pending;    // keys pressed but not saved yet
    public bool planDone;         // fly pressed Z with all points used
    public bool specialMode;      // special round: one step only, diagonal or Stay

    public bool limitAround;      // ghost in hand turn: stays inside the 3x3 around limitCenter
    public Vector2Int limitCenter;

    BoardGrid board;

    void Start()
    {
        board = FindFirstObjectByType<BoardGrid>();
        SetCell(startCell);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || !canMove) return;

        if (planMode)
        {
            PlanInput(kb);
            return;
        }

        if (Pressed(kb, up)) Step(0, 1);
        if (Pressed(kb, down)) Step(0, -1);
        if (Pressed(kb, left)) Step(-1, 0);
        if (Pressed(kb, right)) Step(1, 0);
        if (limitAround && Pressed(kb, undo)) SetCell(limitCenter); // ghost: X goes back to the fly
    }

    void PlanInput(Keyboard kb)
    {
        // vertical key sets y, horizontal key sets x -> two perpendicular keys make a diagonal
        // opposite key cancels that direction (A then D -> nothing)
        if (Pressed(kb, up)) pending.y = pending.y == -1 ? 0 : 1;
        if (Pressed(kb, down)) pending.y = pending.y == 1 ? 0 : -1;
        if (Pressed(kb, left)) pending.x = pending.x == 1 ? 0 : -1;
        if (Pressed(kb, right)) pending.x = pending.x == -1 ? 0 : 1;

        if (Pressed(kb, undo))
        {
            if (pending != Vector2Int.zero) pending = Vector2Int.zero;
            else if (plan.Count > 0) plan.RemoveAt(plan.Count - 1);
        }

        if (Pressed(kb, confirm))
        {
            if (PlanFull()) planDone = true;
            else SavePending();
        }
    }

    // Save the pending step into the plan. Returns false if it doesn't fit.
    public bool SavePending()
    {
        bool straight = (pending.x == 0) != (pending.y == 0);
        if (specialMode && straight)
        {
            return false;
        }
        if (!specialMode && PlanCost() + Cost(pending) > maxCost)
        {
            return false;
        }
        if (!InBoard(PlanEnd() + pending))
        {
            return false;
        }
        plan.Add(pending);
        pending = Vector2Int.zero;
        return true;
    }

    public void ResetPlan()
    {
        plan.Clear();
        pending = Vector2Int.zero;
        planDone = false;
    }

    void Step(int dx, int dy)
    {
        var next = cell + new Vector2Int(dx, dy);
        if (limitAround && (Mathf.Abs(next.x - limitCenter.x) > 1 || Mathf.Abs(next.y - limitCenter.y) > 1)) return;
        if (InBoard(next)) SetCell(next);
    }

    // Fly turn: where the fly would end up (saved steps + pending step, if it stays on the board)
    public Vector2Int PreviewCell()
    {
        var p = PlanEnd() + pending;
        return InBoard(p) ? p : PlanEnd();
    }

    bool Pressed(Keyboard kb, Key k) => k != Key.None && kb[k].wasPressedThisFrame;

    public static int Cost(Vector2Int d) => (d.x != 0 && d.y != 0) ? 2 : 1;
    public static bool InBoard(Vector2Int p) => p.x >= 0 && p.x < 5 && p.y >= 0 && p.y < 5;

    public bool PlanFull() => specialMode ? plan.Count >= 1 : PlanCost() >= maxCost;

    public int PlanCost()
    {
        int c = 0;
        foreach (var d in plan) c += Cost(d);
        return c;
    }

    public Vector2Int PlanEnd()
    {
        var p = cell;
        foreach (var d in plan) p += d;
        return p;
    }

    public void SetCell(Vector2Int c)
    {
        cell = c;
        transform.position = board.CellToWorld(cell);
    }
}