using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GolfStrokeInput : NetworkBehaviour, IMinigameUseHandler
{
  [Header("Refs")]
  [SerializeField] private GolfBall ball;
  [SerializeField] private GolfManager manager;
  [SerializeField] private Transform aimSource;
  [SerializeField] private Image powerBar;

  [Header("Power")]
  [SerializeField] private float minForce = 2f;
  [SerializeField] private float maxForce = 12f;
  [SerializeField] private float chargeSpeed = 6f;

  private float charge;
  private bool chargingUp = true;

  private void Awake()
  {
    if (aimSource == null) aimSource = transform;
    if (manager == null) manager = FindFirstObjectByType<GolfManager>();
    if (ball == null) ball = FindFirstObjectByType<GolfBall>();
  }

  public MinigameType MinigameType => MinigameType.Golf;

  public void OnUsePressed() { }

  public void OnUseHeld()
  {
    if (!IsOwner) return;
    if (ball == null) return;
    if (ball.IsMoving())
    {
      SetPowerBarVisible(false);
      return;
    }

    float delta = chargeSpeed * Time.deltaTime;
    charge = chargingUp ? charge + delta : charge - delta;
    if (charge >= 1f)
    {
      charge = 1f;
      chargingUp = false;
    }
    else if (charge <= 0f)
    {
      charge = 0f;
      chargingUp = true;
    }
    SetPowerBarVisible(true);
    UpdatePowerBar(charge);
  }

  public void OnUseReleased()
  {
    if (!IsOwner) return;
    if (ball == null) return;
    if (ball.IsMoving())
    {
      SetPowerBarVisible(false);
      return;
    }

    float force = Mathf.Lerp(minForce, maxForce, charge);
    charge = 0f;
    chargingUp = true;
    SetPowerBarVisible(false);
    RequestStrokeServerRpc(aimSource.forward, force);
  }

  [Rpc(SendTo.Server)]
  private void RequestStrokeServerRpc(Vector3 forward, float force, RpcParams rpcParams = default)
  {
    if (ball == null) return;
    if (manager == null)
      manager = FindFirstObjectByType<GolfManager>();

    ball.ServerStroke(forward, force);
    if (manager != null)
      manager.ServerRegisterStroke(rpcParams.Receive.SenderClientId);
  }

  private void UpdatePowerBar(float value)
  {
    if (powerBar == null) return;
    powerBar.fillAmount = Mathf.Clamp01(value);
  }

  private void SetPowerBarVisible(bool visible)
  {
    if (powerBar == null) return;
    powerBar.enabled = visible;
  }
}
