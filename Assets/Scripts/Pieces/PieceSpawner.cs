using System.Collections.Generic;
using UnityEngine;

// 7-bag randomizer. Maintains a preview queue and provides the next piece type + spawn pivot.
public class PieceSpawner : MonoBehaviour
{
    public const int PreviewCount = 3;

    // Spawn pivot: center-ish of board, near the top
    static readonly Vector2Int SpawnPivot = new Vector2Int(
        GameConstants.BoardWidth / 2,
        GameConstants.BoardHeight - 2
    );

    readonly Queue<TetrominoType> _queue = new Queue<TetrominoType>();
    readonly List<TetrominoType>  _bag   = new List<TetrominoType>(7);

    public void Init()
    {
        _queue.Clear();
        // Pre-fill queue with 2 bags so preview is always full
        FillBag();
        FillBag();
    }

    // Returns the next piece type and its spawn pivot. Refills bag if needed.
    public (TetrominoType type, Vector2Int pivot) Next()
    {
        if (_queue.Count <= PreviewCount)
            FillBag();

        var type = _queue.Dequeue();
        return (type, SpawnPivot);
    }

    // Peek at the upcoming pieces without consuming them
    public TetrominoType[] Preview()
    {
        if (_queue.Count <= PreviewCount)
            FillBag();

        var arr = _queue.ToArray();
        int count = Mathf.Min(PreviewCount, arr.Length);
        var result = new TetrominoType[count];
        for (int i = 0; i < count; i++)
            result[i] = arr[i];
        return result;
    }

    void FillBag()
    {
        _bag.Clear();
        for (int i = 0; i < 7; i++)
            _bag.Add((TetrominoType)i);

        // Fisher-Yates shuffle
        for (int i = _bag.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_bag[i], _bag[j]) = (_bag[j], _bag[i]);
        }

        foreach (var t in _bag)
            _queue.Enqueue(t);
    }
}
