using TMPro;
using UnityEngine;

public class BowlingScoreUI : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private BowlingManager bowlingManager;
  [SerializeField] private TMP_Text scoreText;
  [SerializeField] private TMP_Text shotsText;

  private int lastScore = -1;
  private int lastShots = -1;

  private void Awake()
  {
    if (bowlingManager == null)
      bowlingManager = FindFirstObjectByType<BowlingManager>();
  }

  private void Update()
  {
    if (bowlingManager == null) return;

    int score = bowlingManager.Score;
    int shots = bowlingManager.ShotsFired;

    if (score != lastScore)
    {
      lastScore = score;
      if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    if (shots != lastShots)
    {
      lastShots = shots;
      if (shotsText != null) shotsText.text = $"Shots: {shots}/2";
    }
  }
}
