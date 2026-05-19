using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Scans for full lines after a piece locks, animates clears, awards scrap.
public class LineClearSystem : MonoBehaviour
{
    // Fired when all clears (and animations) are done
    public event Action<int, int> OnClearsDone; // (linesCleared, scrapEarned)

    Board         _board;
    BoardRenderer _renderer;
    int           _currentDepth;

    public void Init(Board board, BoardRenderer renderer)
    {
        _board    = board;
        _renderer = renderer;
    }

    public void SetDepth(int depth) => _currentDepth = depth;

    // Entry point — called by RunManager after each piece lock
    public void ProcessClears() => StartCoroutine(DoClearRoutine());

    IEnumerator DoClearRoutine()
    {
        // Find all full rows (bottom to top to handle multi-line shifts correctly)
        var fullRows = new List<int>();
        for (int row = 0; row < GameConstants.BoardHeight; row++)
            if (_board.IsLineFull(row))
                fullRows.Add(row);

        if (fullRows.Count == 0)
        {
            OnClearsDone?.Invoke(0, 0);
            yield break;
        }

        // Flash all full rows simultaneously
        var flashRoutines = new Coroutine[fullRows.Count];
        for (int i = 0; i < fullRows.Count; i++)
            flashRoutines[i] = StartCoroutine(_renderer.FlashRow(fullRows[i], _board));

        // Wait for flashes to finish
        yield return new WaitForSeconds(0.18f);

        // Process clears from bottom up — each clear shifts rows so we re-scan
        int linesCleared = 0;
        int scrapEarned  = 0;

        // Re-find full rows after animation (board hasn't changed yet)
        // Process from highest row index down to avoid index shifting issues
        var toProcess = new List<int>(fullRows);
        toProcess.Sort((a, b) => b.CompareTo(a)); // descending

        foreach (int row in toProcess)
        {
            // Row index may have shifted due to previous clears, re-scan from bottom
            // Instead, do a fresh full-scan each iteration
        }

        // Simpler: scan bottom-to-top, clear one at a time (each clear collapses board)
        for (int pass = 0; pass < fullRows.Count; pass++)
        {
            // Find the lowest full row in current board state
            int clearRow = -1;
            for (int r = 0; r < GameConstants.BoardHeight; r++)
            {
                if (_board.IsLineFull(r))
                {
                    clearRow = r;
                    break;
                }
            }
            if (clearRow < 0) break;

            bool actuallyCleared = _board.TryClearLine(clearRow);
            if (actuallyCleared)
            {
                linesCleared++;
                _board.InsertTopRow(_currentDepth + linesCleared);
            }
            // If it just cracked compacted junk, that's fine — it'll clear next time
        }

        // Award scrap based on lines cleared simultaneously
        scrapEarned = linesCleared switch
        {
            1 => GameConstants.ScrapPerLine1,
            2 => GameConstants.ScrapPerLine2,
            3 => GameConstants.ScrapPerLine3,
            4 => GameConstants.ScrapPerLine4,
            _ => 0
        };

        _renderer.RefreshAll(_board);

        // Brief pause after clear so player can see the result
        if (linesCleared > 0)
            yield return new WaitForSeconds(0.08f);

        OnClearsDone?.Invoke(linesCleared, scrapEarned);
    }
}
