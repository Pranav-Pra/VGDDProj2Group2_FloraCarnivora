using UnityEngine;
public class BoardGrid : MonoBehaviour
{
    public float cellSize = 1f;    
    public float gap = 0.05f;      
    public Color colorA = new Color(0.9f, 0.9f, 0.9f);
    public Color colorB = new Color(0.8f, 0.8f, 0.8f);
    public Color burgerColor = new Color(1f, 0.8f, 0.5f);
    public int sortingOrder = 0;    

    public Color breathColor = new Color(1f, 0.55f, 0.15f);
    public float breathSpeed = 1f;  

    bool breathing;

    SpriteRenderer[,] cells = new SpriteRenderer[5, 5];

    void Awake()
    {
        var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);

        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
            {
                var go = new GameObject($"Cell_{x}_{y}");
                go.transform.position = CellToWorld(new Vector2Int(x, y));
                go.transform.localScale = Vector3.one * cellSize;
                go.transform.SetParent(transform, true);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = sortingOrder;
                cells[x, y] = sr;
            }
        ResetColors();
    }

    public Vector3 CellToWorld(Vector2Int c)
    {
        float step = cellSize + gap;
        return transform.position + new Vector3((c.x - 2) * step, (c.y - 2) * step, 0);
    }

    public void SetCellColor(Vector2Int c, Color color)
    {
        cells[c.x, c.y].color = color;
    }

    // draw gizmos
    void OnDrawGizmos()
    {
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
            {
                Gizmos.color = (x == 2 && y == 2) ? Color.yellow : Color.green;
                Gizmos.DrawWireCube(CellToWorld(new Vector2Int(x, y)), Vector3.one * cellSize);
            }
    }

    // only open in  special round
    public void SetBreathing(bool on)
    {
        breathing = on;
        if (!on) ResetColors();
    }

    void Update()
    {
        if (!breathing) return;
        float t = (Mathf.Sin(Time.time * breathSpeed * 2f * Mathf.PI) + 1f) * 0.5f; // 0~1 À´»Ø
        for (int dx = -1; dx <= 1; dx += 2)
            for (int dy = -1; dy <= 1; dy += 2)
            {
                int x = 2 + dx, y = 2 + dy;
                cells[x, y].color = Color.Lerp(BaseColor(x, y), breathColor, t);
            }
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
                cells[x, y].color = BaseColor(x, y);
    }
}