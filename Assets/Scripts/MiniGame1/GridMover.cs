using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// A piece on the 5x5 board. RoundManager gives each one a role:
//  Fly   - the bug: plans its steps with the keys (WASD, Z save/confirm, X undo)
//  Ghost - the bug's decoy: moves with WASD inside the 3x3 around the bug, X jumps back to the bug
//  Hand  - the plant: no keys at all, RoundManager moves it to the cell under the mouse
public class GridMover : MonoBehaviour
{
    public enum Role { Fly, Ghost, Hand }

    // Bug player's keys (shared by Fly and Ghost; the plant only uses the mouse)
    public const Key UpKey = Key.W;
    public const Key DownKey = Key.S;
    public const Key LeftKey = Key.A;
    public const Key RightKey = Key.D;
    public const Key ConfirmKey = Key.Z;
    public const Key UndoKey = Key.X;

    [Tooltip("Cell this piece starts on (only matters for the bug).")]
    public Vector2Int startCell = new Vector2Int(4, 0);
    [Tooltip("Bug only")]
    public int maxStep = 2;

    // ---- runtime state, set by RoundManager / code (hidden from the Inspector) ----
    [System.NonSerialized] public Role role = Role.Hand;
    [System.NonSerialized] public Vector2Int cell;
    [System.NonSerialized] public bool canMove;           // keys only work while this is true

    // Fly planning
    [System.NonSerialized] public List<Vector2Int> plan = new List<Vector2Int>();
    [System.NonSerialized] public Vector2Int pending;     // keys pressed but not saved yet
    [System.NonSerialized] public bool Decided;          // Z pressed with all points used
    [System.NonSerialized] public bool specialRound;     // special round: one step only, diagonal or Stay

    // Ghost
    [System.NonSerialized] public Vector2Int limitCenter; // ghost stays inside the 3x3 around this cell

    BoardGrid board;

    void Awake()
    {
        board = FindFirstObjectByType<BoardGrid>(); 
    }

    void Start()
    {
        SetCell(startCell);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || !canMove) return;

        if (role == Role.Fly) FlyInput(kb);
        else if (role == Role.Ghost) GhostInput(kb);
        // Role.Hand: no keyboard input
    }

    // ---------- Fly: plan the steps ----------
    void FlyInput(Keyboard kb)
    {
        // vertical key sets y, horizontal key sets x -> two perpendicular keys make a diagonal
        // opposite key cancels that direction (A then D -> nothing)
        if (Pressed(kb, UpKey)) pending.y = pending.y == -1 ? 0 : 1;
        if (Pressed(kb, DownKey)) pending.y = pending.y == 1 ? 0 : -1;
        if (Pressed(kb, LeftKey)) pending.x = pending.x == 1 ? 0 : -1;
        if (Pressed(kb, RightKey)) pending.x = pending.x == -1 ? 0 : 1;

        if (Pressed(kb, UndoKey))
        {
            if (pending != Vector2Int.zero) pending = Vector2Int.zero;
            else if (plan.Count > 0) plan.RemoveAt(plan.Count - 1);
        }

        if (Pressed(kb, ConfirmKey))
        {
            if (PlanFull()) Decided = true;
            else SavePending();
        }
    }

    // Save the pending step into the plan. Returns false if it doesn't fit.
    public bool SavePending()
    {
        bool straight = (pending.x == 0) != (pending.y == 0);
        if (specialRound && straight) return false;
        if (!specialRound && PlanCost() + Move(pending) > maxStep) return false;
        if (!InBoard(PlanEnd() + pending)) return false;

        plan.Add(pending);
        pending = Vector2Int.zero;
        return true;
    }

    public void ResetPlan()
    {
        plan.Clear();
        pending = Vector2Int.zero;
        Decided = false;
    }

    public bool PlanFull() => specialRound ? plan.Count >= 1 : PlanCost() >= maxStep;

    public int PlanCost()
    {
        int c = 0;
        foreach (var d in plan) c += Move(d);
        return c;
    }

    public Vector2Int PlanEnd()
    {
        var p = cell;
        foreach (var d in plan) p += d;
        return p;
    }

    // Where the fly would end up (saved steps + pending step, if it stays on the board)
    public Vector2Int PreviewCell()
    {
        var p = PlanEnd() + pending;
        return InBoard(p) ? p : PlanEnd();
    }

    // ---------- Ghost: move inside the 3x3 around the fly ----------
    void GhostInput(Keyboard kb)
    {
        if (Pressed(kb, UpKey)) GhostStep(0, 1);
        if (Pressed(kb, DownKey)) GhostStep(0, -1);
        if (Pressed(kb, LeftKey)) GhostStep(-1, 0);
        if (Pressed(kb, RightKey)) GhostStep(1, 0);
        if (Pressed(kb, UndoKey)) SetCell(limitCenter); // X: back to the fly
    }

    void GhostStep(int dx, int dy)
    {
        var next = cell + new Vector2Int(dx, dy);
        if (InBoard(next)) SetCell(next);
    }

    // ---------- shared ----------
    public void SetCell(Vector2Int c)
    {
        cell = c;
        transform.position = board.PieceToWorld(cell); // in front of the 3D board face
    }

    static bool Pressed(Keyboard kb, Key k) => kb[k].wasPressedThisFrame;

    public static int Move(Vector2Int d) => (d.x != 0 && d.y != 0) ? 2 : 1;
    public static bool InBoard(Vector2Int p) => p.x >= 0 && p.x < 5 && p.y >= 0 && p.y < 5;
}