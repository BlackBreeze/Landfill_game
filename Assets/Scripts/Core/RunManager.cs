using UnityEngine;
using UnityEngine.UI;

// Central coordinator for a single dive run.
// Lives on the Board GameObject in GameScene.
// Wire up all [SerializeField] references in the Inspector after running the Setup tool.
public class RunManager : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] BoardRenderer  boardRenderer;
    [SerializeField] PieceController pieceController;
    [SerializeField] PieceSpawner   pieceSpawner;
    [SerializeField] LineClearSystem lineClearSystem;
    [SerializeField] DiveTimer       diveTimer;

    [Header("HUD")]
    [SerializeField] Text depthLabel;
    [SerializeField] Text scrapLabel;
    [SerializeField] Text nextPieceLabel; // placeholder until proper preview is built

    [Header("Run End Overlay")]
    [SerializeField] GameObject runEndPanel;
    [SerializeField] Text       runEndDepthText;
    [SerializeField] Text       runEndScrapText;
    [SerializeField] Button     continueButton;

    Board _board;
    int   _depth;
    int   _scrap;

    // Camera scroll — tracks which board row sits at the bottom of the visible window
    int   _viewBottomRow  = GameConstants.BoardInitialViewBottom;
    float _cameraTargetY;
    const float CameraScrollSpeed = 6f;

    enum RunState { Spawning, Playing, Clearing, RunEnd }
    RunState _state;

    void Start()
    {
        // Hide run-end panel
        if (runEndPanel != null) runEndPanel.SetActive(false);

        // Position camera over the initial view window
        _cameraTargetY = ViewCenterY();
        var cam = Camera.main;
        if (cam != null)
            cam.transform.position = new Vector3(
                (GameConstants.BoardWidth - 1) * 0.5f, _cameraTargetY, -10f);

        // Build board data
        _board = new Board();
        _board.GenerateFill();

        // Init systems
        boardRenderer.Init();
        boardRenderer.RefreshAll(_board);

        pieceSpawner.Init();

        var gm = GameManager.Instance;
        float gravityInterval = gm != null ? gm.GetGravityInterval()  : GameConstants.GravityIntervalDefault;
        bool  lockDelay       = gm != null ? gm.HasLockDelay          : false;
        int   extraCharges    = gm != null ? gm.GetExtractionCharges() : GameConstants.ExtractionChargesDefault;

        pieceController.Init(_board, boardRenderer, gravityInterval, lockDelay);
        pieceController.OnPieceLocked += HandlePieceLocked;
        pieceController.enabled = false; // wait until first piece spawns

        lineClearSystem.Init(_board, boardRenderer);
        lineClearSystem.OnClearsDone += HandleClearsDone;

        float timerDuration = gm != null ? gm.GetDiveTimer() : GameConstants.DiveTimerDefault;
        diveTimer.OnTimerExpired += HandleTimerExpired;
        diveTimer.StartTimer(timerDuration);

        _depth = 0;
        _scrap = 0;
        UpdateHUD();

        // Spawn the first piece
        SpawnNext();
    }

    void Update()
    {
        var cam = Camera.main;
        if (cam == null) return;
        var pos = cam.transform.position;
        pos.y = Mathf.Lerp(pos.y, _cameraTargetY, CameraScrollSpeed * Time.deltaTime);
        cam.transform.position = pos;
    }

    void SpawnNext()
    {
        _state = RunState.Spawning;
        pieceSpawner.SetSpawnRow(_viewBottomRow + GameConstants.BoardDisplayRows - 2);
        lineClearSystem.SetViewBottom(_viewBottomRow);
        var (type, pivot) = pieceSpawner.Next();

        // Lockout check: if spawn position is occupied, run ends
        var cells = TetrominoData.GetCells(type, 0);
        foreach (var offset in cells)
        {
            int col = pivot.x + offset.x;
            int row = pivot.y + offset.y;
            if (_board.IsOccupied(col, row))
            {
                EndRun();
                return;
            }
        }

        pieceController.enabled = true;
        pieceController.SpawnPiece(type, pivot);
        _state = RunState.Playing;
    }

    void HandlePieceLocked(Vector2Int[] lockedCells)
    {
        if (_state != RunState.Playing) return;

        _state = RunState.Clearing;
        pieceController.enabled = false;
        diveTimer.PauseTimer();

        lineClearSystem.SetDepth(_depth);
        lineClearSystem.ProcessClears();
    }

    void HandleClearsDone(int linesCleared, int scrapEarned)
    {
        if (linesCleared > 0)
        {
            _depth         += linesCleared;
            _scrap         += scrapEarned;
            _viewBottomRow -= linesCleared; // scroll camera deeper
            _cameraTargetY  = ViewCenterY();

            var gm = GameManager.Instance;
            float milestoneBonus = gm != null ? gm.GetMilestoneBonus() : GameConstants.MilestoneTimeBonus;
            diveTimer.OnDepthChanged(_depth, milestoneBonus);
        }

        UpdateHUD();
        diveTimer.ResumeTimer();
        SpawnNext();
    }

    void HandleTimerExpired() => EndRun();

    void EndRun()
    {
        _state = RunState.RunEnd;
        pieceController.enabled = false;
        diveTimer.StopTimer();

        if (runEndPanel != null)
        {
            runEndPanel.SetActive(true);
            if (runEndDepthText != null) runEndDepthText.text = $"Depth: {_depth}";
            if (runEndScrapText != null) runEndScrapText.text = $"Scrap: {_scrap}";
        }

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinuePressed);
    }

    void OnContinuePressed()
    {
        var gm = GameManager.Instance;
        if (gm != null)
            gm.OnRunEnd(_depth, _scrap);
        else
        {
            // Fallback if GameManager isn't present (dev testing)
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameConstants.SceneUpgrade);
        }
    }

    void UpdateHUD()
    {
        if (depthLabel != null) depthLabel.text = $"Depth  {_depth}";
        if (scrapLabel != null) scrapLabel.text = $"Scrap  {_scrap}";
    }

    float ViewCenterY()
        => _viewBottomRow + GameConstants.BoardDisplayRows / 2f;

    // Editor helper — instant restart without going to upgrade screen
    [ContextMenu("Restart Run")]
    void RestartRun()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(GameConstants.SceneGame);
    }
}
