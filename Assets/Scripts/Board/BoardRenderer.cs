using UnityEngine;

// Renders the board state as a grid of SpriteRenderers.
// Assign sprites in the Inspector to use art assets; falls back to tinted white squares.
public class BoardRenderer : MonoBehaviour
{
    [Header("Art (optional — fallback uses tinted squares)")]
    [SerializeField] Sprite debrisSprite;
    [SerializeField] Sprite compactedJunkSprite;
    [SerializeField] Sprite compactedHitSprite;

    [Header("Colors (used when no sprite is assigned)")]
    [SerializeField] Color emptyColor        = new Color(0.10f, 0.10f, 0.12f, 1f);
    [SerializeField] Color debrisColor       = new Color(0.50f, 0.50f, 0.52f, 1f);
    [SerializeField] Color compactedColor    = new Color(0.38f, 0.20f, 0.10f, 1f);
    [SerializeField] Color compactedHitColor = new Color(0.60f, 0.35f, 0.15f, 1f);

    SpriteRenderer[,] _renderers;
    Sprite _defaultSprite;

    public void Init()
    {
        _defaultSprite = CreateWhiteSquare();
        _renderers = new SpriteRenderer[GameConstants.BoardWidth, GameConstants.BoardHeight];

        for (int row = 0; row < GameConstants.BoardHeight; row++)
        {
            for (int col = 0; col < GameConstants.BoardWidth; col++)
            {
                var go = new GameObject($"Cell_{col}_{row}");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(col, row, 0f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite       = _defaultSprite;
                sr.color        = emptyColor;
                sr.sortingOrder = 0;
                _renderers[col, row] = sr;
            }
        }
    }

    public void RefreshAll(Board board)
    {
        for (int row = 0; row < GameConstants.BoardHeight; row++)
            for (int col = 0; col < GameConstants.BoardWidth; col++)
                RefreshCell(col, row, board.GetCell(col, row));
    }

    public void RefreshCell(int col, int row, int cellValue)
    {
        if (_renderers == null || !board_InBounds(col, row)) return;
        var sr = _renderers[col, row];

        switch (cellValue)
        {
            case GameConstants.CellEmpty:
                sr.sprite = _defaultSprite;
                sr.color  = emptyColor;
                break;
            case GameConstants.CellDebris:
                sr.sprite = debrisSprite != null ? debrisSprite : _defaultSprite;
                sr.color  = debrisColor;
                break;
            case GameConstants.CellCompactedJunk:
                sr.sprite = compactedJunkSprite != null ? compactedJunkSprite : _defaultSprite;
                sr.color  = compactedColor;
                break;
            case GameConstants.CellCompactedHit:
                sr.sprite = compactedHitSprite != null ? compactedHitSprite : _defaultSprite;
                sr.color  = compactedHitColor;
                break;
            default:
                sr.sprite = _defaultSprite;
                sr.color  = debrisColor;
                break;
        }
    }

    // Flash a row white then restore — called during line clear animation
    public System.Collections.IEnumerator FlashRow(int row, Board board)
    {
        if (!board_InBounds(0, row)) yield break;

        for (int col = 0; col < GameConstants.BoardWidth; col++)
            _renderers[col, row].color = Color.white;

        yield return new WaitForSeconds(0.08f);

        for (int col = 0; col < GameConstants.BoardWidth; col++)
            _renderers[col, row].color = new Color(0.9f, 0.9f, 0.9f, 1f);

        yield return new WaitForSeconds(0.06f);

        // Restore to current board state
        for (int col = 0; col < GameConstants.BoardWidth; col++)
            RefreshCell(col, row, board.GetCell(col, row));
    }

    // Show active piece cells and ghost cells
    public void SetPieceCells(Vector2Int[] cells, Color color, int sortOrder = 2)
    {
        foreach (var c in cells)
            if (board_InBounds(c.x, c.y))
            {
                _renderers[c.x, c.y].color        = color;
                _renderers[c.x, c.y].sortingOrder  = sortOrder;
            }
    }

    // Clear piece rendering (restore underlying board cell)
    public void ClearPieceCells(Vector2Int[] cells, Board board)
    {
        foreach (var c in cells)
            if (board_InBounds(c.x, c.y))
            {
                RefreshCell(c.x, c.y, board.GetCell(c.x, c.y));
                _renderers[c.x, c.y].sortingOrder = 0;
            }
    }

    static bool board_InBounds(int col, int row)
        => col >= 0 && col < GameConstants.BoardWidth
        && row >= 0 && row < GameConstants.BoardHeight;

    static Sprite CreateWhiteSquare()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
