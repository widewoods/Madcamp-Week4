using TMPro;
using Unity.Collections;
using Unity.Netcode;
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
    if (playerName != null)
      playerName.playerName.OnValueChanged += HandleNameChanged;
    if (NetworkManager.Singleton != null)
      NetworkManager.Singleton.OnClientConnectedCallback += UpdateName;
  }

  private void OnDisable()
  {
    if (playerName != null)
      playerName.playerName.OnValueChanged -= HandleNameChanged;
    if (NetworkManager.Singleton != null)
      NetworkManager.Singleton.OnClientConnectedCallback -= UpdateName;
  }

  private void UpdateName(ulong clientId)
  {
    if (nameText == null || playerName == null) return;
    nameText.text = playerName.Name;
  }

  private void HandleNameChanged(FixedString64Bytes previous, FixedString64Bytes current)
  {
    UpdateName(0);
  }
}
