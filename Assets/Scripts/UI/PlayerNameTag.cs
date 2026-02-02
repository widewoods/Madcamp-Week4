using TMPro;
using UnityEngine;

public class PlayerNameTag : MonoBehaviour
{
  [SerializeField] private TMP_Text nameText;
  [SerializeField] private PlayerName playerName;

  private void Awake()
  {
    if (playerName == null) playerName = GetComponentInParent<PlayerName>();
  }

  private void OnEnable()
  {
    UpdateName();
  }

  private void Update()
  {
    if (playerName == null) return;
  }

  private void UpdateName()
  {
    if (nameText == null || playerName == null) return;
    nameText.text = playerName.Name;
  }
}
