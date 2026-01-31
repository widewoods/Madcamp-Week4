using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BowlingManager : NetworkBehaviour
{
  [Header("Pins")]
  [SerializeField] private Transform pinStackRoot;

  [Header("Knock Detection")]
  [SerializeField] private float minUpDot = 0.6f;
  [SerializeField] private float minHeight = 0.1f;

  private readonly NetworkVariable<int> score = new NetworkVariable<int>(
    0,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  private readonly NetworkVariable<int> shotsFired = new NetworkVariable<int>(
    0,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  private readonly NetworkVariable<ulong> lastRollerId = new NetworkVariable<ulong>(
    ulong.MaxValue,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  [SerializeField] private List<Transform> pins = new List<Transform>();
  private readonly List<Vector3> pinLocalPositions = new List<Vector3>();
  private readonly List<Quaternion> pinLocalRotations = new List<Quaternion>();
  private readonly List<bool> pinKnocked = new List<bool>();
  private readonly List<PinVisibility> pinVis = new List<PinVisibility>();

  public int Score => score.Value;
  public int ShotsFired => shotsFired.Value;
  public ulong LastRollerId => lastRollerId.Value;

  public override void OnNetworkSpawn()
  {
    if (!IsServer) return;
    EnsurePinStack();
  }

  public void ServerRegisterShot(ulong rollerClientId)
  {
    if (!IsServer) return;

    lastRollerId.Value = rollerClientId;
    shotsFired.Value = Mathf.Clamp(shotsFired.Value + 1, 0, 2);

    int newlyKnocked = CountNewlyKnockedPins();
    score.Value += newlyKnocked;

    ResetStandingPins();

    if (shotsFired.Value >= 2)
      ResetPins();
  }

  private int CountNewlyKnockedPins()
  {
    int count = 0;
    for (int i = 0; i < pins.Count; i++)
    {
      var pin = pins[i];
      if (pin == null) continue;
      if (i >= pinKnocked.Count) continue;
      if (pinKnocked[i]) continue;
      if (pin.position.y < minHeight || Vector3.Dot(pin.forward, Vector3.up) < minUpDot)
      {
        pinKnocked[i] = true;
        count++;
        if (i < pinVis.Count && pinVis[i] != null)
          pinVis[i].ServerSetVisible(false);
      }

    }
    return count;
  }

  private void EnsurePinStack()
  {
    if (pinStackRoot == null) return;
    CachePins();
  }

  private void ResetPins()
  {
    if (!IsServer) return;
    shotsFired.Value = 0;
    ResetPinsToStart();
  }

  private void CachePins()
  {
    pins.Clear();
    pinLocalPositions.Clear();
    pinLocalRotations.Clear();
    pinKnocked.Clear();
    pinVis.Clear();
    if (pinStackRoot == null) return;
    var rbList = pinStackRoot.GetComponentsInChildren<Rigidbody>(true);
    foreach (var rb in rbList)
    {
      pins.Add(rb.transform);
      pinLocalPositions.Add(rb.transform.localPosition);
      pinLocalRotations.Add(rb.transform.localRotation);
      pinKnocked.Add(false);
      pinVis.Add(rb.GetComponent<PinVisibility>());
    }
  }

  private void ResetPinsToStart()
  {
    if (pinStackRoot == null) return;
    for (int i = 0; i < pins.Count; i++)
    {
      var pin = pins[i];
      if (pin == null) continue;
      if (i < pinVis.Count && pinVis[i] != null)
        pinVis[i].ServerSetVisible(true);
      var rb = pin.GetComponent<Rigidbody>();
      pin.localPosition = pinLocalPositions[i];
      pin.localRotation = pinLocalRotations[i];
      if (rb != null)
      {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
      }
      if (i < pinKnocked.Count)
        pinKnocked[i] = false;
    }
  }

  private void ResetStandingPins()
  {
    if (!IsServer) return;
    if (pinStackRoot == null) return;

    for (int i = 0; i < pins.Count; i++)
    {
      var pin = pins[i];
      if (pin == null) continue;
      if (i >= pinKnocked.Count) continue;
      if (pinKnocked[i]) continue;

      var rb = pin.GetComponent<Rigidbody>();
      pin.localPosition = pinLocalPositions[i];
      pin.localRotation = pinLocalRotations[i];
      if (rb != null)
      {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
      }
    }
  }
}
