using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class NetworkConnectUI : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private Button hostButton;
  [SerializeField] private Button clientButton;
  [SerializeField] private TMP_InputField joinCodeInput;
  [SerializeField] private TMP_Text joinCodeText;
  [SerializeField] private TMP_Text statusText;

  [Header("Relay")]
  [SerializeField] private bool useRelay = true;
  [SerializeField] private int maxRelayConnections = 10;
  [SerializeField] private string relayConnectionType = "dtls";

  private bool isBusy;

  private void OnEnable()
  {
    if (hostButton != null) hostButton.onClick.AddListener(OnHostClicked);
    if (clientButton != null) clientButton.onClick.AddListener(OnClientClicked);
    _ = InitializeRelayServicesAsync();
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

  private async void OnHostClicked()
  {
    if (NetworkManager.Singleton == null) return;
    if (isBusy) return;

    if (!useRelay)
    {
      NetworkManager.Singleton.StartHost();
      return;
    }

    isBusy = true;
    SetStatus("Creating relay...");
    try
    {
      await InitializeRelayServicesAsync();
      var transport = GetTransport();
      if (transport == null) return;

      Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxRelayConnections);
      string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

      SetRelayHostData(transport, allocation);
      if (NetworkManager.Singleton.StartHost())
      {
        if (joinCodeText != null)
          joinCodeText.text = $"참가 코드: {joinCode}";
        SetStatus($"참가 코드: {joinCode}");
      }
      else
      {
        SetStatus("Failed to start host.");
      }
    }
    catch (System.Exception ex)
    {
      Debug.LogError($"[NetworkConnectUI] Relay host failed: {ex.Message}");
      SetStatus("Relay host failed. Check Console.");
    }
    finally
    {
      isBusy = false;
    }
  }

  private async void OnClientClicked()
  {
    if (NetworkManager.Singleton == null) return;
    if (isBusy) return;

    if (!useRelay)
    {
      NetworkManager.Singleton.StartClient();
      return;
    }

    if (joinCodeInput == null || string.IsNullOrWhiteSpace(joinCodeInput.text))
    {
      SetStatus("Enter a join code.");
      return;
    }

    isBusy = true;
    SetStatus("Joining relay...");
    try
    {
      await InitializeRelayServicesAsync();
      var transport = GetTransport();
      if (transport == null) return;

      JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCodeInput.text.Trim());
      SetRelayClientData(transport, joinAllocation);

      if (NetworkManager.Singleton.StartClient())
      {
        SetStatus("Joined relay.");
      }
      else
      {
        SetStatus("Failed to start client.");
      }
    }
    catch (System.Exception ex)
    {
      Debug.LogError($"[NetworkConnectUI] Relay join failed: {ex.Message}");
      SetStatus("Relay join failed. Check code and Console.");
    }
    finally
    {
      isBusy = false;
    }
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
    if (joinCodeInput != null) joinCodeInput.gameObject.SetActive(visible && useRelay);
  }

  private async System.Threading.Tasks.Task InitializeRelayServicesAsync()
  {
    if (!useRelay) return;

    if (UnityServices.State != ServicesInitializationState.Initialized)
      await UnityServices.InitializeAsync();

    if (!AuthenticationService.Instance.IsSignedIn)
      await AuthenticationService.Instance.SignInAnonymouslyAsync();
  }

  private UnityTransport GetTransport()
  {
    if (NetworkManager.Singleton == null) return null;
    var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    if (transport == null)
    {
      SetStatus("UnityTransport missing on NetworkManager.");
      Debug.LogError("[NetworkConnectUI] UnityTransport component not found.");
    }
    return transport;
  }

  private void SetStatus(string message)
  {
    if (statusText != null)
      statusText.text = message;
  }

  private void SetRelayHostData(UnityTransport transport, Allocation allocation)
  {
    bool isSecure = IsSecureRelayType(relayConnectionType);
    transport.SetRelayServerData(
      allocation.RelayServer.IpV4,
      (ushort)allocation.RelayServer.Port,
      allocation.AllocationIdBytes,
      allocation.Key,
      allocation.ConnectionData,
      allocation.ConnectionData,
      isSecure
    );
  }

  private void SetRelayClientData(UnityTransport transport, JoinAllocation joinAllocation)
  {
    bool isSecure = IsSecureRelayType(relayConnectionType);
    transport.SetRelayServerData(
      joinAllocation.RelayServer.IpV4,
      (ushort)joinAllocation.RelayServer.Port,
      joinAllocation.AllocationIdBytes,
      joinAllocation.Key,
      joinAllocation.ConnectionData,
      joinAllocation.HostConnectionData,
      isSecure
    );
  }

  private static bool IsSecureRelayType(string connectionType)
  {
    return string.Equals(connectionType, "dtls", System.StringComparison.OrdinalIgnoreCase)
      || string.Equals(connectionType, "wss", System.StringComparison.OrdinalIgnoreCase);
  }
}
