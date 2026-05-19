using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Persistent run stats
    public int   Parts        { get; private set; }
    public int   BestDepth    { get; private set; }
    public int   TotalDives   { get; private set; }

    // Current run — set by BoardManager at run end, read by UpgradeScreen
    public int   LastRunDepth { get; private set; }
    public int   LastRunScrap { get; private set; }

    // Applied upgrades (modified by SkillTree, read by gameplay systems)
    public float DiveTimerBonus       { get; private set; }
    public float MilestoneBonusExtra  { get; private set; }
    public float GravityMultiplier    { get; private set; } = 1f;
    public int   ExtractionCharges    { get; private set; }
    public bool  HasLockDelay         { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSave();
    }

    // Called by BoardManager when a run ends
    public void OnRunEnd(int depthReached, int scrapEarned)
    {
        LastRunDepth = depthReached;
        LastRunScrap = scrapEarned;
        TotalDives++;

        if (depthReached > BestDepth)
            BestDepth = depthReached;

        int partsGained = scrapEarned / GameConstants.ScrapToPartsRate;
        Parts += partsGained;

        SaveGame();
        LoadUpgradeScene();
    }

    // Called by UpgradeScreen when the player starts the next dive
    public void OnDiveStart()
    {
        SaveGame();
        LoadGameScene();
    }

    // Called by SkillTree nodes when purchased
    public bool SpendParts(int cost)
    {
        if (Parts < cost) return false;
        Parts -= cost;
        SaveGame();
        return true;
    }

    public void ApplyUpgrades(UpgradeProfile profile)
    {
        DiveTimerBonus      = profile.diveTimerBonus;
        MilestoneBonusExtra = profile.milestoneBonusExtra;
        GravityMultiplier   = profile.gravityMultiplier;
        ExtractionCharges   = GameConstants.ExtractionChargesDefault + profile.extraExtractionCharges;
        HasLockDelay        = profile.hasLockDelay;
    }

    public float GetDiveTimer()   => GameConstants.DiveTimerDefault + DiveTimerBonus;
    public float GetMilestoneBonus() => GameConstants.MilestoneTimeBonus + MilestoneBonusExtra;
    public float GetGravityInterval() => GameConstants.GravityIntervalDefault / GravityMultiplier;
    public int   GetExtractionCharges() => ExtractionCharges > 0
                                            ? ExtractionCharges
                                            : GameConstants.ExtractionChargesDefault;

    void LoadGameScene()    => SceneManager.LoadScene(GameConstants.SceneGame);
    void LoadUpgradeScene() => SceneManager.LoadScene(GameConstants.SceneUpgrade);

    // --- Persistence (PlayerPrefs for prototype) ---
    void SaveGame()
    {
        PlayerPrefs.SetInt("Parts",      Parts);
        PlayerPrefs.SetInt("BestDepth",  BestDepth);
        PlayerPrefs.SetInt("TotalDives", TotalDives);
        PlayerPrefs.Save();
    }

    void LoadSave()
    {
        Parts      = PlayerPrefs.GetInt("Parts",      0);
        BestDepth  = PlayerPrefs.GetInt("BestDepth",  0);
        TotalDives = PlayerPrefs.GetInt("TotalDives", 0);

        // Default upgrade values until SkillTree is built
        ExtractionCharges = GameConstants.ExtractionChargesDefault;
    }

    // Dev helper — wipe save in Editor via GameObject context menu
    [ContextMenu("Reset Save Data")]
    void ResetSave()
    {
        PlayerPrefs.DeleteAll();
        Parts = BestDepth = TotalDives = 0;
        Debug.Log("Save data cleared.");
    }
}
