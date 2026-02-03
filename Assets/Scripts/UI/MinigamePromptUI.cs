using TMPro;
using UnityEngine;

public class MinigamePromptUI : MonoBehaviour
{
  [SerializeField] private TMP_Text promptText;
  [SerializeField] private GameObject root;
  [SerializeField] private MinigamePrompt[] prompts;

  private MinigameInputRouter router;

  public void Bind(MinigameInputRouter inputRouter)
  {
    if (router != null)
      router.OnMinigameChanged -= HandleMinigameChanged;

    router = inputRouter;
    if (router != null)
      router.OnMinigameChanged += HandleMinigameChanged;
  }

  private void Awake()
  {
    if (root != null) root.SetActive(false);
  }

  private void OnEnable()
  {
    if (router != null)
      router.OnMinigameChanged += HandleMinigameChanged;
  }

  private void OnDisable()
  {
    if (router != null)
      router.OnMinigameChanged -= HandleMinigameChanged;
  }

  private void HandleMinigameChanged(MinigameType type)
  {
    if (root == null || promptText == null) return;
    if (type == MinigameType.None)
    {
      root.SetActive(false);
      return;
    }

    string prompt = GetPrompt(type);
    promptText.text = prompt;
    root.SetActive(true);
  }

  private string GetPrompt(MinigameType type)
  {
    foreach (var p in prompts)
    {
      if (p.type == type && !string.IsNullOrWhiteSpace(p.prompt))
        return p.prompt;
    }
    return "Press F";
  }
}
