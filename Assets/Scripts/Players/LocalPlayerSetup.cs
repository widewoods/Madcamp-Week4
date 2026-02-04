using Unity.Netcode;
using UnityEngine;

public class LocalPlayerSetup : NetworkBehaviour
{
  public override void OnNetworkSpawn()
  {
    if (!IsOwner) return;

    var interactor = GetComponent<PlayerInteractor>();
    var ui = FindFirstObjectByType<InteractionPromptUI>(FindObjectsInactive.Include);

    ui.Bind(interactor);

    var name = GetComponent<PlayerName>();
    var nameUi = FindFirstObjectByType<PlayerNameInputUI>(FindObjectsInactive.Include);
    if (name != null && nameUi != null)
      nameUi.Bind(name);

    var router = GetComponent<MinigameInputRouter>();
    var promptUi = FindFirstObjectByType<MinigamePromptUI>(FindObjectsInactive.Include);
    if (router != null && promptUi != null)
      promptUi.Bind(router);

    var minigameUis = FindObjectsByType<MinigameUIVisibility>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    if (router != null && minigameUis != null)
    {
      foreach (var minigameUi in minigameUis)
      {
        minigameUi.Bind(router);
      }
    }

    var minigameObjects = FindObjectsByType<MinigameObjectVisibility>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    if (router != null && minigameObjects != null)
    {
      foreach (var obj in minigameObjects)
      {
        obj.Bind(router);
      }
    }
  }
}
