using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkConnectUI : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private Button hostButton;
  [SerializeField] private Button clientButton;

  private void OnEnable()
  {
    if (hostButton != null) hostButton.onClick.AddListener(OnHostClicked);
    if (clientButton != null) clientButton.onClick.AddListener(OnClientClicked);
    RefreshStatus();
  }

  private void OnDisable()
  {
    if (hostButton != null) hostButton.onClick.RemoveListener(OnHostClicked);
    if (clientButton != null) clientButton.onClick.RemoveListener(OnClientClicked);
  }

  private void Update()
  {
    RefreshStatus();
  }

  private void OnHostClicked()
  {
    if (NetworkManager.Singleton == null) return;
    NetworkManager.Singleton.StartHost();
  }

  private void OnClientClicked()
  {
    if (NetworkManager.Singleton == null) return;
    NetworkManager.Singleton.StartClient();
  }

  private void RefreshStatus()
  {
    if (NetworkManager.Singleton == null)
    {
      SetButtonsVisible(true);
      return;
    }

    if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
    {
      SetButtonsVisible(true);
      return;
    }

    SetButtonsVisible(false);
  }

  private void SetButtonsVisible(bool visible)
  {
    if (hostButton != null) hostButton.gameObject.SetActive(visible);
    if (clientButton != null) clientButton.gameObject.SetActive(visible);
  }
}
