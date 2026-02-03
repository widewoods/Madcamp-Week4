using UnityEngine;

public class MinigameZone : MonoBehaviour
{
  [SerializeField] private MinigameType minigameType = MinigameType.None;

  private void OnTriggerEnter(Collider other)
  {
    var router = other.GetComponentInParent<MinigameInputRouter>();
    if (router == null) return;
    router.SetActiveMinigame(minigameType);
  }

  private void OnTriggerExit(Collider other)
  {
    var router = other.GetComponentInParent<MinigameInputRouter>();
    if (router == null) return;
    router.SetActiveMinigame(MinigameType.None);
  }
}
