public static class GameConstants
{
    // Board
    public const int BoardWidth       = 10;
    public const int BoardHeight      = 20;   // visible rows
    public const int SpawnRowCount    = 4;    // empty rows at top before pre-fill starts
    public const float InitialFillDensity  = 0.70f; // % of cells that spawn as debris

    // Timing
    public const float DiveTimerDefault   = 90f;
    public const float MilestoneTimeBonus = 4f;   // seconds added every MilestoneInterval rows
    public const int   MilestoneInterval  = 10;   // rows between milestones

    // Gravity
    public const float GravityIntervalDefault = 1.0f; // seconds per cell drop

    // Currency
    public const int ScrapPerLine1 =  10;
    public const int ScrapPerLine2 =  30;
    public const int ScrapPerLine3 =  60;
    public const int ScrapPerLine4 = 100;
    public const int ScrapToPartsRate = 10; // 10 scrap = 1 part

    // Extraction
    public const int ExtractionChargesDefault = 2;

    // Camera — board occupies [0,BoardWidth-1] x [0,BoardHeight-1] in world space
    // 1 Unity unit = 1 tile (set sprite Pixels Per Unit to 64 to match 64x64 sprites)
    public const float CameraOrthoSize = 11f;  // shows 22 units tall: 20 board + 1 padding each side
    public static readonly UnityEngine.Vector3 CameraPosition =
        new UnityEngine.Vector3((BoardWidth - 1) * 0.5f, (BoardHeight - 1) * 0.5f, -10f);

    // Scene names
    public const string SceneGame    = "GameScene";
    public const string SceneUpgrade = "UpgradeScene";

    // Cell state values stored in board array
    public const int CellEmpty         = 0;
    public const int CellDebris        = 1;
    public const int CellCompactedJunk = 2;  // requires 2 clears
    public const int CellCompactedHit  = 3;  // cracked state after first clear
    public const int CellCreature      = 10; // 10+ reserved for creature IDs
}
