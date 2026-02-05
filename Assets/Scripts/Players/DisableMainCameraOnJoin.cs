using Unity.Netcode;
using UnityEngine;

public class DisableMainCameraOnJoin : NetworkBehaviour
{
  [Header("Menu Camera")]
  [SerializeField] private string menuCameraName = "Main Camera";
  [SerializeField] private string menuCameraTag = "MainCamera";
  [SerializeField] private bool fallbackToMainCamera = true;

  public override void OnNetworkSpawn()
  {
    if (!IsOwner) return;

    var cam = FindMenuCamera();
    if (cam == null) return;

    cam.gameObject.SetActive(false);
  }

  private Camera FindMenuCamera()
  {
    if (!string.IsNullOrWhiteSpace(menuCameraName))
    {
      var byName = GameObject.Find(menuCameraName);
      if (byName != null)
      {
        var cam = byName.GetComponent<Camera>();
        if (cam != null) return cam;
      }
    }

    if (!string.IsNullOrWhiteSpace(menuCameraTag))
    {
      var tagged = GameObject.FindWithTag(menuCameraTag);
      if (tagged != null)
      {
        var cam = tagged.GetComponent<Camera>();
        if (cam != null) return cam;
      }
    }

    if (fallbackToMainCamera)
      return Camera.main;

    return null;
  }
}
