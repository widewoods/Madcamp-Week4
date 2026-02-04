using TMPro;
using UnityEngine;

public class GolfScoreUI : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private GolfManager golfManager;
  [SerializeField] private TMP_Text strokesText;

  private int lastStrokes = -1;

  private void Awake()
  {
    if (golfManager == null)
      golfManager = FindFirstObjectByType<GolfManager>();
  }

  private void Update()
  {
    if (golfManager == null) return;

    int strokes = golfManager.TotalStrokes;
    if (strokes == lastStrokes) return;

    lastStrokes = strokes;
    if (strokesText != null)
      strokesText.text = $"타수: {strokes}";
  }
}
