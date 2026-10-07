using UnityEngine;
using UnityEngine.InputSystem;

// For the GHOST: faces left on A, right on D, in real time during both turns.
// (The plant player only uses the mouse, so any A/D press belongs to the bug player.)
[RequireComponent(typeof(SpriteRenderer))]
public class GhostBug_MG : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private GridMover mover;
    private bool artFacesRight = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mover = GetComponent<GridMover>();
    }

    private void Start()
    {
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb[GridMover.LeftKey].wasPressedThisFrame) SetFacingLeft(true);
        if (kb[GridMover.RightKey].wasPressedThisFrame) SetFacingLeft(false);
    }

    public void SetFacingLeft(bool left)
    {
        spriteRenderer.flipX = artFacesRight ? left : !left;
    }

    private static bool Pressed(Keyboard kb, Key k) => k != Key.None && kb[k].wasPressedThisFrame;
}