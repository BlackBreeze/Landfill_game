// Holds the aggregate effect of all purchased skill tree nodes.
// SkillTree writes to this; GameManager reads from it each dive start.
[System.Serializable]
public class UpgradeProfile
{
    public float diveTimerBonus;          // extra seconds added to dive timer
    public float milestoneBonusExtra;     // extra seconds per depth milestone
    public float gravityMultiplier = 1f;  // multiplier on fall speed (>1 = slower)
    public int   extraExtractionCharges;  // added on top of the default 2
    public bool  hasLockDelay;            // 0.3s lock delay before piece sets
}
