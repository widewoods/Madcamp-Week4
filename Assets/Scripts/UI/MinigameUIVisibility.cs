using UnityEngine;

public class MinigameUIVisibility : MonoBehaviour
{
  [SerializeField] private MinigameType minigameType = MinigameType.None;
  [SerializeField] private GameObject root;

  private MinigameInputRouter router;

  private void Awake()
  {
    if (root != null) root.SetActive(false);
  }

  void OnDisable()
  {
    if (router != null)
      router.OnMinigameChanged -= HandleMinigameChanged;
  }

  public void Bind(MinigameInputRouter inputRouter)
  {
    router = inputRouter;
    if (router != null)
      router.OnMinigameChanged += HandleMinigameChanged;
  }

  private void HandleMinigameChanged(MinigameType type)
  {
    if (root == null) return;
    root.SetActive(type == minigameType);
  }
}
