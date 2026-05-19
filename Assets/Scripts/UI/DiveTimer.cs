using System;
using UnityEngine;
using UnityEngine.UI;

// Countdown timer for a dive run. Grants milestone bonuses every N rows descended.
public class DiveTimer : MonoBehaviour
{
    public event Action OnTimerExpired;

    [Header("UI References")]
    [SerializeField] Image  timerBarFill;   // Image with fillAmount
    [SerializeField] Text   timerLabel;     // "23.4"
    [SerializeField] Color  normalColor  = Color.white;
    [SerializeField] Color  warningColor = Color.red;
    [SerializeField] float  warningThreshold = 15f;

    float _timeRemaining;
    float _maxTime;
    bool  _running;
    int   _lastMilestoneDepth;

    public float TimeRemaining => _timeRemaining;

    public void StartTimer(float duration)
    {
        _timeRemaining     = duration;
        _maxTime           = duration;
        _running           = true;
        _lastMilestoneDepth = 0;
        Refresh();
    }

    public void StopTimer() => _running = false;

    public void PauseTimer() => _running = false;
    public void ResumeTimer() => _running = true;

    // Call whenever depth increases — adds time on milestone boundaries
    public void OnDepthChanged(int newDepth, float bonusPerMilestone)
    {
        int milestonesPassed = newDepth / GameConstants.MilestoneInterval
                             - _lastMilestoneDepth / GameConstants.MilestoneInterval;

        if (milestonesPassed > 0)
        {
            float bonus = milestonesPassed * bonusPerMilestone;
            _timeRemaining = Mathf.Min(_timeRemaining + bonus, _maxTime);
            _lastMilestoneDepth = newDepth;
            ShowMilestonePopup(bonus);
        }
    }

    void Update()
    {
        if (!_running) return;

        _timeRemaining -= Time.deltaTime;

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _running       = false;
            Refresh();
            OnTimerExpired?.Invoke();
            return;
        }

        Refresh();
    }

    void Refresh()
    {
        if (timerBarFill != null)
            timerBarFill.fillAmount = _maxTime > 0f ? _timeRemaining / _maxTime : 0f;

        if (timerLabel != null)
        {
            timerLabel.text  = Mathf.CeilToInt(_timeRemaining).ToString();
            timerLabel.color = _timeRemaining <= warningThreshold ? warningColor : normalColor;
        }

        if (timerBarFill != null)
            timerBarFill.color = _timeRemaining <= warningThreshold ? warningColor : normalColor;
    }

    void ShowMilestonePopup(float bonus)
    {
        // Placeholder — a floating text popup can be added here later
        Debug.Log($"[Timer] Milestone! +{bonus:F0}s");
    }
}
