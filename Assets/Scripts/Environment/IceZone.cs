using UnityEngine;
using System.Collections.Generic;

public class IceZone : MonoBehaviour
{
  [SerializeField] private bool isIce = true;
  [SerializeField] private float skatingWellDelaySeconds = 2f;

  private readonly Dictionary<Collider, Coroutine> skatingCoroutines = new Dictionary<Collider, Coroutine>();

  private void OnTriggerEnter(Collider other)
  {
    var animator = other.GetComponentInChildren<Animator>();
    var mover = other.GetComponentInParent<NGOPlayerMovement>();
    if (mover == null) return;
    mover.SetIce(isIce);
    if (animator != null)
    {
      animator.SetBool("IsSkating", true);
      StartSkatingWellTimer(other, animator);
    }
  }

  private void OnTriggerExit(Collider other)
  {
    var animator = other.GetComponentInChildren<Animator>();
    var mover = other.GetComponentInParent<NGOPlayerMovement>();
    if (mover == null) return;
    mover.SetIce(false);
    if (animator != null)
    {
      animator.SetBool("IsSkating", false);
      animator.SetBool("IsSkatingWell", false);
    }
    StopSkatingWellTimer(other);
  }

  private void StartSkatingWellTimer(Collider other, Animator animator)
  {
    if (skatingCoroutines.TryGetValue(other, out var running))
    {
      if (running != null) StopCoroutine(running);
      skatingCoroutines.Remove(other);
    }
    skatingCoroutines[other] = StartCoroutine(EnableSkatingWellAfterDelay(other, animator));
  }

  private void StopSkatingWellTimer(Collider other)
  {
    if (!skatingCoroutines.TryGetValue(other, out var running)) return;
    if (running != null) StopCoroutine(running);
    skatingCoroutines.Remove(other);
  }

  private System.Collections.IEnumerator EnableSkatingWellAfterDelay(Collider other, Animator animator)
  {
    if (skatingWellDelaySeconds > 0f)
      yield return new WaitForSeconds(skatingWellDelaySeconds);

    if (other == null || animator == null) yield break;
    animator.SetBool("IsSkatingWell", true);
  }
}
