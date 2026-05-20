using System;
using UnityEngine;

// Handles the active falling piece: input, gravity, ghost, locking.
// Disabled by RunManager during line-clear animations and run-end.
public class PieceController : MonoBehaviour
{
    // Fired when a piece locks onto the board
    public event Action<Vector2Int[]> OnPieceLocked;

    [Header("Timing")]
    [SerializeField] float dasDelay    = 0.15f; // seconds before auto-shift starts
    [SerializeField] float dasInterval = 0.05f; // seconds between auto-shifts
    [SerializeField] float lockDelay   = 0.50f; // seconds on ground before locking (0 = instant)

    Board          _board;
    BoardRenderer  _renderer;

    // Active piece state
    TetrominoType  _type;
    Vector2Int     _pivot;
    int            _rotation;
    Vector2Int[]   _cells;      // world positions of the 4 active cells
    Vector2Int[]   _ghostCells; // world positions of the ghost

    // Gravity
    float _gravityTimer;
    float _gravityInterval;

    // DAS (Delayed Auto Shift) for left/right
    float _dasTimer;
    float _dasRepeatTimer;
    int   _dasDirection; // -1, 0, +1

    // Lock delay
    bool  _isGrounded;
    float _lockTimer;
    bool  _hasLockDelay;

    Color _pieceColor;
    Color _ghostColor;

    public void Init(Board board, BoardRenderer renderer, float gravityInterval, bool hasLockDelay)
    {
        _board         = board;
        _renderer      = renderer;
        _gravityInterval = gravityInterval;
        _hasLockDelay    = hasLockDelay;
    }

    public void SpawnPiece(TetrominoType type, Vector2Int pivot)
    {
        _type     = type;
        _pivot    = pivot;
        _rotation = 0;

        _pieceColor = TetrominoData.PieceColors[(int)type];
        _ghostColor = new Color(_pieceColor.r, _pieceColor.g, _pieceColor.b, 0.28f);

        _gravityTimer  = _gravityInterval;
        _isGrounded    = false;
        _lockTimer     = lockDelay;
        _dasDirection  = 0;
        _dasTimer      = 0f;
        _dasRepeatTimer = 0f;

        RefreshCells();
        DrawPiece();
    }

    void Update()
    {
        if (_cells == null) return;

        HandleRotation();
        HandleHorizontal();
        HandleSoftDrop();
        HandleHardDrop();
        HandleGravity();
        HandleLock();
    }

    // --- Input ---

    void HandleRotation()
    {
        bool cwPressed  = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Z);
        bool ccwPressed = Input.GetKeyDown(KeyCode.X);

        if (cwPressed)  TryRotate(clockwise: true);
        if (ccwPressed) TryRotate(clockwise: false);
    }

    void HandleHorizontal()
    {
        int dir = 0;
        if (Input.GetKey(KeyCode.LeftArrow))  dir = -1;
        if (Input.GetKey(KeyCode.RightArrow)) dir =  1;

        if (dir == 0)
        {
            _dasDirection = 0;
            return;
        }

        if (dir != _dasDirection)
        {
            // New direction pressed — move immediately, reset DAS
            _dasDirection   = dir;
            _dasTimer       = 0f;
            _dasRepeatTimer = dasInterval;
            TryMove(dir, 0);
            return;
        }

        _dasTimer += Time.deltaTime;
        if (_dasTimer >= dasDelay)
        {
            _dasRepeatTimer -= Time.deltaTime;
            if (_dasRepeatTimer <= 0f)
            {
                _dasRepeatTimer = dasInterval;
                TryMove(dir, 0);
            }
        }
    }

    void HandleSoftDrop()
    {
        if (!Input.GetKey(KeyCode.DownArrow)) return;
        if (TryMove(0, -1))
        {
            _gravityTimer = _gravityInterval; // reset gravity on manual drop
            _isGrounded   = false;
        }
    }

    void HandleHardDrop()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        // Drop until grounded
        while (TryMove(0, -1)) { }
        LockPiece();
    }

    void HandleGravity()
    {
        _gravityTimer -= Time.deltaTime;
        if (_gravityTimer <= 0f)
        {
            _gravityTimer = _gravityInterval;
            if (!TryMove(0, -1))
                _isGrounded = true;
        }
    }

    void HandleLock()
    {
        if (!_isGrounded) return;

        // If the piece can now move down again (e.g. board changed), cancel grounded
        if (CanDrop(_pivot, _rotation))
        {
            _isGrounded = false;
            return;
        }

        if (!_hasLockDelay)
        {
            LockPiece();
            return;
        }

        _lockTimer -= Time.deltaTime;
        if (_lockTimer <= 0f)
            LockPiece();
    }

    // --- Movement & Rotation ---

    bool TryMove(int dx, int dy)
    {
        var newPivot = new Vector2Int(_pivot.x + dx, _pivot.y + dy);
        if (!CanPlace(newPivot, _rotation)) return false;

        ErasePiece();
        _pivot = newPivot;
        RefreshCells();
        DrawPiece();
        return true;
    }

    void TryRotate(bool clockwise)
    {
        int newRot = (clockwise ? _rotation + 1 : _rotation + 3) & 3;
        var kicks  = TetrominoData.GetKickOffsets(_type, _rotation, clockwise);

        foreach (var kick in kicks)
        {
            var testPivot = new Vector2Int(_pivot.x + kick.x, _pivot.y + kick.y);
            if (!CanPlace(testPivot, newRot)) continue;

            ErasePiece();
            _pivot    = testPivot;
            _rotation = newRot;

            // Successful rotation resets lock delay
            _isGrounded = false;
            _lockTimer  = lockDelay;

            RefreshCells();
            DrawPiece();
            return;
        }
    }

    // True if piece at pivot+rot can exist there — only hard rock and board edges block it.
    // Soft terrain is passable: pieces fall through soil and carve it on landing.
    bool CanPlace(Vector2Int pivot, int rot)
    {
        foreach (var offset in TetrominoData.GetCells(_type, rot))
        {
            int col = pivot.x + offset.x;
            int row = pivot.y + offset.y;
            if (!_board.CanPieceEnter(col, row)) return false;
        }
        return true;
    }

    // Check if piece can move down one step
    bool CanDrop(Vector2Int pivot, int rot)
    {
        var below = new Vector2Int(pivot.x, pivot.y - 1);
        return CanPlace(below, rot);
    }

    void LockPiece()
    {
        var carvedCells = _cells;
        ErasePiece();
        _board.CarveCells(carvedCells);     // carve the piece's shape out of the terrain
        _renderer.RefreshAll(_board);
        _cells      = null;
        _ghostCells = null;
        OnPieceLocked?.Invoke(carvedCells); // RunManager controls enabled state from here
    }

    // --- Cell calculation ---

    void RefreshCells()
    {
        var offsets = TetrominoData.GetCells(_type, _rotation);
        _cells = new Vector2Int[4];
        for (int i = 0; i < 4; i++)
            _cells[i] = new Vector2Int(_pivot.x + offsets[i].x, _pivot.y + offsets[i].y);

        _ghostCells = CalcGhost();
    }

    Vector2Int[] CalcGhost()
    {
        var ghostPivot = _pivot;
        while (CanPlace(new Vector2Int(ghostPivot.x, ghostPivot.y - 1), _rotation))
            ghostPivot.y--;

        var offsets = TetrominoData.GetCells(_type, _rotation);
        var ghost   = new Vector2Int[4];
        for (int i = 0; i < 4; i++)
            ghost[i] = new Vector2Int(ghostPivot.x + offsets[i].x, ghostPivot.y + offsets[i].y);
        return ghost;
    }

    // --- Rendering ---

    void DrawPiece()
    {
        if (_ghostCells != null)
            _renderer.SetPieceCells(_ghostCells, _ghostColor, sortOrder: 1);
        if (_cells != null)
            _renderer.SetPieceCells(_cells, _pieceColor, sortOrder: 2);
    }

    void ErasePiece()
    {
        if (_ghostCells != null) _renderer.ClearPieceCells(_ghostCells, _board);
        if (_cells      != null) _renderer.ClearPieceCells(_cells,      _board);
    }
}
