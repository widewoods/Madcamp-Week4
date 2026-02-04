using UnityEngine;

public class MinigameObjectVisibility : MonoBehaviour
{
  [Header("Rules")]
  [SerializeField] private MinigameType minigameType = MinigameType.None;
  [SerializeField] private bool showWhenActive = true;

  [Header("Targets")]
  [SerializeField] private GameObject[] targets;

  [Header("Refs")]
  [SerializeField] private MinigameInputRouter router;

  private void Awake()
  {
    ApplyVisibility(false);
  }

  private void OnEnable()
  {
    ResolveRouter();
    if (router != null)
      router.OnMinigameChanged += HandleMinigameChanged;
    HandleMinigameChanged(router != null ? router.ActiveMinigame : MinigameType.None);
  }

  private void OnDisable()
  {
    if (router != null)
      router.OnMinigameChanged -= HandleMinigameChanged;
  }

  public void Bind(MinigameInputRouter inputRouter)
  {
    if (router != null)
      router.OnMinigameChanged -= HandleMinigameChanged;

    router = inputRouter;
    if (router != null)
      router.OnMinigameChanged += HandleMinigameChanged;

    HandleMinigameChanged(router != null ? router.ActiveMinigame : MinigameType.None);
  }

  private void ResolveRouter()
  {
    if (router == null)
      router = GetComponentInParent<MinigameInputRouter>();
  }

  private void HandleMinigameChanged(MinigameType type)
  {
    bool active = type == minigameType;
    ApplyVisibility(showWhenActive ? active : !active);
  }

  private void ApplyVisibility(bool visible)
  {
    if (targets == null) return;
    for (int i = 0; i < targets.Length; i++)
    {
      if (targets[i] != null)
        targets[i].SetActive(visible);
    }
  }
}
