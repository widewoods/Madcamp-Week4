using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
  [SerializeField] private TMP_Text promptText;
  [SerializeField] private GameObject root;

  public void Bind(PlayerInteractor interactor)
  {
    interactor.OnTargetChanged += HandleTargetChanged;
    HandleTargetChanged(null);
  }

  private void HandleTargetChanged(InteractableBase target)
  {
    if (target == null)
    {
      root.SetActive(false);
      return;
    }

    root.SetActive(true);
    promptText.text = $"E - {target.Prompt}";
  }
}
