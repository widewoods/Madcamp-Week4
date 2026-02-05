using Unity.Netcode;
using UnityEngine;

public class MinigameObjectVisibility : NetworkBehaviour
{
  [Header("Rules")]
  [SerializeField] private MinigameType minigameType = MinigameType.None;
  [SerializeField] private bool showWhenActive = true;

  [Header("Targets")]
  [SerializeField] private GameObject[] targets;

  [Header("Refs")]
  [SerializeField] private MinigameInputRouter router;

  private readonly NetworkVariable<bool> isVisible = new NetworkVariable<bool>(
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  private void Awake()
  {
    ApplyVisibility(false);
  }

  public override void OnNetworkSpawn()
  {
    isVisible.OnValueChanged += OnVisibleChanged;
    OnVisibleChanged(false, isVisible.Value);
  }

  public override void OnNetworkDespawn()
  {
    isVisible.OnValueChanged -= OnVisibleChanged;
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
    if (!IsSpawned || !IsClient) return;
    bool active = type == minigameType;
    RequestSetVisibleServerRpc(showWhenActive ? active : !active);
  }

  [Rpc(SendTo.Server)]
  private void RequestSetVisibleServerRpc(bool visible)
  {
    if (!IsServer) return;
    isVisible.Value = visible;
  }

  private void OnVisibleChanged(bool previous, bool current)
  {
    ApplyVisibility(current);
  }

  private void ApplyVisibility(bool visible)
  {
    if (targets == null) return;
    for (int i = 0; i < targets.Length; i++)
    {
      var target = targets[i];
      if (target == null) continue;
      target.SetActive(visible);
    }
  }
}
