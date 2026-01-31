using UnityEngine;

public class BowlingClearTrigger : MonoBehaviour
{

  void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Minigame"))
    {
      Destroy(other.gameObject);
    }
  }
}
