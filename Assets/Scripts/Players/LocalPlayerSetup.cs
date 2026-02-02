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
  }
}
