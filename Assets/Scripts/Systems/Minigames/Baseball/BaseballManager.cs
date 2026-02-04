using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BaseballManager : NetworkBehaviour
{
  [Header("Refs")]
  [SerializeField] private Transform pitcherPoint;
  [SerializeField] private Transform platePoint;
  [SerializeField] private BaseballBall ballPrefab;

  [Header("Pitch")]
  [SerializeField] private float pitchSpeed = 12f;
  [SerializeField] private float pitchDelay = 1.5f;

  [Header("Hit Window")]
  [SerializeField] private float plateRadius = 0.8f;
  [SerializeField] private float swingWindowSeconds = 0.35f;

  [Header("Hit Power")]
  [SerializeField] private float minHitForce = 4f;
  [SerializeField] private float maxHitForce = 16f;

  private BaseballBall currentBall;
  private bool pitchScheduled;

  private readonly NetworkVariable<float> lastDistance = new NetworkVariable<float>(
    0f,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  private readonly NetworkVariable<ulong> lastHitterId = new NetworkVariable<ulong>(
    ulong.MaxValue,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  public float LastDistance => lastDistance.Value;
  public ulong LastHitterId => lastHitterId.Value;

  public override void OnNetworkSpawn()
  {
    if (!IsServer) return;
  }

  public override void OnNetworkDespawn()
  {
    if (!IsServer) return;
  }

  private void Update()
  {
    if (!IsServer) return;
    if (currentBall == null) return;
    if (!currentBall.InPlay) return;
    float distance = Vector3.Distance(currentBall.HitStartPosition, currentBall.transform.position);
    lastDistance.Value = distance;
    if (!currentBall.IsStopped()) return;

    DespawnCurrentBall();
  }

  public void ServerSchedulePitch()
  {
    if (!IsServer) return;
    if (pitchScheduled) return;
    pitchScheduled = true;
    StartCoroutine(PitchAfterDelay());
  }

  private System.Collections.IEnumerator PitchAfterDelay()
  {
    yield return new WaitForSeconds(pitchDelay);
    pitchScheduled = false;
    ServerPitchNow();
  }

  private void ServerPitchNow()
  {
    if (!IsServer) return;
    if (currentBall != null)
    {
      return;
    }
    if (ballPrefab == null || pitcherPoint == null || platePoint == null) return;

    var ball = Instantiate(ballPrefab, pitcherPoint.position, Quaternion.identity);
    var netObj = ball.GetComponent<NetworkObject>();
    if (netObj == null) return;
    netObj.Spawn();

    currentBall = ball;

    Vector3 dir = (platePoint.position - pitcherPoint.position).normalized;
    ball.ServerLaunch(dir * pitchSpeed);
  }

  public void ServerTrySwing(ulong clientId, Vector3 position)
  {
    if (!IsServer) return;
    if (currentBall == null) return;
    if (currentBall.InPlay) return;
    if (platePoint == null) return;

    float dist = Vector3.Distance(currentBall.transform.position, platePoint.position);
    if (dist > plateRadius)
    {
      lastDistance.Value = 0;
      DespawnAfterSeconds(10);
      return;
    }

    float force = Mathf.Lerp(minHitForce, maxHitForce, (plateRadius - dist) / plateRadius);
    currentBall.ServerHit(position, force);
    lastHitterId.Value = clientId;
    SfxNetEmitter.Instance?.ServerPlay(SfxId.BaseballHit, currentBall.transform.position);
  }

  IEnumerator DespawnAfterSeconds(float seconds)
  {
    yield return new WaitForSeconds(seconds);

    DespawnCurrentBall();
  }

  private void DespawnCurrentBall()
  {
    if (currentBall == null) return;
    var netObj = currentBall.GetComponent<NetworkObject>();
    if (netObj != null && netObj.IsSpawned)
      netObj.Despawn(true);
    currentBall = null;
  }
}
