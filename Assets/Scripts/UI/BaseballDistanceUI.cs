using TMPro;
using UnityEngine;

public class BaseballDistanceUI : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private BaseballManager baseballManager;
  [SerializeField] private TMP_Text distanceText;

  private float lastDistance = -1f;

  private void Awake()
  {
    if (baseballManager == null)
      baseballManager = FindFirstObjectByType<BaseballManager>();
  }

  private void Update()
  {
    if (baseballManager == null) return;

    float distance = baseballManager.LastDistance;
    if (Mathf.Approximately(distance, lastDistance)) return;

    lastDistance = distance;
    if (distanceText != null)
      distanceText.text = $"비거리: {distance:0.0}m";
  }
}
