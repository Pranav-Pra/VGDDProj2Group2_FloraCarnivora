using UnityEngine;

public class BoardGrid : MonoBehaviour
{
    public float cellSize = 1f;
    public float gap = 0.05f;
    [Tooltip("How deep each cube is (away from the camera).")]
    public float cellDepth = 1f;
    [Tooltip("How far pieces (fly, ghost, hand) float in front of the board face, toward the camera.")]
    public float pieceOffset = 0.05f;

    public Color colorA;
    public Color colorB;
    public Color burgerColor;

    [Tooltip("Layer for the cells, so mouse raycasts only hit the board. Create it in Tags & Layers.")]
    public string cellLayer = "Board";

    public Color breathColor;
    public float breathSpeed = 1f;

    bool breathing;
    bool hasHover;          // the cell under mouse breathes
    Vector2Int hoverCell;

    MeshRenderer[,] cells = new MeshRenderer[5, 5]; // the FRONT FACE of each cube (board colors live here)

    void Awake()
    {
        int layer = LayerMask.NameToLayer(cellLayer);
        if (layer < 0) Debug.LogWarning($"Layer '{cellLayer}' not found, cells stay on Default layer");

        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube); 
                go.name = $"Cell_{x}_{y}";
                go.transform.SetParent(transform, false);
                go.transform.rotation = transform.rotation;
                go.transform.position = CellCenter(new Vector2Int(x, y));
                go.transform.localScale = new Vector3(cellSize, cellSize, cellDepth);
                if (layer >= 0) go.layer = layer;

                var face = GameObject.CreatePrimitive(PrimitiveType.Quad);
                face.name = "Face";
                Destroy(face.GetComponent<Collider>()); 
                face.transform.SetParent(go.transform, false);
                face.transform.localPosition = new Vector3(0f, 0f, -0.501f); 
                face.transform.localRotation = Quaternion.identity;
                face.transform.localScale = Vector3.one;
                if (layer >= 0) face.layer = layer;

                cells[x, y] = face.GetComponent<MeshRenderer>();
            }
        ResetColors();
    }

    public Vector3 CellToWorld(Vector2Int c)
    {
        float step = cellSize + gap;
        return transform.position
             + transform.right * ((c.x - 2) * step)
             + transform.up * ((c.y - 2) * step);
    }

    public Vector3 PieceToWorld(Vector2Int c) => CellToWorld(c) - transform.forward * pieceOffset;

    Vector3 CellCenter(Vector2Int c) => CellToWorld(c) + transform.forward * (cellDepth * 0.5f);

    public bool WorldToCell(Vector3 world, out Vector2Int c)
    {
        float step = cellSize + gap;
        Vector3 local = world - transform.position;
        c = new Vector2Int(
            Mathf.RoundToInt(Vector3.Dot(local, transform.right) / step) + 2,
            Mathf.RoundToInt(Vector3.Dot(local, transform.up) / step) + 2);
        return GridMover.InBoard(c);
    }

    public void SetCellColor(Vector2Int c, Color color)
    {
        cells[c.x, c.y].material.color = color;
    }

    void OnDrawGizmos()
    {
        var size = new Vector3(cellSize, cellSize, cellDepth);
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
            {
                Gizmos.color = (x == 2 && y == 2) ? Color.yellow : Color.green;
                Gizmos.matrix = Matrix4x4.TRS(CellCenter(new Vector2Int(x, y)), transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, size);
            }
        Gizmos.matrix = Matrix4x4.identity;
    }

    public void SetBreathing(bool on)
    {
        breathing = on;
        if (!on) ResetColors();
    }

    public void SetHover(bool on, Vector2Int c)
    {
        if (hasHover && (!on || c != hoverCell))
            cells[hoverCell.x, hoverCell.y].material.color = BaseColor(hoverCell.x, hoverCell.y); // restore old one
        hasHover = on;
        hoverCell = c;
    }

    public void ClearHover() => SetHover(false, default);

    void Update()
    {
        if (!breathing && !hasHover) return;
        float t = (Mathf.Sin(Time.time * breathSpeed * 2f * Mathf.PI) + 1f) * 0.5f; // 0~1

        if (breathing)
            for (int dx = -1; dx <= 1; dx += 2)
                for (int dy = -1; dy <= 1; dy += 2)
                {
                    int x = 2 + dx, y = 2 + dy;
                    cells[x, y].material.color = Color.Lerp(BaseColor(x, y), breathColor, t);
                }

        if (hasHover)
            cells[hoverCell.x, hoverCell.y].material.color =
                Color.Lerp(BaseColor(hoverCell.x, hoverCell.y), breathColor, t);
    }

    Color BaseColor(int x, int y)
    {
        if (x == 2 && y == 2) return burgerColor;
        return (x + y) % 2 == 0 ? colorA : colorB;
    }

    public void ResetColors()
    {
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                cells[x, y].material.color = BaseColor(x, y);
    }
}