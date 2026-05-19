using UnityEngine;
using UnityEngine.UI;

// Phase 1 stub — shows run stats and a Start Dive button.
// Upgrade nodes are display-only placeholders until Phase 3.
public class UpgradeScreen : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] Text partsLabel;
    [SerializeField] Text lastDepthLabel;
    [SerializeField] Text lastScrapLabel;
    [SerializeField] Text bestDepthLabel;
    [SerializeField] Text totalDivesLabel;

    [Header("Upgrade node buttons (stub)")]
    [SerializeField] Button[] upgradeButtons; // 7 placeholder buttons

    [Header("Actions")]
    [SerializeField] Button startDiveButton;

    void Start()
    {
        var gm = GameManager.Instance;

        if (gm != null)
        {
            if (partsLabel     != null) partsLabel.text      = $"Parts: {gm.Parts}";
            if (lastDepthLabel != null) lastDepthLabel.text  = $"Last Dive: {gm.LastRunDepth} rows";
            if (lastScrapLabel != null) lastScrapLabel.text  = $"Scrap Earned: {gm.LastRunScrap}";
            if (bestDepthLabel != null) bestDepthLabel.text  = $"Best Depth: {gm.BestDepth}";
            if (totalDivesLabel!= null) totalDivesLabel.text = $"Total Dives: {gm.TotalDives}";
        }

        // Stub upgrade buttons — disabled for now, will be wired in Phase 3
        if (upgradeButtons != null)
            foreach (var btn in upgradeButtons)
                if (btn != null) btn.interactable = false;

        if (startDiveButton != null)
            startDiveButton.onClick.AddListener(OnStartDive);
    }

    void OnStartDive()
    {
        var gm = GameManager.Instance;
        if (gm != null)
            gm.OnDiveStart();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameConstants.SceneGame);
    }
}
