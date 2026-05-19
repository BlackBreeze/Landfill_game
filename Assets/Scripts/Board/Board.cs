using System;
using UnityEngine;

public class Board
{
    public int Width  => GameConstants.BoardWidth;
    public int Height => GameConstants.BoardHeight;

    int[,] _cells; // [col, row] — row 0 = bottom of board
    System.Random _rng;

    public Board(int seed = -1)
    {
        _cells = new int[Width, Height];
        _rng   = seed < 0 ? new System.Random() : new System.Random(seed);
    }

    // Fill the board from row 0 up to (Height - SpawnRowCount - 1)
    public void GenerateFill()
    {
        Array.Clear(_cells, 0, _cells.Length);
        int topFillRow = Height - GameConstants.SpawnRowCount - 1;

        for (int row = 0; row <= topFillRow; row++)
        {
            for (int col = 0; col < Width; col++)
            {
                double roll = _rng.NextDouble();
                if (roll < GameConstants.InitialFillDensity)
                {
                    _cells[col, row] = roll < 0.12
                        ? GameConstants.CellCompactedJunk
                        : GameConstants.CellDebris;
                }
            }
        }
    }

    // --- Accessors ---

    public int  GetCell(int col, int row)
        => InBounds(col, row) ? _cells[col, row] : GameConstants.CellDebris;

    public void SetCell(int col, int row, int value)
    { if (InBounds(col, row)) _cells[col, row] = value; }

    public bool InBounds(int col, int row)
        => col >= 0 && col < Width && row >= 0 && row < Height;

    // Treats out-of-bounds and any non-empty cell as occupied
    public bool IsOccupied(int col, int row)
        => !InBounds(col, row) || _cells[col, row] != GameConstants.CellEmpty;

    // Lock piece cells onto the board as debris
    public void LockCells(Vector2Int[] cells)
    {
        foreach (var c in cells)
            SetCell(c.x, c.y, GameConstants.CellDebris);
    }

    // --- Line logic ---

    // A line is full when every cell is occupied (CellCompactedJunk blocks completion)
    public bool IsLineFull(int row)
    {
        for (int col = 0; col < Width; col++)
        {
            int v = _cells[col, row];
            if (v == GameConstants.CellEmpty || v == GameConstants.CellCompactedJunk)
                return false;
        }
        return true;
    }

    // Tries to clear a full row.
    // If the row contains fresh CompactedJunk, cracks it and returns false (no clear yet).
    // Otherwise removes the row, collapses rows above, returns true.
    public bool TryClearLine(int row)
    {
        bool cracked = false;
        for (int col = 0; col < Width; col++)
        {
            if (_cells[col, row] == GameConstants.CellCompactedJunk)
            {
                _cells[col, row] = GameConstants.CellCompactedHit;
                cracked = true;
            }
        }
        if (cracked) return false;

        // Collapse rows above down
        for (int r = row; r < Height - 1; r++)
            for (int col = 0; col < Width; col++)
                _cells[col, r] = _cells[col, r + 1];

        // Top row becomes empty
        for (int col = 0; col < Width; col++)
            _cells[col, Height - 1] = GameConstants.CellEmpty;

        return true;
    }

    // After a real clear: shift all rows up 1 and generate a new garbage row at the bottom.
    // This is the "descend deeper" mechanic — new landfill material appears from below.
    public void InsertBottomRow(int currentDepth)
    {
        for (int r = Height - 1; r > 0; r--)
            for (int col = 0; col < Width; col++)
                _cells[col, r] = _cells[col, r - 1];

        float density = Mathf.Clamp(0.60f + currentDepth * 0.002f, 0.60f, 0.82f);
        for (int col = 0; col < Width; col++)
            _cells[col, 0] = _rng.NextDouble() < density
                ? GameConstants.CellDebris
                : GameConstants.CellEmpty;
    }
}
